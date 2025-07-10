namespace Enterprise.DataPurge
{
	class OperationalScriptRepository : ScriptRepository
	{
		protected override string[] PurgeScripts
		{
			get
			{
				return new string[] {
					ProcessWorkflowExceptionScript,
					ReportAndDocSchedulerScript,
					CommissionScript,
					TaskOpportunityCampaignManagementScript,
					CustomsGeneralScript,
					EUCustomsInBondScript,
					USCustomsISFScript,
					AccTransactionHeaderScript,
					AccGLAggregateScript,
					AccGLBudgetScript,
					HRSubsystemScript,
					RefContainerStock,
					HouseBillScript,
					HVLVScript,
					CFSAndCYScript,
					TransportBookingScript,
					TransitWarehouseScript,
					JobHeaderScript,
					WhsPackingScript,
					PackageJobScript,
					JobVoyageScript,
					OtherJobsScript,
					WarehouseScript,
					StorageJobsScript,
					PrintJobsScript,
					WorkItemsScript,
					EDIInterchangeScript,
					LicenceScript,
					VotingExamSurveySystemScript,
					NonTemplateWorkflowTasksScript,
					OtherOperationTablesScript,
					BookingOneTimeQuotesScript,
					NumberFountainScript,
					TemplateRecordScript,
					DashDocumentsScript
				};
			}
		}

		#region Purge Scripts

		#region House Bill

		const string HouseBillScript = @"
		-- House Bill
DELETE dbo.CusUSLVItemPGA;
DELETE dbo.CusUSLVItem;
DELETE dbo.CusUSLVConsignment;
DELETE dbo.CusUSLVClearance;

DELETE dbo.CusPartShip;
DELETE dbo.CusHAWBItems;
DELETE dbo.CusHAWB;
DELETE dbo.CusMAWB;

DELETE dbo.CusSCAPivot;
DELETE dbo.CusSCAHouse;
DELETE dbo.CusSCAContainer;
DELETE dbo.CusSCAOceanBill;
";

		#endregion

		#region HVLV

		const string HVLVScript = @"
		-- HVLV
DELETE dbo.HVLVReturnPivot
DELETE dbo.HVLVUsage;
DELETE dbo.HVLVItemLine;
DELETE dbo.HVLVItem;
DELETE dbo.HVLVOuterPackage;
DELETE dbo.HVLVConsignment;
DELETE dbo.HVLVConsignmentHeader;
DELETE dbo.HVLVBookingHeader;
DELETE dbo.HVLVOriginLoadList;
DELETE dbo.HVLVScanningSummary;
";

		#endregion

		#region HVLV

		const string CFSAndCYScript = @"
		-- CFS and CY
DELETE dbo.GateTransportCFSDetail;

DELETE dbo.GateTransportCYDetail;

DELETE dbo.GateTransport;

DELETE dbo.RateLines
FROM dbo.RateLines
INNER JOIN dbo.RateEntry ON TL_TI = TI_PK
WHERE TI_ParentID IS NOT NULL AND TI_ParentTableCode = 'CYS'

DELETE dbo.RateEntry
WHERE TI_ParentID IS NOT NULL AND TI_ParentTableCode = 'CYS'
";

		#endregion

		#region Transport Booking

		const string TransportBookingScript = @"
		-- Domestic Transport Booking
DELETE dbo.SupplierBookingLine;
DELETE dbo.DtbBookingConfirmation;
DELETE dbo.DtbBookingInstructionPkgDivot;
DELETE dbo.DtbBookingInstruction;
DELETE dbo.DtbConsignmentLeg;
DELETE dbo.DtbConsignmentActionPackageDivot;
DELETE dbo.DtbConsignmentAction;
DELETE dbo.DtbConsignmentAddress;
DELETE dbo.DtbLinehaulManifestLeg;
DELETE dbo.DtbLinehaulManifest;
DELETE dbo.DtbConsignmentLodgementPivot;
DELETE dbo.DtbConsignmentLodgement;
DELETE dbo.DtbConsignmentRunSheetInstruction;
DELETE dbo.DtbConsignmentRunSheet;
DELETE dbo.DtbConsignment;
DELETE dbo.DtbAgentBooking;
DELETE dbo.DtbBooking;
DELETE dbo.DtbBookingConsolidation;
";

		#endregion

		#region Transit Warehouse

		const string TransitWarehouseScript = @"
		-- Transit Warehouse
DELETE dbo.WhsItemUnloadTask;
DELETE dbo.WhsItemCycleCountLocationVariance;
DELETE dbo.WhsItemTransferLine
DELETE dbo.WhsItemPackageStateReturnDetail;
DELETE dbo.WhsItemPackageState;
DELETE dbo.WhsItemReceiveConsignmentRTUDivot;
DELETE dbo.WhsItemReceiveConsignment;
DELETE dbo.WhsItemReceiveASNRTUPivot;
DELETE dbo.WhsItemReceiveTransportationUnit;
DELETE dbo.WhsItemReceiveASN;
DELETE dbo.WhsItemDispatchConsignment;
DELETE dbo.WhsItemDispatchLoadListDTUPivot;
DELETE dbo.WhsItemDTUReturnDetail;
DELETE dbo.WhsItemDispatchTransportationUnit;
DELETE dbo.WhsItemDispatchLoadList;
DELETE dbo.WhsItemConsignmentOrderReference;
";

		#endregion

		#region Whs Packing

		const string WhsPackingScript = @"
-- Whs Trolley Picking
DELETE dbo.WhsPickTrolleySlot
DELETE dbo.WhsPickTrolleyJob

-- Whs Pick By Label
DELETE dbo.WhsPickByLabelLabel
DELETE dbo.WhsPickByLabelJob

-- Whs Load
DELETE dbo.WhsLoadPkgPackagePivot
";

		#endregion

		#region Package Job

		const string PackageJobScript = @"
-- Package Job
DELETE dbo.PkgPackageContainer
DELETE dbo.PkgPackageItemDivot
DELETE dbo.PkgPackageJobPackageHeaderPivot
DELETE dbo.PkgPackageBookedDetail
DELETE dbo.PkgPackageScreening
DELETE dbo.PkgPackageSeal
DELETE dbo.PkgPackageHold
DELETE dbo.PkgPackageTemperature
DELETE dbo.PkgPackageHandlingUnitDivot
DELETE dbo.PkgHandlingUnit
DELETE dbo.PkgPackageExtension
DELETE dbo.PkgPackage
DELETE dbo.PkgPackageHeader
DELETE dbo.PkgPackageJob
";

		#endregion

		#region Report and Doc Scheduler

		const string ReportAndDocSchedulerScript = @"
--StmScheduleTask
DELETE dbo.StmScheduleTaskCopyRecipient
DELETE dbo.StmScheduleTaskRecipient
DELETE dbo.StmReportRun
-- Purge StmScheduleTask which are not Service Task schedules
DELETE dbo.StmScheduleTask WHERE S5_ParentTableCode != 'SH'";

		#endregion

		#region Commission

		const string CommissionScript = @"
--Commission
DELETE dbo.AccCommissionApprovalRequestItem
DELETE dbo.AccCommissionApprovalRequest
DELETE dbo.AccCommissionLine
DELETE dbo.AccCommissionLineGroup
DELETE dbo.AccCommissionHeader
DELETE dbo.OrgCommissionCalculationQueue
";

		#endregion

		#region Task/Opportunity/Campaign Management

		const string TaskOpportunityCampaignManagementScript = @"
--OrgOpportunity
DELETE dbo.WorkProject;
DELETE dbo.OrgOpportunityValue;
DELETE dbo.OrgOpportunityStageProgress;
DELETE dbo.OrgOpportunity;
DELETE dbo.OrgColdCallRegister;
DELETE dbo.OrgSalesCallAdditionalAttendee;
DELETE dbo.OrgSalesCall;
";

		#endregion

		#region CustomsGeneral

		const string CustomsGeneralScript = @"
--CustomsGeneral
DELETE dbo.CusHouseContPackInvoiceHeaderPivot
";
		#endregion

		#region EU Customs InBond

		const string EUCustomsInBondScript = @"
--EUCustomsInBondScript
DELETE dbo.CusInBondEvent
DELETE dbo.CusESNctsHeader
DELETE dbo.CusFRNctsHeader
";

		#endregion

		#region US Customs ISF / InBond / AMS

		const string USCustomsISFScript = @"
--US Customs InBond / AMS
-- Right Hand Side of Diag
DELETE dbo.CusInBondVehicleCtrl
DELETE dbo.CusInBondFee
DELETE dbo.CusInBondCargoDesc
DELETE dbo.CusInBondContainer
DELETE dbo.CusInBondMoveLineItem
DELETE dbo.CusInBondMoveDetail
DELETE dbo.CusInBondPayInfo
DELETE dbo.CusInBondMoveHeader
-- Left Hand Side of Diag
DELETE dbo.CusInbondBillAddRef
DELETE dbo.CusInBondBill
DELETE dbo.CusInBondHeader

--US Customs ISF
DELETE dbo.CusISFLine
DELETE dbo.CusISFEquip
DELETE dbo.CusISFBill
DELETE dbo.CusISFHeader";

		#endregion

		#region AccTransactionHeader

		const string AccTransactionHeaderScript = @"
--AccTransactionHeader
DELETE dbo.AccGeneralLedgerData
DELETE dbo.AccDraftInvoiceJobReference
DELETE dbo.AccDraftInvoiceJob
DELETE dbo.AccDraftInvoiceJobCluster
DELETE dbo.AccDraftInvoiceExRate
DELETE dbo.AccDraftInvoiceProcessingErrorLog
DELETE dbo.AccDraftInvoiceHeader
DELETE dbo.AccTaxGLMovementQueue
DELETE dbo.AccTaxGLMovement
DELETE dbo.AccTaxRecordTransactionLinePivot
DELETE dbo.AccTaxTransaction
DELETE dbo.AccCashBasisVATQueue
DELETE dbo.AccCashBasisVAT
DELETE dbo.GenApprovalRequest
DELETE dbo.AccConsolidationBatch
DELETE dbo.AccStatement
DELETE dbo.AccOrgBalance
DELETE dbo.AccOrgBalanceChanges
DELETE dbo.AccEPaymentDeal
DELETE dbo.AccEPaymentQuote
DELETE dbo.AccPaymentApprovalItem
DELETE dbo.AccPaymentApproval
DELETE dbo.AccHotCheque
DELETE dbo.AccQueryClaim
DELETE dbo.AccComplianceDocumentPivot
DELETE dbo.AccComplianceDocumentLine
DELETE dbo.AccTransLinePay
DELETE dbo.JobChargeAttrib
DELETE dbo.JobPaymentBasis
DELETE dbo.JobChargeTarget
DELETE dbo.JobChargePostingQueue
DELETE dbo.JobCharge
DELETE dbo.JobConsolCostAttrib
DELETE dbo.JobConsolCost
DELETE dbo.AccTransactionLineSubAccount
DELETE dbo.AccTransactionLineDissectionAttribute
DELETE dbo.AccTransactionLines
DELETE dbo.AccTransactionMatchLink
DELETE dbo.AccTransactionHeaderReference
DELETE dbo.AccTransactionHeaderNettingLink
DELETE dbo.AccCollectionOrderLine
DELETE dbo.AccCollectionOrder
DELETE dbo.AccCollectionBatch
DELETE dbo.AccPayableOrderLine
DELETE dbo.AccPayableOrderHeader
DELETE dbo.AccTransactionHeaderSubAccount
DELETE dbo.DsbJobCloseBatch
DELETE dbo.AccEInvoicingTransactionPivot
DELETE dbo.AccEInvoicingBatch
DELETE dbo.AccTransactionHeaderAuthorisationRecord
DELETE dbo.AccTransactionHeaderFiscalization
DELETE dbo.AccTransactionHeader";

		#endregion

		#region AccGLAggregate

		const string AccGLAggregateScript = @"
--AccGLAggregate
DELETE dbo.AccGLAggregate";

		#endregion

		#region AccGLBudget

		const string AccGLBudgetScript = @"
--AccGLBudget
DELETE dbo.AccGLBudgetLines
DELETE dbo.AccGLBudget";

		#endregion

		#region HR Subsystem

		const string HRSubsystemScript = @"
--HR Subsystem
DELETE dbo.TalActivityResult
DELETE dbo.TalApplicationActivity
DELETE dbo.TalDefaultActivity
DELETE dbo.TalActivity
DELETE dbo.TalApplicationStage
DELETE dbo.TalDefaultStage
DELETE dbo.TalEducationHistory
DELETE dbo.TalExtracurricular
DELETE dbo.TalWorkHistory
DELETE dbo.TalWorkReference
DELETE dbo.GlbAccreditationGroupPivot
DELETE dbo.GlbAccreditationGroup
DELETE dbo.GlbAccreditationJobSkillPivot
DELETE dbo.GlbAccreditationJobSkillGroup
DELETE dbo.GlbAccreditationRequirementPivot
DELETE dbo.GlbAccreditationAttempt
DELETE dbo.GlbAccreditation
DELETE dbo.GlbStaffManager
DELETE dbo.GlbEmploymentHistory
DELETE dbo.HRJobApplicantSkillRatingTest
DELETE dbo.HRJobApplicationInterview
DELETE dbo.HRjobSkillTest
DELETE dbo.HRJobApplicationParsingQueue
DELETE dbo.HRJobApplicantApplicationRating
DELETE dbo.HRJobApplicationDocument
DELETE dbo.HRJobApplication
DELETE dbo.TalAdPostTarget
DELETE dbo.TalAdvert
DELETE dbo.HRJobAdPlacement
DELETE dbo.HRJobRoleSkillPivot
DELETE dbo.HRRecruitmentJobCampaign
DELETE dbo.HRJobRole
DELETE dbo.HRJobApplicantSkillRating
DELETE dbo.HRJobSkillExamGroup
DELETE dbo.HRJobSkill
DELETE hrm.HRCandidateEntitlement
DELETE dbo.HRHiringRequest
DELETE dbo.HROnBoarding
DELETE hrm.HROnBoardingEntitlement
DELETE dbo.HRJobApplicant";

		#endregion

		#region RefContainerStock

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "this is an SQL statement")]
		const string RefContainerStock = @"
