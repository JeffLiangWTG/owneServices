using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.CN.Business.Testing
{
	class CustomsOfficeValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCY_Data()
		{
			var directCustomsErrorMessage = "The Customs Office should not be a directly competent Customs (ends with 00)";

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("CUSOF", "Customs Office");
			helper.CreateNewOrGetExistingCusCodeList("CN", "CUSOF", "2300", "2300", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList("CN", "CUSOF", "2301", "2301", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList("CN", "CUSOF", "2302", "2302", ZDateTime.MinSmallDateTimeValue, ZDateTime.Today.AddDays(-1));
			Factory.Save();

			var declartion = Factory.New<JobDeclaration>();
			var instruction = declartion.CustomsEntryInstructions.AddNew();
			var invoiceLine = declartion.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			var customsOffice = declartion.CustomsOffices.AddNew();
			customsOffice.CY_Code = CustomsOfficeTypeList.Codes.DES;

			Assert("Precondition: CIQRequires", !declartion.CIQRequires);
			customsOffice.CY_Data = ZString.Empty;
			AssertNoMessageErrorContaining(customsOffice.CY_DataInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasInvalidCodeMessageError();

			invoiceLine.JI_CIQTariff = "1";
			instruction.CEI_CIQRequires = true;
			Assert("Precondition: CIQRequires", declartion.CIQRequires);
			customsOffice.CY_Data = ZString.Empty;
			AssertHasMessageErrorContaining(customsOffice.CY_DataInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasInvalidCodeMessageError();

			void AssertHasInvalidCodeMessageError()
			{
				customsOffice.CY_Data = "2302";
				AssertHasMessageErrorContaining(customsOffice.CY_DataInfo, ListValidation.InvalidCodeMessageError);
				AssertNoMessageError(customsOffice.CY_DataInfo, directCustomsErrorMessage);
				customsOffice.CY_Data = "2300";
				AssertNoMessageErrorContaining(customsOffice.CY_DataInfo, ListValidation.InvalidCodeMessageError);
				AssertHasMessageError(customsOffice.CY_DataInfo, directCustomsErrorMessage);
				customsOffice.CY_Data = "2301";
				AssertNoMessageErrorContaining(customsOffice.CY_DataInfo, ListValidation.InvalidCodeMessageError);
				AssertNoMessageError(customsOffice.CY_DataInfo, directCustomsErrorMessage);
			}
		}
	}
}
