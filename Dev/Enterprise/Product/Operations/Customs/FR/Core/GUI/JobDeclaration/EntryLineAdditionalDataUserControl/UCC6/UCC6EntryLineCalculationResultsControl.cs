using Enterprise.Core.Forms;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.FR.GUI
{
	public partial class UCC6EntryLineCalculationResultsControl : ZUserControl
	{
		public UCC6EntryLineCalculationResultsControl()
		{
			InitializeComponent();
			SetupFeesCalculatedByCWGrid();
			SetupFeesConfirmedByCustomsGrid();
		}

		void SetupFeesCalculatedByCWGrid()
		{
			FeesCalculatedByCWGrid.ReadOnly = true;
			FeesCalculatedByCWGrid.ColumnStyles.AddRange(new ZGridColumnInfo[] {
				new ZTextBoxColumnStyleInfo
				{
					CaptionResourceString = Res.GetData("2ff6708d-36ce-44e3-9fab-1c3f176c3711", "Category"),
					ColumnName = nameof(CusEntryLineCalculatedFee.Category),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
				},
				new ZTextBoxColumnStyleInfo
				{
					CaptionResourceString = Res.GetData("2154c8b7-6977-4e2c-a2f8-4433f9d30629", "Code"),
					ColumnName = nameof(CusEntryLineCalculatedFee.ChargeType),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
				},
				new ZCalcEditColumnStyleInfo
				{
					BindToDecimalPlaces = null,
					CaptionResourceString = Res.GetData("9b38ef3a-f0d7-4679-b6e1-56f72729f556", "Amount"),
					ColumnName = nameof(CusEntryLineCalculatedFee.Amount),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120),
				},
				new ZTextBoxColumnStyleInfo
				{
					CaptionResourceString = Res.GetData("b2eaaf0f-389b-4262-a245-4037e183e505", "Currency"),
					ColumnName = nameof(CusEntryLineCalculatedFee.Currency),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
				}
			});
		}

		void SetupFeesConfirmedByCustomsGrid()
		{
			FeesConfirmedByCustomsGrid.ReadOnly = true;
			FeesConfirmedByCustomsGrid.ColumnStyles.AddRange(new ZGridColumnInfo[] {
				new ZTextBoxColumnStyleInfo
				{
					CaptionResourceString = Res.GetData("DEF1E80A-5ADD-4B7A-B620-6482FD427C9F", "Description"),
					ColumnName = nameof(CusEntryLineConfirmedFee.Description),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
				},
				new ZCalcEditColumnStyleInfo
				{
					BindToDecimalPlaces = null,
					CaptionResourceString = Res.GetData("9b38ef3a-f0d7-4679-b6e1-56f72729f556", "Amount"),
					ColumnName = nameof(CusEntryLineConfirmedFee.Amount),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120),
				},
				new ZTextBoxColumnStyleInfo
				{
					CaptionResourceString = Res.GetData("b2eaaf0f-389b-4262-a245-4037e183e505", "Currency"),
					ColumnName = nameof(CusEntryLineConfirmedFee.Currency),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
				}
			});
		}
	}
}
