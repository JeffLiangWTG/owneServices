using System.Linq;
using CargoWise.Application;
using CargoWise.Billing.Collectors;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts
{
	sealed class RefStlScriptWithDefaultsTestCase : TestCase
	{
		public void TestDynamicStlScriptClassesInStlDynamicCollectorsList()
		{
			var stlCollectorsList = ObjectFactory.Get<ListObject>("StlDynamicCollectorsList").Cast<IRefStlScript>();

			foreach (var instance in RefStlScriptFactoryTest.ProductionScriptsLoadedViaReflection.Select(s => (IStlScript)s))
			{
				var instanceFromFactory = stlCollectorsList.FirstOrDefault(stlCollector => stlCollector.FeatureCode == instance.Code);

				AssertNotNull($"StlDynamicCollectorsList does not contain {instance.Name}. Please add {instance.Name} to the StlDynamicCollectorsList of StlDynamicCollectorsListConfiguration.xml.", instanceFromFactory);
			}
		}
	}
}
