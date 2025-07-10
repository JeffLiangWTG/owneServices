using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Integration.Billing;

namespace Enterprise.Billing.StlCollector.Retriever.Testing
{
	sealed class BillingDataCollectorForTestFactory : IBillingDataCollectorFactory
	{
		public BillingDataCollector Create(ILogger logger, BusinessObjectFactory bizoFactory, IEnumerable<IStlScriptWithConfig> stlScripts) => new BillingDataCollectorForTest(logger, bizoFactory, stlScripts);
	}
}
