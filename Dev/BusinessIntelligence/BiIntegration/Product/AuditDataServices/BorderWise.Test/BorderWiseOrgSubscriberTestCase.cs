using System.Linq;
using Enterprise.AuditDataServices.BorderWise.Subscribers;
using Enterprise.AuditDataServices.Subscription;
using Enterprise.AuditDataServices.Subscription.Common;
using Enterprise.AuditDataServices.Subscription.Testing;

namespace Enterprise.AuditDataServices.BorderWise.Test
{
	abstract class BorderWiseOrgSubscriberTestCase<T> : ActualDataChangesAuditSubscriberTest where T : ActualDataChangesAuditSubscriber
	{
		public void TestIsRequired()
		{
			using (TestHelper.EnableSyncForTest(out var configOverride))
			{
				Assert(TestDataChangeSubscriber.IsRequired());
			}
		}
		public void TestIsLoaded()
		{
			var borderWiseServiceTask = new BorderWiseSubscriberServiceTask();
			var borderWiseSubscribers = new SubscriberLoader().EnumerateSubscribersOfType(borderWiseServiceTask.AssemblyName, borderWiseServiceTask.SubscriberNamespace);
			Assert(borderWiseSubscribers.Any());

			foreach (object subscriber in borderWiseSubscribers)
			{
				AssertNoExceptionThrown(() =>
				{
					var sub = subscriber as BorderWiseSubscriberBase;
				});
			}
		}
	}
}
