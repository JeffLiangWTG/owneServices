
using System;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Tools.DuplicateDetector;
using CargoWise.Types;
using Enterprise.AuditDataServices.MDM.Subscribers;
using Enterprise.AuditDataServices.Subscription.Common;
using Enterprise.AuditDataServices.Subscription.Testing;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.AuditDataServices.MDM.Testing
{
	[TestedType(typeof(OrgContactPhoneNumberSubscriber))]
	class OrgContactPhoneNumberSubscriberTest : ActualDataChangesAuditSubscriberTest
	{
		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestOrgContactNumberProcessesChangesForNewRecords_ContactPhone()
		{
			OrgContactNumberProcessesChangesForNewRecordsImplementation(
				"INSERT [dbo].[OrgContact] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], OC_PK, OC_OH, OC_Phone) VALUES (0x10, 0x01, 2, 0x0, 1803, 1, '{0}', '{1}', '{2}')",
				"INSERT [biadmin].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x10, '2018-03-15'); INSERT INTO [biadmin].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES (0x10, 'dbo', 'OrgContact', 1, 1803);",
				"Queued Deduplication in OrgContactPhoneNumberSubscriber for [OH_PK:{0}], [PER_PK:{1}]. Row: Added - {2} - {3} -  -  -  - ");
			OrgContactNumberProcessesChangesForNewRecordsImplementation(
				"INSERT [dbo].[OrgContact] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], OC_PK, OC_OH, OC_HomePhone) VALUES (0x10, 0x01, 2, 0x0, 1803, 1, '{0}', '{1}', '{2}')",
				"INSERT [biadmin].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x10, '2018-03-15'); INSERT INTO [biadmin].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES (0x10, 'dbo', 'OrgContact', 1, 1803);",
				"Queued Deduplication in OrgContactPhoneNumberSubscriber for [OH_PK:{0}], [PER_PK:{1}]. Row: Added - {2} -  - {3} -  -  - ");
			OrgContactNumberProcessesChangesForNewRecordsImplementation(
				"INSERT [dbo].[OrgContact] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], OC_PK, OC_OH, OC_Mobile) VALUES (0x10, 0x01, 2, 0x0, 1803, 1, '{0}', '{1}', '{2}')",
				"INSERT [biadmin].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x10, '2018-03-15'); INSERT INTO [biadmin].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES (0x10, 'dbo', 'OrgContact', 1, 1803);",
				"Queued Deduplication in OrgContactPhoneNumberSubscriber for [OH_PK:{0}], [PER_PK:{1}]. Row: Added - {2} -  -  - {3} -  - ");
			OrgContactNumberProcessesChangesForNewRecordsImplementation(
				"INSERT [dbo].[OrgContact] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], OC_PK, OC_OH, OC_OtherPhone) VALUES (0x10, 0x01, 2, 0x0, 1803, 1, '{0}', '{1}', '{2}')",
				"INSERT [biadmin].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x10, '2018-03-15'); INSERT INTO [biadmin].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES (0x10, 'dbo', 'OrgContact', 1, 1803);",
				"Queued Deduplication in OrgContactPhoneNumberSubscriber for [OH_PK:{0}], [PER_PK:{1}]. Row: Added - {2} -  -  -  - {3} - ");
			OrgContactNumberProcessesChangesForNewRecordsImplementation(
				"INSERT [dbo].[OrgContact] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], OC_PK, OC_OH, OC_Fax) VALUES (0x10, 0x01, 2, 0x0, 1803, 1, '{0}', '{1}', '{2}')",
				"INSERT [biadmin].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x10, '2018-03-15'); INSERT INTO [biadmin].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES (0x10, 'dbo', 'OrgContact', 1, 1803);",
				"Queued Deduplication in OrgContactPhoneNumberSubscriber for [OH_PK:{0}], [PER_PK:{1}]. Row: Added - {2} -  -  -  -  - {3}");
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestOrgContactPhoneNumberNoPatternMatchingForPlaceholderRecords()
		{
			var insertIntoCDCQuery = @"
				INSERT [dbo].[OrgContact] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], OC_PK, OC_OH, OC_Phone) VALUES 
					(0x10, 0x01, 2, 0x0, 1803, 1, '{0}', '{1}', '{2}')
				INSERT [biadmin].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x10, '2018-03-15'); INSERT INTO [biadmin].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES (0x10, 'dbo', 'OrgContact', 1, 1803);";

			var expectedLog = "Queued Deduplication in OrgContactPhoneNumberSubscriber for [OH_PK:{0}], [PER_PK:{1}]. Row: Added - {2} - {3} -  -  -  - ";

			var factory = new BusinessObjectFactory();

			var orgHeader = SubscriberTestUtilities.CreateTestOrgHeader(factory);

			var orgContact = orgHeader.Contacts.AddNew();
			orgContact.OC_ContactName = "test";

			factory.Save();

			using (var auditConnection = AuditTestHelper.GetAuditConnection())
			{
				var testSubscribers = new ActualDataChangesAuditSubscriber[] {
					NewDataChangeSubscriber()
				};

				AuditTestHelper.CleanupAuditTestData(auditConnection, testSubscribers);
				AuditTestHelper.ResetSubscriberControlWaterMark(auditConnection, testSubscribers);

				var patternMatchingPhone = factory.Load<PatternMatchingPhone>(new ZQuery(PatternMatchingPhoneSchema.PMP_ParentId, orgContact.PK)).ToList();
				AssertEquals(0, patternMatchingPhone.Count);

				var orgContactNumber = "0000";
				var insertOrgAddressCDC = string.Format(CultureInfo.InvariantCulture, insertIntoCDCQuery, orgContact.PK, orgHeader.PK, orgContactNumber);
				auditConnection.ExecuteNonQuery(insertOrgAddressCDC);

				AuditTestHelper.RunNotificationCycleAndAssert(
					auditConnection,
					testSubscribers,
					"0x10", "2018-03-08 00:00:00.000",
					expectedLog: new BetterLogForTest(LogType.Information, string.Format(CultureInfo.InvariantCulture, expectedLog, orgContact.Header.PK, orgContact.Person.PK, orgContact.PK, orgContactNumber))
				);

				patternMatchingPhone = factory.Load<PatternMatchingPhone>(new ZQuery(PatternMatchingPhoneSchema.PMP_ParentId, orgContact.PK)).ToList();
				AssertEquals(0, patternMatchingPhone.Count);
			}
		}

		public void OrgContactNumberProcessesChangesForNewRecordsImplementation(string insertIntoCDCQuery, string insertLsnMappingQuery, string expectedLog)
		{
			var factory = new BusinessObjectFactory();

			var orgHeader = SubscriberTestUtilities.CreateTestOrgHeader(factory);

			var orgContact = orgHeader.Contacts.AddNew();
			orgContact.OC_OH = orgHeader.PK;

			factory.Save();

			using (var auditConnection = AuditTestHelper.GetAuditConnection())
			{
				var testSubscribers = new ActualDataChangesAuditSubscriber[] {
					NewDataChangeSubscriber()
				};

				AuditTestHelper.CleanupAuditTestData(auditConnection, testSubscribers);
				AuditTestHelper.ResetSubscriberControlWaterMark(auditConnection, testSubscribers);

				var patternMatchingPhone = factory.Load<PatternMatchingPhone>(new ZQuery(PatternMatchingPhoneSchema.PMP_ParentId, orgContact.PK)).ToList();
				AssertEquals(0, patternMatchingPhone.Count);

				var orgContactNumber = "92840102";
				var insertOrgContactCDC = string.Format(CultureInfo.InvariantCulture, insertIntoCDCQuery, orgContact.PK, orgHeader.PK, orgContactNumber);
				auditConnection.ExecuteNonQuery(insertOrgContactCDC);

				auditConnection.ExecuteNonQuery(insertLsnMappingQuery);

				AuditTestHelper.RunNotificationCycleAndAssert(
					auditConnection,
					testSubscribers,
					"0x10", "2018-03-08 00:00:00.000",
					expectedLog: new BetterLogForTest(LogType.Information, string.Format(CultureInfo.InvariantCulture, expectedLog, orgContact.Header.PK, orgContact.Person.PK, orgContact.PK, orgContactNumber))
				);

				var hashedValue = TextStandardizerHelper.ComputeStringHashFast(orgContactNumber);

				patternMatchingPhone = factory.Load<PatternMatchingPhone>(new ZQuery(PatternMatchingPhoneSchema.PMP_ParentId, orgContact.PK)).ToList();
				AssertEquals(1, patternMatchingPhone.Count);

				var firstPatternMatchingPhone = patternMatchingPhone.Single();
				AssertEquals(hashedValue, firstPatternMatchingPhone.PMP_HashedValue);
				AssertEquals(orgContact.Header.PK, firstPatternMatchingPhone.PMP_OH);
				AssertEquals(orgContact.Person.PK, firstPatternMatchingPhone.PMP_PER);
				AssertEquals(OrgContactSchema.Constants.Prefix, firstPatternMatchingPhone.PMP_ParentTableCode);
				AssertEquals(orgContact.PK, firstPatternMatchingPhone.PMP_ParentId);
				AssertEquals(orgHeader.CountryCode, firstPatternMatchingPhone.PMP_RN_NKCountryCode);
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestOrgContactNumberProcessesChangesForExistingRecords()
		{
			OrgContactNumberProcessesChangesForExistingRecordsImplementation(
				"INSERT [dbo].[OrgContact] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], OC_PK, OC_OH, OC_Phone) VALUES (0x10, 0x01, 3, 0x0, 1803, 1, '{0}', '{1}', '{2}')",
				"INSERT [dbo].[OrgContact] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], OC_PK, OC_OH, OC_Phone) VALUES (0x10, 0x01, 4, 0x0, 1803, 1, '{0}', '{1}', '{2}')",
				"INSERT [biadmin].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x10, '2018-03-15'); INSERT INTO [biadmin].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES (0x10, 'dbo', 'OrgContact', 2, 1803);",
				"Queued Deduplication in OrgContactPhoneNumberSubscriber for [OH_PK:{0}], [PER_PK:{1}]. Row: Modified - {2} - {3} -  -  -  - ");
			OrgContactNumberProcessesChangesForExistingRecordsImplementation(
				"INSERT [dbo].[OrgContact] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], OC_PK, OC_OH, OC_HomePhone) VALUES (0x10, 0x01, 3, 0x0, 1803, 1, '{0}', '{1}', '{2}')",
				"INSERT [dbo].[OrgContact] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], OC_PK, OC_OH, OC_HomePhone) VALUES (0x10, 0x01, 4, 0x0, 1803, 1, '{0}', '{1}', '{2}')",
				"INSERT [biadmin].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x10, '2018-03-15'); INSERT INTO [biadmin].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES (0x10, 'dbo', 'OrgContact', 2, 1803);",
				"Queued Deduplication in OrgContactPhoneNumberSubscriber for [OH_PK:{0}], [PER_PK:{1}]. Row: Modified - {2} -  - {3} -  -  - ");
			OrgContactNumberProcessesChangesForExistingRecordsImplementation(
				"INSERT [dbo].[OrgContact] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], OC_PK, OC_OH, OC_Mobile) VALUES (0x10, 0x01, 3, 0x0, 1803, 1, '{0}', '{1}', '{2}')",
				"INSERT [dbo].[OrgContact] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], OC_PK, OC_OH, OC_Mobile) VALUES (0x10, 0x01, 4, 0x0, 1803, 1, '{0}', '{1}', '{2}')",
				"INSERT [biadmin].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x10, '2018-03-15'); INSERT INTO [biadmin].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES (0x10, 'dbo', 'OrgContact', 2, 1803);",
				"Queued Deduplication in OrgContactPhoneNumberSubscriber for [OH_PK:{0}], [PER_PK:{1}]. Row: Modified - {2} -  -  - {3} -  - ");
			OrgContactNumberProcessesChangesForExistingRecordsImplementation(
				"INSERT [dbo].[OrgContact] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], OC_PK, OC_OH, OC_OtherPhone) VALUES (0x10, 0x01, 3, 0x0, 1803, 1, '{0}', '{1}', '{2}')",
				"INSERT [dbo].[OrgContact] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], OC_PK, OC_OH, OC_OtherPhone) VALUES (0x10, 0x01, 4, 0x0, 1803, 1, '{0}', '{1}', '{2}')",
				"INSERT [biadmin].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x10, '2018-03-15'); INSERT INTO [biadmin].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES (0x10, 'dbo', 'OrgContact', 2, 1803);",
				"Queued Deduplication in OrgContactPhoneNumberSubscriber for [OH_PK:{0}], [PER_PK:{1}]. Row: Modified - {2} -  -  -  - {3} - ");
			OrgContactNumberProcessesChangesForExistingRecordsImplementation(
				"INSERT [dbo].[OrgContact] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], OC_PK, OC_OH, OC_Fax) VALUES (0x10, 0x01, 3, 0x0, 1803, 1, '{0}', '{1}', '{2}')",
				"INSERT [dbo].[OrgContact] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], OC_PK, OC_OH, OC_Fax) VALUES (0x10, 0x01, 4, 0x0, 1803, 1, '{0}', '{1}', '{2}')",
				"INSERT [biadmin].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x10, '2018-03-15'); INSERT INTO [biadmin].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES (0x10, 'dbo', 'OrgContact', 2, 1803);",
				"Queued Deduplication in OrgContactPhoneNumberSubscriber for [OH_PK:{0}], [PER_PK:{1}]. Row: Modified - {2} -  -  -  -  - {3}");
		}

		public void OrgContactNumberProcessesChangesForExistingRecordsImplementation(string insertIntoCDCQuery, string insertIntoCDCUpdateQuery, string insertLsnMappingQuery, string expectedLog)
		{
			var factory = new BusinessObjectFactory();
			var orgHeader = SubscriberTestUtilities.CreateTestOrgHeader(factory);
			var glbPerson = SubscriberTestUtilities.CreateTestGlbPerson(factory);

			var orgContact = orgHeader.Contacts.AddNew();
			orgContact.OC_OH = orgHeader.PK;
			orgContact.OC_PER = glbPerson.PK;

			var originalOrgContactNumber = "92840102";
			var originalHashedValue = TextStandardizerHelper.ComputeStringHashFast(originalOrgContactNumber);

			SubscriberTestUtilities.CreatePatternMatchingBusinessObjectForOrgAndPerson<PatternMatchingPhone>(factory, orgHeader.PK, glbPerson.PK, originalHashedValue, OrgContactSchema.Constants.Prefix, orgContact.PK, orgContact.Header.CountryCode);

			factory.Save();

			using (var auditConnection = AuditTestHelper.GetAuditConnection())
			{
				var testSubscribers = new ActualDataChangesAuditSubscriber[] {
					NewDataChangeSubscriber()
				};

				AuditTestHelper.CleanupAuditTestData(auditConnection, testSubscribers);
				AuditTestHelper.ResetSubscriberControlWaterMark(auditConnection, testSubscribers);

				var insertOrgContactOriginalCDC = string.Format(CultureInfo.InvariantCulture, insertIntoCDCQuery, orgContact.PK, orgHeader.PK, originalOrgContactNumber);
				auditConnection.ExecuteNonQuery(insertOrgContactOriginalCDC);

				var updatedOrgContactNumber = "87291311";
				var insertOrgContactCurrentCDC = string.Format(CultureInfo.InvariantCulture, insertIntoCDCUpdateQuery, orgContact.PK, orgHeader.PK, updatedOrgContactNumber);
				auditConnection.ExecuteNonQuery(insertOrgContactCurrentCDC);

				auditConnection.ExecuteNonQuery(insertLsnMappingQuery);

				AuditTestHelper.RunNotificationCycleAndAssert(
					auditConnection,
					testSubscribers,
					"0x10", "2018-03-15 00:00:00.000",
					expectedLog: new BetterLogForTest(LogType.Information, string.Format(CultureInfo.InvariantCulture, expectedLog, orgContact.Header.PK, orgContact.Person.PK, orgContact.PK, updatedOrgContactNumber))
				);

				var hashedValue = TextStandardizerHelper.ComputeStringHashFast(updatedOrgContactNumber);

				var patternMatchingPhone = factory.Load<PatternMatchingPhone>(new ZQuery(PatternMatchingPhoneSchema.PMP_OH, orgHeader.PK)).ToList();
				AssertEquals(1, patternMatchingPhone.Count);

				var firstPatternMatchingPhone = patternMatchingPhone.Single();
				AssertEquals(hashedValue, firstPatternMatchingPhone.PMP_HashedValue);
				AssertEquals(orgContact.Header.PK, firstPatternMatchingPhone.PMP_OH);
				AssertEquals(orgContact.Person.PK, firstPatternMatchingPhone.PMP_PER);
				AssertEquals(OrgContactSchema.Constants.Prefix, firstPatternMatchingPhone.PMP_ParentTableCode);
				AssertEquals(orgContact.PK, firstPatternMatchingPhone.PMP_ParentId);
				AssertEquals(orgHeader.CountryCode, firstPatternMatchingPhone.PMP_RN_NKCountryCode);
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestOrgContactNumberDoesNotProcessChangesForNonSubscribedColumns()
		{
			var factory = new BusinessObjectFactory();

			var orgHeader = SubscriberTestUtilities.CreateTestOrgHeader(factory);
			var glbPerson = SubscriberTestUtilities.CreateTestGlbPerson(factory);

			var orgContact = orgHeader.Contacts.AddNew();
			orgContact.OC_OH = orgHeader.PK;
			orgContact.OC_PER = glbPerson.PK;

			SubscriberTestUtilities.CreatePatternMatchingBusinessObjectForOrgAndPerson<PatternMatchingPhone>(factory, orgHeader.PK, glbPerson.PK, 0, OrgContactSchema.Constants.Prefix, orgContact.PK, orgContact.Header.CountryCode);
			SubscriberTestUtilities.CreatePatternMatchingBusinessObjectForOrgAndPerson<PatternMatchingEmail>(factory, orgHeader.PK, glbPerson.PK, 0, OrgContactSchema.Constants.Prefix, orgContact.PK, orgContact.Header.CountryCode);

			factory.Save();

			using (var auditConnection = AuditTestHelper.GetAuditConnection())
			{
				var testSubscribers = new ActualDataChangesAuditSubscriber[] {
					NewDataChangeSubscriber()
				};

				AuditTestHelper.CleanupAuditTestData(auditConnection, testSubscribers);
				AuditTestHelper.ResetSubscriberControlWaterMark(auditConnection, testSubscribers);

				var insertOrgContactOriginalCDC = string.Format(CultureInfo.InvariantCulture, @"INSERT [dbo].[OrgContact] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], OC_PK, OC_OH) VALUES 
					(0x10, 0x01, 3, 0x0, 0, 1, '{0}', '{1}');", orgContact.PK, orgHeader.PK);
				auditConnection.ExecuteNonQuery(insertOrgContactOriginalCDC);

				var orgContactGender = "M";
				var insertOrgContactCurrentCDC = string.Format(CultureInfo.InvariantCulture, @"INSERT [dbo].[OrgContact] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], OC_PK, OC_OH, OC_Gender) VALUES 
					(0x10, 0x01, 4, 0x0, 0, 1, '{0}', '{1}', '{2}');", orgContact.PK, orgHeader.PK, orgContactGender);
				auditConnection.ExecuteNonQuery(insertOrgContactCurrentCDC);

				AuditTestHelper.RunNotificationCycleAndAssert(
					auditConnection,
					testSubscribers,
					"0x10", "2018-03-08 00:00:00.000",
					expectedLog: null
				);

				var patternMatchingPhone = factory.Load<PatternMatchingPhone>(new ZQuery(PatternMatchingPhoneSchema.PMP_OH, orgHeader.PK)).ToList();
				AssertEquals(1, patternMatchingPhone.Count);

				var firstPatternMatchingPhone = patternMatchingPhone.Single();
				AssertEquals(0, firstPatternMatchingPhone.PMP_HashedValue);
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestUpdatePersonPKColumnWhenOriginalPersonPKColumnIsEmpty()
		{
			var factory = new BusinessObjectFactory();
			var orgHeader = SubscriberTestUtilities.CreateTestOrgHeader(factory);
			var glbPerson = SubscriberTestUtilities.CreateTestGlbPerson(factory);

			var orgContact = orgHeader.Contacts.AddNew();
			orgContact.OC_OH = orgHeader.PK;
			orgContact.OC_PER = glbPerson.PK;

			var originalOrgContactNumber = "92840102";
			var originalHashedValue = TextStandardizerHelper.ComputeStringHashFast(originalOrgContactNumber);

			SubscriberTestUtilities.CreatePatternMatchingBusinessObject<PatternMatchingPhone>(factory, orgHeader.PK, originalHashedValue, OrgContactSchema.Constants.Prefix, orgContact.PK, orgContact.Header.CountryCode);

			factory.Save();

			var patternMatchingPhone = factory.Load<PatternMatchingPhone>(new ZQuery(PatternMatchingPhoneSchema.PMP_OH, orgHeader.PK)).ToList();
			AssertEquals(1, patternMatchingPhone.Count);
			AssertEquals(ZGuid.Empty, patternMatchingPhone.Single().PMP_PER);

			using (var auditConnection = AuditTestHelper.GetAuditConnection())
			{
				var testSubscribers = new ActualDataChangesAuditSubscriber[] {
					NewDataChangeSubscriber()
				};

				AuditTestHelper.CleanupAuditTestData(auditConnection, testSubscribers);
				AuditTestHelper.ResetSubscriberControlWaterMark(auditConnection, testSubscribers);

				var insertOrgContactOriginalCDC = string.Format(CultureInfo.InvariantCulture, "INSERT [dbo].[OrgContact] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], OC_PK, OC_OH, OC_Phone) VALUES (0x10, 0x01, 3, 0x0, 1803, 1, '{0}', '{1}', '{2}')", orgContact.PK, orgHeader.PK, originalOrgContactNumber);
				auditConnection.ExecuteNonQuery(insertOrgContactOriginalCDC);

				var updatedOrgContactNumber = "87291311";
				var insertOrgContactCurrentCDC = string.Format(CultureInfo.InvariantCulture, "INSERT [dbo].[OrgContact] ([__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [__$lsn_period], [__$command_id], OC_PK, OC_OH, OC_Phone) VALUES (0x10, 0x01, 4, 0x0, 1803, 1, '{0}', '{1}', '{2}')", orgContact.PK, orgHeader.PK, updatedOrgContactNumber);
				auditConnection.ExecuteNonQuery(insertOrgContactCurrentCDC);

				auditConnection.ExecuteNonQuery("INSERT [biadmin].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x10, '2018-06-15');");

				auditConnection.ExecuteNonQuery("INSERT INTO [biadmin].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES(0x10, 'dbo', 'OrgContact', 2, 1803);");

				AuditTestHelper.RunNotificationCycleAndAssert(
					auditConnection,
					testSubscribers,
					"0x10", "2018-06-15 00:00:00.000",
					expectedLog: new BetterLogForTest(LogType.Information, string.Format(CultureInfo.InvariantCulture, "Queued Deduplication in OrgContactPhoneNumberSubscriber for [OH_PK:{0}], [PER_PK:{1}]. Row: Modified - {2} - {3} -  -  -  - ", orgContact.Header.PK, orgContact.Person.PK, orgContact.PK, updatedOrgContactNumber))
				);

				var hashedValue = TextStandardizerHelper.ComputeStringHashFast(updatedOrgContactNumber);

				patternMatchingPhone = factory.Load<PatternMatchingPhone>(new ZQuery(PatternMatchingPhoneSchema.PMP_OH, orgHeader.PK)).ToList();
				AssertEquals(1, patternMatchingPhone.Count);

				var firstPatternMatchingPhone = patternMatchingPhone.Single();
				AssertEquals(hashedValue, firstPatternMatchingPhone.PMP_HashedValue);
				AssertEquals(orgContact.Header.PK, firstPatternMatchingPhone.PMP_OH);
				AssertNotEquals(ZGuid.Empty, firstPatternMatchingPhone.PMP_PER);
				AssertEquals(orgContact.Person.PK, firstPatternMatchingPhone.PMP_PER);
				AssertEquals(OrgContactSchema.Constants.Prefix, firstPatternMatchingPhone.PMP_ParentTableCode);
				AssertEquals(orgContact.PK, firstPatternMatchingPhone.PMP_ParentId);
				AssertEquals(orgHeader.CountryCode, firstPatternMatchingPhone.PMP_RN_NKCountryCode);
			}
		}

		public override void TestCustomFilter()
		{
			var subscriber = new OrgContactPhoneNumberSubscriber();

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
				row[OrgContactSchema.OC_Phone.Name] = DBNull.Value;
				row[OrgContactSchema.OC_HomePhone.Name] = DBNull.Value;
				row[OrgContactSchema.OC_Mobile.Name] = DBNull.Value;
				row[OrgContactSchema.OC_OtherPhone.Name] = DBNull.Value;
				row[OrgContactSchema.OC_Fax.Name] = DBNull.Value;
			}
			else
			{
				row[OrgContactSchema.OC_Phone.Name] = "Phone";
				row[OrgContactSchema.OC_HomePhone.Name] = "Phone";
				row[OrgContactSchema.OC_Mobile.Name] = "Phone";
				row[OrgContactSchema.OC_OtherPhone.Name] = "Phone";
				row[OrgContactSchema.OC_Fax.Name] = "Fax";
			}
			table.Rows.Add(row);
			return row;
		}

		protected override DataTable GetTestDataTable()
		{
			var changeTable = new DataTable();
			var tableColumns = new SchemaColumn[] { OrgContactSchema.OC_Phone, OrgContactSchema.OC_HomePhone, OrgContactSchema.OC_Mobile, OrgContactSchema.OC_OtherPhone, OrgContactSchema.OC_Fax };
			foreach (var column in tableColumns)
			{
				changeTable.Columns.Add(column.Name, column.DotNetType);
			}
			return changeTable;
		}
	}
}
