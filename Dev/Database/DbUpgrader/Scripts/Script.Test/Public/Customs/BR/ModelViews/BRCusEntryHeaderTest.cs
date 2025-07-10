using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using NUnit.Framework;
using static System.Data.SqlDbType;

namespace Enterprise.Build.Database.Script.Testing.Public.Customs.BR.ModelViews
{
	[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.Customs.BR.ModelViews.BRCusEntryHeader))]
	class BRCusEntryHeaderTest : DbCreateScriptTest
	{
		public void TestViewColumns()
		{
			TestDbViewHelper.AssertViewColumns(
				Db.Connection,
				"BRCusEntryHeader",
				"CusEntryHeader",
				new[]
				{
					new TestDbViewHelper.DbColumn("CH_PK", UniqueIdentifier, -1),
					new TestDbViewHelper.DbColumn("CH_ClusterKey", Int, -1, 10, 0),
					new TestDbViewHelper.DbColumn("CH_AdministrativeStatus", VarChar, 1),
					new TestDbViewHelper.DbColumn("CH_CargoStatus", VarChar, 2),
					new TestDbViewHelper.DbColumn("CH_CustomsPostedStatus", VarChar, 3),
					new TestDbViewHelper.DbColumn("CH_RiskChannel", VarChar, 1),
					new TestDbViewHelper.DbColumn("CH_ValidityILDispatchDate", DateTime, -1),
					new TestDbViewHelper.DbColumn("CH_ValidityILShipmentDate", DateTime, -1),
					new TestDbViewHelper.DbColumn("CH_AuthorityVersion", VarChar, 4)
				}
			);
		}

		public void TestViewIndexes()
		{
			AssertEquals("BRCusEntryHeader doesn't require any indexes.", false, DbObjectCreator.ViewExists(Db.Connection, "BRCusEntryHeader_Idx"));
		}
	}
}
