using System.Linq;
using System.Windows.Forms;
using Enterprise.Core.Forms;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.GUI.Testing
{
	[TestedType(typeof(AUCOLSAttachmentsSelectionForm))]
	sealed class AUCOLSAttachmentsSelectionFormTest : ZFormBasherTest
	{
		public void TestSendBoundButtonDisabledOnLoadWhenMustSelectOneOrMoreDocuments()
		{
			var doc = colsHeader.EDocPivotCollection.AddNew();
			doc.CSD_MessageStatus = COLSDocumentStatusList.Codes.Discarded;
			using (var form = GetAttachmentsSelectionForm())
			{
				var attachmentOption = (CusStorageDocPivotMessageSendingAction)messageSendingActionParent.SendingObjectsCollection.Single();
				AssertEquals("Pre-requisite: form MustSelectOneOrMoreAttachments is True", true, form.MustSelectOneOrMoreAttachments);
				AssertEquals("Pre-requisite: attachment is not selected", false, attachmentOption.ShouldSend);
				AssertEquals("Send button is disabled on load no attachments are selected", false, form.SendBoundButton.Enabled);

				attachmentOption.ShouldSend = true;
				AssertEquals("Send button is enabled now that attachment is selected", true, form.SendBoundButton.Enabled);
			}
		}

		public void TestSendBoundButtonEnabledOnLoadWhenDontHaveToSelectOneOrMoreDocuments()
		{
			var doc = colsHeader.EDocPivotCollection.AddNew();
			doc.CSD_MessageStatus = COLSDocumentStatusList.Codes.Discarded;
			using (var form = GetAttachmentsSelectionForm(false))
			{
				var attachmentOption = (CusStorageDocPivotMessageSendingAction)messageSendingActionParent.SendingObjectsCollection.Single();
				AssertEquals("Pre-requisite: form MustSelectOneOrMoreAttachments is False", false, form.MustSelectOneOrMoreAttachments);
				AssertEquals("Pre-requisite: attachment is not selected", false, attachmentOption.ShouldSend);
				AssertEquals("Send button is enabled on load", true, form.SendBoundButton.Enabled);
			}
		}

		public void TestSendBoundButtonEnabledOnLoad()
		{
			_ = colsHeader.EDocPivotCollection.AddNew();
			using (var form = GetAttachmentsSelectionForm())
			{
				var attachmentOption = (CusStorageDocPivotMessageSendingAction)messageSendingActionParent.SendingObjectsCollection.Single();
				AssertEquals("Pre-requisite: attachment is selected", true, attachmentOption.ShouldSend);
				AssertEquals("Send button is enabled on load when one or more attachments are selected", true, form.SendBoundButton.Enabled);

				attachmentOption.ShouldSend = false;
				AssertEquals("Send button is disabled now that attachment is not selected", false, form.SendBoundButton.Enabled);
			}
		}

		public void TestFormCaption()
		{
			using (var form = GetAttachmentsSelectionForm())
			{
				AssertEquals("Select Attachments", form.Text);
			}
		}

		public void TestMessageSendingObjectsGrid()
		{
			using (var form = GetAttachmentsSelectionForm())
			{
				var groupBox = form.MessageSendingObjectsGroupBox;
				var grid = form.MessageSendingObjectsGrid;
				AssertEquals("Grid.BindTo", nameof(CusStorageDocPivotMessageSendingActionParent.SendingObjectsCollection), grid.BindTo);
				AssertSequencesEqual("Grid.Columns", new string[] {
						nameof(CusStorageDocPivotMessageSendingAction.ShouldSend),
						nameof(CusStorageDocPivotMessageSendingAction.DocReference),
						nameof(CusStorageDocPivotMessageSendingAction.DocType),
						nameof(CusStorageDocPivotMessageSendingAction.Description),
						nameof(CusStorageDocPivotMessageSendingAction.MessageStatus),
						nameof(CusStorageDocPivotMessageSendingAction.MessageStatusDescription) },
					grid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName));
				AssertEquals("Grid is within groupbox", true, groupBox.Contains(grid));
				AssertEquals("GroupBox.Text", "Attachments to be sent", groupBox.Text);
			}
		}

		protected override Form GetFormToBashCore() => GetAttachmentsSelectionForm();

		protected override void SetUp()
		{
			base.SetUp();

			colsHeader = Factory.New<QuarantineColsHeader>();
			messageSendingActionParent = new CusStorageDocPivotMessageSendingActionParent(colsHeader);
		}

		AUCOLSAttachmentsSelectionForm GetAttachmentsSelectionForm(bool mustSelectOneOrMoreAttachments = true)
		{
			return new AUCOLSAttachmentsSelectionForm(messageSendingActionParent, mustSelectOneOrMoreAttachments);
		}

		QuarantineColsHeader colsHeader;
		CusStorageDocPivotMessageSendingActionParent messageSendingActionParent;
	}
}
