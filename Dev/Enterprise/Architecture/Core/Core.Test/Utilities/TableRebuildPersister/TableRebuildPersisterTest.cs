using System.Linq;
using CargoWise.Data;
using CargoWise.Data.SqlServer;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class TableRebuildPersisterTest : TransactionedTestCase
	{
		public void TestSaveAndLoad()
		{
			var persister = new TableRebuildPersister(Db.Connection);
			persister.Clear();

			persister.MarkTableAsRequiringRebuild(new DbSchemaTable(Db.DatabaseName, "dbo", "OrgHeader"));
			var nextToRebuild = persister.GetTablesToRebuild().First();
			AssertEquals("[" + Db.DatabaseName + "].[dbo].[OrgHeader]", nextToRebuild.ToString());
			// to prove it doesn't steal
			AssertEquals("[" + Db.DatabaseName + "].[dbo].[OrgHeader]", persister.GetTablesToRebuild().First().ToString());

			persister.MarkTableAsNotRequiringRebuild(nextToRebuild);
			AssertEquals(0, persister.GetTablesToRebuild().Count());
		}

		public void TestSaveAndLoad_NonExistentTable()
		{
			var persister = new TableRebuildPersister(Db.Connection);
			persister.Clear();

			var nonExistentTable = new DbSchemaTable(Db.DatabaseName, "dbo", "NonExistentTable");
			AssertEquals("Table does not exist", true, persister.MarkTableAsRequiringRebuild(nonExistentTable));
			AssertEquals("Table does not exist", 0, persister.GetTablesToRebuild().Count());
		}

		public void TestEmptySchemaName()
		{
			var persister = new TableRebuildPersister(Db.Connection);
			persister.Clear();

			persister.MarkTableAsRequiringRebuild(new DbSchemaTable(Db.DatabaseName, string.Empty, "OrgHeader"));
			var nextToRebuild = persister.GetTablesToRebuild().First();
			AssertEquals("[" + Db.DatabaseName + "].[dbo].[OrgHeader]", nextToRebuild.ToString());
		}
	}
}
