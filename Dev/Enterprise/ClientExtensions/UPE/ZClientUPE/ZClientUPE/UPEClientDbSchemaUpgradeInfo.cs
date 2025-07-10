using System.Collections.Immutable;
using CargoWise.Database.Abstractions.Extensions;
using CargoWise.Database.Shared;
using CargoWise.Types;
using Enterprise.Client.UPE.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.UPE
{
	public class UPEClientDbSchemaUpgradeInfo : IExtensionObjects
	{
		ImmutableArray<DatabaseObjectCreateScript> IExtensionObjects.TableCreationScripts => TableCreationScripts;
		ImmutableArray<DatabaseViewAndRoutineCreateScript> IExtensionObjects.ViewAndRoutineCreationScripts => ViewAndRoutinesCreationScripts;

		#region TableCreationScripts

		static ImmutableArray<DatabaseObjectCreateScript> TableCreationScripts => ImmutableArray.Create(
			// TABLES
			ClientPWSHeader,
			ClientPWSCharge,
			ClientBISIShipmentHeader,
			ClientBISIShipmentCharge,
			ClientPrintBatch,
			ClientPrintBatchItem,
			ClientOrgRematch,
			ClientADPScoring,
			ClientRefund,
			ClientXPLDUploadLog,

			// INDEXES
			Index_ProcessQueue_P4_CustomAttrib2,
			Index_ProcessQueue_P4_CustomsStatus,
			Index_ProcessQueue_P4_CustomDecimal4,
			Index_ProcessQueue_P4_CustomDate4
		);

		#region TABLE ClientPWSHeader

		static DatabaseObjectCreateScript ClientPWSHeader
		{
			get
			{
				return new DatabaseObjectCreateScript("ClientPWSHeader", @"
CREATE TABLE ClientPWSHeader
(
	U1_PK uniqueidentifier NOT NULL CONSTRAINT DF_U1_PK DEFAULT (newid()),
	U1_WayBillNumber varchar (35) NOT NULL,
	U1_WayBillShortNumber varchar (11),
	U1_InvoiceNumber varchar (12),
	U1_BillToAccount varchar (10),
	U1_BillableWeight decimal (9, 3) NOT NULL,
	U1_ImportedDate datetime NOT NULL

	CONSTRAINT PK_UX__U1_PK PRIMARY KEY  NONCLUSTERED ( U1_PK )
)
CREATE UNIQUE INDEX NR_UX__U1_WayBillNumber ON ClientPWSHeader (U1_WayBillNumber)
CREATE UNIQUE INDEX NR_UX__U1_WayBillShortNumber ON ClientPWSHeader (U1_WayBillShortNumber)
", "DROP TABLE ClientPWSHeader");
			}
		}

		#endregion

		#region TABLE ClientPWSCharge

		static DatabaseObjectCreateScript ClientPWSCharge
		{
			get
			{
				return new DatabaseObjectCreateScript("ClientPWSCharge", @"
CREATE TABLE ClientPWSCharge
(
	U2_PK uniqueidentifier NOT NULL CONSTRAINT DF_U2_PK DEFAULT (newid()),
	U2_U1 uniqueidentifier NOT NULL,
	U2_ChargeDescription varchar (20) NOT NULL,
	U2_TaxableAmount money NOT NULL,
	U2_NonTaxableAmount money NOT NULL,
	U2_Discount money NOT NULL,
	U2_NetAmount money NOT NULL

	CONSTRAINT PK_UX__U2_PK PRIMARY KEY  NONCLUSTERED 
	( U2_PK )

	CONSTRAINT ClientPWSCharge_U2_U1_FK2_ClientPWSHeader_RRR_121 FOREIGN KEY 
	( U2_U1 ) REFERENCES ClientPWSHeader ( U1_PK )
);
CREATE NONCLUSTERED INDEX [FK_RX__U2_U1] ON [ClientPWSCharge] ([U2_U1] ASC);
", "DROP TABLE ClientPWSCharge");
			}
		}

		#endregion

		#region TABLE ClientBISIShipmentHeader

		static DatabaseObjectCreateScript ClientBISIShipmentHeader
		{
			get
			{
				return new DatabaseObjectCreateScript("ClientBISIShipmentHeader", @"
CREATE TABLE ClientBISIShipmentHeader
(
	T8_PK uniqueidentifier NOT NULL CONSTRAINT DF_T8_PK DEFAULT (newid()),
	T8_CS uniqueidentifier NOT NULL,
	T8_UploadBatchNumber int NOT NULL,
	T8_ThirdPartyIndicator varchar(1) NOT NULL DEFAULT(''),

	CONSTRAINT PK_UX__T8_PK PRIMARY KEY  NONCLUSTERED 
	( T8_PK )
)
CREATE UNIQUE INDEX NR_UX__T8_CS ON ClientBISIShipmentHeader (T8_CS)
",
					"DROP TABLE ClientBISIShipmentHeader");
			}
		}

		#endregion

		#region TABLE ClientBISIShipmentCharge

		static DatabaseObjectCreateScript ClientBISIShipmentCharge
		{
			get
			{
				return new DatabaseObjectCreateScript("ClientBISIShipmentCharge", @"
CREATE TABLE ClientBISIShipmentCharge
(
	T9_PK uniqueidentifier NOT NULL CONSTRAINT DF_T9_PK DEFAULT (newid()),
	T9_T8 uniqueidentifier NOT NULL,
	T9_ChargeType varchar (4) NOT NULL,
	T9_GrossAmount money NOT NULL

	CONSTRAINT PK_UX__T9_PK PRIMARY KEY  NONCLUSTERED 
	( T9_PK )

	CONSTRAINT ClientBISIShipmentCharge_T9_T8_FK2_ClientBISIShipmentHeader_RRR_121 FOREIGN KEY 
	( T9_T8 ) REFERENCES ClientBISIShipmentHeader ( T8_PK )
);
CREATE NONCLUSTERED INDEX [FK_RX__T9_T8] ON [ClientBISIShipmentCharge] ([T9_T8] ASC);
",
					"DROP TABLE ClientBISIShipmentCharge");
			}
		}

		#endregion

		#region TABLE ClientPrintBatch

		static DatabaseObjectCreateScript ClientPrintBatch
		{
			get
			{
				return new DatabaseObjectCreateScript("ClientPrintBatch", @"
CREATE TABLE ClientPrintBatch
(
	T7_PK uniqueidentifier NOT NULL CONSTRAINT DF_T7_PK DEFAULT (newid()),
	T7_BatchType varchar(3) NOT NULL,
	T7_BatchNumber int NOT NULL DEFAULT 0,
	T7_LastPrintedDate smalldatetime NULL,
	T7_PrintCount int NOT NULL DEFAULT 0,

	CONSTRAINT PK_UX__T7_PK PRIMARY KEY  NONCLUSTERED 
	( T7_PK )
)
CREATE UNIQUE INDEX NR_UX__T7_BatchType_T7_BatchNumber ON ClientPrintBatch (T7_BatchType, T7_BatchNumber)
",
					"DROP TABLE ClientPrintBatch");
			}
		}

		#endregion

		#region TABLE ClientPrintBatchItem

		static DatabaseObjectCreateScript ClientPrintBatchItem
		{
			get
			{
				return new DatabaseObjectCreateScript("ClientPrintBatchItem", @"
CREATE TABLE ClientPrintBatchItem
(
	T6_PK uniqueidentifier NOT NULL CONSTRAINT DF_T6_PK DEFAULT (newid()),
	T6_T7 uniqueidentifier NOT NULL,
	T6_ParentID uniqueidentifier NOT NULL,
	T6_GS_QueuedBy uniqueidentifier NOT NULL,
	T6_SU uniqueidentifier NOT NULL,

	CONSTRAINT PK_UX__T6_PK PRIMARY KEY  NONCLUSTERED 
	( T6_PK ),

	CONSTRAINT ClientPrintBatchItem_T6_T7_FK2_ClientPrintBatch_RRR_121 FOREIGN KEY 
	( T6_T7 ) REFERENCES ClientPrintBatch ( T7_PK ),

	CONSTRAINT ClientPrintBatchItem_T6_GS_QueuedBy_FK2_GlbStaff_RRR_121 FOREIGN KEY 
	( T6_GS_QueuedBy ) REFERENCES GlbStaff ( GS_PK ),

	CONSTRAINT ClientPrintBatchItem_T6_SU_FK2_StmMenuItem_RRR_121 FOREIGN KEY 
	( T6_SU ) REFERENCES StmMenuItem ( SU_PK )
)
CREATE NONCLUSTERED INDEX [FK_RX__T6_T7] ON [ClientPrintBatchItem] ([T6_T7] ASC);
CREATE NONCLUSTERED INDEX [FK_RX__T6_SU] ON [ClientPrintBatchItem] ([T6_SU] ASC);
CREATE NONCLUSTERED INDEX [FK_RX__T6_ParentID] ON [ClientPrintBatchItem] ([T6_ParentID] ASC);
",
					"DROP TABLE ClientPrintBatchItem");
			}
		}

		#endregion

		#region TABLE ClientOrgRematch

		static DatabaseObjectCreateScript ClientOrgRematch
		{
			get
			{
				return new DatabaseObjectCreateScript("ClientOrgRematch", @"
CREATE TABLE ClientOrgRematch
(
	T5_PK uniqueidentifier NOT NULL CONSTRAINT DF_T5_PK DEFAULT (newid()),
	T5_JE uniqueidentifier NOT NULL,
	T5_OrganisationType varchar(3) NOT NULL,
	T5_GS_RematchedBy uniqueidentifier NOT NULL,
	T5_OH_RematchedFromOrg uniqueidentifier NULL,
	T5_OH_RematchedToOrg uniqueidentifier NULL,
	T5_RematchedFromDate smalldatetime NULL,
	T5_RematchedToDate smalldatetime NULL,

	CONSTRAINT PK_UX__T5_PK PRIMARY KEY  NONCLUSTERED 
	( T5_PK ),

	CONSTRAINT ClientOrgRematch_T5_GS_RematchedBy_FK2_GlbStaff_RRR_121 FOREIGN KEY 
	( T5_GS_RematchedBy ) REFERENCES GlbStaff ( GS_PK ),

	CONSTRAINT ClientOrgRematch_T5_OH_RematchedFromOrg_FK2_OrgHeader_RRR_121 FOREIGN KEY 
	( T5_OH_RematchedFromOrg ) REFERENCES OrgHeader ( OH_PK ),

	CONSTRAINT ClientOrgRematch_T5_OH_RematchedToOrg_FK2_OrgHeader_RRR_121 FOREIGN KEY 
	( T5_OH_RematchedToOrg ) REFERENCES OrgHeader ( OH_PK )
);
CREATE NONCLUSTERED INDEX [FK_RX__T5_GS_RematchedBy] ON [ClientOrgRematch] ([T5_GS_RematchedBy] ASC);
CREATE NONCLUSTERED INDEX [FK_RX__T5_OH_RematchedFromOrg] ON [ClientOrgRematch] ([T5_OH_RematchedFromOrg] ASC);
CREATE NONCLUSTERED INDEX [FK_RX__T5_OH_RematchedToOrg] ON [ClientOrgRematch] ([T5_OH_RematchedToOrg] ASC);
",
					"DROP TABLE ClientOrgRematch");
			}
		}

		#endregion

		#region TABLE ClientADPScoring

		static DatabaseObjectCreateScript ClientADPScoring
		{
			get
			{
				return new DatabaseObjectCreateScript("ClientADPScoring", @"
CREATE TABLE ClientADPScoring
(
	T4_PK uniqueidentifier NOT NULL CONSTRAINT DF_T4_PK DEFAULT (newid()),
	T4_GS_NKUser varchar(3) NOT NULL,
	T4_Date smalldatetime NOT NULL,
	T4_IsStaffWorkingDay char(1) NOT NULL DEFAULT 'Y',
	T4_SubmissionCount int NOT NULL DEFAULT (0),
	T4_SubmissionErrorCount int NOT NULL DEFAULT (0),
	T4_RegularVolumeCount int NOT NULL DEFAULT (0),

	CONSTRAINT PK_UX__T4_PK PRIMARY KEY  NONCLUSTERED 
	( T4_PK )
)
",
					"DROP TABLE ClientADPScoring");
			}
		}

		#endregion

		#region TABLE ClientRefund

		static DatabaseObjectCreateScript ClientRefund
		{
			get
			{
				return new DatabaseObjectCreateScript("ClientRefund", @"
CREATE TABLE ClientRefund
(
	T10_PK uniqueidentifier NOT NULL CONSTRAINT DF_T10_PK DEFAULT (newid()),
	T10_JE uniqueidentifier,
	T10_CS uniqueidentifier,
	T10_ControlNumber varchar(11) NOT NULL,
	T10_GS_NKCreatedUser varchar(3) NOT NULL,
	T10_GS_NKAtFaultUser varchar(3),
	T10_IsRefundRejected char(1) NOT NULL DEFAULT('N'),
	T10_RefundReason varchar(50),
	T10_RefundRejectedDetails varchar(1000),
	T10_DateCreated smalldatetime,
	T10_DateProcessed smalldatetime,
	T10_EnquiryContact varchar(256) NOT NULL,
	T10_EnquiryDetails varchar(1000) NOT NULL,
	T10_EnquiryPhoneNumber varchar(100) NOT NULL,
	T10_EnquiryRaisedBy varchar(20) NOT NULL,
	T10_RefundAmount money NOT NULL DEFAULT (0),
	T10_RefundProcessingFee money NOT NULL DEFAULT (0),
	T10_AdditionalCharges money NOT NULL DEFAULT (0),	
	T10_AmountRefundedToUPS money NOT NULL DEFAULT (0),	
	T10_WriteOffAmount	money NOT NULL DEFAULT (0),	
	CONSTRAINT PK_UX__T10_PK PRIMARY KEY  NONCLUSTERED 
	( T10_PK )
)
",
					"DROP TABLE ClientRefund");
			}
		}

		#endregion

		#region TABLE ClientXPLDUploadLog

		static DatabaseObjectCreateScript ClientXPLDUploadLog
		{
			get
			{
				return new DatabaseObjectCreateScript("ClientXPLDUploadLog", @"
CREATE TABLE ClientXPLDUploadLog
(
	U3_PK uniqueidentifier NOT NULL CONSTRAINT DF_U3_PK DEFAULT (newid()),
	U3_ReasonCode varchar (100) NOT NULL,
	U3_DateCreated datetime,
	U3_BISIData varchar(1000) NOT NULL,
	U3_TrackingNumber varchar(100) NOT NULL,

	CONSTRAINT PK_UX__U3_PK PRIMARY KEY  NONCLUSTERED ( U3_PK )
)

CREATE NONCLUSTERED INDEX NR_UX__U3_TrackingNumber ON ClientXPLDUploadLog (U3_TrackingNumber)
",
					"DROP TABLE ClientXPLDUploadLog");
			}
		}

		#endregion

		#region Client Specific Indexes

		static DatabaseObjectCreateScript Index_ProcessQueue_P4_CustomAttrib2
		{
			get
			{
				return new DatabaseObjectCreateScript(
					"NR_RX__P4_CustomAttrib2",
					"CREATE NONCLUSTERED INDEX NR_RX__P4_CustomAttrib2 ON ProcessQueue (P4_CustomAttrib2)",
					"DROP INDEX ProcessQueue.NR_RX__P4_CustomAttrib2");
			}
		}

		static DatabaseObjectCreateScript Index_ProcessQueue_P4_CustomsStatus
		{
			get
			{
				return new DatabaseObjectCreateScript(
					"NR_RX__P4_CustomsStatus",
					"CREATE NONCLUSTERED INDEX NR_RX__P4_CustomsStatus ON ProcessQueue (P4_CustomsStatus)",
					"DROP INDEX ProcessQueue.NR_RX__P4_CustomsStatus");
			}
		}

		static DatabaseObjectCreateScript Index_ProcessQueue_P4_CustomDecimal4
		{
			get
			{
				return new DatabaseObjectCreateScript(
					"NR_RX__P4_CustomDecimal4",
					"CREATE NONCLUSTERED INDEX NR_RX__P4_CustomDecimal4 ON ProcessQueue (P4_CustomDecimal4)",
					"DROP INDEX ProcessQueue.NR_RX__P4_CustomDecimal4");
			}
		}

		static DatabaseObjectCreateScript Index_ProcessQueue_P4_CustomDate4
		{
			get
			{
				return new DatabaseObjectCreateScript(
					"NR_RX__P4_CustomDate4",
					"CREATE NONCLUSTERED INDEX NR_RX__P4_CustomDate4 ON ProcessQueue (P4_CustomDate4)",
					"DROP INDEX ProcessQueue.NR_RX__P4_CustomDate4");
			}
		}

		#endregion

		#endregion

		#region ViewAndRoutinesCreationScripts

		static ImmutableArray<DatabaseViewAndRoutineCreateScript> ViewAndRoutinesCreationScripts => ImmutableArray.Create(
			// FUNCTIONS
			ClientIsBranchUPECustomised,
			ClientUPE_VW_CusHAWB_JobDec,
			ClientGetPaymentMethodDescription,
			ClientGetBillingTermsDescription,
			ClientGetReasonCodeDescription,
			ClientGetCusHAWBConsigneeName,
			ClientFinanceReleaseReport,
			ClientCustomsJobQueuedReport,
			ClientCommercialJobQueuedReport,
			ClientBISIInterchangeReport,
			ClientBISIUploadWarningReport,
			ClientDeclarationHasValidReasonCode,
			ClientBrokerageExceptionsReport,
			ClientCMRMissingSupplierImporterCodesReport,
			ClientShipmentLodgedReport,
			ClientOrgRematchReport,
			ClientGetDatePartOnly,
			ClientMinDate,
			ClientMaxDate,
			ClientRefundReport,
			ClientADPScoringReport,
			ClientUPEProductivityReport,
			ClientUPEEIRReport,
			ClientDetailedUPEEIRReport,
			ClientGetRemarksFromQueueLogReference,
			ClientUPEInterventionReport,
			ClientUPEBPWReportOLD,
			ClientUPEBPWReport,
			ClientGetLevel1FileNames,
			ClientDataManagementLogReport,
			ClientShortlandsThatHaveNeverArrivedReport,
			ClientUPEGetScriptForMastersWithNoOuturnDocs,
			ClientUPEREVReport,
			Client_UPE_GetRegistryItem,
			ClientOrgJobNumbers,
			Client_UPE_CheckPostcodeIsInZoneName,
			Client_UPE_CODManifestReport,
			// STORED PROCEDURES
			ClientMissingOutturnReport,

			// VIEWS
			vw_Report_ClientAirwaybillStatusReport
		);

		#region FUNCTION ClientGetPaymentMethodDescription

		static DatabaseViewAndRoutineCreateScript ClientGetPaymentMethodDescription
		{
			get
			{
				return new DatabaseViewAndRoutineCreateScript("ClientGetPaymentMethodDescription", @"
CREATE FUNCTION ClientGetPaymentMethodDescription(@PaymentMethod as decimal)
RETURNS varchar(50)
AS
BEGIN

DECLARE @Result varchar(50)
SET @Result = ''

SET @Result =
(
	CASE @PaymentMethod
		WHEN " + (int)UPECargoPaymentMethod.None + @" THEN 'None'
		WHEN " + (int)UPECargoPaymentMethod.Account + @" THEN 'Account'
		WHEN " + (int)UPECargoPaymentMethod.BPay + @" THEN 'BPay' 
		WHEN " + (int)UPECargoPaymentMethod.CreditCard + @" THEN 'Credit Card'
		WHEN " + (int)UPECargoPaymentMethod.Cheque + @" THEN 'Cheque'
		WHEN " + (int)UPECargoPaymentMethod.EFT + @" THEN 'EFT'
		WHEN " + (int)UPECargoPaymentMethod.Nett7Day + @" THEN 'Nett 7 Day'
		WHEN " + (int)UPECargoPaymentMethod.Other + @" THEN 'Other'
		WHEN " + (int)UPECargoPaymentMethod.PurchaseOrder + @" THEN 'Purchase Order'
	END
)

RETURN @Result

END
",
					"DROP FUNCTION ClientGetPaymentMethodDescription",
					DbRoutineType.SqlFunctionScalarTypeDesc);
			}
		}

		#endregion

		#region FUNCTION ClientGetBillingTermsDescription

		static DatabaseViewAndRoutineCreateScript ClientGetBillingTermsDescription
		{
			get
			{
				string createSql = @"
CREATE FUNCTION ClientGetBillingTermsDescription(@Code as varchar(3))
Returns varchar(200)
AS
BEGIN
Declare @Description varchar(200)

Set @Description =

CASE RTRIM(@Code)
";
				foreach (CodeDescriptionPair pair in new BillingTermsCodeDescriptionPairList())
				{
					createSql += string.Format("WHEN '{0}' THEN '{1}'\r\n", pair.Code, pair.Description);
				}
				createSql += @"
ELSE ''
END

RETURN @Description
END
";
				return new DatabaseViewAndRoutineCreateScript("ClientGetBillingTermsDescription", createSql, "DROP FUNCTION ClientGetBillingTermsDescription", DbRoutineType.SqlFunctionScalarTypeDesc);
			}
		}

		#endregion

		#region FUNCTION ClientGetReasonCodeDescription

		static DatabaseViewAndRoutineCreateScript ClientGetReasonCodeDescription
		{
			get
			{
				string createSql = @"
CREATE FUNCTION ClientGetReasonCodeDescription(@Code as varchar(3))
Returns varchar(200)
AS
BEGIN
Declare @Description varchar(200)

Set @Description =

CASE RTRIM(@Code)
";
				foreach (CodeDescriptionPair pair in new ReasonCodeDescriptionPairList())
				{
					createSql += string.Format("WHEN '{0}' THEN '{1}'\r\n", pair.Code.Substring(pair.Code.Length - 2), pair.Description);
				}
				createSql += @"
ELSE ''
END

RETURN @Description
END
";
				return new DatabaseViewAndRoutineCreateScript("ClientGetReasonCodeDescription", createSql, "DROP FUNCTION ClientGetReasonCodeDescription", DbRoutineType.SqlFunctionScalarTypeDesc);
			}
		}

		#endregion

		#region FUNCTION ClientGetCusHAWBConsigneeName

		static DatabaseViewAndRoutineCreateScript ClientGetCusHAWBConsigneeName
		{
			get
			{
				return new DatabaseViewAndRoutineCreateScript("ClientGetCusHAWBConsigneeName", @"
CREATE FUNCTION ClientGetCusHAWBConsigneeName(@CS_PK uniqueidentifier)
RETURNS varchar(30)
AS
BEGIN

DECLARE @Result varchar(30)
SET @Result =
(
	SELECT	isnull(OH_FullName, CS_ConsigneeName)
	FROM 	dbo.CusHAWB
	LEFT OUTER JOIN dbo.OrgAddress ON CS_OA_ConsigneeAddress=OA_PK
	LEFT OUTER JOIN dbo.OrgHeader ON OA_OH=OH_PK
	WHERE CS_PK=@CS_PK
)
RETURN @Result

END
",
					"DROP FUNCTION ClientGetCusHAWBConsigneeName",
					DbRoutineType.SqlFunctionScalarTypeDesc);
			}
		}

		#endregion

		#region WIP

		#region VIEW vw_Report_ClientJobStatus

		//		static DatabaseViewAndRoutineCreateScript ClientJobStatusReport
		//		{
		//			get
		//			{
		//				return new DatabaseViewAndRoutineCreateScript(@"
		//CREATE VIEW vw_Report_ClientJobStatus
		//AS
		//SELECT		Declaration.JE_PK AS JobPK, 
		//			Declaration.JE_DeclarationReference AS Job, 
		//			Declaration.JE_HouseBill AS HAWB, 
		//            Declaration.JE_MasterBill AS MAWB, 
		//			Declaration.JE_RL_NKOrigin AS Origin, 
		//			Declaration.JE_DateOfArrival AS Arrival, 
		//			Declaration.JE_SystemCreateTimeUtc AS Registered, 
		//			Declaration.JE_OH_Importer AS ImporterPK, 
		//            Declaration.JE_RL_NKPortOfArrival AS Discharge, 
		//			(CASE
		//				WHEN HAWB.CS_IsResponsePending=1 THEN 'WAIT'
		//				ELSE HAWB.CS_CustomsStatus
		//			END) AS ACAStatus,
		//            HAWB.CS_RS_NK_ServiceLevel AS ServiceLevel, 
		//			(CASE WHEN HAWB.CS_OH_Consignee IS NULL THEN HAWB.CS_ConsigneePostcode 
		//				ELSE ImporterOfficeAddress.OA_Postcode END) AS PostCode, 
		//			(CASE WHEN Declaration.JE_TotalWeightUnit = 'KG' THEN Declaration.JE_TotalWeight 
		//				ELSE (SELECT Value FROM dbo.ConvertWeight(Declaration.JE_TotalWeight, Declaration.JE_TotalWeightUnit, 'KG')) END) AS Weight, 
		//			Declaration.JE_TotalWeightUnit AS WeightUQ, 
		//            Declaration.JE_TotalNoOfPieces AS Pieces, 
		//			ProcessQueue.P4_CustomsStatus AS QueueStatus, 
		//            ProcessQueue.P4_CustomsReason AS QueueDetails, 
		//			Agent.GS_Code AS Agent
		//FROM        dbo.JobDeclaration Declaration
		//			LEFT OUTER JOIN dbo.CusHAWB HAWB ON Declaration.JE_PK = HAWB.CS_JE_CustomsFormalEntry AND Declaration.JE_IsCancelled = 0
		//			LEFT OUTER JOIN dbo.ProcessQueue ON ProcessQueue.P4_ParentID = Declaration.JE_PK
		//            LEFT OUTER JOIN dbo.OrgHeader CusDecImporter ON CusDecImporter.OH_PK = Declaration.JE_OH_Importer
		//			LEFT OUTER JOIN dbo.OrgAddress ImporterOfficeAddress ON CusDecImporter.OH_PK = ImporterOfficeAddress.OA_OH AND ImporterOfficeAddress.OA_AddressType = 'OFC'
		//			LEFT OUTER JOIN dbo.GlbStaff Agent ON Agent.GS_Code = ProcessQueue.P4_GS_NKCustomsTaskAssignedTo
		//",
		//					"DROP VIEW vw_Report_ClientJobStatus");
		//			}
		//		}

		#endregion

		#region VIEW vw_Report_ClientUnworkedJobs
		//
		//		static DatabaseViewAndRoutineCreateScript ClientUnworkedJobsReport
		//		{
		//			get
		//			{
		//				return new DatabaseViewAndRoutineCreateScript(@"
		//
		//CREATE VIEW vw_Report_ClientUnworkedJobs
		//AS
		//SELECT  JobDeclaration.JE_DeclarationReference AS Job, 
		//		JobDeclaration.JE_RL_NKPortOfArrival AS Discharge, 
		//		JobDeclaration.JE_HouseBill AS HAWB, 
		//		JobDeclaration.JE_RL_NKOrigin AS Origin, 
		//		JobDeclaration.JE_TotalNoOfPieces AS Pieces, 
		//		(CASE WHEN JobDeclaration.JE_TotalWeightUnit = 'KG' THEN JobDeclaration.JE_TotalWeight 
		//			ELSE (SELECT Value FROM dbo.ConvertWeight(JobDeclaration.JE_TotalWeight, JobDeclaration.JE_TotalWeightUnit, 'KG')) END) AS Weight, 
		//		JobDeclaration.JE_TotalWeightUnit AS WeightUQ, 
		//		(CASE WHEN CusHAWB.CS_OH_Consignee IS NULL THEN CusHAWB.CS_ConsigneePostcode 
		//			ELSE OrgAddress.OA_Postcode END) AS PostCode, 
		//		(CASE
		//            WHEN CusHAWB.CS_IsResponsePending=1 THEN 'WAIT'
		//            ELSE CusHAWB.CS_CustomsStatus
		//         END) AS ACAStatus,
		//		JobDeclaration.JE_SystemCreateTimeUtc AS Registered, 
		//		CusHAWB.CS_RS_NK_ServiceLevel AS SLevel, 
		//		StmALog.SL_PostedTimeUtc AS HeldOn, 
		//		ProcessQueue.P4_CustomsQueue AS QueueName, 
		//		ProcessQueue.P4_CustomsStatus AS QueueStatus, 
		//		ProcessQueue.P4_CustomsSubStatus AS QueueSubStatus, 
		//		ProcessQueue.P4_CustomsReason AS QueueDetails, 
		//		ProcessQueue.P4_GS_NKCustomsTaskAssignedTo AS AssignedTo
		//
		//FROM    dbo.CusHAWB
		//		LEFT OUTER JOIN dbo.JobDeclaration ON JobDeclaration.JE_PK = CusHAWB.CS_JE_CustomsFormalEntry AND JE_IsCancelled = 0
		//		LEFT OUTER JOIN dbo.OrgHeader CusDecImporter ON CusDecImporter.OH_PK = JobDeclaration.JE_OH_Importer 
		//		LEFT OUTER JOIN dbo.OrgAddress ON CusDecImporter.OH_PK = OrgAddress.OA_OH AND OrgAddress.OA_AddressType = 'OFC' 
		//		LEFT OUTER JOIN dbo.StmALog ON JobDeclaration.JE_PK = StmALog.SL_Parent 
		//		LEFT OUTER JOIN dbo.ProcessQueue ON JobDeclaration.JE_PK = ProcessQueue.P4_ParentID  
		//WHERE	SL_PostedTimeUtc = (SELECT MAX(SL_PostedTimeUtc) 
		//                         FROM dbo.StmALog
		//                         WHERE JobDeclaration.JE_PK = StmALog.SL_Parent AND
		//                               StmALog.SL_SE_NKEvent = 'EDT') AND
		//        ProcessQueue.P4_CustomsStatus <> ''
		//",
		//					"DROP VIEW vw_Report_ClientUnworkedJobs");
		//			}
		//		}			
		//
		#endregion

		#endregion

		#region FUNCTION ClientFinanceReleaseReport

		static string CommaSeparatedCompletedCommercialQueueNames
		{
			get
			{
				string result = "";
				foreach (string queueName in CommercialQueueCodeDescriptionPairList.CompletedQueueNames)
				{
					if (!string.IsNullOrEmpty(result))
					{
						result += ",";
					}
					result += "'" + queueName + "'";
				}
				return result;
			}
		}

		static string CommercialProcessQueueNotCompletedFilter
		{
			get
			{
				string result = "";
				foreach (string queueName in CommercialQueueCodeDescriptionPairList.CompletedQueueNames)
				{
					if (!string.IsNullOrEmpty(result))
					{
						result += " and ";
					}
					result += StmALogSchema.SL_Reference.Name + " not like 'COM\"" + queueName + "\"%'";
				}
				return result;
			}
		}

		static DatabaseViewAndRoutineCreateScript ClientFinanceReleaseReport
		{
			get
			{
				string result = @"
CREATE FUNCTION ClientFinanceReleaseReport(@StartDate datetime, @EndDate datetime, @Queue varchar(10))

RETURNS TABLE
AS
RETURN
(
SELECT case
		   when len(min(CS_HAWB))=11 then ''
		   else min(CS_HAWB)
	   end AS HAWB,
	   case
		   when len(min(CS_HAWB))=11 then min(CS_HAWB)
		   else min(EB_WaybillShortNumber)
	   end AS ShortHAWB,
	   min(P4_QueueName) AS QueueName,
	   min(P4_CustomAttrib1) AS InvoiceNumber,
	   dbo.ClientGetCusHAWBConsigneeName(CS_PK) AS ConsigneeName,
	   min(CS_RS_NK_ServiceLevel) AS ServiceLevel,
	   min(P4_CustomDate4) AS ReleaseDate,
	   min(CM_ArrivalDate) AS ArrivalDate,
	   min(CS_PiecesLanded) AS Pieces,
	   dbo.ClientGetPaymentMethodDescription(min(P4_CustomDecimal2)) AS ReleaseMethod,
	   sum(CASE JR_OSSellAmt
		   WHEN 0 THEN (JR_OSCostAmt - JR_LocalCostAmt) + JR_OSSellAmt * 1.1
		   ELSE        JR_OSCostAmt + (JR_OSSellAmt - JR_LocalCostAmt) * 1.1
	   END) AS CODInvoiceValue,
	   (CASE min(CompletedBy.GS_Code) WHEN '~BP' THEN 'Automatic' ELSE min(CompletedBy.GS_FullName) END) AS CompletedBy,
	   (select top 1 substring(SL_Reference, 5, 3) from dbo.StmALog where SL_Parent=P4_PK and SL_Reference like 'COM""%' and " + CommercialProcessQueueNotCompletedFilter + @" and SL_Reference not like 'COM""""%' order by SL_PostedTimeUtc desc) AS QueueNameBeforeCompleted
FROM   dbo.CusHAWB
	   LEFT OUTER JOIN dbo.CusMAWB ON CM_PK=CS_CM
	   LEFT OUTER JOIN dbo.JobRelatedWayBill RelatedWayBill ON EB_ParentID=CS_PK AND EB_WaybillType='PAR'
	   INNER JOIN dbo.ProcessQueue ON P4_ParentID=CS_PK
	   LEFT OUTER JOIN dbo.OrgAddress ON CusHAWB.CS_OA_ConsigneeAddress = OrgAddress.OA_PK
	   LEFT OUTER JOIN dbo.OrgHeader ON OrgAddress.OA_OH = OrgHeader.OH_PK 
	   LEFT OUTER JOIN dbo.JobHeader ON CS_PK=JH_ParentID
	   LEFT OUTER JOIN dbo.JobCharge ON JH_PK=JR_JH
	   LEFT OUTER JOIN dbo.GlbStaff CompletedBy ON GS_Code=P4_CustomAttrib5
	   CROSS APPLY dbo.ClientIsBranchUPECustomised(CM_GB) AS Branch
WHERE  Branch.Enabled = 1 AND
	   (
		((@StartDate = '' OR P4_CustomDate4 >= @StartDate) AND (@EndDate = '' OR P4_CustomDate4 <= dateadd(day, 1, @EndDate))) OR
		((@StartDate = '' OR P4_CustomDate5 >= @StartDate) AND (@EndDate = '' OR P4_CustomDate5 <= dateadd(day, 1, @EndDate)))
	   ) AND
	   P4_QueueName in (" + CommaSeparatedCompletedCommercialQueueNames + @") AND
	   P4_Status != '" + ReasonCodeDescriptionPairList.Codes._C1_SubsequentSplitShipment + @"' AND
	   (@Queue='ALL' OR P4_QueueName=@Queue OR @Queue=(select top 1 substring(SL_Reference, 5, 3) from dbo.StmALog where SL_Parent=P4_PK and SL_Reference like 'COM""%' and ((" + CommercialProcessQueueNotCompletedFilter + @") or SL_Reference like 'COM""' + @Queue + '""%') and SL_Reference not like 'COM""""%' order by SL_PostedTimeUtc desc))
GROUP BY CS_PK,P4_PK
)
";
				return new DatabaseViewAndRoutineCreateScript("ClientFinanceReleaseReport", result, "DROP FUNCTION ClientFinanceReleaseReport", DbRoutineType.SqlFunctionInlineTypeDesc);
			}
		}

		#endregion

		#region FUNCTION ClientCustomsJobQueuedReport / ClientCommercialJobQueuedReport

		static DatabaseViewAndRoutineCreateScript ClientCustomsJobQueuedReport
		{
			get
			{
				return new DatabaseViewAndRoutineCreateScript("ClientCustomsJobQueuedReport", GetClientJobQueuedReportSQL("ClientCustomsJobQueuedReport", "CUS"), "DROP FUNCTION ClientCustomsJobQueuedReport", DbRoutineType.SqlFunctionInlineTypeDesc);
			}
		}

		static DatabaseViewAndRoutineCreateScript ClientCommercialJobQueuedReport
		{
			get
			{
				return new DatabaseViewAndRoutineCreateScript("ClientCommercialJobQueuedReport", GetClientJobQueuedReportSQL("ClientCommercialJobQueuedReport", "COM"), "DROP FUNCTION ClientCommercialJobQueuedReport", DbRoutineType.SqlFunctionInlineTypeDesc);
			}
		}

		static string GetClientJobQueuedReportSQL(string functionName, string queueType)
		{
			return string.Format(@"
CREATE FUNCTION {0}(@StartDate datetime, @EndDate datetime, @Queue varchar(10), @ReportName varchar(50))

RETURNS TABLE
AS
RETURN
(
SELECT P4_CustomAttrib1 AS InvoiceNumber,
	   case
		   when len(CS_HAWB)=11 then ''
		   else CS_HAWB
	   end AS HAWB,
	   case
		   when len(CS_HAWB)=11 then CS_HAWB
		   else EB_WaybillShortNumber
	   end AS ShortHAWB,
	   CS_RS_NK_ServiceLevel AS ServiceLevel,
	   substring(QueuedLog.SL_Reference, 5, 3) AS Queue,
	   (CASE
		   WHEN substring(QueuedLog.SL_Reference, 11, 1)='""' OR substring(SL_Reference, 11, 1)=',' THEN ''
		   WHEN substring(QueuedLog.SL_Reference, 11, 1)='_' THEN substring(QueuedLog.SL_Reference, 12, 2)
		   ELSE substring(QueuedLog.SL_Reference, 11, 2)
		END) AS QueueReason,
	   CASE WHEN QueuedLog.SL_GS_NKUser='~BP' THEN 'System' ELSE QueuedLog.SL_GS_NKUser END AS QueuedBy,
	   (SELECT top 1 CASE WHEN PreviousLog.SL_Reference like '{1}""""%' THEN '' ELSE substring(PreviousLog.SL_Reference, 5, 3) END
		  FROM dbo.StmALog PreviousLog
		  WHERE PreviousLog.SL_Parent=QueuedLog.SL_Parent
			AND PreviousLog.SL_SE_NKEvent='QUC'
			AND PreviousLog.SL_PostedTimeUtc < QueuedLog.SL_PostedTimeUtc
			AND PreviousLog.SL_Reference like '{1}""%'
		  ORDER BY SL_PostedTimeUtc DESC
	   ) AS QueuedFrom,
	   SL_EventTime AS QueuedDate,
	   CM_ArrivalDate AS ArrivalDate,
	   (select sum(CASE JR_OSSellAmt
				   WHEN 0 THEN (JR_OSCostAmt - JR_LocalCostAmt) + JR_OSSellAmt * 1.1
				   ELSE        JR_OSCostAmt + (JR_OSSellAmt - JR_LocalCostAmt) * 1.1
				   END)
		FROM dbo.JobHeader 
		LEFT OUTER JOIN dbo.JobCharge ON JH_PK=JR_JH
		WHERE JH_ParentID=CS_PK
	   ) AS CODInvoiceValue,
	   CS_PiecesLanded AS Pieces,
	   P4_CustomAttrib2 BillToAccountNum,
	   substring(CS_RL_NKOrigin, 1, 2) OriginCountry,
	   (datepart(year, SL_EventTime) * 1000000 + datepart(month, SL_EventTime) * 10000 + datepart(day, SL_EventTime) * 100 + datepart(hour, SL_EventTime)) QueuedForHourGroupBy,
	   (convert(varchar, datepart(day, SL_EventTime)) + '-' + convert(varchar, datepart(month, SL_EventTime)) + '-' + convert(varchar, datepart(year, SL_EventTime)) + ' ' +
		(CASE
			WHEN datepart(hour, SL_EventTime) > 12 THEN (convert(varchar, datepart(hour, SL_EventTime) - 12)) + 'pm'
			WHEN datepart(hour, SL_EventTime) = 0  THEN '12am'
			WHEN datepart(hour, SL_EventTime) = 12 THEN '12pm'
			WHEN datepart(hour, SL_EventTime) < 12 THEN convert(varchar, datepart(hour, SL_EventTime)) + 'am'
		 END)
	   ) QueuedForHourText
FROM   dbo.ProcessQueue
	   INNER JOIN dbo.StmALog QueuedLog ON SL_Parent=P4_PK AND SL_SE_NKEvent='QUC'
	   INNER JOIN dbo.CusHAWB ON P4_ParentID=CS_PK
	   INNER JOIN dbo.CusMAWB ON CM_PK=CS_CM
	   LEFT OUTER JOIN dbo.JobRelatedWayBill ON EB_ParentID=CS_PK AND EB_WaybillType='PAR'
	   CROSS APPLY dbo.ClientIsBranchUPECustomised(CM_GB) AS Branch
  WHERE  Branch.Enabled = 1
  AND  (@StartDate = '' OR SL_PostedTimeUtc >= @StartDate)
  AND  (@EndDate = '' OR SL_PostedTimeUtc <= dateadd(day, 1, @EndDate))
  AND  (P4_Status != '" + ReasonCodeDescriptionPairList.Codes._C1_SubsequentSplitShipment + @"')
  AND  SL_Reference like '{1}""%'
  AND  SL_Reference not like '{1}""""%'
  AND  (patindex('%jump%', @ReportName) = 0 OR SL_GS_NKUser != '~BP')
  AND  (@Queue='ALL' OR @Queue=substring(SL_Reference, 5, 3))
  AND  substring(SL_Reference, 5, 3) != (SELECT top 1 substring(PreviousLog.SL_Reference, 5, 3)
										 FROM dbo.StmALog PreviousLog
										 WHERE PreviousLog.SL_Parent=QueuedLog.SL_Parent
										   AND PreviousLog.SL_SE_NKEvent='QUC'
										   AND PreviousLog.SL_PostedTimeUtc < QueuedLog.SL_PostedTimeUtc
										   AND PreviousLog.SL_Reference like '{1}""%'
										 ORDER BY PreviousLog.SL_PostedTimeUtc DESC)
)
", functionName, queueType);
		}

		#endregion

		#region FUNCTION ClientBISIInterchangeReport

		static DatabaseViewAndRoutineCreateScript ClientBISIInterchangeReport
		{
			get
			{
				return new DatabaseViewAndRoutineCreateScript("ClientBISIInterchangeReport", @"
CREATE FUNCTION ClientBISIInterchangeReport(@BatchNumber int)

RETURNS TABLE
AS
RETURN
(
SELECT 	CS_HAWB AS HAWB, 
		(CASE
			WHEN CS_IsResponsePending = 1 THEN 'WAIT'
			ELSE CS_CustomsStatus
		 END) AS ACAStatus,
		dbo.ClientGetBillingTermsDescription(P4_CustomAttrib3) As BillingTerms,
		CS_RL_NKDestination AS Destination,
		T8_UploadBatchNumber AS BatchNumber,
		T9_ChargeType AS ChargeType,
		T9_GrossAmount AS GrossAmount
FROM	dbo.CusHAWB
		INNER JOIN dbo.CusMAWB ON CM_PK=CS_CM
		INNER JOIN dbo.ProcessQueue ON P4_ParentID = CS_PK
		INNER JOIN ClientBISIShipmentHeader ON T8_CS = CS_PK
		LEFT OUTER JOIN ClientBISIShipmentCharge ON T9_T8 = T8_PK
		CROSS APPLY dbo.ClientIsBranchUPECustomised(CM_GB) AS Branch
WHERE Branch.Enabled = 1 AND
		P4_CustomDate1 IS NOT NULL AND
		T8_UploadBatchNumber=@BatchNumber
)
",
					"DROP FUNCTION ClientBISIInterchangeReport",
					DbRoutineType.SqlFunctionInlineTypeDesc);
			}
		}

		#endregion

		#region FUNCTION ClientBISIUploadWarningReport

		static DatabaseViewAndRoutineCreateScript ClientBISIUploadWarningReport
		{
			get
			{
				string createSql = @"
CREATE VIEW vw_Report_ClientBISIUploadWarningReport
AS
SELECT CS_PK AS CusHAWBPK,
	   isnull(EB_WaybillShortNumber, CS_HAWB) AS ShortHAWB,
	   CASE
		 WHEN CS_IsResponsePending = 1 THEN 'WAIT'
		 ELSE CS_CustomsStatus
	   END AS ACAStatus,
	   dbo.ClientGetBillingTermsDescription(P4_CustomAttrib3) As FreightTerms,
	   dbo.ClientGetBillingTermsDescription(P4_CustomAttrib3) As BillingTermsDescription,
	   P4_CustomAttrib3 As BillingTerms,
	   T8_UploadBatchNumber AS BatchNumber,
	   T9_ChargeType AS ChargeType,
	   T9_GrossAmount AS GrossAmount,
	   P4_CustomDate1 AS BISIUploadDate
FROM dbo.CusHAWB
	INNER JOIN dbo.CusMAWB ON CM_PK=CS_CM
	LEFT OUTER JOIN dbo.JobRelatedWayBill ON EB_ParentID = CS_PK AND EB_WaybillType = 'PAR'
	INNER JOIN dbo.ProcessQueue ON P4_ParentID = CS_PK AND P4_CustomFlag1 = 0
		AND P4_CustomDate2 IS NULL 
		AND P4_CustomDate1 >= DATEADD(MONTH, -1, GETDATE())
		AND DATEDIFF(MINUTE, P4_CustomDate1, GETDATE()) >= 60
	INNER JOIN ClientBISIShipmentHeader ON T8_CS = CS_PK
	LEFT OUTER JOIN ClientBISIShipmentCharge ON T9_T8 = T8_PK
	CROSS APPLY dbo.ClientIsBranchUPECustomised(CM_GB) AS Branch
WHERE
	Branch.Enabled = 1 
	AND NOT (CS_JE_CustomsFormalEntry IS NULL and P4_CustomAttrib3 = '" + BillingTermsCodeDescriptionPairList.Codes.Prepaid + @"')
	AND CS_SystemCreateTimeUTC >= DATEADD(MONTH, -4, GETUTCDATE())
	AND CS_PK NOT IN
	(
		SELECT EB_ParentID 
		FROM dbo.JobRelatedWayBill
		INNER JOIN ClientPWSHeader on 
			(EB_WaybillShortNumber = U1_WayBillShortNumber AND (U1_WayBillNumber IS NULL or U1_WayBillNumber ='')) OR (EB_WaybillNumber = U1_WayBillNumber) 
		UNION
		SELECT CS_PK 
		FROM dbo.CUSHAWB
		INNER JOIN ClientPWSHeader on U1_WayBillNumber = CS_HAWB 
	)";
				return new DatabaseViewAndRoutineCreateScript("vw_Report_ClientBISIUploadWarningReport", createSql, "DROP VIEW vw_Report_ClientBISIUploadWarningReport", DbRoutineType.SqlViewTypeDesc);
			}
		}

		#endregion

		#region FUNCTION ClientDeclarationHasValidReasonCode

		static DatabaseViewAndRoutineCreateScript ClientDeclarationHasValidReasonCode
		{
			get
			{
				return new DatabaseViewAndRoutineCreateScript("ClientDeclarationHasValidReasonCode", @"
CREATE FUNCTION ClientDeclarationHasValidReasonCode(@DeclarationPK uniqueidentifier)
RETURNS char(1)
AS
BEGIN

DECLARE @Result char(1)

SET @Result = CASE WHEN

	(SELECT count(*)
	FROM dbo.ProcessQueue
	WHERE P4_ParentID=@DeclarationPK AND
		  P4_CustomsStatus <> '' AND
		  P4_Status not like '_%') > 0

	THEN 'Y' ELSE 'N' END

RETURN @Result

END
",
					"DROP FUNCTION ClientDeclarationHasValidReasonCode",
					DbRoutineType.SqlFunctionScalarTypeDesc);
			}
		}

		#endregion

		#region FUNCTION ClientBrokerageExceptionsReport

		static DatabaseViewAndRoutineCreateScript ClientBrokerageExceptionsReport
		{
			get
			{
				return new DatabaseViewAndRoutineCreateScript("ClientBrokerageExceptionsReport", @"
CREATE FUNCTION ClientBrokerageExceptionsReport(@StartDate datetime, @EndDate datetime)
RETURNS TABLE
AS
RETURN
(
	SELECT	P4_CustomsStatus AS Code,
		dbo.ClientGetReasonCodeDescription(P4_CustomsStatus) AS [Description],
		CS_RS_NK_ServiceLevel AS ServiceLevel,
		COUNT(*) as Count
	FROM dbo.ProcessQueue CurrentQueue
		LEFT OUTER JOIN dbo.JobDeclaration ON P4_ParentID = JE_PK OR JE_PK = (select CS_JE_CustomsFormalEntry from dbo.CusHAWB where CS_PK=P4_ParentID)
		LEFT OUTER JOIN dbo.CusHAWB ON CS_PK = (select top 1 CS_PK from dbo.CusHAWB where P4_ParentID = CS_PK OR CS_JE_CustomsFormalEntry=JE_PK)
		INNER JOIN dbo.CusMAWB ON CM_PK = CS_CM
		INNER JOIN dbo.StmALog ON SL_Parent = P4_PK AND SL_IsCancelled='N'
		CROSS APPLY dbo.ClientIsBranchUPECustomised(CM_GB) AS Branch
	WHERE Branch.Enabled = 1 AND
		  ((dbo.ClientDeclarationHasValidReasonCode(JE_PK)='Y' AND P4_ParentID=JE_PK) OR
			   (dbo.ClientDeclarationHasValidReasonCode(JE_PK)='N' AND CS_PK IS NOT NULL)) AND
		  CS_RS_NK_ServiceLevel IN ('1', '5') AND
		  P4_CustomsStatus <> '' AND
		  P4_Status not like '_%' AND
		  SL_PostedTimeUtc = (select max(SL_PostedTimeUtc)
						  FROM dbo.StmALog
						  WHERE SL_Parent = P4_PK AND
								SL_PostedTimeUtc BETWEEN @StartDate AND DATEADD(day, 1, @EndDate) AND
								SL_IsCancelled = 'N')
	GROUP BY P4_CustomsStatus, CS_RS_NK_ServiceLevel
)
",
					"DROP FUNCTION ClientBrokerageExceptionsReport",
					DbRoutineType.SqlFunctionInlineTypeDesc);
			}
		}

		#endregion

		#region FUNCTION ClientCMRMissingSupplierImporterCodesReport

		static DatabaseViewAndRoutineCreateScript ClientCMRMissingSupplierImporterCodesReport
		{
			get
			{
				return new DatabaseViewAndRoutineCreateScript("ClientCMRMissingSupplierImporterCodesReport", @"
CREATE FUNCTION ClientCMRMissingSupplierImporterCodesReport(@UseImporter char(1))
RETURNS TABLE
AS
RETURN
(
	SELECT  Declaration.JE_PK AS DeclarationPK, 
			Declaration.JE_DeclarationReference AS DeclarationReference,
			Declaration.JE_HouseBill AS HAWB, 
			Declaration.JE_DateOfArrival AS Arrival, 
			Declaration.JE_RS_NKServiceLevel AS ServiceLevel, 
			Declaration.JE_TotalNoOfPacks As TotalNoOfPacks,
			Organisation.OH_Code AS Code,
			Organisation.OH_FullName AS FullName,
			MainAddress.OA_Address1 AS Address1, 
			MainAddress.OA_Address2 AS Address2, 
			MainAddress.OA_City AS City, 
			MainAddress.OA_Phone AS Phone,
			Declaration.JE_MasterBill AS MAWB
	FROM    dbo.JobDeclaration Declaration
			INNER JOIN dbo.ProcessQueue ON Declaration.JE_PK = P4_ParentID AND Declaration.JE_IsCancelled = 0
			INNER JOIN dbo.OrgHeader Organisation ON (@UseImporter='Y' AND Declaration.JE_OH_Importer = Organisation.OH_PK AND Declaration.JE_IsCancelled = 0) OR (@UseImporter='N' AND Declaration.JE_OH_Supplier = Organisation.OH_PK AND Declaration.JE_IsCancelled = 0)
			INNER JOIN dbo.OrgAddress MainAddress ON Organisation.OH_PK = MainAddress.OA_OH 
			AND MainAddress.OA_PK IN (SELECT PZ_OA FROM dbo.OrgAddressCapability WHERE OrgAddressCapability.PZ_AddressType='OFC')
			CROSS APPLY dbo.ClientIsBranchUPECustomised(JE_GB) AS Branch
	WHERE	Branch.Enabled = 1 AND
			(P4_CustomsQueue='" + DeclarationQueueCodeDescriptionPairList.Codes.Classification + "' OR P4_CustomsQueue='" + DeclarationQueueCodeDescriptionPairList.Codes.Compiling + @"') AND
			((@UseImporter='Y' AND
			 (Declaration.JE_OH_Importer NOT IN
				(SELECT OK_OH
				FROM dbo.OrgCusCode
				WHERE OK_CodeType IN ('" + OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber + "', '" + OrgCusCode.CodeTypes.CustomsClientID + "', '" + OrgCusCode.CodeTypes.GSTCode + @"')))) OR
			(@UseImporter='N' AND
			 (Declaration.JE_OH_Supplier NOT IN
				(SELECT OK_OH
				FROM dbo.OrgCusCode
				WHERE OK_CodeType IN ('" + OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber + "', '" + OrgCusCode.CodeTypes.CustomsClientID + "', '" + OrgCusCode.CodeTypes.GSTCode + @"')))))
)
",
					"DROP FUNCTION ClientCMRMissingSupplierImporterCodesReport",
					DbRoutineType.SqlFunctionInlineTypeDesc);
			}
		}

		#endregion

		#region FUNCTION ClientShipmentLodgedReport

		static DatabaseViewAndRoutineCreateScript ClientShipmentLodgedReport
		{
			get
			{
				return new DatabaseViewAndRoutineCreateScript("ClientShipmentLodgedReport", @"
CREATE FUNCTION ClientShipmentLodgedReport(@ArrivalStartDate datetime, @ArrivalEndDate datetime, @LodgedStartDate datetime, @LodgedEndDate datetime, @LodgementQueuedBy varchar(3))
RETURNS TABLE
AS
RETURN
(
	SELECT Declaration.JE_PK AS DeclarationPK,				Declaration.JE_HouseBill AS HAWB,
		Declaration.JE_RS_NKServiceLevel AS ServiceLevel,	Declaration.JE_TotalNoOfPacks AS Packs,
		Declaration.JE_TotalWeight AS Weight,					Declaration.JE_GoodsDescription AS GoodsDescription,
		Declaration.JE_EntryStatus AS EntryStatus,			substring(Declaration.JE_RL_NKOrigin, 1, 2) AS Origin,
		Supplier.OH_FullName AS SupplierName,					Importer.OH_FullName AS ImporterName,
		(select top 1 SL_EventTime from dbo.StmALog WHERE SL_Parent=P4_PK AND SL_SE_NKEvent='QUC' AND SL_Reference like 'CUS^LDG^%' order by SL_PostedTimeUtc) AS LodgementQueuedDate,
		(select top 1 SL_GS_NKUser  from dbo.StmALog WHERE SL_Parent=P4_PK AND SL_SE_NKEvent='QUC' AND SL_Reference like 'CUS^LDG^%' order by SL_PostedTimeUtc) AS LodgementQueuedBy,
		CASE isnull((SELECT top 1 SL_GS_NKUser FROM dbo.StmALog WHERE SL_Parent=P4_PK and SL_SE_NKEvent='QUC' and SL_Reference like 'CUS^LDG^%' ORDER BY SL_PostedTimeUtc), '')
			WHEN '~BP' THEN 'Lodgement Queued By System'
			WHEN ''  THEN 'Lodgement Queued By Nobody' ELSE 'Lodgement Queued By ' +
			(SELECT top 1 SL_GS_NKUser  from dbo.StmALog WHERE SL_Parent=P4_PK and SL_SE_NKEvent='QUC' and SL_Reference like 'CUS^LDG^%' order by SL_PostedTimeUtc)
			END AS LodgementQueuedByOrNobody,
			(select top 1 SL_EventTime from dbo.StmALog WHERE SL_Parent=JE_PK and SL_SE_NKEvent='CCC' order by SL_PostedTimeUtc desc) AS LodgedDate,
			isnull('Lodged By ' + 
				(select top 1 SL_GS_NKUser from dbo.StmALog WHERE SL_Parent=JE_PK and SL_SE_NKEvent='CCC' order by SL_PostedTimeUtc desc), 'Not Lodged') AS LodgedByOrNotLodged

	FROM dbo.JobDeclaration Declaration
		INNER JOIN dbo.ProcessQueue Queue ON Declaration.JE_PK=P4_ParentID
		LEFT OUTER JOIN dbo.OrgHeader Supplier ON Declaration.JE_OH_Supplier = Supplier.OH_PK
		LEFT OUTER JOIN dbo.OrgHeader Importer ON Declaration.JE_OH_Importer = Importer.OH_PK
		CROSS APPLY dbo.ClientIsBranchUPECustomised(JE_GB) AS Branch
	WHERE Branch.Enabled = 1 AND
		(EXISTS (select top 1 SL_PostedTimeUtc from dbo.StmALog where SL_Parent=P4_PK and SL_SE_NKEvent='QUC' and SL_Reference like 'CUS^LDG^%')
		OR EXISTS (select top 1 SL_PostedTimeUtc from dbo.StmALog where SL_Parent=JE_PK and SL_SE_NKEvent='CCC'))
	  AND ((@ArrivalStartDate = '' OR Declaration.JE_DateOfArrival >= @ArrivalStartDate) AND (@ArrivalEndDate = '' OR Declaration.JE_DateOfArrival <= dateadd(day, 1, @ArrivalEndDate)))
	  AND ((@LodgedStartDate = '' OR 
			(SELECT top 1 SL_PostedTimeUtc FROM dbo.StmALog WHERE SL_Parent=JE_PK and SL_SE_NKEvent='CCC' order by SL_PostedTimeUtc desc) >= @LodgedStartDate) 
				AND (@LodgedEndDate = '' 
					OR (SELECT top 1 SL_PostedTimeUtc FROM dbo.StmALog WHERE SL_Parent=JE_PK AND SL_SE_NKEvent='CCC' ORDER BY SL_PostedTimeUtc desc) <= dateadd(day, 1, @LodgedEndDate)))
	  AND (@LodgementQueuedBy is null OR @LodgementQueuedBy = '' 
			OR (select top 1 SL_GS_NKUser FROM dbo.StmALog WHERE SL_Parent=P4_PK and SL_SE_NKEvent='QUC' and SL_Reference like 'CUS^LDG^%' order by SL_PostedTimeUtc) = @LodgementQueuedBy)

	UNION ALL

	SELECT Declaration.JE_PK AS DeclarationPK,					Declaration.JE_HouseBill AS HAWB,
		Declaration.JE_RS_NKServiceLevel AS ServiceLevel,		Declaration.JE_TotalNoOfPacks AS Packs,
		Declaration.JE_TotalWeight AS Weight,						Declaration.JE_GoodsDescription AS GoodsDescription,
		Declaration.JE_EntryStatus AS EntryStatus,				substring(Declaration.JE_RL_NKOrigin, 1, 2) AS Origin,
		Supplier.OH_FullName AS SupplierName,						Importer.OH_FullName AS ImporterName,
		(select top 1 SL_EventTime from dbo.StmALog where SL_Parent=P4_PK and SL_SE_NKEvent='QUC' and SL_Reference like 'CUS^SUB^%' order by SL_PostedTimeUtc) AS LodgementQueuedDate,
		(select top 1 SL_GS_NKUser  from dbo.StmALog where SL_Parent=P4_PK and SL_SE_NKEvent='QUC' and SL_Reference like 'CUS^SUB^%' order by SL_PostedTimeUtc) AS LodgementQueuedBy,
		CASE isnull((select top 1 SL_GS_NKUser  from dbo.StmALog where SL_Parent=P4_PK and SL_SE_NKEvent='QUC' and SL_Reference like 'CUS^SUB^%' ORDER BY SL_PostedTimeUtc), '')
			WHEN '~BP' THEN 'Lodgement Queued By System'
			WHEN ''  THEN 'Lodgement Queued By Nobody' ELSE 'Lodgement Queued By ' +
			(SELECT top 1 SL_GS_NKUser FROM dbo.StmALog WHERE SL_Parent=P4_PK and SL_SE_NKEvent='QUC' and SL_Reference like 'CUS^SUB^%' ORDER BY SL_PostedTimeUtc)
			END AS LodgementQueuedByOrNobody,
			(SELECT top 1 SL_EventTime FROM dbo.StmALog WHERE SL_Parent=JE_PK and SL_SE_NKEvent='CCC' ORDER BY SL_PostedTimeUtc desc) AS LodgedDate,
			isnull('Lodged By ' + 
				(SELECT top 1 SL_GS_NKUser FROM dbo.StmALog WHERE SL_Parent=JE_PK and SL_SE_NKEvent='CCC' ORDER BY SL_PostedTimeUtc desc), 'Not Lodged') AS LodgedByOrNotLodged

	FROM dbo.JobDeclaration Declaration
		INNER JOIN dbo.ProcessQueue Queue ON Declaration.JE_PK=P4_ParentID
		LEFT OUTER JOIN dbo.OrgHeader Supplier ON Declaration.JE_OH_Supplier = Supplier.OH_PK
		LEFT OUTER JOIN dbo.OrgHeader Importer ON Declaration.JE_OH_Importer = Importer.OH_PK
		CROSS APPLY dbo.ClientIsBranchUPECustomised(JE_GB) AS Branch
	WHERE Branch.Enabled = 1 AND
		((EXISTS (SELECT top 1 SL_PostedTimeUtc FROM dbo.StmALog WHERE SL_Parent=P4_PK and SL_SE_NKEvent='QUC' AND SL_Reference like 'CUS^CLS^%')
		AND EXISTS (SELECT top 1 SL_PostedTimeUtc FROM dbo.StmALog WHERE SL_Parent=P4_PK and SL_SE_NKEvent='QUC' AND SL_Reference like 'CUS^SUB^%')
		AND NOT EXISTS (SELECT top 1 SL_PostedTimeUtc FROM dbo.StmALog WHERE SL_Parent=P4_PK and SL_SE_NKEvent='QUC' and SL_Reference like 'CUS^LDG^%'))
		AND ((@ArrivalStartDate = '' OR Declaration.JE_DateOfArrival >= @ArrivalStartDate) AND (@ArrivalEndDate = '' OR Declaration.JE_DateOfArrival <= dateadd(day, 1, @ArrivalEndDate)))
	  AND ((@LodgedStartDate = '' 
			OR (SELECT top 1 SL_PostedTimeUtc FROM dbo.StmALog WHERE SL_Parent=JE_PK and SL_SE_NKEvent='CCC' ORDER BY SL_PostedTimeUtc DESC) >= @LodgedStartDate) 
			AND (@LodgedEndDate = '' 
				OR (SELECT top 1 SL_PostedTimeUtc FROM dbo.StmALog WHERE SL_Parent=JE_PK and SL_SE_NKEvent='CCC' ORDER BY SL_PostedTimeUtc DESC) <= dateadd(day, 1, @LodgedEndDate)))
	  AND (@LodgementQueuedBy is null OR @LodgementQueuedBy = '' 
			OR (SELECT top 1 SL_GS_NKUser FROM dbo.StmALog WHERE SL_Parent=P4_PK and SL_SE_NKEvent='QUC' and SL_Reference like 'CUS^SUB^%' ORDER BY SL_PostedTimeUtc) = @LodgementQueuedBy))
)
".Replace("^", "\""), "DROP FUNCTION ClientShipmentLodgedReport", DbRoutineType.SqlFunctionInlineTypeDesc);
			}
		}

		#endregion

		#region FUNCTIONS ClientGetDatePartOnly / ClientMinDate / ClientMaxDate

		static DatabaseViewAndRoutineCreateScript ClientGetDatePartOnly
		{
			get
			{
				return new DatabaseViewAndRoutineCreateScript("ClientGetDatePartOnly", @"
CREATE FUNCTION ClientGetDatePartOnly(@Date datetime)
RETURNS datetime
AS
BEGIN
RETURN convert(datetime, floor(convert(float, @Date)))
END
", "DROP FUNCTION ClientGetDatePartOnly", DbRoutineType.SqlFunctionScalarTypeDesc);
			}
		}

		static DatabaseViewAndRoutineCreateScript ClientMinDate
		{
			get
			{
				return new DatabaseViewAndRoutineCreateScript("ClientMinDate", @"
CREATE FUNCTION ClientMinDate(@Date1 datetime, @Date2 datetime)
RETURNS datetime
AS
BEGIN
RETURN CASE
	WHEN (@Date1 < @Date2) THEN @Date1
	ELSE @Date2
END
END
", "DROP FUNCTION ClientMinDate", DbRoutineType.SqlFunctionScalarTypeDesc);
			}
		}

		static DatabaseViewAndRoutineCreateScript ClientMaxDate
		{
			get
			{
				return new DatabaseViewAndRoutineCreateScript("ClientMaxDate", @"
CREATE FUNCTION ClientMaxDate(@Date1 datetime, @Date2 datetime)
RETURNS datetime
AS
BEGIN
RETURN CASE
	WHEN (@Date1 > @Date2) THEN @Date1
	ELSE @Date2
END
END
", "DROP FUNCTION ClientMaxDate", DbRoutineType.SqlFunctionScalarTypeDesc);
			}
		}

		#endregion

		#region FUNCTION ClientRefundReport

		static DatabaseViewAndRoutineCreateScript ClientRefundReport
		{
			get
			{
				string sQL = @"
CREATE PROCEDURE ClientRefundReport(@DateFrom datetime, @DateTo datetime, 
				  @OpenedRefunds char(1), @ClosedRefunds char(1), 
				  @WriteOff char(1), @Refunded char(1), @AdditionalCharges char(1), 
				  @RefundToUPS char(1), @OutstandingDays char(5) , @AtFault char(3), @User char(3), @Reason char(10))
AS BEGIN
SELECT
	T10_GS_NKCreatedUser,
	T10_ControlNumber,
	Importer.OH_FullName AS ImporterFullName,
	JE_HouseBill AS HouseBill,
	T10_DateCreated,
	T10_DateProcessed,
	(CASE
		 WHEN T10_IsRefundRejected='Y' THEN 0
		 ELSE datediff(day, dbo.ClientGetDatePartOnly(T10_DateCreated), isnull(dbo.ClientGetDatePartOnly(T10_DateProcessed), dbo.ClientGetDatePartOnly(getdate())))
	 END) AS DaysOutstanding,
	(CASE
		 WHEN T10_IsRefundRejected='Y' THEN 0
		 ELSE datediff(day, dbo.ClientGetDatePartOnly(T10_DateCreated), isnull(T10_DateProcessed, dbo.ClientGetDatePartOnly(getdate()))) / 7 * 10
	 END) AS PointsDeducted,
	(CASE WHEN T10_IsRefundRejected='Y' THEN 0 ELSE T10_WriteOffAmount END) AS T10_WriteOffAmount,
	(CASE WHEN T10_IsRefundRejected='Y' THEN 0 ELSE T10_RefundAmount END) AS T10_RefundAmount,
	T10_AdditionalCharges,
	T10_AmountRefundedToUPS,
	T10_GS_NKAtFaultUser,
	T10_RefundReason,
	T10_EnquiryRaisedBy
FROM ClientRefund
INNER JOIN dbo.JobDeclaration ON T10_JE = JE_PK
INNER JOIN dbo.OrgHeader Importer ON JE_OH_Importer=Importer.OH_PK
CROSS APPLY dbo.ClientIsBranchUPECustomised(JE_GB) AS Branch
WHERE Branch.Enabled = 1 AND
	  ((@DateFrom = '' OR isnull(T10_DateProcessed, getdate()) >= @DateFrom) AND (@DateTo = '' OR T10_DateCreated <= dateadd(day, 1, @DateTo)) AND
	  ((@OpenedRefunds = 'Y' AND (T10_DateCreated IS NOT NULL AND T10_DateProcessed IS NULL)) OR (@OpenedRefunds = '')) AND
	  ((@ClosedRefunds = 'Y' AND (T10_DateProcessed IS NOT NULL)) OR (@ClosedRefunds = '')) AND
	  ((@WriteOff = 'Y' AND (T10_WriteOffAmount > 0)) OR (@WriteOff = '')) AND
	  ((@Refunded = 'Y' AND (T10_IsRefundRejected = 'N')) OR (@Refunded = '')) AND
	  ((@AdditionalCharges = 'Y' AND (T10_AdditionalCharges > 0)) OR (@AdditionalCharges = '')) AND
	  ((@RefundToUPS = 'Y' AND (T10_AmountRefundedToUPS > 0)) OR (@RefundToUPS = '')) AND
	  ((CONVERT(int, CAST(@OutstandingDays AS FLOAT)) <=  datediff(day, dbo.ClientGetDatePartOnly(T10_DateCreated), isnull(dbo.ClientGetDatePartOnly(T10_DateProcessed), dbo.ClientGetDatePartOnly(getdate())))) OR ((CONVERT(int, CAST(@OutstandingDays AS FLOAT)) = 0))) AND
	  ((@AtFault = T10_GS_NKAtFaultUser) OR (@AtFault = '')) AND
	  ((@User = T10_GS_NKCreatedUser) OR (@User = '')) AND
	  ((@Reason = T10_RefundReason) OR (@Reason = '')) AND
	  T10_ControlNumber <> ''
)
END
";
				return new DatabaseViewAndRoutineCreateScript("ClientRefundReport", sQL, "DROP PROCEDURE ClientRefundReport", DbRoutineType.SqlProcedureTypeDesc);
			}
		}

		#endregion

		#region FUNCTION ClientADPScoringReport

		static DatabaseViewAndRoutineCreateScript ClientADPScoringReport
		{
			get
			{
				return new DatabaseViewAndRoutineCreateScript("ClientADPScoringReport", @"
CREATE PROCEDURE ClientADPScoringReport(@DateFrom datetime, @DateTo datetime)
AS BEGIN

SELECT GS_Code AS UserInitials,
	   (SELECT COUNT(*)
		FROM ClientADPScoring
		WHERE T4_IsStaffWorkingDay='Y'
		  AND T4_GS_NKUser=GS_Code
		  AND (@DateFrom = '' OR T4_Date >= @DateFrom)
		  AND (@DateTo = '' OR T4_Date < dateadd(day, 1, @DateTo))) AS Days,
	   ISNULL((SELECT SUM(T4_SubmissionCount)
			   FROM ClientADPScoring
			   WHERE (T4_IsStaffWorkingDay='Y'
				 AND (@DateFrom = '' OR T4_Date >= @DateFrom)
				 AND (@DateTo = '' OR T4_Date < dateadd(day, 1, @DateTo)))
				 AND T4_GS_NKUser=GS_Code), 0) AS SubmissionPoints,
	   ISNULL((SELECT SUM(T4_SubmissionErrorCount)
			   FROM ClientADPScoring
			   WHERE (T4_IsStaffWorkingDay='Y'
				 AND (@DateFrom = '' OR T4_Date >= @DateFrom)
				 AND (@DateTo = '' OR T4_Date < dateadd(day, 1, @DateTo)))
				 AND T4_GS_NKUser=GS_Code), 0) * -2 AS SubmissionErrorPoints,
	   ISNULL((SELECT SUM(T4_RegularVolumeCount)
			   FROM ClientADPScoring
			   WHERE (T4_IsStaffWorkingDay='Y'
				 AND (@DateFrom = '' OR T4_Date >= @DateFrom)
				 AND (@DateTo = '' OR T4_Date < dateadd(day, 1, @DateTo)))
				 AND T4_GS_NKUser=GS_Code), 0) * 250 AS RegularVolumePoints,
	   ISNULL(((SELECT SUM(datediff(day,
									dbo.ClientGetDatePartOnly(dbo.ClientMaxDate(@DateFrom, T10_DateCreated)),
									dbo.ClientGetDatePartOnly(dbo.ClientMinDate(@DateTo, isnull(T10_DateProcessed, getdate())))))
				FROM ClientRefund
				WHERE T10_IsRefundRejected='N'
					  AND T10_GS_NKCreatedUser=GS_Code
					  AND (@DateFrom = '' OR isnull(T10_DateProcessed, getdate()) >= @DateFrom)
					  AND (@DateTo   = '' OR T10_DateCreated < dateadd(day, 1, @DateTo))
			   ) / 7), 0) * -10 AS RefundPoints
FROM dbo.GlbStaff
LEFT OUTER JOIN ClientADPScoring ON T4_GS_NKUser=GS_Code
WHERE (T4_IsStaffWorkingDay='Y' AND (@DateFrom = '' OR T4_Date >= @DateFrom) AND (@DateTo = '' OR T4_Date < dateadd(day, 1, @DateTo)))
	  OR (EXISTS (SELECT null FROM ClientRefund
				  WHERE (T10_IsRefundRejected='N'
					AND T10_GS_NKCreatedUser=GS_Code
					AND (@DateFrom = '' OR isnull(T10_DateProcessed, getdate()) >= @DateFrom)
					AND (@DateTo   = '' OR T10_DateCreated < dateadd(day, 1, @DateTo)))))
GROUP BY GS_Code
END
", "DROP PROCEDURE ClientADPScoringReport", DbRoutineType.SqlProcedureTypeDesc);
			}
		}

		#endregion

		#region VIEW ClientUPE_VW_CusHAWB_JobDec

		static DatabaseViewAndRoutineCreateScript ClientUPE_VW_CusHAWB_JobDec
		{
			get
			{
				return new DatabaseViewAndRoutineCreateScript("ClientUPE_VW_CusHAWB_JobDec", @"
					CREATE VIEW ClientUPE_VW_CusHAWB_JobDec AS
					SELECT 
						CS_PK AS PK,
						'CusHAWB' as TableName,
						CS_HAWB as Housebill
					FROM 
						dbo.CusHAWB JOIN dbo.CusMAWB ON CS_CM = CM_PK
					CROSS APPLY dbo.ClientIsBranchUPECustomised(CM_GB) AS Branch
					WHERE
						Branch.Enabled = 1
					UNION ALL
					SELECT 
						JE_PK AS PK,
						'JobDeclaration' as TableName,
						JE_Housebill as Housebill
					FROM 
						dbo.JobDeclaration
					CROSS APPLY dbo.ClientIsBranchUPECustomised(JE_GB) AS Branch
					WHERE
						Branch.Enabled = 1", "DROP VIEW ClientUPE_VW_CusHAWB_JobDec", DbRoutineType.SqlViewTypeDesc);
			}
		}

		#endregion

		#region FUNCTION ClientUPEProductivityReport

		static DatabaseViewAndRoutineCreateScript ClientUPEProductivityReport
		{
			get
			{
				return new DatabaseViewAndRoutineCreateScript("ClientUPEProductivityReport", @"
CREATE FUNCTION ClientUPEProductivityReport(@LogPostedDateFrom datetime, @LogPostedDateTo datetime, @Staff varchar(3), @Group varchar(38))
RETURNS TABLE
AS
RETURN
(
	select  EditedRecords.Location,
				substring (
						InnerQueueWorked, 
						charindex('^', InnerQueueWorked) + 1,
						charindex('^', InnerQueueWorked, charindex('^', InnerQueueWorked) - charindex('^', InnerQueueWorked)) - 1
				) as QueueWorked,
				isnull(GS_FullName, SL_GS_NKUser) as [User],
				count(*) as NumberOfShipmentsWorked
		from (
				select JobDecOrHAWBLog.SL_PostedTimeUtc,
					   isnull(
							  (
								select top 1 ProcessQueueLog.SL_Reference 
								from dbo.StmALog ProcessQueueLog 
								where ProcessQueueLog.SL_SE_NKEvent = 'QUC' 
								and ProcessQueueLog.SL_Reference like 'CUS%'
								and ProcessQueueLog.SL_Reference not like 'CUS^^%'
								and ProcessQueueLog.SL_PostedTimeUtc < JobDecOrHAWBLog.SL_PostedTimeUtc
								and SL_Parent = JobDecOrHAWBQueue.P4_PK
								order by SL_PostedTimeUtc desc
							  ), ''
							 ) as InnerQueueWorked,
					   JobDecOrHAWBLog.SL_GS_NKUser,
					   CASE WHEN JobDecOrHAWBLog.SL_Table = 'JobDeclaration' THEN 'Declaration'
							WHEN JobDecOrHAWBLog.SL_Table = 'CusHAWB' AND P4_QueueName = 'CAL' THEN 'Finance'
							ELSE 'Cargo Report'
					   END AS Location
				from dbo.StmALog JobDecOrHAWBLog 
				inner join ClientUPE_VW_CusHAWB_JobDec JobDecOrCusHAWB on JobDecOrHAWBLog.SL_Parent = JobDecOrCusHAWB.PK
				inner join dbo.ProcessQueue JobDecOrHAWBQueue on JobDecOrHAWBQueue.P4_ParentID = JobDecOrCusHAWB.PK
				where JobDecOrHAWBLog.SL_SE_NKEvent = 'EDT'
				and JobDecOrHAWBLog.SL_GS_NKUser != '~BP'
			 ) EditedRecords
		inner join dbo.GlbStaff GlbStaff on GS_Code = SL_GS_NKUser
		where SL_PostedTimeUtc >= @LogPostedDateFrom
		and SL_PostedTimeUtc < dateadd(day, 1, @LogPostedDateTo)
		and (@Staff is null or @Staff = '' or SL_GS_NKUser = @Staff)
		and (@Group is null or @Group = '' or exists (
														select 1
														from dbo.GlbGroupLink 
														where GK_GS = GlbStaff.GS_PK
														and GK_GG = @Group
													 )
			)
		and InnerQueueWorked != ''
		group by 
		EditedRecords.Location,
		substring (      
				InnerQueueWorked, 
				charindex('^', InnerQueueWorked) + 1, 
				charindex('^', InnerQueueWorked, charindex('^', InnerQueueWorked) - charindex('^', InnerQueueWorked)) - 1
				 ),
		isnull(GS_FullName, SL_GS_NKUser)
)".Replace("^", "\""), "DROP FUNCTION ClientUPEProductivityReport", DbRoutineType.SqlFunctionInlineTypeDesc);
			}
		}

		#endregion

		#region FUNCTION ClientUPEEIRReport

		static DatabaseViewAndRoutineCreateScript ClientUPEEIRReport
		{
			get
			{
				return new DatabaseViewAndRoutineCreateScript("ClientUPEEIRReport", @"
CREATE FUNCTION ClientUPEEIRReport(@DateFrom datetime, @DateTo datetime)
RETURNS TABLE
AS
RETURN
(
select 'Cargo Report' as Source, CS_HAWB as Housebill, substring(SL_Reference, 11, 2) as Reason, replace(substring(SL_Reference, 16, 2), '^,', '') as Status
from dbo.StmALog
	inner join dbo.ProcessQueue  on SL_Parent = P4_PK
	inner join dbo.CusHAWB on CS_PK = P4_ParentID
	inner join dbo.CusMAWB on CM_PK = CS_CM
	cross apply dbo.ClientIsBranchUPECustomised(CM_GB) AS Branch
where Branch.Enabled = 1
	and SL_SE_NKEvent = 'QUC'
	and SL_Reference like 'CUS^EIR^%'
	and SL_PostedTimeUtc >= @DateFrom
	and SL_PostedTimeUtc < dateadd(day, 1, @DateTo)
union all
select 'Declaration' as Source, JE_Housebill as Housebill, substring(SL_Reference, 11, 2) as Reason, replace(substring(SL_Reference, 16, 2), '^,', '') as Status
from dbo.StmALog
	inner join dbo.ProcessQueue  on SL_Parent = P4_PK
	inner join dbo.JobDeclaration on JE_PK = P4_ParentID
	cross apply dbo.ClientIsBranchUPECustomised(JE_GB) AS Branch
where Branch.Enabled = 1
	and SL_SE_NKEvent = 'QUC'
	and SL_Reference like 'CUS^EIR^%'
	and SL_PostedTimeUtc >= @DateFrom
	and SL_PostedTimeUtc < dateadd(day, 1, @DateTo)
)".Replace("^", "\""),
					"DROP FUNCTION ClientUPEEIRReport", DbRoutineType.SqlFunctionInlineTypeDesc);
			}
		}

		#endregion

		#region FUNCTION ClientDetailedUPEEIRREport

		static DatabaseViewAndRoutineCreateScript ClientDetailedUPEEIRReport
		{
			get
			{
				return new DatabaseViewAndRoutineCreateScript("ClientDetailedUPEEIRReport", @"
CREATE FUNCTION ClientDetailedUPEEIRReport(@DateFrom datetime, @DateTo datetime)
RETURNS TABLE
AS
RETURN
select 
		convert(varchar(10), P4_CustomDate4, 111) as [Date],
		cast(case when datepart(hour, P4_CustomDate4) in (0, 1, 2, 3, 4 , 5, 6, 7, 8, 9)
			 then '0' + cast(datepart(hour, P4_CustomDate4) as varchar(2)) + ':00'
			 else cast(datepart(hour, P4_CustomDate4) as varchar(2)) + ':00'
		end as varchar(5))
		+ '-' +
		cast(case when (datepart(hour, P4_CustomDate4) + 1) in (0, 1, 2, 3, 4 , 5, 6, 7, 8, 9)
			 then '0' + cast((datepart(hour, P4_CustomDate4) + 1) as varchar(2)) + ':00'
			 else cast((datepart(hour, P4_CustomDate4) + 1) as varchar(2)) + ':00'
		end as varchar(5))

		as [Time],
		case TableName 
			 WHEN 'CusHawb'
			 THEN 'Cargo Report'
			 ELSE 'Declaration'
		end as Source,
		Housebill as Housebill,
		substring(SL_Reference, 11, 2) as Reason,

		case when replace(substring(SL_Reference, 16, 2), '^,', '') = '__'
			 then '_No status'
			 else replace(substring(SL_Reference, 16, 2), '^,', '')
		end as Status,
		1 as ColumnForCount
from dbo.StmALog
inner join dbo.ProcessQueue on SL_Parent = P4_PK
inner join ClientUPE_VW_CusHAWB_JobDec on PK = P4_ParentID 
inner join 
	(
		select 
			SL_Parent,
			MAX(SL_PostedTimeUtc) AS SL_LastPostedTime
		FROM 
			dbo.StmAlog 
			inner join dbo.ProcessQueue  on SL_Parent = P4_PK
		WHERE
			SL_PostedTimeUtc <= P4_CustomDate4
			and SL_SE_NKEvent = 'QUC'
			and SL_Reference like 'CUS^EIR^%'
			and P4_CustomDate4 IS NOT NULL
			and P4_CustomDate4 >= @DateFrom
			and P4_CustomDate4 < dateadd(day, 1, @DateTo)
		GROUP BY
			SL_Parent
	) StmAlogEIRQueEvents
	ON StmAlog.SL_Parent = StmAlogEIRQueEvents.SL_Parent AND StmAlog.SL_PostedTimeUtc = StmAlogEIRQueEvents.SL_LastPostedTime 
where SL_SE_NKEvent = 'QUC' 
	and SL_Reference like 'CUS^EIR^%'
	and P4_CustomDate4 IS NOT NULL
	and P4_CustomDate4 >= @DateFrom
	and P4_CustomDate4 < dateadd(day, 1, @DateTo)
".Replace("^", "\""), "DROP FUNCTION ClientDetailedUPEEIRReport", DbRoutineType.SqlFunctionInlineTypeDesc);
			}
		}

		#endregion

		#region FUNCTION ClientGetRemarksFromQueueLogReference

		static DatabaseViewAndRoutineCreateScript ClientGetRemarksFromQueueLogReference
		{
			get
			{
				return new DatabaseViewAndRoutineCreateScript("ClientGetRemarksFromQueueLogReference", @"
CREATE FUNCTION ClientGetRemarksFromQueueLogReference(@SL_Reference varchar(200))
RETURNS varchar(200)
AS
BEGIN
  declare @result varchar(200)
  set @result = ''

  set @result = replace(substring(@SL_Reference, 
								  charindex('^,^', @SL_Reference, charindex('^,^', @SL_Reference, charindex('^,^', @SL_Reference) + 3) + 3) + 3, 
								  charindex('^,^', @SL_Reference, charindex('^,^', @SL_Reference, charindex('^,^', @SL_Reference, charindex('^,^', @SL_Reference) + 3) + 3) + 3) -
								  ((charindex('^,^', @SL_Reference, charindex('^,^', @SL_Reference, charindex('^,^', @SL_Reference) + 3) + 3) + 3) - 1)
								 ),
						'^',
						'')    
  return(@result)
END
".Replace("^", "\""),
					"DROP FUNCTION ClientGetRemarksFromQueueLogReference", DbRoutineType.SqlFunctionScalarTypeDesc);
			}
		}

		#endregion

		#region FUNCTION ClientUPEInterventionReport

		static DatabaseViewAndRoutineCreateScript ClientUPEInterventionReport
		{
			get
			{
				return new DatabaseViewAndRoutineCreateScript("ClientUPEInterventionReport", @"
CREATE FUNCTION ClientUPEInterventionReport(@DateFrom datetime, @DateTo datetime, @Masterbill varchar(11))
RETURNS TABLE
AS
RETURN
(
  select CM_MAWB as Masterbill,
	   (select top 1 isnull(GS_FullName, CusHAWBLog.SL_GS_NKUser)		
		  from dbo.StmALog CusHAWBLog                  
		  inner join dbo.CusHAWB HAWB  on CS_PK = SL_Parent
		  left outer join dbo.GlbStaff  on GS_Code = SL_GS_NKUser
		  where CusHAWBLog.SL_SE_NKEvent = 'EDT' 
		  and CusHAWBLog.SL_PostedTimeUtc > ProcessQueueLog.SL_PostedTimeUtc
		  and HAWB.CS_HAWB = ProcessQueueHAWB.CS_HAWB
		  order by SL_PostedTimeUtc asc
		) 
		as [User],
		replace (substring ((select top 1 CusHAWBLog.SL_Reference
							   from dbo.StmALog CusHAWBLog                  
							   where CusHAWBLog.SL_Parent = ProcessQueueLog.SL_Parent
							   and CusHAWBLog.SL_SE_NKEvent = 'QUC'
							   and CusHAWBLog.SL_PostedTimeUtc > ProcessQueueLog.SL_PostedTimeUtc          
							   order by SL_PostedTimeUtc asc
							),
							5, 
							3
						   ),
				 '^,^',
								 '') 
		as QueueMovedTo,

		dbo.ClientGetRemarksFromQueueLogReference((select top 1 CusHAWBLog.SL_Reference
											 from dbo.StmALog CusHAWBLog                  
											 where CusHAWBLog.SL_Parent = ProcessQueueLog.SL_Parent
											 and CusHAWBLog.SL_SE_NKEvent = 'QUC'
											 and CusHAWBLog.SL_PostedTimeUtc > ProcessQueueLog.SL_PostedTimeUtc          
											 order by SL_PostedTimeUtc asc)
										  )       
		as RemarksOnQueueMovedTo,

		CS_HAWB as Housebill,
		substring(CS_RL_NKOrigin, 1, 2) 
		as CountryOfOrigin,
		case 
		  when CS_OA_ConsignorAddress is null then CS_ConsigneeName 
		  else OH_FullName
		end 
				as ConsignorName,
		isnull(OK_CustomsRegNo, CS_OtherSystemConsignorCode) as ConsignorAccountNumber
from dbo.StmALog ProcessQueueLog 
inner join dbo.ProcessQueue on SL_Parent = P4_PK
inner join dbo.CusHAWB ProcessQueueHAWB  on P4_ParentID = CS_PK
inner join dbo.CusMAWB  on CS_CM = CM_PK
left outer join dbo.OrgAddress  on CS_OA_ConsignorAddress = OA_PK
left outer join dbo.OrgHeader  on OA_OH = OH_PK
left outer join dbo.OrgCusCode  on OK_OH = OH_PK and OK_CodeType = 'UAN'
cross apply dbo.ClientIsBranchUPECustomised(CM_GB) AS Branch
where Branch.Enabled = 1
and SL_SE_NKEvent = 'QUC'
and SL_Reference like 'CUS^INV^%'
and (@Masterbill = '' or CM_MAWB = @Masterbill)
and (@DateFrom = '' or SL_PostedTimeUtc >= @DateFrom)
and (@DateTo = '' or SL_PostedTimeUtc < dateadd(day, 1, @DateTo))
)
".Replace("^", "\""),
					"DROP FUNCTION ClientUPEInterventionReport", DbRoutineType.SqlFunctionInlineTypeDesc);
			}
		}

		#endregion

		// To be deleted after UPS are finished testing the new report
		#region FUNCTION ClientUPEBPWReport OLD

		static DatabaseViewAndRoutineCreateScript ClientUPEBPWReportOLD
		{
			get
			{
				return new DatabaseViewAndRoutineCreateScript("ClientUPEBPWReportOLD", @"
CREATE FUNCTION ClientUPEBPWReportOLD(@DateFrom datetime, @DateTo datetime)
RETURNS TABLE
AS
RETURN
(
select
  (select count(*) 
	 from dbo.CusHAWB
	 inner join dbo.CusMAWB on CM_PK = CS_CM
	 inner join dbo.StmALog  on SL_Parent = CS_PK
	 inner join dbo.ProcessQueue  on CS_PK = P4_ParentID
	 cross apply dbo.ClientIsBranchUPECustomised(CM_GB) AS Branch
	 where Branch.Enabled = 1
	 and SL_SE_NKEvent = 'ADD'
	 and P4_CustomAttrib4 = '03'     
	 and SL_PostedTimeUtc >= @DateFrom
	 and SL_PostedTimeUtc < dateadd(day, 1, @DateTo)) 
  as AirCargoNonDocsCreatedCount,

  (select count(*) 
	 from dbo.CusHAWB
	 inner join dbo.CusMAWB on CM_PK = CS_CM
	 inner join dbo.StmALog  on SL_Parent = CS_PK
	 inner join dbo.ProcessQueue  on CS_PK = P4_ParentID
	 cross apply dbo.ClientIsBranchUPECustomised(CM_GB) AS Branch
	 where Branch.Enabled = 1
	 and SL_SE_NKEvent = 'ADD'
	 and P4_CustomAttrib4 = '03' 
	 and SL_PostedTimeUtc >= @DateFrom
	 and SL_PostedTimeUtc < dateadd(day, 1, @DateTo)
	 and exists (select 1
				   from dbo.StmALog 
				   where SL_Parent = CS_PK
				   and SL_SE_NKEvent = 'EDT'
				   and SL_GS_NKUser != '~BP'
				   and SL_PostedTimeUtc >= @DateFrom
				   and SL_PostedTimeUtc < dateadd(day, 1, @DateTo)))
  as AirCargoNonDocsWorkedCount,

  (select count(*) 
	 from dbo.JobDeclaration
	 inner join dbo.StmALog  on SL_Parent = JE_PK
	 cross apply dbo.ClientIsBranchUPECustomised(JE_GB) AS Branch
	 where Branch.Enabled = 1
	 and SL_SE_NKEvent = 'ADD'
	 and SL_PostedTimeUtc >= @DateFrom
	 and SL_PostedTimeUtc < dateadd(day, 1, @DateTo))
  as DeclarationCreatedCount,

  (select count(*) 
	 from dbo.JobDeclaration
	 inner join dbo.StmALog  on SL_Parent = JE_PK
	 cross apply dbo.ClientIsBranchUPECustomised(JE_GB) AS Branch
	 where Branch.Enabled = 1
	 and SL_SE_NKEvent = 'ADD'
	 and SL_PostedTimeUtc >= @DateFrom
	 and SL_PostedTimeUtc < dateadd(day, 1, @DateTo)
	 and exists (select 1
				   from dbo.StmALog 
				   where SL_Parent = JE_PK
				   and SL_SE_NKEvent = 'EDT'
				   and SL_GS_NKUser != '~BP'
				   and SL_PostedTimeUtc >= @DateFrom
				   and SL_PostedTimeUtc < dateadd(day, 1, @DateTo)))
  as DeclarationWorkedCount,

  (select count(*) 
	 from dbo.CusHAWB
	 inner join dbo.StmALog on SL_Parent = CS_PK
	 inner join dbo.CusMAWB on CS_CM = CM_PK
	 cross apply dbo.ClientIsBranchUPECustomised(CM_GB) AS Branch
	 where Branch.Enabled = 1
	 and SL_SE_NKEvent = 'ADD'
	 and exists (select 1 
				   from dbo.OrgPatternMatchAddress 
				   inner join dbo.OrgMatchApproval  on P2_ParentID = P3_PK
				   where P3_ParentID = CS_PK)
	 and SL_PostedTimeUtc >= @DateFrom
	 and SL_PostedTimeUtc < dateadd(day, 1, @DateTo))
  as MatchingQueueAddedCount,

  (select count(*) 
	 from dbo.CusHAWB
	 inner join dbo.StmALog on SL_Parent = CS_PK
	 inner join dbo.CusMAWB on CS_CM = CM_PK
	 cross apply dbo.ClientIsBranchUPECustomised(CM_GB) AS Branch
	 where Branch.Enabled = 1
	 and SL_SE_NKEvent = 'ADD'
	 and exists (select 1 
				   from dbo.OrgPatternMatchAddress 
				   inner join dbo.OrgMatchApproval  on P2_ParentID = P3_PK
				   where P3_ParentID = CS_PK
				   and P2_RelatedDateForPatternMatch >= @DateFrom
				   and P2_RelatedDateForPatternMatch < dateadd(day, 1, @DateTo))
	 and SL_PostedTimeUtc >= @DateFrom
	 and SL_PostedTimeUtc < dateadd(day, 1, @DateTo))
  as MatchingQueueWorkedCount,

  (select count(distinct CS_HAWB) 
	 from dbo.CusHAWB 
	 inner join dbo.CusMAWB on CM_PK = CS_CM
	 inner join dbo.ProcessQueue  on CS_PK = P4_ParentID
	 inner join dbo.StmALog  on SL_Parent = P4_PK
	 cross apply dbo.ClientIsBranchUPECustomised(CM_GB) AS Branch
	 where Branch.Enabled = 1
	 and SL_SE_NKEvent = 'QUC'
	 and (SL_Reference like 'CUS^INV^%' or SL_Reference like 'CUS^EIR^%' or SL_Reference like 'CUS^HLD^%')
	 and SL_PostedTimeUtc >= @DateFrom
	 and SL_PostedTimeUtc < dateadd(day, 1, @DateTo))
  as AirCargoQueuesHitCount,

  (select count(distinct CS_HAWB)
	 from dbo.CusHAWB 
	 inner join dbo.CusMAWB on CM_PK = CS_CM
	 inner join dbo.ProcessQueue  on CS_PK = P4_ParentID
	 inner join dbo.StmALog  on SL_Parent = P4_PK
	 cross apply dbo.ClientIsBranchUPECustomised(CM_GB) AS Branch
	 where Branch.Enabled = 1
	 and SL_SE_NKEvent = 'QUC'
	 and (SL_Reference like 'CUS^INV^%' or SL_Reference like 'CUS^EIR^%' or SL_Reference like 'CUS^HLD^%')
	 and SL_PostedTimeUtc >= @DateFrom
	 and SL_PostedTimeUtc < dateadd(day, 1, @DateTo)
	 and exists (select 1
				   from dbo.StmALog 
				   where SL_Parent = CS_PK
				   and SL_SE_NKEvent = 'EDT'
				   and SL_GS_NKUser != '~BP'
				   and SL_PostedTimeUtc >= @DateFrom
				   and SL_PostedTimeUtc < dateadd(day, 1, @DateTo)))
  as AirCargoQueuesWorkedCount,

  (select count(distinct JE_DeclarationReference) 
	 from dbo.JobDeclaration 
	 inner join dbo.ProcessQueue  on JE_PK = P4_ParentID
	 inner join dbo.StmALog  on SL_Parent = P4_PK
	 cross apply dbo.ClientIsBranchUPECustomised(JE_GB) AS Branch
	 where Branch.Enabled = 1
	 and SL_SE_NKEvent = 'QUC'
	 and (SL_Reference like 'CUS^EIR^%' or SL_Reference like 'CUS^HLD^,^B5^%' or SL_Reference like 'CUS^HLD^,^RU^%' or SL_Reference like 'CUS^HLD^,^RJ^%')
	 and SL_PostedTimeUtc >= @DateFrom
	 and SL_PostedTimeUtc < dateadd(day, 1, @DateTo))
  as DeclarationQueuesHitCount,

  (select count(distinct JE_DeclarationReference) 
	 from dbo.JobDeclaration 
	 inner join dbo.ProcessQueue  on JE_PK = P4_ParentID
	 inner join dbo.StmALog  on SL_Parent = P4_PK
	 cross apply dbo.ClientIsBranchUPECustomised(JE_GB) AS Branch
	 where Branch.Enabled = 1
	 and SL_SE_NKEvent = 'QUC'
	 and (SL_Reference like 'CUS^EIR^%' or SL_Reference like 'CUS^HLD^,^B5^%' or SL_Reference like 'CUS^HLD^,^RU^%' or SL_Reference like 'CUS^HLD^,^RJ^%')
	 and SL_PostedTimeUtc >= @DateFrom
	 and SL_PostedTimeUtc < dateadd(day, 1, @DateTo)
	 and exists (select 1
				   from dbo.StmALog 
				   where SL_Parent = JE_PK
				   and SL_SE_NKEvent = 'EDT'
				   and SL_GS_NKUser != '~BP'
				   and SL_PostedTimeUtc >= @DateFrom
				   and SL_PostedTimeUtc < dateadd(day, 1, @DateTo)))
  as DeclarationQueuesWorkedCount,

  (select count(*) 
	 from dbo.JobDeclaration 
	 cross apply dbo.ClientIsBranchUPECustomised(JE_GB) AS Branch
	 where Branch.Enabled = 1
	 and exists (select 1
					 from dbo.OrgHeader 
					 inner join dbo.StmALog on SL_Parent = OH_PK
					 where SL_PostedTimeUtc >= @DateFrom
					 and SL_PostedTimeUtc < dateadd(day, 1, @DateTo)
					 and JE_OH_Importer = OH_PK
					 and (SL_SE_NKEvent = 'ADD' or SL_SE_NKEvent = 'EDT')))
  as ImporterCreatedOrEditedCount,

  (select count(*) 
	 from dbo.JobDeclaration 
	 cross apply dbo.ClientIsBranchUPECustomised(JE_GB) AS Branch
	 where Branch.Enabled = 1
	 and exists (select 1
					 from dbo.OrgHeader 
					 inner join dbo.StmALog  on SL_Parent = OH_PK
					 where SL_PostedTimeUtc >= @DateFrom
					 and SL_PostedTimeUtc < dateadd(day, 1, @DateTo)
					 and JE_OH_Supplier = OH_PK
					 and (SL_SE_NKEvent = 'ADD' or SL_SE_NKEvent = 'EDT')))

  as SupplierCreatedOrEditedCount
)
".Replace("^", "\""),
					"DROP FUNCTION ClientUPEBPWReportOLD", DbRoutineType.SqlFunctionInlineTypeDesc);
			}
		}

		#endregion

		#region FUNCTION ClientUPEBPWReport

		static DatabaseViewAndRoutineCreateScript ClientUPEBPWReport
		{
			get
			{
				return new DatabaseViewAndRoutineCreateScript("ClientUPEBPWReport", @"
CREATE FUNCTION ClientUPEBPWReport(@DateFrom datetime, @DateTo datetime)
RETURNS TABLE
AS
RETURN
(
select
  (select count(*) 
	 from dbo.CusHAWB
	 inner join dbo.CusMAWB on CM_PK = CS_CM
	 inner join dbo.StmALog  on SL_Parent = CS_PK
	 inner join dbo.ProcessQueue  on CS_PK = P4_ParentID
	 cross apply dbo.ClientIsBranchUPECustomised(CM_GB) AS Branch
	 where Branch.Enabled = 1
	 and SL_SE_NKEvent = 'ADD'
	 and P4_CustomAttrib4 = '01'
	 and SL_PostedTimeUtc >= @DateFrom
	 and SL_PostedTimeUtc < dateadd(day, 1, @DateTo))
  as AirCargoLettersCreatedCount,

  (select count(*) 
	 from dbo.CusHAWB
	 inner join dbo.CusMAWB on CM_PK = CS_CM
	 inner join dbo.StmALog  on SL_Parent = CS_PK
	 inner join dbo.ProcessQueue  on CS_PK = P4_ParentID
	 cross apply dbo.ClientIsBranchUPECustomised(CM_GB) AS Branch
	 where Branch.Enabled = 1
	 and SL_SE_NKEvent = 'ADD'
	 and P4_CustomAttrib4 = '02'
	 and SL_PostedTimeUtc >= @DateFrom
	 and SL_PostedTimeUtc < dateadd(day, 1, @DateTo))
  as AirCargoDocsCreatedCount,

  (select count(*) 
	 from dbo.CusHAWB
	 inner join dbo.CusMAWB on CM_PK = CS_CM
	 inner join dbo.StmALog  on SL_Parent = CS_PK
	 inner join dbo.ProcessQueue  on CS_PK = P4_ParentID
	 cross apply dbo.ClientIsBranchUPECustomised(CM_GB) AS Branch
	 where Branch.Enabled = 1
	 and SL_SE_NKEvent = 'ADD'
	 and P4_CustomAttrib4 = '03'     
	 and SL_PostedTimeUtc >= @DateFrom
	 and SL_PostedTimeUtc < dateadd(day, 1, @DateTo)) 
  as AirCargoNonDocsCreatedCount,

  (select count(*) 
	 from dbo.JobDeclaration
	 cross apply dbo.ClientIsBranchUPECustomised(JE_GB) AS Branch
	 where Branch.Enabled = 1
	 and JE_MessageSubType = 'FRM'
	 and JE_SystemCreateTimeUtc >= @DateFrom
	 and JE_SystemCreateTimeUtc < dateadd(day, 1, @DateTo))
  as FRMDeclarationCreatedCount,

  (select count(*) 
	 from dbo.JobDeclaration
	 cross apply dbo.ClientIsBranchUPECustomised(JE_GB) AS Branch
	 where Branch.Enabled = 1
	 and JE_MessageSubType = 'SAC'
	 and JE_SystemCreateTimeUtc >= @DateFrom
	 and JE_SystemCreateTimeUtc < dateadd(day, 1, @DateTo))
  as SACDeclarationCreatedCount,

  (select count(*) 
	 from dbo.JobDeclaration
	 cross apply dbo.ClientIsBranchUPECustomised(JE_GB) AS Branch
	 where Branch.Enabled = 1
	 and JE_MessageSubType = 'SWL'
	 and JE_SystemCreateTimeUtc >= @DateFrom
	 and JE_SystemCreateTimeUtc < dateadd(day, 1, @DateTo))
  as SWLDeclarationCreatedCount,

  (select count(*) 
	 from dbo.CusHAWB
	 inner join dbo.StmALog  on SL_Parent = CS_PK
	 inner join dbo.CusMAWB on CS_CM = CM_PK
	 cross apply dbo.ClientIsBranchUPECustomised(CM_GB) AS Branch
	 where Branch.Enabled = 1
	 and SL_SE_NKEvent = 'ADD'
	 and exists (select 1 
				   from dbo.OrgPatternMatchAddress 
				   inner join dbo.OrgMatchApproval  on P2_ParentID = P3_PK
				   where P3_ParentID = CS_PK)
	 and SL_PostedTimeUtc >= @DateFrom
	 and SL_PostedTimeUtc < dateadd(day, 1, @DateTo))
  as MatchingQueueAddedCount,

  (select count(*) 
	 from dbo.CusHAWB
	 inner join dbo.StmALog  on SL_Parent = CS_PK
	 inner join dbo.CusMAWB on CS_CM = CM_PK
	 cross apply dbo.ClientIsBranchUPECustomised(CM_GB) AS Branch
	 where Branch.Enabled = 1
	 and SL_SE_NKEvent = 'ADD'
	 and exists (select 1 
				   from dbo.OrgPatternMatchAddress 
				   inner join dbo.OrgMatchApproval  on P2_ParentID = P3_PK
				   where P3_ParentID = CS_PK
				   and P2_RelatedDateForPatternMatch >= @DateFrom
				   and P2_RelatedDateForPatternMatch < dateadd(day, 1, @DateTo))
	 and SL_PostedTimeUtc >= @DateFrom
	 and SL_PostedTimeUtc < dateadd(day, 1, @DateTo))
  as MatchingQueueWorkedCount,

  (select count(distinct CS_HAWB) 
	 from dbo.CusHAWB 
	 inner join dbo.CusMAWB on CM_PK = CS_CM
	 inner join dbo.ProcessQueue  on CS_PK = P4_ParentID
	 inner join dbo.StmALog  on SL_Parent = P4_PK
	 cross apply dbo.ClientIsBranchUPECustomised(CM_GB) AS Branch
	 where Branch.Enabled = 1
	 and SL_SE_NKEvent = 'QUC'
	 and SL_Reference like 'CUS^INV^%'
	 and SL_PostedTimeUtc >= @DateFrom
	 and SL_PostedTimeUtc < dateadd(day, 1, @DateTo))
  as AirCargoInterventionQueuesHitCount,

  (select count(distinct CS_HAWB)
	 from dbo.CusHAWB 
	 inner join dbo.CusMAWB on CM_PK = CS_CM
	 inner join dbo.ProcessQueue  on CS_PK = P4_ParentID
	 inner join dbo.StmALog  on SL_Parent = P4_PK
	 cross apply dbo.ClientIsBranchUPECustomised(CM_GB) AS Branch
	 where Branch.Enabled = 1
	 and SL_SE_NKEvent = 'QUC'
	 and SL_Reference like 'CUS^INV^%'
	 and SL_PostedTimeUtc >= @DateFrom
	 and SL_PostedTimeUtc < dateadd(day, 1, @DateTo)
	 and exists (select 1
				   from dbo.StmALog 
				   where SL_Parent = CS_PK
				   and SL_SE_NKEvent = 'EDT'
				   and SL_GS_NKUser != '~BP'
				   and SL_PostedTimeUtc >= @DateFrom
				   and SL_PostedTimeUtc < dateadd(day, 1, @DateTo)))
  as AirCargoInterventionQueuesWorkedCount,

  (select count(distinct CS_HAWB) 
	 from dbo.CusHAWB
	 inner join dbo.CusMAWB on CM_PK = CS_CM
	 inner join dbo.ProcessQueue  on CS_PK = P4_ParentID
	 inner join dbo.StmALog  on SL_Parent = P4_PK
	 cross apply dbo.ClientIsBranchUPECustomised(CM_GB) AS Branch
	 where Branch.Enabled = 1
	 and SL_SE_NKEvent = 'QUC'
	 and SL_Reference like 'CUS^EIR^%'
	 and SL_PostedTimeUtc >= @DateFrom
	 and SL_PostedTimeUtc < dateadd(day, 1, @DateTo))
  as AirCargoEIRQueuesHitCount,

  (select count(distinct CS_HAWB)
	 from dbo.CusHAWB
	 inner join dbo.CusMAWB on CM_PK = CS_CM
	 inner join dbo.ProcessQueue  on CS_PK = P4_ParentID
	 inner join dbo.StmALog  on SL_Parent = P4_PK
	 cross apply dbo.ClientIsBranchUPECustomised(CM_GB) AS Branch
	 where Branch.Enabled = 1
	 and SL_SE_NKEvent = 'QUC'
	 and SL_Reference like 'CUS^EIR^%'
	 and SL_PostedTimeUtc >= @DateFrom
	 and SL_PostedTimeUtc < dateadd(day, 1, @DateTo)
	 and exists (select 1
				   from dbo.StmALog 
				   where SL_Parent = CS_PK
				   and SL_SE_NKEvent = 'EDT'
				   and SL_GS_NKUser != '~BP'
				   and SL_PostedTimeUtc >= @DateFrom
				   and SL_PostedTimeUtc < dateadd(day, 1, @DateTo)))
  as AirCargoEIRQueuesWorkedCount,

  (select count(distinct CS_HAWB) 
	 from dbo.CusHAWB
	 inner join dbo.CusMAWB on CM_PK = CS_CM
	 inner join dbo.ProcessQueue  on CS_PK = P4_ParentID
	 inner join dbo.StmALog  on SL_Parent = P4_PK
	 cross apply dbo.ClientIsBranchUPECustomised(CM_GB) AS Branch
	 where Branch.Enabled = 1
	 and SL_SE_NKEvent = 'QUC'
	 and SL_Reference like 'CUS^HLD^%'
	 and SL_PostedTimeUtc >= @DateFrom
	 and SL_PostedTimeUtc < dateadd(day, 1, @DateTo))
  as AirCargoHoldQueuesHitCount,

  (select count(distinct CS_HAWB)
	 from dbo.CusHAWB
	 inner join dbo.CusMAWB on CM_PK = CS_CM
	 inner join dbo.ProcessQueue  on CS_PK = P4_ParentID
	 inner join dbo.StmALog  on SL_Parent = P4_PK
	 cross apply dbo.ClientIsBranchUPECustomised(CM_GB) AS Branch
	 where Branch.Enabled = 1
	 and SL_SE_NKEvent = 'QUC'
	 and SL_Reference like 'CUS^HLD^%'
	 and SL_PostedTimeUtc >= @DateFrom
	 and SL_PostedTimeUtc < dateadd(day, 1, @DateTo)
	 and exists (select 1
				   from dbo.StmALog 
				   where SL_Parent = CS_PK
				   and SL_SE_NKEvent = 'EDT'
				   and SL_GS_NKUser != '~BP'
				   and SL_PostedTimeUtc >= @DateFrom
				   and SL_PostedTimeUtc < dateadd(day, 1, @DateTo)))
  as AirCargoHoldQueuesWorkedCount,

  (select count(distinct JE_DeclarationReference) 
	 from dbo.JobDeclaration 
	 inner join dbo.ProcessQueue  on JE_PK = P4_ParentID
	 inner join dbo.StmALog  on SL_Parent = P4_PK
	 cross apply dbo.ClientIsBranchUPECustomised(JE_GB) AS Branch
	 where Branch.Enabled = 1
	 and SL_SE_NKEvent = 'QUC'
	 and SL_Reference like 'CUS^EIR^%'
	 and SL_PostedTimeUtc >= @DateFrom
	 and SL_PostedTimeUtc < dateadd(day, 1, @DateTo))
  as DeclarationEIRQueuesHitCount,

  (select count(distinct JE_DeclarationReference) 
	 from dbo.JobDeclaration 
	 inner join dbo.ProcessQueue  on JE_PK = P4_ParentID
	 inner join dbo.StmALog  on SL_Parent = P4_PK
	 cross apply dbo.ClientIsBranchUPECustomised(JE_GB) AS Branch
	 where Branch.Enabled = 1
	 and SL_SE_NKEvent = 'QUC'
	 and SL_Reference like 'CUS^EIR^%'
	 and SL_PostedTimeUtc >= @DateFrom
	 and SL_PostedTimeUtc < dateadd(day, 1, @DateTo)
	 and exists (select 1
				   from dbo.StmALog 
				   where SL_Parent = JE_PK
				   and SL_SE_NKEvent = 'EDT'
				   and SL_GS_NKUser != '~BP'
				   and SL_PostedTimeUtc >= @DateFrom
				   and SL_PostedTimeUtc < dateadd(day, 1, @DateTo)))
  as DeclarationEIRQueuesWorkedCount,

  (select count(distinct JE_DeclarationReference) 
	 from dbo.JobDeclaration 
	 inner join dbo.ProcessQueue on JE_PK = P4_ParentID
	 inner join dbo.StmALog on SL_Parent = P4_PK
	 cross apply dbo.ClientIsBranchUPECustomised(JE_GB) AS Branch
	 where Branch.Enabled = 1
	 and SL_SE_NKEvent = 'QUC'
	 and (SL_Reference like 'CUS^HLD^,^B5^%' or SL_Reference like 'CUS^HLD^,^RU^%' or SL_Reference like 'CUS^HLD^,^RJ^%')
	 and SL_PostedTimeUtc >= @DateFrom
	 and SL_PostedTimeUtc < dateadd(day, 1, @DateTo))
  as DeclarationHold_B5_RU_RJ_QueuesHitCount,

  (select count(distinct JE_DeclarationReference) 
	 from dbo.JobDeclaration 
	 inner join dbo.ProcessQueue on JE_PK = P4_ParentID
	 inner join dbo.StmALog on SL_Parent = P4_PK
	 cross apply dbo.ClientIsBranchUPECustomised(JE_GB) AS Branch
	 where Branch.Enabled = 1
	 and SL_SE_NKEvent = 'QUC'
	 and (SL_Reference like 'CUS^HLD^,^B5^%' or SL_Reference like 'CUS^HLD^,^RU^%' or SL_Reference like 'CUS^HLD^,^RJ^%')
	 and SL_PostedTimeUtc >= @DateFrom
	 and SL_PostedTimeUtc < dateadd(day, 1, @DateTo)
	 and exists (select 1
				   from dbo.StmALog 
				   where SL_Parent = JE_PK
				   and SL_SE_NKEvent = 'EDT'
				   and SL_GS_NKUser != '~BP'
				   and SL_PostedTimeUtc >= @DateFrom
				   and SL_PostedTimeUtc < dateadd(day, 1, @DateTo)))
  as DeclarationHold_B5_RU_RJ_QueuesWorkedCount,

  (select count(*) 
	 from dbo.JobDeclaration 
	 cross apply dbo.ClientIsBranchUPECustomised(JE_GB) AS Branch
	 where Branch.Enabled = 1
	 and exists (select 1
					 from dbo.OrgHeader 
					 inner join dbo.StmALog on SL_Parent = OH_PK
					 where SL_PostedTimeUtc >= @DateFrom
					 and SL_PostedTimeUtc < dateadd(day, 1, @DateTo)
					 and JE_OH_Importer = OH_PK
					 and (SL_SE_NKEvent = 'ADD' or SL_SE_NKEvent = 'EDT')
				     and (SL_Reference like 'Registration No.%Type: GST' OR SL_Reference like 'Registration No.%Type: CID')))
  as ImporterCreatedOrEditedCount,

  (select count(*) 
	 from dbo.JobDeclaration 
	 cross apply dbo.ClientIsBranchUPECustomised(JE_GB) AS Branch
	 where Branch.Enabled = 1
	 and exists (select 1
					 from dbo.OrgHeader 
					 inner join dbo.StmALog on SL_Parent = OH_PK
					 where SL_PostedTimeUtc >= @DateFrom
					 and SL_PostedTimeUtc < dateadd(day, 1, @DateTo)
					 and JE_OH_Supplier = OH_PK
					 and (SL_SE_NKEvent = 'ADD' or SL_SE_NKEvent = 'EDT')
					 and (SL_Reference like 'Registration No.%Type: GST' OR SL_Reference like 'Registration No.%Type: CID')))
  as SupplierCreatedOrEditedCount
)".Replace("^", "\""),
					"DROP FUNCTION ClientUPEBPWReport", DbRoutineType.SqlFunctionInlineTypeDesc);
			}
		}

		#endregion

		#region FUNCTION ClientDataManagementLogReport

		static DatabaseViewAndRoutineCreateScript ClientDataManagementLogReport
		{
			get
			{
				return new DatabaseViewAndRoutineCreateScript("ClientDataManagementLogReport", @"
CREATE FUNCTION ClientDataManagementLogReport(@DateFrom datetime, @DateTo datetime)
RETURNS TABLE
AS
RETURN
(
  select ArrivalDate,
		 Level1FileNames,
		 Masterbill, 
		 Shipments, 
		 PiecesManifested, 
		 UnderbondSubmittedDate,
		 UnderbondApprovalDate,
		 IARReceived
  from 
	(select 
			dbo.ClientGetLevel1FileNames(CM_PK) as Level1FileNames,
			CM_MAWB as Masterbill, 
			CM_ArrivalDate as ArrivalDate,
			(select count(*)
			   from dbo.CusHAWB
			   where CS_CM = CM_PK)
			as Shipments,
   
			(select sum(CS_PiecesManifested)
			   from dbo.CusHAWB
			   where CS_CM = CM_PK)
			as PiecesManifested,

			(select top 1 EM_SystemCreateTimeUtc
			   from dbo.EDIMessage
			   inner join dbo.CusUnderbond on EM_LinkUniqueID = C4_PK
			   where EM_ReceiveTransmit = 'TRX'
			   and EM_MessageType = 'UBM'
			   and C4_ParentID = CM_PK
			   and EM_SystemCreateTimeUtc > dateadd(day, -7, @DateFrom)
			   order by EM_SystemCreateTimeUtc desc)
			 as UnderbondSubmittedDate,

			 (select top 1 EM_SystemCreateTimeUtc
			   from dbo.EDIMessage
			   inner join dbo.CusUnderbond on EM_LinkUniqueID = C4_PK
			   where EM_ReceiveTransmit = 'RCV'
			   and EM_MessageType = 'URR'
			   and EM_MessageText like '%UNDERBOND APPROVAL%'
			   and C4_ParentID = CM_PK
			   and EM_SystemCreateTimeUtc > dateadd(day, -7, @DateFrom)
			   order by EM_SystemCreateTimeUtc desc)
			 as UnderbondApprovalDate,

			 (select top 1 EM_SystemCreateTimeUtc
			   from dbo.EDIMessage
			   inner join dbo.CusHAWB on EM_LinkUniqueID = CS_PK          
			   where EM_ReceiveTransmit = 'RCV'
			   and EM_ApplicationCode = 'CMR'
			   and EM_MessageType = 'CRS'
			   and CS_CM = CM_PK
			   and EM_SystemCreateTimeUtc > dateadd(day, -7, @DateFrom)
			   order by EM_SystemCreateTimeUtc asc)
			 as IARReceived

	from dbo.CusMAWB
	CROSS APPLY dbo.ClientIsBranchUPECustomised(CM_GB) AS Branch
	where Branch.Enabled = 1
	and CM_ArrivalDate >= @DateFrom
	and CM_ArrivalDate < dateadd(day, 1, @DateTo)) InnerResult
)
",
					"DROP FUNCTION ClientDataManagementLogReport", DbRoutineType.SqlFunctionInlineTypeDesc);
			}
		}

		#endregion

		#region FUNCTION ClientShortlandsThatHaveNeverArrivedReport

		static DatabaseViewAndRoutineCreateScript ClientShortlandsThatHaveNeverArrivedReport
		{
			get
			{
				return new DatabaseViewAndRoutineCreateScript("ClientShortlandsThatHaveNeverArrivedReport", @"
CREATE FUNCTION ClientShortlandsThatHaveNeverArrivedReport(@DateFrom datetime, @DateTo datetime)
RETURNS TABLE
AS
RETURN
(
select CS_HAWB as Housebill, 
	   CS_PiecesManifested as PiecesManifested, 
	   (select sum(C5_PackagesOutturned)
		  from dbo.CusHAWB OutturnHAWB
		  inner join dbo.CusOutturn on C5_ParentID = OutturnHAWB.CS_PK
		  where OutturnHAWB.CS_HAWB = CusHAWB.CS_HAWB
		  and C5_IsDeleted = 0) as OuturnedInTotal

from dbo.CusMAWB CusMAWB
inner join dbo.CusHAWB CusHAWB on CusHAWB.CS_CM = CusMAWB.CM_PK
CROSS APPLY dbo.ClientIsBranchUPECustomised(CM_GB) AS Branch
where
Branch.Enabled = 1
and CusMAWB.CM_ApplicationCode = 'CMR'
and CusMAWB.CM_ArrivalDate >= @DateFrom
and CusMAWB.CM_ArrivalDate < dateadd(day, 1, @DateTo)
and CS_PiecesManifested > (select sum(C5_PackagesOutturned)
							 from dbo.CusHAWB OutturnHAWB 
							 inner join dbo.CusOutturn on C5_ParentID = OutturnHAWB.CS_PK
							 where OutturnHAWB.CS_HAWB = CusHAWB.CS_HAWB
							 and C5_IsDeleted = 0)
and exists (select 1 
			  from dbo.CusOutturn               
			  where C5_ParentID = CusHAWB.CS_PK
			  and C5_OutturnResultType = 'SH')
)
",
					"DROP FUNCTION ClientShortlandsThatHaveNeverArrivedReport", DbRoutineType.SqlFunctionInlineTypeDesc);
			}
		}

		#endregion

		#region FUNCTION ClientUPEGetScriptForMastersWithNoOuturnDocs

		static DatabaseViewAndRoutineCreateScript ClientUPEGetScriptForMastersWithNoOuturnDocs
		{
			get
			{
				return new DatabaseViewAndRoutineCreateScript("ClientUPEGetScriptForMastersWithNoOuturnDocs", @"
CREATE FUNCTION ClientUPEGetScriptForMastersWithNoOuturnDocs(@DateFrom datetime, @DateTo datetime)
RETURNS nvarchar(4000)
as
begin
  declare @sqlStatement nvarchar(4000)

  declare @statementPrefix nvarchar(4000)
  set @statementPrefix = 'select CM_MAWB, CM_ArrivalDate, SC_Date' +
						 ' from dbo.CusMAWB' +
						 ' inner join dbo.StorageMain on SM_ParentFK = CM_PK and SM_Type = ''ACG''' +
						 ' cross apply dbo.ClientIsBranchUPECustomised(CM_GB) AS Branch '

  declare @statementInnerJoin nvarchar(4000)

  declare @statementSuffix nvarchar(4000)
  set @statementSuffix = ' where Branch.Enabled = 1 and CM_ArrivalDate >= @dateFrom' +
						 ' and CM_ArrivalDate < dateadd(day, 1, @dateTo)'

  declare @databaseName nvarchar(100)

  declare docDatabaseCursor cursor read_only for 
	select db_name() + '_SD' + replicate('0', 3 - len (cast(SM_DB as varchar(3)))) + cast(SM_DB as varchar(3))
	  from dbo.CusMAWB    
	  inner join dbo.StorageMain on SM_ParentFK = CM_PK and SM_Type = 'ACG'
	  cross apply dbo.ClientIsBranchUPECustomised(CM_GB) AS Branch
	  where Branch.Enabled = 1
		and CM_ArrivalDate >= @dateFrom
		and CM_ArrivalDate < dateadd(day, 1, @dateTo)
		and SM_DB in (select convert(int, right(name, 3)) from sys.databases where name like db_name() + '_SD[0-9][0-9][0-9]')

  open docDatabaseCursor

  fetch next from docDatabaseCursor into @databaseName

  while @@fetch_status = 0
  begin
	set @statementInnerJoin = ' inner join ' + @databaseName + '.dbo.StorageDocs on SC_SM = SM_PK and SC_DocType = ''OUT'''
	set @sqlStatement = @statementPrefix + @statementInnerJoin + @statementSuffix + ' union all '

	fetch next from docDatabaseCursor into @databaseName
  end

  close docDatabaseCursor
  deallocate docDatabaseCursor

  if len(@statementInnerJoin) > 0
  begin
	set @sqlStatement = substring (@sqlStatement, 1, len(@sqlStatement) - 9)    
  end
  else set @sqlStatement = '';
  
  return(@sqlStatement)
end
",
					"DROP FUNCTION ClientUPEGetScriptForMastersWithNoOuturnDocs", DbRoutineType.SqlFunctionScalarTypeDesc);
			}
		}

		#endregion

		#region FUNCTION ClientGetLevel1FileNames

		static DatabaseViewAndRoutineCreateScript ClientGetLevel1FileNames
		{
			get
			{
				return new DatabaseViewAndRoutineCreateScript("ClientGetLevel1FileNames", @"
CREATE FUNCTION ClientGetLevel1FileNames(@CM_PK uniqueidentifier)
RETURNS varchar(1000)
AS
BEGIN
  declare @result varchar(1000)
  set @result = ''

  select @result = @result + SL_Reference + ', '
  from dbo.StmALog
  inner join dbo.CusMAWB on CM_PK = SL_Parent
  cross apply ClientIsBranchUPECustomised(CM_GB) AS Branch
  where Branch.Enabled = 1
  and SL_SE_NKEvent = 'DIM'
  and CM_PK = @CM_PK

  if (len(@result) > 1) set @result = substring(@result, 1, len(rtrim(@result)) - 1)
  
  return(@result)
END
",
					"DROP FUNCTION ClientGetLevel1FileNames", DbRoutineType.SqlFunctionScalarTypeDesc);
			}
		}

		#endregion

		#region FUNCTION ClientOrgRematchReport

		static DatabaseViewAndRoutineCreateScript ClientOrgRematchReport
		{
			get
			{
				return new DatabaseViewAndRoutineCreateScript("ClientOrgRematchReport", @"
CREATE FUNCTION ClientOrgRematchReport(@RematchStartDate datetime, @RematchEndDate datetime, @Organisations varchar(3))
RETURNS TABLE
AS
RETURN
(
SELECT
	JE_HouseBill AS MainHouseBill,
	'X' AS NoGrouping,
	T5_OrganisationType AS OrganisationType,
	RematchedFromOrg.OH_Code AS RematchedFromCode,
	RematchedFromOrg.OH_FullName AS RematchedFromName,
	RematchedToOrg.OH_Code AS RematchedToCode,
	RematchedToOrg.OH_FullName AS RematchedToName,
	RematchedBy.GS_FullName AS RematchedBy,
	T5_RematchedFromDate AS RematchedFromDate,
	T5_RematchedToDate AS RematchedToDate
FROM ClientOrgRematch 
INNER JOIN dbo.JobDeclaration Declaration  ON T5_JE=JE_PK
LEFT OUTER JOIN dbo.OrgHeader RematchedFromOrg  ON T5_OH_RematchedFromOrg=RematchedFromOrg.OH_PK
LEFT OUTER JOIN dbo.OrgHeader RematchedToOrg  ON T5_OH_RematchedToOrg=RematchedToOrg.OH_PK
INNER JOIN dbo.GlbStaff RematchedBy  ON T5_GS_RematchedBy=RematchedBy.GS_PK
CROSS APPLY ClientIsBranchUPECustomised(JE_GB) AS Branch
WHERE Branch.Enabled = 1 AND
	((@RematchStartDate = '' OR T5_RematchedToDate >= @RematchStartDate) AND (@RematchEndDate = '' OR T5_RematchedToDate <= dateadd(day, 1, @RematchEndDate)))
  AND (@Organisations='ALL' OR @Organisations=T5_OrganisationType)
)

", "DROP FUNCTION ClientOrgRematchReport", DbRoutineType.SqlFunctionInlineTypeDesc);
			}
		}

		#endregion

		#region FUNCTION ClientUPEREVReport
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1119:DoNotUseSLEventTimeTableColumn", Justification = "Baseline")]
		static DatabaseViewAndRoutineCreateScript ClientUPEREVReport
		{
			get
			{
				return new DatabaseViewAndRoutineCreateScript("ClientUPEREVReport", @"
CREATE FUNCTION ClientUPEREVReport(@DateFrom datetime, @DateTo datetime)
RETURNS TABLE
AS
RETURN
	SELECT
		SL_EventTime AS CreatedDate,
		CM_MAWB AS Masterbill,
		SUM(CASE WHEN P4_GS_NKCustomsTaskAssignedTo = 'REV' THEN 1 ELSE 0 END) AS TotalREV,
		SUM(CASE WHEN P4_GS_NKCustomsTaskAssignedTo <> 'REV' THEN 1 ELSE 0 END) AS TotalOther
	FROM
		dbo.JobDeclaration JobDec
		INNER JOIN dbo.CusMAWB ON CM_MAWB = JE_MasterBill
		INNER JOIN dbo.StmALog CusMAWBLog ON SL_Parent = CM_PK 
			AND CusMAWBLog.SL_SE_NKEvent = 'ADD'
			AND CusMAWBLog.SL_PostedTimeUtc >= @DateFrom 
			AND CusMAWBLog.SL_PostedTimeUtc < dateadd(day, 1, @DateTo)
		INNER JOIN dbo.ProcessQueue ON P4_ParentID = JobDec.JE_PK
		INNER JOIN
		(
			SELECT SL_Parent, MIN(SL_PostedTimeUtc) AS FirstPostedTime
			FROM dbo.StmALog ProcessQueueLog
			WHERE SL_Reference LIKE 'CUS^CLS%'
			AND SL_SE_NKEvent = 'QUC'
			GROUP BY SL_Parent
		) ProcessQueueLog ON ProcessQueueLog.SL_Parent = P4_PK
		CROSS APPLY ClientIsBranchUPECustomised(JE_GB) AS Branch
	WHERE
		Branch.Enabled = 1 
	GROUP BY 
		CusMAWBLog.SL_EventTime, CM_MAWB ".Replace("^", "\""),
		"DROP FUNCTION ClientUPEREVReport", DbRoutineType.SqlFunctionInlineTypeDesc);
			}
		}

		#endregion

		#region FUNCTION ClientOrgJobNumbers

		static DatabaseViewAndRoutineCreateScript ClientOrgJobNumbers
		{
			get
			{
				return new DatabaseViewAndRoutineCreateScript("ClientOrgJobNumbers", ZString.Format(
@"
CREATE FUNCTION ClientOrgJobNumbers(@Organisation uniqueidentifier)
RETURNS TABLE
AS 
RETURN(
	SELECT
		JE_DeclarationReference AS JobNumber		
	FROM
		dbo.JobDeclaration JobDec
		INNER JOIN dbo.ProcessQueue ON P4_ParentID = JobDec.JE_PK
			AND ProcessQueue.P4_CustomsQueue <> '{0}'
		CROSS APPLY ClientIsBranchUPECustomised(JE_GB) AS Branch
		WHERE Branch.Enabled = 1 AND (JE_OH_Importer = @Organisation AND JE_IsCancelled = 0)
)
", CommercialQueueCodeDescriptionPairList.Codes.Completed), "DROP FUNCTION ClientOrgJobNumbers", DbRoutineType.SqlFunctionInlineTypeDesc);
			}
		}

		#endregion

		#region FUNCTION Client_UPE_GetRegistryItem

		static DatabaseViewAndRoutineCreateScript Client_UPE_GetRegistryItem
		{
			get
			{
				return new DatabaseViewAndRoutineCreateScript("Client_UPE_GetRegistryItem", ZString.Format(
@"
CREATE FUNCTION Client_UPE_GetRegistryItem(@Owner AS UNIQUEIDENTIFIER, @RegistryKey AS VARCHAR(50))
RETURNS TABLE
AS
RETURN
(
	 SELECT 
			convert(NVARCHAR(max), convert(VARBINARY(max), RegistryItem.SD_BinaryValue)) AS Value 
	 FROM 
			dbo.StmData as RegistryItem 
	 WHERE
			RegistryItem.SD_Name = @RegistryKey AND ((@Owner IS NULL AND SD_Owner IS NULL) OR (@Owner IS NOT NULL AND SD_Owner = @Owner))
)
"), "DROP FUNCTION Client_UPE_GetRegistryItem", DbRoutineType.SqlFunctionInlineTypeDesc);
			}
		}

		#endregion

		#region FUNCTION Client_UPE_CheckPostcodeIsInZoneName

		static DatabaseViewAndRoutineCreateScript Client_UPE_CheckPostcodeIsInZoneName
		{
			get
			{
				return new DatabaseViewAndRoutineCreateScript("Client_UPE_CheckPostcodeIsInZoneName", ZString.Format(
@"
CREATE FUNCTION Client_UPE_CheckPostcodeIsInZoneName(@Postcode AS VARCHAR(5), @ZoneName AS VARCHAR(200))
RETURNS BIT
AS
BEGIN
	DECLARE @Return BIT
	SELECT @Return = 
	(
		CASE WHEN (
			SELECT 
				COUNT(*)
			FROM  
				dbo.RateTransportZones
				INNER JOIN dbo.RateTransportZoneItem on TQ_TZ_DomesticZone=TZ_PK
				INNER JOIN dbo.RateTransportProvider ON TP_PK=TZ_TP
			WHERE
				(@ZoneName = '" + UPETransportZonesCodeDescriptionPairProvider.AllMetro + @"' OR TZ_ZoneName = @ZoneName)
				AND TP_OH_RelatedParty = (select TOP 1 * from Client_UPE_GetRegistryItem(null, '" + UPEDataRegistry.Instance.CODManifestReportZoneRelatedPartyItem.Name + @"')) 
				AND (@Postcode BETWEEN TQ_FromPostCode AND TQ_ToPostCode)
			) > 0 THEN 1 ELSE 0 
		END
	)
	RETURN @Return
END
"), "DROP FUNCTION Client_UPE_CheckPostcodeIsInZoneName", DbRoutineType.SqlFunctionScalarTypeDesc);
			}
		}

		#endregion

		#region FUNCTION Client_UPE_CODManifestReport

		static DatabaseViewAndRoutineCreateScript Client_UPE_CODManifestReport
		{
			get
			{
				return new DatabaseViewAndRoutineCreateScript("Client_UPE_CODManifestReport", ZString.Format(
@"
CREATE FUNCTION Client_UPE_CODManifestReport(@FromDate AS DATETIME, @ToDate AS DATETIME, @ZoneName as varchar(100), @ReleasedMethod as varchar(100))
RETURNS TABLE
AS
RETURN
(
SELECT 
	*
FROM
	(
	SELECT 
		P4_CustomDate4 AS ReleaseDate,
		P4_CustomAttrib1 AS InvoiceNumber,
		CS_HAWB AS ShipmentNumber,
		ISNULL(Consignee.OH_FullName, CS_ConsigneeName) as ConsigneeName,
		CAST(CS_ConsigneePostcode AS INT) AS PostCode,
		
		CASE WHEN (OJ_Code = '10'
			AND P4_CustomDecimal4 <= CONVERT(money, (select * from Client_UPE_GetRegistryItem(null, '" + UPEDataRegistry.Instance.CODAutoReleaseAndChaseThresholdItem.Name + @"')))) 
			AND dbo.Client_UPE_CheckPostcodeIsInZoneName(CS_ConsigneePostcode, @ZoneName) > 0
		THEN 'Automatic' 
		ELSE 'Manual' END AS ReleasedMethod,
		
		P4_CustomDecimal4 AS InvoiceTotal,
		ST_NoteText AS ToBeCollected
	from
		(
		select 
				P4_ParentID, P4_CustomAttrib2,P4_CustomDecimal2,P4_CustomDecimal4,P4_CustomDate4,P4_CustomAttrib1
		from 
				dbo.StmALog
				JOIN dbo.ProcessQueue on SL_Parent = P4_PK 
		where 
				SL_Table = '" + ProcessQueueSchema.Constants.TableName + @"' 
				AND (SL_Reference like '%"",""" + ResolutionCodeDescriptionPairList.Codes.DA_Released + @"""%' OR SL_Reference like '%"",""" + ResolutionCodeDescriptionPairList.Codes.B7_SeizedByCustoms + @"""%' OR SL_Reference like '%"",""" + ResolutionCodeDescriptionPairList.Codes.BZ_Abandoned + @"""%' OR SL_Reference like '%"",""" + ResolutionCodeDescriptionPairList.Codes.BU_ReleasedByAlternateBroker + @"""%' )
				AND P4_CustomDate4 BETWEEN @FromDate AND @ToDate
		group by 
				P4_ParentID, P4_CustomAttrib2, SL_PostedTimeUtc,P4_CustomDecimal2,P4_CustomDecimal4,P4_CustomDate4,P4_CustomAttrib1
		) as Logs
		LEFT JOIN dbo.CusHAWB on P4_ParentID = CS_PK
		INNER JOIN dbo.CusMAWB ON CM_PK = CS_CM 
		CROSS APPLY dbo.ClientIsBranchUPECustomised(CM_GB) AS Branch
		LEFT JOIN dbo.OrgAddress ConsigneeAddress ON CS_OA_ConsigneeAddress = ConsigneeAddress.OA_PK
		LEFT JOIN 
		(
			SELECT 
				* 
			FROM 
				dbo.OrgHeader
		) Consignee ON ConsigneeAddress.OA_OH = Consignee.OH_PK  

		LEFT JOIN
		(
			SELECT 
				*
			FROM
				dbo.OrgHeader 
				LEFT JOIN dbo.OrgCompanyData ON OB_OH = OH_PK
				LEFT JOIN dbo.OrgDebtorGroup ON OJ_PK = OB_OJ_ARDebtorGroup
				LEFT JOIN dbo.OrgCusCode ON OH_PK = OK_OH
		) BillTo ON BillTo.OK_CustomsRegNo = P4_CustomAttrib2
		
		LEFT JOIN dbo.StmNote ON ST_ParentID = CS_PK AND ST_Description = 'Import Delivery Instructions'
	where
		Branch.Enabled = 1 
	and 
		P4_CustomDecimal2 = " + (int)UPECargoPaymentMethod.Cheque + @" -- Cheque payment
		AND (@ZoneName = '" + UPETransportZonesCodeDescriptionPairProvider.AllMetro + @"' OR dbo.Client_UPE_CheckPostcodeIsInZoneName(CS_ConsigneePostcode, @ZoneName) > 0)      

	) AS RESULTS
WHERE
	@ReleasedMethod = 'All' OR ReleasedMethod = @ReleasedMethod
)
"), "DROP FUNCTION Client_UPE_CODManifestReport", DbRoutineType.SqlFunctionInlineTypeDesc);
			}
		}

		#endregion

		#region STORED PROC ClientMissingOutturnReport

		static DatabaseViewAndRoutineCreateScript ClientMissingOutturnReport
		{
			get
			{
				return new DatabaseViewAndRoutineCreateScript("ClientMissingOutturnReport", @"
CREATE PROCEDURE ClientMissingOutturnReport
  @DateFrom datetime, 
  @DateTo datetime, 
  @FinalisationStatus char(3)
AS
begin
  create table #MasterBillsForOPS
  (
	Masterbill varchar(20) collate DATABASE_DEFAULT not null,
	ArrivalDate datetime,
	DateFinalised datetime
  )

  declare @ParmDefinition nvarchar(100)
  set @ParmDefinition = N'@dateFrom datetime, @dateTo datetime'

  declare @sqlStatement nvarchar(4000)

  select @sqlStatement = dbo.ClientUPEGetScriptForMastersWithNoOuturnDocs (@dateFrom, @dateTo)

  insert into #MasterBillsForOPS
	execute sp_executesql @sqlStatement, @ParmDefinition, @dateFrom, @dateTo

  create table #MasterBills
  (
	Masterbill varchar(20) collate DATABASE_DEFAULT not null,
	ArrivalDate datetime,
	DateFinalised datetime
  )
  
  insert into #MasterBills
	select CM_MAWB, 
		   CM_ArrivalDate, 
		   (select max(C4_Outurned)
				  from dbo.CusUnderbond
				  where C4_ParentID = CM_PK)
	from dbo.CusMAWB
	cross apply dbo.ClientIsBranchUPECustomised(CM_GB) AS Branch
	where Branch.Enabled = 1
	and CM_ApplicationCode = 'CMR'
	and CM_ArrivalDate >= @dateFrom
	and CM_ArrivalDate < dateadd(day, 1, @dateTo)

  select Masterbills.Masterbill,
		 Masterbills.ArrivalDate,
		 Masterbills.DateFinalised as BRKDateFinalised,
		 OPS.DateFinalised as OPSDateFinalised,
		 (select sum(C5_PackagesOutturned)
			from dbo.CusHAWB
			inner join dbo.CusMAWB on CM_MAWB = isnull(Masterbills.Masterbill, OPS.Masterbill)
			inner join dbo.CusOutturn on C5_ParentID = CS_PK
			where C5_IsDeleted = 0
			and C5_OutturnResultType = 'SU'
			and CS_CM = CM_PK)
		 as SurplusConsignementTotalPackageCount,

		 (select sum(C5_PackagesOutturned)
			from dbo.CusHAWB
			inner join dbo.CusMAWB on CM_MAWB = isnull(Masterbills.Masterbill, OPS.Masterbill)
			inner join dbo.CusOutturn on C5_ParentID = CS_PK
			where C5_IsDeleted = 0
			and C5_OutturnResultType = 'SH'
			and CS_CM = CM_PK)
		 as ShortLandedTotalPackageCount
  from #MasterBills Masterbills
  left outer join #MasterBillsForOPS OPS on Masterbills.Masterbill = OPS.Masterbill
  where (@FinalisationStatus = 'BTH' and Masterbills.DateFinalised is not null and OPS.DateFinalised is not null)
  or (@FinalisationStatus = 'NON' and Masterbills.DateFinalised is null and OPS.DateFinalised is null)
  or (@FinalisationStatus = 'BRK' and Masterbills.DateFinalised is not null)
  or (@FinalisationStatus = 'BRN' and Masterbills.DateFinalised is null)
  or (@FinalisationStatus = 'OPS' and OPS.DateFinalised is not null)
  or (@FinalisationStatus = 'OPN' and OPS.DateFinalised is null)
  order by Masterbills.ArrivalDate
end
", "DROP PROCEDURE ClientMissingOutturnReport", DbRoutineType.SqlProcedureTypeDesc);
			}
		}

		#endregion

		#region VIEW vw_Report_ClientAirwaybillStatusReport

		static DatabaseViewAndRoutineCreateScript vw_Report_ClientAirwaybillStatusReport
		{
			get
			{
				return new DatabaseViewAndRoutineCreateScript("vw_Report_ClientAirwaybillStatusReport", @"
CREATE VIEW vw_Report_ClientAirwaybillStatusReport
AS
SELECT  CusMAWB.CM_MAWB AS MAWB, 
		CusMAWB.CM_ArrivalDate AS Arrival, 
		CusHAWB.CS_HAWB AS HAWB, 
		CusHAWB.CS_RL_NKDestination AS Location, 
		(CASE WHEN CusHAWB.CS_WeightUQ = 'KG' THEN CusHAWB.CS_Weight 
			ELSE (SELECT Value FROM dbo.ConvertWeight(CusHAWB.CS_Weight, CusHAWB.CS_WeightUQ, 'KG')) END) AS Weight,
		CusHAWB.CS_WeightUQ AS WeightUQ, 
		(CASE
			WHEN CusHAWB.CS_IsResponsePending = 1 THEN 'WAIT'
			ELSE CusHAWB.CS_CustomsStatus
		END) AS Status,
		(CASE WHEN CusHAWB.CS_CustomsStatus='C150' THEN 'C000'
			ELSE CusHAWB.CS_CustomsStatus END) AS StatusWithC150RenamedToC000,
		(CASE WHEN CusHAWB.CS_OA_ConsigneeAddress IS NULL THEN CusHAWB.CS_ConsigneeName 
			ELSE Consignee.OH_FullName END) AS Consignee, 
		CusHAWB.CS_ConsigneePostcode AS PostCode, 
		JobDeclaration.JE_DeclarationReference AS Brokerage, 
		QueueAssignedTo.GS_Code AS Broker, 
		(CASE WHEN CusHAWB.CS_OA_ConsignorAddress IS NULL THEN CusHAWB.CS_ConsignorName
			ELSE Consignor.OH_FullName END) AS Shipper, 
		CusHAWB.CS_PiecesLanded AS Pieces
FROM    dbo.CusMAWB 
		INNER JOIN dbo.CusHAWB ON CusMAWB.CM_PK = CusHAWB.CS_CM 
		LEFT OUTER JOIN dbo.ProcessQueue ON CS_PK=P4_ParentID
		LEFT OUTER JOIN dbo.OrgAddress ConsigneeAddress ON CusHAWB.CS_OA_ConsigneeAddress = ConsigneeAddress.OA_PK
		LEFT OUTER JOIN dbo.OrgHeader Consignee ON ConsigneeAddress.OA_OH = Consignee.OH_PK
		LEFT OUTER JOIN dbo.OrgAddress ConsignorAddress ON CusHAWB.CS_OA_ConsignorAddress = ConsignorAddress.OA_PK
		LEFT OUTER JOIN dbo.OrgHeader Consignor ON ConsignorAddress.OA_OH = Consignor.OH_PK 
		LEFT OUTER JOIN dbo.JobDeclaration ON CusHAWB.CS_JE_CustomsFormalEntry = JobDeclaration.JE_PK 
		LEFT OUTER JOIN dbo.GlbStaff QueueAssignedTo ON P4_GS_NKCustomsTaskAssignedTo = QueueAssignedTo.GS_Code
		CROSS APPLY ClientIsBranchUPECustomised(CM_GB) AS Branch
		WHERE Branch.Enabled = 1 ",
					"DROP VIEW vw_Report_ClientAirwaybillStatusReport", DbRoutineType.SqlViewTypeDesc);
			}
		}

		#endregion

		#region FUNCTION ClientIsBranchUPECustomised

		static DatabaseViewAndRoutineCreateScript ClientIsBranchUPECustomised
		{
			get
			{
				return new DatabaseViewAndRoutineCreateScript("ClientIsBranchUPECustomised", ZString.Format(@"
			CREATE FUNCTION ClientIsBranchUPECustomised(@BranchPK AS UNIQUEIDENTIFIER)  
	RETURNS TABLE
		AS
		RETURN
		SELECT TOP 1 (
			CASE WHEN (
			SELECT
				COUNT(*)
			FROM
			dbo.GlbBranch
			JOIN dbo.StmData ON SD_Owner = GB_GC
			WHERE
				GB_PK = @BranchPK AND
				SD_Name = 'EnableUPECustomisations' AND
				CONVERT(NVARCHAR(400), CONVERT(VARBINARY(8000), SD_BinaryValue)) = 'True'
        ) > 0
		THEN  
			CAST (1 AS BIT)
		ELSE  
			CAST (0 AS BIT)
		END
      ) AS Enabled  "), "DROP FUNCTION ClientIsBranchUPECustomised", DbRoutineType.SqlFunctionInlineTypeDesc);
			}
		}

		#endregion

		#endregion
	}
}
