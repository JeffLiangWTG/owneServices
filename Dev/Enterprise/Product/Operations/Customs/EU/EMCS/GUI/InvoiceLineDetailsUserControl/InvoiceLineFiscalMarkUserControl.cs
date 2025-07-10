using System;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.EMCS.GUI
{
	public partial class InvoiceLineFiscalMarkUserControl : ZUserControl
	{
		public InvoiceLineFiscalMarkUserControl()
		{
			InitializeComponent();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			BindingSource.SetBindingMember(FiscalMarkTextBox, EMCSJobComInvoiceLine.Schema.ZG_FiscalMark);
		}
	}
}
