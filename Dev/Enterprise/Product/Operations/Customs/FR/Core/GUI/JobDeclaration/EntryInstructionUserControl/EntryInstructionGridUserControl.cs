using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.FR.GUI
{
	public partial class EntryInstructionGridUserControl : EU.GUI.EntryInstructionGridUserControl
	{
		public EntryInstructionGridUserControl()
		{
			InitializeComponent();
		}

		protected override void InitializeGridLayoutCore()
		{
			base.InitializeGridLayoutCore();

			var columnStyles = EntryInstructionsGrid.ColumnStyles;
			columnStyles.Clear();

			columnStyles.Add(new ZDropEditColumnStyleInfo()
			{
				ColumnName = "CEI_SubStyle",
				IsCustomColumn = false,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(68),
			});

			columnStyles.Add(new ZTextBoxColumnStyleInfo()
			{
				ColumnName = "WarehouseIDFor27",
				CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("9F18CBDC-3351-4691-BC25-DEB4F2267C53", "Warehouse 2/7"),
				IsCustomColumn = false,
				IsVisible = false,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(133),
			});

			columnStyles.Add(new ZDropEditColumnStyleInfo()
			{
				ColumnName = "CEI_Style",
				CharacterCasing = System.Windows.Forms.CharacterCasing.Upper,
				IsCustomColumn = false,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(105),
			});

			columnStyles.Add(new ZTextBoxColumnStyleInfo()
			{
				ColumnName = "CEI_Description",
				CharacterCasing = System.Windows.Forms.CharacterCasing.Upper,
				IsCustomColumn = false,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(250),
			});

			columnStyles.Add(new ZDateEditColumnStyleInfo()
			{
				ColumnName = "CEI_DateForDuty",
				CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("B8E5F4FF-8E65-41E0-8670-AB324AED70C4", "Assessment Date"),
				IsCustomColumn = false,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(106),
			});

			columnStyles.Add(new ZDropEditColumnStyleInfo()
			{
				ColumnName = "CEI_Procedure",
				IsCustomColumn = false,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60),
			});

			columnStyles.Add(new ZDropEditColumnStyleInfo()
			{
				ColumnName = "ZG_TransNature",
				CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("D2FE604F-8396-4D14-B8F6-3905E43D4394", "[24] Tran. Nature"),
				IsCustomColumn = false,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110),
			});

			columnStyles.Add(new ZCalcEditColumnStyleInfo()
			{
				ColumnName = "CEI_TotalInnerPackages",
				IsCustomColumn = false,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90),
			});
		}

		protected override bool ShowRequestedProcedure => true;
	}
}
