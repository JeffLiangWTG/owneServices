using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using NUnit.Framework;
using static System.Data.SqlDbType;

namespace Enterprise.Build.Database.Script.Testing.Public.Customs.CH.ModelViews
{
	[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.Customs.CH.ModelViews.CHCusEntryHeader))]
	sealed class CHCusEntryHeaderTest : DbCreateScriptTest
	{
		public void TestViewColumns()
		{
			TestDbViewHelper.AssertViewColumns(
				Db.Connection,
				"CHCusEntryHeader",
				"CusEntryHeader",
				new[]
				{
					new TestDbViewHelper.DbColumn("CH_PK", UniqueIdentifier, -1),
					new TestDbViewHelper.DbColumn("CH_ClusterKey", Int, -1, 10, 0),
					new TestDbViewHelper.DbColumn("CH_LastEComplaintStatus", VarChar, 3),
				}
			);
		}

		public void TestViewIndexes()
		{
			AssertEquals("CHCusEntryHeader doesn't require any indexes.", false, DbObjectCreator.ViewExists(Db.Connection, "CHCusEntryHeader_Idx"));
		}
	}
}
