using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Diagnostics.Testing
{
	sealed class DbSizeInfoCollectionTest : TestCase
	{
		public void TestGetAllocationBuffer()
		{
			AssertEquals("DB Size =     0 => Allocation Buffer", 0, DbGroupSize.GetAllocationBuffer(0));
			AssertEquals("DB Size =     1 => Allocation Buffer", 1, DbGroupSize.GetAllocationBuffer(1));
			AssertEquals("DB Size =     2 => Allocation Buffer", 1, DbGroupSize.GetAllocationBuffer(2));
			AssertEquals("DB Size =     5 => Allocation Buffer", 1, DbGroupSize.GetAllocationBuffer(5));
			AssertEquals("DB Size =     6 => Allocation Buffer", 2, DbGroupSize.GetAllocationBuffer(6));
			AssertEquals("DB Size =    10 => Allocation Buffer", 2, DbGroupSize.GetAllocationBuffer(10));
			AssertEquals("DB Size =    11 => Allocation Buffer", 3, DbGroupSize.GetAllocationBuffer(11));
			AssertEquals("DB Size =   100 => Allocation Buffer", 20, DbGroupSize.GetAllocationBuffer(100));
			AssertEquals("DB Size =   101 => Allocation Buffer", 21, DbGroupSize.GetAllocationBuffer(101));
			AssertEquals("DB Size =  1001 => Allocation Buffer", 201, DbGroupSize.GetAllocationBuffer(1001));
			AssertEquals("DB Size = 10001 => Allocation Buffer", 2001, DbGroupSize.GetAllocationBuffer(10001));
		}

		public void TestDbGroupIsNotCaseSensitive()
		{
			AssertEquals(DbGroupEnum.eDocs, new DbSizeInfo("", 200, 300).GetDbGroup("MainDb_sd001"));
			AssertEquals(DbGroupEnum.eDocs, new DbSizeInfo("", 200, 300).GetDbGroup("MainDb_SD001"));

			AssertEquals(DbGroupEnum.UserRepository, new DbSizeInfo("", 200, 300).GetDbGroup("Main_userrepository"));
			AssertEquals(DbGroupEnum.UserRepository, new DbSizeInfo("", 200, 300).GetDbGroup("Main_UserRepository"));

			AssertEquals(DbGroupEnum.Other, new DbSizeInfo("", 200, 300).GetDbGroup("Main_SD01234"));
			AssertEquals(DbGroupEnum.Other, new DbSizeInfo("", 200, 300).GetDbGroup("Main_RefDb"));
			AssertEquals(DbGroupEnum.Other, new DbSizeInfo("", 200, 300).GetDbGroup("Main_UserRepository123"));
		}

		public void TestGetDatabaseAndGroupSizes()
		{
			const string testDbName = "DbSizeInfoCollectionTestDb";

			try
			{
				using (var adminCnx = Db.NewAdminConnection())
				{
					DropTestDatabases(adminCnx, testDbName);
					CreateTestDatabases(adminCnx, testDbName);
					InsertBiServerRegistries(adminCnx, testDbName);
				}

				using (var testCnx = Db.NewAdminConnection(testDbName))
				{
					var dbSizeCollection = new DbSizeInfoCollectionForTest(testCnx);

					//
					// Databases
					var dbSizes = dbSizeCollection.GetDatabaseSizes().ToList();
					AssertEquals("DB Count", 6, dbSizes.Count);
					AssertDbSizeInfo(dbSizes, 0, testDbName, DbGroupEnum.Main);
					AssertDbSizeInfo(dbSizes, 1, testDbName + "_SD001", DbGroupEnum.eDocs);
					AssertDbSizeInfo(dbSizes, 2, testDbName + "_SD749", DbGroupEnum.eDocs);
					AssertDbSizeInfo(dbSizes, 3, testDbName + "_UserRepository", DbGroupEnum.UserRepository);
					AssertDbSizeInfo(dbSizes, 4, testDbName + Db.AuditDatabaseSuffix, DbGroupEnum.Audit);
					AssertDbSizeInfo(dbSizes, 5, testDbName + Db.EdwDatabaseSuffix, DbGroupEnum.EDW);

					//
					// DB Groups
					var dbGroupSizes = dbSizeCollection.GetDbGroupSizes().ToList();
					AssertEquals("Group Count", 5, dbGroupSizes.Count);

					long expectedMainDbGroupUsedSize = dbSizes[0].UsedDataSizeMb;
					long expectedMainDbGroupDiskSize = dbSizes[0].DiskSizeMb;
					AssertGroupSize(dbGroupSizes, 0, DbGroupEnum.Main, expectedMainDbGroupUsedSize, expectedMainDbGroupDiskSize);

					long expectedEdocsGroupUsedSize = dbSizes[1].UsedDataSizeMb + dbSizes[2].UsedDataSizeMb;
					long expectedEdocsGroupDiskSize = dbSizes[1].DiskSizeMb + dbSizes[2].DiskSizeMb;
					AssertGroupSize(dbGroupSizes, 1, DbGroupEnum.eDocs, expectedEdocsGroupUsedSize, expectedEdocsGroupDiskSize);

					long expectedRepositoryGroupUsedSize = dbSizes[3].UsedDataSizeMb;
					long expectedRepositoryGroupDiskSize = dbSizes[3].DiskSizeMb;
					AssertGroupSize(dbGroupSizes, 2, DbGroupEnum.UserRepository, expectedRepositoryGroupUsedSize, expectedRepositoryGroupDiskSize);

					long expectedAuditGroupUsedSize = dbSizes[4].UsedDataSizeMb;
					long expectedAuditGroupDiskSize = dbSizes[4].DiskSizeMb;
					AssertGroupSize(dbGroupSizes, 3, DbGroupEnum.Audit, expectedAuditGroupUsedSize, expectedAuditGroupDiskSize);

					long expectedEdwGroupUsedSize = dbSizes[5].UsedDataSizeMb;
					long expectedEdwGroupDiskSize = dbSizes[5].DiskSizeMb;
					AssertGroupSize(dbGroupSizes, 4, DbGroupEnum.EDW, expectedEdwGroupUsedSize, expectedEdwGroupDiskSize);
				}
			}
			finally
			{
				using (var adminCnx = Db.NewAdminConnection())
				{
					DropTestDatabases(adminCnx, testDbName);
				}
			}
		}

		void CreateTestDatabases(AdminConnection adminCnx, string testDbName)
		{
			AdoTestUtils.DropDbIfExists(adminCnx, testDbName);
			AdoTestUtils.CreateDbIfNotExists(adminCnx, testDbName);
			AdoTestUtils.CreateDbIfNotExists(adminCnx, testDbName + "_SomethingElse");
			AdoTestUtils.CreateDbIfNotExists(adminCnx, testDbName + "_SD749");
			CreateEpDbSetInfoProcedure(adminCnx, testDbName, Db.DatabaseName);
			AdoTestUtils.CreateDbIfNotExists(adminCnx, testDbName + "_SD001");
			AdoTestUtils.CreateDbIfNotExists(adminCnx, testDbName + "_UserRepository");
			AdoTestUtils.CreateDbIfNotExists(adminCnx, testDbName + Db.AuditDatabaseSuffix);
			CreateEpDbSetInfoProcedure(adminCnx, testDbName + Db.AuditDatabaseSuffix, Db.AuditDatabaseName);
			AdoTestUtils.CreateDbIfNotExists(adminCnx, testDbName + Db.EdwDatabaseSuffix);
			CreateEpDbSetInfoProcedure(adminCnx, testDbName + Db.EdwDatabaseSuffix, Db.EdwDatabaseName);
		}

		void InsertBiServerRegistries(AdminConnection adminCnx, string testDbName)
		{
			var sqlText = String.Format(@"
CREATE TABLE dbo.StmData
( 
		SD_PK UNIQUEIDENTIFIER NOT NULL DEFAULT newid(),
		SD_Name VARCHAR(300) NOT NULL,
		SD_BinaryValue VARBINARY(max) NULL,
		SD_Owner UNIQUEIDENTIFIER NULL,
		SD_DepartmentGuid UNIQUEIDENTIFIER NULL,
)
;

INSERT INTO dbo.StmData(SD_PK, SD_Name, SD_BinaryValue) SELECT newid(), @ServerName, CONVERT(VARBINARY(MAX), N'{0}')
INSERT INTO dbo.StmData(SD_PK, SD_Name, SD_BinaryValue) SELECT newid(), @ServerName, CONVERT(VARBINARY(MAX), N'{0}')",
				Db.ServerName);
			using (((ICurrentDbControl)adminCnx).UseDatabase(testDbName))
			using (var cmd = adminCnx.Command(sqlText))
			{
				cmd.AddParameter("@ServerName", SqlDbType.NVarChar, adminCnx.ServerName);

				cmd.ExecuteNonQuery();
			}
		}

		void CreateEpDbSetInfoProcedure(AdminConnection adminCnx, string testDbName, string mainDbName)
		{
			string sqlText = String.Format(CultureInfo.InvariantCulture, @"
				DECLARE @EpDbSetInfoCreateCmd nvarchar(max) = (
					SELECT m.definition
					FROM
						[{0}].sys.procedures p
						INNER JOIN [{0}].sys.sql_modules m ON m.object_id = p.object_id
					WHERE
						p.name = 'ep_DatabaseSetInfo'
				);
				EXEC [{1}]..sp_executesql @EpDbSetInfoCreateCmd;",
				mainDbName,
				testDbName);
			adminCnx.ExecuteNonQuery(sqlText);
		}

		void DropTestDatabases(AdminConnection adminCnx, string testDbName)
		{
			AdoTestUtils.DropDbIfExists(adminCnx, testDbName + "_SomethingElse");
			AdoTestUtils.DropDbIfExists(adminCnx, testDbName + "_SD749");
			AdoTestUtils.DropDbIfExists(adminCnx, testDbName);
			AdoTestUtils.DropDbIfExists(adminCnx, testDbName + "_SD001");
			AdoTestUtils.DropDbIfExists(adminCnx, testDbName + "_UserRepository");
			AdoTestUtils.DropDbIfExists(adminCnx, testDbName + Db.AuditDatabaseSuffix);
			AdoTestUtils.DropDbIfExists(adminCnx, testDbName + Db.EdwDatabaseSuffix);
		}

		void AssertDbSizeInfo(List<DbSizeInfo> dbSizeInfo, int dbIndex, string expectedName, DbGroupEnum expectedGroup)
		{
			AssertEquals("DbName", expectedName, dbSizeInfo[dbIndex].DbName);
			AssertEquals(expectedName + " => DbGroup", expectedGroup, dbSizeInfo[dbIndex].DbGroup);
			AssertEquals(expectedName + " => UsedDataSizeMb > 0?", true, dbSizeInfo[dbIndex].UsedDataSizeMb > 0);
			AssertEquals(expectedName + " => DiskSizeMb > 0?", true, dbSizeInfo[dbIndex].DiskSizeMb > 0);
		}

		void AssertGroupSize(List<DbGroupSize> dbGroupInfo, int groupIndex, DbGroupEnum expectedGroup, long expectedUsedSize, long expectedDiskSize)
		{
			AssertEquals("DbGroup", expectedGroup, dbGroupInfo[groupIndex].DbGroup);
			AssertEquals("UsedSizeMb", expectedUsedSize, dbGroupInfo[groupIndex].UsedSizeMb);
			AssertEquals("DiskSizeMb", expectedDiskSize, dbGroupInfo[groupIndex].DiskSizeMb);
			AssertEquals(
				string.Format("UsedSizeMb = {0} => AllocationBufferMb = ?", expectedUsedSize),
				DbGroupSize.GetAllocationBuffer(dbGroupInfo[groupIndex].UsedSizeMb),
				dbGroupInfo[groupIndex].AllocationBufferMb);
		}

		class DbSizeInfoCollectionForTest : DbSizeInfoCollection
		{
			public DbSizeInfoCollectionForTest(DbConnection testConnection)
			{
				this.testConnection = testConnection;
			}

			protected override DbConnection Connection
			{
				get { return testConnection; }
			}

			readonly DbConnection testConnection;

			public IEnumerable<DbSizeInfo> GetDatabaseSizes()
			{
				return
					from dbSizeInfo in DbSizeInfoList
					orderby dbSizeInfo.DbGroup, dbSizeInfo.DbName
					select dbSizeInfo;
			}
		}
	}
}
