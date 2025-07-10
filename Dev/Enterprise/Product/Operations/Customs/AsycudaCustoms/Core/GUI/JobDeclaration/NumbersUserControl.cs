using Enterprise.Customs.Common;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AsycudaCustoms.GUI
{
	public partial class NumbersUserControl : Customs.GUI.NumbersUserControl
	{
		public NumbersUserControl()
		{
			InitializeComponent();
			AddColumnToNumbersGrid();
		}

		void AddColumnToNumbersGrid()
		{
			NumbersGrid.ColumnStyles.Add(new ZDateEditColumnStyleInfo()
			{
				ColumnName = CusEntryNumber.Schema.CE_ExpiryDate,
				CaptionResourceString = Enterprise.Customs.AsycudaCustoms.GUI.Res.GetData("287B049E-A579-4F94-8ED8-452733F21201", "Expiry Date"),
				IsVisible = false,
				DateTimeFormat = ZDateTimePickerFormat.Short,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
			});
		}
	}
}
