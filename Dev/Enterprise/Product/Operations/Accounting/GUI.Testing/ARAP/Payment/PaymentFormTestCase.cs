using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.Accounting.Business.ARAP.EPayment;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.GUI.ARAP.Payment;
using Enterprise.Accounting.GUI.ARAP.ReceiptPayment;
using Enterprise.Accounting.GUI.Testing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using DealStatusCodes = Enterprise.MasterFiles.Business.EPaymentStatusCodes.Deal;
using QuoteStatusCodes = Enterprise.MasterFiles.Business.EPaymentStatusCodes.Quote;

namespace Enterprise.Accounting.GUI.ARAP.Testing
{
	[TestedType(typeof(PaymentForm))]
	public class PaymentFormTestCase : AccountingZFormBasherTest
	{
		public void TestPaymentReasonVisibility()
		{
			SetupDataForTest();
			var bankAccount = TestObjectCreator.CreateEPaymentBankAccount();
			var payment = GetNewTestAPPayment(TestObjectCreator.AALSHI, bankAccount, ReceiptTypes.EPayment, TestCheques, "2", 200m);
			using (var paymentForm = new PaymentForm(payment))
			{
				paymentForm.Show();
				Application.DoEvents();

				Assert(payment.IsEPayment);
				var paymentReasonDropEdit = paymentForm.Controls.Find("PaymentReasonDropEdit", true).First() as ZDropEdit;
				Assert(paymentReasonDropEdit.Visible);

				payment.AH_ReceiptType = ReceiptTypes.Cheque;
				Assert(!paymentReasonDropEdit.Visible);
			}
		}

		#region Process E-Payment

		public void TestProcessEPaymentButton_Click_WhenPaymentHasAcceptedQuoteAndPaymentDetailsAreMatching()
		{
			SetupDataForTest();
			var bankAccount = TestObjectCreator.CreateEPaymentBankAccount();
			var payment = GetNewTestAPPayment(TestObjectCreator.AALSHI, bankAccount, ReceiptTypes.Cheque, TestCheques, "2", 200m, CurrencyCodes.UnitedStates, true);
			payment.RelatedPaymentApproval.AV_Status = PaymentApprovalStatus.FullyApproved;
			TestObjectCreator.CreateEPaymentStaffToken(bankAccount.PK, ZDateTime.UtcNow.AddHours(2), Env.CurrentUser.Initials, AccEPaymentStaffTokenLookups.StatusCodes.Authorised);
			var quote = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Accepted, payment.RelatedPaymentApproval);
			quote.QU_FeeAmount = 10m;
			quote.QU_RX_NKFeeCurrency = CurrencyCodes.Australia;
			var beneficary = TestObjectCreator.CreateEPaymentBeneficiary("apple");
			var accountDetails = payment.RelatedPaymentApproval.PayeeOrganisation.CompanyData.AccountDetailsCollection;
			accountDetails.RemoveAndDeleteAll();
			var ofxAccountDetail = accountDetails.AddNew();
			ofxAccountDetail.A1_PaymentMethod = EPaymentMethods.EPaymentViaOFX;
			ofxAccountDetail.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			ofxAccountDetail.A1_IsDefaultAccount = true;
			ofxAccountDetail.A1_EPaymentBeneficiaryId = beneficary.PK;
			Factory.Save();

			payment.RelatedPaymentApproval.PaymentQuotes.Reload(true);

			var deals = Factory.Load<AccEPaymentDeal>(new ZQuery(AccEPaymentDealSchema.AED_QU_Quote, quote.PK));
			AssertEquals(0, deals.Length);

			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			using (var paymentForm = new PaymentForm(payment))
			{
				paymentForm.Show();
				Application.DoEvents();

				UnitTestUserNotification.Instance.ClearMessages();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				paymentForm.ProcessEPaymentButton_ForTestOnly.PerformClick();
				AssertEquals(@"This FX transaction will be executed by a third party provider, OFX. CargoWise provides the messaging and information exchange only.
Review the details of the transaction to ensure they are correct.

Provider: OFX
Payment To: A.A.L. SHIPPING AGENCIES P/L
OS Amount: 200.00 USD
Exchange Rate: 1.000000 (1.000000)
Funding Currency Amount: 200.00 AUD
Processing Fee: 10.00 AUD
Funding Currency Total Cost: 210.00 AUD

The OS Amount is the amount your recipient will receive* and the Total Cost is the amount you will be required to pay to OFX.
*Occasionally third-party intermediary banks may deduct a fee from your transfer before paying your recipient. This fee may vary, and OFX receives no portion of it.

By clicking “Yes”, this FX transaction with OFX becomes legally binding if accepted by OFX. Would you like to continue?

Please check the status of the transaction after clicking ""Yes"".
If the OFX quote has expired, OFX cannot process the FX transaction and CargoWise will display the status as ""DEC - Provider Declined"".
If you have any issues or questions about the transaction, please contact OFX directly.", UnitTestUserNotification.Instance.LastMessage.Text);

				deals = Factory.Load<AccEPaymentDeal>(new ZQuery(AccEPaymentDealSchema.AED_QU_Quote, quote.PK));
				AssertEquals(0, deals.Length);

				UnitTestUserNotification.Instance.ClearMessages();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				paymentForm.ProcessEPaymentButton_ForTestOnly.PerformClick();

				var expectedMessage1 = @"This FX transaction will be executed by a third party provider, OFX. CargoWise provides the messaging and information exchange only.
Review the details of the transaction to ensure they are correct.

Provider: OFX
Payment To: A.A.L. SHIPPING AGENCIES P/L
OS Amount: 200.00 USD
Exchange Rate: 1.000000 (1.000000)
Funding Currency Amount: 200.00 AUD
Processing Fee: 10.00 AUD
Funding Currency Total Cost: 210.00 AUD

The OS Amount is the amount your recipient will receive* and the Total Cost is the amount you will be required to pay to OFX.
*Occasionally third-party intermediary banks may deduct a fee from your transfer before paying your recipient. This fee may vary, and OFX receives no portion of it.

By clicking “Yes”, this FX transaction with OFX becomes legally binding if accepted by OFX. Would you like to continue?

Please check the status of the transaction after clicking ""Yes"".
If the OFX quote has expired, OFX cannot process the FX transaction and CargoWise will display the status as ""DEC - Provider Declined"".
If you have any issues or questions about the transaction, please contact OFX directly.";

				var expectedMessage2 = "E-Payment Deal request generated to service provider OFX. Response may take from a few moments up to several minutes to be received.";

				Assert(UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(expectedMessage1));
				AssertEquals(expectedMessage2, UnitTestUserNotification.Instance.LastMessage.Text);

				deals = Factory.Load<AccEPaymentDeal>(new ZQuery(AccEPaymentDealSchema.AED_QU_Quote, quote.PK));
				AssertEquals(1, deals.Length);

				ZTabControl tabControl = (ZTabControl)paymentForm.Controls.Find("BankAndEPaymentTabControl", true).First();
				AssertEquals("After click, E-Payment tab page should be focused", 1, tabControl.SelectedIndex);
			}
		}

		public void TestProcessEPaymentButton_Click_WhenPaymentHasAcceptedQuoteAndPaymentDetailsAreDifferent()
		{
			SetupDataForTest();
			var bankAccount = TestObjectCreator.CreateEPaymentBankAccount();
			var payment = GetNewTestAPPayment(TestOrg, bankAccount, ReceiptTypes.Cheque, TestCheques, "2", 200m, CurrencyCodes.UnitedStates, true);
			payment.RelatedPaymentApproval.AV_Status = PaymentApprovalStatus.FullyApproved;
			TestObjectCreator.CreateEPaymentStaffToken(bankAccount.PK, ZDateTime.UtcNow.AddHours(2), Env.CurrentUser.Initials, AccEPaymentStaffTokenLookups.StatusCodes.Authorised);
			var quote = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Accepted, payment.RelatedPaymentApproval);
			quote.QU_FromAmount = 300m;
			var beneficary = TestObjectCreator.CreateEPaymentBeneficiary("apple");
			var accountDetails = payment.RelatedPaymentApproval.PayeeOrganisation.CompanyData.AccountDetailsCollection;
			accountDetails.RemoveAndDeleteAll();
			var ofxAccountDetail = accountDetails.AddNew();
			ofxAccountDetail.A1_PaymentMethod = EPaymentMethods.EPaymentViaOFX;
			ofxAccountDetail.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			ofxAccountDetail.A1_IsDefaultAccount = true;
			ofxAccountDetail.A1_EPaymentBeneficiaryId = beneficary.PK;
			Factory.Save();

			payment.RelatedPaymentApproval.PaymentQuotes.Reload(true);

			var deals = Factory.Load<AccEPaymentDeal>(new ZQuery(AccEPaymentDealSchema.AED_QU_Quote, quote.PK));
			AssertEquals(0, deals.Length);

			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			using (var paymentForm = new PaymentForm(payment))
			{
				paymentForm.Show();
				Application.DoEvents();

				UnitTestUserNotification.Instance.ClearMessages();
				paymentForm.ProcessEPaymentButton_ForTestOnly.PerformClick();

				AssertEquals("Payment has an accepted E-Quote, but its details don't match the payment. Please review payment details in order to continue.", UnitTestUserNotification.Instance.LastMessage.Text);

				deals = Factory.Load<AccEPaymentDeal>(new ZQuery(AccEPaymentDealSchema.AED_QU_Quote, quote.PK));
				AssertEquals(0, deals.Length);
			}
		}

