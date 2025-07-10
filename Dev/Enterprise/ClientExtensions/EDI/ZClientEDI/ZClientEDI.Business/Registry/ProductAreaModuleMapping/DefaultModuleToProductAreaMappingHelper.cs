using System.Collections.Generic;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.CustomerService.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.Registry.Business
{
	public static class DefaultModuleToProductAreaMappingHelper
	{
		public static Dictionary<string, string> GetEnterpriseDefaultsModule()
		{
			var result = new Dictionary<string, string>();
			result.Add(ModuleTreeCustomerServiceMenuSectionList.Codes.Account, "FIN");
			result.Add(WebTrackerPreloadModulesList.Codes.Admin, "CSV");
			result.Add("ADV", "MDM"); // Address Cleansing
			result.Add(ModuleTreeCustomerServiceMenuSectionList.Codes.Alerts, "CSV");
			result.Add("APP", "PER"); // Application Deployment
			result.Add(ModuleTreeCustomerServiceMenuSectionList.Codes.ArchiveManager, "ARC");
			result.Add(ModuleTreeCustomerServiceMenuSectionList.Codes.AssetManagement, "FIN");
			result.Add("BAP", "BI"); // Business Intelligence - API
			result.Add("BAS", "BI"); // Business Intelligence - SSAS
			result.Add(AutoEnterpriseModuleList.Codes.BrokerAU, "BI");
			result.Add("BED", "BI"); // Business Intelligence - EDW
			result.Add("BIE", "BI"); // Business Intelligence - Enhancements
			result.Add("BIL", "XRM"); // WTG Automated Billing/Usage reports
			result.Add("BLU", "MDM"); // Blue Planet
			result.Add(ModuleTreeCustomerServiceMenuSectionList.Codes.BehaviorManagement, "PAV");
			result.Add("BNI", "BI"); // Business Intelligence - New Implementation
			result.Add("BOT", "BI"); // Business Intelligence - Other
			result.Add("BRE", "BI"); // Business Intelligence - Reporting
			result.Add(ModuleTreeCustomerServiceMenuSectionList.Codes.BufferManagement, "PAV");
			result.Add(ModuleTreeCustomerServiceMenuSectionList.Codes.Budgets, "FIN");
			result.Add(ModuleTreeCustomerServiceMenuSectionList.Codes.CustomsCa, "CUS");
			result.Add(ModuleTreeCustomerServiceMenuSectionList.Codes.CustomsCo, "CUS");
			result.Add(ModuleTreeCustomerServiceMenuSectionList.Codes.CashBooks, "FIN");
			result.Add("CCM", "RAT"); // Carrier Contract Management (WiseRates)
			result.Add(ModuleTreeCustomerServiceMenuSectionList.Codes.CustomsFiles, "CUS");
			result.Add(ModuleTreeCustomerServiceMenuSectionList.Codes.CfsCto, "INT");
			result.Add(ModuleTreeCustomerServiceMenuSectionList.Codes.CustomsGlobal, "CUS");
			result.Add(ModuleTreeCustomerServiceMenuSectionList.Codes.ComplianceWise, "MDM");
			result.Add(ModuleTreeCustomerServiceMenuSectionList.Codes.ClientRelationManagement, "SAL");
			result.Add("CST", "PAV"); // Customer Service Tickets (Module)
			result.Add(ModuleTreeCustomerServiceMenuSectionList.Codes.CcsUk, "CUS");
			result.Add(ModuleTreeCustomerServiceMenuSectionList.Codes.Customs, "CUS");
			result.Add("CYM", "INT"); // Container Yard
			result.Add("GDM", "DOM"); // Gate Management
			result.Add("DBO", "PER"); // Database Optimization
			result.Add(ModuleTreeCustomerServiceMenuSectionList.Codes.DocManager, "ARC");
			result.Add("DDS", "MDM"); // De duplication
			result.Add("DIS", "MDM"); // Distance Calculations
			result.Add(AutoEnterpriseModuleList.Codes.DocumentEngine, "ARC");
			result.Add(ModuleTreeCustomerServiceMenuSectionList.Codes.DomesticTransport, "TIT");
			result.Add("DPS", "MDM"); // Denied Party Screening
			result.Add("EAH", "AUT"); // eAdaptor HTTP+XML
			result.Add("EAS", "AUT"); // eAdaptor SOAP
			result.Add("ECO", "ECO"); // Ecommerce
			result.Add(ModuleTreeCustomerServiceMenuSectionList.Codes.EDIMessaging, "AUT");
			result.Add("EDM", "AUT"); // EDI Message
			result.Add("EDT", "BOR"); // ediTariff
			result.Add(ModuleTreeCustomerServiceMenuSectionList.Codes.EmailSection, "ARC");
			result.Add(ModuleTreeCustomerServiceMenuSectionList.Codes.ComPayIntegration, "FIN");
			result.Add("ENS", "MDM"); // Enrichment
			result.Add(ModuleTreeCustomerServiceMenuSectionList.Codes.EquipmentManagement, "INT");
			result.Add(ModuleTreeCustomerServiceMenuSectionList.Codes.EServices, "SVP");
			result.Add("ETL", "ECO"); // eTail
			result.Add(ModuleTreeCustomerServiceMenuSectionList.Codes.CustomsEu, "CUS");
			result.Add("EXM", "CCP"); // CargoWise Certification Exams
			result.Add(ModuleTreeCustomerServiceMenuSectionList.Codes.Forwarding, "INT");
			result.Add(ModuleTreeCustomerServiceMenuSectionList.Codes.CustomsGb, "CUS");
			result.Add(ModuleTreeCustomerServiceMenuSectionList.Codes.GlConsolidations, "FIN");
			result.Add(ModuleTreeCustomerServiceMenuSectionList.Codes.GeneralLedger, "FIN");
			result.Add("GLM", "FIN"); // GL Period Management Change
			result.Add("GLT", "INT"); // Container & AWB Automation
			result.Add(AutoEnterpriseModuleList.Codes.Hosting, "HOS");
			result.Add(ModuleTreeCustomerServiceMenuSectionList.Codes.PeopleOperations, "XRM");
			result.Add(ModuleTreeCustomerServiceMenuSectionList.Codes.EHubInterfaces, "SVC");
			result.Add("I&L", "CIL"); // Invoicing & Licensing - General Queries
			result.Add(AutoEnterpriseModuleList.Codes.IncidentManager, "XRM");
			result.Add("INS", "INS"); // Installation
			result.Add(AutoEnterpriseModuleList.Codes.InternalDevelopment, "XRM");
			result.Add(ModuleTreeCustomerServiceMenuSectionList.Codes.JobCosting, "RAT");
			result.Add("JRB", "FIN"); // Job Related Billing
			result.Add("JTS", "MDM"); // Job Titles
			result.Add(ModuleTreeCustomerServiceMenuSectionList.Codes.LearningDevelopment, "XRM");
			result.Add(AutoEnterpriseModuleList.Codes.Language, "LAN");
			result.Add(ModuleTreeCustomerServiceMenuSectionList.Codes.Locations, "ARC");
			result.Add("LDS", "LDS"); // Logistics Device
			result.Add(ModuleTreeCustomerServiceMenuSectionList.Codes.LocalTransport, "DTP");
			result.Add("LTN", "DTP"); // Land Transport
			result.Add(ModuleTreeCustomerServiceMenuSectionList.Codes.MasterData, "MDM");
			result.Add("MYA", "CSV"); // My Account Authentication/Credentials
			result.Add("NCN", "PAV"); // Planning, NCN and SST's
			result.Add(ModuleTreeCustomerServiceMenuSectionList.Codes.NCTS, "CUS");
			result.Add("NEO", "WEB"); // CargoWise Neo
			result.Add(ModuleTreeCustomerServiceMenuSectionList.Codes.Netting, "FIN");
			result.Add(ModuleTreeCustomerServiceMenuSectionList.Codes.OceanCarrier, "INT");
			result.Add(ModuleTreeCustomerServiceMenuSectionList.Codes.OrderManager, "INT");
			result.Add("OTH", "ARC"); // Other
			result.Add(ModuleTreeCustomerServiceMenuSectionList.Codes.Payables, "FIN");
			result.Add("PCG", "PRO"); // PAVE Productivity Configuration
			result.Add("PNS", "MDM"); // Phone Number Standards
			result.Add(ModuleTreeCustomerServiceMenuSectionList.Codes.WorkflowManager, "AUT");
			result.Add("PRT", "PAV"); // Productivity Tools
			result.Add(ModuleTreeCustomerServiceMenuSectionList.Codes.PrintingSection, "ARC");
			result.Add(ModuleTreeCustomerServiceMenuSectionList.Codes.Receivables, "FIN");
			result.Add(ModuleTreeCustomerServiceMenuSectionList.Codes.Recruitment, "HRM");
			result.Add(ModuleTreeCustomerServiceMenuSectionList.Codes.Recruiter, "HRM");
			result.Add(ModuleTreeCustomerServiceMenuSectionList.Codes.ReferenceFiles, "ARC");
			result.Add("REG", "ARC"); // Registry
			result.Add(ArchitectureModuleList.Codes.REP, "DMM");
			result.Add(ModuleTreeCustomerServiceMenuSectionList.Codes.ReportingBooks, "FIN");
			result.Add(ModuleTreeCustomerServiceMenuSectionList.Codes.ReportSection, "ARC");
			result.Add("RTU", "DPW"); // Real Time Universal Shipment
			result.Add(ModuleTreeCustomerServiceMenuSectionList.Codes.SalesMarketing, "SAL");
			result.Add("SAR", "CUS"); // SARS
			result.Add(ModuleTreeCustomerServiceMenuSectionList.Codes.Schedules, "INT");
			result.Add(ModuleTreeCustomerServiceMenuSectionList.Codes.LinerAndAgency, "INT");
			result.Add("STM", "HRM"); // Staff and Resources
			result.Add("SVT", "ARC"); // Service Tasks / Process Controllers
			result.Add(ModuleTreeCustomerServiceMenuSectionList.Codes.System, "ARC");
			result.Add(ModuleTreeCustomerServiceMenuSectionList.Codes.TariffsRates, "RAT");
			result.Add(ModuleTreeCustomerServiceMenuSectionList.Codes.TransportBooking, "DTP");
			result.Add("TEL", "EMB"); // Telematics
			result.Add(AutoEnterpriseModuleList.Codes.Training, "XRM");
			result.Add(ModuleTreeCustomerServiceMenuSectionList.Codes.TransitWarehouse, "DTW");
			result.Add(AutoEnterpriseModuleList.Codes.UpgradeAssurance, "ARC");
			result.Add("UPN", "ARC"); // Update Notes
			result.Add("USA", "ARC"); // Update Notes
			result.Add(ModuleTreeCustomerServiceMenuSectionList.Codes.CustomsUs, "CUS");
			result.Add("VAL", "SAL"); // Value Analysis
			result.Add("WAM", "PER"); // WiseCloud Automated Monitoring
			result.Add(ModuleTreeCustomerServiceMenuSectionList.Codes.Warehouse, "DPW");
			result.Add(ArchitectureModuleList.Codes.WEB, "WEB");
			result.Add("WFT", "DM"); // Workflow Templates
			result.Add(ModuleTreeCustomerServiceMenuSectionList.Codes.WorkflowAndProcesses, "AUT");
			result.Add(ModuleTreeCustomerServiceMenuSectionList.Codes.WiseRates, "RAT");
			result.Add(ModuleTreeCustomerServiceMenuSectionList.Codes.EXDOC, "CUS");
			result.Add("XNA", "AUT"); // Native XML
			result.Add("XUN", "AUT"); // Universal XML
			return result;
		}

		public static Dictionary<string, string> GetCr8DefaultsModule()
		{
			var result = new Dictionary<string, string>();
			result.Add(Cr8ModuleList.Codes.GeneralAccountingReportingRegulatory, "FIN");
			result.Add(Cr8ModuleList.Codes.CustomsCompliance, "CUS");
			result.Add(Cr8ModuleList.Codes.CarbonEnvironmentalCompliance, "GEO");
			result.Add(Cr8ModuleList.Codes.CarrierSupplierTariffRateUploadMapping, "RAT");
			result.Add(Cr8ModuleList.Codes.DangerousGoodsManagement, "GEO");
			result.Add(Cr8ModuleList.Codes.LocalCountryDocumentOrForm, "GEO");
			result.Add(Cr8ModuleList.Codes.OtherComplianceIssue, "GEO");
			result.Add(Cr8ModuleList.Codes.ReferenceDataBaseData, "ARC");
			result.Add(Cr8ModuleList.Codes.SupplyChainSecurity, "GEO");
			result.Add(Cr8ModuleList.Codes.TranslationIssueUiWebDocs, "LAN");
			result.Add(Cr8ModuleList.Codes.VatComplianceIssue, "FIN");
			result.Add(Cr8ModuleList.Codes.WithholdingTaxComplianceIssue, "FIN");
			return result;
		}

		public static Dictionary<string, string> GetCr9DefaultsModule()
		{
			var result = new Dictionary<string, string>();
			result.Add(WebTrackerPreloadModulesList.Codes.Admin, "CSV");
			result.Add(Cr9ModuleList.Codes.AccountingDataTakeOn, "FIN");
			result.Add("ADW", "INS"); // Advanced Data Automation Wizard activation
			result.Add("BIR", "BI"); // Business Intelligence - General
			result.Add("CAH", "SVP"); // Consulting/Assistance on eHub Configurations
			result.Add(Cr9ModuleList.Codes.ConsultingAssistanceOnAccountingProcedures, "FIN");
			result.Add("CDA", "INS"); // Copy Data to Non-Production System
			result.Add(Cr9ModuleList.Codes.ConsultingAssistanceOnDocbuilderCustomization, "ARC");
			result.Add(Cr9ModuleList.Codes.ConsultingAssistanceOnDataWizard, "ARC");
			result.Add(Cr9ModuleList.Codes.ConsultingAssistanceOnEadaptor, "SVC");
			result.Add("CFM", "RAT"); // Carrier Fees and Charges Management (WiseRates)
			result.Add(Cr9ModuleList.Codes.ConsultingAssistanceOnRating, "RAT");
			result.Add(Cr9ModuleList.Codes.ConsultingAssistanceOnReportCustomization, "ARC");
			result.Add(Cr9ModuleList.Codes.ConsultingAssistanceOnWorkflowManager, "AUT");
			result.Add("DBO", "PER"); // Database Optimization
			result.Add("ECO", "ECO"); // Ecommerce
			result.Add("EEC", "ECO"); // Enable Ecommerce
			result.Add(AutoEnterpriseModuleList.Codes.Hosting, "HOS");
			result.Add("I&L", "CIL"); // Invoicing & Licensing - General Queries
			result.Add(Cr9ModuleList.Codes.LogisticsDevice, "LDS");
			result.Add(Cr9ModuleList.Codes.LicensingMaintenanceUpdates, "CIL");
			result.Add(Cr9ModuleList.Codes.MasterDataTakeOn, "ARC");
			result.Add("MES", "PAV"); // Acceptability Band Measurement
			result.Add(Cr9ModuleList.Codes.MessagingConfiguration, "INS");
			result.Add("MYA", "CSV"); // My Account Authentication/Credentials
			result.Add("NEW", "SCH"); // New Team Setup
			result.Add(Cr9ModuleList.Codes.OtherConsultingPleaseDescribeClearly, "ARC");
			result.Add(AutoEnterpriseModuleList.Codes.Organisations, "CSV");
			result.Add("PAV", "CSV"); // PAVE Consulting
			result.Add("PCG", "SCH"); // PAVE Productivity Configuration
			result.Add(ModuleTreeCustomerServiceMenuSectionList.Codes.WorkflowManager, "DM");
			result.Add("RCA", "CSV"); // Root Cause Analysis
			result.Add(ModuleTreeCustomerServiceMenuSectionList.Codes.Recruiter, "HRM");
			result.Add("REP", "DMM"); // CW1 Reports
			result.Add(Cr9ModuleList.Codes.ReopenClosedGlPeriod, "FIN");
			result.Add(WebTrackerPreloadModulesList.Codes.Reports, "CCP");
			result.Add("RTU", "DPW"); // CW1 Reports
			result.Add(ModuleTreeCustomerServiceMenuSectionList.Codes.SalesMarketing, "XRM");
			result.Add(Cr9ModuleList.Codes.SelfHostedSqlInfrastructureUpdateAssistance, "INS");
			result.Add("STM", "HRM"); // Staff Management
			result.Add("UXD", "UXD"); // User Experience Design
			result.Add(Cr9ModuleList.Codes.WisecloudLoginAndOrPasswordRequests, "INS");
			result.Add(Cr9ModuleList.Codes.WisecloudOtherRequests, "HOS");
			result.Add("WFT", "DM"); // Workflow Templates
			result.Add("WPO", "WEB"); // Web Portals
			return result;
		}
	}
}
