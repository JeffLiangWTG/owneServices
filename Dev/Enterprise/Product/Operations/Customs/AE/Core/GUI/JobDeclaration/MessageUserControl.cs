using Enterprise.Customs.AE.Business;
using Enterprise.Customs.GUI;

namespace Enterprise.Customs.AE.GUI;

public partial class MessageUserControl : ImportMessageUserControl
{
	public MessageUserControl()
	{
		InitializeComponent();
		SetupEntryHeaderColumns();
	}

	void SetupEntryHeaderColumns()
	{
		EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZCalcEditColumnStyleInfo
		{
			BindToDecimalPlaces = null,
			CaptionResourceString = Res.GetData("97AF60C6-EAFF-450C-B003-7A726C2B5BA9", "Total Paid"),
			ColumnName = CusEntryHeader.Schema.CH_TotalPaid,
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(75)
		});
	}
}
