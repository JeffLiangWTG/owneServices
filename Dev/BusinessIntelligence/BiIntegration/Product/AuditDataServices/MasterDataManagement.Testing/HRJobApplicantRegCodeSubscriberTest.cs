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
	[TestedType(typeof(HRJobApplicantRegCodeSubscriber))]
	class HRJobApplicantRegCodeSubscriberTest : ActualDataChangesAuditSubscriberTest
	{
		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestHRJobApplicantRegCodeProcessesChangesForNewRecords()
		{
			HRJobApplicantRegCodeProcessesChangesForNewRecordsImplementation(
				"INSERT [dbo].[HRJobApplicant] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], HA_PK, HA_PER, HA_OtherIdentityDocument) VALUES (0x10, 0x01, 2, 0x0, 1803, 1, '{0}', '{1}', '{2}')",
				"INSERT [biadmin].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x10, '2018-06-08'); INSERT INTO [biadmin].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES (0x10, 'dbo', 'HRJobApplicant', 1, 1803);",
				"Queued Deduplication in HRJobApplicantRegCodeSubscriber for [PER_PK:{0}]. Row: Added - {1} - {2}", 3, CodeTypeIdentifier.Other);
		}

		void HRJobApplicantRegCodeProcessesChangesForNewRecordsImplementation(string insertIntoCDCQuery, string insertLsnMappingQuery, string expectedLog, int batch, string prefix)
		{
			var factory = new BusinessObjectFactory();

			var glbPerson = SubscriberTestUtilities.CreateTestGlbPerson(factory);

			var jobApplicant = factory.New<HRJobApplicant>();
			jobApplicant.HA_PER = glbPerson.PK;
			jobApplicant.HA_EmailAddress = "test@test.com" + batch;
			jobApplicant.HA_RN_NKCountry = "AU";

			factory.Save();

			using (var auditConnection = AuditTestHelper.GetAuditConnection())
			{
				var testSubscribers = new ActualDataChangesAuditSubscriber[] {
					NewDataChangeSubscriber()
				};

				AuditTestHelper.CleanupAuditTestData(auditConnection, testSubscribers);
				AuditTestHelper.ResetSubscriberControlWaterMark(auditConnection, testSubscribers);

				var patternMatchingRegCode = factory.Load<PatternMatchingRegCode>(new ZQuery(PatternMatchingRegCodeSchema.PMR_ParentId, jobApplicant.PK)).ToList();
				AssertEquals(0, patternMatchingRegCode.Count);

				var regCode = "1991-02-03";
				var insertHRJobApplicantCDC = string.Format(CultureInfo.InvariantCulture, insertIntoCDCQuery, jobApplicant.PK, glbPerson.PK, regCode);
				auditConnection.ExecuteNonQuery(insertHRJobApplicantCDC);

				auditConnection.ExecuteNonQuery(insertLsnMappingQuery);

				AuditTestHelper.RunNotificationCycleAndAssert(
					auditConnection,
					testSubscribers,
					"0x10", "2018-06-08 00:00:00.000",
					expectedLog: new BetterLogForTest(LogType.Information, string.Format(CultureInfo.InvariantCulture, expectedLog, jobApplicant.Person.PK, jobApplicant.PK, GetLogStr(regCode, batch == 4)))
				);

				patternMatchingRegCode = factory.Load<PatternMatchingRegCode>(new ZQuery(PatternMatchingRegCodeSchema.PMR_ParentId, jobApplicant.PK)).ToList();
				AssertEquals(1, patternMatchingRegCode.Count);

				var firstPatternMatchingRegCode = patternMatchingRegCode.Single();

				AssertEquals(GetHashedValue(regCode, batch == 4, prefix), firstPatternMatchingRegCode.PMR_HashedValue);
				AssertEquals(jobApplicant.Person.PK, firstPatternMatchingRegCode.PMR_PER);
				AssertEquals(HRJobApplicantSchema.Constants.Prefix, firstPatternMatchingRegCode.PMR_ParentTableCode);
				AssertEquals(jobApplicant.PK, firstPatternMatchingRegCode.PMR_ParentId);
				AssertEquals(glbPerson.PER_RN_NKCountry, firstPatternMatchingRegCode.PMR_RN_NKCountryCode);
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestHRJobApplicantRegCodeNoPatternMatchingForPlaceholderRecords()
		{
			var insertIntoCDCQuery = @"
				INSERT [dbo].[HRJobApplicant] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], HA_PK, HA_PER, HA_OtherIdentityDocument) VALUES 
					(0x10, 0x01, 2, 0x0, 1803, 1, '{0}', '{1}', '{2}')
				INSERT [biadmin].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x10, '2018-06-08');
				INSERT INTO [biadmin].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES (0x10, 'dbo', 'HRJobApplicant', 1, 1803);";

			var expectedLog = "Queued Deduplication in HRJobApplicantRegCodeSubscriber for [PER_PK:{0}]. Row: Added - {1} - {2}";

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

				var patternMatchingRegCode = factory.Load<PatternMatchingRegCode>(new ZQuery(PatternMatchingRegCodeSchema.PMR_ParentId, jobApplicant.PK)).ToList();
				AssertEquals(0, patternMatchingRegCode.Count);

				var regCode = "111";
				var insertHRJobApplicantCDC = string.Format(CultureInfo.InvariantCulture, insertIntoCDCQuery, jobApplicant.PK, glbPerson.PK, regCode);
				auditConnection.ExecuteNonQuery(insertHRJobApplicantCDC);

				AuditTestHelper.RunNotificationCycleAndAssert(
					auditConnection,
					testSubscribers,
					"0x10", "2018-06-08 00:00:00.000",
					expectedLog: new BetterLogForTest(LogType.Information, string.Format(CultureInfo.InvariantCulture, expectedLog, jobApplicant.Person.PK, jobApplicant.PK, regCode))
				);

				patternMatchingRegCode = factory.Load<PatternMatchingRegCode>(new ZQuery(PatternMatchingRegCodeSchema.PMR_ParentId, jobApplicant.PK)).ToList();
				AssertEquals(0, patternMatchingRegCode.Count);
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestHRJobApplicantRegCodeProcessesChangesForExistingRecords()
		{
			HRJobApplicantRegCodeProcessesChangesForExistingRecords(
			"INSERT [dbo].[HRJobApplicant] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], HA_PK, HA_PER, HA_OtherIdentityDocument) VALUES (0x10, 0x01, 3, 0x0, 1803, 1, '{0}', '{1}', '{2}')",
			"INSERT [dbo].[HRJobApplicant] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], HA_PK, HA_PER, HA_OtherIdentityDocument) VALUES (0x10, 0x01, 4, 0x0, 1803, 1, '{0}', '{1}', '{2}')",
			"INSERT [biadmin].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x10, '2018-06-08'); INSERT INTO [biadmin].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES (0x10, 'dbo', 'HRJobApplicant', 2, 1803);",
			"Queued Deduplication in HRJobApplicantRegCodeSubscriber for [PER_PK:{0}]. Row: Modified - {1} - {2}", 3, CodeTypeIdentifier.Other);
		}

		void HRJobApplicantRegCodeProcessesChangesForExistingRecords(string insertIntoCDCQuery, string insertIntoCDCUpdateQuery, string insertLsnMappingQuery, string expectedLog, int batch, string prefix)
		{
			var factory = new BusinessObjectFactory();

			var glbPerson = SubscriberTestUtilities.CreateTestGlbPerson(factory);

			var jobApplicant = factory.New<HRJobApplicant>();
			jobApplicant.HA_PER = glbPerson.PK;
			jobApplicant.HA_EmailAddress = "test@test.com" + batch;
			jobApplicant.HA_RN_NKCountry = "AU";

			var originalValue = "1990-01-01";
			var originalHashedValue = GetHashedValue(originalValue, batch == 4, prefix);

			SubscriberTestUtilities.CreatePatternMatchingBusinessObjectForPerson<PatternMatchingRegCode>(factory, glbPerson.PK, originalHashedValue, HRJobApplicantSchema.Constants.Prefix, jobApplicant.PK, glbPerson.PER_RN_NKCountry);

			factory.Save();

			using (var auditConnection = AuditTestHelper.GetAuditConnection())
			{
				var testSubscribers = new ActualDataChangesAuditSubscriber[] {
					NewDataChangeSubscriber()
				};

				AuditTestHelper.CleanupAuditTestData(auditConnection, testSubscribers);
				AuditTestHelper.ResetSubscriberControlWaterMark(auditConnection, testSubscribers);

				var insertHRJobApplicantOriginalCDC = string.Format(CultureInfo.InvariantCulture, insertIntoCDCQuery, jobApplicant.PK, glbPerson.PK, originalValue);
				auditConnection.ExecuteNonQuery(insertHRJobApplicantOriginalCDC);

				var regCode = "1991-02-03";
				var insertHRJobApplicantCurrentCDC = string.Format(CultureInfo.InvariantCulture, insertIntoCDCUpdateQuery, jobApplicant.PK, glbPerson.PK, regCode);
				auditConnection.ExecuteNonQuery(insertHRJobApplicantCurrentCDC);

				auditConnection.ExecuteNonQuery(insertLsnMappingQuery);

				AuditTestHelper.RunNotificationCycleAndAssert(
					auditConnection,
					testSubscribers,
					"0x10", "2018-06-08 00:00:00.000",
					expectedLog: new BetterLogForTest(LogType.Information, string.Format(CultureInfo.InvariantCulture, expectedLog, jobApplicant.Person.PK, jobApplicant.PK, GetLogStr(regCode, batch == 4)))
				);

				var patternMatchingRegCode = factory.Load<PatternMatchingRegCode>(new ZQuery(PatternMatchingRegCodeSchema.PMR_ParentId, jobApplicant.PK)).ToList();
				AssertEquals(1, patternMatchingRegCode.Count);

				var firstPatternMatchingRegCode = patternMatchingRegCode.Single();

				AssertEquals(GetHashedValue(regCode, batch == 4, prefix), firstPatternMatchingRegCode.PMR_HashedValue);
				AssertEquals(glbPerson.PK, firstPatternMatchingRegCode.PMR_PER);
				AssertEquals(HRJobApplicantSchema.Constants.Prefix, firstPatternMatchingRegCode.PMR_ParentTableCode);
				AssertEquals(jobApplicant.PK, firstPatternMatchingRegCode.PMR_ParentId);
				AssertEquals(glbPerson.PER_RN_NKCountry, firstPatternMatchingRegCode.PMR_RN_NKCountryCode);
			}
		}

		readonly DateTime beginDate = new DateTime(1753, 1, 1);

		int GetHashedValue(string valueToHash, bool isBirthdate, string prefix)
		{
			if (isBirthdate)
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
		public void TestHRJobApplicantRegCodeDoesNotProcessChangesForNonSubscribedColumns()
		{
			var factory = new BusinessObjectFactory();

			var glbPerson = SubscriberTestUtilities.CreateTestGlbPerson(factory);

			var jobApplicant = factory.New<HRJobApplicant>();
			jobApplicant.HA_PER = glbPerson.PK;

			SubscriberTestUtilities.CreatePatternMatchingBusinessObjectForPerson<PatternMatchingRegCode>(factory, glbPerson.PK, 0, HRJobApplicantSchema.Constants.Prefix, jobApplicant.PK, glbPerson.PER_RN_NKCountry);

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

				var patternMatchingRegCode = factory.Load<PatternMatchingRegCode>(new ZQuery(PatternMatchingRegCodeSchema.PMR_ParentId, jobApplicant.PK)).ToList();
				AssertEquals(1, patternMatchingRegCode.Count);

				var firstPatternMatchingRegCode = patternMatchingRegCode.Single();
				AssertEquals(0, firstPatternMatchingRegCode.PMR_HashedValue);
			}
		}

		public override void TestCustomFilter()
		{
			var subscriber = new HRJobApplicantRegCodeSubscriber();

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
				row[HRJobApplicantSchema.HA_OtherIdentityDocument.Name] = DBNull.Value;
			}
			else
			{
				row[HRJobApplicantSchema.HA_OtherIdentityDocument.Name] = "Document";
			}
			table.Rows.Add(row);
			return row;
		}

		protected override DataTable GetTestDataTable()
		{
			var changeTable = new DataTable();
			var tableColumns = new SchemaColumn[] { HRJobApplicantSchema.HA_OtherIdentityDocument };
			foreach (var column in tableColumns)
			{
				changeTable.Columns.Add(column.Name, column.DotNetType);
			}
			return changeTable;
		}
	}
}
