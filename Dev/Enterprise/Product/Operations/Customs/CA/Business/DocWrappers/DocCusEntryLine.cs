using CargoWise.EntityFramework;
using Enterprise.DocumentWrappers.Customs.Base;

namespace Enterprise.Customs.CA.Business
{
	public class DocCusEntryLine : DocBaseCusEntryLine
	{
		DocCusEntryLine(CusEntryLine cusEntryLine, BusinessObjectFactory factoryToWrap)
			: base(cusEntryLine, factoryToWrap)
		{
		}

		public static DocCusEntryLine New(CusEntryLine cusEntryLine, BusinessObjectFactory factoryToWrap)
		{
			return cusEntryLine == null ? null : new DocCusEntryLine(cusEntryLine, factoryToWrap);
		}

		protected override DocBaseJobComInvoiceLine CreateJobComInvoiceLine(Customs.Business.BaseJobComInvoiceLine invoiceLineToWrap)
		{
			return DocJobComInvoiceLine.New((JobComInvoiceLine)invoiceLineToWrap, Factory);
		}

		public DocJobComInvoiceLine InvoiceLine
		{
			get { return (DocJobComInvoiceLine)InvoiceLineInternal; }
		}
	}
}
