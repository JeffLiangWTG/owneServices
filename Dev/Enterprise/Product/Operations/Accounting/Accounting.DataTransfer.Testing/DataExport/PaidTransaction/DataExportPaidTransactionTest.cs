using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.DataTransfer.DataExport.Testing
{
	[TestedType(typeof(DataExportPaidTransaction))]
	internal class DataExportPaidTransactionTest : NonPersistentBusinessObjectTestCase
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			APInvoice invoice = Factory.NewWithValidTestData<APInvoice>();

			var payment = Factory.NewWithValidTestData<APPayment>();
			payment.AH_RX_NKTransactionCurrency = invoice.AH_RX_NKTransactionCurrency;
			var exportPayment = new DataExportPaymentHeader(Factory, payment);

			return new DataExportPaidTransaction(Factory, invoice, exportPayment);
		}

		public void TestConstructor()
		{
			var paymentInSameCurrency = Factory.NewWithValidTestData<APPayment>();
			paymentInSameCurrency.AH_ExchangeRate = 13800m;
			paymentInSameCurrency.AH_RX_NKTransactionCurrency = TestObjectCreator.IDR.Code;
			paymentInSameCurrency.AH_OSTotal = 15000000m;
			paymentInSameCurrency.AH_InvoiceAmount = 1086.96m;
			AssertConstructor(isEnableNewOSOutstandingAmountFeature: false,
				apOSAmount: 0m,
				expectedOSMatchedAmount: 1000149m,
				payment: paymentInSameCurrency,
				expectedLocalAmountInPaymentCurrency: 1000149m);

			var paymentInOtherCurrency = Factory.NewWithValidTestData<APPayment>();
			paymentInOtherCurrency.AH_RX_NKTransactionCurrency = TestObjectCreator.KRW.Code;
			paymentInOtherCurrency.AH_ExchangeRate = 13800m;
			paymentInOtherCurrency.AH_OSTotal = 15000000m;
			paymentInOtherCurrency.AH_InvoiceAmount = 1086.96m;
			AssertConstructor(isEnableNewOSOutstandingAmountFeature: false,
				apOSAmount: 0m,
				expectedOSMatchedAmount: 1000149m,
				payment: paymentInOtherCurrency,
				expectedLocalAmountInPaymentCurrency: 985731m);
		}

		public void TestConstructor_EnableNewOSOutstandingAmountFeature()
		{
			var paymentInSameCurrency = Factory.NewWithValidTestData<APPayment>();
			paymentInSameCurrency.AH_ExchangeRate = 13800m;
			paymentInSameCurrency.AH_RX_NKTransactionCurrency = TestObjectCreator.IDR.Code;
			paymentInSameCurrency.AH_OSTotal = 15000000m;
			paymentInSameCurrency.AH_InvoiceAmount = 1086.96m;
			AssertConstructor(isEnableNewOSOutstandingAmountFeature: true,
				apOSAmount: 1000000m,
				expectedOSMatchedAmount: 1000000m,
				payment: paymentInSameCurrency,
				expectedLocalAmountInPaymentCurrency: 1000000m);

			var paymentInOtherCurrency = Factory.NewWithValidTestData<APPayment>();
			paymentInOtherCurrency.AH_RX_NKTransactionCurrency = TestObjectCreator.KRW.Code;
			paymentInOtherCurrency.AH_ExchangeRate = 13800m;
			paymentInOtherCurrency.AH_OSTotal = 15000000m;
			paymentInOtherCurrency.AH_InvoiceAmount = 1086.96m;
			AssertConstructor(isEnableNewOSOutstandingAmountFeature: true,
				apOSAmount: 1000000m,
				expectedOSMatchedAmount: 1000000m,
				payment: paymentInOtherCurrency,
				expectedLocalAmountInPaymentCurrency: 985731m);
		}

		void AssertConstructor(bool isEnableNewOSOutstandingAmountFeature, decimal apOSAmount, decimal expectedOSMatchedAmount, APPayment payment, decimal expectedLocalAmountInPaymentCurrency)
		{
			AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, isEnableNewOSOutstandingAmountFeature);

			APInvoice invoice = Factory.NewWithValidTestData<APInvoice>();
			invoice.AH_RX_NKTransactionCurrency = TestObjectCreator.IDR.Code;
			invoice.AH_ExchangeRate = 14000m;
			invoice.AH_OSTotal = 15000000m;
			invoice.AH_InvoiceAmount = 1071.29m;
			invoice.AH_GSTAmount = 0m;
			invoice.AH_LocalTaxAmountOtherTaxes = 0m;
			invoice.AH_Desc = "TEST";
			invoice.AH_TransactionNum = "001";
			invoice.InvoiceRemittanceReference = "123456";

			var matchLink = Factory.NewWithValidTestData<TransactionMatchLink>();
			matchLink.AP_AH = invoice.PK;
			matchLink.AP_Amount = 71.43m;
			matchLink.AP_OSAmount = apOSAmount;
			invoice.SetMatchedAmount(matchLink, true);

			AssertEquals("Precondition - Invoice LocalMatchedAmount", 71.43m, invoice.LocalMatchedAmount);
			AssertEquals("Precondition - Invoice MatchedAmount", expectedOSMatchedAmount, invoice.MatchedAmount);

			var exportPayment = new DataExportPaymentHeader(Factory, payment);

			DataExportPaidTransaction export = new DataExportPaidTransaction(Factory, invoice, exportPayment);
			AssertEquals("AH_Desc", invoice.AH_Desc, export.AH_Desc);
			AssertEquals("AH_DueDate", invoice.AH_DueDate, export.AH_DueDate);
			AssertEquals("AH_ExchangeRate", invoice.AH_ExchangeRate, export.AH_ExchangeRate);
			AssertEquals("AH_InvoiceDate", invoice.AH_InvoiceDate, export.AH_InvoiceDate);
			AssertEquals("AH_OH", invoice.AH_OH, export.AH_OH);
			AssertEquals("AH_OSExTaxAmount", invoice.AH_OSExTaxAmount, export.AH_OSExTaxAmount);
			AssertEquals("AH_OSTaxAmount", invoice.AH_OSTaxAmount, export.AH_OSTaxAmount);
			AssertEquals("AH_OSTotalAmount", invoice.AH_OSTotalAmount, export.AH_OSTotalAmount);
			AssertEquals("AH_PostDate", invoice.AH_PostDate, export.AH_PostDate);
			AssertEquals("AH_RX_NKTransactionCurrency", invoice.AH_RX_NKTransactionCurrency, export.AH_RX_NKTransactionCurrency);
			AssertEquals("AH_TransactionNum", invoice.AH_TransactionNum, export.AH_TransactionNum);
			AssertEquals("LocalMatchedAmount", invoice.LocalMatchedAmount, export.LocalMatchedAmount);
			AssertEquals("OSMatchedAmount", invoice.MatchedAmount, export.OSMatchedAmount);
			AssertEquals("LocalAmountInPaymentCurrency", expectedLocalAmountInPaymentCurrency, export.LocalAmountInPaymentCurrency);
			AssertEquals("InvoiceRemittanceReference", invoice.InvoiceRemittanceReference, export.InvoiceRemittanceReference);
			AssertEquals("TransactionHeader", invoice, export.TransactionHeader);
		}

		public void TestCurrencyCaching()
		{
			APInvoice invoice = Factory.NewWithValidTestData<APInvoice>();
			invoice.AH_Desc = "TEST";
			invoice.AH_TransactionNum = "001";
			invoice.AH_RX_NKTransactionCurrency = "USD";

			var payment = Factory.NewWithValidTestData<APPayment>();
			payment.AH_RX_NKTransactionCurrency = invoice.AH_RX_NKTransactionCurrency;
			var exportPayment = new DataExportPaymentHeader(Factory, payment);

			DataExportPaidTransaction export = new DataExportPaidTransaction(Factory, invoice, exportPayment);
			AssertNotNull(export.Currency);

			export.Currency.Delete();
			AssertNull(export.Currency);

			export.AH_RX_NKTransactionCurrency = "AUD";
			AssertNotNull(export.Currency);
			AssertEquals("New Currecny Code", "AUD", export.Currency.RX_Code);
		}

		#endregion

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}
