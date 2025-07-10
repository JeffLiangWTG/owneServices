using System.ComponentModel;

namespace CargoWise.EntityFramework.Testing
{
	sealed class PercentageStringComparerTest : TestCaseWithFactory
	{
		public void TestCompare()
		{
			var dummy1 = Factory.New<DummyBusinessObject>();
			var dummy2 = Factory.New<DummyBusinessObject>();

			dummy1.Z0_VarCharMax = "100.02 %";
			dummy2.Z0_VarCharMax = "100.01 %";

			var ascComparer = new PercentageStringComparer(dummy1.GetType(), "Z0_VarCharMax", ListSortDirection.Ascending);

			AssertEquals(1, ascComparer.Compare(dummy1, dummy2));
			AssertEquals(-1, ascComparer.Compare(dummy2, dummy1));
			AssertEquals(0, ascComparer.Compare(dummy1, dummy1));

			var descComparer = new PercentageStringComparer(dummy1.GetType(), "Z0_VarCharMax", ListSortDirection.Descending);

			AssertEquals(-1, descComparer.Compare(dummy1, dummy2));
			AssertEquals(1, descComparer.Compare(dummy2, dummy1));
			AssertEquals(0, descComparer.Compare(dummy1, dummy1));
		}
	}
}
