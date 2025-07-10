using System;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.ChangeDataCapture.Common;
using Enterprise.ChangeDataCapture.Common.Testing;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Shared
{
	public sealed class DbObjectCreatorAdminConnectionTest : TestCase
	{
		/// <summary>
		/// Checks rename handles error:
		/// Cannot alter column 'Col2' because it is 'REPLICATED'.
		/// </summary>
		[UseSnapshotProtection]
		public void TestRenameColumnInCdcEnabledTable()
		{
			string testRefDb = ((IPhysicalRefDbLocation)Db.Connection).GetReferenceDatabaseName(RefDbTypeEnum.Tariff, "NZ");
			const string testTable = "DbObjectCreatorAdminConnectionTest$Table";

			using (var testAdminConnection = Db.NewAdminConnection(testRefDb))
			{
				try
				{
					if (CdcDatabase.IsEnabled(testAdminConnection, testRefDb))
					{
						CdcDatabase.Disable(testAdminConnection, testRefDb);
					}
					CdcDatabase.Enable(testAdminConnection, testRefDb);

					testAdminConnection.BeginTransaction();

					testAdminConnection.ExecuteNonQuery(String.Format("CREATE TABLE [{0}] (Col1 int PRIMARY KEY, Col2 int)", testTable));
					CdcTableTest.EnableCdcTable(testAdminConnection, testTable);
					CdcTableTest.AssertCdcIsEnabled(testAdminConnection, testTable, true);

					DbObjectCreator.RenameColumn(testAdminConnection, Db.SqlDbOwnerSchema, testTable, "Col2", "Col2New");
					AssertEquals("Is connection still in a transaction context?", true, testAdminConnection.IsInTransaction);
					AssertEquals("Old Column exists?", false, DbObjectCreator.ColumnExists(testAdminConnection, testTable, "Col2"));
					AssertEquals("New Column exists?", true, DbObjectCreator.ColumnExists(testAdminConnection, testTable, "Col2New"));
					CdcTableTest.AssertCdcIsEnabled(testAdminConnection, testTable, false);
				}
				finally
				{
					testAdminConnection.RollbackTransaction();
					CdcDatabase.Disable(testAdminConnection, testRefDb);
				}
			}
		}
	}
}