-- RefContainerStock
DELETE dbo.JobContainerMove
DELETE dbo.JobContainerDetention
DELETE dbo.RefContainerStock";

		#endregion

		#region JobHeader

		const string JobHeaderScript = @"
--JobHeader
DELETE dbo.LandCostInput;
DELETE dbo.LandedCostHistory;
DELETE dbo.OrgLandedCostingPrefCharges;
DELETE dbo.OrgLandedCostingPrefs;
DELETE dbo.JobComInvLineComponentInventory;
DELETE dbo.JobComInvLineRefs;
DELETE dbo.JobContainerPackPivot;
DELETE dbo.JobOrderLineDeliverContainer;
DELETE dbo.JobPackLoc;
DELETE dbo.JobTransportLegPackLineDivot;
DELETE dbo.ContainerLoadListLine;
DELETE dbo.JobOrderLineDelivery;
DELETE dbo.OrderShipmentPlanningLine;
DELETE dbo.OrderShipmentPlanning;
DELETE dbo.JobSupplierBookingLine;
DELETE dbo.JobSupplierBookingLineGroup;
DELETE dbo.JobPackProduct;
DELETE dbo.JobPackLinePackage;
DELETE dbo.JobPackLinePortMessaging;
DELETE dbo.JobPackLineHarmonisedCode;
DELETE dbo.JobPackLines;
DELETE dbo.JobComInvoiceHeaderRefs;
DELETE dbo.CusHouseContPackInvoiceLinePivot;
DELETE dbo.CusContainerInvoiceLinePivot;
DELETE dbo.CusContainerEntryHeaderPivot
DELETE dbo.QuarantineColsDirection
DELETE dbo.QuarantineColsHeader
DELETE dbo.QuarantineExDocEstablishmentAndTime;
DELETE dbo.QuarantineExDocLine;
DELETE dbo.CusUnderbondDec;
DELETE dbo.CusRulingConfig;
DELETE dbo.CusTWProductLabelRange;
DELETE dbo.JobComInvoiceLineTax;
DELETE dbo.CusPackableItem;
DELETE dbo.CusAuthorizationUsage;
DELETE dbo.JobUSComInvoiceLine;
DELETE dbo.JobTWComInvoiceLine;
DELETE dbo.JobComInvoiceLine;
DELETE dbo.JobComInvHeaderCharge;
DELETE dbo.CusEntryLineFee;
DELETE dbo.CusSCADepotHouse;
DELETE dbo.CusDecHouseContainerPack;
DELETE dbo.CusEntryCPDec;
DELETE dbo.CusEntryHeaderCharges;
DELETE dbo.CusEntryLine;
DELETE dbo.CusStatementLineGroupFinancialDetail
DELETE dbo.CusStatementLineGroup
DELETE dbo.CusStatementLineCharge;
DELETE dbo.CusStatementLine;
DELETE dbo.CusStatementHeader;
DELETE dbo.CusDecHouseContainerPivot;
DELETE dbo.CusSCADepotContainer;
DELETE dbo.AccCashAdvanceRequestLine;
DELETE dbo.AccCashAdvanceRequestHeader;
DELETE dbo.ShipmentProfitShares;
DELETE dbo.QuarantineExDocShipsCompartment;
DELETE dbo.QuarantineExDocHeader;
DELETE dbo.CusPackingList;
DELETE dbo.JobCAComInvoiceHeader;
DELETE dbo.JobComInvoiceHeader;
DELETE dbo.JobOrderContainer;
DELETE dbo.LocalCartageVehicleActivity;
DELETE dbo.JobContainerLegs;
DELETE dbo.JobContainerPenalty;
DELETE dbo.JobBookedCtgMovPkgDivot;
DELETE dbo.JobBookedCtgMove;
DELETE dbo.JobCartageRunSheet;
DELETE dbo.JobOrderLine;
DELETE dbo.JobOrderHeader;
DELETE dbo.JobShipmentPreplanning;
DELETE dbo.JobDecRefs;
DELETE dbo.CusContainerEntryInstructionPivot;
DELETE dbo.CusContainer;
DELETE dbo.JobPickupDeliveryConfirm
DELETE dbo.JobContainer;
DELETE dbo.CusUSDecHouseBill;
DELETE dbo.CusDecHouseBill;
DELETE dbo.CusEntryPayInfo;
DELETE dbo.CusEUEntryHeader;
DELETE dbo.CusReconSnapshot;
DELETE dbo.CusReconCustomsCharge;
DELETE dbo.CusReconEntryLine;
DELETE dbo.CusReconEntry;
DELETE dbo.CusReconDeclaration;
DELETE dbo.CusEntrySnapshot;
DELETE dbo.CusEntryHeader;
DELETE dbo.CusTWControllingMessageHeader;
DELETE dbo.CusTRPreviousDocumentItem;
DELETE dbo.CusTRPreviousDocument;
DELETE dbo.CusCNEntryInstruction;
DELETE dbo.CusEntryInstruction;
DELETE dbo.CusDV1Detail;
DELETE dbo.JobCADeclaration;
DELETE dbo.JobUSDeclaration;
DELETE dbo.JobEUDeclaration;
DELETE dbo.JobSGDeclaration;
DELETE dbo.JobDeclaration;
DELETE dbo.JobExRate;
DELETE dbo.JobCartage;
DELETE dbo.JobToCloseQueue;
DELETE dbo.JobHeader;
DELETE dbo.AsycudaPackPackedItemPivot
DELETE dbo.AsycudaBillScreening;
DELETE dbo.AsycudaTransferBill;
DELETE dbo.AsycudaTransferHeader;
DELETE dbo.AsycudaTax;
DELETE dbo.AsycudaPackedItem;
DELETE dbo.AsycudaContainerBillOrPackageLink;
DELETE dbo.AsycudaArrivalLine;
DELETE dbo.AsycudaPack;
DELETE dbo.AsycudaContainer;
DELETE dbo.AsycudaArrivalHeader;
DELETE dbo.AsycudaBill;
DELETE dbo.AsycudaManifestHeader;
DELETE dbo.ExportCustomsManifestLines;
DELETE dbo.ExportCustomsManifestHeader;
DELETE dbo.CusOutturn;
DELETE dbo.CusUnderbond;
DELETE dbo.CusOutturnHeader;
DELETE dbo.CusSeaManOBLDetail;
DELETE dbo.CusSeaManOBLHeader;
DELETE dbo.CusSeaManArrivalPort;
DELETE dbo.SupplierBookingLine;
DELETE dbo.SupplierBookingHeader;
DELETE dbo.JobConsolTransport;
DELETE dbo.JobConShipLink;
DELETE dbo.JobMawb;
DELETE dbo.JobConsolAWBSpecialHandling;
DELETE dbo.JobConsolDGRestrictions;
DELETE dbo.JobConsol;
DELETE dbo.ELoadList;
DELETE dbo.JobShipmentGateway;
DELETE dbo.JobShipmentPortMessaging;
DELETE dbo.JobShipment;
DELETE dbo.ContainerLoadListHeader;
DELETE dbo.JobSupplierBooking;
";

		#endregion

		#region JobVoyage

		const string JobVoyageScript = @"
