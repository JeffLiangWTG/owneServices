using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.Testing;

[TestedType(typeof(ExportEntryMessageSendingActionValidation))]
sealed class ExportEntryMessageSendingActionValidationTest : BEJobDeclarationMessageSendingObjectValidationTest<ExportEntryMessageSendingAction>
{
	public override void TestValidateTypeOfEntry_Valid()
	{
		var declaration = Factory.New<JobDeclaration>();
		var instruction = declaration.CustomsEntryInstructions.AddNew();
		var entry = declaration.CustomsEntryHeaders.AddNew();
		entry.CH_CEI_Instruction = instruction.PK;

		var testItem = new ExportEntryMessageSendingAction(entry);
		ValidationTestHelper.AssertErrorIfInvalidCode(testItem.TypeOfEntryInfo, "XXX", BEExportEntryTypeList.Codes.ExportAmendment);
	}
}
