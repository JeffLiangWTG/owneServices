using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Moq.Protected;
using NUnit.Framework;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	[TestedType(typeof(EntryInstructionValidationModesCalculator))]
	class EntryInstructionValidationModesCalculatorTest : ValidationModesCalculatorAbstractTest<EntryInstructionValidationModesCalculator, CusEntryInstruction>
	{
		protected override IEnumerable<(CusEntryInstruction support, ValidationModes expectedResult, string description)> GetRecalculateValidationModesTestList()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			var instruction = Factory.New<CusEntryInstruction>();
			instruction.CEI_JE = declaration.PK;
			yield return (instruction, ValidationModes.None, "ValidationModes.None for empty Instruction.");
		}
	}

	[TestedType(typeof(EntryInstructionValidationModesCalculatorForTesting))]
	class EntryInstructionValidationModesCalculatorForTestingTest : ValidationModesCalculatorAbstractTest<EntryInstructionValidationModesCalculatorForTesting, CusEntryInstruction>
	{
		protected override IEnumerable<(CusEntryInstruction support, ValidationModes expectedResult, string description)> GetRecalculateValidationModesTestList()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			var instructionMock = Factory.NewMoq<CusEntryInstruction>();
			var instruction = instructionMock.Object;
			instructionMock.Protected().Setup<EntryInstructionValidationModesCalculator>("CreateNewValidationModesCalculator").Returns(new EntryInstructionValidationModesCalculatorForTesting(instruction));
			instruction.CEI_JE = declaration.PK;
			yield return (instruction, ValidationModes.None, "ValidationModes.None for empty Instruction.");

			instruction.CEI_SubStyle = "ADM";
			yield return (instruction, ValidationModes.None, "ValidationModes.None for Instruction with SubStyle, but JE_MessageType not EXP.");

			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
			yield return (instruction, (ValidationModes)5, "ValidationModes.None | ValidationModes.Amendment for Instruction with SubStyle, JE_MessageType EXP.");
		}
	}

	[CodeAlive("Test Object")]
	public class EntryInstructionValidationModesCalculatorForTesting : EntryInstructionValidationModesCalculator
	{
		public EntryInstructionValidationModesCalculatorForTesting(CusEntryInstruction instruction)
			: base(instruction)
		{ }

		protected override ValidationModes RecalculateValidationModesCore()
		{
			var result = ValidationModes.None;

			switch (supporter.JobDeclaration?.JE_MessageType.ToUpperInvariant() ?? ZString.Empty)
			{
				case EUJobMessageTypeList.Codes.Export:
					if (supporter.CEI_SubStyle == "ADM")
					{
						result |= ValidationModes.Amendment;
					}
					break;
			}

			return result;
		}

		protected new CusEntryInstruction supporter => base.supporter;
	}
}
