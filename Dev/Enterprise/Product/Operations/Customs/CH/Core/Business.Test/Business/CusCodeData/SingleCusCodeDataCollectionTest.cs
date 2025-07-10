
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CH.Business.Testing;

public abstract class SingleCusCodeDataCollectionTest<T> : BusinessObjectCollectionTestCase
														where T : CusCodeData
{
	public void TestFindOrCreate() => CombineAssertions(() =>
	{
		var collection = GetCollectionToTest() as SingleCusCodeDataCollection<T>;
		Factory.Save();
		AssertEquals("Empty", 0, collection.Count);
		collection.FindOrCreate();
		AssertEquals("First element", 1, collection.Count);
		AssertEquals("Still got only one element", 1, collection.Count);
	});
}
