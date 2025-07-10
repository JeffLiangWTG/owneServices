using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;

namespace Enterprise.Customs.IE.GUI.Testing
{
	sealed class DocumentSendingSupportingDocumentUserControlTest : TestCaseWithFactory
	{
		public void TestDocumentSendingGridLayout_Type()
		{
			using (var control = new DocumentSendingSupportingDocumentUserControl())
			{
				AssertType<DocumentSendingSupportingDocumentGridColumnLayout>(control.GridColumnLayout);
			}
		}

		public void TestSupportingDocumentsGrid()
		{
			using (var control = new DocumentSendingSupportingDocumentUserControl())
			{
				var grid = control.SupportingDocumentsGrid;
				AssertNotNull("SupportingDocumentsGrid", grid);
				AssertEquals("SupportingDocumentsGrid Column Count", 4, grid.ColumnStyles.Count);
				AssertEquals("Binding Member", "SendingObjectsCollection.SupportingDocuments", grid.GetBindingMember());
			}
		}
	}
}
