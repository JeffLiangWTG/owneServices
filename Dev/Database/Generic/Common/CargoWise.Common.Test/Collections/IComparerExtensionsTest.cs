using NUnit.Framework;
using static CargoWise.Common.Collections.Testing.IEnumerableExtensionsTest;

namespace CargoWise.Common.Collections.Testing
{
	public class IComparerExtensionsTest : TestCase
	{
		public void TestGroupMerge()
		{
			var a11 = new A { I = 1 };
			var a12 = new A { I = 1 };
			var a2 = new A { I = 2 };
			var a3 = new A { I = 3 };
			var a4 = new A { I = 4 };
			AssertEquals(0, new AEqualityComparer().GroupMerge().Count);
			CombineAssertions("Single group. Not that you would use this.", () =>
			{
				var groups = new AEqualityComparer().GroupMerge(new[] { a11, a12, a2, a3, a4 });
				AssertEquals("A1 is not distinct.", groups.Count, 4);
				AssertEquals("One collection means one group", 1, groups[a11].Count);
				AssertEquals(1, groups.Depth);
				AssertContainsExactElementsInAnyOrder("Have the correct elements", new[] { a11, a12 }, groups[a11][0]);
				AssertContainsExactElementsInAnyOrder("You too", new[] { a2 }, groups[a2][0]);
			});
			CombineAssertions("Covering and subgroup", () =>
			{
				var g1 = new[] { a11, a12, a2, a3, a4 };
				var g2 = new[] { a11, a12 };
				var groups = new AEqualityComparer().GroupMerge(g1, g2);
				AssertEquals("A1 is not distinct.", groups.Count, 4);
				AssertEquals("Both groups live.", 2, groups[a11].Count);
				AssertEquals(2, groups.Depth);
				AssertContainsExactElementsInAnyOrder("First collection is grouped", new[] { a11, a12 }, groups[a11][0]);
				AssertContainsExactElementsInAnyOrder("Second collection is grouped", new[] { a11, a12 }, groups[a11][1]);
				AssertContainsExactElementsInAnyOrder("You too for one", new[] { a2 }, groups[a2][0]);
				AssertContainsExactElementsInAnyOrder("You too for two", System.Array.Empty<A>(), groups[a2][1]);
			});
			CombineAssertions("Split group", () =>
			{
				var g1 = new[] { a2, a3, a4 };
				var g2 = new[] { a11, a12 };
				var groups = new AEqualityComparer().GroupMerge(g1, g2);
				AssertEquals("A1 is not distinct.", groups.Count, 4);
				AssertEquals("Both groups live.", 2, groups[a11].Count);
				AssertEquals(2, groups.Depth);
				AssertContainsExactElementsInAnyOrder("First collection is grouped", System.Array.Empty<A>(), groups[a11][0]);
				AssertContainsExactElementsInAnyOrder("Second collection is grouped", new[] { a11, a12 }, groups[a11][1]);
				AssertContainsExactElementsInAnyOrder("You too for one", new[] { a2 }, groups[a2][0]);
				AssertContainsExactElementsInAnyOrder("You too for two", System.Array.Empty<A>(), groups[a2][1]);
			});
		}

		public void TestIncrementalGroupMerge()
		{
			var a11 = new A { I = 1 };
			var a12 = new A { I = 1 };
			var a2 = new A { I = 2 };
			var a3 = new A { I = 3 };
			var a4 = new A { I = 4 };
			CombineAssertions("Covering and subgroup", () =>
			{
				var g1 = new[] { a11, a12, a2, a3, a4 };
				var g2 = new[] { a11, a12 };
				var groups = new AEqualityComparer().GroupMerge(g1);
				AssertEquals(1, groups.Depth);
				groups.GroupMerge(g2);
				AssertEquals(2, groups.Depth);
				AssertEquals("A1 is not distinct.", groups.Count, 4);
				AssertEquals("Both groups live.", 2, groups[a11].Count);
				AssertContainsExactElementsInAnyOrder("First collection is grouped", new[] { a11, a12 }, groups[a11][0]);
				AssertContainsExactElementsInAnyOrder("Second collection is grouped", new[] { a11, a12 }, groups[a11][1]);
				AssertContainsExactElementsInAnyOrder("You too for one", new[] { a2 }, groups[a2][0]);
				AssertContainsExactElementsInAnyOrder("You too for two", System.Array.Empty<A>(), groups[a2][1]);
			});
			CombineAssertions("Split group", () =>
			{
				var g1 = new[] { a2, a3, a4 };
				var g2 = new[] { a11, a12 };
				var groups = new AEqualityComparer().GroupMerge(g1);
				AssertEquals(1, groups.Depth);
				groups.GroupMerge(g2);
				AssertEquals(2, groups.Depth);
				AssertEquals("A1 is not distinct.", groups.Count, 4);
				AssertEquals("Both groups live.", 2, groups[a11].Count);
				AssertContainsExactElementsInAnyOrder("First collection is grouped", System.Array.Empty<A>(), groups[a11][0]);
				AssertContainsExactElementsInAnyOrder("Second collection is grouped", new[] { a11, a12 }, groups[a11][1]);
				AssertContainsExactElementsInAnyOrder("You too for one", new[] { a2 }, groups[a2][0]);
				AssertContainsExactElementsInAnyOrder("You too for two", System.Array.Empty<A>(), groups[a2][1]);
			});
		}

		public void TestMergeInto()
		{
			var a1 = new A { I = 1 };
			var a2 = new A { I = 1 };
			var a3 = new A { I = 1 };
			var group = new AEqualityComparer().GroupMerge(new[] { a1 });
			group.MergeInto(0, new[] { a2 });
			AssertContainsExactElementsInAnyOrder("First collection is grouped", new A[] { a1, a2 }, group[a1][0]);
		}
	}
}
