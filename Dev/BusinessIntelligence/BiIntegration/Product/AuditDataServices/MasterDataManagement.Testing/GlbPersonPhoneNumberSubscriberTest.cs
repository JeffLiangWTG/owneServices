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
	[TestedType(typeof(GlbPersonPhoneNumberSubscriber))]
	class GlbPersonPhoneNumberSubscriberTest : ActualDataChangesAuditSubscriberTest
	{
		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestGlbPersonPhoneNumberProcessesChangesForNewRecords()
		{
			GlbPersonPhoneNumberProcessesChangesForNewRecordsImplementation(
				"INSERT [dbo].[GlbPerson] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], PER_PK, PER_FullName, PER_FaxNumber) VALUES (0x10, 0x01, 2, 0x0, 1803, 1, '{0}', '{1}', '{2}')",
				"INSERT [biadmin].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x10, '2018-06-08'); INSERT INTO [biadmin].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES (0x10, 'dbo', 'GlbPerson', 1, 1803);",
				"Queued Deduplication in GlbPersonPhoneNumberSubscriber for [PER_PK:{0}]. Row: Added - {0} - {1} -  -  - ");
			GlbPersonPhoneNumberProcessesChangesForNewRecordsImplementation(
				"INSERT [dbo].[GlbPerson] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], PER_PK, PER_FullName, PER_HomePhone) VALUES (0x10, 0x01, 2, 0x0, 1803, 1, '{0}', '{1}', '{2}')",
				"INSERT [biadmin].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x10, '2018-06-08'); INSERT INTO [biadmin].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES (0x10, 'dbo', 'GlbPerson', 1, 1803);",
				"Queued Deduplication in GlbPersonPhoneNumberSubscriber for [PER_PK:{0}]. Row: Added - {0} -  - {1} -  - ");
			GlbPersonPhoneNumberProcessesChangesForNewRecordsImplementation(
				"INSERT [dbo].[GlbPerson] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], PER_PK, PER_FullName, PER_MobilePhone) VALUES (0x10, 0x01, 2, 0x0, 1803, 1, '{0}', '{1}', '{2}')",
				"INSERT [biadmin].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x10, '2018-06-08'); INSERT INTO [biadmin].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES (0x10, 'dbo', 'GlbPerson', 1, 1803);",
				"Queued Deduplication in GlbPersonPhoneNumberSubscriber for [PER_PK:{0}]. Row: Added - {0} -  -  - {1} - ");
			GlbPersonPhoneNumberProcessesChangesForNewRecordsImplementation(
				"INSERT [dbo].[GlbPerson] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], PER_PK, PER_FullName, PER_MobilePhone2) VALUES (0x10, 0x01, 2, 0x0, 1803, 1, '{0}', '{1}', '{2}')",
				"INSERT [biadmin].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x10, '2018-06-08'); INSERT INTO [biadmin].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES (0x10, 'dbo', 'GlbPerson', 1, 1803);",
				"Queued Deduplication in GlbPersonPhoneNumberSubscriber for [PER_PK:{0}]. Row: Added - {0} -  -  -  - {1}");
		}

		void GlbPersonPhoneNumberProcessesChangesForNewRecordsImplementation(string insertIntoCDCQuery, string insertLsnMappingQuery, string expectedLog)
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

				var patternMatchingPhone = factory.Load<PatternMatchingPhone>(new ZQuery(PatternMatchingPhoneSchema.PMP_ParentId, glbPerson.PK)).ToList();
				AssertEquals(0, patternMatchingPhone.Count);

				var phoneNumber = "92041334";
				var insertGlbPersonCDC = string.Format(CultureInfo.InvariantCulture, insertIntoCDCQuery, glbPerson.PK, glbPerson.PER_FullName, phoneNumber);
				auditConnection.ExecuteNonQuery(insertGlbPersonCDC);

				auditConnection.ExecuteNonQuery(insertLsnMappingQuery);

				AuditTestHelper.RunNotificationCycleAndAssert(
					auditConnection,
					testSubscribers,
					"0x10", "2018-06-08 00:00:00.000",
					expectedLog: new BetterLogForTest(LogType.Information, string.Format(CultureInfo.InvariantCulture, expectedLog, glbPerson.PK, phoneNumber))
				);

				patternMatchingPhone = factory.Load<PatternMatchingPhone>(new ZQuery(PatternMatchingPhoneSchema.PMP_ParentId, glbPerson.PK)).ToList();
				AssertEquals(1, patternMatchingPhone.Count);

				var firstPatternMatchingPhone = patternMatchingPhone.Single();

				AssertEquals(TextStandardizerHelper.ComputeStringHashFast(phoneNumber), firstPatternMatchingPhone.PMP_HashedValue);
				AssertEquals(glbPerson.PK, firstPatternMatchingPhone.PMP_PER);
				AssertEquals(GlbPersonSchema.Constants.Prefix, firstPatternMatchingPhone.PMP_ParentTableCode);
				AssertEquals(glbPerson.PK, firstPatternMatchingPhone.PMP_ParentId);
				AssertEquals(glbPerson.PER_RN_NKCountry, firstPatternMatchingPhone.PMP_RN_NKCountryCode);
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestGlbPersonPhoneNumberNoPatternMatchingForPlaceholderRecords()
		{
			var insertIntoCDCQuery = @"
				INSERT [dbo].[GlbPerson] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], PER_PK, PER_FullName, PER_FaxNumber) VALUES 
					(0x10, 0x01, 2, 0x0, 1803, 1, '{0}', '{1}', '{2}')
				INSERT [biadmin].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x10, '2018-06-08');
				INSERT INTO [biadmin].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES
					(0x10, 'dbo', 'GlbPerson', 1, 1803);";

			var expectedLog = "Queued Deduplication in GlbPersonPhoneNumberSubscriber for [PER_PK:{0}]. Row: Added - {0} - {1} -  -  - ";

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

				var patternMatchingPhone = factory.Load<PatternMatchingPhone>(new ZQuery(PatternMatchingPhoneSchema.PMP_ParentId, glbPerson.PK)).ToList();
				AssertEquals(0, patternMatchingPhone.Count);

				var phoneNumber = "0000";
				var insertGlbPersonCDC = string.Format(CultureInfo.InvariantCulture, insertIntoCDCQuery, glbPerson.PK, glbPerson.PER_FullName, phoneNumber);
				auditConnection.ExecuteNonQuery(insertGlbPersonCDC);

				AuditTestHelper.RunNotificationCycleAndAssert(
					auditConnection,
					testSubscribers,
					"0x10", "2018-06-08 00:00:00.000",
					expectedLog: new BetterLogForTest(LogType.Information, string.Format(CultureInfo.InvariantCulture, expectedLog, glbPerson.PK, phoneNumber))
				);

				patternMatchingPhone = factory.Load<PatternMatchingPhone>(new ZQuery(PatternMatchingPhoneSchema.PMP_ParentId, glbPerson.PK)).ToList();
				AssertEquals(0, patternMatchingPhone.Count);
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestGlbPersonPhoneNumberProcessesChangesForExistingRecords()
		{
			GlbPersonPhoneNumberProcessesChangesForExistingRecords(
				"INSERT [dbo].[GlbPerson] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], PER_PK, PER_FullName, PER_FaxNumber) VALUES (0x10, 0x01, 3, 0x0, 1803, 1, '{0}', '{1}', '{2}')",
				"INSERT [dbo].[GlbPerson] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], PER_PK, PER_FullName, PER_FaxNumber) VALUES (0x10, 0x01, 4, 0x0, 1803, 1, '{0}', '{1}', '{2}')",
				"INSERT [biadmin].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x10, '2018-03-15'); INSERT INTO [biadmin].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES (0x10, 'dbo', 'GlbPerson', 2, 1803);",
				"Queued Deduplication in GlbPersonPhoneNumberSubscriber for [PER_PK:{0}]. Row: Modified - {0} - {1} -  -  - ");
			GlbPersonPhoneNumberProcessesChangesForExistingRecords(
				"INSERT [dbo].[GlbPerson] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], PER_PK, PER_FullName, PER_HomePhone) VALUES (0x10, 0x01, 3, 0x0, 1803, 1, '{0}', '{1}', '{2}')",
				"INSERT [dbo].[GlbPerson] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], PER_PK, PER_FullName, PER_HomePhone) VALUES (0x10, 0x01, 4, 0x0, 1803, 1, '{0}', '{1}', '{2}')",
				"INSERT [biadmin].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x10, '2018-03-15'); INSERT INTO [biadmin].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES (0x10, 'dbo', 'GlbPerson', 2, 1803);",
				"Queued Deduplication in GlbPersonPhoneNumberSubscriber for [PER_PK:{0}]. Row: Modified - {0} -  - {1} -  - ");
			GlbPersonPhoneNumberProcessesChangesForExistingRecords(
				"INSERT [dbo].[GlbPerson] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], PER_PK, PER_FullName, PER_MobilePhone) VALUES (0x10, 0x01, 3, 0x0, 1803, 1, '{0}', '{1}', '{2}')",
				"INSERT [dbo].[GlbPerson] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], PER_PK, PER_FullName, PER_MobilePhone) VALUES (0x10, 0x01, 4, 0x0, 1803, 1, '{0}', '{1}', '{2}')",
				"INSERT [biadmin].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x10, '2018-03-15'); INSERT INTO [biadmin].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES (0x10, 'dbo', 'GlbPerson', 2, 1803);",
				"Queued Deduplication in GlbPersonPhoneNumberSubscriber for [PER_PK:{0}]. Row: Modified - {0} -  -  - {1} - ");
			GlbPersonPhoneNumberProcessesChangesForExistingRecords(
				"INSERT [dbo].[GlbPerson] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], PER_PK, PER_FullName, PER_MobilePhone2) VALUES (0x10, 0x01, 3, 0x0, 1803, 1, '{0}', '{1}', '{2}')",
				"INSERT [dbo].[GlbPerson] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], PER_PK, PER_FullName, PER_MobilePhone2) VALUES (0x10, 0x01, 4, 0x0, 1803, 1, '{0}', '{1}', '{2}')",
				"INSERT [biadmin].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x10, '2018-03-15'); INSERT INTO [biadmin].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES (0x10, 'dbo', 'GlbPerson', 2, 1803);",
				"Queued Deduplication in GlbPersonPhoneNumberSubscriber for [PER_PK:{0}]. Row: Modified - {0} -  -  -  - {1}");
		}

		void GlbPersonPhoneNumberProcessesChangesForExistingRecords(string insertIntoCDCQuery, string insertIntoCDCUpdateQuery, string insertLsnMappingQuery, string expectedLog)
		{
			GlbPersonPhoneNumberProcessesChangesForExistingRecords(insertIntoCDCQuery, insertIntoCDCUpdateQuery, insertLsnMappingQuery, expectedLog, true);
			GlbPersonPhoneNumberProcessesChangesForExistingRecords(insertIntoCDCQuery, insertIntoCDCUpdateQuery, insertLsnMappingQuery, expectedLog, false);
		}

		void GlbPersonPhoneNumberProcessesChangesForExistingRecords(string insertIntoCDCQuery, string insertIntoCDCUpdateQuery, string insertLsnMappingQuery, string expectedLog, bool createOriginalPatternData)
		{
			var factory = new BusinessObjectFactory();
			var glbPerson = SubscriberTestUtilities.CreateTestGlbPerson(factory);
			var originalHashedValue = TextStandardizerHelper.ComputeStringHashFast("32541356");

			if (createOriginalPatternData)
			{
				SubscriberTestUtilities.CreatePatternMatchingBusinessObjectForPerson<PatternMatchingPhone>(factory, glbPerson.PK, originalHashedValue, GlbPersonSchema.Constants.Prefix, glbPerson.PK, glbPerson.PER_RN_NKCountry);
			}

			factory.Save();

			using (var auditConnection = AuditTestHelper.GetAuditConnection())
			{
				var testSubscribers = new ActualDataChangesAuditSubscriber[] {
					NewDataChangeSubscriber()
				};

				AuditTestHelper.CleanupAuditTestData(auditConnection, testSubscribers);
				AuditTestHelper.ResetSubscriberControlWaterMark(auditConnection, testSubscribers);

				var insertGlbPersonOriginalCDC = string.Format(CultureInfo.InvariantCulture, insertIntoCDCQuery, glbPerson.PK, glbPerson.PER_FullName, "32541356");
				auditConnection.ExecuteNonQuery(insertGlbPersonOriginalCDC);

				var phoneNumber = "87654321";
				var insertGlbPersonCurrentCDC = string.Format(CultureInfo.InvariantCulture, insertIntoCDCUpdateQuery, glbPerson.PK, glbPerson.PER_FullName, phoneNumber);
				auditConnection.ExecuteNonQuery(insertGlbPersonCurrentCDC);

				auditConnection.ExecuteNonQuery(insertLsnMappingQuery);

				AuditTestHelper.RunNotificationCycleAndAssert(
					auditConnection,
					testSubscribers,
					"0x10", "2018-03-15 00:00:00.000",
					expectedLog: new BetterLogForTest(LogType.Information, string.Format(CultureInfo.InvariantCulture, expectedLog, glbPerson.PK, phoneNumber))
				);

				var hashedValue = TextStandardizerHelper.ComputeStringHashFast(phoneNumber);

				var patternMatchingPhone = factory.Load<PatternMatchingPhone>(new ZQuery(PatternMatchingPhoneSchema.PMP_ParentId, glbPerson.PK)).ToList();
				AssertEquals(1, patternMatchingPhone.Count);

				var firstPatternMatchingPhone = patternMatchingPhone.Single();
				AssertEquals(hashedValue, firstPatternMatchingPhone.PMP_HashedValue);
				AssertEquals(glbPerson.PK, firstPatternMatchingPhone.PMP_PER);
				AssertEquals(GlbPersonSchema.Constants.Prefix, firstPatternMatchingPhone.PMP_ParentTableCode);
				AssertEquals(glbPerson.PK, firstPatternMatchingPhone.PMP_ParentId);
				AssertEquals(glbPerson.PER_RN_NKCountry, firstPatternMatchingPhone.PMP_RN_NKCountryCode);
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestGlbPersonPhoneNumberDoesNotProcessChangesForNonSubscribedColumns()
		{
			var factory = new BusinessObjectFactory();
			var glbPerson = SubscriberTestUtilities.CreateTestGlbPerson(factory);
			SubscriberTestUtilities.CreatePatternMatchingBusinessObjectForPerson<PatternMatchingPhone>(factory, glbPerson.PK, 0, GlbPersonSchema.Constants.Prefix, glbPerson.PK, glbPerson.PER_RN_NKCountry);

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

				var patternMatchingPhone = factory.Load<PatternMatchingPhone>(new ZQuery(PatternMatchingPhoneSchema.PMP_ParentId, glbPerson.PK)).ToList();
				AssertEquals(1, patternMatchingPhone.Count);

				var firstPatternMatchingPhone = patternMatchingPhone.Single();
				AssertEquals(0, firstPatternMatchingPhone.PMP_HashedValue);
			}
		}
		public override void TestCustomFilter()
		{
			var subscriber = new GlbPersonPhoneNumberSubscriber();

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
				row[GlbPersonSchema.PER_FaxNumber.Name] = DBNull.Value;
				row[GlbPersonSchema.PER_HomePhone.Name] = DBNull.Value;
				row[GlbPersonSchema.PER_MobilePhone.Name] = DBNull.Value;
				row[GlbPersonSchema.PER_MobilePhone2.Name] = DBNull.Value;
			}
			else
			{
				row[GlbPersonSchema.PER_FaxNumber.Name] = "PhoneNumber";
				row[GlbPersonSchema.PER_HomePhone.Name] = "PhoneNumber";
				row[GlbPersonSchema.PER_MobilePhone.Name] = "PhoneNumber";
				row[GlbPersonSchema.PER_MobilePhone2.Name] = "PhoneNumber";
			}
			table.Rows.Add(row);
			return row;
		}

		protected override DataTable GetTestDataTable()
		{
			var changeTable = new DataTable();
			var tableColumns = new SchemaColumn[] { GlbPersonSchema.PER_FaxNumber, GlbPersonSchema.PER_HomePhone, GlbPersonSchema.PER_MobilePhone, GlbPersonSchema.PER_MobilePhone2 };
			foreach (var column in tableColumns)
			{
				changeTable.Columns.Add(column.Name, column.DotNetType);
			}
			return changeTable;
		}
	}
}
