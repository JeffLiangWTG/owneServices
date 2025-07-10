using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.Accounting.Business.ARAP.EPayment;
using Enterprise.Accounting.Business.ARAP.HotCheque;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.GenericCharge;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.DataTransfer.eNett_Integration;
using Enterprise.Accounting.DataTransfer.eNett_Integration.Testing;
using Enterprise.Accounting.GUI.ARAP.Payment;
using Enterprise.Accounting.GUI.ARAP.ReceiptPayment;
using Enterprise.Accounting.GUI.ARAP.TransactionView;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core.Forms;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Encryption;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using DealStatusCodes = Enterprise.MasterFiles.Business.EPaymentStatusCodes.Deal;
using QuoteStatusCodes = Enterprise.MasterFiles.Business.EPaymentStatusCodes.Quote;

namespace Enterprise.Accounting.GUI.ARAP.Testing
{
	[TestedType(typeof(PaymentApprovalForm))]
	public class PaymentApprovalFormTestCase : ZFormBasherTest
	{
		public void TestPaymentReasonVisibility()
		{
			SetupDataForTest();
			var bankAccount = TestObjectCreator.CreateEPaymentBankAccount();
			var payment = TestObjectCreator.GetNewTestAPPaymentApproval(TestObjectCreator.AALSHI, bankAccount, ReceiptTypes.EPayment, "2", 200m, TestCheques);
			using (var paymentForm = new PaymentApprovalForm(payment))
			{
				paymentForm.Show();
				Application.DoEvents();

				Assert(payment.IsEPayment);
				var paymentReasonDropEdit = paymentForm.Controls.Find("PaymentReasonDropEdit", true).First() as ZDropEdit;
				var securityCodeTextBox = paymentForm.Controls.Find("CardSecurityCodeTextBox", true).First() as ZTextBox;
				Assert(paymentReasonDropEdit.Visible);
				Assert("Security Code should not be visible when Payment Reason is visible, because they overlap each other.", !securityCodeTextBox.Visible);

				payment.AV_PaymentType = ReceiptTypes.eNettCreditCard;
				Assert("Payment Reason should not be visible when Security Code is visible, because they overlap each other.", !paymentReasonDropEdit.Visible);
				Assert(securityCodeTextBox.Visible);

				payment.AV_PaymentType = ReceiptTypes.Cheque;
				Assert(!paymentReasonDropEdit.Visible);
				Assert(!securityCodeTextBox.Visible);
			}
		}

		public void TestDisclaimerMessage()
		{
			SetupDataForTest();
			var bankAccount = TestObjectCreator.CreateEPaymentBankAccount();
			var payment = TestObjectCreator.GetNewTestAPPaymentApproval(TestObjectCreator.AALSHI, bankAccount, ReceiptTypes.EPayment, "2", 200m, TestCheques);

			using (var form = new PaymentApprovalForm(payment))
			{
				form.Show();

				var disclaimerMessage = (ZLabel)form.Controls.Find("DisclaimerMessageLabel", true).FirstOrDefault();
				AssertNotNull(disclaimerMessage);
				AssertEquals(System.Drawing.SystemColors.Highlight, disclaimerMessage.ForeColor);

				Assert(payment.BankAccount.IsEPaymentAccount);
				Assert(disclaimerMessage.Visible);
				AssertEquals(@"FX quotes are provided by and the FX transaction is executed by OFX, a third party service provider.
CargoWise provides the messaging and information exchange only.", disclaimerMessage.Text);

				payment.AV_AB = TestObjectCreator.AUDBankAccount.PK;
				Assert(!payment.BankAccount.IsEPaymentAccount);
				Assert(!disclaimerMessage.Visible);
				AssertEquals(string.Empty, disclaimerMessage.Text);

				payment.AV_AB = ZGuid.Empty;
				Assert(!disclaimerMessage.Visible);
			}
		}

		public void TestLearnMoreButtonOpensUpProductMarketingPage()
		{
			SetupDataForTest();
			var bankAccount = TestObjectCreator.CreateEPaymentBankAccount();
			var payment = TestObjectCreator.GetNewTestAPPaymentApproval(TestObjectCreator.AALSHI, bankAccount, ReceiptTypes.EPayment, "2", 200m, TestCheques);

			using (var form = new PaymentApprovalForm(payment))
			{
				form.Show();
				var learnMoreButton = (ZButton)form.Controls.Find("LearnMoreButton", true).FirstOrDefault();
				AssertNotNull(learnMoreButton);

				Assert(payment.BankAccount.IsEPaymentAccount);
				Assert(learnMoreButton.Visible);
				WebUrlLauncher.ClearLastUrlLaunched();
				learnMoreButton.PerformClick();
				AssertEquals(AccountingMasterFilesRegistry.Instance.EPaymentProductMarketingWebURL.Value, WebUrlLauncher.LastUrlLaunched);
			}
		}

		public void TestEPaymentProviderLogoAndServiceProviderLabel()
		{
			SetupDataForTest();
			var payment = TestObjectCreator.GetNewTestAPPaymentApproval(TestObjectCreator.AALSHI, TestBank, ReceiptTypes.EPayment, "2", 200m, TestCheques);

			using (var form = new PaymentApprovalForm(payment))
			{
				form.Show();

				var providerLogoPictureBox = (KPictureBox)form.Controls.Find("ProviderLogoPictureBox", true).FirstOrDefault();
				AssertNotNull(providerLogoPictureBox);
				AssertEquals(PictureBoxSizeMode.Zoom, providerLogoPictureBox.SizeMode);
				var serviceProviderLabel = (ZLabel)form.Controls.Find("ServiceProviderLabel", true).FirstOrDefault();
				AssertNotNull(serviceProviderLabel);

				Assert(!payment.BankAccount.IsEPaymentAccount);
				Assert(!providerLogoPictureBox.Visible);
				Assert(!serviceProviderLabel.Visible);
				AssertNull("OFX logo should not be displayed", providerLogoPictureBox.Image);
				WebUrlLauncher.ClearLastUrlLaunched();
				form.ProviderLogoPictureBox_Click_ForTestOnly(null, null);
				AssertEquals(string.Empty, WebUrlLauncher.LastUrlLaunched);

				var bankAccount = TestObjectCreator.CreateEPaymentBankAccount();
				payment.AV_AB = bankAccount.PK;
				Assert(payment.BankAccount.IsEPaymentAccount);
				Assert(providerLogoPictureBox.Visible);
				Assert(serviceProviderLabel.Visible);
				AssertNotNull("OFX logo should be displayed", providerLogoPictureBox.Image);
				AssertImageEquals("OFX logo should be displayed", AccountingMasterFilesRegistry.Instance.OFXLogo.Value, providerLogoPictureBox.Image);
				WebUrlLauncher.ClearLastUrlLaunched();
				form.ProviderLogoPictureBox_Click_ForTestOnly(null, null);
				AssertEquals(AccountingMasterFilesRegistry.Instance.OFXWebURL.Value, WebUrlLauncher.LastUrlLaunched);

				payment.AV_AB = ZGuid.Empty;
				Assert(!providerLogoPictureBox.Visible);
				Assert(!serviceProviderLabel.Visible);
				AssertNull("OFX logo should not be displayed", providerLogoPictureBox.Image);
				WebUrlLauncher.ClearLastUrlLaunched();
				form.ProviderLogoPictureBox_Click_ForTestOnly(null, null);
				AssertEquals(string.Empty, WebUrlLauncher.LastUrlLaunched);
			}
		}

		#region Process E-Payment

		public void TestProcessEPaymentButton_Click_WhenAccountIsEpaAndUserIsNotRegistered()
		{
			SetupDataForTest();
			var bankAccount = TestObjectCreator.CreateEPaymentBankAccount();
			var payment = TestObjectCreator.GetNewTestAPPaymentApproval(TestObjectCreator.AALSHI, bankAccount, ReceiptTypes.Cheque, "2", 200m, TestCheques);
			payment.AV_RX_NKPaymentCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			payment.AV_PayExRate = 1m;
			payment.AV_Status = PaymentApprovalStatus.FullyApproved;
			Factory.Save();
			var quote = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Accepted, payment);
			quote.QU_FeeAmount = 10m;
			quote.QU_RX_NKFeeCurrency = payment.AV_Calc_LocalCurrency;
			Factory.Save();

			payment.PaymentQuotes.Reload(true);

			var deals = Factory.Load<AccEPaymentDeal>(new ZQuery(AccEPaymentDealSchema.AED_QU_Quote, quote.PK));
			AssertEquals(0, deals.Length);

			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			using (var paymentForm = new PaymentApprovalForm(payment))
			{
				paymentForm.Show();
				Application.DoEvents();

				UnitTestUserNotification.Instance.ClearMessages();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				paymentForm.ProcessEPaymentButton_ForTestOnly.PerformClick();
				AssertEquals("Your staff profile is not currently registered in the list of users who can use this Bank Account. To manage users, edit the Bank Account and select “Manage Users”.", UnitTestUserNotification.Instance.LastMessage.Text);

				paymentForm.CheckEPayRateButton_ForTestOnly.PerformClick();
				AssertEquals("Your staff profile is not currently registered in the list of users who can use this Bank Account. To manage users, edit the Bank Account and select “Manage Users”.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestProcessEPaymentButton_Click_WhenAccountIsEpaAndUserIsUnauthorized()
		{
			SetupDataForTest();
			var bankAccount = TestObjectCreator.CreateEPaymentBankAccount();
			var payment = TestObjectCreator.GetNewTestAPPaymentApproval(TestObjectCreator.AALSHI, bankAccount, ReceiptTypes.Cheque, "2", 200m, TestCheques);
			payment.AV_RX_NKPaymentCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			payment.AV_PayExRate = 1m;
			payment.AV_Status = PaymentApprovalStatus.FullyApproved;
			TestObjectCreator.CreateEPaymentStaffToken(bankAccount.PK, ZDateTime.Empty, Env.CurrentUser.Initials, AccEPaymentStaffTokenLookups.StatusCodes.Pending);
			Factory.Save();
			var quote = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Accepted, payment);
			quote.QU_FeeAmount = 10m;
			quote.QU_RX_NKFeeCurrency = payment.AV_Calc_LocalCurrency;

			var beneficary = TestObjectCreator.CreateEPaymentBeneficiary("apple");
			var accountDetails = payment.PayeeOrganisation.CompanyData.AccountDetailsCollection;
			accountDetails.RemoveAndDeleteAll();
			var ofxAccountDetail = accountDetails.AddNew();
			ofxAccountDetail.A1_PaymentMethod = EPaymentMethods.EPaymentViaOFX;
			ofxAccountDetail.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			ofxAccountDetail.A1_IsDefaultAccount = true;
			ofxAccountDetail.A1_EPaymentBeneficiaryId = beneficary.PK;

			Factory.Save();

			payment.PaymentQuotes.Reload(true);

			var deals = Factory.Load<AccEPaymentDeal>(new ZQuery(AccEPaymentDealSchema.AED_QU_Quote, quote.PK));
			AssertEquals(0, deals.Length);

			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			using (var paymentForm = new PaymentApprovalForm(payment))
			{
				paymentForm.Show();
				Application.DoEvents();

				UnitTestUserNotification.Instance.ClearMessages();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

				paymentForm.ProcessEPaymentButton_ForTestOnly.PerformClick();
				AssertEquals(@"You need to authorize your OFX user account before proceeding.
Please note that the authorization process can take up to several minutes.
Would you like to authorize your account now?", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessages();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				paymentForm.CheckEPayRateButton_ForTestOnly.PerformClick();
				AssertEquals(@"You need to authorize your OFX user account before proceeding.
Please note that the authorization process can take up to several minutes.
Would you like to authorize your account now?", UnitTestUserNotification.Instance.LastMessage.Text);

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
			SetupDataForTest();
			var bankAccount = TestObjectCreator.CreateEPaymentBankAccount();
			var payment = TestObjectCreator.GetNewTestAPPaymentApproval(TestObjectCreator.AALSHI, bankAccount, ReceiptTypes.Cheque, "2", 200m, TestCheques);
			payment.AV_RX_NKPaymentCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			payment.AV_PayExRate = 1m;
			payment.AV_Status = PaymentApprovalStatus.FullyApproved;
			TestObjectCreator.CreateEPaymentStaffToken(bankAccount.PK, ZDateTime.UtcNow.AddHours(-1), Env.CurrentUser.Initials, AccEPaymentStaffTokenLookups.StatusCodes.Authorised);
			Factory.Save();
			var quote = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Accepted, payment);
			quote.QU_FeeAmount = 10m;
			quote.QU_RX_NKFeeCurrency = payment.AV_Calc_LocalCurrency;

			var beneficary = TestObjectCreator.CreateEPaymentBeneficiary("apple");
			var accountDetails = payment.PayeeOrganisation.CompanyData.AccountDetailsCollection;
			accountDetails.RemoveAndDeleteAll();
			var ofxAccountDetail = accountDetails.AddNew();
			ofxAccountDetail.A1_PaymentMethod = EPaymentMethods.EPaymentViaOFX;
			ofxAccountDetail.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			ofxAccountDetail.A1_IsDefaultAccount = true;
			ofxAccountDetail.A1_EPaymentBeneficiaryId = beneficary.PK;

			Factory.Save();

			payment.PaymentQuotes.Reload(true);

			var deals = Factory.Load<AccEPaymentDeal>(new ZQuery(AccEPaymentDealSchema.AED_QU_Quote, quote.PK));
			AssertEquals(0, deals.Length);

			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			using (var paymentForm = new PaymentApprovalForm(payment))
			{
				paymentForm.Show();
				Application.DoEvents();

				UnitTestUserNotification.Instance.ClearMessages();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

				paymentForm.ProcessEPaymentButton_ForTestOnly.PerformClick();
				AssertEquals(@"You need to authorize your OFX user account before proceeding.
Please note that the authorization process can take up to several minutes.
Would you like to authorize your account now?", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessages();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				paymentForm.CheckEPayRateButton_ForTestOnly.PerformClick();
				AssertEquals(@"You need to authorize your OFX user account before proceeding.
Please note that the authorization process can take up to several minutes.
Would you like to authorize your account now?", UnitTestUserNotification.Instance.LastMessage.Text);

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

		public void TestProcessEPaymentButton_Click_WhenPaymentHasAcceptedQuoteAndPaymentDetailsAreMatching()
		{
			SetupDataForTest();
			var bankAccount = TestObjectCreator.CreateEPaymentBankAccount();
			var payment = TestObjectCreator.GetNewTestAPPaymentApproval(TestObjectCreator.AALSHI, bankAccount, ReceiptTypes.Cheque, "2", 200m, TestCheques);
			payment.AV_RX_NKPaymentCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			payment.AV_PayExRate = 1m;
			payment.AV_Status = PaymentApprovalStatus.FullyApproved;
			TestObjectCreator.CreateEPaymentStaffToken(bankAccount.PK, ZDateTime.UtcNow.AddHours(2), Env.CurrentUser.Initials, AccEPaymentStaffTokenLookups.StatusCodes.Authorised);
			var quote = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Accepted, payment);
			quote.QU_FeeAmount = 10m;
			quote.QU_RX_NKFeeCurrency = payment.AV_Calc_LocalCurrency;
			var beneficary = TestObjectCreator.CreateEPaymentBeneficiary("apple");
			var accountDetails = payment.PayeeOrganisation.CompanyData.AccountDetailsCollection;
			accountDetails.RemoveAndDeleteAll();
			var ofxAccountDetail = accountDetails.AddNew();
			ofxAccountDetail.A1_PaymentMethod = EPaymentMethods.EPaymentViaOFX;
			ofxAccountDetail.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			ofxAccountDetail.A1_IsDefaultAccount = true;
			ofxAccountDetail.A1_EPaymentBeneficiaryId = beneficary.PK;
			Factory.Save();

			payment.PaymentQuotes.Reload(true);

			var deals = Factory.Load<AccEPaymentDeal>(new ZQuery(AccEPaymentDealSchema.AED_QU_Quote, quote.PK));
			AssertEquals(0, deals.Length);

			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			using (var paymentForm = new PaymentApprovalForm(payment))
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

				Assert(!paymentForm.SaveAsDraftButton_ForTestOnly.Enabled);
			}
		}

		public void TestProcessEPaymentButton_Click_WhenPaymentHasAcceptedQuoteAndPaymentDetailsAreDifferent()
		{
			SetupDataForTest();
			var bankAccount = TestObjectCreator.CreateEPaymentBankAccount();
			var payment = TestObjectCreator.GetNewTestAPPaymentApproval(TestObjectCreator.AALSHI, bankAccount, ReceiptTypes.Cheque, "2", 200m, TestCheques);
			payment.AV_RX_NKPaymentCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			payment.AV_Status = PaymentApprovalStatus.FullyApproved;
			TestObjectCreator.CreateEPaymentStaffToken(bankAccount.PK, ZDateTime.UtcNow.AddHours(2), Env.CurrentUser.Initials, AccEPaymentStaffTokenLookups.StatusCodes.Authorised);
			var quote = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Accepted, payment);
			quote.QU_FromAmount = 300m;
			var beneficary = TestObjectCreator.CreateEPaymentBeneficiary("apple");
			var accountDetails = payment.PayeeOrganisation.CompanyData.AccountDetailsCollection;
			accountDetails.RemoveAndDeleteAll();
			var ofxAccountDetail = accountDetails.AddNew();
			ofxAccountDetail.A1_PaymentMethod = EPaymentMethods.EPaymentViaOFX;
			ofxAccountDetail.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			ofxAccountDetail.A1_IsDefaultAccount = true;
			ofxAccountDetail.A1_EPaymentBeneficiaryId = beneficary.PK;
			Factory.Save();

			payment.PaymentQuotes.Reload(true);

			var deals = Factory.Load<AccEPaymentDeal>(new ZQuery(AccEPaymentDealSchema.AED_QU_Quote, quote.PK));
			AssertEquals(0, deals.Length);

			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			using (var paymentForm = new PaymentApprovalForm(payment))
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
			var payment = TestObjectCreator.GetNewTestAPPaymentApproval(TestObjectCreator.AALSHI, bankAccount, ReceiptTypes.Cheque, "2", 200m, TestCheques);
			payment.AV_RX_NKPaymentCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			payment.AV_Status = PaymentApprovalStatus.FullyApproved;
			TestObjectCreator.CreateEPaymentStaffToken(bankAccount.PK, ZDateTime.UtcNow.AddHours(2), Env.CurrentUser.Initials, AccEPaymentStaffTokenLookups.StatusCodes.Authorised);
			TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Discarded, payment);
			TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Failed, payment);
			TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Error, payment);
			var beneficary = TestObjectCreator.CreateEPaymentBeneficiary("apple");
			var accountDetails = payment.PayeeOrganisation.CompanyData.AccountDetailsCollection;
			accountDetails.RemoveAndDeleteAll();
			var ofxAccountDetail = accountDetails.AddNew();
			ofxAccountDetail.A1_PaymentMethod = EPaymentMethods.EPaymentViaOFX;
			ofxAccountDetail.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			ofxAccountDetail.A1_IsDefaultAccount = true;
			ofxAccountDetail.A1_EPaymentBeneficiaryId = beneficary.PK;
			Factory.Save();

			AssertEquals(3, payment.PaymentQuotes.Count);

			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			using (var paymentForm = new PaymentApprovalForm(payment))
			{
				paymentForm.Show();
				Application.DoEvents();

				UnitTestUserNotification.Instance.ClearMessages();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				paymentForm.ProcessEPaymentButton_ForTestOnly.PerformClick();
				AssertEquals("Payment must have an accepted E-Quote in order to continue. Would you like to request an E-Quote now?", UnitTestUserNotification.Instance.LastMessage.Text);

				payment.PaymentQuotes.Reload(true);
				AssertEquals(3, payment.PaymentQuotes.Count);

				UnitTestUserNotification.Instance.ClearMessages();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				paymentForm.ProcessEPaymentButton_ForTestOnly.PerformClick();
				AssertEquals("E-Quote request generated to service provider OFX. Response may take from a few moments up to several minutes to be received.", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText("Payment must have an accepted E-Quote in order to continue. Would you like to request an E-Quote now?"));

				payment.PaymentQuotes.Reload(true);
				AssertEquals(4, payment.PaymentQuotes.Count);
				AssertEquals(1, payment.PaymentQuotes.Cast<EPaymentQuote>().Count(x => x.QU_Status == QuoteStatusCodes.Queued));
				AssertEquals(3, payment.PaymentQuotes.Cast<EPaymentQuote>().Count(x => x.QU_Status == QuoteStatusCodes.Discarded));
			}
		}

		public void TestProcessEPaymentButton_Click_WhenPaymentHasAReceivedQuote()
		{
			SetupDataForTest();
			var bankAccount = TestObjectCreator.CreateEPaymentBankAccount();
			var payment = TestObjectCreator.GetNewTestAPPaymentApproval(TestObjectCreator.AALSHI, bankAccount, ReceiptTypes.Cheque, "2", 200m, TestCheques);
			payment.AV_RX_NKPaymentCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			payment.AV_PayExRate = 1m;
			payment.AV_Status = PaymentApprovalStatus.FullyApproved;
			TestObjectCreator.CreateEPaymentStaffToken(bankAccount.PK, ZDateTime.UtcNow.AddHours(2), Env.CurrentUser.Initials, AccEPaymentStaffTokenLookups.StatusCodes.Authorised);
			var quote = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Received, payment);
			quote.QU_FeeAmount = 10m;
			quote.QU_RX_NKFeeCurrency = payment.AV_Calc_LocalCurrency;
			var beneficary = TestObjectCreator.CreateEPaymentBeneficiary("apple");
			var accountDetails = payment.PayeeOrganisation.CompanyData.AccountDetailsCollection;
			accountDetails.RemoveAndDeleteAll();
			var ofxAccountDetail = accountDetails.AddNew();
			ofxAccountDetail.A1_PaymentMethod = EPaymentMethods.EPaymentViaOFX;
			ofxAccountDetail.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			ofxAccountDetail.A1_IsDefaultAccount = true;
			ofxAccountDetail.A1_EPaymentBeneficiaryId = beneficary.PK;
			Factory.Save();
			payment.PaymentQuotes.Reload(true);

			var deals = Factory.Load<AccEPaymentDeal>(new ZQuery(AccEPaymentDealSchema.AED_QU_Quote, quote.PK));
			AssertEquals(0, deals.Length);

			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			using (var paymentForm = new PaymentApprovalForm(payment))
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
Payment must have an accepted E-Quote in order to continue. The following E-Quote is available for this Payment and will be automatically accepted if you proceed to book the deal.

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
			var payment = TestObjectCreator.GetNewTestAPPaymentApproval(TestObjectCreator.AALSHI, TestBank, ReceiptTypes.Cheque, "2", 200m, TestCheques);
			payment.AV_RX_NKPaymentCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			payment.AV_Status = PaymentApprovalStatus.FullyApproved;
			Factory.Save();
			var quote = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Received, payment);
			Factory.Save();

			AssertNotEquals(AccountTypeCodeDescriptionPairList.Codes.EPA, payment.BankAccount.AB_AccountType);
			var deals = Factory.Load<AccEPaymentDeal>(new ZQuery(AccEPaymentDealSchema.AED_QU_Quote, quote.PK));
			AssertEquals(0, deals.Length);

			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			using (var paymentForm = new PaymentApprovalForm(payment))
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

		public void TestEPaymentControlsVisibilityDependsOnRegistry()
		{
			SetupDataForTest();
			AssertEPaymentControlsVisibility(true);
			AssertEPaymentControlsVisibility(false);
		}

		void AssertEPaymentControlsVisibility(bool isOFXEPaymentEnabled)
		{
			var payment = TestObjectCreator.GetNewTestAPPaymentApproval(TestObjectCreator.AALSHI, TestBank, ReceiptTypes.Cheque, "2", 200m, TestCheques);

			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, isOFXEPaymentEnabled))
			{
				using (var paymentForm = new PaymentApprovalForm(payment))
				{
					paymentForm.Show();
					if (isOFXEPaymentEnabled)
					{
						Assert("CheckEPayRateButton should be visible when E-Payment functionality is enabled.", paymentForm.CheckEPayRateButton_ForTestOnly.Visible);
						Assert("Learn More Button should be visible when E-Payment functionality is enabled.", paymentForm.LearnMoreButton_ForTestOnly.Visible);
						Assert("ProcessEPaymentButton should be visible when E-Payment functionality is enabled.", paymentForm.ProcessEPaymentButton_ForTestOnly.Visible);
						Assert("EPaymentTabPage should be visible when E-Payment functionality is enabled.", paymentForm.EPaymentTabPage_ForTestOnly.TabVisible);
					}
					else
					{
						Assert("CheckEPayRateButton should be invisible when E-Payment functionality is disabled.", !paymentForm.CheckEPayRateButton_ForTestOnly.Visible);
						Assert("Learn More Button should be invisible when E-Payment functionality is disabled.", !paymentForm.LearnMoreButton_ForTestOnly.Visible);
						Assert("ProcessEPaymentButton should be invisible when E-Payment functionality is disabled.", !paymentForm.ProcessEPaymentButton_ForTestOnly.Visible);
						Assert("EPaymentTabPage should be invisible when E-Payment functionality is disabled.", !paymentForm.EPaymentTabPage_ForTestOnly.TabVisible);
					}
				}
			}
		}

