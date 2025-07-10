using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IN.GUI;

public partial class InvoiceLineDetailsLayoutUserControl : ZUserControl
{
	public InvoiceLineDetailsLayoutUserControl()
	{
		InitializeComponent();
		this.BindingSource.SetBindingMember(this.AccessoryDescriptionLongTextBox, nameof(Business.JobComInvoiceLine.AccessoryDescription));
	}
}

