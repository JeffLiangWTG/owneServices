using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
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
					IsReadOnly = true,
					IsVisible = false
				});
		}
	}
}
