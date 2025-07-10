using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Windows.Forms;
using AuthenticationService.Client.Models;
using CargoWise.Application;
using CargoWise.BrandManager;
using CargoWise.Common;
using CargoWise.Database.Abstractions.Extensions;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Client.EDI;
using Enterprise.Client.EDI.ApplicationLogging;
using Enterprise.Client.EDI.Billing.Business.USSalesTax;
using Enterprise.Client.EDI.Business;
using Enterprise.Client.EDI.ClientConfiguration;
using Enterprise.Client.EDI.CommissionManagement.Business;
using Enterprise.Client.EDI.CommissionManagement.GUI;
using Enterprise.Client.EDI.CommissionManagement.Module;
using Enterprise.Client.EDI.DbRestoreKey;
using Enterprise.Client.EDI.DeviceManagement.ServiceTasks;
using Enterprise.Client.EDI.DocManager.Business;
using Enterprise.Client.EDI.EndpointManagement.Module;
using Enterprise.Client.EDI.FeatureControl.Business;
using Enterprise.Client.EDI.FeatureControl.Module;
using Enterprise.Client.EDI.IdentityApplication;
using Enterprise.Client.EDI.IdentityCertificate;
using Enterprise.Client.EDI.IdentityTenant;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.IncidentManager.Module;
using Enterprise.Client.EDI.IssueManager.Module;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Module;
using Enterprise.Client.EDI.Mail.Business;
using Enterprise.Client.EDI.Mail.Module;
using Enterprise.Client.EDI.MarketingManager.Business;
using Enterprise.Client.EDI.MarketingManager.GUI;
using Enterprise.Client.EDI.MarketingManager.Module;
using Enterprise.Client.EDI.MasterFiles;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.MasterFiles.GUI;
using Enterprise.Client.EDI.MasterFiles.Module;
using Enterprise.Client.EDI.MasterFiles.ProcessManagement;
using Enterprise.Client.EDI.Modules;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Client.EDI.ReleaseBuilds.Module;
using Enterprise.Client.EDI.Security;
using Enterprise.Client.EDI.Services;
using Enterprise.Client.EDI.ServiceTasks;
using Enterprise.Client.EDI.Telematics;
using Enterprise.Client.EDI.TokenAuthenticationOnBoarding;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.Client.EDI.UserManagement.GUI;
using Enterprise.Client.EDI.UserManagement.Module;
using Enterprise.Client.EDI.Web.Login;
using Enterprise.Core.Environment;
using Enterprise.Core.Modules;
using Enterprise.CustomerService.Business;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.EConversation.Business;
using Enterprise.Environment;
using Enterprise.Integration.Rating;
using Enterprise.MailManager.Business;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.Module;
using Enterprise.Rating.Business;
using Enterprise.Registry.Business;
using Enterprise.Workflow.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.DocumentScanning;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.GUI.Login;
using WTG.IdentitySecurity;
using ZClientEDI.Business.EConversation;
using ZClientEDI.GUI.Rating;
using Res = ZClientEDI.Res;
using ResString = ZClientEDI.ResString;

[assembly: AssemblyDataProvider(typeof(Enterprise.Client.ClientOverride.ProfessionalServicesQuoteData), IncidentConstants.ProfessionalServicesQuoteDocManagerCode, ClientSpecificCode = Clients.EDI)]
[assembly: AssemblyDataProvider(typeof(Enterprise.Client.ClientOverride.IncidentManagementGroupData), IncidentConstants.IncidentManagementGroupDocManagerCode, ClientSpecificCode = Clients.EDI)]
[assembly: AssemblyDataProvider(typeof(Enterprise.Client.ClientOverride.FeatureControlHeaderData), FeatureControlHeaderSchema.Constants.Prefix, ClientSpecificCode = Clients.EDI)]
[assembly: AssemblyDataProvider(typeof(Enterprise.Client.ClientOverride.IncidentDiagnosticCriteriaData), IncidentDiagnosticCriteriaSchema.Constants.Prefix, ClientSpecificCode = Clients.EDI)]
[assembly: AssemblyDataProvider(typeof(Enterprise.Client.ClientOverride.EdiUserAgreementData), EdiUserAgreementSchema.Constants.Prefix, ClientSpecificCode = Clients.EDI)]

namespace Enterprise.Client
{
	public class ClientOverride : ClientHook
	{
		#region Initialise / Uninitialise

		protected ClientOverride()
		{
		}

		public static ClientOverride Instance => new ClientOverride();

		protected override void InitialiseCore()
		{
			CMRMessageResponseProcessor.SendReportToWebUsers.Value = true;
			OrgSecurityProfileUpdateForm.CustomerSelfManagementEnabled.Value = true;
			WebSecurityUserControl.CustomerSelfManagementEnabled.Value = true;
			OrgSecurityProfileControl.CustomerSelfManagementEnabled.Value = true;

			EDIAllWebSecurityRights.RegisterThisSubTypeOverride();
			EDIARInvoiceDocManagerInfo.RegisterThisSubTypeOverride();
			EdiCommissionAgreementControl.RegisterThisSubTypeOverride();
			EDICommissionLookups.RegisterThisSubTypeOverride();
			EdiDisableStaffCommissionAgreementsAction.RegisterThisSubTypeOverride();
			EDIDepartmentChooser.RegisterThisSubTypeOverride();
			EDIFreightWrapperDecider.RegisterThisSubTypeOverride();
			EDIGlbTimeAllocationLookups.RegisterThisSubTypeOverride();
			EDIJobInvoicingConsumerTypes.RegisterThisSubTypeOverride();
			EDIMailDBItemTemplateLookups.RegisterThisSubTypeOverride();
			EDINonJobRelatedTransactionCommissionCreator.RegisterThisSubTypeOverride();
			EDIOpportunityManagementValueAnalysisControl.RegisterThisSubTypeOverride();
			EDIPredefinedNoteTypes.RegisterThisSubTypeOverride();
			EDIRelatedActivityLinkLookups.RegisterThisSubTypeOverride();
			EDITransactionHeaderConsolOrJobTypeXmlMapping.RegisterThisSubTypeOverride();
			EDIWebReportSecurityRightsList.RegisterThisSubTypeOverride();
			EDIWebSecurityRightsList.RegisterThisSubTypeOverride();
			EDIWorkflowDescriptors.RegisterThisSubTypeOverride();
			EDISalesClientSummaryUserControl.RegisterThisSubTypeOverride();
			EDIOpportunityManagementControl.RegisterThisSubTypeOverride();
			EDIOrgCodeLists.RegisterThisSubTypeOverride();
			EDISalesRelationTypeList.RegisterThisSubTypeOverride();
			EDISalesRelationDirectionRuleCollection.RegisterThisSubTypeOverride();
			EDIOpportunityRegistryCaption.RegisterThisSubTypeOverride();

			RegisterObjectInObjectFactoryList("OrganisationMergeActions", new OrganisationMergeSyncWithBorderWiseAction());
			RegisterObjectInObjectFactoryList("IUSSalesTaxCalculator_ClientSpecific", new AvalaraUSSalesTaxCalculatorFactory());

			var newHelpMenuItemsList = (ArrayList)ObjectFactory.Get("ClientSpecificHelpMenuItems");
			foreach (var menuItem in GetNewHelpMenuItems())
			{
				newHelpMenuItemsList.Add(menuItem);
			}

			GetStatusChangeRespondersList().Insert(0, new CompetencyRequirementsTaskStatusChangeResponder());

			DocumentScanning.GUI.ChartFXLicence.CreateChartFXLicenceRegistryItemIfRequired();

			TelematicsServiceTaskHook.Hook();
			TelematicsExtension.RegisterEdiProcessors();

			EnterpriseUrlHandlerService.RegisterUrlHandler(ShortCodeUrlHandler.Instance);

			AddChildToParentTableMapping("IncidentRequest", "SupportIncident");
		}

		protected override void UninitialiseCore()
		{
			RemoveObjectFromObjectFactoryList<OrganisationMergeSyncWithBorderWiseAction>("OrganisationMergeActions");
			RemoveObjectFromObjectFactoryList<AvalaraUSSalesTaxCalculatorFactory>("IUSSalesTaxCalculator_ClientSpecific");
			var newHelpMenuItemsList = (ArrayList)ObjectFactory.Get("ClientSpecificHelpMenuItems");
			newHelpMenuItemsList.Clear();
			var statusChangeResponderList = GetStatusChangeRespondersList();
			var responderToRemove = statusChangeResponderList.OfType<CompetencyRequirementsTaskStatusChangeResponder>().SingleOrDefault();
			if (responderToRemove != null)
			{
				statusChangeResponderList.Remove(responderToRemove);
			}

			RemoveChildToParentTableMapping("IncidentRequest");
		}

