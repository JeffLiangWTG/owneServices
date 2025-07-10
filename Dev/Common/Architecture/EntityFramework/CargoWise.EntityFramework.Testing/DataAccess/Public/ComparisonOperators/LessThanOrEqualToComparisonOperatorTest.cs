using System.Linq;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing.DataAccess
{
	sealed class LessThanOrEqualToComparisonOperatorTest : TestCase
	{
		public void TestValuesAreCorrect()
		{
			LessThanOrEqualToComparisonOperator obj = new LessThanOrEqualToComparisonOperator();
			AssertEquals("<=", obj.comparisonText);
			Assert(!obj.SupportsNullComparison);
		}

		public void TestPredicate()
		{
			AssertArrayEqualsByElements(
				new int[] { 1, 2, 3 },
				new int[] { 1, 2, 3, 4, 5 }
					.Where(new LessThanOrEqualToComparisonOperator().GetPredicate(3)).ToArray());
		}
	}
}
