using System;
using CargoWise.Types;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing.DataAccess
{
	sealed class GreaterThanOrEqualToDatePartComparisonOperatorTest : TestCase
	{
		public void TestAdjustedValue()
		{
			SQLComparisonOperator comparisonOperator = new GreaterThanOrEqualToDatePartComparisonOperator();
			AssertEquals(ZDateTime.Empty, comparisonOperator.ValueForLiteralADO(ZDateTime.Empty));
			AssertEquals(ZDateTimeOffset.Empty, comparisonOperator.ValueForLiteralADO(ZDateTimeOffset.Empty));
			AssertEquals("X", comparisonOperator.ValueForLiteralADO("X"));
			AssertEquals(new ZDateTime(2005, 1, 1), comparisonOperator.ValueForLiteralADO(new ZDateTime(2005, 1, 1, 12, 12, 12)));
			AssertEquals(new DateTime(2005, 1, 1), comparisonOperator.ValueForLiteralADO(new DateTime(2005, 1, 1, 12, 12, 12)));
			AssertEquals(new ZDateTimeOffset(2005, 1, 1, 0, 0, 0, TimeSpan.FromHours(11)), comparisonOperator.ValueForLiteralADO(new ZDateTimeOffset(2005, 1, 1, 12, 12, 12, TimeSpan.FromHours(11))));
			AssertEquals(new DateTimeOffset(2005, 1, 1, 0, 0, 0, TimeSpan.FromHours(11)), comparisonOperator.ValueForLiteralADO(new DateTimeOffset(2005, 1, 1, 12, 12, 12, TimeSpan.FromHours(11))));
			AssertEquals(new ZDate(2005, 1, 1), comparisonOperator.ValueForLiteralADO(new ZDate(2005, 1, 1)));
		}
	}
}
