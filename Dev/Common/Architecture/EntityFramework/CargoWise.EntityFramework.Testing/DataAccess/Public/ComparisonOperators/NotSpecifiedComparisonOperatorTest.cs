using System;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing.DataAccess
{
	sealed class NotSpecifiedComparisonOperatorTest : TestCase
	{
		public void TestValuesAreCorrect()
		{
			NotSpecifiedComparisonOperator obj = new NotSpecifiedComparisonOperator();
			AssertEquals(String.Empty, obj.comparisonText);
			Assert(obj.SupportsNullComparison);
		}
	}
}