		#endregion

		public void TestUpdateAccountNameCaption()
		{
			SetupDataForTest();
			var bankAccount = TestObjectCreator.CreateEPaymentBankAccount();
			var payment = TestObjectCreator.GetNewTestAPPaymentApproval(TestObjectCreator.AALSHI, bankAccount, ReceiptTypes.EPayment, "2", 200m, TestCheques);

			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			using (var paymentForm = new PaymentApprovalWithAuthorisationForm(payment))
			{
				paymentForm.Show();
				Application.DoEvents();

				var bankAccountGuidFindBox = paymentForm.Controls.Find("BankAccountGuidFindBox", true).First() as ZGuidFindBox;
				AssertEquals("E-Payment Account", bankAccountGuidFindBox.CaptionResourceString.Caption);

				payment.AV_PaymentType = ReceiptTypes.Cheque;
				AssertEquals("Bank Account", bankAccountGuidFindBox.CaptionResourceString.Caption);
			}
		}

		[TestDate(2021, 8, 26, 22, 00, 00)]
		public void TestDisplayQuoteTimeInLocalTimezone()
		{
			SetupDataForTest();
			var payment = TestObjectCreator.GetNewTestAPPaymentApproval(TestObjectCreator.AALSHI, TestBank, ReceiptTypes.Cheque, "2", 200m, TestCheques);
			payment.AV_RX_NKPaymentCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			payment.AV_Status = PaymentApprovalStatus.AwaitingApproval;
			Factory.Save();
			var quote = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Received, payment);
			var utcTime = quote.QU_LastResponseReceivedUtc;
			var localTime = utcTime.ToLocalBranchTime();
			Factory.Save();
			payment.PaymentQuotes.Reload(true);
			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			using (var paymentForm = new PaymentApprovalForm(payment))
			{
				paymentForm.Show();
				Application.DoEvents();

				var bankAndEPaymentTabControl = paymentForm.Controls.Find("BankAndEPaymentTabControl", true).First() as ZTabControl;
				AssertNotNull(bankAndEPaymentTabControl);
				bankAndEPaymentTabControl.SelectNextTabPage();

				AssertEquals(1, payment.PaymentQuotes_ForDisplay.Count);
				AssertEquals(localTime, payment.PaymentQuotes_ForDisplay[0].LastResponseReceivedLocalTime);
			}
		}

		[TestDate(2021, 8, 26, 22, 00, 00)]
		public void TestDisplayDealTimeInLocalTimezone()
		{
			SetupDataForTest();
			var payment = TestObjectCreator.GetNewTestAPPaymentApproval(TestObjectCreator.AALSHI, TestBank, ReceiptTypes.Cheque, "2", 200m, TestCheques);
			payment.AV_RX_NKPaymentCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			payment.AV_Status = PaymentApprovalStatus.FullyApproved;
			TestObjectCreator.CreateValidEPaymentDealForStatus(DealStatusCodes.Accepted, payment);
			Factory.Save();
			var localTime = payment.AV_SystemCreateTimeUtc.ToLocalBranchTime();

			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			using (var paymentForm = new PaymentApprovalForm(payment))
			{
				paymentForm.Show();
				Application.DoEvents();

				var bankAndEPaymentTabControl = paymentForm.Controls.Find("BankAndEPaymentTabControl", true).First() as ZTabControl;
				AssertNotNull(bankAndEPaymentTabControl);
				bankAndEPaymentTabControl.SelectNextTabPage();

				AssertEquals(localTime, payment.DealSubmittedLocalTime);
				AssertEquals(localTime, payment.DealLastResponseLocalTime);

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
			var payment = TestObjectCreator.GetNewTestAPPaymentApproval(TestObjectCreator.AALSHI, TestBank, ReceiptTypes.Cheque, "2", 200m, TestCheques);
			payment.AV_RX_NKPaymentCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			payment.AV_Status = PaymentApprovalStatus.AwaitingApproval;
			Factory.Save();
			var quote = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Received, payment);
			Factory.Save();

			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			using (var paymentForm = new PaymentApprovalForm(payment))
			{
				paymentForm.Show();
				Application.DoEvents();

				var bankAndEPaymentTabControl = paymentForm.Controls.Find("BankAndEPaymentTabControl", true).First() as ZTabControl;
				AssertNotNull(bankAndEPaymentTabControl);
				bankAndEPaymentTabControl.SelectNextTabPage();

				UnitTestUserNotification.Instance.ClearMessages();
				payment.AV_PaymentComment = "new comment";
				paymentForm.AcceptQuoteButton_ForTestOnly.PerformClick();
				AssertEquals("Please save your payment approval first.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(QuoteStatusCodes.Received, quote.QU_Status);
			}
		}

		public void TestAcceptQuoteButton_Click_WhenNoQuotesSelected()
		{
			SetupDataForTest();
			var payment = TestObjectCreator.GetNewTestAPPaymentApproval(TestObjectCreator.AALSHI, TestBank, ReceiptTypes.Cheque, "2", 200m, TestCheques);
			payment.AV_RX_NKPaymentCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			payment.AV_Status = PaymentApprovalStatus.AwaitingApproval;
			Factory.Save();
			var quote = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Received, payment);
			Factory.Save();
			payment.PaymentQuotes.Reload(true);

			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			using (var paymentForm = new PaymentApprovalForm(payment))
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
			var payment = TestObjectCreator.GetNewTestAPPaymentApproval(TestObjectCreator.AALSHI, TestBank, ReceiptTypes.Cheque, "2", 200m, TestCheques);
			payment.AV_RX_NKPaymentCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			payment.AV_Status = PaymentApprovalStatus.AwaitingApproval;
			Factory.Save();
			var quote1 = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Received, payment);
			var quote2 = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Requested, payment);
			Factory.Save();
			payment.PaymentQuotes.Reload(true);

			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			using (var paymentForm = new PaymentApprovalForm(payment))
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
			var payment = TestObjectCreator.GetNewTestAPPaymentApproval(TestObjectCreator.AALSHI, TestBank, ReceiptTypes.Cheque, "2", 200m, TestCheques);
			payment.AV_RX_NKPaymentCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			payment.AV_PayExRate = 1.4m;
			payment.AV_Amount = 1000m;
			payment.AV_Status = PaymentApprovalStatus.AwaitingApproval;
			Factory.Save();

			//invoice -> 1000 USD / 1.6 ex rate = 625 AUD
			var apInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV123", TestObjectCreator.USD, 1.6m, 1000m, 0m, 625m, 0m, TestObjectCreator.Creditor1, TestObjectCreator.GLHeader1.PK);
			Factory.Save();

			//exx journal -> 625 AUD - 714.29 AUD = -89.29 AUD
			var paymentApprovalItem = Factory.New<PaymentApprovalItem>();
			paymentApprovalItem.A2_AH = apInvoice.PK;
			paymentApprovalItem.A2_AV = payment.PK;
			paymentApprovalItem.A2_PaymentThisRun = -apInvoice.AH_LocalExTaxAmount;
			payment.AV_ExchangeDifference = -89.29m; //for payment approvals, we do not create database record for exx journal, we store the amount on AV_ExchangeDifference column.
			Factory.Save();

			//quote -> 1000 USD / 1.2 ex rate = 833.33 AUD
			var quote1 = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Received, payment);
			quote1.QU_FromAmount = 833.33m;
			quote1.QU_ToAmount = 1000m;
			quote1.QU_ExchangeRate = 1.2m;
			quote1.QU_ExchangeRateInverted = 0.8333m;
			var quote2 = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Requested, payment);
			Factory.Save();

			var exxJournals = Factory.Load<ExchangeDifference>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.ExchangeDifference));
			AssertEquals(0, exxJournals.Length);

			var formFactory = new BusinessObjectFactory();
			var quote1InFormFactory = formFactory.LoadTop1<AccEPaymentQuote>(new ZQuery(AccEPaymentQuoteSchema.PK, quote1.PK));
			var quote2InFormFactory = formFactory.LoadTop1<AccEPaymentQuote>(new ZQuery(AccEPaymentQuoteSchema.PK, quote2.PK));
			var paymentInFormFactory = formFactory.LoadTop1<PaymentApprovalBase>(new ZQuery(AccPaymentApprovalSchema.PK, payment.PK));

			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			using (var paymentForm = new PaymentApprovalForm(paymentInFormFactory))
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
				AssertEquals($"Quote {selectedQuote.RealQuote.QU_InternalReference} has been accepted.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(QuoteStatusCodes.Accepted, quote1InFormFactory.QU_Status);
				AssertEquals(QuoteStatusCodes.Discarded, quote2InFormFactory.QU_Status);

				//new payment approval values -> 1000 USD / 1.2 ex rate = 833.33 AUD
				//invoice -> 1000 USD / 1.6 ex rate = 625 AUD
				//new exx journal -> 625 AUD - 833.33 AUD = -208.33 AUD
				AssertEquals(833.33m, paymentInFormFactory.AV_Calc_LocalAmount);
				AssertEquals(1000m, paymentInFormFactory.AV_Amount);
				AssertEquals(1.2000m, paymentInFormFactory.AV_PayExRate.Round(4));
				AssertEquals(-208.33m, paymentInFormFactory.AV_ExchangeDifference);
				var exxJournalsInFormFactory = formFactory.Load<ExchangeDifference>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.ExchangeDifference));
				AssertEquals(0, exxJournalsInFormFactory.Length);
			}
		}

		public void TestAcceptQuoteButton_Click_Error()
		{
			SetupDataForTest();
			var payment = TestObjectCreator.GetNewTestAPPaymentApproval(TestObjectCreator.AALSHI, TestBank, ReceiptTypes.Cheque, "2", 200m, TestCheques);
			payment.AV_RX_NKPaymentCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			payment.AV_Status = PaymentApprovalStatus.AwaitingApproval;
			Factory.Save();
			var quote1 = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Queued, payment);
			var quote2 = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Requested, payment);
			Factory.Save();
			payment.PaymentQuotes.Reload(true);

			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			using (var paymentForm = new PaymentApprovalForm(payment))
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

		#region Check Ex Rate Button

		public void TestCheckEPaymentExchangeRateButtonIsEnabledBasedOnCurrency()
		{
			SetupDataForTest();
			var payment = TestObjectCreator.GetNewTestAPPaymentApproval(TestObjectCreator.AALSHI, TestBank, ReceiptTypes.Cheque, "2", 200m, TestCheques);
			payment.AV_RX_NKPaymentCurrency = Core.Constants.CurrencyCodes.Australia;
			AssertEquals(payment.AV_RX_NKPaymentCurrency, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);

			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			using (var paymentForm = new PaymentApprovalForm(payment))
			{
				paymentForm.Show();
				Assert("button should be disabled when payment currency is same as local currency", !paymentForm.CheckEPayRateButton_ForTestOnly.Enabled);

				payment.AV_RX_NKPaymentCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
				AssertNotEquals(payment.AV_RX_NKPaymentCurrency, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
				Assert("button should be enabled when payment currency is different to local currency", paymentForm.CheckEPayRateButton_ForTestOnly.Enabled);

				payment.AV_RX_NKPaymentCurrency = Core.Constants.CurrencyCodes.Australia;
				AssertEquals(payment.AV_RX_NKPaymentCurrency, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
				Assert("button should be disabled when payment currency is same as local currency", !paymentForm.CheckEPayRateButton_ForTestOnly.Enabled);
			}
		}

		public void TestCheckEPaymentExchangeRateButtonIsEnabledBasedOnApprovalStatus()
		{
			SetupDataForTest();
			var payment = TestObjectCreator.GetNewTestAPPaymentApproval(TestObjectCreator.AALSHI, TestBank, ZArchitecture.Core.ReceiptTypes.Cheque, "2", 200m, TestCheques);
			payment.AV_RX_NKPaymentCurrency = "USD";
			Factory.Save();

			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			{
				var supportedStatusList = new ZString[] { PaymentApprovalStatus.AwaitingApproval, PaymentApprovalStatus.FullyApproved, PaymentApprovalStatus.Rejected, PaymentApprovalStatus.Draft };
				var unSupportedStatusList = new ZString[] { PaymentApprovalStatus.Cancelled, PaymentApprovalStatus.Posted };
				foreach (var status in supportedStatusList)
				{
					payment.AV_Status = status;
					using (var paymentForm = new PaymentApprovalForm(payment))
					{
						paymentForm.Show();
						Assert(paymentForm.CheckEPayRateButton_ForTestOnly.Enabled);
					}
				}
				foreach (var status in unSupportedStatusList)
				{
					payment.AV_Status = status;
					using (var paymentForm = new PaymentApprovalForm(payment))
					{
						paymentForm.Show();
						Assert(!paymentForm.CheckEPayRateButton_ForTestOnly.Enabled);
					}
				}
			}
		}

		public void TestCheckEPaymentExchangeRateButtonIsEnabledWhenPaymentNotInDatabase()
		{
			SetupDataForTest();
			var payment = TestObjectCreator.GetNewTestAPPaymentApproval(TestObjectCreator.AALSHI, TestBank, ReceiptTypes.Cheque, "2", 200m, TestCheques);
			payment.AV_RX_NKPaymentCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			payment.AV_Status = PaymentApprovalStatus.AwaitingApproval;
			Assert(!payment.IsInDatabase);

			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			using (var paymentForm = new PaymentApprovalForm(payment))
			{
				paymentForm.Show();
				Assert("button should be enabled when payment is not in database", paymentForm.CheckEPayRateButton_ForTestOnly.Enabled);

				payment.Factory.Save();
				Assert(payment.IsInDatabase);
				Assert("button should be enabled when payment is in database and has awaiting approval status", paymentForm.CheckEPayRateButton_ForTestOnly.Enabled);
			}
		}

		public void TestCheckEPaymentExchangeRateButtonClick_PaymentNotInDatabase_SavingAsDraftSuccessful()
		{
			SetupDataForTest();
			var payment = TestObjectCreator.GetNewTestAPPaymentApproval(TestObjectCreator.AALSHI, TestBank, ReceiptTypes.Cheque, "2", 200m, TestCheques);
			payment.AV_RX_NKPaymentCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			Assert(!payment.IsInDatabase);
			AssertNotEquals(PaymentApprovalStatus.Draft, payment.AV_Status);
			var quote = Factory.LoadTop1<AccEPaymentQuote>(new ZQuery(AccEPaymentQuoteSchema.QU_AV, payment.PK));
			AssertNull(quote);

			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			using (var paymentForm = new PaymentApprovalForm(payment))
			{
				paymentForm.Show();
				Assert("button should be enabled when payment is not in database", paymentForm.CheckEPayRateButton_ForTestOnly.Enabled);

				var bankAndEPaymentTabControl = paymentForm.Controls.Find("BankAndEPaymentTabControl", true).First() as ZTabControl;
				AssertEquals("Before click, Bank Detail tab page should be focused", bankAndEPaymentTabControl.SelectedIndex, 0);

				UnitTestUserNotification.Instance.ClearMessages();
				paymentForm.CheckEPayRateButton_ForTestOnly.PerformClick();

				AssertEquals("E-Quote request generated to service provider OFX. Response may take from a few moments up to several minutes to be received.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("After click, E-Payment tab page should be focused", bankAndEPaymentTabControl.SelectedIndex, 1);
				Assert(payment.IsInDatabase);
				AssertEquals(PaymentApprovalStatus.Draft, payment.AV_Status);
				Assert(!payment.ReadOnly);

				quote = new BusinessObjectFactory().LoadTop1<AccEPaymentQuote>(new ZQuery(AccEPaymentQuoteSchema.QU_AV, payment.PK));
				AssertNotNull(quote);
				AssertEquals(EPaymentProviderCodes.Codes.OFX, quote.QU_ProviderCode);
				AssertEquals(200m, quote.QU_ToAmount);
				AssertEquals(Core.Constants.CurrencyCodes.UnitedStates, quote.QU_RX_NKToCurrency);
				AssertEquals(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, quote.QU_RX_NKFromCurrency);
				AssertEquals(GlbCompany.CurrentCompany.PK, quote.QU_GC);
			}
		}

		public void TestCheckEPaymentExchangeRateButtonClick_PaymentNotInDatabase_SavingAsDraftUnsuccessful()
		{
			SetupDataForTest();
			var payment = TestObjectCreator.GetNewTestAPPaymentApproval(TestObjectCreator.AALSHI, TestBank, ReceiptTypes.Cheque, "2", 200m, TestCheques);
			payment.AV_RX_NKPaymentCurrency = ZString.Empty;
			Assert(!payment.IsInDatabase);
			AssertNotEquals(PaymentApprovalStatus.Draft, payment.AV_Status);
			var quote = Factory.LoadTop1<AccEPaymentQuote>(new ZQuery(AccEPaymentQuoteSchema.QU_AV, payment.PK));
			AssertNull(quote);

			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			using (var paymentForm = new PaymentApprovalForm(payment))
			{
				paymentForm.Show();
				Assert("button should be enabled when payment is not in database", paymentForm.CheckEPayRateButton_ForTestOnly.Enabled);

				UnitTestUserNotification.Instance.ClearMessages();
				paymentForm.CheckEPayRateButton_ForTestOnly.PerformClick();

				AssertEquals("There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(!payment.IsInDatabase);
				AssertNotEquals(PaymentApprovalStatus.Draft, payment.AV_Status);
				quote = new BusinessObjectFactory().LoadTop1<AccEPaymentQuote>(new ZQuery(AccEPaymentQuoteSchema.QU_AV, payment.PK));
				AssertNull(quote);
			}
		}

		public void TestCheckEPaymentExchangeRateButtonClick_PaymentInDatabase()
		{
			SetupDataForTest();
			var payment = TestObjectCreator.GetNewTestAPPaymentApproval(TestObjectCreator.AALSHI, TestBank, ReceiptTypes.Cheque, "2", 200m, TestCheques);
			payment.AV_RX_NKPaymentCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			payment.AV_Status = PaymentApprovalStatus.Draft;
			Factory.Save();
			Assert(payment.IsInDatabase);
			var quote = Factory.LoadTop1<AccEPaymentQuote>(new ZQuery(AccEPaymentQuoteSchema.QU_AV, payment.PK));
			AssertNull(quote);

			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			using (var paymentForm = new PaymentApprovalForm(payment))
			{
				paymentForm.Show();
				Assert("button should be enabled when payment is saved as draft", paymentForm.CheckEPayRateButton_ForTestOnly.Enabled);

				var bankAndEPaymentTabControl = paymentForm.Controls.Find("BankAndEPaymentTabControl", true).First() as ZTabControl;
				AssertEquals("Before click, Bank Detail tab page should be focused", bankAndEPaymentTabControl.SelectedIndex, 0);

				UnitTestUserNotification.Instance.ClearMessages();
				payment.AV_PaymentComment = "new comment";
				paymentForm.CheckEPayRateButton_ForTestOnly.PerformClick();
				AssertEquals("Please save your payment approval first.", UnitTestUserNotification.Instance.LastMessage.Text);

				Factory.Save();

				UnitTestUserNotification.Instance.ClearMessages();
				paymentForm.CheckEPayRateButton_ForTestOnly.PerformClick();

				AssertEquals("E-Quote request generated to service provider OFX. Response may take from a few moments up to several minutes to be received.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("After click, E-Payment tab page should be focused", bankAndEPaymentTabControl.SelectedIndex, 1);
				quote = new BusinessObjectFactory().LoadTop1<AccEPaymentQuote>(new ZQuery(AccEPaymentQuoteSchema.QU_AV, payment.PK));
				AssertNotNull(quote);
				AssertEquals(EPaymentProviderCodes.Codes.OFX, quote.QU_ProviderCode);
				AssertEquals(200m, quote.QU_ToAmount);
				AssertEquals(Core.Constants.CurrencyCodes.UnitedStates, quote.QU_RX_NKToCurrency);
				AssertEquals(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, quote.QU_RX_NKFromCurrency);
				AssertEquals(GlbCompany.CurrentCompany.PK, quote.QU_GC);
			}
		}

		public void TestCheckEPaymentExchangeRateButtonClick_HasExistingQueuedQuote()
		{
			SetupDataForTest();
			var payment = TestObjectCreator.GetNewTestAPPaymentApproval(TestObjectCreator.AALSHI, TestBank, ReceiptTypes.Cheque, "2", 200m, TestCheques);
			payment.AV_RX_NKPaymentCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			payment.AV_Status = PaymentApprovalStatus.AwaitingApproval;
			payment.TryToCreateQuote(EPaymentProviderCodes.Codes.OFX);
			Factory.Save();

			var quote = Factory.LoadTop1<AccEPaymentQuote>(new ZQuery(AccEPaymentQuoteSchema.QU_AV, payment.PK));
			AssertNotNull(quote);
			AssertEquals(QuoteStatusCodes.Queued, quote.QU_Status);

			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			using (var paymentForm = new PaymentApprovalForm(payment))
			{
				paymentForm.Show();
				Assert(paymentForm.CheckEPayRateButton_ForTestOnly.Enabled);

				UnitTestUserNotification.Instance.ClearMessages();
				paymentForm.CheckEPayRateButton_ForTestOnly.PerformClick();
				AssertEquals("Exchange rate already requested", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestCheckEPaymentExchangeRateButtonClick_HasExistingFailedQuote()
		{
			SetupDataForTest();
			var payment = TestObjectCreator.GetNewTestAPPaymentApproval(TestObjectCreator.AALSHI, TestBank, ReceiptTypes.Cheque, "2", 200m, TestCheques);
			payment.AV_RX_NKPaymentCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			payment.AV_Status = PaymentApprovalStatus.AwaitingApproval;
			var quote = payment.TryToCreateQuote(EPaymentProviderCodes.Codes.OFX).Quote;
			quote.QU_Status = QuoteStatusCodes.Failed;
			Factory.Save();

			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			using (var paymentForm = new PaymentApprovalForm(payment))
			{
				paymentForm.Show();
				Assert(paymentForm.CheckEPayRateButton_ForTestOnly.Enabled);
				paymentForm.CheckEPayRateButton_ForTestOnly.PerformClick();

				var quotes = Factory.Load<AccEPaymentQuote>(new ZQuery(AccEPaymentQuoteSchema.QU_AV, payment.PK));
				AssertEquals("Number of quotes", 2, quotes.Length);

				var oldQuote = quotes.First(q => q.PK == quote.PK);
				AssertEquals("Post-condition - old quote is discarded", QuoteStatusCodes.Discarded, oldQuote.QU_Status);

				var newQuote = quotes.First(q => q.PK != quote.PK);
				AssertNotNull("Post-condition - a new quote is created for payment", newQuote.IsInDatabase);
				AssertEquals(EPaymentProviderCodes.Codes.OFX, newQuote.QU_ProviderCode);
				AssertEquals(200m, newQuote.QU_ToAmount);
				AssertEquals(Core.Constants.CurrencyCodes.UnitedStates, newQuote.QU_RX_NKToCurrency);
				AssertEquals(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, newQuote.QU_RX_NKFromCurrency);
				AssertEquals(GlbCompany.CurrentCompany.PK, newQuote.QU_GC);
			}
		}

		public void TestCheckEPaymentExchangeRateButtonClick_HasExistingQueuedQuote_PaymentDetailsChanged()
		{
			SetupDataForTest();
			var payment = TestObjectCreator.GetNewTestAPPaymentApproval(TestObjectCreator.AALSHI, TestBank, ZArchitecture.Core.ReceiptTypes.Cheque, "2", 200m, TestCheques);
			payment.AV_RX_NKPaymentCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			payment.AV_Status = PaymentApprovalStatus.AwaitingApproval;
			var quote = payment.TryToCreateQuote(EPaymentProviderCodes.Codes.OFX).Quote;
			AssertEquals("Pre-condition - quote is queued", QuoteStatusCodes.Queued, quote.QU_Status);
			Factory.Save();

			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			using (var paymentForm = new PaymentApprovalForm(payment))
			{
				paymentForm.Show();
				payment.AV_PaymentComment = "new comment";
				payment.AV_Amount = 258M;
				Factory.Save();

				Assert(paymentForm.CheckEPayRateButton_ForTestOnly.Enabled);
				paymentForm.CheckEPayRateButton_ForTestOnly.PerformClick();

				var quotes = Factory.Load<AccEPaymentQuote>(new ZQuery(AccEPaymentQuoteSchema.QU_AV, payment.PK));
				AssertEquals("Number of quotes", 2, quotes.Length);

				var oldQuote = quotes.First(q => q.PK == quote.PK);
				AssertEquals("Post-condition - old quote is discarded", QuoteStatusCodes.Discarded, oldQuote.QU_Status);

				var newQuote = quotes.First(q => q.PK != quote.PK);
				AssertNotNull("Post-condition - a new quote is created for payment", newQuote.IsInDatabase);
				AssertEquals(EPaymentProviderCodes.Codes.OFX, newQuote.QU_ProviderCode);
				AssertEquals(258M, newQuote.QU_ToAmount);
				AssertEquals(Core.Constants.CurrencyCodes.UnitedStates, newQuote.QU_RX_NKToCurrency);
				AssertEquals(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, newQuote.QU_RX_NKFromCurrency);
				AssertEquals(GlbCompany.CurrentCompany.PK, newQuote.QU_GC);
			}
		}

		#endregion

		public void TestPaymentApprovalFormProperties()
		{
			SetupDataForTest();
			var bankAccount = TestObjectCreator.CreateEPaymentBankAccount();
			var payment = TestObjectCreator.GetNewTestAPPaymentApproval(TestObjectCreator.AALSHI, bankAccount, ReceiptTypes.Cheque, "2", 200m, TestCheques);
			Factory.Save();

			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			using (var paymentForm = new PaymentApprovalForm(payment))
			{
				paymentForm.Show();
				ZTabControl bankAndEPaymentTabControl = (ZTabControl)paymentForm.Controls.Find("BankAndEPaymentTabControl", true).First();
				var ePaymentTab = bankAndEPaymentTabControl.GetTabPage("EPaymentTabPage");
				AssertNotNull(ePaymentTab);
				// Asserts to make sure Payment form is visible properly on different screen resolutions with minimum acceptable resolution being 1366 x 768
				Assert(ePaymentTab.AutoScroll);
			}
		}

		public void TestFormProperties()
		{
			SetupDataForTest();
			var payment = TestObjectCreator.GetNewTestAPPaymentApproval(TestObjectCreator.AALSHI, TestBank, ZArchitecture.Core.ReceiptTypes.Cheque, "2", 200m, TestCheques);
			payment.AV_RX_NKPaymentCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			payment.AV_Status = PaymentApprovalStatus.AwaitingApproval;
			Factory.Save();

			using (var paymentForm = new PaymentApprovalForm(payment))
			{
				paymentForm.Show();
				AssertEquals("Refresh", paymentForm.RefreshEPaymentButton_ForTestOnly.CaptionResourceString.Caption);
			}
		}

		public void TestCheckEPaymentExchangeRateButtonClick_HasExistingRequestedQuote()
		{
			SetupDataForTest();
			var payment = TestObjectCreator.GetNewTestAPPaymentApproval(TestObjectCreator.AALSHI, TestBank, ReceiptTypes.Cheque, "2", 200m, TestCheques);
			payment.AV_RX_NKPaymentCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			payment.AV_Status = PaymentApprovalStatus.AwaitingApproval;
			var rquote = payment.TryToCreateQuote(EPaymentProviderCodes.Codes.OFX).Quote;
			rquote.QU_Status = QuoteStatusCodes.Requested;
			Factory.Save();

			var quote = Factory.LoadTop1<AccEPaymentQuote>(new ZQuery(AccEPaymentQuoteSchema.QU_AV, payment.PK));
			AssertNotNull(quote);
			AssertEquals(QuoteStatusCodes.Requested, quote.QU_Status);

			using (var paymentForm = new PaymentApprovalForm(payment))
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

				payment.PaymentQuotes.Reload(true);
				AssertEquals(2, payment.PaymentQuotes.Count);
				AssertEquals(1, payment.PaymentQuotes.Cast<EPaymentQuote>().Count(x => x.QU_Status == QuoteStatusCodes.Queued));
				AssertEquals(1, payment.PaymentQuotes.Cast<EPaymentQuote>().Count(x => x.QU_Status == QuoteStatusCodes.Discarded));
			}
		}

		public void TestPaymentIsCancelled_ActionButtonsDisabled()
		{
			SetupDataForTest();
			var payment = TestObjectCreator.GetNewTestAPPaymentApproval(TestObjectCreator.AALSHI, TestBank, ZArchitecture.Core.ReceiptTypes.Cheque, "2", 200m, TestCheques);
			payment.AV_RX_NKPaymentCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			payment.AV_Status = PaymentApprovalStatus.Cancelled;
			Factory.Save();

			using (var paymentForm = new PaymentApprovalForm(payment))
			{
				paymentForm.Show();
				Assert(!paymentForm.ProcessEPaymentButton_ForTestOnly.Enabled);
				Assert(!paymentForm.CheckEPayRateButton_ForTestOnly.Enabled);
				Assert(!paymentForm.AcceptQuoteButton_ForTestOnly.Enabled);
			}
		}

		public void TestQuoteWarningMessage()
		{
			SetupDataForTest();
			var payment = TestObjectCreator.GetNewTestAPPaymentApproval(TestObjectCreator.AALSHI, TestBank, ReceiptTypes.Cheque, "2", 200m, TestCheques);
			payment.AV_RX_NKPaymentCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			payment.AV_Status = PaymentApprovalStatus.AwaitingApproval;
			Factory.Save();
			var quote1 = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Received, payment);
			quote1.QU_ProviderReference = string.Empty;
			var quote2 = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Received, payment);
			Factory.Save();
			payment.PaymentQuotes.Reload(true);

			using (var paymentForm = new PaymentApprovalForm(payment))
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

		public void TestRefreshEPaymentButtonClick()
		{
			SetupDataForTest();
			var payment = TestObjectCreator.GetNewTestAPPaymentApproval(TestObjectCreator.AALSHI, TestBank, ZArchitecture.Core.ReceiptTypes.Cheque, "2", 200m, TestCheques);
			payment.AV_RX_NKPaymentCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			payment.AV_Status = PaymentApprovalStatus.AwaitingApproval;
			payment.TryToCreateQuote(EPaymentProviderCodes.Codes.OFX);
			Factory.Save();

			AssertEquals("Preconditon - 1 quote in DB", 1, payment.PaymentQuotes.Count);
			var quotesInDB = Factory.Load<AccEPaymentQuote>(new ZQuery(AccEPaymentQuoteSchema.QU_AV, payment.PK));
			AssertEquals(1, quotesInDB.Length);
			AssertEquals("00001000", quotesInDB[0].QU_InternalReference);

			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			using (var paymentForm = new PaymentApprovalForm(payment))
			{
				paymentForm.Show();
				Application.DoEvents();

				var bankAndEPaymentTabControl = paymentForm.Controls.Find("BankAndEPaymentTabControl", true).First() as ZTabControl;
				AssertNotNull(bankAndEPaymentTabControl);
				bankAndEPaymentTabControl.SelectNextTabPage();

				var epaymentGrid = paymentForm.Controls.Find("EPaymentGrid", true).First() as ZGrid;
				AssertNotNull(epaymentGrid);
				epaymentGrid.SelectAllElements();
				AssertEquals(1, epaymentGrid.SelectedElements.Length);

				var quoteInGrid = epaymentGrid.SelectedElements[0] as EPaymentQuoteForDisplay;
				AssertEquals("00001000", quoteInGrid.RealQuote.QU_InternalReference);

				quotesInDB[0].QU_Status = QuoteStatusCodes.Failed;
				Factory.Save();

				payment.TryToCreateQuote(EPaymentProviderCodes.Codes.OFX);
				Factory.Save();
				quotesInDB = Factory.Load<AccEPaymentQuote>(new ZQuery(AccEPaymentQuoteSchema.QU_AV, payment.PK));
				AssertEquals(2, quotesInDB.Length);
				AssertContainsExactElementsInAnyOrder(new[] { "00001000", "00001001" }, quotesInDB.Select(x => x.QU_InternalReference));

				paymentForm.RefreshEPaymentButton_ForTestOnly.PerformClick();

				epaymentGrid.UnSelectAll();
				epaymentGrid.SelectAllElements();
				AssertEquals(2, epaymentGrid.SelectedElements.Length);

				AssertContainsExactElementsInAnyOrder(new[] { "00001000", "00001001" }, epaymentGrid.SelectedElements.Cast<EPaymentQuoteForDisplay>().Select(x => x.RealQuote.QU_InternalReference));
			}
		}

		public void TestPaymentIsSavedAsDraft_UserClicksMatchAndCloseButton_ShouldPost_ForNonEPAPayment()
		{
			SetupDataForTest();
			var shipment = TestObjectCreator.CreateShipment("S1");
			var job = TestObjectCreator.CreateJob(shipment);
			var testAPInv = Factory.NewWithValidTestData<APInvoice>();
			testAPInv.AH_OH = TestOrg.PK;
			var genericChargeCodeGuid = TestObjectCreator.CC1.PK;

			var line = TestObjectCreator.CreateInvoiceLine(testAPInv, testAPInv.TransactionCurrency, testAPInv.AH_ExchangeRate, 200m, 0m, 0m, 200m, 0m, 0m, genericChargeCodeGuid);
			line.AL_JH = job.PK;
			var charge = TestObjectCreator.CreateJobCharge(line, job, TestObjectCreator.CC1, testAPInv.TransactionCurrency);
			testAPInv.AH_FullyPaidDate = ZDateTime.Empty;
			Factory.Save();

			var payment = GetNewTestAPPaymentApprovalThatPostsOnSaving(TestOrg, TestBank, ReceiptTypes.Cheque, TestCheques, "2", 200m);
			Assert(!payment.IsInDatabase);
			AssertNotEquals(PaymentApprovalStatus.Draft, payment.AV_Status);

			using (var paymentForm = new MockPaymentApprovalForm(payment))
			{
				NewMatchGroupForm matchingForm = null;
				PaymentDocumentsPrintPopup exposedPrintForm = null;

				try
				{
					paymentForm.Show();
					UnitTestUserNotification.Instance.ClearMessages();
					paymentForm.SaveAsDraftButton_ForTestOnly.PerformClick();
					AssertEquals(@"Save as Draft successful.
If payment is closed without posting, it can be found in the Payment Processing module.", UnitTestUserNotification.Instance.LastMessage.Text);
					Assert(payment.IsInDatabase);
					AssertEquals(PaymentApprovalStatus.Draft, payment.AV_Status);

					paymentForm.PaymentDetailButton_ForTestOnly.PerformClick();

					matchingForm = ZFormModaliser.ActiveForm as NewMatchGroupForm;
					AssertNotNull("The active form should be the matching form", matchingForm);
					var matchingBizO = matchingForm.BusinessEntity as APPaymentApprovalMatching;

					matchingBizO.MoveAllFromUnmatchToMatch();
					AssertNotNull("Matching Object should be APMatchingBase", matchingBizO);
					AssertEquals("Should be 2 transactions in MatchedTransactions", 2, matchingBizO.MatchedTransactions.Count);
					AssertEquals("Balance should be 0", 0M, matchingBizO.Balance);

					var emptyFactory = new BusinessObjectFactory();
					var approvalItems = new PaymentApprovalItemCollection(emptyFactory);
					approvalItems.Load();
					AssertEquals("No Approval Items should be posted to DB yet since posting happens when MatchingForm is closed", 0, approvalItems.Count);

					matchingForm.MatchAndCloseButton.PerformClick();

					AssertEquals(PaymentApprovalStatus.Posted, payment.AV_Status);
					AssertEquals("DisplayMode of Original PaymentApprovalForm should be Browse", ODisplayMode.Browse, paymentForm.DisplayMode);
					Assert(paymentForm.IsDisposed);

					approvalItems.Load();
					AssertEquals("1 PaymentApprovalItem should be posted to DB when matching form is closed", 1, approvalItems.Count);
					var invMatchFilter = new ZQuery(AccPaymentApprovalItemSchema.A2_AH, testAPInv.PK);
					var invApproval = emptyFactory.LoadTop1<AccPaymentApprovalItem>(invMatchFilter);
					AssertNotNull("There should be a PaymentApprovalItem  for the invoice", invApproval);

					exposedPrintForm = ((PaymentPrintManager.TestPaymentPrintManager)paymentForm.PrintManager_ForTestOnly).LastPrintForm;
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
					FindAndDisposeOpenPaymentForm();
				}
			}
		}

		public void TestPaymentIsSavedAsDraft_UserClicksPostWithoutMatchingButton()
		{
			SetupDataForTest();
			var payment = TestObjectCreator.GetNewTestAPPaymentApproval(TestObjectCreator.AALSHI, TestBank, ReceiptTypes.Cheque, "2", 200m, TestCheques);
			payment.AV_RX_NKPaymentCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			Assert(!payment.IsInDatabase);
			AssertNotEquals(PaymentApprovalStatus.Draft, payment.AV_Status);

			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			using (var paymentForm = new PaymentApprovalForm(payment))
			{
				paymentForm.Show();

				UnitTestUserNotification.Instance.ClearMessages();
				paymentForm.SaveAsDraftButton_ForTestOnly.PerformClick();

				AssertEquals(@"Save as Draft successful.
If payment is closed without posting, it can be found in the Payment Processing module.", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(payment.IsInDatabase);
				AssertEquals(PaymentApprovalStatus.Draft, payment.AV_Status);

				UnitTestUserNotification.Instance.ClearMessages();
				payment.SetIsAllowedToPost_ForTestOnly(true);
				paymentForm.PostWithoutMatchingButton_ForTestOnly.PerformClick();

				AssertEquals(PaymentApprovalStatus.Posted, payment.AV_Status);
				Assert(paymentForm.IsDisposed);

				var openedForms = ZApplication.GetOpenForms();
				var newPaymentForm = openedForms.FirstOrDefault(x => x.Name.Contains("APPaymentForm"));
				AssertNotNull(newPaymentForm);
				AssertEquals(ODisplayMode.Browse, ((ZForm)newPaymentForm).DisplayMode);
				AssertNotEquals(payment.PK, ((APPaymentForm)newPaymentForm).Payment_ForTestOnly.PK);
				newPaymentForm.Dispose();
			}
		}

		public void TestPaymentIsSavedAsDraft_UserClicksPaymentDetailButton()
		{
			SetupDataForTest();
			var payment = TestObjectCreator.GetNewTestAPPaymentApproval(TestObjectCreator.AALSHI, TestBank, ReceiptTypes.Cheque, "2", 200m, TestCheques);
			payment.AV_RX_NKPaymentCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			Assert(!payment.IsInDatabase);
			AssertNotEquals(PaymentApprovalStatus.Draft, payment.AV_Status);

			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			using (var paymentForm = new PaymentApprovalForm(payment))
			{
				paymentForm.Show();

				UnitTestUserNotification.Instance.ClearMessages();
				paymentForm.SaveAsDraftButton_ForTestOnly.PerformClick();

				AssertEquals(@"Save as Draft successful.
If payment is closed without posting, it can be found in the Payment Processing module.", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(payment.IsInDatabase);
				AssertEquals(PaymentApprovalStatus.Draft, payment.AV_Status);

				paymentForm.PaymentDetailButton_ForTestOnly.PerformClick();

				var lastShownForm = ZFormModaliser.LastFormShownForTest;
				AssertNotNull("Match Group Form must be opened after user clicks Payment Details button.", lastShownForm);
				AssertEquals("Match Group Form must be opened after user clicks Payment Details button.", typeof(NewMatchGroupForm), lastShownForm.GetType());
			}
		}

		public void TestQuotesAreDiscardedWhenPaymentDetailsChange()
		{
			SetupDataForTest();
			var payment = TestObjectCreator.GetNewTestAPPaymentApproval(TestObjectCreator.AALSHI, TestBank, ZArchitecture.Core.ReceiptTypes.Cheque, "2", 200m, TestCheques);
			payment.AV_RX_NKPaymentCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			payment.AV_Status = PaymentApprovalStatus.Draft;
			Factory.Save();

			var failedQuote = payment.TryToCreateQuote(EPaymentProviderCodes.Codes.OFX).Quote;
			failedQuote.QU_Status = QuoteStatusCodes.Failed;
			Factory.Save();

			var errorQuote = payment.TryToCreateQuote(EPaymentProviderCodes.Codes.OFX).Quote;
			errorQuote.QU_Status = QuoteStatusCodes.Error;
			errorQuote.QU_LastResponseReceivedUtc = ZDateTime.Now;
			Factory.Save();

			var acceptedQuote = payment.TryToCreateQuote(EPaymentProviderCodes.Codes.OFX).Quote;
			acceptedQuote.QU_Status = QuoteStatusCodes.Accepted;
			acceptedQuote.QU_LastResponseReceivedUtc = ZDateTime.Now;
			acceptedQuote.QU_ExchangeRate = 2.0M;
			acceptedQuote.QU_ExchangeRateInverted = 0.5M;
			acceptedQuote.QU_FeeAmount = 20M;
			acceptedQuote.QU_RX_NKFeeCurrency = "AUD";
			acceptedQuote.QU_FromAmount = 200M;
			acceptedQuote.QU_ProviderReference = "PRF001";
			Factory.Save();

			var expiredQuote = payment.TryToCreateQuote(EPaymentProviderCodes.Codes.OFX).Quote;
			expiredQuote.QU_Status = QuoteStatusCodes.Expired;
			expiredQuote.QU_LastResponseReceivedUtc = ZDateTime.Now;
			expiredQuote.QU_ExchangeRate = 2.0M;
			expiredQuote.QU_ExchangeRateInverted = 0.5M;
			expiredQuote.QU_FeeAmount = 20M;
			expiredQuote.QU_RX_NKFeeCurrency = "AUD";
			expiredQuote.QU_FromAmount = 200M;
			expiredQuote.QU_ProviderReference = "PRF001";
			Factory.Save();

			var queuedQuote = payment.TryToCreateQuote(EPaymentProviderCodes.Codes.OFX).Quote;
			queuedQuote.QU_Status = QuoteStatusCodes.Queued;
			Factory.Save();

			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			using (var paymentForm = new PaymentApprovalForm(payment))
			{
				paymentForm.Show();
				Application.DoEvents();

				payment.AV_Amount = 2589M;
				paymentForm.SaveAsDraftButton_ForTestOnly.PerformClick();

				AssertEquals(QuoteStatusCodes.Discarded, failedQuote.QU_Status);
				AssertEquals(QuoteStatusCodes.Discarded, errorQuote.QU_Status);
				AssertEquals(QuoteStatusCodes.Discarded, acceptedQuote.QU_Status);
				AssertEquals(QuoteStatusCodes.Discarded, expiredQuote.QU_Status);
				AssertEquals(QuoteStatusCodes.Discarded, queuedQuote.QU_Status);
			}
		}

		#region TestAllocateChequeNumber

		public void TestAllocateChequeNumber_WhenPost_ChequeBookIsAutoPrint()
		{
			AssertAllocateChequeNumberCore(true, new Action<PaymentApprovalForm, PaymentApprovalBase>((form, paymentApproval) => {
				AssertNullOrEmpty("Per condition", paymentApproval.AV_ChequeOrReference);

				form.PostWithoutMatchingButton_ForTestOnly.PerformClick();

				var printFormToTest = ZFormModaliser.LastFormShownDialogForTest as PaymentDocumentsPrintPopup;
				AssertNotNullOrEmpty(paymentApproval.AV_ChequeOrReference);
				AssertNotNull("Should have opened Remittance Advice Printing Form", printFormToTest);
				AssertEquals(true, form.PrintManager_ForTestOnly.ChequeIsAutoPrinted);
			}));
		}

		public void TestAllocateChequeNumber_WhenPost_ChequeBookIsNotAutoPrint()
		{
			AssertAllocateChequeNumberCore(false, new Action<PaymentApprovalForm, PaymentApprovalBase>((form, paymentApproval) => {
				AssertNotNullOrEmpty("Per condition", paymentApproval.AV_ChequeOrReference);

				form.PostWithoutMatchingButton_ForTestOnly.PerformClick();

				var printFormToTest = ZFormModaliser.LastFormShownDialogForTest as PaymentDocumentsPrintPopup;
				AssertNotNullOrEmpty(paymentApproval.AV_ChequeOrReference);
				AssertNotNull("Should have opened Remittance Advice Printing Form", printFormToTest);
				AssertEquals(false, form.PrintManager_ForTestOnly.ChequeIsAutoPrinted);
			}));
		}

		public void TestAllocateChequeNumber_WhenSaveAsDraft_ChequeBookIsAutoPrint()
		{
			AssertAllocateChequeNumberCore(true, new Action<PaymentApprovalForm, PaymentApprovalBase>((form, paymentApproval) => {
				AssertNullOrEmpty("Per condition", paymentApproval.AV_ChequeOrReference);

				form.SaveAsDraftButton_ForTestOnly.PerformClick();

				AssertNullOrEmpty(paymentApproval.AV_ChequeOrReference);
				AssertNull("Should not have opened Remittance Advice Printing Form", ZFormModaliser.LastFormShownDialogForTest);
			}));
		}

		public void TestAllocateChequeNumber_WhenSaveAsDraft_ChequeBookIsNotAutoPrint()
		{
			AssertAllocateChequeNumberCore(false, new Action<PaymentApprovalForm, PaymentApprovalBase>((form, paymentApproval) => {
				AssertNotNullOrEmpty("Per condition", paymentApproval.AV_ChequeOrReference);

				form.SaveAsDraftButton_ForTestOnly.PerformClick();

				AssertNotNullOrEmpty("Per condition", paymentApproval.AV_ChequeOrReference);
				AssertNull("Should not have opened Remittance Advice Printing Form", ZFormModaliser.LastFormShownDialogForTest);
			}));
		}

		void AssertAllocateChequeNumberCore(bool chequeBookIsAutoPrint, Action<PaymentApprovalForm, PaymentApprovalBase> action)
		{
			ZFormModaliser.ShowDialogsInTest = true;
			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper();
			testHelper.SetupPeriods();

			var bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			var chequeBook = GetChequeBook(bankAccount, 1, 3, 1, chequeBookIsAutoPrint);
			var aPPaymentApproval = Factory.New<APPaymentApprovalWithoutAuthorisation>();
			aPPaymentApproval.AV_OH = TestOrg.PK;
			aPPaymentApproval.AV_AB = bankAccount.PK;
			aPPaymentApproval.AV_PaymentType = ReceiptTypes.Cheque;
			aPPaymentApproval.AV_AK = chequeBook.PK;
			aPPaymentApproval.AV_Amount = 94m;

			AssertEquals("TestPayment should not have any errors before saving", false, aPPaymentApproval.HasErrors);

			using (var newForm = new PaymentApprovalForm(aPPaymentApproval))
			{
				newForm.Show();
				newForm.IsPostWithoutMatching_ForTestOnly = true;

				action(newForm, aPPaymentApproval);

				FindAndDisposeOpenPaymentForm();
			}
		}

		#endregion

		#region Draft Payment Approval

		public void TestSaveAsDraftButtonWillShow()
		{
			SetupDataForTest();
			var approval = GetNewTestAPPaymentApprovalThatPostsOnSaving(TestOrg, TestBank, ReceiptTypes.Cheque,	TestCheques, "2", 90M);
			using (PaymentApprovalForm form = new PaymentApprovalForm(approval))
			{
				form.Show();
				Application.DoEvents();

				AssertEquals(false, approval.IsInDatabase);
				AssertEquals(true, form.SaveAsDraftButton_ForTestOnly.Visible);
				AssertEquals(true, form.SaveAsDraftButton_ForTestOnly.Enabled);
			}
		}

		public void TestSaveAsDraftSuccessfully_OnPaymentApprovalForm()
		{
			SetupDataForTest();
			var approval = GetNewTestAPPaymentApprovalThatPostsOnSaving(TestOrg, TestBank, ReceiptTypes.Cheque,	TestCheques, "2", 90M);

			using (PaymentApprovalForm form = new PaymentApprovalForm(approval))
			{
				form.Show();
				Application.DoEvents();

				Assert(!approval.IsInDatabase);
				AssertNotEquals(approval.AV_Status, PaymentApprovalStatus.Draft);

				UnitTestUserNotification.Instance.ClearMessages();
				//Click Save As Draft, save Draft Successuflly, and form is not closed
				form.SaveAsDraftButton_ForTestOnly.PerformClick();
				Application.DoEvents();

				AssertEquals(false, form.IsDisposed);
				AssertEquals(@"Save as Draft successful.
If payment is closed without posting, it can be found in the Payment Processing module.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(true, approval.IsInDatabase);
				AssertEquals(true, approval.IsDraft);
				AssertEquals("00001000", approval.AV_PaymentApprovalReference);
				AssertNull(approval.NewPayment);
				Assert(!approval.ReadOnly);
			}
		}

		public void TestSaveAsDraftSuccessfully_OnMatchForm()
		{
			SetupDataForTest();
			var approval = GetNewTestAPPaymentApprovalThatPostsOnSaving(TestOrg, TestBank, ReceiptTypes.Cheque,	TestCheques, "2", 100M);
			using (PaymentApprovalForm form = new PaymentApprovalForm(approval))
			{
				form.Show();
				Application.DoEvents();

				Assert(!approval.IsInDatabase);
				AssertNotEquals(approval.AV_Status, PaymentApprovalStatus.Draft);

				form.PaymentDetailButton_ForTestOnly.PerformClick();
				var matchingForm = ZFormModaliser.ActiveForm as NewMatchGroupForm;
				var matchingBizO = matchingForm.BusinessEntity as APPaymentApprovalMatching;
				AssertNotNull(matchingForm);

				var saveAsDraftButton = matchingForm.Controls.Find("SaveAsDraftButton", true)[0] as ZButton;
				AssertNotNull(saveAsDraftButton);
				Assert(saveAsDraftButton.Visible);
				Assert(saveAsDraftButton.Enabled);

				//Click Save As Draft, save Draft Successuflly, and all form closed
				saveAsDraftButton.PerformClick();
				AssertEquals(true, form.IsDisposed);
				AssertEquals(true, matchingForm.IsDisposed);
				AssertEquals(true, approval.IsInDatabase);
				AssertEquals(true, approval.IsDraft);
				AssertEquals("00001000", approval.AV_PaymentApprovalReference);
				AssertNull(approval.NewPayment);
			}
		}

		public void TestSaveAsDraftSuccessfully_OnMatchForm_WithMatchedTransaction()
		{
			SetupDataForTest();

			var shipment = TestObjectCreator.CreateShipment("S1");
			var job = TestObjectCreator.CreateJob(shipment);

			var testAPInv = Factory.NewWithValidTestData<APInvoice>();
			testAPInv.AH_OH = TestOrg.PK;
			var line = TestObjectCreator.CreateInvoiceLine(testAPInv, testAPInv.TransactionCurrency, testAPInv.AH_ExchangeRate, 94m, 0m, 0m, 94m, 0m, 0m, TestObjectCreator.CC1.PK);
			line.AL_JH = job.PK;
			var charge = TestObjectCreator.CreateJobCharge(line, job, TestObjectCreator.CC1, testAPInv.TransactionCurrency);
			testAPInv.AH_FullyPaidDate = ZDateTime.Empty;
			Factory.Save();

			var approval = GetNewTestAPPaymentApprovalThatPostsOnSaving(TestOrg, TestBank, ReceiptTypes.Cheque,	TestCheques, "2", 94m);
			using (PaymentApprovalForm form = new PaymentApprovalForm(approval))
			{
				form.Show();
				Application.DoEvents();

				AssertEquals(true, form.SaveAsDraftButton_ForTestOnly.Visible);
				AssertEquals(true, form.SaveAsDraftButton_ForTestOnly.Enabled);

				form.PaymentDetailButton_ForTestOnly.PerformClick();
				var matchingForm = ZFormModaliser.ActiveForm as NewMatchGroupForm;
				var matchingBizO = matchingForm.BusinessEntity as APPaymentApprovalMatching;
				matchingBizO.MoveAllFromUnmatchToMatch();
				AssertEquals("Should be 2 transactions in MatchedTransactions", 2, matchingBizO.MatchedTransactions.Count);
				AssertEquals("Balance should be 0", 0M, matchingBizO.Balance);

				matchingForm.SaveAsDraftButton.PerformClick();
				var matchingBaseObject = approval.MatchingBaseObject as PaymentApprovalMatchingBase;
				AssertEquals(true, approval.IsInDatabase);
				AssertEquals(true, approval.IsDraft);
				AssertEquals(true, matchingForm.IsDisposed);
				AssertEquals(true, form.IsDisposed);
				AssertEquals("00001000", approval.AV_PaymentApprovalReference);
				AssertNull(approval.NewPayment);

				AssertEquals(1, matchingBaseObject.PaymentApprovalItems.Count);
				AssertEquals(true, matchingBaseObject.PaymentApprovalItems[0].IsInDatabase);
				AssertEquals(-94m, matchingBaseObject.PaymentApprovalItems[0].A2_PaymentThisRun);
				AssertEquals(testAPInv.PK, matchingBaseObject.PaymentApprovalItems[0].A2_AH);
				AssertEquals("OutstandingAmount not changed", -94m, testAPInv.AH_OutstandingAmount);

				AssertEquals(2, matchingBaseObject.MatchedTransactions.Count);
				AssertEquals(0, matchingBaseObject.MatchLinks.Count);

				AssertEquals(true, matchingForm.IsDisposed);
				AssertEquals(true, form.IsDisposed);

				AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSaveAsDraftWillNotShowErrorAboutBalance()
		{
			SetupDataForTest();
			var approval = GetNewTestAPPaymentApprovalThatPostsOnSaving(TestOrg, TestBank, ReceiptTypes.Cheque,	TestCheques, "2", 100M);
			using (PaymentApprovalForm form = new PaymentApprovalForm(approval))
			{
				form.Show();
				Application.DoEvents();

				form.PaymentDetailButton_ForTestOnly.PerformClick();
				AssertType<NewMatchGroupForm>(ZFormModaliser.LastFormShownForTest);
				AssertNotEquals("Pre-condition: The balance should not equal 0.", 0m, approval.MatchingBaseObject.Balance);

				var newMatchGroupForm = ZFormModaliser.LastFormShownForTest as NewMatchGroupForm;
				var saveAsDraftButton = newMatchGroupForm.Controls.Find("SaveAsDraftButton", true)[0] as ZButton;
				saveAsDraftButton.PerformClick();
				AssertEquals("Should not show any message as saved successfully.", null, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("The status should be 'DFT' as save as draft successfully.", true, approval.IsDraft);
				AssertEquals("Should save successfully even the balance is NOT zero as save as draft.", true, approval.IsInDatabase);
			}
		}

		public void TestSaveAsDraftShowErrorAboutTransactions()
		{
			var exceptedMessage = @"Matching containing Bank Fee, AR Journal or AP Journal cannot be saved in Draft mode. To save as Draft, please remove these transactions.";

			SetupDataForTest();
			var approval = GetNewTestAPPaymentApprovalThatPostsOnSaving(TestOrg, TestBank, ReceiptTypes.Cheque,	TestCheques, "2", 100M);
			approval.MatchingBaseObject.AddMiscellaneousTransaction(Factory.NewWithValidTestData<ARJournal>());
			AssertNotNull(approval.MatchingBaseObject.BankFeeCurrent);
			AssertEquals(true, approval.HasNonDraftTransactions_ForTestOnly);

			using (PaymentApprovalForm form = new PaymentApprovalForm(approval))
			{
				form.Show();
				Application.DoEvents();

				form.SaveAsDraftButton_ForTestOnly.PerformClick();
				AssertEquals(exceptedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessages();

				form.PaymentDetailButton_ForTestOnly.PerformClick();
				AssertType<NewMatchGroupForm>(ZFormModaliser.LastFormShownForTest);
				var newMatchGroupForm = ZFormModaliser.LastFormShownForTest as NewMatchGroupForm;
				var saveAsDraftButton = newMatchGroupForm.Controls.Find("SaveAsDraftButton", true)[0] as ZButton;
				saveAsDraftButton.PerformClick();
				AssertEquals(exceptedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestJournalNotSavedWhenPostPaymentAfterUndoMatchingForm()
		{
			SetupDataForTest();

			var paymentApproval = GetNewTestAPPaymentApprovalThatPostsOnSaving(TestOrg, TestBank, ReceiptTypes.Cheque, TestCheques, "2", 100M);

			using (var payApproveForm = new MockPaymentApprovalForm(paymentApproval))
			{
				try
				{
					payApproveForm.Show();
					Application.DoEvents();

					payApproveForm.PaymentDetailButton_ForTestOnly.PerformClick();
					var matchingForm = ZFormModaliser.ActiveForm as NewMatchGroupForm;

					AssertNotNull("The active form should be the matching form", matchingForm);

					var journal1 = Factory.NewWithValidTestData<APJournal>();
					journal1.AH_OH = TestOrg.PK;
					journal1.AH_RX_NKTransactionCurrency = "EUR";
					journal1.AH_ExchangeRate = 0.5M;
					journal1.AH_OSTotal = 400M;
					journal1.AH_InvoiceAmount = 800M;
					journal1.AH_OutstandingAmount = 800M;
					journal1.AH_AG = TestObjectCreator.CreateAPControlAccount().PK;

					var matchingBase = matchingForm.FMatchingBase_ForTestOnly;
					var copiedJournal = matchingBase.CopyJournalWithOppositeAmount(journal1);
					matchingBase.AddToBalancingJournals(copiedJournal);

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					matchingForm.CloseButton.PerformClick();

					payApproveForm.PostWithoutMatchingButton_ForTestOnly.PerformClick();

					AssertEquals(true, journal1.IsDeleted);
					AssertEquals(true, copiedJournal.IsDeleted);

					var query = new ZQuery().AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Journal);
					AssertEquals(0, new BusinessObjectFactory().Load<APJournal>(query).Length);
				}
				finally
				{
					ZFormModaliser.LastFormShownDialogForTest?.Dispose();
					FindAndDisposeOpenPaymentForm();
				}
			}
		}

		public void TestPaymentDetailButton()
		{
			SetupDataForTest();

			var paymentApproval = GetNewTestAPPaymentApprovalThatPostsOnSaving(TestOrg, TestBank, ReceiptTypes.Cheque, TestCheques, "2", 100M);
			paymentApproval.AV_AK = ZGuid.Empty;
			paymentApproval.AV_ChequeOrReference = string.Empty;

			using (MockPaymentApprovalForm payApproveForm = new MockPaymentApprovalForm(paymentApproval))
			{
				NewMatchGroupForm matchingForm = null;
				try
				{
					payApproveForm.Show();
					Application.DoEvents();

					AssertHasError("Percondition", paymentApproval.AV_AKInfo, "Please enter a Check Book.");
					AssertHasError("Percondition", paymentApproval.AV_ChequeOrReferenceInfo, "Please enter a Check / Reference.");

					//Click Payment Detail Button will skip validate some properties, and show NewMatchGroupForm
					payApproveForm.PaymentDetailButton_ForTestOnly.PerformClick();
					matchingForm = ZFormModaliser.ActiveForm as NewMatchGroupForm;
					AssertNotNull("The active form should be the matching form", matchingForm);
					AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);
					AssertNoErrors(paymentApproval.AV_AKInfo);
					AssertNoErrors(paymentApproval.AV_ChequeOrReferenceInfo);

					//Click Match and close on match Form, will validate those skipped properties
					matchingForm.MatchAndCloseButton.PerformClick();
					AssertEquals("There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals(true, matchingForm.BusinessEntity.Notifications.Contains("Error - Balance: The balance must equal 0"));
					AssertEquals(true, matchingForm.BusinessEntity.Notifications.Contains("Error - AV_AK: Please enter a Check Book."));
					AssertEquals(true, matchingForm.BusinessEntity.Notifications.Contains("Error - AV_ChequeOrReference: Please enter a Check / Reference."));

					//Click Save As Draft match Form, will close all froms and saved successfully
					UnitTestUserNotification.Instance.ClearMessages();
					matchingForm.SaveAsDraftButton.PerformClick();
					AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals(true, matchingForm.IsDisposed);
					AssertEquals(true, payApproveForm.IsDisposed);
					AssertEquals(true, paymentApproval.IsInDatabase);
					AssertEquals(true, paymentApproval.IsDraft);
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

		public void TestClickSaveAsDraftButton_IsPostWithoutMatching()
		{
			SetupDataForTest();
			var testAPPaymentApproval = GetNewTestAPPaymentApprovalThatPostsOnSaving(TestOrg, TestBank, ReceiptTypes.Cheque, TestCheques, "2", 0M);
			using (MockPaymentApprovalForm payApproveForm = new MockPaymentApprovalForm(testAPPaymentApproval))
			{
				payApproveForm.Show();

				payApproveForm.PostWithoutMatchingButton_ForTestOnly.PerformClick();
				AssertEquals(true, testAPPaymentApproval.IsPostWithoutMatching);
				AssertEquals("There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);

				payApproveForm.SaveAsDraftButton_ForTestOnly.PerformClick();
				AssertEquals(false, testAPPaymentApproval.IsPostWithoutMatching);
			}
		}

		#endregion

		#region Submit For Approval

		public void TestSubmitForApproval_ForEPAPayment()
		{
			SetupDataForTest();

			var shipment = TestObjectCreator.CreateShipment("S1");
			var job = TestObjectCreator.CreateJob(shipment);

			var testAPInv = Factory.NewWithValidTestData<APInvoice>();
			testAPInv.AH_OH = TestOrg.PK;
			var genericChargeCodeGuid = TestObjectCreator.CC1.PK;

			var line = TestObjectCreator.CreateInvoiceLine(testAPInv, testAPInv.TransactionCurrency, testAPInv.AH_ExchangeRate, 3000m, 0m, 0m, 3000m, 0m, 0m, genericChargeCodeGuid);
			line.AL_JH = job.PK;
			var charge = TestObjectCreator.CreateJobCharge(line, job, TestObjectCreator.CC1, testAPInv.TransactionCurrency);
			testAPInv.AH_FullyPaidDate = ZDateTime.Empty;

			Factory.Save();

			var ePaymentBankAccount = TestObjectCreator.CreateEPaymentBankAccount();
			var testAPPaymentApproval = TestObjectCreator.GetNewTestAPPaymentApproval(TestOrg, ePaymentBankAccount, ReceiptTypes.EPayment, "2", 3000m, "USD", 1, PaymentApprovalStatus.FullyApproved);
			testAPPaymentApproval.IsAllowedToPost = true;
			var deal = TestObjectCreator.CreateValidEPaymentDealForStatus(DealStatusCodes.Accepted, testAPPaymentApproval);
			deal.Quote.QU_FromAmount = 3000m;

			using (AccountingConfigurationRegistry.Instance.PayableAuthorizationSettings.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, TestObjectCreator.CreatePaymentAuthorisationSettingsExample()))
			{
				using (var payApproveForm = new MockPaymentApprovalForm(testAPPaymentApproval))
				{
					try
					{
						payApproveForm.Show();

						AssertEquals(true, payApproveForm.Visible);
						AssertNotEquals(PaymentApprovalStatus.Draft, testAPPaymentApproval.AV_Status);

						var submitForApproval = payApproveForm.Controls.Find("SubmitForApprovalButton", true)[0] as ZButton;
						AssertNotNull(submitForApproval);
						AssertEquals(false, submitForApproval.Visible);

						var approveForPostingButton = payApproveForm.Controls.Find("ApproveForPostingButton", true)[0] as ZButton;
						AssertNotNull(approveForPostingButton);
						AssertEquals(false, approveForPostingButton.Visible);

						payApproveForm.SaveAsDraftButton_ForTestOnly.PerformClick();
						AssertEquals(PaymentApprovalStatus.Draft, testAPPaymentApproval.AV_Status);

						AssertEquals(true, submitForApproval.Visible);
						AssertEquals(false, approveForPostingButton.Visible);

						//Try to submit for approval
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
						payApproveForm.SubmitForApprovalButton_ForTestOnly.PerformClick();
						AssertEquals($@"The following Payments cannot be submitted for approval:
{testAPPaymentApproval.GetDescription()}
Error - Balance: The balance must equal 0", UnitTestUserNotification.Instance.LastMessage.Text);

						payApproveForm.PaymentDetailButton_ForTestOnly.PerformClick();

						var matchingForm = ZFormModaliser.ActiveForm as NewMatchGroupForm;
						var matchingBizO = matchingForm.BusinessEntity as APPaymentApprovalMatching;
						matchingBizO.MoveAllFromUnmatchToMatch();
						matchingForm.MatchAndCloseButton.PerformClick();    // Note: this does post the payment for the EPA payment type

						payApproveForm.SaveAsDraftButton_ForTestOnly.PerformClick();
						AssertEquals(PaymentApprovalStatus.Draft, testAPPaymentApproval.AV_Status);

						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

						payApproveForm.SubmitForApprovalButton_ForTestOnly.PerformClick();

						AssertEquals($@"The following Payments will be submitted for approval:
{testAPPaymentApproval.GetDescription()}

Do you want to submit these Payments for approval?", UnitTestUserNotification.Instance.PreviousMessages[1].Text);

						AssertEquals(
							"Payment Approval saved. Authorized users can approve the payment in the Payment Processing module.",
							UnitTestUserNotification.Instance.LastMessage.Text
						);

						AssertEquals(PaymentApprovalStatus.AwaitingApproval, testAPPaymentApproval.AV_Status);
						AssertEquals(false, payApproveForm.Visible);
					}
					finally
					{
						FindAndDisposeOpenPaymentForm();
					}
				}
			}
		}

		#endregion

		#region Payment Approval Status Binding

		public void TestPaymentApprovalStatusBinding()
		{
			SetupDataForTest();

			var testAPPaymentApproval = GetNewTestAPPaymentApprovalThatPostsOnSaving(TestOrg, TestBank, ReceiptTypes.Cheque, TestCheques, "2", 94M);

			using (var payApproveForm = new MockPaymentApprovalForm(testAPPaymentApproval))
			{
				try
				{
					payApproveForm.Show();

					var statusDropEdit = payApproveForm.Controls.Find("StatusDropEdit", true)[0] as ZDropEdit;
					Assert(statusDropEdit.Visible);
					Assert(!statusDropEdit.EditableInViewMode);

					string[] approvalStatusCodes =
					{
						PaymentApprovalStatus.FullyApproved,
						PaymentApprovalStatus.Posted,
						PaymentApprovalStatus.AwaitingApproval,
						PaymentApprovalStatus.Draft,
						PaymentApprovalStatus.Cancelled,
						PaymentApprovalStatus.Rejected,
					};

					foreach (var approvalStatusCode in approvalStatusCodes)
					{
						testAPPaymentApproval.AV_Status = approvalStatusCode;
						AssertEquals(approvalStatusCode, statusDropEdit.CodeBox.Text);
					}
				}
				finally
				{
					FindAndDisposeOpenPaymentForm();
				}
			}
		}

		#endregion

		#region Approve For Posting

		public void TestApproveForPosting_ForEPAPayment()
		{
			SetupDataForTest();

			var shipment = TestObjectCreator.CreateShipment("S1");
			var job = TestObjectCreator.CreateJob(shipment);

			var testAPInv = Factory.NewWithValidTestData<APInvoice>();
			testAPInv.AH_OH = TestOrg.PK;
			var genericChargeCodeGuid = TestObjectCreator.CC1.PK;

			var line = TestObjectCreator.CreateInvoiceLine(testAPInv, testAPInv.TransactionCurrency, testAPInv.AH_ExchangeRate, 3000m, 0m, 0m, 3000m, 0m, 0m, genericChargeCodeGuid);
			line.AL_JH = job.PK;
			var charge = TestObjectCreator.CreateJobCharge(line, job, TestObjectCreator.CC1, testAPInv.TransactionCurrency);
			testAPInv.AH_FullyPaidDate = ZDateTime.Empty;

			Factory.Save();

			var ePaymentBankAccount = TestObjectCreator.CreateEPaymentBankAccount();
			var testAPPaymentApproval = TestObjectCreator.GetNewTestAPPaymentApproval(TestOrg, ePaymentBankAccount, ReceiptTypes.EPayment, "2", 3000m, "USD", 1, PaymentApprovalStatus.FullyApproved);
			testAPPaymentApproval.IsAllowedToPost = true;
			var deal = TestObjectCreator.CreateValidEPaymentDealForStatus(DealStatusCodes.Accepted, testAPPaymentApproval);
			deal.Quote.QU_FromAmount = 3000m;

			using (AccountingConfigurationRegistry.Instance.PayableAuthorizationSettings.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, TestObjectCreator.CreatePaymentAuthorisationSettingsEmpty()))
			{
				using (var payApproveForm = new MockPaymentApprovalForm(testAPPaymentApproval))
				{
					try
					{
						payApproveForm.Show();

						AssertEquals(true, payApproveForm.Visible);
						AssertNotEquals(PaymentApprovalStatus.Draft, testAPPaymentApproval.AV_Status);

						var submitForApproval = payApproveForm.Controls.Find("SubmitForApprovalButton", true)[0] as ZButton;
						AssertNotNull(submitForApproval);
						AssertEquals(false, submitForApproval.Visible);

						var approveForPostingButton = payApproveForm.Controls.Find("ApproveForPostingButton", true)[0] as ZButton;
						AssertNotNull(approveForPostingButton);
						AssertEquals(false, approveForPostingButton.Visible);

						payApproveForm.SaveAsDraftButton_ForTestOnly.PerformClick();
						AssertEquals(PaymentApprovalStatus.Draft, testAPPaymentApproval.AV_Status);

						AssertEquals(false, submitForApproval.Visible);
						AssertEquals(true, approveForPostingButton.Visible);

						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
						payApproveForm.ApproveForPostingButton_ForTestOnly.PerformClick();
						AssertEquals($@"The following Payments cannot be approved for posting:
{testAPPaymentApproval.GetDescription()}
Error - Balance: The balance must equal 0", UnitTestUserNotification.Instance.LastMessage.Text);

						payApproveForm.PaymentDetailButton_ForTestOnly.PerformClick();

						var matchingForm = ZFormModaliser.ActiveForm as NewMatchGroupForm;
						var matchingBizO = matchingForm.BusinessEntity as APPaymentApprovalMatching;
						matchingBizO.MoveAllFromUnmatchToMatch();
						matchingForm.MatchAndCloseButton.PerformClick();    // Note: this does post the payment for the EPA payment type

						payApproveForm.SaveAsDraftButton_ForTestOnly.PerformClick();
						AssertEquals(PaymentApprovalStatus.Draft, testAPPaymentApproval.AV_Status);

						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

						payApproveForm.ApproveForPostingButton_ForTestOnly.PerformClick();
						AssertEquals($@"The following Payments will be approved for posting:
{testAPPaymentApproval.GetDescription()}

Do you want to approve these Payments for posting?", UnitTestUserNotification.Instance.LastMessage.Text);
						AssertEquals(PaymentApprovalStatus.FullyApproved, testAPPaymentApproval.AV_Status);
						AssertEquals(true, payApproveForm.Visible);
					}
					finally
					{
						FindAndDisposeOpenPaymentForm();
					}
				}
			}
		}

		#endregion

		#region TestfMatchingForm_ClosingHandlesNullPayment
		[ExpectNoExceptions]
		public void TestSeeHowfMatchingForm_ClosingBlowsUp()
		{
			SetupDataForTest();

			APPaymentApprovalWithoutAuthorisation testAPPaymentApproval = GetNewTestAPPaymentApprovalThatPostsOnSaving(TestOrg, TestBank, ReceiptTypes.Cheque,
			TestCheques, "2", 0M);

			using (MockPaymentApprovalForm payApproveForm = new MockPaymentApprovalForm())
			using (var newMatchGroupForm = new NewMatchGroupForm(testAPPaymentApproval.PaymentMatchingBaseObject))
			{
				payApproveForm.FMatchingForm_ForTestOnly = newMatchGroupForm;
				testAPPaymentApproval.PaymentMatchingBaseObject.CreateTemporaryTransactions();
				payApproveForm.FMatchingForm_ForTestOnly.HideMatchAndContinueButtonForReceiptPayment();
				payApproveForm.FMatchingForm_Closed_ForTestOnly(null, null);
			}
		}
		#endregion

		#region TestAllowZeroAmountWhenShowMatchingForm

		public void TestAllowZeroAmountWhenShowMatchingForm()
		{
			SetupDataForTest();
			var testAPPaymentApproval = GetNewTestAPPaymentApprovalThatPostsOnSaving(TestOrg, TestBank, ReceiptTypes.Cheque, TestCheques, "2", 0M);
			using (MockPaymentApprovalForm payApproveForm = new MockPaymentApprovalForm(testAPPaymentApproval))
			{
				NewMatchGroupForm matchingForm = null;
				try
				{
					payApproveForm.Show();
					AssertEquals("SessionBalancesToZero_ForTestOnly", ZBool.True, payApproveForm.SessionBalancesToZero_ForTestOnly);
					payApproveForm.PaymentDetailButton_ForTestOnly.PerformClick();  // user clicks payment detail button
					AssertEquals("IsPostWithoutMatching_ForTestOnly", ZBool.False, payApproveForm.IsPostWithoutMatching_ForTestOnly);
					matchingForm = ZFormModaliser.ActiveForm as NewMatchGroupForm;
					AssertNotNull("The active form should be the matching form when Overseas amount is Zero", matchingForm);
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

		public void TestNotAllowZeroAmountWhenValidateMatchingForm()
		{
			SetupDataForTest();
			var testAPPaymentApproval = GetNewTestAPPaymentApprovalThatPostsOnSaving(TestOrg, TestBank, ReceiptTypes.Cheque, TestCheques, "2", 0M);
			using (MockPaymentApprovalForm payApproveForm = new MockPaymentApprovalForm(testAPPaymentApproval))
			{
				NewMatchGroupForm matchingForm = null;
				try
				{
					payApproveForm.Show();

					AssertEquals(ZBool.True, payApproveForm.SessionBalancesToZero_ForTestOnly);
					payApproveForm.PaymentDetailButton_ForTestOnly.PerformClick();  // user clicks payment detail button
					AssertEquals(ZBool.False, payApproveForm.IsPostWithoutMatching_ForTestOnly);

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					matchingForm = ZFormModaliser.ActiveForm as NewMatchGroupForm;
					AssertNotNull("The active form should be the matching form when Overseas amount is Zero", matchingForm);
					matchingForm.Close();
					AssertEquals(@"This record has been modified.
Would you like to save the changes?", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals(true, matchingForm.IsDisposed);

					payApproveForm.PaymentDetailButton_ForTestOnly.PerformClick();  // user clicks payment detail button again
					matchingForm = ZFormModaliser.ActiveForm as NewMatchGroupForm;

					matchingForm.MatchAndCloseButton.PerformClick();
					AssertEquals(false, matchingForm.IsDisposed);
					AssertEquals("There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals(true, matchingForm.BusinessEntity.Notifications.Contains("Error - record: Overseas amount can not be zero"));
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

		public void TestNotAllowZeroAmountWhenValidateMatchingForm_WithMatchedTransaction()
		{
			SetupDataForTest();

			var shipment = TestObjectCreator.CreateShipment("S1");
			var job = TestObjectCreator.CreateJob(shipment);

			var testAPInv = Factory.NewWithValidTestData<APInvoice>();
			testAPInv.AH_OH = TestOrg.PK;
			var line = TestObjectCreator.CreateInvoiceLine(testAPInv, testAPInv.TransactionCurrency, testAPInv.AH_ExchangeRate, 94m, 0m, 0m, 94m, 0m, 0m, TestObjectCreator.CC1.PK);
			line.AL_JH = job.PK;
			var charge = TestObjectCreator.CreateJobCharge(line, job, TestObjectCreator.CC1, testAPInv.TransactionCurrency);
			testAPInv.AH_FullyPaidDate = ZDateTime.Empty;
			Factory.Save();

			var testAPPaymentApproval = GetNewTestAPPaymentApprovalThatPostsOnSaving(TestOrg, TestBank, ReceiptTypes.Cheque, TestCheques, "2", 0m);
			using (PaymentApprovalForm payApproveForm = new PaymentApprovalForm(testAPPaymentApproval))
			{
				NewMatchGroupForm matchingForm = null;
				try
				{
					payApproveForm.Show();

					AssertEquals(ZBool.True, payApproveForm.SessionBalancesToZero_ForTestOnly);
					payApproveForm.PaymentDetailButton_ForTestOnly.PerformClick();  // user clicks payment detail button
					AssertEquals(ZBool.False, payApproveForm.IsPostWithoutMatching_ForTestOnly);

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					matchingForm = ZFormModaliser.ActiveForm as NewMatchGroupForm;
					AssertNotNull("The active form should be the matching form when Overseas amount is Zero", matchingForm);
					matchingForm.FMatchingBase_ForTestOnly.MoveAllFromUnmatchToMatch();
					matchingForm.Close();
					AssertEquals(@"This record has been modified.
Would you like to save the changes?", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals(true, matchingForm.IsDisposed);

					payApproveForm.PaymentDetailButton_ForTestOnly.PerformClick();  // user clicks payment detail button again
					matchingForm = ZFormModaliser.ActiveForm as NewMatchGroupForm;

					matchingForm.MatchAndCloseButton.PerformClick();
					AssertEquals(false, matchingForm.IsDisposed);
					AssertEquals("There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals(true, matchingForm.BusinessEntity.Notifications.Contains("Error - record: Overseas amount can not be zero"));
					AssertEquals(true, matchingForm.BusinessEntity.Notifications.Contains("Error - Balance: The balance must equal 0"));
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

		#endregion

		#region TestMatchingFormPostSuccessful

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
			var charge = TestObjectCreator.CreateJobCharge(line, job, TestObjectCreator.CC1, testAPInv.TransactionCurrency);
			testAPInv.AH_FullyPaidDate = ZDateTime.Empty;

			Factory.Save();

			APPaymentApprovalWithoutAuthorisation testAPPaymentApproval = GetNewTestAPPaymentApprovalThatPostsOnSaving(TestOrg, TestBank, ReceiptTypes.Cheque,
				TestCheques, "2", 94M);

			using (MockPaymentApprovalForm payApproveForm = new MockPaymentApprovalForm(testAPPaymentApproval))
			{
				NewMatchGroupForm matchingForm = null;
				PaymentDocumentsPrintPopup exposedPrintForm = null;

				try
				{
					payApproveForm.Show();
					payApproveForm.PaymentDetailButton_ForTestOnly.PerformClick();  // user clicks payment detail button

					matchingForm = ZFormModaliser.ActiveForm as NewMatchGroupForm;
					AssertNotNull("The active form should be the matching form", matchingForm);
					APPaymentApprovalMatching matchingBizO = matchingForm.BusinessEntity as APPaymentApprovalMatching;
					Assert("OSPartialPaymentAmount should be readonly", ((IMatching)testAPPaymentApproval).OSPartialPaymentAmountInfo.ReadOnly);

					matchingBizO.MoveAllFromUnmatchToMatch();

					AssertNotNull("Matching Object should be APMatchingBase", matchingBizO);
					AssertEquals("Should be 2 transactions in MatchedTransactions", 2, matchingBizO.MatchedTransactions.Count);
					AssertEquals("Balance should be 0", 0M, matchingBizO.Balance);

					BusinessObjectFactory emptyFactory = new BusinessObjectFactory();
					PaymentApprovalItemCollection approvalItems = new PaymentApprovalItemCollection(emptyFactory);
					approvalItems.Load();
					AssertEquals("No Approval Items should be posted to DB yet since posting happens when MatchingForm is closed", 0, approvalItems.Count);

					matchingForm.MatchAndCloseButton.PerformClick();    // Note: this does not post the matching session

					approvalItems.Load();
					AssertEquals("1 PaymentApprovalItem should be posted to DB when matching form is closed", 1, approvalItems.Count);
					ZQuery invMatchFilter = new ZQuery(AccPaymentApprovalItemSchema.A2_AH, testAPInv.PK);
					AccPaymentApprovalItem invApproval = emptyFactory.LoadTop1<AccPaymentApprovalItem>(invMatchFilter);
					AssertNotNull("There should be a PaymentApprovalItem  for the invoice", invApproval);

					AssertEquals("DisplayMode of Original PaymentApprovalForm should be Browse", ODisplayMode.Browse,
						payApproveForm.DisplayMode);

					// check that remittance form pops up
					exposedPrintForm = ((PaymentPrintManager.TestPaymentPrintManager)payApproveForm.PrintManager_ForTestOnly).LastPrintForm;
					AssertNotNull("The remittance advice print form should not be null because it should have been shown", exposedPrintForm);
					//Assert("Remittance advice print form should be visible", ExposedPrintForm.Visible);
					//AssertEquals("Parent form should be the Payment form", PayApproveForm, ((IModalForm)ExposedPrintForm).ModalTo);
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
					FindAndDisposeOpenPaymentForm();
				}
			}
		}

		#endregion

		[ExpectNoExceptions]
		public void TestAccessDisplayModeOnDeletedPayment()
		{
			SetupDataForTest();
			var testAPPaymentApproval = GetNewTestAPPaymentApprovalThatPostsOnSaving(TestOrg, TestBank, ReceiptTypes.Cheque,
				TestCheques, "2", 94M);
			using (var payApproveForm = new MockPaymentApprovalForm(testAPPaymentApproval))
			{
				testAPPaymentApproval.Delete();
				Assert(testAPPaymentApproval.IsDeleted);
				AssertEquals(ODisplayMode.Edit, payApproveForm.DisplayMode);
			}
		}

		#region TestChangingCurrencyUpdatesExchangeRate

		public void TestChangingCurrencyUpdatesExchangeRate()
		{
			SetupDataForTest();

			AccBankAccount testUSDBank = Factory.NewWithValidTestData<AccBankAccount>();
			testUSDBank.AB_RX_NKAccountCurrency = "USD";

			APPaymentApprovalWithoutAuthorisation testAPPaymentApproval = GetNewTestAPPaymentApprovalThatPostsOnSaving(TestOrg, testUSDBank, ReceiptTypes.Cheque,
				TestCheques, "2", 94M);
			AssertEquals("PaymentApproval Currency", "USD", testAPPaymentApproval.AV_RX_NKPaymentCurrency);
			testAPPaymentApproval.ExchangeRate.Rate = 0.9m;

			using (MockPaymentApprovalForm payApproveForm = new MockPaymentApprovalForm(testAPPaymentApproval))
			{
				NewMatchGroupForm matchingForm = null;
				PaymentDocumentsPrintPopup exposedPrintForm = null;

				try
				{
					payApproveForm.Show();

					testAPPaymentApproval.AV_AB = TestBank.PK;

					AssertEquals("ExchangeRate should be set to 1 for local currency", 1m, testAPPaymentApproval.ExchangeRate.Rate);

					payApproveForm.Close();
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
					FindAndDisposeOpenPaymentForm();
				}
			}
		}

		#endregion

		#region TestMatchingFormNotPosted

		public void TestMatchingFormNotPosted()
		{
			SetupDataForTest();
			Factory.Save();

			PaymentApprovalWithAuthorisation testARPaymentApproval = Factory.New<APPaymentApprovalWithAuthorisation>();
			testARPaymentApproval.AV_OH = TestOrg.PK;
			testARPaymentApproval.AV_AB = TestBank.PK;
			testARPaymentApproval.AV_PaymentType = ReceiptTypes.Cash;
			testARPaymentApproval.AV_ChequeOrReference = "3";
			testARPaymentApproval.AV_Amount = 46M;

			using (PaymentApprovalForm testPayForm = new PaymentApprovalForm(testARPaymentApproval))
			{
				NewMatchGroupForm matchingForm = null;

				try
				{
					testPayForm.FireSaveButton();   // user clicks payment detail

					matchingForm = ZFormModaliser.ActiveForm as NewMatchGroupForm;

					AssertNotNull("Active form should be the NewMatchGroupForm", matchingForm);
					APPaymentApprovalMatching matchingBizO = matchingForm.BusinessEntity as APPaymentApprovalMatching;
					AssertNotNull("MatchingBizO should have type of APPaymentApprovalMatching", matchingBizO);

					AssertEquals("The payment should be one of the transactions selected for matching", 1, matchingBizO.MatchedTransactions.Count);
					Assert("The payment should be one of the transactions selected for matching", matchingBizO.MatchedTransactions.Contains(testARPaymentApproval));

					matchingForm.Close();   // this should post the transactions

					Assert("The form should still be writeable", !testARPaymentApproval.ReadOnly);
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

		#endregion

		#region TestWhenUserClosesTheMatchScreenThenReselectsPaymentDetails

		public void TestUserClosesTheMatchScreenThenReselectsPaymentDetails()
		{
			SetupDataForTest();
			Factory.Save();

			APPaymentApprovalWithoutAuthorisation testPaymentApproval = Factory.New<APPaymentApprovalWithoutAuthorisation>();
			testPaymentApproval.AV_OH = TestOrg.PK;
			testPaymentApproval.AV_AB = TestBank.PK;
			testPaymentApproval.AV_PaymentType = ReceiptTypes.Cash;
			testPaymentApproval.AV_ChequeOrReference = "3";
			testPaymentApproval.AV_Amount = 46M;

			APInvoice invoice = Factory.New<APInvoice>();
			invoice.AH_TransactionNum = "10001";
			invoice.AH_OH = TestOrg.PK;
			APInvoiceLine line = (APInvoiceLine)invoice.Lines.AddNew();
			var chargeList = line.ChargeList;
			chargeList.Load();
			var chargeLines = chargeList.Cast<GenericCharge>().Where(gc => gc.VC_Description != ZString.Empty);
			AssertNotEquals("Precondition: There must be non empty description Charge Lines in Setup Data", 0, chargeLines.Count());
			line.GenericCharge = chargeLines.First().PK;
			line.AL_OSExTaxAmount = 46m;
			using (MockPaymentApprovalForm testPayForm = new MockPaymentApprovalForm(testPaymentApproval))
			{
				NewMatchGroupForm firstMatchingForm = null;
				NewMatchGroupForm secondMatchingForm = null;
				PaymentDocumentsPrintPopup exposedPrintForm = null;

				try
				{
					testPayForm.Show();
					testPayForm.PaymentDetailButton_ForTestOnly.PerformClick();

					//TestPayForm.FireSaveButton();
					//TestPayForm.ShowPreSaveDialogs(); // user clicks payment detail

					firstMatchingForm = ZFormModaliser.ActiveForm as NewMatchGroupForm;
					AssertNotNull("Active form should be the NewMatchGroupForm", firstMatchingForm);
					APPaymentApprovalMatching matchingBizO = firstMatchingForm.BusinessEntity as APPaymentApprovalMatching;
					AssertNotNull("MatchingBizO should have type of APPaymentApprovalMatching", matchingBizO);

					AssertEquals("There should be one transaction selected for matching", 1, matchingBizO.MatchedTransactions.Count);
					Assert("The payment should be one of the transactions selected for matching", matchingBizO.MatchedTransactions.Contains(testPaymentApproval));
					matchingBizO.MoveFromUnmatchToMatch(new BusinessObject[] { invoice });
					AssertEquals("Precondition: Balance should be zero", 0m, matchingBizO.Balance);

					firstMatchingForm.CloseButton.PerformClick(); // User clicks 'close' instead of 'Match and Close'
					Assert("The form should still be writeable", !testPaymentApproval.ReadOnly);

					// If the user now goes to click on 'Payment Details' it should always show the matching form.
					// The save MUST be done on the matching form otherwise the matchlinks will not be created.

					testPayForm.PaymentDetailButton_ForTestOnly.PerformClick(); // User clicks payment detail for the second time

					secondMatchingForm = ZFormModaliser.ActiveForm as NewMatchGroupForm;
					AssertNotNull("Active form should be the NewMatchGroupForm", secondMatchingForm);
					matchingBizO = secondMatchingForm.BusinessEntity as APPaymentApprovalMatching;
					AssertNotNull("MatchingBizO should have type of APPaymentApprovalMatching", matchingBizO);

					secondMatchingForm.MatchAndCloseButton.PerformClick(); // User clicks 'Match and Close' button to finalise
					Assert("The Payment Form should be read only", testPaymentApproval.ReadOnly);

					exposedPrintForm = ((PaymentPrintManager.TestPaymentPrintManager)testPayForm.PrintManager_ForTestOnly).LastPrintForm;
					AssertNotNull("The remittance advice print form should not be null because it should have been shown", exposedPrintForm);
					//Assert("Remittance advice print form should be visible", ExposedPrintForm.Visible);
					//AssertEquals("Parent form should be the Payment form", TestPayForm, ((IModalForm)ExposedPrintForm).ModalTo);
				}
				finally
				{
					if (firstMatchingForm != null)
					{
						firstMatchingForm.Dispose();
					}

					if (secondMatchingForm != null)
					{
						secondMatchingForm.Dispose();
					}

					if (exposedPrintForm != null)
					{
						exposedPrintForm.Dispose();
					}

					FindAndDisposeOpenPaymentForm();
				}
			}

			APPayment newPayment = Factory.Load<APPayment>(testPaymentApproval.AV_AH);
			AssertNotNull("Payment should be created", newPayment);
			AssertEquals("Outstanding Amount on the Payment", 0m, newPayment.AH_OutstandingAmount);
			AssertEquals("Outstanding Amount on the Invoice", 0m, newPayment.AH_OutstandingAmount);

			ZQuery findPaymentMatchLinkQuery = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, newPayment.PK);
			AccTransactionMatchLink paymentMatchLink = Factory.LoadTop1<AccTransactionMatchLink>(findPaymentMatchLinkQuery);
			AssertNotNull("Matchlink should have been created for the Payment", paymentMatchLink);

			ZQuery findInvoiceMatchLinkQuery = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, invoice.PK);
			AccTransactionMatchLink invoiceMatchLink = Factory.LoadTop1<AccTransactionMatchLink>(findInvoiceMatchLinkQuery);
			AssertNotNull("Matchlink should have been created for the Invoice", invoiceMatchLink);
		}

		#endregion

		#region TestShowPreSaveDialogs

		public void TestShowPreSaveDialogs()
		{
			AccBankAccount testBank = Factory.NewWithValidTestData<AccBankAccount>();
			Factory.Save();

			AccChequeBook testCheques = Factory.NewWithValidTestData<AccChequeBook>();
			testCheques.AK_StartNo = 1;
			testCheques.AK_LastNo = 100;
			testCheques.AK_CurrentNo = 1;
			testCheques.AK_AB = testBank.PK;

			Factory.Save();

			APInvoice invoice = Factory.New<APInvoice>();
			invoice.AH_OH = TestOrg.PK;
			invoice.AH_TransactionNum = "Pay1";
			TestObjectCreator.CreateInvoiceLine(invoice, invoice.TransactionCurrency, 1m, 0M);
			invoice.Lines[0].AL_AT = ZGuid.Empty;
			invoice.Lines[0].AL_OSExTaxAmount = 94M;
			invoice.Lines[0].GenericCharge = Factory.LoadTop1<GenericCharge>(invoice.Lines[0].ChargeList.CompleteFilter).PK;
			invoice.Lines[0].AL_Desc = "Line Description 1";
			Factory.Save();

			PaymentApprovalWithAuthorisation aPPaymentApproval = TestObjectCreator.GetNewTestAPPaymentApproval(TestOrg, testBank, ReceiptTypes.Cheque,
				"2", 94M, testCheques);

			Assert("Payment Approval should not have errors", !aPPaymentApproval.HasErrors);

			using (PaymentApprovalForm payForm = new PaymentApprovalForm(aPPaymentApproval)) // test popping up matching form
			{
				ContinueWithSave continueResult = payForm.ShowPreSaveDialogs_ForTestOnly();
				AssertEquals("Should not continue since matching form is shown", ContinueWithSave.No, continueResult);
				using (NewMatchGroupForm matchingForm = ZFormModaliser.ActiveForm as NewMatchGroupForm)
				{
					AssertNotNull("Matching form should be the active form", matchingForm);
					matchingForm.Close();
				}
			}

			aPPaymentApproval.PaymentMatchingBaseObject.MoveAllFromUnmatchToMatch();
			using (PaymentApprovalForm payForm = new PaymentApprovalForm(aPPaymentApproval))
			{
				ContinueWithSave continueResult = payForm.ShowPreSaveDialogs_ForTestOnly();
				AssertEquals("Should continue since matching was completed", ContinueWithSave.Yes, continueResult);
			}

			PaymentApprovalWithAuthorisation aPPaymentApproval2 = TestObjectCreator.GetNewTestAPPaymentApproval(TestOrg, testBank, ReceiptTypes.Cheque,
				"2", 0M, testCheques);

			using (PaymentApprovalForm payForm = new PaymentApprovalForm(aPPaymentApproval2)) // test popping up matching form
			{
				ContinueWithSave continueResult = payForm.ShowPreSaveDialogs_ForTestOnly();
				AssertEquals("Should not continue since matching form is shown", ContinueWithSave.No, continueResult);
				using (NewMatchGroupForm matchingForm = ZFormModaliser.ActiveForm as NewMatchGroupForm)
				{
					AssertNotNull("Matching form should be the active form", matchingForm);
					matchingForm.Close();
				}
			}

			aPPaymentApproval.PaymentMatchingBaseObject.MoveAllFromUnmatchToMatch();

			using (PaymentApprovalForm payForm = new PaymentApprovalForm(aPPaymentApproval))
			{
				ContinueWithSave continueResult = payForm.ShowPreSaveDialogs_ForTestOnly();
				AssertEquals("Should continue since matching was completed", ContinueWithSave.Yes, continueResult);
			}
		}
		public void TestShowPreSaveDialogs_WithMatchedtransactionsHasErrors()
		{
			var testBank = Factory.NewWithValidTestData<AccBankAccount>();
			Factory.Save();

			var testCheques = Factory.NewWithValidTestData<AccChequeBook>();
			testCheques.AK_StartNo = 1;
			testCheques.AK_LastNo = 100;
			testCheques.AK_CurrentNo = 1;
			testCheques.AK_AB = testBank.PK;

			Factory.Save();

			var invoice = Factory.New<APInvoice>();
			invoice.AH_OH = TestOrg.PK;
			invoice.AH_TransactionNum = "Pay1";
			TestObjectCreator.CreateInvoiceLine(invoice, invoice.TransactionCurrency, 1m, 0M);
			invoice.Lines[0].AL_AT = ZGuid.Empty;
			invoice.Lines[0].AL_OSExTaxAmount = 94M;
			Factory.Save();

			var aPPaymentApproval = TestObjectCreator.GetNewTestAPPaymentApproval(TestOrg, testBank, ReceiptTypes.Cheque,
				"2", 94M, testCheques);

			aPPaymentApproval.PaymentMatchingBaseObject.MoveAllFromUnmatchToMatch();
			using (var payForm = new PaymentApprovalForm(aPPaymentApproval))
			{
				var continueResult = payForm.ShowPreSaveDialogs_ForTestOnly();
				AssertEquals("Should not continue since matched transactions has error", ContinueWithSave.No, continueResult);
				using (var matchingForm = ZFormModaliser.ActiveForm as NewMatchGroupForm)
				{
					AssertNotNull("Matching form should be the active form", matchingForm);
					matchingForm.Close();
				}
			}
		}

		#endregion

		#region TestPrintManager

		public void TestPrintManager()
		{
			APPayment newlyCreatedPayment = Factory.New<APPayment>();
			PaymentApprovalWithAuthorisation aPPaymentApproval = Factory.New<APPaymentApprovalWithAuthorisation>();
			aPPaymentApproval.AV_AH = newlyCreatedPayment.PK;

			using (PaymentApprovalForm payForm = new PaymentApprovalForm(aPPaymentApproval))
			{
				AssertNotNull("PaymentPrintManager_ForTestOnly should exist", payForm.PrintManager_ForTestOnly);
			}
		}

		#endregion

		#region TestCancellingMatchSessionMakesChequeNumberEditable

		public void TestCancellingMatchSessionMakesAllFieldsEditable()
		{
			SetupDataForTest();
			PaymentApprovalWithAuthorisation aPPaymentApproval = TestObjectCreator.GetNewTestAPPaymentApproval(TestOrg, TestBank, ReceiptTypes.Cheque,
				"2", 23M, TestCheques);

			using (PaymentApprovalForm payForm = new PaymentApprovalForm(aPPaymentApproval))
			{
				payForm.FireSaveButton();
				using (NewMatchGroupForm matchingForm = ZFormModaliser.ActiveForm as NewMatchGroupForm)
				{
					Assert("Precondition: Fields should be editable", !aPPaymentApproval.ReadOnly);
				}
			}
		}

		#endregion

		public void TestMessageIfChequeBookUsesSamePrinterDidNotShow_WhenSaveAsDraft()
		{
			AssertMessageIfChequeBookUsesSamePrinterCore(new Action<PaymentApprovalForm, AccChequeBook, StmPrintQueue>((form, chequeBook, printer) => {
				(form.BusinessEntity as PaymentApprovalBase).ImportSelectedHotCheque(Factory.NewWithValidTestData<AccHotCheque>());
				form.FireSaveButton();
				AssertNotEquals("Last message should not be that chequebook uses same printer", AccChequeBook.WarningPaymentWithChequeBookWithSamePrinterMessage(printer.SQ_QueueName, chequeBook.AK_CurrentNo), UnitTestUserNotification.Instance.LastMessage.Text);
			}));
		}

		public void TestMessageIfChequeBookUsesSamePrinterDidNotShow_WhenHotChequeImported()
		{
			AssertMessageIfChequeBookUsesSamePrinterCore(new Action<PaymentApprovalForm, AccChequeBook, StmPrintQueue>((form, chequeBook, printer) => {
				form.SaveAsDraftButton_ForTestOnly.PerformClick();
				AssertNotEquals("Last message should not be that chequebook uses same printer", AccChequeBook.WarningPaymentWithChequeBookWithSamePrinterMessage(printer.SQ_QueueName, chequeBook.AK_CurrentNo), UnitTestUserNotification.Instance.LastMessage.Text);
			}));
		}

		public void TestMessageIfChequeBookUsesSamePrinterShown()
		{
			AssertMessageIfChequeBookUsesSamePrinterCore(new Action<PaymentApprovalForm, AccChequeBook, StmPrintQueue>((form, chequeBook, printer) => {
				form.FireSaveButton();
				AssertEquals("Last message should be that chequebook uses same printer", AccChequeBook.WarningPaymentWithChequeBookWithSamePrinterMessage(printer.SQ_QueueName, chequeBook.AK_CurrentNo), UnitTestUserNotification.Instance.LastMessage.Text);
			}));
		}

		public void AssertMessageIfChequeBookUsesSamePrinterCore(Action<PaymentApprovalForm, AccChequeBook, StmPrintQueue> action)
		{
			SetupDataForTest();

			StmPrintQueue printer = Factory.New<StmPrintQueue>();
			printer.SQ_QueueName = "Queue Name";
			AccBankAccount testBank = Factory.NewWithValidTestData<AccBankAccount>();
			AccChequeBook chequeBook = Factory.New(typeof(AccChequeBook)) as AccChequeBook;
			chequeBook.AK_AB = testBank.PK;
			chequeBook.AK_GB = Factory.LoadTop1(typeof(GlbBranch), new ZQuery()).PK;
			chequeBook.AK_Code = "Book1";
			chequeBook.AK_AutoPrintCheque = ZBool.True;
			chequeBook.AK_SQ = printer.PK;
			chequeBook.BankAccount.AB_SO_ChequeTemplate = TestObjectCreator.StandardTemplatePK;

			AccChequeBook book2 = Factory.New(typeof(AccChequeBook)) as AccChequeBook;
			book2.AK_AB = testBank.PK;
			book2.AK_GB = chequeBook.AK_GB;
			book2.AK_Code = "Book2";
			book2.AK_AutoPrintCheque = ZBool.True;
			book2.AK_SQ = printer.PK;
			Factory.Save();

			APPaymentApprovalWithoutAuthorisation aPPaymentApproval = GetNewTestAPPaymentApprovalThatPostsOnSaving(TestOrg, testBank, ReceiptTypes.Cheque,
				TestCheques, "2", 94M);

			using (PaymentApprovalForm form = new PaymentApprovalForm(aPPaymentApproval))
			{
				aPPaymentApproval.AV_AB = chequeBook.AK_AB;
				aPPaymentApproval.AV_AK = chequeBook.PK;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				action(form, chequeBook, printer);
			}
		}

		public void TestSave()
		{
			TestObjectCreator objectCreator = new TestObjectCreator(Factory);
			new AccountingPeriodTestHelper().SetupPeriods();
			APPaymentApprovalWithoutAuthorisation paymentApproval = Factory.New<APPaymentApprovalWithoutAuthorisation>();
			paymentApproval.AV_OH = TestOrg.PK;
			paymentApproval.AV_PaymentType = ReceiptTypes.Cash;
			paymentApproval.AV_AB = objectCreator.AUDBankAccount.PK;
			paymentApproval.AV_Amount = 100m;
			paymentApproval.FillWithValidTestData();
			TestObjectCreator.FillPaymentMatchTransactions("1", typeof(APInvoice), TestOrg, paymentApproval, 100m);

			using (PaymentApprovalForm payForm = new PaymentApprovalForm(paymentApproval))
			{
				try
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

					using (var handler = GetSaveInProgressHandler(payForm))
					{
						//TODO: Hello, fellow developer! If this test is failing, uncomment the "DISABLE TRIGGER" command below and remove this TODO line.
						//HACK: Temporarily allow UPDATE of AH_InvoiceAmount to test critical validation. Refer to WI00559931, WI00482153.
						//TestConnection.ExecuteNonQuery("DISABLE TRIGGER TG_AccTransactionHeader_ProtectCriticalFieldsFromUpdating ON AccTransactionHeader");
						payForm.Save_ForTestOnly(new ITransactionParticipant[] { Factory });
					}

					AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
				}
				finally
				{
					if (ZFormModaliser.LastFormShownDialogForTest != null)
					{
						ZFormModaliser.LastFormShownDialogForTest.Dispose();
					}
					FindAndDisposeOpenPaymentForm();
				}
			}

			using (PaymentApprovalForm payForm = new PaymentApprovalForm(paymentApproval))
			{
				try
				{
					paymentApproval.TransactionHeader.AH_InvoiceAmount = 1;
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

					using (var handler = GetSaveInProgressHandler(payForm))
					{
						payForm.Save_ForTestOnly(new ITransactionParticipant[] { Factory });
					}

					AssertContains(@"An error has occurred. Your unsaved work must be re-entered.

Please close the form in which you were working and re-enter the data.

Error Message: Incorrect outstanding amount.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
				finally
				{
					if (ZFormModaliser.LastFormShownDialogForTest != null)
					{
						ZFormModaliser.LastFormShownDialogForTest.Dispose();
					}
					FindAndDisposeOpenPaymentForm();
				}
			}
			AssertNotNull("ExceptionReporter should have caught the exceptions", ExceptionReporterTestListener.Instance[0]);
			ExceptionReporterTestListener.Instance.Clear();

			IDisposable GetSaveInProgressHandler(ZForm form)
			{
				var methodInfo = typeof(ZForm).GetMethod("SetSavingInProgress", BindingFlags.Instance | BindingFlags.NonPublic);
				return (IDisposable)methodInfo.Invoke(form, Array.Empty<object>());
			}
		}

		#region TestPostWithoutMatching

		public void TestPostWithoutMatching()
		{
			SetupDataForTest();

			APPaymentApprovalWithoutAuthorisation aPPaymentApproval = GetNewTestAPPaymentApprovalThatPostsOnSaving(TestOrg, TestBank, ReceiptTypes.Cheque, TestCheques, "2", 94M);

			try
			{
				using (PaymentApprovalForm payForm = new PaymentApprovalForm(aPPaymentApproval))
				{
					payForm.OpenedMatchFormsForTest = 0;
					payForm.AutoCloseMatchingForms = true;

					payForm.Show();

					aPPaymentApproval.PaymentMatchingBaseObject.AddMiscellaneousTransaction(aPPaymentApproval.PaymentMatchingBaseObject.GetMiscellaneousTransaction(TransactionTypes.Discount));
					aPPaymentApproval.AV_Discount = -94M;

					payForm.PostWithoutMatchingButton_ForTestOnly.PerformClick();

					AssertEquals("IsPostWithoutMatching_ForTestOnly", ZBool.True, payForm.IsPostWithoutMatching_ForTestOnly);
					AssertEquals("APPaymentApproval.IsProcessingPaymentDetail", ZBool.False, aPPaymentApproval.IsProcessingPaymentDetail);
					AssertEquals("Matching form should not be shown", 0, payForm.OpenedMatchFormsForTest);

					AssertEquals("The Payment should not be fully paid", true, aPPaymentApproval.TransactionHeader.AH_FullyPaidDate.IsEmpty);
					AssertEquals("The Payment should be fully outstanding", aPPaymentApproval.TransactionHeader.AH_InvoiceAmount, aPPaymentApproval.TransactionHeader.AH_OutstandingAmount);

					if (ZFormModaliser.LastFormShownDialogForTest != null)
					{
						ZFormModaliser.LastFormShownDialogForTest.Dispose();
					}
				}
			}
			finally
			{
				FindAndDisposeOpenPaymentForm();
			}
		}

		public void TestPostWithoutMatchingWithSaveException()
		{
			SetupDataForTest();

			var paymentApproval = GetNewTestAPPaymentApprovalThatPostsOnSaving<MockAPPaymentApprovalWithoutAuthorisation>(TestOrg, TestBank, ReceiptTypes.Cheque, TestCheques, "2", 94M);

			using (var form = new MockPaymentApprovalForm_ForCloseFormTest(paymentApproval))
			{
				form.Show();

				paymentApproval.PaymentMatchingBaseObject.AddMiscellaneousTransaction(paymentApproval.PaymentMatchingBaseObject.GetMiscellaneousTransaction(TransactionTypes.Discount));
				paymentApproval.AV_Discount = -94M;

				form.PostWithoutMatchingButton_ForTestOnly.PerformClick();

				AssertEquals("IsPostWithoutMatching_ForTestOnly", ZBool.True, form.IsPostWithoutMatching_ForTestOnly);
				AssertEquals("Should trigger critical validation exception", "Simulate critical validation error", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Should NOT save payment into DB", false, paymentApproval.IsInDatabase);
				AssertEquals("Form should be enabled when close", true, form.IsFormEnabledWhenClose_ForTestOnly);
			}
		}

		public void TestPostWithoutMatchingWithReportException()
		{
			SetupDataForTest();

			var paymentApproval = GetNewTestAPPaymentApprovalThatPostsOnSaving<APPaymentApprovalWithoutAuthorisation>(TestOrg, TestBank, ReceiptTypes.Cheque, TestCheques, "2", 94M);

			using (var form = new MockPaymentApprovalForm_ForCloseFormTest(paymentApproval))
			{
				form.Show();

				paymentApproval.PaymentMatchingBaseObject.AddMiscellaneousTransaction(paymentApproval.PaymentMatchingBaseObject.GetMiscellaneousTransaction(TransactionTypes.Discount));
				paymentApproval.AV_Discount = -94M;

				form.PostWithoutMatchingButton_ForTestOnly.PerformClick();

				AssertEquals("IsPostWithoutMatching_ForTestOnly", ZBool.True, form.IsPostWithoutMatching_ForTestOnly);
				AssertEquals("Payment is already saved before printing", true, paymentApproval.IsInDatabase);
				AssertEquals("Should trigger report exception", "Simulate report exception when printing", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Form should be enabled when close", true, form.IsFormEnabledWhenClose_ForTestOnly);
			}
		}

		#endregion

		#region TestPostWithoutMatchingButtonForAPPaymentsOnly

		public void TestPostWithoutMatchingButtonForAPPaymentsOnly()
		{
			ARPaymentApprovalWithoutAuthorisation aRPaymentApproval = Factory.New<ARPaymentApprovalWithoutAuthorisation>();

			using (PaymentApprovalForm payForm = new PaymentApprovalForm(aRPaymentApproval))
			{
				payForm.Show();

				bool isPostButtonExist = false;
				foreach (Control control in payForm.Controls)
				{
					if (control.Name == "PostWithoutMatchingButton")
					{
						isPostButtonExist = true;
					}
				}
				Assert("PostWithoutMatchingButton should be shown only for Payable Payments", !isPostButtonExist);
			}
		}

		#endregion

		#region TestPostWithoutMatchingButtonWithAllowedSecurity

		public void TestPostWithoutMatchingButtonWithAllowedSecurity()
		{
			SetupDataForTest();

			APPaymentApprovalWithoutAuthorisation aPPaymentApproval = GetNewTestAPPaymentApprovalThatPostsOnSaving(TestOrg, TestBank, ReceiptTypes.Cheque,
				TestCheques, "2", 94M);

			bool oldNewPayablesPaymentAllowPostingWithoutMatching = Env.Security.NewPayablesPaymentAllowPostingWithoutMatching.IsAllowed;
			try
			{
				Env.Security.NewPayablesPaymentAllowPostingWithoutMatching.IsAllowed = false;
				using (PaymentApprovalForm payForm = new PaymentApprovalForm(aPPaymentApproval))
				{
					payForm.Show();

					payForm.PostWithoutMatchingButton_ForTestOnly.PerformClick();

					AssertEquals("Error message should be shown.", SecurityCore.SecurityErrorMessage + " Posting Without Matching.",
						UnitTestUserNotification.Instance.LastMessage.Text);
				}

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				Env.Security.NewPayablesPaymentAllowPostingWithoutMatching.IsAllowed = true;
				using (PaymentApprovalForm payForm = new PaymentApprovalForm(aPPaymentApproval))
				{
					payForm.Show();

					payForm.PostWithoutMatchingButton_ForTestOnly.PerformClick();

					AssertEquals("No messages should be shown.", null, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
			finally
			{
				Env.Security.NewPayablesPaymentAllowPostingWithoutMatching.IsAllowed = oldNewPayablesPaymentAllowPostingWithoutMatching;

				if (ZFormModaliser.LastFormShownDialogForTest != null)
				{
					ZFormModaliser.LastFormShownDialogForTest.Dispose();
				}

				FindAndDisposeOpenPaymentForm();
			}
		}

		#endregion

		#region ReferenceNumberLabel Test

		public void TestReferenceNumberLabel()
		{
			using (PaymentApprovalForm form = (PaymentApprovalForm)GetFormToBashCore())
			{
				AssertEquals("Text on ChequeNumberLabel should be 'Check Number'", "Check Number", form.ChequeNoTextBox_ForTestOnly.GetExtension<LabelCaptionRenderer>().Caption);
			}
		}

		#endregion

		#region Test Prompt Bank Transfer Form after posting EPayment approval
		public void TestPromptBankTransferFormAfterPostingEpaymentApproval()
		{
			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper();
			testHelper.SetupPeriods();

			var ePaymentBankAccount = TestObjectCreator.CreateEPaymentBankAccount();
			var paymentApproval = TestObjectCreator.GetNewTestAPPaymentApproval(TestObjectCreator.AALSHI, ePaymentBankAccount, ReceiptTypes.EPayment, "2", 200m, "USD", 1, PaymentApprovalStatus.FullyApproved);
			paymentApproval.IsAllowedToPost = true;
			var deal = TestObjectCreator.CreateValidEPaymentDealForStatus(DealStatusCodes.Accepted, paymentApproval);
			deal.Quote.QU_FromAmount = 200m;

			var openedForms = Array.Empty<Form>();
			try
			{
				using (var newForm = new MockPaymentApprovalForm(paymentApproval))
				{
					newForm.Show();
					newForm.IsPostWithoutMatching_ForTestOnly = true;
					newForm.PostWithoutMatchingButton_ForTestOnly.PerformClick();

					openedForms = ZApplication.GetOpenForms();

					var newBankTansferForm = openedForms.FirstOrDefault(x => x.Name.Contains("BankTransferForm"));

					AssertNotNull(newBankTansferForm);
					AssertEquals(ODisplayMode.New, ((ZForm)newBankTansferForm).DisplayMode);
				}
			}
			finally
			{
				foreach (var form in openedForms)
				{
					if ((!form.IsDisposed) && (!form.Text.Contains("CargoWise"))) //Skips the CargoWise Main window when people are running unit tests with local CW1
					{
						form.Dispose();
					}
				}
			}
		}

		#endregion

		#region TestAutoPrintCheque

		[ExpectNoExceptions]
		public void TestAutoPrintCheque_PostWithoutMatching()
		{
			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper();
			testHelper.SetupPeriods();

			AccBankAccount bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			AccChequeBook chequeBook = GetAutoPrintChequeBook(bankAccount, 1, 3, 3);

			APPaymentApprovalWithoutAuthorisation aPPaymentApproval = GetNewTestAPPaymentApprovalThatPostsOnSaving(TestOrg, bankAccount, ZArchitecture.Core.ReceiptTypes.Cheque,
				chequeBook, "", 94M);

			aPPaymentApproval.AV_PostDate = ZDateTime.Now;

			aPPaymentApproval.RunPreSaveValidation();
			Assert("TestPayment should not have any errors before saving", !aPPaymentApproval.HasErrors);
			Assert("ChequeNumber should be empty", aPPaymentApproval.AV_ChequeOrReference.IsEmpty);
			Assert("Cheque Number allocation should be enabled", ((IChequeNumberAutoAllocation)aPPaymentApproval).IsAutoAllocationEnabled);

			try
			{
				using (MockPaymentApprovalForm newForm = new MockPaymentApprovalForm(aPPaymentApproval))
				{
					newForm.Show();
					newForm.IsPostWithoutMatching_ForTestOnly = true;
					newForm.PostWithoutMatchingButton_ForTestOnly.PerformClick();

					Assert("Auto printing should be performed", newForm.Test_Allocator.ChequeWasAutoPrinted);
					AssertEquals("Printing called only once", 1, newForm.Test_Allocator.PrintingCalled_Counter);
					AssertEquals("PaymentPrinter should have correct Printer passed to", chequeBook.AK_SQ, newForm.Test_Allocator.PrinterPassedForAutoPrinting);
					AssertEquals("Cheque number should be autoallocated on PaymentApproval", "3", aPPaymentApproval.AV_ChequeOrReference);
					AssertEquals("Payment ChequeOrReference", aPPaymentApproval.AV_ChequeOrReference, aPPaymentApproval.NewPayment.AH_ChequeOrReference);
					Assert("Cheque should be auto printed for the payment", ((IChequeNumberAutoAllocation)aPPaymentApproval.NewPayment).ChequeIsAutoPrinted);

					Assert("PrinterManager should not have auto printed the payment again", !((PaymentPrintManager.MockPaymentPrint)((PaymentPrintManager.TestPaymentPrintManager)newForm.PrintManager_ForTestOnly).PaymentPrinter_Exposed).IsChequeAutoPrinted);
					AssertEquals("Should have opened Remittance Advice Printing Form", typeof(PaymentDocumentsPrintPopup.MockPaymentDocumentsPrintPopup), ZFormModaliser.LastFormShownDialogForTest.GetType());
					((PaymentDocumentsPrintPopup)ZFormModaliser.LastFormShownDialogForTest).Close();
					Assert("The flag on the print manager should be set", newForm.PrintManager_ForTestOnly.ChequeIsAutoPrinted);

					chequeBook.Reload();
					AssertEquals("CurrentNo should change on the cheque book", 4m, chequeBook.AK_CurrentNo);
					Assert("Out of cheque numbers - Cheque book should become inactive", !chequeBook.AK_IsActive);
				}
			}
			finally
			{
				FindAndDisposeOpenPaymentForm();
			}
		}

		[ExpectNoExceptions]
		public void TestAutoAllocationAndPrintChequesErrorInTransaction()
		{
			(new AccountingPeriodTestHelper()).SetupPeriods();
			var bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			var chequeBook = GetAutoPrintChequeBook(bankAccount, 1, 3, 3);
			chequeBook.AK_Desc = "TestAutoAllocationAndPrintCheques";
			Factory.Save();

			var aPPaymentApproval = GetNewTestAPPaymentApprovalThatPostsOnSaving(TestOrg, bankAccount, ZArchitecture.Core.ReceiptTypes.Cheque, chequeBook, "", 94M);
			aPPaymentApproval.AV_PostDate = ZDateTime.Now;
			try
			{
				using (var newForm = new MockPaymentApprovalForm(aPPaymentApproval))
				{
					newForm.Show();
					newForm.IsPostWithoutMatching_ForTestOnly = true;
					Assert("Error shown to user", PaymentDocumentsPrinter.PerformTestAutoAllocationAndPrintChequesFailure(() => newForm.PostWithoutMatchingButton_ForTestOnly.PerformClick()));
				}
			}
			finally
			{
				FindAndDisposeOpenPaymentForm();
			}
		}

		#endregion

		#region TestAutoPrintCheque_MatchingForm

		public void TestAutoPrintCheque_MatchingForm()
		{
			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper();
			testHelper.SetupPeriods();

			AccBankAccount bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			AccChequeBook chequeBook = GetAutoPrintChequeBook(bankAccount, 1, 3, 3);

			ForwardingShipment shipment = TestObjectCreator.CreateShipment("S1");
			Job job = TestObjectCreator.CreateJob(shipment);

			APInvoice testAPInv = Factory.NewWithValidTestData<APInvoice>();
			testAPInv.AH_OH = TestOrg.PK;
			ZGuid genericChargeCodeGuid = TestObjectCreator.CC1.PK;

			InvoicingLineBase line = TestObjectCreator.CreateInvoiceLine(testAPInv, testAPInv.TransactionCurrency, testAPInv.AH_ExchangeRate, 94m, 0m, 0m, 94m, 0m, 0m, genericChargeCodeGuid);
			line.AL_JH = job.PK;
			var charge = TestObjectCreator.CreateJobCharge(line, job, TestObjectCreator.CC1, testAPInv.TransactionCurrency);
			testAPInv.AH_FullyPaidDate = ZDateTime.Empty;

			Factory.Save();

			APPaymentApprovalWithoutAuthorisation testAPPaymentApproval = GetNewTestAPPaymentApprovalThatPostsOnSaving(TestOrg, bankAccount, ZArchitecture.Core.ReceiptTypes.Cheque,
				chequeBook, "", 94M);
			Assert("TestPayment should not have any errors before saving", !testAPPaymentApproval.HasErrors);
			Assert("ChequeNumber should be empty", testAPPaymentApproval.AV_ChequeOrReference.IsEmpty);
			Assert("Cheque Number allocation should be enabled", ((IChequeNumberAutoAllocation)testAPPaymentApproval).IsAutoAllocationEnabled);

			using (MockPaymentApprovalForm payApproveForm = new MockPaymentApprovalForm(testAPPaymentApproval))
			{
				NewMatchGroupForm matchingForm = null;
				PaymentDocumentsPrintPopup exposedPrintForm = null;

				try
				{
					payApproveForm.Show();
					payApproveForm.PaymentDetailButton_ForTestOnly.PerformClick();  // user clicks payment detail button

					matchingForm = ZFormModaliser.ActiveForm as NewMatchGroupForm;
					AssertNotNull("The active form should be the matching form", matchingForm);
					APPaymentApprovalMatching matchingBizO = matchingForm.BusinessEntity as APPaymentApprovalMatching;
					Assert("OSPartialPaymentAmount should be readonly", ((IMatching)testAPPaymentApproval).OSPartialPaymentAmountInfo.ReadOnly);

					matchingBizO.MoveAllFromUnmatchToMatch();

					AssertNotNull("Matching Object should be APMatchingBase", matchingBizO);
					AssertEquals("Should be 2 transactions in MatchedTransactions", 2, matchingBizO.MatchedTransactions.Count);
					AssertEquals("Balance should be 0", 0M, matchingBizO.Balance);

					BusinessObjectFactory emptyFactory = new BusinessObjectFactory();
					PaymentApprovalItemCollection approvalItems = new PaymentApprovalItemCollection(emptyFactory);
					approvalItems.Load();
					AssertEquals("No Approval Items should be posted to DB yet since posting happens when MatchingForm is closed", 0, approvalItems.Count);

					matchingForm.MatchAndCloseButton.PerformClick();

					approvalItems.Load();
					AssertEquals("1 PaymentApprovalItem should be posted to DB when matching form is closed", 1, approvalItems.Count);
					ZQuery invMatchFilter = new ZQuery(AccPaymentApprovalItemSchema.A2_AH, testAPInv.PK);
					AccPaymentApprovalItem invApproval = emptyFactory.LoadTop1<AccPaymentApprovalItem>(invMatchFilter);
					AssertNotNull("There should be a PaymentApprovalItem  for the invoice", invApproval);

					AssertEquals("DisplayMode of Original PaymentApprovalForm should be Browse", ZArchitecture.Core.ODisplayMode.Browse,
						payApproveForm.DisplayMode);

					AssertEquals("Cheque number should be autoallocated on PaymentApproval", "3", testAPPaymentApproval.AV_ChequeOrReference);
					AssertEquals("Payment ChequeOrReference", testAPPaymentApproval.AV_ChequeOrReference, testAPPaymentApproval.NewPayment.AH_ChequeOrReference);
					Assert("Cheque should be auto printed for the payment", ((IChequeNumberAutoAllocation)testAPPaymentApproval.NewPayment).ChequeIsAutoPrinted);

					chequeBook.Reload();
					AssertEquals("CurrentNo should change on the cheque book", 4m, chequeBook.AK_CurrentNo);
					Assert("Out of cheque numbers - Cheque book should become inactive", !chequeBook.AK_IsActive);

					// check that remittance form pops up
					exposedPrintForm = ((PaymentPrintManager.TestPaymentPrintManager)payApproveForm.PrintManager_ForTestOnly).LastPrintForm;
					AssertNotNull("The remittance advice print form should not be null because it should have been shown", exposedPrintForm);
					Assert("AutoPrintCheque should have fired", payApproveForm.PrintManager_ForTestOnly.ChequeIsAutoPrinted);
					Assert("PrinterManager should not have auto printed the payment again", !((PaymentPrintManager.MockPaymentPrint)((PaymentPrintManager.TestPaymentPrintManager)payApproveForm.PrintManager_ForTestOnly).PaymentPrinter_Exposed).IsChequeAutoPrinted);
					Assert("Auto printing should be performed", payApproveForm.Test_Allocator.ChequeWasAutoPrinted);
					AssertEquals("Printing called only once", 1, payApproveForm.Test_Allocator.PrintingCalled_Counter);
					AssertEquals("PaymentPrinter should have correct Printer passed to", chequeBook.AK_SQ, payApproveForm.Test_Allocator.PrinterPassedForAutoPrinting);

					AssertEquals("Should have opened Remittance Advice Printing Form", typeof(PaymentDocumentsPrintPopup.MockPaymentDocumentsPrintPopup), ZFormModaliser.LastFormShownDialogForTest.GetType());
					((PaymentDocumentsPrintPopup)ZFormModaliser.LastFormShownDialogForTest).Close();
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
					FindAndDisposeOpenPaymentForm();
				}
			}
		}

		#endregion

		public void TestFormStaysOpenAfterMatching()
		{
			TestObjectCreator objectCreator = new TestObjectCreator(Factory);
			new AccountingPeriodTestHelper().SetupPeriods();
			APPaymentApprovalWithoutAuthorisation paymentApproval = Factory.New<APPaymentApprovalWithoutAuthorisation>();
			paymentApproval.AV_OH = TestOrg.PK;
			paymentApproval.AV_PaymentType = ReceiptTypes.Cash;
			paymentApproval.AV_AB = objectCreator.AUDBankAccount.PK;
			paymentApproval.AV_Amount = 100m;
			paymentApproval.FillWithValidTestData();

			using (MockPaymentApprovalForm payApproveForm = new MockPaymentApprovalForm(paymentApproval))
			{
				NewMatchGroupForm matchingForm = null;
				PaymentDocumentsPrintPopup exposedPrintForm = null;

				try
				{
					payApproveForm.Show();
					payApproveForm.PaymentDetailButton_ForTestOnly.PerformClick();
					matchingForm = ZFormModaliser.ActiveForm as NewMatchGroupForm;
					AssertNotNull("The active form should be the matching form", matchingForm);
					ZButton newExchangeDifferenceButton = matchingForm.Controls.Find("ExchangeDiffButton", true)[0] as ZButton;
					AssertNotNull("Exchange Difference Button should be found", newExchangeDifferenceButton);
					newExchangeDifferenceButton.PerformClick();
					TransactionViewForm transactionViewForm = ZFormModaliser.ActiveForm as TransactionViewForm;
					AssertNotNull("The active form should be the transaction view form", transactionViewForm);
					((ZPostingButtonsUserControl)transactionViewForm.Controls.Find("oPostingButtonsUserControl1", false)[0]).SaveAndCloseButton.PerformClick();
					AssertNull("Active form should now be the AP Match Group form", ZFormModaliser.GetActiveChildFormForParentForm(matchingForm));
					matchingForm.MatchAndCloseButton.PerformClick();
					exposedPrintForm = ((PaymentPrintManager.TestPaymentPrintManager)payApproveForm.PrintManager_ForTestOnly).LastPrintForm;

					AssertEquals("Payment approval form should close", false, payApproveForm.Visible);
					Assert("Payment form should open and stay open", FindAndDisposeOpenPaymentForm());
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

		public void TestFormClosesOnEsc()
		{
			using (PaymentApprovalForm form = (PaymentApprovalForm)GetFormToBashCore())
			{
				form.Show();
				TestKeyStrokeHelper.SendKeyToControl(form, Keys.Escape, true);
				Assert("Form should have closed", !form.Visible);
			}
		}

		#region TestCreditCardPaymentViaENett

		APPaymentApprovalWithoutAuthorisation GetAPPaymentApprovalWithoutAuthorisationForENett(AccBankAccount bankAccount, OrgHeader orgHeader)
		{
			APPaymentApprovalWithoutAuthorisation paymentApproval = Factory.NewWithValidTestData<APPaymentApprovalWithoutAuthorisation>();
			paymentApproval.AV_PaymentType = ReceiptTypes.eNettCreditCard;
			paymentApproval.AV_AB = bankAccount.PK;
			paymentApproval.AV_Amount = 100m;
			paymentApproval.AV_ChequeOrReference = "111";
			paymentApproval.CreditCardSecurityCode = "123";
			paymentApproval.AV_OH = orgHeader.PK;
			return paymentApproval;
		}

		public void TestCreditCardPaymentViaENett()
		{
			TestObjectCreator objectCreator = new TestObjectCreator(Factory);
			new AccountingPeriodTestHelper().SetupPeriods();

			bool originalComPayEnabled = AccountingConfigurationRegistry.Instance.EnableCreditCardPaymentsViaComPay.Value;

			try
			{
				AccountingConfigurationRegistry.Instance.EnableCreditCardPaymentsViaComPay.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				var header = Factory.NewWithValidTestData<AccGLHeader>();
				header.AG_AccountNum = "ZZAUDAcc";
				var bankAccount = objectCreator.CreateBankAccount("ZZHSBCAUD", "HSBC AUD ACCT", "HSBC", "AUD", objectCreator.AUD, "123456", "12345678", header);
				bankAccount.AB_DebitCreditCardExpiry = "0699";
				bankAccount.AB_DebitCreditCardName = "MR JOHN SMITH";
				var encoder = new TwoWayEncoder(bankAccount.PK.ToGuid());
				bankAccount.AB_DebitCreditCardNumber = encoder.Encrypt("1234567812345678");
				bankAccount.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.CCD;
				bankAccount.AB_AccountNum = "**** **** ***4 5678";

				OrgHeader orgHeader = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_IsActive, true));
				OrgCusCode cusCode = orgHeader.CustomsCodes.AddNew();
				cusCode.OK_CodeType = OrgCusCode.CodeTypes.eNettRegistrationNumber;
				cusCode.OK_CustomsRegNo = "123456";
				cusCode.OK_RN_NKCodeCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				AssertEquals("eNettRegistrationNumber", "123456", orgHeader.ENettRegistrationNumber);

				Factory.Save();

				APPaymentApprovalWithoutAuthorisation paymentApproval = GetAPPaymentApprovalWithoutAuthorisationForENett(bankAccount, orgHeader);

				eNettWebServiceWrapper.UseRealWebService_ForTesting = false;
				MockENettWebService.Instance.SetupForTesting("CARGOWISE");
				int initialInvokedCount = MockENettWebService.Instance.CountProcessCreditCardWasInvoked;

				using (PaymentApprovalForm payForm = new PaymentApprovalForm(paymentApproval))
				{
					try
					{
						AssertEquals("Payment Approval should NOT be in database", false, paymentApproval.IsInDatabase);
						AssertEquals("Payment should be null", null, paymentApproval.NewPayment);
						AssertEquals("Payment should not have errors: " + paymentApproval.NotificationsIncludingChildren.ToUniqueMessageListString(), false, paymentApproval.HasErrors);

						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						payForm.IsPostWithoutMatching_ForTestOnly = true;
						payForm.FireSaveButton();
						AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
						AssertEquals("Payment Approval should be in database", true, paymentApproval.IsInDatabase);
						AssertEquals("Payment should be in database", true, paymentApproval.NewPayment.IsInDatabase);
						AssertEquals("ProcessCreditCard should have been called", 1, MockENettWebService.Instance.CountProcessCreditCardWasInvoked - initialInvokedCount);
					}
					finally
					{
						if (ZFormModaliser.LastFormShownDialogForTest != null)
						{
							ZFormModaliser.LastFormShownDialogForTest.Dispose();
						}
						FindAndDisposeOpenPaymentForm();
					}
				}

				paymentApproval = GetAPPaymentApprovalWithoutAuthorisationForENett(bankAccount, orgHeader);
				MockENettWebService.Instance.SetupForTesting("BOGUS");

				using (PaymentApprovalForm payForm = new PaymentApprovalForm(paymentApproval))
				{
					try
					{
						AssertEquals("Payment Approval should NOT be in database", false, paymentApproval.IsInDatabase);
						AssertEquals("Payment should be null", null, paymentApproval.NewPayment);

						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						payForm.IsPostWithoutMatching_ForTestOnly = true;
						payForm.FireSaveButton();
						AssertEquals("Error making Credit Card payment over ComPay.  Please try again.\r\nComPay Error: (100) Invalid Integrator Details", UnitTestUserNotification.Instance.LastMessage.Text);
						AssertEquals("Payment Approval should NOT be in database", false, paymentApproval.IsInDatabase);
						AssertEquals("Payment should NOT be in database", false, paymentApproval.NewPayment.IsInDatabase);
						AssertEquals("ProcessCreditCard should have been called", 2, MockENettWebService.Instance.CountProcessCreditCardWasInvoked - initialInvokedCount);
					}
					finally
					{
						if (ZFormModaliser.LastFormShownDialogForTest != null)
						{
							ZFormModaliser.LastFormShownDialogForTest.Dispose();
						}
						FindAndDisposeOpenPaymentForm();
					}
				}
			}
			finally
			{
				AccountingConfigurationRegistry.Instance.EnableCreditCardPaymentsViaComPay.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, originalComPayEnabled);
			}
		}

#endregion

		public void TestAddressesOnPayments()
		{
			AccountingConfigurationRegistry.Instance.EditPaymentAddress.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			using (var form = (PaymentApprovalForm)GetFormToBashCore())
			{
				form.Show();
				AssertEquals(false, form.Payment_ForTestOnly.AV_OA_AddressOverrideInfo.ReadOnly);
				AssertEquals(false, form.Payment_ForTestOnly.AV_OC_ContactOverrideInfo.ReadOnly);
			}

			AccountingConfigurationRegistry.Instance.EditPaymentAddress.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			using (var form = (PaymentApprovalForm)GetFormToBashCore())
			{
				form.Show();
				AssertEquals(true, form.Payment_ForTestOnly.AV_OA_AddressOverrideInfo.ReadOnly);
				AssertEquals(true, form.Payment_ForTestOnly.AV_OC_ContactOverrideInfo.ReadOnly);
			}
		}

		public void TestPaymentExchangeRateWhenClickPaymentDetail()
		{
			var objectCreator = new TestObjectCreator(Factory);
			new AccountingPeriodTestHelper().SetupPeriods();
			var paymentApproval = Factory.NewWithValidTestData<APPaymentApprovalWithoutAuthorisation>();
			paymentApproval.AV_OH = TestOrg.PK;
			paymentApproval.AV_PaymentType = ReceiptTypes.Cash;
			paymentApproval.AV_AB = objectCreator.USDBankAccount.PK;
			paymentApproval.AV_Amount = 100m;
			paymentApproval.ExchangeRate.Currency = "USD";
			paymentApproval.ExchangeRate.Rate = 0.84m;

			using (MockPaymentApprovalForm payApproveForm = new MockPaymentApprovalForm(paymentApproval))
			{
				payApproveForm.Show();
				payApproveForm.PaymentDetailButton_ForTestOnly.PerformClick();
				AssertEquals("should not be changed", 0.84m, paymentApproval.ExchangeRate.Rate);
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

		public void TestEventsWhenPaymentIsNull()
		{
			var formEvents = typeof(PaymentApprovalForm).GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
				.Where(e => {
					var inputParams = e.GetParameters();
					return inputParams.Length == 2
						&& inputParams[0].ParameterType.FullName == "System.Object" && inputParams[0].Name == "sender"
						&& inputParams[1].ParameterType.FullName == "System.EventArgs" && inputParams[1].Name == "e";
				})
				.ToArray();

			using (var payApproveForm = new MockPaymentApprovalForm())
			{
				payApproveForm.Show();
				AssertNull("Precondition", payApproveForm.Payment);

				var eventsExcluded = new string[] {
					"fMatchingForm_Closed",
					"FMatchingForm_Closed_ForTestOnly",
				};
				var eventsThrowExceptionWhenPaymentIsNull = new List<MethodInfo>();
				foreach (var formEvent in formEvents)
				{
					try
					{
						formEvent.Invoke(payApproveForm, new object[] { payApproveForm, EventArgs.Empty });
					}
					catch
					{
						if (!eventsExcluded.Contains(formEvent.Name))
						{
							eventsThrowExceptionWhenPaymentIsNull.Add(formEvent);
						}
					}
				}
				ExceptionReporterTestListener.Instance.Clear();

				var suspender = new FunctionalitySuspender();
				payApproveForm.SetSuspendReiterantEventsDueToDangerousApplicationDoEvents_ForTestOnly(suspender);
				var eventsStillThrowException = new List<MethodInfo>();
				using (suspender.GetSuspender())
				{
					foreach (var formEvent in eventsThrowExceptionWhenPaymentIsNull)
					{
						try
						{
							formEvent.Invoke(payApproveForm, new object[] { payApproveForm, EventArgs.Empty });
						}
						catch
						{
							eventsStillThrowException.Add(formEvent);
						}
					}
				}

				AssertEquals($@"Event can be processed with null Payment after form closed, because of fMatchingForm_Closed --> Application.DoEvents. Please check whether events below that should be suspended:
{string.Join(System.Environment.NewLine, eventsStillThrowException.Select(methodInfo => methodInfo.Name))}"
					, 0, eventsStillThrowException.Count);
			}
		}

		public void TestEventSuspendWhenMatchingFormClosing()
		{
			var formEvents = typeof(PaymentApprovalForm).GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
				.Where(e => {
					var inputParams = e.GetParameters();
					return inputParams.Length == 2
						&& inputParams[0].ParameterType.FullName == "System.Object" && inputParams[0].Name == "sender"
						&& inputParams[1].ParameterType.FullName == "System.EventArgs" && inputParams[1].Name == "e";
				});

			var eventsShouldSuspendWhenMatchingFormClosing = new string[] {
				"PostWithoutMatchingButton_Click",
				"PaymentDetailButton_Click",
				"SaveAsDraftButton_Click",
				"CheckEPayRateButton_Click",
				"RefreshEPaymentButton_Click",
				"AcceptQuoteButton_Click",
				"ProcessEPaymentButton_Click",
				"OnPostButtonClick",
				"OnApplyButtonClick",
				"AV_PaymentTypeInfo_ValueChanged",
				"AV_RX_NKPaymentCurrencyInfo_ValueChanged",
				"AV_ABInfo_ValueChanged",
				"LearnMoreButton_Click",
				"ProviderLogoPictureBox_Click",
				"OnSubmitForApprovalButton_Click",
				"OnApproveForPostingButton_Click"
			};

			var eventsNotAboutMatchingFormClosing = new string[] {
				"ChequeLinkForm_Closed",
				"WorkflowTabPage_InitializeTab",
				"fMatchingForm_Closed",
				"FMatchingForm_Closed_ForTestOnly",
				"ProviderLogoPictureBox_Click_ForTestOnly",
#if !WINZOR
				"OnInvokedSetScrollPosition", // Winzor does not support this event
#endif
			};

			AssertContainsExactElementsInAnyOrder(
				@"Event can be processed with null Payment after form closed, because of fMatchingForm_Closed --> Application.DoEvents. Please check whether new events should be suspended when close form, and add event name into proper list.
[Event method which use Payment variable](1)Add SuspendReiterantEventsDueToDangerousApplicationDoEvents at the beginning of event method. (2)Add event method name to list eventsShouldSuspendWhenMatchingFormClosing.
[Event method which does NOT use Payment variable](1)Add method name to list eventsNotAboutMatchingFormClosing."
				, eventsShouldSuspendWhenMatchingFormClosing.Concat(eventsNotAboutMatchingFormClosing)
				, formEvents.Select(e => e.Name).ToArray());

			using (var payApproveForm = new MockPaymentApprovalForm())
			{
				var isSuspenderCalled = false;
				var suspender = new FunctionalitySuspender(() => isSuspenderCalled = true, true);
				payApproveForm.SetSuspendReiterantEventsDueToDangerousApplicationDoEvents_ForTestOnly(suspender);
				foreach (var formEvent in formEvents.Where(e => eventsShouldSuspendWhenMatchingFormClosing.Contains(e.Name)))
				{
					isSuspenderCalled = false;
					using (suspender.GetSuspender())
					{
						formEvent.Invoke(payApproveForm, new object[] { payApproveForm, EventArgs.Empty });
					}
					AssertEquals($"Suspender should be called in {formEvent.Name}", true, isSuspenderCalled);
				}
			}
		}

		public void TestNoCriticalValidationForDraftedPaymentMatching()
		{
			AccountingConfigurationRegistry.Instance.APMatchingSessionControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty);
			SetupDataForTest();
			var shipment = TestObjectCreator.CreateShipment("S1");
			var job = TestObjectCreator.CreateJob(shipment);
			var testAPInv = Factory.NewWithValidTestData<APInvoice>();
			testAPInv.AH_OH = TestOrg.PK;
			var genericChargeCodeGuid = TestObjectCreator.CC1.PK;

			var line = TestObjectCreator.CreateInvoiceLine(testAPInv, testAPInv.TransactionCurrency, testAPInv.AH_ExchangeRate, 200m, 0m, 0m, 200m, 0m, 0m, genericChargeCodeGuid);
			line.AL_JH = job.PK;
			var charge = TestObjectCreator.CreateJobCharge(line, job, TestObjectCreator.CC1, testAPInv.TransactionCurrency);
			testAPInv.AH_FullyPaidDate = ZDateTime.Empty;
			Factory.Save();

			var payment = GetNewTestAPPaymentApprovalThatPostsOnSaving(TestOrg, TestBank, ReceiptTypes.Cheque, TestCheques, "2", 200M);
			payment.AV_RX_NKPaymentCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			payment.AV_PayExRate = 1;

			using (var paymentForm = new MockPaymentApprovalForm(payment))
			{
				PaymentDocumentsPrintPopup exposedPrintForm = null;
				NewMatchGroupForm matchingForm = null;

				try
				{
					paymentForm.Show();
					Application.DoEvents();
					paymentForm.SaveAsDraftButton_ForTestOnly.PerformClick();
					AssertEquals(PaymentApprovalStatus.Draft, payment.AV_Status);
					paymentForm.PaymentDetailButton_ForTestOnly.PerformClick();
					matchingForm = ZFormModaliser.ActiveForm as NewMatchGroupForm;
					var matchingBizO = matchingForm.BusinessEntity as APPaymentApprovalMatching;
					matchingBizO.MoveAllFromUnmatchToMatch();
					AssertEquals("Should be 2 transactions in MatchedTransactions", 2, matchingBizO.MatchedTransactions.Count);
					AssertEquals("Balance should be 0", 0M, matchingBizO.Balance);

					UnitTestUserNotification.Instance.ClearMessages();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					AssertNoExceptionThrown(() => matchingForm.MatchAndCloseButton.PerformClick());

					exposedPrintForm = ((PaymentPrintManager.TestPaymentPrintManager)paymentForm.PrintManager_ForTestOnly).LastPrintForm;
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
					FindAndDisposeOpenPaymentForm();
				}
			}
		}

		public void TestAuditTabShouldBeInPaymentApprovalForm()
		{
			SetupDataForTest();
			var bankAccount = TestObjectCreator.CreateEPaymentBankAccount();
			var payment = TestObjectCreator.GetNewTestAPPaymentApproval(TestObjectCreator.AALSHI, bankAccount, ReceiptTypes.EPayment, "2", 200m, TestCheques);

			using (var form = new PaymentApprovalForm(payment))
			{
				AssertEquals("Audit plugIn should be added", true, form.PlugIns.IsPlugInAvailable(ControllerIDs.Audit));
			}
		}

		public void TestPaymentApprovalFormBorderStyle()
		{
			SetupDataForTest();
			var bankAccount = TestObjectCreator.CreateEPaymentBankAccount();
			var payment = TestObjectCreator.GetNewTestAPPaymentApproval(TestObjectCreator.AALSHI, bankAccount, ReceiptTypes.EPayment, "2", 200m, TestCheques);

			using (var form = new PaymentApprovalForm(payment))
			{
				AssertEquals("Should be the default value", FormBorderStyle.Sizable, form.FormBorderStyle);
			}
		}

		#region Implementation

		OrgHeader TestOrg;
		AccountingPeriodTestHelper PeriodHelper;
		AccBankAccount TestBank;
		AccChequeBook TestCheques;

		protected override void SetUp()
		{
			base.SetUp();

			TestOrg = Factory.NewWithValidTestData<OrgHeader>();
			TestOrg.OH_Code = Accounting.Business.TestObjectCreator.GetRandomString(10);
			TestOrg.CompanyData.OB_IsCreditor = true;
			TestOrg.CompanyData.SetAPTaxApplicable(false);
			Factory.Save();
			MockENettWebService.ClearInstance();
		}

		protected override void TearDown()
		{
			base.TearDown();
			MockENettWebService.ClearInstance();
		}

		protected AccChequeBook GetChequeBook(AccBankAccount bankAccount, ZDecimal startNO, ZDecimal lastNO, ZDecimal currentNO, bool isAutoPrint)
		{
			bankAccount.AB_ChequeNumDigits = 1;

			BusinessObject printQueue = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.DocumentEngine.IStmPrintQueue)));
			AccChequeBook chequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			chequeBook.AK_AB = bankAccount.PK;
			chequeBook.AK_SQ = printQueue.PK;
			chequeBook.AK_StartNo = startNO;
			chequeBook.AK_LastNo = lastNO;
			chequeBook.AK_CurrentNo = currentNO;

			if (isAutoPrint)
			{
				bankAccount.AB_SO_ChequeTemplate = TestObjectCreator.StandardTemplatePK;
				chequeBook.AK_AutoPrintCheque = ZBool.True;
				AssertEquals("Cheque Book should be AutoPrint", true, chequeBook.IsAutoPrint);
			}
			else
			{
				AssertEquals("Cheque Book should NOT be AutoPrint", false, chequeBook.IsAutoPrint);
			}

			Factory.Save();
			return chequeBook;
		}

		protected AccChequeBook GetAutoPrintChequeBook(AccBankAccount bankAccount, ZDecimal startNO, ZDecimal lastNO, ZDecimal currentNO)
		{
			return GetChequeBook(bankAccount, startNO, lastNO, currentNO, true);
		}

		protected override Form GetFormToBashCore()
		{
			PaymentApprovalWithAuthorisation testPaymentApproval = Factory.New<APPaymentApprovalWithAuthorisation>();
			testPaymentApproval.AV_OH = TestOrg.PK;
			testPaymentApproval.HasChanges = false;

			var result = new PaymentApprovalForm(testPaymentApproval);
			result.ControllerID = ControllerIDs.APPaymentProcessing;
			return result;
		}

		void SetupDataForTest()
		{
			PeriodHelper = new AccountingPeriodTestHelper(Factory);
			PeriodHelper.SetupPeriods();

			TestBank = Factory.NewWithValidTestData<AccBankAccount>();
			TestCheques = Factory.NewWithValidTestData<AccChequeBook>();
			TestCheques.AK_StartNo = 1;
			TestCheques.AK_LastNo = 100;
			TestCheques.AK_CurrentNo = 1;
			TestCheques.AK_AB = TestBank.PK;

			Factory.Save();
		}

		APPaymentApprovalWithoutAuthorisation GetNewTestAPPaymentApprovalThatPostsOnSaving(OrgHeader org, AccBankAccount bank, ZString receiptType, AccChequeBook chequeBook, ZString chequeOrRef, ZDecimal amount)
			=> GetNewTestAPPaymentApprovalThatPostsOnSaving<APPaymentApprovalWithoutAuthorisation>(org, bank, receiptType, chequeBook, chequeOrRef, amount);

		APPaymentApprovalWithoutAuthorisation GetNewTestAPPaymentApprovalThatPostsOnSaving<T>(OrgHeader org, AccBankAccount bank, ZString receiptType, AccChequeBook chequeBook, ZString chequeOrRef, ZDecimal amount)
			where T : APPaymentApprovalWithoutAuthorisation
		{
			var paymentApproval = Factory.New<T>();
			paymentApproval.AV_OH = org.PK;
			paymentApproval.AV_AB = bank.PK;
			paymentApproval.AV_PaymentType = receiptType;
			paymentApproval.AV_AK = chequeBook.PK;
			paymentApproval.AV_ChequeOrReference = chequeOrRef;
			paymentApproval.AV_Amount = amount;
			return paymentApproval;
		}

		bool FindAndDisposeOpenPaymentForm()
		{
			foreach (Form form in Application.OpenForms)
			{
				if (form is PaymentForm)
				{
					form.Dispose();
					return true;
				}
			}

			return false;
		}

		TestObjectCreator fTestObjectCreator;
		TestObjectCreator TestObjectCreator
		{
			get { return fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(Factory)); }
		}

#region Test Classes

		public class MockPaymentApprovalForm : PaymentApprovalForm
		{
			public MockPaymentApprovalForm(PaymentApprovalBase paymentApprovalBizO)
				: base(paymentApprovalBizO)
			{
			}

			public MockPaymentApprovalForm()
				: base()
			{
			}

			public new PaymentApprovalBase Payment => base.Payment;

			protected override PaymentPrintManager PrintManager
			{
				get
				{
					if (fTestPrintManager == null)
					{
						fTestPrintManager = new PaymentPrintManager.TestPaymentPrintManager(Payment.TransactionHeader.PK.ToGuid(), TransactionTypes.Payment, BusinessEntity.Factory);
					}

					return fTestPrintManager;
				}
			}

			PaymentPrintManager.TestPaymentPrintManager fTestPrintManager;
		}

		class MockAPPaymentApprovalWithoutAuthorisation : APPaymentApprovalWithoutAuthorisation
		{
			public MockAPPaymentApprovalWithoutAuthorisation(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			protected override void OnFactorySavingBeforeTransactionCore()
			{
				throw new CannotSaveAfterCriticalErrorException("Simulate critical validation error");
			}
		}

		public class MockPaymentApprovalForm_ForCloseFormTest : PaymentApprovalForm
		{
			public MockPaymentApprovalForm_ForCloseFormTest(PaymentApprovalBase paymentApproval) : base(paymentApproval)
			{
				Closed += PaymentFormClosed;
			}

			void PaymentFormClosed(object sender, EventArgs e)
			{
				var propertyInfo = typeof(ZForm).GetProperty("IsEnabledCore", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
				IsFormEnabledWhenClose_ForTestOnly = (bool)propertyInfo.GetValue(this);
			}

			public bool IsFormEnabledWhenClose_ForTestOnly { get; private set; }

			protected override PaymentPrintManager PrintManager
			{
				get
				{
					if (fTestPrintManager == null)
					{
						fTestPrintManager = new MockPaymentPrintManager(Payment.TransactionHeader.PK.ToGuid(), TransactionTypes.Payment, BusinessEntity.Factory);
					}

					return fTestPrintManager;
				}
			}
			PaymentPrintManager fTestPrintManager;

			class MockPaymentPrintManager : PaymentPrintManager
			{
				public MockPaymentPrintManager(Guid pk, string transactionType, BusinessObjectFactory factory) : base(pk, transactionType, factory)
				{
				}

				public override void Print()
				{
					throw new ReportException("Simulate report exception when printing");
				}
			}
		}

#endregion

#endregion
	}
}
