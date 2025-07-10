using System.Collections.Generic;
using System.Collections.Immutable;
using CargoWise.Types;

namespace Enterprise.Customs.CN.Business
{
	interface IEntryRequiresCIQStrategy
	{
		ZString Validate(CusEntryInstruction instruction);
	}

	internal static class EntryRequiresCIQStrategy
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule", Justification = "It is readonly")]
		static readonly ImmutableArray<IEntryRequiresCIQStrategy> exitingStrategies = new List<IEntryRequiresCIQStrategy> { new EntryRequiresCIQStrategyRequirementB() }.ToImmutableArray();

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule", Justification = "It is readonly")]
		static readonly ImmutableArray<IEntryRequiresCIQStrategy> enteringStrategies = new List<IEntryRequiresCIQStrategy> {
			new EntryRequiresCIQStrategyRequirementA()
			, new EntryRequiresCIQStrategyProcedureCode4561()
			, new EntryRequiresCIQStrategy3612MED()
			, new EntryRequiresCIQStrategyCFCS()
			, new EntryRequiresCIQStrategyPackageType()
			, new EntryRequiresCIQStrategyCargoAttributes()
			, new EntryRequiresCIQStrategyLCL()
			, new EntryRequiresCIQStrategyDangerousChemical()
		}.ToImmutableArray();

		public static EntryRequiresCIQStrategyValidationResult ValidateEntry(CusEntryInstruction instruction)
		{
			EntryRequiresCIQStrategyValidationResult result = new EntryRequiresCIQStrategyValidationResult();
			if (instruction.WillGenerateExitingEntry)
			{
				result = ValidateEntry(instruction, exitingStrategies);
			}
			else if (instruction.WillGenerateEnteringEntry)
			{
				result = ValidateEntry(instruction, enteringStrategies);
			}
			return result;
		}

		static EntryRequiresCIQStrategyValidationResult ValidateEntry(CusEntryInstruction instruction, ImmutableArray<IEntryRequiresCIQStrategy> strategies)
		{
			EntryRequiresCIQStrategyValidationResult result = new EntryRequiresCIQStrategyValidationResult();
			foreach (var strategy in strategies)
			{
				var message = strategy.Validate(instruction);
				if (!message.IsEmpty)
				{
					result.ErrorCount++;
					result.Messages.Add(message);
				}
			}
			return result;
		}
	}
}
