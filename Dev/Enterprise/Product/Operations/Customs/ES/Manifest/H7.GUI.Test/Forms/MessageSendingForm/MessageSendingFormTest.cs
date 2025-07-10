using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Manifest.H7.GUI.Testing
{
	[TestedType(typeof(MessageSendingForm))]
	sealed class MessageSendingFormTest : EU.H7.GUI.Testing.MessageSendingFormTest
	{
		public void TestSendH7QueryMessage()
		{
			var broker = Factory.NewWithValidTestData<GlbStaff>();
			var header = Factory.NewWithValidTestData<Business.AsycudaManifestHeader>();
			header.AMA_GS_NKCustomsAgent = broker.GS_Code;
			header.AMA_CustomsProfile = "test";

			var bill = header.Bills.AddNew();

			var messageSendingObjectParent = new MessageSendingObjectParent<Business.H7MessageSendingObject>(header);
			foreach (var messageSendingObject in messageSendingObjectParent.SendingObjectsCollection)
			{
				((MessageSendingObject)messageSendingObject).Action = DeclarationMessageTypeList.Codes.H7Query;
			}

			using (var form = new MessageSendingForm(messageSendingObjectParent))
			{
				form.Show();

				var sendWithValidationErrorsCheckBox = form.FindSingle<ZCheckBox>("SendWithValidationErrorsCheckBox");
				sendWithValidationErrorsCheckBox.Checked = true;

				var sendButton = form.FindSingleOrDefault<ZButton>(c => c.Name == "SendButton");
				sendButton.PerformClick();

				CombineAssertions("New message created", () =>
				{
					AssertEquals(1, bill.Messages.Count);
					AssertEquals("Application code", "ESC", bill.Messages[0].EM_ApplicationCode);
					AssertEquals("Message Type", DeclarationMessageTypeList.Codes.H7Query, bill.Messages[0].EM_MessageType);
					AssertEquals("Direction", "TRX", bill.Messages[0].EM_ReceiveTransmit);
					AssertEquals("Status", "QUE", bill.Messages[0].EM_Status);
					AssertEquals("1 message(s) queued for sending.", UnitTestUserNotification.Instance.LastMessage.Text);
				});
			}
		}

		public void TestSendMessage_ShowProgress()
		{
			var broker = Factory.NewWithValidTestData<GlbStaff>();
			var header = Factory.NewWithValidTestData<Business.AsycudaManifestHeader>();
			header.AMA_GS_NKCustomsAgent = broker.GS_Code;
			header.AMA_CustomsProfile = "test";
			header.Bills.AddNew();
			header.Bills.AddNew();

			var messageSendingObjectParent = new MessageSendingObjectParent<Business.H7MessageSendingObject>(header);
			foreach (var messageSendingObject in messageSendingObjectParent.SendingObjectsCollection)
			{
				((MessageSendingObject)messageSendingObject).Action = DeclarationMessageTypeList.Codes.H7Query;
			}

			var progressUpdateLogs = new List<(int ProcessedMessages, int MessagesToProcess)>();
			Action<int, int> progressUpdateCallback = (message, progress) =>
			{
				progressUpdateLogs.Add((message, progress));
			};

			using (var form = new MessageSendingFormForTesting(messageSendingObjectParent))
			{
				var messagesSent = form.SendMessageToCustoms(progressUpdateCallback);

				CombineAssertions(() =>
				{
					AssertEquals("2 messages should be sent", 2, messagesSent);
					AssertEquals("Expected logs count", 2, progressUpdateLogs.Count);
				});
			}
		}

		protected override Form GetFormToBashCore()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var testingParent = new MessageSendingObjectParent<Business.H7MessageSendingObject>(header);
			return new MessageSendingForm(testingParent);
		}

		public override Type FormToBashType => typeof(MessageSendingForm);

		protected override IReadOnlyList<string> ExpectedGridColumnNames => new[] { "ShouldSend", "BillNumber", "Action", "G3LocalReferenceNumber", "G3MovementReferenceNumber", "H7MovementReferenceNumber", "MessageStatus", "CustomsStatus", "OperationCode" };

		protected override Type MessageSendingGridColumnLayoutType => typeof(MessageSendingGridColumnLayout);
	}

	class MessageSendingFormForTesting : MessageSendingForm
	{
		public MessageSendingFormForTesting(BaseMessageSendingObjectParent parent)
			: base(parent)
		{
		}

		public new int SendMessageToCustoms(Action<int, int> updateProgressCallback) => base.SendMessageToCustoms(updateProgressCallback);
	}
}
