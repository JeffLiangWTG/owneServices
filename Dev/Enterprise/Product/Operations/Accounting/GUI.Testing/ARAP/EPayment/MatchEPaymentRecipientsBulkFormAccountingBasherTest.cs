using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Testing
{
	[TestedType(typeof(MatchEPaymentRecipientsBulkForm))]
	public class MatchEPaymentRecipientsBulkFormAccountingZFormBasherTest : AccountingZFormBasherTest
	{
		MatchEPaymentRecipientsBulk GetFormBizO()
		{
			return new MatchEPaymentRecipientsBulk(Factory);
		}

		MatchEPaymentRecipientsBulkForm GetForm(MatchEPaymentRecipientsBulk matchEPaymentRecipients)
		{
			return new MatchEPaymentRecipientsBulkForm(matchEPaymentRecipients);
		}

		protected override Form GetFormToBashCore()
		{
			return GetForm(GetFormBizO());
		}

		public void TestContorlPropertyBinding()
		{
			var matchEPaymentRecipientsBulk = new MatchEPaymentRecipientsBulk(Factory);
			using (var form = new MatchEPaymentRecipientsBulkFormForTestOnly(matchEPaymentRecipientsBulk))
			{
				form.Show();

				AssertEquals("AllowOverrideDefault", form.BindingSource_ForTestOnly.GetBindingMember(form.CheckBoxOverrideDefault_ForTestOnly));
				AssertEquals("CreditorPK", form.BindingSource_ForTestOnly.GetBindingMember(form.CreditorGuidFindBox_ForTestOnly));
				AssertEquals("DefaultPaymentReason", form.BindingSource_ForTestOnly.GetBindingMember(form.DefaultPaymentReasonDropEdit_ForTestOnly));
				AssertEquals("DefaultBankAccountPK", form.BindingSource_ForTestOnly.GetBindingMember(form.BankAccountGuidFindBox_ForTestOnly));
				AssertEquals("AgreedPaymentMethod", form.BindingSource_ForTestOnly.GetBindingMember(form.DropEditAgreedPaymentMethod_ForTestOnly));
				AssertEquals("PaymentReferenceType", form.BindingSource_ForTestOnly.GetBindingMember(form.DropEditPaymentReferenceType_ForTestOnly));
				AssertEquals("PaymentReference", form.BindingSource_ForTestOnly.GetBindingMember(form.TextPaymentReference_ForTestOnly));
			}
		}

		public void TestTextPaymentReference_Visible()
		{
			var matchEPaymentRecipientsBulk = new MatchEPaymentRecipientsBulk(Factory);
			matchEPaymentRecipientsBulk.CreditorPK = TestObjectCreator.Creditor1.PK;

			using (var form = new MatchEPaymentRecipientsBulkFormForTestOnly(matchEPaymentRecipientsBulk))
			{
				form.Show();

				matchEPaymentRecipientsBulk.PaymentReferenceType = "";
				form.TextPaymentReference_ForTestOnly.Visible = false;

				foreach (CodeDescriptionPair referenceType in matchEPaymentRecipientsBulk.PaymentReferenceTypeList)
				{
					matchEPaymentRecipientsBulk.PaymentReferenceType = referenceType.Code;
					AssertEquals(referenceType.Code == EPaymentReferenceTypes.FreeText, form.TextPaymentReference_ForTestOnly.Visible);
				}
			}
		}

		public void TestPaymentReferenceFreature()
		{
			var matchEPaymentRecipientsBulk = new MatchEPaymentRecipientsBulk(Factory);

			using (var form = new MatchEPaymentRecipientsBulkFormForTestOnly(matchEPaymentRecipientsBulk))
			{
				form.Show();

				AssertEquals(true, form.DropEditPaymentReferenceType_ForTestOnly.Visible);
				AssertEquals(false, form.TextPaymentReference_ForTestOnly.Visible);
			}
		}

		public override void TestFormVerb()
		{
			using (AccountingZForm testForm = (AccountingZForm)GetFormToBashCore())
			{
				testForm.DisplayMode = ODisplayMode.New;
				AssertEquals("Verb should be 'New'", "New", testForm.FormVerb);
			}
		}

		public void TestMatchBeneficiaryToOrgWithoutDefaultPaymentReason_NoExistingBankAccount()
		{
			var beneficiary = Factory.NewWithValidTestData<AccEPaymentBeneficiary>();
			beneficiary.ABF_BeneficiaryFullName = "Peter Walter";
			beneficiary.ABF_BeneficiaryNickName = "Walter";
			beneficiary.ABF_RX_NKAccountCurrency = "AUD";
			beneficiary.ABF_RN_NKCountryCode = "AU";
			beneficiary.ABF_GC_Company = GlbCompany.CurrentCompany.PK;
			Factory.Save();

			AssertEquals(0, TestObjectCreator.Creditor1.CompanyData.AccountDetailsCollection.Count);

			var matchEPaymentRecipientsBulk = new MatchEPaymentRecipientsBulk(Factory);
			using (var form = new MatchEPaymentRecipientsBulkForm(matchEPaymentRecipientsBulk))
			{
				form.Show();
				Application.DoEvents();
				form.MatchRecipientsControl_ForTestOnly.FilteredRecipientsFilterControl_PerformSearch(this, null);
				form.MatchRecipientsControl_ForTestOnly.FilteredRecipientsGrid_ForTestOnly.Select(0);
				matchEPaymentRecipientsBulk.CreditorPK = TestObjectCreator.Creditor1.PK;
				matchEPaymentRecipientsBulk.PaymentReferenceType = EPaymentReferenceTypes.FreeText;
				matchEPaymentRecipientsBulk.PaymentReference = "123456789";
				matchEPaymentRecipientsBulk.DefaultPaymentReason = ZString.Empty;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				var expectedQuestion = "You have not selected a Default Payment Method. If your FX Provider requires a Payment Method, you will need to add a Payment Method to each payment you create which will be sent via this FX Provider. Do you wish to continue without adding a Default Payment Method?";
				form.MatchButton_Click(this, null);
				AssertEquals(expectedQuestion, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(0, TestObjectCreator.Creditor1.CompanyData.AccountDetailsCollection.Count);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.MatchButton_Click(this, null);
				Assert(UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedQuestion));
				AssertEquals("Payables Organization successfully matched", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(1, TestObjectCreator.Creditor1.CompanyData.AccountDetailsCollection.Count);
				var accountDetails = TestObjectCreator.Creditor1.CompanyData.AccountDetailsCollection[0];
				AssertEquals(beneficiary.PK, accountDetails.A1_EPaymentBeneficiaryId);
				AssertEquals(ZString.Empty, accountDetails.A1_EPaymentReasonCode);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.MatchButton_Click(this, null);
				AssertEquals("Please choose an unmatched recipient first", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestMatchBeneficiaryToOrg_NoExistingBankAccount()
		{
			var beneficiary = Factory.NewWithValidTestData<AccEPaymentBeneficiary>();
			beneficiary.ABF_BeneficiaryFullName = "Peter Walter";
			beneficiary.ABF_BeneficiaryNickName = "Walter";
			beneficiary.ABF_RX_NKAccountCurrency = "AUD";
			beneficiary.ABF_RN_NKCountryCode = "AU";
			beneficiary.ABF_GC_Company = GlbCompany.CurrentCompany.PK;
			Factory.Save();

			AssertEquals(0, TestObjectCreator.Creditor1.CompanyData.AccountDetailsCollection.Count);

			var matchEPaymentRecipientsBulk = new MatchEPaymentRecipientsBulk(Factory);
			using (var form = new MatchEPaymentRecipientsBulkForm(matchEPaymentRecipientsBulk))
			{
				form.Show();
				Application.DoEvents();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.MatchButton_Click(this, null);
				AssertEquals("Please choose a recipient first", UnitTestUserNotification.Instance.LastMessage.Text);

				form.MatchRecipientsControl_ForTestOnly.FilteredRecipientsFilterControl_PerformSearch(this, null);
				form.MatchRecipientsControl_ForTestOnly.FilteredRecipientsGrid_ForTestOnly.Select(0);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.MatchButton_Click(this, null);
				AssertEquals("Please choose a valid creditor first", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				matchEPaymentRecipientsBulk.CreditorPK = TestObjectCreator.Creditor1.PK;
				matchEPaymentRecipientsBulk.PaymentReferenceType = EPaymentReferenceTypes.FreeText;
				matchEPaymentRecipientsBulk.PaymentReference = "123456789";
				AssertEquals(EPaymentReasonCodes.OFXReasonCodes.ServicesTrade, matchEPaymentRecipientsBulk.DefaultPaymentReason);
				form.MatchButton_Click(this, null);
				Assert(!UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("You have not selected a Default Payment Method.If your FX Provider requires a Payment Method, you will need to add a Payment Method to each payment you create which will be sent via this FX Provider.Do you wish to continue without adding a Default Payment Method?"));
				AssertEquals("Payables Organization successfully matched", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(1, TestObjectCreator.Creditor1.CompanyData.AccountDetailsCollection.Count);
				var accountDetails = TestObjectCreator.Creditor1.CompanyData.AccountDetailsCollection[0];
				AssertEquals(beneficiary.PK, accountDetails.A1_EPaymentBeneficiaryId);
				AssertEquals(EPaymentMethods.EPaymentViaOFX, accountDetails.A1_PaymentMethod);
				Assert(accountDetails.A1_IsDefaultAccount);
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
				AssertEquals(EPaymentReferenceTypes.FreeText, accountDetails.A1_EPaymentReferenceType);
				AssertEquals("123456789", accountDetails.A1_EPaymentReference);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.MatchButton_Click(this, null);
				AssertEquals("Please choose an unmatched recipient first", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestMatchBeneficiaryToOrg_PaymentReference()
		{
			var beneficiary = Factory.NewWithValidTestData<AccEPaymentBeneficiary>();
			beneficiary.ABF_BeneficiaryFullName = "Peter Walter";
			beneficiary.ABF_BeneficiaryNickName = "Walter";
			beneficiary.ABF_RX_NKAccountCurrency = "AUD";
			beneficiary.ABF_RN_NKCountryCode = "AU";
			beneficiary.ABF_GC_Company = GlbCompany.CurrentCompany.PK;
			Factory.Save();

			AssertEquals(0, TestObjectCreator.Creditor1.CompanyData.AccountDetailsCollection.Count);

			var matchEPaymentRecipientsBulk = new MatchEPaymentRecipientsBulk(Factory);
			using (var form = new MatchEPaymentRecipientsBulkForm(matchEPaymentRecipientsBulk))
			{
				form.Show();
				Application.DoEvents();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.MatchButton_Click(this, null);
				AssertEquals("Please choose a recipient first", UnitTestUserNotification.Instance.LastMessage.Text);

				form.MatchRecipientsControl_ForTestOnly.FilteredRecipientsFilterControl_PerformSearch(this, null);
				form.MatchRecipientsControl_ForTestOnly.FilteredRecipientsGrid_ForTestOnly.Select(0);
				matchEPaymentRecipientsBulk.CreditorPK = TestObjectCreator.Creditor1.PK;
				AssertEquals("INV", matchEPaymentRecipientsBulk.PaymentReferenceType);
				AssertEquals(ZString.Empty, matchEPaymentRecipientsBulk.PaymentReference);
				AssertEquals(EPaymentReasonCodes.OFXReasonCodes.ServicesTrade, matchEPaymentRecipientsBulk.DefaultPaymentReason);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				matchEPaymentRecipientsBulk.PaymentReferenceType = EPaymentReferenceTypes.FreeText;
				matchEPaymentRecipientsBulk.PaymentReference = "123456789";
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.MatchButton_Click(this, null);
				Assert(!UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("You have not selected a Default Payment Method.If your FX Provider requires a Payment Method, you will need to add a Payment Method to each payment you create which will be sent via this FX Provider.Do you wish to continue without adding a Default Payment Method?"));
				AssertEquals("Payables Organization successfully matched", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(1, TestObjectCreator.Creditor1.CompanyData.AccountDetailsCollection.Count);
				var accountDetails = TestObjectCreator.Creditor1.CompanyData.AccountDetailsCollection[0];
				AssertEquals(beneficiary.PK, accountDetails.A1_EPaymentBeneficiaryId);
				AssertEquals(EPaymentMethods.EPaymentViaOFX, accountDetails.A1_PaymentMethod);
				Assert(accountDetails.A1_IsDefaultAccount);
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
				AssertEquals(EPaymentReferenceTypes.FreeText, accountDetails.A1_EPaymentReferenceType);
				AssertEquals("123456789", accountDetails.A1_EPaymentReference);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.MatchButton_Click(this, null);
				AssertEquals("Please choose an unmatched recipient first", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestMatchBeneficiaryToOrgWithoutDefaultPaymentReason_HasExistingBankAccount()
		{
			var beneficiary = Factory.NewWithValidTestData<AccEPaymentBeneficiary>();
			beneficiary.ABF_BeneficiaryFullName = "Peter Walter";
			beneficiary.ABF_BeneficiaryNickName = "Walter";
			beneficiary.ABF_RX_NKAccountCurrency = "AUD";
			beneficiary.ABF_RN_NKCountryCode = "AU";
			beneficiary.ABF_GC_Company = GlbCompany.CurrentCompany.PK;

			TestObjectCreator.Creditor1.MiscServ.OM_AB_APDefaultBankAccount = TestObjectCreator.AUDBankAccount.PK;
			TestObjectCreator.Creditor1.CompanyData.OB_APCreditAgreedPaymentMethod = OrgConstants.CreditAgreedPaymentMethods.Code.BusinessCheck;

			var accountDetail1 = TestObjectCreator.Creditor1.CompanyData.AccountDetailsCollection.AddNew();
			accountDetail1.A1_RX_NKAccountCurrency = "AUD";
			accountDetail1.A1_IsDefaultAccount = true;
			accountDetail1.A1_PaymentMethod = EPaymentMethods.EPaymentViaOFX;
			accountDetail1.A1_EPaymentReasonCode = EPaymentReasonCodes.OFXReasonCodes.BusinessConsultancyAndPRSevices;
			Factory.Save();

			var matchEPaymentRecipientsBulk = new MatchEPaymentRecipientsBulk(Factory);
			using (var form = new MatchEPaymentRecipientsBulkForm(matchEPaymentRecipientsBulk))
			{
				form.Show();
				Application.DoEvents();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.MatchRecipientsControl_ForTestOnly.FilteredRecipientsFilterControl_PerformSearch(this, null);
				form.MatchRecipientsControl_ForTestOnly.FilteredRecipientsGrid_ForTestOnly.Select(0);
				matchEPaymentRecipientsBulk.CreditorPK = TestObjectCreator.Creditor1.PK;
				matchEPaymentRecipientsBulk.PaymentReferenceType = EPaymentReferenceTypes.FreeText;
				matchEPaymentRecipientsBulk.PaymentReference = "123456789";
				matchEPaymentRecipientsBulk.AllowOverrideDefault = true;
				matchEPaymentRecipientsBulk.DefaultBankAccountPK = TestObjectCreator.AUDBankAccount2.PK;
				matchEPaymentRecipientsBulk.AgreedPaymentMethod = OrgConstants.CreditAgreedPaymentMethods.Code.CreditCard;
				matchEPaymentRecipientsBulk.DefaultPaymentReason = ZString.Empty;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				var expectedQuestion = "You have not selected a Default Payment Method. If your FX Provider requires a Payment Method, you will need to add a Payment Method to each payment you create which will be sent via this FX Provider. Do you wish to continue without adding a Default Payment Method?";
				form.MatchButton_Click(this, null);
				AssertEquals(expectedQuestion, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(1, TestObjectCreator.Creditor1.CompanyData.AccountDetailsCollection.Count);
				Assert(accountDetail1.A1_EPaymentBeneficiaryId.IsEmpty);
				AssertEquals(EPaymentReasonCodes.OFXReasonCodes.BusinessConsultancyAndPRSevices, accountDetail1.A1_EPaymentReasonCode);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.MatchButton_Click(this, null);
				AssertEquals("Payables Organization successfully matched", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedQuestion));
				AssertEquals(1, TestObjectCreator.Creditor1.CompanyData.AccountDetailsCollection.Count);
				AssertEquals(beneficiary.PK, accountDetail1.A1_EPaymentBeneficiaryId);
				AssertEquals(ZString.Empty, accountDetail1.A1_EPaymentReasonCode);
			}
		}

		public void TestMatchBeneficiaryToOrg_HasExistingBankAccount()
		{
			var beneficiary = Factory.NewWithValidTestData<AccEPaymentBeneficiary>();
			beneficiary.ABF_BeneficiaryFullName = "Peter Walter";
			beneficiary.ABF_BeneficiaryNickName = "Walter";
			beneficiary.ABF_RX_NKAccountCurrency = "AUD";
			beneficiary.ABF_RN_NKCountryCode = "AU";
			beneficiary.ABF_GC_Company = GlbCompany.CurrentCompany.PK;

			TestObjectCreator.Creditor1.MiscServ.OM_AB_APDefaultBankAccount = TestObjectCreator.AUDBankAccount.PK;
			TestObjectCreator.Creditor1.CompanyData.OB_APCreditAgreedPaymentMethod = OrgConstants.CreditAgreedPaymentMethods.Code.BusinessCheck;

			var accountDetail1 = TestObjectCreator.Creditor1.CompanyData.AccountDetailsCollection.AddNew();
			accountDetail1.A1_RX_NKAccountCurrency = "AUD";
			accountDetail1.A1_IsDefaultAccount = false;
			accountDetail1.A1_PaymentMethod = EPaymentMethods.EPaymentViaOFX;

			var accountDetail2 = TestObjectCreator.Creditor1.CompanyData.AccountDetailsCollection.AddNew();
			accountDetail2.A1_RX_NKAccountCurrency = "AUD";
			accountDetail2.A1_IsDefaultAccount = true;
			accountDetail2.A1_PaymentMethod = EPaymentMethods.EPaymentViaOFX;
			Factory.Save();

			var matchEPaymentRecipientsBulk = new MatchEPaymentRecipientsBulk(Factory);
			using (var form = new MatchEPaymentRecipientsBulkForm(matchEPaymentRecipientsBulk))
			{
				form.Show();
				Application.DoEvents();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.MatchRecipientsControl_ForTestOnly.FilteredRecipientsFilterControl_PerformSearch(this, null);
				form.MatchRecipientsControl_ForTestOnly.FilteredRecipientsGrid_ForTestOnly.Select(0);
				matchEPaymentRecipientsBulk.CreditorPK = TestObjectCreator.Creditor1.PK;
				matchEPaymentRecipientsBulk.AllowOverrideDefault = true;
				matchEPaymentRecipientsBulk.DefaultBankAccountPK = TestObjectCreator.AUDBankAccount2.PK;
				matchEPaymentRecipientsBulk.AgreedPaymentMethod = OrgConstants.CreditAgreedPaymentMethods.Code.CreditCard;
				matchEPaymentRecipientsBulk.PaymentReferenceType = EPaymentReferenceTypes.FreeText;
				matchEPaymentRecipientsBulk.PaymentReference = "123456789";
				AssertEquals(EPaymentReasonCodes.OFXReasonCodes.ServicesTrade, matchEPaymentRecipientsBulk.DefaultPaymentReason);

				form.MatchButton_Click(this, null);
				AssertEquals("Payables Organization successfully matched", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(!UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("You have not selected a Default Payment Method.If your FX Provider requires a Payment Method, you will need to add a Payment Method to each payment you create which will be sent via this FX Provider.Do you wish to continue without adding a Default Payment Method?"));
				AssertEquals(2, TestObjectCreator.Creditor1.CompanyData.AccountDetailsCollection.Count);

				Assert(accountDetail1.A1_EPaymentBeneficiaryId.IsEmpty);

				AssertEquals(beneficiary.PK, accountDetail2.A1_EPaymentBeneficiaryId);
				AssertEquals(EPaymentMethods.EPaymentViaOFX, accountDetail2.A1_PaymentMethod);
				Assert(accountDetail2.A1_IsDefaultAccount);
				AssertEquals(EPaymentReasonCodes.OFXReasonCodes.ServicesTrade, accountDetail2.A1_EPaymentReasonCode);
				AssertEquals(beneficiary.ABF_BeneficiaryFullName, accountDetail2.A1_AccountName);
				AssertEquals(beneficiary.ABF_RX_NKAccountCurrency, accountDetail2.A1_RX_NKAccountCurrency);
				AssertEquals(beneficiary.ABF_RN_NKCountryCode, accountDetail2.A1_RN_NKCountryCode);
				AssertEquals(beneficiary.ABF_BankName, accountDetail2.A1_BankName);
				AssertEquals(beneficiary.ABF_BankBranchName, accountDetail2.A1_BankBranchName);
				AssertEquals(beneficiary.ABF_BankBsb, accountDetail2.A1_BankBsb);
				AssertEquals(beneficiary.ABF_BankAccount, accountDetail2.A1_BankAccount);
				AssertEquals(beneficiary.ABF_BankSwift, accountDetail2.A1_BankSwift);
				AssertEquals(beneficiary.ABF_BankAddress1, accountDetail2.A1_BankAddress1);
				AssertEquals(beneficiary.ABF_BankAddress2, accountDetail2.A1_BankAddress2);
				AssertEquals(beneficiary.ABF_BankAddress3, accountDetail2.A1_BankAddress3);
				AssertEquals(EPaymentReferenceTypes.FreeText, accountDetail2.A1_EPaymentReferenceType);
				AssertEquals("123456789", accountDetail2.A1_EPaymentReference);

				AssertEquals(TestObjectCreator.AUDBankAccount2.PK, TestObjectCreator.Creditor1.MiscServ.OM_AB_APDefaultBankAccount);
				AssertEquals(OrgConstants.CreditAgreedPaymentMethods.Code.CreditCard, TestObjectCreator.Creditor1.CompanyData.OB_APCreditAgreedPaymentMethod);
			}
		}

		public void TestUnmatchBeneficiaryToOrg()
		{
			var beneficiary = Factory.NewWithValidTestData<AccEPaymentBeneficiary>();
			beneficiary.ABF_BeneficiaryFullName = "Peter Walter";
			beneficiary.ABF_BeneficiaryNickName = "Walter";
			beneficiary.ABF_RX_NKAccountCurrency = "AUD";
			beneficiary.ABF_RN_NKCountryCode = "AU";
			beneficiary.ABF_GC_Company = GlbCompany.CurrentCompany.PK;

			TestObjectCreator.Creditor1.MiscServ.OM_AB_APDefaultBankAccount = TestObjectCreator.AUDBankAccount.PK;
			TestObjectCreator.Creditor1.CompanyData.OB_APCreditAgreedPaymentMethod = OrgConstants.CreditAgreedPaymentMethods.Code.BusinessCheck;

			var accountDetail1 = TestObjectCreator.Creditor1.CompanyData.AccountDetailsCollection.AddNew();
			accountDetail1.A1_EPaymentBeneficiaryId = beneficiary.PK;
			accountDetail1.A1_RX_NKAccountCurrency = "AUD";
			accountDetail1.A1_IsDefaultAccount = false;
			accountDetail1.A1_PaymentMethod = EPaymentMethods.EPaymentViaOFX;
			accountDetail1.A1_EPaymentReasonCode = EPaymentReasonCodes.OFXReasonCodes.AccountingServices;
			accountDetail1.A1_EPaymentReferenceType = EPaymentReferenceTypes.FreeText;
			accountDetail1.A1_EPaymentReference = "123456789";
			Factory.Save();

			var matchEPaymentRecipientsBulk = new MatchEPaymentRecipientsBulk(Factory);
			using (var form = new MatchEPaymentRecipientsBulkForm(matchEPaymentRecipientsBulk))
			{
				form.Show();
				Application.DoEvents();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.MatchButton_Click(this, null);
				AssertEquals("Please choose a recipient first", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.MatchRecipientsControl_ForTestOnly.FilteredRecipientsFilterControl_PerformSearch(this, null);
				form.MatchRecipientsControl_ForTestOnly.FilteredRecipientsGrid_ForTestOnly.Select(0);
				AssertEquals(TestObjectCreator.Creditor1.PK, matchEPaymentRecipientsBulk.CreditorPK);
				AssertEquals(TestObjectCreator.AUDBankAccount.PK, matchEPaymentRecipientsBulk.DefaultBankAccountPK);
				AssertEquals(OrgConstants.CreditAgreedPaymentMethods.Code.BusinessCheck, matchEPaymentRecipientsBulk.AgreedPaymentMethod);
				AssertEquals(EPaymentReasonCodes.OFXReasonCodes.AccountingServices, matchEPaymentRecipientsBulk.DefaultPaymentReason);
				AssertEquals(EPaymentReferenceTypes.FreeText, matchEPaymentRecipientsBulk.PaymentReferenceType);
				AssertEquals("123456789", matchEPaymentRecipientsBulk.PaymentReference);

				form.UnmatchButton_Click(this, null);
				AssertEquals("Payables Organization successfully unmatched", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(accountDetail1.A1_EPaymentBeneficiaryId.IsEmpty);
				AssertEquals(EPaymentReasonCodes.OFXReasonCodes.AccountingServices, accountDetail1.A1_EPaymentReasonCode);
				AssertEquals(TestObjectCreator.AUDBankAccount.PK, TestObjectCreator.Creditor1.MiscServ.OM_AB_APDefaultBankAccount);
				AssertEquals(OrgConstants.CreditAgreedPaymentMethods.Code.BusinessCheck, TestObjectCreator.Creditor1.CompanyData.OB_APCreditAgreedPaymentMethod);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.UnmatchButton_Click(this, null);
				AssertEquals("Please choose a matched recipient first", UnitTestUserNotification.Instance.LastMessage.Text);

				form.MatchRecipientsControl_ForTestOnly.FilteredRecipientsGrid_ForTestOnly.UnSelect(0);
				Assert(matchEPaymentRecipientsBulk.CreditorPK.IsEmpty);
				Assert(matchEPaymentRecipientsBulk.DefaultBankAccountPK.IsEmpty);
				Assert(matchEPaymentRecipientsBulk.AgreedPaymentMethod.IsEmpty);
				Assert(matchEPaymentRecipientsBulk.DefaultPaymentReason.IsEmpty);
				AssertEquals(ZString.Empty, matchEPaymentRecipientsBulk.PaymentReferenceType);
				AssertEquals(ZString.Empty, matchEPaymentRecipientsBulk.PaymentReference);
			}
		}

		[TestDate(2021, 8, 18)]
		public void TestSyncButtonClicked()
		{
			var matchEPaymentRecipients = new MatchEPaymentRecipientsBulk(Factory);
			using (var form = new MatchEPaymentRecipientsBulkForm(matchEPaymentRecipients))
			{
				form.Show();
				form.SyncButton_Click(this, null);
				AssertEquals("In order to request OFX Recipient List, you must have a registered OFX account. If you already have an account, please ensure that you have configured an OFX E-Payment Account in the Bank Account Maintenance module and that your staff profile is registered and authorized on this Bank Account.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
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
				token.TK_ExpiryUtc = new ZDateTime(2021, 8, 15);
				token.TK_Status = AccEPaymentStaffTokenLookups.StatusCodes.Authorised;
				Factory.Save();
				form.SyncButton_Click(this, null);
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

		class MatchEPaymentRecipientsBulkFormForTestOnly : MatchEPaymentRecipientsBulkForm
		{
			public MatchEPaymentRecipientsBulkFormForTestOnly(MatchEPaymentRecipientsBulk matchEPaymentRecipientsBulk) : base(matchEPaymentRecipientsBulk)
			{ }

			public KBindingSource BindingSource_ForTestOnly => BindingSource;
			public Control CreditorGuidFindBox_ForTestOnly => CreditorGuidFindBox;
			public Control DefaultPaymentReasonDropEdit_ForTestOnly => DefaultPaymentReasonDropEdit;
			public Control BankAccountGuidFindBox_ForTestOnly => BankAccountGuidFindBox;
			public Control DropEditAgreedPaymentMethod_ForTestOnly => DropEditAgreedPaymentMethod;
			public Control DropEditPaymentReferenceType_ForTestOnly => DropEditPaymentReferenceType;
			public Control TextPaymentReference_ForTestOnly => TextPaymentReference;
			public Control CheckBoxOverrideDefault_ForTestOnly => CheckBoxOverrideDefault;
		}
	}
}
