using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Integration.Billing;

namespace Enterprise.Billing.StlCollector.Retriever
{
	class CustomStlCollectorFactory : IScriptFactory
	{
		public IEnumerable<IStlItem> CreateScripts(BusinessObjectFactory factory) => ObjectFactory.Get<ListObject>("StlCustomCollectorsList").Cast<IStlItem>().Where(s => s.IsActive);
	}
}
