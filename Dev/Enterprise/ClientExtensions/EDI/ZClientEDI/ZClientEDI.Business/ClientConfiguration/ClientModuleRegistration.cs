using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.Modules
{
	public static class ClientModuleRegistration
	{
		// Development
		public readonly static ClientModuleIdentifier ProfessionalServicesQuote = new ClientModuleIdentifier(EDIModuleId.ProfessionalServicesQuote, "Professional Services Quotes");
		public readonly static ClientModuleIdentifier IssueManager = new ClientModuleIdentifier(EDIModuleId.IssueManager, "Issue Manager");
		public readonly static ClientModuleIdentifier UserAgreements = new ClientModuleIdentifier(EDIModuleId.UserAgreements, "User Agreements");
		public readonly static ClientModuleIdentifier UserAgreementAcceptances = new ClientModuleIdentifier(EDIModuleId.UserAgreementAcceptances, "User Agreement Acceptances");
#if DEBUG
		public readonly static ClientModuleIdentifier GlbReleaseNote = new ClientModuleIdentifier(EDIModuleId.GlbReleaseNote, "Update Notes");
#endif
		public readonly static ClientModuleIdentifier WorkItemsReports = new ClientModuleIdentifier(EDIModuleId.WorkItemsReports, "Reports", "Reports (Work Items)");

		// Customer Service
		public readonly static ClientModuleIdentifier SupportIncident = new ClientModuleIdentifier(EDIModuleId.SupportIncident, "Incidents");
		public readonly static ClientModuleIdentifier CustomerServiceReports = new ClientModuleIdentifier(EDIModuleId.CustSvcReports, "Reports", "Reports (Customer Service)");
		public readonly static ClientModuleIdentifier DbRestoreKey = new ClientModuleIdentifier(EDIModuleId.DbRestoreKey, "DB Restore Release Key");
		public readonly static ClientModuleIdentifier EDICustomerServiceEmails = new ClientModuleIdentifier(EDIModuleId.EDICustomerServiceEmails, "Customer Service Emails");
		public readonly static ClientModuleIdentifier LicenceEnterprise = new ClientModuleIdentifier(EDIModuleId.LicenceEnterprise, "Enterprise Licences");
		public readonly static ClientModuleIdentifier LicenseKey = new ClientModuleIdentifier(EDIModuleId.LicenseKey, "License Keys");
		public readonly static ClientModuleIdentifier LicenceDatabase = new ClientModuleIdentifier(EDIModuleId.LicenceDatabase, "Licence Databases");
		public readonly static ClientModuleIdentifier EdiTrustedMessagingConfig = new ClientModuleIdentifier(EDIModuleId.EdiTrustedMessagingConfig, "Trusted Messaging Configuration");
		public readonly static ClientModuleIdentifier EdiTrustedSystem = new ClientModuleIdentifier(EDIModuleId.EdiTrustedSystem, "Trusted Systems");
		public readonly static ClientModuleIdentifier LicenceHeader = new ClientModuleIdentifier(EDIModuleId.LicenceHeader, "Licence Installations");
		public readonly static ClientModuleIdentifier ReleaseBuild = new ClientModuleIdentifier(EDIModuleId.ReleaseBuild, "Release Builds");
		public readonly static ClientModuleIdentifier IncidentManagementGroup = new ClientModuleIdentifier(EDIModuleId.IncidentManagementGroups, "Incident Management Groups");
		public readonly static ClientModuleIdentifier EdiIdentityCertificate = new ClientModuleIdentifier(EDIModuleId.IdentityCertificate, "System Identity Certificates");
		public readonly static ClientModuleIdentifier TokenAuthenticationOnBoarding = new ClientModuleIdentifier(EDIModuleId.TokenAuthenticationOnBoarding, "Token Authentication Onboarding");
		public readonly static ClientModuleIdentifier EdiIdentityApplication = new ClientModuleIdentifier(EDIModuleId.EdiIdentityApplication, "System Identity Application");
		public readonly static ClientModuleIdentifier EdiIdentityTenant = new ClientModuleIdentifier(EDIModuleId.EdiIdentityTenant, "System Identity Tenant");
		public readonly static ClientModuleIdentifier IncidentTriage = new ClientModuleIdentifier(EDIModuleId.IncidentTriage, "Incident Triage");
		public readonly static ClientModuleIdentifier IncidentTriageChecklistItem = new ClientModuleIdentifier(EDIModuleId.IncidentTriageChecklistItem, "Checklist Items");
		public readonly static ClientModuleIdentifier IncidentDiagnosticCriteria = new ClientModuleIdentifier(EDIModuleId.IncidentDiagnosticCriteria, "Diagnostic Criteria");
		public readonly static ClientModuleIdentifier InvestigationItem = new ClientModuleIdentifier(EDIModuleId.InvestigationItem, "Investigation Items");
		public readonly static ClientModuleIdentifier FeatureControl = new ClientModuleIdentifier(EDIModuleId.FeatureControl, "Feature Control");
		public readonly static ClientModuleIdentifier FeatureSet = new ClientModuleIdentifier(EDIModuleId.FeatureSet, "Feature Set");

		// Manage -> Workflow & Process
		public readonly static ClientModuleIdentifier ImplementationEmails = new ClientModuleIdentifier(EDIModuleId.ImplementationEmails, "Implementation Emails");

		// Training
		public readonly static ClientModuleIdentifier GlbTrainingCourse = new ClientModuleIdentifier(EDIModuleId.GlbTrainingCourse, "Training Schedules");
		public readonly static ClientModuleIdentifier GlbClassroomSessionPublic = new ClientModuleIdentifier(EDIModuleId.GlbClassroomSessionPublic, "Public Classroom Sessions");
		public readonly static ClientModuleIdentifier GlbClassroomSessionCourse = new ClientModuleIdentifier(EDIModuleId.GlbClassroomSessionCourse, "Training Schedule Sessions");
		public readonly static ClientModuleIdentifier GlbClassroomSubject = new ClientModuleIdentifier(EDIModuleId.GlbClassroomSubject, "Training Subjects");
		public readonly static ClientModuleIdentifier TrainingReports = new ClientModuleIdentifier(EDIModuleId.TrainingReports, "Reports", "Reports (Training)");
		public readonly static ClientModuleIdentifier TrainingSurvey = new ClientModuleIdentifier(EDIModuleId.TrainingSurvey, "Training Survey");

		public readonly static ClientModuleIdentifier eRouterCustoms = new ClientModuleIdentifier(EDIModuleId.eRouterCustoms, "eRouter Customs Communication");

		// Maintain > Sales Marketing
		public readonly static ClientModuleIdentifier SalesDocuments = new ClientModuleIdentifier(EDIModuleId.SalesDocuments, "Sales Documents");

		// On Demand Monthly Usage Billing
		public readonly static ClientModuleIdentifier MonthlyUsageBilling = new ClientModuleIdentifier(EDIModuleId.MonthlyUsageBilling, "Billing ODPL");

		public readonly static ClientModuleIdentifier StlBilling = new ClientModuleIdentifier(EDIModuleId.StlBilling, "Billing STL");

		// Maintenance Billing
		public readonly static ClientModuleIdentifier MaintenanceBilling = new ClientModuleIdentifier(EDIModuleId.MaintenanceBilling, "Billing Maintenance");

		// Billing Prices
		public readonly static ClientModuleIdentifier BillingPrices = new ClientModuleIdentifier(EDIModuleId.BillingPrices, "Billing Prices");

		// Device Management
		public readonly static ClientModuleIdentifier ClientDevice = new ClientModuleIdentifier(EDIModuleId.ClientDevice, "LDaaS Devices");
		public readonly static ClientModuleIdentifier ClientDeviceTemplate = new ClientModuleIdentifier(EDIModuleId.ClientDeviceTemplate, "LDaaS Device Models");

		// Product and Licence Management
		public readonly static ClientModuleIdentifier LicenceDatabaseRegistration = new ClientModuleIdentifier(EDIModuleId.LicenceDatabaseRegistration, "Licence Database Registration");
		public readonly static ClientModuleIdentifier SystemUserAccounts = new ClientModuleIdentifier(EDIModuleId.SystemUserAccounts, "System User Accounts");

		// Application Logging
		public readonly static ClientModuleIdentifier ApplicationLogger = new ClientModuleIdentifier(EDIModuleId.ApplicationLogger, "Application Loggers");
		public readonly static ClientModuleIdentifier ApplicationActiveLogger = new ClientModuleIdentifier(EDIModuleId.ApplicationActiveLogger, "Active Application Loggers");

		public static class Section
		{
			public const string Development = "Development";
			public const string CustomerService = "Customer Service";
			public const string Training = "Training";
			public const string SystemAndUserManagement = "System and User Management";
		}

		public static class Subcategory
		{
			public static ModuleTreeLoaderConstant.Entry WTG
			{
				get { return new ModuleTreeLoaderConstant.Entry("WTG", (NoResString)"WiseTech Global", "X"); }
			}
		}
	}
}
