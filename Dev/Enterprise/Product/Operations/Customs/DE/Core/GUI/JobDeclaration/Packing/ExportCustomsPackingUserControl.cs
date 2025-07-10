using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DE.GUI
{
	public partial class ExportCustomsPackingUserControl : Customs.GUI.BaseCustomsPackingUserControl
	{
		public ExportCustomsPackingUserControl()
		{
			InitializeComponent();
		}

		protected override void InitializeLayoutPackingDetailsGrid()
		{
			base.InitializeLayoutPackingDetailsGrid();
			PackingDetailsGrid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo
			{
				ColumnName = CusDecHouseContainerPackSchema.Constants.CW_Seal,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(129),
				CharacterCasing = System.Windows.Forms.CharacterCasing.Normal
			});
		}

		protected override void ChangeGridColumnsVisibility()
		{
			base.ChangeGridColumnsVisibility();
			bool showSealColumn = !JobDeclaration.JE_ContainerMode.IsEmpty && !JobDeclaration.IsContainerised;
			PackingDetailsGrid.SetAvailability(showSealColumn, CusDecHouseContainerPackSchema.Constants.CW_Seal);
		}
	}
}
