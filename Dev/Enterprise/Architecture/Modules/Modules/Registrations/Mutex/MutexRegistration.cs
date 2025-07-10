using System;

namespace Enterprise.ZArchitecture.Modules
{
	/// <summary>
	/// Identifies and describes mutually exclusive, critical sections of code that 
	/// can only be run by one user at one time in the entire multi-user Enterprise environment.
	/// </summary>
	public static class MutexIDs
	{
		#region SuppressResourceStringsCheckRegion
#if DEBUG
		public static readonly MutexID NewDummyAttachedToShipment = new MutexID("NewDummyAttachedToShipment", "Ensures only one user can attach a new DummyBusinessObject to a Shipment, using the Dummy plug-in, at one time.");
#endif

		// Please read the list of existing MutexIDs to enusre that your critical section is not here already.
		// Also, please make your descriptions as informative as possible, to help people understand what your Mutex is for. The Mutex description must be at least 30 characters.
		public static readonly MutexID CustomsTransactionIDAllocation = new MutexID("CustomsTransactionIDAllocation", "Ensures only one user per company can create an ID for a Job at any moment of time. If one user has already created the ID, but didn't save yet, others will not be able to create an ID until he saves or cancels.");
		public static readonly MutexID JobBeingCreatedForShipment = new MutexID("JobBeingCreatedForShipment", "Ensures only one user per company can create the Job for a Shipment/Declaration/etc at any moment of time. If one user has already created the job, but didn't save yet, others will not be able to create a job until he saves or cancels.");
		public static readonly MutexID InBondBeingCreatedForJob = new MutexID("InBondBeingCreatedForShipment", "Ensures only one user per company can create an In-Bond movement for a Shipment/Declaration/etc at any moment of time. If one user has already created the job, but didn't save yet, others will not be able to create a job until he saves or cancels.");
		public static readonly MutexID BatchAggregtorRunning = new MutexID("BatchAggregtorRunning", "Ensures only one user per company can update dbo.AccGLAggregate Table. The other user will be put on hold for a specific time period. If it expires, then the other user will be notified, asking them to ty again.");
		public static readonly MutexID BatchXMLExportNumber = new MutexID("BatchXMLExportNumber", "Ensures only one user per company can update BatchNumber in AccTransactionLines Table. The other user will be put on hold for a specific time period. If it expires, then the other user will be notified, asking them to ty again.");
		public static readonly MutexID MAWBNumberAllocation = new MutexID("MAWBNumberAllocation", "Ensures the same MAWB number will not be allocated to two or more different consols.");
		public static readonly MutexID TransactionSourceReference = new MutexID("TransactionSourceReference", "Ensures the same Source Reference will not be allocated to more than one transaction in the same company.");

		public static readonly MutexID CusHAWBJobBeingCreatedForShipment = new MutexID("CusHAWBJobBeingCreatedForShipment", "Ensures only one user per company can create the Job for a Shipment/AirCargo at any moment of time. If one user has already created the job, but didn't save yet, others will not be able to create a job until he saves or cancels.");
		public static readonly MutexID CusMAWBJobBeingCreatedForConsol = new MutexID("CusMAWBJobBeingCreatedForConsol", "Ensures only one user per company can create the Job for a Consol/AirCargo at any moment of time. If one user has already created the job, but didn't save yet, others will not be able to create a job until he saves or cancels.");
		public static readonly MutexID CusSCAOceanBillJobBeingCreatedForConsol = new MutexID("CusSCAOceanBillJobBeingCreatedForConsol", "Ensures only one user per company can create the Job for a Consol/SeaCargo at any moment of time. If one user has already created the job, but didn't save yet, others will not be able to create a job until he saves or cancels.");
		public static readonly MutexID CusUSLVConsignmentForm = new MutexID("CusUSLVConsignmentForm", "Ensures the same US Customs Low Value Consignment only can be edited by one user.");
		public static readonly MutexID ECIWriteOffCreatedForShipment = new MutexID("ECIWriteOffCreatedForShipment", "For ECIWriteOff for NZ, this ensures only one user per company can create the job per shipment.");
		public static readonly MutexID ScanningForOutturn = new MutexID("ScanningForOutturn", "Ensures only one user per company can do Scan for Outturn at any moment of time.");
		public static readonly MutexID GuaranteeManagement = new MutexID(nameof(GuaranteeManagement), "Ensures the same guarantee only can be edited by one user.");
		public static readonly MutexID CusMAWBSendAIROUTMutex = new MutexID("CusMAWBSendAIROUTMutex", "Ensures only one user can send AirCargo Outturn messages at any moment for one MAWB.");

