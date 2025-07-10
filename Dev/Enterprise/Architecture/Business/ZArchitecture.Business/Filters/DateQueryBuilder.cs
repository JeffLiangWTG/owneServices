using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.Business
{
	public sealed class DateQueryBuilder
	{
		public DateQueryBuilder(bool convertFromLocalToUtc = false)
		{
			ConvertFromLocalToUtc = convertFromLocalToUtc;
		}

		public bool ConvertFromLocalToUtc { get; set; }

		public ZQuery CreateDateRange(DateComparisonOperator comparisonOperator, SchemaDateTimeColumn column, ZDate lowerEmptyOk, ZDate upperEmptyOk)
		{
			return CreateFilterCore(comparisonOperator, column, lowerEmptyOk, upperEmptyOk, true, true);
		}

		public ZQuery CreateDateTimeRange(DateComparisonOperator comparisonOperator, SchemaDateTimeColumn column, ZDateTime lowerEmptyOk, ZDateTime upperEmptyOk, bool lowerDatePartOnly, bool upperDatePartOnly)
		{
			var lowerDate = ConvertFromLocalToUtc && lowerEmptyOk.IsValid ? TimeFactory.Instance.GetUtcFromLocalTime(lowerEmptyOk.ToDateTime()) : lowerEmptyOk;
			var upperDate = ConvertFromLocalToUtc && upperEmptyOk.IsValid ? TimeFactory.Instance.GetUtcFromLocalTime(upperEmptyOk.ToDateTime()) : upperEmptyOk;
			return CreateFilterCore(comparisonOperator, column, lowerDate, upperDate, lowerDatePartOnly, upperDatePartOnly);
		}

		public ZQuery CreateDateTimeOffsetRange(DateComparisonOperator comparisonOperator, SchemaDateTimeColumn column, ZDateTimeOffset lowerEmptyOk, ZDateTimeOffset upperEmptyOk, bool lowerDatePartOnly, bool upperDatePartOnly)
		{
			return CreateFilterCore(comparisonOperator, column, lowerEmptyOk, upperEmptyOk, lowerDatePartOnly, upperDatePartOnly);
		}

		public ZQuery CreateDateTimeOffsetRange(DateComparisonOperator comparisonOperator, SchemaDateTimeOffsetColumn column, ZDateTimeOffset lowerEmptyOk, ZDateTimeOffset upperEmptyOk, bool lowerDatePartOnly, bool upperDatePartOnly)
		{
			return CreateFilterCore(comparisonOperator, column, lowerEmptyOk, upperEmptyOk, lowerDatePartOnly, upperDatePartOnly);
		}

		ZQuery CreateFilterCore(DateComparisonOperator comparisonOperator, SchemaColumn column, IZType lowerEmptyOk, IZType upperEmptyOk, bool lowerDatePartOnly, bool upperDatePartOnly)
		{
			var dateQuery = new ZQuery();

			if (comparisonOperator == DateComparisonOperator.HasNoDateEntered)
			{
				if (column.IsNullable)
				{
					dateQuery.AddToFilter(column, SQLComparisonOperator.Equal, null);
				}
				else
				{
					dateQuery.IsNoResultQuery = true;
				}
			}
			else if (comparisonOperator == DateComparisonOperator.HasDateEntered)
			{
				if (column.IsNullable)
				{
					dateQuery.AddToFilter(column, SQLComparisonOperator.NotEqual, null);
				}
			}
			else
			{
				if (lowerEmptyOk.IsValid)
				{
					dateQuery.AddToFilter(column, (lowerDatePartOnly ? SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly : SQLComparisonOperator.GreaterThanOrEqualTo), lowerEmptyOk);
				}

				if (upperEmptyOk.IsValid)
				{
					dateQuery.AddToFilter(column, (upperDatePartOnly ? SQLComparisonOperator.LessThanOrEqualToDatePartOnly : SQLComparisonOperator.LessThanOrEqualTo), upperEmptyOk);
				}
			}

			return dateQuery;
		}
	}
}
