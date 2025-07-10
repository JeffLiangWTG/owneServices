using CargoWise.Data;
using NUnit.Framework;
using static System.Data.SqlDbType;

namespace Enterprise.Build.Database.Script.Testing.Public.Customs.CA.ModelViews
{
	[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.Customs.CA.ModelViews.CAJobComInvoiceHeader))]
	class CAJobComInvoiceHeaderTest : DbCreateScriptTest
	{
		public void TestViewColumns()
		{
			TestDbViewHelper.AssertViewColumns(
				Db.Connection,
				"CAJobComInvoiceHeader",
				"JobComInvoiceHeader",
				new[]
				{
					new TestDbViewHelper.DbColumn("JZ_PK", UniqueIdentifier, -1),
					new TestDbViewHelper.DbColumn("JZ_ClusterKey", Int, -1, 10, 0),
					new TestDbViewHelper.DbColumn("JZ_CasualImportDestinationProvince", VarChar, 2),
					new TestDbViewHelper.DbColumn("JZ_CasualImportCommodity", VarChar, 25),
					new TestDbViewHelper.DbColumn("JZ_ConditionsOfSale", VarChar, 35),
					new TestDbViewHelper.DbColumn("JZ_DepartmentRuling", VarChar, 25),
					new TestDbViewHelper.DbColumn("JZ_IsCasualImport", Bit, -1),
					new TestDbViewHelper.DbColumn("JZ_OtherReference", VarChar, 140),
					new TestDbViewHelper.DbColumn("JZ_PortOfClearance", VarChar, 4),
					new TestDbViewHelper.DbColumn("JZ_RL_NKLastPort", VarChar, 5),
					new TestDbViewHelper.DbColumn("JZ_RN_NKExport", VarChar, 2),
					new TestDbViewHelper.DbColumn("JZ_RN_NKTranshipment", VarChar, 2),
					new TestDbViewHelper.DbColumn("JZ_RoyaltyInd", Bit, -1),
					new TestDbViewHelper.DbColumn("JZ_ServicesInd", Bit, -1),
					new TestDbViewHelper.DbColumn("JZ_TermsOfPayment", VarChar, 35),
					new TestDbViewHelper.DbColumn("JZ_TimeLimit", Int, -1, 10 , 0),
					new TestDbViewHelper.DbColumn("JZ_TimeLimitCode", VarChar, 2),
					new TestDbViewHelper.DbColumn("JZ_TradeZone", VarChar, 4),
					new TestDbViewHelper.DbColumn("JZ_TreatmentCode", VarChar, 2),
					new TestDbViewHelper.DbColumn("JZ_USPortOfExit", VarChar, 5),
					new TestDbViewHelper.DbColumn("JZ_USStateOfExport", VarChar, 2),
					new TestDbViewHelper.DbColumn("JZ_ValueForDutyCode", VarChar, 2),
					new TestDbViewHelper.DbColumn("JZ_LVSLastPrintDate", DateTime, -1),
					new TestDbViewHelper.DbColumn("JZ_IsSeeded", Bit, -1),
					new TestDbViewHelper.DbColumn("JZ_CarrierCode", VarChar, 4),
					new TestDbViewHelper.DbColumn("JZ_IIDRegion", VarChar, 70),
					new TestDbViewHelper.DbColumn("JZ_RN_NKSource", VarChar, 2),
					new TestDbViewHelper.DbColumn("JZ_StateOfSource", VarChar, 2),
					new TestDbViewHelper.DbColumn("JZ_ReadyForConsolidation", Bit, -1),
					new TestDbViewHelper.DbColumn("JZ_DDPDeductDutyOnly", Bit, -1)
				}
			);
		}

		public void TestViewIndexes()
		{
			TestDbViewHelper.AssertViewIndexes(
				Db.Connection,
				"CAJobComInvoiceHeader_Idx",
				new[]
				{
					new TestDbViewHelper.DbIndex("NR_UC__JZ_ClusterKey_JZ_PK", "JZ_ClusterKey,JZ_PK"),
					new TestDbViewHelper.DbIndex("NR_UX__JZ_ReadyForConsolidation", "JZ_ReadyForConsolidation"),
					new TestDbViewHelper.DbIndex("NR_UX__JZ_PortOfClearance", "JZ_PortOfClearance"),
					new TestDbViewHelper.DbIndex("NR_UX__JZ_RN_NKExport", "JZ_RN_NKExport"),
					new TestDbViewHelper.DbIndex("NR_UX__JZ_TreatmentCode", "JZ_TreatmentCode"),
					new TestDbViewHelper.DbIndex("NR_UX__JZ_RN_NKSource", "JZ_RN_NKSource")
				}
			);
		}
	}
}
