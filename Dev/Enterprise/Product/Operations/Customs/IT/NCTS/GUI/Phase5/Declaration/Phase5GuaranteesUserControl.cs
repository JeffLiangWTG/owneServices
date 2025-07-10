using Enterprise.Customs.IT.NCTS.Business;

namespace Enterprise.Customs.IT.NCTS.GUI;

partial class Phase5GuaranteesUserControl : EU.NCTS.GUI.Phase5GuaranteesUserControl
{
	public Phase5GuaranteesUserControl()
	{
		InitializeComponent();
	}

	protected override void AddAndRemoveColumns()
	{
		base.AddAndRemoveColumns();

		var customsOfficeColumnStyleInfo = new ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo
		{
			ColumnName = Business.NctsGuarantee.Schema.PW_BondFiledPort,
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
		};

		var customsOfficeDescriptionColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo
		{
			ColumnName = nameof(NctsGuarantee.OfficeDescription),
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150),
		};

		GuaranteesGrid.ColumnStyles.Add(customsOfficeColumnStyleInfo);
		GuaranteesGrid.ColumnStyles.Add(customsOfficeDescriptionColumnStyleInfo);
	}
}
