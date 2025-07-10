using CargoWise.Types;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	class GuaranteeForDeclarationValidationTest : CommonGuaranteeValidationTest
	{
		public void TestCheckEntryInstructionID()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction1.CEI_Style = "A";

			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction2.CEI_Style = "B";

			var guarantee = declaration.Guarantees.AddNew();
			guarantee.EntryInstructionID = ZGuid.Invalid;
			AssertHasError(guarantee.EntryInstructionIDInfo, "Enter a valid selection.");

			var guarantee2 = declaration.Guarantees.AddNew();
			guarantee2.EntryInstructionID = entryInstruction1.PK;
			AssertNoError(guarantee2.EntryInstructionIDInfo, "Enter a valid selection.");
		}
	}
}
