
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
using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.AuditDataServices.MDM.Testing
{
	[TestedType(typeof(HRJobApplicantPhoneNumberSubscriber))]
	class HRJobApplicantPhoneNumberSubscriberTest : ActualDataChangesAuditSubscriberTest
	{
		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestHRJobApplicantPhoneNumberProcessesChangesForNewRecords()
		{
			HRJobApplicantPhoneNumberProcessesChangesForNewRecordsImplementation(
				"INSERT [dbo].[HRJobApplicant] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], HA_PK, HA_PER, HA_WorkPhone) VALUES (0x10, 0x01, 2, 0x0, 1803, 1, '{0}', '{1}', '{2}')",
				"INSERT [biadmin].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x10, '2018-06-08'); INSERT INTO [biadmin].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES (0x10, 'dbo', 'HRJobApplicant', 1, 1803);",
				"Queued Deduplication in HRJobApplicantPhoneNumberSubscriber for [PER_PK:{0}]. Row: Added - {1} - {2}", 4);
		}

		void HRJobApplicantPhoneNumberProcessesChangesForNewRecordsImplementation(string insertIntoCDCQuery, string insertLsnMappingQuery, string expectedLog, int batch)
		{
			var factory = new BusinessObjectFactory();

			var glbPerson = SubscriberTestUtilities.CreateTestGlbPerson(factory);

			var jobApplicant = factory.New<HRJobApplicant>();
			jobApplicant.HA_PER = glbPerson.PK;
			jobApplicant.HA_EmailAddress = "test@test.com" + batch;

			factory.Save();

			using (var auditConnection = AuditTestHelper.GetAuditConnection())
			{
				var testSubscribers = new ActualDataChangesAuditSubscriber[] {
					NewDataChangeSubscriber()
				};

				AuditTestHelper.CleanupAuditTestData(auditConnection, testSubscribers);
				AuditTestHelper.ResetSubscriberControlWaterMark(auditConnection, testSubscribers);

				var patternMatchingPhone = factory.Load<PatternMatchingPhone>(new ZQuery(PatternMatchingPhoneSchema.PMP_ParentId, jobApplicant.PK)).ToList();
				AssertEquals(0, patternMatchingPhone.Count);

				var phoneNumber = "92041334";
				var insertHRJobApplicantCDC = string.Format(CultureInfo.InvariantCulture, insertIntoCDCQuery, jobApplicant.PK, glbPerson.PK, phoneNumber);
				auditConnection.ExecuteNonQuery(insertHRJobApplicantCDC);

				auditConnection.ExecuteNonQuery(insertLsnMappingQuery);

				AuditTestHelper.RunNotificationCycleAndAssert(
					auditConnection,
					testSubscribers,
					"0x10", "2018-06-08 00:00:00.000",
					expectedLog: new BetterLogForTest(LogType.Information, string.Format(CultureInfo.InvariantCulture, expectedLog, jobApplicant.Person.PK, jobApplicant.PK, phoneNumber))
				);

				patternMatchingPhone = factory.Load<PatternMatchingPhone>(new ZQuery(PatternMatchingPhoneSchema.PMP_ParentId, jobApplicant.PK)).ToList();
				AssertEquals(1, patternMatchingPhone.Count);

				var firstPatternMatchingPhone = patternMatchingPhone.Single();

				AssertEquals(TextStandardizerHelper.ComputeStringHashFast(phoneNumber), firstPatternMatchingPhone.PMP_HashedValue);
				AssertEquals(jobApplicant.Person.PK, firstPatternMatchingPhone.PMP_PER);
				AssertEquals(HRJobApplicantSchema.Constants.Prefix, firstPatternMatchingPhone.PMP_ParentTableCode);
				AssertEquals(jobApplicant.PK, firstPatternMatchingPhone.PMP_ParentId);
				AssertEquals(glbPerson.PER_RN_NKCountry, firstPatternMatchingPhone.PMP_RN_NKCountryCode);
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestHRJobApplicantPhoneNumberNoPatternMatchingForPlaceholderRecords()
		{
			var insertIntoCDCQuery = @"
				INSERT [dbo].[HRJobApplicant] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], HA_PK, HA_PER, HA_WorkPhone) VALUES 
					(0x10, 0x01, 2, 0x0, 1803, 1, '{0}', '{1}', '{2}')
				INSERT [biadmin].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x10, '2018-06-08'); INSERT INTO [biadmin].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES (0x10, 'dbo', 'HRJobApplicant', 1, 1803);";

			var expectedLog = "Queued Deduplication in HRJobApplicantPhoneNumberSubscriber for [PER_PK:{0}]. Row: Added - {1} - {2}";

			var factory = new BusinessObjectFactory();

			var glbPerson = SubscriberTestUtilities.CreateTestGlbPerson(factory);

			var jobApplicant = factory.New<HRJobApplicant>();
			jobApplicant.HA_PER = glbPerson.PK;

			factory.Save();

			using (var auditConnection = AuditTestHelper.GetAuditConnection())
			{
				var testSubscribers = new ActualDataChangesAuditSubscriber[] {
					NewDataChangeSubscriber()
				};

				AuditTestHelper.CleanupAuditTestData(auditConnection, testSubscribers);
				AuditTestHelper.ResetSubscriberControlWaterMark(auditConnection, testSubscribers);

				var patternMatchingPhone = factory.Load<PatternMatchingPhone>(new ZQuery(PatternMatchingPhoneSchema.PMP_ParentId, jobApplicant.PK)).ToList();
				AssertEquals(0, patternMatchingPhone.Count);

				var phoneNumber = "0000";
				var insertHRJobApplicantCDC = string.Format(CultureInfo.InvariantCulture, insertIntoCDCQuery, jobApplicant.PK, glbPerson.PK, phoneNumber);
				auditConnection.ExecuteNonQuery(insertHRJobApplicantCDC);

				AuditTestHelper.RunNotificationCycleAndAssert(
					auditConnection,
					testSubscribers,
					"0x10", "2018-06-08 00:00:00.000",
					expectedLog: new BetterLogForTest(LogType.Information, string.Format(CultureInfo.InvariantCulture, expectedLog, jobApplicant.Person.PK, jobApplicant.PK, phoneNumber))
				);

				patternMatchingPhone = factory.Load<PatternMatchingPhone>(new ZQuery(PatternMatchingPhoneSchema.PMP_ParentId, jobApplicant.PK)).ToList();
				AssertEquals(0, patternMatchingPhone.Count);
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestHRJobApplicantPhoneNumberProcessesChangesForExistingRecords()
		{
			HRJobApplicantPhoneNumberProcessesChangesForExistingRecords(
				"INSERT [dbo].[HRJobApplicant] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], HA_PK, HA_PER, HA_WorkPhone) VALUES (0x10, 0x01, 3, 0x0, 1803, 1, '{0}', '{1}', '{2}')",
				"INSERT [dbo].[HRJobApplicant] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], HA_PK, HA_PER, HA_WorkPhone) VALUES (0x10, 0x01, 4, 0x0, 1803, 1, '{0}', '{1}', '{2}')",
				"INSERT [biadmin].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x10, '2018-03-15'); INSERT INTO [biadmin].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES (0x10, 'dbo', 'HRJobApplicant', 2, 1803);",
				"Queued Deduplication in HRJobApplicantPhoneNumberSubscriber for [PER_PK:{0}]. Row: Modified - {1} - {2}", 4);
		}

		void HRJobApplicantPhoneNumberProcessesChangesForExistingRecords(string insertIntoCDCQuery, string insertIntoCDCUpdateQuery, string insertLsnMappingQuery, string expectedLog, int batch)
		{
			var factory = new BusinessObjectFactory();

			var glbPerson = SubscriberTestUtilities.CreateTestGlbPerson(factory);

			var jobApplicant = factory.New<HRJobApplicant>();
			jobApplicant.HA_PER = glbPerson.PK;
			jobApplicant.HA_EmailAddress = "test@test.com" + batch;
			jobApplicant.HA_RN_NKCountry = "AU";

			var originalHashedValue = TextStandardizerHelper.ComputeStringHashFast("32541356");

			SubscriberTestUtilities.CreatePatternMatchingBusinessObjectForPerson<PatternMatchingPhone>(factory, glbPerson.PK, originalHashedValue, HRJobApplicantSchema.Constants.Prefix, jobApplicant.PK, glbPerson.PER_RN_NKCountry);

			factory.Save();

			using (var auditConnection = AuditTestHelper.GetAuditConnection())
			{
				var testSubscribers = new ActualDataChangesAuditSubscriber[] {
					NewDataChangeSubscriber()
				};

				AuditTestHelper.CleanupAuditTestData(auditConnection, testSubscribers);
				AuditTestHelper.ResetSubscriberControlWaterMark(auditConnection, testSubscribers);

				var insertHRJobApplicantOriginalCDC = string.Format(CultureInfo.InvariantCulture, insertIntoCDCQuery, jobApplicant.PK, glbPerson.PK, "32541356");
				auditConnection.ExecuteNonQuery(insertHRJobApplicantOriginalCDC);

				var phoneNumber = "87654321";
				var insertHRJobApplicantCurrentCDC = string.Format(CultureInfo.InvariantCulture, insertIntoCDCUpdateQuery, jobApplicant.PK, glbPerson.PK, phoneNumber);
				auditConnection.ExecuteNonQuery(insertHRJobApplicantCurrentCDC);

				auditConnection.ExecuteNonQuery(insertLsnMappingQuery);

				AuditTestHelper.RunNotificationCycleAndAssert(
					auditConnection,
					testSubscribers,
					"0x10", "2018-03-15 00:00:00.000",
					expectedLog: new BetterLogForTest(LogType.Information, string.Format(CultureInfo.InvariantCulture, expectedLog, jobApplicant.Person.PK, jobApplicant.PK, phoneNumber))
				);

				var hashedValue = TextStandardizerHelper.ComputeStringHashFast(phoneNumber);

				var patternMatchingPhone = factory.Load<PatternMatchingPhone>(new ZQuery(PatternMatchingPhoneSchema.PMP_ParentId, jobApplicant.PK)).ToList();
				AssertEquals(1, patternMatchingPhone.Count);

				var firstPatternMatchingPhone = patternMatchingPhone.Single();
				AssertEquals(hashedValue, firstPatternMatchingPhone.PMP_HashedValue);
				AssertEquals(glbPerson.PK, firstPatternMatchingPhone.PMP_PER);
				AssertEquals(HRJobApplicantSchema.Constants.Prefix, firstPatternMatchingPhone.PMP_ParentTableCode);
				AssertEquals(jobApplicant.PK, firstPatternMatchingPhone.PMP_ParentId);
				AssertEquals(glbPerson.PER_RN_NKCountry, firstPatternMatchingPhone.PMP_RN_NKCountryCode);
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestHRJobApplicantPhoneNumberDoesNotProcessChangesForNonSubscribedColumns()
		{
			var factory = new BusinessObjectFactory();

			var glbPerson = SubscriberTestUtilities.CreateTestGlbPerson(factory);

			var jobApplicant = factory.New<HRJobApplicant>();
			jobApplicant.HA_PER = glbPerson.PK;

			SubscriberTestUtilities.CreatePatternMatchingBusinessObjectForPerson<PatternMatchingPhone>(factory, glbPerson.PK, 0, HRJobApplicantSchema.Constants.Prefix, jobApplicant.PK, glbPerson.PER_RN_NKCountry);

			factory.Save();

			using (var auditConnection = AuditTestHelper.GetAuditConnection())
			{
				var testSubscribers = new ActualDataChangesAuditSubscriber[] {
					NewDataChangeSubscriber()
				};
				AuditTestHelper.ResetSubscriberControlWaterMark(auditConnection, testSubscribers);

				var insertHRJobApplicantOriginalCDC = string.Format(CultureInfo.InvariantCulture, @"INSERT [dbo].[HRJobApplicant] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], HA_PK, HA_PER) VALUES 
					(0x10, 0x01, 3, 0x0, 0, 1, '{0}', '{1}');", jobApplicant.PK, glbPerson.PK);
				auditConnection.ExecuteNonQuery(insertHRJobApplicantOriginalCDC);

				var insertHRJobApplicantCurrentCDC = string.Format(CultureInfo.InvariantCulture, @"INSERT [dbo].[HRJobApplicant] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], HA_PK, HA_PER) VALUES 
					(0x10, 0x01, 4, 0x0, 0, 1, '{0}', '{1}');", jobApplicant.PK, glbPerson.PK);
				auditConnection.ExecuteNonQuery(insertHRJobApplicantCurrentCDC);

				AuditTestHelper.RunNotificationCycleAndAssert(
					auditConnection,
					testSubscribers,
					"0x10", "2018-06-08 00:00:00.000",
					expectedLog: null
				);

				var patternMatchingPhone = factory.Load<PatternMatchingPhone>(new ZQuery(PatternMatchingPhoneSchema.PMP_ParentId, jobApplicant.PK)).ToList();
				AssertEquals(1, patternMatchingPhone.Count);

				var firstPatternMatchingPhone = patternMatchingPhone.Single();
				AssertEquals(0, firstPatternMatchingPhone.PMP_HashedValue);
			}
		}

		public override void TestCustomFilter()
		{
			var subscriber = new HRJobApplicantPhoneNumberSubscriber();

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
				row[HRJobApplicantSchema.HA_WorkPhone.Name] = DBNull.Value;
			}
			else
			{
				row[HRJobApplicantSchema.HA_WorkPhone.Name] = "Phone";
			}
			table.Rows.Add(row);
			return row;
		}

		protected override DataTable GetTestDataTable()
		{
			var changeTable = new DataTable();
			var tableColumns = new SchemaColumn[] { HRJobApplicantSchema.HA_WorkPhone };
			foreach (var column in tableColumns)
			{
				changeTable.Columns.Add(column.Name, column.DotNetType);
			}
			return changeTable;
		}
	}
}
