using System;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing.DataAccess
{
	sealed class LikeComparisonOperatorTest : TestCase
	{
		public void TestValuesAreCorrect()
		{
			LikeComparisonOperator comparisonOperator = new LikeComparisonOperator();
			AssertEquals("like", comparisonOperator.comparisonText);
			AssertEquals("", comparisonOperator.prefix);
			AssertEquals("", comparisonOperator.suffix);
		}

		public void TestLiteralProcessedValues()
		{
			LikeComparisonOperator comparisonOperator = new LikeComparisonOperator();
			var processedValue1 = comparisonOperator.ValueForLiteralADO(String.Format("TST[*'%"));

			AssertEquals("TST[[][*]'%", processedValue1);
		}
	}
}
