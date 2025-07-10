using Enterprise.Customs.BE.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BE.GUI.PlugIn;

public partial class InvoiceLineAdditionalInfosUserControlWithGrid : EU.GUI.PlugIn.AdditionalInfosUserControlWithGrid
{
	public InvoiceLineAdditionalInfosUserControlWithGrid()
	{
		InitializeComponent();
	}

	protected override void AdditionalInfosGridColumnsVisible()
	{
		Grid.SetAvailability(false, [AdditionalInfo.Schema.CSI_ReferenceNumber2, AdditionalInfo.Schema.CSI_RX_NKCurrency, AdditionalInfo.Schema.CSI_Value]);
	}

	protected override IPanelLayoutProvider CreateNewInvoiceLineAdditionalInformationDetailsLayout() => new InvoiceLineAdditionalInformationDetailsLayout();
}
