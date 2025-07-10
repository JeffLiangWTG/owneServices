using System;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI
{
	public partial class EntryInstructionDetailsUserControl : BaseCustomsEntryUserControl
	{
		public EntryInstructionDetailsUserControl()
		{
			InitializeComponent();
			InitializeGridLayout();
		}

		void EntryInstructionsGridOnAfterBind(object sender, EventArgs eventArgs)
		{
			EntryInstructionsGrid.ListManager.PositionChanged += ListManagerOnPositionChanged;
			ListManagerOnPositionChanged(null, null);
			MasterCSI_Procedure_ValueChanged(null, null);
		}

		CusEntryInstruction CurrentEntryInstruction => (CusEntryInstruction)EntryInstructionsGrid.ListManager?.GetCurrent();

		void CurrentEntryInstructionChanged(object sender, EventArgs eventArgs)
		{
			ChangeControlsVisibilityForCurrentEntryInstruction();
		}

		void ListManagerOnPositionChanged(object sender, EventArgs eventArgs)
		{
			UnHookCurrentEntryInstructionEvents();
			HookCurrentEntryInstructionEvents();
			CurrentEntryInstructionChanged(null, null);
		}

		void UnHookCurrentEntryInstructionEvents()
		{
			var currentEntryInstruction = CurrentEntryInstruction;
			if (currentEntryInstruction != null)
			{
				currentEntryInstruction.CEI_StyleInfo.ValueChanged -= CurrentEntryInstructionChanged;
				currentEntryInstruction.CEI_SubStyleInfo.ValueChanged -= CurrentEntryInstructionChanged;
				currentEntryInstruction.CEI_SimplifiedGrantAuthorizationInfo.ValueChanged -= CurrentEntryInstructionChanged;
				currentEntryInstruction.PreviousDocumentMaster.CSI_Procedure_ValueChanged -= MasterCSI_Procedure_ValueChanged;
			}
		}

		void HookCurrentEntryInstructionEvents()
		{
			var currentEntryInstruction = CurrentEntryInstruction;
			if (currentEntryInstruction != null)
			{
				currentEntryInstruction.CEI_StyleInfo.ValueChanged += CurrentEntryInstructionChanged;
				currentEntryInstruction.CEI_SubStyleInfo.ValueChanged += CurrentEntryInstructionChanged;
				currentEntryInstruction.CEI_SimplifiedGrantAuthorizationInfo.ValueChanged += CurrentEntryInstructionChanged;
				currentEntryInstruction.PreviousDocumentMaster.CSI_Procedure_ValueChanged += MasterCSI_Procedure_ValueChanged;
			}
		}

		void MasterCSI_Procedure_ValueChanged(object sender, EventArgs e)
		{
			var currentEntryInstruction = CurrentEntryInstruction;
			if (currentEntryInstruction != null)
			{
				PreviousDocumentsUserControl.SetPreviousDocumentsGridColumnsVisible(currentEntryInstruction.PreviousDocumentMaster.CSI_Procedure);
			}
		}

		protected override void Dispose(bool disposing)
		{
			EntryInstructionsGrid.AfterBind -= CurrentEntryInstructionChanged;
			UnHookCurrentEntryInstructionEvents();

			if (disposing && (components != null))
			{
				components.Dispose();
			}

			base.Dispose(disposing);
		}

		protected override void InitializeGridLayoutCore()
		{
			base.InitializeGridLayoutCore();
			EntryInstructionsGrid.ReOrderColumns(Columns);
		}

		protected override void ChangeGridColumnsVisibility()
		{
			var isExport = JobDeclaration.IsExport;
			if (isExport)
			{
				EntryInstructionsGrid.SetColumnCaption(CusEntryInstruction.Schema.CEI_Style, EntryInstructionDetailBasicUserControl.ExportStyleCaption.Caption);
				EntryInstructionsGrid.SetColumnCaption(CusEntryInstruction.Schema.CEI_SubStyle, EntryInstructionDetailBasicUserControl.ExportSubStyleCaption.Caption);
			}
			else
			{
				EntryInstructionsGrid.SetColumnCaption(CusEntryInstruction.Schema.CEI_Style, EntryInstructionDetailBasicUserControl.DefaultStyleCaption.Caption);
				EntryInstructionsGrid.SetColumnCaption(CusEntryInstruction.Schema.CEI_SubStyle, EntryInstructionDetailBasicUserControl.DefaultSubStyleCaption.Caption);
			}
			EntryInstructionsGrid.SetAvailability(isExport, [AutoCusEntryInstruction.Schema.ZG_PartyConstellation, CusEntryInstruction.Schema.CEI_DateForDuty, AutoCusEntryInstruction.Schema.ZG_ExitDate]);
			EntryInstructionsGrid.SetAvailability(!isExport, [CusEntryInstruction.Schema.CEI_Procedure, CusEntryInstruction.Schema.CEI_LocalClearanceDate, CusEntryInstruction.Schema.CEI_EarlyClearanceFlag]);
			EntryInstructionsGrid.ReOrderColumns(Columns);
		}

		protected override void ChangeControlsVisibility()
		{
			base.ChangeControlsVisibility();

			ChangeTabsVisibilityForDeclaration();
			BasicUserControl?.ChangeControlsVisibility();
		}

		void ChangeTabsVisibilityForDeclaration()
		{
			var declaration = JobDeclaration;
			var isImport = declaration?.IsImport ?? false;

			SupplyChainActorReferencesTabPage.TabVisible = declaration?.IsExport ?? false;

			ChangeTabsVisibilityForDeclarationAndCurrentEntryInstruction(CurrentEntryInstruction);
		}

		void ChangeControlsVisibilityForCurrentEntryInstruction()
		{
			var currentEntryInstruction = CurrentEntryInstruction;
			ChangeTabsVisibilityForDeclarationAndCurrentEntryInstruction(currentEntryInstruction);

			InwardProcessingUserControl.ChangeControlsVisibility(currentEntryInstruction);
		}

		void ChangeTabsVisibilityForDeclarationAndCurrentEntryInstruction(CusEntryInstruction currentEntryInstruction)
		{
			InwardProcessingTabPage.TabVisible = currentEntryInstruction?.EnabledInwardProcessing ?? false;
			OutwardProcessingTabPage.TabVisible = currentEntryInstruction?.EnabledOutwardProcessing ?? false;

			var declaration = JobDeclaration;
			var isImport = declaration?.IsImport ?? false;
			var isInwardProcessingAVABR = currentEntryInstruction != null && currentEntryInstruction.CEI_Style == Business.ImportDeclarationTypeList.Codes.AVABR;
			PreviousDocumentsTabPage.TabVisible = !isInwardProcessingAVABR && isImport;
			DV1DetailsTabPage.TabVisible = !isInwardProcessingAVABR && isImport;
			FiscalReferencesTabPage.TabVisible = !isInwardProcessingAVABR && isImport;
			AuthorizationsTabPage.TabVisible = !isInwardProcessingAVABR;
		}

		string[] Columns => (JobDeclaration?.IsExport ?? false) ? columnsForExportDeclaration : columnsForImportDeclaration;

		readonly string[] columnsForExportDeclaration =
		{
			CusEntryInstruction.Schema.CEI_SubStyle,
			CusEntryInstruction.Schema.CEI_Style,
			AutoCusEntryInstruction.Schema.ZG_PartyConstellation,
			CusEntryInstruction.Schema.CEI_Description,
			CusEntryInstruction.Schema.CEI_DateForDuty,
			AutoCusEntryInstruction.Schema.ZG_ExitDate
		};

		readonly string[] columnsForImportDeclaration =
		{
			CusEntryInstruction.Schema.CEI_Style,
			CusEntryInstruction.Schema.CEI_SubStyle,
			CusEntryInstruction.Schema.CEI_Procedure,
			CusEntryInstruction.Schema.CEI_Description,
			CusEntryInstruction.Schema.CEI_LocalClearanceDate,
			CusEntryInstruction.Schema.CEI_EarlyClearanceFlag
		};
	}
}
