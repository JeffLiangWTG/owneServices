using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI
{
	internal partial class EntryInstructionGridUserControl : EU.GUI.EntryInstructionGridUserControl
	{
		public EntryInstructionGridUserControl()
		{
			InitializeComponent();
		}

		protected override void ChangeGridColumnsVisibility()
		{
			base.ChangeGridColumnsVisibility();
			EntryInstructionsGrid.SetAvailability(JobDeclaration.IsImport, CusEntryInstruction.Schema.CEI_Style);
		}

		protected override void InitializeGridLayoutCore()
		{
			base.InitializeGridLayoutCore();

			var columnStyles = EntryInstructionsGrid.ColumnStyles;
			columnStyles.Clear();

			columnStyles.Add(new ZDropEditColumnStyleInfo()
			{
				ColumnName = CusEntryInstruction.Schema.CEI_Style,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(96),
			});

			columnStyles.Add(new ZDropEditColumnStyleInfo()
			{
				ColumnName = CusEntryInstruction.Schema.CEI_SubStyle,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(96),
			});

			columnStyles.Add(new ZTextBoxColumnStyleInfo()
			{
				ColumnName = CusEntryInstruction.Schema.CEI_Description,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(290),
			});
		}
	}
}
