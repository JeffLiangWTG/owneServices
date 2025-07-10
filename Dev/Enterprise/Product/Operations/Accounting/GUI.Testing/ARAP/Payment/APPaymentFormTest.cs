using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.HotCheque;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.ARAP.Testing
{
	[TestedType(typeof(APPaymentForm))]
	public class APPaymentFormTest : PaymentFormTestCase
	{
		#region Test Class

		public class MockAPPaymentForm : APPaymentForm
		{
			public MockAPPaymentForm(APPayment aPPay)
				: base(aPPay)
			{
			}

			protected override HotChequeLinkForm GetHotChequeLinkForm(HotChequeLink chequeLink)
			{
				return new HotChequeLinkFormTest.MockHotChequeLinkForm(chequeLink);
			}
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			Payment = Factory.New<APPayment>();
		}

		protected override Form GetFormToBashCore()
		{
			var result = new APPaymentForm(Payment);
			result.ControllerID = ControllerIDs.ZAPPayment;
			return result;
		}

		AccHotCheque GetNewHotCheque(OrgHeader org, AccChequeBook chequeBook)
		{
			AccHotCheque hotCheque = Factory.NewWithValidTestData<AccHotCheque>();
			hotCheque.AQ_OH = org.PK;
			hotCheque.AQ_AK = chequeBook.PK;
			hotCheque.AQ_Cancelled = false;
			hotCheque.AQ_AH = ZGuid.Empty;
			return hotCheque;
		}

		APPayment Payment;

		#endregion

		#region SettingOrgDisplaysHotCheques Test

		public void TestSettingOrgDisplaysHotCheques()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			AccChequeBook chequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			chequeBook.AK_GB = GlbBranch.CurrentBranch.PK;
			AccHotCheque hotCheque = GetNewHotCheque(org, chequeBook);

			using (APPaymentForm form = (APPaymentForm)GetFormToBashCore())
			{
				form.Show();
				form.APPayment.AH_OH = org.PK;
				form.EndInvoke_ForTest();
				using (HotChequeLinkForm hotChequesForm = ZFormModaliser.ActiveForm as HotChequeLinkForm)
				{
					AssertNotNull("HotChequeLinkForm should have popped up", hotChequesForm);
					AssertEquals("There should be one HotCheque in the collection", 1, hotChequesForm.HotChequeLink.HotCheques.Count);
					Assert("The collection should contain the test HotCheque", hotChequesForm.HotChequeLink.HotCheques.Contains(hotCheque));
				}
			}
		}

		#endregion

		#region SelectHotChequeImportsIntoPayment Test

		public void TestSelectHotChequeImportsIntoPayment()
		{
			// select hot cheque in link form and assert payment properties are set correctly
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			AccBankAccount bank = Factory.NewWithValidTestData<AccBankAccount>();
			bank.AB_GB = GlbBranch.CurrentBranch.PK;
			AccChequeBook chequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			chequeBook.AK_AB = bank.PK;
			AccHotCheque hotCheque = GetNewHotCheque(org, chequeBook);
			hotCheque.AQ_ChequeNumber = "789900";
			hotCheque.AQ_Amount = 7799.90m;

			using (MockAPPaymentForm form = new MockAPPaymentForm(Payment))
			{
				form.Show();
				form.APPayment.AH_OH = org.PK;
				form.EndInvoke_ForTest();
				using (HotChequeLinkFormTest.MockHotChequeLinkForm hotChequesForm = ZFormModaliser.ActiveForm as HotChequeLinkFormTest.MockHotChequeLinkForm)
				{
					AssertEquals("There should be 1 HotCheque in the collection", 1, hotChequesForm.HotChequeLink.HotCheques.Count);
					hotChequesForm.HotChequeGrid_Exposed.ListManager.Position = 1;
					hotChequesForm.HandleDoubleClick_Exposed(hotChequesForm, EventArgs.Empty);
					AssertEquals("Payment type should be Cheque", ReceiptTypes.Cheque, form.APPayment.AH_ReceiptType);
					AssertEquals("Payment Bank account should be TestBank", bank.PK, form.APPayment.AH_AB);
					AssertEquals("Payment Cheque book should be TestChequeBook", chequeBook.PK, form.APPayment.ChequeBook);
					AssertEquals("Payment cheque number should be 789900", "789900", form.APPayment.AH_ChequeOrReference);
					AssertEquals("Payment amount should be 7799.90", 7799.90m, form.APPayment.AH_OSExTaxAmount);
				}
			}
		}

		#endregion

		#region NotifyUserPaymentUneditable

		public void TestNotifyUserPaymentUneditable()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			AccBankAccount bank = Factory.NewWithValidTestData<AccBankAccount>();
			AccChequeBook chequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			chequeBook.AK_AB = bank.PK;
			chequeBook.AK_StartNo = 1;
			chequeBook.AK_LastNo = 100;
			AccHotCheque hotCheque = GetNewHotCheque(org, chequeBook);
			hotCheque.AQ_ChequeNumber = "000089";
			hotCheque.AQ_Amount = 10m;

			using (APPaymentForm form = new APPaymentForm(Payment))
			{
				form.Show();
				form.APPayment.ImportSelectedHotCheque(hotCheque);
				form.APPayment.AH_ChequeOrReference = "000090";
				Assert("Popup information message should be shown", UnitTestUserNotification.Instance.LastMessage.WasInformation);
				AssertEquals("Information text should be as follows", APPayment.AH_ChequeOrReferenceError, UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.APPayment.AH_OSExTaxAmount = 11m;
				Assert("Popup information message should be displayed", UnitTestUserNotification.Instance.LastMessage.WasInformation);
				AssertEquals("Information text should relate to OS Ex Tax", APPayment.AH_OSExTaxActualError, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#endregion

		public void TestEDIMessagesPluginAttached()
		{
			using (APPaymentForm form = new APPaymentForm(Payment))
			{
				Assert("Form should have the plugin attached", form.PlugIns.GetPlugIn(ControllerIDs.LinkedeNettEDIMessage) != null);
			}
		}

		#region Exchange Rate is empty while reversing a foreign AP Payment

		public void TestExchangeRateIsEmptyWhileReversingAForeignAPPayment()
		{
			Payment.AH_RX_NKTransactionCurrency = "USD";
			Payment.AH_ChequeOrReference = "32423";
			Payment.AH_ExchangeRate = 0.7m;
			Payment.AH_OSExTaxAmount = 11m;

			Factory.Save();

			Payment.IsReverseTransaction = true;
			using (var form = new APPaymentForm(Payment))
			{
				form.DisplayMode = ODisplayMode.Delete;

				form.Show();

				var exchangeRateControl = form.Controls.Find("ExchangeRateControl", true).FirstOrDefault();
				AssertNotNull("ExchangeRateControl", exchangeRateControl);
				var rateCalcEdit = exchangeRateControl.Controls.Find("RateCalcEdit", true).FirstOrDefault();
				AssertNotNull("RateCalcEdit", rateCalcEdit);

				AssertEquals("Should be equal to the exchange rate.", 0.7m, ZDecimal.Parse(rateCalcEdit.Text));

				Application.DoEvents();

				AssertEquals("Shouldn't be refreshed to 0m.", 0.7m, ZDecimal.Parse(rateCalcEdit.Text));
			}
		}

		#endregion
	}
}
