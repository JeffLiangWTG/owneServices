using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows.Forms;
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
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.GUI.ARAP.Payment;
using Enterprise.Accounting.GUI.ARAP.ReceiptPayment;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using AuthorisationCodes = Enterprise.Registry.Business.AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes;
using DealStatusCodes = Enterprise.MasterFiles.Business.EPaymentStatusCodes.Deal;
using QuoteStatusCodes = Enterprise.MasterFiles.Business.EPaymentStatusCodes.Quote;
using RangeCodes = Enterprise.Registry.Business.AmountBasedMultiLevelAuthorisationRequirement.RangeCodes;

namespace Enterprise.Accounting.GUI.ARAP.Testing
{
	[TestedType(typeof(PaymentApprovalWithAuthorisationForm))]
	public class PaymentApprovalWithAuthorisationFormTestCase : ZFormBasherTest
	{
		public void TestPaymentReasonVisibility()
		{
			SetupDataForTest();
			var bankAccount = TestObjectCreator.CreateEPaymentBankAccount();
			var payment = GetNewTestAPPaymentApproval(TestObjectCreator.AALSHI, bankAccount, ReceiptTypes.EPayment, TestCheques, "2", 200m);
			using (var paymentForm = new PaymentApprovalWithAuthorisationForm(payment))
			{
				paymentForm.Show();
				Application.DoEvents();

				Assert(payment.IsEPayment);
				var paymentReasonDropEdit = paymentForm.Controls.Find("PaymentReasonDropEdit", true).First() as ZDropEdit;
				Assert(paymentReasonDropEdit.Visible);

				payment.AV_PaymentType = ReceiptTypes.Cheque;
				Assert(!paymentReasonDropEdit.Visible);
			}
		}

