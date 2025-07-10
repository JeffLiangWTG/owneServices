using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.GUI;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Messaging.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using GlbCompanyWrapper = Enterprise.Customs.IE.Business.GlbCompanyWrapper;

namespace Enterprise.Customs.IE.NCTS.GUI.Testing
{
	class GuaranteeMessagingMenuProviderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new GuaranteeMessagingMenuProvider(null));
		}

		const string GuaranteeVoucherSoldMenuItemText = "Guarantee Voucher Sold";
		const string GuaranteeAccessCodesMenuItemText = "Update Access Code";

		public void TestCreateMenuItems()
		{
			AssertContainsExactElementsInExactOrder(new[] { GuaranteeVoucherSoldMenuItemText, GuaranteeAccessCodesMenuItemText }, menuItems.Cast<MenuItem>().Select(x => x.Text));
		}

		public void TestRefreshMenu()
		{
			provider.RefreshMenu();
			AssertContainsExactElementsInExactOrder(new[] { GuaranteeVoucherSoldMenuItemText, GuaranteeAccessCodesMenuItemText }, menuItems.Cast<MenuItem>().Select(x => x.Text));
		}

		#region Guarantee Voucher Sold

		public void TestGuaranteeVoucherSold_PreSaveInvoked()
		{
			CheckPreSaveInvoked(GuaranteeVoucherSoldMenuItemText);
		}

		public void TestGuaranteeVoucherSold_CredentialCheck_NoValidCredential()
		{
			CredentialCheck_NoValidCredential(GuaranteeVoucherSoldMenuItemText);
		}

		public void TestGuaranteeVoucherSold_CredentialCheck_ExpiredCredential()
		{
			CredentialCheck_ExpiredCredential(GuaranteeVoucherSoldMenuItemText);
		}

		public void TestGuaranteeVoucherSold_CredentialCheck_NoCredential()
		{
			CredentialCheck_NoCredential(GuaranteeVoucherSoldMenuItemText);
		}

		#endregion

		#region Guarantee Access Codes

		public void TestGuaranteeAccessCodes_PreSaveInvoked()
		{
			CheckPreSaveInvoked(GuaranteeAccessCodesMenuItemText);
		}

		public void TestGuaranteeAccessCodes_CredentialCheck_NoValidCredential()
		{
			CredentialCheck_NoValidCredential(GuaranteeAccessCodesMenuItemText);
		}

		public void TestGuaranteeAccessCodes_CredentialCheck_ExpiredCredential()
		{
			CredentialCheck_ExpiredCredential(GuaranteeAccessCodesMenuItemText);
		}

		public void TestGuaranteeAccessCodes_CredentialCheck_NoCredential()
		{
			CredentialCheck_NoCredential(GuaranteeAccessCodesMenuItemText);
		}

		public void TestGuaranteeAccessCodes_ShowSendingForm()
		{
			using (var cusGuaranteeForm = GetForm(true))
			{
				var sendMenuItem = cusGuaranteeForm.FindMenuItem_ForTest(GuaranteeAccessCodesMenuItemText);
				sendMenuItem.PerformClick();
				AssertType<GuaranteeAccessCodesSendingForm>("GuaranteeAccessCodesSendingForm should be shown", ZFormModaliser.LastFormShownDialogForTest);
			}
		}

		#endregion

		void CheckPreSaveInvoked(string menuItemCaption)
		{
			const string messageSaveConfirmation = "The Job has not yet been saved. Do you want to save and proceed?";
			using (var cusGuaranteeForm = GetForm(true))
			{
				var sendMenuItem = cusGuaranteeForm.FindMenuItem_ForTest(menuItemCaption);
				CombineAssertions(() =>
				{
					AssertEquals("Pre-condition not saved", true, header.HasChanges);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					sendMenuItem.PerformClick();
					AssertEquals("Message for deny save", messageSaveConfirmation, UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("Save denied", true, header.HasChanges);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					sendMenuItem.PerformClick();
					AssertEquals("Message for confirm save", messageSaveConfirmation, UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("Save confirmed", false, header.HasChanges);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					sendMenuItem.PerformClick();
					AssertNotEquals("No message when no changes", messageSaveConfirmation, UnitTestUserNotification.Instance.LastMessage.Text);
				});
			}
		}

		void CredentialCheck_NoValidCredential(string menuItemCaption)
		{
			using (var cusGuaranteeForm = GetForm(false))
			{
				var company = GlbCompany.CurrentCompany;
				var companyCredential = GlbCompanyWrapper.Get(company).GlbExternalPassword;
				companyCredential.CurrentDecryptedCertificatePassphrase = "HELLO";
				Factory.Save();
				var menuItem = (ZMenuItem)cusGuaranteeForm.FindMenuItem_ForTest(menuItemCaption);

				CombineAssertions("No valid credential", () =>
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					menuItem.PerformClick();
					var lastMessage = UnitTestUserNotification.Instance.LastMessage;
					AssertEquals("Caption", "Send to Customs Error", lastMessage.Caption);
					AssertEquals("Text", $"Cannot send message as Company ({company.GC_Code}) doesn't have a Revenue ROS certificate uploaded or credential added. Please see eLearning 'Set up CargoWise for Customs Messaging in Ireland' on My Account for assistance.", lastMessage.Text);
					AssertEquals("WasError", true, lastMessage.WasError);
				});
			}
		}

		void CredentialCheck_ExpiredCredential(string menuItemCaption)
		{
			using (var cusGuaranteeForm = GetForm(false))
			{
				var company = GlbCompany.CurrentCompany;
				var companyCredential = InterchangeProcessorTestHelper.CreateValidCredential(company);
				companyCredential.GP_ExpiryDate = ZDateTime.UtcNow.AddDays(-1);
				Factory.Save();
				AssertEquals("Pre-condition", PasswordStatusList.Codes.Valid, companyCredential.GP_PasswordStatus);
				var menuItem = (ZMenuItem)cusGuaranteeForm.FindMenuItem_ForTest(menuItemCaption);

				CombineAssertions("No valid credential", () =>
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					menuItem.PerformClick();
					var lastMessage = UnitTestUserNotification.Instance.LastMessage;
					AssertEquals("Caption", "Send to Customs Error", lastMessage.Caption);
					AssertEquals("Text", $"Cannot send message as Company ({company.GC_Code}) doesn't have a Revenue ROS certificate uploaded or credential added. Please see eLearning 'Set up CargoWise for Customs Messaging in Ireland' on My Account for assistance.", lastMessage.Text);
					AssertEquals("WasError", true, lastMessage.WasError);
				});
			}
		}

		void CredentialCheck_NoCredential(string menuItemCaption)
		{
			using (var cusGuaranteeForm = GetForm(false))
			{
				var company = GlbCompany.CurrentCompany;
				var menuItem = (ZMenuItem)cusGuaranteeForm.FindMenuItem_ForTest(menuItemCaption);

				CombineAssertions("No valid credential", () =>
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					menuItem.PerformClick();
					var lastMessage = UnitTestUserNotification.Instance.LastMessage;
					AssertEquals("Caption", "Send to Customs Error", lastMessage.Caption);
					AssertEquals("Text", $"Cannot send message as Company ({company.GC_Code}) doesn't have a Revenue ROS certificate uploaded or credential added. Please see eLearning 'Set up CargoWise for Customs Messaging in Ireland' on My Account for assistance.", lastMessage.Text);
					AssertEquals("WasError", true, lastMessage.WasError);
				});
			}
		}

		GuaranteeForm GetForm(bool setupCredential)
		{
			var form = new GuaranteeForm(header);
			if (setupCredential)
			{
				InterchangeProcessorTestHelper.CreateValidCredential(Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK));
			}
			return form;
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			provider = new GuaranteeMessagingMenuProvider(header);
			menuItems = provider.CreateMenuItems();
		}

		CusGuaranteeHeader header;
		GuaranteeMessagingMenuProvider provider;
		IEnumerable<ZMenuItem> menuItems;
	}
}