--JobVoyage
DELETE dbo.AsycudaPackPackedItemPivot
DELETE dbo.AsycudaBillScreening;
DELETE dbo.AsycudaTransferBill;
DELETE dbo.AsycudaTransferHeader;
DELETE dbo.AsycudaTax;
DELETE dbo.AsycudaPackedItem;
DELETE dbo.AsycudaContainerBillOrPackageLink;
DELETE dbo.AsycudaArrivalLine;
DELETE dbo.AsycudaPack;
DELETE dbo.AsycudaContainer;
DELETE dbo.AsycudaArrivalHeader;
DELETE dbo.AsycudaBill;
DELETE dbo.AsycudaManifestHeader;
DELETE dbo.ExportCustomsManifestLines;
DELETE dbo.ExportCustomsManifestHeader;
DELETE dbo.CusOutturn;
DELETE dbo.CusUnderbond;
DELETE dbo.CusOutturnHeader;
DELETE dbo.CusSeaManOBLDetail;
DELETE dbo.CusSeaManOBLHeader;
DELETE dbo.CusSeaManArrivalPort;
DELETE dbo.SupplierBookingLine;
DELETE dbo.SupplierBookingHeader;
DELETE dbo.JobConsolTransport;
DELETE dbo.JobConShipLink;
DELETE dbo.JobMawb;
DELETE dbo.JobConsolAWBSpecialHandling;
DELETE dbo.JobConsolDGRestrictions;
DELETE dbo.JobConsol;
DELETE dbo.ELoadList;
DELETE dbo.JobShipmentGateway;
DELETE dbo.JobShipmentPortMessaging;
DELETE dbo.JobShipment;
DELETE dbo.JobSlotAllocation;
DELETE dbo.JobSlotAllocationAspect;
DELETE dbo.RatingContractAllocationLine;
DELETE dbo.JobSailing;
DELETE dbo.JobVoyDestination;
DELETE dbo.JobVoyOrigin;
DELETE dbo.JobVoyCountry;
DELETE dbo.JobVoyageExRate;
DELETE dbo.JobTradeLaneVoyage;
DELETE dbo.JobVoyage;
";

		#endregion

		#region Other Jobs

		const string OtherJobsScript = @"
