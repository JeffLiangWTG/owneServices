using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Data;
using Enterprise.Core;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Core.Constants;
#if DEBUG
using CargoWise.Common.Testing;
#endif

namespace Enterprise.ZArchitecture.Modules
{
	#region SuppressResourceStringsCheckRegion

	public static class ModuleIDs
	{
		#region AllExcludingClientModules

		public static IEnumerable<ModuleIdentifier> AllExcludingClientModules
		{
			get
			{
				if (fAll == null)
				{
					fAll = ((ModuleIdentifier[])ModuleIDLoader.GetModuleIDs(typeof(ModuleIDs), typeof(ModuleIdentifier)))
						.Concat(ModuleListingSubsetRegister.GetRegisteredModuleIdentifiers())
						.ToArray();
				}
				return fAll;
			}
		}

#if DEBUG
		public static void ResetModuleIDs()
		{
			fAll = null;
		}

		[SuppressThreadStaticFieldMessage]
#endif

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule", Justification = "Suppress the warning.")]
		static ModuleIdentifier[] fAll;

		#endregion

		#region AllIncludingClientModules

		public static IEnumerable<ModuleIdentifier> AllIncludingClientModules
		{
			get
			{
				var clientModules = ClientHookLoader.Instance.ClientHook?.NewClientModules;
				return clientModules == null ? AllExcludingClientModules : AllExcludingClientModules.Concat(clientModules.Select(x => x.ID));
			}
		}

		#endregion

		#region Module IDs

		public static readonly ModuleIdentifier NotAssigned = new ModuleIdentifier(ModuleId.NotAssigned, (NoResString)"NotAssigned");

		public static readonly ModuleIdentifier JobRequiredDocumentAddInfo = new ModuleIdentifier(ModuleId.JobRequiredDocumentAddInfo, ResString.GetMultilingualString("ModuleId.DIS", "Document Image System"));
		public static readonly ModuleIdentifier GenCustomAddOnRule = new OrgModuleIdentifier(ModuleId.GenCustomAddOnRule, ResString.GetMultilingualString("Module.GenCustomAddOnRule", "Add On Rules"));

		public static readonly ModuleIdentifier ReviewProcess = new ModuleIdentifier(ModuleId.ReviewProcess, ResString.GetMultilingualString("Module.ReviewProcess", "Review Process"));
		public static readonly ModuleIdentifier ReviewProcessNode = new ModuleIdentifier(ModuleId.ReviewProcessNode, ResString.GetMultilingualString("Module.ReviewProcessNode", "Review Process Nodes"));

		public static readonly ModuleIdentifier Organisation = new OrgModuleIdentifier(ModuleId.Organisation, ResString.GetMultilingualString("Module.Organisation", "Organization"));
		public static readonly ModuleIdentifier ClientIntelligence = new OrgModuleIdentifier(ModuleId.ClientIntelligence, ResString.GetMultilingualString("Module.ClientIntelligence", "Client Intelligence"));
		public static readonly ModuleIdentifier CompetitorIntelligence = new OrgModuleIdentifier(ModuleId.CompetitorIntelligence, ResString.GetMultilingualString("Module.CompetitorIntelligence", "Competitor Intelligence"));

		public static readonly ModuleIdentifier StaffAssignments = new ModuleIdentifier(ModuleId.StaffAssignments, ResString.GetMultilingualString("Module.StaffAssignments", "Staff Assignments"));
		public static readonly ModuleIdentifier OrgCollectionCalls = new ModuleIdentifier(ModuleId.OrgCollectionCalls, ResString.GetMultilingualString("Module.OrgCollectionCalls", "Collection Calls"));
		public static readonly ModuleIdentifier Orders = new ModuleIdentifier(ModuleId.Orders, ResString.GetMultilingualString("Module.Orders", "Orders"), ResString.GetMultilingualString("Module.OrdersExtended", "Orders (Order Manager)"));
		public static readonly ModuleIdentifier JobShipmentPreplanning = new ModuleIdentifier(ModuleId.JobShipmentPreplanning, ResString.GetMultilingualString("Module.JobShipmentPreplanning", "Shipment Pre Advice"));
		public static readonly ModuleIdentifier ClientRates = new ModuleIdentifier(ModuleId.ClientRates, ResString.GetMultilingualString("Module.ClientRates", "Client Rates"));
		public static readonly ModuleIdentifier GlobalRates = new ModuleIdentifier(ModuleId.GlobalRates, ResString.GetMultilingualString("Module.GlobalRates", "Company Tariffs"));
		public static readonly ModuleIdentifier UniversalChargeCode = new ModuleIdentifier(ModuleId.UniversalChargeCode, ResString.GetMultilingualString("Module.UniversalChargeCode", "Universal Charge Codes"));
		public static readonly ModuleIdentifier CarrierChargeCode = new ModuleIdentifier(ModuleId.CarrierChargeCode, ResString.GetMultilingualString("Module.CarrierChargeCode", "Carrier Charge Codes"));
		public static readonly ModuleIdentifier UniversalCommodityCode = new ModuleIdentifier(ModuleId.UniversalCommodityCode, ResString.GetMultilingualString("Module.UniversalCommodityCode", "Universal Commodity Codes"));
		public static readonly ModuleIdentifier UrsNamedAccount = new ModuleIdentifier(ModuleId.UrsNamedAccount, ResString.GetMultilingualString("Module.UrsNamedAccount", "URS Named Accounts"));
		public static readonly ModuleIdentifier Quotations = new ModuleIdentifier(ModuleId.Quotations, ResString.GetMultilingualString("Module.Quotations", "Quotations"));    // used to use registry
		public static readonly ModuleIdentifier Costing = new ModuleIdentifier(ModuleId.Costing, ResString.GetMultilingualString("Module.Costing", "Costing"));
		public static readonly ModuleIdentifier IntercompanyTariffs = new ModuleIdentifier(ModuleId.IntercompanyTariffs, ResString.GetMultilingualString("Module.IntercompanyTariffs", "Intercompany Tariffs"));
		public static readonly ModuleIdentifier WiseRates = new ModuleIdentifier(ModuleId.WiseRates, ResString.GetMultilingualString("Module.WiseRatesSearch", "Multimodal Rates"));
		public static readonly ModuleIdentifier WiseRatesCargoguide = new ModuleIdentifier(ModuleId.WiseRatesCargoguide, ResString.GetMultilingualString("Module.WiseRatesCargoguide", "Cargoguide - Air Rates"));
		public static readonly ModuleIdentifier WiseRatesCargoSphere = new ModuleIdentifier(ModuleId.WiseRatesCargoSphere, ResString.GetMultilingualString("Module.WiseRatesCargoSphere", "CargoSphere - Ocean Rates"));
		public static readonly ModuleIdentifier CarrierConnect = new ModuleIdentifier(ModuleId.CarrierConnect, ResString.GetMultilingualString("Module.CarrierConnect", "CarrierConnect"));
		public static readonly ModuleIdentifier ProfitShare = new ModuleIdentifier(ModuleId.ProfitShare, ResString.GetMultilingualString("Module.ProfitShare", "Profit Share Agreements"));
		public static readonly ModuleIdentifier TransactionsExport = new ModuleIdentifier(ModuleId.TransactionsExport, ResString.GetMultilingualString("Module.TransactionsExport", "Export Transactions"));
		public static readonly ModuleIdentifier XmlTransactionsImport = new ModuleIdentifier(ModuleId.XmlTransactionsImport, ResString.GetMultilingualString("Module.XmlTransactionsImport", "Import XML Transactions"));
		public static readonly ModuleIdentifier CsvTransactionsImport = new ModuleIdentifier(ModuleId.CsvTransactionsImport, ResString.GetMultilingualString("Module.CsvTransactionsImport", "Import CSV Transactions"));
		public static readonly ModuleIdentifier CsvAccountsImport = new ModuleIdentifier(ModuleId.CsvAccountsImportModule, ResString.GetMultilingualString("Module.CsvAccountsImportModule", "Import Chart of Accounts"));
		public static readonly ModuleIdentifier OustandingJournalsExport = new ModuleIdentifier(ModuleId.OustandingJournalsExport, ResString.GetMultilingualString("Module.OustandingJournalsExport", "System Merge Accounts - Export"));
		public static readonly ModuleIdentifier OustandingJournalsImport = new ModuleIdentifier(ModuleId.OustandingJournalsImport, ResString.GetMultilingualString("Module.OustandingJournalsImport", "System Merge Accounts - Import"));

		public static readonly ModuleIdentifier DialogDefault = new ModuleIdentifier(ModuleId.DialogDefault, ResString.GetMultilingualString("Module.DialogDefaultOptions", "Dialog Default Options"));

		public static readonly ModuleIdentifier APPaymentProcessing = new ModuleIdentifier(ModuleId.APPaymentProcessing, ResString.GetMultilingualString("Module.APPaymentProcessing", "Payment Processing"), ResString.GetMultilingualString("Module.APPaymentProcessingExtended", "Payment Processing (AP)"));
		public static readonly ModuleIdentifier ARPaymentProcessing = new ModuleIdentifier(ModuleId.ARPaymentProcessing, ResString.GetMultilingualString("Module.ARPaymentProcessing", "Payment Processing"), ResString.GetMultilingualString("Module.ARPaymentProcessingExtended", "Payment Processing (AR)"));
		public static readonly ModuleIdentifier JobManagement = new ModuleIdentifier(ModuleId.JobManagement, ResString.GetMultilingualString("Module.JobManagement", "Job Management"));
		public static readonly ModuleIdentifier BulkDSBJobCloseBatchApproval = new ModuleIdentifier(ModuleId.BulkDSBJobCloseBatchApproval, ResString.GetMultilingualString("Module.BulkDSBJobCloseBatchApproval", "Disbursement Job Close Batch Approval"));
		public static readonly ModuleIdentifier WIPAccruals = new ModuleIdentifier(ModuleId.WIPAccruals, ResString.GetMultilingualString("Module.WIPAccruals", "WIPs and Accruals"));
		public static readonly ModuleIdentifier JobRevenueJournal = new ModuleIdentifier(ModuleId.JobRevenueJournal, ResString.GetMultilingualString("Module.JobRevenueJournal", "Job Revenue Journals"));
		public static readonly ModuleIdentifier GLJournal = new ModuleIdentifier(ModuleId.GLJournal, ResString.GetMultilingualString("Module.GLJournal", "Journals"));
		public static readonly ModuleIdentifier GLJournalApproval = new ModuleIdentifier(ModuleId.GLJournalApproval, ResString.GetMultilingualString("Module.GLJournalApproval", "Journals Awaiting Approval"));
		public static readonly ModuleIdentifier GLConsolidationGroups = new ModuleIdentifier(ModuleId.GLConsolidationGroups, ResString.GetMultilingualString("Module.GLConsolidations", "Consolidation Groups"));
		public static readonly ModuleIdentifier GLReportingBooksReport = new ModuleIdentifier(ModuleId.GLRepBooksReport, ResString.GetMultilingualString("Module.GLReportingBooksReport", "Reports"), ResString.GetMultilingualString("Module.GLReportingBooksReportExtended", "Reports (Reporting Books)"));
		public static readonly ModuleIdentifier ServiceLevel = new ModuleIdentifier(ModuleId.ServiceLevel, ResString.GetMultilingualString("Module.ServiceLevel", "Service Levels"));
		public static readonly ModuleIdentifier Opportunity = new ModuleIdentifier(ModuleId.Opportunity, ResString.GetMultilingualString("Module.Opportunity", "Opportunity Management"));
		public static readonly ModuleIdentifier CrmOpportunity = new ModuleIdentifier(ModuleId.CrmOpportunity, ResString.GetMultilingualString("Module.CrmOpportunity", "CRM Opportunity Management"));
		public static readonly ModuleIdentifier RefVessel = new ModuleIdentifier(ModuleId.RefVessel, ResString.GetMultilingualString("Module.RefVessel", "Vessels"));
		public static readonly ModuleIdentifier RefVesselZZ = new ModuleIdentifier(ModuleId.RefVesselZZ, ResString.GetMultilingualString("Module.RefVesselZZ", "Global Vessels"));
		public static readonly ModuleIdentifier GLAccountFormat = new ModuleIdentifier(ModuleId.GLAccountFormat, ResString.GetMultilingualString("Module.GLAccountFormat", "GL Account Format"));
		public static readonly ModuleIdentifier Containers = new ModuleIdentifier(ModuleId.Containers, ResString.GetMultilingualString("Module.Containers", "Containers"));
		public static readonly ModuleIdentifier MarketIntelligenceAndAnalytics = new ModuleIdentifier(ModuleId.MarketIntelligenceAndAnalytics, ResString.GetMultilingualString("Module.MarketIntelligenceAndAnalytics", "Market Intelligence and Analytics"));
		public static readonly ModuleIdentifier OceanCarrierBookingAnalysisReport = new ModuleIdentifier(ModuleId.OceanCarrierBookingAnalysisReport, ResString.GetMultilingualString("Module.OceanCarrierBookingAnalysisReport", "Ocean Carrier Booking Analysis Report"));
		public static readonly ModuleIdentifier ServiceRequest = new ModuleIdentifier(ModuleId.ServiceRequest, ResString.GetMultilingualString("Module.ServiceRequest", "Customer Service (Legacy)"));
		public static readonly ModuleIdentifier QuotedBookings = new ModuleIdentifier(ModuleId.QuotedBookings, ResString.GetMultilingualString("Module.QuotedBookings", "Bookings"));
		public static readonly ModuleIdentifier OneOffQuotes = new ModuleIdentifier(ModuleId.OneOffQuotes, ResString.GetMultilingualString("Module.OneOffQuotes", "One Off Quotes"));
		public static readonly ModuleIdentifier JobShipment = new ModuleIdentifier(ModuleId.JobShipment, ResString.GetMultilingualString("Module.JobShipment", "Shipments"), ResString.GetMultilingualString("Module.JobShipmentExtended", "Shipments (Forwarding)"));
		public static readonly ModuleIdentifier JobConsol = new ModuleIdentifier(ModuleId.JobConsol, ResString.GetMultilingualString("Module.JobConsol", "Consolidations"));
		public static readonly ModuleIdentifier PackLines = new ModuleIdentifier(ModuleId.PackLines, ResString.GetMultilingualString("Module.PackLines", "Pack Lines"));
		public static readonly ModuleIdentifier ConsolPlanningBoard = new ModuleIdentifier(ModuleId.ConsolPlanningBoard, ResString.GetMultilingualString("Module.ConsolPlanningBoard", "Consolidation Planning Board"));
		public static readonly ModuleIdentifier CarrierContracts = new ModuleIdentifier(ModuleId.CarrierContracts, ResString.GetMultilingualString("Module.CarrierContracts", "Carrier Contracts"));
		public static readonly ModuleIdentifier ContractAllocationRoutes = new ModuleIdentifier(ModuleId.ContractAllocationRoutes, ResString.GetMultilingualString("Module.ContractAllocationRoutes", "Allocation Routes"));
		public static readonly ModuleIdentifier GatewayConsolProfitShareRedistribution = new ModuleIdentifier(ModuleId.GatewayConsolProfitShareRedistribution, ResString.GetMultilingualString("Module.GatewayConsolProfitShareRedistribution", "G/W Consol Profit Redistribution"));
		public static readonly ModuleIdentifier SupplierBooking = new ModuleIdentifier(ModuleId.SupplierBooking, ResString.GetMultilingualString("Module.SupplierBooking", "Supplier Booking"));
		public static readonly ModuleIdentifier SupplierBookingLine = new ModuleIdentifier(ModuleId.SupplierBookingLine, ResString.GetMultilingualString("Module.SupplierBookingLine", "Supplier Booking Line"));
		public static readonly ModuleIdentifier RelatedTransportLegs = new ModuleIdentifier(ModuleId.RelatedTransportLegs, ResString.GetMultilingualString("Module.RelatedTransportLegs", "Related Transport Legs"));
		public static readonly ModuleIdentifier ELoadList = new ModuleIdentifier(ModuleId.eLoadList, ResString.GetMultilingualString("Module.eLoadList", "eLoadLists"));
		public static readonly ModuleIdentifier RoutingLookups = new ModuleIdentifier(ModuleId.RoutingSolver, ResString.GetMultilingualString("Module.GlobalRoutingSolver", "Global Flight Schedules"));
		public static readonly ModuleIdentifier RefCommodityCode = new ModuleIdentifier(ModuleId.RefCommodityCode, ResString.GetMultilingualString("Module.RefCommodityCode", "Commodities"));
		public static readonly ModuleIdentifier RefOrgPartCategory = new ModuleIdentifier(ModuleId.RefOrgPartCategory, ResString.GetMultilingualString("Module.RefOrgPartCategory", "Product Categories"));
		public static readonly ModuleIdentifier RefEquipment = new ModuleIdentifier(ModuleId.RefEquipment, ResString.GetMultilingualString("Module.RefEquipment", "Equipment"));
		public static readonly ModuleIdentifier ProductionRulesPortal = new ModuleIdentifier(ModuleId.ProductionRulesPortal, ResString.GetMultilingualString("Module.ProductionRulesPortal", "Production Rules Management Portal"));
		public static readonly ModuleIdentifier RefPremisesGateCode = new ModuleIdentifier(ModuleId.RefPremisesGateCode, ResString.GetMultilingualString("Module.RefPremisesGateCode", "Premises Gate Codes"));
		public static readonly ModuleIdentifier AccChargeCode = new ModuleIdentifier(ModuleId.AccChargeCode, ResString.GetMultilingualString("Module.AccChargeCode", "Charge Codes"));
		public static readonly ModuleIdentifier AccGlobalChargeCode = new ModuleIdentifier(ModuleId.AccGlobalChargeCode, ResString.GetMultilingualString("Module.AccGlobalChargeCode", "Global Charge Codes"));
		public static readonly ModuleIdentifier AccChargeCodeForRegistry = new ModuleIdentifier(ModuleId.AccChargeCodeForRegistry, ResString.GetMultilingualString("Module.AccChargeCodeForRegistry", "Charge Codes"), ResString.GetMultilingualString("Module.AccChargeCodeForRegistryExtended", "Charge Codes (For Registry)"));
		public static readonly ModuleIdentifier AccTaxOverrideGroup = new ModuleIdentifier(ModuleId.AccTaxOverrideGroup, ResString.GetMultilingualString("Module.AccTaxOverrideGroup", "Tax Override Groups"));
		public static readonly ModuleIdentifier AccReportingBook = new ModuleIdentifier(ModuleId.AccReportingBook, ResString.GetMultilingualString("Module.AccReportingBook", "Reporting Books"));
		public static readonly ModuleIdentifier AccPlaceOfSupplyChargeCodeGroup = new ModuleIdentifier(ModuleId.AccPlaceOfSupplyChargeCodeGroup, ResString.GetMultilingualString("Module.AccPlaceOfSupplyChargeCodeGroup", "Place of Supply Configuration Groups"));
		public static readonly ModuleIdentifier AccOrgTaxConfigurationTemplate = new ModuleIdentifier(ModuleId.AccOrgTaxConfigurationTemplate, ResString.GetMultilingualString("Module.AccOrgTaxConfigurationTemplate", "Tax Configuration Templates"));
		public static readonly ModuleIdentifier GlobalChargeCodeIntercompany = new ModuleIdentifier(ModuleId.GlobalChargeCodeIntercompany, ResString.GetMultilingualString("Module.GlobalChargeCodeIntercompany", "Intercompany Charge Code Mappings"));
		public static readonly ModuleIdentifier GlobalChargeCodeOrganization = new ModuleIdentifier(ModuleId.GlobalChargeCodeOrganization, ResString.GetMultilingualString("Module.GlobalChargeCodeOrganization", "Organization Charge Code Mappings"));
		public static readonly ModuleIdentifier RefCurrency = new ModuleIdentifier(ModuleId.RefCurrency, ResString.GetMultilingualString("Module.RefCurrency", "Currencies"));
		public static readonly ModuleIdentifier InternationalZone = new ModuleIdentifier(ModuleId.InternationalZone, ResString.GetMultilingualString("Module.InternationalZone", "International Zones"));
		public static readonly ModuleIdentifier RefTimeZoneSet = new ModuleIdentifier(ModuleId.RefTimeZoneSet, ResString.GetMultilingualString("Module.RefTimeZoneSet", "Time Zones"));
		public static readonly ModuleIdentifier RefCountry = new ModuleIdentifier(ModuleId.RefCountry, ResString.GetMultilingualString("Module.RefCountry", "Countries/Regions"));
		public static readonly ModuleIdentifier RefCityTown = new ModuleIdentifier(ModuleId.RefCityTown, ResString.GetMultilingualString("Module.RefCityTown", "Cities/Towns"));
		public static readonly ModuleIdentifier RefCountryStates = new ModuleIdentifier(ModuleId.RefCountryStates, ResString.GetMultilingualString("Module.RefCountryStates", "States"));
		public static readonly ModuleIdentifier RefPostCode = new ModuleIdentifier(ModuleId.RefPostCode, ResString.GetMultilingualString("Module.RefPostCode", "Post Codes"));
		public static readonly ModuleIdentifier OrgAddresses = new ModuleIdentifier(ModuleId.OrgAddresses, ResString.GetMultilingualString("Module.OrgAddresses", "Organization Addresses"));
		public static readonly ModuleIdentifier GenShapeGeography = new ModuleIdentifier(ModuleId.GenShapeGeography, ResString.GetMultilingualString("Module.GenShapeGeography", "Geography"));
		public static readonly ModuleIdentifier Sales = new ModuleIdentifier(ModuleId.Sales, ResString.GetMultilingualString("Module.Sales", "Sales"));
		public static readonly ModuleIdentifier SalesEnquiry = new ModuleIdentifier(ModuleId.SalesEnquiry, ResString.GetMultilingualString("Module.SalesEnquiry", "Inquiry Manager"));
		public static readonly ModuleIdentifier SalesProduct = new ModuleIdentifier(ModuleId.SalesProduct, ResString.GetMultilingualString("Module.SalesProduct", "Sales Products"));
		public static readonly ModuleIdentifier Commission = new ModuleIdentifier(ModuleId.Commission, ResString.GetMultilingualString("Module.Commission", "Commission Management"));
		public static readonly ModuleIdentifier CommissionApprovalRequest = new ModuleIdentifier(ModuleId.CommissionApprovalRequest, ResString.GetMultilingualString("Module.CommissionApprovalRequest", "Commission Approval Request"));
		public static readonly ModuleIdentifier CommissionAgreementLogFilter = new ModuleIdentifier(ModuleId.CommissionAgreementLogFilter, ResString.GetMultilingualString("Module.CommissionAgreementLogFilter", "Commission Agreement Log Filter"));
		public static readonly ModuleIdentifier Communication = new ModuleIdentifier(ModuleId.Communication, ResString.GetMultilingualString("Module.Communication", "Communication Manager"));
		public static readonly ModuleIdentifier OrgCommissionAgreement = new ModuleIdentifier(ModuleId.OrgCommissionAgreement, ResString.GetMultilingualString("Module.OrgCommissionAgreement", "Commission Agreements"));
		public static readonly ModuleIdentifier OrgContacts = new ModuleIdentifier(ModuleId.OrgContacts, ResString.GetMultilingualString("Module.OrgContacts", "Organization Contacts"));
		public static readonly ModuleIdentifier OrgContactsStmALog = new ModuleIdentifier(ModuleId.OrgContactsStmALog, ResString.GetMultilingualString("Module.OrgContactsStmALog", "Organization Contacts Logs"));
		public static readonly ModuleIdentifier OrgCusCode = new ModuleIdentifier(ModuleId.OrgCusCode, ResString.GetMultilingualString("Module.OrgCusCode", "Customs Registration Numbers"));
		public static readonly ModuleIdentifier RefContainer = new ModuleIdentifier(ModuleId.RefContainer, ResString.GetMultilingualString("Module.RefContainer", "Containers"), ResString.GetMultilingualString("Module.RefContainerExtended", "Containers (Reference)"));
		public static readonly ModuleIdentifier RefContainerISOTypes = new ModuleIdentifier(ModuleId.RefContainerISOTypes, ResString.GetMultilingualString("Module.RefContainerISOTypes", "Containers ISO Types"));
		public static readonly ModuleIdentifier RefNMFC = new ModuleIdentifier(ModuleId.RefNMFC, ResString.GetMultilingualString("Module.RefNMFC", "NMFC"));
		public static readonly ModuleIdentifier CountryStatesGlbHoliday = new ModuleIdentifier(ModuleId.CountryStatesGlbHoliday, ResString.GetMultilingualString("Module.CountryStatesGlbHoliday", "Holidays"));
		public static readonly ModuleIdentifier RefUNLOCO = new ModuleIdentifier(ModuleId.RefUNLOCO, ResString.GetMultilingualString("Module.RefUNLOCO", "UNLOCO"));
		public static readonly ModuleIdentifier OrgCreditorGroup = new ModuleIdentifier(ModuleId.OrgCreditorGroup, ResString.GetMultilingualString("Module.OrgCreditorGroup", "Creditor Groups"));
		public static readonly ModuleIdentifier OrgDebtorGroup = new ModuleIdentifier(ModuleId.OrgDebtorGroup, ResString.GetMultilingualString("Module.OrgDebtorGroup", "Debtor Groups"));
		public static readonly ModuleIdentifier RefDocType = new ModuleIdentifier(ModuleId.RefDocType, ResString.GetMultilingualString("Module.RefDocType", "Document Types"));
		public static readonly ModuleIdentifier RefDocSource = new ModuleIdentifier(ModuleId.RefDocSource, ResString.GetMultilingualString("Module.RefDocSource", "Document Sources"));
		public static readonly ModuleIdentifier RefDocOrgCusCode = new ModuleIdentifier(ModuleId.RefDocOrgCusCode, ResString.GetMultilingualString("Module.RefDocOrgCusCode", "Document Organization Registration Mapping"));
		public static readonly ModuleIdentifier GlbPerson = new ModuleIdentifier(ModuleId.GlbPerson, ResString.GetMultilingualString("Module.GlbPerson", "Person Intelligence"));
		public static readonly ModuleIdentifier GlbBranch = new ModuleIdentifier(ModuleId.GlbBranch, ResString.GetMultilingualString("Module.GlbBranch", "Branches"));
		public static readonly ModuleIdentifier GlbBranchNotCurrentCompanyRelated = new ModuleIdentifier(ModuleId.GlbBranchNotCurrentCompanyRelated, ResString.GetMultilingualString("Module.GlbBranchNotCurrentCompanyRelated", "Branches"), ResString.GetMultilingualString("Module.GlbBranchNotCurrentCompanyRelatedExtended", "Branches (Not Current Company Related)"));
		public static readonly ModuleIdentifier GlbCompany = new ModuleIdentifier(ModuleId.GlbCompany, ResString.GetMultilingualString("Module.GlbCompany", "Companies"));
		public static readonly ModuleIdentifier GlbCompanyCampaign = new ModuleIdentifier(ModuleId.GlbCompanyCampaign, ResString.GetMultilingualString("Module.GlbCompanyCampaign", "Campaign Management"));
		public static readonly ModuleIdentifier GlbCompanyCampaignWithoutFilter = new ModuleIdentifier(ModuleId.GlbCompanyCampaignWithoutFilter, ResString.GetMultilingualString("Module.GlbCompanyCampaignWithoutFilter", "Campaign Management"), ResString.GetMultilingualString("Module.GlbCompanyCampaignWithoutFilterExtended", "Campaign Management (Without Filter)"));
		public static readonly ModuleIdentifier GlbCompanyCampaignClick = new ModuleIdentifier(ModuleId.GlbCompanyCampaignClick, ResString.GetMultilingualString("GUI.GlbCompanyCampaignClick", "Campaign link click module filters"));
		public static readonly ModuleIdentifier GlbCompanyCampaignContact = new ModuleIdentifier(ModuleId.GlbCompanyCampaignContact, ResString.GetMultilingualString("GUI.GlbCompanyCampaignContact", "Campaign contact module filters"));
		public static readonly ModuleIdentifier GlbCompanyCampaignItem = new ModuleIdentifier(ModuleId.GlbCompanyCampaignItem, ResString.GetMultilingualString("GUI.GlbCompanyCampaignItem", "Campaign sent to Contact"));
		public static readonly ModuleIdentifier GlbCompanyCampaignItemSchedule = new ModuleIdentifier(ModuleId.GlbCompanyCampaignItemSchedule, ResString.GetMultilingualString("GUI.GlbCompanyCampaignItemSchedule", "Campaign items scheduled for sending"));
		public static readonly ModuleIdentifier DripMarketingFilterRule = new ModuleIdentifier(ModuleId.DripMarketingFilterRule, ResString.GetMultilingualString("Module.DripMarketingFilterRule", "Filter Rule"));
		public static readonly ModuleIdentifier DripMarketingFilterRuleHR = new ModuleIdentifier(ModuleId.DripMarketingFilterRuleHR, ResString.GetMultilingualString("Module.DripMarketingFilterRuleHR", "Filter Rule"));
		public static readonly ModuleIdentifier DripMarketingFilterRuleEDI = new ModuleIdentifier(ModuleId.DripMarketingFilterRuleEDI, ResString.GetMultilingualString("Module.DripMarketingFilterRuleEDI", "Filter Rule"));
		public static readonly ModuleIdentifier SalesDashboard = new ModuleIdentifier(ModuleId.SalesDashboard, ResString.GetMultilingualString("Module.SalesDashboard", "Dashboard"));
		public static readonly ModuleIdentifier OrgSalesDashboard = new ModuleIdentifier(ModuleId.OrgSalesDashboard, ResString.GetMultilingualString("Module.OrgSalesDashboard", "Organization Sales Dashboard"));
		public static readonly ModuleIdentifier GlbDepartment = new ModuleIdentifier(ModuleId.GlbDepartment, ResString.GetMultilingualString("Module.GlbDepartment", "Departments"));
		public static readonly ModuleIdentifier GlbGroup = new ModuleIdentifier(ModuleId.GlbGroup, ResString.GetMultilingualString("Module.GlbGroup", "Group"));
		public static readonly ModuleIdentifier NewsAndAnnouncement = new ModuleIdentifier(ModuleId.NewsAndAnnouncement, ResString.GetMultilingualString("Module.NewsAndAnnouncement", "News and Announcements"));
		public static readonly ModuleIdentifier AccGroups = new ModuleIdentifier(ModuleId.AccGroups, ResString.GetMultilingualString("Module.AccGroups", "Sales/Expense Groups"));
		public static readonly ModuleIdentifier AccGLHeader = new ModuleIdentifier(ModuleId.AccGLHeader, ResString.GetMultilingualString("Module.AccGLHeader", "GL Accounts"));
		public static readonly ModuleIdentifier AccGLAccountDescriptor = new ModuleIdentifier(ModuleId.AccGLAccountDescriptor, ResString.GetMultilingualString("Module.AccGLAccountDescriptor", "GL Multi-Language Mapping"));
		public static readonly ModuleIdentifier AccChequeBook = new ModuleIdentifier(ModuleId.AccChequeBook, ResString.GetMultilingualString("Module.AccChequeBook", "Check Books"));
		public static readonly ModuleIdentifier AccComplianceSequence = new ModuleIdentifier(ModuleId.AccComplianceSequence, ResString.GetMultilingualString("Module.AccComplianceSequence", "Compliance Sequences"));
		public static readonly ModuleIdentifier AccGeneralLedgerData = new ModuleIdentifier(ModuleId.AccGeneralLedgerData, ResString.GetMultilingualString("Module.AccGeneralLedgerData", "Accounting Journals"));
		public static readonly ModuleIdentifier AccComplianceReport = new ModuleIdentifier(ModuleId.AccComplianceReport, ResString.GetMultilingualString("Module.AccComplianceReport", "Compliance Reports"));
		public static readonly ModuleIdentifier AccBankAccount = new ModuleIdentifier(ModuleId.AccBankAccount, ResString.GetMultilingualString("Module.AccBankAccount", "Bank Accounts"));
		public static readonly ModuleIdentifier ImportAccountingData = new ModuleIdentifier(ModuleId.ImportAccountingData, ResString.GetMultilingualString("Module.ImportAccountingData", "Import Data"), ResString.GetMultilingualString("Module.ImportAccountingDataExtended", "Import Data (Accounting)"));
		public static readonly ModuleIdentifier OrderLine = new ModuleIdentifier(ModuleId.OrderLine, ResString.GetMultilingualString("Module.OrderLine", "Order Lines"), ResString.GetMultilingualString("Module.OrderLineExtended", "Order Lines (Order Manager)"));
		public static readonly ModuleIdentifier ForwardingReport = new ModuleIdentifier(ModuleId.FreightReport, ResString.GetMultilingualString("Module.FreightReport", "Reports"), ResString.GetMultilingualString("Module.FreightReportExtended", "Reports (Forwarding)"));
		public static readonly ModuleIdentifier CustomsGlobalReport = new ModuleIdentifier(ModuleId.CustomsGlblReport, ResString.GetMultilingualString("Module.CustomsGlblReport", "Reports"), ResString.GetMultilingualString("Module.CustomsGlblReportExtended", "Reports (Global Customs)"));
		public static readonly ModuleIdentifier CustomsReport = new ModuleIdentifier(ModuleId.CustomsReport, ResString.GetMultilingualString("Module.CustomsReport", "Reports"), ResString.GetMultilingualString("Module.CustomsReportExtended", "Reports (Customs)"));
		public static readonly ModuleIdentifier AccTaxRate = new ModuleIdentifier(ModuleId.AccTaxRate, ResString.GetMultilingualString("Module.AccTaxRate", "Tax ID"));
		public static readonly ModuleIdentifier AlternateChartofAccounts = new ModuleIdentifier(ModuleId.AlternateChartofAccounts, ResString.GetMultilingualString("Module.AlternateChartofAccounts", "Alternate Chart of Accounts"));
		public static readonly ModuleIdentifier AlternateGLAccounts = new ModuleIdentifier(ModuleId.AlternateGLAccounts, ResString.GetMultilingualString("Module.AlternateGLAccounts", "Alternate GL Accounts"));
		public static readonly ModuleIdentifier AccTaxRateForRegistry = new ModuleIdentifier(ModuleId.AccTaxRateForRegistry, ResString.GetMultilingualString("Module.AccTaxRateForRegistry", "Tax ID"), ResString.GetMultilingualString("Module.AccTaxRateForRegistryExtended", "Tax ID (For Registry)"));
		public static readonly ModuleIdentifier AccInvMsg = new ModuleIdentifier(ModuleId.AccInvMsg, ResString.GetMultilingualString("Module.AccInvMsg", "Invoice Tax Messages"));
		public static readonly ModuleIdentifier AccWithholding = new ModuleIdentifier(ModuleId.AccWithholding, ResString.GetMultilingualString("Module.AccWithholding", "WHT Tax ID"));
		public static readonly ModuleIdentifier AccApportionmentTemplate = new ModuleIdentifier(ModuleId.AccApportionmentTemplate, ResString.GetMultilingualString("Module.AccApportionmentTemplate", "Apportionment Templates"));
		public static readonly ModuleIdentifier Location = new ModuleIdentifier(ModuleId.Location, ResString.GetMultilingualString("Module.Location", "Location"));
		public static readonly ModuleIdentifier ViewLocation = new ModuleIdentifier(ModuleId.ViewLocation, ResString.GetMultilingualString("Module.ViewLocation", "Location"), ResString.GetMultilingualString("Module.ViewLocationExtended", "Location (View)"));
		public static readonly ModuleIdentifier JobAirSailing = new ModuleIdentifier(ModuleId.JobAirSailing, ResString.GetMultilingualString("Module.JobAirSailing", "Flight Schedule"));
		public static readonly ModuleIdentifier DocumentAllocation = new ModuleIdentifier(ModuleId.DocumentAllocation, ResString.GetMultilingualString("Module.DocumentAllocation", "Allocate eDocs"));
		public static readonly ModuleIdentifier DocumentDbMerger = new ModuleIdentifier(ModuleId.DocumentDbMerger, ResString.GetMultilingualString("Module.DocumentDbMerger", "Merge eDocs Databases"));
		public static readonly ModuleIdentifier DocumentDbManager = new ModuleIdentifier(ModuleId.DocumentDbManager, ResString.GetMultilingualString("Module.DocumentDbManager", "eDocs Database Manager"));
		public static readonly ModuleIdentifier ArchiveEDocs = new ModuleIdentifier(ModuleId.ArchiveEDocs, ResString.GetMultilingualString("Module.ArchiveEDocs", "Create eDocs CD"));
		public static readonly ModuleIdentifier DocumentTemplate = new ModuleIdentifier(ModuleId.DocumentTemplate, ResString.GetMultilingualString("Module.DocumentTemplate", "Document Templates"));
		public static readonly ModuleIdentifier RateAttachmentSet = new ModuleIdentifier(ModuleId.RateAttachmentSet, ResString.GetMultilingualString("Module.RateAttachmentSet", "Quotation Documents"));
		public static readonly ModuleIdentifier SalesTeam = new ModuleIdentifier(ModuleId.SalesTeam, ResString.GetMultilingualString("Module.SalesTeam", "Sales Teams"));
		public static readonly ModuleIdentifier SalesRep = new ModuleIdentifier(ModuleId.SalesRep, ResString.GetMultilingualString("Module.SalesRep", "Sales Reps"));
		public static readonly ModuleIdentifier RateTransportProvider = new ModuleIdentifier(ModuleId.RateTransportProvider, ResString.GetMultilingualString("Module.RateTransportProvider", "Transport Zone Sets"));
		public static readonly ModuleIdentifier RateTransportZone = new ModuleIdentifier(ModuleId.RateTransportZone, ResString.GetMultilingualString("Module.RateTransportZone", "Transport Zones"));
		public static readonly ModuleIdentifier PeriodManagement = new ModuleIdentifier(ModuleId.PeriodManagement, ResString.GetMultilingualString("Module.PeriodManagement", "Period Management"));
		public static readonly ModuleIdentifier OrdersReport = new ModuleIdentifier(ModuleId.OrdersReport, ResString.GetMultilingualString("Module.OrdersReport", "Reports"), ResString.GetMultilingualString("Module.OrdersReportExtended", "Reports (Orders)"));
		public static readonly ModuleIdentifier ShipmentReceival = new ModuleIdentifier(ModuleId.ShipmentReceival, ResString.GetMultilingualString("Module.ShipmentReceival", "Shipments"), ResString.GetMultilingualString("Module.ShipmentReceivalExtended", "Shipments (CFS/CTO)"));
		public static readonly ModuleIdentifier AccHotCheque = new ModuleIdentifier(ModuleId.AccHotCheque, ResString.GetMultilingualString("Module.AccHotCheque", "Hot Check"));
		public static readonly ModuleIdentifier JobCostingReport = new ModuleIdentifier(ModuleId.JobCostingReport, ResString.GetMultilingualString("Module.JobCostingReport", "Reports"), ResString.GetMultilingualString("Module.JobCostingReportExtended", "Reports (Job Costing)"));
		public static readonly ModuleIdentifier PackContainerRegistration = new ModuleIdentifier(ModuleId.PackContainerRegistration, ResString.GetMultilingualString("Module.PackContainerRegistration", "Container Registration"));
		public static readonly ModuleIdentifier LoadListConsol = new ModuleIdentifier(ModuleId.LoadListConsol, ResString.GetMultilingualString("Module.LoadListConsol", "Load Lists"));
		public static readonly ModuleIdentifier JobMawb = new ModuleIdentifier(ModuleId.JobMawb, ResString.GetMultilingualString("Module.JobMawb", "MAWB Stock"));
		public static readonly ModuleIdentifier ManifestTally = new ModuleIdentifier(ModuleId.ManifestTally, ResString.GetMultilingualString("Module.ManifestTally", "Tally"));
		public static readonly ModuleIdentifier ShipmentGatePass = new ModuleIdentifier(ModuleId.ShipmentGatePass, ResString.GetMultilingualString("Module.ShipmentGatePass", "Gate Pass"));
		public static readonly ModuleIdentifier PrintJob = new ModuleIdentifier(ModuleId.PrintJob, ResString.GetMultilingualString("Module.PrintJob", "Print Jobs"));
		public static readonly ModuleIdentifier DocumentSigningJob = new ModuleIdentifier(ModuleId.DocumentSigningJob, ResString.GetMultilingualString("Module.DocumentSigningJobs", "Document Signing Job"));
		public static readonly ModuleIdentifier ScheduledReports = new ModuleIdentifier(ModuleId.ScheduledReports, ResString.GetMultilingualString("Module.ScheduledReports", "Scheduled Reports"));
		public static readonly ModuleIdentifier ReportStatistics = new ModuleIdentifier(ModuleId.ReportStatistics, ResString.GetMultilingualString("Module.ReportStatistics", "Report Statistics"));
		public static readonly ModuleIdentifier UpdateNotesPortal = new ModuleIdentifier(ModuleId.UpdateNotesPortal, ResString.GetMultilingualString("Module.UpdateNotesPortal", "Update Notes Portal"));
		public static readonly ModuleIdentifier ReportManagement = new ModuleIdentifier(ModuleId.ReportManagement, ResString.GetMultilingualString("Module.ReportManagement", "Report Management"));
		public static readonly ModuleIdentifier StmServiceTask = new ModuleIdentifier(ModuleId.StmServiceTask, ResString.GetMultilingualString("Module.StmServiceTask", "Service Tasks"));
		public static readonly ModuleIdentifier ProcessController = new ModuleIdentifier(ModuleId.ProcessController, ResString.GetMultilingualString("Module.ProcessController", "Process Controllers"));
		public static readonly ModuleIdentifier LicenceUsage = new ModuleIdentifier(ModuleId.LicenceUsage, ResString.GetMultilingualString("Module.LicenceUsage", "License Usage"));
		public static readonly ModuleIdentifier PrintQueue = new ModuleIdentifier(ModuleId.PrintQueue, ResString.GetMultilingualString("Module.PrintQueue", "Print Queues"));
		public static readonly ModuleIdentifier StmMenuItem = new ModuleIdentifier(ModuleId.StmMenuItem, ResString.GetMultilingualString("Module.StmMenuItem", "Document Menu Item"));
		public static readonly ModuleIdentifier GlbStaff = new ModuleIdentifier(ModuleId.GlbStaff, ResString.GetMultilingualString("Module.GlbStaff", "Staff and Resources"));
		public static readonly ModuleIdentifier GlbStaffChangeRequest = new ModuleIdentifier(ModuleId.GlbStaffChangeRequest, ResString.GetMultilingualString("Module.GlbStaffChangeRequest", "Staff Change Request"));
		public static readonly ModuleIdentifier GlbStaffHoliday = new ModuleIdentifier(ModuleId.GlbStaffHoliday, ResString.GetMultilingualString("Module.GlbStaffHoliday", "Staff Holiday"));
		public static readonly ModuleIdentifier GlbCapability = new ModuleIdentifier(ModuleId.GlbCapability, ResString.GetMultilingualString("Module.GlbCapability", "Resource Capabilities"));
		public static readonly ModuleIdentifier ActiveUsers = new ModuleIdentifier(ModuleId.ActiveUsers, ResString.GetMultilingualString("Module.ActiveUsers", "Active Users"));
		public static readonly ModuleIdentifier JobSailing = new ModuleIdentifier(ModuleId.JobSailing, ResString.GetMultilingualString("Module.JobSailing", "Sailing Schedule"), ResString.GetMultilingualString("Module.JobSailingExtended", "Sailing Schedule (Generic)"));
		public static readonly ModuleIdentifier JobSeaSailing = new ModuleIdentifier(ModuleId.JobSeaSailing, ResString.GetMultilingualString("Module.JobSeaSailing", "Sailing Schedule"));
		public static readonly ModuleIdentifier JobRailSailing = new ModuleIdentifier(ModuleId.JobRailSailing, ResString.GetMultilingualString("Module.JobRailSailing", "Rail Schedule"));
		public static readonly ModuleIdentifier JobRoadSailing = new ModuleIdentifier(ModuleId.JobRoadSailing, ResString.GetMultilingualString("Module.JobRoadSailing", "Road Schedule"));
		public static readonly ModuleIdentifier JobSeaVoyage = new ModuleIdentifier(ModuleId.JobSeaVoyage, ResString.GetMultilingualString("Module.JobSeaVoyage", "Sailing Schedule"), ResString.GetMultilingualString("Module.JobSeaVoyageExtended", "Sailing Schedule (Voyage)"));
		public static readonly ModuleIdentifier SailingDataVendorImporting = new ModuleIdentifier(ModuleId.SailingDataVendorImporting, ResString.GetMultilingualString("Module.SailingDataVendorImporting", "Sailing Schedule Feed"));
		public static readonly ModuleIdentifier OnlineSailingSchedules = new ModuleIdentifier(ModuleId.OnlineSailingSchedules, ResString.GetMultilingualString("Module.GlobalSailingSchedules", "Global Sailing Schedules"));
		public static readonly ModuleIdentifier DocumentTracking = new ModuleIdentifier(ModuleId.DocumentTracking, ResString.GetMultilingualString("Module.DocumentTracking", "Document Tracking"));
		public static readonly ModuleIdentifier ARAccQueryClaim = new ModuleIdentifier(ModuleId.ARAccQueryClaim, ResString.GetMultilingualString("Module.ARAccQueryClaim", "Claims and Queries"), ResString.GetMultilingualString("Module.ARAccQueryClaimExtended", "Claims and Queries (AR)"));
		public static readonly ModuleIdentifier APAccQueryClaim = new ModuleIdentifier(ModuleId.APAccQueryClaim, ResString.GetMultilingualString("Module.APAccQueryClaim", "Claims and Queries"), ResString.GetMultilingualString("Module.APAccQueryClaimExtended", "Claims and Queries (AP)"));
		public static readonly ModuleIdentifier GlbPortDeliveryTime = new ModuleIdentifier(ModuleId.GlbPortDeliveryTime, ResString.GetMultilingualString("Module.GlbPortDeliveryTime", "Port Default Delivery Time"));
		public static readonly ModuleIdentifier BookingsReports = new ModuleIdentifier(ModuleId.BookingsReports, ResString.GetMultilingualString("Module.BookingsReports", "Reports"), ResString.GetMultilingualString("Module.BookingsReportsExtended", "Reports (Booking)"));
		public static readonly ModuleIdentifier TransportReports = new ModuleIdentifier(ModuleId.TransportReports, ResString.GetMultilingualString("Module.TransportReports", "Reports"), ResString.GetMultilingualString("Module.TransportReportsExtended", "Reports (Transport)"));
		public static readonly ModuleIdentifier CFSCTOReports = new ModuleIdentifier(ModuleId.CFSCTOReports, ResString.GetMultilingualString("Module.CFSCTOReports", "Reports"), ResString.GetMultilingualString("Module.CFSCTOReportsExtended", "Reports (CFS/CTO)"));
		public static readonly ModuleIdentifier SalesMgrReports = new ModuleIdentifier(ModuleId.SalesMgrReports, ResString.GetMultilingualString("Module.SalesMgrReports", "Reports"), ResString.GetMultilingualString("Module.SalesMgrReportsExtended", "Reports (Sales & Marketing)"));
		public static readonly ModuleIdentifier TariffRateReports = new ModuleIdentifier(ModuleId.TariffRateReports, ResString.GetMultilingualString("Module.TariffRateReports", "Reports"), ResString.GetMultilingualString("Module.TariffRateReportsExtended", "Reports (Tariffs & Rates)"));
		public static readonly ModuleIdentifier DocManagerReports = new ModuleIdentifier(ModuleId.DocManagerReports, ResString.GetMultilingualString("Module.DocManagerReports", "Reports"), ResString.GetMultilingualString("Module.DocManagerReportsExtended", "Reports (DocManager)"));
		public static readonly ModuleIdentifier ReceivReports = new ModuleIdentifier(ModuleId.ReceivReports, ResString.GetMultilingualString("Module.ReceivReports", "Reports"), ResString.GetMultilingualString("Module.ReceivReportsExtended", "Reports (Receivables)"));
		public static readonly ModuleIdentifier PayablesReports = new ModuleIdentifier(ModuleId.PayablesReports, ResString.GetMultilingualString("Module.PayablesReports", "Reports"), ResString.GetMultilingualString("Module.PayablesReportsExtended", "Reports (Payables)"));
		public static readonly ModuleIdentifier CashBookReports = new ModuleIdentifier(ModuleId.CashBookReports, ResString.GetMultilingualString("Module.CashBookReports", "Reports"), ResString.GetMultilingualString("Module.CashBookReportsExtended", "Reports (Cash Book)"));
		public static readonly ModuleIdentifier GLReports = new ModuleIdentifier(ModuleId.GLReports, ResString.GetMultilingualString("Module.GLReports", "Reports"), ResString.GetMultilingualString("Module.GLReportsExtended", "Reports (GL)"));
		public static readonly ModuleIdentifier BudgetReports = new ModuleIdentifier(ModuleId.BudgetReports, ResString.GetMultilingualString("Module.BudgetReports", "Reports"), ResString.GetMultilingualString("Module.BudgetReportsExtended", "Reports (Budgets)"));
		public static readonly ModuleIdentifier NettingReports = new ModuleIdentifier(ModuleId.NettingReports, ResString.GetMultilingualString("Module.NettingReports", "Reports"), ResString.GetMultilingualString("Module.NettingReportsExtended", "Reports (Netting)"));
		public static readonly ModuleIdentifier EISReports = new ModuleIdentifier(ModuleId.EISReports, ResString.GetMultilingualString("Module.EISReports", "Reports"), ResString.GetMultilingualString("Module.EISReportsExtended", "Reports (EIS)"));
		public static readonly ModuleIdentifier RefFilesReports = new ModuleIdentifier(ModuleId.RefFilesReports, ResString.GetMultilingualString("Module.RefFilesReports", "Reports"), ResString.GetMultilingualString("Module.RefFilesReportsExtended", "Reports (Reference Files)"));
		public static readonly ModuleIdentifier ArchiveReports = new ModuleIdentifier(ModuleId.ArchiveReports, ResString.GetMultilingualString("Module.ArchiveReports", "Reports"), ResString.GetMultilingualString("Module.ArchiveReportsExtended", "Reports (Archive Manager)"));
		public static readonly ModuleIdentifier MasterDataReports = new ModuleIdentifier(ModuleId.MasterDataReports, ResString.GetMultilingualString("Module.MasterDataReports", "Reports"), ResString.GetMultilingualString("Module.MasterDataReportsExtended", "Reports (Master Data)"));
		public static readonly ModuleIdentifier LocationsReports = new ModuleIdentifier(ModuleId.LocationsReports, ResString.GetMultilingualString("Module.LocationsReports", "Reports"), ResString.GetMultilingualString("Module.LocationsReportsExtended", "Reports (Locations)"));
		public static readonly ModuleIdentifier AccountReports = new ModuleIdentifier(ModuleId.AccountReports, ResString.GetMultilingualString("Module.AccountReports", "Reports"), ResString.GetMultilingualString("Module.AccountReportsExtended", "Reports (Account)"));
		public static readonly ModuleIdentifier CustFilesReports = new ModuleIdentifier(ModuleId.CustFilesReports, ResString.GetMultilingualString("Module.CustFilesReports", "Reports"), ResString.GetMultilingualString("Module.CustFilesReportsExtended", "Reports (Customs Files)"));
		public static readonly ModuleIdentifier ProcessMgrReports = new ModuleIdentifier(ModuleId.ProcessMgrReports, ResString.GetMultilingualString("Module.ProcessMgrReports", "Reports"), ResString.GetMultilingualString("Module.ProcessMgrReportsExtended", "Reports (Workflow & Process)"));
		public static readonly ModuleIdentifier SystemReports = new ModuleIdentifier(ModuleId.SystemReports, ResString.GetMultilingualString("Module.SystemReports", "Reports"), ResString.GetMultilingualString("Module.SystemReportsExtended", "Reports (System)"));
		public static readonly ModuleIdentifier UserAdminReports = new ModuleIdentifier(ModuleId.UserAdminReports, ResString.GetMultilingualString("Module.UserAdminReports", "Reports"), ResString.GetMultilingualString("Module.UserAdminReportsExtended", "Reports (User Admin)"));
		public static readonly ModuleIdentifier Statement = new ModuleIdentifier(ModuleId.Statement, ResString.GetMultilingualString("Module.Statement", "Statements"));
		public static readonly ModuleIdentifier NettingStatement = new ModuleIdentifier(ModuleId.NettingStatement, ResString.GetMultilingualString("Module.NettingStatement", "Netting Statements"));
		public static readonly ModuleIdentifier NettingPeriod = new ModuleIdentifier(ModuleId.NettingPeriod, ResString.GetMultilingualString("Module.NettingPeriod", "Netting Period"));
		public static readonly ModuleIdentifier ARTransaction = new ModuleIdentifier(ModuleId.ARTransaction, ResString.GetMultilingualString("Module.ARTransaction", "Receivables Transactions"));
		public static readonly ModuleIdentifier APTransaction = new ModuleIdentifier(ModuleId.APTransaction, ResString.GetMultilingualString("Module.APTransaction", "Payables Transactions"));
		public static readonly ModuleIdentifier TransactionsPendingAllocation = new ModuleIdentifier(ModuleId.TransactionsPendingAllocation, ResString.GetMultilingualString("Module.TransactionsPendingAllocation", "Transactions Pending Allocation"));
		public static readonly ModuleIdentifier CashbookTransaction = new ModuleIdentifier(ModuleId.CashbookTransaction, ResString.GetMultilingualString("Module.CashbookTransaction", "Cashbook Transactions"));
		public static readonly ModuleIdentifier Cheque = new ModuleIdentifier(ModuleId.Cheque, ResString.GetMultilingualString("Module.Cheque", "Cheques"));
		public static readonly ModuleIdentifier ChequeTransaction = new ModuleIdentifier(ModuleId.ChequeTransaction, ResString.GetMultilingualString("Module.ChequeTransaction", "Cheque Transactions"));
		public static readonly ModuleIdentifier UnapprovedTransaction = new ModuleIdentifier(ModuleId.UnapprovedTransaction, ResString.GetMultilingualString("Module.UnapprovedTransaction", "Transaction Approval"));
		public static readonly ModuleIdentifier UnapprovedIntercompanyTransaction = new ModuleIdentifier(ModuleId.UnapprovedIntercompanyTransaction, ResString.GetMultilingualString("Module.UnapprovedIntercompanyTransaction", "Intercompany Transaction Approval"));
		public static readonly ModuleIdentifier CASSCostFileImport = new ModuleIdentifier(ModuleId.CASSCostFileImport, ResString.GetMultilingualString("Module.CASSCostFileImport", "CASS Cost File Import"));
		public static readonly ModuleIdentifier GenericCharge = new ModuleIdentifier(ModuleId.GenericCharge, ResString.GetMultilingualString("Module.GenericCharge", "Charge Code / GL Account"));
		public static readonly ModuleIdentifier GenericConsol = new ModuleIdentifier(ModuleId.GenericConsol, ResString.GetMultilingualString("Module.GenericConsol", "Forwarding Consol / Warehouse Consol"));
		public static readonly ModuleIdentifier GenericTransaction = new ModuleIdentifier(ModuleId.GenericTransaction, ResString.GetMultilingualString("Module.GenericTransaction", "Transaction Search"));
		public static readonly ModuleIdentifier CNDataInterface = new ModuleIdentifier(ModuleId.CNDataInterface, ResString.GetMultilingualString("Module.CNDataInterface", "GB-T 24589.1 Data-interface"));
		public static readonly ModuleIdentifier CN2004DataInterface = new ModuleIdentifier(ModuleId.CN2004DataInterface, ResString.GetMultilingualString("Module.CN2004DataInterface", "GB-T 19851-2004 Data-interface"));
		public static readonly ModuleIdentifier CNReconciliationExport = new ModuleIdentifier(ModuleId.CNReconciliationExport, ResString.GetMultilingualString("Module.CNReconciliationExport", "Invoices Reconciliation Export"));
		public static readonly ModuleIdentifier PortDepotCarrierSelection = new ModuleIdentifier(ModuleId.PortDepotCarrierSelection, ResString.GetMultilingualString("Module.PortDepotCarrierSelection", "Port/Depot/Carrier Selection"));
		public static readonly ModuleIdentifier PortHubSelection = new ModuleIdentifier(ModuleId.PortHubSelection, ResString.GetMultilingualString("Module.PortHubSelection", "Port and Depot Selection"));
		public static readonly ModuleIdentifier RefTransitTime = new ModuleIdentifier(ModuleId.RefTransitTime, ResString.GetMultilingualString("Module.RefTransitTime", "Transit Time"));
		public static readonly ModuleIdentifier TransitTimeServiceLevelCombination = new ModuleIdentifier(ModuleId.TransitTimeServiceLevelCombination, ResString.GetMultilingualString("Module.TransitTimeServiceLevelCombination", "Service Levels"));
		public static readonly ModuleIdentifier HVLVBookingHeader = new ModuleIdentifier(ModuleId.HVLVBookingHeader, ResString.GetMultilingualString("Module.HVLVBookingHeader", "HVLV Booking Headers"));
		public static readonly ModuleIdentifier HVLVOriginLoadList = new ModuleIdentifier(ModuleId.HVLVOriginLoadList, ResString.GetMultilingualString("Module.HVLVOriginLoadList", "HVLV Origin Load List"));
		public static readonly ModuleIdentifier HVLVOuterPackage = new ModuleIdentifier(ModuleId.HVLVOuterPackage, ResString.GetMultilingualString("Module.HVLVOuterPackage", "HVLV Outer Packages"));
		public static readonly ModuleIdentifier HVLVConsignment = new ModuleIdentifier(ModuleId.HVLVConsignment, ResString.GetMultilingualString("Module.HVLVConsignment", "HVLV Consignments"));
		public static readonly ModuleIdentifier JobBillingExRateSysConfig = new ModuleIdentifier(ModuleId.JobBillingExRateSysConfig, ResString.GetMultilingualString("Module.JobBillingExRateSysConfig", "Job Billing Exchange Rate Configuration"));
		public static readonly ModuleIdentifier ARComplianceDocument = new ModuleIdentifier(ModuleId.ARComplianceDocument, ResString.GetMultilingualString("Module.ARComplianceDocument", "Receivables Compliance Documents"));
		public static readonly ModuleIdentifier APComplianceDocument = new ModuleIdentifier(ModuleId.APComplianceDocument, ResString.GetMultilingualString("Module.APComplianceDocument", "Payables Compliance Documents"));
		public static readonly ModuleIdentifier PaymentBatch = new ModuleIdentifier(ModuleId.PaymentBatch, ResString.GetMultilingualString("Module.PaymentBatch", "Payment Batches"));
		public static readonly ModuleIdentifier APInvoiceProcessingPortal = new ModuleIdentifier(ModuleId.APInvoiceProcessingPortal, ResString.GetMultilingualString("Module.APInvoiceProcessingPortal", "Invoice Processing Portal"));

		public static readonly ModuleIdentifier ChinaJournalListing = new ModuleIdentifier(ModuleId.ChinaJournalListing, ResString.GetMultilingualString("Module.ChinaJournalListing", "Print China Journal Listing"));
		public static readonly ModuleIdentifier AccountingVoucher = new ModuleIdentifier(ModuleId.AccountingVoucher, ResString.GetMultilingualString("Module.AccountingVoucher", "Print Accounting Voucher"));
		public static readonly ModuleIdentifier DirectDebitFile = new ModuleIdentifier(ModuleId.DirectDebitFile, ResString.GetMultilingualString("Module.DirectDebitFile", "Direct Debit Batches"));

		public static readonly ModuleIdentifier DepositBatch = new ModuleIdentifier(ModuleId.DepositBatch, ResString.GetMultilingualString("Module.DepositBatch", "Deposit Batches"));
		public static readonly ModuleIdentifier BankReconcilliation = new ModuleIdentifier(ModuleId.BankReconcilliation, ResString.GetMultilingualString("Module.BankReconcilliation", "Bank Reconciliations"));
		public static readonly ModuleIdentifier InvoiceBatch = new ModuleIdentifier(ModuleId.InvoiceBatch, ResString.GetMultilingualString("Module.InvoiceBatch", "Invoice Batch"));
		public static readonly ModuleIdentifier APEnquiry = new ModuleIdentifier(ModuleId.APEnquiry, ResString.GetMultilingualString("Module.APEnquiry", "Payables Enquiries"));
		public static readonly ModuleIdentifier AREnquiry = new ModuleIdentifier(ModuleId.AREnquiry, ResString.GetMultilingualString("Module.AREnquiry", "Receivables Enquiries"));
		public static readonly ModuleIdentifier ARCreditNoteApproval = new ModuleIdentifier(ModuleId.ARCreditNoteApproval, ResString.GetMultilingualString("Module.ARCreditNoteApproval", "Credit Note Approval"));
		public static readonly ModuleIdentifier TransactionsPendingAllocationApproval = new ModuleIdentifier(ModuleId.TransactionsPendingAllocationApproval, ResString.GetMultilingualString("Module.TransactionsPendingAllocationApproval", "Transactions Pending Allocation Approval"));
		public static readonly ModuleIdentifier APInvoiceApproval = new ModuleIdentifier(ModuleId.APInvoiceApproval, ResString.GetMultilingualString("Module.APInvoiceApproval", "Invoice Approval"));
		public static readonly ModuleIdentifier CreditControlledDocumentsApproval = new ModuleIdentifier(ModuleId.CreditControlledDocumentsApproval, ResString.GetMultilingualString("Module.CreditControlledDocumentsApproval", "Credit Controlled Documents Approval"));
		public static readonly ModuleIdentifier InvoicePrinting = new ModuleIdentifier(ModuleId.InvoicePrinting, ResString.GetMultilingualString("Module.InvoicePrinting", "Print Invoice"));
		public static readonly ModuleIdentifier JobHeader = new ModuleIdentifier(ModuleId.JobHeader, ResString.GetMultilingualString("Module.JobHeader", "Job Header"));
		public static readonly ModuleIdentifier OrgMatchApproval = new ModuleIdentifier(ModuleId.OrgMatchApproval, ResString.GetMultilingualString("Module.OrgMatchApproval", "Organization Matching"));
		public static readonly ModuleIdentifier CartageType = new ModuleIdentifier(ModuleId.CartageType, ResString.GetMultilingualString("Module.CartageType", "Port Transport Job Type"));
		public static readonly ModuleIdentifier Cartage = new ModuleIdentifier(ModuleId.Cartage, ResString.GetMultilingualString("Module.Cartage", "Transport Jobs"));
		public static readonly ModuleIdentifier CartageWorkSheet = new ModuleIdentifier(ModuleId.CartageWorkSheet, ResString.GetMultilingualString("Module.CartageWorkSheet", "Run Sheets"), ResString.GetMultilingualString("Module.CartageWorkSheetExtended", "Run Sheets (Port Transport)"));
		public static readonly ModuleIdentifier CartageLeg = new ModuleIdentifier(ModuleId.CartageLeg, ResString.GetMultilingualString("Module.CartageLeg", "Leg Viewer"));
		public static readonly ModuleIdentifier CartageLegPlanner = new ModuleIdentifier(ModuleId.CartageLegPlanner, ResString.GetMultilingualString("Module.CartageLegPlanner", "Leg Planner"));
		public static readonly ModuleIdentifier CartageRunSheetDashboard = new ModuleIdentifier(ModuleId.CartageRunSheetDashboard, ResString.GetMultilingualString("Module.CartageRunSheetDashboard", "Run Sheet Dashboard"));
		public static readonly ModuleIdentifier TradeLane = new ModuleIdentifier(ModuleId.TradeLane, ResString.GetMultilingualString("Module.TradeLane", "Trade Lanes"));
		public static readonly ModuleIdentifier APIncompleteInvoices = new ModuleIdentifier(ModuleId.APIncompleteInvoices, ResString.GetMultilingualString("Module.APIncompleteInvoices", "Incomplete Invoices"));
		public static readonly ModuleIdentifier ComPayRegisteredOrganisations = new ModuleIdentifier(ModuleId.ComPayRegisteredOrganisations, ResString.GetMultilingualString("Module.ComPayRegisteredOrganisations", "ComPay Registered Organizations"));
		public static readonly ModuleIdentifier AccCollectionBatch = new ModuleIdentifier(ModuleId.AccCollectionBatch, ResString.GetMultilingualString("Module.AccCollectionBatch", "Collection Batch"));
		public static readonly ModuleIdentifier AccCollectionOrder = new ModuleIdentifier(ModuleId.AccCollectionOrder, ResString.GetMultilingualString("Module.AccCollectionOrder", "Collection Orders"));
		public static readonly ModuleIdentifier AccPayableOrder = new ModuleIdentifier(ModuleId.AccPayableOrder, ResString.GetMultilingualString("Module.AccPayableOrder", "Purchase Orders"));
		public static readonly ModuleIdentifier ARCashAdvance = new ModuleIdentifier(ModuleId.ARCashAdvance, ResString.GetMultilingualString("Module.ARCashAdvanceRenamed", "Advance Payments"));
		public static readonly ModuleIdentifier APCashAdvance = new ModuleIdentifier(ModuleId.APCashAdvance, ResString.GetMultilingualString("Module.APCashAdvanceRenamed", "Advance Payments"));

		public static readonly ModuleIdentifier AssetManagementPortal = new ModuleIdentifier(ModuleId.AssetManagementPortal, ResString.GetMultilingualString("Module.AssetManagementPortal", "Asset Management Portal"));
		public static readonly ModuleIdentifier AssetManagementReports = new ModuleIdentifier(ModuleId.AssetMgmtReports, ResString.GetMultilingualString("Module.AssetManagementReports", "Reports"), ResString.GetMultilingualString("Module.AssetManagementReportsExtended", "Reports (Asset Management)"));

		public static readonly ModuleIdentifier ConsolidatedTransportBooking = new ModuleIdentifier(ModuleId.ConsolidatedTransportBooking, ResString.GetMultilingualString("Module.ConsolidatedTransportBooking", "Transport Booking"));
		public static readonly ModuleIdentifier PickupDeliveryConfirm = new ModuleIdentifier(ModuleId.PickupDeliveryConfirm, ResString.GetMultilingualString("Module.PickupDeliveryConfirm", "Confirmations"));

		public static readonly ModuleIdentifier WarningAcknowledgement = new ModuleIdentifier(ModuleId.WarningAcknowledgement, ResString.GetMultilingualString("Module.WarningAcknowledgement", "Warning Acknowledgement"));
		public static readonly ModuleIdentifier StmUpgrade = new ModuleIdentifier(ModuleId.StmUpgrade, ResString.GetMultilingualString("Module.StmUpgrade", "Upgrades"));
		public static readonly ModuleIdentifier UNDGSubstance = new ModuleIdentifier(ModuleId.UNDGSubstance, ResString.GetMultilingualString("Module.UNDGSubstance", "Dangerous Goods"));
		public static readonly ModuleIdentifier UNDGCommonData = new ModuleIdentifier(ModuleId.UNDGCommonData, ResString.GetMultilingualString("Module.UNDGCommonData", "Dangerous Goods Common Provisions"));
		public static readonly ModuleIdentifier UNDGCountryReference = new ModuleIdentifier(ModuleId.UNDGCountryReference, ResString.GetMultilingualString("Module.UNDGCountryReference", "Dangerous Goods Country/Region References"));
		public static readonly ModuleIdentifier RefCarrierConsortium = new ModuleIdentifier(ModuleId.RefCarrierConsortium, ResString.GetMultilingualString("Module.RefCarrierConsortium", "Vessel Consortium"));
		public static readonly ModuleIdentifier GLBudget = new ModuleIdentifier(ModuleId.GLBudget, ResString.GetMultilingualString("Module.GLBudget", "Budgets"));
		public static readonly ModuleIdentifier Registry = new ModuleIdentifier(ModuleId.Registry, ResString.GetMultilingualString("Module.Registry", "Registry"));
		public static readonly ModuleIdentifier StmFeatureTest = new ModuleIdentifier(ModuleId.StmFeatureTest, ResString.GetMultilingualString("Module.StmFeatureTest", "Feature Test"));
		public static readonly ModuleIdentifier ZARMatching = new ModuleIdentifier(ModuleId.ZARMatching, ResString.GetMultilingualString("Module.ZARMatching", "Match Transactions"), ResString.GetMultilingualString("Module.ZARMatchingExtended", "Match Transactions (AR)"));
		public static readonly ModuleIdentifier ZAPMatching = new ModuleIdentifier(ModuleId.ZAPMatching, ResString.GetMultilingualString("Module.ZAPMatching", "Match Transactions"), ResString.GetMultilingualString("Module.ZAPMatchingExtended", "Match Transactions (AP)"));
		public static readonly ModuleIdentifier RefAirline = new ModuleIdentifier(ModuleId.RefAirline, ResString.GetMultilingualString("Module.RefAirline", "Airlines"));
		public static readonly ModuleIdentifier NumericCodeRefAirline = new ModuleIdentifier(ModuleId.NumericCodeRefAirline, ResString.GetMultilingualString("Module.NumericCodeRefAirline", "Airlines"));
		public static readonly ModuleIdentifier RefPackType = new ModuleIdentifier(ModuleId.RefPackType, ResString.GetMultilingualString("Module.RefPackType", "Package Types"));
		public static readonly ModuleIdentifier RefDomesticCartageZone = new ModuleIdentifier(ModuleId.RefDomesticCartageZone, ResString.GetMultilingualString("Module.RefDomesticCartageZone", "Port Transport Zones (ACI)"));
		public static readonly ModuleIdentifier MailItem = new ModuleIdentifier(ModuleId.MailItem, ResString.GetMultilingualString("Module.MailItem", "Emails"));
		public static readonly ModuleIdentifier MailItemTemplate = new ModuleIdentifier(ModuleId.MailItemTemplate, ResString.GetMultilingualString("Module.MailItemTemplate", "Email Templates"));
		public static readonly ModuleIdentifier StmALog = new ModuleIdentifier(ModuleId.StmALog, ResString.GetMultilingualString("Module.StmALog", "Logs"));
		public static readonly ModuleIdentifier StmModuleFilter = new ModuleIdentifier(ModuleId.StmModuleFilter, ResString.GetMultilingualString("Module.StmModuleFilter", "Module Filters"));
		public static readonly ModuleIdentifier ErrorReporting = new ModuleIdentifier(ModuleId.ErrorReporting, ResString.GetMultilingualString("Module.ErrorReporting", "Error Reports"));
		public static readonly ModuleIdentifier LDaaSDevices = new ModuleIdentifier(ModuleId.LDaaSDevices, ResString.GetMultilingualString("Module.LDaaSDevices", "Mobile/Telematics (LDaaS) Devices"));
		public static readonly ModuleIdentifier TelematicsPreDriveChecklists = new ModuleIdentifier(ModuleId.TelematicsPreDriveChecklists, ResString.GetMultilingualString("Module.TelematicsPreDriveChecklists", "Telematics Pre-Drive Declaration"));
		public static readonly ModuleIdentifier TelematicsPreDriveChecklistTemplates = new ModuleIdentifier(ModuleId.TelematicsPreDriveChecklistTemplates, ResString.GetMultilingualString("Module.TelematicsPreDriveChecklistTemplates", "Telematics Pre-Drive Declaration Templates"));
		public static readonly ModuleIdentifier AdministrationPanel = new ModuleIdentifier(ModuleId.AdministrationPanel, ResString.GetMultilingualString("Module.AdministrationPanel", "MDM Administration"));
		public static readonly ModuleIdentifier RefShippingLine = new ModuleIdentifier(ModuleId.RefShippingLine, ResString.GetMultilingualString("Module.RefShippingLine", "Shipping Lines"));
		public static readonly ModuleIdentifier RefFacility = new ModuleIdentifier(ModuleId.RefFacility, ResString.GetMultilingualString("Module.RefFacility", "Facilities"));
		public static readonly ModuleIdentifier RefComplianceList = new ModuleIdentifier(ModuleId.RefComplianceList, ResString.GetMultilingualString("Module.RefComplianceList", "Compliance Lists (Party Screening)"));
		public static readonly ModuleIdentifier RefComplianceCommodityAlert = new ModuleIdentifier(ModuleId.RefComplianceCommodityAlert, ResString.GetMultilingualString("Module.RefComplianceCommodityAlert", "Compliance Lists (Commodities)"));
		public static readonly ModuleIdentifier ExchangeRate = new ModuleIdentifier(ModuleId.ExchangeRate, ResString.GetMultilingualString("Module.ExchangeRate", "Exchange Rates"));
		// barcode parsing
		public static readonly ModuleIdentifier BarcodeParsing = new ModuleIdentifier(ModuleId.BarcodeParsing, ResString.GetMultilingualString("Module.BarcodeParsing", "Barcode Parsing"));

		// barcode validation
		public static readonly ModuleIdentifier BarcodeValidation = new ModuleIdentifier(ModuleId.BarcodeValidation, ResString.GetMultilingualString("Module.BarcodeValidation", "Barcode Validation"));

		// Carrier Messaging Buss
		public static readonly ModuleIdentifier RefAccessorial = new ModuleIdentifier(ModuleId.RefAccessorial, ResString.GetMultilingualString("Module.RefAccessorial", "Service Options (Domestic)"));
		public static readonly ModuleIdentifier RefMessagingBussCarrierInfo = new ModuleIdentifier(ModuleId.RefMessagingBussCarrierInfo, ResString.GetMultilingualString("Module.RefMessagingBussCarrierInfo", "Carriers (Domestic)"));

		// Equipment Combination
		public static readonly ModuleIdentifier RefJobEquipment = new ModuleIdentifier(ModuleId.RefJobEquipment, ResString.GetMultilingualString("Module.RefJobEquipment", "Equipment Combination"));

		// domestic transport booking
		public static readonly ModuleIdentifier DtbBooking = new ModuleIdentifier(ModuleId.DtbBooking, ResString.GetMultilingualString("Module.DtbBooking", "Bookings"), ResString.GetMultilingualString("Module.DtbBookingExtended", "Bookings (Transport)"));
		public static readonly ModuleIdentifier DtbBookingConsolidation = new ModuleIdentifier(ModuleId.DtbBookingConsolidation, ResString.GetMultilingualString("Module.DtbBookingConsolidation", "Consolidated Bookings"));
		public static readonly ModuleIdentifier DtbBookingTmpl = new ModuleIdentifier(ModuleId.DtbBookingTmpl, ResString.GetMultilingualString("Module.DtbBookingTmpl", "Transport Booking Templates"));
		public static readonly ModuleIdentifier DtbBookingReports = new ModuleIdentifier(ModuleId.DtbBookingReports, ResString.GetMultilingualString("Module.DtbBookingReports", "Reports"), ResString.GetMultilingualString("Module.DtbBookingReportsExtended", "Reports (Transport Bookings)"));

		// land transport consignment
		public static readonly ModuleIdentifier DtbConsignment = new ModuleIdentifier(ModuleId.DtbConsignment, ResString.GetMultilingualString("Module.DtbConsignment", "Consignments"));

		// domestic transport consignment
		public static readonly ModuleIdentifier DtbBookingConsignment = new ModuleIdentifier(ModuleId.DtbBookingConsignment, ResString.GetMultilingualString("Module.DtbBookingConsignment", "Booking Consignments"));
		public static readonly ModuleIdentifier DtbConsignmentRunSheet = new ModuleIdentifier(ModuleId.DtbConsignmentRunSheet, ResString.GetMultilingualString("Module.DtbConsignmentRunSheet", "Run Sheets"), ResString.GetMultilingualString("Module.DtbConsignmentRunSheetExtended", "Run Sheets (Land Transport)"));
		public static readonly ModuleIdentifier DtbRoutePlanner = new ModuleIdentifier(ModuleId.DtbRoutePlanner, ResString.GetMultilingualString("Module.DtbRoutePlanner", "Route Planner"));
		public static readonly ModuleIdentifier DtbReports = new ModuleIdentifier(ModuleId.DtbReports, ResString.GetMultilingualString("Module.DtbReports", "Reports"), ResString.GetMultilingualString("Module.DtbReportsExtended", "Reports (Land Transport)"));
		public static readonly ModuleIdentifier DtbConsignmentWebPortal = new ModuleIdentifier(ModuleId.DtbConsignmentWebPortal, ResString.GetMultilingualString("Module.DtbConsignmentWebPortal", "Consignment (Web Portal)"));
		public static readonly ModuleIdentifier DtbConsignmentRunSheetWebPortal = new ModuleIdentifier(ModuleId.DtbConsignmentRunSheetWebPortal, ResString.GetMultilingualString("Module.DtbConsignmentRunSheetWebPortal", "Run Sheet (Web Portal)"));

		// Product Warehouse
		public static readonly ModuleIdentifier WhsConfigWarehouse = new ModuleIdentifier(ModuleId.WhsConfigWarehouse, ResString.GetMultilingualString("Module.WhsConfigWarehouse", "Warehouses"));
		public static readonly ModuleIdentifier WhsConfigRow = new ModuleIdentifier(ModuleId.WhsConfigRow, ResString.GetMultilingualString("Module.WhsConfigRow", "Locations"));
		public static readonly ModuleIdentifier WhsConfigLocation = new ModuleIdentifier(ModuleId.WhsConfigLocation, ResString.GetMultilingualString("Module.WhsConfigLocation", "Locations New"));
		public static readonly ModuleIdentifier WhsConfigLocationType = new ModuleIdentifier(ModuleId.WhsConfigLocationType, ResString.GetMultilingualString("Module.WhsConfigLocationType", "Location Types"));
		public static readonly ModuleIdentifier WhsConfigArea = new ModuleIdentifier(ModuleId.WhsConfigArea, ResString.GetMultilingualString("Module.WhsConfigArea", "Areas"));
		public static readonly ModuleIdentifier WhsConfigDynamicPickFaces = new ModuleIdentifier(ModuleId.WhsConfigDynamicPickFaces, ResString.GetMultilingualString("Module.WhsConfigDynamicPickFaces", "Dynamic Pick Faces"));
		public static readonly ModuleIdentifier WhsConfigPickFaces = new ModuleIdentifier(ModuleId.WhsConfigPickFaces, ResString.GetMultilingualString("Module.WhsConfigPickFaces", "Fixed Pick Faces"));
		public static readonly ModuleIdentifier WhsConfigProduct = new ModuleIdentifier(ModuleId.WhsConfigProduct, ResString.GetMultilingualString("Module.WhsConfigProduct", "Products"), ResString.GetMultilingualString("Module.WhsConfigProductExtended", "Products (Warehouse)"));
		public static readonly ModuleIdentifier WhsConfigProductStyle = new ModuleIdentifier(ModuleId.WhsConfigProductStyle, ResString.GetMultilingualString("Module.WhsConfigProductStyle", "Product Styles"));
		public static readonly ModuleIdentifier WhsConfigPutawayGroup = new ModuleIdentifier(ModuleId.WhsConfigPutawayGroup, ResString.GetMultilingualString("Module.WhsConfigPutawayGroup", "Putaway Groups"));
		public static readonly ModuleIdentifier WhsReceive = new ModuleIdentifier(ModuleId.WhsReceive, ResString.GetMultilingualString("Module.WhsReceive", "Receive"));
		public static readonly ModuleIdentifier WhsOrder = new ModuleIdentifier(ModuleId.WhsOrder, ResString.GetMultilingualString("Module.WhsOrder", "Orders"), ResString.GetMultilingualString("Module.WhsOrderExtended", "Orders (Warehouse)"));
		public static readonly ModuleIdentifier WhsOrderLine = new ModuleIdentifier(ModuleId.WhsOrderLine, ResString.GetMultilingualString("Module.WhsOrderLine", "Order Lines"), ResString.GetMultilingualString("Module.WhsOrderLineExtended", "Order Lines (Warehouse)"));
		public static readonly ModuleIdentifier WhsWorkOrder = new ModuleIdentifier(ModuleId.WhsWorkOrder, ResString.GetMultilingualString("Module.WhsWorkOrder", "Work Orders"));
		public static readonly ModuleIdentifier WhsDynamicWorkOrder = new ModuleIdentifier(ModuleId.WhsDynamicWorkOrder, ResString.GetMultilingualString("Module.WhsDynamicWorkOrder", "Dynamic Work Orders"));
		public static readonly ModuleIdentifier WhsPicking = new ModuleIdentifier(ModuleId.WhsPicking, ResString.GetMultilingualString("Module.WhsPicking", "Picking"));
		public static readonly ModuleIdentifier WhsRelease = new ModuleIdentifier(ModuleId.WhsRelease, ResString.GetMultilingualString("Module.WhsRelease", "Release"));
		public static readonly ModuleIdentifier WhsReleasePackageJob = new ModuleIdentifier(ModuleId.WhsReleasePackageJob, ResString.GetMultilingualString("Module.WhsReleasePackageJob", "Release Package Job"));
		public static readonly ModuleIdentifier WhsTransfer = new ModuleIdentifier(ModuleId.WhsTransfer, ResString.GetMultilingualString("Module.WhsTransfer", "Transfers"), ResString.GetMultilingualString("Module.WhsTransferExtended", "Transfers (Warehouse)"));
		public static readonly ModuleIdentifier WhsAdjustment = new ModuleIdentifier(ModuleId.WhsAdjustment, ResString.GetMultilingualString("Module.WhsAdjustment", "Adjustments"));
		public static readonly ModuleIdentifier WhsStocktake = new ModuleIdentifier(ModuleId.WhsStocktake, ResString.GetMultilingualString("Module.WhsStocktake", "Legacy Stocktake"));
		public static readonly ModuleIdentifier WhsStocktakeLine = new ModuleIdentifier(ModuleId.WhsStocktakeLine, ResString.GetMultilingualString("Module.WhsStocktakeLine", "Stocktake Line"));
		public static readonly ModuleIdentifier WhsHandlingUnit = new ModuleIdentifier(ModuleId.WhsHandlingUnit, ResString.GetMultilingualString("Module.WhsHandlingUnit", "Handling Unit"), ResString.GetMultilingualString("Module.WhsHandlingUnitExtended", "Handling Unit (Warehouse)"));
		public static readonly ModuleIdentifier WhsInventory = new ModuleIdentifier(ModuleId.WhsInventory, ResString.GetMultilingualString("Module.WhsInventory", "Inventory"));
		public static readonly ModuleIdentifier WhsInventoryHeldCodes = new ModuleIdentifier(ModuleId.WhsInventoryHeldCodes, ResString.GetMultilingualString("Module.WhsInventoryHeldCodes", "Inventory Hold Codes"));
		public static readonly ModuleIdentifier WhsReport = new ModuleIdentifier(ModuleId.WhsReport, ResString.GetMultilingualString("Module.WhsReport", "Reports"), ResString.GetMultilingualString("Module.WhsReportExtended", "Reports (Warehouse)"));
		public static readonly ModuleIdentifier WhsInvoicing = new ModuleIdentifier(ModuleId.WhsInvoicing, ResString.GetMultilingualString("Module.WhsInvoicing", "Periodic Billing"));
		public static readonly ModuleIdentifier WhsEntryLine = new ModuleIdentifier(ModuleId.WhsEntryLine, ResString.GetMultilingualString("Module.WhsEntryLine", "Entry Line"), ResString.GetMultilingualString("Module.WhsEntryLineExtended", "Entry Line (Warehouse)"));
		public static readonly ModuleIdentifier WhsPickLine = new ModuleIdentifier(ModuleId.WhsPickLine, ResString.GetMultilingualString("Module.WhsPickLine", "Cross-Dock Allocations"));
		public static readonly ModuleIdentifier WhsVASOrder = new ModuleIdentifier(ModuleId.WhsVASOrder, ResString.GetMultilingualString("Module.WhsVASOrder", "VAS Orders"));
		public static readonly ModuleIdentifier WhsCartonSize = new ModuleIdentifier(ModuleId.WhsCartonSize, ResString.GetMultilingualString("Module.WhsCartonSize", "Carton Size"));
		public static readonly ModuleIdentifier WhsCartonGroup = new ModuleIdentifier(ModuleId.WhsCartonGroup, ResString.GetMultilingualString("Module.WhsCartonGroup", "Carton Group"));
		public static readonly ModuleIdentifier WhsAdHocServiceJob = new ModuleIdentifier(ModuleId.WhsAdHocServiceJob, ResString.GetMultilingualString("Module.WhsAdHocServiceJob", "Ad Hoc Service Jobs"));
		public static readonly ModuleIdentifier WhsItemReceiveTransportationUnit = new ModuleIdentifier(ModuleId.WhsItemReceiveTransportationUnit, ResString.GetMultilingualString("Module.WhsItemReceiveTransportationUnit", "Receive Transportation Unit"));
		public static readonly ModuleIdentifier WhsTransitReport = new ModuleIdentifier(ModuleId.WhsTransitReport, ResString.GetMultilingualString("Module.WhsTransitReport", "Reports"), ResString.GetMultilingualString("Module.WhsTransitReportExtended", "Reports (Transit Warehouse)"));
		public static readonly ModuleIdentifier WhsTransitReceiveConsignment = new ModuleIdentifier(ModuleId.WhsTransitReceiveConsignment, ResString.GetMultilingualString("Module.WhsTransitReceiveConsignment", "Receive Consignment"));
		public static readonly ModuleIdentifier WhsTransitDispatchConsignment = new ModuleIdentifier(ModuleId.WhsTransitDispatchConsignment, ResString.GetMultilingualString("Module.WhsTransitDispatchConsignment", "Dispatch Consignment"));
		public static readonly ModuleIdentifier WhsItemDispatchTransportationUnit = new ModuleIdentifier(ModuleId.WhsItemDispatchTransportationUnit, ResString.GetMultilingualString("Module.WhsItemDispatchTransportationUnit", "Dispatch Transportation Unit"));
		public static readonly ModuleIdentifier WhsItemReceiveASN = new ModuleIdentifier(ModuleId.WhsItemReceiveASN, ResString.GetMultilingualString("Module.WhsItemReceiveASN", "Receive ASN"));
		public static readonly ModuleIdentifier TransitHandlingUnit = new ModuleIdentifier(ModuleId.TransitHandlingUnit, ResString.GetMultilingualString("Module.TransitHandlingUnit", "Handling Unit"));
		public static readonly ModuleIdentifier WhsSalesChannel = new ModuleIdentifier(ModuleId.WhsSalesChannel, ResString.GetMultilingualString("Module.WhsSalesChannel", "Sales Channel"));
		public static readonly ModuleIdentifier WhsProductionRulesPortal = new ModuleIdentifier(ModuleId.WhsProductionRulesPortal, ResString.GetMultilingualString("Module.WhsProductionRulesPortal", "Production Rules Management Portal"), ResString.GetMultilingualString("Module.WhsProductionRulesPortalExtended", "Production Rules Management Portal (Transit Warehouse)"));
		public static readonly ModuleIdentifier WhsProductWarehousePortal = new ModuleIdentifier(ModuleId.WhsProductWarehousePortal, ResString.GetMultilingualString("Module.WhsProductWarehousePortal", "Product Warehouse Management Portal"));
		public static readonly ModuleIdentifier WhsLoad = new ModuleIdentifier(ModuleId.WhsLoad, ResString.GetMultilingualString("Module.WhsLoad", "Load Planning"));

		//Transit Warehouse
		public static readonly ModuleIdentifier TransitWarehouseAttachPackages = new ModuleIdentifier(ModuleId.TransitWarehouseAttachPackages, ResString.GetMultilingualString("Module.TransitWarehouseAttachPackages", "Transit Packages"));
		public static readonly ModuleIdentifier TransitWarehouseViewPackages = new ModuleIdentifier(ModuleId.TransitWarehouseViewPackages, ResString.GetMultilingualString("Module.TransitWarehouseViewPackages", "Transit Packages"));
		public static readonly ModuleIdentifier WhsItemTransferHeader = new ModuleIdentifier(ModuleId.WhsItemTransferHeader, ResString.GetMultilingualString("Module.WhsItemTransferHeader", "Transfers"), ResString.GetMultilingualString("Module.WhsItemTransferHeaderExtended", "Transfers (Transit Warehouse)"));
		public static readonly ModuleIdentifier WhsItemDispatchLoadList = new ModuleIdentifier(ModuleId.WhsItemDispatchLoadList, ResString.GetMultilingualString("Module.WhsItemDispatchLoadList", "Dispatch Load List"));
		public static readonly ModuleIdentifier TransitWarehousePortal = new ModuleIdentifier(ModuleId.TransitWarehousePortal, ResString.GetMultilingualString("Module.TransitWarehousePortal", "Transit Warehouse Management Portal"));

		// Packing
		public static readonly ModuleIdentifier Packing = new ModuleIdentifier(ModuleId.Packing, ResString.GetMultilingualString("Module.Packing", "Packing"));
		public static readonly ModuleIdentifier PalletTransaction = new ModuleIdentifier(ModuleId.PalletTransaction, ResString.GetMultilingualString("Module.PalletTransaction", "Pallet Transactions"));

		// Agency
		public static readonly ModuleIdentifier AgencyBooking = new ModuleIdentifier(ModuleId.AgencyBooking, ResString.GetMultilingualString("Module.AgencyBooking", "Bookings"), ResString.GetMultilingualString("Module.AgencyBookingExtended", "Bookings (Agency)"));
		public static readonly ModuleIdentifier AgencyBillOfLading = new ModuleIdentifier(ModuleId.AgencyBillOfLading, ResString.GetMultilingualString("Module.AgencyBillOfLading", "Bills of Lading"));
		public static readonly ModuleIdentifier AgencyBillContainers = new ModuleIdentifier(ModuleId.AgencyBillContainers, ResString.GetMultilingualString("Module.AgencyBillContainers", "Bill of Lading Containers"));
		public static readonly ModuleIdentifier AgencyContainerDetention = new ModuleIdentifier(ModuleId.AgencyContainerDetention, ResString.GetMultilingualString("Module.AgencyContainerDetention", "Container Detention"));
		public static readonly ModuleIdentifier AgencyContainerManager = new ModuleIdentifier(ModuleId.AgencyContainerManager, ResString.GetMultilingualString("Module.AgencyContainerManager", "Container Manager"));
		public static readonly ModuleIdentifier AgencyContainerMove = new ModuleIdentifier(ModuleId.AgencyContainerMove, ResString.GetMultilingualString("Module.AgencyContainerMove", "Container Movements"));
		public static readonly ModuleIdentifier AgencyVoyageAccounting = new ModuleIdentifier(ModuleId.AgencyVoyageAccounting, ResString.GetMultilingualString("Module.AgencyVoyageAccounting", "Voyage Accounting"));
		public static readonly ModuleIdentifier AgencySundryCharges = new ModuleIdentifier(ModuleId.AgencySundryCharges, ResString.GetMultilingualString("Module.AgencySundryCharges", "Sundry Charges"));
		public static readonly ModuleIdentifier AgencyReports = new ModuleIdentifier(ModuleId.AgencyReports, ResString.GetMultilingualString("Module.AgencyReports", "Reports"), ResString.GetMultilingualString("Module.AgencyReportsExtended", "Reports (Agency)"));

		// Ocean Carrier
		public static readonly ModuleIdentifier OceanCarrierPortal = new ModuleIdentifier(ModuleId.OceanCarrierPortal, ResString.GetMultilingualString("Module.OceanCarrierPortal", "Ocean Carrier Portal"));
		public static readonly ModuleIdentifier CarrierShipmentHeader = new ModuleIdentifier(ModuleId.CarrierShipmentHeader, ResString.GetMultilingualString("Module.CarrierShipmentHeader", "Carrier Shipments"));
		public static readonly ModuleIdentifier RouteSegments = new ModuleIdentifier(ModuleId.RouteSegments, ResString.GetMultilingualString("Module.RouteSegments", "Route Segments"));
		public static readonly ModuleIdentifier CarrierServices = new ModuleIdentifier(ModuleId.CarrierServices, ResString.GetMultilingualString("Module.CarrierServices", "Carrier Services"));

		// Equipment Management
		public static readonly ModuleIdentifier EquipmentManagementPortal = new ModuleIdentifier(ModuleId.EquipmentManagementPortal, ResString.GetMultilingualString("Module.EquipmentManagementPortal", "Equipment Management Portal"));

		// ResourceStrings
		public static readonly ModuleIdentifier LocalLanguages = new ModuleIdentifier(ModuleId.LocalLanguages, ResString.GetMultilingualString("Module.LocalLanguages", "Local Languages"));
		public static readonly ModuleIdentifier ResourceStrings = new ModuleIdentifier(ModuleId.ResourceStrings, ResString.GetMultilingualString("Module.ResourceStrings", "Resource Strings"));
		public static readonly ModuleIdentifier TranslationFeedback = new ModuleIdentifier(ModuleId.TranslationFeedback, ResString.GetMultilingualString("Module.TranslationFeedback", "Translation Feedback"));

		public static readonly ModuleIdentifier GlowHRMS = new ModuleIdentifier(ModuleId.GlowHRMS, ResString.GetMultilingualString("Module.GlowHRMS", "HRMS Web Portal"));

		//Recruiter
		public static readonly ModuleIdentifier HRJobApplicant = new ModuleIdentifier(ModuleId.HRJobApplicant, ResString.GetMultilingualString("Module.HRJobApplicant", "Job Applicant"));
		public static readonly ModuleIdentifier HRJobRole = new ModuleIdentifier(ModuleId.HRJobRole, ResString.GetMultilingualString("Module.HRJobRole", "Job Role"));
		public static readonly ModuleIdentifier HRJobOpenings = new ModuleIdentifier(ModuleId.HRJobOpenings, ResString.GetMultilingualString("Module.HRJobOpenings", "Job Openings"));
		public static readonly ModuleIdentifier LearningCentreCampaign = new ModuleIdentifier(ModuleId.LearningCentreCampaign, ResString.GetMultilingualString("Module.LearningCentreCampaign", "Learning Center"));
		public static readonly ModuleIdentifier HRGlbCompanyCampaign = new ModuleIdentifier(ModuleId.HRGlbCompanyCampaign, ResString.GetMultilingualString("Module.HRGlbCompanyCampaign", "HR Campaign Management"));
		public static readonly ModuleIdentifier HRGlbCompanyCampaignContact = new ModuleIdentifier(ModuleId.HRGlbCompanyCampaignContact, ResString.GetMultilingualString("GUI.HRGlbCompanyCampaignContact", "HR Campaign contact module filters"));
		public static readonly ModuleIdentifier HRReports = new ModuleIdentifier(ModuleId.HRReports, ResString.GetMultilingualString("Module.HRReports", "Reports"), ResString.GetMultilingualString("Module.HRReportsExtended", "Reports (HR)"));
		public static readonly ModuleIdentifier GlbAccreditation = new ModuleIdentifier(ModuleId.GlbAccreditation, ResString.GetMultilingualString("Module.GlbAccreditation", "Accreditation Requirements"));
		public static readonly ModuleIdentifier GlbAccreditationAttempt = new ModuleIdentifier(ModuleId.GlbAccreditationAttempt, ResString.GetMultilingualString("Module.GlbAccreditationAttempt", "Accreditation Attempts"));
		public static readonly ModuleIdentifier GlbAccreditationGroup = new ModuleIdentifier(ModuleId.GlbAccreditationGroup, ResString.GetMultilingualString("Module.GlbAccreditationGroup", "Accreditation Program"));
		public static readonly ModuleIdentifier HRJobApplication = new ModuleIdentifier(ModuleId.HRJobApplication, ResString.GetMultilingualString("Module.HRJobApplication", "Job Application"));
		public static readonly ModuleIdentifier ExamSetting = new ModuleIdentifier(ModuleId.ExamSetting, ResString.GetMultilingualString("Module.ExamSetting", "Exam Settings"));
		public static readonly ModuleIdentifier HREmails = new ModuleIdentifier(ModuleId.HREmails, ResString.GetMultilingualString("Module.HREmails", "HR Emails"));
		public static readonly ModuleIdentifier HRHiringRequest = new ModuleIdentifier(ModuleId.HRHiringRequest, ResString.GetMultilingualString("Module.HRHiringRequest", "HR Hiring Request"));
		public static readonly ModuleIdentifier HROnBoarding = new ModuleIdentifier(ModuleId.HROnBoarding, ResString.GetMultilingualString("Module.HROnBoarding", "HR Onboarding"));

		// Recruitment
		public static readonly ModuleIdentifier RecruitmentCandidateManagement = new ModuleIdentifier(ModuleId.RecruitmentCandidateManagement, ResString.GetMultilingualString("Module.RecruitmentCandidateManagement", "Candidate Management"));

		// Process Manager
		public static readonly ModuleIdentifier ProcessTasks = new ModuleIdentifier(ModuleId.ProcessTasks, ResString.GetMultilingualString("Module.ProcessTasks", "Task List"));
		public static readonly ModuleIdentifier ProcessHeader = new ModuleIdentifier(ModuleId.ProcessHeader, ResString.GetMultilingualString("Module.ProcessHeader", "Job Workflows"));
		public static readonly ModuleIdentifier ProcessHeaderLink = new ModuleIdentifier(ModuleId.ProcessHeaderLink, ResString.GetMultilingualString("Module.ProcessHeaderLink", "Workflow Dependencies"));
		public static readonly ModuleIdentifier ProcessTemplates = new ModuleIdentifier(ModuleId.ProcessTemplates, ResString.GetMultilingualString("Module.ProcessTemplates", "Workflow Templates"));
		public static readonly ModuleIdentifier ProcessCompanyLinkRule = new ModuleIdentifier(ModuleId.ProcessCompanyLinkRule, ResString.GetMultilingualString("Module.ProcessCompanyLinkRule", "Template Company Rules"));
		public static readonly ModuleIdentifier ProcessFieldChangeRule = new ModuleIdentifier(ModuleId.ProcessFieldChangeRule, ResString.GetMultilingualString("Module.ProcessFieldChangeRule", "Field Change Events"));
		public static readonly ModuleIdentifier BMTagDefinition = new ModuleIdentifier(ModuleId.BMTagDefinition, ResString.GetMultilingualString("Module.TagDefinition", "Tag Groups"));
		public static readonly ModuleIdentifier BMTagMagnitude = new ModuleIdentifier(ModuleId.BMTagMagnitude, ResString.GetMultilingualString("Module.TagMagnitude", "Tag Magnitudes"));
		public static readonly ModuleIdentifier BMTagRule = new ModuleIdentifier(ModuleId.BMTagRule, ResString.GetMultilingualString("Module.TagRule", "Tag Rules"));
		public static readonly ModuleIdentifier AcceptabilityBand = new ModuleIdentifier(ModuleId.AcceptabilityBand, ResString.GetMultilingualString("Module.AcceptabilityBand", "Acceptability Bands"));
		public static readonly ModuleIdentifier BMFilterRule = new ModuleIdentifier(ModuleId.BMFilterRule, ResString.GetMultilingualString("Module.BMFilterRule", "Filter Rule"));
		public static readonly ModuleIdentifier Events = new ModuleIdentifier(ModuleId.Events, ResString.GetMultilingualString("Module.Events", "Events"));
		public static readonly ModuleIdentifier BMSystems = new ModuleIdentifier(ModuleId.BMSystems, ResString.GetMultilingualString("Module.BMSystems", "Buffer Management Systems"));
		public static readonly ModuleIdentifier BMBufferTimespan = new ModuleIdentifier(ModuleId.BMBufferTimespan, ResString.GetMultilingualString("Module.BMBufferTimespan", "Buffer Timespans"));
		public static readonly ModuleIdentifier BMBoard = new ModuleIdentifier(ModuleId.BMBoard, ResString.GetMultilingualString("Module.BMBoard", "Visual Boards"), ResString.GetMultilingualString("Module.BMBoardExtended", "Visual Boards (Buffer Management)"));
		public static readonly ModuleIdentifier VisualBoard = new ModuleIdentifier(ModuleId.VisualBoard, ResString.GetMultilingualString("Module.VisualBoard", "Visual Boards"));
		public static readonly ModuleIdentifier BMBoardSlideshow = new ModuleIdentifier(ModuleId.BMBoardSlideshows, ResString.GetMultilingualString("Module.BMBoardSlideshows", "Visual Board Slide Shows"));
		public static readonly ModuleIdentifier BMReports = new ModuleIdentifier(ModuleId.BMReports, ResString.GetMultilingualString("Module.BMReports", "Reports"), ResString.GetMultilingualString("Module.BMReportsExtended", "Reports (Buffer Management)"));
		public static readonly ModuleIdentifier BMComponent = new ModuleIdentifier(ModuleId.BMComponent, ResString.GetMultilingualString("Module.BMComponent", "Buffer Management System Component"));
		public static readonly ModuleIdentifier ComponentRelationship = new ModuleIdentifier(ModuleId.ComponentRelationship, ResString.GetMultilingualString("Module.ComponentRelationship", "Component Relationships"));
		public static readonly ModuleIdentifier ViewComponentChangeLog = new ModuleIdentifier(ModuleId.ViewComponentChangeLog, ResString.GetMultilingualString("Module.ViewComponentChangeLog", "Component Change Logs"));
		public static readonly ModuleIdentifier BMControlCustomisation = new ModuleIdentifier(ModuleId.BMControlCustomisation, ResString.GetMultilingualString("Module.BMControlCustomisation", "Customized Visual Layouts"));
		public static readonly ModuleIdentifier BMReleaseSequence = new ModuleIdentifier(ModuleId.BMReleaseSequence, ResString.GetMultilingualString("Module.BMReleaseSequence", "Release Sequences"));
		public static readonly ModuleIdentifier NetworkDiagram = new ModuleIdentifier(ModuleId.NetworkDiagram, ResString.GetMultilingualString("Module.NetworkDiagram", "Network Diagrams"));
		public static readonly ModuleIdentifier WorkQueues = new ModuleIdentifier(ModuleId.WorkQueues, ResString.GetMultilingualString("Module.WorkQueues", "Work Queues"));
		public static readonly ModuleIdentifier CompletionTriggerAction = new ModuleIdentifier(ModuleId.CompletionTriggerAction, ResString.GetMultilingualString("Module.CompletionTriggerAction", "Completion Trigger Actions"));
		public static readonly ModuleIdentifier WorkflowExceptions = new ModuleIdentifier(ModuleId.WorkflowExceptions, ResString.GetMultilingualString("Module.WorkflowExceptions", "Exceptions"));
		public static readonly ModuleIdentifier WorkflowExceptionTypes = new ModuleIdentifier(ModuleId.WorkflowExceptionTypes, ResString.GetMultilingualString("Module.WorkflowExceptionTypes", "Exception Types"));
		public static readonly ModuleIdentifier WorkflowMilestones = new ModuleIdentifier(ModuleId.WorkflowMilestones, ResString.GetMultilingualString("Module.WorkflowMilestones", "Milestones"));
		public static readonly ModuleIdentifier WorkflowTriggers = new ModuleIdentifier(ModuleId.WorkflowTriggers, ResString.GetMultilingualString("Module.WorkflowTriggers", "Triggers"));
		public static readonly ModuleIdentifier MENTAgedScoreQuery = new ModuleIdentifier(ModuleId.MENTAgedScoreQuery, ResString.GetMultilingualString("Module.MENTAgedScoreQuery", "Measurement of Employee Normalized Throughput"));
		public static readonly ModuleIdentifier MENTAgedScoreExtraction = new ModuleIdentifier(ModuleId.MENTAgedScoreExtraction, ResString.GetMultilingualString("Module.MENTAgedScoreExtraction", "Extractions"));
		public static readonly ModuleIdentifier MENTSeries = new ModuleIdentifier(ModuleId.MENTSeries, ResString.GetMultilingualString("Module.MENTSeries", "MENT Series"));
		public static readonly ModuleIdentifier ExternalRequestTypes = new ModuleIdentifier(ModuleId.ExternalRequestTypes, ResString.GetMultilingualString("Module.ExternalRequestTypes", "Request Types"));
		public static readonly ModuleIdentifier ExternalRequestInfoTemplate = new ModuleIdentifier(ModuleId.ExternalRequestInfoTemplate, ResString.GetMultilingualString("Module.ExternalRequestInfoTemplate", "Request Template"));
		public static readonly ModuleIdentifier ExternalRequests = new ModuleIdentifier(ModuleId.ExternalRequests, ResString.GetMultilingualString("Module.ExternalRequests", "Requests"));

		// Archive Manager
		public static readonly ModuleIdentifier ArchiveSchedule = new ModuleIdentifier(ModuleId.ArchiveSchedule, ResString.GetMultilingualString("Module.ArchiveSchedule", "Archive Schedule"));
		public static readonly ModuleIdentifier ArchivedRecords = new ModuleIdentifier(ModuleId.ArchivedRecords, ResString.GetMultilingualString("Module.ArchivedRecords", "Archived Records"));

		// Process Management
		public static readonly ModuleIdentifier WorkItem = new ModuleIdentifier(ModuleId.WorkItem, ResString.GetMultilingualString("Module.WorkItem", "Work Items"));
		public static readonly ModuleIdentifier Project = new ModuleIdentifier(ModuleId.Project, ResString.GetMultilingualString("Module.Project", "Projects"));
		public static readonly ModuleIdentifier CustomerServiceTicket = new ModuleIdentifier(ModuleId.CustomerServiceTicket, ResString.GetMultilingualString("Module.CustomerServiceTicket", "Customer Service Tickets"));

		#region Customs

		#region Shared

		public static readonly ModuleIdentifier SingleTariffClassification = new ModuleIdentifier(ModuleId.Classification, ResString.GetMultilingualString("Module.Classification", "Classification Lookup"));
		public static readonly ModuleIdentifier TariffBulkChange = new ModuleIdentifier(ModuleId.TariffBulkChange, ResString.GetMultilingualString("Module.TariffBulkChange", "Tariff Bulk Change"));
		public static readonly ModuleIdentifier SupplierPart = new ModuleIdentifier(ModuleId.SupplierPart, ResString.GetMultilingualString("Module.SupplierPart", "Products"), ResString.GetMultilingualString("Module.SupplierPartExtended", "Products (Customs)"));
		public static readonly ModuleIdentifier EntryLine = new ModuleIdentifier(ModuleId.EntryLine, ResString.GetMultilingualString("Module.EntryLine", "Entry Line"), ResString.GetMultilingualString("Module.EntryLineExtended", "Entry Line (Customs)"));
		public static readonly ModuleIdentifier ImportClassification = new ModuleIdentifier(ModuleId.ImportClassification, ResString.GetMultilingualString("Module.ImportClassification", "Import Classification Lookup"));
		public static readonly ModuleIdentifier ExportClassification = new ModuleIdentifier(ModuleId.ExportClassification, ResString.GetMultilingualString("Module.ExportClassification", "Export Classification Lookup"));
		public static readonly ModuleIdentifier ImporterSecurityFiling = new ModuleIdentifier(ModuleId.ImporterSecurityFiling, ResString.GetMultilingualString("Module.ImporterSecurityFiling", "US Importer Security Filing"));
		public static readonly ModuleIdentifier SendTestCustomsMessage = new ModuleIdentifier(ModuleId.SendTestCustomsMessage, ResString.GetMultilingualString("Module.SendTestCustomsMessage", "Message Diagnostic Test"));

		public static readonly ModuleIdentifier CusPerson = new ModuleIdentifier(ModuleId.CusPerson, (NoResString)"Persons");

		public static readonly ModuleIdentifier BorderWiseWebReturnHook = new ModuleIdentifier(ModuleId.BorderWiseWebReturnHook, ResString.GetMultilingualString("Module.BorderWiseWebReturnHook", "BorderWise Web"));

		#endregion

		#region AU

		public static readonly ModuleIdentifier ImportTariffBulkChange = new ModuleIdentifier(ModuleId.AUImportTariffBulkChange, ResString.GetMultilingualString("ModuleId.AUImportTariffBulkChange", "Import Tariff Bulk Change"));
		public static readonly ModuleIdentifier ExportTariffBulkChange = new ModuleIdentifier(ModuleId.AUExportTariffBulkChange, ResString.GetMultilingualString("ModuleId.AUExportTariffBulkChange", "Export Tariff Bulk Change"));
		public static readonly ModuleIdentifier AQISProducerCode = new ModuleIdentifier(ModuleId.AQISProducerCode, ResString.GetMultilingualString("ModuleId.AQISProducerCode", "Quarantine Producer Code"));
		public static readonly ModuleIdentifier Premises = new ModuleIdentifier(ModuleId.Premises, ResString.GetMultilingualString("ModuleId.Premises", "Premises"));
		public static readonly ModuleIdentifier CMRCodeLists = new ModuleIdentifier(ModuleId.CMRCodeLists, ResString.GetMultilingualString("ModuleId.CMRCodeLists", "CMR Code Lists"));
		public static readonly ModuleIdentifier InstrumentNumber = new ModuleIdentifier(ModuleId.InstrumentNumber, ResString.GetMultilingualString("ModuleId.InstrumentNumber", "Instrument Number"));
		public static readonly ModuleIdentifier CMRLodgementQuestion = new ModuleIdentifier(ModuleId.CMRLodgementQuestion, ResString.GetMultilingualString("ModuleId.CMRLodgementQuestion", "Community Protection Question"));

		#endregion

		public static readonly ModuleIdentifier CommercialInvoice = new ModuleIdentifier(ModuleId.CommercialInvoice, ResString.GetMultilingualString("ModuleId.CommercialInvoice", "Commercial Invoice"));
		public static readonly ModuleIdentifier CopyCommercialInvoice = new ModuleIdentifier(ModuleId.CopyCommercialInvoice, ResString.GetMultilingualString("ModuleId.CopyCommercialInvoice", "Copy Commercial Invoice"));

		public static class Customs
		{
#if DEBUG
			public static readonly ModuleIdentifier NestedDummy = new ModuleIdentifier(ModuleId.NestedDummy, ResString.GetMultilingualString("ModuleId.NestedDummy", "Nested Dummy"));
#endif
			public static readonly ModuleIdentifier JobDeclaration = new ModuleIdentifier(ModuleId.CusDec, ResString.GetMultilingualString("ModuleId.CusDec|CustomsDeclarations", "Customs Declarations"));
			public static readonly ModuleIdentifier ImportCustomsFilesData = new ModuleIdentifier(ModuleId.ImportCustomsFilesData, ResString.GetMultilingualString("ModuleId.ImportCustomsFilesData", "Import Data"), ResString.GetMultilingualString("ModuleId.ImportCustomsFilesDataExtended", "Import Data (Customs)"));
			public static readonly ModuleIdentifier RefPacks = new ModuleIdentifier(ModuleId.RefPacks, ResString.GetMultilingualString("ModuleId.RefPacks", "Packs Conversion"));
			public static readonly ModuleIdentifier CusCalculationRules = new ModuleIdentifier(ModuleId.CusCalculationRules, ResString.GetMultilingualString("ModuleId.CusCalculationRules", "Customs Calculation Rules"));
			public static readonly ModuleIdentifier Guarantees = new ModuleIdentifier(ModuleId.Guarantees, ResString.GetMultilingualString("ModuleId.Guarantees", "Customs Guarantees"));
			public static readonly ModuleIdentifier CustomsRules = new ModuleIdentifier(ModuleId.CustomsRules, ResString.GetMultilingualString("ModuleId.CustomsRules", "Rules"));
			public static readonly ModuleIdentifier EntryHeader = new ModuleIdentifier(ModuleId.EntryHeader, ResString.GetMultilingualString("ModuleId.EntryHeader", "Customs Entries"));
			public static readonly ModuleIdentifier InvoiceLine = new ModuleIdentifier(ModuleId.InvoiceLine, ResString.GetMultilingualString("ModuleId.InvoiceLine", "Customs Invoice Line"));
			public static readonly ModuleIdentifier Permits = new ModuleIdentifier(ModuleId.Permits, ResString.GetMultilingualString("ModuleId.Permits", "Permits"));
			public static readonly ModuleIdentifier TemporaryStorage = new ModuleIdentifier(ModuleId.TemporaryStorage, ResString.GetMultilingualString("ModuleId.TemporaryStorage", "Temporary Storage"));
			public static readonly ModuleIdentifier CusAuthorisations = new ModuleIdentifier(ModuleId.CusAuthorisations, ResString.GetMultilingualString("ModuleId.Authorisations", "Authorizations"));
			public static readonly ModuleIdentifier CusRefPreference = new ModuleIdentifier(ModuleId.CusRefPreference, ResString.GetMultilingualString("ModuleId.CusRefPreference", "Preference Codes"));
			public static readonly ModuleIdentifier CusRefRateCode = new ModuleIdentifier(ModuleId.CusRefRateCode, ResString.GetMultilingualString("ModuleId.CusRefRateCode", "Rate Codes"));
			public static readonly ModuleIdentifier CusPackingList = new ModuleIdentifier(ModuleId.CusPackingList, ResString.GetMultilingualString("ModuleId.CusPackingList", "Customs Packing List"));
			public static readonly ModuleIdentifier TradeGroups = new ModuleIdentifier(ModuleId.TradeGroups, ResString.GetMultilingualString("ModuleId.TradeGroups", "Trade Groups"));
			public static readonly ModuleIdentifier CusRefTariffVersion = new ModuleIdentifier(ModuleId.CusRefTariffVersion, ResString.GetMultilingualString("ModuleId.CusRefTariffVersion", "Tariff Version"));
			public static readonly ModuleIdentifier ConsolidatedDeclaration = new ModuleIdentifier(ModuleId.ConsolidatedDeclaration, ResString.GetMultilingualString("ModuleId.ConsolidatedEntry", "Consolidated Entry"));
			public static readonly ModuleIdentifier GoodsCatalog = new ModuleIdentifier(ModuleId.GoodsCatalog, ResString.GetMultilingualString("ModuleId.GoodsCatalog", "Catalog"));
			public static readonly ModuleIdentifier SumARegister = new ModuleIdentifier(ModuleId.TemporaryStorageRegister, ResString.GetMultilingualString("ModuleId.Customs.SumARegister", "SumA Register"));
			public static readonly ModuleIdentifier SumARegisterReadOnly = new ModuleIdentifier(ModuleId.TemporaryStorageRegisterReadOnly, ResString.GetMultilingualString("ModuleId.Customs.SumARegisterReadOnly", "SumA Register Read Only"));
			public static readonly ModuleIdentifier ImportFromTemporaryStorageRegister = new ModuleIdentifier(ModuleId.ImportFromTemporaryStorageRegister, ResString.GetMultilingualString("ModuleId.Customs.ImportFromTemporaryStorageRegister", "Import from Temporary Storage Register"));

			public static class Universal
			{
				public static readonly ModuleIdentifier RefCusTariff = new ModuleIdentifier(ModuleId.RefCusTariff, ResString.GetMultilingualString("ModuleId.RefCusTariff", "Global Tariffs"));
				public static readonly ModuleIdentifier RefDataGrouping = new ModuleIdentifier(ModuleId.RefDataGrouping, ResString.GetMultilingualString("ModuleId.RefDataGrouping", "Global Country/Region Groups"));
				public static readonly ModuleIdentifier RefHarbourRate = new ModuleIdentifier(ModuleId.RefHarbourRate, ResString.GetMultilingualString("ModuleId.RefHarbourRate", "Global Harbor Rates"));
				public static readonly ModuleIdentifier RefCusTradeGroup = new ModuleIdentifier(ModuleId.RefCusTradeGroup, ResString.GetMultilingualString("ModuleId.RefCusTradeGroup", "Trade Group/Country"));
				public static readonly ModuleIdentifier ZZRefCarrier = new ModuleIdentifier(ModuleId.ZZRefCarrier, ResString.GetMultilingualString("ModuleId.ZZRefCarrier", "Global Carriers"));
				public static readonly ModuleIdentifier ZZRefCusCodeList = new ModuleIdentifier(ModuleId.ZZRefCusCodeList, ResString.GetMultilingualString("ModuleId.ZZRefCusCodeList", "Global Codes"));
				public static readonly ModuleIdentifier ZZRefCusMap = new ModuleIdentifier(ModuleId.ZZRefCusMap, ResString.GetMultilingualString("ModuleId.ZZRefCusMap", "Global Data Maps"));
				public static readonly ModuleIdentifier ZZRefCusProcedure = new ModuleIdentifier(ModuleId.ZZRefCusProcedure, ResString.GetMultilingualString("ModuleId.ZZRefCusProcedure", "Global Customs Procedure Codes"));
				public static readonly ModuleIdentifier ZZRefCusRuling = new ModuleIdentifier(ModuleId.ZZRefCusRuling, ResString.GetMultilingualString("ModuleId.ZZRefCusRuling", "Rulings"));
			}

			public static class ASYCUDA
			{
				public static readonly ModuleIdentifier Manifest = new ModuleIdentifier(ModuleId.Manifest, ResString.GetMultilingualString("ModuleId.Manifest", "Global Manifest"));
				public static readonly ModuleIdentifier ManifestBill = new ModuleIdentifier(ModuleId.ManifestBill, ResString.GetMultilingualString("Module.ASYCUDAManifest", "Global Manifest Bill"));

				public static readonly ModuleIdentifier PreBoardingNotification = new ModuleIdentifier(ModuleId.PreBoardingNotification, ResString.GetMultilingualString("Module.PreBoardingNotification", "PBN-Pre Boarding Notification"));

				public static class SGAccess
				{
					public static readonly ModuleIdentifier Manifest = new ModuleIdentifier(ModuleId.SGAccessManifest, ResString.GetMultilingualString("ModuleId.SGAccessManifest", "SG Access"));
					public static readonly ModuleIdentifier ManifestBill = new ModuleIdentifier(ModuleId.SGAccessManifestBill, ResString.GetMultilingualString("ModuleId.SGAccessManifestBill", "SG Access Bill"));
				}
			}

			public static class AU
			{
				public static readonly ModuleIdentifier AirCargo = new ModuleIdentifier(ModuleId.AUCustomsAirCargo, ResString.GetMultilingualString("ModuleId.AUCustomsAirCargo", "Air Cargo Report"));
				public static readonly ModuleIdentifier AirCargoDepot = new ModuleIdentifier(ModuleId.AirCargoDepotStandAlone, ResString.GetMultilingualString("ModuleId.AirCargoDepotStandAlone", "Air Cargo Depot"));
				public static readonly ModuleIdentifier AirCargoOutturnBills = new ModuleIdentifier(ModuleId.AirCargoOutturnBills, ResString.GetMultilingualString("ModuleId.AirCargoOutturnBills", "Air Cargo Outturn Bills"));
				public static readonly ModuleIdentifier CusSCADepotContainer = new ModuleIdentifier(ModuleId.CusSCADepotContainer, ResString.GetMultilingualString("ModuleId.CusSCADepotContainer", "Depot Container"));
				public static readonly ModuleIdentifier CusSCADepotHouse = new ModuleIdentifier(ModuleId.CusSCADepotHouse, ResString.GetMultilingualString("ModuleId.CusSCADepotHouse", "Depot Container"), ResString.GetMultilingualString("ModuleId.CusSCADepotHouseExtended", "Depot Container (House)"));
				public static readonly ModuleIdentifier HouseAirCargo = new ModuleIdentifier(ModuleId.AUCustomsHouseAirCargo, ResString.GetMultilingualString("ModuleId.AUCustomsHouseAirCargo", "Air Cargo House"));
				public static readonly ModuleIdentifier ExportCustomsManifest = new ModuleIdentifier(ModuleId.AUExportCustomsManifest, ResString.GetMultilingualString("ModuleId.AUExportCustomsManifest", "Customs Export Manifest"));
				public static readonly ModuleIdentifier AirCTOImport = new ModuleIdentifier(ModuleId.AUCustomsAirCTOImport, ResString.GetMultilingualString("ModuleId.AUCustomsAirCTOImport", "Air CTO - Import"));
				public static readonly ModuleIdentifier AirCTOExport = new ModuleIdentifier(ModuleId.AUCustomsAirCTOExport, ResString.GetMultilingualString("ModuleId.AUCustomsAirCTOExport", "Air CTO - Export"));
				public static readonly ModuleIdentifier VoyageManifest = new ModuleIdentifier(ModuleId.VoyageManifest, ResString.GetMultilingualString("ModuleId.VoyageManifest", "Customs Import Manifest"));
				public static readonly ModuleIdentifier SeaCargo = new ModuleIdentifier(ModuleId.SeaCargo, ResString.GetMultilingualString("ModuleId.SeaCargo", "Sea Cargo Report"));
				public static readonly ModuleIdentifier NexDocNotifications = new ModuleIdentifier(ModuleId.NexDocNotifications, ResString.GetMultilingualString("ModuleId.NexDocNotifications", "NEXDOC Notifications"));
				public static readonly ModuleIdentifier SeaCargoDepot = new ModuleIdentifier(ModuleId.SeaCargoDepot, ResString.GetMultilingualString("ModuleId.SeaCargoDepot", "Sea Cargo Outturn"));
				public static readonly ModuleIdentifier SeaCargoOutturnBills = new ModuleIdentifier(ModuleId.SeaCargoOutturnBills, ResString.GetMultilingualString("ModuleId.SeaCargoOutturnBills", "Sea Cargo Outturn Bills"));
				public static readonly ModuleIdentifier HouseSeaCargo = new ModuleIdentifier(ModuleId.AUCustomsHouseSeaCargo, ResString.GetMultilingualString("ModuleId.AUCustomsHouseSeaCargo", "Sea Cargo House"));
				public static readonly ModuleIdentifier CMREstablishmentCodes = new ModuleIdentifier(ModuleId.CMREstablishmentCodes, ResString.GetMultilingualString("ModuleId.CMREstablishmentCodes", "Establishment Codes"));
				public static readonly ModuleIdentifier DrawbackEntryLine = new ModuleIdentifier(ModuleId.DrawbackEntryLine, ResString.GetMultilingualString("ModuleId.DrawbackEntryLine", "Drawback Entry Lines"));
			}

			public static class CA
			{
				public static readonly ModuleIdentifier HTSTariffBulkChange = new ModuleIdentifier(ModuleId.CAHTSTariffBulkChange, ResString.GetMultilingualString("ModuleId.CAHTSTariffBulkChange", "HS Tariff Bulk Change"));
				public static readonly ModuleIdentifier ClassTariff = new ModuleIdentifier(ModuleId.CACClass, ResString.GetMultilingualString("ModuleId.CACClass", "Classification Tariff"));
				public static readonly ModuleIdentifier ExportTariff = new ModuleIdentifier(ModuleId.CACExportTariff, ResString.GetMultilingualString("ModuleId.CACExportTariff", "Export Tariff"));
				public static readonly ModuleIdentifier CAQueryMessages = new ModuleIdentifier(ModuleId.CAQueryMessages, ResString.GetMultilingualString("ModuleId.CAQueryMessages", "Query Messages"), ResString.GetMultilingualString("ModuleId.CAQueryMessagesExtended", "Query Messages (CA Customs)"));
				public static readonly ModuleIdentifier K84Reports = new ModuleIdentifier(ModuleId.K84Reports, ResString.GetMultilingualString("ModuleId.K84Reports", "ARL Messages"));
				public static readonly ModuleIdentifier CADailyNoticeReconciliation = new ModuleIdentifier(ModuleId.CADailyNoticeReconciliation, ResString.GetMultilingualString("ModuleId.CADailyNoticeReconciliation", "Daily Notice Reconciliation"));
				public static readonly ModuleIdentifier CAARLStatementOfAccount = new ModuleIdentifier(ModuleId.CAARLStatementOfAccount, ResString.GetMultilingualString("ModuleId.CAARLStatementOfAccount", "ARL Statement of Account"));
				public static readonly ModuleIdentifier CAReleaseNotifications = new ModuleIdentifier(ModuleId.CAReleaseNotifications, ResString.GetMultilingualString("ModuleId.CAReleaseNotifications", "Release Notifications"));
				public static readonly ModuleIdentifier HTSClassification = new ModuleIdentifier(ModuleId.HTSClassification, ResString.GetMultilingualString("ModuleId.HTSClassification", "HTS Classification Lookup"));
				public static readonly ModuleIdentifier CAExportClassification = new ModuleIdentifier(ModuleId.CAExportClassification, ResString.GetMultilingualString("ModuleId.CAExportClassification", "Export Classification Lookup"), ResString.GetMultilingualString("ModuleId.CAExportClassificationExtended", "Export Classification Lookup (Customs)"));
				public static readonly ModuleIdentifier SubLocation = new ModuleIdentifier(ModuleId.CACSubLocation, ResString.GetMultilingualString("ModuleId.CACSubLocation", "Warehouse Sub-Location"));
				public static readonly ModuleIdentifier CFIAEndUseCodes = new ModuleIdentifier(ModuleId.CACFIAEndUseCodes, ResString.GetMultilingualString("ModuleId.CACFIAEndUseCodes", "CFIA End Use Codes"));
				public static readonly ModuleIdentifier CFIAMiscCodes = new ModuleIdentifier(ModuleId.CACFIAMiscCodes, ResString.GetMultilingualString("ModuleId.CACFIAMiscCodes", "CFIA Miscellaneous Codes"));
				public static readonly ModuleIdentifier CATransactionNumberSetting = new ModuleIdentifier(ModuleId.CATransactionNumberSetting, ResString.GetMultilingualString("ModuleId.CATransactionNumberSetting", "Transaction Number"));
				public static readonly ModuleIdentifier B2Adjustments = new ModuleIdentifier(ModuleId.B2Adjustments, ResString.GetMultilingualString("ModuleId.B2Adjustments", "B2/B3X Adjustments"));
				public static readonly ModuleIdentifier CAHouseBilleManifest = new ModuleIdentifier(ModuleId.CAHouseBilleManifest, ResString.GetMultilingualString("ModuleId.CAHouseBilleManifest", "CA eManifest Forwarder"));
				public static readonly ModuleIdentifier CAManifestForward = new ModuleIdentifier(ModuleId.CAManifestForward, ResString.GetMultilingualString("ModuleId.CAManifestForward", "Forwarded Manifests"));
				public static readonly ModuleIdentifier CALVXJobs = new ModuleIdentifier(ModuleId.CALVXJobs, ResString.GetMultilingualString("ModuleId.CALVXJobs", "Courier LVS Declarations"));
				public static readonly ModuleIdentifier CusSCAOceanBill = new ModuleIdentifier(ModuleId.CusSCAOceanBill, ResString.GetMultilingualString("ModuleId.CusSCAOceanBill", "CusSCAOceanBill"));
				public static readonly ModuleIdentifier CAJobDocAddresses = new ModuleIdentifier(ModuleId.CAJobDocAddresses, ResString.GetMultilingualString("ModuleId.CAJobDocAddresses", "Declaration Document Addresses"));
				public static readonly ModuleIdentifier CACusRuling = new ModuleIdentifier(ModuleId.CACusRuling, ResString.GetMultilingualString("ModuleId.CACusRemission", "Remissions"));
				public static readonly ModuleIdentifier CACSARevenueSummaryForm = new ModuleIdentifier(ModuleId.CACSARevenueSummaryForm, ResString.GetMultilingualString("ModuleID.CACSARevenueSummaryForm", "CSA Revenue Summary Form"));
			}

			public static class CH
			{
				public static readonly ModuleIdentifier CustomsSummary = new ModuleIdentifier(ModuleId.CHCustomsSummary, ResString.GetMultilingualString("ModuleId.CHCustomsSummary", "Customs Summary"));
				public static readonly ModuleIdentifier DeclarationActivation = new ModuleIdentifier(ModuleId.CHDeclarationActivation, ResString.GetMultilingualString("ModuleId.CHDeclarationActivation", "Customs Declaration Activation"));
			}

			public static class EU
			{
				public static readonly ModuleIdentifier CusClassificationModule = new ModuleIdentifier(ModuleId.CusClassificationModule, ResString.GetMultilingualString("ModuleId.CusClassificationModule", "Customs Classification Module"));
				public static readonly ModuleIdentifier EMCS = new ModuleIdentifier(ModuleId.EMCS, ResString.GetMultilingualString("ModuleId.EMCS", "Excise Movement Control System (EMCS)"));
				public static readonly ModuleIdentifier IntrastatReports = new ModuleIdentifier(ModuleId.IntrastatReports, ResString.GetMultilingualString("ModuleId.IntrastatReports", "Intrastat - Reports"));
				public static readonly ModuleIdentifier NctsMovementModule = new ModuleIdentifier(ModuleId.NctsMovementModule, ResString.GetMultilingualString("ModuleId.NctsMovementModule", "NCTS Transit Movements"));
				public static readonly ModuleIdentifier NctsReportsModule = new ModuleIdentifier(ModuleId.NctsReportsModule, ResString.GetMultilingualString("ModuleId.NctsReportsModule", "NCTS Reports"));
				public static readonly ModuleIdentifier ExitControl = new ModuleIdentifier(ModuleId.ExitControl, ResString.GetMultilingualString("ModuleId.ExitControl", "Exit Control"));
				public static readonly ModuleIdentifier ExitControlReport = new ModuleIdentifier(ModuleId.ExitControlReport, ResString.GetMultilingualString("ModuleId.ExitControlReport", "Exit Reports"));
				public static readonly ModuleIdentifier EUH7 = new ModuleIdentifier(ModuleId.EUH7, ResString.GetMultilingualString("ModuleId.EUH7", "Low Value (H7)"));
				public static readonly ModuleIdentifier EUH7Bill = new ModuleIdentifier(ModuleId.EUH7Bill, ResString.GetMultilingualString("ModuleId.EUH7Bill", "Low Value (H7) by Bill"));
				public static readonly ModuleIdentifier UCC6TemporaryStorage = new ModuleIdentifier(ModuleId.UCC6TemporaryStorage, ResString.GetMultilingualString("ModuleId.UCCTemporaryStorage", "Temporary Storage - UCC"));
				public static readonly ModuleIdentifier IntrastatTransactions = new ModuleIdentifier(ModuleId.IntrastatTransactions, ResString.GetMultilingualString("ModuleId.IntrastatTransactions", "Intrastat - Transactions"));
				public static readonly ModuleIdentifier TempStorageRegister = new ModuleIdentifier(ModuleId.TempStorageRegister, ResString.GetMultilingualString("ModuleId.TempStorageRegister", "Temp. Storage Register"));
				public static readonly ModuleIdentifier TempStoragePremises = new ModuleIdentifier(ModuleId.TempStoragePremises, ResString.GetMultilingualString("ModuleId.TempStoragePremises", "Temporary Storage Premises"));
				public static readonly ModuleIdentifier TempStorageRegisterLines = new ModuleIdentifier(ModuleId.TempStorageRegisterLines, ResString.GetMultilingualString("ModuleId.TempStorageRegisterLines", "Temporary Storage Register Lines"));

				public static class GB
				{
					public static readonly ModuleIdentifier CcsukGenralMessage = new ModuleIdentifier(ModuleId.CcsukGenralMessage, ResString.GetMultilingualString("ModuleId.CcsukGenralMessage", "GENRAL Messages"));
					public static readonly ModuleIdentifier CcsukAirInventory = new ModuleIdentifier(ModuleId.GbCcsukAirInventory, ResString.GetMultilingualString("ModuleId.GbCcsukAirInventory", "Master Bills"));
					public static readonly ModuleIdentifier DLUMessage = new ModuleIdentifier(ModuleId.GbDLUMessage, ResString.GetMultilingualString("ModuleId.GbDLUMessage", "License Usage Enquiries"));
					public static readonly ModuleIdentifier CcsukStandAloneFsrEnquiry = new ModuleIdentifier(ModuleId.GbCcsukStandAloneFsrEnquiry, ResString.GetMultilingualString("ModuleId.GbCcsukStandAloneFsrEnquiry", "Community DB Enquiries"));
					public static readonly ModuleIdentifier CcsukAirInventoryHouse = new ModuleIdentifier(ModuleId.GbCcsukAirInventoryHouse, ResString.GetMultilingualString("ModuleId.GbCcsukAirInventoryHouse", "House Bills"));
					public static readonly ModuleIdentifier CcsukMasterAndHouseCombined = new ModuleIdentifier(ModuleId.CcsukMasterAndHouseCombined, ResString.GetMultilingualString("ModuleId.CcsukMasterAndHouseCombined", "All AWBs"));
					public static readonly ModuleIdentifier CcsukSplitHouse = new ModuleIdentifier(ModuleId.GbCcsukSplitHouse, ResString.GetMultilingualString("ModuleId.GbCcsukSplitHouse", "Split Houses"));
					public static readonly ModuleIdentifier CcsukSplitBasic = new ModuleIdentifier(ModuleId.GbCcsukSplitBasic, ResString.GetMultilingualString("ModuleId.GbCcsukSplitBasic", "Split Basics"));
					public static readonly ModuleIdentifier GbCcsukReports = new ModuleIdentifier(ModuleId.GbCcsukReports, ResString.GetMultilingualString("ModuleId.GbCcsukReports", "Reports"));
					public static readonly ModuleIdentifier CDSDISQuery = new ModuleIdentifier(ModuleId.GBCDSDISQuery, ResString.GetMultilingualString("ModuleId.GBCDSDISQuery", "CDS DIS Query"));
					public static readonly ModuleIdentifier CDSCashPayments = new ModuleIdentifier(ModuleId.GBCDSCashPayments, ResString.GetMultilingualString("ModuleId.GBCDSCashPayments", "CDS Cash & PVA Payments"));
					public static readonly ModuleIdentifier EUH7 = new ModuleIdentifier(ModuleId.EUH7, ResString.GetMultilingualString("ModuleId.GB.EUH7", "Low Value (H7/BIRDS)"));
					public static readonly ModuleIdentifier EUH7Bill = new ModuleIdentifier(ModuleId.EUH7Bill, ResString.GetMultilingualString("ModuleId.GB.EUH7Bill", "Low Value (H7/BIRDS) by Bill"));
					public static readonly ModuleIdentifier PreBoardingNotification = new ModuleIdentifier(ModuleId.PreBoardingNotification, ResString.GetMultilingualString("Module.GBPreBoardingNotification", "Goods vehicle movement system (GVMS)"));
				}

				public static class DE
				{
					public static readonly ModuleIdentifier SumARegister = new ModuleIdentifier(ModuleId.SumARegister, ResString.GetMultilingualString("ModuleId.SumARegister", "SumA Register"));
					public static readonly ModuleIdentifier SumARegisterReadOnly = new ModuleIdentifier(ModuleId.SumARegisterReadOnly, ResString.GetMultilingualString("ModuleId.SumARegisterReadOnly", "Temporary Storage - Register"));
					public static readonly ModuleIdentifier ImportFromSumARegister = new ModuleIdentifier(ModuleId.ImportFromSumARegister, ResString.GetMultilingualString("ModuleId.ImportFromSumARegister", "Import from SumA Register"));
					public static readonly ModuleIdentifier ExportStatusRequest = new ModuleIdentifier(ModuleId.ExportStatusRequest, ResString.GetMultilingualString("ModuleId.ExportStatusRequest", "AES/NCTS Status Request"));
					public static readonly ModuleIdentifier MonthlyClosing = new ModuleIdentifier(ModuleId.MonthlyClosing, ResString.GetMultilingualString("ModuleId.MonthlyClosing", "Monthly Closing"));
					public static readonly ModuleIdentifier SimplifiedDeclaration = new ModuleIdentifier(ModuleId.SimplifiedDeclaration, ResString.GetMultilingualString("ModuleId.SimplifiedDeclaration", "Simplified Declaration"));
					public static readonly ModuleIdentifier TaxChangeAssessment = new ModuleIdentifier(ModuleId.TaxChangeAssessment, ResString.GetMultilingualString("ModuleId.TaxChangeAssessment", "TAX Change Assessment"));
				}

				public static class ES
				{
					public static readonly ModuleIdentifier TemporaryStorageRegister = new ModuleIdentifier(ModuleId.ESTemporaryStorageRegister, ResString.GetMultilingualString("ModuleId.ESTemporaryStorageRegister", "Temporary Storage Register"));
					public static readonly ModuleIdentifier G3Declaration = new ModuleIdentifier(ModuleId.G3Declaration, ResString.GetMultilingualString("ModuleId.Customs.G3Declaration", "G3 (ES H7) Declarations"));
					public static readonly ModuleIdentifier EUH7 = new ModuleIdentifier(ModuleId.EUH7, ResString.GetMultilingualString("ModuleId.ES.EUH7", "Low Value (H7)"));
					public static readonly ModuleIdentifier EUH7Bill = new ModuleIdentifier(ModuleId.EUH7Bill, ResString.GetMultilingualString("ModuleId.ES.EUH7Bill", "Low Value (H7) by bill"));
				}

				public static class FR
				{
					public static readonly ModuleIdentifier CustomsStatement = new ModuleIdentifier(ModuleId.FRCustomsStatement, ResString.GetMultilingualString("ModuleId.FRCustomsStatement", "Liquidation Statements"));
				}

				public static class PL
				{
					public static readonly ModuleIdentifier AuthorisationRule = new ModuleIdentifier(ModuleId.AuthorisationRule, ResString.GetMultilingualString("ModuleId.AuthorisationRule", "Authorization Rule"));
				}

				public static class IE
				{
					public static readonly ModuleIdentifier CustomsAndExciseReports = new ModuleIdentifier(ModuleId.IECustomsAndExciseReports, ResString.GetMultilingualString("ModuleId.IECustomsAndExciseReports", "Customs and Excise Reports"));
					public static readonly ModuleIdentifier PreBoardingNotification = new ModuleIdentifier(ModuleId.PreBoardingNotification, ResString.GetMultilingualString("Module.IEPreBoardingNotification", "PBN-Pre Boarding Notification"));
				}
			}

			public static class JP
			{
				public static readonly ModuleIdentifier AFR = new ModuleIdentifier(ModuleId.AFR, ResString.GetMultilingualString("Module.AFR", "JP AFR"));
				public static readonly ModuleIdentifier AFRBill = new ModuleIdentifier(ModuleId.AFRBill, ResString.GetMultilingualString("Module.AFRBill", "JP AFR Bill"));
			}

			public static class NO
			{
				public static readonly ModuleIdentifier TemporaryStorageRegister = new(ModuleId.TemporaryStorageRegister, ResString.GetMultilingualString("ModuleId.NO.SumARegister", "Temporary Storage - Register"));
				public static readonly ModuleIdentifier TemporaryStorageRegisterReadOnly = new(ModuleId.TemporaryStorageRegisterReadOnly, ResString.GetMultilingualString("ModuleId.NO.SumARegisterReadOnly", "Temporary Storage - Register"));
			}

			public static class NZ
			{
				public static readonly ModuleIdentifier CUSCAR = new ModuleIdentifier(ModuleId.NZCUSCAR, ResString.GetMultilingualString("ModuleId.NZCUSCAR", "ECI Write-Off"));
				public static readonly ModuleIdentifier ECIWriteOffManifesting = new ModuleIdentifier(ModuleId.ECIWriteOffManifesting, ResString.GetMultilingualString("ModuleId.ECIWriteOffManifesting", "ECI Manifesting"));
				public static readonly ModuleIdentifier Concession = new ModuleIdentifier(ModuleId.Concession, ResString.GetMultilingualString("ModuleId.Concession", "Concession"));
				public static readonly ModuleIdentifier OutwardReport = new ModuleIdentifier(ModuleId.OutwardReport, ResString.GetMultilingualString("ModuleId.OutwardReport", "ORN Status"));
				public static readonly ModuleIdentifier ExpressECI = new ModuleIdentifier(ModuleId.ExpressECI, ResString.GetMultilingualString("ModuleId.ExpressECI", Constants.NZCustoms.ExpressECIName));
				public static readonly ModuleIdentifier InwardCargoReport = new ModuleIdentifier(ModuleId.InwardCargoReport, ResString.GetMultilingualString("ModuleId.InwardCargoReport", "ICR Status"));
				public static readonly ModuleIdentifier SeaCargoICR = new ModuleIdentifier(ModuleId.SeaCargoICR, ResString.GetMultilingualString("ModuleId.SeaCargoICR", Constants.NZCustoms.ExpressSeaCargoName));
			}

			public static class SG
			{
				public static readonly ModuleIdentifier JobDeclaration = new ModuleIdentifier(ModuleId.CusDec, ResString.GetMultilingualString("ModuleId.CusDec|Tradenet", "TradeNet"));
				public static readonly ModuleIdentifier SG4Classification = new ModuleIdentifier(ModuleId.SG4Classification, ResString.GetMultilingualString("ModuleId.SG4Classification", "Classification"));
			}

			public static class US
			{
				public static readonly ModuleIdentifier AffirmationOfCompliance = new ModuleIdentifier(ModuleId.USCAffirmationOCompliance, ResString.GetMultilingualString("ModuleId.USCAffirmationOCompliance", "Aff. of Compliance"));
				public static readonly ModuleIdentifier Carrier = new ModuleIdentifier(ModuleId.USCarrierCombined, ResString.GetMultilingualString("ModuleId.USCarrierCombined", "Carriers"));
				public static readonly ModuleIdentifier Country = new ModuleIdentifier(ModuleId.USCCountry, ResString.GetMultilingualString("ModuleId.USCCountry", "Countries/Regions"), ResString.GetMultilingualString("ModuleId.USCCountryExtended", "Countries/Regions (Customs)"));
				public static readonly ModuleIdentifier Drawback = new ModuleIdentifier(ModuleId.USDrawback, ResString.GetMultilingualString("ModuleId.USDrawback", "Drawback"));
				public static readonly ModuleIdentifier ForeignPort = new ModuleIdentifier(ModuleId.USCForeignPort, ResString.GetMultilingualString("ModuleId.USCForeignPort", "Schedule K Port Codes"));
				public static readonly ModuleIdentifier RegionDistrictPort = new ModuleIdentifier(ModuleId.USCRegionDistrictPort, ResString.GetMultilingualString("ModuleId.USCRegionDistrictPort", "Schedule D Port Codes (Import)"));
				public static readonly ModuleIdentifier Tariff = new ModuleIdentifier(ModuleId.USCTariff, ResString.GetMultilingualString("ModuleId.USCTariff", "US Import Tariffs"));
				public static readonly ModuleIdentifier USTariffBulkChange = new ModuleIdentifier(ModuleId.USTariffBulkChange, ResString.GetMultilingualString("ModuleId.USTariffBulkChange", "Tariff Bulk Change"), ResString.GetMultilingualString("ModuleId.USTariffBulkChangeExtended", "Tariff Bulk Change (US Customs)"));
				public static readonly ModuleIdentifier TeamSpecialist = new ModuleIdentifier(ModuleId.USCTeamSpecialist, ResString.GetMultilingualString("ModuleId.USCTeamSpecialist", "Team Specialist File"));
				public static readonly ModuleIdentifier FIRMS = new ModuleIdentifier(ModuleId.USCFIRMS, ResString.GetMultilingualString("ModuleId.USCFIRMS", "FIRMS Codes"));
				public static readonly ModuleIdentifier USCustomsStatement = new ModuleIdentifier(ModuleId.USCustomsStatement, ResString.GetMultilingualString("ModuleId.USCustomsStatement", "Statements / ACH Payments"));
				public static readonly ModuleIdentifier Visa = new ModuleIdentifier(ModuleId.USCVisa, ResString.GetMultilingualString("ModuleId.USCVisa", "Visas"));
				public static readonly ModuleIdentifier TariffRequiringVisa = new ModuleIdentifier(ModuleId.USCVisaTariff, ResString.GetMultilingualString("ModuleId.USCVisaTariff", "Visa Tariffs"));
				public static readonly ModuleIdentifier Quota = new ModuleIdentifier(ModuleId.USCQuota, ResString.GetMultilingualString("ModuleId.USCQuota", "Quotas"));
				public static readonly ModuleIdentifier AMSBrokerDownloadMessage = new ModuleIdentifier(ModuleId.AMSBrokerDownloadMessage, ResString.GetMultilingualString("ModuleId.AMSBrokerDownloadMessage", "AMS Broker Download"));
				public static readonly ModuleIdentifier CourtesyNoticesOfLiquidation = new ModuleIdentifier(ModuleId.CourtesyNoticesOfLiquidation, ResString.GetMultilingualString("ModuleId.CourtesyNoticesOfLiquidation", "Liquidation Notices"));
				public static readonly ModuleIdentifier BorderLineReleaseMessage = new ModuleIdentifier(ModuleId.BorderLineReleaseMessage, ResString.GetMultilingualString("ModuleId.BorderLineReleaseMessage", "Border Line Release"));
				public static readonly ModuleIdentifier QueryMessage = new ModuleIdentifier(ModuleId.QueryMessage, ResString.GetMultilingualString("ModuleId.QueryMessage", "Query Messages"), ResString.GetMultilingualString("ModuleId.QueryMessageExtended", "Query Messages (US Customs)"));
				public static readonly ModuleIdentifier InBondNumber = new ModuleIdentifier(ModuleId.InBondNumber, ResString.GetMultilingualString("ModuleId.InBondNumber", "In-Bond Number"), ResString.GetMultilingualString("ModuleId.InBondNumberExtended", "In-Bond Number (US Customs)"));
				public static readonly ModuleIdentifier USCRule = new ModuleIdentifier(ModuleId.USCRule, ResString.GetMultilingualString("ModuleId.USCRule", "Rules"));
				public static readonly ModuleIdentifier USCTariffRule = new ModuleIdentifier(ModuleId.USCTariffRule, ResString.GetMultilingualString("ModuleId.USCTariffRule", "Tariff Rules"));
				public static readonly ModuleIdentifier USCACCase = new ModuleIdentifier(ModuleId.USCACCase, ResString.GetMultilingualString("ModuleId.USCACCase", "ACE AD/CVD Case"));
				public static readonly ModuleIdentifier EntryLine = new ModuleIdentifier(ModuleId.USEntryLine, ResString.GetMultilingualString("ModuleId.USEntryLine", "Customs Entry Lines"));
				public static readonly ModuleIdentifier Protest = new ModuleIdentifier(ModuleId.Protest, ResString.GetMultilingualString("ModuleId.Protest", "Protest"));
				public static readonly ModuleIdentifier Reconciliation = new ModuleIdentifier(ModuleId.Reconciliation, ResString.GetMultilingualString("ModuleId.Reconciliation", "Reconciliation"));
				public static readonly ModuleIdentifier USCDataVersion = new ModuleIdentifier(ModuleId.USCDataVersion, ResString.GetMultilingualString("ModuleId.USCDataVersion", "Data Version"));
				public static readonly ModuleIdentifier InBond = new ModuleIdentifier(ModuleId.USInBond, ResString.GetMultilingualString("ModuleId.USInBond", "In-Bond"));
				public static readonly ModuleIdentifier InBondMoveHeader = new ModuleIdentifier(ModuleId.USInBondMoveHeader, ResString.GetMultilingualString("ModuleId.USInBondMoveHeader", "In-Bond Movements"));
				public static readonly ModuleIdentifier USCForeignAndRegionPort = new ModuleIdentifier(ModuleId.USCForeignAndRegionPort, ResString.GetMultilingualString("ModuleId.USCForeignAndRegionPort", "Combined Port Codes"));
				public static readonly ModuleIdentifier USCCarrierAndFIRMS = new ModuleIdentifier(ModuleId.USCCarrierAndFIRMS, ResString.GetMultilingualString("ModuleId.USCCarrierAndFIRMS", "Combined Carrier And FIRMS Codes"));
				public static readonly ModuleIdentifier eManifest = new ModuleIdentifier(ModuleId.USeManifest, ResString.GetMultilingualString("ModuleId.USeManifest", "e-Manifest"));
				public static readonly ModuleIdentifier eManifestShipment = new ModuleIdentifier(ModuleId.USeManifestShipment, ResString.GetMultilingualString("ModuleId.USeManifestShipment", "e-Manifest Shipments"));
				public static readonly ModuleIdentifier eManifestIntl = new ModuleIdentifier(ModuleId.eManifestIntl, ResString.GetMultilingualString("Module.eManifestIntl", "US e-Manifest"));
				public static readonly ModuleIdentifier AMS = new ModuleIdentifier(ModuleId.AMS, ResString.GetMultilingualString("Module.AMS", "US AMS"));
				public static readonly ModuleIdentifier AMSBill = new ModuleIdentifier(ModuleId.AMSBill, ResString.GetMultilingualString("Module.AMSBill", "US AMS Bill"));
				public static readonly ModuleIdentifier ThreeLetterRefAirline = new ModuleIdentifier(ModuleId.ThreeLetterRefAirline, ResString.GetMultilingualString("Module.ThreeLetterRefAirline", "Airlines"));
				public static readonly ModuleIdentifier USLowValueEntries = new ModuleIdentifier(ModuleId.USLowValueEntries, ResString.GetMultilingualString("Module.USLowValueEntries", "Low Value Entries"));
				public static readonly ModuleIdentifier USLowValueEntriesBill = new ModuleIdentifier(ModuleId.USLowValueEntriesBill, ResString.GetMultilingualString("Module.USLowValueEntriesBill", "Low Value Entries by Bill"));
				public static readonly ModuleIdentifier USLowValueEntriesDeclaration = new ModuleIdentifier(ModuleId.USLowValueEntriesDeclaration, ResString.GetMultilingualString("Module.USLowValueEntriesDeclaration", "Low Value Entries by Declaration"));
				public static readonly ModuleIdentifier USImportClassification = new ModuleIdentifier(ModuleId.ImportClassification, ResString.GetMultilingualString("Module.USImportClassification", "HTS Lookup Codes"));
				public static readonly ModuleIdentifier USExportClassification = new ModuleIdentifier(ModuleId.ExportClassification, ResString.GetMultilingualString("Module.USExportClassification", "Schedule B Lookup Codes"));
			}

			public static class TW
			{
				public static readonly ModuleIdentifier Transhipment = new ModuleIdentifier(ModuleId.TWTranshipment, ResString.GetMultilingualString("ModuleId.TWTranshipment", "Transhipment"));
				public static readonly ModuleIdentifier SpecialCode = new ModuleIdentifier(ModuleId.TWSpecialCode, ResString.GetMultilingualString("ModuleId.TWSpecialCode", "Special Code"));
				public static readonly ModuleIdentifier BriefCustomsDeclarations = new ModuleIdentifier(ModuleId.BriefCustomsDeclarations, ResString.GetMultilingualString("ModuleId.BriefCustomsDeclarations", "Brief Customs Declarations"));
			}

			public static class TR
			{
				public static readonly ModuleIdentifier ETrade = new ModuleIdentifier(ModuleId.ETrade, ResString.GetMultilingualString("ModuleId.ETrade", "E-Trade"));
				public static readonly ModuleIdentifier SimplifiedProcedureTransitSystem = new ModuleIdentifier(ModuleId.SimplifiedProcedureTransitSystem, ResString.GetMultilingualString("ModuleId.SimplifiedProcedureTransitSystem", "SPTS Simplified Procedure Transit System"));
				public static readonly ModuleIdentifier StatementsStampDuty = new ModuleIdentifier(ModuleId.StatementsStampDuty, ResString.GetMultilingualString("ModuleId.StatementsStampDutyModule", "Statements / Stamp Duty"));
			}

			public static class KR
			{
				public static readonly ModuleIdentifier CustomsStatement = new ModuleIdentifier(ModuleId.KRCustomsStatement, ResString.GetMultilingualString("ModuleId.KRCustomsStatement", "Customs Statements"));
				public static readonly ModuleIdentifier MiscRequestMessages = new ModuleIdentifier(ModuleId.MiscRequestMessages, ResString.GetMultilingualString("ModuleId.MiscRequestMessages", "Misc. Request Messages"));
				public static readonly ModuleIdentifier ExportEntryDetails = new ModuleIdentifier(ModuleId.ExportEntryDetails, ResString.GetMultilingualString("ModuleId.ExportEntryDetails", "Export Declaration Search"));
				public static readonly ModuleIdentifier ImportEntryDetails = new ModuleIdentifier(ModuleId.ImportEntryDetails, ResString.GetMultilingualString("ModuleId.ImportEntryDetails", "Import Declaration Search"));
				public static readonly ModuleIdentifier EntryDetailsFor5SG = new ModuleIdentifier(ModuleId.EntryDetailsFor5SG, ResString.GetMultilingualString("ModuleId.EntryDetailsFor5SG", "Import Declaration Search"));
				public static readonly ModuleIdentifier EntryCustomsBillsFor5UL = new ModuleIdentifier(ModuleId.EntryCustomsBillsFor5UL, ResString.GetMultilingualString("ModuleId.EntryCustomsBillsFor5UL", "Customs Disbursement Bills Search"));
				public static readonly ModuleIdentifier EntryLineDetailsFor5UL = new ModuleIdentifier(ModuleId.EntryLineDetailsFor5UL, ResString.GetMultilingualString("ModuleId.EntryLineDetailsFor5UL", "Entry Line Details"));
				public static readonly ModuleIdentifier DocumentListMessages = new ModuleIdentifier(ModuleId.DocumentListMessages, ResString.GetMultilingualString("ModuleId.DocumentListMessages", "Document List Messages"));
				public static readonly ModuleIdentifier CusReconDeclaration = new ModuleIdentifier(ModuleId.CusReconDeclaration, ResString.GetMultilingualString("ModuleId.CusReconDeclaration", "Refund Declarations"));
			}

			public static class BR
			{
				public static readonly ModuleIdentifier LPCO = new ModuleIdentifier(ModuleId.BRLPCO, ResString.GetMultilingualString("ModuleId.BRLPCO", "LPCO"));
				public static readonly ModuleIdentifier LPCODeclaration = new ModuleIdentifier(ModuleId.BRLPCODeclaration, ResString.GetMultilingualString("ModuleId.BRLPCODeclaration", "LPCO Declarations"));
				public static readonly ModuleIdentifier LPCOEntryHeader = new ModuleIdentifier(ModuleId.BRLPCOEntryHeader, ResString.GetMultilingualString("ModuleId.BRLPCOEntryHeader", "LPCO Declarations - Detailing"));
				public static readonly ModuleIdentifier License = new ModuleIdentifier(ModuleId.BRLicense, ResString.GetMultilingualString("ModuleId.BRLicense", "Licenses"));
				public static readonly ModuleIdentifier LicenseEntryHeader = new ModuleIdentifier(ModuleId.BRLicenseEntryHeader, ResString.GetMultilingualString("ModuleId.BRLicenseEntryHeader", "Licenses - Detailing"));
				public static readonly ModuleIdentifier ForeignOperator = new ModuleIdentifier(ModuleId.BRForeignOperator, ResString.GetMultilingualString("ModuleId.BRForeignOperator", "Foreign Operators"));
			}

			public static class CO
			{
				public static readonly ModuleIdentifier DocumentIDs = new ModuleIdentifier(ModuleId.DocumentIDs, ResString.GetMultilingualString("ModuleId.DocumentIDs", "Document IDs"));
			}
		}

		public static class Messaging
		{
			public static readonly ModuleIdentifier EDIMessage = new ModuleIdentifier(ModuleId.EDIMessage, ResString.GetMultilingualString("Module.EDIMessage", "EDI Message"));
			public static readonly ModuleIdentifier EDIMessagePurpose = new ModuleIdentifier(ModuleId.EDIMessagePurpose, ResString.GetMultilingualString("Module.EDIMessagePurpose", "Purpose Code"));
			public static readonly ModuleIdentifier EDICommunicationsMode = new ModuleIdentifier(ModuleId.EDICommunicationsMode, ResString.GetMultilingualString("Module.EDICommunicationsMode", "EDI Communication Modes"));
			public static readonly ModuleIdentifier EDIInterchange = new ModuleIdentifier(ModuleId.EDIInterchange, ResString.GetMultilingualString("Module.EDIInterchange", "EDI Interchange"));
			public static readonly ModuleIdentifier EDICommunicationParty = new ModuleIdentifier(ModuleId.EDICommunicationParty, ResString.GetMultilingualString("Module.EDICommunicationParty", "EDI Client Details"));
			public static readonly ModuleIdentifier EDIMessageContentFilter = new ModuleIdentifier(ModuleId.EDIMessageContentFilter, ResString.GetMultilingualString("Module.EDIMessageContentFilter", "EDI Message Profile"));
			public static readonly ModuleIdentifier EDIMessageDeliveryContext = new ModuleIdentifier(ModuleId.EDIMessageDeliveryContext, ResString.GetMultilingualString("Module.EDIMessageDeliveryContext", "Additional Event Context"));
			public static readonly ModuleIdentifier EDICodeMapping = new ModuleIdentifier(ModuleId.EDICodeMapping, ResString.GetMultilingualString("Module.EDICodeMapping", "EDI Code Mapping"));
			public static readonly ModuleIdentifier UniversalValidationRule = new ModuleIdentifier(ModuleId.UniversalValidationRule, ResString.GetMultilingualString("Module.UniversalValidationRule", "Business Rule Engine"));
		}

		#endregion

		public static readonly ModuleIdentifier UniversalCopySchedule = new ModuleIdentifier(ModuleId.UniversalCopySchedule, ResString.GetMultilingualString("Module.UniversalCopySchedule", "Universal Copy Schedules"));

		#region Business Intelligence and Analytics

		public static readonly ModuleIdentifier AnalyticsReports = new ModuleIdentifier(ModuleId.AnalyticsReports, ResString.GetMultilingualString("Module.AnalyticsReports", "Analytics Reports"));
		public static readonly ModuleIdentifier BiManager = new ModuleIdentifier(ModuleId.BiManager, ResString.GetMultilingualString("Manager.BiManager", "BI Manager"));
		public static readonly ModuleIdentifier Audit = new ModuleIdentifier(ModuleId.Audit, ResString.GetMultilingualString("Module.Audit", "Audit Data"));

		#endregion

		// Value Analysis
		public static readonly ModuleIdentifier ValueAnalysisForwardingOrg = new ModuleIdentifier(ModuleId.ValueAnalysisForwardingOrg, ResString.GetMultilingualString("Module.ValueAnalysisForwardingOrganisation", "Organization Forwarding Value Analysis"));
		public static readonly ModuleIdentifier ValueAnalysisCustomsBrokerageOrg = new ModuleIdentifier(ModuleId.ValueAnalysisCustomsBrokerageOrg, ResString.GetMultilingualString("Module.ValueAnalysisCustomsBrokerageOrganisation", "Organization Customs Brokerage Value Analysis"));
		public static readonly ModuleIdentifier ValueAnalysisTransportOrg = new ModuleIdentifier(ModuleId.ValueAnalysisTransportOrg, ResString.GetMultilingualString("Module.ValueAnalysisTransportOrganisation", "Organization Transport Value Analysis"));
		public static readonly ModuleIdentifier ValueAnalysisWarehouseOrg = new ModuleIdentifier(ModuleId.ValueAnalysisWarehouseOrg, ResString.GetMultilingualString("Module.ValueAnalysisWarehouseOrganisation", "Organization Warehouse Value Analysis"));
		public static readonly ModuleIdentifier ValueAnalysisLinerAgencyOrg = new ModuleIdentifier(ModuleId.ValueAnalysisLinerAgencyOrg, ResString.GetMultilingualString("Module.ValueAnalysisLinerAgencyOrganisation", "Organization Liner & Agency Value Analysis"));

		public static readonly ModuleIdentifier ValueAnalysisForwardingOpp = new ModuleIdentifier(ModuleId.ValueAnalysisForwardingOpp, ResString.GetMultilingualString("Module.ValueAnalysisForwardingOpportunity", "Opportunity Forwarding Value Analysis"));
		public static readonly ModuleIdentifier ValueAnalysisCustomsBrokerageOpp = new ModuleIdentifier(ModuleId.ValueAnalysisCustomsBrokerageOpp, ResString.GetMultilingualString("Module.ValueAnalysisCustomsBrokerageOpportunity", "Opportunity Customs Brokerage Value Analysis"));
		public static readonly ModuleIdentifier ValueAnalysisTransportOpp = new ModuleIdentifier(ModuleId.ValueAnalysisTransportOpp, ResString.GetMultilingualString("Module.ValueAnalysisTransportOpportunity", "Opportunity Transport Value Analysis"));
		public static readonly ModuleIdentifier ValueAnalysisWarehouseOpp = new ModuleIdentifier(ModuleId.ValueAnalysisWarehouseOpp, ResString.GetMultilingualString("Module.ValueAnalysisWarehouseOpportunity", "Opportunity Warehouse Value Analysis"));
		public static readonly ModuleIdentifier ValueAnalysisLinerAgencyOpp = new ModuleIdentifier(ModuleId.ValueAnalysisLinerAgencyOpp, ResString.GetMultilingualString("Module.ValueAnalysisLinerAgencyOpportunity", "Opportunity Liner & Agency Value Analysis"));

		public static readonly ModuleIdentifier GateBooking = new ModuleIdentifier(ModuleId.GateBooking, ResString.GetMultilingualString("Module.GateBooking", "Gate Booking"));
		public static readonly ModuleIdentifier GateControl = new ModuleIdentifier(ModuleId.GateControl, ResString.GetMultilingualString("Module.GateControl", "Gate Control"));

		public static readonly ModuleIdentifier RefAirlineCommodityCode = new ModuleIdentifier(ModuleId.RefAirlineCommodityCode, ResString.GetMultilingualString("Module.RefAirlineCommodityCode", "Airline Commodity Codes"));

		public static readonly ModuleIdentifier CarrierContractAndAllocations = new ModuleIdentifier(ModuleId.CarrierContractAndAllocations, ResString.GetMultilingualString("Module.CarrierContract", "Carrier Contract & Allocations"));
		public static readonly ModuleIdentifier ClientContractAndAllocations = new ModuleIdentifier(ModuleId.ClientContractAndAllocations, ResString.GetMultilingualString("Module.ClientContract", "Client Contract & Allocations"));

		// Container Yard

		public static readonly ModuleIdentifier CYDAdHocServiceOrder = new ModuleIdentifier(ModuleId.CYDAdHocServiceOrder, ResString.GetMultilingualString("Module.CYDAdHocServiceOrder", "Yard Ad Hoc Service Order"));
		public static readonly ModuleIdentifier CYDDeliveryHeader = new ModuleIdentifier(ModuleId.CYDDeliveryHeader, ResString.GetMultilingualString("Module.CYDDeliveryHeader", "Bulk Runs In"));
		public static readonly ModuleIdentifier CYDPickupHeader = new ModuleIdentifier(ModuleId.CYDPickupHeader, ResString.GetMultilingualString("Module.CYDPickupHeader", "Bulk Runs Out"));
		public static readonly ModuleIdentifier CYDReceiveAdvice = new ModuleIdentifier(ModuleId.CYDReceiveAdvice, ResString.GetMultilingualString("Module.CYDReceiveAdvice", "Pre-Arrival Instructions"));
		public static readonly ModuleIdentifier CYDReleaseAdvice = new ModuleIdentifier(ModuleId.CYDReleaseAdvice, ResString.GetMultilingualString("Module.CYDReleaseAdvice", "Release Orders"));
		public static readonly ModuleIdentifier ContainerYardPortal = new ModuleIdentifier(ModuleId.ContainerYardPortal, ResString.GetMultilingualString("Module.ContainerYardPortal", "Yard Management Portal"));
		public static readonly ModuleIdentifier CYDTransportationUnit = new ModuleIdentifier(ModuleId.CYDTransportationUnit, ResString.GetMultilingualString("Module.CYDTransportationUnit", "Transportation Unit"));
		public static readonly ModuleIdentifier CYDYardUnitState = new ModuleIdentifier(ModuleId.CYDYardUnitState, ResString.GetMultilingualString("Module.CYDYardUnitState", "Yard Units"));
		public static readonly ModuleIdentifier CYDYardReport = new ModuleIdentifier(ModuleId.CYDYardReport, ResString.GetMultilingualString("Module.CYDYardReport", "Reports"), ResString.GetMultilingualString("Module.CYDYardReportExtended", "Reports (Container Yard)"));
		public static readonly ModuleIdentifier MNRWorkOrder = new ModuleIdentifier(ModuleId.MNRWorkOrder, ResString.GetMultilingualString("Module.MNRWorkOrder", "Maintenance & Repair"));
		public static readonly ModuleIdentifier MNRSurvey = new ModuleIdentifier(ModuleId.MNRSurvey, ResString.GetMultilingualString("Module.MNRSurvey", "Survey"));
		public static readonly ModuleIdentifier CYDPeriodicInvoicing = new ModuleIdentifier(ModuleId.CYDPeriodicInvoicing, ResString.GetMultilingualString("Module.CYDPeriodicInvoicing", "Periodic Billing"));

		// Gate Management
		public static readonly ModuleIdentifier GteBooking = new ModuleIdentifier(ModuleId.GteBooking, ResString.GetMultilingualString("Module.GteBooking", "Gate Bookings"));
		public static readonly ModuleIdentifier GteGateMovementBooking = new ModuleIdentifier(ModuleId.GteGateMovementBooking, ResString.GetMultilingualString("Module.GteGateMovementBooking", "Gate Movement Bookings"));
		public static readonly ModuleIdentifier GteGateMovement = new ModuleIdentifier(ModuleId.GteGateMovement, ResString.GetMultilingualString("Module.GteGateMovement", "Gate Movements"));
		public static readonly ModuleIdentifier GteVehicleMovement = new ModuleIdentifier(ModuleId.GteVehicleMovement, ResString.GetMultilingualString("Module.GteVehicleMovement", "Vehicle Movements"));
		public static readonly ModuleIdentifier GateManagementPortal = new ModuleIdentifier(ModuleId.GateManagementPortal, ResString.GetMultilingualString("Module.GateManagementPortal", "Gate Management Portal"));

		//Container Load List
		public static readonly ModuleIdentifier ContainerLoadList = new ModuleIdentifier(ModuleId.ContainerLoadList, ResString.GetMultilingualString("Module.ContainerLoadList", "Container Load List"));

		//Container Load Plan
		public static readonly ModuleIdentifier ContainerLoadPlan = new ModuleIdentifier(ModuleId.ContainerLoadPlan, ResString.GetMultilingualString("Module.ContainerLoadPlan", "Container Load Plan"));

		public static readonly ModuleIdentifier OrdersWebPortal = new ModuleIdentifier(ModuleId.OrdersWebPortal, ResString.GetMultilingualString("Module.OrdersWebPortal", "Advanced Order Manager"));
		public static readonly ModuleIdentifier OrderLinesWebPortal = new ModuleIdentifier(ModuleId.OrderLinesWebPortal, ResString.GetMultilingualString("Module.OrderLinesWebPortal", "Advanced Order Lines"));
		public static readonly ModuleIdentifier ControlTower = new ModuleIdentifier(ModuleId.ControlTower, ResString.GetMultilingualString("Module.ControlTower", "Control Tower"));

		// GHG
		public static readonly ModuleIdentifier CO2eDashboard = new ModuleIdentifier(ModuleId.CO2eDashboard, ResString.GetMultilingualString("Module.CO2eDashboard", "Greenhouse Gas Emissions Dashboard"));

		#endregion
	}

	public class ModuleList : RegistrationList<ModuleIdentifier, ModuleInfo>
	{
		#region List

		public ModuleList()
		{
			Add(new ModuleInfo(ModuleIDs.JobRequiredDocumentAddInfo, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.JobRequiredDocumentAddInfoModule"));
			Add(new ModuleInfo(ModuleIDs.GenCustomAddOnRule, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.GenCustomAddOnRuleModule"));

			Add(new ModuleInfo(ModuleIDs.ARComplianceDocument, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.ARComplianceDocumentModule"));
			Add(new ModuleInfo(ModuleIDs.APComplianceDocument, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.APComplianceDocumentModule"));
			Add(new ModuleInfo(ModuleIDs.AlternateChartofAccounts, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.AlternateChartofAccountsModule"));
			Add(new ModuleInfo(ModuleIDs.AlternateGLAccounts, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.AlternateGLAccountsModule"));
			Add(new ModuleInfo(ModuleIDs.PaymentBatch, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.PaymentBatchModule"));
			Add(new ModuleInfo(ModuleIDs.DialogDefault, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.DialogDefaultModule"));
			Add(new ModuleInfo(ModuleIDs.Orders, "Enterprise.Freight.Forwarding.Module", "Enterprise.Freight.Forwarding.Orders.Module.OrdersModule", new TableRegistrationInfo(JobOrderHeaderSchema.Constants.TableName)));
			Add(new ModuleInfo(ModuleIDs.JobShipmentPreplanning, "Enterprise.Freight.Forwarding.Module", "Enterprise.Freight.Forwarding.Orders.Module.JobShipmentPreplanningModule"));
			Add(new ModuleInfo(ModuleIDs.ClientRates, "Enterprise.Rating.Module", "Enterprise.Rating.Module.ClientRatesModule"));
			Add(new ModuleInfo(ModuleIDs.GlobalRates, "Enterprise.Rating.Module", "Enterprise.Rating.Module.CompanyTariffsModule"));
			Add(new ModuleInfo(ModuleIDs.UniversalChargeCode, "Enterprise.Rating.Module", "Enterprise.Rating.Module.UniversalChargeCodeModule"));
			Add(new ModuleInfo(ModuleIDs.CarrierChargeCode, "Enterprise.Rating.Module", "Enterprise.Rating.Module.CarrierChargeCodeModule"));
			Add(new ModuleInfo(ModuleIDs.UniversalCommodityCode, "Enterprise.Rating.Module", "Enterprise.Rating.Module.UniversalCommodityCodeModule"));
			Add(new ModuleInfo(ModuleIDs.UrsNamedAccount, "Enterprise.Rating.Module", "Enterprise.Rating.Module.UrsNamedAccountModule"));
			Add(new ModuleInfo(ModuleIDs.Quotations, "Enterprise.Rating.Module", "Enterprise.Rating.Module.QuotationsModule"));
			Add(new ModuleInfo(ModuleIDs.Costing, "Enterprise.Rating.Module", "Enterprise.Rating.Module.CostingModule"));
			Add(new ModuleInfo(ModuleIDs.IntercompanyTariffs, "Enterprise.Rating.Module", "Enterprise.Rating.Module.IntercompanyTariffModule"));
			Add(new ModuleInfo(ModuleIDs.WiseRates, "Enterprise.Rating.Module", "Enterprise.Rating.Module.WiseRatesModule"));
			Add(new ModuleInfo(ModuleIDs.WiseRatesCargoguide, "Enterprise.Rating.Module", "Enterprise.Rating.Module.CargoguideModule"));
			Add(new ModuleInfo(ModuleIDs.WiseRatesCargoSphere, "Enterprise.Rating.Module", "Enterprise.Rating.Module.CargoSphereModule"));
			Add(new ModuleInfo(ModuleIDs.CarrierConnect, "Enterprise.Rating.Module", "Enterprise.Rating.Module.GlowRateSelectorModule"));
			Add(new ModuleInfo(ModuleIDs.Organisation, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.OrganisationModule", new TableRegistrationInfo(OrgHeaderSchema.Constants.TableName)));
			Add(new ModuleInfo(ModuleIDs.StaffAssignments, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.StaffAssignmentsModule"));
			Add(new ModuleInfo(ModuleIDs.ClientIntelligence, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.ClientIntelligenceModule"));
			Add(new ModuleInfo(ModuleIDs.CompetitorIntelligence, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.CompetitorIntelligenceModule"));
			Add(new ModuleInfo(ModuleIDs.ProfitShare, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.OrgAgentRelationshipModule"));
			Add(new ModuleInfo(ModuleIDs.ARPaymentProcessing, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.ARPaymentProcessingModule"));
			Add(new ModuleInfo(ModuleIDs.APPaymentProcessing, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.APPaymentProcessingModule"));
			Add(new ModuleInfo(ModuleIDs.JobManagement, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.JobManagementModule"));
			Add(new ModuleInfo(ModuleIDs.BulkDSBJobCloseBatchApproval, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.BulkDSBJobCloseBatchApprovalModule"));
			Add(new ModuleInfo(ModuleIDs.TransactionsExport, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.TransactionsExportModule"));
			Add(new ModuleInfo(ModuleIDs.XmlTransactionsImport, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.XmlTransactionsImportModule"));
			Add(new ModuleInfo(ModuleIDs.CsvTransactionsImport, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.CsvTransactionsImportModule"));
			Add(new ModuleInfo(ModuleIDs.OustandingJournalsExport, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.XmlJournalExportModule"));
			Add(new ModuleInfo(ModuleIDs.OustandingJournalsImport, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.XmlJournalImportModule"));
			Add(new ModuleInfo(ModuleIDs.CsvAccountsImport, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.CsvAccountsImportModule"));
			Add(new ModuleInfo(ModuleIDs.WIPAccruals, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.WIPAccrualsModule"));
			Add(new ModuleInfo(ModuleIDs.JobRevenueJournal, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.JobRevenueJournalModule"));
			Add(new ModuleInfo(ModuleIDs.GLJournal, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.GLJournalModule"));
			Add(new ModuleInfo(ModuleIDs.GLJournal, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.GLJournalModuleChina", Constants.CountryCodes.China));
			Add(new ModuleInfo(ModuleIDs.GLJournal, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.GLJournalModuleChina", Constants.CountryCodes.Taiwan));
			Add(new ModuleInfo(ModuleIDs.GLJournalApproval, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.TransactionApproval.GLJournalApprovalModule"));
			Add(new ModuleInfo(ModuleIDs.GLConsolidationGroups, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.GLConsolidationGroupModule"));
			Add(new ModuleInfo(ModuleIDs.GLReportingBooksReport, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.GLReportingBooksReport"));
			Add(new ModuleInfo(ModuleIDs.Opportunity, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.OrgOpportunityModule", new TableRegistrationInfo(OrgOpportunitySchema.Constants.TableName)));
			Add(new ModuleInfo(ModuleIDs.CrmOpportunity, "Enterprise.CRM.Module", "Enterprise.CRM.Module.CrmOpportunityModule", new TableRegistrationInfo(CrmOpportunitySchema.Constants.TableName)));
			Add(new ModuleInfo(ModuleIDs.ServiceLevel, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.ServiceLevelModule", new TableRegistrationInfo(RefServiceLevelSchema.Constants.TableName)));
			Add(new ModuleInfo(ModuleIDs.RefVessel, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.RefVesselModule"));
			Add(new ModuleInfo(ModuleIDs.RefVesselZZ, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.RefVesselZZModule"));
			Add(new ModuleInfo(ModuleIDs.GLAccountFormat, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.GLAccountFormatModule"));
			Add(new ModuleInfo(ModuleIDs.ImportAccountingData, "Enterprise.DataConverters", "Enterprise.DataConverters.Accounting.AccountingImportModule"));
			Add(new ModuleInfo(ModuleIDs.Containers, "Enterprise.Freight.Module", "Enterprise.Freight.Module.ContainersModule", new TableRegistrationInfo(JobContainerSchema.Constants.TableName)));
			Add(new ModuleInfo(ModuleIDs.MarketIntelligenceAndAnalytics, "Enterprise.Freight.Module", "Enterprise.Freight.Module.MarketIntelligenceAndAnalyticsModule"));
			Add(new ModuleInfo(ModuleIDs.OceanCarrierBookingAnalysisReport, "Enterprise.Freight.Module", "Enterprise.Freight.Module.OceanCarrierBookingAnalysisReportModule"));
			Add(new ModuleInfo(ModuleIDs.ServiceRequest, "Enterprise.CustomerService.GUI", "Enterprise.CustomerService.Module.IncidentApprovalModule"));
			Add(new ModuleInfo(ModuleIDs.QuotedBookings, "Enterprise.Freight.QuotedBookings.Module", "Enterprise.Freight.QuotedBookings.Module.QuotedBookingModule"));
			Add(new ModuleInfo(ModuleIDs.OneOffQuotes, "Enterprise.Freight.QuotedBookings.Module", "Enterprise.Freight.QuotedBookings.Module.OneOffQuoteModule"));
			Add(new ModuleInfo(ModuleIDs.CarrierContractAndAllocations, "Enterprise.ContractManagement.Module", "Enterprise.ContractManagement.Module.CarrierContractGlowModule"));
			Add(new ModuleInfo(ModuleIDs.ClientContractAndAllocations, "Enterprise.ContractManagement.Module", "Enterprise.ContractManagement.Module.ClientContractModule"));
			Add(new ModuleInfo(ModuleIDs.JobShipment, "Enterprise.Freight.Forwarding.Module", "Enterprise.Freight.Forwarding.Module.JobShipmentModule", new TableRegistrationInfo(JobShipmentSchema.Constants.TableName)));
			Add(new ModuleInfo(ModuleIDs.JobShipment, "Enterprise.Freight.Forwarding.Module", "Enterprise.Freight.Forwarding.Module.JobShipmentModule", Constants.CountryCodes.Australia, new TableRegistrationInfo(JobShipmentSchema.Constants.TableName)));
			Add(new ModuleInfo(ModuleIDs.JobShipment, "Enterprise.Freight.Forwarding.Module", "Enterprise.Freight.Forwarding.Module.JobShipmentModule", Constants.CountryCodes.NewZealand, new TableRegistrationInfo(JobShipmentSchema.Constants.TableName)));
			Add(new ModuleInfo(ModuleIDs.JobConsol, "Enterprise.Freight.Forwarding.Module", "Enterprise.Freight.Forwarding.Module.JobConsolModule", new TableRegistrationInfo(JobConsolSchema.Constants.TableName)));
			Add(new ModuleInfo(ModuleIDs.ConsolPlanningBoard, "Enterprise.Freight.Forwarding.Module", "Enterprise.Freight.Forwarding.Module.ConsolDashboardModule"));
			Add(new ModuleInfo(ModuleIDs.PackLines, "Enterprise.Freight.Forwarding.Module", "Enterprise.Freight.Forwarding.Module.PackLinesModule"));
			Add(new ModuleInfo(ModuleIDs.CarrierContracts, "Enterprise.ContractManagement.Module", "Enterprise.ContractManagement.Module.CarrierContractModule"));
			Add(new ModuleInfo(ModuleIDs.ContractAllocationRoutes, "Enterprise.ContractManagement.Module", "Enterprise.ContractManagement.Module.AllocationRouteModule"));
			Add(new ModuleInfo(ModuleIDs.GatewayConsolProfitShareRedistribution, "Enterprise.Freight.Forwarding.Module", "Enterprise.Freight.Forwarding.Module.GatewayConsolProfitShareRedistributionModule"));
			Add(new ModuleInfo(ModuleIDs.SupplierBooking, "Enterprise.Freight.Forwarding.Module", "Enterprise.Freight.Forwarding.Module.JobSupplierBookingModule"));
			Add(new ModuleInfo(ModuleIDs.SupplierBookingLine, "Enterprise.Freight.Forwarding.Module", "Enterprise.Freight.Forwarding.Module.JobSupplierBookingLineModule"));
			Add(new ModuleInfo(ModuleIDs.RelatedTransportLegs, "Enterprise.Freight.Forwarding.Module", "Enterprise.Freight.Forwarding.Module.RelatedTransportLegsModule"));
			Add(new ModuleInfo(ModuleIDs.ELoadList, "Enterprise.eManifest.Module", "Enterprise.eManifest.Module.ELoadListModule"));
			Add(new ModuleInfo(ModuleIDs.RoutingLookups, "Enterprise.Freight.Forwarding.Routing.S8.Module", "Enterprise.Freight.Forwarding.Routing.S8.Module.RealTimeRoutingModule"));
			Add(new ModuleInfo(ModuleIDs.RefCommodityCode, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.RefCommodityCodeModule"));
			Add(new ModuleInfo(ModuleIDs.RefAirlineCommodityCode, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.RefAirlineCommodityCodeModule"));
			Add(new ModuleInfo(ModuleIDs.RefOrgPartCategory, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.RefOrgPartCategoryModule"));
			Add(new ModuleInfo(ModuleIDs.RefEquipment, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.RefEquipmentModule"));
			Add(new ModuleInfo(ModuleIDs.ProductionRulesPortal, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.ProductionRulesPortalModule"));
			Add(new ModuleInfo(ModuleIDs.RefPremisesGateCode, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.RefPremisesGateCodeModule"));
			Add(new ModuleInfo(ModuleIDs.AccChargeCode, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.AccChargeCodeModule"));
			Add(new ModuleInfo(ModuleIDs.AccChargeCodeForRegistry, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.AccChargeCodeModuleForRegistry"));
			Add(new ModuleInfo(ModuleIDs.AccGlobalChargeCode, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.AccGlobalChargeCodeModule"));
			Add(new ModuleInfo(ModuleIDs.AccTaxOverrideGroup, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.AccTaxOverrideGroupModule"));
			Add(new ModuleInfo(ModuleIDs.AccReportingBook, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.AccReportingBookModule"));
			Add(new ModuleInfo(ModuleIDs.AccPlaceOfSupplyChargeCodeGroup, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.AccPOSChargeCodeGroupModule"));
			Add(new ModuleInfo(ModuleIDs.AccOrgTaxConfigurationTemplate, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.AccOrgTaxConfigurationTemplateModule"));
			Add(new ModuleInfo(ModuleIDs.GlobalChargeCodeIntercompany, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.GlobalChargeCodeIntercompanyModule"));
			Add(new ModuleInfo(ModuleIDs.GlobalChargeCodeOrganization, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.GlobalChargeCodeOrganizationModule"));
			Add(new ModuleInfo(ModuleIDs.RefCurrency, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.RefCurrencyModule"));
			Add(new ModuleInfo(ModuleIDs.InternationalZone, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.InternationalZonesModule", new TableRegistrationInfo(RefZoneHeaderSchema.Constants.TableName)));
			Add(new ModuleInfo(ModuleIDs.RefTimeZoneSet, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.RefTimeZoneSetModule"));
			Add(new ModuleInfo(ModuleIDs.RefNMFC, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.RefNMFCModule"));
			Add(new ModuleInfo(ModuleIDs.RefCountry, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.RefCountryModule"));
			Add(new ModuleInfo(ModuleIDs.RefCityTown, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.RefCityTownModule"));
			Add(new ModuleInfo(ModuleIDs.RefCountryStates, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.RefCountryStatesModule"));
			Add(new ModuleInfo(ModuleIDs.RefPostCode, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.RefPostCodeModule"));
			Add(new ModuleInfo(ModuleIDs.OrgAddresses, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.OrgAddressesModule"));
			Add(new ModuleInfo(ModuleIDs.GenShapeGeography, "Enterprise.MasterData.Module", "Enterprise.MasterData.Module.GenShapeGeographyModule"));
			Add(new ModuleInfo(ModuleIDs.OrgCusCode, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.OrgCusCodeModule"));
			Add(new ModuleInfo(ModuleIDs.Sales, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.OrgSalesModule"));
			Add(new ModuleInfo(ModuleIDs.SalesEnquiry, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.SalesEnquiryModule", new TableRegistrationInfo(OrgColdCallRegisterSchema.Constants.TableName)));
			Add(new ModuleInfo(ModuleIDs.SalesProduct, "Enterprise.MarketingManager.Module", "Enterprise.MarketingManager.Module.SalesProductModule"));
			Add(new ModuleInfo(ModuleIDs.Commission, "Enterprise.CommissionManagement.Module", "Enterprise.CommissionManagement.Module.CommissionManagementModule"));
			Add(new ModuleInfo(ModuleIDs.CommissionApprovalRequest, "Enterprise.CommissionManagement.Module", "Enterprise.CommissionManagement.Module.CommissionApprovalRequestModule"));
			Add(new ModuleInfo(ModuleIDs.CommissionAgreementLogFilter, "Enterprise.CommissionManagement.GUI", "Enterprise.CommissionManagement.GUI.CommissionAgreementLogFilterModule"));
			Add(new ModuleInfo(ModuleIDs.Communication, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.CommunicationModule"));
			Add(new ModuleInfo(ModuleIDs.OrgCommissionAgreement, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.CommissionAgreementModule"));
			Add(new ModuleInfo(ModuleIDs.OrgContacts, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.OrgContactsModule", new TableRegistrationInfo(OrgContactSchema.Constants.TableName)));
			Add(new ModuleInfo(ModuleIDs.OrgContactsStmALog, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.OrgContactsStmALogModule"));
			Add(new ModuleInfo(ModuleIDs.RefContainer, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.RefContainerModule"));
			Add(new ModuleInfo(ModuleIDs.RefContainerISOTypes, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.RefContainerISOTypesModule"));
			Add(new ModuleInfo(ModuleIDs.CountryStatesGlbHoliday, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.CountryStatesGlbHolidayModule"));
			Add(new ModuleInfo(ModuleIDs.RefUNLOCO, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.RefUNLOCOModule"));
			Add(new ModuleInfo(ModuleIDs.DocumentTracking, "Enterprise.Freight.Forwarding.Module", "Enterprise.Freight.Forwarding.Module.DocumentTrackingModule"));
			Add(new ModuleInfo(ModuleIDs.OrgCreditorGroup, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.OrgCreditorGroupModule"));
			Add(new ModuleInfo(ModuleIDs.OrgDebtorGroup, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.OrgDebtorGroupModule"));
			Add(new ModuleInfo(ModuleIDs.RefDocType, "Enterprise.DocumentScanning.Module", "Enterprise.DocumentScanning.Module.RefDocTypeModule"));
			Add(new ModuleInfo(ModuleIDs.RefDocSource, "Enterprise.DocumentScanning.Module", "Enterprise.DocumentScanning.Module.RefDocSourceModule"));
			Add(new ModuleInfo(ModuleIDs.RefDocOrgCusCode, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.RefDocOrgCusCodeModule"));
			Add(new ModuleInfo(ModuleIDs.GlbBranch, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.GlbBranchModule"));
			Add(new ModuleInfo(ModuleIDs.GlbBranchNotCurrentCompanyRelated, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.GlbBranchNotCurrentCompanyRelatedModule"));
			Add(new ModuleInfo(ModuleIDs.GlbCompany, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.GlbCompanyModule"));
			Add(new ModuleInfo(ModuleIDs.GlbCompanyCampaign, "Enterprise.MarketingManager.Module", "Enterprise.MarketingManager.Module.CRMGlbCompanyCampaignModule", new TableRegistrationInfo(GlbCompanyCampaignSchema.Constants.TableName)));
			Add(new ModuleInfo(ModuleIDs.GlbCompanyCampaignWithoutFilter, "Enterprise.MarketingManager.Module", "Enterprise.MarketingManager.Module.GlbCompanyCampaignWithoutFilterModule"));
			Add(new ModuleInfo(ModuleIDs.GlbCompanyCampaignItem, "Enterprise.MarketingManager.GUI", "Enterprise.MarketingManager.GUI.GlbCompanyCampaignItemModule"));
			Add(new ModuleInfo(ModuleIDs.GlbCompanyCampaignItemSchedule, "Enterprise.MarketingManager.GUI", "Enterprise.MarketingManager.GUI.SendScheduleModule"));
			Add(new ModuleInfo(ModuleIDs.GlbCompanyCampaignClick, "Enterprise.MarketingManager.GUI", "Enterprise.MarketingManager.GUI.GlbCompanyCampaignClickModule"));
			Add(new ModuleInfo(ModuleIDs.GlbCompanyCampaignContact, "Enterprise.MarketingManager.GUI", "Enterprise.MarketingManager.GUI.GlbCompanyCampaignContactModule"));
			Add(new ModuleInfo(ModuleIDs.SalesDashboard, "Enterprise.MarketingManager.Module", "Enterprise.MarketingManager.Module.SalesDashboardModule"));
			Add(new ModuleInfo(ModuleIDs.OrgSalesDashboard, "Enterprise.MarketingManager.Module", "Enterprise.MarketingManager.Module.OrgSalesDashboardModule"));
			Add(new ModuleInfo(ModuleIDs.GlbDepartment, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.GlbDepartmentModule", new TableRegistrationInfo(GlbDepartmentSchema.Constants.TableName)));
			Add(new ModuleInfo(ModuleIDs.GlbGroup, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.GlbGroupModule"));
			Add(new ModuleInfo(ModuleIDs.WarningAcknowledgement, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.WarningAcknowledgementModule"));
			Add(new ModuleInfo(ModuleIDs.NewsAndAnnouncement, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.NewsAndAnnouncementModule"));
			Add(new ModuleInfo(ModuleIDs.AccGroups, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.AccGroupsModule"));
			Add(new ModuleInfo(ModuleIDs.AccGLAccountDescriptor, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.AccGLAccountDescriptorModule"));
			Add(new ModuleInfo(ModuleIDs.AccGLHeader, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.AccGLHeaderModule"));
			Add(new ModuleInfo(ModuleIDs.AccChequeBook, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.AccChequeBookModule"));
			Add(new ModuleInfo(ModuleIDs.AccBankAccount, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.AccBankAccountModule"));
			Add(new ModuleInfo(ModuleIDs.OrderLine, "Enterprise.Freight.Forwarding.Module", "Enterprise.Freight.Forwarding.Orders.Module.OrderLineModule"));
			Add(new ModuleInfo(ModuleIDs.ForwardingReport, "Enterprise.Freight.Forwarding.Module", "Enterprise.Freight.Forwarding.Module.ForwardingReportsModule"));
			Add(new ModuleInfo(ModuleIDs.AccTaxRate, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.AccTaxRateModule"));
			Add(new ModuleInfo(ModuleIDs.AccTaxRateForRegistry, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.AccTaxRateModuleForRegistry"));
			Add(new ModuleInfo(ModuleIDs.AccInvMsg, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.AccInvMsgModule"));
			Add(new ModuleInfo(ModuleIDs.AccWithholding, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.AccWithholdingModule"));
			Add(new ModuleInfo(ModuleIDs.Location, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.LocationModule"));
			Add(new ModuleInfo(ModuleIDs.ViewLocation, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.ViewLocationModule"));
			Add(new ModuleInfo(ModuleIDs.TradeLane, "Enterprise.Freight.Module", "Enterprise.Freight.Module.JobTradeLaneModule"));
			Add(new ModuleInfo(ModuleIDs.JobAirSailing, "Enterprise.Freight.Module", "Enterprise.Freight.Module.JobAirSailingModule"));
			Add(new ModuleInfo(ModuleIDs.DocumentAllocation, "Enterprise.DocumentScanning.Module", "Enterprise.DocumentScanning.Module.DocumentAllocationModule"));
			Add(new ModuleInfo(ModuleIDs.DocumentDbMerger, "Enterprise.DocumentScanning.Module", "Enterprise.DocumentScanning.Module.DocumentDbMergerModule"));
			Add(new ModuleInfo(ModuleIDs.DocumentDbManager, "Enterprise.DocumentScanning.Module", "Enterprise.DocumentScanning.Module.DocumentDbManagerModule"));
			Add(new ModuleInfo(ModuleIDs.ArchiveEDocs, "Enterprise.DocumentScanning.Module", "Enterprise.DocumentScanning.Module.ArchiveEDocsModule"));
			Add(new ModuleInfo(ModuleIDs.DocumentTemplate, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.StmTemplateModule"));
			Add(new ModuleInfo(ModuleIDs.RateAttachmentSet, "Enterprise.Rating.Module", "Enterprise.Rating.Module.RateAttachmentSetModule"));
			Add(new ModuleInfo(ModuleIDs.SalesTeam, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.SalesTeamModule"));
			Add(new ModuleInfo(ModuleIDs.SalesRep, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.SalesRepModule"));
			Add(new ModuleInfo(ModuleIDs.RateTransportProvider, "Enterprise.Rating.Module", "Enterprise.Rating.Module.RateTransportProviderModule"));
			Add(new ModuleInfo(ModuleIDs.RateTransportZone, "Enterprise.Rating.Module", "Enterprise.Rating.Module.RateTransportZoneModule"));
			Add(new ModuleInfo(ModuleIDs.PeriodManagement, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.PeriodManagementModule"));
			Add(new ModuleInfo(ModuleIDs.OrdersReport, "Enterprise.Freight.Forwarding.Module", "Enterprise.Freight.Forwarding.Orders.Module.OrdersReportModule"));
			Add(new ModuleInfo(ModuleIDs.ShipmentReceival, "Enterprise.Freight.CFS.Module", "Enterprise.Freight.CFS.Module.ShipmentReceivalModule", new TableRegistrationInfo(JobShipmentSchema.Constants.TableName)));
			Add(new ModuleInfo(ModuleIDs.AccHotCheque, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.AccHotChequeModule"));
			Add(new ModuleInfo(ModuleIDs.JobCostingReport, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.JobCostingReportModule"));
			Add(new ModuleInfo(ModuleIDs.PackContainerRegistration, "Enterprise.Freight.CFS.Module", "Enterprise.Freight.CFS.Module.PackContainerRegistrationModule"));
			Add(new ModuleInfo(ModuleIDs.LoadListConsol, "Enterprise.Freight.CFS.Module", "Enterprise.Freight.CFS.Module.LoadListConsolModule"));
			Add(new ModuleInfo(ModuleIDs.JobMawb, "Enterprise.Freight.Forwarding.Module", "Enterprise.Freight.Forwarding.Module.JobMawbModule"));
			Add(new ModuleInfo(ModuleIDs.PrintJob, "Enterprise.DocumentEngine.Module", "Enterprise.DocumentEngine.Module.PrintJobModule"));
			Add(new ModuleInfo(ModuleIDs.DocumentSigningJob, "Enterprise.DocumentEngine.Module", "Enterprise.DocumentEngine.Module.DocumentSigningJobModule"));
			Add(new ModuleInfo(ModuleIDs.ScheduledReports, "Enterprise.DocumentEngine.Module", "Enterprise.DocumentEngine.Scheduler.Module.ScheduledReportsModule", new TableRegistrationInfo(StmScheduleTaskSchema.Constants.TableName)));
			Add(new ModuleInfo(ModuleIDs.ReportStatistics, "Enterprise.DocumentEngine.Module", "Enterprise.DocumentEngine.Scheduler.Module.ReportStatisticsModule", new TableRegistrationInfo(StmReportRunSchema.Constants.TableName)));
			Add(new ModuleInfo(ModuleIDs.UpdateNotesPortal, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.UpdateNotesPortalModule"));
			Add(new ModuleInfo(ModuleIDs.ReportManagement, "Enterprise.DocumentEngine.Module", "Enterprise.DocumentEngine.Scheduler.Module.ReportManagementModule", new TableRegistrationInfo(StmScheduleTaskSchema.Constants.TableName)));
			Add(new ModuleInfo(ModuleIDs.StmServiceTask, "Enterprise.ServiceManager.Module", "Enterprise.ServiceManager.Module.StmServiceTaskModule"));
			Add(new ModuleInfo(ModuleIDs.ProcessController, "Enterprise.ServiceManager.Module", "Enterprise.ServiceManager.Module.ProcessControllerModule"));
			Add(new ModuleInfo(ModuleIDs.LicenceUsage, "Enterprise.Licensing.Module", "Enterprise.Licencing.Module.LicenceUsageFilterGridModule"));
			Add(new ModuleInfo(ModuleIDs.PrintQueue, "Enterprise.DocumentEngine.Module", "Enterprise.DocumentEngine.Module.PrintQueueModule"));
			Add(new ModuleInfo(ModuleIDs.StmMenuItem, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.StmMenuItemModule"));
			Add(new ModuleInfo(ModuleIDs.GlbStaff, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.GlbStaffModule", new TableRegistrationInfo(GlbStaffSchema.Constants.TableName)));
			Add(new ModuleInfo(ModuleIDs.GlbStaffChangeRequest, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.GlbStaffChangeRequestModule"));
			Add(new ModuleInfo(ModuleIDs.GlbStaffHoliday, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.GlbStaffHolidayModule"));
			Add(new ModuleInfo(ModuleIDs.GlbPerson, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.GlbPersonModule"));
			Add(new ModuleInfo(ModuleIDs.GlbCapability, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.GlbCapabilityModule", new TableRegistrationInfo(GlbCapabilitySchema.Constants.TableName)));
			Add(new ModuleInfo(ModuleIDs.ActiveUsers, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.ActiveUsersModule"));
			Add(new ModuleInfo(ModuleIDs.BookingsReports, "Enterprise.Freight.Module", "Enterprise.Freight.Module.BookingReports"));
			Add(new ModuleInfo(ModuleIDs.TransportReports, "Enterprise.Freight.Module", "Enterprise.Freight.Module.TransportReports"));
			Add(new ModuleInfo(ModuleIDs.CFSCTOReports, "Enterprise.Freight.CFS.Module", "Enterprise.Freight.CFS.Module.CFSCTOReports"));
			Add(new ModuleInfo(ModuleIDs.SalesMgrReports, "Enterprise.Rating.Module", "Enterprise.Rating.Module.SalesMgrReports"));
			Add(new ModuleInfo(ModuleIDs.TariffRateReports, "Enterprise.Rating.Module", "Enterprise.Rating.Module.TariffsAndRatesReports"));
			Add(new ModuleInfo(ModuleIDs.DocManagerReports, "Enterprise.DocumentScanning.Module", "Enterprise.DocumentScanning.Module.DocManagerReports"));
			Add(new ModuleInfo(ModuleIDs.ManifestTally, "Enterprise.Freight.CFS.Module", "Enterprise.Freight.CFS.Module.ManifestTallyModule"));
			Add(new ModuleInfo(ModuleIDs.ShipmentGatePass, "Enterprise.Freight.CFS.Module", "Enterprise.Freight.CFS.Module.ShipmentGatePassModule"));
			Add(new ModuleInfo(ModuleIDs.JobSeaSailing, "Enterprise.Freight.Module", "Enterprise.Freight.Module.JobSeaSailingModule"));
			Add(new ModuleInfo(ModuleIDs.JobRailSailing, "Enterprise.Freight.Module", "Enterprise.Freight.Module.JobRailSailingModule"));
			Add(new ModuleInfo(ModuleIDs.JobRoadSailing, "Enterprise.Freight.Module", "Enterprise.Freight.Module.JobRoadSailingModule"));
			Add(new ModuleInfo(ModuleIDs.JobSeaVoyage, "Enterprise.Freight.Module", "Enterprise.Freight.Module.JobSeaVoyageModule"));
			Add(new ModuleInfo(ModuleIDs.PortDepotCarrierSelection, "Enterprise.Freight.PortHubs.Module", "Enterprise.Freight.PortHubs.Module.PortDepotCarrierSelectionModule"));
			Add(new ModuleInfo(ModuleIDs.PortHubSelection, "Enterprise.Freight.PortHubs.Module", "Enterprise.Freight.PortHubs.Module.PortHubSelectionModule"));
			Add(new ModuleInfo(ModuleIDs.ErrorReporting, "Enterprise.ErrorReporting.Module", "Enterprise.ErrorReporting.Module.ErrorReportingModule"));
			Add(new ModuleInfo(ModuleIDs.LDaaSDevices, "Enterprise.Telematics.Module", "Enterprise.Telematics.Module.LDaaSDevicesModule"));
			Add(new ModuleInfo(ModuleIDs.TelematicsPreDriveChecklistTemplates, "Enterprise.Telematics.Module", "Enterprise.Telematics.Module.TelematicsPreDriveChecklistTemplatesModule"));
			Add(new ModuleInfo(ModuleIDs.TelematicsPreDriveChecklists, "Enterprise.Telematics.Module", "Enterprise.Telematics.Module.TelematicsPreDriveChecklistModule"));
			Add(new ModuleInfo(ModuleIDs.AdministrationPanel, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.AdministrationPanelModule"));
			Add(new ModuleInfo(ModuleIDs.RefTransitTime, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.RefTransitTimeModule"));
			Add(new ModuleInfo(ModuleIDs.TransitTimeServiceLevelCombination, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.TransitTimeServiceLevelCombinationModule"));
			Add(new ModuleInfo(ModuleIDs.HVLVBookingHeader, "Enterprise.eTail.Module", "Enterprise.eTail.Module.HVLVBookingHeaderModule"));
			Add(new ModuleInfo(ModuleIDs.HVLVOriginLoadList, "Enterprise.eTail.Module", "Enterprise.eTail.Module.HVLVOriginLoadListModule"));
			Add(new ModuleInfo(ModuleIDs.HVLVOuterPackage, "Enterprise.eTail.Module", "Enterprise.eTail.Module.HVLVOuterPackageModule"));
			Add(new ModuleInfo(ModuleIDs.HVLVConsignment, "Enterprise.eTail.Module", "Enterprise.eTail.Module.HVLVConsignmentModule"));

			Add(new ModuleInfo(ModuleIDs.ReceivReports, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.ReceivReports"));
			Add(new ModuleInfo(ModuleIDs.PayablesReports, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.PayablesReports"));
			Add(new ModuleInfo(ModuleIDs.CashBookReports, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.CashBookReports"));
			Add(new ModuleInfo(ModuleIDs.GLReports, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.GLReports"));
			Add(new ModuleInfo(ModuleIDs.BudgetReports, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.BudgetReports"));
			Add(new ModuleInfo(ModuleIDs.NettingReports, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.NettingReports"));
			Add(new ModuleInfo(ModuleIDs.EISReports, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.EISReports"));
			Add(new ModuleInfo(ModuleIDs.Statement, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.StatementModule"));
			Add(new ModuleInfo(ModuleIDs.RefFilesReports, "Enterprise.DocumentEngine.Module", "Enterprise.MasterFiles.Module.RefFilesReports"));
			Add(new ModuleInfo(ModuleIDs.ArchiveReports, "Enterprise.DocumentEngine.Module", "Enterprise.MasterFiles.Module.ArchiveReports"));
			Add(new ModuleInfo(ModuleIDs.MasterDataReports, "Enterprise.DocumentEngine.Module", "Enterprise.MasterFiles.Module.MasterDataReports"));
			Add(new ModuleInfo(ModuleIDs.LocationsReports, "Enterprise.DocumentEngine.Module", "Enterprise.MasterFiles.Module.LocationsReports"));
			Add(new ModuleInfo(ModuleIDs.AccountReports, "Enterprise.DocumentEngine.Module", "Enterprise.MasterFiles.Module.AccountReports"));
			Add(new ModuleInfo(ModuleIDs.CustFilesReports, "Enterprise.DocumentEngine.Module", "Enterprise.MasterFiles.Module.CustFilesReports"));
			Add(new ModuleInfo(ModuleIDs.ProcessMgrReports, "Enterprise.DocumentEngine.Module", "Enterprise.MasterFiles.Module.ProcessMgrReports"));
			Add(new ModuleInfo(ModuleIDs.SystemReports, "Enterprise.DocumentEngine.Module", "Enterprise.MasterFiles.Module.SystemReports"));
			Add(new ModuleInfo(ModuleIDs.UserAdminReports, "Enterprise.DocumentEngine.Module", "Enterprise.MasterFiles.Module.UserAdminReports"));
			Add(new ModuleInfo(ModuleIDs.ARTransaction, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.ARTransactionModuleStrip"));
			Add(new ModuleInfo(ModuleIDs.APTransaction, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.APTransactionModuleStrip"));
			Add(new ModuleInfo(ModuleIDs.TransactionsPendingAllocation, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.Transaction.TransactionsPendingAllocationModule"));
			Add(new ModuleInfo(ModuleIDs.CashbookTransaction, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.CashBookTransactionModule"));
			Add(new ModuleInfo(ModuleIDs.Cheque, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.ChequeModule"));
			Add(new ModuleInfo(ModuleIDs.ChequeTransaction, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.ChequeTransactionModule"));
			Add(new ModuleInfo(ModuleIDs.GenericCharge, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.GenericChargeModule"));
			Add(new ModuleInfo(ModuleIDs.GenericConsol, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.GenericConsolModule"));
			Add(new ModuleInfo(ModuleIDs.GenericTransaction, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.GenericTransactionModule"));
			Add(new ModuleInfo(ModuleIDs.APEnquiry, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.APEnquiryModule"));
			Add(new ModuleInfo(ModuleIDs.AREnquiry, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.AREnquiryModule"));
			Add(new ModuleInfo(ModuleIDs.ARCreditNoteApproval, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.TransactionApproval.ARCreditNoteApprovalModule"));
			Add(new ModuleInfo(ModuleIDs.TransactionsPendingAllocationApproval, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.TransactionApproval.TransactionPendingAllocationApprovalModule"));
			Add(new ModuleInfo(ModuleIDs.APInvoiceApproval, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.TransactionApproval.APInvoiceApprovalModule"));
			Add(new ModuleInfo(ModuleIDs.CreditControlledDocumentsApproval, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.CreditControlledDocumentsApprovalModule"));

			Add(new ModuleInfo(ModuleIDs.UnapprovedTransaction, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.UnapprovedTransactionModule"));
			Add(new ModuleInfo(ModuleIDs.UnapprovedIntercompanyTransaction, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.UnapprovedIntercompanyTransactionModule"));
			Add(new ModuleInfo(ModuleIDs.CASSCostFileImport, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.CASSBillingModule"));
			Add(new ModuleInfo(ModuleIDs.JobBillingExRateSysConfig, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.JobBillingExRateConfigModule"));
			Add(new ModuleInfo(ModuleIDs.CNDataInterface, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.CNDataInterfaceModule"));
			Add(new ModuleInfo(ModuleIDs.CN2004DataInterface, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.CN2004DataInterfaceModule"));
			Add(new ModuleInfo(ModuleIDs.CNReconciliationExport, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.CNReconciliationExportModule"));
			Add(new ModuleInfo(ModuleIDs.AccountingVoucher, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.AccountingVoucherModule"));
			Add(new ModuleInfo(ModuleIDs.ChinaJournalListing, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.ChinaJournalListingModule"));
			Add(new ModuleInfo(ModuleIDs.DirectDebitFile, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.DirectDebitFileModule"));
			Add(new ModuleInfo(ModuleIDs.DepositBatch, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.DepositBatchModule"));
			Add(new ModuleInfo(ModuleIDs.BankReconcilliation, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.BankReconcilliationModule"));
			Add(new ModuleInfo(ModuleIDs.InvoiceBatch, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.InvoiceBatchModule"));
			Add(new ModuleInfo(ModuleIDs.InvoicePrinting, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.InvoicePrintingModule"));
			Add(new ModuleInfo(ModuleIDs.JobHeader, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.JobManagementModuleBase"));
			Add(new ModuleInfo(ModuleIDs.OrgMatchApproval, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.OrgMatchApprovalModule"));
			Add(new ModuleInfo(ModuleIDs.Registry, "Enterprise.Registry.GUI", "Enterprise.Registry.GUI.RegistryModule"));
			Add(new ModuleInfo(ModuleIDs.StmFeatureTest, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.StmFeatureTestModule"));
			Add(new ModuleInfo(ModuleIDs.APIncompleteInvoices, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.Transaction.APIncompleteInvoicesModule"));
			Add(new ModuleInfo(ModuleIDs.APInvoiceProcessingPortal, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.APInvoiceProcessingPortalModule"));

			Add(new ModuleInfo(ModuleIDs.Cartage, "Enterprise.Freight.LocalCartage.Module", "Enterprise.Freight.LocalCartage.Module.CartageModule"));
			Add(new ModuleInfo(ModuleIDs.CartageWorkSheet, "Enterprise.Freight.LocalCartage.Module", "Enterprise.Freight.LocalCartage.Module.CartageWorkSheetModule"));
			Add(new ModuleInfo(ModuleIDs.CartageLeg, "Enterprise.Freight.LocalCartage.Module", "Enterprise.Freight.LocalCartage.Module.CartageLegModule"));
			Add(new ModuleInfo(ModuleIDs.CartageLegPlanner, "Enterprise.Freight.LocalCartage.Module", "Enterprise.Freight.LocalCartage.Module.CartageLegPlannerModule"));
			Add(new ModuleInfo(ModuleIDs.CartageRunSheetDashboard, "Enterprise.Freight.LocalCartage.Module", "Enterprise.Freight.LocalCartage.Module.CartageRunSheetDashboardModule"));

			Add(new ModuleInfo(ModuleIDs.ConsolidatedTransportBooking, "Enterprise.Freight.Confirmations.Module", "Enterprise.Freight.Confirmations.Module.ConsolidatedTransportBookingModule"));
			Add(new ModuleInfo(ModuleIDs.PickupDeliveryConfirm, "Enterprise.Freight.Confirmations.Module", "Enterprise.Freight.Confirmations.Module.PickupDeliveryConfirmModule"));

			Add(new ModuleInfo(ModuleIDs.CartageType, "Enterprise.Freight.Common.Module", "Enterprise.Freight.Common.Module.CartageTypeModule"));
			Add(new ModuleInfo(ModuleIDs.StmUpgrade, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.StmUpgradeModule"));
			Add(new ModuleInfo(ModuleIDs.UNDGSubstance, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.UNDGSubstanceModule"));
			Add(new ModuleInfo(ModuleIDs.UNDGCommonData, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.UNDGCommonDataModule"));
			Add(new ModuleInfo(ModuleIDs.UNDGCountryReference, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.UNDGCountryReferenceModule"));

			Add(new ModuleInfo(ModuleIDs.RefCarrierConsortium, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.RefCarrierConsortiumModule"));
			Add(new ModuleInfo(ModuleIDs.GLBudget, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.GLBudgetModule"));
			Add(new ModuleInfo(ModuleIDs.APAccQueryClaim, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.APAccQueryClaimModule"));
			Add(new ModuleInfo(ModuleIDs.ARAccQueryClaim, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.ARAccQueryClaimModule"));
			Add(new ModuleInfo(ModuleIDs.OrgCollectionCalls, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.OrgCollectionCallsModule"));
			Add(new ModuleInfo(ModuleIDs.GlbPortDeliveryTime, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.GlbPortDeliveryTimeModule"));
			Add(new ModuleInfo(ModuleIDs.ZARMatching, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.ARMatchingModule"));
			Add(new ModuleInfo(ModuleIDs.ZAPMatching, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.APMatchingModule"));
			Add(new ModuleInfo(ModuleIDs.AccApportionmentTemplate, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.AccApportionmentTemplateModule"));
			Add(new ModuleInfo(ModuleIDs.RefAirline, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.RefAirlineModule"));
			Add(new ModuleInfo(ModuleIDs.NumericCodeRefAirline, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.NumericCodeRefAirlineModule"));
			Add(new ModuleInfo(ModuleIDs.RefPackType, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.RefPackTypeModule"));
			Add(new ModuleInfo(ModuleIDs.RefDomesticCartageZone, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.RefDomesticCartageZoneModule"));
			Add(new ModuleInfo(ModuleIDs.MailItem, "MailManager.Module", "Enterprise.MailManager.Module.MailItemModule"));
			Add(new ModuleInfo(ModuleIDs.MailItemTemplate, "MailManager.Module", "Enterprise.MailManager.Module.MailItemTemplateModule"));
			Add(new ModuleInfo(ModuleIDs.StmALog, "Enterprise.ZArchitecture.GUI.UserControls", "Enterprise.ZArchitecture.GUI.ZStmALogModule"));
			Add(new ModuleInfo(ModuleIDs.StmModuleFilter, "Enterprise.ZArchitecture.GUI.UserControls", "Enterprise.ZArchitecture.GUI.ZStmModuleFilterModule"));
			Add(new ModuleInfo(ModuleIDs.AccComplianceSequence, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.AccComplianceSequenceModule"));
			Add(new ModuleInfo(ModuleIDs.AccCollectionBatch, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.AccCollectionBatchModule"));
			Add(new ModuleInfo(ModuleIDs.AccCollectionOrder, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.AccCollectionOrderModule"));
			Add(new ModuleInfo(ModuleIDs.AccGeneralLedgerData, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.AccGeneralLedgerDataModule"));
			Add(new ModuleInfo(ModuleIDs.AccComplianceReport, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.AccComplianceReportModule"));
			Add(new ModuleInfo(ModuleIDs.AccPayableOrder, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.AccPayableOrderModule"));
			Add(new ModuleInfo(ModuleIDs.RefShippingLine, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.RefShippingLineModule"));
			Add(new ModuleInfo(ModuleIDs.RefFacility, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.RefFacilityModule"));
			Add(new ModuleInfo(ModuleIDs.RefComplianceList, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.RefComplianceListModule"));
			Add(new ModuleInfo(ModuleIDs.RefComplianceCommodityAlert, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.RefComplianceCommodityAlertModule"));
			Add(new ModuleInfo(ModuleIDs.ARCashAdvance, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.ARCashAdvanceModule"));
			Add(new ModuleInfo(ModuleIDs.APCashAdvance, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.APCashAdvanceModule"));

			Add(new ModuleInfo(ModuleIDs.NettingStatement, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.NettingStatementModule"));
			Add(new ModuleInfo(ModuleIDs.NettingPeriod, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.NettingPeriodModule"));

			Add(new ModuleInfo(ModuleIDs.AssetManagementPortal, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.AssetManagementPortalModule"));
			Add(new ModuleInfo(ModuleIDs.AssetManagementReports, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.AssetManagementReports"));

			Add(new ModuleInfo(ModuleIDs.ExchangeRate, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.RefExchangeRateModule"));

			// Agency
			Add(new ModuleInfo(ModuleIDs.AgencyBooking, "Enterprise.Freight.Agency.Module", "Enterprise.Freight.Agency.Module.BookingModule"));
			Add(new ModuleInfo(ModuleIDs.AgencyBillOfLading, "Enterprise.Freight.Agency.Module", "Enterprise.Freight.Agency.Module.BillOfLadingModule"));
			Add(new ModuleInfo(ModuleIDs.AgencyBillContainers, "Enterprise.Freight.Agency.Module", "Enterprise.Freight.Agency.Module.BillContainersModule"));
			Add(new ModuleInfo(ModuleIDs.AgencyContainerDetention, "Enterprise.Freight.Agency.Module", "Enterprise.Freight.Agency.Module.ContainerDetentionModule"));
			Add(new ModuleInfo(ModuleIDs.AgencyContainerManager, "Enterprise.Freight.Agency.Module", "Enterprise.Freight.Agency.Module.ContainerManagerModule"));
			Add(new ModuleInfo(ModuleIDs.AgencyContainerMove, "Enterprise.Freight.Agency.Module", "Enterprise.Freight.Agency.Module.ContainerMoveModule"));
			Add(new ModuleInfo(ModuleIDs.AgencyVoyageAccounting, "Enterprise.Freight.Agency.Module", "Enterprise.Freight.Agency.Module.VoyageAccountingModule"));
			Add(new ModuleInfo(ModuleIDs.AgencySundryCharges, "Enterprise.Freight.Agency.Module", "Enterprise.Freight.Agency.Module.SundryChargesModule"));
			Add(new ModuleInfo(ModuleIDs.AgencyReports, "Enterprise.Freight.Agency.Module", "Enterprise.Freight.Agency.Module.AgencyReports"));

			// Ocean Carrier
			Add(new ModuleInfo(ModuleIDs.OceanCarrierPortal, "Enterprise.OceanCarrier.Module", "Enterprise.OceanCarrier.Module.OceanCarrierPortalModule"));
			Add(new ModuleInfo(ModuleIDs.CarrierShipmentHeader, "Enterprise.OceanCarrier.Module", "Enterprise.OceanCarrier.Module.CarrierShipmentHeaderModule"));
			Add(new ModuleInfo(ModuleIDs.RouteSegments, "Enterprise.OceanCarrier.Module", "Enterprise.OceanCarrier.Module.RouteSegmentsModule"));
			Add(new ModuleInfo(ModuleIDs.CarrierServices, "Enterprise.OceanCarrier.Module", "Enterprise.OceanCarrier.Module.CarrierServicesModule"));

			// Equipment Management
			Add(new ModuleInfo(ModuleIDs.EquipmentManagementPortal, "Enterprise.EquipmentManagement.Module", "Enterprise.EquipmentManagement.Module.EquipmentManagementPortalModule"));

			// Barcode Parsing
			Add(new ModuleInfo(ModuleIDs.BarcodeParsing, "Enterprise.BarcodeParsing.Module", "Enterprise.BarcodeParsing.Module.BarcodeParsingModule"));

			// Barcode Validation
			Add(new ModuleInfo(ModuleIDs.BarcodeValidation, "Enterprise.BarcodeParsing.Module", "Enterprise.BarcodeParsing.Module.BarcodeValidationModule"));

			// Carrier Messaging Buss
			Add(new ModuleInfo(ModuleIDs.RefAccessorial, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.RefAccessorialModule"));
			Add(new ModuleInfo(ModuleIDs.RefMessagingBussCarrierInfo, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.RefMessagingBussCarrierInfoModule"));

			// Equipment Combination
			Add(new ModuleInfo(ModuleIDs.RefJobEquipment, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.RefJobEquipmentModule"));

			// domestic transport booking
			Add(new ModuleInfo(ModuleIDs.DtbBooking, "Enterprise.TransportBookings.Module", "Enterprise.TransportBookings.Module.DtbBookingModule"));
			Add(new ModuleInfo(ModuleIDs.DtbBookingConsolidation, "Enterprise.TransportBookings.Module", "Enterprise.TransportBookings.Module.DtbBookingConsolidationModule"));
			Add(new ModuleInfo(ModuleIDs.DtbBookingTmpl, "Enterprise.TransportBookings.Module", "Enterprise.TransportBookings.Module.DtbBookingTmplModule"));
			Add(new ModuleInfo(ModuleIDs.DtbBookingReports, "Enterprise.TransportBookings.Module", "Enterprise.TransportBookings.Module.ReportModule"));

			// land transport consignment
			Add(new ModuleInfo(ModuleIDs.DtbConsignment, "Enterprise.TransportConsignment.Module", "Enterprise.TransportConsignment.Module.DtbConsignmentModule"));

			// domestic transport consignment
			Add(new ModuleInfo(ModuleIDs.DtbBookingConsignment, "Enterprise.TransportConsignment.Module", "Enterprise.TransportConsignment.Module.DtbBookingConsignmentModule"));
			Add(new ModuleInfo(ModuleIDs.DtbConsignmentRunSheet, "Enterprise.TransportConsignment.Module", "Enterprise.TransportConsignment.Module.DtbConsignmentRunSheetModule"));
			Add(new ModuleInfo(ModuleIDs.DtbRoutePlanner, "Enterprise.TransportConsignment.Module", "Enterprise.TransportConsignment.Module.DtbRoutePlannerModule"));
			Add(new ModuleInfo(ModuleIDs.DtbReports, "Enterprise.TransportConsignment.Module", "Enterprise.TransportConsignment.Module.DtbReportsModule"));
			Add(new ModuleInfo(ModuleIDs.DtbConsignmentWebPortal, "Enterprise.TransportConsignment.Module", "Enterprise.TransportConsignment.Module.DtbConsignmentWebPortalModule"));
			Add(new ModuleInfo(ModuleIDs.DtbConsignmentRunSheetWebPortal, "Enterprise.TransportConsignment.Module", "Enterprise.TransportConsignment.Module.DtbConsignmentRunSheetWebPortalModule"));
			//
			// Product Warehouse
			Add(new ModuleInfo(ModuleIDs.WhsConfigWarehouse, "Enterprise.Warehouse.Environment.Module", "Enterprise.Warehouse.Environment.Module.WarehouseModule"));
			Add(new ModuleInfo(ModuleIDs.WhsConfigRow, "Enterprise.Warehouse.Environment.Module", "Enterprise.Warehouse.Environment.Module.RowModule"));
			Add(new ModuleInfo(ModuleIDs.WhsConfigLocation, "Enterprise.Warehouse.Environment.Module", "Enterprise.Warehouse.Environment.Module.LocationModule"));
			Add(new ModuleInfo(ModuleIDs.WhsConfigArea, "Enterprise.Warehouse.Environment.Module", "Enterprise.Warehouse.Environment.Module.AreaModule"));
			Add(new ModuleInfo(ModuleIDs.WhsConfigLocationType, "Enterprise.Warehouse.Environment.Module", "Enterprise.Warehouse.Environment.Module.LocationTypeModule"));
			Add(new ModuleInfo(ModuleIDs.WhsConfigDynamicPickFaces, "Enterprise.Warehouse.Environment.Module", "Enterprise.Warehouse.Environment.Module.DynamicPickFacesModule"));
			Add(new ModuleInfo(ModuleIDs.WhsConfigPickFaces, "Enterprise.Warehouse.Environment.Module", "Enterprise.Warehouse.Environment.Module.PickFacesModule"));
			Add(new ModuleInfo(ModuleIDs.WhsConfigProduct, "Enterprise.Warehouse.Transactions.Module", "Enterprise.Warehouse.Transactions.Module.ProductModule"));
			Add(new ModuleInfo(ModuleIDs.WhsConfigProductStyle, "Enterprise.Warehouse.Environment.Module", "Enterprise.Warehouse.Environment.Module.ProductStyleModule"));
			Add(new ModuleInfo(ModuleIDs.WhsConfigPutawayGroup, "Enterprise.Warehouse.Environment.Module", "Enterprise.Warehouse.Environment.Module.WhsPutawayGroupModule"));
			Add(new ModuleInfo(ModuleIDs.WhsReceive, "Enterprise.Warehouse.Transactions.Module", "Enterprise.Warehouse.Transactions.Module.ReceiveModule"));
			Add(new ModuleInfo(ModuleIDs.WhsOrder, "Enterprise.Warehouse.Transactions.Module", "Enterprise.Warehouse.Transactions.Module.OrderModule"));
			Add(new ModuleInfo(ModuleIDs.WhsOrderLine, "Enterprise.Warehouse.Transactions.Module", "Enterprise.Warehouse.Transactions.Module.OrderLineModule"));
			Add(new ModuleInfo(ModuleIDs.WhsWorkOrder, "Enterprise.Warehouse.Transactions.Module", "Enterprise.Warehouse.Transactions.Module.WorkOrderModule"));
			Add(new ModuleInfo(ModuleIDs.WhsDynamicWorkOrder, "Enterprise.Warehouse.Transactions.Module", "Enterprise.Warehouse.Transactions.Module.DynamicWorkOrderModule"));
			Add(new ModuleInfo(ModuleIDs.WhsPicking, "Enterprise.Warehouse.Transactions.Module", "Enterprise.Warehouse.Transactions.Module.PickingModule"));
			Add(new ModuleInfo(ModuleIDs.WhsRelease, "Enterprise.Warehouse.Transactions.Module", "Enterprise.Warehouse.Transactions.Module.ReleaseModule"));
			Add(new ModuleInfo(ModuleIDs.WhsReleasePackageJob, "Enterprise.Warehouse.Transactions.Module", "Enterprise.Warehouse.Transactions.Module.ReleasePackageJobModule"));
			Add(new ModuleInfo(ModuleIDs.WhsTransfer, "Enterprise.Warehouse.Transactions.Module", "Enterprise.Warehouse.Transactions.Module.TransferModule"));
			Add(new ModuleInfo(ModuleIDs.WhsAdjustment, "Enterprise.Warehouse.Transactions.Module", "Enterprise.Warehouse.Transactions.Module.AdjustmentModule"));
			Add(new ModuleInfo(ModuleIDs.WhsStocktake, "Enterprise.Warehouse.Transactions.Module", "Enterprise.Warehouse.Transactions.Module.StocktakeModule"));
			Add(new ModuleInfo(ModuleIDs.WhsHandlingUnit, "Enterprise.Warehouse.Transactions.Module", "Enterprise.Warehouse.Transactions.Module.WhsHandlingUnitModule"));
			Add(new ModuleInfo(ModuleIDs.WhsInventory, "Enterprise.Warehouse.Transactions.Module", "Enterprise.Warehouse.Transactions.Module.InventoryModule"));
			Add(new ModuleInfo(ModuleIDs.WhsInventoryHeldCodes, " Enterprise.Warehouse.Transactions.Module", "Enterprise.Warehouse.Transactions.Module.WhsInventoryHeldCodeModule"));
			Add(new ModuleInfo(ModuleIDs.WhsReport, "Enterprise.Warehouse.Transactions.Module", "Enterprise.Warehouse.Transactions.Module.ReportModule"));
			Add(new ModuleInfo(ModuleIDs.WhsInvoicing, "Enterprise.Warehouse.Transactions.Module", "Enterprise.Warehouse.Transactions.Module.InvoicingModule"));
			Add(new ModuleInfo(ModuleIDs.WhsEntryLine, "Enterprise.Warehouse.Transactions.Module", "Enterprise.Warehouse.Transactions.Module.EntryLineModule"));
			Add(new ModuleInfo(ModuleIDs.WhsPickLine, "Enterprise.Warehouse.Transactions.Module", "Enterprise.Warehouse.Transactions.Module.PickLineModule"));
			Add(new ModuleInfo(ModuleIDs.WhsVASOrder, "Enterprise.Warehouse.Transactions.Module", "Enterprise.Warehouse.Transactions.Module.VASOrderModule"));
			Add(new ModuleInfo(ModuleIDs.WhsCartonSize, "Enterprise.Warehouse.Environment.Module", "Enterprise.Warehouse.Environment.Module.WhsCartonSizeModule"));
			Add(new ModuleInfo(ModuleIDs.WhsCartonGroup, "Enterprise.Warehouse.Environment.Module", "Enterprise.Warehouse.Environment.Module.WhsCartonGroupModule"));
			Add(new ModuleInfo(ModuleIDs.WhsAdHocServiceJob, "Enterprise.Warehouse.Transactions.Module", "Enterprise.Warehouse.Transactions.Module.AdHocServiceJobModule"));
			Add(new ModuleInfo(ModuleIDs.WhsSalesChannel, "Enterprise.Warehouse.Environment.Module", "Enterprise.Warehouse.Environment.Module.WhsSalesChannelModule"));
			Add(new ModuleInfo(ModuleIDs.WhsProductionRulesPortal, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.ProductionRulesPortalModule"));
			Add(new ModuleInfo(ModuleIDs.WhsProductWarehousePortal, "Enterprise.Warehouse.Transactions.Module", "Enterprise.Warehouse.Transactions.Module.ProductWarehousePortalModule"));
			Add(new ModuleInfo(ModuleIDs.WhsLoad, "Enterprise.Warehouse.Transactions.Module", "Enterprise.Warehouse.Transactions.Module.LoadModule"));

			// Transit Warehouse
			Add(new ModuleInfo(ModuleIDs.WhsItemReceiveTransportationUnit, "Enterprise.Warehouse.Transit.Module", "Enterprise.Warehouse.Transit.Module.WhsItemReceiveTransportationUnitModule"));
			Add(new ModuleInfo(ModuleIDs.WhsItemDispatchTransportationUnit, "Enterprise.Warehouse.Transit.Module", "Enterprise.Warehouse.Transit.Module.WhsItemDispatchTransportationUnitModule"));
			Add(new ModuleInfo(ModuleIDs.WhsTransitReport, "Enterprise.Warehouse.Transactions.Module", "Enterprise.Warehouse.Transactions.Module.TransitReportModule"));
			Add(new ModuleInfo(ModuleIDs.WhsTransitReceiveConsignment, "Enterprise.Warehouse.Transit.Module", "Enterprise.Warehouse.Transit.Module.WhsTransitReceiveConsignmentModule"));
			Add(new ModuleInfo(ModuleIDs.WhsTransitDispatchConsignment, "Enterprise.Warehouse.Transit.Module", "Enterprise.Warehouse.Transit.Module.WhsTransitDispatchConsignmentModule"));
			Add(new ModuleInfo(ModuleIDs.WhsItemReceiveASN, "Enterprise.Warehouse.Transit.Module", "Enterprise.Warehouse.Transit.Module.WhsItemReceiveASNModule"));
			Add(new ModuleInfo(ModuleIDs.TransitHandlingUnit, "Enterprise.Warehouse.Transit.Module", "Enterprise.Warehouse.Transit.Module.TransitHandlingUnitModule"));
			Add(new ModuleInfo(ModuleIDs.TransitWarehouseAttachPackages, "Enterprise.Warehouse.Transit.Module", "Enterprise.Warehouse.Transit.Module.TransitPackageStateAttachModule"));
			Add(new ModuleInfo(ModuleIDs.TransitWarehouseViewPackages, "Enterprise.Warehouse.Transit.Module", "Enterprise.Warehouse.Transit.Module.ViewAssignedPackagesModule"));
			Add(new ModuleInfo(ModuleIDs.WhsItemTransferHeader, "Enterprise.Warehouse.Transit.Module", "Enterprise.Warehouse.Transit.Module.WhsItemTransferHeaderModule"));
			Add(new ModuleInfo(ModuleIDs.WhsItemDispatchLoadList, "Enterprise.Warehouse.Transit.Module", "Enterprise.Warehouse.Transit.Module.WhsItemDispatchLoadListModule"));
			Add(new ModuleInfo(ModuleIDs.TransitWarehousePortal, "Enterprise.Warehouse.Transit.Module", "Enterprise.Warehouse.Transit.Module.TransitWarehousePortalModule"));

			// Packing
			Add(new ModuleInfo(ModuleIDs.Packing, "Enterprise.Packing.Module", "Enterprise.Packing.Module.PackingModule"));
			Add(new ModuleInfo(ModuleIDs.PalletTransaction, "Enterprise.Packing.Module", "Enterprise.Packing.Module.PalletTransactionModule"));

			// Customs
			Add(new ModuleInfo(ModuleIDs.Customs.ImportCustomsFilesData, "Enterprise.DataConverters", "Enterprise.DataConverters.CustomsFiles.CustomsFilesImportModule"));
			Add(new ModuleInfo(ModuleIDs.SupplierPart, "Enterprise.Customs.Module", "Enterprise.Customs.Module.OrgSupplierPartModule"));
			Add(new ModuleInfo(ModuleIDs.EntryLine, "Enterprise.Customs.Module", "Enterprise.Customs.Module.EntryLineModule"));
			Add(new ModuleInfo(ModuleIDs.Customs.EntryHeader, "Enterprise.Customs.Module", "Enterprise.Customs.Module.EntryHeaderModule"));
			Add(new ModuleInfo(ModuleIDs.CustomsGlobalReport, "Enterprise.Customs.Module", "Enterprise.Customs.Module.CustomsGlobalReportModule"));
			Add(new ModuleInfo(ModuleIDs.CustomsReport, "Enterprise.Customs.Module", "Enterprise.Customs.Module.CustomsReportModule"));
			Add(new ModuleInfo(ModuleIDs.CommercialInvoice, "Enterprise.Customs.Module", "Enterprise.Customs.Module.CommercialInvoiceModule"));
			Add(new ModuleInfo(ModuleIDs.CopyCommercialInvoice, "Enterprise.Customs.Module", "Enterprise.Customs.Module.CopyCommercialInvoiceModule"));
			Add(new ModuleInfo(ModuleIDs.Customs.JobDeclaration, "Enterprise.Customs.Module", "Enterprise.Customs.Module.JobDeclarationModule", new TableRegistrationInfo(JobDeclarationSchema.Constants.TableName)));
			Add(new ModuleInfo(ModuleIDs.SingleTariffClassification, "Enterprise.Customs.Module", "Enterprise.Customs.Module.SingleTariffClassificationModule"));
			Add(new ModuleInfo(ModuleIDs.Customs.RefPacks, "Enterprise.Customs.Module", "Enterprise.Customs.Shared.Module.RefPacksModule"));
			Add(new ModuleInfo(ModuleIDs.Customs.Guarantees, "Enterprise.Customs.Module", "Enterprise.Customs.Module.GuaranteesModule"));
			Add(new ModuleInfo(ModuleIDs.Customs.CustomsRules, "Enterprise.Customs.Module", "Enterprise.Customs.Module.CustomsRulesModule"));
			Add(new ModuleInfo(ModuleIDs.ImporterSecurityFiling, "Enterprise.Customs.US.ISF.Module", "Enterprise.Customs.US.ISF.Module.ISFModule"));
			Add(new ModuleInfo(ModuleIDs.Customs.CA.CAHouseBilleManifest, "Enterprise.Customs.CA.Module", "Enterprise.Customs.CA.Module.HouseBilleManifestModule"));
			Add(new ModuleInfo(ModuleIDs.Customs.Permits, "Enterprise.Customs.Module", "Enterprise.Customs.Module.CusPermitModule"));
			Add(new ModuleInfo(ModuleIDs.CusPerson, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.CusPersonModule"));
			Add(new ModuleInfo(ModuleIDs.BorderWiseWebReturnHook, "Enterprise.Customs.Module", "Enterprise.Customs.Module.BorderWiseWebReturnHookModule"));
			Add(new ModuleInfo(ModuleIDs.Customs.CusAuthorisations, "Enterprise.Customs.Module", "Enterprise.Customs.Module.CusAuthorisationsModule"));
			Add(new ModuleInfo(ModuleIDs.Customs.CusRefPreference, "Enterprise.Customs.Module", "Enterprise.Customs.Module.CusRefPreferenceModule"));
			Add(new ModuleInfo(ModuleIDs.Customs.CusRefRateCode, "Enterprise.Customs.Module", "Enterprise.Customs.Module.CusRefRateCodeModule"));
			Add(new ModuleInfo(ModuleIDs.Customs.CusPackingList, "Enterprise.Customs.Module", "Enterprise.Customs.Module.CusPackingListModule"));
			Add(new ModuleInfo(ModuleIDs.Customs.TradeGroups, "Enterprise.Customs.Module", "Enterprise.Customs.Module.TradeGroupsModule"));
			Add(new ModuleInfo(ModuleIDs.Customs.CusRefTariffVersion, "Enterprise.Customs.Module", "Enterprise.Customs.Module.CusRefTariffVersionModule"));
			Add(new ModuleInfo(ModuleIDs.Customs.GoodsCatalog, "Enterprise.Customs.Module", "Enterprise.Customs.Module.GoodsCatalogModule"));
			Add(new ModuleInfo(ModuleIDs.Customs.SumARegister, "Enterprise.Customs.EFTA.TemporaryStorageRegister.Module", "Enterprise.Customs.EFTA.TemporaryStorageRegister.Module.SumARegisterModule"));
			Add(new ModuleInfo(ModuleIDs.Customs.SumARegisterReadOnly, "Enterprise.Customs.EFTA.TemporaryStorageRegister.Module", "Enterprise.Customs.EFTA.TemporaryStorageRegister.Module.SumARegisterReadOnlyModule"));
			Add(new ModuleInfo(ModuleIDs.Customs.ImportFromTemporaryStorageRegister, "Enterprise.Customs.EFTA.TemporaryStorageRegister.Module", "Enterprise.Customs.EFTA.TemporaryStorageRegister.Module.ImportFromTemporaryStorageRegisterModule"));

			// AU
			Add(new ModuleInfo(ModuleIDs.ImportClassification, "Enterprise.Customs.AU.Module", "Enterprise.Customs.AU.Module.ImportClassificationModule", Constants.CountryCodes.Australia));
			Add(new ModuleInfo(ModuleIDs.ExportClassification, "Enterprise.Customs.AU.Module", "Enterprise.Customs.AU.Module.ExportClassificationModule", Constants.CountryCodes.Australia));
			Add(new ModuleInfo(ModuleIDs.ImportTariffBulkChange, "Enterprise.Customs.AU.Module", "Enterprise.Customs.AU.Module.AUImportTariffBulkChangeModule", Constants.CountryCodes.Australia));
			Add(new ModuleInfo(ModuleIDs.ExportTariffBulkChange, "Enterprise.Customs.AU.Module", "Enterprise.Customs.AU.Module.AUExportTariffBulkChangeModule", Constants.CountryCodes.Australia));
			Add(new ModuleInfo(ModuleIDs.Customs.AU.AirCargo, "Enterprise.Customs.AU.Module", "Enterprise.Customs.AU.Module.AirCargo.AUCustomsAirCargoModule", Constants.CountryCodes.Australia));
			Add(new ModuleInfo(ModuleIDs.Customs.AU.AirCargoDepot, "Enterprise.Customs.AU.Module", "Enterprise.Customs.AU.Module.AirCargo.AirCargoDepotStandAloneModule", Constants.CountryCodes.Australia));
			Add(new ModuleInfo(ModuleIDs.Customs.AU.HouseAirCargo, "Enterprise.Customs.AU.Module", "Enterprise.Customs.AU.Module.AirCargo.AUCustomsHouseAirCargoModule", Constants.CountryCodes.Australia));
			Add(new ModuleInfo(ModuleIDs.Customs.AU.CusSCADepotContainer, "Enterprise.Customs.AU.Module", "Enterprise.Customs.AU.Module.SeaCargo.CusSCADepotContainerModule", Constants.CountryCodes.Australia));
			Add(new ModuleInfo(ModuleIDs.Customs.AU.CusSCADepotHouse, "Enterprise.Customs.AU.Module", "Enterprise.Customs.AU.Module.SeaCargo.CusSCADepotHouseModule", Constants.CountryCodes.Australia));
			Add(new ModuleInfo(ModuleIDs.Customs.JobDeclaration, "Enterprise.Customs.AU.Module", "Enterprise.Customs.AU.Module.JobDeclarationModule", Constants.CountryCodes.Australia));
			Add(new ModuleInfo(ModuleIDs.Customs.AU.ExportCustomsManifest, "Enterprise.Customs.AU.Module", "Enterprise.Customs.AU.Module.ExportCustomsManifestModule", Constants.CountryCodes.Australia));
			Add(new ModuleInfo(ModuleIDs.Customs.AU.AirCTOImport, "Enterprise.Customs.AU.Module", "Enterprise.Customs.AU.Module.AirCTOImportModule", Constants.CountryCodes.Australia));
			Add(new ModuleInfo(ModuleIDs.Customs.AU.AirCTOExport, "Enterprise.Customs.AU.Module", "Enterprise.Customs.AU.Module.AirCTOExportModule", Constants.CountryCodes.Australia));
			Add(new ModuleInfo(ModuleIDs.Customs.AU.AirCargoOutturnBills, "Enterprise.Customs.AU.Module", "Enterprise.Customs.AU.Module.AirCargoOutturnBillsModule", Constants.CountryCodes.Australia));
			Add(new ModuleInfo(ModuleIDs.Customs.AU.VoyageManifest, "Enterprise.Customs.AU.Module", "Enterprise.Customs.AU.Module.VoyageManifestModule", Constants.CountryCodes.Australia));
			Add(new ModuleInfo(ModuleIDs.Customs.AU.SeaCargo, "Enterprise.Customs.AU.Module", "Enterprise.Customs.AU.Module.SeaCargoModule", Constants.CountryCodes.Australia));
			Add(new ModuleInfo(ModuleIDs.Customs.AU.SeaCargoDepot, "Enterprise.Customs.AU.Module", "Enterprise.Customs.AU.Module.SeaCargoDepotModule", Constants.CountryCodes.Australia));
			Add(new ModuleInfo(ModuleIDs.Customs.AU.SeaCargoOutturnBills, "Enterprise.Customs.AU.Module", "Enterprise.Customs.AU.Module.SeaCargoOutturnBillsModule", Constants.CountryCodes.Australia));
			Add(new ModuleInfo(ModuleIDs.Customs.AU.HouseSeaCargo, "Enterprise.Customs.AU.Module", "Enterprise.Customs.AU.Module.SeaCargoHouseModule", Constants.CountryCodes.Australia));
			Add(new ModuleInfo(ModuleIDs.AQISProducerCode, "Enterprise.Customs.AU.Module", "Enterprise.Customs.AU.Module.AQISProducerCodeModule", Constants.CountryCodes.Australia));
			Add(new ModuleInfo(ModuleIDs.Premises, "Enterprise.Customs.AU.Module", "Enterprise.Customs.AU.Module.PremisesModule", Constants.CountryCodes.Australia));
			Add(new ModuleInfo(ModuleIDs.CMRCodeLists, "Enterprise.Customs.AU.Module", "Enterprise.Customs.AU.Module.CMRCodeListsModule", Constants.CountryCodes.Australia));
			Add(new ModuleInfo(ModuleIDs.InstrumentNumber, "Enterprise.Customs.AU.Module", "Enterprise.Customs.AU.Module.InstrumentNumberModule", Constants.CountryCodes.Australia));
			Add(new ModuleInfo(ModuleIDs.CMRLodgementQuestion, "Enterprise.Customs.AU.Module", "Enterprise.Customs.AU.Module.CMRLodgementQuestionModule", Constants.CountryCodes.Australia));
			Add(new ModuleInfo(ModuleIDs.SupplierPart, "Enterprise.Customs.AU.Module", "Enterprise.Customs.AU.Module.OrgSupplierPartModule", Constants.CountryCodes.Australia));
			Add(new ModuleInfo(ModuleIDs.SailingDataVendorImporting, "Enterprise.Freight.Module", "Enterprise.Freight.SailingDataVendor.Module.SailingScheduleImportingModule", Constants.CountryCodes.Australia));
			Add(new ModuleInfo(ModuleIDs.OnlineSailingSchedules, "Enterprise.Freight.Module", "Enterprise.Freight.Module.OnlineSailingSchedulesModule"));
			Add(new ModuleInfo(ModuleIDs.ComPayRegisteredOrganisations, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.ComPayRegisteredOrganisationsModule", Constants.CountryCodes.Australia));
			Add(new ModuleInfo(ModuleIDs.SendTestCustomsMessage, "Enterprise.Customs.AU.Module", "Enterprise.Customs.AU.Module.AUSendTestCustomsMessageModule", Constants.CountryCodes.Australia));
			Add(new ModuleInfo(ModuleIDs.Customs.AU.CMREstablishmentCodes, "Enterprise.Customs.AU.Module", "Enterprise.Customs.AU.Module.CMREstablishmentCodesModule", Constants.CountryCodes.Australia));
			Add(new ModuleInfo(ModuleIDs.Customs.AU.DrawbackEntryLine, "Enterprise.Customs.AU.Module", "Enterprise.Customs.AU.Module.DrawbackEntryLineModule", Constants.CountryCodes.Australia));
			Add(new ModuleInfo(ModuleIDs.Customs.AU.NexDocNotifications, "Enterprise.Customs.AU.Module", "Enterprise.Customs.AU.Module.NEXDOCNotificationModule", Constants.CountryCodes.Australia));
			Add(new ModuleInfo(ModuleIDs.CommercialInvoice, "Enterprise.Customs.AU.Module", "Enterprise.Customs.AU.Module.CommercialInvoiceModule", Constants.CountryCodes.Australia));
			Add(new ModuleInfo(ModuleIDs.Customs.ConsolidatedDeclaration, "Enterprise.Customs.AU.Module", "Enterprise.Customs.AU.Module.ConsolidatedDeclarationModule", Constants.CountryCodes.Australia));
			Add(new ModuleInfo(ModuleIDs.Customs.CusCalculationRules, "Enterprise.Customs.AU.Module", "Enterprise.Customs.AU.Module.CusCalculationRulesModule", Constants.CountryCodes.Australia));

			// CA
			Add(new ModuleInfo(ModuleIDs.Customs.JobDeclaration, "Enterprise.Customs.CA.Module", "Enterprise.Customs.CA.Module.JobDeclarationModule", Constants.CountryCodes.Canada));
			Add(new ModuleInfo(ModuleIDs.SupplierPart, "Enterprise.Customs.CA.Module", "Enterprise.Customs.CA.Module.OrgSupplierPartModule", Constants.CountryCodes.Canada));
			Add(new ModuleInfo(ModuleIDs.Customs.CA.HTSTariffBulkChange, "Enterprise.Customs.CA.Module", "Enterprise.Customs.CA.Module.CAHTSTariffBulkChangeModule", Constants.CountryCodes.Canada));
			Add(new ModuleInfo(ModuleIDs.Customs.CA.ClassTariff, "Enterprise.Customs.CA.Module", "Enterprise.Customs.CA.Module.CACClassModule", Constants.CountryCodes.Canada));
			Add(new ModuleInfo(ModuleIDs.Customs.CA.ExportTariff, "Enterprise.Customs.CA.Module", "Enterprise.Customs.CA.Module.CACExportTariffModule", Constants.CountryCodes.Canada));
			Add(new ModuleInfo(ModuleIDs.Customs.CA.CAQueryMessages, "Enterprise.Customs.CA.Module", "Enterprise.Customs.CA.Module.CAQueryMessagesModule", Constants.CountryCodes.Canada));
			Add(new ModuleInfo(ModuleIDs.Customs.CA.K84Reports, "Enterprise.Customs.CA.Module", "Enterprise.Customs.CA.Module.K84ReportsModule", Constants.CountryCodes.Canada));
			Add(new ModuleInfo(ModuleIDs.Customs.CA.CADailyNoticeReconciliation, "Enterprise.Customs.CA.Module", "Enterprise.Customs.CA.Module.DailyNoticeReconciliationModule", Constants.CountryCodes.Canada));
			Add(new ModuleInfo(ModuleIDs.Customs.CA.CAARLStatementOfAccount, "Enterprise.Customs.CA.Module", "Enterprise.Customs.CA.Module.ARLStatementOfAccountModule", Constants.CountryCodes.Canada));
			Add(new ModuleInfo(ModuleIDs.Customs.CA.CAReleaseNotifications, "Enterprise.Customs.CA.Module", "Enterprise.Customs.CA.Module.CAReleaseNotificationsModule", Constants.CountryCodes.Canada));
			Add(new ModuleInfo(ModuleIDs.Customs.CA.CAExportClassification, "Enterprise.Customs.CA.Module", "Enterprise.Customs.CA.Module.ExportClassificationModule", Constants.CountryCodes.Canada));
			Add(new ModuleInfo(ModuleIDs.Customs.CA.HTSClassification, "Enterprise.Customs.CA.Module", "Enterprise.Customs.CA.Module.HTSClassificationModule", Constants.CountryCodes.Canada));
			Add(new ModuleInfo(ModuleIDs.Customs.CA.SubLocation, "Enterprise.Customs.CA.Module", "Enterprise.Customs.CA.Module.CACSubLocationModule"));
			Add(new ModuleInfo(ModuleIDs.Customs.CA.CFIAEndUseCodes, "Enterprise.Customs.CA.Module", "Enterprise.Customs.CA.Module.CACFIAEndUseCodesModule", Constants.CountryCodes.Canada));
			Add(new ModuleInfo(ModuleIDs.Customs.CA.CFIAMiscCodes, "Enterprise.Customs.CA.Module", "Enterprise.Customs.CA.Module.CACFIAMiscCodesModule", Constants.CountryCodes.Canada));
			Add(new ModuleInfo(ModuleIDs.Customs.CA.CATransactionNumberSetting, "Enterprise.Customs.CA.Module", "Enterprise.Customs.CA.Module.TransactionNumberSettingModule", Constants.CountryCodes.Canada));
			Add(new ModuleInfo(ModuleIDs.Customs.CA.B2Adjustments, "Enterprise.Customs.CA.Module", "Enterprise.Customs.CA.Module.B2AdjustmentsModule", Constants.CountryCodes.Canada));
			Add(new ModuleInfo(ModuleIDs.Customs.CA.CAManifestForward, "Enterprise.Customs.CA.Module", "Enterprise.Customs.CA.Module.ManifestForwardModule", Constants.CountryCodes.Canada));
			Add(new ModuleInfo(ModuleIDs.Customs.CA.CALVXJobs, "Enterprise.Customs.CA.Module", "Enterprise.Customs.CA.Module.LVXModule", Constants.CountryCodes.Canada));
			Add(new ModuleInfo(ModuleIDs.Customs.CA.CAJobDocAddresses, "Enterprise.Customs.CA.Module", "Enterprise.Customs.CA.Module.JobDocAddressesModule", Constants.CountryCodes.Canada));
			Add(new ModuleInfo(ModuleIDs.JobRequiredDocumentAddInfo, "Enterprise.Customs.CA.Module", "Enterprise.Customs.CA.Module.JobRequiredDocumentAddInfoModule", Constants.CountryCodes.Canada));
			Add(new ModuleInfo(ModuleIDs.Customs.Permits, "Enterprise.Customs.CA.Module", "Enterprise.Customs.CA.Module.CACusPermitModule", Constants.CountryCodes.Canada));
			Add(new ModuleInfo(ModuleIDs.Customs.CA.CACusRuling, "Enterprise.Customs.CA.Module", "Enterprise.Customs.CA.Module.CACusRulingModule", Constants.CountryCodes.Canada));
			Add(new ModuleInfo(ModuleIDs.Customs.CA.CACSARevenueSummaryForm, "Enterprise.Customs.CA.Module", "Enterprise.Customs.CA.Module.CSARevenueSummaryFormModule", Constants.CountryCodes.Canada));
			Add(new ModuleInfo(ModuleIDs.CommercialInvoice, "Enterprise.Customs.CA.Module", "Enterprise.Customs.CA.Module.CommercialInvoiceModule", Constants.CountryCodes.Canada));

			//CN
			Add(new ModuleInfo(ModuleIDs.SingleTariffClassification, "Enterprise.Customs.CN.Module", "Enterprise.Customs.CN.Module.CusClassificationModule", Constants.CountryCodes.China));
			Add(new ModuleInfo(ModuleIDs.Customs.JobDeclaration, "Enterprise.Customs.CN.Module", "Enterprise.Customs.CN.Module.JobDeclarationModule", Constants.CountryCodes.China));
			Add(new ModuleInfo(ModuleIDs.SupplierPart, "Enterprise.Customs.CN.Module", "Enterprise.Customs.CN.Module.OrgSupplierPartModule", Constants.CountryCodes.China));
			Add(new ModuleInfo(ModuleIDs.Customs.EntryHeader, "Enterprise.Customs.CN.Module", "Enterprise.Customs.CN.Module.EntryHeaderModule", Constants.CountryCodes.China));

			// EU / Latvia (test country)
#if DEBUG
			Add(new ModuleInfo(ModuleIDs.Customs.EU.CusClassificationModule, "Enterprise.Customs.EU.Module", "Enterprise.Customs.EU.Module.CusClassificationModule", Constants.CountryCodes.Latvia));
#endif

			//GB
			Add(new ModuleInfo(ModuleIDs.Customs.EU.GB.CcsukGenralMessage, "Enterprise.Customs.GB.Module", "Enterprise.Customs.GB.Module.CcsukGenralMessageModule", Constants.CountryCodes.UnitedKingdom));
			Add(new ModuleInfo(ModuleIDs.Customs.EU.GB.CcsukAirInventory, "Enterprise.Customs.GB.Module", "Enterprise.Customs.GB.Module.CcsukAirInventoryModule", Constants.CountryCodes.UnitedKingdom));
			Add(new ModuleInfo(ModuleIDs.Customs.EU.GB.CcsukAirInventoryHouse, "Enterprise.Customs.GB.Module", "Enterprise.Customs.GB.Module.CcsukAirInventoryHouseModule", Constants.CountryCodes.UnitedKingdom));
			Add(new ModuleInfo(ModuleIDs.Customs.EU.GB.CcsukMasterAndHouseCombined, "Enterprise.Customs.GB.Module", "Enterprise.Customs.GB.Module.CcsukMasterAndHouseCombinedModule", Constants.CountryCodes.UnitedKingdom));
			Add(new ModuleInfo(ModuleIDs.Customs.EU.GB.DLUMessage, "Enterprise.Customs.GB.Module", "Enterprise.Customs.GB.Module.DLU.DLUMessageModule", Constants.CountryCodes.UnitedKingdom));
			Add(new ModuleInfo(ModuleIDs.Customs.EU.GB.CcsukStandAloneFsrEnquiry, "Enterprise.Customs.GB.Module", "Enterprise.Customs.GB.Module.StandAloneFsrEnquiry.StandAloneFsrEnquiryModule", Constants.CountryCodes.UnitedKingdom));
			Add(new ModuleInfo(ModuleIDs.Customs.EU.GB.CcsukSplitHouse, "Enterprise.Customs.GB.Module", "Enterprise.Customs.GB.Module.CcsukSplitHouseModule", Constants.CountryCodes.UnitedKingdom));
			Add(new ModuleInfo(ModuleIDs.Customs.EU.GB.CcsukSplitBasic, "Enterprise.Customs.GB.Module", "Enterprise.Customs.GB.Module.CcsukSplitBasicModule", Constants.CountryCodes.UnitedKingdom));
			Add(new ModuleInfo(ModuleIDs.Customs.EU.GB.GbCcsukReports, "Enterprise.Customs.GB.Module", "Enterprise.Customs.GB.Module.CcsukReportsModule", Constants.CountryCodes.UnitedKingdom));
			Add(new ModuleInfo(ModuleIDs.Customs.JobDeclaration, "Enterprise.Customs.GB.Module", "Enterprise.Customs.GB.Module.JobDeclarationModule", Constants.CountryCodes.UnitedKingdom));
			Add(new ModuleInfo(ModuleIDs.SupplierPart, "Enterprise.Customs.EU.Module", "Enterprise.Customs.EU.Module.OrgSupplierPartModule", Constants.CountryCodes.UnitedKingdom));
			Add(new ModuleInfo(ModuleIDs.Customs.EU.GB.CDSDISQuery, "Enterprise.Customs.GB.Module", "Enterprise.Customs.GB.Module.CDSDISQueryModule", Constants.CountryCodes.UnitedKingdom));
			Add(new ModuleInfo(ModuleIDs.Customs.EU.GB.CDSCashPayments, "Enterprise.Customs.GB.Module", "Enterprise.Customs.GB.Module.CDSCashPaymentsModule", Constants.CountryCodes.UnitedKingdom));
			Add(new ModuleInfo(ModuleIDs.Customs.EntryHeader, "Enterprise.Customs.GB.Module", "Enterprise.Customs.GB.Module.EntryHeaderModule", Constants.CountryCodes.UnitedKingdom));
			Add(new ModuleInfo(ModuleIDs.Customs.EU.GB.PreBoardingNotification, "Enterprise.Customs.ASYCUDA.Module", "Enterprise.Customs.ASYCUDA.Module.AsycudaPreBoardingNotificationModule", Constants.CountryCodes.UnitedKingdom));
			Add(new ModuleInfo(ModuleIDs.Customs.EU.GB.EUH7Bill, "Enterprise.Customs.GB.H7.Module", "Enterprise.Customs.GB.H7.Module.GBH7BillModule", Constants.CountryCodes.UnitedKingdom));
			Add(new ModuleInfo(ModuleIDs.Customs.EU.GB.EUH7, "Enterprise.Customs.EU.H7.Module", "Enterprise.Customs.EU.H7.Module.EUH7Module", Constants.CountryCodes.UnitedKingdom));

			// DE
			Add(new ModuleInfo(ModuleIDs.Customs.JobDeclaration, "Enterprise.Customs.DE.Module", "Enterprise.Customs.DE.Module.JobDeclarationModule", Constants.CountryCodes.Germany));
			Add(new ModuleInfo(ModuleIDs.SailingDataVendorImporting, "Enterprise.Freight.Module", "Enterprise.Freight.SailingDataVendor.Module.SailingScheduleImportingModule", Constants.CountryCodes.Germany));
			Add(new ModuleInfo(ModuleIDs.Customs.TemporaryStorage, "Enterprise.Customs.DE.Module", "Enterprise.Customs.DE.Module.SumAModule", Constants.CountryCodes.Germany, ResString.GetMultilingualString("3C5BBC05-1D4D-4B50-98DD-184CEB7A6F1B", "Temporary Storage - SumA")));
			Add(new ModuleInfo(ModuleIDs.Customs.EU.DE.SumARegister, "Enterprise.Customs.DE.Module", "Enterprise.Customs.DE.Module.SumARegisterModule", Constants.CountryCodes.Germany));
			Add(new ModuleInfo(ModuleIDs.Customs.EU.DE.SumARegisterReadOnly, "Enterprise.Customs.DE.Module", "Enterprise.Customs.DE.Module.SumARegisterReadOnlyModule", Constants.CountryCodes.Germany));
			Add(new ModuleInfo(ModuleIDs.Customs.EU.DE.ImportFromSumARegister, "Enterprise.Customs.DE.Module", "Enterprise.Customs.DE.Module.ImportFromSumARegisterModule", Constants.CountryCodes.Germany));
			Add(new ModuleInfo(ModuleIDs.Customs.EU.DE.ExportStatusRequest, "Enterprise.Customs.DE.Module", "Enterprise.Customs.DE.Module.ExportStatusRequestModule", Constants.CountryCodes.Germany));
			Add(new ModuleInfo(ModuleIDs.Customs.EU.DE.MonthlyClosing, "Enterprise.Customs.DE.Module", "Enterprise.Customs.DE.Module.MonthlyClosingModule", Constants.CountryCodes.Germany));
			Add(new ModuleInfo(ModuleIDs.Customs.EU.DE.SimplifiedDeclaration, "Enterprise.Customs.DE.Module", "Enterprise.Customs.DE.Module.SimplifiedDeclarationModule", Constants.CountryCodes.Germany));
			Add(new ModuleInfo(ModuleIDs.Customs.EU.DE.TaxChangeAssessment, "Enterprise.Customs.DE.Module", "Enterprise.Customs.DE.Module.TaxChangeAssessmentModule", Constants.CountryCodes.Germany));

			// IT
			Add(new ModuleInfo(ModuleIDs.Customs.EntryHeader, "Enterprise.Customs.IT.Module", "Enterprise.Customs.IT.Module.EntryHeaderModule", Constants.CountryCodes.Italy));
			Add(new ModuleInfo(ModuleIDs.Customs.JobDeclaration, "Enterprise.Customs.IT.Module", "Enterprise.Customs.IT.Module.JobDeclarationModule", Constants.CountryCodes.Italy));
			Add(new ModuleInfo(ModuleIDs.Customs.EU.NctsMovementModule, "Enterprise.Customs.IT.NCTS.Module", "Enterprise.Customs.IT.NCTS.Module.NctsMovementModule", Constants.CountryCodes.Italy));
			Add(new ModuleInfo(ModuleIDs.Customs.EU.UCC6TemporaryStorage, "Enterprise.Customs.IT.TemporaryStorage.Module", "Enterprise.Customs.IT.TemporaryStorage.Module.UCC6TemporaryStorageModule", Constants.CountryCodes.Italy));
			Add(new ModuleInfo(ModuleIDs.Customs.EU.TempStorageRegister, "Enterprise.Customs.IT.TemporaryStorage.Module", "Enterprise.Customs.IT.TemporaryStorage.Module.TempStorageRegisterModule", Constants.CountryCodes.Italy));

			// All EU countries
			foreach (var jurisdiction in ObjectFactory.Get<Enterprise.Integration.Customs.Shared.IEuropeanUnionCustomsMembersProvider>().GetEuropeanUnionForCustomsMembersAndCustomsUnionAdditionalMembers().Select(countryCode => CountryCodes.GetCustomsCountryOfJurisdiction(countryCode)).Distinct())
			{
				foreach (var country in CountryCodes.GetCountriesAndTerritoriesBelongingToCustomsJurisdiction(jurisdiction))
				{
					if (!EuCountriesThatHaveTheirOwnJobDeclarationModule.Contains(jurisdiction))
					{
						Add(new ModuleInfo(ModuleIDs.Customs.JobDeclaration, "Enterprise.Customs.EU.Module", "Enterprise.Customs.EU.Module.JobDeclarationModule", country));
					}
					Add(new ModuleInfo(ModuleIDs.SingleTariffClassification, "Enterprise.Customs.EU.Module", "Enterprise.Customs.EU.Module.CusClassificationModule", country));
					Add(new ModuleInfo(ModuleIDs.Customs.Guarantees, "Enterprise.Customs.EU.Module", "Enterprise.Customs.EU.Module.GuaranteesModule", country));
					Add(new ModuleInfo(ModuleIDs.Customs.Permits, "Enterprise.Customs.EU.Module", "Enterprise.Customs.EU.Module.CusPermitModule", country));

					if (!EuCountriesThatHaveTheirOwnUCC6TemporaryStorageModule.Contains(jurisdiction))
					{
						Add(new ModuleInfo(ModuleIDs.Customs.EU.UCC6TemporaryStorage, "Enterprise.Customs.EU.TemporaryStorage.Module", "Enterprise.Customs.EU.TemporaryStorage.Module.UCC6TemporaryStorageModule", country));
					}

					if (!EuCountriesThatHaveTheirOwnEntryHeaderModule.Contains(jurisdiction))
					{
						Add(new ModuleInfo(ModuleIDs.Customs.EntryHeader, "Enterprise.Customs.EU.Module", "Enterprise.Customs.EU.Module.EntryHeaderModule", country));
					}
					if (!EuCountriesThatHaveTheirOwnTempStorageRegistryModule.Contains(jurisdiction))
					{
						Add(new ModuleInfo(ModuleIDs.Customs.EU.TempStorageRegister, "Enterprise.Customs.EU.TemporaryStorage.Module", "Enterprise.Customs.EU.TemporaryStorage.Module.TempStorageRegisterModule", country));
					}
					if (!ControllerList.EuCountriesThatHaveTheirOwnTemporaryStoragePremisesController.Contains(jurisdiction))
					{
						Add(new ModuleInfo(ModuleIDs.Customs.EU.TempStoragePremises, "Enterprise.Customs.EU.TemporaryStorage.Module", "Enterprise.Customs.EU.TemporaryStorage.Module.TempStoragePremisesModule", country));
					}
					if (!EuCountriesThatHaveTheirOwnH7BillModule.Contains(jurisdiction))
					{
						Add(new ModuleInfo(ModuleIDs.Customs.EU.EUH7Bill, "Enterprise.Customs.EU.H7.Module", "Enterprise.Customs.EU.H7.Module.EUH7BillModule", country));
					}
					if (!EuCountriesThatHaveTheirOwnH7ModuleInfo.Contains(jurisdiction))
					{
						Add(new ModuleInfo(ModuleIDs.Customs.EU.EUH7, "Enterprise.Customs.EU.H7.Module", "Enterprise.Customs.EU.H7.Module.EUH7Module", country));
					}
					if (!EuCountriesThatHaveTheirOwnTempStorageRegisterLinesModule.Contains(jurisdiction))
					{
						Add(new ModuleInfo(ModuleIDs.Customs.EU.TempStorageRegisterLines, "Enterprise.Customs.EU.TemporaryStorage.Module", "Enterprise.Customs.EU.TemporaryStorage.Module.TempStorageRegisterLinesModule", country));
					}

					Add(new ModuleInfo(ModuleIDs.Customs.CusCalculationRules, "Enterprise.Customs.EU.Module", "Enterprise.Customs.EU.Module.CusCalculationRulesModule", country));
				}
			}

			Add(new ModuleInfo(ModuleIDs.Customs.EU.EMCS, "Enterprise.Customs.EU.EMCS.Module", "Enterprise.Customs.EU.EMCS.Module.Module"));
			Add(new ModuleInfo(ModuleIDs.Customs.EU.IntrastatReports, "Enterprise.Customs.EU.Intrastat.Module", "Enterprise.Customs.EU.Intrastat.Module.IntrastatReportsModule"));
			Add(new ModuleInfo(ModuleIDs.Customs.EU.NctsMovementModule, "Enterprise.Customs.EU.NCTS.Module", "Enterprise.Customs.EU.NCTS.Module.NctsMovementModule"));
			Add(new ModuleInfo(ModuleIDs.Customs.EU.NctsReportsModule, "Enterprise.Customs.EU.NCTS.Module", "Enterprise.Customs.EU.NCTS.Module.NctsReportsModule"));
			Add(new ModuleInfo(ModuleIDs.Customs.EU.ExitControl, "Enterprise.Customs.EU.ExitControl.Module", "Enterprise.Customs.EU.ExitControl.Module.ExitControlModule"));
			Add(new ModuleInfo(ModuleIDs.Customs.EU.ExitControlReport, "Enterprise.Customs.EU.ExitControl.Module", "Enterprise.Customs.EU.ExitControl.Module.ExitControlReportModule"));
			Add(new ModuleInfo(ModuleIDs.Customs.EU.IntrastatTransactions, "Enterprise.Customs.EU.Intrastat.Module", "Enterprise.Customs.EU.Intrastat.Module.IntrastatTransactionsModule"));

			//Asycuda
			Add(new ModuleInfo(ModuleIDs.Customs.ASYCUDA.Manifest, "Enterprise.Customs.ASYCUDA.Module", "Enterprise.Customs.ASYCUDA.Module.AsycudaModule"));
			Add(new ModuleInfo(ModuleIDs.Customs.ASYCUDA.ManifestBill, "Enterprise.Customs.ASYCUDA.Module", "Enterprise.Customs.ASYCUDA.Module.ASYCUDAManifestBillModule"));

			Add(new ModuleInfo(ModuleIDs.Customs.ASYCUDA.PreBoardingNotification, "Enterprise.Customs.ASYCUDA.Module", "Enterprise.Customs.ASYCUDA.Module.AsycudaPreBoardingNotificationModule"));

			//SGAccess
			Add(new ModuleInfo(ModuleIDs.Customs.ASYCUDA.SGAccess.Manifest, "Enterprise.Customs.SG.Access.GUI", "Enterprise.Customs.SG.Access.GUI.ManifestModule", Constants.CountryCodes.Singapore));
			Add(new ModuleInfo(ModuleIDs.Customs.ASYCUDA.SGAccess.ManifestBill, "Enterprise.Customs.SG.Access.GUI", "Enterprise.Customs.SG.Access.GUI.ManifestBillModule", Constants.CountryCodes.Singapore));

			// JP
			Add(new ModuleInfo(ModuleIDs.Customs.JP.AFR, "Enterprise.Customs.JP.AFR.Module", "Enterprise.Customs.JP.AFR.Module.JPAFRModule"));
			Add(new ModuleInfo(ModuleIDs.Customs.JP.AFRBill, "Enterprise.Customs.JP.AFR.Module", "Enterprise.Customs.JP.AFR.Module.JPAFRBillModule"));
			Add(new ModuleInfo(ModuleIDs.Customs.EntryHeader, "Enterprise.Customs.JP.Module", "Enterprise.Customs.JP.Module.EntryHeaderModule", Constants.CountryCodes.Japan));

			// MY
			Add(new ModuleInfo(ModuleIDs.Customs.JobDeclaration, "Enterprise.Customs.MY.Module", "Enterprise.Customs.MY.Module.JobDeclarationModule", Constants.CountryCodes.Malaysia));
			Add(new ModuleInfo(ModuleIDs.SingleTariffClassification, "Enterprise.Customs.MY.Module", "Enterprise.Customs.MY.Module.CusClassificationModule", Constants.CountryCodes.Malaysia));
			Add(new ModuleInfo(ModuleIDs.SupplierPart, "Enterprise.Customs.MY.Module", "Enterprise.Customs.MY.Module.OrgSupplierPartModule", Constants.CountryCodes.Malaysia));

			// NZ
			Add(new ModuleInfo(ModuleIDs.Customs.JobDeclaration, "Enterprise.Customs.NZ.Module", "Enterprise.Customs.NZ.Module.Declaration.FormalEntry.JobDeclarationModule", Constants.CountryCodes.NewZealand));
			Add(new ModuleInfo(ModuleIDs.SingleTariffClassification, "Enterprise.Customs.NZ.Module", "Enterprise.Customs.NZ.Module.CusClassificationModule", Constants.CountryCodes.NewZealand));
			Add(new ModuleInfo(ModuleIDs.TariffBulkChange, "Enterprise.Customs.NZ.Module", "Enterprise.Customs.NZ.Module.NZTariffBulkChangeModule", Constants.CountryCodes.NewZealand));
			Add(new ModuleInfo(ModuleIDs.Customs.NZ.CUSCAR, "Enterprise.Customs.NZ.Module", "Enterprise.Customs.NZ.Module.Declaration.ECIWriteOff.NZCUSCARModule", Constants.CountryCodes.NewZealand));
			Add(new ModuleInfo(ModuleIDs.Customs.NZ.ECIWriteOffManifesting, "Enterprise.Customs.NZ.Module", "Enterprise.Customs.NZ.Module.Declaration.ECIWriteOffManifestingModule", Constants.CountryCodes.NewZealand));
			Add(new ModuleInfo(ModuleIDs.Customs.NZ.ExpressECI, "Enterprise.Customs.NZ.Module", "Enterprise.Customs.NZ.Module.Express.ExpressECIModule", Constants.CountryCodes.NewZealand));
			Add(new ModuleInfo(ModuleIDs.Customs.NZ.Concession, "Enterprise.Customs.NZ.Module", "Enterprise.Customs.NZ.Module.NZCConcessionModule", Constants.CountryCodes.NewZealand));
			Add(new ModuleInfo(ModuleIDs.Customs.NZ.OutwardReport, "Enterprise.Customs.NZ.Module", "Enterprise.Customs.NZ.Module.OutwardReportModule", Constants.CountryCodes.NewZealand));
			Add(new ModuleInfo(ModuleIDs.SupplierPart, "Enterprise.Customs.NZ.Module", "Enterprise.Customs.NZ.Module.OrgSupplierPartModule", Constants.CountryCodes.NewZealand));
			Add(new ModuleInfo(ModuleIDs.SailingDataVendorImporting, "Enterprise.Freight.Module", "Enterprise.Freight.SailingDataVendor.Module.SailingScheduleImportingModule", Constants.CountryCodes.NewZealand));
			Add(new ModuleInfo(ModuleIDs.SendTestCustomsMessage, "Enterprise.Customs.NZ.Module", "Enterprise.Customs.NZ.Module.NZSendTestCustomsMessageModule", Constants.CountryCodes.NewZealand));
			Add(new ModuleInfo(ModuleIDs.Customs.NZ.InwardCargoReport, "Enterprise.Customs.NZ.Module", "Enterprise.Customs.NZ.Module.ICRModule", Constants.CountryCodes.NewZealand));
			Add(new ModuleInfo(ModuleIDs.Customs.NZ.SeaCargoICR, "Enterprise.Customs.NZ.Module", "Enterprise.Customs.NZ.Module.Express.SeaCargoICRModule", Constants.CountryCodes.NewZealand));
			Add(new ModuleInfo(ModuleIDs.Customs.ConsolidatedDeclaration, "Enterprise.Customs.NZ.Module", "Enterprise.Customs.NZ.Module.ConsolidatedDeclarationModule", Constants.CountryCodes.NewZealand));

			//AE
			Add(new ModuleInfo(ModuleIDs.Customs.JobDeclaration, "Enterprise.Customs.AE.Module", "Enterprise.Customs.AE.Module.JobDeclarationModule", Constants.CountryCodes.UnitedArabEmirates));
			Add(new ModuleInfo(ModuleIDs.SingleTariffClassification, "Enterprise.Customs.AE.Module", "Enterprise.Customs.AE.Module.CusClassificationModule", Constants.CountryCodes.UnitedArabEmirates));
			Add(new ModuleInfo(ModuleIDs.SupplierPart, "Enterprise.Customs.AE.Module", "Enterprise.Customs.AE.Module.OrgSupplierPartModule", Constants.CountryCodes.UnitedArabEmirates));
			Add(new ModuleInfo(ModuleIDs.Customs.ASYCUDA.ManifestBill, "Enterprise.Customs.AE.Manifest.Module", "Enterprise.Customs.AE.Manifest.Module.AEManifestBillModule", Constants.CountryCodes.UnitedArabEmirates));

			// SG
			Add(new ModuleInfo(ModuleIDs.Customs.SG.JobDeclaration, "Enterprise.Customs.SG.V4.Module", "Enterprise.Customs.SG.V4.Module.JobDeclarationModule", Constants.CountryCodes.Singapore));
			Add(new ModuleInfo(ModuleIDs.Customs.SG.SG4Classification, "Enterprise.Customs.SG.V4.Module", "Enterprise.Customs.SG.V4.Module.ClassificationModule", Constants.CountryCodes.Singapore));
			Add(new ModuleInfo(ModuleIDs.SupplierPart, "Enterprise.Customs.SG.V4.Module", "Enterprise.Customs.SG.V4.Module.OrgSupplierPartModule", Constants.CountryCodes.Singapore));
			Add(new ModuleInfo(ModuleIDs.RefCurrency, "Enterprise.Customs.SG.V4.Module", "Enterprise.Customs.SG.V4.Module.RefCurrencyModule", Constants.CountryCodes.Singapore));
			Add(new ModuleInfo(ModuleIDs.CommercialInvoice, "Enterprise.Customs.SG.V4.Module", "Enterprise.Customs.SG.V4.Module.CommercialInvoiceModule", Constants.CountryCodes.Singapore));

			//Universal
			Add(new ModuleInfo(ModuleIDs.Customs.Universal.RefCusTariff, "Enterprise.Customs.Universal.Module", "Enterprise.Customs.Universal.Module.RefCusTariffModule"));
			Add(new ModuleInfo(ModuleIDs.Customs.Universal.ZZRefCarrier, "Enterprise.Customs.Universal.Module", "Enterprise.Customs.Universal.Module.ZZRefCarrierModule"));
			Add(new ModuleInfo(ModuleIDs.Customs.Universal.ZZRefCusCodeList, "Enterprise.Customs.Universal.Module", "Enterprise.Customs.Universal.Module.ZZRefCusCodeListModule"));
			Add(new ModuleInfo(ModuleIDs.Customs.Universal.RefDataGrouping, "Enterprise.Customs.Universal.Module", "Enterprise.Customs.Universal.Module.RefDataGroupingModule"));
			Add(new ModuleInfo(ModuleIDs.Customs.Universal.RefCusTradeGroup, "Enterprise.Customs.Universal.Module", "Enterprise.Customs.Universal.Module.RefCusTradeGroupModule"));
			Add(new ModuleInfo(ModuleIDs.Customs.Universal.ZZRefCusMap, "Enterprise.Customs.Universal.Module", "Enterprise.Customs.Universal.Module.ZZRefCusMapModule", new TableRegistrationInfo(ZZRefCusMapCombinedSchema.Constants.TableName)));
			Add(new ModuleInfo(ModuleIDs.Customs.Universal.ZZRefCusProcedure, "Enterprise.Customs.Universal.Module", "Enterprise.Customs.Universal.Module.ZZRefCusProcedureModule"));
			Add(new ModuleInfo(ModuleIDs.Customs.Universal.ZZRefCusRuling, "Enterprise.Customs.Universal.Module", "Enterprise.Customs.Universal.Module.ZZRefCusRulingModule"));
			Add(new ModuleInfo(ModuleIDs.Customs.Universal.RefHarbourRate, "Enterprise.Customs.Universal.Module", "Enterprise.Customs.Universal.Module.RefHarbourRateModule"));

			// US and PR
			Add(new ModuleInfo(ModuleIDs.Customs.US.Carrier, "Enterprise.Customs.US.Module", "Enterprise.Customs.US.Module.USCarrierCombinedModule"));
			Add(new ModuleInfo(ModuleIDs.Customs.US.ForeignPort, "Enterprise.Customs.Universal.Module", "Enterprise.Customs.Universal.Module.ZZRefCusCodeListModule"));
			Add(new ModuleInfo(ModuleIDs.Customs.US.RegionDistrictPort, "Enterprise.Customs.Universal.Module", "Enterprise.Customs.Universal.Module.ZZRefCusCodeListModule"));
			Add(new ModuleInfo(ModuleIDs.Customs.US.Tariff, "Enterprise.Customs.US.Module", "Enterprise.Customs.US.Module.USCTariffModule"));
			Add(new ModuleInfo(ModuleIDs.Customs.US.FIRMS, "Enterprise.Customs.US.Module", "Enterprise.Customs.US.Module.USCFIRMSModule"));
			Add(new ModuleInfo(ModuleIDs.Customs.US.USCForeignAndRegionPort, "Enterprise.Customs.US.Module", "Enterprise.Customs.US.Module.USCForeignAndRegionPortModule"));
			Add(new ModuleInfo(ModuleIDs.Customs.US.USCCarrierAndFIRMS, "Enterprise.Customs.US.Module", "Enterprise.Customs.US.Module.USCCarrierAndFIRMSModule"));
			Add(new ModuleInfo(ModuleIDs.Customs.US.eManifest, "Enterprise.Customs.US.eManifest.Module", "Enterprise.Customs.US.eManifest.Module.eManifestModule"));
			Add(new ModuleInfo(ModuleIDs.Customs.US.eManifestShipment, "Enterprise.Customs.US.eManifest.Module", "Enterprise.Customs.US.eManifest.Module.eManifestShipmentModule"));
			Add(new ModuleInfo(ModuleIDs.Customs.US.eManifestIntl, "Enterprise.Customs.US.eManifest.Module", "Enterprise.Customs.US.eManifest.Module.eManifestInternationalModule"));
			Add(new ModuleInfo(ModuleIDs.Customs.US.AMS, "Enterprise.Customs.US.AMS.Module", "Enterprise.Customs.US.AMS.Module.USAMSModule"));
			Add(new ModuleInfo(ModuleIDs.Customs.US.AMSBill, "Enterprise.Customs.US.AMS.Module", "Enterprise.Customs.US.AMS.Module.USAMSBillModule"));
			Add(new ModuleInfo(ModuleIDs.Customs.US.InBond, "Enterprise.Customs.US.InBond.Module", "Enterprise.Customs.US.InBond.Module.CusInBondHeaderModule"));
			Add(new ModuleInfo(ModuleIDs.Customs.US.InBondMoveHeader, "Enterprise.Customs.US.InBond.Module", "Enterprise.Customs.US.InBond.Module.USInBondMoveHeaderModule"));
			Add(new ModuleInfo(ModuleIDs.Customs.US.ThreeLetterRefAirline, "Enterprise.Customs.US.InBond.Module", "Enterprise.Customs.US.InBond.Module.ThreeLetterRefAirlineModule"));
			Add(new ModuleInfo(ModuleIDs.Customs.US.USLowValueEntries, "Enterprise.Customs.US.LVS.Module", "Enterprise.Customs.US.LVS.Module.CusUSLVClearanceModule"));
			Add(new ModuleInfo(ModuleIDs.Customs.US.USLowValueEntriesBill, "Enterprise.Customs.US.LVS.Module", "Enterprise.Customs.US.LVS.Module.CusUSLVConsignmentModule"));
			Add(new ModuleInfo(ModuleIDs.Customs.US.USLowValueEntriesDeclaration, "Enterprise.Customs.US.LVS.Module", "Enterprise.Customs.US.LVS.Module.CusUSLVDeclarationModule"));

			foreach (var countryCode in new string[] { Constants.CountryCodes.UnitedStates, Constants.CountryCodes.PuertoRico })
			{
				Add(new ModuleInfo(ModuleIDs.Customs.US.AffirmationOfCompliance, "Enterprise.Customs.US.Module", "Enterprise.Customs.US.Module.USCAffirmationOfComplianceModule", countryCode));
				Add(new ModuleInfo(ModuleIDs.Customs.US.USExportClassification, "Enterprise.Customs.US.Module", "Enterprise.Customs.US.Module.ExportClassificationModule", countryCode));
				Add(new ModuleInfo(ModuleIDs.Customs.US.USImportClassification, "Enterprise.Customs.US.Module", "Enterprise.Customs.US.Module.ImportClassificationModule", countryCode));
				Add(new ModuleInfo(ModuleIDs.Customs.JobDeclaration, "Enterprise.Customs.US.Module", "Enterprise.Customs.US.Module.JobDeclarationModule", countryCode));
				Add(new ModuleInfo(ModuleIDs.CommercialInvoice, "Enterprise.Customs.US.Module", "Enterprise.Customs.US.Module.CommercialInvoiceModule", countryCode));
				Add(new ModuleInfo(ModuleIDs.Customs.US.Country, "Enterprise.Customs.US.Module", "Enterprise.Customs.US.Module.USCCountryModule", countryCode));
				Add(new ModuleInfo(ModuleIDs.Customs.US.TeamSpecialist, "Enterprise.Customs.US.Module", "Enterprise.Customs.US.Module.USCTeamSpecialistModule", countryCode));
				Add(new ModuleInfo(ModuleIDs.SupplierPart, "Enterprise.Customs.US.Module", "Enterprise.Customs.US.Module.OrgSupplierPartModule", countryCode));
				Add(new ModuleInfo(ModuleIDs.Customs.US.USTariffBulkChange, "Enterprise.Customs.US.Module", "Enterprise.Customs.US.Module.USTariffBulkChangeModule", countryCode));
				Add(new ModuleInfo(ModuleIDs.Customs.US.USCustomsStatement, "Enterprise.Customs.US.Module", "Enterprise.Customs.US.Module.StatementModule", countryCode));
				Add(new ModuleInfo(ModuleIDs.Customs.US.Quota, "Enterprise.Customs.US.Module", "Enterprise.Customs.US.Module.USCQuotaModule", countryCode));
				Add(new ModuleInfo(ModuleIDs.Customs.US.Visa, "Enterprise.Customs.US.Module", "Enterprise.Customs.US.Module.USCVisaModule", countryCode));
				Add(new ModuleInfo(ModuleIDs.Customs.US.TariffRequiringVisa, "Enterprise.Customs.US.Module", "Enterprise.Customs.US.Module.USCVisaTariffModule", countryCode));
				Add(new ModuleInfo(ModuleIDs.Customs.US.AMSBrokerDownloadMessage, "Enterprise.Customs.US.Module", "Enterprise.Customs.US.Module.AMSBrokerDownloadModule", countryCode));
				Add(new ModuleInfo(ModuleIDs.Customs.US.BorderLineReleaseMessage, "Enterprise.Customs.US.Module", "Enterprise.Customs.US.Module.BorderLineReleaseMessageModule", countryCode));
				Add(new ModuleInfo(ModuleIDs.Customs.US.QueryMessage, "Enterprise.Customs.US.Module", "Enterprise.Customs.US.Module.QueryMessageModule", countryCode));
				Add(new ModuleInfo(ModuleIDs.Customs.US.InBondNumber, "Enterprise.Customs.US.Module", "Enterprise.Customs.US.Module.InBondNumberModule", countryCode));
				Add(new ModuleInfo(ModuleIDs.Customs.US.USCTariffRule, "Enterprise.Customs.US.Module", "Enterprise.Customs.US.Module.USCTariffRuleModule", countryCode));
				Add(new ModuleInfo(ModuleIDs.Customs.US.USCRule, "Enterprise.Customs.US.Module", "Enterprise.Customs.US.Module.USCRuleModule", countryCode));
				Add(new ModuleInfo(ModuleIDs.Customs.EntryHeader, "Enterprise.Customs.US.Module", "Enterprise.Customs.US.Module.EntryHeaderModule", countryCode));
				Add(new ModuleInfo(ModuleIDs.Customs.US.USCACCase, "Enterprise.Customs.US.Module", "Enterprise.Customs.US.Module.USCACCaseModule", countryCode));
				Add(new ModuleInfo(ModuleIDs.Customs.US.EntryLine, "Enterprise.Customs.US.Module", "Enterprise.Customs.US.Module.EntryLineModule", countryCode));
				Add(new ModuleInfo(ModuleIDs.Customs.US.CourtesyNoticesOfLiquidation, "Enterprise.Customs.US.Module", "Enterprise.Customs.US.Module.CourtesyNoticesOfLiquidationModule", countryCode));
				Add(new ModuleInfo(ModuleIDs.Customs.US.USCDataVersion, "Enterprise.Customs.US.Module", "Enterprise.Customs.US.Module.USCDataVersionModule", countryCode));
				Add(new ModuleInfo(ModuleIDs.Customs.US.Reconciliation, "Enterprise.Customs.US.Module", "Enterprise.Customs.US.Module.ReconModule", countryCode));
				Add(new ModuleInfo(ModuleIDs.Customs.US.Drawback, "Enterprise.Customs.US.Module", "Enterprise.Customs.US.Module.DrawbackModule", countryCode));
				Add(new ModuleInfo(ModuleIDs.Customs.US.Protest, "Enterprise.Customs.US.Module", "Enterprise.Customs.US.Module.ProtestModule", countryCode));
			}

			// TW
			Add(new ModuleInfo(ModuleIDs.Customs.JobDeclaration, "Enterprise.Customs.TW.Module", "Enterprise.Customs.TW.Module.JobDeclarationModule", Constants.CountryCodes.Taiwan));
			Add(new ModuleInfo(ModuleIDs.SingleTariffClassification, "Enterprise.Customs.TW.Module", "Enterprise.Customs.TW.Module.CusClassificationModule", Constants.CountryCodes.Taiwan));
			Add(new ModuleInfo(ModuleIDs.SupplierPart, "Enterprise.Customs.TW.Module", "Enterprise.Customs.TW.Module.OrgSupplierPartModule", Constants.CountryCodes.Taiwan));
			Add(new ModuleInfo(ModuleIDs.Customs.TW.Transhipment, "Enterprise.Customs.TW.Module", "Enterprise.Customs.TW.Transhipment.Module.CusInBondHeaderModule", Constants.CountryCodes.Taiwan));
			Add(new ModuleInfo(ModuleIDs.Customs.TW.SpecialCode, "Enterprise.Customs.TW.Module", "Enterprise.Customs.TW.Module.TWSpecialCodeListModule", Constants.CountryCodes.Taiwan));
			Add(new ModuleInfo(ModuleIDs.Customs.TW.BriefCustomsDeclarations, "Enterprise.Customs.TW.BriefCustomsDeclaration.Module", "Enterprise.Customs.TW.BriefCustomsDeclaration.Module.ManifestModule", Constants.CountryCodes.Taiwan));

			// AT
			Add(new ModuleInfo(ModuleIDs.SailingDataVendorImporting, "Enterprise.Freight.Module", "Enterprise.Freight.SailingDataVendor.Module.SailingScheduleImportingModule", Constants.CountryCodes.Austria));

			// BR
			Add(new ModuleInfo(ModuleIDs.CommercialInvoice, "Enterprise.Customs.BR.Module", "Enterprise.Customs.BR.Module.CommercialInvoiceModule", Constants.CountryCodes.Brazil));
			Add(new ModuleInfo(ModuleIDs.Customs.JobDeclaration, "Enterprise.Customs.BR.Module", "Enterprise.Customs.BR.Module.JobDeclarationModule", Constants.CountryCodes.Brazil));
			Add(new ModuleInfo(ModuleIDs.SingleTariffClassification, "Enterprise.Customs.BR.Module", "Enterprise.Customs.BR.Module.CusClassificationModule", Constants.CountryCodes.Brazil));
			Add(new ModuleInfo(ModuleIDs.SupplierPart, "Enterprise.Customs.BR.Module", "Enterprise.Customs.BR.Module.OrgSupplierPartModule", Constants.CountryCodes.Brazil));
			Add(new ModuleInfo(ModuleIDs.Customs.BR.LPCO, "Enterprise.Customs.BR.Module", "Enterprise.Customs.BR.Module.LPCOModule", Constants.CountryCodes.Brazil));
			Add(new ModuleInfo(ModuleIDs.Customs.BR.LPCODeclaration, "Enterprise.Customs.BR.Module", "Enterprise.Customs.BR.Module.LPCODeclarationModule", Constants.CountryCodes.Brazil));
			Add(new ModuleInfo(ModuleIDs.Customs.BR.LPCOEntryHeader, "Enterprise.Customs.BR.Module", "Enterprise.Customs.BR.Module.LPCOEntryHeaderModule", Constants.CountryCodes.Brazil));
			Add(new ModuleInfo(ModuleIDs.Customs.BR.License, "Enterprise.Customs.BR.Module", "Enterprise.Customs.BR.Module.LicenseModule", Constants.CountryCodes.Brazil));
			Add(new ModuleInfo(ModuleIDs.Customs.BR.LicenseEntryHeader, "Enterprise.Customs.BR.Module", "Enterprise.Customs.BR.Module.LicenseEntryHeaderModule", Constants.CountryCodes.Brazil));
			Add(new ModuleInfo(ModuleIDs.Customs.GoodsCatalog, "Enterprise.Customs.BR.Module", "Enterprise.Customs.BR.Module.GoodsCatalogModule", Constants.CountryCodes.Brazil));
			Add(new ModuleInfo(ModuleIDs.Customs.BR.ForeignOperator, "Enterprise.Customs.BR.Module", "Enterprise.Customs.BR.Module.ForeignOperatorModule", Constants.CountryCodes.Brazil));

			// JP
			Add(new ModuleInfo(ModuleIDs.Customs.JobDeclaration, "Enterprise.Customs.JP.Module", "Enterprise.Customs.JP.Module.JobDeclarationModule", Constants.CountryCodes.Japan));
			Add(new ModuleInfo(ModuleIDs.SingleTariffClassification, "Enterprise.Customs.JP.Module", "Enterprise.Customs.JP.Module.CusClassificationModule", Constants.CountryCodes.Japan));
			Add(new ModuleInfo(ModuleIDs.SupplierPart, "Enterprise.Customs.JP.Module", "Enterprise.Customs.JP.Module.OrgSupplierPartModule", Constants.CountryCodes.Japan));

			// FR
			foreach (var countryCode in Constants.CountryCodes.FranceAndOverseasDepartmentsUnderItsCustomsJurisdiction)
			{
				Add(new ModuleInfo(ModuleIDs.Customs.JobDeclaration, "Enterprise.Customs.FR.Module", "Enterprise.Customs.FR.Module.JobDeclarationModule", countryCode));
				Add(new ModuleInfo(ModuleIDs.SupplierPart, "Enterprise.Customs.EU.Module", "Enterprise.Customs.EU.Module.OrgSupplierPartModule", countryCode));
				Add(new ModuleInfo(ModuleIDs.Customs.TemporaryStorage, "Enterprise.Customs.FR.Module", "Enterprise.Customs.FR.Module.TemporaryStorageModule", countryCode));
				Add(new ModuleInfo(ModuleIDs.Customs.EntryHeader, "Enterprise.Customs.FR.Module", "Enterprise.Customs.FR.Module.EntryHeaderModule", countryCode));
				Add(new ModuleInfo(ModuleIDs.Customs.EU.NctsMovementModule, "Enterprise.Customs.FR.Module", "Enterprise.Customs.FR.Module.NctsMovementModule", countryCode));
				Add(new ModuleInfo(ModuleIDs.Customs.EU.FR.CustomsStatement, "Enterprise.Customs.FR.Module", "Enterprise.Customs.FR.Module.StatementModule", countryCode));
				Add(new ModuleInfo(ModuleIDs.Customs.EU.TempStorageRegister, "Enterprise.Customs.FR.Module", "Enterprise.Customs.FR.Module.TempStorageRegisterModule", countryCode));
				Add(new ModuleInfo(ModuleIDs.Customs.EU.UCC6TemporaryStorage, "Enterprise.Customs.FR.Module", "Enterprise.Customs.FR.Module.UCC6TemporaryStorageModule", countryCode));
			}

			// KR
			Add(new ModuleInfo(ModuleIDs.Customs.JobDeclaration, "Enterprise.Customs.KR.Module", "Enterprise.Customs.KR.Module.JobDeclarationModule", Constants.CountryCodes.KoreaSouth));
			Add(new ModuleInfo(ModuleIDs.SupplierPart, "Enterprise.Customs.KR.Module", "Enterprise.Customs.KR.Module.OrgSupplierPartModule", Constants.CountryCodes.KoreaSouth));
			Add(new ModuleInfo(ModuleIDs.Customs.KR.CustomsStatement, "Enterprise.Customs.KR.Module", "Enterprise.Customs.KR.Module.CusStatementModule", Constants.CountryCodes.KoreaSouth));
			Add(new ModuleInfo(ModuleIDs.Customs.KR.MiscRequestMessages, "Enterprise.Customs.KR.Module", "Enterprise.Customs.KR.Module.MiscRequestMessagesModule", Constants.CountryCodes.KoreaSouth));
			Add(new ModuleInfo(ModuleIDs.Customs.KR.ExportEntryDetails, "Enterprise.Customs.KR.Module", "Enterprise.Customs.KR.Module.EntryDetailsFor5ACModule", Constants.CountryCodes.KoreaSouth));
			Add(new ModuleInfo(ModuleIDs.Customs.KR.ImportEntryDetails, "Enterprise.Customs.KR.Module", "Enterprise.Customs.KR.Module.EntryDetailsFor5GWModule", Constants.CountryCodes.KoreaSouth));
			Add(new ModuleInfo(ModuleIDs.Customs.KR.EntryCustomsBillsFor5UL, "Enterprise.Customs.KR.Module", "Enterprise.Customs.KR.Module.EntryCustomsBillsFor5ULModule", Constants.CountryCodes.KoreaSouth));
			Add(new ModuleInfo(ModuleIDs.Customs.KR.EntryDetailsFor5SG, "Enterprise.Customs.KR.Module", "Enterprise.Customs.KR.Module.EntryDetailsFor5SGModule", Constants.CountryCodes.KoreaSouth));
			Add(new ModuleInfo(ModuleIDs.Customs.KR.EntryLineDetailsFor5UL, "Enterprise.Customs.KR.Module", "Enterprise.Customs.KR.Module.EntryLineDetailsFor5ULModule", Constants.CountryCodes.KoreaSouth));
			Add(new ModuleInfo(ModuleIDs.Customs.KR.DocumentListMessages, "Enterprise.Customs.KR.Module", "Enterprise.Customs.KR.Module.DocumentListMessagesModule", Constants.CountryCodes.KoreaSouth));
			Add(new ModuleInfo(ModuleIDs.Customs.KR.CusReconDeclaration, "Enterprise.Customs.KR.Module", "Enterprise.Customs.KR.Module.CusReconDeclarationModule", Constants.CountryCodes.KoreaSouth));

			// AsycudaCustoms
			foreach (var country in ObjectFactory.Get<Enterprise.Integration.Customs.Shared.IAsycudaCustomsCountryProvider>().GetAsycudaCustomsCountryCodes())
			{
				Add(new ModuleInfo(ModuleIDs.Customs.JobDeclaration, "Enterprise.Customs.AsycudaCustoms.Module", "Enterprise.Customs.AsycudaCustoms.Module.JobDeclarationModule", country));
				Add(new ModuleInfo(ModuleIDs.SingleTariffClassification, "Enterprise.Customs.AsycudaCustoms.Module", "Enterprise.Customs.AsycudaCustoms.Module.CusClassificationModule", country));
				Add(new ModuleInfo(ModuleIDs.SupplierPart, "Enterprise.Customs.AsycudaCustoms.Module", "Enterprise.Customs.AsycudaCustoms.Module.OrgSupplierPartModule", country));
				Add(new ModuleInfo(ModuleIDs.Customs.EntryHeader, "Enterprise.Customs.AsycudaCustoms.Module", "Enterprise.Customs.AsycudaCustoms.Module.EntryHeaderModule", country));
			}

			// ES
			Add(new ModuleInfo(ModuleIDs.Customs.JobDeclaration, "Enterprise.Customs.ES.Module", "Enterprise.Customs.ES.Module.JobDeclarationModule", Constants.CountryCodes.Spain));
			Add(new ModuleInfo(ModuleIDs.SupplierPart, "Enterprise.Customs.EU.Module", "Enterprise.Customs.EU.Module.OrgSupplierPartModule", Constants.CountryCodes.Spain));
			Add(new ModuleInfo(ModuleIDs.Customs.TemporaryStorage, "Enterprise.Customs.ES.TemporaryStorage.Module", "Enterprise.Customs.ES.TemporaryStorage.Module.TemporaryStorageModule", Constants.CountryCodes.Spain));
			Add(new ModuleInfo(ModuleIDs.Customs.EU.NctsMovementModule, "Enterprise.Customs.ES.NCTS.Module", "Enterprise.Customs.ES.NCTS.Module.NctsMovementModule", Constants.CountryCodes.Spain));
			Add(new ModuleInfo(ModuleIDs.Customs.EntryHeader, "Enterprise.Customs.ES.Module", "Enterprise.Customs.ES.Module.EntryHeaderModule", Constants.CountryCodes.Spain));
			Add(new ModuleInfo(ModuleIDs.Customs.EU.ExitControl, "Enterprise.Customs.ES.ExitControl.Module", "Enterprise.Customs.ES.ExitControl.Module.ExitControlModule", Constants.CountryCodes.Spain));
			Add(new ModuleInfo(ModuleIDs.Customs.EU.ExitControlReport, "Enterprise.Customs.ES.ExitControl.Module", "Enterprise.Customs.ES.ExitControl.Module.ExitControlReportModule", Constants.CountryCodes.Spain));
			Add(new ModuleInfo(ModuleIDs.Customs.EU.ES.TemporaryStorageRegister, "Enterprise.Customs.ES.TemporaryStorage.Module", "Enterprise.Customs.ES.TemporaryStorage.Module.TemporaryStorageRegisterModule", Constants.CountryCodes.Spain));
			Add(new ModuleInfo(ModuleIDs.Customs.EU.TempStoragePremises, "Enterprise.Customs.ES.TemporaryStorage.Module", "Enterprise.Customs.ES.TemporaryStorage.Module.TempStoragePremisesModule", Constants.CountryCodes.Spain));
			Add(new ModuleInfo(ModuleIDs.Customs.EU.TempStorageRegisterLines, "Enterprise.Customs.ES.TemporaryStorage.Module", "Enterprise.Customs.ES.TemporaryStorage.Module.TemporaryStorageRegisterLinesModule", Constants.CountryCodes.Spain));
			Add(new ModuleInfo(ModuleIDs.Customs.EU.ES.G3Declaration, "Enterprise.Customs.ES.Manifest.H7.Module", "Enterprise.Customs.ES.Manifest.H7.Module.G3DeclarationModule", Constants.CountryCodes.Spain));
			Add(new ModuleInfo(ModuleIDs.Customs.EU.ES.EUH7, "Enterprise.Customs.ES.Manifest.H7.Module", "Enterprise.Customs.ES.Manifest.H7.Module.ESH7Module", Constants.CountryCodes.Spain));
			Add(new ModuleInfo(ModuleIDs.Customs.EU.ES.EUH7Bill, "Enterprise.Customs.ES.Manifest.H7.Module", "Enterprise.Customs.ES.Manifest.H7.Module.ESH7BillModule", Constants.CountryCodes.Spain));

			// IE
			Add(new ModuleInfo(ModuleIDs.Customs.EntryHeader, "Enterprise.Customs.IE.Module", "Enterprise.Customs.IE.Module.EntryHeaderModule", Constants.CountryCodes.Ireland));
			Add(new ModuleInfo(ModuleIDs.Customs.JobDeclaration, "Enterprise.Customs.IE.Module", "Enterprise.Customs.IE.Module.JobDeclarationModule", Constants.CountryCodes.Ireland));
			Add(new ModuleInfo(ModuleIDs.SupplierPart, "Enterprise.Customs.EU.Module", "Enterprise.Customs.EU.Module.OrgSupplierPartModule", Constants.CountryCodes.Ireland));
			Add(new ModuleInfo(ModuleIDs.Customs.EU.ExitControlReport, "Enterprise.Customs.IE.ExitControl.Module", "Enterprise.Customs.IE.ExitControl.Module.ExitControlReportModule", Constants.CountryCodes.Ireland));
			Add(new ModuleInfo(ModuleIDs.Customs.EU.UCC6TemporaryStorage, "Enterprise.Customs.IE.Module", "Enterprise.Customs.IE.Module.UCC6TemporaryStorageModule", Constants.CountryCodes.Ireland));
			Add(new ModuleInfo(ModuleIDs.Customs.EU.IE.PreBoardingNotification, "Enterprise.Customs.IE.PBN.Module", "Enterprise.Customs.IE.PBN.Module.PBNModule", Constants.CountryCodes.Ireland));

			// PL
			Add(new ModuleInfo(ModuleIDs.Customs.EU.PL.AuthorisationRule, "Enterprise.Customs.PL.NCTS.Module", "Enterprise.Customs.PL.NCTS.Module.AuthorisationRuleModule", Constants.CountryCodes.Poland));
			Add(new ModuleInfo(ModuleIDs.Customs.JobDeclaration, "Enterprise.Customs.PL.Module", "Enterprise.Customs.PL.Module.JobDeclarationModule", Constants.CountryCodes.Poland));
			Add(new ModuleInfo(ModuleIDs.SupplierPart, "Enterprise.Customs.EU.Module", "Enterprise.Customs.EU.Module.OrgSupplierPartModule", Constants.CountryCodes.Poland));
			Add(new ModuleInfo(ModuleIDs.Customs.TemporaryStorage, "Enterprise.Customs.PL.Module", "Enterprise.Customs.PL.Module.TemporaryStorageModule", Constants.CountryCodes.Poland));

			// BE
			Add(new ModuleInfo(ModuleIDs.Customs.JobDeclaration, "Enterprise.Customs.BE.Module", "Enterprise.Customs.BE.Module.JobDeclarationModule", Constants.CountryCodes.Belgium));
			Add(new ModuleInfo(ModuleIDs.SupplierPart, "Enterprise.Customs.EU.Module", "Enterprise.Customs.EU.Module.OrgSupplierPartModule", Constants.CountryCodes.Belgium));

			// TR
			Add(new ModuleInfo(ModuleIDs.Customs.JobDeclaration, "Enterprise.Customs.TR.Module", "Enterprise.Customs.TR.Module.JobDeclarationModule", Constants.CountryCodes.Turkey));
			Add(new ModuleInfo(ModuleIDs.SupplierPart, "Enterprise.Customs.EU.Module", "Enterprise.Customs.EU.Module.OrgSupplierPartModule", Constants.CountryCodes.Turkey));
			Add(new ModuleInfo(ModuleIDs.Customs.EU.NctsMovementModule, "Enterprise.Customs.TR.NCTS.Module", "Enterprise.Customs.TR.NCTS.Module.NctsMovementModule", Constants.CountryCodes.Turkey));

			// NL
			Add(new ModuleInfo(ModuleIDs.Customs.JobDeclaration, "Enterprise.Customs.NL.Module", "Enterprise.Customs.NL.Module.JobDeclarationModule", Constants.CountryCodes.Netherlands));
			Add(new ModuleInfo(ModuleIDs.SupplierPart, "Enterprise.Customs.EU.Module", "Enterprise.Customs.EU.Module.OrgSupplierPartModule", Constants.CountryCodes.Netherlands));
			Add(new ModuleInfo(ModuleIDs.Customs.EU.NctsMovementModule, "Enterprise.Customs.NL.NCTS.Module", "Enterprise.Customs.NL.NCTS.Module.NctsMovementModule", Constants.CountryCodes.Netherlands));
			Add(new ModuleInfo(ModuleIDs.Customs.EntryHeader, "Enterprise.Customs.NL.Module", "Enterprise.Customs.NL.Module.EntryHeaderModule", Constants.CountryCodes.Netherlands));

			// SE
			Add(new ModuleInfo(ModuleIDs.Customs.JobDeclaration, "Enterprise.Customs.SE.Module", "Enterprise.Customs.SE.Module.JobDeclarationModule", Constants.CountryCodes.Sweden));
			Add(new ModuleInfo(ModuleIDs.SupplierPart, "Enterprise.Customs.EU.Module", "Enterprise.Customs.EU.Module.OrgSupplierPartModule", Constants.CountryCodes.Sweden));

			// CH
			Add(new ModuleInfo(ModuleIDs.SingleTariffClassification, "Enterprise.Customs.CH.Module", "Enterprise.Customs.CH.Module.CusClassificationModule", Constants.CountryCodes.Switzerland));
			Add(new ModuleInfo(ModuleIDs.Customs.JobDeclaration, "Enterprise.Customs.CH.Module", "Enterprise.Customs.CH.Module.JobDeclarationModule", Constants.CountryCodes.Switzerland));
			Add(new ModuleInfo(ModuleIDs.SupplierPart, "Enterprise.Customs.CH.Module", "Enterprise.Customs.CH.Module.OrgSupplierPartModule", Constants.CountryCodes.Switzerland));
			Add(new ModuleInfo(ModuleIDs.Customs.Permits, "Enterprise.Customs.CH.Module", "Enterprise.Customs.CH.Module.CusPermitModule", Constants.CountryCodes.Switzerland));
			Add(new ModuleInfo(ModuleIDs.Customs.EU.NctsMovementModule, "Enterprise.Customs.CH.NCTS.Module", "Enterprise.Customs.CH.NCTS.Module.NctsMovementModule", Constants.CountryCodes.Switzerland));
			Add(new ModuleInfo(ModuleIDs.Customs.EntryHeader, "Enterprise.Customs.CH.Module", "Enterprise.Customs.CH.Module.EntryHeaderModule", Constants.CountryCodes.Switzerland));
			Add(new ModuleInfo(ModuleIDs.Customs.CH.CustomsSummary, "Enterprise.Customs.CH.Module", "Enterprise.Customs.CH.Module.CustomsSummaryModule", Constants.CountryCodes.Switzerland));
			Add(new ModuleInfo(ModuleIDs.Customs.CH.DeclarationActivation, "Enterprise.Customs.CH.DeclarationActivation.Module", "Enterprise.Customs.CH.DeclarationActivation.Module.DeclarationActivationModule", Constants.CountryCodes.Switzerland));

			// NO
			Add(new ModuleInfo(ModuleIDs.Customs.JobDeclaration, "Enterprise.Customs.NO.Module", "Enterprise.Customs.NO.Module.JobDeclarationModule", Constants.CountryCodes.Norway));
			Add(new ModuleInfo(ModuleIDs.SingleTariffClassification, "Enterprise.Customs.NO.Module", "Enterprise.Customs.NO.Module.CusClassificationModule", Constants.CountryCodes.Norway));
			Add(new ModuleInfo(ModuleIDs.SupplierPart, "Enterprise.Customs.NO.Module", "Enterprise.Customs.NO.Module.OrgSupplierPartModule", Constants.CountryCodes.Norway));
			Add(new ModuleInfo(ModuleIDs.Customs.NO.TemporaryStorageRegister, "Enterprise.Customs.NO.Module", "Enterprise.Customs.NO.Module.SumARegisterModule", Constants.CountryCodes.Norway));
			Add(new ModuleInfo(ModuleIDs.Customs.NO.TemporaryStorageRegisterReadOnly, "Enterprise.Customs.NO.Module", "Enterprise.Customs.NO.Module.SumARegisterReadOnlyModule", Constants.CountryCodes.Norway));

			//IL
			Add(new ModuleInfo(ModuleIDs.Customs.JobDeclaration, "Enterprise.Customs.IL.Module", "Enterprise.Customs.IL.Module.JobDeclarationModule", Constants.CountryCodes.Israel));
			Add(new ModuleInfo(ModuleIDs.SingleTariffClassification, "Enterprise.Customs.IL.Module", "Enterprise.Customs.IL.Module.CusClassificationModule", Constants.CountryCodes.Israel));
			Add(new ModuleInfo(ModuleIDs.SupplierPart, "Enterprise.Customs.IL.Module", "Enterprise.Customs.IL.Module.OrgSupplierPartModule", Constants.CountryCodes.Israel));

			// IN
			Add(new ModuleInfo(ModuleIDs.Customs.JobDeclaration, "Enterprise.Customs.IN.Module", "Enterprise.Customs.IN.Module.JobDeclarationModule", Constants.CountryCodes.India));
			Add(new ModuleInfo(ModuleIDs.SingleTariffClassification, "Enterprise.Customs.IN.Module", "Enterprise.Customs.IN.Module.CusClassificationModule", Constants.CountryCodes.India));
			Add(new ModuleInfo(ModuleIDs.SupplierPart, "Enterprise.Customs.IN.Module", "Enterprise.Customs.IN.Module.OrgSupplierPartModule", Constants.CountryCodes.India));

			// DK
			Add(new ModuleInfo(ModuleIDs.Customs.JobDeclaration, "Enterprise.Customs.DK.Module", "Enterprise.Customs.DK.Module.JobDeclarationModule", Constants.CountryCodes.Denmark));
			Add(new ModuleInfo(ModuleIDs.SupplierPart, "Enterprise.Customs.EU.Module", "Enterprise.Customs.EU.Module.OrgSupplierPartModule", Constants.CountryCodes.Denmark));

			// MX
			Add(new ModuleInfo(ModuleIDs.Customs.JobDeclaration, "Enterprise.Customs.MX.Module", "Enterprise.Customs.MX.Module.JobDeclarationModule", Constants.CountryCodes.Mexico));
			Add(new ModuleInfo(ModuleIDs.SingleTariffClassification, "Enterprise.Customs.MX.Module", "Enterprise.Customs.MX.Module.CusClassificationModule", Constants.CountryCodes.Mexico));
			Add(new ModuleInfo(ModuleIDs.SupplierPart, "Enterprise.Customs.MX.Module", "Enterprise.Customs.MX.Module.OrgSupplierPartModule", Constants.CountryCodes.Mexico));

			//FI
			Add(new ModuleInfo(ModuleIDs.Customs.JobDeclaration, "Enterprise.Customs.FI.Module", "Enterprise.Customs.FI.Module.JobDeclarationModule", Constants.CountryCodes.Finland));
			Add(new ModuleInfo(ModuleIDs.SupplierPart, "Enterprise.Customs.EU.Module", "Enterprise.Customs.EU.Module.OrgSupplierPartModule", Constants.CountryCodes.Finland));

#if DEBUG // _CustomsTemplate_
			Add(new ModuleInfo(ModuleIDs.Customs.JobDeclaration, "Enterprise.Customs._CustomsTemplate_.Module", "Enterprise.Customs._CustomsTemplate_.Module.JobDeclarationModule", Constants.CountryCodes._TemplateCountryName_));
			Add(new ModuleInfo(ModuleIDs.SingleTariffClassification, "Enterprise.Customs._CustomsTemplate_.Module", "Enterprise.Customs._CustomsTemplate_.Module.CusClassificationModule", Constants.CountryCodes._TemplateCountryName_));
			Add(new ModuleInfo(ModuleIDs.SupplierPart, "Enterprise.Customs._CustomsTemplate_.Module", "Enterprise.Customs._CustomsTemplate_.Module.OrgSupplierPartModule", Constants.CountryCodes._TemplateCountryName_));

			// _EUCustomsTemplate_
			Add(new ModuleInfo(ModuleIDs.Customs.JobDeclaration, "Enterprise.Customs._EUCustomsTemplate_.Module", "Enterprise.Customs._EUCustomsTemplate_.Module.JobDeclarationModule", Constants.CountryCodes._EUTemplateCountryName_));
			Add(new ModuleInfo(ModuleIDs.SupplierPart, "Enterprise.Customs.EU.Module", "Enterprise.Customs.EU.Module.OrgSupplierPartModule", Constants.CountryCodes._EUTemplateCountryName_));
#endif

			// Message and Interchange
			Add(new ModuleInfo(ModuleIDs.Messaging.EDICommunicationsMode, "Enterprise.Messaging.Module", "Enterprise.Messaging.Module.EDICommunicationsModeModule", new TableRegistrationInfo(EDICommunicationsModeSchema.Constants.TableName)));
			Add(new ModuleInfo(ModuleIDs.Messaging.EDIInterchange, "Enterprise.Messaging.Module", "Enterprise.Messaging.Module.EDIInterchangeModule", new TableRegistrationInfo(EDIInterchangeSchema.Constants.TableName)));
			Add(new ModuleInfo(ModuleIDs.Messaging.EDIMessage, "Enterprise.Messaging.Module", "Enterprise.Messaging.Module.EDIMessageModule", new TableRegistrationInfo(EDIMessageSchema.Constants.TableName)));
			Add(new ModuleInfo(ModuleIDs.Messaging.EDICommunicationParty, "Enterprise.Messaging.Module", "Enterprise.Messaging.Module.EDICommunicationPartyModule"));
			Add(new ModuleInfo(ModuleIDs.Messaging.EDICodeMapping, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.EDICodeMappingModule"));
			Add(new ModuleInfo(ModuleIDs.Messaging.EDIMessagePurpose, "Enterprise.Workflow.Module", "Enterprise.Workflow.Module.EDIMessagePurposeModule"));
			Add(new ModuleInfo(ModuleIDs.Messaging.EDIMessageContentFilter, "Enterprise.Workflow.Module", "Enterprise.Workflow.Module.EDIMessageContentFilterModule"));
			Add(new ModuleInfo(ModuleIDs.Messaging.EDIMessageDeliveryContext, "Enterprise.Workflow.Module", "Enterprise.Workflow.Module.EDIMessageDeliveryContextModule"));
			Add(new ModuleInfo(ModuleIDs.Messaging.UniversalValidationRule, "Enterprise.Workflow.Module", "Enterprise.Workflow.Module.ValidationRuleModule"));

			// Resource Strings
			Add(new ModuleInfo(ModuleIDs.ResourceStrings, "Enterprise.ResourceStrings.Module", "Enterprise.ResourceStrings.Module.ResourceStringsModule"));
			Add(new ModuleInfo(ModuleIDs.LocalLanguages, "Enterprise.ResourceStrings.Module", "Enterprise.ResourceStrings.Module.LocalLanguagesModule"));
			Add(new ModuleInfo(ModuleIDs.TranslationFeedback, "Enterprise.ResourceStrings.Module", "Enterprise.ResourceStrings.Module.TranslationFeedbackModule"));

			Add(new ModuleInfo(ModuleIDs.GlowHRMS, "Enterprise.HRM.Module", "Enterprise.HRM.Module.GlowHRMSModule"));

			//Recruiter
			Add(new ModuleInfo(ModuleIDs.HRJobApplicant, "Enterprise.Recruiter.Module", "Enterprise.Recruiter.Module.HRJobApplicantModule"));
			Add(new ModuleInfo(ModuleIDs.HRJobApplication, "Enterprise.Recruiter.Module", "Enterprise.Recruiter.Module.HRJobApplicationModule"));
			Add(new ModuleInfo(ModuleIDs.HRJobRole, "Enterprise.Recruiter.Module", "Enterprise.Recruiter.Module.HRJobRoleModule"));
			Add(new ModuleInfo(ModuleIDs.HRJobOpenings, "Enterprise.Recruiter.Module", "Enterprise.Recruiter.Module.HRJobOpeningsModule"));
			Add(new ModuleInfo(ModuleIDs.LearningCentreCampaign, "Enterprise.Recruiter.Module", "Enterprise.Recruiter.Module.LearningCentreCampaignModule"));
			Add(new ModuleInfo(ModuleIDs.HRGlbCompanyCampaign, "Enterprise.Recruiter.Module", "Enterprise.Recruiter.Module.HRGlbCompanyCampaignModule"));
			Add(new ModuleInfo(ModuleIDs.HRGlbCompanyCampaignContact, "Enterprise.Recruiter.GUI", "Enterprise.Recruiter.GUI.HRGlbCompanyCampaignContactModule"));
			Add(new ModuleInfo(ModuleIDs.HRReports, "Enterprise.DocumentEngine.Module", "Enterprise.MasterFiles.Module.HRReports"));
			Add(new ModuleInfo(ModuleIDs.GlbAccreditation, "Enterprise.Recruiter.Module", "Enterprise.Recruiter.Module.GlbAccreditationModule"));
			Add(new ModuleInfo(ModuleIDs.GlbAccreditationAttempt, "Enterprise.Recruiter.Module", "Enterprise.Recruiter.Module.GlbAccreditationAttemptModule"));
			Add(new ModuleInfo(ModuleIDs.GlbAccreditationGroup, "Enterprise.Recruiter.Module", "Enterprise.Recruiter.Module.GlbAccreditationGroupModule"));
			Add(new ModuleInfo(ModuleIDs.HREmails, "Enterprise.Recruiter.Module", "Enterprise.Recruiter.Module.HREmailsModule"));
			Add(new ModuleInfo(ModuleIDs.ExamSetting, "Enterprise.Recruiter.Module", "Enterprise.Recruiter.Module.ExamSettingModule"));
			Add(new ModuleInfo(ModuleIDs.HRHiringRequest, "Enterprise.Recruiter.Module", "Enterprise.Recruiter.Module.HRHiringRequestModule"));
			Add(new ModuleInfo(ModuleIDs.HROnBoarding, "Enterprise.Recruiter.Module", "Enterprise.Recruiter.Module.HROnBoardingModule"));

			// Recruitment
			Add(new ModuleInfo(ModuleIDs.RecruitmentCandidateManagement, "Enterprise.Recruitment.Module", "Enterprise.Recruitment.Module.CandidateManagementModule"));
			Add(new ModuleInfo(ModuleIDs.ReviewProcess, "Enterprise.HRM.Module", "Enterprise.HRM.Module.ReviewProcessModule"));
			Add(new ModuleInfo(ModuleIDs.ReviewProcessNode, "Enterprise.HRM.Module", "Enterprise.HRM.Module.ReviewProcessNodeModule"));

			// Process Manager
			Add(new ModuleInfo(ModuleIDs.ProcessTasks, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.ProcessTasksModule", new TableRegistrationInfo(ProcessTasksSchema.Constants.TableName)));
			Add(new ModuleInfo(ModuleIDs.ProcessHeader, "Enterprise.BufferManagement.Module", "Enterprise.BufferManagement.Module.ProcessHeaderModule", new TableRegistrationInfo(ProcessHeaderSchema.Constants.TableName)));
			Add(new ModuleInfo(ModuleIDs.ProcessHeaderLink, "Enterprise.BufferManagement.Module", "Enterprise.BufferManagement.Module.ProcessHeaderLinkModule"));
			Add(new ModuleInfo(ModuleIDs.ProcessTemplates, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.ProcessTaskTemplateModule"));
			Add(new ModuleInfo(ModuleIDs.ProcessCompanyLinkRule, "Enterprise.Workflow.Module", "Enterprise.Workflow.Module.ProcessCompanyLinkRuleModule"));
			Add(new ModuleInfo(ModuleIDs.ProcessFieldChangeRule, "Enterprise.Workflow.Module", "Enterprise.Workflow.Module.ProcessFieldChangeRuleModule"));
			Add(new ModuleInfo(ModuleIDs.BMTagDefinition, "Enterprise.BufferManagement.Module", "Enterprise.BufferManagement.Module.BMTagDefinitionModule"));
			Add(new ModuleInfo(ModuleIDs.BMTagMagnitude, "Enterprise.BufferManagement.Module", "Enterprise.BufferManagement.Module.BMTagMagnitudeModule"));
			Add(new ModuleInfo(ModuleIDs.BMTagRule, "Enterprise.BufferManagement.Module", "Enterprise.BufferManagement.Module.BMTagRuleModule"));
			Add(new ModuleInfo(ModuleIDs.AcceptabilityBand, "Enterprise.BufferManagement.Module", "Enterprise.BufferManagement.Module.AcceptabilityBandModule", new TableRegistrationInfo(BMComponentAcceptabilityBandSchema.Constants.TableName)));
			Add(new ModuleInfo(ModuleIDs.BMFilterRule, "Enterprise.BufferManagement.Module", "Enterprise.BufferManagement.Module.BMFilterRuleModule"));
			Add(new ModuleInfo(ModuleIDs.Events, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.EventsModule"));
			Add(new ModuleInfo(ModuleIDs.BMSystems, "Enterprise.BufferManagement.Module", "Enterprise.BufferManagement.Module.BMSystemsModule", new TableRegistrationInfo(BMSystemSchema.Constants.TableName)));
			Add(new ModuleInfo(ModuleIDs.BMBufferTimespan, "Enterprise.BufferManagement.Module", "Enterprise.BufferManagement.Module.BMBufferTimespanModule"));
			Add(new ModuleInfo(ModuleIDs.BMBoard, "Enterprise.BufferManagement.Module", "Enterprise.BufferManagement.Module.BMBoardModule"));
			Add(new ModuleInfo(ModuleIDs.VisualBoard, "Enterprise.BufferManagement.Module", "Enterprise.BufferManagement.Module.VisualBoardModule"));
			Add(new ModuleInfo(ModuleIDs.BMBoardSlideshow, "Enterprise.BufferManagement.Module", "Enterprise.BufferManagement.Module.BMBoardSlideshowModule"));
			Add(new ModuleInfo(ModuleIDs.BMReports, "Enterprise.DocumentEngine.Module", "Enterprise.MasterFiles.Module.BMReports"));
			Add(new ModuleInfo(ModuleIDs.BMComponent, "Enterprise.BufferManagement.Module", "Enterprise.BufferManagement.Module.BMComponentModule"));
			Add(new ModuleInfo(ModuleIDs.ComponentRelationship, "Enterprise.BufferManagement.Module", "Enterprise.BufferManagement.Module.ComponentRelationshipModule"));
			Add(new ModuleInfo(ModuleIDs.ViewComponentChangeLog, "Enterprise.BufferManagement.Module", "Enterprise.BufferManagement.Module.ViewComponentChangeLogModule"));
			Add(new ModuleInfo(ModuleIDs.NetworkDiagram, "Enterprise.BufferManagement.NetworkVisualisation.Module", "Enterprise.BufferManagement.NetworkVisualisation.Module.NetworkDiagramModule", new TableRegistrationInfo(BMNCNShapeSchema.Constants.TableName)));
			Add(new ModuleInfo(ModuleIDs.WorkQueues, "Enterprise.BufferManagement.Module", "Enterprise.BufferManagement.Module.WorkQueuesModule"));
			Add(new ModuleInfo(ModuleIDs.BMControlCustomisation, "Enterprise.BufferManagement.Module", "Enterprise.BufferManagement.Module.BMControlCustomisationModule", new TableRegistrationInfo(BMControlCustomisationSchema.Constants.TableName)));
			Add(new ModuleInfo(ModuleIDs.BMReleaseSequence, "Enterprise.BufferManagement.Module", "Enterprise.BufferManagement.Module.BMReleaseSequenceModule"));
			Add(new ModuleInfo(ModuleIDs.CompletionTriggerAction, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.CompletionTriggerActionModule"));
			Add(new ModuleInfo(ModuleIDs.WorkflowExceptions, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.WorkflowExceptionsModule"));
			Add(new ModuleInfo(ModuleIDs.WorkflowExceptionTypes, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.WorkflowExceptionTypesModule"));
			Add(new ModuleInfo(ModuleIDs.WorkflowMilestones, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.WorkflowMilestonesModule"));
			Add(new ModuleInfo(ModuleIDs.WorkflowTriggers, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.WorkflowTriggersModule"));
			Add(new ModuleInfo(ModuleIDs.ExternalRequestTypes, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.ExternalRequestTypesModule"));
			Add(new ModuleInfo(ModuleIDs.ExternalRequestInfoTemplate, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.ExternalRequestInfoTemplateModule"));
			Add(new ModuleInfo(ModuleIDs.ExternalRequests, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.ExternalRequestsModule"));

			Add(new ModuleInfo(ModuleIDs.MENTAgedScoreQuery, "Enterprise.PAVE.MENT.Module", "Enterprise.PAVE.MENT.Module.MENTAgedScoreQueryModule"));
			Add(new ModuleInfo(ModuleIDs.MENTAgedScoreExtraction, "Enterprise.PAVE.MENT.Module", "Enterprise.PAVE.MENT.Module.MENTAgedScoreExtractionModule"));

			//Archive Manager
			Add(new ModuleInfo(ModuleIDs.ArchiveSchedule, "Enterprise.ArchiveManager.Module", "Enterprise.ArchiveManager.Module.Schedule.ArchiveScheduleModule"));
			Add(new ModuleInfo(ModuleIDs.ArchivedRecords, "Enterprise.ArchiveManager.Module", "Enterprise.ArchiveManager.Module.Records.ArchivedRecordsModule"));

			// Process Management
			Add(new ModuleInfo(ModuleIDs.WorkItem, "Enterprise.ProcessManagement.Module", "Enterprise.ProcessManagement.Module.WorkItemModule", new TableRegistrationInfo(WorkItemSchema.Constants.TableName)));
			Add(new ModuleInfo(ModuleIDs.Project, "Enterprise.ProcessManagement.Module", "Enterprise.ProcessManagement.Module.ProjectModule", new TableRegistrationInfo(WorkProjectSchema.Constants.TableName)));
			Add(new ModuleInfo(ModuleIDs.CustomerServiceTicket, "Enterprise.ProcessManagement.Module", "Enterprise.ProcessManagement.Module.CustomerServiceTicketModule"));

			Add(new ModuleInfo(ModuleIDs.UniversalCopySchedule, "Enterprise.UniversalCopy.Module", "Enterprise.UniversalCopy.Module.UniversalCopyScheduleModule", new TableRegistrationInfo(StmUniversalCopySchema.Constants.TableName)));

			var newClientModules = RegistrationOverrides?.NewClientModules;
			if (newClientModules != null)
			{
				Add(newClientModules);
			}

			//Business Intelligence and Analytics
			Add(new ModuleInfo(ModuleIDs.AnalyticsReports, "CargoWise.Bi.Product.Module", "CargoWise.Bi.Product.Module.Controller.PowerBiAnalyticsReportsModule"));
			Add(new ModuleInfo(ModuleIDs.BiManager, "CargoWise.Bi.Product.Manager.GUI", "CargoWise.Bi.Product.Manager.Controller.BiManagerModule"));
			Add(new ModuleInfo(ModuleIDs.Audit, "Enterprise.ZArchitecture.GUI.UserControls", "Enterprise.ZArchitecture.GUI.ZAudit.PlugIn.AuditModule"));

			// Value Analysis
			Add(new ModuleInfo(ModuleIDs.ValueAnalysisForwardingOrg, "Enterprise.MarketingManager.Module", "Enterprise.MarketingManager.Module.ValueAnalysisForwardingOrgModule"));
			Add(new ModuleInfo(ModuleIDs.ValueAnalysisCustomsBrokerageOrg, "Enterprise.MarketingManager.Module", "Enterprise.MarketingManager.Module.ValueAnalysisCustomsBrokerageOrgModule"));
			Add(new ModuleInfo(ModuleIDs.ValueAnalysisTransportOrg, "Enterprise.MarketingManager.Module", "Enterprise.MarketingManager.Module.ValueAnalysisTransportOrgModule"));
			Add(new ModuleInfo(ModuleIDs.ValueAnalysisWarehouseOrg, "Enterprise.MarketingManager.Module", "Enterprise.MarketingManager.Module.ValueAnalysisWarehouseOrgModule"));
			Add(new ModuleInfo(ModuleIDs.ValueAnalysisLinerAgencyOrg, "Enterprise.MarketingManager.Module", "Enterprise.MarketingManager.Module.ValueAnalysisLinerAgencyOrgModule"));

			Add(new ModuleInfo(ModuleIDs.ValueAnalysisForwardingOpp, "Enterprise.MarketingManager.Module", "Enterprise.MarketingManager.Module.ValueAnalysisForwardingOppModule"));
			Add(new ModuleInfo(ModuleIDs.ValueAnalysisCustomsBrokerageOpp, "Enterprise.MarketingManager.Module", "Enterprise.MarketingManager.Module.ValueAnalysisCustomsBrokerageOppModule"));
			Add(new ModuleInfo(ModuleIDs.ValueAnalysisTransportOpp, "Enterprise.MarketingManager.Module", "Enterprise.MarketingManager.Module.ValueAnalysisTransportOppModule"));
			Add(new ModuleInfo(ModuleIDs.ValueAnalysisWarehouseOpp, "Enterprise.MarketingManager.Module", "Enterprise.MarketingManager.Module.ValueAnalysisWarehouseOppModule"));
			Add(new ModuleInfo(ModuleIDs.ValueAnalysisLinerAgencyOpp, "Enterprise.MarketingManager.Module", "Enterprise.MarketingManager.Module.ValueAnalysisLinerAgencyOppModule"));

			Add(new ModuleInfo(ModuleIDs.GateBooking, "Enterprise.Freight.CFS.Module", "Enterprise.Freight.CFS.Module.GateBookingModule"));
			Add(new ModuleInfo(ModuleIDs.GateControl, "Enterprise.Freight.CFS.Module", "Enterprise.Freight.CFS.Module.GateControlModule"));

			// Container Yard
			Add(new ModuleInfo(ModuleIDs.CYDAdHocServiceOrder, "Enterprise.Warehouse.Yard.Module", "Enterprise.Warehouse.Yard.Module.CYDAdHocServiceOrderModule"));
			Add(new ModuleInfo(ModuleIDs.CYDDeliveryHeader, "Enterprise.Warehouse.Yard.Module", "Enterprise.Warehouse.Yard.Module.CYDDeliveryHeaderModule"));
			Add(new ModuleInfo(ModuleIDs.CYDPickupHeader, "Enterprise.Warehouse.Yard.Module", "Enterprise.Warehouse.Yard.Module.CYDPickupHeaderModule"));
			Add(new ModuleInfo(ModuleIDs.CYDReceiveAdvice, "Enterprise.Warehouse.Yard.Module", "Enterprise.Warehouse.Yard.Module.CYDReceiveAdviceModule"));
			Add(new ModuleInfo(ModuleIDs.CYDReleaseAdvice, "Enterprise.Warehouse.Yard.Module", "Enterprise.Warehouse.Yard.Module.CYDReleaseAdviceModule"));
			Add(new ModuleInfo(ModuleIDs.ContainerYardPortal, "Enterprise.Warehouse.Yard.Module", "Enterprise.Warehouse.Yard.Module.ContainerYardPortalModule"));
			Add(new ModuleInfo(ModuleIDs.CYDTransportationUnit, "Enterprise.Warehouse.Yard.Module", "Enterprise.Warehouse.Yard.Module.CYDTransportationUnitModule"));
			Add(new ModuleInfo(ModuleIDs.CYDYardUnitState, "Enterprise.Warehouse.Yard.Module", "Enterprise.Warehouse.Yard.Module.CYDYardUnitStateModule"));
			Add(new ModuleInfo(ModuleIDs.CYDYardReport, "Enterprise.Warehouse.Yard.Module", "Enterprise.Warehouse.Yard.Module.YardReportModule"));
			Add(new ModuleInfo(ModuleIDs.MNRWorkOrder, "Enterprise.Warehouse.Yard.Module", "Enterprise.Warehouse.Yard.Module.MNRWorkOrderModule"));
			Add(new ModuleInfo(ModuleIDs.MNRSurvey, "Enterprise.Warehouse.Yard.Module", "Enterprise.Warehouse.Yard.Module.MNRSurveyModule"));
			Add(new ModuleInfo(ModuleIDs.CYDPeriodicInvoicing, "Enterprise.Warehouse.Yard.Module", "Enterprise.Warehouse.Yard.Module.CYDPeriodicInvoicingModule"));

			// Gate Management
			Add(new ModuleInfo(ModuleIDs.GteBooking, "Enterprise.Warehouse.GateManagement.GUI", "Enterprise.Warehouse.GateManagement.GUI.GteBookingModule"));
			Add(new ModuleInfo(ModuleIDs.GteGateMovementBooking, "Enterprise.Warehouse.GateManagement.GUI", "Enterprise.Warehouse.GateManagement.GUI.GteGateMovementBookingModule"));
			Add(new ModuleInfo(ModuleIDs.GteGateMovement, "Enterprise.Warehouse.GateManagement.GUI", "Enterprise.Warehouse.GateManagement.GUI.GteGateMovementModule"));
			Add(new ModuleInfo(ModuleIDs.GteVehicleMovement, "Enterprise.Warehouse.GateManagement.GUI", "Enterprise.Warehouse.GateManagement.GUI.GteVehicleMovementModule"));
			Add(new ModuleInfo(ModuleIDs.GateManagementPortal, "Enterprise.Warehouse.GateManagement.GUI", "Enterprise.Warehouse.GateManagement.GUI.GateManagementPortalModule"));

			//TR
			Add(new ModuleInfo(ModuleIDs.Customs.TR.ETrade, "Enterprise.Customs.TR.ETrade.Module", "Enterprise.Customs.TR.ETrade.Module.ETradeModule", Constants.CountryCodes.Turkey));
			Add(new ModuleInfo(ModuleIDs.Customs.TR.SimplifiedProcedureTransitSystem, "Enterprise.Customs.TR.NCTS.Module", "Enterprise.Customs.TR.NCTS.Module.SPTSModule", Constants.CountryCodes.Turkey));
			Add(new ModuleInfo(ModuleIDs.Customs.TR.StatementsStampDuty, "Enterprise.Customs.TR.Module", "Enterprise.Customs.TR.Module.StatementsStampDutyModule", Constants.CountryCodes.Turkey));

			//Container Load List
			Add(new ModuleInfo(ModuleIDs.ContainerLoadList, "Enterprise.Freight.Forwarding.Module", "Enterprise.Freight.Forwarding.Module.ContainerLoadListModule"));

			//Container Load Plan
			Add(new ModuleInfo(ModuleIDs.ContainerLoadPlan, "Enterprise.Freight.Forwarding.Module", "Enterprise.Freight.Forwarding.Module.ContainerLoadPlanModule"));

			Add(new ModuleInfo(ModuleIDs.OrdersWebPortal, "Enterprise.Freight.Forwarding.Module", "Enterprise.Freight.Forwarding.Module.OrdersWebPortalModule"));
			Add(new ModuleInfo(ModuleIDs.OrderLinesWebPortal, "Enterprise.Freight.Forwarding.Module", "Enterprise.Freight.Forwarding.Module.OrderLinesWebPortalModule"));
			Add(new ModuleInfo(ModuleIDs.ControlTower, "Enterprise.Freight.Forwarding.Module", "Enterprise.Freight.Forwarding.Module.ControlTowerModule"));

			//CO
			Add(new ModuleInfo(ModuleIDs.Customs.CO.DocumentIDs, "Enterprise.Customs.CO.Manifest.Module", "Enterprise.Customs.CO.Manifest.Module.DocumentIDsModule", Constants.CountryCodes.Colombia));

			//IE
			Add(new ModuleInfo(ModuleIDs.Customs.EU.IE.CustomsAndExciseReports, "Enterprise.Customs.IE.Module", "Enterprise.Customs.IE.Module.CustomsAndExciseReportsModule", Constants.CountryCodes.Ireland));

			// GHG
			Add(new ModuleInfo(ModuleIDs.CO2eDashboard, "Enterprise.Freight.Forwarding.Module", "Enterprise.Freight.Forwarding.Module.CO2eDashboardModule"));

			AddAdditionalModuleRegistrations();
		}

		void AddAdditionalModuleRegistrations()
		{
			foreach (var moduleInfo in ModuleListingSubsetRegister.GetRegisteredModuleInfos())
			{
				Add(moduleInfo);
			}
		}

		public ModuleIdentifier GetRegisteredIdentifierByTableName(string tableName, string countryCode = null)
		{
			return GetRegisteredIdentifiersByTableName(tableName, countryCode)?.FirstOrDefault();
		}

		public IEnumerable<ModuleIdentifier> GetRegisteredIdentifiersByTableName(string tableName, string countryCode = null)
		{
			if (string.IsNullOrEmpty(tableName))
			{
				return null;
			}
			countryCode ??= "";
			var filteredByTable = All.Where(i => i.TableName == tableName).ToList();
			var found = filteredByTable.Where(i => i.CountryCode == countryCode).ToList();
			if (found.Count == 0 && !string.IsNullOrEmpty(countryCode))
			{
				found = filteredByTable.Where(i => string.IsNullOrEmpty(i.CountryCode)).ToList();
			}

			if (found.Count > 0)
			{
				return found.Select(m => m.ID);
			}

			var id = GetRegisteredIdentifierByName(tableName);
			if (id != null)
			{
				var moduleInfo = this[id, countryCode];
				if (moduleInfo != null && string.IsNullOrEmpty(moduleInfo.TableName))
				{
					return new List<ModuleIdentifier>() { id };
				}
			}

			return GetModuleIdentifiersByQueryingParentModule(tableName, countryCode);
		}

		IEnumerable<ModuleIdentifier> GetModuleIdentifiersByQueryingParentModule(string tableName, string countryCode)
		{
			var parentTableName = RegistrationOverrides?.GetParentTableName(tableName);

			if (parentTableName == null)
			{
				return null;
			}

			var id = GetRegisteredIdentifierByName(parentTableName);
			if (id != null)
			{
				var moduleInfo = this[id, countryCode];
				if (moduleInfo != null)
				{
					return new List<ModuleIdentifier>() { id };
				}
			}

			return null;
		}

		public ModuleIdentifier GetRegisteredIdentifierByColumnNamePrefix(string prefix, string countryCode = null)
		{
			return GetRegisteredIdentifiersByColumnNamePrefix(prefix, countryCode)?.FirstOrDefault();
		}

		public IEnumerable<ModuleIdentifier> GetRegisteredIdentifiersByColumnNamePrefix(string prefix, string countryCode = null)
		{
			if (string.IsNullOrEmpty(prefix))
			{
				return null;
			}
			var schema = EnterpriseSchema.GetTableSchemaFromColumnNamePrefix(prefix);
			if (schema != null)
			{
				return GetRegisteredIdentifiersByTableName(schema.TableName, countryCode);
			}
			return null;
		}

		#endregion

		#region Implementation

		void Add(NewClientModuleInfo[] infos)
		{
			foreach (NewClientModuleInfo clientInfo in infos)
			{
				Add(clientInfo.Info);
			}
		}

		protected override void Add(ModuleInfo info)
		{
			base.Add(RegistrationOverrides?.ModuleOverrides?[info.ID, info.CountryCode] ?? info);
		}

#if DEBUG
		public void Add_ExposedForTesting(ModuleInfo info)
		{
			Add(info);
		}
#endif

		internal static string[] EuCountriesThatHaveTheirOwnJobDeclarationModule
		{
			get
			{
				return new string[]
					{
						Constants.CountryCodes.UnitedKingdom,
						Constants.CountryCodes.Germany,
						Constants.CountryCodes.Italy,
						Constants.CountryCodes.France,
						Constants.CountryCodes.Spain,
						Constants.CountryCodes.Poland,
						Constants.CountryCodes.Ireland,
						Constants.CountryCodes.Belgium,
						Constants.CountryCodes.Turkey,
						Constants.CountryCodes.Netherlands,
						Constants.CountryCodes.Sweden,
						Constants.CountryCodes.Denmark,
						Constants.CountryCodes.Finland,
						Constants.CountryCodes._EUTemplateCountryName_
					};
			}
		}

		internal static string[] EuCountriesThatHaveTheirOwnTempStorageRegistryModule
		{
			get
			{
				return new string[]
				{
					Constants.CountryCodes.France,
					Constants.CountryCodes.Italy
				};
			}
		}

		internal static string[] EuCountriesThatHaveTheirOwnTempStorageRegisterLinesModule
		{
			get
			{
				return new string[]
				{
					Constants.CountryCodes.Spain
				};
			}
		}

		internal static string[] EuCountriesThatHaveTheirOwnH7BillModule
		{
			get
			{
				return new string[]
				{
					Constants.CountryCodes.UnitedKingdom,
					Constants.CountryCodes.Spain
				};
			}
		}

		internal static string[] EuCountriesThatHaveTheirOwnH7ModuleInfo
		{
			get
			{
				return new string[]
				{
					Constants.CountryCodes.UnitedKingdom,
					Constants.CountryCodes.Spain
				};
			}
		}

		internal static string[] EuCountriesThatHaveTheirOwnCommercialInvoiceModule => new[]
		{
			Constants.CountryCodes.UnitedKingdom,
			Constants.CountryCodes.Germany,
			Constants.CountryCodes.Ireland,
			Constants.CountryCodes.Poland
		};

		internal static string[] EuCountriesThatHaveTheirOwnEntryHeaderModule => new[]
		{
			Constants.CountryCodes.France,
			Constants.CountryCodes.Ireland,
			Constants.CountryCodes.Italy,
			Constants.CountryCodes.Spain,
			Constants.CountryCodes.UnitedKingdom,
			Constants.CountryCodes.Netherlands
		};

		internal static string[] EuCountriesThatHaveTheirOwnSupplierPartModule => new[]
		{
			Constants.CountryCodes.UnitedKingdom,
			Constants.CountryCodes.Italy,
			Constants.CountryCodes.France,
			Constants.CountryCodes.Germany,
			Constants.CountryCodes.Spain,
			Constants.CountryCodes.Poland,
			Constants.CountryCodes.Ireland,
			Constants.CountryCodes.Belgium,
			Constants.CountryCodes.Turkey,
			Constants.CountryCodes.Netherlands,
			Constants.CountryCodes.Sweden,
			Constants.CountryCodes.Denmark,
			Constants.CountryCodes.Finland,
			Constants.CountryCodes._EUTemplateCountryName_
		};

		internal static string[] EuCountriesThatHaveTheirOwnTemporaryStorageModule
		{
			get
			{
				return new string[]
					{
						Constants.CountryCodes.Germany,
						Constants.CountryCodes.Italy
					};
			}
		}

		internal static string[] EuCountriesThatHaveTheirOwnUCC6TemporaryStorageModule
		{
			get
			{
				return
				[
					Constants.CountryCodes.France,
					Constants.CountryCodes.Ireland,
					Constants.CountryCodes.Italy,
				];
			}
		}

		internal static string[] EuCountriesThatHaveTheirOwnPermitModule => new[]
		{
			Constants.CountryCodes.UnitedKingdom
		};

		internal static string[] EuCountriesThatHaveTheirOwnGuaranteeModule => new[]
		{
			Constants.CountryCodes.France,
			Constants.CountryCodes.Germany,
			Constants.CountryCodes.Spain
		};

		public static string[] EuCountriesThatHaveTheirOwnNctsModule => new[]
		{
			Constants.CountryCodes.France,
			Constants.CountryCodes.Spain,
			Constants.CountryCodes.Netherlands,
		};

		#endregion
#if DEBUG

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Method called via Reflection from Enterprise.ReflectionTestDeadCodeTest.TestNoDeadCode()")]
		[TypeFactoryAnnotationMethod]
		static IEnumerable<string> TypeFactoryAnnotation()
		{
			using (Db.DisposableActionForDbConnection())
			{
				return new ModuleList().All.Where(m => m != null && !string.IsNullOrEmpty(m.TypePath)).Select(m => m.TypePath);
			}
		}
#endif
	}

	#endregion
}
