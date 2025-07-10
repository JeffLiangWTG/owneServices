namespace Enterprise.Customs.BE.GUI.PlugIn;

public partial class InvoiceLineSupportingDocumentsFieldsControl : EU.GUI.PlugIn.SupportingDocumentsFieldsControl
{
	public InvoiceLineSupportingDocumentsFieldsControl()
	{
		InitializeComponent();
		SupportingDocumentsGroupBox.CaptionResourceString = Res.GetData("ACFDA3F6-A18D-4DC7-83ED-906DEF0D32C2", "[UCC 2/3] Supporting documents");
	}
}
