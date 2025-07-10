using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class AQISUniqueCodeCaseInsensitiveComparerTest : TestCaseWithFactory
	{
		public void TestAQISUniqueCodeCaseInsensitiveComparer()
		{
			DummyAQISUniqueCodeForSort dummy1 = new DummyAQISUniqueCodeForSort();
			dummy1.CodesToSortByExposed = new ZString[] { "zzz", "AAA" };

			DummyAQISUniqueCodeForSort dummy2 = new DummyAQISUniqueCodeForSort();
			dummy2.CodesToSortByExposed = new ZString[] { "ZZZ", "bbb" };

			DummyAQISUniqueCodeForSort dummy3 = new DummyAQISUniqueCodeForSort();
			dummy3.CodesToSortByExposed = new ZString[] { "CCC", "BBB" };

			AQISUniqueCodeCaseInsensitiveComparer testComparer = new AQISUniqueCodeCaseInsensitiveComparer();
			int comparisonResult = testComparer.Compare(dummy1, dummy2);
			AssertEquals("Dummy1 < Dummy2", -1, comparisonResult);

			comparisonResult = testComparer.Compare(dummy2, dummy3);
			AssertEquals("Dummy2 > Dummy3", 1, comparisonResult);
		}

		sealed class DummyAQISUniqueCodeForSort : IAQISUniqueCodeForSort
		{
			public ZString[] CodesToSortByExposed;

			public ZString[] CodesToSortBy => CodesToSortByExposed;

			public ZPropertyInfo[] CodeInfosToSortByForTestingOnly => null;
		}
	}
}
