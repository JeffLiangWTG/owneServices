using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace CargoWise.Common.Collections
{
	public class ICollectionExtensionsTest : TestCase
	{
		public void TestAsReadOnly()
		{
			var list = new List<int> { 0, 1, 2, 3 };
			var readOnlyList = list.AsReadOnly(i => i * 2);
			AssertEquals(list.Count, readOnlyList.Count);
			var expected = new[] { 0, 2, 4, 6 };
			AssertArrayEqualsByElements(expected, readOnlyList.ToArray());
		}
	}
}