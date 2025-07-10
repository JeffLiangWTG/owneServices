using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Integration.Billing;

namespace Enterprise.Billing.StlCollector.Retriever
{
	interface IBillingDataCollectorFactory
	{
		BillingDataCollector Create(ILogger logger, BusinessObjectFactory bizoFactory, IEnumerable<IStlScriptWithConfig> stlScripts);
	}

	class BillingDataCollectorFactory : IBillingDataCollectorFactory
	{
		public BillingDataCollector Create(ILogger logger, BusinessObjectFactory bizoFactory, IEnumerable<IStlScriptWithConfig> stlScripts) => new BillingDataCollector(logger, bizoFactory, stlScripts);
	}
}
