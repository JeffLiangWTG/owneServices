using System.ComponentModel;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	sealed class SortInfoTest : TestCase
	{
		public void TestSortInfoConstructor()
		{
			SortInfo info = new SortInfo(AutoDummyBizo.Schema.Z0_Description, ListSortDirection.Ascending);
			AssertEquals(info.Direction, ListSortDirection.Ascending);
			AssertEquals(info.PropertyName, AutoDummyBizo.Schema.Z0_Description);
		}

		public void TestEquals()
		{
			SortInfo info = new SortInfo(AutoDummyBizo.Schema.Z0_Description, ListSortDirection.Ascending);
			SortInfo info2 = new SortInfo(AutoDummyBizo.Schema.Z0_Description, ListSortDirection.Ascending);
			SortInfo info3 = new SortInfo(AutoDummyBizo.Schema.Z0_Description, ListSortDirection.Descending);
			AssertEquals("Should be equal", info, info2);
			Assert("Should not be equal", !info3.Equals(info2));
		}
	}
}
