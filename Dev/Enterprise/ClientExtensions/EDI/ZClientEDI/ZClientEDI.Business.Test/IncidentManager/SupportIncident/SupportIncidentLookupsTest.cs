using System;
using System.Collections;
using System.Linq;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.Registry;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.CustomerService.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ProcessManagement.Business;
using Enterprise.ProcessManagement.Business.Test;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	sealed class SupportIncidentLookupsTest : IncidentMainLookupsTestCase
	{
		public void TestContacts()
		{
			OrgHeader org1 = Factory.New<OrgHeader>();
			org1.Contacts.AddNew();
			org1.Contacts.AddNew();

			OrgHeader org2 = Factory.New<OrgHeader>();
			org2.Contacts.AddNew();
			org2.Contacts.AddNew();
			org2.Contacts.AddNew();
			OrgContact inactiveContact = org2.Contacts.AddNew();
			inactiveContact.OC_IsActive = false;

			var incident = GetIncident();

			AssertNull(incident.Lookups.ContactList.Master);
			incident.Lookups.ContactList.Load();
			AssertEquals(0, incident.Lookups.ContactList.Count);

			incident.IM_OH_Client = org1.PK;
			AssertNotNull(incident.Lookups.ContactList.Master);
			incident.Lookups.ContactList.Load();
			AssertEquals(2, incident.Lookups.ContactList.Count);

			incident.IM_OH_Client = org2.PK;
			AssertNotNull(incident.Lookups.ContactList.Master);
			incident.Lookups.ContactList.Load();
			AssertEquals(3, incident.Lookups.ContactList.Count);

			incident.IM_OH_Client = ZGuid.Empty;
			AssertNull(incident.Lookups.ContactList.Master);
			incident.Lookups.ContactList.Load();
			AssertEquals(0, incident.Lookups.ContactList.Count);
		}

		public void TestGetProductList()
		{
			var incident = GetIncident();
			var expected = new ProductTypes();
			AssertArrayEqualsByElements(expected.ToArray(), incident.Lookups.ProductList.ToArray());
		}

		public void TestProductListIsCached()
		{
			var incident = GetIncident();
			Factory.Save();
			var productList = incident.Lookups.ProductList;
			AssertNotNull("Precondition:", productList);
			AssertEquals("ProductList should be cached.", productList, Factory.GetCachedValue<CodeDescriptionPairList>("SupportIncidentLookups.ProductList", () => null));
		}

		public void TestProductListShouldBeInAlphabeticalOrder()
		{
			var incident = GetIncident();
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
			var productList = incident.Lookups.ProductList;
			AssertEquals("Should be in alphabetical order", "AAA", productList[0].Code);
			AssertEquals("Should be in alphabetical order", "ZZZ Zebra", productList[0].Description);
			AssertEquals("Should be in alphabetical order", ProductTypes.Codes.Enterprise, productList[1].Code);
			AssertEquals("Should be in alphabetical order", ProductTypes.Descriptions.CargoWise, productList[1].Description);
			AssertEquals("Should be in alphabetical order", "ZZZ", productList[2].Code);
			AssertEquals("Should be in alphabetical order", "AAA Aardvark", productList[2].Description);
		}

		public void TestProfessionalServicesQuoteList()
		{
			var incident = GetIncident();
			AssertNotNull(incident.Lookups.ProfessionalServicesQuoteList);
		}

		public void TestProductAreaSortedAlphabetically()
		{
			var productAreasList = new CodeDescriptionPairList();
			productAreasList.AddPair("CFB", "C Fruits Area B");
			productAreasList.AddPair("CFA", "C Fruits Area A");
			productAreasList.AddPair("AFA", "A Fruits Area A");
			productAreasList.AddPair("AFB", "A Fruits Area B");
			EDIDataRegistry.Instance.ProductAreas.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, productAreasList);

			var collection = new SystemProductCollection();
			var products = collection.AddNew(ProductTypes.Codes.Enterprise, "ZZ", true);
			products.ModuleMappings.AddNew("TOM", "Tomatoes", "CFB", false);
			products.ModuleMappings.AddNew("APP", "Apples", "CFA", false);
			products.ModuleMappings.AddNew("PER", "Pears", "AFA", false);
			products.ModuleMappings.AddNew("BAN", "Bannanas", "AFB", false);
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			var incident = Factory.New<SupportIncident>();
			incident.IM_Product = "ENT";
			incident.IM_Priority = "CR4";
			incident.IM_Module = "APP";
			var codeList = incident.Lookups.FilteredProductAreaList.Cast<ICodeDescription>().Select(x => x.Code);
			bool isSortedAlphabetically = codeList.SequenceEqual(codeList.OrderBy(s => s, StringComparer.OrdinalIgnoreCase), StringComparer.OrdinalIgnoreCase);

			Assert("The output of the code should be in alphabetical order", isSortedAlphabetically);
		}

		public void TestProductAreaLookupShouldIncludeTriageNodeProductArea()
		{
			var productAreasList = new CodeDescriptionPairList();
			productAreasList.AddPair("APP", "Apple");
			productAreasList.AddPair("BAN", "Banana");
			productAreasList.AddPair("ORA", "Orange");
			EDIDataRegistry.Instance.ProductAreas.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, productAreasList);

			var collection = new SystemProductCollection();
			var products = collection.AddNew(ProductTypes.Codes.Enterprise, "ZZ", isProductReadOnly: true);
			products.ModuleMappings.AddNew("RED", "Red", "APP", isModuleReadOnly: false);
			products.ModuleMappings.AddNew("YEL", "Yellow", "BAN", isModuleReadOnly: false);
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			var triage = Factory.NewWithValidTestData<IncidentTriage>();
			triage.IMT_Type = IncidentTriageTypes.Codes.Support;
			triage.IMT_Module = "RED";
			triage.IMT_Product = "ENT";
			triage.IMT_ProductArea = "ORA";

			var incident = Factory.New<SupportIncident>();
			incident.IM_Product = "ENT";
			incident.IM_Priority = "CR4";
			incident.IM_Module = "RED";

			Assert("Should not include ORA since it doesn't have a module mapping for the ENT product", !incident.Lookups.FilteredProductAreaList.ContainsCode("ORA"));

			incident.IM_IMT_Triage = triage.PK;

			Assert("Should include ORA since it's on the associated triage", incident.Lookups.FilteredProductAreaList.ContainsCode("ORA"));

			Factory.ClearCachedValue<CodeDescriptionPairList>("SupportIncidentLookups.GetFilteredProductAreaList" + incident.ModuleType + incident.IM_Product + triage.PK);
			triage.IMT_SetProductAreaByMenuItem = true;
			AssertEquals("Precondition", string.Empty, triage.IMT_ProductArea);
			Assert("Should not include add empty string if triage product area is empty", !incident.Lookups.FilteredProductAreaList.ContainsCode(string.Empty));
		}

		public void TestModuleListEnabledModulesOnly_ShouldIncludeTriageModuleIfProductProductAreaAndCriticalityMatchTriage()
		{
			var productAreasList = new CodeDescriptionPairList();
			productAreasList.AddPair("AAA", "Apple");
			productAreasList.AddPair("BBB", "Banana");
			productAreasList.AddPair("CCC", "Carrot");
			EDIDataRegistry.Instance.ProductAreas.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, productAreasList);

			var sourceModules = new SourceModuleCollection();
			sourceModules.AddNew("SourceModule1", "source module 1", "Dummies >", ModuleListType.MenuSection, "DUM", isSelectableForOverride: true, isSearchable: true, "ENT");
			EDIDataRegistry.Instance.SourceModules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, sourceModules);

			var collection = new SystemProductCollection();
			var products = collection.AddNew(ProductTypes.Codes.Enterprise, "ZZ", isProductReadOnly: true);
			var redModuleMapping = products.ModuleMappings.AddNew("RED", "Red", "AAA", isModuleReadOnly: false);
			products.ModuleMappings.AddNew("YEL", "Yellow", "CCC", isModuleReadOnly: false, isEnabled: false);
			redModuleMapping.SourceModuleMappings.AddNew("SourceModule1", "BBB");
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			var triage = Factory.NewWithValidTestData<IncidentTriage>();
			triage.IMT_Type = IncidentTriageTypes.Codes.Support;
			triage.IMT_Module = "RED";
			triage.IMT_Product = "ENT";
			triage.IMT_ProductArea = "CCC";

			var incident = Factory.New<SupportIncident>();
			incident.IM_Product = "ENT";
			incident.IM_Priority = Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround;
			incident.IM_Module = "RED";
			Factory.Save();

			incident.IM_ProgramArea = triage.IMT_ProductArea;
			AssertEquals("Module Lookup list should not include module from triage since it hasn't been attached yet", false, incident.Lookups.ModuleListEnabledModulesOnly.ContainsCode("RED"));

			incident.IM_IMT_Triage = triage.PK;

			AssertEquals("Precondition", triage.IMT_ProductArea, incident.IM_ProgramArea);
			AssertEquals("Module Lookup list should include module from triage since product, product area and criticality match", true, incident.Lookups.ModuleListEnabledModulesOnly.ContainsCode("RED"));

			triage.IMT_Module = "YEL";
			AssertEquals("Module Lookup list should not include disabled module", false, incident.Lookups.ModuleListEnabledModulesOnly.ContainsCode("YEL"));

			triage.IMT_Module = "RED";

			incident.IM_Priority = Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR1_SystemDown;
			AssertEquals("Module Lookup list should include module from triage since product, product area and criticality match", true, incident.Lookups.ModuleListEnabledModulesOnly.ContainsCode("RED"));

			incident.IM_ProgramArea = "ZZZ";
			AssertEquals("Module Lookup list should not include module from triage since product areas don't match", false, incident.Lookups.ModuleListEnabledModulesOnly.ContainsCode("RED"));

			incident.IM_Product = "ZZZ";
			incident.IM_ProgramArea = triage.IMT_ProductArea;
			AssertEquals("Module Lookup list should not include module from triage since product does not match triage", false, incident.Lookups.ModuleListEnabledModulesOnly.ContainsCode("RED"));

			incident.IM_Product = "ENT";
			incident.IM_Priority = Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR8_ComplianceRequirement;
			incident.IM_ProgramArea = triage.IMT_ProductArea;
			AssertEquals("Precondition", triage.IMT_ProductArea, incident.IM_ProgramArea);
			AssertEquals("Module Lookup list should not include module from triage since criticality does not match triage", false, incident.Lookups.ModuleListEnabledModulesOnly.ContainsCode("RED"));

			incident.IM_Priority = Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR9_CustomerServiceRequest;
			incident.IM_ProgramArea = triage.IMT_ProductArea;
			AssertEquals("Precondition", triage.IMT_ProductArea, incident.IM_ProgramArea);
			AssertEquals("Module Lookup list should not include module from triage since criticality does not match triage", false, incident.Lookups.ModuleListEnabledModulesOnly.ContainsCode("RED"));

			triage.IMT_Type = IncidentTriageTypes.Codes.Service;
			AssertEquals("Module Lookup list should include module from triage since product, product area and criticality match", true, incident.Lookups.ModuleListEnabledModulesOnly.ContainsCode("RED"));

			triage.IMT_Type = IncidentTriageTypes.Codes.Compliance;
			incident.IM_Priority = Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR8_ComplianceRequirement;
			incident.IM_ProgramArea = triage.IMT_ProductArea;
			AssertEquals("Precondition", triage.IMT_ProductArea, incident.IM_ProgramArea);
			AssertEquals("Module Lookup list should include module from triage since product, product area and criticality match", true, incident.Lookups.ModuleListEnabledModulesOnly.ContainsCode("RED"));

			incident.IM_Priority = Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR2_ModuleDown;
			incident.IM_ProgramArea = "BBB";
			AssertEquals("Module Lookup list should include module since it matches the menu item", true, incident.Lookups.ModuleListEnabledModulesOnly.ContainsCode("RED"));

			incident.IM_ProgramArea = "AAA";
			AssertEquals("Module Lookup list should include module since it matches the default module mapping", true, incident.Lookups.ModuleListEnabledModulesOnly.ContainsCode("RED"));

			triage.IMT_SetProductAreaByMenuItem = true;
			incident.IM_Priority = Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR8_ComplianceRequirement;
			incident.IM_ProgramArea = triage.IMT_ProductArea;
			AssertEquals("Precondition", string.Empty, triage.IMT_ProductArea);
			AssertEquals("Module Lookup list should not include module since product area is empty and CR8 list does not include RED", false, incident.Lookups.ModuleListEnabledModulesOnly.ContainsCode("RED"));
		}

		public void TestProductAreaIndependentModuleList_ShouldIncludeTriageModuleIfProductProductAreaAndCriticalityMatchTriage()
		{
			var productAreasList = new CodeDescriptionPairList();
			productAreasList.AddPair("AAA", "Apple");
			productAreasList.AddPair("BBB", "Banana");
			productAreasList.AddPair("CCC", "Carrot");
			EDIDataRegistry.Instance.ProductAreas.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, productAreasList);

			var sourceModules = new SourceModuleCollection();
			sourceModules.AddNew("SourceModule1", "source module 1", "Dummies >", ModuleListType.MenuSection, "DUM", isSelectableForOverride: true, isSearchable: true, "ENT");
			EDIDataRegistry.Instance.SourceModules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, sourceModules);

			var collection = new SystemProductCollection();
			var products = collection.AddNew(ProductTypes.Codes.Enterprise, "ZZ", isProductReadOnly: true);
			var redModuleMapping = products.ModuleMappings.AddNew("RED", "Red", "AAA", isModuleReadOnly: false);
			redModuleMapping.SourceModuleMappings.AddNew("SourceModule1", "BBB");
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			var triage = Factory.NewWithValidTestData<IncidentTriage>();
			triage.IMT_Type = IncidentTriageTypes.Codes.Support;
			triage.IMT_Module = "RED";
			triage.IMT_Product = "ENT";
			triage.IMT_ProductArea = "CCC";

			var incident = Factory.New<SupportIncident>();
			incident.IM_Product = "ENT";
			incident.IM_Priority = Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround;
			incident.IM_Module = "RED";
			Factory.Save();

			incident.IM_ProgramArea = triage.IMT_ProductArea;
			incident.IM_IMT_Triage = triage.PK;

			AssertEquals("Precondition", triage.IMT_ProductArea, incident.IM_ProgramArea);
			AssertEquals("Module Lookup list should include module from triage since product, product area and criticality match", true, incident.Lookups.ProductAreaIndependentModuleList.ContainsCode("RED"));

			incident.IM_Priority = Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR1_SystemDown;
			AssertEquals("Module Lookup list should include module from triage since product, product area and criticality match", true, incident.Lookups.ProductAreaIndependentModuleList.ContainsCode("RED"));

			incident.IM_Product = "ZZZ";
			incident.IM_ProgramArea = triage.IMT_ProductArea;
			AssertEquals("Module Lookup list should not include module from triage since product does not match triage", false, incident.Lookups.ProductAreaIndependentModuleList.ContainsCode("RED"));

			incident.IM_Product = "ENT";
			incident.IM_Priority = Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR8_ComplianceRequirement;
			incident.IM_ProgramArea = triage.IMT_ProductArea;
			AssertEquals("Precondition", triage.IMT_ProductArea, incident.IM_ProgramArea);
			AssertEquals("Module Lookup list should not include module from triage since criticality does not match triage", false, incident.Lookups.ProductAreaIndependentModuleList.ContainsCode("RED"));

			incident.IM_Priority = Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR9_CustomerServiceRequest;
			incident.IM_ProgramArea = triage.IMT_ProductArea;
			AssertEquals("Precondition", triage.IMT_ProductArea, incident.IM_ProgramArea);
			AssertEquals("Module Lookup list should not include module from triage since criticality does not match triage", false, incident.Lookups.ProductAreaIndependentModuleList.ContainsCode("RED"));

			triage.IMT_Type = IncidentTriageTypes.Codes.Service;
			AssertEquals("Module Lookup list should include module from triage since product, product area and criticality match", true, incident.Lookups.ProductAreaIndependentModuleList.ContainsCode("RED"));

			triage.IMT_Type = IncidentTriageTypes.Codes.Compliance;
			incident.IM_Priority = Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR8_ComplianceRequirement;
			incident.IM_ProgramArea = triage.IMT_ProductArea;
			AssertEquals("Precondition", triage.IMT_ProductArea, incident.IM_ProgramArea);
			AssertEquals("Module Lookup list should include module from triage since product, product area and criticality match", true, incident.Lookups.ProductAreaIndependentModuleList.ContainsCode("RED"));

			incident.IM_Priority = Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR2_ModuleDown;
			incident.IM_ProgramArea = "BBB";
			AssertEquals("Module Lookup list should include module since it matches the menu item", true, incident.Lookups.ProductAreaIndependentModuleList.ContainsCode("RED"));

			incident.IM_ProgramArea = "AAA";
			AssertEquals("Module Lookup list should include module since it matches the default module mapping", true, incident.Lookups.ProductAreaIndependentModuleList.ContainsCode("RED"));

			triage.IMT_SetProductAreaByMenuItem = true;
			incident.IM_Priority = Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR8_ComplianceRequirement;
			incident.IM_ProgramArea = triage.IMT_ProductArea;
			AssertEquals("Precondition", string.Empty, triage.IMT_ProductArea);
			AssertEquals("Module Lookup list should include module since product area is empty and CR8 list does not include RED", false, incident.Lookups.ProductAreaIndependentModuleList.ContainsCode("RED"));
		}

		protected sealed override AutoIncidentMain GetNewIncidentMain()
		{
			return GetIncident();
		}

		#region Test Constructors

		public void TestConstructors()
		{
			var incident = GetIncident();
			Type lookupsType = incident.Lookups.GetType();
			var lookups1 = incident.Lookups;
			var lookups2 = new SupportIncidentLookups(incident);
			var lookups3 = new SupportIncidentLookups(Factory);

			AssertEquals(incident, lookups1.Parent);
			AssertEquals(incident, lookups2.Parent);
			AssertEquals(null, lookups3.Parent);

			AssertCollectionsNotNullAndEquals(lookups1.Clients, lookups2.Clients);
			AssertCollectionsNotNullAndEquals(lookups1.ContactList, lookups2.ContactList);
			AssertCollectionsNotNullAndEquals(lookups1.ProductList, lookups2.ProductList);
			AssertCollectionsNotNullAndEquals(lookups1.ModuleListAllModules, lookups2.ModuleListAllModules);

			AssertCollectionsNotNullAndEquals(lookups1.Clients, lookups3.Clients);
			AssertCollectionsNotNullAndEquals(lookups1.ProductList, lookups3.ProductList);
		}

		void AssertCollectionsNotNullAndEquals(ICollection c1, ICollection c2)
		{
			AssertNotNull("Should not be null", c1);
			AssertNotNull("Should not be null", c2);

			foreach (object o in c1)
			{
				AssertCollectionContains(o, c2);
			}
		}

		#endregion Test Constructors

		public void TestFeatureRequestTypes()
		{
			SupportIncident incident = Factory.New<SupportIncident>();

			AssertEquals(4, incident.Lookups.FeatureRequestTypes.Count);

			AssertEquals("CLI", incident.Lookups.FeatureRequestTypes[0].Code);
			AssertEquals("COP", incident.Lookups.FeatureRequestTypes[1].Code);
			AssertEquals("PRD", incident.Lookups.FeatureRequestTypes[2].Code);
			AssertEquals("MOD", incident.Lookups.FeatureRequestTypes[3].Code);
		}

		public void TestFeatureRequests()
		{
			SupportIncident incident1 = Factory.NewWithValidTestData<SupportIncident>();
			incident1.IM_Product = "ENT";
			incident1.Escalate(SupportIncidentCategoriesList.Codes.FeatureRequest, "");
			Factory.Save();

			SupportIncident incident2 = Factory.NewWithValidTestData<SupportIncident>();
			incident2.IM_Product = "ENT";
			incident2.Escalate(SupportIncidentCategoriesList.Codes.FeatureRequest, "");
			Factory.Save();

			incident2.Lookups.FeatureRequests.Load();
			AssertEquals(1, incident2.Lookups.FeatureRequests.Count);
			AssertEquals(incident1.PK, incident2.Lookups.FeatureRequests[0].PK);

			incident1.Lookups.FeatureRequests.Load();
			AssertEquals(1, incident1.Lookups.FeatureRequests.Count);
			AssertEquals(incident2.PK, incident1.Lookups.FeatureRequests[0].PK);
		}

		public void TestGetFeatureRequestStatusDispositionList()
		{
			SupportIncident incident = Factory.New<SupportIncident>();
			CombineAssertions(() =>
			{
				AssertEquals("", 26, incident.Lookups.GetFeatureRequestStatusDispositionList("", "", "").Count);
				AssertEquals("Open", 10, incident.Lookups.GetFeatureRequestStatusDispositionList(SupportIncidentLookups.Status.Open, "", "").Count);
				AssertEquals("Working", 13, incident.Lookups.GetFeatureRequestStatusDispositionList(SupportIncidentLookups.Status.Working, "", "").Count);
				AssertEquals("Suspended", 9, incident.Lookups.GetFeatureRequestStatusDispositionList(SupportIncidentLookups.Status.Suspended, "", "").Count);
				AssertEquals("Closed", 19, incident.Lookups.GetFeatureRequestStatusDispositionList(SupportIncidentLookups.Status.Closed, "", "").Count);
				AssertEquals("NotClosed", 15, incident.Lookups.GetFeatureRequestStatusDispositionList(SupportIncidentLookups.Status.NotClosed, "", "").Count);
			});
		}

		public void TestGetDefectStatusDispositionList()
		{
			SupportIncident incident = Factory.New<SupportIncident>();
			CombineAssertions(() =>
			{
				AssertEquals("", 21, incident.Lookups.GetDefectStatusDispositionList("", "", "").Count);
				AssertEquals("Open", 2, incident.Lookups.GetDefectStatusDispositionList(SupportIncidentLookups.Status.Open, "", "").Count);
				AssertEquals("Working", 5, incident.Lookups.GetDefectStatusDispositionList(SupportIncidentLookups.Status.Working, "", "").Count);
				AssertEquals("Suspended", 1, incident.Lookups.GetDefectStatusDispositionList(SupportIncidentLookups.Status.Suspended, "", "").Count);
				AssertEquals("Closed", 15, incident.Lookups.GetDefectStatusDispositionList(SupportIncidentLookups.Status.Closed, "", "").Count);
				AssertEquals("NotClosed", 7, incident.Lookups.GetDefectStatusDispositionList(SupportIncidentLookups.Status.NotClosed, "", "").Count);
			});
		}

		public void TestGetSupportStatusDispositionList()
		{
			SupportIncident incident = Factory.New<SupportIncident>();
			CombineAssertions(() =>
			{
				AssertEquals("", 22, incident.Lookups.GetSupportStatusDispositionList("", "", "").Count);
				AssertEquals("Open", 2, incident.Lookups.GetSupportStatusDispositionList(SupportIncidentLookups.Status.Open, "", "").Count);
				AssertEquals("Working", 2, incident.Lookups.GetSupportStatusDispositionList(SupportIncidentLookups.Status.Working, "", "").Count);
				AssertEquals("Suspended", 1, incident.Lookups.GetSupportStatusDispositionList(SupportIncidentLookups.Status.Suspended, "", "").Count);
				AssertEquals("Closed", 17, incident.Lookups.GetSupportStatusDispositionList(SupportIncidentLookups.Status.Closed, "", "").Count);
				AssertEquals("NotClosed", 4, incident.Lookups.GetSupportStatusDispositionList(SupportIncidentLookups.Status.NotClosed, "", "").Count);
			});
		}

		public void TestGetComplianceRequirementStatusDispositionList()
		{
			var incident = Factory.New<SupportIncident>();
			CombineAssertions(() =>
			{
				AssertEquals("", 17, incident.Lookups.GetComplianceRequirementStatusDispositionList("", "", "").Count);
				AssertEquals("Open", 2, incident.Lookups.GetComplianceRequirementStatusDispositionList(SupportIncidentLookups.Status.Open, "", "").Count);
				AssertEquals("Working", 5, incident.Lookups.GetComplianceRequirementStatusDispositionList(SupportIncidentLookups.Status.Working, "", "").Count);
				AssertEquals("Suspended", 1, incident.Lookups.GetComplianceRequirementStatusDispositionList(SupportIncidentLookups.Status.Suspended, "", "").Count);
				AssertEquals("Closed", 11, incident.Lookups.GetComplianceRequirementStatusDispositionList(SupportIncidentLookups.Status.Closed, "", "").Count);
				AssertEquals("NotClosed", 6, incident.Lookups.GetComplianceRequirementStatusDispositionList(SupportIncidentLookups.Status.NotClosed, "", "").Count);
			});
		}

		public void TestGetCustomerServiceStatusDispositionList()
		{
			var incident = Factory.New<SupportIncident>();
			CombineAssertions(() =>
			{
				AssertEquals("", 17, incident.Lookups.GetCustomerServiceStatusDispositionList("", "", "").Count);
				AssertEquals("Open", 2, incident.Lookups.GetCustomerServiceStatusDispositionList(SupportIncidentLookups.Status.Open, "", "").Count);
				AssertEquals("Working", 5, incident.Lookups.GetCustomerServiceStatusDispositionList(SupportIncidentLookups.Status.Working, "", "").Count);
				AssertEquals("Suspended", 1, incident.Lookups.GetCustomerServiceStatusDispositionList(SupportIncidentLookups.Status.Suspended, "", "").Count);
				AssertEquals("Closed", 11, incident.Lookups.GetCustomerServiceStatusDispositionList(SupportIncidentLookups.Status.Closed, "", "").Count);
				AssertEquals("NotClosed", 6, incident.Lookups.GetCustomerServiceStatusDispositionList(SupportIncidentLookups.Status.NotClosed, "", "").Count);
			});
		}

		public void TestStatusDispositionListContainsInactiveCode()
		{
			var tree = new IncidentClosureDispositionCollection(true, 3, 4);
			CodeDescriptionBoolTreeTestHelper.Add(tree, "DEF");
			CodeDescriptionBoolTreeTestHelper.Add(tree, "DEF", "CR1");
			CodeDescriptionBoolTreeTestHelper.Add(tree, "DEF", "CR1", "ENT");
			CodeDescriptionBoolTreeTestHelper.Add(tree, "DEF", "CR1", "ENT", "AA1");
			CodeDescriptionBoolTreeTestHelper.Add(tree, "DEF", "CR1", "ENT", "AA2");
			CodeDescriptionBoolTreeTestHelper.Add(tree, "DEF", "CR1", "ENT", "AA3");

			var inactiveNode = tree.Find("DEF", "CR1", "ENT", "AA2");
			inactiveNode.Bool = false;

			EDIDataRegistry.Instance.IncidentClosureDispositions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, tree);

			var incident = Factory.New<SupportIncident>();
			incident.IM_Status = "CLS";
			incident.IM_Priority = "CR1";
			incident.IM_Category = "DEF";
			incident.IM_Product = "ENT";
			var list = incident.Lookups.StatusDispositionList;
			AssertEquals(9, list.Count);
			AssertEquals("Should contain active code AA1", "AA1", list[0].Code);
			AssertEquals("Should contain inactive code AA2", "AA2", list[1].Code);
			AssertEquals("Should contain active code AA3", "AA3", list[2].Code);
			Assert("Should contain active code UPO", list.Cast<ICodeDescription>().Any(x => x.Code == "UPO"));
			Assert("Should contain active code UDO", list.Cast<ICodeDescription>().Any(x => x.Code == "UDO"));
		}

		public void TestStatusDispositionList_ShouldShowClosedDispositions()
		{
			var tree = new IncidentClosureDispositionCollection(true, 3, 4);
			CodeDescriptionBoolTreeTestHelper.Add(tree, "DEF");
			CodeDescriptionBoolTreeTestHelper.Add(tree, "DEF", Core.Constants.CustomerService.CriticalityCodes.CR1_SystemDown);
			CodeDescriptionBoolTreeTestHelper.Add(tree, "DEF", Core.Constants.CustomerService.CriticalityCodes.CR1_SystemDown, "ENT");
			CodeDescriptionBoolTreeTestHelper.Add(tree, "DEF", Core.Constants.CustomerService.CriticalityCodes.CR1_SystemDown, "ENT", "AA1");
			CodeDescriptionBoolTreeTestHelper.Add(tree, "DEF", Core.Constants.CustomerService.CriticalityCodes.CR1_SystemDown, "ENT", "AA2");
			CodeDescriptionBoolTreeTestHelper.Add(tree, "DEF", Core.Constants.CustomerService.CriticalityCodes.CR1_SystemDown, "ENT", "AA3");

			var inactiveNode = tree.Find(SupportIncidentCategoriesList.Codes.Defect, Core.Constants.CustomerService.CriticalityCodes.CR1_SystemDown, "ENT", "AA2");
			inactiveNode.Bool = false;

			EDIDataRegistry.Instance.IncidentClosureDispositions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, tree);

			var incident = Factory.New<SupportIncident>();
			incident.IM_Status = IncidentMainLookups.Status.Closed;
			incident.IM_Priority = "CR1";
			incident.IM_Category = "DEF";
			incident.IM_Product = "ENT";
			var closedList = incident.Lookups.StatusDispositionList;
			AssertEquals("Precondition", "AA1", closedList[0].Code);
			AssertEquals("Precondition", "AA2", closedList[1].Code);
			AssertEquals("Precondition", "AA3", closedList[2].Code);
			var closedArray = closedList.ToArray();

			incident.IM_Status = IncidentMainLookups.Status.Open;
			AssertEquals("Precondition", false, incident.ShouldShowClosedDispositions);

			var openList = incident.Lookups.StatusDispositionList;
			AssertNotEquals("Precondition", closedList.Count, openList.Count);
			Assert(!closedArray.All(x => openList.Contains(x)));

			incident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Closed.Resolved;
			AssertEquals("Precondition", true, incident.ShouldShowClosedDispositions);

			var resolvedList = incident.Lookups.StatusDispositionList;
			Assert(closedArray.All(x => resolvedList.Contains(x)));

			incident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed;
			AssertEquals("Precondition", true, incident.ShouldShowClosedDispositions);

			var resolvedAndClosedList = incident.Lookups.StatusDispositionList;
			Assert(closedArray.All(x => resolvedAndClosedList.Contains(x)));

			incident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse;
			AssertEquals("Precondition", true, incident.ShouldShowClosedDispositions);

			var awaitingClientResponseList = incident.Lookups.StatusDispositionList;
			Assert(closedArray.All(x => awaitingClientResponseList.Contains(x)));
		}

		public void TestGetStatusDispositionList_ShouldShowClosedDispositions()
		{
			var tree = new IncidentClosureDispositionCollection(true, 3, 4);
			CodeDescriptionBoolTreeTestHelper.Add(tree, SupportIncidentCategoriesList.Codes.Defect);
			CodeDescriptionBoolTreeTestHelper.Add(tree, SupportIncidentCategoriesList.Codes.Defect, Core.Constants.CustomerService.CriticalityCodes.CR1_SystemDown);
			CodeDescriptionBoolTreeTestHelper.Add(tree, SupportIncidentCategoriesList.Codes.Defect, Core.Constants.CustomerService.CriticalityCodes.CR1_SystemDown, "ENT");
			CodeDescriptionBoolTreeTestHelper.Add(tree, SupportIncidentCategoriesList.Codes.Defect, Core.Constants.CustomerService.CriticalityCodes.CR1_SystemDown, "ENT", "AA1");
			CodeDescriptionBoolTreeTestHelper.Add(tree, SupportIncidentCategoriesList.Codes.Defect, Core.Constants.CustomerService.CriticalityCodes.CR1_SystemDown, "ENT", "AA2");
			CodeDescriptionBoolTreeTestHelper.Add(tree, SupportIncidentCategoriesList.Codes.Defect, Core.Constants.CustomerService.CriticalityCodes.CR1_SystemDown, "ENT", "AA3");

			EDIDataRegistry.Instance.IncidentClosureDispositions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, tree);

			var incident = Factory.New<SupportIncident>();
			var closedList = incident.Lookups.GetStatusDispositionList(SupportIncidentCategoriesList.Codes.Defect, IncidentMainLookups.Status.Closed, Core.Constants.CustomerService.CriticalityCodes.CR1_SystemDown, "ENT", shouldShowClosedDispositions: false);
			AssertEquals("Precondition", "AA1", closedList[0].Code);
			AssertEquals("Precondition", "AA2", closedList[1].Code);
			AssertEquals("Precondition", "AA3", closedList[2].Code);
			var closedArray = closedList.ToArray();

			var openList = incident.Lookups.GetStatusDispositionList(SupportIncidentCategoriesList.Codes.Defect, IncidentMainLookups.Status.Open, Core.Constants.CustomerService.CriticalityCodes.CR1_SystemDown, "ENT", shouldShowClosedDispositions: false);
			AssertNotEquals("Precondition", closedList.Count, openList.Count);
			Assert("Precondition: Open disposition list should not include all the closed dispositions", !closedArray.All(x => openList.Contains(x)));

			var openStatusButClosedResolvedDispositionList = incident.Lookups.GetStatusDispositionList(SupportIncidentCategoriesList.Codes.Defect, IncidentMainLookups.Status.Open, Core.Constants.CustomerService.CriticalityCodes.CR1_SystemDown, "ENT", shouldShowClosedDispositions: true);
			Assert("Closed or resolved disposition lists should include all the closed dispositions", closedArray.All(x => openStatusButClosedResolvedDispositionList.Contains(x)));
		}

		public void TestGetSupportStatusDispositionList_ShouldShowClosedDispositions()
		{
			var tree = new IncidentClosureDispositionCollection(true, 3, 4);
			CodeDescriptionBoolTreeTestHelper.Add(tree, SupportIncidentCategoriesList.Codes.Support);
			CodeDescriptionBoolTreeTestHelper.Add(tree, SupportIncidentCategoriesList.Codes.Support, Core.Constants.CustomerService.CriticalityCodes.CR1_SystemDown);
			CodeDescriptionBoolTreeTestHelper.Add(tree, SupportIncidentCategoriesList.Codes.Support, Core.Constants.CustomerService.CriticalityCodes.CR1_SystemDown, "ENT");
			CodeDescriptionBoolTreeTestHelper.Add(tree, SupportIncidentCategoriesList.Codes.Support, Core.Constants.CustomerService.CriticalityCodes.CR1_SystemDown, "ENT", "AA1");
			CodeDescriptionBoolTreeTestHelper.Add(tree, SupportIncidentCategoriesList.Codes.Support, Core.Constants.CustomerService.CriticalityCodes.CR1_SystemDown, "ENT", "AA2");
			CodeDescriptionBoolTreeTestHelper.Add(tree, SupportIncidentCategoriesList.Codes.Support, Core.Constants.CustomerService.CriticalityCodes.CR1_SystemDown, "ENT", "AA3");

			EDIDataRegistry.Instance.IncidentClosureDispositions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, tree);

			var incident = Factory.New<SupportIncident>();
			var closedList = incident.Lookups.GetSupportStatusDispositionList(IncidentMainLookups.Status.Closed, Core.Constants.CustomerService.CriticalityCodes.CR1_SystemDown, "ENT", shouldShowClosedDispositions: false);
			AssertEquals("Precondition", "AA1", closedList[0].Code);
			AssertEquals("Precondition", "AA2", closedList[1].Code);
			AssertEquals("Precondition", "AA3", closedList[2].Code);
			var closedArray = closedList.ToArray();

			var openList = incident.Lookups.GetSupportStatusDispositionList(IncidentMainLookups.Status.Open, Core.Constants.CustomerService.CriticalityCodes.CR1_SystemDown, "ENT", shouldShowClosedDispositions: false);
			AssertNotEquals("Precondition", closedList.Count, openList.Count);
			Assert("Precondition: Open disposition list should not include all the closed dispositions", !closedArray.All(x => openList.Contains(x)));

			var openStatusButClosedResolvedDispositionList = incident.Lookups.GetSupportStatusDispositionList(IncidentMainLookups.Status.Open, Core.Constants.CustomerService.CriticalityCodes.CR1_SystemDown, "ENT", shouldShowClosedDispositions: true);
			Assert("Closed or resolved disposition lists should include all the closed dispositions", closedArray.All(x => openStatusButClosedResolvedDispositionList.Contains(x)));
		}

		public void TestGetContentDevelopmentStatusDispositionList_ShouldShowClosedDispositions()
		{
			var tree = new IncidentClosureDispositionCollection(true, 3, 4);
			CodeDescriptionBoolTreeTestHelper.Add(tree, SupportIncidentCategoriesList.Codes.ContentDevelopment);
			CodeDescriptionBoolTreeTestHelper.Add(tree, SupportIncidentCategoriesList.Codes.ContentDevelopment, Core.Constants.CustomerService.CriticalityCodes.CR1_SystemDown);
			CodeDescriptionBoolTreeTestHelper.Add(tree, SupportIncidentCategoriesList.Codes.ContentDevelopment, Core.Constants.CustomerService.CriticalityCodes.CR1_SystemDown, "ENT");
			CodeDescriptionBoolTreeTestHelper.Add(tree, SupportIncidentCategoriesList.Codes.ContentDevelopment, Core.Constants.CustomerService.CriticalityCodes.CR1_SystemDown, "ENT", "AA1");
			CodeDescriptionBoolTreeTestHelper.Add(tree, SupportIncidentCategoriesList.Codes.ContentDevelopment, Core.Constants.CustomerService.CriticalityCodes.CR1_SystemDown, "ENT", "AA2");
			CodeDescriptionBoolTreeTestHelper.Add(tree, SupportIncidentCategoriesList.Codes.ContentDevelopment, Core.Constants.CustomerService.CriticalityCodes.CR1_SystemDown, "ENT", "AA3");

			EDIDataRegistry.Instance.IncidentClosureDispositions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, tree);

			var incident = Factory.New<SupportIncident>();
			var closedList = incident.Lookups.GetContentDevelopmentStatusDispositionList(IncidentMainLookups.Status.Closed, Core.Constants.CustomerService.CriticalityCodes.CR1_SystemDown, "ENT", shouldShowClosedDispositions: false);
			Assert("Precondition", closedList.ContainsCode("AA1"));
			Assert("Precondition", closedList.ContainsCode("AA2"));
			Assert("Precondition", closedList.ContainsCode("AA3"));
			var closedArray = closedList.ToArray();

			var openList = incident.Lookups.GetContentDevelopmentStatusDispositionList(IncidentMainLookups.Status.Open, Core.Constants.CustomerService.CriticalityCodes.CR1_SystemDown, "ENT", shouldShowClosedDispositions: false);
			AssertNotEquals("Precondition", closedList.Count, openList.Count);
			Assert("Precondition: Open disposition list should not include all the closed dispositions", !closedArray.All(x => openList.Contains(x)));

			var openStatusButClosedResolvedDispositionList = incident.Lookups.GetContentDevelopmentStatusDispositionList(IncidentMainLookups.Status.Open, Core.Constants.CustomerService.CriticalityCodes.CR1_SystemDown, "ENT", shouldShowClosedDispositions: true);
			Assert("Closed or resolved disposition lists should include all the closed dispositions", closedArray.All(x => openStatusButClosedResolvedDispositionList.Contains(x)));
		}

		public void TestGetDefectStatusDispositionList_ShouldShowClosedDispositions()
		{
			var tree = new IncidentClosureDispositionCollection(true, 3, 4);
			CodeDescriptionBoolTreeTestHelper.Add(tree, SupportIncidentCategoriesList.Codes.Defect);
			CodeDescriptionBoolTreeTestHelper.Add(tree, SupportIncidentCategoriesList.Codes.Defect, Core.Constants.CustomerService.CriticalityCodes.CR1_SystemDown);
			CodeDescriptionBoolTreeTestHelper.Add(tree, SupportIncidentCategoriesList.Codes.Defect, Core.Constants.CustomerService.CriticalityCodes.CR1_SystemDown, "ENT");
			CodeDescriptionBoolTreeTestHelper.Add(tree, SupportIncidentCategoriesList.Codes.Defect, Core.Constants.CustomerService.CriticalityCodes.CR1_SystemDown, "ENT", "AA1");
			CodeDescriptionBoolTreeTestHelper.Add(tree, SupportIncidentCategoriesList.Codes.Defect, Core.Constants.CustomerService.CriticalityCodes.CR1_SystemDown, "ENT", "AA2");
			CodeDescriptionBoolTreeTestHelper.Add(tree, SupportIncidentCategoriesList.Codes.Defect, Core.Constants.CustomerService.CriticalityCodes.CR1_SystemDown, "ENT", "AA3");

			EDIDataRegistry.Instance.IncidentClosureDispositions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, tree);

			var incident = Factory.New<SupportIncident>();
			var closedList = incident.Lookups.GetDefectStatusDispositionList(IncidentMainLookups.Status.Closed, Core.Constants.CustomerService.CriticalityCodes.CR1_SystemDown, "ENT", shouldShowClosedDispositions: false);
			AssertEquals("Precondition", "AA1", closedList[0].Code);
			AssertEquals("Precondition", "AA2", closedList[1].Code);
			AssertEquals("Precondition", "AA3", closedList[2].Code);
			var closedArray = closedList.ToArray();

			var openList = incident.Lookups.GetDefectStatusDispositionList(IncidentMainLookups.Status.Open, Core.Constants.CustomerService.CriticalityCodes.CR1_SystemDown, "ENT", shouldShowClosedDispositions: false);
			AssertNotEquals("Precondition", closedList.Count, openList.Count);
			Assert("Precondition: Open disposition list should not include all the closed dispositions", !closedArray.All(x => openList.Contains(x)));

			var openStatusButClosedResolvedDispositionList = incident.Lookups.GetDefectStatusDispositionList(IncidentMainLookups.Status.Open, Core.Constants.CustomerService.CriticalityCodes.CR1_SystemDown, "ENT", shouldShowClosedDispositions: true);
			Assert("Closed or resolved disposition lists should include all the closed dispositions", closedArray.All(x => openStatusButClosedResolvedDispositionList.Contains(x)));
		}

		public void TestGetFeatureRequestStatusDispositionList_ShouldShowClosedDispositions()
		{
			var tree = new IncidentClosureDispositionCollection(true, 3, 4);
			CodeDescriptionBoolTreeTestHelper.Add(tree, SupportIncidentCategoriesList.Codes.FeatureRequest);
			CodeDescriptionBoolTreeTestHelper.Add(tree, SupportIncidentCategoriesList.Codes.FeatureRequest, Core.Constants.CustomerService.CriticalityCodes.CR1_SystemDown);
			CodeDescriptionBoolTreeTestHelper.Add(tree, SupportIncidentCategoriesList.Codes.FeatureRequest, Core.Constants.CustomerService.CriticalityCodes.CR1_SystemDown, "ENT");
			CodeDescriptionBoolTreeTestHelper.Add(tree, SupportIncidentCategoriesList.Codes.FeatureRequest, Core.Constants.CustomerService.CriticalityCodes.CR1_SystemDown, "ENT", "AA1");
			CodeDescriptionBoolTreeTestHelper.Add(tree, SupportIncidentCategoriesList.Codes.FeatureRequest, Core.Constants.CustomerService.CriticalityCodes.CR1_SystemDown, "ENT", "AA2");
			CodeDescriptionBoolTreeTestHelper.Add(tree, SupportIncidentCategoriesList.Codes.FeatureRequest, Core.Constants.CustomerService.CriticalityCodes.CR1_SystemDown, "ENT", "AA3");

			EDIDataRegistry.Instance.IncidentClosureDispositions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, tree);

			var incident = Factory.New<SupportIncident>();
			var closedList = incident.Lookups.GetFeatureRequestStatusDispositionList(IncidentMainLookups.Status.Closed, Core.Constants.CustomerService.CriticalityCodes.CR1_SystemDown, "ENT", shouldShowClosedDispositions: false);
			AssertEquals("Precondition", "AA1", closedList[0].Code);
			AssertEquals("Precondition", "AA2", closedList[1].Code);
			AssertEquals("Precondition", "AA3", closedList[2].Code);
			var closedArray = closedList.ToArray();

			var openList = incident.Lookups.GetFeatureRequestStatusDispositionList(IncidentMainLookups.Status.Open, Core.Constants.CustomerService.CriticalityCodes.CR1_SystemDown, "ENT", shouldShowClosedDispositions: false);
			AssertNotEquals("Precondition", closedList.Count, openList.Count);
			Assert("Precondition: Open disposition list should not include all the closed dispositions", !closedArray.All(x => openList.Contains(x)));

			var openStatusButClosedResolvedDispositionList = incident.Lookups.GetFeatureRequestStatusDispositionList(IncidentMainLookups.Status.Open, Core.Constants.CustomerService.CriticalityCodes.CR1_SystemDown, "ENT", shouldShowClosedDispositions: true);
			Assert("Closed or resolved disposition lists should include all the closed dispositions", closedArray.All(x => openStatusButClosedResolvedDispositionList.Contains(x)));
		}

		public void TestGetComplianceRequirementStatusDispositionList_ShouldShowClosedDispositions()
		{
			var tree = new IncidentClosureDispositionCollection(true, 3, 4);
			CodeDescriptionBoolTreeTestHelper.Add(tree, SupportIncidentCategoriesList.Codes.ComplianceRequirement);
			CodeDescriptionBoolTreeTestHelper.Add(tree, SupportIncidentCategoriesList.Codes.ComplianceRequirement, Core.Constants.CustomerService.CriticalityCodes.CR1_SystemDown);
			CodeDescriptionBoolTreeTestHelper.Add(tree, SupportIncidentCategoriesList.Codes.ComplianceRequirement, Core.Constants.CustomerService.CriticalityCodes.CR1_SystemDown, "ENT");
			CodeDescriptionBoolTreeTestHelper.Add(tree, SupportIncidentCategoriesList.Codes.ComplianceRequirement, Core.Constants.CustomerService.CriticalityCodes.CR1_SystemDown, "ENT", "AA1");
			CodeDescriptionBoolTreeTestHelper.Add(tree, SupportIncidentCategoriesList.Codes.ComplianceRequirement, Core.Constants.CustomerService.CriticalityCodes.CR1_SystemDown, "ENT", "AA2");
			CodeDescriptionBoolTreeTestHelper.Add(tree, SupportIncidentCategoriesList.Codes.ComplianceRequirement, Core.Constants.CustomerService.CriticalityCodes.CR1_SystemDown, "ENT", "AA3");

			EDIDataRegistry.Instance.IncidentClosureDispositions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, tree);

			var incident = Factory.New<SupportIncident>();
			var closedList = incident.Lookups.GetComplianceRequirementStatusDispositionList(IncidentMainLookups.Status.Closed, Core.Constants.CustomerService.CriticalityCodes.CR1_SystemDown, "ENT", shouldShowClosedDispositions: false);
			var closedArray = closedList.ToArray();
			Assert("Precondition", closedList.ContainsCode("AA1"));
			Assert("Precondition", closedList.ContainsCode("AA2"));
			Assert("Precondition", closedList.ContainsCode("AA3"));

			var openList = incident.Lookups.GetComplianceRequirementStatusDispositionList(IncidentMainLookups.Status.Open, Core.Constants.CustomerService.CriticalityCodes.CR1_SystemDown, "ENT", shouldShowClosedDispositions: false);
			AssertNotEquals("Precondition", closedList.Count, openList.Count);
			Assert("Precondition: Open disposition list should not include all the closed dispositions", !closedArray.All(x => openList.Contains(x)));

			var openStatusButClosedResolvedDispositionList = incident.Lookups.GetComplianceRequirementStatusDispositionList(IncidentMainLookups.Status.Open, Core.Constants.CustomerService.CriticalityCodes.CR1_SystemDown, "ENT", shouldShowClosedDispositions: true);
			Assert("Closed or resolved disposition lists should include all the closed dispositions", closedArray.All(x => openStatusButClosedResolvedDispositionList.Contains(x)));
		}

		public void TestGetCustomerServiceStatusDispositionList_ShouldShowClosedDispositions()
		{
			var tree = new IncidentClosureDispositionCollection(true, 3, 4);
			CodeDescriptionBoolTreeTestHelper.Add(tree, SupportIncidentCategoriesList.Codes.CustomerServiceRequest);
			CodeDescriptionBoolTreeTestHelper.Add(tree, SupportIncidentCategoriesList.Codes.CustomerServiceRequest, Core.Constants.CustomerService.CriticalityCodes.CR1_SystemDown);
			CodeDescriptionBoolTreeTestHelper.Add(tree, SupportIncidentCategoriesList.Codes.CustomerServiceRequest, Core.Constants.CustomerService.CriticalityCodes.CR1_SystemDown, "ENT");
			CodeDescriptionBoolTreeTestHelper.Add(tree, SupportIncidentCategoriesList.Codes.CustomerServiceRequest, Core.Constants.CustomerService.CriticalityCodes.CR1_SystemDown, "ENT", "AA1");
			CodeDescriptionBoolTreeTestHelper.Add(tree, SupportIncidentCategoriesList.Codes.CustomerServiceRequest, Core.Constants.CustomerService.CriticalityCodes.CR1_SystemDown, "ENT", "AA2");
			CodeDescriptionBoolTreeTestHelper.Add(tree, SupportIncidentCategoriesList.Codes.CustomerServiceRequest, Core.Constants.CustomerService.CriticalityCodes.CR1_SystemDown, "ENT", "AA3");

			EDIDataRegistry.Instance.IncidentClosureDispositions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, tree);

			var incident = Factory.New<SupportIncident>();
			var closedList = incident.Lookups.GetCustomerServiceStatusDispositionList(IncidentMainLookups.Status.Closed, Core.Constants.CustomerService.CriticalityCodes.CR1_SystemDown, "ENT", shouldShowClosedDispositions: false);
			Assert("Precondition", closedList.ContainsCode("AA1"));
			Assert("Precondition", closedList.ContainsCode("AA2"));
			Assert("Precondition", closedList.ContainsCode("AA3"));
			var closedArray = closedList.ToArray();

			var openList = incident.Lookups.GetCustomerServiceStatusDispositionList(IncidentMainLookups.Status.Open, Core.Constants.CustomerService.CriticalityCodes.CR1_SystemDown, "ENT", shouldShowClosedDispositions: false);
			AssertNotEquals("Precondition", closedList.Count, openList.Count);
			Assert("Precondition: Open disposition list should not include all the closed dispositions", !closedArray.All(x => openList.Contains(x)));

			var openStatusButClosedResolvedDispositionList = incident.Lookups.GetCustomerServiceStatusDispositionList(IncidentMainLookups.Status.Open, Core.Constants.CustomerService.CriticalityCodes.CR1_SystemDown, "ENT", shouldShowClosedDispositions: true);
			Assert("Closed or resolved disposition lists should include all the closed dispositions", closedArray.All(x => openStatusButClosedResolvedDispositionList.Contains(x)));
		}

		public void TestGetClosureDispositionList()
		{
			const string ALL = CodeDescriptionBoolTreeNode.AllCode;

			var tree = new IncidentClosureDispositionCollection(true, 3, 4);
			CodeDescriptionBoolTreeTestHelper.Add(tree, "DEF");
			CodeDescriptionBoolTreeTestHelper.Add(tree, "DEF", "CR1");
			CodeDescriptionBoolTreeTestHelper.Add(tree, "DEF", "CR1", "ENT");
			CodeDescriptionBoolTreeTestHelper.Add(tree, "DEF", "CR1", "ENT", "AA1");
			CodeDescriptionBoolTreeTestHelper.Add(tree, "DEF", ALL, ALL, "BB1");
			CodeDescriptionBoolTreeTestHelper.Add(tree, "DEF", ALL, ALL, "BB2");

			EDIDataRegistry.Instance.IncidentClosureDispositions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, tree);

			var incident = Factory.New<SupportIncident>();
			incident.IM_Priority = "CR1";
			incident.IM_Category = "DEF";
			incident.IM_Product = "ENT";
			var list = incident.Lookups.GetStatusDispositionList(incident.IM_Category, IncidentMainLookups.Status.Closed, incident.IM_Priority, incident.IM_Product);
			AssertEquals(7, list.Count);
			AssertEquals("AA1", list[0].Code);
			Assert("Should always contain SLV for closure dispositions", list.ContainsCode(SupportIncidentLookups.DispositionList.Constants.Closed.Resolved));
			Assert("Should always contain CLS for closure dispositions", list.ContainsCode(SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed));
			Assert("Should always contain UPO for closure dispositions", list.ContainsCode(SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade));
			Assert("Should always contain UDO for closure dispositions", list.ContainsCode(SupportIncidentLookups.DispositionList.Constants.Closed.UpgradeDelayed));

			incident.IM_Priority = "CR2";
			list = incident.Lookups.GetStatusDispositionList(incident.IM_Category, IncidentMainLookups.Status.Closed, incident.IM_Priority, incident.IM_Product);
			AssertEquals(8, list.Count);
			AssertEquals("BB1", list[0].Code);
			AssertEquals("BB2", list[1].Code);
			Assert("Should always contain SLV for closure dispositions", list.ContainsCode(SupportIncidentLookups.DispositionList.Constants.Closed.Resolved));
			Assert("Should always contain CLS for closure dispositions", list.ContainsCode(SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed));
			Assert("Should always contain UPO for closure dispositions", list.ContainsCode(SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade));
			Assert("Should always contain UDO for closure dispositions", list.ContainsCode(SupportIncidentLookups.DispositionList.Constants.Closed.UpgradeDelayed));

			tree = new IncidentClosureDispositionCollection(true, 3, 4);
			CodeDescriptionBoolTreeTestHelper.Add(tree, "DEF");
			CodeDescriptionBoolTreeTestHelper.Add(tree, "DEF", "CR3");
			CodeDescriptionBoolTreeTestHelper.Add(tree, "DEF", "CR3", ALL, "CC1");
			CodeDescriptionBoolTreeTestHelper.Add(tree, "DEF", ALL, "ENT");
			CodeDescriptionBoolTreeTestHelper.Add(tree, "DEF", ALL, "ENT", "DD1");
			CodeDescriptionBoolTreeTestHelper.Add(tree, "DEF", ALL, "ENT", "DD2");
			EDIDataRegistry.Instance.IncidentClosureDispositions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, tree);

			incident.IM_Priority = "CR3";
			list = incident.Lookups.GetStatusDispositionList(incident.IM_Category, IncidentMainLookups.Status.Closed, incident.IM_Priority, incident.IM_Product);
			AssertEquals(7, list.Count);
			AssertEquals("CC1", list[0].Code);

			tree = new IncidentClosureDispositionCollection(true, 3, 4);
			CodeDescriptionBoolTreeTestHelper.Add(tree, "DEF");
			CodeDescriptionBoolTreeTestHelper.Add(tree, "DEF", ALL, "ENT");
			CodeDescriptionBoolTreeTestHelper.Add(tree, "DEF", ALL, "ENT", "DD1");
			CodeDescriptionBoolTreeTestHelper.Add(tree, "DEF", ALL, "ENT", "DD2");
			CodeDescriptionBoolTreeTestHelper.Add(tree, "DEF", ALL, ALL, "EE1");
			CodeDescriptionBoolTreeTestHelper.Add(tree, "DEF", ALL, ALL, "EE2");
			CodeDescriptionBoolTreeTestHelper.Add(tree, "DEF", ALL, ALL, "EE3");
			EDIDataRegistry.Instance.IncidentClosureDispositions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, tree);

			incident.IM_Priority = "CR4";
			list = incident.Lookups.GetStatusDispositionList(incident.IM_Category, IncidentMainLookups.Status.Closed, incident.IM_Priority, incident.IM_Product);
			AssertEquals(8, list.Count);
			AssertEquals("DD1", list[0].Code);
			AssertEquals("DD2", list[1].Code);

			list = new SupportIncidentLookups(Factory).GetStatusDispositionList(incident.IM_Category, IncidentMainLookups.Status.Closed, incident.IM_Priority, incident.IM_Product);
			AssertEquals(8, list.Count);
			AssertEquals("DD1", list[0].Code);
			AssertEquals("DD2", list[1].Code);
		}

		public void TestSourceList()
		{
			SupportIncident incident = Factory.New<SupportIncident>();
			AssertEquals(null, incident.Lookups.SourceList.GetDescriptionFromCode(SupportIncidentLookups.SourceListConstants.ERequestPortal));
			AssertEquals(null, incident.Lookups.SourceList.GetDescriptionFromCode(SupportIncidentLookups.SourceListConstants.IssueManagerReported));
			AssertEquals(null, incident.Lookups.SourceList.GetDescriptionFromCode(SupportIncidentLookups.SourceListConstants.CreatedFromProject));
			AssertEquals("Proactive Outbound", incident.Lookups.SourceList.GetDescriptionFromCode(SupportIncidentLookups.SourceListConstants.ProactiveOutbound));
			AssertEquals("Email Inbound", incident.Lookups.SourceList.GetDescriptionFromCode(SupportIncidentLookups.SourceListConstants.EmailInbound));
			AssertEquals(null, incident.Lookups.SourceList.GetDescriptionFromCode(SupportIncidentLookups.SourceListConstants.APIInboundInternal));
			AssertEquals(null, incident.Lookups.SourceList.GetDescriptionFromCode(SupportIncidentLookups.SourceListConstants.APIInboundExternal));
			AssertEquals("Chat Inbound", incident.Lookups.SourceList.GetDescriptionFromCode(SupportIncidentLookups.SourceListConstants.ChatInbound));
			AssertEquals("WTG Internal via ediProd", incident.Lookups.SourceList.GetDescriptionFromCode(SupportIncidentLookups.SourceListConstants.WTGInternalViaEdiProd));
			AssertEquals("Phone Inbound", incident.Lookups.SourceList.GetDescriptionFromCode(SupportIncidentLookups.SourceListConstants.PhoneInbound));

			incident.IM_Source = SupportIncidentLookups.SourceListConstants.ERequestPortal;
			AssertEquals("eRequest Portal", incident.Lookups.SourceList.GetDescriptionFromCode(SupportIncidentLookups.SourceListConstants.ERequestPortal));
			incident.IM_Source = SupportIncidentLookups.SourceListConstants.IssueManagerReported;
			AssertEquals("Created From Issue Manager", incident.Lookups.SourceList.GetDescriptionFromCode(SupportIncidentLookups.SourceListConstants.IssueManagerReported));
			incident.IM_Source = SupportIncidentLookups.SourceListConstants.CreatedFromProject;
			AssertEquals("Created From Project", incident.Lookups.SourceList.GetDescriptionFromCode(SupportIncidentLookups.SourceListConstants.CreatedFromProject));
			incident.IM_Source = SupportIncidentLookups.SourceListConstants.APIInboundInternal;
			AssertEquals("API Inbound (Internal)", incident.Lookups.SourceList.GetDescriptionFromCode(SupportIncidentLookups.SourceListConstants.APIInboundInternal));
			incident.IM_Source = SupportIncidentLookups.SourceListConstants.APIInboundExternal;
			AssertEquals("API Inbound (External)", incident.Lookups.SourceList.GetDescriptionFromCode(SupportIncidentLookups.SourceListConstants.APIInboundExternal));
		}

		public void TestSuspendedDescription()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();

			LicenceEnterprise enterprise = Factory.NewWithValidTestData<LicenceEnterprise>();
			enterprise.LE_OH = org.PK;

			LicenceCompany company = Factory.NewWithValidTestData<LicenceCompany>();
			company.LC_LE = enterprise.PK;
			company.LC_OH = org.PK;

			LicenceDatabase database = Factory.NewWithValidTestData<LicenceDatabase>();
			database.LD_LE = enterprise.PK;

			LicenceHeader header = Factory.NewWithValidTestData<LicenceHeader>();
			header.LA_LC = company.PK;
			header.LA_LD = database.PK;

			ClientCompany clientCompany = Factory.New<ClientCompany>();
			clientCompany.LCC_Code = company.LC_CompanyCode;
			clientCompany.LCC_LD = database.PK;
			clientCompany.LCC_OH = org.PK;
			Factory.Save();

			InternalIncidentLicenceSettings settings = new InternalIncidentLicenceSettings();
			LicenceEnterpriseKey key = new LicenceEnterpriseKey();
			key.LE_PK = enterprise.PK;
			settings.LicenceEnterpriseKeys.Add(key);
			settings.EdiProd_LicencePK = header.PK;
			settings.UAT_ALP_LicencePK = header.PK;
			settings.UAT_DPR_LicencePK = header.PK;
			settings.UAT_GPC_LicencePK = header.PK;
			settings.UAT_GPR_LicencePK = header.PK;
			settings.UAT_STD_LicencePK = header.PK;
			EDIDataRegistry.Instance.InternalIncidentLicenceSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, settings);

			CategorisedWorkflowTaskTypesCollection collection = new CategorisedWorkflowTaskTypesCollection();
			CategorisedWorkflowTaskTypes taskTypes = collection.AddNew();
			taskTypes.Code = "INC";
			taskTypes.Description = (NoResString)"Incident";
			WorkflowTaskType taskType1 = taskTypes.TaskTypes.AddNew();
			taskType1.Code = "AAA";
			taskType1.Description = (NoResString)"AAA Description";
			WorkflowTaskType taskType2 = taskTypes.TaskTypes.AddNew();
			taskType2.Code = "BBB";
			taskType2.Description = (NoResString)"BBB Description";
			WorkflowDataRegistry.Instance.TaskTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.SetupForNewCreatedFeatureRequest();
			incident.IM_Status = SupportIncidentLookups.Status.Suspended;
			incident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Suspended.Deferred;

			AssertEquals("Suspended", incident.IM_ResolutionCodeDescription);

			incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.SetupForProjectFeatureRequest();
			incident.IM_Status = SupportIncidentLookups.Status.Suspended;
			incident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Suspended.Deferred;
			SupportIncidentProcessTask task1 = incident.WorkflowItems.AddNew();
			task1.P9_Type = "AAA";
			task1.P9_GS_NKAssignedStaffMember = Factory.NewWithValidTestData<GlbStaff>().GS_Code;
			SupportIncidentProcessTask task2 = incident.WorkflowItems.AddNew();
			task2.P9_Type = "BBB";
			task2.P9_GS_NKAssignedStaffMember = Factory.NewWithValidTestData<GlbStaff>().GS_Code;
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertEquals("BBB Description", incident.IM_ResolutionCodeDescription);

			incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.SetupForInternalReportedIncident(SupportIncidentCategoriesList.Codes.FeatureRequest, SupportIncident.InternalFeatureRequestComment, null);
			incident.IM_LCC = clientCompany.PK;
			incident.IM_Category = SupportIncidentCategoriesList.Codes.FeatureRequest;
			incident.IM_Status = SupportIncidentLookups.Status.Suspended;
			incident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Suspended.Deferred;
			SupportIncidentProcessTask task3 = incident.WorkflowItems.AddNew();
			task3.P9_Type = "BBB";
			task3.P9_GS_NKAssignedStaffMember = Factory.NewWithValidTestData<GlbStaff>().GS_Code;
			task3.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			AssertEquals("BBB Description", incident.IM_ResolutionCodeDescription);
		}

		public void TestProjects()
		{
			SupportIncident incident = Factory.New<SupportIncident>();
			AssertNotNull(incident.Lookups.Projects);
		}

		public void TestGetStatusDispositionList()
		{
			SupportIncident incident = Factory.New<SupportIncident>();
			incident.IM_Status = "";

			AssertEquals(incident.Lookups.GetSupportStatusDispositionList("", "", "").ElementsAsString, incident.Lookups.GetStatusDispositionList(SupportIncidentCategoriesList.Codes.Support, "", "", "").ElementsAsString);
			AssertEquals(incident.Lookups.GetDefectStatusDispositionList("", "", "").ElementsAsString, incident.Lookups.GetStatusDispositionList(SupportIncidentCategoriesList.Codes.Defect, "", "", "").ElementsAsString);
			AssertEquals(incident.Lookups.GetFeatureRequestStatusDispositionList("", "", "").ElementsAsString, incident.Lookups.GetStatusDispositionList(SupportIncidentCategoriesList.Codes.FeatureRequest, "", "", "").ElementsAsString);
			AssertEquals(incident.Lookups.GetComplianceRequirementStatusDispositionList("", "", "").ElementsAsString, incident.Lookups.GetStatusDispositionList(SupportIncidentCategoriesList.Codes.ComplianceRequirement, "", "", "").ElementsAsString);
			AssertEquals(incident.Lookups.GetCustomerServiceStatusDispositionList("", "", "").ElementsAsString, incident.Lookups.GetStatusDispositionList(SupportIncidentCategoriesList.Codes.CustomerServiceRequest, "", "", "").ElementsAsString);
		}

		public void TestStageCriticalityMapping()
		{
			var incident = Factory.New<SupportIncident>();

			var criticalityList = incident.Lookups.ActiveStageCriticalityMapping[SupportIncidentCategoriesList.Codes.Support];
			AssertEquals(6, criticalityList.Count);
			Assert(criticalityList.Contains(Core.Constants.CustomerService.CriticalityCodes.CR1_SystemDown));
			Assert(criticalityList.Contains(Core.Constants.CustomerService.CriticalityCodes.CR2_ModuleDown));
			Assert(criticalityList.Contains(Core.Constants.CustomerService.CriticalityCodes.CR3_SingleFunctionNoWorkAround));
			Assert(criticalityList.Contains(Core.Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround));
			Assert(criticalityList.Contains(Core.Constants.CustomerService.CriticalityCodes.CR5_Training));
			Assert(criticalityList.Contains(Core.Constants.CustomerService.CriticalityCodes.CR8_ComplianceRequirement));

			criticalityList = incident.Lookups.ActiveStageCriticalityMapping[SupportIncidentCategoriesList.Codes.Defect];
			AssertEquals(4, criticalityList.Count);
			Assert(criticalityList.Contains(Core.Constants.CustomerService.CriticalityCodes.CR1_SystemDown));
			Assert(criticalityList.Contains(Core.Constants.CustomerService.CriticalityCodes.CR2_ModuleDown));
			Assert(criticalityList.Contains(Core.Constants.CustomerService.CriticalityCodes.CR3_SingleFunctionNoWorkAround));
			Assert(criticalityList.Contains(Core.Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround));

			criticalityList = incident.Lookups.ActiveStageCriticalityMapping[SupportIncidentCategoriesList.Codes.FeatureRequest];
			AssertEquals(2, criticalityList.Count);
			Assert(criticalityList.Contains(Core.Constants.CustomerService.CriticalityCodes.CR6_FeatureRequest));
			Assert(criticalityList.Contains(Core.Constants.CustomerService.CriticalityCodes.CR7_CustomisationRequest));

			criticalityList = incident.Lookups.ActiveStageCriticalityMapping[SupportIncidentCategoriesList.Codes.ContentDevelopment];
			AssertEquals(1, criticalityList.Count);
			Assert(criticalityList.Contains(Core.Constants.CustomerService.CriticalityCodes.CR5_Training));

			criticalityList = incident.Lookups.ActiveStageCriticalityMapping[SupportIncidentCategoriesList.Codes.ComplianceRequirement];
			AssertEquals(1, criticalityList.Count);
			Assert(criticalityList.Contains(Core.Constants.CustomerService.CriticalityCodes.CR8_ComplianceRequirement));

			criticalityList = incident.Lookups.ActiveStageCriticalityMapping[SupportIncidentCategoriesList.Codes.CustomerServiceRequest];
			AssertEquals(1, criticalityList.Count);
			Assert(criticalityList.Contains(Core.Constants.CustomerService.CriticalityCodes.CR9_CustomerServiceRequest));
		}

		public void TestCriticalityDefaultStageMapping()
		{
			var incident = Factory.New<SupportIncident>();
			var defaultStageMap = incident.Lookups.CriticalityDefaultStageMapping;

			AssertEquals(SupportIncidentCategoriesList.Codes.Support, defaultStageMap[Core.Constants.CustomerService.CriticalityCodes.CR1_SystemDown]);
			AssertEquals(SupportIncidentCategoriesList.Codes.Support, defaultStageMap[Core.Constants.CustomerService.CriticalityCodes.CR2_ModuleDown]);
			AssertEquals(SupportIncidentCategoriesList.Codes.Support, defaultStageMap[Core.Constants.CustomerService.CriticalityCodes.CR3_SingleFunctionNoWorkAround]);
			AssertEquals(SupportIncidentCategoriesList.Codes.Support, defaultStageMap[Core.Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround]);
			AssertEquals(SupportIncidentCategoriesList.Codes.Support, defaultStageMap[Core.Constants.CustomerService.CriticalityCodes.CR5_Training]);
			AssertEquals(SupportIncidentCategoriesList.Codes.FeatureRequest, defaultStageMap[Core.Constants.CustomerService.CriticalityCodes.CR6_FeatureRequest]);
			AssertEquals(SupportIncidentCategoriesList.Codes.FeatureRequest, defaultStageMap[Core.Constants.CustomerService.CriticalityCodes.CR7_CustomisationRequest]);
			AssertEquals(SupportIncidentCategoriesList.Codes.Support, defaultStageMap[Core.Constants.CustomerService.CriticalityCodes.CR8_ComplianceRequirement]);
			AssertEquals(SupportIncidentCategoriesList.Codes.CustomerServiceRequest, defaultStageMap[Core.Constants.CustomerService.CriticalityCodes.CR9_CustomerServiceRequest]);
		}

		public void TestClientCompanyList()
		{
			var org1 = Factory.NewWithValidTestData<EDIOrgHeader>();
			org1.OH_Code = "AAA123SYD";
			var enterprise1 = Factory.NewWithValidTestData<LicenceEnterprise>();
			enterprise1.LE_EnterpriseCode = "AAA";
			var db1 = Factory.NewWithValidTestData<LicenceDatabase>();
			db1.LD_ServerCode = "SYD";
			db1.LD_LE = enterprise1.PK;
			var clientCompany1 = Factory.New<ClientCompany>();
			clientCompany1.LCC_LD = db1.PK;
			clientCompany1.LCC_Code = "AAA";

			var org2 = Factory.NewWithValidTestData<EDIOrgHeader>();
			org2.OH_Code = "BBB456MEL";
			var enterprise2 = Factory.NewWithValidTestData<LicenceEnterprise>();
			enterprise2.LE_EnterpriseCode = "BBB";
			var db2 = Factory.NewWithValidTestData<LicenceDatabase>();
			db2.LD_ServerCode = "MEL";
			db2.LD_LE = enterprise2.PK;
			var clientCompany2a = Factory.New<ClientCompany>();
			clientCompany2a.LCC_LD = db2.PK;
			clientCompany2a.LCC_Code = "BB1";
			clientCompany2a.LCC_OH = org2.PK;
			var clientCompany2b = Factory.New<ClientCompany>();
			clientCompany2b.LCC_LD = db2.PK;
			clientCompany2b.LCC_Code = "BB2";
			var clientCompany2c = Factory.New<ClientCompany>();
			clientCompany2c.LCC_LD = db2.PK;
			clientCompany2c.LCC_Code = "BB3";
			clientCompany2c.LCC_OH = Factory.NewWithValidTestData<OrgHeader>().PK;

			Factory.Save();

			var clientPK = ZGuid.NewZGuid();
			clientCompany2a.LCC_ClientPK = clientPK;
			clientCompany2a.LCC_CreateTimeUtc = new ZDateTime(2015, 11, 4);
			clientCompany2a.LCC_DeactivateTimeUtc = new ZDateTime(2015, 11, 5);
			clientCompany2b.LCC_CreateTimeUtc = new ZDateTime(2015, 11, 5);

			Factory.Save();

			var incident = Factory.New<SupportIncident>();
			AssertEquals(0, incident.Lookups.ActiveClientCompanyCodeDescriptionPairList.Count);
			AssertEquals(0, incident.Lookups.ClientCompanyCodeDescriptionPairList.Count);

			incident.IM_LD = db1.PK;
			AssertEquals(1, incident.Lookups.ActiveClientCompanyCodeDescriptionPairList.Cast<CodeDescriptionPair>().Count(x => !(x is CategoryCodeDescriptionPair) && !string.IsNullOrEmpty(x.Code)));
			Assert(incident.Lookups.ActiveClientCompanyCodeDescriptionPairList.ContainsCode(clientCompany1.LCC_Code));
			AssertEquals(1, incident.Lookups.ClientCompanyCodeDescriptionPairList.Cast<CodeDescriptionPair>().Count(x => !(x is CategoryCodeDescriptionPair) && !string.IsNullOrEmpty(x.Code)));
			Assert(incident.Lookups.ClientCompanyCodeDescriptionPairList.ContainsCode(clientCompany1.LCC_Code));

			incident.IM_LD = db2.PK;
			incident.IM_LCC = clientCompany2a.PK;
			incident.IM_OH_Client = ZGuid.Empty;
			Factory.Save();

			AssertEquals(2, incident.Lookups.ActiveClientCompanyCodeDescriptionPairList.Cast<CodeDescriptionPair>().Count(x => !(x is CategoryCodeDescriptionPair) && !string.IsNullOrEmpty(x.Code)));
			Assert(incident.Lookups.ActiveClientCompanyCodeDescriptionPairList.ContainsCode(clientCompany2b.LCC_Code));
			Assert(incident.Lookups.ActiveClientCompanyCodeDescriptionPairList.ContainsCode(clientCompany2c.LCC_Code));
			AssertEquals(3, incident.Lookups.ClientCompanyCodeDescriptionPairList.Cast<CodeDescriptionPair>().Count(x => !(x is CategoryCodeDescriptionPair) && !string.IsNullOrEmpty(x.Code)));
			Assert(incident.Lookups.ClientCompanyCodeDescriptionPairList.ContainsCode(clientCompany2a.LCC_Code));
			Assert(incident.Lookups.ClientCompanyCodeDescriptionPairList.ContainsCode(clientCompany2b.LCC_Code));
			Assert(incident.Lookups.ClientCompanyCodeDescriptionPairList.ContainsCode(clientCompany2c.LCC_Code));
		}

		public void TestFilteredProductAreaList()
		{
			#region Menu Section Test Data
			{
				var menuSectionCollection = new SystemProductCollection();
				var entProduct = menuSectionCollection.AddNew("ENT", "ediEnterprise / CargoWise One", true);
				var ar1ModuleMapping = entProduct.ModuleMappings.AddNew("AR1", "ediArchiveManager", ProductAreaList.Codes.ARC, false);
				ar1ModuleMapping.SourceModuleMappings.AddNew("HAHA", ProductAreaList.Codes.EDC);
				entProduct.ModuleMappings.AddNew("CCC", "Test Module C", ProductAreaList.Codes.CUS, false);
				entProduct.ModuleMappings.AddNew("DDD", "Test Module D", ProductAreaList.Codes.CUS, false);

				var glwProduct = menuSectionCollection.AddNew("GLW", "Glow", false);
				glwProduct.ModuleMappings.AddNew("WEB", "Web Module", ProductAreaList.Codes.PAV, false);

				var hubProduct = menuSectionCollection.AddNew("HUB", "eHub", false);

				EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, menuSectionCollection);
			}
			#endregion

			#region CR8 Test Data
			{
				var cr8Collection = new SystemProductCollection();
				var entProduct = cr8Collection.AddNew("ENT", "ediEnterprise / CargoWise One", true);
				entProduct.ModuleMappings.AddNew("8EA", "8EA", ProductAreaList.Codes.GEO, false);
				entProduct.ModuleMappings.AddNew("8EB", "8EB", ProductAreaList.Codes.CUS, false);

				var glwProduct = cr8Collection.AddNew("GLW", "Glow", true);
				var moduleMapping = glwProduct.ModuleMappings.AddNew("8GA", "8GA", ProductAreaList.Codes.CIL, false);
				moduleMapping.SourceModuleMappings.AddNew("WOW", ProductAreaList.Codes.FIN);
				glwProduct.ModuleMappings.AddNew("8GB", "8GB", ProductAreaList.Codes.CIL, false);

				var hubProduct = cr8Collection.AddNew("HUB", "eHub", false);

				EDIDataRegistry.Instance.ProductAreaIncidentCr8Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, cr8Collection);
			}
			#endregion

			#region CR9 Test Data
			{
				var cr9Collection = new SystemProductCollection();
				var entProduct = cr9Collection.AddNew("ENT", "ediEnterprise / CargoWise One", true);
				entProduct.ModuleMappings.AddNew("9EA", "9EA", ProductAreaList.Codes.RAT, false);

				var glwProduct = cr9Collection.AddNew("GLW", "Glow", true);

				var hubProduct = cr9Collection.AddNew("HUB", "eHub", false);
				var moduleMapping = hubProduct.ModuleMappings.AddNew("8HA", "8HA", ProductAreaList.Codes.REF, false);
				moduleMapping.SourceModuleMappings.AddNew("BAM", ProductAreaList.Codes.HRM);
				hubProduct.ModuleMappings.AddNew("8HB", "8HB", ProductAreaList.Codes.REF, false);
				hubProduct.ModuleMappings.AddNew("8HC", "8HC", ProductAreaList.Codes.WFM, false);

				EDIDataRegistry.Instance.ProductAreaIncidentCr9Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, cr9Collection);
			}
			#endregion

			var incident = Factory.New<SupportIncident>();

			#region Menu Section
			{
				incident.IM_Priority = Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround;
				AssertEquals("Precondition", ModuleListType.MenuSection, incident.ModuleType);

				incident.IM_Product = "ENT";
				AssertContainsExactElementsInAnyOrder(
					new[]
					{
						ProductAreaList.Codes.ARC,
						ProductAreaList.Codes.CUS,
						ProductAreaList.Codes.EDC,
					},
					incident.Lookups.FilteredProductAreaList.Cast<ICodeDescription>().Select(x => x.Code));

				incident.IM_Product = "GLW";
				AssertContainsExactElementsInAnyOrder(
					new[]
					{
						ProductAreaList.Codes.PAV
					},
					incident.Lookups.FilteredProductAreaList.Cast<ICodeDescription>().Select(x => x.Code));

				incident.IM_Product = "HUB";
				AssertContainsExactElementsInAnyOrder(
					Array.Empty<string>(),
					incident.Lookups.FilteredProductAreaList.Cast<ICodeDescription>().Select(x => x.Code));
			}
			#endregion

			#region CR8
			{
				incident.IM_Priority = Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR8_ComplianceRequirement;
				AssertEquals("Precondition", ModuleListType.Cr8, incident.ModuleType);

				incident.IM_Product = "ENT";
				AssertContainsExactElementsInAnyOrder(
					new[]
					{
						ProductAreaList.Codes.GEO,
						ProductAreaList.Codes.CUS,
					},
					incident.Lookups.FilteredProductAreaList.Cast<ICodeDescription>().Select(x => x.Code));

				incident.IM_Product = "GLW";
				AssertContainsExactElementsInAnyOrder(
					new[]
					{
						ProductAreaList.Codes.CIL,
						ProductAreaList.Codes.FIN,
					},
					incident.Lookups.FilteredProductAreaList.Cast<ICodeDescription>().Select(x => x.Code));

				incident.IM_Product = "HUB";
				AssertContainsExactElementsInAnyOrder(
					Array.Empty<string>(),
					incident.Lookups.FilteredProductAreaList.Cast<ICodeDescription>().Select(x => x.Code));
			}
			#endregion

			#region CR9
			{
				incident.IM_Priority = Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR9_CustomerServiceRequest;
				AssertEquals("Precondition", ModuleListType.Cr9, incident.ModuleType);

				incident.IM_Product = "ENT";
				AssertContainsExactElementsInAnyOrder(
					new[]
					{
						ProductAreaList.Codes.RAT
					},
					incident.Lookups.FilteredProductAreaList.Cast<ICodeDescription>().Select(x => x.Code));

				incident.IM_Product = "GLW";
				AssertContainsExactElementsInAnyOrder(
					Array.Empty<string>(),
					incident.Lookups.FilteredProductAreaList.Cast<ICodeDescription>().Select(x => x.Code));

				incident.IM_Product = "HUB";
				AssertContainsExactElementsInAnyOrder(
					new[]
					{
						ProductAreaList.Codes.HRM,
						ProductAreaList.Codes.REF,
						ProductAreaList.Codes.WFM,
					},
					incident.Lookups.FilteredProductAreaList.Cast<ICodeDescription>().Select(x => x.Code));
			}
			#endregion

			#region Unspecified
			{
				incident.IM_Priority = ZString.Empty;
				AssertEquals("Precondition", ModuleListType.Unspecified, incident.ModuleType);

				incident.IM_Product = "ENT";
				AssertContainsExactElementsInAnyOrder(
					Array.Empty<string>(),
					incident.Lookups.FilteredProductAreaList.Cast<ICodeDescription>().Select(x => x.Code));

				incident.IM_Product = "GLW";
				AssertContainsExactElementsInAnyOrder(
					Array.Empty<string>(),
					incident.Lookups.FilteredProductAreaList.Cast<ICodeDescription>().Select(x => x.Code));

				incident.IM_Product = "HUB";
				AssertContainsExactElementsInAnyOrder(
					Array.Empty<string>(),
					incident.Lookups.FilteredProductAreaList.Cast<ICodeDescription>().Select(x => x.Code));
			}
			#endregion
		}

		public void TestGetResolutionAndClosureBehaviour()
		{
			var resolutionAndClosureBehaviourCollection = EDIDataRegistry.Instance.ResolutionAndClosureBehaviour.Value;

			var newCriticalityBehaviour = resolutionAndClosureBehaviourCollection.Add("CR4", (NoResString)"Single function not working with manual work around", true, false, null);
			resolutionAndClosureBehaviourCollection.AddSystemChildren(newCriticalityBehaviour);
			resolutionAndClosureBehaviourCollection.Add("ENT", (NoResString)"CargoWise", true, false, newCriticalityBehaviour);

			using (EDIDataRegistry.Instance.ResolutionAndClosureBehaviour.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, resolutionAndClosureBehaviourCollection))
			{
				var cr4CriticalityBehaviour = SupportIncidentLookups.GetResolutionAndClosureBehaviour(Factory, "CR4", ZGuid.Empty);
				var cr3CriticalityBehaviour = SupportIncidentLookups.GetResolutionAndClosureBehaviour(Factory, "CR3", ZGuid.Empty);
				var entProductBehaviour = SupportIncidentLookups.GetResolutionAndClosureBehaviour(Factory, "ENT", cr4CriticalityBehaviour.ID);
				var cw1ProductBehaviour = SupportIncidentLookups.GetResolutionAndClosureBehaviour(Factory, "CW1", cr4CriticalityBehaviour.ID);
				AssertEquals(cr4CriticalityBehaviour.Description, "Single function not working with manual work around");
				AssertEquals(cr3CriticalityBehaviour.Description, "All Criticalities that are not listed");
				AssertEquals(entProductBehaviour.Description, "CargoWise");
				AssertEquals(cw1ProductBehaviour.Description, "All Products that are not listed");
			}
		}

		public void TestGetResolutionAndClosureBehaviourFallbacks()
		{
			var resolutionAndClosureBehaviourCollection = EDIDataRegistry.Instance.ResolutionAndClosureBehaviour.Value;

			AssertEquals("Precondition: Should have the ALL criticalities and ALL products default fallback", 2, resolutionAndClosureBehaviourCollection.Count);
			var resolutionAndClosureBehaviourArray = resolutionAndClosureBehaviourCollection.Cast<ResolutionAndClosureBehaviour>();

			var defaultCriticalityBehaviour = resolutionAndClosureBehaviourArray.FirstOrDefault(x => x.ParentID.IsEmpty);
			var defaultProductBehaviour = resolutionAndClosureBehaviourArray.FirstOrDefault(x => x.ParentID == defaultCriticalityBehaviour.PK);

			var defaultCriticalityENTBehaviour = resolutionAndClosureBehaviourCollection.AddNew();
			defaultCriticalityENTBehaviour.Code = ProductTypes.Codes.Enterprise;
			defaultCriticalityENTBehaviour.ParentID = defaultCriticalityBehaviour.PK;

			var expectedCr4Behaviour = resolutionAndClosureBehaviourCollection.AddNew();
			expectedCr4Behaviour.Code = Core.Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround;
			resolutionAndClosureBehaviourCollection.AddSystemChildren(expectedCr4Behaviour);
			AssertEquals("Precondition: Should have the ALL products fallback for cr4", 5, resolutionAndClosureBehaviourCollection.Count);

			var expectedCr4ALLProductBehaviour = resolutionAndClosureBehaviourCollection.Cast<ResolutionAndClosureBehaviour>().FirstOrDefault(x => x.ParentID == expectedCr4Behaviour.PK);

			var expectedCr4ENTBehaviour = resolutionAndClosureBehaviourCollection.AddNew();
			expectedCr4ENTBehaviour.Code = ProductTypes.Codes.Enterprise;
			expectedCr4ENTBehaviour.ParentID = expectedCr4Behaviour.PK;

			var expectedCr4ZZZBehaviour = resolutionAndClosureBehaviourCollection.AddNew();
			expectedCr4ZZZBehaviour.Code = "ZZZ";
			expectedCr4ZZZBehaviour.ParentID = expectedCr4Behaviour.PK;

			using (EDIDataRegistry.Instance.ResolutionAndClosureBehaviour.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, resolutionAndClosureBehaviourCollection))
			{
				var cr4CriticalityBehaviour = SupportIncidentLookups.GetResolutionAndClosureBehaviour(Factory, Core.Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround, ZGuid.Empty);
				var cr3CriticalityBehaviour = SupportIncidentLookups.GetResolutionAndClosureBehaviour(Factory, Core.Constants.CustomerService.CriticalityCodes.CR3_SingleFunctionNoWorkAround, ZGuid.Empty);
				var cr4ENTBehaviour = SupportIncidentLookups.GetResolutionAndClosureBehaviour(Factory, ProductTypes.Codes.Enterprise, cr4CriticalityBehaviour.ID);
				AssertEquals("CR4 behaviour has been specified", expectedCr4Behaviour.ID, cr4CriticalityBehaviour.ID);
				AssertEquals("CR3 behaviour should fallback to default", defaultCriticalityBehaviour.PK, cr3CriticalityBehaviour.ID);

				AssertEquals("CR4 ENT behaviour has been specified", expectedCr4ENTBehaviour.PK, SupportIncidentLookups.GetResolutionAndClosureBehaviour(Factory, ProductTypes.Codes.Enterprise, cr4CriticalityBehaviour.ID).ID);
				AssertEquals("CR4 ZZZ behaviour has been specified", expectedCr4ZZZBehaviour.PK, SupportIncidentLookups.GetResolutionAndClosureBehaviour(Factory, "ZZZ", cr4CriticalityBehaviour.ID).ID);
				AssertEquals("CR4 ABC behaviour has not been specified so should fallback to CR4 ALL products", expectedCr4ALLProductBehaviour.PK, SupportIncidentLookups.GetResolutionAndClosureBehaviour(Factory, "ABC", cr4CriticalityBehaviour.ID).ID);
				AssertEquals("CR3 behaviour is default criticality. ENT behaviour has been specified", defaultCriticalityENTBehaviour.PK, SupportIncidentLookups.GetResolutionAndClosureBehaviour(Factory, ProductTypes.Codes.Enterprise, cr3CriticalityBehaviour.ID).ID);
				AssertEquals("CR3 ZZZ combionation behaviour has not been specified so should fallback to default ALL criticalities, ALL products", defaultProductBehaviour.PK, SupportIncidentLookups.GetResolutionAndClosureBehaviour(Factory, "ZZZ", cr3CriticalityBehaviour.ID).ID);
			}
		}

		public void TestGetModuleList()
		{
			SetUpTestData();
			TestGetModuleListEnabledModulesOnly();
			TestGetModuleListAllModules();
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
					incident.Lookups.ModuleListEnabledModulesOnly.Cast<ICodeDescription>());

				expectedIncidentModuleList.Clear();

				incident.IM_Product = "GLW";
				expectedIncidentModuleList.AddPair("WEB", "Web Module (Enabled)");

				AssertContainsExactElementsInAnyOrder(
					expectedIncidentModuleList,
					incident.Lookups.ModuleListEnabledModulesOnly.Cast<ICodeDescription>());
				expectedIncidentModuleList.Clear();
			}
			#endregion

			#region CR8
			{
				incident.IM_Priority = Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR8_ComplianceRequirement;
				incident.IM_Product = "ENT";
				AssertEquals("Precondition", ModuleListType.Cr8, incident.ModuleType);
				var incidentModuleList = incident.Lookups.ModuleListEnabledModulesOnly;
				AssertEquals(incidentModuleList.Count, 0);

				incident.IM_Product = "GLW";
				incidentModuleList = incident.Lookups.ModuleListEnabledModulesOnly;
				AssertEquals(incidentModuleList.Count, 0);

				incident.IM_Product = "HUB";
				incidentModuleList = incident.Lookups.ModuleListEnabledModulesOnly;
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
					incident.Lookups.ModuleListEnabledModulesOnly.Cast<ICodeDescription>());
				expectedIncidentModuleList.Clear();

				incident.IM_Product = "GLW";
				var incidentModuleList = incident.Lookups.ModuleListEnabledModulesOnly;
				AssertEquals(incidentModuleList.Count, 0);

				incident.IM_Product = "HUB";
				incidentModuleList = incident.Lookups.ModuleListEnabledModulesOnly;
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
					incident.Lookups.ModuleListEnabledModulesOnly.Cast<ICodeDescription>());

				incident.IM_Product = "GLW";
				var expectedGlowIncidentModuleList = new CodeDescriptionPairList();
				expectedGlowIncidentModuleList.AddPair("WEB", "Web Module (Enabled)");
				AssertContainsExactElementsInAnyOrder(
					expectedGlowIncidentModuleList,
					incident.Lookups.ModuleListEnabledModulesOnly.Cast<ICodeDescription>());

				incident.IM_Product = "HUB";
				AssertEquals(incident.Lookups.ModuleListEnabledModulesOnly.Count, 0);
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
			AssertEquals(1, incident.Lookups.ModuleListEnabledModulesOnly.Count);
			AssertEquals("XXX", incident.Lookups.ModuleListEnabledModulesOnly[0].Code);
		}

		void TestGetModuleListAllModules()
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
					incident.Lookups.ModuleListAllModules.Cast<ICodeDescription>());

				incident.IM_Product = "HUB";
				AssertContainsExactElementsInAnyOrder(
					expectedIncidentModuleList,
					incident.Lookups.ModuleListAllModules.Cast<ICodeDescription>());

				expectedIncidentModuleList.Clear();

				incident.IM_Product = "GLW";
				expectedIncidentModuleList.AddPair("WEB", "Web Module (Enabled)");

				AssertContainsExactElementsInAnyOrder(
					expectedIncidentModuleList,
					incident.Lookups.ModuleListAllModules.Cast<ICodeDescription>());
				expectedIncidentModuleList.Clear();
			}
			#endregion

			#region CR8
			{
				incident.IM_Priority = Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR8_ComplianceRequirement;
				incident.IM_Product = "ENT";
				AssertEquals("Precondition", ModuleListType.Cr8, incident.ModuleType);
				var expectedIncidentModuleList = new CodeDescriptionPairList();
				expectedIncidentModuleList.AddPair("8EA", "8EA (Disabled)");
				expectedIncidentModuleList.AddPair("8EB", "8EB (Disabled)");
				AssertContainsExactElementsInAnyOrder(expectedIncidentModuleList, incident.Lookups.ModuleListAllModules.Cast<ICodeDescription>());
				expectedIncidentModuleList.Clear();

				incident.IM_Product = "GLW";
				expectedIncidentModuleList.AddPair("8GA", "8GA (Disabled)");
				expectedIncidentModuleList.AddPair("8GB", "8GB (Disabled)");
				AssertContainsExactElementsInAnyOrder(expectedIncidentModuleList, incident.Lookups.ModuleListAllModules.Cast<ICodeDescription>());

				incident.IM_Product = "HUB";
				var incidentModuleList = incident.Lookups.ModuleListAllModules;
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
					incident.Lookups.ModuleListAllModules.Cast<ICodeDescription>());
				expectedIncidentModuleList.Clear();

				incident.IM_Product = "GLW";
				var incidentModuleList = incident.Lookups.ModuleListAllModules;
				AssertEquals(incidentModuleList.Count, 0);

				incident.IM_Product = "HUB";
				expectedIncidentModuleList.AddPair("8HB", "8HB (Disabled)");
				expectedIncidentModuleList.AddPair("8HA", "8HA (Disabled)");
				expectedIncidentModuleList.AddPair("8HC", "8HC (Disabled)");
				AssertContainsExactElementsInAnyOrder(
					expectedIncidentModuleList,
					incident.Lookups.ModuleListAllModules.Cast<ICodeDescription>());
				expectedIncidentModuleList.Clear();
			}
			#endregion

			#region Unspecified
			{
				incident.IM_Priority = ZString.Empty;
				AssertEquals("Precondition", ModuleListType.Unspecified, incident.ModuleType);

				incident.IM_Product = "ENT";
				var expectedENTIncidentModuleList = new CodeDescriptionPairList();
				expectedENTIncidentModuleList.AddPair("8EA", "8EA (Disabled)");
				expectedENTIncidentModuleList.AddPair("8EB", "8EB (Disabled)");
				expectedENTIncidentModuleList.AddPair("9EA", "9EA (Enabled)");
				expectedENTIncidentModuleList.AddPair("9EB", "9EB (Enabled)");
				expectedENTIncidentModuleList.AddPair("AR1", "ediArchiveManager (Enabled)");
				expectedENTIncidentModuleList.AddPair("CCC", "Test Module C (Enabled)");
				expectedENTIncidentModuleList.AddPair("DDD", "Test Module D (Enabled)");
				AssertContainsExactElementsInAnyOrder(
					expectedENTIncidentModuleList,
					incident.Lookups.ModuleListAllModules.Cast<ICodeDescription>());

				incident.IM_Product = "GLW";
				var expectedGlowIncidentModuleList = new CodeDescriptionPairList();
				expectedGlowIncidentModuleList.AddPair("WEB", "Web Module (Enabled)");
				expectedGlowIncidentModuleList.AddPair("8GA", "8GA (Disabled)");
				expectedGlowIncidentModuleList.AddPair("8GB", "8GB (Disabled)");
				AssertContainsExactElementsInAnyOrder(
					expectedGlowIncidentModuleList,
					incident.Lookups.ModuleListAllModules.Cast<ICodeDescription>());

				incident.IM_Product = "HUB";
				var expectedHUBIncidentModuleList = new CodeDescriptionPairList();
				expectedHUBIncidentModuleList.AddPair("CCC", "Test Module C (Enabled)");
				expectedHUBIncidentModuleList.AddPair("DDD", "Test Module D (Enabled)");
				expectedHUBIncidentModuleList.AddPair("AR1", "ediArchiveManager (Enabled)");
				expectedHUBIncidentModuleList.AddPair("8HA", "8HA (Disabled)");
				expectedHUBIncidentModuleList.AddPair("8HB", "8HB (Disabled)");
				expectedHUBIncidentModuleList.AddPair("8HC", "8HC (Disabled)");

				AssertContainsExactElementsInAnyOrder(
					expectedHUBIncidentModuleList,
					incident.Lookups.ModuleListAllModules.Cast<ICodeDescription>());
			}
			#endregion
		}

		public void TestLanguages()
		{
			var incident = Factory.New<SupportIncident>();
			AssertContainsExactElementsInAnyOrder(new CodeDescriptionPairList(OLookUpEditType.Language), incident.Lookups.Languages);
		}

		public void TestEnterprise()
		{
			var incident = Factory.New<SupportIncident>();
			AssertNotNull(incident.Lookups.EnterpriseList);
			AssertNotNull(incident.Lookups.EnterpriseCodeList);
			AssertType(typeof(LicenceEnterpriseCollection), incident.Lookups.EnterpriseList);
			AssertType(typeof(LicenceEnterpriseCollectionForEntCodeFilter), incident.Lookups.EnterpriseCodeList);
		}

		SupportIncident GetIncident()
		{
			return Factory.New<SupportIncident>();
		}

		public void TestServiceTypeList_GetRightCollection()
		{
			SetupServiceTypeTestData();
			var incident = Factory.New<SupportIncident>();
			incident.IM_Product = "AAA";

			#region Service Type
			{
				incident.IM_Priority = Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround;
				incident.IM_ProgramArea = "XRM";
				incident.IM_Module = "AA1";

				var serviceTypeList = incident.Lookups.ServiceTypeList;
				AssertEquals("TEA", serviceTypeList[0].Code);
				AssertEquals("Tear down", serviceTypeList[0].Description);
			}
			#endregion

			#region CR8
			{
				incident.IM_Priority = Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR8_ComplianceRequirement;
				incident.IM_ProgramArea = "XRM";
				incident.IM_Module = "AA1";

				var serviceTypeList = incident.Lookups.ServiceTypeList;
				AssertEquals("CON", serviceTypeList[0].Code);
				AssertEquals("Configuration", serviceTypeList[0].Description);
			}
			#endregion

			#region CR9
			{
				incident.IM_Priority = Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR9_CustomerServiceRequest;
				incident.IM_ProgramArea = "XRM";
				incident.IM_Module = "AA1";

				var serviceTypeList = incident.Lookups.ServiceTypeList;
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
