using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.NCTS.GUI
{
	public partial class DeclarationAdditionalDocumentsGridUserControl : EU.NCTS.GUI.DeclarationAdditionalDocumentsGridUserControl
	{
		public DeclarationAdditionalDocumentsGridUserControl()
		{
			InitializeComponent();
		}

		protected override IGridColumnLayoutProvider GetGridColumnLayoutProvider() => new AdditionalDocumentsGridColumnLayout();
	}
}
