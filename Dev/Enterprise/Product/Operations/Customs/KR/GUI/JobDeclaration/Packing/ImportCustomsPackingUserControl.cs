using System.Windows.Forms;
using Enterprise.Customs.GUI;
using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public partial class ImportCustomsPackingUserControl : BaseCustomsPackingUserControl
	{
		public ImportCustomsPackingUserControl()
		{
			InitializeComponent();
			AddColumns();
			ReOrderColumns();
		}

		void AddColumns()
		{
			HouseBillsGrid.ColumnStyles.AddRange(new ZTextBoxColumnStyleInfo[]
			{
				new ZDropEditColumnStyleInfo
				{
					CharacterCasing = CharacterCasing.Upper,
					ColumnName = Bill.Schema.CU_HBSplitDecInd,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120)
				},
				new ZDropEditColumnStyleInfo
				{
					CharacterCasing = CharacterCasing.Upper,
					ColumnName = Bill.Schema.CU_HBSplitDecReasonCode,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140)
				},
				new ZTextBoxColumnStyleInfo
				{
					ColumnName = nameof(Bill.HBSplitDecReasonRemark),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(500)
				},
			});
		}
		void ReOrderColumns()
		{
			HouseBillsGrid.ReOrderColumnsAndChangeVisibility(listColumns);
		}
		readonly string[] listColumns =
		{
			Bill.Schema.CU_BillType,
			Bill.Schema.CU_BillNum,
			Bill.Schema.CU_IssueDate,
			Bill.Schema.CU_ParentBillUniqueCode,
			Bill.Schema.CU_HBSplitDecInd,
			Bill.Schema.CU_HBSplitDecReasonCode,
			nameof(Bill.HBSplitDecReasonRemark),
		};
	}
}