		public void TestDisclaimerMessage()
		{
			SetupDataForTest();
			var bankAccount = TestObjectCreator.CreateEPaymentBankAccount();
			var payment = GetNewTestAPPaymentApproval(TestObjectCreator.AALSHI, bankAccount, ReceiptTypes.EPayment, TestCheques, "2", 200m);

			using (var form = new PaymentApprovalWithAuthorisationForm(payment))
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

		public void TestEPaymentProviderLogoAndServiceProviderLabel()
		{
			SetupDataForTest();
			var payment = GetNewTestAPPaymentApproval(TestObjectCreator.AALSHI, TestBank, ReceiptTypes.EPayment, TestCheques, "2", 200m);

			using (var form = new PaymentApprovalWithAuthorisationForm(payment))
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
			var payment = GetNewTestAPPaymentApproval(TestObjectCreator.AALSHI, bankAccount, ReceiptTypes.Cheque, TestCheques, "2", 200m);
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
			using (var paymentForm = new PaymentApprovalWithAuthorisationForm(payment))
			{
				paymentForm.Show();
				Application.DoEvents();

				UnitTestUserNotification.Instance.ClearMessages();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				paymentForm.ProcessEPaymentButton_ForTestOnly.PerformClick();
				AssertEquals("Your staff profile is not currently registered in the list of users who can use this Bank Account. To manage users, edit the Bank Account and select “Manage Users”.", UnitTestUserNotification.Instance.LastMessage.Text);

				paymentForm.CheckExRateButton_ForTestOnly.PerformClick();
				AssertEquals("Your staff profile is not currently registered in the list of users who can use this Bank Account. To manage users, edit the Bank Account and select “Manage Users”.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestProcessEPaymentButton_Click_WhenAccountIsEpaAndUserIsUnauthorized()
		{
			SetupDataForTest();
			var bankAccount = TestObjectCreator.CreateEPaymentBankAccount();
			var payment = GetNewTestAPPaymentApproval(TestObjectCreator.AALSHI, bankAccount, ReceiptTypes.Cheque, TestCheques, "2", 200m);
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
			using (var paymentForm = new PaymentApprovalWithAuthorisationForm(payment))
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

				paymentForm.CheckExRateButton_ForTestOnly.PerformClick();
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
			var payment = GetNewTestAPPaymentApproval(TestObjectCreator.AALSHI, bankAccount, ReceiptTypes.Cheque, TestCheques, "2", 200m);
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
			using (var paymentForm = new PaymentApprovalWithAuthorisationForm(payment))
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

				paymentForm.CheckExRateButton_ForTestOnly.PerformClick();
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
			var payment = GetNewTestAPPaymentApproval(TestObjectCreator.AALSHI, bankAccount, ReceiptTypes.Cheque, TestCheques, "2", 200m);
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
			using (var paymentForm = new PaymentApprovalWithAuthorisationForm(payment))
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

				ZTabControl tabControl = (ZTabControl)paymentForm.Controls.Find("TabControlBankAndEPayment", true).First();
				AssertEquals("After click, E-Payment tab page should be focused", 2, tabControl.SelectedIndex);

				Assert(!paymentForm.SaveAsDraftButton_ForTestOnly.Enabled);
				Assert(!paymentForm.PostingButtonsUserControl_ForTestOnly.SaveButton.Enabled);
				Assert(!paymentForm.PostingButtonsUserControl_ForTestOnly.SaveAndCloseButton.Enabled);
			}
		}

		public void TestProcessEPaymentButton_Click_WhenPaymentHasAcceptedQuoteAndPaymentDetailsAreDifferent()
		{
			SetupDataForTest();
			var bankAccount = TestObjectCreator.CreateEPaymentBankAccount();
			var payment = GetNewTestAPPaymentApproval(TestObjectCreator.AALSHI, bankAccount, ReceiptTypes.Cheque, TestCheques, "2", 200m);
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
			using (var paymentForm = new PaymentApprovalWithAuthorisationForm(payment))
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
			var payment = GetNewTestAPPaymentApproval(TestObjectCreator.AALSHI, bankAccount, ReceiptTypes.Cheque, TestCheques, "2", 200m);
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
			using (var paymentForm = new PaymentApprovalWithAuthorisationForm(payment))
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
			var payment = GetNewTestAPPaymentApproval(TestObjectCreator.AALSHI, bankAccount, ReceiptTypes.Cheque, TestCheques, "2", 200m);
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
			using (var paymentForm = new PaymentApprovalWithAuthorisationForm(payment))
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
			var payment = GetNewTestAPPaymentApproval(TestObjectCreator.AALSHI, TestBank, ReceiptTypes.Cheque, TestCheques, "2", 200m);
			payment.AV_RX_NKPaymentCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			payment.AV_Status = PaymentApprovalStatus.FullyApproved;
			Factory.Save();
			var quote = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Received, payment);
			Factory.Save();

			AssertNotEquals(AccountTypeCodeDescriptionPairList.Codes.EPA, payment.BankAccount.AB_AccountType);
			var deals = Factory.Load<AccEPaymentDeal>(new ZQuery(AccEPaymentDealSchema.AED_QU_Quote, quote.PK));
			AssertEquals(0, deals.Length);

			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			using (var paymentForm = new PaymentApprovalWithAuthorisationForm(payment))
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
			var bankAccount = TestObjectCreator.CreateEPaymentBankAccount();
			var payment = GetNewTestAPPaymentApproval(TestObjectCreator.AALSHI, bankAccount, ReceiptTypes.EPayment, TestCheques, "2", 200m);

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
			var payment = GetNewTestAPPaymentApproval(TestObjectCreator.AALSHI, TestBank, ReceiptTypes.Cheque, TestCheques, "2", 200m);
			payment.AV_RX_NKPaymentCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			payment.AV_Status = PaymentApprovalStatus.AwaitingApproval;
			Factory.Save();
			var quote = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Received, payment);
			var utcTime = quote.QU_LastResponseReceivedUtc;
			var localTime = utcTime.ToLocalBranchTime();
			Factory.Save();
			payment.PaymentQuotes.Reload(true);
			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			using (var paymentForm = new PaymentApprovalWithAuthorisationForm(payment))
			{
				paymentForm.Show();
				Application.DoEvents();

				var bankAndEPaymentTabControl = paymentForm.Controls.Find("TabControlBankAndEPayment", true).First() as ZTabControl;
				AssertNotNull(bankAndEPaymentTabControl);
				bankAndEPaymentTabControl.SelectTab("EPaymentTab");

				AssertEquals(1, payment.PaymentQuotes_ForDisplay.Count);
				AssertEquals(localTime, payment.PaymentQuotes_ForDisplay[0].LastResponseReceivedLocalTime);
			}
		}

		[TestDate(2021, 8, 26, 22, 00, 00)]
		public void TestDisplayDealTimeInLocalTimezone()
		{
			SetupDataForTest();
			var payment = GetNewTestAPPaymentApproval(TestObjectCreator.AALSHI, TestBank, ZArchitecture.Core.ReceiptTypes.Cheque, TestCheques, "2", 200m);
			payment.AV_RX_NKPaymentCurrency = "USD";
			var quote = payment.TryToCreateQuote(EPaymentProviderCodes.Codes.OFX).Quote;
			TestObjectCreator.CreateValidEPaymentDealForStatus(DealStatusCodes.Accepted, payment, quote);
			Factory.Save();

			var localTime = payment.AV_SystemCreateTimeUtc.ToLocalBranchTime();

			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			using (var paymentForm = new PaymentApprovalWithAuthorisationForm(payment))
			{
				paymentForm.Show();
				Application.DoEvents();
				AssertEquals(localTime, payment.DealSubmittedLocalTime);
				AssertEquals(localTime, payment.DealLastResponseLocalTime);

				var bankAndEPaymentTabControl = paymentForm.Controls.Find("TabControlBankAndEPayment", true).First() as ZTabControl;
				AssertNotNull(bankAndEPaymentTabControl);
				bankAndEPaymentTabControl.SelectTab("EPaymentTab");

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
			var payment = GetNewTestAPPaymentApproval(TestObjectCreator.AALSHI, TestBank, ReceiptTypes.Cheque, TestCheques, "2", 200m);
			payment.AV_RX_NKPaymentCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			payment.AV_Status = PaymentApprovalStatus.AwaitingApproval;
			Factory.Save();
			var quote = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Received, payment);
			Factory.Save();

			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			using (var paymentForm = new PaymentApprovalWithAuthorisationForm(payment))
			{
				paymentForm.Show();
				Application.DoEvents();

				var bankAndEPaymentTabControl = paymentForm.Controls.Find("TabControlBankAndEPayment", true).First() as ZTabControl;
				AssertNotNull(bankAndEPaymentTabControl);
				bankAndEPaymentTabControl.SelectTab("EPaymentTab");

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
			var payment = GetNewTestAPPaymentApproval(TestObjectCreator.AALSHI, TestBank, ReceiptTypes.Cheque, TestCheques, "2", 200m);
			payment.AV_RX_NKPaymentCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			payment.AV_Status = PaymentApprovalStatus.AwaitingApproval;
			Factory.Save();
			var quote = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Received, payment);
			Factory.Save();
			payment.PaymentQuotes.Reload(true);

			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			using (var paymentForm = new PaymentApprovalWithAuthorisationForm(payment))
			{
				paymentForm.Show();
				Application.DoEvents();

				var bankAndEPaymentTabControl = paymentForm.Controls.Find("TabControlBankAndEPayment", true).First() as ZTabControl;
				AssertNotNull(bankAndEPaymentTabControl);
				bankAndEPaymentTabControl.SelectTab("EPaymentTab");

				var epaymentGrid = paymentForm.Controls.Find("zGrid1", true).First() as ZGrid;
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
			var payment = GetNewTestAPPaymentApproval(TestObjectCreator.AALSHI, TestBank, ReceiptTypes.Cheque, TestCheques, "2", 200m);
			payment.AV_RX_NKPaymentCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			payment.AV_Status = PaymentApprovalStatus.AwaitingApproval;
			Factory.Save();
			var quote1 = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Received, payment);
			var quote2 = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Requested, payment);
			Factory.Save();
			payment.PaymentQuotes.Reload(true);

			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			using (var paymentForm = new PaymentApprovalWithAuthorisationForm(payment))
			{
				paymentForm.Show();
				Application.DoEvents();

				var bankAndEPaymentTabControl = paymentForm.Controls.Find("TabControlBankAndEPayment", true).First() as ZTabControl;
				AssertNotNull(bankAndEPaymentTabControl);
				bankAndEPaymentTabControl.SelectTab("EPaymentTab");

				var epaymentGrid = paymentForm.Controls.Find("zGrid1", true).First() as ZGrid;
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
			var payment = GetNewTestAPPaymentApproval(TestObjectCreator.AALSHI, TestBank, ReceiptTypes.Cheque, TestCheques, "2", 200m);
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
			var paymentInFormFactory = formFactory.LoadTop1<PaymentApprovalWithAuthorisation>(new ZQuery(AccPaymentApprovalSchema.PK, payment.PK));

			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			using (var paymentForm = new PaymentApprovalWithAuthorisationForm(paymentInFormFactory))
			{
				paymentForm.Show();
				Application.DoEvents();

				var bankAndEPaymentTabControl = paymentForm.Controls.Find("TabControlBankAndEPayment", true).First() as ZTabControl;
				AssertNotNull(bankAndEPaymentTabControl);
				bankAndEPaymentTabControl.SelectTab("EPaymentTab");

				var epaymentGrid = paymentForm.Controls.Find("zGrid1", true).First() as ZGrid;
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
			var payment = GetNewTestAPPaymentApproval(TestObjectCreator.AALSHI, TestBank, ReceiptTypes.Cheque, TestCheques, "2", 200m);
			payment.AV_RX_NKPaymentCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			payment.AV_Status = PaymentApprovalStatus.AwaitingApproval;
			Factory.Save();
			var quote1 = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Queued, payment);
			var quote2 = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Requested, payment);
			Factory.Save();
			payment.PaymentQuotes.Reload(true);

			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			using (var paymentForm = new PaymentApprovalWithAuthorisationForm(payment))
			{
				paymentForm.Show();
				Application.DoEvents();

				var bankAndEPaymentTabControl = paymentForm.Controls.Find("TabControlBankAndEPayment", true).First() as ZTabControl;
				AssertNotNull(bankAndEPaymentTabControl);
				bankAndEPaymentTabControl.SelectTab("EPaymentTab");

				var epaymentGrid = paymentForm.Controls.Find("zGrid1", true).First() as ZGrid;
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

		#region Draft Payment Approval

		public void TestSaveAsDraftButtonWillShow_NotInDB()
		{
			SetupDataForTest();
			var approval = GetNewTestAPPaymentApproval(TestOrg, TestBank, ReceiptTypes.Cheque, TestCheques, "2", 94M);
			using (PaymentApprovalWithAuthorisationForm form = new PaymentApprovalWithAuthorisationForm(approval))
			{
				form.Show();
				Application.DoEvents();

				AssertEquals(false, approval.IsInDatabase);
				AssertEquals(true, form.SaveAsDraftButton_ForTestOnly.Visible);
				AssertEquals(true, form.SaveAsDraftButton_ForTestOnly.Enabled);
			}
		}

		public void TestSaveAsDraftButtonWillShow_DraftApproval_InDB()
		{
			SetupDataForTest();
			var approval = GetNewTestAPPaymentApproval(TestOrg, TestBank, ReceiptTypes.Cheque, TestCheques, "2", 94M);
			approval.AV_Status = PaymentApprovalStatus.Draft;
			Factory.Save();

			ZController controller = ZControllerFactory.Create(ControllerIDs.APPaymentProcessing);
			using (PaymentApprovalWithAuthorisationForm form = controller.ShowEditForm(approval) as PaymentApprovalWithAuthorisationForm)
			{
				form.Show();
				Application.DoEvents();

				AssertEquals(true, approval.IsInDatabase);
				AssertEquals(true, approval.IsDraft);

				AssertEquals(false, form.SaveAsDraftButton_ForTestOnly.Visible);
				AssertEquals(false, form.SaveAsDraftButton_ForTestOnly.Enabled);
				AssertEquals(true, form.PostingButtonsUserControl_ForTestOnly.SaveAndCloseButton.Visible);
				AssertEquals(false, form.PostingButtonsUserControl_ForTestOnly.SaveAndCloseButton.Enabled);

				(form.BusinessEntity as PaymentApprovalWithAuthorisation).AV_Amount = 95m;

				AssertEquals(true, form.SaveAsDraftButton_ForTestOnly.Visible);
				AssertEquals(true, form.SaveAsDraftButton_ForTestOnly.Enabled);
				AssertEquals(true, form.PostingButtonsUserControl_ForTestOnly.SaveAndCloseButton.Visible);
				AssertEquals(true, form.PostingButtonsUserControl_ForTestOnly.SaveAndCloseButton.Enabled);
			}
		}

		public void TestSaveAsDraftButtonWillNotShow_PostedApproval_InDB()
		{
			SetupDataForTest();
			var approval = GetNewTestAPPaymentApproval(TestOrg, TestBank, ReceiptTypes.Cheque, TestCheques, "2", 94M);
			approval.AV_Status = PaymentApprovalStatus.Posted;
			Factory.Save();

			ZController controller = ZControllerFactory.Create(ControllerIDs.APPaymentProcessing);
			using (PaymentApprovalWithAuthorisationForm form = controller.ShowEditForm(approval) as PaymentApprovalWithAuthorisationForm)
			{
				form.Show();
				Application.DoEvents();

				AssertEquals(true, approval.IsInDatabase);
				AssertEquals(false, form.SaveAsDraftButton_ForTestOnly.Visible);
				AssertEquals(false, form.SaveAsDraftButton_ForTestOnly.Enabled);
			}
		}

		public void TestClickAuthorisationOrRejectForDraftPaymentApproval()
		{
			SetupDataForTest();
			var approval = GetNewTestAPPaymentApproval(TestOrg, TestBank, ReceiptTypes.Cheque, TestCheques, "2", 94M);
			approval.AV_Status = PaymentApprovalStatus.Draft;

			using (PaymentApprovalWithAuthorisationForm form = new PaymentApprovalWithAuthorisationForm(approval))
			{
				form.Show();
				Application.DoEvents();

				string exceptedMessage = PaymentApprovalWithAuthorisation.DealWithDraftPaymentErrorMessage;

				UnitTestUserNotification.Instance.ClearMessages();
				form.FirstAuthorisationButton_Click_ForTestOnly(null, new EventArgs());
				AssertEquals(exceptedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessages();
				form.SecondAuthorisationButton_Click_ForTestOnly(null, new EventArgs());
				AssertEquals(exceptedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessages();
				form.ThirdAuthorisationButton_Click_ForTestOnly(null, new EventArgs());
				AssertEquals(exceptedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessages();
				form.RejectButton_Click_ForTestOnly(null, new EventArgs());
				AssertEquals(exceptedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSaveAsDraftSuccessfully_OnPaymentApprovalWithAuthorisationForm()
		{
			SetupDataForTest();
			var approval = GetNewTestAPPaymentApproval(TestOrg, TestBank, ReceiptTypes.Cheque, TestCheques, "2", 94M);

			using (PaymentApprovalWithAuthorisationForm form = new PaymentApprovalWithAuthorisationForm(approval))
			{
				form.Show();
				Application.DoEvents();

				Assert(!approval.IsInDatabase);
				AssertNotEquals(approval.AV_Status, PaymentApprovalStatus.Draft);

				//Click Save As Draft, save Draft Successuflly, and form does not close
				form.SaveAsDraftButton_ForTestOnly.PerformClick();
				AssertEquals(false, form.IsDisposed);
				AssertEquals(true, approval.IsInDatabase);
				AssertEquals(true, approval.IsDraft);
				AssertEquals("00001000", approval.AV_PaymentApprovalReference);
			}
		}

		public void TestSaveAsDraftSuccessfully_OnMatchForm()
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

			var approval = GetNewTestAPPaymentApproval(TestOrg, TestBank, ZArchitecture.Core.ReceiptTypes.Cheque, TestCheques, "2", 94M);
			using (PaymentApprovalWithAuthorisationForm form = new PaymentApprovalWithAuthorisationForm(approval))
			{
				form.Show();
				Application.DoEvents();

				AssertEquals(true, form.SaveAsDraftButton_ForTestOnly.Visible);
				AssertEquals(true, form.SaveAsDraftButton_ForTestOnly.Enabled);

				form.FireSaveButton();
				var matchingForm = ZFormModaliser.ActiveForm as NewMatchGroupForm;
				var matchingBizO = matchingForm.BusinessEntity as APPaymentApprovalMatching;
				matchingBizO.MoveAllFromUnmatchToMatch();
				AssertEquals("Should be 2 transactions in MatchedTransactions", 2, matchingBizO.MatchedTransactions.Count);
				AssertEquals("Balance should be 0", 0M, matchingBizO.Balance);

				matchingForm.SaveAsDraftButton.PerformClick();
				var matchingBaseObject = approval.MatchingBaseObject as PaymentApprovalMatchingBase;
				AssertEquals(true, approval.IsInDatabase);
				AssertEquals(true, approval.IsDraft);
				AssertEquals("00001000", approval.AV_PaymentApprovalReference);
				AssertNull(approval.NewPayment);

				matchingBaseObject.PaymentApprovalItems.Load();
				AssertEquals(1, matchingBaseObject.PaymentApprovalItems.Count);
				AssertEquals(true, matchingBaseObject.PaymentApprovalItems[0].IsInDatabase);
				AssertEquals(-94m, matchingBaseObject.PaymentApprovalItems[0].A2_PaymentThisRun);
				AssertEquals(testAPInv.PK, matchingBaseObject.PaymentApprovalItems[0].A2_AH);
				AssertEquals("OutstandingAmount not changed", -94m, testAPInv.AH_OutstandingAmount);

				AssertEquals(2, matchingBaseObject.MatchedTransactions.Count);
				AssertEquals(0, matchingBaseObject.MatchLinks.Count);

				AssertEquals(true, matchingForm.IsDisposed);
				AssertEquals(false, form.SaveAsDraftButton_ForTestOnly.Visible);
				AssertEquals(false, form.SaveAsDraftButton_ForTestOnly.Enabled);

				AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSaveAsDraftWillNotShowErrorAboutBalance()
		{
			SetupDataForTest();
			var approval = GetNewTestAPPaymentApproval(TestOrg, TestBank, ReceiptTypes.Cheque, TestCheques, "2", 94M);
			using (PaymentApprovalWithAuthorisationForm form = new PaymentApprovalWithAuthorisationForm(approval))
			{
				form.Show();
				Application.DoEvents();

				form.PostingButtonsUserControl_ForTestOnly.SaveAndCloseButton.PerformClick();
				AssertType<NewMatchGroupForm>(ZFormModaliser.LastFormShownForTest);
				AssertNotEquals("Pre-condition: The balance should not equal 0.", 0m, approval.MatchingBaseObject.Balance);
				var newMatchGroupForm = ZFormModaliser.LastFormShownForTest as NewMatchGroupForm;
				var saveAsDraftButton = newMatchGroupForm.Controls.Find("SaveAsDraftButton", true)[0] as ZButton;
				saveAsDraftButton.PerformClick();
				AssertEquals("Should not show any message as saved successfully.", null, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("The status should be 'DFT' as save as draft successfully.", PaymentApprovalStatus.Draft, approval.AV_Status);
				AssertEquals("Should save successfully even the balance is NOT zero as save as draft.", true, approval.IsInDatabase);
			}
		}

		public void TestSaveAsDraftShowErrorAboutTransactions()
		{
			var exceptedMessage = @"Matching containing Bank Fee, AR Journal or AP Journal cannot be saved in Draft mode. To save as Draft, please remove these transactions.";

			SetupDataForTest();
			var approval = GetNewTestAPPaymentApproval(TestOrg, TestBank, ReceiptTypes.Cheque, TestCheques, "2", 94M);
			approval.MatchingBaseObject.AddMiscellaneousTransaction(Factory.NewWithValidTestData<ARJournal>());
			AssertNotNull(approval.MatchingBaseObject.BankFeeCurrent);
			AssertEquals(true, approval.HasNonDraftTransactions_ForTestOnly);

			using (PaymentApprovalWithAuthorisationForm form = new PaymentApprovalWithAuthorisationForm(approval))
			{
				form.Show();
				Application.DoEvents();

				form.SaveAsDraftButton_ForTestOnly.PerformClick();
				AssertEquals(exceptedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessages();

				form.PostingButtonsUserControl_ForTestOnly.SaveAndCloseButton.PerformClick();
				AssertType<NewMatchGroupForm>(ZFormModaliser.LastFormShownForTest);
				var newMatchGroupForm = ZFormModaliser.LastFormShownForTest as NewMatchGroupForm;
				var saveAsDraftButton = newMatchGroupForm.Controls.Find("SaveAsDraftButton", true)[0] as ZButton;
				saveAsDraftButton.PerformClick();
				AssertEquals(exceptedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSaveAsDraftShowErrorAboutCurrentStatus_OnMatchForm()
		{
			SetupDataForTest();
			var approval = GetNewTestAPPaymentApproval(TestOrg, TestBank, ReceiptTypes.Cheque, TestCheques, "2", 94M);
			approval.MatchingBaseObject.AddMiscellaneousTransaction(Factory.NewWithValidTestData<ARJournal>());
			Factory.Save();
			AssertNotEquals(approval.AV_Status, PaymentApprovalStatus.Draft);

			using (PaymentApprovalWithAuthorisationForm form = new PaymentApprovalWithAuthorisationForm(approval))
			{
				form.Show();
				form.ControllerID = ControllerIDs.APPaymentProcessing;
				approval.AV_Amount = 95m;
				form.PostingButtonsUserControl_ForTestOnly.SaveAndCloseButton.PerformClick();
				Application.DoEvents();

				AssertType<NewMatchGroupForm>(ZFormModaliser.LastFormShownForTest);
				var newMatchGroupForm = ZFormModaliser.LastFormShownForTest as NewMatchGroupForm;
				var saveAsDraftButton = newMatchGroupForm.Controls.Find("SaveAsDraftButton", true)[0] as ZButton;
				saveAsDraftButton.PerformClick();
				AssertEquals($"{approval.GetDescription()} can not Save as Draft since status is Fully Approved", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSaveAsDraftShowErrorAboutCurrentStatus_OnPaymentApprovalForm()
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

			PaymentApprovalWithAuthorisation testAPPaymentApproval = GetNewTestAPPaymentApproval(TestOrg, TestBank, ZArchitecture.Core.ReceiptTypes.Cheque,
				TestCheques, "2", 94M);

			using (PaymentApprovalWithAuthorisationForm form = new PaymentApprovalWithAuthorisationForm(testAPPaymentApproval))
			{
				form.Show();
				Application.DoEvents();

				form.FireSaveButton();
				var matchingForm = ZFormModaliser.ActiveForm as NewMatchGroupForm;
				var matchingBizO = matchingForm.BusinessEntity as APPaymentApprovalMatching;
				matchingBizO.MoveAllFromUnmatchToMatch();
				AssertEquals("Should be 2 transactions in MatchedTransactions", 2, matchingBizO.MatchedTransactions.Count);
				AssertEquals("Balance should be 0", 0M, matchingBizO.Balance);

				//Will Close Match Form, and save in DB with status APP
				AssertEquals("Perconditon", true, !testAPPaymentApproval.IsInDatabase);
				AssertEquals("Perconditon", PaymentApprovalStatus.FullyApproved, testAPPaymentApproval.AV_Status);
				matchingForm.MatchAndCloseButton.PerformClick();
				AssertEquals(true, matchingForm.IsDisposed);
				AssertEquals(true, testAPPaymentApproval.IsInDatabase);
				AssertEquals(PaymentApprovalStatus.FullyApproved, testAPPaymentApproval.AV_Status);

				//Save as draft button will not be Visible
				AssertEquals(false, form.SaveAsDraftButton_ForTestOnly.Visible);
				AssertEquals(false, form.SaveAsDraftButton_ForTestOnly.Enabled);

				(form.BusinessEntity as PaymentApprovalWithAuthorisation).AV_Amount = 95m;

				//Save as draft button will be Visible
				AssertEquals(true, form.SaveAsDraftButton_ForTestOnly.Visible);
				AssertEquals(true, form.SaveAsDraftButton_ForTestOnly.Enabled);

				form.SaveAsDraftButton_ForTestOnly.PerformClick();
				AssertEquals($"{testAPPaymentApproval.GetDescription()} can not Save as Draft since status is Fully Approved", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public PaymentApprovalWithAuthorisation SetupDataForTestMatchDraftPaymentApproval()
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

			var testAPPaymentApproval = GetNewTestAPPaymentApproval(TestOrg, TestBank, ZArchitecture.Core.ReceiptTypes.Cheque, TestCheques, "2", 94m);
			testAPPaymentApproval.AV_Status = PaymentApprovalStatus.Draft;
			Factory.Save();

			return testAPPaymentApproval;
		}

		public void TestMatchDraftPaymentApproval_AwaitingApproval()
		{
			PaymentAuthorisationSettingsCollection collection = new PaymentAuthorisationSettingsCollection();
			PaymentAuthorisationSettings newUpToSetting = collection.AddNew();
			newUpToSetting.Amount = 10m;
			newUpToSetting.AuthorisationRequirement = AuthorisationCodes.NoApprovalRequired;
			newUpToSetting.Range = RangeCodes.UpTo;
			PaymentAuthorisationSettings newSetting = collection.AddNew();
			newSetting.Amount = 10m;
			newSetting.AuthorisationRequirement = AuthorisationCodes.AllThreeApprovalRequired;
			newSetting.Range = RangeCodes.Above;

			using (AccountingConfigurationRegistry.Instance.PayableAuthorizationSettings.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection))
			{
				var testAPPaymentApproval = SetupDataForTestMatchDraftPaymentApproval();

				using (PaymentApprovalWithAuthorisationForm form = new PaymentApprovalWithAuthorisationForm(testAPPaymentApproval))
				{
					form.Show();
					Application.DoEvents();

					form.FireSaveButton();
					var matchingForm = ZFormModaliser.ActiveForm as NewMatchGroupForm;
					var matchingBizO = matchingForm.BusinessEntity as APPaymentApprovalMatching;
					matchingBizO.MoveAllFromUnmatchToMatch();
					AssertEquals("Should be 2 transactions in MatchedTransactions", 2, matchingBizO.MatchedTransactions.Count);
					AssertEquals("Balance should be 0", 0M, matchingBizO.Balance);

					//Will Close Match Form, and status changed from Draft to APP
					AssertEquals("Perconditon", true, testAPPaymentApproval.IsInDatabase);
					AssertEquals("Perconditon", PaymentApprovalStatus.Draft, testAPPaymentApproval.AV_Status);
					matchingForm.MatchAndCloseButton.PerformClick();
					AssertEquals(true, matchingForm.IsDisposed);
					AssertEquals(PaymentApprovalStatus.AwaitingApproval, testAPPaymentApproval.AV_Status);
				}
			}
		}

		public void TestMatchDraftPaymentApproval_FullyApproved()
		{
			var testAPPaymentApproval = SetupDataForTestMatchDraftPaymentApproval();

			using (PaymentApprovalWithAuthorisationForm form = new PaymentApprovalWithAuthorisationForm(testAPPaymentApproval))
			{
				form.Show();
				Application.DoEvents();

				form.FireSaveButton();
				var matchingForm = ZFormModaliser.ActiveForm as NewMatchGroupForm;
				var matchingBizO = matchingForm.BusinessEntity as APPaymentApprovalMatching;
				matchingBizO.MoveAllFromUnmatchToMatch();
				AssertEquals("Should be 2 transactions in MatchedTransactions", 2, matchingBizO.MatchedTransactions.Count);
				AssertEquals("Balance should be 0", 0M, matchingBizO.Balance);

				//Will Close Match Form, and status changed from Draft to APP
				AssertEquals("Perconditon", true, testAPPaymentApproval.IsInDatabase);
				AssertEquals("Perconditon", PaymentApprovalStatus.Draft, testAPPaymentApproval.AV_Status);
				matchingForm.MatchAndCloseButton.PerformClick();
				AssertEquals(true, matchingForm.IsDisposed);
				AssertEquals(PaymentApprovalStatus.FullyApproved, testAPPaymentApproval.AV_Status);
			}
		}

		public void TestCanOpenMatchFormWithContactIsEmptyAndChequeOrReferenceIsEmptyWhenClickPaymentDetailButton()
		{
			SetupDataForTest();
			var approval = GetNewTestAPPaymentApproval(TestOrg, TestBank, ReceiptTypes.Cheque, TestCheques, "2", 94M);
			approval.AV_OC_ContactOverride = Guid.Empty;
			approval.AV_ChequeOrReference = string.Empty;
			using (PaymentApprovalWithAuthorisationForm form = new PaymentApprovalWithAuthorisationForm(approval))
			{
				form.Show();
				AssertEquals("Pre-condition: The contact should be empty.", Guid.Empty, approval.AV_OC_ContactOverride);
				AssertEquals("Pre-condition: The Cheque/Reference should be empty.", string.Empty, approval.AV_ChequeOrReference);
				form.PaymentDetailButton_ForTestOnly.PerformClick();
				Application.DoEvents();
				var matchingForm = ZFormModaliser.ActiveForm as NewMatchGroupForm;
				AssertNotNull("The NewMatchGroupForm should be opened as already skipped validation for Contact and ChequeOrReference.", matchingForm);
			}
		}

		public void TestCanOpenMatchFormWithPaymentTypeIsCHQAndChequeBookIsEmptyWhenClickPaymentDetailButton()
		{
			SetupDataForTest();
			var approval = GetNewTestAPPaymentApproval(TestOrg, TestBank, ReceiptTypes.Cheque, TestCheques, "2", 94M);
			approval.AV_OC_ContactOverride = Guid.Empty;
			approval.AV_ChequeOrReference = string.Empty;
			approval.AV_AK = Guid.Empty;
			using (PaymentApprovalWithAuthorisationForm form = new PaymentApprovalWithAuthorisationForm(approval))
			{
				form.Show();
				AssertEquals("Pre-condition: The contact should be empty.", Guid.Empty, approval.AV_OC_ContactOverride);
				AssertEquals("Pre-condition: The Cheque/Reference should be empty.", string.Empty, approval.AV_ChequeOrReference);
				AssertEquals("Pre-condition: The ChequeBook should be empty.", Guid.Empty, approval.AV_AK);
				form.PaymentDetailButton_ForTestOnly.PerformClick();
				Application.DoEvents();
				var matchingForm = ZFormModaliser.ActiveForm as NewMatchGroupForm;
				AssertNotNull("The NewMatchGroupForm should be opened as already skipped validation for Contact and ChequeOrReference and Cheque Book(Payment Type should be CHQ).", matchingForm);
			}
		}

		public void TestFactoryCachedMatchedTransactionsShouldBeUpdatedWhenSavedSuccessfully()
		{
			SetupDataForTest();
			var approval = GetNewTestAPPaymentApproval(TestOrg, TestBank, ReceiptTypes.Cheque, TestCheques, "2", 94M);
			approval.AV_Status = PaymentApprovalStatus.Draft;
			Factory.Save();
			AssertEquals(true, approval.IsInDatabase);
			AssertEquals(PaymentApprovalStatus.Draft, approval.AV_Status);

			var invoice1 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "123456789", TestObjectCreator.USD, 2, 100M, 20, 200, 40);
			invoice1.AH_OH = approval.AV_OH;
			Factory.Save();

			using (PaymentApprovalWithAuthorisationForm form = new PaymentApprovalWithAuthorisationForm(approval))
			{
				form.Show();
				Application.DoEvents();

				approval.AV_Amount = 95m;

				//Click Save And Close, will show match Form
				form.PaymentDetailButton_ForTestOnly.PerformClick();
				AssertType<NewMatchGroupForm>(ZFormModaliser.LastFormShownForTest);
				IEnumerable<ZGuid> cachedValue;
				Factory.TryGetValueFromCacheOnly(approval.PaymentMatchingBaseObject.GetMatchedTransactionsCacheKey_ForTestOnly(), out cachedValue);
				AssertEquals("Pre-condition: The Factory cached value should only contain Must Matched transaction.", 1, cachedValue.Count());
				AssertEquals($"Pre-condition: The Factory cached value should contain the PK {approval.PK}.", true, cachedValue.Contains(approval.PK));

				//Match form have "Save As Draft" Button
				var newMatchGroupForm = ZFormModaliser.LastFormShownForTest as NewMatchGroupForm;
				var saveAsDraftButton = newMatchGroupForm.Controls.Find("SaveAsDraftButton", true)[0] as ZButton;
				AssertNotNull(saveAsDraftButton);
				AssertEquals(true, saveAsDraftButton.Visible);
				AssertEquals(true, saveAsDraftButton.Enabled);

				InvoicingBaseCollection invoices = new InvoicingBaseCollection(Factory);
				invoices.AddRange(new BusinessObject[] { invoice1 });
				approval.PaymentMatchingBaseObject.MoveFromUnmatchToMatch(invoices.ToArray());
				saveAsDraftButton.PerformClick();

				Factory.TryGetValueFromCacheOnly(approval.PaymentMatchingBaseObject.GetMatchedTransactionsCacheKey_ForTestOnly(), out cachedValue);
				AssertEquals("The Factory cached value should be updated after PaymentApprovalMathcingBase saved.", 2, cachedValue.Count());
				AssertEquals($"The Factory cached value should contain the PK {approval.PK}.", true, cachedValue.Contains(approval.PK));
				AssertEquals($"The Factory cached value should contain the PK {invoice1.PK}.", true, cachedValue.Contains(invoice1.PK));
			}
		}

		#endregion

		#region Cancel E-Payment

		public void TestCancelEPaymentActionMenuItem()
		{
			SetupDataForTest();
			var bankAccount = TestObjectCreator.CreateEPaymentBankAccount();
			var payment = GetNewTestAPPaymentApproval(TestObjectCreator.AALSHI, bankAccount, ReceiptTypes.Cheque, TestCheques, "2", 200m);
			payment.AV_RX_NKPaymentCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			payment.AV_PayExRate = 1m;
			payment.AV_Status = PaymentApprovalStatus.FullyApproved;
			TestObjectCreator.CreateEPaymentStaffToken(bankAccount.PK, ZDateTime.UtcNow.AddHours(2), Env.CurrentUser.Initials, AccEPaymentStaffTokenLookups.StatusCodes.Authorised);

			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			{
				using (var paymentForm1 = new PaymentApprovalWithAuthorisationForm(payment))
				{
					paymentForm1.Show();
					Application.DoEvents();

					Assert(!payment.IsInDatabase);
					AssertNull(paymentForm1.ActionsMenuItem_ForTestOnly.MenuItems.FindByText("Cancel E-Payment"));
				}

				Factory.Save();

				using (var paymentForm2 = new PaymentApprovalWithAuthorisationForm(payment))
				{
					paymentForm2.Show();
					Application.DoEvents();

					Assert(payment.IsInDatabase);
					AssertNull(payment.CurrentDeal);
					AssertNull(paymentForm2.ActionsMenuItem_ForTestOnly.MenuItems.FindByText("Cancel E-Payment"));
				}

				var deal = TestObjectCreator.CreateValidEPaymentDealForStatus(EPaymentStatusCodes.Deal.Queued);
				Factory.Save();

				var paymentWithDeal = Factory.Load<PaymentApprovalWithAuthorisation>(deal.Quote.PaymentApproval.PK);

				using (var paymentForm3 = new PaymentApprovalWithAuthorisationForm(paymentWithDeal))
				{
					paymentForm3.Show();
					Application.DoEvents();

					Assert(paymentWithDeal.IsInDatabase);
					AssertNotNull(paymentWithDeal.CurrentDeal);
					AssertNotNull(paymentForm3.ActionsMenuItem_ForTestOnly.MenuItems.FindByText("Cancel E-Payment"));
				}
			}
		}

		public void TestCancelEPayment_Click_WhenUserDoesNotHasSecurity()
		{
			var deal = TestObjectCreator.CreateValidEPaymentDealForStatus(EPaymentStatusCodes.Deal.Queued);
			Factory.Save();
			var payment = Factory.Load<PaymentApprovalWithAuthorisation>(deal.Quote.PaymentApproval.PK);
			Env.Security.APPaymentProcessingCancelEPayment.IsAllowed = false;
			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			using (var paymentForm = new PaymentApprovalWithAuthorisationForm(payment))
			{
				paymentForm.Show();
				Application.DoEvents();

				Assert(payment.IsInDatabase);
				AssertNotNull(payment.CurrentDeal);
				AssertNotNull(paymentForm.ActionsMenuItem_ForTestOnly.MenuItems.FindByText("Cancel E-Payment"));

				UnitTestUserNotification.Instance.ClearMessages();
				paymentForm.CancelEPaymentMenuItem_ForTestOnly.PerformClick();
				var errorMessage = @"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Manage -> Payables -> Payment Processing -> Cancel E-Payment";

				AssertEquals(errorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestCancelEPayment_Click_WhenUserHasSecurity()
		{
			var deal = TestObjectCreator.CreateValidEPaymentDealForStatus(EPaymentStatusCodes.Deal.Accepted);
			Factory.Save();
			Env.Security.APPaymentProcessingCancelEPayment.IsAllowed = true;
			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			{
				var payment = Factory.Load<PaymentApprovalWithAuthorisation>(deal.Quote.PaymentApproval.PK);
				using (var paymentForm1 = new PaymentApprovalWithAuthorisationForm(payment))
				{
					paymentForm1.Show();
					Application.DoEvents();

					UnitTestUserNotification.Instance.ClearMessages();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					paymentForm1.CancelEPaymentMenuItem_ForTestOnly.PerformClick();
					var dialogueMessage = @"You are attempting to change the status of this E-Payment to CAN.
You should only make this change if you have been in contact with your FX provider who have advised that they have manually canceled your E-Payment.
Changing the status here will have no impact on the status in your FX provider's system. If you have not been advised that they have canceled this E-Payment, depending on your payment method, funds for this payment could still be taken from your nominated bank account.
Do you wish to proceed?";

					AssertEquals(dialogueMessage, UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals(DealStatusCodes.Accepted, payment.CurrentDeal.AED_Status);

					UnitTestUserNotification.Instance.ClearMessages();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					paymentForm1.CancelEPaymentMenuItem_ForTestOnly.PerformClick();

					AssertEquals(DealStatusCodes.Cancelled, payment.CurrentDeal.AED_Status);
					AssertEquals(ZDateTime.Empty, payment.CurrentDeal.AED_LastResponseReceivedUtc);
					AssertNullOrEmpty(payment.CurrentDeal.AED_ProviderReference);
				}

				deal.AED_Status = DealStatusCodes.Declined;
				payment = Factory.Load<PaymentApprovalWithAuthorisation>(deal.Quote.PaymentApproval.PK);
				using (var paymentForm2 = new PaymentApprovalWithAuthorisationForm(payment))
				{
					paymentForm2.Show();
					Application.DoEvents();

					UnitTestUserNotification.Instance.ClearMessages();
					paymentForm2.CancelEPaymentMenuItem_ForTestOnly.PerformClick();
					var errorMessage = @"This option can only be used to cancel an E-Payment with a status of ACP or INP. 
Its purpose is to cancel E-Payments which have been manually canceled with your OFX provider.";

					AssertEquals(errorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals(DealStatusCodes.Declined, payment.CurrentDeal.AED_Status);
				}
			}
		}

		#endregion

		public void TestCheckEPaymentExchangeRateButtonClick_HasExistingRequestedQuote()
		{
			SetupDataForTest();
			var payment = GetNewTestAPPaymentApproval(TestObjectCreator.AALSHI, TestBank, ReceiptTypes.Cheque, TestCheques, "2", 200m);
			payment.AV_RX_NKPaymentCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			payment.AV_Status = PaymentApprovalStatus.AwaitingApproval;
			var rquote = payment.TryToCreateQuote(EPaymentProviderCodes.Codes.OFX).Quote;
			rquote.QU_Status = QuoteStatusCodes.Requested;
			Factory.Save();

			var quote = Factory.LoadTop1<AccEPaymentQuote>(new ZQuery(AccEPaymentQuoteSchema.QU_AV, payment.PK));
			AssertNotNull(quote);
			AssertEquals(QuoteStatusCodes.Requested, quote.QU_Status);

			using (var paymentForm = new PaymentApprovalWithAuthorisationForm(payment))
			{
				paymentForm.Show();
				Assert(paymentForm.CheckExRateButton_ForTestOnly.Enabled);

				UnitTestUserNotification.Instance.ClearMessages();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				paymentForm.CheckExRateButton_ForTestOnly.PerformClick();
				AssertEquals("E-Quote already requested. Response may take up to several minutes to be received. If you generate a new request, then the previous request will be discarded. Are you sure you want to proceed?", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessages();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				paymentForm.CheckExRateButton_ForTestOnly.PerformClick();

				AssertEquals("E-Quote request generated to service provider OFX. Response may take from a few moments up to several minutes to be received.", UnitTestUserNotification.Instance.LastMessage.Text);

				payment.PaymentQuotes.Reload(true);
				AssertEquals(2, payment.PaymentQuotes.Count);
				AssertEquals(1, payment.PaymentQuotes.Cast<EPaymentQuote>().Count(x => x.QU_Status == QuoteStatusCodes.Queued));
				AssertEquals(1, payment.PaymentQuotes.Cast<EPaymentQuote>().Count(x => x.QU_Status == QuoteStatusCodes.Discarded));
			}
		}

		public void TestFormProperties()
		{
			SetupDataForTest();
			var payment = GetNewTestAPPaymentApproval(TestObjectCreator.AALSHI, TestBank, ZArchitecture.Core.ReceiptTypes.Cheque, TestCheques, "2", 200m);
			payment.AV_RX_NKPaymentCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			payment.AV_Status = PaymentApprovalStatus.AwaitingApproval;
			Factory.Save();

			using (var paymentForm = new PaymentApprovalWithAuthorisationForm(payment))
			{
				paymentForm.Show();
				AssertEquals("Refresh", paymentForm.RefreshButton_ForTestOnly.CaptionResourceString.Caption);
			}
		}

		public void TestPaymentIsCancelled_ActionButtonsDisabled()
		{
			SetupDataForTest();
			var payment = GetNewTestAPPaymentApproval(TestObjectCreator.AALSHI, TestBank, ZArchitecture.Core.ReceiptTypes.Cheque, TestCheques, "2", 200m);
			payment.AV_RX_NKPaymentCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			payment.AV_Status = PaymentApprovalStatus.Cancelled;
			Factory.Save();

			using (var paymentForm = new PaymentApprovalWithAuthorisationForm(payment))
			{
				paymentForm.Show();
				Assert(!paymentForm.ProcessEPaymentButton_ForTestOnly.Enabled);
				Assert(!paymentForm.CheckExRateButton_ForTestOnly.Enabled);
				Assert(!paymentForm.AcceptQuoteButton_ForTestOnly.Enabled);
			}
		}

		public void TestQuoteWarningMessage()
		{
			SetupDataForTest();
			var payment = GetNewTestAPPaymentApproval(TestObjectCreator.AALSHI, TestBank, ReceiptTypes.Cheque, TestCheques, "2", 200m);
			payment.AV_RX_NKPaymentCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			payment.AV_Status = PaymentApprovalStatus.AwaitingApproval;
			Factory.Save();
			var quote1 = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Received, payment);
			quote1.QU_ProviderReference = string.Empty;
			var quote2 = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Received, payment);
			Factory.Save();
			payment.PaymentQuotes.Reload(true);

			using (var paymentForm = new PaymentApprovalWithAuthorisationForm(payment))
			{
				paymentForm.Show();
				Application.DoEvents();

				var bankAndEPaymentTabControl = paymentForm.Controls.Find("TabControlBankAndEPayment", true).First() as ZTabControl;
				AssertNotNull(bankAndEPaymentTabControl);
				bankAndEPaymentTabControl.SelectTab("EPaymentTab");

				var epaymentGrid = paymentForm.Controls.Find("zGrid1", true).First() as ZGrid;
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

		public void TestClickNewButtonWhenEditPaymentApproval()
		{
			SetupDataForTest();
			var approval = GetNewTestAPPaymentApproval(TestOrg, TestBank, ReceiptTypes.Cheque, TestCheques, "2", 94M);
			Factory.Save();

			ZController controller = ZControllerFactory.Create(ControllerIDs.APPaymentProcessing);
			using (PaymentApprovalWithAuthorisationForm form = controller.ShowEditForm(approval) as PaymentApprovalWithAuthorisationForm)
			{
				form.Show();
				Application.DoEvents();
				AssertEquals("&New", form.PostingButtonsUserControl_ForTestOnly.SaveButton.Text);
				AssertEquals(true, form.BusinessEntity.IsInDatabase);

				AssertNoExceptionThrown(() => form.PostingButtonsUserControl_ForTestOnly.SaveButton.PerformClick());
				Application.DoEvents();
				var formCache = OpenedFormCache.GetInstance();
				var paymentApprovalWithAuthorisationFormCache = formCache.FormCache.Values.First() as PaymentApprovalWithAuthorisationForm;
				AssertEquals(1, formCache.Count);
				AssertNotNull(paymentApprovalWithAuthorisationFormCache);
				AssertEquals(false, paymentApprovalWithAuthorisationFormCache.BusinessEntity.IsInDatabase);

				paymentApprovalWithAuthorisationFormCache.Close();
				paymentApprovalWithAuthorisationFormCache.Dispose();
			}
		}

		public void TestCheckEPaymentExchangeRateButton_Click_HasExistingActiveDeal()
		{
			SetupDataForTest();
			var payment = GetNewTestAPPaymentApproval(TestObjectCreator.AALSHI, TestBank, ZArchitecture.Core.ReceiptTypes.Cheque, TestCheques, "2", 200m);
			payment.AV_RX_NKPaymentCurrency = "USD";
			var quote = payment.TryToCreateQuote(EPaymentProviderCodes.Codes.OFX).Quote;
			TestObjectCreator.CreateValidEPaymentDealForStatus(DealStatusCodes.Queued, payment, quote);
			Factory.Save();

			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			{
				using (var paymentForm = new PaymentApprovalWithAuthorisationForm(payment))
				{
					paymentForm.Show();
					Assert(paymentForm.CheckExRateButton_ForTestOnly.Enabled);

					Assert(payment.HasActiveDeal);
					AssertEquals(1, payment.PaymentQuotes.Count);

					UnitTestUserNotification.Instance.ClearMessages();
					paymentForm.CheckExRateButton_Click_ForTestOnly(null, new EventArgs());

					AssertEquals(PaymentApprovalBase.ActiveDealErrorTextForChangingPaymentDetails, UnitTestUserNotification.Instance.LastMessage.Text);
					payment.RefreshQuotes();
					AssertEquals(1, payment.PaymentQuotes.Count);
				}
			}
		}

		public void TestCheckEPaymentExchangeRateButton_Click_NoExistingActiveQuote()
		{
			SetupDataForTest();
			PaymentApprovalWithAuthorisation payment = GetNewTestAPPaymentApproval(TestObjectCreator.AALSHI, TestBank, ZArchitecture.Core.ReceiptTypes.Cheque, TestCheques, "2", 200m);
			payment.AV_RX_NKPaymentCurrency = "USD";
			Factory.Save();

			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			{
				payment.AV_Status = PaymentApprovalStatus.AwaitingApproval;
				using (var paymentForm = new PaymentApprovalWithAuthorisationForm(payment))
				{
					paymentForm.Show();
					payment.AV_PaymentComment = "new comment";
					Assert(paymentForm.CheckExRateButton_ForTestOnly.Enabled);
					paymentForm.CheckExRateButton_Click_ForTestOnly(null, new EventArgs());
					AssertEquals("Please save your payment approval first.", UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessages();

					Factory.Save();

					var quote = Factory.LoadTop1<AccEPaymentQuote>(new ZQuery(AccEPaymentQuoteSchema.QU_AV, payment.PK));
					AssertNull("Pre-condition - no quote for payment in DB", quote);
					UnitTestUserNotification.Instance.AddOKAnswer();
					ZTabControl tabControl = (ZTabControl)paymentForm.Controls.Find("TabControlBankAndEPayment", true).First();
					AssertEquals("Before click, Bank Detail tab page should be focused", 0, tabControl.SelectedIndex);
					paymentForm.CheckExRateButton_Click_ForTestOnly(null, new EventArgs());
					AssertEquals("After click, E-Payment tab page should be focused", 2, tabControl.SelectedIndex);
					AssertEquals("E-Quote request generated to service provider OFX. Response may take from a few moments up to several minutes to be received.", UnitTestUserNotification.Instance.LastMessage.Text);
					quote = Factory.LoadTop1<AccEPaymentQuote>(new ZQuery(AccEPaymentQuoteSchema.QU_AV, payment.PK));
					AssertNotNull("Post-condition - quote is created for payment", quote);
					AssertEquals("OFX", quote.QU_ProviderCode);
					AssertEquals(200m, quote.QU_ToAmount);
					AssertEquals("USD", quote.QU_RX_NKToCurrency);
					AssertEquals(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, quote.QU_RX_NKFromCurrency);
					AssertEquals(GlbCompany.CurrentCompany.PK, quote.QU_GC);
				}
			}
		}

		public void TestCheckEPaymentExchangeRateButton_Click_ForNewPaymentApproval()
		{
			SetupDataForTest();
			var payment = GetNewTestAPPaymentApproval(TestObjectCreator.AALSHI, TestBank, ZArchitecture.Core.ReceiptTypes.Cheque, TestCheques, "2", 200m);
			AccEPaymentQuote quote = null;
			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			{
				using (var paymentForm = new PaymentApprovalWithAuthorisationForm(payment))
				{
					paymentForm.Show();

					quote = Factory.LoadTop1<AccEPaymentQuote>(new ZQuery(AccEPaymentQuoteSchema.QU_AV, payment.PK));
					AssertNull("Pre-condition - no quote for payment in DB", quote);
					Assert("Pre-condition - Payment is not saved.", !payment.IsInDatabase);

					payment.AV_RX_NKPaymentCurrency = "USD";
					payment.AV_PaymentComment = "new comment";
					Assert(paymentForm.CheckExRateButton_ForTestOnly.Enabled);
					paymentForm.CheckExRateButton_Click_ForTestOnly(null, new EventArgs());

					Assert("Post-condition - Payment is saved.", payment.IsInDatabase);
					quote = Factory.LoadTop1<AccEPaymentQuote>(new ZQuery(AccEPaymentQuoteSchema.QU_AV, payment.PK));
					AssertNotNull("Post-condition - quote is created for payment", quote.IsInDatabase);
					AssertEquals("OFX", quote.QU_ProviderCode);
					AssertEquals(200m, quote.QU_ToAmount);
					AssertEquals("USD", quote.QU_RX_NKToCurrency);
					AssertEquals(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, quote.QU_RX_NKFromCurrency);
					AssertEquals(GlbCompany.CurrentCompany.PK, quote.QU_GC);
				}

				quote.QU_Status = QuoteStatusCodes.Discarded;
				Factory.Save();

				//opening the form second time with paymentapproval in Draft status and previous quote is discarded
				using (var paymentForm = new PaymentApprovalWithAuthorisationForm(payment))
				{
					paymentForm.Show();

					quote = Factory.LoadTop1<AccEPaymentQuote>(new ZQuery(AccEPaymentQuoteSchema.QU_AV, payment.PK));
					AssertEquals("Pre-condition - Current quote is discarded", QuoteStatusCodes.Discarded, quote.QU_Status);
					AssertEquals("Pre-condition - Payment is in draft.", PaymentApprovalStatus.Draft, payment.AV_Status);

					Assert(paymentForm.CheckExRateButton_ForTestOnly.Enabled);
					paymentForm.CheckExRateButton_Click_ForTestOnly(null, new EventArgs());

					var quotes = Factory.Load<AccEPaymentQuote>(new ZQuery(AccEPaymentQuoteSchema.QU_AV, payment.PK));
					AssertEquals("Number of quotes", 2, quotes.Length);

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

		public void TestCheckEPaymentExchangeRateButton_Click_HasExistingQueuedQuote()
		{
			SetupDataForTest();
			PaymentApprovalWithAuthorisation payment = GetNewTestAPPaymentApproval(TestObjectCreator.AALSHI, TestBank, ZArchitecture.Core.ReceiptTypes.Cheque, TestCheques, "2", 200m);
			payment.AV_RX_NKPaymentCurrency = "USD";
			payment.TryToCreateQuote(EPaymentProviderCodes.Codes.OFX);
			Factory.Save();

			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			{
				payment.AV_Status = PaymentApprovalStatus.AwaitingApproval;
				using (var paymentForm = new PaymentApprovalWithAuthorisationForm(payment))
				{
					paymentForm.Show();
					payment.AV_PaymentComment = "new comment";
					Assert(paymentForm.CheckExRateButton_ForTestOnly.Enabled);
					Factory.Save();

					UnitTestUserNotification.Instance.ClearMessages();
					paymentForm.CheckExRateButton_Click_ForTestOnly(null, new EventArgs());
					AssertEquals("Exchange rate already requested", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestCheckEPaymentExchangeRateButton_Click_HasExistingActiveQuote()
		{
			SetupDataForTest();
			PaymentApprovalWithAuthorisation payment = GetNewTestAPPaymentApproval(TestObjectCreator.AALSHI, TestBank, ZArchitecture.Core.ReceiptTypes.Cheque, TestCheques, "2", 200m);
			payment.AV_RX_NKPaymentCurrency = "USD";
			var quote = payment.TryToCreateQuote(EPaymentProviderCodes.Codes.OFX).Quote;
			quote.QU_Status = QuoteStatusCodes.Failed;
			Factory.Save();

			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			{
				payment.AV_Status = PaymentApprovalStatus.AwaitingApproval;
				using (var paymentForm = new PaymentApprovalWithAuthorisationForm(payment))
				{
					paymentForm.Show();
					payment.AV_PaymentComment = "new comment";
					Assert(paymentForm.CheckExRateButton_ForTestOnly.Enabled);
					Factory.Save();

					UnitTestUserNotification.Instance.ClearMessages();
					paymentForm.CheckExRateButton_Click_ForTestOnly(null, new EventArgs());

					var quotes = Factory.Load<AccEPaymentQuote>(new ZQuery(AccEPaymentQuoteSchema.QU_AV, payment.PK));
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
			PaymentApprovalWithAuthorisation payment = GetNewTestAPPaymentApproval(TestObjectCreator.AALSHI, TestBank, ZArchitecture.Core.ReceiptTypes.Cheque, TestCheques, "2", 200m);
			payment.AV_RX_NKPaymentCurrency = "USD";
			var quote = payment.TryToCreateQuote(EPaymentProviderCodes.Codes.OFX).Quote;
			AssertEquals("Pre-condition - quote is queued", QuoteStatusCodes.Queued, quote.QU_Status);
			Factory.Save();

			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			{
				payment.AV_Status = PaymentApprovalStatus.AwaitingApproval;
				using (var paymentForm = new PaymentApprovalWithAuthorisationForm(payment))
				{
					paymentForm.Show();
					payment.AV_PaymentComment = "new comment";
					payment.AV_Amount = 258M;
					Assert(paymentForm.CheckExRateButton_ForTestOnly.Enabled);
					Factory.Save();

					UnitTestUserNotification.Instance.ClearMessages();
					paymentForm.CheckExRateButton_Click_ForTestOnly(null, new EventArgs());

					var quotes = Factory.Load<AccEPaymentQuote>(new ZQuery(AccEPaymentQuoteSchema.QU_AV, payment.PK));
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

		public void TestRefreshButton_Click()
		{
			SetupDataForTest();
			PaymentApprovalWithAuthorisation payment = GetNewTestAPPaymentApproval(TestObjectCreator.AALSHI, TestBank, ReceiptTypes.Cheque, TestCheques, "2", 200m);
			payment.AV_RX_NKPaymentCurrency = "USD";
			var deal = TestObjectCreator.CreateEPaymentDeal(payment);
			Factory.Save();

			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			{
				payment.AV_Status = PaymentApprovalStatus.AwaitingApproval;
				using (var paymentForm = new PaymentApprovalWithAuthorisationForm(payment))
				{
					paymentForm.Show();
					AssertEquals("Preconditon - 1 quote in DB", 1, payment.PaymentQuotes.Count);
					AssertEquals("Preconditon - Deal on payment", deal.PK, payment.CurrentDeal.PK);

					var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
					var paymentInNewFactory = newFactory.Load<PaymentApprovalWithAuthorisation>(payment.PK);
					var dealInNewFactory = newFactory.Load<AccEPaymentDeal>(deal.PK);
					dealInNewFactory.AED_Status = DealStatusCodes.Cancelled;

					var newDeal = new TestObjectCreator(newFactory).CreateValidEPaymentDealForStatus(DealStatusCodes.InProgress, paymentInNewFactory);
					newDeal.AED_SystemCreateTimeUtc = ZDateTime.Now.AddDays(1);
					newDeal.AED_LastResponseReceivedUtc = ZDateTime.Now.AddDays(1);
					newFactory.Save();

					AssertEquals("No update has happened before pressing refresh", 1, payment.PaymentQuotes.Count);
					AssertEquals("Deal has not updated yet", deal.PK, payment.CurrentDeal.PK);
					paymentForm.RefreshButton_Click_ForTestOnly(null, null);
					AssertEquals("Expect clicking the refresh button will update the quotes collection", 2, payment.PaymentQuotes.Count);
					AssertEquals("Expect active deal has updated", newDeal.PK, payment.CurrentDeal.PK);
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
			var payment = GetNewTestAPPaymentApproval(TestObjectCreator.AALSHI, TestBank, ZArchitecture.Core.ReceiptTypes.Cheque, TestCheques, "2", 200m);

			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, isOFXEPaymentEnabled))
			{
				using (var paymentForm = new PaymentApprovalWithAuthorisationForm(payment))
				{
					paymentForm.Show();
					if (isOFXEPaymentEnabled)
					{
						Assert("CheckExRateButton should be visible when E-Payment functionality is enabled.", paymentForm.CheckExRateButton_ForTestOnly.Visible);
						Assert("ProcessEPaymentButton should be visible when E-Payment functionality is enabled.", paymentForm.ProcessEPaymentButton_ForTestOnly.Visible);
						Assert("EPaymentTabPage should be visible when E-Payment functionality is enabled.", paymentForm.EPaymentTabPage_ForTestOnly.TabVisible);

						Assert("Learn More Button should be visible when E-Payment functionality is enabled.", paymentForm.LearnMoreButton_ForTestOnly.Visible); // TODO
						TestLearnMoreButtonOpensUpProductMarketingPage(paymentForm);
					}
					else
					{
						Assert("CheckExRateButton should be invisible when E-Payment functionality is disabled.", !paymentForm.CheckExRateButton_ForTestOnly.Visible);
						Assert("ProcessEPaymentButton should be invisible when E-Payment functionality is disabled.", !paymentForm.ProcessEPaymentButton_ForTestOnly.Visible);
						Assert("EPaymentTabPage should be invisible when E-Payment functionality is disabled.", !paymentForm.EPaymentTabPage_ForTestOnly.TabVisible);
						Assert("Learn More Button should be invisible when E-Payment functionality is disabled.", !paymentForm.LearnMoreButton_ForTestOnly.Visible);
					}
				}
			}
		}

		void TestLearnMoreButtonOpensUpProductMarketingPage(PaymentApprovalWithAuthorisationForm form)
		{
			var learnMoreButton = (ZButton)form.Controls.Find("LearnMoreButton", true).FirstOrDefault();
			AssertNotNull(learnMoreButton);
			Assert(learnMoreButton.Visible);
			WebUrlLauncher.ClearLastUrlLaunched();
			learnMoreButton.PerformClick();
			AssertEquals(AccountingMasterFilesRegistry.Instance.EPaymentProductMarketingWebURL.Value, WebUrlLauncher.LastUrlLaunched);
		}

		public void TestCheckEPaymentExchangeRateButtonEnablenessDependsOnStatus()
		{
			SetupDataForTest();
			PaymentApprovalWithAuthorisation payment = GetNewTestAPPaymentApproval(TestObjectCreator.AALSHI, TestBank, ZArchitecture.Core.ReceiptTypes.Cheque, TestCheques, "2", 200m);
			payment.AV_RX_NKPaymentCurrency = "USD";
			Factory.Save();

			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			{
				var supportedStatusList = new ZString[] { PaymentApprovalStatus.AwaitingApproval, PaymentApprovalStatus.FullyApproved, PaymentApprovalStatus.Rejected, PaymentApprovalStatus.Draft };
				var unSupportedStatusList = new ZString[] { PaymentApprovalStatus.Cancelled, PaymentApprovalStatus.Posted };
				foreach (var status in supportedStatusList)
				{
					payment.AV_Status = status;
					using (var paymentForm = new PaymentApprovalWithAuthorisationForm(payment))
					{
						paymentForm.Show();
						Assert(paymentForm.CheckExRateButton_ForTestOnly.Enabled);
					}
				}
				foreach (var status in unSupportedStatusList)
				{
					payment.AV_Status = status;
					using (var paymentForm = new PaymentApprovalWithAuthorisationForm(payment))
					{
						paymentForm.Show();
						Assert(!paymentForm.CheckExRateButton_ForTestOnly.Enabled);
					}
				}
			}
		}

		public void TestCheckEPaymentExchangeRateButtonIsEnabledForNewApproval()
		{
			SetupDataForTest();
			PaymentApprovalWithAuthorisation payment = GetNewTestAPPaymentApproval(TestObjectCreator.AALSHI, TestBank, ZArchitecture.Core.ReceiptTypes.Cheque, TestCheques, "2", 200m);
			payment.AV_RX_NKPaymentCurrency = "USD";
			payment.AV_Status = PaymentApprovalStatus.AwaitingApproval;

			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			{
				using (var paymentForm = new PaymentApprovalWithAuthorisationForm(payment))
				{
					paymentForm.Show();
					Assert("button should be enabled when approval is not saved", paymentForm.CheckExRateButton_ForTestOnly.Enabled);
				}

				Factory.Save();
				using (var paymentForm = new PaymentApprovalWithAuthorisationForm(payment))
				{
					paymentForm.Show();
					Assert("button should be enabled when approval is saved", paymentForm.CheckExRateButton_ForTestOnly.Enabled);
				}
			}
		}

		public void TestCheckEPaymentExchangeRateButtonEnablenessDependsOnCurrency()
		{
			SetupDataForTest();
			var payment = GetNewTestAPPaymentApproval(TestObjectCreator.AALSHI, TestBank, ReceiptTypes.Cheque, TestCheques, "2", 200m);
			payment.AV_RX_NKPaymentCurrency = "AUD";
			AssertEquals(payment.AV_RX_NKPaymentCurrency, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);

			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			{
				using (var paymentForm = new PaymentApprovalWithAuthorisationForm(payment))
				{
					paymentForm.Show();
					Assert("button should be disabled when payment currency is same as local currency", !paymentForm.CheckExRateButton_ForTestOnly.Enabled);

					payment.AV_RX_NKPaymentCurrency = "EUR";
					AssertNotEquals(payment.AV_RX_NKPaymentCurrency, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
					Assert("button should be enabled when payment currency is different to local currency", paymentForm.CheckExRateButton_ForTestOnly.Enabled);

					payment.AV_RX_NKPaymentCurrency = "AUD";
					AssertEquals(payment.AV_RX_NKPaymentCurrency, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
					Assert("button should be disabled when payment currency is same as local currency", !paymentForm.CheckExRateButton_ForTestOnly.Enabled);
				}
			}
		}

		[TestDate(2012, 11, 1, 11, 0, 0)]
		public void TestCriticalValidationErrorWhenClosing()
		{
			SetupDataForTest();
			PaymentApprovalWithAuthorisation payment = GetNewTestAPPaymentApproval(TestObjectCreator.AALSHI, TestBank, ZArchitecture.Core.ReceiptTypes.Cheque, TestCheques, "2", 200m);
			Factory.Save();

			using (var paymentForm = new PaymentApprovalWithAuthorisationForm(payment))
			{
				NewMatchGroupForm matchingForm = null;

				try
				{
					paymentForm.FireSaveButton();   // user clicks payment detail button sorta

					matchingForm = ZFormModaliser.ActiveForm as NewMatchGroupForm;
					AssertNotNull("The active form should be the matching form", matchingForm);
					APPaymentApprovalMatching matchingBizO = matchingForm.BusinessEntity as APPaymentApprovalMatching;
					matchingBizO.MoveAllFromMatchToUnmatch();

					payment.AV_Discount = -200m;
					payment.PaymentMatchingBaseObject.CreateTemporaryTransactions();

					AssertNotNull("Matching Object should be APMatchingBase", matchingBizO);
					AssertEquals("Should be 2 transactions in MatchedTransactions", 2, matchingBizO.MatchedTransactions.Count);
					AssertEquals("Balance should be 0", 0M, matchingBizO.Balance);

					//Discount is added to generate a critical validation on purpose
					APDiscount apDiscount = TestObjectCreator.CreateAPDiscount(1000m, ZDateTime.Today, TestObjectCreator.AALSHI.PK);
					payment.PaymentMatchingBaseObject.AddMiscellaneousTransaction(apDiscount);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					matchingForm.Close();   // this should post the transactions
				}
				finally
				{
					if (matchingForm != null)
					{
						matchingForm.Dispose();
					}
				}

				AssertContains("Critical validation error should be shown (User Message)", string.Format(@"An error has occurred. Your unsaved work must be re-entered.

Please close the form in which you were working and re-enter the data.

Error Message: Non zero outstanding amount on miscellaneous transaction.",
				((AccTransactionHeader)(payment.PaymentMatchingBaseObject.MatchedTransactions[1])).PK),
				UnitTestUserNotification.Instance.LastMessage.Text);
				ExceptionReporterTestListener.Instance.Clear();
			}
		}

		public void TestShowPreSaveDialogsWillNotLoadTransactionsFromDBIfPaymentIsInDBAndSessionBalanceIsZero()
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
			invoice.AH_OSExTaxAmount = 94M;
			invoice.AH_TransactionNum = "00001000";

			PaymentApprovalWithAuthorisation aPPaymentApproval = GetNewTestAPPaymentApproval(TestOrg, testBank, ZArchitecture.Core.ReceiptTypes.Cheque, testCheques, "2", 94M);

			ZFormModaliser.LastFormShownForTest = null;
			using (PaymentApprovalWithAuthorisationForm payForm = new PaymentApprovalWithAuthorisationForm(aPPaymentApproval))
			{
				AssertEquals("Precondition: payment approval is NOT in database", false, payForm.Payment_ForTestOnly.IsInDatabase);
				int beforeHit = Factory.GetTableHitCount(AccTransactionHeaderSchema.Constants.TableName);
				ContinueWithSave continueResult = payForm.ShowPreSaveDialogs_ForTestOnly();
				int afterHit = Factory.GetTableHitCount(AccTransactionHeaderSchema.Constants.TableName);
				AssertEquals("AccTransactionHeader table should be loaded once if Payment is NOT in database", 1, afterHit - beforeHit);
				AssertEquals("Should prompt NewMatchGroupForm because Payment is NOT in database", typeof(NewMatchGroupForm), ZFormModaliser.LastFormShownForTest.GetType());
				ZFormModaliser.LastFormShownForTest = null;
			}
			Factory.Save();

			using (PaymentApprovalWithAuthorisationForm payForm = new PaymentApprovalWithAuthorisationForm(aPPaymentApproval))
			{
				AssertEquals("Precondition: payment approval is in database", true, payForm.Payment_ForTestOnly.IsInDatabase);
				AssertEquals("Precondition: session balances is NOT zero", false, payForm.Payment_ForTestOnly.PaymentMatchingBaseObject.SessionBalancesToZero);
				int beforeHit = Factory.GetTableHitCount(AccTransactionHeaderSchema.Constants.TableName);
				ContinueWithSave continueResult = payForm.ShowPreSaveDialogs_ForTestOnly();
				int afterHit = Factory.GetTableHitCount(AccTransactionHeaderSchema.Constants.TableName);
				AssertEquals("AccTransactionHeader table should be loaded once if Payment is in database AND session balance is NOT zero", 1, afterHit - beforeHit);
				AssertEquals("Should prompt NewMatchGroupForm because Payment is in database AND session balances is NOT zero", typeof(NewMatchGroupForm), ZFormModaliser.LastFormShownForTest.GetType());
				ZFormModaliser.LastFormShownForTest = null;
			}

			using (PaymentApprovalWithAuthorisationForm payForm = new PaymentApprovalWithAuthorisationForm(aPPaymentApproval))
			{
				using (payForm.Payment_ForTestOnly.PaymentMatchingBaseObject.MatchedTransactions.SuspendBalanceCalculation())
				{
					AssertEquals("Precondition: payment approval is in database", true, payForm.Payment_ForTestOnly.IsInDatabase);
					AssertEquals("Precondition: session balances is zero", true, payForm.Payment_ForTestOnly.PaymentMatchingBaseObject.SessionBalancesToZero);
					int beforeHit = Factory.GetTableHitCount(AccTransactionHeaderSchema.Constants.TableName);
					ContinueWithSave continueResult = payForm.ShowPreSaveDialogs_ForTestOnly();
					int afterHit = Factory.GetTableHitCount(AccTransactionHeaderSchema.Constants.TableName);
					AssertEquals("AccTransactionHeader table should NOT be loaded if Payment is in database AND session balance is zero", beforeHit, afterHit);
					AssertEquals("Should NOT prompt NewMatchGroupForm because Payment is in database AND session balances is zero", null, ZFormModaliser.LastFormShownForTest);
					ZFormModaliser.LastFormShownForTest = null;
				}
			}
		}

		public void TestPaymentReadOnlyIsTrueForViewForm()
		{
			var approval = Factory.New<APPaymentApprovalForTest>();
			using (PaymentApprovalWithAuthorisationForm form = new PaymentApprovalWithAuthorisationForm(approval))
			{
				form.DisplayMode = ODisplayMode.ReadOnly;
				form.Show();
				Assert(approval.ReadOnly);
			}
		}

		public void TestPaymentDetailButtonIsReadonlyForCancelledApproval()
		{
			var approval = Factory.New<APPaymentApprovalForTest>();
			using (var form = new PaymentApprovalWithAuthorisationForm(approval))
			{
				approval.AV_Status = PaymentApprovalStatus.AwaitingApproval;
				form.DisplayMode = ODisplayMode.Browse;
				Assert(!form.PaymentDetailButton_ForTestOnly.ReadOnly);
			}
			using (var form = new PaymentApprovalWithAuthorisationForm(approval))
			{
				approval.AV_Status = PaymentApprovalStatus.Cancelled;
				form.DisplayMode = ODisplayMode.Browse;
				Assert(form.PaymentDetailButton_ForTestOnly.ReadOnly);
			}
		}

		public void TestAuthorisationButtonIsReadonlyForCancelledApproval()
		{
			var approval = Factory.New<APPaymentApprovalForTest>();
			approval.fUserHasAuthoriseLevel1Security = true;
			approval.fUserHasAuthoriseLevel2Security = true;
			approval.fUserHasAuthoriseLevel3Security = true;
			approval.fLevel1AuthorisationRequired = true;
			approval.fLevel2AuthorisationRequired = true;
			approval.fLevel3AuthorisationRequired = true;
			using (var form = new PaymentApprovalWithAuthorisationForm(approval))
			{
				approval.AV_Status = PaymentApprovalStatus.Cancelled;
				form.UpdateAuthorizationButtonReadonly_ForTestOnly();
				Assert(form.FirstAuthorisationButton_ForTestOnly.ReadOnly);
				Assert(form.SecondAuthorisationButton_ForTestOnly.ReadOnly);
				Assert(form.ThirdAuthorisationButton_ForTestOnly.ReadOnly);

				approval.AV_Status = PaymentApprovalStatus.AwaitingApproval;
				form.UpdateAuthorizationButtonReadonly_ForTestOnly();
				Assert(!form.FirstAuthorisationButton_ForTestOnly.ReadOnly);
				Assert(!form.SecondAuthorisationButton_ForTestOnly.ReadOnly);
				Assert(!form.ThirdAuthorisationButton_ForTestOnly.ReadOnly);
			}
		}

		#region Test Click Handlers

		public void TestFirstAuthorisationButton_Click()
		{
			APPaymentApprovalForTest approval = Factory.New<APPaymentApprovalForTest>();
			using (PaymentApprovalWithAuthorisationForm form = new PaymentApprovalWithAuthorisationForm(approval))
			{
				AssertEquals("Precondition: First Authorisation should be empty", true, approval.AV_GS_NKApproval1st.IsEmpty);
				AssertEquals("First Authorise Button Text", PaymentApprovalWithAuthorisationForm.Authorise, form.FirstAuthorisationButton_ForTestOnly.Text);

				approval.fLevel1AuthorisationRequired = true;

				approval.fUserHasAuthoriseLevel1Security = false;
				form.PaymentApprovalBizO_RequiredAuthorisationChanged_ForTestOnly(null, ZString.Empty);
				AssertEquals("First Authorisation Button Readonly", true, form.FirstAuthorisationButton_ForTestOnly.ReadOnly);
				form.FirstAuthorisationButton_Click_ForTestOnly(null, new EventArgs());
				AssertEquals("First Authorisation should be empty", true, approval.AV_GS_NKApproval1st.IsEmpty);
				AssertEquals("First Authorise Button Text", PaymentApprovalWithAuthorisationForm.Authorise, form.FirstAuthorisationButton_ForTestOnly.Text);

				approval.fUserHasAuthoriseLevel1Security = true;
				form.PaymentApprovalBizO_RequiredAuthorisationChanged_ForTestOnly(null, ZString.Empty);
				AssertEquals("First Authorisation Button Readonly", false, form.FirstAuthorisationButton_ForTestOnly.ReadOnly);
				form.FirstAuthorisationButton_Click_ForTestOnly(null, new EventArgs());
				AssertEquals("First Authorisation should NOT be empty", false, approval.AV_GS_NKApproval1st.IsEmpty);
				AssertEquals("First Authorise Button Text", PaymentApprovalWithAuthorisationForm.UnAuthorise, form.FirstAuthorisationButton_ForTestOnly.Text);

				approval.fUserHasAuthoriseLevel1Security = false;
				form.PaymentApprovalBizO_RequiredAuthorisationChanged_ForTestOnly(null, ZString.Empty);
				AssertEquals("First Authorisation Button Readonly", true, form.FirstAuthorisationButton_ForTestOnly.ReadOnly);
				form.FirstAuthorisationButton_Click_ForTestOnly(null, new EventArgs());
				AssertEquals("First Authorisation should NOT be empty", false, approval.AV_GS_NKApproval1st.IsEmpty);
				AssertEquals("First Authorise Button Text", PaymentApprovalWithAuthorisationForm.UnAuthorise, form.FirstAuthorisationButton_ForTestOnly.Text);

				approval.fUserHasAuthoriseLevel1Security = true;
				form.PaymentApprovalBizO_RequiredAuthorisationChanged_ForTestOnly(null, ZString.Empty);
				AssertEquals("First Authorisation Button Readonly", false, form.FirstAuthorisationButton_ForTestOnly.ReadOnly);
				form.FirstAuthorisationButton_Click_ForTestOnly(null, new EventArgs());
				AssertEquals("First Authorisation should be empty", true, approval.AV_GS_NKApproval1st.IsEmpty);
				AssertEquals("First Authorise Button Text", PaymentApprovalWithAuthorisationForm.Authorise, form.FirstAuthorisationButton_ForTestOnly.Text);

				approval.TryRejectPayment(new NotificationBuffer());
				AssertEquals("Precondition: Status is Rejected", PaymentApprovalStatus.Rejected, approval.AV_Status);
				AssertEquals("First Authorisation Button Readonly", true, form.FirstAuthorisationButton_ForTestOnly.ReadOnly);
				AssertEquals("First Authorise Button Text", PaymentApprovalWithAuthorisationForm.Authorise, form.FirstAuthorisationButton_ForTestOnly.Text);
			}
		}

		public void TestSecondAuthorisationButton_Click()
		{
			APPaymentApprovalForTest approval = Factory.New<APPaymentApprovalForTest>();
			using (var form = new PaymentApprovalWithAuthorisationForm(approval))
			{
				AssertEquals("Precondition: Second Authorisation should be empty", true, approval.AV_GS_NKApproval2nd.IsEmpty);
				AssertEquals("Second Authorise Button Text", PaymentApprovalWithAuthorisationForm.Authorise, form.SecondAuthorisationButton_ForTestOnly.Text);

				approval.fLevel2AuthorisationRequired = true;

				approval.fUserHasAuthoriseLevel2Security = false;
				form.PaymentApprovalBizO_RequiredAuthorisationChanged_ForTestOnly(null, ZString.Empty);
				AssertEquals("Second Authorisation Button Readonly", true, form.SecondAuthorisationButton_ForTestOnly.ReadOnly);
				form.SecondAuthorisationButton_Click_ForTestOnly(null, new EventArgs());
				AssertEquals("Second Authorisation should be empty", true, approval.AV_GS_NKApproval2nd.IsEmpty);
				AssertEquals("Second Authorise Button Text", PaymentApprovalWithAuthorisationForm.Authorise, form.SecondAuthorisationButton_ForTestOnly.Text);

				approval.fUserHasAuthoriseLevel2Security = true;
				form.PaymentApprovalBizO_RequiredAuthorisationChanged_ForTestOnly(null, ZString.Empty);
				AssertEquals("Second Authorisation Button Readonly", false, form.SecondAuthorisationButton_ForTestOnly.ReadOnly);
				form.SecondAuthorisationButton_Click_ForTestOnly(null, new EventArgs());
				AssertEquals("Second Authorisation should NOT be empty", false, approval.AV_GS_NKApproval2nd.IsEmpty);
				AssertEquals("Second Authorise Button Text", PaymentApprovalWithAuthorisationForm.UnAuthorise, form.SecondAuthorisationButton_ForTestOnly.Text);

				approval.fUserHasAuthoriseLevel2Security = false;
				form.PaymentApprovalBizO_RequiredAuthorisationChanged_ForTestOnly(null, ZString.Empty);
				AssertEquals("Second Authorisation Button Readonly", true, form.SecondAuthorisationButton_ForTestOnly.ReadOnly);
				form.SecondAuthorisationButton_Click_ForTestOnly(null, new EventArgs());
				AssertEquals("Second Authorisation should NOT be empty", false, approval.AV_GS_NKApproval2nd.IsEmpty);
				AssertEquals("Second Authorise Button Text", PaymentApprovalWithAuthorisationForm.UnAuthorise, form.SecondAuthorisationButton_ForTestOnly.Text);

				approval.fUserHasAuthoriseLevel2Security = true;
				form.PaymentApprovalBizO_RequiredAuthorisationChanged_ForTestOnly(null, ZString.Empty);
				AssertEquals("Second Authorisation Button Readonly", false, form.SecondAuthorisationButton_ForTestOnly.ReadOnly);
				form.SecondAuthorisationButton_Click_ForTestOnly(null, new EventArgs());
				AssertEquals("Second Authorisation should be empty", true, approval.AV_GS_NKApproval2nd.IsEmpty);
				AssertEquals("Second Authorise Button Text", PaymentApprovalWithAuthorisationForm.Authorise, form.SecondAuthorisationButton_ForTestOnly.Text);

				approval.TryRejectPayment(new NotificationBuffer());
				AssertEquals("Precondition: Status is Rejected", PaymentApprovalStatus.Rejected, approval.AV_Status);
				AssertEquals("Second Authorisation Button Readonly", true, form.SecondAuthorisationButton_ForTestOnly.ReadOnly);
				AssertEquals("Second Authorise Button Text", PaymentApprovalWithAuthorisationForm.Authorise, form.SecondAuthorisationButton_ForTestOnly.Text);
			}
		}

		public void TestThirdAuthorisationButton_Click()
		{
			APPaymentApprovalForTest approval = Factory.New<APPaymentApprovalForTest>();
			using (PaymentApprovalWithAuthorisationForm form = new PaymentApprovalWithAuthorisationForm(approval))
			{
				AssertEquals("Precondition: Third Authorisation should be empty", true, approval.AV_GS_NKApproval3rd.IsEmpty);
				AssertEquals("Third Authorise Button Text", PaymentApprovalWithAuthorisationForm.Authorise, form.ThirdAuthorisationButton_ForTestOnly.Text);

				approval.fLevel3AuthorisationRequired = true;

				approval.fUserHasAuthoriseLevel3Security = false;
				form.PaymentApprovalBizO_RequiredAuthorisationChanged_ForTestOnly(null, ZString.Empty);
				AssertEquals("Third Authorisation Button Readonly", true, form.ThirdAuthorisationButton_ForTestOnly.ReadOnly);
				form.ThirdAuthorisationButton_Click_ForTestOnly(null, new EventArgs());
				AssertEquals("Third Authorisation should be empty", true, approval.AV_GS_NKApproval3rd.IsEmpty);
				AssertEquals("Third Authorise Button Text", PaymentApprovalWithAuthorisationForm.Authorise, form.ThirdAuthorisationButton_ForTestOnly.Text);

				approval.fUserHasAuthoriseLevel3Security = true;
				form.PaymentApprovalBizO_RequiredAuthorisationChanged_ForTestOnly(null, ZString.Empty);
				AssertEquals("Third Authorisation Button Readonly", false, form.ThirdAuthorisationButton_ForTestOnly.ReadOnly);
				form.ThirdAuthorisationButton_Click_ForTestOnly(null, new EventArgs());
				AssertEquals("Third Authorisation should NOT be empty", false, approval.AV_GS_NKApproval3rd.IsEmpty);
				AssertEquals("Third Authorise Button Text", PaymentApprovalWithAuthorisationForm.UnAuthorise, form.ThirdAuthorisationButton_ForTestOnly.Text);

				approval.fUserHasAuthoriseLevel3Security = false;
				form.PaymentApprovalBizO_RequiredAuthorisationChanged_ForTestOnly(null, ZString.Empty);
				AssertEquals("Third Authorisation Button Readonly", true, form.ThirdAuthorisationButton_ForTestOnly.ReadOnly);
				form.ThirdAuthorisationButton_Click_ForTestOnly(null, new EventArgs());
				AssertEquals("Third Authorisation should NOT be empty", false, approval.AV_GS_NKApproval3rd.IsEmpty);
				AssertEquals("Third Authorise Button Text", PaymentApprovalWithAuthorisationForm.UnAuthorise, form.ThirdAuthorisationButton_ForTestOnly.Text);

				approval.fUserHasAuthoriseLevel3Security = true;
				form.PaymentApprovalBizO_RequiredAuthorisationChanged_ForTestOnly(null, ZString.Empty);
				AssertEquals("Third Authorisation Button Readonly", false, form.ThirdAuthorisationButton_ForTestOnly.ReadOnly);
				form.ThirdAuthorisationButton_Click_ForTestOnly(null, new EventArgs());
				AssertEquals("Third Authorisation should be empty", true, approval.AV_GS_NKApproval3rd.IsEmpty);
				AssertEquals("Third Authorise Button Text", PaymentApprovalWithAuthorisationForm.Authorise, form.ThirdAuthorisationButton_ForTestOnly.Text);

				approval.TryRejectPayment(new NotificationBuffer());
				AssertEquals("Precondition: Status is Rejected", PaymentApprovalStatus.Rejected, approval.AV_Status);
				AssertEquals("Third Authorisation Button Readonly", true, form.ThirdAuthorisationButton_ForTestOnly.ReadOnly);
				AssertEquals("Third Authorise Button Text", PaymentApprovalWithAuthorisationForm.Authorise, form.ThirdAuthorisationButton_ForTestOnly.Text);
			}
		}

		public void TestUnAuthoriseButtonsWithActiveDeal()
		{
			var deal = TestObjectCreator.CreateEPaymentDeal();
			Factory.Save();

			var approval = Factory.Load<APPaymentApprovalForTest>(deal.Quote.PaymentApproval.PK);
			approval.fLevel1AuthorisationRequired = true;
			approval.fLevel2AuthorisationRequired = true;
			approval.fLevel3AuthorisationRequired = true;
			approval.AV_GS_NKApproval1st = GlbStaff.CurrentUser.GS_Code;
			approval.AV_GS_NKApproval2nd = GlbStaff.CurrentUser.GS_Code;
			approval.AV_GS_NKApproval3rd = GlbStaff.CurrentUser.GS_Code;
			approval.fUserHasAuthoriseLevel1Security = true;
			approval.fUserHasAuthoriseLevel2Security = true;
			approval.fUserHasAuthoriseLevel3Security = true;
			Factory.Save();

			Env.Security.APPaymentProcessingFirstApproval.IsAllowed = true;
			Env.Security.APPaymentProcessingSecondApproval.IsAllowed = true;
			Env.Security.APPaymentProcessingThirdApproval.IsAllowed = true;

			using (var form = new PaymentApprovalWithAuthorisationForm(approval))
			{
				AssertEquals(PaymentApprovalWithAuthorisationForm.UnAuthorise, form.ThirdAuthorisationButton_ForTestOnly.Text);
				AssertEquals(PaymentApprovalWithAuthorisationForm.UnAuthorise, form.ThirdAuthorisationButton_ForTestOnly.Text);
				AssertEquals(PaymentApprovalWithAuthorisationForm.UnAuthorise, form.ThirdAuthorisationButton_ForTestOnly.Text);

				foreach (var status in DealStatusCodes.ActiveStatusCodes)
				{
					approval.CurrentDeal.AED_Status = status;
					form.FirstAuthorisationButton_Click_ForTestOnly(null, new EventArgs());
					AssertEquals(PaymentApprovalBase.ActiveDealErrorTextForChangingPaymentDetails, UnitTestUserNotification.Instance.LastMessage.Text);
					Assert(UnitTestUserNotification.Instance.LastMessage.WasError);
					UnitTestUserNotification.Instance.ClearMessages();

					form.SecondAuthorisationButton_Click_ForTestOnly(null, new EventArgs());
					AssertEquals(PaymentApprovalBase.ActiveDealErrorTextForChangingPaymentDetails, UnitTestUserNotification.Instance.LastMessage.Text);
					Assert(UnitTestUserNotification.Instance.LastMessage.WasError);
					UnitTestUserNotification.Instance.ClearMessages();

					form.ThirdAuthorisationButton_Click_ForTestOnly(null, new EventArgs());
					AssertEquals(PaymentApprovalBase.ActiveDealErrorTextForChangingPaymentDetails, UnitTestUserNotification.Instance.LastMessage.Text);
					Assert(UnitTestUserNotification.Instance.LastMessage.WasError);
					UnitTestUserNotification.Instance.ClearMessages();
				}

				foreach (var status in DealStatusCodes.InactiveStatusCodes)
				{
					approval.CurrentDeal.AED_Status = status;
					form.FirstAuthorisationButton_Click_ForTestOnly(null, new EventArgs());
					AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);
					AssertNullOrEmpty(approval.AV_GS_NKApproval1st);

					form.SecondAuthorisationButton_Click_ForTestOnly(null, new EventArgs());
					AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);
					AssertNullOrEmpty(approval.AV_GS_NKApproval2nd);

					form.ThirdAuthorisationButton_Click_ForTestOnly(null, new EventArgs());
					AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);
					AssertNullOrEmpty(approval.AV_GS_NKApproval3rd);

					approval.AV_GS_NKApproval1st = GlbStaff.CurrentUser.GS_Code;
					approval.AV_GS_NKApproval2nd = GlbStaff.CurrentUser.GS_Code;
					approval.AV_GS_NKApproval3rd = GlbStaff.CurrentUser.GS_Code;
					form.PaymentApprovalBizO_RequiredAuthorisationChanged_ForTestOnly(null, ZString.Empty);
					AssertEquals(PaymentApprovalWithAuthorisationForm.UnAuthorise, form.ThirdAuthorisationButton_ForTestOnly.Text);
					AssertEquals(PaymentApprovalWithAuthorisationForm.UnAuthorise, form.ThirdAuthorisationButton_ForTestOnly.Text);
					AssertEquals(PaymentApprovalWithAuthorisationForm.UnAuthorise, form.ThirdAuthorisationButton_ForTestOnly.Text);
				}
			}
		}

		public void TestRejectButton()
		{
			var authCollection = new PaymentAuthorisationSettingsCollection();
			var auth1 = authCollection.AddNew();
			var auth2 = authCollection.AddNew();

			auth1.Amount = 1000;
			auth1.AuthorisationRequirement = AuthorisationCodes.NoApprovalRequired;
			auth1.Range = RangeCodes.UpTo;

			auth2.Amount = 1000;
			auth2.AuthorisationRequirement = AuthorisationCodes.FirstApprovalRequiredOnly;
			auth2.Range = RangeCodes.Above;

			AccountingConfigurationRegistry.Instance.PayableAuthorizationSettings.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, authCollection);

			var reasonCodes = new CodeDescriptionPairList();
			reasonCodes.AddPairIfNotExist("XYZ", "Some Description");
			AccountingMasterFilesRegistry.Instance.PaymentRejectionReasonCodesList.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, reasonCodes);

			var approvingUser = TestObjectCreator.CreateStaff("NEW");
			Factory.Save();

			var approval = Factory.New<APPaymentApprovalWithAuthorisation>();
			using (var form = new PaymentApprovalWithAuthorisationForm(approval))
			{
				approval.AV_Amount = 100;
				Assert(approval.NoAuthorisationRequired);
				AssertRejectControlsReadOnly(true);

				approval.AV_Amount = 1001;
				Assert(!approval.NoAuthorisationRequired);
				AssertRejectControlsReadOnly(false);

				approval.AV_GS_NKApproval1st = approvingUser.GS_Code;
				Env.Security.APPaymentProcessingFirstApproval.IsAllowed = false;
				form.PaymentApprovalBizO_RequiredAuthorisationChanged_ForTestOnly(null, ZString.Empty);
				AssertRejectControlsReadOnly(true);
				Env.Security.APPaymentProcessingFirstApproval.IsAllowed = true;
				form.PaymentApprovalBizO_RequiredAuthorisationChanged_ForTestOnly(null, ZString.Empty);

				form.RejectButton_Click_ForTestOnly(null, new EventArgs());
				Assert(!approval.IsRejected);
				AssertRejectControlsReadOnly(false);
				AssertEquals("Please choose a valid reason code", UnitTestUserNotification.Instance.LastMessage.Text);

				approval.AV_RejectionReasonCode = "ABC";
				form.RejectButton_Click_ForTestOnly(null, new EventArgs());
				Assert(!approval.IsRejected);
				AssertRejectControlsReadOnly(false);
				AssertEquals("Please choose a valid reason code", UnitTestUserNotification.Instance.LastMessage.Text);

				approval.AV_RejectionReasonCode = "XYZ";
				approval.AV_RejectionReasonDetails = ZString.Empty;
				form.RejectButton_Click_ForTestOnly(null, new EventArgs());
				Assert(!approval.IsRejected);
				AssertRejectControlsReadOnly(false);
				AssertEquals("Please enter a non-blank reason text", UnitTestUserNotification.Instance.LastMessage.Text);

				approval.AV_RejectionReasonDetails = "Here is a description";
				form.RejectButton_Click_ForTestOnly(null, new EventArgs());
				Assert(approval.IsRejected);
				AssertRejectControlsReadOnly(true);

				approval.AV_Status = PaymentApprovalStatus.AwaitingApproval;
				AssertRejectControlsReadOnly(false);

				approval.TryCancelPayment(new NotificationBuffer());
				AssertRejectControlsReadOnly(true);

				void AssertRejectControlsReadOnly(bool expectReadOnly)
				{
					AssertEquals(expectReadOnly, form.RejectButton_ForTestOnly.ReadOnly);
					AssertEquals(expectReadOnly, form.RejectReasonCodeDropEdit_ForTestOnly.ReadOnly);
					AssertEquals(expectReadOnly, form.RejectReasonTextBox_ForTestOnly.ReadOnly);
				}
			}
		}

		public void TestRejectButtonWithActiveDeal()
		{
			var deal = TestObjectCreator.CreateEPaymentDeal();
			Factory.Save();
			var approval = Factory.Load<PaymentApprovalWithAuthorisation>(deal.Quote.PaymentApproval.PK);

			using (var form = new PaymentApprovalWithAuthorisationForm(approval))
			{
				foreach (var status in DealStatusCodes.ActiveStatusCodes)
				{
					deal.AED_Status = status;
					approval.AV_RejectionReasonCode = "INS";
					approval.AV_RejectionReasonDetails = "Insufficient Funds";
					form.RejectButton_Click_ForTestOnly(null, new EventArgs());
					Assert(!approval.IsRejected);
					AssertEquals(PaymentApprovalBase.ActiveDealErrorTextForChangingPaymentDetails, UnitTestUserNotification.Instance.LastMessage.Text);
					Assert(UnitTestUserNotification.Instance.LastMessage.WasError);
					UnitTestUserNotification.Instance.ClearMessages();
				}

				foreach (var status in DealStatusCodes.InactiveStatusCodes)
				{
					deal.AED_Status = status;
					approval.AV_RejectionReasonCode = "INS";
					approval.AV_RejectionReasonDetails = "Insufficient Funds";
					form.RejectButton_Click_ForTestOnly(null, new EventArgs());
					Assert(approval.IsRejected);
					AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);
					approval.AV_Status = PaymentApprovalStatus.FullyApproved;
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

			PaymentApprovalWithAuthorisation testAPPaymentApproval = GetNewTestAPPaymentApproval(TestOrg, TestBank, ZArchitecture.Core.ReceiptTypes.Cheque,
				TestCheques, "2", 94M);

			using (MockPaymentApprovalWithAuthorisationForm payApproveForm = new MockPaymentApprovalWithAuthorisationForm(testAPPaymentApproval))
			{
				NewMatchGroupForm matchingForm = null;
				PaymentDocumentsPrintPopup exposedPrintForm = null;

				try
				{
					payApproveForm.FireSaveButton();    // user clicks payment detail button

					matchingForm = ZFormModaliser.ActiveForm as NewMatchGroupForm;
					AssertNotNull("The active form should be the matching form", matchingForm);
					APPaymentApprovalMatching matchingBizO = matchingForm.BusinessEntity as APPaymentApprovalMatching;
					Assert("OSPartialPaymentAmount should be readonly", ((IMatching)testAPPaymentApproval).OSPartialPaymentAmountInfo.ReadOnly);

					matchingBizO.MoveAllFromUnmatchToMatch();

					AssertNotNull("Matching Object should be APMatchingBase", matchingBizO);
					AssertEquals("Should be 2 transactions in MatchedTransactions", 2, matchingBizO.MatchedTransactions.Count);
					AssertEquals("Balance should be 0", 0M, matchingBizO.Balance);

					matchingForm.FireSaveButton();  // Note: this does not post the matching session

					BusinessObjectFactory emptyFactory = new BusinessObjectFactory();
					PaymentApprovalItemCollection approvalItems = new PaymentApprovalItemCollection(emptyFactory);
					approvalItems.Load();
					AssertEquals("No Approval Items should be posted to DB yet since posting happens when MatchingForm is closed", 0, approvalItems.Count);

					matchingForm.Close();

					approvalItems.Load();
					AssertEquals("1 PaymentApprovalItem should be posted to DB when matching form is closed", 1, approvalItems.Count);
					ZQuery invMatchFilter = new ZQuery(AccPaymentApprovalItemSchema.A2_AH, testAPInv.PK);
					AccPaymentApprovalItem invApproval = emptyFactory.LoadTop1<AccPaymentApprovalItem>(invMatchFilter);
					AssertNotNull("There should be a PaymentApprovalItem  for the invoice", invApproval);

					AssertEquals("DisplayMode of Original PaymentApprovalWithAuthorisationForm should be Browse", ZArchitecture.Core.ODisplayMode.Browse,
						payApproveForm.DisplayMode);
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

		#endregion

		#region TestMatchingFormNotPosted

		public void TestMatchingFormNotPosted()
		{
			SetupDataForTest();
			Factory.Save();

			PaymentApprovalWithAuthorisation testARPaymentApproval = Factory.New<APPaymentApprovalWithAuthorisation>();
			testARPaymentApproval.AV_OH = TestOrg.PK;
			testARPaymentApproval.AV_AB = TestBank.PK;
			testARPaymentApproval.AV_PaymentType = ZArchitecture.Core.ReceiptTypes.Cash;
			testARPaymentApproval.AV_ChequeOrReference = "3";
			testARPaymentApproval.AV_Amount = 46M;

			using (PaymentApprovalWithAuthorisationForm testPayForm = new PaymentApprovalWithAuthorisationForm(testARPaymentApproval))
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

		#region TestShouldCreateAndCleanUpTemporaryTransactions

		public void TestWhenMatchGroupFormClosesDecideActionBasedOnDialogResultOnlyInClosedEventHandler()
		{
			SetupDataForTest();

			var paymentApproval = Factory.New<APPaymentApprovalWithAuthorisation>();
			paymentApproval.AV_OH = TestOrg.PK;
			paymentApproval.AV_AB = TestBank.PK;
			paymentApproval.AV_PaymentType = ZArchitecture.Core.ReceiptTypes.Cash;
			paymentApproval.AV_Amount = 100M;

			using (var paymentApprovalForm = new PaymentApprovalWithAuthorisationForm(paymentApproval))
			{
				paymentApprovalForm.Show();
				paymentApprovalForm.PaymentDetailButton_ForTestOnly.PerformClick(); //click payment details button
				var matchingForm = ZFormModaliser.ActiveForm as NewMatchGroupForm;
				AssertNotNull(matchingForm);
				var matchingBase = matchingForm.BusinessEntity as APPaymentApprovalMatching;

				var exchangeDiff = matchingBase.GetMiscellaneousTransaction(TransactionTypes.ExchangeDifference);
				exchangeDiff.AH_InvoiceAmount = -10m;
				matchingBase.AddMiscellaneousTransaction(exchangeDiff);

				AssertEquals("Matched Transactions should have two transactions", 2, matchingBase.MatchedTransactions.Count);
				Assert("Payment should be included in Matched Transactions", matchingBase.MatchedTransactions.Contains(paymentApproval));
				Assert("Exchange Difference should be included in Matched Transactions", matchingBase.MatchedTransactions.Contains(exchangeDiff));

				//Cancel form closing, Closed Event Handler will not run
				matchingForm.Closing += (object sender, CancelEventArgs e) => e.Cancel = true;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				matchingForm.Close();

				Assert("Exchange Difference should only be deleted by Closed Event Handler", !exchangeDiff.IsDeleted);
			}
		}

		public void TestShouldCreateAndCleanUpTemporaryTransactions()
		{
			SetupDataForTest();

			var paymentApproval = Factory.New<APPaymentApprovalWithAuthorisation>();
			paymentApproval.AV_OH = TestOrg.PK;
			paymentApproval.AV_AB = TestBank.PK;
			paymentApproval.AV_PaymentType = ZArchitecture.Core.ReceiptTypes.Cash;
			paymentApproval.AV_Amount = 100M;

			using (var paymentApprovalForm = new PaymentApprovalWithAuthorisationForm(paymentApproval))
			{
				paymentApprovalForm.Show();
				paymentApprovalForm.PaymentDetailButton_ForTestOnly.PerformClick();
				var matchingForm = ZFormModaliser.ActiveForm as NewMatchGroupForm;
				AssertNotNull(matchingForm);
				var matchingBase = matchingForm.BusinessEntity as APPaymentApprovalMatching;

				var exchangeDiff = matchingBase.GetMiscellaneousTransaction(TransactionTypes.ExchangeDifference);
				exchangeDiff.AH_InvoiceAmount = -10m;
				matchingBase.AddMiscellaneousTransaction(exchangeDiff);

				AssertEquals("Matched Transactions should have two transactions", 2, matchingBase.MatchedTransactions.Count);
				Assert("Payment should be included in Matched Transactions", matchingBase.MatchedTransactions.Contains(paymentApproval));
				Assert("Exchange Difference should be included in Matched Transactions", matchingBase.MatchedTransactions.Contains(exchangeDiff));

				//close match group form - User says CANCEL when prompted to save changes
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				matchingForm.Close();

				Assert("Matching form should not be closed", matchingForm.Visible);
				Assert("Exchange Difference should not be deleted", !exchangeDiff.IsDeleted);

				//Avoid code analysis warning - CA2202: Do not dispose objects multiple times
				matchingForm = ZFormModaliser.ActiveForm as NewMatchGroupForm;

				//close match group form - User says NO when prompted to save changes
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				matchingForm.Close();

				Assert("Matching form should be closed", !matchingForm.Visible);
				Assert("Exchange Difference should be deleted", exchangeDiff.IsDeleted);

				//reopen match group form
				paymentApprovalForm.PaymentDetailButton_ForTestOnly.PerformClick();
				matchingForm = ZFormModaliser.ActiveForm as NewMatchGroupForm;
				AssertNotNull(matchingForm);
				matchingBase = matchingForm.BusinessEntity as APPaymentApprovalMatching;

				AssertEquals("Matched Transactions should have two transactions", 2, matchingBase.MatchedTransactions.Count);
				Assert("Payment should be included in Matched Transactions", matchingBase.MatchedTransactions.Contains(paymentApproval));
				var newExchangeDiff = matchingBase.MatchedTransactions.First(x => x.PK != paymentApproval.PK);
				AssertType("Exchange Difference should be included in Matched Transactions", typeof(APExchangeDifference), newExchangeDiff);

				//Balance the match group
				var journal = TestObjectCreator.CreateJournal<APJournal>(90m, ZDateTime.Today, TestOrg1.PK);
				journal.DebitCreditSign = Core.Constants.DebitCredit.Debit;
				matchingBase.MoveFromUnmatchToMatch(new BusinessObject[] { journal });

				AssertEquals("Matched Transactions should have three transactions", 3, matchingBase.MatchedTransactions.Count);
				Assert("Journal should be included in Matched Transactions", matchingBase.MatchedTransactions.Contains(journal));
				Assert("Match group should be balanced", matchingBase.SessionBalancesToZero);

				//close match group form - User says YES when prompted to save changes
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				matchingForm.Close();

				Assert("Matching form should be closed", !matchingForm.Visible);
				Assert("Exchange Difference should be deleted", newExchangeDiff.IsDeleted);
			}
		}

		#endregion
		#region TestDisplayModeAfterOpenMatchForm

		public void TestDisplayModeAfterCancelledMatching()
		{
			var approval = PerpareTestDataForDisplayModeTest();

			using (PaymentApprovalWithAuthorisationForm form = new PaymentApprovalWithAuthorisationForm(approval))
			{
				form.Show();
				Application.DoEvents();
				AssertEquals("Display mode is Browse", ZArchitecture.Core.ODisplayMode.Browse, form.DisplayMode);

				approval.AV_PaymentComment = "Test 1234";
				AssertEquals("Display mode is Edit", ZArchitecture.Core.ODisplayMode.Edit, form.DisplayMode);

				form.PaymentDetailButton_ForTestOnly.PerformClick();
				Application.DoEvents();
				var matchingForm = ZFormModaliser.ActiveForm as NewMatchGroupForm;
				APPaymentApprovalMatching matchingBizO = matchingForm.BusinessEntity as APPaymentApprovalMatching;
				matchingBizO.MoveAllFromUnmatchToMatch();

				AssertEquals("Should be 2 transactions in MatchedTransactions", 2, matchingBizO.MatchedTransactions.Count);
				AssertEquals("Balance should be 0", 0M, matchingBizO.Balance);

				matchingForm.SaveFactoryResult = SaveFactoryFlag.Cancel;
				matchingForm.Close();
				AssertEquals("Display mode is Edit", ZArchitecture.Core.ODisplayMode.Edit, form.DisplayMode);

				AssertEquals("&Save", form.PostingButtonsUserControl_ForTestOnly.SaveButton.Text);
				AssertEquals(true, form.PostingButtonsUserControl_ForTestOnly.SaveButton.Enabled);
				AssertEquals(true, form.PostingButtonsUserControl_ForTestOnly.SaveButton.Visible);

				AssertEquals(true, form.PostingButtonsUserControl_ForTestOnly.SaveAndCloseButton.Enabled);
				AssertEquals(true, form.PostingButtonsUserControl_ForTestOnly.SaveAndCloseButton.Visible);

				AssertEquals(true, form.PostingButtonsUserControl_ForTestOnly.CloseButton.Enabled);
				AssertEquals(true, form.PostingButtonsUserControl_ForTestOnly.CloseButton.Visible);

				AssertEquals(true, form.SaveAsDraftButton_ForTestOnly.Enabled);
				AssertEquals(true, form.SaveAsDraftButton_ForTestOnly.Visible);
			}
		}

		public void TestDisplayModeAfterSaveMatching()
		{
			var approval = PerpareTestDataForDisplayModeTest();

			using (PaymentApprovalWithAuthorisationForm form = new PaymentApprovalWithAuthorisationForm(approval))
			{
				form.Show();
				Application.DoEvents();
				AssertEquals("Display mode is Browse", ZArchitecture.Core.ODisplayMode.Browse, form.DisplayMode);

				approval.AV_PaymentComment = "Test 1234";
				AssertEquals("Display mode is Edit", ZArchitecture.Core.ODisplayMode.Edit, form.DisplayMode);

				form.FireSaveButton();
				var matchingForm = ZFormModaliser.ActiveForm as NewMatchGroupForm;
				var matchingBizO = matchingForm.BusinessEntity as APPaymentApprovalMatching;
				matchingBizO.MoveAllFromUnmatchToMatch();
				AssertEquals("Should be 2 transactions in MatchedTransactions", 2, matchingBizO.MatchedTransactions.Count);
				AssertEquals("Balance should be 0", 0M, matchingBizO.Balance);

				matchingForm.SaveFactoryResult = SaveFactoryFlag.OK;
				matchingForm.Close();
				AssertEquals("Display mode is Browse", ZArchitecture.Core.ODisplayMode.Browse, form.DisplayMode);

				AssertEquals("&New", form.PostingButtonsUserControl_ForTestOnly.SaveButton.Text);
				AssertEquals(true, form.PostingButtonsUserControl_ForTestOnly.SaveButton.Enabled);
				AssertEquals(true, form.PostingButtonsUserControl_ForTestOnly.SaveButton.Visible);

				AssertEquals(false, form.PostingButtonsUserControl_ForTestOnly.SaveAndCloseButton.Enabled);
				AssertEquals(true, form.PostingButtonsUserControl_ForTestOnly.SaveAndCloseButton.Visible);

				AssertEquals(true, form.PostingButtonsUserControl_ForTestOnly.CloseButton.Enabled);
				AssertEquals(true, form.PostingButtonsUserControl_ForTestOnly.CloseButton.Visible);

				AssertEquals(false, form.SaveAsDraftButton_ForTestOnly.Enabled);
				AssertEquals(false, form.SaveAsDraftButton_ForTestOnly.Visible);
			}
		}

		PaymentApprovalWithAuthorisation PerpareTestDataForDisplayModeTest()
		{
			SetupDataForTest();

			var shipment = TestObjectCreator.CreateShipment("S1");
			var job = TestObjectCreator.CreateJob(shipment);

			var testAPInv = TestObjectCreator.CreateInvoice(typeof(APInvoice));
			testAPInv.AH_OH = TestOrg.PK;
			var line = TestObjectCreator.CreateInvoiceLine(testAPInv, testAPInv.TransactionCurrency, testAPInv.AH_ExchangeRate, 94m, 0m, 0m, 94m, 0m, 0m, TestObjectCreator.CC1.PK);
			line.AL_JH = job.PK;
			var charge = TestObjectCreator.CreateJobCharge(line, job, TestObjectCreator.CC1, testAPInv.TransactionCurrency);
			testAPInv.AH_FullyPaidDate = ZDateTime.Empty;
			Factory.Save();

			var approval = GetNewTestAPPaymentApproval(TestOrg, TestBank, ZArchitecture.Core.ReceiptTypes.Cheque, TestCheques, "2", 94M);
			Factory.Save();
			return approval;
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

			APInvoice invoice = Factory.NewWithValidTestData<APInvoice>();
			invoice.AH_OSExTaxAmount = 94M;
			var line = TestObjectCreator.CreateInvoiceLine(invoice, invoice.TransactionCurrency, invoice.AH_ExchangeRate, 94m, 0m, 0m, 94m, 0m, 0m, TestObjectCreator.CC1.PK);
			TestObjectCreator.AttachJobToAPLine(line);
			invoice.AH_LocalOutstandingAmount = 94M;
			invoice.AH_OH = TestOrg.PK;
			TestObjectCreator.AttachChargeToAPLine(line);
			Factory.Save();

			PaymentApprovalWithAuthorisation aPPaymentApproval = GetNewTestAPPaymentApproval(TestOrg, testBank, ZArchitecture.Core.ReceiptTypes.Cheque,
				testCheques, "2", 94M);

			Assert("Payment Approval should not have errors", !aPPaymentApproval.HasErrors);

			using (PaymentApprovalWithAuthorisationForm payForm = new PaymentApprovalWithAuthorisationForm(aPPaymentApproval)) // test popping up matching form
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

			using (PaymentApprovalWithAuthorisationForm payForm = new PaymentApprovalWithAuthorisationForm(aPPaymentApproval))
			{
				ContinueWithSave continueResult = payForm.ShowPreSaveDialogs_ForTestOnly();
				AssertEquals("Should continue since matching was completed", ContinueWithSave.Yes, continueResult);
			}
		}

		#endregion

		#region TestPrintManager

		public void TestPrintManager()
		{
			APPayment newlyCreatedPayment = Factory.New<APPayment>();
			PaymentApprovalWithAuthorisation aPPaymentApproval = Factory.New<APPaymentApprovalWithAuthorisation>();
			aPPaymentApproval.AV_AH = newlyCreatedPayment.PK;

			using (PaymentApprovalWithAuthorisationForm payForm = new PaymentApprovalWithAuthorisationForm(aPPaymentApproval))
			{
				AssertNotNull("PaymentPrintManager_ForTestOnly should exist", payForm.PrintManager_ForTestOnly);
			}
		}

		#endregion

		#region TestCancellingMatchSessionMakesChequeNumberEditable

		public void TestCancellingMatchSessionMakesAllFieldsEditable()
		{
			SetupDataForTest();
			PaymentApprovalWithAuthorisation aPPaymentApproval = GetNewTestAPPaymentApproval(TestOrg, TestBank, ZArchitecture.Core.ReceiptTypes.Cheque,
				TestCheques, "2", 23M);

			using (PaymentApprovalWithAuthorisationForm payForm = new PaymentApprovalWithAuthorisationForm(aPPaymentApproval))
			{
				payForm.FireSaveButton();
				using (NewMatchGroupForm matchingForm = ZFormModaliser.ActiveForm as NewMatchGroupForm)
				{
					Assert("Precondition: Fields should be editable", !aPPaymentApproval.ReadOnly);
				}
			}
		}

		#endregion

		public void TestMessageIfChequeBookUsesSamePrinterDidNotShow_WhenSave()
		{
			AssertMessageIfChequeBookUsesSamePrinterCore(new Action<PaymentApprovalWithAuthorisationForm, AccChequeBook, StmPrintQueue>((form, chequeBook, printer) =>
			{
				form.FireSaveButton();
				AssertNotEquals("Last message should not be that chequebook uses same printer", AccChequeBook.WarningPaymentWithChequeBookWithSamePrinterMessage(printer.SQ_QueueName, chequeBook.AK_CurrentNo), UnitTestUserNotification.Instance.LastMessage.Text);
			}));
		}

		public void TestMessageIfChequeBookUsesSamePrinterDidNotShow_WhenSaveAsDraft()
		{
			AssertMessageIfChequeBookUsesSamePrinterCore(new Action<PaymentApprovalWithAuthorisationForm, AccChequeBook, StmPrintQueue>((form, chequeBook, printer) =>
			{
				form.SaveAsDraftButton_ForTestOnly.PerformClick();
				AssertNotEquals("Last message should not be that chequebook uses same printer", AccChequeBook.WarningPaymentWithChequeBookWithSamePrinterMessage(printer.SQ_QueueName, chequeBook.AK_CurrentNo), UnitTestUserNotification.Instance.LastMessage.Text);
			}));
		}

		public void AssertMessageIfChequeBookUsesSamePrinterCore(Action<PaymentApprovalWithAuthorisationForm, AccChequeBook, StmPrintQueue> action)
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

			PaymentApprovalWithAuthorisation aPPaymentApproval = GetNewTestAPPaymentApproval(TestOrg, testBank, ZArchitecture.Core.ReceiptTypes.Cheque,
				TestCheques, "2", 94M);

			using (PaymentApprovalWithAuthorisationForm form = new PaymentApprovalWithAuthorisationForm(aPPaymentApproval))
			{
				aPPaymentApproval.AV_AB = chequeBook.AK_AB;
				aPPaymentApproval.AV_AK = chequeBook.PK;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				action(form, chequeBook, printer);
			}
		}

		#region AuthorisationButtonsReadOnly

		public void TestAuthorisationButtonsReadOnly()
		{
			SetupDataForTest();
			APPaymentApprovalForTest testPaymentApproval = Factory.New<APPaymentApprovalForTest>();
			testPaymentApproval.AV_OH = TestOrg.PK;
			testPaymentApproval.AV_AB = TestBank.PK;
			testPaymentApproval.AV_PaymentType = ZArchitecture.Core.ReceiptTypes.Cash;
			testPaymentApproval.AV_ChequeOrReference = "3";
			testPaymentApproval.AV_Amount = 46M;

			using (PaymentApprovalWithAuthorisationForm testPayForm = new PaymentApprovalWithAuthorisationForm(testPaymentApproval))
			{
				NewMatchGroupForm matchingForm = null;

				try
				{
					testPaymentApproval.fUserHasAuthoriseLevel1Security = false;
					testPaymentApproval.fUserHasAuthoriseLevel2Security = false;
					testPaymentApproval.fUserHasAuthoriseLevel3Security = false;
					testPaymentApproval.fLevel1AuthorisationRequired = false;
					testPaymentApproval.fLevel2AuthorisationRequired = false;
					testPaymentApproval.fLevel3AuthorisationRequired = false;
					testPayForm.PaymentApprovalBizO_RequiredAuthorisationChanged_ForTestOnly(null, ZString.Empty);
					AssertEquals("FirstAuthorisationButton_ForTestOnly.ReadOnly", true, testPayForm.FirstAuthorisationButton_ForTestOnly.ReadOnly);
					AssertEquals("SecondAuthorisationButton_ForTestOnly.ReadOnly", true, testPayForm.SecondAuthorisationButton_ForTestOnly.ReadOnly);
					AssertEquals("ThirdAuthorisationButton_ForTestOnly.ReadOnly", true, testPayForm.ThirdAuthorisationButton_ForTestOnly.ReadOnly);

					testPaymentApproval.fUserHasAuthoriseLevel1Security = true;
					testPaymentApproval.fUserHasAuthoriseLevel2Security = true;
					testPaymentApproval.fUserHasAuthoriseLevel3Security = true;
					testPaymentApproval.fLevel1AuthorisationRequired = false;
					testPaymentApproval.fLevel2AuthorisationRequired = false;
					testPaymentApproval.fLevel3AuthorisationRequired = false;
					testPayForm.PaymentApprovalBizO_RequiredAuthorisationChanged_ForTestOnly(null, ZString.Empty);
					AssertEquals("FirstAuthorisationButton_ForTestOnly.ReadOnly", true, testPayForm.FirstAuthorisationButton_ForTestOnly.ReadOnly);
					AssertEquals("SecondAuthorisationButton_ForTestOnly.ReadOnly", true, testPayForm.SecondAuthorisationButton_ForTestOnly.ReadOnly);
					AssertEquals("ThirdAuthorisationButton_ForTestOnly.ReadOnly", true, testPayForm.ThirdAuthorisationButton_ForTestOnly.ReadOnly);

					testPaymentApproval.fUserHasAuthoriseLevel1Security = false;
					testPaymentApproval.fUserHasAuthoriseLevel2Security = false;
					testPaymentApproval.fUserHasAuthoriseLevel3Security = false;
					testPaymentApproval.fLevel1AuthorisationRequired = true;
					testPaymentApproval.fLevel2AuthorisationRequired = true;
					testPaymentApproval.fLevel3AuthorisationRequired = true;
					testPayForm.PaymentApprovalBizO_RequiredAuthorisationChanged_ForTestOnly(null, ZString.Empty);
					AssertEquals("FirstAuthorisationButton_ForTestOnly.ReadOnly", true, testPayForm.FirstAuthorisationButton_ForTestOnly.ReadOnly);
					AssertEquals("SecondAuthorisationButton_ForTestOnly.ReadOnly", true, testPayForm.SecondAuthorisationButton_ForTestOnly.ReadOnly);
					AssertEquals("ThirdAuthorisationButton_ForTestOnly.ReadOnly", true, testPayForm.ThirdAuthorisationButton_ForTestOnly.ReadOnly);

					testPaymentApproval.fUserHasAuthoriseLevel1Security = true;
					testPaymentApproval.fUserHasAuthoriseLevel2Security = true;
					testPaymentApproval.fUserHasAuthoriseLevel3Security = true;
					testPaymentApproval.fLevel1AuthorisationRequired = true;
					testPaymentApproval.fLevel2AuthorisationRequired = true;
					testPaymentApproval.fLevel3AuthorisationRequired = true;
					testPayForm.PaymentApprovalBizO_RequiredAuthorisationChanged_ForTestOnly(null, ZString.Empty);
					AssertEquals("FirstAuthorisationButton_ForTestOnly.ReadOnly", false, testPayForm.FirstAuthorisationButton_ForTestOnly.ReadOnly);
					AssertEquals("SecondAuthorisationButton_ForTestOnly.ReadOnly", false, testPayForm.SecondAuthorisationButton_ForTestOnly.ReadOnly);
					AssertEquals("ThirdAuthorisationButton_ForTestOnly.ReadOnly", false, testPayForm.ThirdAuthorisationButton_ForTestOnly.ReadOnly);
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

		#region ReferenceNumberLabel Test

		public void TestReferenceNumberLabel()
		{
			using (PaymentApprovalWithAuthorisationForm form = (PaymentApprovalWithAuthorisationForm)GetFormToBashCore())
			{
				AssertEquals("Text on ChequeNumberLabel should be 'Check / Reference'", "Check / Reference", form.ChequeNoTextBox_ForTestOnly.GetExtension<LabelCaptionRenderer>().Caption);
			}
		}

		#endregion

		public void TestAddressesOnPayments()
		{
			AccountingConfigurationRegistry.Instance.EditPaymentAddress.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			using (var form = (PaymentApprovalWithAuthorisationForm)GetFormToBashCore())
			{
				form.Show();
				AssertEquals(false, form.Payment_ForTestOnly.AV_OA_AddressOverrideInfo.ReadOnly);
				AssertEquals(false, form.Payment_ForTestOnly.AV_OC_ContactOverrideInfo.ReadOnly);
			}

			AccountingConfigurationRegistry.Instance.EditPaymentAddress.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			using (var form = (PaymentApprovalWithAuthorisationForm)GetFormToBashCore())
			{
				form.Show();
				AssertEquals(true, form.Payment_ForTestOnly.AV_OA_AddressOverrideInfo.ReadOnly);
				AssertEquals(true, form.Payment_ForTestOnly.AV_OC_ContactOverrideInfo.ReadOnly);
			}
		}

		public void TestPaymentDetailsButtonReadOnly()
		{
			APPayment newlyCreatedPayment = Factory.NewWithValidTestData<APPayment>();
			PaymentApprovalWithAuthorisation aPPaymentApproval = Factory.NewWithValidTestData<APPaymentApprovalWithAuthorisation>();

			using (PaymentApprovalWithAuthorisationForm payForm = new PaymentApprovalWithAuthorisationForm(aPPaymentApproval))
			{
				Assert(!payForm.PaymentDetailButton_ForTestOnly.ReadOnly);
				payForm.DisplayMode = ODisplayMode.ReadOnly;
				Assert(payForm.PaymentDetailButton_ForTestOnly.ReadOnly);
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

		#region Test Journal IsDeleted when MatchingForm.SaveFactoryResult is SaveFactoryFlag.Cancel

		public void TestJournalIsDeletedWhenFormSaveResultIsCancel_WhenSaveAsDraft()
		{
			SetupDataForTest();
			var paymentApprovalWithAuthorisation = GetNewTestAPPaymentApproval(TestOrg, TestBank, ReceiptTypes.Cheque, TestCheques, "2", 94M);

			using (var paymentApprovalWithAuthorisationForm = new PaymentApprovalWithAuthorisationForm(paymentApprovalWithAuthorisation))
			{
				paymentApprovalWithAuthorisationForm.Show();
				Application.DoEvents();

				paymentApprovalWithAuthorisationForm.PaymentDetailButton_ForTestOnly.PerformClick();
				var matchingForm = ZFormModaliser.ActiveForm as NewMatchGroupForm;
				var matchingBase = matchingForm.FMatchingBase_ForTestOnly;
				var journal = matchingBase.BalancingAPJournals.AddNew();
				journal.AH_AG = ZGuid.Empty;

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				matchingForm.CloseButton.PerformClick();

				AssertNoExceptionThrown("Will not show Critical Validation Error: TransactionWithEmptyGLAccountField", () => paymentApprovalWithAuthorisationForm.SaveAsDraftButton_ForTestOnly.PerformClick());
				AssertEquals(true, journal.IsDeleted);
				AssertEquals("The status should be 'DFT' as save as draft successfully.", PaymentApprovalStatus.Draft, paymentApprovalWithAuthorisation.AV_Status);
			}
		}

		public void TestJournalIsDeletedWhenFormSaveResultIsCancel_WhenClickSaveButton()
		{
			SetupDataForTest();
			var paymentApprovalWithAuthorisation = GetNewTestAPPaymentApproval(TestOrg, TestBank, ReceiptTypes.Cheque, TestCheques, "2", 200M);

			using (var paymentApprovalWithAuthorisationForm = new PaymentApprovalWithAuthorisationForm(paymentApprovalWithAuthorisation))
			{
				paymentApprovalWithAuthorisationForm.Show();
				Application.DoEvents();

				paymentApprovalWithAuthorisationForm.PaymentDetailButton_ForTestOnly.PerformClick();
				var matchingForm = ZFormModaliser.ActiveForm as NewMatchGroupForm;
				var matchingBase = matchingForm.FMatchingBase_ForTestOnly;
				paymentApprovalWithAuthorisation.AV_Discount = -200m;
				paymentApprovalWithAuthorisation.PaymentMatchingBaseObject.CreateTemporaryTransactions();

				AssertEquals("Precondition: Should be 2 transactions in MatchedTransactions", 2, matchingBase.MatchedTransactions.Count);
				AssertEquals("Precondition: Balance should be 0", 0M, matchingBase.Balance);

				var journal = matchingBase.BalancingAPJournals.AddNew();
				journal.AH_AG = ZGuid.Empty;

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				matchingForm.CloseButton.PerformClick();

				AssertNoExceptionThrown("Will not show Critical Validation Error: TransactionWithEmptyGLAccountField", () => paymentApprovalWithAuthorisationForm.PostingButtonsUserControl_ForTestOnly.SaveButton.PerformClick());
				AssertEquals(true, journal.IsDeleted);
				AssertEquals("The status should be 'APP' as save successfully.", PaymentApprovalStatus.FullyApproved, paymentApprovalWithAuthorisation.AV_Status);
			}
		}

		#endregion

		#region TestChangingMatchDateWillNotAffectPostedBankFeeJournalWhenReopenMatchForm

		[TestDate(2012, 11, 1, 11, 0, 0)]
		public void TestChangingMatchDateWillNotAffectPostedBankFeeJournalWhenReopenMatchForm()
		{
			SetupDataForTest();
			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			PaymentApprovalWithAuthorisation paymentApproval = GetNewTestAPPaymentApproval(TestOrg, TestBank, ZArchitecture.Core.ReceiptTypes.Cheque, TestCheques, "2", 200m);
			paymentApproval.AV_PostDate = ZDateTime.Today.AddDays(-2);

			using (var paymentApprovalForm = new PaymentApprovalWithAuthorisationForm(paymentApproval))
			{
				NewMatchGroupForm matchingForm = null;

				try
				{
					paymentApprovalForm.Show();
					paymentApprovalForm.PaymentDetailButton_ForTestOnly.PerformClick();
					matchingForm = ZFormModaliser.ActiveForm as NewMatchGroupForm;
					AssertNotNull("The active form should be the matching form", matchingForm);
					var matchingBizO = matchingForm.BusinessEntity as APPaymentApprovalMatching;

					var bankFee = matchingBizO.GetMiscellaneousTransaction(TransactionTypes.Journal);
					bankFee.AH_OSExTaxAmount = 200m;
					matchingBizO.AddMiscellaneousTransaction(bankFee);

					var balance = matchingBizO.Balance;
					AssertNotNull("Matching Object should be APMatchingBase", matchingBizO);
					AssertEquals("Should be 2 transactions in MatchedTransactions", 2, matchingBizO.MatchedTransactions.Count);
					AssertEquals("Balance should be 0", 0M, matchingBizO.Balance);

					matchingForm.FireSaveButton();
					matchingForm.Close();
					Assert("Matching form should be closed", !matchingForm.Visible);

					var bankFeeJournal = Factory.LoadTop1<APJournal>(new ZQuery(AccTransactionHeaderSchema.PK, bankFee.PK));
					AssertEquals(bankFeeJournal.AH_PostDate, ZDateTime.Today.AddDays(-2));

					//reopen match group form
					paymentApprovalForm.PaymentDetailButton_ForTestOnly.PerformClick();
					matchingForm = ZFormModaliser.ActiveForm as NewMatchGroupForm;
					AssertNotNull("The active form should be the matching form", matchingForm);
					matchingBizO = matchingForm.BusinessEntity as APPaymentApprovalMatching;
					AssertEquals("Should be 2 transactions in MatchedTransactions", 2, matchingBizO.MatchedTransactions.Count);
					AssertEquals("Balance should be 0", 0M, matchingBizO.Balance);

					matchingBizO.MatchDate = ZDateTime.Today;
					AssertEquals("Changing the MatchDate will not affect the posted bank fee journal", bankFeeJournal.AH_PostDate, ZDateTime.Today.AddDays(-2));
					AssertNull("The newly opened Matching Form should not be bound again to a bank fee journal that has already been posted.", matchingBizO.BankFeeBizO_ForTestOnly);

					matchingForm.FireSaveButton();
					matchingForm.Close();
					Assert("Matching form should be closed", !matchingForm.Visible);
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

		#region Implementation

		OrgHeader TestOrg;
		AccountingPeriodTestHelper PeriodHelper;
		AccBankAccount TestBank;
		AccChequeBook TestCheques;
		OrgHeader TestOrg1;

		protected override void SetUp()
		{
			base.SetUp();

			TestOrg = Factory.NewWithValidTestData<OrgHeader>();
			TestOrg.OH_Code = Accounting.Business.TestObjectCreator.GetRandomString(10);
			TestOrg.CompanyData.OB_IsCreditor = true;
			TestOrg.CompanyData.SetAPTaxApplicable(false);
			Factory.Save();
		}

		protected override Form GetFormToBashCore()
		{
			PaymentApprovalWithAuthorisation testPaymentApproval = Factory.New<APPaymentApprovalWithAuthorisation>();
			testPaymentApproval.AV_OH = TestOrg.PK;
			testPaymentApproval.HasChanges = false;

			var result = new PaymentApprovalWithAuthorisationForm(testPaymentApproval);
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

			TestOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			TestOrg1.CompanyData.OB_IsDebtor = true;
			TestOrg1.CompanyData.OB_IsCreditor = true;

			Factory.Save();
		}

		PaymentApprovalWithAuthorisation GetNewTestAPPaymentApproval(OrgHeader org, AccBankAccount bank, ZString receiptType,
			AccChequeBook chequeBook, ZString chequeOrRef, ZDecimal amount)
		{
			APPaymentApprovalWithAuthorisation paymentApproval = Factory.New<APPaymentApprovalWithAuthorisation>();
			paymentApproval.AV_OH = org.PK;
			paymentApproval.AV_AB = bank.PK;
			paymentApproval.AV_PaymentType = receiptType;
			paymentApproval.AV_AK = chequeBook.PK;
			paymentApproval.AV_ChequeOrReference = chequeOrRef;
			paymentApproval.AV_Amount = amount;
			return paymentApproval;
		}

		TestObjectCreator fTestObjectCreator;
		TestObjectCreator TestObjectCreator
		{
			get { return fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(Factory)); }
		}

		#endregion

		public class MockPaymentApprovalWithAuthorisationForm : PaymentApprovalWithAuthorisationForm
		{
			public MockPaymentApprovalWithAuthorisationForm(PaymentApprovalWithAuthorisation paymentApprovalBizO)
				: base(paymentApprovalBizO)
			{
			}

			protected override PaymentPrintManager PrintManager
			{
				get
				{
					if (fTestPrintManager == null)
					{
						fTestPrintManager = new PaymentPrintManager.TestPaymentPrintManager(Payment.TransactionHeader.PK.ToGuid(), ZArchitecture.Core.TransactionTypes.Payment, BusinessEntity.Factory);
					}

					return fTestPrintManager;
				}
			}

			PaymentPrintManager.TestPaymentPrintManager fTestPrintManager;
		}

		protected class APPaymentApprovalForTest : APPaymentApprovalWithAuthorisation
		{
			public APPaymentApprovalForTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override ZBool Level1AuthorisationRequiredCore
			{
				get { return fLevel1AuthorisationRequired; }
			}

			public ZBool fLevel1AuthorisationRequired;

			protected override ZBool Level2AuthorisationRequiredCore
			{
				get { return fLevel2AuthorisationRequired; }
			}

			public ZBool fLevel2AuthorisationRequired;

			protected override ZBool Level3AuthorisationRequiredCore
			{
				get { return fLevel3AuthorisationRequired; }
			}

			public ZBool fLevel3AuthorisationRequired;

			protected override ZBool UserHasAuthoriseLevel1SecurityCore
			{
				get { return fUserHasAuthoriseLevel1Security; }
			}

			public ZBool fUserHasAuthoriseLevel1Security;

			protected override ZBool UserHasAuthoriseLevel2SecurityCore
			{
				get { return fUserHasAuthoriseLevel2Security; }
			}

			public ZBool fUserHasAuthoriseLevel2Security;

			protected override ZBool UserHasAuthoriseLevel3SecurityCore
			{
				get { return fUserHasAuthoriseLevel3Security; }
			}

			public ZBool fUserHasAuthoriseLevel3Security;

			protected override PaymentAuthorisationSettings GetAuthorisationRequired(ZDecimal localAmount)
			{
				if (Result == null)
				{
					Result = new PaymentAuthorisationSettingsCollection().AddNew();
					Result.Amount = 1;
					Result.Range = RangeCodes.Above;
					Result.AuthorisationRequirement = AuthorisationCodes.AllThreeApprovalRequired;
				}
				return Result;
			}

			PaymentAuthorisationSettings Result;
		}
	}
}