--Other Jobs
DELETE dbo.JobOrderItem
DELETE dbo.JobRequiredDocumentAddInfo
DELETE dbo.JobRequiredDocument
DELETE dbo.JobDocsAndCartage";

		#endregion

		#region Warehouse

		const string WarehouseScript = @"
--Warehouse
DELETE dbo.WhsPutawayLocationCache
DELETE dbo.WhsProductParamsByWhsAndClient
DELETE dbo.WhsClientParameterByWarehouse
DELETE dbo.WhsStocktakeProductFilter
DELETE dbo.WhsStocktakeLine
DELETE dbo.WhsStocktake
DELETE dbo.WhsPackageAuditLineFailure
DELETE dbo.WhsPackageAudit
DELETE dbo.WhsSerialNumberPivot
DELETE dbo.WhsSerialNumber
DELETE dbo.WhsPickLine
DELETE dbo.WhsPickShortLine
DELETE dbo.WhsAsnLine
DELETE dbo.WhsDocketContainer
DELETE dbo.WhsBOMInventoryPivot
DELETE dbo.WhsInventoryHoldChangeLog
DELETE dbo.WhsDocketLine
DELETE dbo.WhsBondedWarehouseAttribute
DELETE dbo.WhsDocketPallet
DELETE dbo.WhsDocketReference
DELETE dbo.WhsDocketJobPivot
DELETE dbo.WhsVASOrderLine
DELETE dbo.WhsVASOrder
DELETE dbo.WhsCycleCountLocationVariance
DELETE dbo.WhsCycleCountLocation
DELETE dbo.WhsDocket
DELETE dbo.WhsLoad
DELETE dbo.WhsPick
";

		#endregion

		#region Storage Jobs

		const string StorageJobsScript = @"
