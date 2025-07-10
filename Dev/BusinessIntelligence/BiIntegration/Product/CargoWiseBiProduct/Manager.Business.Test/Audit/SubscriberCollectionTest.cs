using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace CargoWise.Bi.Product.Manager.Business
{
	[TestedType(typeof(SubscriberCollection))]
	class SubscriberCollectionTest : NonPersistentBusinessObjectCollectionTestCase<SubscriberCollection>
	{
		protected override SubscriberCollection GetCollectionToTest()
		{
			return new SubscriberCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new SubscriberInfo("");
		}
	}
}
