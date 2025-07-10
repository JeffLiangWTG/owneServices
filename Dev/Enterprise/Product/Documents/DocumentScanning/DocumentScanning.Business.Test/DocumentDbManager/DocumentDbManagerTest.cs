using System;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.Business.Test
{
	public class DocumentDbManagerTest : TestCase
	{
		[UseSnapshotProtection]
		public void TestSetDbReadOnlyAndReadWrite()
		{
			using (AdminConnection auxConnection = Db.NewAdminConnection())
			{
				AdoTestUtils.CreateDbIfNotExists(auxConnection, db1Name);

				try
				{
					AdoTestUtils.CreateDbIfNotExists(auxConnection, db2Name);
					InsertTestStorageMainRecords(auxConnection);
					CreateTestStorageDocs(auxConnection, db2Name);

					BusinessObjectFactory factory = new BusinessObjectFactory(auxConnection);
					DocumentDbManager docDbManager = new DocumentDbManager(factory);
					AssertStorageDbCollection(docDbManager, false, 3);

					// Make SD002 read-only
					docDbManager.StorageDatabaseCollection[1].NewReadOnly = true;
					docDbManager.Save();
					docDbManager.StorageDatabaseCollection.Load();
					AssertStorageDbCollection(docDbManager, true, 3);

					// Make SD002 writeable
					docDbManager.StorageDatabaseCollection[1].NewReadOnly = false;
					docDbManager.Save();
					docDbManager.StorageDatabaseCollection.Load();
					AssertStorageDbCollection(docDbManager, false, 3);
				}
				finally
				{
					AdoTestUtils.DropDbIfExists(auxConnection, db2Name);
				}
			}
		}

		void AssertStorageDbCollection(DocumentDbManager docDbManager, bool expectedDb2ReadOnly, int expectedDb2Count)
		{
			DbConnection conn = ((IDbConnected)docDbManager.Factory).Connection;

			AssertEquals("eDocs DB collection count", 2, docDbManager.StorageDatabaseCollection.Count);

			AssertEquals("1st Database - DatabaseNumber", 1, docDbManager.StorageDatabaseCollection[0].DatabaseNumber);
			AssertEquals("1st Database - DatabaseName", db1Name, docDbManager.StorageDatabaseCollection[0].DatabaseName);
			AssertEquals("1st Database - OriginalReadOnly", false, docDbManager.StorageDatabaseCollection[0].OriginalReadOnly);
			AssertEquals("1st Database - NewReadOnly", false, docDbManager.StorageDatabaseCollection[0].NewReadOnly);

			AssertEquals("2nd Database - DatabaseNumber", 2, docDbManager.StorageDatabaseCollection[1].DatabaseNumber);
			AssertEquals("2nd Database - DatabaseName", db2Name, docDbManager.StorageDatabaseCollection[1].DatabaseName);
			AssertEquals("2nd Database - OriginalReadOnly", expectedDb2ReadOnly, docDbManager.StorageDatabaseCollection[1].OriginalReadOnly);
			AssertEquals("2nd Database - NewReadOnly", expectedDb2ReadOnly, docDbManager.StorageDatabaseCollection[1].NewReadOnly);

			AssertEquals("2nd Database - eDocs Count", expectedDb2Count, GetStorageDocsCount(conn, db2Name));
		}

		int GetStorageDocsCount(DbConnection conn, string storageDbName)
		{
			string sqlText = String.Format("SELECT count(*) FROM [{0}]..StorageDocs;", storageDbName);
			return (int)conn.ExecuteScalar(sqlText);
		}

		void InsertTestStorageMainRecords(DbConnection conn)
		{
			string sqlText = String.Format(@"
				INSERT dbo.StorageMain (SM_PK, SM_DB, SM_ParentFK) VALUES ('{0}', 2, newid());
				INSERT dbo.StorageMain (SM_PK, SM_DB, SM_ParentFK) VALUES ('{1}', 999, newid());",
				storageMainDb002Pk.ToString(),
				storageMainDb999Pk.ToString());
			conn.ExecuteNonQuery(sqlText);
		}

		void CreateTestStorageDocs(DbConnection conn, string storageDbName)
		{
			var deleteStorageDocTableScript = FormattableString.Invariant($@"
					IF OBJECT_ID('{storageDbName}..StorageDocs','U') IS NOT NULL
						DROP TABLE [{storageDbName}].[dbo].[StorageDocs]
");
			string createDbObjectScript = new DbUpgrader.Resource.ScriptManager().DocManagerSchemaScript;

			string insertStorageDocsSql = String.Format(@"
				INSERT dbo.StorageDocs (SC_PK, SC_SM, SC_Date, SC_SystemCreateTimeUtc, SC_SystemLastEditTimeUtc, SC_ImageData) VALUES (newid(), newid(), getutcdate(), getutcdate(), getutcdate(), 0x0001);	
				INSERT dbo.StorageDocs (SC_PK, SC_SM, SC_Date, SC_SystemCreateTimeUtc, SC_SystemLastEditTimeUtc, SC_ImageData) VALUES (newid(), '{0}', getutcdate(), getutcdate(), getutcdate(), 0x0002);	
				INSERT dbo.StorageDocs (SC_PK, SC_SM, SC_Date, SC_SystemCreateTimeUtc, SC_SystemLastEditTimeUtc, SC_ImageData) VALUES (newid(), '{1}', getutcdate(), getutcdate(), getutcdate(), 0x0999);",
				storageMainDb002Pk, storageMainDb999Pk);

			using (((ICurrentDbControl)conn).UseDatabase(storageDbName))
			{
				conn.ExecuteNonQuery(deleteStorageDocTableScript);
				conn.ExecuteNonQuery(createDbObjectScript);
				conn.ExecuteNonQuery(insertStorageDocsSql);
			}
		}

		readonly string db1Name = Db.DatabaseName + "_SD001";
		readonly string db2Name = Db.DatabaseName + "_SD002";
		readonly Guid storageMainDb002Pk = Guid.NewGuid();
		readonly Guid storageMainDb999Pk = Guid.NewGuid();
	}

	[TestedType(typeof(DocumentDbManager))]
	class DocumentDbManagerRequiredTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new DocumentDbManager(Factory);
		}
	}
}
