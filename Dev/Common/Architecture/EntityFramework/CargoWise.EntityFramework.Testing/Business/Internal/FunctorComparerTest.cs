using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	sealed class FunctorComparerTest : TestCase
	{
		public void TestCompare()
		{
			FunctorComparer<string> comparer = new FunctorComparer<string>(delegate(string x, string y)
			{ return int.Parse(x).CompareTo(int.Parse(y)); });
			Assert(comparer.Compare("20", "100") < 0);
			Assert(comparer.Compare("1", "01") == 0);
			Assert(comparer.Compare("100", "20") > 0);
		}
	}
}
