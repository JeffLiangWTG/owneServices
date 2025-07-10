using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Data;
using Enterprise.Core;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Core.Constants;

namespace Enterprise.ZArchitecture.Modules
{
	#region SuppressResourceStringsCheckRegion

	public static class ControllerIDs
	{
		#region Controller IDs

		public static readonly ControllerID GenCustomAddOnRule = new ControllerID("GenCustomAddOnRule");

		public static readonly ControllerID DialogDefault = new ControllerID("DialogDefault");
		public static readonly ControllerID TradeLane = new ControllerID("TradeLane");
		public static readonly ControllerID Orders = new ControllerID("Orders");
		public static readonly ControllerID JobShipmentPreplanning = new ControllerID("JobShipmentPreplanning");
		public static readonly ControllerID Routing = new ControllerID("Routing");
		public static readonly ControllerID ClientRates = new ControllerID("ClientRates");
		public static readonly ControllerID ClientRateUpdates = new ControllerID("ClientRateUpdates");
		public static readonly ControllerID BulkRateUpdates = new ControllerID("BulkRateUpdates");
		public static readonly ControllerID GlobalRates = new ControllerID("GlobalRates");
		public static readonly ControllerID Quotations = new ControllerID("Quotation");
		public static readonly ControllerID Costing = new ControllerID("Costing");
		public static readonly ControllerID IntercompanyTariffs = new ControllerID("IntercompanyTariffs");
		public static readonly ControllerID CostsComparer = new ControllerID("CostsComparer");
		public static readonly ControllerID WiseRates = new ControllerID("WiseRates");
		public static readonly ControllerID Organisation = new ControllerID("Organisation");
		public static readonly ControllerID StaffAssignments = new ControllerID("StaffAssignments");
		public static readonly ControllerID OrgCollectionCalls = new ControllerID("OrgCollectionCalls");
		public static readonly ControllerID ClientIntelligence = new ControllerID("ClientIntelligence");
		public static readonly ControllerID CompetitorIntelligence = new ControllerID("CompetitorIntelligence");
		public static readonly ControllerID ProfitShare = new ControllerID("ProfitShare");
		public static readonly ControllerID TemporaryOrgRemover = new ControllerID("TemporaryOrgRemover");
		public static readonly ControllerID Opportunity = new ControllerID("Opportunity");
		public static readonly ControllerID CrmOpportunity = new ControllerID("CrmOpportunity");
		public static readonly ControllerID ServiceLevel = new ControllerID("ServiceLevel");
		public static readonly ControllerID RefVessel = new ControllerID("RefVessels");
		public static readonly ControllerID RefVesselZZ = new ControllerID("RefVesselsZZ");
		public static readonly ControllerID DataExportBatchPlugin = new ControllerID("DataExportBatchPlugin");
		public static readonly ControllerID TransactionsExport = new ControllerID("TransactionsExport");
		public static readonly ControllerID XmlTransactionsImport = new ControllerID("XmlTransactionsImport");
		public static readonly ControllerID CsvTransactionsImport = new ControllerID("CsvTransactionsImport");
		public static readonly ControllerID XmlJournalExport = new ControllerID("XmlJournalExport");
		public static readonly ControllerID XmlJournalImport = new ControllerID("XmlJournalImport");
		public static readonly ControllerID ARPaymentProcessing = new ControllerID("ARPaymentProcessing");
		public static readonly ControllerID APPaymentProcessing = new ControllerID("APPaymentProcessing");
		public static readonly ControllerID APPaymentBatchPosting = new ControllerID("APPaymentBatchPosting");
		public static readonly ControllerID ARReceiptBatchPosting = new ControllerID("ARReceiptBatchPosting");
		public static readonly ControllerID ChequeTransactionHeader = new ControllerID("ChequeTransactionHeader");
		public static readonly ControllerID Cheque = new ControllerID("Cheque");
		public static readonly ControllerID CsvAccountsImport = new ControllerID("CsvAccountsImport");
		public static readonly ControllerID JobManagement = new ControllerID("JobManagement");
		public static readonly ControllerID JobInvoicingForm = new ControllerID("JobInvoicingForm");
		public static readonly ControllerID JobConsolCostingForm = new ControllerID("JobConsolCostingForm");
		public static readonly ControllerID BulkJobClose = new ControllerID("BulkJobClose");
		public static readonly ControllerID BulkDSBJobCloseBatchApproval = new ControllerID("BulkDSBJobCloseBatchApproval");
		public static readonly ControllerID WIP = new ControllerID("WIP");
		public static readonly ControllerID ReviewProcess = new ControllerID("ReviewProcess");
		public static readonly ControllerID ReviewProcessNode = new ControllerID("ReviewProcessNode");
		public static readonly ControllerID Accrual = new ControllerID("Accrual");
		public static readonly ControllerID JobRevenueJournal = new ControllerID("JobRevenueJournal");
		public static readonly ControllerID GLAccountFormat = new ControllerID("GLAccountFormat");
		public static readonly ControllerID GLJournal = new ControllerID("GLJournal");
		public static readonly ControllerID GLJournalLinkedToApproval = new ControllerID("GLJournalLinkedToApproval");
		public static readonly ControllerID GLJournalApproval = new ControllerID("GLJournalApproval");
		public static readonly ControllerID GLConsolidationGroups = new ControllerID("GLConsolidationGroups");
		public static readonly ControllerID GLJournalImport = new ControllerID("GLJournalImport");
		public static readonly ControllerID AlternateChartofAccounts = new ControllerID("AlternateChartofAccounts");
		public static readonly ControllerID AlternateGLAccounts = new ControllerID("AlternateGLAccounts");
		public static readonly ControllerID APBulkInvoicePosting = new ControllerID("APBulkInvoicePosting");
		public static readonly ControllerID TransactionsPendingAllocation = new ControllerID("TransactionsPendingAllocation");
		public static readonly ControllerID MatchEPaymentRecipients = new ControllerID("MatchEPaymentRecipients");
		public static readonly ControllerID MatchEPaymentRecipientsBulk = new ControllerID("MatchEPaymentRecipientsBulk");
		public static readonly ControllerID PeriodicInvoice = new ControllerID("PeriodicInvoice");
		public static readonly ControllerID PeriodicInvoiceBulk = new ControllerID("PeriodicInvoiceBulk");
		public static readonly ControllerID ContainerStoragePayment = new ControllerID("ContainerStoragePayment");
		public static readonly ControllerID APInvoice = new ControllerID("APInvoice");
		public static readonly ControllerID APInvoiceFromDraftInvoice = new ControllerID("APInvoiceFromDraftInvoice");
		public static readonly ControllerID ARInvoice = new ControllerID("ARInvoice");
		public static readonly ControllerID ARInvoiceForInterCompanyTransaction = new ControllerID("ARInvoiceForInterCompanyTransaction");
		public static readonly ControllerID APCreditNote = new ControllerID("APCreditNote");
		public static readonly ControllerID APCreditNoteFromDraftInvoice = new ControllerID("APCreditNoteFromDraftInvoice");
		public static readonly ControllerID ARCreditNote = new ControllerID("ARCreditNote");
		public static readonly ControllerID ARCreditNoteForInterCompanyTransaction = new ControllerID("ARCreditNoteForInterCompanyTransaction");
		public static readonly ControllerID APAdjustmentNote = new ControllerID("APAdjustmentNote");
		public static readonly ControllerID ARAdjustmentNote = new ControllerID("ARAdjustmentNote");
		public static readonly ControllerID APJournal = new ControllerID("APJournal");
		public static readonly ControllerID ARJournal = new ControllerID("ARJournal");
		public static readonly ControllerID APBankFeeJournal = new ControllerID("APBankFeeJournal");
		public static readonly ControllerID ARBankFeeJournal = new ControllerID("ARBankFeeJournal");
		public static readonly ControllerID ARBalancingJournal = new ControllerID("ARBalancingJournal");
		public static readonly ControllerID APBalancingJournal = new ControllerID("APBalancingJournal");
		public static readonly ControllerID APPayment = new ControllerID("APPayment");
		public static readonly ControllerID ARPayment = new ControllerID("ARPayment");
		public static readonly ControllerID APReceipt = new ControllerID("APReceipt");
		public static readonly ControllerID ARReceipt = new ControllerID("ARReceipt");
		public static readonly ControllerID ZAPPayment = new ControllerID("ZAPPayment");
		public static readonly ControllerID ZARPayment = new ControllerID("ZARPayment");
		public static readonly ControllerID ZAPReceipt = new ControllerID("ZAPReceipt");
		public static readonly ControllerID ZARReceipt = new ControllerID("ZARReceipt");
		public static readonly ControllerID APDiscount = new ControllerID("APDiscount");
		public static readonly ControllerID ARDiscount = new ControllerID("ARDiscount");
		public static readonly ControllerID APExchangeDifference = new ControllerID("APExchangeDifference");
		public static readonly ControllerID ARExchangeDifference = new ControllerID("ARExchangeDifference");
		public static readonly ControllerID APOverpayment = new ControllerID("APOverpayment");
		public static readonly ControllerID AROverpayment = new ControllerID("AROverpayment");
		public static readonly ControllerID CNDataInterface = new ControllerID("CNDataInterface");
		public static readonly ControllerID CN2004DataInterface = new ControllerID("CN2004DataInterface");
		public static readonly ControllerID CNReconciliationExport = new ControllerID("CNReconciliationExport");
		public static readonly ControllerID AccountingVoucher = new ControllerID("AccountingVoucher");
		public static readonly ControllerID ChinaJournalListing = new ControllerID("ChinaJournalListing");// Controller ID
		public static readonly ControllerID DirectDebitFile = new ControllerID("DirectDebitFile");
		public static readonly ControllerID InvoiceBatch = new ControllerID("InvoiceBatch");
		public static readonly ControllerID InvoiceBulkBatch = new ControllerID("InvoiceBulkBatch");
		public static readonly ControllerID DirectPayment = new ControllerID("DirectPayment");
		public static readonly ControllerID DirectReceipt = new ControllerID("DirectReceipt");
		public static readonly ControllerID BankReconDirectPayment = new ControllerID("BankReconDirectPayment");
		public static readonly ControllerID BankReconDirectReceipt = new ControllerID("BankReconDirectReceipt");
		public static readonly ControllerID OpeningPayment = new ControllerID("OpeningPayment");
		public static readonly ControllerID OpeningReceipt = new ControllerID("OpeningReceipt");
		public static readonly ControllerID BankTransfer = new ControllerID("BankTransfer");
		public static readonly ControllerID DepositBatch = new ControllerID("DepositBatch");
		public static readonly ControllerID BankReconcilliation = new ControllerID("BankReconcilliation");
		public static readonly ControllerID BankCurrencyAdjustment = new ControllerID("BankCurrencyAdjustment");
		public static readonly ControllerID CashBookAPPayment = new ControllerID("CashBookAPPayment");
		public static readonly ControllerID CashBookAPReceipt = new ControllerID("CashBookAPReceipt");
		public static readonly ControllerID CashBookARPayment = new ControllerID("CashBookARPayment");
		public static readonly ControllerID CashBookARReceipt = new ControllerID("CashBookARReceipt");
		public static readonly ControllerID ARComplianceDocument = new ControllerID("ARComplianceDocument");
		public static readonly ControllerID ARVoidComplianceDocument = new ControllerID("ARVoidComplianceDocument");
		public static readonly ControllerID APVoidComplianceDocument = new ControllerID("APVoidComplianceDocument");
		public static readonly ControllerID APComplianceDocument = new ControllerID("APComplianceDocument");
		public static readonly ControllerID PaymentBatch = new ControllerID("PaymentBatch");
		public static readonly ControllerID ExporterScheme = new ControllerID("ExporterScheme");
		public static readonly ControllerID AdministrationPanel = new ControllerID("AdministrationPanel");
		public static readonly ControllerID ARAccQueryClaim = new ControllerID("ARAccQueryClaim");
		public static readonly ControllerID APAccQueryClaim = new ControllerID("APAccQueryClaim");
		public static readonly ControllerID GlbPortDeliveryTime = new ControllerID("GlbPortDeliveryTime");
		public static readonly ControllerID Containers = new ControllerID("Containers");
		public static readonly ControllerID JobShipment = new ControllerID("JobShipment");
		public static readonly ControllerID RoutingLookups = new ControllerID("RoutingLookups");
		public static readonly ControllerID JobConsol = new ControllerID("JobConsol");
		public static readonly ControllerID ConsolPlanningBoard = new ControllerID("ConsolPlanningBoard");
		public static readonly ControllerID SupplierBooking = new ControllerID("SupplierBooking");
		public static readonly ControllerID JobConsolContainersPacking = new ControllerID("JobConsolContainersPacking");
		public static readonly ControllerID DocumentTracking = new ControllerID("DocumentTracking");
		public static readonly ControllerID RefCommodityCode = new ControllerID("RefCommodityCode");
		public static readonly ControllerID RefAirlineCommodityCode = new ControllerID("RefAirlineCommodityCode");
		public static readonly ControllerID RefOrgPartCategory = new ControllerID("RefOrgPartCategory");
		public static readonly ControllerID RefEquipment = new ControllerID("RefEquipment");
		public static readonly ControllerID WarningAcknowledgement = new ControllerID("WarningAcknowledgement");
		public static readonly ControllerID RefPremisesGateCode = new ControllerID("RefPremisesGateCode");
		public static readonly ControllerID AccChargeCode = new ControllerID("AccChargeCode");
		public static readonly ControllerID AccGlobalChargeCode = new ControllerID("AccGlobalChargeCode");
		public static readonly ControllerID AccTaxOverrideGroup = new ControllerID("AccTaxOverrideGroup");
		public static readonly ControllerID AccReportingBook = new ControllerID("AccReportingBook");
		public static readonly ControllerID TaxFrameworkAccTaxOverrideGroup = new ControllerID("TaxFrameworkAccTaxOverrideGroup");
		public static readonly ControllerID AccPlaceOfSupplyChargeCodeGroup = new ControllerID("AccPlaceOfSupplyChargeCodeGroup");
		public static readonly ControllerID AccOrgTaxConfigurationTemplate = new ControllerID("AccOrgTaxConfigurationTemplate");
		public static readonly ControllerID GlobalChargeCodeIntercompany = new ControllerID("GlobalChargeCodeIntercompany");
		public static readonly ControllerID GlobalChargeCodeOrganization = new ControllerID("GlobalChargeCodeOrganization");
		public static readonly ControllerID RefCurrency = new ControllerID("RefCurrency");
		public static readonly ControllerID InternationalZone = new ControllerID("InternationalZone");
		public static readonly ControllerID RefTimeZoneSet = new ControllerID("RefTimeZoneSet");
		public static readonly ControllerID RefNMFC = new ControllerID("RefNMFC");
		public static readonly ControllerID RefCountry = new ControllerID("RefCountry");
		public static readonly ControllerID RefCityTown = new ControllerID("RefCityTown");
		public static readonly ControllerID RefPostCode = new ControllerID("RefPostCode");
		public static readonly ControllerID RefCountryStates = new ControllerID("RefCountryStates");
		public static readonly ControllerID CountryStatesGlbHoliday = new ControllerID("CountryStatesGlbHoliday");
		public static readonly ControllerID SalesEnquiry = new ControllerID("SalesEnquiry");
		public static readonly ControllerID SalesProduct = new ControllerID("SalesProduct");
		public static readonly ControllerID Commission = new ControllerID("Commission");
		public static readonly ControllerID CommissionLine = new ControllerID("CommissionLine");
		public static readonly ControllerID CommissionApprovalRequest = new ControllerID("CommissionApprovalRequest");
		public static readonly ControllerID Communication = new ControllerID("Communication");
		public static readonly ControllerID OrgAddresses = new ControllerID("OrgAddresses");
		public static readonly ControllerID GenShapeGeography = new ControllerID("GenShapeGeography");
		public static readonly ControllerID OrgCusCode = new ControllerID("OrgCusCode");
		public static readonly ControllerID OrgCommissionAgreement = new ControllerID("OrgCommissionAgreement");
		public static readonly ControllerID OrgContacts = new ControllerID("OrgContacts");
		public static readonly ControllerID OrgContactsStmALog = new ControllerID("OrgContactsStmALog");
		public static readonly ControllerID RefContainer = new ControllerID("RefContainer");
		public static readonly ControllerID RefContainerISOTypes = new ControllerID("RefContainerISOTypes");
		public static readonly ControllerID RefUNLOCO = new ControllerID("RefUNLOCO");
		public static readonly ControllerID DocAddresses = new ControllerID("DocAddresses");
		public static readonly ControllerID GateIn = new ControllerID("GateIn");
		public static readonly ControllerID GateOut = new ControllerID("GateOut");
		public static readonly ControllerID OutstandingDispatch = new ControllerID("OutstandingDispatch");
		public static readonly ControllerID RepairEstimate = new ControllerID("RepairEstimate");
		public static readonly ControllerID RepairPart = new ControllerID("RepairPart");
		public static readonly ControllerID RepairCategory = new ControllerID("RepairCategory");
		public static readonly ControllerID RepairItem = new ControllerID("RepairItem");
		public static readonly ControllerID Release = new ControllerID("Release");
		public static readonly ControllerID OrgCreditorGroup = new ControllerID("OrgCreditorGroup");
		public static readonly ControllerID OrgDebtorGroup = new ControllerID("OrgDebtorGroup");
		public static readonly ControllerID RefDocType = new ControllerID("RefDocType");
		public static readonly ControllerID RefDocSource = new ControllerID("RefDocSource");
		public static readonly ControllerID RefDocOrgCusCode = new ControllerID("RefDocOrgCusCode");
		public static readonly ControllerID GlbPerson = new ControllerID("GlbPerson");
		public static readonly ControllerID GlbPersonNew = new ControllerID("GlbPersonNew");
		public static readonly ControllerID GlbBranch = new ControllerID("GlbBranch");
		public static readonly ControllerID GlbCompany = new ControllerID("GlbCompany");
		public static readonly ControllerID GlbCompanyCampaignWithoutFilter = new ControllerID("GlbCompanyCampaignWithoutFilter");
		public static readonly ControllerID GlbCompanyCampaignClick = new ControllerID("GlbCompanyCampaignClick");
		public static readonly ControllerID GlbCompanyCampaignContact = new ControllerID("GlbCompanyCampaignContact");
		public static readonly ControllerID GlbDepartment = new ControllerID("GlbDepartment");
		public static readonly ControllerID GlbGroup = new ControllerID("GlbGroup");
		public static readonly ControllerID GlbStaff = new ControllerID("GlbStaff");
		public static readonly ControllerID GlbStaffChangeRequest = new ControllerID("GlbStaffChangeRequest");
		public static readonly ControllerID GlbStaffHoliday = new ControllerID("GlbStaffHoliday");
		public static readonly ControllerID GlbStaffCommissionPlugIn = new ControllerID("GlbStaffCommissionPlugIn");
		public static readonly ControllerID TradeProfileForOrgPlugin = new ControllerID("TradeProfileForOrgPlugin");
		public static readonly ControllerID TradeProfileForRelatedPlugin = new ControllerID("TradeProfileForRelatedPlugin");
		public static readonly ControllerID GlbCapability = new ControllerID("GlbCapability");
		public static readonly ControllerID GlbResource = new ControllerID("GlbResource");
		public static readonly ControllerID NewsAndAnnouncement = new ControllerID("NewsAndAnnouncement");
		public static readonly ControllerID AccGroups = new ControllerID("AccGroups");
		public static readonly ControllerID AccGLHeader = new ControllerID("AccGLHeader");
		public static readonly ControllerID AccGLAccountDescriptor = new ControllerID("AccGLAccountDescriptor");
		public static readonly ControllerID AccChequeBook = new ControllerID("AccChequeBook");
		public static readonly ControllerID AccComplianceSequence = new ControllerID("AccComplianceSequence");
		public static readonly ControllerID AccComplianceSequenceBulk = new ControllerID("AccComplianceSequenceBulk");
		public static readonly ControllerID LockComplianceBook = new ControllerID("LockComplianceBook");
		public static readonly ControllerID ReleaseComplianceBook = new ControllerID("ReleaseComplianceBook");
		public static readonly ControllerID AccBankAccount = new ControllerID("AccBankAccount");
		public static readonly ControllerID ImportAccountingData = new ControllerID("ImportAccountingData");
		public static readonly ControllerID OrderLine = new ControllerID("OrderLine");
		public static readonly ControllerID OrderLineFromOrder = new ControllerID("OrderLineFromOrder");
		public static readonly ControllerID AccTaxRate = new ControllerID("AccTaxRate");
		public static readonly ControllerID AccInvMsg = new ControllerID("AccInvMsg");
		public static readonly ControllerID AccWithholding = new ControllerID("AccWithholding");
		public static readonly ControllerID AccApportionmentTemplate = new ControllerID("AccApportionmentTemplate");
		public static readonly ControllerID JobSeaSailing = new ControllerID("JobSeaSailing");
		public static readonly ControllerID JobAirSailing = new ControllerID("JobAirSailing");
		public static readonly ControllerID JobRailSailing = new ControllerID("JobRailSailing");
		public static readonly ControllerID JobRoadSailing = new ControllerID("JobRoadSailing");
		public static readonly ControllerID JobSeaVoyage = new ControllerID("JobSeaVoyage");
		public static readonly ControllerID SailingScheduleImporting = new ControllerID("SailingImports");
		public static readonly ControllerID OnlineSailingSchedules = new ControllerID("OnlineSailingSchedules");
		public static readonly ControllerID DocumentAllocation = new ControllerID("DocumentAllocation");
		public static readonly ControllerID DocumentDbMerger = new ControllerID("DocumentDbMerger");
		public static readonly ControllerID DocumentDbManager = new ControllerID("DocumentDbManager");
		public static readonly ControllerID ArchiveEDocs = new ControllerID("ArchiveEDocs");
		public static readonly ControllerID DocumentTemplate = new ControllerID("DocumentTemplate");
		public static readonly ControllerID RateAttachmentSet = new ControllerID("RateAttachmentSet");
		public static readonly ControllerID SalesTeam = new ControllerID("SalesTeam");
		public static readonly ControllerID SalesRep = new ControllerID("SalesRep");
		public static readonly ControllerID RateTransportProvider = new ControllerID("RateTransportProvider");
		public static readonly ControllerID RateTransportZone = new ControllerID("RateTransportZone");
		public static readonly ControllerID PeriodManagement = new ControllerID("PeriodManagement");
		public static readonly ControllerID QuotedBookings = new ControllerID("QuotedBookings");
		public static readonly ControllerID OneOffQuotes = new ControllerID("OneOffQuotes");
		public static readonly ControllerID PreAllocations = new ControllerID("PreAllocations");
		public static readonly ControllerID ComplexAllLegs = new ControllerID("ComplexAllLegs");
		public static readonly ControllerID ComplexAnyLegs = new ControllerID("ComplexAnyLegs");
		public static readonly ControllerID ComplexDelivery = new ControllerID("ComplexDelivery");
		public static readonly ControllerID ComplexPickup = new ControllerID("ComplexPickup");
		public static readonly ControllerID ShipmentReceival = new ControllerID("ShipmentReceival");
		public static readonly ControllerID AccHotCheque = new ControllerID("AccHotCheque");
		public static readonly ControllerID PackContainerRegistration = new ControllerID("PackContainerRegistration");
		public static readonly ControllerID LoadListConsol = new ControllerID("LoadListConsol");
		public static readonly ControllerID LoadListContainersPacking = new ControllerID("LoadListContainersPacking");
		public static readonly ControllerID JobMawb = new ControllerID("JobMawb");
		public static readonly ControllerID PackContainers = new ControllerID("PackContainers");
		public static readonly ControllerID ManifestTally = new ControllerID("ManifestTally");
		public static readonly ControllerID ShipmentGatePass = new ControllerID("ShipmentGatePass");
		public static readonly ControllerID LoadList = new ControllerID("LoadList");
		public static readonly ControllerID eConversationPlugIn = new ControllerID("eConversationPlugIn");
		public static readonly ControllerID eDocsPlugIn = new ControllerID("eDocsPlugIn");
		public static readonly ControllerID DocumentUDFPlugIn = new ControllerID("DocumentUDFPlugIn");
		public static readonly ControllerID DocumentSDFPlugIn = new ControllerID("DocumentSDFPlugIn");
		public static readonly ControllerID DocDataPlugIn = new ControllerID("DocDataPlugIn");
		public static readonly ControllerID OperationalActions = new ControllerID("OperationalActions");
		public static readonly ControllerID PrintJob = new ControllerID("PrintJob");
		public static readonly ControllerID DocumentSigningJob = new ControllerID("DocumentSigningJob");
		public static readonly ControllerID ArchiveSchedule = new ControllerID("ArchiveSchedule");
		public static readonly ControllerID ArchivedRecords = new ControllerID("ArchivedRecords");
		public static readonly ControllerID ScheduledReports = new ControllerID("ScheduledReports");
		public static readonly ControllerID ReportStatistics = new ControllerID("ReportStatistics");
		public static readonly ControllerID ReportManagement = new ControllerID("ReportManagement");
		public static readonly ControllerID StmServiceTask = new ControllerID("StmServiceTask");
		public static readonly ControllerID ProcessController = new ControllerID("ProcessController");
		public static readonly ControllerID ServiceTaskLogViewer = new ControllerID("ServiceTaskLogViewer");
		public static readonly ControllerID ServiceTaskProxyConfiguration = new ControllerID("ServiceTaskProxyConfiguration");
		public static readonly ControllerID PrintQueue = new ControllerID("PrintQueue");
		public static readonly ControllerID Statement = new ControllerID("Statement");
		public static readonly ControllerID JobInvoicing = new ControllerID("JobInvoicing");
		public static readonly ControllerID JobInvoicingConsol = new ControllerID("JobInvoicingConsol");
		public static readonly ControllerID JobProfitLossConsol = new ControllerID("JobProfitLossConsolController");
		public static readonly ControllerID LinkedeNettEDIMessage = new ControllerID("LinkedeNettEDIMessage");
		public static readonly ControllerID Apportionment = new ControllerID("Apportionment");
		public static readonly ControllerID SellApportionmentForGateway = new ControllerID("SellApportionmentForGateway");
		public static readonly ControllerID ApportionmentForCommonWorkSheet = new ControllerID("ApportionmentForCommonWorkSheet");
		public static readonly ControllerID ApportionmentForTransitTransportationUnit = new ControllerID("ApportionmentForTransitTransportationUnit");
		public static readonly ControllerID APContra = new ControllerID("APContra");
		public static readonly ControllerID ARContra = new ControllerID("ARContra");
		public static readonly ControllerID JCCostingJournal = new ControllerID("JCCostingJournal");
		public static readonly ControllerID ARTransfer = new ControllerID("ARTransfer");
		public static readonly ControllerID APTransfer = new ControllerID("APTransfer");
		public static readonly ControllerID InvoicePrinting = new ControllerID("InvoicePrinting");
		public static readonly ControllerID JobHeader = new ControllerID("JobHeader");
		public static readonly ControllerID OrgMatchApproval = new ControllerID("OrgMatchApproval");
		public static readonly ControllerID OrgMatchApprovalCreateNewOrg = new ControllerID("OrgMatchApprovalCreateNewOrg");
		public static readonly ControllerID Cartage = new ControllerID("Cartage");
		public static readonly ControllerID CartageWorkSheet = new ControllerID("CartageWorkSheet");
		public static readonly ControllerID CartagePlugin = new ControllerID("CartagePlugin");
		public static readonly ControllerID ConfirmationsPlugin = new ControllerID("ConfirmationsPlugin");
		public static readonly ControllerID CartageLeg = new ControllerID("CartageLeg");
		public static readonly ControllerID CartageLegPlanner = new ControllerID("CartageLegPlanner");
		public static readonly ControllerID CartageRunSheetDashboard = new ControllerID("CartageRunSheetDashboard");
		public static readonly ControllerID APIncompleteInvoice = new ControllerID("APIncompleteInvoice");
		public static readonly ControllerID APIncompleteCreditNote = new ControllerID("APIncompleteCreditNote");
		public static readonly ControllerID APIncompleteAdjustmentNote = new ControllerID("APIncompleteAdjustmentNote");
		public static readonly ControllerID ARCreditNoteApproval = new ControllerID("ARCreditNoteApproval");
		public static readonly ControllerID TransactionsPendingAllocationApproval = new ControllerID("TransactionsPendingAllocationApproval");
		public static readonly ControllerID APInvoiceApproval = new ControllerID("APInvoiceApproval");
		public static readonly ControllerID APInvoiceLinkedToApproval = new ControllerID("APInvoiceLinkedToApproval");
		public static readonly ControllerID APCreditNoteLinkedToApproval = new ControllerID("APCreditNoteLinkedToApproval");
		public static readonly ControllerID APInvoiceNewForApproval = new ControllerID("APInvoiceNewForApproval");
		public static readonly ControllerID APCreditNoteNewForApproval = new ControllerID("APCreditNoteNewForApproval");
		public static readonly ControllerID CreditControlledDocumentsApproval = new ControllerID("CreditControlledDocumentsApproval");
		public static readonly ControllerID StmALog = new ControllerID("StmALog");
		public static readonly ControllerID StmModuleFilter = new ControllerID("StmModuleFilter");
		public static readonly ControllerID APIntercompanyCostsApportionment = new ControllerID("APIntercompanyCostsApportionment");
		public static readonly ControllerID PortDepotCarrierSelection = new ControllerID("PortDepotCarrierSelection");
		public static readonly ControllerID PortHubSelection = new ControllerID("PortHubSelection");
		public static readonly ControllerID DocumentVisualizer = new ControllerID("DocumentVisualizer");
		public static readonly ControllerID ErrorReportDetails = new ControllerID("ErrorReportDetails");
		public static readonly ControllerID AccCollectionBatch = new ControllerID("AccCollectionBatch");
		public static readonly ControllerID AccCollectionBatchPosting = new ControllerID("AccCollectionBatchPosting");
		public static readonly ControllerID AccCollectionOrder = new ControllerID("AccCollectionOrder");
		public static readonly ControllerID DeviceDetails = new ControllerID("TelematicsDeviceDetails");
		public static readonly ControllerID TelPreDriveChecklist = new ControllerID("TelematicsPreDriveChecklists");
		public static readonly ControllerID TelPreDriveChecklistTemplate = new ControllerID("TelematicsPreDriveChecklistTemplates");
		public static readonly ControllerID RefTransitTime = new ControllerID("RefTransitTime");
		public static readonly ControllerID AccPayableOrder = new ControllerID("AccPayableOrder");
		public static readonly ControllerID HVLVBookingHeader = new ControllerID("HVLVBookingHeader");
		public static readonly ControllerID HVLVConsignment = new ControllerID("HVLVConsignment");
		public static readonly ControllerID HVLVOriginLoadList = new ControllerID("HVLVOriginLoadList");
		public static readonly ControllerID HVLVOuterPackage = new ControllerID("HVLVOuterPackage");
		public static readonly ControllerID ARCashAdvance = new ControllerID("ARCashAdvance");
		public static readonly ControllerID APCashAdvance = new ControllerID("APCashAdvance");
		public static readonly ControllerID GatewayConsolProfitShareRedistribution = new ControllerID(nameof(GatewayConsolProfitShareRedistribution));
		public static readonly ControllerID ComplianceRiskPlugin = new ControllerID(nameof(ComplianceRiskPlugin));
		public static readonly ControllerID GlobalCommercialInvoicePlugin = new ControllerID(nameof(GlobalCommercialInvoicePlugin));
		public static readonly ControllerID TransitTimeServiceLevelCombination = new ControllerID("TransitTimeServiceLevelCombination");

		public static readonly ControllerID NettingStatement = new ControllerID("NettingStatement");
		public static readonly ControllerID NettingPeriod = new ControllerID("NettingPeriod");

		public static readonly ControllerID ConsolidatedTransportBooking = new ControllerID("ConsolidatedTransportBooking");
		public static readonly ControllerID PickupDeliveryConfirm = new ControllerID("PickupDeliveryConfirm");
		public static readonly ControllerID QuickPOD = new ControllerID("QuickPOD");

		public static readonly ControllerID CartageType = new ControllerID("CommonCartageType");
		public static readonly ControllerID ExchangeRateWrapper = new ControllerID("ExchangeRateWrapper");
		public static readonly ControllerID StmUpgrade = new ControllerID("StmUpgrade");
		public static readonly ControllerID UNDGSubstance = new ControllerID("UNDGSubstance");
		public static readonly ControllerID UNDGCommonData = new ControllerID("UNDGCommonData");
		public static readonly ControllerID RefCarrierConsortium = new ControllerID("RefCarrierConsortium");
		public static readonly ControllerID AUContainerMessaging = new ControllerID("AUContainerMessaging");
		public static readonly ControllerID GLBudget = new ControllerID("GLBudget");

		public static readonly ControllerID Registry = new ControllerID("Registry");
		public static readonly ControllerID StmFeatureTest = new ControllerID("StmFeatureTest");
		public static readonly ControllerID ZARMatching = new ControllerID("ZARMatching");
		public static readonly ControllerID ZAPMatching = new ControllerID("ZAPMatching");
		public static readonly ControllerID RefAirline = new ControllerID("RefAirline");
		public static readonly ControllerID NumericCodeRefAirline = new ControllerID("NumericCodeRefAirline");
		public static readonly ControllerID RefPackType = new ControllerID("RefPackType");
		public static readonly ControllerID RefDomesticCartageZone = new ControllerID("RefDomesticCartageZone");
		public static readonly ControllerID MailItem = new ControllerID("MailItem");
		public static readonly ControllerID MailItemTemplate = new ControllerID("MailItemTemplate");
		public static readonly ControllerID ProcessQueue = new ControllerID("ProcessQueue");
		public static readonly ControllerID ServiceRequest = new ControllerID("ServiceRequest");
		public static readonly ControllerID CASSCostFileImport = new ControllerID("CASSCostFileImport");
		public static readonly ControllerID UACreditNote = new ControllerID("UACreditNote");
		public static readonly ControllerID UAInvoice = new ControllerID("UAInvoice");
		public static readonly ControllerID GPSSupporter = new ControllerID("GPSSupporter");
		public static readonly ControllerID ClaimCharges = new ControllerID("ClaimCharges");
		public static readonly ControllerID AccComplianceReport = new ControllerID("AccComplianceReport");
		public static readonly ControllerID JobBillingExRateSysConfig = new ControllerID("JobBillingExRateSysConfig");
		public static readonly ControllerID RefShippingLine = new ControllerID("RefShippingLine");
		public static readonly ControllerID RefFacility = new ControllerID("RefFacility");
		public static readonly ControllerID RefComplianceList = new ControllerID("RefComplianceList");
		public static readonly ControllerID RefComplianceCommodityAlert = new ControllerID("RefComplianceCommodityAlert");

