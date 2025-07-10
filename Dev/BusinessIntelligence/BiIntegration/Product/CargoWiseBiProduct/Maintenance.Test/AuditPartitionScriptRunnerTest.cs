using System;
using System.Data;
using System.Globalization;
using CargoWise.Bi.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.ChangeDataCapture.Common;
using NUnit.Framework;

namespace CargoWise.Bi.Maintenance.Testing
{
	class AuditPartitionScriptRunnerTest : TestCase
	{
		#region Partition Tests

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestNciAreRecreatedIfPartitioningSucceedsButIndexCreationFails()
		{
			var auditDbName = Db.AuditDatabaseName;

			using (var connection = Db.NewAdminConnection())
			{
				RemoveAuditTablesFromConfiguration(connection, auditDbName);
				TruncateLsnTimeMappingTable(connection, auditDbName);
				CreateTestTable(connection, auditDbName, testTableName + "1");
				PartitionChsWithCciIndex(connection);

				CombineAssertions("Partition and CCI should exist, but nonclustered indexes should not", () =>
				{
					AssertNumberOfPartitions(auditDbName, "biadmin", "CdcHistorySummary", connection, 1);
					Assert("Nonclustered index IX_CdcHistorySummary_Lsn should not exist", !ChsNciExists(connection, dataSpaceType: "PS", indexSuffix: "Lsn"));
					Assert("Nonclustered index IX_CdcHistorySummary_SchemaName_ChangedTableName_Lsn should not exist", !ChsNciExists(connection, dataSpaceType: "PS", indexSuffix: "SchemaName_ChangedTableName_Lsn"));
					Assert("Columnstore index cci_biadmin_CdcHistorySummary should exist", ChsCciExists(connection));
				});

				Execute_usp_RecreatePartitionsAndPurgeOldData(auditDbName, connection);

				CombineAssertions("Partition and indexes should exist", () =>
				{
					AssertNumberOfPartitions(auditDbName, "biadmin", "CdcHistorySummary", connection, 6);
					Assert("Nonclustered index IX_CdcHistorySummary_Lsn should exist", ChsNciExists(connection, dataSpaceType: "PS", indexSuffix: "Lsn"));
					Assert("Nonclustered index IX_CdcHistorySummary_SchemaName_ChangedTableName_Lsn should exist", ChsNciExists(connection, dataSpaceType: "PS", indexSuffix: "SchemaName_ChangedTableName_Lsn"));
					Assert("Columnstore index cci_biadmin_CdcHistorySummary should exist", ChsCciExists(connection));
				});
			}
		}

