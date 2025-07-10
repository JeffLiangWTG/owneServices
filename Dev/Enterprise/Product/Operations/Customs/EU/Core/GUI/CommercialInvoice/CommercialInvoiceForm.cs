using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.EU.GUI.CommercialInvoice
{
	public partial class CommercialInvoiceForm : Customs.GUI.CommercialInvoiceForm
	{
		public CommercialInvoiceForm(JobComInvoiceHeader invoiceHeader)
			: base(invoiceHeader)
		{
			this.invoiceHeader = invoiceHeader;
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		protected override Customs.GUI.CommonInvoiceHeaderUserControl GetHeaderUserControl() => new InvoiceHeaderUserControl();

		protected override Customs.GUI.BaseInvoiceLineUserControl GetNewInvoiceLineUserControl()
		{
			EUInvoiceLineUserControl result = null;
			if (invoiceHeader != null)
			{
				if (invoiceHeader.JZ_MessageType == MessageTypeList.Codes.Import)
				{
					result = GetImportInvoiceLineUserControl();
				}
				else if (invoiceHeader.JZ_MessageType == MessageTypeList.Codes.Export)
				{
					result = GetExportInvoiceLineUserControl();
				}
			}
			if (result == null)
			{
				result = new EUInvoiceLineUserControl();
			}

			result.SetIsStandaloneInvoiceForm(true);

			return result;
		}

		protected virtual EUInvoiceLineUserControl GetImportInvoiceLineUserControl() => new EUImportInvoiceLineUserControl();

		protected virtual EUInvoiceLineUserControl GetExportInvoiceLineUserControl() => new EUExportInvoiceLineUserControl();

		readonly JobComInvoiceHeader invoiceHeader;
	}
}
