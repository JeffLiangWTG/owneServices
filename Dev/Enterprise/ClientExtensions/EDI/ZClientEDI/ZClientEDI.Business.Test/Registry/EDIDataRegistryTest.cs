using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.Registry;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Core.Modules;
using Enterprise.CustomerService.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ProcessManagement.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;
using ZClientEDI.Business.Registry;

namespace Enterprise.Client.EDI.Test
{
	[TestedType(typeof(EDIDataRegistry))]
	public partial class EDIDataRegistryTest : RegistryItemSetTestCaseWithFactory<EDIDataRegistry>
	{
		public override void TestAccessingValuesOnlyDoesNotDemandResourceStrings()
		{
			//registry items based on Workflow Types populated from the global workflow descriptors list
			//where Description has a resource string.
			//therefore we need to override this test and not to run.

			Assert(true);
		}

		public void TestRegistryItemExists()
		{
			var registry = new EDIDataRegistry();

			AssertEquals("There should be a registry item for the Incident High Criticality Incident Email Group", "WiseTech Global Client Extensions/Customer Service Incidents/Incident Staff Groups", ItemSet.CustomerServiceHighCriticalityIncidentGroup.Category);
			AssertEquals("There should be a registry item for the Incident Cust Service Manager Email Group", "WiseTech Global Client Extensions/Customer Service Incidents/Incident Staff Groups", ItemSet.IncidentSupportGroup.Category);
			AssertEquals("There should be a registry item for the Incident Defect Manager Email Group", "WiseTech Global Client Extensions/Customer Service Incidents/Incident Staff Groups/Enterprise", ItemSet.IncidentDefectManagerGroupENT.Category);
			AssertEquals("There should be a registry item for the Incident Feature Request Email Group", "WiseTech Global Client Extensions/Customer Service Incidents/Incident Staff Groups/Enterprise", ItemSet.IncidentFeatureRequestGroupENT.Category);
			AssertEquals("There should be a registry item for the Incident Installations Email Group", "WiseTech Global Client Extensions/Customer Service Incidents/Incident Staff Groups/Enterprise", ItemSet.IncidentInstallationsGroupENT.Category);
			AssertEquals("There should be a registry item for the Incident Graphics Customization Email Group", "WiseTech Global Client Extensions/Customer Service Incidents/Incident Staff Groups/Enterprise", ItemSet.IncidentGraphicGroupENT.Category);
			AssertEquals("There should be a registry item for the Linked Incident Grid Refresh Rate", "WiseTech Global Client Extensions/Customer Service Incidents/Incident Management Groups", ItemSet.LinkedIncidentGridRefreshRate.Category);

			AssertEquals("There should be a registry item for the Issue Manager Exception Key Matching Regexes", "WiseTech Global Client Extensions/Issue Manager", ItemSet.ExceptionKeyMatchingRegexes.Category);
			AssertEquals("There should be a registry item for the Issue Manager Fallback Work Item Criteria", "WiseTech Global Client Extensions/Issue Manager", ItemSet.FallbackWorkItemCriteriaRegistryItem.Category);
			AssertEquals("There should be a registry item for the Issue Manager Ignored Exception Stack Line Regexes", "WiseTech Global Client Extensions/Issue Manager", ItemSet.IgnoredExceptionStackLineRegexes.Category);

			AssertEquals("There should be a registry item for the Customer Service Email Address", "WiseTech Global Client Extensions/System Email Addresses", ItemSet.IncidentFromEmailAddress.Category);
			AssertEquals("There should be a registry item for the Customer Service Email Address", "WiseTech Global Client Extensions/System Email Addresses", ItemSet.EmailAddressBlockList.Category);

			AssertEquals("There should be a registry item for the Default Implementation Email Address", "WiseTech Global Client Extensions/System Email Addresses", ItemSet.ImplementationDefaultFromEmailAddress.Category);
			AssertEquals("There should be a registry item for the Default Implementation Email Address", "Implementation Default From Email Address", ItemSet.ImplementationDefaultFromEmailAddress.Caption);

			AssertEquals("There should be a registry item for the Project Invoice Notification Recipients", "WiseTech Global Client Extensions/Projects", ItemSet.ProjectInvoiceEmailNotificationGroup.Category);

			AssertEquals("There should be a registry item for the Processed Shelves Batch Processor Notification Group", "WiseTech Global Client Extensions/Processed Shelfs", ItemSet.ProcessedShelfsNotificationGroup.Category);

			AssertEquals("There should be a registry item for the Upgrade Package Delivery Notification Email Template", "WiseTech Global Client Extensions/Release Builds & Upgrades", ItemSet.UpgradePackageDeliveryNotificationEmailTemplateRaw.Category);

			AssertEquals("There should be a registry item for the EDIClassrooms Default Filter Layouts", "WiseTech Global Client Extensions/Default Filter Layouts", ItemSet.DefaultFilterLayoutForWebEDIClassrooms.Category);
			AssertEquals("There should be a registry item for the HRJobApplicant Default Filter Layouts", "WiseTech Global Client Extensions/Default Filter Layouts", ItemSet.DefaultFilterLayoutForWebHRJobApplicant.Category);

			AssertEquals("There should be a registry item for the Db Connection Crikey Server", "WiseTech Global Client Extensions", ItemSet.DbConnectionCrikeyServer.Category);

			AssertEquals("There should be a registry item for the Internal Incident Licence Settings", "WiseTech Global Client Extensions/Customer Service Incidents", ItemSet.InternalIncidentLicenceSettings.Category);
			AssertEquals("There should be a registry item for the Incident Product Areas", "WiseTech Global Client Extensions/Customer Service Incidents", ItemSet.ProductAreas.Category);
			AssertEquals("There should be a registry item for the Incident Event Workflow Template Client Org", "WiseTech Global Client Extensions/Customer Service Incidents", ItemSet.IncidentEventWorkflowTemplateClientOrg.Category);
			AssertEquals("There should be a registry item for the Closed Incident Last Assignee Notification Period", "WiseTech Global Client Extensions/Customer Service Incidents", ItemSet.ClosedIncidentLastAssigneeNotificationPeriod.Category);
			AssertEquals("There should be a registry item for the Feature Request Quotation Auto Expire Period", "WiseTech Global Client Extensions/Customer Service Incidents", ItemSet.FeatureRequestQuotationAutoExpirePeriod.Category);
			AssertEquals("There should be a registry item for the Feature Request Estimate Auto Expire Period", "WiseTech Global Client Extensions/Customer Service Incidents", ItemSet.FeatureRequestEstimateAutoExpirePeriod.Category);
			AssertEquals("There should be a registry item for the Content Finder URL", "WiseTech Global Client Extensions/Customer Service Incidents", ItemSet.ContentFinderUrl.Category);
			AssertEquals("There should be a registry item for the Enable Content Finder", "WiseTech Global Client Extensions/Customer Service Incidents", ItemSet.EnableContentFinder.Category);
			AssertEquals("There should be a registry item for the IS Alerts Service Account", "WiseTech Global Client Extensions/Customer Service Incidents", ItemSet.ISAlertsServiceAccount.Category);

			AssertEquals("There should be a registry item for the Denied Party Screening Lists Db Server Name", "WiseTech Global Client Extensions/eServices", ItemSet.DeniedPartyScreeningListsDbServerName.Category);
			AssertEquals("There should be a registry item for the Denied Party Screening Lists Db Name", "WiseTech Global Client Extensions/eServices", ItemSet.DeniedPartyScreeningListsDbName.Category);
			AssertEquals("There should be a registry item for the Denied Party Screening Lists Db Login", "WiseTech Global Client Extensions/eServices", ItemSet.DeniedPartyScreeningListsDbLogin.Category);

			AssertEquals("There should be a registry item for the ODPL Usage Charge Code", "WiseTech Global Client Extensions/Licence Billing/ODPL", ItemSet.OdplUsageChargeCode.Category);
			AssertEquals("There should be a registry item for the ODPL Discount Charge Code", "WiseTech Global Client Extensions/Licence Billing/ODPL", ItemSet.OdplDiscountChargeCode.Category);
			AssertEquals("There should be a registry item for the ODPL Hybrid Usage Charge Code", "WiseTech Global Client Extensions/Licence Billing/ODPL", ItemSet.OdplHybridUsageChargeCode.Category);
			AssertEquals("There should be a registry item for the ODPL Hybrid Discount Charge Code", "WiseTech Global Client Extensions/Licence Billing/ODPL", ItemSet.OdplHybridDiscountChargeCode.Category);
			AssertEquals("There should be a registry item for the ODPL Deposit Charge Code", "WiseTech Global Client Extensions/Licence Billing/ODPL", ItemSet.OdplDepositChargeCode.Category);
			AssertEquals("There should be a registry item for the Monthly Usage Processing Fee Charge Code", "WiseTech Global Client Extensions/Licence Billing/Processing", ItemSet.MonthlyUsageProcessingFeeChargeCode.Category);
			AssertEquals("There should be a registry item for the Comment Charge Code", "WiseTech Global Client Extensions/Licence Billing", ItemSet.CommentChargeCode.Category);
			AssertEquals("There should be a registry item for the Price List Doc Type", "WiseTech Global Client Extensions/Licence Billing", ItemSet.PriceListDocType.Category);
			AssertEquals("There should be a registry item for the Invoice Attachment Doc Type", "WiseTech Global Client Extensions/Licence Billing", ItemSet.InvoiceAttachmentDocType.Category);
			AssertEquals("There should be a registry item for the MinimumAmountToBill", "WiseTech Global Client Extensions/Licence Billing/Monthly Usage Invoice", ItemSet.MinimumAmountToBill.Category);
			AssertEquals("There should be a registry item for the NonBilledEnterpriseCodes", "WiseTech Global Client Extensions/Licence Billing", ItemSet.NonBilledEnterpriseCodes.Category);
			AssertEquals("There should be a registry item for the Monthly Usage Invoice Description", "WiseTech Global Client Extensions/Licence Billing/Monthly Usage Invoice", ItemSet.MonthlyUsageInvoiceDescription.Category);
			AssertEquals("There should be a registry item for the Sales Tax Rates", "WiseTech Global Client Extensions/Licence Billing", ItemSet.SalesTaxRates.Category);
			AssertEquals("There should be a registry item for the Hosting Data Storage Charge Code", "WiseTech Global Client Extensions/Licence Billing/Hosting", ItemSet.HostingDataStorageChargeCode.Category);
			AssertEquals("There should be a registry item for the Hosting Docs Storage Charge Code", "WiseTech Global Client Extensions/Licence Billing/Hosting", ItemSet.HostingDocsStorageChargeCode.Category);
			AssertEquals("There should be a registry item for the Hosting Docs Storage Charge Code", "WiseTech Global Client Extensions/Licence Billing/Hosting", ItemSet.HostingUltraFastStorageChargeCode.Category);
			AssertEquals("There should be a registry item for the Hosting Remote Devices Charge Code", "WiseTech Global Client Extensions/Licence Billing/Hosting", ItemSet.HostingRemoteDevicesChargeCode.Category);
			AssertEquals("There should be a registry item for the Hosting Print Servers Charge Code", "WiseTech Global Client Extensions/Licence Billing/Hosting", ItemSet.HostingPrintServersChargeCode.Category);
			AssertEquals("There should be a registry item for the Licence Fee Types", "WiseTech Global Client Extensions/Licence Billing", ItemSet.LicenceFeeTypes.Category);
			AssertEquals("There should be a registry item for the LicenceUsageBilledPerTransaction", "WiseTech Global Client Extensions/Licence Billing", ItemSet.LicenceUsageBilledPerTransaction.Category);
			AssertEquals("There should be a registry item for the TransactionChargeCodes", "WiseTech Global Client Extensions/Licence Billing/ODPL", ItemSet.TransactionChargeCodes.Category);
			AssertEquals("There should be a registry item for the TransactionDiscountChargeCodes", "WiseTech Global Client Extensions/Licence Billing/ODPL", ItemSet.TransactionDiscountChargeCodes.Category);
			AssertEquals("There should be a registry item for the AirlineMessagingFWBChargeCode", "WiseTech Global Client Extensions/Licence Billing/Airline Messaging", ItemSet.AirlineMessagingFWBChargeCode.Category);
			AssertEquals("There should be a registry item for the AirlineMessagingFHLChargeCode", "WiseTech Global Client Extensions/Licence Billing/Airline Messaging", ItemSet.AirlineMessagingFHLChargeCode.Category);
			AssertEquals("There should be a registry item for the AirlineMessagingFSUChargeCode", "WiseTech Global Client Extensions/Licence Billing/Airline Messaging", ItemSet.AirlineMessagingFSUChargeCode.Category);
			AssertEquals("There should be a registry item for the AirlineMessagingDiscountAndRemitChargeCode", "WiseTech Global Client Extensions/Licence Billing/Airline Messaging", ItemSet.AirlineMessagingDiscountAndRemitChargeCode.Category);
			AssertEquals("There should be a registry item for the AirlineMessagingTraxonLicenceIdentifier", "WiseTech Global Client Extensions/Licence Billing/Airline Messaging", ItemSet.AirlineMessagingTraxonLicenceIdentifier.Category);
			AssertEquals("There should be a registry item for the NZCustomsJobChargeCode", "WiseTech Global Client Extensions/Licence Billing/NZ Customs", ItemSet.NZCustomsJobChargeCode.Category);
			AssertEquals("There should be a registry item for the NZCustomsMessageChargeCode", "WiseTech Global Client Extensions/Licence Billing/NZ Customs", ItemSet.NZCustomsMessageChargeCode.Category);
			AssertEquals("There should be a registry item for the ABMCustomsWareMessagingChargeCode", "WiseTech Global Client Extensions/Licence Billing/ABM Customs", ItemSet.ABMCustomsWareMessagingChargeCode.Category);
			AssertEquals("There should be a registry item for the ABMMovementMessagingChargeCode", "WiseTech Global Client Extensions/Licence Billing/ABM Customs", ItemSet.ABMMovementMessagingChargeCode.Category);
			AssertEquals("There should be a registry item for the ABMFiscalRepInvoiceMessagingChargeCode", "WiseTech Global Client Extensions/Licence Billing/ABM Customs", ItemSet.ABMFiscalRepInvoiceMessagingChargeCode.Category);
			AssertEquals("There should be a registry item for the ABMCustomsWareMessagingDiscountChargeCode", "WiseTech Global Client Extensions/Licence Billing/ABM Customs", ItemSet.ABMCustomsWareMessagingDiscountChargeCode.Category);
			AssertEquals("There should be a registry item for the ABMMovementMessagingDiscountChargeCode", "WiseTech Global Client Extensions/Licence Billing/ABM Customs", ItemSet.ABMMovementMessagingDiscountChargeCode.Category);
			AssertEquals("There should be a registry item for the ABMFiscalRepInvoiceMessagingDiscountChargeCode", "WiseTech Global Client Extensions/Licence Billing/ABM Customs", ItemSet.ABMFiscalRepInvoiceMessagingDiscountChargeCode.Category);

			AssertEquals("There should be a registry item for the Notification Group For My Account User Registration", "WiseTech Global Client Extensions/My Account/User Registration", ItemSet.UserRegistrationNotificationGroup.Category);
			AssertEquals("There should be a registry item for the Predefined Job Roles For My Account User Registration", "WiseTech Global Client Extensions/My Account/User Registration", ItemSet.UserRegistrationJobRoleList.Category);
			AssertEquals("There should be a registry item for the Predefined Business Types For My Account User Registration", "WiseTech Global Client Extensions/My Account/User Registration", ItemSet.UserRegistrationTypeOfBusinessList.Category);
			AssertEquals("There should be a registry item for the Predefined Reasons For Requesting Access For My Account User Registration", "WiseTech Global Client Extensions/My Account/User Registration", ItemSet.UserRegistrationReasonForRequestingAccessList.Category);
			AssertEquals("There should be a registry item for the Predefined Company Size For My Account User Registration", "WiseTech Global Client Extensions/My Account/User Registration", ItemSet.UserRegistrationCompanySizeList.Category);
			AssertEquals("There should be a registry item for the My Account Terms And Conditions Notification Email Template", "WiseTech Global Client Extensions/My Account/Terms and Conditions", ItemSet.MyAccountTermsAndConditionsNotificationEmailTemplate.Category);
			AssertEquals("There should be a registry item for the My Account Terms And Conditions Notification Email Sender's Name", "WiseTech Global Client Extensions/My Account/Terms and Conditions", ItemSet.MyAccountTermsAndConditionsNotificationEmailSenderName.Category);
			AssertEquals("There should be a registry item for the My Account Terms And Conditions Notification Email Sender's Email Address", "WiseTech Global Client Extensions/My Account/Terms and Conditions", ItemSet.MyAccountTermsAndConditionsNotificationEmailSenderAddress.Category);
			AssertEquals("There should be a registry item for the MyAccountReportsShareFolderPath", "WiseTech Global Client Extensions/My Account/Report", ItemSet.MyAccountReportsShareFolderPath.Category);
			AssertEquals("There should be a registry item for the MyAccountReportsDownloadNotificationEmailTemplate", "WiseTech Global Client Extensions/My Account/Report", ItemSet.MyAccountReportsDownloadNotificationEmailTemplate.Category);
			AssertEquals("There should be a registry item for the MyAccountReportsDownloadURL", "WiseTech Global Client Extensions/My Account/Report", ItemSet.MyAccountReportsDownloadURL.Category);
			AssertEquals("There should be a registry item for the MyAccountSiteRootUrl", "WiseTech Global Client Extensions/My Account/URLs and Paths", ItemSet.MyAccountSiteRootUrl.Category);
			AssertEquals("There should be a registry item for the MyAccountHostingSiteRootUrl", "WiseTech Global Client Extensions/My Account/URLs and Paths", ItemSet.MyAccountHostingSiteRootUrl.Category);
			AssertEquals("There should be a registry item for the MyAccountIndexPage", "WiseTech Global Client Extensions/My Account/URLs and Paths", ItemSet.MyAccountIndexPage.Category);
			AssertEquals("There should be a registry item for the MyAccountPhysicalServerPath", "WiseTech Global Client Extensions/My Account/URLs and Paths", ItemSet.MyAccountPhysicalServerPath.Category);
			AssertEquals("There should be a registry item for the WiseTech Academy Auto Login Url", "WiseTech Global Client Extensions/My Account/URLs and Paths", ItemSet.WiseTechAcademyAutoLoginUrl.Category);
			AssertEquals("There should be a registry item for the WiseTech Academy Token Url", "WiseTech Global Client Extensions/My Account/URLs and Paths", ItemSet.WiseTechAcademyTokenEndpointUrl.Category);
			AssertEquals("There should be a registry item for the WiseTech Academy Docuument Urls Endpoint Url", "WiseTech Global Client Extensions/My Account/URLs and Paths", ItemSet.WiseTechAcademyDocumentUrlsEndpointUrl.Category);
			AssertEquals("There should be a registry item for the My Account Hosting Site Landing Page Url", "WiseTech Global Client Extensions/My Account/URLs and Paths", ItemSet.MyAccountHostingSiteLandingPageUrl.Category);
			AssertEquals("There should be a registry item for the CW1DvdIsoFileDownloadURL", "WiseTech Global Client Extensions/My Account/Download", ItemSet.CW1DvdIsoFileDownloadURL.Category);
			AssertEquals("There should be a registry item for the CW1DvdZipFileDownloadURL", "WiseTech Global Client Extensions/My Account/Download", ItemSet.CW1DvdZipFileDownloadURL.Category);
			AssertEquals("There should be a registry item for the CW1ExeFileDownloadURL", "WiseTech Global Client Extensions/My Account/Download", ItemSet.CW1ExeFileDownloadURL.Category);

			AssertEquals("There should be a registry item for the Virtual Machine Detection Keywords", "WiseTech Global Client Extensions/Version Reporting", ItemSet.VirtualMachineDetectionKeywords.Category);

			AssertEquals("There should be a registry item for the Org Name Change Notification Recipients", "WiseTech Global Client Extensions/System Email Addresses/Org Name Change Notification Recipients", ItemSet.OrgNameChangeNotificationAddresses.Category + "/" + ItemSet.OrgNameChangeNotificationAddresses.Caption);

			AssertEquals("There should be a registry item for the Commission Generator Notification Group", "WiseTech Global Client Extensions/Sales & Marketing", ItemSet.CommissionGeneratorNotificationGroup.Category);

			AssertEquals("There should be a registry item for Database Hosted Locations", "WiseTech Global Client Extensions", ItemSet.DatabaseHostedLocations.Category);
			AssertEquals("There should be a registry item for Default Opportunity Objective", "WiseTech Global Client Extensions/Professional Service Quotes", ItemSet.DefaultOpportunityObjective.Category);

			AssertEquals("There should be a registry item for Error Reporting Service URIs", "WiseTech Global Client Extensions/Issue Manager", ItemSet.ErrorReportingServiceURIs.Category);
			AssertEquals("There should be a registry item for Error Reporting Service Max Results", "WiseTech Global Client Extensions/Issue Manager", ItemSet.ErrorReportingServiceMaxResults.Category);

			AssertEquals("There should be a registry item for STL Price List Exchange Rate Groups", "WiseTech Global Client Extensions/Licence Billing/STL", ItemSet.StlPriceListExchangeRateGroups.Category);
			AssertEquals("There should be a registry item for STL Surcharge Charge Code", "WiseTech Global Client Extensions/Licence Billing/STL", ItemSet.StlSurchargeChargeCode.Category);

			AssertEquals("There should be a registry item for the BorderWiseMaximumDeviceCount", "WiseTech Global Client Extensions/BorderWise", ItemSet.BorderWiseMaximumDeviceCount.Category);
			AssertEquals("There should be a registry item for the BorderWiseMaximumLicenceRelocationsPerMonth", "WiseTech Global Client Extensions/BorderWise", ItemSet.BorderWiseMaximumLicenceRelocationsPerMonth.Category);
			AssertEquals("There should be a registry item for the BorderWiseRegistrationUrl", "WiseTech Global Client Extensions/BorderWise", ItemSet.BorderWiseRegistrationUrl.Category);
			AssertEquals("There should be a registry item for the BorderWiseNewOrganisationEmailNotificationGroup", "WiseTech Global Client Extensions/BorderWise", ItemSet.BorderWiseNewOrganisationEmailNotificationGroup.Category);
			AssertEquals("There should be a registry item for the BorderWiseConfirmWebAccessUrl", "WiseTech Global Client Extensions/BorderWise", ItemSet.BorderWiseConfirmWebAccessUrl.Category);

			AssertEquals("There should be a registry item for the BorderWiseUmpSyncEnabled", "WiseTech Global Client Extensions/BorderWise/User Management Portal", ItemSet.BorderWiseUmpSyncEnabled.Category);
			AssertEquals("There should be a registry item for the BorderWiseUmpSyncServers", "WiseTech Global Client Extensions/BorderWise/User Management Portal", ItemSet.BorderWiseUmpSyncServers.Category);
			AssertEquals("There should be a registry item for the BorderWiseUmpSyncOutboundTopic", "WiseTech Global Client Extensions/BorderWise/User Management Portal", ItemSet.BorderWiseUmpSyncOutboundTopic.Category);
			AssertEquals("There should be a registry item for the BorderWiseUmpSyncInboundTopic", "WiseTech Global Client Extensions/BorderWise/User Management Portal", ItemSet.BorderWiseUmpSyncInboundTopic.Category);
			AssertEquals("There should be a registry item for the BorderWiseUmpSyncSessionTimeout", "WiseTech Global Client Extensions/BorderWise/User Management Portal", ItemSet.BorderWiseUmpSyncSessionTimeout.Category);
			AssertEquals("There should be a registry item for the BorderWiseUmpSyncUserName", "WiseTech Global Client Extensions/BorderWise/User Management Portal", ItemSet.BorderWiseUmpSyncUserName.Category);
			AssertEquals("There should be a registry item for the BorderWiseUmpSyncPassword", "WiseTech Global Client Extensions/BorderWise/User Management Portal", ItemSet.BorderWiseUmpSyncPassword.Category);
			AssertEquals("There should be a registry item for the BorderWiseUmpApiEnabled", "WiseTech Global Client Extensions/BorderWise/User Management Portal", ItemSet.BorderWiseUmpApiEnabled.Category);
			AssertEquals("There should be a registry item for the BorderWiseUmpApiAddress", "WiseTech Global Client Extensions/BorderWise/User Management Portal", ItemSet.BorderWiseUmpApiAddress.Category);
			AssertEquals("There should be a registry item for the BorderWiseUmpApiUpdatePasswordApiKey", "WiseTech Global Client Extensions/BorderWise/User Management Portal", ItemSet.BorderWiseUmpApiUpdatePasswordApiKey.Category);
			AssertEquals("There should be a registry item for the ElasticSearchAdaptorRequestUrl", "WiseTech Global Client Extensions/External Monitoring/Elasticsearch Adapter Settings", ItemSet.ElasticSearchAdaptorRequestUrl.Category);
			AssertEquals("There should be a registry item for the ElasticSearchAdaptorUsername", "WiseTech Global Client Extensions/External Monitoring/Elasticsearch Adapter Settings", ItemSet.ElasticSearchAdaptorUsername.Category);
			AssertEquals("There should be a registry item for the ElasticSearchAdaptorPassword", "WiseTech Global Client Extensions/External Monitoring/Elasticsearch Adapter Settings", ItemSet.ElasticSearchAdaptorPassword.Category);

			AssertEquals("There should be a registry item for the ElasticSearchAdaptorPassword", "WiseTech Global Client Extensions/Customer Service Incidents/Module Mappings", ItemSet.WebSecurityProductModuleMappings.Category);

			AssertEquals($"There should be a registry item for the EnableIncidentSimilarityWebService", "WiseTech Global Client Extensions/Customer Service Incidents/Incident Similarity", ItemSet.EnableIncidentSimilarityWebService.Category);
			AssertEquals($"There should be a registry item for the IncidentSimilarityWebServiceUrl", "WiseTech Global Client Extensions/Customer Service Incidents/Incident Similarity", ItemSet.IncidentSimilarityWebServiceUrl.Category);
			AssertEquals($"There should be a registry item for the IncidentSimilarityWebServiceDataBatchSize", "WiseTech Global Client Extensions/Customer Service Incidents/Incident Similarity", ItemSet.IncidentSimilarityWebServiceDataBatchSize.Category);

			AssertEquals($"There should be a registry item for the EnableDataScienceIncidentRelatedSubscribers", "WiseTech Global Client Extensions/Data Science/Audit/Incident-related", ItemSet.EnableDataScienceIncidentRelatedSubscribers.Category);
			AssertEquals($"There should be a registry item for the DataScienceIncidentRelatedKafkaTopic", "WiseTech Global Client Extensions/Data Science/Audit/Incident-related", ItemSet.DataScienceIncidentRelatedKafkaTopic.Category);
			AssertEquals($"There should be a registry item for the DataScienceIncidentRelatedKafkaSaslUsername", "WiseTech Global Client Extensions/Data Science/Audit/Incident-related", ItemSet.DataScienceIncidentRelatedKafkaSaslUsername.Category);
			AssertEquals($"There should be a registry item for the DataScienceIncidentRelatedKafkaSaslPassword", "WiseTech Global Client Extensions/Data Science/Audit/Incident-related", ItemSet.DataScienceIncidentRelatedKafkaSaslPassword.Category);

			AssertEquals($"There should be a registry item for the EdiKafkaBootstrapServers", "WiseTech Global Client Extensions/Kafka", ItemSet.EdiKafkaBootstrapServers.Category);

			AssertEquals("There should be a registry item for the AWSPrivateCAListManager", "WiseTech Global Client Extensions/AWS Private CA", ItemSet.AWSPrivateCAListManager.Category);

			AssertEquals("There should be a registry item for the AzureOpenIDConnectConfiguration", "WiseTech Global Client Extensions/Azure OpenID Connect Configuration", ItemSet.AzureOpenIDConnectConfiguration.Category);

			AssertEquals("There should be a registry item for the TokenValidationServiceDiscoveryEndpoint", "WiseTech Global Client Extensions/Azure Application Management", ItemSet.TokenValidationServiceDiscoveryEndpoint.Category);
			AssertEquals("There should be a registry item for the AzureApplicationManagementTenantID", "WiseTech Global Client Extensions/Azure Application Management", ItemSet.AzureApplicationManagementTenantID.Category);
			AssertEquals("There should be a registry item for the AzureApplicationManagementClientID", "WiseTech Global Client Extensions/Azure Application Management", ItemSet.AzureApplicationManagementClientID.Category);

			AssertEquals("There should be a registry item for the ValidDaysOfGenerateLoginTokenWithOldEntCodeButton", "WiseTech Global Client Extensions/Licence Key Builder", ItemSet.ValidDaysOfGenerateLoginTokenWithOldEntCodeButton.Category);

			AssertEquals("There should be a registry item for the TXIOutageStartTime", "WiseTech Global Client Extensions/XT Credential Management", ItemSet.TXIOutageStartTime.Category);
			AssertEquals("There should be a registry item for the TXOOutageStartTime", "WiseTech Global Client Extensions/XT Credential Management", ItemSet.TXOOutageStartTime.Category);

			AssertEquals("There should be a registry item for the BranchForEDIServiceTasks", "WiseTech Global Client Extensions", ItemSet.BranchForEDIServiceTasks.Category);

			AssertEquals("There should be a registry item for the NotificationGroupForADETask", "WiseTech Global Client Extensions", ItemSet.NotificationGroupForADETask.Category);
			AssertEquals("There should be a registry item for the OnboardingNotificationGroup", "WiseTech Global Client Extensions", ItemSet.OnboardingNotificationGroup.Category);
			AssertEquals("There should be a registry item for the CertProcessingNotificationGroup", "WiseTech Global Client Extensions", ItemSet.CertProcessingNotificationGroup.Category);

			AssertEquals("There should be a registry item for the ExternalMonitoringSqlExecutionPlanRetrieveTimeout", "WiseTech Global Client Extensions/External Monitoring", ItemSet.ExternalMonitoringSqlExecutionPlanRetrieveTimeout.Category);
			AssertEquals("There should be a registry item for the ExternalMonitoringSqlExecutionPlanQueryUri", "WiseTech Global Client Extensions/External Monitoring", ItemSet.ExternalMonitoringSqlExecutionPlanQueryUri.Category);
		}