		void PartitionChsWithCciIndex(AdminConnection connection)
		{
			EnsureNonclusteredIndexExistsIfItShould(connection, false, indexSuffix: "Lsn");
			EnsureNonclusteredIndexExistsIfItShould(connection, false, indexSuffix: "SchemaName_ChangedTableName_Lsn");
			EnsureColumnstoreIndexExistsIfItShould(connection, true);
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestRecreatePartitionsAndPurgeOldData_AllIndexesExist()
		{
			RecreatePartitionsAndPurgeAllDataTestCore(nonclustered_lsn: true, nonclustered__SchemaName_ChangedTableName_Lsn: true, columnstore: true);
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestRecreatePartitionsAndPurgeOldData_NoIndexesExist()
		{
			RecreatePartitionsAndPurgeAllDataTestCore(nonclustered_lsn: false, nonclustered__SchemaName_ChangedTableName_Lsn: false, columnstore: false);
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestRecreatePartitionsAndPurgeOldData_SecondNonClusteredIndexMissing()
		{
			RecreatePartitionsAndPurgeAllDataTestCore(nonclustered_lsn: false, nonclustered__SchemaName_ChangedTableName_Lsn: true, columnstore: true);
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestRecreatePartitionsAndPurgeOldData_FirstNonClusteredIndexMissing()
		{
			RecreatePartitionsAndPurgeAllDataTestCore(nonclustered_lsn: true, nonclustered__SchemaName_ChangedTableName_Lsn: false, columnstore: true);
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestRecreatePartitionsAndPurgeOldData_ColumnstoreIndexMissing()
		{
			RecreatePartitionsAndPurgeAllDataTestCore(nonclustered_lsn: true, nonclustered__SchemaName_ChangedTableName_Lsn: true, columnstore: false);
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestRecreatePartitionsAndPurgeOldData_BothNonClusteredIndexMissing()
		{
			RecreatePartitionsAndPurgeAllDataTestCore(nonclustered_lsn: false, nonclustered__SchemaName_ChangedTableName_Lsn: false, columnstore: true);
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestRecreatePartitionsAndPurgeOldData_OnlySecondNonclusteredIndex()
		{
			RecreatePartitionsAndPurgeAllDataTestCore(nonclustered_lsn: false, nonclustered__SchemaName_ChangedTableName_Lsn: true, columnstore: false);
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestRecreatePartitionsAndPurgeOldData_OnlyFirstNonclusteredIndex()
		{
			RecreatePartitionsAndPurgeAllDataTestCore(nonclustered_lsn: true, nonclustered__SchemaName_ChangedTableName_Lsn: false, columnstore: false);
		}

		void RecreatePartitionsAndPurgeAllDataTestCore(bool nonclustered_lsn, bool nonclustered__SchemaName_ChangedTableName_Lsn, bool columnstore)
		{
			var auditDbName = Db.AuditDatabaseName;

			using (var connection = Db.NewAdminConnection())
			{
				SetupTestRecreatePartitionsAndPurgeOldData(auditDbName, connection);
				SetCdcHistorySummaryIndexes(connection, nonclustered_lsn, nonclustered__SchemaName_ChangedTableName_Lsn, columnstore);
				Execute_usp_RecreatePartitionsAndPurgeOldData(auditDbName, connection);

				CombineAssertions(() =>
				{
					AssertNumberOfRecords(auditDbName, testSchemaName, testTableName + "1", connection, 4);
					AssertNumberOfRecords(auditDbName, testSchemaName, testTableName + "2", connection, 4);

					AssertNumberOfPartitions(auditDbName, testSchemaName, testTableName + "1", connection, 9);
					AssertNumberOfPartitions(auditDbName, testSchemaName, testTableName + "2", connection, 9);

					AssertPartitionRangeValues(auditDbName, testSchemaName, testTableName + "1", connection, 4, 4);
					AssertPartitionRangeValues(auditDbName, testSchemaName, testTableName + "2", connection, 4, 4);
				});
			}
		}

		void SetupTestRecreatePartitionsAndPurgeOldData(string auditDbName, AdminConnection connection)
		{
			RemoveAuditTablesFromConfiguration(connection, auditDbName);
			TruncateLsnTimeMappingTable(connection, auditDbName);
			CreateTestTable(connection, auditDbName, testTableName + "1");
			CreateTestTable(connection, auditDbName, testTableName + "2");

			InsertLsnTimeMappingRows(connection, auditDbName, -3, 4);
			InsertTestAuditRecords(connection, auditDbName, testSchemaName, testTableName + "1");
			InsertTestAuditRecords(connection, auditDbName, testSchemaName, testTableName + "2");
		}

		void SetCdcHistorySummaryIndexes(AdminConnection connection, bool nonclustered_lsn, bool nonclustered__SchemaName_ChangedTableName_Lsn, bool columnstore)
		{
			EnsureNonclusteredIndexExistsIfItShould(connection, nonclustered_lsn, indexSuffix: "Lsn");
			EnsureNonclusteredIndexExistsIfItShould(connection, nonclustered__SchemaName_ChangedTableName_Lsn, indexSuffix: "SchemaName_ChangedTableName_Lsn");
			EnsureColumnstoreIndexExistsIfItShould(connection, columnstore);
		}

		void EnsureColumnstoreIndexExistsIfItShould(AdminConnection connection, bool shouldExist)
		{
			var doesExist = ChsCciExists(connection);

			var sqlText = $@"
				-- if it should not exist, but does, drop it
				IF @ShouldExist = 0 AND @DoesExist = 1 BEGIN
						DROP INDEX cci_biadmin_CdcHistorySummary ON biadmin.CdcHistorySummary;
				END

				-- if it should exist, but does not, create it
				ELSE IF @ShouldExist = 1 AND @DoesExist = 0 BEGIN
					CREATE CLUSTERED INDEX cci_biadmin_CdcHistorySummary ON biadmin.CdcHistorySummary (LsnPeriod ASC) ON PS_AuditPartitionScheme(LsnPeriod);
				END
			";

			try
			{
				using (((ICurrentDbControl)connection).UseDatabase(Db.AuditDatabaseName))
				using (var cmd = connection.Command(sqlText))
				{
					cmd.AddParameter("@ShouldExist", SqlDbType.Bit, shouldExist);
					cmd.AddParameter("@DoesExist", SqlDbType.Bit, doesExist);

					cmd.ExecuteNonQuery();
				}
			}
			catch (Exception ex)
			{
				throw new Exception($"Failure in ensuring existence of index 'cci_biadmin_CdcHistorySummary'.\r\nShouldExist = {shouldExist}\r\nDoesExist = {doesExist}\r\nQuery:\r\n{sqlText}", ex);
			}
		}

		static bool ChsCciExists(AdminConnection connection)
		{
			using (((ICurrentDbControl)connection).UseDatabase(Db.AuditDatabaseName))
			{
				return connection.ExecuteScalar<int>(@"
					IF EXISTS (
						SELECT 1 FROM sys.indexes WHERE name = 'cci_biadmin_CdcHistorySummary'
					) SELECT 1 ELSE SELECT 0
				") == 1;
			}
		}

		void EnsureNonclusteredIndexExistsIfItShould(AdminConnection connection, bool shouldExist, string indexSuffix)
		{
			string createIndexQuery = "";
			if (indexSuffix == "Lsn")
			{
				createIndexQuery = $"CREATE NONCLUSTERED INDEX IX_CdcHistorySummary_{indexSuffix} ON biadmin.CdcHistorySummary(Lsn) WITH(DATA_COMPRESSION = PAGE) ON PS_AuditPartitionScheme([LsnPeriod]);";
			}
			if (indexSuffix == "SchemaName_ChangedTableName_Lsn")
			{
				createIndexQuery = $"CREATE NONCLUSTERED INDEX IX_CdcHistorySummary_{indexSuffix} ON biadmin.CdcHistorySummary(SchemaName, ChangedTableName, Lsn) INCLUDE(TranEndTimeUTC) WITH(DATA_COMPRESSION = PAGE) ON PS_AuditPartitionScheme([LsnPeriod]);";
			}

			var dataSpaceType = shouldExist ? "FG" : "PS";
			var doesExist = ChsNciExists(connection, dataSpaceType, indexSuffix);

			var sqlText = $@"
				IF @ShouldExist = 0 AND @DoesExist = 1 BEGIN
						DROP INDEX IX_CdcHistorySummary_{indexSuffix} ON biadmin.CdcHistorySummary
				END

				ELSE IF @ShouldExist = 1 AND @DoesExist = 0 BEGIN
					{createIndexQuery}
				END
";
			try
			{
				using (((ICurrentDbControl)connection).UseDatabase(Db.AuditDatabaseName))
				using (var cmd = connection.Command(sqlText))
				{
					cmd.AddParameter("@ShouldExist", SqlDbType.Bit, shouldExist);
					cmd.AddParameter("@DoesExist", SqlDbType.Bit, doesExist);

					cmd.ExecuteNonQuery();
				}
			}
			catch (Exception ex)
			{
				throw new Exception($"Failure in ensuring existence of index 'IX_CdcHistorySummary_{indexSuffix}'.\r\nShouldExist = {shouldExist}\r\nDoesExist = {doesExist}\r\nQuery:\r\n{sqlText}", ex);
			}
		}

		static bool ChsNciExists(AdminConnection connection, string dataSpaceType, string indexSuffix)
		{
			var sqlText = $@"
				IF EXISTS (
					SELECT NULL
					FROM sys.indexes i
						INNER JOIN sys.data_spaces ds ON i.data_space_id = ds.data_space_id  
					WHERE
						ds.type = @DataSpaceType AND
						i.name like 'IX_CdcHistorySummary_' + @IndexSuffix
				) SELECT 1 ELSE SELECT 0
			";

			using (((ICurrentDbControl)connection).UseDatabase(Db.AuditDatabaseName))
			using (var cmd = connection.Command(sqlText))
			{
				cmd.AddParameter("@IndexSuffix", SqlDbType.VarChar, 128, indexSuffix);
				cmd.AddParameter("@DataSpaceType", SqlDbType.VarChar, 128, dataSpaceType);

				return (int)cmd.ExecuteScalar() == 1;
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestRecreatePartitionsAndPurgeOldDataEmptyTables()
		{
			var auditDbName = Db.AuditDatabaseName;

			using (var connection = Db.NewAdminConnection())
			{
				RemoveAuditTablesFromConfiguration(connection, auditDbName);
				TruncateLsnTimeMappingTable(connection, auditDbName);
				CreateTestTable(connection, auditDbName, testTableName + "1");
				CreateTestTable(connection, auditDbName, testTableName + "2");

				Execute_usp_RecreatePartitionsAndPurgeOldData(auditDbName, connection);

				AssertNumberOfRecords(auditDbName, testSchemaName, testTableName + "1", connection, 0);
				AssertNumberOfRecords(auditDbName, testSchemaName, testTableName + "2", connection, 0);

				AssertNumberOfPartitions(auditDbName, testSchemaName, testTableName + "1", connection, 6);
				AssertNumberOfPartitions(auditDbName, testSchemaName, testTableName + "2", connection, 6);

				AssertPartitionRangeValues(auditDbName, testSchemaName, testTableName + "1", connection, 0, 5);
				AssertPartitionRangeValues(auditDbName, testSchemaName, testTableName + "2", connection, 0, 5);
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestRecreatePartitionsAndPurgeOldDataHeteroTables()
		{
			var auditDbName = Db.AuditDatabaseName;

			using (var connection = Db.NewAdminConnection())
			{
				RemoveAuditTablesFromConfiguration(connection, auditDbName);
				TruncateLsnTimeMappingTable(connection, auditDbName);
				CreateTestTable(connection, auditDbName, testTableName + "1");
				CreateTestColumnStoreTable(connection, auditDbName, testTableName + "2");
				CreateTestRowStorePartitionedIndexTable(connection, auditDbName, testTableName + "3");

				InsertLsnTimeMappingRows(connection, auditDbName, -3, 4);
				InsertTestAuditRecords(connection, auditDbName, testSchemaName, testTableName + "1");
				InsertTestAuditRecords(connection, auditDbName, testSchemaName, testTableName + "2");
				InsertTestAuditRecords(connection, auditDbName, testSchemaName, testTableName + "3");

				Execute_usp_RecreatePartitionsAndPurgeOldData(auditDbName, connection);

				AssertNumberOfRecords(auditDbName, testSchemaName, testTableName + "1", connection, 4);
				AssertNumberOfRecords(auditDbName, testSchemaName, testTableName + "2", connection, 4);
				AssertNumberOfRecords(auditDbName, testSchemaName, testTableName + "3", connection, 4);

				AssertNumberOfPartitions(auditDbName, testSchemaName, testTableName + "1", connection, 9);
				AssertNumberOfPartitions(auditDbName, testSchemaName, testTableName + "2", connection, 9);
				AssertNumberOfPartitions(auditDbName, testSchemaName, testTableName + "3", connection, 9);

				AssertPartitionRangeValues(auditDbName, testSchemaName, testTableName + "1", connection, 4, 4);
				AssertPartitionRangeValues(auditDbName, testSchemaName, testTableName + "2", connection, 4, 4);
				AssertPartitionRangeValues(auditDbName, testSchemaName, testTableName + "3", connection, 4, 4);
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestRecreatePartitionsAndPurgeOldDataFuturePeriods()
		{
			var auditDbName = Db.AuditDatabaseName;

			using (var connection = Db.NewAdminConnection())
			{
				RemoveAuditTablesFromConfiguration(connection, auditDbName);
				TruncateLsnTimeMappingTable(connection, auditDbName);
				CreateTestTable(connection, auditDbName, testTableName + "1");
				CreateTestTable(connection, auditDbName, testTableName + "2");

				InsertLsnTimeMappingRows(connection, auditDbName, -3, 4);
				InsertTestAuditRecords(connection, auditDbName, testSchemaName, testTableName + "1");
				InsertTestAuditRecords(connection, auditDbName, testSchemaName, testTableName + "2");

				Execute_usp_RecreatePartitionsAndPurgeOldData(auditDbName, connection, 12, 3);

				CombineAssertions(() =>
				{
					AssertNumberOfRecords(auditDbName, testSchemaName, testTableName + "1", connection, 4);
					AssertNumberOfRecords(auditDbName, testSchemaName, testTableName + "2", connection, 4);

					AssertNumberOfPartitions(auditDbName, testSchemaName, testTableName + "1", connection, 9);
					AssertNumberOfPartitions(auditDbName, testSchemaName, testTableName + "2", connection, 9);

					AssertPartitionRangeValues(auditDbName, testSchemaName, testTableName + "1", connection, 4, 4);
					AssertPartitionRangeValues(auditDbName, testSchemaName, testTableName + "2", connection, 4, 4);
				});

				Execute_usp_RecreatePartitionsAndPurgeOldData(auditDbName, connection, 12, 5);

				CombineAssertions(() =>
				{
					AssertNumberOfRecords(auditDbName, testSchemaName, testTableName + "1", connection, 4);
					AssertNumberOfRecords(auditDbName, testSchemaName, testTableName + "2", connection, 4);

					AssertNumberOfPartitions(auditDbName, testSchemaName, testTableName + "1", connection, 11);
					AssertNumberOfPartitions(auditDbName, testSchemaName, testTableName + "2", connection, 11);

					AssertPartitionRangeValues(auditDbName, testSchemaName, testTableName + "1", connection, 4, 6);
					AssertPartitionRangeValues(auditDbName, testSchemaName, testTableName + "2", connection, 4, 6);
				});
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestRecreatePartitionsAndPurgeOldDataRePartitionStoppedLongPeriod()
		{
			var auditDbName = Db.AuditDatabaseName;

			using (var connection = Db.NewAdminConnection())
			{
				RemoveAuditTablesFromConfiguration(connection, auditDbName);
				TruncateLsnTimeMappingTable(connection, auditDbName);
				CreateTestTable(connection, auditDbName, testTableName + "1");
				CreateTestTable(connection, auditDbName, testTableName + "2");

				InsertLsnTimeMappingRows(connection, auditDbName, -3, 4);
				InsertTestAuditRecords(connection, auditDbName, testSchemaName, testTableName + "1");
				InsertTestAuditRecords(connection, auditDbName, testSchemaName, testTableName + "2");

				Execute_usp_RecreatePartitionsAndPurgeOldData(auditDbName, connection, 12, 3);

				CombineAssertions(() =>
				{
					AssertNumberOfRecords(auditDbName, testSchemaName, testTableName + "1", connection, 4);
					AssertNumberOfRecords(auditDbName, testSchemaName, testTableName + "2", connection, 4);

					AssertNumberOfPartitions(auditDbName, testSchemaName, testTableName + "1", connection, 9);
					AssertNumberOfPartitions(auditDbName, testSchemaName, testTableName + "2", connection, 9);

					AssertPartitionRangeValues(auditDbName, testSchemaName, testTableName + "1", connection, 4, 4);
					AssertPartitionRangeValues(auditDbName, testSchemaName, testTableName + "2", connection, 4, 4);
				});

				InsertLsnTimeMappingRows(connection, auditDbName, 2, 4);
				InsertTestAuditRecords(connection, auditDbName, testSchemaName, testTableName + "1");
				InsertTestAuditRecords(connection, auditDbName, testSchemaName, testTableName + "2");

				Execute_usp_RecreatePartitionsAndPurgeOldData(auditDbName, connection, 12, 5);

				CombineAssertions(() =>
				{
					AssertNumberOfRecords(auditDbName, testSchemaName, testTableName + "1", connection, 8);
					AssertNumberOfRecords(auditDbName, testSchemaName, testTableName + "2", connection, 8);

					AssertNumberOfPartitions(auditDbName, testSchemaName, testTableName + "1", connection, 11);
					AssertNumberOfPartitions(auditDbName, testSchemaName, testTableName + "2", connection, 11);

					AssertPartitionRangeValues(auditDbName, testSchemaName, testTableName + "1", connection, 8, 2);
					AssertPartitionRangeValues(auditDbName, testSchemaName, testTableName + "2", connection, 8, 2);
				});
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestRecreatePartitionsAndPurgeOldPurgingWorks()
		{
			var auditDbName = Db.AuditDatabaseName;

			using (var connection = Db.NewAdminConnection())
			{
				RemoveAuditTablesFromConfiguration(connection, auditDbName);
				TruncateLsnTimeMappingTable(connection, auditDbName);
				CreateTestTable(connection, auditDbName, testTableName + "1");
				CreateTestTable(connection, auditDbName, testTableName + "2");

				InsertLsnTimeMappingRows(connection, auditDbName, monthOffset: -12, rowsToInsert: 13);
				InsertTestAuditRecords(connection, auditDbName, testSchemaName, testTableName + "1");
				InsertTestAuditRecords(connection, auditDbName, testSchemaName, testTableName + "2");

				Execute_usp_RecreatePartitionsAndPurgeOldData(auditDbName, connection, 12, 3);

				CombineAssertions(() =>
				{
					AssertNumberOfRecords(auditDbName, testSchemaName, testTableName + "1", connection, 13);
					AssertNumberOfRecords(auditDbName, testSchemaName, testTableName + "2", connection, 13);

					AssertNumberOfPartitions(auditDbName, testSchemaName, testTableName + "1", connection, 18);
					AssertNumberOfPartitions(auditDbName, testSchemaName, testTableName + "2", connection, 18);

					AssertPartitionRangeValues(auditDbName, testSchemaName, testTableName + "1", connection, 13, 4);
					AssertPartitionRangeValues(auditDbName, testSchemaName, testTableName + "2", connection, 13, 4);
				});

				Execute_usp_RecreatePartitionsAndPurgeOldData(auditDbName, connection, 6, 3);

				CombineAssertions(() =>
				{
					AssertNumberOfRecords(auditDbName, testSchemaName, testTableName + "1", connection, 7);
					AssertNumberOfRecords(auditDbName, testSchemaName, testTableName + "2", connection, 7);

					AssertNumberOfPartitions(auditDbName, testSchemaName, testTableName + "1", connection, 12);
					AssertNumberOfPartitions(auditDbName, testSchemaName, testTableName + "2", connection, 12);

					AssertPartitionRangeValues(auditDbName, testSchemaName, testTableName + "1", connection, 7, 4);
					AssertPartitionRangeValues(auditDbName, testSchemaName, testTableName + "2", connection, 7, 4);

					AssertPurging(auditDbName, testSchemaName, testTableName + "1", connection, retentionPeriod: 6, expectedOldDataCount: 0, expectedLsnTimeMappingCount: 0, expectedRecordLsnMappingCount: 7);
					AssertPurging(auditDbName, testSchemaName, testTableName + "2", connection, retentionPeriod: 6, expectedOldDataCount: 0, expectedLsnTimeMappingCount: 0, expectedRecordLsnMappingCount: 7);
				});
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestRecreatePartitionsAndPurgeOldDataIndexReorganized()
		{
			var auditDbName = Db.AuditDatabaseName;

			using (var connection = Db.NewAdminConnection())
			{
				RemoveAuditTablesFromConfiguration(connection, auditDbName);
				TruncateLsnTimeMappingTable(connection, auditDbName);
				CreateTestTable(connection, auditDbName, testTableName + "1");
				CreateTestTable(connection, auditDbName, testTableName + "2");

				InsertLsnTimeMappingRows(connection, auditDbName, monthOffset: -3, rowsToInsert: 4);
				InsertTestAuditRecords(connection, auditDbName, testSchemaName, testTableName + "1");
				InsertTestAuditRecords(connection, auditDbName, testSchemaName, testTableName + "2");

				Execute_usp_RecreatePartitionsAndPurgeOldData(auditDbName, connection, 12, 3);
				Execute_ReorganizeCCI(auditDbName, testSchemaName, testTableName + "1", connection, 4);
				Execute_ReorganizeCCI(auditDbName, testSchemaName, testTableName + "2", connection, 4);
			}
		}

		public void TestRecreatePartitionsAndPurgeOldDataPartitionFunctionSchemeExistence()
		{
			using (var connection = Db.NewAdminConnection(Db.AuditDatabaseName))
			{
				CombineAssertions(() =>
				{
					var functionCount = Convert.ToInt32(connection.ExecuteScalar("SELECT count(*) FROM sys.partition_functions pf WHERE pf.name = 'PF_LsnPeriodFunction'"));
					AssertEquals("PF_LsnPeriodFunction function should not exist.", 0, functionCount);

					var schemeCount = Convert.ToInt32(connection.ExecuteScalar("SELECT count(*) FROM sys.partition_schemes ps WHERE ps.name = 'PS_AuditPartitionScheme'"));
					AssertEquals("PS_AuditPartitionScheme scheme should not exist.", 0, schemeCount);
				});
			}
		}

		#endregion

		#region Implementation

		const string testSchemaName = "TestSchema";
		const string testTableName = "TestTable_AuditPartitioning";

		#region Test Table Creation

		void TruncateLsnTimeMappingTable(DbConnection connection, string auditDbName)
		{
			using (((ICurrentDbControl)connection).UseDatabase(auditDbName))
			{
				connection.ExecuteNonQuery(string.Format(CultureInfo.InvariantCulture, "TRUNCATE TABLE [{0}].[LsnTimeMapping]", BiConstants.BiAdminSchemaName));
			}
		}

		void RemoveAuditTablesFromConfiguration(DbConnection connection, string auditDbName)
		{
			using (((ICurrentDbControl)connection).UseDatabase(auditDbName))
			{
				connection.ExecuteNonQuery(string.Format(CultureInfo.InvariantCulture, "TRUNCATE TABLE [{0}].[TableConfiguration]; TRUNCATE TABLE [{0}].[TableState]", BiConstants.BiAdminSchemaName));
			}
		}

		void CreateTestTable(AdminConnection connection, string auditDbName, string testTableName)
		{
			CreateTestTableInMainDb(connection, testTableName);
			CreateTestTableInAuditDb(connection, auditDbName, testTableName);
		}

		void CreateTestColumnStoreTable(AdminConnection connection, string auditDbName, string testTableName)
		{
			CreateTestTableInMainDb(connection, testTableName);
			CreateTestTableInAuditDb(connection, auditDbName, testTableName);

			using (((ICurrentDbControl)connection).UseDatabase(auditDbName))
			{
				var sqlText = @"
IF EXISTS (SELECT name FROM sys.indexes WHERE name = N'cci_{0}_{1}')
    DROP INDEX cci_{0}_{1} ON [{0}].[{1}];
CREATE CLUSTERED COLUMNSTORE INDEX cci_{0}_{1} ON [{0}].[{1}];";

				connection.ExecuteNonQuery(string.Format(sqlText, testSchemaName, testTableName, BiConstants.BiAdminSchemaName));
			}
		}

		void CreateTestRowStorePartitionedIndexTable(AdminConnection connection, string auditDbName, string testTableName)
		{
			CreateTestTableInMainDb(connection, testTableName);
			CreateTestTableInAuditDb(connection, auditDbName, testTableName);

			using (((ICurrentDbControl)connection).UseDatabase(auditDbName))
			{
				var sqlText = @"
IF EXISTS (SELECT * FROM sys.partition_schemes  
    WHERE name = 'myPartitionScheme')  
DROP PARTITION SCHEME myPartitionScheme;  

-- drop the partition function if it exists
IF( EXISTS( SELECT * FROM sys.partition_functions WHERE name = 'ValuePartitionFunction' ) )
BEGIN
	DROP PARTITION FUNCTION ValuePartitionFunction;
END;
 
CREATE PARTITION FUNCTION ValuePartitionFunction (int)
	AS RANGE RIGHT FOR VALUES (1, 2, 3, 4, 5, 6);

CREATE PARTITION SCHEME myPartitionScheme 
	AS PARTITION ValuePartitionFunction ALL TO ([PRIMARY]);

IF EXISTS (SELECT name FROM sys.indexes WHERE name = N'cci_{0}_{1}')
	DROP INDEX cci_{0}_{1} ON [{0}].[{1}];

CREATE CLUSTERED INDEX cci_{0}_{1} ON [{0}].[{1}] (Value)
	WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, 
	ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) 
	ON myPartitionScheme(Value);";
				connection.ExecuteNonQuery(string.Format(sqlText, testSchemaName, testTableName, BiConstants.BiAdminSchemaName));
			}
		}

		void CreateTestTableInMainDb(AdminConnection connection, string testTableName)
		{
			var cdcTable = new CdcTable(testSchemaName, testTableName);
			if (cdcTable.IsCdcEnabled(connection))
			{
				cdcTable.DisableCdc(connection, string.Format("{0}_{1}", testSchemaName, testTableName));
			}

			var sqlText = @"
IF NOT EXISTS(SELECT null FROM sys.schemas WHERE name = '{0}')
	EXEC ('CREATE SCHEMA [{0}]');

IF EXISTS (SELECT null FROM sys.tables WHERE name = '{1}')
	DROP TABLE [{0}].[{1}]
CREATE TABLE [{0}].[{1}] (
	ID uniqueidentifier NOT NULL PRIMARY KEY,
	Value int
)";

			connection.ExecuteNonQuery(string.Format(sqlText, testSchemaName, testTableName));
		}

		void CreateTestTableInAuditDb(DbConnection connection, string auditDbName, string testTableName)
		{
			using (((ICurrentDbControl)connection).UseDatabase(auditDbName))
			{
				var sqlText = @"
IF NOT EXISTS(SELECT null FROM sys.schemas WHERE name = '{0}')
	EXEC ('CREATE SCHEMA [{0}]');

IF EXISTS (SELECT null FROM sys.tables t inner join sys.schemas s on t.schema_id = s.schema_id WHERE t.name = '{1}' and s.name = '{0}')
	DROP TABLE [{0}].[{1}]
CREATE TABLE [{0}].[{1}] (
	[__$start_lsn] binary(10) NOT NULL,
	[__$seqval] binary(10) NOT NULL,
	[__$operation] int NOT NULL,
	[__$update_mask] varbinary(128) NOT NULL,
	[__$lsn_period] smallint DEFAULT (0) NOT NULL,
	ID uniqueidentifier,
	Value int,
	[__$command_id] int NOT NULL DEFAULT 0
)

IF NOT EXISTS (SELECT NULL FROM [{2}].TableConfiguration WHERE SourceSchemaName = '{0}' AND SourceTableName = '{1}')
	INSERT INTO [{2}].TableConfiguration (SourceSchemaName, SourceTableName, TableColumnList, PkName)
	VALUES('{0}', '{1}', '__$start_lsn,__$seqval,__$operation,__$update_mask,ID,Value,__$command_id', 'ID')

IF NOT EXISTS (SELECT NULL FROM [{2}].TableState WHERE SourceSchemaName = '{0}' AND SourceTableName = '{1}')
	INSERT INTO [{2}].TableState (SourceSchemaName, SourceTableName, CurrentState)
	VALUES('{0}','{1}', 'New')";
				connection.ExecuteNonQuery(string.Format(sqlText, testSchemaName, testTableName, BiConstants.BiAdminSchemaName));
			}
		}

		#endregion

		#region Assertions

		void AssertPartitionRangeValues(string auditDbName, string tableSchema, string tableName, AdminConnection connection, int dataPartitionRange, int extraPartitionRange)
		{
			using (((ICurrentDbControl)connection).UseDatabase(auditDbName))
			{
				string sqlTextRecordRange = string.Format(CultureInfo.InvariantCulture, @"
			SELECT count(*)
			FROM sys.partition_functions f
			INNER JOIN sys.partition_schemes s ON s.function_id = f.function_id
			INNER JOIN sys.partition_range_values prv ON prv.function_id = f.function_id
			WHERE f.name = 'PF_LsnPeriodFunction' and CAST(prv.value AS SMALLINT) in (select distinct [__$lsn_period] from [{0}].[{1}])",
					/*0*/tableSchema,
					/*1*/tableName
				);
				var dataRecordRangeCount = Convert.ToInt32(connection.ExecuteScalar(sqlTextRecordRange));
				AssertEquals("Number of partitions ranges for data mismatched after RecreatePartitionsAndPurgeOldData", dataPartitionRange, dataRecordRangeCount);

				string sqlTextExtraRange = string.Format(CultureInfo.InvariantCulture, @"
			SELECT count(*)
			FROM sys.partition_functions f
			INNER JOIN sys.partition_schemes s ON s.function_id = f.function_id
			INNER JOIN sys.partition_range_values prv ON prv.function_id = f.function_id
			WHERE f.name = 'PF_LsnPeriodFunction' and CAST(prv.value AS SMALLINT) not in (select distinct [__$lsn_period] from [{0}].[{1}])",
					/*0*/tableSchema,
					/*1*/tableName
				);
				var extraRecordRangeCount = Convert.ToInt32(connection.ExecuteScalar(sqlTextExtraRange));
				AssertEquals("Number of extra partitions ranges mismatched after RecreatePartitionsAndPurgeOldData", extraPartitionRange, extraRecordRangeCount);

				string sqlTextRecordCheckSum = string.Format(CultureInfo.InvariantCulture, @"select sum(CHECK_SUM) as CHECK_SUM from (select distinct [__$lsn_period] as CHECK_SUM from [{0}].[{1}]) as Tab", tableSchema, tableName);
				var checkSumObj = connection.ExecuteScalar(sqlTextRecordCheckSum);
				if (checkSumObj != DBNull.Value)
				{
					var recordCheckSum = Convert.ToInt32(connection.ExecuteScalar(sqlTextRecordCheckSum));

					string sqlTextExtraPartitionCheckSum = string.Format(CultureInfo.InvariantCulture, @"
			SELECT sum(CAST(prv.value AS SMALLINT)) as CHECK_SUM
			FROM sys.partition_functions f
			INNER JOIN sys.partition_schemes s ON s.function_id = f.function_id
			INNER JOIN sys.partition_range_values prv ON prv.function_id = f.function_id
			WHERE f.name = 'PF_LsnPeriodFunction' and CAST(prv.value AS SMALLINT) not in (select distinct [__$lsn_period] from [{0}].[{1}])",
						/*0*/tableSchema,
						/*1*/tableName
					);
					var extraPartitionCheckSum = Convert.ToInt32(connection.ExecuteScalar(sqlTextExtraPartitionCheckSum));

					string sqlTextAllPartitionCheckSum = string.Format(CultureInfo.InvariantCulture, @"
			SELECT sum(CAST(prv.value AS SMALLINT)) as CHECK_SUM
			FROM sys.partition_functions f
			INNER JOIN sys.partition_schemes s ON s.function_id = f.function_id
			INNER JOIN sys.partition_range_values prv ON prv.function_id = f.function_id
			WHERE f.name = 'PF_LsnPeriodFunction';",
						/*0*/tableSchema,
						/*1*/tableName
					);
					var allPartitionCheckSum = Convert.ToInt32(connection.ExecuteScalar(sqlTextAllPartitionCheckSum));
					AssertEquals("Check sum of period of data mismatched with check sum of period of partitions after RecreatePartitionsAndPurgeOldData", allPartitionCheckSum, (recordCheckSum + extraPartitionCheckSum));
				}
			}
		}

		void AssertNumberOfPartitions(string auditDbName, string tableSchema, string tableName, AdminConnection connection, int expectedPartitionCount)
		{
			using (((ICurrentDbControl)connection).UseDatabase(auditDbName))
			{
				string sqlText = string.Format(CultureInfo.InvariantCulture, @"
select count(*)
from sys.indexes i
inner join sys.tables t on i.object_id = t.object_id
inner join sys.schemas s on s.schema_id = t.schema_id
inner join sys.partitions p on p.object_id = t.object_id and p.index_id = i.index_id
where i.type = 5 and t.name = '{1}' and s.name = '{0}'
				;",
				/*0*/tableSchema,
				/*1*/tableName
			);
				AssertEquals("Number of partitions mismatched after RecreatePartitionsAndPurgeOldData", expectedPartitionCount, connection.ExecuteScalar(sqlText));
			}
		}

		void AssertNumberOfRecords(string auditDbName, string tableSchema, string tableName, AdminConnection connection, int expectedRecordCount)
		{
			string sqlText = string.Format(CultureInfo.InvariantCulture, @"
				select count(*) from [{0}].[{1}].[{2}];",
				/*0*/auditDbName,
				/*1*/tableSchema,
				/*2*/tableName
			);
			AssertEquals("Number of records mismatched after RecreatePartitionsAndPurgeOldData", expectedRecordCount, connection.ExecuteScalar(sqlText));
		}

		void AssertPurging(string auditDbName, string tableSchema, string tableName, AdminConnection connection, int retentionPeriod, int expectedOldDataCount, int expectedLsnTimeMappingCount, int expectedRecordLsnMappingCount)
		{
			using (((ICurrentDbControl)connection).UseDatabase(auditDbName))
			{
				string oldDataQuery = string.Format(CultureInfo.InvariantCulture, @"
			DECLARE @MyDate datetime = DATEADD(month, -{0}, GETUTCDATE());
			DECLARE @MyPeriod smallint = convert(smallint, ((YEAR(@MyDate) - 2000) * 100) + MONTH(@MyDate));

			SELECT count(*)
			FROM sys.partition_functions f
			INNER JOIN sys.partition_schemes s ON s.function_id = f.function_id
			INNER JOIN sys.partition_range_values prv ON prv.function_id = f.function_id
			WHERE f.name = 'PF_LsnPeriodFunction' and CAST(prv.value AS SMALLINT) < @MyPeriod;", retentionPeriod);
				var oldDataCount = Convert.ToInt32(connection.ExecuteScalar(oldDataQuery));
				AssertEquals("Partition older than purging period not purged after RecreatePartitionsAndPurgeOldData", expectedOldDataCount, oldDataCount);

				string oldLsnTimeMappingQuery = string.Format(CultureInfo.InvariantCulture, @"
			DECLARE @MyDate datetime = DATEADD(month, -{1}, GETUTCDATE());

			SELECT count(*)
			FROM {0}.LsnTimeMapping
			where cast(TranEndTimeUtc as date) < cast(@MyDate as date);", BiConstants.BiAdminSchemaName, retentionPeriod);
				var oldLsnTimeMappingCount = Convert.ToInt32(connection.ExecuteScalar(oldLsnTimeMappingQuery));
				AssertEquals("Lsn time mapping older than purging period not purged after RecreatePartitionsAndPurgeOldData", expectedLsnTimeMappingCount, oldLsnTimeMappingCount);

				string recordLsnTimeMappingQuery = string.Format(CultureInfo.InvariantCulture, @"
			SELECT count(*)
			FROM {3}.LsnTimeMapping
			where StartLsn in (
			select distinct __$start_lsn
			from [{0}].[{1}].[{2}]
			);",
					Db.AuditDatabaseName,
					tableSchema,
					tableName,
					BiConstants.BiAdminSchemaName
				);
				var recordLsnMappingCount = Convert.ToInt32(connection.ExecuteScalar(recordLsnTimeMappingQuery));
				AssertEquals("Lsn time mapping older than purging period not purged after RecreatePartitionsAndPurgeOldData", expectedRecordLsnMappingCount, recordLsnMappingCount);
			}
		}

		#endregion

		#region Script Execution

		void Execute_usp_RecreatePartitionsAndPurgeOldData(string auditDbName, AdminConnection connection, int dataRetentionMonth = 12, int? numberOfFuturePeriods = null)
		{
			string sqlText = string.Format(CultureInfo.InvariantCulture, @"{0}.{1}.usp_RecreatePartitionsAndPurgeOldData",
				/*0*/auditDbName,
				/*1*/BiConstants.BiAdminSchemaName
			);

			using (((ICurrentDbControl)connection).UseDatabase(auditDbName))
			using (var cmd = connection.Command(sqlText))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.AddParameter("@DataRetentionMonth", SqlDbType.Int, dataRetentionMonth);
				cmd.AddParameter("@PrintMessages", SqlDbType.Bit, 1);
				if (numberOfFuturePeriods != null)
				{
					cmd.AddParameter("@NumberOfFuturePeriods", SqlDbType.Int, numberOfFuturePeriods.Value);
				}
				cmd.AddParameter("@CurrentTimeUTC", SqlDbType.DateTime, DBNull.Value);
				cmd.AddOutputParameter("@ErrorCode", SqlDbType.Int, 0, 0, 0, -1);
				cmd.AddOutputParameter("@ErrorMessage", SqlDbType.VarChar, -1, 0, 0, "");
				cmd.AddOutputParameter("@InfoMessage", SqlDbType.VarChar, -1, 0, 0, "");

				cmd.ExecuteNonQuery();
				var errorCode = cmd.GetParameterValue("@ErrorCode");
				var errorMessage = cmd.GetParameterValue("@ErrorMessage");
				var infoMessage = cmd.GetParameterValue("@InfoMessage");
				AssertEquals(errorMessage + " : " + infoMessage, 0, errorCode);
			}
		}

		void Execute_ReorganizeCCI(string auditDbName, string tableSchema, string tableName, AdminConnection connection, int noOfRecords)
		{
			using (((ICurrentDbControl)connection).UseDatabase(auditDbName))
			{
				string sqlText = string.Format(CultureInfo.InvariantCulture, @"
				ALTER INDEX cci_{0}_{1} ON {0}.{1} 
				REORGANIZE WITH (COMPRESS_ALL_ROW_GROUPS = ON)
				;",
					/*0*/tableSchema,
					/*1*/tableName
				);
				connection.ExecuteNonQuery(sqlText);

				string updateQuery = string.Format(CultureInfo.InvariantCulture, @"
				update {0}.{1} 
				set ID = ID,
					Value = Value
				SELECT @@ROWCOUNT;",
					/*0*/tableSchema,
					/*1*/tableName
				);
				int recordsUpdated = Convert.ToInt32(connection.ExecuteScalar(updateQuery));
				AssertEquals("No of updated records mismatched", noOfRecords, recordsUpdated);

				using (var cmd = connection.Command($"[{auditDbName}].[{BiConstants.BiAdminSchemaName}].[usp_IndexMaintenance]"))
				{
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.AddParameter("@timer_in_seconds", SqlDbType.BigInt, DBNull.Value);
					cmd.AddParameter("@print_messages", SqlDbType.Bit, 1);
					cmd.AddOutputParameter("@IsIndexRebuilt", SqlDbType.Int, 0, 0, 0, -1);
					cmd.AddOutputParameter("@ErrorCode", SqlDbType.Int, 0, 0, 0, -1);
					cmd.AddOutputParameter("@CdcHistorySummaryErrorCode", SqlDbType.Int, 0, 0, 0, null);

					cmd.ExecuteNonQuery();

					var errorCode = cmd.GetParameterValue("@ErrorCode");
					AssertEquals("Executing index maintenance failed", 0, errorCode);
				}

				string sqlAllIndexReorganized = string.Format(CultureInfo.InvariantCulture, @"
				select count(*)
				from sys.column_store_row_groups
				where object_name(object_id) = '{0}'
				and state_description = 'OPEN'
				;",
					/*1*/tableName
				);
				var openCount = Convert.ToInt32(connection.ExecuteScalar(sqlAllIndexReorganized));
				AssertEquals("All indexes not re-organized for usp_IndexMaintenance after RecreatePartitionsAndPurgeOldData", 0, openCount);
			}
		}

		#endregion

		#region Test Data Insertion

		void InsertLsnTimeMappingRows(AdminConnection connection, string auditDbName, int monthOffset, int rowsToInsert)
		{
			if (rowsToInsert > 0)
			{
				var sqlText = string.Format(CultureInfo.InvariantCulture, @"
				DECLARE @TransactionDate datetime
				DECLARE @CurrentDate datetime = GETUTCDATE()");

				for (var counter = 0; counter < rowsToInsert; counter++)
				{
					var currentMonthOffset = counter + monthOffset;
					sqlText += string.Format(CultureInfo.InvariantCulture, @"
						SELECT @TransactionDate = DATEADD(month, {1}, @CurrentDate);
						INSERT [{0}].[LsnTimeMapping] (StartLsn, TranEndTimeUtc)
						VALUES (0x50 + {1}, @TransactionDate);",
						BiConstants.BiAdminSchemaName, currentMonthOffset);
				}

				using (((ICurrentDbControl)connection).UseDatabase(auditDbName))
				{
					connection.ExecuteNonQuery(sqlText);
				}
			}
		}

		void InsertTestAuditRecords(AdminConnection connection, string auditDbName, string tableSchema, string tableName)
		{
			using (((ICurrentDbControl)connection).UseDatabase(auditDbName))
			{
				var sqlText = string.Format(CultureInfo.InvariantCulture, @"
INSERT INTO [{1}].[{2}] ([__$lsn_period], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [ID], [Value])
SELECT convert(smallint, ((YEAR(TranEndTimeUtc) - 2000) * 100) + MONTH(TranEndTimeUtc)), StartLsn, 0x01, 2, 0x0, NEWID(), 1
FROM [{0}].[LsnTimeMapping]
WHERE StartLsn NOT IN (SELECT DISTINCT [__$start_lsn] FROM [{1}].[{2}])",
					BiConstants.BiAdminSchemaName,
					tableSchema,
					tableName
				);

				connection.ExecuteNonQuery(sqlText);
			}
		}

		#endregion

		#endregion
	}
}
