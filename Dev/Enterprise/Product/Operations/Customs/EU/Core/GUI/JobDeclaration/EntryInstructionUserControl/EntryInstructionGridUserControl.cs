using System;
using System.Linq;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public partial class EntryInstructionGridUserControl : BaseCustomsEntryUserControl
	{
		public EntryInstructionGridUserControl()
		{
			InitializeComponent();
			InitializeGridLayout();
		}

		JobDeclaration Declaration => (JobDeclaration)base.JobDeclaration;

		protected override void InitializeGridLayoutCore()
		{
			base.InitializeGridLayoutCore();
			var columnStyles = EntryInstructionsGrid.ColumnStyles;
			columnStyles.Add(new ZCalcEditColumnStyleInfo()
			{
				ColumnName = CusEntryInstruction.Schema.CEI_TotalInnerPackages,
				IsCustomColumn = false,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90),
			});
			columnStyles.Add(new ZDropEditColumnStyleInfo()
			{
				ColumnName = CusEntryInstruction.Schema.CEI_Procedure,
				CharacterCasing = System.Windows.Forms.CharacterCasing.Normal,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90),
				IsVisible = ShowRequestedProcedure,
				GroupName = RequestedProcedureGroupName
			});
			columnStyles.Add(new ZTextBoxColumnStyleInfo()
			{
				ColumnName = CusEntryInstruction.Schema.ProcedureDescription,
				CharacterCasing = System.Windows.Forms.CharacterCasing.Normal,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200),
				IsVisible = ShowRequestedProcedure,
				GroupName = RequestedProcedureGroupName
			});
		}

		protected override void ChangeGridColumnsVisibility()
		{
			base.ChangeGridColumnsVisibility();
			if (EntryInstructionsGrid.ColumnStyles.OfType<ZDropEditColumnStyleInfo>().Any(x => x.ColumnName.Equals(CusEntryInstruction.Schema.CEI_Procedure)))
			{
				EntryInstructionsGrid.SetAvailability(ShowRequestedProcedure, CusEntryInstruction.Schema.CEI_Procedure);
				EntryInstructionsGrid.SetColumnVisible(ShowRequestedProcedure, CusEntryInstruction.Schema.CEI_Procedure);
			}
			if (EntryInstructionsGrid.ColumnStyles.OfType<ZTextBoxColumnStyleInfo>().Any(x => x.ColumnName.Equals(CusEntryInstruction.Schema.ProcedureDescription)))
			{
				EntryInstructionsGrid.SetAvailability(ShowRequestedProcedure, CusEntryInstruction.Schema.ProcedureDescription);
				EntryInstructionsGrid.SetColumnVisible(ShowRequestedProcedure, CusEntryInstruction.Schema.ProcedureDescription);
			}
		}

		protected virtual bool ShowRequestedProcedure => Declaration?.IsRequestedProcedureEnable ?? false;

		ResourceStringData RequestedProcedureGroupName => Res.GetData("F77684AA-9C71-4D81-A9A3-512CD3865D15", "Requested Procedure");

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				EntryInstructionsGrid.AfterBind -= EntryInstructionsGrid_AfterBind;
				if (EntryInstructionsGrid.ListManager != null)
				{
					EntryInstructionsGrid.ListManager.CurrentChanged -= ListManager_CurrentChanged;
				}
			}
			base.Dispose(disposing);
		}

		public event EventHandler<EntryInstructionChangingEvent> OnEntryInstructionChanging;
		public event EventHandler<EventArgs> OnEntryInstructionChanged;

		void EntryInstructionsGrid_AfterBind(object sender, EventArgs e)
		{
			var listManager = EntryInstructionsGrid.ListManager;
			if (listManager != null)
			{
				listManager.CurrentChanged += ListManager_CurrentChanged;
				ListManager_CurrentChanged(this, EventArgs.Empty);
			}
		}

		void ListManager_CurrentChanged(object sender, EventArgs e)
		{
			var listManager = EntryInstructionsGrid.ListManager;
			if (listManager != null)
			{
				if (listManager.Count > 0)
				{
					var currentEntryFromListManager = (CusEntryInstruction)listManager.GetCurrent();
					if (currentEntryFromListManager != null && CurrentEntryInstruction != currentEntryFromListManager)
					{
						OnEntryInstructionChanging?.Invoke(this, new EntryInstructionChangingEvent(CurrentEntryInstruction, currentEntryFromListManager));
						CurrentEntryInstruction = currentEntryFromListManager;
						OnEntryInstructionChanged?.Invoke(this, EventArgs.Empty);
					}
				}
				else
				{
					OnEntryInstructionChanging?.Invoke(this, new EntryInstructionChangingEvent(CurrentEntryInstruction, null));
					CurrentEntryInstruction = null;
					OnEntryInstructionChanged?.Invoke(this, EventArgs.Empty);
				}
			}
		}

		public CusEntryInstruction CurrentEntryInstruction { get; private set; }
	}
}
