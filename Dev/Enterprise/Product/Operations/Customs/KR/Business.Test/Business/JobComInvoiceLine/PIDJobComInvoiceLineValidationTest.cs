using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class PIDJobComInvoiceLineValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckJI_ProductTypeCode()
		{
			invoiceLine.JI_ProductTypeCode = PersonalItemCategoryCodeList.Codes._3;
			AssertNoMessageErrors(invoiceLine.JI_ProductTypeCodeInfo);
			invoiceLine.JI_ProductTypeCode = "5";
			AssertHasMessageErrorContaining(invoiceLine.JI_ProductTypeCodeInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.JI_ProductTypeCode = "";
			AssertHasMessageErrorContaining(invoiceLine.JI_ProductTypeCodeInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckJI_InvoiceUQ()
		{
			invoiceLine.JI_InvoiceUQ = "KG";
			AssertHasMessageErrorContaining(invoiceLine.JI_InvoiceUQInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.JI_InvoiceUQ = PersonalItemCodeList.Codes._001;
			AssertNoMessageErrorContaining(invoiceLine.JI_InvoiceUQInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckJI_NDescription()
		{
			invoiceLine.JI_NDescription = "Test Model";
			AssertNoMessageErrorContaining(invoiceLine.JI_NDescriptionInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.JI_NDescription = "";
			AssertHasMessageErrorContaining(invoiceLine.JI_NDescriptionInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckJI_CustomsQuantity()
		{
			invoiceLine.JI_CustomsQuantity = 10;
			AssertNoMessageErrors(invoiceLine.JI_CustomsQuantityInfo);
			invoiceLine.JI_CustomsQuantity = 0;
			AssertNoMessageErrors(invoiceLine.JI_CustomsQuantityInfo);
			invoiceLine.JI_CustomsQuantity = -10;
			AssertHasMessageErrorContaining(invoiceLine.JI_CustomsQuantityInfo, MandatoryValidation.ValueCannotBeNegative);
		}

		public void TestCheckJI_InvoiceQuantity()
		{
			invoiceLine.JI_InvoiceQuantity = 10;
			AssertNoMessageErrors(invoiceLine.JI_InvoiceQuantityInfo);
			invoiceLine.JI_InvoiceQuantity = 0;
			AssertNoMessageErrors(invoiceLine.JI_InvoiceQuantityInfo);
			invoiceLine.JI_InvoiceQuantity = -10;
			AssertHasMessageErrorContaining(invoiceLine.JI_InvoiceQuantityInfo, MandatoryValidation.ValueCannotBeNegative);
		}

		public void TestCheckJI_LinePrice()
		{
			invoiceLine.JI_LinePrice = 100.00m;
			AssertNoMessageErrors(invoiceLine.JI_LinePriceInfo);
			invoiceLine.JI_LinePrice = 0.00m;
			AssertNoMessageErrors(invoiceLine.JI_LinePriceInfo);
			invoiceLine.JI_LinePrice = -100.00m;
			AssertHasMessageErrorContaining(invoiceLine.JI_LinePriceInfo, MandatoryValidation.ValueCannotBeNegative);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = ElectronicDocumentTypeList.Codes._008;

			var invoice = declaration.Invoices[0];
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
		}
		JobComInvoiceLine invoiceLine;
	}
}
