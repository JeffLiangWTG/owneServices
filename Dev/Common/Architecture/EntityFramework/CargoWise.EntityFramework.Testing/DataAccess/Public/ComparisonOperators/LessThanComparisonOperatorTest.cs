using System.Linq;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing.DataAccess
{
	sealed class LessThanComparisonOperatorTest : TestCase
	{
		public void TestValuesAreCorrect()
		{
			LessThanComparisonOperator obj = new LessThanComparisonOperator();
			AssertEquals("<", obj.comparisonText);
			Assert(!obj.SupportsNullComparison);
		}

		public void TestPredicate()
		{
			AssertArrayEqualsByElements(
				new int[] { 1, 2 },
				new int[] { 1, 2, 3, 4, 5 }
					.Where(new LessThanComparisonOperator().GetPredicate(3)).ToArray());
		}
	}
}
