using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(PRAContainerCollection))]
	sealed class PRAContainerCollectionTest : NonPersistentBusinessObjectCollectionTestCase<PRAContainerCollection>
	{
		public void TestSomeFormOfIContainerMessagingObjectGetsReturnedWhenAddNewIsCalled()
		{
			PRAContainerCollection collection = new PRAContainerCollection(Factory);
			IPRAContainerMessaging container = collection.AddNew();
			AssertNotNull("Some object (don't care what really as long as it's not null) of type IContainerMessaging should be returned by AddNew()", container);
		}

		public void TestAddingRealBOImplementingIContainerMessagingToThisCollectionDoesntBork()
		{
			var container = (BusinessObject)Factory.New<Integration.Customs.AU.ICusContainer>();
			PRAContainerCollection collection = new PRAContainerCollection(Factory);
			collection.Add(container);
			AssertEquals("collection[0]", container, collection[0]);
		}

		protected override PRAContainerCollection GetCollectionToTest() => new PRAContainerCollection(Factory);

		protected override BusinessObject GetNewElementToAddToTheCollection() => new DummyIContainerMessaging(Factory);
	}
}
