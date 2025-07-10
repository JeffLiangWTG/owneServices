using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.GUI.PlugIn;

namespace Enterprise.Customs.BE.GUI.PlugIn;

public partial class EntryInstructionSupportingDocumentsUserControl : InvoiceLayoutSupportingDocumentsUserControl
{
	public EntryInstructionSupportingDocumentsUserControl()
	{
		InitializeComponent();
	}

	protected override LayoutSupportingDocumentsFieldsControl GetSupportingDocumentsFieldsControl(JobDeclaration declaration) => new EntryInstructionSupportingDocumentsFieldsControl(declaration);
}