--JobStorage
DELETE dbo.CYDYardStorageLines;
DELETE dbo.JobStorage";

		#endregion

		#region PrintJobs

		const string PrintJobsScript = @"
--StmPrintJob
DELETE dbo.StmPrintJobCopyRecipient
DELETE dbo.StmPrintJob
DELETE dbo.StmDeliveryGroup";

		#endregion

		#region EDIInterchange

		const string EDIInterchangeScript = @"
--EDIInterchange
DELETE dbo.CusWHSOperatorTransactionLine
DELETE dbo.CusWHSOperatorTransaction
DELETE dbo.CusWHSOperatorTransactionBatch
DELETE dbo.CusSCADepotHouse
DELETE dbo.CusSCADepotContainer
DELETE dbo.EDIMessage
DELETE dbo.EDIInterchange";

		#endregion

		#region Licence

		const string LicenceScript = @"
-- Licence
DELETE dbo.MailDBRecipients

";

		#endregion

		#region Voting, Exam and Survey system

		const string VotingExamSurveySystemScript = @"
--Voting, Exam and Survey system
DELETE dbo.VoteExamSurveyAnswer
DELETE dbo.VoteExamSurveyQuestion";

		#endregion

		#region Non-Template Workflow Tasks (ProcessTasks)

		const string NonTemplateWorkflowTasksScript = @"
