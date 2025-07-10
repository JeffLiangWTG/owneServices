using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI
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
				ColumnName = CusEntryInstruction.Schema.CEI_SubStyle,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(68),
			});

			columnStyles.Add(new ZDropEditColumnStyleInfo()
			{
				ColumnName = CusEntryInstruction.Schema.CEI_Style,
				CharacterCasing = System.Windows.Forms.CharacterCasing.Upper,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(105),
			});

			columnStyles.Add(new ZTextBoxColumnStyleInfo()
			{
				ColumnName = CusEntryInstruction.Schema.CEI_Description,
				CharacterCasing = System.Windows.Forms.CharacterCasing.Upper,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(250),
			});

			columnStyles.Add(new ZTextBoxColumnStyleInfo()
			{
				ColumnName = CusEntryInstruction.Schema.CEI_SplitReference,
				CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("56836332-0D4A-433A-A994-6A5D10A032BD", "Split Reference"),
				CharacterCasing = System.Windows.Forms.CharacterCasing.Normal,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(133),
			});

			columnStyles.Add(new ZCalcEditColumnStyleInfo()
			{
				ColumnName = CusEntryInstruction.Schema.CEI_PackageCount,
				CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("11111111-0D4A-433A-A994-6A5D10A042CD", "[UCC 6/18] Package Count"),
				BindToDecimalPlaces = null,
				MaxValue = 0,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200),
			});

			columnStyles.Add(new ZTextBoxColumnStyleInfo()
			{
				ColumnName = CusEntryInstruction.Schema.CurrentLocationFor523,
				CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("11111111-0D4A-433A-A994-6A5D10A032BD", "Location 5/23"),
				CharacterCasing = System.Windows.Forms.CharacterCasing.Upper,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(170),
			});

			columnStyles.Add(new ZTextBoxColumnStyleInfo()
			{
				ColumnName = CusEntryInstruction.Schema.WarehouseIDFor27,
				CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("3791aff7-154e-406b-be0a-2b0e2e5e4881", "Warehouse 2/7"),
				IsVisible = false,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(133),
			});

			columnStyles.Add(new ZCalcEditColumnStyleInfo()
			{
				ColumnName = CusEntryInstruction.Schema.CEI_DisplaySequence,
				BindToDecimalPlaces = null,
				Decimals = 0,
				MaxValue = 0,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
			});

			columnStyles.Add(new ZCheckBoxColumnStyleInfo()
			{
				ColumnName = CusEntryInstruction.Schema.IsPostponedVatViaFiscalReference,
				IsReadOnly = true,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40),
			});
		}
	}
}
