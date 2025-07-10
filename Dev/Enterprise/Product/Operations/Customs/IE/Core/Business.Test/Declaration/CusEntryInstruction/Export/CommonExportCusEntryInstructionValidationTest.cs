namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	abstract class CommonExportCusEntryInstructionValidationTest<T> : CusEntryInstructionValidationAbstractTest<T> where T : CommonExportCusEntryInstructionValidation
	{
		public void TestCheckCEI_SubStyle_MustHaveSDECusAuthorizationUsage()
		{
			var errorMessage = "Please enter at least a Authorization of code 'SDE'";

			instruction.CEI_SubStyle = "C";
			instruction.CusAuthorizationUsages.RemoveAndDeleteAll();
			instruction.Validation.ValidateCEI_SubStyle();
			AssertHasMessageError("SDE Authorization not exist", instruction.CEI_SubStyleInfo, errorMessage);

			instruction.CEI_SubStyle = "F";
			instruction.CusAuthorizationUsages.RemoveAndDeleteAll();
			instruction.Validation.ValidateCEI_SubStyle();
			AssertHasMessageError("SDE Authorization not exist", instruction.CEI_SubStyleInfo, errorMessage);

			instruction.CEI_SubStyle = "B";
			var cusAuthorizationUsage = instruction.CusAuthorizationUsages.AddNew();
			cusAuthorizationUsage.AGC_Code = Customs.Business.CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration;
			instruction.Validation.ValidateCEI_SubStyle();
			AssertNoMessageError("CEI_SubStyle not in C,F", instruction.CEI_SubStyleInfo, errorMessage);

			instruction.CEI_SubStyle = "C";
			AssertNoMessageError("SDE Authorization exist", instruction.CEI_SubStyleInfo, errorMessage);

			instruction.CusAuthorizationUsages.RemoveAndDeleteAll();
		}

		public void TestCheckCEI_OA_Warehouse2()
		{
			instruction.CEI_Style = ExportDeclarationTypeList.Codes.B3;
			Customs.Business.Testing.ValidationTestHelper.AssertYouHaveNotEnteredMessageError(instruction.CEI_OA_Warehouse2Info, "Warehouse To is mandatory when Declaration Type is B3.");
		}
	}
}
