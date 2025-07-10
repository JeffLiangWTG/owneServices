using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.Accounting.Business.ARAP.EPayment;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.DataTransfer.eNett_Integration;
using Enterprise.Accounting.DataTransfer.eNett_Integration.Testing;
using Enterprise.Accounting.GUI.ARAP;
using Enterprise.Accounting.GUI.ARAP.Payment;
using Enterprise.Accounting.GUI.ARAP.ReceiptPayment;
using Enterprise.Accounting.GUI.ARAP.TransactionView;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Core.Forms;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Encryption;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using AuthorisationCodes = Enterprise.Registry.Business.AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes;
using DealStatusCodes = Enterprise.MasterFiles.Business.EPaymentStatusCodes.Deal;
using QuoteStatusCodes = Enterprise.MasterFiles.Business.EPaymentStatusCodes.Quote;
using RangeCodes = Enterprise.Registry.Business.AmountBasedMultiLevelAuthorisationRequirement.RangeCodes;

namespace Enterprise.Accounting.GUI.Testing.ARAP.Payment.BatchPosting
{
	[TestedType(typeof(PaymentBatchForm))]
	public class PaymentBatchFormTestCase : ZFormBasherTest
	{
		public void TestFundingBankAccountFindBoxVisibility()
		{
			SetupDataForPostingTest();
			BatchPoster.APB_PaymentType = ReceiptTypes.Cash;

			using (var form = new PaymentBatchForm_ForTestOnly(BatchPoster))
			{
				form.Show();
				BatchPoster.APB_PaymentType = ReceiptTypes.EPayment;

				var fundingBankAccountFindBox = (ZGuidFindBox)form.Controls.Find("FundingBankAccountFindBox", true).FirstOrDefault();
				var fundingCurrencyCodeFindBox = (ZCodeFindBox)form.Controls.Find("FundingCurrencyCodeFindBox", true).FirstOrDefault();
				var chequeBookGuidFindBox = (ZGuidFindBox)form.Controls.Find("ChequeBookGuidFindBox", true).FirstOrDefault();

				Assert(fundingBankAccountFindBox.Visible);
				Assert(fundingCurrencyCodeFindBox.Visible);
				Assert(!chequeBookGuidFindBox.Visible);

				BatchPoster.APB_PaymentType = ReceiptTypes.Cash;

				Assert(!fundingBankAccountFindBox.Visible);
				Assert(!fundingCurrencyCodeFindBox.Visible);
				Assert(chequeBookGuidFindBox.Visible);
			}
		}

		public void TestDisclaimerMessage()
		{
			SetupDataForPostingTest();
			var bankAccount = TestObjectCreator.CreateEPaymentBankAccount();
			BatchPoster.APB_AB = bankAccount.PK;

			using (var form = new PaymentBatchForm_ForTestOnly(BatchPoster))
			{
				form.Show();

				var disclaimerMessage = (ZLabel)form.Controls.Find("DisclaimerMessageLabel", true).FirstOrDefault();
				AssertNotNull(disclaimerMessage);
				AssertEquals(System.Drawing.SystemColors.Highlight, disclaimerMessage.ForeColor);

				Assert(BatchPoster.BankAccount.IsEPaymentAccount);
				Assert(disclaimerMessage.Visible);
				AssertEquals(@"FX quotes are provided by and the FX transaction is executed by OFX, a third party service provider.
CargoWise provides the messaging and information exchange only.", disclaimerMessage.Text);

				BatchPoster.APB_AB = TestObjectCreator.AUDBankAccount.PK;
				Assert(!BatchPoster.BankAccount.IsEPaymentAccount);
				Assert(!disclaimerMessage.Visible);
				AssertEquals(string.Empty, disclaimerMessage.Text);

				BatchPoster.APB_AB = ZGuid.Empty;
				Assert(!disclaimerMessage.Visible);
			}
		}

		public void TestCurrencySummaryTotalPaymentTextBoxCaptionResourceStringIsTotalForAllPaymentsInLocalCurrency()
		{
			SetupDataForPostingTest();

			using (var form = new PaymentBatchForm_ForTestOnly(BatchPoster))
			{
				form.Show();

				var currencySummaryTotalPaymentTextBox = form.Controls.Find("CurrencySummaryTotalPaymentTextBox", true).First() as ZCalcEdit;
				AssertEquals("Total for all Payments in Local Currency", currencySummaryTotalPaymentTextBox.CaptionResourceString.Caption);
			}
		}

		public void TestLearnMoreButtonOpensUpProductMarketingPage()
		{
			SetupDataForPostingTest();
			var bankAccount = TestObjectCreator.CreateEPaymentBankAccount();
			BatchPoster.APB_AB = bankAccount.PK;

			using (var form = new PaymentBatchForm_ForTestOnly(BatchPoster))
			{
				form.Show();

				var learnMoreButton = (ZButton)form.Controls.Find("LearnMoreButton", true).FirstOrDefault();
				AssertNotNull(learnMoreButton);

				Assert(BatchPoster.BankAccount.IsEPaymentAccount);
				Assert(learnMoreButton.Visible);
				WebUrlLauncher.ClearLastUrlLaunched();
				learnMoreButton.PerformClick();
				AssertEquals(AccountingMasterFilesRegistry.Instance.EPaymentProductMarketingWebURL.Value, WebUrlLauncher.LastUrlLaunched);

				BatchPoster.APB_AB = TestObjectCreator.AUDBankAccount.PK;
				Assert(!BatchPoster.BankAccount.IsEPaymentAccount);
				Assert(!learnMoreButton.Visible);

				BatchPoster.APB_AB = ZGuid.Empty;
				Assert(!learnMoreButton.Visible);
			}
		}

		public void TestLearnMoreButtonIsEnabledAfterSavingForm()
		{
			SetupDataForPostingTest();
			var bankAccount = TestObjectCreator.CreateEPaymentBankAccount();
			BatchPoster.APB_AB = bankAccount.PK;
			BatchPoster.APB_PaymentType = ReceiptTypes.EPayment;
			BatchPoster.APB_ChequeOrReference = "00001114";
			BatchPoster.PostPaymentsAsPaymentApprovals = true;
			BatchPoster.PaymentApprovalCollection[0].AV_EPaymentReasonCode = EPaymentReasonCodes.OFXReasonCodes.AccountingServices;
			BatchPoster.PaymentApprovalCollection[1].AV_EPaymentReasonCode = EPaymentReasonCodes.OFXReasonCodes.AccountingServices;
			BatchPoster.PaymentApprovalCollection[2].AV_EPaymentReasonCode = EPaymentReasonCodes.OFXReasonCodes.AccountingServices;

			var approvalWithUnconfirmedDeal = BatchPoster.PaymentApprovalCollection[1];
			approvalWithUnconfirmedDeal.AV_ChequeOrReference = "0002";
			approvalWithUnconfirmedDeal.AV_Status = PaymentApprovalStatus.FullyApproved;
			TestObjectCreator.CreateValidEPaymentDealForStatus(DealStatusCodes.SubmissionFailed, approvalWithUnconfirmedDeal);

			var approvalWithoutDeal = BatchPoster.PaymentApprovalCollection[2];

			using (var form = new PaymentBatchForm_ForTestOnly(BatchPoster))
			{
				form.Show();

				var learnMoreButton = (ZButton)form.Controls.Find("LearnMoreButton", true).FirstOrDefault();
				AssertNotNull(learnMoreButton);

				Assert(!BatchPoster.IsInDatabase);
				Assert(learnMoreButton.Visible);
				Assert(learnMoreButton.Enabled);

				form.SaveAsDraftButton_ForTestOnly.PerformClick();

				Assert(BatchPoster.IsInDatabase);
				Assert(learnMoreButton.Visible);
				Assert(learnMoreButton.Enabled);
			}
		}

		public void TestEPaymentProviderLogoAndServiceProviderLabel()
		{
			SetupDataForPostingTest();
			BatchPoster.APB_AB = TestObjectCreator.AUDBankAccount.PK;

			using (var form = new PaymentBatchForm_ForTestOnly(BatchPoster))
			{
				form.Show();

				var providerLogoPictureBox = (KPictureBox)form.Controls.Find("ProviderLogoPictureBox", true)[0];
				AssertNotNull(providerLogoPictureBox);
				AssertEquals(PictureBoxSizeMode.Zoom, providerLogoPictureBox.SizeMode);
				var serviceProviderLabel = (ZLabel)form.Controls.Find("ServiceProviderLabel", true).FirstOrDefault();
				AssertNotNull(serviceProviderLabel);

				Assert(!BatchPoster.BankAccount.IsEPaymentAccount);
				Assert(!providerLogoPictureBox.Visible);
				Assert(!serviceProviderLabel.Visible);
				AssertNull("OFX logo should not be displayed", providerLogoPictureBox.Image);
				WebUrlLauncher.ClearLastUrlLaunched();
				form.ProviderLogoPictureBox_Click_ForTestOnly(null, null);
				AssertEquals(string.Empty, WebUrlLauncher.LastUrlLaunched);

				var bankAccount = TestObjectCreator.CreateEPaymentBankAccount();
				BatchPoster.APB_AB = bankAccount.PK;
				Assert(BatchPoster.BankAccount.IsEPaymentAccount);
				Assert(providerLogoPictureBox.Visible);
				Assert(serviceProviderLabel.Visible);
				AssertNotNull("OFX logo should be displayed", providerLogoPictureBox.Image);
				AssertImageEquals("OFX logo should be displayed", AccountingMasterFilesRegistry.Instance.OFXLogo.Value, providerLogoPictureBox.Image);
				WebUrlLauncher.ClearLastUrlLaunched();
				form.ProviderLogoPictureBox_Click_ForTestOnly(null, null);
				AssertEquals(AccountingMasterFilesRegistry.Instance.OFXWebURL.Value, WebUrlLauncher.LastUrlLaunched);

				BatchPoster.APB_AB = ZGuid.Empty;
				Assert(!providerLogoPictureBox.Visible);
				Assert(!serviceProviderLabel.Visible);
				AssertNull("OFX logo should not be displayed", providerLogoPictureBox.Image);
				WebUrlLauncher.ClearLastUrlLaunched();
				form.ProviderLogoPictureBox_Click_ForTestOnly(null, null);
				AssertEquals(string.Empty, WebUrlLauncher.LastUrlLaunched);
			}
		}

		#region Payment Batch With Deals

		public void TestPostingApprovalsWithDeals_PaymentTypeIsEPA()
		{
			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			{
				SetupDataForPostingTest();
				var bankAccount = TestObjectCreator.CreateEPaymentBankAccount();
				var chequeBook = TestObjectCreator.CreateChequeBook("ofxCheque", 1000, bankAccount);
				BatchPoster.APB_PaymentType = ReceiptTypes.EPayment;
				BatchPoster.APB_AB = bankAccount.PK;
				BatchPoster.PostPaymentsAsPaymentApprovals = true;

				var approvalWithConfirmedDeal = BatchPoster.PaymentApprovalCollection[0];
				approvalWithConfirmedDeal.AV_ChequeOrReference = "0001";
				approvalWithConfirmedDeal.AV_Status = PaymentApprovalStatus.FullyApproved;
				approvalWithConfirmedDeal.AV_EPaymentReasonCode = EPaymentReasonCodes.OFXReasonCodes.AccountingServices;
				TestObjectCreator.CreateValidEPaymentDealForStatus(DealStatusCodes.Accepted, approvalWithConfirmedDeal);

				var approvalWithUnconfirmedDeal = BatchPoster.PaymentApprovalCollection[1];
				approvalWithUnconfirmedDeal.AV_ChequeOrReference = "0002";
				approvalWithUnconfirmedDeal.AV_Status = PaymentApprovalStatus.FullyApproved;
				approvalWithUnconfirmedDeal.AV_EPaymentReasonCode = EPaymentReasonCodes.OFXReasonCodes.AccountingServices;
				TestObjectCreator.CreateValidEPaymentDealForStatus(DealStatusCodes.SubmissionFailed, approvalWithUnconfirmedDeal);

				var approvalWithoutDeal = BatchPoster.PaymentApprovalCollection[2];
				approvalWithoutDeal.AV_ChequeOrReference = "0003";
				approvalWithoutDeal.AV_Status = PaymentApprovalStatus.FullyApproved;
				approvalWithoutDeal.AV_EPaymentReasonCode = EPaymentReasonCodes.OFXReasonCodes.AccountingServices;

				var paymentApprovalItem = Factory.NewWithValidTestData<PaymentApprovalItem>();
				paymentApprovalItem.A2_AH = approvalWithConfirmedDeal.MatchingBaseObject.MatchedTransactions[0].Identifier;
				paymentApprovalItem.A2_AV = approvalWithConfirmedDeal.PK;
				paymentApprovalItem.A2_PaymentThisRun = approvalWithConfirmedDeal.MatchingBaseObject.MatchedTransactions[0].OutstandingAmount;

				Factory.Save();

				Assert(BatchPoster.BankAccount.IsEPaymentAccount);

				using (var form = new PaymentBatchForm_ForTestOnly(BatchPoster))
				{
					form.Show();
					Application.DoEvents();
					form.PaymentBatchGrid_ForTestOnly.SelectAllElements();

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

					Assert(!approvalWithConfirmedDeal.IsPosted);
					Assert(!approvalWithUnconfirmedDeal.IsPosted);
					Assert(!approvalWithoutDeal.IsPosted);

					form.ActionsMenuItem_ForTestOnly.MenuItems.FindByText(PaymentProcessingGUIHelper.PostItemMenuText, true).PerformClick();

					var expectedMessage = $@"The following Payments cannot be processed:
{approvalWithUnconfirmedDeal.GetDescription()}
Payment type is an E-Payment, but it doesn't have a confirmed E-Payment Deal yet. You can post the payment once the deal has been 'Accepted' by the provider and the final exchange rate has been confirmed.
{approvalWithoutDeal.GetDescription()}
Payment type is an E-Payment, but it doesn't have an E-Payment Deal yet. Please process an E-Payment before posting.

The following Payments will be processed:
{approvalWithConfirmedDeal.GetDescription()}

Do you want to post these transactions?";

					AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
					Assert(approvalWithConfirmedDeal.IsPosted);
					Assert(!approvalWithUnconfirmedDeal.IsPosted);
					Assert(!approvalWithoutDeal.IsPosted);

					var paymentProcessingGuiHelper = form.PaymentProcessingGUIHelperField_ForTestOnly;
					AssertEquals(1, paymentProcessingGuiHelper.bankTransferFormsPrompted.Count);
					AssertEquals(typeof(GUI.CashBook.Transfer.BankTransferForm), paymentProcessingGuiHelper.bankTransferFormsPrompted[0].GetType());
					paymentProcessingGuiHelper.bankTransferFormsPrompted[0].Dispose();
				}
			}
		}

		public void TestPostingApprovalsWithoutDeals_PaymentTypeIsNotEPA()
		{
			SetupDataForPostingTest();
			BatchPoster.APB_AB = TestBank.PK;
			BatchPoster.APB_AK = TestObjectCreator.CreateChequeBook("cheque1", 1000, TestBank).PK;
			BatchPoster.PostPaymentsAsPaymentApprovals = true;

			var approvalWithoutDeal1 = BatchPoster.PaymentApprovalCollection[0];
			approvalWithoutDeal1.AV_ChequeOrReference = "0001";

			var approvalWithoutDeal2 = BatchPoster.PaymentApprovalCollection[1];
			approvalWithoutDeal2.AV_ChequeOrReference = "0002";

			var approvalWithoutDeal3 = BatchPoster.PaymentApprovalCollection[2];
			approvalWithoutDeal3.AV_ChequeOrReference = "0003";

			var paymentApprovalItem1 = Factory.NewWithValidTestData<PaymentApprovalItem>();
			paymentApprovalItem1.A2_AH = approvalWithoutDeal1.MatchingBaseObject.MatchedTransactions[0].Identifier;
			paymentApprovalItem1.A2_AV = approvalWithoutDeal1.PK;
			paymentApprovalItem1.A2_PaymentThisRun = approvalWithoutDeal1.MatchingBaseObject.MatchedTransactions[0].OutstandingAmount;

			var paymentApprovalItem2 = Factory.NewWithValidTestData<PaymentApprovalItem>();
			paymentApprovalItem2.A2_AH = approvalWithoutDeal2.MatchingBaseObject.MatchedTransactions[0].Identifier;
			paymentApprovalItem2.A2_AV = approvalWithoutDeal2.PK;
			paymentApprovalItem2.A2_PaymentThisRun = approvalWithoutDeal2.MatchingBaseObject.MatchedTransactions[0].OutstandingAmount;

			var paymentApprovalItem3 = Factory.NewWithValidTestData<PaymentApprovalItem>();
			paymentApprovalItem3.A2_AH = approvalWithoutDeal3.MatchingBaseObject.MatchedTransactions[0].Identifier;
			paymentApprovalItem3.A2_AV = approvalWithoutDeal3.PK;
			paymentApprovalItem3.A2_PaymentThisRun = approvalWithoutDeal3.MatchingBaseObject.MatchedTransactions[0].OutstandingAmount;

			Factory.Save();

			Assert(!BatchPoster.BankAccount.IsEPaymentAccount);

			using (var form = new PaymentBatchForm_ForTestOnly(BatchPoster))
			{
				form.Show();
				form.PaymentBatchGrid_ForTestOnly.SelectAllElements();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				Assert(!approvalWithoutDeal1.IsPosted);
				Assert(!approvalWithoutDeal2.IsPosted);
				Assert(!approvalWithoutDeal3.IsPosted);

				form.ActionsMenuItem_ForTestOnly.MenuItems.FindByText(PaymentProcessingGUIHelper.PostItemMenuText, true).PerformClick();

				var expectedMessage = $@"The following Payments will be processed:
{approvalWithoutDeal1.GetDescription()}
{approvalWithoutDeal2.GetDescription()}
{approvalWithoutDeal3.GetDescription()}

Do you want to post these transactions?";

				AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(approvalWithoutDeal1.IsPosted);
				Assert(approvalWithoutDeal2.IsPosted);
				Assert(approvalWithoutDeal3.IsPosted);
			}
		}

		public void TestCancelPaymentApprovalsWithDeals()
		{
			SetupDataForPostingTest();
			var bankAccount = TestObjectCreator.CreateEPaymentBankAccount();
			BatchPoster.APB_AB = bankAccount.PK;
			BatchPoster.PostPaymentsAsPaymentApprovals = true;
			Factory.Save();

			var approvalWithActiveDeal1 = BatchPoster.PaymentApprovalCollection[0];
			TestObjectCreator.CreateValidEPaymentDealForStatus(DealStatusCodes.Accepted, approvalWithActiveDeal1);

			var approvalWithActiveDeal2 = BatchPoster.PaymentApprovalCollection[1];
			TestObjectCreator.CreateValidEPaymentDealForStatus(DealStatusCodes.Accepted, approvalWithActiveDeal2);

			var approvalWithInactiveDeal = BatchPoster.PaymentApprovalCollection[2];
			TestObjectCreator.CreateValidEPaymentDealForStatus(DealStatusCodes.Declined, approvalWithInactiveDeal);

			Factory.Save();

			using (var form = new PaymentBatchForm_ForTestOnly(BatchPoster))
			{
				form.Show();
				form.PaymentBatchGrid_ForTestOnly.SelectAllElements();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				AssertEquals(PaymentApprovalStatus.FullyApproved, approvalWithActiveDeal1.AV_Status);
				AssertEquals(PaymentApprovalStatus.FullyApproved, approvalWithActiveDeal2.AV_Status);
				AssertEquals(PaymentApprovalStatus.FullyApproved, approvalWithInactiveDeal.AV_Status);

				form.ActionsMenuItem_ForTestOnly.MenuItems.FindByText(PaymentProcessingGUIHelper.CancelMenuText, true).PerformClick();

				var expectedMessage = $@"The following Payments cannot be canceled:
{approvalWithActiveDeal1.GetDescription()}
The payment has an active E-Payment Deal. Action not permitted.

{approvalWithActiveDeal2.GetDescription()}
The payment has an active E-Payment Deal. Action not permitted.

The following Payments will be canceled:
{approvalWithInactiveDeal.GetDescription()}

Do you want to cancel these Payments?";

				AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(PaymentApprovalStatus.FullyApproved, approvalWithActiveDeal1.AV_Status);
				AssertEquals(PaymentApprovalStatus.FullyApproved, approvalWithActiveDeal2.AV_Status);
				AssertEquals(PaymentApprovalStatus.Cancelled, approvalWithInactiveDeal.AV_Status);
			}
		}

		public void TestRejectPaymentApprovalsWithDeals()
		{
			SetupDataForPostingTest();
			var bankAccount = TestObjectCreator.CreateEPaymentBankAccount();
			BatchPoster.APB_AB = bankAccount.PK;
			BatchPoster.PostPaymentsAsPaymentApprovals = true;
			Factory.Save();

			var approvalWithActiveDeal1 = BatchPoster.PaymentApprovalCollection[0];
			TestObjectCreator.CreateValidEPaymentDealForStatus(DealStatusCodes.Accepted, approvalWithActiveDeal1);

			var approvalWithActiveDeal2 = BatchPoster.PaymentApprovalCollection[1];
			TestObjectCreator.CreateValidEPaymentDealForStatus(DealStatusCodes.Accepted, approvalWithActiveDeal2);

			var approvalWithInactiveDeal = BatchPoster.PaymentApprovalCollection[2];
			TestObjectCreator.CreateValidEPaymentDealForStatus(DealStatusCodes.Declined, approvalWithInactiveDeal);

			Factory.Save();

			using (var form = new PaymentBatchForm_ForTestOnly(BatchPoster))
			{
				form.Show();
				form.PaymentBatchGrid_ForTestOnly.SelectAllElements();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				void setReason(object x)
				{
					var rejectionForm = x as PaymentRejectionReasonForm;
					rejectionForm.SetReason("INS", "Not enough money");
				}

				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(setReason);

				AssertEquals(PaymentApprovalStatus.FullyApproved, approvalWithActiveDeal1.AV_Status);
				AssertEquals(PaymentApprovalStatus.FullyApproved, approvalWithActiveDeal2.AV_Status);
				AssertEquals(PaymentApprovalStatus.FullyApproved, approvalWithInactiveDeal.AV_Status);

				form.ActionsMenuItem_ForTestOnly.MenuItems.FindByText(PaymentProcessingGUIHelper.RejectItemMenuText, true).PerformClick();

				var expectedMessage = $@"The following Payments cannot be rejected:
{approvalWithActiveDeal1.GetDescription()}
The payment has an active E-Payment Deal. Action not permitted.

{approvalWithActiveDeal2.GetDescription()}
The payment has an active E-Payment Deal. Action not permitted.";

				AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(PaymentApprovalStatus.FullyApproved, approvalWithActiveDeal1.AV_Status);
				AssertEquals(PaymentApprovalStatus.FullyApproved, approvalWithActiveDeal2.AV_Status);
				AssertEquals(PaymentApprovalStatus.Rejected, approvalWithInactiveDeal.AV_Status);
			}
		}

		public void TestUnauthorizePaymentApprovalsWithDeals()
		{
			SetupDataForPostingTest();
			var bankAccount = TestObjectCreator.CreateEPaymentBankAccount();
			BatchPoster.APB_AB = bankAccount.PK;
			BatchPoster.PostPaymentsAsPaymentApprovals = true;

			var approvalWithActiveDeal1 = BatchPoster.PaymentApprovalCollection[0];
			approvalWithActiveDeal1.AV_GS_NKApproval1st = GlbStaff.CurrentUser.GS_Code;
			TestObjectCreator.CreateValidEPaymentDealForStatus(DealStatusCodes.Accepted, approvalWithActiveDeal1);

			var approvalWithActiveDeal2 = BatchPoster.PaymentApprovalCollection[1];
			approvalWithActiveDeal2.AV_GS_NKApproval1st = GlbStaff.CurrentUser.GS_Code;
			TestObjectCreator.CreateValidEPaymentDealForStatus(DealStatusCodes.Accepted, approvalWithActiveDeal2);

			var approvalWithInactiveDeal = BatchPoster.PaymentApprovalCollection[2];
			approvalWithInactiveDeal.AV_GS_NKApproval1st = GlbStaff.CurrentUser.GS_Code;
			TestObjectCreator.CreateValidEPaymentDealForStatus(DealStatusCodes.Declined, approvalWithInactiveDeal);
			Factory.Save();

			var authorisationSettings = new PaymentAuthorisationSettingsCollection();
			var setting1 = authorisationSettings.AddNew();
			setting1.Amount = 70m;
			setting1.AuthorisationRequirement = AuthorisationCodes.NoApprovalRequired;
			setting1.Range = RangeCodes.UpTo;
			var setting2 = authorisationSettings.AddNew();
			setting2.Amount = 70m;
			setting2.AuthorisationRequirement = AuthorisationCodes.FirstApprovalRequiredOnly;
			setting2.Range = RangeCodes.Above;

			using (AccountingConfigurationRegistry.Instance.PayableAuthorizationSettings.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, authorisationSettings))
			using (var form = new PaymentBatchForm_ForTestOnly(BatchPoster))
			{
				form.Show();
				form.PaymentBatchGrid_ForTestOnly.SelectAllElements();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				AssertEquals(GlbStaff.CurrentUser.GS_Code, approvalWithActiveDeal1.AV_GS_NKApproval1st);
				AssertEquals(GlbStaff.CurrentUser.GS_Code, approvalWithActiveDeal2.AV_GS_NKApproval1st);
				AssertEquals(GlbStaff.CurrentUser.GS_Code, approvalWithInactiveDeal.AV_GS_NKApproval1st);

				form.ActionsMenuItem_ForTestOnly.MenuItems.FindByText(PaymentProcessingGUIHelper.UnAuthoriseMenuText, true).PerformClick();

				var expectedMessage = $@"The following Payments cannot be unauthorized:
{approvalWithActiveDeal1.GetDescription()}
The payment has an active E-Payment Deal. Action not permitted.

{approvalWithActiveDeal2.GetDescription()}
The payment has an active E-Payment Deal. Action not permitted.

The following Payments will be unauthorized:
{approvalWithInactiveDeal.GetDescription()}

Do you want to unauthorize these Payments?";

				AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(GlbStaff.CurrentUser.GS_Code, approvalWithActiveDeal1.AV_GS_NKApproval1st);
				AssertEquals(GlbStaff.CurrentUser.GS_Code, approvalWithActiveDeal2.AV_GS_NKApproval1st);
				AssertEquals(ZString.Empty, approvalWithInactiveDeal.AV_GS_NKApproval1st);
			}
		}

		public void TestRemovePaymentApprovalsWithDeals()
		{
			SetupDataForPostingTest();
			var bankAccount = TestObjectCreator.CreateEPaymentBankAccount();
			BatchPoster.APB_AB = bankAccount.PK;
			BatchPoster.PostPaymentsAsPaymentApprovals = true;
			Factory.Save();

			var approvalWithActiveDeal1 = BatchPoster.PaymentApprovalCollection[0];
			TestObjectCreator.CreateValidEPaymentDealForStatus(DealStatusCodes.Accepted, approvalWithActiveDeal1);

			var approvalWithActiveDeal2 = BatchPoster.PaymentApprovalCollection[1];
			TestObjectCreator.CreateValidEPaymentDealForStatus(DealStatusCodes.Accepted, approvalWithActiveDeal2);

			var approvalWithInactiveDeal = BatchPoster.PaymentApprovalCollection[2];
			TestObjectCreator.CreateValidEPaymentDealForStatus(DealStatusCodes.Declined, approvalWithInactiveDeal);

			Factory.Save();

			using (var form = new PaymentBatchForm_ForTestOnly(BatchPoster))
			{
				form.Show();
				form.PaymentBatchGrid_ForTestOnly.SelectAllElements();

				Assert(!approvalWithActiveDeal1.IsDeleted);
				Assert(!approvalWithActiveDeal2.IsDeleted);
				Assert(!approvalWithInactiveDeal.IsDeleted);
				AssertEquals(3, BatchPoster.PaymentApprovalCollection.Count);

				form.PaymentBatchGrid_ForTestOnly.Select(0);
				AssertEquals(approvalWithActiveDeal1.PK, form.PaymentBatchGrid_ForTestOnly.SelectedElements[0].PK);
				form.DeletePaymentTransaction_ForTestOnly(null, null);

				Assert(!approvalWithActiveDeal1.IsDeleted);
				Assert(!approvalWithActiveDeal2.IsDeleted);
				Assert(!approvalWithInactiveDeal.IsDeleted);
				AssertEquals("approvalWithActiveDeal1 is removed from batch, but not deleted.", 2, BatchPoster.PaymentApprovalCollection.Count);

				form.PaymentBatchGrid_ForTestOnly.Select(0);
				AssertEquals(approvalWithActiveDeal2.PK, form.PaymentBatchGrid_ForTestOnly.SelectedElements[0].PK);
				form.DeletePaymentTransaction_ForTestOnly(null, null);

				Assert(!approvalWithActiveDeal1.IsDeleted);
				Assert(!approvalWithActiveDeal2.IsDeleted);
				Assert(!approvalWithInactiveDeal.IsDeleted);
				AssertEquals("approvalWithActiveDeal2 is removed from batch, but not deleted.", 1, BatchPoster.PaymentApprovalCollection.Count);

				form.PaymentBatchGrid_ForTestOnly.Select(0);
				AssertEquals(approvalWithInactiveDeal.PK, form.PaymentBatchGrid_ForTestOnly.SelectedElements[0].PK);
				form.DeletePaymentTransaction_ForTestOnly(null, null);

				Assert(!approvalWithActiveDeal1.IsDeleted);
				Assert(!approvalWithActiveDeal2.IsDeleted);
				Assert(!approvalWithInactiveDeal.IsDeleted);
				AssertEquals("approvalWithInactiveDeal is removed from batch, but not deleted.", 0, BatchPoster.PaymentApprovalCollection.Count);
			}
		}

		public void TestChangingExchangeRateForPaymentApprovalsWithDeals()
		{
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, "BUY", 1m);
			SetupDataForPostingTest();
			var bankAccount = TestObjectCreator.CreateEPaymentBankAccount();
			BatchPoster.APB_AB = bankAccount.PK;
			BatchPoster.PostPaymentsAsPaymentApprovals = true;
			Factory.Save();

			var approvalWithActiveDeal1 = BatchPoster.PaymentApprovalCollection[0];
			TestObjectCreator.CreateValidEPaymentDealForStatus(DealStatusCodes.Accepted, approvalWithActiveDeal1);

			var approvalWithActiveDeal2 = BatchPoster.PaymentApprovalCollection[1];
			TestObjectCreator.CreateValidEPaymentDealForStatus(DealStatusCodes.Accepted, approvalWithActiveDeal2);

			var approvalWithInactiveDeal = BatchPoster.PaymentApprovalCollection[2];
			TestObjectCreator.CreateValidEPaymentDealForStatus(DealStatusCodes.Declined, approvalWithInactiveDeal);

			Factory.Save();

			AssertEquals(Core.Constants.CurrencyCodes.UnitedStates, approvalWithActiveDeal1.AV_RX_NKPaymentCurrency);
			AssertEquals(Core.Constants.CurrencyCodes.UnitedStates, approvalWithActiveDeal2.AV_RX_NKPaymentCurrency);
			AssertEquals(Core.Constants.CurrencyCodes.UnitedStates, approvalWithInactiveDeal.AV_RX_NKPaymentCurrency);

			using (var form = new PaymentBatchForm_ForTestOnly(BatchPoster))
			{
				form.Show();
				Application.DoEvents();

				var summaryRows = BatchPoster.CurrencySummary.SummaryRows.OfType<CurrencySummaryRow>();
				var summaryRowUSD = summaryRows.Single(x => x.Currency == Core.Constants.CurrencyCodes.UnitedStates);

				AssertEquals(1m, summaryRowUSD.ExchangeRate);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				summaryRowUSD.ExchangeRate = 1.5m;

				AssertEquals(PaymentApprovalBase.ActiveDealErrorTextForChangingPaymentDetails, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(1m, summaryRowUSD.ExchangeRate);
			}
		}

		#endregion

		#region Draft Payment Approval

		public void TestSaveAsDraftButton()
		{
			SetupDataForPostingTest();
			using (var form = new PaymentBatchForm_ForTestOnly(BatchPoster))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.Show();
				var saveAsDraftButton = form.Controls.Find("SaveAsDraftButton", true)[0];

				AssertEquals(true, saveAsDraftButton.Visible);
				AssertEquals(true, saveAsDraftButton.Enabled);
			}
		}

		public void TestSaveAsDraftSuccessfully()
		{
			SetupDataForPostingTest();
			using (var form = new PaymentBatchForm_ForTestOnly(BatchPoster))
			{
				form.Show();

				AssertEquals("There should be 3 PaymentApproval objects created", 3, BatchPoster.PaymentApprovalCollection.Count);
				AssertEquals("Should be 1 transaction in MatchedTransactions", 1, BatchPoster.PaymentApprovalCollection[0].MatchingBaseObject.MatchedTransactions.Count);
				AssertEquals("Should be 1 transaction in MatchedTransactions", 1, BatchPoster.PaymentApprovalCollection[1].MatchingBaseObject.MatchedTransactions.Count);
				AssertEquals("Should be 1 transaction in MatchedTransactions", 1, BatchPoster.PaymentApprovalCollection[2].MatchingBaseObject.MatchedTransactions.Count);
				AssertEquals("Payment amount should be defaulted", BatchPoster.PaymentApprovalCollection[0].AV_Amount, BatchPoster.PaymentApprovalCollection[0].AV_Calc_LocalAmount - BatchPoster.PaymentApprovalCollection[0].MatchingBaseObject.Balance);
				AssertEquals("Transaction Payment amount should be set", ((IMatching)TestAPInv).OSPartialPaymentAmount, ((IMatching)TestAPInv).OutstandingAmount);

				var saveAsDraftButton = form.Controls.Find("SaveAsDraftButton", true)[0] as ZButton;
				var saveButton = form.Controls.Find("SaveButton", true)[0];
				AssertEquals(true, saveButton.Enabled);
				saveAsDraftButton.PerformClick();
				BatchPostingHelper.AssertBatchPosterWillMatchAndPost(BatchPoster, 3, 0, 2, ZBool.False);

				AssertEquals(true, BatchPoster.PaymentApprovalCollection[0].IsInDatabase);
				AssertEquals(true, BatchPoster.PaymentApprovalCollection[1].IsInDatabase);
				AssertEquals(true, BatchPoster.PaymentApprovalCollection[2].IsInDatabase);
				AssertEquals(true, BatchPoster.PaymentApprovalCollection[0].IsDraft);
				AssertEquals(true, BatchPoster.PaymentApprovalCollection[1].IsDraft);
				AssertEquals(true, BatchPoster.PaymentApprovalCollection[2].IsDraft);
				AssertNotNull(BatchPoster.PaymentApprovalCollection[0].AV_PaymentApprovalReference);
				AssertNotNull(BatchPoster.PaymentApprovalCollection[1].AV_PaymentApprovalReference);
				AssertNotNull(BatchPoster.PaymentApprovalCollection[2].AV_PaymentApprovalReference);

				Assert("The posting form should not have been closed", !form.IsDisposed);
				AssertEquals(false, saveButton.Enabled);
			}
		}

		public void TestSaveAsDraftShowErrorAboutTransactions()
		{
			var exceptedMessage = @"Matching containing Bank Fee, AR Journal or AP Journal cannot be saved in Draft mode. To save as Draft, please remove these transactions.";

			SetupDataForPostingTest();
			BatchPoster.APB_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			BatchPoster.APB_AB = TestBank.PK;
			BatchPoster.PaymentApprovalCollection[0].MatchingBaseObject.AddMiscellaneousTransaction(Factory.NewWithValidTestData<ARJournal>());
			AssertNotNull(BatchPoster.PaymentApprovalCollection[0].MatchingBaseObject.BankFeeCurrent);
			AssertEquals(true, BatchPoster.PaymentApprovalCollection[0].HasNonDraftTransactions_ForTestOnly);

			using (var form = new PaymentBatchForm_ForTestOnly(BatchPoster))
			{
				form.Show();
				var saveAsDraftButton = form.Controls.Find("SaveAsDraftButton", true)[0] as ZButton;
				saveAsDraftButton.PerformClick();
				AssertEquals(exceptedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSaveAsDraftWillNotShowErrorAboutBalance()
		{
			SetupDataForPostingTest();
			BatchPoster.APB_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			BatchPoster.APB_AB = TestBank.PK;
			BatchPoster.PaymentApprovalCollection[0].AV_Amount++;

			using (var form = new PaymentBatchForm_ForTestOnly(BatchPoster))
			{
				AssertNotEquals("Pre-condition: The balance should not equal 0.", 0m, BatchPoster.PaymentApprovalCollection[0].MatchingBaseObject.Balance);

				form.Show();
				var saveAsDraftButton = form.Controls.Find("SaveAsDraftButton", true)[0] as ZButton;
				saveAsDraftButton.PerformClick();
				AssertEquals("Should not show any message as saved successfully.", null, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("The status should be 'DFT' as save as draft successfully.", true, BatchPoster.PaymentApprovalCollection[0].IsDraft);
				AssertEquals("Should save successfully even the balance is NOT zero as save as draft.", true, BatchPoster.PaymentApprovalCollection[0].IsInDatabase);
			}
		}

		#endregion

		#region EPayment Quotes

		public void TestCheckExchangeRateButton_WithActiveDeals()
		{
			SetupDataForPostingTest();
			var bankAccount = TestObjectCreator.CreateEPaymentBankAccount();
			BatchPoster.APB_AB = bankAccount.PK;
			BatchPoster.PostPaymentsAsPaymentApprovals = true;
			Factory.Save();

			var approvalWithActiveDeal1 = BatchPoster.PaymentApprovalCollection[0];
			TestObjectCreator.CreateValidEPaymentDealForStatus(DealStatusCodes.Accepted, approvalWithActiveDeal1);

			var approvalWithActiveDeal2 = BatchPoster.PaymentApprovalCollection[1];
			TestObjectCreator.CreateValidEPaymentDealForStatus(DealStatusCodes.Accepted, approvalWithActiveDeal2);

			var approvalWithInactiveDeal = BatchPoster.PaymentApprovalCollection[2];
			TestObjectCreator.CreateValidEPaymentDealForStatus(DealStatusCodes.Declined, approvalWithInactiveDeal);

			Factory.Save();

			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			using (var form = new PaymentBatchForm_ForTestOnly(BatchPoster))
			{
				form.Show();
				Application.DoEvents();

				var payments = BatchPoster.PaymentApprovalCollection.Cast<APPaymentApprovalWithoutAuthorisation>();
				var quotesOnPayments = BatchPoster.FilteredEPaymentQuotes.Cast<EPaymentQuote>();
				AssertEquals(3, quotesOnPayments.Count());

				var checkExRateButton = form.Controls.Find("CheckExRateButton", true)[0] as ZButton;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				checkExRateButton.PerformClick();

				AssertEquals(PaymentApprovalBase.ActiveDealErrorTextForChangingPaymentDetails, UnitTestUserNotification.Instance.LastMessage.Text);
				BatchPoster.FilteredEPaymentQuotes.Reload(true);
				AssertEquals(3, quotesOnPayments.Count());
			}
		}

		public void TestEPaymentControlsVisibilityDependsOnRegistry()
		{
			SetupDataForPostingTest();
			AssertEPaymentControlsVisibility(true);
			AssertEPaymentControlsVisibility(false);
		}

		void AssertEPaymentControlsVisibility(bool isOFXEPaymentEnabled)
		{
			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, isOFXEPaymentEnabled))
			{
				using (var form = new PaymentBatchForm_ForTestOnly(BatchPoster))
				{
					form.Show();
					if (isOFXEPaymentEnabled)
					{
						Assert("CheckExRateButton should be visible when E-Payment functionality is enabled.", form.CheckExRateButton_ForTestOnly.Visible);
						Assert("ProcessEPaymentsButton should be visible E-Payment functionality is enabled.", form.ProcessEPaymentsButton_ForTestOnly.Visible);
						Assert("TabControlBankAndEPayment should be visible E-Payment functionality is enabled.", form.EPaymentTabPage_ForTestOnly.TabVisible);
						Assert("CurrencySummaryTotalEPaymentTextBox should be visible E-Payment functionality is enabled.", form.CurrencySummaryTotalEPaymentTextBox_ForTestOnly.Visible);
						Assert("CurrencySummaryTotalEPaymentFeesTextBox should be visible when E-Payment functionality is enabled.", form.CurrencySummaryTotalEPaymentFeesTextBox_ForTestOnly.Visible);

						Assert("E-Payment Status column is shown when E-Payment functionality is enabled.", GetColumnStyle(form.PaymentBatchGrid_ForTestOnly, "DealStatusDescription").IsVisible);
						Assert("E-Payment Submitted Date column is shown when E-Payment functionality is enabled.", GetColumnStyle(form.PaymentBatchGrid_ForTestOnly, "DealSubmittedLocalTime").IsVisible);
						Assert("E-Payment Last Response Received Date is shown when E-Payment functionality is enabled.", GetColumnStyle(form.PaymentBatchGrid_ForTestOnly, "DealLastResponseLocalTime").IsVisible);
						Assert("E-Payment Message column is shown when E-Payment functionality is enabled.", GetColumnStyle(form.PaymentBatchGrid_ForTestOnly, "DealErrorMessage").IsVisible);
						Assert("E-Payment Provider Reference column is shown when E-Payment functionality is enabled.", GetColumnStyle(form.PaymentBatchGrid_ForTestOnly, "DealProviderReference").IsVisible);
						Assert("Default Payment Reason column is shown when E-Payment functionality is enabled.", GetColumnStyle(form.PaymentBatchGrid_ForTestOnly, "AV_EPaymentReasonCode").IsVisible);
					}
					else
					{
						Assert("CheckExRateButton should be invisible when E-Payment functionality is disabled.", !form.CheckExRateButton_ForTestOnly.Visible);
						Assert("ProcessEPaymentsButton should be invisible when E-Payment functionality is disabled.", !form.ProcessEPaymentsButton_ForTestOnly.Visible);
						Assert("TabControlBankAndEPayment should be invisible when E-Payment functionality is disabled.", !form.EPaymentTabPage_ForTestOnly.TabVisible);
						Assert("CurrencySummaryTotalEPaymentTextBox should be invisible E-Payment functionality is disabled.", !form.CurrencySummaryTotalEPaymentTextBox_ForTestOnly.Visible);
						Assert("CurrencySummaryTotalEPaymentFeesTextBox should be invisible when E-Payment functionality is disabled.", !form.CurrencySummaryTotalEPaymentFeesTextBox_ForTestOnly.Visible);

						AssertNull("E-Payment Status column is not shown when E-Payment functionality is disabled.", GetColumnStyle(form.PaymentBatchGrid_ForTestOnly, "DealStatusDescription"));
						AssertNull("E-Payment Submitted Date column is not shown when E-Payment functionality is disabled.", GetColumnStyle(form.PaymentBatchGrid_ForTestOnly, "DealSubmittedLocalTime"));
						AssertNull("E-Payment Last Response Received Date is not shown when E-Payment functionality is disabled.", GetColumnStyle(form.PaymentBatchGrid_ForTestOnly, "DealLastResponseLocalTime"));
						AssertNull("E-Payment Message column is not shown when E-Payment functionality is disabled.", GetColumnStyle(form.PaymentBatchGrid_ForTestOnly, "DealErrorMessage"));
						AssertNull("E-Payment Provider Reference column is not shown when E-Payment functionality is disabled.", GetColumnStyle(form.PaymentBatchGrid_ForTestOnly, "DealProviderReference"));
						AssertNull("Default Payment Reason column is not shown when E-Payment functionality is disabled.", GetColumnStyle(form.PaymentBatchGrid_ForTestOnly, "AV_EPaymentReasonCode"));
					}
				}
			}

			ZGridColumnInfo GetColumnStyle(ZGrid grid, string columnName)
			{
				return grid.ColumnStyles.Cast<ZGridColumnInfo>().FirstOrDefault(s => s.ColumnName == columnName);
			}
		}

		public void TestOnLoad_EPaymentRecipientListLastUpdated_Refreshed()
		{
			SetupDataForPostingTest();
			var bankAccount = TestObjectCreator.CreateEPaymentBankAccount();
			BatchPoster.APB_AB = bankAccount.PK;
			BatchPoster.APB_PaymentType = ReceiptTypes.EPayment;
			BatchPoster.APB_ChequeOrReference = "00001114";
			BatchPoster.PostPaymentsAsPaymentApprovals = true;
			var paymentApproval = BatchPoster.PaymentApprovalCollection[0];
			AssertEquals("Last Updated time should be empty", ZDateTime.Empty, paymentApproval.EPaymentRecipientListLastUpdatedTimeLocal);

			var latestRequestReceived = new ZDateTime(2021, 8, 16);
			var request = Factory.NewWithValidTestData<AccEPaymentBeneficiaryRequest>();
			request.ABR_GC_Company = GlbCompany.CurrentCompany.PK;
			request.ABR_Status = EPaymentStatusCodes.BeneficiaryRequest.Received;
			request.ABR_LastResponseReceivedUtc = latestRequestReceived;
			Factory.Save();
			AssertEquals("Requests Not refreshed: Last Updated time should be empty", ZDateTime.Empty, paymentApproval.EPaymentRecipientListLastUpdatedTimeLocal);

			using (var form = new PaymentBatchForm_ForTestOnly(BatchPoster))
			{
				form.Show();
				AssertEquals("Requests have refreshed: Last Updated time should be correct", latestRequestReceived.ToLocalBranchTime(), paymentApproval.EPaymentRecipientListLastUpdatedTimeLocal);
			}
		}

		public void TestColumnAvailability_PaymentTypeIsDDR_BankAuditAndPayeeDetailsAvailable_EPaymentSyncDetailsUnavailable()
		{
			SetupDataForPostingTest();
			BatchPoster.APB_PaymentType = ZArchitecture.Core.ReceiptTypes.DirectDebit;

			using (var form = new PaymentBatchForm_ForTestOnly(BatchPoster))
			{
				form.Show();

				var columns = form.PaymentBatchGrid_ForTestOnly.Columns;
				AssertBankAuditColumnsAvailability(columns, true);
				AssertPayeeDetailsColumnsAvailability(columns, true);
				AssertEPaymentSyncDetailsColumnsAvailability(columns, false);
			}
		}

		public void TestColumnAvailability_PaymentTypeIsEPA_BankAuditAndPayeeDetailsAndEPaymentSyncDetailsAvailable()
		{
			SetupDataForPostingTest();
			BatchPoster.APB_PaymentType = ZArchitecture.Core.ReceiptTypes.EPayment;

			using (var form = new PaymentBatchForm_ForTestOnly(BatchPoster))
			{
				form.Show();

				var columns = form.PaymentBatchGrid_ForTestOnly.Columns;
				AssertBankAuditColumnsAvailability(columns, true);
				AssertPayeeDetailsColumnsAvailability(columns, true);
				AssertEPaymentSyncDetailsColumnsAvailability(columns, true);
			}
		}

		public void TestColumnAvailabilityChangesOnPaymentTypeChange()
		{
			SetupDataForPostingTest();
			BatchPoster.APB_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;

			using (var form = new PaymentBatchForm_ForTestOnly(BatchPoster))
			{
				form.Show();

				var columns = form.PaymentBatchGrid_ForTestOnly.Columns;
				AssertBankAuditColumnsAvailability(columns, false);
				AssertPayeeDetailsColumnsAvailability(columns, false);
				AssertEPaymentSyncDetailsColumnsAvailability(columns, false);

				BatchPoster.APB_PaymentType = ZArchitecture.Core.ReceiptTypes.DirectDebit;
				AssertBankAuditColumnsAvailability(columns, true);
				AssertPayeeDetailsColumnsAvailability(columns, true);
				AssertEPaymentSyncDetailsColumnsAvailability(columns, false);

				BatchPoster.APB_PaymentType = ZArchitecture.Core.ReceiptTypes.Cash;
				AssertBankAuditColumnsAvailability(columns, false);
				AssertPayeeDetailsColumnsAvailability(columns, false);
				AssertEPaymentSyncDetailsColumnsAvailability(columns, false);

				BatchPoster.APB_PaymentType = ZArchitecture.Core.ReceiptTypes.EPayment;
				AssertBankAuditColumnsAvailability(columns, true);
				AssertPayeeDetailsColumnsAvailability(columns, true);
				AssertEPaymentSyncDetailsColumnsAvailability(columns, true);

				BatchPoster.APB_PaymentType = ZArchitecture.Core.ReceiptTypes.CreditCard;
				AssertBankAuditColumnsAvailability(columns, false);
				AssertPayeeDetailsColumnsAvailability(columns, false);
				AssertEPaymentSyncDetailsColumnsAvailability(columns, false);
			}
		}

		void AssertBankAuditColumnsAvailability(ZGridColumns paymentBatchGridColumns, bool available)
		{
			var bankAuditColumnNames = new[] { "BankCreateUser", "BankCreateTimeLocal", "BankLastEditUser", "BankLastEditTimeLocal" };
			AssertColumnAvailability(paymentBatchGridColumns, available, bankAuditColumnNames);
		}

		void AssertPayeeDetailsColumnsAvailability(ZGridColumns paymentBatchGridColumns, bool available)
		{
			var payeeDetailsColumns = new[] { "PayeeBankName", "AccountTitle", "PayeeBankAccountCountry", "PayeeBankBSB", "PayeeBankAccountNumber" };
			AssertColumnAvailability(paymentBatchGridColumns, available, payeeDetailsColumns);
		}

		void AssertEPaymentSyncDetailsColumnsAvailability(ZGridColumns paymentBatchGridColumns, bool available)
		{
			var ePaymentSyncDetailsColumns = new[] { "EPaymentRecipientListLastUpdatedTimeLocal", "EPaymentBeneficiaryLastEditTimeLocal" };
			AssertColumnAvailability(paymentBatchGridColumns, available, ePaymentSyncDetailsColumns);
		}

		void AssertColumnAvailability(ZGridColumns paymentBatchGridColumns, bool available, string[] columnNames)
		{
			foreach (var columnName in columnNames)
			{
				var thisColumn = paymentBatchGridColumns[columnName, false];
				if (available)
				{
					AssertNotNull($"Column {columnName} should exist", thisColumn);
					AssertEquals($"Column {columnName} availablility", available, !thisColumn.IsUnavailable);
					Assert($"Column {columnName} should not be visible", !thisColumn.IsVisible);
				}
				else
				{
					AssertNull($"Column {columnName} should not exist", thisColumn);
				}
			}
		}

		public void TestCheckExchangeRateButton_Enabled()
		{
			SetupDataForPostingTest();
			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			{
				BatchPoster.PaymentApprovalCollection.RemoveAll();
				foreach (var status in PaymentApprovalStatus.StatusesAllowingExchangeRateQuote)
				{
					var paymentWithValidStatus = TestObjectCreator.CreatePaymentApproval(typeof(APPaymentApprovalWithoutAuthorisation), ReceiptTypes.EFT, TestBank, TestCheques);
					paymentWithValidStatus.AV_Status = status;
					BatchPoster.PaymentApprovalCollection.Add(paymentWithValidStatus);
				}
				var paymentWithInvalidStatus = TestObjectCreator.CreatePaymentApproval(typeof(APPaymentApprovalWithoutAuthorisation), ReceiptTypes.EFT, TestBank, TestCheques);
				BatchPoster.PaymentApprovalCollection.Add(paymentWithInvalidStatus);

				var paymentStatusCodes = typeof(PaymentApprovalStatus).GetProperties(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public).Select(x => x.GetValue(null)).Where(x => x is string).Cast<string>();
				foreach (var status in paymentStatusCodes)
				{
					paymentWithInvalidStatus.AV_Status = status;
					using (var form = new PaymentBatchForm_ForTestOnly(BatchPoster))
					{
						form.Show();
						var checkExRateButton = form.Controls.Find("CheckExRateButton", true)[0] as ZButton;
						AssertEquals(PaymentApprovalStatus.StatusesAllowingExchangeRateQuote.Contains(status), checkExRateButton.Enabled);
					}
				}
			}
		}

		public void TestCheckExchangeRateButton_RefreshedOnSave()
		{
			SetupDataForPostingTest();
			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			{
				using (var form = new PaymentBatchForm_ForTestOnly(BatchPoster))
				{
					form.Show();
					BatchPoster.APB_PaymentType = ReceiptTypes.EFT;
					BatchPoster.APB_AB = TestBank.PK;

					var checkExRateButton = form.Controls.Find("CheckExRateButton", true)[0] as ZButton;
					AssertEquals(true, checkExRateButton.Visible);
					AssertEquals(true, checkExRateButton.Enabled);

					BatchPoster.APB_ChequeOrReference = "12345";

					form.FApplyButton_ForTestOnly.PerformClick();
					foreach (PaymentApprovalBase batch in BatchPoster.PaymentApprovalCollection)
					{
						Assert(batch.IsInDatabase);
						Assert(!batch.IsDraft);
						Assert(batch.IsPosted);
					}

					AssertEquals(true, checkExRateButton.Visible);
					AssertEquals(false, checkExRateButton.Enabled);
				}
			}
		}

		public void TestCheckExchangeRateButton_ClickBeforeSaving()
		{
			AssertCheckExchangeRateButton();
			var payments = BatchPoster.PaymentApprovalCollection.Cast<PaymentApprovalBase>();
			Assert(payments.All(x => x.IsInDatabase));
			Assert(payments.All(x => x.IsDraft));
		}

		public void TestCheckExchangeRateButton_ErrorWhenClickedBeforeSaving()
		{
			SetupDataForPostingTest();
			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			using (var form = new PaymentBatchForm_ForTestOnly(BatchPoster))
			{
				form.Show();
				BatchPoster.APB_PaymentType = ReceiptTypes.EFT;
				BatchPoster.APB_AB = TestBank.PK;

				var checkExRateButton = form.Controls.Find("CheckExRateButton", true)[0] as ZButton;
				AssertEquals(true, checkExRateButton.Visible);
				AssertEquals(true, checkExRateButton.Enabled);

				BatchPoster.PaymentApprovalCollection[0].MatchingBaseObject.AddMiscellaneousTransaction(Factory.NewWithValidTestData<ARJournal>());

				AssertEquals(true, checkExRateButton.Visible);
				AssertEquals(true, checkExRateButton.Enabled);
				checkExRateButton.PerformClick();

				AssertEquals("Matching containing Bank Fee, AR Journal or AP Journal cannot be saved in Draft mode. To save as Draft, please remove these transactions.", UnitTestUserNotification.Instance.LastMessage.Text);
				var payments = BatchPoster.PaymentApprovalCollection.Cast<APPaymentApprovalWithoutAuthorisation>();
				AssertEquals(0, BatchPoster.FilteredEPaymentQuotes.Count);
				Assert(payments.All(x => !x.IsInDatabase));
			}
		}

		public void TestCheckExchangeRateButton_ClickWhenSavedAsDraft()
		{
			AssertCheckExchangeRateButton((form) =>
			{
				var saveAsDraftButton = form.Controls.Find("SaveAsDraftButton", true)[0] as ZButton;
				saveAsDraftButton.PerformClick();
				BatchPostingHelper.AssertBatchPosterWillMatchAndPost(BatchPoster, 3, 0, 2, false);

				foreach (PaymentApprovalBase batch in BatchPoster.PaymentApprovalCollection)
				{
					Assert(batch.IsInDatabase);
					Assert(batch.IsDraft);
					Assert(!batch.IsFullyApproved);
				}

				AssertZButton(form, new List<string>
				{
					"CheckExRateButton",
					"CancelPostingButton",
					"ProcessEPaymentsButton"
				});
			});
		}

		public void TestCheckExchangeRateButton_ClickWhenSavedAsApproval()
		{
			AssertCheckExchangeRateButton((form) =>
			{
				BatchPoster.APB_ChequeOrReference = "12345";
				BatchPoster.PostPaymentsAsPaymentApprovals = true;
				form.FApplyButton_ForTestOnly.PerformClick();
				BatchPostingHelper.AssertBatchPosterWillMatchAndPost(BatchPoster, 3, 0, 2, false);
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				foreach (PaymentApprovalBase batch in BatchPoster.PaymentApprovalCollection)
				{
					Assert(batch.IsInDatabase);
					Assert(!batch.IsDraft);
					Assert(!batch.IsPosted);
					Assert(batch.IsFullyApproved);
				}
			});
		}

		void AssertCheckExchangeRateButton(Action<PaymentBatchForm_ForTestOnly> doFormActions = null)
		{
			SetupDataForPostingTest();
			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			using (var form = new PaymentBatchForm_ForTestOnly(BatchPoster))
			{
				form.Show();
				BatchPoster.APB_PaymentType = ReceiptTypes.EFT;
				BatchPoster.APB_AB = TestBank.PK;

				var checkExRateButton = form.Controls.Find("CheckExRateButton", true)[0] as ZButton;
				AssertEquals(true, checkExRateButton.Visible);
				AssertEquals(true, checkExRateButton.Enabled);

				doFormActions?.Invoke(form);

				AssertEquals(true, checkExRateButton.Visible);
				AssertEquals(true, checkExRateButton.Enabled);
				checkExRateButton.PerformClick();

				var payments = BatchPoster.PaymentApprovalCollection.Cast<PaymentApprovalBase>();
				var quotesOnPayments = BatchPoster.FilteredEPaymentQuotes.Cast<EPaymentQuote>();
				AssertEquals(3, quotesOnPayments.Count());
				Assert(quotesOnPayments.All(x => x.IsInDatabase));
				Assert(quotesOnPayments.All(x => !x.QU_InternalReference.IsEmpty));
				AssertEquals(
$@"E-Quote requests generated to service provider OFX. Response may take from a few moments up to several minutes to be received.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestClickingCheckExchangeRateButtonSendsUserToEPaymentTab()
		{
			SetupDataForPostingTest();
			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			using (var form = new PaymentBatchForm_ForTestOnly(BatchPoster))
			{
				form.Show();
				BatchPoster.APB_PaymentType = ReceiptTypes.EFT;
				BatchPoster.APB_AB = TestBank.PK;

				var checkExRateButton = form.Controls.Find("CheckExRateButton", true)[0] as ZButton;
				AssertEquals(true, checkExRateButton.Visible);
				AssertEquals(true, checkExRateButton.Enabled);

				var bankEPaymentTabControl = form.Controls.Find("TabControlBankAndEPayment", true).First() as ZTabControl;
				var summaryDetailsTabControl = form.Controls.Find("TabControlSummaryAndDetails", true).First() as ZTabControl;
				AssertEquals("Before click, Bank Detail tab page should be focused", bankEPaymentTabControl.SelectedIndex, 0);
				checkExRateButton.PerformClick();
				AssertEquals("After click, E-Payment tab page should be focused", bankEPaymentTabControl.SelectedIndex, 1);
				AssertEquals("After click, E-Payment summary tab page should be focused", summaryDetailsTabControl.SelectedIndex, 0);
			}
		}

		public void TestQuoteDetailsGridRefreshesOnSearch()
		{
			SetupDataForPostingTest();
			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			using (var form = new PaymentBatchForm_ForTestOnly(BatchPoster))
			{
				form.Show();
				BatchPoster.APB_PaymentType = ReceiptTypes.EFT;
				BatchPoster.APB_AB = TestBank.PK;

				var checkExRateButton = form.Controls.Find("CheckExRateButton", true)[0] as ZButton;
				checkExRateButton.PerformClick();
				AssertEquals(3, BatchPoster.FilteredEPaymentQuotes.Count);

				var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
				var quotesInNewFactory = newFactory.Load<EPaymentQuote>(new ZQuery(AccEPaymentQuoteSchema.PK, BatchPoster.FilteredEPaymentQuotes.Select(x => x.PK)));
				quotesInNewFactory.ForEach(x => x.QU_Status = QuoteStatusCodes.Failed);
				quotesInNewFactory.First().QU_Status = QuoteStatusCodes.Discarded;
				newFactory.Save();

				Assert(BatchPoster.FilteredEPaymentQuotes.Cast<EPaymentQuote>().All(x => x.QU_Status == QuoteStatusCodes.Queued));
				Assert(quotesInNewFactory.All(x => x.QU_Status != QuoteStatusCodes.Queued));

				var filterControl = form.Controls.Find("QuoteDetailsFilterControl", true).First() as AccountingOnFormFilterControl;
				AssertNotNull(filterControl?.FilterBusinessObject);

				var filter = filterControl.FilterBusinessObject["Include Discarded Quotes"] as ModuleFlagsFilter;
				filter.IsActive = true;
				filterControl.FirePerformSearch();
				AssertEquals(2, BatchPoster.FilteredEPaymentQuotes.Count);
				Assert(BatchPoster.FilteredEPaymentQuotes.Cast<EPaymentQuote>().All(x => x.QU_Status == QuoteStatusCodes.Failed));

				quotesInNewFactory.First().QU_Status = QuoteStatusCodes.Failed;
				newFactory.Save();
				filterControl.FirePerformSearch();
				AssertEquals(3, BatchPoster.FilteredEPaymentQuotes.Count);
				Assert(BatchPoster.FilteredEPaymentQuotes.Cast<EPaymentQuote>().All(x => x.QU_Status == QuoteStatusCodes.Failed));
			}
		}

		public void TestRefreshButtonUpdatesQuotesSummaries()
		{
			SetupDataForPostingTest();
			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			using (var form = new PaymentBatchForm_ForTestOnly(BatchPoster))
			{
				form.Show();
				BatchPoster.APB_PaymentType = ReceiptTypes.EFT;
				BatchPoster.APB_AB = TestBank.PK;

				var checkExRateButton = form.Controls.Find("CheckExRateButton", true)[0] as ZButton;
				checkExRateButton.PerformClick();
				AssertEquals(3, BatchPoster.FilteredEPaymentQuotes.Count);
				AssertEquals(1, BatchPoster.EPaymentQuotesSummary.Count);

				var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
				var quotesInNewFactory = newFactory.Load<EPaymentQuote>(new ZQuery(AccEPaymentQuoteSchema.PK, BatchPoster.FilteredEPaymentQuotes.Select(x => x.PK)));
				quotesInNewFactory.First().QU_Status = QuoteStatusCodes.Failed;
				newFactory.Save();

				AssertEquals(1, BatchPoster.EPaymentQuotesSummary.Count);
				var refreshButton = form.Controls.Find("RefreshButton", true).First() as ZButton;

				refreshButton.PerformClick();
				AssertEquals(2, BatchPoster.EPaymentQuotesSummary.Count);

				quotesInNewFactory.First().QU_Status = QuoteStatusCodes.Queued;
				newFactory.Save();
				refreshButton.PerformClick();
				AssertEquals(1, BatchPoster.EPaymentQuotesSummary.Count);
			}
		}

		public void TestRefreshButtonUpdatesLatestReceivedBeneficiariesTime()
		{
			var request1 = Factory.NewWithValidTestData<AccEPaymentBeneficiaryRequest>();
			request1.ABR_GC_Company = GlbCompany.CurrentCompany.PK;
			request1.ABR_Status = EPaymentStatusCodes.BeneficiaryRequest.Received;
			request1.ABR_SystemCreateTimeUtc = new ZDateTime(2021, 8, 15);
			request1.ABR_LastResponseReceivedUtc = new ZDateTime(2021, 8, 15);
			request1.ABR_SystemCreateUser = "AAA";

			var request2 = Factory.NewWithValidTestData<AccEPaymentBeneficiaryRequest>();
			request2.ABR_GC_Company = GlbCompany.CurrentCompany.PK;
			request2.ABR_Status = EPaymentStatusCodes.BeneficiaryRequest.Received;
			request2.ABR_SystemCreateTimeUtc = new ZDateTime(2021, 8, 16);
			request2.ABR_LastResponseReceivedUtc = new ZDateTime(2021, 8, 16);
			request2.ABR_SystemCreateUser = "BBB";

			var request3 = Factory.NewWithValidTestData<AccEPaymentBeneficiaryRequest>();
			request3.ABR_GC_Company = TestObjectCreator.NonCurrentCompany.PK;
			request3.ABR_Status = EPaymentStatusCodes.BeneficiaryRequest.Received;
			request3.ABR_SystemCreateTimeUtc = new ZDateTime(2021, 8, 18);
			request3.ABR_LastResponseReceivedUtc = new ZDateTime(2021, 8, 18);
			request3.ABR_SystemCreateUser = "CCC";

			var request4 = Factory.NewWithValidTestData<AccEPaymentBeneficiaryRequest>();
			request4.ABR_GC_Company = GlbCompany.CurrentCompany.PK;
			request4.ABR_Status = EPaymentStatusCodes.BeneficiaryRequest.Error;
			request4.ABR_ErrorDescription = "Some error text here";
			request4.ABR_SystemCreateTimeUtc = new ZDateTime(2021, 8, 17);
			request4.ABR_LastResponseReceivedUtc = new ZDateTime(2021, 8, 17);
			request4.ABR_SystemCreateUser = TestObjectCreator.Staff.GS_Code;

			Factory.Save();

			SetupDataForPostingTest();

			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			using (var form = new PaymentBatchForm_ForTestOnly(BatchPoster))
			{
				form.Show();
				BatchPoster.APB_PaymentType = ReceiptTypes.EFT;
				BatchPoster.APB_AB = TestBank.PK;
				var bankEPaymentTabControl = form.Controls.Find("TabControlBankAndEPayment", true).First() as ZTabControl;
				bankEPaymentTabControl.SelectedIndex = 1;
				var refreshButton = form.Controls.Find("RefreshButton", true).First() as ZButton;
				Assert("Precondition", refreshButton.Visible);
				Assert("Precondition", refreshButton.Enabled);

				AssertEquals(request4.PK, BatchPoster.MatchEPaymentRecipients.CurrentRequest.PK);
				AssertEquals(request2.PK, BatchPoster.MatchEPaymentRecipients.LastReceivedRequest.PK);

				refreshButton.PerformClick();
				AssertEquals(request4.PK, BatchPoster.MatchEPaymentRecipients.CurrentRequest.PK);
				AssertEquals(request2.PK, BatchPoster.MatchEPaymentRecipients.LastReceivedRequest.PK);

				var request5 = Factory.NewWithValidTestData<AccEPaymentBeneficiaryRequest>();
				request5.ABR_GC_Company = GlbCompany.CurrentCompany.PK;
				request5.ABR_Status = EPaymentStatusCodes.BeneficiaryRequest.Received;
				request5.ABR_SystemCreateTimeUtc = new ZDateTime(2021, 8, 18);
				request5.ABR_LastResponseReceivedUtc = new ZDateTime(2021, 8, 18);
				request5.ABR_SystemCreateUser = TestObjectCreator.Staff.GS_Code;
				Factory.Save();

				refreshButton.PerformClick();
				AssertEquals(request5.PK, BatchPoster.MatchEPaymentRecipients.CurrentRequest.PK);
				AssertEquals(request5.PK, BatchPoster.MatchEPaymentRecipients.LastReceivedRequest.PK);
			}
		}

		[TestDate(2021, 8, 18)]
		public void TestSyncRecipientsButtonClicked()
		{
			SetupDataForPostingTest();
			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			using (var form = new PaymentBatchForm_ForTestOnly(BatchPoster))
			{
				form.Show();
				BatchPoster.APB_PaymentType = ReceiptTypes.EFT;
				BatchPoster.APB_AB = TestBank.PK;
				var bankEPaymentTabControl = form.Controls.Find("TabControlBankAndEPayment", true).First() as ZTabControl;
				bankEPaymentTabControl.SelectedIndex = 1;
				var syncRecipientsButton = form.Controls.Find("SyncRecipientsButton", true)[0] as ZButton;
				Assert("Precondition", syncRecipientsButton.Visible);
				Assert("Precondition", syncRecipientsButton.Enabled);

				syncRecipientsButton.PerformClick();
				AssertEquals("In order to request OFX Recipient List, you must have a registered OFX account. If you already have an account, please ensure that you have configured an OFX E-Payment Account in the Bank Account Maintenance module and that your staff profile is registered and authorized on this Bank Account.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
				bankAccount.AB_GC = GlbCompany.CurrentCompany.PK;
				bankAccount.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.EPA;
				Factory.Save();
				syncRecipientsButton.PerformClick();
				AssertEquals("In order to request OFX Recipient List, you must have a registered OFX account. If you already have an account, please ensure that you have configured an OFX E-Payment Account in the Bank Account Maintenance module and that your staff profile is registered and authorized on this Bank Account.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				var token = Factory.NewWithValidTestData<AccEPaymentStaffToken>();
				token.TK_GC = GlbCompany.CurrentCompany.PK;
				token.TK_GS_NKStaffCode = GlbStaff.CurrentUser.GS_Code;
				token.TK_Status = AccEPaymentStaffTokenLookups.StatusCodes.NotAuthorised;
				Factory.Save();
				syncRecipientsButton.PerformClick();
				AssertEquals(@"You need to authorize your OFX user account before proceeding.
Please note that the authorization process can take up to several minutes.
Would you like to authorize your account now?", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(WebUrlLauncher.LastUrlLaunched.StartsWith(AccountingMasterFilesRegistry.Instance.OFXOAuthLoginWebURL.Value));

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				token.TK_ExpiryUtc = new ZDateTime(2021, 8, 15);
				token.TK_Status = AccEPaymentStaffTokenLookups.StatusCodes.Authorised;
				Factory.Save();
				syncRecipientsButton.PerformClick();
				AssertEquals(@"You need to authorize your OFX user account before proceeding.
Please note that the authorization process can take up to several minutes.
Would you like to authorize your account now?", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				token.TK_ExpiryUtc = new ZDateTime(2021, 10, 15);
				Factory.Save();

				var request1 = Factory.NewWithValidTestData<AccEPaymentBeneficiaryRequest>();
				request1.ABR_GC_Company = GlbCompany.CurrentCompany.PK;
				request1.ABR_Status = EPaymentStatusCodes.BeneficiaryRequest.Queued;
				Factory.Save();
				syncRecipientsButton.PerformClick();
				AssertEquals("A request has already been generated for the recipients list", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				request1.ABR_Status = EPaymentStatusCodes.BeneficiaryRequest.Requested;
				Factory.Save();
				syncRecipientsButton.PerformClick();
				AssertEquals("A request has already been sent to OFX for a recipients list. This may take up to several minutes to receive. Are you sure you wish to generate another request?", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				var requests = Factory.Load<AccEPaymentBeneficiaryRequest>(new ZQuery());
				var beforeCount = requests.Length;
				syncRecipientsButton.PerformClick();
				AssertEquals("Recipients list requested from third party provider OFX. This may take several minutes to receive.", UnitTestUserNotification.Instance.LastMessage.Text);
				requests = Factory.Load<AccEPaymentBeneficiaryRequest>(new ZQuery());
				var afterCount = requests.Length;
				AssertEquals("Expect 1 new request being created", beforeCount + 1, afterCount);
			}
		}

		public void TestPaymentWithQueuedQuotesPreventsQueuingMore()
		{
			SetupDataForPostingTest();
			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			using (var form = new PaymentBatchForm_ForTestOnly(BatchPoster))
			{
				form.Show();
				BatchPoster.APB_PaymentType = ReceiptTypes.EFT;
				BatchPoster.APB_AB = TestBank.PK;

				var saveAsDraftButton = form.Controls.Find("SaveAsDraftButton", true)[0] as ZButton;
				saveAsDraftButton.PerformClick();

				var payments = BatchPoster.PaymentApprovalCollection.Cast<PaymentApprovalBase>().ToArray();
				var quote1 = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Queued, payments[0]);
				var quote2 = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Queued, payments[1]);
				var quote3 = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Queued, payments[2]);
				Factory.Save();
				BatchPoster.FilteredEPaymentQuotes.Reload(true);

				var expectedMessage = @"All of the payments in this batch already have active Quote Requests.
No new Quotes have been requested";

				AssertEquals(3, BatchPoster.FilteredEPaymentQuotes.Count);
				var checkExRateButton = form.Controls.Find("CheckExRateButton", true)[0] as ZButton;
				checkExRateButton.PerformClick();
				AssertEquals(3, BatchPoster.FilteredEPaymentQuotes.Count);
				AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(UnitTestUserNotification.Instance.LastMessage.WasWarning);
				Assert(BatchPoster.FilteredEPaymentQuotes.Cast<EPaymentQuote>().All(x => x.QU_Status != QuoteStatusCodes.Discarded));
				UnitTestUserNotification.Instance.ClearMessages();

				quote2.QU_Status = QuoteStatusCodes.Received;
				quote2.QU_LastResponseReceivedUtc = ZDateTime.Now;
				quote2.QU_ExchangeRate = 1;
				quote2.QU_ExchangeRateInverted = 1;
				quote2.QU_FromAmount = quote2.QU_ToAmount;
				quote2.QU_ProviderReference = "12345";
				Factory.Save();
				BatchPoster.FilteredEPaymentQuotes.Reload(true);

				checkExRateButton.PerformClick();
				AssertEquals(4, BatchPoster.FilteredEPaymentQuotes.Count);
				AssertNotEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(QuoteStatusCodes.Queued, quote1.QU_Status);
				AssertEquals(QuoteStatusCodes.Discarded, quote2.QU_Status);
				AssertEquals(QuoteStatusCodes.Queued, quote3.QU_Status);
			}
		}

		public void TestPaymentWithRequestedQuotesAllowsReQueuing()
		{
			SetupDataForPostingTest();
			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			using (var form = new PaymentBatchForm_ForTestOnly(BatchPoster))
			{
				form.Show();
				BatchPoster.APB_PaymentType = ReceiptTypes.EFT;
				BatchPoster.APB_AB = TestBank.PK;

				var saveAsDraftButton = form.Controls.Find("SaveAsDraftButton", true)[0] as ZButton;
				saveAsDraftButton.PerformClick();

				var payments = BatchPoster.PaymentApprovalCollection.Cast<PaymentApprovalBase>().ToArray();
				var quote1 = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Queued, payments[0]);
				var quote2 = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Queued, payments[1]);
				var quote3 = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Requested, payments[2]);
				Factory.Save();
				BatchPoster.FilteredEPaymentQuotes.Reload(true);

				var expectedConfirmationMessage = "This batch contains E-Quotes in Requested status. Response may take up to several minutes to be received. If you generate new requests, then the previous requests will be discarded. Are you sure you want to proceed?";
				var expectedMessage = "E-Quote requests generated to service provider OFX. Response may take from a few moments up to several minutes to be received.";

				AssertEquals(3, BatchPoster.FilteredEPaymentQuotes.Count);
				UnitTestUserNotification.Instance.ClearMessages();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				var checkExRateButton = form.Controls.Find("CheckExRateButton", true)[0] as ZButton;
				checkExRateButton.PerformClick();

				AssertEquals(expectedConfirmationMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(3, BatchPoster.FilteredEPaymentQuotes.Count);

				UnitTestUserNotification.Instance.ClearMessages();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				checkExRateButton.PerformClick();

				AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(4, BatchPoster.FilteredEPaymentQuotes.Count);
				AssertEquals(1, BatchPoster.FilteredEPaymentQuotes.Cast<EPaymentQuote>().Count(x => x.QU_Status == QuoteStatusCodes.Discarded));
				AssertEquals(3, BatchPoster.FilteredEPaymentQuotes.Cast<EPaymentQuote>().Count(x => x.QU_Status == QuoteStatusCodes.Queued));
			}
		}

		public void TestAcceptQuotesButton_Success()
		{
			SetupDataForPostingTest();
			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			using (var form = new PaymentBatchForm_ForTestOnly(BatchPoster))
			{
				form.Show();
				BatchPoster.APB_PaymentType = ReceiptTypes.EFT;
				BatchPoster.APB_AB = TestBank.PK;

				form.AcceptQuotesButton_ForTestOnly(null, null);
				AssertEquals(UnitTestUserNotification.Instance.LastMessage.Text, "Please save the Payment Batch before Accepting Quotes.");
				UnitTestUserNotification.Instance.ClearMessages();
				Factory.Save();

				var payments = BatchPoster.PaymentApprovalCollection.Cast<APPaymentApprovalWithoutAuthorisation>().ToArray();
				var quoteCreator = new TestObjectCreator(new BusinessObjectFactory());
				var quote1 = quoteCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Received, payments[0]);
				quote1.QU_FromAmount = 500;
				var quote2 = quoteCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Received, payments[1]);
				quote2.QU_FromAmount = 1000;
				var quote3 = quoteCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Received, payments[2]);
				quote3.QU_FromAmount = 2000;
				quoteCreator.Factory.Save();

				form.AcceptQuotesButton_ForTestOnly(null, null);

				AssertEquals(QuoteStatusCodes.Accepted, quote1.QU_Status);
				AssertEquals(QuoteStatusCodes.Accepted, quote2.QU_Status);
				AssertEquals(QuoteStatusCodes.Accepted, quote3.QU_Status);
				AssertEquals("Quotes have been accepted. Payment Batch form will be reloaded to update details.", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(BatchPoster.PaymentApprovalCollection.Cast<PaymentApprovalBase>().All(x => !x.IsPosted));

				var reloadedForm = Application.OpenForms.OfType<PaymentBatchForm>().Single();
				AssertNotNull(reloadedForm);
				AssertNotEquals("Form reopened", form, reloadedForm);
				reloadedForm.Close();
			}
		}

		public void TestAcceptQuotesButton_NoQuotes()
		{
			SetupDataForPostingTest();
			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			using (var form = new PaymentBatchForm_ForTestOnly(BatchPoster))
			{
				form.Show();
				BatchPoster.APB_PaymentType = ReceiptTypes.EFT;
				BatchPoster.APB_AB = TestBank.PK;

				Factory.Save();

				AssertEquals("Precondition - no quotes on batch", 0, BatchPoster.EPaymentQuotesSummary.Count);
				form.AcceptQuotesButton_ForTestOnly(null, null);
				AssertEquals(@"There are payments on this batch that do not have an active quote.
Please click 'Check E-Pay rate' to create quotes for these payments.", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(UnitTestUserNotification.Instance.LastMessage.WasWarning);
			}
		}

		public void TestAcceptQuotesButton_BadStatus()
		{
			SetupDataForPostingTest();
			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			using (var form = new PaymentBatchForm_ForTestOnly(BatchPoster))
			{
				form.Show();
				BatchPoster.APB_PaymentType = ReceiptTypes.EFT;
				BatchPoster.APB_AB = TestBank.PK;

				Factory.Save();

				var payments = BatchPoster.PaymentApprovalCollection.Cast<APPaymentApprovalWithoutAuthorisation>().ToArray();
				var quoteCreator = new TestObjectCreator(new BusinessObjectFactory());
				var quote0_1 = quoteCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Discarded, payments[0]);
				var quote1_1 = quoteCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Error, payments[1]);
				var quote2_1 = quoteCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Received, payments[2]);
				quoteCreator.Factory.Save();

				form.AcceptQuotesButton_ForTestOnly(null, null);
				AssertEquals(@"There are payments on this batch that do not have an active quote.
Please click 'Check E-Pay rate' to create quotes for these payments.", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(UnitTestUserNotification.Instance.LastMessage.WasWarning);
				AssertNotEquals(QuoteStatusCodes.Accepted, quote0_1.QU_Status);
				AssertNotEquals(QuoteStatusCodes.Accepted, quote1_1.QU_Status);
				AssertNotEquals(QuoteStatusCodes.Accepted, quote2_1.QU_Status);
				UnitTestUserNotification.Instance.ClearMessages();

				var quote0_2 = quoteCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Received, payments[0]);
				quoteCreator.Factory.Save();

				form.AcceptQuotesButton_ForTestOnly(null, null);
				AssertEquals("E-Quote can be accepted only when it is in Received status, has a Provider Reference and the associated payment is not yet posted", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertNotEquals(QuoteStatusCodes.Accepted, quote0_1.QU_Status);
				AssertNotEquals(QuoteStatusCodes.Accepted, quote1_1.QU_Status);
				AssertNotEquals(QuoteStatusCodes.Accepted, quote2_1.QU_Status);
				UnitTestUserNotification.Instance.ClearMessages();

				quote1_1.QU_Status = QuoteStatusCodes.Discarded;
				var quote1_2 = quoteCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Received, payments[1]);
				quoteCreator.Factory.Save();

				form.AcceptQuotesButton_ForTestOnly(null, null);

				AssertNotEquals(QuoteStatusCodes.Accepted, quote0_1.QU_Status);
				AssertNotEquals(QuoteStatusCodes.Accepted, quote1_1.QU_Status);
				AssertEquals(QuoteStatusCodes.Accepted, quote0_2.QU_Status);
				AssertEquals(QuoteStatusCodes.Accepted, quote1_2.QU_Status);
				AssertEquals(QuoteStatusCodes.Accepted, quote2_1.QU_Status);

				var reloadedForm = Application.OpenForms.OfType<PaymentBatchForm>().Single();
				AssertNotNull(reloadedForm);
				AssertNotEquals("Form reopened", form, reloadedForm);
				reloadedForm.Close();
			}
		}

		public void TestAcceptQuotesButton_InvalidQuote_NoProviderReference()
		{
			AssertAcceptQuotesButton_InvalidQuote((quote) => quote.QU_ProviderReference = ZString.Empty);
		}

		public void TestAcceptQuotesButton_InvalidQuote_MismatchedDetails()
		{
			AssertAcceptQuotesButton_InvalidQuote((quote) => quote.QU_RX_NKFromCurrency = CurrencyCodes.Belgium);
		}

		void AssertAcceptQuotesButton_InvalidQuote(Action<EPaymentQuote> makeQuoteInvalid)
		{
			SetupDataForPostingTest();
			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			using (var form = new PaymentBatchForm_ForTestOnly(BatchPoster))
			{
				form.Show();
				BatchPoster.APB_PaymentType = ReceiptTypes.EFT;
				BatchPoster.APB_AB = TestBank.PK;

				Factory.Save();

				var payments = BatchPoster.PaymentApprovalCollection.Cast<APPaymentApprovalWithoutAuthorisation>().ToArray();
				var quoteCreator = new TestObjectCreator(new BusinessObjectFactory());
				var quote1 = quoteCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Received, payments[0]);
				var quote2 = quoteCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Received, payments[1]);
				var quote3 = quoteCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Received, payments[2]);
				makeQuoteInvalid(quote1);
				quoteCreator.Factory.Save();

				form.AcceptQuotesButton_ForTestOnly(null, null);
				AssertEquals("E-Quote can be accepted only when it is in Received status, has a Provider Reference and the associated payment is not yet posted", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertNotEquals(QuoteStatusCodes.Accepted, quote1.QU_Status);
				AssertNotEquals(QuoteStatusCodes.Accepted, quote2.QU_Status);
				AssertNotEquals(QuoteStatusCodes.Accepted, quote3.QU_Status);
			}
		}

		#endregion

		#region Process EPayment Quotes

		public void TestProcessEPaymentButton_Click_WhenAccountIsEpaAndUserIsNotRegistered()
		{
			SetupDataForPostingTest();
			var bankAccount = TestObjectCreator.CreateEPaymentBankAccount();
			BatchPoster.APB_AB = bankAccount.PK;
			Factory.Save();

			var payments = BatchPoster.PaymentApprovalCollection.Cast<PaymentApprovalBase>().ToList();
			var quote1 = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Accepted, payments[0]);
			quote1.QU_FeeAmount = 10;
			var quote2 = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Accepted, payments[1]);
			quote2.QU_FeeAmount = 20;
			var quote3 = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Accepted, payments[2]);
			quote3.QU_FeeAmount = 30;

			Factory.Save();
			payments.ForEach(x => x.RefreshQuotes());

			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			using (var form = new PaymentBatchForm_ForTestOnly(BatchPoster))
			{
				form.Show();
				Application.DoEvents();

				UnitTestUserNotification.Instance.ClearMessages();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				form.ProcessEPaymentsButton_ForTestOnly.PerformClick();
				AssertEquals("Your staff profile is not currently registered in the list of users who can use this Bank Account. To manage users, edit the Bank Account and select “Manage Users”.", UnitTestUserNotification.Instance.LastMessage.Text);

				var bankEPaymentTabControl = form.Controls.Find("TabControlBankAndEPayment", true).First() as ZTabControl;
				AssertEquals("Bank Detail tab page should remain in focus if E-Payments can't be processed", bankEPaymentTabControl.SelectedIndex, 0);

				var checkExRateButton = form.Controls.Find("CheckExRateButton", true)[0] as ZButton;
				checkExRateButton.PerformClick();
				AssertEquals("Your staff profile is not currently registered in the list of users who can use this Bank Account. To manage users, edit the Bank Account and select “Manage Users”.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestProcessEPaymentButton_Click_WhenAccountIsEpaAndUserIsUnauthorized()
		{
			SetupDataForPostingTest();
			var bankAccount = TestObjectCreator.CreateEPaymentBankAccount();
			TestObjectCreator.CreateEPaymentStaffToken(bankAccount.PK, ZDateTime.Empty, Env.CurrentUser.Initials, AccEPaymentStaffTokenLookups.StatusCodes.Pending);
			BatchPoster.APB_AB = bankAccount.PK;
			Factory.Save();

			var payments = BatchPoster.PaymentApprovalCollection.Cast<PaymentApprovalBase>().ToList();
			payments.ForEach(x => PrepareEPaymentAccountDetailsCollection(x.PayeeOrganisation));

			var quote1 = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Accepted, payments[0]);
			quote1.QU_FeeAmount = 10;
			var quote2 = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Accepted, payments[1]);
			quote2.QU_FeeAmount = 20;
			var quote3 = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Accepted, payments[2]);
			quote3.QU_FeeAmount = 30;

			Factory.Save();
			payments.ForEach(x => x.RefreshQuotes());

			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			using (var form = new PaymentBatchForm_ForTestOnly(BatchPoster))
			{
				form.Show();
				Application.DoEvents();

				UnitTestUserNotification.Instance.ClearMessages();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

				form.ProcessEPaymentsButton_ForTestOnly.PerformClick();
				AssertEquals(@"You need to authorize your OFX user account before proceeding.
Please note that the authorization process can take up to several minutes.
Would you like to authorize your account now?", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessages();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				var checkExRateButton = form.Controls.Find("CheckExRateButton", true)[0] as ZButton;
				checkExRateButton.PerformClick();
				AssertEquals(@"You need to authorize your OFX user account before proceeding.
Please note that the authorization process can take up to several minutes.
Would you like to authorize your account now?", UnitTestUserNotification.Instance.LastMessage.Text);

				var bankEPaymentTabControl = form.Controls.Find("TabControlBankAndEPayment", true).First() as ZTabControl;
				AssertEquals("Bank Detail tab page should remain in focus if E-Payments can't be processed", bankEPaymentTabControl.SelectedIndex, 0);

				var oAuthURL = AccountingMasterFilesRegistry.Instance.OFXOAuthLoginWebURL.Value;
				var callbackURL = AccountingMasterFilesRegistry.Instance.WTCCallbackSiteWebURL.Value;
				var encryptedStateBeginAt = WebUrlLauncher.LastUrlLaunched.IndexOf("&state=") + 7;
				var encryptedStateEndAt = WebUrlLauncher.LastUrlLaunched.IndexOf("&scope=");
				var encryptedState = WebUrlLauncher.LastUrlLaunched.Substring(encryptedStateBeginAt, encryptedStateEndAt - encryptedStateBeginAt);
				var ofxSecret = (new AESCrypto()).DecryptStringAES(AccountingMasterFilesRegistry.Instance.OFXEncryptionKey.Value, AESCrypto.RANDOM_SHAREDSECRET);
				AssertEquals("EDIDAT.EDI.E.EPA.payments.Insert.False.False", (new AESCrypto()).DecryptStringAES(encryptedState, ofxSecret));
				var expectedURL = string.Format("{0}?response_type=code&client_id=xwFiSN389IiPKBdqXROEyUFpcG3w6lM6&state={2}&scope=payments&redirect_uri={1}", oAuthURL, callbackURL, encryptedState);
				AssertEquals(expectedURL, WebUrlLauncher.LastUrlLaunched);

				var staffToken = Factory.Load<AccEPaymentStaffToken>(new ZQuery(AccEPaymentStaffTokenSchema.TK_GS_NKStaffCode, Env.CurrentUser.Initials)).FirstOrDefault();
				AssertNotNull(staffToken);
				AssertEquals(staffToken.TK_Status, "PND");
			}
		}

		public void TestProcessEPaymentButton_Click_WhenAccountIsEpaAndUserAuthorizationExpired()
		{
			SetupDataForPostingTest();
			var bankAccount = TestObjectCreator.CreateEPaymentBankAccount();
			TestObjectCreator.CreateEPaymentStaffToken(bankAccount.PK, ZDateTime.UtcNow.AddHours(-1), Env.CurrentUser.Initials, AccEPaymentStaffTokenLookups.StatusCodes.Authorised);
			BatchPoster.APB_AB = bankAccount.PK;
			Factory.Save();

			var payments = BatchPoster.PaymentApprovalCollection.Cast<PaymentApprovalBase>().ToList();
			payments.ForEach(x => PrepareEPaymentAccountDetailsCollection(x.PayeeOrganisation));

			var quote1 = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Accepted, payments[0]);
			quote1.QU_FeeAmount = 10;
			var quote2 = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Accepted, payments[1]);
			quote2.QU_FeeAmount = 20;
			var quote3 = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Accepted, payments[2]);
			quote3.QU_FeeAmount = 30;

			Factory.Save();
			payments.ForEach(x => x.RefreshQuotes());

			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			using (var form = new PaymentBatchForm_ForTestOnly(BatchPoster))
			{
				form.Show();
				Application.DoEvents();

				UnitTestUserNotification.Instance.ClearMessages();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

				form.ProcessEPaymentsButton_ForTestOnly.PerformClick();
				AssertEquals(@"You need to authorize your OFX user account before proceeding.
Please note that the authorization process can take up to several minutes.
Would you like to authorize your account now?", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessages();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				var checkExRateButton = form.Controls.Find("CheckExRateButton", true)[0] as ZButton;
				checkExRateButton.PerformClick();
				AssertEquals(@"You need to authorize your OFX user account before proceeding.
Please note that the authorization process can take up to several minutes.
Would you like to authorize your account now?", UnitTestUserNotification.Instance.LastMessage.Text);

				var bankEPaymentTabControl = form.Controls.Find("TabControlBankAndEPayment", true).First() as ZTabControl;
				AssertEquals("Bank Detail tab page should remain in focus if E-Payments can't be processed", bankEPaymentTabControl.SelectedIndex, 0);

				var oAuthURL = AccountingMasterFilesRegistry.Instance.OFXOAuthLoginWebURL.Value;
				var callbackURL = AccountingMasterFilesRegistry.Instance.WTCCallbackSiteWebURL.Value;
				var encryptedStateBeginAt = WebUrlLauncher.LastUrlLaunched.IndexOf("&state=") + 7;
				var encryptedStateEndAt = WebUrlLauncher.LastUrlLaunched.IndexOf("&scope=");
				var encryptedState = WebUrlLauncher.LastUrlLaunched.Substring(encryptedStateBeginAt, encryptedStateEndAt - encryptedStateBeginAt);
				var ofxSecret = (new AESCrypto()).DecryptStringAES(AccountingMasterFilesRegistry.Instance.OFXEncryptionKey.Value, AESCrypto.RANDOM_SHAREDSECRET);
				AssertEquals("EDIDAT.EDI.E.EPA.payments.Insert.False.False", (new AESCrypto()).DecryptStringAES(encryptedState, ofxSecret));
				var expectedURL = string.Format("{0}?response_type=code&client_id=xwFiSN389IiPKBdqXROEyUFpcG3w6lM6&state={2}&scope=payments&redirect_uri={1}", oAuthURL, callbackURL, encryptedState);
				AssertEquals(expectedURL, WebUrlLauncher.LastUrlLaunched);

				var staffToken = Factory.Load<AccEPaymentStaffToken>(new ZQuery(AccEPaymentStaffTokenSchema.TK_GS_NKStaffCode, Env.CurrentUser.Initials)).FirstOrDefault();
				AssertNotNull(staffToken);
				AssertEquals(staffToken.TK_Status, "PND");
			}
		}

		public void TestProcessEPayments_Success_WithFundingCurrency()
		{
			SetupDataForPostingTest();
			var bankAccount = TestObjectCreator.CreateEPaymentBankAccount();
			TestObjectCreator.CreateEPaymentStaffToken(bankAccount.PK, ZDateTime.UtcNow.AddHours(2), Env.CurrentUser.Initials, AccEPaymentStaffTokenLookups.StatusCodes.Authorised);
			BatchPoster.APB_AB = bankAccount.PK;
			BatchPoster.APB_AB_FundingBankAccount = TestObjectCreator.USDBankAccount.PK;
			TestObjectCreator.USD.RX_SubUnitRatio = 10;
			Factory.Save();

			var payments = BatchPoster.PaymentApprovalCollection.Cast<PaymentApprovalBase>().ToList();
			payments.ForEach(x => PrepareEPaymentAccountDetailsCollection(x.PayeeOrganisation));

			BatchPoster.PaymentApprovalCollection.ForEach(x => {
				var quote = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Accepted, (PaymentApprovalBase)x);
				quote.QU_RX_NKFromCurrency = "USD";
				quote.QU_FeeAmount = 2;
			});
			Factory.Save();

			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			using (var form = new PaymentBatchForm_ForTestOnly(BatchPoster))
			{
				form.Show();

				form.ProcessEPaymentsButton_ForTestOnly.PerformClick();
				AssertEquals(@"The FX transactions in this Payment Batch will be executed by a third party provider, OFX. CargoWise provides the messaging and information exchange only.

Review the details of the transactions to ensure they are correct.

Provider: OFX
USD Total: 218.6
Total in Funding Currency: 291.4 USD
Total Fees in Funding Currency: 6.0 USD
Total Cost in Funding Currency: 297.4 USD

The Exchange Rate used for each transaction within this Batch Payment is as per the accepted quote.
Total Cost is the amount you will be required to pay to OFX. The OS Amount of each transaction is the amount your recipients will receive*.
*Occasionally third-party intermediary banks may deduct a fee from your transfer before paying your recipient. This fee may vary, and OFX receives no portion of it.

By clicking ""Yes"", the FX transactions in this Batch become legally binding if accepted by OFX. Would you like to continue?

Please check the status of each transaction after clicking ""Yes"".
If an OFX quote has expired, OFX cannot process the FX transaction and CargoWise will display the status as ""DEC - Provider Declined"".
If you have any issues or questions about the transactions, please contact OFX directly.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestProcessEPayments_Success()
		{
			SetupDataForPostingTest();
			var bankAccount = TestObjectCreator.CreateEPaymentBankAccount();
			TestObjectCreator.CreateEPaymentStaffToken(bankAccount.PK, ZDateTime.UtcNow.AddHours(2), Env.CurrentUser.Initials, AccEPaymentStaffTokenLookups.StatusCodes.Authorised);
			BatchPoster.APB_AB = bankAccount.PK;

			var payments = BatchPoster.PaymentApprovalCollection.Cast<PaymentApprovalBase>().ToList();
			payments[0].AV_PayExRate = 1.1;
			payments[0].AV_RX_NKPaymentCurrency = CurrencyCodes.NewZealand;
			payments[0].AV_Amount = 103.4;
			payments[0].AV_Calc_LocalAmount = 94;
			Factory.Save();

			var creditors = payments.Select(x => x.PayeeOrganisation).ToList();
			var creditor_account_currencies = new List<string>()
				{
					CurrencyCodes.NewZealand,
					CurrencyCodes.UnitedStates,
					CurrencyCodes.UnitedStates
				};
			for (var i = 0; i < creditors.Count; i++)
			{
				PrepareEPaymentAccountDetailsCollection(creditors[i], EPaymentMethods.EPaymentViaOFX, creditor_account_currencies[i]);
			}
			Factory.Save();

			var quote1 = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Accepted, payments[0]);
			quote1.QU_FeeAmount = 10;
			quote1.QU_ExchangeRate = 0.9090909090909m;
			quote1.QU_ExchangeRateInverted = 1.1m;
			var quote2 = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Accepted, payments[1]);
			quote2.QU_FeeAmount = 20;
			quote2.QU_ExchangeRate = 1.3333333333333m;
			quote2.QU_ExchangeRateInverted = 0.75m;
			var quote3 = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Accepted, payments[2]);
			quote3.QU_FeeAmount = 30;
			quote3.QU_ExchangeRate = 1.3333333333333m;
			quote3.QU_ExchangeRateInverted = 0.75m;

			Factory.Save();
			payments.ForEach(x => x.RefreshQuotes());

			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			using (var form = new PaymentBatchForm_ForTestOnly(BatchPoster))
			{
				form.Show();

				var expectedMessage = @"The FX transactions in this Payment Batch will be executed by a third party provider, OFX. CargoWise provides the messaging and information exchange only.

Review the details of the transactions to ensure they are correct.

Provider: OFX
NZD Total: 103.40
USD Total: 148.05
Total in Funding Currency: 291.40 AUD
Total Fees in Funding Currency: 60.00 AUD
Total Cost in Funding Currency: 351.40 AUD

The Exchange Rate used for each transaction within this Batch Payment is as per the accepted quote.
Total Cost is the amount you will be required to pay to OFX. The OS Amount of each transaction is the amount your recipients will receive*.
*Occasionally third-party intermediary banks may deduct a fee from your transfer before paying your recipient. This fee may vary, and OFX receives no portion of it.

By clicking ""Yes"", the FX transactions in this Batch become legally binding if accepted by OFX. Would you like to continue?

Please check the status of each transaction after clicking ""Yes"".
If an OFX quote has expired, OFX cannot process the FX transaction and CargoWise will display the status as ""DEC - Provider Declined"".
If you have any issues or questions about the transactions, please contact OFX directly.";

				var expectedMessage2 = "E-Payment Deal requests generated to service provider OFX. Response may take from a few moments up to several minutes to be received.";

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				form.ProcessEPaymentsButton_ForTestOnly.PerformClick();
				AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				var dealQuery = GetDealQueryForBatchPoster();
				var deals = Factory.Load<AccEPaymentDeal>(dealQuery);
				AssertEquals(0, deals.Length);

				var bankEPaymentTabControl = form.Controls.Find("TabControlBankAndEPayment", true).First() as ZTabControl;
				AssertEquals("Before E-Payments have been processed, Bank Detail tab page should be focused", bankEPaymentTabControl.SelectedIndex, 0);

				foreach (APPaymentApprovalWithoutAuthorisation payment in BatchPoster.PaymentApprovalCollection)
				{
					AssertEquals("Payment shouldn't yet have a current deal", payment.CurrentDeal, null);
				}

				UnitTestUserNotification.Instance.ClearMessages();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.ProcessEPaymentsButton_ForTestOnly.PerformClick();

				deals = Factory.Load<AccEPaymentDeal>(dealQuery);
				AssertEquals(3, deals.Length);
				Assert(UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(expectedMessage));
				AssertEquals(expectedMessage2, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("After E-Payments have been processed, E-Payment tab page should be focused", bankEPaymentTabControl.SelectedIndex, 1);

				foreach (APPaymentApprovalWithoutAuthorisation payment in BatchPoster.PaymentApprovalCollection)
				{
					AssertEquals("Deal status should be QUE", payment.CurrentDeal.AED_Status, "QUE");
				}

				BatchPoster.LoadPayments();
				Assert(BatchPoster.PaymentApprovalCollection.Cast<PaymentApprovalBase>().All(x => !x.IsPosted));
			}
		}

		public void TestRefreshButtonRefreshesPaymentApprovalCollectionEPaymentStatus()
		{
			SetupDataForPostingTest();
			var bankAccount = TestObjectCreator.CreateEPaymentBankAccount();
			TestObjectCreator.CreateEPaymentStaffToken(bankAccount.PK, ZDateTime.UtcNow.AddHours(2), Env.CurrentUser.Initials, AccEPaymentStaffTokenLookups.StatusCodes.Authorised);
			BatchPoster.APB_AB = bankAccount.PK;

			var payments = BatchPoster.PaymentApprovalCollection.Cast<PaymentApprovalBase>().ToList();
			payments[0].AV_PayExRate = 1.1;
			payments[0].AV_RX_NKPaymentCurrency = CurrencyCodes.NewZealand;
			payments[0].AV_Amount = 103.4;
			payments[0].AV_Calc_LocalAmount = 94;
			Factory.Save();

			var creditors = payments.Select(x => x.PayeeOrganisation).ToList();
			var creditor_account_currencies = new List<string>()
				{
					CurrencyCodes.NewZealand,
					CurrencyCodes.UnitedStates,
					CurrencyCodes.UnitedStates
				};
			for (var i = 0; i < creditors.Count; i++)
			{
				PrepareEPaymentAccountDetailsCollection(creditors[i], EPaymentMethods.EPaymentViaOFX, creditor_account_currencies[i]);
			}
			Factory.Save();

			var quote1 = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Accepted, payments[0]);
			quote1.QU_FeeAmount = 10;
			quote1.QU_ExchangeRate = 0.9090909090909m;
			quote1.QU_ExchangeRateInverted = 1.1m;
			var quote2 = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Accepted, payments[1]);
			quote2.QU_FeeAmount = 20;
			quote2.QU_ExchangeRate = 1.3333333333333m;
			quote2.QU_ExchangeRateInverted = 0.75m;
			var quote3 = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Accepted, payments[2]);
			quote3.QU_FeeAmount = 30;
			quote3.QU_ExchangeRate = 1.3333333333333m;
			quote3.QU_ExchangeRateInverted = 0.75m;

			Factory.Save();
			payments.ForEach(x => x.RefreshQuotes());

			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			using (var form = new PaymentBatchForm_ForTestOnly(BatchPoster))
			{
				form.Show();

				var expectedMessage = @"The FX transactions in this Payment Batch will be executed by a third party provider, OFX. CargoWise provides the messaging and information exchange only.

Review the details of the transactions to ensure they are correct.

Provider: OFX
NZD Total: 103.40
USD Total: 148.05
Total in Funding Currency: 291.40 AUD
Total Fees in Funding Currency: 60.00 AUD
Total Cost in Funding Currency: 351.40 AUD

The Exchange Rate used for each transaction within this Batch Payment is as per the accepted quote.
Total Cost is the amount you will be required to pay to OFX. The OS Amount of each transaction is the amount your recipients will receive*.
*Occasionally third-party intermediary banks may deduct a fee from your transfer before paying your recipient. This fee may vary, and OFX receives no portion of it.

By clicking ""Yes"", the FX transactions in this Batch become legally binding if accepted by OFX. Would you like to continue?

Please check the status of each transaction after clicking ""Yes"".
If an OFX quote has expired, OFX cannot process the FX transaction and CargoWise will display the status as ""DEC - Provider Declined"".
If you have any issues or questions about the transactions, please contact OFX directly.";

				var expectedMessage2 = "E-Payment Deal requests generated to service provider OFX. Response may take from a few moments up to several minutes to be received.";

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.ProcessEPaymentsButton_ForTestOnly.PerformClick();

				var dealQuery = GetDealQueryForBatchPoster();
				var deals = Factory.Load<AccEPaymentDeal>(dealQuery);

				AssertEquals(3, deals.Length);
				Assert(UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(expectedMessage));
				AssertEquals(expectedMessage2, UnitTestUserNotification.Instance.LastMessage.Text);

				foreach (APPaymentApprovalWithoutAuthorisation payment in BatchPoster.PaymentApprovalCollection)
				{
					AssertEquals("Deal status should be QUE", payment.CurrentDeal.AED_Status, "QUE");
				}

				foreach (var deal in deals)
				{
					deal.AED_Status = "REQ";
				}

				Factory.Save();
				form.RefreshButton_ForTestOnly.PerformClick();

				foreach (APPaymentApprovalWithoutAuthorisation payment in BatchPoster.PaymentApprovalCollection)
				{
					AssertEquals("Deal status should be REQ", payment.CurrentDeal.AED_Status, "REQ");
				}
			}
		}

		public void TestUpdateAccountNameCaption()
		{
			SetupDataForPostingTest();
			var bankAccount = TestObjectCreator.CreateEPaymentBankAccount();
			TestObjectCreator.CreateEPaymentStaffToken(bankAccount.PK, ZDateTime.UtcNow.AddHours(2), Env.CurrentUser.Initials, AccEPaymentStaffTokenLookups.StatusCodes.Authorised);
			BatchPoster.APB_AB = bankAccount.PK;
			Factory.Save();

			var payments = BatchPoster.PaymentApprovalCollection.Cast<PaymentApprovalBase>().ToList();
			payments.ForEach(x => PrepareEPaymentAccountDetailsCollection(x.PayeeOrganisation));

			var quote1 = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Accepted, payments[0]);
			quote1.QU_FeeAmount = 10;
			var quote2 = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Accepted, payments[1]);
			quote2.QU_FeeAmount = 20;
			var quote3 = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Accepted, payments[2]);
			quote3.QU_FeeAmount = 30;

			Factory.Save();
			payments.ForEach(x => x.RefreshQuotes());

			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			using (var form = new PaymentBatchForm_ForTestOnly(BatchPoster))
			{
				form.Show();
				Application.DoEvents();

				var bankAccountGuidFindBox = form.Controls.Find("BankAccountGuidFindBox", true).First() as ZGuidFindBox;
				AssertEquals("Bank Account", bankAccountGuidFindBox.CaptionResourceString.Caption);

				form.PaymentBatchPoster_ForTestOnly.APB_PaymentType = ReceiptTypes.EPayment;
				AssertEquals("E-Payment Account", bankAccountGuidFindBox.CaptionResourceString.Caption);
			}
		}

		public void TestProcessEPayments_BatchHasChanges() => AssertProcessEPaymentsHasChanges(() => BatchPoster.HasChanges = true);
		public void TestProcessEPayments_BatchPaymentsHaveChanges() => AssertProcessEPaymentsHasChanges(() => BatchPoster.PaymentApprovalCollection.First().HasChanges = true);

		void AssertProcessEPaymentsHasChanges(Action addChanges)
		{
			SetupDataForPostingTest();
			var bankAccount = TestObjectCreator.CreateEPaymentBankAccount();
			TestObjectCreator.CreateEPaymentStaffToken(bankAccount.PK, ZDateTime.UtcNow.AddHours(2), Env.CurrentUser.Initials, AccEPaymentStaffTokenLookups.StatusCodes.Authorised);
			BatchPoster.APB_AB = bankAccount.PK;
			Factory.Save();

			var payments = BatchPoster.PaymentApprovalCollection.Cast<PaymentApprovalBase>().ToList();
			var quote1 = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Accepted, payments[0]);
			var quote2 = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Accepted, payments[1]);
			var quote3 = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Accepted, payments[2]);

			Factory.Save();
			payments.ForEach(x => x.RefreshQuotes());

			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			using (var form = new PaymentBatchForm_ForTestOnly(BatchPoster))
			{
				form.Show();

				addChanges();
				form.ProcessEPaymentsButton_ForTestOnly.PerformClick();
				AssertEquals("Please save your payment batch before processing E-Payments.", UnitTestUserNotification.Instance.LastMessage.Text);

				var deals = Factory.Load<AccEPaymentDeal>(GetDealQueryForBatchPoster());
				AssertEquals(0, deals.Length);
			}
		}

		public void TestProcessEPayments_NoActivePayments()
		{
			SetupDataForPostingTest();
			var bankAccount = TestObjectCreator.CreateEPaymentBankAccount();
			TestObjectCreator.CreateEPaymentStaffToken(bankAccount.PK, ZDateTime.UtcNow.AddHours(2), Env.CurrentUser.Initials, AccEPaymentStaffTokenLookups.StatusCodes.Authorised);
			BatchPoster.APB_AB = bankAccount.PK;
			Factory.Save();

			BatchPoster.PaymentApprovalCollection.ForEach(x => TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Accepted, (PaymentApprovalBase)x));
			Factory.Save();

			BatchPoster.PaymentApprovalCollection.ForEach(x => ((PaymentApprovalBase)x).PaymentQuotes.Reload(false));

			BatchPoster.PaymentApprovalCollection[0].AV_Status = PaymentApprovalStatus.Cancelled;
			BatchPoster.PaymentApprovalCollection[1].AV_Status = PaymentApprovalStatus.Posted;
			BatchPoster.PaymentApprovalCollection[2].AV_Status = PaymentApprovalStatus.Cancelled;
			Factory.Save();

			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			using (var form = new PaymentBatchForm_ForTestOnly(BatchPoster))
			{
				form.Show();

				form.ProcessEPaymentsButton_ForTestOnly.PerformClick();
				AssertEquals("All payments in this batch are either canceled or posted.", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(UnitTestUserNotification.Instance.LastMessage.WasWarning);
				var deals = Factory.Load<AccEPaymentDeal>(GetDealQueryForBatchPoster());
				AssertEquals(0, deals.Length);

				var bankEPaymentTabControl = form.Controls.Find("TabControlBankAndEPayment", true).First() as ZTabControl;
				AssertEquals("Bank Detail tab page should remain in focus if E-Payments can't be processed", bankEPaymentTabControl.SelectedIndex, 0);
			}
		}

		public void TestProcessEPayments_NoSecurityRight()
		{
			SetupDataForPostingTest();
			var bankAccount = TestObjectCreator.CreateEPaymentBankAccount();
			TestObjectCreator.CreateEPaymentStaffToken(bankAccount.PK, ZDateTime.UtcNow.AddHours(2), Env.CurrentUser.Initials, AccEPaymentStaffTokenLookups.StatusCodes.Authorised);
			BatchPoster.APB_AB = bankAccount.PK;
			Factory.Save();

			BatchPoster.ClearPaymentApprovalCollection_ForTestOnly();
			BatchPoster.LoadPayments();

			var payments = BatchPoster.PaymentApprovalCollection.Cast<PaymentApprovalBase>();
			payments.ForEach(x => TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Accepted, x));
			payments.First().ProcessEPaymentSecurityCheckPoint.IsAllowed = false;
			Factory.Save();

			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			using (var form = new PaymentBatchForm_ForTestOnly(BatchPoster))
			{
				form.Show();

				var expectedMessage = payments.First().ProcessEPaymentSecurityCheckPoint.ErrorMessageForNotAllowed;

				form.ProcessEPaymentsButton_ForTestOnly.PerformClick();
				AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(UnitTestUserNotification.Instance.LastMessage.WasError);
				var deals = Factory.Load<AccEPaymentDeal>(GetDealQueryForBatchPoster());
				AssertEquals(0, deals.Length);

				var bankEPaymentTabControl = form.Controls.Find("TabControlBankAndEPayment", true).First() as ZTabControl;
				AssertEquals("Bank Detail tab page should remain in focus if E-Payments can't be processed", bankEPaymentTabControl.SelectedIndex, 0);
			}
		}

		public void TestProcessEPayments_NoOFXAccount()
		{
			SetupDataForPostingTest();
			var bankAccount = TestObjectCreator.AUDBankAccount;
			BatchPoster.APB_AB = bankAccount.PK;
			Factory.Save();

			BatchPoster.PaymentApprovalCollection.ForEach(x => TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Accepted, (PaymentApprovalBase)x));
			Factory.Save();

			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			using (var form = new PaymentBatchForm_ForTestOnly(BatchPoster))
			{
				form.Show();

				form.ProcessEPaymentsButton_ForTestOnly.PerformClick();
				AssertEquals("Payment Bank Account must be an E-Payment Account in order to submit payment for electronic processing.", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(UnitTestUserNotification.Instance.LastMessage.WasError);
				var deals = Factory.Load<AccEPaymentDeal>(GetDealQueryForBatchPoster());
				AssertEquals(0, deals.Length);

				var bankEPaymentTabControl = form.Controls.Find("TabControlBankAndEPayment", true).First() as ZTabControl;
				AssertEquals("Bank Detail tab page should remain in focus if E-Payments can't be processed", bankEPaymentTabControl.SelectedIndex, 0);
			}
		}

		public void TestProcessEPayments_InvalidPayment_NoQuote() => AssertProcessEPayments_InvalidPayment((x) =>
		{
			x.PaymentQuotes.Reload(true);
			x.PaymentQuotes[0].QU_Status = QuoteStatusCodes.Discarded;
		});

		public void TestProcessEPayments_InvalidPayment_QuoteNotAccepted() => AssertProcessEPayments_InvalidPayment((x) =>
		{
			x.PaymentQuotes.Reload(true);
			x.PaymentQuotes[0].QU_Status = QuoteStatusCodes.Received;
		});

		public void TestProcessEPayments_InvalidPayment_PaymentNotApproved()
		{
			SetupDataForPostingTest();
			var bankAccount = TestObjectCreator.CreateEPaymentBankAccount();
			TestObjectCreator.CreateEPaymentStaffToken(bankAccount.PK, ZDateTime.UtcNow.AddHours(2), Env.CurrentUser.Initials, AccEPaymentStaffTokenLookups.StatusCodes.Authorised);
			BatchPoster.APB_AB = bankAccount.PK;
			Factory.Save();

			BatchPoster.PaymentApprovalCollection.ForEach(x => TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Accepted, (PaymentApprovalBase)x));

			var payments = BatchPoster.PaymentApprovalCollection.Cast<PaymentApprovalBase>().ToList();
			payments.ForEach(x => PrepareEPaymentAccountDetailsCollection(x.PayeeOrganisation));

			Factory.Save();

			BatchPoster.PaymentApprovalCollection.ForEach(x => ((PaymentApprovalBase)x).PaymentQuotes.Reload(false));

			(BatchPoster.PaymentApprovalCollection.First() as PaymentApprovalBase).AV_Status = PaymentApprovalStatus.AwaitingApproval;
			Factory.Save();

			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			using (var form = new PaymentBatchForm_ForTestOnly(BatchPoster))
			{
				form.Show();

				form.ProcessEPaymentsButton_ForTestOnly.PerformClick();
				AssertEquals("Only payments in Approved status can be submitted for processing.", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(UnitTestUserNotification.Instance.LastMessage.WasError);
				var deals = Factory.Load<AccEPaymentDeal>(GetDealQueryForBatchPoster());
				AssertEquals(0, deals.Length);

				var bankEPaymentTabControl = form.Controls.Find("TabControlBankAndEPayment", true).First() as ZTabControl;
				AssertEquals("Bank Detail tab page should remain in focus if E-Payments can't be processed", bankEPaymentTabControl.SelectedIndex, 0);
			}
		}

		void AssertProcessEPayments_InvalidPayment(Action<PaymentApprovalBase> makePaymentInvalid)
		{
			SetupDataForPostingTest();
			var bankAccount = TestObjectCreator.CreateEPaymentBankAccount();
			TestObjectCreator.CreateEPaymentStaffToken(bankAccount.PK, ZDateTime.UtcNow.AddHours(2), Env.CurrentUser.Initials, AccEPaymentStaffTokenLookups.StatusCodes.Authorised);
			BatchPoster.APB_AB = bankAccount.PK;
			Factory.Save();

			BatchPoster.PaymentApprovalCollection.ForEach(x => TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Accepted, (PaymentApprovalBase)x));

			var payments = BatchPoster.PaymentApprovalCollection.Cast<PaymentApprovalBase>().ToList();
			payments.ForEach(x => PrepareEPaymentAccountDetailsCollection(x.PayeeOrganisation));
			Factory.Save();

			makePaymentInvalid(BatchPoster.PaymentApprovalCollection.First() as PaymentApprovalBase);
			Factory.Save();

			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			using (var form = new PaymentBatchForm_ForTestOnly(BatchPoster))
			{
				form.Show();

				form.ProcessEPaymentsButton_ForTestOnly.PerformClick();
				AssertEquals("E-Payments can be processed only if an authorized OFX account is provided, Creditor Organizations are configured for E-Payment, payments are in Approved status and have corresponding E-Quotes in Accepted status.", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(UnitTestUserNotification.Instance.LastMessage.WasError);
				var deals = Factory.Load<AccEPaymentDeal>(GetDealQueryForBatchPoster());
				AssertEquals(0, deals.Length);

				var bankEPaymentTabControl = form.Controls.Find("TabControlBankAndEPayment", true).First() as ZTabControl;
				AssertEquals("Bank Detail tab page should remain in focus if E-Payments can't be processed", bankEPaymentTabControl.SelectedIndex, 0);
			}
		}

		#endregion

		#region Test View Payment Batch

		public void TestPaymentBatchModuleGrid_DoubleClick()
		{
			var batch = Factory.NewWithValidTestData<AccPaymentBatch>();
			batch.APB_Status = "CMP";
			Factory.Save();

			var controller = ZControllerFactory.Create(ControllerIDs.PaymentBatch);
			using (var form = controller.ShowEditForm(batch) as PaymentBatchForm)
			{
				AssertViewPaymentBatchCore(form);
			}

			batch.APB_Status = "CAN";
			Factory.Save();
			using (var form = controller.ShowEditForm(batch) as PaymentBatchForm)
			{
				AssertViewPaymentBatchCore(form);
			}

			batch.APB_Status = "WRK";
			Factory.Save();
			using (var form = controller.ShowEditForm(batch) as PaymentBatchForm)
			{
				AssertEquals("Edit AP Payment Batch", form.FormHeading);
			}
		}

		public void TestPaymentBatchGrid_DoubleClick()
		{
			PrepareForEditPaymentBatch();

			var controller = ZControllerFactory.Create(ControllerIDs.PaymentBatch);

			using (var form = controller.ShowEditForm(BatchPoster) as PaymentBatchForm)
			{
				form.PaymentBatchGrid_ForTestOnly.Select(0);
				var apPaymentEditForm = form.PaymentBatchGrid_DoubleClick_ForTestOnly() as PaymentApprovalWithAuthorisationForm;
				AssertNotNull(apPaymentEditForm);
				AssertEquals("View AP PAYMENT", apPaymentEditForm.FormHeading);
				apPaymentEditForm.Dispose();

				form.PaymentBatchGrid_ForTestOnly.UnSelect(0);
				form.PaymentBatchGrid_ForTestOnly.Select(1);
				apPaymentEditForm = form.PaymentBatchGrid_DoubleClick_ForTestOnly() as PaymentApprovalWithAuthorisationForm;
				AssertEquals("Edit AP PAYMENT", apPaymentEditForm.FormHeading);
				AssertEquals(false, apPaymentEditForm.PaymentDetailButton_ForTestOnly.Enabled);
				AssertEquals(false, apPaymentEditForm.RejectButton_ForTestOnly.Enabled);
				apPaymentEditForm.Dispose();

				form.PaymentBatchGrid_ForTestOnly.UnSelect(1);
				form.PaymentBatchGrid_ForTestOnly.Select(2);
				apPaymentEditForm = form.PaymentBatchGrid_DoubleClick_ForTestOnly() as PaymentApprovalWithAuthorisationForm;
				var paymentApprovalWithAuthorisation = apPaymentEditForm.BusinessEntity as PaymentApprovalWithAuthorisation;
				paymentApprovalWithAuthorisation.AV_Amount = 20m;
				apPaymentEditForm.FireSaveButton();
				var matchingForm = ZFormModaliser.ActiveForm as NewMatchGroupForm;
				var matchingBizO = matchingForm.BusinessEntity as APPaymentApprovalMatching;
				var newExxForm = (TransactionViewForm)matchingForm.ShowNewMiscTransactionForm_ForTestOnly(ZArchitecture.Core.TransactionTypes.ExchangeDifference);
				newExxForm.FireSaveButton();
				newExxForm.Dispose();
				AssertEquals("Balance should be 0", 0M, matchingBizO.Balance);
				matchingForm.FireSaveButton();

				AssertEquals("Edit AP PAYMENT", apPaymentEditForm.FormHeading);
				apPaymentEditForm.Dispose();
			}
		}

		public void TestPaymentBatchGrid_DoubleClick_ReloadPaymentBatchFormWhenAPPaymentProcessingFormClosed()
		{
			PrepareForEditPaymentBatch();

			var controller = ZControllerFactory.Create(ControllerIDs.PaymentBatch);

			using (var form = controller.ShowEditForm(BatchPoster) as PaymentBatchForm)
			{
				form.PaymentBatchGrid_ForTestOnly.Select(2);
				var apPaymentProcessing = form.PaymentBatchGrid_DoubleClick_ForTestOnly() as PaymentApprovalWithAuthorisationForm;
				var localAmount = form.PaymentBatchPoster_ForTestOnly.CurrencySummary.SummaryRows.GetSummaryRow("AUD").LocalAmount;
				AssertEquals(291.4m, localAmount);

				var paymentApprovalWithAuthorisation = apPaymentProcessing.BusinessEntity as PaymentApprovalWithAuthorisation;
				AssertEquals(103.4m, paymentApprovalWithAuthorisation.AV_Amount);
				paymentApprovalWithAuthorisation.AV_Amount = 20m;
				AssertEquals(20m, paymentApprovalWithAuthorisation.AV_Amount);
				apPaymentProcessing.FireSaveButton();

				var matchingForm = ZFormModaliser.ActiveForm as NewMatchGroupForm;
				var matchingBizO = matchingForm.BusinessEntity as APPaymentApprovalMatching;
				var newExxForm = (TransactionViewForm)matchingForm.ShowNewMiscTransactionForm_ForTestOnly(ZArchitecture.Core.TransactionTypes.ExchangeDifference);
				newExxForm.FireSaveButton();
				newExxForm.Close();

				AssertEquals("Balance should be 0", 0M, matchingBizO.Balance);

				matchingForm.FireSaveButton();
				matchingForm.Close();
				Assert(!form.IsDisposed);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				apPaymentProcessing.Close();
				Application.DoEvents();
				AssertEquals("This form will be reloaded since the payment approval has been changed.", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(form.IsDisposed);

				var reloadedFrom = Application.OpenForms.OfType<PaymentBatchForm>().Single();
				AssertNotNull(reloadedFrom);
				localAmount = reloadedFrom.PaymentBatchPoster_ForTestOnly.CurrencySummary.SummaryRows.GetSummaryRow("AUD").LocalAmount;
				AssertEquals(208m, localAmount);
				reloadedFrom.Close();
			}
		}

		public void TestPaymentBatchGrid_DoubleClick_AlertToSaveWithPosterHasChanges()
		{
			PrepareForEditPaymentBatch();

			var controller = ZControllerFactory.Create(ControllerIDs.PaymentBatch);

			using (var form = controller.ShowEditForm(BatchPoster) as PaymentBatchForm)
			{
				form.PaymentBatchPoster_ForTestOnly.HasChanges = true;
				form.PaymentBatchGrid_ForTestOnly.Select(2);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				var apPaymentProcessing = form.PaymentBatchGrid_DoubleClick_ForTestOnly() as PaymentApprovalWithAuthorisationForm;
				AssertEquals("You must save this form first. Would you like to save now?", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(form.PaymentBatchPoster_ForTestOnly.HasChanges);
				AssertNull(apPaymentProcessing);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				apPaymentProcessing = form.PaymentBatchGrid_DoubleClick_ForTestOnly() as PaymentApprovalWithAuthorisationForm;
				AssertEquals("You must save this form first. Would you like to save now?", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(!form.PaymentBatchPoster_ForTestOnly.HasChanges);
				AssertNotNull(apPaymentProcessing);
				apPaymentProcessing.Close();
			}
		}

		public void TestPaymentBatchGrid_WhenViewMenuItemClicked()
		{
			PrepareForEditPaymentBatch();

			var controller = ZControllerFactory.Create(ControllerIDs.PaymentBatch);

			using (var form = controller.ShowEditForm(BatchPoster) as PaymentBatchForm)
			{
				form.PaymentBatchGrid_ForTestOnly.Select(0);
				var apPaymentViewForm = form.PaymentBatchGrid_ViewMenuItemClick_ForTestOnly() as PaymentApprovalWithAuthorisationForm;
				AssertEquals("Show view form for posted payment", "View AP PAYMENT", apPaymentViewForm.FormHeading);
				apPaymentViewForm.Dispose();

				form.PaymentBatchGrid_ForTestOnly.UnSelect(0);
				form.PaymentBatchGrid_ForTestOnly.Select(1);
				apPaymentViewForm = form.PaymentBatchGrid_ViewMenuItemClick_ForTestOnly() as PaymentApprovalWithAuthorisationForm;
				AssertEquals("Show view form for canceled payment", "View AP PAYMENT", apPaymentViewForm.FormHeading);
				apPaymentViewForm.Dispose();

				form.PaymentBatchGrid_ForTestOnly.UnSelect(1);
				form.PaymentBatchGrid_ForTestOnly.Select(2);
				apPaymentViewForm = form.PaymentBatchGrid_ViewMenuItemClick_ForTestOnly() as PaymentApprovalWithAuthorisationForm;
				AssertEquals("Show view form for awaitingApproval payment", "View AP PAYMENT", apPaymentViewForm.FormHeading);
				apPaymentViewForm.Dispose();
			}
		}

		public void TestPaymentBatchGrid_ViewMenuItemClicked_WhenAPPaymentIsDeletedByAnotherUser()
		{
			PrepareForEditPaymentBatch();

			var controller = ZControllerFactory.Create(ControllerIDs.PaymentBatch);

			using (var form = controller.ShowEditForm(BatchPoster) as PaymentBatchForm)
			{
				var paymentApprovalPKs = BatchPoster.PaymentApprovalCollection.Select(x => x.PK);
				AssertEquals("Pre-condition: must exist", 3, paymentApprovalPKs.Count());

				form.PaymentBatchGrid_ForTestOnly.Select(0);

				var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
				foreach (var pk in paymentApprovalPKs)
				{
					var apPaymentApproval = newFactory.Load<PaymentApprovalWithAuthorisation>(pk);
					apPaymentApproval.Delete();
				}
				newFactory.Save();

				PaymentApprovalWithAuthorisationForm apPaymentViewForm = null;
				AssertNoExceptionThrown("Expect no exception for deleted Payment.", () =>
				{
					apPaymentViewForm = form.PaymentBatchGrid_ViewMenuItemClick_ForTestOnly() as PaymentApprovalWithAuthorisationForm;
				});
				AssertNull("Should not show view form since the payment has been deleted.", apPaymentViewForm);
			}
		}

		public void TestViewPaymentBatch_WhenBalanceEqualsToZero()
		{
			AssertViewPaymentBatchButtonsAndMenuItems(true);
		}

		public void TestViewPaymentBatch_WhenBalanceNotEqualsToZero()
		{
			AssertViewPaymentBatchButtonsAndMenuItems(false);
		}

		public void TestViewPaymentBatch_WhenNoneRelatedPaymentApproval()
		{
			var batch = Factory.NewWithValidTestData<AccPaymentBatch>();
			Factory.Save();
			var approvals = Factory.Load<AccPaymentApproval>(new ZQuery(AccPaymentApprovalSchema.AV_APB_PaymentBatch, batch.PK));
			AssertEquals("Percondition", 0, approvals.Length);

			var controller = ZControllerFactory.Create(ControllerIDs.PaymentBatch);
			using (var form = controller.ShowViewForm(batch) as PaymentBatchForm)
			{
				Application.DoEvents();
				AssertViewPaymentBatchCore(form);
			}
		}

		public void TestViewPaymentBatch_WhenAllowSendingEPayments()
		{
			AssertViewPaymentBatch_EPayments(true);
		}

		public void TestViewPaymentBatch_WhenNotAllowSendingEPayments()
		{
			AssertViewPaymentBatch_EPayments(false);
		}

		void AssertViewPaymentBatch_EPayments(bool isOFXEPaymentsEnabled)
		{
			SetupBalancedPaymentBatch();

			var controller = ZControllerFactory.Create(ControllerIDs.PaymentBatch);
			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, isOFXEPaymentsEnabled))
			using (var form = controller.ShowViewForm(BatchPoster) as PaymentBatchForm)
			{
				Application.DoEvents();

				var tabpageEPayments = form.Controls.Find("zTabPage2", true);
				var checkExRateButton = form.Controls.Find("CheckExRateButton", true);
				if (isOFXEPaymentsEnabled)
				{
					AssertEquals(1, tabpageEPayments.Length);
					AssertEquals(true, (tabpageEPayments[0] as ZTabPage).TabVisible);

					AssertEquals(1, checkExRateButton.Length);
					AssertEquals(true, (checkExRateButton[0] as ZButton).Visible);
					AssertEquals(false, (checkExRateButton[0] as ZButton).Enabled);
				}
				else
				{
					AssertEquals(0, tabpageEPayments.Length);

					AssertEquals(1, checkExRateButton.Length);
					AssertEquals(false, (checkExRateButton[0] as ZButton).Visible);
					AssertEquals(false, (checkExRateButton[0] as ZButton).Enabled);
				}

				AssertViewPaymentBatchCore(form);
			}
		}

		void AssertViewPaymentBatchCore(PaymentBatchForm form)
		{
			AssertEquals("View AP Payment Batch", form.FormHeading);

			AssertNull(form.ActionsMenuItem_ForTestOnly.MenuItems.FindByText("Authorization"));
			AssertNull(form.ActionsMenuItem_ForTestOnly.MenuItems.FindByText("Post Approvals"));
			AssertNull(form.ActionsMenuItem_ForTestOnly.MenuItems.FindByText("Cancel"));
			AssertNull(form.ActionsMenuItem_ForTestOnly.MenuItems.FindByText("Submit for Approval"));

			AssertNull(form.PaymentBatchGrid_ForTestOnly.ContextMenu.MenuItems.FindByText("Actio&ns"));

			var buttonsCanBeVisibleAndEnabled = new List<string>
			{
				"CancelPostingButton",
			};

			AssertNotNull(form);

			var tabControlBankAndEPayment = form.Controls.Find("TabControlBankAndEPayment", true)[0] as ZTabControl;
			AssertEquals("zTabPage1", tabControlBankAndEPayment.SelectedTab.Name);
			AssertZButton(form, buttonsCanBeVisibleAndEnabled);

			var saveButton = form.Controls.Find("SaveButton", true)[0];
			var saveAndCloseButton = form.Controls.Find("SaveAndCloseButton", true)[0];
			var postPaymentsAsPaymentApprovalsCheckBox = form.Controls.Find("PostPaymentsAsPaymentApprovalsCheckBox", true)[0];
			AssertEquals("Save", saveButton.Text);
			AssertEquals("Save && Close", saveAndCloseButton.Text);
			AssertEquals(false, postPaymentsAsPaymentApprovalsCheckBox.Visible);

			if (AccountingMasterFilesRegistry.Instance.EnableEPaymentFunctionality.Value.IsEPaymentEnabledForAnyProvider)
			{
				tabControlBankAndEPayment.SelectNextTabPage();
				var tabControlSummaryAndDetails = form.Controls.Find("TabControlSummaryAndDetails", true)[0] as ZTabControl;
				AssertEquals("zTabPage2", tabControlBankAndEPayment.SelectedTab.Name);
				AssertEquals("zTabPage3", tabControlSummaryAndDetails.SelectedTab.Name);
				AssertZButton(form, buttonsCanBeVisibleAndEnabled);

				tabControlSummaryAndDetails.SelectNextTabPage();
				AssertEquals("zTabPage2", tabControlBankAndEPayment.SelectedTab.Name);
				AssertEquals("zTabPage4", tabControlSummaryAndDetails.SelectedTab.Name);
				AssertZButton(form, buttonsCanBeVisibleAndEnabled);
			}
		}

		void AssertViewPaymentBatchButtonsAndMenuItems(bool balanceEqualsToZero)
		{
			SetupBalancedPaymentBatch(balanceEqualsToZero);
			BatchPoster.LoadPayments();

			var controller = ZControllerFactory.Create(ControllerIDs.PaymentBatch);
			using (var form = controller.ShowViewForm(BatchPoster) as PaymentBatchForm)
			{
				BatchPoster = form.BusinessEntity as APPaymentBatchPoster;
				AssertEquals(balanceEqualsToZero ? 0m : -100m, BatchPoster.PaymentApprovalCollection[0].MatchingBaseObject.Balance);

				Application.DoEvents();
				AssertViewPaymentBatchCore(form);

				AssertEquals("Percondition", ODisplayMode.ReadOnly, form.DisplayMode);

				form.PaymentBatchGrid_ForTestOnly.ContextMenu.ShowPopupMenu();
				AssertNull(form.PaymentBatchGrid_ForTestOnly.ContextMenu.MenuItems.FindByText("Remove"));
				AssertNull(form.PaymentBatchGrid_ForTestOnly.ContextMenu.MenuItems.FindByText("Edit Payment Organization Detail"));

				form.PaymentBatchGrid_ForTestOnly.SelectAllElements();
				AssertEquals("Percondition", 1, form.PaymentBatchGrid_ForTestOnly.SelectedElements.Length);

				form.MatchTransactionsGrid_ForTestOnly.SelectAllElements();
				form.MatchTransactionsGrid_ForTestOnly.ContextMenu.ShowPopupMenu();
				AssertEquals("Percondition", 4, form.MatchTransactionsGrid_ForTestOnly.SelectedElements.Length);
				AssertNull(form.MatchTransactionsGrid_ForTestOnly.ContextMenu.MenuItems.FindByText("Remove"));
				AssertNull(form.MatchTransactionsGrid_ForTestOnly.ContextMenu.MenuItems.FindByText("Add Overpayment"));
				AssertNull(form.MatchTransactionsGrid_ForTestOnly.ContextMenu.MenuItems.FindByText("Add Exchange Difference"));
				AssertNull(form.MatchTransactionsGrid_ForTestOnly.ContextMenu.MenuItems.FindByText("Add Discount"));
				AssertNull(form.MatchTransactionsGrid_ForTestOnly.ContextMenu.MenuItems.FindByText("View"));

				form.MatchTransactionsGrid_ForTestOnly.UnSelectAll();
				form.MatchTransactionsGrid_ForTestOnly.Select(0);
				form.MatchTransactionsGrid_ForTestOnly.ContextMenu.ShowPopupMenu();
				AssertEquals("Percondition", 1, form.MatchTransactionsGrid_ForTestOnly.SelectedElements.Length);
				AssertNull(form.MatchTransactionsGrid_ForTestOnly.ContextMenu.MenuItems.FindByText("Remove"));
				AssertNull(form.MatchTransactionsGrid_ForTestOnly.ContextMenu.MenuItems.FindByText("Add Overpayment"));
				AssertNull(form.MatchTransactionsGrid_ForTestOnly.ContextMenu.MenuItems.FindByText("Add Exchange Difference"));
				AssertNull(form.MatchTransactionsGrid_ForTestOnly.ContextMenu.MenuItems.FindByText("Add Discount"));
				AssertNotNull(form.MatchTransactionsGrid_ForTestOnly.ContextMenu.MenuItems.FindByText("View"));
			}
		}

		void AssertZButton(Control control, IEnumerable<string> buttonsCanBeVisibleAndEnabled, IEnumerable<string> buttonsSkipCheck = null)
		{
			if (control.Controls.Count > 0)
			{
				foreach (Control c in control.Controls)
				{
					if (c.GetType().IsSubclassOf(typeof(ZButton)) || c.GetType() == typeof(ZButton))
					{
						var button = c as ZButton;

						if (buttonsSkipCheck != null && buttonsSkipCheck.Contains(button.Name))
						{
							//Skip check those Button
						}
						else if (!buttonsCanBeVisibleAndEnabled.Contains(button.Name) && button.Visible && button.Enabled)
						{
							Fail(button.Name + " should be disabled");
						}
						else if (buttonsCanBeVisibleAndEnabled.Contains(button.Name) && (!button.Visible || !button.Enabled))
						{
							Fail(button.Name + " should be Visible & Enabled");
						}
					}
					else
					{
						AssertZButton(c, buttonsCanBeVisibleAndEnabled, buttonsSkipCheck);
					}
				}
			}
		}

		#endregion

		#region Test New PaymentBatch

		public void TestNewPaymentBatch_Controls()
		{
			SetupNewPaymentBatch();

			using (var form = new PaymentBatchForm_ForTestOnly(BatchPoster))
			{
				form.Show();
				Application.DoEvents();

				var saveButton = form.Controls.Find("SaveButton", true)[0];
				var saveAndCloseButton = form.Controls.Find("SaveAndCloseButton", true)[0];
				var postPaymentsAsPaymentApprovalsCheckBox = form.Controls.Find("PostPaymentsAsPaymentApprovalsCheckBox", true)[0];
				AssertEquals("&Post", saveButton.Text);
				AssertEquals("P&ost && Close", saveAndCloseButton.Text);
				AssertEquals(true, postPaymentsAsPaymentApprovalsCheckBox.Visible);

				AssertEquals("New AP Payment Batch Posting", form.FormHeading);

				AssertNull(form.ActionsMenuItem_ForTestOnly.MenuItems.FindByText("Authorization"));
				AssertNull(form.ActionsMenuItem_ForTestOnly.MenuItems.FindByText("Post Approvals"));
				AssertNull(form.ActionsMenuItem_ForTestOnly.MenuItems.FindByText("Cancel"));
				AssertNull(form.ActionsMenuItem_ForTestOnly.MenuItems.FindByText("Submit for Approval"));
			}
		}

		public void TestNewPaymentBatch_PaymentBatchGridMenuItemsAfterSaved()
		{
			SetupNewPaymentBatch();
			AssertEquals("Percondition", false, BatchPoster.IsInDatabase);

			using (var form = new PaymentBatchForm_ForTestOnly(BatchPoster))
			{
				form.Show();
				Application.DoEvents();

				form.PaymentBatchGrid_ForTestOnly.ContextMenu.ShowPopupMenu();
				AssertNotEquals("Percondition", -1, form.PaymentBatchGrid_ForTestOnly.CurrentRowIndex);
				AssertNotNull("Percondition", form.SelectedPayment_ForTestOnly);
				AssertNotEquals("Percondition", ODisplayMode.Browse, form.DisplayMode);
				AssertNotNull(form.PaymentBatchGrid_ForTestOnly.ContextMenu.MenuItems.FindByText("Remove"));
				AssertNotNull(form.PaymentBatchGrid_ForTestOnly.ContextMenu.MenuItems.FindByText("Edit Payment Organization Detail"));

				AssertNull(form.PaymentBatchGrid_ForTestOnly.ContextMenu.MenuItems.FindByText("Actio&ns"));

				var payment = BatchPoster.PaymentApprovalCollection[0];
				var paymentMatchingBaseObject = payment.MatchingBaseObject;
				var exchangeDifference = paymentMatchingBaseObject.GetMiscellaneousTransaction(TransactionTypes.ExchangeDifference);
				exchangeDifference.BindableOSAmount = 600m;
				paymentMatchingBaseObject.AddMiscellaneousTransaction(exchangeDifference);
				form.FApplyButton_ForTestOnly.PerformClick();
				Application.DoEvents();

				form.PaymentBatchGrid_ForTestOnly.ContextMenu.ShowPopupMenu();
				AssertNotEquals("Percondition", -1, form.PaymentBatchGrid_ForTestOnly.CurrentRowIndex);
				AssertNotNull("Percondition", form.SelectedPayment_ForTestOnly);
				AssertEquals("Percondition", ODisplayMode.Browse, form.DisplayMode);
				AssertNull(form.PaymentBatchGrid_ForTestOnly.ContextMenu.MenuItems.FindByText("Remove"));
				AssertNull(form.PaymentBatchGrid_ForTestOnly.ContextMenu.MenuItems.FindByText("Edit Payment Organization Detail"));

				AssertNull(form.PaymentBatchGrid_ForTestOnly.ContextMenu.MenuItems.FindByText("Actio&ns"));
			}
		}

		public void TestNewPaymentBatch_MatchTransactionsGridMenuItemsAfterSaved()
		{
			SetupNewPaymentBatch();
			AssertEquals("Percondition", false, BatchPoster.IsInDatabase);

			using (var form = new PaymentBatchForm_ForTestOnly(BatchPoster))
			{
				form.Show();
				Application.DoEvents();

				form.MatchTransactionsGrid_ForTestOnly.Select(0);
				form.MatchTransactionsGrid_ForTestOnly.ContextMenu.ShowPopupMenu();
				AssertEquals("Percondition", 1, form.MatchTransactionsGrid_ForTestOnly.SelectedElements.Length);
				AssertNotEquals("Percondition", 0m, form.SelectedPayment_ForTestOnly.MatchedTransactionsBalanceWithPaymentAmount);
				AssertNotEquals("Percondition", ODisplayMode.Browse, form.DisplayMode);
				AssertNotNull(form.MatchTransactionsGrid_ForTestOnly.ContextMenu.MenuItems.FindByText("Remove"));
				AssertNotNull(form.MatchTransactionsGrid_ForTestOnly.ContextMenu.MenuItems.FindByText("Add Overpayment"));
				AssertNotNull(form.MatchTransactionsGrid_ForTestOnly.ContextMenu.MenuItems.FindByText("Add Exchange Difference"));
				AssertNotNull(form.MatchTransactionsGrid_ForTestOnly.ContextMenu.MenuItems.FindByText("Add Discount"));
				AssertNotNull(form.MatchTransactionsGrid_ForTestOnly.ContextMenu.MenuItems.FindByText("View"));

				var payment = BatchPoster.PaymentApprovalCollection[0];
				var paymentMatchingBaseObject = payment.MatchingBaseObject;
				var exchangeDifference = paymentMatchingBaseObject.GetMiscellaneousTransaction(TransactionTypes.ExchangeDifference);
				exchangeDifference.BindableOSAmount = 600m;
				paymentMatchingBaseObject.AddMiscellaneousTransaction(exchangeDifference);
				form.MatchTransactionsGrid_ForTestOnly.Select(0);
				form.MatchTransactionsGrid_ForTestOnly.ContextMenu.ShowPopupMenu();
				AssertEquals("Percondition", 1, form.MatchTransactionsGrid_ForTestOnly.SelectedElements.Length);
				AssertEquals("Percondition", 0m, form.SelectedPayment_ForTestOnly.MatchedTransactionsBalanceWithPaymentAmount);
				AssertNotEquals("Percondition", ODisplayMode.Browse, form.DisplayMode);
				AssertNotNull(form.MatchTransactionsGrid_ForTestOnly.ContextMenu.MenuItems.FindByText("Remove"));
				AssertNull(form.MatchTransactionsGrid_ForTestOnly.ContextMenu.MenuItems.FindByText("Add Overpayment"));
				AssertNull(form.MatchTransactionsGrid_ForTestOnly.ContextMenu.MenuItems.FindByText("Add Exchange Difference"));
				AssertNull(form.MatchTransactionsGrid_ForTestOnly.ContextMenu.MenuItems.FindByText("Add Discount"));
				AssertNotNull(form.MatchTransactionsGrid_ForTestOnly.ContextMenu.MenuItems.FindByText("View"));

				form.MatchTransactionsGrid_ForTestOnly.SelectAllElements();
				form.MatchTransactionsGrid_ForTestOnly.ContextMenu.ShowPopupMenu();
				AssertEquals("Percondition", 2, form.MatchTransactionsGrid_ForTestOnly.SelectedElements.Length);
				AssertEquals("Percondition", 0m, form.SelectedPayment_ForTestOnly.MatchedTransactionsBalanceWithPaymentAmount);
				AssertNull(form.MatchTransactionsGrid_ForTestOnly.ContextMenu.MenuItems.FindByText("Remove"));
				AssertNull(form.MatchTransactionsGrid_ForTestOnly.ContextMenu.MenuItems.FindByText("Add Overpayment"));
				AssertNull(form.MatchTransactionsGrid_ForTestOnly.ContextMenu.MenuItems.FindByText("Add Exchange Difference"));
				AssertNull(form.MatchTransactionsGrid_ForTestOnly.ContextMenu.MenuItems.FindByText("Add Discount"));
				AssertNull(form.MatchTransactionsGrid_ForTestOnly.ContextMenu.MenuItems.FindByText("View"));

				form.FApplyButton_ForTestOnly.PerformClick();
				Application.DoEvents();

				form.MatchTransactionsGrid_ForTestOnly.Select(0);
				form.MatchTransactionsGrid_ForTestOnly.ContextMenu.ShowPopupMenu();
				AssertEquals("Percondition", ODisplayMode.Browse, form.DisplayMode);
				AssertEquals("Percondition", 1, form.MatchTransactionsGrid_ForTestOnly.SelectedElements.Length);
				AssertNull(form.MatchTransactionsGrid_ForTestOnly.ContextMenu.MenuItems.FindByText("Remove"));
				AssertNull(form.MatchTransactionsGrid_ForTestOnly.ContextMenu.MenuItems.FindByText("Add Overpayment"));
				AssertNull(form.MatchTransactionsGrid_ForTestOnly.ContextMenu.MenuItems.FindByText("Add Exchange Difference"));
				AssertNull(form.MatchTransactionsGrid_ForTestOnly.ContextMenu.MenuItems.FindByText("Add Discount"));
				AssertNotNull(form.MatchTransactionsGrid_ForTestOnly.ContextMenu.MenuItems.FindByText("View"));
			}
		}

		public void TestNewPaymentBatch_ButtonsAfterSuccessfullySaved_WhenAllowSendingEPayments_SavedAsDraft()
		{
			AssertNewPaymentBatch_ButtonsAfterSuccessfullySaved_WhenAllowSendingEPayments((form) =>
			{
				var saveAsDraftButton = form.Controls.Find("SaveAsDraftButton", true)[0] as ZButton;
				saveAsDraftButton.PerformClick();

				return true;
			});
		}

		public void TestNewPaymentBatch_ButtonsAfterSuccessfullySaved_WhenAllowSendingEPayments_CheckExRate()
		{
			AssertNewPaymentBatch_ButtonsAfterSuccessfullySaved_WhenAllowSendingEPayments((form) =>
			{
				var checkExRateButton = form.Controls.Find("CheckExRateButton", true)[0] as ZButton;
				checkExRateButton.PerformClick();

				return true;
			});
		}

		public void TestNewPaymentBatch_ButtonsAfterSuccessfullySaved_WhenAllowSendingEPayments_PostPaymentsAsPaymentApprovals()
		{
			AssertNewPaymentBatch_ButtonsAfterSuccessfullySaved_WhenAllowSendingEPayments((form) =>
			{
				form.PaymentBatchPoster_ForTestOnly.PostPaymentsAsPaymentApprovals = true;
				form.FApplyButton_ForTestOnly.PerformClick();

				return true;
			}, true);
		}

		public void TestNewPaymentBatch_ButtonsAfterSuccessfullySaved_WhenAllowSendingEPayments_PostPayments()
		{
			AssertNewPaymentBatch_ButtonsAfterSuccessfullySaved_WhenAllowSendingEPayments((form) =>
			{
				form.FApplyButton_ForTestOnly.PerformClick();

				AssertEquals("Percondition", true, BatchPoster.IsInDatabase);

				var tabControlBankAndEPayment = form.Controls.Find("TabControlBankAndEPayment", true)[0] as ZTabControl;
				var tabControlSummaryAndDetails = form.Controls.Find("TabControlSummaryAndDetails", true)[0] as ZTabControl;
				tabControlBankAndEPayment.SelectTab(0);
				AssertEquals("zTabPage1", tabControlBankAndEPayment.SelectedTab.Name);
				AssertZButton(form, new List<string> { "CancelPostingButton", "ProcessEPaymentsButton" });

				tabControlBankAndEPayment.SelectTab(1);
				tabControlSummaryAndDetails.SelectTab(0);
				AssertEquals("zTabPage2", tabControlBankAndEPayment.SelectedTab.Name);
				AssertEquals("zTabPage3", tabControlSummaryAndDetails.SelectedTab.Name);
				AssertZButton(form, new List<string> { "RefreshButton", "AcceptQuotesButton", "SyncRecipientsButton", "ProcessEPaymentsButton", "CancelPostingButton" });

				tabControlSummaryAndDetails.SelectTab(1);
				AssertEquals("zTabPage2", tabControlBankAndEPayment.SelectedTab.Name);
				AssertEquals("zTabPage4", tabControlSummaryAndDetails.SelectedTab.Name);
				AssertZButton(form, new List<string> { "AddButton", "FilterPropertyLockButton", "CancelPostingButton", "ProcessEPaymentsButton" });

				return false;
			}, true);
		}

		void AssertNewPaymentBatch_ButtonsAfterSuccessfullySaved_WhenAllowSendingEPayments(Func<PaymentBatchForm, bool> doFormFunc, bool balanceEqualsToZero = false)
		{
			SetupNewPaymentBatch();
			AssertEquals("Percondition", false, BatchPoster.IsInDatabase);

			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			using (var form = new PaymentBatchForm_ForTestOnly(BatchPoster))
			{
				form.Show();
				Application.DoEvents();

				if (balanceEqualsToZero)
				{
					var payment = BatchPoster.PaymentApprovalCollection[0];
					var paymentMatchingBaseObject = payment.MatchingBaseObject;
					var exchangeDifference = paymentMatchingBaseObject.GetMiscellaneousTransaction(TransactionTypes.ExchangeDifference);
					exchangeDifference.BindableOSAmount = 600m;
					paymentMatchingBaseObject.AddMiscellaneousTransaction(exchangeDifference);
					AssertEquals("Percondition", 0m, paymentMatchingBaseObject.Balance);
				}

				var ifContinueAssert = doFormFunc.Invoke(form);
				if (ifContinueAssert)
				{
					AssertEquals("Percondition", true, BatchPoster.IsInDatabase);

					var tabControlBankAndEPayment = form.Controls.Find("TabControlBankAndEPayment", true)[0] as ZTabControl;
					var tabControlSummaryAndDetails = form.Controls.Find("TabControlSummaryAndDetails", true)[0] as ZTabControl;
					tabControlBankAndEPayment.SelectTab(0);
					AssertEquals("zTabPage1", tabControlBankAndEPayment.SelectedTab.Name);
					AssertZButton(form, new List<string> { "CheckExRateButton", "CancelPostingButton", "ProcessEPaymentsButton" });

					tabControlBankAndEPayment.SelectTab(1);
					tabControlSummaryAndDetails.SelectTab(0);
					AssertEquals("zTabPage2", tabControlBankAndEPayment.SelectedTab.Name);
					AssertEquals("zTabPage3", tabControlSummaryAndDetails.SelectedTab.Name);
					AssertZButton(form, new List<string> { "RefreshButton", "AcceptQuotesButton", "SyncRecipientsButton", "ProcessEPaymentsButton", "CancelPostingButton" });

					tabControlSummaryAndDetails.SelectTab(1);
					AssertEquals("zTabPage2", tabControlBankAndEPayment.SelectedTab.Name);
					AssertEquals("zTabPage4", tabControlSummaryAndDetails.SelectedTab.Name);
					AssertZButton(form, new List<string> { "AddButton", "FilterPropertyLockButton", "CancelPostingButton", "ProcessEPaymentsButton" });
				}
			}
		}

		public void TestNewPaymentBatch_ButtonsAfterUnSuccessfullySaved_WhenAllowSendingEPayments()
		{
			SetupNewPaymentBatch();
			AssertEquals("Percondition", false, BatchPoster.IsInDatabase);

			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			using (var form = new PaymentBatchForm_ForTestOnly(BatchPoster))
			{
				form.Show();
				Application.DoEvents();

				form.FApplyButton_ForTestOnly.PerformClick();
				AssertEquals("Percondition", false, BatchPoster.IsInDatabase);

				var buttonsCanBeVisibleAndEnabled = new List<string>
				{
					"OverpaymentButton",
					"DiscountButton",
					"ExchangeDiffButton",

					"SaveButton",
					"SaveAndCloseButton",
					"CancelPostingButton",
					"SaveAsDraftButton",
					"ProcessEPaymentsButton"
				};

				var tabControlBankAndEPayment = form.Controls.Find("TabControlBankAndEPayment", true)[0] as ZTabControl;
				var tabControlSummaryAndDetails = form.Controls.Find("TabControlSummaryAndDetails", true)[0] as ZTabControl;
				AssertEquals("zTabPage1", tabControlBankAndEPayment.SelectedTab.Name);
				AssertZButton(form, buttonsCanBeVisibleAndEnabled.Union(new List<string> { "ApplyEXXButton", "CheckExRateButton" }), new List<string> { "PopupButton", "CalendarButton" });

				tabControlBankAndEPayment.SelectTab(1);
				tabControlSummaryAndDetails.SelectTab(0);
				AssertEquals("zTabPage2", tabControlBankAndEPayment.SelectedTab.Name);
				AssertEquals("zTabPage3", tabControlSummaryAndDetails.SelectedTab.Name);
				AssertZButton(form, buttonsCanBeVisibleAndEnabled.Union(new List<string> { "RefreshButton", "AcceptQuotesButton", "SyncRecipientsButton" }));

				tabControlSummaryAndDetails.SelectTab(1);
				AssertEquals("zTabPage2", tabControlBankAndEPayment.SelectedTab.Name);
				AssertEquals("zTabPage4", tabControlSummaryAndDetails.SelectedTab.Name);
				AssertZButton(form, buttonsCanBeVisibleAndEnabled.Union(new List<string> { "AddButton", "FilterPropertyLockButton" }));
			}
		}

		#endregion

		#region New PaymentBatch Prevent Posting if Payment Type is E-Payment and No Deals

		public void TestPostingApprovalWithNoDeals_PaymentTypeIsEPA_WhenPostButtonPressed_Message()
		{
			TestPostingWithNoDeals_PaymentTypeIsEPACore(postPaymentsAsPaymentApprovals: true, (form) => form.FApplyButton_ForTestOnly, allowSaving: false);
		}

		public void TestPostingPaymentWithNoDeals_PaymentTypeIsEPA_WhenPostButtonPressed_Message()
		{
			TestPostingWithNoDeals_PaymentTypeIsEPACore(postPaymentsAsPaymentApprovals: false, (form) => form.FApplyButton_ForTestOnly, allowSaving: false);
		}

		public void TestPostingApprovalWithNoDeals_PaymentTypeIsEPA_WhenPostAndCloseButtonPressed_Message()
		{
			TestPostingWithNoDeals_PaymentTypeIsEPACore(postPaymentsAsPaymentApprovals: true, (form) => form.SaveAndCloseButton_ForTestOnly, allowSaving: false);
		}

		public void TestPostingPaymentWithNoDeals_PaymentTypeIsEPA_WhenPostAndCloseButtonPressed_Message()
		{
			TestPostingWithNoDeals_PaymentTypeIsEPACore(postPaymentsAsPaymentApprovals: false, (form) => form.SaveAndCloseButton_ForTestOnly, allowSaving: false);
		}

		public void TestPostingApprovalWithNoDeals_PaymentTypeIsEPA_WhenSaveAsDraftButtonPressed_Saved()
		{
			TestPostingWithNoDeals_PaymentTypeIsEPACore(postPaymentsAsPaymentApprovals: true, (form) => form.SaveAsDraftButton_ForTestOnly, allowSaving: true);
		}

		public void TestPostingPaymentWithNoDeals_PaymentTypeIsEPA_WhenSaveAsDraftButtonPressed_Saved()
		{
			TestPostingWithNoDeals_PaymentTypeIsEPACore(postPaymentsAsPaymentApprovals: false, (form) => form.SaveAsDraftButton_ForTestOnly, allowSaving: true);
		}

		void TestPostingWithNoDeals_PaymentTypeIsEPACore(bool postPaymentsAsPaymentApprovals, Func<PaymentBatchForm_ForTestOnly, IButton> getButton, bool allowSaving)
		{
			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			{
				SetupDataForPostingTest();
				var bankAccount = TestObjectCreator.CreateEPaymentBankAccount();
				BatchPoster.APB_AB = bankAccount.PK;
				BatchPoster.APB_PaymentType = ReceiptTypes.EPayment;
				BatchPoster.APB_ChequeOrReference = "00001114";
				BatchPoster.PostPaymentsAsPaymentApprovals = postPaymentsAsPaymentApprovals;

				var approvalWithoutDeal1 = BatchPoster.PaymentApprovalCollection[0];
				approvalWithoutDeal1.AV_EPaymentReasonCode = EPaymentReasonCodes.OFXReasonCodes.AccountingServices;
				var approvalWithoutDeal2 = BatchPoster.PaymentApprovalCollection[1];
				approvalWithoutDeal2.AV_EPaymentReasonCode = EPaymentReasonCodes.OFXReasonCodes.AccountingServices;
				var approvalWithoutDeal3 = BatchPoster.PaymentApprovalCollection[2];
				approvalWithoutDeal3.AV_EPaymentReasonCode = EPaymentReasonCodes.OFXReasonCodes.AccountingServices;

				Assert("Precondition: using EPayment account", BatchPoster.BankAccount.IsEPaymentAccount);

				using (var form = new PaymentBatchForm_ForTestOnly(BatchPoster))
				{
					form.Show();
					Assert("Precondition: The Batch has not been saved to database.", !BatchPoster.IsInDatabase);

					var button = getButton(form);

					Assert("Precondition: The button is visible", button.Visible);
					Assert("Precondition: The button is enabled", button.Enabled);
					button.PerformClick();

					AssertEquals("IsInDatabase should match expected value", allowSaving, BatchPoster.IsInDatabase);

					if (!allowSaving)
					{
						var expectedMessage = $@"The following Payments cannot be processed:
{approvalWithoutDeal1.GetDescription()}
Payment type is an E-Payment, but it doesn't have an E-Payment Deal yet. Please process an E-Payment before posting.
{approvalWithoutDeal2.GetDescription()}
Payment type is an E-Payment, but it doesn't have an E-Payment Deal yet. Please process an E-Payment before posting.
{approvalWithoutDeal3.GetDescription()}
Payment type is an E-Payment, but it doesn't have an E-Payment Deal yet. Please process an E-Payment before posting.

These transactions cannot be posted

Alternatively, change the payment type from E-Payment to another payment type";

						AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
					}
				}
			}
		}

		#endregion

		#region Test Edit PaymentBatch

		public void TestEditPaymentBatch_Controls()
		{
			SetupBalancedPaymentBatch();

			var controller = ZControllerFactory.Create(ControllerIDs.PaymentBatch);
			var payableAuthorizationSettings = new PaymentAuthorisationSettingsCollection();
			var setting = payableAuthorizationSettings.AddNew();
			setting.Amount = 0;
			setting.AuthorisationRequirement = AuthorisationCodes.FirstApprovalRequiredOnly;
			setting.Range = RangeCodes.Above;

			using (AccountingConfigurationRegistry.Instance.PayableAuthorizationSettings.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, payableAuthorizationSettings))
			using (var form = controller.ShowEditForm(BatchPoster) as PaymentBatchForm)
			{
				Application.DoEvents();

				var saveButton = form.Controls.Find("SaveButton", true)[0];
				var saveAndCloseButton = form.Controls.Find("SaveAndCloseButton", true)[0];
				var postPaymentsAsPaymentApprovalsCheckBox = form.Controls.Find("PostPaymentsAsPaymentApprovalsCheckBox", true)[0];
				var saveAsDraftButton = form.Controls.Find("SaveAsDraftButton", true)[0] as ZButton;

				AssertEquals(ODisplayMode.Browse, form.DisplayMode);
				AssertEquals("Save", saveButton.Text);
				AssertEquals("Save && Close", saveAndCloseButton.Text);
				AssertEquals(false, saveButton.Enabled);
				AssertEquals(false, saveAndCloseButton.Enabled);
				AssertEquals(false, saveAsDraftButton.Enabled);
				AssertEquals(false, postPaymentsAsPaymentApprovalsCheckBox.Visible);

				form.PaymentBatchPoster_ForTestOnly.APB_AB = new ZGuid();
				AssertEquals(ODisplayMode.Edit, form.DisplayMode);
				AssertEquals(true, saveButton.Enabled);
				AssertEquals(true, saveAndCloseButton.Enabled);
				AssertEquals(true, saveAsDraftButton.Enabled);

				AssertEquals("Edit AP Payment Batch", form.FormHeading);

				var authoriseMenuItem = form.ActionsMenuItem_ForTestOnly.MenuItems.FindByText("Authorization");
				AssertNotNull(authoriseMenuItem);
				AssertEquals(3, authoriseMenuItem.MenuItems.Count);
				AssertNotNull(authoriseMenuItem.MenuItems.FindByText("Authorize"));
				AssertNotNull(authoriseMenuItem.MenuItems.FindByText("Unauthorize"));
				AssertNotNull(authoriseMenuItem.MenuItems.FindByText("Reject"));

				var postMenuItem = form.ActionsMenuItem_ForTestOnly.MenuItems.FindByText("Post Approvals");
				AssertNotNull(postMenuItem);
				AssertEquals(3, postMenuItem.MenuItems.Count);
				AssertNotNull(postMenuItem.MenuItems.FindByText("Post"));
				AssertNotNull(postMenuItem.MenuItems.FindByText("Allocate Check No."));
				AssertNotNull(postMenuItem.MenuItems.FindByText("Allocate Check No. and Post"));

				var cancelMenuItem = form.ActionsMenuItem_ForTestOnly.MenuItems.FindByText("Cancel");
				AssertNotNull(cancelMenuItem);
				AssertEquals(0, cancelMenuItem.MenuItems.Count);

				var submitForApprovalMenuItem = form.ActionsMenuItem_ForTestOnly.MenuItems.FindByText("Submit for Approval");
				AssertNotNull(submitForApprovalMenuItem);
				AssertEquals(0, submitForApprovalMenuItem.MenuItems.Count);
			}
		}

		void PrepareForEditPaymentBatch()
		{
			SetupDataForPostingTest();
			BatchPoster.APB_PaymentType = ReceiptTypes.Cheque;
			BatchPoster.APB_AB = TestBank.PK;
			BatchPoster.APB_AK = TestCheques.PK;
			BatchPostingHelper.AssertBatchPosterWillMatchAndPost(BatchPoster, -1, 3, -1);

			BatchPoster.PaymentApprovalCollection[0].AV_Status = PaymentApprovalStatus.Posted;
			BatchPoster.PaymentApprovalCollection[1].AV_Status = PaymentApprovalStatus.Cancelled;
			BatchPoster.PaymentApprovalCollection[2].AV_Status = PaymentApprovalStatus.AwaitingApproval;
			BatchPoster.APB_Status = Core.Constants.AccPaymentBatchStatus.Working;
			Factory.Save();
		}

		public void TestEditPaymentBatch_AddDiscount()
		{
			SetupNewPaymentBatch();
			using (Factory.SetTempContext(BusinessContext.SavingPaymentApprovalAsDraft))
			{
				BatchPoster.PaymentApprovalCollection[0].AV_Status = PaymentApprovalStatus.Draft;
				BatchPoster.MatchTransactions();
				Factory.Save();
			}

			var controller = ZControllerFactory.Create(ControllerIDs.PaymentBatch);
			using (var form = controller.ShowEditForm(BatchPoster) as PaymentBatchForm)
			{
				Application.DoEvents();
				var poster = form.BusinessEntity as APPaymentBatchPoster;
				AssertEquals(-600m, poster.PaymentApprovalCollection[0].MatchingBaseObject.Balance);
				AssertEquals("Percondition", true, ((APPaymentBatchApprovalMatching)form.SelectedPayment_ForTestOnly.MatchingBaseObject).IsPaymentAddedToMatchedTransactions_ForTestOnly);

				var discountButton = form.Controls.Find("DiscountButton", true)[0];
				(discountButton as ZButton).PerformClick();
				var lastFormShown = ZFormModaliser.LastFormShownForTest as TransactionViewForm;
				AssertNotNull(lastFormShown);
				AssertType<APDiscount>(lastFormShown.BusinessEntity);
				AssertEquals(ODisplayMode.Browse, form.DisplayMode);
				AssertEquals(ODisplayMode.New, lastFormShown.DisplayMode);

				lastFormShown.Close();
				AssertEquals(false, form.BusinessEntity.HasChanges);
				AssertEquals("DisplayMode not changed since BusinessEntity.HasChanges = false", ODisplayMode.Browse, form.DisplayMode);

				(discountButton as ZButton).PerformClick();
				lastFormShown = ZFormModaliser.LastFormShownForTest as TransactionViewForm;
				lastFormShown.FPostButton_ForTestOnly.PerformClick();
				AssertEquals(true, form.BusinessEntity.HasChanges);
				AssertEquals("DisplayMode not changed since BusinessEntity.HasChanges = true", ODisplayMode.Edit, form.DisplayMode);
			}
		}

		protected void MakeNewAuthorisationSetting(PaymentAuthorisationSettingsCollection collection,
			ZString range, ZInt amount, ZString requirement)
		{
			PaymentAuthorisationSettings newSetting = collection.AddNew();
			newSetting.Amount = (ZDecimal)amount;
			newSetting.AuthorisationRequirement = requirement;
			newSetting.Range = range;
		}

		public void TestEditPaymentBatch_UpdateExchangeRate()
		{
			var paymentAuthoSettings = new PaymentAuthorisationSettingsCollection();
			MakeNewAuthorisationSetting(paymentAuthoSettings, RangeCodes.Above, 0, AuthorisationCodes.FirstApprovalRequiredOnly);
			AccountingConfigurationRegistry.Instance.PayableAuthorizationSettings.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, paymentAuthoSettings);

			var exchangeRateTolerancesConfiguration = new ExchangeRateToleranceConfiguration();
			exchangeRateTolerancesConfiguration.ExchangeRateToleranceCollection.Add(new ExchangeRateTolerance() { Currency = "NZD", ExchangeRateTolerancePercentage = 10 });
			AccountingConfigurationRegistry.Instance.ExchangeRateTolerance.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, exchangeRateTolerancesConfiguration);

			SetupNewPaymentBatch();

			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			BatchPoster = newFactory.Load<APPaymentBatchPoster>(BatchPoster.PK);
			BatchPoster.LoadPayments();

			BatchPoster.PaymentApprovalCollection[0].AV_PayExRate = 1;

			AssertEquals("AWA", BatchPoster.PaymentApprovalCollection[0].AV_Status);
		}

		public void TestEditPaymentBatch_AddExchangeDiff()
		{
			SetupNewPaymentBatch();
			using (Factory.SetTempContext(BusinessContext.SavingPaymentApprovalAsDraft))
			{
				BatchPoster.PaymentApprovalCollection[0].AV_Status = PaymentApprovalStatus.Draft;
				BatchPoster.MatchTransactions();
				Factory.Save();
			}

			var controller = ZControllerFactory.Create(ControllerIDs.PaymentBatch);
			using (var form = controller.ShowEditForm(BatchPoster) as PaymentBatchForm)
			{
				Application.DoEvents();
				var poster = form.BusinessEntity as APPaymentBatchPoster;
				AssertEquals(-600m, poster.PaymentApprovalCollection[0].MatchingBaseObject.Balance);
				AssertEquals("Percondition", true, ((APPaymentBatchApprovalMatching)form.SelectedPayment_ForTestOnly.MatchingBaseObject).IsPaymentAddedToMatchedTransactions_ForTestOnly);

				var exchangeDiffButton = form.Controls.Find("ExchangeDiffButton", true)[0];
				(exchangeDiffButton as ZButton).PerformClick();
				var lastFormShown = ZFormModaliser.LastFormShownForTest as TransactionViewForm;
				AssertNotNull(lastFormShown);
				AssertType<APExchangeDifference>(lastFormShown.BusinessEntity);
				AssertEquals(ODisplayMode.Browse, form.DisplayMode);
				AssertEquals(ODisplayMode.New, lastFormShown.DisplayMode);

				lastFormShown.Close();
				AssertEquals(false, form.BusinessEntity.HasChanges);
				AssertEquals("DisplayMode not changed since BusinessEntity.HasChanges = false", ODisplayMode.Browse, form.DisplayMode);

				(exchangeDiffButton as ZButton).PerformClick();
				lastFormShown = ZFormModaliser.LastFormShownForTest as TransactionViewForm;
				lastFormShown.FPostButton_ForTestOnly.PerformClick();
				AssertEquals(true, form.BusinessEntity.HasChanges);
				AssertEquals("DisplayMode not changed since BusinessEntity.HasChanges = true", ODisplayMode.Edit, form.DisplayMode);
			}
		}

		public void TestEditPaymentBatch_ViewAndEditExchangeDiff()
		{
			SetupBalancedPaymentBatch();

			var controller = ZControllerFactory.Create(ControllerIDs.PaymentBatch);
			using (var form = controller.ShowEditForm(BatchPoster) as PaymentBatchForm)
			{
				Application.DoEvents();
				var poster = form.BusinessEntity as APPaymentBatchPoster;
				AssertEquals(0m, poster.PaymentApprovalCollection[0].MatchingBaseObject.Balance);
				AssertEquals("Percondition", true, ((APPaymentBatchApprovalMatching)form.SelectedPayment_ForTestOnly.MatchingBaseObject).IsPaymentAddedToMatchedTransactions_ForTestOnly);

				var exchangeDiffButton = form.Controls.Find("ExchangeDiffButton", true)[0];
				(exchangeDiffButton as ZButton).PerformClick();
				var lastFormShown = ZFormModaliser.LastFormShownForTest as TransactionViewForm;
				AssertNotNull(lastFormShown);
				AssertType<APExchangeDifference>(lastFormShown.BusinessEntity);
				AssertEquals(ODisplayMode.Browse, form.DisplayMode);
				AssertEquals(ODisplayMode.Browse, lastFormShown.DisplayMode);

				lastFormShown.Close();
				AssertEquals(false, form.BusinessEntity.HasChanges);
				AssertEquals("DisplayMode not changed since BusinessEntity.HasChanges = false", ODisplayMode.Browse, form.DisplayMode);

				(exchangeDiffButton as ZButton).PerformClick();
				lastFormShown = ZFormModaliser.LastFormShownForTest as TransactionViewForm;
				(lastFormShown.BusinessEntity as APExchangeDifference).BindableOSAmount = 150m;
				AssertEquals(true, form.BusinessEntity.HasChanges);
				AssertEquals("DisplayMode not changed since BusinessEntity.HasChanges = true", ODisplayMode.Edit, form.DisplayMode);
			}
		}

		public void TestEditPaymentBatch_ViewAndEditDiscount()
		{
			SetupBalancedPaymentBatch();

			var controller = ZControllerFactory.Create(ControllerIDs.PaymentBatch);
			using (var form = controller.ShowEditForm(BatchPoster) as PaymentBatchForm)
			{
				Application.DoEvents();
				var poster = form.BusinessEntity as APPaymentBatchPoster;
				AssertEquals(0m, poster.PaymentApprovalCollection[0].MatchingBaseObject.Balance);
				AssertEquals("Percondition", true, ((APPaymentBatchApprovalMatching)form.SelectedPayment_ForTestOnly.MatchingBaseObject).IsPaymentAddedToMatchedTransactions_ForTestOnly);

				var discountButton = form.Controls.Find("DiscountButton", true)[0];
				(discountButton as ZButton).PerformClick();
				var lastFormShown = ZFormModaliser.LastFormShownForTest as TransactionViewForm;
				AssertNotNull(lastFormShown);
				AssertType<APDiscount>(lastFormShown.BusinessEntity);
				AssertEquals(ODisplayMode.Browse, form.DisplayMode);
				AssertEquals(ODisplayMode.Browse, lastFormShown.DisplayMode);

				lastFormShown.Close();
				AssertEquals(false, form.BusinessEntity.HasChanges);
				AssertEquals("DisplayMode not changed since BusinessEntity.HasChanges = false", ODisplayMode.Browse, form.DisplayMode);

				(discountButton as ZButton).PerformClick();
				lastFormShown = ZFormModaliser.LastFormShownForTest as TransactionViewForm;
				(lastFormShown.BusinessEntity as APDiscount).BindableOSAmount = 150m;
				AssertEquals(true, form.BusinessEntity.HasChanges);
				AssertEquals("DisplayMode not changed since BusinessEntity.HasChanges = true", ODisplayMode.Edit, form.DisplayMode);
			}
		}

		public void TestEditPaymentBatch_AddOverpayment_ShowError()
		{
			PrepareForEditPaymentBatch();

			var controller = ZControllerFactory.Create(ControllerIDs.PaymentBatch);
			using (var form = controller.ShowEditForm(BatchPoster) as PaymentBatchForm)
			{
				Application.DoEvents();

				var overpaymentButton = form.Controls.Find("OverpaymentButton", true)[0];

				form.PaymentBatchGrid_ForTestOnly.CurrentRowIndex = 0;
				(overpaymentButton as ZButton).PerformClick();
				AssertEquals("You cannot create an Over Payment on this payment approval since it's status is Posted.", UnitTestUserNotification.Instance.LastMessage.Text);

				form.PaymentBatchGrid_ForTestOnly.CurrentRowIndex = 1;
				(overpaymentButton as ZButton).PerformClick();
				AssertEquals("You cannot create an Over Payment on this payment approval since it's status is Canceled.", UnitTestUserNotification.Instance.LastMessage.Text);

				form.PaymentBatchGrid_ForTestOnly.CurrentRowIndex = 2;
				(overpaymentButton as ZButton).PerformClick();
				AssertEquals("You cannot create an OverPayment in this matching session", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestEditPaymentBatch_AddDiscount_ShowError()
		{
			PrepareForEditPaymentBatch();

			var controller = ZControllerFactory.Create(ControllerIDs.PaymentBatch);
			using (var form = controller.ShowEditForm(BatchPoster) as PaymentBatchForm)
			{
				Application.DoEvents();

				var discountButton = form.Controls.Find("DiscountButton", true)[0];

				form.PaymentBatchGrid_ForTestOnly.CurrentRowIndex = 0;
				(discountButton as ZButton).PerformClick();
				AssertEquals("You cannot create an Discount on this payment approval since it's status is Posted.", UnitTestUserNotification.Instance.LastMessage.Text);

				form.PaymentBatchGrid_ForTestOnly.CurrentRowIndex = 1;
				(discountButton as ZButton).PerformClick();
				AssertEquals("You cannot create an Discount on this payment approval since it's status is Canceled.", UnitTestUserNotification.Instance.LastMessage.Text);

				form.PaymentBatchGrid_ForTestOnly.CurrentRowIndex = 2;
				(discountButton as ZButton).PerformClick();
				AssertEquals("Discount cannot be created here because the balance is zero.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestEditPaymentBatch_AddExchangeDiff_ShowError()
		{
			PrepareForEditPaymentBatch();

			var controller = ZControllerFactory.Create(ControllerIDs.PaymentBatch);
			using (var form = controller.ShowEditForm(BatchPoster) as PaymentBatchForm)
			{
				Application.DoEvents();

				var exchangeDiffButton = form.Controls.Find("ExchangeDiffButton", true)[0];

				form.PaymentBatchGrid_ForTestOnly.CurrentRowIndex = 0;
				(exchangeDiffButton as ZButton).PerformClick();
				AssertEquals("You cannot create an Exchange Difference on this payment approval since it's status is Posted.", UnitTestUserNotification.Instance.LastMessage.Text);

				form.PaymentBatchGrid_ForTestOnly.CurrentRowIndex = 1;
				(exchangeDiffButton as ZButton).PerformClick();
				AssertEquals("You cannot create an Exchange Difference on this payment approval since it's status is Canceled.", UnitTestUserNotification.Instance.LastMessage.Text);

				form.PaymentBatchGrid_ForTestOnly.CurrentRowIndex = 2;
				(exchangeDiffButton as ZButton).PerformClick();
				AssertEquals("Exchange Difference cannot be created here because the balance is zero.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestEditPaymentBatch_ApplyEXXButton_Click()
		{
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, "BUY", 1.5m);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.CNY, "BUY", 2.5m);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.TWD, "BUY", 3.5m);

			SetupDataForBaseTest();

			var poster = BatchPostingHelper.CreateBatchPosterWithMultipleTransactionsWithForeignCurrency();
			poster.PaymentApprovalCollection.Cast<PaymentApprovalBase>().Single(x => x.CurrencyCode == TestObjectCreator.USD.Code).AV_Status = PaymentApprovalStatus.Cancelled;
			Factory.Save();

			var controller = ZControllerFactory.Create(ControllerIDs.PaymentBatch);
			using (var form = controller.ShowEditForm(poster) as PaymentBatchForm)
			{
				Application.DoEvents();
				var applyEXXButton = form.Controls.Find("ApplyEXXButton", true)[0];
				var saveAsDraftButton = form.Controls.Find("SaveAsDraftButton", true)[0] as ZButton;

				poster = form.BusinessEntity as APPaymentBatchPoster;
				var payment1 = poster.PaymentApprovalCollection.Cast<PaymentApprovalBase>().Single(x => x.CurrencyCode == TestObjectCreator.USD.Code);
				var payment2 = poster.PaymentApprovalCollection.Cast<PaymentApprovalBase>().Single(x => x.CurrencyCode == TestObjectCreator.CNY.Code);
				var payment3 = poster.PaymentApprovalCollection.Cast<PaymentApprovalBase>().Single(x => x.CurrencyCode == TestObjectCreator.TWD.Code);

				AssertEquals("Percondition", -333.3300m, payment1.MatchingBaseObject.Balance);
				AssertEquals("Percondition", -200.0000m, payment2.MatchingBaseObject.Balance);
				AssertEquals("Percondition", -142.8600m, payment3.MatchingBaseObject.Balance);
				AssertEquals("Percondition", ODisplayMode.Browse, form.DisplayMode);

				(applyEXXButton as ZButton).PerformClick();
				AssertEquals("One or more payments were skipped because they were Posted Or Canceled.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(ODisplayMode.Edit, form.DisplayMode);

				AssertEquals(-333.3300m, payment1.MatchingBaseObject.Balance);
				AssertEquals(0m, payment2.MatchingBaseObject.Balance);
				AssertEquals(0m, payment3.MatchingBaseObject.Balance);

				AssertEquals(0m, payment2.AV_ExchangeDifference);
				AssertEquals(0m, payment3.AV_ExchangeDifference);

				poster.RemovePaymentFromBatch(payment1);
				saveAsDraftButton.PerformClick();

				AssertEquals(200.0000m, payment2.AV_ExchangeDifference);
				AssertEquals(142.8600m, payment3.AV_ExchangeDifference);
			}
		}

		public void TestEditPaymentBatch_ValidatePaymentBatch()
		{
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, "BUY", 1.5m);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.CNY, "BUY", 2.5m);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.TWD, "BUY", 3.5m);

			SetupDataForBaseTest();

			var poster = BatchPostingHelper.CreateBatchPosterWithMultipleTransactionsWithForeignCurrency();
			var cancelledPayment = poster.PaymentApprovalCollection.Cast<PaymentApprovalBase>().Single(x => x.CurrencyCode == TestObjectCreator.USD.Code);
			cancelledPayment.AV_Status = PaymentApprovalStatus.Cancelled;
			cancelledPayment.AV_AB = Factory.NewWithValidTestData<AccBankAccount>().PK;
			Factory.Save();

			var controller = ZControllerFactory.Create(ControllerIDs.PaymentBatch);
			using (var form = controller.ShowEditForm(poster) as PaymentBatchForm)
			{
				Application.DoEvents();

				poster = form.BusinessEntity as APPaymentBatchPoster;
				AssertEquals(true, poster.NotificationsIncludingChildren.HasErrors());
				AssertEquals(true, poster.NotificationsIncludingChildren.Contains("Error - Balance: The balance must equal 0"));
				AssertEquals(false, poster.NotificationsIncludingChildren.Contains("Error - AV_AB: Payment approval detail does not match payment batch header - please reconcile Bank Account with payment batch header manually or by re-entering header details."));
				AssertEquals(false, poster.NotificationsIncludingChildren.Contains("Error - AV_Status: Payment approval has status CAN-Canceled/PST-Posted, please remove this payment approval via Right Click > Remove in order to save Payment Batch record"));

				form.FireValidateAllForTest();
				AssertEquals(true, poster.NotificationsIncludingChildren.HasErrors());
				AssertEquals(true, poster.NotificationsIncludingChildren.Contains("Error - Balance: The balance must equal 0"));
				AssertEquals(true, poster.NotificationsIncludingChildren.Contains("Error - AV_AB: Payment approval detail does not match payment batch header - please reconcile Bank Account with payment batch header manually or by re-entering header details."));
				AssertEquals(true, poster.NotificationsIncludingChildren.Contains("Error - AV_Status: Payment approval has status CAN-Canceled/PST-Posted, please remove this payment approval via Right Click > Remove in order to save Payment Batch record"));
			}
		}

		public void TestEditPaymentBatch_DeletePaymentTransaction()
		{
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, "BUY", 1.5m);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.CNY, "BUY", 2.5m);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.TWD, "BUY", 3.5m);

			SetupDataForBaseTest();

			var poster = BatchPostingHelper.CreateBatchPosterWithMultipleTransactionsWithForeignCurrency();
			var cancelledPayment = poster.PaymentApprovalCollection.Cast<APPaymentApprovalWithoutAuthorisation>().Single(x => x.CurrencyCode == TestObjectCreator.USD.Code);
			cancelledPayment.AV_Status = PaymentApprovalStatus.Cancelled;
			cancelledPayment.AV_ChequeOrReference = "0000567";
			Factory.Save();

			var controller = ZControllerFactory.Create(ControllerIDs.PaymentBatch);
			using (var form = controller.ShowEditForm(poster) as PaymentBatchForm)
			{
				Application.DoEvents();

				poster = form.BusinessEntity as APPaymentBatchPoster;
				var previouslySelectedPayment = form.SelectedPayment_ForTestOnly;
				AssertEquals("Percondition", 3, poster.PaymentApprovalCollection.Count);
				AssertEquals("Percondition", true, previouslySelectedPayment.IsCancelled);
				AssertEquals("Percondition", "0000567", previouslySelectedPayment.AV_ChequeOrReference);

				AssertEquals("Percondition", ODisplayMode.Browse, form.DisplayMode);
				form.SelectedPayment_ForTestOnly.AV_ChequeOrReference = "0000789";
				AssertEquals(ODisplayMode.Edit, form.DisplayMode);

				form.DeletePaymentTransaction_ForTestOnly(null, null);

				AssertEquals(2, poster.PaymentApprovalCollection.Count);
				AssertEquals("0000567", previouslySelectedPayment.AV_ChequeOrReference);
				AssertEquals(ZGuid.Empty, previouslySelectedPayment.AV_APB_PaymentBatch);

				var saveAsDraftButton = form.Controls.Find("SaveAsDraftButton", true)[0] as ZButton;
				saveAsDraftButton.PerformClick();

				var newFactory = new BusinessObjectFactory();
				var payment = newFactory.Load<AccPaymentApproval>(cancelledPayment.PK);
				AssertEquals("0000567", payment.AV_ChequeOrReference);
				AssertEquals(ZGuid.Empty, payment.AV_APB_PaymentBatch);
			}
		}

		public void TestEditPaymentBatch_DeletePaymentTransactionWillSetHasChanges()
		{
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, "BUY", 1.5m);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.CNY, "BUY", 2.5m);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.TWD, "BUY", 3.5m);

			SetupDataForBaseTest();

			var poster = BatchPostingHelper.CreateBatchPosterWithMultipleTransactionsWithForeignCurrency();
			var cancelledPayment = poster.PaymentApprovalCollection.Cast<APPaymentApprovalWithoutAuthorisation>().Single(x => x.CurrencyCode == TestObjectCreator.USD.Code);
			cancelledPayment.AV_Status = PaymentApprovalStatus.Cancelled;
			cancelledPayment.AV_ChequeOrReference = "0000567";
			Factory.Save();

			var controller = ZControllerFactory.Create(ControllerIDs.PaymentBatch);
			using (var form = controller.ShowEditForm(poster) as PaymentBatchForm)
			{
				Application.DoEvents();

				poster = form.BusinessEntity as APPaymentBatchPoster;
				var previouslySelectedPayment = form.SelectedPayment_ForTestOnly;
				AssertEquals("Percondition", 3, poster.PaymentApprovalCollection.Count);
				AssertEquals("Percondition", true, previouslySelectedPayment.IsCancelled);
				AssertEquals("Percondition", "0000567", previouslySelectedPayment.AV_ChequeOrReference);

				AssertEquals("Percondition", ODisplayMode.Browse, form.DisplayMode);
				form.DeletePaymentTransaction_ForTestOnly(null, null);
				AssertEquals(ODisplayMode.Edit, form.DisplayMode);
				AssertEquals(true, poster.HasChanges);
			}
		}

		public void TestEditPaymentBatch_Save_ShowErrorAboutBalance()
		{
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, "BUY", 1m);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.CNY, "BUY", 2.5m);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.TWD, "BUY", 3.5m);

			SetupDataForBaseTest();

			var poster = BatchPostingHelper.CreateBatchPosterWithMultipleTransactionsWithForeignCurrency();
			var controller = ZControllerFactory.Create(ControllerIDs.PaymentBatch);
			using (var form = controller.ShowEditForm(poster) as PaymentBatchForm)
			{
				Application.DoEvents();
				poster = form.BusinessEntity as APPaymentBatchPoster;

				AssertEquals("Percondition", true, BatchPoster.IsInDatabase);
				AssertEquals("Percondition", ODisplayMode.Browse, form.DisplayMode);
				poster.APB_PaymentDate = ZDateTime.Now.AddDays(3);
				AssertEquals(ODisplayMode.Edit, form.DisplayMode);

				var payment1 = poster.PaymentApprovalCollection.Cast<PaymentApprovalBase>().Single(x => x.CurrencyCode == TestObjectCreator.USD.Code);
				var payment2 = poster.PaymentApprovalCollection.Cast<PaymentApprovalBase>().Single(x => x.CurrencyCode == TestObjectCreator.CNY.Code);
				var payment3 = poster.PaymentApprovalCollection.Cast<PaymentApprovalBase>().Single(x => x.CurrencyCode == TestObjectCreator.TWD.Code);
				AssertEquals("Percondition", 0m, payment1.MatchingBaseObject.Balance);
				AssertEquals("Percondition", -200.0000m, payment2.MatchingBaseObject.Balance);
				AssertEquals("Percondition", -142.8600m, payment3.MatchingBaseObject.Balance);

				poster.PaymentForBinding = payment1;
				AssertEquals("Percondition", false, poster.Notifications.HasErrors());
				AssertEquals("Percondition", true, poster.NotificationsIncludingChildren.HasErrors());
				AssertEquals(false, payment1.MatchingBaseObject.Notifications.Contains("Error - Balance: The balance must equal 0"));
				AssertEquals(true, payment2.MatchingBaseObject.Notifications.Contains("Error - Balance: The balance must equal 0"));
				AssertEquals(true, payment3.MatchingBaseObject.Notifications.Contains("Error - Balance: The balance must equal 0"));

				form.FApplyButton_ForTestOnly.PerformClick();
				AssertEquals("There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(false, poster.Notifications.HasErrors());
				AssertEquals(false, payment1.MatchingBaseObject.Notifications.Contains("Error - Balance: The balance must equal 0"));
				AssertEquals(true, payment2.MatchingBaseObject.Notifications.Contains("Error - Balance: The balance must equal 0"));
				AssertEquals(true, payment3.MatchingBaseObject.Notifications.Contains("Error - Balance: The balance must equal 0"));
			}
		}

		public void TestEditPaymentBatch_Save_Successfully()
		{
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, "BUY", 1m);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.CNY, "BUY", 2m);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.TWD, "BUY", 3m);

			SetupDataForBaseTest();

			var poster = BatchPostingHelper.CreateBatchPosterWithMultipleTransactionsWithForeignCurrency();
			var controller = ZControllerFactory.Create(ControllerIDs.PaymentBatch);
			using (var form = controller.ShowEditForm(poster) as PaymentBatchForm)
			{
				poster = form.BusinessEntity as APPaymentBatchPoster;
				var summaryRows = poster.CurrencySummary.SummaryRows.OfType<CurrencySummaryRow>();
				var summaryRowUSD = summaryRows.Single(x => x.Currency == TestObjectCreator.USD.Code);
				var summaryRowCNY = summaryRows.Single(x => x.Currency == TestObjectCreator.CNY.Code);
				var summaryRowTWD = summaryRows.Single(x => x.Currency == TestObjectCreator.TWD.Code);
				var payment1 = poster.PaymentApprovalCollection.Cast<PaymentApprovalBase>().Single(x => x.CurrencyCode == TestObjectCreator.USD.Code);
				var payment2 = poster.PaymentApprovalCollection.Cast<PaymentApprovalBase>().Single(x => x.CurrencyCode == TestObjectCreator.CNY.Code);
				var payment3 = poster.PaymentApprovalCollection.Cast<PaymentApprovalBase>().Single(x => x.CurrencyCode == TestObjectCreator.TWD.Code);

				AssertEquals("Percondition", 1m, summaryRowUSD.ExchangeRate);
				AssertEquals("Percondition", 2m, summaryRowCNY.ExchangeRate);
				AssertEquals("Percondition", 3m, summaryRowTWD.ExchangeRate);

				AssertEquals("Percondition", 1000m, payment1.AV_Calc_LocalAmount);
				AssertEquals("Percondition", 1000m, payment2.AV_Calc_LocalAmount);
				AssertEquals("Percondition", 1000m, payment3.AV_Calc_LocalAmount);
				AssertEquals("Percondition", 1m, payment1.AV_PayExRate);
				AssertEquals("Percondition", 2m, payment2.AV_PayExRate);
				AssertEquals("Percondition", 3m, payment3.AV_PayExRate);

				AssertEquals("Percondition", 0m, payment1.AV_ExchangeDifference);
				AssertEquals("Percondition", 0m, payment1.AV_Discount);

				AssertEquals("Percondition", 0m, payment1.MatchingBaseObject.Balance);
				AssertEquals("Percondition", 0m, payment2.MatchingBaseObject.Balance);
				AssertEquals("Percondition", 0m, payment3.MatchingBaseObject.Balance);

				AssertEquals("Percondition", ODisplayMode.Browse, form.DisplayMode);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				summaryRowUSD.ExchangeRate = 1.5m;
				AssertEquals("Update all USD payment(s) with exchange rate 1.5?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(666.67m, payment1.AV_Calc_LocalAmount);
				AssertEquals(-333.33m, payment1.MatchingBaseObject.Balance);

				AssertEquals(ODisplayMode.Edit, form.DisplayMode);

				var paymentMatchingBaseObject1 = payment1.MatchingBaseObject;
				var exchangeDifference = paymentMatchingBaseObject1.GetMiscellaneousTransaction(TransactionTypes.ExchangeDifference);
				var discount = paymentMatchingBaseObject1.GetMiscellaneousTransaction(TransactionTypes.Discount);
				exchangeDifference.BindableOSAmount = 100m;
				discount.BindableOSAmount = 233.33m;
				paymentMatchingBaseObject1.AddMiscellaneousTransaction(exchangeDifference);
				paymentMatchingBaseObject1.AddMiscellaneousTransaction(discount);
				AssertEquals(0m, payment1.MatchingBaseObject.Balance);

				AssertEquals(true, poster.HasChanges);
				var saveAsDraftButton = form.Controls.Find("SaveAsDraftButton", true)[0] as ZButton;
				var saveButton = form.Controls.Find("SaveButton", true)[0];
				var saveAndCloseButton = form.Controls.Find("SaveAndCloseButton", true)[0];
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.FApplyButton_ForTestOnly.PerformClick();
				AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(ODisplayMode.Browse, form.DisplayMode);
				AssertEquals(false, poster.HasChanges);
				AssertEquals(false, saveButton.Enabled);
				AssertEquals(false, saveAndCloseButton.Enabled);
				AssertEquals(false, saveAsDraftButton.Enabled);

				var newFactory = new BusinessObjectFactory();
				var newPayment1 = newFactory.Load<AccPaymentApproval>(payment1.PK);
				var newPayment2 = newFactory.Load<AccPaymentApproval>(payment2.PK);
				var newPayment3 = newFactory.Load<AccPaymentApproval>(payment3.PK);

				AssertEquals(1000m, newPayment1.AV_Amount);
				AssertEquals(2000m, newPayment2.AV_Amount);
				AssertEquals(3000m, newPayment3.AV_Amount);

				AssertEquals(1.5m, newPayment1.AV_PayExRate);
				AssertEquals(2m, newPayment2.AV_PayExRate);
				AssertEquals(3m, newPayment3.AV_PayExRate);

				AssertEquals(100m, newPayment1.AV_ExchangeDifference);
				AssertEquals(233.33m, newPayment1.AV_Discount);
			}
		}

		public void TestEditPaymentBatch_SaveAsDraft_ShowErrorAboutStatus()
		{
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, "BUY", 1.5m);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.CNY, "BUY", 2.5m);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.TWD, "BUY", 3.5m);

			SetupDataForBaseTest();

			var poster = BatchPostingHelper.CreateBatchPosterWithMultipleTransactionsWithForeignCurrency();
			var cancelledPayment = poster.PaymentApprovalCollection.Cast<APPaymentApprovalWithoutAuthorisation>().Single(x => x.CurrencyCode == TestObjectCreator.USD.Code);
			cancelledPayment.AV_Status = PaymentApprovalStatus.Cancelled;
			cancelledPayment.AV_ChequeOrReference = "0000567";
			Factory.Save();

			var controller = ZControllerFactory.Create(ControllerIDs.PaymentBatch);
			using (var form = controller.ShowEditForm(poster) as PaymentBatchForm)
			{
				var saveButton = form.Controls.Find("SaveButton", true)[0];
				var saveAndCloseButton = form.Controls.Find("SaveAndCloseButton", true)[0];
				var saveAsDraftButton = form.Controls.Find("SaveAsDraftButton", true)[0] as ZButton;

				AssertEquals("Percondition", ODisplayMode.Browse, form.DisplayMode);
				AssertEquals(false, saveButton.Enabled);
				AssertEquals(false, saveAndCloseButton.Enabled);
				AssertEquals(false, saveAsDraftButton.Enabled);

				poster = form.BusinessEntity as APPaymentBatchPoster;
				poster.APB_PaymentDate = ZDateTime.Now.AddDays(3);
				AssertEquals(ODisplayMode.Edit, form.DisplayMode);
				AssertEquals(true, saveButton.Enabled);
				AssertEquals(true, saveAndCloseButton.Enabled);
				AssertEquals(true, saveAsDraftButton.Enabled);

				saveAsDraftButton.PerformClick();
				AssertEquals($"{cancelledPayment.GetDescription()} can not Save as Draft since status is Canceled", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(ODisplayMode.Edit, form.DisplayMode);
				AssertEquals(true, saveButton.Enabled);
				AssertEquals(true, saveAndCloseButton.Enabled);
				AssertEquals(true, saveAsDraftButton.Enabled);
			}
		}

		public void TestEditBatchPaymentType_HideEPAFields()
		{
			SetupDataForPostingTest();
			var bankAccount = TestObjectCreator.CreateEPaymentBankAccount();
			TestObjectCreator.CreateEPaymentStaffToken(bankAccount.PK, ZDateTime.UtcNow.AddHours(2), Env.CurrentUser.Initials, AccEPaymentStaffTokenLookups.StatusCodes.Authorised);
			BatchPoster.APB_AB = bankAccount.PK;
			Factory.Save();

			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			using (var testForm = new PaymentBatchForm_ForTestOnly(BatchPoster))
			{
				testForm.Show();
				testForm.PaymentBatchPoster_ForTestOnly.APB_PaymentType = ReceiptTypes.Cash;
				Assert("CurrencySummaryTotalEPaymentTextBox should be invisible when E-Payment functionality is disabled (Cash is enabled).", !testForm.CurrencySummaryTotalEPaymentTextBox_ForTestOnly.Visible);
				Assert("CurrencySummaryTotalEPaymentFeesTextBox should be invisible when E-Payment functionality is disabled (Cash is enabled).", !testForm.CurrencySummaryTotalEPaymentFeesTextBox_ForTestOnly.Visible);

				testForm.PaymentBatchPoster_ForTestOnly.APB_PaymentType = ReceiptTypes.EPayment;
				Assert("CurrencySummaryTotalEPaymentTextBox should be visible when E-Payment functionality is enabled.", testForm.CurrencySummaryTotalEPaymentTextBox_ForTestOnly.Visible);
				Assert("CurrencySummaryTotalEPaymentFeesTextBox should be visible when E-Payment functionality is enabled.", testForm.CurrencySummaryTotalEPaymentFeesTextBox_ForTestOnly.Visible);

				testForm.PaymentBatchPoster_ForTestOnly.APB_PaymentType = ReceiptTypes.Cheque;
				Assert("CurrencySummaryTotalEPaymentTextBox should be invisible E-Payment functionality is disabled (Cheque is enabled).", !testForm.CurrencySummaryTotalEPaymentTextBox_ForTestOnly.Visible);
				Assert("CurrencySummaryTotalEPaymentFeesTextBox should be invisible when E-Payment functionality is disabled (Cheque is enabled).", !testForm.CurrencySummaryTotalEPaymentFeesTextBox_ForTestOnly.Visible);
			}
		}

		public void TestEditPaymentBatch_SaveAsDraft_ShowErrorAboutEmptyApproval()
		{
			SetupDataForBaseTest();
			BatchPoster.APB_PaymentType = ReceiptTypes.Cash;
			Factory.Save();

			using (var testForm = new PaymentBatchForm_ForTestOnly(BatchPoster))
			{
				var saveAsDraftButton = testForm.Controls.Find("SaveAsDraftButton", true)[0] as ZButton;

				testForm.Show();
				Assert("Form should be visible", testForm.Visible);
				AssertEquals("There should be only 1 payment for posting", 1, BatchPoster.PaymentApprovalCollection.Count);
				testForm.PaymentBatchGrid_ForTestOnly.Select(0);
				testForm.DeletePaymentTransaction_ForTestOnly(null, null);
				AssertEquals("PaymentCollection should be empty", 0, BatchPoster.PaymentApprovalCollection.Count);
				Assert("Form should be visible", testForm.Visible);

				saveAsDraftButton.PerformClick();
				AssertEquals("Payment Batch cannot be saved as draft when there are no linked payment approvals.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
			Factory.Save();
		}

		public void TestEditPaymentBatch_SaveAsDraft_Successfully()
		{
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, "BUY", 1m);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.CNY, "BUY", 2m);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.TWD, "BUY", 3m);

			SetupDataForBaseTest();

			var poster = BatchPostingHelper.CreateBatchPosterWithMultipleTransactionsWithForeignCurrency();
			var controller = ZControllerFactory.Create(ControllerIDs.PaymentBatch);
			using (var form = controller.ShowEditForm(poster) as PaymentBatchForm)
			{
				poster = form.BusinessEntity as APPaymentBatchPoster;
				var summaryRows = poster.CurrencySummary.SummaryRows.OfType<CurrencySummaryRow>();
				var summaryRowUSD = summaryRows.Single(x => x.Currency == TestObjectCreator.USD.Code);
				var summaryRowCNY = summaryRows.Single(x => x.Currency == TestObjectCreator.CNY.Code);
				var summaryRowTWD = summaryRows.Single(x => x.Currency == TestObjectCreator.TWD.Code);
				var payment1 = poster.PaymentApprovalCollection.Cast<PaymentApprovalBase>().Single(x => x.CurrencyCode == TestObjectCreator.USD.Code);
				var payment2 = poster.PaymentApprovalCollection.Cast<PaymentApprovalBase>().Single(x => x.CurrencyCode == TestObjectCreator.CNY.Code);
				var payment3 = poster.PaymentApprovalCollection.Cast<PaymentApprovalBase>().Single(x => x.CurrencyCode == TestObjectCreator.TWD.Code);

				AssertEquals("Percondition", 1m, summaryRowUSD.ExchangeRate);
				AssertEquals("Percondition", 2m, summaryRowCNY.ExchangeRate);
				AssertEquals("Percondition", 3m, summaryRowTWD.ExchangeRate);

				AssertEquals("Percondition", 1000m, payment1.AV_Calc_LocalAmount);
				AssertEquals("Percondition", 1000m, payment2.AV_Calc_LocalAmount);
				AssertEquals("Percondition", 1000m, payment3.AV_Calc_LocalAmount);
				AssertEquals("Percondition", 1m, payment1.AV_PayExRate);
				AssertEquals("Percondition", 2m, payment2.AV_PayExRate);
				AssertEquals("Percondition", 3m, payment3.AV_PayExRate);

				AssertEquals("Percondition", 0m, payment1.AV_ExchangeDifference);
				AssertEquals("Percondition", 0m, payment1.AV_Discount);

				AssertEquals("Percondition", 0m, payment1.MatchingBaseObject.Balance);
				AssertEquals("Percondition", 0m, payment2.MatchingBaseObject.Balance);
				AssertEquals("Percondition", 0m, payment3.MatchingBaseObject.Balance);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				summaryRowUSD.ExchangeRate = 1.5m;
				AssertEquals("Update all USD payment(s) with exchange rate 1.5?", UnitTestUserNotification.Instance.LastMessage.Text);
				summaryRowCNY.ExchangeRate = 2.5m;
				AssertEquals("Update all CNY payment(s) with exchange rate 2.5?", UnitTestUserNotification.Instance.LastMessage.Text);
				summaryRowTWD.ExchangeRate = 3.5m;
				AssertEquals("Update all TWD payment(s) with exchange rate 3.5?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(666.67m, payment1.AV_Calc_LocalAmount);
				AssertEquals(800m, payment2.AV_Calc_LocalAmount);
				AssertEquals(857.14m, payment3.AV_Calc_LocalAmount);
				AssertEquals(-333.33m, payment1.MatchingBaseObject.Balance);
				AssertEquals(-200m, payment2.MatchingBaseObject.Balance);
				AssertEquals(-142.86m, payment3.MatchingBaseObject.Balance);

				var paymentMatchingBaseObject1 = payment1.MatchingBaseObject;
				var exchangeDifference = paymentMatchingBaseObject1.GetMiscellaneousTransaction(TransactionTypes.ExchangeDifference);
				var discount = paymentMatchingBaseObject1.GetMiscellaneousTransaction(TransactionTypes.Discount);
				exchangeDifference.BindableOSAmount = 100m;
				discount.BindableOSAmount = 200m;
				paymentMatchingBaseObject1.AddMiscellaneousTransaction(exchangeDifference);
				paymentMatchingBaseObject1.AddMiscellaneousTransaction(discount);
				AssertEquals(-33.33m, payment1.MatchingBaseObject.Balance);

				AssertEquals(true, poster.HasChanges);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var saveAsDraftButton = form.Controls.Find("SaveAsDraftButton", true)[0] as ZButton;
				var saveButton = form.Controls.Find("SaveButton", true)[0];
				var saveAndCloseButton = form.Controls.Find("SaveAndCloseButton", true)[0];
				saveAsDraftButton.PerformClick();
				AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(ODisplayMode.Browse, form.DisplayMode);
				AssertEquals(ODisplayMode.Browse, form.DisplayMode);
				AssertEquals(false, poster.HasChanges);
				AssertEquals(false, saveButton.Enabled);
				AssertEquals(false, saveAndCloseButton.Enabled);
				AssertEquals(false, saveAsDraftButton.Enabled);

				var newFactory = new BusinessObjectFactory();
				var newPayment1 = newFactory.Load<AccPaymentApproval>(payment1.PK);
				var newPayment2 = newFactory.Load<AccPaymentApproval>(payment2.PK);
				var newPayment3 = newFactory.Load<AccPaymentApproval>(payment3.PK);

				AssertEquals(1000m, newPayment1.AV_Amount);
				AssertEquals(2000m, newPayment2.AV_Amount);
				AssertEquals(3000m, newPayment3.AV_Amount);

				AssertEquals(1.5m, newPayment1.AV_PayExRate);
				AssertEquals(2.5m, newPayment2.AV_PayExRate);
				AssertEquals(3.5m, newPayment3.AV_PayExRate);

				AssertEquals(100m, newPayment1.AV_ExchangeDifference);
				AssertEquals(200m, newPayment1.AV_Discount);
			}
		}

		public void TestEditPaymentBatch_SaveAsDraft_GridMenuItems()
		{
			SetupDataForPostingTest();

			using (var form = new PaymentBatchForm_ForTestOnly(BatchPoster))
			{
				form.Show();
				BatchPoster.APB_PaymentType = ReceiptTypes.Cheque;
				BatchPoster.APB_AB = TestBank.PK;

				form.PaymentBatchGrid_ForTestOnly.ContextMenu.ShowPopupMenu();

				AssertNotNull(form.PaymentBatchGrid_ForTestOnly.ContextMenu.MenuItems.FindByText("Remove"));
				AssertNotNull(form.PaymentBatchGrid_ForTestOnly.ContextMenu.MenuItems.FindByText("Edit Payment Organization Detail"));

				var saveAsDraftButton = form.Controls.Find("SaveAsDraftButton", true)[0] as ZButton;
				saveAsDraftButton.PerformClick();
				BatchPostingHelper.AssertBatchPosterWillMatchAndPost(BatchPoster, 3, 0, 2, ZBool.False);

				form.PaymentBatchGrid_ForTestOnly.ContextMenu.ShowPopupMenu();

				AssertNull(form.PaymentBatchGrid_ForTestOnly.ContextMenu.MenuItems.FindByText("Remove"));
				AssertNull(form.PaymentBatchGrid_ForTestOnly.ContextMenu.MenuItems.FindByText("Edit Payment Organization Detail"));
			}
		}

		public void TestEditPaymentBatch_MatchTransactionsGridMenuItems()
		{
			SetupNewPaymentBatch();
			using (Factory.SetTempContext(BusinessContext.SavingPaymentApprovalAsDraft))
			{
				BatchPoster.PaymentApprovalCollection[0].AV_Status = PaymentApprovalStatus.Draft;
				BatchPoster.MatchTransactions();
				Factory.Save();
			}

			var controller = ZControllerFactory.Create(ControllerIDs.PaymentBatch);
			using (var form = controller.ShowEditForm(BatchPoster) as PaymentBatchForm)
			{
				Application.DoEvents();

				AssertEquals("Percondition", true, BatchPoster.IsInDatabase);
				AssertEquals("Percondition", ODisplayMode.Browse, form.DisplayMode);

				var poster = form.BusinessEntity as APPaymentBatchPoster;
				poster.APB_PaymentDate = ZDateTime.Now.AddDays(3);

				AssertEquals(ODisplayMode.Edit, form.DisplayMode);

				form.MatchTransactionsGrid_ForTestOnly.Select(0);
				form.MatchTransactionsGrid_ForTestOnly.ContextMenu.ShowPopupMenu();
				AssertNotNull("Percondition", form.SelectedPayment_ForTestOnly);
				AssertNotNull(form.MatchTransactionsGrid_ForTestOnly.ContextMenu.MenuItems.FindByText("View"));
				AssertNull(form.MatchTransactionsGrid_ForTestOnly.ContextMenu.MenuItems.FindByText("Remove"));
				AssertNotNull(form.MatchTransactionsGrid_ForTestOnly.ContextMenu.MenuItems.FindByText("Add Overpayment"));
				AssertNotNull(form.MatchTransactionsGrid_ForTestOnly.ContextMenu.MenuItems.FindByText("Add Exchange Difference"));
				AssertNotNull(form.MatchTransactionsGrid_ForTestOnly.ContextMenu.MenuItems.FindByText("Add Discount"));

				form.SaveAsDraftButton_ForTestOnly.PerformClick();
				AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);

				form.MatchTransactionsGrid_ForTestOnly.Select(0);
				form.MatchTransactionsGrid_ForTestOnly.ContextMenu.ShowPopupMenu();
				AssertEquals(ODisplayMode.Browse, form.DisplayMode);
				AssertEquals(1, form.MatchTransactionsGrid_ForTestOnly.SelectedElements.Length);
				AssertNotNull(form.MatchTransactionsGrid_ForTestOnly.ContextMenu.MenuItems.FindByText("View"));
				AssertNull("The grid should be not editable once 'Save as draft'.", form.MatchTransactionsGrid_ForTestOnly.ContextMenu.MenuItems.FindByText("Remove"));
				AssertNull(form.MatchTransactionsGrid_ForTestOnly.ContextMenu.MenuItems.FindByText("Add Overpayment"));
				AssertNull(form.MatchTransactionsGrid_ForTestOnly.ContextMenu.MenuItems.FindByText("Add Exchange Difference"));
				AssertNull(form.MatchTransactionsGrid_ForTestOnly.ContextMenu.MenuItems.FindByText("Add Discount"));
			}
		}

		public void TestEditPaymentBatch_PaymentBatchGridMenuItems()
		{
			SetupNewPaymentBatch();
			using (Factory.SetTempContext(BusinessContext.SavingPaymentApprovalAsDraft))
			{
				BatchPoster.PaymentApprovalCollection[0].AV_Status = PaymentApprovalStatus.Draft;
				BatchPoster.MatchTransactions();
				Factory.Save();
			}

			var controller = ZControllerFactory.Create(ControllerIDs.PaymentBatch);

			var registryValue = TestObjectCreator.CreatePaymentAuthorisationSettings(PaymentAuthorisationSettings.RangeCodes.Above, 0M, PaymentAuthorisationSettings.AuthorisationRequirementCodes.FirstApprovalRequiredOnly);
			AccountingConfigurationRegistry.Instance.PayableAuthorizationSettings.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryValue);

			using (var form = controller.ShowEditForm(BatchPoster) as PaymentBatchForm)
			{
				Application.DoEvents();

				var poster = form.BusinessEntity as APPaymentBatchPoster;
				AssertEquals("Percondition", true, poster.IsInDatabase);

				form.PaymentBatchGrid_ForTestOnly.ContextMenu.ShowPopupMenu();
				AssertNotNull("Percondition", form.SelectedPayment_ForTestOnly);
				AssertEquals("Percondition", ODisplayMode.Browse, form.DisplayMode);
				AssertNotNull(form.PaymentBatchGrid_ForTestOnly.ContextMenu.MenuItems.FindByText("Remove"));
				AssertNotNull(form.PaymentBatchGrid_ForTestOnly.ContextMenu.MenuItems.FindByText("Edit Payment Organization Detail"));

				var authoriseMenuItem = form.PaymentBatchGrid_ForTestOnly.ContextMenu.MenuItems.FindByText("Actio&ns").MenuItems.FindByText("Authorization");
				AssertNotNull(authoriseMenuItem);
				AssertEquals(3, authoriseMenuItem.MenuItems.Count);
				AssertNotNull(authoriseMenuItem.MenuItems.FindByText("Authorize"));
				AssertNotNull(authoriseMenuItem.MenuItems.FindByText("Unauthorize"));
				AssertNotNull(authoriseMenuItem.MenuItems.FindByText("Reject"));

				var postMenuItem = form.PaymentBatchGrid_ForTestOnly.ContextMenu.MenuItems.FindByText("Actio&ns").MenuItems.FindByText("Post Approvals");
				AssertNotNull(postMenuItem);
				AssertEquals(3, postMenuItem.MenuItems.Count);
				AssertNotNull(postMenuItem.MenuItems.FindByText("Post"));
				AssertNotNull(postMenuItem.MenuItems.FindByText("Allocate Check No."));
				AssertNotNull(postMenuItem.MenuItems.FindByText("Allocate Check No. and Post"));

				var cancelMenuItem = form.PaymentBatchGrid_ForTestOnly.ContextMenu.MenuItems.FindByText("Actio&ns").MenuItems.FindByText("Cancel");
				AssertNotNull(cancelMenuItem);
				AssertEquals(0, cancelMenuItem.MenuItems.Count);

				var submitForApprovalMenuItem = form.PaymentBatchGrid_ForTestOnly.ContextMenu.MenuItems.FindByText("Actio&ns").MenuItems.FindByText("Submit for Approval");
				AssertNotNull(submitForApprovalMenuItem);
				AssertEquals(0, submitForApprovalMenuItem.MenuItems.Count);

				AssertEquals("Percondition", ODisplayMode.Browse, form.DisplayMode);
				poster.APB_PaymentDate = ZDateTime.Now.AddDays(3);
				AssertEquals(ODisplayMode.Edit, form.DisplayMode);

				form.SaveAsDraftButton_ForTestOnly.PerformClick();
				AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);

				form.PaymentBatchGrid_ForTestOnly.ContextMenu.ShowPopupMenu();
				AssertNotNull(form.PaymentBatchGrid_ForTestOnly.ContextMenu.MenuItems.FindByText("Actio&ns").MenuItems.FindByText("Authorization"));
				AssertNotNull(form.PaymentBatchGrid_ForTestOnly.ContextMenu.MenuItems.FindByText("Actio&ns").MenuItems.FindByText("Post Approvals"));
				AssertNotNull(form.PaymentBatchGrid_ForTestOnly.ContextMenu.MenuItems.FindByText("Actio&ns").MenuItems.FindByText("Cancel"));
				AssertNotNull(form.PaymentBatchGrid_ForTestOnly.ContextMenu.MenuItems.FindByText("Actio&ns").MenuItems.FindByText("Submit for Approval"));
				AssertNull("The grid should be not editable once 'Save as draft'.", form.PaymentBatchGrid_ForTestOnly.ContextMenu.MenuItems.FindByText("Remove"));
				AssertNull(form.PaymentBatchGrid_ForTestOnly.ContextMenu.MenuItems.FindByText("Edit Payment Organization Detail"));
			}
		}

		public void TestEditPaymentBatchWhenPaymentAuthorisationRequiredSettingUpdated_GridMenuItemSubmitForApproval()
		{
			SetupNewPaymentBatch();
			using (Factory.SetTempContext(BusinessContext.SavingPaymentApprovalAsDraft))
			{
				BatchPoster.PaymentApprovalCollection[0].AV_Status = PaymentApprovalStatus.Draft;
				BatchPoster.MatchTransactions();
				Factory.Save();
			}

			var controller = ZControllerFactory.Create(ControllerIDs.PaymentBatch);

			using (AccountingConfigurationRegistry.Instance.PayableAuthorizationSettings.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, TestObjectCreator.CreatePaymentAuthorisationSettingsEmpty()))
			using (var form = controller.ShowEditForm(BatchPoster) as PaymentBatchForm)
			{
				AssertEquals(0, AccountingConfigurationRegistry.Instance.PayableAuthorizationSettings.Value.Count);

				var submitForApprovalMenuItem = form.PaymentBatchGrid_ForTestOnly.ContextMenu.MenuItems.FindByText("Actio&ns").MenuItems.FindByText("Submit for Approval");
				AssertNull(submitForApprovalMenuItem);

				var approveForPostingMenuItem = form.PaymentBatchGrid_ForTestOnly.ContextMenu.MenuItems.FindByText("Actio&ns").MenuItems.FindByText("Approve for Posting");
				AssertNotNull(approveForPostingMenuItem);
				AssertEquals(0, approveForPostingMenuItem.MenuItems.Count);
				AssertNotNull(form.PaymentBatchGrid_ForTestOnly.ContextMenu.MenuItems.FindByText("Actio&ns").MenuItems.FindByText("Approve for Posting"));
			}

			using (AccountingConfigurationRegistry.Instance.PayableAuthorizationSettings.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, TestObjectCreator.CreatePaymentAuthorisationSettingsExample()))
			using (var form = controller.ShowEditForm(BatchPoster) as PaymentBatchForm)
			{
				var submitForApprovalMenuItem = form.PaymentBatchGrid_ForTestOnly.ContextMenu.MenuItems.FindByText("Actio&ns").MenuItems.FindByText("Submit for Approval");
				AssertNotNull(submitForApprovalMenuItem);
				AssertEquals(0, submitForApprovalMenuItem.MenuItems.Count);

				var approveForPostingMenuItem = form.PaymentBatchGrid_ForTestOnly.ContextMenu.MenuItems.FindByText("Actio&ns").MenuItems.FindByText("Approve for Posting");
				AssertNull(approveForPostingMenuItem);

				AssertNotNull(form.PaymentBatchGrid_ForTestOnly.ContextMenu.MenuItems.FindByText("Actio&ns").MenuItems.FindByText("Submit for Approval"));
			}

			using (AccountingConfigurationRegistry.Instance.PayableAuthorizationSettings.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, TestObjectCreator.CreatePaymentAuthorisationSettings(RangeCodes.Above, 0, AuthorisationCodes.NoApprovalRequired)))
			using (var form = controller.ShowEditForm(BatchPoster) as PaymentBatchForm)
			{
				var submitForApprovalMenuItem = form.PaymentBatchGrid_ForTestOnly.ContextMenu.MenuItems.FindByText("Actio&ns").MenuItems.FindByText("Submit for Approval");
				AssertNull(submitForApprovalMenuItem);

				var approveForPostingMenuItem = form.PaymentBatchGrid_ForTestOnly.ContextMenu.MenuItems.FindByText("Actio&ns").MenuItems.FindByText("Approve for Posting");
				AssertNotNull(approveForPostingMenuItem);
				AssertEquals(0, approveForPostingMenuItem.MenuItems.Count);

				AssertNotNull(form.PaymentBatchGrid_ForTestOnly.ContextMenu.MenuItems.FindByText("Actio&ns").MenuItems.FindByText("Approve for Posting"));
			}
		}

		public void TestEditPaymentBatch_SubmitForApproval()
		{
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, "BUY", 1m);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.CNY, "BUY", 2m);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.TWD, "BUY", 3m);

			SetupDataForBaseTest();
			var poster = BatchPostingHelper.CreateBatchPosterWithMultipleTransactionsWithForeignCurrency();
			Factory.Save();

			var controller = ZControllerFactory.Create(ControllerIDs.PaymentBatch);
			using (AccountingConfigurationRegistry.Instance.PayableAuthorizationSettings.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, TestObjectCreator.CreatePaymentAuthorisationSettingsExample()))
			using (var form = controller.ShowEditForm(poster) as PaymentBatchForm)
			{
				Application.DoEvents();

				poster = form.BusinessEntity as APPaymentBatchPoster;
				var paymentApprovalCollection = poster.PaymentApprovalCollection.Cast<PaymentApprovalBase>();
				var payment1 = paymentApprovalCollection.Single(x => x.CurrencyCode == TestObjectCreator.USD.Code);
				var payment2 = paymentApprovalCollection.Single(x => x.CurrencyCode == TestObjectCreator.CNY.Code);
				var payment3 = paymentApprovalCollection.Single(x => x.CurrencyCode == TestObjectCreator.TWD.Code);

				AssertEquals("Percondition", true, payment1.IsDraft);
				AssertEquals("Percondition", true, payment2.IsDraft);
				AssertEquals("Percondition", true, payment3.IsDraft);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.PaymentBatchGrid_ForTestOnly.SelectSingleElement(payment1);
				form.PaymentBatchGrid_ForTestOnly.ContextMenu.ShowPopupMenu();
				form.PaymentBatchGrid_ForTestOnly.ContextMenu.MenuItems.FindByText("Actio&ns").MenuItems.FindByText("Submit for Approval").PerformClick();

				AssertEquals($@"The following Payments will be submitted for approval:
{payment1.GetDescription()}

Do you want to submit these Payments for approval?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(true, payment1.IsFullyApproved);
				AssertEquals(true, payment2.IsDraft);
				AssertEquals(true, payment3.IsDraft);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.PaymentBatchGrid_ForTestOnly.SelectAllElements();
				form.PaymentBatchGrid_ForTestOnly.UnSelect(poster.PaymentApprovalCollection.IndexOf(x => x.PK == payment3.PK));
				form.PaymentBatchGrid_ForTestOnly.ContextMenu.MenuItems.FindByText("Actio&ns").MenuItems.FindByText("Submit for Approval").PerformClick();
				AssertEquals($@"The following Payments cannot be submitted for approval:
{payment1.GetDescription()}
This Payment is not in Draft status

The following Payments will be submitted for approval:
{payment2.GetDescription()}

Do you want to submit these Payments for approval?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(true, payment1.IsFullyApproved);
				AssertEquals(true, payment2.IsFullyApproved);
				AssertEquals(true, payment3.IsDraft);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.PaymentBatchGrid_ForTestOnly.UnSelectAll();
				form.ActionsMenuItem_ForTestOnly.MenuItems.FindByText("Submit for Approval").PerformClick();
				AssertContains("The following Payments cannot be submitted for approval:", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertContains($"{payment1.GetDescription()}\r\nThis Payment is not in Draft status", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertContains($"{payment2.GetDescription()}\r\nThis Payment is not in Draft status", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertContains($@"The following Payments will be submitted for approval:
{payment3.GetDescription()}

Do you want to submit these Payments for approval?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(true, payment1.IsFullyApproved);
				AssertEquals(true, payment2.IsFullyApproved);
				AssertEquals(true, payment3.IsFullyApproved);
			}
		}

		public void TestEditPaymentBatch_ApproveForPosting()
		{
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, "BUY", 1m);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.CNY, "BUY", 2m);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.TWD, "BUY", 3m);

			SetupDataForBaseTest();
			var poster = BatchPostingHelper.CreateBatchPosterWithMultipleTransactionsWithForeignCurrency();
			Factory.Save();

			var controller = ZControllerFactory.Create(ControllerIDs.PaymentBatch);
			using (var form = controller.ShowEditForm(poster) as PaymentBatchForm)
			{
				Application.DoEvents();

				poster = form.BusinessEntity as APPaymentBatchPoster;
				var paymentApprovalCollection = poster.PaymentApprovalCollection.Cast<PaymentApprovalBase>();
				var payment1 = paymentApprovalCollection.Single(x => x.CurrencyCode == TestObjectCreator.USD.Code);
				var payment2 = paymentApprovalCollection.Single(x => x.CurrencyCode == TestObjectCreator.CNY.Code);
				var payment3 = paymentApprovalCollection.Single(x => x.CurrencyCode == TestObjectCreator.TWD.Code);

				AssertEquals("Percondition", true, payment1.IsDraft);
				AssertEquals("Percondition", true, payment2.IsDraft);
				AssertEquals("Percondition", true, payment3.IsDraft);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.PaymentBatchGrid_ForTestOnly.SelectSingleElement(payment1);
				form.PaymentBatchGrid_ForTestOnly.ContextMenu.ShowPopupMenu();
				form.PaymentBatchGrid_ForTestOnly.ContextMenu.MenuItems.FindByText("Actio&ns").MenuItems.FindByText("Approve for Posting").PerformClick();

				AssertEquals($@"The following Payments will be approved for posting:
{payment1.GetDescription()}

Do you want to approve these Payments for posting?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(true, payment1.IsFullyApproved);
				AssertEquals(true, payment2.IsDraft);
				AssertEquals(true, payment3.IsDraft);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.PaymentBatchGrid_ForTestOnly.SelectAllElements();
				form.PaymentBatchGrid_ForTestOnly.UnSelect(poster.PaymentApprovalCollection.IndexOf(x => x.PK == payment3.PK));
				form.PaymentBatchGrid_ForTestOnly.ContextMenu.MenuItems.FindByText("Actio&ns").MenuItems.FindByText("Approve for Posting").PerformClick();
				AssertEquals($@"The following Payments cannot be approved for posting:
{payment1.GetDescription()}
This Payment is not in Draft status

The following Payments will be approved for posting:
{payment2.GetDescription()}

Do you want to approve these Payments for posting?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(true, payment1.IsFullyApproved);
				AssertEquals(true, payment2.IsFullyApproved);
				AssertEquals(true, payment3.IsDraft);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.PaymentBatchGrid_ForTestOnly.UnSelectAll();
				form.ActionsMenuItem_ForTestOnly.MenuItems.FindByText("Approve for Posting").PerformClick();
				AssertContains("The following Payments cannot be approved for posting:", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertContains($"{payment1.GetDescription()}\r\nThis Payment is not in Draft status", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertContains($"{payment2.GetDescription()}\r\nThis Payment is not in Draft status", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertContains($@"The following Payments will be approved for posting:
{payment3.GetDescription()}

Do you want to approve these Payments for posting?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(true, payment1.IsFullyApproved);
				AssertEquals(true, payment2.IsFullyApproved);
				AssertEquals(true, payment3.IsFullyApproved);
			}
		}

		public void TestEditPaymentBatch_CheckBeforeProcessingPayments_HasChanges()
		{
			SetupDataForBaseTest();
			var poster = BatchPostingHelper.CreateBatchPosterWithMultipleTransactionsWithForeignCurrency();
			Factory.Save();
			var controller = ZControllerFactory.Create(ControllerIDs.PaymentBatch);

			using (AccountingConfigurationRegistry.Instance.PayableAuthorizationSettings.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, TestObjectCreator.CreatePaymentAuthorisationSettingsExample()))
			using (var form = controller.ShowEditForm(poster) as PaymentBatchForm)
			{
				Application.DoEvents();
				poster = form.BusinessEntity as APPaymentBatchPoster;

				AssertEquals("Percondition", true, BatchPoster.IsInDatabase);
				AssertEquals("Percondition", ODisplayMode.Browse, form.DisplayMode);

				var authoriseMenuItem = form.ActionsMenuItem_ForTestOnly.MenuItems.FindByText("Authorization");
				var postApprovalsMenuItem = form.ActionsMenuItem_ForTestOnly.MenuItems.FindByText("Post Approvals");
				var menuitems = new List<MenuItem>
				{
					authoriseMenuItem.MenuItems.FindByText("Authorize"),
					authoriseMenuItem.MenuItems.FindByText("Unauthorize"),
					authoriseMenuItem.MenuItems.FindByText("Reject"),
					postApprovalsMenuItem.MenuItems.FindByText("Post"),
					postApprovalsMenuItem.MenuItems.FindByText("Allocate Check No."),
					postApprovalsMenuItem.MenuItems.FindByText("Allocate Check No. and Post"),
					form.ActionsMenuItem_ForTestOnly.MenuItems.FindByText("Cancel"),
					form.ActionsMenuItem_ForTestOnly.MenuItems.FindByText("Submit for Approval"),
				};

				poster.APB_PaymentDate = ZDateTime.Now.AddDays(3);
				AssertEquals(ODisplayMode.Edit, form.DisplayMode);
				AssertEquals(true, poster.HasChanges);
				foreach (var menuitem in menuitems)
				{
					menuitem.PerformClick();
					AssertEquals("Please save this payment batch first.", UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessages();
				}
			}
		}

		public void TestEditPaymentBatch_CheckBeforeProcessingPayments_IsDiscrepancyWithPaymentBatch()
		{
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, "BUY", 1m);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.CNY, "BUY", 2m);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.TWD, "BUY", 3m);

			SetupDataForBaseTest();
			var poster = BatchPostingHelper.CreateBatchPosterWithMultipleTransactionsWithForeignCurrency();
			Factory.Save();

			poster.PaymentApprovalCollection[0].AV_PaymentType = ReceiptTypes.Cash;
			Factory.Save();

			int menuitemCount = 8;
			for (int i = 0; i < menuitemCount; i++)
			{
				var controller = ZControllerFactory.Create(ControllerIDs.PaymentBatch);
				using (AccountingConfigurationRegistry.Instance.PayableAuthorizationSettings.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, TestObjectCreator.CreatePaymentAuthorisationSettingsExample()))
				using (var form = controller.ShowEditForm(poster) as PaymentBatchForm)
				{
					Application.DoEvents();
					poster = form.BusinessEntity as APPaymentBatchPoster;

					AssertEquals("Percondition", true, BatchPoster.IsInDatabase);
					AssertEquals("Percondition", ODisplayMode.Browse, form.DisplayMode);

					var authoriseMenuItem = form.ActionsMenuItem_ForTestOnly.MenuItems.FindByText("Authorization");
					var postApprovalsMenuItem = form.ActionsMenuItem_ForTestOnly.MenuItems.FindByText("Post Approvals");
					var menuitems = new List<MenuItem>
					{
						authoriseMenuItem.MenuItems.FindByText("Authorize"),
						authoriseMenuItem.MenuItems.FindByText("Unauthorize"),
						authoriseMenuItem.MenuItems.FindByText("Reject"),
						postApprovalsMenuItem.MenuItems.FindByText("Post"),
						postApprovalsMenuItem.MenuItems.FindByText("Allocate Check No."),
						postApprovalsMenuItem.MenuItems.FindByText("Allocate Check No. and Post"),
						form.ActionsMenuItem_ForTestOnly.MenuItems.FindByText("Cancel"),
						form.ActionsMenuItem_ForTestOnly.MenuItems.FindByText("Submit for Approval"),
					};

					AssertEquals(menuitemCount, menuitems.Count);
					AssertEquals(false, poster.HasChanges);
					AssertEquals(true, poster.PaymentApprovalCollection.OfType<PaymentApprovalBase>().Any(x => x.IsDiscrepancyWithPaymentBatch));

					AssertEquals(false, poster.NotificationsIncludingChildren.HasErrors());
					menuitems[i].PerformClick();
					AssertEquals(true, poster.NotificationsIncludingChildren.HasErrors());
					AssertEquals("There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals(true, poster.NotificationsIncludingChildren.Contains("Error - AV_AK: Payment approval detail does not match payment batch header - please reconcile Check Book with payment batch header manually or by re-entering header details."));
					AssertEquals(true, poster.NotificationsIncludingChildren.Contains("Error - AV_PaymentType: Payment approval detail does not match payment batch header - please reconcile Payment Method with payment batch header manually or by re-entering header details."));
					UnitTestUserNotification.Instance.ClearMessages();
				}
			}
		}

		public void TestEditPaymentBatch_AuthorisePaymentApprovals()
		{
			SetupDataForBaseTest();
			var poster = BatchPostingHelper.CreateBatchPosterWithMultipleTransactionsWithForeignCurrency();
			Factory.Save();
			var paymentApprovalCollection = poster.PaymentApprovalCollection.Cast<PaymentApprovalBase>();
			var payment1 = paymentApprovalCollection.Single(x => x.CurrencyCode == TestObjectCreator.USD.Code);
			var payment2 = paymentApprovalCollection.Single(x => x.CurrencyCode == TestObjectCreator.CNY.Code);
			var payment3 = paymentApprovalCollection.Single(x => x.CurrencyCode == TestObjectCreator.TWD.Code);
			payment1.AV_Status = payment2.AV_Status = payment3.AV_Status = PaymentApprovalStatus.AwaitingApproval;
			Factory.Save();

			var controller = ZControllerFactory.Create(ControllerIDs.PaymentBatch);
			using (var form = controller.ShowEditForm(poster) as PaymentBatchForm)
			{
				Application.DoEvents();

				poster = form.BusinessEntity as APPaymentBatchPoster;
				paymentApprovalCollection = poster.PaymentApprovalCollection.Cast<PaymentApprovalBase>();
				payment1 = paymentApprovalCollection.Single(x => x.CurrencyCode == TestObjectCreator.USD.Code);
				payment2 = paymentApprovalCollection.Single(x => x.CurrencyCode == TestObjectCreator.CNY.Code);
				payment3 = paymentApprovalCollection.Single(x => x.CurrencyCode == TestObjectCreator.TWD.Code);

				AssertEquals("Percondition", true, payment1.IsAwaitingApproval);
				AssertEquals("Percondition", true, payment2.IsAwaitingApproval);
				AssertEquals("Percondition", true, payment3.IsAwaitingApproval);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.PaymentBatchGrid_ForTestOnly.SelectSingleElement(payment1);
				form.PaymentBatchGrid_ForTestOnly.ContextMenu.ShowPopupMenu();
				form.PaymentBatchGrid_ForTestOnly.ContextMenu.MenuItems.FindByText("Actio&ns").MenuItems.FindByText("Authorization").MenuItems.FindByText("Authorize").PerformClick();

				AssertEquals($@"The following Payments will be authorized:
{payment1.GetDescription()}

Do you want to authorize these Payments?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(true, payment1.IsFullyApproved);
				AssertEquals(true, payment2.IsAwaitingApproval);
				AssertEquals(true, payment3.IsAwaitingApproval);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.PaymentBatchGrid_ForTestOnly.SelectAllElements();
				form.PaymentBatchGrid_ForTestOnly.UnSelect(poster.PaymentApprovalCollection.IndexOf(x => x.PK == payment3.PK));
				form.PaymentBatchGrid_ForTestOnly.ContextMenu.MenuItems.FindByText("Actio&ns").MenuItems.FindByText("Authorization").MenuItems.FindByText("Authorize").PerformClick();
				AssertEquals($@"The following Payments cannot be authorized:
{payment1.GetDescription()}
This Payment is already Fully Approved

The following Payments will be authorized:
{payment2.GetDescription()}

Do you want to authorize these Payments?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(true, payment1.IsFullyApproved);
				AssertEquals(true, payment2.IsFullyApproved);
				AssertEquals(true, payment3.IsAwaitingApproval);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.PaymentBatchGrid_ForTestOnly.UnSelectAll();
				form.ActionsMenuItem_ForTestOnly.MenuItems.FindByText("Authorization").MenuItems.FindByText("Authorize").PerformClick();
				AssertContains("The following Payments cannot be authorized:", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertContains($"{payment1.GetDescription()}\r\nThis Payment is already Fully Approved", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertContains($"{payment2.GetDescription()}\r\nThis Payment is already Fully Approved", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertContains($@"The following Payments will be authorized:
{payment3.GetDescription()}

Do you want to authorize these Payments?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(true, payment1.IsFullyApproved);
				AssertEquals(true, payment2.IsFullyApproved);
				AssertEquals(true, payment3.IsFullyApproved);
			}
		}

		public void TestEditPaymentBatch_UnAuthorisePaymentApprovals()
		{
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, "BUY", 1m);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.CNY, "BUY", 2m);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.TWD, "BUY", 3m);

			SetupDataForBaseTest();
			var poster = BatchPostingHelper.CreateBatchPosterWithMultipleTransactionsWithForeignCurrency();
			Factory.Save();
			poster.PostPaymentsAsPaymentApprovals = true;
			var paymentApprovalCollection = poster.PaymentApprovalCollection.Cast<PaymentApprovalBase>();
			var payment1 = paymentApprovalCollection.Single(x => x.CurrencyCode == TestObjectCreator.USD.Code);
			var payment2 = paymentApprovalCollection.Single(x => x.CurrencyCode == TestObjectCreator.CNY.Code);
			var payment3 = paymentApprovalCollection.Single(x => x.CurrencyCode == TestObjectCreator.TWD.Code);
			payment1.AV_GS_NKApproval1st = payment2.AV_GS_NKApproval1st = payment3.AV_GS_NKApproval1st = GlbStaff.CurrentUser.GS_Code;
			payment1.AV_Status = payment2.AV_Status = payment3.AV_Status = PaymentApprovalStatus.FullyApproved;
			Factory.Save();

			var payableAuthorizationSettings = new PaymentAuthorisationSettingsCollection();
			var setting = payableAuthorizationSettings.AddNew();
			setting.Amount = 0m;
			setting.AuthorisationRequirement = AuthorisationCodes.FirstApprovalRequiredOnly;
			setting.Range = RangeCodes.Above;

			var controller = ZControllerFactory.Create(ControllerIDs.PaymentBatch);
			using (AccountingConfigurationRegistry.Instance.PayableAuthorizationSettings.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, payableAuthorizationSettings))
			using (var form = controller.ShowEditForm(poster) as PaymentBatchForm)
			{
				Application.DoEvents();

				poster = form.BusinessEntity as APPaymentBatchPoster;
				paymentApprovalCollection = poster.PaymentApprovalCollection.Cast<PaymentApprovalBase>();
				payment1 = paymentApprovalCollection.Single(x => x.CurrencyCode == TestObjectCreator.USD.Code);
				payment2 = paymentApprovalCollection.Single(x => x.CurrencyCode == TestObjectCreator.CNY.Code);
				payment3 = paymentApprovalCollection.Single(x => x.CurrencyCode == TestObjectCreator.TWD.Code);

				AssertEquals("Percondition", true, payment1.IsFullyApproved);
				AssertEquals("Percondition", true, payment2.IsFullyApproved);
				AssertEquals("Percondition", true, payment3.IsFullyApproved);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.PaymentBatchGrid_ForTestOnly.SelectSingleElement(payment1);
				form.PaymentBatchGrid_ForTestOnly.ContextMenu.ShowPopupMenu();
				form.PaymentBatchGrid_ForTestOnly.ContextMenu.MenuItems.FindByText("Actio&ns").MenuItems.FindByText("Authorization").MenuItems.FindByText("Unauthorize").PerformClick();

				AssertEquals($@"The following Payments will be unauthorized:
{payment1.GetDescription()}

Do you want to unauthorize these Payments?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(true, payment1.IsAwaitingApproval);
				AssertEquals(true, payment2.IsFullyApproved);
				AssertEquals(true, payment3.IsFullyApproved);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.PaymentBatchGrid_ForTestOnly.SelectAllElements();
				form.PaymentBatchGrid_ForTestOnly.UnSelect(poster.PaymentApprovalCollection.IndexOf(x => x.PK == payment3.PK));
				form.PaymentBatchGrid_ForTestOnly.ContextMenu.MenuItems.FindByText("Actio&ns").MenuItems.FindByText("Authorization").MenuItems.FindByText("Unauthorize").PerformClick();
				AssertContains("The following Payments will be unauthorized:", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertContains($"{payment1.GetDescription()}\r\n", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertContains($"{payment2.GetDescription()}\r\n", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNotContains("The following Payments cannot be unauthorized:", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(true, payment1.IsAwaitingApproval);
				AssertEquals(true, payment2.IsAwaitingApproval);
				AssertEquals(true, payment3.IsFullyApproved);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.PaymentBatchGrid_ForTestOnly.UnSelectAll();
				form.ActionsMenuItem_ForTestOnly.MenuItems.FindByText("Authorization").MenuItems.FindByText("Unauthorize").PerformClick();
				AssertContains("The following Payments will be unauthorized:", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertContains($"{payment1.GetDescription()}\r\n", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertContains($"{payment2.GetDescription()}\r\n", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertContains($"{payment3.GetDescription()}\r\n", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNotContains("The following Payments cannot be unauthorized:", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(true, payment1.IsAwaitingApproval);
				AssertEquals(true, payment2.IsAwaitingApproval);
				AssertEquals(true, payment3.IsAwaitingApproval);
			}
		}

		public void TestEditPaymentBatch_RejectPaymentApprovals()
		{
			SetupDataForBaseTest();
			var poster = BatchPostingHelper.CreateBatchPosterWithMultipleTransactionsWithForeignCurrency();
			Factory.Save();
			var paymentApprovalCollection = poster.PaymentApprovalCollection.Cast<PaymentApprovalBase>();
			var payment1 = paymentApprovalCollection.Single(x => x.CurrencyCode == TestObjectCreator.USD.Code);
			var payment2 = paymentApprovalCollection.Single(x => x.CurrencyCode == TestObjectCreator.CNY.Code);
			var payment3 = paymentApprovalCollection.Single(x => x.CurrencyCode == TestObjectCreator.TWD.Code);
			payment1.AV_Status = payment2.AV_Status = payment3.AV_Status = PaymentApprovalStatus.AwaitingApproval;
			Factory.Save();

			var controller = ZControllerFactory.Create(ControllerIDs.PaymentBatch);
			using (var form = controller.ShowEditForm(poster) as PaymentBatchForm)
			{
				Application.DoEvents();

				poster = form.BusinessEntity as APPaymentBatchPoster;
				paymentApprovalCollection = poster.PaymentApprovalCollection.Cast<PaymentApprovalBase>();
				payment1 = paymentApprovalCollection.Single(x => x.CurrencyCode == TestObjectCreator.USD.Code);
				payment2 = paymentApprovalCollection.Single(x => x.CurrencyCode == TestObjectCreator.CNY.Code);
				payment3 = paymentApprovalCollection.Single(x => x.CurrencyCode == TestObjectCreator.TWD.Code);

				AssertEquals("Percondition", true, payment1.IsAwaitingApproval);
				AssertEquals("Percondition", true, payment2.IsAwaitingApproval);
				AssertEquals("Percondition", true, payment3.IsAwaitingApproval);

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(x => ((PaymentRejectionReasonForm)x).SetReason("INS", "Not enough money"));

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.PaymentBatchGrid_ForTestOnly.SelectSingleElement(payment1);
				form.PaymentBatchGrid_ForTestOnly.ContextMenu.ShowPopupMenu();
				form.PaymentBatchGrid_ForTestOnly.ContextMenu.MenuItems.FindByText("Actio&ns").MenuItems.FindByText("Authorization").MenuItems.FindByText("Reject").PerformClick();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(true, payment1.IsRejected);
				AssertEquals(true, payment2.IsAwaitingApproval);
				AssertEquals(true, payment3.IsAwaitingApproval);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.PaymentBatchGrid_ForTestOnly.SelectAllElements();
				form.PaymentBatchGrid_ForTestOnly.UnSelect(poster.PaymentApprovalCollection.IndexOf(x => x.PK == payment3.PK));
				form.PaymentBatchGrid_ForTestOnly.ContextMenu.MenuItems.FindByText("Actio&ns").MenuItems.FindByText("Authorization").MenuItems.FindByText("Reject").PerformClick();
				AssertEquals($@"The following Payments cannot be rejected:
{payment1.GetDescription()}
This Payment is already Rejected", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(true, payment1.IsRejected);
				AssertEquals(true, payment2.IsRejected);
				AssertEquals(true, payment3.IsAwaitingApproval);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.PaymentBatchGrid_ForTestOnly.UnSelectAll();
				form.ActionsMenuItem_ForTestOnly.MenuItems.FindByText("Authorization").MenuItems.FindByText("Reject").PerformClick();
				AssertContains("The following Payments cannot be rejected:", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertContains($"{payment1.GetDescription()}\r\nThis Payment is already Rejected", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertContains($"{payment2.GetDescription()}\r\nThis Payment is already Rejected", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(true, payment1.IsRejected);
				AssertEquals(true, payment2.IsRejected);
				AssertEquals(true, payment3.IsRejected);
			}
		}

		public void TestEditPaymentBatch_PostPaymentApprovals()
		{
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, "BUY", 1m);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.CNY, "BUY", 2m);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.TWD, "BUY", 3m);

			SetupDataForBaseTest();
			var poster = BatchPostingHelper.CreateBatchPosterWithMultipleTransactionsWithForeignCurrency();
			Factory.Save();
			poster.PostPaymentsAsPaymentApprovals = true;
			var paymentApprovalCollection = poster.PaymentApprovalCollection.Cast<PaymentApprovalBase>();
			var payment1 = paymentApprovalCollection.Single(x => x.CurrencyCode == TestObjectCreator.USD.Code);
			var payment2 = paymentApprovalCollection.Single(x => x.CurrencyCode == TestObjectCreator.CNY.Code);
			var payment3 = paymentApprovalCollection.Single(x => x.CurrencyCode == TestObjectCreator.TWD.Code);
			payment1.AV_Status = payment2.AV_Status = payment3.AV_Status = PaymentApprovalStatus.FullyApproved;
			Factory.Save();

			var controller = ZControllerFactory.Create(ControllerIDs.PaymentBatch);
			using (var form = controller.ShowEditForm(poster) as PaymentBatchForm)
			{
				Application.DoEvents();

				AssertEquals("Pre-condition", false, form.PaymentBatchGrid_ForTestOnly.ReadOnly);

				poster = form.BusinessEntity as APPaymentBatchPoster;
				paymentApprovalCollection = poster.PaymentApprovalCollection.Cast<PaymentApprovalBase>();
				payment1 = paymentApprovalCollection.Single(x => x.CurrencyCode == TestObjectCreator.USD.Code);
				payment2 = paymentApprovalCollection.Single(x => x.CurrencyCode == TestObjectCreator.CNY.Code);
				payment3 = paymentApprovalCollection.Single(x => x.CurrencyCode == TestObjectCreator.TWD.Code);

				AssertEquals("Percondition", true, payment1.IsFullyApproved);
				AssertEquals("Percondition", true, payment2.IsFullyApproved);
				AssertEquals("Percondition", true, payment3.IsFullyApproved);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.PaymentBatchGrid_ForTestOnly.SelectSingleElement(payment1);
				form.PaymentBatchGrid_ForTestOnly.ContextMenu.ShowPopupMenu();
				form.PaymentBatchGrid_ForTestOnly.ContextMenu.MenuItems.FindByText("Actio&ns").MenuItems.FindByText("Post Approvals").MenuItems.FindByText("Post").PerformClick();
				AssertEquals($@"The following Payments will be processed:
{payment1.GetDescription()}

Do you want to post these transactions?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(true, payment1.IsPosted);
				AssertEquals(true, payment2.IsFullyApproved);
				AssertEquals(true, payment3.IsFullyApproved);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.PaymentBatchGrid_ForTestOnly.SelectAllElements();
				form.PaymentBatchGrid_ForTestOnly.UnSelect(poster.PaymentApprovalCollection.IndexOf(x => x.PK == payment3.PK));
				form.PaymentBatchGrid_ForTestOnly.ContextMenu.MenuItems.FindByText("Actio&ns").MenuItems.FindByText("Post Approvals").MenuItems.FindByText("Post").PerformClick();
				AssertEquals($@"The following Payments cannot be processed:
{payment1.GetDescription()}
This Payment is already posted

The following Payments will be processed:
{payment2.GetDescription()}

Do you want to post these transactions?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(true, payment1.IsPosted);
				AssertEquals(true, payment2.IsPosted);
				AssertEquals(true, payment3.IsFullyApproved);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.PaymentBatchGrid_ForTestOnly.UnSelectAll();
				form.ActionsMenuItem_ForTestOnly.MenuItems.FindByText("Post Approvals").MenuItems.FindByText("Post").PerformClick();
				AssertContains($@"The following Payments cannot be processed:", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertContains($"{payment1.GetDescription()}\r\nThis Payment is already posted", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertContains($"{payment2.GetDescription()}\r\nThis Payment is already posted", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertContains($@"The following Payments will be processed:
{payment3.GetDescription()}

Do you want to post these transactions?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(true, payment1.IsPosted);
				AssertEquals(true, payment2.IsPosted);
				AssertEquals(true, payment3.IsPosted);

				AssertEquals("Some Payment Batch controls should be readonly once posted.", true, form.PaymentBatchGrid_ForTestOnly.ReadOnly);
			}
		}

		public void TestEditPaymentBatch_PopulateChequeNoForPaymentApprovals()
		{
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, "BUY", 1m);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.CNY, "BUY", 2m);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.TWD, "BUY", 3m);

			SetupDataForBaseTest();
			var poster = BatchPostingHelper.CreateBatchPosterWithMultipleTransactionsWithForeignCurrency();
			Factory.Save();
			poster.PostPaymentsAsPaymentApprovals = true;
			var paymentApprovalCollection = poster.PaymentApprovalCollection.Cast<PaymentApprovalBase>();
			var payment1 = paymentApprovalCollection.Single(x => x.CurrencyCode == TestObjectCreator.USD.Code);
			var payment2 = paymentApprovalCollection.Single(x => x.CurrencyCode == TestObjectCreator.CNY.Code);
			var payment3 = paymentApprovalCollection.Single(x => x.CurrencyCode == TestObjectCreator.TWD.Code);
			payment1.AV_ChequeOrReference = payment2.AV_ChequeOrReference = payment3.AV_ChequeOrReference = ZString.Empty;
			payment1.AV_Status = payment2.AV_Status = payment3.AV_Status = PaymentApprovalStatus.FullyApproved;
			Factory.Save();

			var controller = ZControllerFactory.Create(ControllerIDs.PaymentBatch);
			using (var form = controller.ShowEditForm(poster) as PaymentBatchForm)
			{
				Application.DoEvents();

				poster = form.BusinessEntity as APPaymentBatchPoster;
				paymentApprovalCollection = poster.PaymentApprovalCollection.Cast<PaymentApprovalBase>();
				payment1 = paymentApprovalCollection.Single(x => x.CurrencyCode == TestObjectCreator.USD.Code);
				payment2 = paymentApprovalCollection.Single(x => x.CurrencyCode == TestObjectCreator.CNY.Code);
				payment3 = paymentApprovalCollection.Single(x => x.CurrencyCode == TestObjectCreator.TWD.Code);

				AssertEquals("Percondition", true, payment1.AV_ChequeOrReference.IsEmpty);
				AssertEquals("Percondition", true, payment2.AV_ChequeOrReference.IsEmpty);
				AssertEquals("Percondition", true, payment3.AV_ChequeOrReference.IsEmpty);

				var exceptedMessage = $@"The following Payments will be processed:
{payment1.GetDescription()}

Do you want to populate the cheque number for these transactions?";
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.PaymentBatchGrid_ForTestOnly.SelectSingleElement(payment1);
				form.PaymentBatchGrid_ForTestOnly.ContextMenu.ShowPopupMenu();
				form.PaymentBatchGrid_ForTestOnly.ContextMenu.MenuItems.FindByText("Actio&ns").MenuItems.FindByText("Post Approvals").MenuItems.FindByText("Allocate Check No.").PerformClick();
				AssertEquals(exceptedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(false, payment1.AV_ChequeOrReference.IsEmpty);
				AssertEquals(true, payment2.AV_ChequeOrReference.IsEmpty);
				AssertEquals(true, payment3.AV_ChequeOrReference.IsEmpty);

				exceptedMessage = payment2.GetDescription();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.PaymentBatchGrid_ForTestOnly.SelectAllElements();
				form.PaymentBatchGrid_ForTestOnly.UnSelect(poster.PaymentApprovalCollection.IndexOf(x => x.PK == payment3.PK));
				form.PaymentBatchGrid_ForTestOnly.ContextMenu.MenuItems.FindByText("Actio&ns").MenuItems.FindByText("Post Approvals").MenuItems.FindByText("Allocate Check No.").PerformClick();
				AssertContains("The following Payments will be processed:", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertContains(payment1.GetDescription(), UnitTestUserNotification.Instance.LastMessage.Text);
				AssertContains(exceptedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNotContains("The following Payments cannot be processed:", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(false, payment1.AV_ChequeOrReference.IsEmpty);
				AssertEquals(false, payment2.AV_ChequeOrReference.IsEmpty);
				AssertEquals(true, payment3.AV_ChequeOrReference.IsEmpty);

				exceptedMessage = payment3.GetDescription();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.PaymentBatchGrid_ForTestOnly.UnSelectAll();
				form.ActionsMenuItem_ForTestOnly.MenuItems.FindByText("Post Approvals").MenuItems.FindByText("Allocate Check No.").PerformClick();
				AssertContains("The following Payments will be processed:", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertContains(payment1.GetDescription(), UnitTestUserNotification.Instance.LastMessage.Text);
				AssertContains(payment2.GetDescription(), UnitTestUserNotification.Instance.LastMessage.Text);
				AssertContains(exceptedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNotContains("The following Payments cannot be processed:", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(false, payment1.AV_ChequeOrReference.IsEmpty);
				AssertEquals(false, payment2.AV_ChequeOrReference.IsEmpty);
				AssertEquals(false, payment3.AV_ChequeOrReference.IsEmpty);
			}
		}

		public void TestEditPaymentBatch_PopulateChequeNoAndPostPaymentApprovals()
		{
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, "BUY", 1m);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.CNY, "BUY", 2m);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.TWD, "BUY", 3m);

			SetupDataForBaseTest();
			var poster = BatchPostingHelper.CreateBatchPosterWithMultipleTransactionsWithForeignCurrency();
			Factory.Save();
			poster.PostPaymentsAsPaymentApprovals = true;
			var paymentApprovalCollection = poster.PaymentApprovalCollection.Cast<PaymentApprovalBase>();
			var payment1 = paymentApprovalCollection.Single(x => x.CurrencyCode == TestObjectCreator.USD.Code);
			var payment2 = paymentApprovalCollection.Single(x => x.CurrencyCode == TestObjectCreator.CNY.Code);
			var payment3 = paymentApprovalCollection.Single(x => x.CurrencyCode == TestObjectCreator.TWD.Code);
			payment1.AV_ChequeOrReference = payment2.AV_ChequeOrReference = payment3.AV_ChequeOrReference = ZString.Empty;
			payment1.AV_Status = payment2.AV_Status = payment3.AV_Status = PaymentApprovalStatus.FullyApproved;
			Factory.Save();

			var controller = ZControllerFactory.Create(ControllerIDs.PaymentBatch);
			using (var form = controller.ShowEditForm(poster) as PaymentBatchForm)
			{
				Application.DoEvents();

				AssertEquals("Pre-condition", false, form.PaymentBatchGrid_ForTestOnly.ReadOnly);

				poster = form.BusinessEntity as APPaymentBatchPoster;
				paymentApprovalCollection = poster.PaymentApprovalCollection.Cast<PaymentApprovalBase>();
				payment1 = paymentApprovalCollection.Single(x => x.CurrencyCode == TestObjectCreator.USD.Code);
				payment2 = paymentApprovalCollection.Single(x => x.CurrencyCode == TestObjectCreator.CNY.Code);
				payment3 = paymentApprovalCollection.Single(x => x.CurrencyCode == TestObjectCreator.TWD.Code);

				AssertEquals("Percondition", true, payment1.AV_ChequeOrReference.IsEmpty);
				AssertEquals("Percondition", true, payment2.AV_ChequeOrReference.IsEmpty);
				AssertEquals("Percondition", true, payment3.AV_ChequeOrReference.IsEmpty);
				AssertEquals("Percondition", true, payment1.IsFullyApproved);
				AssertEquals("Percondition", true, payment2.IsFullyApproved);
				AssertEquals("Percondition", true, payment3.IsFullyApproved);

				var exceptedMessage = $@"The following Payments will be processed:
{payment1.GetDescription()}

Do you want to populate the cheque number and post these transactions?";
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.PaymentBatchGrid_ForTestOnly.SelectSingleElement(payment1);
				form.PaymentBatchGrid_ForTestOnly.ContextMenu.ShowPopupMenu();
				form.PaymentBatchGrid_ForTestOnly.ContextMenu.MenuItems.FindByText("Actio&ns").MenuItems.FindByText("Post Approvals").MenuItems.FindByText("Allocate Check No. and Post").PerformClick();
				AssertEquals(exceptedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(false, payment1.AV_ChequeOrReference.IsEmpty);
				AssertEquals(true, payment2.AV_ChequeOrReference.IsEmpty);
				AssertEquals(true, payment3.AV_ChequeOrReference.IsEmpty);
				AssertEquals(true, payment1.IsPosted);
				AssertEquals(true, payment2.IsFullyApproved);
				AssertEquals(true, payment3.IsFullyApproved);

				exceptedMessage = payment2.GetDescription();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.PaymentBatchGrid_ForTestOnly.SelectAllElements();
				form.PaymentBatchGrid_ForTestOnly.UnSelect(poster.PaymentApprovalCollection.IndexOf(x => x.PK == payment3.PK));
				form.PaymentBatchGrid_ForTestOnly.ContextMenu.MenuItems.FindByText("Actio&ns").MenuItems.FindByText("Post Approvals").MenuItems.FindByText("Allocate Check No. and Post").PerformClick();
				AssertEquals($@"The following Payments cannot be processed:
{payment1.GetDescription()}
This Payment is already posted

The following Payments will be processed:
{exceptedMessage}

Do you want to populate the cheque number and post these transactions?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(false, payment1.AV_ChequeOrReference.IsEmpty);
				AssertEquals(false, payment2.AV_ChequeOrReference.IsEmpty);
				AssertEquals(true, payment3.AV_ChequeOrReference.IsEmpty);
				AssertEquals(true, payment1.IsPosted);
				AssertEquals(true, payment2.IsPosted);
				AssertEquals(true, payment3.IsFullyApproved);

				exceptedMessage = payment3.GetDescription();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.PaymentBatchGrid_ForTestOnly.UnSelectAll();
				form.ActionsMenuItem_ForTestOnly.MenuItems.FindByText("Post Approvals").MenuItems.FindByText("Allocate Check No. and Post").PerformClick();
				AssertContains("The following Payments cannot be processed:", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertContains($"{payment1.GetDescription()}\r\nThis Payment is already posted", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertContains($"{payment2.GetDescription()}\r\nThis Payment is already posted", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertContains($@"The following Payments will be processed:
{exceptedMessage}

Do you want to populate the cheque number and post these transactions?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(false, payment1.AV_ChequeOrReference.IsEmpty);
				AssertEquals(false, payment2.AV_ChequeOrReference.IsEmpty);
				AssertEquals(false, payment3.AV_ChequeOrReference.IsEmpty);
				AssertEquals(true, payment1.IsPosted);
				AssertEquals(true, payment2.IsPosted);
				AssertEquals(true, payment3.IsPosted);

				AssertEquals("Some Payment Batch controls should be readonly once posted.", true, form.PaymentBatchGrid_ForTestOnly.ReadOnly);
			}
		}

		public void TestEditPaymentBatch_CancelPaymentApprovals()
		{
			SetupDataForBaseTest();
			var poster = BatchPostingHelper.CreateBatchPosterWithMultipleTransactionsWithForeignCurrency();
			Factory.Save();

			var controller = ZControllerFactory.Create(ControllerIDs.PaymentBatch);
			using (var form = controller.ShowEditForm(poster) as PaymentBatchForm)
			{
				Application.DoEvents();

				poster = form.BusinessEntity as APPaymentBatchPoster;
				var paymentApprovalCollection = poster.PaymentApprovalCollection.Cast<PaymentApprovalBase>();
				var payment1 = paymentApprovalCollection.Single(x => x.CurrencyCode == TestObjectCreator.USD.Code);
				var payment2 = paymentApprovalCollection.Single(x => x.CurrencyCode == TestObjectCreator.CNY.Code);
				var payment3 = paymentApprovalCollection.Single(x => x.CurrencyCode == TestObjectCreator.TWD.Code);

				AssertEquals("Percondition", true, payment1.IsDraft);
				AssertEquals("Percondition", true, payment2.IsDraft);
				AssertEquals("Percondition", true, payment3.IsDraft);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.PaymentBatchGrid_ForTestOnly.SelectSingleElement(payment1);
				form.PaymentBatchGrid_ForTestOnly.ContextMenu.ShowPopupMenu();
				form.PaymentBatchGrid_ForTestOnly.ContextMenu.MenuItems.FindByText("Actio&ns").MenuItems.FindByText("Cancel").PerformClick();

				AssertEquals($@"The following Payments will be canceled:
{payment1.GetDescription()}

Do you want to cancel these Payments?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(true, payment1.IsCancelled);
				AssertEquals(true, payment2.IsDraft);
				AssertEquals(true, payment3.IsDraft);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.PaymentBatchGrid_ForTestOnly.SelectAllElements();
				form.PaymentBatchGrid_ForTestOnly.UnSelect(poster.PaymentApprovalCollection.IndexOf(x => x.PK == payment3.PK));
				form.PaymentBatchGrid_ForTestOnly.ContextMenu.MenuItems.FindByText("Actio&ns").MenuItems.FindByText("Cancel").PerformClick();
				AssertEquals($@"The following Payments cannot be canceled:
{payment1.GetDescription()}
This Payment is already Canceled

The following Payments will be canceled:
{payment2.GetDescription()}

Do you want to cancel these Payments?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(true, payment1.IsCancelled);
				AssertEquals(true, payment2.IsCancelled);
				AssertEquals(true, payment3.IsDraft);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.PaymentBatchGrid_ForTestOnly.UnSelectAll();
				form.ActionsMenuItem_ForTestOnly.MenuItems.FindByText("Cancel").PerformClick();
				AssertContains("The following Payments cannot be canceled:", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertContains($"{payment1.GetDescription()}\r\nThis Payment is already Canceled", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertContains($"{payment2.GetDescription()}\r\nThis Payment is already Canceled", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertContains($@"The following Payments will be canceled:
{payment3.GetDescription()}

Do you want to cancel these Payments?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(true, payment1.IsCancelled);
				AssertEquals(true, payment2.IsCancelled);
				AssertEquals(true, payment3.IsCancelled);
			}
		}

		[ExpectNoExceptions]
		public void TestEditPaymentBatch_WhenNoPayment()
		{
			var batch = Factory.NewWithValidTestData<AccPaymentBatch>();
			Factory.Save();

			var controller = ZControllerFactory.Create(ControllerIDs.PaymentBatch);
			using (var form = controller.ShowEditForm(batch) as PaymentBatchForm)
			{
				form.MatchTransactionsGrid_ForTestOnly.ContextMenu.ShowPopupMenu();
				form.PaymentBatchGrid_ForTestOnly.ContextMenu.ShowPopupMenu();

				var overpaymentButton = form.Controls.Find("OverpaymentButton", true)[0];
				(overpaymentButton as ZButton).PerformClick();

				var discountButton = form.Controls.Find("DiscountButton", true)[0];
				(discountButton as ZButton).PerformClick();

				var exchangeDiffButton = form.Controls.Find("ExchangeDiffButton", true)[0];
				(exchangeDiffButton as ZButton).PerformClick();

				var applyEXXButton = form.Controls.Find("ApplyEXXButton", true)[0];
				(applyEXXButton as ZButton).PerformClick();

				var saveAsDraftButton = form.Controls.Find("SaveAsDraftButton", true)[0] as ZButton;
				saveAsDraftButton.PerformClick();
			}
		}

		#endregion

		public void TestApplyEXXButton()
		{
			SetupDataForPostingTest();
			using (var form = new PaymentBatchForm_ForTestOnly(BatchPoster))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.Show();
				form.ApplyEXXButton_ForTestOnly.PerformClick();
				AssertEquals("User notification", "One or more payments were skipped because they were using a local currency", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		int GetApplyEXXButtonWidth(string buttonCaption)
		{
			using (var form = new PaymentBatchForm_ForTestOnly(BatchPoster))
			{
				form.ApplyEXXButton_ForTestOnly.Text = buttonCaption;
				form.Show();
				return form.ApplyEXXButton_ForTestOnly.Width;
			}
		}

		public void TestUpdatingCurrencySummaryRowExRateAskUserToConfirm()
		{
			SetupDataForPostingTest();
			using (var form = new PaymentBatchForm_ForTestOnly(BatchPoster))
			{
				var creator = new TestObjectCreator(Factory);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				var summaryRow = ((APPaymentBatchPoster)form.BusinessEntity).CurrencySummary.SummaryRows.First() as CurrencySummaryRow;
				summaryRow.Currency = creator.EUR.RX_Code;
				summaryRow.ExchangeRate = 1.5m;
				AssertEquals("Update all EUR payment(s) with exchange rate 1.5?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(1.5m, summaryRow.ExchangeRate);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				summaryRow.ExchangeRate = 1.6m;
				AssertEquals("Update all EUR payment(s) with exchange rate 1.6?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(1.6m, summaryRow.ExchangeRate);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				summaryRow.ExchangeRate = 1.6m;
				AssertEquals("Update all EUR payment(s) with exchange rate 1.6?", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestUpdatingCurrencySummaryRowExRateUpdatePaymentsExRateWithTheSameCurrency()
		{
			var creator = new TestObjectCreator(Factory);
			SetupDataForPostingTest();
			using (var form = new PaymentBatchForm_ForTestOnly(BatchPoster))
			{
				var batchPoster = form.PaymentBatchPoster_ForTestOnly;
				batchPoster.APB_PaymentType = ReceiptTypes.Cash;
				batchPoster.APB_AB = TestBank.PK;

				var usdPayment = batchPoster.PaymentApprovalCollection[0];
				usdPayment.AV_RX_NKPaymentCurrency = creator.USD.RX_Code;

				AssertEquals(3, batchPoster.PaymentApprovalCollection.Count);
				AssertEquals(creator.USD.RX_Code, usdPayment.AV_RX_NKPaymentCurrency);
				AssertEquals(creator.AUD.RX_Code, batchPoster.PaymentApprovalCollection[1].AV_RX_NKPaymentCurrency);
				AssertEquals(creator.AUD.RX_Code, batchPoster.PaymentApprovalCollection[2].AV_RX_NKPaymentCurrency);
				AssertEquals(2, batchPoster.CurrencySummary.SummaryRows.Count);

				var usdCurrencySummaryRow = BatchPoster.CurrencySummary.SummaryRows.GetSummaryRow(creator.USD.RX_Code);
				AssertEquals(creator.USD.RX_Code, usdCurrencySummaryRow.Currency);
				AssertEquals(0.75m, usdCurrencySummaryRow.ExchangeRate);
				AssertEquals(false, usdCurrencySummaryRow.ExchangeRateInfo.ReadOnly);

				var audCurrencySummaryRow = BatchPoster.CurrencySummary.SummaryRows.GetSummaryRow(creator.AUD.RX_Code);
				AssertEquals(creator.AUD.RX_Code, audCurrencySummaryRow.Currency);
				AssertEquals(1m, audCurrencySummaryRow.ExchangeRate);
				AssertEquals(true, audCurrencySummaryRow.ExchangeRateInfo.ReadOnly);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				usdCurrencySummaryRow.ExchangeRate = 2;
				AssertEquals("Update all USD payment(s) with exchange rate 2?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNotEquals("PayExRate should not be updated on the PaymentApproval as the answer was NO", 2m, usdPayment.AV_PayExRate);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				usdCurrencySummaryRow.ExchangeRate = 2;
				AssertEquals("Update all USD payment(s) with exchange rate 2?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("PayExRate should be updated on the PaymentApproval as the answer was YES", 2m, usdPayment.AV_PayExRate);
			}
		}

		public void TestApplyEXXButtonWidth()
		{
			SetupDataForPostingTest();

			var width = GetApplyEXXButtonWidth("");
			Assert("The minimum width of the button should be 150px",
				width >= CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150));

			width = GetApplyEXXButtonWidth("That is some very wide text to be used as a caption, even wider than the widest localized text");
			Assert("The width of the button should be at least 452px to accomodate the widest localized caption, which appears to be the Russian translation",
				width >= CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(452));
		}

		public void TestOrganisationFormPopUp()
		{
			SetupDataForPostingTest();
			using (var form = new PaymentBatchForm_ForTestOnly(BatchPoster))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				form.DisplayMode = ODisplayMode.ReadOnly;
				form.Show();

				using (ZOrganisationsForm testOrgForm = form.ShowOrganisationFormToEdit_ForTestOnly())
				{
					AssertNotNull(testOrgForm);
				}
			}
		}

		public void TestAllocateChequeNumber_ChequeBookIsAutoPrint_PostPayments()
		{
			AssertAllocateChequeNumberCore(true, new Action<PaymentBatchForm_ForTestOnly>((form) =>
			{
				form.SaveAndCloseButton_ForTestOnly.PerformClick();

				var printFormToTest = ZFormModaliser.LastFormShownDialogForTest as PaymentDocumentsPrintPopup.MockPaymentDocumentsPrintPopup;
				BatchPostingHelper.AssertBatchPosterWillMatchAndPost(BatchPoster, 3, 3, 2, ZBool.False);

				AsssertChequeOrReferenceIsNullOrEmpty(false);
				AssertNotNull("Should have opened Remittance Advice Printing Form", printFormToTest);
				AssertEquals(true, printFormToTest.ChequeIsAutoPrintLabel_Exposed.Visible);
			}));
		}

		public void TestAllocateChequeNumber_ChequeBookIsNotAutoPrint_PostPayments()
		{
			AssertAllocateChequeNumberCore(false, new Action<PaymentBatchForm_ForTestOnly>((form) =>
			{
				form.SaveAndCloseButton_ForTestOnly.PerformClick();

				var printFormToTest = ZFormModaliser.LastFormShownDialogForTest as PaymentDocumentsPrintPopup.MockPaymentDocumentsPrintPopup;
				BatchPostingHelper.AssertBatchPosterWillMatchAndPost(BatchPoster, 3, 3, 2, ZBool.False);

				AsssertChequeOrReferenceIsNullOrEmpty(false);
				AssertNotNull("Should have opened Remittance Advice Printing Form", printFormToTest);
				AssertEquals(false, printFormToTest.ChequeIsAutoPrintLabel_Exposed.Visible);
			}));
		}

		public void TestAllocateChequeNumber_ChequeBookIsAutoPrint_PostPaymentsAsPaymentApprovals()
		{
			AssertAllocateChequeNumberCore(true, new Action<PaymentBatchForm_ForTestOnly>((form) =>
			{
				(form.BusinessEntity as APPaymentBatchPoster).PostPaymentsAsPaymentApprovals = true;
				form.SaveAndCloseButton_ForTestOnly.PerformClick();

				var printFormToTest = ZFormModaliser.LastFormShownDialogForTest as PaymentDocumentsPrintPopup.MockPaymentDocumentsPrintPopup;
				BatchPostingHelper.AssertBatchPosterWillMatchAndPost(BatchPoster, 3, 0, 2, ZBool.False);

				AsssertChequeOrReferenceIsNullOrEmpty(true);
				AssertNotNull("Should have opened Remittance Advice Printing Form", printFormToTest);
				AssertEquals(false, printFormToTest.ChequeIsAutoPrintLabel_Exposed.Visible);
			}));
		}

		public void TestAllocateChequeNumber_ChequeBookIsNotAutoPrint_PostPaymentsAsPaymentApprovals()
		{
			AssertAllocateChequeNumberCore(false, new Action<PaymentBatchForm_ForTestOnly>((form) =>
			{
				(form.BusinessEntity as APPaymentBatchPoster).PostPaymentsAsPaymentApprovals = true;
				form.SaveAndCloseButton_ForTestOnly.PerformClick();

				var printFormToTest = ZFormModaliser.LastFormShownDialogForTest as PaymentDocumentsPrintPopup.MockPaymentDocumentsPrintPopup;
				BatchPostingHelper.AssertBatchPosterWillMatchAndPost(BatchPoster, 3, 0, 2, ZBool.False);

				AsssertChequeOrReferenceIsNullOrEmpty(false);
				AssertNotNull("Should have opened Remittance Advice Printing Form", printFormToTest);
				AssertEquals(false, printFormToTest.ChequeIsAutoPrintLabel_Exposed.Visible);
			}));
		}

		public void TestAllocateChequeNumber_ChequeBookIsAutoPrint_SaveAsDraft()
		{
			AssertAllocateChequeNumberCore(true, new Action<PaymentBatchForm_ForTestOnly>((form) =>
			{
				var saveAsDraftButton = form.Controls.Find("SaveAsDraftButton", true)[0] as ZButton;
				saveAsDraftButton.PerformClick();

				var printFormToTest = ZFormModaliser.LastFormShownDialogForTest as PaymentDocumentsPrintPopup.MockPaymentDocumentsPrintPopup;
				BatchPostingHelper.AssertBatchPosterWillMatchAndPost(BatchPoster, 3, 0, 2, ZBool.False);

				AsssertChequeOrReferenceIsNullOrEmpty(true);
				AssertNull("Should not have opened Remittance Advice Printing Form", printFormToTest);
			}));
		}

		public void TestAllocateChequeNumber_ChequeBookIsNotAutoPrint_SaveAsDraft()
		{
			AssertAllocateChequeNumberCore(false, new Action<PaymentBatchForm_ForTestOnly>((form) =>
			{
				var saveAsDraftButton = form.Controls.Find("SaveAsDraftButton", true)[0] as ZButton;
				saveAsDraftButton.PerformClick();

				var printFormToTest = ZFormModaliser.LastFormShownDialogForTest as PaymentDocumentsPrintPopup.MockPaymentDocumentsPrintPopup;
				BatchPostingHelper.AssertBatchPosterWillMatchAndPost(BatchPoster, 3, 0, 2, ZBool.False);

				AsssertChequeOrReferenceIsNullOrEmpty(false);
				AssertNull("Should not have opened Remittance Advice Printing Form", printFormToTest);
			}));
		}

		void AssertAllocateChequeNumberCore(bool isChequeBookAutoPrint, Action<PaymentBatchForm_ForTestOnly> action)
		{
			ZFormModaliser.ShowDialogsInTest = true;

			if (isChequeBookAutoPrint)
			{
				SetUpDataForAutoAllocationTest();
				AssertEquals("Cheque Book should be AutoPrint", true, TestCheques.IsAutoPrint);
			}
			else
			{
				SetupDataForPostingTest();
				AssertEquals("Cheque Book should NOT be AutoPrint", false, TestCheques.IsAutoPrint);
			}

			using (var form = new PaymentBatchForm_ForTestOnly(BatchPoster))
			{
				form.Show();
				AssertEquals("There should be 3 PaymentApproval objects created", 3, BatchPoster.PaymentApprovalCollection.Count);

				BatchPoster.APB_PaymentType = ReceiptTypes.Cheque;
				BatchPoster.APB_AB = TestBank.PK;
				BatchPoster.APB_AK = TestCheques.PK;
				AsssertChequeOrReferenceIsNullOrEmpty(isChequeBookAutoPrint);

				action(form);
			}
		}

		void AsssertChequeOrReferenceIsNullOrEmpty(bool isNullOrEmpty)
		{
			if (isNullOrEmpty)
			{
				AssertNullOrEmpty("There cheque number should NOT be set", BatchPoster.PaymentApprovalCollection[0].AV_ChequeOrReference);
				AssertNullOrEmpty("There cheque number should NOT be set", BatchPoster.PaymentApprovalCollection[1].AV_ChequeOrReference);
				AssertNullOrEmpty("There cheque number should NOT be set", BatchPoster.PaymentApprovalCollection[2].AV_ChequeOrReference);
			}
			else
			{
				AssertNotNullOrEmpty("There cheque number should be set", BatchPoster.PaymentApprovalCollection[0].AV_ChequeOrReference);
				AssertNotNullOrEmpty("There cheque number should be set", BatchPoster.PaymentApprovalCollection[1].AV_ChequeOrReference);
				AssertNotNullOrEmpty("There cheque number should be set", BatchPoster.PaymentApprovalCollection[2].AV_ChequeOrReference);
			}
		}

		public void TestPostMatchSuccessful()
		{
			SetupDataForPostingTest();

			using (var form = new PaymentBatchForm_ForTestOnly(BatchPoster))
			{
				PaymentDocumentsPrintPopup exposedPrintForm = null;
				try
				{
					form.Show();

					AssertEquals("There should be 3 PaymentApproval objects created", 3, BatchPoster.PaymentApprovalCollection.Count);
					AssertEquals("Should be 1 transaction in MatchedTransactions", 1, BatchPoster.PaymentApprovalCollection[0].MatchingBaseObject.MatchedTransactions.Count);
					AssertEquals("Should be 1 transaction in MatchedTransactions", 1, BatchPoster.PaymentApprovalCollection[1].MatchingBaseObject.MatchedTransactions.Count);
					AssertEquals("Should be 1 transaction in MatchedTransactions", 1, BatchPoster.PaymentApprovalCollection[2].MatchingBaseObject.MatchedTransactions.Count);

					BatchPoster.APB_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
					BatchPoster.APB_AB = TestBank.PK;
					BatchPoster.APB_AK = TestCheques.PK;
					AssertEquals("There cheque number should be set correctly", "000001", BatchPoster.PaymentApprovalCollection[0].AV_ChequeOrReference);
					AssertEquals("There cheque number should be set correctly", "000002", BatchPoster.PaymentApprovalCollection[1].AV_ChequeOrReference);
					AssertEquals("There cheque number should be set correctly", "000003", BatchPoster.PaymentApprovalCollection[2].AV_ChequeOrReference);
					AssertEquals("Payment amount should be defaulted", BatchPoster.PaymentApprovalCollection[0].AV_Amount, BatchPoster.PaymentApprovalCollection[0].AV_Calc_LocalAmount - BatchPoster.PaymentApprovalCollection[0].MatchingBaseObject.Balance);
					AssertEquals("Transaction Payment amount should be set", ((IMatching)TestAPInv).OSPartialPaymentAmount, ((IMatching)TestAPInv).OutstandingAmount);

					form.SaveAndCloseButton_ForTestOnly.PerformClick(); // user clicks save button
					BatchPostingHelper.AssertBatchPosterWillMatchAndPost(BatchPoster, 3, 3, 2, ZBool.False);

					Assert("The posting form should have been closed as the posting is finished", !form.Visible);

					// check that remittance form pops up
					exposedPrintForm = ((PaymentBatchPrintManager.TestPaymentPrintManager)form.PrintManager_ForTestOnly).RemittancePrintForm_Exposed;
					AssertNotNull("The remittance advice print form should not be null because it should have been shown", exposedPrintForm);
				}
				finally
				{
					if (exposedPrintForm != null)
					{
						exposedPrintForm.Dispose();
					}
				}
			}
		}

		public void TestPrintManagerIsOfValidType()
		{
			SetupDataForBaseTest();
			using (PaymentBatchForm testForm = new PaymentBatchForm(BatchPoster))
			{
				AssertEquals(typeof(PaymentBatchPrintManager), testForm.PrintManager_ForTestOnly.GetType());
			}
		}

		public StmPrintQueue PrepareForTestMessageIfChequeBookUsesSamePrinter()
		{
			SetupDataForPostingTest();
			BatchPoster.APB_AB = TestBank.PK;
			var newFactory = new BusinessObjectFactory();

			var printer = newFactory.New<StmPrintQueue>();
			printer.SQ_QueueName = "Queue Name";
			newFactory.Save();

			var book2 = newFactory.New<AccChequeBook>();
			book2.AK_AB = TestCheques.AK_AB;
			book2.AK_GB = TestCheques.AK_GB;
			book2.AK_Code = "Book2";
			book2.AK_AutoPrintCheque = ZBool.True;
			book2.AK_SQ = printer.PK;
			newFactory.Save();

			var testChequeInNewFactory = newFactory.Load<AccChequeBook>(TestCheques.PK);
			testChequeInNewFactory.AK_SQ = printer.PK;
			testChequeInNewFactory.AK_Code = "My cheq";
			testChequeInNewFactory.AK_AutoPrintCheque = true;
			testChequeInNewFactory.BankAccount.AB_SO_ChequeTemplate = TestObjectCreator.StandardTemplatePK;
			newFactory.Save();

			TestCheques.Reload();

			BatchPoster.APB_AK = TestCheques.PK;
			BatchPoster.APB_ChequeOrReference = "1";
			BatchPoster.APB_AB = TestCheques.AK_AB;
			BatchPoster.PaymentApprovalCollection[0].AV_ChequeOrReference = "2";
			BatchPoster.PaymentApprovalCollection[1].AV_ChequeOrReference = "3";
			BatchPoster.PaymentApprovalCollection[2].AV_ChequeOrReference = "4";

			return printer;
		}

		public void TestMessageIfChequeBookUsesSamePrinter_ChequeBookIsAutoPrint_PostPayments()
		{
			var printer = PrepareForTestMessageIfChequeBookUsesSamePrinter();

			using (var form = new PaymentBatchForm_ForTestOnly(BatchPoster))
			{
				form.FireSaveButton();
				var exposedPrintForm = ((PaymentBatchPrintManager.TestPaymentPrintManager)form.PrintManager_ForTestOnly).RemittancePrintForm_Exposed;
				AssertNotNull("The remittance advice print form should not be null because it should have been shown", exposedPrintForm);
				AssertEquals("Last message should be that chequebook uses same printer", AccChequeBook.WarningPaymentWithChequeBookWithSamePrinterMessage(printer.SQ_QueueName, TestCheques.AK_CurrentNo), UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestMessageIfChequeBookUsesSamePrinter_ChequeBookIsAutoPrint_PaymentBatchInDB()
		{
			var printer = PrepareForTestMessageIfChequeBookUsesSamePrinter();
			Factory.Save();
			using (var form = new PaymentBatchForm_ForTestOnly(BatchPoster))
			{
				form.FireSaveButton();
				var exposedPrintForm = ((PaymentBatchPrintManager.TestPaymentPrintManager)form.PrintManager_ForTestOnly).RemittancePrintForm_Exposed;
				AssertNull("The remittance advice print form should be null because it should not have been shown", exposedPrintForm);
				AssertNullOrEmpty("Last message should be null", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestMessageIfChequeBookUsesSamePrinter_ChequeBookIsAutoPrint_PostPaymentsAsPaymentApprovals()
		{
			PrepareForTestMessageIfChequeBookUsesSamePrinter();
			using (var form = new PaymentBatchForm_ForTestOnly(BatchPoster))
			{
				(form.BusinessEntity as APPaymentBatchPoster).PostPaymentsAsPaymentApprovals = true;
				form.SaveAndCloseButton_ForTestOnly.PerformClick();

				AssertNullOrEmpty("Last message should be null", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestMessageIfChequeBookUsesSamePrinter_ChequeBookIsAutoPrint_SaveAsDraft()
		{
			PrepareForTestMessageIfChequeBookUsesSamePrinter();
			using (var form = new PaymentBatchForm_ForTestOnly(BatchPoster))
			{
				var saveAsDraftButton = form.Controls.Find("SaveAsDraftButton", true)[0] as ZButton;
				saveAsDraftButton.PerformClick();

				AssertNullOrEmpty("Last message should be null", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestOnSelectBankAccountFromDefault()
		{
			var anotherBank = Factory.NewWithValidTestData<AccBankAccount>();
			SetupDataForBaseTest(false);

			TestOrg.CompanyData.OB_AB_APDefaultBankAccount = TestBank.PK;
			BatchPostingHelper.TestOrg2 = BatchPostingHelper.CreateCreditorTestOrg(new Guid("ff715a34-30c0-4646-9c32-1de8ff4925d0"));
			BatchPostingHelper.TestOrg2.CompanyData.OB_AB_APDefaultBankAccount = anotherBank.PK;

			BatchPostingHelper.SetupDataForPostingTest();
			BatchPoster = BatchPostingHelper.BatchPoster;
			BatchPoster.APB_AB = ZGuid.Empty;

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

			using (var testForm = new PaymentBatchForm_ForTestOnly(BatchPoster))
			{
				testForm.Show();
				testForm.OnShown_ForTestOnly(null);
				AssertEquals("Bank selection form should have been shown.", typeof(PaymentBatchBankSelectionForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			}
		}

		public void TestSelectionOfAnObjectWillRefreshDetails()
		{
			SetupDataForPostingTest();
			using (var testForm = new PaymentBatchForm_ForTestOnly(BatchPoster))
			{
				testForm.Show();
				testForm.OnShown_ForTestOnly(null);
				testForm.PaymentBatchGrid_ForTestOnly.CurrentRowIndex = 0;
				testForm.SelectedPayment_ForTestOnly.AV_PayExRate = 2m;
				BusinessObject previousPayment = testForm.SelectedPayment_ForTestOnly;
				AssertEquals("Button name should be set correctly", "New", testForm.ExchangeDiffButton_ForTestOnly.Text);
				TransactionViewForm miscForm = (TransactionViewForm)testForm.ShowNewMiscTransactionForm_ForTestOnly(ZArchitecture.Core.TransactionTypes.ExchangeDifference);
				miscForm.FireSaveButton();
				miscForm.Dispose();
				AssertEquals("Button name should be changed", "Edit", testForm.ExchangeDiffButton_ForTestOnly.Text);
				testForm.PaymentDetailsRefreshed_ForTestOnly = ZBool.False;
				testForm.PaymentBatchGrid_ForTestOnly.CurrentRowIndex = 1;
				Assert(testForm.PaymentDetailsRefreshed_ForTestOnly);
				AssertNotEquals("SelectedPayment_ForTestOnly should be changed", previousPayment, testForm.SelectedPayment_ForTestOnly);
				AssertEquals("Button name should be set correctly", "New", testForm.ExchangeDiffButton_ForTestOnly.Text);
			}
		}

		public void TestCurrentCellChanged_NoEPaymentDeals_NoDBHitsToAccEPaymentDealTable()
		{
			SetupDataForPostingTest();
			using (var testForm = new PaymentBatchForm_ForTestOnly(BatchPoster))
			{
				testForm.Show();
				Factory.ResetDatabaseLoadCount();
				var expectedDBHits = new Dictionary<string, int>()
				{
					{ AccEPaymentDealSchema.Constants.TableName, 0 }
				};
				using (AssertDbHitsWithUsefulQueryInformation(expectedDBHits, Factory))
				{
					testForm.PaymentBatchGrid_CurrentCellChanged_ForTestOnly(null, null);
				}
			}
		}

		public void TestRefreshButton_NoEPaymentDeals_DBHitsToAccEPaymentDealTable()
		{
			SetupDataForPostingTest();
			using (var testForm = new PaymentBatchForm_ForTestOnly(BatchPoster))
			{
				testForm.Show();
				var bankEPaymentTabControl = testForm.Controls.Find("TabControlBankAndEPayment", true).First() as ZTabControl;
				bankEPaymentTabControl.SelectedIndex = 1;

				Factory.ResetDatabaseLoadCount();
				var expectedDBHits = new Dictionary<string, int>()
				{
					{ AccEPaymentDealSchema.Constants.TableName, 3 },
					{ AccEPaymentQuoteSchema.Constants.TableName, 1 },
					// The below values are for baseline and are not the focus of this test
					{ AccAPAccountDetailsSchema.Constants.TableName, 6 }
				};
				using (AssertDbHitsWithUsefulQueryInformation(expectedDBHits, Factory))
				{
					var refreshButton = testForm.Controls.Find("RefreshButton", true).First() as ZButton;

					refreshButton.PerformClick();
				}
			}
		}

		public void TestInitializePaymentCollectionForPrinting()
		{
			SetupDataForPostingTest();

			using (var testForm = new PaymentBatchForm_ForTestOnly(BatchPoster))
			{
				testForm.Show();
				BatchPoster.APB_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
				BatchPoster.APB_AB = TestBank.PK;
				BatchPoster.APB_AK = TestCheques.PK;
				testForm.SaveAndCloseButton_ForTestOnly.PerformClick(); // user clicks save button
				BatchPostingHelper.AssertBatchPosterWillMatchAndPost(BatchPoster, 3, 3, 2, ZBool.False);

				AssertEquals("PaymentCollection_ForTestOnly for printing should contain 3 payments", 3, testForm.PaymentCollectionForPrinting_ForTestOnly.Count());
				AssertEquals("Should have opened Remittance Advice Printing Form", typeof(PaymentDocumentsPrintPopup.MockPaymentDocumentsPrintPopup), ZFormModaliser.LastFormShownDialogForTest.GetType());
				((PaymentDocumentsPrintPopup)ZFormModaliser.LastFormShownDialogForTest).Close();
			}
		}

		public void TestRemovingAllPayments_DisplayModeEditPost()
		{
			SetupDataForBaseTest();
			BatchPoster.APB_PaymentType = ReceiptTypes.Cash;
			Factory.Save();

			using (var testForm = new PaymentBatchForm_ForTestOnly(BatchPoster))
			{
				testForm.Show();
				Assert("Form should be visible", testForm.Visible);
				AssertEquals("There should be only 1 payment for posting", 1, BatchPoster.PaymentApprovalCollection.Count);
				testForm.PaymentBatchGrid_ForTestOnly.Select(0);
				testForm.DeletePaymentTransaction_ForTestOnly(null, null);
				AssertEquals("PaymentCollection should be empty", 0, BatchPoster.PaymentApprovalCollection.Count);
				Assert("Form should be visible", testForm.Visible);

				testForm.FPostButton_ForTestOnly.PerformClick();
				Assert("Form should be invisible", !testForm.Visible);
				AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestRemovingAllPayments_DisplayModeEditApply()
		{
			SetupDataForBaseTest();
			Factory.Save();

			using (var testForm = new PaymentBatchForm_ForTestOnly(BatchPoster))
			{
				testForm.Show();
				Assert("Form should be visible", testForm.Visible);
				AssertEquals("There should be only 1 payment for posting", 1, BatchPoster.PaymentApprovalCollection.Count);
				testForm.PaymentBatchGrid_ForTestOnly.Select(0);
				testForm.DeletePaymentTransaction_ForTestOnly(null, null);
				AssertEquals("PaymentCollection should be empty", 0, BatchPoster.PaymentApprovalCollection.Count);
				Assert("Form should be visible", testForm.Visible);

				testForm.FApplyButton_ForTestOnly.PerformClick();
				Assert("Form should be invisible", !testForm.Visible);
				AssertEquals("A message should be shown on closing", "There are no payments left for posting. The form will be closed.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestRemovingAllPayments_DisplayModeNew()
		{
			SetupDataForBaseTest();

			using (var testForm = new PaymentBatchForm_ForTestOnly(BatchPoster))
			{
				testForm.Show();
				Assert("Form should be visible", testForm.Visible);
				AssertEquals("There should be only 1 payment for posting", 1, BatchPoster.PaymentApprovalCollection.Count);
				testForm.PaymentBatchGrid_ForTestOnly.Select(0);
				testForm.DeletePaymentTransaction_ForTestOnly(null, null);
				AssertEquals("PaymentCollection should be empty", 0, BatchPoster.PaymentApprovalCollection.Count);
				Assert("Form should be invisible", !testForm.Visible);
				AssertEquals("A message should be shown on closing", "There are no payments left for posting. The form will be closed.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestDeletingLastTransaction_DisplayModeNew()
		{
			SetupDataForBaseTest();

			using (var testForm = new PaymentBatchForm_ForTestOnly(BatchPoster))
			{
				testForm.Show();
				Assert("Form should be visible", testForm.Visible);
				AssertEquals("There should be only 1 payment for posting", 1, BatchPoster.PaymentApprovalCollection.Count);
				testForm.PaymentBatchGrid_ForTestOnly.Select(0);
				testForm.MatchTransactionsGrid_ForTestOnly.Select(0);
				testForm.DeleteTransaction_ForTestOnly(null, null);
				AssertEquals("PaymentCollection should be empty", 0, BatchPoster.PaymentApprovalCollection.Count);
				Assert("Form should be invisible", !testForm.Visible);
				AssertEquals("A message should be shown on closing", "There are no payments left for posting. The form will be closed.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestDeletingLastTransaction_DisplayModeEditApply()
		{
			SetupDataForBaseTest();
			Factory.Save();

			using (var testForm = new PaymentBatchForm_ForTestOnly(BatchPoster))
			{
				testForm.Show();
				Assert("Form should be visible", testForm.Visible);
				AssertEquals("There should be only 1 payment for posting", 1, BatchPoster.PaymentApprovalCollection.Count);
				testForm.PaymentBatchGrid_ForTestOnly.Select(0);
				testForm.MatchTransactionsGrid_ForTestOnly.Select(0);
				testForm.DeleteTransaction_ForTestOnly(null, null);
				AssertEquals("PaymentCollection should be empty", 0, BatchPoster.PaymentApprovalCollection.Count);
				Assert("Form should be visible", testForm.Visible);

				testForm.FApplyButton_ForTestOnly.PerformClick();
				Assert("Form should be invisible", !testForm.Visible);
				AssertEquals("A message should be shown on closing", "There are no payments left for posting. The form will be closed.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestDeletingLastTransaction_DisplayModeEditPost()
		{
			SetupDataForBaseTest();
			BatchPoster.APB_PaymentType = ReceiptTypes.Cash;
			Factory.Save();

			using (var testForm = new PaymentBatchForm_ForTestOnly(BatchPoster))
			{
				testForm.Show();
				Assert("Form should be visible", testForm.Visible);
				AssertEquals("There should be only 1 payment for posting", 1, BatchPoster.PaymentApprovalCollection.Count);
				testForm.PaymentBatchGrid_ForTestOnly.Select(0);
				testForm.MatchTransactionsGrid_ForTestOnly.Select(0);
				testForm.DeleteTransaction_ForTestOnly(null, null);
				AssertEquals("PaymentCollection should be empty", 0, BatchPoster.PaymentApprovalCollection.Count);
				Assert("Form should be visible", testForm.Visible);

				testForm.FPostButton_ForTestOnly.PerformClick();
				Assert("Form should be invisible", !testForm.Visible);
				AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestEditMiscellaneousTransaction()
		{
			SetupDataForBaseTest();

			using (var testForm = new PaymentBatchForm_ForTestOnly(BatchPoster))
			{
				testForm.Show();
				testForm.PaymentBatchGrid_ForTestOnly.Select(0);
				testForm.SelectedPayment_ForTestOnly.AV_PayExRate = 4m;
				TransactionViewForm miscForm = (TransactionViewForm)testForm.ShowNewMiscTransactionForm_ForTestOnly(ZArchitecture.Core.TransactionTypes.ExchangeDifference);

				miscForm.FireSaveButton();
				miscForm.Dispose();

				ZController miscTransController = AccountingControllerCreator.GetNewController(testForm.SelectedMatchingBase_ForTestOnly.ExchangeDiffCurrent);
				if (miscTransController != null)
				{
					miscTransController.ShowEditForm(testForm.SelectedMatchingBase_ForTestOnly.ExchangeDiffCurrent);
				}

				AssertEquals("Should be 2 transactions in MatchedTransactions", 2, testForm.SelectedMatchingBase_ForTestOnly.MatchedTransactions.Count);
				miscTransController.LastShownForm.Dispose();
			}
		}

		public void TestMiscTransactionFormIsModal()
		{
			SetupDataForBaseTest();

			using (var testForm = new PaymentBatchForm_ForTestOnly(BatchPoster))
			{
				TransactionViewForm oVPForm = null;
				try
				{
					testForm.Show();
					testForm.PaymentBatchGrid_ForTestOnly.Select(0);
					testForm.ShowNewMiscTransactionForm_ForTestOnly(ZArchitecture.Core.TransactionTypes.Overpayment);

					oVPForm = (TransactionViewForm)ZFormModaliser.ActiveForm;
					AssertEquals("New OVP Form should be shown modally", testForm, ZFormModaliser.GetParentFormForModalForm(oVPForm));
					AssertEquals("New OVP Form should be shown modally", oVPForm, ZFormModaliser.GetActiveChildFormForParentForm(testForm));
				}
				finally
				{
					if (oVPForm != null)
					{
						oVPForm.Dispose();
					}
				}
			}
		}

		#region TestAutoPrintCheque

		[ExpectNoExceptions]
		public void TestAutoPrintCheques()
		{
			SetUpDataForAutoAllocationTest();
			using (var form = new PaymentBatchForm_ForTestOnly(BatchPoster))
			{
				PaymentDocumentsPrintPopup exposedPrintForm = null;
				try
				{
					form.Show();

					AssertEquals("There should be 3 PaymentApproval objects created", 3, BatchPoster.PaymentApprovalCollection.Count);
					// Sort Payment Approvals by Org Code
					BatchPoster.PaymentApprovalCollection.Sort<APPaymentApprovalWithoutAuthorisation>((x, y) => x.Header.OH_Code.CompareTo(y.Header.OH_Code));

					Assert("Sorted", BatchPoster.PaymentApprovalCollection[0].Header.OH_Code.StartsWith("A_"));
					Assert("Sorted", BatchPoster.PaymentApprovalCollection[1].Header.OH_Code.StartsWith("B_"));
					Assert("Sorted", BatchPoster.PaymentApprovalCollection[2].Header.OH_Code.StartsWith("C_"));

					AssertEquals("Should be 1 transaction in MatchedTransactions", 1, BatchPoster.PaymentApprovalCollection[0].MatchingBaseObject.MatchedTransactions.Count);
					AssertEquals("Should be 1 transaction in MatchedTransactions", 1, BatchPoster.PaymentApprovalCollection[1].MatchingBaseObject.MatchedTransactions.Count);
					AssertEquals("Should be 1 transaction in MatchedTransactions", 1, BatchPoster.PaymentApprovalCollection[2].MatchingBaseObject.MatchedTransactions.Count);

					BatchPoster.APB_PaymentType = ReceiptTypes.Cheque;
					BatchPoster.APB_AB = TestBank.PK;
					BatchPoster.APB_AK = TestCheques.PK;
					AssertEquals("Payment amount should be defaulted", BatchPoster.PaymentApprovalCollection[0].AV_Amount, BatchPoster.PaymentApprovalCollection[0].AV_Calc_LocalAmount - BatchPoster.PaymentApprovalCollection[0].MatchingBaseObject.Balance);
					AssertEquals("Transaction Payment amount should be set", ((IMatching)TestAPInv).OSPartialPaymentAmount, ((IMatching)TestAPInv).OutstandingAmount);
					Assert("BatchPoster should be in autoallocation mode", BatchPoster.IsChequeNumberAutoAllocated);
					AssertEquals("ChequeNumberIsAutoAllocatedLabel", AccountingConstants.ChequeLabelConstants.ChequeNumberIsAutoAllocatedLabel, BatchPoster.Calc_ChequeNumberIsAutoAllocatedLabel);
					AssertEquals("ChequeAutoPrintedLabel", AccountingConstants.ChequeLabelConstants.ChequeAutoPrintedLabel, BatchPoster.Calc_ChequeIsAutoPrintedLabel);
					Assert("Cheque number should be empty", BatchPoster.APB_ChequeOrReference.IsEmpty);
					Assert("Cheque number should be read only", BatchPoster.APB_ChequeOrReferenceInfo.ReadOnly);
					Assert("There cheque number should be empty on a payment approval", BatchPoster.PaymentApprovalCollection[0].AV_ChequeOrReference.IsEmpty);
					Assert("There cheque number should be empty on a payment approval", BatchPoster.PaymentApprovalCollection[1].AV_ChequeOrReference.IsEmpty);
					Assert("There cheque number should be empty on a payment approval", BatchPoster.PaymentApprovalCollection[2].AV_ChequeOrReference.IsEmpty);

					form.SaveAndCloseButton_ForTestOnly.PerformClick(); // user clicks save button
					BatchPostingHelper.AssertBatchPosterWillMatchAndPost(BatchPoster, 3, 3, 2, ZBool.False);

					Assert("The posting form should have been closed as the posting is finished", !form.Visible);

					Assert("Auto printing should be performed", form.Test_Allocator.ChequeWasAutoPrinted);
					AssertEquals("Printing called only once", 1, form.Test_Allocator.PrintingCalled_Counter);

					// check that remittance form pops up
					exposedPrintForm = (PaymentDocumentsPrintPopup)ZFormModaliser.LastFormShownDialogForTest;
					AssertNotNull("The remittance advice print form should not be null because it should have been shown", exposedPrintForm);
					Assert("AutoPrintCheque should have fired", form.PrintManager_ForTestOnly.ChequeIsAutoPrinted);

					var paymentPrint = ((PaymentBatchPrintManager.MockPaymentPrint)((PaymentBatchPrintManager.TestPaymentPrintManager)form.PrintManager_ForTestOnly).PaymentPrinter_ForTestOnly);
					AssertEquals("There should be only one printer passed for printing", 1, form.Test_Allocator.PrintersPassedForAutoPrinting.Count);
					AssertEquals("PaymentPrinter should have correct Printer passed to", TestCheques.AK_SQ, form.Test_Allocator.PrintersPassedForAutoPrinting[0]);
					AssertEquals("Should have opened Remittance Advice Printing Form", typeof(PaymentDocumentsPrintPopup.MockPaymentDocumentsPrintPopup), ZFormModaliser.LastFormShownDialogForTest.GetType());
					((PaymentDocumentsPrintPopup)ZFormModaliser.LastFormShownDialogForTest).Close();

					var paymentCollectionPassedForAutoPrinting = paymentPrint.PaymentCollectionPassedForAutoPrinting.ToArray();
					AssertEquals("There should be 3 payments passed for printing", 3, paymentCollectionPassedForAutoPrinting.Length);
					Assert("Payment1 should have the flag of auto printing set", ((IChequeNumberAutoAllocation)paymentCollectionPassedForAutoPrinting[0]).ChequeIsAutoPrinted);
					Assert("Payment2 should have the flag of auto printing set", ((IChequeNumberAutoAllocation)paymentCollectionPassedForAutoPrinting[1]).ChequeIsAutoPrinted);
					Assert("Payment3 should have the flag of auto printing set", ((IChequeNumberAutoAllocation)paymentCollectionPassedForAutoPrinting[2]).ChequeIsAutoPrinted);
					var paymentApproval = GetPaymentApprovalByChequeNumber(BatchPoster.PaymentApprovalCollection, "000001");
					AssertNotNull("Payment Approval should exist", paymentApproval);
					Assert("New Payment should be created", !paymentApproval.AV_AH.IsEmpty);
					TransactionHeader newPayment = Factory.Load<TransactionHeader>(paymentApproval.AV_AH);
					AssertEquals("Payment ChequeOrReference", paymentApproval.AV_ChequeOrReference, newPayment.AH_ChequeOrReference);
					Assert("Collection that was passed for autoprinting should contain the payment", form.Test_Allocator.CollectionsPassedForPrinting[0].Contains(newPayment));
					Assert("Cheque# allocated in sort order", newPayment.Header.OH_Code.StartsWith("A_"));

					paymentApproval = GetPaymentApprovalByChequeNumber(BatchPoster.PaymentApprovalCollection, "000002");
					AssertNotNull("Payment Approval should exist", paymentApproval);
					Assert("New Payment should be created", !paymentApproval.AV_AH.IsEmpty);
					newPayment = Factory.Load<TransactionHeader>(paymentApproval.AV_AH);
					AssertEquals("Payment ChequeOrReference", paymentApproval.AV_ChequeOrReference, newPayment.AH_ChequeOrReference);
					Assert("Collection that was passed for autoprinting should contain the payment", form.Test_Allocator.CollectionsPassedForPrinting[0].Contains(newPayment));
					Assert("Cheque# allocated in sort order", newPayment.Header.OH_Code.StartsWith("B_"));

					paymentApproval = GetPaymentApprovalByChequeNumber(BatchPoster.PaymentApprovalCollection, "000003");
					AssertNotNull("Payment Approval should exist", paymentApproval);
					Assert("New Payment should be created", !paymentApproval.AV_AH.IsEmpty);
					newPayment = Factory.Load<TransactionHeader>(paymentApproval.AV_AH);
					AssertEquals("Payment ChequeOrReference", paymentApproval.AV_ChequeOrReference, newPayment.AH_ChequeOrReference);
					Assert("Collection that was passed for autoprinting should contain the payment", form.Test_Allocator.CollectionsPassedForPrinting[0].Contains(newPayment));
					Assert("Cheque# allocated in sort order", newPayment.Header.OH_Code.StartsWith("C_"));

					TestCheques.Reload();
					AssertEquals("CurrentNo should change on the cheque book", 4m, TestCheques.AK_CurrentNo);
					Assert("Cheque book is sitll active", TestCheques.AK_IsActive);
				}
				finally
				{
					if (exposedPrintForm != null)
					{
						exposedPrintForm.Dispose();
					}
				}
			}
		}

		public void TestCheckExRateButtonWithCancelledAndPostedPaymentApprovals()
		{
			SetUpDataForAutoAllocationTest();
			BatchPoster.APB_PaymentType = ReceiptTypes.Cheque;
			BatchPoster.APB_AB = TestBank.PK;
			BatchPoster.APB_AK = TestCheques.PK;
			Factory.Save();

			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			using (var form = new PaymentBatchForm_ForTestOnly(BatchPoster))
			{
				form.Show();

				var checkExRateButton = form.Controls.Find("CheckExRateButton", true)[0] as ZButton;
				AssertEquals(true, checkExRateButton.Visible);
				AssertEquals(true, checkExRateButton.Enabled);

				BatchPoster.PaymentApprovalCollection[0].AV_Status = PaymentApprovalStatus.Cancelled;
				BatchPoster.PaymentApprovalCollection[1].AV_Status = PaymentApprovalStatus.Posted;
				Factory.Save();
				AssertEquals("Precondition", 3, BatchPoster.PaymentApprovalCollection.Count);
				AssertEquals("Precondition", 1, BatchPoster.PaymentApprovalCollectionWithoutCancelledOrPosted.Count());

				AssertEquals(true, checkExRateButton.Visible);
				AssertEquals(true, checkExRateButton.Enabled);
				checkExRateButton.PerformClick();

				var payments = BatchPoster.PaymentApprovalCollection.Cast<PaymentApprovalBase>();
				var quotesOnPayments = BatchPoster.FilteredEPaymentQuotes.Cast<EPaymentQuote>();
				AssertEquals(1, quotesOnPayments.Count());
				Assert(quotesOnPayments.All(x => x.IsInDatabase));
				AssertEquals(BatchPoster.PaymentApprovalCollection.OfType<PaymentApprovalBase>().Single(x => !x.IsCancelledOrIsPosted).PK, quotesOnPayments.FirstOrDefault().QU_AV);
				AssertEquals(
$@"E-Quote requests generated to service provider OFX. Response may take from a few moments up to several minutes to be received.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestWillNotAllocationAndPrintCheques_ForPaymentBatchInDB()
		{
			SetUpDataForAutoAllocationTest("TestAutoAllocationAndPrintCheques");
			BatchPoster.APB_PaymentType = ReceiptTypes.Cheque;
			BatchPoster.APB_AB = TestBank.PK;
			BatchPoster.APB_AK = TestCheques.PK;
			Factory.Save();

			using (var form = new PaymentBatchForm(BatchPoster))
			{
				form.FireSaveButton();
				AssertNull(form.Test_Allocator);
			}
		}

		[ExpectNoExceptions]
		public void TestAutoAllocationAndPrintCheques_ForPaymentBatchNotInDB_ShowErrorInTransaction()
		{
			SetUpDataForAutoAllocationTest("TestAutoAllocationAndPrintCheques");
			BatchPoster.APB_PaymentType = ReceiptTypes.Cheque;
			BatchPoster.APB_AB = TestBank.PK;
			BatchPoster.APB_AK = TestCheques.PK;
			using (var form = new PaymentBatchForm(BatchPoster))
			{
				form.Show();
				AssertEquals("Error shown to user", true, PaymentDocumentsPrinter.PerformTestAutoAllocationAndPrintChequesFailure(() => form.SaveAndCloseButton_ForTestOnly.PerformClick()));
			}
		}

		#endregion

		public void TestHandleSaveException()
		{
			SetupDataForPostingTest();
			BatchPoster.APB_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			BatchPoster.APB_AB = TestBank.PK;
			BatchPoster.APB_AK = TestCheques.PK;
			BatchPoster.APB_ChequeOrReference = "1";
			Factory.Save();

			BatchPostingHelper.PaymentApproval1.UnRegisterEditableChildObject(BatchPostingHelper.PaymentApproval1.MatchingBaseObject);
			BatchPostingHelper.PaymentApproval1.MatchingBaseObject.MatchedTransactions.AddTransactionThatMustBeMatched(BatchPostingHelper.PaymentApproval1);
			BatchPostingHelper.PaymentApproval1.MatchingBaseObject.MatchAndClearTransactions();
			Factory.Save();

			AssertEquals(true, BatchPostingHelper.PaymentApproval1.IsPosted);
			AssertNotEquals(true, BatchPostingHelper.PaymentApproval2.IsPosted);
			AssertNotEquals(true, BatchPostingHelper.PaymentApproval3.IsPosted);

			var poster = NewFactory().Load<APPaymentBatchPoster>(BatchPoster.PK);
			poster.LoadPayments();
			using (var testForm = new PaymentBatchForm_ForTestOnly(poster))
			{
				testForm.HandleSaveException_ForTestOnly(new ENettProcessCreditCardException("Test", "Test", "Test"));

				AssertContains(@"Failed to process credit card payments via ComPay.
ComPay Error: (Test) Test

Payment failed for the following creditors:", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertContains(BatchPostingHelper.PaymentApproval1.Header.OH_Code, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertContains(BatchPostingHelper.PaymentApproval2.Header.OH_Code, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertContains(BatchPostingHelper.PaymentApproval3.Header.OH_Code, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		void enableOrganisationForComPay(ZGuid organisationPK)
		{
			OrgHeader orgHeader = Factory.Load<OrgHeader>(organisationPK);
			OrgCusCode cusCode = orgHeader.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.eNettRegistrationNumber;
			cusCode.OK_CustomsRegNo = "123456";
			cusCode.OK_RN_NKCodeCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			AssertEquals("eNettRegistrationNumber", "123456", orgHeader.ENettRegistrationNumber);
		}

		public void TestPostingWithCreditCardPaymentViaENett()
		{
			TestObjectCreator objectCreator = new TestObjectCreator(Factory);
			SetupDataForPostingTest();
			enableOrganisationForComPay(BatchPoster.PaymentApprovalCollection[0].AV_OH);
			enableOrganisationForComPay(BatchPoster.PaymentApprovalCollection[1].AV_OH);
			enableOrganisationForComPay(BatchPoster.PaymentApprovalCollection[2].AV_OH);

			using (var testForm = new PaymentBatchForm_ForTestOnly(BatchPoster))
			{
				PaymentDocumentsPrintPopup exposedPrintForm = null;
				bool originalComPayEnabled = AccountingConfigurationRegistry.Instance.EnableCreditCardPaymentsViaComPay.Value;
				try
				{
					AccountingConfigurationRegistry.Instance.EnableCreditCardPaymentsViaComPay.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
					int initialInvokedCount = MockENettWebService.Instance.CountProcessCreditCardWasInvoked;
					testForm.Show();
					Assert("Form should be visible", testForm.Visible);

					AssertEquals("There should be 3 PaymentApproval objects created", 3, BatchPoster.PaymentApprovalCollection.Count);
					AssertEquals("Should be 1 transaction in MatchedTransactions", 1, BatchPoster.PaymentApprovalCollection[0].MatchingBaseObject.MatchedTransactions.Count);
					AssertEquals("Should be 1 transaction in MatchedTransactions", 1, BatchPoster.PaymentApprovalCollection[1].MatchingBaseObject.MatchedTransactions.Count);
					AssertEquals("Should be 1 transaction in MatchedTransactions", 1, BatchPoster.PaymentApprovalCollection[2].MatchingBaseObject.MatchedTransactions.Count);

					BatchPoster.APB_PaymentType = ZArchitecture.Core.ReceiptTypes.eNettCreditCard;

					var header = Factory.NewWithValidTestData<AccGLHeader>();
					header.AG_AccountNum = "ZZAUDAcc";
					var bankAccount = objectCreator.CreateBankAccount("ZZHSBCAUD", "HSBC AUD ACCT", "HSBC", "AUD", objectCreator.AUD, "123456", "12345678", header);
					bankAccount.AB_DebitCreditCardExpiry = "0699";
					bankAccount.AB_DebitCreditCardName = "MR JOHN SMITH";
					var encoder = new TwoWayEncoder(bankAccount.PK.ToGuid());
					bankAccount.AB_DebitCreditCardNumber = encoder.Encrypt("1234567812345678");
					bankAccount.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.CCD;
					bankAccount.AB_AccountNum = "**** **** ***4 5678";
					BatchPoster.APB_AB = bankAccount.PK;

					BatchPoster.APB_ChequeOrReference = "111";
					BatchPoster.CardSecurityCode = "123";

					AssertEquals("PaymentType of BatchPoster", ZArchitecture.Core.ReceiptTypes.eNettCreditCard, BatchPoster.APB_PaymentType);
					AssertEquals("PaymentBatch[0].AV_PaymentType", ZArchitecture.Core.ReceiptTypes.eNettCreditCard, BatchPoster.PaymentApprovalCollection[0].AV_PaymentType);
					AssertEquals("PaymentBatch[1].AV_PaymentType", ZArchitecture.Core.ReceiptTypes.eNettCreditCard, BatchPoster.PaymentApprovalCollection[1].AV_PaymentType);
					AssertEquals("PaymentBatch[2].AV_PaymentType", ZArchitecture.Core.ReceiptTypes.eNettCreditCard, BatchPoster.PaymentApprovalCollection[2].AV_PaymentType);
					AssertEquals("AB_AccountType", AccountTypeCodeDescriptionPairList.Codes.CCD, bankAccount.AB_AccountType);

					AssertEquals("Payment amount should be defaulted", BatchPoster.PaymentApprovalCollection[0].AV_Amount, BatchPoster.PaymentApprovalCollection[0].AV_Calc_LocalAmount - BatchPoster.PaymentApprovalCollection[0].MatchingBaseObject.Balance);
					AssertEquals("Transaction Payment amount should be set", ((IMatching)TestAPInv).OSPartialPaymentAmount, ((IMatching)TestAPInv).OutstandingAmount);

					MockENettWebService.Instance.SetupForTesting("CARGOWISE");
					eNettWebServiceWrapper.UseRealWebService_ForTesting = false;

					testForm.SaveAndCloseButton_ForTestOnly.PerformClick(); // user clicks save button
					BatchPostingHelper.AssertBatchPosterWillMatchAndPost(BatchPoster, 3, 3, 2, ZBool.False);

					Assert("The posting form should have been closed as the posting is finished", !testForm.Visible);
					AssertEquals("ProcessCreditCard should have been invoked three times", 3, MockENettWebService.Instance.CountProcessCreditCardWasInvoked - initialInvokedCount);

					// check that remittance form pops up
					exposedPrintForm = ((PaymentBatchPrintManager.TestPaymentPrintManager)testForm.PrintManager_ForTestOnly).RemittancePrintForm_Exposed;
					AssertNotNull("The remittance advice print form should not be null because it should have been shown", exposedPrintForm);
				}
				finally
				{
					AccountingConfigurationRegistry.Instance.EnableCreditCardPaymentsViaComPay.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, originalComPayEnabled);
					if (exposedPrintForm != null)
					{
						exposedPrintForm.Dispose();
					}
				}
			}
		}

		public void TestAddressesAndContact()
		{
			AccountingConfigurationRegistry.Instance.EditPaymentAddress.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			using (var form = (PaymentBatchForm)GetFormToBashCore())
			{
				form.Show();
				var payment = (form.BusinessEntity as APPaymentBatchPoster).PaymentApprovalCollection[0];
				AssertEquals(false, payment.AV_OA_AddressOverrideInfo.ReadOnly);
				AssertEquals(false, payment.AV_OC_ContactOverrideInfo.ReadOnly);

				AccountingConfigurationRegistry.Instance.EditPaymentAddress.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

				form.Refresh();
				AssertEquals(true, payment.AV_OA_AddressOverrideInfo.ReadOnly);
				AssertEquals(true, payment.AV_OC_ContactOverrideInfo.ReadOnly);
			}
		}

		public void TestCurrencyCodeIsTextBoxColumn()
		{
			using (var form = (PaymentBatchForm)GetFormToBashCore())
			{
				form.Show();
				Application.DoEvents();

				var paymentCurrencyCodeColumn = form.QuoteSummaryGrid_ForTestOnly.GetColumnStyle("PaymentCurrencyCode");
				AssertType<ZTextBoxColumnStyleInfo>(paymentCurrencyCodeColumn);

				var fundingCurrencyCodeColumn = form.QuoteSummaryGrid_ForTestOnly.GetColumnStyle("FundingCurrencyCode");
				AssertType<ZTextBoxColumnStyleInfo>(fundingCurrencyCodeColumn);
			}
		}

		void PrepareEPaymentAccountDetailsCollection(OrgHeader creditor, string paymentMethod = EPaymentMethods.EPaymentViaOFX, string currency = CurrencyCodes.UnitedStates)
		{
			var beneficary = TestObjectCreator.CreateEPaymentBeneficiary("ref" + creditor.OH_Code);
			var accountDetails = creditor.CompanyData.AccountDetailsCollection;
			accountDetails.RemoveAndDeleteAll();
			var ofxAccountDetail = accountDetails.AddNew();
			ofxAccountDetail.A1_PaymentMethod = paymentMethod;
			ofxAccountDetail.A1_RX_NKAccountCurrency = currency;
			ofxAccountDetail.A1_IsDefaultAccount = true;
			ofxAccountDetail.A1_EPaymentBeneficiaryId = beneficary.PK;
		}

		#region Implementation

		OrgHeader TestOrg;
		APInvoice TestAPInv;
		APPaymentBatchPoster BatchPoster;

		AccBankAccount TestBank;
		AccChequeBook TestCheques;

		protected override Form GetFormToBashCore()
		{
			SetupDataForPostingTest();
			BatchPoster.APB_PaymentType = ZArchitecture.Core.ReceiptTypes.Cash;
			BatchPoster.SetHasChangesToFalse_ForTestOnly();

			PaymentBatchForm form = new PaymentBatchForm(BatchPoster);
			return form;
		}

		public override Type FormToBashType
		{
			get { return typeof(PaymentBatchForm); }
		}

		void SetupNewPaymentBatch()
		{
			new AccountingPeriodTestHelper().SetupPeriods();

			TestObjectCreator.CreateExchangeRate(TestObjectCreator.CNY, "BUY", 2.5m);

			var batches = Factory.Load<AccPaymentBatch>(new ZQuery());
			var approvals = Factory.Load<AccPaymentApproval>(new ZQuery());
			AssertEquals(0, batches.Length);
			AssertEquals(0, approvals.Length);

			var bank = Factory.NewWithValidTestData<AccBankAccount>();
			bank.AB_ChequeNumDigits = 6;
			var cheque = Factory.NewWithValidTestData<AccChequeBook>();
			cheque.AK_StartNo = 1;
			cheque.AK_LastNo = 100;
			cheque.AK_CurrentNo = 1;
			cheque.AK_AB = bank.PK;

			var invoice = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV", TestObjectCreator.CNY, 1m, 1000m, 0m, 1000m, 0m);
			invoice.AH_OH = TestObjectCreator.AALSHI.PK;
			Factory.Save();

			invoice.Lines[0].Validation.ValidateAll();
			TransactionHeaderCollection transactions = new TransactionHeaderCollection(Factory);
			transactions.Add(invoice);

			BatchPoster = Factory.New<APPaymentBatchPoster>();
			BatchPoster.SetDefaultValuesByTransactions(transactions);
			BatchPoster.APB_AB = bank.PK;
			BatchPoster.SetPaymentDetails(BatchPoster.PaymentApprovalCollection[0]);
			BatchPoster.APB_AB = bank.PK;
			BatchPoster.APB_AK = cheque.PK;
		}

		void SetupBalancedPaymentBatch(bool balanceEqualsToZero = true, PaymentBatchCreatingStatus status = PaymentBatchCreatingStatus.SaveAsDraft)
		{
			SetupNewPaymentBatch();

			var payment = BatchPoster.PaymentApprovalCollection[0];
			var paymentMatchingBaseObject = payment.MatchingBaseObject;
			var exchangeDifference = paymentMatchingBaseObject.GetMiscellaneousTransaction(TransactionTypes.ExchangeDifference);
			var discount = paymentMatchingBaseObject.GetMiscellaneousTransaction(TransactionTypes.Discount);
			exchangeDifference.BindableOSAmount = 400m;
			discount.BindableOSAmount = balanceEqualsToZero ? 200m : 100m;
			paymentMatchingBaseObject.AddMiscellaneousTransaction(exchangeDifference);
			paymentMatchingBaseObject.AddMiscellaneousTransaction(discount);

			if (status != PaymentBatchCreatingStatus.New)
			{
				using (Factory.SetTempContext(BusinessContext.SavingPaymentApprovalAsDraft))
				{
					payment.AV_Status = PaymentApprovalStatus.Draft;
					BatchPoster.MatchTransactions();
					Factory.Save();
				}

				if (status == PaymentBatchCreatingStatus.SaveAsFullyApproved)
				{
					var newFactory = Factory.CreateNewFactory();
					newFactory.RefreshEnabled = false;
					var reloadedPayment = newFactory.Load<AccPaymentApproval>(payment.PK);
					reloadedPayment.AV_Status = PaymentApprovalStatus.FullyApproved;
					newFactory.Save();
				}
			}

			var batches = Factory.Load<AccPaymentBatch>(new ZQuery());
			var approvals = Factory.Load<AccPaymentApproval>(new ZQuery());
			AssertEquals(1, batches.Length);
			AssertEquals(1, approvals.Length);
		}

		void SetupDataForBaseTest(bool shouldCreateSingleTransactionBatch = true)
		{
			BatchPostingHelper.PrepareForBaseTest(shouldCreateSingleTransactionBatch);
			TestOrg = BatchPostingHelper.TestOrg;
			TestBank = BatchPostingHelper.TestBank;
			TestCheques = BatchPostingHelper.TestCheques;
			TestCheques.AK_GB = GlbBranch.CurrentBranch.PK;
			TestAPInv = BatchPostingHelper.TestAPInv;
			BatchPoster = BatchPostingHelper.BatchPoster;
		}

		void SetupDataForPostingTest()
		{
			SetupDataForBaseTest(false);
			BatchPostingHelper.SetupDataForPostingTest();
			BatchPoster = BatchPostingHelper.BatchPoster;
		}

		void SetUpDataForAutoAllocationTest(string chequeBookDesc = null)
		{
			SetupDataForPostingTest();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			BusinessObject printQueue = newFactory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.DocumentEngine.IStmPrintQueue)));
			AccBankAccount bank = newFactory.Load<AccBankAccount>(TestBank.PK);
			bank.AB_SO_ChequeTemplate = TestObjectCreator.StandardTemplatePK;
			newFactory.Save();
			TestBank.Reload();
			AccChequeBook chequeBook = newFactory.Load<AccChequeBook>(TestCheques.PK);
			chequeBook.AK_AutoPrintCheque = ZBool.True;
			chequeBook.AK_SQ = printQueue.PK;
			if (!string.IsNullOrEmpty(chequeBookDesc))
			{
				chequeBook.AK_Desc = chequeBookDesc;
			}

			Assert("Cheque Book should be AutoPrint", chequeBook.IsAutoPrint);
			newFactory.Save();
			TestCheques.Reload();
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;

		PaymentBatchPostingTestHelper BatchPostingHelper
		{
			get
			{
				if (fBatchPostingHelper == null)
				{
					fBatchPostingHelper = new PaymentBatchPostingTestHelper(Factory);
				}
				return fBatchPostingHelper;
			}
		}
		PaymentBatchPostingTestHelper fBatchPostingHelper;

		PaymentApprovalBase GetPaymentApprovalByChequeNumber(PaymentApprovalBaseCollection collection, ZString chequeNumber)
		{
			foreach (PaymentApprovalBase payment in collection)
			{
				if (payment.AV_ChequeOrReference == chequeNumber)
				{
					return payment;
				}
			}
			return null;
		}

		ZQuery GetDealQueryForBatchPoster()
		{
			var query = new ZDBOnlyQuery(typeof(AccEPaymentDeal));
			var subQuery = new ZDBOnlySubQuery(typeof(AccEPaymentQuote), AccEPaymentDealSchema.AED_QU_Quote);
			subQuery.AddToFilter(AccEPaymentQuoteSchema.QU_AV, BatchPoster.PaymentApprovalCollection.Select(x => x.PK));
			query.AddSubQuery(subQuery, JoinCondition.And);

			return query;
		}

		protected override void SetUp()
		{
			base.SetUp();
			MockENettWebService.ClearInstance();
		}

		protected override void TearDown()
		{
			base.TearDown();
			MockENettWebService.ClearInstance();
		}

		#region Test Classes

		public class PaymentBatchForm_ForTestOnly : PaymentBatchForm
		{
			public PaymentBatchForm_ForTestOnly(APPaymentBatchPoster paymentBatchPosterBizO)
				: base(paymentBatchPosterBizO)
			{
			}

			protected override PaymentBatchPrintManager PrintManager
			{
				get
				{
					if (fTestPrintManager == null)
					{
						fTestPrintManager = new PaymentBatchPrintManager.TestPaymentPrintManager(PaymentCollectionForPrinting_ForTestOnly, FirstApproval_ForTestOnly, TransactionTypes.Payment, BusinessEntity.Factory);
					}

					return fTestPrintManager;
				}
			}
			PaymentBatchPrintManager.TestPaymentPrintManager fTestPrintManager;

			public void HandleSaveException_ForTestOnly(Exception e)
			{
				HandleSaveException(e);
			}
		}

		enum PaymentBatchCreatingStatus
		{
			New,
			SaveAsDraft,
			SaveAsFullyApproved,
		}

#endregion

#endregion
	}
}
