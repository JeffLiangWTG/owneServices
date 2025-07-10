using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Business.Testing
{
	[TestedType(typeof(DataRefreshBus.BusinessObjectCollectionSubscription))]
	sealed class BusinessObjectCollectionSubscriptionTest : DataRefreshBusSubscriptionTestCase<DataRefreshBus.BusinessObjectCollectionSubscription>
	{
		protected override IEnumerable<BusinessObject> GetObjectsToPublish(BusinessObjectFactory factory)
		{
			return new[] { factory.New<DummyBusinessObject>() };
		}

		protected override DataRefreshBus.BusinessObjectCollectionSubscription GetSubscription(BusinessObjectFactory factory)
		{
			var collection = new DummyBusinessObjectCollection(factory, new ZQuery());
			collection.Load();
			return new DataRefreshBus.BusinessObjectCollectionSubscription(collection);
		}
	}
}
