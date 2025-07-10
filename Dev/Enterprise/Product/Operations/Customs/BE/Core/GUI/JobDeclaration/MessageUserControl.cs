using Enterprise.ZArchitecture;

namespace Enterprise.Customs.BE.GUI;

public partial class MessageUserControl : EU.GUI.MessageUserControl
{
	public MessageUserControl()
	{
		InitializeComponent();
	}

	protected override void InitializeGridLayoutCore()
	{
		base.InitializeGridLayoutCore();

		ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new ZCheckBoxColumnStyleInfo();
		zCheckBoxColumnStyleInfo1.ColumnName = "ZG_ManualDeclaration";
		zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
		zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.BE.GUI.Res.GetData("14645CE7-C8BE-4FB9-90FF-61289588ACB0", "Manual Declaration");
		zCheckBoxColumnStyleInfo1.IsReadOnly = true;
		EntriesBoundGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
	}

	protected override EU.GUI.EntryLineAdditionalDataUserControl GetEntryLineAdditionalData() => new EntryLineAdditionalDataUserControl();
}
