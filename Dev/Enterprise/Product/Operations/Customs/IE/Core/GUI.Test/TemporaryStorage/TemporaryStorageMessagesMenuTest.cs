using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Business.CusTempStorage;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.GUI.Testing
{
	sealed class TemporaryStorageMessagesMenuTest : TestCaseWithFactory
	{
		public void TestGetNewMessageSendingForm()
		{
			using (var form = new ZForm())
			using (var menu = new TemporaryStorageMessagesMenuForTest(form))
			{
				var storageHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();
				var messageSendingHeader = new TemporaryStorageMessageSendingObjectParent(storageHeader);
				using (var sendingForm = menu.GetNewMessageSendingForm(messageSendingHeader))
				{
					AssertType<UCC6TemporaryStorageMessageSendingForm>("GetNewMessageSendingForm", sendingForm);
				}
			}
		}

		public void TestSendToCustoms_UCC5()
		{
			TestSendToCustoms(ImportDeclarationApplicationCodeList.Codes.V1);
		}

		public void TestSendToCustoms_UCC6()
		{
			TestSendToCustoms(ImportDeclarationApplicationCodeList.Codes.V2);
		}

		void TestSendToCustoms(string manifestType)
		{
			var tempStorage = Factory.New<TemporaryStorageHeader>();
			tempStorage.AMA_RN_NKCountry = Core.Constants.CountryCodes.Ireland;
			tempStorage.AMA_ManifestType = manifestType;
			using (var form = new TemporaryStorageForm(tempStorage))
			{
				var sendToCustomsMenuItem = form.Menu.MenuItems.FindByText("Send To Customs", true);
				AssertNotNull(sendToCustomsMenuItem);
				AssertEquals("Is Visible", true, sendToCustomsMenuItem.Visible);
				sendToCustomsMenuItem.PerformClick();
				AssertType<UCC6TemporaryStorageMessageSendingForm>(ZFormModaliser.LastFormShownDialogForTest);
			}
		}

		public void TestNeedToPreviewMessage()
		{
			using (var form = new ZForm())
			using (var menu = new TemporaryStorageMessagesMenuForTest(form))
			{
				AssertEquals(true, menu.NeedToPreviewMessage);
			}
		}

		public void TestLastOutgoingMessageIsWaitingForResponse()
		{
			var tempStorage = Factory.New<TemporaryStorageHeader>();
			tempStorage.AMA_RN_NKCountry = Core.Constants.CountryCodes.Ireland;
			var messagingProvider = tempStorage.MessagingProvider;
			var sendingObject = new TemporaryStorageMessageSendingObject(tempStorage);
			sendingObject.MessageType = IETemporaryStorageMessageTypeList.Codes.Declaration;
			messagingProvider.SendMessage(sendingObject, new SendsMessagesToCustomsGUI(), Globals.Message);

			using (var form = new TemporaryStorageForm(tempStorage))
			{
				var sendMessageMenuItem = form.Menu.MenuItems.FindByText("Send To Customs", true);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				sendMessageMenuItem.PerformClick();
				AssertEquals("The last outgoing T15 message is waiting for response, you can only resend another T15 message, do you want to resend this message?", UnitTestUserNotification.Instance.PreviousMessages[1].Text);
				AssertEquals("The resend message is of the same type as the last outgoing message", IETemporaryStorageMessageTypeList.Codes.Declaration, tempStorage.Messages[1].EM_MessageType);
			}
		}

		sealed class TemporaryStorageMessagesMenuForTest : TemporaryStorageMessagesMenu
		{
			public TemporaryStorageMessagesMenuForTest(ZForm parentForm) : base(parentForm)
			{
			}

			public new MessageSendingFormWithValidationDetails GetNewMessageSendingForm(BaseMessageSendingObjectParent messageSendingObjectParent) => base.GetNewMessageSendingForm(messageSendingObjectParent);

			public new bool NeedToPreviewMessage => base.NeedToPreviewMessage;
		}
	}
}
