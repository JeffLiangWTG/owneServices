using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Business.Testing
{
	[TestedType(typeof(DataRefreshBus.BusinessObjectSubscription))]
	sealed class BusinessObjectSubscriptionTest : DataRefreshBusSubscriptionTestCase<DataRefreshBus.BusinessObjectSubscription>
	{
		protected override void SetUp()
		{
			base.SetUp();
			dummy = Factory.New<DummyBusinessObject>();
			Factory.Save();
		}

		DummyBusinessObject dummy;

		protected override IEnumerable<BusinessObject> GetObjectsToPublish(BusinessObjectFactory factory)
		{
			return new[] { factory.ImportFromAnotherFactory(dummy) };
		}

		protected override DataRefreshBus.BusinessObjectSubscription GetSubscription(BusinessObjectFactory factory)
		{
			var bizo = factory.ImportFromAnotherFactory(dummy);
			return new DataRefreshBus.BusinessObjectSubscription(bizo);
		}
	}
}
