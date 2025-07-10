using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.GUI
{
	public partial class DocumentSendingSupportingDocumentUserControl : ZUserControl
	{
		public DocumentSendingSupportingDocumentUserControl()
		{
			InitializeComponent();
			InitializeGridColumns();
		}

		internal IGridColumnLayoutProvider GridColumnLayout => new DocumentSendingSupportingDocumentGridColumnLayout();

		void InitializeGridColumns() => SupportingDocumentsGrid.ApplyGridColumnLayout(GridColumnLayout);
	}
}
