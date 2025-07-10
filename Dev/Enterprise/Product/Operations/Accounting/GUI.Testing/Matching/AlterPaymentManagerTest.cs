using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.GUI.ARAP;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.GUI.Testing
{
	public class AlterPaymentManagerTest : TestCaseWithFactory
	{
		#region TestShowForm

		public void TestShowForm()
		{
			APPaymentApprovalWithAuthorisation aPPaymentApproval = Factory.New<APPaymentApprovalWithAuthorisation>();
			PaymentApprovalMatchingBase testMatching = new APPaymentApprovalMatching(Factory, aPPaymentApproval);
			fPaymentManager = new AlterPaymentManager(testMatching);

			using (NewMatchGroupForm testForm = new NewMatchGroupForm(testMatching))
			{
				AssertControlsAreReadonly(aPPaymentApproval, false);

				fPaymentManager.ShowForm(testForm);
				AssertControlsAreReadonly(aPPaymentApproval, true);

				using (var alterPayForm = ZFormModaliser.ActiveForm as AlterPaymentApprovalForm)
				{
					AssertNotNull("The AlterPayment form should pop up", alterPayForm);
					AssertNotNull("The ControllerID must be set for Workflow tab security checkpoint", alterPayForm.ControllerID);
					AssertEquals("ControllerID should be for AP Payments", ControllerIDs.ZAPPayment, alterPayForm.ControllerID);

					alterPayForm.Close();
					AssertControlsAreReadonly(aPPaymentApproval, false);

					Assert("Organisation should NOT be readonly", !aPPaymentApproval.AV_OHInfo.ReadOnly);
				}
			}
		}

		void AssertControlsAreReadonly(PaymentApprovalBase approval, bool shouldBeReadOnly)
		{
			AssertEquals(shouldBeReadOnly, approval.AV_OHInfo.ReadOnly);
		}

		#endregion

		#region TestShowFormWithValidationErrors

		public void TestShowFormWithValidationErrors()
		{
			var apPaymentApproval = Factory.NewWithValidTestData<APPaymentApprovalWithAuthorisation>();
			apPaymentApproval.AV_Amount = 10m;

			var arInvoice = GetInvoice<ARInvoice>(100m, 1m, 100m, GlbCompany.CurrentCompany.LocalCurrency);

			var testMatching = new APPaymentApprovalMatching(Factory, apPaymentApproval);
			testMatching.MatchedTransactions.Add(arInvoice);
			fPaymentManager = new AlterPaymentManager(testMatching);

			AssertEquals("Matching Balance should be 110", 110m, testMatching.Balance);

			using (var testForm = new NewMatchGroupForm(testMatching))
			{
				fPaymentManager.ShowForm(testForm);

				using (var alterPayForm = ZFormModaliser.ActiveForm as AlterPaymentApprovalForm)
				{
					AssertNotNull("The AlterPayment form should pop up", alterPayForm);
					AssertEquals("Payment amount should be adjusted to -100", -100m, apPaymentApproval.AV_Amount);
					AssertEquals("Payment amount has validation error", "Overseas amount must be greater than zero", apPaymentApproval.AV_AmountInfo.GetErrors().GetFirstMessage());

					alterPayForm!.Close();
				}
			}

			AssertEquals("Matching Balance should be zero", 0m, testMatching.Balance);
		}

		#endregion

		#region TestPreparePaymentAllSameCurrency

		public void TestPreparePaymentAllSameCurrency()
		{
			RefCurrency oSCurrency = Factory.NewWithValidTestData<RefCurrency>();
			CoreTestForPreparePayment(oSCurrency, oSCurrency);
		}

		#endregion

		#region TestPreparePaymentDifferentCurrencies

		public void TestPreparePaymentDifferentCurrencies()
		{
			RefCurrency oSCurrency1 = Factory.NewWithValidTestData<RefCurrency>();
			RefCurrency oSCurrency2 = Factory.NewWithValidTestData<RefCurrency>();

			CoreTestForPreparePayment(oSCurrency1, oSCurrency2);
		}

		void CoreTestForPreparePayment(RefCurrency oSCurrency1, RefCurrency oSCurrency2)
		{
			APPaymentApprovalWithAuthorisation aPPaymentApproval = GetAPPaymentApproval(oSCurrency1, 0.70M);
			APInvoice aPInv1 = GetInvoice<APInvoice>(50M, 0.75M, -30M, oSCurrency1); // LocalPartialPaidAmount is -40.00
			APInvoice aPInv2 = GetInvoice<APInvoice>(21M, 0.70m, -21M, oSCurrency2); // LocalPartialPaidAmount is -30.00

			PaymentApprovalMatchingBase testMatching = new APPaymentApprovalMatching(Factory, aPPaymentApproval);
			testMatching.MatchedTransactions.Add(aPInv1);
			testMatching.MatchedTransactions.Add(aPInv2);

			Assert("Matching Balance should not be zero", testMatching.Balance != 0m);
			ZDecimal expectedPaymentAmount = testMatching.Balance;

			fPaymentManager = new AlterPaymentManager(testMatching);
			fPaymentManager.PreparePayment_ForTestOnly();

			AssertEquals("Payment Exchange Rate", 0.70M, aPPaymentApproval.AV_PayExRate);
			if (oSCurrency1 == oSCurrency2)
			{
				AssertEquals("Payment Local Amount", 72.86M, aPPaymentApproval.AV_Calc_LocalAmount);
				AssertEquals("Payment OS Amount", 51M, aPPaymentApproval.AV_Amount);
				AssertEquals("Matching Balance should not be zero", 2.86M, testMatching.Balance);
			}
			else
			{
				AssertEquals("Payment Local Amount", 70M, aPPaymentApproval.AV_Calc_LocalAmount);
				AssertEquals("Payment OS Amount", 49M, aPPaymentApproval.AV_Amount);
				AssertEquals("Matching Balance should be zero", 0M, testMatching.Balance);
			}
		}

		#endregion

		#region Test Balance Is Validated

		public void TestBalanceIsValidatedOnClosing()
		{
			APPaymentApprovalWithAuthorisation aPPaymentApproval = Factory.New<APPaymentApprovalWithAuthorisation>();
			APInvoice aPInv = GetInvoice<APInvoice>(10m, 1m, -10m, GlbCompany.CurrentCompany.LocalCurrency);

			PaymentApprovalMatchingBase testMatching = new APPaymentApprovalMatching(Factory, aPPaymentApproval);
			testMatching.MatchedTransactions.Add(aPInv);

			fPaymentManager = new AlterPaymentManager(testMatching);
			using (ZForm testForm = new ZForm())
			{
				AssertEquals("The balance is -10 before form shown", -10m, testMatching.Balance);
				fPaymentManager.ShowForm(testForm);
				AlterPaymentApprovalForm alterPayForm = ZFormModaliser.ActiveForm as AlterPaymentApprovalForm;
				AssertNotNull("AlterPaymentForm should be shown", alterPayForm);
				AssertEquals("ControllerID should be for AP Payments", ControllerIDs.ZAPPayment, alterPayForm.ControllerID);
				AssertEquals("Local Amount is 10", 10m, aPPaymentApproval.AV_Amount);
				AssertEquals("The balance should now be 0", 0m, testMatching.Balance);
				AssertEquals("Local Amount is 10", 10m, aPPaymentApproval.AV_Calc_LocalAmount);
				aPPaymentApproval.AV_PayExRate = 0.88m;
				alterPayForm.Close();
			}
		}

		#endregion

		#region TestShowFormWhenARLedger

		public void TestShowFormWhenARLedger()
		{
			var arPaymentApproval = Factory.New<ARPaymentApprovalWithAuthorisation>();
			PaymentApprovalMatchingBase testMatching = new ARPaymentApprovalMatching(Factory, arPaymentApproval);
			fPaymentManager = new AlterPaymentManager(testMatching);

			using (NewMatchGroupForm testForm = new NewMatchGroupForm(testMatching))
			{
				fPaymentManager.ShowForm(testForm);

				using (var alterPaymentForm = ZFormModaliser.ActiveForm as AlterPaymentApprovalForm)
				{
					AssertNotNull("The AlterPayment form should pop up", alterPaymentForm);
					AssertEquals("ControllerID should be for AR Payments", ControllerIDs.ZARPayment, alterPaymentForm.ControllerID);
					alterPaymentForm.Close();
				}
			}
		}

		#endregion

		#region Implementation

		T GetInvoice<T>(ZDecimal oSExTaxAmount, ZDecimal exchangeRate, ZDecimal oSPartialPaid, RefCurrency currency) where T : Invoice
		{
			T invoice = Factory.NewWithValidTestData<T>();
			invoice.AH_RX_NKTransactionCurrency = currency.RX_Code;
			invoice.AH_ExchangeRate = exchangeRate;
			invoice.AH_OSExTaxAmount = oSExTaxAmount;
			((IMatching)invoice).OSPartialPaymentAmount = oSPartialPaid;
			return invoice;
		}

		APPaymentApprovalWithAuthorisation GetAPPaymentApproval(RefCurrency currency, ZDecimal exchangeRate)
		{
			APPaymentApprovalWithAuthorisation aPPaymentApproval = Factory.New<APPaymentApprovalWithAuthorisation>();
			aPPaymentApproval.AV_RX_NKPaymentCurrency = currency.RX_Code;
			aPPaymentApproval.AV_PayExRate = exchangeRate;
			return aPPaymentApproval;
		}

		AlterPaymentManager fPaymentManager;

		#endregion
	}
}