		public static readonly MutexID LandedCostingBeingCreatedForDeclarationOrOrder = new MutexID("LandedCostingBeingCreatedForDeclarationOrOrder", "Ensures only one user per company can create LC job for a declaration or an order");

		public static readonly MutexID WhsNewPick = new MutexID("WhsNewPick", "Ensures only one user per warehouse / client can create a pick. This is to ensure inventory is not committed to more than one pick.");
		public static readonly MutexID WhsEditRowSize = new MutexID("WhsEditRowSize", "Ensures only one user per warehouse can exit row sizes. This is to ensure rows / locations are rebuilt correctly.");
		public static readonly MutexID WhsPickAllocatingPackageLabels = new MutexID("WhsPickAllocatingPackageLabels", "Ensures only one user can Cartonise a Pick at a time. This is to prevent concurrency issues involving the Cartonisation process, as well as preventing modification of allocations during Cartonisation.");
		public static readonly MutexID WhsVasOrderCheckingLocationCapacity = new MutexID("WhsVasOrderCheckingLocationCapacity", "Ensures only one user can check locations capacity for specific area during initial transfer creation for VAS Orders. This is to prevent capacity overfills when many users create transfers into same area simultaniously.");
		public static readonly MutexID WhsPutawayLocationCacheUpdate = new MutexID("WhsPutawayLocationCacheUpdate", "Ensures only one user can update a warehouse's putaway location cache. This is to ensure only one process is updating the same putaway location cache at any given time.");
		public static readonly MutexID WhsInvoiceAutoRate = new MutexID("WhsInvoiceAutoRate", "Ensures only one user can auto rate an invoice at a time. This prevents another process from autorating the same invoice at any given time.");

		public static readonly MutexID AgencyAllocation = new MutexID("AgencyAllocation", "Ensures only one user can change allocations/usages on a voyage at the same time. Otherwise it may be possible to over book without so much as a warning.");
		public static readonly MutexID AgencySundry = new MutexID("AgencySundry", "Prevent multiple sundry accounts for the same bill to party coviring the same date range.");

		public static readonly MutexID WebDeploymentUpdater = new MutexID("WebDeploymentUpdater", "Ensures only a single web deployment occurs at a time.");

		public static readonly MutexID S8CargoLoginTokenRetrieval = new MutexID("S8CargoLoginTokenRetrieval", "Ensure only one user can retrieve a login token for S8 Cargo Routings.");

		public static readonly MutexID StorageDocsDBBeingCreated = new MutexID("StorageDocsDBBeingCreated", "Ensures there are no race conditions for database creation.");

		public static readonly MutexID ArchivingOffline = new MutexID("ArchivingOffline", "Ensures only one user can start an offline archiving process.");

		public static readonly MutexID AMSJobBeingCreatedForConsol = new MutexID("AMSJobBeingCreatedForConsol", "Ensures only one user per system can create the AMS Job for a Consol at any moment in time. If one user has already created the job, but didn't save yet, others will not be able to create a job until he saves or cancels.");
		public static readonly MutexID AFRJobBeingCreated = new MutexID("AFRJobBeingCreatedForConsol", "Ensures only one user per system can create the AFR Job for a parent job at any moment in time. If one user has already created the job, but didn't save yet, others will not be able to create a job until he saves or cancels.");

