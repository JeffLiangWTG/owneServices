using System;
using System.Data;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	public class DateQueryBuilderTest : TestCase
	{
		public void TestCreateDateRange_NullableDateTime()
		{
			var nullableDateColumn = new SchemaDateTimeColumn(DummyBizoSchema.Instance, "Zayden", 0, SqlDbType.DateTime, null, true);

			AssertCreateDateRange(DateComparisonOperator.HasDateInRange, nullableDateColumn, new ZDate(2008, 12, 3), new ZDate(2009, 12, 3), "Zayden >= #2008-12-03 00:00:00.000# and Zayden < #2009-12-04 00:00:00.000#", false, false);
			AssertCreateDateRange(DateComparisonOperator.HasNoDateEntered, nullableDateColumn, new ZDate(2008, 12, 3), new ZDate(2009, 12, 3), "Zayden is null", false, false);
			AssertCreateDateRange(DateComparisonOperator.HasDateEntered, nullableDateColumn, new ZDate(2008, 12, 3), new ZDate(2009, 12, 3), "Zayden is not null", false, false);
		}

		public void TestCreateDateRange_NotNullableDateTime()
		{
			var notNullableDateColumn = new SchemaDateTimeColumn(DummyBizoSchema.Instance, "Lola", 0, SqlDbType.DateTime, ZDateTime.Now, false);

			AssertCreateDateRange(DateComparisonOperator.HasDateInRange, notNullableDateColumn, new ZDate(2008, 12, 3), new ZDate(2009, 12, 3), "Lola >= #2008-12-03 00:00:00.000# and Lola < #2009-12-04 00:00:00.000#", false, false);
			AssertCreateDateRange(DateComparisonOperator.HasNoDateEntered, notNullableDateColumn, new ZDate(2008, 12, 3), new ZDate(2009, 12, 3), "", false, true);
			AssertCreateDateRange(DateComparisonOperator.HasDateEntered, notNullableDateColumn, new ZDate(2008, 12, 3), new ZDate(2009, 12, 3), "", true, false);
		}

		public void TestCreateDateTimeRange_NullableDateTime()
		{
			var nullableDateColumn = new SchemaDateTimeColumn(DummyBizoSchema.Instance, "Zayden", 0, SqlDbType.DateTime, null, true);

			AssertCreateDateTimeRange(DateComparisonOperator.HasDateInRange, nullableDateColumn, new ZDateTime(2008, 12, 3, 9, 32, 44), new ZDateTime(2009, 12, 3, 11, 14, 16), "Zayden >= #2008-12-03 09:32:44.000# and Zayden <= #2009-12-03 11:14:16.000#", false, false);
			AssertCreateDateTimeRange(DateComparisonOperator.HasNoDateEntered, nullableDateColumn, new ZDateTime(2008, 12, 3, 9, 32, 44), new ZDateTime(2009, 12, 3, 11, 14, 16), "Zayden is null", false, false);
			AssertCreateDateTimeRange(DateComparisonOperator.HasDateEntered, nullableDateColumn, new ZDateTime(2008, 12, 3, 9, 32, 44), new ZDateTime(2009, 12, 3, 11, 14, 16), "Zayden is not null", false, false);
		}

		public void TestCreateDateTimeRange_NotNullableDateTime()
		{
			var notNullableDateColumn = new SchemaDateTimeColumn(DummyBizoSchema.Instance, "Lola", 0, SqlDbType.DateTime, ZDateTime.Now, false);

			AssertCreateDateTimeRange(DateComparisonOperator.HasDateInRange, notNullableDateColumn, new ZDateTime(2008, 12, 3, 9, 32, 44), new ZDateTime(2009, 12, 3, 11, 14, 16), "Lola >= #2008-12-03 09:32:44.000# and Lola <= #2009-12-03 11:14:16.000#", false, false);
			AssertCreateDateTimeRange(DateComparisonOperator.HasNoDateEntered, notNullableDateColumn, new ZDateTime(2008, 12, 3, 9, 32, 44), new ZDateTime(2009, 12, 3, 11, 14, 16), "", false, true);
			AssertCreateDateTimeRange(DateComparisonOperator.HasDateEntered, notNullableDateColumn, new ZDateTime(2008, 12, 3, 9, 32, 44), new ZDateTime(2009, 12, 3, 11, 14, 16), "", true, false);
		}

		public void TestCreateDateTimeOffsetRange_NullableDateTimeOffset()
		{
			var nullableDateTimeOffsetColumn = new SchemaDateTimeOffsetColumn(DummyBizoSchema.Instance, "Zayden", 0, SqlDbType.DateTimeOffset, null, true, 7);

			AssertCreateDateTimeOffsetRange(DateComparisonOperator.HasDateInRange, nullableDateTimeOffsetColumn, new ZDateTimeOffset(2008, 12, 3, 9, 32, 44, TimeSpan.FromHours(10)), new ZDateTimeOffset(2009, 12, 3, 11, 14, 16, TimeSpan.FromHours(-10)), "CONVERT(CONVERT(Zayden, System.String), System.DateTime) >= #2008-12-03 09:32:44.0000000 +10:00# and CONVERT(CONVERT(Zayden, System.String), System.DateTime) <= #2009-12-03 11:14:16.0000000 -10:00#", false, false);
			AssertCreateDateTimeOffsetRange(DateComparisonOperator.HasNoDateEntered, nullableDateTimeOffsetColumn, new ZDateTimeOffset(2008, 12, 3, 9, 32, 44, TimeSpan.FromHours(10)), new ZDateTimeOffset(2009, 12, 3, 11, 14, 16, TimeSpan.FromHours(-10)), "CONVERT(CONVERT(Zayden, System.String), System.DateTime) is null", false, false);
			AssertCreateDateTimeOffsetRange(DateComparisonOperator.HasDateEntered, nullableDateTimeOffsetColumn, new ZDateTimeOffset(2008, 12, 3, 9, 32, 44, TimeSpan.FromHours(10)), new ZDateTimeOffset(2009, 12, 3, 11, 14, 16, TimeSpan.FromHours(-10)), "CONVERT(CONVERT(Zayden, System.String), System.DateTime) is not null", false, false);
		}

		public void TestCreateDateTimeOffsetRange_NotNullableDateTimeOffset()
		{
			var notNullableDateTimeOffsetColumn = new SchemaDateTimeOffsetColumn(DummyBizoSchema.Instance, "Lola", 0, SqlDbType.DateTimeOffset, ZDateTimeOffset.Now, false, 7);

			AssertCreateDateTimeOffsetRange(DateComparisonOperator.HasDateInRange, notNullableDateTimeOffsetColumn, new ZDateTimeOffset(2008, 12, 3, 9, 32, 44, TimeSpan.FromHours(10)), new ZDateTimeOffset(2009, 12, 3, 11, 14, 16, TimeSpan.FromHours(-10)), "CONVERT(CONVERT(Lola, System.String), System.DateTime) >= #2008-12-03 09:32:44.0000000 +10:00# and CONVERT(CONVERT(Lola, System.String), System.DateTime) <= #2009-12-03 11:14:16.0000000 -10:00#", false, false);
			AssertCreateDateTimeOffsetRange(DateComparisonOperator.HasNoDateEntered, notNullableDateTimeOffsetColumn, new ZDateTimeOffset(2008, 12, 3, 9, 32, 44, TimeSpan.FromHours(10)), new ZDateTimeOffset(2009, 12, 3, 11, 14, 16, TimeSpan.FromHours(-10)), "", false, true);
			AssertCreateDateTimeOffsetRange(DateComparisonOperator.HasDateEntered, notNullableDateTimeOffsetColumn, new ZDateTimeOffset(2008, 12, 3, 9, 32, 44, TimeSpan.FromHours(10)), new ZDateTimeOffset(2009, 12, 3, 11, 14, 16, TimeSpan.FromHours(-10)), "", true, false);
		}

		[TestUtcOffset(10, 0, 0)]
		public void TestCreateDateTimeRange_ConvertFromLocalToUtc()
		{
			var localToUtcBuilder = new DateQueryBuilder(true);
			var column = new SchemaDateTimeColumn(DummyBizoSchema.Instance, "Zayden", 0, SqlDbType.DateTime, null, true);

			var query = localToUtcBuilder.CreateDateTimeRange(DateComparisonOperator.HasDateInRange, column, new ZDateTime(2008, 12, 3, 9, 32, 44), new ZDateTime(2009, 12, 3, 11, 14, 16), false, false);
			AssertEquals("Zayden >= #2008-12-02 23:32:44.000# and Zayden <= #2009-12-03 01:14:16.000#", query.LiteralTextADO);
			AssertEquals(false, query.IsEmpty);
			AssertEquals(false, query.IsNoResultQuery);

			query = localToUtcBuilder.CreateDateTimeRange(DateComparisonOperator.HasNoDateEntered, column, new ZDateTime(2008, 12, 3, 9, 32, 44), new ZDateTime(2009, 12, 3, 11, 14, 16), false, false);
			AssertEquals("Zayden is null", query.LiteralTextADO);
			AssertEquals(false, query.IsEmpty);
			AssertEquals(false, query.IsNoResultQuery);

			query = localToUtcBuilder.CreateDateTimeRange(DateComparisonOperator.HasDateEntered, column, new ZDateTime(2008, 12, 3, 9, 32, 44), new ZDateTime(2009, 12, 3, 11, 14, 16), false, false);
			AssertEquals("Zayden is not null", query.LiteralTextADO);
			AssertEquals(false, query.IsEmpty);
			AssertEquals(false, query.IsNoResultQuery);
		}

		#region Implementation

		void AssertCreateDateRange(DateComparisonOperator comparisonOperator, SchemaDateTimeColumn dateTimeColumn, ZDate fromDate, ZDate toDate, string expectedQuery, bool expectedEmpty, bool expectedNoResult)
		{
			var query = new DateQueryBuilder().CreateDateRange(comparisonOperator, dateTimeColumn, fromDate, toDate);
			AssertEquals(expectedQuery, query.LiteralTextADO);
			AssertEquals(expectedEmpty, query.IsEmpty);
			AssertEquals(expectedNoResult, query.IsNoResultQuery);
		}

		void AssertCreateDateTimeRange(DateComparisonOperator comparisonOperator, SchemaDateTimeColumn dateTimeColumn, ZDateTime fromDate, ZDateTime toDate, string expectedQuery, bool expectedEmpty, bool expectedNoResult)
		{
			var query = new DateQueryBuilder().CreateDateTimeRange(comparisonOperator, dateTimeColumn, fromDate, toDate, false, false);
			AssertEquals(expectedQuery, query.LiteralTextADO);
			AssertEquals(expectedEmpty, query.IsEmpty);
			AssertEquals(expectedNoResult, query.IsNoResultQuery);
		}

		void AssertCreateDateTimeOffsetRange(DateComparisonOperator comparisonOperator, SchemaDateTimeOffsetColumn dateTimeOffsetColumn, ZDateTimeOffset fromDate, ZDateTimeOffset toDate, string expectedQuery, bool expectedEmpty, bool expectedNoResult)
		{
			var query = new DateQueryBuilder().CreateDateTimeOffsetRange(comparisonOperator, dateTimeOffsetColumn, fromDate, toDate, false, false);
			AssertEquals(expectedQuery, query.LiteralTextADO);
			AssertEquals(expectedEmpty, query.IsEmpty);
			AssertEquals(expectedNoResult, query.IsNoResultQuery);
		}

		#endregion
	}
}
