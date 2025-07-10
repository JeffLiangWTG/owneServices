using CargoWise.Windows.UI;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.GUI
{
	public partial class EntryLineTaxAndConfirmedFeeUserControl : EU.GUI.EntryLineTaxAndConfirmedFeeUserControl
	{
		public EntryLineTaxAndConfirmedFeeUserControl()
		{
			InitializeComponent();
			SetEntryLineDutyAndTaxGridColumn();
		}

		void SetEntryLineDutyAndTaxGridColumn()
		{
			var dutyAndTaxGrid = EntryLineCalculatedDutyAndTaxUserControl.EntryLineDutyAndTaxGrid;
			dutyAndTaxGrid.SetColumnVisible(false, nameof(CusEntryLineFee.CF_ChargeType));
			dutyAndTaxGrid.RemoveFromAvailableColumns(nameof(CusEntryLineFee.CF_ChargeType));
			dutyAndTaxGrid.SetColumnVisible(false, nameof(EU.Business.Declaration.CusEntryLineFee.ChargeTypeDescription));
			dutyAndTaxGrid.RemoveFromAvailableColumns(nameof(EU.Business.Declaration.CusEntryLineFee.ChargeTypeDescription));

			dutyAndTaxGrid.ColumnStyles.Insert(1, new ZTextBoxColumnStyleInfo
			{
				ColumnName = nameof(CusEntryLineFee.NationalFeeTypeDescriptionForDisplay),
				Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(150),
			});
			dutyAndTaxGrid.ColumnStyles.Insert(1, new ZDropEditColumnStyleInfo
			{
				ColumnName = nameof(CusEntryLineFee.NationalFeeTypeCode),
				Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
				ShowInDropDown = ZDropEdit.ShowInDropDownList.ShowCodeAndDescription,
			});
		}
	}
}
