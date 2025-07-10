using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Integration.Billing;

namespace Enterprise.Billing.StlCollector.Retriever.Testing
{
	sealed class ScriptLoaderForTest : IScriptLoader
	{
		readonly IEnumerable<IStlScriptWithConfig> stlScripts;

		public ScriptLoaderForTest(IEnumerable<IStlScriptWithConfig> stlScripts)
		{
			this.stlScripts = stlScripts;
		}

		public IEnumerable<IStlScriptWithConfig> Load(BusinessObjectFactory bizOFactory) => stlScripts;
	}
}
