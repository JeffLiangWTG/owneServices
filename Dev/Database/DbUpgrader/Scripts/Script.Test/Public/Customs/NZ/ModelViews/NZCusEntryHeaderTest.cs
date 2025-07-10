using CargoWise.Data;
using Enterprise.Build.Database.Script.Testing;
using Enterprise.DbUpgrader.Shared;
using NUnit.Framework;
using static System.Data.SqlDbType;

namespace Enterprise.Build.Database.Script.Public.Customs.TW.ModelViews
{
	[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.Customs.NZ.ModelViews.NZCusEntryHeader))]
	sealed class NZCusEntryHeaderTest : DbCreateScriptTest
	{
		public void TestViewColumns()
		{
			TestDbViewHelper.AssertViewColumns(
				Db.Connection,
				"NZCusEntryHeader",
				"CusEntryHeader",
				new[]
				{
					new TestDbViewHelper.DbColumn("CH_PK", UniqueIdentifier, -1),
					new TestDbViewHelper.DbColumn("CH_ConsolidatedEntryMemberID", SmallInt, -1, 5, 0),
					new TestDbViewHelper.DbColumn("CH_ClusterKey", Int, -1, 10, 0),
					new TestDbViewHelper.DbColumn("CH_EDITransmitDate", DateTime, -1),
					new TestDbViewHelper.DbColumn("CH_EntryChargeWaived", Bit, -1),
					new TestDbViewHelper.DbColumn("CH_IsActive", Bit, -1),
					new TestDbViewHelper.DbColumn("CH_IsEntryCancelled", Bit, -1),
					new TestDbViewHelper.DbColumn("CH_IsRestored", Bit, -1),
					new TestDbViewHelper.DbColumn("CH_LastEntryStyle", VarChar, 3),
					new TestDbViewHelper.DbColumn("CH_LastNumberOfLinesSentToCustoms", Int, -1, 10, 0),
					new TestDbViewHelper.DbColumn("CH_LastResponseVersionNumber", Int, -1, 10, 0),
					new TestDbViewHelper.DbColumn("CH_MPIBioMovementStatus", VarChar, 3),
					new TestDbViewHelper.DbColumn("CH_MPIBioMovementStatusTime", DateTime, -1),
					new TestDbViewHelper.DbColumn("CH_MPIBioResponseTime", DateTime, -1),
					new TestDbViewHelper.DbColumn("CH_MPIBioStatus", VarChar, 3),
					new TestDbViewHelper.DbColumn("CH_MPIFoodResponseTime", DateTime, -1),
					new TestDbViewHelper.DbColumn("CH_MPIFoodStatus", VarChar, 3),
					new TestDbViewHelper.DbColumn("CH_NZCSMovementStatus", VarChar, 3),
					new TestDbViewHelper.DbColumn("CH_NZCSMovementStatusTime", DateTime, -1),
					new TestDbViewHelper.DbColumn("CH_NZCSResponseTime", DateTime, -1),
					new TestDbViewHelper.DbColumn("CH_NZCSStatus", VarChar, 3),
					new TestDbViewHelper.DbColumn("CH_OverrideIndicator", Bit, -1),
					new TestDbViewHelper.DbColumn("CH_RecordAdded", DateTime, -1),
					new TestDbViewHelper.DbColumn("CH_TotalAmountReturned", Decimal, -1, 12, 2),
				});
		}

		public void TestViewIndexes()
		{
			AssertEquals("NZCusEntryHeader doesn't require any indexes.", false, DbObjectCreator.ViewExists(Db.Connection, "NZCusEntryHeader_Idx"));
		}
	}
}
