using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	sealed class RegistryBusinessObjectCollectionFindBoxListProviderTest : TestCase
	{
		public void TestCodeAndDescriptionFromPrimaryKey()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList();

			list.AddPair("ABC", "ABC Description");
			list.AddPair("XYZ", "XYZ Description");

			var collection = new DummyRegistryBusinessObjectCollection(list, 55);
			var collectionFindBoxListProvider = new RegistryBusinessObjectCollectionFindBoxListProvider(collection);

			AssertEquals(string.Empty, collectionFindBoxListProvider.DescriptionFromPrimaryKey(new ZGuid()));
			AssertEquals(string.Empty, collectionFindBoxListProvider.CodeFromPrimaryKey(new ZGuid()));
			AssertEquals("ABC", collectionFindBoxListProvider.CodeFromDescription("ABC Description"));
		}
	}
}
