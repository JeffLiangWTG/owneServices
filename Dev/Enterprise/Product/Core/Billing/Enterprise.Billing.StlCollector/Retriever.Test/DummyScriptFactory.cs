using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Integration.Billing;

namespace Enterprise.Billing.StlCollector.Retriever
{
	public class DummyScriptFactory : IScriptFactory
	{
		readonly IEnumerable<IStlItem> scripts;

		public DummyScriptFactory(IEnumerable<IStlItem> scripts)
		{
			this.scripts = scripts;
		}

		public IEnumerable<IStlItem> CreateScripts(BusinessObjectFactory factory) => scripts;
	}
}
