
using Moq;

namespace CargoWise.EntityFramework.Testing
{
	sealed class ActiveBusinessObjectCollectionIndexNotifierTest : TestCaseWithFactory
	{
		public void TestForReturnsNonNull()
		{
			AssertNotNull("For method should not return null", ActiveBusinessObjectCollectionIndexNotifier.For(Factory));
		}

		public void TestForReturnsTheSameNotifierForTheSameFactory()
		{
			var notifier1 = ActiveBusinessObjectCollectionIndexNotifier.For(Factory);
			var notifier2 = ActiveBusinessObjectCollectionIndexNotifier.For(Factory);

			Assert("For method should return the same notifier for the same factory", object.ReferenceEquals(notifier1, notifier2));
		}

		public void TestForReturnsDifferentNotifiersForTheDifferentFactories()
		{
			var anotherFactory = new BusinessObjectFactory();

			var notifier1 = ActiveBusinessObjectCollectionIndexNotifier.For(Factory);
			var notifier2 = ActiveBusinessObjectCollectionIndexNotifier.For(anotherFactory);

			Assert("For method should return different notifiers for different factories", !object.ReferenceEquals(notifier1, notifier2));
		}

		public void TestAddCausesTheIndexObjectToReceiveTheOnSavedNotificationOnFactorySave()
		{
			var notifier = ActiveBusinessObjectCollectionIndexNotifier.For(Factory);
			var onSavedCalled = false;

			var collectionMock = new Mock<IActiveBusinessObjectCollectionIndex>();
			collectionMock.Setup(c => c.OnFactorySaved(It.IsAny<bool>())).Callback(() => onSavedCalled = true);

			notifier.Add(collectionMock.Object);

			Factory.NewWithValidTestData<DummyBusinessObject>();
			Factory.Save();

			Assert("IActiveBusinessObjectCollectionIndex.OnSaved should have been called", onSavedCalled);
		}

		public void TestAddIsIdempotent()
		{
			var notifier = ActiveBusinessObjectCollectionIndexNotifier.For(Factory);
			var numberOfCalls = 0;

			var collectionMock = new Mock<IActiveBusinessObjectCollectionIndex>();
			collectionMock.Setup(c => c.OnFactorySaved(It.IsAny<bool>())).Callback(() => numberOfCalls++);

			notifier.Add(collectionMock.Object);
			AssertNoExceptionThrown(() => notifier.Add(collectionMock.Object));

			Factory.NewWithValidTestData<DummyBusinessObject>();
			Factory.Save();

			AssertEquals("IActiveBusinessObjectCollectionIndex.OnSaved should have been only called once", 1, numberOfCalls);
		}

		public void TestRemoveCausesTheIndexObjectNotToReceiveTheOnSavedNotificationOnFactorySave()
		{
			var notifier = ActiveBusinessObjectCollectionIndexNotifier.For(Factory);
			var onSavedCalled = false;

			var collectionMock = new Mock<IActiveBusinessObjectCollectionIndex>();
			collectionMock.Setup(c => c.OnFactorySaved(It.IsAny<bool>())).Callback(() => onSavedCalled = true);

			notifier.Add(collectionMock.Object);
			notifier.Remove(collectionMock.Object);

			Factory.NewWithValidTestData<DummyBusinessObject>();
			Factory.Save();

			Assert("IActiveBusinessObjectCollectionIndex.OnSaved shouldn't have been called", !onSavedCalled);
		}

		public void TestRemoveIsIdempotent()
		{
			var notifier = ActiveBusinessObjectCollectionIndexNotifier.For(Factory);
			var collectionMock = new Mock<IActiveBusinessObjectCollectionIndex>();

			AssertNoExceptionThrown(() => notifier.Remove(collectionMock.Object));
		}

		public void TestClearRemovesAllTheIndexObjectsFromTheNotifier()
		{
			var notifier = ActiveBusinessObjectCollectionIndexNotifier.For(Factory);
			var onSavedCalled = false;

			var collectionMock1 = new Mock<IActiveBusinessObjectCollectionIndex>();
			collectionMock1.Setup(c => c.OnFactorySaved(It.IsAny<bool>())).Callback(() => onSavedCalled = true);

			var collectionMock2 = new Mock<IActiveBusinessObjectCollectionIndex>();
			collectionMock2.Setup(c => c.OnFactorySaved(It.IsAny<bool>())).Callback(() => onSavedCalled = true);

			notifier.Add(collectionMock1.Object);
			notifier.Add(collectionMock2.Object);

			notifier.Clear();

			Factory.NewWithValidTestData<DummyBusinessObject>();
			Factory.Save();

			Assert("IActiveBusinessObjectCollectionIndex.OnSaved shouldn't have been called", !onSavedCalled);
		}
	}
}
