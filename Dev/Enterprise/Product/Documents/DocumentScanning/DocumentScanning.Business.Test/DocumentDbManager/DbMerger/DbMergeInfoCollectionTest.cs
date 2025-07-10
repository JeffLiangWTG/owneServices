using System;
using System.Collections.Generic;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.IO;
using Enterprise.DocumentScanning.Business.Testing;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.Business.Test
{
	sealed class DbMergeInfoCollectionTest : TestCase
	{
		#region TestInitialiseAttributes

		[UseSnapshotProtection]
		public void TestInitialiseAttributes_DB() => AssertInitialiseInternalAttributesWithEDocsStorageProvider(Core.Constants.EDocsStorageProviders.Code.DB);

		[UseSnapshotProtection]
		public void TestInitialiseAttributes_S3() => AssertInitialiseInternalAttributesWithEDocsStorageProvider(Core.Constants.EDocsStorageProviders.Code.S3);

		public void TestGetLastWritableDatabaseBeforeDbNumber()
		{
			var internalDictionary = new Dictionary<int, DbMergeInfo>
			{
				{ 1, new DbMergeInfo(1, "1", 1, false) },
				{ 2, new DbMergeInfo(2, "2", 2, false) },
				{ 3, new DbMergeInfo(3, "3", 3, true) },
				{ 4, new DbMergeInfo(4, "4", 4, false) },
				{ 5, new DbMergeInfo(5, "5", 5, false) },
			};
			var collection = new DbMergeInfoCollectionForTesting(internalDictionary);

			AssertEquals(5, collection.GetLastWritableDatabaseBeforeDbNumber(int.MaxValue).Number);
			AssertEquals(4, collection.GetLastWritableDatabaseBeforeDbNumber(5).Number);
			AssertEquals(2, collection.GetLastWritableDatabaseBeforeDbNumber(4).Number);
			AssertEquals(2, collection.GetLastWritableDatabaseBeforeDbNumber(3).Number);
			AssertEquals(1, collection.GetLastWritableDatabaseBeforeDbNumber(2).Number);
			AssertNull(collection.GetLastWritableDatabaseBeforeDbNumber(1));
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());

		void AssertInitialiseInternalAttributesWithEDocsStorageProvider(string providerCode)
		{
			using (SystemDataRegistry.Instance.EDocsStorageProvider.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, providerCode))
			using (var conn = Db.NewAdminConnection())
			{
				var testImageData = resourceRetriever.Value.GetBytes("Enterprise.DocumentScanning.Business.Test.TestDocs.1MB.dat");
				mergerHelper = new DbMergerTestDataHelper(4, testImageData);

				try
				{
					mergerHelper.PrepareTestDbs(conn, 2, 4);
					InitialiseAttributesAndAssert(conn);
				}
				finally
				{
					mergerHelper.CleanupTestData(conn);
				}
			}
		}

		void InitialiseAttributesAndAssert(DbConnection conn)
		{
			try
			{
				var isS3Provider = SystemDataRegistry.Instance.EDocsStorageProvider.Value == Core.Constants.EDocsStorageProviders.Code.S3;

				conn.BeginTransaction();

				DbMergeInfoCollection mergeInfoItems = DbMergeInfoCollection.New(conn, 0);
				AssertEquals("[PRE-CONDITION] Total DB count", 4, mergeInfoItems.Count);

				mergerHelper.NewTestStorageMainAndDocs(conn, 1, 6);
				mergerHelper.NewTestStorageMainAndDocs(conn, 3, 2);

				mergeInfoItems = DbMergeInfoCollection.New(conn, 6);
				AssertEquals("Main DB name", dbHelper.GetDatabaseName(0), mergeInfoItems.MainDbName);
				AssertEquals("Total DB count", 4, mergeInfoItems.Count);

				if (isS3Provider)
				{
					AssertEquals("Number of writable DBs with free space", 2, mergeInfoItems.CountWritableWithFreeSpace);
					AssertEquals("First writable DB with free space", 1, mergeInfoItems.FirstWritableDbWithFreeSpace.Number);
				}
				else
				{
					AssertEquals("Number of writable DBs with free space", 1, mergeInfoItems.CountWritableWithFreeSpace);
					AssertEquals("First writable DB with free space", 3, mergeInfoItems.FirstWritableDbWithFreeSpace.Number);
				}

				AssertEquals("Last DB", 3, mergeInfoItems.LastWritableDatabase.Number);

				AssertDbMergeInfo(mergeInfoItems, conn);

				mergeInfoItems = DbMergeInfoCollection.New(conn, 2);

				if (isS3Provider)
				{
					AssertEquals("All writable DBs larger than the limit", 2, mergeInfoItems.CountWritableWithFreeSpace);
					AssertEquals("First writable DB with free space", 1, mergeInfoItems.FirstWritableDbWithFreeSpace.Number);
					AssertDbMergeInfo(mergeInfoItems, conn);
				}
				else
				{
					AssertEquals("All writable DBs larger than the limit", 0, mergeInfoItems.CountWritableWithFreeSpace);
					AssertNull("No writable DB with free space", mergeInfoItems.FirstWritableDbWithFreeSpace);
				}

				AssertEquals("Last writable DB should still be the same", 3, mergeInfoItems.LastWritableDatabase.Number);
			}
			finally
			{
				conn.RollbackTransaction();
			}
		}

		void AssertDbMergeInfo(DbMergeInfoCollection mergeInfoItems, DbConnection conn)
		{
			CombineAssertions(() =>
			{
				AssertEquals("1st DB Name", dbHelper.GetDatabaseName(1), mergeInfoItems[1].Name);
				AssertEquals("1st DB Number", 1, mergeInfoItems[1].Number);
				Assert("1st DB Size", mergeInfoItems[1].Size >= 6); // SD001 is not recreated, hence the allocated size might not be accurate
				AssertEquals("1st DB is read-only", false, mergeInfoItems[1].IsReadOnly);

				AssertEquals("2nd DB Name", dbHelper.GetDatabaseName(2), mergeInfoItems[2].Name);
				AssertEquals("2nd DB Number", 2, mergeInfoItems[2].Number);
				Assert("2nd DB Size", mergeInfoItems[2].Size >= 1);
				AssertEquals("2nd DB Size (user objects only)", 0, GetSpaceUsedByUserObjects(conn, mergeInfoItems[2].Name));
				AssertEquals("2nd DB is read-only", true, mergeInfoItems[2].IsReadOnly);

				AssertEquals("3rd DB Name", dbHelper.GetDatabaseName(3), mergeInfoItems[3].Name);
				AssertEquals("3rd DB Number", 3, mergeInfoItems[3].Number);
				Assert("3rd DB Size", mergeInfoItems[3].Size >= 3);
				AssertEquals("3rd DB Size (user objects only)", 2, GetSpaceUsedByUserObjects(conn, mergeInfoItems[3].Name));
				AssertEquals("3rd DB is read-only", false, mergeInfoItems[3].IsReadOnly);

				AssertEquals("4th DB Name", dbHelper.GetDatabaseName(4), mergeInfoItems[4].Name);
				AssertEquals("4th DB Number", 4, mergeInfoItems[4].Number);
				Assert("4th DB Size", mergeInfoItems[4].Size >= 1);
				AssertEquals("4th DB Size (user objects only)", 0, GetSpaceUsedByUserObjects(conn, mergeInfoItems[4].Name));
				AssertEquals("4th DB is read-only", true, mergeInfoItems[4].IsReadOnly);
			});
		}

		#endregion

		#region Implementation

		long GetSpaceUsedByUserObjects(DbConnection conn, string dbName)
		{
			var sqlText = string.Format(@"
SELECT sum(used_pages)/128
FROM
	[{0}].sys.allocation_units a
	INNER JOIN [{0}].sys.partitions p
		ON (a.type = 2  AND a.container_id = p.partition_id)
		OR (a.type <> 2 AND a.container_id = p.hobt_id)
	INNER JOIN [{0}].sys.objects o
		ON p.object_id = o.object_id
WHERE
	o.is_ms_shipped = 0", dbName);

			return Convert.ToInt64(conn.ExecuteScalar(sqlText));
		}

		DbMergerTestDataHelper mergerHelper;
		readonly DocManagerDBHelperTestClass dbHelper = new DocManagerDBHelperTestClass();

		#endregion
	}

	public class DbMergeInfoCollectionForTesting : DbMergeInfoCollection
	{
		public DbMergeInfoCollectionForTesting(Dictionary<int, DbMergeInfo> fakeDbMergeInfos, string mainDbName = "")
		{
			internalDictionary = fakeDbMergeInfos;
			MainDbName = mainDbName;
		}
	}
}