		static void RegisterObjectInObjectFactoryList(string listName, object obj)
		{
			var list = (ArrayList)ObjectFactory.Get(listName);
			list.Add(obj);
		}

		static void RemoveObjectFromObjectFactoryList<T>(string listName)
		{
			var list = (ArrayList)ObjectFactory.Get(listName);
			{
				var item = list.OfType<T>().FirstOrDefault();
				if (item != null)
				{
					list.Remove(item);
				}
			}
		}

		static ArrayList GetStatusChangeRespondersList() => (ArrayList)ObjectFactory.Get("TaskStatusChangeResponders");

		#endregion

		#region Client Code / Name

		public override Clients Client => Clients.EDI;

		public override string ClientDisplayName => EDIConstants.ClientDisplayName;

		public override string HelpWebPage => string.Empty;

		#endregion

		#region Registry

		public override IRegistryItemSet AdditionalRegistryItemSet => EDIDataRegistry.Instance;

		#endregion

		#region Controllers

		protected override ControllerOverrides GetControllerOverrides()
		{
			var controllerOverrides = new ControllerOverrides();
			AddControllerOverride(controllerOverrides, ControllerIDs.AdministrationPanel, typeof(EDIAdministrationPanelControllerOverride));
			AddControllerOverride(controllerOverrides, ControllerIDs.Organisation, typeof(EDIOrganisationControllerOverride));
			AddControllerOverride(controllerOverrides, ControllerIDs.GlbCompanyCampaign, typeof(EDICRMGlbCompanyCampaignController));
			AddControllerOverride(controllerOverrides, ControllerIDs.Opportunity, typeof(EDIOrgOpportunityController));
			AddControllerOverride(controllerOverrides, ControllerIDs.SalesEnquiry, typeof(EDISalesInquiryController));
			AddControllerOverride(controllerOverrides, ControllerIDs.GlbStaff, typeof(EDIGlbStaffControllerOverride));
			AddControllerOverride(controllerOverrides, ControllerIDs.WorkItem, typeof(EDIWorkItemController));
			AddControllerOverride(controllerOverrides, ControllerIDs.Project, typeof(EDIProjectController));
			AddControllerOverride(controllerOverrides, ControllerIDs.AccChargeCode, typeof(EDIAccChargeCodeController));
			AddControllerOverride(controllerOverrides, ControllerIDs.OrgContacts, typeof(EDIOrgContactsController));
			return controllerOverrides;
		}

		void AddControllerOverride(ControllerOverrides controllerOverrides, ControllerID id, Type controllerType)
		{
			var info = new ClientOverrideControllerInfo(new ClientOverrideControllerID(id), controllerType.Assembly.FullName, controllerType.FullName);
			controllerOverrides.AddControllerOverride(info);
		}

		protected override ControllerInfo[] NewClientControllersCore
		{
			get
			{
				var controllers = new List<ControllerInfo>();

				controllers.Add(new ControllerInfo(ClientControllerRegistration.IssueManager, typeof(IssueManagerController)));
				controllers.Add(new ControllerInfo(ClientControllerRegistration.UserAgreements, typeof(EdiUserAgreementController)));
				controllers.Add(new ControllerInfo(ClientControllerRegistration.UserAgreementAcceptances, typeof(EdiUserAgreementAcceptanceLogController)));
				controllers.Add(new ControllerInfo(ClientControllerRegistration.EdiUserAgreementAssignment, typeof(EdiAgreementAssignmentPluginController)));
				controllers.Add(new ControllerInfo(ClientControllerRegistration.ObsoleteNonGenericWorkItemForOldHyperlinksOnly, typeof(ObsoleteNonGenericWorkItemControllerForOldHyperlinksOnly)));
				controllers.Add(new ControllerInfo(ClientControllerRegistration.EdiIncidentRequest, typeof(EdiIncidentRequestController)));
				controllers.Add(new ControllerInfo(ClientControllerRegistration.SupportIncident, typeof(SupportIncidentController)));
				controllers.Add(new ControllerInfo(ClientControllerRegistration.IncidentEConversationPlugIn, typeof(EDI.IncidentManager.GUI.IncidentConversationPluginController)));
				controllers.Add(new ControllerInfo(ClientControllerRegistration.IncidentManagementEConversationPlugin, typeof(EDI.IncidentManager.GUI.IncidentManagementEConversationPluginController)));
				controllers.Add(new ControllerInfo(ClientControllerRegistration.DbRestoreKey, typeof(DbRestoreKeyController)));
				controllers.Add(new ControllerInfo(ClientControllerRegistration.IncidentManagementGroup, typeof(IncidentManagementGroupController)));
				controllers.Add(new ControllerInfo(ClientControllerRegistration.IncidentTriage, typeof(IncidentTriageController)));
				controllers.Add(new ControllerInfo(ClientControllerRegistration.IncidentTriageChecklistItem, typeof(IncidentTriageChecklistItemController)));
				controllers.Add(new ControllerInfo(ClientControllerRegistration.IncidentDiagnosticCriteria, typeof(IncidentDiagnosticCriteriaController)));
				controllers.Add(new ControllerInfo(ClientControllerRegistration.InvestigationItem, typeof(InvestigationItemController)));
				controllers.Add(new ControllerInfo(ClientControllerRegistration.FeatureControl, typeof(FeatureControlController)));
				controllers.Add(new ControllerInfo(ClientControllerRegistration.FeatureSet, typeof(FeatureSetController)));
#if DEBUG
				controllers.Add(new ControllerInfo(ClientControllerRegistration.GlbReleaseNote, typeof(GlbReleaseNoteController)));
#endif
				controllers.Add(new ControllerInfo(ClientControllerRegistration.ReleaseBuild, typeof(ReleaseBuildController)));
				controllers.Add(new ControllerInfo(ClientControllerRegistration.LicenceEnterprise, typeof(LicenceEnterpriseController)));
				controllers.Add(new ControllerInfo(ClientControllerRegistration.LicenseKey, typeof(LicenceKeyController)));
				controllers.Add(new ControllerInfo(ClientControllerRegistration.Organisations, typeof(EDIOrganisationControllerOverride)));
				controllers.Add(new ControllerInfo(ClientControllerRegistration.LicenceDatabase, typeof(LicenceDatabaseController)));
				controllers.Add(new ControllerInfo(ClientControllerRegistration.EdiTrustedMessagingConfig, typeof(EdiTrustedMessagingConfigController)));
				controllers.Add(new ControllerInfo(ClientControllerRegistration.EdiTrustedSystem, typeof(EdiTrustedSystemController)));
				controllers.Add(new ControllerInfo(ClientControllerRegistration.EdiIdentityCertificate, typeof(EdiIdentityCertificateController)));

				controllers.Add(new ControllerInfo(ClientControllerRegistration.TokenAuthenticationOnBoarding, typeof(TokenAuthenticationOnBoardingController)));
				controllers.Add(new ControllerInfo(ClientControllerRegistration.EdiIdentityApplication, typeof(EdiIdentityApplicationController)));
				controllers.Add(new ControllerInfo(ClientControllerRegistration.EdiIdentityTenant, typeof(EdiIdentityTenantController)));
				controllers.Add(new ControllerInfo(ClientControllerRegistration.LicenceHeader, typeof(LicenceHeaderController)));
				controllers.Add(new ControllerInfo(ClientControllerRegistration.ProfessionalServicesQuote, typeof(ProfessionalServicesQuoteController)));
				controllers.Add(new ControllerInfo(ClientControllerRegistration.Project, typeof(ProjectController)));
				controllers.Add(new ControllerInfo(ClientControllerRegistration.WiseServicePartnerSurveyPlugIn, typeof(WiseServicePartnerSurveyPluginController)));
				controllers.Add(new ControllerInfo(ClientControllerRegistration.SupportIncidentClientOrgLicence, typeof(SupportIncidentClientOrgLicenceController)));
				controllers.Add(new ControllerInfo(ClientControllerRegistration.ProjectClientOrgLicence, typeof(ProjectClientOrgLicenceController)));
				controllers.Add(new ControllerInfo(ClientControllerRegistration.OpportunityRelatedProjects, typeof(OpportunityRelatedProjectsController)));
				controllers.Add(new ControllerInfo(ClientControllerRegistration.OpportunityClientOrgLicence, typeof(OpportunityClientOrgLicenceController)));
				controllers.Add(new ControllerInfo(ClientControllerRegistration.MonthlyUsageBilling, typeof(EDI.Billing.Module.MonthlyUsageBillingController)));
				controllers.Add(new ControllerInfo(ClientControllerRegistration.StlBilling, typeof(EDI.Billing.Module.StlBillingController)));
				controllers.Add(new ControllerInfo(ClientControllerRegistration.EngineeringTask, typeof(EngineeringTaskController)));
				controllers.Add(new ControllerInfo(ClientControllerRegistration.PSQRelatedOpportunities, typeof(PSQRelatedOpportunitiesController)));
				controllers.Add(new ControllerInfo(ClientControllerRegistration.EDIOpportunityRelatedPSQs, typeof(EDIOpportunityRelatedPSQsController)));
				controllers.Add(new ControllerInfo(ClientControllerRegistration.MaintenanceBilling, typeof(EDI.Billing.Module.MaintenanceBillingController)));
				controllers.Add(new ControllerInfo(ClientControllerRegistration.BillingPrices, typeof(EDI.Billing.Module.BillingPrices.BillingPricesController)));
				controllers.Add(new ControllerInfo(ClientControllerRegistration.ChargeCodeCommissionConfiguration, typeof(ChargeCodeCommissionConfigurationPluginController)));

				// Device Management
				controllers.Add(new ControllerInfo(ClientControllerRegistration.ClientDevice, typeof(EDI.DeviceManagement.Module.ClientDeviceHeaderController)));
				controllers.Add(new ControllerInfo(ClientControllerRegistration.ClientDeviceTemplate, typeof(EDI.DeviceManagement.Module.ClientDeviceHeaderTemplateController)));

				controllers.Add(new ControllerInfo(ClientControllerRegistration.EDIOpportunityValueAnalysis, typeof(EDIOpportunityValueAnalysisController)));

				// Product and Licence Management
				controllers.Add(new ControllerInfo(ClientControllerRegistration.LicenceDatabaseRegistration, typeof(LicenceDatabaseRegistrationController)));
				controllers.Add(new ControllerInfo(ClientControllerRegistration.SystemUserAccounts, typeof(SystemUserAccountsController)));

				// Application Logging
				controllers.Add(new ControllerInfo(ClientControllerRegistration.ApplicationLogger, typeof(ApplicationLoggerController)));
				controllers.Add(new ControllerInfo(ClientControllerRegistration.ApplicationActiveLogger, typeof(ApplicationActiveLoggerController)));

				return controllers.ToArray();
			}
		}

