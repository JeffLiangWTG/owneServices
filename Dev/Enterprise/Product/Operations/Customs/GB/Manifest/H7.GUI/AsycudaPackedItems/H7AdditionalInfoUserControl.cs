using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.Business;
using Enterprise.Customs.GB.Business.Declaration.MultiLineAddInfos;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.H7.GUI
{
	public partial class H7AdditionalInfoUserControl : EU.GUI.PlugIn.AdditionalInfosUserControl, IAdditionalTabPage
	{
		public H7AdditionalInfoUserControl() : base()
		{
			InitializeComponent();
			AdditionalInfosGroupBox.CaptionResourceString = Res.GetData("d7bc3903-af24-4784-8b85-ee7db87f6ec0", "Additional Info");
			new ControlRebinder().Rebind(this, "FilteredInvoiceLines.AdditionalInfos", "AdditionalDocuments");
		}

		protected override void ChangeGridColumnsVisibility()
		{
			using (AdditionalInfosGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				AdditionalInfosGrid.SetAvailability(false, [AdditionalInfo.Schema.CSI_NctsExportFromEC, AutoCusSupportingInfo.Schema.CSI_Status, AutoCusSupportingInfo.Schema.CSI_SubType, AutoCusSupportingInfo.Schema.CSI_RN_NKCountryCode, AutoCusSupportingInfo.Schema.CSI_Value]);
				AdditionalInfosGrid.ReOrderColumns(OrderedColumns);
				AdditionalInfosGrid.SetColumnMandatory(AutoCusSupportingInfo.Schema.CSI_Description, isMandatory: false);
			}
		}

		public string[] OrderedColumns => new[]
		{
			AutoCusSupportingInfo.Schema.CSI_Code,
			AutoCusSupportingInfo.Schema.CSI_Description,
		};

		#region IAdditionalTabPage Implementation

		ZUserControl IAdditionalTabPage.AdditionalTabPageUserControl => this;

		ResourceStringData IAdditionalTabPage.AdditionalTabPageCaption => Res.GetData("2cb05bef-6e18-46e1-a168-01b2899bfaf7", "Additional Info");

		AdditionalTabPageVisibility IAdditionalTabPage.AdditionalControlVisibility => new AdditionalTabPageVisibility(h => true, null);

		int IAdditionalTabPage.TabPageSequence => 21;

		#endregion
	}
}
