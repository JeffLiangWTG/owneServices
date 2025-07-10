using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.Testing;

[TestedType(typeof(ImportEntryMessageSendingActionValidation))]
sealed class ImportEntryMessageSendingActionValidationTest : BEJobDeclarationMessageSendingObjectValidationTest<ImportEntryMessageSendingAction>
{
	public override void TestValidateTypeOfEntry_Valid()
	{
		var declaration = Factory.New<JobDeclaration>();
		var instruction = declaration.CustomsEntryInstructions.AddNew();
		var entry = declaration.CustomsEntryHeaders.AddNew();
		entry.CH_CEI_Instruction = instruction.PK;

		var testItem = new ImportEntryMessageSendingAction(entry);
		ValidationTestHelper.AssertErrorIfInvalidCode(testItem.TypeOfEntryInfo, "XXX", BEImportEntryTypeList.Codes.ImportDeclaration);
	}
}