		public static readonly ControllerID ExchangeRate = new ControllerID("ExchangeRate");

		public static readonly ControllerID StaffCredentialsPlugIn = new ControllerID("StaffCredentialsPlugIn");
		public static readonly ControllerID StaffNumberRangesPlugIn = new ControllerID("StaffNumberRangesPlugIn");

		public static readonly ControllerID AccGeneralLedgerData = new ControllerID("AccGeneralLedgerData");

		// Agency
		public static readonly ControllerID AgencyBooking = new ControllerID("AgencyBooking");
		public static readonly ControllerID AgencyBillOfLading = new ControllerID("AgencyBillOfLading");
		public static readonly ControllerID AgencyBillContainers = new ControllerID("AgencyBillContainers");
		public static readonly ControllerID AgencyContainerDetention = new ControllerID("AgencyContainerDetention");
		public static readonly ControllerID AgencyContainerManager = new ControllerID("AgencyContainerManager");
		public static readonly ControllerID AgencyContainerMove = new ControllerID("AgencyContainerMove");
		public static readonly ControllerID AgencyVoyageAccounting = new ControllerID("AgencyVoyageAccounting");
		public static readonly ControllerID AgencySundryCharges = new ControllerID("AgencySundryCharges");
		public static readonly ControllerID AgencyAllocation = new ControllerID("AgencyAllocation");
		public static readonly ControllerID AgencyPortMessaging = new ControllerID("AgencyPortMessaging");
		public static readonly ControllerID AgencyNZPortMessaging = new ControllerID("AgencyNZPortMessaging");
		public static readonly ControllerID AgencyDangerousGoodsManifest = new ControllerID("AgencyDangerousGoodsManifest");

		// Ocean Carrier
		public static readonly ControllerID CarrierShipmentHeader = new ControllerID("CarrierShipmentHeader");

		// Barcode Parsing
		public static readonly ControllerID BarcodeParsing = new ControllerID("BarcodeParsing");

		// Barcode Validation
		public static readonly ControllerID BarcodeValidation = new ControllerID("BarcodeValidation");

		// Carrier Messaging Buss
		public static readonly ControllerID RefAccessorial = new ControllerID("RefAccessorial");
		public static readonly ControllerID RefMessagingBussCarrierInfo = new ControllerID("RefMessagingBussCarrierInfo");

		// Equipment Combination
		public static readonly ControllerID RefJobEquipment = new ControllerID("RefJobEquipment");

		// domestic transport booking
		public static readonly ControllerID DtbBooking = new ControllerID("DtbBooking");
		public static readonly ControllerID DtbBookingTabPlugIn = new ControllerID("DtbBookingTabPlugIn");
		public static readonly ControllerID DtbBookingConsolidation = new ControllerID("DtbBookingConsolidation");
		public static readonly ControllerID DtbBookingTmpl = new ControllerID("DtbBookingTmpl");
		public static readonly ControllerID MasterBookingPackingPlugIn = new ControllerID("MasterBookingPackingPlugIn");

		// land transport consignment
		public static readonly ControllerID DtbConsignment = new ControllerID("DtbConsignment");

		// domestic consignment
		public static readonly ControllerID DtbBookingConsignment = new ControllerID("DtbBookingConsignment");
		public static readonly ControllerID DtbConsignmentRunSheet = new ControllerID("DtbConsignmentRunSheet");
		public static readonly ControllerID DtbRoutePlanner = new ControllerID("DtbRoutePlanner");

		// Product Warehouse
		public static readonly ControllerID WhsReceive = new ControllerID("WhsReceive");
		public static readonly ControllerID WhsOrder = new ControllerID("WhsOrder");
		public static readonly ControllerID WhsOrderLine = new ControllerID("WhsOrderLine");
		public static readonly ControllerID WhsWorkOrder = new ControllerID("WhsWorkOrder");
		public static readonly ControllerID WhsDynamicWorkOrder = new ControllerID("WhsDynamicWorkOrder");
		public static readonly ControllerID WhsWorkOrderLine = new ControllerID("WhsWorkOrderLine");
		public static readonly ControllerID WhsPicking = new ControllerID("WhsPicking");
		public static readonly ControllerID WhsRelease = new ControllerID("WhsRelease");
		public static readonly ControllerID WhsReleasePackageJob = new ControllerID("WhsReleasePackageJob");
		public static readonly ControllerID WhsTransfer = new ControllerID("WhsTransfer");
		public static readonly ControllerID WhsAdjustment = new ControllerID("WhsAdjustment");
		public static readonly ControllerID WhsStocktake = new ControllerID("WhsStocktake");
		public static readonly ControllerID WhsStocktakeLine = new ControllerID("WhsStocktakeLine");
		public static readonly ControllerID WhsHandlingUnit = new ControllerID("WhsHandlingUnit");
		public static readonly ControllerID WhsInventory = new ControllerID("WhsInventory");
		public static readonly ControllerID WhsInventoryHeldCodes = new ControllerID("WhsInventoryHeldCodes");
		public static readonly ControllerID WhsConfigWarehouse = new ControllerID("WhsConfigWarehouse");
		public static readonly ControllerID WhsConfigRow = new ControllerID("WhsConfigRow");
		public static readonly ControllerID WhsConfigArea = new ControllerID("WhsConfigArea");
		public static readonly ControllerID WhsConfigLocation = new ControllerID("WhsConfigLocation");
		public static readonly ControllerID WhsConfigLocationType = new ControllerID("WhsConfigLocationType");
		public static readonly ControllerID WhsConfigOrgReceive = new ControllerID("WhsConfigOrgReceive");
		public static readonly ControllerID WhsConfigOrgPicking = new ControllerID("WhsConfigOrgPicking");
		public static readonly ControllerID WhsConfigDynamicPickFaces = new ControllerID("WhsConfigDynamicPickFaces");
		public static readonly ControllerID WhsConfigPickFaces = new ControllerID("WhsConfigPickFaces");
		public static readonly ControllerID WhsConfigProduct = new ControllerID("WhsConfigProduct");
		public static readonly ControllerID WhsConfigProductStyle = new ControllerID("WhsConfigProductStyle");
		public static readonly ControllerID WhsInvoicing = new ControllerID("WhsInvoicing");
		public static readonly ControllerID WhsVASOrder = new ControllerID("WhsVASOrder");
		public static readonly ControllerID WhsCartonSize = new ControllerID("WhsCartonSize");
		public static readonly ControllerID WhsCartonGroup = new ControllerID("WhsCartonGroup");
		public static readonly ControllerID WhsAdHocServiceJob = new ControllerID("WhsAdHocServiceJob");
		public static readonly ControllerID WhsConfigPutawayGroup = new ControllerID("WhsConfigPutawayGroup");
		public static readonly ControllerID WhsSalesChannel = new ControllerID("WhsSalesChannel");
		public static readonly ControllerID WhsLoad = new ControllerID("WhsLoad");
		public static readonly ControllerID WhsCycleCountWave = new ControllerID("WhsCycleCountWave");

		// Transit Warehouse
		public static readonly ControllerID WhsItemReceiveTransportationUnit = new ControllerID("WhsItemReceiveTransportationUnit");
		public static readonly ControllerID WhsTransitReceiveConsignment = new ControllerID("WhsTransitReceiveConsignment");
		public static readonly ControllerID WhsTransitDispatchConsignment = new ControllerID("WhsTransitDispatchConsignment");
		public static readonly ControllerID WhsItemDispatchTransportationUnit = new ControllerID("WhsItemDispatchTransportationUnit");
		public static readonly ControllerID WhsItemReceiveASN = new ControllerID("WhsItemReceiveASN");
		public static readonly ControllerID TransitHandlingUnit = new ControllerID("TransitHandlingUnit");
		public static readonly ControllerID TransitWarehouseAttachPackages = new ControllerID("TransitWarehouseAttachPackages");
		public static readonly ControllerID TransitWarehouseViewPackages = new ControllerID("TransitWarehouseViewPackages");
		public static readonly ControllerID WhsItemTransferHeader = new ControllerID("WhsItemTransferHeader");
		public static readonly ControllerID WhsItemDispatchLoadList = new ControllerID("WhsItemDispatchLoadList");

		// Resource Strings
		public static readonly ControllerID LocalLanguages = new ControllerID("LocalLanguages");
		public static readonly ControllerID ResourceStrings = new ControllerID("ResourceStrings");
		public static readonly ControllerID TranslationFeedback = new ControllerID("TranslationFeedback");

		//Recruiter
		public static readonly ControllerID HRJobApplicant = new ControllerID("HRJobApplicant");
		public static readonly ControllerID HRJobRole = new ControllerID("HRJobRole");
		public static readonly ControllerID HRJobSkill = new ControllerID("HRJobSkill");
		public static readonly ControllerID HRJobOpenings = new ControllerID("HRJobOpenings");
		public static readonly ControllerID HRGlbCompanyCampaign = new ControllerID("HRGlbCompanyCampaign");
		public static readonly ControllerID HRGlbCompanyCampaignContact = new ControllerID("HRGlbCompanyCampaignContact");
		public static readonly ControllerID HRJobApplication = new ControllerID("HRJobApplication");
		public static readonly ControllerID LearningCentreCampaign = new ControllerID("LearningCentreCampaign");
		public static readonly ControllerID LearningCentreExamPlugIn = new ControllerID("LearningCentreExamPlugIn");
		public static readonly ControllerID LearningCentreScaledTestPlugIn = new ControllerID("LearningCentreScaledTestPlugIn");
		public static readonly ControllerID GlbAccreditation = new ControllerID("GlbAccreditation");
		public static readonly ControllerID GlbAccreditationAttempt = new ControllerID("GlbAccreditationAttempt");
		public static readonly ControllerID GlbAccreditationGroup = new ControllerID("GlbAccreditationGroup");
		public static readonly ControllerID ExamSetting = new ControllerID("ExamSetting");
		public static readonly ControllerID HREmails = new ControllerID("HREmails");
		public static readonly ControllerID CandidateManagement = new ControllerID("CandidateManagement");
		public static readonly ControllerID HRHiringRequest = new ControllerID("HRHiringRequest");
		public static readonly ControllerID HROnBoarding = new ControllerID("HROnBoarding");

		//Process Manager
		public static readonly ControllerID ProcessTasks = new ControllerID("ProcessTasks");
		public static readonly ControllerID ProcessHeader = new ControllerID("ProcessHeader");
		public static readonly ControllerID ProcessHeaderLink = new ControllerID("ProcessHeaderLink");
		public static readonly ControllerID ProcessTaskForJob = new ControllerID("ProcessTaskForJob");
		public static readonly ControllerID ProcessTemplates = new ControllerID("ProcessTemplates");
		public static readonly ControllerID ProcessCompanyLinkRule = new ControllerID("ProcessCompanyLinkRule");
		public static readonly ControllerID ProcessFieldChangeRule = new ControllerID("ProcessFieldChangeRule");
		public static readonly ControllerID BMTagDefinition = new ControllerID("BMTagDefinition");
		public static readonly ControllerID BMTagMagnitude = new ControllerID("BMTagMagnitude");
		public static readonly ControllerID BMTagRule = new ControllerID("BMTagRule");
		public static readonly ControllerID AcceptabilityBand = new ControllerID("AcceptabilityBand");
		public static readonly ControllerID BMFilterRule = new ControllerID("BMFilterRule");
		public static readonly ControllerID Events = new ControllerID("Events");
		public static readonly ControllerID BMSystems = new ControllerID("BMSystems");
		public static readonly ControllerID BMBufferTimespan = new ControllerID("BMBufferTimespan");
		public static readonly ControllerID BMBoard = new ControllerID("BMBoard");
		public static readonly ControllerID VisualBoard = new ControllerID("VisualBoard");
		public static readonly ControllerID BMBoardSlideshow = new ControllerID("BMBoardSlideshow");
		public static readonly ControllerID BMComponent = new ControllerID("BMComponent");
		public static readonly ControllerID ComponentRelationship = new ControllerID("ComponentRelationship");
		public static readonly ControllerID ViewComponentChangeLog = new ControllerID("ViewComponentChangeLog");
		public static readonly ControllerID BMControlCustomisation = new ControllerID("BMControlCustomisation");
		public static readonly ControllerID BMReleaseSequence = new ControllerID("BMReleaseSequence");
		public static readonly ControllerID NetworkDiagram = new ControllerID("NetworkDiagram");
		public static readonly ControllerID WorkQueues = new ControllerID("WorkQueues");
		public static readonly ControllerID CompletionTriggerAction = new ControllerID("CompletionTriggerAction");
		public static readonly ControllerID WorkflowExceptions = new ControllerID("WorkflowExceptions");
		public static readonly ControllerID WorkflowExceptionTypes = new ControllerID("WorkflowExceptionTypes");
		public static readonly ControllerID WorkflowMilestones = new ControllerID("WorkflowMilestones");
		public static readonly ControllerID WorkflowTriggers = new ControllerID("WorkflowTriggers");
		public static readonly ControllerID MENTAgedScoreQuery = new ControllerID("MENTAgedScoreQuery");
		public static readonly ControllerID MENTAgedScoreExtraction = new ControllerID("MENTAgedScoreExtraction");
		public static readonly ControllerID ExternalRequestTypes = new ControllerID("ExternalRequestTypes");
		public static readonly ControllerID ExternalRequestInfoTemplate = new ControllerID("ExternalRequestInfoTemplate");

		// Customs
		public static readonly ControllerID CommercialInvoice = new ControllerID("CommercialInvoice");
		public static readonly ControllerID ImporterSecurityFiling = new ControllerID("ImporterSecurityFiling");

		public static readonly ControllerID LandedCosting = new ControllerID("LandedCosting");
		public static readonly ControllerID LandedCostingHistoryView = new ControllerID("LandedCostingHistoryView");

		public static readonly ControllerID CusPerson = new ControllerID("CusPerson");
		public static readonly ControllerID GroupCredentialsPlugIn = new ControllerID("GroupCredentialsPlugIn");
		public static readonly ControllerID CompanyCredentialsPlugIn = new ControllerID("CompanyCredentialsPlugIn");

		public static readonly ControllerID BorderWiseWebReturnHook = new ControllerID("BorderWiseWebReturnHook");

		public static class Customs
		{
			public static readonly ControllerID EntryHeader = new ControllerID("EntryHeader");
			public static readonly ControllerID SingleTariffClassification = new ControllerID("Classification");
			public static readonly ControllerID ImportClassification = new ControllerID("ImportClassification");
			public static readonly ControllerID ExportClassification = new ControllerID("ExportClassification");
			public static readonly ControllerID DocumentImageSystem = new ControllerID("DocumentImageSystem");
			public static readonly ControllerID TariffBulkChange = new ControllerID("TariffBulkChange");
			public static readonly ControllerID ImportCustomsFilesData = new ControllerID("ImportCustomsFilesData");
			public static readonly ControllerID JobDeclaration = new ControllerID("JobDeclaration");
			public static readonly ControllerID JobDeclarationPluggedIntoShipment = new ControllerID("JobDeclarationPluggedIntoShipment");
			public static readonly ControllerID SGV3JobDeclaration = new ControllerID("SGV3JobDeclaration");
			public static readonly ControllerID RefPacks = new ControllerID("RefPacks");
			public static readonly ControllerID Tariff = new ControllerID("Tariff");
			public static readonly ControllerID SupplierPart = new ControllerID("SupplierPart");
			public static readonly ControllerID CargoManifestPlugInForConsol = new ControllerID("CargoManifestPlugInForConsol");
			public static readonly ControllerID OrgBuyerSupplierLinkAdditionalCustomsDetails = new ControllerID("OrgBuyerSupplierLinkAdditionalCustomsDetails");
			public static readonly ControllerID SendTestCustomsMessage = new ControllerID("SendTestCustomsMessage");
			public static readonly ControllerID Permits = new ControllerID("Permit");
			public static readonly ControllerID CusCalculationRules = new ControllerID("CusCalculationRules");
			public static readonly ControllerID Guarantees = new ControllerID("Guarantees");
			public static readonly ControllerID TemporaryStorage = new ControllerID("TemporaryStorage");
			public static readonly ControllerID BaseAirCargo = new ControllerID("BaseAirCargo");
			public static readonly ControllerID BaseSeaCargo = new ControllerID("BaseSeaCargo");
			public static readonly ControllerID CusAuthorisations = new ControllerID("CusAuthorisations");
			public static readonly ControllerID CusRefPreference = new ControllerID("CusRefPreference");
			public static readonly ControllerID CusRefRateCode = new ControllerID("CusRefRateCode");
			public static readonly ControllerID CusPackingList = new ControllerID("CusPackingList");
			public static readonly ControllerID TradeGroups = new ControllerID("TradeGroups");
			public static readonly ControllerID CusRefTariffVersion = new ControllerID("CusRefTariffVersion");
			public static readonly ControllerID CustomsStatement = new ControllerID("CustomsStatement");
			public static readonly ControllerID ConsolidatedDeclaration = new ControllerID("ConsolidatedDeclaration");
			public static readonly ControllerID CustomsRules = new ControllerID("CustomsRules");
			public static readonly ControllerID GoodsCatalog = new ControllerID("GoodsCatalog");
			public static readonly ControllerID SumARegister = new("SumARegister");
			public static readonly ControllerID SumARegisterReadOnly = new("SumARegisterReadOnly");

			public static class Universal
			{
				public static readonly ControllerID RefCusTariff = new ControllerID("RefCusTariff");
				public static readonly ControllerID ZZRefCarrier = new ControllerID("ZZRefCarrier");
				public static readonly ControllerID ZZRefCusCodeList = new ControllerID("ZZRefCusCodeList");
				public static readonly ControllerID ZZRefCusMap = new ControllerID("ZZRefCusMap");
				public static readonly ControllerID ZZRefCusProcedure = new ControllerID("ZZRefCusProcedure");
				public static readonly ControllerID ZZRefCusRuling = new ControllerID("ZZRefCusRuling");
				public static readonly ControllerID RefHarbourRate = new ControllerID("RefHarbourRate");
				public static readonly ControllerID RefDataGrouping = new ControllerID("RefDataGrouping");
				public static readonly ControllerID RefCusTradeGroup = new ControllerID("RefCusTradeGroup");
			}

			public static class ASYCUDA
			{
				public static readonly ControllerID ASYCUDAManifest = new ControllerID("ASYCUDAManifest");
				public static readonly ControllerID ASYCUDAManifestBill = new ControllerID("ASYCUDAManifestBill");
				public static readonly ControllerID ASYCUDAManifestConsol = new ControllerID("ASYCUDAManifestConsol");
				public static readonly ControllerID ASYCUDAPreBoardingNotification = new("PreBoardingNotification");

				public static class SGAccess
				{
					public static readonly ControllerID Manifest = new ControllerID("SGAccessManifest");
					public static readonly ControllerID ManifestBill = new ControllerID("SGAccessManifestBill");
				}
			}

			public static class AU
			{
				public static readonly ControllerID ImportTariffBulkChange = new ControllerID("AUImportTariffBulkChange");
				public static readonly ControllerID ExportTariffBulkChange = new ControllerID("AUExportTariffBulkChange");
				public static readonly ControllerID AirCargo = new ControllerID("AUCustomsAirCargo");
				public static readonly ControllerID AirCargoDepot = new ControllerID("AirCargoDepotStandAlone");
				public static readonly ControllerID SeaCargo = new ControllerID("AUCustomsSeaCargo");
				public static readonly ControllerID SeaCargoDepot = new ControllerID("AUCustomsSeaCargoDepot");
				public static readonly ControllerID CusSCADepotContainer = new ControllerID("CusSCADepotContainer");
				public static readonly ControllerID CusSCADepotHouse = new ControllerID("CusSCADepotHouse");
				public static readonly ControllerID HouseAirCargo = new ControllerID("AUCustomsHouseAirCargo");
				public static readonly ControllerID ExportCustomsManifest = new ControllerID("AUExportCustomsManifest");
				public static readonly ControllerID AirCargoShipmentController = new ControllerID("AirCargoShipmentController");
				public static readonly ControllerID AirCargoConsolController = new ControllerID("AirCargoConsolController");
				public static readonly ControllerID CusHAWBCusUnderbondPluginController = new ControllerID("CusHAWBCusUnderbondPluginController");
				public static readonly ControllerID AirCTOImport = new ControllerID("AUCustomsAirCTOImportController");
				public static readonly ControllerID AirCTOHawbImport = new ControllerID("AUCustomsAirCTOHawbImportController");
				public static readonly ControllerID AirCTOExport = new ControllerID("AUCustomsAirCTOExportController");
				public static readonly ControllerID AirCTOHawbExport = new ControllerID("AUCustomsAirCTOHawbExportController");
				public static readonly ControllerID CusSCAHouseCusUnderbondPluginController = new ControllerID("CusSCAHouseCusUnderbondPluginController");
				public static readonly ControllerID VoyageManifest = new ControllerID("VoyageManifest");
				public static readonly ControllerID CusSeaManOBLDetailCusUnderbondPluginController = new ControllerID("CusSeaManOBLDetailCusUnderbondPluginController");
				public static readonly ControllerID AirCargoCusUnderbondPluginController = new ControllerID("AirCargoCusUnderbondPluginController");
				public static readonly ControllerID AirCTOCusUnderbondPluginController = new ControllerID("AirCTOCusUnderbondPluginController");
				public static readonly ControllerID AirCargoDeclarationCusUnderbondController = new ControllerID("AirCargoDeclarationCusUnderbondController");
				public static readonly ControllerID AirCargoOutturnBillsController = new ControllerID("AirCargoOutturnBillsController");
				public static readonly ControllerID ArrivalPluginToSailingController = new ControllerID("ArrivalPluginToSailingController");
				public static readonly ControllerID CusSCAContainerUnderbondPluginController = new ControllerID("CusSCAContainerUnderbondPluginController");
				public static readonly ControllerID SeaCargoStandAloneController = new ControllerID("SeaCargoStandAloneController");
				public static readonly ControllerID SeaCargoDepotStandAloneController = new ControllerID("SeaCargoDepotStandAloneController");
				public static readonly ControllerID SeaCargoHouseController = new ControllerID("AUCustomsSeaCargoHouseController");
				public static readonly ControllerID SeaCargoHouseShipmentController = new ControllerID("AUCustomsSeaCargoHouseShipmentController");
				public static readonly ControllerID SeaCargoOutturnBillsController = new ControllerID("SeaCargoOutturnBillsController");
				public static readonly ControllerID AQISProducerCode = new ControllerID("AQISProducerCode");
				public static readonly ControllerID Premises = new ControllerID("Premises");
				public static readonly ControllerID CMRCodeLists = new ControllerID("CMRCodeLists");
				public static readonly ControllerID InstrumentNumber = new ControllerID("InstrumentNumber");
				public static readonly ControllerID CMRLodgementQuestion = new ControllerID("CMRLodgementQuestion");
				public static readonly ControllerID ManifestPluginToSailingController = new ControllerID("ManifestPluginToSailingController");
				public static readonly ControllerID OrganisationCustomsMessaging = new ControllerID("AUOrganisationCustomsMessaging");
				public static readonly ControllerID CMREstablishmentCodes = new ControllerID("CMREstablishmentCodes");
				public static readonly ControllerID NEXDOCNotificationController = new ControllerID("NEXDOCNotificationController");
			}

			public static class BR
			{
				public static readonly ControllerID OrganisationConsigneePlugIn = new ControllerID("BROrganisationConsigneePlugIn");
				public static readonly ControllerID LPCO = new ControllerID("BRLPCO");
				public static readonly ControllerID LPCODeclaration = new ControllerID("BRLPCODeclaration");
				public static readonly ControllerID LPCOEntryHeader = new ControllerID("BRLPCOEntryHeader");
				public static readonly ControllerID License = new ControllerID("BRLicense");
				public static readonly ControllerID LicenseEntryHeader = new ControllerID("BRLicenseEntryHeader");
				public static readonly ControllerID OrganisationDetailsPlugIn = new ControllerID("BROrganisationDetailPlugIn");
				public static readonly ControllerID ForeignOperator = new ControllerID("BRForeignOperator");
			}

			public static class EU
			{
				public static readonly ControllerID EMCS = new ControllerID("EMCS");
				public static readonly ControllerID EntryHeaderController = new ControllerID("EUEntryHeaderController");
				public static readonly ControllerID NctsMovementController = new ControllerID("NctsMovementController");
				public static readonly ControllerID ExitSummaryController = new ControllerID("ExitSummaryController");
				public static readonly ControllerID NctsMovementInShipmentController = new ControllerID("NctsMovementInShipmentController");
				public static readonly ControllerID NctsMovementInConsolController = new ControllerID("NctsMovementInConsolController");
				public static readonly ControllerID NctsOrganisationDetailsPlugIn = new ControllerID("NctsOrganisationDetailsPlugIn");
				public static readonly ControllerID OrganisationConsigneePlugIn = new ControllerID("EUOrganisationConsigneePlugIn");
				public static readonly ControllerID ExitControl = new ControllerID("ExitControl");
				public static readonly ControllerID ExitControlReport = new ControllerID("ExitControlReport");
				public static readonly ControllerID EUH7 = new ControllerID("EUH7");
				public static readonly ControllerID EUH7Bill = new ControllerID("EUH7Bill");
				public static readonly ControllerID UCC6TemporaryStorage = new ControllerID("UCC6TemporaryStorage");
				public static readonly ControllerID IntrastatReports = new ControllerID("IntrastatReports");
				public static readonly ControllerID IntrastatTransactionsController = new ControllerID("IntrastatTransactionsController");
				public static readonly ControllerID OrganisationConsignorPlugIn = new ControllerID("EUOrganisationConsignorPlugIn");
				public static readonly ControllerID TempStorageRegister = new ControllerID("TempStorageRegister");
				public static readonly ControllerID TempStoragePremises = new ControllerID("TempStoragePremises");
			}

			public static class PL
			{
				public static readonly ControllerID AuthorisationRule = new ControllerID("AuthorisationRule");
			}

			public static class IN
			{
				public static readonly ControllerID OrganisationConsignorPlugIn = new ControllerID("INOrganisationConsignorPlugIn");
				public static readonly ControllerID OrganisationConsigneePlugIn = new ControllerID("INOrganisationConsigneePlugIn");
				public static readonly ControllerID OrganisationDetailsPlugIn = new ControllerID("INOrganisationDetailsPlugIn");
			}

			public static class GB
			{
				public static readonly ControllerID OrganisationConsigneePlugIn = new ControllerID("GBOrganisationConsigneePlugIn");
				public static readonly ControllerID CcsukGenralMessage = new ControllerID("CcsukGenralMessage");
				public static readonly ControllerID CcsukAirInventory = new ControllerID("CcsukAirInventory");
				public static readonly ControllerID CcsukAirInventoryUFO = new ControllerID("CcsukAirInventoryUFO");
				public static readonly ControllerID CcsukAirInventoryInConsol = new ControllerID("CcsukAirInventoryInConsol");
				public static readonly ControllerID CcsukAirInventoryHouse = new ControllerID("CcsukAirInventoryHouse");
				public static readonly ControllerID CcsukAirInventoryHouseInShipment = new ControllerID("CcsukAirInventoryHouseInShipment");
				public static readonly ControllerID CcsukMasterAndHouseCombined = new ControllerID("CcsukMasterAndHouseCombined");
				public static readonly ControllerID CcsukStandAloneFsrEnquiry = new ControllerID("CcsukStandAloneFsrEnquiry");
				public static readonly ControllerID DLUController = new ControllerID("DLUController");
				public static readonly ControllerID CcsukSplitHouseController = new ControllerID("CcsukSplitHouseController");
				public static readonly ControllerID CcsukSplitBasicController = new ControllerID("CcsukSplitBasicController");
				public static readonly ControllerID ChiefExportConsolIntegrationController = new ControllerID("ChiefExportConsolIntegrationController");
				public static readonly ControllerID CDSDISQueryController = new ControllerID("CDSDISQueryController");
				public static readonly ControllerID CDSCashPaymentsController = new ControllerID(nameof(CDSCashPaymentsController));
				public static readonly ControllerID EntryHeader = new ControllerID("GBEntryHeader");
				public static readonly ControllerID H7Bill = new ControllerID("GBH7Bill");
				public static readonly ControllerID OrganisationCustomsMessaging = new ControllerID("GBOrganisationController");
			}

			public static class ES
			{
				public static readonly ControllerID OrganisationConsigneePlugIn = new ControllerID("ESOrganisationConsigneePlugIn");
				public static readonly ControllerID TemporaryStorageRegister = new ControllerID("ESTempStorageRegister");
				public static readonly ControllerID G3Declaration = new ControllerID("G3Declaration");
			}

			public static class FR
			{
				public static readonly ControllerID OrganisationConsigneePlugIn = new ControllerID("FROrganisationConsigneePlugIn");
				public static readonly ControllerID CINExportConsolIntegrationController = new ControllerID("CINExportConsolIntegrationController");
				public static readonly ControllerID CINTemporaryStorageConsolController = new ControllerID("CINTemporaryStorageConsolController");
				public static readonly ControllerID OrganisationDetailsPlugIn = new ControllerID("FROrganisationDetailsController");
				public static readonly ControllerID EntryHeader = new ControllerID("FREntryHeader");
			}

			public static class CA
			{
				public static readonly ControllerID HTSTariffBulkChange = new ControllerID("CAHTSTariffBulkChange");
				public static readonly ControllerID CAShipmentCargoReport = new ControllerID("CAShipmentCargoReport");
				public static readonly ControllerID CAConsolACI = new ControllerID("CAConsolACI");
				public static readonly ControllerID CAQueryMessages = new ControllerID("CAQueryMessages");
				public static readonly ControllerID K84Reports = new ControllerID("K84Reports");
				public static readonly ControllerID CAReleaseNotifications = new ControllerID("CAReleaseNotifications");
				public static readonly ControllerID CACusClassification = new ControllerID("CACusClassification");
				public static readonly ControllerID RNSShipmentPlugIn = new ControllerID("RNSShipmentPlugIn");
				public static readonly ControllerID RNSConsolPlugIn = new ControllerID("RNSConsolPlugIn");
				public static readonly ControllerID OrganisationConsigneePlugIn = new ControllerID("OrganisationConsigneePlugIn");
				public static readonly ControllerID OrganisationDetailsPlugIn = new ControllerID("OrganisationDetailsPlugIn");
				public static readonly ControllerID CATransactionNumberSetting = new ControllerID("CATransactionNumberSetting");
				public static readonly ControllerID B2Adjustments = new ControllerID("B2Adjustments");
				public static readonly ControllerID CAHouseBilleManifest = new ControllerID("CAHouseBilleManifest");
				public static readonly ControllerID CAConsoleManifest = new ControllerID("CAConsoleManifest");
				public static readonly ControllerID CAManifestForward = new ControllerID("CAManifestForward");
				public static readonly ControllerID RNSCFSShipmentPlugIn = new ControllerID("RNSCFSShipmentPlugIn");
				public static readonly ControllerID RNSMFLoadListPlugIn = new ControllerID("RNSMFLoadListPlugIn");
				public static readonly ControllerID RNSMFTallyPlugIn = new ControllerID("RNSMFTallyPlugIn");
				public static readonly ControllerID RNSMFTallyShipmentsPlugIn = new ControllerID("RNSMFTallyShipmentsPlugIn");
				public static readonly ControllerID CALVXJobs = new ControllerID("CALVXJobs");
				public static readonly ControllerID OrganisationCustomsMessaging = new ControllerID("OrganisationCustomsMessaging");
				public static readonly ControllerID CusSCAOceanBill = new ControllerID("CusSCAOceanBill");
				public static readonly ControllerID CAJobDocAddresses = new ControllerID("CAJobDocAddresses");
				public static readonly ControllerID CADailyNoticeReconciliation = new ControllerID("CADailyNoticeReconciliation");
				public static readonly ControllerID CACSARevenueSummaryForm = new ControllerID("CACSARevenueSummaryForm");
			}

			public static class CN
			{
				public static readonly ControllerID OrgBuyerSupplierLinkChinaCustomsDetails = new ControllerID("CNOrgBuyerSupplierLinkChinaCustomsDetails");
				public static readonly ControllerID OrganisationDetailsPlugIn = new ControllerID("CNOrganisationDetailsPlugIn");
				public static readonly ControllerID OrgSupBuyLinkTrnModeAdditionalCustomsDetails = new ControllerID("CNOrgSupBuyLinkTrnModeAdditionalCustomsDetails");
				public static readonly ControllerID EntryHeader = new ControllerID("CNEntryHeader");
			}

			public static class HK
			{
				public static readonly ControllerID Traxon = new ControllerID("HKCustomsTraxon");
			}

			public static class NZ
			{
				public static readonly ControllerID CUSCAR = new ControllerID("NZCUSCAR");
				public static readonly ControllerID CUSCARPluggedIntoShipment = new ControllerID("CUSCARPluggedIntoShipment");
				public static readonly ControllerID ECIWriteOffManifesting = new ControllerID("ECIWriteOffManifesting");
				public static readonly ControllerID MAFeBACCaDeclarationPlugin = new ControllerID("MAFeBACCaDeclarationPlugin");
				public static readonly ControllerID MAFeBACCaConsolPlugIn = new ControllerID("MAFeBACCaConsolPlugIn");
				public static readonly ControllerID MAFeBACCaContainerPlugin = new ControllerID("MAFeBACCaContainerPlugin");
				public static readonly ControllerID MAFeBACCaInvoiceLinePlugin = new ControllerID("MAFeBACCaInvoiceLinePlugin");
				public static readonly ControllerID ExpressECI = new ControllerID("ExpressECI");
				public static readonly ControllerID Concession = new ControllerID("Concession");
				public static readonly ControllerID Supplier = new ControllerID("Supplier");
				public static readonly ControllerID OutwardReport = new ControllerID("OutwardReport");
				public static readonly ControllerID ECIWriteOffManifestingConsolSynchroniser = new ControllerID("ECIWriteOffManifestingConsolSynchroniser");
				public static readonly ControllerID ConsolToExpressECIConverter = new ControllerID("ConsolToExpressECIConverter");
				public static readonly ControllerID ShipmentToExpressECIConverter = new ControllerID("ShipmentToExpressECIConverter");
				public static readonly ControllerID InwardCargoReport = new ControllerID("InwardCargoReport");
				public static readonly ControllerID SeaCargoICR = new ControllerID("SeaCargoICR");
				public static readonly ControllerID ConsolToSeaCargoWriteOffConverter = new ControllerID("ConsolToSeaCargoWriteOffConverter");
				public static readonly ControllerID ShipmentToSeaCargoWriteOffConverter = new ControllerID("ShipmentToSeaCargoWriteOffConverter");
			}

