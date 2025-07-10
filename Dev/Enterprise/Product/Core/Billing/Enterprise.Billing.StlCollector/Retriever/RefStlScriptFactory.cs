using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Billing.Collectors;
using CargoWise.EntityFramework;
using Enterprise.Billing.Business;
using Enterprise.Billing.StlCollector.Retriever.Scripts;
using Enterprise.Integration.Billing;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Billing.StlCollector.Retriever
{
	class RefStlScriptFactory : IScriptFactory
	{
		public IEnumerable<IStlItem> CreateScripts(BusinessObjectFactory factory)
		{
			var transactionFactoryProvider = new TransactionFactoryProvider();
			var targetScripts = new Dictionary<string, RefStlScriptRetriever>();
			var rawScripts = RawDataRegistry.Instance.ObtainDynamicSTLCollectorDefinitionsFromTheCode.Value ?
				ObjectFactory.Get<ListObject>("StlDynamicCollectorsList").Cast<IRefStlScript>() :
				factory.Load<RefStlScript>(new ZQuery());
			foreach (var script in rawScripts.Select(s => new RefStlScriptRetriever(s, transactionFactoryProvider)).Where(r => r.IsActive))
			{
				var code = ((IStlScript)script).Code;
				if (targetScripts.TryGetValue(code, out var existingScript))
				{
					if (existingScript.IsSupersededBy(script))
					{
						targetScripts[code] = script;
					}
				}
				else
				{
					targetScripts.Add(code, script);
				}
			}

			return targetScripts.Values;
		}
	}
}
