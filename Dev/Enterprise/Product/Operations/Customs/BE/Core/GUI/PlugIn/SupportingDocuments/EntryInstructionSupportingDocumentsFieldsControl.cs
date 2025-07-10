namespace Enterprise.Customs.BE.GUI.PlugIn;

public partial class EntryInstructionSupportingDocumentsFieldsControl : EU.GUI.PlugIn.InvoiceLayoutSupportingDocumentsFieldsControl
{
	public EntryInstructionSupportingDocumentsFieldsControl(EU.Business.Declaration.JobDeclaration declaration) : base(declaration)
	{
		InitializeComponent();
		SupportingDocumentsGroupBox.CaptionResourceString = Res.GetData("BEF8CCFD-717D-442F-A670-1AA28AB3392E", "Supporting Documents");
	}
}
