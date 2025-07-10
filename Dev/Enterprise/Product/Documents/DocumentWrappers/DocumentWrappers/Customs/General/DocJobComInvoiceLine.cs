using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.DocumentWrappers.Customs.Base;

namespace Enterprise.DocumentWrappers.Customs.General
{
	public class DocJobComInvoiceLine : DocBaseJobComInvoiceLine
	{
		DocJobComInvoiceLine(BaseJobComInvoiceLine jobComInvoiceLine, BusinessObjectFactory factoryToWrap)
			: base(jobComInvoiceLine, factoryToWrap)
		{
		}

		public static new DocJobComInvoiceLine New(BaseJobComInvoiceLine jobComInvoiceLine, BusinessObjectFactory factoryToWrap)
		{
			return jobComInvoiceLine == null ? null : new DocJobComInvoiceLine(jobComInvoiceLine, factoryToWrap);
		}

		#region Overrides

		protected override DocBaseJobComInvoiceHeader CreateJobComInvoiceHeader(BaseJobComInvoiceHeader invoiceToWrap)
		{
			return DocJobComInvoiceHeader.New(invoiceToWrap, Factory);
		}

		protected override DocBaseCusEntryLine CreateCusEntryLine(CusEntryLine entryLineToWrap)
		{
			return DocCusEntryLine.New(entryLineToWrap, Factory);
		}

		#endregion

		#region Wrapper Fields

		public DocCusEntryLine EntryLine
		{
			get { return (DocCusEntryLine)CusEntryLineInternal; }
		}

		public DocJobComInvoiceHeader ComInvoiceHeader
		{
			get { return (DocJobComInvoiceHeader)InvoiceHeaderInternal; }
		}

		#endregion
	}
}
