using System;
using Enterprise.Customs.GUI;
using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.KR.GUI
{
	public partial class EntryInstructionDetailsUserControl : BaseEntryInstructionDetailsUserControl
	{
		public EntryInstructionDetailsUserControl()
		{
			InitializeComponent();
			UpdateLayouts();
			EditControls();
			EditColumns();
			ReOrderColumns();
		}
		void UpdateLayouts()
		{
			DynamicMisc929DetailsItemLayoutPanel.UpdateLayout(new Misc929DetailsItemLayout());
			DynamicBondedFactoryLayoutPanel.UpdateLayout(new BondedFactoryLayout());
		}
		void EditControls()
		{
			AssessmentDateEdit.CaptionResourceString = null;
			AssessmentDateEdit.ReadOnly = true;
		}

		void EditColumns()
		{
			EntryInstructionsGrid.ColumnStyles.Remove(EntryInstructionsGrid.GetColumnStyle(CusEntryInstruction.Schema.CEI_Style));
			EntryInstructionsGrid.ColumnStyles.Remove(EntryInstructionsGrid.GetColumnStyle(CusEntryInstruction.Schema.CEI_DateForDuty));

			EntryInstructionsGrid.ColumnStyles.AddRange(new ZTextBoxColumnStyleInfo[] {
				new ZTextBoxColumnStyleInfo
				{
					CharacterCasing = System.Windows.Forms.CharacterCasing.Upper,
					ColumnName = CusEntryInstruction.Schema.CEI_Style,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
				},
				new ZDateEditColumnStyleInfo
				{
					ColumnName = nameof(CusEntryInstruction.AcceptedDate),
					DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Short,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120),
				}
			});
		}
		void ReOrderColumns()
		{
			EntryInstructionsGrid.ReOrderColumnsAndChangeVisibility(InstructionsColumnOrder);
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			DynamicFTADetailsLayoutPanel.UpdateLayout(new FTADetailsLayout());
		}

		public static string[] InstructionsColumnOrder => new string[]
		{
				CusEntryInstruction.Schema.CEI_Style,
				CusEntryInstruction.Schema.CEI_Description,
				nameof(CusEntryInstruction.AcceptedDate)
		};
	}
}
