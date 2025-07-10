using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	internal interface IBusinessObjectFactoryProviderForJCS
	{
		internal BusinessObjectFactory GetNewBusinessObjectFactory(string factoryName);
		internal bool SaveChanges {  get; }
	}

	internal class BusinessObjectFactoryProviderForJCS : IBusinessObjectFactoryProviderForJCS
	{
		BusinessObjectFactory IBusinessObjectFactoryProviderForJCS.GetNewBusinessObjectFactory(string factoryName) => new BusinessObjectFactory { RefreshEnabled = false, NameForDebugging = factoryName };
		bool IBusinessObjectFactoryProviderForJCS.SaveChanges => true;
	}
}
