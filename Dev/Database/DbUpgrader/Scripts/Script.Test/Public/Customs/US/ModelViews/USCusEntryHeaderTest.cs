using CargoWise.Data;
using NUnit.Framework;
using static System.Data.SqlDbType;

namespace Enterprise.Build.Database.Script.Testing.Public.Customs.US.ModelViews
{
	[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.Customs.US.ModelViews.USCusEntryHeader))]
	class USCusEntryHeaderTest : DbCreateScriptTest
	{
		public void TestViewColumns()
		{
			TestDbViewHelper.AssertViewColumns(
				Db.Connection,
				"USCusEntryHeader",
				"CusEntryHeader",
				new []
				{
					new TestDbViewHelper.DbColumn("CH_PK", UniqueIdentifier, -1),
					new TestDbViewHelper.DbColumn("CH_ClusterKey", Int, -1, 10, 0),
					new TestDbViewHelper.DbColumn("CH_ALDate", DateTime, -1),
					new TestDbViewHelper.DbColumn("CH_ALDuty", Decimal, -1, 12, 4),
					new TestDbViewHelper.DbColumn("CH_CollectionDate", DateTime, -1),
					new TestDbViewHelper.DbColumn("CH_DestinationState", VarChar, 2),
					new TestDbViewHelper.DbColumn("CH_DutyCalcDate", DateTime, -1),
					new TestDbViewHelper.DbColumn("CH_ShouldBeReportToCustoms", Bit, -1),
					new TestDbViewHelper.DbColumn("CH_UseConsigneeNameAddress", Bit, -1),
					new TestDbViewHelper.DbColumn("CH_XTN", VarChar, 35),
					new TestDbViewHelper.DbColumn("CH_IsDeactivated", Bit, -1),
					new TestDbViewHelper.DbColumn("CH_MPFRate", VarChar, 8),
					new TestDbViewHelper.DbColumn("CH_MPFCalcDate", DateTime, -1),
					new TestDbViewHelper.DbColumn("CH_TIBExpiryDate", DateTime, -1),
					new TestDbViewHelper.DbColumn("CH_TIBNumOfExtensions", Int, -1, 10, 0),
					new TestDbViewHelper.DbColumn("CH_CRLCertStatus", VarChar, 1),
					new TestDbViewHelper.DbColumn("CH_CWOStatus", VarChar, 3),
					new TestDbViewHelper.DbColumn("CH_SentLatestFDA", Bit, -1),
					new TestDbViewHelper.DbColumn("CH_ProtestID", VarChar, 35),
					new TestDbViewHelper.DbColumn("CH_PendingActionIDType", VarChar, 1),
					new TestDbViewHelper.DbColumn("CH_PendingActionID", VarChar, 35),
					new TestDbViewHelper.DbColumn("CH_PriorDisclosure", Bit, -1),
					new TestDbViewHelper.DbColumn("CH_NAFTAClaimStat", Bit, -1),
					new TestDbViewHelper.DbColumn("CH_ProtestStat", Bit, -1),
				}
			);
		}

		public void TestViewIndexes()
		{
			TestDbViewHelper.AssertViewIndexes(
				Db.Connection,
				"USCusEntryHeader_Idx",
				new[]
				{
					new TestDbViewHelper.DbIndex("NR_UC__CH_ClusterKey_CH_PK", "CH_ClusterKey,CH_PK"),
					new TestDbViewHelper.DbIndex("NR_UX__CH_ALDate", "CH_ALDate"),
					new TestDbViewHelper.DbIndex("NR_UX__CH_TIBExpiryDate", "CH_TIBExpiryDate"),
				}
			);
		}
	}
}
