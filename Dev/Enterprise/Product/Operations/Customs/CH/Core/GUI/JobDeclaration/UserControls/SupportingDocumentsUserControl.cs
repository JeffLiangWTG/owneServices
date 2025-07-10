using Enterprise.Customs.CH.Business;
using Enterprise.Customs.GUI;

namespace Enterprise.Customs.CH.GUI;

public partial class SupportingDocumentsUserControl : BaseCustomsEntryUserControl
{
	public SupportingDocumentsUserControl()
	{
		InitializeComponent();
	}

	new JobDeclaration JobDeclaration => (JobDeclaration)base.JobDeclaration;

	protected override void ChangeGridColumnsVisibility()
	{
		base.ChangeGridColumnsVisibility();

		using (SupportingDocumentsGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
		{
			SupportingDocumentsGrid.SetAvailability(!JobDeclaration?.IsExportOrExportDeclarationActivation ?? true, SupportingDocument.Schema.CSI_ReferenceNumber2);
			SupportingDocumentsGrid.SetAvailability(!JobDeclaration?.IsExportOrExportDeclarationActivation ?? true, SupportingDocument.Schema.CSI_DateOfIssue);
		}
	}
}
