using Enterprise.Customs.CH.Business;
using Enterprise.Customs.GUI;

namespace Enterprise.Customs.CH.GUI;

public partial class SupportingDocumentsFieldsControl : BaseCustomsEntryUserControl
{
	public SupportingDocumentsFieldsControl()
	{
		InitializeComponent();
	}

	new JobDeclaration JobDeclaration => (JobDeclaration)base.JobDeclaration;

	protected override void ChangeControlsVisibility()
	{
		base.ChangeControlsVisibility();

		if (!SupportingDocumentsGroupBox.IsDisposed)
		{
			CSI_ReferenceNumber2TextBox.Visible = !JobDeclaration?.IsExportOrExportDeclarationActivation ?? true;
			CSI_DateOfIssueDateEdit.Visible = !JobDeclaration?.IsExportOrExportDeclarationActivation ?? true;
		}
	}
}
