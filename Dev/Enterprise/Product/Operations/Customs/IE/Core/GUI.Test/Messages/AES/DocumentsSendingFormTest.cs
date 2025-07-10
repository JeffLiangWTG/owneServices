using System.Windows.Forms;
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
	[TestedType(typeof(DocumentsSendingForm))]
	class DocumentsSendingFormTest : ZFormBasherTest
	{
		protected override bool AllowHasChangesOnFormOpen => true;

		protected override bool AllowSaveOnFormForTestHasChanges => false;

		protected override Form GetFormToBashCore()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var enryHeader = declaration.CustomsEntryHeaders.AddNew();
			var messageSendingObjectParent = new DocumentsSendingActionParent(declaration, AESOutgoingMessageTypeList.Codes.DocumentUpload);
			return new DocumentsSendingForm(messageSendingObjectParent);
		}

		public void TestMessageSendingGridColumns()
		{
			using (var form = new DocumentsSendingForm())
			{
				var messageSendingGrid = form.FindSingle<ZGrid>("MessageSendingObjectsGrid");
				AssertNotNull("MessageSendingObjectsGrid", messageSendingGrid);
				AssertEquals("MessageSendingObjectsGrid ColumnStyles Count", 3, messageSendingGrid.ColumnStyles.Count);
			}
		}

		public void TestDocumentSendingSupportingDocumentUserControl()
		{
			using (var form = new DocumentsSendingForm())
			{
				var supportingDocumentsControl = form.DocumentSendingSupportingDocumentUserControl;
				AssertNotNull(supportingDocumentsControl);
				AssertType<DocumentSendingSupportingDocumentUserControl>(form.DocumentSendingSupportingDocumentUserControl);
			}
		}
	}
}
