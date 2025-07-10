using System.Linq;
using NUnit.Framework;

namespace CargoWise.Common.Enumeration.Testing
{
	public class ZLinqTest : TestCase
	{
		public void TestIterate()
		{
			var numbers = ZEnumerable.Iterate(0, i => ++i).Take(5).ToArray();
			AssertArrayEqualsByElements(new[] { 0, 1, 2, 3, 4 }, numbers);
		}

		public void TestIterateWithStoppingPoint()
		{
			var numbers = ZEnumerable.Iterate(0, i => ++i, 5).Take(10).ToArray();
			AssertArrayEqualsByElements(new[] { 0, 1, 2, 3, 4 }, numbers);
		}

		public void TestIterateUntil()
		{
			var numbers = ZEnumerable.IterateUntil(0, i => ++i, i => i == 5).Take(10).ToArray();
			AssertArrayEqualsByElements(new[] { 0, 1, 2, 3, 4 }, numbers);
		}
	}
}