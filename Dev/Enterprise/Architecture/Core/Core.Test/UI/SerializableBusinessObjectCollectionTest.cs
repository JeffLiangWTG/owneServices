using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class SerializableBusinessObjectCollectionTest : TestCaseWithFactory
	{
		public void TestAddBaseBusinessObjects()
		{
			var dummy1 = Factory.New<DummyBusinessObject>();
			var dummy2 = Factory.New<DummyBusinessObject>();

			var collection = new WrappedBusinessObjectCollection();
			collection.AddBaseBusinessObjects(new BusinessObject[] { dummy1, dummy2 });

			AssertEquals(2, collection.Count);

			AssertEquals(typeof(WrappedBusinessObject), collection[0].GetType());
			AssertSame(dummy1, collection[0].BaseBusinessObject);

			AssertEquals(typeof(WrappedBusinessObject), collection[1].GetType());
			AssertSame(dummy2, collection[1].BaseBusinessObject);

			var dummy3 = Factory.New<DummyBusinessObject>();

			collection.AddBaseBusinessObjects(new BusinessObject[] { dummy1, dummy3 });

			AssertEquals(4, collection.Count);

			AssertEquals(typeof(WrappedBusinessObject), collection[2].GetType());
			AssertSame(dummy1, collection[2].BaseBusinessObject);
			Assert("No checks for duplicates", !object.ReferenceEquals(collection[0], collection[2]));

			AssertEquals(typeof(WrappedBusinessObject), collection[3].GetType());
			AssertSame(dummy3, collection[3].BaseBusinessObject);
		}

		public void TestAsArrayList()
		{
			var dummy1 = Factory.New<DummyBusinessObject>();
			var dummy2 = Factory.New<DummyBusinessObject>();

			var collection = new WrappedBusinessObjectCollection();
			collection.AddBaseBusinessObjects(new BusinessObject[] { dummy1, dummy2 });

			var arrayList = collection.AsArrayList();

			AssertEquals(2, arrayList.Count);
			AssertSame(dummy1, arrayList[0]);
			AssertSame(dummy2, arrayList[1]);
		}

		public void TestWrappBusinessObjectCollectionSerializable()
		{
			var collection = new WrappedBusinessObjectCollection();
#pragma warning disable SYSLIB0050 // Type or member is obsolete
			Assert(collection.GetType().IsSerializable);
#pragma warning restore SYSLIB0050 // Type or member is obsolete
		}
	}
}
