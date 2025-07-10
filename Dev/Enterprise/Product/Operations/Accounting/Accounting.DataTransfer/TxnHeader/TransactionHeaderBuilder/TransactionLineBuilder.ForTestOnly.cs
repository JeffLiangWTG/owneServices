#if DEBUG

using CargoWise.EntityFramework;

namespace Enterprise.Accounting.DataTransfer.Invoices
{
	public partial class TransactionLineBuilder
	{
		public Business.GenericJob.GenericJob GetGenericJob_ForTestOnly(BusinessObjectFactory factory, Enterprise.DataTransfer.Xml.XsdVersion1.TxnLine xmlInvoiceLine)
		{
			return GetGenericJob(factory, xmlInvoiceLine);
		}
	}
}

#endif
