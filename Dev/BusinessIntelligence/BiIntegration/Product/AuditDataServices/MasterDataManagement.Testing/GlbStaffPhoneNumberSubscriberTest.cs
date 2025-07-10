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
	[TestedType(typeof(GlbStaffPhoneNumberSubscriber))]
	class GlbStaffPhoneNumberSubscriberTest : ActualDataChangesAuditSubscriberTest
	{
		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestGlbStaffPhoneNumberProcessesChangesForNewRecords()
		{
			GlbStaffPhoneNumberProcessesChangesForNewRecordsImplementation(
				"INSERT [dbo].[GlbStaff] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], GS_PK, GS_Code, GS_PER, GS_FaxNum) VALUES (0x10, 0x01, 2, 0x0, 1803, 1, '{0}', '{1}', '{2}', '{3}')",
				"INSERT [biadmin].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x10, '2018-06-08'); INSERT INTO [biadmin].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES (0x10, 'dbo', 'GlbStaff', 1, 1803);",
				"Queued Deduplication in GlbStaffPhoneNumberSubscriber for [PER_PK:{0}]. Row: Added - {1} - {2} -  -  - ", 1);
			GlbStaffPhoneNumberProcessesChangesForNewRecordsImplementation(
				"INSERT [dbo].[GlbStaff] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], GS_PK, GS_Code, GS_PER, GS_HomePhone) VALUES (0x10, 0x01, 2, 0x0, 1803, 1, '{0}', '{1}', '{2}', '{3}')",
				"INSERT [biadmin].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x10, '2018-06-08'); INSERT INTO [biadmin].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES (0x10, 'dbo', 'GlbStaff', 1, 1803);",
				"Queued Deduplication in GlbStaffPhoneNumberSubscriber for [PER_PK:{0}]. Row: Added - {1} -  - {2} -  - ", 2);
			GlbStaffPhoneNumberProcessesChangesForNewRecordsImplementation(
				"INSERT [dbo].[GlbStaff] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], GS_PK, GS_Code, GS_PER, GS_MobilePhone) VALUES (0x10, 0x01, 2, 0x0, 1803, 1, '{0}', '{1}', '{2}', '{3}')",
				"INSERT [biadmin].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x10, '2018-06-08'); INSERT INTO [biadmin].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES (0x10, 'dbo', 'GlbStaff', 1, 1803);",
				"Queued Deduplication in GlbStaffPhoneNumberSubscriber for [PER_PK:{0}]. Row: Added - {1} -  -  - {2} - ", 3);
			GlbStaffPhoneNumberProcessesChangesForNewRecordsImplementation(
				"INSERT [dbo].[GlbStaff] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], GS_PK, GS_Code, GS_PER, GS_WorkPhone) VALUES (0x10, 0x01, 2, 0x0, 1803, 1, '{0}', '{1}', '{2}', '{3}')",
				"INSERT [biadmin].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x10, '2018-06-08'); INSERT INTO [biadmin].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES (0x10, 'dbo', 'GlbStaff', 1, 1803);",
				"Queued Deduplication in GlbStaffPhoneNumberSubscriber for [PER_PK:{0}]. Row: Added - {1} -  -  -  - {2}", 4);
		}

		void GlbStaffPhoneNumberProcessesChangesForNewRecordsImplementation(string insertIntoCDCQuery, string insertLsnMappingQuery, string expectedLog, int batch)
		{
			var factory = new BusinessObjectFactory();
			var person = SubscriberTestUtilities.CreateTestGlbPerson(factory);
			var staff = factory.NewWithValidTestData<GlbStaff>();

			staff.GS_Code = "XR" + batch;
			staff.GS_PER = person.PK;
			staff.GS_LoginName = "eddie" + batch;

			factory.Save();

			using (var auditConnection = AuditTestHelper.GetAuditConnection())
			{
				var testSubscribers = new ActualDataChangesAuditSubscriber[] {
					NewDataChangeSubscriber()
				};

				AuditTestHelper.CleanupAuditTestData(auditConnection, testSubscribers);
				AuditTestHelper.ResetSubscriberControlWaterMark(auditConnection, testSubscribers);

				var patternMatchingPhone = factory.Load<PatternMatchingPhone>(new ZQuery(PatternMatchingPhoneSchema.PMP_ParentId, staff.PK)).ToList();
				AssertEquals(0, patternMatchingPhone.Count);

				var phoneNumber = "92041334";
				var insertGlbStaffCDC = string.Format(CultureInfo.InvariantCulture, insertIntoCDCQuery, staff.PK, staff.GS_Code, person.PK, phoneNumber);
				auditConnection.ExecuteNonQuery(insertGlbStaffCDC);

				auditConnection.ExecuteNonQuery(insertLsnMappingQuery);

				AuditTestHelper.RunNotificationCycleAndAssert(
					auditConnection,
					testSubscribers,
					"0x10", "2018-06-08 00:00:00.000",
					expectedLog: new BetterLogForTest(LogType.Information, string.Format(CultureInfo.InvariantCulture, expectedLog, staff.Person.PK, staff.PK, phoneNumber))
				);

				patternMatchingPhone = factory.Load<PatternMatchingPhone>(new ZQuery(PatternMatchingPhoneSchema.PMP_ParentId, staff.PK)).ToList();
				AssertEquals(1, patternMatchingPhone.Count);

				var firstPatternMatchingPhone = patternMatchingPhone.Single();

				AssertEquals(TextStandardizerHelper.ComputeStringHashFast(phoneNumber), firstPatternMatchingPhone.PMP_HashedValue);
				AssertEquals(staff.Person.PK, firstPatternMatchingPhone.PMP_PER);
				AssertEquals(GlbStaffSchema.Constants.Prefix, firstPatternMatchingPhone.PMP_ParentTableCode);
				AssertEquals(staff.PK, firstPatternMatchingPhone.PMP_ParentId);
				AssertEquals(person.PER_RN_NKCountry, firstPatternMatchingPhone.PMP_RN_NKCountryCode);
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestGlbStaffPhoneNumberNoPatternMatchingForPlaceholderRecords()
		{
			var insertIntoCDCQuery = @"
				INSERT [dbo].[GlbStaff] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], GS_PK, GS_Code, GS_PER, GS_FaxNum) VALUES 
					(0x10, 0x01, 2, 0x0, 1803, 1, '{0}', '{1}', '{2}', '{3}')
				INSERT [biadmin].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x10, '2018-06-08');
				INSERT INTO [biadmin].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES (0x10, 'dbo', 'GlbStaff', 1, 1803);";
			var expectedLog = "Queued Deduplication in GlbStaffPhoneNumberSubscriber for [PER_PK:{0}]. Row: Added - {1} - {2} -  -  - ";
			var factory = new BusinessObjectFactory();
			var person = SubscriberTestUtilities.CreateTestGlbPerson(factory);
			var staff = factory.New<GlbStaff>();

			staff.GS_Code = "BAE";
			staff.GS_PER = person.PK;

			factory.Save();

			using (var auditConnection = AuditTestHelper.GetAuditConnection())
			{
				var testSubscribers = new ActualDataChangesAuditSubscriber[] {
					NewDataChangeSubscriber()
				};

				AuditTestHelper.CleanupAuditTestData(auditConnection, testSubscribers);
				AuditTestHelper.ResetSubscriberControlWaterMark(auditConnection, testSubscribers);

				var patternMatchingPhone = factory.Load<PatternMatchingPhone>(new ZQuery(PatternMatchingPhoneSchema.PMP_ParentId, staff.PK)).ToList();
				AssertEquals(0, patternMatchingPhone.Count);

				var phoneNumber = "0000";
				var insertGlbStaffCDC = string.Format(CultureInfo.InvariantCulture, insertIntoCDCQuery, staff.PK, staff.GS_Code, person.PK, phoneNumber);
				auditConnection.ExecuteNonQuery(insertGlbStaffCDC);

				AuditTestHelper.RunNotificationCycleAndAssert(
					auditConnection,
					testSubscribers,
					"0x10", "2018-06-08 00:00:00.000",
					expectedLog: new BetterLogForTest(LogType.Information, string.Format(CultureInfo.InvariantCulture, expectedLog, staff.Person.PK, staff.PK, phoneNumber))
				);

				patternMatchingPhone = factory.Load<PatternMatchingPhone>(new ZQuery(PatternMatchingPhoneSchema.PMP_ParentId, staff.PK)).ToList();
				AssertEquals(0, patternMatchingPhone.Count);
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestGlbStaffPhoneNumberProcessesChangesForExistingRecords()
		{
			GlbStaffPhoneNumberProcessesChangesForExistingRecords(
				"INSERT [dbo].[GlbStaff] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], GS_PK, GS_Code, GS_PER, GS_FaxNum) VALUES (0x10, 0x01, 3, 0x0, 1803, 1, '{0}', '{1}', '{2}', '{3}')",
				"INSERT [dbo].[GlbStaff] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], GS_PK, GS_Code, GS_PER, GS_FaxNum) VALUES (0x10, 0x01, 4, 0x0, 1803, 1, '{0}', '{1}', '{2}', '{3}')",
				"INSERT [biadmin].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x10, '2018-03-15'); INSERT INTO [biadmin].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES (0x10, 'dbo', 'GlbStaff', 2, 1803);",
				"Queued Deduplication in GlbStaffPhoneNumberSubscriber for [PER_PK:{0}]. Row: Modified - {1} - {2} -  -  - ", 1);
			GlbStaffPhoneNumberProcessesChangesForExistingRecords(
				"INSERT [dbo].[GlbStaff] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], GS_PK, GS_Code, GS_PER, GS_HomePhone) VALUES (0x10, 0x01, 3, 0x0, 1803, 1, '{0}', '{1}', '{2}', '{3}')",
				"INSERT [dbo].[GlbStaff] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], GS_PK, GS_Code, GS_PER, GS_HomePhone) VALUES (0x10, 0x01, 4, 0x0, 1803, 1, '{0}', '{1}', '{2}', '{3}')",
				"INSERT [biadmin].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x10, '2018-03-15'); INSERT INTO [biadmin].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES (0x10, 'dbo', 'GlbStaff', 2, 1803);",
				"Queued Deduplication in GlbStaffPhoneNumberSubscriber for [PER_PK:{0}]. Row: Modified - {1} -  - {2} -  - ", 2);
			GlbStaffPhoneNumberProcessesChangesForExistingRecords(
				"INSERT [dbo].[GlbStaff] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], GS_PK, GS_Code, GS_PER, GS_MobilePhone) VALUES (0x10, 0x01, 3, 0x0, 1803, 1, '{0}', '{1}', '{2}', '{3}')",
				"INSERT [dbo].[GlbStaff] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], GS_PK, GS_Code, GS_PER, GS_MobilePhone) VALUES (0x10, 0x01, 4, 0x0, 1803, 1, '{0}', '{1}', '{2}', '{3}')",
				"INSERT [biadmin].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x10, '2018-03-15'); INSERT INTO [biadmin].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES (0x10, 'dbo', 'GlbStaff', 2, 1803);",
				"Queued Deduplication in GlbStaffPhoneNumberSubscriber for [PER_PK:{0}]. Row: Modified - {1} -  -  - {2} - ", 3);
			GlbStaffPhoneNumberProcessesChangesForExistingRecords(
				"INSERT [dbo].[GlbStaff] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], GS_PK, GS_Code, GS_PER, GS_WorkPhone) VALUES (0x10, 0x01, 3, 0x0, 1803, 1, '{0}', '{1}', '{2}', '{3}')",
				"INSERT [dbo].[GlbStaff] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], GS_PK, GS_Code, GS_PER, GS_WorkPhone) VALUES (0x10, 0x01, 4, 0x0, 1803, 1, '{0}', '{1}', '{2}', '{3}')",
				"INSERT [biadmin].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x10, '2018-03-15'); INSERT INTO [biadmin].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES (0x10, 'dbo', 'GlbStaff', 2, 1803);",
				"Queued Deduplication in GlbStaffPhoneNumberSubscriber for [PER_PK:{0}]. Row: Modified - {1} -  -  -  - {2}", 4);
		}

		void GlbStaffPhoneNumberProcessesChangesForExistingRecords(string insertIntoCDCQuery, string insertIntoCDCUpdateQuery, string insertLsnMappingQuery, string expectedLog, int batch)
		{
			var factory = new BusinessObjectFactory();
			var person = SubscriberTestUtilities.CreateTestGlbPerson(factory);
			var staff = factory.New<GlbStaff>();

			staff.GS_Code = "XX" + batch;
			staff.GS_PER = person.PK;
			staff.GS_LoginName = "eddie" + batch;
			staff.GS_RN_NKCountryCode = "AU";

			var originalHashedValue = TextStandardizerHelper.ComputeStringHashFast("32541356");

			SubscriberTestUtilities.CreatePatternMatchingBusinessObjectForPerson<PatternMatchingPhone>(factory, person.PK, originalHashedValue, GlbStaffSchema.Constants.Prefix, staff.PK, person.PER_RN_NKCountry);

			factory.Save();

			using (var auditConnection = AuditTestHelper.GetAuditConnection())
			{
				var testSubscribers = new ActualDataChangesAuditSubscriber[] {
					NewDataChangeSubscriber()
				};

				AuditTestHelper.CleanupAuditTestData(auditConnection, testSubscribers);
				AuditTestHelper.ResetSubscriberControlWaterMark(auditConnection, testSubscribers);

				var insertGlbStaffOriginalCDC = string.Format(CultureInfo.InvariantCulture, insertIntoCDCQuery, staff.PK, staff.GS_Code, person.PK, "32541356");
				auditConnection.ExecuteNonQuery(insertGlbStaffOriginalCDC);

				var phoneNumber = "87654321";
				var insertGlbStaffCurrentCDC = string.Format(CultureInfo.InvariantCulture, insertIntoCDCUpdateQuery, staff.PK, staff.GS_Code, person.PK, phoneNumber);
				auditConnection.ExecuteNonQuery(insertGlbStaffCurrentCDC);

				auditConnection.ExecuteNonQuery(insertLsnMappingQuery);

				AuditTestHelper.RunNotificationCycleAndAssert(
					auditConnection,
					testSubscribers,
					"0x10", "2018-03-15 00:00:00.000",
					expectedLog: new BetterLogForTest(LogType.Information, string.Format(CultureInfo.InvariantCulture, expectedLog, staff.Person.PK, staff.PK, phoneNumber))
				);

				var hashedValue = TextStandardizerHelper.ComputeStringHashFast(phoneNumber);

				var patternMatchingPhone = factory.Load<PatternMatchingPhone>(new ZQuery(PatternMatchingPhoneSchema.PMP_ParentId, staff.PK)).ToList();
				AssertEquals(1, patternMatchingPhone.Count);

				var firstPatternMatchingPhone = patternMatchingPhone.Single();
				AssertEquals(hashedValue, firstPatternMatchingPhone.PMP_HashedValue);
				AssertEquals(person.PK, firstPatternMatchingPhone.PMP_PER);
				AssertEquals(GlbStaffSchema.Constants.Prefix, firstPatternMatchingPhone.PMP_ParentTableCode);
				AssertEquals(staff.PK, firstPatternMatchingPhone.PMP_ParentId);
				AssertEquals(person.PER_RN_NKCountry, firstPatternMatchingPhone.PMP_RN_NKCountryCode);
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestGlbStaffPhoneNumberDoesNotProcessChangesForNonSubscribedColumns()
		{
			var factory = new BusinessObjectFactory();
			var person = SubscriberTestUtilities.CreateTestGlbPerson(factory);
			var staff = factory.New<GlbStaff>();

			staff.GS_Code = "RRE";
			staff.GS_PER = person.PK;

			SubscriberTestUtilities.CreatePatternMatchingBusinessObjectForPerson<PatternMatchingPhone>(factory, person.PK, 0, GlbStaffSchema.Constants.Prefix, staff.PK, person.PER_RN_NKCountry);

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

				var patternMatchingPhone = factory.Load<PatternMatchingPhone>(new ZQuery(PatternMatchingPhoneSchema.PMP_ParentId, staff.PK)).ToList();
				AssertEquals(1, patternMatchingPhone.Count);

				var firstPatternMatchingPhone = patternMatchingPhone.Single();
				AssertEquals(0, firstPatternMatchingPhone.PMP_HashedValue);
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestSubscriberDoesNotProcessWithNullMaster()
		{
			var factory = new BusinessObjectFactory();
			var bizO = factory.NewWithValidTestData<GlbStaff>();

			bizO.GS_Code = "QAA";
			bizO.GS_IsSystemAccount = true;

			factory.Save();

			Db.Connection.ExecuteNonQuery($"UPDATE dbo.GlbStaff SET GS_PER = NULL, GS_SystemLastEditUser = 'E', GS_SystemLastEditTimeUtc = GetDate() WHERE GS_PK = '{bizO.PK}'");

			using (var auditConnection = AuditTestHelper.GetAuditConnection())
			{
				var testSubscribers = new ActualDataChangesAuditSubscriber[] {
					NewDataChangeSubscriber()
				};
				AuditTestHelper.ResetSubscriberControlWaterMark(auditConnection, testSubscribers);

				var patternMatching = factory.Load<PatternMatchingPhone>(new ZQuery(PatternMatchingPhoneSchema.PMP_ParentId, bizO.PK)).ToList();
				AssertEquals(0, patternMatching.Count);

				var query = @"INSERT [dbo].[GlbStaff] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], GS_PK, GS_Code, GS_UserAddress1, GS_ResourceType) VALUES (0x10, 0x01, {0}, 0x0, 1803, 1, '{1}', '{2}', '{3}', '{4}');";
				var insertIntoCDCText = string.Format(CultureInfo.InvariantCulture, query, "2", bizO.PK, bizO.GS_Code, bizO.GS_UserAddress1, bizO.GS_ResourceType);

				auditConnection.ExecuteNonQuery(insertIntoCDCText);

				var insertLsnMappingText = "INSERT [biadmin].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x10, '2018-06-08');";
				auditConnection.ExecuteNonQuery(insertLsnMappingText);

				AuditTestHelper.RunNotificationCycleAndAssert(
					auditConnection,
					testSubscribers,
					"0x10", "2018-06-08 00:00:00.000",
					expectedLog: null
				);

				patternMatching = factory.Load<PatternMatchingPhone>(new ZQuery(PatternMatchingPhoneSchema.PMP_ParentId, bizO.PK)).ToList();
				AssertEquals(0, patternMatching.Count);
			}
		}

		public override void TestCustomFilter()
		{
			var subscriber = new GlbStaffPhoneNumberSubscriber();

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
				row[GlbStaffSchema.GS_FaxNum.Name] = DBNull.Value;
				row[GlbStaffSchema.GS_HomePhone.Name] = DBNull.Value;
				row[GlbStaffSchema.GS_MobilePhone.Name] = DBNull.Value;
				row[GlbStaffSchema.GS_WorkPhone.Name] = DBNull.Value;
			}
			else
			{
				row[GlbStaffSchema.GS_FaxNum.Name] = "Phone";
				row[GlbStaffSchema.GS_HomePhone.Name] = "Phone";
				row[GlbStaffSchema.GS_MobilePhone.Name] = "Phone";
				row[GlbStaffSchema.GS_WorkPhone.Name] = "Phone";
			}
			table.Rows.Add(row);
			return row;
		}

		protected override DataTable GetTestDataTable()
		{
			var changeTable = new DataTable();
			var tableColumns = new SchemaColumn[] { GlbStaffSchema.GS_FaxNum, GlbStaffSchema.GS_HomePhone, GlbStaffSchema.GS_MobilePhone, GlbStaffSchema.GS_WorkPhone };
			foreach (var column in tableColumns)
			{
				changeTable.Columns.Add(column.Name, column.DotNetType);
			}
			return changeTable;
		}
	}
}
