using System.Linq;
using System.Windows.Forms;
using Enterprise.Core.Forms;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Messaging;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.GUI.Testing
{
	[TestedType(typeof(AISDocumentsUploadForm))]
	sealed class AISDocumentsUploadFormTest : ZFormBasherTest
	{
		protected override bool AllowHasChangesOnFormOpen => true;

		protected override bool AllowSaveOnFormForTestHasChanges => false;

		protected override Form GetFormToBashCore()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var enryHeader = declaration.CustomsEntryHeaders.AddNew();
			enryHeader.CH_CEI_Instruction = Factory.NewWithValidTestData<CusEntryInstruction>().PK;
			var messageSendingObjectParent = new UploadDocumentsSendingActionParent(declaration, AESOutgoingMessageTypeList.Codes.DocumentUpload);
			return new AISDocumentsUploadForm(messageSendingObjectParent);
		}

		public void TestMessageSendingGridColumns()
		{
			using (var form = GetFormToBashCore())
			{
				var messageSendingGrid = form.FindSingle<ZGrid>("MessageSendingObjectsGrid");
				AssertNotNull("MessageSendingObjectsGrid", messageSendingGrid);
				AssertEquals("MessageSendingObjectsGrid ColumnStyles Count", 6, messageSendingGrid.ColumnStyles.Count);
			}
		}

		public void TestAttachmentGridColumns()
		{
			using (var form = GetFormToBashCore())
			{
				var attachmentsGrid = form.FindSingle<ZGrid>("AttachmentsGrid");
				AssertNotNull("AttachmentsGrid", attachmentsGrid);
				AssertEquals("AttachmentsGrid ColumnStyles Count", 4, attachmentsGrid.ColumnStyles.Count);
			}
		}

		public void TestSendWithValidationErrorsCheckBoxHidden()
		{
			using (var form = GetFormToBashCore())
			{
				var checkBox = form.FindSingle<ZCheckBox>("SendWithValidationErrorsCheckBox");
				AssertEquals(false, checkBox.Visible);
			}
		}

		public void TestFormHeading()
		{
			using (var form = (AISDocumentsUploadForm)GetFormToBashCore())
			{
				AssertEquals("Form Heading", "Upload Documents", form.FormHeading);
			}
		}

		public void TestPreviewMessageCheckboxVisible()
		{
			using (var form = (AISDocumentsUploadForm)GetFormToBashCore())
			{
				form.Show();
				AssertEquals("PreviewMessageCheckBox visibility", true, form.PreviewMessageCheckBox.Visible);
			}
		}

		public void TestDescriptionColumn_ReadOnly()
		{
			using (var form = GetFormToBashCore())
			{
				var attachmentsGrid = form.FindSingle<ZGrid>("AttachmentsGrid");
				var fileDescColumn = attachmentsGrid.ColumnStyles.Cast<ZGridColumnInfo>().First(c => c.ColumnName.Equals("FileDescription"));
				AssertEquals("File description is editable", false, fileDescColumn.IsReadOnly);
			}
		}
	}
}
