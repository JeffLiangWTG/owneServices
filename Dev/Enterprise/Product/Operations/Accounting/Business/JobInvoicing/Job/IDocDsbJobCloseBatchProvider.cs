using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public interface IDocDsbJobCloseBatchProvider
	{
		IBODocDataProvider CreateDocDsbJobCloseBatch(DsbJobCloseBatch batch, BusinessObjectFactory factory);
	}
}
