using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Client.EDI.ReleaseBuilds.Business;
using Enterprise.Core;
using Enterprise.CustomerService.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	public class SupportIncidentValidationTest : BusinessObjectValidationTestCase
	{
		public void TestIM_GG_Team()
		{
			var group = Factory.NewWithValidTestData<GlbGroup>();
			Factory.Save();

			var incident = Factory.New<SupportIncident>();

			incident.IM_GG_Team = group.PK;
			AssertNoErrors(incident.IM_GG_TeamInfo);

			incident.IM_GG_Team = ZGuid.Invalid;
			AssertHasErrors(incident.IM_GG_TeamInfo);

			incident.IM_GG_Team = ZGuid.Empty;
			AssertNoErrors(incident.IM_GG_TeamInfo);
		}

		public void TestIM_Description()
		{
			var incident = Factory.New<SupportIncident>();

			incident.IM_Description = "";
			AssertHasErrors(incident.IM_DescriptionInfo);

			incident.IM_Description = "hello everyone";
			AssertNoErrors(incident.IM_DescriptionInfo);
		}

		public void TestFeatureRequestClientAddressPK()
		{
			var incident = Factory.New<SupportIncident>();
			incident.Validation.ValidateFeatureRequestClientAddressPK();
			AssertNoErrors("No mandatory errors because not feature request", incident.FeatureRequestClientAddressPKInfo);

			incident.FeatureRequestClientAddressPK = ZGuid.Invalid;
			incident.Validation.ValidateFeatureRequestClientAddressPK();
			AssertHasErrors("Should still error on invalid input", incident.FeatureRequestClientAddressPKInfo);

			var org = Factory.NewWithValidTestData<OrgHeader>();

			var featureRequest = Factory.New<SupportIncident>();
			featureRequest.IM_Category = SupportIncidentCategoriesList.Codes.FeatureRequest;
			featureRequest.Validation.ValidateFeatureRequestClientAddressPK();
			AssertHasErrors("Has mandatory errors because feature request", featureRequest.FeatureRequestClientAddressPKInfo);

			featureRequest.FeatureRequestClientAddressPK = org.MainAddress.PK;
			featureRequest.Validation.ValidateFeatureRequestClientAddressPK();
			AssertNoErrors("No error when valid input", featureRequest.FeatureRequestClientAddressPKInfo);

			featureRequest.FeatureRequestClientAddressPK = ZGuid.Invalid;
			featureRequest.Validation.ValidateFeatureRequestClientAddressPK();
			AssertHasErrors("Error on invalid input", featureRequest.FeatureRequestClientAddressPKInfo);
		}

		public void TestLanguage()
		{
			var incident = Factory.New<SupportIncident>();
			incident.IM_Language = "";
			AssertHasErrors(incident.IM_LanguageInfo);
			incident.IM_Language = SharedConstants.Languages.English;
			AssertNoErrors(incident.IM_LanguageInfo);
			incident.IM_Language = "ZZZ";
			AssertHasErrors(incident.IM_LanguageInfo);
		}

		public void TestFeatureRequestContactPK()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org1.Contacts.AddNew();
			contact1.OC_ContactName = ZGuid.NewZGuid().ToString();
			var contact11 = org1.Contacts.AddNew();
			contact11.OC_ContactName = ZGuid.NewZGuid().ToString();
			contact11.OC_IsActive = false;

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var contact2 = org2.Contacts.AddNew();
			contact2.OC_ContactName = ZGuid.NewZGuid().ToString();

			var incident = Factory.New<SupportIncident>();
			incident.Validation.ValidateFeatureRequestContactPK();
			AssertNoErrors("No mandatory errors because not feature request", incident.FeatureRequestContactPKInfo);
			AssertNoWarnings("No mandatory errors because not feature request", incident.FeatureRequestContactPKInfo);

			incident.FeatureRequestContactPK = ZGuid.Invalid;
			incident.Validation.ValidateFeatureRequestContactPK();
			AssertHasErrors("Should still error on invalid input", incident.FeatureRequestContactPKInfo);
			AssertNoWarnings("Invalid guid causes error", incident.FeatureRequestContactPKInfo);

			var featureRequest = Factory.New<SupportIncident>();
			featureRequest.IM_Category = SupportIncidentCategoriesList.Codes.FeatureRequest;
			featureRequest.Validation.ValidateFeatureRequestContactPK();
			AssertHasErrors("Should still error on invalid input", featureRequest.FeatureRequestContactPKInfo);
			AssertNoWarnings("Invalid guid causes error", featureRequest.FeatureRequestContactPKInfo);

			featureRequest.FeatureRequestClientPK = org1.PK;
			featureRequest.FeatureRequestContactPK = contact1.PK;
			featureRequest.Validation.ValidateFeatureRequestContactPK();
			AssertNoErrors("No errors when valid input", featureRequest.FeatureRequestContactPKInfo);
			AssertNoWarnings("No warnings when valid input", featureRequest.FeatureRequestContactPKInfo);

			featureRequest.FeatureRequestContactPK = contact2.PK;
			featureRequest.Validation.ValidateFeatureRequestContactPK();
			AssertNoWarnings("No warnings on wrong existing contact", featureRequest.FeatureRequestContactPKInfo);
			AssertHasErrors("Errors on wrong existing contact", featureRequest.FeatureRequestContactPKInfo);

			featureRequest.FeatureRequestContactPK = contact11.PK;
			featureRequest.Validation.ValidateFeatureRequestContactPK();
			AssertNoWarnings("Warning on inactive contact", featureRequest.FeatureRequestContactPKInfo);
			AssertHasErrors("Warning on inactive contact", featureRequest.FeatureRequestContactPKInfo);

			incident.FeatureRequestContactPK = ZGuid.NewZGuid();
			incident.Validation.ValidateFeatureRequestContactPK();
			AssertHasErrors("PK of non-existing contact causes error", featureRequest.FeatureRequestContactPKInfo);
			AssertNoWarnings("PK of non-existing contact causes error", featureRequest.FeatureRequestContactPKInfo);

			featureRequest.FeatureRequestContactPK = contact1.PK;
			featureRequest.Validation.ValidateFeatureRequestContactPK();
			Factory.Save();

			contact1.OC_IsActive = false;
			Factory.Save();

			featureRequest = new BusinessObjectFactory().Load<SupportIncident>(featureRequest.PK);
			featureRequest.Validation.ValidateFeatureRequestContactPK();
			AssertNoErrors("No errors when valid input", featureRequest.FeatureRequestContactPKInfo);
			AssertHasWarnings("Should show warning for saved incident with inactive contact", featureRequest.FeatureRequestContactPKInfo);
		}

		public void TestIM_Source()
		{
			var incident = Factory.New<SupportIncident>();

			incident.IM_Source = "XYZ";
			AssertHasErrors(incident.IM_SourceInfo);

			incident.IM_Source = SupportIncidentLookups.SourceListConstants.ERequestPortal;
			AssertNoErrors(incident.IM_SourceInfo);

			incident.IM_Source = "";
			AssertHasErrors(incident.IM_SourceInfo);
		}

		public void TestIM_FeatureRequestIndustryValue()
		{
			var incident = Factory.New<SupportIncident>();

			incident.IM_FeatureRequestIndustryValue = "LOW";
			AssertNoErrors(incident.IM_FeatureRequestIndustryValueInfo);

			incident.IM_FeatureRequestIndustryValue = "XYZ";
			AssertHasErrors(incident.IM_FeatureRequestIndustryValueInfo);

			incident.IM_FeatureRequestIndustryValue = "";
			AssertNoErrors(incident.IM_FeatureRequestIndustryValueInfo);
		}

		public void TestIM_ClientBugSeverity()
		{
			var incident = Factory.New<SupportIncident>();

			incident.IM_ClientBugSeverity = "LOW";
			AssertNoErrors(incident.IM_ClientBugSeverityInfo);

			incident.IM_ClientBugSeverity = "ZUB";
			AssertHasErrors(incident.IM_ClientBugSeverityInfo);

			incident.IM_ClientBugSeverity = "";
			AssertNoErrors(incident.IM_ClientBugSeverityInfo);
		}

		public void TestIM_ActualHoursWorked()
		{
			var incident = Factory.New<SupportIncident>();

			incident.IM_ActualHoursWorked = 0m;
			AssertNoErrors(incident.IM_ActualHoursWorkedInfo);

			incident.IM_ActualHoursWorked = 20m;
			AssertNoErrors(incident.IM_ActualHoursWorkedInfo);

			incident.IM_ChargableWork = true;
			incident.IM_ActualHoursWorked = 0m;
			AssertHasErrors(incident.IM_ActualHoursWorkedInfo);

			incident.IM_ActualHoursWorked = 20m;
			AssertNoErrors(incident.IM_ActualHoursWorkedInfo);
		}

		public void TestIM_Priority()
		{
			var incident = Factory.New<SupportIncident>();

			incident.IM_Priority = "XXX";
			AssertHasErrors(incident.IM_PriorityInfo);

			incident.IM_Priority = "CR1";
			AssertNoErrors(incident.IM_PriorityInfo);

			incident.IM_Priority = "";
			AssertHasErrors(incident.IM_PriorityInfo);
		}

		public void TestIM_Priority_CheckIncidentSupportDisabledCR4_SingleFunctionWithWorkAround()
		{
			EDIDataRegistry.CreateProductsAndModulesForTest();

			var collection = new SystemProductCollection();
			var product = collection.AddNew("NO4", "Disable CR4_SingleFunctionWithWorkAround", false);
			product.Enabled = false;
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			collection = new SystemProductCollection();
			product = collection.AddNew("NO4", "Disable CR4_SingleFunctionWithWorkAround", false);
			product.Enabled = true;
			EDIDataRegistry.Instance.ProductAreaIncidentCr8Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			collection = new SystemProductCollection();
			product = collection.AddNew("NO4", "Disable CR4_SingleFunctionWithWorkAround", false);
			product.Enabled = true;
			EDIDataRegistry.Instance.ProductAreaIncidentCr9Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();

			incident.IM_Product = "NO4";
			incident.IM_Priority = Constants.CustomerService.CriticalityCodes.CR1_SystemDown;
			AssertHasErrors(incident.IM_PriorityInfo);

			incident.IM_Priority = Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround;
			AssertHasErrors(incident.IM_PriorityInfo);

			incident.IM_Priority = Constants.CustomerService.CriticalityCodes.CR8_ComplianceRequirement;
			AssertNoErrors(incident.IM_PriorityInfo);
		}

		public void TestIM_Priority_CheckIncidentSupportDisabledCR8_ComplianceRequirement()
		{
			EDIDataRegistry.CreateProductsAndModulesForTest();

			var collection = new SystemProductCollection();
			var product = collection.AddNew("NO8", "Disable CR8_ComplianceRequirement", false);
			product.Enabled = true;
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			collection = new SystemProductCollection();
			product = collection.AddNew("NO8", "Disable CR8_ComplianceRequirement", false);
			product.Enabled = false;
			EDIDataRegistry.Instance.ProductAreaIncidentCr8Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			collection = new SystemProductCollection();
			product = collection.AddNew("NO8", "Disable CR8_ComplianceRequirement", false);
			product.Enabled = true;
			EDIDataRegistry.Instance.ProductAreaIncidentCr9Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();

			incident.IM_Product = "NO8";
			incident.IM_Priority = Constants.CustomerService.CriticalityCodes.CR1_SystemDown;
			AssertNoErrors(incident.IM_PriorityInfo);

			incident.IM_Priority = Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround;
			AssertNoErrors(incident.IM_PriorityInfo);

			incident.IM_Priority = Constants.CustomerService.CriticalityCodes.CR8_ComplianceRequirement;
			AssertHasErrors(incident.IM_PriorityInfo);
		}

		public void TestIM_Priority_CheckIncidentSupportDisabledCR9_CustomerServiceRequest()
		{
			EDIDataRegistry.CreateProductsAndModulesForTest();

			var collection = new SystemProductCollection();
			var product = collection.AddNew("NO9", "Disable CR9_CustomerServiceRequest", false);
			product.Enabled = true;
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			collection = new SystemProductCollection();
			product = collection.AddNew("NO9", "Disable CR9_CustomerServiceRequest", false);
			product.Enabled = true;
			EDIDataRegistry.Instance.ProductAreaIncidentCr8Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			collection = new SystemProductCollection();
			product = collection.AddNew("NO9", "Disable CR9_CustomerServiceRequest", false);
			product.Enabled = false;
			EDIDataRegistry.Instance.ProductAreaIncidentCr9Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();

			incident.IM_Product = "NO9";
			incident.IM_Priority = Constants.CustomerService.CriticalityCodes.CR1_SystemDown;
			AssertNoErrors(incident.IM_PriorityInfo);

			incident.IM_Priority = Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround;
			AssertNoErrors(incident.IM_PriorityInfo);

			incident.IM_Priority = Constants.CustomerService.CriticalityCodes.CR9_CustomerServiceRequest;
			AssertHasErrors(incident.IM_PriorityInfo);
		}

		public void TestIM_Priority_CheckIncidentSupportsCr8Cr9()
		{
			var org = Factory.NewWithValidTestData<EDIOrgHeader>();

			var enterprise = Factory.New<LicenceEnterprise>();
			enterprise.LE_EnterpriseCode = "DDD";
			enterprise.LE_OH = org.PK;

			var releaseBuild = Factory.New<ReleaseBuild>();

			var database = Factory.New<LicenceDatabase>();
			database.LD_LE = enterprise.PK;
			database.LD_HL_CurrentRunningVersion = releaseBuild.PK;

			var clientCompany = Factory.New<ClientCompany>();
			clientCompany.LCC_Code = "AAA";
			clientCompany.LCC_LD = database.PK;

			Factory.Save();

			var incident = Factory.New<SupportIncident>();
			incident.IM_LD = database.PK;
			incident.IM_LCC = clientCompany.PK;
			Factory.Save();

			EDIDataRegistry.Instance.Cr8Cr9ReleaseBuilds.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "1.1.2.2");

			incident.IM_Product = ProductTypes.Codes.Enterprise;
			releaseBuild.VersionNumber = new VersionNumber("1.1.2.0");
			AssertEquals("Precondition", false, incident.ClientSupportsCr8Cr9);
			CombineAssertions("Should have error when criticality is cr8/cr9 if client doesn't support", () =>
			{
				incident.IM_Category = SupportIncidentCategoriesList.Codes.ComplianceRequirement;
				incident.IM_Priority = Constants.CustomerService.CriticalityCodes.CR8_ComplianceRequirement;
				AssertNoErrors(incident.IM_PriorityInfo);
				AssertHasWarning(incident.IM_PriorityInfo, "The version that the client is currently on does not support CR8/CR9 criticalities. It will appear as CR5 for the client.");

				incident.IM_Category = SupportIncidentCategoriesList.Codes.CustomerServiceRequest;
				incident.IM_Priority = Constants.CustomerService.CriticalityCodes.CR9_CustomerServiceRequest;
				AssertNoErrors(incident.IM_PriorityInfo);
				AssertHasWarning(incident.IM_PriorityInfo, "The version that the client is currently on does not support CR8/CR9 criticalities. It will appear as CR5 for the client.");
			});

			releaseBuild.VersionNumber = new VersionNumber("1.1.2.2");
			AssertEquals("Precondition", true, incident.ClientSupportsCr8Cr9);
			CombineAssertions("No error nor warning when criticality is cr8/cr9 if client supports", () =>
			{
				incident.IM_Category = SupportIncidentCategoriesList.Codes.ComplianceRequirement;
				incident.IM_Priority = Constants.CustomerService.CriticalityCodes.CR8_ComplianceRequirement;
				AssertNoErrors(incident.IM_PriorityInfo);
				AssertNoWarnings(incident.IM_PriorityInfo);

				incident.IM_Category = SupportIncidentCategoriesList.Codes.CustomerServiceRequest;
				incident.IM_Priority = Constants.CustomerService.CriticalityCodes.CR9_CustomerServiceRequest;
				AssertNoErrors(incident.IM_PriorityInfo);
				AssertNoWarnings(incident.IM_PriorityInfo);
			});
		}

		public void TestIM_Priority_DefectOnlyList()
		{
			SupportIncident incident = Factory.New<SupportIncident>();
			Factory.Save();
			incident.Escalate(SupportIncidentCategoriesList.Codes.Defect, "");
			const string messageError =
@"A Defect can only be in the following criticalities:
 - CR1 (Entire system is down – system failure)
 - CR2 (Entire module not working with no manual work around)
 - CR3 (Single function not working with no manual work around)
 - CR4 (Single function not working with manual work around)";

			AssertCriticalityIsValidOnStage(incident, new ZString[]
				{
					Constants.CustomerService.CriticalityCodes.CR1_SystemDown,
					Constants.CustomerService.CriticalityCodes.CR2_ModuleDown,
					Constants.CustomerService.CriticalityCodes.CR3_SingleFunctionNoWorkAround,
					Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround
				});

			AssertCriticalityIsNotValidOnStage(incident, messageError, new ZString[]
				{
					Constants.CustomerService.CriticalityCodes.CR5_Training,
					Constants.CustomerService.CriticalityCodes.CR6_FeatureRequest,
					Constants.CustomerService.CriticalityCodes.CR7_CustomisationRequest,
					Constants.CustomerService.CriticalityCodes.CR8_ComplianceRequirement,
					Constants.CustomerService.CriticalityCodes.CR9_CustomerServiceRequest
				});
		}

		public void TestIM_Priority_FeatureOnlyList()
		{
			SupportIncident incident = Factory.New<SupportIncident>();
			Factory.Save();
			incident.Escalate(SupportIncidentCategoriesList.Codes.FeatureRequest, "");
			const string messageError =
@"A Feature Request can only be in the following criticalities:
 - CR6 (Feature Request)
 - CR7 (Estimate / Quote Request)";

			AssertCriticalityIsValidOnStage(incident, new ZString[]
				{
					Constants.CustomerService.CriticalityCodes.CR6_FeatureRequest,
					Constants.CustomerService.CriticalityCodes.CR7_CustomisationRequest
				});

			AssertCriticalityIsNotValidOnStage(incident, messageError, new ZString[]
				{
					Constants.CustomerService.CriticalityCodes.CR1_SystemDown,
					Constants.CustomerService.CriticalityCodes.CR2_ModuleDown,
					Constants.CustomerService.CriticalityCodes.CR3_SingleFunctionNoWorkAround,
					Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround,
					Constants.CustomerService.CriticalityCodes.CR5_Training,
					Constants.CustomerService.CriticalityCodes.CR8_ComplianceRequirement,
					Constants.CustomerService.CriticalityCodes.CR9_CustomerServiceRequest
				});
		}

		public void TestIM_Priority_ContentDevelopmentOnlyList()
		{
			SupportIncident incident = Factory.New<SupportIncident>();
			Factory.Save();
			incident.Escalate(SupportIncidentCategoriesList.Codes.ContentDevelopment, "");
			const string messageError =
@"A Content Development can only be in the following criticalities:
 - CR5 (Training Questions)";

			AssertCriticalityIsValidOnStage(incident, new ZString[]
				{
					Constants.CustomerService.CriticalityCodes.CR5_Training
				});

			AssertCriticalityIsNotValidOnStage(incident, messageError, new ZString[]
				{
					Constants.CustomerService.CriticalityCodes.CR1_SystemDown,
					Constants.CustomerService.CriticalityCodes.CR2_ModuleDown,
					Constants.CustomerService.CriticalityCodes.CR3_SingleFunctionNoWorkAround,
					Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround,
					Constants.CustomerService.CriticalityCodes.CR6_FeatureRequest,
					Constants.CustomerService.CriticalityCodes.CR7_CustomisationRequest,
					Constants.CustomerService.CriticalityCodes.CR8_ComplianceRequirement,
					Constants.CustomerService.CriticalityCodes.CR9_CustomerServiceRequest
				});
		}

		public void TestIM_Priority_ComplianceRequirementOnlyList()
		{
			SupportIncident incident = Factory.New<SupportIncident>();
			Factory.Save();
			incident.Escalate(SupportIncidentCategoriesList.Codes.ComplianceRequirement, "");
			const string messageError =
@"A Compliance Requirement can only be in the following criticalities:
 - CR8 (Compliance, Reference and Master Data)";

			AssertCriticalityIsValidOnStage(incident, new ZString[]
				{
					Constants.CustomerService.CriticalityCodes.CR8_ComplianceRequirement
				});

			AssertCriticalityIsNotValidOnStage(incident, messageError, new ZString[]
				{
					Constants.CustomerService.CriticalityCodes.CR1_SystemDown,
					Constants.CustomerService.CriticalityCodes.CR2_ModuleDown,
					Constants.CustomerService.CriticalityCodes.CR3_SingleFunctionNoWorkAround,
					Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround,
					Constants.CustomerService.CriticalityCodes.CR5_Training,
					Constants.CustomerService.CriticalityCodes.CR6_FeatureRequest,
					Constants.CustomerService.CriticalityCodes.CR7_CustomisationRequest,
					Constants.CustomerService.CriticalityCodes.CR9_CustomerServiceRequest
				});
		}

		public void TestIM_Priority_CustomerServiceRequestOnlyList()
		{
			SupportIncident incident = Factory.New<SupportIncident>();
			Factory.Save();
			incident.Escalate(SupportIncidentCategoriesList.Codes.CustomerServiceRequest, "");
			const string messageError =
@"A Service Request can only be in the following criticalities:
 - CR9 (Service Request)";

			AssertCriticalityIsValidOnStage(incident, new ZString[]
				{
					Constants.CustomerService.CriticalityCodes.CR9_CustomerServiceRequest
				});

			AssertCriticalityIsNotValidOnStage(incident, messageError, new ZString[]
				{
					Constants.CustomerService.CriticalityCodes.CR1_SystemDown,
					Constants.CustomerService.CriticalityCodes.CR2_ModuleDown,
					Constants.CustomerService.CriticalityCodes.CR3_SingleFunctionNoWorkAround,
					Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround,
					Constants.CustomerService.CriticalityCodes.CR5_Training,
					Constants.CustomerService.CriticalityCodes.CR6_FeatureRequest,
					Constants.CustomerService.CriticalityCodes.CR7_CustomisationRequest,
					Constants.CustomerService.CriticalityCodes.CR8_ComplianceRequirement
				});
		}

		void AssertCriticalityIsNotValidOnStage(SupportIncident incident, string errorMessage, ZString[] criticalities)
		{
			foreach (var criticality in criticalities)
			{
				incident.IM_Priority = criticality;
				AssertHasError(incident.IM_PriorityInfo, errorMessage);
			}
		}

		void AssertCriticalityIsValidOnStage(SupportIncident incident, ZString[] criticalities)
		{
			foreach (var criticality in criticalities)
			{
				incident.IM_Priority = criticality;
				AssertNoErrors(incident.IM_PriorityInfo);
			}
		}

		public void TestIM_Priority_ValidateInactiveStageMapping()
		{
			var incident = Factory.New<SupportIncident>();
			incident.IM_Priority = Constants.CustomerService.CriticalityCodes.CR6_FeatureRequest;
			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.TrainingReferredToLearningMaterials, "");
			incident.IM_Category = SupportIncidentCategoriesList.Codes.Support;
			AssertHasErrors(incident.IM_PriorityInfo);

			Factory.Save();

			var loadedIncident = new BusinessObjectFactory().Load<SupportIncident>(incident.PK);
			AssertNoErrors("No error if no change made to saved incident criticality", loadedIncident.IM_PriorityInfo);

			loadedIncident.IM_Priority = Constants.CustomerService.CriticalityCodes.CR7_CustomisationRequest;
			AssertHasErrors("If change criticality there should be error", loadedIncident.IM_PriorityInfo);
		}

		public void TestIM_Product()
		{
			var incident = Factory.New<SupportIncident>();

			incident.IM_Product = "XXX";
			AssertHasErrors(incident.IM_ProductInfo);

			incident.IM_Product = "ENT";
			AssertNoErrors(incident.IM_ProductInfo);

			incident.IM_Product = "";
			AssertHasErrors(incident.IM_ProductInfo);
		}

		public void TestIM_Module()
		{
			TestIM_ModuleBaseBehaviour();
			TestIM_ModuleDisablingModulesNoImpactOnExistingModules();
		}

		void TestIM_ModuleBaseBehaviour()
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
			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();

			incident.IM_Product = "ENT";
			incident.IM_Module = "XXX";
			AssertListValidationInvalidCodeError(incident.IM_ModuleInfo, true);

			incident.IM_Priority = "CR4";
			incident.IM_Module = "CAT";
			AssertEquals("Precondition", moduleMapping1.IsEnabled, true);
			AssertNoErrors(incident.IM_ModuleInfo);

			incident.IM_Priority = "CR8";
			incident.IM_Module = "888";
			AssertEquals("Precondition", moduleMapping4.IsEnabled, true);
			AssertNoErrors(incident.IM_ModuleInfo);

			incident.IM_Priority = "CR9";
			incident.IM_Module = "999";
			AssertEquals("Precondition", moduleMapping7.IsEnabled, true);
			AssertNoErrors(incident.IM_ModuleInfo);

			moduleMapping1.IsEnabled = true;
			EDIDataRegistry.Instance.ProductAreaIncidentCr8Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection1);
			AssertEquals("Precondition", moduleMapping1.IsEnabled, true);
			incident.IM_Priority = "CR8";
			incident.IM_Module = "CAT";
			AssertListValidationInvalidCodeError(incident.IM_ModuleInfo, true);

			moduleMapping4.IsEnabled = true;
			EDIDataRegistry.Instance.ProductAreaIncidentCr8Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection1);
			AssertEquals("Precondition", moduleMapping4.IsEnabled, true);
			incident.IM_Priority = "CR9";
			incident.IM_Module = "888";
			AssertListValidationInvalidCodeError(incident.IM_ModuleInfo, true);

			moduleMapping5.IsEnabled = true;
			EDIDataRegistry.Instance.ProductAreaIncidentCr9Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection2);
			AssertEquals("Precondition", moduleMapping5.IsEnabled, true);
			incident.IM_Priority = "CR4";
			incident.IM_Module = "999";
			AssertListValidationInvalidCodeError(incident.IM_ModuleInfo, true);

			AssertEquals("Precondition", moduleMapping3.IsEnabled, false);
			incident.IM_Priority = "CR4";
			incident.IM_Module = "CAA";
			AssertListValidationInvalidCodeError(incident.IM_ModuleInfo, true);

			AssertEquals("Precondition", moduleMapping6.IsEnabled, false);
			incident.IM_Priority = "CR8";
			incident.IM_Module = "884";
			AssertListValidationInvalidCodeError(incident.IM_ModuleInfo, true);

			AssertEquals("Precondition", moduleMapping9.IsEnabled, false);
			incident.IM_Priority = "CR9";
			incident.IM_Module = "993";
			AssertListValidationInvalidCodeError(incident.IM_ModuleInfo, true);

			incident.IM_Priority = "CR4";
			incident.IM_Module = "CA2";
			AssertNoErrors(incident.IM_ModuleInfo);
			AssertHasWarning(incident.IM_ModuleInfo, "Not linked to any product areas.");

			incident.IM_Priority = "CR8";
			incident.IM_Module = "882";
			AssertNoErrors(incident.IM_ModuleInfo);
			AssertHasWarning(incident.IM_ModuleInfo, "Not linked to any product areas.");

			incident.IM_Priority = "CR9";
			incident.IM_Module = "992";
			AssertNoErrors(incident.IM_ModuleInfo);
			AssertHasWarning(incident.IM_ModuleInfo, "Not linked to any product areas.");

			Factory.Save();

			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new SystemProductCollection());
			EDIDataRegistry.Instance.ProductAreaIncidentCr8Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new SystemProductCollection());
			EDIDataRegistry.Instance.ProductAreaIncidentCr9Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new SystemProductCollection());

			incident.Validation.ValidateIM_Module();
			AssertNoErrors(incident.IM_ModuleInfo);

			incident.ProductArea = "XXX";
			AssertEquals("Precondition", true, incident.ProductAreaInfo.HasChanges);
			incident.Validation.ValidateIM_Module();
			AssertListValidationInvalidCodeError(incident.IM_ModuleInfo, true);

			incident.ProductArea = "";
			AssertEquals("Precondition", false, incident.ProductAreaInfo.HasChanges);
			incident.Validation.ValidateIM_Module();
			AssertNoErrors(incident.IM_ModuleInfo);

			incident.IM_Module = "882";
			AssertEquals("Precondition", true, incident.IM_ModuleInfo.HasChanges);
			AssertListValidationInvalidCodeError(incident.IM_ModuleInfo, true);
		}

		void TestIM_ModuleDisablingModulesNoImpactOnExistingModules()
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
			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_Product = "ENT";

			incident.IM_Priority = "CR4";
			incident.IM_Module = "CAT";
			AssertEquals("Precondition", moduleMapping1.IsEnabled, true);
			AssertNoErrors(incident.IM_ModuleInfo);
			moduleMapping1.IsEnabled = false;
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			Factory.Save();
			incident.Validation.ValidateIM_Module();
			AssertEquals("Precondition", moduleMapping1.IsEnabled, false);
			AssertNoErrors(incident.IM_ModuleInfo);

			incident.IM_Priority = "CR8";
			incident.IM_Module = "888";
			AssertEquals("Precondition", moduleMapping4.IsEnabled, true);
			AssertNoErrors(incident.IM_ModuleInfo);
			moduleMapping4.IsEnabled = false;
			EDIDataRegistry.Instance.ProductAreaIncidentCr8Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection1);
			Factory.Save();
			incident.Validation.ValidateIM_Module();
			AssertEquals("Precondition", moduleMapping4.IsEnabled, false);
			AssertNoErrors(incident.IM_ModuleInfo);

			incident.IM_Priority = "CR9";
			incident.IM_Module = "999";
			AssertEquals("Precondition", moduleMapping7.IsEnabled, true);
			AssertNoErrors(incident.IM_ModuleInfo);
			moduleMapping7.IsEnabled = false;
			EDIDataRegistry.Instance.ProductAreaIncidentCr9Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection2);
			Factory.Save();
			incident.Validation.ValidateIM_Module();
			AssertEquals("Precondition", moduleMapping7.IsEnabled, false);
			AssertNoErrors(incident.IM_ModuleInfo);
		}

		public void TestIM_ModuleClearRegistryModules()
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

			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();

			incident.IM_Priority = "CR9";
			incident.IM_Module = "992";
			AssertNoErrors(incident.IM_ModuleInfo);
			AssertHasWarning(incident.IM_ModuleInfo, "Not linked to any product areas.");

			Factory.Save();

			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new SystemProductCollection());
			EDIDataRegistry.Instance.ProductAreaIncidentCr8Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new SystemProductCollection());
			EDIDataRegistry.Instance.ProductAreaIncidentCr9Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new SystemProductCollection());

			incident.Validation.ValidateIM_Module();
			AssertNoErrors(incident.IM_ModuleInfo);
		}

		public void TestIM_ProgramArea_ShouldWarnIfProductAreaMatchesTriageNodeButNotMenuItemAndViceVersa()
		{
			var productAreasList = new CodeDescriptionPairList();
			productAreasList.AddPair("AAA", "Apple");
			productAreasList.AddPair("BBB", "Banana");
			productAreasList.AddPair("CCC", "Carrot");
			EDIDataRegistry.Instance.ProductAreas.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, productAreasList);

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
			incident.IM_Priority = "CR4";
			incident.IM_Module = "RED";
			incident.IM_SourceModuleId = "SourceModule1";
			AssertEquals("Precondition: Incident's product area should be recalculated to match the menu item (source module id's) product area mapping", "BBB", incident.IM_ProgramArea);

			Factory.Save();
			AssertEquals("Precondition: Should be set from menu item SourceModule1's mapping in the registry", "BBB", incident.IM_ProgramArea);
			AssertNoErrors(incident.IM_ProgramAreaInfo);

			incident.IM_IMT_Triage = triage.PK;
			incident.IM_ProgramArea = triage.IMT_ProductArea;

			AssertEquals("Precondition", triage.IMT_ProductArea, incident.IM_ProgramArea);
			AssertHasWarning("Should have warning since product area set from triage doesn't match the menu item.", incident.IM_ProgramAreaInfo, "The Product Area matches the attached Triage Node, but conflicts with the expected Product Area BBB for the selected Menu Item.");

			incident.IM_ProgramArea = "BBB";
			AssertHasWarning("Should have warning since product area set from the menu item doesn't match the triage.", incident.IM_ProgramAreaInfo, "The Product Area matches the attached selected Menu Item, but conflicts with the expected Product Area CCC for the Triage Node.");

			incident.IM_SourceModuleId = "invalid";
			AssertNoExceptionThrown(() => incident.Validation.ValidateIM_ProgramArea());
			AssertNoWarnings(incident.IM_ProgramAreaInfo);

			incident.IM_Module = "INV";
			AssertNoExceptionThrown(() => incident.Validation.ValidateIM_ProgramArea());
			AssertNoWarnings(incident.IM_ProgramAreaInfo);
		}

		public void TestValidateIM_Module_MatchingTriage_ShouldAllowIfProductProductAreaAndCriticalityAlsoMatch()
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
			incident.IM_Priority = Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround;
			incident.IM_Module = "RED";
			Factory.Save();

			incident.IM_ProgramArea = triage.IMT_ProductArea;
			incident.Validation.ValidateIM_Module();
			AssertHasError(incident.IM_ModuleInfo, "Enter a valid Module Code.");

			incident.IM_IMT_Triage = triage.PK;

			AssertEquals("Precondition", triage.IMT_ProductArea, incident.IM_ProgramArea);
			incident.Validation.ValidateIM_Module();
			AssertNoErrors("Module from triage should be valid since product area matches triage", incident.IM_ModuleInfo);

			incident.IM_Priority = Constants.CustomerService.CriticalityCodes.CR1_SystemDown;
			incident.Validation.ValidateIM_Module();
			AssertNoErrors("Module from triage should be valid since product area matches triage", incident.IM_ModuleInfo);

			incident.IM_ProgramArea = "ZZZ";
			incident.Validation.ValidateIM_Module();
			AssertHasError("Module from triage should not be valid since product area does not match triage", incident.IM_ModuleInfo, "Enter a valid Module Code.");

			incident.IM_ProgramArea = triage.IMT_ProductArea;
			incident.IM_Product = "ZZZ";
			incident.Validation.ValidateIM_Module();
			AssertHasError("Module from triage should not be valid since product does not match triage", incident.IM_ModuleInfo, "Enter a valid Module Code.");

			incident.IM_Product = "ENT";
			incident.IM_Priority = Constants.CustomerService.CriticalityCodes.CR8_ComplianceRequirement;
			incident.IM_ProgramArea = triage.IMT_ProductArea;
			AssertEquals("Precondition", triage.IMT_ProductArea, incident.IM_ProgramArea);
			incident.Validation.ValidateIM_Module();
			AssertHasError("Module from triage should not be valid since criticality does not match triage", incident.IM_ModuleInfo, "Enter a valid Module Code.");

			incident.IM_Priority = Constants.CustomerService.CriticalityCodes.CR9_CustomerServiceRequest;
			incident.IM_ProgramArea = triage.IMT_ProductArea;
			AssertEquals("Precondition", triage.IMT_ProductArea, incident.IM_ProgramArea);
			incident.Validation.ValidateIM_Module();
			AssertHasError("Module from triage should not be valid since criticality does not match triage", incident.IM_ModuleInfo, "Enter a valid Module Code.");

			triage.IMT_Type = IncidentTriageTypes.Codes.Service;
			AssertEquals("Precondition", triage.IMT_ProductArea, incident.IM_ProgramArea);
			incident.Validation.ValidateIM_Module();
			AssertNoErrors("Module from triage should be valid since criticality matches triage", incident.IM_ModuleInfo);

			triage.IMT_Type = IncidentTriageTypes.Codes.Compliance;
			incident.IM_Priority = Constants.CustomerService.CriticalityCodes.CR8_ComplianceRequirement;
			incident.IM_ProgramArea = triage.IMT_ProductArea;
			AssertEquals("Precondition", triage.IMT_ProductArea, incident.IM_ProgramArea);
			incident.Validation.ValidateIM_Module();
			AssertNoErrors("Module from triage should be valid since criticality matches triage", incident.IM_ModuleInfo);

			Factory.Save();
			var incidentReloaded = new BusinessObjectFactory().Load<SupportIncident>(incident.PK);
			incidentReloaded.Validation.ValidateIM_Module();
			AssertNoErrors("Module from triage should be valid even without changes since criticality matches triage", incidentReloaded.IM_ModuleInfo);
		}

		public void TestIM_ResolutionCode()
		{
			var incident = Factory.New<SupportIncident>();

			incident.IM_Status = SupportIncidentLookups.Status.Open;
			incident.IM_ResolutionCode = "XXX";
			AssertNoErrors(incident.IM_ResolutionCodeInfo);

			incident.IM_ResolutionCode = "ADD";
			AssertNoErrors(incident.IM_ResolutionCodeInfo);

			incident.IM_ResolutionCode = "";
			AssertHasErrors(incident.IM_ResolutionCodeInfo);
		}

		public void TestIM_Status()
		{
			var incident = Factory.New<SupportIncident>();

			incident.IM_Status = "XXX";
			AssertHasErrors(incident.IM_StatusInfo);

			incident.IM_Status = SupportIncidentLookups.Status.Open;
			AssertNoErrors(incident.IM_StatusInfo);

			incident.IM_Status = "";
			AssertHasErrors(incident.IM_StatusInfo);
		}

		public void TestDatabaseServerCode()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			var org4NoDb = BillingTestHelper.CreateLicenceCompany(Factory, "EN4", "CO4").Header;

			var enterprise1 = Factory.New<LicenceEnterprise>();
			enterprise1.LE_EnterpriseCode = "ENT";
			enterprise1.LE_OH = org1.PK;

			var enterprise2 = Factory.New<LicenceEnterprise>();
			enterprise2.LE_EnterpriseCode = "EDI";
			enterprise2.LE_OH = org2.PK;

			var enterprise3 = Factory.New<LicenceEnterprise>();
			enterprise3.LE_EnterpriseCode = "HYE";
			enterprise3.LE_OH = org3.PK;

			var database1 = Factory.New<LicenceDatabase>();
			database1.LD_ServerCode = "SRV";
			database1.LD_LE = enterprise1.PK;
			database1.LD_LicenceType = DatabaseTypes.Codes.Production;

			var database2 = Factory.New<LicenceDatabase>();
			database2.LD_ServerCode = "SYD";
			database2.LD_LE = enterprise2.PK;
			database2.LD_LicenceType = DatabaseTypes.Codes.Test;

			var company2 = Factory.New<LicenceCompany>();
			company2.LC_CompanyCode = "CM2";
			company2.LC_LE = enterprise2.PK;
			company2.LC_OH = org2.PK;

			var header2 = Factory.New<LicenceHeader>();
			header2.LA_LD = database2.PK;
			header2.LA_LC = company2.PK;

			var database3 = Factory.New<LicenceDatabase>();
			database3.LD_ServerCode = "BRN";
			database3.LD_LE = enterprise3.PK;

			Factory.Save();

			var internalLicences = new InternalIncidentLicenceSettings();
			internalLicences.LicenceEnterpriseKeys.Add(new LicenceEnterpriseKey() { LE_PK = enterprise2.PK });
			internalLicences.LicenceEnterpriseKeys.Add(new LicenceEnterpriseKey() { LE_PK = enterprise3.PK });
			internalLicences.EdiProd_LicencePK = header2.PK;
			internalLicences.UAT_ALP_LicencePK = header2.PK;
			internalLicences.UAT_DPR_LicencePK = header2.PK;
			internalLicences.UAT_GPC_LicencePK = header2.PK;
			internalLicences.UAT_GPR_LicencePK = header2.PK;
			internalLicences.UAT_STD_LicencePK = header2.PK;
			EDIDataRegistry.Instance.InternalIncidentLicenceSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, internalLicences);

			var incident1 = Factory.New<SupportIncident>();
			incident1.EnterprisePK = enterprise1.PK;
			incident1.IM_Product = ProductTypes.Codes.Enterprise;

			incident1.Validation.ValidateDatabaseServerCode();
			AssertHasErrors(incident1.DatabaseServerCodeInfo);

			incident1.IM_Product = "AAA";
			incident1.Validation.ValidateDatabaseServerCode();
			AssertNoErrors("not mandatory for non-enterprise product", incident1.DatabaseServerCodeInfo);

			incident1.DatabaseServerCode = database1.LD_ServerCode;
			database1.LD_LicenceType = DatabaseTypes.Codes.Test;
			incident1.Validation.ValidateDatabaseServerCode();
			AssertNoErrors("No error for non-production database", incident1.DatabaseServerCodeInfo);

			var incidentSavedWithNoDb = CreateIncident(org4NoDb.Contacts[0], "ENT", "CR4", "ARC", "OTH");

			Factory.Save();
			incident1.Validation.ValidateDatabaseServerCode();
			AssertNoErrors("No error for incident already saved", incident1.DatabaseServerCodeInfo);

			incidentSavedWithNoDb.Validation.ValidateDatabaseServerCode();
			AssertNoErrors("PRE: No error for saved incident", incidentSavedWithNoDb.DatabaseServerCodeInfo);

			incidentSavedWithNoDb.IM_OH_Client = org1.PK;
			incidentSavedWithNoDb.Validation.ValidateDatabaseServerCode();
			AssertHasErrors("changing the client re-validates the server", incidentSavedWithNoDb.DatabaseServerCodeInfo);
			AssertHasError("changing the client re-validates the server", incidentSavedWithNoDb.DatabaseServerCodeInfo, "Database is mandatory when the Product is ediEnterprise / CargoWiseOne / CargoWiseNext or GLOW.");
		}

		public void TestDatabaseServerCode_ReadonlyAndInactive()
		{
			var incident = new SupportIncidentTestHelper(Factory).CreateIncidentWithBidirectionalUpdateClient();
			var db = incident.Database;
			Factory.Save();

			var reloadFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var reloadDb = reloadFactory.Load<LicenceDatabase>(db.PK);
			reloadDb.LD_IsActive = false;
			reloadFactory.Save();

			var reloadIncident = reloadFactory.Load<SupportIncident>(incident.PK);
			AssertEquals(reloadIncident.IM_LD, db.PK);
			reloadIncident.Validation.ValidateDatabaseServerCode();
			AssertNoErrors(reloadIncident.DatabaseServerCodeInfo);
		}

		public void TestDatabaseServerCode_AllowBlankWhenNoActiveLicence()
		{
			var licCW1 = BillingTestHelper.CreateLicence(Factory, "EN1", "CO1", "CW1", false);
			licCW1.Database.LD_Product = ProductTypes.Codes.CargoWiseOne;
			licCW1.Database.LD_IsActive = false;
			var licENT = BillingTestHelper.CreateLicence(Factory, "EN2", "CO2", "ENT", false);
			licENT.Database.LD_Product = ProductTypes.Codes.Enterprise;
			licENT.Database.LD_IsActive = false;
			var licBW = BillingTestHelper.CreateBorderWiseLicence(Factory, "EN3", "CO3", "BW1");
			licBW.Database.LD_IsActive = false;
			var orgWithLicenceCompanyOnly = BillingTestHelper.CreateLicencedOrganization(Factory, "EN4", "CO4").Header;
			var orgNoLic = Factory.NewWithValidTestData<OrgHeader>();
			var contactNoLic = orgNoLic.Contacts.AddNew();
			contactNoLic.OC_ContactName = "Some User";
			contactNoLic.OC_Email = "some.user@test.org";
			Factory.Save();

			var collection = new SystemProductCollection();
			var enterprise = collection.AddNew(ProductTypes.Codes.Enterprise, ProductTypes.Descriptions.EnterpriseCW1, true);
			enterprise.ModuleMappings.AddNew(MandatoryCustomerServiceMenuSectionList.Codes.Other, MandatoryCustomerServiceMenuSectionList.Descriptions.Other, ProductAreaList.Codes.ARC, true);
			var product1 = collection.AddNew(ProductTypes.Codes.BorderWise, "BorderWise", true);
			product1.ModuleMappings.AddNew("BW1", "BW1 description", ProductAreaList.Codes.ARC, true);
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			var crit = Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround;
			var area = ProductAreaList.Codes.ARC;
			var module = MandatoryCustomerServiceMenuSectionList.Codes.Other;

			AssertValidIncident("entNoLic", contactNoLic, ProductTypes.Codes.Enterprise, crit, area, module);
			AssertValidIncident("bwNoLic", contactNoLic, ProductTypes.Codes.BorderWise, crit, area, "BW1");
			AssertValidIncident("entWithLicenceCompanyOnly", orgWithLicenceCompanyOnly.Contacts[0], ProductTypes.Codes.Enterprise, crit, area, module);
			AssertValidIncident("bwWithLicenceCompanyOnly", orgWithLicenceCompanyOnly.Contacts[0], ProductTypes.Codes.BorderWise, crit, area, "BW1");
			AssertValidIncident("entWithNoActiveDb", licENT.Company.Header.Contacts[0], ProductTypes.Codes.Enterprise, crit, area, module);
			AssertValidIncident("cwWithNoActiveDb", licCW1.Company.Header.Contacts[0], ProductTypes.Codes.Enterprise, crit, area, module);
			AssertValidIncident("bwWithNoActiveDb", licBW.Company.Header.Contacts[0], ProductTypes.Codes.BorderWise, crit, area, "BW1");
		}

		public void TestDatabaseServerCode_NoErrorWhenNoChange()
		{
			var collection = new SystemProductCollection();
			var enterprise = collection.AddNew(ProductTypes.Codes.Enterprise, ProductTypes.Descriptions.EnterpriseCW1, true);
			enterprise.ModuleMappings.AddNew(MandatoryCustomerServiceMenuSectionList.Codes.Other, MandatoryCustomerServiceMenuSectionList.Descriptions.Other, ProductAreaList.Codes.ARC, true);
			var product1 = collection.AddNew(ProductTypes.Codes.BorderWise, "BorderWise", true);
			product1.ModuleMappings.AddNew("BW1", "BW1 description", ProductAreaList.Codes.ARC, true);
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			var lic1 = BillingTestHelper.CreateLicence(Factory, "EN1", "CO1", "CW1", true);
			var lic2 = BillingTestHelper.CreateAnotherDatabase(lic1, "CW2", true);

			Factory.Save();

			var crit = Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround;
			var area = ProductAreaList.Codes.ARC;
			var module = MandatoryCustomerServiceMenuSectionList.Codes.Other;
			var incident = CreateIncident(lic1.Company.Header.Contacts[0], ProductTypes.Codes.Enterprise, crit, area, module);
			incident.SetDatabaseOnly(lic1.Database);
			incident.SetClientCompanyOnly(lic1.ClientCompany);
			incident.Validation.ValidateAll();
			AssertNoNotifications(incident);
			Factory.Save();
			lic1.Database.LD_IsActive = false;
			lic2.Database.LD_IsActive = false;
			Factory.Save();

			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };
			var savedIncident = factory2.Load<SupportIncident>(incident.PK);
			var unsavedIncident = CreateIncident(lic1.Company.Header.Contacts[0], ProductTypes.Codes.Enterprise, crit, area, module);
			unsavedIncident.DatabaseServerCode = lic1.Database.LD_ServerCode;

			savedIncident.Validation.ValidateAll();
			AssertNoErrors("no errors - saved incident with inactive DB, but no changes", savedIncident);
			AssertHasErrors("has error - unsaved incident with inactive DB", unsavedIncident.DatabaseServerCodeInfo);

			savedIncident.DatabaseServerCode = lic2.Database.LD_ServerCode;
			AssertHasErrors("has error - saved incident with inactive DB and changes", savedIncident.DatabaseServerCodeInfo);
		}

		SupportIncident CreateIncident(OrgContact contact, string product, string priority, string productArea, string module)
		{
			var incident = Factory.New<SupportIncident>();
			incident.IM_OH_Client = contact.OC_OH;
			incident.IM_OC_Contact = contact.PK;
			incident.IM_Source = SupportIncidentLookups.SourceListConstants.ERequestPortal;
			incident.IM_Product = product;
			incident.IM_Priority = priority;
			incident.ProductArea = productArea;
			incident.IM_Module = module;
			incident.IM_Description = "1 2 3 4 5";
			incident.DetailNoteText = "6 7 8 9 10";
			return incident;
		}

		void AssertValidIncident(string msg, OrgContact contact, string product, string priority, string productArea, string module)
		{
			var incident = CreateIncident(contact, product, priority, productArea, module);
			incident.Validation.ValidateAll();
			AssertNoNotifications(msg, incident);
		}

		public void TestClientCompanyCode()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();

			var enterprise1 = Factory.New<LicenceEnterprise>();
			enterprise1.LE_EnterpriseCode = "ENT";
			enterprise1.LE_OH = org1.PK;

			var database1 = Factory.New<LicenceDatabase>();
			database1.LD_ServerCode = "SRV";
			database1.LD_LE = enterprise1.PK;

			var clientCompany1a = Factory.New<ClientCompany>();
			clientCompany1a.LCC_LD = database1.PK;
			clientCompany1a.LCC_Code = "MEO";
			clientCompany1a.LCC_OH = org1.PK;

			var clientCompany1b = Factory.New<ClientCompany>();
			clientCompany1b.LCC_LD = database1.PK;
			clientCompany1b.LCC_Code = "MOO";

			var clientCompany1c = Factory.New<ClientCompany>();
			clientCompany1c.LCC_LD = database1.PK;
			clientCompany1c.LCC_Code = "MMM";

			var database2 = Factory.New<LicenceDatabase>();
			database2.LD_ServerCode = "SYD";
			database2.LD_LE = enterprise1.PK;

			Factory.Save();

			var incident1 = Factory.New<SupportIncident>();
			incident1.IM_Product = ProductTypes.Codes.Enterprise;
			incident1.IM_LD = database1.PK;

			incident1.Validation.ValidateClientCompanyCode();
			AssertHasErrors(incident1.ClientCompanyCodeInfo);

			database1.LD_Product = "SPH";
			incident1.IM_Product = "AAA";
			incident1.Validation.ValidateClientCompanyCode();
			AssertNoErrors("not mandatory for non-enterprise database product", incident1.ClientCompanyCodeInfo);

			database1.LD_Product = ProductTypes.Codes.CargoWiseOne;
			incident1.IM_Product = ProductTypes.Codes.Enterprise;
			incident1.ClientCompanyCode = clientCompany1a.LCC_Code;
			incident1.Validation.ValidateClientCompanyCode();
			AssertNoErrors(incident1.ClientCompanyCodeInfo);

			incident1.ClientCompanyCode = ZString.Empty;
			incident1.Validation.ValidateClientCompanyCode();
			AssertHasErrors(incident1.ClientCompanyCodeInfo);

			incident1.IM_OH_Client = org1.PK;
			incident1.ClientCompanyCode = clientCompany1b.LCC_Code;
			incident1.Validation.ValidateClientCompanyCode();
			AssertNoErrors(incident1.ClientCompanyCodeInfo);

			database1.LD_LicenceType = DatabaseTypes.Codes.Production;
			clientCompany1b.LCC_DeactivateTimeUtc = new ZDateTime(2015, 11, 1);
			Factory.Save();

			var incident2 = Factory.New<SupportIncident>();
			incident2.IM_Product = ProductTypes.Codes.Enterprise;
			incident2.IM_LD = database1.PK;

			incident2.ClientCompanyCode = clientCompany1b.LCC_Code;
			incident2.Validation.ValidateClientCompanyCode();
			AssertHasErrors("Client company 1b is inactive", incident2.ClientCompanyCodeInfo);

			incident2.ClientCompanyCode = clientCompany1c.LCC_Code;
			incident2.Validation.ValidateClientCompanyCode();
			AssertNoErrors("Client company 1c is active", incident2.ClientCompanyCodeInfo);

			incident2.IM_LCC = clientCompany1b.PK;
			Factory.Save();

			var loadedIncident2 = new BusinessObjectFactory().Load<SupportIncident>(incident2.PK);
			loadedIncident2.Validation.ValidateClientCompanyCode();
			AssertNoErrors("No error for saved incident for client company 1b not active", loadedIncident2.ClientCompanyCodeInfo);
			AssertHasWarnings("Warning for saved incident for client company 1b not active", loadedIncident2.ClientCompanyCodeInfo);

			var incident3 = Factory.New<SupportIncident>();
			incident3.IM_LD = database2.PK;
			incident3.Validation.ValidateClientCompanyCode();
			AssertEquals(0, incident3.Lookups.ClientCompanyCodeDescriptionPairList.Count);
			AssertNoErrors("Client company is not mandatory if no client company for the selected database", incident3.ClientCompanyCodeInfo);

			incident3.IM_OH_Client = org1.PK;
			incident3.IM_LD = database1.PK;
			incident3.ClientCompanyCode = clientCompany1a.LCC_Code;
			clientCompany1a.LCC_OH = org2.PK;
			Factory.Save();

			var loadedIncident3 = new BusinessObjectFactory().Load<SupportIncident>(incident3.PK);
			loadedIncident3.Validation.ValidateClientCompanyCode();
			AssertNoErrors("No error for saved incident for client company is not linked to selected org", loadedIncident3.ClientCompanyCodeInfo);
			AssertNoWarnings("No warning for saved incident for client company is not linked to selected org", loadedIncident3.ClientCompanyCodeInfo);
		}

		public void TestClientCompanyCodeAndDatabaseAreMandatory()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Dmitry";

			string currentLicenceCode = Env.CurrentCompany.GetLicenceCode();

			LicenceEnterprise enterprise = Factory.New<LicenceEnterprise>();
			enterprise.LE_EnterpriseCode = currentLicenceCode.Substring(0, 3);
			enterprise.LE_OH = org.PK;

			var build = Factory.New<ReleaseBuild>();
			build.VersionNumber = new VersionNumber(LicenceDatabase.FirstReleaseCSBiDirectionMessage);

			// NOTE: this database has no related LicenceHeader
			LicenceDatabase database = Factory.New<LicenceDatabase>();
			database.LD_ServerCode = currentLicenceCode.Substring(6, 3);
			database.LD_LE = enterprise.PK;
			database.LD_HL_CurrentRunningVersion = build.PK;
			database.LD_PublicEmailAddressForUpdate = "test@test.com";
			database.LD_Product = ProductTypes.Codes.CargoWiseOne;

			ClientCompany clientCompany = Factory.New<ClientCompany>();
			clientCompany.LCC_Code = currentLicenceCode.Substring(3, 3);
			clientCompany.LCC_LD = database.PK;

			Factory.Save();

			// CASE A - both db and company are mandatory

			// arrange
			var incidentA = Factory.NewWithValidTestData<SupportIncident>();
			incidentA.IM_OH_Client = org.PK;
			incidentA.IM_OC_Contact = contact.PK;
			incidentA.IM_ClientIncidentReference = "SR00006000";

			incidentA.IM_Product = "GLW";
			incidentA.IM_LCC = ZGuid.Empty;
			incidentA.IM_LD = ZGuid.Empty;

			// preconditions
			AssertEquals("Case A, precondition", ZGuid.Empty, incidentA.IM_LCC);
			AssertEquals("Case A, precondition", ZGuid.Empty, incidentA.IM_LD);
			AssertEquals("Case A, precondition", "GLW", incidentA.IM_Product);
			AssertEquals("Case A, precondition", false, incidentA.IsInDatabase);

			// act & assert
			incidentA.Validation.ValidateDatabaseServerCode();
			AssertHasError("There must be a validation error for case A", incidentA.DatabaseServerCodeInfo, "Database is mandatory when the Product is ediEnterprise / CargoWiseOne / CargoWiseNext or GLOW.");
			incidentA.IM_LD = database.PK;  // we need a specified licence database to check client company code validation properly
			incidentA.Validation.ValidateClientCompanyCode();
			AssertHasError("There must be a validation error for case A", incidentA.ClientCompanyCodeInfo, "Company is mandatory when the Database Product is ediEnterprise / CargoWiseOne / CargoWiseNext.");

			// CASE B - db is mandatory and company is not mandatory

			// arrange
			var incidentB = Factory.NewWithValidTestData<SupportIncident>();
			incidentB.IM_OH_Client = org.PK;
			incidentB.IM_OC_Contact = contact.PK;
			incidentB.IM_ClientIncidentReference = "SR00006001";

			incidentB.IM_Product = "GLW";
			incidentB.IM_LCC = clientCompany.PK;

			Factory.Save();
			incidentB.IM_LCC = ZGuid.Empty;  // the client company code has been changed
			incidentB.IM_LD = ZGuid.Empty;

			// preconditions
			AssertEquals("Case B, precondition", ZGuid.Empty, incidentB.IM_LCC);
			AssertEquals("Case B, precondition", ZGuid.Empty, incidentB.IM_LD);
			AssertEquals("Case B, precondition", "GLW", incidentB.IM_Product);
			AssertEquals("Case B, precondition", true, incidentB.IsInDatabase);

			// act & assert
			incidentB.Validation.ValidateDatabaseServerCode();
			AssertHasError("There must be a validation error for case B", incidentB.DatabaseServerCodeInfo, "Database is mandatory when the Product is ediEnterprise / CargoWiseOne / CargoWiseNext or GLOW.");
			incidentB.IM_LD = database.PK;  // we need a specified licence database to check client company code validation properly
			incidentB.Validation.ValidateClientCompanyCode();
			AssertNoErrors("There must be no validation error for case B as database product is unknown", incidentB.ClientCompanyCodeInfo);

			// CASE C - not mandatory

			// arrange
			var incidentD = Factory.NewWithValidTestData<SupportIncident>();
			incidentD.IM_OH_Client = org.PK;
			incidentD.IM_OC_Contact = contact.PK;
			incidentD.IM_ClientIncidentReference = "SR00006003";

			incidentD.IM_Product = "SPH";
			incidentD.IM_LCC = ZGuid.Empty;
			incidentD.IM_LD = ZGuid.Empty;
			Factory.Save();

			// preconditions
			AssertEquals("Case C, precondition", ZGuid.Empty, incidentD.IM_LCC);
			AssertEquals("Case C, precondition", ZGuid.Empty, incidentD.IM_LD);
			AssertEquals("Case C, precondition", "SPH", incidentD.IM_Product);
			AssertEquals("Case C, precondition", true, incidentD.IsInDatabase);

			// act & assert
			incidentD.Validation.ValidateDatabaseServerCode();
			AssertNoErrors("Case C: database is not mandatory", incidentD.DatabaseServerCodeInfo);
			incidentD.Validation.ValidateClientCompanyCode();
			AssertNoErrors("Case C: client company is not mandatory", incidentD.ClientCompanyCodeInfo);
		}

		public void TestIM_OA_BranchAddress()
		{
			EDIOrgHeader activeClient = Factory.NewWithValidTestData<EDIOrgHeader>();
			EDIOrgHeader inactiveClient = Factory.NewWithValidTestData<EDIOrgHeader>();
			activeClient.OH_IsActive = true;
			inactiveClient.OH_IsActive = false;

			Factory.Save();

			var incident = Factory.New<SupportIncident>();

			incident.IM_OA_BranchAddress = ZGuid.NewZGuid();
			AssertHasErrors(incident.IM_OA_BranchAddressInfo);

			incident.IM_OA_BranchAddress = Factory.New<OrgAddress>().PK;
			AssertNoErrors(incident.IM_OA_BranchAddressInfo);

			incident.IM_OA_BranchAddress = ZGuid.Empty;
			AssertHasErrors(incident.IM_OA_BranchAddressInfo);

			incident.IM_OA_BranchAddress = inactiveClient.Addresses.MainAddress.PK;
			AssertHasError(incident.IM_OA_BranchAddressInfo, "Please do not select Inactive Clients.");

			incident.IM_OA_BranchAddress = activeClient.Addresses.MainAddress.PK;
			AssertNoErrors(incident.IM_OA_BranchAddressInfo);
		}

		public void TestIM_GS_NKSpecifiedBy()
		{
			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_GS_NKSpecifiedBy = "";
			incident.RunPreSaveValidation();
			AssertNoErrors(incident.IM_GS_NKSpecifiedByInfo);

			incident.SetupForProjectFeatureRequest();
			Assert(incident.IsProjectRelatedIncident);
			incident.RunPreSaveValidation();
			AssertHasErrors(incident.IM_GS_NKSpecifiedByInfo);
		}

		public void TestIM_Category()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_Category = "";
			AssertHasErrors(incident.IM_CategoryInfo);

			incident.IM_Category = "XXX";
			AssertHasErrors(incident.IM_CategoryInfo);

			foreach (CodeDescriptionPair stage in new SupportIncidentCategoriesList())
			{
				incident.IM_Category = stage.Code;
				AssertNoErrors(incident.IM_CategoryInfo);
			}
		}

		public void TestDefectCausedByWorkItemPK()
		{
			NewWorkItem workItem = Factory.NewWithValidTestData<NewWorkItem>();
			Factory.Save();

			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.RunPreSaveValidation();
			AssertNoErrors(incident.DefectCausedByWorkItemPKInfo);

			incident.DefectCausedByWorkItemPK = ZGuid.Invalid;
			incident.RunPreSaveValidation();
			AssertHasErrors(incident.DefectCausedByWorkItemPKInfo);

			incident.SetupForNewCreatedDefect();
			incident.DefectCausedByWorkItemPK = ZGuid.Empty;
			incident.RunPreSaveValidation();
			AssertNoErrors(incident.DefectCausedByWorkItemPKInfo);
			AssertHasWarnings(incident.DefectCausedByWorkItemPKInfo);

			incident.DefectCausedByWorkItemPK = workItem.PK;
			incident.RunPreSaveValidation();
			AssertNoErrors(incident.DefectCausedByWorkItemPKInfo);
			AssertNoWarnings(incident.DefectCausedByWorkItemPKInfo);
		}

		public void TestRelatedWorkItemMatchesDefectCausedByWorkItem()
		{
			NewWorkItem defectCausingWI = Factory.NewWithValidTestData<NewWorkItem>();

			SupportIncident defectIncident = Factory.NewWithValidTestData<SupportIncident>();
			defectIncident.IM_IncidentType = IncidentConstants.IncidentType.SupportIncident;
			defectIncident.IM_Category = SupportIncidentCategoriesList.Codes.Defect;
			defectIncident.IM_Status = SupportIncidentLookups.Status.Open;
			defectIncident.Escalate(SupportIncidentCategoriesList.Codes.Defect, "");

			defectIncident.DefectCausedByWorkItemPK = defectCausingWI.PK;
			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			SupportIncident reloadedDefectIncident = newFactory.Load<SupportIncident>(defectIncident.PK);
			reloadedDefectIncident.RelatedItems.Add(defectCausingWI);

			string expectedErrorMessage = "Causing WorkItem cannot also be a Related WorkItem. It cannot be both the cause and the fix of an incident.";
			reloadedDefectIncident.RunPreSaveValidation();
			AssertHasError(reloadedDefectIncident.DefectCausedByWorkItemPKInfo, expectedErrorMessage);

			NewWorkItem workItem = newFactory.NewWithValidTestData<NewWorkItem>();
			reloadedDefectIncident.DefectCausedByWorkItemPK = workItem.PK;
			reloadedDefectIncident.RunPreSaveValidation();
			AssertNoError(reloadedDefectIncident.DefectCausedByWorkItemPKInfo, expectedErrorMessage);
		}

		public void TestDetailNoteText()
		{
			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.DetailNoteText = "";
			incident.RunPreSaveValidation();
			AssertHasErrors("IncidentDetails is mandatory", incident.DetailNoteTextInfo);

			incident.DetailNoteText = "Test Details Data";
			incident.RunPreSaveValidation();
			AssertNoErrors("IncidentDetails is long enough", incident.DetailNoteTextInfo);

			incident.DetailNoteText = "\u24c8 \u2075 \u221e Details should be long enough";
			incident.RunPreSaveValidation();
			AssertNoErrors("IncidentDetails accepts non western europe chars", incident.DetailNoteTextInfo);
		}

		public void TestIM_OC_Contact()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Sam";

			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_OH_Client = org.PK;
			incident.IM_OC_Contact = contact.PK;
			incident.IM_ClientIncidentReference = "SR00005555";

			incident.Validation.ValidateIM_OC_Contact();
			AssertHasError(incident.IM_OC_ContactInfo, "Contact does not have a valid email address");

			contact.OC_Email = "invalidemail";
			incident.Validation.ValidateIM_OC_Contact();
			AssertHasError(incident.IM_OC_ContactInfo, "Contact does not have a valid email address");

			contact.OC_Email = "validemail@test.org";
			incident.Validation.ValidateIM_OC_Contact();
			AssertNoError(incident.IM_OC_ContactInfo, "Contact does not have a valid email address");

			string currentLicenceCode = Env.CurrentCompany.GetLicenceCode();

			LicenceEnterprise enterprise = Factory.New<LicenceEnterprise>();
			enterprise.LE_EnterpriseCode = currentLicenceCode.Substring(0, 3);
			enterprise.LE_OH = org.PK;

			var build = Factory.New<ReleaseBuild>();
			build.VersionNumber = new VersionNumber(LicenceDatabase.FirstReleaseCSBiDirectionMessage);

			LicenceDatabase database = Factory.New<LicenceDatabase>();
			database.LD_ServerCode = currentLicenceCode.Substring(6, 3);
			database.LD_LE = enterprise.PK;
			database.LD_HL_CurrentRunningVersion = build.PK;
			database.LD_PublicEmailAddressForUpdate = "test@test.com";

			ClientCompany clientCompany = Factory.New<ClientCompany>();
			clientCompany.LCC_Code = currentLicenceCode.Substring(3, 3);
			clientCompany.LCC_LD = database.PK;

			Factory.Save();

			incident.IM_LD = database.PK;
			incident.IM_LCC = clientCompany.PK;
			contact.OC_Email = "invalidemail";
			incident.Validation.ValidateIM_OC_Contact();
			AssertHasWarning(incident.IM_OC_ContactInfo, "Contact does not have a valid email address");

			contact.OC_Email = "validemail@test.org";
			incident.Validation.ValidateIM_OC_Contact();
			AssertNoError(incident.IM_OC_ContactInfo, "Contact does not have a valid email address");
		}

		public void TestRelatedProjectPK()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Sam";

			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_OH_Client = org.PK;
			incident.IM_OC_Contact = contact.PK;
			incident.IM_ClientIncidentReference = "SR00005555";

			incident.IM_Priority = Constants.CustomerService.CriticalityCodes.CR6_FeatureRequest;
			incident.IM_Category = SupportIncidentCategoriesList.Codes.FeatureRequest;
			incident.IM_Status = SupportIncidentLookups.Status.Open;
			incident.IM_Source = SupportIncidentLookups.SourceListConstants.CreatedFromProject;

			incident.RunPreSaveValidation();

			AssertHasWarning(incident.RelatedProjectPKInfo, "You have not entered a Related Project.");
		}

		public void TestValidateWorkflowTemplateMatchingProperties()
		{
			var areas = new CodeDescriptionPairList();
			areas.AddPair("XRM", "XRMs");
			EDIDataRegistry.Instance.ProductAreas.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, areas);

			var collection = new SystemProductCollection();
			var product = collection.AddNew(ProductTypes.Codes.Enterprise, "test", true);
			product.ModuleMappings.AddNew("INT", "Super Module A", "XRM", false);
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();

			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_OH_Client = org.PK;
			incident.IM_OC_Contact = contact.PK;

			incident.IM_Product = "ENT";
			incident.IM_ProgramArea = "XRM";
			incident.IM_Module = "INT";
			incident.IM_SourceModuleId = "ABC";
			incident.IM_Priority = Constants.CustomerService.CriticalityCodes.CR6_FeatureRequest;
			incident.IM_Category = SupportIncidentCategoriesList.Codes.FeatureRequest;
			incident.IM_RN_NKCountry = "AU";
			incident.IM_Language = "EN";

			var validationResult = incident.Validation.ValidateWorkflowTemplateMatchingProperties();
			AssertEquals(true, validationResult);

			ZString invalidValue = "**";
			ChangePropertyToInvalidValueAndAssert(incident, incident.IM_ProductInfo, invalidValue);
			ChangePropertyToInvalidValueAndAssert(incident, incident.IM_ProgramAreaInfo, invalidValue);
			ChangePropertyToInvalidValueAndAssert(incident, incident.IM_ModuleInfo, invalidValue);
			ChangePropertyToInvalidValueAndAssert(incident, incident.IM_SourceModuleIdInfo, (ZString)"\u2085");
			ChangePropertyToInvalidValueAndAssert(incident, incident.IM_RN_NKCountryInfo, invalidValue);
			ChangePropertyToInvalidValueAndAssert(incident, incident.IM_LanguageInfo, invalidValue);
		}

		void ChangePropertyToInvalidValueAndAssert(SupportIncident incident, IZPropertyInfo propertyInfo, IZType invalidValue)
		{
			var originalValue = propertyInfo.Value;
			propertyInfo.Value = invalidValue;
			var validationResult = incident.Validation.ValidateWorkflowTemplateMatchingProperties();
			AssertEquals(false, validationResult);
			propertyInfo.Value = originalValue;
		}
	}
}
