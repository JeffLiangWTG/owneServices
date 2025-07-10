using System;
using Enterprise.Customs.MX.Business;

namespace Enterprise.Customs.MX.GUI
{
	public partial class CommercialInvoiceForm : Customs.GUI.CommercialInvoiceForm
	{
		[Obsolete("Do not call. Only for designer use.")]
		public CommercialInvoiceForm()
		{
		}

		public CommercialInvoiceForm(JobComInvoiceHeader header)
			: base(header)
		{
		}

		protected override Customs.GUI.CommonInvoiceHeaderUserControl GetHeaderUserControl() => new InvoiceHeaderUserControl();

		protected override Customs.GUI.BaseInvoiceLineUserControl GetNewInvoiceLineUserControl()
		{
			if (Invoice.IsImport)
			{
				return new ImportInvoiceLineUserControl();
			}
			else if (Invoice.IsExport)
			{
				return new ExportInvoiceLineUserControl();
			}
			return new BaseInvoiceLineUserControl();
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}
	}
}
