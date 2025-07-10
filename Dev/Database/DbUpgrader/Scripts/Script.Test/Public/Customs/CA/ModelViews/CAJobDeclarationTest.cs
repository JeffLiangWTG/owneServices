using CargoWise.Data;
using NUnit.Framework;
using static System.Data.SqlDbType;

namespace Enterprise.Build.Database.Script.Testing.Public.Customs.CA.ModelViews
{
	[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.Customs.CA.ModelViews.CAJobDeclaration))]
	class CAJobDeclarationTest : DbCreateScriptTest
	{
		public void TestViewColumns()
		{
			TestDbViewHelper.AssertViewColumns(
				Db.Connection,
				"CAJobDeclaration",
				"JobDeclaration",
				new[]
				{
					new TestDbViewHelper.DbColumn("JE_PK", UniqueIdentifier, -1),
					new TestDbViewHelper.DbColumn("JE_ClusterKey", Int, -1, 10, 0),
					new TestDbViewHelper.DbColumn("JE_AssesmentOption", VarChar, 1),
					new TestDbViewHelper.DbColumn("JE_BondNo", VarChar, 35),
					new TestDbViewHelper.DbColumn("JE_BondType", VarChar, 1),
					new TestDbViewHelper.DbColumn("JE_CarrierName", VarChar, 255),
					new TestDbViewHelper.DbColumn("JE_EstReleaseDate", DateTime, -1),
					new TestDbViewHelper.DbColumn("JE_InspectionArrangementsComplete", Bit, -1),
					new TestDbViewHelper.DbColumn("JE_MergeBy", VarChar, 3),
					new TestDbViewHelper.DbColumn("JE_OGDCFIA", Bit, -1),
					new TestDbViewHelper.DbColumn("JE_OGDIC", Bit, -1),
					new TestDbViewHelper.DbColumn("JE_OGDNR", Bit, -1),
					new TestDbViewHelper.DbColumn("JE_OGDTC", Bit, -1),
					new TestDbViewHelper.DbColumn("JE_CSAEntry", Bit, -1),
					new TestDbViewHelper.DbColumn("JE_PermitApplication", Bit, -1),
					new TestDbViewHelper.DbColumn("JE_PlaceOfReport", VarChar, 4),
					new TestDbViewHelper.DbColumn("JE_PortOfExit", VarChar, 4),
					new TestDbViewHelper.DbColumn("JE_ProvinceOfClearance", VarChar, 2),
					new TestDbViewHelper.DbColumn("JE_UnladingOffice", VarChar, 4),
					new TestDbViewHelper.DbColumn("JE_PriorityInd", VarChar, 1),
					new TestDbViewHelper.DbColumn("JE_ReasonForExportCode", VarChar, 2),
					new TestDbViewHelper.DbColumn("JE_WoodPackagingInd", Bit, -1),
					new TestDbViewHelper.DbColumn("JE_K84AccountingDate", DateTime, -1),
					new TestDbViewHelper.DbColumn("JE_K84StatementDate", DateTime, -1),
					new TestDbViewHelper.DbColumn("JE_RX_DeclaredCurr", UniqueIdentifier, -1),
					new TestDbViewHelper.DbColumn("JE_ServiceOption", VarChar, 3),
					new TestDbViewHelper.DbColumn("JE_SubLocationName", VarChar, 255),
					new TestDbViewHelper.DbColumn("JE_SuretyCode", VarChar, 3),
					new TestDbViewHelper.DbColumn("JE_TransportDocumentNumber", VarChar, 35),
					new TestDbViewHelper.DbColumn("JE_NetWeight", Decimal, -1, 9, 3),
					new TestDbViewHelper.DbColumn("JE_NetWeightUQ", VarChar, 2),
					new TestDbViewHelper.DbColumn("JE_ReleaseOffice", VarChar, 4),
					new TestDbViewHelper.DbColumn("JE_UseImporterAccountSecurityNumber", Bit, -1),
					new TestDbViewHelper.DbColumn("JE_LVSCloseDate", DateTime, -1),
					new TestDbViewHelper.DbColumn("JE_B2Type", VarChar, 2),
					new TestDbViewHelper.DbColumn("JE_SecurityNo", VarChar, 20),
					new TestDbViewHelper.DbColumn("JE_IsDocAttached", Bit, -1),
					new TestDbViewHelper.DbColumn("JE_OriginalTransactionNo", VarChar, 14),
					new TestDbViewHelper.DbColumn("JE_JustificationForRequest", VarChar, 100),
					new TestDbViewHelper.DbColumn("JE_Under", VarChar, 50),
					new TestDbViewHelper.DbColumn("JE_ClaimedInterestAmount", Decimal, -1, 9, 2),
					new TestDbViewHelper.DbColumn("JE_AnySightDepositAmount", Decimal, -1, 9, 2),
					new TestDbViewHelper.DbColumn("JE_AmendReasonCode", VarChar, 2),
					new TestDbViewHelper.DbColumn("JE_ATDExCode", VarChar, 6),
					new TestDbViewHelper.DbColumn("JE_ExamLocationCode", VarChar, 4),
					new TestDbViewHelper.DbColumn("JE_ExamLocationName", VarChar, 35),
					new TestDbViewHelper.DbColumn("JE_RequiresMerge", Bit, -1),
					new TestDbViewHelper.DbColumn("JE_B3AutoSend", Bit, -1),
					new TestDbViewHelper.DbColumn("JE_OGDStatus", VarChar, 3),
					new TestDbViewHelper.DbColumn("JE_AccountingAge", Int, -1, 10, 0),
					new TestDbViewHelper.DbColumn("JE_EstimatedPaymentDueDate", DateTime, -1),
					new TestDbViewHelper.DbColumn("JE_JobReadyForPost", Bit, -1),
					new TestDbViewHelper.DbColumn("JE_Version", Int, -1, 10, 0),
					new TestDbViewHelper.DbColumn("JE_JE_PreviousJob", UniqueIdentifier, -1),
					new TestDbViewHelper.DbColumn("JE_B2SubmissionDate", DateTime, -1),
					new TestDbViewHelper.DbColumn("JE_B2AcceptedDate", DateTime, -1),
					new TestDbViewHelper.DbColumn("JE_IsOurFault", Bit, -1),
					new TestDbViewHelper.DbColumn("JE_InitiatedBy", VarChar, 3),
					new TestDbViewHelper.DbColumn("JE_ChequeNo", VarChar, 30),
					new TestDbViewHelper.DbColumn("JE_ChequeDate", DateTime, -1),
					new TestDbViewHelper.DbColumn("JE_B2Total", Decimal, -1, 15, 2),
					new TestDbViewHelper.DbColumn("JE_DeclarationException", VarChar, 3),
					new TestDbViewHelper.DbColumn("JE_ConfirmedDate", DateTime, -1),
					new TestDbViewHelper.DbColumn("JE_AmendmentTo", VarChar, 3),
					new TestDbViewHelper.DbColumn("JE_OriginalAccountingDate", DateTime, -1),
					new TestDbViewHelper.DbColumn("JE_AllowOIC", Bit, -1)
				}
			);
		}

		public void TestViewIndexes()
		{
			TestDbViewHelper.AssertViewIndexes(
				Db.Connection,
				"CAJobDeclaration_Idx",
				new[]
				{
					new TestDbViewHelper.DbIndex("NR_UC__JE_ClusterKey_JE_PK", "JE_ClusterKey,JE_PK"),
					new TestDbViewHelper.DbIndex("NR_UX__JE_ProvinceOfClearance", "JE_ProvinceOfClearance"),
					new TestDbViewHelper.DbIndex("NR_UX__JE_LVSCloseDate", "JE_LVSCloseDate"),
					new TestDbViewHelper.DbIndex("NR_UX__JE_AllowOIC", "JE_AllowOIC"),
					new TestDbViewHelper.DbIndex("NR_UX__JE_PlaceOfReport", "JE_PlaceOfReport"),
					new TestDbViewHelper.DbIndex("NR_UX__JE_PortOfExit", "JE_PortOfExit"),
					new TestDbViewHelper.DbIndex("NR_UX__JE_UnladingOffice", "JE_UnladingOffice"),
					new TestDbViewHelper.DbIndex("NR_UX__JE_K84AccountingDate", "JE_K84AccountingDate"),
					new TestDbViewHelper.DbIndex("NR_UX__JE_K84StatementDate", "JE_K84StatementDate"),
					new TestDbViewHelper.DbIndex("NR_UX__JE_ServiceOption", "JE_ServiceOption"),
					new TestDbViewHelper.DbIndex("NR_UX__JE_B2Type", "JE_B2Type"),
					new TestDbViewHelper.DbIndex("NR_UX__JE_OriginalTransactionNo", "JE_OriginalTransactionNo"),
					new TestDbViewHelper.DbIndex("NR_UX__JE_OGDStatus", "JE_OGDStatus"),
					new TestDbViewHelper.DbIndex("NR_UX__JE_AccountingAge", "JE_AccountingAge"),
					new TestDbViewHelper.DbIndex("NR_UX__JE_EstimatedPaymentDueDate", "JE_EstimatedPaymentDueDate"),
					new TestDbViewHelper.DbIndex("NR_UX__JE_B2SubmissionDate", "JE_B2SubmissionDate"),
					new TestDbViewHelper.DbIndex("NR_UX__JE_B2AcceptedDate", "JE_B2AcceptedDate"),
					new TestDbViewHelper.DbIndex("NR_UX__JE_DeclarationException", "JE_DeclarationException"),
					new TestDbViewHelper.DbIndex("NR_UX__JE_ConfirmedDate", "JE_ConfirmedDate"),
					new TestDbViewHelper.DbIndex("NR_UX__JE_AssesmentOption", "JE_AssesmentOption"),
					new TestDbViewHelper.DbIndex("NR_UX__JE_ReleaseOffice", "JE_ReleaseOffice"),
					new TestDbViewHelper.DbIndex("NR_UX__JE_CSAEntry", "JE_CSAEntry")
				}
			);
		}
	}
}
