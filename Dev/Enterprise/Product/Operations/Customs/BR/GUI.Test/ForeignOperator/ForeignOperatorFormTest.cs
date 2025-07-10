using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.BR.GUI.Testing
{
	[TestedType(typeof(ForeignOperatorForm))]
	class ForeignOperatorFormTest : ZFormBasherTest
	{
		public void TestSendMessageMenuItem_CreateNewVersion()
		{
			AssertSendMessageMenuItem("1", "1", "Create New Version", ActionList.Codes.CreateNewVersion);
		}

		public void TestSendMessageMenuItem_Activate()
		{
			AssertSendMessageMenuItem("1", "1", "Activate", ActionList.Codes.Activate);
		}

		public void TestSendMessageMenuItem_Deactivate()
		{
			AssertSendMessageMenuItem("1", "1", "Deactivate", ActionList.Codes.Deactivate);
		}

		void AssertSendMessageMenuItem(ZString authorityIdentifier, ZString authorityVersion, ZString caption, ZString action)
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

			var owner = Factory.NewWithValidTestData<OrgHeader>();
			var manufacturer = Factory.NewWithValidTestData<OrgHeader>();
			var foreignOperator = Factory.NewWithValidTestData<CusBRForeignOperator>();
			foreignOperator.BFR_OH_Owner = owner.PK;
			foreignOperator.BFR_OH_ForeignOperator = manufacturer.PK;
			foreignOperator.BFR_AuthorityIdentifier = authorityIdentifier;
			foreignOperator.BFR_AuthorityVersion = authorityVersion;
			foreignOperator.BFR_MessageStatus = BRMessageStatusList.Codes.Accepted;
			Factory.Save();
			using (var form = new ForeignOperatorForm(foreignOperator))
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
					var sendingMessage = form.BusinessEntity as ForeignOperatorMessageSendingObject;
					sendingMessage.BrokerCode = staff.GS_Code;
					AssertEquals("Action should be", action, sendingMessage.Action);

					var sendButton = form.FindSingle<ZButton>("SendButton");
					sendButton.PerformClick();
				});

				sendActivationMessageMenuItem.PerformClick();
				AssertType<SingleMessageSendingForm>(ZFormModaliser.LastFormShownDialogForTest);
				AssertEquals("Last message should be", "1 message(s) have been sent.", UnitTestUserNotification.Instance.LastMessage.Text);
				var message = Factory.LoadTop1<BREDIMessage>(new ZQuery(EDIMessageSchema.EM_LinkUniqueID, foreignOperator.PK));
				AssertEquals("OPE message sent", MessageTypeList.Codes.OPE, message.EM_MessageType);
				AssertEquals("OPE/ORI message sent", EDIMessageSubTypeList.Codes.Original, message.EM_MessageSubType);

				var log = foreignOperator.Logs.MostRecentLogByEventTime(Events.MessageSent);
				AssertEquals("A CES Log should be added", Events.MessageSent.Code, log.SL_SE_NKEvent);
				AssertEquals("CES Log Reference", action, log.SL_Reference);
				AssertEquals("BFR_MessageStatus should be update to AWA", BRMessageStatusList.Codes.AwaitingResponse, foreignOperator.BFR_MessageStatus);
			}
		}

		public void TestCustomsMenusVisibility()
		{
			var cusBRForeignOperator = Factory.NewWithValidTestData<CusBRForeignOperator>();
			cusBRForeignOperator.BFR_AuthorityIdentifier = ZString.Empty;
			cusBRForeignOperator.BFR_AuthorityVersion = ZString.Empty;
			using (var form = new ForeignOperatorForm(cusBRForeignOperator))
			{
				form.Show();
				Application.DoEvents();
				var customsMenuItem = form.Menu.MenuItems.FindByText("Customs");
				customsMenuItem.ShowPopupMenu();
				var sendMessageMenuItem = customsMenuItem.MenuItems.FindByText("Send Message");
				AssertVisibleMenu(sendMessageMenuItem, newVersion: false, desactivation: false, activation: true);

				cusBRForeignOperator.BFR_CustomsStatus = ForeignOperatorCustomsStatusTypeList.Codes.Active;
				customsMenuItem.ShowPopupMenu();
				AssertVisibleMenu(sendMessageMenuItem, newVersion: false, desactivation: false, activation: true);

				cusBRForeignOperator.BFR_CustomsStatus = ForeignOperatorCustomsStatusTypeList.Codes.Active;
				cusBRForeignOperator.BFR_AuthorityIdentifier = "1";
				cusBRForeignOperator.BFR_AuthorityVersion = "1";
				customsMenuItem.ShowPopupMenu();
				AssertVisibleMenu(sendMessageMenuItem, newVersion: true, desactivation: true, activation: false);

				cusBRForeignOperator.BFR_CustomsStatus = ForeignOperatorCustomsStatusTypeList.Codes.Inactive;
				customsMenuItem.ShowPopupMenu();
				AssertVisibleMenu(sendMessageMenuItem, newVersion: false, desactivation: false, activation: true);
			}

			void AssertVisibleMenu(MenuItem sendMessageMenuItem, ZBool newVersion, ZBool desactivation, ZBool activation)
			{
				var createNewVersionMessageMenuItem = sendMessageMenuItem.MenuItems.FindByText("Create New Version");
				var desactivationMessageMenuItem = sendMessageMenuItem.MenuItems.FindByText("Deactivate");
				var activationMessageMenuItem = sendMessageMenuItem.MenuItems.FindByText("Activate");

				AssertEquals("createNewVersionMessageMenuItem should be visible", newVersion, createNewVersionMessageMenuItem.Visible);
				AssertEquals("desactivationMessageMenuItem should be visible", desactivation, desactivationMessageMenuItem.Visible);
				AssertEquals("activationMessageMenuItem should be visible", activation, activationMessageMenuItem.Visible);
			}
		}

		public void TestGetUserControl()
		{
			using (var form = GetFormToBashCore() as ForeignOperatorForm)
			{
				form.Show();
				var tabControl = form.Controls.Find("MainTabControl", true).FirstOrDefault() as ZTemplateTabControl;
				var userControl = tabControl.TabPages[0].Controls[0];
				AssertType<ForeignOperatorUserControl>(userControl);
				AssertEquals(DockStyle.Fill, userControl.Dock);
			}
		}

		public void TestFormCaption()
		{
			var @operator = Factory.NewWithValidTestData<OrgHeader>();
			@operator.OH_Code = "TSTOPE";
			@operator.OH_FullName = "Test Operator";
			var foreignOperator = Factory.New<CusBRForeignOperator>();
			foreignOperator.BFR_OH_ForeignOperator = @operator.PK;
			using (var form = new ForeignOperatorForm(foreignOperator))
			{
				AssertEquals("Foreign Operator - TSTOPE", form.FormCaption);
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
			var foreignOperator = Factory.NewWithValidTestData<CusBRForeignOperator>();

			using (var form = new ForeignOperatorForm(foreignOperator))
			{
				var messageTabPage = form.Controls.Find("MessagesTabPage", true).FirstOrDefault() as ZTabPage;
				AssertNotNull(messageTabPage);

				var messagesTabUserControl = messageTabPage.Controls.Find("MessageTabUserControl", true).FirstOrDefault() as MessagesTabUserControl;
				AssertNotNull(messagesTabUserControl);

				AssertEquals("BindingMember should be ", nameof(CusBRForeignOperator.Messages), messagesTabUserControl.GetBindingMember());
			}
		}

		public void TestWorkflowTabPage()
		{
			using (var form = GetFormToBashCore() as ForeignOperatorForm)
			{
				form.Show();
				Assert("Workflow TabPage should be shown", form.WorkflowTabPage.TabRelevant);
			}
		}

		public void TestStopSendingWhenWaitingForResponse()
		{
			var expectedMessage = "There is message waiting for response. Please wait until the messages are responded.";

			var foreignOperator = Factory.NewWithValidTestData<CusBRForeignOperator>();
			foreignOperator.BFR_MessageStatus = BRMessageStatusList.Codes.AwaitingResponse;
			foreignOperator.BFR_AuthorityIdentifier = "1";
			foreignOperator.BFR_AuthorityVersion = "1";
			Factory.Save();

			using (var form = new ForeignOperatorForm(foreignOperator))
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

		public void TestShowPreSaveDialogs()
		{
			var expectedMessage = "This Foreign Operator cannot be edited because a message is awaiting a response (Message Status: Awaiting Response).";

			var owner = Factory.NewWithValidTestData<OrgHeader>();
			owner.PrimaryRegistrationNumber.Number = "75.400.331/0001-15";
			var foreignOperator = Factory.NewWithValidTestData<CusBRForeignOperator>();
			foreignOperator.BFR_OH_Owner = owner.PK;
			foreignOperator.BFR_OH_ForeignOperator = Factory.NewWithValidTestData<OrgHeader>().PK;
			foreignOperator.BFR_AuthorityIdentifier = "1";
			foreignOperator.BFR_AuthorityVersion = "1";
			foreignOperator.BFR_MessageStatus = BRMessageStatusList.Codes.AwaitingResponse;
			Factory.Save();

			using (var form = new ForeignOperatorForm(foreignOperator))
			{
				foreignOperator.BFR_OH_Owner = Factory.NewWithValidTestData<OrgHeader>().PK;
				form.FireSaveButton();
				Assert("Message pops up to stop saving", UnitTestUserNotification.Instance.PreviousMessages.Any(x => x.Contains(expectedMessage)));
				Assert("Foreign Operator is not saved", foreignOperator.HasChanges);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				foreignOperator.BFR_OH_Owner = owner.PK;
				form.FireSaveButton();
				Assert("No message pops up", !UnitTestUserNotification.Instance.PreviousMessages.Any(x => x.Contains(expectedMessage)));
				Assert("Foreign Operator is saved", !foreignOperator.HasChanges);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var form = new ForeignOperatorForm(Factory.New<CusBRForeignOperator>());
			form.ControllerID = ControllerIDs.Customs.BR.ForeignOperator;
			return form;
		}
	}
}
