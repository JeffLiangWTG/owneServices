using Enterprise.Customs.BR.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BR.GUI
{
	public partial class InvoiceLineDetailsUserControl : ZUserControl
	{
		public InvoiceLineDetailsUserControl()
		{
			InitializeComponent();

			BindingSource.SetBindingMember(ComplementaryDescriptionTextBox, nameof(JobComInvoiceLine.ComplementaryDescription));
			BindingSource.SetBindingMember(FullGoodsDescriptionTextBox, nameof(JobComInvoiceLine.FullGoodsDescription));
		}
	}
}
