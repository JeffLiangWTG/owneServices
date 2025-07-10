using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.GUI
{
	class ImportInvoiceLineLayoutPreviousDocumentsUserControl : EU.GUI.PlugIn.LayoutPreviousDocumentsUserControl
	{
		protected override IGridColumnLayoutProvider GetGridColumnLayoutProvider() => new ImportInvoiceLinePreviousDocumentGridColumnLayout();
	}
}
