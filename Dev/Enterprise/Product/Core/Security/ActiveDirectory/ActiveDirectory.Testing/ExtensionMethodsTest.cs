using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Enterprise.Security.ActiveDirectory.Test
{
	class ExtensionMethodsTest : TestCase
	{
		public void TestIndexOfFirst()
		{
			var items = new[] { 1, 2, 3, 4, 5 };
			AssertEquals(2, items.IndexOfFirst(i => i == 3));
			AssertEquals(-1, items.IndexOfFirst(i => i == 100));
		}

		public void TestIndexOfFirstFromStartIndex()
		{
			var items = new[] { 5, 5, 3, 4, 5 };
			AssertEquals(4, items.IndexOfFirst(2, i => i == 5));
			AssertEquals(-1, items.IndexOfFirst(3, i => i == 3));
		}

		public void TestForEach()
		{
			var items = new[] { 1, 2, 3, 4, 5 };
			var visited = new List<int>();
			items.ForEach(i => visited.Add(i));
			Assert(items.SequenceEqual(visited));
		}
	}
}
