using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public partial class DeclarationAdditionalDocumentsGridUserControl : ZUserControl
	{
		public DeclarationAdditionalDocumentsGridUserControl()
		{
			InitializeComponent();
			UpdateGridColumnLayout();
		}

		protected virtual IGridColumnLayoutProvider GetGridColumnLayoutProvider() => new AdditionalDocumentsGridColumnLayout();

		void UpdateGridColumnLayout()
		{
			AdditionalDocumentsGrid.ApplyGridColumnLayout(GetGridColumnLayoutProvider());
		}
	}
}