--Non-Template Workflow Tasks (ProcessTasks)
DECLARE @NonTemplateTasks TABLE (TaskPk uniqueidentifier)
INSERT @NonTemplateTasks SELECT P9_PK FROM dbo.ProcessTasks
    WHERE (P9_ParentID is null OR P9_ParentID not in (SELECT P0_PK FROM dbo.ProcessTaskTemplate))

DECLARE @IterationLinksToDelete TABLE (IterationPk uniqueidentifier)
INSERT @IterationLinksToDelete SELECT P9I_PK FROM dbo.ProcessTaskIterationLink WHERE P9I_P9_ContainmentBarrierTask in (SELECT TaskPk FROM @NonTemplateTasks)
INSERT @IterationLinksToDelete SELECT P9I_PK FROM dbo.ProcessTaskIterationLink WHERE P9I_P9_IterationTask in (SELECT TaskPk FROM @NonTemplateTasks)

DELETE dbo.ProcessTaskExtraResource WHERE PE_P9 in (SELECT TaskPk FROM @NonTemplateTasks)
DELETE dbo.ProcessTaskNotification WHERE PQ_P9 in (SELECT TaskPk FROM @NonTemplateTasks)
DELETE dbo.ProcessTaskIterationLinkPivot WHERE P9P_P9I_Iteration in (SELECT IterationPk FROM @IterationLinksToDelete)
DELETE dbo.ProcessTaskIterationLinkPivot WHERE P9P_P9_Task in (SELECT TaskPk FROM @NonTemplateTasks)
DELETE dbo.ProcessTaskIterationLink WHERE P9I_PK in (SELECT IterationPk FROM @IterationLinksToDelete)
DELETE dbo.ProcessTasksSecure WHERE P9H_P9_Parent in (SELECT TaskPk FROM @NonTemplateTasks)
DELETE FROM dbo.ProcessTasks WHERE P9_PK in (SELECT TaskPk FROM @NonTemplateTasks)";

		#endregion

		#region Other Operation Tables

		const string OtherOperationTablesScript = @"
