using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers
{
	public class DocDisbursementJobsCloseBatchProvider : IDocDsbJobCloseBatchProvider
	{
		public DocDisbursementJobsCloseBatchProvider()
		{
		}

		public IBODocDataProvider CreateDocDsbJobCloseBatch(DsbJobCloseBatch batch, BusinessObjectFactory factory)
		{
			return DocDisbursementJobsCloseBatch.New(batch, factory);
		}
	}
}
