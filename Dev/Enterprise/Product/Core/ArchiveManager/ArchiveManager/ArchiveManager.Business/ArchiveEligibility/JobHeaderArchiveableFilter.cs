using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.ArchiveManager.Business.ArchiveEligibility
{
	[ThreadSafe]
	public class JobHeaderArchiveableFilter : ArchiveableFilter<JobHeader>
	{
		JobHeaderArchiveableFilter(string filterName, string filterClause)
			: base(filterName, filterClause) { }

		JobHeaderArchiveableFilter(string filterName, ZQuery filterClause)
			: base(filterName, filterClause) { }

		public static readonly JobHeaderArchiveableFilter ParentTableCodeIsOperationalJob = new(
			Res.GetString("7fd29250-02b7-4645-95d8-2a8e7a0abc3f", "Job is shipment, consol, rating header, or cartage"),
			JobIsOperationalQuery);

		public static readonly JobHeaderArchiveableFilter ParentTableCodeIsOperationalJobOrDeclaration = new(
			Res.GetString("62dbae67-94ac-454d-bd09-1a0cc7878788", "Job is shipment, consol, rating header, cartage, or declaration"),
			JobIsOperationalOrDeclarationQuery);

		public static readonly JobHeaderArchiveableFilter JobIsClosed = new(
			Res.GetString("81faa75d-219d-41b0-a4c2-0cbfb662f6d4", "Job is closed"),
			JobIsClosedQuery);

		public static readonly JobHeaderArchiveableFilter AccountingPeriod = new(
			Res.GetString("b5412eb1-c69a-4ba6-89ad-097e6e4116c8", "Accounting period"),
			@"NOT EXISTS
			(
				SELECT 1 FROM dbo.AccTransactionLines
				JOIN dbo.AccPeriodManagement ON AM_GC_Company = AL_GC
				AND(AL_PostDate BETWEEN AM_StartDate AND AM_EndDate)
				AND(AM_IsGeneralLedgerClosed = 0 OR AM_IsSubLedgerClosed = 0)
				WHERE AL_JH = JH_PK
			)");

		public static readonly JobHeaderArchiveableFilter NoOpenAssociatedHotCheques = new(
			Res.GetString("e8abfebd-5446-4676-9077-861c04a20a61", "No associated open hot cheques"),
			@"NOT EXISTS
			(
				SELECT 1 FROM dbo.AccHotCheque
				WHERE AQ_JH = JH_PK AND(AQ_Cancelled = 0 OR AQ_AH IS NULL)
			)");

		public static readonly JobHeaderArchiveableFilter NoAssociatedCusUSLVConsignment = new(
			Res.GetString("e525d0a4-14d9-4b91-b504-1be68f644aa6", "No associated Customs USLV Consignments"),
			@"NOT EXISTS
			(
			 SELECT 1 FROM dbo.CusUSLVConsignment
			 JOIN dbo.HVLVConsignment ON HVC_PK = ULB_HVC_Consignment
			 JOIN dbo.HVLVConsignmentHeader on HVC_HCH_Header = HCH_PK
			 JOIN dbo.JobShipment on HCH_JS_Shipment = JS_PK
			 WHERE JS_PK = JH_ParentID AND JH_ParentTableCode = 'JS'
			)");

		public static readonly JobHeaderArchiveableFilter NoAssociatedHVLVScanningSummary = new(
			Res.GetString("F3408F03-EF81-47B0-8665-C189BE2FBA96", "No associated HVLV Scanning Summary"),
			@"NOT EXISTS
			(
				SELECT 1 FROM dbo.HVLVScanningSummary
				JOIN dbo.JobShipment on HSR_JS_Shipment = JS_PK
				WHERE JS_PK = JH_ParentID and JH_ParentTableCode = 'JS'
			)");

		public static readonly JobHeaderArchiveableFilter NoAssociatedRateAttachments = new(
			Res.GetString("aa156b08-427f-414a-b863-0340c6cfc055", "No associated rate attachments"),
			@"NOT EXISTS
			(
				SELECT 1 from dbo.RateAttachment
				JOIN dbo.RatingHeader ON TH_PK = TA_TH
				WHERE TH_PK = JH_ParentId AND JH_ParentTableCode = 'TH'
			)");

		public static readonly JobHeaderArchiveableFilter NoAssociatedJobShipments = new(
			Res.GetString("404baba6-d7aa-45c6-8171-98cf115301fe", "No associated job shipments"),
			@"NOT EXISTS
			(
				SELECT 1 from dbo.RateAttachment
				JOIN dbo.RatingHeader ON TH_PK = TA_TH
				JOIN dbo.JobShipment ON JS_TH_OneTimeQuote = TH_PK
				WHERE JS_PK = JH_ParentId AND JH_ParentTableCode = 'JS'
			)");

		public static readonly JobHeaderArchiveableFilter NoAssociatedJobDeclarations = new(
			Res.GetString("32db3bca-c00c-4396-bca7-fedda25aeb11", "No associated job declarations"),
			@"NOT EXISTS
			(
				SELECT 1 FROM dbo.JobDeclaration
				WHERE JE_JS = JH_ParentID AND JH_ParentTableCode = 'JS'
			)");

		public static readonly JobHeaderArchiveableFilter NoAssociatedCusSCAHouse = new(
			Res.GetString("9830b702-f52e-48ef-b49e-112b759b5e88", "No associated Customs Sea Cargo Houses"),
			@"NOT EXISTS
			(
				SELECT 1 FROM dbo.CusSCAHouse
				WHERE CA_JS = JH_ParentID AND JH_ParentTableCode = 'JS'
			)");

		public static readonly JobHeaderArchiveableFilter NoAssociatedCusHawb = new(
			Res.GetString("fdc2edfd-08af-4f02-b9f4-633c13dced18", "No associated Customs House Airway Bills"),
			@"NOT EXISTS
			(
				SELECT 1 FROM dbo.CusHawb
				WHERE CS_JS = JH_ParentID AND JH_ParentTableCode = 'JS'
			)");

		public static readonly JobHeaderArchiveableFilter NoAssociatedCusSCADepotHouse = new(
			Res.GetString("07eaeba8-45c0-409c-9de8-6655a3faf0f4", "No associated Customs Sea Cargo Depot Houses"),
			@"NOT EXISTS
			(
				SELECT 1 FROM dbo.CusSCADepotHouse
				WHERE CX_JS = JH_ParentID AND JH_ParentTableCode = 'JS'
			)");

		public static readonly JobHeaderArchiveableFilter NoAssociatedCusSCAOceanBill = new(
			Res.GetString("baeece4f-1574-4c20-8ec9-d8aa775ba419", "No associated Customs Sea Cargo Ocean Bills"),
			@"NOT EXISTS
			(
				SELECT 1 FROM dbo.CusSCAOceanBill 
				JOIN dbo.JobConShipLink on CB_ParentTableCode = 'JK' AND CB_ParentId = JN_JK
				WHERE JN_JS = JH_ParentID AND JH_ParentTableCode = 'JS'
			)");

		public static readonly JobHeaderArchiveableFilter NoAssociatedCusCAeMHMaster_JobConShipLink = new(
			Res.GetString("f20332ef-b546-4da9-a1d9-3a6c3c21727d", "No associated Canadian Customs eManifest House Masters via attached consolidation relationships"),
			@"NOT EXISTS
			(
				SELECT 1 FROM dbo.CusCAeMHMaster 
				JOIN dbo.JobConShipLink on BP_ParentTableCode = 'JK' AND BP_ParentId = JN_JK
				WHERE JN_JS = JH_ParentID AND JH_ParentTableCode = 'JS'
			)");

		public static readonly JobHeaderArchiveableFilter NoAssociatedCusCAeMHMaster_JobConsol = new(
			Res.GetString("7cdc0111-0b18-4381-8509-6480358b97ad", "No associated Canadian Customs eManifest House Masters via attached consolidations"),
			@"NOT EXISTS
			(
				SELECT 1 FROM dbo.CusCAeMHMaster 
				JOIN dbo.JobConsol on BP_ParentTableCode = 'JK' AND BP_ParentId = JK_PK
				WHERE JK_PK = JH_ParentID AND JH_ParentTableCode = 'JK'
			)");

		public static readonly JobHeaderArchiveableFilter NoAssociatedAsycudaManifestHeader_JobConShipLink = new(
			Res.GetString("04b9694a-0e6f-4b27-a30f-c7947d935593", "No associated Asycuda Manifest Headers via attached consolidation relationships"),
			@"NOT EXISTS
			(
				SELECT 1 FROM dbo.AsycudaManifestHeader 
				JOIN dbo.JobConShipLink on AMA_ParentTableCode = 'JK' AND AMA_ParentId = JN_JK
				WHERE JN_JS = JH_ParentID AND JH_ParentTableCode = 'JS'
			)");

		public static readonly JobHeaderArchiveableFilter NoAssociatedAsycudaManifestHeader_JobConsol = new(
			Res.GetString("79377daf-b2b8-49a5-91b7-69573f835448", "No associated Asycuda Manifest Headers via attached consolidations"),
			@"NOT EXISTS
			(
				SELECT 1 FROM dbo.AsycudaManifestHeader 
				JOIN dbo.JobConsol on AMA_ParentTableCode = 'JK' AND AMA_ParentId = JK_PK
				WHERE JK_PK = JH_ParentID AND JH_ParentTableCode = 'JK'
			)");

		public static readonly JobHeaderArchiveableFilter NoAssociatedAsycudaBill = new(
			Res.GetString("7553deb6-ca21-4c25-a2c1-593ec79ebcba", "No associated Asycuda Bills"),
			@"NOT EXISTS
			(
				SELECT 1 FROM dbo.AsycudaBill
				JOIN dbo.JobShipment on ABL_JS_Shipment = JS_PK
				WHERE JS_PK = JH_ParentID AND JH_ParentTableCode = 'JS'
			)");

		public static readonly JobHeaderArchiveableFilter NoAssociatedCusDecHouseBill = new(
			Res.GetString("3830d1db-a1b1-4b88-b8e5-5d223a217d33", "No associated Customs Declaration House Bills"),
			@"NOT EXISTS
			(
				SELECT 1 FROM dbo.CusDecHouseBill
				JOIN dbo.JobShipment on CU_JS = JS_PK
				WHERE JS_PK = JH_ParentID AND JH_ParentTableCode = 'JS'
			)");

		public static readonly JobHeaderArchiveableFilter NoAssociatedCusOutturn = new(
			Res.GetString("f66f2831-9af0-4994-b24e-7b6f9ded4bd1", "No associated Customs Outturns"),
			@"NOT EXISTS
			(
				SELECT 1 FROM dbo.CusOutturn
				JOIN dbo.JobShipment on C5_ParentTableCode = 'JS' AND C5_ParentID = JS_PK
				WHERE JS_PK = JH_ParentID and JH_ParentTableCode = 'JS'
			)");

		public static readonly JobHeaderArchiveableFilter JobIsNotOpenInAnotherCompany = new(
			Res.GetString("75965f4c-a1eb-43e8-b968-05eae15e8405", "Job is not open in another company"),
			@"NOT EXISTS
			(
				SELECT 1 from dbo.JobHeader child
				WHERE child.JH_ParentId = mainArchiveableItem.JH_ParentId
				AND child.JH_PK <> mainArchiveableItem.JH_PK
				AND child.JH_GC <> mainArchiveableItem.JH_GC
				AND child.JH_Status <> 'CLS'
			)");

		public static readonly JobHeaderArchiveableFilter NoAssociatedJobConsolLinkedToJobDeclaration = new(
			Res.GetString("85965f4c-a1eb-43e8-b968-05eae15e8405", "No associated Job Consol which is linked to job declaration via job shipments"),
			@"NOT EXISTS
			(
				SELECT 1 FROM dbo.JobDeclaration
				JOIN dbo.JobShipment ON JE_JS = JS_PK
				JOIN dbo.JobConShipLink ON JN_JS = JS_PK
				JOIN dbo.JobConsol ON JN_JK = JK_PK
				WHERE JK_PK = JH_ParentID
				AND JH_ParentTableCode = 'JK'
			)");

		public static JobHeaderArchiveableFilter GetDateColumnFilter(SchemaDateTimeColumn column, ZDateTime archiveJobsOnOrBeforeThisDate)
		{
			var columnFriendlyName = DateParameterStrings.GetMultilingualName(column);
			var name = Res.GetString("ef7af896-ea5b-493f-9f13-8b60bb0979d6", "{0} on or before {1}", columnFriendlyName, archiveJobsOnOrBeforeThisDate.ToShortDateString());
			var clause = new ZQuery(column, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, archiveJobsOnOrBeforeThisDate);

			return new(name, clause);
		}

		#region Queries

		static ZQuery JobIsOperationalQuery => new ZQuery()
			.AddToFilter(JoinCondition.Or, JobHeaderSchema.JH_ParentTableCode, JobShipmentSchema.Constants.Prefix)
			.AddToFilter(JoinCondition.Or, JobHeaderSchema.JH_ParentTableCode, JobConsolSchema.Constants.Prefix)
			.AddToFilter(JoinCondition.Or, JobHeaderSchema.JH_ParentTableCode, RatingHeaderSchema.Constants.Prefix)
			.AddToFilter(JoinCondition.Or, JobHeaderSchema.JH_ParentTableCode, JobCartageSchema.Constants.Prefix);

		static ZQuery JobIsOperationalOrDeclarationQuery => JobIsOperationalQuery
			.AddToFilter(JoinCondition.Or, JobHeaderSchema.JH_ParentTableCode, JobDeclarationSchema.Constants.Prefix);

		static ZQuery JobIsClosedQuery
			=> new(JobHeaderSchema.JH_Status, JobHeaderStatus.Closed.Code);

		#endregion
	}
}
