﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using RimWorld;
using Verse;
using HarmonyLib;
using System.Runtime;

namespace AssortedTweaks
{
    //[HarmonyPatch(typeof(ItemAvailability), "ThingsAvailableAnywhere")]
    public static class DeliverAsMuchAsPossible
    {
        //public bool ThingsAvailableAnywhere(ThingDefCountClass need, Pawn pawn)
        public static bool Prefix(ThingDefCountClass need, Pawn pawn, ref bool __result)
        {
            if (AssortedTweaksMod.instance.Settings.DeliverAsMuchAsYouCan)
            {
                List<Thing> list = pawn.Map.listerThings.ThingsOfDef(need.thingDef);
                __result = list.Any(t => !t.IsForbidden(pawn));
                return false;
            }
            return true;
        }
    }

    //This is just to change a break into a continue
    //Honestly is a vanilla bug
    //Would only deliver resource #2 once there's enough resource #1 
    //Though resource #1 doesn't care if there's enough #2
    //[HarmonyPatch(typeof(WorkGiver_ConstructDeliverResources), "ResourceDeliverJobFor")]
    public static class BreakToContinue_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            //File.WriteAllText("E:\\before.txt", string.Join("\n", instructions.Select(x => x.ToString())));
            Label continueLabel = il.DefineLabel();
            bool foundandreplaced = false;

            List<CodeInstruction> instList = instructions.ToList();

            for (int i = 0; i < instList.Count(); i++)
            {
                CodeInstruction inst = instList[i];
                if (!foundandreplaced
                    && inst.opcode == OpCodes.Leave
                    && instList[i - 1].opcode == OpCodes.Beq)
                {
                    inst.operand = instList[i - 1].operand;
                    foundandreplaced = true;
                }
                yield return inst;

            }
            if (!foundandreplaced)
            {
                Log.Error("Old transpiler for WorkGiver_ConstructDeliverResources.ResourceDeliverJobFor");
            }
            //File.WriteAllText("E:\\after.txt", string.Join("\n", instList.Select(x => x.ToString())));
        }
        //protected Job ResourceDeliverJobFor(Pawn pawn, IConstructible c, bool canRemoveExistingFloorUnderNearbyNeeders = true)
        /*
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            Label continueLabel = il.DefineLabel();
            bool setLabel = false;

            List<CodeInstruction> instList = instructions.ToList();
            for (int i = 0; i < instList.Count(); i++)
            {
                CodeInstruction inst = instList[i];

                //i=0;Br To for condition
                if (!setLabel
                && inst.opcode == OpCodes.Br
                && instList[i - 1].IsStloc()
                && instList[i - 2].LoadsConstant(0))
                {
                    Label forCheck = (Label)inst.operand;

                    for (int k = instList.Count() - 1; k >= 4; k--)
                    {
                        if (instList[k].labels.Contains(forCheck))
                        {
                            instList[k - 4].labels.Add(continueLabel);
                            setLabel = true;
                            break;
                        }
                    }
                }
                //break; preceded by thingDefCountClass = object.need;
                else if (inst.opcode == OpCodes.Br
                && instList[i - 1].IsStloc()
                && instList[i - 2].opcode == OpCodes.Ldfld)// operand == need, but inside a compilergenerated mess
                {
                    if (setLabel)
                    {
                        yield return new CodeInstruction(OpCodes.Br, continueLabel);
                        continue;
                    }
                }
                yield return inst;
            }
        }*/
    }
}
