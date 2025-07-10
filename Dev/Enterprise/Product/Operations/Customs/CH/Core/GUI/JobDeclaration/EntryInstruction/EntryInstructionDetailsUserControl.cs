using System;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.GUI;

namespace Enterprise.Customs.CH.GUI;

public partial class EntryInstructionDetailsUserControl : BaseCustomsEntryUserControl
{
	public EntryInstructionDetailsUserControl()
	{
		InitializeComponent();
		ReorderTabPages();
	}

	void ReorderTabPages()
	{
		EntryInstructionTabControl.ReorderTabPages(EntryInstructionDetailsTabPage, PreviousDocumentsTabPage, SupportingDocumentsTabPage, TransportDocumentsTabPage, CusSupplyChainActorsTabPage);
	}

	protected override void OnAfterFirstBinding(EventArgs e)
	{
		base.OnAfterFirstBinding(e);
		DetailsUserControl.UserControlType = typeof(LayoutEntryInstructionDetailBasicUserControl);
	}

	protected override void ChangeGridColumnsVisibility()
	{
		base.ChangeGridColumnsVisibility();

		using (EntryInstructionsGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
		{
			if (JobDeclaration is JobDeclaration declaration)
			{
				EntryInstructionsGrid.GridId = declaration.IsImport ? ImportGridId : ExportGridId;
				EntryInstructionsGrid.SetAvailability(declaration.IsExportOrExportDeclarationActivation, CusEntryInstruction.Schema.CEI_Procedure);
				EntryInstructionsGrid.SetAvailability(declaration.IsImport || declaration.IsExportDeclarationActivation, CusEntryInstruction.Schema.CEI_SubStyle);
				EntryInstructionsGrid.SetAvailability(declaration.IsImport, CusEntryInstruction.Schema.CEI_DeclarationReason);
				EntryInstructionsGrid.SetAvailability(declaration.IsExportOrExportDeclarationActivation, CusEntryInstruction.Schema.CEI_NextProcedure);
				EntryInstructionsGrid.SetAvailability(declaration.IsImport, CusEntryInstruction.Schema.CEI_Description);
				EntryInstructionsGrid.SetAvailability(declaration.IsImport, CusEntryInstruction.Schema.CEI_DateForDuty);
			}
		}
	}

	protected override void ChangeControlsVisibility()
	{
		base.ChangeControlsVisibility();

		if (!EntryInstructionTabControl.IsDisposed)
		{
			PreviousDocumentsTabPage.TabVisible = JobDeclaration?.IsExportOrExportDeclarationActivation ?? false;
			SupportingDocumentsTabPage.TabVisible = JobDeclaration?.IsExportOrExportDeclarationActivation ?? false;
			CusSupplyChainActorsTabPage.TabVisible = JobDeclaration?.IsExportOrExportDeclarationActivation ?? false;
			AdditionalInformationTabPage.TabVisible = JobDeclaration?.IsExportOrExportDeclarationActivation ?? false;
			TransportDocumentsTabPage.TabVisible = JobDeclaration?.IsExportOrExportDeclarationActivation ?? false;
		}
	}

	const string ImportGridId = "25333A1B-5B3C-447F-9622-D6DA7C226C52";
	const string ExportGridId = "FA02B042-94B1-408C-9943-C1D85FBF3071";

	new JobDeclaration JobDeclaration => (JobDeclaration)base.JobDeclaration;
}
