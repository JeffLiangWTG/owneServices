using CargoWise.EntityFramework;
using Enterprise.DocumentWrappers.Customs.Base;

namespace Enterprise.Customs.MX.Business
{
	public class DocCusEntryLine : DocBaseCusEntryLine
	{
		DocCusEntryLine(CusEntryLine cusEntryLine, BusinessObjectFactory factoryToWrap)
			: base(cusEntryLine, factoryToWrap)
		{
		}

		public static DocCusEntryLine New(CusEntryLine cusEntryLine, BusinessObjectFactory factoryToWrap) => cusEntryLine == null ? null : new DocCusEntryLine(cusEntryLine, factoryToWrap);

		protected override DocBaseJobComInvoiceLine CreateJobComInvoiceLine(Customs.Business.BaseJobComInvoiceLine invoiceLineToWrap) => DocJobComInvoiceLine.New((JobComInvoiceLine)invoiceLineToWrap, Factory);

		public DocJobComInvoiceLine InvoiceLine => (DocJobComInvoiceLine)InvoiceLineInternal;
	}
}
