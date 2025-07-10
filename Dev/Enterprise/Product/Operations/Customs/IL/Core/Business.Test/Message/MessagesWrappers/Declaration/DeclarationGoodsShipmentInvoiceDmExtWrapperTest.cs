using CargoWise.Customs.IL.MessageDefinitions.DEC.IMP;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class DeclarationGoodsShipmentInvoiceDmExtWrapperTest : Customs.Business.Testing.DataProviderTestCase<IDeclarationGoodsShipmentInvoiceDmExtensions>
	{
		public void TestNewOrNull()
		{
			AssertNull("When invoiceHeader is null", DeclarationGoodsShipmentInvoiceDmExtWrapper.NewOrNull(null));
			AssertNotNull("When invoiceHeader is null", DeclarationGoodsShipmentInvoiceDmExtWrapper.NewOrNull(invoiceHeader));
		}

		public void TestInvoiceAmount()
		{
			invoiceHeader.JZ_InvoiceAmount = 0.1999m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
			AssertNotNull("InvoiceAmount is not null When value is not zero and Currency is not empty", Provider.InvoiceAmount);
			AssertEquals("InvoiceAmount.Value should be set as expected", 0.20m, Provider.InvoiceAmount.Value);
			AssertEquals("InvoiceAmount.CurrencyID should be set as expected", Iso3AlphaCurrencyCodeContentType.Usd, Provider.InvoiceAmount.CurrencyID);

			invoiceHeader.JZ_InvoiceAmount = 0.2007m;
			AssertEquals("InvoiceAmount.Value should be set as expected", 0.20m, Provider.InvoiceAmount.Value);

			invoiceHeader.JZ_RX_NKInvoice_Currency = null;
			AssertNull("InvoiceAmount is null When Currency is empty", Provider.InvoiceAmount);
		}

		public void TestActualPayedAmount()
		{
			AssertNull(Provider.ActualPayedAmount);
		}

		public void TestIsPrefarenceDocumentInd()
		{
			Assert("IsPrefarenceDocumentInd should be set to false", !Provider.IsPrefarenceDocumentInd.Value);
			invoiceHeader.JZ_PreferenceDocumentType = "XYZ";
			Assert("IsPrefarenceDocumentInd should be set to true", Provider.IsPrefarenceDocumentInd.Value);
		}

		public void TestPaymentType()
		{
			invoiceHeader.JZ_PaymentTerms = "FOB";
			AssertEquals("PaymentType should be set as expected", "FOB", Provider.PaymentType.Value);
		}

		public void TestPrefarenceDocumentType()
		{
			invoiceHeader.JZ_PreferenceDocumentType = "XYZ";
			AssertEquals("PrefarenceDocumentType should be set as expected", "XYZ", Provider.PrefarenceDocumentType.Value);
		}

		public void TestRateNumericValue()
		{
			AssertNull(Provider.RateNumericValue);
		}

		protected override IDeclarationGoodsShipmentInvoiceDmExtensions GetProvider()
			=> DeclarationGoodsShipmentInvoiceDmExtWrapper.NewOrNull(invoiceHeader);

		protected override void SetUp()
		{
			base.SetUp();
			invoiceHeader = Factory.New<JobComInvoiceHeader>();
		}

		JobComInvoiceHeader invoiceHeader;
	}
}
