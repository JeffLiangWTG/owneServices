using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI;

sealed partial class EntryDetailsUserControl : ZUserControl
{
	public EntryDetailsUserControl()
	{
		InitializeComponent();
		SetupA93GridColumns();
	}

	void SetupA93GridColumns()
	{
		this.A93Grid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
		{
			ColumnName = CusEntryPayInfo.Schema.C9_TransactionType,
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50),
			CaptionResourceString = Res.GetData("376D7665-82F3-4D96-BAAB-E2BDE75F908B", "Registry"),
		});
		this.A93Grid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
		{
			ColumnName = CusEntryPayInfo.Schema.A93Number,
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50),
			CaptionResourceString = Res.GetData("9C9803F0-FCF8-4BFB-9DB8-1E13EE4BCA5E", "Number"),
		});
		this.A93Grid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
		{
			ColumnName = CusEntryPayInfo.Schema.MethodOfPayment,
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
			CaptionResourceString = Res.GetData("D69DAE8F-00EA-49B3-A4AF-3216D0A9AEB5", "Payment Type"),
		});
		this.A93Grid.ColumnStyles.Add(new ZArchitecture.ZCalcEditColumnStyleInfo
		{
			ColumnName = CusEntryPayInfo.Schema.C9_PaymentAmount,
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60),
			CaptionResourceString = Res.GetData("37163DAA-041C-4420-91FA-AD8B12F1956A", "Amount"),
			Decimals = 2,
		});
		this.A93Grid.ColumnStyles.Add(new ZArchitecture.ZDateEditColumnStyleInfo
		{
			ColumnName = CusEntryPayInfo.Schema.C9_PaymentDate,
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70),
			CaptionResourceString = Res.GetData("476FC1E3-E268-4C50-879E-81EFE264E5F4", "Expiry Date"),
			DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Short,
		});
	}
}
