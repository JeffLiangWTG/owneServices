using NUnit.Framework;

namespace CargoWise.Common.Collections.Testing
{
	class GroupingTest : TestCase
	{
		public void TestGrouping()
		{
			var items = new[] { "one", "two" };
			var group = new Grouping<int, string>(1, items);
			AssertEquals(1, group.Key);
			AssertContainsExactElementsInAnyOrder(items, group);
			group.Add("memes");
			AssertContainsExactElementsInAnyOrder(items.Append("memes"), group);
		}
	}
}