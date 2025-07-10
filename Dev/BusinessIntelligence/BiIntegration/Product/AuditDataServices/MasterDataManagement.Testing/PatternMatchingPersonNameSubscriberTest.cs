using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using Enterprise.AuditDataServices.MDM.Testing;
using Enterprise.AuditDataServices.Subscription.Common;
using Enterprise.AuditDataServices.Subscription.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.AuditDataService.MDM.Testing
{
	abstract class PatternMatchingPersonNameSubscriberTest<T, S> : PatternMatchingBaseSubscriberTest<T, S> where T : BusinessObject where S : ActualDataChangesAuditSubscriber
	{
		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestNoPatternMatchingNameForNameLessThanTwoWords()
		{
			var factory = new BusinessObjectFactory();
			var bizO = CreateParentRecords(factory);
			factory.Save();
			SetNameToSingleWord(bizO);
			factory.Save();

			using (var auditConnection = AuditTestHelper.GetAuditConnection())
			{
				var testSubscribers = new ActualDataChangesAuditSubscriber[] {
					NewDataChangeSubscriber()
				};

				AuditTestHelper.CleanupAuditTestData(auditConnection, testSubscribers);
				AuditTestHelper.ResetSubscriberControlWaterMark(auditConnection, testSubscribers);

				var patternMatchingName = factory.Load<PatternMatchingName>(new ZQuery(PatternMatchingNameSchema.PMN_ParentId, bizO.PK)).ToList();
				AssertEquals(0, patternMatchingName.Count);

				auditConnection.ExecuteNonQuery(GetNewRecordCDCInsertQuery(bizO));

				auditConnection.ExecuteNonQuery(GetLsnMappingInsertQuery());

				auditConnection.ExecuteNonQuery(GetCdcMappingInsertQuery());

				AuditTestHelper.RunNotificationCycleAndAssert(
					auditConnection,
					testSubscribers,
					"0x10", "2018-06-08 00:00:00.000",
					expectedLog: GetLogForNewRecords(bizO)
				);

				patternMatchingName = factory.Load<PatternMatchingName>(new ZQuery(PatternMatchingNameSchema.PMN_ParentId, bizO.PK)).ToList();
				AssertEquals(0, patternMatchingName.Count);
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestDeletePatternMatchingNameForNameLessThanTwoWords()
		{
			var factory = new BusinessObjectFactory();
			var bizO = CreateParentRecords(factory);
			factory.Save();
			SetNameToMultiWords(bizO);
			CreatePatternMatchingRecords(factory, bizO);
			factory.Save();

			using (var auditConnection = AuditTestHelper.GetAuditConnection())
			{
				var testSubscribers = new ActualDataChangesAuditSubscriber[] {
					NewDataChangeSubscriber()
				};

				AuditTestHelper.CleanupAuditTestData(auditConnection, testSubscribers);
				AuditTestHelper.ResetSubscriberControlWaterMark(auditConnection, testSubscribers);

				var patternMatchingName = factory.Load<PatternMatchingName>(new ZQuery(PatternMatchingNameSchema.PMN_ParentId, bizO.PK)).ToList();
				AssertEquals(1, patternMatchingName.Count);

				var insertOriginalQuery = GetOriginalRecordCDCInsertQuery(bizO);
				auditConnection.ExecuteNonQuery(insertOriginalQuery);

				var updateNameToSingleWordQuery = GetUpdateRecordNameToSingleWordCDCInsertQuery(bizO);
				auditConnection.ExecuteNonQuery(updateNameToSingleWordQuery);

				auditConnection.ExecuteNonQuery(GetLsnMappingInsertQuery());

				auditConnection.ExecuteNonQuery(GetCdcMappingInsertQuery());

				AuditTestHelper.RunNotificationCycleAndAssert(
					auditConnection,
					testSubscribers,
					"0x10", "2018-06-08 00:00:00.000",
					expectedLog: GetLogForUpdateNameToSingleWordRecords(bizO)
				);

				patternMatchingName = factory.Load<PatternMatchingName>(new ZQuery(PatternMatchingNameSchema.PMN_ParentId, bizO.PK)).ToList();
				AssertEquals(0, patternMatchingName.Count);
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestSubscriberDoesNotSaveDummyContactToSuppressDocsRecords()
		{
			var factory = new BusinessObjectFactory();
			var dummyContact = CreateDummyContact(factory);

			using var auditConnection = AuditTestHelper.GetAuditConnection();
			var testSubscribers = new ActualDataChangesAuditSubscriber[]
			{
				NewDataChangeSubscriber()
			};

			AuditTestHelper.CleanupAuditTestData(auditConnection, testSubscribers);
			AuditTestHelper.ResetSubscriberControlWaterMark(auditConnection, testSubscribers);

			var insertIntoCDCText = GetNewRecordCDCInsertQuery(dummyContact);
			auditConnection.ExecuteNonQuery(insertIntoCDCText);

			var insertLsnMappingText = GetLsnMappingInsertQuery();
			auditConnection.ExecuteNonQuery(insertLsnMappingText);

			var insertCdcMappingText = GetCdcMappingInsertQuery();
			auditConnection.ExecuteNonQuery(insertCdcMappingText);

			AuditTestHelper.RunNotificationCycleAndAssert(
				auditConnection,
				testSubscribers,
				"0x10", "2018-06-08 00:00:00.000",
				expectedLog: null
			);

			var patternMatchingName = factory.Load<PatternMatchingName>(new ZQuery(PatternMatchingNameSchema.PMN_ParentId, dummyContact.PK)).ToList();
			AssertEquals(0, patternMatchingName.Count);
		}

		protected abstract BetterLogForTest GetLogForUpdateNameToSingleWordRecords(T bizO);

		protected abstract string GetUpdateRecordNameToSingleWordCDCInsertQuery(T bizO);

		protected abstract void SetNameToSingleWord(T bizO);

		protected abstract void SetNameToMultiWords(T bizO);

		protected abstract T CreateDummyContact(BusinessObjectFactory factory);
	}

	abstract class GlbStaffNameSubscriberTest<TSubscriber> : GlbStaffSubscriberTest<TSubscriber>
		where TSubscriber : ActualDataChangesAuditSubscriber
	{
		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestNoPatternMatchingNameForNameLessThanTwoWords()
		{
			var factory = new BusinessObjectFactory();
			var bizO = CreateParentRecords(factory);
			factory.Save();
			SetNameToSingleWord(bizO);
			factory.Save();

			using (var auditConnection = AuditTestHelper.GetAuditConnection())
			{
				var testSubscribers = new ActualDataChangesAuditSubscriber[]
				{
					NewDataChangeSubscriber()
				};

				AuditTestHelper.CleanupAuditTestData(auditConnection, testSubscribers);
				AuditTestHelper.ResetSubscriberControlWaterMark(auditConnection, testSubscribers);

				var patternMatchingName =
					factory.Load<PatternMatchingName>(new ZQuery(PatternMatchingNameSchema.PMN_ParentId, bizO.PK)).ToList();
				AssertEquals(0, patternMatchingName.Count);

				auditConnection.ExecuteNonQuery(GetNewRecordCDCInsertQuery(bizO));

				auditConnection.ExecuteNonQuery(GetLsnMappingInsertQuery());

				auditConnection.ExecuteNonQuery(GetCdcMappingInsertQuery());

				AuditTestHelper.RunNotificationCycleAndAssert(
					auditConnection,
					testSubscribers,
					"0x10", "2018-06-08 00:00:00.000",
					expectedLog: GetLogForNewRecords(bizO)
				);

				patternMatchingName = factory.Load<PatternMatchingName>(new ZQuery(PatternMatchingNameSchema.PMN_ParentId, bizO.PK))
					.ToList();
				AssertEquals(0, patternMatchingName.Count);
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestDeletePatternMatchingNameForNameLessThanTwoWords()
		{
			var factory = new BusinessObjectFactory();
			var bizO = CreateParentRecords(factory);
			factory.Save();
			SetNameToMultiWords(bizO);
			CreatePatternMatchingRecords(factory, bizO);
			factory.Save();

			using (var auditConnection = AuditTestHelper.GetAuditConnection())
			{
				var testSubscribers = new ActualDataChangesAuditSubscriber[]
				{
					NewDataChangeSubscriber()
				};

				AuditTestHelper.CleanupAuditTestData(auditConnection, testSubscribers);
				AuditTestHelper.ResetSubscriberControlWaterMark(auditConnection, testSubscribers);

				var patternMatchingName =
					factory.Load<PatternMatchingName>(new ZQuery(PatternMatchingNameSchema.PMN_ParentId, bizO.PK)).ToList();
				AssertEquals(1, patternMatchingName.Count);

				var insertOriginalQuery = GetOriginalRecordCDCInsertQuery(bizO);
				auditConnection.ExecuteNonQuery(insertOriginalQuery);

				var updateNameToSingleWordQuery = GetUpdateRecordNameToSingleWordCDCInsertQuery(bizO);
				auditConnection.ExecuteNonQuery(updateNameToSingleWordQuery);

				auditConnection.ExecuteNonQuery(GetLsnMappingInsertQuery());

				auditConnection.ExecuteNonQuery(GetCdcMappingInsertQuery());

				AuditTestHelper.RunNotificationCycleAndAssert(
					auditConnection,
					testSubscribers,
					"0x10", "2018-06-08 00:00:00.000",
					expectedLog: GetLogForUpdateNameToSingleWordRecords(bizO)
				);

				patternMatchingName = factory.Load<PatternMatchingName>(new ZQuery(PatternMatchingNameSchema.PMN_ParentId, bizO.PK))
					.ToList();
				AssertEquals(0, patternMatchingName.Count);
			}
		}

		protected abstract BetterLogForTest GetLogForUpdateNameToSingleWordRecords(GlbStaff bizO);

		protected abstract string GetUpdateRecordNameToSingleWordCDCInsertQuery(GlbStaff bizO);

		protected abstract void SetNameToSingleWord(GlbStaff bizO);

		protected abstract void SetNameToMultiWords(GlbStaff bizO);
	}
}
