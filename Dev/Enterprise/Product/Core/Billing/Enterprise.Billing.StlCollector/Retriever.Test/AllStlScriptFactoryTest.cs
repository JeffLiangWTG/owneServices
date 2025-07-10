using System;
using System.Linq;
using CargoWise.Billing.Collectors;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.Billing.StlCollector.Retriever.Scripts;
using Enterprise.Billing.StlCollector.Retriever.Testing;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever
{
	sealed class AllStlScriptFactoryTest : TestCaseWithFactory
	{
		public void TestLogErrorForDynamicSTLCollectorClashingWithCustom()
		{
			using (RetrieverTestHelper.MockProductRegistration(DatabaseTypes.Codes.Production, isInternalSystem: false))
			using (RawDataRegistry.Instance.EHubTesting.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var logger = new Mock<ILogger>();
				var scriptFactory = new AllStlScriptFactory(logger.Object);
				var staticStlScriptFactory = new CustomStlCollectorFactory();
				var staticScript = staticStlScriptFactory.CreateScripts(Factory).First();
				RefStlScriptFactoryTest.CreateDummyScriptBizo(Factory, featureCode: staticScript.Code, activeOn: "ALL");
				Factory.Save();

				var scripts = scriptFactory.CreateScripts(Factory);
				AssertGreaterThan("Scripts should be created", scripts.Count(), 0);
				var matchingScripts = scripts.Where(s => s.Code.Equals(staticScript.Code));
				AssertEquals("duplicate script should have been removed", 1, matchingScripts.Count());
				var errorMessage = $"The STL collector with FeatureCode={staticScript.Code} has both custom and dynamic definitions.  The Dynamic collector could not be added to the system.";
				logger.Verify(l => l.Log(LogType.Error, errorMessage));
				AssertEquals(errorMessage, ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
			}
		}

		public void TestDynamicCollectorsCreateOnAllEnvironments()
		{
			CreateDummyScriptBizoAndSave();
			AssertDynamicCollectors(DatabaseTypes.Codes.Production, isInternalSystem: true);
			AssertDynamicCollectors(DatabaseTypes.Codes.Training, isInternalSystem: true);
			AssertDynamicCollectors(DatabaseTypes.Codes.Test, isInternalSystem: true);
			AssertDynamicCollectors(DatabaseTypes.Codes.Production, isInternalSystem: false);
			AssertDynamicCollectors(DatabaseTypes.Codes.Training, isInternalSystem: false);
			AssertDynamicCollectors(DatabaseTypes.Codes.Test, isInternalSystem: false);
		}

		public void TestTransactionFactoryProvider()
		{
			IRefStlScript dummyScript = RefStlScriptFactoryTest.CreateDummyScriptBizo(Factory, featureCode: "DUM", activeOn: "ALL");
			TestTransactionFactoryWhenNotProductionSystem(DatabaseTypes.Codes.Training, false, dummyScript, typeof(UsageTransactionFactory));
			TestTransactionFactoryWhenNotProductionSystem(DatabaseTypes.Codes.Test, false, dummyScript, typeof(UsageTransactionFactory));
			TestTransactionFactoryWhenNotProductionSystem(DatabaseTypes.Codes.Production, false, dummyScript, typeof(BillingTransactionFactory));
			TestTransactionFactoryWhenNotProductionSystem(DatabaseTypes.Codes.Production, true, dummyScript, typeof(BillingTransactionFactory));
			TestTransactionFactoryWhenNotProductionSystem(DatabaseTypes.Codes.Test, true, dummyScript, typeof(BillingTransactionFactory));
			TestTransactionFactoryWhenNotProductionSystem(DatabaseTypes.Codes.Training, true, dummyScript, typeof(BillingTransactionFactory));
		}

		[ExpectNoExceptions]
		public void TestNoCodeClashesBetweenDynamicAndStaticCollectors()
		{
			using (ReleaseInfo.SetTemporaryInstanceForTesting(ReleaseInfo.CreateNewInstanceForTestingWithALPVersionBasedOffDateNow()))
			using (RawDataRegistry.Instance.ObtainDynamicSTLCollectorDefinitionsFromTheCode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var scripts = CustomStlCollectorFactoryTest.ScriptsLoadedViaReflection.ToDictionary((s) => s.Code);
				new RefStlScriptFactory().CreateScripts(Factory).ForEach(s => scripts.Add(s.Code, s));
			}
		}

		[ExpectNoExceptions]
		public void TestNoFeatureClashesBetweenDynamicAndStaticCollectors()
		{
			using (ReleaseInfo.SetTemporaryInstanceForTesting(ReleaseInfo.CreateNewInstanceForTestingWithALPVersionBasedOffDateNow()))
			using (RawDataRegistry.Instance.ObtainDynamicSTLCollectorDefinitionsFromTheCode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var scripts = CustomStlCollectorFactoryTest.ScriptsLoadedViaReflection.ToDictionary((s) => s.Feature);
				new RefStlScriptFactory().CreateScripts(Factory).ForEach(s => scripts.Add(s.Feature, s));
			}
		}

		public void TestCreatesStaticAndDynamicCollectors()
		{
			AssertGreaterThan("Should be some static scripts", CustomStlCollectorFactoryTest.ScriptsLoadedViaReflection.Length, 0);
			RefStlScriptFactoryTest.CreateDummyScriptBizo(Factory, "DYN", "ALL");
			Factory.Save();

			using (RetrieverTestHelper.MockProductRegistration(DatabaseTypes.Codes.Production, isInternalSystem: false))
			using (RawDataRegistry.Instance.EHubTesting.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var scriptsLoaded = new AllStlScriptFactory().CreateScripts(Factory);
				AssertEquals("All static scripts and dynamic scripts should be loaded", CustomStlCollectorFactoryTest.ScriptsLoadedViaReflection.Length + 1, scriptsLoaded.Count());

				foreach (var item in CustomStlCollectorFactoryTest.ScriptsLoadedViaReflection)
				{
					var matchingItem = scriptsLoaded.SingleOrDefault(i => i.Code.Equals(item.Code, StringComparison.InvariantCultureIgnoreCase));
					AssertNotNull(item.Code, matchingItem);
					AssertEquals("Type should match", item.GetType(), matchingItem.GetType());
				}

				var dynamicScript = scriptsLoaded.SingleOrDefault(i => i.Code.Equals("DYN", StringComparison.InvariantCultureIgnoreCase));
				AssertNotNull("Could not find dynamic script", dynamicScript);
				AssertEquals("Type should match", typeof(RefStlScriptRetriever), dynamicScript.GetType());
			}
		}

		void CreateDummyScriptBizoAndSave()
		{
			RefStlScriptFactoryTest.CreateDummyScriptBizo(Factory, featureCode: "DUM", activeOn: "ALL");
			Factory.Save();
		}

		public void TestTransactionFactoryWhenNotProductionSystem(string dataBaseType, bool isInternalSystem, IRefStlScript dummyScript, Type typeOfTransactionFactory)
		{
			bool result = false;
			using (RetrieverTestHelper.MockProductRegistration(dataBaseType, isInternalSystem))
			{
				TransactionFactoryProvider transactionFactoryProvider = new TransactionFactoryProvider();
				if (transactionFactoryProvider.GetTransactionFactory(dummyScript).GetType() == typeOfTransactionFactory)
				{
					result = true;
				}
			}
			Assert(result);
		}

		public void AssertDynamicCollectors(string databaseType, bool isInternalSystem)
		{
			using (RetrieverTestHelper.MockProductRegistration(databaseType, isInternalSystem))
			using (Globals.TemporaryOverrideForIsTest(false))
			using (Globals.TemporaryOverrideForIsDebugMode(false))
			using (Globals.SetIsUserInteractiveForTest(false))
			{
				var logger = new Mock<ILogger>();
				var scriptFactory = new AllStlScriptFactory(logger.Object);
				var staticStlScriptFactory = new CustomStlCollectorFactory();
				var staticScripts = staticStlScriptFactory.CreateScripts(Factory);
				var scripts = scriptFactory.CreateScripts(Factory);

				AssertEquals("All scripts should be created", staticScripts.Count() + 1, scripts.Count());
				AssertNotNull("Dynamic script should be created", scripts.SingleOrDefault((s) => s.Code == "DUM"));
			}
		}
	}
}