		#endregion

		#region Modules

		protected override ModuleOverrides GetModuleOverrides()
		{
			var moduleOverrides = new ModuleOverrides();
			AddModuleOverride(moduleOverrides, ModuleIDs.AdministrationPanel, typeof(EDIAdministrationPanelModule));
			AddModuleOverride(moduleOverrides, ModuleIDs.Organisation, typeof(EDIOrganisationModule), new TableRegistrationInfo(OrgHeaderSchema.Constants.TableName));
			AddModuleOverride(moduleOverrides, ModuleIDs.OrgContacts, typeof(EDIOrgContactsModule), new TableRegistrationInfo(OrgContactSchema.Constants.TableName));
			AddModuleOverride(moduleOverrides, ModuleIDs.ProcessTasks, typeof(EDIProcessTasksModule), new TableRegistrationInfo(ProcessTasksSchema.Constants.TableName));
			AddModuleOverride(moduleOverrides, ModuleIDs.GlbCompanyCampaign, typeof(EDICRMGlbCompanyCampaignModule), new TableRegistrationInfo(GlbCompanyCampaignSchema.Constants.TableName));
			AddModuleOverride(moduleOverrides, ModuleIDs.GlbCompanyCampaignContact, typeof(EDIGlbCompanyCampaignContactModule));
			AddModuleOverride(moduleOverrides, ModuleIDs.InternationalZone, typeof(EDIRefZoneHeaderModule), new TableRegistrationInfo(RefZoneHeaderSchema.Constants.TableName));
			AddModuleOverride(moduleOverrides, ModuleIDs.Opportunity, typeof(EDIOrgOpportunityModule), new TableRegistrationInfo(OrgOpportunitySchema.Constants.TableName));
			AddModuleOverride(moduleOverrides, ModuleIDs.Commission, typeof(EDICommissionManagementModule));
			AddModuleOverride(moduleOverrides, ModuleIDs.Messaging.EDIInterchange, typeof(EDI.Messaging.EDIEDIInterchangeModule), new TableRegistrationInfo(EDIInterchangeSchema.Constants.TableName));
			AddModuleOverride(moduleOverrides, ModuleIDs.Messaging.EDIMessage, typeof(EDI.Messaging.EDIEDIMessageModule), new TableRegistrationInfo(EDIMessageSchema.Constants.TableName));
			AddModuleOverride(moduleOverrides, ModuleIDs.WorkItem, typeof(EDIWorkItemModule), new TableRegistrationInfo(WorkItemSchema.Constants.TableName));
			AddModuleOverride(moduleOverrides, ModuleIDs.Project, typeof(EDIProjectModule), new TableRegistrationInfo(WorkProjectSchema.Constants.TableName));
			AddModuleOverride(moduleOverrides, ModuleIDs.GlbStaff, typeof(EdiStaffModule), new TableRegistrationInfo(GlbStaffSchema.Constants.TableName));
			AddModuleOverride(moduleOverrides, ModuleIDs.RefDocOrgCusCode, typeof(EDIRefDocOrgCusCodeModule));
			AddModuleOverride(moduleOverrides, ModuleIDs.StaffAssignments, typeof(EDIStaffAssignmentsModule), new TableRegistrationInfo(OrgStaffAssignmentsSchema.Constants.TableName));
			return moduleOverrides;
		}

		static void AddModuleOverride(ModuleOverrides overrides, ModuleIdentifier id, Type overrideType, TableRegistrationInfo moduleTable = null) => overrides.AddModuleOverride(new ClientOverrideModuleInfo(new ClientOverrideModuleIdentifier(id), overrideType.Assembly.FullName, overrideType.FullName, moduleTable));

