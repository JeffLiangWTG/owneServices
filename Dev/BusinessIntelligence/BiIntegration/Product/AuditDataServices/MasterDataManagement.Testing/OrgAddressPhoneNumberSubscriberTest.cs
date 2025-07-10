
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
	[TestedType(typeof(OrgAddressPhoneNumberSubscriber))]
	class OrgAddressPhoneNumberSubscriberTest : ActualDataChangesAuditSubscriberTest
	{
		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestOrgAddressContactNumberProcessesChangesForNewRecords()
		{
			OrgAddressContactNumberProcessesChangesForNewRecordsImplementation(
				"INSERT [dbo].[OrgAddress] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], OA_PK, OA_OH, OA_Phone) VALUES (0x10, 0x01, 2, 0x0, 1803, 1, '{0}', '{1}', '{2}')",
				"INSERT [biadmin].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x10, '2018-03-15'); INSERT INTO [biadmin].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES (0x10, 'dbo', 'OrgAddress', 1, 1803);",
				"Queued Deduplication in OrgAddressPhoneNumberSubscriber for [OH_PK:{0}]. Row: Added - {1} - {2} -  - ");
			OrgAddressContactNumberProcessesChangesForNewRecordsImplementation(
				"INSERT [dbo].[OrgAddress] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], OA_PK, OA_OH, OA_Fax) VALUES (0x10, 0x01, 2, 0x0, 1803, 1, '{0}', '{1}', '{2}')",
				"INSERT [biadmin].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x10, '2018-03-15'); INSERT INTO [biadmin].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES (0x10, 'dbo', 'OrgAddress', 1, 1803);",
				"Queued Deduplication in OrgAddressPhoneNumberSubscriber for [OH_PK:{0}]. Row: Added - {1} -  - {2} - ");
			OrgAddressContactNumberProcessesChangesForNewRecordsImplementation(
				"INSERT [dbo].[OrgAddress] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], OA_PK, OA_OH, OA_Mobile) VALUES (0x10, 0x01, 2, 0x0, 1803, 1, '{0}', '{1}', '{2}')",
				"INSERT [biadmin].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x10, '2018-03-15'); INSERT INTO [biadmin].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES (0x10, 'dbo', 'OrgAddress', 1, 1803);",
				"Queued Deduplication in OrgAddressPhoneNumberSubscriber for [OH_PK:{0}]. Row: Added - {1} -  -  - {2}");
		}

		void OrgAddressContactNumberProcessesChangesForNewRecordsImplementation(string insertIntoCDCQuery, string insertLsnMappingQuery, string expectedLog)
		{
			var factory = new BusinessObjectFactory();

			var orgHeader = SubscriberTestUtilities.CreateTestOrgHeader(factory);

			var orgAddress = orgHeader.Addresses.AddNew();
			orgAddress.OA_OH = orgHeader.PK;
			orgAddress.OA_Address1 = "11 notplaceholder avenue";

			factory.Save();

			using (var auditConnection = AuditTestHelper.GetAuditConnection())
			{
				var testSubscribers = new ActualDataChangesAuditSubscriber[] {
					NewDataChangeSubscriber()
				};

				AuditTestHelper.CleanupAuditTestData(auditConnection, testSubscribers);
				AuditTestHelper.ResetSubscriberControlWaterMark(auditConnection, testSubscribers);

				var patternMatchingPhone = factory.Load<PatternMatchingPhone>(new ZQuery(PatternMatchingPhoneSchema.PMP_ParentId, orgAddress.PK)).ToList();
				AssertEquals(0, patternMatchingPhone.Count);

				var orgAddressContactNumber = "92041334";
				var insertOrgAddressCDC = string.Format(CultureInfo.InvariantCulture, insertIntoCDCQuery, orgAddress.PK, orgHeader.PK, orgAddressContactNumber);
				auditConnection.ExecuteNonQuery(insertOrgAddressCDC);

				auditConnection.ExecuteNonQuery(insertLsnMappingQuery);

				AuditTestHelper.RunNotificationCycleAndAssert(
					auditConnection,
					testSubscribers,
					"0x10", "2018-03-15 00:00:00.000",
					expectedLog: new BetterLogForTest(LogType.Information, string.Format(CultureInfo.InvariantCulture, expectedLog, orgAddress.Header.PK, orgAddress.PK, orgAddressContactNumber))
				);

				patternMatchingPhone = factory.Load<PatternMatchingPhone>(new ZQuery(PatternMatchingPhoneSchema.PMP_ParentId, orgAddress.PK)).ToList();
				AssertEquals(1, patternMatchingPhone.Count);

				var firstPatternMatchingPhone = patternMatchingPhone.Single();

				AssertEquals(TextStandardizerHelper.ComputeStringHashFast(orgAddressContactNumber), firstPatternMatchingPhone.PMP_HashedValue);
				AssertEquals(orgAddress.Header.PK, firstPatternMatchingPhone.PMP_OH);
				AssertEquals(OrgAddressSchema.Constants.Prefix, firstPatternMatchingPhone.PMP_ParentTableCode);
				AssertEquals(orgAddress.PK, firstPatternMatchingPhone.PMP_ParentId);
				AssertEquals(orgHeader.CountryCode, firstPatternMatchingPhone.PMP_RN_NKCountryCode);
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestOrgAddressContactNumberNoPatternMatchingForPlaceholderRecords()
		{
			var insertIntoCDCQuery = @"
				INSERT [dbo].[OrgAddress] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], OA_PK, OA_OH, OA_Phone) VALUES 
					(0x10, 0x01, 2, 0x0, 1803, 1, '{0}', '{1}', '{2}')
				INSERT [biadmin].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x10, '2018-03-15');
				INSERT INTO [biadmin].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES (0x10, 'dbo', 'OrgAddress', 1, 1803);";

			var expectedLog = "Queued Deduplication in OrgAddressPhoneNumberSubscriber for [OH_PK:{0}]. Row: Added - {1} - {2} -  - ";

			var factory = new BusinessObjectFactory();

			var orgHeader = SubscriberTestUtilities.CreateTestOrgHeader(factory);

			var orgAddress = orgHeader.Addresses.AddNew();
			orgAddress.OA_OH = orgHeader.PK;
			orgAddress.OA_Address1 = "test";

			factory.Save();

			using (var auditConnection = AuditTestHelper.GetAuditConnection())
			{
				var testSubscribers = new ActualDataChangesAuditSubscriber[] {
					NewDataChangeSubscriber()
				};

				AuditTestHelper.CleanupAuditTestData(auditConnection, testSubscribers);
				AuditTestHelper.ResetSubscriberControlWaterMark(auditConnection, testSubscribers);

				var patternMatchingPhone = factory.Load<PatternMatchingPhone>(new ZQuery(PatternMatchingPhoneSchema.PMP_ParentId, orgAddress.PK)).ToList();
				AssertEquals(0, patternMatchingPhone.Count);

				var orgAddressContactNumber = "0000";
				var insertOrgAddressCDC = string.Format(CultureInfo.InvariantCulture, insertIntoCDCQuery, orgAddress.PK, orgHeader.PK, orgAddressContactNumber);
				auditConnection.ExecuteNonQuery(insertOrgAddressCDC);

				AuditTestHelper.RunNotificationCycleAndAssert(
					auditConnection,
					testSubscribers,
					"0x10", "2018-03-15 00:00:00.000",
					expectedLog: new BetterLogForTest(LogType.Information, string.Format(CultureInfo.InvariantCulture, expectedLog, orgAddress.Header.PK, orgAddress.PK, orgAddressContactNumber))
				);

				patternMatchingPhone = factory.Load<PatternMatchingPhone>(new ZQuery(PatternMatchingPhoneSchema.PMP_ParentId, orgAddress.PK)).ToList();
				AssertEquals(0, patternMatchingPhone.Count);
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestOrgAddressContactNumberProcessesChangesForExistingRecords()
		{
			OrgAddressContactNumberProcessesChangesForExistingRecords(
				"INSERT [dbo].[OrgAddress] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], OA_PK, OA_OH, OA_Phone) VALUES (0x10, 0x01, 3, 0x0, 1803, 1, '{0}', '{1}', '{2}')",
				"INSERT [dbo].[OrgAddress] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], OA_PK, OA_OH, OA_Phone) VALUES (0x10, 0x01, 4, 0x0, 1803, 1, '{0}', '{1}', '{2}')",
				"INSERT [biadmin].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x10, '2018-03-15'); INSERT INTO [biadmin].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES (0x10, 'dbo', 'OrgAddress', 2, 1803);",
				"Queued Deduplication in OrgAddressPhoneNumberSubscriber for [OH_PK:{0}]. Row: Modified - {1} - {2} -  - ");
			OrgAddressContactNumberProcessesChangesForExistingRecords(
				"INSERT [dbo].[OrgAddress] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], OA_PK, OA_OH, OA_Fax) VALUES (0x10, 0x01, 3, 0x0, 1803, 1, '{0}', '{1}', '{2}')",
				"INSERT [dbo].[OrgAddress] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], OA_PK, OA_OH, OA_Fax) VALUES (0x10, 0x01, 4, 0x0, 1803, 1, '{0}', '{1}', '{2}')",
				"INSERT [biadmin].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x10, '2018-03-15'); INSERT INTO [biadmin].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES (0x10, 'dbo', 'OrgAddress', 2, 1803);",
				"Queued Deduplication in OrgAddressPhoneNumberSubscriber for [OH_PK:{0}]. Row: Modified - {1} -  - {2} - ");
			OrgAddressContactNumberProcessesChangesForExistingRecords(
				"INSERT [dbo].[OrgAddress] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], OA_PK, OA_OH, OA_Mobile) VALUES (0x10, 0x01, 3, 0x0, 1803, 1, '{0}', '{1}', '{2}')",
				"INSERT [dbo].[OrgAddress] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], OA_PK, OA_OH, OA_Mobile) VALUES (0x10, 0x01, 4, 0x0, 1803, 1, '{0}', '{1}', '{2}')",
				"INSERT [biadmin].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x10, '2018-03-15'); INSERT INTO [biadmin].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES (0x10, 'dbo', 'OrgAddress', 2, 1803);",
				"Queued Deduplication in OrgAddressPhoneNumberSubscriber for [OH_PK:{0}]. Row: Modified - {1} -  -  - {2}");
		}

		void OrgAddressContactNumberProcessesChangesForExistingRecords(string insertIntoCDCQuery, string insertIntoCDCUpdateQuery, string insertLsnMappingQuery, string expectedLog)
		{
			var factory = new BusinessObjectFactory();
			var orgHeader = SubscriberTestUtilities.CreateTestOrgHeader(factory);

			var orgAddress = orgHeader.Addresses.AddNew();
			orgAddress.OA_OH = orgHeader.PK;
			orgAddress.OA_Email = "82330193";
			orgAddress.OA_Address1 = "1 little whinging avenue";

			var originalHashedValue = TextStandardizerHelper.ComputeStringHashFast("32541356");

			SubscriberTestUtilities.CreatePatternMatchingBusinessObject<PatternMatchingPhone>(factory, orgHeader.PK, originalHashedValue, OrgAddressSchema.Constants.Prefix, orgAddress.PK, orgAddress.Header.CountryCode);

			factory.Save();

			using (var auditConnection = AuditTestHelper.GetAuditConnection())
			{
				var testSubscribers = new ActualDataChangesAuditSubscriber[] {
					NewDataChangeSubscriber()
				};

				AuditTestHelper.CleanupAuditTestData(auditConnection, testSubscribers);
				AuditTestHelper.ResetSubscriberControlWaterMark(auditConnection, testSubscribers);

				var insertOrgAddressOriginalCDC = string.Format(CultureInfo.InvariantCulture, insertIntoCDCQuery, orgAddress.PK, orgHeader.PK, "32541356");
				auditConnection.ExecuteNonQuery(insertOrgAddressOriginalCDC);

				var updatedOrgAddressContactNumber = "87654321";
				var insertOrgAddressCurrentCDC = string.Format(CultureInfo.InvariantCulture, insertIntoCDCUpdateQuery, orgAddress.PK, orgHeader.PK, updatedOrgAddressContactNumber);
				auditConnection.ExecuteNonQuery(insertOrgAddressCurrentCDC);

				auditConnection.ExecuteNonQuery(insertLsnMappingQuery);

				AuditTestHelper.RunNotificationCycleAndAssert(
					auditConnection,
					testSubscribers,
					"0x10", "2018-06-08 00:00:00.000",
					expectedLog: new BetterLogForTest(LogType.Information, string.Format(CultureInfo.InvariantCulture, expectedLog, orgAddress.Header.PK, orgAddress.PK, updatedOrgAddressContactNumber))
				);

				var hashedValue = TextStandardizerHelper.ComputeStringHashFast(updatedOrgAddressContactNumber);

				var patternMatchingPhone = factory.Load<PatternMatchingPhone>(new ZQuery(PatternMatchingPhoneSchema.PMP_ParentId, orgAddress.PK)).ToList();
				AssertEquals(1, patternMatchingPhone.Count);

				var firstPatternMatchingPhone = patternMatchingPhone.Single();
				AssertEquals(hashedValue, firstPatternMatchingPhone.PMP_HashedValue);
				AssertEquals(orgAddress.Header.PK, firstPatternMatchingPhone.PMP_OH);
				AssertEquals(OrgAddressSchema.Constants.Prefix, firstPatternMatchingPhone.PMP_ParentTableCode);
				AssertEquals(orgAddress.PK, firstPatternMatchingPhone.PMP_ParentId);
				AssertEquals(orgHeader.CountryCode, firstPatternMatchingPhone.PMP_RN_NKCountryCode);
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestOrgAddressContactNumberDoesNotProcessChangesForNonSubscribedColumns()
		{
			var factory = new BusinessObjectFactory();

			var orgHeader = SubscriberTestUtilities.CreateTestOrgHeader(factory);

			var orgAddress = orgHeader.Addresses.AddNew();
			orgAddress.OA_OH = orgHeader.PK;
			orgAddress.OA_Address1 = "1 gin alley";

			SubscriberTestUtilities.CreatePatternMatchingBusinessObject<PatternMatchingPhone>(factory, orgHeader.PK, 0, OrgAddressSchema.Constants.Prefix, orgAddress.PK, orgAddress.Header.CountryCode);

			factory.Save();

			using (var auditConnection = AuditTestHelper.GetAuditConnection())
			{
				var testSubscribers = new ActualDataChangesAuditSubscriber[] {
					NewDataChangeSubscriber()
				};
				AuditTestHelper.ResetSubscriberControlWaterMark(auditConnection, testSubscribers);

				var insertOrgAddressOriginalCDC = string.Format(CultureInfo.InvariantCulture, @"INSERT [dbo].[OrgAddress] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], OA_PK, OA_OH) VALUES 
					(0x10, 0x01, 3, 0x0, 0, 1, '{0}', '{1}');", orgAddress.PK, orgHeader.PK);
				auditConnection.ExecuteNonQuery(insertOrgAddressOriginalCDC);

				var orgAddressLanguage = "ABC";
				var insertOrgAddressCurrentCDC = string.Format(CultureInfo.InvariantCulture, @"INSERT [dbo].[OrgAddress] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], OA_PK, OA_OH, OA_Language) VALUES 
					(0x10, 0x01, 4, 0x0, 0, 1, '{0}', '{1}', '{2}');", orgAddress.PK, orgHeader.PK, orgAddressLanguage);
				auditConnection.ExecuteNonQuery(insertOrgAddressCurrentCDC);

				AuditTestHelper.RunNotificationCycleAndAssert(
					auditConnection,
					testSubscribers,
					"0x10", "2018-03-15 00:00:00.000",
					expectedLog: null
				);

				var patternMatchingPhone = factory.Load<PatternMatchingPhone>(new ZQuery(PatternMatchingPhoneSchema.PMP_ParentId, orgAddress.PK)).ToList();
				AssertEquals(1, patternMatchingPhone.Count);

				var firstPatternMatchingPhone = patternMatchingPhone.Single();
				AssertEquals(0, firstPatternMatchingPhone.PMP_HashedValue);
			}
		}

		public override void TestCustomFilter()
		{
			var subscriber = new OrgAddressPhoneNumberSubscriber();

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
				row[OrgAddressSchema.OA_Phone.Name] = DBNull.Value;
				row[OrgAddressSchema.OA_Fax.Name] = DBNull.Value;
				row[OrgAddressSchema.OA_Mobile.Name] = DBNull.Value;
			}
			else
			{
				row[OrgAddressSchema.OA_Phone.Name] = "Phone";
				row[OrgAddressSchema.OA_Fax.Name] = "Phone";
				row[OrgAddressSchema.OA_Mobile.Name] = "Phone";
			}
			table.Rows.Add(row);
			return row;
		}

		protected override DataTable GetTestDataTable()
		{
			var changeTable = new DataTable();
			var tableColumns = new SchemaColumn[] { OrgAddressSchema.OA_Phone, OrgAddressSchema.OA_Fax, OrgAddressSchema.OA_Mobile };
			foreach (var column in tableColumns)
			{
				changeTable.Columns.Add(column.Name, column.DotNetType);
			}
			return changeTable;
		}
	}
}
