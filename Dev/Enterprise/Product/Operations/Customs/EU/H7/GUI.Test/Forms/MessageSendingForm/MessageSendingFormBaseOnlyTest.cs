using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.Customs.EU.H7.Business.Testing;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using MessageSender = Enterprise.Customs.EU.H7.Business.MessageSender;

namespace Enterprise.Customs.EU.H7.GUI.Testing
{
	[TestedType(typeof(MessageSendingForm))]
	sealed class MessageSendingFormBaseOnlyTest : MessageSendingFormTest
	{
		public void TestSendMessages()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var branch = header.Branch.Company.Branches.AddNew();
			header.AMA_GB = branch.PK;
			var bill1 = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();
			var bill3 = header.Bills.AddNew();
			var sendingObjectParent = new MessageSendingObjectParent<MessageSendingObjectForTest>(header);
			using (var form = GetTestingForm(sendingObjectParent))
			{
				form.Show();
				var sendingObject1 = sendingObjectParent.SendingObjectsCollection
					.Cast<MessageSendingObjectForTest>().Single(x => x.Bill == bill1);
				var sendingObject2 = sendingObjectParent.SendingObjectsCollection
					.Cast<MessageSendingObjectForTest>().Single(x => x.Bill == bill2);
				var sendingObject3 = sendingObjectParent.SendingObjectsCollection
					.Cast<MessageSendingObjectForTest>().Single(x => x.Bill == bill3);
				sendingObject1.ShouldSend = true;
				sendingObject1.Action = "ABCD";
				sendingObject2.ShouldSend = false;
				sendingObject2.Action = "ABCD";
				sendingObject3.ShouldSend = true;
				sendingObject3.Action = "GHJE";

				var sendWithValidationErrorsCheckBox = form.FindSingle<ZCheckBox>("SendWithValidationErrorsCheckBox");
				sendWithValidationErrorsCheckBox.Checked = true;

				var sendButton = form.FindSingleOrDefault<ZButton>(c => c.Name == "SendButton");
				sendButton.PerformClick();
				AssertEquals("2 message(s) queued for sending.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertBillHasMessage(bill1, "ABC");
				AssertEquals("bill2.Messages.Count", 0, bill2.Messages.Count);
				AssertBillHasMessage(bill3, "GHJ");

				void AssertBillHasMessage(AsycudaBill bill, ZString messageType)
				{
					CombineAssertions("New message created", () =>
					{
						AssertEquals(1, bill.Messages.Count);
						var message = bill.Messages[0];
						AssertEquals("Message Type", messageType, message.EM_MessageType);
						AssertEquals("Direction", "TRX", message.EM_ReceiveTransmit);
						AssertEquals("Status", "QUE", message.EM_Status);
					});
				}
			}
		}

		public void TestSendMessages_WithProgressForm()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.Bills.AddNew();
			header.Bills.AddNew();

			var sendingObjectParent = new MessageSendingObjectParent<MessageSendingObjectForTest>(header);

			using var form = new MessageSendingFormForTesting(sendingObjectParent);
			form.Show();

			var sendWithValidationErrorsCheckBox = form.FindSingle<ZCheckBox>("SendWithValidationErrorsCheckBox");
			sendWithValidationErrorsCheckBox.Checked = true;

			var sendButton = form.FindSingleOrDefault<ZButton>(c => c.Name == "SendButton");
			sendButton.PerformClick();

			CombineAssertions(() =>
			{
				AssertEquals("Progress log count", 2, form.ProgressLogs.Count);

				AssertEquals("First log's ProcessedMessages", 1, form.ProgressLogs[0].ProcessedMessages);
				AssertEquals("First log's MessagesToProcess", 2, form.ProgressLogs[0].MessagesToProcess);

				AssertEquals("Second log's ProcessedMessages", 2, form.ProgressLogs[1].ProcessedMessages);
				AssertEquals("Second log's MessagesToProcess", 2, form.ProgressLogs[1].MessagesToProcess);

				AssertEquals("Progress form caption", "Sending H7 messages", form.ProgressForm.CaptionResourceString.Caption);
				AssertEquals("Progress status text", "[2 / 2] H7 Messages queued", form.ProgressForm.Status);

				AssertEquals("2 message(s) queued for sending.", UnitTestUserNotification.Instance.LastMessage.Text);
			});
		}

