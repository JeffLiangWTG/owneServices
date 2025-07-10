using CargoWise.EntityFramework;
using Enterprise.DocumentWrappers.Customs.Base;

namespace Enterprise.Customs.CH.Business;

public class DocJobComInvoiceLine : DocBaseJobComInvoiceLine
{
	DocJobComInvoiceLine(JobComInvoiceLine jobComInvoiceLine, BusinessObjectFactory factoryToWrap)
		: base(jobComInvoiceLine, factoryToWrap)
	{
	}

	public static DocJobComInvoiceLine New(JobComInvoiceLine jobComInvoiceLine, BusinessObjectFactory factoryToWrap)
	{
		return jobComInvoiceLine == null ? null : new DocJobComInvoiceLine(jobComInvoiceLine, factoryToWrap);
	}

	#region Overrides

	protected override DocBaseJobComInvoiceHeader CreateJobComInvoiceHeader(Customs.Business.BaseJobComInvoiceHeader invoiceToWrap)
	{
		return DocJobComInvoiceHeader.New((JobComInvoiceHeader)invoiceToWrap, Factory);
	}

	protected override DocBaseCusEntryLine CreateCusEntryLine(Customs.Business.CusEntryLine entryLineToWrap)
	{
		return DocCusEntryLine.New((CusEntryLine)entryLineToWrap, Factory);
	}

	#endregion

	#region Wrapper Fields

	public DocCusEntryLine EntryLine => (DocCusEntryLine)CusEntryLineInternal;

	public DocJobComInvoiceHeader ComInvoiceHeader => (DocJobComInvoiceHeader)InvoiceHeaderInternal;

	#endregion
}
