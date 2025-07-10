using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	abstract class AQISCollectionTest<T1, T2> : NonPersistentBusinessObjectCollectionTestCase<T1>
			where T1 : AQISCollection<T2>
			where T2 : NonPersistentBusinessObject
	{
		public void TestSortByUniqueCode()
		{
			var testCollection = GetCollectionToTest();

			IAQISUniqueCodeForSort firstItem = (IAQISUniqueCodeForSort)testCollection.AddNew();
			foreach (ZPropertyInfo info in firstItem.CodeInfosToSortByForTestingOnly)
			{
				info.Value = new ZString("ZZZ");
			}

			IAQISUniqueCodeForSort secondItem = (IAQISUniqueCodeForSort)testCollection.AddNew();
			foreach (ZPropertyInfo info in secondItem.CodeInfosToSortByForTestingOnly)
			{
				info.Value = new ZString("AAA");
			}

			AssertEquals("There are two items in the collection", 2, testCollection.Count);
			testCollection.SortByUniqueCode();

			IAQISUniqueCodeForSort firstItemAfterSort = (IAQISUniqueCodeForSort)((System.Collections.IList)testCollection)[0];
			IAQISUniqueCodeForSort secondItemAfterSort = (IAQISUniqueCodeForSort)((System.Collections.IList)testCollection)[1];

			AssertEquals("FirstItemAfterSort should be 'AAA'", "AAA", firstItemAfterSort.CodesToSortBy[0]);
			AssertEquals("SecondItemAfterSort should be 'ZZZ'", "ZZZ", secondItemAfterSort.CodesToSortBy[0]);
		}
	}
}