		public void TestSendMessages_WithoutProgressForm()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.Bills.AddNew();
			header.Bills.AddNew();

			var sendingObjectParent = new MessageSendingObjectParent<MessageSendingObjectForTest>(header);

			using var form = new MessageSendingFormForTesting(sendingObjectParent, false);
			form.Show();

			var sendWithValidationErrorsCheckBox = form.FindSingle<ZCheckBox>("SendWithValidationErrorsCheckBox");
			sendWithValidationErrorsCheckBox.Checked = true;

			var sendButton = form.FindSingleOrDefault<ZButton>(c => c.Name == "SendButton");
			sendButton.PerformClick();

			CombineAssertions(() =>
			{
				AssertNull("Progress Form is not created", form.ProgressForm);
				AssertEquals("2 message(s) queued for sending.", UnitTestUserNotification.Instance.LastMessage.Text);
			});
		}

		public void TestSendMessage_ExceptionOccurs_ShouldReportError()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.Bills.AddNew();

			var sendingObjectParent = new MessageSendingObjectParent<MessageSendingObjectForTest>(header);

			using var form = new MessageSendingFormForTesting(sendingObjectParent);
			form.Show();

			sendingObjectParent.SendingObjectsCollection.Cast<MessageSendingObjectForTest>().First().CreateSenderForTesting = (sendingObject) => new MessageSenderAlwaysThrowException(sendingObject);

			var sendWithValidationErrorsCheckBox = form.FindSingle<ZCheckBox>("SendWithValidationErrorsCheckBox");
			sendWithValidationErrorsCheckBox.Checked = true;

			var sendButton = form.FindSingleOrDefault<ZButton>(c => c.Name == "SendButton");
			sendButton.PerformClick();

			CombineAssertions("Should report error when exception occurred", () =>
			{
				AssertEquals("Key", "H7MessageSendingFailure", ErrorReporter.LastKeyReported);
				AssertEquals("Message", "Exception occurred when attempting to send message", ErrorReporter.LastMessageReported);
			});
			ErrorReporter.Clear();
		}

		protected override Form GetTestingForm(BaseMessageSendingObjectParent testingParent) => new MessageSendingForm(testingParent);
	}

	class MessageSendingFormForTesting : MessageSendingForm
	{
		public MessageSendingFormForTesting(BaseMessageSendingObjectParent parent, bool shouldShowProgressForm = true)
			: base(parent)
		{
			progressLogs = new List<(int ProcessedMessages, int MessagesToProcess)>();
			this.shouldShowProgressForm = shouldShowProgressForm;
		}

		public List<(int ProcessedMessages, int MessagesToProcess)> ProgressLogs => progressLogs;
		readonly List<(int ProcessedMessages, int MessagesToProcess)> progressLogs;

		protected override Action<int, int> ProgressUpdateCallBack(ProgressForm progressForm)
		{
			var baseCallback = base.ProgressUpdateCallBack(progressForm);

			return (int messagesSent, int messagesToBeSend) =>
			{
				baseCallback(messagesSent, messagesToBeSend);
				progressLogs.Add((messagesSent, messagesToBeSend));
			};
		}

		protected override bool ShouldShowProgressForm => shouldShowProgressForm;
		readonly bool shouldShowProgressForm;

		protected override ProgressForm CreateProgressForm()
		{
			progressForm = base.CreateProgressForm();
			return progressForm;
		}

		protected override bool CheckIsOKToSend() => true;

		public ProgressForm ProgressForm => progressForm;
		ProgressForm progressForm;
	}

	class MessageSenderAlwaysThrowException : MessageSender
	{
		public MessageSenderAlwaysThrowException(IH7MessageSendingObject sendingObject)
			: base(sendingObject)
		{
		}

		protected override IXmlMessageBuilder CreateMessageBuilder(EDIMessage message)
		{
			throw new NotImplementedException();
		}

		protected override EDIMessage CreateOutboundEDIMessage(BusinessObjectFactory factory)
		{
			throw new NotImplementedException();
		}
	}
}
