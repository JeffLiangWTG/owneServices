using System;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Linq;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Modules
{
	#region Test Only
#if DEBUG

	public enum TestClientModuleId
	{
		TestClientModuleID,
		ClientModuleID,
		ClientModuleID1,
		ClientModuleID2
	}

#endif
	#endregion

	/// <summary>
	/// IMPORTANT: If removing or renaming one of these, please notify the team looking after the eRequest system. The system depends on these ModuleIDs being named exactly as is.
	/// </summary>
	public enum ModuleId
	{
		NotAssigned,
		Organisation,
		ClientIntelligence,
		CompetitorIntelligence,
		StaffAssignments,
		OrgCollectionCalls,
		Orders,
		JobShipmentPreplanning,
		ClientRates,
		GlobalRates,
		UniversalChargeCode,
		CarrierChargeCode,
		UniversalCommodityCode,
		UrsNamedAccount,
		Quotations,
		Costing,
		IntercompanyTariffs,
		WiseRates,
		WiseRatesCargoguide,
		WiseRatesCargoSphere,
		CarrierConnect,
		ProfitShare,
		TransactionsExport,
		XmlTransactionsImport,
		CsvTransactionsImport,
		CsvAccountsImportModule,
		OustandingJournalsExport,
		OustandingJournalsImport,
		APPaymentProcessing,
		ARPaymentProcessing,
		JobManagement,
		BulkDSBJobCloseBatchApproval,
		WIPAccruals,
		JobRevenueJournal,
		GLJournal,
		GLJournalApproval,
		GLConsolidationGroups,
		ServiceLevel,
		Opportunity,
		CrmOpportunity,
		RefVessel,
		RefVesselZZ,
		GLAccountFormat,
		Containers,
		ServiceRequest,
		QuotedBookings,
		OneOffQuotes,
		JobShipment,
		JobConsol,
		PackLines,
		ConsolPlanningBoard,
		CarrierContracts,
		ContractAllocationRoutes,
		GatewayConsolProfitShareRedistribution,
		RelatedTransportLegs,
		SupplierBooking,
		SupplierBookingLine,
		eLoadList,
		RoutingSolver,
		RefCommodityCode,
		RefOrgPartCategory,
		RefEquipment,
		ProductionRulesPortal,
		RefPremisesGateCode,
		AccChargeCode,
		AccGlobalChargeCode,
		AccChargeCodeForRegistry,
		AccTaxOverrideGroup,
		AccReportingBook,
		AccPlaceOfSupplyChargeCodeGroup,
		AccOrgTaxConfigurationTemplate,
		GlobalChargeCodeIntercompany,
		GlobalChargeCodeOrganization,
		RefCurrency,
		InternationalZone,
		RefTimeZoneSet,
		RefCountry,
		RefCityTown,
		RefPostCode,
		RefCountryStates,
		OrgAddresses,
		GenShapeGeography,
		Sales,
		SalesEnquiry,
		SalesProduct,
		Commission,
		CommissionApprovalRequest,
		CommissionAgreementLogFilter,
		Communication,
		OrgCommissionAgreement,
		OrgContacts,
		OrgContactsStmALog,
		OrgCusCode,
		RefContainer,
		RefContainerISOTypes,
		RefNMFC,
		CountryStatesGlbHoliday,
		RefUNLOCO,
		OrgCreditorGroup,
		OrgDebtorGroup,
		RefDocType,
		RefDocSource,
		RefDocOrgCusCode,
		GlbBranch,
		GlbBranchNotCurrentCompanyRelated,
		GlbCapability,
		GlbCompany,
		GlbCompanyCampaign,
		GlbCompanyCampaignWithoutFilter,
		GlbCompanyCampaignItem,
		GlbCompanyCampaignItemSchedule,
		GlbCompanyCampaignClick,
		GlbCompanyCampaignContact,
		DripMarketingFilterRule,
		DripMarketingFilterRuleHR,
		DripMarketingFilterRuleEDI,
		SalesDashboard,
		OrgSalesDashboard,
		GlbDepartment,
		UpdateNotesPortal,
		GlbGroup,
		NewsAndAnnouncement,
		AccGroups,
		AccGLHeader,
		AccGLAccountDescriptor,
		AccChequeBook,
		AccComplianceSequence,
		AccBankAccount,
		ImportAccountingData,
		OrderLine,
		FreightReport,
		CustomsGlblReport,
		CustomsReport,
		AccTaxRate,
		AccTaxRateForRegistry,
		AccInvMsg,
		AccWithholding,
		AccApportionmentTemplate,
		Location,
		ViewLocation,
		JobAirSailing,
		DocumentAllocation,
		DocumentDbMerger,
		DocumentDbManager,
		ArchiveEDocs,
		DocumentTemplate,
		RateAttachmentSet,
		SalesTeam,
		SalesRep,
		RateTransportProvider,
		RateTransportZone,
		PeriodManagement,
		OrdersReport,
		ShipmentReceival,
		AccHotCheque,
		JobCostingReport,
		PackContainerRegistration,
		LoadListConsol,
		JobMawb,
		ManifestTally,
		ShipmentGatePass,
		PrintJob,
		DocumentSigningJob,
		ArchiveSchedule,
		ArchivedRecords,
		ScheduledReports,
		ReportStatistics,
		ReportManagement,
		StmServiceTask,
		ProcessController,
		LicenceUsage,
		PrintQueue,
		StmMenuItem,
		GlbStaff,
		GlbPerson,
		ActiveUsers,
		JobSailing,
		JobSeaSailing,
		JobRailSailing,
		JobRoadSailing,
		JobSeaVoyage,
		SailingDataVendorImporting,
		OnlineSailingSchedules,
		DocumentTracking,
		ARAccQueryClaim,
		APAccQueryClaim,
		GlbPortDeliveryTime,
		BookingsReports,
		TransportReports,
		CFSCTOReports,
		SalesMgrReports,
		DocManagerReports,
		ReceivReports,
		PayablesReports,
		CashBookReports,
		GLReports,
		GLRepBooksReport,
		BudgetReports,
		NettingReports,
		EISReports,
		RefFilesReports,
		ArchiveReports,
		MasterDataReports,
		LocationsReports,
		AccountReports,
		CustFilesReports,
		ProcessMgrReports,
		SystemReports,
		UserAdminReports,
		TariffRateReports,
		Statement,
		ARTransaction,
		APTransaction,
		TransactionsPendingAllocation,
		CashbookTransaction,
		Cheque,
		ChequeTransaction,
		UnapprovedTransaction,
		UnapprovedIntercompanyTransaction,
		CASSCostFileImport,
		GenericCharge,
		GenericConsol,
		GenericTransaction,
		CNDataInterface,
		CN2004DataInterface,
		CNReconciliationExport,
		AccountingVoucher,
		ChinaJournalListing,
		DirectDebitFile,
		DepositBatch,
		BankReconcilliation,
		InvoiceBatch,
		AccPayableOrder,
		APEnquiry,
		AREnquiry,
		ARCreditNoteApproval,
		TransactionsPendingAllocationApproval,
		APInvoiceApproval,
		AccCollectionBatch,
		AccCollectionOrder,
		ARCashAdvance,
		APCashAdvance,
		CreditControlledDocumentsApproval,
		InvoicePrinting,
		PortDepotCarrierSelection,
		PortHubSelection,
		JobHeader,
		OrgMatchApproval,
		LocalCartageJobType,
		CartageType,
		StmUpgrade,
		UNDGSubstance,
		UNDGCommonData,
		UNDGCountryReference,
		RefCarrierConsortium,
		GLBudget,
		Registry,
		ErrorReporting,
		LDaaSDevices,
		TelematicsPreDriveChecklistTemplates,
		TelematicsPreDriveChecklists,
		AdministrationPanel,
		RefTransitTime,
		TransitTimeServiceLevelCombination,
		HVLVBookingHeader,
		HVLVOriginLoadList,
		HVLVOuterPackage,
		HVLVConsignment,
		JobBillingExRateSysConfig,
		ARComplianceDocument,
		APComplianceDocument,
		RefShippingLine,
		RefComplianceList,
		RefComplianceCommodityAlert,
		ExchangeRate,
		PaymentBatch,
		APInvoiceProcessingPortal,
		AssetManagementPortal,
		AssetMgmtReports,

		#region Business Intelligence and Analytics

		BiConfiguration,
		AnalyticsReports,
		BiDataExtract,
		BiManager,
		Audit,

		#endregion

		#region Barcode Parsing

		BarcodeParsing,
		BarcodeValidation,

		#endregion

		#region Carrier Messaging Buss

		RefAccessorial,
		RefMessagingBussCarrierInfo,

		#endregion

		#region Equipment Combination

		RefJobEquipment,

		#endregion

		#region Domestic Transport Booking

		DtbBooking,
		DtbBookingConsolidation,
		DtbBookingReports,
		DtbBookingTmpl,

		#endregion

		#region Land Transport

		DtbConsignment,

		#endregion

		#region Domestic Transport Consignment

		DtbBookingConsignment,
		DtbConsignmentRunSheet,
		DtbRoutePlanner,
		DtbLinehaulManifest,
		DtbReports,

		#endregion

		#region Product Warehouse

		WhsConfigWarehouse,
		WhsConfigLocation,
		WhsConfigLocationType,
		WhsConfigRow,
		WhsConfigArea,
		WhsConfigDynamicPickFaces,
		WhsConfigPickFaces,
		WhsConfigProduct,
		WhsConfigProductStyle,
		WhsConfigPutawayGroup,
		WhsReceive,
		WhsOrder,
		WhsOrderLine,
		WhsWorkOrder,
		WhsDynamicWorkOrder,
		WhsPicking,
		WhsRelease,
		WhsReleasePackageJob,
		WhsTransfer,
		WhsAdjustment,
		WhsStocktake,
		WhsStocktakeLine,
		WhsHandlingUnit,
		WhsInventory,
		WhsInventoryHeldCodes,
		WhsReport,
		WhsInvoicing,
		WhsEntryLine,
		WhsPickLine,
		WhsVASOrder,
		WhsCartonSize,
		WhsCartonGroup,
		WhsAdHocServiceJob,
		WhsSalesChannel,
		WhsProductionRulesPortal,
		WhsProductWarehousePortal,
		WhsLoad,

		#endregion

		#region Transit Warehouse

		WhsItemReceiveTransportationUnit,
		WhsTransitReport,
		WhsTransitReceiveConsignment,
		WhsTransitDispatchConsignment,
		WhsItemDispatchTransportationUnit,
		WhsItemReceiveASN,
		TransitHandlingUnit,
		TransitWarehouseAttachPackages,
		TransitWarehouseViewPackages,
		WhsItemTransferHeader,
		WhsItemDispatchLoadList,
		TransitWarehousePortal,

		#endregion

		ZARMatching,
		ZAPMatching,
		RefAirline,
		RefAirlineSpecialHandlingCode,
		RefDomesticCartageZone,
		MailItem,
		MailItemTemplate,
		AgencyBooking,
		AgencyBillOfLading,
		AgencyBillContainers,
		AgencyContainerDetention,
		AgencyContainerManager,
		AgencyContainerMove,
		AgencyVoyageAccounting,
		AgencySundryCharges,
		AgencyReports,
		LocalLanguages,
		ResourceStrings,
		TranslationFeedback,

		#region OceanCarrier

		OceanCarrierPortal,
		RouteSegments,
		CarrierShipmentHeader,
		CarrierServices,

		#endregion

		#region EquipmentManagement

		EquipmentManagementPortal,

		#endregion

		#region HR

		GlowHRMS,
		HRJobApplicant,
		HRJobRole,
		HRJobSkill,
		HRJobOpenings,
		HREmails,
		LearningCentreCampaign,
		HRGlbCompanyCampaign,
		HRGlbCompanyCampaignContact,
		HRReports,
		HRJobApplication,
		HRHiringRequest,
		HROnBoarding,

		#endregion

		#region Recruitment

		RecruitmentCandidateManagement,

		#endregion

		GlbAccreditation,
		GlbAccreditationAttempt,
		GlbAccreditationGroup,
		ExamSetting,
		ProcessTasks,
		ProcessHeader,
		ProcessHeaderLink,
		ProcessTemplates,
		ProcessCompanyLinkRule,
		ProcessFieldChangeRule,
		BMTagDefinition,
		BMTagMagnitude,
		BMTagRule,
		AcceptabilityBand,
		BMFilterRule,
		Events,
		BMSystems,
		BMBufferTimespan,
		BMBoard,
		VisualBoard,
		BMBoardSlideshows,
		BMReports,
		BMComponent,
		ComponentRelationship,
		ViewComponentChangeLog,
		BMControlCustomisation,
		BMReleaseSequence,
		NetworkDiagram,
		WorkQueues,
		CompletionTriggerAction,
		WorkflowExceptions,
		WorkflowExceptionTypes,
		WorkflowMilestones,
		WorkflowTriggers,
		ExternalRequestTypes,
		ExternalRequestInfoTemplate,
		ExternalRequests,
		MENTAgedScoreQuery,
		MENTAgedScoreExtraction,
		MENTSeries,
		Cartage,
		CartageWorkSheet,
		CartageLeg,
		CartageLegPlanner,
		CartageRunSheetDashboard,
		TradeLane,
		APIncompleteInvoices,
		ConsolidatedTransportBooking,
		PickupDeliveryConfirm,
		StmALog,
		StmModuleFilter,
		ComPayRegisteredOrganisations,
		GenCustomAddOnRule,
		ReviewProcess,
		ReviewProcessNode,
		RefPackType,
		Packing,
		PalletTransaction,
		AccGeneralLedgerData,
		AccComplianceReport,
		AlternateChartofAccounts,
		AlternateGLAccounts,
		NettingStatement,
		NettingPeriod,

		#region Customs

		#region Shared

		Classification,
		TariffBulkChange,
		SupplierPart,
		EntryLine,
		ImportClassification,
		ExportClassification,
		ImporterSecurityFiling,
		SendTestCustomsMessage,
		InvoiceLine,
		Permits,
		CusCalculationRules,
		Guarantees,
		CustomsRules,
		JobRequiredDocumentAddInfo,
		CusRefPreference,
		CusRefRateCode,
		CusPackingList,
		TradeGroups,
		CusRefTariffVersion,
		GoodsCatalog,

		BorderWiseWebReturnHook,

		TemporaryStorageRegister,
		TemporaryStorageRegisterReadOnly,
		ImportFromTemporaryStorageRegister,

	#endregion

	#region Universal

	RefCusTariff,
		ZZRefCarrier,
		ZZRefCusCodeList,
		ZZRefCusMap,
		RefDataGrouping,
		RefCusTradeGroup,
		ZZRefCusProcedure,
		ZZRefCusRuling,
		RefHarbourRate,

		#endregion

		#region ASYCUDA

		Manifest,
		ManifestBill,
		PreBoardingNotification,

		#region SGAccess
		SGAccessManifest,
		SGAccessManifestBill,
		#endregion

		#endregion

		#region AU

		AUImportTariffBulkChange,
		AUExportTariffBulkChange,
		AQISProducerCode,
		Premises,
		CMRCodeLists,
		InstrumentNumber,
		CMRLodgementQuestion,
		DrawbackEntryLine,

		#endregion

		#region ZA

		ZACustomsStatement,

		#endregion
		CommercialInvoice,
		CopyCommercialInvoice,

#if DEBUG
		NestedDummy,
		Dummy,
		Dummy2,
		Dummy3,
		JobCartageForTest,
		DummyWithExtendedDescription,
		DummyDependent,
		DummyDependentWithCode,
		DummyWithNoGLOWSupport,
		DummyNoPopup,
		DummyWithTemplates,
		DummyThatHitsFilterBizoOnDispose,
#endif
		CusDec,
		ImportCustomsFilesData,
		RefPacks,
		EntryHeader,
		TemporaryStorage,
		UCC6TemporaryStorage,

		CusPerson,

		ConsolidatedDeclaration,

		#region AU

		AUCustomsAirCargo,
		AirCargoDepotStandAlone,
		AirCargoOutturnBills,
		CusSCADepotContainer,
		CusSCADepotHouse,
		AUCustomsHouseAirCargo,
		AUExportCustomsManifest,
		AUCustomsAirCTOImport,
		AUCustomsAirCTOExport,
		VoyageManifest,
		SeaCargo,
		SeaCargoDepot,
		SeaCargoOutturnBills,
		AUCustomsHouseSeaCargo,
		CMREstablishmentCodes,
		NexDocNotifications,

		#endregion

		#region CA

		CAHTSTariffBulkChange,
		CACClass,
		CACExportTariff,
		CAQueryMessages,
		K84Reports,
		CADailyNoticeReconciliation,
		CAARLStatementOfAccount,
		CAReleaseNotifications,
		CACSubLocation,
		CAExportClassification,
		HTSClassification,
		CACFIAEndUseCodes,
		CACFIAMiscCodes,
		CATransactionNumberSetting,
		B2Adjustments,
		CAHouseBilleManifest,
		CAManifestForward,
		CALVXJobs,
		CusSCAOceanBill,
		CAJobDocAddresses,
		CACusRuling,
		CACSARevenueSummaryForm,

		#endregion

		CusClassificationModule,
		NctsMovementModule,
		NctsReportsModule,
		CcsukGenralMessage,
		GbCcsukReports,
		GbCcsukAirInventory,
		GbCcsukAirInventoryInConsol,
		GbCcsukAirInventoryHouse,
		CcsukMasterAndHouseCombined,
		GbCcsukAirInventoryHouseInShipment,
		GbCcsukStandAloneFsrEnquiry,
		GbDLUMessage,
		GbCcsukSplitHouse,
		GbCcsukSplitBasic,
		GBCDSDISQuery,
		GBCDSCashPayments,

		#region DE

		SumARegister,
		SumARegisterReadOnly,
		ImportFromSumARegister,
		ExportStatusRequest,
		MonthlyClosing,
		SimplifiedDeclaration,
		TaxChangeAssessment,

		#endregion

		#region EU

		EMCS,
		IntrastatReports,
		CusAuthorisations,
		ExitControl,
		ExitControlReport,
		EUH7,
		EUH7Bill,
		IntrastatTransactions,
		TempStorageRegister,
		TempStoragePremises,
		TempStorageRegisterLines,

		#endregion

		#region ES

		ESTemporaryStorageRegister,
		G3Declaration,

		#endregion

		#region PL

		AuthorisationRule,

		#endregion

		#region NZ

		NZCUSCAR,
		ECIWriteOffManifesting,
		Concession,
		OutwardReport,
		ExpressECI,
		InwardCargoReport,
		SeaCargoICR,

		#endregion

		#region SG

		SG4Tariff,
		SG4Classification,

		#endregion

		#region US

		USCAffirmationOCompliance,
		USCarrierCombined,
		USCCountry,
		USCForeignPort,
		USCRegionDistrictPort,
		USCTariff,
		USTariffBulkChange,
		USCTeamSpecialist,
		USCFIRMS,
		USCustomsStatement,
		USCVisa,
		USCVisaTariff,
		USCQuota,
		AMSBrokerDownloadMessage,
		BorderLineReleaseMessage,
		QueryMessage,
		Protest,
		Reconciliation,
		InBondNumber,
		USCRule,
		USCTariffRule,
		USCACCase,
		USEntryLine,
		CourtesyNoticesOfLiquidation,
		USCDataVersion,
		USInBond,
		USInBondMoveHeader,
		USCForeignAndRegionPort,
		USCCarrierAndFIRMS,
		USDrawback,
		USeManifest,
		USeManifestShipment,
		eManifestIntl,
		AMS,
		AMSBill,
		ThreeLetterRefAirline,
		USLowValueEntries,
		USLowValueEntriesBill,
		USLowValueEntriesDeclaration,

		#endregion

		#region Messaging

		EDIMessage,
		EDIMessagePurpose,
		EDICommunicationsMode,
		EDIInterchange,
		EDICommunicationParty,
		EDIMessageContentFilter,
		EDIMessageDeliveryContext,
		EDICodeMapping,
		UniversalValidationRule,

		#endregion

		#endregion

		#region Shipnet
		Shipnet,
		#endregion

		#region Process Management

		WorkItem,
		Project,
		CustomerServiceTicket,

		#endregion

		#region JP
		AFR,
		AFRBill,
		#endregion

		#region ZA

		ZA404ProofOfPayment,
		CustomsResponse,
		ZAOutturnAndGateInOut,
		ZARefCusTariff,
		ZAGenralMessage,
		ZAWarehouseOperatorTransactions,

		#endregion

		#region FR

		FRCustomsStatement,

		#endregion

		UniversalCopySchedule,

		#region Value Analysis

		ValueAnalysisForwardingOrg,
		ValueAnalysisCustomsBrokerageOrg,
		ValueAnalysisTransportOrg,
		ValueAnalysisWarehouseOrg,
		ValueAnalysisLinerAgencyOrg,

		ValueAnalysisForwardingOpp,
		ValueAnalysisCustomsBrokerageOpp,
		ValueAnalysisTransportOpp,
		ValueAnalysisWarehouseOpp,
		ValueAnalysisLinerAgencyOpp,

		#endregion

		DialogDefault,
		WarningAcknowledgement,

		#region TW
		TWTranshipment,
		TWSpecialCode,
		BriefCustomsDeclarations,
		#endregion

		#region TR
		ETrade,
		SimplifiedProcedureTransitSystem,
		StatementsStampDuty,
		#endregion

		#region BR
		BRLPCO,
		BRLPCODeclaration,
		BRLPCOEntryHeader,
		BRLicense,
		BRLicenseEntryHeader,
		BRForeignOperator,
		#endregion

		#region KR
		KRCustomsStatement,
		MiscRequestMessages,
		ExportEntryDetails,
		ImportEntryDetails,
		EntryCustomsBillsFor5UL,
		EntryDetailsFor5SG,
		EntryLineDetailsFor5UL,
		DocumentListMessages,
		CusReconDeclaration,
		#endregion

		#region CO
		DocumentIDs,
		#endregion

		MarketIntelligenceAndAnalytics,
		OceanCarrierBookingAnalysisReport,
		GateBooking,
		GateControl,
		RefAirlineCommodityCode,
		NumericCodeRefAirline,
		CarrierContractAndAllocations,
		ClientContractAndAllocations,
		GlbStaffChangeRequest,
		GlbStaffHoliday,
		ContainerLoadList,
		StmFeatureTest,
		RefFacility,
		ContainerLoadPlan,
		OrdersWebPortal,
		OrderLinesWebPortal,
		ControlTower,

		#region Container Yard

		CYDAdHocServiceOrder,
		CYDDeliveryHeader,
		CYDPickupHeader,
		CYDReceiveAdvice,
		CYDReleaseAdvice,
		ContainerYardPortal,
		CYDTransportationUnit,
		CYDYardUnitState,
		CYDYardReport,
		MNRWorkOrder,
		MNRSurvey,
		CYDPeriodicInvoicing,

		#endregion

		#region GateManagement

		GteBooking,
		GteGateMovementBooking,
		GteGateMovement,
		GteVehicleMovement,
		GateManagementPortal,

		#endregion

		#region IE

		IECustomsAndExciseReports,

		#endregion

		#region CH

		CHCustomsSummary,
		CHDeclarationActivation,

		#endregion

		#region GHG

		CO2eDashboard,

		#endregion

		DtbConsignmentWebPortal,
		DtbConsignmentRunSheetWebPortal
	}

	public static class ModuleIdExtenions
	{
		public static RegistrationIdentifier ToIdentifier(this ModuleId en)
		{
			return new RegistrationIdentifier(en.ToString());
		}

		public static ModuleIdentifier GetModuleID(this ModuleId en)
		{
			return ModuleIDs.AllExcludingClientModules.FirstOrDefault(module => (ModuleId)module.ID == en);
		}
	}

	[WTG.StaticAnalysis.Annotation.Immutable]
	[TypeConverter(typeof(ModuleIDConverter))]
	public class ModuleIdentifier : RegistrationIdentifier
	{
		public ModuleIdentifier(Enum iD, MultilingualString description)
			: base(iD.ToString())
		{
			this.ID = iD;
			this.Description = description;
			this.ExtendedDescription = description;
		}

		public ModuleIdentifier(Enum iD, MultilingualString description, MultilingualString extendedDescription)
			: this(iD, description)
		{
			this.ExtendedDescription = extendedDescription;
		}

		public readonly Enum ID;
		public readonly MultilingualString Description;
		public readonly MultilingualString ExtendedDescription;

		public override bool Equals(object obj)
		{
			return obj is ModuleIdentifier && base.Equals(obj);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Decruftification", "WTG3007:Overrides should not simply call base.", Justification = "Required for Equals override")]
		public override int GetHashCode() // keep compiler from giving a warning
		{
			return base.GetHashCode();
		}
	}

	[WTG.StaticAnalysis.Annotation.Immutable]
	public class ClientModuleIdentifier : ModuleIdentifier
	{
		public ClientModuleIdentifier(Enum iD, string description)
			: base(iD, (NoResString)description)
		{
		}

		public ClientModuleIdentifier(Enum iD, string description, string extendedDescription)
			: base(iD, (NoResString)description, (NoResString)extendedDescription)
		{
		}
	}

	[WTG.StaticAnalysis.Annotation.Immutable]
	public class OrgModuleIdentifier : ModuleIdentifier
	{
		protected internal OrgModuleIdentifier(ModuleId iD, MultilingualString description)
			: base(iD, description)
		{
		}
	}

	[WTG.StaticAnalysis.Annotation.Immutable]
	public class ClientOverrideModuleIdentifier : ModuleIdentifier
	{
		public ClientOverrideModuleIdentifier(ModuleIdentifier moduleIDToOverride)
			: this(moduleIDToOverride, moduleIDToOverride.Description)
		{
		}

		public ClientOverrideModuleIdentifier(ModuleIdentifier moduleIDToOverride, string description)
			: base(moduleIDToOverride.ID, (NoResString)description)
		{
			OverridenModuleID = moduleIDToOverride;
		}

		public readonly ModuleIdentifier OverridenModuleID;
	}

	public class ModuleInfo : RegistrationInfo
	{
		public ModuleInfo(ModuleIdentifier iD, Type moduleType, TableRegistrationInfo moduleTable = null)
			: base(iD, moduleType, moduleTable)
		{
		}

		public ModuleInfo(ModuleIdentifier iD, Type moduleType, string countryCode, TableRegistrationInfo moduleTable = null)
			: base(iD, moduleType, countryCode, moduleTable)
		{
		}

		public ModuleInfo(ModuleIdentifier iD, string moduleAssemblyName, string moduleClassFullName, TableRegistrationInfo moduleTable = null)
			: base(iD, moduleAssemblyName, moduleClassFullName, null, moduleTable)
		{
		}

		public ModuleInfo(ModuleIdentifier iD, string moduleAssemblyName, string moduleClassFullName, string countryCode, TableRegistrationInfo moduleTable = null)
			: base(iD, moduleAssemblyName, moduleClassFullName, countryCode, moduleTable)
		{
		}

		public ModuleInfo(ModuleIdentifier iD, string moduleAssemblyName, string moduleClassFullName, string countryCode, MultilingualString description, TableRegistrationInfo moduleTable = null)
			: base(iD, moduleAssemblyName, moduleClassFullName, countryCode, moduleTable)
		{
			this.description = description;
		}

		public new ModuleIdentifier ID
		{
			get { return (ModuleIdentifier)base.ID; }
		}

		public MultilingualString Description
		{
			get { return description ?? ID.Description; }
		}
		readonly MultilingualString description;
	}

	public class ClientOverrideModuleInfo : ModuleInfo
	{
		public ClientOverrideModuleInfo(ClientOverrideModuleIdentifier iD, Type controllerType)
			: base(iD, controllerType)
		{
		}

		public ClientOverrideModuleInfo(ClientOverrideModuleIdentifier iD, Type controllerType, string countryCode)
			: base(iD, controllerType, countryCode)
		{
		}

		public ClientOverrideModuleInfo(ClientOverrideModuleIdentifier iD, string moduleAssemblyName, string moduleClassFullName, TableRegistrationInfo moduleTable = null)
			: base(iD, moduleAssemblyName, moduleClassFullName, moduleTable)
		{
		}

		public ClientOverrideModuleInfo(ClientOverrideModuleIdentifier iD, string moduleAssemblyName, string moduleClassFullName, string countryCode)
			: base(iD, moduleAssemblyName, moduleClassFullName, countryCode)
		{
		}

		public override bool IsClientOverride
		{
			get { return true; }
		}
	}

	#region Module ID Converter

	public class ModuleIDConverter : TypeConverter
	{
		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			if (destinationType == typeof(InstanceDescriptor))
			{
				return true;
			}
			else
			{
				return base.CanConvertTo(context, destinationType);
			}
		}

		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			if (sourceType == typeof(string))
			{
				return true;
			}
			else
			{
				return base.CanConvertFrom(context, sourceType);
			}
		}

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (destinationType == typeof(InstanceDescriptor))
			{
				var moduleID = (ModuleIdentifier)value;
				if (moduleID == ModuleIDs.NotAssigned)
				{
					var fieldInfo = typeof(ModuleIDs).GetProperty(nameof(ModuleIDs.NotAssigned));
					return new InstanceDescriptor(fieldInfo, Array.Empty<object>());
				}

				foreach (var moduleInfo in modules.All)
				{
					if (moduleID == moduleInfo.ID)
					{
						var propertyInfo = typeof(ModuleInfo).GetProperty(nameof(ModuleInfo.ID));
						return new InstanceDescriptor(propertyInfo, new object[] { moduleInfo });
					}
				}
			}

			return base.ConvertTo(context, culture, value, destinationType);
		}

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			if (value.GetType() == typeof(string))
			{
				var valueAsString = (string)value;
				foreach (var moduleInfo in modules.All)
				{
					if (moduleInfo.ID.ToString() == valueAsString)
					{
						return moduleInfo.ID;
					}
				}

				return ModuleIDs.NotAssigned;
			}
			else
			{
				return base.ConvertFrom(context, culture, value);
			}
		}

		readonly ModuleList modules = new();
	}

	#endregion
}
