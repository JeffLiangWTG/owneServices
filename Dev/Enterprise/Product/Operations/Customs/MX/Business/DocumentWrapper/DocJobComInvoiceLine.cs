using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.DocumentWrappers.Customs.Base;

namespace Enterprise.Customs.MX.Business
{
	public class DocJobComInvoiceLine : DocBaseJobComInvoiceLine
	{
		DocJobComInvoiceLine(JobComInvoiceLine invoiceLine, BusinessObjectFactory factory)
			: base(invoiceLine, factory)
		{
			this.invoiceLine = invoiceLine;
		}

		readonly JobComInvoiceLine invoiceLine;

		public static DocJobComInvoiceLine New(JobComInvoiceLine invoiceLine, BusinessObjectFactory factory)
		{
			return invoiceLine == null ? null : new DocJobComInvoiceLine(invoiceLine, factory);
		}

		public DocCusEntryLine EntryLine => (DocCusEntryLine)CusEntryLineInternal;

		protected override DocBaseCusEntryLine CreateCusEntryLine(Customs.Business.CusEntryLine entryLineToWrap)
		{
			return DocCusEntryLine.New((CusEntryLine)entryLineToWrap, Factory);
		}

		public DocJobComInvoiceHeader ComInvoiceHeader => (DocJobComInvoiceHeader)InvoiceHeaderInternal;

		protected override DocBaseJobComInvoiceHeader CreateJobComInvoiceHeader(BaseJobComInvoiceHeader invoiceToWrap)
		{
			return DocJobComInvoiceHeader.New((JobComInvoiceHeader)invoiceToWrap, Factory);
		}

		#region Mapped Fields

		public ZString DocGoodsDescription => invoiceLine?.JI_Description ?? ZString.Empty;

		public ZString DocInvoiceUQ => invoiceLine?.JI_InvoiceUQ ?? ZString.Empty;

		public ZString DocInvoiceQuantity => invoiceLine?.JI_InvoiceQuantity.ToString(2) ?? ZString.Empty;

		public ZString DocInvoiceCurrencyCode => invoiceLine?.JI_RX_NKLinePriceCurr ?? ZString.Empty;

		public ZString DocInvoiceLinePrice => invoiceLine?.JI_LinePrice.ToString(2) ?? ZString.Empty;

		public ZString DocInvoiceUnitPrice => invoiceLine?.UnitPrice.ToString(2) ?? ZString.Empty;

		#endregion
	}
}
