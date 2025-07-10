using System.Linq;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing.DataAccess
{
	sealed class GreaterThanOrEqualToComparisonOperatorTest : TestCase
	{
		public void TestValuesAreCorrect()
		{
			GreaterThanOrEqualToComparisonOperator obj = new GreaterThanOrEqualToComparisonOperator();
			AssertEquals(">=", obj.comparisonText);
			Assert(!obj.SupportsNullComparison);
		}

		public void TestPredicate()
		{
			AssertArrayEqualsByElements(
				new int[] { 3, 4, 5 },
				new int[] { 1, 2, 3, 4, 5 }
					.Where(new GreaterThanOrEqualToComparisonOperator().GetPredicate(3)).ToArray());
		}
	}
}
