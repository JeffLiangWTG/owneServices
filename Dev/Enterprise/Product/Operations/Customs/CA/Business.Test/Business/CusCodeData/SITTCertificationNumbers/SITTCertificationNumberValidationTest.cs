using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class SITTCertificationNumberValidationTest : CusCodeDataValidationTest
	{
		public void TestCheckCY_Data()
		{
			string messageError = MandatoryValidation.YouHaveNotEntered + " a Number.";
			regNumber.Validation.ValidateCY_Data();
			AssertHasMessageError(regNumber.CY_DataInfo, messageError);

			regNumber.CY_Data = "12345";
			AssertNoMessageError(regNumber.CY_DataInfo, messageError);
		}

		public void TestDuplicates()
		{
			var delcaration = Factory.New<JobDeclaration>();
			delcaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = delcaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var number1 = invoiceLine.SITTCertificationNumbers.AddNew();
			number1.CY_Data = "1";
			var number2 = invoiceLine.SITTCertificationNumbers.AddNew();
			number2.CY_Data = "1";
			AssertHasMessageErrorContaining(number2.CY_DataInfo, "This number is duplicated.");
			number2.CY_Data = "2";
			AssertNoMessageErrorContaining(number2.CY_DataInfo, "This number is duplicated.");
		}

		protected override void SetUp()
		{
			regNumber = Factory.New<SITTCertificationNumber>();
			base.SetUp();
		}

		SITTCertificationNumber regNumber;
	}
}
