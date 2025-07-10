using CargoWise.Types;
using Enterprise.Customs.Common;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	[TestedType(typeof(InvoiceApportionCharge))]
	sealed class InvoiceApportionChargeTest : EU.Business.Declaration.Testing.InvoiceApportionChargeTest
	{
		public void TestLookups()
		{
			var invoiceApportionCharge = Factory.New<InvoiceApportionCharge>();
			AssertType<InvoiceApportionChargeLookups>(invoiceApportionCharge.Lookups);
		}

		public void TestValidation()
		{
			var invoiceApportionCharge = Factory.New<InvoiceApportionCharge>();
			AssertType<InvoiceApportionChargeValidation>(invoiceApportionCharge.Validation);
		}

		public void TestInvoice()
		{
			var invoice = Factory.New<JobComInvoiceHeader>();
			var invoiceApportionCharge = invoice.GroupCharges.AddNew();
			AssertType<JobComInvoiceHeader>(invoiceApportionCharge.Invoice);
		}

		public void TestIsJ7_ExchangeRateIATA()
		{
			CombineAssertions(() =>
			{
				var invoiceApportionCharge = Factory.New<InvoiceApportionCharge>();
				invoiceApportionCharge.J7_ExchangeRateType = ChargeExchangeRateTypeList.Codes.IATARate;
				AssertEquals("Getter - true", true, invoiceApportionCharge.IsJ7_ExchangeRateIATA);
				invoiceApportionCharge.J7_ExchangeRateType = ChargeExchangeRateTypeList.Codes.FixedRate;
				AssertEquals("Getter - false", false, invoiceApportionCharge.IsJ7_ExchangeRateIATA);
			});
		}

		public void TestJ7_RX_NKCurrency_ResetExchangeRateData()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceApportionCharge = invoice.GroupCharges.AddNew();
			invoiceApportionCharge.J7_ChargeType = ImportChargeCodeList.Codes._010;
			invoiceApportionCharge.J7_ExchangeRateType = ChargeExchangeRateTypeList.Codes.IATARate;
			invoiceApportionCharge.J7_ExchangeRate = 1.0066m;
			CombineAssertions(() =>
			{
				invoiceApportionCharge.J7_RX_NKCurrency = "USD";
				AssertEquals("J7_ExchangeRateType wasn't reset", ChargeExchangeRateTypeList.Codes.IATARate, invoiceApportionCharge.J7_ExchangeRateType);
				AssertEquals("J7_ExchangeRate wasn't reset", true, invoiceApportionCharge.J7_ExchangeRate == 1.0066m);

				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
				invoiceApportionCharge.J7_RX_NKCurrency = "EUR";
				AssertEquals("Not IsImport, J7_ExchangeRateType was reset", ZString.Empty, invoiceApportionCharge.J7_ExchangeRateType);
				AssertEquals("Not IsImport, J7_ExchangeRate was reset", false, invoiceApportionCharge.J7_ExchangeRate == 1.0066m);

				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				invoiceApportionCharge.J7_ChargeType = ImportChargeCodeList.Codes._012;
				invoiceApportionCharge.J7_ExchangeRateType = ChargeExchangeRateTypeList.Codes.IATARate;
				invoiceApportionCharge.J7_ExchangeRate = 1.0066m;
				invoiceApportionCharge.J7_RX_NKCurrency = "USD";
				AssertEquals("Not SupportsIATA, J7_ExchangeRateType was reset", ZString.Empty, invoiceApportionCharge.J7_ExchangeRateType);
				AssertEquals("Not SupportsIATA, J7_ExchangeRate was reset", false, invoiceApportionCharge.J7_ExchangeRate == 1.0066m);

				invoiceApportionCharge.J7_ChargeType = ImportChargeCodeList.Codes._010;
				invoiceApportionCharge.J7_ExchangeRateType = ChargeExchangeRateTypeList.Codes.FixedRate;
				invoiceApportionCharge.J7_ExchangeRate = 1.0066m;
				invoiceApportionCharge.J7_RX_NKCurrency = "EUR";
				AssertEquals("Not IsJ7_ExchangeRateIATA, J7_ExchangeRateType was reset", ZString.Empty, invoiceApportionCharge.J7_ExchangeRateType);
				AssertEquals("Not IsJ7_ExchangeRateIATA, J7_ExchangeRate was reset", false, invoiceApportionCharge.J7_ExchangeRate == 1.0066m);
			});
		}

		public void TestDefaultValues()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			var invoice = declaration.Invoices.AddNew();

			var invoiceApportionCharge = invoice.GroupCharges.AddNew();

			AssertEquals(false, invoiceApportionCharge.J7_IsDutiable);
		}
	}
}