--GlbCompanyCampaign
DELETE dbo.ExamAttempt
DELETE dbo.ExamSetting
DELETE dbo.GlbCompanyCampaignDripMarketing
DELETE dbo.GlbCompanyCampaignSendSettings
DELETE dbo.GlbCompanyCampaignSenderPoolItem
DELETE dbo.GlbCompanyCampaignSubscription
DELETE dbo.GlbCompanyCampaignItem
DELETE dbo.GlbCompanyCampaignBudgetItem
DELETE dbo.GlbCompanyCampaign";

		#endregion

		#region Booking (one time) Quotes

		const string BookingOneTimeQuotesScript = @"
-- RatingHeader
DELETE dbo.RateOneOffContainers
FROM dbo.RateOneOffContainers
INNER JOIN dbo.RateOneOffShipment ON TC_TT = TT_PK
INNER JOIN dbo.RatingHeader ON TT_TH = TH_PK 
WHERE TH_OneTimeQuote = 1

DELETE dbo.RateOneOffPackLine
FROM dbo.RateOneOffPackLine
INNER JOIN dbo.RateOneOffShipment ON TPL_TT_RateOneOffShipment = TT_PK
INNER JOIN dbo.RatingHeader ON TT_TH = TH_PK 
WHERE TH_OneTimeQuote = 1