		protected override NewClientModuleInfo[] NewClientModulesCore
		{
			get
			{
				if (newClientModulesRef == null || !newClientModulesRef.TryGetTarget(out var newClientModules))
				{
					var modules = new List<NewClientModuleInfo>(50);

					// Development
					modules.Add(new NewClientModuleInfo(ModuleTreeLoaderConstant.Category.Operations, ClientModuleRegistration.Section.Development, new ModuleInfo(ClientModuleRegistration.ProfessionalServicesQuote, typeof(ProfessionalServicesQuoteModule), new TableRegistrationInfo(IncidentMainSchema.Constants.TableName))));
					modules.Add(new NewClientModuleInfo(ModuleTreeLoaderConstant.Category.Operations, ClientModuleRegistration.Section.Development, new ModuleInfo(ClientModuleRegistration.IssueManager, typeof(IssueManagerModule), new TableRegistrationInfo(HelpErrorLogSchema.Constants.TableName))));
					modules.Add(new NewClientModuleInfo(ModuleTreeLoaderConstant.Category.Operations, ClientModuleRegistration.Section.Development, new ModuleInfo(ClientModuleRegistration.UserAgreements, typeof(EdiUserAgreementModule), new TableRegistrationInfo("UserAgreement"))));
					modules.Add(new NewClientModuleInfo(ModuleTreeLoaderConstant.Category.Operations, ClientModuleRegistration.Section.Development, new ModuleInfo(ClientModuleRegistration.UserAgreementAcceptances, typeof(EdiUserAgreementAcceptanceLogModule), new TableRegistrationInfo("UserAgreementAcceptanceLog"))));
#if DEBUG
					modules.Add(new NewClientModuleInfo(ModuleTreeLoaderConstant.Category.Operations, ClientModuleRegistration.Section.Development, new ModuleInfo(ClientModuleRegistration.GlbReleaseNote, typeof(GlbReleaseNoteModule), new TableRegistrationInfo(GlbReleaseNoteSchema.Constants.TableName))));
#endif
					modules.Add(new NewClientModuleInfo(ModuleTreeLoaderConstant.Category.Operations, ClientModuleRegistration.Section.Development, new ModuleInfo(ClientModuleRegistration.WorkItemsReports, typeof(WorkItemReports))));
					AddModuleInfoIf(modules, ModuleTreeLoaderConstant.Category.Operations, ClientModuleRegistration.Section.Development, ClientModuleRegistration.FeatureControl, typeof(FeatureControlModule), EDIDataRegistry.Instance.EnableFeatureControlModule.Value, new TableRegistrationInfo(FeatureControlHeaderSchema.Constants.TableName));
					AddModuleInfoIf(modules, ModuleTreeLoaderConstant.Category.Operations, ClientModuleRegistration.Section.Development, ClientModuleRegistration.FeatureSet, typeof(FeatureSetModule), EDIDataRegistry.Instance.EnableFeatureControlModule.Value, new TableRegistrationInfo(FeatureControlSetSchema.Constants.TableName));

					// Customer Service
					modules.Add(new NewClientModuleInfo(ModuleTreeLoaderConstant.Category.Operations, ClientModuleRegistration.Section.CustomerService, new ModuleInfo(ClientModuleRegistration.SupportIncident, typeof(SupportIncidentModule), new TableRegistrationInfo(IncidentMainSchema.Constants.TableName))));
					modules.Add(new NewClientModuleInfo(ModuleTreeLoaderConstant.Category.Operations, ClientModuleRegistration.Section.CustomerService, new ModuleInfo(ClientModuleRegistration.EDICustomerServiceEmails, typeof(CustomerServiceMailItemModule), new TableRegistrationInfo(MailDBItemsSchema.Constants.TableName))));
					modules.Add(new NewClientModuleInfo(ModuleTreeLoaderConstant.Category.Operations, ClientModuleRegistration.Section.CustomerService, new ModuleInfo(ClientModuleRegistration.DbRestoreKey, typeof(DbRestoreKeyModule))));
					modules.Add(new NewClientModuleInfo(ModuleTreeLoaderConstant.Category.Operations, ClientModuleRegistration.Section.CustomerService, new ModuleInfo(ClientModuleRegistration.ReleaseBuild, typeof(ReleaseBuildModule), new TableRegistrationInfo(ReleaseBuildSchema.Constants.TableName))));
					modules.Add(new NewClientModuleInfo(ModuleTreeLoaderConstant.Category.Operations, ClientModuleRegistration.Section.CustomerService, new ModuleInfo(ClientModuleRegistration.LicenceEnterprise, typeof(LicenceEnterpriseModule), new TableRegistrationInfo(LicenceEnterpriseSchema.Constants.TableName))));
					modules.Add(new NewClientModuleInfo(ModuleTreeLoaderConstant.Category.Operations, ClientModuleRegistration.Section.CustomerService, new ModuleInfo(ClientModuleRegistration.LicenseKey, typeof(LicenseKeyModule), new TableRegistrationInfo(LicenceHeaderSchema.Constants.TableName))));
					modules.Add(new NewClientModuleInfo(ModuleTreeLoaderConstant.Category.Operations, ClientModuleRegistration.Section.CustomerService, new ModuleInfo(ClientModuleRegistration.LicenceDatabase, typeof(LicenceDatabaseModule), new TableRegistrationInfo(LicenceDatabaseSchema.Constants.TableName))));
					modules.Add(new NewClientModuleInfo(ModuleTreeLoaderConstant.Category.Operations, ClientModuleRegistration.Section.CustomerService, new ModuleInfo(ClientModuleRegistration.EdiTrustedMessagingConfig, typeof(EdiTrustedMessagingConfigModule), new TableRegistrationInfo(EdiTrustedMessagingConfigSchema.Constants.TableName))));
					modules.Add(new NewClientModuleInfo(ModuleTreeLoaderConstant.Category.Operations, ClientModuleRegistration.Section.CustomerService, new ModuleInfo(ClientModuleRegistration.EdiTrustedSystem, typeof(EdiTrustedSystemModule), new TableRegistrationInfo(EdiTrustedSystemSchema.Constants.TableName))));
					modules.Add(new NewClientModuleInfo(ModuleTreeLoaderConstant.Category.Operations, ClientModuleRegistration.Section.CustomerService, new ModuleInfo(ClientModuleRegistration.CustomerServiceReports, typeof(CustomerServiceReports))));
					AddModuleInfoIf(modules, ModuleTreeLoaderConstant.Category.Operations, ClientModuleRegistration.Section.CustomerService, ClientModuleRegistration.IncidentManagementGroup, typeof(IncidentManagementGroupModule), EDIDataRegistry.Instance.EnableIncidentManagementGroupModule.Value, new TableRegistrationInfo(IncidentManagementGroupSchema.Constants.TableName));
					modules.Add(new NewClientModuleInfo(ModuleTreeLoaderConstant.Category.Operations, ClientModuleRegistration.Section.CustomerService, new ModuleInfo(ClientModuleRegistration.EdiIdentityCertificate, typeof(EdiIdentityCertificateModule), new TableRegistrationInfo(EdiIdentityCertificateSchema.Constants.TableName))));
					modules.Add(new NewClientModuleInfo(ModuleTreeLoaderConstant.Category.Operations, ClientModuleRegistration.Section.CustomerService, new ModuleInfo(ClientModuleRegistration.IncidentTriage, typeof(IncidentTriageModule), new TableRegistrationInfo(IncidentTriageSchema.Constants.TableName))));
					modules.Add(new NewClientModuleInfo(ModuleTreeLoaderConstant.Category.Operations, ClientModuleRegistration.Section.CustomerService, new ModuleInfo(ClientModuleRegistration.IncidentTriageChecklistItem, typeof(IncidentTriageChecklistItemModule), new TableRegistrationInfo(IncidentTriageChecklistItemSchema.Constants.TableName))));
					modules.Add(new NewClientModuleInfo(ModuleTreeLoaderConstant.Category.Operations, ClientModuleRegistration.Section.CustomerService, new ModuleInfo(ClientModuleRegistration.IncidentDiagnosticCriteria, typeof(IncidentDiagnosticCriteriaModule), new TableRegistrationInfo(IncidentDiagnosticCriteriaSchema.Constants.TableName))));
					modules.Add(new NewClientModuleInfo(ModuleTreeLoaderConstant.Category.Operations, ClientModuleRegistration.Section.CustomerService, new ModuleInfo(ClientModuleRegistration.InvestigationItem, typeof(InvestigationItemModule), new TableRegistrationInfo(InvestigationItemSchema.Constants.TableName))));

					modules.Add(new NewClientModuleInfo(ModuleTreeLoaderConstant.Category.Operations, ClientModuleRegistration.Section.CustomerService, new ModuleInfo(ClientModuleRegistration.TokenAuthenticationOnBoarding, typeof(TokenAuthenticationOnBoardingModule), new TableRegistrationInfo(EdiTokenAuthOnBoardingDataSchema.Constants.TableName))));
					modules.Add(new NewClientModuleInfo(ModuleTreeLoaderConstant.Category.Operations, ClientModuleRegistration.Section.CustomerService, new ModuleInfo(ClientModuleRegistration.EdiIdentityApplication, typeof(EdiIdentityApplicationModule), new TableRegistrationInfo(EdiIdentityApplicationSchema.Constants.TableName))));
					modules.Add(new NewClientModuleInfo(ModuleTreeLoaderConstant.Category.Operations, ClientModuleRegistration.Section.CustomerService, new ModuleInfo(ClientModuleRegistration.EdiIdentityTenant, typeof(EdiIdentityTenantModule), new TableRegistrationInfo(EdiIdentityTenantSchema.Constants.TableName))));

					// Manage -> Workflow & Process
					modules.Add(new NewClientModuleInfo(ModuleTreeLoaderConstant.Category.Manage, ModuleTreeLoaderConstant.Section.ManageWorkflowSection, new ModuleInfo(ClientModuleRegistration.ImplementationEmails, typeof(ImplementationMailItemModule), new TableRegistrationInfo(MailDBItemsSchema.Constants.TableName))));

					// Other - not in Menu
					modules.Add(new NewClientModuleInfo(new ModuleInfo(ClientModuleRegistration.LicenceHeader, typeof(LicenceHeaderModule), new TableRegistrationInfo(LicenceHeaderSchema.Constants.TableName))));

					// Billing
					modules.Add(new NewClientModuleInfo(ModuleTreeLoaderConstant.Category.Manage, ModuleTreeLoaderConstant.Section.Receivables, new ModuleInfo(ClientModuleRegistration.MonthlyUsageBilling, typeof(EDI.Billing.Module.MonthlyUsageBillingModule))));
					modules.Add(new NewClientModuleInfo(ModuleTreeLoaderConstant.Category.Manage, ModuleTreeLoaderConstant.Section.Receivables, new ModuleInfo(ClientModuleRegistration.StlBilling, typeof(EDI.Billing.Module.StlBillingModule))));
					modules.Add(new NewClientModuleInfo(ModuleTreeLoaderConstant.Category.Manage, ModuleTreeLoaderConstant.Section.Receivables, new ModuleInfo(ClientModuleRegistration.MaintenanceBilling, typeof(EDI.Billing.Module.MaintenanceBillingModule))));
					modules.Add(new NewClientModuleInfo(ModuleTreeLoaderConstant.Category.Manage, ModuleTreeLoaderConstant.Section.Receivables, new ModuleInfo(ClientModuleRegistration.BillingPrices, typeof(EDI.Billing.Module.BillingPrices.BillingPricesModule), new TableRegistrationInfo(ClientLicencePriceItemSchema.Constants.TableName))));

					// Device Management
					modules.Add(new NewClientModuleInfo(ModuleTreeLoaderConstant.Category.Operations, ClientModuleRegistration.Section.CustomerService, new ModuleInfo(ClientModuleRegistration.ClientDeviceTemplate, typeof(EDI.DeviceManagement.Module.ClientDeviceHeaderTemplateModule), new TableRegistrationInfo(DmgDeviceHeaderSchema.Constants.TableName))));
					modules.Add(new NewClientModuleInfo(ModuleTreeLoaderConstant.Category.Operations, ClientModuleRegistration.Section.CustomerService, new ModuleInfo(ClientModuleRegistration.ClientDevice, typeof(EDI.DeviceManagement.Module.ClientDeviceHeaderModule), new TableRegistrationInfo(DmgDeviceHeaderSchema.Constants.TableName))));

					// System And User Management
					modules.Add(new NewClientModuleInfo(ModuleTreeLoaderConstant.Category.Operations, ClientModuleRegistration.Section.SystemAndUserManagement, new ModuleInfo(ClientModuleRegistration.LicenceDatabaseRegistration, typeof(LicenceDatabaseRegistrationModule), new TableRegistrationInfo(LicenceDatabaseSchema.Constants.TableName))));
					modules.Add(new NewClientModuleInfo(ModuleTreeLoaderConstant.Category.Operations, ClientModuleRegistration.Section.SystemAndUserManagement, new ModuleInfo(ClientModuleRegistration.SystemUserAccounts, typeof(SystemUserAccountsModule), new TableRegistrationInfo(EdiCustomerUserAccountSchema.Constants.TableName))));

					// Application Logging
					modules.Add(new NewClientModuleInfo(ModuleTreeLoaderConstant.Category.Operations, ClientModuleRegistration.Section.Development, new ModuleInfo(ClientModuleRegistration.ApplicationLogger, typeof(ApplicationLoggerModule), new TableRegistrationInfo(ApplicationLoggerSchema.Constants.TableName))));
					modules.Add(new NewClientModuleInfo(ModuleTreeLoaderConstant.Category.Operations, ClientModuleRegistration.Section.Development, new ModuleInfo(ClientModuleRegistration.ApplicationActiveLogger, typeof(ApplicationActiveLoggerModule), new TableRegistrationInfo(ApplicationActiveLoggerSchema.Constants.TableName))));

					newClientModules = modules.ToArray();
					newClientModulesRef = new WeakReference<NewClientModuleInfo[]>(newClientModules);
				}

				return newClientModules;
			}
		}
		WeakReference<NewClientModuleInfo[]> newClientModulesRef;

