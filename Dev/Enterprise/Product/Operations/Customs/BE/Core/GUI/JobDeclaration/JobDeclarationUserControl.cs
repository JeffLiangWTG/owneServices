using Enterprise.Customs.EU.GUI;

namespace Enterprise.Customs.BE.GUI;

public partial class JobDeclarationUserControl : EUJobDeclarationUserControl
{
	public JobDeclarationUserControl()
	{
		InitializeComponent();
	}

	protected override void HandleDeclarationControlVisibilityChangedCore()
	{
		base.HandleDeclarationControlVisibilityChangedCore();

		var isImport = JobDeclaration.IsImport;
		IsHighValueOvrdCheckBox.Visible = isImport;

		this.TransportDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(464, 263, true);
		this.ShipmentDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(255, 317, true);
		this.ShipmentDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(464, 405, true);
		this.CustomsOfficesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(255, 728);
	}
}
