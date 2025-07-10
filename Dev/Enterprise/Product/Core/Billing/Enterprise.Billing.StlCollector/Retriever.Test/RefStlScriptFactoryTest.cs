using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Billing.Collectors;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Billing.Business;
using Enterprise.Billing.StlCollector.Retriever.Scripts;
using Enterprise.Billing.StlCollector.Retriever.Testing;
using Enterprise.Integration.Billing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Billing.StlCollector.Retriever
{
	sealed class RefStlScriptFactoryTest : TestCaseWithFactory
	{
		public void TestCreateScripts()
		{
			CreateDummyScriptBizo(Factory, featureCode: "ACT", activeOn: "ALL");
			CreateDummyScriptBizo(Factory, featureCode: "INA", activeOn: "NON");
			Factory.Save();

			using (RetrieverTestHelper.MockProductRegistration(DatabaseTypes.Codes.Test))
			{
				var scriptFactory = new RefStlScriptFactory();
				var scripts = scriptFactory.CreateScripts(Factory);
				AssertEquals("Wrong number of scripts", 1, scripts.Count());
				AssertEquals("Wrong script found", "ACT", scripts.First().Code);
			}
		}

		public void TestAllOverridenByTST()
		{
			CreateDummyScriptBizo(Factory, featureCode: "ACT", activeOn: "ALL");
			var tstBizo = CreateDummyScriptBizo(Factory, featureCode: "ACT", activeOn: "TST");
			tstBizo.STL_DataGranularity = RefStlItemGrain.MonthlyAllowHistoricalData;
			Factory.Save();

			using (RetrieverTestHelper.MockProductRegistration(DatabaseTypes.Codes.Test))
			{
				var scriptFactory = new RefStlScriptFactory();
				var scripts = scriptFactory.CreateScripts(Factory);
				AssertEquals("Wrong number of scripts", 1, scripts.Count());
				var script = scripts.First();
				AssertEquals("Wrong script found", "ACT", script.Code);
				AssertEquals("TST did not override ALL", StlDataGrain.MonthlyAllowHistoricalData, script.StlGrain);
			}
		}

		public void TestAllOverridenByPRD()
		{
			CreateDummyScriptBizo(Factory, featureCode: "ACT", activeOn: "ALL");
			var prdBizo = CreateDummyScriptBizo(Factory, featureCode: "ACT", activeOn: "PRD");
			prdBizo.STL_DataGranularity = RefStlItemGrain.MonthlyAllowHistoricalData;
			Factory.Save();

			using (RetrieverTestHelper.MockProductRegistration(DatabaseTypes.Codes.Production))
			{
				var scriptFactory = new RefStlScriptFactory();
				var scripts = scriptFactory.CreateScripts(Factory);
				AssertEquals("Wrong number of scripts", 1, scripts.Count());
				var script = scripts.First();
				AssertEquals("Wrong script found", "ACT", script.Code);
				AssertEquals("PRD did not override ALL", StlDataGrain.MonthlyAllowHistoricalData, script.StlGrain);
			}
		}

		public void TestCreatesDynamicCollectorsFromCodeIfRegistrySettingEnabled()
		{
			using (ReleaseInfo.SetTemporaryInstanceForTesting(ReleaseInfo.CreateNewInstanceForTestingWithALPVersionBasedOffDateNow()))
			using (RawDataRegistry.Instance.ObtainDynamicSTLCollectorDefinitionsFromTheCode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var scriptsLoaded = new RefStlScriptFactory().CreateScripts(Factory);
				foreach (var script in ProductionScriptsLoadedViaReflection.Select(s => (IStlScript)s).Where(s => string.IsNullOrEmpty(s.MaxCW1Version)))
				{
					var matchingScript = (RefStlScriptRetriever)scriptsLoaded.SingleOrDefault(i => i.Code.Equals(script.Code, StringComparison.InvariantCultureIgnoreCase));
					AssertNotNull(script.Code, matchingScript);
					AssertEquals("Type should match", script.Name, matchingScript.Name);
				}
			}
		}

		public void TestAllDynamicCollectorsAreInTheRightBinary()
		{
			var dynamicBaseType = typeof(RefStlScriptWithDefaults);
			var collectors = ObjectFactory.Get<ListObject>("StlDynamicCollectorsList").Cast<IRefStlScript>();
			foreach (var collector in collectors)
			{
				AssertEquals("All Collectors should be in the CargoWise.Billing.Collectors binary", dynamicBaseType.Assembly.FullName, collector.GetType().Assembly.FullName);
			}
		}

		public static RefStlScript CreateDummyScriptBizo(BusinessObjectFactory factory, string featureCode, string activeOn)
		{
			var dummyBizo = factory.New<RefStlScript>();
			dummyBizo.STL_ActiveOn = activeOn;
			dummyBizo.STL_FeatureCode = featureCode;
			dummyBizo.STL_DataGranularity = "TRN";
			dummyBizo.STL_TransactionDateUtc = "DummyField";
			dummyBizo.STL_GuidReference = "DummyGuid";
			dummyBizo.STL_TransactionCount = "1";
			dummyBizo.STL_FromClause = "FROM DummyTable";
			return dummyBizo;
		}

		static readonly Lazy<IStlScript[]> scriptDataLoadedViaReflection = new Lazy<IStlScript[]>(() =>
		{
			var dynamicBaseType = typeof(RefStlScriptWithDefaults);
			var dynamicBaseAssembly = dynamicBaseType.Assembly;

			var query =
				from t in dynamicBaseAssembly.GetTypes()
				where t.IsClass && !t.IsAbstract && !t.IsNestedPrivate && t.IsSubclassOf(dynamicBaseType)
				select t;

			return query.AsEnumerable().Select(scriptType => new RefStlScriptRetriever((IRefStlScript)Activator.CreateInstance(scriptType))).ToArray();
		});

		public static IEnumerable<RefStlScriptRetriever> ProductionScriptsLoadedViaReflection => scriptDataLoadedViaReflection.Value.Where(s => !ScriptsUsedPurelyForUnitTesting.Contains(s.Name)).Select(s => (RefStlScriptRetriever)s);

		public static string[] ScriptsUsedPurelyForUnitTesting = { "DummyRefStlScript", "DummyScript", "DummyDateTimeOffsetScript" };
	}
}
