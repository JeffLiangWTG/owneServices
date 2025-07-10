using Enterprise.Customs.IL.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IL.GUI
{
	public partial class EntryLineTaxAndFeeUserControl : ZUserControl
	{
		public EntryLineTaxAndFeeUserControl()
		{
			InitializeComponent();
			AddLandedCostOnlyColumn();
		}

		void AddLandedCostOnlyColumn()
		{
			EntryLineDutyAndTaxGrid.ColumnStyles.Add(
				new ZTextBoxColumnStyleInfo(CusEntryLineFee.Schema.CF_IsLandedCostOnly,
					CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160))
				{
					CaptionResourceString = Res.GetData("27EBC316-C662-4CC5-B6C9-F27C070D6D11", "Is Landed Cost Only"),
					IsReadOnly = true,
					IsVisible = false
				});
		}
	}
}
