using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.ExitControl.Business;
using Enterprise.Customs.EU.ExitControl.GUI.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.ES.ExitControl.GUI.Testing
{
	class ExitControlSendToCustomsMenuCreatorTest : ExitControlSendToCustomsMenuCreatorAbstractTest
	{
		[RequiresSTA]
		public void TestCreateExitControlMessage()
		{
			var sendToCustomsMenuItem = new ExitControlSendToCustomsMenuCreatorForTest(exitHeader).Create();

			CombineAssertions(() =>
			{
				sendToCustomsMenuItem.PerformClick();
				AssertEquals("Message informing no exit reports", "No reports exist – Please add exit reports before attempting to send a message.", UnitTestUserNotification.Instance.LastMessage.Text);

				var exitReport1 = exitHeader.CusExitReports.AddNew();
				var consignment1 = exitHeader.CusExitConsignments.AddNew();
				consignment1.CXC_LocalReference = "Ref1";
				exitReport1.CER_CXC_Consignment = consignment1.PK;
				exitReport1.CER_Location = "Location";

				var exitReport2 = exitHeader.CusExitReports.AddNew();
				var consignment2 = exitHeader.CusExitConsignments.AddNew();
				consignment2.CXC_LocalReference = "Ref2";
				exitReport2.CER_CXC_Consignment = consignment2.PK;
				exitReport2.CER_Location = "Location";
				sendToCustomsMenuItem.PerformClick();
				AssertEquals("Message informing broker and certificate are needed", "Cannot send message without a broker and valid certificate; please enter the broker and a valid certificate in the details tab.", UnitTestUserNotification.Instance.LastMessage.Text);

				exitHeader.CXH_GS_NKCustomsAgent = Staff.GS_Code;
				exitHeader.CXH_CustomsProfile = ZString.Empty;
				exitHeader.Factory.Save();
				sendToCustomsMenuItem.PerformClick();
				AssertEquals("Message informing broker and certificate are needed when broker is declared but certificate is empty", "Cannot send message without a broker and valid certificate; please enter the broker and a valid certificate in the details tab.", UnitTestUserNotification.Instance.LastMessage.Text);

				exitHeader.CXH_CustomsProfile = "INVALID";
				exitHeader.Factory.Save();
				sendToCustomsMenuItem.PerformClick();
				AssertEquals("Message informing broker and certificate are needed when broker is declared but certificate declared is invalid", "Cannot send message without a broker and valid certificate; please enter the broker and a valid certificate in the details tab.", UnitTestUserNotification.Instance.LastMessage.Text);

				exitHeader.CXH_CustomsProfile = BuilderHelperTest.CertificateName;
				exitHeader.Factory.Save();
				sendToCustomsMenuItem.PerformClick();
				AssertEquals("Message informing broker and certificate are needed when broker is declared but current user has no authorisation for certificate declared", "Cannot send message without a broker and valid certificate; please enter the broker and a valid certificate in the details tab.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNull("User Notification Last Message was cleared", UnitTestUserNotification.Instance.LastMessage.Text);
				exitHeader.Reload();
				sendToCustomsMenuItem.PerformClick();
				AssertEquals("Message informing broker and certificate are needed when broker is declared but current user has no authorisation for certificate declared, even if we haven't done a new save& validation", "Cannot send message without a broker and valid certificate; please enter the broker and a valid certificate in the details tab.", UnitTestUserNotification.Instance.LastMessage.Text);

				using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
				{
					exitHeader.Reload();
					sendToCustomsMenuItem.PerformClick();
					AssertContains("Messages sent correctly", "2 Messages sent successfully.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertNotContains("New message's text has not been edited, exitReport1", "AAAAAAAAAAAAA", exitReport1.Messages.LastOutgoingMessage.EM_MessageText);
					AssertNotContains("New message's text has not been edited, exitReport2", "AAAAAAAAAAAAA", exitReport2.Messages.LastOutgoingMessage.EM_MessageText);
				}
			});
		}

		[RequiresSTA]
		public void TestCreateExitControlMessage_PreviewMessage()
		{
			var sendToCustomsMenuItem = new ExitControlSendToCustomsMenuCreatorForTest(exitHeader).Create();

			CombineAssertions(() =>
			{
				sendToCustomsMenuItem.PerformClick();
				AssertEquals("Message informing no exit reports", "No reports exist – Please add exit reports before attempting to send a message.", UnitTestUserNotification.Instance.LastMessage.Text);

				var exitReport1 = exitHeader.CusExitReports.AddNew();
				var consignment1 = exitHeader.CusExitConsignments.AddNew();
				consignment1.CXC_LocalReference = "Ref1";
				exitReport1.CER_CXC_Consignment = consignment1.PK;
				exitReport1.CER_Location = "Location";
				sendToCustomsMenuItem.PerformClick();
				AssertEquals("Message informing broker and certificate are needed", "Cannot send message without a broker and valid certificate; please enter the broker and a valid certificate in the details tab.", UnitTestUserNotification.Instance.LastMessage.Text);

				exitHeader.CXH_GS_NKCustomsAgent = Staff.GS_Code;
				exitHeader.CXH_CustomsProfile = ZString.Empty;
				exitHeader.Factory.Save();
				sendToCustomsMenuItem.PerformClick();
				AssertEquals("Message informing broker and certificate are needed when broker is declared but certificate is empty", "Cannot send message without a broker and valid certificate; please enter the broker and a valid certificate in the details tab.", UnitTestUserNotification.Instance.LastMessage.Text);

				exitHeader.CXH_CustomsProfile = "INVALID";
				exitHeader.Factory.Save();
				sendToCustomsMenuItem.PerformClick();
				AssertEquals("Message informing broker and certificate are needed when broker is declared but certificate declared is invalid", "Cannot send message without a broker and valid certificate; please enter the broker and a valid certificate in the details tab.", UnitTestUserNotification.Instance.LastMessage.Text);

				exitHeader.CXH_CustomsProfile = BuilderHelperTest.CertificateName;
				exitHeader.Factory.Save();
				sendToCustomsMenuItem.PerformClick();
				AssertEquals("Message informing broker and certificate are needed when broker is declared but current user has no authorisation for certificate declared", "Cannot send message without a broker and valid certificate; please enter the broker and a valid certificate in the details tab.", UnitTestUserNotification.Instance.LastMessage.Text);

				using (RegistryTemporarySetterHelper.SetAllowEditEDIMessageBody(false))
				using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
				{
					exitHeader.Reload();
					sendToCustomsMenuItem.PerformClick();
					AssertContains("Messages sent correctly", "1 Message sent successfully.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertNotContains("New message's text has not been edited, only preview, exitReport1", "AAAAAAAAAAAAA", exitReport1.Messages.LastOutgoingMessage.EM_MessageText);
				}
			});
		}

		[RequiresSTA]
		public void TestCreateExitControlMessage_EditMessageText_OneReport()
		{
			var sendToCustomsMenuItem = new ExitControlSendToCustomsMenuCreatorForTest(exitHeader).Create();

			CombineAssertions(() =>
			{
				sendToCustomsMenuItem.PerformClick();
				AssertEquals("Message informing no exit reports", "No reports exist – Please add exit reports before attempting to send a message.", UnitTestUserNotification.Instance.LastMessage.Text);

				var exitReport1 = exitHeader.CusExitReports.AddNew();
				var consignment1 = exitHeader.CusExitConsignments.AddNew();
				consignment1.CXC_LocalReference = "Ref1";
				exitReport1.CER_CXC_Consignment = consignment1.PK;
				exitReport1.CER_Location = "Location";
				sendToCustomsMenuItem.PerformClick();
				AssertEquals("Message informing broker and certificate are needed", "Cannot send message without a broker and valid certificate; please enter the broker and a valid certificate in the details tab.", UnitTestUserNotification.Instance.LastMessage.Text);

				exitHeader.CXH_GS_NKCustomsAgent = Staff.GS_Code;
				exitHeader.CXH_CustomsProfile = ZString.Empty;
				exitHeader.Factory.Save();
				sendToCustomsMenuItem.PerformClick();
				AssertEquals("Message informing broker and certificate are needed when broker is declared but certificate is empty", "Cannot send message without a broker and valid certificate; please enter the broker and a valid certificate in the details tab.", UnitTestUserNotification.Instance.LastMessage.Text);

				exitHeader.CXH_CustomsProfile = "INVALID";
				exitHeader.Factory.Save();
				sendToCustomsMenuItem.PerformClick();
				AssertEquals("Message informing broker and certificate are needed when broker is declared but certificate declared is invalid", "Cannot send message without a broker and valid certificate; please enter the broker and a valid certificate in the details tab.", UnitTestUserNotification.Instance.LastMessage.Text);

				exitHeader.CXH_CustomsProfile = BuilderHelperTest.CertificateName;
				exitHeader.Factory.Save();
				sendToCustomsMenuItem.PerformClick();
				AssertEquals("Message informing broker and certificate are needed when broker is declared but current user has no authorisation for certificate declared", "Cannot send message without a broker and valid certificate; please enter the broker and a valid certificate in the details tab.", UnitTestUserNotification.Instance.LastMessage.Text);

				using (RegistryTemporarySetterHelper.SetAllowEditEDIMessageBody(true))
				using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
				{
					ZFormModaliser.ShowDialogsInTest = false;
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

					exitHeader.Reload();
					sendToCustomsMenuItem.PerformClick();
					AssertContains("Messages sent correctly", "1 Message sent successfully.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertContains("New message's text has been edited, exitReport1", "AAAAAAAAAAAAA", exitReport1.Messages.LastOutgoingMessage.EM_MessageText);
				}
			});
		}

		[RequiresSTA]
		public void TestCreateExitControlMessage_EditMessageText_Multiple()
		{
			var sendToCustomsMenuItem = new ExitControlSendToCustomsMenuCreatorForTest(exitHeader).Create();

			CombineAssertions(() =>
			{
				sendToCustomsMenuItem.PerformClick();
				AssertEquals("Message informing no exit reports", "No reports exist – Please add exit reports before attempting to send a message.", UnitTestUserNotification.Instance.LastMessage.Text);

				var exitReport1 = exitHeader.CusExitReports.AddNew();
				var consignment1 = exitHeader.CusExitConsignments.AddNew();
				consignment1.CXC_LocalReference = "Ref1";
				exitReport1.CER_CXC_Consignment = consignment1.PK;
				exitReport1.CER_Location = "Location";

				var exitReport2 = exitHeader.CusExitReports.AddNew();
				var consignment2 = exitHeader.CusExitConsignments.AddNew();
				consignment2.CXC_LocalReference = "Ref2";
				exitReport2.CER_CXC_Consignment = consignment2.PK;
				exitReport2.CER_Location = "Location";
				sendToCustomsMenuItem.PerformClick();
				AssertEquals("Message informing broker and certificate are needed", "Cannot send message without a broker and valid certificate; please enter the broker and a valid certificate in the details tab.", UnitTestUserNotification.Instance.LastMessage.Text);

				exitHeader.CXH_GS_NKCustomsAgent = Staff.GS_Code;
				exitHeader.CXH_CustomsProfile = ZString.Empty;
				exitHeader.Factory.Save();
				sendToCustomsMenuItem.PerformClick();
				AssertEquals("Message informing broker and certificate are needed when broker is declared but certificate is empty", "Cannot send message without a broker and valid certificate; please enter the broker and a valid certificate in the details tab.", UnitTestUserNotification.Instance.LastMessage.Text);

				exitHeader.CXH_CustomsProfile = "INVALID";
				exitHeader.Factory.Save();
				sendToCustomsMenuItem.PerformClick();
				AssertEquals("Message informing broker and certificate are needed when broker is declared but certificate declared is invalid", "Cannot send message without a broker and valid certificate; please enter the broker and a valid certificate in the details tab.", UnitTestUserNotification.Instance.LastMessage.Text);

				exitHeader.CXH_CustomsProfile = BuilderHelperTest.CertificateName;
				exitHeader.Factory.Save();
				sendToCustomsMenuItem.PerformClick();
				AssertEquals("Message informing broker and certificate are needed when broker is declared but current user has no authorisation for certificate declared", "Cannot send message without a broker and valid certificate; please enter the broker and a valid certificate in the details tab.", UnitTestUserNotification.Instance.LastMessage.Text);

				using (RegistryTemporarySetterHelper.SetAllowEditEDIMessageBody(true))
				using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
				{
					ZFormModaliser.ShowDialogsInTest = false;
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

					exitHeader.Reload();
					sendToCustomsMenuItem.PerformClick();
					AssertContains("Messages sent correctly", "2 Messages sent successfully.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertContains("New message's text has been edited, exitReport1", "AAAAAAAAAAAAA", exitReport1.Messages.LastOutgoingMessage.EM_MessageText);
					AssertContains("New message's text has been edited, exitReport2", "AAAAAAAAAAAAA", exitReport2.Messages.LastOutgoingMessage.EM_MessageText);
				}
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			exitHeader = Factory.NewWithValidTestData<CusExitHeader>();

			ZFormModaliser.ShowDialogsInTest = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
		}

		CusExitHeader exitHeader;

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

		class ExitControlSendToCustomsMenuCreatorForTest : ExitControlSendToCustomsMenuCreator
		{
			public ExitControlSendToCustomsMenuCreatorForTest(CusExitHeader header) : base(header)
			{
			}

			protected override EU.ExitControl.Business.ExitControlMessageSendingObjectParent GetMessageSendingParent()
			{
				var sendingParent = base.GetMessageSendingParent();
				foreach (ExitControlMessageSendingObject sendingObject in sendingParent.SendingObjectsCollection)
				{
					sendingObject.ShouldSend = true;
				}
				return sendingParent;
			}
			protected override ES.GUI.MessageEditForm GetMessageEditForm()
				 => new MessageEditFormForTest();
		}

		class MessageEditFormForTest : ES.GUI.MessageEditForm
		{
			public MessageEditFormForTest()
				: base()
			{
			}

			public override (ZString, ZBool) EditMessage(ZString messageText) => (messageText.Replace("Location", "AAAAAAAAAAAAA"), true);
		}
	}
}
