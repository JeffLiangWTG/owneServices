using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CN.Business.Testing
{
	class EnterpriseQualificationValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateCY_Code()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var epq1 = instruction.EnterpriseQualifications.AddNew();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			epq1.Validation.ValidateAll();
			AssertHasErrorContaining(epq1.CY_CodeInfo, "Please enter a Qualification Type.");
			epq1.CY_Code = "0";
			epq1.Validation.ValidateAll();
			AssertNoErrorContaining(epq1.CY_CodeInfo, "Please enter a Qualification Type.");
			AssertHasMessageErrorContaining(epq1.CY_CodeInfo, "The code you have selected is not in the list.");
			epq1.CY_Code = "101";
			epq1.Validation.ValidateAll();
			AssertNoMessageErrorContaining(epq1.CY_CodeInfo, "The code you have selected is not in the list.");
			epq1.CY_Code = "999";
			epq1.Validation.ValidateAll();
			AssertHasMessageErrorContaining(epq1.CY_CodeInfo, "The code you have selected is not in the list.");
		}

		public void TestValidateCY_Data()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var epq1 = instruction.EnterpriseQualifications.AddNew();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			epq1.Validation.ValidateAll();
			AssertHasMessageErrorContaining(epq1.CY_DataInfo, "You have not entered a Qualification Number.");
			epq1.CY_Data = "ABC";
			epq1.Validation.ValidateAll();
			AssertNoMessageErrorContaining(epq1.CY_DataInfo, "You have not entered a Qualification Number.");
			var epq2 = instruction.EnterpriseQualifications.AddNew();
			epq2.CY_Data = "ABC";
			epq1.Validation.ValidateAll();
			AssertNoMessageErrorContaining(epq2.CY_DataInfo, "The type and number are duplicated.");
			epq1.CY_Code = "0";
			epq2.CY_Code = "0";
			epq2.Validation.ValidateAll();
			AssertHasMessageErrorContaining(epq2.CY_DataInfo, "The type and number are duplicated.");
		}
	}
}
