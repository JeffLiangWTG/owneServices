using CargoWise.Types;
using Enterprise.Customs.Common;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	[TestedType(typeof(InvoiceLineApportionCharge))]
	sealed class InvoiceLineApportionChargeTest : EU.Business.Declaration.Testing.InvoiceLineApportionChargeTest
	{
		public void TestLookups()
		{
			var invoiceLineApportionCharge = Factory.New<InvoiceLineApportionCharge>();
			AssertType<InvoiceLineApportionChargeLookups>(invoiceLineApportionCharge.Lookups);
		}

		public void TestValidation()
		{
			var invoiceLineApportionCharge = Factory.New<InvoiceLineApportionCharge>();
			AssertType<InvoiceLineApportionChargeValidation>(invoiceLineApportionCharge.Validation);
		}

		public void TestInvoice()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			var invoiceLineApportionCharge = invoiceLine.ApportionedCharges.AddNew();
			AssertType<JobComInvoiceLine>(invoiceLineApportionCharge.InvoiceLine);
		}

		public void TestIsJ7_ExchangeRateIATA()
		{
			CombineAssertions(() =>
			{
				var invoiceLineApportionCharge = Factory.New<InvoiceLineApportionCharge>();
				invoiceLineApportionCharge.IsJ7_ExchangeRateIATA = true;
				AssertEquals(Common.ChargeExchangeRateTypeList.Codes.IATARate, invoiceLineApportionCharge.J7_ExchangeRateType);
				invoiceLineApportionCharge.IsJ7_ExchangeRateIATA = false;
				AssertEquals(ZString.Empty, invoiceLineApportionCharge.J7_ExchangeRateType);
			});
		}

		public void TestJ7_RX_NKCurrency_ResetExchangeRateData()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var invoiceLineApportionCharge = invoiceLine.ApportionedCharges.AddNew();
			invoiceLineApportionCharge.J7_ChargeType = ImportChargeCodeList.Codes._010;
			invoiceLineApportionCharge.J7_ExchangeRateType = ChargeExchangeRateTypeList.Codes.IATARate;
			invoiceLineApportionCharge.J7_ExchangeRate = 1.0066m;
			CombineAssertions(() =>
			{
				invoiceLineApportionCharge.J7_RX_NKCurrency = "USD";
				AssertEquals("J7_ExchangeRateType wasn't reset", ChargeExchangeRateTypeList.Codes.IATARate, invoiceLineApportionCharge.J7_ExchangeRateType);
				AssertEquals("J7_ExchangeRate wasn't reset", true, invoiceLineApportionCharge.J7_ExchangeRate == 1.0066m);

				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
				invoiceLineApportionCharge.J7_RX_NKCurrency = "EUR";
				AssertEquals("Not IsImport, J7_ExchangeRateType was reset", ZString.Empty, invoiceLineApportionCharge.J7_ExchangeRateType);
				AssertEquals("Not IsImport, J7_ExchangeRate was reset", false, invoiceLineApportionCharge.J7_ExchangeRate == 1.0066m);

				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				invoiceLineApportionCharge.J7_ChargeType = ImportChargeCodeList.Codes._012;
				invoiceLineApportionCharge.J7_ExchangeRateType = ChargeExchangeRateTypeList.Codes.IATARate;
				invoiceLineApportionCharge.J7_ExchangeRate = 1.0066m;
				invoiceLineApportionCharge.J7_RX_NKCurrency = "USD";
				AssertEquals("Not SupportsIATA, J7_ExchangeRateType was reset", ZString.Empty, invoiceLineApportionCharge.J7_ExchangeRateType);
				AssertEquals("Not SupportsIATA, J7_ExchangeRate was reset", false, invoiceLineApportionCharge.J7_ExchangeRate == 1.0066m);

				invoiceLineApportionCharge.J7_ChargeType = ImportChargeCodeList.Codes._010;
				invoiceLineApportionCharge.J7_ExchangeRateType = ChargeExchangeRateTypeList.Codes.FixedRate;
				invoiceLineApportionCharge.J7_ExchangeRate = 1.0066m;
				invoiceLineApportionCharge.J7_RX_NKCurrency = "EUR";
				AssertEquals("Not IsJ7_ExchangeRateIATA, J7_ExchangeRateType was reset", ZString.Empty, invoiceLineApportionCharge.J7_ExchangeRateType);
				AssertEquals("Not IsJ7_ExchangeRateIATA, J7_ExchangeRate was reset", false, invoiceLineApportionCharge.J7_ExchangeRate == 1.0066m);
			});
		}

		public void TestDefaultValues()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			var invoiceLineApportionCharge = invoiceLine.ApportionedCharges.AddNew();

			CombineAssertions(() =>
			{
				AssertEquals("IsDutiable", false, invoiceLineApportionCharge.J7_IsDutiable);
				AssertEquals("IsGSTApplicable", false, invoiceLineApportionCharge.J7_IsGSTApplicable);
			});
		}
	}
}