		public static readonly MutexID IncidentApprovalUpdate = new MutexID("IncidentApprovalUpdate", "Ensures when update arrives from eHub and web service at the same time, only one update is processed.");

		public static readonly MutexID DataProcessing = new MutexID("DataProcessing", "Ensures only single data processing occurs at a time.");

		public static readonly MutexID INManJobBeingCreated = new MutexID("INManJobBeingCreatedForConsol", "Ensures only one user per system can create the INMan Job for a parent job at any moment in time. If one user has already created the job, but didn't save yet, others will not be able to create a job until he saves or cancels.");
		public static readonly MutexID AsycudaManJobBeingCreated = new MutexID("AsycudaManJobBeingCreated", "Ensures only one user per system can create the Manifest for a parent job at any moment in time. If one user has already created the job, but didn't save yet, others will not be able to create a job until he saves or cancels.");

		public static readonly MutexID WorkshopAttendeeRegistering = new MutexID("WorkshopAttendeeRegistering", "Ensures only one workshop attendee registration can be made at any time. This allows us to assign a unique byte to the AnalyzerWorkshopTeam which is used to make the generated resources' GS_Code unique when runs are created at the same time.");

		public static readonly MutexID FTZAdmissionNumberAllocation = new MutexID("FTZAdmissionNumberAllocation", "Ensures only one user per company can create a FTZ Admission Number at any moment of time. If one user has already created the number, but didn't save yet, others will not be able to create a number until he saves or cancels.");

		public static readonly MutexID GLJournalForm = new MutexID("GLJournalForm", "Ensures the same GL Journal only can be edited by one user.");
		public static readonly MutexID COMPayDirectDebitOperation = new MutexID("COMPayDirectDebitOperation", "Ensures there is no race condition when COMPay Operations are running");

		public static readonly MutexID CUSPRLTempStorageDec = new MutexID("CUSPRLTempStorageDec", "Ensures only one CUSPRL CusTempStorageDec can be created per CusTempStorageHeader. If one user has already created the CUSPRL Dec, but not saved yet, others will not be able to create one until the current CUSPRL dec is saved or canceled.");
		public static readonly MutexID REXDISTempStorageDec = new MutexID("REXDISTempStorageDec", "Ensures only one REXDIS CusTempStorageDec can be created per CusTempStorageHeader. If one user has already created the REXDIS Dec, but not saved yet, others will not be able to create one until the current REXDIS dec is saved or canceled.");
		public static readonly MutexID CusTemporaryStorageMutex = new MutexID("CusTemporaryStorageMutex", "Ensures only one TeporaryStorage can be created per Consolidation. If one user has already created the Temporary Storage, but not saved yet, others will not be able to create one until the current Temporary Storage is saved or canceled.");
		public static readonly MutexID GroupCredentialsPlugInBeingCreated = new MutexID("GroupCredentialsPlugInBeingCreated", "Ensures that only one user per system can create or modify the Credentials data at a time. If a user has initiated changes to the Credentials data but has not yet saved them, other users will be prevented from making modifications until the current Group record is either saved or canceled.");
		public static readonly MutexID StaffCredentialsPlugInBeingCreated = new MutexID("StaffCredentialsPlugInBeingCreated", "Ensures that only one user per system can create or modify the Credentials data at a time. If a user has initiated changes to the Credentials data but has not yet saved them, other users will be prevented from making modifications until the current Staff record is either saved or canceled.");

		public static readonly MutexID TranshipmentRequest = new MutexID("TranshipmentRequest", "Ensures only one TranshipmentRequest can be created per JobDeclaration. If one user has already created the TranshipmentRequest, but not saved yet, others will not be able to create one until the current TranshipmentRequest is saved or canceled.");
		public static readonly MutexID CompanyCredentialsPlugInBeingCreated = new MutexID("CompanyCredentialsPlugInBeingCreated", "Ensures that only one user per system can create or modify the Credentials data at a time. If a user has initiated changes to the Credentials data but has not yet saved them, other users will be prevented from making modificationss until the current Company record is either saved or canceled.");
		public static readonly MutexID HMRCOAuthAuthorisation = new MutexID("HMRCOAuthAuthorisation", "Ensures only one user per system can require a client OAuth Authorisation from HMRC MTD service or refresh an access token from the previously granted authorisation.");
		public static readonly MutexID MTDVATSubmission = new MutexID("MTDVATSubmission", "Ensures only one user per system can submit VAT return to HMRC via MTD Service.");

