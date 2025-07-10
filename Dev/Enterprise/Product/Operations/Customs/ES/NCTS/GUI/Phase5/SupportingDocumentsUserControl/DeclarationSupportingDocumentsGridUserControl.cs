using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.NCTS.GUI
{
	public partial class DeclarationSupportingDocumentsGridUserControl : EU.NCTS.GUI.DeclarationSupportingDocumentsGridUserControl
	{
		public DeclarationSupportingDocumentsGridUserControl()
		{
			InitializeComponent();
		}

		protected override IGridColumnLayoutProvider GetGridColumnLayoutProvider() => new SupportingDocumentsGridColumnLayout();
	}
}
