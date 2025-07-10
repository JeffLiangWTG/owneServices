using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using AuthenticationService.Client.Models;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing.USSalesTax;
using Enterprise.Client.EDI.Billing.Business.USSalesTax;
using Enterprise.Client.EDI.ClientConfiguration;
using Enterprise.Client.EDI.CommissionManagement.GUI;
using Enterprise.Client.EDI.CommissionManagement.Module;
using Enterprise.Client.EDI.DbRestoreKey;
using Enterprise.Client.EDI.DeviceManagement.Module;
using Enterprise.Client.EDI.EndpointManagement.Module;
using Enterprise.Client.EDI.FeatureControl.Module;
using Enterprise.Client.EDI.IdentityApplication;
using Enterprise.Client.EDI.IdentityCertificate;
using Enterprise.Client.EDI.IdentityTenant;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.IncidentManager.Module;
using Enterprise.Client.EDI.IssueManager.Module;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Module;
using Enterprise.Client.EDI.Mail.Module;
using Enterprise.Client.EDI.MarketingManager.Module;
using Enterprise.Client.EDI.MasterFiles;
using Enterprise.Client.EDI.MasterFiles.Module;
using Enterprise.Client.EDI.MasterFiles.ProcessManagement;
using Enterprise.Client.EDI.Modules;
using Enterprise.Client.EDI.ReleaseBuilds.Module;
using Enterprise.Client.EDI.Security;
using Enterprise.Client.EDI.TokenAuthenticationOnBoarding;
using Enterprise.Client.EDI.UserManagement.Module;
using Enterprise.Core.Modules;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.Rating;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.Telematics.ServiceTasks.TelematicsXmlMessageProcessors;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Microsoft.Win32;
using Moq;
using NUnit.Framework;
using WTG.IdentitySecurity;
using ZClientEDI.Test.Telematics;

namespace Enterprise.Client.EDI.Test
{
	[TestedType(typeof(ClientOverride))]
	public class ClientOverrideTest : ZArchitecture.Modules.Testing.ClientOverrideTest
	{
		protected override void AddToObjectFailListsIfAplicable(
			Regex objectRegex,
			string objectName,
			string objectScript,
			List<string> invalidScrips,
			List<string> inconsistentObjectNames)
		{
			if ("Constraint_XX_RelationType_XX_Relation1TableCode_XX_Relation2TableCode".Equals(objectName))
			{
				Assert(objectScript.Contains("CONSTRAINT"));
			}
			else
			{
				base.AddToObjectFailListsIfAplicable(objectRegex, objectName, objectScript, invalidScrips, inconsistentObjectNames);
			}
		}

		public void TestAllClientSpecificSecurityCheckpointsHaveValidModuleParent()
		{
			var securityWithAllSecurityCheckpoints = new SecurityCore(null, null, Guid.Empty, Guid.Empty, Guid.Empty);
			var securityVector = new SecurityVector();
			securityVector.Initialise(securityWithAllSecurityCheckpoints);

			var securityWithClientSecurityCheckpointsOnly = new SecurityCore(null, null, Guid.Empty, Guid.Empty, Guid.Empty);
			securityWithClientSecurityCheckpointsOnly.CheckPointLookUpTable_ForTest.Clear();
			ClientHookLoader.Instance.ClientHook.AddClientSpecificSecurityCheckpointsToSecurityInstance(securityWithClientSecurityCheckpointsOnly);

			CombineAssertions(delegate
			{
				foreach (var checkpoint in securityWithClientSecurityCheckpointsOnly.CheckPointLookUpTable_ForTest.Values)
				{
					if (!securityVector.Any(securityInfo => securityInfo.Checkpoint.Code == checkpoint.Code))
					{
						Fail(String.Format("Checkpoint needs to be linked to a valid module - Code:[{0}]  Path:[{1}]", checkpoint.LookupKey.Code, checkpoint.DisplayTextPathToSecurityRight));
					}
				}
			});

			Assert(true);
		}

		public void TestCustomerServiceIncidentJobInvoicingAndProfessionalServicesQuoteJobInvoicing()
		{
			var securityWithAllSecurityCheckpoints = new SecurityCore(null, null, Guid.Empty, Guid.Empty, Guid.Empty);
			var securityVector = new SecurityVector();
			securityVector.Initialise(securityWithAllSecurityCheckpoints);

			CombineAssertions(delegate
			{
				AssertNotNull(securityWithAllSecurityCheckpoints.FindCheckPoint(new CheckpointLookupKey(EDISecurityCheckpoints.Constants.CustomerServiceIncidentJobInvoicingReferenceName + SecurityCore.Invoicing)));
				AssertNotNull(securityWithAllSecurityCheckpoints.FindCheckPoint(new CheckpointLookupKey(EDISecurityCheckpoints.Constants.CustomerServiceIncidentJobInvoicingReferenceName + SecurityCore.InvoicingEntry)));
				AssertNotNull(securityWithAllSecurityCheckpoints.FindCheckPoint(new CheckpointLookupKey(EDISecurityCheckpoints.Constants.CustomerServiceIncidentJobInvoicingReferenceName + SecurityCore.AppendToUnpostedTransactionLineDescriptionForJob)));
			});

			CombineAssertions(delegate
			{
				AssertNotNull(securityWithAllSecurityCheckpoints.FindCheckPoint(new CheckpointLookupKey(EDISecurityCheckpoints.Constants.ProfessionalServicesQuoteJobInvoicingReferenceName + SecurityCore.Invoicing)));
				AssertNotNull(securityWithAllSecurityCheckpoints.FindCheckPoint(new CheckpointLookupKey(EDISecurityCheckpoints.Constants.ProfessionalServicesQuoteJobInvoicingReferenceName + SecurityCore.InvoicingEntry)));
				AssertNotNull(securityWithAllSecurityCheckpoints.FindCheckPoint(new CheckpointLookupKey(EDISecurityCheckpoints.Constants.ProfessionalServicesQuoteJobInvoicingReferenceName + SecurityCore.AppendToUnpostedTransactionLineDescriptionForJob)));
			});
		}

		public void TestOrganisationMergeActionsHookup()
		{
			var list = (ArrayList)ObjectFactory.Get("OrganisationMergeActions");
			Assert("OrganisationMergeSyncWithBorderWiseAction should be added to OrganisationMergeActions", list.OfType<OrganisationMergeSyncWithBorderWiseAction>().Any());
		}

		public void TestUSSalesTaxHookup()
		{
			var list = (ArrayList)ObjectFactory.Get("IUSSalesTaxCalculator_ClientSpecific");
			Assert("AvalaraUSSalesTaxCalculatorFactory should be added as client specific IUSSalesTaxCalculatorFactory in ClientOverride", list.OfType<AvalaraUSSalesTaxCalculatorFactory>().Any());

			var salesTaxCalculator = ObjectFactory.Get<IUSSalesTaxCalculator>();
			using (EDIDataRegistry.Instance.AvalaraIntegrationSettingStatus
					.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, AvalaraConstants.IntegrationStatus.Codes.Sandbox)
			)
			{
				var config = salesTaxCalculator.GetConfiguration(GlbBranch.CurrentBranch);
				var isEnabled = salesTaxCalculator.IsEnabled(GlbBranch.CurrentBranch);
				AssertEquals("IUSSalesTaxCalculator Configuration should be overriden in ClientOverride", ConfigurationStatus.Sandbox, config);
				AssertEquals("IUSSalesTaxCalculator IsEnabled should be overriden in ClientOverride", true, isEnabled);
			}
		}

