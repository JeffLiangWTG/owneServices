using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using Enterprise.AuditDataServices.MDM.Subscribers;
using Enterprise.AuditDataServices.Subscription;
using Enterprise.AuditDataServices.Subscription.Common;
using Enterprise.AuditDataServices.Subscription.Testing;

namespace Enterprise.AuditDataServices.MDM.Testing
{
	abstract class PatternMatchingCountryCodeSubscriberTest<T, S> : ActualDataChangesAuditSubscriberTest where T : BusinessObject where S : ActualDataChangesAuditSubscriber
	{
		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestSubscriberProcessesChangesForCountryCode()
		{
			var factory = new BusinessObjectFactory();
			var bizO = CreateParentRecords(factory);

			CreatePatternMatchingRecords(factory, bizO);
			factory.Save();
			AssertPatternMatchingCountryCodePrecondition(factory, bizO);

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

				var insertCDCHistoryText = GetCDCHistoryInsertQuery();
				auditConnection.ExecuteNonQuery(insertCDCHistoryText);

				AuditTestHelper.RunNotificationCycleAndAssert(
					auditConnection,
					testSubscribers,
					"0x10", "2018-06-08 00:00:00.000",
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

		protected abstract string GetLog { get; }

		protected abstract string GetGenericQuery { get; }

		protected abstract T CreateParentRecords(BusinessObjectFactory factory);

		protected abstract void CreatePatternMatchingRecords(BusinessObjectFactory factory, T bizO);

		protected abstract string GetLsnMappingInsertQuery();

		protected abstract string GetCDCHistoryInsertQuery();

		protected abstract string GetOriginalRecordCDCInsertQuery(T bizO);

		protected abstract string GetUpdateRecordCDCInsertQuery(T bizO);

		protected abstract BetterLogForTest GetLogForExistingRecords(T bizO);

		protected abstract string GetPatternMasters(T bizO);

		protected abstract void AssertPatternMatchingCountryCodePrecondition(BusinessObjectFactory factory, T bizO);

		protected abstract void AssertPatternMatchingForExistingRecords(BusinessObjectFactory factory, T bizO);
	}
}
