using Enterprise.Customs.GUI;

namespace Enterprise.Customs.BE.GUI.PlugIn;

public partial class InvoiceHeaderSupportingDocumentsFieldsControl : BaseCustomsEntryUserControl
{
	public InvoiceHeaderSupportingDocumentsFieldsControl()
	{
		InitializeComponent();
		SupportingDocumentsGroupBox.CaptionResourceString = Res.GetData("904839C3-30E2-43E8-8645-09EDFF98CB48", "Supporting Documents");
	}
}
