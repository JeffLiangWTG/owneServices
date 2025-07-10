using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business.Testing
{
	public abstract class NonPersistentBusinessObjectCollectionRegistryDataTypeTestCase<T, U> : NonPersistentBusinessObjectRegistryDataTypeTestCase<T>
				where T : NonPersistentBusinessObjectCollectionRegistryDataType<U>
				where U : RegistryBusinessObjectCollection
	{
		public void TestDeserialise_RemovesEmptyItems()
		{
			var list = new CodeDescriptionPairList();
			list.AddPair("CAN", "Cancel");
			list.AddPair("", "Closed");
			list.AddPair("OPN", "");
			list.AddPair("", "");

			var collection = new DummyRegistryBusinessObjectCollection(list, 3);
			AssertEquals("Precondtion: collection.Count", 4, collection.Count);

			var dataType = new NonPersistentBusinessObjectCollectionRegistryDataTypeForTesting();
			var deserialisedCollection = dataType.Deserialise(dataType.Serialise(collection));

			AssertEquals("Should remove items with empty Code and Description from collection when deserialised", 3, deserialisedCollection.Count);
		}

		#region Implementation

		class NonPersistentBusinessObjectCollectionRegistryDataTypeForTesting : NonPersistentBusinessObjectCollectionRegistryDataType<DummyRegistryBusinessObjectCollection>
		{
			public NonPersistentBusinessObjectCollectionRegistryDataTypeForTesting()
			{
			}
		}

		#endregion
	}
}