			public static class SG
			{
				public static readonly ControllerID SG4Classification = new ControllerID("SG4Classification");
				public static readonly ControllerID CMDShipment = new ControllerID("CMDShipment");
				public static readonly ControllerID CMDConsol = new ControllerID("CMDConsol");
			}

			public static class US
			{
				public static readonly ControllerID OrganisationCustomsMessaging = new ControllerID("USOrganisationCustomsMessaging");
				public static readonly ControllerID OrganisationDetailsPlugIn = new ControllerID("USOrganisationDetailsPlugIn");
				public static readonly ControllerID AMSBrokerDownloadMessages = new ControllerID("USAMSBrokerDownloadMessages");
				public static readonly ControllerID CourtesyNoticesOfLiquidation = new ControllerID("USCourtesyNoticesOfLiquidation");
				public static readonly ControllerID BorderLineReleaseMessage = new ControllerID("USBorderLineReleaseMessages");
				public static readonly ControllerID QueryMessages = new ControllerID("USQueryMessages");
				public static readonly ControllerID InBondNumber = new ControllerID("InBondNumber");
				public static readonly ControllerID USCRule = new ControllerID("USCRule");
				public static readonly ControllerID USCTariffRule = new ControllerID("USCTariffRule");
				public static readonly ControllerID USTariffBulkChange = new ControllerID("USTariffBulkChange");
				public static readonly ControllerID Protest = new ControllerID("Protest");
				public static readonly ControllerID Recon = new ControllerID("USRecon");
				public static readonly ControllerID Drawback = new ControllerID("USDrawback");
				public static readonly ControllerID USCAntiDumpingCase = new ControllerID("USCAntiDumpingCase");
				public static readonly ControllerID USCACCase = new ControllerID("USCACCase");
				public static readonly ControllerID USCarrierCombined = new ControllerID("USCarrierCombined");
				public static readonly ControllerID USCCountry = new ControllerID("USCCountry");
				public static readonly ControllerID InBond = new ControllerID("USInBond");
				public static readonly ControllerID InBondMoveHeader = new ControllerID("USInBondMoveHeader");
				public static readonly ControllerID InBondPluggedIntoDeclaration = new ControllerID("InBondPluggedIntoDeclaration");
				public static readonly ControllerID InBondPluggedIntoShipment = new ControllerID("USInBondPluggedIntoShipment");
				public static readonly ControllerID InBondPluggedIntoConsol = new ControllerID("USInBondPluggedIntoConsol");
				public static readonly ControllerID AMS = new ControllerID("USAMS");
				public static readonly ControllerID AMSPluggedIntoConsol = new ControllerID("AMSPluggedIntoConsol");
				public static readonly ControllerID AMSBill = new ControllerID("USAMSBill");
				public static readonly ControllerID StowPlan = new ControllerID("StowPlan");
				public static readonly ControllerID OrgBuyerSupplierLinkAdditionalCustomsDetails = new ControllerID("USOrgBuyerSupplierLinkAdditionalCustomsDetails");
				public static readonly ControllerID eManifest = new ControllerID("eManifest");
				public static readonly ControllerID eManifestShipment = new ControllerID("eManifestShipment");
				public static readonly ControllerID ContainerAdditionalReferenceNumbers = new ControllerID("ContainerAdditionalReferenceNumbers");
				public static readonly ControllerID ThreeLetterRefAirline = new ControllerID("ThreeLetterRefAirline");
				public static readonly ControllerID USLowValueEntries = new ControllerID("USLowValueEntries");
				public static readonly ControllerID USLowValueEntriesBill = new ControllerID("USLowValueEntriesBill");
				public static readonly ControllerID USLowValueEntriesDeclaration = new ControllerID("USLowValueEntriesDeclaration");
			}

			public static class TW
			{
				public static readonly ControllerID Transhipment = new ControllerID("TWTranshipment");
				public static readonly ControllerID OrganisationDetailsPlugIn = new ControllerID("TWOrganisationDetailsPlugIn");
				public static readonly ControllerID SpecailCode = new ControllerID("TWSpecialCode");
				public static readonly ControllerID BriefCustomsDeclarations = new ControllerID("TWBriefCustomsDeclarations");
			}

			public static class JP
			{
				public static readonly ControllerID AFR = new ControllerID("JPAFR");
				public static readonly ControllerID AFRPluggedIntoConsol = new ControllerID("AFRPluggedIntoConsol");
				public static readonly ControllerID AFRBill = new ControllerID("JPAFRBill");
			}

			public static class ZA
			{
				public static readonly ControllerID OrganisationDetailsPlugIn = new ControllerID("ZAOrganisationDetailsPlugIn");
				public static readonly ControllerID CALINFMessagingPlugin = new ControllerID("CALINFMessagingPlugin");
			}

			public static class DE
			{
				public static readonly ControllerID SumARegister = new ControllerID("DESumARegister");
				public static readonly ControllerID SumARegisterReadOnly = new ControllerID("DESumARegisterReadOnly");
				public static readonly ControllerID ExportStatusRequest = new ControllerID("DEExportStatusRequest");
				public static readonly ControllerID MonthlyClosing = new ControllerID("DEMonthlyClosing");
				public static readonly ControllerID OrganisationConsigneePlugIn = new ControllerID("DEOrganisationConsigneePlugIn");
				public static readonly ControllerID OrganisationDetailsPlugIn = new ControllerID("DEOrganisationDetailsPlugIn");
				public static readonly ControllerID TaxChangeAssessment = new ControllerID("DETaxChangeAssessment");
			}

			public static class TR
			{
				public static readonly ControllerID ETrade = new ControllerID("TR.ETrade");
				public static readonly ControllerID SimplifiedProcedureTransitSystem = new ControllerID("TRSimplifiedProcedureTransitSystem");
			}

			public static class NO
			{
				public static readonly ControllerID TemporaryStorageRegister = new("NOSumARegister");
				public static readonly ControllerID TemporaryStorageRegisterReadOnly = new("NOSumARegisterReadOnly");
			}

			public static class NL
			{
				public static readonly ControllerID OrganisationConsigneePlugIn = new ControllerID("NLOrganisationConsigneePlugIn");
				public static readonly ControllerID EntryHeader = new ControllerID("NLEntryHeader");
			}

			public static class KR
			{
				public static readonly ControllerID MiscRequestMessages = new ControllerID("KRMiscRequestMessages");
				public static readonly ControllerID ExportEntryDetails = new ControllerID("KRExportEntryDetails");
				public static readonly ControllerID ImportEntryDetails = new ControllerID("KRImportEntryDetails");
				public static readonly ControllerID EntryDetailsFor5SG = new ControllerID("KREntryDetailsFor5SG");
				public static readonly ControllerID EntryLineDetailsFor5UL = new ControllerID("KREntryLineDetailsFor5UL");
				public static readonly ControllerID EntryCustomsBillsFor5UL = new ControllerID("KREntryCustomsBillsFor5UL");
				public static readonly ControllerID OrganisationDetailsPlugIn = new ControllerID("KROrganisationController");
				public static readonly ControllerID DocumentListMessages = new ControllerID("KRDocumentListMessages");
				public static readonly ControllerID CusReconDeclaration = new ControllerID("KRCusReconDeclaration");
			}

			public static class IT
			{
				public static readonly ControllerID EntryHeaderController = new ControllerID("ITEntryHeaderController");
			}

			public static class CO
			{
				public static readonly ControllerID DocumentIDs = new ControllerID("CO.DocumentIDs");
			}

			public static class IL
			{
				public static readonly ControllerID CustomsMessaging = new ControllerID("IL.CustomsMessaging");
			}

			public static class IE
			{
				public static readonly ControllerID CustomsAndExciseReports = new ControllerID("CustomsAndExciseReports");
			}

			public static class CH
			{
				public static readonly ControllerID EntryHeader = new ControllerID("CHEntryHeader");
				public static readonly ControllerID CustomsSummary = new ControllerID("CHCustomsSummary");
				public static readonly ControllerID DeclarationActivation = new ControllerID("CHDeclarationActivation");
			}
		}

		public static class Messaging
		{
			public static readonly ControllerID EDICommunicationsMode = new ControllerID("EDICommunicationsMode");
			public static readonly ControllerID EDIInterchange = new ControllerID("EDIInterchange");
			public static readonly ControllerID EDIMessage = new ControllerID("EDIMessage");
			public static readonly ControllerID EDICommunicationParty = new ControllerID("EDICommunicationParty");
			public static readonly ControllerID EDIMessagePurpose = new ControllerID("EDIMessagePurpose");
			public static readonly ControllerID EDIMessageContentFilter = new ControllerID("EDIMessageContentFilter");
			public static readonly ControllerID EDIMessageDeliveryContext = new ControllerID("EDIMessageDeliveryContext");
			public static readonly ControllerID EDICodeMapping = new ControllerID("EDICodeMapping");
			public static readonly ControllerID UniversalValidationRule = new ControllerID("UniversalValidationRule");
		}

		// Shipnet
		public static readonly ControllerID ShipnetSetupPlugIn = new ControllerID("ShipnetSetupPlugIn");

		// PortMessaging
		public static readonly ControllerID PortMessaging = new ControllerID("PortMessaging");
		public static readonly ControllerID ETerminalReleaseManifestPortMessaging = new ControllerID("ETerminalReleaseManifestPortMessaging");

		// UniversalDataCarrierMessaging
		public static readonly ControllerID UniversalDataCarrierMessaging = new ControllerID("UniversalDataCarrierMessaging");

		// Campaign Management
		public static readonly ControllerID GlbCompanyCampaign = new ControllerID("GlbCompanyCampaign");
		public static readonly ControllerID GlbCompanyCampaignItem = new ControllerID("GlbCompanyCampaignItem");
		public static readonly ControllerID GlbCompanyCampaignItemSchedule = new ControllerID("GlbCompanyCampaignItemSchedule");
		public static readonly ControllerID VoteCampaignPlugIn = new ControllerID("VoteCampaignPlugIn");
		public static readonly ControllerID SurveyCampaignPlugIn = new ControllerID("SurveyCampaignPlugIn");

		// CargoIMP Phase 2
		public static readonly ControllerID CargoIMPPhase2 = new ControllerID("CargoIMPPhase2");

		// EXREL -- Export Consignment Release Advice
		public static readonly ControllerID ExportConsignmentReleaseAdvice = new ControllerID("ExportConsignmentReleaseAdvice");

		public static readonly ControllerID ActiveUsers = new ControllerID("ActiveUsers");

		// Packing
		public static readonly ControllerID Packing = new ControllerID("Packing");
		public static readonly ControllerID PackingPlugIn = new ControllerID("PackingPlugIn");
		public static readonly ControllerID PalletTransaction = new ControllerID("PalletTransaction");

		// eTail
		public static readonly ControllerID ETailShipment = new ControllerID("ETailShipment");

		// Sales Dashboard
		public static readonly ControllerID SalesDashboard = new ControllerID("SalesDashboard");

		// Process Management
		public static readonly ControllerID WorkItem = new ControllerID("WorkItem");
		public static readonly ControllerID Project = new ControllerID("Project");
		public static readonly ControllerID CustomerServiceTicket = new ControllerID("CustomerServiceTicket");

		public static readonly ControllerID UniversalCopySchedule = new ControllerID("UniversalCopySchedule");

		//Business Intelligence and Analytics
		public static readonly ControllerID PowerBiAnalyticsReports = new ControllerID("PowerBiAnalyticsReports");
		public static readonly ControllerID PowerBiAuditReports = new ControllerID("PowerBiAuditReports");
		public static readonly ControllerID BiManager = new ControllerID("BiManager");
		public static readonly ControllerID Audit = new ControllerID("Audit");

		// Container Yard
		public static readonly ControllerID CYDAdHocServiceOrder = new ControllerID("CYDAdHocServiceOrder");
		public static readonly ControllerID CYDDeliveryHeader = new ControllerID("CYDDeliveryHeader");
		public static readonly ControllerID CYDReceiveAdvice = new ControllerID("CYDReceiveAdvice");
		public static readonly ControllerID CYDReleaseAdvice = new ControllerID("CYDReleaseAdvice");
		public static readonly ControllerID CYDPickupHeader = new ControllerID("CYDPickupHeader");
		public static readonly ControllerID CYDTransportationUnit = new ControllerID("CYDTransportationUnit");
		public static readonly ControllerID CYDYardUnitState = new ControllerID("CYDYardUnitState");
		public static readonly ControllerID MNRWorkOrder = new ControllerID("MNRWorkOrder");
		public static readonly ControllerID MNRSurvey = new ControllerID("MNRSurvey");
		public static readonly ControllerID CYDPeriodicInvoicing = new ControllerID("CYDPeriodicInvoicing");

		// Gate management
		public static readonly ControllerID GteBooking = new ControllerID("GteBooking");
		public static readonly ControllerID GteGateMovementBooking = new ControllerID("GteGateMovementBooking");
		public static readonly ControllerID GteGateMovement = new ControllerID("GteGateMovement");
		public static readonly ControllerID GteVehicleMovement = new ControllerID("GteVehicleMovement");

		//Container Load List
		public static readonly ControllerID ContainerLoadList = new ControllerID("ConatinerLoadList");

		//Container Load Plan
		public static readonly ControllerID ContainerLoadPlan = new ControllerID("ContainerLoadPlan");

		//Carbon Emissions
		public static readonly ControllerID CO2ePlugin = new ControllerID("CO2ePlugin");

		//Electronic Bill Of Lading
		public static readonly ControllerID ElectronicBOL = new ControllerID("ElectronicBOLPlugIn");

		// Dangerous Goods
		public static readonly ControllerID DangerousGoodsPlugin = new ControllerID("DangerousGoodsPlugin");

		#endregion
	}

	#endregion

	#region List

