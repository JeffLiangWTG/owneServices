using System;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Tools.DuplicateDetector;
using Enterprise.AuditDataServices.MDM.Subscribers;
using Enterprise.AuditDataServices.Subscription.Common;
using Enterprise.AuditDataServices.Subscription.Testing;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.AuditDataServices.MDM.Testing
{
	[TestedType(typeof(GlbPersonRegCodeSubscriber))]
	class GlbPersonRegCodeSubscriberTest : ActualDataChangesAuditSubscriberTest
	{
		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestGlbPersonRegCodeProcessesChangesForNewRecords()
		{
			GlbPersonRegCodeProcessesChangesForNewRecordsImplementation(
				"INSERT [dbo].[GlbPerson] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], PER_PK, PER_FullName, PER_Passport) VALUES (0x10, 0x01, 2, 0x0, 1803, 1, '{0}', '{1}', '{2}')",
				"INSERT [biadmin].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x10, '2018-06-08'); INSERT INTO [biadmin].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES (0x10, 'dbo', 'GlbPerson', 1, 1803);",
				"Queued Deduplication in GlbPersonRegCodeSubscriber for [PER_PK:{0}]. Row: Added - {0} - {1} -  - ", CodeTypeIdentifier.Passport);
			GlbPersonRegCodeProcessesChangesForNewRecordsImplementation(
				"INSERT [dbo].[GlbPerson] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], PER_PK, PER_FullName, PER_DriversLicenseNumber) VALUES (0x10, 0x01, 2, 0x0, 1803, 1, '{0}', '{1}', '{2}')",
				"INSERT [biadmin].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x10, '2018-06-08'); INSERT INTO [biadmin].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES (0x10, 'dbo', 'GlbPerson', 1, 1803);",
				"Queued Deduplication in GlbPersonRegCodeSubscriber for [PER_PK:{0}]. Row: Added - {0} -  - {1} - ", CodeTypeIdentifier.License);
			GlbPersonRegCodeProcessesChangesForNewRecordsImplementation(
				"INSERT [dbo].[GlbPerson] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], PER_PK, PER_FullName, PER_BirthDate) VALUES (0x10, 0x01, 2, 0x0, 1803, 1, '{0}', '{1}', '{2}')",
				"INSERT [biadmin].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x10, '2018-06-08'); INSERT INTO [biadmin].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES (0x10, 'dbo', 'GlbPerson', 1, 1803);",
				"Queued Deduplication in GlbPersonRegCodeSubscriber for [PER_PK:{0}]. Row: Added - {0} -  -  - {1}", "");
		}

		void GlbPersonRegCodeProcessesChangesForNewRecordsImplementation(string insertIntoCDCQuery, string insertLsnMappingQuery, string expectedLog, string prefix)
		{
			var factory = new BusinessObjectFactory();
			var glbPerson = SubscriberTestUtilities.CreateTestGlbPerson(factory);
			factory.Save();

			using (var auditConnection = AuditTestHelper.GetAuditConnection())
			{
				var testSubscribers = new ActualDataChangesAuditSubscriber[] {
					NewDataChangeSubscriber()
				};

				AuditTestHelper.CleanupAuditTestData(auditConnection, testSubscribers);
				AuditTestHelper.ResetSubscriberControlWaterMark(auditConnection, testSubscribers);

				var patternMatchingRegCode = factory.Load<PatternMatchingRegCode>(new ZQuery(PatternMatchingRegCodeSchema.PMR_ParentId, glbPerson.PK)).ToList();
				AssertEquals(0, patternMatchingRegCode.Count);

				var regCode = "1991-02-03";
				var insertGlbPersonCDC = string.Format(CultureInfo.InvariantCulture, insertIntoCDCQuery, glbPerson.PK, glbPerson.PER_FullName, regCode);
				auditConnection.ExecuteNonQuery(insertGlbPersonCDC);

				auditConnection.ExecuteNonQuery(insertLsnMappingQuery);

				AuditTestHelper.RunNotificationCycleAndAssert(
					auditConnection,
					testSubscribers,
					"0x10", "2018-06-08 00:00:00.000",
					expectedLog: new BetterLogForTest(LogType.Information, string.Format(CultureInfo.InvariantCulture, expectedLog, glbPerson.PK, GetLogStr(regCode, string.IsNullOrEmpty(prefix))))
				);

				patternMatchingRegCode = factory.Load<PatternMatchingRegCode>(new ZQuery(PatternMatchingRegCodeSchema.PMR_ParentId, glbPerson.PK)).ToList();
				AssertEquals(1, patternMatchingRegCode.Count);

				var firstPatternMatchingRegCode = patternMatchingRegCode.Single();

				AssertEquals(GetHashedValue(regCode, prefix), firstPatternMatchingRegCode.PMR_HashedValue);
				AssertEquals(glbPerson.PK, firstPatternMatchingRegCode.PMR_PER);
				AssertEquals(GlbPersonSchema.Constants.Prefix, firstPatternMatchingRegCode.PMR_ParentTableCode);
				AssertEquals(glbPerson.PK, firstPatternMatchingRegCode.PMR_ParentId);
				AssertEquals(glbPerson.PER_RN_NKCountry, firstPatternMatchingRegCode.PMR_RN_NKCountryCode);
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestGlbPersonRegCodeNoPatternMatchingForPlaceholderRecords()
		{
			var insertIntoCDCQuery = @"
				INSERT [dbo].[GlbPerson] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], PER_PK, PER_FullName, PER_Passport) VALUES 
					(0x10, 0x01, 2, 0x0, 1803, 1, '{0}', '{1}', '{2}')
				INSERT [biadmin].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x10, '2018-06-08'); INSERT INTO [biadmin].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES (0x10, 'dbo', 'GlbPerson', 1, 1803);";

			var expectedLog = "Queued Deduplication in GlbPersonRegCodeSubscriber for [PER_PK:{0}]. Row: Added - {0} - {1} -  - ";

			var factory = new BusinessObjectFactory();
			var glbPerson = SubscriberTestUtilities.CreateTestGlbPerson(factory);
			factory.Save();

			using (var auditConnection = AuditTestHelper.GetAuditConnection())
			{
				var testSubscribers = new ActualDataChangesAuditSubscriber[] {
					NewDataChangeSubscriber()
				};

				AuditTestHelper.CleanupAuditTestData(auditConnection, testSubscribers);
				AuditTestHelper.ResetSubscriberControlWaterMark(auditConnection, testSubscribers);

				var patternMatchingRegCode = factory.Load<PatternMatchingRegCode>(new ZQuery(PatternMatchingRegCodeSchema.PMR_ParentId, glbPerson.PK)).ToList();
				AssertEquals(0, patternMatchingRegCode.Count);

				var regCode = "111";
				var insertGlbPersonCDC = string.Format(CultureInfo.InvariantCulture, insertIntoCDCQuery, glbPerson.PK, glbPerson.PER_FullName, regCode);
				auditConnection.ExecuteNonQuery(insertGlbPersonCDC);

				AuditTestHelper.RunNotificationCycleAndAssert(
					auditConnection,
					testSubscribers,
					"0x10", "2018-06-08 00:00:00.000",
					expectedLog: new BetterLogForTest(LogType.Information, string.Format(CultureInfo.InvariantCulture, expectedLog, glbPerson.PK, regCode))
				);

				patternMatchingRegCode = factory.Load<PatternMatchingRegCode>(new ZQuery(PatternMatchingRegCodeSchema.PMR_ParentId, glbPerson.PK)).ToList();
				AssertEquals(0, patternMatchingRegCode.Count);
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestGlbPersonRegCodeProcessesChangesForExistingRecords()
		{
			GlbPersonRegCodeProcessesChangesForExistingRecords(
				"INSERT [dbo].[GlbPerson] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], PER_PK, PER_FullName, PER_Passport) VALUES (0x10, 0x01, 3, 0x0, 1803, 1, '{0}', '{1}', '{2}')",
				"INSERT [dbo].[GlbPerson] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], PER_PK, PER_FullName, PER_Passport) VALUES (0x10, 0x01, 4, 0x0, 1803, 1, '{0}', '{1}', '{2}')",
				"INSERT [biadmin].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x10, '2018-06-08'); INSERT INTO [biadmin].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES (0x10, 'dbo', 'GlbPerson', 2, 1803);",
				"Queued Deduplication in GlbPersonRegCodeSubscriber for [PER_PK:{0}]. Row: Modified - {0} - {1} -  - ", CodeTypeIdentifier.Passport);
			GlbPersonRegCodeProcessesChangesForExistingRecords(
				"INSERT [dbo].[GlbPerson] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], PER_PK, PER_FullName, PER_DriversLicenseNumber) VALUES (0x10, 0x01, 3, 0x0, 1803, 1, '{0}', '{1}', '{2}')",
				"INSERT [dbo].[GlbPerson] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], PER_PK, PER_FullName, PER_DriversLicenseNumber) VALUES (0x10, 0x01, 4, 0x0, 1803, 1, '{0}', '{1}', '{2}')",
				"INSERT [biadmin].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x10, '2018-06-08'); INSERT INTO [biadmin].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES (0x10, 'dbo', 'GlbPerson', 2, 1803);",
				"Queued Deduplication in GlbPersonRegCodeSubscriber for [PER_PK:{0}]. Row: Modified - {0} -  - {1} - ", CodeTypeIdentifier.License);
			GlbPersonRegCodeProcessesChangesForExistingRecords(
				"INSERT [dbo].[GlbPerson] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], PER_PK, PER_FullName, PER_BirthDate) VALUES (0x10, 0x01, 3, 0x0, 1803, 1, '{0}', '{1}', '{2}')",
				"INSERT [dbo].[GlbPerson] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], PER_PK, PER_FullName, PER_BirthDate) VALUES (0x10, 0x01, 4, 0x0, 1803, 1, '{0}', '{1}', '{2}')",
				"INSERT [biadmin].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x10, '2018-06-08'); INSERT INTO [biadmin].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES (0x10, 'dbo', 'GlbPerson', 2, 1803);",
				"Queued Deduplication in GlbPersonRegCodeSubscriber for [PER_PK:{0}]. Row: Modified - {0} -  -  - {1}", "");
		}

		void GlbPersonRegCodeProcessesChangesForExistingRecords(string insertIntoCDCQuery, string insertIntoCDCUpdateQuery, string insertLsnMappingQuery, string expectedLog, string prefix)
		{
			GlbPersonRegCodeProcessesChangesForExistingRecords(insertIntoCDCQuery, insertIntoCDCUpdateQuery, insertLsnMappingQuery, expectedLog, prefix, true);
			GlbPersonRegCodeProcessesChangesForExistingRecords(insertIntoCDCQuery, insertIntoCDCUpdateQuery, insertLsnMappingQuery, expectedLog, prefix, false);
		}

		void GlbPersonRegCodeProcessesChangesForExistingRecords(string insertIntoCDCQuery, string insertIntoCDCUpdateQuery, string insertLsnMappingQuery, string expectedLog, string prefix, bool createOriginalPatternData)
		{
			var factory = new BusinessObjectFactory();
			var glbPerson = SubscriberTestUtilities.CreateTestGlbPerson(factory);
			var originalValue = "1990-01-01";
			var originalHashedValue = GetHashedValue(originalValue, prefix);

			if (createOriginalPatternData)
			{
				SubscriberTestUtilities.CreatePatternMatchingBusinessObjectForPerson<PatternMatchingRegCode>(factory, glbPerson.PK, originalHashedValue, GlbPersonSchema.Constants.Prefix, glbPerson.PK, glbPerson.PER_RN_NKCountry);
			}

			factory.Save();

			using (var auditConnection = AuditTestHelper.GetAuditConnection())
			{
				var testSubscribers = new ActualDataChangesAuditSubscriber[] {
					NewDataChangeSubscriber()
				};

				AuditTestHelper.CleanupAuditTestData(auditConnection, testSubscribers);
				AuditTestHelper.ResetSubscriberControlWaterMark(auditConnection, testSubscribers);

				var insertGlbPersonOriginalCDC = string.Format(CultureInfo.InvariantCulture, insertIntoCDCQuery, glbPerson.PK, glbPerson.PER_FullName, originalValue);
				auditConnection.ExecuteNonQuery(insertGlbPersonOriginalCDC);

				var regCode = "1991-02-03";
				var insertGlbPersonCurrentCDC = string.Format(CultureInfo.InvariantCulture, insertIntoCDCUpdateQuery, glbPerson.PK, glbPerson.PER_FullName, regCode);
				auditConnection.ExecuteNonQuery(insertGlbPersonCurrentCDC);

				auditConnection.ExecuteNonQuery(insertLsnMappingQuery);

				AuditTestHelper.RunNotificationCycleAndAssert(
					auditConnection,
					testSubscribers,
					"0x10", "2018-06-08 00:00:00.000",
					expectedLog: new BetterLogForTest(LogType.Information, string.Format(CultureInfo.InvariantCulture, expectedLog, glbPerson.PK, GetLogStr(regCode, string.IsNullOrEmpty(prefix))))
				);

				var patternMatchingRegCode = factory.Load<PatternMatchingRegCode>(new ZQuery(PatternMatchingRegCodeSchema.PMR_ParentId, glbPerson.PK)).ToList();
				AssertEquals(1, patternMatchingRegCode.Count);

				var firstPatternMatchingRegCode = patternMatchingRegCode.Single();

				AssertEquals(GetHashedValue(regCode, prefix), firstPatternMatchingRegCode.PMR_HashedValue);
				AssertEquals(glbPerson.PK, firstPatternMatchingRegCode.PMR_PER);
				AssertEquals(GlbPersonSchema.Constants.Prefix, firstPatternMatchingRegCode.PMR_ParentTableCode);
				AssertEquals(glbPerson.PK, firstPatternMatchingRegCode.PMR_ParentId);
				AssertEquals(glbPerson.PER_RN_NKCountry, firstPatternMatchingRegCode.PMR_RN_NKCountryCode);
			}
		}

		readonly DateTime beginDate = new DateTime(1753, 1, 1);

		int GetHashedValue(string valueToHash, string prefix)
		{
			if (string.IsNullOrEmpty(prefix))
			{
				valueToHash = (Convert.ToDateTime(valueToHash) - beginDate).Days.ToString();
			}

			return TextStandardizerHelper.ComputeStringHashFast(prefix + valueToHash);
		}

		string GetLogStr(string originalValue, bool isBirthdate)
		{
			if (isBirthdate)
			{
				return Convert.ToDateTime(originalValue).ToString("MM/dd/yyyy");
			}

			return originalValue;
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestGlbPersonRegCodeDoesNotProcessChangesForNonSubscribedColumns()
		{
			var factory = new BusinessObjectFactory();
			var glbPerson = SubscriberTestUtilities.CreateTestGlbPerson(factory);
			SubscriberTestUtilities.CreatePatternMatchingBusinessObjectForPerson<PatternMatchingRegCode>(factory, glbPerson.PK, 0, GlbPersonSchema.Constants.Prefix, glbPerson.PK, glbPerson.PER_RN_NKCountry);

			factory.Save();

			using (var auditConnection = AuditTestHelper.GetAuditConnection())
			{
				var testSubscribers = new ActualDataChangesAuditSubscriber[] {
					NewDataChangeSubscriber()
				};
				AuditTestHelper.ResetSubscriberControlWaterMark(auditConnection, testSubscribers);

				var insertGlbPersonOriginalCDC = string.Format(CultureInfo.InvariantCulture, @"INSERT [dbo].[GlbPerson] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], PER_PK, PER_FullName) VALUES 
					(0x10, 0x01, 3, 0x0, 0, 1, '{0}', '{1}');", glbPerson.PK, glbPerson.PER_FullName);
				auditConnection.ExecuteNonQuery(insertGlbPersonOriginalCDC);

				var insertGlbPersonCurrentCDC = string.Format(CultureInfo.InvariantCulture, @"INSERT [dbo].[GlbPerson] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], PER_PK, PER_FullName, PER_Gender) VALUES 
					(0x10, 0x01, 4, 0x0, 0, 1, '{0}', '{1}', '{2}');", glbPerson.PK, glbPerson.PER_FullName, "F");
				auditConnection.ExecuteNonQuery(insertGlbPersonCurrentCDC);

				AuditTestHelper.RunNotificationCycleAndAssert(
					auditConnection,
					testSubscribers,
					"0x10", "2018-06-08 00:00:00.000",
					expectedLog: null
				);

				var patternMatchingRegCode = factory.Load<PatternMatchingRegCode>(new ZQuery(PatternMatchingRegCodeSchema.PMR_ParentId, glbPerson.PK)).ToList();
				AssertEquals(1, patternMatchingRegCode.Count);

				var firstPatternMatchingRegCode = patternMatchingRegCode.Single();
				AssertEquals(0, firstPatternMatchingRegCode.PMR_HashedValue);
			}
		}

		public override void TestCustomFilter()
		{
			var subscriber = new GlbPersonRegCodeSubscriber();

			var table = GetTestDataTable();
			var rowToKeep = GetPopulatedDataRow(table, false);
			var rowToDelete = GetPopulatedDataRow(table, true);
			table.AcceptChanges();

			AssertEquals(2, table.Rows.Count);

			RunCustomFilter(rowToKeep, subscriber);
			RunCustomFilter(rowToDelete, subscriber);
			table.AcceptChanges();

			AssertEquals(1, table.Rows.Count);
			AssertCollectionContains(rowToKeep, table.Rows);
			AssertCollectionNotContains(rowToDelete, table.Rows);
		}

		DataRow GetPopulatedDataRow(DataTable table, bool shouldBeFiltered)
		{
			var row = table.NewRow();
			if (shouldBeFiltered)
			{
				row[GlbPersonSchema.PER_Passport.Name] = DBNull.Value;
				row[GlbPersonSchema.PER_DriversLicenseNumber.Name] = DBNull.Value;
				row[GlbPersonSchema.PER_BirthDate.Name] = DBNull.Value;
			}
			else
			{
				row[GlbPersonSchema.PER_Passport.Name] = "Name";
				row[GlbPersonSchema.PER_DriversLicenseNumber.Name] = "LicenseNumber";
				row[GlbPersonSchema.PER_BirthDate.Name] = DateTime.Now;
			}
			table.Rows.Add(row);
			return row;
		}

		protected override DataTable GetTestDataTable()
		{
			var changeTable = new DataTable();
			var tableColumns = new SchemaColumn[] { GlbPersonSchema.PER_Passport, GlbPersonSchema.PER_DriversLicenseNumber, GlbPersonSchema.PER_BirthDate };
			foreach (var column in tableColumns)
			{
				changeTable.Columns.Add(column.Name, column.DotNetType);
			}
			return changeTable;
		}
	}
}
