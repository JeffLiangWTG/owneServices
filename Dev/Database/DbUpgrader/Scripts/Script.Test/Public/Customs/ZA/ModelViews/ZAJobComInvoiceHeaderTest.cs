using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using NUnit.Framework;
using static System.Data.SqlDbType;

namespace Enterprise.Build.Database.Script.Testing.Public.Customs.ZA.ModelViews
{
	[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.Customs.ZA.ModelViews.ZAJobComInvoiceHeader))]
	class ZAJobComInvoiceHeaderTest : DbCreateScriptTest
	{
		public void TestViewColumns()
		{
			TestDbViewHelper.AssertViewColumns(
				Db.Connection,
				"ZAJobComInvoiceHeader",
				"JobComInvoiceHeader",
				new []
				{
					new TestDbViewHelper.DbColumn("JZ_PK", UniqueIdentifier, -1),
					new TestDbViewHelper.DbColumn("JZ_ClusterKey", Int, -1, 10, 0),
					new TestDbViewHelper.DbColumn("JZ_ROOCert", VarChar, 35),
					new TestDbViewHelper.DbColumn("JZ_ROOType", VarChar, 3),
					new TestDbViewHelper.DbColumn("JZ_ValuationMarkup", Decimal, -1, 12, 2),
					new TestDbViewHelper.DbColumn("JZ_VDN", VarChar, 35),
				}
			);
		}

		public void TestViewIndexes()
		{
			AssertEquals("ZAJobComInvoiceHeader doesn't require any indexes.", false, DbObjectCreator.ViewExists(Db.Connection, "ZAJobComInvoiceHeader_Idx"));
		}
	}
}
