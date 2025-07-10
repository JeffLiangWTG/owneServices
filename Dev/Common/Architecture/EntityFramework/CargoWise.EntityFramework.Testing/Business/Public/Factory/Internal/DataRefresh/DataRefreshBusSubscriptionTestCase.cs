using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Business.Testing
{
	[TestsSubclassesOf(typeof(DataRefreshBus.Subscription))]
	abstract class DataRefreshBusSubscriptionTestCase<T> : TestCaseWithFactory
			where T : DataRefreshBus.Subscription
	{
		#region Implementation
		protected abstract T GetSubscription(BusinessObjectFactory factory);
		protected abstract IEnumerable<BusinessObject> GetObjectsToPublish(BusinessObjectFactory factory);

		#endregion

		public void TestGetObjectsToPublish()
		{
			var factory = new BusinessObjectFactory();
			var objectsToPublish = GetObjectsToPublish(factory);
			AssertNotNull(objectsToPublish);
			Assert("Should be at least one object to publish", objectsToPublish.Any());
			Assert("Published objects need to have a different factory", objectsToPublish.OfType<BusinessObject>().All(f => f.Factory == factory));
		}

		public void TestPreCondition_ObjectsToPublishDoesNotHitTheDb()
		{
			var factory = new BusinessObjectFactory();
			var expected = new Dictionary<string, int>();
			using (AssertDbHitsForAllFactories(expected, includeFactoryPredicate: f => f == factory))
			{
				Assert(GetObjectsToPublish(factory).All(_ => true));
			}
		}

		public void TestSubscription()
		{
			AssertNotNull(GetSubscription(new BusinessObjectFactory()));
		}

		public void TestSubscriptionParticipant()
		{
			AssertNotNull(GetSubscription(new BusinessObjectFactory()).Participant);
		}

		public void TestTakeAction_NoFactoryLoad()
		{
			var publishedObjectFactory = Factory.CreateNewFactory();
			var publishedObjects = GetObjectsToPublish(publishedObjectFactory);
			var subscriptionFactory = Factory.CreateNewFactory();
			var subscription = GetSubscription(subscriptionFactory);
			subscriptionFactory.ResetDatabaseLoadCount();

			var expected = new Dictionary<string, int>();
			using (AssertDbHitsForAllFactories(expected, includeFactoryPredicate: f => f == subscriptionFactory))
			{
				publishedObjectFactory.Save();
				subscription.TakeAction(publishedObjects);
			}
		}
	}
}
