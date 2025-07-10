using CargoWise.Data;
using NUnit.Framework;
using static System.Data.SqlDbType;

namespace Enterprise.Build.Database.Script.Testing.Public.Customs.JP.ModelViews
{
	[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.Customs.JP.ModelViews.JPCusEntryHeader))]
	sealed class JPCusEntryHeaderTest : DbCreateScriptTest
	{
		public void TestViewColumns()
		{
			TestDbViewHelper.AssertViewColumns(
				Db.Connection,
				"JPCusEntryHeader",
				"CusEntryHeader",
				new[]
				{
					new TestDbViewHelper.DbColumn("CH_PK", UniqueIdentifier, -1),
					new TestDbViewHelper.DbColumn("CH_ClusterKey", Int, -1, 10, 0),
					new TestDbViewHelper.DbColumn("CH_InspectionStatus", VarChar, 4),
				}
			);
		}
	}
}
