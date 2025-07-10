using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BE.GUI;

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
			ColumnName = "CEI_Style",
			CharacterCasing = System.Windows.Forms.CharacterCasing.Upper,
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(107),
		});

		columnStyles.Add(new ZDropEditColumnStyleInfo()
		{
			ColumnName = "CEI_SubStyle",
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(68),
		});

		columnStyles.Add(new ZTextBoxColumnStyleInfo()
		{
			ColumnName = "CEI_Description",
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(290),
		});

		columnStyles.Add(new ZCheckBoxColumnStyleInfo()
		{
			ColumnName = "ZG_ManualDeclaration",
			CaptionResourceString = Res.GetData("14645CE7-C8BE-4FB9-90FF-61289588ACB0", "Manual Declaration"),
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120),
		});
	}

	protected override void HandleDeclarationControlVisibilityChangedCore()
	{
		base.HandleDeclarationControlVisibilityChangedCore();
		EntryInstructionsGrid.ColumnLayoutContext = (JobDeclaration?.IsExport ?? false) ? nameof(Customs.GUI.DeclarationType.Export) : nameof(Customs.GUI.DeclarationType.Import);
	}

	protected override void ChangeGridColumnsVisibility()
	{
		base.ChangeGridColumnsVisibility();
		var jobDeclaration = (JobDeclaration)JobDeclaration;
		EntryInstructionsGrid.SetAvailability(!((jobDeclaration?.IsReExport ?? true) || (jobDeclaration?.IsExitSummary ?? true)), CusEntryInstruction.Schema.CEI_SubStyle);
	}
}
