using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public partial class DeclarationSupportingDocumentsGridUserControl : ZUserControl
	{
		public DeclarationSupportingDocumentsGridUserControl()
		{
			InitializeComponent();
			UpdateGridColumnLayout();
		}

		protected virtual IGridColumnLayoutProvider GetGridColumnLayoutProvider() => new SupportingDocumentsGridColumnLayout();

		void UpdateGridColumnLayout()
		{
			SupportingDocumentsGrid.ApplyGridColumnLayout(GetGridColumnLayoutProvider());
		}
	}
}
