using Enterprise.Core.Environment;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI
{
	public class EDISecurityCheckpoints
	{
		#region Constants

		public static class Constants
		{
			public const string OrgLicenceModifyReferenceName = "OrgLicenceModify";
			public const string OrgLicenceModifyDisplayName = "Enterprise Licencing";

			public const string OrgLicenceModifyKeyConfigurationSecurityReferenceName = "OrgLicenceModifyLicenceKey";
			public const string OrgLicenceModifyKeyConfigurationSecurityDisplayName = "Modify Licence Keys";

			public const string OrgLicenceCreateAndEmailKeyConfigurationSecurityReferenceName = "OrgCreateAndEmailLicenceKey";
			public const string OrgLicenceCreateAndEmailKeyConfigurationSecurityDisplayName = "Create & Email Licence Keys";

			public const string OrgLicenceModify3rdPartySoftwareReferenceName = "OrgLicenceModify3rdPartySoftware";
			public const string OrgLicenceModify3rdPartySoftwareDisplayName = "Modify 3rd Party Software";

			public const string OrgLicenceModifyInstallationDetailsSecurityReferenceName = "OrgLicenceModifyInstallationDetails";
			public const string OrgLicenceModifyInstallationDetailsSecurityDisplayName = "Modify Installation Details";

			public const string OrgLicenceModifyAgreedGoLiveSecurityReferenceName = "OrgLicenceModifyAgreedGoLive";
			public const string OrgLicenceModifyAgreedGoLiveSecurityDisplayName = "Modify Agreed Go-Live";

			public const string OrgLicenceModifyConnectionDetailsSecurityReferenceName = "OrgLicenceModifyConnectionDetails";
			public const string OrgLicenceModifyConnectionDetailsSecurityDisplayName = "Modify Connection Details";

			public const string OrgLicenceModifySupportAndContractDetailsReferenceName = "OrgLicenceModifySupportAndContractDetails";
			public const string OrgLicenceModifySupportAndContractDetailsDisplayName = "Modify Support And Contract Details";

			public const string OrgLicenceModifyDatabaseDetailsReferenceName = "OrgLicenceModifyDatabaseConfiguration";
			public const string OrgLicenceModifyDatabaseDetailsDisplayName = "Modify Database Details";

			public const string OrgLicenceModifySendUpgradeSecurityReferenceName = "OrgLicenceModifySendUpgrade";
			public const string OrgLicenceModifySendUpgradeSecurityDisplayName = "Send Upgrade Packages";

			public const string OrgLicenceModifySaveUpgradeToDiskReferenceName = "OrgLicenceModifySaveUpgradeToDisk";
			public const string OrgLicenceModifySaveUpgradeToDiskDisplayName = "Save Upgrade Packages To Disk";

			public const string OrgLicenceModifySendSupersededUpgradeReferenceName = "OrgLicenceModifySendSupersededUpgrade";
			public const string OrgLicenceModifySendSupersededUpgradeDisplayName = "Send Superseded Upgrade";

			public const string OrgLicenceModifySendUpgradeToHigherRingDBCheckpointReferenceName = "OrgLicenceModifySendUpgradeToHigherRingDB";
			public const string OrgLicenceModifySendUpgradeToHigherRingDBCheckpointDisplayName = "To Client DBs on a Higher Ring than the Package";

			public const string OrgLicenceModifySendUpgradeToDBNotOnLowestSupportedSqlVersionCheckpointReferenceName = "OrgLicenceModifySendUpgradeToDBSqlLowerThanMin";
			public const string OrgLicenceModifySendUpgradeToDBNotOnLowestSupportedSqlVersionCheckpointDisplayName = "To Client DBs that are not on the lowest supported SQL Server version";

			public const string OrgLicenceModifySetDatabaseSecurityModeToOpenReferenceName = "OrgLicenceModifySetDatabaseSecurityModeToOpen";
			public const string OrgLicenceModifySetDatabaseSecurityModeToOpenDisplayName = "Set Database Server Security Mode to Open";

			public const string SupportEmailsReferenceName = "SupportEmails";
			public const string SupportEmailsDisplayName = "Support Email System";

			public const string ImplementationEmailsReferenceName = "ImplementationEmails";
			public const string ImplementationEmailsDisplayName = "Implementation Email System";

			public const string OrgLicenceRemoveLicenceDatabaseSecurityReferenceName = "OrgLicenceRemoveLicenceDatabase";
			public const string OrgLicenceRemoveLicenceDatabaseSecurityDisplayName = "Remove Licence Database";

			public const string OrgLicenceMoveLicencesToNewEnterpriseSecurityReferenceName = "OrgLicenceMoveLicencesToNewEnterpriseCode";
			public const string OrgLicenceMoveLicencesToNewEnterpriseSecurityDisplayName = "Move Licences To New Enterprise";

			public const string OrgLicenceModifyLicDatabaseModifyToHigherRingReferenceName = "OrgLicenceModifyLicDatabaseModifyToHigherRing";
			public const string OrgLicenceModifyLicDatabaseModifyToHigherRingDisplayName = "Modify Licence Database to a Higher Ring";

			public const string OrgLicenceModifyLicDatabaseModifyToRestrictedRingReferenceName = "OrgLicenceModifyLicDatabaseModifyToRestrictedRing";
			public const string OrgLicenceModifyLicDatabaseModifyToRestrictedRingDisplayName = "Modify Licence Database to a Restricted Ring";

			public const string OrgLicenceBillingReferenceName = "OrgLicenceBilling";
			public const string OrgLicenceBillingDisplayName = "Modify Licence Billing";

			public const string OrgLicenceModifyExchangeRatesAndTaxReferenceName = "OrgLicenceModifyExchangeRatesAndTax";
			public const string OrgLicenceModifyExchangeRatesAndTaxDisplayName = "Modify Exchange Rates and Tax";

			#region Commission

			public const string CommissionResolveAmbiguityReferenceName = "CommissionResolveAmbiguity";
			public const string CommissionResolveAmbiguityDisplayName = "Resolve Ambiguity";

			#endregion

			#region Customer Service

			public const string CustomerServiceReferenceName = "CustomerService";
			public const string CustomerServiceDisplayName = "Customer Service";

			public const string CustomerServiceMuteOutboundEmailNotificationsReferenceName = "CustomerServiceMuteOutboundEmailNotifications";
			public const string CustomerServiceMuteOutboundEmailNotificationsDisplayName = "Mute Outbound Email Notifications";

			public const string CustomerServiceIncidentReferenceName = "CustomerServiceIncident";
			public const string CustomerServiceIncidentDisplayName = "Incident";
			public const string CustomerServiceIncidentNewReferenceName = "CustomerServiceIncidentNew";
			public const string CustomerServiceIncidentEditReferenceName = "CustomerServiceIncidentEdit";
			public const string CustomerServiceIncidentViewReferenceName = "CustomerServiceIncidentView";

			public const string CustomerServiceIncidentEditDefectMgmtReferenceName = "CustomerServiceIncidentEditDefectMgmt";
			public const string CustomerServiceIncidentEditDefectMgmtDisplayName = "Modify Defect Management Details";
			public const string CustomerServiceIncidentEditFeatureMgmtReferenceName = "CustomerServiceIncidentEditFeatureMgmt";
			public const string CustomerServiceIncidentEditFeatureMgmtDisplayName = "Modify Feature Request Management Details";

			public const string CustomerServiceIncidentJobInvoicingReferenceName = "CustomerServiceIncidentJobInv";
			public const string CustomerServiceIncidentJobInvoicingDisplayName = "Billing";
			public const string CustomerServiceIncidentAuditBillingReferenceName = "CustomerServiceIncidentAuditBilling";
			public const string CustomerServiceIncidentAuditBillingDisplayName = "Audit Billing";

			public const string CustomerServiceIncidentAllowSendEConvAndCloseIncidentReferenceName = "CustomerServiceIncidentAllowSendEConv";
			public const string CustomerServiceIncidentAllowSendEConvAndCloseIncidentDisplayName = "Allow Send eConversation/Close Incident without working task";

			public const string OrganisationAllowSearchOutsideLoginCountryReferenceName = "CSOrganisationAllowSearchOutsideLoginCountry";
			public const string OrganisationAllowSearchOutsideLoginCountryDisplayName = "Allow Search of Organizations Outside Login Country";

			public const string CustomerServiceIncidentManagementGroupDisplayName = "Incident Management Group";
			public const string CustomerServiceIncidentManagementGroupReferenceName = "CustomerServiceIncidentManagementGroup";
			public const string CustomerServiceIncidentManagementGroupNewReferenceName = "CustomerServiceIncidentManagementGroupNew";
			public const string CustomerServiceIncidentManagementGroupEditReferenceName = "CustomerServiceIncidentManagementGroupEdit";
			public const string CustomerServiceIncidentManagementGroupViewReferenceName = "CustomerServiceIncidentManagementGroupView";

			public const string CustomerServiceIncidentTriageDisplayName = "Incident Triage";
			public const string CustomerServiceIncidentTriageReferenceName = "CustomerServiceIncidentTriage";
			public const string CustomerServiceIncidentTriageNewReferenceName = "CustomerServiceIncidentTriageNew";
			public const string CustomerServiceIncidentTriageEditReferenceName = "CustomerServiceIncidentTriageEdit";
			public const string CustomerServiceIncidentTriageViewReferenceName = "CustomerServiceIncidentTriageView";
			public const string CustomerServiceIncidentTriagePublishAccessDisplayName = "Publish Access";
			public const string CustomerServiceIncidentTriagePublishAccessReferenceName = "CustomerServiceIncidentTriagePublishAccess";

			public const string CustomerServiceIncidentTriageChecklistsDisplayName = "Checklist Items";
			public const string CustomerServiceIncidentTriageChecklistsReferenceName = "CustomerServiceIncidentTriageChecklists";
			public const string CustomerServiceIncidentTriageChecklistsNewReferenceName = "CustomerServiceIncidentTriageChecklistsNew";
			public const string CustomerServiceIncidentTriageChecklistsEditReferenceName = "CustomerServiceIncidentTriageChecklistsEdit";
			public const string CustomerServiceIncidentTriageChecklistsViewReferenceName = "CustomerServiceIncidentTriageChecklistsView";

			public const string CustomerServiceIncidentDiagnosticCriteriaDisplayName = "Diagnostic Criteria";
			public const string CustomerServiceIncidentDiagnosticCriteriaReferenceName = "CustomerServiceIncidentDiagnosticCriteria";
			public const string CustomerServiceIncidentDiagnosticCriteriaNewReferenceName = "CustomerServiceIncidentDiagnosticCriteriaNew";
			public const string CustomerServiceIncidentDiagnosticCriteriaEditReferenceName = "CustomerServiceIncidentDiagnosticCriteriaEdit";
			public const string CustomerServiceIncidentDiagnosticCriteriaViewReferenceName = "CustomerServiceIncidentDiagnosticCriteriaView";

			public const string CustomerServiceInvestigationItemDisplayName = "Investigation Item";
			public const string CustomerServiceInvestigationItemReferenceName = "CustomerServiceInvestigationItem";
			public const string CustomerServiceInvestigationItemNewReferenceName = "CustomerServiceInvestigationItemNew";
			public const string CustomerServiceInvestigationItemEditReferenceName = "CustomerServiceInvestigationItemEdit";
			public const string CustomerServiceInvestigationItemViewReferenceName = "CustomerServiceInvestigationItemView";

			public const string ReportsReferenceName = "CustomerServiceReports";

			public const string EdiIdentityCertificateReferenceName = "SystemIdentityCertificates";
			public const string EdiIdentityCertificateNewReferenceName = "SystemIdentityCertificatesNew";
			public const string EdiIdentityCertificateDisplayName = "System Identity Certificates";
			public const string EdiIdentityCertificateDownloadCertificateReferenceName = "SystemIdentityCertificatesDownloadCertificate";
			public const string EdiIdentityCertificateDownloadCertificateDisplayName = "Download Certificate";
			public const string EdiIdentityCertificateEditCertificateReferenceName = "SystemIdentityCertificatesEditCertificate";
			public const string EdiIdentityCertificateEditCertificateDisplayName = "Edit Certificate";

			public const string TokenAuthenticationOnBoardingReferenceName = "TokenAuthenticationOnBoarding";
			public const string TokenAuthenticationOnBoardingDisplayName = "Token Authentication Onboarding";
			public const string TokenAuthenticationOnBoardingCopyPasswordReferenceName = "TokenAuthenticationOnBoardingCopyPassword";
			public const string TokenAuthenticationOnBoardingCopyPasswordDisplayName = "CopyPassword";

			public const string EdiIdentityApplicationReferenceName = "SystemIdentityApplication";
			public const string EdiIdentityApplicationDisplayName = "System Identity Application";
			public const string EdiIdentityApplicationNewReferenceName = "SystemIdentityApplicationNew";
			public const string EdiIdentityApplicationEditReferenceName = "SystemIdentityApplicationEdit";

			public const string EdiIdentityTenantReferenceName = "SystemIdentityTenant";
			public const string EdiIdentityTenantDisplayName = "System Identity Tenant";
			public const string EdiIdentityTenantNewReferenceName = "SystemIdentityTenantNew";
			public const string EdiIdentityTenantEditReferenceName = "SystemIdentityTenantEdit";

			#endregion

			#region Installation & Upgrades

			public const string InstallationTaskReferenceName = "InstallationTask";
			public const string InstallationTaskDisplayName = "Implementation Task";

			public const string InstallationTaskJobInvoicingReferenceName = "InstallationTaskJobInvoicing";
			public const string InstallationTaskJobInvoicingDisplayName = "Billing";
			public const string InstallationTaskAuditBillingReferenceName = "InstallationTaskAuditBilling";
			public const string InstallationTaskAuditBillingDisplayName = "Audit Billing";

			public const string ReleaseBuildReferenceName = "ReleaseBuild";
			public const string ReleaseBuildDisplayName = "Release Build";

			public const string EdiTrustedMessagingConfigReferenceName = "TrustedMessagingConfig";
			public const string EdiTrustedMessagingConfigDisplayName = "Trusted Messaging Configuration";

			public const string EdiTrustedSystemReferenceName = "TrustedSystems";
			public const string EdiTrustedSystemDisplayName = "Trusted Systems";

			#endregion

			#region Development

			public const string DevelopmentReferenceName = "Development";
			public const string DevelopmentDisplayName = "Development";

			public const string ProfessionalServicesQuoteReferenceName = "ProfessionalServicesQuote";
			public const string ProfessionalServicesQuoteDisplayName = "Professional Services Quote";
			public const string ProfessionalServicesQuoteNewReferenceName = "ProfessionalServicesQuoteNew";
			public const string ProfessionalServicesQuoteEditReferenceName = "ProfessionalServicesQuoteEdit";
			public const string ProfessionalServicesQuoteDeleteReferenceName = "ProfessionalServicesQuoteDelete";
			public const string ProfessionalServicesQuoteJobInvoicingReferenceName = "ProfessionalServicesQuoteJobInv";
			public const string ProfessionalServicesQuoteJobInvoicingDisplayName = "Billing";
			public const string ProfessionalServicesQuoteAuditBillingReferenceName = "ProfessionalServicesQuoteAuditBilling";
			public const string ProfessionalServicesQuoteAuditBillingDisplayName = "Audit Billing";

			public const string IssueManagerReferenceName = "IssueManager";
			public const string IssueManagerDisplayName = "Issue Manager";
			public const string IssueManagerMergeToolReferenceName = "IssueManagerMergeTool";
			public const string IssueManagerMergeToolDisplayName = "Merge";
			public const string IssueManagerReprocessToolReferenceName = "IssueManagerReprocessTool";
			public const string IssueManagerReprocessToolDisplayName = "Reprocess";

			public const string UserAgreementsReferenceName = "UserAgreements";
			public const string UserAgreementsNewReferenceName = "UserAgreementsNew";
			public const string UserAgreementsEditReferenceName = "UserAgreementsEdit";
			public const string UserAgreementsDeleteReferenceName = "UserAgreementsDelete";
			public const string UserAgreementsAcceptForOrganisationReferenceName = "UserAgreementsAcceptForOrganisation";
			public const string UserAgreementsAcceptForOrganisationDisplayName = "Accept For Organisation";
			public const string UserAgreementsDisplayName = "User Agreements";

			public const string UserAgreementAddAcceptanceLogsReferenceName = "UserAgreementAddAcceptanceLog";
			public const string UserAgreementAddAcceptanceLogsDisplayName = "Add acceptance";
			public const string UserAgreementAssigCorporateAgreementReferenceName = "UserAgreementAssigCorporateAgreement";
			public const string UserAgreementAssigCorporateAgreementDisplayName = "Assign Corporate Agreement";

			public const string UserAgreementAcceptanceLogsReferenceName = "UserAgreementAcceptanceLogs";
			public const string UserAgreementAcceptanceLogsDisplayName = "User Agreement Acceptance Logs";

			public const string DevelopmentReportsReferenceName = "DevelopmentReports";
			public const string DevelopmentReportsReferenceDisplayName = "Reports";

			public const string FeatureControlDisplayName = "Feature Control";
			public const string FeatureControlReferenceName = "FeatureControl";
			public const string FeatureControlNewReferenceName = "FeatureControlNew";
			public const string FeatureControlEditReferenceName = "FeatureControlEdit";
			public const string FeatureControlViewReferenceName = "FeatureControlView";

			public const string FeatureSetDisplayName = "Feature Set";
			public const string FeatureSetReferenceName = "FeatureSet";
			public const string FeatureSetNewReferenceName = "FeatureSetNew";
			public const string FeatureSetEditReferenceName = "FeatureSetEdit";
			public const string FeatureSetViewReferenceName = "FeatureSetView";
			public const string FeatureAdminReferenceName = "FeatureAdmin";

			public const string AllowFeatureEditForAnyReleaseGroupDisplayName = "Allow feature edit for any release group";
			public const string AllowFeatureEditForAnyReleaseGroupReferenceName = "AllowFeatureEditForAnyReleaseGroup";

			#endregion

			#region Devices

			public const string DeviceTemplatesReferenceName = "DeviceTemplates";
			public const string DeviceTemplatesDisplayName = "Telematics Device Models";
			public const string DeviceTemplatesNewReferenceName = "DeviceTemplatesNew";
			public const string DeviceTemplatesEditReferenceName = "DeviceTemplatesEdit";
			public const string DeviceTemplatesViewReferenceName = "DeviceTemplatesView";
			public const string DeviceTemplatesDeleteReferenceName = "DeviceTemplatesDelete";

			public const string DevicesReferenceName = "Devices";
			public const string DevicesDisplayName = "Telematics Devices";
			public const string DevicesNewReferenceName = "DevicesNew";
			public const string DevicesEditReferenceName = "DevicesEdit";
			public const string DevicesViewReferenceName = "DevicesView";
			public const string DevicesDeleteReferenceName = "DevicesDelete";
			public const string DevicesDeleteOperationalActionsReferenceName = "DevicesOperationalActions";
			public const string DevicesOperationalActionsCustomizationReferenceName = "DevicesOperationalActionsCustomization";
			public const string DevicesOperationalActionsExecutionReferenceName = "DevicesOperationalActionsExecution";

			#endregion

			#region System and User Management

			public const string SystemAndUserManagementReferenceName = "SystemAndUserManagement";
			public const string SystemAndUserManagementDisplayName = "System and User Management";

			public const string LicenceDatabaseRegistrationReferenceName = "LicenceDatabaseRegistration";
			public const string LicenceDatabaseRegistrationDisplayName = "Licence Database Registration";

			//SystemUserAccounts
			public const string SystemUserAccountsReferenceName = "SystemUserAccounts";
			public const string SystemUserAccountsDisplayName = "System User Accounts";
			public const string SystemUserAccountsEditReferenceName = "SystemUserAccountsEdit";
			public const string SystemUserAccountsViewReferenceName = "SystemUserAccountsView";

			public const string GlowSupportLogonToInternalNonProductionSystem = "GlowSupportLogonToInternalNonProductionSystem";
			public const string GlowSupportLogonToInternalNonProductionSystemDisplayName = "Glow Support Log In to Internal Non-Production Systems";

			public const string GlowSupportLogonToInternalProductionSystem = "GlowSupportLogonToInternalProductionSystem";
			public const string GlowSupportLogonToInternalProductionSystemDisplayName = "Glow Support Log In to Internal Production Systems";

			public const string GlowSupportLogonToExternalNonProductionSystem = "GlowSupportLogonToExternalNonProductionSystem";
			public const string GlowSupportLogonToExternalNonProductionSystemDisplayName = "Glow Support Log In to External Non-Production Systems";

			public const string GlowSupportLogonToExternalProductionSystem = "GlowSupportLogonToExternalProductionSystem";
			public const string GlowSupportLogonToExternalProductionSystemDisplayName = "Glow Support Log In to External Production Systems";

			public const string GlowSupportLogonToInternalNonProductionSystemAsSuperUser = "GlowSupportLogonToInternalNonProductionSystemAsSuperUser";
			public const string GlowSupportLogonToInternalNonProductionSystemAsSuperUserDisplayName = "Glow Support Log In to Internal Non-Production Systems As Super User (CWSupport)";

			public const string GlowSupportLogonToInternalNonProductionSystemAsDiagnosticsUser = "GlowSupportLogonToInternalNonProductionSystemAsDiagnosticsUser";
			public const string GlowSupportLogonToInternalNonProductionSystemAsDiagnosticsUserDisplayName = "Glow Support Log In to Internal Non-Production Systems As Diagnostics User (GlowSupport)";

			public const string GlowSupportLogonToInternalProductionSystemAsSuperUser = "GlowSupportLogonToInternalProductionSystemAsSuperUser";
			public const string GlowSupportLogonToInternalProductionSystemAsSuperUserDisplayName = "Glow Support Log In to Internal Production Systems As Super User (CWSupport)";

			public const string GlowSupportLogonToInternalProductionSystemAsDiagnosticsUser = "GlowSupportLogonToInternalProductionSystemAsDiagnosticsUser";
			public const string GlowSupportLogonToInternalProductionSystemAsDiagnosticsUserDisplayName = "Glow Support Log In to Internal Production Systems As Diagnostics User (GlowSupport)";

			public const string GlowSupportLogonToExternalNonProductionSystemAsSuperUser = "GlowSupportLogonToExternalNonProductionSystemAsSuperUser";
			public const string GlowSupportLogonToExternalNonProductionSystemAsSuperUserDisplayName = "Glow Support Log In to External Non-Production Systems As Super User (CWSupport)";

			public const string GlowSupportLogonToExternalNonProductionSystemAsDiagnosticsUser = "GlowSupportLogonToExternalNonProductionSystemAsDiagnosticsUser";
			public const string GlowSupportLogonToExternalNonProductionSystemAsDiagnosticsUserDisplayName = "Glow Support Log In to External Non-Production Systems As Diagnostics User (GlowSupport)";

			public const string GlowSupportLogonToExternalProductionSystemAsSuperUser = "GlowSupportLogonToExternalProductionSystemAsSuperUser";
			public const string GlowSupportLogonToExternalProductionSystemAsSuperUserDisplayName = "Glow Support Log In to External Production Systems As Super User (CWSupport)";

			public const string GlowSupportLogonToExternalProductionSystemAsDiagnosticsUser = "GlowSupportLogonToExternalProductionSystemAsDiagnosticsUser";
			public const string GlowSupportLogonToExternalProductionSystemAsDiagnosticsUserDisplayName = "Glow Support Log In to External Production Systems As Diagnostics User (GlowSupport)";

			public const string SendGitHubInviteForOthers = "SendGitHubInviteForOthers";
			public const string SendGitHubInviteForOthersDisplayName = "Send GitHub Invite For Others";

			#endregion

			#region Work Item

			public const string WorkItemEConversationReferenceName = "WorkItemEConversation";
			public const string WorkItemEConversationDisplayName = "eConversation";
			public const string WorkItemEConversationAddInternalCommentReferenceName = "WorkItemEConversationAddInternalComment";
			public const string WorkItemEConversationAddInternalCommentDisplayName = "Add Internal Comment";
			public const string WorkItemEConversationSendMessagesReferenceName = "WorkItemEConversationSendMessages";
			public const string WorkItemEConversationSendMessagesDisplayName = "Send Messages";
			public const string WorkItemEConversationBroadcastReferenceName = "WorkItemEConversationBroadcast";
			public const string WorkItemEConversationBroadcastDisplayName = "Broadcast";

			#endregion

			#region Avalara US Sales Tax

			public const string AvalaraUSSalesTaxReceivablesReferenceName = "AvalaraUSSalesTaxReceivables";
			public const string AvalaraUSSalesTaxReceivablesRequestSalesTaxCalculationReferenceName = "AvalaraUSSalesTaxReceivablesRequestSalesTaxCalculation";
			public const string AvalaraUSSalesTaxReceivablesResubmitSalesTaxTransactionReferenceName = "AvalaraUSSalesTaxReceivablesResubmitSalesTaxTransaction";

			public const string AvalaraUSSalesTaxDisplayName = "Avalara US Sales Tax Integration";
			public const string AvalaraUSSalesTaxRequestSalesTaxCalulationDisplayName = "Request Sales Tax Calculation";
			public const string AvalaraUSSalesTaxResubmitSalesTaxTransactionDisplayName = "Resubmit Sales Tax Transaction to Avalara";

			#endregion

			#region Application Logging

			public const string ApplicationLoggingApplicationLoggerReferenceName = "ApplicationLoggers";
			public const string ApplicationLoggingApplicationLoggerDisplayName = "Application Loggers";

			public const string ApplicationLoggingApplicationLoggerViewReferenceName = "ApplicationLoggerViewLoggers";
			public const string ApplicationLoggingApplicationLoggerViewDisplayName = "View Application Loggers";

			public const string ApplicationLoggingApplicationLoggerEditReferenceName = "ApplicationLoggerEditLoggers";
			public const string ApplicationLoggingApplicationLoggerEditDisplayName = "Edit Application Loggers";

			public const string ApplicationLoggingApplicationLoggerNewReferenceName = "ApplicationLoggerCreateLoggers";
			public const string ApplicationLoggingApplicationLoggerNewDisplayName = "Create Application Loggers";

			public const string ApplicationLoggingApplicationLoggerDeleteReferenceName = "ApplicationLoggerDeleteLoggers";
			public const string ApplicationLoggingApplicationLoggerDeleteDisplayName = "Delete Application Loggers";

			public const string ApplicationLoggingApplicationActiveLoggerReferenceName = "ApplicationActiveLoggers";
			public const string ApplicationLoggingApplicationActiveLoggerDisplayName = "Application Active Loggers";

			public const string ApplicationLoggingApplicationActiveLoggerViewReferenceName = "ApplicationActiveLoggerViewLoggers";
			public const string ApplicationLoggingApplicationActiveLoggerViewDisplayName = "View Application Active Loggers";

			public const string ApplicationLoggingApplicationActiveLoggerEditReferenceName = "ApplicationActiveLoggerEditLoggers";
			public const string ApplicationLoggingApplicationActiveLoggerEditDisplayName = "Edit Application Active Loggers";

			public const string ApplicationLoggingApplicationActiveLoggerNewReferenceName = "ApplicationActiveLoggerCreateLoggers";
			public const string ApplicationLoggingApplicationActiveLoggerNewDisplayName = "Create Application Active Loggers";

			public const string ApplicationLoggingApplicationActiveLoggerDeleteReferenceName = "ApplicationActiveLoggerDeleteLoggers";
			public const string ApplicationLoggingApplicationActiveLoggerDeleteDisplayName = "Delete Application Active Loggers";

			#endregion

			#region STL Billing Admin

			public const string STLBillingReferenceName = "STLBilling";
			public const string STLBillingAdminFunctionReferenceName = "STLBillingAdminFunction";

			public const string STLBillingDisplayName = "STL Billing";
			public const string STLBillingAdminFunctionDisplayName = "Admin functions";

			#endregion

		}

		#endregion

		#region Licencing Top-Level Checkpoint

		public static SecurityCheckpoint OrgLicenceModify
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.OrgLicenceModifyReferenceName)); }
		}

		public SecurityCheckpoint AddOrgLicenceModifySecurityCheckpoint(IZSecurity securityInstance)
		{
			SecurityCore securityDefinitions = ((SecurityCore)securityInstance);
			SecurityCheckpoint result = securityDefinitions.AddClientSpecificSecurityCheckpoint(
				Constants.OrgLicenceModifyReferenceName,
				(NoResString)Constants.OrgLicenceModifyDisplayName,
				securityDefinitions.Organisation
			);

			return result;
		}

		#endregion

		#region Support Email System

		public static SecurityCheckpoint SupportEmails
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.SupportEmailsReferenceName)); }
		}

		public SecurityCheckpoint AddSupportEmailsSecurityCheckpoint(IZSecurity securityInstance)
		{
			SecurityCore securityDefinitions = ((SecurityCore)securityInstance);
			SecurityCheckpoint result = securityDefinitions.AddClientSpecificSecurityCheckpoint(
				 Constants.SupportEmailsReferenceName,
				 (NoResString)Constants.SupportEmailsDisplayName,
			securityDefinitions.Emails);

			return result;
		}
		#endregion

		#region Licence Key Modification Checkpoint

		public static SecurityCheckpoint OrgLicenceModifyLicenceKey
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.OrgLicenceModifyKeyConfigurationSecurityReferenceName)); }
		}

		public SecurityCheckpoint AddOrgLicenceModifyLicenceKeySecurityCheckpoint(IZSecurity securityInstance)
		{
			SecurityCore securityDefinitions = ((SecurityCore)securityInstance);
			SecurityCheckpoint result = securityDefinitions.AddClientSpecificSecurityCheckpoint(
				Constants.OrgLicenceModifyKeyConfigurationSecurityReferenceName,
				(NoResString)Constants.OrgLicenceModifyKeyConfigurationSecurityDisplayName,
				securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.OrgLicenceModifyReferenceName))
			);

			return result;
		}

		#endregion

		#region Licence Key Create & Email Checkpoint

		public static SecurityCheckpoint OrgLicenceCreateAndEmailLicenceKey
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.OrgLicenceCreateAndEmailKeyConfigurationSecurityReferenceName)); }
		}

		public SecurityCheckpoint AddOrgLicenceCreateAndEmailLicenceKeySecurityCheckpoint(IZSecurity securityInstance)
		{
			SecurityCore securityDefinitions = ((SecurityCore)securityInstance);
			SecurityCheckpoint result = securityDefinitions.AddClientSpecificSecurityCheckpoint(
				Constants.OrgLicenceCreateAndEmailKeyConfigurationSecurityReferenceName,
				(NoResString)Constants.OrgLicenceCreateAndEmailKeyConfigurationSecurityDisplayName,
				securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.OrgLicenceModifyReferenceName))
			);

			return result;
		}

		#endregion

		#region 3rd Party Software

		public static SecurityCheckpoint OrgLicenceModify3rdPartySoftware
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.OrgLicenceModify3rdPartySoftwareReferenceName)); }
		}

		public SecurityCheckpoint AddOrgLicenceModifyLicence3rdPartySoftware(IZSecurity securityInstance)
		{
			SecurityCore securityDefinitions = ((SecurityCore)securityInstance);
			SecurityCheckpoint result = securityDefinitions.AddClientSpecificSecurityCheckpoint(
				Constants.OrgLicenceModify3rdPartySoftwareReferenceName,
				(NoResString)Constants.OrgLicenceModify3rdPartySoftwareDisplayName,
				securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.OrgLicenceModifyReferenceName))
				);

			return result;
		}

		#endregion

		#region InstallationDetails CheckPoint

		public static SecurityCheckpoint OrgLicenceModifyInstallationDetails
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.OrgLicenceModifyInstallationDetailsSecurityReferenceName)); }
		}

		public SecurityCheckpoint AddOrgLicenceModifyInstallationDetailsSecurityCheckPoint(IZSecurity securityInstance)
		{
			SecurityCore securityDefinitions = ((SecurityCore)securityInstance);
			SecurityCheckpoint result = securityDefinitions.AddClientSpecificSecurityCheckpoint(
				Constants.OrgLicenceModifyInstallationDetailsSecurityReferenceName,
				(NoResString)Constants.OrgLicenceModifyInstallationDetailsSecurityDisplayName,
				securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.OrgLicenceModifyReferenceName))
				);

			return result;
		}

		#endregion

		#region Agreed Go-live CheckPoint

		public static SecurityCheckpoint OrgLicenceModifyAgreedGoLive
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.OrgLicenceModifyAgreedGoLiveSecurityReferenceName)); }
		}

		public SecurityCheckpoint AddOrgLicenceModifyAgreedGoLiveSecurityCheckPoint(IZSecurity securityInstance)
		{
			SecurityCore securityDefinitions = ((SecurityCore)securityInstance);
			SecurityCheckpoint result = securityDefinitions.AddClientSpecificSecurityCheckpoint(
				Constants.OrgLicenceModifyAgreedGoLiveSecurityReferenceName,
				(NoResString)Constants.OrgLicenceModifyAgreedGoLiveSecurityDisplayName,
				securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.OrgLicenceModifyInstallationDetailsSecurityReferenceName))
				);

			return result;
		}

		#endregion

		#region ConnectionDetails CheckPoint

		public static SecurityCheckpoint OrgLicenceModifyConnectionDetails
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.OrgLicenceModifyConnectionDetailsSecurityReferenceName)); }
		}

		public SecurityCheckpoint AddOrgLicenceModifyConnectionDetailsSecurityCheckPoint(IZSecurity securityInstance)
		{
			SecurityCore securityDefinitions = ((SecurityCore)securityInstance);
			SecurityCheckpoint result = securityDefinitions.AddClientSpecificSecurityCheckpoint(
				Constants.OrgLicenceModifyConnectionDetailsSecurityReferenceName,
				(NoResString)Constants.OrgLicenceModifyConnectionDetailsSecurityDisplayName,
				securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.OrgLicenceModifyReferenceName))
				);

			return result;
		}

		#endregion

		#region SupportAndContractDetails checkpoint

		public static SecurityCheckpoint OrgLicenceModifySupportAndContractDetails
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.OrgLicenceModifySupportAndContractDetailsReferenceName)); }
		}

		public SecurityCheckpoint AddOrgLicenceModifySupportAndContractDetailsCheckpoint(IZSecurity securityInstance)
		{
			SecurityCore securityDefinitions = ((SecurityCore)securityInstance);
			SecurityCheckpoint result = securityDefinitions.AddClientSpecificSecurityCheckpoint(
				Constants.OrgLicenceModifySupportAndContractDetailsReferenceName,
				(NoResString)Constants.OrgLicenceModifySupportAndContractDetailsDisplayName,
				securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.OrgLicenceModifyReferenceName))
			);

			return result;
		}

		#endregion

		#region ExchangeRatesAndTax checkpoint

		public static SecurityCheckpoint OrgLicenceModifyExchangeRatesAndTax
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.OrgLicenceModifyExchangeRatesAndTaxReferenceName)); }
		}

		public SecurityCheckpoint AddOrgLicenceModifyExchangeRatesAndTaxCheckpoint(IZSecurity securityInstance)
		{
			SecurityCore securityDefinitions = ((SecurityCore)securityInstance);
			SecurityCheckpoint result = securityDefinitions.AddClientSpecificSecurityCheckpoint(
				Constants.OrgLicenceModifyExchangeRatesAndTaxReferenceName,
				(NoResString)Constants.OrgLicenceModifyExchangeRatesAndTaxDisplayName,
				securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.OrgLicenceModifyReferenceName))
			);

			return result;
		}

		#endregion

		#region Database Details Checkpoint

		public static SecurityCheckpoint OrgLicenceModifyDatabaseDetails
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.OrgLicenceModifyDatabaseDetailsReferenceName)); }
		}

		public SecurityCheckpoint AddOrgLicenceModifyDatabaseDetailsSecurityCheckpoint(IZSecurity securityInstance)
		{
			SecurityCore securityDefinitions = ((SecurityCore)securityInstance);
			SecurityCheckpoint result = securityDefinitions.AddClientSpecificSecurityCheckpoint(
				Constants.OrgLicenceModifyDatabaseDetailsReferenceName,
				(NoResString)Constants.OrgLicenceModifyDatabaseDetailsDisplayName,
				securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.OrgLicenceModifyReferenceName))
			);

			return result;
		}

		#endregion

		#region SendUpgrade CheckPoint

		public static SecurityCheckpoint OrgLicenceModifySendUpgrade
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.OrgLicenceModifySendUpgradeSecurityReferenceName)); }
		}

		public SecurityCheckpoint AddOrgLicenceModifySendUpgradeSecurityCheckPoint(IZSecurity securityInstance)
		{
			SecurityCore securityDefinitions = ((SecurityCore)securityInstance);
			SecurityCheckpoint result = securityDefinitions.AddClientSpecificSecurityCheckpoint(
				Constants.OrgLicenceModifySendUpgradeSecurityReferenceName,
				(NoResString)Constants.OrgLicenceModifySendUpgradeSecurityDisplayName,
				securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.OrgLicenceModifyReferenceName))
				);

			return result;
		}

		#endregion

		#region SaveUpgradeToDisk CheckPoint

		public static SecurityCheckpoint OrgLicenceModifySaveUpgradeToDisk
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.OrgLicenceModifySaveUpgradeToDiskReferenceName)); }
		}

		public SecurityCheckpoint AddOrgLicenceModifySaveUpgradeToDiskCheckpoint(IZSecurity securityInstance)
		{
			SecurityCore securityDefinitions = (SecurityCore)securityInstance;
			return securityDefinitions.AddClientSpecificSecurityCheckpoint(
				Constants.OrgLicenceModifySaveUpgradeToDiskReferenceName,
				(NoResString)Constants.OrgLicenceModifySaveUpgradeToDiskDisplayName,
				securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.OrgLicenceModifySendUpgradeSecurityReferenceName)));
		}

		#endregion

		#region SendSupercededUpgrade CheckPoint

		public static SecurityCheckpoint OrgLicenceModifySendSupersededUpgrade
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.OrgLicenceModifySendSupersededUpgradeReferenceName)); }
		}

		public SecurityCheckpoint AddOrgLicenceModifySendSupersededUpgradeCheckpoint(IZSecurity securityInstance)
		{
			SecurityCore securityDefinitions = (SecurityCore)securityInstance;
			return securityDefinitions.AddClientSpecificSecurityCheckpoint(
				Constants.OrgLicenceModifySendSupersededUpgradeReferenceName,
				(NoResString)Constants.OrgLicenceModifySendSupersededUpgradeDisplayName,
				securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.OrgLicenceModifySendUpgradeSecurityReferenceName)));
		}

		#endregion

		#region SendUpgradeToHigherRingDB Checkpoint

		public static SecurityCheckpoint OrgLicenceModifySendUpgradeToHigherRingDB
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.OrgLicenceModifySendUpgradeToHigherRingDBCheckpointReferenceName)); }
		}

		public SecurityCheckpoint AddOrgLicenceModifySendUpgradeToHigherRingDBCheckpoint(IZSecurity securityInstance)
		{
			SecurityCore securityDefinitions = (SecurityCore)securityInstance;
			return securityDefinitions.AddClientSpecificSecurityCheckpoint(
				Constants.OrgLicenceModifySendUpgradeToHigherRingDBCheckpointReferenceName,
				(NoResString)Constants.OrgLicenceModifySendUpgradeToHigherRingDBCheckpointDisplayName,
				securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.OrgLicenceModifySendUpgradeSecurityReferenceName)));
		}

		#endregion

		#region Set Database Security Mode To Open Checkpoint

		public static SecurityCheckpoint OrgLicenceModifySetDatabaseSecurityModeToOpen
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.OrgLicenceModifySetDatabaseSecurityModeToOpenReferenceName)); }
		}

		public SecurityCheckpoint AddOrgLicenceModifySetDatabaseSecurityModeToOpenSecurityCheckpoint(IZSecurity securityInstance)
		{
			var securityDefinitions = ((SecurityCore)securityInstance);
			var result = securityDefinitions.AddClientSpecificSecurityCheckpoint(
				Constants.OrgLicenceModifySetDatabaseSecurityModeToOpenReferenceName,
				(NoResString)Constants.OrgLicenceModifySetDatabaseSecurityModeToOpenDisplayName,
				securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.OrgLicenceModifyDatabaseDetailsReferenceName))
			);

			return result;
		}

		#endregion

		#region SendUpgradeToDBNotOnSql2008 Checkpoint

		public static SecurityCheckpoint OrgLicenceModifySendUpgradeToDBNotOnLowestSupportedSqlVersion
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.OrgLicenceModifySendUpgradeToDBNotOnLowestSupportedSqlVersionCheckpointReferenceName)); }
		}

		public SecurityCheckpoint AddOrgLicenceModifySendUpgradeToDBNotOnLowestSupportedSqlVersionCheckpoint(IZSecurity securityInstance)
		{
			SecurityCore securityDefinitions = (SecurityCore)securityInstance;
			return securityDefinitions.AddClientSpecificSecurityCheckpoint(
				Constants.OrgLicenceModifySendUpgradeToDBNotOnLowestSupportedSqlVersionCheckpointReferenceName,
				(NoResString)Constants.OrgLicenceModifySendUpgradeToDBNotOnLowestSupportedSqlVersionCheckpointDisplayName,
				securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.OrgLicenceModifySendUpgradeSecurityReferenceName)));
		}

		#endregion

		#region RemoveLicenceDatabase Checkpoint

		public static SecurityCheckpoint OrgLicenceRemoveLicenceDatabase
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.OrgLicenceRemoveLicenceDatabaseSecurityReferenceName)); }
		}

		public SecurityCheckpoint AddOrgLicenceRemoveLicenceDatabaseSecurityCheckpoint(IZSecurity securityInstance)
		{
			SecurityCore securityDefinitions = ((SecurityCore)securityInstance);
			SecurityCheckpoint result = securityDefinitions.AddClientSpecificSecurityCheckpoint(
				Constants.OrgLicenceRemoveLicenceDatabaseSecurityReferenceName,
				(NoResString)Constants.OrgLicenceRemoveLicenceDatabaseSecurityDisplayName,
				securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.OrgLicenceModifyKeyConfigurationSecurityReferenceName))
			);

			return result;
		}

		#endregion

		#region MoveLicencesToNewEnterpriseCode Checkpoint

		public static SecurityCheckpoint OrgLicenceMoveLicencesToNewEnterprise
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.OrgLicenceMoveLicencesToNewEnterpriseSecurityReferenceName)); }
		}

		public SecurityCheckpoint AddOrgLicenceMoveLicencesToNewEnterpriseSecurityCheckpoint(IZSecurity securityInstance)
		{
			SecurityCore securityDefinitions = ((SecurityCore)securityInstance);
			SecurityCheckpoint result = securityDefinitions.AddClientSpecificSecurityCheckpoint(
				Constants.OrgLicenceMoveLicencesToNewEnterpriseSecurityReferenceName,
				(NoResString)Constants.OrgLicenceMoveLicencesToNewEnterpriseSecurityDisplayName,
				securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.OrgLicenceModifyKeyConfigurationSecurityReferenceName))
			);

			return result;
		}

		#endregion

		#region Implementation Email System

		public static SecurityCheckpoint ImplementationEmails
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.ImplementationEmailsReferenceName)); }
		}

		public SecurityCheckpoint AddImplementationEmailsSecurityCheckpoint(IZSecurity securityInstance)
		{
			SecurityCore securityDefinitions = ((SecurityCore)securityInstance);
			SecurityCheckpoint result = securityDefinitions.AddClientSpecificSecurityCheckpoint(
				Constants.ImplementationEmailsReferenceName,
				(NoResString)Constants.ImplementationEmailsDisplayName,
				securityDefinitions.Emails);

			return result;
		}
		#endregion

		#region Commission

		public static SecurityCheckpoint CommissionResolveAmbiguity
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.CommissionResolveAmbiguityReferenceName)); }
		}

		public void AddCommissionSecurityCheckpoints(IZSecurity securityInstance)
		{
			SecurityCore securityDefinitions = ((SecurityCore)securityInstance);

			securityDefinitions.AddClientSpecificSecurityCheckpoint(
				Constants.CommissionResolveAmbiguityReferenceName, (NoResString)Constants.CommissionResolveAmbiguityDisplayName, securityDefinitions.CommissionManager);
		}

		#endregion

		#region Customer Service

		public static SecurityCheckpoint CustomerService
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.CustomerServiceReferenceName)); }
		}

		public static SecurityCheckpoint CustomerServiceIncident
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.CustomerServiceIncidentReferenceName)); }
		}

		public static SecurityCheckpoint CustomerServiceIncidentNew
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.CustomerServiceIncidentNewReferenceName)); }
		}

		public static SecurityCheckpoint CustomerServiceIncidentEdit
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.CustomerServiceIncidentEditReferenceName)); }
		}

		public static SecurityCheckpoint CustomerServiceIncidentView
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.CustomerServiceIncidentViewReferenceName)); }
		}

		public static SecurityCheckpoint CustomerServiceIncidentEditDefectManagement
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.CustomerServiceIncidentEditDefectMgmtReferenceName)); }
		}

		public static SecurityCheckpoint CustomerServiceIncidentEditFeatureManagement
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.CustomerServiceIncidentEditFeatureMgmtReferenceName)); }
		}

		public static SecurityCheckpoint CustomerServiceIncidentJobInvoicing
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.CustomerServiceIncidentJobInvoicingReferenceName)); }
		}

		public static SecurityCheckpoint CustomerServiceIncidentAuditBilling
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.CustomerServiceIncidentAuditBillingReferenceName)); }
		}

		public static SecurityCheckpoint CustomerServiceIncidentAllowSendEConvAndCloseIncident
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.CustomerServiceIncidentAllowSendEConvAndCloseIncidentReferenceName)); }
		}

		public static SecurityCheckpoint CustomerServiceMuteOutboundEmailNotifications
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.CustomerServiceMuteOutboundEmailNotificationsReferenceName)); }
		}

		#region Incident Management Group

		public static SecurityCheckpoint CustomerServiceIncidentManagementGroup
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.CustomerServiceIncidentManagementGroupReferenceName)); }
		}

		public static SecurityCheckpoint CustomerServiceIncidentManagementGroupNew
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.CustomerServiceIncidentManagementGroupNewReferenceName)); }
		}

		public static SecurityCheckpoint CustomerServiceIncidentManagementGroupEdit
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.CustomerServiceIncidentManagementGroupEditReferenceName)); }
		}

		public static SecurityCheckpoint CustomerServiceIncidentManagementGroupView
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.CustomerServiceIncidentManagementGroupViewReferenceName)); }
		}

		#endregion

		#region Incident Triage

		public static SecurityCheckpoint CustomerServiceIncidentTriage
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.CustomerServiceIncidentTriageReferenceName)); }
		}

		public static SecurityCheckpoint CustomerServiceIncidentTriageNew
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.CustomerServiceIncidentTriageNewReferenceName)); }
		}

		public static SecurityCheckpoint CustomerServiceIncidentTriageEdit
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.CustomerServiceIncidentTriageEditReferenceName)); }
		}

		public static SecurityCheckpoint CustomerServiceIncidentTriageView
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.CustomerServiceIncidentTriageViewReferenceName)); }
		}

		public static SecurityCheckpoint CustomerServiceIncidentTriagePublishAccess
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.CustomerServiceIncidentTriagePublishAccessReferenceName)); }
		}

		#endregion

		#region Incident Triage Checklists

		public static SecurityCheckpoint CustomerServiceIncidentTriageChecklists
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.CustomerServiceIncidentTriageChecklistsReferenceName)); }
		}

		public static SecurityCheckpoint CustomerServiceIncidentTriageChecklistsNew
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.CustomerServiceIncidentTriageChecklistsNewReferenceName)); }
		}

		public static SecurityCheckpoint CustomerServiceIncidentTriageChecklistsEdit
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.CustomerServiceIncidentTriageChecklistsEditReferenceName)); }
		}

		public static SecurityCheckpoint CustomerServiceIncidentTriageChecklistsView
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.CustomerServiceIncidentTriageChecklistsViewReferenceName)); }
		}

		#endregion

		#region Incident Diagnostic Criteria

		public static SecurityCheckpoint CustomerServiceIncidentDiagnosticCriteria
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.CustomerServiceIncidentDiagnosticCriteriaReferenceName)); }
		}

		public static SecurityCheckpoint CustomerServiceIncidentDiagnosticCriteriaNew
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.CustomerServiceIncidentDiagnosticCriteriaNewReferenceName)); }
		}

		public static SecurityCheckpoint CustomerServiceIncidentDiagnosticCriteriaEdit
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.CustomerServiceIncidentDiagnosticCriteriaEditReferenceName)); }
		}

		public static SecurityCheckpoint CustomerServiceIncidentDiagnosticCriteriaView
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.CustomerServiceIncidentDiagnosticCriteriaViewReferenceName)); }
		}

		#endregion

		#region Investigation Item

		public static SecurityCheckpoint CustomerServiceInvestigationItem
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.CustomerServiceInvestigationItemReferenceName)); }
		}

		public static SecurityCheckpoint CustomerServiceInvestigationItemNew
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.CustomerServiceInvestigationItemNewReferenceName)); }
		}

		public static SecurityCheckpoint CustomerServiceInvestigationItemEdit
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.CustomerServiceInvestigationItemEditReferenceName)); }
		}

		public static SecurityCheckpoint CustomerServiceInvestigationItemView
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.CustomerServiceInvestigationItemViewReferenceName)); }
		}

		#endregion

		public static SecurityCheckpoint CustomerServiceReports
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.ReportsReferenceName)); }
		}

		public static SecurityCheckpoint OrganisationAllowSearchOutsideLoginCountry
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.OrganisationAllowSearchOutsideLoginCountryReferenceName)); }
		}

		public static SecurityCheckpoint EdiIdentityCertificates
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.EdiIdentityCertificateReferenceName)); }
		}

		public static SecurityCheckpoint EdiIdentityCertificateDownloadCertificate
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.EdiIdentityCertificateDownloadCertificateReferenceName)); }
		}

		public static SecurityCheckpoint EdiIdentityCertificateEditCertificate
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.EdiIdentityCertificateEditCertificateReferenceName)); }
		}

		public static SecurityCheckpoint EdiIdentityCertificatesNew
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.EdiIdentityCertificateNewReferenceName)); }
		}

		public static SecurityCheckpoint TokenAuthenticationOnBoarding
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.TokenAuthenticationOnBoardingReferenceName)); }
		}

		public static SecurityCheckpoint TokenAuthenticationOnBoardingCopyPassword
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.TokenAuthenticationOnBoardingCopyPasswordReferenceName)); }
		}

		public static SecurityCheckpoint EdiIdentityApplication
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.EdiIdentityApplicationReferenceName)); }
		}

		public static SecurityCheckpoint EdiIdentityApplicationNew
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.EdiIdentityApplicationNewReferenceName)); }
		}

		public static SecurityCheckpoint EdiIdentityApplicationEdit
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.EdiIdentityApplicationEditReferenceName)); }
		}

		public static SecurityCheckpoint EdiIdentityTenant
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.EdiIdentityTenantReferenceName)); }
		}

		public static SecurityCheckpoint EdiIdentityTenantNew
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.EdiIdentityTenantNewReferenceName)); }
		}

		public static SecurityCheckpoint EdiIdentityTenantEdit
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.EdiIdentityTenantEditReferenceName)); }
		}

		public void AddCustomerServiceSecurityCheckpoints(IZSecurity securityInstance)
		{
			SecurityCore securityDefinitions = ((SecurityCore)securityInstance);

			securityDefinitions.AddClientSpecificSecurityCheckpoint(
				Constants.CustomerServiceReferenceName, (NoResString)Constants.CustomerServiceDisplayName, securityDefinitions.Operations);

			securityDefinitions.AddClientSpecificSecurityCheckpoint(
				Constants.CustomerServiceIncidentReferenceName, (NoResString)Constants.CustomerServiceIncidentDisplayName, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.CustomerServiceReferenceName)));
			securityDefinitions.AddClientSpecificSecurityCheckpoint(
				Constants.CustomerServiceIncidentNewReferenceName, SecurityCore.Captions.New, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.CustomerServiceIncidentReferenceName)));
			securityDefinitions.AddClientSpecificSecurityCheckpoint(
				Constants.CustomerServiceIncidentEditReferenceName, SecurityCore.Captions.Edit, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.CustomerServiceIncidentReferenceName)));
			securityDefinitions.AddClientSpecificSecurityCheckpoint(
				Constants.CustomerServiceIncidentViewReferenceName, SecurityCore.Captions.View, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.CustomerServiceIncidentReferenceName)));

			securityDefinitions.AddClientSpecificSecurityCheckpoint(
				Constants.CustomerServiceIncidentEditDefectMgmtReferenceName, (NoResString)Constants.CustomerServiceIncidentEditDefectMgmtDisplayName, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.CustomerServiceIncidentReferenceName)));
			securityDefinitions.AddClientSpecificSecurityCheckpoint(
				Constants.CustomerServiceIncidentEditFeatureMgmtReferenceName, (NoResString)Constants.CustomerServiceIncidentEditFeatureMgmtDisplayName, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.CustomerServiceIncidentReferenceName)));

			securityDefinitions.AddClientSpecificSecurityCheckpoint(
				Constants.CustomerServiceIncidentJobInvoicingReferenceName, (NoResString)Constants.CustomerServiceIncidentJobInvoicingDisplayName, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.CustomerServiceIncidentReferenceName)));
			securityDefinitions.AddClientSpecificSecurityCheckpoint(
				Constants.CustomerServiceIncidentAuditBillingReferenceName, (NoResString)Constants.CustomerServiceIncidentAuditBillingDisplayName, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.CustomerServiceIncidentReferenceName)));
			securityDefinitions.AddClientSpecificSecurityCheckpoint(
				Constants.CustomerServiceIncidentAllowSendEConvAndCloseIncidentReferenceName, (NoResString)Constants.CustomerServiceIncidentAllowSendEConvAndCloseIncidentDisplayName, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.CustomerServiceIncidentReferenceName)));
			securityDefinitions.AddClientSpecificSecurityCheckpoint(
				Constants.CustomerServiceMuteOutboundEmailNotificationsReferenceName, (NoResString)Constants.CustomerServiceMuteOutboundEmailNotificationsDisplayName, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.CustomerServiceIncidentReferenceName)));

			securityDefinitions.AddClientSpecificSecurityCheckpoint(
				Constants.OrganisationAllowSearchOutsideLoginCountryReferenceName, (NoResString)Constants.OrganisationAllowSearchOutsideLoginCountryDisplayName, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.CustomerServiceIncidentReferenceName)));

			#region Incident Management Group

			securityDefinitions.AddClientSpecificSecurityCheckpoint(
				Constants.CustomerServiceIncidentManagementGroupReferenceName, (NoResString)Constants.CustomerServiceIncidentManagementGroupDisplayName, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.CustomerServiceReferenceName)));
			securityDefinitions.AddClientSpecificSecurityCheckpoint(
				Constants.CustomerServiceIncidentManagementGroupNewReferenceName, SecurityCore.Captions.New, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.CustomerServiceIncidentManagementGroupReferenceName)));
			securityDefinitions.AddClientSpecificSecurityCheckpoint(
				Constants.CustomerServiceIncidentManagementGroupEditReferenceName, SecurityCore.Captions.Edit, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.CustomerServiceIncidentManagementGroupReferenceName)));
			securityDefinitions.AddClientSpecificSecurityCheckpoint(
				Constants.CustomerServiceIncidentManagementGroupViewReferenceName, SecurityCore.Captions.View, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.CustomerServiceIncidentManagementGroupReferenceName)));

			#endregion

			#region Incident Triage

			securityDefinitions.AddClientSpecificSecurityCheckpoint(
				Constants.CustomerServiceIncidentTriageReferenceName, (NoResString)Constants.CustomerServiceIncidentTriageDisplayName, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.CustomerServiceReferenceName)));
			securityDefinitions.AddClientSpecificSecurityCheckpoint(
				Constants.CustomerServiceIncidentTriageNewReferenceName, SecurityCore.Captions.New, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.CustomerServiceIncidentTriageReferenceName)));
			securityDefinitions.AddClientSpecificSecurityCheckpoint(
				Constants.CustomerServiceIncidentTriageEditReferenceName, SecurityCore.Captions.Edit, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.CustomerServiceIncidentTriageReferenceName)));
			securityDefinitions.AddClientSpecificSecurityCheckpoint(
				Constants.CustomerServiceIncidentTriageViewReferenceName, SecurityCore.Captions.View, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.CustomerServiceIncidentTriageReferenceName)));
			securityDefinitions.AddClientSpecificSecurityCheckpoint(
				Constants.CustomerServiceIncidentTriagePublishAccessReferenceName, (NoResString)Constants.CustomerServiceIncidentTriagePublishAccessDisplayName, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.CustomerServiceIncidentTriageReferenceName)));

			#endregion

			#region Incident Triage Checklists

			securityDefinitions.AddClientSpecificSecurityCheckpoint(
				Constants.CustomerServiceIncidentTriageChecklistsReferenceName, (NoResString)Constants.CustomerServiceIncidentTriageChecklistsDisplayName, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.CustomerServiceReferenceName)));
			securityDefinitions.AddClientSpecificSecurityCheckpoint(
				Constants.CustomerServiceIncidentTriageChecklistsNewReferenceName, SecurityCore.Captions.New, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.CustomerServiceIncidentTriageChecklistsReferenceName)));
			securityDefinitions.AddClientSpecificSecurityCheckpoint(
				Constants.CustomerServiceIncidentTriageChecklistsEditReferenceName, SecurityCore.Captions.Edit, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.CustomerServiceIncidentTriageChecklistsReferenceName)));
			securityDefinitions.AddClientSpecificSecurityCheckpoint(
				Constants.CustomerServiceIncidentTriageChecklistsViewReferenceName, SecurityCore.Captions.View, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.CustomerServiceIncidentTriageChecklistsReferenceName)));

			#endregion

			#region Incident Diagnostic Criteria

			securityDefinitions.AddClientSpecificSecurityCheckpoint(
				Constants.CustomerServiceIncidentDiagnosticCriteriaReferenceName, (NoResString)Constants.CustomerServiceIncidentDiagnosticCriteriaDisplayName, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.CustomerServiceReferenceName)));
			securityDefinitions.AddClientSpecificSecurityCheckpoint(
				Constants.CustomerServiceIncidentDiagnosticCriteriaNewReferenceName, SecurityCore.Captions.New, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.CustomerServiceIncidentDiagnosticCriteriaReferenceName)));
			securityDefinitions.AddClientSpecificSecurityCheckpoint(
				Constants.CustomerServiceIncidentDiagnosticCriteriaEditReferenceName, SecurityCore.Captions.Edit, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.CustomerServiceIncidentDiagnosticCriteriaReferenceName)));
			securityDefinitions.AddClientSpecificSecurityCheckpoint(
				Constants.CustomerServiceIncidentDiagnosticCriteriaViewReferenceName, SecurityCore.Captions.View, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.CustomerServiceIncidentDiagnosticCriteriaReferenceName)));

			#endregion

			#region Investigation Item

			securityDefinitions.AddClientSpecificSecurityCheckpoint(
				Constants.CustomerServiceInvestigationItemReferenceName, (NoResString)Constants.CustomerServiceInvestigationItemDisplayName, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.CustomerServiceReferenceName)));
			securityDefinitions.AddClientSpecificSecurityCheckpoint(
				Constants.CustomerServiceInvestigationItemNewReferenceName, SecurityCore.Captions.New, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.CustomerServiceInvestigationItemReferenceName)));
			securityDefinitions.AddClientSpecificSecurityCheckpoint(
				Constants.CustomerServiceInvestigationItemEditReferenceName, SecurityCore.Captions.Edit, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.CustomerServiceInvestigationItemReferenceName)));
			securityDefinitions.AddClientSpecificSecurityCheckpoint(
				Constants.CustomerServiceInvestigationItemViewReferenceName, SecurityCore.Captions.View, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.CustomerServiceInvestigationItemReferenceName)));

			#endregion

			securityDefinitions.AddClientSpecificReportsSecurityCheckpoint(
				Constants.ReportsReferenceName, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.CustomerServiceReferenceName)));

			securityDefinitions.AddClientSpecificSecurityCheckpoint(
				Constants.EdiIdentityCertificateReferenceName, (NoResString)Constants.EdiIdentityCertificateDisplayName, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.CustomerServiceReferenceName)));
			securityDefinitions.AddClientSpecificSecurityCheckpoint(
				Constants.EdiIdentityCertificateDownloadCertificateReferenceName, (NoResString)Constants.EdiIdentityCertificateDownloadCertificateDisplayName, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.EdiIdentityCertificateReferenceName)));
			securityDefinitions.AddClientSpecificSecurityCheckpoint(
							Constants.EdiIdentityCertificateEditCertificateReferenceName, (NoResString)Constants.EdiIdentityCertificateEditCertificateDisplayName, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.EdiIdentityCertificateReferenceName)));

			securityDefinitions.AddClientSpecificSecurityCheckpoint(
				Constants.EdiIdentityCertificateNewReferenceName, SecurityCore.Captions.New, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.EdiIdentityCertificateReferenceName)));

			securityDefinitions.AddClientSpecificSecurityCheckpoint(
				Constants.TokenAuthenticationOnBoardingReferenceName, (NoResString)Constants.TokenAuthenticationOnBoardingDisplayName, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.CustomerServiceReferenceName)));

			securityDefinitions.AddClientSpecificSecurityCheckpoint(
				Constants.TokenAuthenticationOnBoardingCopyPasswordReferenceName, (NoResString)Constants.TokenAuthenticationOnBoardingCopyPasswordDisplayName, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.TokenAuthenticationOnBoardingReferenceName)));

			securityDefinitions.AddClientSpecificSecurityCheckpoint(
				Constants.EdiIdentityApplicationReferenceName, (NoResString)Constants.EdiIdentityApplicationDisplayName, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.CustomerServiceReferenceName)));

			securityDefinitions.AddClientSpecificSecurityCheckpoint(
				Constants.EdiIdentityApplicationNewReferenceName, SecurityCore.Captions.New, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.EdiIdentityApplicationReferenceName)));

			securityDefinitions.AddClientSpecificSecurityCheckpoint(
				Constants.EdiIdentityApplicationEditReferenceName, SecurityCore.Captions.Edit, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.EdiIdentityApplicationReferenceName)));

			#region Identity Tenant

			securityDefinitions.AddClientSpecificSecurityCheckpoint(
				Constants.EdiIdentityTenantReferenceName, (NoResString)Constants.EdiIdentityTenantDisplayName, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.CustomerServiceReferenceName)));

			securityDefinitions.AddClientSpecificSecurityCheckpoint(
				Constants.EdiIdentityTenantNewReferenceName, SecurityCore.Captions.New, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.EdiIdentityTenantReferenceName)));

			securityDefinitions.AddClientSpecificSecurityCheckpoint(
				Constants.EdiIdentityTenantEditReferenceName, SecurityCore.Captions.Edit, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.EdiIdentityTenantReferenceName)));

			#endregion
		}

		#endregion

		#region Installation & Upgrades

		public static SecurityCheckpoint InstallationTask
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.InstallationTaskReferenceName)); }
		}

		public static SecurityCheckpoint InstallationTaskJobInvoicing
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.InstallationTaskJobInvoicingReferenceName)); }
		}

		public static SecurityCheckpoint InstallationTaskAuditBilling
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.InstallationTaskAuditBillingReferenceName)); }
		}

		public static SecurityCheckpoint ReleaseBuild
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.ReleaseBuildReferenceName)); }
		}

		public static SecurityCheckpoint EdiTrustedMessagingConfig
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.EdiTrustedMessagingConfigReferenceName)); }
		}

		public static SecurityCheckpoint EdiTrustedSystem
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.EdiTrustedSystemReferenceName)); }
		}

		public void AddInstallAndUpgradeCheckpoints(IZSecurity securityInstance)
		{
			SecurityCore securityDefinitions = ((SecurityCore)securityInstance);

			securityDefinitions.AddClientSpecificSecurityCheckpoint(
				Constants.InstallationTaskReferenceName, (NoResString)Constants.InstallationTaskDisplayName, securityDefinitions.FindCheckPoint(securityDefinitions.Project.Code));

			securityDefinitions.AddClientSpecificSecurityCheckpoint(
				Constants.InstallationTaskJobInvoicingReferenceName, (NoResString)Constants.InstallationTaskJobInvoicingDisplayName, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.InstallationTaskReferenceName)));
			securityDefinitions.AddClientSpecificSecurityCheckpoint(
				Constants.InstallationTaskAuditBillingReferenceName, (NoResString)Constants.InstallationTaskAuditBillingDisplayName, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.InstallationTaskReferenceName)));

			securityDefinitions.AddClientSpecificSecurityCheckpoint(
				Constants.ReleaseBuildReferenceName, (NoResString)Constants.ReleaseBuildDisplayName, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.CustomerServiceReferenceName)));

			securityDefinitions.AddClientSpecificSecurityCheckpoint(
				Constants.EdiTrustedMessagingConfigReferenceName, (NoResString)Constants.EdiTrustedMessagingConfigDisplayName, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.CustomerServiceReferenceName)));

			securityDefinitions.AddClientSpecificSecurityCheckpoint(
				Constants.EdiTrustedSystemReferenceName, (NoResString)Constants.EdiTrustedSystemDisplayName, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.CustomerServiceReferenceName)));
		}

		#endregion

		#region Development

		public static SecurityCheckpoint Development
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.DevelopmentReferenceName)); }
		}

		public static SecurityCheckpoint ProfessionalServicesQuote
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.ProfessionalServicesQuoteReferenceName)); }
		}

		public static SecurityCheckpoint ProfessionalServicesQuoteNew
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.ProfessionalServicesQuoteNewReferenceName)); }
		}

		public static SecurityCheckpoint ProfessionalServicesQuoteEdit
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.ProfessionalServicesQuoteEditReferenceName)); }
		}

		public static SecurityCheckpoint ProfessionalServicesQuoteDelete
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.ProfessionalServicesQuoteDeleteReferenceName)); }
		}

		public static SecurityCheckpoint ProfessionalServicesQuoteJobInvoicing
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.ProfessionalServicesQuoteJobInvoicingReferenceName)); }
		}

		public static SecurityCheckpoint ProfessionalServicesQuoteAuditBilling
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.ProfessionalServicesQuoteAuditBillingReferenceName)); }
		}

		public static SecurityCheckpoint IssueManager
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.IssueManagerReferenceName)); }
		}

		public static SecurityCheckpoint IssueManagerMergeTool
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.IssueManagerMergeToolReferenceName)); }
		}

		public static SecurityCheckpoint IssueManagerReprocessTool
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.IssueManagerReprocessToolReferenceName)); }
		}

		public static SecurityCheckpoint UserAgreements => Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.UserAgreementsReferenceName));
		public static SecurityCheckpoint UserAgreementsNew => Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.UserAgreementsNewReferenceName));
		public static SecurityCheckpoint UserAgreementsEdit => Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.UserAgreementsEditReferenceName));
		public static SecurityCheckpoint UserAgreementsDelete => Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.UserAgreementsDeleteReferenceName));
		public static SecurityCheckpoint UserAgreementsAcceptForOrganisation => Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.UserAgreementsAcceptForOrganisationReferenceName));

		public static SecurityCheckpoint UserAgreementAcceptanceLogs => Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.UserAgreementAcceptanceLogsReferenceName));
		public static SecurityCheckpoint UserAgreementAssignCorporateAgreements => Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.UserAgreementAssigCorporateAgreementReferenceName));
		public static SecurityCheckpoint UserAgreementAddAcceptanceLogs => Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.UserAgreementAddAcceptanceLogsReferenceName));

		public static SecurityCheckpoint DevelopmentReports
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.DevelopmentReportsReferenceName)); }
		}

		#region Feature Control

		public static SecurityCheckpoint FeatureControl
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.FeatureControlReferenceName)); }
		}

		public static SecurityCheckpoint FeatureControlNew
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.FeatureControlNewReferenceName)); }
		}

		public static SecurityCheckpoint FeatureControlEdit
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.FeatureControlEditReferenceName)); }
		}

		public static SecurityCheckpoint FeatureControlView
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.FeatureControlViewReferenceName)); }
		}

		public static SecurityCheckpoint AllowFeatureEditForAnyReleaseGroupReferenceName
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.AllowFeatureEditForAnyReleaseGroupReferenceName)); }
		}
		#endregion

		#region Feature Set

		public static SecurityCheckpoint FeatureSet
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.FeatureSetReferenceName)); }
		}

		public static SecurityCheckpoint FeatureSetNew
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.FeatureSetNewReferenceName)); }
		}

		public static SecurityCheckpoint FeatureSetEdit
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.FeatureSetEditReferenceName)); }
		}

		public static SecurityCheckpoint FeatureSetView
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.FeatureSetViewReferenceName)); }
		}

		public static SecurityCheckpoint FeatureAdmin
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.FeatureAdminReferenceName)); }
		}

		#endregion

		public void AddDevelopmentCheckpoints(IZSecurity securityInstance)
		{
			var securityDefinitions = ((SecurityCore)securityInstance);

			var developmentSecurity = securityDefinitions.AddClientSpecificSecurityCheckpoint(
				Constants.DevelopmentReferenceName, (NoResString)Constants.DevelopmentDisplayName, securityDefinitions.Operations);

			securityDefinitions.AddClientSpecificSecurityCheckpoint(
				Constants.ProfessionalServicesQuoteReferenceName, (NoResString)Constants.ProfessionalServicesQuoteDisplayName, developmentSecurity);
			securityDefinitions.AddClientSpecificSecurityCheckpoint(
				Constants.ProfessionalServicesQuoteNewReferenceName, SecurityCore.Captions.New, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.ProfessionalServicesQuoteReferenceName)));
			securityDefinitions.AddClientSpecificSecurityCheckpoint(
				Constants.ProfessionalServicesQuoteEditReferenceName, SecurityCore.Captions.Edit, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.ProfessionalServicesQuoteReferenceName)));
			securityDefinitions.AddClientSpecificSecurityCheckpoint(
				Constants.ProfessionalServicesQuoteDeleteReferenceName, SecurityCore.Captions.Delete, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.ProfessionalServicesQuoteReferenceName)));
			securityDefinitions.AddClientSpecificSecurityCheckpoint(
				Constants.ProfessionalServicesQuoteJobInvoicingReferenceName, (NoResString)Constants.ProfessionalServicesQuoteJobInvoicingDisplayName, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.ProfessionalServicesQuoteReferenceName)));
			securityDefinitions.AddClientSpecificSecurityCheckpoint(
				Constants.ProfessionalServicesQuoteAuditBillingReferenceName, (NoResString)Constants.ProfessionalServicesQuoteAuditBillingDisplayName, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.ProfessionalServicesQuoteReferenceName)));

			securityDefinitions.AddClientSpecificSecurityCheckpoint(
				Constants.IssueManagerReferenceName, (NoResString)Constants.IssueManagerDisplayName, developmentSecurity);
			securityDefinitions.AddClientSpecificSecurityCheckpoint(
				Constants.IssueManagerMergeToolReferenceName, (NoResString)Constants.IssueManagerMergeToolDisplayName, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.IssueManagerReferenceName)));
			securityDefinitions.AddClientSpecificSecurityCheckpoint(
				Constants.IssueManagerReprocessToolReferenceName, (NoResString)Constants.IssueManagerReprocessToolDisplayName, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.IssueManagerReferenceName)));

			securityDefinitions.AddClientSpecificSecurityCheckpoint(
				Constants.UserAgreementsReferenceName, (NoResString)Constants.UserAgreementsDisplayName, developmentSecurity);
			securityDefinitions.AddClientSpecificSecurityCheckpoint(
				Constants.UserAgreementsNewReferenceName, SecurityCore.Captions.New, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.UserAgreementsReferenceName)));
			securityDefinitions.AddClientSpecificSecurityCheckpoint(
				Constants.UserAgreementsEditReferenceName, SecurityCore.Captions.Edit, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.UserAgreementsReferenceName)));
			securityDefinitions.AddClientSpecificSecurityCheckpoint(
				Constants.UserAgreementsDeleteReferenceName, SecurityCore.Captions.Delete, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.UserAgreementsReferenceName)));
			securityDefinitions.AddClientSpecificSecurityCheckpoint(
				Constants.UserAgreementsAcceptForOrganisationReferenceName, (NoResString)Constants.UserAgreementsAcceptForOrganisationDisplayName, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.UserAgreementsReferenceName)));

			securityDefinitions.AddClientSpecificSecurityCheckpoint(
				Constants.UserAgreementAcceptanceLogsReferenceName, (NoResString)Constants.UserAgreementAcceptanceLogsDisplayName, developmentSecurity);
			securityDefinitions.AddClientSpecificSecurityCheckpoint(
				Constants.UserAgreementAssigCorporateAgreementReferenceName, (NoResString)Constants.UserAgreementAssigCorporateAgreementDisplayName, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.UserAgreementsReferenceName)));
			securityDefinitions.AddClientSpecificSecurityCheckpoint(
				Constants.UserAgreementAddAcceptanceLogsReferenceName, (NoResString)Constants.UserAgreementAddAcceptanceLogsDisplayName, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.UserAgreementsReferenceName)));

			securityDefinitions.AddClientSpecificReportsSecurityCheckpoint(Constants.DevelopmentReportsReferenceName, developmentSecurity);

			securityDefinitions.AddClientSpecificSecurityCheckpoint(
				Constants.FeatureControlReferenceName, (NoResString)Constants.FeatureControlDisplayName, developmentSecurity);
			securityDefinitions.AddClientSpecificSecurityCheckpoint(
				Constants.FeatureControlNewReferenceName, SecurityCore.Captions.New, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.FeatureControlReferenceName)));
			securityDefinitions.AddClientSpecificSecurityCheckpoint(
				Constants.FeatureControlEditReferenceName, SecurityCore.Captions.Edit, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.FeatureControlReferenceName)));
			securityDefinitions.AddClientSpecificSecurityCheckpoint(
				Constants.FeatureControlViewReferenceName, SecurityCore.Captions.View, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.FeatureControlReferenceName)));
			
			securityDefinitions.AddClientSpecificSecurityCheckpoint(
				Constants.FeatureSetReferenceName, (NoResString)Constants.FeatureSetDisplayName, developmentSecurity);
			securityDefinitions.AddClientSpecificSecurityCheckpoint(
				Constants.FeatureSetNewReferenceName, SecurityCore.Captions.New, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.FeatureSetReferenceName)));
			securityDefinitions.AddClientSpecificSecurityCheckpoint(
				Constants.FeatureSetEditReferenceName, SecurityCore.Captions.Edit, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.FeatureSetReferenceName)));
			securityDefinitions.AddClientSpecificSecurityCheckpoint(
				Constants.FeatureSetViewReferenceName, SecurityCore.Captions.View, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.FeatureSetReferenceName)));
			securityDefinitions.AddClientSpecificSecurityCheckpoint(
				Constants.FeatureAdminReferenceName, (NoResString)"Admin", securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.FeatureSetReferenceName)));
			securityDefinitions.AddClientSpecificSecurityCheckpoint(
				Constants.AllowFeatureEditForAnyReleaseGroupReferenceName, (NoResString)Constants.AllowFeatureEditForAnyReleaseGroupDisplayName, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.FeatureControlReferenceName)));
		}

		#endregion

		#region LicDatabaseModifyToHigherRing Checkpoint

		public static SecurityCheckpoint OrgLicenceModifyLicDatabaseModifyToHigherRing
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.OrgLicenceModifyLicDatabaseModifyToHigherRingReferenceName)); }
		}

		public SecurityCheckpoint AddOrgLicenceModifyLicDatabaseModifyToHigherRingCheckpoint(IZSecurity securityInstance)
		{
			SecurityCore securityDefinitions = (SecurityCore)securityInstance;
			return securityDefinitions.AddClientSpecificSecurityCheckpoint(
				Constants.OrgLicenceModifyLicDatabaseModifyToHigherRingReferenceName,
				(NoResString)Constants.OrgLicenceModifyLicDatabaseModifyToHigherRingDisplayName,
				securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.OrgLicenceModifyReferenceName)));
		}

		#endregion

		#region LicDatabaseModifyToRestrictedRing Checkpoint

		public static SecurityCheckpoint OrgLicenceModifyLicDatabaseModifyToRestrictedRing
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.OrgLicenceModifyLicDatabaseModifyToRestrictedRingReferenceName)); }
		}

		public SecurityCheckpoint AddOrgLicenceModifyLicDatabaseModifyToRestrictedRingCheckpoint(IZSecurity securityInstance)
		{
			SecurityCore securityDefinitions = (SecurityCore)securityInstance;
			return securityDefinitions.AddClientSpecificSecurityCheckpoint(
				Constants.OrgLicenceModifyLicDatabaseModifyToRestrictedRingReferenceName,
				(NoResString)Constants.OrgLicenceModifyLicDatabaseModifyToRestrictedRingDisplayName,
				securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.OrgLicenceModifyReferenceName)));
		}

		#endregion

		#region OrgLicenceBilling Checkpoint

		public static SecurityCheckpoint OrgLicenceBilling
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.OrgLicenceBillingReferenceName)); }
		}

		public SecurityCheckpoint AddOrgLicenceBillingCheckpoint(IZSecurity securityInstance)
		{
			SecurityCore securityDefinitions = (SecurityCore)securityInstance;
			return securityDefinitions.AddClientSpecificSecurityCheckpoint(
				Constants.OrgLicenceBillingReferenceName,
				(NoResString)Constants.OrgLicenceBillingDisplayName,
				securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.OrgLicenceModifyReferenceName)));
		}

		#endregion

		#region Telematics Devices

		public void AddTelematicsDeviceCheckpoints(IZSecurity securityInstance)
		{
			var securityDefinitions = (SecurityCore)securityInstance;

			securityDefinitions.AddClientSpecificSecurityCheckpoint(Constants.DeviceTemplatesReferenceName, (NoResString)Constants.DeviceTemplatesDisplayName, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.CustomerServiceReferenceName)));
			securityDefinitions.AddClientSpecificSecurityCheckpoint(Constants.DeviceTemplatesNewReferenceName, SecurityCore.Captions.New, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.DeviceTemplatesReferenceName)));
			securityDefinitions.AddClientSpecificSecurityCheckpoint(Constants.DeviceTemplatesEditReferenceName, SecurityCore.Captions.Edit, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.DeviceTemplatesReferenceName)));
			securityDefinitions.AddClientSpecificSecurityCheckpoint(Constants.DeviceTemplatesViewReferenceName, SecurityCore.Captions.View, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.DeviceTemplatesReferenceName)));
			securityDefinitions.AddClientSpecificSecurityCheckpoint(Constants.DeviceTemplatesDeleteReferenceName, SecurityCore.Captions.Delete, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.DeviceTemplatesReferenceName)));

			securityDefinitions.AddClientSpecificSecurityCheckpoint(Constants.DevicesReferenceName, (NoResString)Constants.DevicesDisplayName, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.CustomerServiceReferenceName)));
			securityDefinitions.AddClientSpecificSecurityCheckpoint(Constants.DevicesNewReferenceName, SecurityCore.Captions.New, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.DevicesReferenceName)));
			securityDefinitions.AddClientSpecificSecurityCheckpoint(Constants.DevicesEditReferenceName, SecurityCore.Captions.Edit, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.DevicesReferenceName)));
			securityDefinitions.AddClientSpecificSecurityCheckpoint(Constants.DevicesViewReferenceName, SecurityCore.Captions.View, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.DevicesReferenceName)));
			securityDefinitions.AddClientSpecificSecurityCheckpoint(Constants.DevicesDeleteReferenceName, SecurityCore.Captions.Delete, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.DevicesReferenceName)));

			securityDefinitions.AddClientSpecificSecurityCheckpoint(Constants.DevicesDeleteOperationalActionsReferenceName, SecurityCore.Captions.OperationalActions, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.DevicesReferenceName)));
			securityDefinitions.AddClientSpecificSecurityCheckpoint(Constants.DevicesOperationalActionsCustomizationReferenceName, SecurityCore.Captions.CustomiseActions, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.DevicesDeleteOperationalActionsReferenceName)));
			securityDefinitions.AddClientSpecificSecurityCheckpoint(Constants.DevicesOperationalActionsExecutionReferenceName, SecurityCore.Captions.RunActions, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.DevicesDeleteOperationalActionsReferenceName)));
		}

		public static SecurityCheckpoint DeviceTemplates
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.DeviceTemplatesReferenceName)); }
		}

		public static SecurityCheckpoint DeviceTemplatesNew
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.DeviceTemplatesNewReferenceName)); }
		}

		public static SecurityCheckpoint DeviceTemplatesEdit
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.DeviceTemplatesEditReferenceName)); }
		}

		public static SecurityCheckpoint DeviceTemplatesView
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.DeviceTemplatesViewReferenceName)); }
		}

		public static SecurityCheckpoint DeviceTemplatesDelete
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.DeviceTemplatesDeleteReferenceName)); }
		}

		public static SecurityCheckpoint Devices
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.DevicesReferenceName)); }
		}

		public static SecurityCheckpoint DevicesNew
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.DevicesNewReferenceName)); }
		}

		public static SecurityCheckpoint DevicesEdit
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.DevicesEditReferenceName)); }
		}

		public static SecurityCheckpoint DevicesView
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.DevicesViewReferenceName)); }
		}

		public static SecurityCheckpoint DevicesDelete
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.DevicesDeleteReferenceName)); }
		}

		public static SecurityCheckpoint DevicesOperationalActionsCustomization
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.DevicesOperationalActionsCustomizationReferenceName)); }
		}

		public static SecurityCheckpoint DevicesOperationalActionsExecution
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.DevicesOperationalActionsExecutionReferenceName)); }
		}

		#endregion

		#region System and User Management

		public void AddSystemAndUserManagementCheckpoints(IZSecurity securityInstance)
		{
			var securityDefinitions = (SecurityCore)securityInstance;
			securityDefinitions.AddClientSpecificSecurityCheckpoint(Constants.SystemAndUserManagementReferenceName, (NoResString)Constants.SystemAndUserManagementDisplayName, securityDefinitions.Operations);
			securityDefinitions.AddClientSpecificSecurityCheckpoint(Constants.LicenceDatabaseRegistrationReferenceName, (NoResString)Constants.LicenceDatabaseRegistrationDisplayName, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.SystemAndUserManagementReferenceName)));
			securityDefinitions.AddClientSpecificSecurityCheckpoint(Constants.SystemUserAccountsReferenceName, (NoResString)Constants.SystemUserAccountsDisplayName, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.SystemAndUserManagementReferenceName)));
			securityDefinitions.AddClientSpecificSecurityCheckpoint(Constants.SystemUserAccountsEditReferenceName, SecurityCore.Captions.Edit, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.SystemUserAccountsReferenceName)));
			securityDefinitions.AddClientSpecificSecurityCheckpoint(Constants.SystemUserAccountsViewReferenceName, SecurityCore.Captions.View, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.SystemUserAccountsReferenceName)));

			securityDefinitions.AddClientSpecificSecurityCheckpoint(Constants.SendGitHubInviteForOthers, (NoResString)Constants.SendGitHubInviteForOthersDisplayName, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.SystemUserAccountsReferenceName)));

			securityDefinitions.AddClientSpecificSecurityCheckpoint(Constants.GlowSupportLogonToInternalNonProductionSystem, (NoResString)Constants.GlowSupportLogonToInternalNonProductionSystemDisplayName, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.SystemUserAccountsReferenceName)));
			securityDefinitions.AddClientSpecificSecurityCheckpoint(Constants.GlowSupportLogonToInternalNonProductionSystemAsSuperUser, (NoResString)Constants.GlowSupportLogonToInternalNonProductionSystemAsSuperUserDisplayName, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.GlowSupportLogonToInternalNonProductionSystem)));
			securityDefinitions.AddClientSpecificSecurityCheckpoint(Constants.GlowSupportLogonToInternalNonProductionSystemAsDiagnosticsUser, (NoResString)Constants.GlowSupportLogonToInternalNonProductionSystemAsDiagnosticsUserDisplayName, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.GlowSupportLogonToInternalNonProductionSystem)));

			securityDefinitions.AddClientSpecificSecurityCheckpoint(Constants.GlowSupportLogonToInternalProductionSystem, (NoResString)Constants.GlowSupportLogonToInternalProductionSystemDisplayName, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.SystemUserAccountsReferenceName)));
			securityDefinitions.AddClientSpecificSecurityCheckpoint(Constants.GlowSupportLogonToInternalProductionSystemAsSuperUser, (NoResString)Constants.GlowSupportLogonToInternalProductionSystemAsSuperUserDisplayName, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.GlowSupportLogonToInternalProductionSystem)));
			securityDefinitions.AddClientSpecificSecurityCheckpoint(Constants.GlowSupportLogonToInternalProductionSystemAsDiagnosticsUser, (NoResString)Constants.GlowSupportLogonToInternalProductionSystemAsDiagnosticsUserDisplayName, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.GlowSupportLogonToInternalProductionSystem)));

			securityDefinitions.AddClientSpecificSecurityCheckpoint(Constants.GlowSupportLogonToExternalNonProductionSystem, (NoResString)Constants.GlowSupportLogonToExternalNonProductionSystemDisplayName, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.SystemUserAccountsReferenceName)));
			securityDefinitions.AddClientSpecificSecurityCheckpoint(Constants.GlowSupportLogonToExternalNonProductionSystemAsSuperUser, (NoResString)Constants.GlowSupportLogonToExternalNonProductionSystemAsSuperUserDisplayName, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.GlowSupportLogonToExternalNonProductionSystem)));
			securityDefinitions.AddClientSpecificSecurityCheckpoint(Constants.GlowSupportLogonToExternalNonProductionSystemAsDiagnosticsUser, (NoResString)Constants.GlowSupportLogonToExternalNonProductionSystemAsDiagnosticsUserDisplayName, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.GlowSupportLogonToExternalNonProductionSystem)));

			securityDefinitions.AddClientSpecificSecurityCheckpoint(Constants.GlowSupportLogonToExternalProductionSystem, (NoResString)Constants.GlowSupportLogonToExternalProductionSystemDisplayName, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.SystemUserAccountsReferenceName)));
			securityDefinitions.AddClientSpecificSecurityCheckpoint(Constants.GlowSupportLogonToExternalProductionSystemAsSuperUser, (NoResString)Constants.GlowSupportLogonToExternalProductionSystemAsSuperUserDisplayName, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.GlowSupportLogonToExternalProductionSystem)));
			securityDefinitions.AddClientSpecificSecurityCheckpoint(Constants.GlowSupportLogonToExternalProductionSystemAsDiagnosticsUser, (NoResString)Constants.GlowSupportLogonToExternalProductionSystemAsDiagnosticsUserDisplayName, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.GlowSupportLogonToExternalProductionSystem)));
		}

		public static SecurityCheckpoint SystemAndUserManagement
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.SystemAndUserManagementReferenceName)); }
		}

		public static SecurityCheckpoint CWSupportLogonToExternalProductionSystemAsSuperUser => Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.GlowSupportLogonToExternalProductionSystemAsSuperUser));

		public static SecurityCheckpoint LicenceDatabaseRegistration
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.LicenceDatabaseRegistrationReferenceName)); }
		}

		public static SecurityCheckpoint SystemUserAccounts
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.SystemUserAccountsReferenceName)); }
		}

		public static SecurityCheckpoint SystemUserAccountsEdit
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.SystemUserAccountsEditReferenceName)); }
		}

		public static SecurityCheckpoint SystemUserAccountsView
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.SystemUserAccountsViewReferenceName)); }
		}

		public static SecurityCheckpoint SendGitHubInviteForOthers
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.SendGitHubInviteForOthers)); }
		}

		#endregion

		#region Work Item

		public void AddWorkItemSecurityCheckpoints(IZSecurity securityInstance)
		{
			var securityDefinitions = ((SecurityCore)securityInstance);
			securityDefinitions.AddClientSpecificSecurityCheckpoint(Constants.WorkItemEConversationReferenceName, (NoResString)Constants.WorkItemEConversationDisplayName, securityDefinitions.WorkItem);
			securityDefinitions.AddClientSpecificSecurityCheckpoint(Constants.WorkItemEConversationAddInternalCommentReferenceName, (NoResString)Constants.WorkItemEConversationAddInternalCommentDisplayName, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.WorkItemEConversationReferenceName)));
			securityDefinitions.AddClientSpecificSecurityCheckpoint(Constants.WorkItemEConversationSendMessagesReferenceName, (NoResString)Constants.WorkItemEConversationSendMessagesDisplayName, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.WorkItemEConversationReferenceName)));
			securityDefinitions.AddClientSpecificSecurityCheckpoint(Constants.WorkItemEConversationBroadcastReferenceName, (NoResString)Constants.WorkItemEConversationBroadcastDisplayName, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.WorkItemEConversationReferenceName)));
		}

		public static SecurityCheckpoint WorkItemEConversation
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.WorkItemEConversationReferenceName)); }
		}

		public static SecurityCheckpoint WorkItemEConversationAddInternalComment
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.WorkItemEConversationAddInternalCommentReferenceName)); }
		}

		public static SecurityCheckpoint WorkItemEConversationSendMessages
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.WorkItemEConversationSendMessagesReferenceName)); }
		}

		public static SecurityCheckpoint WorkItemEConversationBroadcast
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.WorkItemEConversationBroadcastReferenceName)); }
		}

		#endregion

		#region Avalara US Sales Tax

		public void AddAvalaraUSSalesTaxCheckpoints(IZSecurity securityInstance)
		{
			var securityDefinitions = ((SecurityCore)securityInstance);
			var receivablesRoot = securityDefinitions.AddClientSpecificSecurityCheckpoint(
				Constants.AvalaraUSSalesTaxReceivablesReferenceName,
				(NoResString)Constants.AvalaraUSSalesTaxDisplayName,
				securityDefinitions.ReceivablesTransactions
			);
			securityDefinitions.AddClientSpecificSecurityCheckpoint(
				Constants.AvalaraUSSalesTaxReceivablesRequestSalesTaxCalculationReferenceName,
				(NoResString)Constants.AvalaraUSSalesTaxRequestSalesTaxCalulationDisplayName,
				receivablesRoot
			);
			securityDefinitions.AddClientSpecificSecurityCheckpoint(
				Constants.AvalaraUSSalesTaxReceivablesResubmitSalesTaxTransactionReferenceName,
				(NoResString)Constants.AvalaraUSSalesTaxResubmitSalesTaxTransactionDisplayName,
				receivablesRoot
			);
		}

		public static SecurityCheckpoint AvalaraUSSalesTaxReceivablesRequestSalesTaxCalculation
			=> Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.AvalaraUSSalesTaxReceivablesRequestSalesTaxCalculationReferenceName));

		public static SecurityCheckpoint AvalaraUSSalesTaxReceivablesResubmitSalesTaxTransaction
			=> Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.AvalaraUSSalesTaxReceivablesResubmitSalesTaxTransactionReferenceName));

		#endregion

		#region ApplicationLogging

		public void AddApplicationLoggingCheckpoints(IZSecurity securityInstance)
		{
			var securityDefinitions = ((SecurityCore)securityInstance);
			securityDefinitions.AddClientSpecificSecurityCheckpoint(Constants.ApplicationLoggingApplicationLoggerReferenceName, (NoResString)Constants.ApplicationLoggingApplicationLoggerDisplayName, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.DevelopmentReferenceName)));
			securityDefinitions.AddClientSpecificSecurityCheckpoint(Constants.ApplicationLoggingApplicationLoggerViewReferenceName, (NoResString)Constants.ApplicationLoggingApplicationLoggerViewDisplayName, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.ApplicationLoggingApplicationLoggerReferenceName)));
			securityDefinitions.AddClientSpecificSecurityCheckpoint(Constants.ApplicationLoggingApplicationLoggerEditReferenceName, (NoResString)Constants.ApplicationLoggingApplicationLoggerEditDisplayName, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.ApplicationLoggingApplicationLoggerReferenceName)));
			securityDefinitions.AddClientSpecificSecurityCheckpoint(Constants.ApplicationLoggingApplicationLoggerNewReferenceName, (NoResString)Constants.ApplicationLoggingApplicationLoggerNewDisplayName, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.ApplicationLoggingApplicationLoggerReferenceName)));
			securityDefinitions.AddClientSpecificSecurityCheckpoint(Constants.ApplicationLoggingApplicationLoggerDeleteReferenceName, (NoResString)Constants.ApplicationLoggingApplicationLoggerDeleteDisplayName, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.ApplicationLoggingApplicationLoggerReferenceName)));

			securityDefinitions.AddClientSpecificSecurityCheckpoint(Constants.ApplicationLoggingApplicationActiveLoggerReferenceName, (NoResString)Constants.ApplicationLoggingApplicationActiveLoggerDisplayName, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.DevelopmentReferenceName)));
			securityDefinitions.AddClientSpecificSecurityCheckpoint(Constants.ApplicationLoggingApplicationActiveLoggerViewReferenceName, (NoResString)Constants.ApplicationLoggingApplicationActiveLoggerViewDisplayName, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.ApplicationLoggingApplicationActiveLoggerReferenceName)));
			securityDefinitions.AddClientSpecificSecurityCheckpoint(Constants.ApplicationLoggingApplicationActiveLoggerEditReferenceName, (NoResString)Constants.ApplicationLoggingApplicationActiveLoggerEditDisplayName, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.ApplicationLoggingApplicationActiveLoggerReferenceName)));
			securityDefinitions.AddClientSpecificSecurityCheckpoint(Constants.ApplicationLoggingApplicationActiveLoggerNewReferenceName, (NoResString)Constants.ApplicationLoggingApplicationActiveLoggerNewDisplayName, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.ApplicationLoggingApplicationActiveLoggerReferenceName)));
			securityDefinitions.AddClientSpecificSecurityCheckpoint(Constants.ApplicationLoggingApplicationActiveLoggerDeleteReferenceName, (NoResString)Constants.ApplicationLoggingApplicationActiveLoggerDeleteDisplayName, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.ApplicationLoggingApplicationActiveLoggerReferenceName)));
		}

		public static SecurityCheckpoint ApplicationLoggingApplicationLogger
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.ApplicationLoggingApplicationLoggerReferenceName)); }
		}

		public static SecurityCheckpoint ApplicationLoggingApplicationLoggerView
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.ApplicationLoggingApplicationLoggerViewReferenceName)); }
		}

		public static SecurityCheckpoint ApplicationLoggingApplicationLoggerEdit
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.ApplicationLoggingApplicationLoggerEditReferenceName)); }
		}

		public static SecurityCheckpoint ApplicationLoggingApplicationLoggerNew
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.ApplicationLoggingApplicationLoggerNewReferenceName)); }
		}

		public static SecurityCheckpoint ApplicationLoggingApplicationLoggerDelete
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.ApplicationLoggingApplicationLoggerDeleteReferenceName)); }
		}

		public static SecurityCheckpoint ApplicationLoggingApplicationActiveLogger
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.ApplicationLoggingApplicationActiveLoggerReferenceName)); }
		}

		public static SecurityCheckpoint ApplicationLoggingApplicationActiveLoggerView
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.ApplicationLoggingApplicationActiveLoggerViewReferenceName)); }
		}

		public static SecurityCheckpoint ApplicationLoggingApplicationActiveLoggerEdit
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.ApplicationLoggingApplicationActiveLoggerEditReferenceName)); }
		}

		public static SecurityCheckpoint ApplicationLoggingApplicationActiveLoggerNew
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.ApplicationLoggingApplicationActiveLoggerNewReferenceName)); }
		}

		public static SecurityCheckpoint ApplicationLoggingApplicationActiveLoggerDelete
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.ApplicationLoggingApplicationActiveLoggerDeleteReferenceName)); }
		}
		#endregion

		#region STL Billing Admin

		public void AddSTLBillAdminCheckpoints(IZSecurity securityInstance)
		{
			var securityDefinitions = ((SecurityCore)securityInstance);
			securityDefinitions.AddClientSpecificSecurityCheckpoint(
				Constants.STLBillingReferenceName, (NoResString)Constants.STLBillingDisplayName, securityDefinitions.Receivables);
			securityDefinitions.AddClientSpecificSecurityCheckpoint(
				Constants.STLBillingAdminFunctionReferenceName, (NoResString)Constants.STLBillingAdminFunctionDisplayName, securityDefinitions.FindCheckPoint(new CheckpointLookupKey(Constants.STLBillingReferenceName)));
		}

		public static SecurityCheckpoint STLBilling
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.STLBillingReferenceName)); }
		}

		public static SecurityCheckpoint STLBillingAdminFunction
		{
			get { return Env.Security.FindCheckPoint(new CheckpointLookupKey(Constants.STLBillingAdminFunctionReferenceName)); }
		}

		#endregion
	}
}
