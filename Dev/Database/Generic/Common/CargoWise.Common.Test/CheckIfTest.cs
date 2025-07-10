using NUnit.Framework;

namespace CargoWise.Common
{
	class CheckIfTest : TestCase
	{
		public void TestAnyElementIsNull()
		{
			object o1 = new object();
			AssertEquals(false, Checks.AnyElement.IsNull(o1));
			object o2 = new object();
			AssertEquals(false, Checks.AnyElement.IsNull(o1, o2));
			object o3 = null;
			AssertEquals(true, Checks.AnyElement.IsNull(o1, o2, o3));
			object o4 = new object();
			AssertEquals(true, Checks.AnyElement.IsNull(o1, o2, o3, o4));
		}

		public void TestAnyElementIsNotNull()
		{
			object o1 = null;
			AssertEquals(false, Checks.AnyElement.IsNotNull(o1));
			object o2 = null;
			AssertEquals(false, Checks.AnyElement.IsNotNull(o1, o2));
			object o3 = new object();
			AssertEquals(true, Checks.AnyElement.IsNotNull(o1, o2, o3));
			object o4 = null;
			AssertEquals(true, Checks.AnyElement.IsNotNull(o1, o2, o3, o4));
		}

		public void TestAnyElementMatchesPredicate()
		{
			var a1 = "a";
			AssertEquals(false, Checks.AnyElement.MatchesPredicate(e => e.Length > 2, a1));
			var a2 = "aa";
			AssertEquals(false, Checks.AnyElement.MatchesPredicate(e => e.Length > 2, a1, a2));
			var a3 = "aaa";
			AssertEquals(true, Checks.AnyElement.MatchesPredicate(e => e.Length > 2, a1, a2, a3));
			var a4 = "aaaa";
			AssertEquals(true, Checks.AnyElement.MatchesPredicate(e => e.Length > 2, a1, a2, a3, a4));
		}
	}
}