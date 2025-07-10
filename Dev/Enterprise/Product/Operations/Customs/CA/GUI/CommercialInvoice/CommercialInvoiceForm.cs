using CargoWise.Types;
using Enterprise.Customs.CA.Business;

namespace Enterprise.Customs.CA.GUI
{
	public partial class CommercialInvoiceForm : Customs.GUI.CommercialInvoiceForm
	{
		public CommercialInvoiceForm() { }

		public CommercialInvoiceForm(JobComInvoiceHeader header)
			: base(header)
		{
		}

		protected override Customs.GUI.CommonInvoiceHeaderUserControl GetHeaderUserControl()
		{
			return new InvoiceHeaderUserControl();
		}

		protected override Customs.GUI.BaseInvoiceLineUserControl GetNewInvoiceLineUserControl()
		{
			Customs.GUI.BaseInvoiceLineUserControl result;
			switch (JobDeclaration?.JE_MessageType ?? ZString.Empty)
			{
				case JobMessageTypeList.Codes.Export:
					result = new CAExportInvoiceLineUserControl();
					break;
				case JobMessageTypeList.Codes.Import:
					result = new CAImportInvoiceLineUserControl();
					break;
				default:
					result = new CAInvoiceLineUserControl();
					break;
			}
			return result;
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}
	}
}
