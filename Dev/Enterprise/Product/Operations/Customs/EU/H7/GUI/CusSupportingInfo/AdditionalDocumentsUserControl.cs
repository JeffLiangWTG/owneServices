using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.H7.GUI
{
	public class AdditionalDocumentsUserControl : AdditionalInfosUserControlWithGrid, IAdditionalTabPage
	{
		public AdditionalDocumentsUserControl()
		{
			AdditionalInfosGroupBox.CaptionResourceString = Res.GetData("9aa20981-26fc-486f-b173-1929c8eb5f14", "Additional Documents");
			new ControlRebinder().Rebind(this, "FilteredInvoiceLines.AdditionalInfos", "AdditionalDocuments");
		}

		protected override void AdditionalInfosGridColumnsVisible()
		{
			using (base.Grid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				base.Grid.SetAvailability(false, ["CSI_ReferenceNumber2", "CSI_RX_NKCurrency", "CSI_Value"]);
				base.Grid.ReOrderColumns(new [] { "CSI_SubType", "CSI_Code", "CSI_ReferenceNumber", "CSI_Description" });
				base.Grid.SetColumnMandatory("CSI_Description", isMandatory: false);
			}
		}

		protected override void SetAdditionalInfosLayout()
		{
			DetailsLayoutControl.SetLayout(new EUH7AdditionalDocumentsDetailsLayout());
		}

		ZUserControl IAdditionalTabPage.AdditionalTabPageUserControl => this;

		ResourceStringData IAdditionalTabPage.AdditionalTabPageCaption => Res.GetData("9aa20981-26fc-486f-b173-1929c8eb5f14", "Additional Documents");

		AdditionalTabPageVisibility IAdditionalTabPage.AdditionalControlVisibility => new AdditionalTabPageVisibility(h => true, null);

		int IAdditionalTabPage.TabPageSequence => 21;
	}
}
