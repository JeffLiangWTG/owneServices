using System;
using CargoWise.Common.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing.DataAccess
{
	sealed class LessThanOrEqualToDatePartComparisonOperatorTest : TestCase
	{
		public void TestMaxDateEscapedSqlValue()
		{
			SQLComparisonOperator comparisonOperator = new LessThanOrEqualToDatePartComparisonOperator();
			AssertEquals(maxSqlDateTimeValue, comparisonOperator.EscapedSqlValue(maxSqlDateTimeValue.Date));
		}

		public void TestAdjustedValue()
		{
			SQLComparisonOperator comparisonOperator = new LessThanOrEqualToDatePartComparisonOperator();
			AssertEquals(ZDateTime.Empty, comparisonOperator.ValueForLiteralADO(ZDateTime.Empty));
			AssertEquals(ZDateTimeOffset.Empty, comparisonOperator.ValueForLiteralADO(ZDateTimeOffset.Empty));
			AssertEquals("X", comparisonOperator.ValueForLiteralADO("X"));
			AssertEquals(new ZDateTime(2005, 1, 2), comparisonOperator.ValueForLiteralADO(new ZDateTime(2005, 1, 1, 12, 12, 12)));
			AssertEquals(new DateTime(2005, 1, 2), comparisonOperator.ValueForLiteralADO(new DateTime(2005, 1, 1, 12, 12, 12)));
			AssertEquals(new ZDate(2005, 1, 2), comparisonOperator.ValueForLiteralADO(new ZDate(2005, 1, 1)));
			AssertEquals(new ZDateTimeOffset(2005, 1, 2, 0, 0, 0, TimeSpan.FromHours(11)), comparisonOperator.ValueForLiteralADO(new ZDateTimeOffset(2005, 1, 1, 12, 12, 12, TimeSpan.FromHours(11))));
			AssertEquals(new DateTimeOffset(2005, 1, 2, 0, 0, 0, TimeSpan.FromHours(-11)), comparisonOperator.ValueForLiteralADO(new DateTimeOffset(2005, 1, 1, 12, 12, 12, TimeSpan.FromHours(-11))));
			AssertEquals(maxSqlDateTimeValue, comparisonOperator.ValueForLiteralADO(DateTime.MaxValue));
			AssertEquals(new ZDateTime(maxSqlDateTimeValue), comparisonOperator.ValueForLiteralADO(DateTime.MaxValue));
			AssertEquals(maxSqlDateTimeValue, comparisonOperator.ValueForLiteralADO(DateTime.MaxValue.AddDays(-0.99)));
			AssertEquals(new ZDateTime(maxSqlDateTimeValue), comparisonOperator.ValueForLiteralADO(DateTime.MaxValue.AddDays(-0.99)));
			AssertEquals(new DateTimeOffset(maxSqlDateTimeValue, TimeSpan.Zero), comparisonOperator.ValueForLiteralADO(DateTimeOffset.MaxValue));
			AssertEquals(new ZDateTimeOffset(maxSqlDateTimeValue, TimeSpan.Zero), comparisonOperator.ValueForLiteralADO(DateTimeOffset.MaxValue));
			AssertEquals(new DateTimeOffset(maxSqlDateTimeValue, TimeSpan.Zero), comparisonOperator.ValueForLiteralADO(DateTimeOffset.MaxValue.AddDays(-0.99)));
			AssertEquals(new ZDateTimeOffset(maxSqlDateTimeValue, TimeSpan.Zero), comparisonOperator.ValueForLiteralADO(DateTimeOffset.MaxValue.AddDays(-0.99)));
			AssertEquals(new DateTimeOffset(maxSqlDateTimeValue, TimeSpan.Zero), comparisonOperator.ValueForLiteralADO(new DateTimeOffset(DateTimeOffset.MaxValue.AddDays(-0.99).DateTime, TimeSpan.FromHours(11))));
			AssertEquals(new ZDateTimeOffset(maxSqlDateTimeValue, TimeSpan.Zero), comparisonOperator.ValueForLiteralADO(new DateTimeOffset(DateTimeOffset.MaxValue.AddDays(-0.99).DateTime, TimeSpan.FromHours(11))));
			AssertEquals(new DateTimeOffset(maxSqlDateTimeValue, TimeSpan.Zero), comparisonOperator.ValueForLiteralADO(new DateTimeOffset(DateTimeOffset.MaxValue.AddDays(-0.99).DateTime, TimeSpan.FromHours(-11))));
			AssertEquals(new ZDateTimeOffset(maxSqlDateTimeValue, TimeSpan.Zero), comparisonOperator.ValueForLiteralADO(new DateTimeOffset(DateTimeOffset.MaxValue.AddDays(-0.99).DateTime, TimeSpan.FromHours(-11))));
		}

		[SuppressThreadStaticFieldMessage]
		static readonly DateTime maxSqlDateTimeValue = new DateTime(9999, 12, 31, 23, 59, 59, 990);
	}
}
