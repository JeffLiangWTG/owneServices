using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.Customs.GB.H7.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;
using AsycudaManifestHeader = Enterprise.Customs.GB.H7.Business.AsycudaManifestHeader;
using MessageSendingObject = Enterprise.Customs.GB.H7.Business.MessageSendingObject;

namespace Enterprise.Customs.GB.H7.GUI.Testing
{
	[TestedType(typeof(MessageSendingForm))]
	sealed class MessageSendingFormTest : ZFormBasherTest
	{
		public void TestMessageSendingGridColumnLayoutProvider()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var testingParent = new MessageSendingObjectParent<MessageSendingObject>(header);

			using (var form = new MessageSendingForm(testingParent))
			{
				var messageSendingGridColumnLayoutProviderPropertyInfo = form.GetType().GetProperty("MessageSendingGridColumnLayoutProvider", BindingFlags.Instance | BindingFlags.NonPublic);
				var messageSendingGridColumnLayoutProvider = messageSendingGridColumnLayoutProviderPropertyInfo.GetValue(form, null);

				AssertType<MessageSendingGridColumnLayout>(messageSendingGridColumnLayoutProvider);
			}
		}

		public void TestSendMessageToCustoms_ShowProgess()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.Bills.AddNew();
			header.Bills.AddNew();

			var progressUpdateLogs = new List<(int ProcessedMessages, int MessagesToProcess)>();
			Action<int, int> progressUpdateCallback = (message, progress) =>
			{
				progressUpdateLogs.Add((message, progress));
			};

			var testingParent = new MessageSendingObjectParent<MessageSendingObject>(header);

			using (var form = new MessageSendingFormForTesting(testingParent))
			{
				var messagesSent = form.SendMessageToCustoms(progressUpdateCallback);

				CombineAssertions(() =>
				{
					AssertEquals("2 messages should be sent", 2, messagesSent);
					AssertEquals("Expected logs count", 2, progressUpdateLogs.Count);
				});
			}
		}

		public void TestSendMessageToCustoms_ShowMessagePreview()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill1 = header.Bills.AddNew();

			var testingParent = new MessageSendingObjectParent<MessageSendingObject>(header);
			testingParent.SendingObjectsCollection[0].Action = H7EDIMessageTypeList.Codes.NewDeclaration;

			using (var form = new MessageSendingFormForTesting(testingParent))
			{
				form.Show();
				var sendWithValidationErrorsCheckBox = form.FindSingle<ZCheckBox>("SendWithValidationErrorsCheckBox");
				sendWithValidationErrorsCheckBox.Checked = true;

				var messagPreviewCheckBox = form.FindSingle<ZCheckBox>("PreviewMessageCheckBox");
				messagPreviewCheckBox.Checked = true;

				var sendButton = form.FindSingleOrDefault<ZButton>(c => c.Name == "SendButton");
				sendButton.PerformClick();
				var messagePreviewForm = ZFormModaliser.LastFormShownDialogForTest;

				CombineAssertions(() =>
				{
					AssertEquals("Message Preview Form Shows", "Enterprise.Customs.GUI.MessageEditForm", messagePreviewForm.GetType().FullName);
				});
			}
		}

		protected override Form GetFormToBashCore()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var testingParent = new MessageSendingObjectParent<MessageSendingObject>(header);
			return new MessageSendingForm(testingParent);
		}

		public override Type FormToBashType => typeof(MessageSendingForm);
	}

	class MessageSendingFormForTesting : MessageSendingForm
	{
		public MessageSendingFormForTesting(BaseMessageSendingObjectParent parent)
			: base(parent)
		{
		}

		protected override bool PreviewMessageCheckboxVisible => true;

		public new int SendMessageToCustoms(Action<int, int> updateProgressCallback) => base.SendMessageToCustoms(updateProgressCallback);
	}
}
