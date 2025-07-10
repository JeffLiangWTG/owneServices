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
	[TestedType(typeof(GlbPersonEmailSubscriber))]
	class GlbPersonEmailSubscriberTest : ActualDataChangesAuditSubscriberTest
	{
		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestGlbPersonEmailProcessesChangesForNewRecords()
		{
			GlbPersonEmailProcessesChangesForNewRecordsImplementation(
				"INSERT [dbo].[GlbPerson] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], PER_PK, PER_FullName, PER_EmailAddress) VALUES (0x10, 0x01, 2, 0x0, 1803, 1, '{0}', '{1}', '{2}')",
				"INSERT [biadmin].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x10, '2018-06-20'); INSERT INTO [biadmin].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES (0x10, 'dbo', 'GlbPerson', 1, 1803);",
				"Queued Deduplication in GlbPersonEmailSubscriber for {0}. Row: Added - {1} - {2} - ");
			GlbPersonEmailProcessesChangesForNewRecordsImplementation(
				"INSERT [dbo].[GlbPerson] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], PER_PK, PER_FullName, PER_EmailAddress2) VALUES (0x10, 0x01, 2, 0x0, 1803, 1, '{0}', '{1}', '{2}')",
				"INSERT [biadmin].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x10, '2018-06-20'); INSERT INTO [biadmin].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES (0x10, 'dbo', 'GlbPerson', 1, 1803);",
				"Queued Deduplication in GlbPersonEmailSubscriber for {0}. Row: Added - {1} -  - {2}");
		}

		void GlbPersonEmailProcessesChangesForNewRecordsImplementation(string insertIntoCDCQuery, string insertLsnMappingQuery, string expectedLog)
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

				var patternMatchingEmail = factory.Load<PatternMatchingEmail>(new ZQuery(PatternMatchingEmailSchema.PME_ParentId, glbPerson.PK)).ToList();
				AssertEquals(0, patternMatchingEmail.Count);

				var email = "Davos.Seaworth@wisetechglobal.com";
				var insertGlbPersonCDC = string.Format(CultureInfo.InvariantCulture, insertIntoCDCQuery, glbPerson.PK, glbPerson.PER_FullName, email);
				auditConnection.ExecuteNonQuery(insertGlbPersonCDC);
				auditConnection.ExecuteNonQuery(insertLsnMappingQuery);

				AuditTestHelper.RunNotificationCycleAndAssert(
					auditConnection,
					testSubscribers,
					"0x10",
					"2018-06-20",
					expectedLog: new BetterLogForTest(LogType.Information, string.Format(CultureInfo.InvariantCulture, expectedLog, GetPatternMasters(glbPerson), glbPerson.PK, email))
				);

				var emailToHash = TextStandardizerHelper.StandardizeEmail(email);
				patternMatchingEmail = factory.Load<PatternMatchingEmail>(new ZQuery(PatternMatchingEmailSchema.PME_ParentId, glbPerson.PK)).ToList();
				AssertEquals(1, patternMatchingEmail.Count);
				AssertPatternMatchingEmailResult(glbPerson, patternMatchingEmail.Single(), emailToHash);
			}
		}

		void AssertPatternMatchingEmailResult(GlbPerson glbPerson, PatternMatchingEmail patternMatchingEmail, string emailToHash)
		{
			AssertEquals(TextStandardizerHelper.ComputeStringHashFast(emailToHash), patternMatchingEmail.PME_HashedValue);
			AssertEquals(glbPerson.PK, patternMatchingEmail.PME_PER);
			AssertEquals(GlbPersonSchema.Constants.Prefix, patternMatchingEmail.PME_ParentTableCode);
			AssertEquals(glbPerson.PK, patternMatchingEmail.PME_ParentId);
			AssertEquals(glbPerson.PER_RN_NKCountry, patternMatchingEmail.PME_RN_NKCountryCode);
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestGlbPersonEmailNoPatternMatchingForPlaceholderRecords()
		{
			GlbPersonEmailNoPatternMatchingForPlaceholderRecordsImplementation(@"
				INSERT [dbo].[GlbPerson] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], PER_PK, PER_FullName, PER_EmailAddress) VALUES (0x10, 0x01, 2, 0x0, 1803, 1, '{0}', '{1}', '{2}')
				INSERT [biadmin].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x10, '2018-06-08');
				INSERT INTO [biadmin].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES (0x10, 'dbo', 'GlbPerson', 1, 1803);",
				"Queued Deduplication in GlbPersonEmailSubscriber for {0}. Row: Added - {1} - {2} - ");

			GlbPersonEmailNoPatternMatchingForPlaceholderRecordsImplementation(@"
				INSERT [dbo].[GlbPerson] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], PER_PK, PER_FullName, PER_EmailAddress2) VALUES (0x10, 0x01, 2, 0x0, 1803, 1, '{0}', '{1}', '{2}')
				INSERT [biadmin].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x10, '2018-06-08');
				INSERT INTO [biadmin].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES (0x10, 'dbo', 'GlbPerson', 1, 1803);",
				"Queued Deduplication in GlbPersonEmailSubscriber for {0}. Row: Added - {1} -  - {2}");
		}

		void GlbPersonEmailNoPatternMatchingForPlaceholderRecordsImplementation(string insertIntoCDCQuery, string expectedLog)
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

				var patternMatchingEmail = factory.Load<PatternMatchingEmail>(new ZQuery(PatternMatchingEmailSchema.PME_ParentId, glbPerson.PK)).ToList();
				AssertEquals(0, patternMatchingEmail.Count);

				var email = "test@test.com";
				var insertGlbPersonCDC = string.Format(CultureInfo.InvariantCulture, insertIntoCDCQuery, glbPerson.PK, glbPerson.PK, email);
				auditConnection.ExecuteNonQuery(insertGlbPersonCDC);

				AuditTestHelper.RunNotificationCycleAndAssert(
					auditConnection,
					testSubscribers,
					"0x10",
					"2018-06-08",
					expectedLog: new BetterLogForTest(LogType.Information, string.Format(CultureInfo.InvariantCulture, expectedLog, GetPatternMasters(glbPerson), glbPerson.PK, email))
				);

				patternMatchingEmail = factory.Load<PatternMatchingEmail>(new ZQuery(PatternMatchingEmailSchema.PME_ParentId, glbPerson.PK)).ToList();
				AssertEquals(0, patternMatchingEmail.Count);
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestGlbPersonEmailProcessesChangesForExistingRecords()
		{
			GlbPersonEmailProcessesChangesForExistingRecords(
				"INSERT [dbo].[GlbPerson] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], PER_PK, PER_FullName, PER_EmailAddress) VALUES (0x10, 0x01, 3, 0x0, 1803, 1, '{0}', '{1}', '{2}')",
				"INSERT [dbo].[GlbPerson] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], PER_PK, PER_FullName, PER_EmailAddress) VALUES (0x10, 0x01, 4, 0x0, 1803, 1, '{0}', '{1}', '{2}')",
				"INSERT [biadmin].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x10, '2018-03-15'); INSERT INTO [biadmin].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES (0x10, 'dbo', 'GlbPerson', 2, 1803);",
				"Queued Deduplication in GlbPersonEmailSubscriber for {0}. Row: Modified - {1} - {2} - ");
			GlbPersonEmailProcessesChangesForExistingRecords(
				"INSERT [dbo].[GlbPerson] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], PER_PK, PER_FullName, PER_EmailAddress2) VALUES (0x10, 0x01, 3, 0x0, 1803, 1, '{0}', '{1}', '{2}')",
				"INSERT [dbo].[GlbPerson] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], PER_PK, PER_FullName, PER_EmailAddress2) VALUES (0x10, 0x01, 4, 0x0, 1803, 1, '{0}', '{1}', '{2}')",
				"INSERT [biadmin].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x10, '2018-03-15'); INSERT INTO [biadmin].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES (0x10, 'dbo', 'GlbPerson', 2, 1803);",
				"Queued Deduplication in GlbPersonEmailSubscriber for {0}. Row: Modified - {1} -  - {2}");
		}

		void GlbPersonEmailProcessesChangesForExistingRecords(string insertIntoCDCQuery, string insertIntoCDCUpdateQuery, string insertLsnMappingQuery, string expectedLog)
		{
			GlbPersonEmailProcessesChangesForExistingRecords(insertIntoCDCQuery, insertIntoCDCUpdateQuery, insertLsnMappingQuery, expectedLog, true);
			GlbPersonEmailProcessesChangesForExistingRecords(insertIntoCDCQuery, insertIntoCDCUpdateQuery, insertLsnMappingQuery, expectedLog, false);
		}

		void GlbPersonEmailProcessesChangesForExistingRecords(string insertIntoCDCQuery, string insertIntoCDCUpdateQuery, string insertLsnMappingQuery, string expectedLog, bool createOriginalPatternData)
		{
			var factory = new BusinessObjectFactory();
			var glbPerson = SubscriberTestUtilities.CreateTestGlbPerson(factory);
			var originalEmailHashedValue = TextStandardizerHelper.ComputeStringHashFast("ALBERT@AABBCC.COM");

			if (createOriginalPatternData)
			{
				SubscriberTestUtilities.CreatePatternMatchingBusinessObjectForPerson<PatternMatchingEmail>(factory, glbPerson.PK, originalEmailHashedValue, GlbPersonSchema.Constants.Prefix, glbPerson.PK, glbPerson.PER_RN_NKCountry);
			}

			factory.Save();

			using (var auditConnection = AuditTestHelper.GetAuditConnection())
			{
				var testSubscribers = new ActualDataChangesAuditSubscriber[] {
					NewDataChangeSubscriber()
				};

				AuditTestHelper.CleanupAuditTestData(auditConnection, testSubscribers);
				AuditTestHelper.ResetSubscriberControlWaterMark(auditConnection, testSubscribers);

				var insertGlbPersonOriginalCDC = string.Format(CultureInfo.InvariantCulture, insertIntoCDCQuery, glbPerson.PK, glbPerson.PER_FullName, "ALBERT@AABBCC.COM");
				auditConnection.ExecuteNonQuery(insertGlbPersonOriginalCDC);

				var email = "Peter@CCAABB.COM";
				var insertGlbPersonCurrentCDC = string.Format(CultureInfo.InvariantCulture, insertIntoCDCUpdateQuery, glbPerson.PK, glbPerson.PER_FullName, email);
				auditConnection.ExecuteNonQuery(insertGlbPersonCurrentCDC);
				auditConnection.ExecuteNonQuery(insertLsnMappingQuery);

				AuditTestHelper.RunNotificationCycleAndAssert(
					auditConnection,
					testSubscribers,
					"0x10",
					"2018-03-15",
					expectedLog: new BetterLogForTest(LogType.Information, string.Format(CultureInfo.InvariantCulture, expectedLog, GetPatternMasters(glbPerson), glbPerson.PK, email))
				);

				var emailToHash = TextStandardizerHelper.StandardizeEmail(email);
				var patternMatchingEmail = factory.Load<PatternMatchingEmail>(new ZQuery(PatternMatchingEmailSchema.PME_ParentId, glbPerson.PK)).ToList();
				AssertEquals(1, patternMatchingEmail.Count);
				AssertPatternMatchingEmailResult(glbPerson, patternMatchingEmail.Single(), emailToHash);
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestGlbPersonEmailDoesNotProcessChangesForNonSubscribedColumns()
		{
			var factory = new BusinessObjectFactory();
			var glbPerson = SubscriberTestUtilities.CreateTestGlbPerson(factory);

			SubscriberTestUtilities.CreatePatternMatchingBusinessObjectForPerson<PatternMatchingEmail>(factory, glbPerson.PK, 0, GlbPersonSchema.Constants.Prefix, glbPerson.PK, glbPerson.PER_RN_NKCountry);

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
					"0x10",
					"2018-06-03",
					expectedLog: null
				);

				var patternMatchingEmail = factory.Load<PatternMatchingEmail>(new ZQuery(PatternMatchingEmailSchema.PME_ParentId, glbPerson.PK)).ToList();
				AssertEquals(1, patternMatchingEmail.Count);

				var firstPatternMatchingEmail = patternMatchingEmail.Single();
				AssertEquals(0, firstPatternMatchingEmail.PME_HashedValue);
			}
		}

		string GetPatternMasters(GlbPerson bizO)
		{
			return string.Format(CultureInfo.InvariantCulture, "[PER_PK:{0}]", bizO.PK);
		}

		public override void TestCustomFilter()
		{
			var subscriber = new GlbPersonEmailSubscriber();

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
				row[GlbPersonSchema.PER_EmailAddress.Name] = DBNull.Value;
				row[GlbPersonSchema.PER_EmailAddress2.Name] = DBNull.Value;
			}
			else
			{
				row[GlbPersonSchema.PER_EmailAddress.Name] = "Address";
				row[GlbPersonSchema.PER_EmailAddress2.Name] = "Address";
			}
			table.Rows.Add(row);
			return row;
		}

		protected override DataTable GetTestDataTable()
		{
			var changeTable = new DataTable();
			var tableColumns = new SchemaColumn[] { GlbPersonSchema.PER_EmailAddress, GlbPersonSchema.PER_EmailAddress2 };
			foreach (var column in tableColumns)
			{
				changeTable.Columns.Add(column.Name, column.DotNetType);
			}
			return changeTable;
		}
	}
}
