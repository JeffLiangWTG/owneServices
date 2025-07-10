using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Testing
{
	[TestedType(typeof(MatchEPaymentRecipientsForm))]
	public class MatchEPaymentRecipientsFormAccountingZFormBasherTest : AccountingZFormBasherTest
	{
		MatchEPaymentRecipients GetFormBizO()
		{
			return new MatchEPaymentRecipients(Factory);
		}

		MatchEPaymentRecipientsForm GetForm(MatchEPaymentRecipients matchEPaymentRecipients)
		{
			return new MatchEPaymentRecipientsForm(matchEPaymentRecipients);
		}

		protected override Form GetFormToBashCore()
		{
			return GetForm(GetFormBizO());
		}

		public override void TestFormVerb()
		{
			using (AccountingZForm testForm = (AccountingZForm)GetFormToBashCore())
			{
				testForm.DisplayMode = ODisplayMode.New;
				AssertEquals("Verb should be 'New'", "New", testForm.FormVerb);
			}
		}

		public void TestLinkBeneficiaryToAPAccountDetail()
		{
			var beneficiary = Factory.NewWithValidTestData<AccEPaymentBeneficiary>();
			beneficiary.ABF_BeneficiaryFullName = "Peter Walter";
			beneficiary.ABF_BeneficiaryNickName = "Walter";
			beneficiary.ABF_RX_NKAccountCurrency = "AUD";
			beneficiary.ABF_RN_NKCountryCode = "AU";
			beneficiary.ABF_GC_Company = GlbCompany.CurrentCompany.PK;
			Factory.Save();

			var accountDetails = Factory.NewWithValidTestData<AccAPAccountDetails>();
			accountDetails.A1_PaymentMethod = "EPO";
			Factory.Save();
			Assert("Precondition - link not set", accountDetails.A1_EPaymentBeneficiaryId.IsEmpty);

			var matchEPaymentRecipients = new MatchEPaymentRecipients(Factory, accountDetails);
			using (var form = new MatchEPaymentRecipientsForm(matchEPaymentRecipients))
			{
				form.Show();
				form.MatchRecipientsControl_ForTestOnly.FilteredRecipientsFilterControl_PerformSearch(this, null);
				form.MatchRecipientsControl_ForTestOnly.FilteredRecipientsGrid_ForTestOnly.Select(0);
				form.ValidateAndSave_ForTestOnly();
			}

			AssertEquals(beneficiary.PK, accountDetails.A1_EPaymentBeneficiaryId);
			AssertEquals(beneficiary.ABF_BeneficiaryFullName, accountDetails.A1_AccountName);
			AssertEquals(beneficiary.ABF_RX_NKAccountCurrency, accountDetails.A1_RX_NKAccountCurrency);
			AssertEquals(beneficiary.ABF_RN_NKCountryCode, accountDetails.A1_RN_NKCountryCode);
			AssertEquals(beneficiary.ABF_BankName, accountDetails.A1_BankName);
			AssertEquals(beneficiary.ABF_BankBranchName, accountDetails.A1_BankBranchName);
			AssertEquals(beneficiary.ABF_BankBsb, accountDetails.A1_BankBsb);
			AssertEquals(beneficiary.ABF_BankAccount, accountDetails.A1_BankAccount);
			AssertEquals(beneficiary.ABF_BankSwift, accountDetails.A1_BankSwift);
			AssertEquals(beneficiary.ABF_BankAddress1, accountDetails.A1_BankAddress1);
			AssertEquals(beneficiary.ABF_BankAddress2, accountDetails.A1_BankAddress2);
			AssertEquals(beneficiary.ABF_BankAddress3, accountDetails.A1_BankAddress3);
		}

		public void TestLinkOtherThanOneBeneficiaryShowErrorMessage()
		{
			var beneficiary = Factory.NewWithValidTestData<AccEPaymentBeneficiary>();
			beneficiary.ABF_BeneficiaryFullName = "Peter Walter";
			beneficiary.ABF_BeneficiaryNickName = "Walter";
			beneficiary.ABF_RX_NKAccountCurrency = "AUD";
			beneficiary.ABF_RN_NKCountryCode = "AU";
			beneficiary.ABF_GC_Company = GlbCompany.CurrentCompany.PK;

			var anotherBeneficiary = Factory.NewWithValidTestData<AccEPaymentBeneficiary>();
			anotherBeneficiary.ABF_BeneficiaryFullName = "Jane White";
			anotherBeneficiary.ABF_BeneficiaryNickName = "White";
			anotherBeneficiary.ABF_RX_NKAccountCurrency = "USD";
			anotherBeneficiary.ABF_RN_NKCountryCode = "US";
			anotherBeneficiary.ABF_GC_Company = GlbCompany.CurrentCompany.PK;

			Factory.Save();

			var accountDetails = Factory.NewWithValidTestData<AccAPAccountDetails>();
			accountDetails.A1_PaymentMethod = "EPO";
			Factory.Save();
			Assert("Precondition - link not set", accountDetails.A1_EPaymentBeneficiaryId.IsEmpty);

			var matchEPaymentRecipients = new MatchEPaymentRecipients(Factory, accountDetails);
			using (var form = new MatchEPaymentRecipientsForm(matchEPaymentRecipients))
			{
				form.Show();
				form.MatchRecipientsControl_ForTestOnly.FilteredRecipientsFilterControl_PerformSearch(this, null);
				form.MatchRecipientsControl_ForTestOnly.FilteredRecipientsGrid_ForTestOnly.SelectAllElements();
				AssertEquals(2, form.MatchRecipientsControl_ForTestOnly.FilteredRecipientsGrid_ForTestOnly.SelectedRowCount);
				form.ValidateAndSave_ForTestOnly();
				AssertEquals("You must select exactly one recipient from the grid.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessages();

				form.MatchRecipientsControl_ForTestOnly.FilteredRecipientsGrid_ForTestOnly.UnSelectAll();
				AssertEquals(0, form.MatchRecipientsControl_ForTestOnly.FilteredRecipientsGrid_ForTestOnly.SelectedRowCount);
				form.ValidateAndSave_ForTestOnly();
				AssertEquals("You must select exactly one recipient from the grid.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[TestDate(2021, 8, 18)]
		public void TestSyncButtonClicked()
		{
			var accountDetails = Factory.NewWithValidTestData<AccAPAccountDetails>();
			accountDetails.A1_PaymentMethod = "EPO";
			Factory.Save();

			var matchEPaymentRecipients = new MatchEPaymentRecipients(Factory, accountDetails);
			using (var form = new MatchEPaymentRecipientsForm(matchEPaymentRecipients))
			{
				form.Show();
				form.SyncButton_Click(this, null);
				AssertEquals("In order to request OFX Recipient List, you must have a registered OFX account. If you already have an account, please ensure that you have configured an OFX E-Payment Account in the Bank Account Maintenance module and that your staff profile is registered and authorized on this Bank Account.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var bankAccount =  Factory.NewWithValidTestData<AccBankAccount>();
				bankAccount.AB_GC = GlbCompany.CurrentCompany.PK;
				bankAccount.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.EPA;
				Factory.Save();
				form.SyncButton_Click(this, null);
				AssertEquals("In order to request OFX Recipient List, you must have a registered OFX account. If you already have an account, please ensure that you have configured an OFX E-Payment Account in the Bank Account Maintenance module and that your staff profile is registered and authorized on this Bank Account.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				var token = Factory.NewWithValidTestData<AccEPaymentStaffToken>();
				token.TK_GC = GlbCompany.CurrentCompany.PK;
				token.TK_GS_NKStaffCode = GlbStaff.CurrentUser.GS_Code;
				token.TK_Status = AccEPaymentStaffTokenLookups.StatusCodes.NotAuthorised;
				Factory.Save();
				form.SyncButton_Click(this, null);
				AssertEquals(@"You need to authorize your OFX user account before proceeding.
Please note that the authorization process can take up to several minutes.
Would you like to authorize your account now?", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(WebUrlLauncher.LastUrlLaunched.StartsWith(AccountingMasterFilesRegistry.Instance.OFXOAuthLoginWebURL.Value));

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				token.TK_ExpiryUtc = new CargoWise.Types.ZDateTime(2021, 8, 15);
				token.TK_Status = AccEPaymentStaffTokenLookups.StatusCodes.Authorised;
				Factory.Save();
				form.SyncButton_Click(this, null);
				AssertEquals(@"You need to authorize your OFX user account before proceeding.
Please note that the authorization process can take up to several minutes.
Would you like to authorize your account now?", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				token.TK_ExpiryUtc = new CargoWise.Types.ZDateTime(2021, 10, 15);
				Factory.Save();

				var request1 = Factory.NewWithValidTestData<AccEPaymentBeneficiaryRequest>();
				request1.ABR_GC_Company = GlbCompany.CurrentCompany.PK;
				request1.ABR_Status = EPaymentStatusCodes.BeneficiaryRequest.Queued;
				Factory.Save();
				form.SyncButton_Click(this, null);
				AssertEquals("A request has already been generated for the recipients list", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				request1.ABR_Status = EPaymentStatusCodes.BeneficiaryRequest.Requested;
				Factory.Save();
				form.SyncButton_Click(this, null);
				AssertEquals("A request has already been sent to OFX for a recipients list. This may take up to several minutes to receive. Are you sure you wish to generate another request?", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				var requests = Factory.Load<AccEPaymentBeneficiaryRequest>(new ZQuery());
				var beforeCount = requests.Length;
				form.SyncButton_Click(this, null);
				AssertEquals("Recipients list requested from third party provider OFX. This may take several minutes to receive.", UnitTestUserNotification.Instance.LastMessage.Text);
				requests = Factory.Load<AccEPaymentBeneficiaryRequest>(new ZQuery());
				var afterCount = requests.Length;
				AssertEquals("Expect 1 new request being created", beforeCount + 1, afterCount);
			}
		}
	}
}