	public class ControllerList : RegistrationList<ControllerID, ControllerInfo>
	{
		public ControllerList()
		{
			Add(new ControllerInfo(ControllerIDs.GenCustomAddOnRule, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.GenCustomAddOnRuleController"));
			Add(new ControllerInfo(ControllerIDs.Orders, "Enterprise.Freight.Forwarding.Module", "Enterprise.Freight.Forwarding.Orders.Module.OrdersController"));
			Add(new ControllerInfo(ControllerIDs.JobShipmentPreplanning, "Enterprise.Freight.Forwarding.Module", "Enterprise.Freight.Forwarding.Orders.Module.JobShipmentPreplanningController"));
			Add(new ControllerInfo(ControllerIDs.Routing, "Enterprise.Freight.Module", "Enterprise.Freight.Module.RoutingController"));
			Add(new ControllerInfo(ControllerIDs.ClientRates, "Enterprise.Rating.Module", "Enterprise.Rating.Module.ClientRatesController"));
			Add(new ControllerInfo(ControllerIDs.ClientRateUpdates, "Enterprise.Rating.Module", "Enterprise.Rating.Module.UpdateRatesController"));
			Add(new ControllerInfo(ControllerIDs.BulkRateUpdates, "Enterprise.Rating.Module", "Enterprise.Rating.Module.BulkRateUpdatesController"));
			Add(new ControllerInfo(ControllerIDs.GlobalRates, "Enterprise.Rating.Module", "Enterprise.Rating.Module.CompanyTariffsController"));
			Add(new ControllerInfo(ControllerIDs.Quotations, "Enterprise.Rating.Module", "Enterprise.Rating.Module.QuotationsController"));
			Add(new ControllerInfo(ControllerIDs.Costing, "Enterprise.Rating.Module", "Enterprise.Rating.Module.CostingController"));
			Add(new ControllerInfo(ControllerIDs.IntercompanyTariffs, "Enterprise.Rating.Module", "Enterprise.Rating.Module.IntercompanyTariffController"));
			Add(new ControllerInfo(ControllerIDs.CostsComparer, "Enterprise.Rating.Module", "Enterprise.Rating.Module.CostsComparerController"));
			Add(new ControllerInfo(ControllerIDs.WiseRates, "Enterprise.Rating.Module", "Enterprise.Rating.Module.WiseRatesController"));
			Add(new ControllerInfo(ControllerIDs.Organisation, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.OrganisationController"));
			Add(new ControllerInfo(ControllerIDs.StaffAssignments, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.StaffAssignmentsController"));
			Add(new ControllerInfo(ControllerIDs.ClientIntelligence, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.ClientIntelligenceController"));
			Add(new ControllerInfo(ControllerIDs.CompetitorIntelligence, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.CompetitorIntelligenceController"));
			Add(new ControllerInfo(ControllerIDs.ProfitShare, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.OrgAgentRelationshipController"));
			Add(new ControllerInfo(ControllerIDs.TemporaryOrgRemover, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.TemporaryOrgRemoverController"));
			Add(new ControllerInfo(ControllerIDs.Opportunity, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.OrgOpportunityController"));
			Add(new ControllerInfo(ControllerIDs.CrmOpportunity, "Enterprise.CRM.Module", "Enterprise.CRM.Module.CrmOpportunityController"));
			Add(new ControllerInfo(ControllerIDs.ServiceLevel, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.ServiceLevelController"));
			Add(new ControllerInfo(ControllerIDs.RefVessel, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.RefVesselController"));
			Add(new ControllerInfo(ControllerIDs.RefVesselZZ, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.RefVesselZZController"));
			Add(new ControllerInfo(ControllerIDs.ARAccQueryClaim, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.ARAccQueryClaimController"));
			Add(new ControllerInfo(ControllerIDs.APAccQueryClaim, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.APAccQueryClaimController"));
			Add(new ControllerInfo(ControllerIDs.OrgCollectionCalls, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.OrgCollectionCallsController"));
			Add(new ControllerInfo(ControllerIDs.ExporterScheme, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.ExporterSchemeController"));
			Add(new ControllerInfo(ControllerIDs.ExporterScheme, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.ExporterSchemeControllerUK", Constants.CountryCodes.UnitedKingdom));

			foreach (var countryCode in new string[] { Constants.CountryCodes.UnitedStates, Constants.CountryCodes.PuertoRico, Constants.CountryCodes.VirginIslands, Constants.CountryCodes.Guam, Constants.CountryCodes.NorthernMarianaIslands, Constants.CountryCodes.AmericanSamoa })
			{
				Add(new ControllerInfo(ControllerIDs.ExporterScheme, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.ExporterSchemeControllerUS", countryCode));
			}

			Add(new ControllerInfo(ControllerIDs.AdministrationPanel, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.AdministrationPanelController"));
			Add(new ControllerInfo(ControllerIDs.GlbPortDeliveryTime, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.GlbPortDeliveryTimeController"));
			Add(new ControllerInfo(ControllerIDs.APPaymentProcessing, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.APPaymentProcessingController"));
			Add(new ControllerInfo(ControllerIDs.ARPaymentProcessing, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.ARPaymentProcessingController"));
			Add(new ControllerInfo(ControllerIDs.APPaymentBatchPosting, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.APPaymentBatchPostingController"));
			Add(new ControllerInfo(ControllerIDs.ARReceiptBatchPosting, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.ARReceiptBatchPostingController"));
			Add(new ControllerInfo(ControllerIDs.ChequeTransactionHeader, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.ChequeTransactionController"));
			Add(new ControllerInfo(ControllerIDs.Cheque, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.ChequeController"));
			Add(new ControllerInfo(ControllerIDs.JobManagement, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.JobManagementController"));
			Add(new ControllerInfo(ControllerIDs.JobInvoicingForm, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.JobInvoicingFormController"));
			Add(new ControllerInfo(ControllerIDs.JobConsolCostingForm, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.JobConsolCostingController"));
			Add(new ControllerInfo(ControllerIDs.BulkJobClose, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.BulkJobCloseController"));
			Add(new ControllerInfo(ControllerIDs.BulkDSBJobCloseBatchApproval, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.BulkDSBJobCloseBatchApprovalController"));
			Add(new ControllerInfo(ControllerIDs.DataExportBatchPlugin, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.DataExportBatchPluginController"));
			Add(new ControllerInfo(ControllerIDs.TransactionsExport, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.TransactionsExportController"));
			Add(new ControllerInfo(ControllerIDs.XmlTransactionsImport, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.XmlTransactionsImportController"));
			Add(new ControllerInfo(ControllerIDs.CsvTransactionsImport, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.CsvTransactionsImportController"));
			Add(new ControllerInfo(ControllerIDs.CsvAccountsImport, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.CsvAccountsChartImportController"));
			Add(new ControllerInfo(ControllerIDs.XmlJournalExport, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.XMLJournalExportController"));
			Add(new ControllerInfo(ControllerIDs.XmlJournalImport, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.XmlJournalImportController"));
			Add(new ControllerInfo(ControllerIDs.WIP, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.WIPController"));
			Add(new ControllerInfo(ControllerIDs.Accrual, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.AccrualController"));
			Add(new ControllerInfo(ControllerIDs.JobRevenueJournal, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.JobRevenueJournalController"));
			Add(new ControllerInfo(ControllerIDs.GLAccountFormat, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.GLAccountFormatController"));
			Add(new ControllerInfo(ControllerIDs.GLJournal, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.GLJournalController"));
			Add(new ControllerInfo(ControllerIDs.GLJournal, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.GLJournalControllerChina", Constants.CountryCodes.China));
			Add(new ControllerInfo(ControllerIDs.GLJournal, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.GLJournalControllerChina", Constants.CountryCodes.Taiwan));
			Add(new ControllerInfo(ControllerIDs.GLJournalLinkedToApproval, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.TransactionApproval.GLJournalLinkedToApprovalController"));
			Add(new ControllerInfo(ControllerIDs.GLJournalApproval, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.TransactionApproval.GLJournalApprovalController"));
			Add(new ControllerInfo(ControllerIDs.GLJournalImport, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.GLJournalControllerImport"));
			Add(new ControllerInfo(ControllerIDs.AlternateChartofAccounts, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.AlternateChartofAccountsController"));
			Add(new ControllerInfo(ControllerIDs.AlternateGLAccounts, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.AlternateGLAccountsController"));
			Add(new ControllerInfo(ControllerIDs.GLConsolidationGroups, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.GLConsolidationGroupController"));
			Add(new ControllerInfo(ControllerIDs.APBulkInvoicePosting, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.APBulkInvoicePostingController"));
			Add(new ControllerInfo(ControllerIDs.TransactionsPendingAllocation, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.Transaction.TransactionsPendingAllocationController"));
			Add(new ControllerInfo(ControllerIDs.MatchEPaymentRecipients, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.MatchEPaymentRecipientsController"));
			Add(new ControllerInfo(ControllerIDs.MatchEPaymentRecipientsBulk, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.MatchEPaymentRecipientsBulkController"));
			Add(new ControllerInfo(ControllerIDs.PeriodicInvoice, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.PeriodicInvoiceController"));
			Add(new ControllerInfo(ControllerIDs.PeriodicInvoiceBulk, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.PeriodicInvoiceBulkController"));
			Add(new ControllerInfo(ControllerIDs.ContainerStoragePayment, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.ContainerStoragePaymentController"));
			Add(new ControllerInfo(ControllerIDs.APInvoice, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.APInvoiceController"));
			Add(new ControllerInfo(ControllerIDs.APInvoiceFromDraftInvoice, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.APInvoiceFromDraftInvoiceController"));
			Add(new ControllerInfo(ControllerIDs.ARInvoice, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.ARInvoiceController"));
			Add(new ControllerInfo(ControllerIDs.ARInvoiceForInterCompanyTransaction, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.ARInvoiceForInterCompanyTransactionController"));
			Add(new ControllerInfo(ControllerIDs.APCreditNote, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.APCreditNoteController"));
			Add(new ControllerInfo(ControllerIDs.APCreditNoteFromDraftInvoice, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.APCreditNoteFromDraftInvoiceController"));
			Add(new ControllerInfo(ControllerIDs.ARCreditNote, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.ARCreditNoteController"));
			Add(new ControllerInfo(ControllerIDs.ARCreditNoteForInterCompanyTransaction, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.ARCreditNoteForInterCompanyTransactionController"));
			Add(new ControllerInfo(ControllerIDs.APAdjustmentNote, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.APAdjustmentNoteController"));
			Add(new ControllerInfo(ControllerIDs.ARAdjustmentNote, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.ARAdjustmentNoteController"));
			Add(new ControllerInfo(ControllerIDs.APJournal, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.APJournalController"));
			Add(new ControllerInfo(ControllerIDs.ARJournal, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.ARJournalController"));
			Add(new ControllerInfo(ControllerIDs.APBankFeeJournal, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.APBankFeeJournalController"));
			Add(new ControllerInfo(ControllerIDs.ARBankFeeJournal, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.ARBankFeeJournalController"));
			Add(new ControllerInfo(ControllerIDs.ARBalancingJournal, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.ARBalancingJournalController"));
			Add(new ControllerInfo(ControllerIDs.APBalancingJournal, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.APBalancingJournalController"));
			Add(new ControllerInfo(ControllerIDs.ZAPPayment, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.ZAPPaymentController"));
			Add(new ControllerInfo(ControllerIDs.ZARPayment, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.ZARPaymentController"));
			Add(new ControllerInfo(ControllerIDs.ZAPReceipt, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.ZAPReceiptController"));
			Add(new ControllerInfo(ControllerIDs.ZARReceipt, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.ZARReceiptController"));
			Add(new ControllerInfo(ControllerIDs.APDiscount, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.APDiscountController"));
			Add(new ControllerInfo(ControllerIDs.ARDiscount, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.ARDiscountController"));
			Add(new ControllerInfo(ControllerIDs.APExchangeDifference, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.APExchangeDifferenceController"));
			Add(new ControllerInfo(ControllerIDs.ARExchangeDifference, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.ARExchangeDifferenceController"));
			Add(new ControllerInfo(ControllerIDs.APOverpayment, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.APOverpaymentController"));
			Add(new ControllerInfo(ControllerIDs.AROverpayment, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.AROverpaymentController"));
			Add(new ControllerInfo(ControllerIDs.DirectPayment, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.DirectPaymentController"));
			Add(new ControllerInfo(ControllerIDs.DirectReceipt, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.DirectReceiptController"));
			Add(new ControllerInfo(ControllerIDs.BankReconDirectPayment, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.BankReconDirectPaymentController"));
			Add(new ControllerInfo(ControllerIDs.BankReconDirectReceipt, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.BankReconDirectReceiptController"));
			Add(new ControllerInfo(ControllerIDs.OpeningPayment, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.OpeningPaymentController"));
			Add(new ControllerInfo(ControllerIDs.OpeningReceipt, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.OpeningReceiptController"));
			Add(new ControllerInfo(ControllerIDs.DepositBatch, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.DepositBatchController"));
			Add(new ControllerInfo(ControllerIDs.BankReconcilliation, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.BankReconcilliationController"));
			Add(new ControllerInfo(ControllerIDs.BankTransfer, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.BankTransferController"));
			Add(new ControllerInfo(ControllerIDs.BankCurrencyAdjustment, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.BankCurrencyAdjustmentController"));
			Add(new ControllerInfo(ControllerIDs.CashBookAPPayment, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.CashBookAPPaymentController"));
			Add(new ControllerInfo(ControllerIDs.CashBookAPReceipt, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.CashBookAPReceiptController"));
			Add(new ControllerInfo(ControllerIDs.CashBookARPayment, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.CashBookARPaymentController"));
			Add(new ControllerInfo(ControllerIDs.CashBookARReceipt, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.CashBookARReceiptController"));
			Add(new ControllerInfo(ControllerIDs.ARComplianceDocument, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.ARComplianceDocumentController"));
			Add(new ControllerInfo(ControllerIDs.APComplianceDocument, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.APComplianceDocumentController"));
			Add(new ControllerInfo(ControllerIDs.PaymentBatch, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.PaymentBatchController"));
			Add(new ControllerInfo(ControllerIDs.Containers, "Enterprise.Freight.Module", "Enterprise.Freight.Module.ContainersController"));
			Add(new ControllerInfo(ControllerIDs.ServiceRequest, "Enterprise.CustomerService.GUI", "Enterprise.CustomerService.Module.IncidentApprovalController"));
			Add(new ControllerInfo(ControllerIDs.GPSSupporter, "Enterprise.GPS.Module", "Enterprise.GPS.Module.GPSSupporterPlugInController"));
			Add(new ControllerInfo(ControllerIDs.JobShipment, "Enterprise.Freight.Forwarding.Module", "Enterprise.Freight.Forwarding.Module.JobShipmentController"));
			Add(new ControllerInfo(ControllerIDs.JobConsol, "Enterprise.Freight.Forwarding.Module", "Enterprise.Freight.Forwarding.Module.JobConsolController"));
			Add(new ControllerInfo(ControllerIDs.ConsolPlanningBoard, "Enterprise.Freight.Forwarding.Module", "Enterprise.Freight.Forwarding.Module.ConsolDashboardController"));
			Add(new ControllerInfo(ControllerIDs.GatewayConsolProfitShareRedistribution, "Enterprise.Freight.Forwarding.Module", "Enterprise.Freight.Forwarding.Module.GatewayConsolProfitShareRedistributionController"));
			Add(new ControllerInfo(ControllerIDs.SupplierBooking, "Enterprise.Freight.Forwarding.Module", "Enterprise.Freight.Forwarding.Module.JobSupplierBookingController"));
			Add(new ControllerInfo(ControllerIDs.JobConsolContainersPacking, "Enterprise.Freight.Forwarding.Module", "Enterprise.Freight.Forwarding.Module.ContainersPackingController"));
			Add(new ControllerInfo(ControllerIDs.RoutingLookups, "Enterprise.Freight.Forwarding.Routing.S8.Module", "Enterprise.Freight.Forwarding.Routing.S8.Module.RealTimeRoutingController"));
			Add(new ControllerInfo(ControllerIDs.DocumentTracking, "Enterprise.Freight.Forwarding.Module", "Enterprise.Freight.Forwarding.Module.DocumentTrackingController"));
			Add(new ControllerInfo(ControllerIDs.RefCommodityCode, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.RefCommodityCodeController"));
			Add(new ControllerInfo(ControllerIDs.RefAirlineCommodityCode, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.RefAirlineCommodityCodeController"));
			Add(new ControllerInfo(ControllerIDs.RefOrgPartCategory, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.RefOrgPartCategoryController"));
			Add(new ControllerInfo(ControllerIDs.RefEquipment, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.RefEquipmentController"));
			Add(new ControllerInfo(ControllerIDs.RefPremisesGateCode, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.RefPremisesGateCodeController"));
			Add(new ControllerInfo(ControllerIDs.AccChargeCode, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.AccChargeCodeController"));
			Add(new ControllerInfo(ControllerIDs.AccGlobalChargeCode, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.AccGlobalChargeCodeController"));
			Add(new ControllerInfo(ControllerIDs.AccTaxOverrideGroup, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.AccTaxOverrideGroupController"));
			Add(new ControllerInfo(ControllerIDs.AccReportingBook, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.AccReportingBookController"));
			Add(new ControllerInfo(ControllerIDs.TaxFrameworkAccTaxOverrideGroup, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.TaxFrameworkAccTaxOverrideGroupController"));
			Add(new ControllerInfo(ControllerIDs.AccPlaceOfSupplyChargeCodeGroup, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.AccPOSChargeCodeGroupController"));
			Add(new ControllerInfo(ControllerIDs.AccOrgTaxConfigurationTemplate, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.AccOrgTaxConfigurationTemplateController"));
			Add(new ControllerInfo(ControllerIDs.GlobalChargeCodeIntercompany, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.GlobalChargeCodeIntercompanyController"));
			Add(new ControllerInfo(ControllerIDs.GlobalChargeCodeOrganization, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.GlobalChargeCodeOrganizationController"));
			Add(new ControllerInfo(ControllerIDs.RefCurrency, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.RefCurrencyController"));
			Add(new ControllerInfo(ControllerIDs.InternationalZone, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.InternationalZonesController"));
			Add(new ControllerInfo(ControllerIDs.RefTimeZoneSet, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.RefTimeZoneSetController"));
			Add(new ControllerInfo(ControllerIDs.RefNMFC, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.RefNMFCController"));
			Add(new ControllerInfo(ControllerIDs.WarningAcknowledgement, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.WarningAcknowledgementController"));
			Add(new ControllerInfo(ControllerIDs.RefCountry, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.RefCountryController"));
			Add(new ControllerInfo(ControllerIDs.RefCityTown, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.RefCityTownController"));
			Add(new ControllerInfo(ControllerIDs.RefPostCode, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.RefPostCodeController"));
			Add(new ControllerInfo(ControllerIDs.RefCountryStates, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.RefCountryStatesController"));
			Add(new ControllerInfo(ControllerIDs.CountryStatesGlbHoliday, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.CountryStatesGlbHolidayController"));
			Add(new ControllerInfo(ControllerIDs.OrgAddresses, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.OrgAddressesController"));
			Add(new ControllerInfo(ControllerIDs.ComplianceRiskPlugin, "Enterprise.ComplianceRisk.GUI", "Enterprise.ComplianceRisk.GUI.ComplianceRiskController"));
			Add(new ControllerInfo(ControllerIDs.GlobalCommercialInvoicePlugin, "Enterprise.GlobalCommercialInvoice.GUI", "Enterprise.GlobalCommercialInvoice.GUI.GlobalCommercialInvoiceController"));
			Add(new ControllerInfo(ControllerIDs.GenShapeGeography, "Enterprise.MasterData.Module", "Enterprise.MasterData.Module.GenShapeGeographyController"));
			Add(new ControllerInfo(ControllerIDs.OrgCusCode, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.OrgCusCodeController"));
			Add(new ControllerInfo(ControllerIDs.SalesEnquiry, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.SalesEnquiryController"));
			Add(new ControllerInfo(ControllerIDs.SalesProduct, "Enterprise.MarketingManager.Module", "Enterprise.MarketingManager.Module.SalesProductController"));
			Add(new ControllerInfo(ControllerIDs.Commission, "Enterprise.CommissionManagement.Module", "Enterprise.CommissionManagement.Module.CommissionManagementController"));
			Add(new ControllerInfo(ControllerIDs.CommissionLine, "Enterprise.CommissionManagement.Module", "Enterprise.CommissionManagement.Module.AccCommissionLineController"));
			Add(new ControllerInfo(ControllerIDs.CommissionApprovalRequest, "Enterprise.CommissionManagement.Module", "Enterprise.CommissionManagement.Module.CommissionApprovalRequestController"));
			Add(new ControllerInfo(ControllerIDs.Communication, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.CommunicationController"));
			Add(new ControllerInfo(ControllerIDs.OrgCommissionAgreement, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.CommissionAgreementController"));
			Add(new ControllerInfo(ControllerIDs.OrgContacts, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.OrgContactsController"));
			Add(new ControllerInfo(ControllerIDs.OrgContactsStmALog, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.OrgContactsStmALogController"));
			Add(new ControllerInfo(ControllerIDs.RefContainer, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.RefContainerController"));
			Add(new ControllerInfo(ControllerIDs.RefContainerISOTypes, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.RefContainerISOTypesController"));
			Add(new ControllerInfo(ControllerIDs.RefUNLOCO, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.RefUNLOCOController"));
			Add(new ControllerInfo(ControllerIDs.DocAddresses, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.DocAddressesController"));
			Add(new ControllerInfo(ControllerIDs.OrgCreditorGroup, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.OrgCreditorGroupController"));
			Add(new ControllerInfo(ControllerIDs.OrgDebtorGroup, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.OrgDebtorGroupController"));
			Add(new ControllerInfo(ControllerIDs.RefDocType, "Enterprise.DocumentScanning.Module", "Enterprise.DocumentScanning.Module.RefDocTypeController"));
			Add(new ControllerInfo(ControllerIDs.RefDocSource, "Enterprise.DocumentScanning.Module", "Enterprise.DocumentScanning.Module.RefDocSourceController"));
			Add(new ControllerInfo(ControllerIDs.RefDocOrgCusCode, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.RefDocOrgCusCodeController"));
			Add(new ControllerInfo(ControllerIDs.GlbBranch, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.GlbBranchController"));
			Add(new ControllerInfo(ControllerIDs.GlbCompany, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.GlbCompanyController"));
			Add(new ControllerInfo(ControllerIDs.GlbCompanyCampaignWithoutFilter, "Enterprise.MarketingManager.Module", "Enterprise.MarketingManager.Module.GlbCompanyCampaignWithoutFilterController"));
			Add(new ControllerInfo(ControllerIDs.GlbCompanyCampaignClick, "Enterprise.MarketingManager.GUI", "Enterprise.MarketingManager.GUI.GlbCompanyCampaignClickController"));
			Add(new ControllerInfo(ControllerIDs.GlbCompanyCampaignContact, "Enterprise.MarketingManager.GUI", "Enterprise.MarketingManager.GUI.GlbCompanyCampaignContactController"));
			Add(new ControllerInfo(ControllerIDs.GlbDepartment, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.GlbDepartmentController"));
			Add(new ControllerInfo(ControllerIDs.GlbGroup, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.GlbGroupController"));
			Add(new ControllerInfo(ControllerIDs.GlbStaff, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.GlbStaffController"));
			Add(new ControllerInfo(ControllerIDs.GlbStaffChangeRequest, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.GlbStaffChangeRequestController"));
			Add(new ControllerInfo(ControllerIDs.ReviewProcess, "Enterprise.HRM.Module", "Enterprise.HRM.Module.ReviewProcessController"));
			Add(new ControllerInfo(ControllerIDs.ReviewProcessNode, "Enterprise.HRM.Module", "Enterprise.HRM.Module.ReviewProcessNodeController"));
			Add(new ControllerInfo(ControllerIDs.GlbStaffHoliday, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.GlbStaffHolidayController"));
			Add(new ControllerInfo(ControllerIDs.GlbPerson, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.GlbPersonController"));
			Add(new ControllerInfo(ControllerIDs.GlbPersonNew, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.GlbPersonNewController"));
			Add(new ControllerInfo(ControllerIDs.GlbStaffCommissionPlugIn, "Enterprise.CommissionManagement.Module", "Enterprise.CommissionManagement.Module.StaffCommissionAgreementsPlugInController"));
			Add(new ControllerInfo(ControllerIDs.TradeProfileForOrgPlugin, "Enterprise.MarketingManager.Module", "Enterprise.MarketingManager.Module.TradeProfileForOrgPluginController"));
			Add(new ControllerInfo(ControllerIDs.TradeProfileForRelatedPlugin, "Enterprise.MarketingManager.Module", "Enterprise.MarketingManager.Module.TradeProfileForRelatedPluginController"));
			Add(new ControllerInfo(ControllerIDs.GlbCapability, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.GlbCapabilityController"));
			Add(new ControllerInfo(ControllerIDs.GlbResource, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.GlbResourceController"));
			Add(new ControllerInfo(ControllerIDs.NewsAndAnnouncement, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.NewsAndAnnouncementController"));
			Add(new ControllerInfo(ControllerIDs.AccGroups, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.AccGroupsController"));
			Add(new ControllerInfo(ControllerIDs.AccGLHeader, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.AccGLHeaderController"));
			Add(new ControllerInfo(ControllerIDs.AccGLAccountDescriptor, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.AccGLAccountDescriptorController"));
			Add(new ControllerInfo(ControllerIDs.AccChequeBook, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.AccChequeBookController"));
			Add(new ControllerInfo(ControllerIDs.AccBankAccount, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.AccBankAccountController"));
			Add(new ControllerInfo(ControllerIDs.ImportAccountingData, "Enterprise.DataConverters", "Enterprise.DataConverters.Accounting.AccountingImportController"));
			Add(new ControllerInfo(ControllerIDs.OrderLine, "Enterprise.Freight.Forwarding.Module", "Enterprise.Freight.Forwarding.Orders.Module.OrderLineController"));
			Add(new ControllerInfo(ControllerIDs.OrderLineFromOrder, "Enterprise.Freight.Forwarding.Module", "Enterprise.Freight.Forwarding.Orders.Module.OrderLineFromOrderController"));
			Add(new ControllerInfo(ControllerIDs.AccTaxRate, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.AccTaxRateController"));
			Add(new ControllerInfo(ControllerIDs.AccInvMsg, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.AccInvMsgController"));
			Add(new ControllerInfo(ControllerIDs.AccWithholding, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.AccWithholdingController"));
			Add(new ControllerInfo(ControllerIDs.JobSeaSailing, "Enterprise.Freight.Module", "Enterprise.Freight.Module.JobSeaSailingController"));
			Add(new ControllerInfo(ControllerIDs.JobAirSailing, "Enterprise.Freight.Module", "Enterprise.Freight.Module.JobAirSailingController"));
			Add(new ControllerInfo(ControllerIDs.JobRailSailing, "Enterprise.Freight.Module", "Enterprise.Freight.Module.JobRailSailingController"));
			Add(new ControllerInfo(ControllerIDs.JobRoadSailing, "Enterprise.Freight.Module", "Enterprise.Freight.Module.JobRoadSailingController"));
			Add(new ControllerInfo(ControllerIDs.TradeLane, "Enterprise.Freight.Module", "Enterprise.Freight.Module.JobTradeLaneController"));
			Add(new ControllerInfo(ControllerIDs.JobSeaVoyage, "Enterprise.Freight.Module", "Enterprise.Freight.Module.JobSeaVoyageController"));
			Add(new ControllerInfo(ControllerIDs.DocumentAllocation, "Enterprise.DocumentScanning.Module", "Enterprise.DocumentScanning.Module.DocumentAllocationController"));
			Add(new ControllerInfo(ControllerIDs.DocumentDbMerger, "Enterprise.DocumentScanning.Module", "Enterprise.DocumentScanning.Module.DocumentDbMergerController"));
			Add(new ControllerInfo(ControllerIDs.DocumentDbManager, "Enterprise.DocumentScanning.Module", "Enterprise.DocumentScanning.Module.DocumentDbManagerController"));
			Add(new ControllerInfo(ControllerIDs.ArchiveEDocs, "Enterprise.DocumentScanning.Module", "Enterprise.DocumentScanning.Module.ArchiveEDocsController"));
			Add(new ControllerInfo(ControllerIDs.DocumentTemplate, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.StmTemplateController"));
			Add(new ControllerInfo(ControllerIDs.RateAttachmentSet, "Enterprise.Rating.Module", "Enterprise.Rating.Module.RateAttachmentSetController"));
			Add(new ControllerInfo(ControllerIDs.SalesTeam, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.SalesTeamController"));
			Add(new ControllerInfo(ControllerIDs.SalesRep, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.SalesRepController"));
			Add(new ControllerInfo(ControllerIDs.RateTransportProvider, "Enterprise.Rating.Module", "Enterprise.Rating.Module.RateTransportProviderController"));
			Add(new ControllerInfo(ControllerIDs.RateTransportZone, "Enterprise.Rating.Module", "Enterprise.Rating.Module.RateTransportZoneController"));
			Add(new ControllerInfo(ControllerIDs.QuotedBookings, "Enterprise.Freight.QuotedBookings.Module", "Enterprise.Freight.QuotedBookings.Module.QuotedBookingController"));
			Add(new ControllerInfo(ControllerIDs.OneOffQuotes, "Enterprise.Freight.QuotedBookings.Module", "Enterprise.Freight.QuotedBookings.Module.OneOffQuoteController"));
			Add(new ControllerInfo(ControllerIDs.PreAllocations, "Enterprise.Freight.QuotedBookings.Module", "Enterprise.Freight.QuotedBookings.Module.PreAllocationController"));
			Add(new ControllerInfo(ControllerIDs.ShipmentReceival, "Enterprise.Freight.CFS.Module", "Enterprise.Freight.CFS.Module.ShipmentReceivalController"));
			Add(new ControllerInfo(ControllerIDs.AccHotCheque, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.AccHotChequeController"));
			Add(new ControllerInfo(ControllerIDs.PackContainerRegistration, "Enterprise.Freight.CFS.Module", "Enterprise.Freight.CFS.Module.PackContainerRegistrationController"));
			Add(new ControllerInfo(ControllerIDs.LoadListConsol, "Enterprise.Freight.CFS.Module", "Enterprise.Freight.CFS.Module.LoadListConsolController"));
			Add(new ControllerInfo(ControllerIDs.LoadListContainersPacking, "Enterprise.Freight.CFS.Module", "Enterprise.Freight.CFS.Module.LoadListContainersPackingController"));
			Add(new ControllerInfo(ControllerIDs.JobMawb, "Enterprise.Freight.Forwarding.Module", "Enterprise.Freight.Forwarding.Module.JobMawbController"));
			Add(new ControllerInfo(ControllerIDs.PackContainers, "Enterprise.Freight.QuotedBookings.Module", "Enterprise.Freight.QuotedBookings.Module.PackContainersController"));
			Add(new ControllerInfo(ControllerIDs.ManifestTally, "Enterprise.Freight.CFS.Module", "Enterprise.Freight.CFS.Module.ManifestTallyController"));
			Add(new ControllerInfo(ControllerIDs.ShipmentGatePass, "Enterprise.Freight.CFS.Module", "Enterprise.Freight.CFS.Module.ShipmentGatePassController"));
			Add(new ControllerInfo(ControllerIDs.LoadList, "Enterprise.Freight.QuotedBookings.Module", "Enterprise.Freight.QuotedBookings.Module.LoadListController"));
			Add(new ControllerInfo(ControllerIDs.eDocsPlugIn, "Enterprise.DocumentScanning.Module", "Enterprise.DocumentScanning.Module.eDocsPlugInController"));
			Add(new ControllerInfo(ControllerIDs.eConversationPlugIn, "Enterprise.EConversation.GUI", "Enterprise.EConversation.GUI.EConversationPluginController"));
			Add(new ControllerInfo(ControllerIDs.DocumentUDFPlugIn, "Enterprise.DocumentEngine.Module", "Enterprise.DocumentEngine.Module.DocumentUDFPlugInController"));
			Add(new ControllerInfo(ControllerIDs.DocumentSDFPlugIn, "Enterprise.DocumentEngine.Module", "Enterprise.DocumentEngine.Module.SDF.DocumentSDFPlugInController"));
			Add(new ControllerInfo(ControllerIDs.DocDataPlugIn, "Enterprise.DocumentEngine.Module", "Enterprise.DocumentEngine.Module.DocumentMenu.DocDataPlugIn.DocDataPlugInController"));
			Add(new ControllerInfo(ControllerIDs.OperationalActions, "Enterprise.Services.OperationalActions.Module", "Enterprise.Services.OperationalActions.Module.OperationalActionsController"));
			Add(new ControllerInfo(ControllerIDs.PrintJob, "Enterprise.DocumentEngine.Module", "Enterprise.DocumentEngine.Module.PrintJobController"));
			Add(new ControllerInfo(ControllerIDs.DocumentSigningJob, "Enterprise.DocumentEngine.Module", "Enterprise.DocumentEngine.Module.DocumentSigningJobController"));
			Add(new ControllerInfo(ControllerIDs.ArchiveSchedule, "Enterprise.ArchiveManager.Module", "Enterprise.ArchiveManager.Module.Schedule.ArchiveScheduleController"));
			Add(new ControllerInfo(ControllerIDs.ArchivedRecords, "Enterprise.ArchiveManager.Module", "Enterprise.ArchiveManager.Module.Records.ArchivedRecordsController"));
			Add(new ControllerInfo(ControllerIDs.ScheduledReports, "Enterprise.DocumentEngine.Module", "Enterprise.DocumentEngine.Scheduler.Module.ScheduledReportsController"));
			Add(new ControllerInfo(ControllerIDs.ReportStatistics, "Enterprise.DocumentEngine.Module", "Enterprise.DocumentEngine.Scheduler.Module.ReportStatisticsController"));
			Add(new ControllerInfo(ControllerIDs.ReportManagement, "Enterprise.DocumentEngine.Module", "Enterprise.DocumentEngine.Scheduler.Module.ReportManagementController"));
			Add(new ControllerInfo(ControllerIDs.StmServiceTask, "Enterprise.ServiceManager.Module", "Enterprise.ServiceManager.Module.StmServiceTaskController"));
			Add(new ControllerInfo(ControllerIDs.ProcessController, "Enterprise.ServiceManager.Module", "Enterprise.ServiceManager.Module.ProcessControllerController"));
			Add(new ControllerInfo(ControllerIDs.ServiceTaskLogViewer, "Enterprise.ServiceManager.Module", "Enterprise.ServiceManager.Module.ServiceTaskLogViewerController"));
			Add(new ControllerInfo(ControllerIDs.ServiceTaskProxyConfiguration, "Enterprise.ServiceManager.Module", "Enterprise.ServiceManager.Module.ServiceTaskProxyConfigurationController"));
			Add(new ControllerInfo(ControllerIDs.PrintQueue, "Enterprise.DocumentEngine.Module", "Enterprise.DocumentEngine.Module.PrintQueueController"));
			Add(new ControllerInfo(ControllerIDs.Statement, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.StatementController"));
			Add(new ControllerInfo(ControllerIDs.JobInvoicing, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.JobInvoicingController"));
			Add(new ControllerInfo(ControllerIDs.JobInvoicingConsol, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.JobInvoicingConsolController"));
			Add(new ControllerInfo(ControllerIDs.JobProfitLossConsol, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.JobProfitLossConsolController"));
			Add(new ControllerInfo(ControllerIDs.LinkedeNettEDIMessage, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.LinkedeNettEDIMessageController"));
			Add(new ControllerInfo(ControllerIDs.Apportionment, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.ApportionmentController"));
			Add(new ControllerInfo(ControllerIDs.SellApportionmentForGateway, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.SellApportionmentForGatewayController"));
			Add(new ControllerInfo(ControllerIDs.ApportionmentForCommonWorkSheet, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.ApportionmentForCommonWorkSheetController"));
			Add(new ControllerInfo(ControllerIDs.ApportionmentForTransitTransportationUnit, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.ApportionmentForTransitTransportationUnitController"));
			Add(new ControllerInfo(ControllerIDs.ARContra, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.ARContraController"));
			Add(new ControllerInfo(ControllerIDs.APContra, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.APContraController"));
			Add(new ControllerInfo(ControllerIDs.JCCostingJournal, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.JCJournalController"));
			Add(new ControllerInfo(ControllerIDs.ARTransfer, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.ARTransferController"));
			Add(new ControllerInfo(ControllerIDs.APTransfer, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.APTransferController"));
			Add(new ControllerInfo(ControllerIDs.InvoicePrinting, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.InvoicePrintingController"));
			Add(new ControllerInfo(ControllerIDs.JobHeader, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.JobManagementControllerBase"));
			Add(new ControllerInfo(ControllerIDs.OrgMatchApproval, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.OrgMatchApprovalController"));
			Add(new ControllerInfo(ControllerIDs.OrgMatchApprovalCreateNewOrg, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.OrgMatchApprovalCreateNewOrgController"));
			Add(new ControllerInfo(ControllerIDs.CNDataInterface, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.CNDataInterfaceController"));
			Add(new ControllerInfo(ControllerIDs.CN2004DataInterface, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.CN2004DataInterfaceController"));
			Add(new ControllerInfo(ControllerIDs.CNReconciliationExport, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.CNReconciliationExportController"));
			Add(new ControllerInfo(ControllerIDs.PortDepotCarrierSelection, "Enterprise.Freight.PortHubs.Module", "Enterprise.Freight.PortHubs.Module.PortDepotCarrierSelectionController"));
			Add(new ControllerInfo(ControllerIDs.PortHubSelection, "Enterprise.Freight.PortHubs.Module", "Enterprise.Freight.PortHubs.Module.PortHubSelectionController"));
			Add(new ControllerInfo(ControllerIDs.DocumentVisualizer, "Enterprise.DocumentVisualizer.Module", "Enterprise.DocumentVisualizer.Module.DocumentVisualizerController"));
			Add(new ControllerInfo(ControllerIDs.ErrorReportDetails, "Enterprise.ErrorReporting.Module", "Enterprise.ErrorReporting.Module.ErrorReportDetailsController"));
			Add(new ControllerInfo(ControllerIDs.DeviceDetails, "Enterprise.Telematics.Module", "Enterprise.Telematics.Module.Controllers.DeviceDetailsController"));
			Add(new ControllerInfo(ControllerIDs.TelPreDriveChecklist, "Enterprise.Telematics.Module", "Enterprise.Telematics.Module.Controllers.TelPreDriveChecklistController"));
			Add(new ControllerInfo(ControllerIDs.TelPreDriveChecklistTemplate, "Enterprise.Telematics.Module", "Enterprise.Telematics.Module.Controllers.TelPreDriveChecklistTemplateController"));
			Add(new ControllerInfo(ControllerIDs.RefTransitTime, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.RefTransitTimeController"));
			Add(new ControllerInfo(ControllerIDs.HVLVBookingHeader, "Enterprise.eTail.Module", "Enterprise.eTail.Module.HVLVBookingHeaderController"));
			Add(new ControllerInfo(ControllerIDs.HVLVConsignment, "Enterprise.eTail.Module", "Enterprise.eTail.Module.HVLVConsignmentController"));
			Add(new ControllerInfo(ControllerIDs.HVLVOriginLoadList, "Enterprise.eTail.Module", "Enterprise.eTail.Module.HVLVOriginLoadListController"));
			Add(new ControllerInfo(ControllerIDs.HVLVOuterPackage, "Enterprise.eTail.Module", "Enterprise.eTail.Module.HVLVOuterPackageController"));
			Add(new ControllerInfo(ControllerIDs.TransitTimeServiceLevelCombination, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.TransitTimeServiceLevelCombinationController"));

			Add(new ControllerInfo(ControllerIDs.AccountingVoucher, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.AccountingVoucherController"));
			Add(new ControllerInfo(ControllerIDs.ChinaJournalListing, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.ChinaJournalListingController"));
			Add(new ControllerInfo(ControllerIDs.DirectDebitFile, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.DirectDebitFileController"));
			Add(new ControllerInfo(ControllerIDs.InvoiceBatch, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.InvoiceBatchController"));
			Add(new ControllerInfo(ControllerIDs.InvoiceBulkBatch, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.InvoiceBulkBatchController"));
			Add(new ControllerInfo(ControllerIDs.Cartage, "Enterprise.Freight.LocalCartage.Module", "Enterprise.Freight.LocalCartage.Module.CartageController"));
			Add(new ControllerInfo(ControllerIDs.CartagePlugin, "Enterprise.Freight.LocalCartage.Module", "Enterprise.Freight.LocalCartage.Module.CartagePluginController"));
			Add(new ControllerInfo(ControllerIDs.ConfirmationsPlugin, "Enterprise.Freight.Confirmations.Module", "Enterprise.Freight.Confirmations.Module.ConfirmationsPluginController"));
			Add(new ControllerInfo(ControllerIDs.CartageWorkSheet, "Enterprise.Freight.LocalCartage.Module", "Enterprise.Freight.LocalCartage.Module.CartageWorkSheetController"));
			Add(new ControllerInfo(ControllerIDs.CartageLeg, "Enterprise.Freight.LocalCartage.Module", "Enterprise.Freight.LocalCartage.Module.CartageLegController"));
			Add(new ControllerInfo(ControllerIDs.CartageLegPlanner, "Enterprise.Freight.LocalCartage.Module", "Enterprise.Freight.LocalCartage.Module.CartageLegPlannerController"));
			Add(new ControllerInfo(ControllerIDs.CartageRunSheetDashboard, "Enterprise.Freight.LocalCartage.Module", "Enterprise.Freight.LocalCartage.Module.CartageRunSheetDashboardController"));
			Add(new ControllerInfo(ControllerIDs.APIncompleteInvoice, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.Transaction.APIncompleteInvoicesController"));
			Add(new ControllerInfo(ControllerIDs.APIncompleteCreditNote, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.Transaction.APIncompleteCreditNotesController"));
			Add(new ControllerInfo(ControllerIDs.APIncompleteAdjustmentNote, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.Transaction.APIncompleteAdjustmentNotesController"));
			Add(new ControllerInfo(ControllerIDs.ARCreditNoteApproval, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.TransactionApproval.ARCreditNoteApprovalController"));
			Add(new ControllerInfo(ControllerIDs.TransactionsPendingAllocationApproval, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.TransactionApproval.TransactionPendingAllocationApprovalController"));
			Add(new ControllerInfo(ControllerIDs.APInvoiceApproval, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.TransactionApproval.APInvoiceApprovalController"));
			Add(new ControllerInfo(ControllerIDs.APInvoiceLinkedToApproval, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.TransactionApproval.APInvoiceLinkedToApprovalController"));
			Add(new ControllerInfo(ControllerIDs.APCreditNoteLinkedToApproval, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.TransactionApproval.APCreditNoteLinkedToApprovalController"));
			Add(new ControllerInfo(ControllerIDs.APInvoiceNewForApproval, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.TransactionApproval.APInvoiceNewForApprovalController"));
			Add(new ControllerInfo(ControllerIDs.APCreditNoteNewForApproval, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.TransactionApproval.APCreditNoteNewForApprovalController"));
			Add(new ControllerInfo(ControllerIDs.CreditControlledDocumentsApproval, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.CreditControlledDocumentsApprovalController"));
			Add(new ControllerInfo(ControllerIDs.StmALog, "Enterprise.ZArchitecture.GUI.UserControls", "Enterprise.ZArchitecture.GUI.ZStmALogController"));
			Add(new ControllerInfo(ControllerIDs.StmModuleFilter, "Enterprise.ZArchitecture.GUI.UserControls", "Enterprise.ZArchitecture.GUI.ZStmModuleFilterController"));
			Add(new ControllerInfo(ControllerIDs.DialogDefault, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.DialogDefaultController"));

			Add(new ControllerInfo(ControllerIDs.ConsolidatedTransportBooking, "Enterprise.Freight.Confirmations.Module", "Enterprise.Freight.Confirmations.Module.ConsolidatedTransportBookingController"));
			Add(new ControllerInfo(ControllerIDs.PickupDeliveryConfirm, "Enterprise.Freight.Confirmations.Module", "Enterprise.Freight.Confirmations.Module.PickupDeliveryConfirmController"));
			Add(new ControllerInfo(ControllerIDs.QuickPOD, "Enterprise.Freight.Confirmations.Module", "Enterprise.Freight.Confirmations.Module.QuickPODController"));

			Add(new ControllerInfo(ControllerIDs.CartageType, "Enterprise.Freight.Common.Module", "Enterprise.Freight.Common.Module.CartageTypeController"));
			Add(new ControllerInfo(ControllerIDs.ExchangeRateWrapper, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.ExchangeRateWrapperController"));
			Add(new ControllerInfo(ControllerIDs.StmUpgrade, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.StmUpgradeController"));
			Add(new ControllerInfo(ControllerIDs.UNDGSubstance, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.UNDGSubstanceController"));
			Add(new ControllerInfo(ControllerIDs.UNDGCommonData, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.UNDGCommonDataController"));
			Add(new ControllerInfo(ControllerIDs.RefCarrierConsortium, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.RefCarrierConsortiumController"));
			Add(new ControllerInfo(ControllerIDs.AUContainerMessaging, "Enterprise.Customs.AU.Module", "Enterprise.Customs.AU.PRA.Module.AUContainerMessagingController", "AU"));
			Add(new ControllerInfo(ControllerIDs.GLBudget, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.GLBudgetController"));
			Add(new ControllerInfo(ControllerIDs.Registry, "Enterprise.Registry.GUI", "Enterprise.Registry.GUI.RegistryController"));
			Add(new ControllerInfo(ControllerIDs.StmFeatureTest, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.StmFeatureTestController"));
			Add(new ControllerInfo(ControllerIDs.APIntercompanyCostsApportionment, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.IntercompanyCostsApportionmentController"));
			Add(new ControllerInfo(ControllerIDs.AccApportionmentTemplate, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.AccApportionmentTemplateController"));

			Add(new ControllerInfo(ControllerIDs.ZARMatching, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.ARMatchingController"));
			Add(new ControllerInfo(ControllerIDs.ZAPMatching, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.APMatchingController"));
			Add(new ControllerInfo(ControllerIDs.RefAirline, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.RefAirlineController"));
			Add(new ControllerInfo(ControllerIDs.NumericCodeRefAirline, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.NumericCodeRefAirlineController"));
			Add(new ControllerInfo(ControllerIDs.RefPackType, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.RefPackTypeController"));
			Add(new ControllerInfo(ControllerIDs.RefDomesticCartageZone, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.RefDomesticCartageZoneController"));
			Add(new ControllerInfo(ControllerIDs.MailItem, "MailManager.Module", "Enterprise.MailManager.Module.MailItemController"));
			Add(new ControllerInfo(ControllerIDs.MailItemTemplate, "MailManager.Module", "Enterprise.MailManager.Module.MailItemTemplateController"));
			Add(new ControllerInfo(ControllerIDs.ProcessQueue, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.ProcessQueueController"));
			Add(new ControllerInfo(ControllerIDs.CASSCostFileImport, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.CASSBillingController"));
			Add(new ControllerInfo(ControllerIDs.UACreditNote, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.UACreditNoteController"));
			Add(new ControllerInfo(ControllerIDs.UAInvoice, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.UAInvoiceController"));
			Add(new ControllerInfo(ControllerIDs.ClaimCharges, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.ClaimChargesController"));
			Add(new ControllerInfo(ControllerIDs.AccComplianceSequence, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.AccComplianceSequenceController"));
			Add(new ControllerInfo(ControllerIDs.AccComplianceSequenceBulk, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.AccComplianceSequnceBulkController"));
			Add(new ControllerInfo(ControllerIDs.LockComplianceBook, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.LockComplianceBookController"));
			Add(new ControllerInfo(ControllerIDs.ReleaseComplianceBook, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.ReleaseComplianceBookController"));
			Add(new ControllerInfo(ControllerIDs.AccCollectionBatch, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.AccCollectionBatchController"));
			Add(new ControllerInfo(ControllerIDs.AccCollectionBatchPosting, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.AccCollectionBatchPostingController"));
			Add(new ControllerInfo(ControllerIDs.AccCollectionOrder, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.AccCollectionOrderController"));
			Add(new ControllerInfo(ControllerIDs.AccComplianceReport, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.AccComplianceReportController"));
			Add(new ControllerInfo(ControllerIDs.AccPayableOrder, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.AccPayableOrderController"));
			Add(new ControllerInfo(ControllerIDs.JobBillingExRateSysConfig, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.JobBillingExRateConfigController"));
			Add(new ControllerInfo(ControllerIDs.RefShippingLine, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.RefShippingLineController"));
			Add(new ControllerInfo(ControllerIDs.RefFacility, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.RefFacilityController"));
			Add(new ControllerInfo(ControllerIDs.RefComplianceList, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.RefComplianceListController"));
			Add(new ControllerInfo(ControllerIDs.RefComplianceCommodityAlert, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.RefComplianceCommodityAlertController"));

			Add(new ControllerInfo(ControllerIDs.NettingStatement, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.NettingStatementController"));
			Add(new ControllerInfo(ControllerIDs.NettingPeriod, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.NettingPeriodController"));

			Add(new ControllerInfo(ControllerIDs.ExchangeRate, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.RefExchangeRateController"));

			Add(new ControllerInfo(ControllerIDs.StaffCredentialsPlugIn, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.StaffCredentialsPlugInController"));
			Add(new ControllerInfo(ControllerIDs.StaffNumberRangesPlugIn, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.StaffNumberRangesPlugInController"));

			Add(new ControllerInfo(ControllerIDs.AccGeneralLedgerData, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.AccGeneralLedgerDataController"));

			//Cash Advance
			Add(new ControllerInfo(ControllerIDs.ARCashAdvance, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.ARCashAdvanceController"));
			Add(new ControllerInfo(ControllerIDs.APCashAdvance, "Enterprise.Accounting.Module", "Enterprise.Accounting.Module.APCashAdvanceController"));

			// Agency
			Add(new ControllerInfo(ControllerIDs.AgencyBooking, "Enterprise.Freight.Agency.Module", "Enterprise.Freight.Agency.Module.BookingController"));
			Add(new ControllerInfo(ControllerIDs.AgencyBillOfLading, "Enterprise.Freight.Agency.Module", "Enterprise.Freight.Agency.Module.BillOfLadingController"));
			Add(new ControllerInfo(ControllerIDs.AgencyBillContainers, "Enterprise.Freight.Agency.Module", "Enterprise.Freight.Agency.Module.BillContainersController"));
			Add(new ControllerInfo(ControllerIDs.AgencyContainerDetention, "Enterprise.Freight.Agency.Module", "Enterprise.Freight.Agency.Module.ContainerDetentionController"));
			Add(new ControllerInfo(ControllerIDs.AgencyContainerManager, "Enterprise.Freight.Agency.Module", "Enterprise.Freight.Agency.Module.ContainerManagerController"));
			Add(new ControllerInfo(ControllerIDs.AgencyContainerMove, "Enterprise.Freight.Agency.Module", "Enterprise.Freight.Agency.Module.ContainerMoveController"));
			Add(new ControllerInfo(ControllerIDs.AgencyVoyageAccounting, "Enterprise.Freight.Agency.Module", "Enterprise.Freight.Agency.Module.VoyageAccountingController"));
			Add(new ControllerInfo(ControllerIDs.AgencySundryCharges, "Enterprise.Freight.Agency.Module", "Enterprise.Freight.Agency.Module.SundryChargesController"));
			Add(new ControllerInfo(ControllerIDs.AgencyAllocation, "Enterprise.Freight.Agency.Module", "Enterprise.Freight.Agency.Module.AgencyAllocationController"));
			Add(new ControllerInfo(ControllerIDs.AgencyPortMessaging, "Enterprise.Freight.Agency.Module", "Enterprise.Freight.Agency.Module.AgencyPortMessagesController"));
			Add(new ControllerInfo(ControllerIDs.AgencyNZPortMessaging, "Enterprise.Freight.Agency.Module", "Enterprise.Freight.Agency.Module.NZPortMessageController"));
			Add(new ControllerInfo(ControllerIDs.AgencyDangerousGoodsManifest, "Enterprise.Freight.Agency.Module", "Enterprise.Freight.Agency.Module.DangerousGoodsManifestController"));

			// Ocean Carrier
			Add(new ControllerInfo(ControllerIDs.CarrierShipmentHeader, "Enterprise.OceanCarrier.Module", "Enterprise.OceanCarrier.Module.CarrierShipmentHeaderController"));

			// Barcode Parsing
			Add(new ControllerInfo(ControllerIDs.BarcodeParsing, "Enterprise.BarcodeParsing.Module", "Enterprise.BarcodeParsing.Module.BarcodeParsingController"));

			// Barcode Validation
			Add(new ControllerInfo(ControllerIDs.BarcodeValidation, "Enterprise.BarcodeParsing.Module", "Enterprise.BarcodeParsing.Module.BarcodeValidationController"));

			// Carrier Messaging Buss
			Add(new ControllerInfo(ControllerIDs.RefAccessorial, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.RefAccessorialController"));
			Add(new ControllerInfo(ControllerIDs.RefMessagingBussCarrierInfo, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.RefMessagingBussCarrierInfoController"));

			// Equipment Combination
			Add(new ControllerInfo(ControllerIDs.RefJobEquipment, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.RefJobEquipmentController"));

			// domestic transport booking
			Add(new ControllerInfo(ControllerIDs.DtbBooking, "Enterprise.TransportBookings.Module", "Enterprise.TransportBookings.Module.DtbBookingController"));
			Add(new ControllerInfo(ControllerIDs.DtbBookingTabPlugIn, "Enterprise.TransportBookings.Module", "Enterprise.TransportBookings.Module.DtbBookingTabPlugInController"));
			Add(new ControllerInfo(ControllerIDs.DtbBookingConsolidation, "Enterprise.TransportBookings.Module", "Enterprise.TransportBookings.Module.DtbBookingConsolidationController"));
			Add(new ControllerInfo(ControllerIDs.DtbBookingTmpl, "Enterprise.TransportBookings.Module", "Enterprise.TransportBookings.Module.DtbBookingTmplController"));
			Add(new ControllerInfo(ControllerIDs.MasterBookingPackingPlugIn, "Enterprise.TransportBookings.Module", "Enterprise.TransportBookings.Module.MasterBookingPackingPlugInController"));

			// land transport consignment
			Add(new ControllerInfo(ControllerIDs.DtbConsignment, "Enterprise.TransportConsignment.Module", "Enterprise.TransportConsignment.Module.DtbConsignmentController"));

			// domestic consignment
			Add(new ControllerInfo(ControllerIDs.DtbBookingConsignment, "Enterprise.TransportConsignment.Module", "Enterprise.TransportConsignment.Module.DtbBookingConsignmentController"));
			Add(new ControllerInfo(ControllerIDs.DtbConsignmentRunSheet, "Enterprise.TransportConsignment.Module", "Enterprise.TransportConsignment.Module.DtbConsignmentRunSheetController"));
			Add(new ControllerInfo(ControllerIDs.DtbRoutePlanner, "Enterprise.TransportConsignment.Module", "Enterprise.TransportConsignment.Module.DtbRoutePlannerController"));

			// Product Warehouse
			Add(new ControllerInfo(ControllerIDs.WhsAdHocServiceJob, "Enterprise.Warehouse.Transactions.Module", "Enterprise.Warehouse.Transactions.Module.AdHocServiceJobController"));
			Add(new ControllerInfo(ControllerIDs.WhsConfigWarehouse, "Enterprise.Warehouse.Environment.Module", "Enterprise.Warehouse.Environment.Module.WarehouseController"));
			Add(new ControllerInfo(ControllerIDs.WhsConfigRow, "Enterprise.Warehouse.Environment.Module", "Enterprise.Warehouse.Environment.Module.RowController"));
			Add(new ControllerInfo(ControllerIDs.WhsConfigArea, "Enterprise.Warehouse.Environment.Module", "Enterprise.Warehouse.Environment.Module.AreaController"));
			Add(new ControllerInfo(ControllerIDs.WhsConfigLocation, "Enterprise.Warehouse.Environment.Module", "Enterprise.Warehouse.Environment.Module.LocationController"));
			Add(new ControllerInfo(ControllerIDs.WhsConfigLocationType, "Enterprise.Warehouse.Environment.Module", "Enterprise.Warehouse.Environment.Module.LocationTypeController"));
			Add(new ControllerInfo(ControllerIDs.WhsConfigDynamicPickFaces, "Enterprise.Warehouse.Environment.Module", "Enterprise.Warehouse.Environment.Module.DynamicPickFacesController"));
			Add(new ControllerInfo(ControllerIDs.WhsConfigPickFaces, "Enterprise.Warehouse.Environment.Module", "Enterprise.Warehouse.Environment.Module.PickFacesController"));
			Add(new ControllerInfo(ControllerIDs.WhsReceive, "Enterprise.Warehouse.Transactions.Module", "Enterprise.Warehouse.Transactions.Module.ReceiveController"));
			Add(new ControllerInfo(ControllerIDs.WhsOrder, "Enterprise.Warehouse.Transactions.Module", "Enterprise.Warehouse.Transactions.Module.OrderController"));
			Add(new ControllerInfo(ControllerIDs.WhsOrderLine, "Enterprise.Warehouse.Transactions.Module", "Enterprise.Warehouse.Transactions.Module.OrderLineController"));
			Add(new ControllerInfo(ControllerIDs.WhsWorkOrder, "Enterprise.Warehouse.Transactions.Module", "Enterprise.Warehouse.Transactions.Module.WorkOrderController"));
			Add(new ControllerInfo(ControllerIDs.WhsWorkOrderLine, "Enterprise.Warehouse.Transactions.Module", "Enterprise.Warehouse.Transactions.Module.WorkOrderLineController"));
			Add(new ControllerInfo(ControllerIDs.WhsDynamicWorkOrder, "Enterprise.Warehouse.Transactions.Module", "Enterprise.Warehouse.Transactions.Module.DynamicWorkOrderController"));
			Add(new ControllerInfo(ControllerIDs.WhsPicking, "Enterprise.Warehouse.Transactions.Module", "Enterprise.Warehouse.Transactions.Module.PickingController"));
			Add(new ControllerInfo(ControllerIDs.WhsTransfer, "Enterprise.Warehouse.Transactions.Module", "Enterprise.Warehouse.Transactions.Module.TransferController"));
			Add(new ControllerInfo(ControllerIDs.WhsRelease, "Enterprise.Warehouse.Transactions.Module", "Enterprise.Warehouse.Transactions.Module.ReleaseController"));
			Add(new ControllerInfo(ControllerIDs.WhsReleasePackageJob, "Enterprise.Warehouse.Transactions.Module", "Enterprise.Warehouse.Transactions.Module.ReleasePackageJobController"));
			Add(new ControllerInfo(ControllerIDs.WhsAdjustment, "Enterprise.Warehouse.Transactions.Module", "Enterprise.Warehouse.Transactions.Module.AdjustmentController"));
			Add(new ControllerInfo(ControllerIDs.WhsStocktake, "Enterprise.Warehouse.Transactions.Module", "Enterprise.Warehouse.Transactions.Module.StocktakeController"));
			Add(new ControllerInfo(ControllerIDs.WhsHandlingUnit, "Enterprise.Warehouse.Transactions.Module", "Enterprise.Warehouse.Transactions.Module.WhsHandlingUnitController"));
			Add(new ControllerInfo(ControllerIDs.WhsInventory, "Enterprise.Warehouse.Transactions.Module", "Enterprise.Warehouse.Transactions.Module.InventoryController"));
			Add(new ControllerInfo(ControllerIDs.WhsInventoryHeldCodes, "Enterprise.Warehouse.Transactions.Module", "Enterprise.Warehouse.Transactions.Module.WhsInventoryHeldCodeController"));
			Add(new ControllerInfo(ControllerIDs.WhsConfigOrgReceive, "Enterprise.Warehouse.Transactions.Module", "Enterprise.Warehouse.Transactions.Module.OrgReceiveController"));
			Add(new ControllerInfo(ControllerIDs.WhsConfigOrgPicking, "Enterprise.Warehouse.Transactions.Module", "Enterprise.Warehouse.Transactions.Module.OrgPickingController"));
			Add(new ControllerInfo(ControllerIDs.WhsConfigProduct, "Enterprise.Warehouse.Transactions.Module", "Enterprise.Warehouse.Transactions.Module.ProductController"));
			Add(new ControllerInfo(ControllerIDs.WhsConfigProductStyle, "Enterprise.Warehouse.Environment.Module", "Enterprise.Warehouse.Environment.Module.ProductStyleController"));
			Add(new ControllerInfo(ControllerIDs.WhsInvoicing, "Enterprise.Warehouse.Transactions.Module", "Enterprise.Warehouse.Transactions.Module.InvoicingController"));
			Add(new ControllerInfo(ControllerIDs.WhsVASOrder, "Enterprise.Warehouse.Transactions.Module", "Enterprise.Warehouse.Transactions.Module.VASOrderController"));
			Add(new ControllerInfo(ControllerIDs.WhsCartonSize, "Enterprise.Warehouse.Environment.Module", "Enterprise.Warehouse.Environment.Module.WhsCartonSizeController"));
			Add(new ControllerInfo(ControllerIDs.WhsCartonGroup, "Enterprise.Warehouse.Environment.Module", "Enterprise.Warehouse.Environment.Module.WhsCartonGroupController"));
			Add(new ControllerInfo(ControllerIDs.WhsConfigPutawayGroup, "Enterprise.Warehouse.Environment.Module", "Enterprise.Warehouse.Environment.Module.WhsPutawayGroupController"));
			Add(new ControllerInfo(ControllerIDs.WhsSalesChannel, "Enterprise.Warehouse.Environment.Module", "Enterprise.Warehouse.Environment.Module.WhsSalesChannelController"));
			Add(new ControllerInfo(ControllerIDs.WhsLoad, "Enterprise.Warehouse.Transactions.Module", "Enterprise.Warehouse.Transactions.Module.LoadController"));
			Add(new ControllerInfo(ControllerIDs.WhsCycleCountWave, "Enterprise.Warehouse.Transactions.Module", "Enterprise.Warehouse.Transactions.Module.CycleCountWaveController"));

			// Transit Warehouse
			Add(new ControllerInfo(ControllerIDs.WhsItemReceiveTransportationUnit, "Enterprise.Warehouse.Transit.Module", "Enterprise.Warehouse.Transit.Module.WhsItemReceiveTransportationUnitController"));
			Add(new ControllerInfo(ControllerIDs.WhsTransitReceiveConsignment, "Enterprise.Warehouse.Transit.Module", "Enterprise.Warehouse.Transit.Module.WhsTransitReceiveConsignmentController"));
			Add(new ControllerInfo(ControllerIDs.WhsTransitDispatchConsignment, "Enterprise.Warehouse.Transit.Module", "Enterprise.Warehouse.Transit.Module.WhsTransitDispatchConsignmentController"));
			Add(new ControllerInfo(ControllerIDs.WhsItemDispatchTransportationUnit, "Enterprise.Warehouse.Transit.Module", "Enterprise.Warehouse.Transit.Module.WhsItemDispatchTransportationUnitController"));
			Add(new ControllerInfo(ControllerIDs.WhsItemReceiveASN, "Enterprise.Warehouse.Transit.Module", "Enterprise.Warehouse.Transit.Module.WhsItemReceiveASNController"));
			Add(new ControllerInfo(ControllerIDs.TransitHandlingUnit, "Enterprise.Warehouse.Transit.Module", "Enterprise.Warehouse.Transit.Module.TransitHandlingUnitController"));
			Add(new ControllerInfo(ControllerIDs.TransitWarehouseAttachPackages, "Enterprise.Warehouse.Transit.Module", "Enterprise.Warehouse.Transit.Module.TransitWarehousePackageAttachController"));
			Add(new ControllerInfo(ControllerIDs.TransitWarehouseViewPackages, "Enterprise.Warehouse.Transit.Module", "Enterprise.Warehouse.Transit.Module.TransitWarehouseViewPackagesController"));
			Add(new ControllerInfo(ControllerIDs.WhsItemTransferHeader, "Enterprise.Warehouse.Transit.Module", "Enterprise.Warehouse.Transit.Module.WhsItemTransferHeaderController"));
			Add(new ControllerInfo(ControllerIDs.WhsItemDispatchLoadList, "Enterprise.Warehouse.Transit.Module", "Enterprise.Warehouse.Transit.Module.WhsItemDispatchLoadListController"));

			// Customs Controllers
			Add(new ControllerInfo(ControllerIDs.Customs.EntryHeader, "Enterprise.Customs.Module", "Enterprise.Customs.Module.EntryHeaderController"));
			Add(new ControllerInfo(ControllerIDs.Customs.ImportCustomsFilesData, "Enterprise.DataConverters", "Enterprise.DataConverters.CustomsFiles.CustomsFilesImportController"));
			Add(new ControllerInfo(ControllerIDs.Customs.SupplierPart, "Enterprise.Customs.Module", "Enterprise.Customs.Module.OrgSupplierPartController"));
			Add(new ControllerInfo(ControllerIDs.CommercialInvoice, "Enterprise.Customs.Module", "Enterprise.Customs.Module.CommercialInvoiceController"));
			Add(new ControllerInfo(ControllerIDs.Customs.JobDeclaration, "Enterprise.Customs.Module", "Enterprise.Customs.Module.JobDeclarationController"));
			Add(new ControllerInfo(ControllerIDs.Customs.SingleTariffClassification, "Enterprise.Customs.Module", "Enterprise.Customs.Module.SingleTariffClassificationController"));
			Add(new ControllerInfo(ControllerIDs.Customs.JobDeclarationPluggedIntoShipment, "Enterprise.Customs.Module", "Enterprise.Customs.Module.JobDeclarationShipmentController"));
			Add(new ControllerInfo(ControllerIDs.Customs.RefPacks, "Enterprise.Customs.Module", "Enterprise.Customs.Shared.Module.RefPacksController"));
			Add(new ControllerInfo(ControllerIDs.ImporterSecurityFiling, "Enterprise.Customs.US.ISF.Module", "Enterprise.Customs.US.ISF.Module.ISFController"));
			Add(new ControllerInfo(ControllerIDs.LandedCosting, "Enterprise.LandedCosting.Module", "Enterprise.LandedCosting.Module.LandedCostingController"));
			Add(new ControllerInfo(ControllerIDs.LandedCostingHistoryView, "Enterprise.LandedCosting.Module", "Enterprise.LandedCosting.Module.LandedCostingHistoryViewController"));
			Add(new ControllerInfo(ControllerIDs.Customs.US.AMS, "Enterprise.Customs.US.AMS.Module", "Enterprise.Customs.US.AMS.Module.CusInBondHeaderController"));
			Add(new ControllerInfo(ControllerIDs.Customs.US.AMSPluggedIntoConsol, "Enterprise.Customs.US.AMS.Module", "Enterprise.Customs.US.AMS.Module.CusInBondHeaderConsolController"));
			Add(new ControllerInfo(ControllerIDs.Customs.US.AMSBill, "Enterprise.Customs.US.AMS.Module", "Enterprise.Customs.US.AMS.Module.CusInBondBillController"));
			Add(new ControllerInfo(ControllerIDs.Customs.US.StowPlan, "Enterprise.Customs.US.AMS.Module", "Enterprise.Customs.US.AMS.Module.StowPlanController"));
			Add(new ControllerInfo(ControllerIDs.Customs.JP.AFR, "Enterprise.Customs.JP.AFR.Module", "Enterprise.Customs.JP.AFR.Module.JPAFRController"));
			Add(new ControllerInfo(ControllerIDs.Customs.JP.AFRPluggedIntoConsol, "Enterprise.Customs.JP.AFR.Module", "Enterprise.Customs.JP.AFR.Module.JPAFRConsolController"));
			Add(new ControllerInfo(ControllerIDs.Customs.JP.AFRBill, "Enterprise.Customs.JP.AFR.Module", "Enterprise.Customs.JP.AFR.Module.JPAFRBillsController"));
			Add(new ControllerInfo(ControllerIDs.Customs.ASYCUDA.ASYCUDAManifest, "Enterprise.Customs.ASYCUDA.Module", "Enterprise.Customs.ASYCUDA.Module.ASYCUDAManifestController"));
			Add(new ControllerInfo(ControllerIDs.Customs.ASYCUDA.ASYCUDAManifestBill, "Enterprise.Customs.ASYCUDA.Module", "Enterprise.Customs.ASYCUDA.Module.ASYCUDAManifestBillController"));
			Add(new ControllerInfo(ControllerIDs.Customs.ASYCUDA.ASYCUDAManifestConsol, "Enterprise.Customs.ASYCUDA.Module", "Enterprise.Customs.ASYCUDA.Module.ASYCUDAManifestConsolController"));
			Add(new ControllerInfo(ControllerIDs.Customs.ASYCUDA.ASYCUDAPreBoardingNotification, "Enterprise.Customs.ASYCUDA.Module", "Enterprise.Customs.ASYCUDA.Module.AsycudaPreBoardingNotificationController"));
			Add(new ControllerInfo(ControllerIDs.Customs.ASYCUDA.SGAccess.Manifest, "Enterprise.Customs.SG.Access.GUI", "Enterprise.Customs.SG.Access.GUI.ManifestController"));
			Add(new ControllerInfo(ControllerIDs.Customs.ASYCUDA.SGAccess.ManifestBill, "Enterprise.Customs.SG.Access.GUI", "Enterprise.Customs.SG.Access.GUI.ManifestBillController"));
			Add(new ControllerInfo(ControllerIDs.Customs.Universal.RefCusTariff, "Enterprise.Customs.Universal.Module", "Enterprise.Customs.Universal.Module.RefCusTariffController"));
			Add(new ControllerInfo(ControllerIDs.Customs.Universal.ZZRefCarrier, "Enterprise.Customs.Universal.Module", "Enterprise.Customs.Universal.Module.ZZRefCarrierController"));
			Add(new ControllerInfo(ControllerIDs.Customs.Universal.ZZRefCusCodeList, "Enterprise.Customs.Universal.Module", "Enterprise.Customs.Universal.Module.ZZRefCusCodeListController"));
			Add(new ControllerInfo(ControllerIDs.Customs.Universal.ZZRefCusMap, "Enterprise.Customs.Universal.Module", "Enterprise.Customs.Universal.Module.ZZRefCusMapController"));
			Add(new ControllerInfo(ControllerIDs.Customs.Universal.ZZRefCusProcedure, "Enterprise.Customs.Universal.Module", "Enterprise.Customs.Universal.Module.ZZRefCusProcedureController"));
			Add(new ControllerInfo(ControllerIDs.Customs.Universal.ZZRefCusRuling, "Enterprise.Customs.Universal.Module", "Enterprise.Customs.Universal.Module.ZZRefCusRulingController"));
			Add(new ControllerInfo(ControllerIDs.Customs.Universal.RefHarbourRate, "Enterprise.Customs.Universal.Module", "Enterprise.Customs.Universal.Module.RefHarbourRateController"));
			Add(new ControllerInfo(ControllerIDs.Customs.Universal.RefDataGrouping, "Enterprise.Customs.Universal.Module", "Enterprise.Customs.Universal.Module.RefDataGroupingController"));
			Add(new ControllerInfo(ControllerIDs.Customs.Universal.RefCusTradeGroup, "Enterprise.Customs.Universal.Module", "Enterprise.Customs.Universal.Module.RefCusTradeGroupController"));
			Add(new ControllerInfo(ControllerIDs.Customs.CusRefPreference, "Enterprise.Customs.Module", "Enterprise.Customs.Module.CusRefPreferenceController"));
			Add(new ControllerInfo(ControllerIDs.Customs.CusRefRateCode, "Enterprise.Customs.Module", "Enterprise.Customs.Module.CusRefRateCodeController"));
			Add(new ControllerInfo(ControllerIDs.Customs.Permits, "Enterprise.Customs.Module", "Enterprise.Customs.Module.CusPermitController"));
			Add(new ControllerInfo(ControllerIDs.Customs.Guarantees, "Enterprise.Customs.Module", "Enterprise.Customs.Module.GuaranteesController"));
			Add(new ControllerInfo(ControllerIDs.Customs.CustomsRules, "Enterprise.Customs.Module", "Enterprise.Customs.Module.CustomsRulesController"));
			Add(new ControllerInfo(ControllerIDs.Customs.BaseAirCargo, "Enterprise.Customs.Module", "Enterprise.Customs.Module.BaseAirCargoController"));
			Add(new ControllerInfo(ControllerIDs.CusPerson, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.CusPersonController"));
			Add(new ControllerInfo(ControllerIDs.GroupCredentialsPlugIn, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.GroupCredentialsPlugInController"));
			Add(new ControllerInfo(ControllerIDs.CompanyCredentialsPlugIn, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.CompanyCredentialsPlugInController"));
			Add(new ControllerInfo(ControllerIDs.BorderWiseWebReturnHook, "Enterprise.Customs.Module", "Enterprise.Customs.Module.BorderWiseWebReturnHookController"));
			Add(new ControllerInfo(ControllerIDs.Customs.BaseSeaCargo, "Enterprise.Customs.Module", "Enterprise.Customs.Module.BaseSeaCargoController"));
			Add(new ControllerInfo(ControllerIDs.Customs.CusAuthorisations, "Enterprise.Customs.Module", "Enterprise.Customs.Module.CusAuthorisationsController"));
			Add(new ControllerInfo(ControllerIDs.Customs.CusPackingList, "Enterprise.Customs.Module", "Enterprise.Customs.Module.CusPackingListController"));
			Add(new ControllerInfo(ControllerIDs.Customs.TradeGroups, "Enterprise.Customs.Module", "Enterprise.Customs.Module.TradeGroupsController"));
			Add(new ControllerInfo(ControllerIDs.Customs.CusRefTariffVersion, "Enterprise.Customs.Module", "Enterprise.Customs.Module.CusRefTariffVersionController"));
			Add(new ControllerInfo(ControllerIDs.Customs.CustomsStatement, "Enterprise.Customs.Module", "Enterprise.Customs.Module.StatementController"));
			Add(new ControllerInfo(ControllerIDs.Customs.GoodsCatalog, "Enterprise.Customs.Module", "Enterprise.Customs.Module.GoodsCatalogController"));
			Add(new ControllerInfo(ControllerIDs.Customs.SumARegister, "Enterprise.Customs.EFTA.TemporaryStorageRegister.Module", "Enterprise.Customs.EFTA.TemporaryStorageRegister.Module.SumARegisterController"));
			Add(new ControllerInfo(ControllerIDs.Customs.SumARegisterReadOnly, "Enterprise.Customs.EFTA.TemporaryStorageRegister.Module", "Enterprise.Customs.EFTA.TemporaryStorageRegister.Module.SumARegisterReadOnlyController"));

			// AU Customs Controllers
			Add(new ControllerInfo(ControllerIDs.Customs.SupplierPart, "Enterprise.Customs.AU.Module", "Enterprise.Customs.AU.Module.OrgSupplierPartController", "AU"));
			Add(new ControllerInfo(ControllerIDs.Customs.JobDeclaration, "Enterprise.Customs.AU.Module", "Enterprise.Customs.AU.Module.JobDeclarationController", "AU"));
			Add(new ControllerInfo(ControllerIDs.Customs.JobDeclarationPluggedIntoShipment, "Enterprise.Customs.AU.Module", "Enterprise.Customs.AU.Module.JobDeclarationShipmentController", "AU"));
			Add(new ControllerInfo(ControllerIDs.Customs.ImportClassification, "Enterprise.Customs.AU.Module", "Enterprise.Customs.AU.Module.ImportClassificationController", "AU"));
			Add(new ControllerInfo(ControllerIDs.Customs.ExportClassification, "Enterprise.Customs.AU.Module", "Enterprise.Customs.AU.Module.ExportClassificationController", "AU"));
			Add(new ControllerInfo(ControllerIDs.Customs.AU.ImportTariffBulkChange, "Enterprise.Customs.AU.Module", "Enterprise.Customs.AU.Module.AUImportTariffBulkChangeController", "AU"));
			Add(new ControllerInfo(ControllerIDs.Customs.AU.ExportTariffBulkChange, "Enterprise.Customs.AU.Module", "Enterprise.Customs.AU.Module.AUExportTariffBulkChangeController", "AU"));
			Add(new ControllerInfo(ControllerIDs.Customs.AU.AirCargo, "Enterprise.Customs.AU.Module", "Enterprise.Customs.AU.Module.AirCargo.AUCustomsAirCargoController", "AU"));
			Add(new ControllerInfo(ControllerIDs.Customs.AU.AirCargoDepot, "Enterprise.Customs.AU.Module", "Enterprise.Customs.AU.Module.AirCargo.AirCargoDepotStandAloneController", "AU"));
			Add(new ControllerInfo(ControllerIDs.Customs.AU.AirCargoShipmentController, "Enterprise.Customs.AU.Module", "Enterprise.Customs.AU.Module.AirCargo.AUCustomsAirCargoShipmentController", "AU"));
			Add(new ControllerInfo(ControllerIDs.Customs.AU.AirCargoConsolController, "Enterprise.Customs.AU.Module", "Enterprise.Customs.AU.Module.AirCargo.AUCustomsAirCargoConsolController", "AU"));
			Add(new ControllerInfo(ControllerIDs.Customs.AU.SeaCargo, "Enterprise.Customs.AU.Module", "Enterprise.Customs.AU.Module.SeaCargo.AUCustomsSeaCargoController", "AU"));
			Add(new ControllerInfo(ControllerIDs.Customs.AU.SeaCargoDepot, "Enterprise.Customs.AU.Module", "Enterprise.Customs.AU.Module.SeaCargo.AUCustomsSCDController", "AU"));
			Add(new ControllerInfo(ControllerIDs.Customs.AU.CusSCADepotContainer, "Enterprise.Customs.AU.Module", "Enterprise.Customs.AU.Module.SeaCargo.CusSCADepotContainerController", "AU"));
			Add(new ControllerInfo(ControllerIDs.Customs.AU.CusSCADepotHouse, "Enterprise.Customs.AU.Module", "Enterprise.Customs.AU.Module.SeaCargo.CusSCADepotHouseController", "AU"));
			Add(new ControllerInfo(ControllerIDs.Customs.AU.HouseAirCargo, "Enterprise.Customs.AU.Module", "Enterprise.Customs.AU.Module.AirCargo.AUCustomsHouseAirCargoController", "AU"));
			Add(new ControllerInfo(ControllerIDs.Customs.CargoManifestPlugInForConsol, "Enterprise.Customs.AU.Declaration.GUI", "Enterprise.Customs.AU.Declaration.GUI.PlugIn.AUCustomsExit2PluginToConsolController", "AU"));
			Add(new ControllerInfo(ControllerIDs.Customs.AU.ExportCustomsManifest, "Enterprise.Customs.AU.Module", "Enterprise.Customs.AU.Module.ExportCustomsManifestController", "AU"));
			Add(new ControllerInfo(ControllerIDs.CommercialInvoice, "Enterprise.Customs.AU.Module", "Enterprise.Customs.AU.Module.CommercialInvoiceController", "AU"));
			Add(new ControllerInfo(ControllerIDs.Customs.AU.CusHAWBCusUnderbondPluginController, "Enterprise.Customs.AU.Module", "Enterprise.Customs.AU.Module.CusHAWBCusUnderbondPluginController", "AU"));
			Add(new ControllerInfo(ControllerIDs.Customs.AU.AirCTOImport, "Enterprise.Customs.AU.Module", "Enterprise.Customs.AU.Module.AirCTOImportController", "AU"));
			Add(new ControllerInfo(ControllerIDs.Customs.AU.AirCTOHawbImport, "Enterprise.Customs.AU.Module", "Enterprise.Customs.AU.Module.AirCTOHawbImportController", "AU"));
			Add(new ControllerInfo(ControllerIDs.Customs.AU.AirCTOExport, "Enterprise.Customs.AU.Module", "Enterprise.Customs.AU.Module.AirCTOExportController", "AU"));
			Add(new ControllerInfo(ControllerIDs.Customs.AU.AirCTOHawbExport, "Enterprise.Customs.AU.Module", "Enterprise.Customs.AU.Module.AirCTOHawbExportController", "AU"));
			Add(new ControllerInfo(ControllerIDs.Customs.AU.CusSCAHouseCusUnderbondPluginController, "Enterprise.Customs.AU.Module", "Enterprise.Customs.AU.Module.CusSCAHouseCusUnderbondPluginController", "AU"));
			Add(new ControllerInfo(ControllerIDs.Customs.AU.VoyageManifest, "Enterprise.Customs.AU.Module", "Enterprise.Customs.AU.Module.VoyageManifestController", "AU"));
			Add(new ControllerInfo(ControllerIDs.SailingScheduleImporting, "Enterprise.Freight.Module", "Enterprise.Freight.SailingDataVendor.Module.SailingScheduleImportingController"));
			Add(new ControllerInfo(ControllerIDs.OnlineSailingSchedules, "Enterprise.Freight.Module", "Enterprise.Freight.Module.OnlineSailingSchedulesController"));
			Add(new ControllerInfo(ControllerIDs.Customs.AU.CusSeaManOBLDetailCusUnderbondPluginController, "Enterprise.Customs.AU.Module", "Enterprise.Customs.AU.Module.CusSeaManOBLDetailCusUnderbondPluginController", "AU"));
			Add(new ControllerInfo(ControllerIDs.Customs.AU.AirCargoCusUnderbondPluginController, "Enterprise.Customs.AU.Module", "Enterprise.Customs.AU.Module.AirCargoCusUnderbondPluginController", "AU"));
			Add(new ControllerInfo(ControllerIDs.Customs.AU.AirCTOCusUnderbondPluginController, "Enterprise.Customs.AU.Module", "Enterprise.Customs.AU.Module.AirCTOCusUnderbondPluginController", "AU"));
			Add(new ControllerInfo(ControllerIDs.Customs.AU.AirCargoDeclarationCusUnderbondController, "Enterprise.Customs.AU.Module", "Enterprise.Customs.AU.Module.AirCargoDeclarationCusUnderbondController", "AU"));
			Add(new ControllerInfo(ControllerIDs.Customs.AU.AirCargoOutturnBillsController, "Enterprise.Customs.AU.Module", "Enterprise.Customs.AU.Module.AirCargoOutturnBillsController", "AU"));
			Add(new ControllerInfo(ControllerIDs.Customs.AU.ArrivalPluginToSailingController, "Enterprise.Customs.AU.Module", "Enterprise.Customs.AU.Module.Sailing.ArrivalPluginToSailingController", "AU"));
			Add(new ControllerInfo(ControllerIDs.Customs.AU.CusSCAContainerUnderbondPluginController, "Enterprise.Customs.AU.Module", "Enterprise.Customs.AU.Module.CusSCAContainerUnderbondPluginController", "AU"));
			Add(new ControllerInfo(ControllerIDs.Customs.AU.SeaCargoStandAloneController, "Enterprise.Customs.AU.Module", "Enterprise.Customs.AU.Module.SeaCargoStandAloneController", "AU"));
			Add(new ControllerInfo(ControllerIDs.Customs.AU.SeaCargoDepotStandAloneController, "Enterprise.Customs.AU.Module", "Enterprise.Customs.AU.Module.SeaCargoDepotStandAloneController", "AU"));
			Add(new ControllerInfo(ControllerIDs.Customs.AU.SeaCargoHouseController, "Enterprise.Customs.AU.Module", "Enterprise.Customs.AU.Module.SeaCargoHouseController", "AU"));
			Add(new ControllerInfo(ControllerIDs.Customs.AU.SeaCargoHouseShipmentController, "Enterprise.Customs.AU.Module", "Enterprise.Customs.AU.Module.SeaCargoHouseShipmentController", "AU"));
			Add(new ControllerInfo(ControllerIDs.Customs.AU.SeaCargoOutturnBillsController, "Enterprise.Customs.AU.Module", "Enterprise.Customs.AU.Module.SeaCargoOutturnBillsController", "AU"));
			Add(new ControllerInfo(ControllerIDs.Customs.AU.AQISProducerCode, "Enterprise.Customs.AU.Module", "Enterprise.Customs.AU.Module.AQISProducerCodeController", "AU"));
			Add(new ControllerInfo(ControllerIDs.Customs.AU.Premises, "Enterprise.Customs.AU.Module", "Enterprise.Customs.AU.Module.PremisesController", "AU"));
			Add(new ControllerInfo(ControllerIDs.Customs.AU.CMRCodeLists, "Enterprise.Customs.AU.Module", "Enterprise.Customs.AU.Module.CMRCodeListsController", "AU"));
			Add(new ControllerInfo(ControllerIDs.Customs.AU.InstrumentNumber, "Enterprise.Customs.AU.Module", "Enterprise.Customs.AU.Module.InstrumentNumberController", "AU"));
			Add(new ControllerInfo(ControllerIDs.Customs.AU.CMRLodgementQuestion, "Enterprise.Customs.AU.Module", "Enterprise.Customs.AU.Module.CMRLodgementQuestionController", "AU"));
			Add(new ControllerInfo(ControllerIDs.Customs.AU.ManifestPluginToSailingController, "Enterprise.Customs.AU.Module", "Enterprise.Customs.AU.Module.ManifestPluginToSailingController", "AU"));
			Add(new ControllerInfo(ControllerIDs.Customs.SendTestCustomsMessage, "Enterprise.Customs.AU.Module", "Enterprise.Customs.AU.Module.AUSendTestCustomsMessageController", "AU"));
			Add(new ControllerInfo(ControllerIDs.Customs.AU.OrganisationCustomsMessaging, "Enterprise.Customs.AU.Module", "Enterprise.Customs.AU.Module.AUOrganisationController", "AU"));
			Add(new ControllerInfo(ControllerIDs.Customs.AU.CMREstablishmentCodes, "Enterprise.Customs.AU.Module", "Enterprise.Customs.AU.Module.CMREstablishmentCodesController", "AU"));
			Add(new ControllerInfo(ControllerIDs.Customs.AU.NEXDOCNotificationController, "Enterprise.Customs.AU.Module", "Enterprise.Customs.AU.Module.NEXDOCNotificationController", "AU"));
			Add(new ControllerInfo(ControllerIDs.Customs.ConsolidatedDeclaration, "Enterprise.Customs.AU.Module", "Enterprise.Customs.AU.Module.ConsolidatedDeclarationController", Constants.CountryCodes.Australia));
			Add(new ControllerInfo(ControllerIDs.Customs.CusCalculationRules, "Enterprise.Customs.AU.Module", "Enterprise.Customs.AU.Module.CusCalculationRulesController", Constants.CountryCodes.Australia));

			foreach (var country in ObjectFactory.Get<Enterprise.Integration.Customs.Shared.IAsycudaCustomsCountryProvider>().GetAsycudaCustomsCountryCodes())
			{
				Add(new ControllerInfo(ControllerIDs.Customs.JobDeclaration, "Enterprise.Customs.AsycudaCustoms.Module", "Enterprise.Customs.AsycudaCustoms.Module.JobDeclarationController", country));
				Add(new ControllerInfo(ControllerIDs.Customs.SupplierPart, "Enterprise.Customs.AsycudaCustoms.Module", "Enterprise.Customs.AsycudaCustoms.Module.OrgSupplierPartController", country));
				Add(new ControllerInfo(ControllerIDs.Customs.JobDeclarationPluggedIntoShipment, "Enterprise.Customs.AsycudaCustoms.Module", "Enterprise.Customs.AsycudaCustoms.Module.JobDeclarationShipmentController", country));
				Add(new ControllerInfo(ControllerIDs.Customs.SingleTariffClassification, "Enterprise.Customs.AsycudaCustoms.Module", "Enterprise.Customs.AsycudaCustoms.Module.CusClassificationController", country));
				Add(new ControllerInfo(ControllerIDs.CommercialInvoice, "Enterprise.Customs.AsycudaCustoms.Module", "Enterprise.Customs.AsycudaCustoms.Module.CommercialInvoiceController", country));
			}

			// EU Customs Controllers
			foreach (var jurisdiction in ObjectFactory.Get<Enterprise.Integration.Customs.Shared.IEuropeanUnionCustomsMembersProvider>().GetEuropeanUnionForCustomsMembersAndCustomsUnionAdditionalMembers().Select(countryCode => CountryCodes.GetCustomsCountryOfJurisdiction(countryCode)).Distinct())
			{
				var shouldAddJobDeclaration = !ModuleList.EuCountriesThatHaveTheirOwnJobDeclarationModule.Contains(jurisdiction);
				var shouldAddCommercialInvoice = !ModuleList.EuCountriesThatHaveTheirOwnCommercialInvoiceModule.Contains(jurisdiction);
				var shouldAddSupplierPart = !ModuleList.EuCountriesThatHaveTheirOwnSupplierPartModule.Contains(jurisdiction);
				var shouldAddPermits = !ModuleList.EuCountriesThatHaveTheirOwnPermitModule.Contains(jurisdiction);
				var shouldAddGuarantees = !ModuleList.EuCountriesThatHaveTheirOwnGuaranteeModule.Contains(jurisdiction);
				var shouldAddUCC6TemporaryStorage = !EuCountriesThatHaveTheirOwnUCC6TemporaryStorageController.Contains(jurisdiction);
				var shouldAddTempStorageRegistry = !ModuleList.EuCountriesThatHaveTheirOwnTempStorageRegistryModule.Contains(jurisdiction);
				var shouldAddTempStoragePremises = !EuCountriesThatHaveTheirOwnTemporaryStoragePremisesController.Contains(jurisdiction);

				foreach (var country in CountryCodes.GetCountriesAndTerritoriesBelongingToCustomsJurisdiction(jurisdiction))
				{
					if (shouldAddJobDeclaration)
					{
						Add(new ControllerInfo(ControllerIDs.Customs.JobDeclaration, "Enterprise.Customs.EU.Module", "Enterprise.Customs.EU.Module.JobDeclarationController", country));
					}
					if (shouldAddCommercialInvoice)
					{
						Add(new ControllerInfo(ControllerIDs.CommercialInvoice, "Enterprise.Customs.EU.Module", "Enterprise.Customs.EU.Module.CommercialInvoiceController", country));
					}
					if (shouldAddSupplierPart)
					{
						Add(new ControllerInfo(ControllerIDs.Customs.SupplierPart, "Enterprise.Customs.EU.Module", "Enterprise.Customs.EU.Module.OrgSupplierPartController", country));
					}
					Add(new ControllerInfo(ControllerIDs.Customs.SingleTariffClassification, "Enterprise.Customs.EU.Module", "Enterprise.Customs.EU.Module.CusClassificationController", country));
					if (shouldAddPermits)
					{
						Add(new ControllerInfo(ControllerIDs.Customs.Permits, "Enterprise.Customs.EU.Module", "Enterprise.Customs.EU.Module.CusPermitController", country));
					}
					if (shouldAddGuarantees)
					{
						Add(new ControllerInfo(ControllerIDs.Customs.Guarantees, "Enterprise.Customs.EU.Module", "Enterprise.Customs.EU.Module.GuaranteesController", country));
					}
					if (shouldAddUCC6TemporaryStorage)
					{
						Add(new ControllerInfo(ControllerIDs.Customs.EU.UCC6TemporaryStorage, "Enterprise.Customs.EU.TemporaryStorage.Module", "Enterprise.Customs.EU.TemporaryStorage.Module.UCC6TemporaryStorageController", country));
					}
					if (shouldAddTempStorageRegistry)
					{
						Add(new ControllerInfo(ControllerIDs.Customs.EU.TempStorageRegister, "Enterprise.Customs.EU.TemporaryStorage.Module", "Enterprise.Customs.EU.TemporaryStorage.Module.TempStorageRegisterController", country));
					}
					if (shouldAddTempStoragePremises)
					{
						Add(new ControllerInfo(ControllerIDs.Customs.EU.TempStoragePremises, "Enterprise.Customs.EU.TemporaryStorage.Module", "Enterprise.Customs.EU.TemporaryStorage.Module.TempStoragePremisesController", country));
					}
					Add(new ControllerInfo(ControllerIDs.Customs.CusCalculationRules, "Enterprise.Customs.EU.Module", "Enterprise.Customs.EU.Module.CusCalculationRulesController", country));
				}
			}

			Add(new ControllerInfo(ControllerIDs.Customs.EU.EMCS, "Enterprise.Customs.EU.EMCS.Module", "Enterprise.Customs.EU.EMCS.Module.Controller"));
			Add(new ControllerInfo(ControllerIDs.Customs.EU.IntrastatReports, "Enterprise.Customs.EU.Intrastat.Module", "Enterprise.Customs.EU.Intrastat.Module.IntrastatReportsController"));
			Add(new ControllerInfo(ControllerIDs.Customs.EU.NctsMovementController, "Enterprise.Customs.EU.NCTS.Module", "Enterprise.Customs.EU.NCTS.Module.NctsMovementController"));
			Add(new ControllerInfo(ControllerIDs.Customs.EU.NctsMovementInShipmentController, "Enterprise.Customs.EU.NCTS.Module", "Enterprise.Customs.EU.NCTS.Module.NctsMovementInShipmentController"));
			Add(new ControllerInfo(ControllerIDs.Customs.EU.NctsMovementInConsolController, "Enterprise.Customs.EU.NCTS.Module", "Enterprise.Customs.EU.NCTS.Module.NctsMovementInConsolController"));
			Add(new ControllerInfo(ControllerIDs.Customs.EU.NctsOrganisationDetailsPlugIn, "Enterprise.Customs.EU.NCTS.Module", "Enterprise.Customs.EU.NCTS.Module.NctsOrganisationDetailsPlugInController"));
			Add(new ControllerInfo(ControllerIDs.Customs.JobDeclaration, "Enterprise.Customs.IT.Module", "Enterprise.Customs.IT.Module.JobDeclarationController", Constants.CountryCodes.Italy));
			Add(new ControllerInfo(ControllerIDs.Customs.EU.OrganisationConsigneePlugIn, "Enterprise.Customs.EU.Module", "Enterprise.Customs.EU.Module.EUOrganisationConsigneePlugInController"));
			Add(new ControllerInfo(ControllerIDs.Customs.EU.OrganisationConsignorPlugIn, "Enterprise.Customs.EU.Module", "Enterprise.Customs.EU.Module.EUOrganisationConsignorPlugInController"));
			Add(new ControllerInfo(ControllerIDs.Customs.SupplierPart, "Enterprise.Customs.IT.Module", "Enterprise.Customs.IT.Module.OrgSupplierPartController", Constants.CountryCodes.Italy));
			Add(new ControllerInfo(ControllerIDs.Customs.EU.NctsMovementController, "Enterprise.Customs.IT.NCTS.Module", "Enterprise.Customs.IT.NCTS.Module.NctsMovementController", Constants.CountryCodes.Italy));
			Add(new ControllerInfo(ControllerIDs.Customs.EU.ExitSummaryController, "Enterprise.Customs.EU.Module", "Enterprise.Customs.EU.Module.ExitSummaryController"));
			Add(new ControllerInfo(ControllerIDs.Customs.EU.ExitControl, "Enterprise.Customs.EU.ExitControl.Module", "Enterprise.Customs.EU.ExitControl.Module.ExitControlController"));
			Add(new ControllerInfo(ControllerIDs.Customs.EU.ExitControlReport, "Enterprise.Customs.EU.ExitControl.Module", "Enterprise.Customs.EU.ExitControl.Module.ExitControlReportController"));
			Add(new ControllerInfo(ControllerIDs.Customs.EU.EUH7, "Enterprise.Customs.EU.H7.Module", "Enterprise.Customs.EU.H7.Module.EUH7Controller"));
			Add(new ControllerInfo(ControllerIDs.Customs.EU.EUH7Bill, "Enterprise.Customs.EU.H7.Module", "Enterprise.Customs.EU.H7.Module.EUH7BillController"));
			foreach (var countryCode in Constants.CountryCodes.FranceAndOverseasDepartmentsUnderItsCustomsJurisdiction)
			{
				Add(new ControllerInfo(ControllerIDs.Customs.EU.NctsMovementController, "Enterprise.Customs.FR.Module", "Enterprise.Customs.FR.Module.NctsMovementController", countryCode));
			}
			Add(new ControllerInfo(ControllerIDs.Customs.EU.IntrastatTransactionsController, "Enterprise.Customs.EU.Intrastat.Module", "Enterprise.Customs.EU.Intrastat.Module.IntrastatTransactionsController"));

			//GB Customs
			Add(new ControllerInfo(ControllerIDs.CommercialInvoice, "Enterprise.Customs.GB.Module", "Enterprise.Customs.GB.Module.CommercialInvoiceController", Constants.CountryCodes.UnitedKingdom));
			Add(new ControllerInfo(ControllerIDs.Customs.JobDeclaration, "Enterprise.Customs.GB.Module", "Enterprise.Customs.GB.Module.JobDeclarationController", Constants.CountryCodes.UnitedKingdom));
			Add(new ControllerInfo(ControllerIDs.Customs.JobDeclarationPluggedIntoShipment, "Enterprise.Customs.EU.Module", "Enterprise.Customs.EU.Module.JobDeclarationShipmentController", Constants.CountryCodes.UnitedKingdom));
			Add(new ControllerInfo(ControllerIDs.Customs.GB.OrganisationConsigneePlugIn, "Enterprise.Customs.GB.Module", "Enterprise.Customs.GB.Module.GBOrganisationConsigneePlugInController"));
			Add(new ControllerInfo(ControllerIDs.Customs.GB.CcsukGenralMessage, "Enterprise.Customs.GB.Module", "Enterprise.Customs.GB.Module.CcsukGenralMessageController", Constants.CountryCodes.UnitedKingdom));
			Add(new ControllerInfo(ControllerIDs.Customs.GB.CcsukAirInventory, "Enterprise.Customs.GB.Module", "Enterprise.Customs.GB.Module.CcsukAirInventoryController", Constants.CountryCodes.UnitedKingdom));
			Add(new ControllerInfo(ControllerIDs.Customs.GB.CcsukAirInventoryUFO, "Enterprise.Customs.GB.Module", "Enterprise.Customs.GB.Module.CcsukAirInventoryControllerUFO", Constants.CountryCodes.UnitedKingdom));
			Add(new ControllerInfo(ControllerIDs.Customs.GB.CcsukAirInventoryInConsol, "Enterprise.Customs.GB.Module", "Enterprise.Customs.GB.Module.CcsukAirInventoryInConsolController", Constants.CountryCodes.UnitedKingdom));
			Add(new ControllerInfo(ControllerIDs.Customs.GB.CcsukAirInventoryHouse, "Enterprise.Customs.GB.Module", "Enterprise.Customs.GB.Module.CcsukAirInventoryHouseController", Constants.CountryCodes.UnitedKingdom));
			Add(new ControllerInfo(ControllerIDs.Customs.GB.CcsukAirInventoryHouseInShipment, "Enterprise.Customs.GB.Module", "Enterprise.Customs.GB.Module.CcsukAirInventoryHouseInShipmentController", Constants.CountryCodes.UnitedKingdom));
			Add(new ControllerInfo(ControllerIDs.Customs.GB.CcsukMasterAndHouseCombined, "Enterprise.Customs.GB.Module", "Enterprise.Customs.GB.Module.CcsukMasterAndHouseCombinedController", Constants.CountryCodes.UnitedKingdom));
			Add(new ControllerInfo(ControllerIDs.Customs.GB.CcsukStandAloneFsrEnquiry, "Enterprise.Customs.GB.Module", "Enterprise.Customs.GB.Module.StandAloneFsrEnquiry.StandAloneFsrEnquiryController", Constants.CountryCodes.UnitedKingdom));
			Add(new ControllerInfo(ControllerIDs.Customs.GB.DLUController, "Enterprise.Customs.GB.Module", "Enterprise.Customs.GB.Module.DLU.DLUMessageController", Constants.CountryCodes.UnitedKingdom));
			Add(new ControllerInfo(ControllerIDs.Customs.GB.CcsukSplitBasicController, "Enterprise.Customs.GB.Module", "Enterprise.Customs.GB.Module.CcsukSplitBasicController", Constants.CountryCodes.UnitedKingdom));
			Add(new ControllerInfo(ControllerIDs.Customs.GB.CcsukSplitHouseController, "Enterprise.Customs.GB.Module", "Enterprise.Customs.GB.Module.CcsukSplitHouseController", Constants.CountryCodes.UnitedKingdom));
			Add(new ControllerInfo(ControllerIDs.Customs.GB.ChiefExportConsolIntegrationController, "Enterprise.Customs.GB.Module", "Enterprise.Customs.GB.Module.ChiefExportConsolIntegration.ChiefExportConsolIntegrationController", Constants.CountryCodes.UnitedKingdom));
			Add(new ControllerInfo(ControllerIDs.Customs.SupplierPart, "Enterprise.Customs.GB.Module", "Enterprise.Customs.GB.Module.OrgSupplierPartController", Constants.CountryCodes.UnitedKingdom));
			Add(new ControllerInfo(ControllerIDs.Customs.Permits, "Enterprise.Customs.GB.Module", "Enterprise.Customs.GB.Module.CusPermitController", Constants.CountryCodes.UnitedKingdom));
			Add(new ControllerInfo(ControllerIDs.Customs.GB.CDSDISQueryController, "Enterprise.Customs.GB.Module", "Enterprise.Customs.GB.Module.CDSDISQueryController", Constants.CountryCodes.UnitedKingdom));
			Add(new ControllerInfo(ControllerIDs.Customs.GB.CDSCashPaymentsController, "Enterprise.Customs.GB.Module", "Enterprise.Customs.GB.Module.CDSCashPaymentsController", Constants.CountryCodes.UnitedKingdom));
			Add(new ControllerInfo(ControllerIDs.Customs.GB.EntryHeader, "Enterprise.Customs.GB.Module", "Enterprise.Customs.GB.Module.EntryHeaderController", Constants.CountryCodes.UnitedKingdom));
			Add(new ControllerInfo(ControllerIDs.Customs.GB.H7Bill, "Enterprise.Customs.GB.H7.Module", "Enterprise.Customs.GB.H7.Module.GBH7BillController", Constants.CountryCodes.UnitedKingdom));
			Add(new ControllerInfo(ControllerIDs.Customs.GB.OrganisationCustomsMessaging, "Enterprise.Customs.GB.Module", "Enterprise.Customs.GB.Module.GBOrganisationController", Constants.CountryCodes.UnitedKingdom));

			// CA Customs Controllers
			Add(new ControllerInfo(ControllerIDs.CommercialInvoice, "Enterprise.Customs.CA.Module", "Enterprise.Customs.CA.Module.CommercialInvoiceController", Constants.CountryCodes.Canada));
			Add(new ControllerInfo(ControllerIDs.Customs.JobDeclaration, "Enterprise.Customs.CA.Module", "Enterprise.Customs.CA.Module.JobDeclarationController", Constants.CountryCodes.Canada));
			Add(new ControllerInfo(ControllerIDs.Customs.JobDeclarationPluggedIntoShipment, "Enterprise.Customs.CA.Module", "Enterprise.Customs.CA.Module.JobDeclarationShipmentController", Constants.CountryCodes.Canada));
			Add(new ControllerInfo(ControllerIDs.Customs.SupplierPart, "Enterprise.Customs.CA.Module", "Enterprise.Customs.CA.Module.OrgSupplierPartController", Constants.CountryCodes.Canada));
			Add(new ControllerInfo(ControllerIDs.Customs.CA.HTSTariffBulkChange, "Enterprise.Customs.CA.Module", "Enterprise.Customs.CA.Module.CAHTSTariffBulkChangeController", Constants.CountryCodes.Canada));
			Add(new ControllerInfo(ControllerIDs.Customs.CA.CAShipmentCargoReport, "Enterprise.Customs.CA.Module", "Enterprise.Customs.CA.Module.CAShipmentCargoReportController"));
			Add(new ControllerInfo(ControllerIDs.Customs.CA.CAConsolACI, "Enterprise.Customs.CA.Module", "Enterprise.Customs.CA.Module.CAConsolACIController"));
			Add(new ControllerInfo(ControllerIDs.Customs.CA.CAQueryMessages, "Enterprise.Customs.CA.Module", "Enterprise.Customs.CA.Module.CAQueryMessagesController", Constants.CountryCodes.Canada));
			Add(new ControllerInfo(ControllerIDs.Customs.CA.K84Reports, "Enterprise.Customs.CA.Module", "Enterprise.Customs.CA.Module.K84ReportsController", Constants.CountryCodes.Canada));
			Add(new ControllerInfo(ControllerIDs.Customs.CA.CAReleaseNotifications, "Enterprise.Customs.CA.Module", "Enterprise.Customs.CA.Module.CAReleaseNotificationsController", Constants.CountryCodes.Canada));
			Add(new ControllerInfo(ControllerIDs.Customs.CA.CACusClassification, "Enterprise.Customs.CA.Module", "Enterprise.Customs.CA.Module.CusClassificationController", Constants.CountryCodes.Canada));
			Add(new ControllerInfo(ControllerIDs.Customs.CA.RNSShipmentPlugIn, "Enterprise.Customs.CA.Module", "Enterprise.Customs.CA.Module.RNSShipmentPlugInController", Constants.CountryCodes.Canada));
			Add(new ControllerInfo(ControllerIDs.Customs.CA.RNSConsolPlugIn, "Enterprise.Customs.CA.Module", "Enterprise.Customs.CA.Module.RNSConsolPlugInController", Constants.CountryCodes.Canada));
			Add(new ControllerInfo(ControllerIDs.Customs.CA.OrganisationConsigneePlugIn, "Enterprise.Customs.CA.Module", "Enterprise.Customs.CA.Module.CAOrganisationConsigneeController", Constants.CountryCodes.Canada));
			Add(new ControllerInfo(ControllerIDs.Customs.CA.OrganisationDetailsPlugIn, "Enterprise.Customs.CA.Module", "Enterprise.Customs.CA.Module.CAOrganisationDetailsController"));
			Add(new ControllerInfo(ControllerIDs.Customs.CA.CATransactionNumberSetting, "Enterprise.Customs.CA.Module", "Enterprise.Customs.CA.Module.TransactionNumberSettingController", Constants.CountryCodes.Canada));
			Add(new ControllerInfo(ControllerIDs.Customs.CA.B2Adjustments, "Enterprise.Customs.CA.Module", "Enterprise.Customs.CA.Module.B2AdjustmentsController", Constants.CountryCodes.Canada));
			Add(new ControllerInfo(ControllerIDs.Customs.CA.CAHouseBilleManifest, "Enterprise.Customs.CA.Module", "Enterprise.Customs.CA.Module.HouseBilleManifestController"));
			Add(new ControllerInfo(ControllerIDs.Customs.CA.CAConsoleManifest, "Enterprise.Customs.CA.Module", "Enterprise.Customs.CA.Module.ConsolCusCAeMHController"));
			Add(new ControllerInfo(ControllerIDs.Customs.CA.CAManifestForward, "Enterprise.Customs.CA.Module", "Enterprise.Customs.CA.Module.ManifestForwardController", Constants.CountryCodes.Canada));
			Add(new ControllerInfo(ControllerIDs.Customs.CA.RNSCFSShipmentPlugIn, "Enterprise.Customs.CA.Module", "Enterprise.Customs.CA.Module.RNSCFSShipmentPlugInController", Constants.CountryCodes.Canada));
			Add(new ControllerInfo(ControllerIDs.Customs.CA.RNSMFLoadListPlugIn, "Enterprise.Customs.CA.Module", "Enterprise.Customs.CA.Module.RNSMFLoadListPlugInController", Constants.CountryCodes.Canada));
			Add(new ControllerInfo(ControllerIDs.Customs.CA.RNSMFTallyPlugIn, "Enterprise.Customs.CA.Module", "Enterprise.Customs.CA.Module.RNSMFTallyPlugInController", Constants.CountryCodes.Canada));
			Add(new ControllerInfo(ControllerIDs.Customs.CA.RNSMFTallyShipmentsPlugIn, "Enterprise.Customs.CA.Module", "Enterprise.Customs.CA.Module.RNSMFTallyShipmentsPlugInController", Constants.CountryCodes.Canada));
			Add(new ControllerInfo(ControllerIDs.Customs.CA.CALVXJobs, "Enterprise.Customs.CA.Module", "Enterprise.Customs.CA.Module.LVXController", Constants.CountryCodes.Canada));
			Add(new ControllerInfo(ControllerIDs.Customs.CA.OrganisationCustomsMessaging, "Enterprise.Customs.CA.Module", "Enterprise.Customs.CA.Module.CAOrganisationCustomsMessagingController"));
			Add(new ControllerInfo(ControllerIDs.Customs.CA.CusSCAOceanBill, "Enterprise.Customs.CA.Module", "Enterprise.Customs.CA.Module.CusSCAOceanBillController"));
			Add(new ControllerInfo(ControllerIDs.Customs.CA.CAJobDocAddresses, "Enterprise.Customs.CA.Module", "Enterprise.Customs.CA.Module.JobDocAddressesController"));
			Add(new ControllerInfo(ControllerIDs.Customs.DocumentImageSystem, "Enterprise.Customs.CA.DIF.Module", "Enterprise.Customs.CA.DIF.Module.DIFController", Constants.CountryCodes.Canada));
			Add(new ControllerInfo(ControllerIDs.Customs.CA.CADailyNoticeReconciliation, "Enterprise.Customs.CA.Module", "Enterprise.Customs.CA.Module.DailyNoticeReconciliationController"));
			Add(new ControllerInfo(ControllerIDs.Customs.CustomsStatement, "Enterprise.Customs.CA.Module", "Enterprise.Customs.CA.Module.ARLStatementOfAccountController", Constants.CountryCodes.Canada));
			Add(new ControllerInfo(ControllerIDs.Customs.CA.CACSARevenueSummaryForm, "Enterprise.Customs.CA.Module", "Enterprise.Customs.CA.Module.CSARevenueSummaryFormController", Constants.CountryCodes.Canada));
			Add(new ControllerInfo(ControllerIDs.Customs.Universal.ZZRefCusRuling, "Enterprise.Customs.CA.Module", "Enterprise.Customs.CA.Module.CACusRulingController", Constants.CountryCodes.Canada));

			// CN Customs Controllers
			Add(new ControllerInfo(ControllerIDs.Customs.JobDeclaration, "Enterprise.Customs.CN.Module", "Enterprise.Customs.CN.Module.JobDeclarationController", Constants.CountryCodes.China));
			Add(new ControllerInfo(ControllerIDs.Customs.JobDeclarationPluggedIntoShipment, "Enterprise.Customs.CN.Module", "Enterprise.Customs.CN.Module.JobDeclarationShipmentController", Constants.CountryCodes.China));
			Add(new ControllerInfo(ControllerIDs.CommercialInvoice, "Enterprise.Customs.CN.Module", "Enterprise.Customs.CN.Module.CommercialInvoiceController", Constants.CountryCodes.China));
			Add(new ControllerInfo(ControllerIDs.Customs.SupplierPart, "Enterprise.Customs.CN.Module", "Enterprise.Customs.CN.Module.OrgSupplierPartController", Constants.CountryCodes.China));
			Add(new ControllerInfo(ControllerIDs.Customs.CN.OrgBuyerSupplierLinkChinaCustomsDetails, "Enterprise.Customs.CN.Module", "Enterprise.Customs.CN.Module.CNOrgBuyerSupplierLinkChinaCustomsDetailsController", Constants.CountryCodes.China));
			Add(new ControllerInfo(ControllerIDs.Customs.CN.OrganisationDetailsPlugIn, "Enterprise.Customs.CN.Module", "Enterprise.Customs.CN.Module.OrganisationDetailsController", Constants.CountryCodes.China));
			Add(new ControllerInfo(ControllerIDs.Customs.CN.OrgSupBuyLinkTrnModeAdditionalCustomsDetails, "Enterprise.Customs.CN.Module", "Enterprise.Customs.CN.Module.OrgSupBuyLinkTrnModeAddInfoController", Constants.CountryCodes.China));
			Add(new ControllerInfo(ControllerIDs.Customs.CN.EntryHeader, "Enterprise.Customs.CN.Module", "Enterprise.Customs.CN.Module.EntryHeaderController", Constants.CountryCodes.China));

			// HK Customs Controllers
			Add(new ControllerInfo(ControllerIDs.Customs.HK.Traxon, "Enterprise.Customs.HK.Module", "Enterprise.Customs.HK.Module.HKCustomsTraxonController", "HK"));

			// NZ Customs Controllers
			Add(new ControllerInfo(ControllerIDs.Customs.JobDeclaration, "Enterprise.Customs.NZ.Module", "Enterprise.Customs.NZ.Module.Declaration.FormalEntry.JobDeclarationController", Constants.CountryCodes.NewZealand));
			Add(new ControllerInfo(ControllerIDs.Customs.JobDeclarationPluggedIntoShipment, "Enterprise.Customs.NZ.Module", "Enterprise.Customs.NZ.Module.Declaration.FormalEntry.JobDeclarationShipmentController", Constants.CountryCodes.NewZealand));
			Add(new ControllerInfo(ControllerIDs.Customs.SingleTariffClassification, "Enterprise.Customs.NZ.Module", "Enterprise.Customs.NZ.Module.CusClassificationController", Constants.CountryCodes.NewZealand));
			Add(new ControllerInfo(ControllerIDs.Customs.TariffBulkChange, "Enterprise.Customs.NZ.Module", "Enterprise.Customs.NZ.Module.NZTariffBulkChangeController", Constants.CountryCodes.NewZealand));
			Add(new ControllerInfo(ControllerIDs.Customs.NZ.CUSCAR, "Enterprise.Customs.NZ.Module", "Enterprise.Customs.NZ.Module.Declaration.ECIWriteOff.NZCUSCARController", Constants.CountryCodes.NewZealand));
			Add(new ControllerInfo(ControllerIDs.Customs.NZ.CUSCARPluggedIntoShipment, "Enterprise.Customs.NZ.Module", "Enterprise.Customs.NZ.Module.Declaration.ECIWriteOff.NZCUSCARShipmentController", Constants.CountryCodes.NewZealand));
			Add(new ControllerInfo(ControllerIDs.Customs.NZ.ECIWriteOffManifesting, "Enterprise.Customs.NZ.Module", "Enterprise.Customs.NZ.Module.Declaration.ECIWriteOffManifestingController", Constants.CountryCodes.NewZealand));
			Add(new ControllerInfo(ControllerIDs.Customs.NZ.ExpressECI, "Enterprise.Customs.NZ.Module", "Enterprise.Customs.NZ.Module.Express.ExpressECIController", Constants.CountryCodes.NewZealand));
			Add(new ControllerInfo(ControllerIDs.Customs.SupplierPart, "Enterprise.Customs.NZ.Module", "Enterprise.Customs.NZ.Module.ProductController", Constants.CountryCodes.NewZealand));
			Add(new ControllerInfo(ControllerIDs.Customs.NZ.Concession, "Enterprise.Customs.NZ.Module", "Enterprise.Customs.NZ.Module.NZCConcessionController", Constants.CountryCodes.NewZealand));
			Add(new ControllerInfo(ControllerIDs.Customs.NZ.OutwardReport, "Enterprise.Customs.NZ.Module", "Enterprise.Customs.NZ.Module.OutwardReportController", Constants.CountryCodes.NewZealand));
			Add(new ControllerInfo(ControllerIDs.Customs.NZ.ECIWriteOffManifestingConsolSynchroniser, "Enterprise.Customs.NZ.Module", "Enterprise.Customs.NZ.Module.ForwardingConsolToNZECIManifestSyncroniserController", Constants.CountryCodes.NewZealand));
			Add(new ControllerInfo(ControllerIDs.Customs.NZ.ConsolToExpressECIConverter, "Enterprise.Customs.NZ.Module", "Enterprise.Customs.NZ.Module.Express.ConsolToExpressECIConverterController", Constants.CountryCodes.NewZealand));
			Add(new ControllerInfo(ControllerIDs.Customs.NZ.ShipmentToExpressECIConverter, "Enterprise.Customs.NZ.Module", "Enterprise.Customs.NZ.Module.Express.ShipmentToExpressECIConverterController", Constants.CountryCodes.NewZealand));
			Add(new ControllerInfo(ControllerIDs.Customs.NZ.MAFeBACCaDeclarationPlugin, "Enterprise.Customs.NZ.Module", "Enterprise.Customs.NZ.Module.MAFeBACCa.MAFeBACCaDeclarationPlugInController", Constants.CountryCodes.NewZealand));
			Add(new ControllerInfo(ControllerIDs.Customs.NZ.MAFeBACCaConsolPlugIn, "Enterprise.Customs.NZ.Module", "Enterprise.Customs.NZ.Module.MAFeBACCa.MAFeBACCaConsolPlugInController", Constants.CountryCodes.NewZealand));
			Add(new ControllerInfo(ControllerIDs.Customs.NZ.MAFeBACCaContainerPlugin, "Enterprise.Customs.NZ.Module", "Enterprise.Customs.NZ.Module.MAFeBACCa.MAFeBACCaContainerPlugInController", Constants.CountryCodes.NewZealand));
			Add(new ControllerInfo(ControllerIDs.Customs.NZ.MAFeBACCaInvoiceLinePlugin, "Enterprise.Customs.NZ.Module", "Enterprise.Customs.NZ.Module.MAFeBACCa.MAFeBACCaInvoiceLinePlugInController", Constants.CountryCodes.NewZealand));
			Add(new ControllerInfo(ControllerIDs.CommercialInvoice, "Enterprise.Customs.NZ.Module", "Enterprise.Customs.NZ.Module.CommercialInvoiceController", Constants.CountryCodes.NewZealand));
			Add(new ControllerInfo(ControllerIDs.Customs.SendTestCustomsMessage, "Enterprise.Customs.NZ.Module", "Enterprise.Customs.NZ.Module.NZSendTestCustomsMessageController", Constants.CountryCodes.NewZealand));
			Add(new ControllerInfo(ControllerIDs.Customs.NZ.InwardCargoReport, "Enterprise.Customs.NZ.Module", "Enterprise.Customs.NZ.Module.ICRController", Constants.CountryCodes.NewZealand));
			Add(new ControllerInfo(ControllerIDs.Customs.NZ.SeaCargoICR, "Enterprise.Customs.NZ.Module", "Enterprise.Customs.NZ.Module.Express.SeaCargoICRController", Constants.CountryCodes.NewZealand));
			Add(new ControllerInfo(ControllerIDs.Customs.NZ.ConsolToSeaCargoWriteOffConverter, "Enterprise.Customs.NZ.Module", "Enterprise.Customs.NZ.Module.Express.ConsolToSeaCargoWriteOffConverterController", Constants.CountryCodes.NewZealand));
			Add(new ControllerInfo(ControllerIDs.Customs.NZ.ShipmentToSeaCargoWriteOffConverter, "Enterprise.Customs.NZ.Module", "Enterprise.Customs.NZ.Module.Express.ShipmentToSeaCargoWriteOffConverterController", Constants.CountryCodes.NewZealand));
			Add(new ControllerInfo(ControllerIDs.Customs.ConsolidatedDeclaration, "Enterprise.Customs.NZ.Module", "Enterprise.Customs.NZ.Module.ConsolidatedDeclarationController", Constants.CountryCodes.NewZealand));

			// SG Customs Controllers
			Add(new ControllerInfo(ControllerIDs.Customs.JobDeclaration, "Enterprise.Customs.SG.V4.Module", "Enterprise.Customs.SG.V4.Module.JobDeclarationController", Constants.CountryCodes.Singapore));
			Add(new ControllerInfo(ControllerIDs.Customs.JobDeclarationPluggedIntoShipment, "Enterprise.Customs.SG.V4.Module", "Enterprise.Customs.SG.V4.Module.JobDeclarationShipmentController", Constants.CountryCodes.Singapore));
			Add(new ControllerInfo(ControllerIDs.Customs.SGV3JobDeclaration, "Enterprise.Customs.SG.V4.Module", "Enterprise.Customs.SG.V3.Module.V3JobDeclarationController", Constants.CountryCodes.Singapore));
			Add(new ControllerInfo(ControllerIDs.Customs.SingleTariffClassification, "Enterprise.Customs.SG.V4.Module", "Enterprise.Customs.SG.V4.Module.ClassificationController", Constants.CountryCodes.Singapore));
			Add(new ControllerInfo(ControllerIDs.Customs.SupplierPart, "Enterprise.Customs.SG.V4.Module", "Enterprise.Customs.SG.V4.Module.OrgSupplierPartController", Constants.CountryCodes.Singapore));

			// SG Customs V4.0
			Add(new ControllerInfo(ControllerIDs.Customs.SG.CMDShipment, "Enterprise.Customs.SG.V4.Module", "Enterprise.Customs.SG.V4.Module.CMDShipmentController", Constants.CountryCodes.Singapore));
			Add(new ControllerInfo(ControllerIDs.Customs.SG.CMDConsol, "Enterprise.Customs.SG.V4.Module", "Enterprise.Customs.SG.V4.Module.CMDConsolController", Constants.CountryCodes.Singapore));
			Add(new ControllerInfo(ControllerIDs.Customs.SG.SG4Classification, "Enterprise.Customs.SG.V4.Module", "Enterprise.Customs.SG.V4.Module.ClassificationController", Constants.CountryCodes.Singapore));
			Add(new ControllerInfo(ControllerIDs.CommercialInvoice, "Enterprise.Customs.SG.V4.Module", "Enterprise.Customs.SG.V4.Module.CommercialInvoiceController", Constants.CountryCodes.Singapore));

			//DE Customs Controllers
			Add(new ControllerInfo(ControllerIDs.Customs.JobDeclaration, "Enterprise.Customs.DE.Module", "Enterprise.Customs.DE.Module.JobDeclarationController", Constants.CountryCodes.Germany));
			Add(new ControllerInfo(ControllerIDs.Customs.EU.NctsMovementController, "Enterprise.Customs.DE.NCTS.Module", "Enterprise.Customs.DE.NCTS.Module.DENctsMovementController", Constants.CountryCodes.Germany));
			Add(new ControllerInfo(ControllerIDs.Customs.TemporaryStorage, "Enterprise.Customs.DE.Module", "Enterprise.Customs.DE.Module.SumAController", Constants.CountryCodes.Germany));
			Add(new ControllerInfo(ControllerIDs.Customs.SupplierPart, "Enterprise.Customs.DE.Module", "Enterprise.Customs.DE.Module.OrgSupplierPartController", Constants.CountryCodes.Germany));
			Add(new ControllerInfo(ControllerIDs.Customs.Guarantees, "Enterprise.Customs.DE.NCTS.Module", "Enterprise.Customs.DE.NCTS.Module.DEGuaranteesController", Constants.CountryCodes.Germany));
			Add(new ControllerInfo(ControllerIDs.Customs.DE.SumARegister, "Enterprise.Customs.DE.Module", "Enterprise.Customs.DE.Module.SumARegisterController", Constants.CountryCodes.Germany));
			Add(new ControllerInfo(ControllerIDs.Customs.DE.SumARegisterReadOnly, "Enterprise.Customs.DE.Module", "Enterprise.Customs.DE.Module.SumARegisterReadOnlyController", Constants.CountryCodes.Germany));
			Add(new ControllerInfo(ControllerIDs.Customs.DE.ExportStatusRequest, "Enterprise.Customs.DE.Module", "Enterprise.Customs.DE.Module.ExportStatusRequestController", Constants.CountryCodes.Germany));
			Add(new ControllerInfo(ControllerIDs.Customs.DE.MonthlyClosing, "Enterprise.Customs.DE.Module", "Enterprise.Customs.DE.Module.MonthlyClosingController", Constants.CountryCodes.Germany));
			Add(new ControllerInfo(ControllerIDs.CommercialInvoice, "Enterprise.Customs.DE.Module", "Enterprise.Customs.DE.Module.CommercialInvoiceController", Constants.CountryCodes.Germany));
			Add(new ControllerInfo(ControllerIDs.Customs.DE.OrganisationDetailsPlugIn, "Enterprise.Customs.DE.Module", "Enterprise.Customs.DE.Module.DEOrganisationDetailsController"));
			Add(new ControllerInfo(ControllerIDs.Customs.DE.OrganisationConsigneePlugIn, "Enterprise.Customs.DE.Module", "Enterprise.Customs.DE.Module.DEOrganisationConsigneePlugInController"));
			Add(new ControllerInfo(ControllerIDs.Customs.DE.TaxChangeAssessment, "Enterprise.Customs.DE.Module", "Enterprise.Customs.DE.Module.TaxChangeAssessmentController", Constants.CountryCodes.Germany));
			Add(new ControllerInfo(ControllerIDs.Customs.EU.ExitControl, "Enterprise.Customs.DE.ExitControl.Module", "Enterprise.Customs.DE.ExitControl.Module.ExitControlController", Constants.CountryCodes.Germany));
			Add(new ControllerInfo(ControllerIDs.Customs.EU.IntrastatTransactionsController, "Enterprise.Customs.DE.Intrastat.Module", "Enterprise.Customs.DE.Intrastat.Module.IntrastatTransactionsController", Constants.CountryCodes.Germany));

			//AE Customs
			Add(new ControllerInfo(ControllerIDs.Customs.JobDeclaration, "Enterprise.Customs.AE.Module", "Enterprise.Customs.AE.Module.JobDeclarationController", Constants.CountryCodes.UnitedArabEmirates));
			Add(new ControllerInfo(ControllerIDs.Customs.SingleTariffClassification, "Enterprise.Customs.AE.Module", "Enterprise.Customs.AE.Module.CusClassificationController", Constants.CountryCodes.UnitedArabEmirates));
			Add(new ControllerInfo(ControllerIDs.Customs.SupplierPart, "Enterprise.Customs.AE.Module", "Enterprise.Customs.AE.Module.OrgSupplierPartController", Constants.CountryCodes.UnitedArabEmirates));
			Add(new ControllerInfo(ControllerIDs.CommercialInvoice, "Enterprise.Customs.AE.Module", "Enterprise.Customs.AE.Module.CommercialInvoiceController", Constants.CountryCodes.UnitedArabEmirates));
			Add(new ControllerInfo(ControllerIDs.Customs.CargoManifestPlugInForConsol, "Enterprise.Customs.AE.GUI", "Enterprise.Customs.AE.GUI.PlugIn.AECustomsCargoManifestController", Constants.CountryCodes.UnitedArabEmirates));

			// MY Customs Controllers
			Add(new ControllerInfo(ControllerIDs.Customs.JobDeclaration, "Enterprise.Customs.MY.Module", "Enterprise.Customs.MY.Module.JobDeclarationController", Constants.CountryCodes.Malaysia));
			Add(new ControllerInfo(ControllerIDs.Customs.JobDeclarationPluggedIntoShipment, "Enterprise.Customs.MY.Module", "Enterprise.Customs.MY.Module.JobDeclarationShipmentController", Constants.CountryCodes.Malaysia));
			Add(new ControllerInfo(ControllerIDs.Customs.SingleTariffClassification, "Enterprise.Customs.MY.Module", "Enterprise.Customs.MY.Module.CusClassificationController", Constants.CountryCodes.Malaysia));
			Add(new ControllerInfo(ControllerIDs.Customs.SupplierPart, "Enterprise.Customs.MY.Module", "Enterprise.Customs.MY.Module.OrgSupplierPartController", Constants.CountryCodes.Malaysia));
			Add(new ControllerInfo(ControllerIDs.CommercialInvoice, "Enterprise.Customs.MY.Module", "Enterprise.Customs.MY.Module.CommercialInvoiceController", Constants.CountryCodes.Malaysia));

			//US&PR Customs
			Add(new ControllerInfo(ControllerIDs.Customs.US.USCarrierCombined, "Enterprise.Customs.US.Module", "Enterprise.Customs.US.Module.USCarrierCombinedController"));
			Add(new ControllerInfo(ControllerIDs.Customs.US.OrgBuyerSupplierLinkAdditionalCustomsDetails, "Enterprise.Customs.US.Module", "Enterprise.Customs.US.Module.USOrgBuyerSupplierLinkAdditionalCustomsDetailsController"));
			Add(new ControllerInfo(ControllerIDs.Customs.US.eManifest, "Enterprise.Customs.US.eManifest.Module", "Enterprise.Customs.US.eManifest.Module.eManifestController"));
			Add(new ControllerInfo(ControllerIDs.Customs.US.eManifestShipment, "Enterprise.Customs.US.eManifest.Module", "Enterprise.Customs.US.eManifest.Module.eManifestShipmentController"));
			Add(new ControllerInfo(ControllerIDs.Customs.US.OrganisationDetailsPlugIn, "Enterprise.Customs.US.Module", "Enterprise.Customs.US.Module.USOrganisationDetailsPlugInController"));
			Add(new ControllerInfo(ControllerIDs.Customs.US.InBond, "Enterprise.Customs.US.InBond.Module", "Enterprise.Customs.US.InBond.Module.CusInBondHeaderController"));
			Add(new ControllerInfo(ControllerIDs.Customs.US.InBondMoveHeader, "Enterprise.Customs.US.InBond.Module", "Enterprise.Customs.US.InBond.Module.USInBondMoveHeaderController"));
			Add(new ControllerInfo(ControllerIDs.Customs.US.ThreeLetterRefAirline, "Enterprise.Customs.US.InBond.Module", "Enterprise.Customs.US.InBond.Module.ThreeLetterRefAirlineController"));
			Add(new ControllerInfo(ControllerIDs.Customs.US.USLowValueEntries, "Enterprise.Customs.US.LVS.Module", "Enterprise.Customs.US.LVS.Module.CusUSLVClearanceController"));
			Add(new ControllerInfo(ControllerIDs.Customs.US.USLowValueEntriesBill, "Enterprise.Customs.US.LVS.Module", "Enterprise.Customs.US.LVS.Module.CusUSLVConsignmentController"));
			Add(new ControllerInfo(ControllerIDs.Customs.US.USLowValueEntriesDeclaration, "Enterprise.Customs.US.LVS.Module", "Enterprise.Customs.US.LVS.Module.CusUSLVDeclarationController"));

			foreach (var countryCode in new string[] { Constants.CountryCodes.UnitedStates, Constants.CountryCodes.PuertoRico })
			{
				Add(new ControllerInfo(ControllerIDs.Customs.JobDeclaration, "Enterprise.Customs.US.Module", "Enterprise.Customs.US.Module.JobDeclarationController", countryCode));
				Add(new ControllerInfo(ControllerIDs.Customs.JobDeclarationPluggedIntoShipment, "Enterprise.Customs.US.Module", "Enterprise.Customs.US.Module.JobDeclarationShipmentController", countryCode));
				Add(new ControllerInfo(ControllerIDs.Customs.SupplierPart, "Enterprise.Customs.US.Module", "Enterprise.Customs.US.Module.OrgSupplierPartController", countryCode));
				Add(new ControllerInfo(ControllerIDs.Customs.ImportClassification, "Enterprise.Customs.US.Module", "Enterprise.Customs.US.Module.ImportClassificationController", countryCode));
				Add(new ControllerInfo(ControllerIDs.Customs.ExportClassification, "Enterprise.Customs.US.Module", "Enterprise.Customs.US.Module.ExportClassificationController", countryCode));
				Add(new ControllerInfo(ControllerIDs.CommercialInvoice, "Enterprise.Customs.US.Module", "Enterprise.Customs.US.Module.CommercialInvoiceController", countryCode));
				Add(new ControllerInfo(ControllerIDs.Customs.DocumentImageSystem, "Enterprise.Customs.US.DIS.Module", "Enterprise.Customs.US.DIS.Module.DISController", countryCode));

				Add(new ControllerInfo(ControllerIDs.Customs.CustomsStatement, "Enterprise.Customs.US.Module", "Enterprise.Customs.US.Module.StatementController", countryCode));
				Add(new ControllerInfo(ControllerIDs.Customs.US.OrganisationCustomsMessaging, "Enterprise.Customs.US.Module", "Enterprise.Customs.US.Module.USOrganisationController", countryCode));
				Add(new ControllerInfo(ControllerIDs.Customs.US.AMSBrokerDownloadMessages, "Enterprise.Customs.US.Module", "Enterprise.Customs.US.Module.AMSBrokerDownloadController", countryCode));
				Add(new ControllerInfo(ControllerIDs.Customs.US.BorderLineReleaseMessage, "Enterprise.Customs.US.Module", "Enterprise.Customs.US.Module.BorderLineReleaseMessageController", countryCode));
				Add(new ControllerInfo(ControllerIDs.Customs.US.QueryMessages, "Enterprise.Customs.US.Module", "Enterprise.Customs.US.Module.QueryMessageController", countryCode));
				Add(new ControllerInfo(ControllerIDs.Customs.US.InBondPluggedIntoDeclaration, "Enterprise.Customs.US.InBond.Module", "Enterprise.Customs.US.InBond.Module.CusInBondHeaderDeclarationController", countryCode));
				Add(new ControllerInfo(ControllerIDs.Customs.US.InBondPluggedIntoShipment, "Enterprise.Customs.US.InBond.Module", "Enterprise.Customs.US.InBond.Module.CusInBondHeaderShipmentController", countryCode));
				Add(new ControllerInfo(ControllerIDs.Customs.US.InBondPluggedIntoConsol, "Enterprise.Customs.US.InBond.Module", "Enterprise.Customs.US.InBond.Module.CusInBondHeaderConsolController", countryCode));
				Add(new ControllerInfo(ControllerIDs.Customs.US.InBondNumber, "Enterprise.Customs.US.Module", "Enterprise.Customs.US.Module.InBondNumberController", countryCode));
				Add(new ControllerInfo(ControllerIDs.Customs.US.USCRule, "Enterprise.Customs.US.Module", "Enterprise.Customs.US.Module.USCRuleController", countryCode));
				Add(new ControllerInfo(ControllerIDs.Customs.US.USCTariffRule, "Enterprise.Customs.US.Module", "Enterprise.Customs.US.Module.USCTariffRuleController", countryCode));
				Add(new ControllerInfo(ControllerIDs.Customs.US.USTariffBulkChange, "Enterprise.Customs.US.Module", "Enterprise.Customs.US.Module.USTariffBulkChangeController", countryCode));
				Add(new ControllerInfo(ControllerIDs.Customs.US.Protest, "Enterprise.Customs.US.Module", "Enterprise.Customs.US.Module.Protest.ProtestController", countryCode));
				Add(new ControllerInfo(ControllerIDs.Customs.US.Recon, "Enterprise.Customs.US.Module", "Enterprise.Customs.US.Module.ReconController", countryCode));
				Add(new ControllerInfo(ControllerIDs.Customs.US.Drawback, "Enterprise.Customs.US.Module", "Enterprise.Customs.US.Module.DrawbackController", countryCode));
				Add(new ControllerInfo(ControllerIDs.Customs.US.USCACCase, "Enterprise.Customs.US.Module", "Enterprise.Customs.US.Module.USCACCaseController", countryCode));
				Add(new ControllerInfo(ControllerIDs.Customs.US.CourtesyNoticesOfLiquidation, "Enterprise.Customs.US.Module", "Enterprise.Customs.US.Module.CourtesyNoticesOfLiquidationController", countryCode));
				Add(new ControllerInfo(ControllerIDs.Customs.US.USCCountry, "Enterprise.Customs.US.Module", "Enterprise.Customs.US.Module.USCCountryController", countryCode));
				Add(new ControllerInfo(ControllerIDs.Customs.US.ContainerAdditionalReferenceNumbers, "Enterprise.Customs.US.Module", "Enterprise.Customs.US.Module.USContainerAdditonalReferenceNumbersController", countryCode));
			}

			// TW Customs Controllers
			Add(new ControllerInfo(ControllerIDs.Customs.JobDeclaration, "Enterprise.Customs.TW.Module", "Enterprise.Customs.TW.Module.JobDeclarationController", Constants.CountryCodes.Taiwan));
			Add(new ControllerInfo(ControllerIDs.Customs.JobDeclarationPluggedIntoShipment, "Enterprise.Customs.TW.Module", "Enterprise.Customs.TW.Module.JobDeclarationShipmentController", Constants.CountryCodes.Taiwan));
			Add(new ControllerInfo(ControllerIDs.Customs.SingleTariffClassification, "Enterprise.Customs.TW.Module", "Enterprise.Customs.TW.Module.CusClassificationController", Constants.CountryCodes.Taiwan));
			Add(new ControllerInfo(ControllerIDs.Customs.SupplierPart, "Enterprise.Customs.TW.Module", "Enterprise.Customs.TW.Module.OrgSupplierPartController", Constants.CountryCodes.Taiwan));
			Add(new ControllerInfo(ControllerIDs.CommercialInvoice, "Enterprise.Customs.TW.Module", "Enterprise.Customs.TW.Module.CommercialInvoiceController", Constants.CountryCodes.Taiwan));
			Add(new ControllerInfo(ControllerIDs.Customs.TW.Transhipment, "Enterprise.Customs.TW.Module", "Enterprise.Customs.TW.Transhipment.Module.CusInBondHeaderController", Constants.CountryCodes.Taiwan));
			Add(new ControllerInfo(ControllerIDs.Customs.TW.OrganisationDetailsPlugIn, "Enterprise.Customs.TW.Module", "Enterprise.Customs.TW.Module.TWOrganisationDetailsController"));
			Add(new ControllerInfo(ControllerIDs.Customs.CusPackingList, "Enterprise.Customs.TW.Module", "Enterprise.Customs.TW.Module.CusPackingListController", Constants.CountryCodes.Taiwan));
			Add(new ControllerInfo(ControllerIDs.Customs.TW.BriefCustomsDeclarations, "Enterprise.Customs.TW.BriefCustomsDeclaration.Module", "Enterprise.Customs.TW.BriefCustomsDeclaration.Module.ManifestController", Constants.CountryCodes.Taiwan));

			// BR Customs Controllers
			Add(new ControllerInfo(ControllerIDs.Customs.JobDeclaration, "Enterprise.Customs.BR.Module", "Enterprise.Customs.BR.Module.JobDeclarationController", Constants.CountryCodes.Brazil));
			Add(new ControllerInfo(ControllerIDs.Customs.JobDeclarationPluggedIntoShipment, "Enterprise.Customs.BR.Module", "Enterprise.Customs.BR.Module.JobDeclarationShipmentController", Constants.CountryCodes.Brazil));
			Add(new ControllerInfo(ControllerIDs.Customs.SingleTariffClassification, "Enterprise.Customs.BR.Module", "Enterprise.Customs.BR.Module.CusClassificationController", Constants.CountryCodes.Brazil));
			Add(new ControllerInfo(ControllerIDs.Customs.SupplierPart, "Enterprise.Customs.BR.Module", "Enterprise.Customs.BR.Module.OrgSupplierPartController", Constants.CountryCodes.Brazil));
			Add(new ControllerInfo(ControllerIDs.CommercialInvoice, "Enterprise.Customs.BR.Module", "Enterprise.Customs.BR.Module.CommercialInvoiceController", Constants.CountryCodes.Brazil));
			Add(new ControllerInfo(ControllerIDs.Customs.BR.OrganisationConsigneePlugIn, "Enterprise.Customs.BR.Module", "Enterprise.Customs.BR.Module.BROrganisationConsigneePlugInController"));
			Add(new ControllerInfo(ControllerIDs.Customs.BR.LPCO, "Enterprise.Customs.BR.Module", "Enterprise.Customs.BR.Module.LPCOController", Constants.CountryCodes.Brazil));
			Add(new ControllerInfo(ControllerIDs.Customs.BR.LPCODeclaration, "Enterprise.Customs.BR.Module", "Enterprise.Customs.BR.Module.LPCODeclarationController", Constants.CountryCodes.Brazil));
			Add(new ControllerInfo(ControllerIDs.Customs.BR.LPCOEntryHeader, "Enterprise.Customs.BR.Module", "Enterprise.Customs.BR.Module.LPCOEntryHeaderController", Constants.CountryCodes.Brazil));
			Add(new ControllerInfo(ControllerIDs.Customs.BR.License, "Enterprise.Customs.BR.Module", "Enterprise.Customs.BR.Module.LicenseController", Constants.CountryCodes.Brazil));
			Add(new ControllerInfo(ControllerIDs.Customs.BR.LicenseEntryHeader, "Enterprise.Customs.BR.Module", "Enterprise.Customs.BR.Module.LicenseEntryHeaderController", Constants.CountryCodes.Brazil));
			Add(new ControllerInfo(ControllerIDs.Customs.GoodsCatalog, "Enterprise.Customs.BR.Module", "Enterprise.Customs.BR.Module.GoodsCatalogController", Constants.CountryCodes.Brazil));
			Add(new ControllerInfo(ControllerIDs.Customs.BR.OrganisationDetailsPlugIn, "Enterprise.Customs.BR.Module", "Enterprise.Customs.BR.Module.BROrganisationDetailPlugInController"));
			Add(new ControllerInfo(ControllerIDs.Customs.BR.ForeignOperator, "Enterprise.Customs.BR.Module", "Enterprise.Customs.BR.Module.ForeignOperatorController", Constants.CountryCodes.Brazil));

			// JP Customs Controllers
			Add(new ControllerInfo(ControllerIDs.Customs.JobDeclaration, "Enterprise.Customs.JP.Module", "Enterprise.Customs.JP.Module.JobDeclarationController", Constants.CountryCodes.Japan));
			Add(new ControllerInfo(ControllerIDs.Customs.JobDeclarationPluggedIntoShipment, "Enterprise.Customs.JP.Module", "Enterprise.Customs.JP.Module.JobDeclarationShipmentController", Constants.CountryCodes.Japan));
			Add(new ControllerInfo(ControllerIDs.Customs.SingleTariffClassification, "Enterprise.Customs.JP.Module", "Enterprise.Customs.JP.Module.CusClassificationController", Constants.CountryCodes.Japan));
			Add(new ControllerInfo(ControllerIDs.Customs.SupplierPart, "Enterprise.Customs.JP.Module", "Enterprise.Customs.JP.Module.OrgSupplierPartController", Constants.CountryCodes.Japan));
			Add(new ControllerInfo(ControllerIDs.CommercialInvoice, "Enterprise.Customs.JP.Module", "Enterprise.Customs.JP.Module.CommercialInvoiceController", Constants.CountryCodes.Japan));
			// FR Customs Controllers
			Add(new ControllerInfo(ControllerIDs.Customs.FR.OrganisationDetailsPlugIn, "Enterprise.Customs.FR.Module", "Enterprise.Customs.FR.Module.FROrganisationDetailsController"));
			Add(new ControllerInfo(ControllerIDs.Customs.FR.OrganisationConsigneePlugIn, "Enterprise.Customs.FR.Module", "Enterprise.Customs.FR.Module.OrganisationConsigneePlugInController"));
			foreach (var countryCode in Constants.CountryCodes.FranceAndOverseasDepartmentsUnderItsCustomsJurisdiction)
			{
				Add(new ControllerInfo(ControllerIDs.Customs.JobDeclaration, "Enterprise.Customs.FR.Module", "Enterprise.Customs.FR.Module.JobDeclarationController", countryCode));
				Add(new ControllerInfo(ControllerIDs.Customs.SupplierPart, "Enterprise.Customs.FR.Module", "Enterprise.Customs.FR.Module.OrgSupplierPartController", countryCode));
				Add(new ControllerInfo(ControllerIDs.Customs.TemporaryStorage, "Enterprise.Customs.FR.Module", "Enterprise.Customs.FR.Module.TemporaryStorageController", countryCode));
				Add(new ControllerInfo(ControllerIDs.Customs.FR.CINTemporaryStorageConsolController, "Enterprise.Customs.FR.Module", "Enterprise.Customs.FR.Module.CINTemporaryStorageConsolController", countryCode));
				Add(new ControllerInfo(ControllerIDs.Customs.FR.CINExportConsolIntegrationController, "Enterprise.Customs.FR.Module", "Enterprise.Customs.FR.Module.CINExportConsolIntegrationController", countryCode));
				Add(new ControllerInfo(ControllerIDs.Customs.Guarantees, "Enterprise.Customs.FR.Module", "Enterprise.Customs.FR.Module.GuaranteesController", countryCode));
				Add(new ControllerInfo(ControllerIDs.Customs.FR.EntryHeader, "Enterprise.Customs.FR.Module", "Enterprise.Customs.FR.Module.EntryHeaderController", countryCode));
				Add(new ControllerInfo(ControllerIDs.Customs.CustomsStatement, "Enterprise.Customs.FR.Module", "Enterprise.Customs.FR.Module.StatementController", countryCode));
				Add(new ControllerInfo(ControllerIDs.Customs.EU.TempStorageRegister, "Enterprise.Customs.FR.Module", "Enterprise.Customs.FR.Module.TempStorageRegisterController", countryCode));
			}
			// KR Customs Controllers
			Add(new ControllerInfo(ControllerIDs.Customs.JobDeclaration, "Enterprise.Customs.KR.Module", "Enterprise.Customs.KR.Module.JobDeclarationController", Constants.CountryCodes.KoreaSouth));
			Add(new ControllerInfo(ControllerIDs.Customs.JobDeclarationPluggedIntoShipment, "Enterprise.Customs.KR.Module", "Enterprise.Customs.KR.Module.JobDeclarationShipmentController", Constants.CountryCodes.KoreaSouth));
			Add(new ControllerInfo(ControllerIDs.Customs.SupplierPart, "Enterprise.Customs.KR.Module", "Enterprise.Customs.KR.Module.OrgSupplierPartController", Constants.CountryCodes.KoreaSouth));
			Add(new ControllerInfo(ControllerIDs.CommercialInvoice, "Enterprise.Customs.KR.Module", "Enterprise.Customs.KR.Module.CommercialInvoiceController", Constants.CountryCodes.KoreaSouth));
			Add(new ControllerInfo(ControllerIDs.Customs.CustomsStatement, "Enterprise.Customs.KR.Module", "Enterprise.Customs.KR.Module.CusStatementController", Constants.CountryCodes.KoreaSouth));
			Add(new ControllerInfo(ControllerIDs.Customs.KR.MiscRequestMessages, "Enterprise.Customs.KR.Module", "Enterprise.Customs.KR.Module.MiscRequestMessagesController", Constants.CountryCodes.KoreaSouth));
			Add(new ControllerInfo(ControllerIDs.Customs.KR.ExportEntryDetails, "Enterprise.Customs.KR.Module", "Enterprise.Customs.KR.Module.EntryDetailsFor5ACController", Constants.CountryCodes.KoreaSouth));
			Add(new ControllerInfo(ControllerIDs.Customs.KR.ImportEntryDetails, "Enterprise.Customs.KR.Module", "Enterprise.Customs.KR.Module.EntryDetailsFor5GWController", Constants.CountryCodes.KoreaSouth));
			Add(new ControllerInfo(ControllerIDs.Customs.KR.EntryCustomsBillsFor5UL, "Enterprise.Customs.KR.Module", "Enterprise.Customs.KR.Module.EntryCustomsBillsFor5ULController", Constants.CountryCodes.KoreaSouth));
			Add(new ControllerInfo(ControllerIDs.Customs.KR.EntryDetailsFor5SG, "Enterprise.Customs.KR.Module", "Enterprise.Customs.KR.Module.EntryDetailsFor5SGController", Constants.CountryCodes.KoreaSouth));
			Add(new ControllerInfo(ControllerIDs.Customs.KR.EntryLineDetailsFor5UL, "Enterprise.Customs.KR.Module", "Enterprise.Customs.KR.Module.EntryLineDetailsFor5ULController", Constants.CountryCodes.KoreaSouth));
			Add(new ControllerInfo(ControllerIDs.Customs.KR.OrganisationDetailsPlugIn, "Enterprise.Customs.KR.Module", "Enterprise.Customs.KR.Module.KROrganisationController"));
			Add(new ControllerInfo(ControllerIDs.Customs.KR.DocumentListMessages, "Enterprise.Customs.KR.Module", "Enterprise.Customs.KR.Module.DocumentListMessagesController", Constants.CountryCodes.KoreaSouth));
			Add(new ControllerInfo(ControllerIDs.Customs.KR.CusReconDeclaration, "Enterprise.Customs.KR.Module", "Enterprise.Customs.KR.Module.CusReconDeclarationController", Constants.CountryCodes.KoreaSouth));

			// ES Customs Controllers
			Add(new ControllerInfo(ControllerIDs.Customs.JobDeclaration, "Enterprise.Customs.ES.Module", "Enterprise.Customs.ES.Module.JobDeclarationController", Constants.CountryCodes.Spain));
			Add(new ControllerInfo(ControllerIDs.Customs.SupplierPart, "Enterprise.Customs.ES.Module", "Enterprise.Customs.ES.Module.OrgSupplierPartController", Constants.CountryCodes.Spain));
			Add(new ControllerInfo(ControllerIDs.Customs.EU.NctsMovementController, "Enterprise.Customs.ES.NCTS.Module", "Enterprise.Customs.ES.NCTS.Module.NctsMovementController", Constants.CountryCodes.Spain));
			Add(new ControllerInfo(ControllerIDs.Customs.ES.OrganisationConsigneePlugIn, "Enterprise.Customs.ES.Module", "Enterprise.Customs.ES.Module.ESOrganisationConsigneePlugInController"));
			Add(new ControllerInfo(ControllerIDs.Customs.TemporaryStorage, "Enterprise.Customs.ES.TemporaryStorage.Module", "Enterprise.Customs.ES.TemporaryStorage.Module.TemporaryStorageController", Constants.CountryCodes.Spain));
			Add(new ControllerInfo(ControllerIDs.Customs.ES.TemporaryStorageRegister, "Enterprise.Customs.ES.TemporaryStorage.Module", "Enterprise.Customs.ES.TemporaryStorage.Module.TemporaryStorageRegisterController", Constants.CountryCodes.Spain));
			Add(new ControllerInfo(ControllerIDs.Customs.EU.ExitSummaryController, "Enterprise.Customs.ES.Module", "Enterprise.Customs.ES.Module.ExitSummaryController", Constants.CountryCodes.Spain));
			Add(new ControllerInfo(ControllerIDs.Customs.Guarantees, "Enterprise.Customs.ES.Module", "Enterprise.Customs.ES.Module.GuaranteesController", Constants.CountryCodes.Spain));
			Add(new ControllerInfo(ControllerIDs.Customs.EU.UCC6TemporaryStorage, "Enterprise.Customs.ES.TemporaryStorage.Module", "Enterprise.Customs.ES.TemporaryStorage.Module.G5V1TemporaryStorageController", Constants.CountryCodes.Spain));
			Add(new ControllerInfo(ControllerIDs.Customs.EU.TempStoragePremises, "Enterprise.Customs.ES.TemporaryStorage.Module", "Enterprise.Customs.ES.TemporaryStorage.Module.TempStoragePremisesController", Constants.CountryCodes.Spain));
			Add(new ControllerInfo(ControllerIDs.Customs.ES.G3Declaration, "Enterprise.Customs.ES.Manifest.H7.Module", "Enterprise.Customs.ES.Manifest.H7.Module.G3DeclarationController", Constants.CountryCodes.Spain));

			// TR Customs Controllers
			Add(new ControllerInfo(ControllerIDs.Customs.EU.NctsMovementController, "Enterprise.Customs.TR.NCTS.Module", "Enterprise.Customs.TR.NCTS.Module.TRNctsMovementController", Constants.CountryCodes.Turkey));
			Add(new ControllerInfo(ControllerIDs.Customs.TR.ETrade, "Enterprise.Customs.TR.ETrade.Module", "Enterprise.Customs.TR.ETrade.Module.ETradeController", Constants.CountryCodes.Turkey));
			Add(new ControllerInfo(ControllerIDs.Customs.TR.SimplifiedProcedureTransitSystem, "Enterprise.Customs.TR.NCTS.Module", "Enterprise.Customs.TR.NCTS.Module.SPTSController", Constants.CountryCodes.Turkey));
			Add(new ControllerInfo(ControllerIDs.Customs.Guarantees, "Enterprise.Customs.TR.Module", "Enterprise.Customs.TR.Module.GuaranteesController", Constants.CountryCodes.Turkey));
			Add(new ControllerInfo(ControllerIDs.Customs.CustomsStatement, "Enterprise.Customs.TR.Module", "Enterprise.Customs.TR.Module.StatementsStampDutyController", Constants.CountryCodes.Turkey));

			// PL Customs Controllers
			Add(new ControllerInfo(ControllerIDs.Customs.JobDeclaration, "Enterprise.Customs.PL.Module", "Enterprise.Customs.PL.Module.JobDeclarationController", Constants.CountryCodes.Poland));
			Add(new ControllerInfo(ControllerIDs.Customs.SupplierPart, "Enterprise.Customs.PL.Module", "Enterprise.Customs.PL.Module.OrgSupplierPartController", Constants.CountryCodes.Poland));
			Add(new ControllerInfo(ControllerIDs.CommercialInvoice, "Enterprise.Customs.PL.Module", "Enterprise.Customs.PL.Module.CommercialInvoiceController", Constants.CountryCodes.Poland));
			Add(new ControllerInfo(ControllerIDs.Customs.TemporaryStorage, "Enterprise.Customs.PL.Module", "Enterprise.Customs.PL.Module.TemporaryStorageController", Constants.CountryCodes.Poland));
			Add(new ControllerInfo(ControllerIDs.Customs.EU.NctsMovementController, "Enterprise.Customs.PL.NCTS.Module", "Enterprise.Customs.PL.NCTS.Module.NctsMovementController", Constants.CountryCodes.Poland));
			Add(new ControllerInfo(ControllerIDs.Customs.PL.AuthorisationRule, "Enterprise.Customs.PL.NCTS.Module", "Enterprise.Customs.PL.NCTS.Module.AuthorisationRuleController", Constants.CountryCodes.Poland));
			Add(new ControllerInfo(ControllerIDs.Messaging.EDIMessage, "Enterprise.Customs.PL.Module", "Enterprise.Customs.PL.Module.EDIMessageController", Constants.CountryCodes.Poland));

			// IE Customs Controllers
			Add(new ControllerInfo(ControllerIDs.Customs.JobDeclaration, "Enterprise.Customs.IE.Module", "Enterprise.Customs.IE.Module.JobDeclarationController", Constants.CountryCodes.Ireland));
			Add(new ControllerInfo(ControllerIDs.Customs.SupplierPart, "Enterprise.Customs.IE.Module", "Enterprise.Customs.IE.Module.OrgSupplierPartController", Constants.CountryCodes.Ireland));
			Add(new ControllerInfo(ControllerIDs.CommercialInvoice, "Enterprise.Customs.IE.Module", "Enterprise.Customs.IE.Module.CommercialInvoiceController", Constants.CountryCodes.Ireland));
			Add(new ControllerInfo(ControllerIDs.Customs.EU.NctsMovementController, "Enterprise.Customs.IE.NCTS.Module", "Enterprise.Customs.IE.NCTS.Module.IENctsMovementController", Constants.CountryCodes.Ireland));
			Add(new ControllerInfo(ControllerIDs.Customs.EU.UCC6TemporaryStorage, "Enterprise.Customs.IE.Module", "Enterprise.Customs.IE.Module.UCC6TemporaryStorageController", Constants.CountryCodes.Ireland));
			Add(new ControllerInfo(ControllerIDs.Customs.IE.CustomsAndExciseReports, "Enterprise.Customs.IE.Module", "Enterprise.Customs.IE.Module.CustomsAndExciseReportsController", Constants.CountryCodes.Ireland));

			// IT Customs Controllers
			Add(new ControllerInfo(ControllerIDs.Customs.IT.EntryHeaderController, "Enterprise.Customs.IT.Module", "Enterprise.Customs.IT.Module.EntryHeaderController", Constants.CountryCodes.Italy));
			Add(new ControllerInfo(ControllerIDs.Customs.EU.UCC6TemporaryStorage, "Enterprise.Customs.IT.TemporaryStorage.Module", "Enterprise.Customs.IT.TemporaryStorage.Module.UCC6TemporaryStorageController", Constants.CountryCodes.Italy));
			Add(new ControllerInfo(ControllerIDs.Customs.EU.TempStorageRegister, "Enterprise.Customs.IT.TemporaryStorage.Module", "Enterprise.Customs.IT.TemporaryStorage.Module.TempStorageRegisterController", Constants.CountryCodes.Italy));

			// BE Customs Controllers
			Add(new ControllerInfo(ControllerIDs.Customs.JobDeclaration, "Enterprise.Customs.BE.Module", "Enterprise.Customs.BE.Module.JobDeclarationController", Constants.CountryCodes.Belgium));
			Add(new ControllerInfo(ControllerIDs.Customs.SupplierPart, "Enterprise.Customs.BE.Module", "Enterprise.Customs.BE.Module.OrgSupplierPartController", Constants.CountryCodes.Belgium));

			// TR
			Add(new ControllerInfo(ControllerIDs.Customs.JobDeclaration, "Enterprise.Customs.TR.Module", "Enterprise.Customs.TR.Module.JobDeclarationController", Constants.CountryCodes.Turkey));
			Add(new ControllerInfo(ControllerIDs.Customs.SupplierPart, "Enterprise.Customs.TR.Module", "Enterprise.Customs.TR.Module.OrgSupplierPartController", Constants.CountryCodes.Turkey));

			// NO Customs
			Add(new ControllerInfo(ControllerIDs.Customs.NO.TemporaryStorageRegister, "Enterprise.Customs.NO.Module", "Enterprise.Customs.NO.Module.SumARegisterController", CountryCodes.Norway));
			Add(new ControllerInfo(ControllerIDs.Customs.NO.TemporaryStorageRegisterReadOnly, "Enterprise.Customs.NO.Module", "Enterprise.Customs.NO.Module.SumARegisterReadOnlyController", CountryCodes.Norway));

			// NL Customs Controllers
			Add(new ControllerInfo(ControllerIDs.Customs.JobDeclaration, "Enterprise.Customs.NL.Module", "Enterprise.Customs.NL.Module.JobDeclarationController", Constants.CountryCodes.Netherlands));
			Add(new ControllerInfo(ControllerIDs.Customs.SupplierPart, "Enterprise.Customs.NL.Module", "Enterprise.Customs.NL.Module.OrgSupplierPartController", Constants.CountryCodes.Netherlands));
			Add(new ControllerInfo(ControllerIDs.Customs.NL.OrganisationConsigneePlugIn, "Enterprise.Customs.NL.Module", "Enterprise.Customs.NL.Module.NLOrganisationConsigneePlugInController"));
			Add(new ControllerInfo(ControllerIDs.Customs.CusAuthorisations, "Enterprise.Customs.NL.Module", "Enterprise.Customs.NL.Module.CusAuthorisationsController", Constants.CountryCodes.Netherlands));
			Add(new ControllerInfo(ControllerIDs.Customs.EU.NctsMovementController, "Enterprise.Customs.NL.NCTS.Module", "Enterprise.Customs.NL.NCTS.Module.NctsMovementController", Constants.CountryCodes.Netherlands));
			Add(new ControllerInfo(ControllerIDs.Customs.NL.EntryHeader, "Enterprise.Customs.NL.Module", "Enterprise.Customs.NL.Module.EntryHeaderController", Constants.CountryCodes.Netherlands));

			// SE Customs Controllers
			Add(new ControllerInfo(ControllerIDs.Customs.JobDeclaration, "Enterprise.Customs.SE.Module", "Enterprise.Customs.SE.Module.JobDeclarationController", Constants.CountryCodes.Sweden));
			Add(new ControllerInfo(ControllerIDs.Customs.SupplierPart, "Enterprise.Customs.SE.Module", "Enterprise.Customs.SE.Module.OrgSupplierPartController", Constants.CountryCodes.Sweden));
			Add(new ControllerInfo(ControllerIDs.Customs.EU.NctsMovementController, "Enterprise.Customs.SE.NCTS.Module", "Enterprise.Customs.SE.NCTS.Module.NctsMovementController", Constants.CountryCodes.Sweden));

			// CH Customs Controllers
			Add(new ControllerInfo(ControllerIDs.Customs.JobDeclaration, "Enterprise.Customs.CH.Module", "Enterprise.Customs.CH.Module.JobDeclarationController", Constants.CountryCodes.Switzerland));
			Add(new ControllerInfo(ControllerIDs.Customs.JobDeclarationPluggedIntoShipment, "Enterprise.Customs.CH.Module", "Enterprise.Customs.CH.Module.JobDeclarationShipmentController", Constants.CountryCodes.Switzerland));
			Add(new ControllerInfo(ControllerIDs.Customs.SingleTariffClassification, "Enterprise.Customs.CH.Module", "Enterprise.Customs.CH.Module.CusClassificationController", Constants.CountryCodes.Switzerland));
			Add(new ControllerInfo(ControllerIDs.Customs.SupplierPart, "Enterprise.Customs.CH.Module", "Enterprise.Customs.CH.Module.OrgSupplierPartController", Constants.CountryCodes.Switzerland));
			Add(new ControllerInfo(ControllerIDs.Customs.Permits, "Enterprise.Customs.CH.Module", "Enterprise.Customs.CH.Module.CusPermitController", Constants.CountryCodes.Switzerland));
			Add(new ControllerInfo(ControllerIDs.CommercialInvoice, "Enterprise.Customs.CH.Module", "Enterprise.Customs.CH.Module.CommercialInvoiceController", Constants.CountryCodes.Switzerland));
			Add(new ControllerInfo(ControllerIDs.Customs.EU.NctsMovementController, "Enterprise.Customs.CH.NCTS.Module", "Enterprise.Customs.CH.NCTS.Module.NctsMovementController", Constants.CountryCodes.Switzerland));
			Add(new ControllerInfo(ControllerIDs.Customs.CH.EntryHeader, "Enterprise.Customs.CH.Module", "Enterprise.Customs.CH.Module.EntryHeaderController", Constants.CountryCodes.Switzerland));
			Add(new ControllerInfo(ControllerIDs.Customs.CH.CustomsSummary, "Enterprise.Customs.CH.Module", "Enterprise.Customs.CH.Module.CustomsSummaryController", Constants.CountryCodes.Switzerland));
			Add(new ControllerInfo(ControllerIDs.Customs.CH.DeclarationActivation, "Enterprise.Customs.CH.DeclarationActivation.Module", "Enterprise.Customs.CH.DeclarationActivation.Module.DeclarationActivationController", Constants.CountryCodes.Switzerland));

			// NO Customs Controllers
			Add(new ControllerInfo(ControllerIDs.Customs.JobDeclaration, "Enterprise.Customs.NO.Module", "Enterprise.Customs.NO.Module.JobDeclarationController", Constants.CountryCodes.Norway));
			Add(new ControllerInfo(ControllerIDs.Customs.JobDeclarationPluggedIntoShipment, "Enterprise.Customs.NO.Module", "Enterprise.Customs.NO.Module.JobDeclarationShipmentController", Constants.CountryCodes.Norway));
			Add(new ControllerInfo(ControllerIDs.Customs.SingleTariffClassification, "Enterprise.Customs.NO.Module", "Enterprise.Customs.NO.Module.CusClassificationController", Constants.CountryCodes.Norway));
			Add(new ControllerInfo(ControllerIDs.Customs.SupplierPart, "Enterprise.Customs.NO.Module", "Enterprise.Customs.NO.Module.OrgSupplierPartController", Constants.CountryCodes.Norway));
			Add(new ControllerInfo(ControllerIDs.CommercialInvoice, "Enterprise.Customs.NO.Module", "Enterprise.Customs.NO.Module.CommercialInvoiceController", Constants.CountryCodes.Norway));
			Add(new ControllerInfo(ControllerIDs.Customs.EU.NctsMovementController, "Enterprise.Customs.NO.NCTS.Module", "Enterprise.Customs.NO.NCTS.Module.NctsMovementController", Constants.CountryCodes.Norway));

			// IL Customs Controllers
			Add(new ControllerInfo(ControllerIDs.Customs.JobDeclaration, "Enterprise.Customs.IL.Module", "Enterprise.Customs.IL.Module.JobDeclarationController", Constants.CountryCodes.Israel));
			Add(new ControllerInfo(ControllerIDs.Customs.JobDeclarationPluggedIntoShipment, "Enterprise.Customs.IL.Module", "Enterprise.Customs.IL.Module.JobDeclarationShipmentController", Constants.CountryCodes.Israel));
			Add(new ControllerInfo(ControllerIDs.Customs.SingleTariffClassification, "Enterprise.Customs.IL.Module", "Enterprise.Customs.IL.Module.CusClassificationController", Constants.CountryCodes.Israel));
			Add(new ControllerInfo(ControllerIDs.Customs.SupplierPart, "Enterprise.Customs.IL.Module", "Enterprise.Customs.IL.Module.OrgSupplierPartController", Constants.CountryCodes.Israel));
			Add(new ControllerInfo(ControllerIDs.CommercialInvoice, "Enterprise.Customs.IL.Module", "Enterprise.Customs.IL.Module.CommercialInvoiceController", Constants.CountryCodes.Israel));
			Add(new ControllerInfo(ControllerIDs.Customs.IL.CustomsMessaging, "Enterprise.Customs.IL.Module", "Enterprise.Customs.IL.Module.CustomsMessagingController", Constants.CountryCodes.Israel));

			// IN Customs Controllers
			Add(new ControllerInfo(ControllerIDs.Customs.JobDeclaration, "Enterprise.Customs.IN.Module", "Enterprise.Customs.IN.Module.JobDeclarationController", Constants.CountryCodes.India));
			Add(new ControllerInfo(ControllerIDs.Customs.JobDeclarationPluggedIntoShipment, "Enterprise.Customs.IN.Module", "Enterprise.Customs.IN.Module.JobDeclarationShipmentController", Constants.CountryCodes.India));
			Add(new ControllerInfo(ControllerIDs.Customs.SingleTariffClassification, "Enterprise.Customs.IN.Module", "Enterprise.Customs.IN.Module.CusClassificationController", Constants.CountryCodes.India));
			Add(new ControllerInfo(ControllerIDs.Customs.SupplierPart, "Enterprise.Customs.IN.Module", "Enterprise.Customs.IN.Module.OrgSupplierPartController", Constants.CountryCodes.India));
			Add(new ControllerInfo(ControllerIDs.CommercialInvoice, "Enterprise.Customs.IN.Module", "Enterprise.Customs.IN.Module.CommercialInvoiceController", Constants.CountryCodes.India));
			Add(new ControllerInfo(ControllerIDs.Customs.IN.OrganisationConsignorPlugIn, "Enterprise.Customs.IN.Module", "Enterprise.Customs.IN.Module.INOrganisationConsignorPlugInController"));
			Add(new ControllerInfo(ControllerIDs.Customs.IN.OrganisationConsigneePlugIn, "Enterprise.Customs.IN.Module", "Enterprise.Customs.IN.Module.INOrganisationConsigneePlugInController"));
			Add(new ControllerInfo(ControllerIDs.Customs.IN.OrganisationDetailsPlugIn, "Enterprise.Customs.IN.Module", "Enterprise.Customs.IN.Module.INOrganisationDetailsPlugInController"));

			// DK Customs Controllers
			Add(new ControllerInfo(ControllerIDs.Customs.JobDeclaration, "Enterprise.Customs.DK.Module", "Enterprise.Customs.DK.Module.JobDeclarationController", Constants.CountryCodes.Denmark));
			Add(new ControllerInfo(ControllerIDs.Customs.SupplierPart, "Enterprise.Customs.DK.Module", "Enterprise.Customs.DK.Module.OrgSupplierPartController", Constants.CountryCodes.Denmark));

			// CO Customs Controllers
			Add(new ControllerInfo(ControllerIDs.Customs.CO.DocumentIDs, "Enterprise.Customs.CO.Manifest.Module", "Enterprise.Customs.CO.Manifest.Module.DocumentIDsController", Constants.CountryCodes.Colombia));

			// MX Customs Controllers
			Add(new ControllerInfo(ControllerIDs.Customs.JobDeclaration, "Enterprise.Customs.MX.Module", "Enterprise.Customs.MX.Module.JobDeclarationController", Constants.CountryCodes.Mexico));
			Add(new ControllerInfo(ControllerIDs.Customs.JobDeclarationPluggedIntoShipment, "Enterprise.Customs.MX.Module", "Enterprise.Customs.MX.Module.JobDeclarationShipmentController", Constants.CountryCodes.Mexico));
			Add(new ControllerInfo(ControllerIDs.Customs.SingleTariffClassification, "Enterprise.Customs.MX.Module", "Enterprise.Customs.MX.Module.CusClassificationController", Constants.CountryCodes.Mexico));
			Add(new ControllerInfo(ControllerIDs.Customs.SupplierPart, "Enterprise.Customs.MX.Module", "Enterprise.Customs.MX.Module.OrgSupplierPartController", Constants.CountryCodes.Mexico));
			Add(new ControllerInfo(ControllerIDs.CommercialInvoice, "Enterprise.Customs.MX.Module", "Enterprise.Customs.MX.Module.CommercialInvoiceController", Constants.CountryCodes.Mexico));

			//FI Customs Controller
			Add(new ControllerInfo(ControllerIDs.Customs.JobDeclaration, "Enterprise.Customs.FI.Module", "Enterprise.Customs.FI.Module.JobDeclarationController", Constants.CountryCodes.Finland));
			Add(new ControllerInfo(ControllerIDs.Customs.SupplierPart, "Enterprise.Customs.FI.Module", "Enterprise.Customs.FI.Module.OrgSupplierPartController", Constants.CountryCodes.Finland));

#if DEBUG
			// _CustomsTemplate_ Customs Controllers
			Add(new ControllerInfo(ControllerIDs.Customs.JobDeclaration, "Enterprise.Customs._CustomsTemplate_.Module", "Enterprise.Customs._CustomsTemplate_.Module.JobDeclarationController", Constants.CountryCodes._TemplateCountryName_));
			Add(new ControllerInfo(ControllerIDs.Customs.JobDeclarationPluggedIntoShipment, "Enterprise.Customs._CustomsTemplate_.Module", "Enterprise.Customs._CustomsTemplate_.Module.JobDeclarationShipmentController", Constants.CountryCodes._TemplateCountryName_));
			Add(new ControllerInfo(ControllerIDs.Customs.SingleTariffClassification, "Enterprise.Customs._CustomsTemplate_.Module", "Enterprise.Customs._CustomsTemplate_.Module.CusClassificationController", Constants.CountryCodes._TemplateCountryName_));
			Add(new ControllerInfo(ControllerIDs.Customs.SupplierPart, "Enterprise.Customs._CustomsTemplate_.Module", "Enterprise.Customs._CustomsTemplate_.Module.OrgSupplierPartController", Constants.CountryCodes._TemplateCountryName_));
			Add(new ControllerInfo(ControllerIDs.CommercialInvoice, "Enterprise.Customs._CustomsTemplate_.Module", "Enterprise.Customs._CustomsTemplate_.Module.CommercialInvoiceController", Constants.CountryCodes._TemplateCountryName_));

			// _EUCustomsTemplate_ Customs Controllers
			Add(new ControllerInfo(ControllerIDs.Customs.JobDeclaration, "Enterprise.Customs._EUCustomsTemplate_.Module", "Enterprise.Customs._EUCustomsTemplate_.Module.JobDeclarationController", Constants.CountryCodes._EUTemplateCountryName_));
			Add(new ControllerInfo(ControllerIDs.Customs.SupplierPart, "Enterprise.Customs._EUCustomsTemplate_.Module", "Enterprise.Customs._EUCustomsTemplate_.Module.OrgSupplierPartController", Constants.CountryCodes._EUTemplateCountryName_));
#endif

			// Messaging
			Add(new ControllerInfo(ControllerIDs.Messaging.EDICommunicationsMode, "Enterprise.Messaging.Module", "Enterprise.Messaging.Module.EDICommunicationsModeController"));
			Add(new ControllerInfo(ControllerIDs.Messaging.EDIInterchange, "Enterprise.Messaging.Module", "Enterprise.Messaging.Module.EDIInterchangeController"));
			Add(new ControllerInfo(ControllerIDs.Messaging.EDIMessage, "Enterprise.Messaging.Module", "Enterprise.Messaging.Module.EDIMessageController"));
			Add(new ControllerInfo(ControllerIDs.Messaging.EDICommunicationParty, "Enterprise.Messaging.Module", "Enterprise.Messaging.Module.EDICommunicationPartyController"));
			Add(new ControllerInfo(ControllerIDs.Messaging.EDIMessagePurpose, "Enterprise.Workflow.Module", "Enterprise.Workflow.Module.EDIMessagePurposeController"));
			Add(new ControllerInfo(ControllerIDs.Messaging.EDIMessageContentFilter, "Enterprise.Workflow.Module", "Enterprise.Workflow.Module.EDIMessageContentFilterController"));
			Add(new ControllerInfo(ControllerIDs.Messaging.EDIMessageDeliveryContext, "Enterprise.Workflow.Module", "Enterprise.Workflow.Module.EDIMessageDeliveryContextController"));
			Add(new ControllerInfo(ControllerIDs.Messaging.EDICodeMapping, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.EDICodeMappingController"));
			Add(new ControllerInfo(ControllerIDs.Messaging.UniversalValidationRule, "Enterprise.Workflow.Module", "Enterprise.Workflow.Module.ValidationRuleController"));

			// Resource Strings
			Add(new ControllerInfo(ControllerIDs.ResourceStrings, "Enterprise.ResourceStrings.Module", "Enterprise.ResourceStrings.Module.ResourceStringsController"));
			Add(new ControllerInfo(ControllerIDs.LocalLanguages, "Enterprise.ResourceStrings.Module", "Enterprise.ResourceStrings.Module.LocalLanguagesController"));
			Add(new ControllerInfo(ControllerIDs.TranslationFeedback, "Enterprise.ResourceStrings.Module", "Enterprise.ResourceStrings.Module.TranslationFeedbackController"));

			// Recruiter
			Add(new ControllerInfo(ControllerIDs.HRJobApplicant, "Enterprise.Recruiter.Module", "Enterprise.Recruiter.Module.HRJobApplicantController"));
			Add(new ControllerInfo(ControllerIDs.HRJobApplication, "Enterprise.Recruiter.Module", "Enterprise.Recruiter.Module.HRJobApplicationController"));
			Add(new ControllerInfo(ControllerIDs.HRJobRole, "Enterprise.Recruiter.Module", "Enterprise.Recruiter.Module.HRJobRoleController"));
			Add(new ControllerInfo(ControllerIDs.HRJobOpenings, "Enterprise.Recruiter.Module", "Enterprise.Recruiter.Module.HRJobOpeningsController"));
			Add(new ControllerInfo(ControllerIDs.HRGlbCompanyCampaign, "Enterprise.Recruiter.Module", "Enterprise.Recruiter.Module.HRGlbCompanyCampaignController"));
			Add(new ControllerInfo(ControllerIDs.HRGlbCompanyCampaignContact, "Enterprise.Recruiter.GUI", "Enterprise.Recruiter.GUI.HRGlbCompanyCampaignContactController"));
			Add(new ControllerInfo(ControllerIDs.LearningCentreCampaign, "Enterprise.Recruiter.Module", "Enterprise.Recruiter.Module.LearningCentreCampaignController"));
			Add(new ControllerInfo(ControllerIDs.LearningCentreExamPlugIn, "Enterprise.Recruiter.Module", "Enterprise.Recruiter.Module.LearningCentreExamPlugInController"));
			Add(new ControllerInfo(ControllerIDs.LearningCentreScaledTestPlugIn, "Enterprise.Recruiter.Module", "Enterprise.Recruiter.Module.LearningCentreScaledTestPlugInController"));
			Add(new ControllerInfo(ControllerIDs.GlbAccreditation, "Enterprise.Recruiter.Module", "Enterprise.Recruiter.Module.GlbAccreditationController"));
			Add(new ControllerInfo(ControllerIDs.GlbAccreditationAttempt, "Enterprise.Recruiter.Module", "Enterprise.Recruiter.Module.GlbAccreditationAttemptController"));
			Add(new ControllerInfo(ControllerIDs.GlbAccreditationGroup, "Enterprise.Recruiter.Module", "Enterprise.Recruiter.Module.GlbAccreditationGroupController"));
			Add(new ControllerInfo(ControllerIDs.HREmails, "Enterprise.Recruiter.Module", "Enterprise.Recruiter.Module.HREmailsController"));
			Add(new ControllerInfo(ControllerIDs.CandidateManagement, "Enterprise.Recruitment.Module", "Enterprise.Recruitment.Module.CandidateManagement.CandidateManagementPopupFormController"));
			Add(new ControllerInfo(ControllerIDs.ExamSetting, "Enterprise.Recruiter.Module", "Enterprise.Recruiter.Module.ExamSettingsController"));
			Add(new ControllerInfo(ControllerIDs.HRHiringRequest, "Enterprise.Recruiter.Module", "Enterprise.Recruiter.Module.HRHiringRequestController"));
			Add(new ControllerInfo(ControllerIDs.HROnBoarding, "Enterprise.Recruiter.Module", "Enterprise.Recruiter.Module.HROnBoardingController"));

			// Process Manager
			Add(new ControllerInfo(ControllerIDs.ProcessTasks, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.ProcessTasksController"));
			Add(new ControllerInfo(ControllerIDs.ProcessHeader, "Enterprise.BufferManagement.Module", "Enterprise.BufferManagement.Module.ProcessHeaderController"));
			Add(new ControllerInfo(ControllerIDs.ProcessHeaderLink, "Enterprise.BufferManagement.Module", "Enterprise.BufferManagement.Module.ProcessHeaderLinkController"));
			Add(new ControllerInfo(ControllerIDs.ProcessTaskForJob, "Enterprise.BufferManagement.Module", "Enterprise.BufferManagement.Module.ProcessTaskForJobController"));
			Add(new ControllerInfo(ControllerIDs.ProcessTemplates, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.ProcessTaskTemplateController"));
			Add(new ControllerInfo(ControllerIDs.ProcessCompanyLinkRule, "Enterprise.Workflow.Module", "Enterprise.Workflow.Module.ProcessCompanyLinkRuleController"));
			Add(new ControllerInfo(ControllerIDs.ProcessFieldChangeRule, "Enterprise.Workflow.Module", "Enterprise.Workflow.Module.ProcessFieldChangeRuleController"));
			Add(new ControllerInfo(ControllerIDs.BMTagDefinition, "Enterprise.BufferManagement.Module", "Enterprise.BufferManagement.Module.BMTagDefinitionController"));
			Add(new ControllerInfo(ControllerIDs.BMTagMagnitude, "Enterprise.BufferManagement.Module", "Enterprise.BufferManagement.Module.BMTagMagnitudeController"));
			Add(new ControllerInfo(ControllerIDs.BMTagRule, "Enterprise.BufferManagement.Module", "Enterprise.BufferManagement.Module.BMTagRuleController"));
			Add(new ControllerInfo(ControllerIDs.AcceptabilityBand, "Enterprise.BufferManagement.Module", "Enterprise.BufferManagement.Module.AcceptabilityBandController"));
			Add(new ControllerInfo(ControllerIDs.BMFilterRule, "Enterprise.BufferManagement.Module", "Enterprise.BufferManagement.Module.BMFilterRuleController"));
			Add(new ControllerInfo(ControllerIDs.Events, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.EventsController"));
			Add(new ControllerInfo(ControllerIDs.BMSystems, "Enterprise.BufferManagement.Module", "Enterprise.BufferManagement.Module.BMSystemsController"));
			Add(new ControllerInfo(ControllerIDs.BMBufferTimespan, "Enterprise.BufferManagement.Module", "Enterprise.BufferManagement.Module.BMBufferTimespanController"));
			Add(new ControllerInfo(ControllerIDs.BMBoard, "Enterprise.BufferManagement.Module", "Enterprise.BufferManagement.Module.BMBoardController"));
			Add(new ControllerInfo(ControllerIDs.VisualBoard, "Enterprise.BufferManagement.Module", "Enterprise.BufferManagement.Module.VisualBoardController"));
			Add(new ControllerInfo(ControllerIDs.BMBoardSlideshow, "Enterprise.BufferManagement.Module", "Enterprise.BufferManagement.Module.BMBoardSlideshowController"));
			Add(new ControllerInfo(ControllerIDs.BMComponent, "Enterprise.BufferManagement.Module", "Enterprise.BufferManagement.Module.BMComponentController"));
			Add(new ControllerInfo(ControllerIDs.ComponentRelationship, "Enterprise.BufferManagement.Module", "Enterprise.BufferManagement.Module.ComponentRelationshipController"));
			Add(new ControllerInfo(ControllerIDs.ViewComponentChangeLog, "Enterprise.BufferManagement.Module", "Enterprise.BufferManagement.Module.ViewComponentChangeLogController"));
			Add(new ControllerInfo(ControllerIDs.BMControlCustomisation, "Enterprise.BufferManagement.Module", "Enterprise.BufferManagement.Module.BMControlCustomisationController"));
			Add(new ControllerInfo(ControllerIDs.BMReleaseSequence, "Enterprise.BufferManagement.Module", "Enterprise.BufferManagement.Module.BMReleaseSequenceController"));
			Add(new ControllerInfo(ControllerIDs.NetworkDiagram, "Enterprise.BufferManagement.NetworkVisualisation.Module", "Enterprise.BufferManagement.NetworkVisualisation.Module.NetworkDiagramController"));
			Add(new ControllerInfo(ControllerIDs.WorkQueues, "Enterprise.BufferManagement.Module", "Enterprise.BufferManagement.Module.WorkQueuesController"));
			Add(new ControllerInfo(ControllerIDs.CompletionTriggerAction, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.CompletionTriggerActionController"));
			Add(new ControllerInfo(ControllerIDs.WorkflowExceptions, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.WorkflowExceptionsController"));
			Add(new ControllerInfo(ControllerIDs.WorkflowExceptionTypes, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.WorkflowExceptionTypesController"));
			Add(new ControllerInfo(ControllerIDs.WorkflowMilestones, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.WorkflowMilestonesController"));
			Add(new ControllerInfo(ControllerIDs.WorkflowTriggers, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.WorkflowTriggersController"));
			Add(new ControllerInfo(ControllerIDs.MENTAgedScoreQuery, "Enterprise.PAVE.MENT.Module", "Enterprise.PAVE.MENT.Module.MENTAgedScoreQueryController"));
			Add(new ControllerInfo(ControllerIDs.MENTAgedScoreExtraction, "Enterprise.PAVE.MENT.Module", "Enterprise.PAVE.MENT.Module.MENTAgedScoreExtractionController"));
			Add(new ControllerInfo(ControllerIDs.ExternalRequestTypes, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.ExternalRequestTypesController"));
			Add(new ControllerInfo(ControllerIDs.ExternalRequestInfoTemplate, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.ExternalRequestInfoTemplateController"));

			if (ClientHookLoader.Instance.ClientHook != null && ClientHookLoader.Instance.ClientHook.NewClientControllers != null)
			{
				Add(ClientHookLoader.Instance.ClientHook.NewClientControllers);
			}

			// PortMessaging
			Add(new ControllerInfo(ControllerIDs.PortMessaging, "Enterprise.Freight.Forwarding.PortMessaging.Module", "Enterprise.Freight.Forwarding.PortMessaging.Module.PortMessagingController"));
			Add(new ControllerInfo(ControllerIDs.ETerminalReleaseManifestPortMessaging, "Enterprise.Freight.Forwarding.Documents.Module", "Enterprise.Freight.Forwarding.Documents.Module.ETerminalReleaseManifestPortMessagingController"));

			// UniversalDataCarrierMessaging
			Add(new ControllerInfo(ControllerIDs.UniversalDataCarrierMessaging, "Enterprise.Freight.Forwarding.Module", "Enterprise.Freight.Forwarding.Module.UniversalDataCarrierMessagingController"));

			// Campaign Management
			Add(new ControllerInfo(ControllerIDs.GlbCompanyCampaign, "Enterprise.MarketingManager.Module", "Enterprise.MarketingManager.Module.CRMGlbCompanyCampaignController"));
			Add(new ControllerInfo(ControllerIDs.GlbCompanyCampaignItem, "Enterprise.MarketingManager.GUI", "Enterprise.MarketingManager.GUI.GlbCompanyCampaignItemController"));
			Add(new ControllerInfo(ControllerIDs.GlbCompanyCampaignItemSchedule, "Enterprise.MarketingManager.GUI", "Enterprise.MarketingManager.GUI.SendScheduleController"));
			Add(new ControllerInfo(ControllerIDs.VoteCampaignPlugIn, "Enterprise.MarketingManager.Module", "Enterprise.MarketingManager.Module.VoteCampaignPlugInController"));
			Add(new ControllerInfo(ControllerIDs.SurveyCampaignPlugIn, "Enterprise.MarketingManager.Module", "Enterprise.MarketingManager.Module.SurveyCampaignPlugInController"));

			// CargoIMP Phase 2
			Add(new ControllerInfo(ControllerIDs.CargoIMPPhase2, "Enterprise.Freight.Forwarding.Module", "Enterprise.Freight.Forwarding.Module.CargoIMPPhase2Controller"));

			Add(new ControllerInfo(ControllerIDs.ActiveUsers, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.ActiveUsersController"));

			Add(new ControllerInfo(ControllerIDs.ExportConsignmentReleaseAdvice, "Enterprise.Customs.Forwarding.Module", "Enterprise.Customs.Forwarding.Module.EXRELController"));

			// Packing
			Add(new ControllerInfo(ControllerIDs.Packing, "Enterprise.Packing.Module", "Enterprise.Packing.Module.PackingController"));
			Add(new ControllerInfo(ControllerIDs.PackingPlugIn, "Enterprise.Packing.Module", "Enterprise.Packing.Module.PackingPlugInController"));
			Add(new ControllerInfo(ControllerIDs.PalletTransaction, "Enterprise.Packing.Module", "Enterprise.Packing.Module.PalletTransactionController"));

			// eTail
			Add(new ControllerInfo(ControllerIDs.ETailShipment, "Enterprise.eTail.Module", "Enterprise.eTail.Module.ETailShipmentController"));

			// Sales Dashboard
			Add(new ControllerInfo(ControllerIDs.SalesDashboard, "Enterprise.MarketingManager.Module", "Enterprise.MarketingManager.Module.SalesDashboardController"));

			// Process Management
			Add(new ControllerInfo(ControllerIDs.WorkItem, "Enterprise.ProcessManagement.Module", "Enterprise.ProcessManagement.Module.WorkItemController"));
			Add(new ControllerInfo(ControllerIDs.Project, "Enterprise.ProcessManagement.Module", "Enterprise.ProcessManagement.Module.ProjectController"));
			Add(new ControllerInfo(ControllerIDs.CustomerServiceTicket, "Enterprise.ProcessManagement.Module", "Enterprise.ProcessManagement.Module.CustomerServiceTicketController"));

			Add(new ControllerInfo(ControllerIDs.UniversalCopySchedule, "Enterprise.UniversalCopy.Module", "Enterprise.UniversalCopy.Module.UniversalCopyScheduleController"));

			//Business Intelligence and Analytics
			Add(new ControllerInfo(ControllerIDs.PowerBiAnalyticsReports, "CargoWise.Bi.Product.Module", "CargoWise.Bi.Product.Module.Controller.PowerBiAnalyticsReportsController"));
			Add(new ControllerInfo(ControllerIDs.PowerBiAuditReports, "CargoWise.Bi.Product.Module", "CargoWise.Bi.Product.Module.Controller.PowerBiAnalyticsReportsController"));
			Add(new ControllerInfo(ControllerIDs.BiManager, "CargoWise.Bi.Product.Manager.GUI", "CargoWise.Bi.Product.Manager.Controller.BiManagerController"));
			Add(new ControllerInfo(ControllerIDs.Audit, "Enterprise.ZArchitecture.GUI.UserControls", "Enterprise.ZArchitecture.GUI.ZAudit.PlugIn.AuditController"));

			// Container Yard
			Add(new ControllerInfo(ControllerIDs.CYDAdHocServiceOrder, "Enterprise.Warehouse.Yard.Module", "Enterprise.Warehouse.Yard.Module.CYDAdHocServiceOrderController"));
			Add(new ControllerInfo(ControllerIDs.CYDDeliveryHeader, "Enterprise.Warehouse.Yard.Module", "Enterprise.Warehouse.Yard.Module.CYDDeliveryHeaderController"));
			Add(new ControllerInfo(ControllerIDs.CYDPickupHeader, "Enterprise.Warehouse.Yard.Module", "Enterprise.Warehouse.Yard.Module.CYDPickupHeaderController"));
			Add(new ControllerInfo(ControllerIDs.CYDReceiveAdvice, "Enterprise.Warehouse.Yard.Module", "Enterprise.Warehouse.Yard.Module.CYDReceiveAdviceController"));
			Add(new ControllerInfo(ControllerIDs.CYDReleaseAdvice, "Enterprise.Warehouse.Yard.Module", "Enterprise.Warehouse.Yard.Module.CYDReleaseAdviceController"));
			Add(new ControllerInfo(ControllerIDs.CYDTransportationUnit, "Enterprise.Warehouse.Yard.Module", "Enterprise.Warehouse.Yard.Module.CYDTransportationUnitController"));
			Add(new ControllerInfo(ControllerIDs.CYDYardUnitState, "Enterprise.Warehouse.Yard.Module", "Enterprise.Warehouse.Yard.Module.CYDYardUnitStateController"));
			Add(new ControllerInfo(ControllerIDs.MNRWorkOrder, "Enterprise.Warehouse.Yard.Module", "Enterprise.Warehouse.Yard.Module.MNRWorkOrderController"));
			Add(new ControllerInfo(ControllerIDs.MNRSurvey, "Enterprise.Warehouse.Yard.Module", "Enterprise.Warehouse.Yard.Module.MNRSurveyController"));
			Add(new ControllerInfo(ControllerIDs.CYDPeriodicInvoicing, "Enterprise.Warehouse.Yard.Module", "Enterprise.Warehouse.Yard.Module.CYDPeriodicInvoicingController"));

			// Gate Management
			Add(new ControllerInfo(ControllerIDs.GteBooking, "Enterprise.Warehouse.GateManagement.GUI", "Enterprise.Warehouse.GateManagement.GUI.GteBookingController"));
			Add(new ControllerInfo(ControllerIDs.GteGateMovementBooking, "Enterprise.Warehouse.GateManagement.GUI", "Enterprise.Warehouse.GateManagement.GUI.GteGateMovementBookingController"));
			Add(new ControllerInfo(ControllerIDs.GteGateMovement, "Enterprise.Warehouse.GateManagement.GUI", "Enterprise.Warehouse.GateManagement.GUI.GteGateMovementController"));
			Add(new ControllerInfo(ControllerIDs.GteVehicleMovement, "Enterprise.Warehouse.GateManagement.GUI", "Enterprise.Warehouse.GateManagement.GUI.GteVehicleMovementController"));

			//Container Load List
			Add(new ControllerInfo(ControllerIDs.ContainerLoadList, "Enterprise.Freight.Forwarding.Module", "Enterprise.Freight.Forwarding.Module.ContainerLoadListController"));

			//Container Load Plan
			Add(new ControllerInfo(ControllerIDs.ContainerLoadPlan, "Enterprise.Freight.Forwarding.Module", "Enterprise.Freight.Forwarding.Module.ContainerLoadPlanController"));

			//Carbon Emissions
			Add(new ControllerInfo(ControllerIDs.CO2ePlugin, "Enterprise.Freight.Forwarding.GUI", "Enterprise.Freight.Forwarding.GUI.CO2eController"));

			//Electronic Bill Of Lading
			Add(new ControllerInfo(ControllerIDs.ElectronicBOL, "Enterprise.Freight.Forwarding.Module", "Enterprise.Freight.Forwarding.Module.ElectronicBOLController"));

			// Dangerous Goods
			Add(new ControllerInfo(ControllerIDs.DangerousGoodsPlugin, "Enterprise.Freight.Forwarding.GUI", "Enterprise.Freight.Forwarding.GUI.DangerousGoods.DangerousGoodsController"));

			AddAdditionalControllerRegistrations();
		}

		#region Implementation

		void AddAdditionalControllerRegistrations()
		{
			foreach (var controllerInfo in ModuleListingSubsetRegister.GetRegisteredControllerInfos())
			{
				Add(controllerInfo);
			}
		}

		protected override void Add(ControllerInfo info)
		{
			ControllerInfo @override = null;

			if (RegistrationOverrides != null && RegistrationOverrides.ControllerOverrides != null)
			{
				bool useFallbackIfRegistrationForCountryNotFound = false;
				@override = RegistrationOverrides.ControllerOverrides[info.ID, info.CountryCode, useFallbackIfRegistrationForCountryNotFound];
			}

			if (@override == null)
			{
				base.Add(info);
			}
			else
			{
				base.Add(@override);
			}
		}

		#endregion

		internal readonly static ISet<string> EuCountriesThatHaveTheirOwnUCC6TemporaryStorageController = new HashSet<string>
		{
			Constants.CountryCodes.Ireland,
			Constants.CountryCodes.Italy,
			Constants.CountryCodes.Spain
		};

		internal readonly static ISet<string> EuCountriesThatHaveTheirOwnTemporaryStoragePremisesController = new HashSet<string>
		{
			Constants.CountryCodes.Spain
		};

#if DEBUG

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Method called via Reflection from Enterprise.ReflectionTestDeadCodeTest.TestNoDeadCode()")]
		[TypeFactoryAnnotationMethod]
		static IEnumerable<string> TypeFactoryAnnotation()
		{
			using (Db.DisposableActionForDbConnection())
			{
				return new ControllerList().All.Where(c => c != null && !string.IsNullOrEmpty(c.TypePath)).Select(c => c.TypePath);
			}
		}
#endif
	}

	#endregion
}
