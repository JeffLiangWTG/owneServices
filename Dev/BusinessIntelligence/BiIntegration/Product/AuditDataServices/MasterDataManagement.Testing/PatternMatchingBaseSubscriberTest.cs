using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Tools.DuplicateDetector;
using CargoWise.Types;
using Enterprise.AuditDataServices.MDM.Subscribers;
using Enterprise.AuditDataServices.Subscription;
using Enterprise.AuditDataServices.Subscription.Common;
using Enterprise.AuditDataServices.Subscription.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.AuditDataServices.MDM.Testing
{
	abstract class PatternMatchingBaseSubscriberTest<T, S> : ActualDataChangesAuditSubscriberTest where T : BusinessObject where S : ActualDataChangesAuditSubscriber
	{
		protected abstract string GetGenericQuery { get; }

		protected abstract string GetLog { get; }

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestSubscriberProcessesChangesForNewRecords()
		{
			var factory = new BusinessObjectFactory();

			var bizO = CreateParentRecords(factory);

			factory.Save();

			using (var auditConnection = AuditTestHelper.GetAuditConnection())
			{
				var testSubscribers = new ActualDataChangesAuditSubscriber[] {
					NewDataChangeSubscriber()
				};

				AuditTestHelper.CleanupAuditTestData(auditConnection, testSubscribers);
				AuditTestHelper.ResetSubscriberControlWaterMark(auditConnection, testSubscribers);

				AssertPatternMatchingNewRecordsPrecondition(factory, bizO);

				var insertIntoCDCText = GetNewRecordCDCInsertQuery(bizO);
				auditConnection.ExecuteNonQuery(insertIntoCDCText);

				var insertLsnMappingText = GetLsnMappingInsertQuery();
				auditConnection.ExecuteNonQuery(insertLsnMappingText);

				var insertCdcMappingText = GetCdcMappingInsertQuery();
				auditConnection.ExecuteNonQuery(insertCdcMappingText);

				AuditTestHelper.RunNotificationCycleAndAssert(
					auditConnection,
					testSubscribers,
					"0x10", "2018-03-08 00:00:00.000",
					expectedLog: GetLogForNewRecords(bizO)
				);

				AssertPatternMatchingForNewRecords(factory, bizO);
				AssertQUEResultRecordExists(factory, bizO);
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestSubscriberDoesNotSavePlaceholderRecords()
		{
			var factory = new BusinessObjectFactory();

			var bizO = CreatePlaceholderRecords(factory);

			factory.Save();

			using (var auditConnection = AuditTestHelper.GetAuditConnection())
			{
				var testSubscribers = new ActualDataChangesAuditSubscriber[] {
					NewDataChangeSubscriber()
				};

				AuditTestHelper.CleanupAuditTestData(auditConnection, testSubscribers);
				AuditTestHelper.ResetSubscriberControlWaterMark(auditConnection, testSubscribers);

				var insertIntoCDCText = GetNewRecordCDCInsertQuery(bizO);
				auditConnection.ExecuteNonQuery(insertIntoCDCText);

				var insertLsnMappingText = GetLsnMappingInsertQuery();
				auditConnection.ExecuteNonQuery(insertLsnMappingText);

				var insertCdcMappingText = GetCdcMappingInsertQuery();
				auditConnection.ExecuteNonQuery(insertCdcMappingText);

				AuditTestHelper.RunNotificationCycleAndAssert(
					auditConnection,
					testSubscribers,
					"0x10", "2018-06-08 00:00:00.000",
					expectedLog: GetLogForNewRecords(bizO)
				);

				AssertNoPatternMatchingForPlaceholderRecords(factory, bizO);
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestSubscriberProcessesChangesForExistingRecords()
		{
			var factory = new BusinessObjectFactory();

			var bizO = CreateParentRecords(factory);

			CreatePatternMatchingRecords(factory, bizO);

			factory.Save();

			using (var auditConnection = AuditTestHelper.GetAuditConnection())
			{
				var testSubscribers = new ActualDataChangesAuditSubscriber[] {
					NewDataChangeSubscriber()
				};

				AuditTestHelper.CleanupAuditTestData(auditConnection, testSubscribers);
				AuditTestHelper.ResetSubscriberControlWaterMark(auditConnection, testSubscribers);

				var insertIntoCDCOriginalText = GetOriginalRecordCDCInsertQuery(bizO);
				auditConnection.ExecuteNonQuery(insertIntoCDCOriginalText);

				var insertIntoCDCUpdatedText = GetUpdateRecordCDCInsertQuery(bizO);
				auditConnection.ExecuteNonQuery(insertIntoCDCUpdatedText);

				var insertLsnMappingText = GetLsnMappingInsertQuery();
				auditConnection.ExecuteNonQuery(insertLsnMappingText);

				var insertCdcMappingText = GetCdcMappingInsertQuery();
				auditConnection.ExecuteNonQuery(insertCdcMappingText);

				AuditTestHelper.RunNotificationCycleAndAssert(
					auditConnection,
					testSubscribers,
					"0x10", "2018-03-08 00:00:00.000",
					expectedLog: GetLogForExistingRecords(bizO)
				);

				AssertPatternMatchingForExistingRecords(factory, bizO);
				AssertQUEResultRecordExists(factory, bizO);
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestSubscriberDoesNotProcessChangesForNonSubscribedColumns()
		{
			var factory = new BusinessObjectFactory();

			var bizO = CreateParentRecords(factory);

			CreatePatternMatchingRecords(factory, bizO);

			factory.Save();

			using (var auditConnection = AuditTestHelper.GetAuditConnection())
			{
				var testSubscribers = new ActualDataChangesAuditSubscriber[] {
					NewDataChangeSubscriber()
				};
				AuditTestHelper.ResetSubscriberControlWaterMark(auditConnection, testSubscribers);

				var insertIntoCDCOriginalText = GetOriginalRecordCDCInsertQuery(bizO);
				auditConnection.ExecuteNonQuery(insertIntoCDCOriginalText);

				var insertIntoCDCNonSubscribedText = GetNonSubscribedColumnsCDCInsertQuery(bizO);
				auditConnection.ExecuteNonQuery(insertIntoCDCNonSubscribedText);

				AuditTestHelper.RunNotificationCycleAndAssert(
					auditConnection,
					testSubscribers,
					"0x10", "2018-03-08 00:00:00.000",
					expectedLog: null
				);

				AssertPatternMatchingForNonSubscribedColumns(factory, bizO);
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestSubscriberDeleteRecordsWhenDataChangeToPlaceholder()
		{
			var factory = new BusinessObjectFactory();

			var bizO = CreateParentRecords(factory);

			CreatePatternMatchingRecords(factory, bizO);

			factory.Save();

			using (var auditConnection = AuditTestHelper.GetAuditConnection())
			{
				var testSubscribers = new ActualDataChangesAuditSubscriber[] {
					NewDataChangeSubscriber()
				};

				AuditTestHelper.CleanupAuditTestData(auditConnection, testSubscribers);
				AuditTestHelper.ResetSubscriberControlWaterMark(auditConnection, testSubscribers);

				var insertIntoCDCOriginalText = GetOriginalRecordCDCInsertQuery(bizO);
				auditConnection.ExecuteNonQuery(insertIntoCDCOriginalText);

				var insertIntoCDCUpdatedText = GetUpdateRecordToPlaceHolderCDCInsertQuery(bizO);
				auditConnection.ExecuteNonQuery(insertIntoCDCUpdatedText);

				var insertLsnMappingText = GetLsnMappingInsertQuery();
				auditConnection.ExecuteNonQuery(insertLsnMappingText);

				var insertCdcMappingText = GetCdcMappingInsertQuery();
				auditConnection.ExecuteNonQuery(insertCdcMappingText);

				AuditTestHelper.RunNotificationCycleAndAssert(
					auditConnection,
					testSubscribers,
					"0x10", "2018-03-08 00:00:00.000",
					expectedLog: GetLogForDeletedRecords(bizO)
				);

				AssertNoPatternMatchingForPlaceholderRecords(factory, bizO);
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestSubscriberCreateRecordsWhenOriginalRecordsNotExist()
		{
			var factory = new BusinessObjectFactory();

			var bizO = CreateParentRecords(factory);

			factory.Save();

			using (var auditConnection = AuditTestHelper.GetAuditConnection())
			{
				var testSubscribers = new ActualDataChangesAuditSubscriber[] {
					NewDataChangeSubscriber()
				};

				AuditTestHelper.CleanupAuditTestData(auditConnection, testSubscribers);
				AuditTestHelper.ResetSubscriberControlWaterMark(auditConnection, testSubscribers);

				AssertPatternMatchingNewRecordsPrecondition(factory, bizO);

				var insertIntoCDCOriginalText = GetOriginalRecordCDCInsertQuery(bizO);
				auditConnection.ExecuteNonQuery(insertIntoCDCOriginalText);

				var insertIntoCDCUpdatedText = GetUpdateRecordCDCInsertQuery(bizO);
				auditConnection.ExecuteNonQuery(insertIntoCDCUpdatedText);

				var insertLsnMappingText = GetLsnMappingInsertQuery();
				auditConnection.ExecuteNonQuery(insertLsnMappingText);

				var insertCdcMappingText = GetCdcMappingInsertQuery();
				auditConnection.ExecuteNonQuery(insertCdcMappingText);

				AuditTestHelper.RunNotificationCycleAndAssert(
					auditConnection,
					testSubscribers,
					"0x10", "2018-03-08 00:00:00.000",
					expectedLog: GetLogForExistingRecords(bizO)
				);

				AssertPatternMatchingForExistingRecords(factory, bizO);
			}
		}

		public void TestIsLoaded()
		{
			var pmServiceTask = new MDMSubscriberServiceTask();
			var pmSubscribers = new SubscriberLoader().EnumerateSubscribersOfType(pmServiceTask.AssemblyName, pmServiceTask.SubscriberNamespace);
			Assert(pmSubscribers.Any());

			foreach (object subscriber in pmSubscribers)
			{
				AssertNoExceptionThrown(() =>
				{
					var sub = subscriber as PatternMatchingSubscriber<BusinessObject>;
				});
			}
		}

		protected abstract T CreateParentRecords(BusinessObjectFactory factory);

		protected abstract T CreatePlaceholderRecords(BusinessObjectFactory factory);

		protected abstract void CreatePatternMatchingRecords(BusinessObjectFactory factory, T bizO);

		protected abstract string GetLsnMappingInsertQuery();

		protected abstract string GetCdcMappingInsertQuery();

		protected abstract string GetNewRecordCDCInsertQuery(T bizO);

		protected abstract string GetOriginalRecordCDCInsertQuery(T bizO);

		protected abstract string GetUpdateRecordCDCInsertQuery(T bizO);

		protected abstract string GetNonSubscribedColumnsCDCInsertQuery(T bizO);

		protected abstract string GetUpdateRecordToPlaceHolderCDCInsertQuery(T bizO);

		protected abstract BetterLogForTest GetLogForNewRecords(T bizO);

		protected abstract BetterLogForTest GetLogForExistingRecords(T bizO);

		protected abstract BetterLogForTest GetLogForDeletedRecords(T bizO);

		protected abstract string GetPatternMasters(T bizO);

		protected abstract void AssertPatternMatchingNewRecordsPrecondition(BusinessObjectFactory factory, T bizO);

		protected abstract void AssertNoPatternMatchingForPlaceholderRecords(BusinessObjectFactory factory, T bizO);

		protected abstract void AssertPatternMatchingForNewRecords(BusinessObjectFactory factory, T bizO);

		protected abstract void AssertPatternMatchingForExistingRecords(BusinessObjectFactory factory, T bizO);

		protected abstract void AssertQUEResultRecordExists(BusinessObjectFactory factory, T bizO);

		protected void AssertQueuedRecordExists(BusinessObjectFactory factory, ZGuid masterPK, ZString ultimateParentPrefix)
		{
			var statusCode = string.Empty;
			if (ultimateParentPrefix == OrgHeaderSchema.Constants.Prefix)
			{
				statusCode = PatternMatchingResult.StatusCodes.Queued;
			}
			else if (ultimateParentPrefix == GlbPersonSchema.Constants.Prefix)
			{
				statusCode = PatternMatchingResult.StatusCodes.QueuedForProcessing;
			}

			Assert(factory.Exists(typeof(PatternMatchingResult), new ZQuery(PatternMatchingResultSchema.PMT_MasterPK, masterPK)
					.AddToFilter(PatternMatchingResultSchema.PMT_Status, statusCode)
					.AddToFilter(PatternMatchingResultSchema.PMT_MasterTableCode, ultimateParentPrefix)));
		}

		protected abstract void AssertPatternMatchingForNonSubscribedColumns(BusinessObjectFactory factory, T bizO);

		internal int GetUpperValueToHash(string value)
		{
			return value == null ? 0 : TextStandardizerHelper.ComputeStringHashFast(value.ToUpperInvariant());
		}
	}
}