		public void TestProcessEPaymentButton_Click_WhenPaymentDoesNotHaveAcceptedOrReceivedQuote()
		{
			SetupDataForTest();
			var bankAccount = TestObjectCreator.CreateEPaymentBankAccount();
			var payment = GetNewTestAPPayment(TestOrg, bankAccount, ReceiptTypes.Cheque, TestCheques, "2", 200m, CurrencyCodes.UnitedStates, true);
			payment.RelatedPaymentApproval.AV_Status = PaymentApprovalStatus.FullyApproved;
			TestObjectCreator.CreateEPaymentStaffToken(bankAccount.PK, ZDateTime.UtcNow.AddHours(2), Env.CurrentUser.Initials, AccEPaymentStaffTokenLookups.StatusCodes.Authorised);
			TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Discarded, payment.RelatedPaymentApproval);
			TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Failed, payment.RelatedPaymentApproval);
			TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Error, payment.RelatedPaymentApproval);
			var beneficary = TestObjectCreator.CreateEPaymentBeneficiary("apple");
			var accountDetails = payment.RelatedPaymentApproval.PayeeOrganisation.CompanyData.AccountDetailsCollection;
			accountDetails.RemoveAndDeleteAll();
			var ofxAccountDetail = accountDetails.AddNew();
			ofxAccountDetail.A1_PaymentMethod = EPaymentMethods.EPaymentViaOFX;
			ofxAccountDetail.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			ofxAccountDetail.A1_IsDefaultAccount = true;
			ofxAccountDetail.A1_EPaymentBeneficiaryId = beneficary.PK;
			Factory.Save();

			AssertEquals(3, payment.PaymentQuotes.Count);

			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			using (var paymentForm = new PaymentForm(payment))
			{
				paymentForm.Show();
				Application.DoEvents();

				UnitTestUserNotification.Instance.ClearMessages();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				paymentForm.ProcessEPaymentButton_ForTestOnly.PerformClick();
				AssertEquals("Payment must have an accepted E-Quote in order to continue. Would you like to request an E-Quote now?", UnitTestUserNotification.Instance.LastMessage.Text);

				payment.RelatedPaymentApproval.PaymentQuotes.Reload(true);
				AssertEquals(3, payment.PaymentQuotes.Count);

				UnitTestUserNotification.Instance.ClearMessages();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				paymentForm.ProcessEPaymentButton_ForTestOnly.PerformClick();
				AssertEquals("E-Quote request generated to service provider OFX. Response may take from a few moments up to several minutes to be received.", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText("Payment must have an accepted E-Quote in order to continue. Would you like to request an E-Quote now?"));

				payment.RelatedPaymentApproval.PaymentQuotes.Reload(true);
				AssertEquals(4, payment.PaymentQuotes.Count);
				AssertEquals(1, payment.PaymentQuotes.Cast<EPaymentQuoteForDisplay>().Count(x => x.RealQuote.QU_Status == QuoteStatusCodes.Queued));
				AssertEquals(3, payment.PaymentQuotes.Cast<EPaymentQuoteForDisplay>().Count(x => x.RealQuote.QU_Status == QuoteStatusCodes.Discarded));
			}
		}

		public void TestProcessEPaymentButton_Click_WhenPaymentHasAReceivedQuote()
		{
			SetupDataForTest();
			var bankAccount = TestObjectCreator.CreateEPaymentBankAccount();
			var payment = GetNewTestAPPayment(TestOrg, bankAccount, ReceiptTypes.Cheque, TestCheques, "2", 200m, CurrencyCodes.UnitedStates, true);
			payment.RelatedPaymentApproval.AV_Status = PaymentApprovalStatus.FullyApproved;
			TestObjectCreator.CreateEPaymentStaffToken(bankAccount.PK, ZDateTime.UtcNow.AddHours(2), Env.CurrentUser.Initials, AccEPaymentStaffTokenLookups.StatusCodes.Authorised);
			var quote = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Received, payment.RelatedPaymentApproval);
			quote.QU_FeeAmount = 10m;
			quote.QU_RX_NKFeeCurrency = CurrencyCodes.Australia;
			var beneficary = TestObjectCreator.CreateEPaymentBeneficiary("apple");
			var accountDetails = payment.RelatedPaymentApproval.PayeeOrganisation.CompanyData.AccountDetailsCollection;
			accountDetails.RemoveAndDeleteAll();
			var ofxAccountDetail = accountDetails.AddNew();
			ofxAccountDetail.A1_PaymentMethod = EPaymentMethods.EPaymentViaOFX;
			ofxAccountDetail.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			ofxAccountDetail.A1_IsDefaultAccount = true;
			ofxAccountDetail.A1_EPaymentBeneficiaryId = beneficary.PK;
			Factory.Save();

			payment.RelatedPaymentApproval.PaymentQuotes.Reload(true);

			var deals = Factory.Load<AccEPaymentDeal>(new ZQuery(AccEPaymentDealSchema.AED_QU_Quote, quote.PK));
			AssertEquals(0, deals.Length);

			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			using (var paymentForm = new PaymentForm(payment))
			{
				paymentForm.Show();
				Application.DoEvents();

				UnitTestUserNotification.Instance.ClearMessages();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				paymentForm.ProcessEPaymentButton_ForTestOnly.PerformClick();
				AssertEquals(@"This FX transaction will be executed by a third party provider, OFX. CargoWise provides the messaging and information exchange only.
Review the details of the transaction to ensure they are correct.
Payment must have an accepted E-Quote in order to continue. The following E-Quote is available for this Payment and will be automatically accepted if you proceed to book the deal.

Provider: OFX
Payment To: 
OS Amount: 200.00 USD
Exchange Rate: 1.000000 (1.000000)
Funding Currency Amount: 200.00 AUD
Processing Fee: 10.00 AUD
Funding Currency Total Cost: 210.00 AUD

The OS Amount is the amount your recipient will receive* and the Total Cost is the amount you will be required to pay to OFX.
*Occasionally third-party intermediary banks may deduct a fee from your transfer before paying your recipient. This fee may vary, and OFX receives no portion of it.

By clicking “Yes”, this FX transaction with OFX becomes legally binding if accepted by OFX. Would you like to continue?

Please check the status of the transaction after clicking ""Yes"".
If the OFX quote has expired, OFX cannot process the FX transaction and CargoWise will display the status as ""DEC - Provider Declined"".
If you have any issues or questions about the transaction, please contact OFX directly.", UnitTestUserNotification.Instance.LastMessage.Text);

				deals = Factory.Load<AccEPaymentDeal>(new ZQuery(AccEPaymentDealSchema.AED_QU_Quote, quote.PK));
				AssertEquals(0, deals.Length);

				UnitTestUserNotification.Instance.ClearMessages();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				paymentForm.ProcessEPaymentButton_ForTestOnly.PerformClick();

				var expectedMessage1 = @"This FX transaction will be executed by a third party provider, OFX. CargoWise provides the messaging and information exchange only.
Review the details of the transaction to ensure they are correct.
Payment must have an accepted E-Quote in order to continue. The following E-Quote is available for this Payment and will be automatically accepted if you proceed to book the deal.

Provider: OFX
Payment To: 
OS Amount: 200.00 USD
Exchange Rate: 1.000000 (1.000000)
Funding Currency Amount: 200.00 AUD
Processing Fee: 10.00 AUD
Funding Currency Total Cost: 210.00 AUD

The OS Amount is the amount your recipient will receive* and the Total Cost is the amount you will be required to pay to OFX.
*Occasionally third-party intermediary banks may deduct a fee from your transfer before paying your recipient. This fee may vary, and OFX receives no portion of it.

By clicking “Yes”, this FX transaction with OFX becomes legally binding if accepted by OFX. Would you like to continue?

Please check the status of the transaction after clicking ""Yes"".
If the OFX quote has expired, OFX cannot process the FX transaction and CargoWise will display the status as ""DEC - Provider Declined"".
If you have any issues or questions about the transaction, please contact OFX directly.";

				var expectedMessage2 = $"Quote {quote.QU_InternalReference} has been accepted.";

				var expectedMessage3 = "E-Payment Deal request generated to service provider OFX. Response may take from a few moments up to several minutes to be received.";

				Assert(UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(expectedMessage1));
				Assert(UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(expectedMessage2));
				AssertEquals(expectedMessage3, UnitTestUserNotification.Instance.LastMessage.Text);

				deals = Factory.Load<AccEPaymentDeal>(new ZQuery(AccEPaymentDealSchema.AED_QU_Quote, quote.PK));
				AssertEquals(1, deals.Length);
			}
		}

		public void TestProcessEPaymentButton_Click_HandleErrors()
		{
			SetupDataForTest();
			var payment = GetNewTestAPPayment(TestOrg, TestBank, ReceiptTypes.Cheque, TestCheques, "2", 200m, CurrencyCodes.UnitedStates, true);
			payment.RelatedPaymentApproval.AV_Status = PaymentApprovalStatus.FullyApproved;
			Factory.Save();
			var quote = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Received, payment.RelatedPaymentApproval);
			Factory.Save();

			AssertNotEquals(AccountTypeCodeDescriptionPairList.Codes.EPA, payment.BankAccount.AB_AccountType);
			var deals = Factory.Load<AccEPaymentDeal>(new ZQuery(AccEPaymentDealSchema.AED_QU_Quote, quote.PK));
			AssertEquals(0, deals.Length);

			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			using (var paymentForm = new PaymentForm(payment))
			{
				paymentForm.Show();
				Application.DoEvents();

				UnitTestUserNotification.Instance.ClearMessages();
				paymentForm.ProcessEPaymentButton_ForTestOnly.PerformClick();
				AssertEquals("Payment Bank Account must be an E-Payment Account in order to submit payment for electronic processing.", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(UnitTestUserNotification.Instance.LastMessage.WasError);
			}

			deals = Factory.Load<AccEPaymentDeal>(new ZQuery(AccEPaymentDealSchema.AED_QU_Quote, quote.PK));
			AssertEquals(0, deals.Length);
		}

		#endregion

		public void TestUpdateAccountNameCaption()
		{
			SetupDataForTest();
			var payment = GetNewTestAPPayment(TestOrg, TestBank, ReceiptTypes.EPayment, TestCheques, "2", 200m, CurrencyCodes.UnitedStates, true);

			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			{
				using (var paymentForm = new PaymentForm(payment))
				{
					paymentForm.Show();
					Application.DoEvents();

					var bankAccountGuidFindBox = paymentForm.Controls.Find("BankAccountGuidFindBox", true).First() as ZGuidFindBox;
					AssertEquals("E-Payment Account", bankAccountGuidFindBox.CaptionResourceString.Caption);

					payment.AH_ReceiptType = ReceiptTypes.Cheque;
					AssertEquals("Bank Account", bankAccountGuidFindBox.CaptionResourceString.Caption);
				}
			}
		}

		[TestDate(2021, 8, 26, 22, 00, 00)]
		public void TestDisplayQuoteTimeInLocalTimezone()
		{
			SetupDataForTest();
			var payment = GetNewTestAPPayment(TestOrg, TestBank, ReceiptTypes.EPayment, TestCheques, "2", 200m, CurrencyCodes.UnitedStates, true);
			Factory.Save();
			var quote = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Received, payment.RelatedPaymentApproval);
			var localTime = quote.QU_LastResponseReceivedUtc.ToLocalBranchTime();
			Factory.Save();
			payment.RelatedPaymentApproval.PaymentQuotes.Reload(true);
			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			using (var paymentForm = new PaymentForm(payment))
			{
				paymentForm.Show();
				Application.DoEvents();

				var bankAndEPaymentTabControl = paymentForm.Controls.Find("BankAndEPaymentTabControl", true).First() as ZTabControl;
				AssertNotNull(bankAndEPaymentTabControl);
				bankAndEPaymentTabControl.SelectNextTabPage();

				AssertEquals(1, payment.PaymentQuotes.Count);
				AssertEquals(localTime, payment.PaymentQuotes[0].LastResponseReceivedLocalTime);
			}
		}

		[TestDate(2021, 8, 26, 22, 00, 00)]
		public void TestDisplayDealTimeInLocalTimezone()
		{
			SetupDataForTest();
			var payment = GetNewTestAPPayment(TestObjectCreator.AALSHI, TestBank, ReceiptTypes.Cheque, TestCheques, "2", 200m, CurrencyCodes.UnitedStates, true);
			payment.RelatedPaymentApproval.AV_Status = PaymentApprovalStatus.FullyApproved;
			var quote = payment.RelatedPaymentApproval.TryToCreateQuote(EPaymentProviderCodes.Codes.OFX).Quote;
			Factory.Save();
			TestObjectCreator.CreateValidEPaymentDealForStatus(DealStatusCodes.Accepted, payment.RelatedPaymentApproval);
			Factory.Save();
			payment.RelatedPaymentApproval.PaymentQuotes.Reload(true);
			var localTime = payment.RelatedPaymentApproval.AV_SystemCreateTimeUtc.ToLocalBranchTime();
			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			using (var paymentForm = new PaymentForm(payment))
			{
					paymentForm.Show();
					Application.DoEvents();
					AssertEquals(localTime, payment.DealSubmittedLocalTime);
					AssertEquals(localTime, payment.DealLastResponseLocalTime);

					var bankAndEPaymentTabControl = paymentForm.Controls.Find("BankAndEPaymentTabControl", true).First() as ZTabControl;
					AssertNotNull(bankAndEPaymentTabControl);
					bankAndEPaymentTabControl.SelectNextTabPage();

					var dealSubmittedDateEdit = paymentForm.Controls.Find("DealSubmittedDateEdit", true).First() as ZDateEdit;
					AssertEquals(localTime, dealSubmittedDateEdit.DateTimeValue);

					var dealResponseDateEdit = paymentForm.Controls.Find("DealResponseDateEdit", true).First() as ZDateEdit;
					AssertEquals(localTime, dealResponseDateEdit.DateTimeValue);
				}
		}

		#region Accept Quote

		public void TestAcceptQuoteButton_Click_WhenPaymentHasChanges()
		{
			SetupDataForTest();
			var payment = GetNewTestAPPayment(TestOrg, TestBank, ReceiptTypes.Cheque, TestCheques, "2", 94M, CurrencyCodes.UnitedStates, true);
			Factory.Save();
			var quote = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Received, payment.RelatedPaymentApproval);
			Factory.Save();

			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			using (var paymentForm = new PaymentForm(payment))
			{
				paymentForm.Show();
				Application.DoEvents();

				var bankAndEPaymentTabControl = paymentForm.Controls.Find("BankAndEPaymentTabControl", true).First() as ZTabControl;
				AssertNotNull(bankAndEPaymentTabControl);
				bankAndEPaymentTabControl.SelectNextTabPage();

				UnitTestUserNotification.Instance.ClearMessages();
				payment.RelatedPaymentApproval.AV_PaymentComment = "new comment";
				paymentForm.AcceptQuoteButton_ForTestOnly.PerformClick();
				AssertEquals("Please save your payment approval first.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(QuoteStatusCodes.Received, quote.QU_Status);
			}
		}

		public void TestAcceptQuoteButton_Click_WhenNoQuotesSelected()
		{
			SetupDataForTest();
			var payment = GetNewTestAPPayment(TestOrg, TestBank, ReceiptTypes.Cheque, TestCheques, "2", 94M, CurrencyCodes.UnitedStates, true);
			Factory.Save();
			var quote = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Received, payment.RelatedPaymentApproval);
			payment.RelatedPaymentApproval.PaymentQuotes.Reload(true);

			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			using (var paymentForm = new PaymentForm(payment))
			{
				paymentForm.Show();
				Application.DoEvents();

				var bankAndEPaymentTabControl = paymentForm.Controls.Find("BankAndEPaymentTabControl", true).First() as ZTabControl;
				AssertNotNull(bankAndEPaymentTabControl);
				bankAndEPaymentTabControl.SelectNextTabPage();

				var epaymentGrid = paymentForm.Controls.Find("EPaymentGrid", true).First() as ZGrid;
				AssertNotNull(epaymentGrid);
				AssertEquals(0, epaymentGrid.SelectedElements.Length);

				UnitTestUserNotification.Instance.ClearMessages();
				paymentForm.AcceptQuoteButton_ForTestOnly.PerformClick();
				AssertEquals("Please select a Quote.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(QuoteStatusCodes.Received, quote.QU_Status);
			}
		}

		public void TestAcceptQuoteButton_Click_WhenMoreThanOneQuoteSelected()
		{
			SetupDataForTest();
			var payment = GetNewTestAPPayment(TestOrg, TestBank, ReceiptTypes.Cheque, TestCheques, "2", 94M, CurrencyCodes.UnitedStates, true);
			Factory.Save();
			var quote1 = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Received, payment.RelatedPaymentApproval);
			var quote2 = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Requested, payment.RelatedPaymentApproval);
			Factory.Save();
			payment.RelatedPaymentApproval.PaymentQuotes.Reload(true);

			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			using (var paymentForm = new PaymentForm(payment))
			{
				paymentForm.Show();
				Application.DoEvents();

				var bankAndEPaymentTabControl = paymentForm.Controls.Find("BankAndEPaymentTabControl", true).First() as ZTabControl;
				AssertNotNull(bankAndEPaymentTabControl);
				bankAndEPaymentTabControl.SelectNextTabPage();

				var epaymentGrid = paymentForm.Controls.Find("EPaymentGrid", true).First() as ZGrid;
				AssertNotNull(epaymentGrid);
				epaymentGrid.SelectAllElements();
				AssertEquals(2, epaymentGrid.SelectedElements.Length);

				UnitTestUserNotification.Instance.ClearMessages();
				paymentForm.AcceptQuoteButton_ForTestOnly.PerformClick();
				AssertEquals("Please select one Quote only.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(QuoteStatusCodes.Received, quote1.QU_Status);
				AssertEquals(QuoteStatusCodes.Requested, quote2.QU_Status);
			}
		}

		public void TestAcceptQuoteButton_Click_Success()
		{
			SetupDataForTest();

			//payment approval -> 1000 USD / 1.4 ex rate = 714.29 AUD
			var payment = GetNewTestAPPayment(TestOrg, TestBank, ReceiptTypes.Cheque, TestCheques, "2", 94M, CurrencyCodes.UnitedStates, true);
			payment.AH_ExchangeRate = 1.4m;
			payment.AH_OSExTaxAmount = 1000m;
			payment.RelatedPaymentApproval.AV_PayExRate = 1.4m;
			payment.RelatedPaymentApproval.AV_Amount = 1000m;
			payment.RelatedPaymentApproval.AV_Status = PaymentApprovalStatus.AwaitingApproval;
			Factory.Save();

			//invoice -> 1000 USD / 1.6 ex rate = 625 AUD
			var apInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV123", TestObjectCreator.USD, 1.6m, 1000m, 0m, 625m, 0m, TestObjectCreator.Creditor1, TestObjectCreator.GLHeader1.PK);
			Factory.Save();

			//exx journal -> 625 AUD - 714.29 AUD = -89.29 AUD
			var paymentApprovalItem = Factory.New<PaymentApprovalItem>();
			paymentApprovalItem.A2_AH = payment.PK;
			paymentApprovalItem.A2_AV = payment.RelatedPaymentApproval.PK;
			paymentApprovalItem.A2_PaymentThisRun = -apInvoice.AH_LocalExTaxAmount;
			payment.RelatedPaymentApproval.AV_ExchangeDifference = -89.29m; //for payment approvals, we do not create database record for exx journal, we store the amount on AV_ExchangeDifference column.
			Factory.Save();

			//quote -> 1000 USD / 1.2 ex rate = 833.33 AUD
			var quote1 = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Received, payment.RelatedPaymentApproval);
			quote1.QU_FromAmount = 833.33m;
			quote1.QU_ToAmount = 1000m;
			quote1.QU_ExchangeRate = 1.2m;
			quote1.QU_ExchangeRateInverted = 0.8333m;
			var quote2 = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Requested, payment.RelatedPaymentApproval);
			Factory.Save();

			var exxJournals = Factory.Load<ExchangeDifference>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.ExchangeDifference));
			AssertEquals(0, exxJournals.Length);

			var formFactory = new BusinessObjectFactory();
			var quote1InFormFactory = formFactory.LoadTop1<AccEPaymentQuote>(new ZQuery(AccEPaymentQuoteSchema.PK, quote1.PK));
			var quote2InFormFactory = formFactory.LoadTop1<AccEPaymentQuote>(new ZQuery(AccEPaymentQuoteSchema.PK, quote2.PK));
			var paymentInFormFactory = formFactory.LoadTop1<APPayment>(new ZQuery(AccTransactionHeaderSchema.PK, payment.PK));

			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			using (var paymentForm = new PaymentForm(paymentInFormFactory))
			{
				paymentForm.Show();
				Application.DoEvents();

				var bankAndEPaymentTabControl = paymentForm.Controls.Find("BankAndEPaymentTabControl", true).First() as ZTabControl;
				AssertNotNull(bankAndEPaymentTabControl);
				bankAndEPaymentTabControl.SelectNextTabPage();

				var epaymentGrid = paymentForm.Controls.Find("EPaymentGrid", true).First() as ZGrid;
				AssertNotNull(epaymentGrid);
				epaymentGrid.Select(0);
				AssertEquals(1, epaymentGrid.SelectedElements.Length);

				var selectedQuote = epaymentGrid.SelectedElements[0] as EPaymentQuoteForDisplay;
				AssertEquals("RCV - Received", selectedQuote.StatusDescription);

				UnitTestUserNotification.Instance.ClearMessages();
				paymentForm.AcceptQuoteButton_ForTestOnly.PerformClick();
				AssertEquals($"Quote {selectedQuote.InternalReference} has been accepted.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(QuoteStatusCodes.Accepted, quote1InFormFactory.QU_Status);
				AssertEquals(QuoteStatusCodes.Discarded, quote2InFormFactory.QU_Status);

				//new payment approval values -> 1000 USD / 1.2 ex rate = 833.33 AUD
				//invoice -> 1000 USD / 1.6 ex rate = 625 AUD
				//new exx journal -> 625 AUD - 833.33 AUD = -208.33 AUD
				AssertEquals(833.33m, paymentInFormFactory.RelatedPaymentApproval.AV_Calc_LocalAmount);
				AssertEquals(1000m, paymentInFormFactory.RelatedPaymentApproval.AV_Amount);
				AssertEquals(1.2000m, paymentInFormFactory.RelatedPaymentApproval.AV_PayExRate.Round(4));
				AssertEquals(-208.33m, paymentInFormFactory.RelatedPaymentApproval.AV_ExchangeDifference);
				var exxJournalsInFormFactory = formFactory.Load<ExchangeDifference>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.ExchangeDifference));
				AssertEquals(0, exxJournalsInFormFactory.Length);
			}
		}

		public void TestAcceptQuoteButton_Click_Error()
		{
			SetupDataForTest();
			var payment = GetNewTestAPPayment(TestOrg, TestBank, ReceiptTypes.Cheque, TestCheques, "2", 94M, CurrencyCodes.UnitedStates, true);
			payment.RelatedPaymentApproval.AV_Status = PaymentApprovalStatus.AwaitingApproval;
			Factory.Save();
			var quote1 = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Queued, payment.RelatedPaymentApproval);
			var quote2 = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Requested, payment.RelatedPaymentApproval);
			Factory.Save();
			payment.RelatedPaymentApproval.PaymentQuotes.Reload(true);

			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			using (var paymentForm = new PaymentForm(payment))
			{
				paymentForm.Show();
				Application.DoEvents();

				var bankAndEPaymentTabControl = paymentForm.Controls.Find("BankAndEPaymentTabControl", true).First() as ZTabControl;
				AssertNotNull(bankAndEPaymentTabControl);
				bankAndEPaymentTabControl.SelectNextTabPage();

				var epaymentGrid = paymentForm.Controls.Find("EPaymentGrid", true).First() as ZGrid;
				AssertNotNull(epaymentGrid);
				epaymentGrid.Select(0);
				AssertEquals(1, epaymentGrid.SelectedElements.Length);

				var selectedQuote = epaymentGrid.SelectedElements[0] as EPaymentQuoteForDisplay;
				AssertEquals("QUE - Queued", selectedQuote.StatusDescription);

				UnitTestUserNotification.Instance.ClearMessages();
				paymentForm.AcceptQuoteButton_ForTestOnly.PerformClick();
				AssertEquals("This quote is not in Received status.", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals(QuoteStatusCodes.Queued, quote1.QU_Status);
				AssertEquals(QuoteStatusCodes.Requested, quote2.QU_Status);
			}
		}

		#endregion

		#region E-Payment

		public void TestCheckEPaymentExchangeRateButton_Click_NoExistingActiveQuote()
		{
			SetupDataForTest();
			var payment = GetNewTestAPPayment(TestOrg, TestBank, ReceiptTypes.Cheque, TestCheques, "2", 200m, CurrencyCodes.UnitedStates, true);
			Factory.Save();
			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			{
				payment.RelatedPaymentApproval.AV_Status = PaymentApprovalStatus.AwaitingApproval;
				using (var paymentForm = new PaymentForm(payment))
				{
					paymentForm.Show();
					payment.RelatedPaymentApproval.AV_PaymentComment = "new comment";
					Assert(paymentForm.CheckEPayRateButton_ForTestOnly.Enabled);
					paymentForm.CheckEPayRateButton_ForTestOnly.PerformClick();
					AssertEquals("Please save your payment first.", UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessages();

					Factory.Save();

					var quote = Factory.LoadTop1<AccEPaymentQuote>(new ZQuery(AccEPaymentQuoteSchema.QU_AV, payment.RelatedPaymentApproval.PK));
					AssertNull("Pre-condition - no quote for payment in DB", quote);
					UnitTestUserNotification.Instance.AddOKAnswer();
					ZTabControl tabControl = (ZTabControl)paymentForm.Controls.Find("BankAndEPaymentTabControl", true).First();
					AssertEquals("Before click, Bank Detail tab page should be focused", 0, tabControl.SelectedIndex);
					paymentForm.CheckEPayRateButton_ForTestOnly.PerformClick();
					AssertEquals("After click, E-Payment tab page should be focused", 1, tabControl.SelectedIndex);
					AssertEquals("E-Quote request generated to service provider OFX. Response may take from a few moments up to several minutes to be received.", UnitTestUserNotification.Instance.LastMessage.Text);
					quote = Factory.LoadTop1<AccEPaymentQuote>(new ZQuery(AccEPaymentQuoteSchema.QU_AV, payment.RelatedPaymentApproval.PK));
					AssertNotNull("Post-condition - quote is created for payment", quote);
					AssertEquals("OFX", quote.QU_ProviderCode);
					AssertEquals(200m, quote.QU_ToAmount);
					AssertEquals("USD", quote.QU_RX_NKToCurrency);
					AssertEquals(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, quote.QU_RX_NKFromCurrency);
					AssertEquals(GlbCompany.CurrentCompany.PK, quote.QU_GC);
				}
			}
		}

		public void TestCheckEPaymentExchangeRateButton_Click_HasExistingQueuedQuote()
		{
			SetupDataForTest();
			var payment = GetNewTestAPPayment(TestOrg, TestBank, ReceiptTypes.Cheque, TestCheques, "2", 94M, CurrencyCodes.UnitedStates, true);
			payment.RelatedPaymentApproval.TryToCreateQuote(EPaymentProviderCodes.Codes.OFX);
			Factory.Save();

			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			{
				payment.RelatedPaymentApproval.AV_Status = PaymentApprovalStatus.AwaitingApproval;
				using (var paymentForm = new PaymentForm(payment))
				{
					paymentForm.Show();
					payment.RelatedPaymentApproval.AV_PaymentComment = "new comment";
					Assert(paymentForm.CheckEPayRateButton_ForTestOnly.Enabled);
					Factory.Save();

					UnitTestUserNotification.Instance.ClearMessages();
					paymentForm.CheckEPayRateButton_ForTestOnly.PerformClick();
					AssertEquals("Exchange rate already requested", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestCheckEPaymentExchangeRateButton_Click_HasExistingActiveQuote()
		{
			SetupDataForTest();
			var payment = GetNewTestAPPayment(TestOrg, TestBank, ReceiptTypes.Cheque, TestCheques, "2", 200m, CurrencyCodes.UnitedStates, true);
			var quote = payment.RelatedPaymentApproval.TryToCreateQuote(EPaymentProviderCodes.Codes.OFX).Quote;
			quote.QU_Status = QuoteStatusCodes.Failed;
			Factory.Save();

			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			{
				payment.RelatedPaymentApproval.AV_Status = PaymentApprovalStatus.AwaitingApproval;
				using (var paymentForm = new PaymentForm(payment))
				{
					paymentForm.Show();
					payment.RelatedPaymentApproval.AV_PaymentComment = "new comment";
					Assert(paymentForm.CheckEPayRateButton_ForTestOnly.Enabled);
					Factory.Save();

					UnitTestUserNotification.Instance.ClearMessages();
					paymentForm.CheckEPayRateButton_ForTestOnly.PerformClick();

					var quotes = Factory.Load<AccEPaymentQuote>(new ZQuery(AccEPaymentQuoteSchema.QU_AV, payment.RelatedPaymentApproval.PK));
					AssertEquals("Number of quotes", 2, quotes.Length);

					var oldQuote = quotes.First(q => q.PK == quote.PK);
					AssertEquals("Post-condition - old quote is discarded", QuoteStatusCodes.Discarded, oldQuote.QU_Status);

					var newQuote = quotes.First(q => q.PK != quote.PK);
					AssertNotNull("Post-condition - a new quote is created for payment", newQuote.IsInDatabase);
					AssertEquals("OFX", newQuote.QU_ProviderCode);
					AssertEquals(200m, newQuote.QU_ToAmount);
					AssertEquals("USD", newQuote.QU_RX_NKToCurrency);
					AssertEquals(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, newQuote.QU_RX_NKFromCurrency);
					AssertEquals(GlbCompany.CurrentCompany.PK, newQuote.QU_GC);
				}
			}
		}

		public void TestCheckEPaymentExchangeRateButton_Click_HasExistingQueuedQuote_PaymentDetailsChanged()
		{
			SetupDataForTest();
			var payment = GetNewTestAPPayment(TestOrg, TestBank, ReceiptTypes.Cheque, TestCheques, "2", 94M, CurrencyCodes.UnitedStates, true);
			var quote = payment.RelatedPaymentApproval.TryToCreateQuote(EPaymentProviderCodes.Codes.OFX).Quote;
			AssertEquals("Pre-condition - quote is queued", QuoteStatusCodes.Queued, quote.QU_Status);
			Factory.Save();

			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			{
				payment.RelatedPaymentApproval.AV_Status = PaymentApprovalStatus.AwaitingApproval;
				using (var paymentForm = new PaymentForm(payment))
				{
					paymentForm.Show();
					payment.RelatedPaymentApproval.AV_PaymentComment = "new comment";
					payment.RelatedPaymentApproval.AV_Amount = 258M;
					Assert(paymentForm.CheckEPayRateButton_ForTestOnly.Enabled);
					Factory.Save();

					UnitTestUserNotification.Instance.ClearMessages();
					paymentForm.CheckEPayRateButton_ForTestOnly.PerformClick();

					var quotes = Factory.Load<AccEPaymentQuote>(new ZQuery(AccEPaymentQuoteSchema.QU_AV, payment.RelatedPaymentApproval.PK));
					AssertEquals("Number of quotes", 2, quotes.Length);

					var oldQuote = quotes.First(q => q.PK == quote.PK);
					AssertEquals("Post-condition - old quote is discarded", QuoteStatusCodes.Discarded, oldQuote.QU_Status);

					var newQuote = quotes.First(q => q.PK != quote.PK);
					AssertNotNull("Post-condition - a new quote is created for payment", newQuote.IsInDatabase);
					AssertEquals("OFX", newQuote.QU_ProviderCode);
					AssertEquals(258M, newQuote.QU_ToAmount);
					AssertEquals("USD", newQuote.QU_RX_NKToCurrency);
					AssertEquals(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, newQuote.QU_RX_NKFromCurrency);
					AssertEquals(GlbCompany.CurrentCompany.PK, newQuote.QU_GC);
				}
			}
		}

		public void TestRefreshQuotesButton_Click()
		{
			SetupDataForTest();
			var payment = GetNewTestAPPayment(TestOrg, TestBank, ReceiptTypes.Cheque, TestCheques, "2", 94M, CurrencyCodes.UnitedStates, true);
			var quote = payment.RelatedPaymentApproval.TryToCreateQuote(EPaymentProviderCodes.Codes.OFX).Quote;
			quote.QU_Status = QuoteStatusCodes.Discarded;
			quote.QU_FromAmount = quote.QU_ToAmount = 200M;
			quote.QU_LastResponseReceivedUtc = ZDateTime.Now;
			quote.QU_ExchangeRate = quote.QU_ExchangeRateInverted = 1;
			Factory.Save();

			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			{
				payment.RelatedPaymentApproval.AV_Status = PaymentApprovalStatus.AwaitingApproval;
				using (var paymentForm = new PaymentForm(payment))
				{
					paymentForm.Show();
					AssertEquals("Preconditon - 1 quote in DB", 1, payment.PaymentQuotes.Count);
					payment.RelatedPaymentApproval.TryToCreateQuote(EPaymentProviderCodes.Codes.OFX);
					Factory.Save();

					AssertEquals("PaymentQuotes collection gets updated as a new quote is added to it", 1, payment.PaymentQuotes.Count);
					paymentForm.RefreshButton_ForTestonly(null ,null);
					AssertEquals("Expect clicking the refresh button will update the quotes collection", 2, payment.PaymentQuotes.Count);
				}
			}
		}

		public void TestEPaymentControlsVisibilityDependsOnRegistry()
		{
			SetupDataForTest();
			AssertEPaymentControlsVisibility(true);
			AssertEPaymentControlsVisibility(false);
		}

		void AssertEPaymentControlsVisibility(bool isOFXEPaymentEnabled)
		{
			var configurationCollection = new EPaymentConfigurationCollection();
			var config = configurationCollection.AddNew();
			config.CountryCode = GlbCompany.CurrentCompany.Country.RN_Code;
			config.CountryDescription = GlbCompany.CurrentCompany.Country.RN_Desc;
			config.OFXEPaymentEnabled = isOFXEPaymentEnabled;

			var payment = GetNewTestAPPayment(TestOrg, TestBank, ReceiptTypes.Cheque, TestCheques, "2", 94M, CurrencyCodes.UnitedStates, true);

			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, isOFXEPaymentEnabled))
			{
				using (var paymentForm = new PaymentForm(payment))
				{
					paymentForm.Show();
					if (isOFXEPaymentEnabled)
					{
						Assert("CheckEPayRateButton should be visible when E-Payment functionality is enabled.", paymentForm.CheckEPayRateButton_ForTestOnly.Visible);
						Assert("ProcessEPaymentButton should be visible when E-Payment functionality is enabled.", paymentForm.ProcessEPaymentButton_ForTestOnly.Visible);
						Assert("EPaymentTabPage should be visible when E-Payment functionality is enabled.", paymentForm.EPaymentTabPage_ForTestOnly.TabVisible);
					}
					else
					{
						Assert("CheckEPayRateButton should be invisible when E-Payment functionality is disabled.", !paymentForm.CheckEPayRateButton_ForTestOnly.Visible);
						Assert("ProcessEPaymentButton should be invisible when E-Payment functionality is disabled.", !paymentForm.ProcessEPaymentButton_ForTestOnly.Visible);
						Assert("EPaymentTabPage should be invisible when E-Payment functionality is disabled.", !paymentForm.EPaymentTabPage_ForTestOnly.TabVisible);
					}
				}
			}
		}

		public void TestCheckEPaymentExchangeRateButtonEnablenessDependsOnStatus()
		{
			SetupDataForTest();
			var payment = GetNewTestAPPayment(TestOrg, TestBank, ReceiptTypes.Cheque, TestCheques, "2", 94M, CurrencyCodes.UnitedStates, true);
			Factory.Save();
			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			{
				var supportedStatusList = new ZString[] { PaymentApprovalStatus.AwaitingApproval, PaymentApprovalStatus.FullyApproved, PaymentApprovalStatus.Rejected, PaymentApprovalStatus.Draft };
				var unSupportedStatusList = new ZString[] { PaymentApprovalStatus.Cancelled, PaymentApprovalStatus.Posted };
				foreach (var status in supportedStatusList)
				{
					payment.RelatedPaymentApproval.AV_Status = status;
					using (var paymentForm = new PaymentForm(payment))
					{
						paymentForm.Show();
						Assert(paymentForm.CheckEPayRateButton_ForTestOnly.Enabled);
					}
				}
				foreach (var status in unSupportedStatusList)
				{
					payment.RelatedPaymentApproval.AV_Status = status;
					using (var paymentForm = new PaymentForm(payment))
					{
						paymentForm.Show();
						Assert(!paymentForm.CheckEPayRateButton_ForTestOnly.Enabled);
					}
				}
			}
		}

		public void TestCheckEPaymentExchangeRateButtonIsEnabledForNewApproval()
		{
			SetupDataForTest();
			var payment = GetNewTestAPPayment(TestOrg, TestBank, ReceiptTypes.Cheque, TestCheques, "2", 94M, CurrencyCodes.UnitedStates, true);
			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			{
				using (var paymentForm = new PaymentForm(payment))
				{
					paymentForm.Show();
					Assert("button should be enabled when approval is not saved", paymentForm.CheckEPayRateButton_ForTestOnly.Enabled);
				}

				Factory.Save();
				using (var paymentForm = new PaymentForm(payment))
				{
					paymentForm.Show();
					Assert("button should be enabled when approval is saved", paymentForm.CheckEPayRateButton_ForTestOnly.Enabled);
				}
			}
		}

		public void TestCheckEPaymentExchangeRateButtonEnablenessDependsOnCurrency()
		{
			SetupDataForTest();
			var payment = GetNewTestAPPayment(TestOrg, TestBank, ReceiptTypes.Cheque, TestCheques, "2", 94M, CurrencyCodes.Australia, true);
			Factory.Save();
			AssertEquals(payment.RelatedPaymentApproval.CurrencyCode, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);

			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			{
				using (var paymentForm = new PaymentForm(payment))
				{
					paymentForm.Show();
					Assert("button should be disabled when payment currency is same as local currency", !paymentForm.CheckEPayRateButton_ForTestOnly.Enabled);

					payment.RelatedPaymentApproval.AV_RX_NKPaymentCurrency = "EUR";
					AssertNotEquals(payment.RelatedPaymentApproval.AV_RX_NKPaymentCurrency, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
					Assert("button should be enabled when payment currency is different to local currency", paymentForm.CheckEPayRateButton_ForTestOnly.Enabled);

					payment.RelatedPaymentApproval.AV_RX_NKPaymentCurrency = "AUD";
					AssertEquals(payment.RelatedPaymentApproval.AV_RX_NKPaymentCurrency, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
					Assert("button should be disabled when payment currency is same as local currency", !paymentForm.CheckEPayRateButton_ForTestOnly.Enabled);
				}
			}
		}

		#endregion

		public void TestCheckEPaymentExchangeRateButtonClick_HasExistingRequestedQuote()
		{
			SetupDataForTest();
			var payment = GetNewTestAPPayment(TestOrg, TestBank, ReceiptTypes.Cheque, TestCheques, "2", 94M, CurrencyCodes.UnitedStates, true);
			payment.RelatedPaymentApproval.AV_Status = PaymentApprovalStatus.AwaitingApproval;
			var rquote = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Requested, payment.RelatedPaymentApproval);
			Factory.Save();

			using (var paymentForm = new PaymentForm(payment))
			{
				paymentForm.Show();
				Assert(paymentForm.CheckEPayRateButton_ForTestOnly.Enabled);

				UnitTestUserNotification.Instance.ClearMessages();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				paymentForm.CheckEPayRateButton_ForTestOnly.PerformClick();
				AssertEquals("E-Quote already requested. Response may take up to several minutes to be received. If you generate a new request, then the previous request will be discarded. Are you sure you want to proceed?", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessages();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				paymentForm.CheckEPayRateButton_ForTestOnly.PerformClick();

				AssertEquals("E-Quote request generated to service provider OFX. Response may take from a few moments up to several minutes to be received.", UnitTestUserNotification.Instance.LastMessage.Text);

				AssertEquals(2, payment.PaymentQuotes.Count);
				AssertEquals(1, payment.PaymentQuotes.Cast<EPaymentQuoteForDisplay>().Count(x => x.RealQuote.QU_Status == QuoteStatusCodes.Queued));
				AssertEquals(1, payment.PaymentQuotes.Cast<EPaymentQuoteForDisplay>().Count(x => x.RealQuote.QU_Status == QuoteStatusCodes.Discarded));
			}
		}

		public void TestFormProperties()
		{
			SetupDataForTest();
			var payment = GetNewTestAPPayment(TestOrg, TestBank, ReceiptTypes.Cheque, TestCheques, "2", 94M, CurrencyCodes.UnitedStates, true);
			payment.RelatedPaymentApproval.AV_Status = PaymentApprovalStatus.AwaitingApproval;
			Factory.Save();

			using (var paymentForm = new PaymentForm(payment))
			{
				paymentForm.Show();
				AssertEquals("Refresh", paymentForm.RefreshButton_ForTestOnly.CaptionResourceString.Caption);
			}
		}

		public void TestQuoteWarningMessage()
		{
			SetupDataForTest();
			var payment = GetNewTestAPPayment(TestOrg, TestBank, ReceiptTypes.Cheque, TestCheques, "2", 94M, CurrencyCodes.UnitedStates, true);
			payment.RelatedPaymentApproval.AV_Status = PaymentApprovalStatus.AwaitingApproval;
			Factory.Save();
			var quote1 = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Received, payment.RelatedPaymentApproval);
			quote1.QU_ProviderReference = string.Empty;
			var quote2 = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Received, payment.RelatedPaymentApproval);
			Factory.Save();
			payment.RelatedPaymentApproval.PaymentQuotes.Reload(true);

			using (var paymentForm = new PaymentForm(payment))
			{
				paymentForm.Show();
				Application.DoEvents();

				var bankAndEPaymentTabControl = paymentForm.Controls.Find("BankAndEPaymentTabControl", true).First() as ZTabControl;
				AssertNotNull(bankAndEPaymentTabControl);
				bankAndEPaymentTabControl.SelectNextTabPage();

				var epaymentGrid = paymentForm.Controls.Find("EPaymentGrid", true).First() as ZGrid;
				AssertNotNull(epaymentGrid);
				epaymentGrid.Select(0);
				AssertEquals(1, epaymentGrid.SelectedElements.Length);

				var selectedQuote = epaymentGrid.SelectedElements[0] as EPaymentQuoteForDisplay;
				Assert(selectedQuote.FeeAmountInfo.HasWarnings());
				var warnings = selectedQuote.FeeAmountInfo.GetWarnings();
				AssertEquals(1, warnings.Count());
				AssertEquals("This is indicative rate only. Processing Fee may be applied on the formal quote. To request a formal quote, please select an OFX E-Payment Account as the Bank Account for this payment, and ensure you have authorized your OFX User Account.", warnings.First().Message);
				epaymentGrid.UnSelectAll();
				epaymentGrid.Select(1);
				selectedQuote = epaymentGrid.SelectedElements[0] as EPaymentQuoteForDisplay;
				Assert(!selectedQuote.FeeAmountInfo.HasWarnings());
			}
		}

		public void TestPaymentFormProperties()
		{
			SetupDataForTest();
			var payment = GetNewTestAPPayment(TestOrg, TestBank, ReceiptTypes.Cheque, TestCheques, "2", 94M, CurrencyCodes.UnitedStates, true);
			payment.RelatedPaymentApproval.TryToCreateQuote(EPaymentProviderCodes.Codes.OFX);
			Factory.Save();
			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			{
				using (var paymentForm = new PaymentForm(payment))
				{
					paymentForm.Show();
					ZTabControl bankAndEPaymentTabControl = (ZTabControl)paymentForm.Controls.Find("BankAndEPaymentTabControl", true).First();
					var ePaymentTab = bankAndEPaymentTabControl.GetTabPage("EPaymentTabPage");
					AssertNotNull(ePaymentTab);

					// Asserts to make sure Payment form is visible properly on different screen resolutions with minimum acceptable resolution being 1366 x 768
					Assert(ePaymentTab.AutoScroll);
					ZTabControl tabControl = (ZTabControl)paymentForm.Controls.Find("TabControl", true).First();
					AssertEquals(DockStyle.Fill, tabControl.Dock);
					ZGroupBox eQuoteGroupBox = (ZGroupBox)paymentForm.Controls.Find("EQuoteGroupBox", true).First();
					AssertEquals(900, eQuoteGroupBox.Width);
					ZGroupBox dealGroupBox = (ZGroupBox)paymentForm.Controls.Find("DealGroupBox", true).First();
					AssertEquals(900, dealGroupBox.Width);
					ZGrid ePaymentGrid = (ZGrid)paymentForm.Controls.Find("EPaymentGrid", true).First();
					AssertEquals(895, ePaymentGrid.Width);
				}
			}
		}

		public void TestMatchingFormPostSuccessful()
		{
			SetupDataForTest();

			ForwardingShipment shipment = TestObjectCreator.CreateShipment("S1");
			Job job = TestObjectCreator.CreateJob(shipment);

			APInvoice testAPInv = Factory.NewWithValidTestData<APInvoice>();
			testAPInv.AH_OH = TestOrg.PK;
			ZGuid genericChargeCodeGuid = TestObjectCreator.CC1.PK;

			InvoicingLineBase line = TestObjectCreator.CreateInvoiceLine(testAPInv, testAPInv.TransactionCurrency, testAPInv.AH_ExchangeRate, 94m, 0m, 0m, 94m, 0m, 0m, genericChargeCodeGuid);
			line.AL_JH = job.PK;
			TestObjectCreator.CreateJobCharge(line, job, TestObjectCreator.CC1, testAPInv.TransactionCurrency);
			testAPInv.AH_FullyPaidDate = ZDateTime.Empty;

			Factory.Save();

			APPayment testAPPay = GetNewTestAPPayment(TestOrg, TestBank, ZArchitecture.Core.ReceiptTypes.Cheque,
				TestCheques, "2", 94M);

			using (MockPaymentForm payForm = new MockPaymentForm(testAPPay))
			{
				NewMatchGroupForm matchingForm = null;
				PaymentDocumentsPrintPopup exposedPrintForm = null;

				try
				{
					payForm.FireSaveButton();	// user clicks payment detail button

					matchingForm = ZFormModaliser.ActiveForm as NewMatchGroupForm;
					AssertNotNull("The active form should be the matching form", matchingForm);
					APMatchingBase matchingBizO = matchingForm.BusinessEntity as APMatchingBase;
					Assert("OSPartialPaymentAmount should be readonly", ((IMatching)testAPPay).OSPartialPaymentAmountInfo.ReadOnly);

					matchingBizO.MoveAllFromUnmatchToMatch();

					AssertNotNull("Matching Object should be APMatchingBase", matchingBizO);
					AssertEquals("Should be 2 transactions in MatchedTransactions", 2, matchingBizO.MatchedTransactions.Count);
					AssertEquals("Balance should be 0", 0M, matchingBizO.Balance);

					matchingForm.FireSaveButton();	// Note: this does not post the matching session

					BusinessObjectFactory emptyFactory = new BusinessObjectFactory();
					TransactionMatchLinkCollection matchLinks = new TransactionMatchLinkCollection(emptyFactory);
					matchLinks.Load();
					AssertEquals("No Matchlinks should be posted to DB yet since posting happens when MatchingForm is closed", 0, matchLinks.Count);

					matchingForm.Close();

					matchLinks.Load();
					AssertEquals("2 Matchlinks should be posted to DB when matching form is closed", 2, matchLinks.Count);
					ZQuery invMatchFilter = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, testAPInv.PK);
					TransactionMatchLink invMatch = emptyFactory.LoadTop1<TransactionMatchLink>(invMatchFilter);
					AssertNotNull("There should be a matchlink for the invoice", invMatch);

					AssertEquals("DisplayMode of Original Paymentform should be Browse", ZArchitecture.Core.ODisplayMode.Browse,
						payForm.DisplayMode);
					AssertEquals("Text on PostButton should be New", ZFormPostingButtonsStrategy.NewButtonText(payForm).Text, payForm.PaymentDetailButton_ForTestOnly.Text);

					// check that remittance form pops up
					exposedPrintForm = ((PaymentPrintManager.TestPaymentPrintManager)payForm.PrintManager_ForTestOnly).LastPrintForm;
					AssertNotNull("The remittance advice print form should not be null because it should have been shown", exposedPrintForm);
				}
				finally
				{
					if (matchingForm != null)
					{
						matchingForm.Dispose();
					}
					if (exposedPrintForm != null)
					{
						exposedPrintForm.Dispose();
					}
				}
			}
		}

		public void TestMatchingFormNotPosted()
		{
			SetupDataForTest();

			ARPayment testARPay = Factory.NewWithValidTestData<ARPayment>();
			testARPay.AH_OH = TestOrg.PK;
			testARPay.AH_AB = TestBank.PK;
			testARPay.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cash;
			testARPay.AH_ChequeOrReference = "3";
			testARPay.AH_OSExTaxAmount = 46M;

			using (PaymentForm testPayForm = new PaymentForm(testARPay))
			{
				NewMatchGroupForm matchingForm = null;

				try
				{
					testPayForm.FireSaveButton();	// user clicks payment detail
					testPayForm.Show();
					Application.DoEvents();

					matchingForm = ZFormModaliser.ActiveForm as NewMatchGroupForm;

					AssertNotNull("Active form should be the NewMatchGroupForm", matchingForm);
					ARMatchingBase matchingBizO = matchingForm.BusinessEntity as ARMatchingBase;
					AssertNotNull("MatchingBizO should have type of ARMatchingBase", matchingBizO);

					AssertEquals("The payment should be one of the transactions selected for matching", 1, matchingBizO.MatchedTransactions.Count);
					Assert("The payment should be one of the transactions selected for matching", matchingBizO.MatchedTransactions.Contains(testARPay));

					matchingForm.Close();	// this should not post the transactions

					Assert("The form should still be writeable", !testARPay.ReadOnly);

					TransactionMatchLinkCollection matchlinks = new TransactionMatchLinkCollection(Factory);
					matchlinks.Load();
					AssertEquals("There should be no matchlinks in the DB", 0, matchlinks.Count);
				}
				finally
				{
					if (matchingForm != null)
					{
						matchingForm.Dispose();
					}
				}
			}
		}

		public void TestShowPreSaveDialogs()
		{
			OrgHeader testOrg = Factory.NewWithValidTestData<OrgHeader>();
			testOrg.OH_IsCreditor = true;
			Factory.Save();
			AccBankAccount testBank = Factory.NewWithValidTestData<AccBankAccount>();
			Factory.Save();
			AccChequeBook testCheques = Factory.NewWithValidTestData<AccChequeBook>();
			testCheques.AK_StartNo = 1;
			testCheques.AK_LastNo = 100;
			testCheques.AK_CurrentNo = 1;

			Factory.Save();

			APPayment aPPay = GetNewTestAPPayment(testOrg, testBank, ZArchitecture.Core.ReceiptTypes.Cheque,
				testCheques, "2", 94M);
			Assert("Payment should not have errors", !aPPay.HasErrors);

			using (PaymentForm payForm = new PaymentForm(aPPay)) // test popping up matching form
			{
				NewMatchGroupForm matchingForm = null;

				try
				{
					Assert("Precondition: CalledFromMatchingFormClosing_ForTestOnly should be false", !payForm.CalledFromMatchingFormClosing_ForTestOnly);
					ContinueWithSave continueResult = payForm.ShowPreSaveDialogs_ForTestOnly();
					AssertEquals("Should not continue since matching form is shown", ContinueWithSave.No, continueResult);
					Assert("CalledFromMatchingFormClosing_ForTestOnly should be set to true", payForm.CalledFromMatchingFormClosing_ForTestOnly);
					matchingForm = ZFormModaliser.ActiveForm as NewMatchGroupForm;
					AssertNotNull("Matching form should be the active form", matchingForm);
					matchingForm.Close();
				}
				finally
				{
					if (matchingForm != null)
					{
						matchingForm.Dispose();
					}
				}
			}

			using (PaymentForm payForm = new PaymentForm(aPPay))
			{
				Assert("Precondition: CalledFromMatchingFormClosing_ForTestOnly should be false", !payForm.CalledFromMatchingFormClosing_ForTestOnly);
				payForm.SetCalledFromMatchingFormClosingTrue_ForTestOnly();
				ContinueWithSave continueResult = payForm.ShowPreSaveDialogs_ForTestOnly();
				AssertEquals("Should continue since matching was completed", ContinueWithSave.Yes, continueResult);
				Assert("CalledFromMatchingFormClosing_ForTestOnly should be set to false", !payForm.CalledFromMatchingFormClosing_ForTestOnly);
			}
		}

		public void TestClosingMatchingFormDeletesMiscTrans()
		{
			SetupDataForTest();

			APPayment testAPPay = GetNewTestAPPayment(TestOrg, TestBank, ZArchitecture.Core.ReceiptTypes.Cheque,
				TestCheques, "2", 94M);

			using (PaymentForm testPayForm = new PaymentForm(testAPPay))
			{
				NewMatchGroupForm matchingForm = null;
				try
				{
					testPayForm.FireSaveButton();
					matchingForm = ZFormModaliser.ActiveForm as NewMatchGroupForm;
					AssertNotNull("Active form should be the NewMatchGroupForm", matchingForm);

					MatchingBase currentMatchingBizO = (MatchingBase)matchingForm.BusinessEntity;
					TransactionHeader miscTrans = currentMatchingBizO.GetMiscellaneousTransaction(ZArchitecture.Core.TransactionTypes.Discount);
					currentMatchingBizO.AddMiscellaneousTransaction(miscTrans);
					AssertEquals("There should be 2 transactions selected for matching - Discount and Payment", 2,
						currentMatchingBizO.MatchedTransactions.Count);
					Assert("Discount should be selected", currentMatchingBizO.MatchedTransactions.Contains(miscTrans));

					matchingForm.CancelButton.PerformClick();

					Assert("Payment form should not be read only", !((BusinessObject)testPayForm.BusinessEntity).ReadOnly);

					testPayForm.FireSaveButton();
					matchingForm = ZFormModaliser.ActiveForm as NewMatchGroupForm;
					AssertNotNull("Active form should be the NewMatchGroupForm", matchingForm);

					currentMatchingBizO = (MatchingBase)matchingForm.BusinessEntity;
					AssertEquals("There should be no Outstanding Transactions", 0, currentMatchingBizO.UnmatchedTransactions.Count);
				}
				finally
				{
					if (matchingForm != null)
					{
						matchingForm.Dispose();
					}
				}
			}
		}

		public void TestClosingMatchingFormRemovesPaymentFromCollections()
		{
			SetupDataForTest();
			APPayment aPPay = GetNewTestAPPayment(TestOrg, TestBank, ZArchitecture.Core.ReceiptTypes.Cheque,
				TestCheques, "2", 94M);
			using (PaymentForm payForm = new PaymentForm(aPPay))
			{
				payForm.Show();
				payForm.PaymentDetailButton_ForTestOnly.PerformClick();
				NewMatchGroupForm matchingForm = ZFormModaliser.ActiveForm as NewMatchGroupForm;
				AssertNotNull("MatchingForm should be displayed", matchingForm);

				MatchingBase matchingBizO = matchingForm.BusinessEntity as MatchingBase;
				Assert("MatchedTransactions contains the payment", matchingBizO.MatchedTransactions.Contains(aPPay));
				Assert("MustBeMatched in MatchedTransactions contains the payment", matchingBizO.MatchedTransactions.MustTransactionBeMatched(aPPay));

				matchingForm.Close();

				Assert("MatchedTransactions does not contain the payment", !matchingBizO.MatchedTransactions.Contains(aPPay));
				Assert("MustBeMatched in MatchedTransactions does not contain the payment", !matchingBizO.MatchedTransactions.MustTransactionBeMatched(aPPay));
			}
		}

		public void TestDisplayModeChanged()
		{
			APPayment testAPPay = Factory.NewWithValidTestData<APPayment>();
			Factory.Save();
			using (PaymentForm deleteForm = new PaymentForm(testAPPay))
			{
				deleteForm.DisplayMode = ODisplayMode.Delete;
				Assert("PaymentDetail button should be enabled", deleteForm.PaymentDetailButton_ForTestOnly.Enabled);
				Assert("Cancel button should be enabled", deleteForm.CloseButton_ForTestOnly.Enabled);
			}
		}

		public void TestHideChequeControl()
		{
			var testBank = Factory.NewWithValidTestData<AccBankAccount>();
			testBank.AB_ChequeNumDigits = (ZByte)5;
			testBank.AB_GC = GlbCompany.CurrentCompany.PK;
			var testChequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			testChequeBook.AK_AB = testBank.PK;
			testChequeBook.AK_StartNo = 1;
			testChequeBook.AK_LastNo = 100;
			testChequeBook.AK_CurrentNo = 34;
			Factory.Save();

			var testAPPay = Factory.NewWithValidTestData<APPayment>();

			using (PaymentForm newForm = new PaymentForm(testAPPay))
			{
				newForm.DisplayMode = ODisplayMode.ReadOnly;
				newForm.Show();
				Assert("ChequeBookGuidFindBox_ForTestOnly should be invisible", !newForm.ChequeBookGuidFindBox_ForTestOnly.Visible);

				testAPPay.AH_AB = testBank.PK;
				testAPPay.ChequeBook = testChequeBook.PK;
			}
			Factory.Save();

			using (PaymentForm newForm = new PaymentForm(testAPPay))
			{
				newForm.DisplayMode = ODisplayMode.Browse;
				newForm.Show();
				Assert("ChequeBookGuidFindBox_ForTestOnly should be visible", newForm.ChequeBookGuidFindBox_ForTestOnly.Visible);
			}
		}

		public void TestOnLoad_CloseButtonText()
		{
			APPayment testAPPay = Factory.NewWithValidTestData<APPayment>();
			Factory.Save();

			using (PaymentForm newForm = new PaymentForm(testAPPay))
			{
				newForm.DisplayMode = ODisplayMode.Browse;
				newForm.Show();
				AssertEquals("CloseButton_ForTestOnly.Text", "Close", newForm.CloseButton_ForTestOnly.Text);
			}

			using (PaymentForm newForm = new PaymentForm(testAPPay))
			{
				newForm.DisplayMode = ODisplayMode.Delete;
				newForm.Show();
				AssertEquals("CloseButton_ForTestOnly.Text", ZFormPostingButtonsStrategy.CancelButtonText(newForm).Text, newForm.CloseButton_ForTestOnly.Text);
			}
		}

		public void TestPrintManager()
		{
			APPayment aPPay = Factory.NewWithValidTestData<APPayment>();
			using (PaymentForm payForm = new PaymentForm(aPPay))
			{
				AssertNotNull("PaymentPrintManager_ForTestOnly should exist", payForm.PrintManager_ForTestOnly);
			}
		}

		public void TestCancellingMatchSessionMakesChequeNumberEditable()
		{
			SetupDataForTest();
			APPayment aPPay = GetNewTestAPPayment(TestOrg, TestBank, ZArchitecture.Core.ReceiptTypes.Cheque,
				TestCheques, "2", 23M);

			using (PaymentForm payForm = new PaymentForm(aPPay))
			{
				payForm.FireSaveButton();
				NewMatchGroupForm matchingForm = ZFormModaliser.ActiveForm as NewMatchGroupForm;
				Assert("Precondition: AH_ChequeOrReference should not be editable", aPPay.AH_ChequeOrReferenceInfo.ReadOnly);
				Assert("Precondition: ChequeOrReference should not be editable", ((IMatching)aPPay).ChequeOrReferenceInfo.ReadOnly);
				matchingForm.Close();
				Assert("AH_ChequeOrReference should be editable", !aPPay.AH_ChequeOrReferenceInfo.ReadOnly);
			}
		}

		public void TestReferenceNumberLabel()
		{
			using (PaymentForm form = (PaymentForm)GetFormToBashCore())
			{
				AssertEquals("Text on ChequeNumberLabel should be 'Reference No.'", "Reference No.", "Reference No.");
			}
		}

		public void TestUnmatchDateControl()
		{
			var payment = Factory.NewWithValidTestData<APPayment>();
			Factory.Save();

			using (var form = new MockPaymentForm(payment))
			{
				payment.UnmatchingData.InitializaReverseTransactionData(new DummyUnmatchingData() { MaxMatchDate = ZDateTime.Now, AllowBackPosting = true });
				AssertEquals("Precondition: MinUnmatchDate value", true, payment.UnmatchingData.WasUnmatched);
				form.Show();
				AssertEquals("Precondition: UnmatchDateInfo_ReadOnly", false, payment.UnmatchDate_ReadOnly);
				AssertEquals("Precondition: No. of Documents should have correct Top position.", form.PostDateEditExposed.Top, form.AH_NumberOfSupportingDocumentsCalcEditExposed.Top);
				AssertEquals("Precondition: Unmatch Date should have correct Top position.", form.InvoiceDateEditExposed.Top, form.UnmatchDateEditExposed.Top);
				AssertEquals("Precondition: No. of Documents should not be visible", false, form.AH_NumberOfSupportingDocumentsCalcEditExposed.Visible);
				AssertEquals("UnmatchDateEditExposed.Visible", true, form.UnmatchDateEditExposed.Visible);
				AssertNoExceptionThrown("Control 'Unmatch Date' should pass CheckControlPosition", () => { new PositionChecker(this).CheckControlPosition(form.UnmatchDateEditExposed); });
			}

			using (var form = new MockPaymentForm(payment))
			{
				payment.UnmatchingData.InitializaReverseTransactionData(new DummyUnmatchingData() { MaxMatchDate = ZDateTime.Empty, AllowBackPosting = true });
				AssertEquals("Precondition: MinUnmatchDate value", false, payment.UnmatchingData.WasUnmatched);
				form.Show();
				AssertEquals("Precondition: UnmatchDateInfo_ReadOnly", true, payment.UnmatchDate_ReadOnly);
				AssertEquals("Precondition: No. of Documents should not be visible", false, form.AH_NumberOfSupportingDocumentsCalcEditExposed.Visible);
				AssertEquals("Precondition: Unmatch Date should have correct Top position.", form.InvoiceDateEditExposed.Top, form.UnmatchDateEditExposed.Top);
				AssertEquals("UnmatchDateEditExposed.Visible", false, form.UnmatchDateEditExposed.Visible);
			}

			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.China);

			using (var form = new MockPaymentForm(payment))
			{
				int initialUnmatchDateLeftPosition = form.UnmatchDateEditExposed.Left;
				AssertEquals("Precondition: UnmatchDateEdit should have correct initial Left position", true,
					form.PostDateEditExposed.Left < initialUnmatchDateLeftPosition && initialUnmatchDateLeftPosition < form.AH_NumberOfSupportingDocumentsCalcEditExposed.Left);
				payment.UnmatchingData.InitializaReverseTransactionData(new DummyUnmatchingData() { MaxMatchDate = ZDateTime.Now, AllowBackPosting = true });
				AssertEquals("Precondition: MinUnmatchDate value", true, payment.UnmatchingData.WasUnmatched);
				form.Show();
				AssertEquals("Precondition: UnmatchDateInfo_ReadOnly", false, payment.UnmatchDate_ReadOnly);
				AssertEquals("Precondition: No. of Documents should have correct Top position.", form.PostDateEditExposed.Top, form.AH_NumberOfSupportingDocumentsCalcEditExposed.Top);
				AssertEquals("Precondition: Unmatch Date should have correct Top position.", form.InvoiceDateEditExposed.Top, form.UnmatchDateEditExposed.Top);
				AssertEquals("Precondition: No. of Documents should be visible for China", true, form.AH_NumberOfSupportingDocumentsCalcEditExposed.Visible);
				AssertEquals("UnmatchDateEditExposed.Visible", true, form.UnmatchDateEditExposed.Visible);
				AssertEquals("No. of Documents should have correct Right position.", form.PaymentNoTextBoxExposed.Right, form.AH_NumberOfSupportingDocumentsCalcEditExposed.Right);
				AssertNoExceptionThrown("Control 'Unmatch Date' should pass CheckControlPosition", () => { new PositionChecker(this).CheckControlPosition(form.UnmatchDateEditExposed); });
				AssertNoExceptionThrown("Control 'No. of Documents' should pass CheckControlPosition", () => { new PositionChecker(this).CheckControlPosition(form.AH_NumberOfSupportingDocumentsCalcEditExposed); });
			}

			using (var form = new MockPaymentForm(payment))
			{
				payment.UnmatchingData.InitializaReverseTransactionData(new DummyUnmatchingData() { MaxMatchDate = ZDateTime.Empty, AllowBackPosting = true });
				AssertEquals("Precondition: MinUnmatchDate value", false, payment.UnmatchingData.WasUnmatched);
				form.Show();
				AssertEquals("Precondition: UnmatchDateInfo_ReadOnly", true, payment.UnmatchDate_ReadOnly);
				AssertEquals("Precondition: No. of Documents should have correct Top position.", form.PostDateEditExposed.Top, form.AH_NumberOfSupportingDocumentsCalcEditExposed.Top);
				AssertEquals("Precondition: No. of Documents should be visible for China", true, form.AH_NumberOfSupportingDocumentsCalcEditExposed.Visible);
				AssertEquals("UnmatchDateEditExposed.Visible", false, form.UnmatchDateEditExposed.Visible);
				AssertNoExceptionThrown("Control 'No. of Documents' should pass CheckControlPosition", () => { new PositionChecker(this).CheckControlPosition(form.AH_NumberOfSupportingDocumentsCalcEditExposed); });
			}
		}

		public void TestAddressesOnPayments()
		{
			AccountingConfigurationRegistry.Instance.EditPaymentAddress.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			using (var form = (PaymentForm)GetFormToBashCore())
			{
				form.Show();
				AssertEquals(true, form.Payment_ForTestOnly.DisplayInvoiceAddressOverrideInfo.ReadOnly);
				AssertEquals(true, form.Payment_ForTestOnly.DisplayInvoiceContactOverrideInfo.ReadOnly);
			}

			AccountingConfigurationRegistry.Instance.EditPaymentAddress.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			using (var form = (PaymentForm)GetFormToBashCore())
			{
				form.Show();
				AssertEquals(true, form.Payment_ForTestOnly.DisplayInvoiceAddressOverrideInfo.ReadOnly);
				AssertEquals(true, form.Payment_ForTestOnly.DisplayInvoiceContactOverrideInfo.ReadOnly);
			}
		}

		public void TestWorkflowTabPage()
		{
			using (var form = GetFormToBashCore())
			{
				form.Show();
				var tabControl = form.FindAll<ZTemplateTabControl>().Single();
				AssertNotEquals("WorkflowTabPage Visible", -1, tabControl.TabPages.IndexOfKey("WorkflowTabPage"));
			}
		}

		public void TestAPPaymentFormBorderStyle()
		{
			var apPayment = Factory.NewWithValidTestData<APPayment>();

			using (var form = new PaymentForm(apPayment))
			{
				AssertEquals("Should be the default value", FormBorderStyle.Sizable, form.FormBorderStyle);
			}
		}

		public void TestARPaymentFormBorderStyle()
		{
			var arPayment = Factory.NewWithValidTestData<ARPayment>();

			using (var form = new PaymentForm(arPayment))
			{
				AssertEquals("Should be the default value", FormBorderStyle.Sizable, form.FormBorderStyle);
			}
		}

		#region Implementation

		OrgHeader TestOrg;
		AccountingPeriodTestHelper PeriodHelper;
		AccBankAccount TestBank;
		AccChequeBook TestCheques;

		protected override Form GetFormToBashCore()
		{
			var testPayment = Factory.New<ARPayment>();
			var result = new PaymentForm(testPayment);
			result.ControllerID = ControllerIDs.ZARPayment;
			return result;
		}

		protected override bool ShouldHaveAuditPlugIn => true;

		void SetupDataForTest()
		{
			PeriodHelper = new AccountingPeriodTestHelper(Factory);
			PeriodHelper.SetupPeriods();

			TestOrg = Factory.NewWithValidTestData<OrgHeader>();
			TestOrg.OH_IsCreditor = true;
			TestOrg.CompanyData.SetAPTaxApplicable(false);
			TestOrg.OH_IsDebtor = true;
			Factory.Save();

			TestBank = Factory.NewWithValidTestData<AccBankAccount>();
			TestCheques = Factory.NewWithValidTestData<AccChequeBook>();
			TestCheques.AK_StartNo = 1;
			TestCheques.AK_LastNo = 100;
			TestCheques.AK_CurrentNo = 1;
			Factory.Save();
		}

		APPayment GetNewTestAPPayment(OrgHeader org, AccBankAccount bank, ZString receiptType,
			AccChequeBook chequeBook, ZString chequeOrRef, ZDecimal amount, string currency = "AUD", bool attachApproval = false)
		{
			APPayment aPPay = Factory.NewWithValidTestData<APPayment>();
			aPPay.AH_OH = org.PK;
			aPPay.AH_AB = bank.PK;
			aPPay.AH_ReceiptType = receiptType;
			aPPay.ChequeBook = chequeBook.PK;
			aPPay.AH_ChequeOrReference = chequeOrRef;
			aPPay.AH_RX_NKTransactionCurrency = currency;
			aPPay.AH_ExchangeRate = 1;
			aPPay.AH_OSExTaxAmount = amount;
			if (attachApproval)
			{
				var paymentApproval = Factory.New<APPaymentApprovalWithAuthorisation>();
				paymentApproval.AV_OH = org.PK;
				paymentApproval.AV_AB = bank.PK;
				paymentApproval.AV_PaymentType = receiptType;
				paymentApproval.AV_AK = chequeBook.PK;
				paymentApproval.AV_ChequeOrReference = chequeOrRef;
				paymentApproval.AV_RX_NKPaymentCurrency = currency;
				paymentApproval.AV_PayExRate = 1;
				paymentApproval.AV_Amount = amount;
				paymentApproval.AV_AH = aPPay.PK;
			}
			return aPPay;
		}

		#endregion

		public class MockPaymentForm : PaymentForm
		{
			public MockPaymentForm(Business.ARAP.ReceiptPayment.Payment paymentBizO)
				: base(paymentBizO)
			{
			}

			protected override PaymentPrintManager PrintManager
			{
				get
				{
					if (fTestPrintManager == null)
					{
						fTestPrintManager = new PaymentPrintManager.TestPaymentPrintManager(BusinessEntity.Identifier.ToGuid(), ZArchitecture.Core.TransactionTypes.Payment, new BusinessObjectFactory());
					}
					return fTestPrintManager;
				}
			}

			PaymentPrintManager.TestPaymentPrintManager fTestPrintManager;

			public ZDateEdit UnmatchDateEditExposed
			{
				get { return UnmatchDateEdit; }
			}

			public ZDateEdit PostDateEditExposed
			{
				get { return PostDateEdit; }
			}

			public ZDateEdit InvoiceDateEditExposed
			{
				get { return InvoiceDateEdit; }
			}

			public ZTextBox PaymentNoTextBoxExposed
			{
				get { return PaymentNoTextBox; }
			}

			public ZCalcEdit AH_NumberOfSupportingDocumentsCalcEditExposed
			{
				get { return AH_NumberOfSupportingDocumentsCalcEdit; }
			}
		}
	}
}
