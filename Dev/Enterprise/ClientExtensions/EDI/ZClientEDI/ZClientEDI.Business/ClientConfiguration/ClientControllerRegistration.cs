using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.Modules
{
	public static class ClientControllerRegistration
	{
		public readonly static ClientControllerID IssueManager = new ClientControllerID("IssueManager");
		public readonly static ClientControllerID UserAgreements = new ClientControllerID("UserAgreements");
		public readonly static ClientControllerID UserAgreementAcceptances = new ClientControllerID("UserAgreementAcceptances");
		public readonly static ClientControllerID EdiUserAgreementAssignment = new ClientControllerID("EdiUserAgreementAssignment");
		public readonly static ClientControllerID GlbClassroomSessionPublic = new ClientControllerID("GlbClassroomSessionPublic");
		public readonly static ClientControllerID GlbClassroomSessionCourse = new ClientControllerID("GlbClassroomSessionCourse");
		public readonly static ClientControllerID ObsoleteNonGenericWorkItemForOldHyperlinksOnly = new ClientControllerID("NewWorkItem");
		public readonly static ClientControllerID EdiIncidentRequest = new ClientControllerID("EdiIncidentRequest");
		public readonly static ClientControllerID SupportIncident = new ClientControllerID("SupportIncident");
		public readonly static ClientControllerID IncidentManagementGroup = new ClientControllerID("IncidentManagementGroup");
		public readonly static ClientControllerID IncidentTriage = new ClientControllerID("IncidentTriage");
		public readonly static ClientControllerID IncidentTriageChecklistItem = new ClientControllerID("IncidentTriageChecklistItem");
		public readonly static ClientControllerID IncidentDiagnosticCriteria = new ClientControllerID("IncidentDiagnosticCriteria");
		public readonly static ClientControllerID InvestigationItem = new ClientControllerID("InvestigationItem");
#if DEBUG
		public readonly static ClientControllerID GlbReleaseNote = new ClientControllerID("GlbReleaseNote");
#endif
		public readonly static ClientControllerID ReleaseBuild = new ClientControllerID("ReleaseBuild");
		public readonly static ClientControllerID Organisations = new ClientControllerID("Organisations");
		public readonly static ClientControllerID LicenceEnterprise = new ClientControllerID("LicenceEnterprise");
		public readonly static ClientControllerID LicenseKey = new ClientControllerID("LicenseKey");
		public readonly static ClientControllerID GlbClassroomSubject = new ClientControllerID("GlbClassroomSubject");
		public readonly static ClientControllerID GlbTrainingCourse = new ClientControllerID("GlbTrainingCourse");
		public readonly static ClientControllerID LicenceDatabase = new ClientControllerID("LicenceDatabase");
		public readonly static ClientControllerID EdiTrustedMessagingConfig = new ClientControllerID("EdiTrustedMessagingConfig");
		public readonly static ClientControllerID EdiTrustedSystem = new ClientControllerID("EdiTrustedSystem");
		public readonly static ClientControllerID LicenceHeader = new ClientControllerID("LicenceHeader");
		public readonly static ClientControllerID ProfessionalServicesQuote = new ClientControllerID("ProfessionalServicesQuote");
		public readonly static ClientControllerID Project = new ClientControllerID("EDIProject");
		public readonly static ClientControllerID WiseServicePartnerSurveyPlugIn = new ClientControllerID("WiseServicePartnerSurveyPlugIn");
		public readonly static ClientControllerID DbRestoreKey = new ClientControllerID("DbRestoreKey");
		public readonly static ClientControllerID eRouterCustoms = new ClientControllerID("eRouterCustoms");
		public readonly static ClientControllerID TrainingSurvey = new ClientControllerID("TrainingSurvey");
		public readonly static ClientControllerID SupportIncidentClientOrgLicence = new ClientControllerID("SupportIncidentClientOrgLicence");
		public readonly static ClientControllerID ProjectClientOrgLicence = new ClientControllerID("ProjectClientOrgLicence");
		public readonly static ClientControllerID OpportunityRelatedProjects = new ClientControllerID("OpportunityRelatedProjects");
		public readonly static ClientControllerID OpportunityClientOrgLicence = new ClientControllerID("OpportunityClientOrgLicence");
		public readonly static ClientControllerID ODPLUsageBilling = new ClientControllerID("ODPLUsageBilling");
		public readonly static ClientControllerID MonthlyUsageBilling = new ClientControllerID("MonthlyUsageBilling");
		public readonly static ClientControllerID EngineeringTask = new ClientControllerID("EngineeringTask");
		public readonly static ClientControllerID PSQRelatedOpportunities = new ClientControllerID("PSQRelatedOpportunities");
		public readonly static ClientControllerID EDIOpportunityRelatedPSQs = new ClientControllerID("EDIOpportunityRelatedPSQs");
		public readonly static ClientControllerID MaintenanceBilling = new ClientControllerID("MaintenanceBilling");
		public readonly static ClientControllerID ChargeCodeCommissionConfiguration = new ClientControllerID("ChargeCodeCommissionConfiguration");
		public readonly static ClientControllerID ClientDevice = new ClientControllerID("ClientDevice");
		public readonly static ClientControllerID ClientDeviceTemplate = new ClientControllerID("ClientDeviceTemplate");
		public readonly static ClientControllerID StlBilling = new ClientControllerID("StlBilling");
		public readonly static ClientControllerID BillingPrices = new ClientControllerID("BillingPrices");
		public readonly static ClientControllerID EDIOpportunityValueAnalysis = new ClientControllerID("EDIOpportunityValueAnalysis");
		public readonly static ClientControllerID IncidentEConversationPlugIn = new ClientControllerID("IncidentEConversationPlugIn");
		public readonly static ClientControllerID IncidentManagementEConversationPlugin = new ClientControllerID("IncidentManagementEConversationPlugin");
		public readonly static ClientControllerID LicenceDatabaseRegistration = new ClientControllerID("LicenceDatabaseRegistration");
		public readonly static ClientControllerID SystemUserAccounts = new ClientControllerID("SystemUserAccounts");
		public readonly static ClientControllerID EdiIdentityCertificate = new ClientControllerID("EdiIdentityCertificate");
		public readonly static ClientControllerID TokenAuthenticationOnBoarding = new ClientControllerID("TokenAuthenticationOnBoarding");
		public readonly static ClientControllerID EdiIdentityApplication = new ClientControllerID("EdiIdentityApplication");
		public readonly static ClientControllerID EdiIdentityTenant = new ClientControllerID("EdiIdentityTenant");
		public readonly static ClientControllerID FeatureControl = new ClientControllerID("FeatureControl");
		public readonly static ClientControllerID ApplicationLogger = new ClientControllerID("ApplicationLogger");
		public readonly static ClientControllerID ApplicationActiveLogger = new ClientControllerID("ApplicationActiveLogger");
		public readonly static ClientControllerID FeatureSet = new ClientControllerID("FeatureSet");
	}
}
