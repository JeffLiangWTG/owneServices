using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.BR.GUI.Testing
{
	[TestedType(typeof(CusGoodsCatalogForm))]
	class CusGoodsCatalogFormTest : ZFormBasherTest
	{
		public void TestSendActivationMessageMenuItem_CreateNewVersion()
		{
			AssertSendMessageMenuItem(GoodsCatalogStatusTypeList.Codes.Active, "Create New Version", ActionList.Codes.CreateNewVersion);
		}

		public void TestSendActivationMessageMenuItem_Activation()
		{
			AssertSendMessageMenuItem(GoodsCatalogStatusTypeList.Codes.Active, "Activate", ActionList.Codes.Activate);
		}

		public void TestSendDesactivationMessageMenuItem()
		{
			AssertSendMessageMenuItem(GoodsCatalogStatusTypeList.Codes.Active, "Deactivate", ActionList.Codes.Deactivate);
		}

		public void TestSendDraftMessageMenuItem_CreateDraft()
		{
			AssertSendMessageMenuItem(ZString.Empty, "Create Draft", ActionList.Codes.CreateDraft);
		}

		public void TestSendDraftMessageMenuItem_UpdateDraft()
		{
			AssertSendMessageMenuItem(ZString.Empty, "Update Draft", ActionList.Codes.UpdateDraft);
		}

		public void TestSendLinkUnlinkMessageMenuItem()
		{
			AssertSendMessageMenuItem("0", "Send Link/Unlink Foreign Operator", ActionList.Codes.LinkUnlinkForeignOperator);
		}

		void AssertSendMessageMenuItem(ZString authorityStatus, ZString caption, ZString action)
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_WorkPhone = "0412345678";
			staff.GS_FullName = "USERPERMIT";
			staff.GS_Code = "PRT";

			var password = BRGlbStaffWrapper.Get(staff).CCTPassword;
			password.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			password.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
			password.GP_ExpiryDate = ZDateTime.Today.AddDays(10);
			password.GP_PasswordStatus = BRPasswordStatusList.Codes.Valid;

			Factory.Save();

			var goodsCatalog = Factory.NewWithValidTestData<CusGoodsCatalog>();
			goodsCatalog.CGC_AuthorityStatus = authorityStatus;
			if (action == ActionList.Codes.LinkUnlinkForeignOperator)
			{
				goodsCatalog.CGC_AuthorityIdentifier = "1";
				goodsCatalog.CGC_CustomsStatus = "ACC";
				var productInfo = goodsCatalog.ForeignOperators.AddNew();
				productInfo.CGI_CustomsStatus = ForeignOperatorCustomsStatusTypeList.Codes.Active;
				productInfo.CountryCode = "AR";
			}

			using (var form = new CusGoodsCatalogForm(goodsCatalog))
			{
				form.Show();
				Application.DoEvents();
				var customsMenuItem = form.Menu.MenuItems.FindByText("Customs");
				AssertNotNull("There should be a Customs menu", customsMenuItem);

				var sendMessageMenuItem = customsMenuItem.MenuItems.FindByText("Send Message");
				AssertNotNull("There should be a Send Message menu", sendMessageMenuItem);

				var sendActivationMessageMenuItem = sendMessageMenuItem.MenuItems.FindByText(caption);
				AssertNotNull($"There should be {caption} menu", sendActivationMessageMenuItem);

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = System.Windows.Forms.DialogResult.OK;

				ZFormModaliser.SetDelegateToCallOnFormShown(obj =>
				{
					var form = (SingleMessageSendingForm)obj;
					var sendingMessage = form.BusinessEntity as GoodsCatalogMessageSendingObject;
					sendingMessage.BrokerCode = staff.GS_Code;
					AssertEquals("Action should be", action, sendingMessage.Action);

					var sendButton = form.FindSingle<ZButton>("SendButton");
					sendButton.PerformClick();
				});

				sendActivationMessageMenuItem.PerformClick();
				AssertType<SingleMessageSendingForm>(ZFormModaliser.LastFormShownDialogForTest);
				AssertEquals("Last message should be", "1 message(s) have been sent.", UnitTestUserNotification.Instance.LastMessage.Text);
				var message = Factory.LoadTop1<BREDIMessage>(new ZQuery(EDIMessageSchema.EM_LinkUniqueID, goodsCatalog.PK));
				AssertEquals("CAT message sent", MessageTypeList.Codes.CAT, message.EM_MessageType);

				if (action == ActionList.Codes.LinkUnlinkForeignOperator)
				{
					AssertEquals("CAT/LIN message sent", EDIMessageSubTypeList.Codes.Link, message.EM_MessageSubType);
				}
				else
				{
					AssertEquals("CAT/ORI message sent", EDIMessageSubTypeList.Codes.Original, message.EM_MessageSubType);
				}
			}
		}

		public void TestCustomsMenusVisibility()
		{
			var goodsCatalog = Factory.NewWithValidTestData<CusGoodsCatalog>();
			var foreignOperator = goodsCatalog.ForeignOperators.AddNew();

			using (var form = new CusGoodsCatalogForm(goodsCatalog))
			{
				form.Show();
				Application.DoEvents();
				var customsMenuItem = form.Menu.MenuItems.FindByText("Customs");
				customsMenuItem.ShowPopupMenu();
				var sendMessageMenuItem = customsMenuItem.MenuItems.FindByText("Send Message");
				AssertVisibleMenu(sendMessageMenuItem, false, false, true, false, true, false);

				goodsCatalog.CGC_CustomsStatus = CustomsPostedStatusList.Codes.Accepted;
				customsMenuItem.ShowPopupMenu();
				AssertVisibleMenu(sendMessageMenuItem, false, false, true, false, false, false);

				goodsCatalog.CGC_AuthorityStatus = GoodsCatalogStatusTypeList.Codes.Active;
				customsMenuItem.ShowPopupMenu();
				AssertVisibleMenu(sendMessageMenuItem, false, true, false, false, false, true);

				foreignOperator.CGI_CustomsStatus = CustomsPostedStatusList.Codes.DeletePending;
				customsMenuItem.ShowPopupMenu();
				AssertVisibleMenu(sendMessageMenuItem, false, true, false, false, false, true);

				foreignOperator.CGI_CustomsStatus = ZString.Empty;
				customsMenuItem.ShowPopupMenu();
				AssertVisibleMenu(sendMessageMenuItem, false, true, false, false, false, false);

				goodsCatalog.CGC_CustomsStatus = CustomsPostedStatusList.Codes.UpdatePending;
				customsMenuItem.ShowPopupMenu();
				AssertVisibleMenu(sendMessageMenuItem, true, true, false, false, false, false);

				goodsCatalog.CGC_AuthorityStatus = ZString.Empty;
				customsMenuItem.ShowPopupMenu();
				AssertVisibleMenu(sendMessageMenuItem, false, false, true, false, true, false);

				goodsCatalog.CGC_CustomsStatus = CustomsPostedStatusList.Codes.Active;
				customsMenuItem.ShowPopupMenu();
				AssertVisibleMenu(sendMessageMenuItem, false, false, true, false, true, false);

				goodsCatalog.CGC_CustomsStatus = ZString.Empty;
				customsMenuItem.ShowPopupMenu();
				AssertVisibleMenu(sendMessageMenuItem, false, false, true, false, false, false);

				goodsCatalog.CGC_AuthorityStatus = GoodsCatalogStatusTypeList.Codes.Inactive;
				customsMenuItem.ShowPopupMenu();
				AssertVisibleMenu(sendMessageMenuItem, false, false, false, false, false, false);

				goodsCatalog.CGC_CustomsStatus = CustomsPostedStatusList.Codes.Active;
				customsMenuItem.ShowPopupMenu();
				AssertVisibleMenu(sendMessageMenuItem, true, false, false, false, false, false);

				goodsCatalog.CGC_CustomsStatus = CustomsPostedStatusList.Codes.UpdatePending;
				customsMenuItem.ShowPopupMenu();
				AssertVisibleMenu(sendMessageMenuItem, true, false, false, false, false, false);

				goodsCatalog.CGC_AuthorityStatus = GoodsCatalogStatusTypeList.Codes.Draft;
				customsMenuItem.ShowPopupMenu();
				AssertVisibleMenu(sendMessageMenuItem, false, false, true, true, false, false);

				goodsCatalog.CGC_CustomsStatus = CustomsPostedStatusList.Codes.Active;
				customsMenuItem.ShowPopupMenu();
				AssertVisibleMenu(sendMessageMenuItem, false, false, true, true, false, false);

				goodsCatalog.CGC_CustomsStatus = ZString.Empty;
				customsMenuItem.ShowPopupMenu();
				AssertVisibleMenu(sendMessageMenuItem, false, false, true, false, false, false);
			}

			void AssertVisibleMenu(MenuItem sendMessageMenuItem, ZBool newVersion, ZBool desactivation, ZBool activation, ZBool updateDraft, ZBool createDraft, ZBool linkUnlinkForeignOperator)
			{
				var createNewVersionMessageMenuItem = sendMessageMenuItem.MenuItems.FindByText("Create New Version");
				var desactivationMessageMenuItem = sendMessageMenuItem.MenuItems.FindByText("Deactivate");
				var activationMessageMenuItem = sendMessageMenuItem.MenuItems.FindByText("Activate");
				var updateDraftMessageMenuItem = sendMessageMenuItem.MenuItems.FindByText("Update Draft");
				var createDraftMessageMenuItem = sendMessageMenuItem.MenuItems.FindByText("Create Draft");
				var linkUnlinkForeignOperatorMenuItem = sendMessageMenuItem.MenuItems.FindByText("Send Link/Unlink Foreign Operator");

				AssertEquals("createNewVersionMessageMenuItem should be visible", newVersion, createNewVersionMessageMenuItem.Visible);
				AssertEquals("desactivationMessageMenuItem should be visible", desactivation, desactivationMessageMenuItem.Visible);
				AssertEquals("activationMessageMenuItem should be visible", activation, activationMessageMenuItem.Visible);
				AssertEquals("updateDraftMessageMenuItem should be visible", updateDraft, updateDraftMessageMenuItem.Visible);
				AssertEquals("createDraftMessageMenuItem should be visible", createDraft, createDraftMessageMenuItem.Visible);
				AssertEquals("linkUnlinkForeignOperatorMenuItem should be visible", linkUnlinkForeignOperator, linkUnlinkForeignOperatorMenuItem.Visible);
			}
		}

		public void TestMenuItems()
		{
			using (var form = GetFormToBashCore())
			{
				var menuItems = form.Menu.MenuItems;
				AssertSequencesEqual(new[] { "&File", "&Edit", "Actio&ns", "Customs", "&Help" }, menuItems.Cast<MenuItem>().Select(x => x.Text));
			}
		}

		public void TestMessagesTabPage()
		{
			var goodsCatalog = Factory.NewWithValidTestData<CusGoodsCatalog>();

			using (var form = new CusGoodsCatalogForm(goodsCatalog))
			{
				var messageTabPage = form.Controls.Find("MessagesTabPage", true).FirstOrDefault() as ZTabPage;
				AssertNotNull(messageTabPage);

				var messagesTabUserControl = messageTabPage.Controls.Find("MessageTabUserControl", true).FirstOrDefault() as MessagesTabUserControl;
				AssertNotNull(messagesTabUserControl);

				AssertEquals("BindingMember should be ", nameof(CusGoodsCatalog.Messages), messagesTabUserControl.GetBindingMember());
			}
		}

		public void TestWorkflowTabPage()
		{
			var goodsCatalog = Factory.New<CusGoodsCatalog>();
			using (var form = new CusGoodsCatalogForm(goodsCatalog))
			{
				form.Show();
				var workflowTabPage = form.WorkflowTabPage;
				Assert(workflowTabPage.TabRelevant);
			}
		}

		public void TestShowPreSaveDialogs()
		{
			var expectedMessage = "There are messages waiting for responses and the system has detected you have made changes that affect Customs entries. The changes cannot be saved. Please wait until the messages are responded.";

			var goodsCatalog = Factory.NewWithValidTestData<CusGoodsCatalog>();
			goodsCatalog.CGC_Type = GoodsCatalogTypeList.Codes.Export;
			goodsCatalog.CGC_Tariff = "56049000";
			goodsCatalog.CGC_MessageStatus = BRMessageStatusList.Codes.AwaitingResponse;
			Factory.Save();

			using (var form = new CusGoodsCatalogForm(goodsCatalog))
			{
				form.Show();
				Application.DoEvents();
				goodsCatalog.CGC_Tariff = "78049001";

				form.FireSaveButton();
				AssertContains("Message pops up to stop saving", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("GoodsCatalog is not saved", goodsCatalog.HasChanges);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				goodsCatalog.CGC_Tariff = "56049000";

				form.FireSaveButton();
				Assert("No message pops up", !UnitTestUserNotification.Instance.PreviousMessages.Any(x => x.Contains(expectedMessage)));
				Assert("GoodsCatalog is saved", !goodsCatalog.HasChanges);
			}
		}

		public void TestStopSendingWhenWaitingForResponse()
		{
			var expectedMessage = "There is message waiting for response. Please wait until the messages are responded.";

			var goodsCatalog = Factory.NewWithValidTestData<CusGoodsCatalog>();
			goodsCatalog.CGC_Type = GoodsCatalogTypeList.Codes.Export;
			goodsCatalog.CGC_Tariff = "56049000";
			goodsCatalog.CGC_MessageStatus = BRMessageStatusList.Codes.AwaitingResponse;
			Factory.Save();

			using (var form = new CusGoodsCatalogForm(goodsCatalog))
			{
				form.Show();
				Application.DoEvents();

				var customsMenuItem = form.Menu.MenuItems.FindByText("Customs");
				var sendMessageMenuItem = customsMenuItem.MenuItems.FindByText("Send Message");
				var sendActivationMessageMenuItem = sendMessageMenuItem.MenuItems.FindByText("Activate");

				sendActivationMessageMenuItem.PerformClick();
				AssertContains("Message pops up to stop sending", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestStopSendingDeactivateWhenHasPendingChanges()
		{
			var expectedMessage = "This Catalog cannot be deactivated because there are pending changes to be sent to Customs.";

			var goodsCatalog = Factory.NewWithValidTestData<CusGoodsCatalog>();
			goodsCatalog.CGC_Type = GoodsCatalogTypeList.Codes.Export;
			goodsCatalog.CGC_Tariff = "56049000";
			goodsCatalog.CGC_CustomsStatus = CustomsPostedStatusList.Codes.UpdatePending;
			Factory.Save();

			using (var form = new CusGoodsCatalogForm(goodsCatalog))
			{
				form.Show();
				Application.DoEvents();

				var customsMenuItem = form.Menu.MenuItems.FindByText("Customs");
				var sendMessageMenuItem = customsMenuItem.MenuItems.FindByText("Send Message");
				var sendActivationMessageMenuItem = sendMessageMenuItem.MenuItems.FindByText("Deactivate");

				sendActivationMessageMenuItem.PerformClick();
				AssertContains("Message pops up to stop sending", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				goodsCatalog.CGC_CustomsStatus = CustomsPostedStatusList.Codes.Accepted;

				sendActivationMessageMenuItem.PerformClick();
				Assert("No message pops up", !UnitTestUserNotification.Instance.PreviousMessages.Any(x => x.Contains(expectedMessage)));

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var productInfo = goodsCatalog.ForeignOperators.AddNew();
				productInfo.CGI_Reference = "AR";
				productInfo.CGI_CustomsStatus = CustomsPostedStatusList.Codes.Accepted;

				sendActivationMessageMenuItem.PerformClick();
				AssertContains("Message pops up to stop sending", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				productInfo.CGI_CustomsStatus = CustomsPostedStatusList.Codes.DeletePending;

				sendActivationMessageMenuItem.PerformClick();
				AssertContains("Message pops up to stop sending", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				productInfo.CGI_CustomsStatus = CustomsPostedStatusList.Codes.UpdatePending;

				sendActivationMessageMenuItem.PerformClick();
				Assert("No message pops up", !UnitTestUserNotification.Instance.PreviousMessages.Any(x => x.Contains(expectedMessage)));
			}
		}

		public void TestProcessWhenChangesAreSavedWithoutSending()
		{
			var goodsCatalog = Factory.NewWithValidTestData<CusGoodsCatalog>();
			goodsCatalog.CGC_Type = GoodsCatalogTypeList.Codes.Export;
			goodsCatalog.CGC_MessageStatus = BRMessageStatusList.Codes.Accepted;
			goodsCatalog.CGC_AuthorityIdentifier = "1";
			goodsCatalog.CGC_Tariff = "78049001";
			Factory.Save();

			using (var form = new CusGoodsCatalogForm(goodsCatalog))
			{
				form.Show();
				Application.DoEvents();
				goodsCatalog.CGC_Tariff = "78049002";

				form.FireSaveButton();

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = System.Windows.Forms.DialogResult.OK;

				AssertType<CatalogBackdoorForSavingOnAmendmentForm>(ZFormModaliser.LastFormShownDialogForTest);

				AssertEquals("Message Status reset", BRMessageStatusList.Codes.NotSent, goodsCatalog.CGC_MessageStatus);
				AssertEquals("no EdiMessages created", 0, goodsCatalog.Messages.Count);
			}
		}

		public void TestRevertChanges()
		{
			var expectedMessage = "This catalog does not meet the criteria to be reverted (it does not have an Authority Identifier, and its Message Status is different from NOT - Not Sent).";

			var goodsCatalog = Factory.NewWithValidTestData<CusGoodsCatalog>();
			goodsCatalog.CGC_Type = GoodsCatalogTypeList.Codes.Export;
			goodsCatalog.CGC_AuthorityIdentifier = "1";
			goodsCatalog.CGC_Tariff = "78049001";
			Factory.Save();

			using (var form = new CusGoodsCatalogForm(goodsCatalog))
			{
				form.Show();
				Application.DoEvents();

				var customsMenuItem = form.Menu.MenuItems.FindByText("Customs");
				var revertChangesMenuItem = customsMenuItem.MenuItems.FindByText("Revert Changes");
				AssertNotNull("There should be a Revert Changes menu", revertChangesMenuItem);

				revertChangesMenuItem.PerformClick();
				Assert("No message pops up to stop revert changes", UnitTestUserNotification.Instance.LastMessage.Text.IsNullOrEmpty());

				goodsCatalog.CGC_AuthorityIdentifier = "";
				goodsCatalog.CGC_MessageStatus = BRMessageStatusList.Codes.NotSent;
				form.FireSaveButton();

				revertChangesMenuItem.PerformClick();
				Assert("No Message pops up to stop revert changes", UnitTestUserNotification.Instance.LastMessage.Text.IsNullOrEmpty());

				goodsCatalog.CGC_MessageStatus = BRMessageStatusList.Codes.AwaitingResponse;
				form.FireSaveButton();

				revertChangesMenuItem.PerformClick();
				AssertEquals("Message pops up to stop revert changes", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var goodsCatalog = Factory.New<CusGoodsCatalog>();
			var form = new CusGoodsCatalogForm(goodsCatalog);
			form.ControllerID = ControllerIDs.Customs.GoodsCatalog;
			return form;
		}
	}
}
