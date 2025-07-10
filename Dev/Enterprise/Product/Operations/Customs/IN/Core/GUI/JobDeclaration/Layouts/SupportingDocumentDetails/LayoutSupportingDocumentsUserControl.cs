using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IN.GUI;

public partial class LayoutSupportingDocumentsUserControl : ZUserControl
{
	public LayoutSupportingDocumentsUserControl()
	{
		InitializeComponent();

		DetailsLayoutPanel.UpdateLayout(new SupportingDocumentDetailsLayout());
	}
}
