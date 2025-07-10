using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.JobInvoicing.AutoJobClosureServiceTask.Diagnostic
{
	internal class BusinessObjectFactoryProviderForJCSDiagnosis : IBusinessObjectFactoryProviderForJCS
	{
		internal BusinessObjectFactoryProviderForJCSDiagnosis(BusinessObjectFactory factory)
		{
			this.factory = Argument.NotNull(factory, nameof(factory));
		}
		readonly BusinessObjectFactory factory;

		BusinessObjectFactory IBusinessObjectFactoryProviderForJCS.GetNewBusinessObjectFactory(string factoryName)
		{
			factory.NameForDebugging = factoryName;
			return factory;
		}

		bool IBusinessObjectFactoryProviderForJCS.SaveChanges => false;
	}
}
