using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.IN.Manifest.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Manifest.GUI.Testing;

[TestedType(typeof(SendMessageForm))]
sealed class SendMessageFormTest : Customs.GUI.Testing.MessageSendingFormWithValidationDetailsAbstractTest
{
	public void TestMessageTypeTextLabel()
	{
		using (var form = new MessageSendingFormForTest(MessageSendingObjectParent))
		{
			var messageSendingObject = MessageSendingObjectParent.SendingObjectsCollection[0];
			form.Show();
			CombineAssertions(() =>
			{
				messageSendingObject.MessageType = ZString.Empty;
				AssertEquals("MessageType empty", ZString.Empty, form.MessageTypeTextLabelExposed.Text);

				messageSendingObject.MessageType = "F";
				AssertEquals("MessageType F", "Submission of the Fresh CGM Manifest Message to ICEGate.", form.MessageTypeTextLabelExposed.Text);

				messageSendingObject.MessageType = "D";
				AssertEquals("MessageType D", "Delete requisition for registered CGM Manifest Message to ICEGate.", form.MessageTypeTextLabelExposed.Text);

				messageSendingObject.MessageType = "A";
				AssertEquals("MessageType A", "Amendment requisition for registered CGM Manifest Message to ICEGate.", form.MessageTypeTextLabelExposed.Text);

				messageSendingObject.MessageType = "XYZ";
				AssertEquals("MessageType invalid", ZString.Empty, form.MessageTypeTextLabelExposed.Text);
			});
		}
	}

	protected override Form GetFormToBashCore()
	{
		return new MessageSendingFormForTest(MessageSendingObjectParent);
	}

	ManifestMessageSendingObjectParent GetManifestMessageSendingObjectParent()
	{
		var header = Factory.New<CGMAsycudaManifestHeader>();
		var sendingParent = new ManifestMessageSendingObjectParent(header);
		foreach (var sendingObject in sendingParent.SendingObjectsCollection)
		{
			sendingObject.HasChanges = false;
		}
		return sendingParent;
	}

	ManifestMessageSendingObjectParent MessageSendingObjectParent => messageSendingObjectParent ??= GetManifestMessageSendingObjectParent();
	ManifestMessageSendingObjectParent messageSendingObjectParent;

	class MessageSendingFormForTest : SendMessageForm
	{
		public MessageSendingFormForTest(ManifestMessageSendingObjectParent messageSendingObjectParent) : base(messageSendingObjectParent)
		{
		}

		public ZLabel MessageTypeTextLabelExposed => base.MessageTypeTextLabel;
	}
}
