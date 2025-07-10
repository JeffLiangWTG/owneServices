using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.PaymentApproval.Testing
{
	[TestedType(typeof(PaymentApprovalItem))]
	public class PaymentApprovalItemTest : AccPaymentApprovalItemTest
	{
		public void TestZDecimalsHaveCorrectDecimalPlacesPaymentApprovalItem()
		{
			PaymentApprovalItem newApprovalItem = Factory.New<PaymentApprovalItem>();
			TransactionHeader newTransactionHeader = Factory.New<APInvoice>();
			newApprovalItem.A2_AH = newTransactionHeader.PK;
			AssertNotNull("Company should not be null", newTransactionHeader.Company);

			var localList = new List<string>
				{
					nameof(newApprovalItem.A2_PaymentThisRun)
				};

			var osList = new List<string>
				{
					nameof(newApprovalItem.OSAmountPaidThisRun)
				};

			var tester = new DecimalPlacesAttributeTester(newApprovalItem, newTransactionHeader.Company);
			tester.CheckLocalCurrency(localList, nameof(newApprovalItem.LocalDecimals));
			tester.CheckNonLocalCurrency(osList, nameof(newApprovalItem.OSDecimals), nameof(newTransactionHeader.AH_RX_NKTransactionCurrency), newTransactionHeader);
		}

		public void TestHeader()
		{
			TransactionHeader newTransactionHeader = Factory.New<APInvoice>();
			PaymentApprovalItem newApprovalItem = Factory.New<PaymentApprovalItem>();

			newApprovalItem.A2_AH = ZGuid.Empty;
			AssertNull("Header should be null", newApprovalItem.Approval);

			newApprovalItem.A2_AH = newTransactionHeader.PK;
			AssertNotNull("Header should not be null", newApprovalItem.Header);
			AssertNotNull("Header is of type TransactionHeader", newApprovalItem.Header);
		}

		public void TestApproval()
		{
			PaymentApprovalBase newApproval = Factory.New<APPaymentApprovalWithAuthorisation>();
			PaymentApprovalItem newApprovalItem = Factory.New<PaymentApprovalItem>();

			newApprovalItem.A2_AV = ZGuid.Empty;
			AssertNull("Approval should be null", newApprovalItem.Approval);

			newApprovalItem.A2_AV = newApproval.PK;
			AssertNotNull("Approval should not be null", newApprovalItem.Approval);
			Assert("Approval Type should be assignable from PaymentApprovalBase",
				typeof(PaymentApprovalBase).IsAssignableFrom(newApprovalItem.Approval.GetType()));
		}

		public void TestOSAmountPaidThisRun()
		{
			ZDecimal localPayAmount = 100M;
			ZDecimal exchangeRate = 0.5M;

			TransactionHeader newTransactionHeader = Factory.New<APInvoice>();
			PaymentApprovalItem newApprovalItem = Factory.New<PaymentApprovalItem>();

			RefCurrency newCurrency = Factory.NewWithValidTestData<RefCurrency>();
			RefExchangeRate todaysExchangeRate = newCurrency.ExchangeRates.AddNew();

			todaysExchangeRate.RE_RX_NKExCurrency = newCurrency.RX_Code;
			todaysExchangeRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.BuyRate;
			todaysExchangeRate.RE_SellRate = 0.8M;
			todaysExchangeRate.RE_GC = GlbCompany.CurrentCompany.PK;
			todaysExchangeRate.RE_StartDate = ZDateTime.Now.AddDays(-10);
			todaysExchangeRate.RE_ExpiryDate = ZDateTime.Now.AddDays(10);

			newTransactionHeader.AH_RX_NKTransactionCurrency = newCurrency.RX_Code;
			newTransactionHeader.AH_ExchangeRate = exchangeRate;

			newApprovalItem.A2_AH = ZGuid.Empty;
			AssertEquals("OS Amount Paid This Run", 0M, newApprovalItem.OSAmountPaidThisRun);

			newApprovalItem.A2_AH = newTransactionHeader.PK;
			newApprovalItem.A2_PaymentThisRun = localPayAmount;

			ZDecimal expected = Env.CurrentCompany.ExchangeRate.LocalToForeign(localPayAmount, exchangeRate, newCurrency.RX_Code);
			AssertEquals("OS Amount Paid This Run", expected, newApprovalItem.OSAmountPaidThisRun);
		}

		public void TestOSAmountPaidThisRunRecalulatesProperlyFromLocalAmount()
		{
			ZDecimal oSAmount = -140.75;
			ZDecimal localAmount = -186.68;
			ZDecimal exchangeRate = 0.7540M;

			TransactionHeader newTransactionHeader = Factory.New<APInvoice>();
			PaymentApprovalItem newApprovalItem = Factory.New<PaymentApprovalItem>();

			RefCurrency newCurrency = Factory.NewWithValidTestData<RefCurrency>();
			RefExchangeRate todaysExchangeRate = newCurrency.ExchangeRates.AddNew();

			todaysExchangeRate.RE_RX_NKExCurrency = newCurrency.RX_Code;
			todaysExchangeRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.BuyRate;
			todaysExchangeRate.RE_SellRate = exchangeRate;
			todaysExchangeRate.RE_GC = GlbCompany.CurrentCompany.PK;
			todaysExchangeRate.RE_StartDate = ZDateTime.Now.AddDays(-10);
			todaysExchangeRate.RE_ExpiryDate = ZDateTime.Now.AddDays(10);

			newTransactionHeader.AH_RX_NKTransactionCurrency = newCurrency.RX_Code;
			newTransactionHeader.AH_ExchangeRate = exchangeRate;
			newTransactionHeader.AH_OSTotal = oSAmount;
			newTransactionHeader.AH_InvoiceAmount = -186.00M;
			newTransactionHeader.AH_GSTAmount = -0.68M;

			AssertEquals("Local Amount", localAmount, newTransactionHeader.AH_InvoiceAmount + newTransactionHeader.AH_GSTAmount);
			AssertEquals("OS Amount", oSAmount, newTransactionHeader.AH_OSTotal);
			AssertEquals("Exchange Rate", exchangeRate, newTransactionHeader.AH_ExchangeRate);

			newApprovalItem.A2_AH = ZGuid.Empty;
			AssertEquals("OS Amount Paid This Run", 0M, newApprovalItem.OSAmountPaidThisRun);

			newApprovalItem.A2_AH = newTransactionHeader.PK;
			newApprovalItem.A2_PaymentThisRun = localAmount;

			AssertEquals("OS Amount Paid This Run", oSAmount, newApprovalItem.OSAmountPaidThisRun);
		}

		public void TestOSAmountPaidThisRunRecalulatesProperlyFromLocalAmountWhenPartPayingWithTrickyAmounts()
		{
			ZDecimal invoiceAmount = -14986.94m;
			ZDecimal oSTotal = -9666.58m;
			ZDecimal exchangeRate = 0.645m;
			ZString currency = "USD";

			TransactionHeader newTransactionHeader = Factory.New<APInvoice>();
			PaymentApprovalItem newApprovalItem = Factory.New<PaymentApprovalItem>();

			newTransactionHeader.AH_RX_NKTransactionCurrency = currency;
			newTransactionHeader.AH_ExchangeRate = exchangeRate;
			newTransactionHeader.AH_OSTotal = oSTotal;
			newTransactionHeader.AH_InvoiceAmount = invoiceAmount;

			newApprovalItem.A2_AH = ZGuid.Empty;
			AssertEquals("OS Amount Paid This Run", 0M, newApprovalItem.OSAmountPaidThisRun);

			newApprovalItem.A2_AH = newTransactionHeader.PK;
			newTransactionHeader.AH_OutstandingAmount = -14986.94m;
			newApprovalItem.A2_PaymentThisRun = -5426.36m;
			ZDecimal amountCalculatedByLocalToForeign = Env.CurrentCompany.ExchangeRate.LocalToForeign(newApprovalItem.A2_PaymentThisRun, exchangeRate, currency);
			ZDecimal amountCalculatedByHighPrecisionExRate = TransactionHeaderOSOutstandingAmountProvider.GetHighPrecisionOSOutstandingAmount(invoiceAmount, oSTotal, newApprovalItem.A2_PaymentThisRun, currency);

			AssertEquals("When part paying, OS Amount Paid This Run is calculated by LocalToForeign", amountCalculatedByLocalToForeign, newApprovalItem.OSAmountPaidThisRun);

			newTransactionHeader.AH_OutstandingAmount = -9560.58m;
			newApprovalItem.A2_PaymentThisRun = -9560.58m;
			amountCalculatedByLocalToForeign = Env.CurrentCompany.ExchangeRate.LocalToForeign(newApprovalItem.A2_PaymentThisRun, exchangeRate, currency);
			amountCalculatedByHighPrecisionExRate = TransactionHeaderOSOutstandingAmountProvider.GetHighPrecisionOSOutstandingAmount(invoiceAmount, oSTotal, newApprovalItem.A2_PaymentThisRun, currency);

			AssertNotEquals(amountCalculatedByHighPrecisionExRate, amountCalculatedByLocalToForeign);
			AssertEquals("When part paying the rest, OS Amount Paid This Run is calculated by HighPrecisionExchangeRate", amountCalculatedByHighPrecisionExRate, newApprovalItem.OSAmountPaidThisRun);
		}

		public void TestOSAmountPaidThisRun_NewOSOutstandingAmountFeature()
		{
			const decimal localTotal = -714.29m;
			const decimal oSTotal = -10000000m;
			const decimal exchangeRate = 14000m;
			const string currency = "IDR";

			TransactionHeader newTransactionHeader = Factory.New<APInvoice>();

			newTransactionHeader.AH_RX_NKTransactionCurrency = currency;
			newTransactionHeader.AH_ExchangeRate = exchangeRate;
			newTransactionHeader.AH_OSTotal = oSTotal;
			newTransactionHeader.AH_InvoiceAmount = localTotal;
			AssertEquals("PreCondition", localTotal, newTransactionHeader.AH_LocalTotal);
			AssertEquals("PreCondition", oSTotal, newTransactionHeader.AH_OSTotal);

			AssertOSAmountPaidThisRun(isEnableNewOSOutstandingAmountFeature: false);
			AssertOSAmountPaidThisRun(isEnableNewOSOutstandingAmountFeature: true);

			void AssertOSAmountPaidThisRun(bool isEnableNewOSOutstandingAmountFeature)
			{
				newTransactionHeader.AH_IsOSOutstandingAmountApplicable = isEnableNewOSOutstandingAmountFeature;
				var newApprovalItem = Factory.New<PaymentApprovalItem>();
				newApprovalItem.A2_AH = newTransactionHeader.PK;
				AssertEquals("OS Amount Paid This Run", 0m, newApprovalItem.OSAmountPaidThisRun);

				using (AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, isEnableNewOSOutstandingAmountFeature))
				{
					newTransactionHeader.AH_OutstandingAmount = localTotal;
					newApprovalItem.A2_PaymentThisRun = -214.29m;
					newApprovalItem.A2_OSPaymentThisRun = -3000000m;

					if (isEnableNewOSOutstandingAmountFeature)
					{
						AssertEquals("When enable EnableNewOSOutstandingAmountFeature, OS Amount is equals to A2_OSPaymentThisRun", -3000000m, newApprovalItem.OSAmountPaidThisRun);
					}
					else
					{
						var amountCalculatedByLocalToForeign = Env.CurrentCompany.ExchangeRate.LocalToForeign(newApprovalItem.A2_PaymentThisRun, exchangeRate, currency);
						var amountCalculatedByHighPrecisionExRate = TransactionHeaderOSOutstandingAmountProvider.GetHighPrecisionOSOutstandingAmount(localTotal, oSTotal, newApprovalItem.A2_PaymentThisRun, currency);
						AssertNotEquals(-3000000m, amountCalculatedByLocalToForeign);
						AssertEquals("When part paying, OS Amount Paid This Run is calculated by LocalToForeign", amountCalculatedByLocalToForeign, newApprovalItem.OSAmountPaidThisRun);
					}

					newTransactionHeader.AH_OutstandingAmount = -214.29m;
					newApprovalItem.A2_PaymentThisRun = -214.29m;
					newApprovalItem.A2_OSPaymentThisRun = -3000000m;

					if (isEnableNewOSOutstandingAmountFeature)
					{
						AssertEquals("When enable EnableNewOSOutstandingAmountFeature, OS Amount is equals to A2_OSPaymentThisRun", -3000000m, newApprovalItem.OSAmountPaidThisRun);
					}
					else
					{
						var amountCalculatedByLocalToForeign = Env.CurrentCompany.ExchangeRate.LocalToForeign(newApprovalItem.A2_PaymentThisRun, exchangeRate, currency);
						var amountCalculatedByHighPrecisionExRate = TransactionHeaderOSOutstandingAmountProvider.GetHighPrecisionOSOutstandingAmount(localTotal, oSTotal, newApprovalItem.A2_PaymentThisRun, currency);

						AssertNotEquals(amountCalculatedByHighPrecisionExRate, amountCalculatedByLocalToForeign);
						AssertNotEquals(-3000000m, amountCalculatedByHighPrecisionExRate);
						AssertEquals("When part paying the rest, OS Amount Paid This Run is calculated by HighPrecisionExchangeRate", amountCalculatedByHighPrecisionExRate, newApprovalItem.OSAmountPaidThisRun);
					}
				}
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<PaymentApprovalItem>();
		}
	}
}