		public void TestCreateChartFXLicenceRegistryKey()
		{
			string chartFXRegKeyName = "864054A1-108E-4F89-B54F-842C5A886248";
			string chartFXLicence = @"TevdGftbAACX8aEnbQLjQJdjAAABAAAABUNMTjYwC1NGWERPV05MT0FEBE5TPTEBAAAAAUYkODY0MDU0QTEtMTA4RS00Rjg5LUI1NEYtODQyQzVBODg2MjQ4AF4wMDowRjpFQTo0QjpEODo4Rj8/R0JUX19fP0FXUkRBQ1BJPzc2NDg3LTY0MC0zOTQ3ODc0LTIzOTAzPzIwMDYwMTMxMTAyOTE5LjAwMDAwMCs2NjA/ODg0MTZBN0U/AAAAAA==
ejRDYItKqUTxv+8lU6f2WeO9ywOpNIyqJMQo/N9IkMfGNEEFMChQ9Iw+/To7oiAWH4QK2WHSq3taB0ikpU2aDCHgVegkbUCoLyCcUpi97LlKZCIiLYF4ShSmZHh40IbSBjuZawWAGFFF0GSsc24apdrZnV0X1qhbKyPfP3SCpFQ=";

			using (RegistryKey licencesKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software\\Classes\\Licenses"))
			using (RegistryKey chartFXKey = licencesKey.OpenSubKey(chartFXRegKeyName))
			{
				string value = (string)chartFXKey.GetValue("");
				AssertEquals(chartFXLicence, value);
			}
		}

		protected override Type ClientOverrideType
		{
			get
			{
				return typeof(ClientOverride);
			}
		}

		public void TestCMRMessageResponseProcessorSendReportToWebUsers()
		{
			Assert("Should be set to True", CMRMessageResponseProcessor.SendReportToWebUsers.Value);
		}

		public void TestClientOverrideAddsEDISecurityCheckpoints()
		{
			GlbSecurityCollection testSecurityCollection = new GlbSecurityCollection(GlbStaff.CurrentUser, new BusinessObjectFactory());
			SecurityCore securityInstance = new SecurityCore(testSecurityCollection, GlbStaff.CurrentUser, Env.CurrentBranch.PK, Env.CurrentDepartment.PK, Env.CurrentCompany.PK);

			AssertNotNull("The LicenceKeyBuilder checkpoint should be in the system", securityInstance.FindCheckPoint(new CheckpointLookupKey(EDISecurityCheckpoints.Constants.OrgLicenceModifyReferenceName)));
			AssertNotNull("The Licence Key Modification checkpoint should be in the system", securityInstance.FindCheckPoint(new CheckpointLookupKey(EDISecurityCheckpoints.Constants.OrgLicenceModifyKeyConfigurationSecurityReferenceName)));
			AssertNotNull("The Create and Send Licence Key checkpoint should be in the system", securityInstance.FindCheckPoint(new CheckpointLookupKey(EDISecurityCheckpoints.Constants.OrgLicenceCreateAndEmailKeyConfigurationSecurityReferenceName)));
			AssertNotNull("The Installation Details checkpoint should be in the system", securityInstance.FindCheckPoint(new CheckpointLookupKey(EDISecurityCheckpoints.Constants.OrgLicenceModifyInstallationDetailsSecurityReferenceName)));
			AssertNotNull("The Connection details checkpoint should be in the system", securityInstance.FindCheckPoint(new CheckpointLookupKey(EDISecurityCheckpoints.Constants.OrgLicenceModifyConnectionDetailsSecurityReferenceName)));
			AssertNotNull("The Send Upgrade checkpoint should be in the system", securityInstance.FindCheckPoint(new CheckpointLookupKey(EDISecurityCheckpoints.Constants.OrgLicenceModifySendUpgradeSecurityReferenceName)));
			AssertNotNull("The Save Upgrade To Disk checkpoint should be in the system", securityInstance.FindCheckPoint(new CheckpointLookupKey(EDISecurityCheckpoints.Constants.OrgLicenceModifySaveUpgradeToDiskReferenceName)));
			AssertNotNull("The Send Superseded Upgrade checkpoint should be in the system", securityInstance.FindCheckPoint(new CheckpointLookupKey(EDISecurityCheckpoints.Constants.OrgLicenceModifySendSupersededUpgradeReferenceName)));
			AssertNotNull("The Database details checkpoint should be in the system", securityInstance.FindCheckPoint(new CheckpointLookupKey(EDISecurityCheckpoints.Constants.OrgLicenceModifyDatabaseDetailsReferenceName)));
			AssertNotNull("Support and contact details should be in the system", securityInstance.FindCheckPoint(new CheckpointLookupKey(EDISecurityCheckpoints.Constants.OrgLicenceModifySupportAndContractDetailsReferenceName)));
			AssertNotNull("Licence 3rd Party Software checkpoint should be in the system", securityInstance.FindCheckPoint(new CheckpointLookupKey(EDISecurityCheckpoints.Constants.OrgLicenceModify3rdPartySoftwareReferenceName)));
			AssertNotNull("The Send Upgrade to Higher Ring DB checkpoint should be in the system.", securityInstance.FindCheckPoint(new CheckpointLookupKey(EDISecurityCheckpoints.Constants.OrgLicenceModifySendUpgradeToHigherRingDBCheckpointReferenceName)));
			AssertNotNull("The Send Upgrade to DB not on SQL 2005 checkpoint should be in the system.", securityInstance.FindCheckPoint(new CheckpointLookupKey(EDISecurityCheckpoints.Constants.OrgLicenceModifySendUpgradeToDBNotOnLowestSupportedSqlVersionCheckpointReferenceName)));
			AssertNotNull("The Implementation Mails checkpoint should be in the system", securityInstance.FindCheckPoint(new CheckpointLookupKey(EDISecurityCheckpoints.Constants.ImplementationEmailsReferenceName)));
			AssertNotNull("The Remove Licence Database checkpoint should be in the system", securityInstance.FindCheckPoint(new CheckpointLookupKey(EDISecurityCheckpoints.Constants.OrgLicenceRemoveLicenceDatabaseSecurityReferenceName)));
			AssertNotNull("The Move Licences To New Enterprise checkpoint should be in the system", securityInstance.FindCheckPoint(new CheckpointLookupKey(EDISecurityCheckpoints.Constants.OrgLicenceMoveLicencesToNewEnterpriseSecurityReferenceName)));
			AssertNotNull("The Customer Service checkpoint should be in the system", securityInstance.FindCheckPoint(new CheckpointLookupKey(EDISecurityCheckpoints.Constants.CustomerServiceReferenceName)));
			AssertNotNull("The Development checkpoint should be in the system", securityInstance.FindCheckPoint(new CheckpointLookupKey(EDISecurityCheckpoints.Constants.DevelopmentReferenceName)));
			AssertNotNull("SetDatabaseSecurityModeToOpen checkpoint should be in the system", securityInstance.FindCheckPoint(new CheckpointLookupKey(EDISecurityCheckpoints.Constants.OrgLicenceModifySetDatabaseSecurityModeToOpenReferenceName)));
			AssertNotNull("The Modify Exchange Rates And Tax checkpoint should be in the system", securityInstance.FindCheckPoint(new CheckpointLookupKey(EDISecurityCheckpoints.Constants.OrgLicenceModifyExchangeRatesAndTaxReferenceName)));
		}

		public void TestEDIAllWebSecurityRightsSuccesfullyRuns()
		{
			AssertType<EDIAllWebSecurityRights>("We've registered the type, should get it when we call", AllWebSecurityRights.New(new BusinessObjectFactory()));
		}

		public void TestDocumentEngineCollectionProviders()
		{
			DocumentEngine.RuntimeOptions.CollectionAndModuleIDBuilder.ClearLookupForTest();
			ClientOverride clientOverrideForTest = ClientOverride.Instance;
			AssertEquals(3, clientOverrideForTest.DocumentEngineCollectionProviders.Count);
			AssertEquals("LicenceEnterpriseCollectionProvider", clientOverrideForTest.DocumentEngineCollectionProviders["licence enterprise"].Name);
			AssertEquals("IncidentCollectionProvider", clientOverrideForTest.DocumentEngineCollectionProviders["incident"].Name);

			AssertNotNull("Client specific included in doc engine list", DocumentEngine.RuntimeOptions.CollectionAndModuleIDBuilder.GetCollectionAndModuleID(new BusinessObjectFactory(), "licence enterprise"));
		}

		public void TestDocumentEngineCodeDescriptionPairProviders()
		{
			ClientOverride clientOverrideForTest = ClientOverride.Instance;
			AssertEquals(7, clientOverrideForTest.DocumentEngineCodeDescriptionPairProviders.Count);
			AssertEquals(typeof(LicenceModuleCodeDescriptionPairProvider), clientOverrideForTest.DocumentEngineCodeDescriptionPairProviders["licencemodules"]);
			AssertEquals(typeof(SupportIncidentCategoryCodeDescriptionPairProvider), clientOverrideForTest.DocumentEngineCodeDescriptionPairProviders["incidentcategories"]);
			AssertEquals(typeof(SupportIncidentModuleCodeDescriptionPairProvider), clientOverrideForTest.DocumentEngineCodeDescriptionPairProviders["incidentmodules"]);
			AssertEquals(typeof(SupportIncidentSourceModuleCodeDescriptionPairProvider), clientOverrideForTest.DocumentEngineCodeDescriptionPairProviders["incidentsourcemodules"]);
			AssertEquals(typeof(SupportIncidentProductCodeDescriptionPairProvider), clientOverrideForTest.DocumentEngineCodeDescriptionPairProviders["incidentproducts"]);
			AssertEquals(typeof(SupportIncidentProductAreaCodeDescriptionPairProvider), clientOverrideForTest.DocumentEngineCodeDescriptionPairProviders["incidentproductareas"]);
			AssertEquals(typeof(SupportIncidentDispositionCodeDescriptionPairProvider), clientOverrideForTest.DocumentEngineCodeDescriptionPairProviders["incidentdispositions"]);

			AssertNotNull("Client specific included in doc engine list", DocumentEngine.RuntimeOptions.CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("licencemodules"));
			AssertNotNull("Client specific included in doc engine list", DocumentEngine.RuntimeOptions.CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("incidentcategories"));
			AssertNotNull("Client specific included in doc engine list", DocumentEngine.RuntimeOptions.CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("incidentmodules"));
			AssertNotNull("Client specific included in doc engine list", DocumentEngine.RuntimeOptions.CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("incidentsourcemodules"));
			AssertNotNull("Client specific included in doc engine list", DocumentEngine.RuntimeOptions.CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("incidentproducts"));
			AssertNotNull("Client specific included in doc engine list", DocumentEngine.RuntimeOptions.CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("incidentproductareas"));
			AssertNotNull("Client specific included in doc engine list", DocumentEngine.RuntimeOptions.CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider("incidentdispositions"));
		}

		public void TestTelematicsExtensionIsRegistered()
		{
			// Arrange
			var loggerMock = new Mock<ILogger>();

			// Act
			var types = ObjectFactory.Get<ITelematicsXmlMessageTypeProcessorsFactory>()
				.GetProcessors(loggerMock.Object)
				.Select(processor => processor.GetType())
				.ToList();

			// Assert
			CombineAssertions(() =>
			{
				foreach (var type in TelematicsExtensionTest.ExtensionTypes)
				{
					AssertCollectionContains(type, types);
				}
			});
		}

		#region Test Client Controllers

		public void TestControllerOverrides()
		{
			ClientOverride clientOverride = ClientOverride.Instance;
			ControllerOverrides controllerOverrides = clientOverride.ControllerOverrides;

			AssertControllerInfo(controllerOverrides, ControllerIDs.Organisation, typeof(EDIOrganisationControllerOverride));
			AssertControllerInfo(controllerOverrides, ControllerIDs.GlbCompanyCampaign, typeof(EDICRMGlbCompanyCampaignController));
			AssertControllerInfo(controllerOverrides, ControllerIDs.Opportunity, typeof(EDIOrgOpportunityController));
			AssertControllerInfo(controllerOverrides, ControllerIDs.SalesEnquiry, typeof(EDISalesInquiryController));
			AssertControllerInfo(controllerOverrides, ControllerIDs.GlbStaff, typeof(EDIGlbStaffControllerOverride));
			AssertControllerInfo(controllerOverrides, ControllerIDs.AccChargeCode, typeof(EDIAccChargeCodeController));
		}

		void AssertControllerInfo(ControllerOverrides controllerOverrides, ControllerID controllerID, Type classType)
		{
			ClientOverrideControllerID clientControllerID = new ClientOverrideControllerID(controllerID);
			ClientOverrideControllerInfo controllerInfo = (ClientOverrideControllerInfo)controllerOverrides[clientControllerID, null];

			AssertNotNull(controllerInfo);
			AssertEquals(true, string.IsNullOrEmpty(controllerInfo.CountryCodeForTest));
			AssertEquals(true, controllerInfo.IsClientOverride);
			AssertEquals(false, controllerInfo.CanOverrideForClientWithNoCountryWhenOtherCountryOverridesExist);
			AssertEquals(classType.Assembly.FullName, controllerInfo.AssemblyNameForTest);
			AssertEquals(classType.FullName, controllerInfo.ClassFullNameForTest);
		}

		public void TestNewClientControllers()
		{
			ClientOverride clientOverride = ClientOverride.Instance;
			AssertContainsNewClientController(clientOverride, ClientControllerRegistration.IssueManager, typeof(IssueManagerController));
			AssertContainsNewClientController(clientOverride, ClientControllerRegistration.UserAgreements, typeof(EdiUserAgreementController));
			AssertContainsNewClientController(clientOverride, ClientControllerRegistration.ObsoleteNonGenericWorkItemForOldHyperlinksOnly, typeof(ObsoleteNonGenericWorkItemControllerForOldHyperlinksOnly));
			AssertContainsNewClientController(clientOverride, ClientControllerRegistration.EdiIncidentRequest, typeof(EdiIncidentRequestController));
			AssertContainsNewClientController(clientOverride, ClientControllerRegistration.SupportIncident, typeof(SupportIncidentController));
			AssertContainsNewClientController(clientOverride, ClientControllerRegistration.DbRestoreKey, typeof(DbRestoreKeyController));
			AssertContainsNewClientController(clientOverride, ClientControllerRegistration.GlbReleaseNote, typeof(GlbReleaseNoteController));
			AssertContainsNewClientController(clientOverride, ClientControllerRegistration.ReleaseBuild, typeof(ReleaseBuildController));
			AssertContainsNewClientController(clientOverride, ClientControllerRegistration.LicenceEnterprise, typeof(LicenceEnterpriseController));
			AssertContainsNewClientController(clientOverride, ClientControllerRegistration.LicenseKey, typeof(LicenceKeyController));
			AssertContainsNewClientController(clientOverride, ClientControllerRegistration.Organisations, typeof(EDIOrganisationControllerOverride));
			AssertContainsNewClientController(clientOverride, ClientControllerRegistration.LicenceDatabase, typeof(LicenceDatabaseController));
			AssertContainsNewClientController(clientOverride, ClientControllerRegistration.EdiTrustedMessagingConfig, typeof(EdiTrustedMessagingConfigController));
			AssertContainsNewClientController(clientOverride, ClientControllerRegistration.EdiTrustedSystem, typeof(EdiTrustedSystemController));
			AssertContainsNewClientController(clientOverride, ClientControllerRegistration.LicenceHeader, typeof(LicenceHeaderController));
			AssertContainsNewClientController(clientOverride, ClientControllerRegistration.ProfessionalServicesQuote, typeof(ProfessionalServicesQuoteController));
			AssertContainsNewClientController(clientOverride, ClientControllerRegistration.WiseServicePartnerSurveyPlugIn, typeof(WiseServicePartnerSurveyPluginController));
			AssertContainsNewClientController(clientOverride, ClientControllerRegistration.SupportIncidentClientOrgLicence, typeof(SupportIncidentClientOrgLicenceController));
			AssertContainsNewClientController(clientOverride, ClientControllerRegistration.ProjectClientOrgLicence, typeof(ProjectClientOrgLicenceController));
			AssertContainsNewClientController(clientOverride, ClientControllerRegistration.OpportunityClientOrgLicence, typeof(OpportunityClientOrgLicenceController));
			AssertContainsNewClientController(clientOverride, ClientControllerRegistration.MonthlyUsageBilling, typeof(Billing.Module.MonthlyUsageBillingController));
			AssertContainsNewClientController(clientOverride, ClientControllerRegistration.StlBilling, typeof(Billing.Module.StlBillingController));
			AssertContainsNewClientController(clientOverride, ClientControllerRegistration.EngineeringTask, typeof(EngineeringTaskController));
			AssertContainsNewClientController(clientOverride, ClientControllerRegistration.MaintenanceBilling, typeof(Billing.Module.MaintenanceBillingController));
			AssertContainsNewClientController(clientOverride, ClientControllerRegistration.BillingPrices, typeof(Billing.Module.BillingPrices.BillingPricesController));
			AssertContainsNewClientController(clientOverride, ClientControllerRegistration.ChargeCodeCommissionConfiguration, typeof(ChargeCodeCommissionConfigurationPluginController));
			AssertContainsNewClientController(clientOverride, ClientControllerRegistration.TokenAuthenticationOnBoarding, typeof(TokenAuthenticationOnBoardingController));
			AssertContainsNewClientController(clientOverride, ClientControllerRegistration.EdiIdentityApplication, typeof(EdiIdentityApplicationController));
			AssertContainsNewClientController(clientOverride, ClientControllerRegistration.EdiIdentityTenant, typeof(EdiIdentityTenantController));
		}

		void AssertContainsNewClientController(ClientOverride clientOverride, ClientControllerID controllerID, Type expectedControllerType)
		{
			ControllerInfo newControllerInfo = Array.Find(clientOverride.NewClientControllers,
				(ControllerInfo controllerInfo) => { return controllerInfo.ID == controllerID; });
			AssertNotNull(controllerID + " is not found", newControllerInfo);
			AssertEquals(expectedControllerType.FullName, newControllerInfo.ClassFullNameForTest);
			AssertEquals(expectedControllerType.Assembly.FullName, newControllerInfo.AssemblyNameForTest);
		}

		#endregion Test Client Controllers

		#region Test Client Modules

		public void TestModuleOverrides()
		{
			ClientOverride clientOverride = ClientOverride.Instance;
			ModuleOverrides moduleOverrides = clientOverride.ModuleOverrides;

			AssertModuleInfo(moduleOverrides, ModuleIDs.Organisation, typeof(EDIOrganisationModule));
			AssertModuleInfo(moduleOverrides, ModuleIDs.OrgContacts, typeof(EDIOrgContactsModule));
			AssertModuleInfo(moduleOverrides, ModuleIDs.ProcessTasks, typeof(EDIProcessTasksModule));
			AssertModuleInfo(moduleOverrides, ModuleIDs.GlbCompanyCampaign, typeof(EDICRMGlbCompanyCampaignModule));
			AssertModuleInfo(moduleOverrides, ModuleIDs.InternationalZone, typeof(EDIRefZoneHeaderModule));
			AssertModuleInfo(moduleOverrides, ModuleIDs.Opportunity, typeof(EDIOrgOpportunityModule));
			AssertModuleInfo(moduleOverrides, ModuleIDs.Commission, typeof(EDICommissionManagementModule));
			AssertModuleInfo(moduleOverrides, ModuleIDs.Messaging.EDIInterchange, typeof(Messaging.EDIEDIInterchangeModule));
			AssertModuleInfo(moduleOverrides, ModuleIDs.Messaging.EDIMessage, typeof(Messaging.EDIEDIMessageModule));
			AssertModuleInfo(moduleOverrides, ModuleIDs.AdministrationPanel, typeof(EDIAdministrationPanelModule));
			AssertModuleInfo(moduleOverrides, ModuleIDs.GlbStaff, typeof(EdiStaffModule));
			AssertModuleInfo(moduleOverrides, ModuleIDs.RefDocOrgCusCode, typeof(EDIRefDocOrgCusCodeModule));
		}

		void AssertModuleInfo(ModuleOverrides moduleOverrides, ModuleIdentifier moduleID, Type classType)
		{
			ClientOverrideModuleIdentifier clientModuleID = new ClientOverrideModuleIdentifier(moduleID);
			ClientOverrideModuleInfo moduleInfo = (ClientOverrideModuleInfo)moduleOverrides[clientModuleID, null];

			AssertNotNull(moduleInfo);
			AssertEquals(true, string.IsNullOrEmpty(moduleInfo.CountryCodeForTest));
			AssertEquals(true, moduleInfo.IsClientOverride);
			AssertEquals(false, moduleInfo.CanOverrideForClientWithNoCountryWhenOtherCountryOverridesExist);
			AssertEquals(classType.Assembly.FullName, moduleInfo.AssemblyNameForTest);
			AssertEquals(classType.FullName, moduleInfo.ClassFullNameForTest);
		}

		public void TestModuleTableNameForModuleOverrides()
		{
			var clientOverride = ClientOverride.Instance;
			var moduleOverrides = clientOverride.ModuleOverrides;

			AssertModuleTableName(moduleOverrides, ModuleIDs.Organisation, "OrgHeader");
			AssertModuleTableName(moduleOverrides, ModuleIDs.OrgContacts, "OrgContact");
			AssertModuleTableName(moduleOverrides, ModuleIDs.ProcessTasks, "ProcessTasks");
			AssertModuleTableName(moduleOverrides, ModuleIDs.GlbCompanyCampaign, "GlbCompanyCampaign");
			AssertModuleTableName(moduleOverrides, ModuleIDs.InternationalZone, "RefZoneHeader");
			AssertModuleTableName(moduleOverrides, ModuleIDs.Opportunity, "OrgOpportunity");
			AssertModuleTableName(moduleOverrides, ModuleIDs.Messaging.EDIInterchange, "EDIInterchange");
			AssertModuleTableName(moduleOverrides, ModuleIDs.Messaging.EDIMessage, "EDIMessage");
			AssertModuleTableName(moduleOverrides, ModuleIDs.Project, "WorkProject");
			AssertModuleTableName(moduleOverrides, ModuleIDs.WorkItem, "WorkItem");
			AssertModuleTableName(moduleOverrides, ModuleIDs.GlbStaff, "GlbStaff");
		}

		void AssertModuleTableName(ModuleOverrides moduleOverrides, ModuleIdentifier moduleID, string tableName)
		{
			var clientModuleID = new ClientOverrideModuleIdentifier(moduleID);
			var moduleInfo = (ClientOverrideModuleInfo)moduleOverrides[clientModuleID, null];

			AssertEquals(tableName, moduleInfo.TableName);
		}

		public void TestNewModules()
		{
			ClientOverride clientOverride = ClientOverride.Instance;
			string operations = ModuleTreeLoaderConstant.Category.Operations.Name;

			// Development
			const string development = ClientModuleRegistration.Section.Development;
			AssertContainsNewClientModule(clientOverride, ClientModuleRegistration.ProfessionalServicesQuote, operations, development, typeof(ProfessionalServicesQuoteModule), "IncidentMain");
			AssertContainsNewClientModule(clientOverride, ClientModuleRegistration.IssueManager, operations, development, typeof(IssueManagerModule), "HelpErrorLog");
			AssertContainsNewClientModule(clientOverride, ClientModuleRegistration.UserAgreements, operations, development, typeof(EdiUserAgreementModule), "UserAgreement");
			AssertContainsNewClientModule(clientOverride, ClientModuleRegistration.UserAgreementAcceptances, operations, development, typeof(EdiUserAgreementAcceptanceLogModule), "UserAgreementAcceptanceLog");
			AssertContainsNewClientModule(clientOverride, ClientModuleRegistration.GlbReleaseNote, operations, development, typeof(GlbReleaseNoteModule), "GlbReleaseNote");
			AssertContainsNewClientModule(clientOverride, ClientModuleRegistration.WorkItemsReports, operations, development, typeof(WorkItemReports));
			AssertContainsNewClientModule(clientOverride, ClientModuleRegistration.FeatureControl, operations, development, typeof(FeatureControlModule), "FeatureControlHeader");
			AssertContainsNewClientModule(clientOverride, ClientModuleRegistration.FeatureSet, operations, development, typeof(FeatureSetModule), "FeatureControlSet");

			// Customer Service
			const string custService = ClientModuleRegistration.Section.CustomerService;
			AssertContainsNewClientModule(clientOverride, ClientModuleRegistration.SupportIncident, operations, custService, typeof(SupportIncidentModule), "IncidentMain");
			AssertContainsNewClientModule(clientOverride, ClientModuleRegistration.EDICustomerServiceEmails, operations, custService, typeof(CustomerServiceMailItemModule), "MailDBItems");
			AssertContainsNewClientModule(clientOverride, ClientModuleRegistration.DbRestoreKey, operations, custService, typeof(DbRestoreKeyModule));
			AssertContainsNewClientModule(clientOverride, ClientModuleRegistration.ReleaseBuild, operations, custService, typeof(ReleaseBuildModule), "ReleaseBuild");
			AssertContainsNewClientModule(clientOverride, ClientModuleRegistration.LicenceEnterprise, operations, custService, typeof(LicenceEnterpriseModule), "LicenceEnterprise");
			AssertContainsNewClientModule(clientOverride, ClientModuleRegistration.LicenseKey, operations, custService, typeof(LicenseKeyModule), "LicenceHeader");
			AssertContainsNewClientModule(clientOverride, ClientModuleRegistration.LicenceDatabase, operations, custService, typeof(LicenceDatabaseModule), "LicenceDatabase");
			AssertContainsNewClientModule(clientOverride, ClientModuleRegistration.EdiTrustedMessagingConfig, operations, custService, typeof(EdiTrustedMessagingConfigModule), "EdiTrustedMessagingConfig");
			AssertContainsNewClientModule(clientOverride, ClientModuleRegistration.EdiTrustedSystem, operations, custService, typeof(EdiTrustedSystemModule), "EdiTrustedSystem");
			AssertContainsNewClientModule(clientOverride, ClientModuleRegistration.CustomerServiceReports, operations, custService, typeof(CustomerServiceReports));
			AssertContainsNewClientModule(clientOverride, ClientModuleRegistration.IncidentManagementGroup, operations, custService, typeof(IncidentManagementGroupModule), "IncidentManagementGroup");
			AssertContainsNewClientModule(clientOverride, ClientModuleRegistration.EdiIdentityCertificate, operations, custService, typeof(EdiIdentityCertificateModule), "EdiIdentityCertificate");
			AssertContainsNewClientModule(clientOverride, ClientModuleRegistration.IncidentTriage, operations, custService, typeof(IncidentTriageModule), "IncidentTriage");
			AssertContainsNewClientModule(clientOverride, ClientModuleRegistration.IncidentTriageChecklistItem, operations, custService, typeof(IncidentTriageChecklistItemModule), "IncidentTriageChecklistItem");
			AssertContainsNewClientModule(clientOverride, ClientModuleRegistration.IncidentDiagnosticCriteria, operations, custService, typeof(IncidentDiagnosticCriteriaModule), "IncidentDiagnosticCriteria");
			AssertContainsNewClientModule(clientOverride, ClientModuleRegistration.TokenAuthenticationOnBoarding, operations, custService, typeof(TokenAuthenticationOnBoardingModule), "EdiTokenAuthOnBoardingData");
			AssertContainsNewClientModule(clientOverride, ClientModuleRegistration.EdiIdentityApplication, operations, custService, typeof(EdiIdentityApplicationModule), "EdiIdentityApplication");
			AssertContainsNewClientModule(clientOverride, ClientModuleRegistration.EdiIdentityTenant, operations, custService, typeof(EdiIdentityTenantModule), "EdiIdentityTenant");

			// Device Management
			AssertContainsNewClientModule(clientOverride, ClientModuleRegistration.ClientDeviceTemplate, operations, custService, typeof(ClientDeviceHeaderTemplateModule), "DmgDeviceHeader");
			AssertContainsNewClientModule(clientOverride, ClientModuleRegistration.ClientDevice, operations, custService, typeof(ClientDeviceHeaderModule), "DmgDeviceHeader");

			// System And User Management
			const string management = ClientModuleRegistration.Section.SystemAndUserManagement;
			AssertContainsNewClientModule(clientOverride, ClientModuleRegistration.LicenceDatabaseRegistration, operations, management, typeof(LicenceDatabaseRegistrationModule), "LicenceDatabase");
			AssertContainsNewClientModule(clientOverride, ClientModuleRegistration.SystemUserAccounts, operations, management, typeof(SystemUserAccountsModule), "EdiCustomerUserAccount");

			// Others
			AssertContainsNewClientModule(clientOverride, ClientModuleRegistration.LicenceHeader, "", "", typeof(LicenceHeaderModule), "LicenceHeader");

			AssertContainsNewClientModule(clientOverride, ClientModuleRegistration.MonthlyUsageBilling,
				ModuleTreeLoaderConstant.Category.Manage,
				Enterprise.ZArchitecture.Modules.ModuleTreeLoaderConstant.Section.Receivables,
				typeof(Billing.Module.MonthlyUsageBillingModule));

			AssertContainsNewClientModule(clientOverride, ClientModuleRegistration.StlBilling,
				ModuleTreeLoaderConstant.Category.Manage,
				Enterprise.ZArchitecture.Modules.ModuleTreeLoaderConstant.Section.Receivables,
				typeof(Billing.Module.StlBillingModule));

			AssertContainsNewClientModule(clientOverride, ClientModuleRegistration.MaintenanceBilling,
				ModuleTreeLoaderConstant.Category.Manage,
				Enterprise.ZArchitecture.Modules.ModuleTreeLoaderConstant.Section.Receivables,
				typeof(Billing.Module.MaintenanceBillingModule));

			AssertContainsNewClientModule(clientOverride, ClientModuleRegistration.BillingPrices,
				ModuleTreeLoaderConstant.Category.Manage,
				Enterprise.ZArchitecture.Modules.ModuleTreeLoaderConstant.Section.Receivables,
				typeof(Billing.Module.BillingPrices.BillingPricesModule), "ClientLicencePriceItem");

			AssertContainsNewClientModule(clientOverride, ClientModuleRegistration.ImplementationEmails,
				ModuleTreeLoaderConstant.Category.Manage,
				Enterprise.ZArchitecture.Modules.ModuleTreeLoaderConstant.Section.ManageWorkflowSection,
				typeof(ImplementationMailItemModule), "MailDBItems");
		}

		void AssertContainsNewClientModule(ClientOverride clientOverride, ClientModuleIdentifier newClientModuleID, ModuleTreeLoaderConstant.Entry expectedCategory, ModuleTreeLoaderConstant.Entry expectedSection, Type expectedModuleType, string expectedTableName = "")
		{
			AssertContainsNewClientModule(clientOverride, newClientModuleID, expectedCategory.Name, expectedSection.Name, expectedModuleType, "", expectedTableName);
		}

		void AssertContainsNewClientModule(ClientOverride clientOverride, ClientModuleIdentifier newClientModuleID, ModuleTreeLoaderConstant.Entry expectedCategory, string expectedSection, Type expectedModuleType, string expectedTableName = "")
		{
			AssertContainsNewClientModule(clientOverride, newClientModuleID, expectedCategory.Name, expectedSection, expectedModuleType, "", expectedTableName);
		}

		void AssertContainsNewClientModule(ClientOverride clientOverride, ClientModuleIdentifier newClientModuleID, string expectedCategory, string expectedSection, Type expectedModuleType, string expectedTableName = "")
		{
			AssertContainsNewClientModule(clientOverride, newClientModuleID, expectedCategory, expectedSection, expectedModuleType, "", expectedTableName);
		}

		void AssertContainsNewClientModule(ClientOverride clientOverride, ClientModuleIdentifier newClientModuleID, string expectedCategory, string expectedSection, Type expectedModuleType, string expectedCountryCode, string expectedTableName = "")
		{
			NewClientModuleInfo newModuleInfo = Array.Find(clientOverride.NewClientModules,
				(NewClientModuleInfo moduleInfo) => { return moduleInfo.ID == newClientModuleID; });
			AssertNotNull(newClientModuleID.Name + " is not found", newModuleInfo);

			AssertEquals(expectedCategory, newModuleInfo.CategoryName);
			AssertEquals(expectedSection, newModuleInfo.SectionName);
			AssertEquals(expectedModuleType.FullName, newModuleInfo.Info.ClassFullNameForTest);
			AssertEquals(expectedModuleType.Assembly.FullName, newModuleInfo.Info.AssemblyNameForTest);
			AssertEquals(expectedCountryCode, newModuleInfo.Info.CountryCodeForTest);
			AssertEquals(expectedTableName, newModuleInfo.Info.TableName);
		}

		#endregion Test Client Modules

		#region Test Client Sections

		public void TestNewModuleSection()
		{
			ClientOverride clientOverrideForTest = ClientOverride.Instance;
			AssertEquals(3, clientOverrideForTest.NewModuleSectionsToAddForClient.Length);
			AssertNewModuleSectionAddOn(clientOverrideForTest.NewModuleSectionsToAddForClient[0], ModuleTreeLoaderConstant.Category.Operations.Name, ClientModuleRegistration.Section.Development, "Development", EDISecurityCheckpoints.Development, IconTypes.Truck, IconTypes.Truck20x16, ClientModuleRegistration.Subcategory.WTG);
			AssertNewModuleSectionAddOn(clientOverrideForTest.NewModuleSectionsToAddForClient[1], ModuleTreeLoaderConstant.Category.Operations.Name, ClientModuleRegistration.Section.CustomerService, "Customer Service", EDISecurityCheckpoints.CustomerService, IconTypes.Phone, IconTypes.Phone20x16, ClientModuleRegistration.Subcategory.WTG);
			AssertNewModuleSectionAddOn(clientOverrideForTest.NewModuleSectionsToAddForClient[2], ModuleTreeLoaderConstant.Category.Operations.Name, ClientModuleRegistration.Section.SystemAndUserManagement, "System And User Management", EDISecurityCheckpoints.SystemAndUserManagement, IconTypes.SomeBooks, IconTypes.SomeBooks20x16, ClientModuleRegistration.Subcategory.WTG);
		}

		void AssertNewModuleSectionAddOn(IModuleSectionAddOn newSection, string expectedCategory, string expectedSectionName, string expectedDisplayText, SecurityCheckpoint expectedSecurity, IconTypes expectedIcon, IconTypes expectedGroupImage, ModuleTreeLoaderConstant.Entry expectedSubcategory)
		{
			ModuleSectionAddOn moduleSection = newSection as ModuleSectionAddOn;
			AssertNotNull("Has to be of type " + nameof(ModuleSectionAddOn), moduleSection);
			AssertEquals(expectedCategory, moduleSection.CategoryName);
			AssertEquals(expectedSectionName, moduleSection.Name);
			AssertEquals(expectedDisplayText, moduleSection.DisplayText);
			AssertEquals(expectedSecurity, moduleSection.SecurityCheckpoint);
			AssertEquals(expectedIcon, moduleSection.Icon);
			AssertEquals(expectedGroupImage, moduleSection.GroupImage);
			AssertEquals(expectedSubcategory.Name, moduleSection.Subcategory.Name);
		}

		#endregion Test Client Sections

		#region Test Client DB Schema

		public void TestTableSchemas()
		{
			AssertCollectionContains(ClientFeatureRequestValueAndContributionSchema.Instance, ClientOverride.Instance.TableSchemas);
			AssertCollectionContains(ClientLicenceBillingSchema.Instance, ClientOverride.Instance.TableSchemas);
			AssertCollectionContains(ClientLicenceBillingDiscountSchema.Instance, ClientOverride.Instance.TableSchemas);
			AssertCollectionContains(ClientLicencePriceHeaderSchema.Instance, ClientOverride.Instance.TableSchemas);
			AssertCollectionContains(ClientLicencePriceItemSchema.Instance, ClientOverride.Instance.TableSchemas);
			AssertCollectionContains(ClientLicenceFeeSchema.Instance, ClientOverride.Instance.TableSchemas);
			AssertCollectionContains(ClientChargeableUsageSchema.Instance, ClientOverride.Instance.TableSchemas);
			AssertCollectionContains(ClientInvoiceDeliverySchema.Instance, ClientOverride.Instance.TableSchemas);
		}

		// WTG.SqlTests/src/SqlTests/TheTest.cs
		public void TestAllTablesHaveClusteredIndex()
		{
			var sql =
	@"SELECT name
FROM sys.tables t
	WHERE is_ms_shipped = 0 AND object_id NOT IN (
	SELECT object_id
	FROM sys.indexes
	WHERE type = 1) ORDER BY 1;";

			AssertEDITables("All tables should have a clustered index so rows delete properly without TABLOCKX", sql, Enumerable.Empty<string>());
		}

		// WTG.SqlTests/src/SqlTests/TheTest.cs
		public void TestClusteredIndexesHaveNonNullableFirstKey()
		{
			var sql =
	@"SELECT o.name
FROM sys.indexes i
JOIN sys.index_columns ic ON ic.object_id = i.object_id AND ic.index_id = i.index_id
JOIN sys.columns c ON i.object_id = c.object_id AND ic.column_id = c.column_id
JOIN sys.objects o ON o.object_id = i.object_id
JOIN sys.schemas s ON o.schema_id = s.schema_id
WHERE i.type = 1 AND c.is_nullable = 1 AND key_ordinal = 1
AND s.name NOT IN ('sys', 'cdc') ORDER BY 1;";

			AssertEDITables("Clustered indexes should be static, narrow and incrementing. If your index contains values that change you should consider another key.", sql,
				new[]
				{
					"ClientLicenceFee",
					"ClientLicencePriceHeader",
					"ClientLicenceUsage",
					"EdiBilledDiscount",
					"EdiPriceHeaderLink",
					"EdiUserAgreementAcceptanceLog",
					"GlbTime",
					"HelpErrorLogOccurrence",
					"IncidentMain",
					"LicenceConnection",
					"LicenceModules",
					"UpgradesToClient",
					"EdiIdentityCertificate",
					"EdiIdentityApplication",
					"EdiIdentityRedirectUrl",
					"EdiIdentityTenant"
				});
		}

		// WTG.SqlTests/src/SqlTests/TheTest.cs
		public void TestIndexNamingConvention()
		{
			AssertIndexNamingConvention(unique: true, clustered: true);
			AssertIndexNamingConvention(unique: true, clustered: false);
			AssertIndexNamingConvention(unique: false, clustered: true);
			AssertIndexNamingConvention(unique: false, clustered: false);
		}

		void AssertIndexNamingConvention(bool unique, bool clustered)
		{
			var indexSecondPrefix = (unique ? "U" : "R") + (clustered ? "C" : "X");

			var sqlText = $@"
				SELECT tab.name [TAB], ind.name [IND]
				FROM sys.indexes ind
				JOIN sys.objects tab ON ind.object_id = tab.object_id
				WHERE (ObjectProperty(ind.object_id,'IsMSShipped') = 0)
				AND   tab.type = 'U'
				AND	  ind.is_primary_key = 0";

			sqlText += string.Format(CultureInfo.InvariantCulture, " AND (ind.name NOT LIKE 'NR[_]{0}[_][_]%') ", indexSecondPrefix);
			sqlText += string.Format(CultureInfo.InvariantCulture, " AND (ind.name NOT LIKE 'FK[_]{0}[_][_]%') ", indexSecondPrefix);

			sqlText += unique
				? " AND (ind.is_unique = 1) "
				: " AND (ind.is_unique = 0) ";
			sqlText += clustered
				? " AND (ind.type = 1 OR ind.type = 5) "
				: " AND (ind.type = 2 OR ind.type = 6 OR ind.type = 7) ";

			var message = new StringBuilder();
			message.Append(unique ? " Unique" : string.Empty);
			message.Append(clustered ? " clustered" : " non-clustered");
			message.AppendFormat(CultureInfo.InvariantCulture, " index(es) not following the [NR_{0}__*] or [FK_{0}__*] naming convention.", indexSecondPrefix);

			var ediTables = GetEdiTables();
			var tables = Utilities.GetDataTableFromQuery(TestConnection, sqlText).Rows.OfType<DataRow>()
			.Select(x => new { TAB = x["TAB"].ToString(), IND = x["IND"].ToString() })
			.Where(x => ediTables.Contains(x.TAB));
			AssertEquals(message.ToString(), "", string.Join("\r\n", tables.Select(x => $"{x.TAB}.{x.IND}")));
		}

		void AssertEDITables(string message, string tableQuery, IEnumerable<string> excludedTables)
		{
			var ediTables = GetEdiTables().Except(excludedTables).ToHashSet();
			var tables = string.Join("\r\n", Utilities.GetDataTableFromQuery(TestConnection, tableQuery).Rows.OfType<DataRow>()
			.Select(x => $"{x["name"]}").Where(x => ediTables.Contains(x)));
			AssertEquals(message, "", tables);
		}

		HashSet<string> GetEdiTables() => ClientOverride.Instance.TableSchemas.Select(x => x.TableName)
			.Except(new[] {
							"DmgDeviceHeader",    //Schema_Main.sql
							"DmgDeviceComponent", //Schema_Main.sql
							"DmgDeviceComponentIdentification" //Schema_Main.sql
			}).ToHashSet();

		#endregion

		public void TestGetClientSpecificJobInvoicingCheckpoints()
		{
			GlbSecurityCollection testSecurityCollection = new GlbSecurityCollection(GlbStaff.CurrentUser, new BusinessObjectFactory());
			SecurityCore securityInstance = new SecurityCore(testSecurityCollection, GlbStaff.CurrentUser, Env.CurrentBranch.PK, Env.CurrentDepartment.PK, Env.CurrentCompany.PK);
			ClientOverride clientOverrideForTest = ClientOverride.Instance;
			IEnumerable<ISecurityCheckpoint> checkpoints = clientOverrideForTest.GetClientSpecificJobInvoicingCheckpoints(securityInstance);

			AssertEquals(3, checkpoints.Count());
			Assert(checkpoints.Contains(securityInstance.FindCheckPoint(new CheckpointLookupKey(EDISecurityCheckpoints.Constants.CustomerServiceIncidentJobInvoicingReferenceName))));
			Assert(checkpoints.Contains(securityInstance.FindCheckPoint(new CheckpointLookupKey(EDISecurityCheckpoints.Constants.ProfessionalServicesQuoteJobInvoicingReferenceName))));
			Assert(checkpoints.Contains(securityInstance.FindCheckPoint(new CheckpointLookupKey(EDISecurityCheckpoints.Constants.InstallationTaskJobInvoicingReferenceName))));
		}

		public void TestClientSpecificHelpMenuItems()
		{
			var items = ((ArrayList)ObjectFactory.Get("ClientSpecificHelpMenuItems")).OfType<ToolStripMenuItem>().ToArray();
			var expectedItems = new[]
			{
				"My CW Product Keys",
				"Support Password",
				"Support Token",
				"SSO Auth Token",
				"Rates Service Admin",
			};
			AssertContainsExactElementsInExactOrder(expectedItems, items.Select(item => item.Text));
		}

		public void TestStaffProductKeysMenuItems()
		{
			var clientOverrideForTest = ClientOverride.Instance;

			var myCwProductKeyDropDownItem = GetClientSpecificHelpMenuItem("My CW Product Keys");
			myCwProductKeyDropDownItem.ShowDropDown();
			myCwProductKeyDropDownItem.HideDropDown();

			var productKeyDropDownItems = myCwProductKeyDropDownItem.DropDownItems.Cast<ToolStripDropDownItem>().Where(item => item.Text.StartsWith("WTL")).ToList();
			var createNewKeyDropDownItems = myCwProductKeyDropDownItem.DropDownItems.Cast<ToolStripDropDownItem>().Where(item => item.Text == StaffProductKeyMenu.CreateNewKeyCaption);
			var productKeyUnregisterDropDownItems = (from productKeyDropDownItem in productKeyDropDownItems from ToolStripDropDownItem unregisterDropDownItem in productKeyDropDownItem.DropDownItems where unregisterDropDownItem.Text == StaffProductKeyMenu.UnregisteredCaption select unregisterDropDownItem).ToList();

			AssertEquals(StaffProductKeyMenu.MyCwProductKeysCaption, myCwProductKeyDropDownItem.Text);
			Assert(productKeyDropDownItems.Count <= 10);
			AssertEquals(productKeyDropDownItems.Count, productKeyUnregisterDropDownItems.Count);
			AssertEquals(createNewKeyDropDownItems.ToList().Count, 1);
		}

		[DeveloperOnlyTest]
		public void TestSupportPasswordManagement()
		{
			var clientOverrideForTest = ClientOverride.Instance;

			var supportPasswordItem = GetClientSpecificHelpMenuItem("Support Password");

			AssertEquals(true, supportPasswordItem.ShowShortcutKeys);
			AssertEquals((Keys)Shortcut.CtrlShiftS, supportPasswordItem.ShortcutKeys);

			var password = MasterPassword.GenerateForCurrentUserToday();

			supportPasswordItem.PerformClick();
			AssertEquals("Password should be copied to clipboard", password, SafeClipboard.GetText());
		}

		[TestDate]
		[DeveloperOnlyTest]
		public void TestSupportTokenManagement()
		{
			var currentDate = DateTime.Now;
			TestDateAttribute.Date = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, currentDate.Hour, currentDate.Minute, currentDate.Second);
			_ = ClientOverride.Instance;

			var supportTokenItem = GetClientSpecificHelpMenuItem("Support Token");

			AssertEquals(true, supportTokenItem.ShowShortcutKeys);
			AssertEquals((Keys)Shortcut.CtrlShiftT, supportTokenItem.ShortcutKeys);

			using (EDIDataRegistry.Instance.CWSupportLoginTokenPrivateKey.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Encoding.UTF8.GetBytes(string.Empty)))
			{
				supportTokenItem.PerformClick();
				var expectedMessage = @"Please upload the private key in the registry item 'WiseTech Global Client Extensions -> Customer Service Incidents -> CWSupport Account Login Token Private Key' to sign the login token.";
				var lastMessage = UnitTestUserNotification.Instance.LastMessage.Text;
				AssertEquals(expectedMessage, lastMessage);
			}

