using System;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.EMCS.GUI
{
	public partial class EMCSInvoiceLinePackagesUserControl : ZUserControl
	{
		public EMCSInvoiceLinePackagesUserControl()
		{
			InitializeComponent();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			BindingSource.SetBindingMember(MarksAndNumbersTextBox, NonPersistentPackagePivot.Schema.MarksAndNumbers);
		}
	}
}
