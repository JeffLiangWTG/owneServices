using System.Linq;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing.DataAccess
{
	sealed class GreaterThanComparisonOperatorTest : TestCase
	{
		public void TestValuesAreCorrect()
		{
			GreaterThanComparisonOperator obj = new GreaterThanComparisonOperator();
			AssertEquals(">", obj.comparisonText);
			Assert(!obj.SupportsNullComparison);
		}

		public void TestPredicate()
		{
			AssertArrayEqualsByElements(
				new int[] { 4, 5 },
				new int[] { 1, 2, 3, 4, 5 }
					.Where(new GreaterThanComparisonOperator().GetPredicate(3)).ToArray());
		}
	}
}
