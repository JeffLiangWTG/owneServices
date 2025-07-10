using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Core;
using Enterprise.CustomerService.Business;
using Enterprise.ProcessManagement.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	public class IncidentManagementGroupValidationTest : BusinessObjectValidationTestCase
	{
		public void TestING_Type()
		{
			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group.Validation.ValidateING_Type();
			AssertHasError(group.ING_TypeInfo, "Please enter a Group Type.");

			group.ING_Type = IncidentGroupStatusConfigurationConstants.MajorIncidentCode;
			AssertNoErrors(group.ING_TypeInfo);

			group.ING_Type = "XXX";
			AssertHasError(group.ING_TypeInfo, "Enter a valid Group Type.");

			var otherIncidentType = "OTH";
			var registryValue = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value;
			registryValue.AddNew(otherIncidentType, "Other Management group type");
			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);

			Factory.ClearCachedValue<CodeDescriptionPairList>("IncidentManagementGroupLookups.Types");
			group.ING_Type = otherIncidentType;
			AssertNoErrors(group.ING_TypeInfo);
		}

		public void TestING_Status()
		{
			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group.ING_Status = IncidentGroupStatusConfigurationConstants.ConfigurationINV.Code;
			AssertHasError(group.ING_StatusInfo, "Type should be set before Status");

			group.ING_Type = IncidentGroupStatusConfigurationConstants.MajorIncidentCode;
			AssertNoErrors(group.ING_StatusInfo);

			group.ING_Status = IncidentGroupStatusConfigurationConstants.ConfigurationRSV.Code;
			AssertNoErrors(group.ING_StatusInfo);

			var newCode = "OZZ";
			group.ING_Status = newCode;
			AssertHasError(group.ING_StatusInfo, "The current status is invalid. You should reload the form to select a valid status before continuing.");

			var otherIncidentType = "OTH";
			var newCode2 = "OZY";
			var registryValue = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value;
			registryValue[0].IncidentGroupStatusConfigurations.AddNew(newCode, "description", IncidentGroupStatusConfigurationConstants.TriggerOnDefaultValue);
			var otherGroup = registryValue.AddNew(otherIncidentType, "Other Management group type");
			otherGroup.IncidentGroupStatusConfigurations.AddNew(newCode2, "description2", IncidentGroupStatusConfigurationConstants.TriggerOnDefaultValue);
			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);
			group.ING_Type = string.Empty;
			group.ING_Type = IncidentGroupStatusConfigurationConstants.MajorIncidentCode;

			Factory.ClearCachedValue<CodeDescriptionPairList>("IncidentManagementGroupLookups.StageList" + group.ING_Type);
			AssertNoErrors(group.ING_StatusInfo);

			group.ING_Type = otherIncidentType;
			Factory.ClearCachedValue<CodeDescriptionPairList>("IncidentManagementGroupLookups.StageList" + group.ING_Type);
			group.ING_Status = newCode;
			AssertHasError(group.ING_StatusInfo, "The current status is invalid. You should reload the form to select a valid status before continuing.");

			group.ING_Status = newCode2;
			AssertNoErrors(group.ING_StatusInfo);

			group.ING_Status = string.Empty;
			AssertHasError(group.ING_StatusInfo, "Please enter a value.");
		}

		public void TestING_GS_NKGroupOwner()
		{
			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();

			group.ING_Type = IncidentGroupStatusConfigurationConstants.MajorIncidentCode;
			group.ING_Status = IncidentGroupStatusConfigurationConstants.ConfigurationRSV.Code;
			group.ING_GS_NKGroupOwner = null;
			AssertHasError(group.ING_GS_NKGroupOwnerInfo, "Please enter a Group Owner.");

			group.ING_GS_NKGroupOwner = "AAA";
			AssertHasError(group.ING_GS_NKGroupOwnerInfo, "Enter a valid Group Owner.");

			group.ING_GS_NKGroupOwner = "E";
			AssertNoErrors(group.ING_GS_NKGroupOwnerInfo);
		}

		public void TestING_ServiceOutage()
		{
			var group = Factory.New<IncidentManagementGroup>();
			group.ING_ServiceOutage = string.Empty;
			AssertHasError(group.ING_ServiceOutageInfo, "Please enter a Service Outage.");

			group.ING_ServiceOutage = IncidentManagementGroupConstants.ServiceOutageCodes.Active;
			AssertNoErrors(group.ING_ServiceOutageInfo);
			group.ING_ServiceOutage = "ZZZ";
			AssertHasError(group.ING_ServiceOutageInfo, "Enter a valid Service Outage.");
			group.ING_ServiceOutage = IncidentManagementGroupConstants.ServiceOutageCodes.Restored;
			AssertNoErrors(group.ING_ServiceOutageInfo);
			group.ING_ServiceOutage = IncidentManagementGroupConstants.ServiceOutageCodes.Downgraded;
			AssertNoErrors(group.ING_ServiceOutageInfo);
		}

		public void TestING_BusinessImpact()
		{
			var group = Factory.New<IncidentManagementGroup>();
			Assert("Precondition", group.ING_BusinessImpact.IsEmpty);
			group.Validation.ValidateING_BusinessImpact();
			AssertHasError(group.ING_BusinessImpactInfo, "Please enter a Business Impact.");

			group.ING_BusinessImpact = IncidentManagementGroupConstants.BusinessImpactCodes.HighImpact;
			AssertNoErrors(group.ING_BusinessImpactInfo);
			group.ING_BusinessImpact = "ZZZ";
			AssertHasError(group.ING_BusinessImpactInfo, "Enter a valid Business Impact.");
			group.ING_BusinessImpact = IncidentManagementGroupConstants.BusinessImpactCodes.Significant;
			AssertNoErrors(group.ING_BusinessImpactInfo);
			group.ING_BusinessImpact = IncidentManagementGroupConstants.BusinessImpactCodes.MinorLocalised;
			AssertNoErrors(group.ING_BusinessImpactInfo);
		}

		public void TestING_Urgency()
		{
			var group = Factory.New<IncidentManagementGroup>();
			Assert("Precondition", group.ING_Urgency.IsEmpty);
			group.Validation.ValidateING_Urgency();
			AssertHasError(group.ING_UrgencyInfo, "Please enter an Urgency.");

			group.ING_Urgency = IncidentManagementGroupConstants.UrgencyCodes.Critical;
			AssertNoErrors(group.ING_UrgencyInfo);
			group.ING_Urgency = "ZZZ";
			AssertHasError(group.ING_UrgencyInfo, "Enter a valid Urgency.");
			group.ING_Urgency = IncidentManagementGroupConstants.UrgencyCodes.Medium;
			AssertNoErrors(group.ING_UrgencyInfo);
			group.ING_Urgency = IncidentManagementGroupConstants.UrgencyCodes.Low;
			AssertNoErrors(group.ING_UrgencyInfo);
		}

		public void TestING_Priority()
		{
			var group = Factory.New<IncidentManagementGroup>();
			Assert("Precondition", group.ING_Priority.IsEmpty);
			group.Validation.ValidateING_Priority();
			AssertHasError(group.ING_PriorityInfo, "Please enter a Criticality.");

			var incidentApprovalLookups = new IncidentApprovalLookups(null);
			var validCodes = incidentApprovalLookups.CriticalityList.GetAllCodes();
			group.ING_Priority = validCodes[0];
			AssertNoErrors(group.ING_PriorityInfo);
			group.ING_Priority = "ZZZ";
			AssertHasError(group.ING_PriorityInfo, "Enter a valid Criticality.");
			group.ING_Priority = validCodes[1];
			AssertNoErrors(group.ING_PriorityInfo);
		}

		public void TestING_Product()
		{
			var group = Factory.New<IncidentManagementGroup>();
			Assert("Precondition", group.ING_Product.IsEmpty);
			group.Validation.ValidateING_Product();
			AssertHasError(group.ING_ProductInfo, "Please enter a Product.");

			group.ING_Product = ProductTypes.Codes.Enterprise;
			AssertNoErrors(group.ING_ProductInfo);
			group.ING_Product = "ZZZ";
			AssertHasError(group.ING_ProductInfo, "Enter a valid Product.");
			group.ING_Product = ProductTypes.Codes.Enterprise;
			AssertNoErrors(group.ING_ProductInfo);
		}

		public void TestING_ProductArea()
		{
			var group = Factory.New<IncidentManagementGroup>();
			Assert("Precondition", group.ING_ProductArea.IsEmpty);
			group.Validation.ValidateING_ProductArea();
			AssertNoErrors(group.ING_ProductAreaInfo);

			var areas = new CodeDescriptionPairList();
			var validCode1 = "XRM";
			areas.AddPair(validCode1, "XRM");
			EDIDataRegistry.Instance.ProductAreas.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, areas);

			group.ING_Priority = "CR5";
			group.ING_Product = ProductTypes.Codes.Enterprise;
			group.ING_ProductArea = validCode1;
			AssertNoErrors(group.ING_ProductAreaInfo);
			group.ING_ProductArea = "ZZZ";
			AssertHasError(group.ING_ProductAreaInfo, "Enter a valid Product Area.");
			group.ING_ProductArea = validCode1;
			AssertNoErrors(group.ING_ProductAreaInfo);
		}

		#region Module

		public void TestING_Module()
		{
			TestING_ModuleBaseBehaviour();
			TestING_ModuleDisablingModulesNoImpactOnExistingModules();
		}

		void TestING_ModuleBaseBehaviour()
		{
			EDIDataRegistry.CreateProductsAndModulesForTest();

			var collection = new SystemProductCollection();
			var product = collection.AddNew(ProductTypes.Codes.Enterprise, "test", true);
			var moduleMapping1 = product.ModuleMappings.AddNew("CAT", "Category Module", ProductAreaList.Codes.ARC, true);
			var moduleMapping2 = product.ModuleMappings.AddNew("CA2", "Category Module 2", "", true);
			var moduleMapping3 = product.ModuleMappings.AddNew("CAA", "Category Module 3 Disabled", "", true, false, false);
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			var collection1 = new SystemProductCollection();
			product = collection1.AddNew(ProductTypes.Codes.Enterprise, "test", true);
			var moduleMapping4 = product.ModuleMappings.AddNew("888", "CR8 Module", ProductAreaList.Codes.CUS, true);
			var moduleMapping5 = product.ModuleMappings.AddNew("882", "CR8 Module 2", "", true);
			var moduleMapping6 = product.ModuleMappings.AddNew("884", "CR8 Module 3 Disabled", "", true, false, false);
			EDIDataRegistry.Instance.ProductAreaIncidentCr8Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection1);

			var collection2 = new SystemProductCollection();
			product = collection2.AddNew(ProductTypes.Codes.Enterprise, "test", true);
			var moduleMapping7 = product.ModuleMappings.AddNew("999", "CR9 Module", ProductAreaList.Codes.GEO, true);
			var moduleMapping8 = product.ModuleMappings.AddNew("992", "CR9 Module 2", "", true);
			var moduleMapping9 = product.ModuleMappings.AddNew("993", "CR9 Module 3 Disabled", ProductAreaList.Codes.CRM, true, false, false);
			EDIDataRegistry.Instance.ProductAreaIncidentCr9Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection2);
			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();

			group.ING_Product = "ENT";
			group.ING_Module = "XXX";
			AssertListValidationInvalidCodeError(group.ING_ModuleInfo, true);

			group.ING_Priority = "CR4";
			group.ING_Module = "CAT";
			group.ING_ProductArea = "";
			AssertEquals("Precondition", moduleMapping1.IsEnabled, true);
			AssertNoErrors(group.ING_ModuleInfo);

			group.ING_Priority = "CR8";
			group.ING_Module = "888";
			group.ING_ProductArea = "";
			AssertEquals("Precondition", moduleMapping4.IsEnabled, true);
			AssertNoErrors(group.ING_ModuleInfo);

			group.ING_Priority = "CR9";
			group.ING_Module = "999";
			group.ING_ProductArea = "";
			AssertEquals("Precondition", moduleMapping7.IsEnabled, true);
			AssertNoErrors(group.ING_ModuleInfo);

			moduleMapping1.IsEnabled = true;
			EDIDataRegistry.Instance.ProductAreaIncidentCr8Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection1);
			AssertEquals("Precondition", moduleMapping1.IsEnabled, true);
			group.ING_Priority = "CR8";
			group.ING_Module = "CAT";
			AssertListValidationInvalidCodeError(group.ING_ModuleInfo, true);

			moduleMapping4.IsEnabled = true;
			EDIDataRegistry.Instance.ProductAreaIncidentCr8Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection1);
			AssertEquals("Precondition", moduleMapping4.IsEnabled, true);
			group.ING_Priority = "CR9";
			group.ING_Module = "888";
			AssertListValidationInvalidCodeError(group.ING_ModuleInfo, true);

			moduleMapping5.IsEnabled = true;
			EDIDataRegistry.Instance.ProductAreaIncidentCr9Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection2);
			AssertEquals("Precondition", moduleMapping5.IsEnabled, true);
			group.ING_Priority = "CR4";
			group.ING_Module = "999";
			AssertListValidationInvalidCodeError(group.ING_ModuleInfo, true);

			AssertEquals("Precondition", moduleMapping3.IsEnabled, false);
			group.ING_Priority = "CR4";
			group.ING_Module = "CAA";
			AssertListValidationInvalidCodeError(group.ING_ModuleInfo, true);

			AssertEquals("Precondition", moduleMapping6.IsEnabled, false);
			group.ING_Priority = "CR8";
			group.ING_Module = "884";
			AssertListValidationInvalidCodeError(group.ING_ModuleInfo, true);

			AssertEquals("Precondition", moduleMapping9.IsEnabled, false);
			group.ING_Priority = "CR9";
			group.ING_Module = "993";
			AssertListValidationInvalidCodeError(group.ING_ModuleInfo, true);

			group.ING_Priority = "CR4";
			group.ING_Module = "CA2";
			AssertNoErrors(group.ING_ModuleInfo);
			AssertHasWarning(group.ING_ModuleInfo, "Not linked to any product areas.");

			group.ING_Priority = "CR8";
			group.ING_Module = "882";
			AssertNoErrors(group.ING_ModuleInfo);
			AssertHasWarning(group.ING_ModuleInfo, "Not linked to any product areas.");

			group.ING_Priority = "CR9";
			group.ING_Module = "992";
			AssertNoErrors(group.ING_ModuleInfo);
			AssertHasWarning(group.ING_ModuleInfo, "Not linked to any product areas.");

			Factory.Save();

			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new SystemProductCollection());
			EDIDataRegistry.Instance.ProductAreaIncidentCr8Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new SystemProductCollection());
			EDIDataRegistry.Instance.ProductAreaIncidentCr9Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new SystemProductCollection());

			group.Validation.ValidateING_Module();
			AssertNoErrors(group.ING_ModuleInfo);

			group.ING_ProductArea = "XXX";
			AssertEquals("Precondition", true, group.ING_ProductAreaInfo.HasChanges);
			group.Validation.ValidateING_Module();
			AssertListValidationInvalidCodeError(group.ING_ModuleInfo, true);

			group.ING_ProductArea = "";
			AssertEquals("Precondition", false, group.ING_ProductAreaInfo.HasChanges);
			group.Validation.ValidateING_Module();
			AssertNoErrors(group.ING_ModuleInfo);

			group.ING_Module = "882";
			AssertEquals("Precondition", true, group.ING_ModuleInfo.HasChanges);
			AssertListValidationInvalidCodeError(group.ING_ModuleInfo, true);
		}

		void TestING_ModuleDisablingModulesNoImpactOnExistingModules()
		{
			EDIDataRegistry.CreateProductsAndModulesForTest();

			var collection = new SystemProductCollection();
			var product = collection.AddNew(ProductTypes.Codes.Enterprise, "test", true);
			var moduleMapping1 = product.ModuleMappings.AddNew("CAT", "Category Module", ProductAreaList.Codes.ARC, true);
			var moduleMapping2 = product.ModuleMappings.AddNew("CA2", "Category Module 2", "", true);
			var moduleMapping3 = product.ModuleMappings.AddNew("CAA", "Category Module 3 Disabled", "", true, false, false);
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			var collection1 = new SystemProductCollection();
			product = collection1.AddNew(ProductTypes.Codes.Enterprise, "test", true);
			var moduleMapping4 = product.ModuleMappings.AddNew("888", "CR8 Module", ProductAreaList.Codes.CUS, true);
			var moduleMapping5 = product.ModuleMappings.AddNew("882", "CR8 Module 2", "", true);
			var moduleMapping6 = product.ModuleMappings.AddNew("884", "CR8 Module 3 Disabled", "", true, false, false);
			EDIDataRegistry.Instance.ProductAreaIncidentCr8Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection1);

			var collection2 = new SystemProductCollection();
			product = collection2.AddNew(ProductTypes.Codes.Enterprise, "test", true);
			var moduleMapping7 = product.ModuleMappings.AddNew("999", "CR9 Module", ProductAreaList.Codes.GEO, true);
			var moduleMapping8 = product.ModuleMappings.AddNew("992", "CR9 Module 2", "", true);
			var moduleMapping9 = product.ModuleMappings.AddNew("993", "CR9 Module 3 Disabled", ProductAreaList.Codes.CRM, true, false, false);
			EDIDataRegistry.Instance.ProductAreaIncidentCr9Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection2);
			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group.ING_Product = "ENT";

			group.ING_Priority = "CR4";
			group.ING_Module = "CAT";
			group.ING_ProductArea = "";
			AssertEquals("Precondition", moduleMapping1.IsEnabled, true);
			AssertNoErrors(group.ING_ModuleInfo);
			moduleMapping1.IsEnabled = false;
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			Factory.Save();
			group.Validation.ValidateING_Module();
			AssertEquals("Precondition", moduleMapping1.IsEnabled, false);
			AssertNoErrors(group.ING_ModuleInfo);

			group.ING_Priority = "CR8";
			group.ING_Module = "888";
			group.ING_ProductArea = "";
			AssertEquals("Precondition", moduleMapping4.IsEnabled, true);
			AssertNoErrors(group.ING_ModuleInfo);
			moduleMapping4.IsEnabled = false;
			EDIDataRegistry.Instance.ProductAreaIncidentCr8Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection1);
			Factory.Save();
			group.Validation.ValidateING_Module();
			AssertEquals("Precondition", moduleMapping4.IsEnabled, false);
			AssertNoErrors(group.ING_ModuleInfo);

			group.ING_Priority = "CR9";
			group.ING_Module = "999";
			group.ING_ProductArea = "";
			AssertEquals("Precondition", moduleMapping7.IsEnabled, true);
			AssertNoErrors(group.ING_ModuleInfo);
			moduleMapping7.IsEnabled = false;
			EDIDataRegistry.Instance.ProductAreaIncidentCr9Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection2);
			Factory.Save();
			group.Validation.ValidateING_Module();
			AssertEquals("Precondition", moduleMapping7.IsEnabled, false);
			AssertNoErrors(group.ING_ModuleInfo);
		}

		public void TestING_ModuleClearRegistryModules()
		{
			EDIDataRegistry.CreateProductsAndModulesForTest();

			var collection = new SystemProductCollection();
			var product = collection.AddNew(ProductTypes.Codes.Enterprise, "test", true);
			var moduleMapping1 = product.ModuleMappings.AddNew("CAT", "Category Module", ProductAreaList.Codes.ARC, true);
			var moduleMapping2 = product.ModuleMappings.AddNew("CA2", "Category Module 2", "", true);
			var moduleMapping3 = product.ModuleMappings.AddNew("CAA", "Category Module 3 Disabled", "", true, false, false);
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			var collection1 = new SystemProductCollection();
			product = collection1.AddNew(ProductTypes.Codes.Enterprise, "test", true);
			var moduleMapping4 = product.ModuleMappings.AddNew("888", "CR8 Module", ProductAreaList.Codes.CUS, true);
			var moduleMapping5 = product.ModuleMappings.AddNew("882", "CR8 Module 2", "", true);
			var moduleMapping6 = product.ModuleMappings.AddNew("884", "CR8 Module 3 Disabled", "", true, false, false);
			EDIDataRegistry.Instance.ProductAreaIncidentCr8Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection1);

			var collection2 = new SystemProductCollection();
			product = collection2.AddNew(ProductTypes.Codes.Enterprise, "test", true);
			var moduleMapping7 = product.ModuleMappings.AddNew("999", "CR9 Module", ProductAreaList.Codes.GEO, true);
			var moduleMapping8 = product.ModuleMappings.AddNew("992", "CR9 Module 2", "", true);
			var moduleMapping9 = product.ModuleMappings.AddNew("993", "CR9 Module 3 Disabled", ProductAreaList.Codes.CRM, true, false, false);
			EDIDataRegistry.Instance.ProductAreaIncidentCr9Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection2);

			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();

			group.ING_Priority = "CR9";
			group.ING_Module = "992";
			AssertNoErrors(group.ING_ModuleInfo);
			AssertHasWarning(group.ING_ModuleInfo, "Not linked to any product areas.");

			Factory.Save();

			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new SystemProductCollection());
			EDIDataRegistry.Instance.ProductAreaIncidentCr8Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new SystemProductCollection());
			EDIDataRegistry.Instance.ProductAreaIncidentCr9Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new SystemProductCollection());

			group.Validation.ValidateING_Module();
			AssertNoErrors(group.ING_ModuleInfo);
		}

		#endregion

		public void TestING_ServiceType()
		{
			var moduleCode = "AA1";
			var productArea = "A1A";
			var areas = new CodeDescriptionPairList();
			areas.AddPair(productArea, productArea);
			EDIDataRegistry.Instance.ProductAreas.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, areas);

			var systemProductMappingCollection = new SystemProductCollection();
			var productA = systemProductMappingCollection.AddNew(ProductTypes.Codes.Enterprise, (NoResString)ProductTypes.Codes.Enterprise, true);
			productA.ModuleMappings.AddNew(moduleCode, "AA1 Description", productArea, true);
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, systemProductMappingCollection);

			var systemProductCollection = new SystemProductCollection();
			var productAAAModules = systemProductCollection.AddNew(ProductTypes.Codes.Enterprise, (NoResString)ProductTypes.Codes.Enterprise, true);
			var moduleAA1 = productAAAModules.ServiceTypeModuleMappings.AddNew(moduleCode, (NoResString)"AA1", productArea, true);
			var serviceType1 = "AAA";
			var serviceType2 = "BBB";
			moduleAA1.ServiceTypeMappings.AddNew(serviceType1);
			moduleAA1.ServiceTypeMappings.AddNew(serviceType2);
			EDIDataRegistry.Instance.ServiceTypeMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, systemProductCollection);

			var group = Factory.New<IncidentManagementGroup>();
			group.ING_Priority = Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround;
			group.ING_Product = ProductTypes.Codes.Enterprise;
			group.ING_ProductArea = productArea;
			group.ING_Module = moduleCode;

			AssertEquals("Precondition", string.Empty, group.ING_ServiceType);
			AssertNoErrors(group.ING_ServiceTypeInfo);

			group.ING_ServiceType = "ZZZ";
			AssertHasError(group.ING_ServiceTypeInfo, "Enter a valid Service Type.");
			group.ING_ServiceType = serviceType1;
			AssertNoErrors(group.ING_ServiceTypeInfo);
			group.ING_ServiceType = "ZZZ";
			AssertHasError(group.ING_ServiceTypeInfo, "Enter a valid Service Type.");
			group.ING_ServiceType = serviceType2;
			AssertNoErrors(group.ING_ServiceTypeInfo);
		}

		public void TestNotesEntered()
		{
			var group = Factory.New<IncidentManagementGroup>();
			group.Validation.ValidateAll();
			AssertHasError(group.InitialSymptomsTextInfo, "Please enter an Initial Symptoms.");
			AssertHasError(group.BusinessImpactDescriptionTextInfo, "Please enter a Business Impact Description.");
			Assert(!group.RootCauseTextInfo.GetErrors().Any(x => x.Message == "Please enter a Root Cause."));

			group.InitialSymptomsText = "hi";
			group.BusinessImpactDescriptionText = "hello";
			group.RootCauseText = "ciao";

			group.Validation.ValidateAll();
			AssertNoError(group.InitialSymptomsTextInfo, "Please enter an Initial Symptoms.");
			AssertNoError(group.BusinessImpactDescriptionTextInfo, "Please enter a Business Impact Description.");
			Assert(!group.RootCauseTextInfo.GetErrors().Any(x => x.Message == "Please enter a Root Cause."));
		}

		public void TestAutoCascadeRelatedItems()
		{
			var workItem = Factory.NewWithValidTestData<WorkItem>();
			workItem.WKI_ActivitySubtype = NewWorkItemLookups.WorkItemTypeConstants.DefectFix;
			workItem.WKI_WorkItemNumber = "WI00PYN001";

			var incident1 = Factory.NewWithValidTestData<SupportIncident>();
			incident1.IM_IncidentNumber = "CS00PYN001";
			incident1.IM_Priority = Constants.CustomerService.CriticalityCodes.CR3_SingleFunctionNoWorkAround;

			var incident2 = Factory.NewWithValidTestData<SupportIncident>();
			incident2.IM_IncidentNumber = "CS00PYN002";
			incident2.IM_Priority = Constants.CustomerService.CriticalityCodes.CR3_SingleFunctionNoWorkAround;

			var group = Factory.New<IncidentManagementGroup>();
			group.ING_Priority = Constants.CustomerService.CriticalityCodes.CR3_SingleFunctionNoWorkAround;

			var link1 = Factory.New<IncidentManagementLink>();
			link1.INL_IM_Incident = incident1.PK;
			link1.INL_ING_Group = group.PK;

			var link2 = Factory.New<IncidentManagementLink>();
			link2.INL_IM_Incident = incident2.PK;
			link2.INL_ING_Group = group.PK;

			Factory.Save();

			group.AutoCascadeRelatedItems.Add(workItem);
			group.Validation.ValidateAll();
			AssertNull(group.RowErrors.GetFirstMessage());
			AssertNull(group.RowWarnings.GetFirstMessage());

			workItem.WKI_ActivitySubtype = "ABC";
			group.Validation.ValidateAll();
			AssertEquals(group.RowErrors.GetFirstMessage(), $"Only Defect {NewWorkItemLookups.WorkItemTypeConstants.DefectFix} type work items can be cascaded");
			AssertNull(group.RowWarnings.GetFirstMessage());

			workItem.WKI_ActivitySubtype = NewWorkItemLookups.WorkItemTypeConstants.DefectFix;
			group.ING_Priority = Constants.CustomerService.CriticalityCodes.CR6_FeatureRequest;
			group.Validation.ValidateAll();
			AssertNull(group.RowErrors.GetFirstMessage());
			AssertEquals(group.RowWarnings.GetFirstMessage(), "A work item has been set to auto-cascade, but the incident management group's criticality is not CR1-CR4");

			group.ING_Priority = Constants.CustomerService.CriticalityCodes.CR3_SingleFunctionNoWorkAround;
			incident1.IM_Priority = Constants.CustomerService.CriticalityCodes.CR6_FeatureRequest;
			incident2.IM_Priority = Constants.CustomerService.CriticalityCodes.CR6_FeatureRequest;
			group.Validation.ValidateAll();
			AssertNull(group.RowErrors.GetFirstMessage());
			AssertEquals(group.RowWarnings.Count(), 2);
			Assert(group.RowWarnings.Contains("A work item has been set to auto-cascade, but could not be cascaded to incident CS00PYN001 because the incident criticality is not CR1-CR4"));
			Assert(group.RowWarnings.Contains("A work item has been set to auto-cascade, but could not be cascaded to incident CS00PYN002 because the incident criticality is not CR1-CR4"));

			incident1.IM_Priority = Constants.CustomerService.CriticalityCodes.CR3_SingleFunctionNoWorkAround;
			incident2.IM_Priority = Constants.CustomerService.CriticalityCodes.CR3_SingleFunctionNoWorkAround;
			group.Validation.ValidateAll();
			AssertNull(group.RowErrors.GetFirstMessage());
			AssertNull(group.RowWarnings.GetFirstMessage());

			workItem.WKI_ActivitySubtype = "ABC";
			group.ING_Priority = Constants.CustomerService.CriticalityCodes.CR6_FeatureRequest;
			incident1.IM_Priority = Constants.CustomerService.CriticalityCodes.CR6_FeatureRequest;
			group.AutoCascadeRelatedItems.DeleteAll();
			group.Validation.ValidateAll();
			AssertNull(group.RowErrors.GetFirstMessage());
			AssertNull(group.RowWarnings.GetFirstMessage());
		}
	}
}
