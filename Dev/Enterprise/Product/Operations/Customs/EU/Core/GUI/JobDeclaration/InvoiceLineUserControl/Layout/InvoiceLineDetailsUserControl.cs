using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public partial class InvoiceLineDetailsUserControl : ZUserControl
	{
		public InvoiceLineDetailsUserControl()
		{
			InitializeComponent();
			BindingSource.SetBindingMember(ProcessingDescriptionLongTextControl, JobComInvoiceLine.Schema.ZG_ProcessingDescription);
		}
	}
}
