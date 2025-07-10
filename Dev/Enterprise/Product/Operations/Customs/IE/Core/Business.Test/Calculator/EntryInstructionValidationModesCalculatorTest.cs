using System.Collections.Generic;
using Enterprise.Customs.Common.IE;
using Enterprise.Customs.EU.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	[TestedType(typeof(EntryInstructionValidationModesCalculator))]
	class EntryInstructionValidationModesCalculatorTest : EU.Business.Declaration.Testing.ValidationModesCalculatorAbstractTest<EntryInstructionValidationModesCalculator, CusEntryInstruction>
	{
		protected override IEnumerable<(CusEntryInstruction support, ValidationModes expectedResult, string description)> GetRecalculateValidationModesTestList()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.ReExport;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			yield return (instruction, EU.Business.Declaration.ValidationModes.None, "ValidationModes.None for empty Instruction.");

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;
			yield return (instruction, EU.Business.Declaration.ValidationModes.None, "ValidationModes.None for Instruction with empty EntryHeader.");

			entry.MovementReferenceNumberSetter("MRN001");
			yield return (instruction, EU.Business.Declaration.ValidationModes.None, "ValidationModes.None for Instruction with EntryHeader with MRN, but JE_MessageType not EXP.");

			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
			yield return (instruction, (ValidationModes)5, "ValidationModes.None, ValidationModes.ExportAmendment for Instruction with EntryHeader with MRN, JE_MessageType EXP.");

			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			yield return (instruction, (ValidationModes)5, "ValidationModes.None, ValidationModes.ExportAmendment for Instruction with EntryHeader with MRN, JE_MessageType EXP.");
		}
	}
}