DELETE dbo.RateOneOffShipment
FROM dbo.RateOneOffShipment
INNER JOIN dbo.RatingHeader ON TT_TH = TH_PK 
WHERE TH_OneTimeQuote = 1

DELETE dbo.RateTariffDiscount
FROM dbo.RateTariffDiscount
INNER JOIN dbo.RatingHeader ON TD_TH = TH_PK 
WHERE TH_OneTimeQuote = 1

DELETE dbo.RateAttachment
FROM dbo.RateAttachment
INNER JOIN dbo.RatingHeader ON TA_TH = TH_PK 
WHERE TH_OneTimeQuote = 1

DELETE dbo.RatingHeader
WHERE TH_OneTimeQuote = 1";

		#endregion

		#region Number Fountain

		const string NumberFountainScript = @"
-- Number Fountain (StmNums)
DECLARE @IncidentApprovalFountain varchar(256); SET @IncidentApprovalFountain = 'IncidentApprovalClientRef'
DECLARE @QuoteNumberFountain varchar(256); SET @QuoteNumberFountain = 'QuoteNumber'
DELETE dbo.StmNums WHERE SN_Name NOT IN (@IncidentApprovalFountain, @QuoteNumberFountain)
";

		#endregion

		#region Template Record

		const string TemplateRecordScript = @"
-- Template Record
DELETE dbo.StmTemplateRecord
";

		#endregion

		#region Work Items

		internal const string WorkItemsScript = @"
		-- Work Items
DELETE dbo.WorkItemRequestLink
DELETE dbo.WorkRequest
DELETE dbo.WorkItem
";

		#endregion

		#region Process Workflow Exception Items

		internal const string ProcessWorkflowExceptionScript = @"
		-- Process Workflow Exceptions
DELETE dbo.ProcessWorkflowException
";

		#endregion

		#region DashDocuments

		const string DashDocumentsScript = @"
--DashDocuments
DELETE dbo.DashDocCoordinate
DELETE dbo.DashDocumentText
DELETE dbo.DashOrgCandidateAddress
DELETE dbo.DashOrgCandidateName
DELETE dbo.DashOrgCandidate
DELETE dbo.DashCommercialInvoiceLineItem
DELETE dbo.DashCommercialInvoice
DELETE dbo.DashAPInvoiceChargeLineRef
DELETE dbo.DashAPInvoiceClusterRef
DELETE dbo.DashAPInvoiceChargeLine
DELETE dbo.DashAPInvoiceCluster
DELETE dbo.DashAPInvoiceRef
DELETE dbo.DashAPInvoice
DELETE dbo.DashDocument
DELETE dbo.DashMatchingConfig";

		#endregion

		#endregion

	}
}
