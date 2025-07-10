using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageSending;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI.Testing
{
	public class EcsMessageMenuProviderTest : EU.GUI.PlugIn.Testing.EcsMessageMenuProviderTest
	{
		protected override Type ExpectEcsMessageMenuProviderType => typeof(EcsMessageMenuProvider);

		public override void TestCreateMenuItems()
		{
			var menuItems = EcsMessageMenuProvider.New(exitHeader).CreateMenuItems();

			CombineAssertions(() =>
			{
				AssertEquals("CreateMenuItems", 5, menuItems.Count());
				Assert(menuItems.Any(c => c.Text == "Arrive at Exit Location"));
				AssertEquals("Menu Download EAL Clearance Document exists", true, menuItems.Any(c => c.Text == "Download EAL Clearance Document"));
				Assert(!menuItems.Any(c => c.Text == "Depart from Exit Location"));
				Assert(!menuItems.Any(c => c.Text == "Capture MRNs"));
				Assert(menuItems.Any(c => c.Text == "-"));
				Assert(menuItems.Any(c => c.Text == "Lock Declaration"));
				Assert(menuItems.Any(c => c.Text == "Unlock Declaration"));
			});
		}

		public void TestLockOrUnlockCustomsFileMenuItems_Visible()
		{
			var collection = new DeclarationLockConfigCollection(null, Factory);

			var config = collection.AddNew();
			config.DeclarationType = "FRM";

			var tabInfo = config.TabInfos.AddNew();
			tabInfo.TabPage = Core.Constants.Customs.DeclarationTabPages.Codes.Declaration;

			LogEventAndAssertGivenMenuItemVisible(AutoEvents.LockForEdit, "Lock Declaration", collection, false);
			LogEventAndAssertGivenMenuItemVisible(AutoEvents.UnlockForEdit, "Unlock Declaration", collection, false);

			LogEventAndAssertGivenMenuItemVisible(AutoEvents.UnlockForEdit, "Lock Declaration", collection, true);
			LogEventAndAssertGivenMenuItemVisible(AutoEvents.LockForEdit, "Unlock Declaration", collection, true);

			config.DeclarationType = "EXP";

			LogEventAndAssertGivenMenuItemVisible(AutoEvents.UnlockForEdit, "Lock Declaration", collection, false);
			LogEventAndAssertGivenMenuItemVisible(AutoEvents.LockForEdit, "Unlock Declaration", collection, false);
		}

		void LogEventAndAssertGivenMenuItemVisible(Event eventType, string menuName, DeclarationLockConfigCollection configures, bool isVisible)
		{
			var declaration = Factory.New<JobDeclaration>();

			var exitHeader = Factory.NewWithValidTestData<CusExitControlHeader>();
			exitHeader.CEH_ParentTableCode = "JE";
			exitHeader.CEH_ParentID = declaration.PK;
			((ICustomsFileParent)exitHeader).DeclarationTypeInfo.SetValueFromString("FRM");

			var log = exitHeader.Logs.AddNew(eventType, ZDateTimeOffset.Now);
			log.IsCancelled = false;

			exitHeader.Factory.Save();

			var originalValue = Env.Security.LockOrUnlockFileForEdit.IsAllowed;
			var securityAction = new DisposableAction(() => { Env.Security.LockOrUnlockFileForEdit.IsAllowed = true; }, () => { Env.Security.LockOrUnlockFileForEdit.IsAllowed = originalValue; });

			using (securityAction)
			using (CustomsDataRegistry.Instance.DeclarationLockForEdit.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, configures))
			{
				menuProvider = new SendMenuForTest(exitHeader);
				var menuItem = menuProvider.CreateMenuItems().ToArray().FindByText(menuName);
				AssertEquals(isVisible, menuItem != null && menuItem.Visible);
			}
		}

		public void TestLockOrUnlockCustomsFileMenuItems_Click()
		{
			var collection = new DeclarationLockConfigCollection(null, Factory);
			var config = collection.AddNew();
			config.DeclarationType = "FRM";
			var tabInfo = config.TabInfos.AddNew();
			tabInfo.TabPage = Core.Constants.Customs.DeclarationTabPages.Codes.Declaration;
			using (CustomsDataRegistry.Instance.DeclarationLockForEdit.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			{
				var declaration = Factory.New<JobDeclaration>();
				exitHeader = Factory.NewWithValidTestData<CusExitControlHeader>();
				exitHeader.CEH_ParentTableCode = "JE";
				exitHeader.CEH_ParentID = declaration.PK;
				var exitDetail = exitHeader.CusExitDetails.AddNew();
				exitDetail.CED_Status = MessageProcessorConstants.EntryStatusCodes.Cleared;
				exitDetail.ReadOnly = false;
				((ICustomsFileParent)exitHeader).DeclarationTypeInfo.SetValueFromString("FRM");
				menuProvider = new SendMenuForTest(exitHeader);
				var menuItems = menuProvider.CreateMenuItems().ToArray();
				exitHeader.Logs.RemoveAndDeleteAll();
				Factory.Save();
				var lockCustomsFileMenuItem = menuItems.FindByText("Lock Declaration");
				var unlockCustomsFileMenuItem = menuItems.FindByText("Unlock Declaration");
				lockCustomsFileMenuItem.PerformClick();
				var log = exitHeader.Logs.Find(c => c.SL_SE_NKEvent == AutoEvents.LockForEditCode).First();
				Assert("Should contains the active LCK event.", !log.IsCancelled);
				Factory.Save();
				unlockCustomsFileMenuItem.PerformClick();
				log = exitHeader.Logs.Find(c => c.SL_SE_NKEvent == AutoEvents.UnlockForEditCode).First();
				Assert("Should contains the actived UCK event.", !log.IsCancelled);
			}
		}

		public override void TestCreateArrivalMessages()
		{
			exitHeader.CusExitDetails.AddNew();

			var arrMenuItem = menuItems.FindByText("Arrive at Exit Location");

			CombineAssertions(() =>
			{
				arrMenuItem.PerformClick();
				AssertEquals("Message informing broker and certificate are needed", "Cannot send message without a broker and valid certificate; please enter the broker and a valid certificate.", UnitTestUserNotification.Instance.LastMessage.Text);

				exitHeader.CEH_GS_NKCustomsAgent = Staff.GS_Code;
				exitHeader.CEH_CustomsProfile = ZString.Empty;
				exitHeader.Factory.Save();
				arrMenuItem.PerformClick();
				AssertEquals("Message informing broker and certificate are needed when broker is declared but certificate is empty", "Cannot send message without a broker and valid certificate; please enter the broker and a valid certificate.", UnitTestUserNotification.Instance.LastMessage.Text);

				exitHeader.CEH_CustomsProfile = "INVALID";
				exitHeader.Factory.Save();
				arrMenuItem.PerformClick();
				AssertEquals("Message informing broker and certificate are needed when broker is declared but certificate declared is invalid", "Cannot send message without a broker and valid certificate; please enter the broker and a valid certificate.", UnitTestUserNotification.Instance.LastMessage.Text);

				exitHeader.CEH_CustomsProfile = BuilderHelperTest.CertificateName;
				exitHeader.Factory.Save();
				arrMenuItem.PerformClick();
				AssertEquals("Message informing broker and certificate are needed when broker is declared but current user has no authorisation for certificate declared", "Cannot send message without a broker and valid certificate; please enter the broker and a valid certificate.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNull("User Notification Last Message was cleared", UnitTestUserNotification.Instance.LastMessage.Text);
				exitHeader.Reload();
				arrMenuItem.PerformClick();
				AssertEquals("Message informing broker and certificate are needed when broker is declared but current user has no authorisation for certificate declared, even if we haven't done a new save& validation", "Cannot send message without a broker and valid certificate; please enter the broker and a valid certificate.", UnitTestUserNotification.Instance.LastMessage.Text);

				using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
				{
					exitHeader.Reload();
					TestHelper.CheckFactoryHasNoPendingChanges("Before updating CSV", exitHeader.Factory);
					arrMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After updating CSV", exitHeader.Factory);
					AssertEquals("Should have message asking if the user wants to edit the message", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("Edit message mode is enabled in Registry/Customs/Country or Region Specific/Spain/Allow Edit EDI Message Body.\r\n \r\nDo you want to edit the EDI Messages generated by this operation?"));
					AssertContains("Messages sent correctly", "2 Messages sent successfully.", UnitTestUserNotification.Instance.LastMessage.Text);

					exitHeader.CusExitDetails.RemoveAndDeleteAll();

					arrMenuItem.PerformClick();
					AssertEquals("No details exist – Please add details before attempting to send a message.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			});
		}

		public void TestCreateArrivalMessages_OnlyOne()
		{
			using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				var arrMenuItem = menuItems.FindByText("Arrive at Exit Location");

				CombineAssertions(() =>
				{
					exitHeader.CEH_GS_NKCustomsAgent = Staff.GS_Code;
					exitHeader.CEH_CustomsProfile = BuilderHelperTest.CertificateName;
					exitHeader.Factory.Save();
					TestHelper.CheckFactoryHasNoPendingChanges("Before updating CSV", exitHeader.Factory);
					arrMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After updating CSV", exitHeader.Factory);
					AssertEquals("Should have message asking if the user wants to edit the message", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("Edit message mode is enabled in Registry/Customs/Country or Region Specific/Spain/Allow Edit EDI Message Body.\r\n \r\nDo you want to edit the EDI Messages generated by this operation?"));
					AssertContains("Messages sent correctly", "1 Message sent successfully.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertNotContains("New message's text has not been edited", "AAAAAAAAAAAAA", exitDetail.Messages.LastOutgoingMessage.EM_MessageText);
				});
			}
		}

		public void TestCreateArrivalMessages_EditMessageText()
		{
			using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			using (RegistryTemporarySetterHelper.SetAllowEditEDIMessageBody(true))
			{
				var menuProvider = new SendMenuForTest(exitHeader);
				var menuItems = menuProvider.CreateMenuItems().ToArray();

				ZFormModaliser.ShowDialogsInTest = false;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				var arrMenuItem = menuItems.FindByText("Arrive at Exit Location");

				CombineAssertions(() =>
				{
					exitHeader.CEH_GS_NKCustomsAgent = Staff.GS_Code;
					exitHeader.CEH_CustomsProfile = BuilderHelperTest.CertificateName;
					exitHeader.Factory.Save();
					TestHelper.CheckFactoryHasNoPendingChanges("Before updating CSV", exitHeader.Factory);
					arrMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After updating CSV", exitHeader.Factory);
					AssertEquals("Should have message asking if the user wants to edit the message", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("Edit message mode is enabled in Registry/Customs/Country or Region Specific/Spain/Allow Edit EDI Message Body.\r\n \r\nDo you want to edit the EDI Messages generated by this operation?"));
					AssertContains("Messages sent correctly", "1 Message sent successfully.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertContains("New message's text has been edited", "AAAAAAAAAAAAA", exitDetail.Messages.LastOutgoingMessage.EM_MessageText);
				});
			}
		}

		public void TestDownloadEAL_NoDocumentNeeded()
		{
			using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				var docManagerInfo = ((IDocManagerSupport)exitDetail).DocManagerInfo;
				docManagerInfo.AddFileOrDocument(new byte[1], MRNCode + "_E_AEAT_EAL_CLR.pdf", "CLR");
				docManagerInfo.Save();

				var downloadDocMenuItem = menuItems.FindByText("Download EAL Clearance Document");

				exitDetail.ZG_CSVClearance = "ABCDEFGHIJKLMNOP";
				exitHeader.CEH_GS_NKCustomsAgent = Staff.GS_Code;
				exitHeader.CEH_CustomsProfile = BuilderHelperTest.CertificateName;
				exitHeader.Factory.Save();
				downloadDocMenuItem.PerformClick();
				AssertContains("All Documents for AEAT already exist for the exit detail so nothing will be sent", "0 Document Capture request(s) created", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestDownloadEAL_AllDocumentsNeeded()
		{
			var downloadDocMenuItem = menuItems.FindByText("Download EAL Clearance Document");

			CombineAssertions(() =>
			{
				downloadDocMenuItem.PerformClick();
				AssertEquals("Message informing broker and certificate are needed", "Cannot send message without a broker and valid certificate; please enter the broker and a valid certificate.", UnitTestUserNotification.Instance.LastMessage.Text);

				exitHeader.CEH_GS_NKCustomsAgent = Staff.GS_Code;
				exitHeader.CEH_CustomsProfile = ZString.Empty;
				exitHeader.Factory.Save();
				downloadDocMenuItem.PerformClick();
				AssertEquals("Message informing broker and certificate are needed when broker is declared but certificate is empty", "Cannot send message without a broker and valid certificate; please enter the broker and a valid certificate.", UnitTestUserNotification.Instance.LastMessage.Text);

				exitHeader.CEH_CustomsProfile = "INVALID";
				exitHeader.Factory.Save();
				downloadDocMenuItem.PerformClick();
				AssertEquals("Message informing broker and certificate are needed when broker is declared but certificate declared is invalid", "Cannot send message without a broker and valid certificate; please enter the broker and a valid certificate.", UnitTestUserNotification.Instance.LastMessage.Text);

				exitHeader.CEH_CustomsProfile = BuilderHelperTest.CertificateName;
				exitHeader.Factory.Save();
				downloadDocMenuItem.PerformClick();
				AssertEquals("Message informing broker and certificate are needed when broker is declared but current user has no authorisation for certificate declared", "Cannot send message without a broker and valid certificate; please enter the broker and a valid certificate.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNull("User Notification Last Message was cleared", UnitTestUserNotification.Instance.LastMessage.Text);
				exitHeader.Reload();
				downloadDocMenuItem.PerformClick();
				AssertEquals("Message informing broker and certificate are needed when broker is declared but current user has no authorisation for certificate declared, even if we haven't done a new save& validation", "Cannot send message without a broker and valid certificate; please enter the broker and a valid certificate.", UnitTestUserNotification.Instance.LastMessage.Text);

				using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
				{
					exitHeader.CEH_CustomsProfile = BuilderHelperTest.CertificateName;
					exitHeader.Factory.Save();
					exitDetail.CED_MovementReferenceNumber = ZString.Empty;
					downloadDocMenuItem.PerformClick();
					AssertContains("EAL Document Capture Request can not be sent when there is no MRN so nothing will be sent and no message will be shown", ZString.Empty, UnitTestUserNotification.Instance.LastMessage.Text);

					exitDetail.CED_MovementReferenceNumber = MRNCode;
					downloadDocMenuItem.PerformClick();
					AssertContains("EAL Document Capture Request can not be sent when CSV Clearance is empty so nothing will be sent and no message will be shown", ZString.Empty, UnitTestUserNotification.Instance.LastMessage.Text);

					exitDetail.ZG_CSVClearance = "ABCDEFGHIJKLMNOP";
					exitHeader.Factory.Save();

					TestHelper.CheckFactoryHasNoPendingChanges("Before updating CSV", exitHeader.Factory);
					downloadDocMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After updating CSV", exitHeader.Factory);
					AssertContains("1 Document Capture request(s) created", UnitTestUserNotification.Instance.LastMessage.Text);

					exitHeader.CusExitDetails.RemoveAndDeleteAll();
					downloadDocMenuItem.PerformClick();
					AssertEquals("No details exist – Please add details before attempting to send a message.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			});
		}

		public void TestDownloadEAL_AllDocumentsNeeded_MultipleDeclarations()
		{
			using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				var downloadDocMenuItem = menuItems.FindByText("Download EAL Clearance Document");

				exitHeader.CEH_GS_NKCustomsAgent = Staff.GS_Code;
				exitHeader.CEH_CustomsProfile = BuilderHelperTest.CertificateName;
				exitDetail.ZG_CSVClearance = "ABCDEFGHIJKLMNOP";

				var exitDetail2 = exitHeader.CusExitDetails.AddNew();
				exitDetail2.CED_MovementReferenceNumber = "mrnCode2";
				exitDetail2.ZG_CSVClearance = "ABCDEFGHIJKLMNOP";

				exitHeader.Factory.Save();
				CombineAssertions(() =>
				{
					TestHelper.CheckFactoryHasNoPendingChanges("Before updating CSV", exitHeader.Factory);
					downloadDocMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After updating CSV", exitHeader.Factory);
					AssertContains("2 Document Capture request(s) created", UnitTestUserNotification.Instance.LastMessage.Text);
				});
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			var declaration = Factory.New<JobDeclaration>();

			exitHeader = Factory.NewWithValidTestData<CusExitControlHeader>();
			exitHeader.CEH_ParentTableCode = "JE";
			exitHeader.CEH_ParentID = declaration.PK;

			exitDetail = exitHeader.CusExitDetails.AddNew();
			exitDetail.CED_MovementReferenceNumber = MRNCode;

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			menuProvider = new SendMenuForTest(exitHeader);
			menuItems = menuProvider.CreateMenuItems().ToArray();
		}

		CusExitControlHeader exitHeader;
		CusExitDetail exitDetail;
		SendMenuForTest menuProvider;
		ZMenuItem[] menuItems;

		public GlbStaff Staff
		{
			get
			{
				if (staff == null)
				{
					staff = Factory.GetStaffAccount();
				}

				return staff;
			}
		}
		GlbStaff staff;

		const string MRNCode = "MRNCode";

		class SendFormForTest : ECSMessageSendingForm
		{
			public SendFormForTest(ECSExitHeaderMessageSendingObjectParent exitHeaderWrapper)
				: base(exitHeaderWrapper)
			{
				foreach (ECSExitHeaderMessageSendingObject sendingObject in exitHeaderWrapper.SendingObjectsCollection)
				{
					sendingObject.ShouldSend = true;
				}
			}

			public ZGrid MessageSendingGrid => base.MessageSendingObjectsGrid;
		}

		class SendMenuForTest : EcsMessageMenuProvider
		{
			public SendMenuForTest(EU.Business.CusExitControlHeader exitHeader) : base(exitHeader)
			{
			}

			public MenuItem sendToCustomsMenuItem => base.CreateMenuItems().FindByText("Arrive at Exit Location");
			protected override ECSMessageSendingForm GetMessageSendingForm(ECSExitHeaderMessageSendingObjectParent exitHeaderWrapper)
				=> new SendFormForTest(exitHeaderWrapper);
			protected override MessageEditForm GetMessageEditForm()
				 => new MessageEditFormForTest();
		}

		class MessageEditFormForTest : MessageEditForm
		{
			public MessageEditFormForTest()
				: base()
			{
			}

			public override (ZString, ZBool) EditMessage(ZString messageText) => (messageText.Replace(MRNCode, "AAAAAAAAAAAAA"), true);
		}
	}
}
