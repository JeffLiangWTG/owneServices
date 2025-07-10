using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.DE.Business.Declaration;

namespace Enterprise.Customs.DE.Business.Testing
{
	class ContentInformationTypeValidationTest : CusCodeDataValidationTest
	{
		public new void TestCheckCY_Code()
		{
			contentInformationType.CY_Code = "";
			AssertHasMessageErrorContaining(contentInformationType.CY_CodeInfo, "not entered");

			contentInformationType.CY_Code = "01";
			AssertNoMessageErrorContaining(contentInformationType.CY_CodeInfo, "not entered");
			AssertNoMessageError(contentInformationType.CY_CodeInfo, "The code you have selected is not in the list.");

			contentInformationType.CY_Code = "03";
			AssertHasMessageError(contentInformationType.CY_CodeInfo, "The code you have selected is not in the list.");

			contentInformationType.CY_Code = "01";

			var ctiCusCode1 = invoiceLine.ContentInformationTypes.AddNew();
			ctiCusCode1.CY_Code = "01";
			AssertHasMessageError(ctiCusCode1.CY_CodeInfo, "Content Information Type should not be duplicated.");

			contentInformationType.CY_Code = "";

			var ctiCusCode2 = invoiceLine.ContentInformationTypes.AddNew();
			ctiCusCode2.CY_Code = "";
			AssertNoMessageError(ctiCusCode2.CY_CodeInfo, "Content Information Type should not be duplicated.");

			contentInformationType.CY_Code = "03";

			var ctiCusCode3 = invoiceLine.ContentInformationTypes.AddNew();
			ctiCusCode3.CY_Code = "03";
			AssertHasMessageError(ctiCusCode3.CY_CodeInfo, "Content Information Type should not be duplicated.");
		}

		public void TestCheckCY_Data()
		{
			contentInformationType.CY_Code = "";
			contentInformationType.CY_Data = "0.00";
			AssertNoMessageError(contentInformationType.CY_DataInfo, "The value should be between 0.01 and 100.00.");

			contentInformationType.CY_Code = "01";
			contentInformationType.CY_Data = "0.00";
			AssertHasMessageError(contentInformationType.CY_DataInfo, "The value should be between 0.01 and 100.00.");

			contentInformationType.CY_Data = "101.00";
			AssertHasMessageError(contentInformationType.CY_DataInfo, "The value should be between 0.01 and 100.00.");

			contentInformationType.CY_Data = "100.00";
			AssertNoMessageError(contentInformationType.CY_DataInfo, "The value should be between 0.01 and 100.00.");
		}

		protected override void SetUp()
		{
			base.SetUp();
			contentInformationType = Factory.CreateContentInformationType();
			invoiceLine = (JobComInvoiceLine)contentInformationType.Parent;
		}

		JobComInvoiceLine invoiceLine;
		ContentInformationType contentInformationType;
	}
}
