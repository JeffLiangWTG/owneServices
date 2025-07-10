using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using NUnit.Framework;
using static System.Data.SqlDbType;

namespace Enterprise.Build.Database.Script.Testing.Public.Customs.AE.ModelViews;

[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.Customs.AE.ModelViews.AEJobComInvoiceHeader))]
sealed class AEJobComInvoiceHeaderTest : DbCreateScriptTest
{
	public void TestViewColumns()
	{
		TestDbViewHelper.AssertViewColumns(
			Db.Connection,
			"AEJobComInvoiceHeader",
			"JobComInvoiceHeader",
			new[]
			{
				new TestDbViewHelper.DbColumn("JZ_PK", UniqueIdentifier, -1),
				new TestDbViewHelper.DbColumn("JZ_ClusterKey", Int, -1, 10, 0),
				new TestDbViewHelper.DbColumn("JZ_InvoiceType", Int, -1, 10, 0),
				new TestDbViewHelper.DbColumn("JZ_TotNoOfInvPages", Int, -1, 10, 0),
				new TestDbViewHelper.DbColumn("JZ_AttestationNo", VarChar, 20)
			}
		);
	}

	public void TestViewIndexes()
	{
		AssertEquals("AEJobComInvoiceHeader doesn't require any indexes.", false, DbObjectCreator.ViewExists(Db.Connection, "AEJobComInvoiceHeader_Idx"));
	}
}