		void AddModuleInfoIf(List<NewClientModuleInfo> moduleInfoList, ModuleTreeLoaderConstant.Entry category, string sectionName, ClientModuleIdentifier moduleID, Type moduleType, bool condition, TableRegistrationInfo moduleTable = null)
		{
			if (condition)
			{
				moduleInfoList.Add(new NewClientModuleInfo(category, sectionName, new ModuleInfo(moduleID, moduleType, moduleTable)));
			}
		}

		protected override ModuleIdentifier[] NewReportModulesCore
		{
			get => new[]
			{
				ClientModuleRegistration.TrainingReports,
				ClientModuleRegistration.WorkItemsReports,
				ClientModuleRegistration.CustomerServiceReports
			};
		}

		#endregion

		#region Sections

		protected override IModuleSectionAddOn[] NewModuleSectionsToAddForClientCore
		{
			get
			{
				var sections = new List<IModuleSectionAddOn>();
				sections.Add(new ModuleSectionAddOn(ModuleTreeLoaderConstant.Category.Operations.Name, ClientModuleRegistration.Section.Development, (NoResString)"Development", IncidentApproval.DefaultClientSpecificMenuSection, EDISecurityCheckpoints.Development, IconTypes.Truck, IconTypes.Truck20x16, ClientModuleRegistration.Subcategory.WTG));
				sections.Add(new ModuleSectionAddOn(ModuleTreeLoaderConstant.Category.Operations.Name, ClientModuleRegistration.Section.CustomerService, (NoResString)"Customer Service", IncidentApproval.DefaultClientSpecificMenuSection, EDISecurityCheckpoints.CustomerService, IconTypes.Phone, IconTypes.Phone20x16, ClientModuleRegistration.Subcategory.WTG));
				sections.Add(new ModuleSectionAddOn(ModuleTreeLoaderConstant.Category.Operations.Name, ClientModuleRegistration.Section.SystemAndUserManagement, (NoResString)"System And User Management", IncidentApproval.DefaultClientSpecificMenuSection, EDISecurityCheckpoints.SystemAndUserManagement, IconTypes.SomeBooks, IconTypes.SomeBooks20x16, ClientModuleRegistration.Subcategory.WTG));
				return sections.ToArray();
			}
		}

		public override bool AddNewModuleSectionsAtTopOfTree => true;

		#endregion

		#region Document Scanning Assemblies

		public class ProfessionalServicesQuoteData : AssemblyData
		{
			public override string ReferenceType => Core.Constants.ReferenceTypes.BusinessEntityProcessWorkflow;
			public override MultilingualString HumanReadableName => ResString.GetMultilingualString("1CA9751A-E381-47BE-A6EF-B41723712FB3", "Professional Services Quote");
			public override Type BusinessObjectType => typeof(ProfessionalServicesQuote);
			protected override Type CollectionType => null;
			public override ModuleIdentifier ModuleID => ClientModuleRegistration.ProfessionalServicesQuote;
		}

		public class IncidentManagementGroupData : AssemblyData
		{
			public override Type BusinessObjectType => typeof(IncidentManagementGroup);

			protected override Type CollectionType => typeof(IncidentManagementGroupCollection);

			public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory) => new IncidentManagementGroupCollection(factory);

			public override ModuleIdentifier ModuleID => ModuleIDs.ServiceRequest;

			public override string ReferenceType => Core.Constants.ReferenceTypes.BusinessEntityProcessWorkflow;

			public override MultilingualString HumanReadableName => ResString.GetMultilingualString("4921D8D9-7587-4EDD-822F-97D5AA582386", "Incident Management Group");

			public override bool IsAllowedForUnallocatedeDocs => true;
		}

		public class FeatureControlHeaderData : AssemblyData
		{
			public override Type BusinessObjectType => typeof(FeatureControlHeader);

			protected override Type CollectionType => typeof(FeatureControlHeaderCollection);

			public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory) => new FeatureControlHeaderCollection(factory);

			public override string ReferenceType => Core.Constants.ReferenceTypes.BusinessEntityProcessWorkflow;

			public override MultilingualString HumanReadableName => ResString.GetMultilingualString("c17f5bc4-ba67-4504-8cfe-73fa619dec20", "Feature Control");

			public override bool IsAllowedForUnallocatedeDocs => true;