			var privateKey = @"-----BEGIN PRIVATE KEY-----
MIIEvQIBADANBgkqhkiG9w0BAQEFAASCBKcwggSjAgEAAoIBAQCyrpE0VrQNxG8h
zQmpeQfkjRu80jvy7N6pTwbHeocNyV2sAkufUV7xuztamiWjCBUjEVTzdCLeAxcC
lQ7LmhU6nGjVTsNK6DaPQ2Zl67L2yJClMWdvZzuOxRx6JPFNa/XyboRLexIeqesz
n1W98T8mPBTs3rOrZXvyKLx5Jth+LISyw+Mo67yaouSdiw1GiucWBqIuBR49vyda
Mix+Ce3pK/YTIFnglFEzj7Dnxa6GhAe4fPTLUd02nu6+Ut5t8fHJUn0EB799+eAB
UqV0ZatdvlOlISmcJEXWL1/0IoEfGH4VDKD8zOb3sUOsOPLJBgGicpfb9LIfPfkh
5lqL1V0dAgMBAAECggEAK9R+ceRCzo285QGyuQujUAD9KNg5NGG+TLHB6/S2ZD9c
5vC5NB91tr5C1Pqy+MbmyG9b80wtsV/4qP1/X5owUuxDGu/zH9DOcV4LJD0o7ThN
ovf3c3BTP7ZCQgQF3QP6lLlfYlSSIUt1EninQ6yF3Q8n4uLOF+ERAlnTwbQxruE7
S442554sdsjRXzHX7+ql/XCZwZzE8NlSPBJTaLc1Df8GpPAyOvSUW2Dk08JkUNT5
iyqAK+NnGqGAd3qZGQ0AdFALk9v0mppvPhI0ldBjMkk1UjDRYswLDw+heb/53Smp
4pJJsHT2U1h5VhLMrcB/Yq+817cLWt8PI8zxRrIDUQKBgQDsDiEA3pj8m9PS3IKf
Yba5l4TztE62aHwJiQ1gLlVXpPk6EdECDHFZWnMQlre/ZT9e0hecd8P3aaeMs8Vi
zjOrf2J7nbZIlTgWHWeC48K9eUd7l/+UotvsmJqeHmTo3ZIsKyS1LOE7cUodyfCL
tQYfzEm4qrs11vRaciI3Zun8GwKBgQDBx3RoykYzRb9VZogHcLsdP/pmpzdJ3pb0
9viZr+kb8rpWBq3R0qB2iv+0TqV4F4B/XfSMI1eUbTgteafofQ70i0h7ovcypgRz
UUORho6Hrd58Znbxd/Q9CW8KLFiTps+moKaw2S2dwu9CME9dlBOL6/VIz7Sgn4O+
XMRf+yUvJwKBgQCs5YRi4KfpjjFOZtj96FIwCb0Fy3FDxa/kRBAZ/JXhxiIN2HLg
L0Duk4NoCRy5AW2zA+rrXgWZODfSpPHUdvf9iyYVKOUUsMcN26evhSdkJGqpKiG3
Oroex3+ohNaggXnJBCi00xR9t3Lz8q9PhN3heH4e1l6dBr6faK2LKsQDNQKBgDQT
Mclncm4c9Eoy/6NgPCikJNqpXUZQtyilpjFHANIt7L1plhSpEc5JlGYULIuVZUbV
LP7sEIEmyM4Pv3vO/9HgDF6NcPj/fHqxAAN/sZXst7men6BMqCou+tQ1Dqi/T1Zs
Hd+wvX2EAWA8M1fmj0ou4v/qMZRoybLCo1NX3qpJAoGAZ99dvWIPQerXdRpeN+za
rtLOfO5gWjdD6qvmGUZGBn9YPwC7pcDTSwGBuJzqlrKxUdLBYK2RMqQ5jEBPJugC
ZJD/BpVIQlyHFH2rsVBxFJetT2YreniKyt1Onqrps6DwTyGZd0MtzHhUj+K+a7JZ
PGEiRH+A0BGHG1fWlRKD/4A=
-----END PRIVATE KEY-----";

