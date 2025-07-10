using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.BufferManagement.Business.Test
{
	class ModuleGridSectionPanelConfigurationLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestFindModules_ExcludeReports()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var component = BMSTestHelper.CreateBucket(system);
			var section = BMSTestHelper.CreateBoardSection(component);
			section.MS_SectionType = BMConstants.ModuleGridSectionType;

			var panelConfig = new ModuleGridSectionPanelConfiguration(section);
			var lookups = new ModuleGridSectionPanelConfigurationLookups(panelConfig);

			var modules = lookups.AllModules;

			foreach (var module in ReportModules.GetReportModules())
			{
				AssertCollectionNotContains(module, modules);
			}
		}

		public void TestModuleFilters_NewFiltersShouldBeIncludedInCollection()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var component = BMSTestHelper.CreateBucket(system);
			var section = BMSTestHelper.CreateBoardSection(component);
			section.MS_SectionType = BMConstants.ModuleGridSectionType;

			var panelConfig = new ModuleGridSectionPanelConfiguration(section);
			panelConfig.ModuleName = ModuleIDs.ProcessHeader.Name;

			var filter1 = Factory.New<StmModuleFilter>();
			filter1.S9_ModuleID = ModuleIDs.ProcessHeader.Name;
			filter1.S9_FilterName = "Daniel";

			var lookups = new ModuleGridSectionPanelConfigurationLookups(panelConfig);

			Factory.Save();

			var filters = lookups.ModuleFilters;
			AssertCollectionContains(filter1, filters);
		}

		public void TestAllModules_WhenNotLoggedInAsClient_ShouldNotIncludeClientSpecificModules()
		{
			var moduleId = new ClientModuleIdentifier(TestClientModuleId.ClientModuleID1, (NoResString)"ClientModuleID1");
			var info = new ModuleInfo(moduleId, "Enterprise.BufferManagement.Business", "Enterprise.BufferManagement.Business.Testing.DummyFilterGridModule");
			var clientModuleInfo = new NewClientModuleInfo("CategoryName1", "SectionName1", info);
			var clientModuleInfos = new[] { clientModuleInfo };
			TestClientHook.Instance.NewClientModulesForTest = clientModuleInfos;

			var system = BMSTestHelper.CreateSystem(Factory);
			var component = BMSTestHelper.CreateBucket(system);
			var section = BMSTestHelper.CreateBoardSection(component);
			section.MS_SectionType = BMConstants.ModuleGridSectionType;

			var panelConfig = new ModuleGridSectionPanelConfiguration(section);
			var lookups = new ModuleGridSectionPanelConfigurationLookups(panelConfig);

			AssertCollectionNotContains("The client-specific module should not be in the list because we are not logged in as the client, and yet...", moduleId.Name, lookups.AllModules.GetAllCodes());
		}

		public void TestAllModules_WhenLoggedInAsClient_ShouldIncludeClientSpecificModules()
		{
			var moduleId = new ClientModuleIdentifier(TestClientModuleId.ClientModuleID1, (NoResString)"ClientModuleID1");
			var info = new ModuleInfo(moduleId, "Enterprise.BufferManagement.Business", "Enterprise.BufferManagement.Business.Testing.DummyFilterGridModule");
			var clientModuleInfo = new NewClientModuleInfo("CategoryName1", "SectionName1", info);
			var clientModuleInfos = new[] { clientModuleInfo };
			TestClientHook.Instance.NewClientModulesForTest = clientModuleInfos;

			using (ClientHookLoader.Instance.OverrideClientHookForTest(TestClientHook.Instance))
			{
				AssertCollectionNotContains("All property should not contain the new client module, and yet...", moduleId, ModuleIDs.AllExcludingClientModules);
				AssertCollectionContains("AllAllIncludingClientModules property should contain the new client module, and yet...", moduleId, ModuleIDs.AllIncludingClientModules);

				var system = BMSTestHelper.CreateSystem(Factory);
				var component = BMSTestHelper.CreateBucket(system);
				var section = BMSTestHelper.CreateBoardSection(component);
				section.MS_SectionType = BMConstants.ModuleGridSectionType;

				var panelConfig = new ModuleGridSectionPanelConfiguration(section);
				var lookups = new ModuleGridSectionPanelConfigurationLookups(panelConfig);

				AssertCollectionContains("The client-specific module should be in the list because we are logged in as the client, and yet...", moduleId.Name, lookups.AllModules.GetAllCodes());
			}
		}
		public void TestModuleFilters()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var component = BMSTestHelper.CreateBucket(system);
			var section = BMSTestHelper.CreateBoardSection(component);
			section.MS_SectionType = BMConstants.ModuleGridSectionType;

			var panelConfig = new ModuleGridSectionPanelConfiguration(section);
			panelConfig.ModuleName = ModuleIDs.ProcessHeader.Name;

			var filter1 = Factory.New<StmModuleFilter>();
			var filter2 = Factory.New<StmModuleFilter>();
			var filter3 = Factory.New<StmModuleFilter>();
			filter1.S9_ModuleID = ModuleIDs.ProcessHeader.Name;
			filter2.S9_ModuleID = ModuleIDs.ProcessHeader.Name;
			filter3.S9_ModuleID = ModuleIDs.ProcessHeader.Name;
			filter1.S9_FilterName = "Daniel";
			filter2.S9_FilterName = "Stephen";
			filter3.S9_FilterName = "Keogh";

			var lookups = new ModuleGridSectionPanelConfigurationLookups(panelConfig);

			Factory.Save();

			var filters = lookups.ModuleFilters;

			AssertCollectionContains(filter1, filters);
			AssertCollectionContains(filter2, filters);
			AssertCollectionContains(filter3, filters);
		}
		public void TestModuleFilters_FilterBusinessObjectsDefaults()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var component = BMSTestHelper.CreateBucket(system);
			var section = BMSTestHelper.CreateBoardSection(component);
			section.MS_SectionType = BMConstants.ModuleGridSectionType;

			var panelConfig = new ModuleGridSectionPanelConfiguration(section);
			panelConfig.ModuleName = ModuleIDs.ProcessHeader.Name;
			var lookups = new ModuleGridSectionPanelConfigurationLookups(panelConfig);

			AssertEquals(panelConfig.ModuleName, lookups.ModuleFilters.FilterBusinessObjectDefaults["Module" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"].Value);

			panelConfig.ModuleName = string.Empty;
			Factory.Save();
			AssertEquals(0, lookups.ModuleFilters.FilterBusinessObjectDefaults.Count);
		}

		public void TestShouldListUnpublishedLayouts_WhenTheirNamesDoNotCollideWithPublishedLayouts()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var component = BMSTestHelper.CreateBucket(system);
			var section = BMSTestHelper.CreateBoardSection(component);
			section.MS_SectionType = BMConstants.ModuleGridSectionType;

			var panelConfig = new ModuleGridSectionPanelConfiguration(section);
			panelConfig.ModuleName = ModuleIDs.ProcessHeader.Name;

			var publishedFilter = Factory.New<StmModuleFilter>();
			publishedFilter.S9_ModuleID = ModuleIDs.ProcessHeader.Name;
			publishedFilter.S9_IsPublished = true;
			publishedFilter.S9_FilterName = "Alpha";

			var unpublishedFilter = Factory.New<StmModuleFilter>();
			unpublishedFilter.S9_ModuleID = ModuleIDs.ProcessHeader.Name;
			unpublishedFilter.S9_IsPublished = false;
			unpublishedFilter.S9_FilterName = "Beta";

			var lookups = new ModuleGridSectionPanelConfigurationLookups(panelConfig);

			Factory.Save();

			var filters = lookups.ModuleFilters;

			AssertCollectionContains(publishedFilter, filters);
			AssertCollectionContains(unpublishedFilter, filters);
		}
	}
}
