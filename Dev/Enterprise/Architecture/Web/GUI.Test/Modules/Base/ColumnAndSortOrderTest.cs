using System.ComponentModel;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.Modules.Testing
{
	sealed class ColumnAndSortOrderTest : TestCase
	{
		public void TestConstructor()
		{
			var columnAndSortOrder = new ColumnAndSortOrder("AAA", ListSortDirection.Descending);
			AssertEquals("AAA", columnAndSortOrder.OrderByColumnName);
			AssertEquals(ListSortDirection.Descending, columnAndSortOrder.SortDirection);
		}
	}
}