			public override ModuleIdentifier ModuleID => ClientModuleRegistration.FeatureControl;
		}

		public class IncidentDiagnosticCriteriaData : AssemblyData
		{
			public override Type BusinessObjectType => typeof(IncidentDiagnosticCriteria);

			protected override Type CollectionType => typeof(IncidentDiagnosticCriteriaCollection);

			public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory) => new IncidentDiagnosticCriteriaCollection(factory);

			public override string ReferenceType => Core.Constants.ReferenceTypes.BusinessEntityProcessWorkflow;

			public override MultilingualString HumanReadableName => ResString.GetMultilingualString("7c9302fb-f6f6-4cde-94e9-daffeeabad9e", "Diagnostic Criteria");

			public override bool IsAllowedForUnallocatedeDocs => true;

			public override ModuleIdentifier ModuleID => ClientModuleRegistration.IncidentDiagnosticCriteria;
		}

		public class EdiUserAgreementData : AssemblyData
		{
			public override Type BusinessObjectType => typeof(EdiUserAgreement);

			protected override Type CollectionType => typeof(EdiUserAgreementCollection);

			public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory) => new EdiUserAgreementCollection(factory);

			public override string ReferenceType => Core.Constants.ReferenceTypes.BusinessEntityProcessWorkflow;

			public override MultilingualString HumanReadableName => ResString.GetMultilingualString("fe440168-9f0f-40b0-8f2e-6be4368a1468", "User Agreement");

			public override bool IsAllowedForUnallocatedeDocs => true;

			public override ModuleIdentifier ModuleID => ClientModuleRegistration.UserAgreements;
		}

		#endregion

		#region Document Engine Collection Providers

		public override Dictionary<string, Type> DocumentEngineCollectionProviders
		{
			get
			{
				var result = new Dictionary<string, Type>();
				result.Add("licence enterprise", typeof(LicenceEnterpriseCollectionProvider));
				result.Add("survey campaign", typeof(SurveyCampaignCollectionProvider));
				result.Add("incident", typeof(IncidentCollectionProvider));
				return result;
			}
		}

		#endregion

		#region Document Engine Code Description Pair Providers

		public override Dictionary<string, Type> DocumentEngineCodeDescriptionPairProviders
		{
			get
			{
				var result = new Dictionary<string, Type>();
				result.Add("licencemodules", typeof(LicenceModuleCodeDescriptionPairProvider));
				result.Add("incidentcategories", typeof(SupportIncidentCategoryCodeDescriptionPairProvider));
				result.Add("incidentmodules", typeof(SupportIncidentModuleCodeDescriptionPairProvider));
				result.Add("incidentsourcemodules", typeof(SupportIncidentSourceModuleCodeDescriptionPairProvider));
				result.Add("incidentproducts", typeof(SupportIncidentProductCodeDescriptionPairProvider));
				result.Add("incidentproductareas", typeof(SupportIncidentProductAreaCodeDescriptionPairProvider));
				result.Add("incidentdispositions", typeof(SupportIncidentDispositionCodeDescriptionPairProvider));
				return result;
			}
		}

		#endregion

		#region Type Deciders

		public override ITypeDeciderDictionary ClientTypeDeciders
		{
			get
			{
				if (clientTypeDeciders == null)
				{
					var list = new Dictionary<Type, ITypeDecider>();

					list.Add(typeof(OrgHeader), new TypeDeciderImpl(typeof(EDIOrgHeader)));
					list.Add(typeof(OrgAddress), new TypeDeciderImpl(typeof(EDIOrgAddress)));
					list.Add(typeof(OrgMiscServ), new TypeDeciderImpl(typeof(EDIOrgMiscServ)));
					list.Add(typeof(OrgCommissionAgreement), new TypeDeciderImpl(typeof(EdiCommissionAgreement)));
					list.Add(typeof(OrgCompanyData), new TypeDeciderImpl(typeof(EDIOrgCompanyData)));
					list.Add(typeof(OrgOpportunity), new TypeDeciderImpl(typeof(EDIOrgOpportunity)));
					list.Add(typeof(OrganisationFilterBusinessObject), new TypeDeciderImpl(typeof(EDIOrganisationFilterBusinessObject)));
					list.Add(typeof(OrgContactsFilterBusinessObject), new TypeDeciderImpl(typeof(EDIOrgContactsFilterBusinessObject)));
					list.Add(typeof(GlbCompanyCampaign), new EDIGlbCompanyCampaignTypeDecider());
					list.Add(typeof(GlbCompanyCampaignItem), new EDIGlbCompanyCampaignItemTypeDecider());
					list.Add(typeof(GlbCompanyCampaignDripMarketing), new TypeDeciderImpl(typeof(EDIGlbCompanyCampaignDripMarketing)));
					list.Add(typeof(ProcessTask), new EDIProcessTaskTypeDecider());
					list.Add(typeof(ARInvoice), new TypeDeciderImpl(typeof(EDIARInvoice)));
					list.Add(typeof(ARCreditNote), new TypeDeciderImpl(typeof(EDIARCreditNote)));
					list.Add(typeof(RefZoneHeader), new TypeDeciderImpl(typeof(EDIRefZoneHeader)));
					list.Add(typeof(RefZonePivot), new TypeDeciderImpl(typeof(EDIRefZonePivot)));
					list.Add(typeof(OrgContact), new TypeDeciderImpl(typeof(EDIOrgContact)));
					list.Add(typeof(MailItem), new TypeDeciderImpl(typeof(EDIMailItem)));
					list.Add(typeof(GlbStaff), new TypeDeciderImpl(typeof(EDIGlbStaff)));
					list.Add(typeof(GlbPerson), new TypeDeciderImpl(typeof(EDIGlbPerson)));
					list.Add(typeof(SalesEnquiry), new TypeDeciderImpl(typeof(EDISalesInquiry)));
					list.Add(typeof(OrgRelatedParty), new TypeDeciderImpl(typeof(EDIOrgRelatedParty)));
					list.Add(typeof(OrgCusCode), new TypeDeciderImpl(typeof(EDIOrgCusCode)));
					list.Add(typeof(DeduplicationOrganisation), new TypeDeciderImpl(typeof(EDIDeduplicationOrganisation)));
					list.Add(typeof(CommissionManagement.Business.AccCommissionHeader), new TypeDeciderImpl(typeof(EdiCommissionHeader)));
					list.Add(typeof(CommissionManagement.Business.CommissionAgreementApprover), new TypeDeciderImpl(typeof(EDICommissionAgreementApprover)));
					list.Add(typeof(CommissionManagement.Business.ViewCommissionLine), new TypeDeciderImpl(typeof(EdiViewCommissionLine)));
					list.Add(typeof(ServiceManager.Tasks.StandardXMLProcessor.SystemXmlMessageProcessor), new TypeDeciderImpl(typeof(EDISystemXmlMessageProcessor)));
					list.Add(typeof(ProcessManagement.Business.WorkItem), new TypeDeciderImpl(typeof(NewWorkItem)));
					list.Add(typeof(ProcessManagement.Business.WorkItemConvertedFromJiraIssue), new TypeDeciderImpl(typeof(NewWorkItemConvertedFromJiraIssue)));
					list.Add(typeof(ProcessManagement.Business.WorkItemProcessTask), new TypeDeciderImpl(typeof(WorkItemProcessTask)));
					list.Add(typeof(ProcessManagement.Business.Project), new TypeDeciderImpl(typeof(EDIProject)));
					list.Add(typeof(ProcessManagement.Business.ProjectProcessTask), new TypeDeciderImpl(typeof(ProjectProcessTask)));
					list.Add(typeof(OrgPatternMatchOverride), new TypeDeciderImpl(typeof(EDIOrgPatternMatchOverride)));
					list.Add(typeof(MailManager.DatabaseEmailManagement), new TypeDeciderImpl(typeof(EDI.Mail.ServiceTasks.EDIDatabaseEmailManagement)));
					list.Add(typeof(CommissionAgreementConflictsFinder), new TypeDeciderImpl(typeof(EdiCommissionAgreementConflictsFinder)));
					list.Add(typeof(CommissionAgreementConflictsTextProvider), new TypeDeciderImpl(typeof(EdiCommissionAgreementConflictsTextProvider)));
					list.Add(typeof(AddRelatedManagementOrganisationModuleDecisionProvider), new TypeDeciderImpl(typeof(EdiAddRelatedManagementOrganisationModuleDecisionProvider)));
					list.Add(typeof(SendEmailActionMenuStrategy), new TypeDeciderImpl(typeof(EDI.IncidentManager.GUI.EDISendEmailActionMenuStrategy)));
					list.Add(typeof(MarketingManager.Module.GlbCompanyCampaignContactFilterModuleStrategy), new TypeDeciderImpl(typeof(EDIGlbCompanyCampaignContactFilterModuleStrategy)));
					list.Add(typeof(ZArchitecture.Business.EventManagement.WorkflowSupportableTableNames), new TypeDeciderImpl(typeof(EDIWorkflowSupportableTableNames)));
					list.Add(typeof(ProcessTaskRowFetchStrategy), new TypeDeciderImpl(typeof(EDIProcessTaskRowFetchStrategy)));
					list.Add(typeof(Services.ServiceHost.IIncidentRequestService), new TypeDeciderImpl(typeof(EDI.IncidentManager.Service.EdiIncidentRequestService)));
					list.Add(typeof(IncidentRequest), new TypeDeciderImpl(typeof(EdiIncidentRequest)));
					list.Add(typeof(JobConversationMessage), new TypeDeciderImpl(typeof(EdiJobConversationMessage)));
					list.Add(typeof(Accounting.Business.GenericCharge.GenericCharge), new TypeDeciderImpl(typeof(EDIGenericCharge)));
					list.Add(typeof(LoginContact), new TypeDeciderImpl(typeof(EDILoginContact)));
					list.Add(typeof(Services.ServiceHost.IDeviceLicenceService), new TypeDeciderImpl(typeof(EDI.DeviceManagement.Service.EdiDeviceLicenceService)));
					list.Add(typeof(JobConversationParticipant), new TypeDeciderImpl(typeof(EdiJobConversationParticipant)));
					list.Add(typeof(StaffAssignmentsFilterBusinessObject), new TypeDeciderImpl(typeof(EDIStaffAssignmentsFilterBusinessObject)));
					list.Add(typeof(ZClientSpecificPurgeSettingConfig), new TypeDeciderImpl(typeof(EdiClientPurgeSettingsConfig)));
					list.Add(typeof(AutoLoggedTablesDefaultConfigValues), new TypeDeciderImpl(typeof(EDIAutoLoggedTablesDefaultConfigValues)));

					clientTypeDeciders = new TypeDeciderDictionary(list);
				}
				return clientTypeDeciders;
			}
		}

		ITypeDeciderDictionary clientTypeDeciders;

		#endregion

		#region Security

		public override void AddClientSpecificSecurityCheckpointsToSecurityInstance(IZSecurity securityInstance)
		{
			var result = new EDISecurityCheckpoints();
			result.AddOrgLicenceModifySecurityCheckpoint(securityInstance);
			result.AddOrgLicenceModifyLicenceKeySecurityCheckpoint(securityInstance);
			result.AddOrgLicenceCreateAndEmailLicenceKeySecurityCheckpoint(securityInstance);
			result.AddOrgLicenceModifyLicence3rdPartySoftware(securityInstance);
			result.AddOrgLicenceModifyInstallationDetailsSecurityCheckPoint(securityInstance);
			result.AddOrgLicenceModifyAgreedGoLiveSecurityCheckPoint(securityInstance);
			result.AddOrgLicenceModifyConnectionDetailsSecurityCheckPoint(securityInstance);
			result.AddOrgLicenceModifySupportAndContractDetailsCheckpoint(securityInstance);
			result.AddOrgLicenceModifyDatabaseDetailsSecurityCheckpoint(securityInstance);
			result.AddOrgLicenceModifySendUpgradeSecurityCheckPoint(securityInstance);
			result.AddOrgLicenceModifySaveUpgradeToDiskCheckpoint(securityInstance);
			result.AddOrgLicenceModifySendSupersededUpgradeCheckpoint(securityInstance);
			result.AddOrgLicenceModifySendUpgradeToHigherRingDBCheckpoint(securityInstance);
			result.AddOrgLicenceModifySendUpgradeToDBNotOnLowestSupportedSqlVersionCheckpoint(securityInstance);
			result.AddOrgLicenceModifySetDatabaseSecurityModeToOpenSecurityCheckpoint(securityInstance);
			result.AddOrgLicenceModifyExchangeRatesAndTaxCheckpoint(securityInstance);
			result.AddOrgLicenceRemoveLicenceDatabaseSecurityCheckpoint(securityInstance);
			result.AddOrgLicenceMoveLicencesToNewEnterpriseSecurityCheckpoint(securityInstance);
			result.AddSupportEmailsSecurityCheckpoint(securityInstance);
			result.AddImplementationEmailsSecurityCheckpoint(securityInstance);
			result.AddCommissionSecurityCheckpoints(securityInstance);
			result.AddCustomerServiceSecurityCheckpoints(securityInstance);
			result.AddInstallAndUpgradeCheckpoints(securityInstance);
			result.AddDevelopmentCheckpoints(securityInstance);
			result.AddOrgLicenceModifyLicDatabaseModifyToHigherRingCheckpoint(securityInstance);
			result.AddOrgLicenceModifyLicDatabaseModifyToRestrictedRingCheckpoint(securityInstance);
			result.AddOrgLicenceBillingCheckpoint(securityInstance);
			result.AddTelematicsDeviceCheckpoints(securityInstance);
			result.AddSystemAndUserManagementCheckpoints(securityInstance);
			result.AddWorkItemSecurityCheckpoints(securityInstance);
			result.AddAvalaraUSSalesTaxCheckpoints(securityInstance);
			result.AddApplicationLoggingCheckpoints(securityInstance);
			result.AddSTLBillAdminCheckpoints(securityInstance);
		}

		public override IEnumerable<ISecurityCheckpoint> GetClientSpecificJobInvoicingCheckpoints(IZSecurity securityInstance)
		{
			yield return securityInstance.FindCheckPoint(new CheckpointLookupKey(EDISecurityCheckpoints.Constants.CustomerServiceIncidentJobInvoicingReferenceName));
			yield return securityInstance.FindCheckPoint(new CheckpointLookupKey(EDISecurityCheckpoints.Constants.ProfessionalServicesQuoteJobInvoicingReferenceName));
			yield return securityInstance.FindCheckPoint(new CheckpointLookupKey(EDISecurityCheckpoints.Constants.InstallationTaskJobInvoicingReferenceName));
		}

		#endregion

		#region Staff Product Keys & Support Password

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1093:DoNotUseSystemWindowsFormsToolStripControls", Justification = "Baseline")]
		ToolStripMenuItem[] GetNewHelpMenuItems()
		{
			var supportPasswordMenuItem = new ZToolStripMenuItem("Support Password", SupportPasswordMenuItem_Click)
			{
				ShortcutKeys = (Keys)Shortcut.CtrlShiftS,
				ShowShortcutKeys = true
			};

			var supportTokenMenuItem = new ZToolStripMenuItem("Support Token", SupportTokenMenuItem_Click)
			{
				ShortcutKeys = (Keys)Shortcut.CtrlShiftT,
				ShowShortcutKeys = true
			};

			var ssoAuthTokenMenuItem = new ZToolStripMenuItem("SSO Auth Token", SSOAuthTokenMenuItem_Click);

			var ratesServiceAdminMenuItem = new ZToolStripMenuItem("Rates Service Admin", RatesServiceAdminMenuItem_Click);

			return new ToolStripMenuItem[] {
				StaffProductKeyMenu.Create(),
				supportPasswordMenuItem,
				supportTokenMenuItem,
				ssoAuthTokenMenuItem,
				ratesServiceAdminMenuItem
			};
		}

		void SafeStartProcess(string url)
		{
			try
			{
				WebUrlLauncher.Launch(url);
			}
			catch (Win32Exception ex)
			{
				ErrorReporter.ReportOnce("HelpUrl", "Failed to open web address for: " + url, ex);
				Globals.Message.ShowInformation(Res.GetString("BC378B4B-9D28-4DEA-A002-980FD8631CA1", "The web address '{0}' could not be opened. Please try copy and pasting it into your web browser instead.", url));
			}
		}

		void SupportTokenMenuItem_Click(object sender, EventArgs e)
		{
			var token = GenerateSupportLoginToken();
			if (token == null)
			{
				return;
			}

			var msg = Res.GetString("f49b3771-3fa9-4d59-918b-28e8879ff018", "Your Support Login Token has been copied to the clipboard.");
			DoClipboard(token, "Support Token", msg);
		}

		void SSOAuthTokenMenuItem_Click(object sender, EventArgs e)
		{
			var msg = Res.GetString("9223cb9d-b97d-4b9c-8f0a-3a596dc1a3c1", "Your SSO Auth Token has been copied to the clipboard.");
			Action<string> doClipboard = (token) => DoClipboard(token, "SSO Auth Token", msg);
			ZFormModaliser.ShowDialogAndDispose(new SSOAuthTokenGeneratorForm(GenerateSupportLoginToken, doClipboard, new WTGAuthTokenProvider()));
		}

		void RatesServiceAdminMenuItem_Click(object sender, EventArgs e)
		{
			void SetLoginTypeAndSupportToken(LoginInfo loginInfo)
			{
				loginInfo.Type = LoginType.CW1Support;
				loginInfo.Password = GenerateSupportLoginToken();
			}
			var tokenProvider = ObjectFactory.Get<IAuthTokenProvider>();
			var (token, validation) = tokenProvider.GetToken(correlationID: Guid.NewGuid().ToString(), overrideLoginInfo: SetLoginTypeAndSupportToken);

			if (token == null)
			{
				Globals.Message.ShowError(Res.GetString("785b27ed-294c-45b5-a265-95be48acba9a", "Unable to obtain SSO Auth token: {0}", validation));
				return;
			}

			var ratesServiceAdminInterfaceUrl = RatingDataRegistry.Instance.RatesServiceAdminInterfaceUrl.Value;
			var url = $"{ratesServiceAdminInterfaceUrl}?token={token}";

			SafeStartProcess(url);
		}

		/// <summary>
		/// We have replace the support password with token, keep the menu to generate support password because of the compatibility
		/// </summary>
		void SupportPasswordMenuItem_Click(object sender, EventArgs e)
		{
			var password = MasterPassword.GenerateForCurrentUserToday();
			var msg = string.Format("Your Support Password for {0} \"{1}\" has been copied to the clipboard.", ZDateTime.UtcNow.ToString("MMM yyyy"), password);
			DoClipboard(password, "Support Password", msg);
		}

		string GenerateSupportLoginToken()
		{
			var privateKeyBytes = EDIDataRegistry.Instance.CWSupportLoginTokenPrivateKey.Value;
			if (privateKeyBytes == null || privateKeyBytes.Length == 0)
			{
				Globals.Message.Show(Res.GetString("3FD982F0-59FB-4DF8-80C5-8C4E7277D5E1", "Please upload the private key in the registry item '{0}' to sign the login token.", EDIDataRegistry.Instance.CWSupportLoginTokenPrivateKey.HumanReadableRegistryPath()));
				return null;
			}

			var privateKeyPem = Encoding.UTF8.GetString(privateKeyBytes);
			var privateKey = RSAKeyProvider.ImportPrivateKey(privateKeyPem);

			var certificateData = DataRegistry.Instance.CWSupportLoginTokenCertificate;
			var certificate = new X509Certificate2(certificateData);

			var time = ZDateTime.UtcNow.ToDateTime();
			var userRoles = EDISecurityCheckPointRoleMapper.GetInternalUserRoles();

			var payload = new JwtPayload(null, null, null, time, time.AddHours(1), time)
			{
				{ "sub", Env.CurrentUser.Initials },
				{ "name", Env.CurrentUser.FullName },
				{ "roles", userRoles },
				{ "jti", ZGuid.NewZGuid().ToString() }
			};

			var token = JwtSecurity.GenerateSignedJwt(privateKey, certificate, payload);
			return token;
		}

		void DoClipboard(string password, string title, string message)
		{
			if (!SafeClipboard.SetText(password))
			{
				Globals.Message.Show(SafeClipboard.ClipboardNotAccessibleWarning);
			}
			else
			{
#if !WINZOR
				NotifyIcon.ShowBalloon(title, message, ZNotifyIconEx.NotifyInfoFlags.Info, 2000);
#else
				new ZNotifyIconEx().DisplayToastNotification(title, message);
#endif
			}
		}

