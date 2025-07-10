using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.EU.Business.FetchStrategies
{
	public class JobComInvoiceHeaderFetchStrategy : Customs.Business.FetchStrategies.BaseJobComInvoiceHeaderFetchStrategy
	{
		public JobComInvoiceHeaderFetchStrategy(JobComInvoiceHeader invoiceHeader)
			: base(invoiceHeader)
		{
		}
	}
}
