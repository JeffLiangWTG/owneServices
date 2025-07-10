using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Manifest.Business;
using Enterprise.Customs.ES.Manifest.H7.Business;
using Enterprise.Customs.EU.H7.GUI.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using AsycudaManifestHeader = Enterprise.Customs.ES.Manifest.H7.Business.AsycudaManifestHeader;

namespace Enterprise.Customs.ES.Manifest.H7.GUI.Testing
{
	[TestedType(typeof(G3MessageSendingForm))]
	sealed class G3MessageSendingFormTest : EU.H7.GUI.Testing.MessageSendingFormTest
	{
		[RequiresSTA]
		public void TestMessageSendingObjectsGridColumnsForRevokeMessage()
		{
			using var form = CreateSendingMessageForm(true);
			form.Show();

			var grid = form.Controls.Find("MessageSendingObjectsGrid", true).Single() as ZGrid;
			EUH7GUITestHelper.AssertGridLayout(grid, ExpectedGridColumnNamesForRevokeMesage);
		}

		public void TestOverrideRevokeReasonControl()
		{
			using var presentForm = CreateSendingMessageForm();
			presentForm.Show();
			var overrideRevokeReasonControl = presentForm.Controls.Find("OverrideRevokeReasonUserControl", true).FirstOrDefault();
			AssertNull(overrideRevokeReasonControl);

			using var revokeForm = CreateSendingMessageForm(true);
			overrideRevokeReasonControl = revokeForm.Controls.Find("OverrideRevokeReasonUserControl", true).FirstOrDefault();
			AssertNotNull(overrideRevokeReasonControl);
		}

		public void TestSendMessages()
		{
			var broker = Factory.NewWithValidTestData<GlbStaff>();
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_GS_NKCustomsAgent = broker.GS_Code;
			header.AMA_CustomsProfile = "test";

			header.Bills.AddNew();

			var messageSendingObjectParent = new G3MessageSendingObjectParent(header);
			messageSendingObjectParent.SendingObjectsCollection[0].Action = G3MessageTypes.Codes.G3Declaration;
			messageSendingObjectParent.SendingObjectsCollection[0].ShouldSend = true;

			header.Branch.OrgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "ES1230789654", "ES");

			using (var form = new G3MessageSendingForm(messageSendingObjectParent))
			{
				form.Show();

				var sendWithValidationErrorsCheckBox = form.FindSingle<ZCheckBox>("SendWithValidationErrorsCheckBox");
				sendWithValidationErrorsCheckBox.Checked = true;

				var sendWithAdditionalWarningCheckBox = form.FindSingle<ZCheckBox>("SendWithAdditionalWarningCheckBox");
				sendWithAdditionalWarningCheckBox.Checked = true;

				var sendButton = form.FindSingleOrDefault<ZButton>(c => c.Name == "SendButton");
				sendButton.PerformClick();

				CombineAssertions("New message created", () =>
				{
					AssertEquals("One message should be created", 1, header.Messages.Count);
					AssertEquals("Message Type", "G3D", header.Messages[0].EM_MessageType);
					AssertEquals("Direction", "TRX", header.Messages[0].EM_ReceiveTransmit);
					AssertEquals("Status", "QUE", header.Messages[0].EM_Status);
					AssertEquals("Success notification message", "1 message(s) queued for sending.", UnitTestUserNotification.Instance.LastMessage.Text);
				});
			}
		}

		public void TestShowErrorWhenFailToSendG3Message()
		{
			var broker = Factory.NewWithValidTestData<GlbStaff>();
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_GS_NKCustomsAgent = broker.GS_Code;
			header.AMA_CustomsProfile = "test";

			header.Bills.AddNew();

			var messageSendingObjectParent = new G3MessageSendingObjectParent(header);
			messageSendingObjectParent.SendingObjectsCollection[0].Action = "ABC";
			messageSendingObjectParent.SendingObjectsCollection[0].ShouldSend = true;

			using (var form = new G3MessageSendingForm(messageSendingObjectParent))
			{
				form.Show();

				var sendWithValidationErrorsCheckBox = form.FindSingle<ZCheckBox>("SendWithValidationErrorsCheckBox");
				sendWithValidationErrorsCheckBox.Checked = true;

				var sendWithAdditionalWarningCheckBox = form.FindSingle<ZCheckBox>("SendWithAdditionalWarningCheckBox");
				sendWithAdditionalWarningCheckBox.Checked = true;

				var sendButton = form.FindSingleOrDefault<ZButton>(c => c.Name == "SendButton");
				sendButton.PerformClick();

				AssertEquals("Could not create G3 message for H7 Job.", UnitTestUserNotification.Instance.LastMessage.Text);
				ErrorReporter.Clear();
			}
		}

		public void TestPreviewMessage_ShouldAllowPreview()
		{
			var broker = Factory.NewWithValidTestData<GlbStaff>();
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_GS_NKCustomsAgent = broker.GS_Code;
			header.AMA_CustomsProfile = "test";

			header.Bills.AddNew();
			header.Bills.AddNew();

			var parent = new G3MessageSendingObjectParent(header);
			using (var form = new G3MessageSendingForm(parent))
			{
				form.Show();
				var previewMessageCheckBox = form.Controls.Find("PreviewMessageCheckBox", true)[0] as ZCheckBox;

				parent.SendingObjectsCollection[0].ShouldSend = true;
				parent.SendingObjectsCollection[1].ShouldSend = true;
				Assert("PreviewMessageCheckBox is enabled when multiple objects are selected", previewMessageCheckBox.Enabled);
			}
		}

		protected override Form GetFormToBashCore()
		{
			return CreateSendingMessageForm();
		}

		protected override Form GetTestingForm(BaseMessageSendingObjectParent testingParent)
		{
			return new G3MessageSendingForm(testingParent);
		}

		public override Type FormToBashType => typeof(G3MessageSendingForm);

		protected override Type MessageSendingGridColumnLayoutType => typeof(G3MessageSendingColumnLayout);

		protected override IReadOnlyList<string> ExpectedGridColumnNames => new[] { "ShouldSend", "BillNumber", "Action", "G3LocalReferenceNumber", "G3MovementReferenceNumber", "H7MovementReferenceNumber", "MessageStatus", "EntryStatus" };

		string[] ExpectedGridColumnNamesForRevokeMesage => new[] { "ShouldSend", "BillNumber", "Action", "G3LocalReferenceNumber", "G3MovementReferenceNumber", "H7MovementReferenceNumber", "MessageStatus", "EntryStatus", "RevokeReason", "RevokeReasonDescription" };

		protected override bool ExpectedAllowOverrideMessageType => false;

		Form CreateSendingMessageForm(bool isRevoke = false)
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var testingParent = new G3MessageSendingObjectParent(header, isRevoke);

			return new G3MessageSendingForm(testingParent, isRevoke);
		}
	}
}