#if !WINZOR
		ZNotifyIconEx NotifyIcon
		{
			get
			{
				if (notifyIcon == null)
				{
					notifyIcon = new ZNotifyIconEx();
					notifyIcon.Icon = BrandingFactory.Instance.ProductIcon;

					notifyIcon.Click += delegate
					{
						notifyIcon.Dispose();
						notifyIcon = null;
					};

					notifyIcon.BalloonClick += delegate
					{
						notifyIcon.Dispose();
						notifyIcon = null;
					};
				}

				return notifyIcon;
			}
		}

		ZNotifyIconEx notifyIcon;
#endif

#endregion

		#region Authentication

		public override bool IsValidLogin(string login, string password)
		{
#if DEBUG
			return true;
#else
			return !User.SupportUserName.Equals(login, StringComparison.InvariantCultureIgnoreCase)
				|| ObjectFactory.Get<Integration.Licensing.IProductRegistration>().Key.DatabaseType != DatabaseTypes.Codes.Production;
#endif
		}

		public override string LoginDeniedMessage => "The Developer Login and Master Password cannot be used on production " + ClientDisplayName + " installations.";

		#endregion

		#region Schema

		protected override ITableSchema[] GetTableSchemas()
		{
			return new ITableSchema[]
			{
				ReleaseBuildSchema.Instance,

				LicenceEnterpriseSchema.Instance,
				LicenceEnterpriseDomainsSchema.Instance,
				LicenceCompanySchema.Instance,
				LicenceDatabaseSchema.Instance,
				LicenceHeaderSchema.Instance,
				LicenceModulesSchema.Instance,
				LicenceConnectionSchema.Instance,
				Licence3rdPartySoftwareSchema.Instance,

				UpgradesToClientSchema.Instance,

				HelpErrorLogOccurrenceSchema.Instance,
				HelpErrorLogKeySchema.Instance,
				HelpErrorStackLineCountSchema.Instance,

				IncidentMainSchema.Instance,
				IncidentManagementLinkSchema.Instance,
				IncidentManagementGroupSchema.Instance,
				IncidentManagementGroupMessageSchema.Instance,
				IncidentManagementLinkSchema.Instance,
				IncidentTriageSchema.Instance,
				IncidentTriageChecklistItemSchema.Instance,
				IncidentTriageChecklistItemPivotSchema.Instance,
				IncidentDiagnosticCriteriaSchema.Instance,
				IncidentDiagnosticCriteriaPivotSchema.Instance,
				IncidentTriageDiagnosticCriteriaPivotSchema.Instance,
				InvestigationItemSchema.Instance,
				InvestigationItemResponseOptionSchema.Instance,
				DiagnosticCriteriaInvestigationItemLinkSchema.Instance,
				DiagnosticCriteriaInvestigationResultSchema.Instance,
				IncidentMetricsSchema.Instance,
				EdiLegacyConversationMessageSchema.Instance,

				AccAmbiguousCommissionSchema.Instance,

				ClientFeatureRequestValueAndContributionSchema.Instance,
				eRouterEdiEnterpriseCommunicationSchema.Instance,
				ClientLicenceBillingSchema.Instance,
				ClientLicenceBillingDiscountSchema.Instance,
				ClientLicenceBillingExcludeOrgSchema.Instance,
				ClientLicencePriceHeaderSchema.Instance,
				ClientLicencePriceItemSchema.Instance,
				EdiCommissionAgreementCompanyPivotSchema.Instance,
				EdiCommissionAgreementCompanyAutoAddCountrySchema.Instance,
				EdiCommissionAgreementCompanyAutoAddDatabaseSchema.Instance,
				EdiCommissionAgreementCustomizationSchema.Instance,
				EdiCommissionAgreementDatabasePivotSchema.Instance,
				EdiCommissionHeaderAdditionalInfoSchema.Instance,
				EdiPriceHeaderDiscountSchema.Instance,
				EdiPriceDiscountGroupMemberSchema.Instance,
				EdiPriceHeaderLinkSchema.Instance,
				EdiPriceItemRateSchema.Instance,
				EdiPriceUsageMappingSchema.Instance,
				EdiLicenceSettingSchema.Instance,
				EdiUsageInvoiceSchema.Instance,
				EdiBilledUsageSchema.Instance,
				EdiBilledDiscountSchema.Instance,
				EdiPriceHeaderExchangeRateSchema.Instance,
				ClientLicenceFeeSchema.Instance,
				ClientChargeableUsageSchema.Instance,
				ClientInvoiceDeliverySchema.Instance,
				ClientLicenceHeaderExSchema.Instance,
				ClientCompanySchema.Instance,
				ClientBranchSchema.Instance,
				ClientStaffSchema.Instance,
				EdiCustomerUserAccountSchema.Instance,
				EdiUserAgreementSchema.Instance,
				EdiUserAgreementAssignmentSchema.Instance,
				EdiUserAgreementAcceptanceLogSchema.Instance,
				EdiTrustedSystemSchema.Instance,
				EdiTrustedMessagingConfigSchema.Instance,
				EdiIdentityApplicationSchema.Instance,
				EdiIdentityApplicationPermissionSchema.Instance,
				EdiIdentityCertificateSchema.Instance,
				EdiIdentityRedirectUrlSchema.Instance,
				EdiIdentityTenantSchema.Instance,
				EdiTokenAuthOnBoardingDataSchema.Instance,
				EdiPromptSkipSchema.Instance,
				ClientLicenceUsageSchema.Instance,
				EDI.Licencing.Business.EdiLicenceUsageSchema.Instance,
				ClientFaxPriceSchema.Instance,
				ClientPremiumServiceSchema.Instance,
				ClientOrgImportHistorySchema.Instance,
				ClientOrgConsolSchema.Instance,
				ClientMailDBRecipientsSchema.Instance,
				ClientWorkProjectSchema.Instance,
				ClientIncidentEstimateSchema.Instance,
				ClientIncidentQuoteSchema.Instance,
				EdiDepositAdjustSchema.Instance,

				ClientTelRimRegistrationSchema.Instance,

				DmgDeviceHeaderSchema.Instance,
				DmgDeviceComponentSchema.Instance,
				DmgDeviceComponentIdentificationSchema.Instance,

				EdiOrgOpportunityExSchema.Instance,
				EdiOrgOpportunityValueAnalysisSchema.Instance,

				EdiReportingQueueSchema.Instance,
				EdiPersonMergeQueueSchema.Instance,

				EdiOrgMembershipSchema.Instance,
				EdiStaffChangeSchema.Instance,
				EdiGlbStaffExSchema.Instance,

				EdiERequestDocumentQueueSchema.Instance,

				IncidentSimilarityTokenSchema.Instance,
				IncidentSimilarityMatrixSchema.Instance,
				IncidentSimilarityTfIdfSchema.Instance,

				ELearningDocumentDescriptionSchema.Instance,
				ELearningDocumentTfIdfSchema.Instance,

				IncidentSimilarityExclusionSchema.Instance,

				EdiLicenceDatabaseOrgSuggestionSchema.Instance,

				FeatureControlHeaderSchema.Instance,
				FeatureControlRuleSchema.Instance,
				FeatureControlRuleLicenceDatabasePivotSchema.Instance,
				FeatureControlSetSchema.Instance,

				ApplicationLoggerSchema.Instance,
				ApplicationActiveLoggerSchema.Instance,
			};
		}

		public override IExtensionObjects DbSchemaExtensionObjects => dbSchemaUpgradeInfo ?? (dbSchemaUpgradeInfo = new EDIClientDbSchemaUpgradeInfo());

		IExtensionObjects dbSchemaUpgradeInfo;

		#endregion

		#region LogSubscribers

		public override IEnumerable<ILogSubscriber> LogSubscribers => new ILogSubscriber[]
				{
					new EDI.IncidentManager.BatchProcessor.IncidentEDocLogSubscriber()
					, new EDI.IncidentManager.BatchProcessor.IncidentApprovalLogSubscriber()
					, new EDI.IncidentManager.BatchProcessor.IncidentAutoresponderLogSubscriber()
				};

		#endregion
	}
}
