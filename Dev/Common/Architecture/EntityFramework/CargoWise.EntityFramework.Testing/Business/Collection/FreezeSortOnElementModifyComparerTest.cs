using System.Collections;

namespace CargoWise.EntityFramework.Testing
{
	sealed class FreezeSortOnElementModifyComparerTest : TestCaseWithFactory
	{
		public void TestWrap()
		{
			FreezeSortOnElementModifyComparer comparer = FreezeSortOnElementModifyComparer.Wrap(new TestComparer());
			AssertEquals(true, comparer.Comparer is TestComparer);
			comparer = FreezeSortOnElementModifyComparer.Wrap(comparer);
			AssertEquals(true, comparer.Comparer is TestComparer);
		}

		public void TestEquals()
		{
			AssertEquals(
				"Equals",
				FreezeSortOnElementModifyComparer.Wrap(new TestComparer()),
				FreezeSortOnElementModifyComparer.Wrap(new TestComparer()));
			AssertEquals(
				"GetHashCode",
				FreezeSortOnElementModifyComparer.Wrap(new TestComparer()).GetHashCode(),
				FreezeSortOnElementModifyComparer.Wrap(new TestComparer()).GetHashCode());
		}

		public void TestCompare()
		{
			AssertEquals(
				"Compare calls through",
				64,
				FreezeSortOnElementModifyComparer.Wrap(new TestComparer()).Compare(null, null));
		}

		#region Test Classes

		class TestComparer : IComparer
		{
			public int Compare(object x, object y)
			{
				return 64;
			}

			public override bool Equals(object obj)
			{
				return obj is TestComparer;
			}

			public override int GetHashCode()
			{
				return 64;
			}
		}

		#endregion
	}
}