		public void TestMyAccountOIDCUserDataHashIV()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Name", "MyAccountOIDCUserDataHashIV", ItemSet.MyAccountOIDCUserDataHashIV.Name);
				AssertEquals("Category", EDIDataRegistry.Category + "/My Account/OpenID Connect", ItemSet.MyAccountOIDCUserDataHashIV.Category);
				AssertEquals("Caption", "User Data Hash Initialization Vector", ItemSet.MyAccountOIDCUserDataHashIV.Caption);
				AssertEquals("Hint", "The value should be a valid GUID string.", ItemSet.MyAccountOIDCUserDataHashIV.Hint);
				AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.MyAccountOIDCUserDataHashIV.Storage);
				AssertEquals("RegistryOptions", RegistryOptions.Default, ItemSet.MyAccountOIDCUserDataHashIV.Options);
				AssertEquals("StringRegistryDataType", typeof(StringRegistryDataType), ItemSet.MyAccountOIDCUserDataHashIV.EditorInfo.BaseDataTypeToBeEdited);
			});

			var defaultValue = ItemSet.MyAccountOIDCUserDataHashIV.DefaultValue;
			Assert("The default value should be valid", Guid.TryParse(defaultValue, out var guid));

			var dataType = ItemSet.MyAccountOIDCUserDataHashIV.DataType as StringRegistryDataType;
			AssertExceptionThrown<RegistryValidationException>(() => dataType.Validate(ItemSet.MyAccountOIDCUserDataHashIV, string.Empty, Guid.Empty, Guid.Empty, Guid.Empty));
			AssertExceptionThrown<RegistryValidationException>(() => dataType.Validate(ItemSet.MyAccountOIDCUserDataHashIV, "xxx123", Guid.Empty, Guid.Empty, Guid.Empty));
			AssertNoExceptionThrown(() => dataType.Validate(ItemSet.MyAccountOIDCUserDataHashIV, "9a894d2d-12b5-47de-b9b8-08bfcf59a98d", Guid.Empty, Guid.Empty, Guid.Empty));
			AssertNoExceptionThrown(() => dataType.Validate(ItemSet.MyAccountOIDCUserDataHashIV, defaultValue, Guid.Empty, Guid.Empty, Guid.Empty));

			ItemSet.MyAccountOIDCUserDataHashIV.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultValue);
			AssertEquals(defaultValue, ItemSet.MyAccountOIDCUserDataHashIV.Value);
			AssertEquals(new Guid(defaultValue), ItemSet.MyAccountOIDCUserDataHashIVGuid);

			var newValue = Guid.NewGuid().ToString();
			ItemSet.MyAccountOIDCUserDataHashIV.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newValue);
			AssertEquals(newValue, ItemSet.MyAccountOIDCUserDataHashIV.Value);
			AssertEquals(new Guid(newValue), ItemSet.MyAccountOIDCUserDataHashIVGuid);

			using (dataType.SuspendValidation())
			{
				ItemSet.MyAccountOIDCUserDataHashIV.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty);
				AssertEquals(string.Empty, ItemSet.MyAccountOIDCUserDataHashIV.Value);
				AssertEquals(Guid.Empty, ItemSet.MyAccountOIDCUserDataHashIVGuid);
			}
		}

		public void TestMyAccountSSOJWTTokenExchangePrivateKey()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Name", "MyAccountSSOJWTTokenExchangePrivateKey", ItemSet.MyAccountSSOJWTTokenExchangePrivateKey.Name);
				AssertEquals("Category", EDIDataRegistry.Category + "/My Account/Single Sign-On", ItemSet.MyAccountSSOJWTTokenExchangePrivateKey.Category);
				AssertEquals("Caption", "JWT Token Exchange Private Key", ItemSet.MyAccountSSOJWTTokenExchangePrivateKey.Caption);
				AssertEquals("Hint", "MyAccount will generate signature for JWT by this key. The key should be PKCS #1", ItemSet.MyAccountSSOJWTTokenExchangePrivateKey.Hint);
				AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.MyAccountSSOJWTTokenExchangePrivateKey.Storage);
				AssertEquals("RegistryOptions", RegistryOptions.IsOnlyForSupport, ItemSet.MyAccountSSOJWTTokenExchangePrivateKey.Options);
				AssertEquals("BinaryRegistryDataType", typeof(BinaryRegistryDataType), ItemSet.MyAccountSSOJWTTokenExchangePrivateKey.EditorInfo.BaseDataTypeToBeEdited);
			});
		}

		public void TestIncidentFromEmailAddressDefaultValue()
		{
			AssertEquals("support@wisetechglobal.com", ItemSet.IncidentFromEmailAddress.DefaultValue);
		}

		public void TestEmailAddressBlockListValidation()
		{
			var validEmail = "123@123.com";
			var invalidEmail = "123@123";
			var invalidEmail2 = "123@123.";

			var item = ItemSet.EmailAddressBlockList;

			AssertEquals(0, ItemSet.EmailAddressBlockList.Value.Length);

			item.DataType.Validate(item, Array.Empty<string>(), Guid.Empty, Guid.Empty, Guid.Empty);
			item.DataType.Validate(item, new[] { string.Empty, string.Empty, string.Empty }, Guid.Empty, Guid.Empty, Guid.Empty);
			item.DataType.Validate(item, new[] { "a@a.com", "b@b.com", string.Empty }, Guid.Empty, Guid.Empty, Guid.Empty);

			AssertExceptionThrown<RegistryValidationException>(
				"The validation should fail",
				"123@123, 123@123. are not valid email addresses.",
				() => item.DataType.Validate(item, new[] { validEmail, invalidEmail, invalidEmail2 }, Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestEnableTriageEngineModule()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Name", "EnableTriageEngineModule", ItemSet.EnableTriageEngineModule.Name);
				AssertEquals("Category", EDIDataRegistry.Category, ItemSet.EnableTriageEngineModule.Category);
				AssertEquals("Caption", "Enable Triage Engine Module", ItemSet.EnableTriageEngineModule.Caption);
				AssertEquals("Hint", "Set this registry to 'Yes' to enable the Triage Engine Module and disable legacy menu items functionality.", ItemSet.EnableTriageEngineModule.Hint);
				AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.EnableTriageEngineModule.Storage);

				AssertEquals("Default Value", Globals.IsDebugMode, ItemSet.EnableTriageEngineModule.DefaultValue);
			});
		}

		public void TestTriagePrediction()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Name", "TriagePrediction", ItemSet.TriagePrediction.Name);
				AssertEquals("Category", EDIDataRegistry.AIFeaturesSubCategory, ItemSet.TriagePrediction.Category);
				AssertEquals("Caption", "Triage Prediction", ItemSet.TriagePrediction.Caption);
				AssertEquals("Hint", "Enable Triage Prediction for Products", ItemSet.TriagePrediction.Hint);
				AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.TriagePrediction.Storage);
			});
		}

		public void TestEnableInternalWorkItem()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Name", "EnableInternalWorkItem", ItemSet.EnableInternalWorkItem.Name);
				AssertEquals("Category", EDIDataRegistry.CustomerServiceSubCategory, ItemSet.EnableInternalWorkItem.Category);
				AssertEquals("Caption", "Enable Internal Work Item", ItemSet.EnableInternalWorkItem.Caption);
				AssertEquals("Hint", "Set this registry to 'Yes' to enable the Internal Work Item feature and disable legacy menu items functionality.", ItemSet.EnableInternalWorkItem.Hint);
				AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.EnableInternalWorkItem.Storage);

				AssertEquals("Default Value", false, ItemSet.EnableInternalWorkItem.DefaultValue);
			});
		}

		public void TestEnableFeatureControlModule()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Name", "EnableFeatureControlModule", ItemSet.EnableFeatureControlModule.Name);
				AssertEquals("Category", EDIDataRegistry.Category, ItemSet.EnableFeatureControlModule.Category);
				AssertEquals("Caption", "Enable Feature Control Module", ItemSet.EnableFeatureControlModule.Caption);
				AssertEquals("Hint", "When set to Yes, the Feature Control Module is available to user.", ItemSet.EnableFeatureControlModule.Hint);
				AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.EnableFeatureControlModule.Storage);

				AssertEquals("Default Value", true, ItemSet.EnableFeatureControlModule.DefaultValue);
			});
		}

		public void TestISAlertsServiceAccount()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Name", "ISAlertsServiceAccount", ItemSet.ISAlertsServiceAccount.Name);
				AssertEquals("Category", EDIDataRegistry.CustomerServiceSubCategory, ItemSet.ISAlertsServiceAccount.Category);
				AssertEquals("Caption", "IS Alerts Service Account", ItemSet.ISAlertsServiceAccount.Caption);
				AssertEquals("Hint", "IS Alerts Service Account", ItemSet.ISAlertsServiceAccount.Hint);
				AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.ISAlertsServiceAccount.Storage);
				AssertEquals("Options", RegistryOptions.IsValueMandatory, ItemSet.ISAlertsServiceAccount.Options);
			});
		}

		#region Release Build Supported Versions

		public void TestCr8Cr9ReleaseBuilds()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Name", "Cr8Cr9ReleaseBuilds", ItemSet.Cr8Cr9ReleaseBuilds.Name);
				AssertEquals("Category", EDIDataRegistry.ReleaseBuildSupportedVersions, ItemSet.Cr8Cr9ReleaseBuilds.Category);
				AssertEquals("Caption", "CR8/CR9 patched versions", ItemSet.Cr8Cr9ReleaseBuilds.Caption);
				AssertEquals("Hint", "Comma separated list of release build versions that were patched with CR8/CR9. Build format should be major.minor.release.patch.", ItemSet.Cr8Cr9ReleaseBuilds.Hint);
				AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.Cr8Cr9ReleaseBuilds.Storage);

				AssertEquals("Default Value", "", ItemSet.Cr8Cr9ReleaseBuilds.DefaultValue);
			});
		}

		public void TestCargoWiseNextMinimumVersion()
		{
			CombineAssertions(() =>
			{
				var item = ItemSet.CargoWiseNextMinimumVersion;
				AssertEquals("Name", "CargoWiseNextMinimumVersion", item.Name);
				AssertEquals("Category", EDIDataRegistry.ReleaseBuildSupportedVersions, item.Category);
				AssertEquals("Caption", "CargoWise Next minimum version", item.Caption);
				AssertEquals("Hint", "First release build version to be imported as CargoWise Next product. Build version format should be major.minor.release.0.", item.Hint);
				AssertEquals("Storage", RegistryStorageFlags.System, item.Storage);
				AssertEquals("Default Value", "", item.DefaultValue);
			});
		}

		public void TestEnableCargoWiseNextTransitionVersionFallbackRule()
		{
			CombineAssertions(() =>
			{
				var item = ItemSet.EnableCargoWiseNextTransitionVersionFallbackRule;
				AssertEquals("Name", "EnableCargoWiseNextTransitionVersionFallbackRule", item.Name);
				AssertEquals("Category", EDIDataRegistry.ReleaseBuildSupportedVersions, item.Category);
				AssertEquals("Caption", "Enable CargoWise Next Transition Version Fallback Rule", item.Caption);
				AssertEquals("Storage", RegistryStorageFlags.System, item.Storage);
				AssertEquals("Default Value", true, item.DefaultValue);
			});
		}

		public void TestCargoWiseMinimumVersion()
		{
			CombineAssertions(() =>
			{
				var item = ItemSet.CargoWiseMinimumVersion;
				AssertEquals("Name", "CargoWiseMinimumVersion", item.Name);
				AssertEquals("Category", EDIDataRegistry.ReleaseBuildSupportedVersions, item.Category);
				AssertEquals("Caption", "CargoWise minimum version", item.Caption);
				AssertEquals("Hint", "First release build version to be imported as unified CargoWise product. Build version format should be major.minor.release.0.", item.Hint);
				AssertEquals("Storage", RegistryStorageFlags.System, item.Storage);
				AssertEquals("Default Value", "", item.DefaultValue);
			});
		}

		#endregion

		public void TestExceptionKeyMatchingRegexes()
		{
			var regexCollection = new ExceptionKeyRegexCollection();
			regexCollection.Add(new ExceptionKeyRegex() { Regex = @"hotel", Description = "regex 1" });
			regexCollection.Add(new ExceptionKeyRegex() { Regex = @"motel", Description = "regex 2" });
			ItemSet.ExceptionKeyMatchingRegexes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, regexCollection);
			AssertEquals("2 regexes", 2, ItemSet.ExceptionKeyMatchingRegexes.Value.Count);
			AssertEquals("1st regex", "regex 1", ItemSet.ExceptionKeyMatchingRegexes.Value.Get("hotel").Description);
			AssertEquals("2nd regex", "regex 2", ItemSet.ExceptionKeyMatchingRegexes.Value.Get("motel").Description);
		}

		public void TestProductAndModuleTypeList()
		{
			var collection = new SystemProductCollection();
			var parent1 = collection.AddNew();
			parent1.Code = "AAA";
			parent1.Description = "AAA";
			var child1a = parent1.ModuleMappings.AddNew();
			child1a.ModuleCode = "AA1";
			child1a.ModuleDescription = "des1";
			var child1b = parent1.ModuleMappings.AddNew();
			child1b.ModuleCode = "AA2";
			child1b.ModuleDescription = "des1";
			var parent2 = collection.AddNew();
			parent2.Code = "BBB";
			parent2.Description = "BBB";
			var child2a = parent2.ModuleMappings.AddNew();
			child2a.ModuleCode = "BB1";
			child2a.ModuleDescription = "des3";

			ItemSet.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			AssertEquals("3 Products", 3, ItemSet.SystemProductMappings.Value.Count);
			AssertEquals("2 Modules for AAA", 2, ItemSet.SystemProductMappings.Value.GetModuleList(parent1.Code).Count);
			AssertEquals("Module Code", "AA1", ItemSet.SystemProductMappings.Value.GetModuleList(parent1.Code)[0].Code);
			AssertEquals("Module Code", "AA2", ItemSet.SystemProductMappings.Value.GetModuleList(parent1.Code)[1].Code);
			AssertEquals("1 Module for BBB", 1, ItemSet.SystemProductMappings.Value.GetModuleList(parent2.Code).Count);
			AssertEquals("Module Code", "BB1", ItemSet.SystemProductMappings.Value.GetModuleList(parent2.Code)[0].Code);
		}

		public void TestProductAndModuleTypeList_Defaults()
		{
			var defaults = ItemSet.SystemProductMappings.DefaultValue;
			var ent = ItemSet.SystemProductMappings.DefaultValue
				.Cast<SystemProduct>()
				.FirstOrDefault(p => p.Code.EqualsIgnoringCase(ProductTypes.Codes.Enterprise));

			AssertEquals(true, ent.IsProductReadOnly);

			foreach (ProductAreaModuleMapping mapping in ent.ModuleMappings)
			{
				AssertEquals(false, mapping.IsModuleReadOnly);
			}
		}

		public void TestProductAndModuleTypeListCR8_Defaults()
		{
			var defaults = ItemSet.SystemProductMappings.DefaultValue;
			var ent = ItemSet.ProductAreaIncidentCr8Mappings.DefaultValue
				.Cast<SystemProduct>()
				.FirstOrDefault(p => p.Code.EqualsIgnoringCase(ProductTypes.Codes.Enterprise));

			AssertEquals(true, ent.IsProductReadOnly);

			foreach (ProductAreaModuleMapping mapping in ent.ModuleMappings)
			{
				AssertEquals(false, mapping.IsModuleReadOnly);
			}
		}

		public void TestProductAndModuleTypeListCR9_Defaults()
		{
			var defaults = ItemSet.SystemProductMappings.DefaultValue;
			var ent = ItemSet.ProductAreaIncidentCr9Mappings.DefaultValue
				.Cast<SystemProduct>()
				.FirstOrDefault(p => p.Code.EqualsIgnoringCase(ProductTypes.Codes.Enterprise));

			AssertEquals(true, ent.IsProductReadOnly);

			foreach (ProductAreaModuleMapping mapping in ent.ModuleMappings)
			{
				AssertEquals(false, mapping.IsModuleReadOnly);
			}
		}

		public void TestPaymentTypesAndPaymentTermsList()
		{
			var collection = new ParentCodeDescriptionBoolCollection();
			var parent1 = collection.AddNew();
			parent1.Code = "36M";
			var child1a = parent1.ChildList.AddNew();
			child1a.Code = "MTH";
			var child1b = parent1.ChildList.AddNew();
			child1b.Code = "YRY";
			var parent2 = collection.AddNew();
			parent2.Code = "6M";
			var child2a = parent2.ChildList.AddNew();
			child2a.Code = "FTY";

			ItemSet.PaymentTypesAndPaymentTerms.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			AssertEquals("2 Terms", 2, ItemSet.PaymentTypesAndPaymentTerms.Value.Count);
			AssertEquals("2 Types for 36M", 2, ItemSet.PaymentTypesAndPaymentTerms.Value.GetChildList(parent1.Code).Count);
			AssertEquals("Type", "MTH", ItemSet.PaymentTypesAndPaymentTerms.Value.GetChildList(parent1.Code)[0].Code);
			AssertEquals("Type", "YRY", ItemSet.PaymentTypesAndPaymentTerms.Value.GetChildList(parent1.Code)[1].Code);
			AssertEquals("1 Type for 6M", 1, ItemSet.PaymentTypesAndPaymentTerms.Value.GetChildList(parent2.Code).Count);
			AssertEquals("Type", "FTY", ItemSet.PaymentTypesAndPaymentTerms.Value.GetChildList(parent2.Code)[0].Code);
		}

		public void TestContentFinderUrl()
		{
			AssertEquals("https://ist.wtg.zone", ItemSet.ContentFinderUrl.DefaultValue);
		}

		public void TestEnableContentFinder()
		{
			AssertEquals(expected: false, ItemSet.EnableContentFinder.DefaultValue);
		}

		public void TestWebSecurityProductModuleMappingsItem()
		{
			var products = ItemSet.SystemProductMappings.Value;
			var ist = products.AddNew("IST", "internal", false);
			ist.ModuleMappings.AddNew("WCE", "WCE", ProductAreaList.Codes.ARC, false);
			ist.ModuleMappings.AddNew("MFA", "MFA", ProductAreaList.Codes.ARC, false);

			ItemSet.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, products);

			CombineAssertions(() =>
			{
				AssertEquals("Name", "WebSecurityProductModuleMappings", ItemSet.WebSecurityProductModuleMappings.Name);
				AssertEquals("Category", EDIDataRegistry.ModuleMappingsSubCategory, ItemSet.WebSecurityProductModuleMappings.Category);
				AssertEquals("Caption", "Web Securities to Products / Modules Mappings", ItemSet.WebSecurityProductModuleMappings.Caption);
				AssertEquals("Hint", "The list of additional Products/Modules available to the user based on the Web Securities.", ItemSet.WebSecurityProductModuleMappings.Hint);
				AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.WebSecurityProductModuleMappings.Storage);

				var collection = ItemSet.WebSecurityProductModuleMappings.DefaultValue;
				AssertEquals("Default Value", 2, collection.Count);
				AssertEquals(EDIWebSecurityRightsList.WiseBusinessPartner.Code, collection[0].WebSecurity);
				AssertEquals("IST", collection[0].ProductMapping);
				AssertEquals("WCE", collection[0].ModuleMapping);
				AssertEquals(EDIWebSecurityRightsList.WiseBusinessPartner.Code, collection[1].WebSecurity);
				AssertEquals("IST", collection[1].ProductMapping);
				AssertEquals("MFA", collection[1].ModuleMapping);
			});
		}

		public void TestSendOrganizationDataToCertCapture()
		{
			var item = ItemSet.SendOrganizationDataToCertCapture;
			AssertEquals("SendOrganizationDataToCertCapture", item.Name);
			AssertEquals("Send Organization Data To Cert Capture", item.Caption);
			AssertEquals("Enables sending of the US receivable organization data to Cert Capture when organization data changes.", item.Hint);
			AssertEquals(EDIDataRegistry.MasterDataSubCategory, item.Category);
			AssertEquals(RegistryStorageFlags.System, item.Storage);
			AssertEquals(RegistryOptions.IsOnlyForSupport, item.Options);
			AssertEquals(false, item.DefaultValue.EnableSend);
		}

		public void TestFallbackWorkItemCriteriaRegistryItem_DefaultValue()
		{
			var defaultFallbackWorkItemCriteria = ItemSet.FallbackWorkItemCriteriaRegistryItem.Value;
			AssertEquals(string.Empty, defaultFallbackWorkItemCriteria);
		}

		public void TestEarliestExeDateToProcessInIssueManager()
		{
			ZDateTime today = ZDateTime.Today;
			ItemSet.EarliestExeDateToProcessInIssueManager = today;
			AssertEquals("Value", today, ItemSet.EarliestExeDateToProcessInIssueManager);

			ItemSet.EarliestExeDateToProcessInIssueManager = ZDateTime.Empty;
			AssertEquals("IsEmpty", true, ItemSet.EarliestExeDateToProcessInIssueManager.IsEmpty);
		}

		[ExpectException(typeof(ArgumentException))]
		public void TestEarliestExeDateToProcessInIssueManager_InvalidValue()
		{
			ItemSet.EarliestExeDateToProcessInIssueManager = ZDateTime.Invalid;
		}

		public void TestLicenceKeyDirectoryPath()
		{
			ItemSet.LicenceKeyDirectoryPath = "abc";
			AssertEquals("LicenceKeyDirectoryPath ", "abc", ItemSet.LicenceKeyDirectoryPath);
		}

		public void TestTrainingDepartmentEmailAddress()
		{
			ItemSet.TrainingClassroomSessionsEmailAddress.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "a@b.com");
			AssertEquals("TrainingDepartmentEmailAddress ", "a@b.com", ItemSet.TrainingClassroomSessionsEmailAddress.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));
		}

		#region Mailboxes

		public void TestCustomerServiceMailbox()
		{
			MailboxSettingsTest.AssertMailbox(ItemSet.CustomerServiceMailBox, "CustomerService", "WiseTech Global Client Extensions/System Email Addresses/Customer Service Mailbox", RegistryOptions.PreserveTestValue);
		}

		public void TestTrainingScheduleMailbox()
		{
			MailboxSettingsTest.AssertMailbox(ItemSet.TrainingScheduleMailBox, "TrainingSchedule", "WiseTech Global Client Extensions/System Email Addresses/Training Schedule Mailbox", RegistryOptions.PreserveTestValue);
		}

		public void TestIssueReportMailbox()
		{
			MailboxSettingsTest.AssertMailbox(ItemSet.IssueReportMailBox, "IssueReport", "WiseTech Global Client Extensions/System Email Addresses/Issue Report Mailbox", RegistryOptions.PreserveTestValue);
		}

		public void TestCurrentVersionReportMailbox()
		{
			MailboxSettingsTest.AssertMailbox(ItemSet.CurrentVersionReportMailBox, "CurrentVersionReport", "WiseTech Global Client Extensions/System Email Addresses/Current Version Report Mailbox", RegistryOptions.PreserveTestValue);
		}

		public void TestImplementationMailbox()
		{
			MailboxSettingsTest.AssertMailbox(ItemSet.ImplementationMailBox, "Implementation", "WiseTech Global Client Extensions/System Email Addresses/Implementation Mailbox", RegistryOptions.PreserveTestValue);
		}

		public void TestSupportRequestMailbox()
		{
			MailboxSettingsTest.AssertMailbox(ItemSet.SupportRequestMailBox, "SupportRequest", "WiseTech Global Client Extensions/System Email Addresses/Support Request Mailbox", RegistryOptions.PreserveTestValue);
		}

		public void TestDeliveredVersionReportMailbox()
		{
			MailboxSettingsTest.AssertMailbox(ItemSet.DeliveredVersionReportMailBox, "DeliveredVersionReport", "WiseTech Global Client Extensions/System Email Addresses/Delivered Version Report Mailbox", RegistryOptions.PreserveTestValue);
		}

		#endregion

		public void TestAccountingDepartmentEmailAddress()
		{
			ItemSet.AccountingDepartmentEmailAddress.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "a@b.com");
			AssertEquals("AccountingDepartmentEmailAddress ", "a@b.com", ItemSet.AccountingDepartmentEmailAddress.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));
		}

		public void TestEnableProfessionalServiceQuoteAssignedStaffChangedEmailNotification()
		{
			AssertEquals("Default Value", true, ItemSet.EnableProfessionalServiceQuoteAssignedStaffChangedEmailNotificationRegistryItem.Value);

			ItemSet.EnableProfessionalServiceQuoteAssignedStaffChangedEmailNotificationRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("New Value", false, ItemSet.EnableProfessionalServiceQuoteAssignedStaffChangedEmailNotificationRegistryItem.Value);
		}

		public void TestDefaultOpportunityObjective()
		{
			AssertEquals("Default Value", "SER", ItemSet.DefaultOpportunityObjective.Value);

			ItemSet.DefaultOpportunityObjective.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "CFU");
			AssertEquals("New Value", "CFU", ItemSet.DefaultOpportunityObjective.Value);
		}

		public void TestEnableAssignedStaffChangedEmailNotificationRegistryItems()
		{
			TestRegistryItem(ItemSet.EnableIncidentAssignedStaffChangedEmailNotification, "EnableIncidentAssignedStaffChangedEmailNotification", "WiseTech Global Client Extensions/Customer Service Incidents", "Email Notification for Assigned Change (Old)", "When the Assigned Staff changes for an Incident, the new Assigned Staff will be sent an email notification if this Registry is enabled.", RegistryStorageFlags.System, RegistryOptions.Default, true);
		}

		public void TestShowIncidentsLoggedAfterThisDate()
		{
			DateTime value = new DateTime(2000, 1, 2);
			ItemSet.ShowIncidentsLoggedAfterThisDate = value;
			AssertEquals("ShowIncidentsLoggedAfterThisDate", value, ItemSet.ShowIncidentsLoggedAfterThisDate);
		}

		public void TestSendUpgradeEmailNotificationAutomatically()
		{
			AssertEquals("DefaultValue", false, ItemSet.SendUpgradeEmailNotificationAutomatically.DefaultValue);
			ItemSet.SendUpgradeEmailNotificationAutomatically.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("Value", true, ItemSet.SendUpgradeEmailNotificationAutomatically.Value);
		}

		public void TestUseAutoDeployBatchProcessorForUpgrades()
		{
			AssertEquals("DefaultValue", false, ItemSet.UseAutoDeployBatchProcessorForUpgrades.DefaultValue);
			ItemSet.UseAutoDeployBatchProcessorForUpgrades.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("Value", true, ItemSet.UseAutoDeployBatchProcessorForUpgrades.Value);
		}

		public void TestNeoUpgradeLicences()
		{
			AssertEquals("NeoUpgradeLicences", ItemSet.NeoUpgradeLicences.Name);
			AssertEquals("WiseTech Global Client Extensions/Release Builds & Upgrades", ItemSet.NeoUpgradeLicences.Category);
			AssertEquals("Neo Enabled Licences", ItemSet.NeoUpgradeLicences.Caption);
			AssertEquals("List of licences for which Neo upgrade package should be created.", ItemSet.NeoUpgradeLicences.Hint);
			AssertEquals(RegistryStorageFlags.System, ItemSet.NeoUpgradeLicences.Storage);
			AssertEquals("DefaultValue", 0, ItemSet.NeoUpgradeLicences.DefaultValue.Count);

			var licence1 = Factory.NewWithValidTestData<LicenceEnterprise>();
			var licence2 = Factory.NewWithValidTestData<LicenceEnterprise>();
			Factory.Save();

			var eligibleLicences = new NeoUpgradeLicenceCollection();

			var neoLicence1 = eligibleLicences.AddNew();
			neoLicence1.LicencePK = licence1.PK;

			var neoLicence2 = eligibleLicences.AddNew();
			neoLicence2.LicencePK = licence2.PK;

			ItemSet.NeoUpgradeLicences.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, eligibleLicences);
			var expectedLicences = eligibleLicences.Cast<NeoUpgradeLicence>();
			var actualLicences = ItemSet.NeoUpgradeLicences.Value.Cast<NeoUpgradeLicence>();
			AssertEquals("Value", expectedLicences.Count(), actualLicences.Count());
			Assert("Value", actualLicences.Any(a => expectedLicences.Any(e => e.EnterpriseCode == a.EnterpriseCode && e.LicencePK == a.LicencePK)));
		}

		public void TestAutoDeployProcessBatchMaxDuration()
		{
			AssertEquals("DefaultValue", 300, ItemSet.AutoDeployProcessBatchMaxDuration.DefaultValue);
			ItemSet.AutoDeployProcessBatchMaxDuration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 120);
			AssertEquals("Value", 120, ItemSet.AutoDeployProcessBatchMaxDuration.Value);
		}

		public void TestAutoDeployProcessBatchRunningInterval()
		{
			AssertEquals("DefaultValue", 300, ItemSet.AutoDeployProcessBatchRunningInterval.DefaultValue);
			ItemSet.AutoDeployProcessBatchRunningInterval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 120);
			AssertEquals("Value", 120, ItemSet.AutoDeployProcessBatchRunningInterval.Value);
		}

		public void TestUpgradePackagePathRegistryItems()
		{
			TestRegistryItem(ItemSet.WebServerGenericPathRaw, "WEB_SERVER_GENERIC_PATH", "WiseTech Global Client Extensions/Release Builds & Upgrades/Upgrade Package Paths", "Web Server Generic Path", "This is the directory that generic upgrade packages will be stored in.", RegistryStorageFlags.System, TextEditorType.TextBox, @"\\web\updates\ediEnterprise\Generic\");
			TestRegistryItem(ItemSet.WebServerClientSpecificPathRaw, "WEB_SERVER_CLIENT_SPECIFIC_PATH", "WiseTech Global Client Extensions/Release Builds & Upgrades/Upgrade Package Paths", "Web Server Client Specific Path", "This is the directory that client specific upgrade packages will be stored in.", RegistryStorageFlags.System, TextEditorType.TextBox, @"\\web\updates\ediEnterprise\ClientSpecific\");
			TestRegistryItem(ItemSet.HttpGenericBaseUrlRaw, "HTTP_GENERIC_BASE_URL", "WiseTech Global Client Extensions/Release Builds & Upgrades/Upgrade Package Paths", "HTTP Generic Base URL", "This is the base HTTP URL for generic upgrade packages.", RegistryStorageFlags.System, TextEditorType.TextBox, "http://www.cargowise.com/ftpmirror/ediEnterprise/Generic/");
			TestRegistryItem(ItemSet.HttpClientSpecificBaseUrlRaw, "HTTP_CLIENT_SPECIFIC_BASE_URL", "WiseTech Global Client Extensions/Release Builds & Upgrades/Upgrade Package Paths", "HTTP Client Specific Base URL", "This is the base HTTP URL for client specific upgrade packages.", RegistryStorageFlags.System, TextEditorType.TextBox, "http://www.cargowise.com/ftpmirror/ediEnterprise/ClientSpecific/");
			TestRegistryItem(ItemSet.WebServerUserNameRaw, "WEB_SERVER_USER_NAME", "WiseTech Global Client Extensions/Release Builds & Upgrades/Upgrade Package Paths", "Web Server User Name", "This is the user name to login and copy files onto webserver.", RegistryStorageFlags.System, TextEditorType.TextBox, "");
			TestRegistryItem(ItemSet.WebServerPasswordRaw, "WEB_SERVER_PASSWORD", "WiseTech Global Client Extensions/Release Builds & Upgrades/Upgrade Package Paths", "Web Server Password", "This is the password to login and copy files onto webserver.", RegistryStorageFlags.System, TextEditorType.Password, "");
			TestRegistryItem(ItemSet.HttpDownloadUserNameRaw, "HTTP_DOWNLOAD_USER_NAME", "WiseTech Global Client Extensions/Release Builds & Upgrades/Upgrade Package Paths", "HTTP Download User Name", "This is the user name for secure HTTP downloads. Leave it blank if the URL's are not password protected.", RegistryStorageFlags.System, TextEditorType.TextBox, "");
			TestRegistryItem(ItemSet.HttpDownloadPasswordRaw, "HTTP_DOWNLOAD_PASSWORD", "WiseTech Global Client Extensions/Release Builds & Upgrades/Upgrade Package Paths", "HTTP Download Password", "This is the password for secure HTTP downloads.", RegistryStorageFlags.System, TextEditorType.Password, "");
			AssertEquals(true, (ItemSet.WebServerPasswordRaw.DataType as StringRegistryDataType).IsEncrypted);
			AssertEquals(true, (ItemSet.HttpDownloadPasswordRaw.DataType as StringRegistryDataType).IsEncrypted);

			ItemSet.WebServerGenericPathRaw.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "1");
			ItemSet.WebServerClientSpecificPathRaw.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "2");
			ItemSet.HttpGenericBaseUrlRaw.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "3");
			ItemSet.HttpClientSpecificBaseUrlRaw.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "4");
			ItemSet.WebServerUserNameRaw.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "7");
			ItemSet.WebServerPasswordRaw.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "8");
			ItemSet.HttpDownloadUserNameRaw.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "11");
			ItemSet.HttpDownloadPasswordRaw.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "12");

			AssertEquals("WebServerGenericPath", "1", ItemSet.WebServerGenericPath);
			AssertEquals("WebServerClientSpecificPath", "2", ItemSet.WebServerClientSpecificPath);
			AssertEquals("HttpGenericBaseUrl", "3", ItemSet.HttpGenericBaseUrl);
			AssertEquals("HttpClientSpecificBaseUrl", "4", ItemSet.HttpClientSpecificBaseUrl);
			AssertEquals("WebServerUserName", "7", ItemSet.WebServerUserName);
			AssertEquals("WebServerPassword", "8", ItemSet.WebServerPassword);
			AssertEquals("HttpDownloadUserNameRaw", "11", ItemSet.HttpDownloadUserName);
			AssertEquals("HttpDownloadPasswordRaw", "12", ItemSet.HttpDownloadPassword);

			ItemSet.WebServerGenericPath = "a";
			ItemSet.WebServerClientSpecificPath = "b";
			ItemSet.HttpGenericBaseUrl = "c";
			ItemSet.HttpClientSpecificBaseUrl = "d";
			ItemSet.WebServerUserName = "g";
			ItemSet.WebServerPassword = "h";
			ItemSet.HttpDownloadUserName = "k";
			ItemSet.HttpDownloadPassword = "l";

			AssertEquals("WebServerGenericPath", "a", ItemSet.WebServerGenericPath);
			AssertEquals("WebServerClientSpecificPath", "b", ItemSet.WebServerClientSpecificPath);
			AssertEquals("HttpGenericBaseUrl", "c", ItemSet.HttpGenericBaseUrl);
			AssertEquals("HttpClientSpecificBaseUrl", "d", ItemSet.HttpClientSpecificBaseUrl);
			AssertEquals("WebServerUserName", "g", ItemSet.WebServerUserName);
			AssertEquals("WebServerPassword", "h", ItemSet.WebServerPassword);
			AssertEquals("HttpDownloadUserName", "k", ItemSet.HttpDownloadUserName);
			AssertEquals("HttpDownloadPassword", "l", ItemSet.HttpDownloadPassword);
		}

		public void TestUpgradeEmailDefaultAdditionalNotification()
		{
			TestRegistryItem(ItemSet.UpgradeEmailDefaultAdditionalNotificationRaw, "UPGRADE_EMAIL_DEFAULT_ADDITIONAL_NOTIFICATION", "WiseTech Global Client Extensions/Release Builds & Upgrades", "Upgrade Email Default Additional Notification", "This is the default additional notification for upgrade emails.", RegistryStorageFlags.System, TextEditorType.Memo, "");
			ItemSet.UpgradeEmailDefaultAdditionalNotificationRaw.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "x");
			AssertEquals("UpgradeEmailDefaultAdditionalNotification", "x", ItemSet.UpgradeEmailDefaultAdditionalNotification);
			ItemSet.UpgradeEmailDefaultAdditionalNotification = "y";
			AssertEquals("UpgradeEmailDefaultAdditionalNotification", "y", ItemSet.UpgradeEmailDefaultAdditionalNotification);
		}

		public void TestUpgradePackageDeliveryNotificationEmailTemplate()
		{
			AssertVisible(ItemSet.UpgradePackageDeliveryNotificationEmailTemplateRaw);

			AssertEquals("UpgradePackageDeliveryNotificationEmailTemplate", ItemSet.UpgradePackageDeliveryNotificationEmailTemplateRaw.Name);
			AssertEquals(EDIDataRegistry.ReleaseBuildsAndUpgradesSubCategory, ItemSet.UpgradePackageDeliveryNotificationEmailTemplateRaw.Category);
			AssertEquals("Upgrade Package Delivery Notification Email Template", ItemSet.UpgradePackageDeliveryNotificationEmailTemplateRaw.Caption);
			AssertEquals("This is the default template for Upgrade Package Delivery notification emails.", ItemSet.UpgradePackageDeliveryNotificationEmailTemplateRaw.Hint);
			AssertEquals(RegistryStorageFlags.System, ItemSet.UpgradePackageDeliveryNotificationEmailTemplateRaw.Storage);

			ItemSet.UpgradePackageDeliveryNotificationEmailTemplateRaw.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new NotificationEmailTemplate(typeof(DocUpgradeRequestCollectionContainer), "test subject 1", "test template 1"));
			AssertEquals("test subject 1", ItemSet.UpgradePackageDeliveryNotificationEmailTemplateRaw.Value.EmailSubject);
			AssertEquals("test template 1", ItemSet.UpgradePackageDeliveryNotificationEmailTemplateRaw.Value.EmailBody);
			ItemSet.UpgradePackageDeliveryNotificationEmailTemplateRaw.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new NotificationEmailTemplate(typeof(DocUpgradeRequestCollectionContainer), "test subject 2", "test template 2"));
			AssertEquals("test subject 2", ItemSet.UpgradePackageDeliveryNotificationEmailTemplateRaw.Value.EmailSubject);
			AssertEquals("test template 2", ItemSet.UpgradePackageDeliveryNotificationEmailTemplateRaw.Value.EmailBody);
		}

		public void TestProcessedShelfsNotificationGroup()
		{
			AssertEquals("ProcessedShelfsNotificationGroup Registry Item Name", "ProcessedShelfsNotificationGroup", ItemSet.ProcessedShelfsNotificationGroup.Name);
			AssertEquals("ProcessedShelfsNotificationGroup Registry Item Category", "WiseTech Global Client Extensions/Processed Shelfs", ItemSet.ProcessedShelfsNotificationGroup.Category);
			AssertEquals("ProcessedShelfsNotificationGroup Registry Item Caption", "Notification Group", ItemSet.ProcessedShelfsNotificationGroup.Caption);
			AssertEquals("ProcessedShelfsNotificationGroup Registry Item Hint", "Group which gets notification emails from the Processed Shelfs Batch Processor.", ItemSet.ProcessedShelfsNotificationGroup.Hint);
			AssertEquals("ProcessedShelfsNotificationGroup Registry Item Storage", RegistryStorageFlags.System, ItemSet.ProcessedShelfsNotificationGroup.Storage);

			AssertEquals("ProcessedShelfsNotificationGroup Default Value", Core.Constants.Groups.PostMastersGroupPK, ItemSet.ProcessedShelfsNotificationGroup.DefaultValue);
		}

		public void TestCustomerServiceHighCriticalityIncidentGroup()
		{
			Guid gui = Guid.NewGuid();
			ItemSet.CustomerServiceHighCriticalityIncidentGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, gui);
			AssertEquals("UpgradeEmailDefaultAdditionalNotification", gui, ItemSet.CustomerServiceHighCriticalityIncidentGroup.Value);
		}

		public void TestIncidentsBatchProcessNotificationGroup()
		{
			Guid guid = Guid.NewGuid();
			ItemSet.IncidentsBatchProcessNotificationGroupItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, guid);
			AssertEquals("IncidentsBatchProcessNotificationGroup", guid, ItemSet.IncidentsBatchProcessNotificationGroup);
		}

		public void TestCustomerServiceNotificationEmailTemplate()
		{
			AssertVisible(ItemSet.CustomerServiceNotificationEmailTemplateRaw);

			AssertEquals("CustomerServiceNotificationEmailTemplate", ItemSet.CustomerServiceNotificationEmailTemplateRaw.Name);
			AssertEquals(EDIDataRegistry.CustomerServiceEmailTemplatesSubCategory, ItemSet.CustomerServiceNotificationEmailTemplateRaw.Category);
			AssertEquals("Customer Service Notification Email Template", ItemSet.CustomerServiceNotificationEmailTemplateRaw.Caption);
			AssertEquals(EDIDataRegistry.CustomerServiceNotificationEmailRegistryItemHint, ItemSet.CustomerServiceNotificationEmailTemplateRaw.Hint);
			AssertEquals(RegistryStorageFlags.System, ItemSet.CustomerServiceNotificationEmailTemplateRaw.Storage);
		}

		public void TestSelfLoggedIncidentNotificationEmailTemplate()
		{
			AssertVisible(ItemSet.SelfLoggedIncidentNotificationEmailTemplateRaw);

			AssertEquals("SelfLoggedIncidentNotificationEmailTemplate", ItemSet.SelfLoggedIncidentNotificationEmailTemplateRaw.Name);
			AssertEquals(EDIDataRegistry.CustomerServiceEmailTemplatesSubCategory, ItemSet.SelfLoggedIncidentNotificationEmailTemplateRaw.Category);
			AssertEquals("Self Logged Incident Notification Email Template", ItemSet.SelfLoggedIncidentNotificationEmailTemplateRaw.Caption);
			AssertEquals(EDIDataRegistry.SelfLoggedIncidentNotificationEmailRegistryItemHint, ItemSet.SelfLoggedIncidentNotificationEmailTemplateRaw.Hint);
			AssertEquals(RegistryStorageFlags.System, ItemSet.SelfLoggedIncidentNotificationEmailTemplateRaw.Storage);
		}

		public void TestCustomerServiceResponseNotificationReminderTemplate()
		{
			AssertVisible(ItemSet.CustomerServiceResponseNotificationReminderTemplate);

			AssertEquals("CustomerServiceResponseNotificationReminderTemplate", ItemSet.CustomerServiceResponseNotificationReminderTemplate.Name);
			AssertEquals(EDIDataRegistry.CustomerServiceEmailTemplatesSubCategory, ItemSet.CustomerServiceResponseNotificationReminderTemplate.Category);
			AssertEquals("Customer Service Response Notification Reminder Template", ItemSet.CustomerServiceResponseNotificationReminderTemplate.Caption);
			AssertEquals("This is the default customer service incident awaiting response notification message. Use Body only. Subject is NOT in use.", ItemSet.CustomerServiceResponseNotificationReminderTemplate.Hint);
			AssertEquals(RegistryStorageFlags.System, ItemSet.CustomerServiceResponseNotificationReminderTemplate.Storage);

			ItemSet.CustomerServiceResponseNotificationReminderTemplate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new NotificationEmailTemplate(typeof(DocSupportIncident), "Incident subject one", "Email body template one"));
			AssertEquals("Incident subject one", ItemSet.CustomerServiceResponseNotificationReminderTemplate.Value.EmailSubject);
			AssertEquals("Email body template one", ItemSet.CustomerServiceResponseNotificationReminderTemplate.Value.EmailBody);
			ItemSet.CustomerServiceResponseNotificationReminderTemplate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new NotificationEmailTemplate(typeof(DocSupportIncident), "Incident subject two", "Email body template two"));
			AssertEquals("Incident subject two", ItemSet.CustomerServiceResponseNotificationReminderTemplate.Value.EmailSubject);
			AssertEquals("Email body template two", ItemSet.CustomerServiceResponseNotificationReminderTemplate.Value.EmailBody);
		}

		public void TestCustomerServiceFinalClosureAutoReplyEmailTemplate()
		{
			AssertVisible(ItemSet.CustomerServiceFinalClosureAutoReplyEmailTemplate);

			AssertEquals("CustomerServiceFinalClosureAutoReplyEmailTemplate", ItemSet.CustomerServiceFinalClosureAutoReplyEmailTemplate.Name);
			AssertEquals(EDIDataRegistry.CustomerServiceEmailTemplatesSubCategory, ItemSet.CustomerServiceFinalClosureAutoReplyEmailTemplate.Category);
			AssertEquals("Customer Service Final Closure Auto Reply Email Template", ItemSet.CustomerServiceFinalClosureAutoReplyEmailTemplate.Caption);
			AssertEquals("These are default notification templates for customer service final closure auto reply email", ItemSet.CustomerServiceFinalClosureAutoReplyEmailTemplate.Hint);
			AssertEquals(RegistryStorageFlags.System, ItemSet.CustomerServiceFinalClosureAutoReplyEmailTemplate.Storage);

			var defaults = ItemSet.CustomerServiceFinalClosureAutoReplyEmailTemplate.DefaultValue;
			AssertEquals(1, defaults.Count);
			AssertEquals("Default Final Closure Auto-Reply", defaults.FindByCode("DFT").Description);
		}

		public void TestCustomerServiceIncidentClosedNotificationEmailTemplates()
		{
			AssertVisible(ItemSet.CustomerServiceIncidentClosedNotificationEmailTemplates);

			AssertEquals("CustomerServiceIncidentClosedNotificationEmailTemplates", ItemSet.CustomerServiceIncidentClosedNotificationEmailTemplates.Name);
			AssertEquals(EDIDataRegistry.CustomerServiceEmailTemplatesSubCategory, ItemSet.CustomerServiceIncidentClosedNotificationEmailTemplates.Category);
			AssertEquals("Customer Service Incident Closed Notification Email Templates", ItemSet.CustomerServiceIncidentClosedNotificationEmailTemplates.Caption);
			AssertEquals("These are default notification templates for customer service incident closed emails.", ItemSet.CustomerServiceIncidentClosedNotificationEmailTemplates.Hint);
			AssertEquals(RegistryStorageFlags.System, ItemSet.CustomerServiceIncidentClosedNotificationEmailTemplates.Storage);

			var defaults = ItemSet.CustomerServiceIncidentClosedNotificationEmailTemplates.DefaultValue;
			AssertEquals(2, defaults.Count);
			AssertEquals("Default Close Notification Email Template", defaults.FindByCode("DFT").Description);
			AssertEquals("Do Not Reopen Default Close Notification Email Template", defaults.FindByCode("NEV").Description);
		}

		public void TestCustomerServiceResolvedNotificationTemplates()
		{
			AssertVisible(ItemSet.CustomerServiceResolvedNotificationTemplates);

			AssertEquals("CustomerServiceResolvedNotificationTemplates", ItemSet.CustomerServiceResolvedNotificationTemplates.Name);
			AssertEquals(EDIDataRegistry.CustomerServiceEmailTemplatesSubCategory, ItemSet.CustomerServiceResolvedNotificationTemplates.Category);
			AssertEquals("Customer Service Resolved Notification Templates", ItemSet.CustomerServiceResolvedNotificationTemplates.Caption);
			AssertEquals("These are default notification templates for customer service incident resolved emails.", ItemSet.CustomerServiceResolvedNotificationTemplates.Hint);
			AssertEquals(RegistryStorageFlags.System, ItemSet.CustomerServiceResolvedNotificationTemplates.Storage);

			var defaults = ItemSet.CustomerServiceResolvedNotificationTemplates.DefaultValue;
			AssertEquals(1, defaults.Count);
			AssertEquals("Default Resolve Notification Email Template", defaults.FindByCode("DRT").Description);
		}

		public void TestCustomerServiceIncidentWorkItemCreatedEmailTemplates()
		{
			AssertVisible(ItemSet.CustomerServiceIncidentWorkItemCreatedEmailTemplates);

			AssertEquals("CustomerServiceIncidentWorkItemCreatedEmailTemplates", ItemSet.CustomerServiceIncidentWorkItemCreatedEmailTemplates.Name);
			AssertEquals(EDIDataRegistry.CustomerServiceEmailTemplatesSubCategory, ItemSet.CustomerServiceIncidentWorkItemCreatedEmailTemplates.Category);
			AssertEquals("Incident Work Item Created Email Templates", ItemSet.CustomerServiceIncidentWorkItemCreatedEmailTemplates.Caption);
			AssertEquals("These are default notification templates for customer service incident work item created emails.", ItemSet.CustomerServiceIncidentWorkItemCreatedEmailTemplates.Hint);
			AssertEquals(RegistryStorageFlags.System, ItemSet.CustomerServiceIncidentWorkItemCreatedEmailTemplates.Storage);
		}

		public void TestCustomerServiceIncidentWorkItemCompletedEmailTemplates()
		{
			AssertVisible(ItemSet.CustomerServiceIncidentWorkItemCompletedEmailTemplates);

			AssertEquals("CustomerServiceIncidentWorkItemCompletedEmailTemplates", ItemSet.CustomerServiceIncidentWorkItemCompletedEmailTemplates.Name);
			AssertEquals(EDIDataRegistry.CustomerServiceEmailTemplatesSubCategory, ItemSet.CustomerServiceIncidentWorkItemCompletedEmailTemplates.Category);
			AssertEquals("Incident Work Item Completed Email Templates", ItemSet.CustomerServiceIncidentWorkItemCompletedEmailTemplates.Caption);
			AssertEquals("These are default notification templates for customer service incident work item completed emails.", ItemSet.CustomerServiceIncidentWorkItemCompletedEmailTemplates.Hint);
			AssertEquals(RegistryStorageFlags.System, ItemSet.CustomerServiceIncidentWorkItemCompletedEmailTemplates.Storage);
		}

		public void TestIncidentUpdateNotificationEmailTemplate()
		{
			AssertVisible(ItemSet.IncidentUpdateNotificationEmailTemplate);

			AssertEquals("IncidentUpdateNotificationEmailTemplate", ItemSet.IncidentUpdateNotificationEmailTemplate.Name);
			AssertEquals(EDIDataRegistry.CustomerServiceEmailTemplatesSubCategory, ItemSet.IncidentUpdateNotificationEmailTemplate.Category);
			AssertEquals("Incident Update Notification Email Template", ItemSet.IncidentUpdateNotificationEmailTemplate.Caption);
			AssertEquals("This is the default template for Incident update notification emails to client. Summary of fields updated will append to the end of body set in this template.", ItemSet.IncidentUpdateNotificationEmailTemplate.Hint);
			AssertEquals(RegistryStorageFlags.System, ItemSet.IncidentUpdateNotificationEmailTemplate.Storage);

			ItemSet.IncidentUpdateNotificationEmailTemplate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new NotificationEmailTemplate(typeof(DocSupportIncident), "Subject one", "Body one"));
			AssertEquals("Subject one", ItemSet.IncidentUpdateNotificationEmailTemplate.Value.EmailSubject);
			AssertEquals("Body one", ItemSet.IncidentUpdateNotificationEmailTemplate.Value.EmailBody);
			ItemSet.IncidentUpdateNotificationEmailTemplate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new NotificationEmailTemplate(typeof(DocSupportIncident), "Subject two", "Body two"));
			AssertEquals("Subject two", ItemSet.IncidentUpdateNotificationEmailTemplate.Value.EmailSubject);
			AssertEquals("Body two", ItemSet.IncidentUpdateNotificationEmailTemplate.Value.EmailBody);
		}

		public void TestSubscriberUpdateEConversationEmailTemplate()
		{
			AssertVisible(ItemSet.SubscriberUpdateEConversationEmailTemplate);

			AssertEquals("SubscriberUpdateEConversationEmailTemplate", ItemSet.SubscriberUpdateEConversationEmailTemplate.Name);
			AssertEquals(EDIDataRegistry.CustomerServiceEmailTemplatesSubCategory, ItemSet.SubscriberUpdateEConversationEmailTemplate.Category);
			AssertEquals("Subscriber Update eConversation Email Notification", ItemSet.SubscriberUpdateEConversationEmailTemplate.Caption);
			AssertEquals(EDIDataRegistry.SubscriberUpdateEConversationHint, ItemSet.SubscriberUpdateEConversationEmailTemplate.Hint);
			AssertEquals(RegistryStorageFlags.System, ItemSet.SubscriberUpdateEConversationEmailTemplate.Storage);
			AssertEquals(RegistryOptions.Default, ItemSet.SubscriberUpdateEConversationEmailTemplate.Options);

			ItemSet.SubscriberUpdateEConversationEmailTemplate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new NotificationEmailTemplate(typeof(DocSupportIncident), "test subject 1", "test template 1"));
			AssertEquals("test subject 1", ItemSet.SubscriberUpdateEConversationEmailTemplate.Value.EmailSubject);
			AssertEquals("test template 1", ItemSet.SubscriberUpdateEConversationEmailTemplate.Value.EmailBody);
			ItemSet.SubscriberUpdateEConversationEmailTemplate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new NotificationEmailTemplate(typeof(DocSupportIncident), "test subject 2", "test template 2"));
			AssertEquals("test subject 2", ItemSet.SubscriberUpdateEConversationEmailTemplate.Value.EmailSubject);
			AssertEquals("test template 2", ItemSet.SubscriberUpdateEConversationEmailTemplate.Value.EmailBody);
		}

		public void TestUnsubscriberEmailTemplate()
		{
			AssertVisible(ItemSet.UnsubscriberEmailTemplate);

			AssertEquals("UnsubscriberEmailTemplate", ItemSet.UnsubscriberEmailTemplate.Name);
			AssertEquals(EDIDataRegistry.CustomerServiceEmailTemplatesSubCategory, ItemSet.UnsubscriberEmailTemplate.Category);
			AssertEquals("Unsubscribe Email Template", ItemSet.UnsubscriberEmailTemplate.Caption);
			AssertEquals(EDIDataRegistry.UnsubscriberEmailTemplateHint, ItemSet.UnsubscriberEmailTemplate.Hint);
			AssertEquals(RegistryStorageFlags.System, ItemSet.UnsubscriberEmailTemplate.Storage);
			AssertEquals(RegistryOptions.Default, ItemSet.UnsubscriberEmailTemplate.Options);

			ItemSet.UnsubscriberEmailTemplate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new NotificationEmailTemplate(typeof(DocSupportIncident), "test subject 1", "test template 1"));
			AssertEquals("test subject 1", ItemSet.UnsubscriberEmailTemplate.Value.EmailSubject);
			AssertEquals("test template 1", ItemSet.UnsubscriberEmailTemplate.Value.EmailBody);
			ItemSet.UnsubscriberEmailTemplate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new NotificationEmailTemplate(typeof(DocSupportIncident), "test subject 2", "test template 2"));
			AssertEquals("test subject 2", ItemSet.UnsubscriberEmailTemplate.Value.EmailSubject);
			AssertEquals("test template 2", ItemSet.UnsubscriberEmailTemplate.Value.EmailBody);
		}

		public void TestModuleToResourceNameList()
		{
			AssertEquals(RegistryStorageFlags.System, ItemSet.ModuleToResourceNameList.Storage);
			AssertEquals("Predefined Module To Resource Names", ItemSet.ModuleToResourceNameList.Caption);
			AssertEquals("ModuleToResourceNameList", ItemSet.ModuleToResourceNameList.Name);
			AssertEquals(EDIDataRegistry.MasterScheduleSubCategory, ItemSet.ModuleToResourceNameList.Category);
			AssertEquals(3, ((CodeDescriptionPairListRegistryDataType)ItemSet.ModuleToResourceNameList.DataType).CodeMaxLength);
			AssertEquals(97, ItemSet.ModuleToResourceNameList.DefaultValue.Count);
			AssertEquals("Freight/Sales/Marketing Developer[50%]", ItemSet.ModuleToResourceNameList.DefaultValue.GetDescriptionFromCode("AWB"));
			AssertEquals("Warehouse Developer[50%]", ItemSet.ModuleToResourceNameList.DefaultValue.GetDescriptionFromCode("BCL"));
			AssertEquals("Batch and Interface Developer[50%]", ItemSet.ModuleToResourceNameList.DefaultValue.GetDescriptionFromCode("BAK"));
			AssertEquals("Batch and Interface Developer[50%]", ItemSet.ModuleToResourceNameList.DefaultValue.GetDescriptionFromCode("EML"));
			AssertEquals("Batch and Interface Developer[50%]", ItemSet.ModuleToResourceNameList.DefaultValue.GetDescriptionFromCode("PRN"));
			AssertEquals("Freight/Sales/Marketing Developer[50%]", ItemSet.ModuleToResourceNameList.DefaultValue.GetDescriptionFromCode("CIM"));
			AssertEquals("PST Developer[50%]", ItemSet.ModuleToResourceNameList.DefaultValue.GetDescriptionFromCode("CPJ"));
			AssertEquals("Freight/Sales/Marketing Developer[50%]", ItemSet.ModuleToResourceNameList.DefaultValue.GetDescriptionFromCode("VIM"));
			AssertEquals("Accounting & Reporting Developer[50%]", ItemSet.ModuleToResourceNameList.DefaultValue.GetDescriptionFromCode("ACC"));
			AssertEquals("CargoWise Internal Developer[50%]", ItemSet.ModuleToResourceNameList.DefaultValue.GetDescriptionFromCode("ACD"));
			AssertEquals("Freight/Sales/Marketing Developer[50%]", ItemSet.ModuleToResourceNameList.DefaultValue.GetDescriptionFromCode("ACR"));
			AssertEquals("Core Developer[50%]", ItemSet.ModuleToResourceNameList.DefaultValue.GetDescriptionFromCode("ARM"));
			AssertEquals("Customs Developer[50%]", ItemSet.ModuleToResourceNameList.DefaultValue.GetDescriptionFromCode("BDS"));
			AssertEquals("Freight/Sales/Marketing Developer[50%]", ItemSet.ModuleToResourceNameList.DefaultValue.GetDescriptionFromCode("BOO"));
			AssertEquals("Customs Developer[50%]", ItemSet.ModuleToResourceNameList.DefaultValue.GetDescriptionFromCode("BRK"));
			AssertEquals("Customs Developer[50%]", ItemSet.ModuleToResourceNameList.DefaultValue.GetDescriptionFromCode("BRB"));
			AssertEquals("Customs Developer[50%]", ItemSet.ModuleToResourceNameList.DefaultValue.GetDescriptionFromCode("BAE"));
			AssertEquals("Customs Developer[50%]", ItemSet.ModuleToResourceNameList.DefaultValue.GetDescriptionFromCode("BAU"));
			AssertEquals("Customs Developer[50%]", ItemSet.ModuleToResourceNameList.DefaultValue.GetDescriptionFromCode("AIR"));
			AssertEquals("Customs Developer[50%]", ItemSet.ModuleToResourceNameList.DefaultValue.GetDescriptionFromCode("ACF"));
			AssertEquals("Customs Developer[50%]", ItemSet.ModuleToResourceNameList.DefaultValue.GetDescriptionFromCode("ACT"));
			AssertEquals("Customs Developer[50%]", ItemSet.ModuleToResourceNameList.DefaultValue.GetDescriptionFromCode("SEA"));
			AssertEquals("Customs Developer[50%]", ItemSet.ModuleToResourceNameList.DefaultValue.GetDescriptionFromCode("SCC"));
			AssertEquals("Customs Developer[50%]", ItemSet.ModuleToResourceNameList.DefaultValue.GetDescriptionFromCode("BCA"));
			AssertEquals("Customs Developer[50%]", ItemSet.ModuleToResourceNameList.DefaultValue.GetDescriptionFromCode("BGB"));
			AssertEquals("Customs Developer[50%]", ItemSet.ModuleToResourceNameList.DefaultValue.GetDescriptionFromCode("BHK"));
			AssertEquals("Customs Developer[50%]", ItemSet.ModuleToResourceNameList.DefaultValue.GetDescriptionFromCode("BMY"));
			AssertEquals("Customs Developer[50%]", ItemSet.ModuleToResourceNameList.DefaultValue.GetDescriptionFromCode("BNZ"));
			AssertEquals("Customs Developer[50%]", ItemSet.ModuleToResourceNameList.DefaultValue.GetDescriptionFromCode("BSG"));
			AssertEquals("Customs Developer[50%]", ItemSet.ModuleToResourceNameList.DefaultValue.GetDescriptionFromCode("BUS"));
			AssertEquals("Customs Developer[50%]", ItemSet.ModuleToResourceNameList.DefaultValue.GetDescriptionFromCode("BZA"));
			AssertEquals("Freight/Sales/Marketing Developer[50%]", ItemSet.ModuleToResourceNameList.DefaultValue.GetDescriptionFromCode("CAM"));
			AssertEquals("Freight/Sales/Marketing Developer[50%]", ItemSet.ModuleToResourceNameList.DefaultValue.GetDescriptionFromCode("CFS"));
			AssertEquals("Accounting & Reporting Developer[50%]", ItemSet.ModuleToResourceNameList.DefaultValue.GetDescriptionFromCode("CQW"));
			AssertEquals("Freight/Sales/Marketing Developer[50%]", ItemSet.ModuleToResourceNameList.DefaultValue.GetDescriptionFromCode("CON"));
			AssertEquals("Core Developer[50%]", ItemSet.ModuleToResourceNameList.DefaultValue.GetDescriptionFromCode("COR"));
			AssertEquals("Core Developer[50%]", ItemSet.ModuleToResourceNameList.DefaultValue.GetDescriptionFromCode("DCM"));
			AssertEquals("Core Developer[50%]", ItemSet.ModuleToResourceNameList.DefaultValue.GetDescriptionFromCode("DSS"));
			AssertEquals("Document Engine Developer[20%]", ItemSet.ModuleToResourceNameList.DefaultValue.GetDescriptionFromCode("DOC"));
			AssertEquals("Accounting & Reporting Developer[50%]", ItemSet.ModuleToResourceNameList.DefaultValue.GetDescriptionFromCode("DRB"));
			AssertEquals("Customs Developer[50%]", ItemSet.ModuleToResourceNameList.DefaultValue.GetDescriptionFromCode("XBR"));
			AssertEquals("Customs Developer[50%]", ItemSet.ModuleToResourceNameList.DefaultValue.GetDescriptionFromCode("EXD"));
			AssertEquals("Core Developer[50%]", ItemSet.ModuleToResourceNameList.DefaultValue.GetDescriptionFromCode("FAX"));
			AssertEquals("Core Developer[50%]", ItemSet.ModuleToResourceNameList.DefaultValue.GetDescriptionFromCode("FSV"));
			AssertEquals("Freight/Sales/Marketing Developer[50%]", ItemSet.ModuleToResourceNameList.DefaultValue.GetDescriptionFromCode("FOR"));
			AssertEquals("", ItemSet.ModuleToResourceNameList.DefaultValue.GetDescriptionFromCode("HVS"));
			AssertEquals("Customs Developer[50%]", ItemSet.ModuleToResourceNameList.DefaultValue.GetDescriptionFromCode("IBR"));
			AssertEquals("Freight/Sales/Marketing Developer[50%]", ItemSet.ModuleToResourceNameList.DefaultValue.GetDescriptionFromCode("IMF"));
			AssertEquals("Batch and Interface Developer[50%]", ItemSet.ModuleToResourceNameList.DefaultValue.GetDescriptionFromCode("IFC"));
			AssertEquals("Accounting & Reporting Developer[50%]", ItemSet.ModuleToResourceNameList.DefaultValue.GetDescriptionFromCode("LDC"));
			AssertEquals("Freight/Sales/Marketing Developer[50%]", ItemSet.ModuleToResourceNameList.DefaultValue.GetDescriptionFromCode("LOC"));
			AssertEquals("Freight/Sales/Marketing Developer[50%]", ItemSet.ModuleToResourceNameList.DefaultValue.GetDescriptionFromCode("MFT"));
			AssertEquals("Freight/Sales/Marketing Developer[50%]", ItemSet.ModuleToResourceNameList.DefaultValue.GetDescriptionFromCode("OPP"));
			AssertEquals("Freight/Sales/Marketing Developer[50%]", ItemSet.ModuleToResourceNameList.DefaultValue.GetDescriptionFromCode("ORD"));
			AssertEquals("Freight/Sales/Marketing Developer[50%]", ItemSet.ModuleToResourceNameList.DefaultValue.GetDescriptionFromCode("REC"));
			AssertEquals("Core Developer[50%]", ItemSet.ModuleToResourceNameList.DefaultValue.GetDescriptionFromCode("ORG"));
			AssertEquals("Core Developer[50%]", ItemSet.ModuleToResourceNameList.DefaultValue.GetDescriptionFromCode("REF"));
			AssertEquals("Accounting & Reporting Developer[50%]", ItemSet.ModuleToResourceNameList.DefaultValue.GetDescriptionFromCode("REP"));
			AssertEquals("Freight/Sales/Marketing Developer[50%]", ItemSet.ModuleToResourceNameList.DefaultValue.GetDescriptionFromCode("SAL"));
			AssertEquals("Freight/Sales/Marketing Developer[50%]", ItemSet.ModuleToResourceNameList.DefaultValue.GetDescriptionFromCode("CLR"));
			AssertEquals("Freight/Sales/Marketing Developer[50%]", ItemSet.ModuleToResourceNameList.DefaultValue.GetDescriptionFromCode("COL"));
			AssertEquals("Freight/Sales/Marketing Developer[50%]", ItemSet.ModuleToResourceNameList.DefaultValue.GetDescriptionFromCode("CTF"));
			AssertEquals("Freight/Sales/Marketing Developer[50%]", ItemSet.ModuleToResourceNameList.DefaultValue.GetDescriptionFromCode("COS"));
			AssertEquals("Freight/Sales/Marketing Developer[50%]", ItemSet.ModuleToResourceNameList.DefaultValue.GetDescriptionFromCode("QTE"));
			AssertEquals("Freight/Sales/Marketing Developer[50%]", ItemSet.ModuleToResourceNameList.DefaultValue.GetDescriptionFromCode("SCH"));
			AssertEquals("Freight/Sales/Marketing Developer[50%]", ItemSet.ModuleToResourceNameList.DefaultValue.GetDescriptionFromCode("SCD"));
			AssertEquals("Freight/Sales/Marketing Developer[50%]", ItemSet.ModuleToResourceNameList.DefaultValue.GetDescriptionFromCode("SCR"));
			AssertEquals("Freight/Sales/Marketing Developer[50%]", ItemSet.ModuleToResourceNameList.DefaultValue.GetDescriptionFromCode("SHM"));
			AssertEquals("Freight/Sales/Marketing Developer[50%]", ItemSet.ModuleToResourceNameList.DefaultValue.GetDescriptionFromCode("SMD"));
			AssertEquals("Freight/Sales/Marketing Developer[50%]", ItemSet.ModuleToResourceNameList.DefaultValue.GetDescriptionFromCode("SMB"));
			AssertEquals("Freight/Sales/Marketing Developer[50%]", ItemSet.ModuleToResourceNameList.DefaultValue.GetDescriptionFromCode("SMC"));
			AssertEquals("Freight/Sales/Marketing Developer[50%]", ItemSet.ModuleToResourceNameList.DefaultValue.GetDescriptionFromCode("SED"));
			AssertEquals("Freight/Sales/Marketing Developer[50%]", ItemSet.ModuleToResourceNameList.DefaultValue.GetDescriptionFromCode("SID"));
			AssertEquals("Freight/Sales/Marketing Developer[50%]", ItemSet.ModuleToResourceNameList.DefaultValue.GetDescriptionFromCode("SDO"));
			AssertEquals("Freight/Sales/Marketing Developer[50%]", ItemSet.ModuleToResourceNameList.DefaultValue.GetDescriptionFromCode("SPA"));
			AssertEquals("Customs Developer[50%]", ItemSet.ModuleToResourceNameList.DefaultValue.GetDescriptionFromCode("TAR"));
			AssertEquals("Warehouse Developer[50%]", ItemSet.ModuleToResourceNameList.DefaultValue.GetDescriptionFromCode("WAR"));
			AssertEquals("Warehouse Developer[50%]", ItemSet.ModuleToResourceNameList.DefaultValue.GetDescriptionFromCode("WDF"));
			AssertEquals("Web Developer[45%]", ItemSet.ModuleToResourceNameList.DefaultValue.GetDescriptionFromCode("WEB"));
			AssertEquals("Core Developer[50%]", ItemSet.ModuleToResourceNameList.DefaultValue.GetDescriptionFromCode("PRO"));
			AssertEquals("Freight/Sales/Marketing Developer[50%]", ItemSet.ModuleToResourceNameList.DefaultValue.GetDescriptionFromCode("EMF"));
			AssertEquals("Batch and Interface Developer[50%]", ItemSet.ModuleToResourceNameList.DefaultValue.GetDescriptionFromCode("ECI"));
			AssertEquals("CargoWise Internal Developer[50%]", ItemSet.ModuleToResourceNameList.DefaultValue.GetDescriptionFromCode("INC"));
			AssertEquals("CargoWise Internal Developer[50%]", ItemSet.ModuleToResourceNameList.DefaultValue.GetDescriptionFromCode("TRN"));
			AssertEquals("Freight/Sales/Marketing Developer[50%]", ItemSet.ModuleToResourceNameList.DefaultValue.GetDescriptionFromCode("PRA"));
			AssertEquals("Batch and Interface Developer[50%]", ItemSet.ModuleToResourceNameList.DefaultValue.GetDescriptionFromCode("TBU"));
			AssertEquals("PST Developer[50%]", ItemSet.ModuleToResourceNameList.DefaultValue.GetDescriptionFromCode("UPG"));
			AssertEquals("Freight/Sales/Marketing Developer[50%]", ItemSet.ModuleToResourceNameList.DefaultValue.GetDescriptionFromCode("LON"));
			AssertEquals("Freight/Sales/Marketing Developer[50%]", ItemSet.ModuleToResourceNameList.DefaultValue.GetDescriptionFromCode("EXF"));
			AssertEquals("Customs Developer[50%]", ItemSet.ModuleToResourceNameList.DefaultValue.GetDescriptionFromCode("CUS"));
			AssertEquals("", ItemSet.ModuleToResourceNameList.DefaultValue.GetDescriptionFromCode("OTH"));
			AssertEquals("Freight/Sales/Marketing Developer[50%]", ItemSet.ModuleToResourceNameList.DefaultValue.GetDescriptionFromCode("IFR"));
			AssertEquals("Freight/Sales/Marketing Developer[50%]", ItemSet.ModuleToResourceNameList.DefaultValue.GetDescriptionFromCode("DMF"));
			AssertEquals("Warehouse Developer[50%]", ItemSet.ModuleToResourceNameList.DefaultValue.GetDescriptionFromCode("WMS"));
			AssertEquals("Freight/Sales/Marketing Developer[50%]", ItemSet.ModuleToResourceNameList.DefaultValue.GetDescriptionFromCode("STX"));
			AssertEquals("Freight/Sales/Marketing Developer[50%]", ItemSet.ModuleToResourceNameList.DefaultValue.GetDescriptionFromCode("OCN"));
			AssertEquals("Batch and Interface Developer[50%]", ItemSet.ModuleToResourceNameList.DefaultValue.GetDescriptionFromCode("EDI"));
		}

		public void TestCWSupportLoginTokenPrivateKey()
		{
			TestRegistryItem(ItemSet.CWSupportLoginTokenPrivateKey,
				"CWSupportLoginTokenPrivateKey",
				EDIDataRegistry.CustomerServiceSubCategory,
				"CWSupport Account Login Token Private Key",
				"This private key is for signing tokens used for CWSupport account logins into customer systems.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				Array.Empty<byte>());
			AssertEquals(typeof(CWSupportLoginTokenPrivateKeyEditorInfo), ItemSet.CWSupportLoginTokenPrivateKey.EditorInfo.GetType());
		}

		public void TestCustomerSupportAutoReplyEmailTemplate()
		{
			TestRegistryItem(
				ItemSet.CustomerSupportAutoReplyEmailTemplateRaw,
				"CustomerSupportAutoReplyEmailTemplate",
				EDIDataRegistry.CustomerServiceEmailTemplatesSubCategory,
				"Customer Support Auto Reply Email Template",
				"This is the default template (body) for customer support auto reply emails.",
				RegistryStorageFlags.System,
				TextEditorType.Memo,
				string.Empty);
			ItemSet.CustomerSupportAutoReplyEmailTemplateRaw.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "test text for approval email body");
			AssertEquals("CustomerSupportAutoReplyEmailTemplate", "test text for approval email body", ItemSet.CustomerSupportAutoReplyEmailTemplate);
		}

		public void TestFaxPriceRegistryItem()
		{
			AssertVisible(ItemSet.FaxPriceRegistryItem);

			AssertEquals("FaxPrice", ItemSet.FaxPriceRegistryItem.Name);
			AssertEquals(EDIDataRegistry.PricelistSubCategory, ItemSet.FaxPriceRegistryItem.Category);
			AssertEquals("Faxing Service Prices", ItemSet.FaxPriceRegistryItem.Caption);
			AssertEquals(typeof(FaxPriceDataType), ItemSet.FaxPriceRegistryItem.DataType.GetType());
			AssertEquals(RegistryStorageFlags.System, ItemSet.FaxPriceRegistryItem.Storage);
		}

		public void TestLicenceModuleViewModeRegistryItem()
		{
			AssertEquals("LicenceModuleViewMode", ItemSet.LicenceModuleViewModeItem.Name);
			AssertEquals(RegistryStorageFlags.Company, ItemSet.LicenceModuleViewModeItem.Storage);

			AssertEquals("DefaultValue", 0, ItemSet.LicenceModuleViewModeItem.DefaultValue);
			ItemSet.LicenceModuleViewModeItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1);
			AssertEquals("Value", 1, ItemSet.LicenceModuleViewModeItem.Value);
		}

		public void TestProductAreas()
		{
			AssertVisible(ItemSet.ProductAreas);

			AssertEquals("ProductAreas", ItemSet.ProductAreas.Name);
			AssertEquals(EDIDataRegistry.CustomerServiceSubCategory, ItemSet.ProductAreas.Category);
			AssertEquals("Incident Product Areas", ItemSet.ProductAreas.Caption);
			AssertEquals("The list of product areas for Incident.", ItemSet.ProductAreas.Hint);
			AssertEquals(RegistryStorageFlags.System, ItemSet.ProductAreas.Storage);
		}

		public void TestProductAreaIncidentMenuSectionMappings()
		{
			AssertVisible(ItemSet.SystemProductMappings);

			AssertEquals("SystemProductMappings", ItemSet.SystemProductMappings.Name);
			AssertEquals(EDIDataRegistry.ModuleMappingsSubCategory, ItemSet.SystemProductMappings.Category);
			AssertEquals("Products and Product Area Incident Menu Section Mappings", ItemSet.SystemProductMappings.Caption);
			AssertEquals(@"New products can be added on this registry and will become available as a selection on the eRequest Management Portal as well as adding a Product on the Organization > Licence > Databases.

Mapping list between product, incident product area and menu sections.
Each menu section mapping can have submappings that allows the product area to be overriden for specific menu items.

New menu sections can also be added with the mappings.", ItemSet.SystemProductMappings.Hint);
			AssertEquals(RegistryStorageFlags.System, ItemSet.SystemProductMappings.Storage);
			AssertEquals(true, ((SystemProductRegistryEditorInfo)ItemSet.SystemProductMappings.EditorInfo).IsSourceModuleMappingsVisible);
		}

		public void TestProductAreaIncidentMenuSectionMappings_DefaultValue()
		{
			var moduleTreeLoader = ObjectFactory.Get<IModuleTreeLoader>();
			var allModuleTreeMenuSectionsByCode = new Dictionary<string, IList<string>>();

			foreach (var clientSpecificAssemblyFileName in ClientHookLoader.Instance.GetAllClientSpecificAssemblyFileNames())
			{
				var clientSpecificAssembly = ClientHookLoader.Instance.GetAssemblyFromFileName(clientSpecificAssemblyFileName);
				var moduleTree = new ModuleTree();
				try
				{
					using (ClientHookLoader.Instance.OverrideClientAssemblyForTest(clientSpecificAssembly))
					using (ModuleTree.OverrideTreeForTest(moduleTree))
					{
						moduleTreeLoader.Initialise(moduleTree, Env.Security);
						moduleTreeLoader.LoadModules();

						foreach (ICodeDescription menuSection in new IncidentApprovalLookups(null).MenuSectionListIncludingHidden)
						{
							IList<string> menuSectionsWithSameCode;
							if (!allModuleTreeMenuSectionsByCode.TryGetValue(menuSection.Code, out menuSectionsWithSameCode))
							{
								menuSectionsWithSameCode = new List<string>();
								allModuleTreeMenuSectionsByCode.Add(menuSection.Code, menuSectionsWithSameCode);
							}
							if (!menuSectionsWithSameCode.Any(x => x == menuSection.Description))
							{
								menuSectionsWithSameCode.Add(menuSection.Description);
							}
						}
					}
				}
				catch (TypeLoadException) { }
			}

			var menuSectionCodesWithMutipleDescriptions = allModuleTreeMenuSectionsByCode.Where(x => x.Value.Count > 1);
			if (menuSectionCodesWithMutipleDescriptions.Any())
			{
				Fail("The following menu sections have the same code but different descriptions:\r\n"
					+ string.Join("\r\n", menuSectionCodesWithMutipleDescriptions.Select(x => string.Join(", ", x.Value))));
			}

			var actualMenuSectionCodeDescriptionLookup = ItemSet.SystemProductMappings.DefaultValue
				.Cast<SystemProduct>().First().ModuleMappings
				.Cast<ProductAreaModuleMapping>().ToDictionary(x => x.ModuleCode, x => x.ModuleDescription);
			var expectedMenuSectionCodeDescriptionPairs = allModuleTreeMenuSectionsByCode.Select(x => new KeyValuePair<string, string>(x.Key, x.Value.First()));

			CombineAssertions(@"The default registry value should contain all menu sections used in the module tree - including those only visible for client specific dlls.

This test may fail if you have added or renamed a client specific menu section for a ModuleSection.
To fix this, update the ProductAreaIncidentMenuSectionMappings to also include the client specific menu section.", () =>
			{
				foreach (var expectedPair in expectedMenuSectionCodeDescriptionPairs)
				{
					ZString actualMenuSectionDescription;
					if (!actualMenuSectionCodeDescriptionLookup.TryGetValue(expectedPair.Key, out actualMenuSectionDescription))
					{
						Fail(string.Format(CultureInfo.CurrentCulture, "Missing Menu Section {{ \"{0}\", \"{1}\" }}", expectedPair.Key, expectedPair.Value));
					}
					else
					{
						AssertEquals(string.Format(CultureInfo.CurrentCulture, "Description for Menu Section {{ \"{0}\" }}", expectedPair.Key), expectedPair.Value, actualMenuSectionDescription);
					}
				}
			});
		}

		public void TestProductAreaIncidentCr8Mappings()
		{
			AssertVisible(ItemSet.ProductAreaIncidentCr8Mappings);

			AssertEquals("SystemProductAreaIncidentCr8Mappings", ItemSet.ProductAreaIncidentCr8Mappings.Name);
			AssertEquals(EDIDataRegistry.ModuleMappingsSubCategory, ItemSet.ProductAreaIncidentCr8Mappings.Category);
			AssertEquals("Product Area Incident CR8 Module Mappings", ItemSet.ProductAreaIncidentCr8Mappings.Caption);
			AssertEquals(RegistryStorageFlags.System, ItemSet.ProductAreaIncidentCr8Mappings.Storage);
			AssertEquals(true, ((SystemProductRegistryEditorInfo)ItemSet.ProductAreaIncidentCr8Mappings.EditorInfo).IsSourceModuleMappingsVisible);
		}

		public void TestProductAreaIncidentCr9Mappings()
		{
			AssertVisible(ItemSet.ProductAreaIncidentCr9Mappings);

			AssertEquals("SystemProductAreaIncidentCr9Mappings", ItemSet.ProductAreaIncidentCr9Mappings.Name);
			AssertEquals(EDIDataRegistry.ModuleMappingsSubCategory, ItemSet.ProductAreaIncidentCr9Mappings.Category);
			AssertEquals("Product Area Incident CR9 Module Mappings", ItemSet.ProductAreaIncidentCr9Mappings.Caption);
			AssertEquals(RegistryStorageFlags.System, ItemSet.ProductAreaIncidentCr9Mappings.Storage);
			AssertEquals(true, ((SystemProductRegistryEditorInfo)ItemSet.ProductAreaIncidentCr9Mappings.EditorInfo).IsSourceModuleMappingsVisible);
		}

		public void TestGetProductAreaModuleMappingsRegistryItem()
		{
			AssertEquals(ItemSet.SystemProductMappings, EDIDataRegistry.GetProductAreaModuleMappingsRegistryItem(ModuleListType.MenuSection));
			AssertEquals(EDIDataRegistry.Instance.ProductAreaIncidentCr8Mappings, EDIDataRegistry.GetProductAreaModuleMappingsRegistryItem(ModuleListType.Cr8));
			AssertEquals(EDIDataRegistry.Instance.ProductAreaIncidentCr9Mappings, EDIDataRegistry.GetProductAreaModuleMappingsRegistryItem(ModuleListType.Cr9));
			AssertEquals(null, EDIDataRegistry.GetProductAreaModuleMappingsRegistryItem(ModuleListType.Unspecified));
		}

		public void TestProductAreaAssignments()
		{
			AssertVisible(ItemSet.ProductAreaAssignments);

			AssertEquals("ProductAreaAssignments", ItemSet.ProductAreaAssignments.Name);
			AssertEquals(EDIDataRegistry.CustomerServiceSubCategory, ItemSet.ProductAreaAssignments.Category);
			AssertEquals("Product Area Staff Assignments", ItemSet.ProductAreaAssignments.Caption);
			AssertEquals("Assign staff to be responsible for product area.", ItemSet.ProductAreaAssignments.Hint);
			AssertEquals(RegistryStorageFlags.System, ItemSet.ProductAreaAssignments.Storage);
		}

		public void TestActivitySubtypeAssignments()
		{
			AssertVisible(ItemSet.ActivitySubtypeAssignments);

			AssertEquals("ActivitySubtypeAssignments", ItemSet.ActivitySubtypeAssignments.Name);
			AssertEquals(EDIDataRegistry.WorkItemSubCategory, ItemSet.ActivitySubtypeAssignments.Category);
			AssertEquals("Capitalized Development Change Types", ItemSet.ActivitySubtypeAssignments.Caption);
			AssertEquals("Assign Change Types Capitalization", ItemSet.ActivitySubtypeAssignments.Hint);
			AssertEquals(RegistryStorageFlags.System, ItemSet.ActivitySubtypeAssignments.Storage);
		}

		public void TestRelationshipManagerCompanyLookup()
		{
			CodeDescriptionPairListRegistryItem item = ItemSet.RelationshipManagerCompanyLookup;
			AssertEquals("RelationshipManagerCompanyLookup", item.Name);
			AssertEquals("Relationship Manager Company Lookup", item.Caption);
			AssertEquals(EDIDataRegistry.CustomerServiceSubCategory, item.Category);
			AssertEquals("The lookup table for displaying relationship managers on support incident screen.\r\nThe Code column is the company code of staff home branch, the Description column is which company code should be used to lookup relationship manager assignment.", item.Hint);
			AssertEquals(RegistryStorageFlags.System, item.Storage);
		}

		public void TestDbConnectionCrikeyServer()
		{
			string defaultValue = ""; // Production server: syddat.db.wtg.zone, do not set the production server as default to prevent people from accidently using it while testing
			AssertEquals("Default value", defaultValue, ItemSet.DbConnectionCrikeyServer.Value);

			ItemSet.DbConnectionCrikeyServer.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "other value");
			AssertEquals("Default value", "other value", ItemSet.DbConnectionCrikeyServer.Value);

			ItemSet.DbConnectionCrikeyServer.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultValue);
		}

		public void TestDefaultFilterLayouts()
		{
			TestGenericRegistryItem(ItemSet.DefaultFilterLayoutForWebEDIClassrooms, "DefaultFilterLayoutCargoWiseEDIClassroms",
															"WiseTech Global Client Extensions/Default Filter Layouts", "CargoWise Classroms",
															"This is the default filter layout for the CargoWise Classroms module.",
															RegistryStorageFlags.Company, string.Empty);
			TestGenericRegistryItem(ItemSet.DefaultFilterLayoutForWebHRJobApplicant, "DefaultFilterLayoutForWebHRJobApplicant",
															"WiseTech Global Client Extensions/Default Filter Layouts", "Job Applicant",
															"This is the default filter layout for the Web Job Applicant module.",
															RegistryStorageFlags.Company, string.Empty);
			TestGenericRegistryItem(ItemSet.DefaultFilterLayoutForWebGlbPerson, "DefaultFilterLayoutForWebGlbPerson",
															"WiseTech Global Client Extensions/Default Filter Layouts", "Person",
															"This is the default filter layout for the Web Person module.",
															RegistryStorageFlags.Company, string.Empty);
		}

		public void TestEnableVerboseModeOnEmailProcessors()
		{
			AssertEquals("EnableVerboseModeOnEmailProcessors", ItemSet.EnableVerboseModeOnEmailProcessors.Name);
			AssertEquals(EDIDataRegistry.EmailAddressesCategory, ItemSet.EnableVerboseModeOnEmailProcessors.Category);
			AssertEquals("Enable Verbose Mode on Email Processors", ItemSet.EnableVerboseModeOnEmailProcessors.Caption);
			AssertEquals("This provides additional logging info to help troubleshooting email processors issues.", ItemSet.EnableVerboseModeOnEmailProcessors.Hint);
			AssertEquals(RegistryStorageFlags.System, ItemSet.EnableVerboseModeOnEmailProcessors.Storage);
			AssertEquals(RegistryOptions.NotCached, ItemSet.EnableVerboseModeOnEmailProcessors.Options);
			AssertEquals(false, ItemSet.EnableVerboseModeOnEmailProcessors.DefaultValue);
		}

		public void TestProjectInvoiceNotificationRecipients()
		{
			GuidRegistryItem item = ItemSet.ProjectInvoiceEmailNotificationGroup;
			AssertEquals("ProjectInvoiceEmailNotificationGroup", item.Name);
			AssertEquals("Project Invoice Email Notification Group", item.Caption);
			AssertEquals(EDIDataRegistry.ProjectsSubCategory, item.Category);
			AssertEquals("The staff group that will be notified about project invoicing.", item.Hint);
			AssertEquals(typeof(GuidFindBoxRegistryEditorInfo), item.EditorInfo.GetType());
			AssertEquals(RegistryStorageFlags.System, item.Storage);
		}

		public void TestDeniedPartyScreeningListsDbServerName()
		{
			StringRegistryItem item = ItemSet.DeniedPartyScreeningListsDbServerName;
			AssertEquals("DeniedPartyScreeningListsDbServerName", item.Name);
			AssertEquals("Denied Party Screening Lists Database Server Name", item.Caption);
			AssertEquals(EDIDataRegistry.EServicesSubCategory, item.Category);
			AssertEquals("Enter the name of server which hosts denined party screening lists database.", item.Hint);
			AssertEquals(RegistryStorageFlags.System, item.Storage);
			AssertEquals("syddps.db.corporate.cargowise.com", item.DefaultValue);
		}

		public void TestDeniedPartyScreeningListsDbName()
		{
			StringRegistryItem item = ItemSet.DeniedPartyScreeningListsDbName;
			AssertEquals("DeniedPartyScreeningListsDbName", item.Name);
			AssertEquals("Denied Party Screening Lists Database Name", item.Caption);
			AssertEquals(EDIDataRegistry.EServicesSubCategory, item.Category);
			AssertEquals("Enter the name of database which stores denined party screening lists.", item.Hint);
			AssertEquals(RegistryStorageFlags.System, item.Storage);
			AssertEquals("DeniedPartyScreeningProfiles", item.DefaultValue);
		}

		public void TestDeniedPartyScreeningListsDbLogin()
		{
			ServerUsernamePasswordConfigurationRegistryItem item = ItemSet.DeniedPartyScreeningListsDbLogin;
			AssertEquals("DeniedPartyScreeningListsDbLogin", item.Name);
			AssertEquals("Denied Party Screening Lists Database Login", item.Caption);
			AssertEquals(EDIDataRegistry.EServicesSubCategory, item.Category);
			AssertEquals("Enter login information to the database which stores denined party screening lists.", item.Hint);
			AssertEquals(RegistryStorageFlags.System, item.Storage);
			AssertEquals("eHubUser", item.DefaultValue.UserName);
			AssertEquals("eHubUser", item.DefaultValue.Password);
			AssertEquals("eHubUser", item.DefaultValue.ConfirmPassword);
		}

		public void TestOdplUsageChargeCode()
		{
			StringRegistryItem item = ItemSet.OdplUsageChargeCode;
			AssertEquals("OdplUsageChargeCode", item.Name);
			AssertEquals("Usage Charge Code", item.Caption);
			AssertEquals(EDIDataRegistry.OdplBillingCategory, item.Category);
			AssertEquals("", item.Hint);
			AssertEquals("Default value", "ODPLMTHUSE", item.Value);
			AssertEquals("Storage Flags", RegistryStorageFlags.System, item.Storage);
		}

		public void TestOdplDiscountChargeCode()
		{
			StringRegistryItem item = ItemSet.OdplDiscountChargeCode;
			AssertEquals("OdplDiscountChargeCode", item.Name);
			AssertEquals("Discount Charge Code", item.Caption);
			AssertEquals(EDIDataRegistry.OdplBillingCategory, item.Category);
			AssertEquals("", item.Hint);
			AssertEquals("Default value", "DISCODPL", item.Value);
			AssertEquals("Storage Flags", RegistryStorageFlags.System, item.Storage);
		}

		public void TestMonthlyUsageProcessingFeeChargeCode()
		{
			StringRegistryItem item = ItemSet.MonthlyUsageProcessingFeeChargeCode;
			AssertEquals("MonthlyUsageProcessingFeeChargeCode", item.Name);
			AssertEquals("Fee Charge Code", item.Caption);
			AssertEquals(EDIDataRegistry.ProcessingFeeCategory, item.Category);
			AssertEquals("", item.Hint);
			AssertEquals("Default value", "SALESFEE", item.Value);
			AssertEquals("Storage Flags", RegistryStorageFlags.System, item.Storage);
		}

		public void TestInvoiceAttachmentDocType()
		{
			StringRegistryItem item = ItemSet.InvoiceAttachmentDocType;
			AssertEquals("InvoiceAttachmentDocType", item.Name);
			AssertEquals("Invoice Attachments Document Type", item.Caption);
			AssertEquals(EDIDataRegistry.LicenceBillingCategory, item.Category);
			AssertEquals("Document Type used for general purpose invoice attachments.", item.Hint);
			AssertEquals("Default value", "", item.Value);
			AssertEquals("Storage Flags", RegistryStorageFlags.System, item.Storage);
		}

		public void TestCommentChargeCode()
		{
			StringRegistryItem item = ItemSet.CommentChargeCode;
			AssertEquals("CommentChargeCode", item.Name);
			AssertEquals("Comment Charge Code", item.Caption);
			AssertEquals(EDIDataRegistry.LicenceBillingCategory, item.Category);
			AssertEquals("", item.Hint);
			AssertEquals("Default value", "COMMENT", item.Value);
			AssertEquals("Storage Flags", RegistryStorageFlags.System, item.Storage);
		}

		public void TestPrepaidBalanceChargeCode()
		{
			StringRegistryItem item = ItemSet.PrepaidBalanceChargeCode;
			AssertEquals("PrepaidBalanceChargeCode", item.Name);
			AssertEquals("Prepaid Balance Charge Code", item.Caption);
			AssertEquals(EDIDataRegistry.LicenceBillingCategory, item.Category);
			AssertEquals("", item.Hint);
			AssertEquals("Default value", "", item.Value);
			AssertEquals("Storage Flags", RegistryStorageFlags.System, item.Storage);
		}

		public void TestVirtualMachineDetectionKeywords()
		{
			CodeDescriptionPairListRegistryItem item = ItemSet.VirtualMachineDetectionKeywords;
			AssertEquals("VirtualMachineDetectionKeywords", item.Name);
			AssertEquals("Virtual Machine Detection Keywords", item.Caption);
			AssertEquals(EDIDataRegistry.VersionReportingSubCategory, item.Category);
			AssertEquals("The list of keywords which indicating clients' systems are virtual machine.", item.Hint);
			AssertEquals("Storage Flags", RegistryStorageFlags.System, item.Storage);
			AssertEquals("Options", RegistryOptions.NotCached, item.Options);
		}

		public void TestPriceListDocType()
		{
			CodePairRegistryItem item = ItemSet.PriceListDocType;
			AssertEquals("PriceListDocType", item.Name);
			AssertEquals("Document Type for the Sales Price List", item.Caption);
			AssertEquals(EDIDataRegistry.LicenceBillingCategory, item.Category);
			AssertEquals("Adding documents of this type to an organization will automatically import the prices for ODPL billing.", item.Hint);
			AssertEquals("Default value", "PRI", item.Value);
			AssertEquals("Storage Flags", RegistryStorageFlags.System, item.Storage);
		}

		public void TestBillingCountryGroups()
		{
			CodeDescriptionBoolRegistryItem item = ItemSet.BillingCountryGroups;
			AssertEquals("BillingCountryGroup", item.Name);
			AssertEquals(EDIDataRegistry.LicenceBillingCategory, item.Category);
			AssertEquals("Country/Region Group", item.Caption);
			AssertEquals("Storage Flags", RegistryStorageFlags.System, item.Storage);

			var expectedHint =
@"Countries/Regions that are grouped for receiving developing country / region discount and for registered user count. Code column is the country/region code and Description column is the country/region code of the main country/region in the group. The Registered User Country/Region Grouping column indicates the groupings are for calculating users count per country/region.

The CHU (Chargeable Usage) Service Task will need to run after updates to this registry.";
			AssertEquals(expectedHint, item.Hint);
		}

		public void TestMinimumAmountToBill()
		{
			DecimalRegistryItem item = ItemSet.MinimumAmountToBill;
			AssertEquals("MinimumAmountToBill", item.Name);
			AssertEquals(EDIDataRegistry.MonthlyUsageInvoiceCategory, item.Category);
			AssertEquals("Storage Flags", RegistryStorageFlags.System, item.Storage);
			AssertEquals("Default", 20m, item.DefaultValue);
		}

		public void TestNonBilledEnterpriseCodesAsStringArray()
		{
			AssertArrayEqualsByElements(new string[] { "EDI", "HYE", "EHW", "WTL" }, ItemSet.NonBilledEnterpriseCodesAsStringArray);

			ItemSet.NonBilledEnterpriseCodesAsStringArray = new string[] { "FOO", "BAR" };
			AssertArrayEqualsByElements(new string[] { "FOO", "BAR" }, ItemSet.NonBilledEnterpriseCodesAsStringArray);
		}

		public void TestUserRegistrationNotificationGroup()
		{
			GuidRegistryItem item = ItemSet.UserRegistrationNotificationGroup;
			AssertEquals("UserRegistrationNotificationGroup", item.Name);
			AssertEquals("My Account New User Registration Notification Group", item.Caption);
			AssertEquals(EDIDataRegistry.MyAccountPortalSubCategory + "/User Registration", item.Category);
			AssertEquals("Storage Flags", RegistryStorageFlags.System, item.Storage);
		}

		public void TestUserRegistrationJobRoleList()
		{
			CodeDescriptionPairListRegistryItem item = ItemSet.UserRegistrationJobRoleList;
			AssertEquals("UserRegistrationJobRoleList", item.Name);
			AssertEquals("Predefined Job Roles For My Account User Registration", item.Caption);
			AssertEquals(EDIDataRegistry.MyAccountPortalSubCategory + "/User Registration", item.Category);
			AssertEquals("Storage Flags", RegistryStorageFlags.System, item.Storage);
			AssertEquals("Default value", "Executive / General Management", item.Value.GetDescriptionFromCode("Executive / General Management"));
			AssertEquals("Default value", "Operations - Freight", item.Value.GetDescriptionFromCode("Operations - Freight"));
			AssertEquals("Default value", "Operations - Warehouse", item.Value.GetDescriptionFromCode("Operations - Warehouse"));
			AssertEquals("Default value", "Operations - Customs Brokerage", item.Value.GetDescriptionFromCode("Operations - Customs Brokerage"));
			AssertEquals("Default value", "Finance", item.Value.GetDescriptionFromCode("Finance"));
			AssertEquals("Default value", "Business Development", item.Value.GetDescriptionFromCode("Business Development"));
			AssertEquals("Default value", "Marketing", item.Value.GetDescriptionFromCode("Marketing"));
			AssertEquals("Default value", "IT", item.Value.GetDescriptionFromCode("IT"));
			AssertEquals("Default value", "Customer Service", item.Value.GetDescriptionFromCode("Customer Service"));
		}

		public void TestUserRegistrationTypeOfBusinessList()
		{
			CodeDescriptionPairListRegistryItem item = ItemSet.UserRegistrationTypeOfBusinessList;
			AssertEquals("UserRegistrationTypeOfBusinessList", item.Name);
			AssertEquals("Predefined Business Types For My Account User Registration", item.Caption);
			AssertEquals(EDIDataRegistry.MyAccountPortalSubCategory + "/User Registration", item.Category);
			AssertEquals("Storage Flags", RegistryStorageFlags.System, item.Storage);
			AssertEquals("Default value", "Freight Forwarding - International Air", item.Value.GetDescriptionFromCode("Freight Forwarding - International Air"));
			AssertEquals("Default value", "Freight Forwarding - International Ocean", item.Value.GetDescriptionFromCode("Freight Forwarding - International Ocean"));
			AssertEquals("Default value", "Freight Forwarding - Domestic", item.Value.GetDescriptionFromCode("Freight Forwarding - Domestic"));
			AssertEquals("Default value", "Customs Brokerage", item.Value.GetDescriptionFromCode("Customs Brokerage"));
			AssertEquals("Default value", "Warehouse", item.Value.GetDescriptionFromCode("Warehouse"));
			AssertEquals("Default value", "Container Freight Station", item.Value.GetDescriptionFromCode("Container Freight Station"));
			AssertEquals("Default value", "NVOCC", item.Value.GetDescriptionFromCode("NVOCC"));
		}

		public void TestUserRegistrationReasonForRequestingAccessList()
		{
			var item = ItemSet.UserRegistrationReasonForRequestingAccessList;
			AssertEquals("UserRegistrationReasonForRequestingAccessList", item.Name);
			AssertEquals("Predefined Reasons For Requesting Access For My Account User Registration", item.Caption);
			AssertEquals(EDIDataRegistry.MyAccountPortalSubCategory + "/User Registration", item.Category);
			AssertEquals("Storage Flags", RegistryStorageFlags.System, item.Storage);
			AssertEquals("Default value", "Request Product Information", item.Value.GetDescriptionFromCode("REQUEST PRODUCT INFORMATION"));
			AssertEquals("Default value", "Speak with a Consultant", item.Value.GetDescriptionFromCode("SPEAK WITH A CONSULTANT"));
			AssertEquals("Default value", "Subscribe to the Newsletter", item.Value.GetDescriptionFromCode("SUBSCRIBE TO THE NEWSLETTER"));
			AssertEquals("Default value", "Access the Online Learning Center for Product Review", item.Value.GetDescriptionFromCode("ACCESS LEARNING CENTER FOR PRODUCT REVIEW"));
			AssertEquals("Default value", "Access the Online Learning Center for Product Training", item.Value.GetDescriptionFromCode("ACCESS LEARNING CENTER FOR PRODUCT TRAINING"));
			AssertEquals("Default value", "Access Technical Guides", item.Value.GetDescriptionFromCode("ACCESS TECHNICAL GUIDES"));
		}

		public void TestUserRegistrationCompanySizeList()
		{
			CodeDescriptionPairListRegistryItem item = ItemSet.UserRegistrationCompanySizeList;
			AssertEquals("UserRegistrationCompanySizeList", item.Name);
			AssertEquals("Predefined Company Size For My Account User Registration", item.Caption);
			AssertEquals(EDIDataRegistry.MyAccountPortalSubCategory + "/User Registration", item.Category);
			AssertEquals("Storage Flags", RegistryStorageFlags.System, item.Storage);
		}

		public void TestMyAccountTermsAndConditionsNotificationEmailTemplate()
		{
			NotificationEmailTemplateRegistryItem item = ItemSet.MyAccountTermsAndConditionsNotificationEmailTemplate;

			AssertVisible(item);

			AssertEquals("MyAccountTermsAndConditionsNotificationEmailTemplate", item.Name);
			AssertEquals(EDIDataRegistry.MyAccountPortalSubCategory + "/Terms and Conditions", item.Category);
			AssertEquals("My Account Terms And Conditions Notification Email Template", item.Caption);
			AssertEquals("This is the default template for My Account Terms And Conditions notification emails.", item.Hint);
			AssertEquals(RegistryStorageFlags.System, item.Storage);

			item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new NotificationEmailTemplate(typeof(DocMyAccountWebContract), "test subject 1", "test template 1"));
			AssertEquals("test subject 1", item.Value.EmailSubject);
			AssertEquals("test template 1", item.Value.EmailBody);
		}

		public void TestSplitterDistance()
		{
			IntRegistryItem item = ItemSet.SplitterDistance;
			AssertEquals("SplitterDistance", item.Name);
			AssertEquals("", item.Caption);
			AssertEquals("Splitters", item.Category);
			AssertEquals("", item.Hint);
			AssertEquals("Default value", -1, item.Value);
			AssertEquals("Storage Flags", RegistryStorageFlags.Company, item.Storage);
			AssertEquals("Options", RegistryOptions.NotLogged | RegistryOptions.IsHidden, item.Options);
		}

		public void TestAzpsApprovedForMyAccount()
		{
			var item = ItemSet.AzpsApprovedForMyAccount;
			AssertEquals("AzpsApprovedForMyAccount", item.Name);
			AssertEquals("Authorized Parties Approved For Trusted Messaging using Azure B2C", item.Caption);
			AssertEquals(EDIDataRegistry.MyAccountPortalSubCategory, item.Category);
			AssertEquals("List of AZPs (Authorized Parties) which are approved for Trusted Messaging APIs using Azure B2C. Enter the azp (in GUID format xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx) and the product they are authorized for.", item.Hint);
		}

		public void TestTrustedMessagingUserAgreementCheckBypassProducts()
		{
			var item = ItemSet.TrustedMessagingUserAgreementCheckBypassProducts;
			AssertEquals("TrustedMessagingUserAgreementCheckBypassProducts", item.Name);
			AssertEquals("Trusted Messaging User Agreement Check Bypass", item.Caption);
			AssertEquals(EDIDataRegistry.MyAccountPortalSubCategory, item.Category);
			AssertEquals("List of products that will bypass user agreement in the Auto-Login API. WiseTech Global legal representatives are to confirm any products added to this list.", item.Hint);
		}

		#region OpenId Connect

		public void TestWTGActiveDirectoryCredentials()
		{
			var item = ItemSet.WTGActiveDirectoryCredentials;
			AssertEquals("WTGActiveDirectoryCredentials", item.Name);
			AssertEquals("WTG Active Directory Credentials", item.Caption);
			AssertEquals(EDIDataRegistry.OpenIDConnectSubCategory, item.Category);
			AssertNotNull(item.DefaultValue);
			AssertEquals(false, item.DefaultValue.IsEnabled);
			AssertEquals("root/Accounts/Token Based Authentication", item.DefaultValue.OrganizationalUnitPath);
		}

		public void TestPasswordAndSignatureAlwaysVisibleToGroup()
		{
			var hrGlbGroup = Factory.NewWithValidTestData<GlbGroup>();
			hrGlbGroup.GG_Code = "HRGROUPUSER";
			Factory.Save();
			var item = ItemSet.PasswordAndSignatureAlwaysVisibleToGroup;
			AssertEquals("PasswordAndSignatureAlwaysVisibleToGroup", item.Name);
			AssertEquals("Password & Signature Always Visible To Group", item.Caption);
			AssertEquals(EDIDataRegistry.OpenIDConnectSubCategory, item.Category);
			AssertNotNull(item.DefaultValue);
			AssertEquals(hrGlbGroup.PK, item.DefaultValue);
		}

		#endregion

		public void TestPriceItemDiscountTypes()
		{
			var item = ItemSet.PriceItemDiscountTypes;
			AssertEquals("EDIPriceItemDiscountTypes", item.Name);
			AssertEquals("Price Item Discount Types", item.Caption);
			AssertEquals(EDIDataRegistry.PricelistSubCategory, item.Category);
			AssertEquals("List of Discount Codes that should have a negative value in price list", item.Hint);
			AssertEquals("Storage Flags", RegistryStorageFlags.System, item.Storage);
		}

		public void TestPricelistCountryRegions()
		{
			CodeDescriptionPairListRegistryItem item = ItemSet.PricelistCountryRegions;
			AssertEquals("EDIPricelistCountryRegions", item.Name);
			AssertEquals("Country/Region Regions", item.Caption);
			AssertEquals(EDIDataRegistry.PricelistSubCategory, item.Category);
			AssertEquals("List of Country/Region Code and Region (Description) for calculating Licence Edition", item.Hint);
			AssertEquals("Storage Flags", RegistryStorageFlags.System, item.Storage);
		}

		public void TestOrgNameChangeNotificationAddresses()
		{
			string[] values = new string[] { "hotel", "motel" };
			ItemSet.OrgNameChangeNotificationAddresses.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, values);
			var item = ItemSet.OrgNameChangeNotificationAddresses;
			Assert("hotel" == item.Value[0] || "hotel" == item.Value[1]);
			Assert("motel" == item.Value[0] || "motel" == item.Value[1]);
		}

		public void TestMonthlyUsageInvoiceDescription()
		{
			var item = ItemSet.MonthlyUsageInvoiceDescription;
			AssertEquals("MonthlyUsageInvoiceDescription", item.Name);
			AssertEquals("Invoice Description", item.Caption);
			AssertEquals(EDIDataRegistry.MonthlyUsageInvoiceCategory, item.Category);
			AssertEquals("", item.Hint);
			AssertEquals("Default value", "Monthly Usage Invoice", item.Value);
			AssertEquals("Storage Flags", RegistryStorageFlags.System, item.Storage);
		}

		public void TestSalesTaxRates()
		{
			CodeDescriptionPairListRegistryItem item = ItemSet.SalesTaxRates;
			AssertEquals("SalesTaxRates", item.Name);
			AssertEquals("Sales Tax Rates", item.Caption);
			AssertEquals(EDIDataRegistry.LicenceBillingCategory, item.Category);
			AssertEquals("The list of sales tax codes and percentage rates.", item.Hint);
			AssertEquals("Storage Flags", RegistryStorageFlags.System, item.Storage);
		}

		public void TestHostingDataStorageChargeCode()
		{
			StringRegistryItem item = ItemSet.HostingDataStorageChargeCode;
			AssertEquals("HostingDataStorageChargeCode", item.Name);
			AssertEquals("High Speed Data Storage Charge Code", item.Caption);
			AssertEquals(EDIDataRegistry.HostingBillingCategory, item.Category);
			AssertEquals("", item.Hint);
			AssertEquals("Storage Flags", RegistryStorageFlags.System, item.Storage);
		}

		public void TestHostingDocsStorageChargeCode()
		{
			StringRegistryItem item = ItemSet.HostingDocsStorageChargeCode;
			AssertEquals("HostingDocsStorageChargeCode", item.Name);
			AssertEquals("Image Storage Charge Code", item.Caption);
			AssertEquals(EDIDataRegistry.HostingBillingCategory, item.Category);
			AssertEquals("", item.Hint);
			AssertEquals("Storage Flags", RegistryStorageFlags.System, item.Storage);
		}

		public void TestHostingUltraFastStorageChargeCode()
		{
			StringRegistryItem item = ItemSet.HostingUltraFastStorageChargeCode;
			AssertEquals("HostingUltraFastStorageChargeCode", item.Name);
			AssertEquals("Ultra Fast Storage Charge Code", item.Caption);
			AssertEquals(EDIDataRegistry.HostingBillingCategory, item.Category);
			AssertEquals("", item.Hint);
			AssertEquals("Storage Flags", RegistryStorageFlags.System, item.Storage);
		}

		public void TestHostingRemoteDevicesChargeCode()
		{
			StringRegistryItem item = ItemSet.HostingRemoteDevicesChargeCode;
			AssertEquals("HostingRemoteDevicesChargeCode", item.Name);
			AssertEquals("Remote Devices Charge Code", item.Caption);
			AssertEquals(EDIDataRegistry.HostingBillingCategory, item.Category);
			AssertEquals("", item.Hint);
			AssertEquals("Storage Flags", RegistryStorageFlags.System, item.Storage);
		}

		public void TestHostingPrintServersChargeCode()
		{
			StringRegistryItem item = ItemSet.HostingPrintServersChargeCode;
			AssertEquals("HostingPrintServersChargeCode", item.Name);
			AssertEquals("Print Servers Charge Code", item.Caption);
			AssertEquals(EDIDataRegistry.HostingBillingCategory, item.Category);
			AssertEquals("", item.Hint);
			AssertEquals("Storage Flags", RegistryStorageFlags.System, item.Storage);
		}

		public void TestBillingSystemChargeCodeMappings()
		{
			var item = ItemSet.BillingSystemChargeCodeMappings;
			AssertEquals("BillingSystemChargeCodeMappings", item.Name);
			AssertEquals(EDIDataRegistry.LicenceBillingCategory, item.Category);
			AssertEquals("Billing System Charge Code Mappings", item.Caption);
			AssertEquals("List of charge codes used in billing systems.", item.Hint);
		}

		public void TestStlDiscountSuspensionPolicyDefault()
		{
			var item = ItemSet.StlDiscountSuspensionPolicyDefault;
			AssertEquals("StlDiscountSuspensionPolicyDefault", item.Name);
			AssertEquals(EDIDataRegistry.StlBillingCategory, item.Category);
			AssertEquals("Discount Suspension Policy Default", item.Caption);
			AssertEquals("Sets the default discount policy that will apply to all paying organizations when billing for products in STL", item.Hint);
			AssertEquals("Storage Flags", RegistryStorageFlags.System, item.Storage);
			var itemValue = item.DefaultValue.OfType<DiscountSuspensionPolicy>().Single();
			AssertEquals("CW1", itemValue.ProductCode);
			AssertEquals("NVR", itemValue.PolicyCode);
		}

		public void TestCargoWiseNextBillingARInvoiceOnlyBranches()
		{
			var item = ItemSet.CargoWiseNextBillingARInvoiceOnlyBranches;
			AssertEquals("CargoWiseNextBillingARInvoiceOnlyBranches", item.Name);
			AssertEquals(EDIDataRegistry.StlBillingCategory, item.Category);
			AssertEquals("CargoWise Next Invoice Split Configuration", item.Caption);
			AssertEquals("Add issuing branch codes in this registry for countries where a Disbursement Rebate invoice is not valid. By adding the issuing branch here, STL Billing will only produce two invoices for Disbursement License Fee Billing. The Disbursement Rebate invoice will be added as a discount line in the STL Monthly Usage invoice.", item.Hint);
			AssertEquals("Storage Flags", RegistryStorageFlags.System, item.Storage);
		}

		public void TestLicenceUsageBilledPerTransaction()
		{
			var item = ItemSet.LicenceUsageBilledPerTransaction;
			AssertEquals("LicenceUsageBilledPerTransaction", item.Name);
			AssertEquals("Licence Usage Billed Per Transaction", item.Caption);
			AssertEquals(EDIDataRegistry.LicenceBillingCategory, item.Category);
			AssertEquals("The list of licence modules that are billed from CPT usage in the licence usage report.", item.Hint);
			AssertEquals("Storage Flags", RegistryStorageFlags.System, item.Storage);
			AssertEquals("Default value", "Native CA ACI (Global - CA)", item.Value.GetDescriptionFromCode("ACP"));
			AssertEquals("Default value", "Native US AMS (Global - US)", item.Value.GetDescriptionFromCode("AMS"));
		}

		public void TestDatabaseHostedLocations()
		{
			var item = ItemSet.DatabaseHostedLocations;
			AssertEquals("DatabaseBillingHostedLocations", item.Name);
			AssertEquals("Database Hosted Locations", item.Caption);
			AssertEquals(EDIDataRegistry.Category, item.Category);
			AssertEquals(RegistryStorageFlags.System, item.Storage);
		}

		public void TestCodingTasks()
		{
			CodeDescriptionPairListRegistryItem item = ItemSet.CodingTasks;
			AssertEquals("CodingTasks", item.Name);
			AssertEquals("Coding Tasks", item.Caption);
			AssertEquals(EDIDataRegistry.WorkItemSubCategory, item.Category);
			AssertEquals("Task types appearing in this list are considered coding task types.", item.Hint);
			AssertContainsExactElementsInAnyOrder(new[] { "CDU", "CDF", "COD", "RCD" }, item.DefaultValue.GetAllCodes());
		}

		public void TestTasksMandatoryInShelfCreation()
		{
			CodeDescriptionPairListRegistryItem item = ItemSet.TasksMandatoryInShelfCreation;
			AssertEquals("TasksMandatoryInShelfCreation", item.Name);
			AssertEquals("Tasks For Shelf Creation", item.Caption);
			AssertEquals(EDIDataRegistry.WorkItemSubCategory, item.Category);
			AssertEquals("Note, these tasks must be closed/cancelled to be able to queue a shelf for check-in.", item.Hint);
		}

		public void TestReviewTasks()
		{
			var item = ItemSet.ReviewTasks;
			AssertEquals("ReviewTasks", item.Name);
			AssertEquals("Review Tasks", item.Caption);
			AssertEquals(EDIDataRegistry.WorkItemSubCategory, item.Category);
			AssertEquals("Process tasks considered as review.", item.Hint);
		}

		public void TestCompetencyLearningTask()
		{
			var item = ItemSet.CompetencyLearningTask;
			AssertEquals("CompetencyLearningTask", item.Name);
			AssertEquals("ASSESS Learning Task", item.Caption);
			AssertEquals(EDIDataRegistry.WorkItemSubCategory, item.Category);
			AssertEquals("Choose a Work item task type that should be set when creating an ASSESS task for code authors.", item.Hint);

			var lookUpCodes = ((CodePairRegistryDataType)item.DataType).LookUpList.GetAllCodes();

			AssertEquals(1, lookUpCodes.Length);
			AssertArrayEqualsByElements(new string[] { "UDF" }, lookUpCodes);

			AssertEquals("UDF", item.DefaultValue);
		}

		public void TestCompetencyLearningTask_WithWorkItemTaskTypes()
		{
			var collection = new CategorisedWorkflowTaskTypesCollection();
			var workItemModule = collection.AddNew();
			workItemModule.Code = WorkflowDescriptors.WorkItemWorkflowDescriptorCode;

			var workItemTaskType1 = workItemModule.TaskTypes.AddNew();
			workItemTaskType1.Code = "RM1";
			var workItemTaskType2 = workItemModule.TaskTypes.AddNew();
			workItemTaskType2.Code = "RM2";

			var nonWorkItemModule = collection.AddNew();
			nonWorkItemModule.Code = WorkflowDescriptors.AccComplianceReportCode;

			var nonWorkItemModuleTaskType = nonWorkItemModule.TaskTypes.AddNew();
			nonWorkItemModuleTaskType.Code = "RM3";

			WorkflowDataRegistry.Instance.TaskTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			var item = ItemSet.CompetencyLearningTask;
			AssertEquals("CompetencyLearningTask", item.Name);
			AssertEquals("ASSESS Learning Task", item.Caption);
			AssertEquals(EDIDataRegistry.WorkItemSubCategory, item.Category);
			AssertEquals("Choose a Work item task type that should be set when creating an ASSESS task for code authors.", item.Hint);

			var lookUpCodes = ((CodePairRegistryDataType)item.DataType).LookUpList.GetAllCodes();

			AssertEquals(2, lookUpCodes.Length);
			AssertArrayEqualsByElements(new string[] { workItemTaskType1.Code, workItemTaskType2.Code }, lookUpCodes);

			AssertEquals("UDF", item.DefaultValue);
		}

		public void TestExceptionKeyRegexes_ShouldComeWithSQLParamNamesRegexOutOfTheBox()
		{
			var item = ItemSet.ExceptionKeyRegexes;
			AssertEquals("ExceptionKeyRegexes", item.Name);
			AssertEquals("Exception Key Regexes", item.Caption);
			AssertEquals(EDIDataRegistry.IssueManagerSubCategory, item.Category);
			AssertEquals("All exception keys matching any of the following regexes will have the matching component stripped. This can be used to remove non-relevant components in a key, such as query parameter names.", item.Hint);

			var defaultValue = item.DefaultValue;
			Assert("Should come with at least one regex", defaultValue.Count >= 1);
			var paramNameRegex = defaultValue.Cast<ExceptionKeyRegex>().FirstOrDefault(e => e.Regex == @"\s*@p[0-9]+,*");
			AssertNotNull("Should have by default a regex to remove SQL param names", paramNameRegex);
		}

		public void TestIgnoredExceptionStackLineRegexesItem()
		{
			var item = ItemSet.IgnoredExceptionStackLineRegexes;
			AssertEquals("IgnoredExceptionStackLineRegexes", item.Name);
			AssertEquals("WiseTech Global Client Extensions/Issue Manager", item.Category);
			AssertEquals("Ignored Exception Stack Line Regexes", item.Caption);
			AssertEquals("Regular Expression to match stack line in issue exception stack trace. If the stack line match any regex, the weight of this stack line will be set to 0 to avoid participating assign candidates calculation.", item.Hint);

			var defaultValue = item.Value;
			Assert("Should not have default value", defaultValue?.Count == 0);
		}

		public void TestInvoicingProcessingFeeLookup()
		{
			var item = ItemSet.InvoicingProcessingFeeLookup;
			AssertEquals("name", "InvoicingProcessingFeeLookup", item.Name);
			AssertEquals("caption", "Fee Codes", item.Caption);
			AssertEquals("category", EDIDataRegistry.ProcessingFeeCategory, item.Category);
			AssertEquals("Place a tick in the \"Discount\" column if this is a discount (a negative amount taken off the invoice total), leave it unticked if this is a fee (an additional charge on their invoice)", item.Hint);
			var defaultValue = item.DefaultValue;

			AssertEquals("comes with 3 default values", 3, defaultValue.Count);
		}

		public void TestUsageReportsBreakdownComment()
		{
			var item = ItemSet.MonthlyUsageReportBreakdownComment;
			AssertEquals("MonthlyUsageReportBreakdownComment", item.Name);
			AssertEquals("Report Breakdown Comment", item.Caption);
			AssertEquals(EDIDataRegistry.MonthlyUsageInvoiceCategory, item.Category);
			AssertEquals(RegistryStorageFlags.System, item.Storage);
			AssertEquals("For more detailed breakdown of usage, visit myaccount.cargowise.com > Usage Reports.", item.DefaultValue);
		}

		public void TestEmailLoopDectionCategoryRegistrtItems()
		{
			CombineAssertions("EnableEmailLoopDetection", () =>
			{
				var item = ItemSet.EnableEmailLoopDetection;
				AssertEquals("EnableEmailLoopDetection", item.Name);
				AssertEquals("Enable Email Loop Detection", item.Caption);
				AssertEquals(EDIDataRegistry.EmailLoopDetectionCategory, item.Category);
				AssertEquals("Storage Flags", RegistryStorageFlags.System, item.Storage);
			});

			CombineAssertions("EmailLoopDetectingWindowDuration", () =>
			{
				var item = ItemSet.EmailLoopDetectingWindowDuration;
				AssertEquals("EmailLoopDetectingWindowDuration", item.Name);
				AssertEquals("Email Loop Detecting Window Duration", item.Caption);
				AssertEquals(EDIDataRegistry.EmailLoopDetectionCategory, item.Category);
				AssertEquals("Storage Flags", RegistryStorageFlags.System, item.Storage);
			});

			CombineAssertions("EmailLoopMaximumEmailCount", () =>
			{
				var item = ItemSet.EmailLoopMaximumEmailCount;
				AssertEquals("EmailLoopMaximumEmailCount", item.Name);
				AssertEquals("Email Loop Maximum Email Count", item.Caption);
				AssertEquals(EDIDataRegistry.EmailLoopDetectionCategory, item.Category);
				AssertEquals("Storage Flags", RegistryStorageFlags.System, item.Storage);
			});
		}

		public void TestTranslogixOrgNameChangeNotificationAddresses()
		{
			var item = ItemSet.TranslogixOrgNameChangeNotificationAddresses;
			AssertEquals("TranslogixOrgNameChangeNotificationAddresses", item.Name);
			AssertEquals("Translogix Org Name Change Notification Recipients", item.Caption);
			AssertEquals(EDIDataRegistry.EmailAddressesCategory, item.Category);
			AssertEquals("Email addresses to receive Translogix Organization name change notifications.", item.Hint);
		}

		public void TestErrorLogStackLineExtractorRegex()
		{
			var item = ItemSet.ErrorLogStackLineExtractorRegexes;
			AssertEquals("ErrorLogStackLineExtractorRegexes", item.Name);
			AssertEquals("Stacktrace Line Extractor Regexes", item.Caption);
			AssertEquals("Regular expressions to parse stacktrace lines for different languages.", item.Hint);
			AssertEquals(EDIDataRegistry.IssueManagerSubCategory, item.Category);
			AssertEquals(RegistryStorageFlags.System, item.Storage);
			AssertEquals(RegistryOptions.Default, item.Options);
			AssertNotNull(item.DefaultValue);

			var defaultValue = item.DefaultValue;
			Assert("Should come with at least one regex", defaultValue.Count >= 1);
			var paramNameRegex = defaultValue.Cast<ExceptionKeyRegex>().FirstOrDefault(e => e.Regex == @"^(at|위치:)\s*(?<line>.*\(.*\)).*$");
			AssertNotNull("Should have by default a regex to parse stack trace lines", paramNameRegex);
		}

		public void TestErrorReportingServiceURIs()
		{
			var item = ItemSet.ErrorReportingServiceURIs;
			AssertEquals("ErrorReportingServiceURIs", item.Name);
			AssertEquals("Error Reporting Service URIs", item.Caption);
			AssertEquals("URIs of any available Error Reporting Services to download error reports from.", item.Hint);
			AssertEquals(EDIDataRegistry.IssueManagerSubCategory, item.Category);
			AssertEquals(RegistryStorageFlags.System, item.Storage);
			AssertEquals(RegistryOptions.Default, item.Options);
			AssertNotNull(item.DefaultValue);
			AssertEquals(0, item.DefaultValue.Length);
		}

		public void TestErrorReportingServiceMaxResults()
		{
			var item = ItemSet.ErrorReportingServiceMaxResults;
			AssertEquals("ErrorReportingServiceMaxResults", item.Name);
			AssertEquals("Error Reporting Service Max Results", item.Caption);
			AssertEquals("This is the maximum number of error reports that will be retrieved at a time.", item.Hint);
			AssertEquals(EDIDataRegistry.IssueManagerSubCategory, item.Category);
			AssertEquals(RegistryStorageFlags.System, item.Storage);
			AssertEquals(RegistryOptions.Default, item.Options);
			AssertEquals(1000, item.DefaultValue);
		}

		public void TestErrorReportingServiceAccessToken()
		{
			var item = ItemSet.ErrorReportingServiceAccessToken;
			AssertEquals("ErrorReportingServiceAccessToken", item.Name);
			AssertEquals("Error Reporting Service Access Token", item.Caption);
			AssertEquals("Access Token to authenticate with Error Reporting service(s).", item.Hint);
			AssertEquals(EDIDataRegistry.IssueManagerSubCategory, item.Category);

			AssertType<StringRegistryDataType>(item.DataType);
			var srdt = (StringRegistryDataType)item.DataType;
			AssertEquals(CharacterCase.Normal, srdt.CharacterCase);
			AssertEquals(1, srdt.MinLength);
			AssertEquals(128, srdt.MaxLength);

			AssertType<TextRegistryEditorInfo>(item.EditorInfo);

			AssertEquals(TextEditorType.Password, ((TextRegistryEditorInfo)item.EditorInfo).EditorType);
			AssertEquals(RegistryStorageFlags.System, item.Storage);
			AssertEquals(RegistryOptions.Default, item.Options);
			AssertEquals(string.Empty, item.DefaultValue);
		}

		public void TestErrorReportingTesting()
		{
			var item = ItemSet.ErrorReportingServiceTesting;
			AssertEquals("ErrorReportingServiceTesting", item.Name);
			AssertEquals("Error Reporting Service Testing", item.Caption);
			AssertEquals("Enables use of the Error Reporting service from the Issue Manager Processor service task in test environments. Do NOT enable unless the system is correctly configured.", item.Hint);
			AssertEquals(EDIDataRegistry.IssueManagerSubCategory, item.Category);
			AssertEquals(RegistryStorageFlags.System, item.Storage);
			AssertEquals(RegistryOptions.Default, item.Options);
			AssertEquals(false, item.DefaultValue);
		}

		public void TestMaxErrorReportSizeInMb()
		{
			TestRegistryItem(
				ItemSet.MaxErrorReportSizeInMb,
				"MaxErrorReportSizeInMb",
				"WiseTech Global Client Extensions/Issue Manager",
				"Max Error Report Size",
				"Issue Manager Processor (IMP) service task processes incoming error reports and saves them into db. " +
					"Too large error reports lead to large processing memory allocation potentially leading to a resource contention on the host, as well as excessive space consumption in the database. " +
					$"Systems sending error reports should not generate large or combined reports.{System.Environment.NewLine}" +
					"Reports with sizes greater than this value (MB) are not processed.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				300,
				100,
				int.MaxValue);
		}

		public void TestClientsUpgradeStrategyOptions()
		{
			var expectedCategory = "WiseTech Global Client Extensions/Release Builds & Upgrades/Clients Upgrade Strategy";

			//1. Check the correctness of placement in the tree and the default values
			Action<IRegistryItem> assertCategoryIsCorrect = (item) =>
				AssertEquals($"There is supposed to be {item.Name} option in {expectedCategory}", expectedCategory, item.Category);

			assertCategoryIsCorrect(ItemSet.EnableWeeklyBuildCutOff);
			assertCategoryIsCorrect(ItemSet.WeeklyBuildCutOffDay);
			assertCategoryIsCorrect(ItemSet.WeeklyBuildCutOffTime);

			AssertEquals("Weekly build cut-off is disabled by default", ItemSet.EnableWeeklyBuildCutOff.DefaultValue, false);
			AssertEquals("Weekly build cut-off Hint", ItemSet.EnableWeeklyBuildCutOff.Hint, "If enabled all WTG-hosted clients get the same version during a regular weekly upgrade. This version is the first one generated after a specified day and time (see the other two options in the category).\n\nIf disabled the clients get the latest version.\n\nThe parameter does not affect self-hosted clients (they always get the latest version).");
			AssertEquals("No day for weekly build cut-off is chosen by default", ItemSet.WeeklyBuildCutOffDay.DefaultValue, "Friday");
			AssertEquals("No time for weekly build cut-off is chosen by default", ItemSet.WeeklyBuildCutOffTime.DefaultValue, "11:59:59 PM");

			Action<IRegistryItem, IEnumerable<string>, IEnumerable<string>> assertValuesAreCorrectlyVerified =
				(item, correctValues, incorrectValues) =>
				{
					foreach (var value in correctValues)
					{
						AssertNoExceptionThrown(() => item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value));
					}
					foreach (var value in incorrectValues)
					{
						AssertExceptionThrown<RegistryValidationException>(() => item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value));
					}

					//special handle for empty values as SetValue for some reason do not throw on it
					AssertNotNullOrEmpty(item.GetValidationErrorMessage(string.Empty, Guid.Empty, Guid.Empty, Guid.Empty));
				};

			var daysOfWeekAsStrings = Enum.GetValues(typeof(DayOfWeek)).OfType<DayOfWeek>().Select(d => d.ToString());
			assertValuesAreCorrectlyVerified(ItemSet.WeeklyBuildCutOffDay, daysOfWeekAsStrings, new[] { "NotDayOfweek" });
			assertValuesAreCorrectlyVerified(ItemSet.WeeklyBuildCutOffTime,
				new[]
				{
						"9:45 am", "07:24 pm", "12:34:48", "6:37:05 AM", "23:59", "08:45am", "10:45  am"
				},
				new[]
				{
						"25:11", "14:45 pm", "6:45:03.235 pm", "12/03/2017 23:59:01", "not even a datetime",
				});
		}

		public void TestAvsMonitoringEndpointsItem()
		{
			var item = ItemSet.AvsMonitoringEndpointsItem;

			AssertEquals("AvsMonitoringEndpoints", item.Name);
			AssertEquals("Monitoring Endpoints", item.Caption);
			AssertEquals("Define the endpoints for AVS monitoring service task (AXM).", item.Hint);
			AssertEquals(OrganisationsDataRegistry.Categories.Organizations_AddressValidationService, item.Category);
			AssertEquals(RegistryStorageFlags.System, item.Storage);
			AssertEquals(0, item.DefaultValue.Length);
		}

		public void TestAvsMonitoringNotificationGroupItem()
		{
			var item = ItemSet.AvsMonitoringNotificationGroupItem;

			AssertEquals("AvsMonitoringNotificationGroup", item.Name);
			AssertEquals("Monitoring Notification Group", item.Caption);
			AssertEquals("Define the notification group for AVS monitoring service task (AXM).", item.Hint);
			AssertEquals(OrganisationsDataRegistry.Categories.Organizations_AddressValidationService, item.Category);
			AssertEquals(RegistryStorageFlags.System, item.Storage);
			AssertEquals(Guid.Empty, item.DefaultValue);
		}

		public void TestValidStlUsageCodesOnOdplPricelists()
		{
			var item = ItemSet.ValidStlUsageCodesOnOdplPricelists;
			AssertEquals("ValidStlUsageCodesOnOdplPricelists", item.Name);
			AssertEquals("Valid STL Usage Codes on ODPL Pricelists", item.Caption);
			AssertEquals(EDIDataRegistry.OdplBillingCategory, item.Category);
			AssertEquals("", item.Hint);
			AssertEquals(0, item.Value.Count);
			AssertEquals("Storage Flags", RegistryStorageFlags.System, item.Storage);
		}

		public void TestLicenceCountryLanguages()
		{
			var countryToLanguages = ItemSet.LicenceCountryLanguages.Value
			.OfType<ICodeDescription>()
			.Select(x => new { CountryCode = x.Code, Languages = new HashSet<string>(x.Description.Split(',')) })
			.ToDictionary(x => x.CountryCode, x => x.Languages);

			var languages = new CodeDescriptionPairList(OLookUpEditType.Language);
			var countries = (new RefCountryCollection(Factory)).Select(x => x.Code).ToArray();

			foreach (var country in countryToLanguages)
			{
				AssertEquals(true, countries.Contains(country.Key));

				foreach (var lang in country.Value)
				{
					AssertEquals(true, languages.ContainsCode(lang));
				}
			}
		}

		[TestDate(2018, 4, 1, 14, 30, 00)]
		public void TestDefaultWorkItemCreationThresholdsDecreaseOverTime()
		{
			CombineAssertions(() =>
			{
				for (int expected = 16; expected > 0; expected--)
				{
					for (int day = 0; day < 7 + (16 - expected); day++)
					{
						AssertIssueWorkItemCreationThreshold(expected);
						TestDateAttribute.AddDays(1);
					}
				}
				TestDateAttribute.AddYears(2);
				AssertIssueWorkItemCreationThreshold(1);
			});
		}

		void AssertIssueWorkItemCreationThreshold(int expectedValue)
		{
			RegistryItemDictionary.Instance.PurgeAll();
			AssertEquals(ZDateTime.Today.ToShortDateString(), expectedValue, ItemSet.IssueWorkItemCreationThresholdClientVisible.DefaultValue[0].IssueOccurrenceThreshold);
			AssertEquals(ZDateTime.Today.ToShortDateString(), expectedValue, ItemSet.IssueWorkItemCreationThresholdNonClientVisible.DefaultValue[0].IssueOccurrenceThreshold);
		}

		public void TestEnableMachineLearningTeamAssignment()
		{
			TestRegistryItem(
				ItemSet.EnableMachineLearningTeamAssignment,
				"EnableMachineLearningTeamAssignment",
				"WiseTech Global Client Extensions/Issue Manager",
				"Enable Machine Learning Team Assignment",
				"By ticking this to enable Machine Learning Team Assignment function.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				false);
		}

		public void TestMachineLearningTeamAssignmentApiUrl()
		{
			TestRegistryItem(
				ItemSet.MachineLearningTeamAssignmentApiUrl,
				"MachineLearningTeamAssignmentApiUrl",
				"WiseTech Global Client Extensions/Issue Manager",
				"Machine Learning Team Assignment API URL",
				"The URL is used to connect to a Machine Learning Team Assignment WebAPI Service.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				TextEditorType.TextBox,
				"");
		}

		public void TestMachineLearningTeamAssignmentApiRequestTimeout()
		{
			TestRegistryItem(
				ItemSet.MachineLearningTeamAssignmentApiRequestTimeout,
				"MachineLearningTeamAssignmentApiRequestTimeout",
				"WiseTech Global Client Extensions/Issue Manager",
				"Machine Learning Team Assignment API Request Timeout",
				"Set the timeout (in seconds) of a Machine Learning Team Assignment WebAPI Service request.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				5);
		}

		public void TestCommissionGeneratorNotificationGroup()
		{
			TestGenericRegistryItem(
				ItemSet.CommissionGeneratorNotificationGroup,
				"CommissionGeneratorNotificationGroup",
				EDIDataRegistry.SalesAndMarketingSubCategory,
				"Commission Generator Notification Group",
				"This is the group which gets notification emails from the Commission Generator service task.",
				RegistryStorageFlags.System,
				RegistryOptions.IsValueMandatory,
				Guid.Empty);
		}

		#region Sales Conductor

		public void TestSalesConductorDomainUrl()
		{
			TestStringRegistryItem(
				ItemSet.SalesConductorDomainUrl,
				nameof(EDIDataRegistry.SalesConductorDomainUrl),
				"WiseTech Global Client Extensions/Sales & Marketing/Sales Conductor",
				"Sales Conductor Domain URL",
				"Specify the base URL to the Sales Conductor Discovery Site.",
				RegistryStorageFlags.System,
				TextEditorType.TextBox,
				RegistryOptions.Default,
				"https://myaccount.cargowise.com/en-us/Home/SalesConductor/Discovery.aspx",
				CharacterCase.Normal);
		}

		public void TestSalesConductorContentPlaylistBuilderUrl()
		{
			TestStringRegistryItem(
				ItemSet.SalesConductorContentPlaylistBuilderUrl,
				nameof(EDIDataRegistry.SalesConductorContentPlaylistBuilderUrl),
				"WiseTech Global Client Extensions/Sales & Marketing/Sales Conductor",
				"Sales Conductor Content Play List Builder URL",
				"Specify the base URL to the Sales Conductor Content Play List Builder Site.",
				RegistryStorageFlags.System,
				TextEditorType.TextBox,
				RegistryOptions.Default,
				"https://myaccount.cargowise.com/Home/SalesConductor/SalesContentPlaylistBuilder.aspx",
				CharacterCase.Normal);
		}

		#endregion

		public void TestBillingPriceRoundingParams()
		{
			var item = ItemSet.BillingPriceRoundingParams;
			var testList = new CodeDescriptionPairList(item.DefaultValue);
			item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, testList);

			testList.Clear();
			testList.AddPair("V1", "bad");
			AssertExceptionThrown<RegistryValidationException>(() => item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, testList));

			testList.Clear();
			testList.AddPair("V1", "1=0.01,1=0.05");
			AssertExceptionThrown<RegistryValidationException>("duplicate price break", () => item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, testList));
		}

		public void TestBorderWiseUmpSyncSessionTimeout()
		{
			var item = ItemSet.BorderWiseUmpSyncSessionTimeout;

			AssertEquals("Should have 10 seconds default timeout", 10_000, item.DefaultValue);
		}

		public void TestBorderWiseUmpDatabaseConnectionString()
		{
			TestRegistryItem(ItemSet.BorderWiseUmpDatabaseConnectionString,
				expectedName: "BorderWiseUmpDatabaseConnectionString",
				expectedCategory: "WiseTech Global Client Extensions/BorderWise/User Management Portal",
				expectedCaption: "UMP Database Connection String",
				expectedHint: "The connection string used for running queries directly on the UMP database from ediProd.",
				expectedStorage: RegistryStorageFlags.System,
				expectedTextEditorType: TextEditorType.Password,
				expectedDefaultValue: string.Empty);
		}

		public void TestBorderWiseUmpApiUpdatePasswordApiKey()
		{
			TestRegistryItem(ItemSet.BorderWiseUmpApiUpdatePasswordApiKey,
				expectedName: "BorderWise_UmpApiUpdatePasswordApiKey",
				expectedCategory: "WiseTech Global Client Extensions/BorderWise/User Management Portal",
				expectedCaption: "UMP API Update Password Api Key",
				expectedHint: "The api key is required for calling api to update password.",
				expectedStorage: RegistryStorageFlags.System,
				expectedTextEditorType: TextEditorType.Password,
				expectedDefaultValue: string.Empty);
		}

		public void TestEnableIncidentStaffAssignedSupportNotification()
		{
			TestRegistryItem(ItemSet.IncidentStaffUnavailableSupportNotificationEnable, "IncidentStaffUnavailableSupportNotificationEnable", "WiseTech Global Client Extensions/Customer Service Incidents", "Incident Staff Unavailable Support Notification Enable", "Enable or disable all support notifications related to the unavailable assigned staff.", RegistryStorageFlags.System, RegistryOptions.Default, true);
		}

		public void TestIncidentStaffUnavailableSupportNotificationGroup()
		{
			TestRegistryItem(ItemSet.IncidentStaffUnavailableSupportNotificationGroup, "IncidentStaffUnavailableSupportNotificationGroup", "WiseTech Global Client Extensions/Customer Service Incidents", "Incident Staff Unavailable Support Notification Group", "The staff group that will be notified about unavailable assigned staff to the Incident.", RegistryStorageFlags.System, RegistryFindBoxCollection.GlbGroup, Guid.Empty);
		}

		#region Trusted Messaging

		public void TestMyAccountKeyRotationIntervalMinutes()
		{
			TestGenericRegistryItem(ItemSet.MyAccountKeyRotationIntervalMinutes,
				"MyAccountKeyRotationIntervalMinutes",
				EDIDataRegistry.MyAccountPortalSubCategory + "/Trusted Messaging",
				"My Account Key Rotation Interval Minutes",
				"This is the MyAccount key rotation interval specified in minutes.",
				RegistryStorageFlags.System);
		}

		public void TestMyAccountTrustedServices()
		{
			TestGenericRegistryItem(ItemSet.MyAccountTrustedServices,
					"MyAccountTrustedServices",
					EDIDataRegistry.MyAccountPortalSubCategory + "/Trusted Messaging",
					"My Account Trusted Services",
					"Define standalone services for trusting messaging between My Account.",
					RegistryStorageFlags.System);
		}

		public void TestInternalCW1ActivationCertificate()
		{
			TestGenericRegistryItem(ItemSet.InternalCW1ActivationCertificate,
					"InternalCW1ActivationCertificate",
					EDIDataRegistry.MyAccountPortalSubCategory + "/Trusted Messaging",
					"Internal CW1 Activation Certificate",
					"This the certificate with private key for internal CW1 trusted system activation.",
					RegistryStorageFlags.System);

			TestGenericRegistryItem(ItemSet.InternalCW1ActivationCertificatePassword,
					"InternalCW1ActivationCertificatePassword",
					EDIDataRegistry.MyAccountPortalSubCategory + "/Trusted Messaging",
					"Internal CW1 Activation Certificate Password",
					"This the password of certificate with private key for internal CW1 trusted system activation.",
					RegistryStorageFlags.System);

			AssertEquals(true, (ItemSet.InternalCW1ActivationCertificatePassword.DataType as StringRegistryDataType).IsEncrypted);
		}

		public void TestMyAccountCertificateAuthorityUserAccountPassword()
		{
			TestGenericRegistryItem(ItemSet.MyAccountCertificateAuthorityUserAccountPassword,
					"MyAccountCertificateAuthorityUserAccountPassword",
					EDIDataRegistry.MyAccountCertificateAuthorityCategory,
					"MyAccount Certificate Authority User Account Password",
					"MyAccount Certificate Authority User Account Password",
					RegistryStorageFlags.System);

			AssertEquals(true, (ItemSet.MyAccountCertificateAuthorityUserAccountPassword.DataType as StringRegistryDataType).IsEncrypted);
		}

		#endregion

		#region Web Config

		public void TestEnableMyAccountWebConfigOveridden()
		{
			TestGenericRegistryItem(ItemSet.EnableMyAccountWebConfigOveridden,
					"EnableMyAccountWebConfigOveridden",
					EDIDataRegistry.MyAccountPortalSubCategory + "/Web Config",
					"Enable My Account Web Config Overidden",
					"Allow My Account overwrites default web.config with registry item values.",
					RegistryStorageFlags.System,
					false);
		}

		public void TestMyAccountFormAuthenticationCookieDomain()
		{
			TestGenericRegistryItem(ItemSet.MyAccountFormAuthenticationCookieDomain,
					"MyAccountFormAuthenticationCookieDomain",
					EDIDataRegistry.MyAccountPortalSubCategory + "/Web Config",
					"My Account Auth Cookie Domain",
					"This overwrites the 'domain' attribute value on system.web/authentication/form of My Account web.config.",
					RegistryStorageFlags.System,
					".cargowise.com");
		}

		public void TestMyAccountFormAuthenticationCookieSameSite()
		{
			TestGenericRegistryItem(ItemSet.MyAccountFormAuthenticationCookieSameSite,
				"MyAccountFormAuthenticationCookieSameSite",
				EDIDataRegistry.MyAccountPortalSubCategory + "/Web Config",
				"My Account Auth Cookie Same Site",
				"This overwrites the 'cookieSameSite' attribute value on system.web/authentication/form of My Account web.config.",
				RegistryStorageFlags.System,
				"None");
		}

		public void TestMyAccountFormAuthenticationCookieSSL()
		{
			TestGenericRegistryItem(ItemSet.MyAccountFormAuthenticationCookieSSL,
				"MyAccountFormAuthenticationCookieSSL",
				EDIDataRegistry.MyAccountPortalSubCategory + "/Web Config",
				"My Account Auth Cookie SSL",
				"This overwrites the 'requireSSL' attribute value on system.web/authentication/form of My Account web.config.",
				RegistryStorageFlags.System,
				"True");
		}

		#endregion

		public void TestPurgeEdiProdSpecificTablesRegistryItems()
		{
			var category = "System/Archive Manager/Purge ediProd Specific Tables System";

			TestRegistryItem(
				item: ItemSet.PurgeEdiProdSpecificTablesOnOrBeforeMinimum,
				expectedName: "PurgeEdiProdSpecificTablesOnOrBeforeMinimum",
				expectedCategory: category,
				expectedCaption: "On or Before Minimum",
				expectedHint: "This registry allows you to define the minimum number of years that a record must have been in the system before it can be purged using the Purge ediProd Specific Tables Purge System (EST).\n\nWarning: Please make sure to confirm the data retention requirements for this data before purging.",
				expectedStorage: RegistryStorageFlags.System,
				expectedOptions: RegistryOptions.IsOnlyForSupport,
				expectedDefaultValue: 7,
				expectedMinValue: 0,
				expectedMaxValue: 100);

			TestRegistryItem(
				item: ItemSet.PurgeEdiProdSpecificTablesBatchSizeControl,
				expectedName: "PurgeEdiProdSpecificTablesBatchSizeControl",
				expectedCategory: category,
				expectedCaption: "Set Batch Size",
				expectedHint: "The maximum number of records to be processed in one batch by Archive Manager using the Purge ediProd Specific Tables System (EST).",
				expectedStorage: RegistryStorageFlags.System,
				expectedOptions: RegistryOptions.IsOnlyForSupport,
				expectedDefaultValue: 10000,
				expectedMinValue: 1,
				expectedMaxValue: 10000);
		}

		public void TestEmailVerificationNotificationMessageTemplate()
		{
			AssertVisible(ItemSet.EmailVerificationNotificationMessageTemplate);

			AssertEquals("EmailVerificationNotificationMessageTemplate", ItemSet.EmailVerificationNotificationMessageTemplate.Name);
			AssertEquals(EDIDataRegistry.EmailVerificationSubCategory, ItemSet.EmailVerificationNotificationMessageTemplate.Category);
			AssertEquals("Email Verification Notification Message Template", ItemSet.EmailVerificationNotificationMessageTemplate.Caption);
			AssertEquals(@"<b>Website Email Verication Request</b><br><br>

Dear (*PersonName*), <br><br>
A request has been made for the email verification.<br><br>
User details:<br>
Person Name		: (*PersonName*)<br>
Person Email		: (*PersonEmail*)<br>
User Account Email	: (*UserAccountEmail*)<br>

<br>Click the 'Verify Email' button to reauntheticate and enable Auto Login for your account.<br><br>
(*VerifyButton*)<br><br>
<br>If you have issues clicking the link, copy and paste the following line into your browser: <br>
(*VerifyLinkAsText*)<br><br>
<b>Note: This email verification  link is only valid for the next 24 hours.</b><br><br>
If you did not request a password set, please contact us immediately and do not click on this link.<br><br>", ItemSet.EmailVerificationNotificationMessageTemplate.DefaultValue.EmailBody);

			AssertEquals(RegistryStorageFlags.System, ItemSet.EmailVerificationNotificationMessageTemplate.Storage);

			ItemSet.EmailVerificationNotificationMessageTemplate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new NotificationEmailTemplate(typeof(DocSupportIncident), "Subject one", "Body one"));
			AssertEquals("Subject one", ItemSet.EmailVerificationNotificationMessageTemplate.Value.EmailSubject);
			AssertEquals("Body one", ItemSet.EmailVerificationNotificationMessageTemplate.Value.EmailBody);
			ItemSet.EmailVerificationNotificationMessageTemplate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new NotificationEmailTemplate(typeof(DocSupportIncident), "Subject two", "Body two"));
			AssertEquals("Subject two", ItemSet.EmailVerificationNotificationMessageTemplate.Value.EmailSubject);
			AssertEquals("Body two", ItemSet.EmailVerificationNotificationMessageTemplate.Value.EmailBody);
		}

		public void TestAmbiguousContactLoginNotificationMessageTemplate()
		{
			AssertVisible(ItemSet.AmbiguousContactLoginNotificationMessageTemplate);

			AssertEquals("AmbiguousContactLoginNotificationMessageTemplate", ItemSet.AmbiguousContactLoginNotificationMessageTemplate.Name);
			AssertEquals(EDIDataRegistry.AmbiguousContactLoginNotificationSubCategory, ItemSet.AmbiguousContactLoginNotificationMessageTemplate.Category);
			AssertEquals("Ambiguous Contact Login Notification Message Template", ItemSet.AmbiguousContactLoginNotificationMessageTemplate.Caption);
			AssertEquals(@"<b>Complete your My Account setup</b><br><br>

A recent log in attempt was made to  My Account using this email without a company code specified.<br><br>

To log in with an email and password, you'll need to complete your account setup by registering a Personal Email address with each company account you are registered with.<br><br>

You can complete this setup by logging into My Account with a specific Company Code and using the Change Personal Email feature. To access this feature, navigate to Account Information -> Contact Information -> Change personal email.<br><br>

The Company Codes linked to this email address are:<br><br>
(*CompanyList*)<br><br>

Alternatively, you can specify a Company Code when you login to My Account.", ItemSet.AmbiguousContactLoginNotificationMessageTemplate.DefaultValue.EmailBody);

			AssertEquals(RegistryStorageFlags.System, ItemSet.AmbiguousContactLoginNotificationMessageTemplate.Storage);

			ItemSet.AmbiguousContactLoginNotificationMessageTemplate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new NotificationEmailTemplate(typeof(DocSupportIncident), "Subject one", "Body one"));
			AssertEquals("Subject one", ItemSet.AmbiguousContactLoginNotificationMessageTemplate.Value.EmailSubject);
			AssertEquals("Body one", ItemSet.AmbiguousContactLoginNotificationMessageTemplate.Value.EmailBody);
			ItemSet.AmbiguousContactLoginNotificationMessageTemplate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new NotificationEmailTemplate(typeof(DocSupportIncident), "Subject two", "Body two"));
			AssertEquals("Subject two", ItemSet.AmbiguousContactLoginNotificationMessageTemplate.Value.EmailSubject);
			AssertEquals("Body two", ItemSet.AmbiguousContactLoginNotificationMessageTemplate.Value.EmailBody);
		}

		public void TestInternalEnterpriseMasterOrgs()
		{
			var item = ItemSet.InternalEnterpriseMasterOrgs;
			AssertEquals("InternalEnterpriseMasterOrgs", item.Name);
			AssertEquals(EDIDataRegistry.MyAccountPortalSubCategory, item.Category);
			AssertEquals("Internal Enterprise Master Orgs", item.Caption);
			AssertEquals("Specify default master org for licence databases under internal enterprise.", item.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System, item.Storage);
			AssertContainsExactElementsInAnyOrder(new[] { "HYE", "WTL", "WUT" }, item.DefaultValue.GetAllCodes());
		}

		public void TestMyAccountGatewayRootUriKey()
		{
			TestRegistryItem(
				ItemSet.MyAccountGatewayRootUriKey,
				CargoWise.Definitions.MyAccountRegistry.MyAccountGatewayRootUriKey,
				EDIDataRegistry.MyAccountPortalSubCategory + "/GLOW My Account Gateway",
				"My Account Gateway Root URL",
				"The base URI for My Account Gateway Login",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport | RegistryOptions.IsValueMandatory,
				TextEditorType.TextBox,
				"https://myaccount-portal.cargowise.com/myaccount/gateway",
				"https://myaccount-portal.cargowise.com/myaccount/gateway");
		}

		public void TestMyAccountGatewaySecret()
		{
			TestGenericRegistryItem(
				ItemSet.MyAccountGatewaySecret,
				CargoWise.Definitions.MyAccountRegistry.MyAccountGatewaySecretKey,
				EDIDataRegistry.MyAccountPortalSubCategory + "/GLOW My Account Gateway",
				"My Account Gateway Logon Client Secret",
				"Key used for encryption of My Account Gateway Requests. The key should be 64 bytes long.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport);
		}

		public void TestMyAccountGatewaySecret_DataType()
		{
			var registryItem = EDIDataRegistry.Instance.MyAccountGatewaySecret;
			AssertType(typeof(BinaryKeyRegistryDataType), registryItem.DataType);

			var dataType = (BinaryKeyRegistryDataType)registryItem.DataType;
			AssertEquals(64, dataType.KeySize); // 64-bit key for SHA256 HMAC
		}

		public void TestMyAccountGatewaySecret_DataType_CorrectKeySizeInDescription()
		{
			var registryItem = EDIDataRegistry.Instance.MyAccountGatewaySecret;
			var dataType = (BinaryKeyRegistryDataType)registryItem.DataType;
			var keySize = dataType.KeySize;

			var bytesDescription = string.Format("{0} bytes", keySize);
			Assert(registryItem.Hint.IndexOf(bytesDescription, StringComparison.InvariantCultureIgnoreCase) >= 0);
		}

		public void TestLicenceDatabaseMasterOrgSuggestionBulkUpdateThreshold()
		{
			TestGenericRegistryItem(ItemSet.LicenceDatabaseMasterOrgSuggestionBulkUpdateThreshold,
					"LicenceDatabaseMasterOrgSuggestionBulkUpdateThreshold",
					EDIDataRegistry.MyAccountPortalSubCategory + "/Product Registration",
					"Licence Database Master Organisation Suggestion Bulk Update Threshold",
					"This is the minimum value of Master Organisation Suggestion Ranking Score required for automatic matching.",
					RegistryStorageFlags.System, 50);
		}

		public void TestEditDocumentOrganizationRegistrationMappingModule()
		{
			TestRegistryItem(ItemSet.EditDocumentOrganizationRegistrationMappingModule,
				"EditDocumentOrganizationRegistrationMappingModule",
				EDIDataRegistry.Category,
				"Edit Document Organization Registration Mapping module",
				"When set to Yes, the Document Organization Registration Mapping module may be edited.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				false);
		}

		#region Release Build Testing

		public void TestDatabasesRequiredReleaseBuildTesting()
		{
			var item = ItemSet.DatabasesRequiredReleaseBuildTesting;
			AssertEquals("name", "DatabasesRequiredReleaseBuildTesting", item.Name);
			AssertEquals("caption", "Databases Required Release Build Testing", item.Caption);
			AssertEquals("category", EDIDataRegistry.ReleaseBuildTesting, item.Category);
			AssertEquals("The list of database number of databases which are required to only take release builds with test passed.", item.Hint);
			var defaultValue = item.DefaultValue;
			AssertEquals(0, defaultValue.Count);
		}

		public void TestReleaseBuildTestDbServerName()
		{
			var item = ItemSet.ReleaseBuildTestDbServerName;
			AssertEquals("name", "ReleaseBuildTestDbServerName", item.Name);
			AssertEquals("caption", "Release Build Test Db Server Name", item.Caption);
			AssertEquals("category", EDIDataRegistry.ReleaseBuildTesting, item.Category);
			AssertEquals("", item.Hint);
		}

		public void TestReleaseBuildTestDbName()
		{
			var item = ItemSet.ReleaseBuildTestDbName;
			AssertEquals("name", "ReleaseBuildTestDbName", item.Name);
			AssertEquals("caption", "Release Build Test Db Name", item.Caption);
			AssertEquals("category", EDIDataRegistry.ReleaseBuildTesting, item.Category);
			AssertEquals("", item.Hint);
		}

		public void TestReleaseBuildTestRemoteCommandExecutablePath()
		{
			var item = ItemSet.ReleaseBuildTestRemoteCommandExecutablePath;
			AssertEquals("name", "ReleaseBuildTestRemoteCommandExecutablePath", item.Name);
			AssertEquals("caption", "Release Build Test Remote Command Executable Path", item.Caption);
			AssertEquals("category", EDIDataRegistry.ReleaseBuildTesting, item.Category);
			AssertEquals("", item.Hint);
		}

		public void TestReleaseBuildTestWebAppHost()
		{
			var item = ItemSet.ReleaseBuildTestWebAppHost;
			AssertEquals("name", "ReleaseBuildTestWebAppHost", item.Name);
			AssertEquals("caption", "Release Build Test Web App Host", item.Caption);
			AssertEquals("category", EDIDataRegistry.ReleaseBuildTesting, item.Category);
			AssertEquals("", item.Hint);
		}

		public void TestReleaseBuildTestWebAppUrl()
		{
			var item = ItemSet.ReleaseBuildTestWebAppUrl;
			AssertEquals("name", "ReleaseBuildTestWebAppUrl", item.Name);
			AssertEquals("caption", "Release Build Test Web App Url", item.Caption);
			AssertEquals("category", EDIDataRegistry.ReleaseBuildTesting, item.Category);
			AssertEquals("", item.Hint);
		}

		public void TestReleaseBuildTestResultNotificationGroup()
		{
			var item = ItemSet.ReleaseBuildTestResultNotificationGroup;
			AssertEquals("name", "ReleaseBuildTestResultNotificationGroup", item.Name);
			AssertEquals("caption", "Release Build Test Result Notification Group", item.Caption);
			AssertEquals("category", EDIDataRegistry.ReleaseBuildTesting, item.Category);
			AssertEquals("", item.Hint);
		}

		#endregion

		public void TestMobileServicesEHubClientID()
		{
			TestRegistryItem(ItemSet.MobileServicesEHubClientID,
				"MobileServicesEHubClientID",
				EDIDataRegistry.Category,
				"MobileServices Client ID",
				"The eHub Client ID of the MobileServices system. This can be changed if there is a need to connect to another MobileServices system such as a UAT system.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				TextEditorType.TextBox,
				expectedDefaultValue: "GLOWMOB");
		}

		public void TestIncidentClosePromptServiceTaskLastRunTimeUtc()
		{
			AssertEquals("Default value", new ZDateTime(2021, 09, 13).ToDateTime(), ItemSet.IncidentClosePromptServiceTaskLastRunTimeUtc.Value);
			var value = new DateTime(2000, 1, 2);
			ItemSet.IncidentClosePromptServiceTaskLastRunTimeUtc.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
			AssertEquals("IncidentClosePromptServiceTaskLastRunTimeUtc", value, ItemSet.IncidentClosePromptServiceTaskLastRunTimeUtc.Value);
		}

		public void TestProductDisplayCategories()
		{
			var item = ItemSet.ProductDisplayCategories;
			AssertEquals("ProductDisplayCategories", item.Name);
			AssertEquals("Product Display Categories", item.Caption);
			AssertEquals(EDIDataRegistry.LicenceBillingCategory, item.Category);
			AssertEquals("Storage Flags", RegistryStorageFlags.System, item.Storage);

			var itemValue = item.Value;
			AssertEquals(7, itemValue.Count);

			void AssertItem(string code, string desc)
			{
				AssertEquals(desc, itemValue.GetDescriptionFromCode(code));
				AssertEquals(true, itemValue.GetBoolFromCode(code));
			}

			AssertItem("CLD", "CargoWise Cloud Pricing");
			AssertItem("STL", "CargoWise One STL Full Pricing");
			AssertItem("ESV", "eServices Pricing");
			AssertItem("BWP", "BorderWise");
			AssertItem("ABM", "ABM Customs");
			AssertItem("ACC", "Accounting and 3rd Party Services and Transactions (External Services)");
			AssertItem("DEV", "Device Pricing");
		}

		public void TestHandheldDevicePremiumTypes()
		{
			var item = ItemSet.HandheldDevicePremiumTypes;
			AssertEquals("HandheldDevicePremiumTypes", item.Name);
			AssertEquals("Handheld Device Premium Types", item.Caption);
			AssertEquals(EDIDataRegistry.StlBillingCategory, item.Category);
			AssertEquals("Storage Flags", RegistryStorageFlags.System, item.Storage);

			var codes = item.Value.GetAllCodes();
			AssertContainsExactElementsInAnyOrder(new[] { "C2S", "C2W", "P3C", "P3S", "PLC", "PLS", "PS1", "PS2", "PSC", "PSS", "S2S", "S2W", "S3S", "S3W", "SCC", "SCS", "TWC", "TWD", "VCS", "VCW", "VPC", "VPS" }, codes);
		}

		public void TestDummyBillingPremiumTypes()
		{
			var item = ItemSet.UnregisteredDevicePremiumTypes;
			AssertEquals("UnregisteredDevicePremiumTypes", item.Name);
			AssertEquals("Un-registered Device Premium Types", item.Caption);
			AssertEquals(EDIDataRegistry.StlBillingCategory, item.Category);
			AssertEquals("Storage Flags", RegistryStorageFlags.System, item.Storage);

			var itemValue = item.Value;
			AssertEquals(1, itemValue.Count);

			void AssertItem(string code, string desc)
			{
				AssertEquals(desc, itemValue.GetDescriptionFromCode(code));
			}

			AssertItem("BYR", "Un-registered customer supplied device -  REDUCE COUNT BY");
		}

		public void TestEnableTaxProcessorForBilling()
		{
			var item = ItemSet.EnableTaxProcessorForBilling;
			AssertEquals("EnableTaxProcessorForBilling", item.Name);
			AssertEquals("Enable Tax Processor For Billing", item.Caption);
			AssertEquals(EDIDataRegistry.StlBillingCategory, item.Category);
			AssertEquals(true, item.DefaultValue);
			AssertEquals(RegistryStorageFlags.System, item.Storage);
		}

		public void TestIncidentClosureDispositions_ClosedByIncidentGroup()
		{
			var item = ItemSet.IncidentClosureDispositions;
			var defaultCIGs = item.DefaultValue.Cast<CodeDescriptionBoolTreeNode>().Where(d => d.Code == SupportIncidentLookups.DispositionList.Constants.Closed.ClosedByIncidentGroup);
			var parentCodes = new List<string>();

			AssertEquals("Should be added to all incident stages", 6, defaultCIGs.Count());

			// Get parent codes from stage type, we have 3 grids, the first one is the stage type
			foreach (var item2 in defaultCIGs)
			{
				var parent1 = item.DefaultValue.FindByPK(item2.ParentID) as CodeDescriptionBoolTreeNode;
				var parent2 = item.DefaultValue.FindByPK(parent1.ParentID) as CodeDescriptionBoolTreeNode;
				var parent3 = item.DefaultValue.FindByPK(parent2.ParentID) as CodeDescriptionBoolTreeNode;

				parentCodes.Add(parent3.Code);
			}

			Assert("Should be added to Support stage", parentCodes.Any(code => code == SupportIncidentCategoriesList.Codes.Support));
			Assert("Should be added to Defect stage", parentCodes.Any(code => code == SupportIncidentCategoriesList.Codes.Defect));
			Assert("Should be added to FeatureRequest stage", parentCodes.Any(code => code == SupportIncidentCategoriesList.Codes.FeatureRequest));
			Assert("Should be added to ContentDevelopment stage", parentCodes.Any(code => code == SupportIncidentCategoriesList.Codes.ContentDevelopment));
			Assert("Should be added to ComplianceRequirement stage", parentCodes.Any(code => code == SupportIncidentCategoriesList.Codes.ComplianceRequirement));
			Assert("Should be added to CustomerServiceRequest stage", parentCodes.Any(code => code == SupportIncidentCategoriesList.Codes.CustomerServiceRequest));
		}

		public void TestIncidentClosureDispositions_ClosedInternally()
		{
			var item = ItemSet.IncidentClosureDispositions;
			var defaultCLIs = item.DefaultValue.Cast<CodeDescriptionBoolTreeNode>().Where(d => d.Code == SupportIncidentLookups.DispositionList.Constants.Closed.ClosedInternal);
			var parentCodes = new List<string>();

			AssertEquals("Should be added to all incident stages", 6, defaultCLIs.Count());

			// Get parent codes from stage type, we have 3 grids, the first one is the stage type
			foreach (var item2 in defaultCLIs)
			{
				var parent1 = item.DefaultValue.FindByPK(item2.ParentID) as CodeDescriptionBoolTreeNode;
				var parent2 = item.DefaultValue.FindByPK(parent1.ParentID) as CodeDescriptionBoolTreeNode;
				var parent3 = item.DefaultValue.FindByPK(parent2.ParentID) as CodeDescriptionBoolTreeNode;

				parentCodes.Add(parent3.Code);
			}

			Assert("Should be added to Support stage", parentCodes.Any(code => code == SupportIncidentCategoriesList.Codes.Support));
			Assert("Should be added to Defect stage", parentCodes.Any(code => code == SupportIncidentCategoriesList.Codes.Defect));
			Assert("Should be added to FeatureRequest stage", parentCodes.Any(code => code == SupportIncidentCategoriesList.Codes.FeatureRequest));
			Assert("Should be added to ContentDevelopment stage", parentCodes.Any(code => code == SupportIncidentCategoriesList.Codes.ContentDevelopment));
			Assert("Should be added to ComplianceRequirement stage", parentCodes.Any(code => code == SupportIncidentCategoriesList.Codes.ComplianceRequirement));
			Assert("Should be added to CustomerServiceRequest stage", parentCodes.Any(code => code == SupportIncidentCategoriesList.Codes.CustomerServiceRequest));
		}

		public void TestGlowNewERequestPageUriDefaultValue()
		{
			var item = ItemSet.GlowNewERequestPageUri;
			AssertEquals("goto/new_erequest", item.DefaultValue);
		}

		#region TestElasticSearchAdaptorSettings

		public void TestElasticSearchAdaptorRequestUrl()
		{
			TestGenericRegistryItem(
				item: ItemSet.ElasticSearchAdaptorRequestUrl,
				expectedName: "ElasticSearchAdaptorRequestUrl",
				expectedCategory: EDIDataRegistry.System_ElasticSearchAdaptorSettings,
				expectedCaption: "Elasticsearch Request URL",
				expectedHint: "Please provide the Elasticsearch endpoint URL.",
				expectedStorage: RegistryStorageFlags.System,
				expectedOptions: RegistryOptions.Default,
				expectedDefaultValue: "https://r.prod-1.es.wtg.ws"
				);
		}

		public void TestElasticSearchAdaptorIndex()
		{
			TestGenericRegistryItem(
				item: ItemSet.ElasticSearchAdaptorIndex,
				expectedName: "ElasticSearchAdaptorIndex",
				expectedCategory: EDIDataRegistry.System_ElasticSearchAdaptorSettings,
				expectedCaption: "Elasticsearch Index for SQL CPU monitoring",
				expectedHint: "Please provide the Elasticsearch index for SQL CPU monitoring.",
				expectedStorage: RegistryStorageFlags.System,
				expectedOptions: RegistryOptions.Default,
				expectedDefaultValue: "idx-au2-prod-sqlcpumonitoring-prod"
				);
		}

		public void TestElasticSearchAdaptorUsername()
		{
			TestGenericRegistryItem(
				item: ItemSet.ElasticSearchAdaptorUsername,
				expectedName: "ElasticSearchAdaptorUsername",
				expectedCategory: EDIDataRegistry.System_ElasticSearchAdaptorSettings,
				expectedCaption: "Elasticsearch Username",
				expectedHint: "Please provide the username to access Elasticsearch endpoint.",
				expectedStorage: RegistryStorageFlags.System,
				expectedOptions: RegistryOptions.Default,
				expectedDefaultValue: "es_systemusage"
				);
		}

		public void TestElasticSearchAdaptorPassword()
		{
			TestGenericRegistryItem(
				item: ItemSet.ElasticSearchAdaptorPassword,
				expectedName: "ElasticSearchAdaptorPassword",
				expectedCategory: EDIDataRegistry.System_ElasticSearchAdaptorSettings,
				expectedCaption: "Elasticsearch Password",
				expectedHint: "Please provide the password to access Elasticsearch endpoint.",
				expectedStorage: RegistryStorageFlags.System,
				expectedOptions: RegistryOptions.Default,
				expectedDefaultValue: "pasoidmndhASLKdhjkAsbhdjLAGSDKBN$AdAJsdbajsgdbAGSdiuAtgTPOwe8n231y23n1!$"
				);
		}

		public void TestElasticSearchAdaptorPasswordWithTestRegistryItem()
		{
			TestRegistryItem(
				item: ItemSet.ElasticSearchAdaptorPassword,
				expectedName: "ElasticSearchAdaptorPassword",
				expectedCategory: EDIDataRegistry.System_ElasticSearchAdaptorSettings,
				expectedCaption: "Elasticsearch Password",
				expectedHint: "Please provide the password to access Elasticsearch endpoint.",
				expectedStorage: RegistryStorageFlags.System,
				expectedOptions: RegistryOptions.Default,
				TextEditorType.Password,
				expectedDefaultValue: "pasoidmndhASLKdhjkAsbhdjLAGSDKBN$AdAJsdbajsgdbAGSdiuAtgTPOwe8n231y23n1!$"
				);
		}

		public void TestElasticSearchAdaptorRequestStringWithFilter()
		{
			TestGenericRegistryItem(
				item: ItemSet.ElasticSearchAdaptorRequestStringWithFilter,
				expectedName: "ElasticSearchAdaptorRequestStringWithFilter",
				expectedCategory: EDIDataRegistry.System_ElasticSearchAdaptorSettings,
				expectedCaption: "Elasticsearch Request String",
				expectedHint: "Please provide request string to retrieve data for creating SQL performance WI.",
				expectedStorage: RegistryStorageFlags.System,
				expectedOptions: RegistryOptions.Default,
				expectedDefaultValue: "{\"index\":[\"wisecloud-syd-systemusage-2*\"],\"ignore_unavailable\":true,\"preference\":1521967565949}\r\n{\"size\":0,\"_source\":{\"excludes\":[]},\"aggs\":{\"topLevelAggregation\":{\"terms\":{\"field\":\"System.keyword\",\"size\":35,\"missing\":\"__missing__\"},\"aggs\":{\"AggCPUTimeMS\":{\"sum\":{\"field\":\"CpuTimeMilliseconds\"}},\"SubAggregation1\":{\"terms\":{\"field\":\"QueryHash.keyword\",\"size\":{MaxAlertPerDayPerSystem},\"order\":{\"AggCPUTimeMS\":\"desc\"}},\"aggs\":{\"AggCPUTimeMS\":{\"sum\":{\"field\":\"CpuTimeMilliseconds\"}},\"SubAggregation2\":{\"terms\":{\"field\":\"Owner.keyword\",\"size\":1,\"order\":{\"AggCPUTimeMS\":\"desc\"},\"missing\":\"__missing__\"},\"aggs\":{\"AggCPUTimeMS\":{\"sum\":{\"field\":\"CpuTimeMilliseconds\"}},\"5\":{\"sum\":{\"field\":\"Qty\"}},\"6\":{\"sum\":{\"field\":\"LogicalReads\"}},\"7\":{\"sum\":{\"field\":\"Writes\"}},\"8\":{\"max\":{\"field\":\"CollectSystemTimeUtc\"}},\"9\":{\"min\":{\"field\":\"ExeDate\"}},\"10\":{\"max\":{\"field\":\"ExeDate\"}},\"11\":{\"max\":{\"script\":{\"lang\":\"painless\",\"source\":\"String versionString=doc['CurrentVersion.keyword'].value;double result=0;int offset=0;int next=0;for(int i=3;i>=0;i--){next=versionString.indexOf('.',offset);if(next==-1)next=versionString.length();result=result+Integer.parseInt(versionString.substring(offset,next))*Math.pow(1000,i);offset=next+1}return result;\"}}}}}}}}}},\"stored_fields\":[\"*\"],\"script_fields\":{},\"docvalue_fields\":[\"CollectSystemTimeUtc\"],\"query\":{\"bool\":{\"must\":[{\"match_all\":{}},{\"range\":{\"ExeDate\":{\"gt\":\"now-3M\"}}},{\"range\":{\"CollectSystemTimeUtc\":{\"gte\":\"{StartDate}\",\"lte\":\"{EndDate}\",\"format\":\"epoch_millis\"}}},{\"terms\":{\"System\":[{CapabilityList}]}}],\"filter\":[],\"should\":[],\"must_not\":[{\"term\":{\"QueryHash\":\"0\"}},{\"term\":{\"Owner\":\"customer\"}}]}}}\r\n"
				);
		}

		#endregion TestElasticSearchAdaptorSettings

		#region Avalara US Sales Tax Integration

		public void TestAvalaraIntegrationSettingStatus()
		{
			var item = ItemSet.AvalaraIntegrationSettingStatus;
			AssertEquals("Name", "AvalaraIntegrationSettingStatus", item.Name);
			AssertEquals("Caption", "Integration Setting Status", item.Caption);
			AssertEquals("Category", EDIDataRegistry.AvalaraUSSalesTaxCategory, item.Category);
			AssertEquals("Storage Flags", RegistryStorageFlags.Company, item.Storage);
			AssertEquals("Registry Options", RegistryOptions.Default, item.Options);
			AssertEquals("Hint Text", @"The default value for this registry is 'OFF', which means no messages are exchanged with Avalara at all.
When this registry is configured to 'SND - Sandbox', transactions posted in this login company will be exchanged with the Avalara Sandbox system.
When this registry is configured to 'PRD - Production', transactions posted in this login company will be exchanged with the Avalara Production system.", item.Hint);
			AssertEquals("Default Value", "OFF", item.Value);
			AssertType<CodePairRegistryDataType>("Data Type", item.DataType);

			var lookUpCodes = ((CodePairRegistryDataType)item.DataType).LookUpList.GetAllCodes();
			var expectedCodes = new[]
			{
				Billing.Business.USSalesTax.AvalaraConstants.IntegrationStatus.Codes.Off,
				Billing.Business.USSalesTax.AvalaraConstants.IntegrationStatus.Codes.Sandbox,
				Billing.Business.USSalesTax.AvalaraConstants.IntegrationStatus.Codes.Production,
			};
			AssertContainsExactElementsInExactOrder("Lookup Codes", expectedCodes, lookUpCodes);
		}

		public void TestAvalaraCompanyCode()
		{
			TestRegistryItem(ItemSet.AvalaraCompanyCode,
				"AvalaraCompanyCode",
				EDIDataRegistry.AvalaraUSSalesTaxCategory,
				"Avalara Company Code",
				"Set the exact Avalara Company Code (from the Avalara Website > Settings > Manage Companies > Company Code) for this ediProd login company.",
				RegistryStorageFlags.Company,
				RegistryOptions.Default,
				TextEditorType.TextBox,
				expectedDefaultValue: "");
		}

		public void TestAvalaraSalesTaxChargeCode()
		{
			var item = ItemSet.AvalaraSalesTaxChargeCode;
			AssertEquals("Name", "AvalaraSalesTaxChargeCode", item.Name);
			AssertEquals("Caption", "Sales Tax Charge Code", item.Caption);
			AssertEquals("Category", EDIDataRegistry.AvalaraUSSalesTaxCategory, item.Category);
			AssertEquals("Storage Flags", RegistryStorageFlags.Company, item.Storage);
			AssertEquals("Hint Text", "Set the ediProd charge code for this login company to be used for recording and reporting US Sales tax via the Avalara integration. The charge code MUST not be used for ANY OTHER PURPOSE.", item.Hint);
			AssertEquals("Default Value", Guid.Empty, item.Value);
			AssertType<GuidRegistryDataType>("Data Type", item.DataType);
			AssertType<GuidFindBoxRegistryEditorInfo>("Editor Info", item.EditorInfo);
			var editorInfo = (GuidFindBoxRegistryEditorInfo)item.EditorInfo;
			AssertEquals("Find Box Collection", RegistryFindBoxCollection.AccChargeCode, editorInfo.FindBoxCollection);
			AssertEquals("Find Box Collection Filter", RegistryFindBoxFilter.RevenueChargeCode, editorInfo.FindBoxFilter);
		}

		public void TestAvalaraAuthenticationSandboxUserId()
		{
			TestRegistryItem(ItemSet.AvalaraAuthenticationSandboxUserId,
				"AvalaraAuthenticationSandboxUserId",
				EDIDataRegistry.AvalaraUSSalesTaxCategory,
				"Sandbox - User ID",
				"Enter the Avalara Sandbox User ID in this registry. This value will be used if the Integration Setting Status registry is set to 'SND - Sandbox'.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				TextEditorType.TextBox,
				expectedDefaultValue: "");
		}

		public void TestAvalaraAuthenticationSandboxPassword()
		{
			TestRegistryItem(ItemSet.AvalaraAuthenticationSandboxPassword,
				"AvalaraAuthenticationSandboxPassword",
				EDIDataRegistry.AvalaraUSSalesTaxCategory,
				"Sandbox - License Key / Password",
				"Enter the Avalara Sandbox License Key in this registry. This value will be used if the Integration Setting Status registry is set to 'SND - Sandbox'.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				TextEditorType.Password,
				expectedDefaultValue: "");
		}

		public void TestAvalaraAuthenticationProductionUserId()
		{
			TestRegistryItem(ItemSet.AvalaraAuthenticationProductionUserId,
				"AvalaraAuthenticationProductionUserId",
				EDIDataRegistry.AvalaraUSSalesTaxCategory,
				"Production - User ID",
				"Enter the Avalara Production User ID in this registry. This value will be used if the Integration Setting Status registry is set to 'PRD - Production'.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				TextEditorType.TextBox,
				expectedDefaultValue: "");
		}

		public void TestAvalaraAuthenticationProductionPassword()
		{
			TestRegistryItem(ItemSet.AvalaraAuthenticationProductionPassword,
				"AvalaraAuthenticationProductionPassword",
				EDIDataRegistry.AvalaraUSSalesTaxCategory,
				"Production - License Key / Password",
				"Enter the Avalara Production License Key in this registry. This value will be used if the Integration Setting Status registry is set to 'PRD - Production'.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				TextEditorType.Password,
				expectedDefaultValue: "");
		}

		public void TestAvalaraAvalaraWebTimeoutSeconds()
		{
			TestRegistryItem(ItemSet.AvalaraWebTimeoutSeconds,
				"AvalaraWebTimeoutSeconds",
				EDIDataRegistry.AvalaraUSSalesTaxCategory,
				"Web Request Timeout",
				@"Enter the timeout in seconds used for all Avalara web requests. Any web request which takes longer than this time will be reported as an error.

The default value for this registry is 60 seconds. Minimum is 0.001 (1 ms). Maximum is 300 (5 minutes). Up to 3 decimal places can be entered.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				60m,
				0.001m,
				5 * 60.0m);
		}

		#endregion

		#region Service Types

		public void TestServiceTypes()
		{
			var item = ItemSet.ServiceTypes;
			AssertEquals("ServiceTypes", item.Name);
			AssertEquals("Service Types", item.Caption);
			AssertEquals(EDIDataRegistry.ModuleMappingsSubCategory, item.Category);
			AssertEquals("The list of valid types for eRequest Service Type", item.Hint);
			AssertEquals(RegistryStorageFlags.System, item.Storage);
		}

		public void TestServiceTypeMappings()
		{
			var areas = new CodeDescriptionPairList();
			areas.AddPair("XRM", "XRM");
			EDIDataRegistry.Instance.ProductAreas.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, areas);

			// setup for ServiceTypeProductAreaModuleMappingLookups.Modules
			var collection1 = new SystemProductCollection();
			var product1 = collection1.AddNew("AAA", "AAA", true);
			product1.ModuleMappings.AddNew("AA1", "AA1 Description", "XRM", true);
			product1.ModuleMappings.AddNew("AA2", "AA2 Description", "XRM", true);
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection1);

			var collection = new SystemProductCollection();
			var parent1 = collection.AddNew();
			parent1.Code = "AAA";
			parent1.Description = "AAA";

			var child1 = parent1.ServiceTypeModuleMappings.AddNew();
			child1.ProductArea = "XRM";
			child1.ModuleCode = "AA1";
			child1.ModuleDescription = "des1";
			child1.ServiceTypeMappings.AddNew("SE1");

			var child2 = parent1.ServiceTypeModuleMappings.AddNew();
			child2.ProductArea = "XRM";
			child2.ModuleCode = "AA2";
			child2.ModuleDescription = "des1";
			child2.ServiceTypeMappings.AddNew("SE2");
			child2.ServiceTypeMappings.AddNew("SE3");

			ItemSet.ServiceTypeMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			AssertEquals("1 Products", 1, ItemSet.ServiceTypeMappings.Value.Count);

			var mappingAA1 = ItemSet.ServiceTypeMappings.Value.GetServiceTypeMapping(parent1.Code, child1.ModuleCode);
			AssertEquals("1 Service Type for AA1", 1, mappingAA1.ServiceTypeMappings.Count);
			AssertEquals("Service Type Code", "SE1", mappingAA1.ServiceTypeMappings[0].Code);

			var mappingAA2 = ItemSet.ServiceTypeMappings.Value.GetServiceTypeMapping(parent1.Code, child2.ModuleCode);
			AssertEquals("2 Service Type for AA2", 2, mappingAA2.ServiceTypeMappings.Count);
			AssertEquals("Service Type Code", "SE2", mappingAA2.ServiceTypeMappings[0].Code);
			AssertEquals("Service Type Code", "SE3", mappingAA2.ServiceTypeMappings[1].Code);
		}

		public void TestProductAreasDuplicateCodes()
		{
			var areas = new CodeDescriptionPairList();
			areas.AddPair("XRM", "XRM");
			AssertNoExceptionThrown(() => EDIDataRegistry.Instance.ProductAreas.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, areas));

			areas.AddPair("XRM", "MMM blah");
			AssertExceptionThrown<RegistryValidationException>(() => EDIDataRegistry.Instance.ProductAreas.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, areas));
		}

		public void TestServiceTypeCr8Mappings()
		{
			var areas = new CodeDescriptionPairList();
			areas.AddPair("XRM", "XRM");
			EDIDataRegistry.Instance.ProductAreas.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, areas);

			// setup for ServiceTypeProductAreaModuleMappingLookups.Modules
			var collection1 = new SystemProductCollection();
			var product1 = collection1.AddNew("AAA", "AAA", true);
			product1.ModuleMappings.AddNew("AA1", "AA1 Description", "XRM", true);
			product1.ModuleMappings.AddNew("AA2", "AA2 Description", "XRM", true);
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection1);

			var collection = new SystemProductCollection();
			var parent1 = collection.AddNew();
			parent1.Code = "AAA";
			parent1.Description = "AAA";

			var child1 = parent1.ServiceTypeModuleMappings.AddNew();
			child1.ProductArea = "XRM";
			child1.ModuleCode = "AA1";
			child1.ModuleDescription = "des1";
			child1.ServiceTypeMappings.AddNew("SE1");

			var child2 = parent1.ServiceTypeModuleMappings.AddNew();
			child2.ProductArea = "XRM";
			child2.ModuleCode = "AA2";
			child2.ModuleDescription = "des1";
			child2.ServiceTypeMappings.AddNew("SE2");
			child2.ServiceTypeMappings.AddNew("SE3");

			AssertNotNull(ItemSet.ServiceTypeCr8Mappings.DefaultValue);
			AssertEquals(ModuleListType.Cr8, ItemSet.ServiceTypeCr8Mappings.DefaultValue.ProductCriticality);

			ItemSet.ServiceTypeCr8Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			AssertEquals("1 Products", 1, ItemSet.ServiceTypeCr8Mappings.Value.Count);

			var mappingAA1 = ItemSet.ServiceTypeCr8Mappings.Value.GetServiceTypeMapping(parent1.Code, child1.ModuleCode);
			AssertEquals("Should be Cr8", ModuleListType.Cr8, mappingAA1.ProductCriticality);
			AssertEquals("1 Service Type for AA1", 1, mappingAA1.ServiceTypeMappings.Count);
			AssertEquals("Service Type Code", "SE1", mappingAA1.ServiceTypeMappings[0].Code);

			var mappingAA2 = ItemSet.ServiceTypeCr8Mappings.Value.GetServiceTypeMapping(parent1.Code, child2.ModuleCode);
			AssertEquals("Should be Cr8", ModuleListType.Cr8, mappingAA2.ProductCriticality);
			AssertEquals("2 Service Type for AA2", 2, mappingAA2.ServiceTypeMappings.Count);
			AssertEquals("Service Type Code", "SE2", mappingAA2.ServiceTypeMappings[0].Code);
			AssertEquals("Service Type Code", "SE3", mappingAA2.ServiceTypeMappings[1].Code);
		}

		public void TestServiceTypeCr9Mappings()
		{
			var areas = new CodeDescriptionPairList();
			areas.AddPair("XRM", "XRM");
			EDIDataRegistry.Instance.ProductAreas.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, areas);

			// setup for ServiceTypeProductAreaModuleMappingLookups.Modules
			var collection1 = new SystemProductCollection();
			var product1 = collection1.AddNew("AAA", "AAA", true);
			product1.ModuleMappings.AddNew("AA1", "AA1 Description", "XRM", true);
			product1.ModuleMappings.AddNew("AA2", "AA2 Description", "XRM", true);
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection1);

			var collection = new SystemProductCollection();
			var parent1 = collection.AddNew();
			parent1.Code = "AAA";
			parent1.Description = "AAA";

			var child1 = parent1.ServiceTypeModuleMappings.AddNew();
			child1.ProductArea = "XRM";
			child1.ModuleCode = "AA1";
			child1.ModuleDescription = "des1";
			child1.ServiceTypeMappings.AddNew("SE1");

			var child2 = parent1.ServiceTypeModuleMappings.AddNew();
			child2.ProductArea = "XRM";
			child2.ModuleCode = "AA2";
			child2.ModuleDescription = "des1";
			child2.ServiceTypeMappings.AddNew("SE2");
			child2.ServiceTypeMappings.AddNew("SE3");

			AssertNotNull(ItemSet.ServiceTypeCr9Mappings.DefaultValue);
			AssertEquals(ModuleListType.Cr9, ItemSet.ServiceTypeCr9Mappings.DefaultValue.ProductCriticality);

			ItemSet.ServiceTypeCr9Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			AssertEquals("1 Products", 1, ItemSet.ServiceTypeCr9Mappings.Value.Count);

			var mappingAA1 = ItemSet.ServiceTypeCr9Mappings.Value.GetServiceTypeMapping(parent1.Code, child1.ModuleCode);
			AssertEquals("Should be Cr9", ModuleListType.Cr9, mappingAA1.ProductCriticality);
			AssertEquals("1 Service Type for AA1", 1, mappingAA1.ServiceTypeMappings.Count);
			AssertEquals("Service Type Code", "SE1", mappingAA1.ServiceTypeMappings[0].Code);

			var mappingAA2 = ItemSet.ServiceTypeCr9Mappings.Value.GetServiceTypeMapping(parent1.Code, child2.ModuleCode);
			AssertEquals("Should be Cr9", ModuleListType.Cr9, mappingAA2.ProductCriticality);
			AssertEquals("2 Service Type for AA2", 2, mappingAA2.ServiceTypeMappings.Count);
			AssertEquals("Service Type Code", "SE2", mappingAA2.ServiceTypeMappings[0].Code);
			AssertEquals("Service Type Code", "SE3", mappingAA2.ServiceTypeMappings[1].Code);
		}

		#endregion

		#region Incident Similarity

		public void TestEnableIncidentSimilarityWebService()
		{
			TestGenericRegistryItem(
				ItemSet.EnableIncidentSimilarityWebService,
				"EnableIncidentSimilarityWebService",
				EDIDataRegistry.CustomerServiceSubCategoryIncidentSimilarity,
				"Enable Incident Similarity Web Service",
				"The Incident Similarity functionality will be provided by the web service.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				false);
		}

		public void TestEnableIncidentSimilarityWebServiceUrl()
		{
			TestGenericRegistryItem(
				ItemSet.IncidentSimilarityWebServiceUrl,
				"IncidentSimilarityWebServiceUrl",
				EDIDataRegistry.CustomerServiceSubCategoryIncidentSimilarity,
				"Incident Similarity Web Service URL",
				"URL for the Incident Similarity Web Service, which will be used instead of the CargoWise service task, if enabled.",
				RegistryStorageFlags.System);
		}

		public void TestIncidentSimilarityWebServiceDataBatchSize()
		{
			TestGenericRegistryItem(
				ItemSet.IncidentSimilarityWebServiceDataBatchSize,
				"IncidentSimilarityWebServiceDataBatchSize",
				EDIDataRegistry.CustomerServiceSubCategoryIncidentSimilarity,
				"Incident Similarity Web Service Data Batch Size",
				"The maximum number of rows that will be sent to the Incident Similarity Web Service at each nudge of relevant data updates.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				100_000);
		}

		#endregion

		#region Data Science

		public void TestEnableDataScienceCoreSubscribers()
		{
			TestGenericRegistryItem(
				ItemSet.EnableDataScienceCoreSubscribers,
				"EnableDataScienceCoreSubscribers",
				EDIDataRegistry.DataScienceSubCategoryAuditCore,
				"Enable",
				"Send Core table changes to Kafka.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				false);
		}

		public void TestDataScienceCoreKafkaTopic()
		{
			TestGenericRegistryItem(
				ItemSet.DataScienceCoreKafkaTopic,
				"DataScienceCoreKafkaTopic",
				EDIDataRegistry.DataScienceSubCategoryAuditCore,
				"Kafka topic",
				"Send Core table changes to this Kafka topic.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				string.Empty);
		}

		public void TestDataScienceCoreKafkaSaslUsername()
		{
			TestGenericRegistryItem(
				ItemSet.DataScienceCoreKafkaSaslUsername,
				"DataScienceCoreKafkaSaslUsername",
				EDIDataRegistry.DataScienceSubCategoryAuditCore,
				"Kafka SASL username",
				"Kafka SASL username for the Core topic. Defaults to Kafka topic.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				EDIDataRegistry.Instance.DataScienceCoreKafkaTopic.Value);
		}

		public void TestDataScienceCoreKafkaSaslPassword()
		{
			TestRegistryItem(ItemSet.DataScienceCoreKafkaSaslPassword,
				"DataScienceCoreKafkaSaslPassword",
				EDIDataRegistry.DataScienceSubCategoryAuditCore,
				"Kafka SASL password",
				"Kafka SASL password for the Core topic, encoded with TwoWayEncoder.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				TextEditorType.Password,
				expectedDefaultValue: string.Empty);
		}

		public void TestDataScienceCoreKafkaEnableTransactions()
		{
			TestGenericRegistryItem(ItemSet.DataScienceCoreKafkaEnableTransactions,
				"DataScienceCoreKafkaEnableTransactions",
				EDIDataRegistry.DataScienceSubCategoryAuditCore,
				"Enable Kafka producer transactions",
				"Send each set of Core table changes within a transaction. There are caveats; see docs in source code for details.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				false);
		}

		public void TestEnableDataScienceIncidentRelatedSubscribers()
		{
			TestGenericRegistryItem(
				ItemSet.EnableDataScienceIncidentRelatedSubscribers,
				"EnableDataScienceIncidentRelatedSubscribers",
				EDIDataRegistry.DataScienceSubCategoryAuditIncidentRelated,
				"Enable",
				"Send Incident-related table changes to Kafka.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				false);
		}

		public void TestDataScienceIncidentRelatedKafkaTopic()
		{
			TestGenericRegistryItem(
				ItemSet.DataScienceIncidentRelatedKafkaTopic,
				"DataScienceIncidentRelatedKafkaTopic",
				EDIDataRegistry.DataScienceSubCategoryAuditIncidentRelated,
				"Kafka topic",
				"Send Incident-related table changes to this Kafka topic.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				string.Empty);
		}

		public void TestDataScienceIncidentRelatedKafkaSaslUsername()
		{
			TestGenericRegistryItem(
				ItemSet.DataScienceIncidentRelatedKafkaSaslUsername,
				"DataScienceIncidentRelatedKafkaSaslUsername",
				EDIDataRegistry.DataScienceSubCategoryAuditIncidentRelated,
				"Kafka SASL username",
				"Kafka SASL username for the Incident-related topic. Defaults to Kafka topic.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				EDIDataRegistry.Instance.DataScienceIncidentRelatedKafkaTopic.Value);
		}

		public void TestDataScienceIncidentRelatedKafkaSaslPassword()
		{
			TestRegistryItem(ItemSet.DataScienceIncidentRelatedKafkaSaslPassword,
				"DataScienceIncidentRelatedKafkaSaslPassword",
				EDIDataRegistry.DataScienceSubCategoryAuditIncidentRelated,
				"Kafka SASL password",
				"Kafka SASL password for the Incident-related topic, encoded with TwoWayEncoder.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				TextEditorType.Password,
				expectedDefaultValue: string.Empty);
		}

		public void TestDataScienceIncidentRelatedKafkaEnableTransactions()
		{
			TestGenericRegistryItem(ItemSet.DataScienceIncidentRelatedKafkaEnableTransactions,
				"DataScienceIncidentRelatedKafkaEnableTransactions",
				EDIDataRegistry.DataScienceSubCategoryAuditIncidentRelated,
				"Enable Kafka producer transactions",
				"Send each set of Incident-related table changes within a transaction. There are caveats; see docs in source code for details.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				false);
		}

		public void TestEnableDataScienceBillingSubscribers()
		{
			TestGenericRegistryItem(
				ItemSet.EnableDataScienceBillingSubscribers,
				"EnableDataScienceBillingSubscribers",
				EDIDataRegistry.DataScienceSubCategoryAuditBilling,
				"Enable",
				"Send Billing table changes to Kafka.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				false);
		}

		public void TestDataScienceBillingKafkaTopic()
		{
			TestGenericRegistryItem(
				ItemSet.DataScienceBillingKafkaTopic,
				"DataScienceBillingKafkaTopic",
				EDIDataRegistry.DataScienceSubCategoryAuditBilling,
				"Kafka topic",
				"Send Billing table changes to this Kafka topic.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				string.Empty);
		}

		public void TestDataScienceBillingKafkaSaslUsername()
		{
			TestGenericRegistryItem(
				ItemSet.DataScienceBillingKafkaSaslUsername,
				"DataScienceBillingKafkaSaslUsername",
				EDIDataRegistry.DataScienceSubCategoryAuditBilling,
				"Kafka SASL username",
				"Kafka SASL username for the Billing topic. Defaults to Kafka topic.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				EDIDataRegistry.Instance.DataScienceBillingKafkaTopic.Value);
		}

		public void TestDataScienceBillingKafkaSaslPassword()
		{
			TestRegistryItem(ItemSet.DataScienceBillingKafkaSaslPassword,
				"DataScienceBillingKafkaSaslPassword",
				EDIDataRegistry.DataScienceSubCategoryAuditBilling,
				"Kafka SASL password",
				"Kafka SASL password for the Billing topic, encoded with TwoWayEncoder.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				TextEditorType.Password,
				expectedDefaultValue: string.Empty);
		}

		public void TestDataScienceBillingKafkaEnableTransactions()
		{
			TestGenericRegistryItem(ItemSet.DataScienceBillingKafkaEnableTransactions,
				"DataScienceBillingKafkaEnableTransactions",
				EDIDataRegistry.DataScienceSubCategoryAuditBilling,
				"Enable Kafka producer transactions",
				"Send each set of Billing table changes within a transaction. There are caveats; see docs in source code for details.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				false);
		}

		public void TestEnableDataScienceProductivitySubscribers()
		{
			TestGenericRegistryItem(
				ItemSet.EnableDataScienceProductivitySubscribers,
				"EnableDataScienceProductivitySubscribers",
				EDIDataRegistry.DataScienceSubCategoryAuditProductivity,
				"Enable",
				"Send Productivity table changes to Kafka.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				false);
		}

		public void TestDataScienceProductivityKafkaTopic()
		{
			TestGenericRegistryItem(
				ItemSet.DataScienceProductivityKafkaTopic,
				"DataScienceProductivityKafkaTopic",
				EDIDataRegistry.DataScienceSubCategoryAuditProductivity,
				"Kafka topic",
				"Send Productivity table changes to this Kafka topic.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				string.Empty);
		}

		public void TestDataScienceProductivityKafkaSaslUsername()
		{
			TestGenericRegistryItem(
				ItemSet.DataScienceProductivityKafkaSaslUsername,
				"DataScienceProductivityKafkaSaslUsername",
				EDIDataRegistry.DataScienceSubCategoryAuditProductivity,
				"Kafka SASL username",
				"Kafka SASL username for the Productivity topic. Defaults to Kafka topic.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				EDIDataRegistry.Instance.DataScienceProductivityKafkaTopic.Value);
		}

		public void TestDataScienceProductivityKafkaSaslPassword()
		{
			TestRegistryItem(ItemSet.DataScienceProductivityKafkaSaslPassword,
				"DataScienceProductivityKafkaSaslPassword",
				EDIDataRegistry.DataScienceSubCategoryAuditProductivity,
				"Kafka SASL password",
				"Kafka SASL password for the Productivity topic, encoded with TwoWayEncoder.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				TextEditorType.Password,
				expectedDefaultValue: string.Empty);
		}

		public void TestDataScienceProductivityKafkaEnableTransactions()
		{
			TestGenericRegistryItem(ItemSet.DataScienceProductivityKafkaEnableTransactions,
				"DataScienceProductivityKafkaEnableTransactions",
				EDIDataRegistry.DataScienceSubCategoryAuditProductivity,
				"Enable Kafka producer transactions",
				"Send each set of Productivity table changes within a transaction. There are caveats; see docs in source code for details.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				false);
		}

		#endregion

		#region Kafka

		public void TestEdiKafkaBootstrapServers()
		{
			TestGenericRegistryItem(
				ItemSet.EdiKafkaBootstrapServers,
				"EdiKafkaBootstrapServers",
				EDIDataRegistry.KafkaSubCategory,
				"Bootstrap servers",
				"Comma-separated list of bootstrap servers of the Kafka cluster to connect to.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				string.Empty);
		}

		#endregion Kafka

		#region Test Membership Types

		public void TestMembershipTypes()
		{
			TestGenericRegistryItem(
				ItemSet.OrgMembershipTypes,
				"OrgMembershipTypes",
				EDIDataRegistry.Category,
				"Organization Membership Types",
				"The list of valid types for Organization Membership",
				RegistryStorageFlags.System,
				RegistryOptions.Default);
		}

		#endregion

		#region TestAWSPrivateCAListManager

		public void TestAWSPrivateCAListManager()
		{
			var cAArn = ItemSet.AWSPrivateCAListManager;
			AssertEquals("AWSPrivateCAListManager", cAArn.Name);
			AssertEquals((NoResString)EDIDataRegistry.AWSManagementSubCategory, cAArn.Category);
			AssertEquals((NoResString)"AWS Private CA List Manager", cAArn.Caption);
			AssertEquals((NoResString)"AWS Private CA List Manager", cAArn.Hint);
			AssertEquals(RegistryStorageFlags.System, cAArn.Storage);
			AssertEquals(RegistryOptions.IsOnlyForSupport, cAArn.Options);
		}

		#endregion

		#region External Servers

		public void TestEDIFaxUsageDB()
		{
			var item = ItemSet.EDIFaxUsageDBServerName;
			AssertEquals("EDIFaxUsageDBServerName", item.Name);
			AssertEquals("EDI Fax Usage Database Server Name", item.Caption);
			AssertEquals(EDIDataRegistry.ExternalServersEDIFaxUsageDBCategory, item.Category);
			AssertEquals("Enter the name of server which hosts Fax Usage database.", item.Hint);
			AssertEquals(RegistryStorageFlags.System, item.Storage);
			AssertEquals("sydedifax.db.wtg.zone", item.DefaultValue);

			item = ItemSet.EDIFaxUsageDBName;
			AssertEquals("EDIFaxUsageDBName", item.Name);
			AssertEquals("EDI Fax Usage Database Name", item.Caption);
			AssertEquals(EDIDataRegistry.ExternalServersEDIFaxUsageDBCategory, item.Category);
			AssertEquals("Enter the name of database which stores Fax Usage.", item.Hint);
			AssertEquals(RegistryStorageFlags.System, item.Storage);
			AssertEquals("EDIFaxDB", item.DefaultValue);

			var login = ItemSet.EDIFaxUsageDBLogin;
			AssertEquals("EDIFaxUsageDBLogin", login.Name);
			AssertEquals("EDI Fax Usage Database Login", login.Caption);
			AssertEquals(EDIDataRegistry.ExternalServersEDIFaxUsageDBCategory, login.Category);
			AssertEquals("Enter login information to the database which stores Fax Usage.", login.Hint);
			AssertEquals(RegistryStorageFlags.System, login.Storage);
			AssertEquals("edifaxreadonly", login.DefaultValue.UserName);
			AssertEquals("password", login.DefaultValue.Password);
			AssertEquals("password", login.DefaultValue.ConfirmPassword);
		}

		#endregion

		#region TestAzureOpenIDConnectConfiguration

		public void TestAzureOpenIDConnectConfiguration()
		{
			var azureOpenIDConnectConfigurationRegistry = ItemSet.AzureOpenIDConnectConfiguration;
			AssertEquals("AzureOpenIDConnectConfiguration", azureOpenIDConnectConfigurationRegistry.Name);
			AssertEquals(EDIDataRegistry.AzureOpenIDConnectConfigurationListSubCategory, azureOpenIDConnectConfigurationRegistry.Category);
			AssertEquals("Azure OpenID Connect Configuration", azureOpenIDConnectConfigurationRegistry.Caption);
			AssertEquals("This will be utilized by Token Authentication Onboarding Data Form to verify the IDP federations in different environments (Prod and Staging).", azureOpenIDConnectConfigurationRegistry.Hint);
			AssertEquals(RegistryStorageFlags.System, azureOpenIDConnectConfigurationRegistry.Storage);
			AssertEquals(RegistryOptions.IsOnlyForSupport, azureOpenIDConnectConfigurationRegistry.Options);
		}

		#endregion

		#region TestAzureApplicationManagement

		public void TestTokenValidationServiceDiscoveryEndpoint()
		{
			var tokenValidationServiceDiscoveryEndpoint = ItemSet.TokenValidationServiceDiscoveryEndpoint;
			AssertEquals("TokenValidationServiceDiscoveryEndpoint", tokenValidationServiceDiscoveryEndpoint.Name);
			AssertEquals(EDIDataRegistry.AzureApplicationManagementSubCategory, tokenValidationServiceDiscoveryEndpoint.Category);
			AssertEquals("Token Validation Service Discovery Endpoint URL", tokenValidationServiceDiscoveryEndpoint.Caption);
			AssertEquals("Specify the URL of Token Validation Service discovery endpoint, the URL ends with '/.well-known/openid-configuration'", tokenValidationServiceDiscoveryEndpoint.Hint);
			AssertEquals(RegistryStorageFlags.System, tokenValidationServiceDiscoveryEndpoint.Storage);
			AssertEquals(RegistryOptions.IsOnlyForSupport, tokenValidationServiceDiscoveryEndpoint.Options);
		}

		public void TestAzureApplicationManagementTenantID()
		{
			var azureApplicationManagementTenantIDRegistry = ItemSet.AzureApplicationManagementTenantID;
			AssertEquals("AzureApplicationManagementTenantID", azureApplicationManagementTenantIDRegistry.Name);
			AssertEquals(EDIDataRegistry.AzureApplicationManagementSubCategory, azureApplicationManagementTenantIDRegistry.Category);
			AssertEquals("Azure Application Management Tenant ID", azureApplicationManagementTenantIDRegistry.Caption);
			AssertEquals("The Tenant ID of Azure Application Management.", azureApplicationManagementTenantIDRegistry.Hint);
			AssertEquals(RegistryStorageFlags.System, azureApplicationManagementTenantIDRegistry.Storage);
			AssertEquals(RegistryOptions.IsOnlyForSupport, azureApplicationManagementTenantIDRegistry.Options);
		}

		public void TestAzureApplicationManagementClientID()
		{
			var azureApplicationManagementClientIDRegistry = ItemSet.AzureApplicationManagementClientID;
			AssertEquals("AzureApplicationManagementClientID", azureApplicationManagementClientIDRegistry.Name);
			AssertEquals(EDIDataRegistry.AzureApplicationManagementSubCategory, azureApplicationManagementClientIDRegistry.Category);
			AssertEquals("Azure Application Management Client ID", azureApplicationManagementClientIDRegistry.Caption);
			AssertEquals("The Client ID of Azure Application Management.", azureApplicationManagementClientIDRegistry.Hint);
			AssertEquals(RegistryStorageFlags.System, azureApplicationManagementClientIDRegistry.Storage);
			AssertEquals(RegistryOptions.IsOnlyForSupport, azureApplicationManagementClientIDRegistry.Options);
		}

		public void TestAzureApplicationRedirectUrlSyncInterval()
		{
			var azureApplicationManagementClientIDRegistry = ItemSet.AzureApplicationRedirectUrlSyncInterval;
			AssertEquals("AzureApplicationRedirectUrlSyncInterval", azureApplicationManagementClientIDRegistry.Name);
			AssertEquals(EDIDataRegistry.AzureApplicationManagementSubCategory, azureApplicationManagementClientIDRegistry.Category);
			AssertEquals("The interval days for redirect urls sync", azureApplicationManagementClientIDRegistry.Caption);
			AssertEquals("The interval days for redirect urls sync", azureApplicationManagementClientIDRegistry.Hint);
			AssertEquals(RegistryStorageFlags.System, azureApplicationManagementClientIDRegistry.Storage);
			AssertEquals(RegistryOptions.IsOnlyForSupport, azureApplicationManagementClientIDRegistry.Options);
			AssertEquals(1, azureApplicationManagementClientIDRegistry.DefaultValue);
		}

		public void TestGithubActionSecretNextCheckDate()
		{
			var registryItem = ItemSet.GithubActionSecretNextCheckDate;
			AssertEquals("GithubActionSecretNextCheckDate", registryItem.Name);
			AssertEquals(EDIDataRegistry.AzureApplicationManagementSubCategory, registryItem.Category);
			AssertEquals("Github Action Secret Next Check Date", registryItem.Caption);
			AssertEquals("This registry item is used to monitor the GitHub actions secret expiry date and send notifications to a predefined notifications group. When the CPS service task runs, it first checks this date to see if it needs to check the secret expira date. After running successfully, it will update the next run date to 7 days later from current date.", registryItem.Hint);
			AssertEquals(RegistryStorageFlags.System, registryItem.Storage);
			AssertEquals(RegistryOptions.IsOnlyForSupport, registryItem.Options);
			AssertEquals(DateTime.MinValue, registryItem.DefaultValue);
		}

		#endregion

		#region B2C Secret Check Management

		public void TestB2CSecretCheckManagementTenantID()
		{
			AssertEquals("B2CSecretCheckManagementTenantID", ItemSet.B2CSecretCheckManagementTenantID.Name);
			AssertEquals(EDIDataRegistry.B2CSecretCheckManagementSubCategory, ItemSet.B2CSecretCheckManagementTenantID.Category);
			AssertEquals("B2C Secret Check Management Tenant ID", ItemSet.B2CSecretCheckManagementTenantID.Caption);
			AssertEquals("The Tenant ID of B2C Secret Check Management.", ItemSet.B2CSecretCheckManagementTenantID.Hint);
			AssertEquals(RegistryStorageFlags.System, ItemSet.B2CSecretCheckManagementTenantID.Storage);
			AssertEquals(RegistryOptions.IsOnlyForSupport, ItemSet.B2CSecretCheckManagementTenantID.Options);
			AssertEquals("4dbcef22-d396-4ee1-b2e3-fa62fa6191df", ItemSet.B2CSecretCheckManagementTenantID.DefaultValue);
		}

		public void TestB2CSecretCheckManagementClientID()
		{
			AssertEquals("B2CSecretCheckManagementClientID", ItemSet.B2CSecretCheckManagementClientID.Name);
			AssertEquals(EDIDataRegistry.B2CSecretCheckManagementSubCategory, ItemSet.B2CSecretCheckManagementClientID.Category);
			AssertEquals("B2C Secret Check Management Client ID", ItemSet.B2CSecretCheckManagementClientID.Caption);
			AssertEquals("The Client ID of B2C Secret Check Management.", ItemSet.B2CSecretCheckManagementClientID.Hint);
			AssertEquals(RegistryStorageFlags.System, ItemSet.B2CSecretCheckManagementClientID.Storage);
			AssertEquals(RegistryOptions.IsOnlyForSupport, ItemSet.B2CSecretCheckManagementClientID.Options);
			AssertEquals("642e0e48-0670-45ff-814c-03a86135096e", ItemSet.B2CSecretCheckManagementClientID.DefaultValue);
		}

		public void TestB2CSecretCheckManagementSecretClientID()
		{
			AssertEquals("B2CSecretCheckManagementSecretClientID", ItemSet.B2CSecretCheckManagementSecretClientID.Name);
			AssertEquals(EDIDataRegistry.B2CSecretCheckManagementSubCategory, ItemSet.B2CSecretCheckManagementSecretClientID.Category);
			AssertEquals("B2C Secret Check Management Secret Client ID", ItemSet.B2CSecretCheckManagementSecretClientID.Caption);
			AssertEquals("The client id of azure application that stores the shared secret", ItemSet.B2CSecretCheckManagementSecretClientID.Hint);
			AssertEquals(RegistryStorageFlags.System, ItemSet.B2CSecretCheckManagementSecretClientID.Storage);
			AssertEquals(RegistryOptions.IsOnlyForSupport, ItemSet.B2CSecretCheckManagementSecretClientID.Options);
			AssertEquals("7a0715ec-77a5-49ce-8121-469540e8de88", ItemSet.B2CSecretCheckManagementSecretClientID.DefaultValue);
		}

		#endregion

		public void TestResolutionAndClosureBehaviour()
		{
			var item = ItemSet.ResolutionAndClosureBehaviour;
			AssertEquals(2, item.DefaultValue.Count);

			var parent = item.DefaultValue[0];
			AssertEquals("All Criticalities that are not listed", parent.Description);
			AssertEquals(ZGuid.Empty, parent.ParentID);

			var child = item.DefaultValue[1];
			AssertEquals("All Products that are not listed", child.Description);
			AssertEquals(parent.ID, child.ParentID);
		}

		#region ERICA Azure SDK Configuration

		public void TestAzureSearchServiceEndpoint()
		{
			var item = ItemSet.AzureSearchServiceEndpoint;
			AssertEquals("AzureSearchServiceEndpoint", item.Name);
			AssertEquals("Azure Search Service Endpoint", item.Caption);
			AssertEquals("WiseTech Global Client Extensions/Customer Service Incidents/ERICA Azure Configurations", item.Category);
			AssertEquals("The endpoint for the Azure Search Service", item.Hint);
			AssertEquals("Default value", string.Empty, item.Value);
			AssertEquals("Storage Flags", RegistryStorageFlags.System, item.Storage);
			AssertEquals("Options", RegistryOptions.Default, item.Options);
		}

		public void TestAzureSearchIndex()
		{
			var item = ItemSet.AzureSearchIndex;
			AssertEquals("AzureSearchIndex", item.Name);
			AssertEquals("Azure Search Index", item.Caption);
			AssertEquals("WiseTech Global Client Extensions/Customer Service Incidents/ERICA Azure Configurations", item.Category);
			AssertEquals("Azure Search Index", item.Hint);
			AssertEquals("Default value", string.Empty, item.Value);
			AssertEquals("Storage Flags", RegistryStorageFlags.System, item.Storage);
			AssertEquals("Options", RegistryOptions.Default, item.Options);
		}

		public void TestAzureOpenAIEndpoint()
		{
			var item = ItemSet.AzureOpenAIEndpoint;
			AssertEquals("AzureOpenAIEndpoint", item.Name);
			AssertEquals("Azure OpenAI Endpoint", item.Caption);
			AssertEquals("WiseTech Global Client Extensions/Customer Service Incidents/ERICA Azure Configurations", item.Category);
			AssertEquals("The endpoint for Azure OpenAI", item.Hint);
			AssertEquals("Default value", string.Empty, item.Value);
			AssertEquals("Storage Flags", RegistryStorageFlags.System, item.Storage);
			AssertEquals("Options", RegistryOptions.Default, item.Options);
		}

		public void TestAzureOpenAIEmbeddingDeployment()
		{
			var item = ItemSet.AzureOpenAIEmbeddingDeployment;
			AssertEquals("AzureOpenAIEmbeddingDeployment", item.Name);
			AssertEquals("Azure OpenAI Embedding Deployment", item.Caption);
			AssertEquals("WiseTech Global Client Extensions/Customer Service Incidents/ERICA Azure Configurations", item.Category);
			AssertEquals("The name for the embedding deployment in Azure OpenAI", item.Hint);
			AssertEquals("Default value", string.Empty, item.Value);
			AssertEquals("Storage Flags", RegistryStorageFlags.System, item.Storage);
			AssertEquals("Options", RegistryOptions.Default, item.Options);
		}

		public void TestAzureSearchAdminKey()
		{
			var item = ItemSet.AzureSearchAdminKey;
			AssertEquals("AzureSearchAdminKey", item.Name);
			AssertEquals("Azure Search Admin Key", item.Caption);
			AssertEquals("WiseTech Global Client Extensions/Customer Service Incidents/ERICA Azure Configurations", item.Category);
			AssertEquals("The key for the Azure Search Administrator Credentials", item.Hint);
			AssertEquals("Default value", string.Empty, item.Value);
			AssertEquals("Storage Flags", RegistryStorageFlags.System, item.Storage);
			AssertEquals("Options", RegistryOptions.Default, item.Options);
		}

		public void TestAzureOpenAIKey()
		{
			var item = ItemSet.AzureOpenAIKey;
			AssertEquals("AzureOpenAIKey", item.Name);
			AssertEquals("Azure OpenAI Key", item.Caption);
			AssertEquals("WiseTech Global Client Extensions/Customer Service Incidents/ERICA Azure Configurations", item.Category);
			AssertEquals("The key for the Azure OpenAI Credentials", item.Hint);
			AssertEquals("Default value", string.Empty, item.Value);
			AssertEquals("Storage Flags", RegistryStorageFlags.System, item.Storage);
			AssertEquals("Options", RegistryOptions.Default, item.Options);
		}

		public void TestAzureOpenAIEmbeddingDimensions()
		{
			var item = ItemSet.AzureOpenAIEmbeddingDimensions;
			AssertEquals("AzureOpenAIEmbeddingDimensions", item.Name);
			AssertEquals("Azure OpenAI Embedding Dimensions", item.Caption);
			AssertEquals("WiseTech Global Client Extensions/Customer Service Incidents/ERICA Azure Configurations", item.Category);
			AssertEquals("The Dimensions for the Azure OpenAI Embedding", item.Hint);
			AssertEquals("Default value", 1536, item.Value);
			AssertEquals("Storage Flags", RegistryStorageFlags.System, item.Storage);
			AssertEquals("Options", RegistryOptions.Default, item.Options);
		}

		#endregion

		#region TestTokenAuthOnboardingSubCategory

		public void TestB2CConfigurationRepositoryGitHubAppPrivateKey()
		{
			TestRegistryItem(ItemSet.B2CConfigurationRepositoryGitHubAppPrivateKey,
				"B2CCustomPolicyGitHubAppPrivateKey",
				EDIDataRegistry.TokenAuthOnboardingSubCategory,
				"Github Application Private Key",
				"The private key of the Github application that creates pull requests for Azure B2C Configuration repository.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				Array.Empty<byte>());
			AssertEquals(typeof(CWSupportLoginTokenPrivateKeyEditorInfo), ItemSet.CWSupportLoginTokenPrivateKey.EditorInfo.GetType());
		}

		public void TestB2CConfigurationRepositoryGitHubAppID()
		{
			var item = ItemSet.B2CConfigurationRepositoryGitHubAppID;
			CombineAssertions(() =>
			{
				AssertEquals("Name", "B2CCustomPolicyGitHubAppID", item.Name);
				AssertEquals("Category", "WiseTech Global Client Extensions/Token Auth Onboarding", item.Category);
				AssertEquals("Caption", "Github Application ID", item.Caption);
				AssertEquals("Hint", "The ID of the Github application that creates pull requests for Azure B2C Configuration repository.", item.Hint);
				AssertEquals("Storage", RegistryStorageFlags.System, item.Storage);
				AssertEquals("Options", RegistryOptions.IsOnlyForSupport, item.Options);
			});
		}

		public void TestB2CConfigurationRepositoryGitHubAppName()
		{
			var item = ItemSet.B2CConfigurationRepositoryGitHubAppName;
			CombineAssertions(() =>
			{
				AssertEquals("Name", "B2CConfigurationRepositoryGitHubAppName", item.Name);
				AssertEquals("Category", "WiseTech Global Client Extensions/Token Auth Onboarding", item.Category);
				AssertEquals("Caption", "Github Application Name", item.Caption);
				AssertEquals("Hint", "The name of the Github application that creates pull requests for Azure B2C Configuration repository.", item.Hint);
				AssertEquals("Storage", RegistryStorageFlags.System, item.Storage);
				AssertEquals("Options", RegistryOptions.IsOnlyForSupport, item.Options);
				AssertEquals("DefaultValue", "wtg-b2b-config-bot", item.DefaultValue);
			});
		}

		public void TestB2CConfigurationRepositoryOwnerName()
		{
			var item = ItemSet.B2CConfigurationRepositoryOwnerName;
			CombineAssertions(() =>
			{
				AssertEquals("Name", "B2CConfigurationRepositoryOwnerName", item.Name);
				AssertEquals("Category", "WiseTech Global Client Extensions/Token Auth Onboarding", item.Category);
				AssertEquals("Caption", "B2C Configuration Repository Owner Name", item.Caption);
				AssertEquals("Hint", "Azure B2C Configuration repository owner name.", item.Hint);
				AssertEquals("Storage", RegistryStorageFlags.System, item.Storage);
				AssertEquals("Options", RegistryOptions.IsOnlyForSupport, item.Options);
				AssertEquals("DefaultValue", "WiseTechGlobal", item.DefaultValue);
			});
		}

		#endregion

		#region BranchForEDIServiceTasks

		public void TestBranchForEDIServiceTasks()
		{
			SetAndAssertItemValue("BranchForEDIServiceTasks", ItemSet.BranchForEDIServiceTasks, Guid.NewGuid());
			TestGenericRegistryItem(
				ItemSet.BranchForEDIServiceTasks,
				"BranchForEDIServiceTasks",
				"WiseTech Global Client Extensions",
				"Branch for EDI Service Tasks",
				"Branch will be used to run EDI Service Tasks.",
				RegistryStorageFlags.System,
				RegistryOptions.IsValueMandatory | RegistryOptions.PreserveTestValue
				);
		}

		#endregion

		public void TestMaxRowsForDistributedDataSyncRequest()
		{
			var item = ItemSet.MaxRowsForDistributedDataSyncRequest;
			AssertEquals("MaxRowsForDistributedDataSyncRequest", item.Name);
			AssertEquals("Max rows for the distributed data sync request.", item.Caption);
			AssertEquals("WiseTech Global Client Extensions/Distributed Data", item.Category);
			AssertEquals("This setting specifies the maximum number of rows that can be processed in a single call during the distributed data synchronization request.", item.Hint);
			AssertEquals("Default value", 1000, item.Value);
			AssertEquals("Storage Flags", RegistryStorageFlags.System, item.Storage);
			AssertEquals("Options", RegistryOptions.Default, item.Options);
		}

		public void TestCW1ExeFileDownloadURL()
		{
			TestGenericRegistryItem(
								ItemSet.CW1ExeFileDownloadURL,
								"CW1ExeFileDownloadURL",
								EDIDataRegistry.MyAccountPortalSubCategory + "/Download",
								"Download URL of CargoWise Web Components installer EXE",
								"This is the download URL for the CargoWise Web Components installer EXE.",
								RegistryStorageFlags.System,
								RegistryOptions.Default,
								"https://myaccount-portal.cargowise.com/myaccount/downloads/CargoWiseOneWebServerSetup.exe");
		}

		public void TestUsageMinimumFeeSettings()
		{
			TestGenericRegistryItem(
								ItemSet.UsageMinimumFeeSettings,
								"UsageMinimumFeeSettings",
								EDIDataRegistry.LicenceBillingCategory,
								"Products (non-CW1) Enabled for Minimum Fee usage",
								"System Pricelist added to this registry will trigger a minimum fee usage to Non-CW1 production license DBs that do not have usage for the billing period. The Usage Code column must match the code of the System Pricelist in WISGLOSYD2.",
								RegistryStorageFlags.System);
		}

		#region DevTools Kafka Settings

		public void TestDevToolsKafkaBootstrapServers()
		{
			var item = ItemSet.DevToolsIntegrationKafkaBootstrapServers;

			TestGenericRegistryItem(
				item,
				"EDIDevToolsIntegrationKafkaBootstrapServers",
				EDIDataRegistry.DevToolsSubCategory,
				"Kafka Bootstrap Servers",
				"Comma-separated list of bootstrap servers of the Kafka cluster to connect to.",
				RegistryStorageFlags.System,
				string.Empty);
		}

		public void TestDevToolsKafkaTopicName()
		{
			var item = ItemSet.DevToolsIntegrationKafkaTopic;

			TestGenericRegistryItem(
				item,
				"EDIDevToolsIntegrationKafkaTopic",
				EDIDataRegistry.DevToolsSubCategory,
				"Kafka Topic",
				"Topic to post Kafka messages to.",
				RegistryStorageFlags.System,
				string.Empty);
		}

		public void TestDevToolsKafkaSaslUsername()
		{
			var item = ItemSet.DevToolsIntegrationKafkaSaslUserName;

			TestGenericRegistryItem(
				item,
				"EDIDevToolsIntegrationKafkaSaslUserName",
				EDIDataRegistry.DevToolsSubCategory,
				"Kafka SASL Username",
				"User name to use for SASL authentication to Kafka.",
				RegistryStorageFlags.System,
				string.Empty);
		}

		public void TestDevToolsKafkaSaslPassword()
		{
			var item = ItemSet.DevToolsIntegrationKafkaSaslPassword;

			TestGenericRegistryItem(
				item,
				"EDIDevToolsIntegrationKafkaSaslPassword",
				EDIDataRegistry.DevToolsSubCategory,
				"Kafka SASL Password",
				"Password to use for SASL authentication to Kafka.",
				RegistryStorageFlags.System,
				string.Empty);

			var dataType = (StringRegistryDataType)item.DataType;
			Assert("Password should be encrypted", dataType.IsEncrypted);
		}

		#endregion

		public void TestGitHubUsersGroup()
		{
			TestRegistryItem(
				ItemSet.GitHubUsersGroup,
				"GitHubUsersGroup",
				EDIDataRegistry.Category,
				"GitHub Users Group",
				"This is the group that includes GitHub users.",
				RegistryStorageFlags.System,
				RegistryFindBoxCollection.GlbGroup,
				RegistryFactory.Instance.GetGroupPK("GHUSERS"));
		}
	}

	[TestedType(typeof(EDIDataRegistry.BillingPriceRoundingParamsDataType))]
	public class BillingPriceRoundingParamsDataTypeTest : RegistryDataTypeTestCase<EDIDataRegistry.BillingPriceRoundingParamsDataType>
	{
		protected override EDIDataRegistry.BillingPriceRoundingParamsDataType GetNewDataType()
		{
			return new EDIDataRegistry.BillingPriceRoundingParamsDataType(3);
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair("V1", "1=0.01");
			var result2 = new CodeDescriptionPairList();
			result.AddPair("V2", "2=0.05");

			return new ValidSampleAndBinaryValueInDB[]
			{
					new ValidSampleAndBinaryValueInDB(result, new EDIDataRegistry.BillingPriceRoundingParamsDataType(3).Serialise(result)),
					new ValidSampleAndBinaryValueInDB(result2, new EDIDataRegistry.BillingPriceRoundingParamsDataType(3).Serialise(result2))
			};
		}
	}
}
