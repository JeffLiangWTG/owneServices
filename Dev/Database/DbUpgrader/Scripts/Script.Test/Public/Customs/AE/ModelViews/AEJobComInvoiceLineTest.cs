using System.Data;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.Customs.AE.ModelViews;

[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.Customs.AE.ModelViews.AEJobComInvoiceLine))]
sealed class AEJobComInvoiceLineTest : DbCreateScriptTest
{
	public void TestViewColumns()
	{
		TestDbViewHelper.AssertViewColumns(
			Db.Connection,
			"AEJobComInvoiceLine",
			"JobComInvoiceLine",
			new[]
			{
				new TestDbViewHelper.DbColumn("JI_PK", SqlDbType.UniqueIdentifier, -1),
				new TestDbViewHelper.DbColumn("JI_ClusterKey", SqlDbType.Int, -1, 10, 0),
				new TestDbViewHelper.DbColumn("JI_NewUsed", SqlDbType.VarChar, 1)
			}
		);
	}

	public void TestViewIndexes()
	{
		AssertEquals("AEJobComInvoiceHeader doesn't require any indexes.", false, DbObjectCreator.ViewExists(Db.Connection, "AEJobComInvoiceLine_Idx"));
	}
}
