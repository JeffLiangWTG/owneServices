using System;
using System.Collections.Generic;
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
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.AuditDataServices.MDM.Testing
{
	[TestedType(typeof(GlbStaffEmailSubscriber))]
	class GlbStaffEmailSubscriberTest : ActualDataChangesAuditSubscriberTest
	{
		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestGlbStaffEmailProcessesChangesForNewRecords()
		{
			GlbStaffEmailProcessesChangesForNewRecordsImplementation(
				"INSERT [dbo].[GlbStaff] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], GS_PK, GS_Code, GS_PER, GS_EmailAddress) VALUES (0x10, 0x01, 2, 0x0, 1803, 1, '{0}', '{1}', '{2}', '{3}')",
				"INSERT [biadmin].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x10, '2018-06-08'); INSERT INTO [biadmin].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES (0x10, 'dbo', 'GlbStaff', 1, 1803);",
				"Queued Deduplication in GlbStaffEmailSubscriber for [PER_PK:{0}]. Row: Added - {1} - {2}");
		}

		void GlbStaffEmailProcessesChangesForNewRecordsImplementation(string insertIntoCDCQuery, string insertLsnMappingQuery, string expectedLog)
		{
			var factory = new BusinessObjectFactory();
			var person = SubscriberTestUtilities.CreateTestGlbPerson(factory);
			var staff = factory.NewWithValidTestData<GlbStaff>();

			staff.GS_Code = "XR";
			staff.GS_PER = person.PK;
			staff.GS_LoginName = "eddie";
			staff.GS_EmailAddress = "test@test.com";

			factory.Save();

			using (var auditConnection = AuditTestHelper.GetAuditConnection())
			{
				var testSubscribers = new ActualDataChangesAuditSubscriber[] {
					NewDataChangeSubscriber()
				};

				AuditTestHelper.CleanupAuditTestData(auditConnection, testSubscribers);
				AuditTestHelper.ResetSubscriberControlWaterMark(auditConnection, testSubscribers);

				var patternMatchingEmails = factory.Load<PatternMatchingEmail>(new ZQuery(PatternMatchingEmailSchema.PME_ParentId, staff.PK)).ToList();
				AssertEquals(0, patternMatchingEmails.Count);

				var valueToHash = "edward@cargowise.com";
				var insertGlbStaffCDC = string.Format(CultureInfo.InvariantCulture, insertIntoCDCQuery, staff.PK, staff.GS_Code, person.PK, valueToHash);

				auditConnection.ExecuteNonQuery(insertGlbStaffCDC);
				auditConnection.ExecuteNonQuery(insertLsnMappingQuery);

				AuditTestHelper.RunNotificationCycleAndAssert(
					auditConnection,
					testSubscribers,
					"0x10", "2018-06-08 00:00:00.000",
					expectedLog: new BetterLogForTest(LogType.Information, string.Format(CultureInfo.InvariantCulture, expectedLog, staff.Person.PK, staff.PK, valueToHash))
				);

				patternMatchingEmails = factory.Load<PatternMatchingEmail>(new ZQuery(PatternMatchingEmailSchema.PME_ParentId, staff.PK)).ToList();
				AssertEquals(1, patternMatchingEmails.Count);
				AssertPatternMatchingTableValues(patternMatchingEmails, "EDWARD@CARGOWISE.COM", staff, person);
			}
		}

		void AssertPatternMatchingTableValues<T>(List<T> patternTableValues, string valueToHash, GlbStaff staff, GlbPerson person)
		{
			IPatternMatchingBusinessObjects firstPatternValue = (IPatternMatchingBusinessObjects)patternTableValues.Single();

			AssertEquals(TextStandardizerHelper.ComputeStringHashFast(valueToHash), firstPatternValue.HashedValue);
			AssertEquals(staff.Person.PK, firstPatternValue.PersonPK);
			AssertEquals(GlbStaffSchema.Constants.Prefix, firstPatternValue.ParentTableCode);
			AssertEquals(staff.PK, firstPatternValue.ParentId);
			AssertEquals(person.PER_RN_NKCountry, firstPatternValue.PatternMatchingCountryCode);
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestGlbStaffEmailNoPatternMatchingForPlaceholderRecords()
		{
			var insertIntoCDCQuery = @"
				INSERT [dbo].[GlbStaff] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], GS_PK, GS_Code, GS_PER, GS_EmailAddress) VALUES 
					(0x10, 0x01, 2, 0x0, 1803, 1, '{0}', '{1}', '{2}', '{3}')
				INSERT [biadmin].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x10, '2018-06-08');
				INSERT INTO [biadmin].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES (0x10, 'dbo', 'GlbStaff', 1, 1803);";
			var expectedLog = "Queued Deduplication in GlbStaffEmailSubscriber for [PER_PK:{0}]. Row: Added - {1} - {2}";
			var factory = new BusinessObjectFactory();
			var person = SubscriberTestUtilities.CreateTestGlbPerson(factory);
			var staff = factory.New<GlbStaff>();

			staff.GS_Code = "BAE";
			staff.GS_PER = person.PK;
			staff.GS_EmailAddress = "testMe@test.com";

			factory.Save();

			using (var auditConnection = AuditTestHelper.GetAuditConnection())
			{
				var testSubscribers = new ActualDataChangesAuditSubscriber[] {
					NewDataChangeSubscriber()
				};

				AuditTestHelper.CleanupAuditTestData(auditConnection, testSubscribers);
				AuditTestHelper.ResetSubscriberControlWaterMark(auditConnection, testSubscribers);

				var emailAddress = "test@test.com";
				var insertGlbStaffCDC = string.Format(CultureInfo.InvariantCulture, insertIntoCDCQuery, staff.PK, staff.GS_Code, person.PK, emailAddress);
				auditConnection.ExecuteNonQuery(insertGlbStaffCDC);

				AuditTestHelper.RunNotificationCycleAndAssert(
					auditConnection,
					testSubscribers,
					"0x10", "2018-06-08 00:00:00.000",
					expectedLog: new BetterLogForTest(LogType.Information, string.Format(CultureInfo.InvariantCulture, expectedLog, staff.Person.PK, staff.PK, emailAddress))
				);
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestGlbStaffEmailProcessesChangesForExistingRecords()
		{
			GlbStaffEmailProcessesChangesForExistingRecords(
				"INSERT [dbo].[GlbStaff] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], GS_PK, GS_Code, GS_PER, GS_EmailAddress) VALUES (0x10, 0x01, 3, 0x0, 1803, 1, '{0}', '{1}', '{2}', '{3}')",
				"INSERT [dbo].[GlbStaff] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], GS_PK, GS_Code, GS_PER, GS_EmailAddress) VALUES (0x10, 0x01, 4, 0x0, 1803, 1, '{0}', '{1}', '{2}', '{3}')",
				"INSERT [biadmin].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x10, '2018-03-15'); INSERT INTO [biadmin].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES (0x10, 'dbo', 'GlbStaff', 2, 1803);",
				"Queued Deduplication in GlbStaffEmailSubscriber for [PER_PK:{0}]. Row: Modified - {1} - {2}");
		}

		void GlbStaffEmailProcessesChangesForExistingRecords(string insertIntoCDCQuery, string insertIntoCDCUpdateQuery, string insertLsnMappingQuery, string expectedLog)
		{
			var factory = new BusinessObjectFactory();
			var person = SubscriberTestUtilities.CreateTestGlbPerson(factory);
			var staff = factory.New<GlbStaff>();

			staff.GS_Code = "XX";
			staff.GS_PER = person.PK;
			staff.GS_LoginName = "eddie";
			staff.GS_EmailAddress = "test@test.com";
			staff.GS_RN_NKCountryCode = "AU";

			var originalEmailHashedValue = TextStandardizerHelper.ComputeStringHashFast("EDWARD@WISETECHGLOBAL.COM");
			SubscriberTestUtilities.CreatePatternMatchingBusinessObjectForPerson<PatternMatchingEmail>(factory, person.PK, originalEmailHashedValue, GlbStaffSchema.Constants.Prefix, staff.PK, person.PER_RN_NKCountry);

			factory.Save();

			using (var auditConnection = AuditTestHelper.GetAuditConnection())
			{
				var testSubscribers = new ActualDataChangesAuditSubscriber[] {
					NewDataChangeSubscriber()
				};

				AuditTestHelper.CleanupAuditTestData(auditConnection, testSubscribers);
				AuditTestHelper.ResetSubscriberControlWaterMark(auditConnection, testSubscribers);

				var insertGlbStaffOriginalCDC = string.Format(CultureInfo.InvariantCulture, insertIntoCDCQuery, staff.PK, staff.GS_Code, person.PK, "edward@wisetechglobal.com");
				auditConnection.ExecuteNonQuery(insertGlbStaffOriginalCDC);

				var valueToHash = "edward@cargowise.com";
				var insertGlbStaffCurrentCDC = string.Format(CultureInfo.InvariantCulture, insertIntoCDCUpdateQuery, staff.PK, staff.GS_Code, person.PK, valueToHash);
				auditConnection.ExecuteNonQuery(insertGlbStaffCurrentCDC);

				auditConnection.ExecuteNonQuery(insertLsnMappingQuery);

				AuditTestHelper.RunNotificationCycleAndAssert(
					auditConnection,
					testSubscribers,
					"0x10", "2018-03-15 00:00:00.000",
					expectedLog: new BetterLogForTest(LogType.Information, string.Format(CultureInfo.InvariantCulture, expectedLog, staff.Person.PK, staff.PK, valueToHash))
				);

				var patternMatchingEmails = factory.Load<PatternMatchingEmail>(new ZQuery(PatternMatchingEmailSchema.PME_ParentId, staff.PK)).ToList();
				AssertEquals(1, patternMatchingEmails.Count);
				AssertPatternMatchingTableValues(patternMatchingEmails, "EDWARD@CARGOWISE.COM", staff, person);
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestGlbStaffEmailDoesNotProcessChangesForNonSubscribedColumns()
		{
			var factory = new BusinessObjectFactory();
			var person = SubscriberTestUtilities.CreateTestGlbPerson(factory);
			var staff = factory.New<GlbStaff>();

			staff.GS_Code = "RRE";
			staff.GS_PER = person.PK;

			SubscriberTestUtilities.CreatePatternMatchingBusinessObjectForPerson<PatternMatchingEmail>(factory, person.PK, 0, GlbStaffSchema.Constants.Prefix, staff.PK, person.PER_RN_NKCountry);

			factory.Save();

			using (var auditConnection = AuditTestHelper.GetAuditConnection())
			{
				var testSubscribers = new ActualDataChangesAuditSubscriber[] {
					NewDataChangeSubscriber()
				};
				AuditTestHelper.ResetSubscriberControlWaterMark(auditConnection, testSubscribers);

				var insertGlbStaffOriginalCDC = string.Format(CultureInfo.InvariantCulture, @"INSERT [dbo].[GlbStaff] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], GS_PK, GS_Code, GS_PER) VALUES 
					(0x10, 0x01, 3, 0x0, 0, 1, '{0}', '{1}', '{2}');", staff.PK, staff.GS_Code, person.PK);
				auditConnection.ExecuteNonQuery(insertGlbStaffOriginalCDC);

				var insertGlbStaffCurrentCDC = string.Format(CultureInfo.InvariantCulture, @"INSERT [dbo].[GlbStaff] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], GS_PK, GS_Code, GS_PER) VALUES 
					(0x10, 0x01, 4, 0x0, 0, 1, '{0}', '{1}', '{2}');", staff.PK, staff.GS_Code, person.PK);
				auditConnection.ExecuteNonQuery(insertGlbStaffCurrentCDC);

				AuditTestHelper.RunNotificationCycleAndAssert(
					auditConnection,
					testSubscribers,
					"0x10", "2018-06-08 00:00:00.000",
					expectedLog: null
				);

				var patternMatchingEmail = factory.Load<PatternMatchingEmail>(new ZQuery(PatternMatchingEmailSchema.PME_ParentId, staff.PK)).ToList();
				AssertEquals(1, patternMatchingEmail.Count);

				var firstPatternMatchingEmail = patternMatchingEmail.Single();
				AssertEquals(0, firstPatternMatchingEmail.PME_HashedValue);
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestSubscriberDoesNotProcessWithNullMaster()
		{
			var factory = new BusinessObjectFactory();
			var bizO = factory.NewWithValidTestData<GlbStaff>();

			bizO.GS_Code = "QAA";
			bizO.GS_IsSystemAccount = true;
			bizO.GS_EmailAddress = "john@cargowise.com";

			factory.Save();

			Db.Connection.ExecuteNonQuery($"UPDATE dbo.GlbStaff SET GS_PER = NULL, GS_SystemLastEditUser = 'E', GS_SystemLastEditTimeUtc = GetDate() WHERE GS_PK = '{bizO.PK}'");

			using (var auditConnection = AuditTestHelper.GetAuditConnection())
			{
				var testSubscribers = new ActualDataChangesAuditSubscriber[] {
					NewDataChangeSubscriber()
				};

				AuditTestHelper.CleanupAuditTestData(auditConnection, testSubscribers);
				AuditTestHelper.ResetSubscriberControlWaterMark(auditConnection, testSubscribers);

				var patternMatching = factory.Load<PatternMatchingEmail>(new ZQuery(PatternMatchingEmailSchema.PME_ParentId, bizO.PK)).ToList();
				AssertEquals(0, patternMatching.Count);

				var query = @"INSERT [dbo].[GlbStaff] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], GS_PK, GS_Code, GS_EmailAddress, GS_ResourceType) VALUES (0x10, 0x01, {0}, 0x0, 1803, 1, '{1}', '{2}', '{3}', '{4}');";
				var insertIntoCDCText = string.Format(CultureInfo.InvariantCulture, query, "2", bizO.PK, bizO.GS_Code, bizO.GS_EmailAddress, bizO.GS_ResourceType);

				auditConnection.ExecuteNonQuery(insertIntoCDCText);

				var insertLsnMappingText = "INSERT [biadmin].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x10, '2018-06-08');";
				auditConnection.ExecuteNonQuery(insertLsnMappingText);

				var insertCDCHistoryText = "INSERT INTO[biadmin].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES(0x10, 'dbo', 'GlbStaff', 1, 1803);";
				auditConnection.ExecuteNonQuery(insertCDCHistoryText);

				AuditTestHelper.RunNotificationCycleAndAssert(
					auditConnection,
					testSubscribers,
					"0x10", "2018-06-08 00:00:00.000",
					expectedLog: new BetterLogForTest(LogType.Debug, string.Format(CultureInfo.InvariantCulture, "None Queuing Deduplication in GlbStaffEmailSubscriber. Row: Added - {0} - {1}", bizO.PK, "john@cargowise.com"))
				);

				patternMatching = factory.Load<PatternMatchingEmail>(new ZQuery(PatternMatchingEmailSchema.PME_ParentId, bizO.PK)).ToList();
				AssertEquals(0, patternMatching.Count);
			}
		}

		public override void TestCustomFilter()
		{
			var subscriber = new GlbStaffEmailSubscriber();

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
				row[GlbStaffSchema.GS_EmailAddress.Name] = DBNull.Value;
			}
			else
			{
				row[GlbStaffSchema.GS_EmailAddress.Name] = "Email";
			}
			table.Rows.Add(row);
			return row;
		}

		protected override DataTable GetTestDataTable()
		{
			var changeTable = new DataTable();
			var tableColumns = new SchemaColumn[] { GlbStaffSchema.GS_EmailAddress };
			foreach (var column in tableColumns)
			{
				changeTable.Columns.Add(column.Name, column.DotNetType);
			}
			return changeTable;
		}
	}
}