		public static readonly MutexID ComplianceReportGeneratingXml = new MutexID(nameof(ComplianceReportGeneratingXml), "Ensures only one user per company can generate XML for Accounting Compliance Reports. This only applies while generating XML, not generating transactions for the report. If one user is already generating XML, others must wait and retry after they have completed. Currently, this only applies to the 'EST - Esterometro' compliance report.");

		public static readonly MutexID SendCustomsMessage = new MutexID("SendCustomsMessage", "Ensures only one user can send customs message at any moment in time. If one user has been generating the message, but not sent it yet, others will not be able to send customs message.");

		public static readonly MutexID CusExitControlHeaderMutex = new MutexID("CusExitControlHeaderMutex", "Ensures only one ExitHeader can be created per JobDeclaration or Shipment. If one user has already created the Exit Header, but not saved yet, others will not be able to create one until the current Exit Header is saved or canceled.");

		public static readonly MutexID CusPackingListMutex = new MutexID("CusPackingListMutex", "Ensures only one Packing List can be created per JobDeclaration. If one user has already created the Packing List, but not saved yet, others will not be able to create one until the current Packing List is saved or canceled.");

		public static readonly MutexID GoodsLocationManagement = new MutexID("GoodsLocationManagemen", "Ensures only one Goods Location can be created per CusEntryInstruction. If one user has already created the Goods Location, but not saved yet, others will not be able to create one until the current Goods Location is saved or canceled.");

		public static readonly MutexID CustomsWarehouseAllocationForCompany = new MutexID("CustomsWarehouseAllocationForCompany", "Acquire a lock (semaphore) for anyone who runs any of the three warehouse operational actions.  The idea is to allow only one process to run at a time (per company).");

		public static readonly MutexID ColombiaDocumentId = new MutexID(nameof(ColombiaDocumentId), "Ensures each Colombia DocumentId can be used only once.");

		public static readonly MutexID CusTempStorageRegHeaderMutex = new ("CusTempStorageRegHeaderMutex", "Ensures only one CusTempStorageRegHeader can be created per NctsHeader. If one user has already created the CusTempStorageRegHeader, but not saved yet, others will not be able to create one until the current CusTempStorageRegHeader is saved or canceled.");

		public static readonly MutexID CSRNumberAllocation = new MutexID("CSRNumberAllocation", "Ensures only one CarrierShipperReference can be created per consol. If one user has already created the CarrierShipperReference, but not saved yet, others will not be able to create one until the current CarrierShipperReference is saved.");

		#endregion
	}

	#region Implementation

	[WTG.StaticAnalysis.Annotation.Immutable]
	public class MutexID
	{
		public MutexID(string name, string reasonForMutex_AtLeast30Characters)
		{
			if (string.IsNullOrEmpty(name))
			{
				throw new ArgumentNullException(nameof(name), "Mutex name is null or empty");
			}
			else if (reasonForMutex_AtLeast30Characters == null)
			{
				throw new ArgumentNullException(nameof(reasonForMutex_AtLeast30Characters));
			}
			else if (reasonForMutex_AtLeast30Characters.Length < 30)
			{
#if DEBUG
				throw new ArgumentException("ReasonForMutex must be at least 30 characters.", nameof(reasonForMutex_AtLeast30Characters));
#endif
			}

			this.Name = name;
			this.Reason = reasonForMutex_AtLeast30Characters;
		}

		public readonly string Name;
		public readonly string Reason;
	}

	#endregion
}
