using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CH.Business.Testing;

public abstract class SingleCusSupportingInfoCollectionTest<T> : BusinessObjectCollectionTestCase
														where T : CusSupportingInfo
{
	public void TestFindOrCreate() => CombineAssertions(() =>
	{
		var collection = GetCollectionToTest() as SingleCusSupportingInfoCollection<T>;

		AssertEquals("Initial empty", 0, collection.Count);
		collection.FindOrCreate();
		AssertEquals("Added first element", 1, collection.Count);
		collection.FindOrCreate();
		AssertEquals("Still got only one element", 1, collection.Count);
	});
}
