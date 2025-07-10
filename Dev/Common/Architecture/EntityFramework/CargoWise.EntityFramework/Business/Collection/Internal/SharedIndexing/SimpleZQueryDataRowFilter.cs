using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Schema;
using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	/// <summary>
	/// ALERT! This class can only generate filters for simple ZQueries with no OR conditions.
	/// </summary>
	public class SimpleZQueryDataRowFilter
	{
		public SimpleZQueryDataRowFilter(ZQuery filter)
		{
			filters = new List<Func<DataRow, bool>>();
			foreach (var subFilter in filter.FilterParts)
			{
				switch (subFilter)
				{
					case ZSQLInFilter inFilter:
						filters.Add(AddInFilterOp(inFilter));
						break;
					case JoinCondition j:
						if (j == JoinCondition.Or)
						{
							throw new InvalidOperationException("No support for OR yet.");
						}
						break;
					case ZSqlParameter parameterComparer:
						filters.Add(AddParameterOp(parameterComparer));
						break;
					default:
						throw new InvalidOperationException(FormattableString.Invariant($"Unknown filter part {subFilter} of type {subFilter.GetType().FullName}"));
				}
			}
		}

		readonly List<Func<DataRow, bool>> filters;

		public bool IsMatch(DataRow row)
		{
			foreach (var filter in filters)
			{
				if (!filter(row))
				{
					return false;
				}
			}

			return true;
		}

		static Func<DataRow, bool> AddInFilterOp(ZSQLInFilter inFilter)
		{
			var col = inFilter.Column;
			var comparisonOperator = inFilter.ComparisonOperator;
			var argCount = inFilter.GetValues().Take(2).Count();
			if (comparisonOperator == SQLComparisonOperator.Equal)
			{
				return MakeHashInFilter(inFilter, col, argCount);
			}
			else if (SQLComparisonOperator.NotEqual == comparisonOperator)
			{
				return Invert(MakeHashInFilter(inFilter, col, argCount));
			}
			else if (SQLComparisonOperator.StartsWith == comparisonOperator)
			{
				return MakeInFilter(inFilter, col, argCount, true, StartsWith);
			}
			else if (SQLComparisonOperator.EndsWith == comparisonOperator)
			{
				return MakeInFilter(inFilter, col, argCount, false, EndsWith);
			}

			throw new InvalidOperationException((FormattableString.Invariant($"Unknown comparsion op {inFilter.ComparisonOperator} for part {inFilter}")));
		}

		static Func<DataRow, bool> MakeHashInFilter(ZSQLInFilter inFilter, SchemaColumn col, int args)
		{
			if (args == 0)
			{
				return _ => false;
			}
			else if (args == 1)
			{
				var val = ZDataType.ObjectToZType(col.GetEquivalentZType(), inFilter.GetValues().Single());
				return r => SimpleZEqualityComparer.AreEqual(GetRowValue(r, col), val);
			}
			else
			{
				var set = new HashSet<IZType>(inFilter.GetValues().Select(s => ZDataType.ObjectToZType(col.GetEquivalentZType(), s)), new SimpleZEqualityComparer());
				return r => set.Contains(GetRowValue(r, col));
			}
		}

		struct SimpleZEqualityComparer : IEqualityComparer<IZType>
		{
			public bool Equals(IZType x, IZType y) => AreEqual(x, y);

			public static bool AreEqual(IZType x, IZType y)
			{
				if (x is ZString)
				{
					return StringComparer.CurrentCultureIgnoreCase.Equals(x, y);
				}
				else
				{
					return x.Equals(y);
				}
			}

			public int GetHashCode(IZType obj)
			{
				if (obj is ZString)
				{
					return StringComparer.CurrentCultureIgnoreCase.GetHashCode(obj);
				}
				else
				{
					return obj.GetHashCode();
				}
			}
		}

		static Func<DataRow, bool> MakeInFilter(ZSQLInFilter inFilter, SchemaColumn col, int args, bool emptyResult, Func<IZType, IZType, bool> compare)
		{
			if (args == 0)
			{
				return _ => emptyResult;
			}
			else if (args == 1)
			{
				var val = ZDataType.ObjectToZType(col.GetEquivalentZType(), inFilter.GetValues().Single());
				return r => compare(GetRowValue(r, col), val);
			}
			else
			{
				var set = inFilter.GetValues().Select(s => ZDataType.ObjectToZType(col.GetEquivalentZType(), s)).ToList();
				return r =>
				{
					var colVal = (ZString)GetRowValue(r, col);
					return set.Any(s => compare(colVal, s));
				};
			}
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "This code is more easily read in this form")]
		static Func<DataRow, bool> AddParameterOp(ZSqlParameter columnComparer)
		{
			var comparisonOperator = columnComparer.ComparisonOperator;
			var col = columnComparer.SchemaColumn;
			var colType = col.GetEquivalentZType();

			if (SQLComparisonOperator.IsBlank == comparisonOperator)
			{
				return RowComparer(col, ZDataType.ObjectToZType(colType, Activator.CreateInstance(colType)), SimpleZEqualityComparer.AreEqual);
			}
			else if (SQLComparisonOperator.IsNotBlank == comparisonOperator)
			{
				return Invert(RowComparer(col, ZDataType.ObjectToZType(colType, Activator.CreateInstance(colType)), SimpleZEqualityComparer.AreEqual));
			}

			var val = ZDataType.ObjectToZType(colType, columnComparer.ValueForPredicate);
			if (SQLComparisonOperator.Equal == comparisonOperator)
			{
				return RowComparer(col, val, SimpleZEqualityComparer.AreEqual);
			}
			else if (SQLComparisonOperator.NotEqual == comparisonOperator)
			{
				return Invert(RowComparer(col, val, SimpleZEqualityComparer.AreEqual));
			}
			else if (SQLComparisonOperator.StartsWith == comparisonOperator)
			{
				return RowComparer(col, val, StartsWith);
			}
			else if (SQLComparisonOperator.DoesNotStartWith == comparisonOperator)
			{
				return Invert(RowComparer(col, val, StartsWith));
			}
			else if (SQLComparisonOperator.EndsWith == comparisonOperator)
			{
				return RowComparer(col, val, EndsWith);
			}
			else if (SQLComparisonOperator.DoesNotEndWith == comparisonOperator)
			{
				return Invert(RowComparer(col, val, EndsWith));
			}
			else if (SQLComparisonOperator.Contains == comparisonOperator)
			{
				return RowComparer(col, val, GenContainsOp);
			}
			else if (SQLComparisonOperator.NotContains == comparisonOperator)
			{
				return Invert(RowComparer(col, val, GenContainsOp));
			}
			else if (SQLComparisonOperator.EqualToDatePartOnly == comparisonOperator)
			{
				var dateVal = ToDateOnlyDatePart(val);
				return r => ToDateOnlyDatePart(GetRowValue(r, col)).Equals(dateVal);
			}
			else if (SQLComparisonOperator.GreaterThan == comparisonOperator)
			{
				return r => GetRowValue(r, col).CompareTo(val) > 0;
			}
			else if (SQLComparisonOperator.GreaterThanOrEqualTo == comparisonOperator)
			{
				return r => GetRowValue(r, col).CompareTo(val) >= 0;
			}
			else if (SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly == comparisonOperator)
			{
				var dateVal = ToDateOnlyDatePart(val);
				return r => ToDateOnlyDatePart(GetRowValue(r, col)) >= dateVal;
			}
			else if (SQLComparisonOperator.LessThan == comparisonOperator)
			{
				return r => GetRowValue(r, col).CompareTo(val) < 0;
			}
			else if (SQLComparisonOperator.LessThanOrEqualTo == comparisonOperator)
			{
				return r => GetRowValue(r, col).CompareTo(val) <= 0;
			}
			else if (SQLComparisonOperator.LessThanOrEqualToDatePartOnly == comparisonOperator)
			{
				var dateVal = ToDateOnlyDatePart(val);
				return r => ToDateOnlyDatePart(GetRowValue(r, col)) <= dateVal;
			}
			else if (SQLComparisonOperator.Like == comparisonOperator)
			{
				var s = (ZString)val;
				var regex = new Regex(Regex.Escape(s).Replace("%", ".*"), RegexOptions.IgnoreCase);
				return r => regex.IsMatch((ZString)GetRowValue(r, col));
			}
			else if (SQLComparisonOperator.NotSpecified == comparisonOperator)
			{
				throw new InvalidOperationException("How could this work?");
			}

			throw new InvalidOperationException((FormattableString.Invariant($"Unknown comparsion op {comparisonOperator} for part {columnComparer}")));
		}

		static bool StartsWith(IZType val1, IZType val2) => ((ZString)val1).StartsWith((ZString)val2, StringComparison.CurrentCultureIgnoreCase);
		static bool EndsWith(IZType val1, IZType val2) => ((ZString)val1).EndsWith((ZString)val2, StringComparison.CurrentCultureIgnoreCase);
		static bool GenContainsOp(IZType val1, IZType val2) => ((ZString)val1).Contains((ZString)val2, StringComparison.CurrentCultureIgnoreCase);
		static Func<DataRow, bool> RowComparer(SchemaColumn column, IZType value, Func<IZType, IZType, bool> func) => r => func(GetRowValue(r, column), value);
		static Func<DataRow, bool> Invert(Func<DataRow, bool> func) => r => !func(r);
		static IZType GetRowValue(DataRow row, SchemaColumn column) => ZDataType.ObjectToZType(column.GetEquivalentZType(), row[column.Name]);

		static ZDateTimeOffset ToDateOnlyDatePart(object o)
		{
			if (o is ZDateTime zDateTime)
			{
				return new ZDateTimeOffset(zDateTime.Date, zDateTime.Kind);
			}
			else if (o is ZDate date)
			{
				return new ZDateTimeOffset(date);
			}
			else if (o is DateTime dateTime)
			{
				return ToDateOnlyDatePart(new ZDateTime(dateTime));
			}
			else if (o is DateTimeOffset dateTimeOffset)
			{
				return new ZDateTimeOffset(dateTimeOffset.Year, dateTimeOffset.Month, dateTimeOffset.Day, 0, 0, 0, dateTimeOffset.Offset);
			}
			else if (o is ZDateTimeOffset zDateTimeOffset)
			{
				return new ZDateTimeOffset(zDateTimeOffset.Year, zDateTimeOffset.Month, zDateTimeOffset.Day, 0, 0, 0, zDateTimeOffset.Offset);
			}
			else
			{
				return ZDateTimeOffset.Empty;
			}
		}
	}
}
