using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	class JobComInvoiceHeaderValidationTest : Customs.Business.Testing.JobComInvoiceHeaderValidationTest
	{
		[TestDate(2020, 09, 16)]
		public virtual void TestCheckJZ_ValuationDateOverride()
		{
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_ValuationDateOverride = new ZDateTime(2019, 09, 15);
			AssertHasWarning(invoice.JZ_ValuationDateOverrideInfo, "The date '15-Sep-2019' is more than 1 year old.");

			invoice.JZ_ValuationDateOverride = new ZDateTime(2020, 09, 15);
			AssertNoWarning(invoice.JZ_ValuationDateOverrideInfo, "The date '15-Sep-2019' is more than 1 year old.");
		}

		public override void TestValidateJZ_InvoiceNumber()
		{
			base.TestValidateJZ_InvoiceNumber();
			var parent = Factory.New<JobComInvoiceHeader>();
			parent.JZ_InvoiceNumber = "123ABC";
			AssertNoWarning(parent.JZ_InvoiceNumberInfo, "The Invoice Number has special characters which cannot be sent in the EDI message. These will be stripped when the messages is sent to Customs.");

			parent.JZ_InvoiceNumber = "123ABC– ";
			AssertHasWarning(parent.JZ_InvoiceNumberInfo, "The Invoice Number has special characters which cannot be sent in the EDI message. These will be stripped when the messages is sent to Customs.");
		}

		public void TestMessageValidation()
		{
			JobComInvoiceHeader parent = Factory.New<JobComInvoiceHeader>();
			AssertEquals(typeof(ExternalMessageValidation), parent.Validation.MessageValidation.GetType());
		}

		public void TestMissingCustomsExchangeRate()
		{
			DeclarationTestHelper helper = new DeclarationTestHelper(Factory, false);
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_ExportDate = new ZDateTime(1980, 1, 2);
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = helper.USD.RX_Code;
			AssertHasMessageErrorContaining(invoice.JZ_RX_NKInvoice_CurrencyInfo, ExternalMessageValidation.AdviceHowToFixNoValidExchangeRates);

			RefExchangeRate rate = helper.USD.ExchangeRates.AddNew();
			rate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;
			rate.RE_StartDate = new ZDateTime(1980, 1, 2);
			rate.RE_ExpiryDate = new ZDateTime(1980, 1, 2);
			rate.RE_SellRate = 0.72m;
			invoice.JZ_RX_NKInvoice_Currency = helper.USD.RX_Code;
			AssertNoMessageErrorContaining(invoice.JZ_RX_NKInvoice_CurrencyInfo, ExternalMessageValidation.AdviceHowToFixNoValidExchangeRates);
		}

		public void TestAddWarningOrMessageErrorToJZ_InvoiceCurrExRateInfoWhenExchangeRateStaleReturnsMessageErrorForCA()
		{
			var jjjCurrency = Factory.NewWithValidTestData<RefCurrency>();
			jjjCurrency.RX_Code = "JJJ";
			jjjCurrency.RX_Desc = "Dummy Currency";

			var exchRate1 = jjjCurrency.ExchangeRates.AddNew();
			exchRate1.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;
			exchRate1.RE_RX_NKExCurrency = jjjCurrency.RX_Code;
			exchRate1.RE_StartDate = ZDateTime.Today.AddDays(-1);
			exchRate1.RE_ExpiryDate = ZDateTime.Today.AddDays(-1);
			exchRate1.RE_SellRate = 1.5555m;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ExportDate = ZDateTime.Today.AddDays(-1);
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "JJJ";

			AssertEquals("JZ_InvoiceCurrExRate", 1.5555m, invoice.JZ_InvoiceCurrExRate);
			AssertNoMessageError(invoice.JZ_InvoiceCurrExRateInfo, "This exchange rate was set when the currency was set. But the current rate for the valuation date is 1.611100. This happens when latest exchange rates are imported after a currency is selected on the invoice. Please perform apportionment by clicking Brokerage > Perform Apportionment. This will update this exchange rate. Please also check if customs entries have correct figures.");

			var exchRate2 = jjjCurrency.ExchangeRates.AddNew();
			exchRate2.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;
			exchRate2.RE_RX_NKExCurrency = jjjCurrency.RX_Code;
			exchRate2.RE_StartDate = ZDateTime.Today;
			exchRate2.RE_ExpiryDate = ZDateTime.Today;
			exchRate2.RE_SellRate = 1.6111m;

			invoice.JZ_ValuationDateOverride = ZDateTime.Today;
			invoice.JZ_InvoiceCurrExRate = 1.5555m;
			invoice.Validation.ValidateJZ_InvoiceCurrExRate();
			AssertHasMessageError(invoice.JZ_InvoiceCurrExRateInfo, "This exchange rate was set when the currency was set. But the current rate for the valuation date is 1.611100. This happens when latest exchange rates are imported after a currency is selected on the invoice. Please perform apportionment by clicking Brokerage > Perform Apportionment. This will update this exchange rate. Please also check if customs entries have correct figures.");

			invoice.JZ_RX_NKInvoice_Currency = "";
			invoice.JZ_RX_NKInvoice_Currency = "JJJ";
			AssertEquals("JZ_InvoiceCurrExRate", 1.6111m, invoice.JZ_InvoiceCurrExRate);
			AssertNoMessageError("Exchange rate should have been updated to todays rate", invoice.JZ_InvoiceCurrExRateInfo, "This exchange rate was set when the currency was set. But the current rate for the valuation date is 1.611100. This happens when latest exchange rates are imported after a currency is selected on the invoice. Please perform apportionment by clicking Brokerage > Perform Apportionment. This will update this exchange rate. Please also check if customs entries have correct figures.");
		}

		protected override Customs.Business.BaseJobComInvoiceHeader GetInvoiceHeader()
		{
			Customs.Business.BaseJobComInvoiceHeader invoice = base.GetInvoiceHeader();
			invoice.JobDeclaration.JE_MessageType = "XXX";
			AssertEquals(false, invoice.IsExport);
			return invoice;
		}

		public override void TestValidateBalance()
		{
			base.TestValidateBalance();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 100;
			invoice.JZ_InvoiceAmount = 1000m;
			declaration.ResumeApportionment();
			AssertHasMessageErrorContaining(invoice.JZ_Calc_BalanceInfo, "total of all invoice lines");
			invoice.JZ_InvoiceAmount = 100m;
			declaration.ResumeApportionment();
			AssertNoMessageErrorContaining(invoice.JZ_Calc_BalanceInfo, "total of all invoice lines");
			invoice.JZ_InvoiceAmount = 0m;
			declaration.ResumeApportionment();
			AssertHasMessageErrorContaining(invoice.JZ_Calc_BalanceInfo, "total of all invoice lines");
		}

		protected override Type GetTypeForTest()
		{
			return typeof(JobComInvoiceHeaderValidation);
		}

		protected new JobDeclaration declaration
		{
			get { return (JobDeclaration)base.declaration; }
		}
	}
}
