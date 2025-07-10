using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.GUI.Testing
{
	public class AlterReceiptManagerTest : TestCaseWithFactory
	{
		#region TestShowForm

		public void TestShowForm()
		{
			APReceipt aPReceipt = Factory.New<APReceipt>();
			MatchingBase testMatching = new APMatchingBase(Factory, aPReceipt);
			fReceiptManager = new AlterReceiptManager(testMatching);

			using (NewMatchGroupForm testForm = new NewMatchGroupForm(testMatching))
			{
				fReceiptManager.ShowForm(testForm);

				using (var alterRecForm = ZFormModaliser.ActiveForm as AlterReceiptForm)
				{
					AssertNotNull("The AlterReceipt form should pop up", alterRecForm);

					AssertNotNull("The ControllerID must be set for Workflow tab security checkpoint", alterRecForm.ControllerID);
					AssertEquals("ControllerID should be for AP Receipts", ControllerIDs.ZAPReceipt, alterRecForm.ControllerID);

					alterRecForm.Close();
				}
			}
		}

		#endregion

		#region TestShowFormWithValidationErrors

		public void TestShowFormWithValidationErrors()
		{
			var apReceipt = Factory.NewWithValidTestData<APReceipt>();
			apReceipt.AH_OSExTaxAmount = 10m;

			var apInvoice = GetAPInvoice(-100m, 1m, -100m, GlbCompany.CurrentCompany.LocalCurrency);

			var testMatching = new APMatchingBase(Factory, apReceipt);
			testMatching.MatchedTransactions.Add(apInvoice);
			fReceiptManager = new AlterReceiptManager(testMatching);

			AssertEquals("Matching Balance should be -110", -110m, testMatching.Balance);

			using (var testForm = new NewMatchGroupForm(testMatching))
			{
				fReceiptManager.ShowForm(testForm);

				using (var alterRecForm = ZFormModaliser.ActiveForm as AlterReceiptForm)
				{
					AssertNotNull("The AlterReceipt form should pop up", alterRecForm);
					AssertEquals("Receipt amount should be adjusted to -100", -100m, apReceipt.AH_OSExTaxAmount);
					AssertEquals("Receipt amount has validation error", "Overseas amount must be greater than zero", apReceipt.AH_OSExTaxAmountInfo.GetErrors().GetFirstMessage());

					alterRecForm!.Close();
				}
			}

			AssertEquals("Matching Balance should be zero", 0m, testMatching.Balance);
		}

		#endregion

		#region TestPrepareReceiptAllSameCurrency

		public void TestPrepareReceipttAllSameCurrency()
		{
			RefCurrency oSCurrency = Factory.NewWithValidTestData<RefCurrency>();
			CoreTestForPrepareReceipt(oSCurrency, oSCurrency);

			AccountingConfigurationRegistry.Instance.UseInvoiceExchangeRateWhenChangingReceiptAmount.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			CoreTestForPrepareReceipt(oSCurrency, oSCurrency);
		}

		#endregion

		#region TestPreparePaymentDifferentCurrencies

		public void TestPreparePaymentDifferentCurrencies()
		{
			RefCurrency oSCurrency1 = Factory.NewWithValidTestData<RefCurrency>();
			RefCurrency oSCurrency2 = Factory.NewWithValidTestData<RefCurrency>();

			CoreTestForPrepareReceipt(oSCurrency1, oSCurrency2);
		}

		void CoreTestForPrepareReceipt(RefCurrency oSCurrency1, RefCurrency oSCurrency2)
		{
			APReceipt aPReceipt = GetAPReceiptApproval(oSCurrency1, 0.70M);
			APInvoice aPInv1 = GetAPInvoice(50M, 0.75M, 30M, oSCurrency1);
			APInvoice aPInv2 = GetAPInvoice(21M, 0.70m, 21M, oSCurrency2);

			MatchingBase testMatching = new APMatchingBase(Factory, aPReceipt);
			testMatching.MatchedTransactions.Add(aPInv1);
			testMatching.MatchedTransactions.Add(aPInv2);

			Assert("Matching Balance should not be zero", testMatching.Balance != 0m);
			ZDecimal expectedReceiptAmount = testMatching.Balance;

			fReceiptManager = new AlterReceiptManager(testMatching);
			fReceiptManager.PrepareReceipt_ForTestOnly();
			fReceiptManager.AlterReceiptFormClosed_ForTestOnly(null, null);

			if (oSCurrency1 == oSCurrency2)
			{
				if (AccountingConfigurationRegistry.Instance.UseInvoiceExchangeRateWhenChangingReceiptAmount.Value)
				{
					AssertEquals("Payment Exchange Rate", 0.728571M, aPReceipt.AH_ExchangeRate);
					AssertEquals("Payment Local Amount", 70M, aPReceipt.AH_LocalExTaxAmount);
					AssertEquals("Payment OS Amount", 51M, aPReceipt.AH_OSExTaxAmount);
					AssertEquals("Matching Balance should be zero", 0M, testMatching.Balance);
				}
				else
				{
					AssertEquals("Payment Exchange Rate", 0.70M, aPReceipt.AH_ExchangeRate);
					AssertEquals("Payment Local Amount", 72.86M, aPReceipt.AH_LocalExTaxAmount);
					AssertEquals("Payment OS Amount", 51M, aPReceipt.AH_OSExTaxAmount);
					AssertEquals("Matching Balance should not be zero", -2.86M, testMatching.Balance);
				}
			}
			else
			{
				AssertEquals("Payment Exchange Rate", 0.70M, aPReceipt.AH_ExchangeRate);
				AssertEquals("Payment Local Amount", 70M, aPReceipt.AH_LocalExTaxAmount);
				AssertEquals("Payment OS Amount", 49M, aPReceipt.AH_OSExTaxAmount);
				AssertEquals("Matching Balance should be zero", 0M, testMatching.Balance);
			}
		}

		#endregion

		#region Test Balance Is Validated

		public void TestBalanceIsValidatedOnClosing()
		{
			APReceipt aPReceipt = Factory.New<APReceipt>();
			APInvoice aPInv = GetAPInvoice(10m, 1m, 10m, GlbCompany.CurrentCompany.LocalCurrency);

			MatchingBase testMatching = new APMatchingBase(Factory, aPReceipt);
			testMatching.MatchedTransactions.Add(aPInv);

			fReceiptManager = new AlterReceiptManager(testMatching);
			using (ZForm testForm = new ZForm())
			{
				AssertEquals("The balance is 10 before form shown", 10m, testMatching.Balance);
				fReceiptManager.ShowForm(testForm);
				AlterReceiptForm alterRecForm = ZFormModaliser.ActiveForm as AlterReceiptForm;
				AssertNotNull("AlterReceiptForm should be shown", alterRecForm);
				AssertEquals("ControllerID should be for AP Receipts", ControllerIDs.ZAPReceipt, alterRecForm.ControllerID);

				alterRecForm.Close();
				AssertEquals("The balance should now be 0", 0m, testMatching.Balance);
				AssertEquals("Local Amount is 10", 10m, aPReceipt.AH_OSExTaxAmount);
			}
		}

		public void TestShowFormWhenNoReceipt()
		{
			APReceipt aPReceipt = Factory.New<APReceipt>();
			MatchingBase testMatching = new APMatchingBase(Factory, aPReceipt);
			fReceiptManager = new AlterReceiptManager(testMatching);

			using (NewMatchGroupForm testForm = new NewMatchGroupForm(testMatching))
			{
				fReceiptManager.FMatchingBase_ForTestOnly = null;
				fReceiptManager.ShowForm(testForm);
				AssertEquals("Error should be shown", "There is no receipt linked.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#endregion

		#region TestShowFormWhenARLedger

		public void TestShowFormWhenARLedger()
		{
			var arReceipt = Factory.New<ARReceipt>();
			MatchingBase testMatching = new ARMatchingBase(Factory, arReceipt);
			fReceiptManager = new AlterReceiptManager(testMatching);

			using (NewMatchGroupForm testForm = new NewMatchGroupForm(testMatching))
			{
				fReceiptManager.ShowForm(testForm);

				using (var alterRecForm = ZFormModaliser.ActiveForm as AlterReceiptForm)
				{
					AssertNotNull("The AlterReceipt form should pop up", alterRecForm);
					AssertEquals("ControllerID should be for AR Receipts", ControllerIDs.ZARReceipt, alterRecForm.ControllerID);
					alterRecForm.Close();
				}
			}
		}

		#endregion

		#region Implementation

		APInvoice GetAPInvoice(ZDecimal oSExTaxAmount, ZDecimal exchangeRate, ZDecimal oSPartialPaid, RefCurrency currency)
		{
			APInvoice aPInv = Factory.NewWithValidTestData<APInvoice>();
			aPInv.AH_RX_NKTransactionCurrency = currency.RX_Code;
			aPInv.AH_ExchangeRate = exchangeRate;
			aPInv.AH_OSExTaxAmount = oSExTaxAmount;
			((IMatching)aPInv).OSPartialPaymentAmount = oSPartialPaid;
			return aPInv;
		}

		APReceipt GetAPReceiptApproval(RefCurrency currency, ZDecimal exchangeRate)
		{
			APReceipt aPReceipt = Factory.New<APReceipt>();
			aPReceipt.AH_RX_NKTransactionCurrency = currency.RX_Code;
			aPReceipt.AH_ExchangeRate = exchangeRate;
			return aPReceipt;
		}

		AlterReceiptManager fReceiptManager;

		#endregion
	}
}
