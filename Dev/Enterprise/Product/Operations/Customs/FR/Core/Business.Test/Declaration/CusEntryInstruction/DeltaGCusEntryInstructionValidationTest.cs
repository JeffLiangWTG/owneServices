using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	class DeltaGCusEntryInstructionValidationTest : CargoWise.EntityFramework.Testing.BusinessObjectValidationTestCase
	{
		public void TestCheckCEI_SubStyle()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = "F";
			AssertNoMessageErrorContaining("declaration is delta G && CEI_substyle is not empty => no error", entryInstruction.CEI_SubStyleInfo, MandatoryValidation.YouHaveNotEntered);

			entryInstruction.CEI_SubStyle = ZString.Empty;
			AssertHasMessageErrorContaining("declaration is delta G && CEI_substyle is empty => error", entryInstruction.CEI_SubStyleInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			entryInstruction.CEI_SubStyle = "F";
			AssertNoMessageErrorContaining("declaration is delta IE && CEI_substyle is not empty => no error", entryInstruction.CEI_SubStyleInfo, MandatoryValidation.YouHaveNotEntered);

			entryInstruction.CEI_SubStyle = ZString.Empty;
			AssertNoMessageErrorContaining("declaration is delta IE && CEI_substyle is empty => no error", entryInstruction.CEI_SubStyleInfo, MandatoryValidation.YouHaveNotEntered);
		}
	}
}