			using (EDIDataRegistry.Instance.CWSupportLoginTokenPrivateKey.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Encoding.UTF8.GetBytes(privateKey)))
			{
				supportTokenItem.PerformClick();

				var token = SafeClipboard.GetText();
				var tokenValidateResult = CWSupportLoginToken.Validate(token);
				Assert("token is valid", tokenValidateResult.IsValid);
			}

			var rsaPrivateKey = @"-----BEGIN RSA PRIVATE KEY-----
MIIEpAIBAAKCAQEA3oQ7NSGjvVf9EOb5t7QkUjIiT5wFYLrQR6zGw504/hJ6TQ9e
Z6iZgDk2Urp+nLtxn1RFyPXIBFDaT7dzRB4PpxdfIw0aKqXkLJYDt2IV5yvQHUBl
ewnCoyAafxfYNNHq+gjXqkL2eVAeSbZOVII7QOHjb1wK7N26dzqseESqqelNZnFT
0qZvcj09oJh/3KrjvFhN3eiu3K5eN+i2b2V557m1uAAZG6twRV+T/aKCS36KJ+WN
ptxO5av4E7yqd7VrHsQQBM2QkkiyJJDeTkQoTjRoQbO3HBH9EgrZi/Y9hPM2re58
jZmwaF6y3sUDyAoh6XPS/RNGtalp9pcMhpvXzQIDAQABAoIBAHnq8ZXW7KQdxaax
JzmkFhKDLZF45uls8hmTbQRY7JUpAqGePheFpTVAI6eITz8I5ORrhCDrqb/TnQn4
dctovSLMB3BpCWE8q5xSRY8AywdyVgDw+6lCW8aNHHduVSP7sEd2+NNtBSbK4w14
OcrL4DNYkQi+4a2Of1A8casj1G5rRBs9KysCcqxiDMl6OBcCm4fgihywuA/4ZmkM
/oWH+36DKREME611wPpGE6t3MKRo/vtvtn37yNjYCPbR6CTZc0wR4j2y8dqJHfo1
dE2jUbsX1CfPdy0rab5KZkCi8V7HqFHWK0tFLnifAl3UQQL7FQYTuAnoNAKQSHSM
tMfFUAECgYEA+OtuBfuVfpUYdyPFt01wMqrg+s2UqR8MFG+tvKNQl8jdsm4poYA0
2FgOCkICcd31VEepj3cIsURyjtnCCAxiLBdFSKqkzmLoltL+cim0C6VchEfuFKD1
Og/695EZhfAVWKc34u1igfBiIbbpQFGdoMIVC2YsSnEUAreZCKGf7M0CgYEA5NiK
ZufSpaS7MiTTdOjPu80x9MbY77Ej2px9Mi+7VOKODKtNQ+T4gEHoWi78ZwmvZkRI
THRsTkpExszLVOO2xUcqflnO/Tvfk1MHyCVr0FvHRTGck7RUAe8C7pdp66udvP4V
U5sFom+QPKckHsfPxfUz9cajeKQHbCxynPGLlwECgYAmbgKQfeT4pAAg9K7ju9rk
l6kgT3jyG078ILnL89LPfD1t/MAEpQyDkiSoxMQn3EKx/lZZReFbrNua3lescmz9
raIOs/m5u195WZ3a9kFLwv3jlk9Vc/woOKtgaVBtc0F4bGiealPZB8m+tsSQH4mc
dhpVpjKUU5zpRnsj8AAGyQKBgQDTX2Muu49oaihxn2wk/ujpKRWsZoJ3mmFoicRl
t4rLU9sqvorGXFZfeQDZU67UqTB2QDmbTKnBAn5WNVDV3uKxgxMv01oJsuTGGhoE
9vOHZ04+jh12nEg/5PkLUZWHFWPD7dfa+kQRDgTm0Obe+2XhDaPprojNZIxqkP9o
EP8DAQKBgQDRYZxA5QsQsE+A1QS4AxBg1EAh6mGTmsDU8CkufpySksPrkP7IKJny
Zsfg1Tms+daTsr1N93k3GEoN/dS4fYJXdccTQyfTVXWjsn6xhhvG2OhjVY2ocZpW
vdfMQ+l5r3kJ52LTTXYe8vnrTfsPdtp2gY+dw5e3uVEl8kUJlj/xww==
-----END RSA PRIVATE KEY-----";

			using (EDIDataRegistry.Instance.CWSupportLoginTokenPrivateKey.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Encoding.UTF8.GetBytes(rsaPrivateKey)))
			{
				supportTokenItem.PerformClick();

				var token = SafeClipboard.GetText();
				var tokenValidateResult = CWSupportLoginToken.Validate(token);
				Assert("token is valid", tokenValidateResult.IsValid);
			}
		}

		[TestDate]
		[DeveloperOnlyTest]
		public void TestSupportTokenForInternalSystem()
		{
			var currentDate = DateTime.UtcNow;
			TestDateAttribute.Date = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, currentDate.Hour, currentDate.Minute, currentDate.Second);
			_ = ClientOverride.Instance;

			var supportTokenItem = GetClientSpecificHelpMenuItem("Support Token");

			AssertEquals(true, supportTokenItem.ShowShortcutKeys);
			AssertEquals((Keys)Shortcut.CtrlShiftT, supportTokenItem.ShortcutKeys);

			using (EDIDataRegistry.Instance.CWSupportLoginTokenPrivateKey.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Encoding.UTF8.GetBytes(PrivateKeyForTest)))
			{
				supportTokenItem.PerformClick();

				var token = SafeClipboard.GetText();
				var cert = new X509Certificate2(Encoding.UTF8.GetBytes(CertificateStringForTest));
				var securityToken = JwtSecurity.VerifySignedJwt(cert.GetRSAPublicKey(), token);
				var jwtToken = (JwtSecurityToken)securityToken;
				AssertNotNull(jwtToken);
				AssertEquals(Env.CurrentUser.Initials, jwtToken.Subject);
				AssertEquals(Env.CurrentUser.FullName, jwtToken.Claims.FirstOrDefault(claim => claim.Type == "name")?.Value);
				AssertNotNull(jwtToken.Payload.Jti);

				string[] expectedUserRoles = { "cw1inttest", "cw1intprod", "diaginttest", "diagintprod" };
				var userRoles = jwtToken.Claims.Where(c => c.Type == "roles").Select(claim => claim.Value).ToArray();
				AssertContainsExactElementsInAnyOrder(expectedUserRoles, userRoles);
			}
		}

		public void TestRatesAdminMenuItem_Success()
		{
			_ = ClientOverride.Instance;

			var item = GetClientSpecificHelpMenuItem("Rates Service Admin");

			using (ObjectFactory.Substitute(MockTokenProvider("token_goes_here", null)))
			{
				item.PerformClick();
			}

			AssertStartsWith("Token URL launched", "http://wiseratesadmin.wtg.zone?token=token_goes_here", WebUrlLauncher.LastUrlLaunched);
		}

		public void TestRatesAdminMenuItem_TokenFail()
		{
			_ = ClientOverride.Instance;

			var item = GetClientSpecificHelpMenuItem("Rates Service Admin");

			using (ObjectFactory.Substitute(MockTokenProvider(null, "token failed")))
			{
				item.PerformClick();
			}

			AssertContains("token failed", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestRatesAdminMenuItem_RegistryChange()
		{
			_ = ClientOverride.Instance;

			var item = GetClientSpecificHelpMenuItem("Rates Service Admin");

			using (ObjectFactory.Substitute(MockTokenProvider("token_goes_here", null)))
			using (RatingDataRegistry.Instance.RatesServiceAdminInterfaceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://elsewhere"))
			{
				item.PerformClick();
			}

			AssertStartsWith("Token URL launched", "http://elsewhere?token=token_goes_here", WebUrlLauncher.LastUrlLaunched);
		}

		public void TestRatesAdminMenuItem_LoginInfoShouldHaveSupportType()
		{
			LoginInfo testLoginInfo = null;

			var authTokenProviderMock = new Mock<IAuthTokenProvider>();
			_ = authTokenProviderMock
				.Setup(m => m.GetToken(
					It.IsAny<string>(),
					It.IsAny<int>(),
					It.Is<Action<LoginInfo>>(x => x != null),
					It.IsAny<CancellationToken>(),
					It.IsAny<bool>()))
				.Callback((string correlationID, int period, Action<LoginInfo> overrideLoginInfo, CancellationToken ct, bool _) =>
				{
					testLoginInfo = new LoginInfo
					{
						Type = LoginType.CW1Client,
						Password = null
					};
					overrideLoginInfo(testLoginInfo);
				})
				.Returns(("tokenString", null));

			_ = ClientOverride.Instance;
			var item = GetClientSpecificHelpMenuItem("Rates Service Admin");

			using (ObjectFactory.Substitute(authTokenProviderMock.Object))
			using (EDIDataRegistry.Instance.CWSupportLoginTokenPrivateKey.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Encoding.UTF8.GetBytes(PrivateKeyForTest)))
			{
				item.PerformClick();
			}

			AssertStartsWith("Token URL launched", "http://wiseratesadmin.wtg.zone?token=tokenString", WebUrlLauncher.LastUrlLaunched);
			AssertEquals(LoginType.CW1Support, testLoginInfo.Type);
			Assert(!string.IsNullOrEmpty(testLoginInfo.Password));
		}

		#region TestInitializeClientOverride

		public void TestInitialiseAndUninitialiseCoreAddsChildToParentTableMapping()
		{
			var clientHook = ClientHookLoader.Instance.ClientHook;
			AssertEquals("ClientHook needs to be initialised", true, clientHook.IsInitialised);
			AssertEquals("Child table must be mapped to its parent table", "SupportIncident" , clientHook.GetParentTableName("IncidentRequest"));

			clientHook.Uninitialise();

			AssertEquals("Child table mapping must be removed after uninitialise", null, clientHook.GetParentTableName("IncidentRequest"));

			// Reinitialise for other tests
			clientHook.Initialise();
		}

		#endregion

		#region Helpers

		ToolStripMenuItem GetClientSpecificHelpMenuItem(string label)
		{
			var items = ((ArrayList)ObjectFactory.Get("ClientSpecificHelpMenuItems")).OfType<ToolStripMenuItem>().ToArray();
			var item = items.Single(i => i.Text == label);
			return item;
		}

		IAuthTokenProvider MockTokenProvider(string resultToken, string resultValidationMessage)
		{
			var tokenMock = new Mock<IAuthTokenProvider>();
			tokenMock.Setup(m => m.GetToken(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<Action<LoginInfo>>(), It.IsAny<CancellationToken>(), It.IsAny<bool>()))
				.Returns((resultToken, resultValidationMessage));

			return tokenMock.Object;
		}

		const string PrivateKeyForTest = @"-----BEGIN PRIVATE KEY-----
MIIEvQIBADANBgkqhkiG9w0BAQEFAASCBKcwggSjAgEAAoIBAQDnv8pQZ3Zgettj
WJWJJfAYJZ7xSy/HE8+w0DkOXOMfWkDLo4lvl1ILxI8ievnFAK/B/frSJ/deuRII
mS8mXH1GFweCmdBB8uK69Ugd9tlAcxEvv46rgA/N01eSdcExJ1SONz15IGOnFGEM
2p8vEdwmVLMZlk0kKMueOcveq0rlYE2CO7uD5k+58woFF8ByI2UAlBYwr8IsUK22
WUf3C4UFHwhhjqivN98nWehx3TCV7mVw2DyKGCwZKiqAxxHfZlTeX4w0TyoNwuKV
XY5pbRqWi7Yret3emrxY61lOAYYw4dpMQ4EBeqBdlBvOC6JcR7qKOgsGDCbOSCse
rDomAZ59AgMBAAECggEAC5sz1H4HVC1NonaTxUgZ86OislPrzc0PXZFit3ZDG6qx
9wuMJ64M5QZQOCUE2u8TXk72yk1HxWiAX4ozGHk7qZB1XFQ8Cql8MxUd/6jWrYnL
EQ3yD7hnUjgPjogI3Qoqtmqhe1j6FKqKcmcv317GHJdTrD2LmdAp47/iQWxpC6lZ
d0g5F6lmdztkZdQhm7njhxExJ+CdU68rnJeks3GPYQzCZA4V7tkxP98Y5XaBWcdT
/dK1vcXS1SrInEDZvli/cXIc8sAf6LqDB9yqdcPw++oI+aUFLGmOnImCouPx4jA5
MXLdf0UXsLJbPgteBCtTxBeWene76Ym3CkyS3n0TMwKBgQDvzsABwAx7rXpIVwbL
ff3a11T2JQVHG8Ts1TvWtxFYBcLt0QThTMve4VENDQUk/il3Ltn+lLgjx+5Vyyaa
3UDYuZmDnbrlq4DfePZa7VNWj6TBFNRChMzxZG3MWUHP7qda8NcFM6pEZJpyFSeQ
8ij5Wu7PuywW5D3rkoc0sDOyKwKBgQD3Zb6KQr1wvCHy0GH86FSNYB0P3mS45KWM
JU3AdlwndtCVlKyDuhtlcaxK2OlZZiLU0aKnvLR+y77OPu5YawcEdGrZmwGerVYd
fzKDzSJuEn8MM13+GcAJlFlsoklUaeu3bHmAGKUw3ph1B6Ix0c2/HHFkjHelbT9b
t2Kc8GGl9wKBgEEzqrsPF5XNDjF7EArmH86Pu7cNS8kQwNNQCuwPbHTNZDm7GiOT
+N6JzrrIrnxnaqjQIU956jM4WhIToVR8EfSbSiUiDr4BipG4VutUGdOwTLB+1FOd
vgdoMf5cymsZzYEJeL0eVg4weFnKbK6ZWRCra8EpeAxlVHyno4Fs4zFvAoGBAOrQ
y3V3u09RgfdyCk9+RSKa43q4X2mOvAK1NYND1FwwzfHr14KAFpjGt/2ivHl6E/1j
rLsAxWDECirAWIHbtCFqTjCUi4kMhPwiStQG1HMdYzE1YDVaQ4fUIryVnHxevLiw
YPJQchpcbOBHio82z85hNM928+k0NDrdaOAE2OopAoGAaMsqGcNzwXZmYI+GNH6Z
zZPH0t4Cg+m3L1HliI+8FeMxwL6sWRxxL1qGG73xj7JGUjxCIPX52oGCoUob/x1Y
VE5Exfnv+O555Sy5hksOvTepn1fiZWVoDQc3KntZegRFKGEuozqQ1eKCkLieQ0/m
k8VXc3uhpFenM2ZGK0TedMY=
-----END PRIVATE KEY-----";

		const string CertificateStringForTest = @"-----BEGIN CERTIFICATE-----
MIID2jCCAsKgAwIBAgIQR0RemLM7Lo27g7CJGUVSRzANBgkqhkiG9w0BAQsFADBx
MQswCQYDVQQGEwJDTjERMA8GA1UECgwIamF5d3RnQ0ExHDAaBgNVBAsME0lkZW50
aXR5QW5kU2VjdXJpdHkxEDAOBgNVBAgMB05hbmppbmcxDTALBgNVBAMMBE5KRzEx
EDAOBgNVBAcMB1dURyBOSkcwHhcNMjMwNTA4MDYxNDAzWhcNMjQwNTA3MDcxNDAz
WjBjMQswCQYDVQQGEwJBVTEMMAoGA1UECAwDU1lEMQwwCgYDVQQHDANTWUQxDDAK
BgNVBAoMA1dURzEMMAoGA1UECwwDV1RHMRwwGgYDVQQDDBN3aXNldGVjaC5nbG9i
YWwuY29tMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEA57/KUGd2YHrb
Y1iViSXwGCWe8UsvxxPPsNA5DlzjH1pAy6OJb5dSC8SPInr5xQCvwf360if3XrkS
CJkvJlx9RhcHgpnQQfLiuvVIHfbZQHMRL7+Oq4APzdNXknXBMSdUjjc9eSBjpxRh
DNqfLxHcJlSzGZZNJCjLnjnL3qtK5WBNgju7g+ZPufMKBRfAciNlAJQWMK/CLFCt
tllH9wuFBR8IYY6orzffJ1nocd0wle5lcNg8ihgsGSoqgMcR32ZU3l+MNE8qDcLi
lV2OaW0alou2K3rd3pq8WOtZTgGGMOHaTEOBAXqgXZQbzguiXEe6ijoLBgwmzkgr
Hqw6JgGefQIDAQABo3wwejAJBgNVHRMEAjAAMB8GA1UdIwQYMBaAFMUZFOQcnWCL
2XW0xLCOqDhZ7wLiMB0GA1UdDgQWBBRgVXKxahXMIirYYm0846gHnJeY5jAOBgNV
HQ8BAf8EBAMCBaAwHQYDVR0lBBYwFAYIKwYBBQUHAwEGCCsGAQUFBwMCMA0GCSqG
SIb3DQEBCwUAA4IBAQBcRs+X0iH/K5dOTQkZ5/v13huLOXb3QFExTJW2+tKmRYe1
e3CoQVE6LQJ+cCQ+Tl6UmbyeyTIqEuFpTmm6Yhl9agu41tlgiobP1+YQ/VMjasgh
Vhgr34KA09iVzpLsIlROdNW5Q5rfjRh1WuBAEPcABKKNFgaqVvS2BKzd/a6aXaId
Zu+YuRV232OXOUZP07DkaLhax6wfSf+tfkNLQLvoVNcJmcF6mNgzg2HULuvia77u
HPpykW02IbnSAr3jDVGHezVNLctFrpHpCWRlTUMulW56xc74ZiONSf+N/2WZJo0x
wNakFhJRZGZJpKCx8xrIO4D9y7jt0WGP7ZCvcXeQ
-----END CERTIFICATE-----";

		#endregion
	}
}
