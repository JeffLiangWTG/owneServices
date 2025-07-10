using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.GUI
{
	public class AdditionalInfosUserControlWithGrid : EU.GUI.PlugIn.AdditionalInfosUserControlWithGrid
	{
		public AdditionalInfosUserControlWithGrid()
		{
			AdditionalInfosGroupBox.CaptionResourceString = Enterprise.Customs.IE.GUI.Res.GetData("A54ABAD0-63B3-4C87-967A-1D94E9D9FC0D", "Additional Documents");
		}

		protected override void AdditionalInfosGridColumnsVisible()
		{
			using (Grid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				Grid.SetAvailability(false, [AdditionalInfo.Schema.CSI_ReferenceNumber2, AdditionalInfo.Schema.CSI_RX_NKCurrency, AdditionalInfo.Schema.CSI_Value]);
				Grid.ReOrderColumns(OrderedColumns);
				Grid.SetColumnMandatory(AdditionalInfo.Schema.CSI_Description, false);
			}
		}

		protected override IPanelLayoutProvider CreateNewInvoiceHeaderAdditionalInformationDetailsLayout()
		{
			return new AdditionalInformationDetailsLayout();
		}

		protected override IPanelLayoutProvider CreateNewInvoiceLineAdditionalInformationDetailsLayout()
		{
			return new AdditionalInformationDetailsLayout();
		}

		protected override IPanelLayoutProvider CreateNewExitSummaryAdditionalInformationDetailsLayout()
		{
			return new AdditionalInformationDetailsLayout();
		}

		protected override IPanelLayoutProvider CreateNewClassPartPivotAdditionalInformationDetailsLayout()
		{
			return new AdditionalInformationDetailsLayout();
		}

		string[] OrderedColumns => new[]
		{
			AdditionalInfo.Schema.CSI_SubType,
			AdditionalInfo.Schema.CSI_Code,
			AdditionalInfo.Schema.CSI_ReferenceNumber,
			AdditionalInfo.Schema.CSI_Description
		};
	}
}
