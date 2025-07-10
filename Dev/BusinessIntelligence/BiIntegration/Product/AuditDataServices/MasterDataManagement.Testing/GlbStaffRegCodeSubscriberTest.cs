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
	[TestedType(typeof(GlbStaffRegCodeSubscriber))]
	class GlbStaffRegCodeSubscriberTest : ActualDataChangesAuditSubscriberTest
	{
		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestGlbStaffRegCodeProcessesChangesForNewRecords()
		{
			GlbStaffRegCodeProcessesChangesForNewRecordsImplementation(
				"INSERT [dbo].[GlbStaff] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], GS_PK, GS_Code, GS_PER, GS_Passport) VALUES (0x10, 0x01, 2, 0x0, 1803, 1, '{0}', '{1}', '{2}', '{3}')",
				"INSERT [biadmin].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x10, '2018-06-08'); INSERT INTO [biadmin].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES (0x10, 'dbo', 'GlbStaff', 1, 1803);",
				"Queued Deduplication in GlbStaffRegCodeSubscriber for [PER_PK:{0}]. Row: Added - {1} - {2} -  - ", 1, CodeTypeIdentifier.Passport);
			GlbStaffRegCodeProcessesChangesForNewRecordsImplementation(
				"INSERT [dbo].[GlbStaff] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], GS_PK, GS_Code, GS_PER, GS_EnterpriseCertificationID) VALUES (0x10, 0x01, 2, 0x0, 1803, 1, '{0}', '{1}', '{2}', '{3}')",
				"INSERT [biadmin].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x10, '2018-06-08'); INSERT INTO [biadmin].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES (0x10, 'dbo', 'GlbStaff', 1, 1803);",
				"Queued Deduplication in GlbStaffRegCodeSubscriber for [PER_PK:{0}]. Row: Added - {1} -  - {2} - ", 2, "CER");
			GlbStaffRegCodeProcessesChangesForNewRecordsImplementation(
				"INSERT [dbo].[GlbStaff] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], GS_PK, GS_Code, GS_PER, GS_Birthdate) VALUES (0x10, 0x01, 2, 0x0, 1803, 1, '{0}', '{1}', '{2}', '{3}')",
				"INSERT [biadmin].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x10, '2018-06-08'); INSERT INTO [biadmin].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES (0x10, 'dbo', 'GlbStaff', 1, 1803);",
				"Queued Deduplication in GlbStaffRegCodeSubscriber for [PER_PK:{0}]. Row: Added - {1} -  -  - {2}", 4, "");
		}

		void GlbStaffRegCodeProcessesChangesForNewRecordsImplementation(string insertIntoCDCQuery, string insertLsnMappingQuery, string expectedLog, int batch, string prefix)
		{
			var factory = new BusinessObjectFactory();
			var person = SubscriberTestUtilities.CreateTestGlbPerson(factory);
			var staff = factory.New<GlbStaff>();

			staff.GS_Code = "RE" + batch;
			staff.GS_PER = person.PK;
			staff.GS_LoginName = "eddie" + batch;
			staff.GS_RN_NKCountryCode = "AU";

			factory.Save();

			using (var auditConnection = AuditTestHelper.GetAuditConnection())
			{
				var testSubscribers = new ActualDataChangesAuditSubscriber[] {
					NewDataChangeSubscriber()
				};

				AuditTestHelper.CleanupAuditTestData(auditConnection, testSubscribers);
				AuditTestHelper.ResetSubscriberControlWaterMark(auditConnection, testSubscribers);

				var patternMatchingRegCode = factory.Load<PatternMatchingRegCode>(new ZQuery(PatternMatchingRegCodeSchema.PMR_ParentId, staff.PK)).ToList();
				AssertEquals(0, patternMatchingRegCode.Count);

				var regCode = "1991-02-03";
				var insertGlbStaffCDC = string.Format(CultureInfo.InvariantCulture, insertIntoCDCQuery, staff.PK, staff.GS_Code, person.PK, regCode);
				auditConnection.ExecuteNonQuery(insertGlbStaffCDC);

				auditConnection.ExecuteNonQuery(insertLsnMappingQuery);

				AuditTestHelper.RunNotificationCycleAndAssert(
					auditConnection,
					testSubscribers,
					"0x10",
					"2018-06-08",
					expectedLog: new BetterLogForTest(LogType.Information, string.Format(CultureInfo.InvariantCulture, expectedLog, staff.Person.PK, staff.PK, GetLogStr(regCode, batch == 4)))
				);

				patternMatchingRegCode = factory.Load<PatternMatchingRegCode>(new ZQuery(PatternMatchingRegCodeSchema.PMR_ParentId, staff.PK)).ToList();
				AssertEquals(1, patternMatchingRegCode.Count);

				var firstPatternMatchingRegCode = patternMatchingRegCode.Single();

				AssertEquals(GetHashedValue(regCode, batch == 4, prefix), firstPatternMatchingRegCode.PMR_HashedValue);
				AssertEquals(staff.Person.PK, firstPatternMatchingRegCode.PMR_PER);
				AssertEquals(GlbStaffSchema.Constants.Prefix, firstPatternMatchingRegCode.PMR_ParentTableCode);
				AssertEquals(staff.PK, firstPatternMatchingRegCode.PMR_ParentId);
				AssertEquals(person.PER_RN_NKCountry, firstPatternMatchingRegCode.PMR_RN_NKCountryCode);
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestGlbStaffRegCodeNoPatternMatchingForPlaceholderRecords()
		{
			var insertIntoCDCQuery = @"
				INSERT [dbo].[GlbStaff] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], GS_PK, GS_Code, GS_PER, GS_Passport) VALUES 
					(0x10, 0x01, 2, 0x0, 1803, 1, '{0}', '{1}', '{2}', '{3}')
				INSERT [biadmin].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x10, '2018-06-08');
				INSERT INTO [biadmin].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES (0x10, 'dbo', 'GlbStaff', 1, 1803);";
			var expectedLog = "Queued Deduplication in GlbStaffRegCodeSubscriber for [PER_PK:{0}]. Row: Added - {1} - {2} -  - ";
			var factory = new BusinessObjectFactory();
			var person = SubscriberTestUtilities.CreateTestGlbPerson(factory);
			var staff = factory.New<GlbStaff>();

			staff.GS_Code = "EER";
			staff.GS_PER = person.PK;

			factory.Save();

			using (var auditConnection = AuditTestHelper.GetAuditConnection())
			{
				var testSubscribers = new ActualDataChangesAuditSubscriber[] {
					NewDataChangeSubscriber()
				};

				AuditTestHelper.CleanupAuditTestData(auditConnection, testSubscribers);
				AuditTestHelper.ResetSubscriberControlWaterMark(auditConnection, testSubscribers);

				var patternMatchingRegCode = factory.Load<PatternMatchingRegCode>(new ZQuery(PatternMatchingRegCodeSchema.PMR_ParentId, staff.PK)).ToList();
				AssertEquals(0, patternMatchingRegCode.Count);

				var regCode = "111";
				var insertGlbStaffCDC = string.Format(CultureInfo.InvariantCulture, insertIntoCDCQuery, staff.PK, staff.GS_Code, person.PK, regCode);
				auditConnection.ExecuteNonQuery(insertGlbStaffCDC);

				AuditTestHelper.RunNotificationCycleAndAssert(
					auditConnection,
					testSubscribers,
					"0x10", "2018-06-08 00:00:00.000",
					expectedLog: new BetterLogForTest(LogType.Information, string.Format(CultureInfo.InvariantCulture, expectedLog, staff.Person.PK, staff.PK, regCode))
				);

				patternMatchingRegCode = factory.Load<PatternMatchingRegCode>(new ZQuery(PatternMatchingRegCodeSchema.PMR_ParentId, staff.PK)).ToList();
				AssertEquals(0, patternMatchingRegCode.Count);
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestGlbStaffRegCodeProcessesChangesForExistingRecords()
		{
			GlbStaffRegCodeProcessesChangesForExistingRecords(
				"INSERT [dbo].[GlbStaff] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], GS_PK, GS_Code, GS_PER, GS_Passport) VALUES (0x10, 0x01, 3, 0x0, 1803, 1, '{0}', '{1}', '{2}', '{3}')",
				"INSERT [dbo].[GlbStaff] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], GS_PK, GS_Code, GS_PER, GS_Passport) VALUES (0x10, 0x01, 4, 0x0, 1803, 1, '{0}', '{1}', '{2}', '{3}')",
				"INSERT [biadmin].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x10, '2018-06-08'); INSERT INTO [biadmin].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES (0x10, 'dbo', 'GlbStaff', 2, 1803);",
				"Queued Deduplication in GlbStaffRegCodeSubscriber for [PER_PK:{0}]. Row: Modified - {1} - {2} -  - ", 1, CodeTypeIdentifier.Passport);
			GlbStaffRegCodeProcessesChangesForExistingRecords(
				"INSERT [dbo].[GlbStaff] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], GS_PK, GS_Code, GS_PER, GS_EnterpriseCertificationID) VALUES (0x10, 0x01, 3, 0x0, 1803, 1, '{0}', '{1}', '{2}', '{3}')",
				"INSERT [dbo].[GlbStaff] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], GS_PK, GS_Code, GS_PER, GS_EnterpriseCertificationID) VALUES (0x10, 0x01, 4, 0x0, 1803, 1, '{0}', '{1}', '{2}', '{3}')",
				"INSERT [biadmin].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x10, '2018-06-08'); INSERT INTO [biadmin].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES (0x10, 'dbo', 'GlbStaff', 2, 1803);",
				"Queued Deduplication in GlbStaffRegCodeSubscriber for [PER_PK:{0}]. Row: Modified - {1} -  - {2} - ", 2, "CER");
			GlbStaffRegCodeProcessesChangesForExistingRecords(
				"INSERT [dbo].[GlbStaff] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], GS_PK, GS_Code, GS_PER, GS_Birthdate) VALUES (0x10, 0x01, 3, 0x0, 1803, 1, '{0}', '{1}', '{2}', '{3}')",
				"INSERT [dbo].[GlbStaff] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], GS_PK, GS_Code, GS_PER, GS_Birthdate) VALUES (0x10, 0x01, 4, 0x0, 1803, 1, '{0}', '{1}', '{2}', '{3}')",
				"INSERT [biadmin].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x10, '2018-06-08'); INSERT INTO [biadmin].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES (0x10, 'dbo', 'GlbStaff', 2, 1803);",
				"Queued Deduplication in GlbStaffRegCodeSubscriber for [PER_PK:{0}]. Row: Modified - {1} -  -  - {2}", 4, "");
		}

		void GlbStaffRegCodeProcessesChangesForExistingRecords(string insertIntoCDCQuery, string insertIntoCDCUpdateQuery, string insertLsnMappingQuery, string expectedLog, int batch, string prefix)
		{
			var factory = new BusinessObjectFactory();
			var person = SubscriberTestUtilities.CreateTestGlbPerson(factory);
			var staff = factory.New<GlbStaff>();

			staff.GS_Code = "XX" + batch;
			staff.GS_PER = person.PK;
			staff.GS_LoginName = "eddie" + batch;
			staff.GS_RN_NKCountryCode = "AU";

			var originalValue = "1990-01-01";
			var originalHashedValue = GetHashedValue(originalValue, batch == 4, prefix);

			SubscriberTestUtilities.CreatePatternMatchingBusinessObjectForPerson<PatternMatchingRegCode>(factory, person.PK, originalHashedValue, GlbStaffSchema.Constants.Prefix, staff.PK, person.PER_RN_NKCountry);

			factory.Save();

			using (var auditConnection = AuditTestHelper.GetAuditConnection())
			{
				var testSubscribers = new ActualDataChangesAuditSubscriber[] {
					NewDataChangeSubscriber()
				};

				AuditTestHelper.CleanupAuditTestData(auditConnection, testSubscribers);
				AuditTestHelper.ResetSubscriberControlWaterMark(auditConnection, testSubscribers);

				var insertGlbStaffOriginalCDC = string.Format(CultureInfo.InvariantCulture, insertIntoCDCQuery, staff.PK, staff.GS_Code, person.PK, originalValue);
				auditConnection.ExecuteNonQuery(insertGlbStaffOriginalCDC);

				var regCode = "1991-02-03";
				var insertGlbStaffCurrentCDC = string.Format(CultureInfo.InvariantCulture, insertIntoCDCUpdateQuery, staff.PK, staff.GS_Code, person.PK, regCode);
				auditConnection.ExecuteNonQuery(insertGlbStaffCurrentCDC);

				auditConnection.ExecuteNonQuery(insertLsnMappingQuery);

				AuditTestHelper.RunNotificationCycleAndAssert(
					auditConnection,
					testSubscribers,
					"0x10", "2018-06-08 00:00:00.000",
					expectedLog: new BetterLogForTest(LogType.Information, string.Format(CultureInfo.InvariantCulture, expectedLog, staff.Person.PK, staff.PK, GetLogStr(regCode, batch == 4)))
				);

				var patternMatchingRegCode = factory.Load<PatternMatchingRegCode>(new ZQuery(PatternMatchingRegCodeSchema.PMR_ParentId, staff.PK)).ToList();
				AssertEquals(1, patternMatchingRegCode.Count);

				var firstPatternMatchingRegCode = patternMatchingRegCode.Single();

				AssertEquals(GetHashedValue(regCode, batch == 4, prefix), firstPatternMatchingRegCode.PMR_HashedValue);
				AssertEquals(person.PK, firstPatternMatchingRegCode.PMR_PER);
				AssertEquals(GlbStaffSchema.Constants.Prefix, firstPatternMatchingRegCode.PMR_ParentTableCode);
				AssertEquals(staff.PK, firstPatternMatchingRegCode.PMR_ParentId);
				AssertEquals(person.PER_RN_NKCountry, firstPatternMatchingRegCode.PMR_RN_NKCountryCode);
			}
		}

		readonly DateTime baseLineDate = new DateTime(1753, 1, 1);

		int GetHashedValue(string valueToHash, bool isBirthdate, string prefix)
		{
			if (isBirthdate)
			{
				valueToHash = (Convert.ToDateTime(valueToHash) - baseLineDate).Days.ToString();
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
		public void TestGlbStaffRegCodeDoesNotProcessChangesForNonSubscribedColumns()
		{
			var factory = new BusinessObjectFactory();
			var person = SubscriberTestUtilities.CreateTestGlbPerson(factory);
			var staff = factory.New<GlbStaff>();

			staff.GS_Code = "ABR";
			staff.GS_PER = person.PK;

			SubscriberTestUtilities.CreatePatternMatchingBusinessObjectForPerson<PatternMatchingRegCode>(factory, person.PK, 0, GlbStaffSchema.Constants.Prefix, staff.PK, person.PER_RN_NKCountry);

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

				var patternMatchingRegCode = factory.Load<PatternMatchingRegCode>(new ZQuery(PatternMatchingRegCodeSchema.PMR_ParentId, staff.PK)).ToList();
				AssertEquals(1, patternMatchingRegCode.Count);

				var firstPatternMatchingRegCode = patternMatchingRegCode.Single();
				AssertEquals(0, firstPatternMatchingRegCode.PMR_HashedValue);
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

				var patternMatching = factory.Load<PatternMatchingRegCode>(new ZQuery(PatternMatchingRegCodeSchema.PMR_ParentId, bizO.PK)).ToList();
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

				patternMatching = factory.Load<PatternMatchingRegCode>(new ZQuery(PatternMatchingRegCodeSchema.PMR_ParentId, bizO.PK)).ToList();
				AssertEquals(0, patternMatching.Count);
			}
		}

		public override void TestCustomFilter()
		{
			var subscriber = new GlbStaffRegCodeSubscriber();

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
				row[GlbStaffSchema.GS_Passport.Name] = DBNull.Value;
				row[GlbStaffSchema.GS_EnterpriseCertificationID.Name] = DBNull.Value;
				row[GlbStaffSchema.GS_Birthdate.Name] = DBNull.Value;
			}
			else
			{
				row[GlbStaffSchema.GS_Passport.Name] = "Passport";
				row[GlbStaffSchema.GS_EnterpriseCertificationID.Name] = "ID";
				row[GlbStaffSchema.GS_Birthdate.Name] = DateTime.Now;
			}
			table.Rows.Add(row);
			return row;
		}

		protected override DataTable GetTestDataTable()
		{
			var changeTable = new DataTable();
			var tableColumns = new SchemaColumn[] { GlbStaffSchema.GS_Passport, GlbStaffSchema.GS_EnterpriseCertificationID, GlbStaffSchema.GS_Birthdate };
			foreach (var column in tableColumns)
			{
				changeTable.Columns.Add(column.Name, column.DotNetType);
			}
			return changeTable;
		}
	}
}
