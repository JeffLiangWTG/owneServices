using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI;

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
			ColumnName = CusEntryInstruction.Schema.CEI_Style,
			CharacterCasing = System.Windows.Forms.CharacterCasing.Upper,
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(105),
		});

		columnStyles.Add(new ZDropEditColumnStyleInfo()
		{
			ColumnName = CusEntryInstruction.Schema.CEI_SubStyle,
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(68),
		});

		columnStyles.Add(new ZTextBoxColumnStyleInfo()
		{
			ColumnName = CusEntryInstruction.Schema.CEI_Description,
			CharacterCasing = System.Windows.Forms.CharacterCasing.Upper,
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(250),
		});

		columnStyles.Add(new ZDateEditColumnStyleInfo()
		{
			ColumnName = CusEntryInstruction.Schema.CEI_DateForDuty,
			DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Short,
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(105),
		});

		columnStyles.Add(new ZTextBoxColumnStyleInfo()
		{
			ColumnName = CusEntryInstruction.Schema.WarehouseIDFor27,
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(145),
		});

		columnStyles.Add(new ZTextBoxColumnStyleInfo()
		{
			ColumnName = CusEntryInstruction.Schema.FromWarehouseCode,
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(145),
		});

		columnStyles.Add(new ZDropEditColumnStyleInfo()
		{
			ColumnName = CusEntryInstruction.Schema.CEI_Procedure,
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
		});

		columnStyles.Add(new ZDateEditColumnStyleInfo()
		{
			ColumnName = CusEntryInstruction.Schema.ZG_TempProcLimitDate,
			DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Short,
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(155),
		});

		columnStyles.Add(new ZDropEditColumnStyleInfo()
		{
			ColumnName = CusEntryInstruction.Schema.ZG_ParticipantType,
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
		});

		columnStyles.Add(new ZTextBoxColumnStyleInfo()
		{
			ColumnName = CusEntryInstruction.Schema.Incoterm,
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70),
		});

		columnStyles.Add(new ZTextBoxColumnStyleInfo()
		{
			ColumnName = CusEntryInstruction.Schema.ValuationCode,
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70),
		});

		columnStyles.Add(new ZTextBoxColumnStyleInfo()
		{
			ColumnName = CusEntryInstruction.Schema.Currency,
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70),
		});
	}

	protected override void HandleDeclarationControlVisibilityChangedCore()
	{
		base.HandleDeclarationControlVisibilityChangedCore();
		EntryInstructionsGrid.SetAvailability(IsParticipantTypeVisible(), CusEntryInstruction.Schema.ZG_ParticipantType);
	}

	protected override bool ShowRequestedProcedure => true;

	#region Implementation

	JobDeclaration Declaration => (JobDeclaration)CurrentDataItem;

	bool IsParticipantTypeVisible()
	{
		var jobDeclaration = Declaration;
		if (jobDeclaration == null)
		{
			return false;
		}

		return jobDeclaration.IsExport && !jobDeclaration.IsUCC6AndIsExport;
	}

	#endregion
}
