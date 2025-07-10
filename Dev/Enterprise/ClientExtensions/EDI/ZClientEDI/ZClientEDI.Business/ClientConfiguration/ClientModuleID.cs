
namespace Enterprise.Client.EDI.Modules
{
	public enum EDIModuleId
	{
		NewWorkItem,
		ProfessionalServicesQuote,
		IssueManager,
		UserAgreements,
		UserAgreementAcceptances,
#if DEBUG
		GlbReleaseNote,
#endif
		WorkItemsReports,
		DevelopmentDashboards,

		// Customer Service
		SupportIncident,
		CustSvcReports,
		Organisations,
		DbRestoreKey,
		EDICustomerServiceEmails,
		CustSvcDashboards,
		IncidentManagementGroups,
		IdentityCertificate,
		TokenAuthenticationOnBoarding,
		EdiIdentityApplication,
		EdiIdentityTenant,
		IncidentTriage,
		IncidentTriageChecklistItem,
		IncidentDiagnosticCriteria,
		InvestigationItem,
		FeatureControl,
		FeatureSet,

		// Projects / Upgrade
		ImplementationEmails,
		LicenceEnterprise,
		LicenceDatabase,
		EdiTrustedMessagingConfig,
		EdiTrustedSystem,
		LicenceHeader,
		ReleaseBuild,
		InstallReports,
		LicenseKey,

		// Training
		GlbTrainingCourse,
		GlbClassroomSessionPublic,
		GlbClassroomSessionCourse,
		GlbClassroomSubject,
		TrainingSurvey,
		TrainingReports,

		eRouterCustoms,

		// Projects
		ProjectItem,
		ProjReports,
		ChangeRequest,

		// Sales Documents
		SalesDocuments,

		// Auto Billing
		ODPLUsageBilling,
		MonthlyUsageBilling,
		MaintenanceBilling,
		StlBilling,
		BillingPrices,

		// Devices
		ClientDevice,
		ClientDeviceTemplate,

		//Product & Licence Management
		LicenceDatabaseRegistration,
		SystemUserAccounts,

		// Application Logging
		ApplicationLogger,
		ApplicationActiveLogger,
	}
}
