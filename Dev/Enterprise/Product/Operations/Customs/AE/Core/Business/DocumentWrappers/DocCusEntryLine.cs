
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.Customs.Base;

namespace Enterprise.Customs.AE.Business;

public class DocCusEntryLine : DocBaseCusEntryLine
{
	DocCusEntryLine(CusEntryLine cusEntryLine, BusinessObjectFactory factoryToWrap)
		: base(cusEntryLine, factoryToWrap)
	{
	}

	public static DocCusEntryLine New(CusEntryLine cusEntryLine, BusinessObjectFactory factoryToWrap)
	{
		if (cusEntryLine == null)
		{
			return null;
		}
		else
		{
			return new DocCusEntryLine(cusEntryLine, factoryToWrap);
		}
	}

	#region Overrides

	protected override DocBaseJobComInvoiceLine CreateJobComInvoiceLine(Customs.Business.BaseJobComInvoiceLine invoiceLineToWrap)
	{
		return DocJobComInvoiceLine.New((JobComInvoiceLine)invoiceLineToWrap, Factory);
	}

	#endregion

	public DocJobComInvoiceLine InvoiceLine
	{
		get { return (DocJobComInvoiceLine)InvoiceLineInternal; }
	}

	public ZDecimal CIF
	{
		get { return CusEntryLine.CIF.Amount; }
	}

	public ZDecimal CIFInLocalCurrency
	{
		get { return CusEntryLine.CIFInLocalCurrency.Amount; }
	}

	#region Implementation

	CusEntryLine CusEntryLine
	{
		get { return (CusEntryLine)WrappedObject; }
	}

	#endregion
}
