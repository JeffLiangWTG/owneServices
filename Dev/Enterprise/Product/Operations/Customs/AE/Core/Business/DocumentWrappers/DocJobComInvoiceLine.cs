
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.Customs.Base;

namespace Enterprise.Customs.AE.Business;

public class DocJobComInvoiceLine : DocBaseJobComInvoiceLine
{
	DocJobComInvoiceLine(JobComInvoiceLine jobComInvoiceLine, BusinessObjectFactory factoryToWrap)
		: base(jobComInvoiceLine, factoryToWrap)
	{
	}

	public static DocJobComInvoiceLine New(JobComInvoiceLine jobComInvoiceLine, BusinessObjectFactory factoryToWrap)
	{
		if (jobComInvoiceLine == null)
		{
			return null;
		}
		else
		{
			return new DocJobComInvoiceLine(jobComInvoiceLine, factoryToWrap);
		}
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

	public DocCusEntryLine EntryLine
	{
		get { return (DocCusEntryLine)CusEntryLineInternal; }
	}

	public DocJobComInvoiceHeader ComInvoiceHeader
	{
		get { return (DocJobComInvoiceHeader)InvoiceHeaderInternal; }
	}

	#endregion

	#region ZString Fields

	public ZDecimal InvoiceHeaderExchangeRate
	{
		get { return JobComInvoiceLine.InvoiceHeader.JZ_InvoiceCurrExRate; }
	}

	#endregion

	#region Implementation

	JobComInvoiceLine JobComInvoiceLine
	{
		get { return (JobComInvoiceLine)WrappedObject; }
	}

	#endregion

}
