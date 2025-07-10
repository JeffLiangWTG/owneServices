using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI;

public partial class InvoiceHeaderAdditionalInfoUserControlWithGrid : EU.GUI.PlugIn.AdditionalInfosUserControlWithGrid
{
	public InvoiceHeaderAdditionalInfoUserControlWithGrid()
	{
		InitializeComponent();
	}

	protected override IPanelLayoutProvider CreateNewInvoiceHeaderAdditionalInformationDetailsLayout() => new InvoiceHeaderAdditionalInfoDetailsLayout();
}
