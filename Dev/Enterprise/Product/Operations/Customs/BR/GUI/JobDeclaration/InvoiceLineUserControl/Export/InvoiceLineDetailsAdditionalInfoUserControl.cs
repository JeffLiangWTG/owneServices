using System;
using Enterprise.Customs.BR.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BR.GUI
{
	public partial class InvoiceLineDetailsAdditionalInfoUserControl : ZUserControl
	{
		public InvoiceLineDetailsAdditionalInfoUserControl()
		{
			InitializeComponent();
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			BRJustificationExportTextBox.Visible = (DataSource as JobDeclaration)?.IsPersistent ?? false;
		}
	}
}
