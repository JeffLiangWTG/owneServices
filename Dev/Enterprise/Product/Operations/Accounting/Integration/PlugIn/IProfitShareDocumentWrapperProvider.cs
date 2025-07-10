using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.Accounting.Integration
{
	public interface IProfitShareDocumentWrapperProvider
	{
		DocumentWrapper[] GetDocumentWrappers(IJobCostingPlugIn plugIn, BusinessObjectFactory factory);
	}
}
