using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.DocumentWrappers.Customs.Base;

namespace Enterprise.DocumentWrappers.Customs.General
{
	public class DocCusEntryLine : DocBaseCusEntryLine
	{
		DocCusEntryLine(ICusEntryLine cusEntryLine, BusinessObjectFactory factoryToWrap)
			: base(cusEntryLine, factoryToWrap)
		{
		}

		public static DocCusEntryLine New(CusEntryLine cusEntryLine, BusinessObjectFactory factoryToWrap)
		{
			return cusEntryLine == null ? null : new DocCusEntryLine(cusEntryLine, factoryToWrap);
		}

		protected override DocBaseJobComInvoiceLine CreateJobComInvoiceLine(BaseJobComInvoiceLine invoiceLineToWrap)
		{
			return DocJobComInvoiceLine.New(invoiceLineToWrap, Factory);
		}

		public DocJobComInvoiceLine InvoiceLine
		{
			get { return (DocJobComInvoiceLine)InvoiceLineInternal; }
		}
	}
}
