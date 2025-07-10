using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Business.Testing
{
	[TestedType(typeof(DataRefreshBusSubscription))]
	sealed class ActiveBusinessObjectCollectionDataRefreshAdderTest : DataRefreshBusSubscriptionTestCase<DataRefreshBusSubscription> 
	{
		protected override IEnumerable<BusinessObject> GetObjectsToPublish(BusinessObjectFactory factory)
		{
			return new[] { factory.New<DummyBusinessObject>() };
		}

		protected override DataRefreshBusSubscription GetSubscription(BusinessObjectFactory factory)
		{
			var subscriber = ActiveBusinessObjectCollectionDataRefreshAdder.GetInstance(DummyBusinessObject.Schema.TableName, factory);
			return new DataRefreshBusSubscription(factory, subscriber);
		}
	}
}
