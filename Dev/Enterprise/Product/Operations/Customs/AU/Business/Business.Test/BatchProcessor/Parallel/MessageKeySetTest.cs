using System.Linq;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class MessageKeySetTest : TestCase
	{
		public void TestMaxLength()
		{
			string s1 = new string('1', LockMechanismConstants.MaxKeyLength);
			string s2 = new string('2', LockMechanismConstants.MaxKeyLength);
			string s1L = new string('1', LockMechanismConstants.MaxKeyLength + 1);
			string s2L = new string('2', LockMechanismConstants.MaxKeyLength + 1);

			var keySet1 = new MessageKeySet(new[] { s1L, s2L });
			var keySet2 = new MessageKeySet(new[] { s1L }, new[] { s2L });

			AssertContainsExactElementsInAnyOrder(new[] { s1, s2 }, keySet1.DependsOn.ToArray());
			AssertContainsExactElementsInAnyOrder(new[] { s1, s2 }, keySet1.Affects.ToArray());
			AssertContainsExactElementsInAnyOrder(new[] { s1 }, keySet2.DependsOn.ToArray());
			AssertContainsExactElementsInAnyOrder(new[] { s2 }, keySet2.Affects.ToArray());
		}

		public void TestNull()
		{
			var keySet1 = new MessageKeySet(new[] { "1", null, "2" });
			var keySet2 = new MessageKeySet(new[] { "1", null, "2" }, new[] { "3", null, "4" });
			AssertContainsExactElementsInAnyOrder(new[] { "1", "", "2" }, keySet1.DependsOn.ToArray());
			AssertContainsExactElementsInAnyOrder(new[] { "1", "", "2" }, keySet1.Affects.ToArray());
			AssertContainsExactElementsInAnyOrder(new[] { "1", "", "2" }, keySet2.DependsOn.ToArray());
			AssertContainsExactElementsInAnyOrder(new[] { "3", "", "4" }, keySet2.Affects.ToArray());
		}
	}
}
