using Enterprise.Customs.CH.Business;
using Enterprise.Customs.GUI;

namespace Enterprise.Customs.CH.GUI;

public partial class JobDeclarationUserControl : BaseCustomsDeclarationUserControl
{
	public JobDeclarationUserControl()
	{
		InitializeComponent();
	}

	protected override void HandleDeclarationControlVisibilityChangedCore()
	{
		base.HandleDeclarationControlVisibilityChangedCore();

		VehicleTypeDropEdit.Visible = JobDeclaration.IsImport && JobDeclaration.IsRoad;
		CustomsOfficesGroupBox.Visible = JobDeclaration.IsImport || ((JobDeclaration)JobDeclaration).IsExportDeclarationActivation;
	}
}
