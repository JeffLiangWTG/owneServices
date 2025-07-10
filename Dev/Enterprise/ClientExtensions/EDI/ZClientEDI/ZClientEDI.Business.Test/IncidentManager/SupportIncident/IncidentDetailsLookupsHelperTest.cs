using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.CustomerService.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	public class IncidentDetailsLookupsHelperTest : TestCaseWithFactory
	{
		public void TestGetProductList()
		{
			var expected = new ProductTypes();
			AssertArrayEqualsByElements(expected.ToArray(), IncidentDetailsLookupsHelper.ProductList.ToArray());
		}

		public void TestProductListShouldBeInAlphabeticalOrder()
		{
			var productCollection = new SystemProductCollection();
			var product1 = productCollection.AddNew();
			product1.Code = "ZZZ";
			product1.Description = "AAA Aardvark";
			product1.Enabled = true;

			var product2 = productCollection.AddNew();
			product2.Code = "AAA";
			product2.Description = "ZZZ Zebra";
			product2.Enabled = true;

			var emptyProductCollection = new SystemProductCollection();

			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, productCollection);
			EDIDataRegistry.Instance.ProductAreaIncidentCr8Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, emptyProductCollection);
			EDIDataRegistry.Instance.ProductAreaIncidentCr9Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, emptyProductCollection);
			var productList = IncidentDetailsLookupsHelper.ProductList;
			AssertEquals("Should be in alphabetical order", "AAA", productList[0].Code);
			AssertEquals("Should be in alphabetical order", "ZZZ Zebra", productList[0].Description);
			AssertEquals("Should be in alphabetical order", ProductTypes.Codes.Enterprise, productList[1].Code);
			AssertEquals("Should be in alphabetical order", ProductTypes.Descriptions.CargoWise, productList[1].Description);
			AssertEquals("Should be in alphabetical order", "ZZZ", productList[2].Code);
			AssertEquals("Should be in alphabetical order", "AAA Aardvark", productList[2].Description);
		}

		public void TestGetModuleList()
		{
			SetUpTestData();
			TestGetModuleListEnabledModulesOnly();
		}

		void SetUpTestData()
		{
			#region Menu Section Test Data
			{
				var menuSectionCollection = new SystemProductCollection();
				var entProduct = menuSectionCollection.AddNew("ENT", "ediEnterprise / CargoWise One", true);
				var ar1ModuleMapping = entProduct.ModuleMappings.AddNew("AR1", "ediArchiveManager (Enabled)", ProductAreaList.Codes.ARC, false);
				ar1ModuleMapping.SourceModuleMappings.AddNew("ARM", ProductAreaList.Codes.EDC);
				entProduct.ModuleMappings.AddNew("CCC", "Test Module C (Enabled)", ProductAreaList.Codes.CUS, false);
				entProduct.ModuleMappings.AddNew("DDD", "Test Module D (Enabled)", ProductAreaList.Codes.CUS, false);

				var glwProduct = menuSectionCollection.AddNew("GLW", "Glow", false);
				glwProduct.ModuleMappings.AddNew("WEB", "Web Module (Enabled)", ProductAreaList.Codes.PAV, false);

				var hubProduct = menuSectionCollection.AddNew("HUB", "eHub", false);

				EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, menuSectionCollection);
			}
			#endregion

			#region CR8 Test Data
			{
				var cr8Collection = new SystemProductCollection();
				var entProduct = cr8Collection.AddNew("ENT", "ediEnterprise / CargoWise One", true);
				entProduct.ModuleMappings.AddNew("8EA", "8EA (Disabled)", ProductAreaList.Codes.GEO, false, false, false);
				entProduct.ModuleMappings.AddNew("8EB", "8EB (Disabled)", ProductAreaList.Codes.CUS, false, false, false);

				var glwProduct = cr8Collection.AddNew("GLW", "Glow", true);
				var moduleMapping = glwProduct.ModuleMappings.AddNew("8GA", "8GA (Disabled)", ProductAreaList.Codes.CIL, false, false, false);
				moduleMapping.SourceModuleMappings.AddNew("WOW", ProductAreaList.Codes.FIN);
				glwProduct.ModuleMappings.AddNew("8GB", "8GB (Disabled)", ProductAreaList.Codes.CIL, false, false, false);

				var hubProduct = cr8Collection.AddNew("HUB", "eHub", false);

				EDIDataRegistry.Instance.ProductAreaIncidentCr8Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, cr8Collection);
			}
			#endregion

			#region CR9 Test Data
			{
				var cr9Collection = new SystemProductCollection();
				var entProduct = cr9Collection.AddNew("ENT", "ediEnterprise / CargoWise One", true);
				entProduct.ModuleMappings.AddNew("9EA", "9EA (Enabled)", ProductAreaList.Codes.RAT, false);
				entProduct.ModuleMappings.AddNew("9EB", "9EB (Enabled)", ProductAreaList.Codes.CRM, false);

				var glwProduct = cr9Collection.AddNew("GLW", "Glow", true);

				var hubProduct = cr9Collection.AddNew("HUB", "eHub", false);
				var moduleMapping = hubProduct.ModuleMappings.AddNew("8HA", "8HA (Disabled)", ProductAreaList.Codes.REF, false, false, false);
				moduleMapping.SourceModuleMappings.AddNew("BAM", ProductAreaList.Codes.HRM);
				hubProduct.ModuleMappings.AddNew("8HB", "8HB (Disabled)", ProductAreaList.Codes.REF, false, false, false);
				hubProduct.ModuleMappings.AddNew("8HC", "8HC (Disabled)", ProductAreaList.Codes.WFM, false, false, false);

				EDIDataRegistry.Instance.ProductAreaIncidentCr9Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, cr9Collection);
			}
			#endregion
		}

		void TestGetModuleListEnabledModulesOnly()
		{
			var incident = Factory.New<SupportIncident>();

			#region Menu Section
			{
				incident.IM_Priority = Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround;
				AssertEquals("Precondition", ModuleListType.MenuSection, incident.ModuleType);

				incident.IM_Product = "ENT";
				var expectedIncidentModuleList = new CodeDescriptionPairList();
				expectedIncidentModuleList.AddPair("CCC", "Test Module C (Enabled)");
				expectedIncidentModuleList.AddPair("DDD", "Test Module D (Enabled)");
				expectedIncidentModuleList.AddPair("AR1", "ediArchiveManager (Enabled)");

				AssertContainsExactElementsInAnyOrder(
					expectedIncidentModuleList,
					IncidentDetailsLookupsHelper.GetModuleListEnabledModulesOnly(incident).Cast<ICodeDescription>());

				expectedIncidentModuleList.Clear();

				incident.IM_Product = "GLW";
				expectedIncidentModuleList.AddPair("WEB", "Web Module (Enabled)");

				AssertContainsExactElementsInAnyOrder(
					expectedIncidentModuleList,
					IncidentDetailsLookupsHelper.GetModuleListEnabledModulesOnly(incident).Cast<ICodeDescription>());
				expectedIncidentModuleList.Clear();
			}
			#endregion

			#region CR8
			{
				incident.IM_Priority = Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR8_ComplianceRequirement;
				incident.IM_Product = "ENT";
				AssertEquals("Precondition", ModuleListType.Cr8, incident.ModuleType);
				var incidentModuleList = IncidentDetailsLookupsHelper.GetModuleListEnabledModulesOnly(incident);
				AssertEquals(incidentModuleList.Count, 0);

				incident.IM_Product = "GLW";
				incidentModuleList = IncidentDetailsLookupsHelper.GetModuleListEnabledModulesOnly(incident);
				AssertEquals(incidentModuleList.Count, 0);

				incident.IM_Product = "HUB";
				incidentModuleList = IncidentDetailsLookupsHelper.GetModuleListEnabledModulesOnly(incident);
				AssertEquals(incidentModuleList.Count, 0);
			}
			#endregion

			#region CR9
			{
				incident.IM_Priority = Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR9_CustomerServiceRequest;
				AssertEquals("Precondition", ModuleListType.Cr9, incident.ModuleType);
				incident.IM_Product = "ENT";
				var expectedIncidentModuleList = new CodeDescriptionPairList();
				expectedIncidentModuleList.AddPair("9EA", "9EA (Enabled)");
				expectedIncidentModuleList.AddPair("9EB", "9EB (Enabled)");

				AssertContainsExactElementsInAnyOrder(
					expectedIncidentModuleList,
					IncidentDetailsLookupsHelper.GetModuleListEnabledModulesOnly(incident).Cast<ICodeDescription>());
				expectedIncidentModuleList.Clear();

				incident.IM_Product = "GLW";
				var incidentModuleList = IncidentDetailsLookupsHelper.GetModuleListEnabledModulesOnly(incident);
				AssertEquals(incidentModuleList.Count, 0);

				incident.IM_Product = "HUB";
				incidentModuleList = IncidentDetailsLookupsHelper.GetModuleListEnabledModulesOnly(incident);
				AssertEquals(incidentModuleList.Count, 0);
			}
			#endregion

			#region Unspecified
			{
				incident.IM_Priority = ZString.Empty;
				AssertEquals("Precondition", ModuleListType.Unspecified, incident.ModuleType);

				incident.IM_Product = "ENT";
				var expectedENTIncidentModuleList = new CodeDescriptionPairList();
				expectedENTIncidentModuleList.AddPair("9EA", "9EA (Enabled)");
				expectedENTIncidentModuleList.AddPair("9EB", "9EB (Enabled)");
				expectedENTIncidentModuleList.AddPair("AR1", "ediArchiveManager (Enabled)");
				expectedENTIncidentModuleList.AddPair("CCC", "Test Module C (Enabled)");
				expectedENTIncidentModuleList.AddPair("DDD", "Test Module D (Enabled)");
				AssertContainsExactElementsInAnyOrder(
					expectedENTIncidentModuleList,
					IncidentDetailsLookupsHelper.GetModuleListEnabledModulesOnly(incident).Cast<ICodeDescription>());

				incident.IM_Product = "GLW";
				var expectedGlowIncidentModuleList = new CodeDescriptionPairList();
				expectedGlowIncidentModuleList.AddPair("WEB", "Web Module (Enabled)");
				AssertContainsExactElementsInAnyOrder(
					expectedGlowIncidentModuleList,
					IncidentDetailsLookupsHelper.GetModuleListEnabledModulesOnly(incident).Cast<ICodeDescription>());

				incident.IM_Product = "HUB";
				AssertEquals(IncidentDetailsLookupsHelper.GetModuleListEnabledModulesOnly(incident).Count, 0);
			}
			#endregion
		}

		public void TestGetModuleListEnabledModulesOnly_EnabledByCriticality()
		{
			var menuSectionCollection = new SystemProductCollection();
			var entProduct = menuSectionCollection.AddNew("ENT", "ediEnterprise / CargoWise One", true);
			entProduct.ModuleMappings.AddNew("CCC", "Test Module C (Enabled)", ProductAreaList.Codes.CUS, false);

			var glwProduct = menuSectionCollection.AddNew("NEW", "NEW", false);
			glwProduct.ModuleMappings.AddNew("XXX", "XXX Module", ProductAreaList.Codes.PAV, false, false, false); // disabled

			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, menuSectionCollection);

			var cr9Collection = new SystemProductCollection();
			var entProductCR9 = cr9Collection.AddNew("ENT", "ediEnterprise / CargoWise One", true);
			entProductCR9.ModuleMappings.AddNew("9EA", "9EA (Enabled)", ProductAreaList.Codes.RAT, false);

			var glwProductCR9 = cr9Collection.AddNew("NEW", "NEW", true);
			glwProductCR9.ModuleMappings.AddNew("XXX", "XXX Module", ProductAreaList.Codes.PAV, false, false, true); // enabled

			EDIDataRegistry.Instance.ProductAreaIncidentCr9Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, cr9Collection);

			var incident = Factory.New<SupportIncident>();
			incident.IM_Priority = Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR9_CustomerServiceRequest;
			AssertEquals("Precondition", ModuleListType.Cr9, incident.ModuleType);
			incident.IM_Product = "NEW";
			AssertEquals(1, IncidentDetailsLookupsHelper.GetModuleListEnabledModulesOnly(incident).Count);
			AssertEquals("XXX", IncidentDetailsLookupsHelper.GetModuleListEnabledModulesOnly(incident)[0].Code);
		}

		public void TestGetServiceTypeList_GetRightCollection()
		{
			SetupServiceTypeTestData();
			var incident = Factory.New<SupportIncident>();
			incident.IM_Product = "AAA";

			#region Service Type
			{
				incident.IM_Priority = Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround;
				incident.IM_ProgramArea = "XRM";
				incident.IM_Module = "AA1";

				var serviceTypeList = IncidentDetailsLookupsHelper.GetServiceTypeList(incident);
				AssertEquals("TEA", serviceTypeList[0].Code);
				AssertEquals("Tear down", serviceTypeList[0].Description);
			}
			#endregion

			#region CR8
			{
				incident.IM_Priority = Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR8_ComplianceRequirement;
				incident.IM_ProgramArea = "XRM";
				incident.IM_Module = "AA1";

				var serviceTypeList = IncidentDetailsLookupsHelper.GetServiceTypeList(incident);
				AssertEquals("CON", serviceTypeList[0].Code);
				AssertEquals("Configuration", serviceTypeList[0].Description);
			}
			#endregion

			#region CR9
			{
				incident.IM_Priority = Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR9_CustomerServiceRequest;
				incident.IM_ProgramArea = "XRM";
				incident.IM_Module = "AA1";

				var serviceTypeList = IncidentDetailsLookupsHelper.GetServiceTypeList(incident);
				AssertEquals("DEP", serviceTypeList[0].Code);
				AssertEquals("Deployment", serviceTypeList[0].Description);
			}
			#endregion
		}

		void SetupServiceTypeTestData()
		{
			var areas = new CodeDescriptionPairList();
			areas.AddPair("XRM", "XRM");
			EDIDataRegistry.Instance.ProductAreas.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, areas);

			// setup for ServiceTypeProductAreaModuleMappingLookups.Modules
			var collection1 = new SystemProductCollection();
			var product1 = collection1.AddNew("AAA", "AAA", true);
			product1.ModuleMappings.AddNew("AA1", "AA1 Description", "XRM", true);
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection1);

			#region Service Type Test Data
			{
				var collectionServiceType = new SystemProductCollection();
				var parent1 = collectionServiceType.AddNew();
				parent1.Code = "AAA";
				parent1.Description = "Service Type Mapping";
				var child1 = parent1.ServiceTypeModuleMappings.AddNew();
				child1.ProductArea = "XRM";
				child1.ModuleCode = "AA1";
				child1.ModuleDescription = "des1";
				child1.ServiceTypeMappings.AddNew("TEA");
				EDIDataRegistry.Instance.ServiceTypeMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collectionServiceType);
			}
			#endregion

			#region CR8 Test Data
			{
				var collectionCr8 = new SystemProductCollection();
				var parentCr8 = collectionCr8.AddNew();
				parentCr8.Code = "AAA";
				parentCr8.Description = "CR8 Desc";
				var child1 = parentCr8.ServiceTypeModuleMappings.AddNew();
				child1.ProductArea = "XRM";
				child1.ModuleCode = "AA1";
				child1.ModuleDescription = "des1";
				child1.ServiceTypeMappings.AddNew("CON");
				EDIDataRegistry.Instance.ServiceTypeCr8Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collectionCr8);
			}
			#endregion

			#region CR9 Test Data
			{
				var collectionCr9 = new SystemProductCollection();
				var parentCr9 = collectionCr9.AddNew();
				parentCr9.Code = "AAA";
				parentCr9.Description = "CR9 Desc";
				var child1 = parentCr9.ServiceTypeModuleMappings.AddNew();
				child1.ProductArea = "XRM";
				child1.ModuleCode = "AA1";
				child1.ModuleDescription = "des1";
				child1.ServiceTypeMappings.AddNew("DEP");
				EDIDataRegistry.Instance.ServiceTypeCr9Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collectionCr9);
			}
			#endregion
		}
	}
}
