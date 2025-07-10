using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.LogWalker.Testing
{
	sealed class LogSubscriberInternalTest : TestCaseWithFactory
	{
		public void TestCollectionIsDeactivated()
		{
			var bizo = Factory.New<DummyWithDependentsBusinessObject>();
			var collection = new ActiveBusinessObjectCollection<DummyDependantBusinessObject>(bizo);
			collection.AddNew();
			((IBusiness)collection).IncrementReadOnlyIncludingChildren();
			AssertEquals(false, collection.IsDeactivated);

			MockSubscriber testSubscriber = new MockSubscriber();
			StmJobQueue log = testSubscriber.QueueNewLog(Factory, bizo);
			testSubscriber.ProcessLogs(new[] { log });

			AssertEquals("Log processed?", true, testSubscriber.LastProcessedLog_Pk.IsValid && !testSubscriber.LastProcessedLog_Pk.IsEmpty);

			AssertEquals(true, collection.IsDeactivated);
		}

		public void TestAllSubscribersHaveTablesAndEventCodes()
		{
			var subscribersWithoutTableNames = string.Join(System.Environment.NewLine, new SubscriberProvider().AllLogSubscribers.Where(t => !t.TableNames.Any()).Select(s => s.Name));
			var subscribersWithoutEventCodes = string.Join(System.Environment.NewLine, new SubscriberProvider().AllLogSubscribers.Where(t => !t.EventTypes.Any()).Select(s => s.Name));
			CombineAssertions(() =>
			{
				AssertEquals("These tables didn't have table names", "WorkflowEventPublish", subscribersWithoutTableNames);
				AssertEquals("These tables didn't have event codes", "WorkflowEventPublish", subscribersWithoutTableNames);
			});
		}

		#region Helper Methods

		public static void AddLog(BusinessObjectFactory factory, string tableName, string eventType)
		{
			var log = factory.New<StmALog>();
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_Parent = ZGuid.Empty;
				log.SL_Table = tableName;
				log.SL_SE_NKEvent = eventType;
			}
		}

		#endregion
	}
}
