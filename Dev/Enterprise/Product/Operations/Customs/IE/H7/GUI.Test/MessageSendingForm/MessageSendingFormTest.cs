using System;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Enterprise.Customs.IE.H7.Business;
using Enterprise.Customs.IE.Messaging;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.H7.GUI.Testing
{
	[TestedType(typeof(MessageSendingForm))]
	sealed class MessageSendingFormTest : ZFormBasherTest
	{
		[RequiresSTA]
		public void TestAmendmentInvalidationReasonColumnIsAdded_WhenCoutryIsIE()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var testingParent = new EU.H7.Business.MessageSendingObjectParent<MessageSendingObject>(header);

			using (var form = new MessageSendingForm(testingParent))
			{
				form.Show();
				var messageSendingObjectsGrid = form.Controls.Find("MessageSendingObjectsGrid", searchAllChildren: true).Single() as ZGrid;
				var amendmentInvalidationReasonColumn = messageSendingObjectsGrid.GetColumnStyle("AmendmentInvalidationReason");

				Assert(string.Format("{0} is visible", messageSendingObjectsGrid.Name), messageSendingObjectsGrid.Visible);
				AssertNotNull("The column 'Amendment/Invalidation Reason' exists", amendmentInvalidationReasonColumn);
				AssertEquals(amendmentInvalidationReasonColumn.ColumnName, "AmendmentInvalidationReason");
			}
		}

		public void TestSendMessage_IM415()
		{
			SendMessageAndAssertNewEDIMessageCreated(AISOutgoingMessageTypeList.Codes.CustomsDeclaration);
		}

		public void TestSendMessage_IM414()
		{
			SendMessageAndAssertNewEDIMessageCreated(AISOutgoingMessageTypeList.Codes.InvalidationRequest);
		}

		void SendMessageAndAssertNewEDIMessageCreated(string actionCode)
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			var messageSendingObjectParent = new EU.H7.Business.MessageSendingObjectParent<MessageSendingObject>(header);

			foreach (var messageSendingObject in messageSendingObjectParent.SendingObjectsCollection)
			{
				((MessageSendingObject)messageSendingObject).Action = actionCode;
				if (actionCode == AISOutgoingMessageTypeList.Codes.InvalidationRequest)
				{
					((MessageSendingObject)messageSendingObject).AmendmentInvalidationReason = "Test";
				}
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
					AssertEquals("Application code", "IEI", bill.Messages[0].EM_ApplicationCode);
					AssertEquals("Message Type", actionCode, bill.Messages[0].EM_MessageType);
					AssertEquals("Direction", "TRX", bill.Messages[0].EM_ReceiveTransmit);
					AssertEquals("Status", "QUE", bill.Messages[0].EM_Status);
					AssertEquals("1 message(s) queued for sending.", UnitTestUserNotification.Instance.LastMessage.Text);
				});
			}
		}

		[RequiresSTA]
		public void TestMessageSendingGridColumnLayoutProvider()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var testingParent = new EU.H7.Business.MessageSendingObjectParent<MessageSendingObject>(header);

			using (var form = new MessageSendingForm(testingParent))
			{
				var messageSendingGridColumnLayoutProviderPropertyInfo = form.GetType().GetProperty("MessageSendingGridColumnLayoutProvider", BindingFlags.Instance | BindingFlags.NonPublic);
				var messageSendingGridColumnLayoutProvider = messageSendingGridColumnLayoutProviderPropertyInfo.GetValue(form, null);

				AssertType<MessageSendingGridColumnLayout>(messageSendingGridColumnLayoutProvider);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var testingParent = new EU.H7.Business.MessageSendingObjectParent<MessageSendingObject>(header);
			return new MessageSendingForm(testingParent);
		}

		public override Type FormToBashType => typeof(MessageSendingForm);
	}
}
