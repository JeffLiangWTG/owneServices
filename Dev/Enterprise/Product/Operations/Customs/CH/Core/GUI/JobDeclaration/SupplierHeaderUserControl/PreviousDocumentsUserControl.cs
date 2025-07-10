using Enterprise.Customs.CH.Business;
using Enterprise.Customs.GUI;

namespace Enterprise.Customs.CH.GUI.PlugIn;

public partial class PreviousDocumentsUserControl : BaseCustomsEntryUserControl
{
	public PreviousDocumentsUserControl()
	{
		InitializeComponent();
	}

	new JobDeclaration JobDeclaration => (JobDeclaration)base.JobDeclaration;

	protected override void ChangeGridColumnsVisibility()
	{
		base.ChangeGridColumnsVisibility();

		using (PrevDocsGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
		{
			PrevDocsGrid.SetAvailability(!JobDeclaration?.IsExportOrExportDeclarationActivation ?? true, PreviousDocument.Schema.CSI_Description);
		}
	}

	protected override void ChangeControlsVisibility()
	{
		base.ChangeControlsVisibility();

		if (!PrevDocsGroupBox.IsDisposed)
		{
			PrevDocsAdditionalInformationTextBox.Visible = !JobDeclaration?.IsExportOrExportDeclarationActivation ?? true;
		}
	}
}
