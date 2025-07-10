using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class B2JobComInvoiceHeaderValidationTest : CommonImportJobComInvoiceHeaderValidationTest
	{
		[TestDate(2020, 09, 16)]
		public override void TestCheckJZ_ValuationDateOverride()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_ValuationDateOverride = new ZDateTime(2016, 09, 15);
			AssertHasWarning(invoice.JZ_ValuationDateOverrideInfo, "The date '15-Sep-2016' is more than 4 years old.");

			invoice.JZ_ValuationDateOverride = new ZDateTime(2020, 09, 15);
			AssertNoWarning(invoice.JZ_ValuationDateOverrideInfo, "The date '15-Sep-2016' is more than 4 years old.");
		}

		public void TestCheckJZ_InvoiceNumber()
		{
			invoiceHeader.Validation.ValidateJZ_InvoiceNumber();
			AssertHasMessageErrorContaining(invoiceHeader.JZ_InvoiceNumberInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceHeader.JZ_InvoiceNumber = "123456";
			AssertNoMessageErrorContaining(invoiceHeader.JZ_InvoiceNumberInfo, MandatoryValidation.YouHaveNotEntered);
		}

		protected override ZString GetMessageType()
		{
			return JobMessageTypeList.Codes.B2Adjustments;
		}

		protected override System.Type GetTypeForTest()
		{
			return typeof(B2JobComInvoiceHeaderValidation);
		}
	}
}
