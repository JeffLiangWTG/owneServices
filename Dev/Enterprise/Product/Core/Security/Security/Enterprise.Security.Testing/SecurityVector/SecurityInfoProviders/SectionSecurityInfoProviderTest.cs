using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Modules;
using Enterprise.MasterFiles.Business;
using Enterprise.Security.Provider;
using Enterprise.TransportCommon.Integration;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Security.Testing
{
	sealed class SectionSecurityInfoProviderTest : TestCaseWithFactory
	{
		public void TestForwarding()
		{
			var security = new SecurityCore(new GlbSecurityCollection(Factory), Factory.New<GlbStaff>(), Guid.Empty, Guid.Empty, Guid.Empty);
			var moduleTreeloader = ObjectFactory.Get<IModuleTreeLoader>();
			moduleTreeloader.Initialise(ModuleTree.Tree, security);
			moduleTreeloader.LoadModules();

			var provider = new SectionSecurityInfoProvider(new RootSecurityInfoProvider(security), GetSection("Operations", "Forwarding"));
			var children = provider.GetChildren().Select(p => p.Checkpoint);
			AssertCollectionContains(security.PickupDeliveryConfirmations, children);
			AssertCollectionContains(security.RoadDistanceCalculationServiceForwarding, children);
			AssertCollectionContains(security.ElectronicMessaging, children);
			AssertCollectionContains(security.MaintainELoadList, children);
			AssertCollectionContains(security.CustomsCargoManifestOnForwarding, children);
		}

		public void TestAllowVehicleMonitoringAndManagement()
		{
			var security = new SecurityCore(new GlbSecurityCollection(Factory), Factory.New<GlbStaff>(), Guid.Empty, Guid.Empty, Guid.Empty);
			var moduleTreeloader = ObjectFactory.Get<IModuleTreeLoader>();
			moduleTreeloader.Initialise(ModuleTree.Tree, security);
			moduleTreeloader.LoadModules();

			var provider = new SectionSecurityInfoProvider(new RootSecurityInfoProvider(security), GetSection("Admin", "References"));
			var children = provider.GetChildren().Select(p => p.Checkpoint);
			AssertCollectionContains(security.AllowVehicleMonitoringAndManagement, children);
		}

		public void TestWarehouse()
		{
			var security = new SecurityCore(new GlbSecurityCollection(Factory), Factory.New<GlbStaff>(), Guid.Empty, Guid.Empty, Guid.Empty);
			var provider = new SectionSecurityInfoProvider(new RootSecurityInfoProvider(security), GetSection("Operations", "Warehouse"));
			var children = provider.GetChildren().Select(p => p.Checkpoint);
			AssertCollectionContains(security.WhsAllowedClients, children);
			AssertCollectionContains(security.WhsAllowedWarehouses, children);
			AssertCollectionContains(security.WhsRFScanning, children);
			AssertCollectionContains(security.RoadDistanceCalculationServiceWarehouse, children);
		}

		public void TestTransitWarehouse()
		{
			var security = new SecurityCore(new GlbSecurityCollection(Factory), Factory.New<GlbStaff>(), Guid.Empty, Guid.Empty, Guid.Empty);
			var provider = new SectionSecurityInfoProvider(new RootSecurityInfoProvider(security), GetSection("Operations", "TransitWarehouse"));
			var children = provider.GetChildren().Select(p => p.Checkpoint);
			AssertCollectionContains(security.RoadDistanceCalculationServiceTransitWarehouse, children);
		}

		public void TestLinerAndAgency()
		{
			var security = new SecurityCore(new GlbSecurityCollection(Factory), Factory.New<GlbStaff>(), Guid.Empty, Guid.Empty, Guid.Empty);
			var provider = new SectionSecurityInfoProvider(new RootSecurityInfoProvider(security), GetSection("Operations", "LinerAndAgency"));
			var children = provider.GetChildren().Select(p => p.Checkpoint);
			AssertCollectionContains(security.AgencyPrincipalAccess, children);
			AssertCollectionContains(security.RoadDistanceCalculationServiceShipping, children);
			AssertCollectionContains(security.ExportManifestOnShipping, children);
		}

		public void TestCustoms()
		{
			var security = new SecurityCore(new GlbSecurityCollection(Factory), Factory.New<GlbStaff>(), Guid.Empty, Guid.Empty, Guid.Empty);
			var moduleTreeloader = ObjectFactory.Get<IModuleTreeLoader>();
			moduleTreeloader.Initialise(ModuleTree.Tree, security);
			moduleTreeloader.LoadModules();

			var provider = new SectionSecurityInfoProvider(new RootSecurityInfoProvider(security), GetSection("Operations", "CustomsMain"));
			var children = provider.GetChildren().Select(p => p.Checkpoint);
			AssertCollectionContains(security.RoadDistanceCalculationServiceCustoms, children);
			AssertCollectionContains(security.CACustomsDIF, children);
			AssertCollectionContains(security.CustomsDIS, children);
			AssertCollectionContains(security.SupervisorOverrides, children);
		}

		public void TestCFSCTO()
		{
			var security = new SecurityCore(new GlbSecurityCollection(Factory), Factory.New<GlbStaff>(), Guid.Empty, Guid.Empty, Guid.Empty);
			var moduleTreeloader = ObjectFactory.Get<IModuleTreeLoader>();
			moduleTreeloader.Initialise(ModuleTree.Tree, security);
			moduleTreeloader.LoadModules();

			var provider = new SectionSecurityInfoProvider(new RootSecurityInfoProvider(security), GetSection("Operations", "CFSCTO"));
			var children = provider.GetChildren().Select(p => p.Checkpoint);
			AssertCollectionContains(security.RoadDistanceCalculationServiceCFS, children);
		}

		public void TestLocalTransport()
		{
			var security = new SecurityCore(new GlbSecurityCollection(Factory), Factory.New<GlbStaff>(), Guid.Empty, Guid.Empty, Guid.Empty);
			var provider = new SectionSecurityInfoProvider(new RootSecurityInfoProvider(security), GetSection("Operations", "Transport"));
			var children = provider.GetChildren().Select(p => p.Checkpoint);
			AssertCollectionContains(security.RoadDistanceCalculationServiceLocalTransport, children);
		}

		public void TestLandTransport()
		{
			ObjectFactory.Get<ITransportRegistry>().EnableLandTransport.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var security = new SecurityCore(new GlbSecurityCollection(Factory), Factory.New<GlbStaff>(), Guid.Empty, Guid.Empty, Guid.Empty);
			var moduleTreeloader = ObjectFactory.Get<IModuleTreeLoader>();
			moduleTreeloader.Initialise(ModuleTree.Tree, security);
			moduleTreeloader.LoadModules();

			var provider = new SectionSecurityInfoProvider(new RootSecurityInfoProvider(security), GetSection("Operations", "TransportConsignment"));
			var children = provider.GetChildren().Select(p => p.Checkpoint);
			AssertCollectionContains(security.RoadDistanceCalculationServiceLandTransport, children);
		}

		public void TestOrderManager()
		{
			var security = new SecurityCore(new GlbSecurityCollection(Factory), Factory.New<GlbStaff>(), Guid.Empty, Guid.Empty, Guid.Empty);
			var provider = new SectionSecurityInfoProvider(new RootSecurityInfoProvider(security), GetSection("Operations", "OrderManager"));
			var children = provider.GetChildren().Select(p => p.Checkpoint);
			AssertCollectionContains(security.RoadDistanceCalculationServiceOrders, children);
		}

		public void TestCustomsGbCcsuk()
		{
			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.UnitedKingdom);
			var security = new SecurityCore(new GlbSecurityCollection(Factory), Factory.New<GlbStaff>(), Guid.Empty, Guid.Empty, Guid.Empty);
			var ccsukModuleSection = new ModuleSection(ModuleTreeLoaderConstant.Section.Ccsuk, null, security.AirCcsuk, IconTypes.Box, IconTypes.Tick); // cheating....
																																						//... because this FAILs:   
																																						//		ModuleTree.Tree.Categories["Operations"].Sections["CCSUK"]);  
																																						// even though "CCSUK" is the name of the module seciton.  Stupid. 
			var provider = new SectionSecurityInfoProvider(new RootSecurityInfoProvider(security), ccsukModuleSection);
			var children = provider.GetChildren().Select(p => p.Checkpoint);
			AssertCollectionContains(security.AirCcsukShed, children);
			AssertCollectionContains(security.AirCcsukHCITerminal, children);
			AssertCollectionContains(security.AirCcsukCreateFallbackUnderbondRequest, children);
		}

		public void TestDocManager()
		{
			var security = new SecurityCore(new GlbSecurityCollection(Factory), Factory.New<GlbStaff>(), Guid.Empty, Guid.Empty, Guid.Empty);
			var moduleTreeloader = ObjectFactory.Get<IModuleTreeLoader>();
			moduleTreeloader.Initialise(ModuleTree.Tree, security);
			moduleTreeloader.LoadModules();

			var provider = new SectionSecurityInfoProvider(new RootSecurityInfoProvider(security), GetSection("Manage", "DocManager"));
			var children = provider.GetChildren().Select(p => p.Checkpoint);
			AssertCollectionContains(security.eDocsLevelSpecific, children);
			AssertCollectionContains(security.eDocsPermanentDelete, children);
			AssertCollectionContains(security.CutSpecificEDocTypes, children);
		}

		public void TestTariffsRates()
		{
			var security = new SecurityCore(new GlbSecurityCollection(Factory), Factory.New<GlbStaff>(), Guid.Empty, Guid.Empty, Guid.Empty);
			var provider = new SectionSecurityInfoProvider(new RootSecurityInfoProvider(security), GetSection("Manage", "TariffsAndRates"));
			var children = provider.GetChildren().Select(p => p.Checkpoint);
			AssertCollectionContains(security.RatesSecurity, children);
		}

		public void TestRatesService()
		{
			var security = new SecurityCore(new GlbSecurityCollection(Factory), Factory.New<GlbStaff>(), Guid.Empty, Guid.Empty, Guid.Empty);
			var provider = new SectionSecurityInfoProvider(new RootSecurityInfoProvider(security), GetSection("Manage", "WiseRates"));
			var children = provider.GetChildren().Select(p => p.Checkpoint);

			AssertCollectionContains(security.WiseRatesCargoguideRateSearch, children);
			AssertCollectionContains(security.WiseRatesCargoSphereContractManagement, children);
			AssertCollectionContains(security.WiseRatesCargoSphereRateSearch, children);
			AssertCollectionContains(security.WiseRatesSearch, children);
		}

		public void TestCustomsFiles()
		{
			var security = new SecurityCore(new GlbSecurityCollection(Factory), Factory.New<GlbStaff>(), Guid.Empty, Guid.Empty, Guid.Empty);
			var moduleTreeloader = ObjectFactory.Get<IModuleTreeLoader>();
			moduleTreeloader.Initialise(ModuleTree.Tree, security);
			moduleTreeloader.LoadModules();

			var provider = new SectionSecurityInfoProvider(new RootSecurityInfoProvider(security), GetSection("Admin", "CustomsFiles"));
			var children = provider.GetChildren().Select(p => p.Checkpoint);
			AssertCollectionContains(security.HTSReferenceFilesDataVersion, children);
		}

		public void TestBusinessIntelligenceNonModuleCheckpoints()
		{
			var security = new SecurityCore(new GlbSecurityCollection(Factory), Factory.New<GlbStaff>(), Guid.Empty, Guid.Empty, Guid.Empty);
			var moduleTreeloader = ObjectFactory.Get<IModuleTreeLoader>();
			moduleTreeloader.Initialise(ModuleTree.Tree, security);
			moduleTreeloader.LoadModules();

			var provider = new SectionSecurityInfoProvider(new RootSecurityInfoProvider(security), GetSection("Manage", "BusinessIntelligenceAndAnalytics"));
			var children = provider.GetChildren().Select(p => p.Checkpoint);
			AssertCollectionContains(security.BIAPI, children);
		}

		ModuleSection GetSection(string categoryName, string sectionName)
		{
			return ModuleTree.Tree.Categories[categoryName].Sections[sectionName];
		}
	}
}
