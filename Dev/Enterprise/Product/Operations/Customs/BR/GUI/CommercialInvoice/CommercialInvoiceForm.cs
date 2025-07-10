using Enterprise.Customs.BR.Business;

namespace Enterprise.Customs.BR.GUI
{
	public partial class CommercialInvoiceForm : Customs.GUI.CommercialInvoiceForm
	{
		public CommercialInvoiceForm() { }

		public CommercialInvoiceForm(JobComInvoiceHeader header)
			: base(header)
		{
		}
		public new JobDeclaration JobDeclaration => (JobDeclaration)base.JobDeclaration;

		protected override Customs.GUI.CommonInvoiceHeaderUserControl GetHeaderUserControl()
		{
			return new InvoiceHeaderUserControl();
		}

		protected override Customs.GUI.BaseInvoiceLineUserControl GetNewInvoiceLineUserControl()
		{
			if (JobDeclaration.IsImportSiscomex)
			{
				return new ImportSiscomexInvoiceLineUserControl();
			}
			else if (Invoice.IsImport)
			{
				return new ImportInvoiceLineUserControl();
			}
			else
			{
				return new ExportInvoiceLineUserControl();
			}
		}

		protected override Customs.GUI.CommercialInvoiceEDIMenu GetNewTopLevelMenuCore()
		{
			return new CommercialInvoiceEDIMenu();
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}
	}
}
