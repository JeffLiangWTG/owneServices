using System.Collections.Immutable;
using System.Linq;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI;

public class Ucc6ExportInvLineAddInfoUserControlWithGrid : EU.GUI.PlugIn.AdditionalInfosUserControlWithGrid
{
	public Ucc6ExportInvLineAddInfoUserControlWithGrid()
	{
		AdditionalInfosGroupBox.CaptionResourceString = Res.GetData("A819B215-AB36-4440-AD76-25FBC677543F", "Additional Documents");
	}

	protected override IPanelLayoutProvider CreateNewInvoiceHeaderAdditionalInformationDetailsLayout()
		=> new Ucc6ExportInvLineAddInfoDetailsLayoutProvider();

	protected override IPanelLayoutProvider CreateNewInvoiceLineAdditionalInformationDetailsLayout()
		=> new Ucc6ExportInvLineAddInfoDetailsLayoutProvider();

	protected override IPanelLayoutProvider CreateNewExitSummaryAdditionalInformationDetailsLayout()
		=> new Ucc6ExportInvLineAddInfoDetailsLayoutProvider();

	protected override void AdditionalInfosGridColumnsVisible()
	{
		using (Grid.SuspendRefreshTableStylesAndRefreshAtDisposal())
		{
			Grid.SetAvailability(false, [AdditionalInfo.Schema.CSI_ReferenceNumber2, AdditionalInfo.Schema.CSI_RX_NKCurrency, AdditionalInfo.Schema.CSI_Value]);
			Grid.ReOrderColumns(gridColumns.ToArray());
			Grid.SetColumnMandatory(AdditionalInfo.Schema.CSI_Description, false);
		}
	}

	ImmutableArray<string> gridColumns => new string[]
	{
		AdditionalInfo.Schema.CSI_SubType,
		AdditionalInfo.Schema.CSI_Code,
		AdditionalInfo.Schema.CSI_ReferenceNumber,
		AdditionalInfo.Schema.CSI_Description
	}.ToImmutableArray();
}
