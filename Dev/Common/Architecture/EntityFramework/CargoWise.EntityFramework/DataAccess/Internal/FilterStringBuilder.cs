using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.Schema;
using CargoWise.Types;
using Microsoft.SqlServer.Types;

namespace CargoWise.EntityFramework
{
	public interface IFilterPartsProvider
	{
		IFilterPart[] FilterParts { get; }
		ZQuery[] GetCompositeParts();
	}

	public class FilterStringBuilder : AbstractFilterPart, IFilterPart, IFilterPartsProvider, IEnumerable<IFilterPart>
	{
		public bool ContainsOnly(JoinCondition joinCondition)
		{
			if (joinCondition == null)
			{
				throw new ArgumentNullException(nameof(joinCondition));
			}

			bool result = true;
			foreach (IFilterPart filterPart in this)
			{
				JoinCondition queryJoinCondition = filterPart as JoinCondition;
				if (queryJoinCondition != null && queryJoinCondition != joinCondition)
				{
					result = false;
					break;
				}
			}
			return result;
		}

		public void Simplify()
		{
			int i = 0;
			JoinCondition lastJoinCondition = null;
			while (i < filterParts.Count)
			{
				IFilterPart filterPart = filterParts[i];
				//#HACK
				if (filterPart.GetType() == typeof(ZQuery))
				{
					ZQuery filterPartAsQuery = filterPart as ZQuery;
					if (filterPartAsQuery.IsTopNQuery)
					{
						filterPartAsQuery.MaximumRows = null;
					}
				}
				//#ENDHACK
				if (filterPart is JoinCondition)
				{
					lastJoinCondition = filterPart as JoinCondition;
				}
				IFilterPart[] simplifiedFilterParts = filterPart.GetSimplifiedVersion(lastJoinCondition);
				if (simplifiedFilterParts.Length == 1)
				{
					filterParts[i] = simplifiedFilterParts[0];
					i++;
				}
				else
				{
					filterParts.RemoveAt(i);
					if (simplifiedFilterParts.Length == 0)
					{
						if (i > 0)
						{
							if ((filterParts[i - 1] is JoinCondition))
							{
								filterParts.RemoveAt(i - 1);
								i--;
							}
							else
							{
								ErrorReporter.ReportOnce("Missing Join Condition", "Missing Join Condition");
							}
						}
						else
						{
							if (filterParts.Count > 0)
							{
								filterParts.RemoveAt(0);
							}
						}
					}
					else
					{
						filterParts.InsertRange(i, simplifiedFilterParts);
					}
					i += simplifiedFilterParts.Length;
				}
			}
		}

		public IEnumerable<SchemaColumn> BlobFilters
		{
			get { return this.SelectMany(filterPart => filterPart.BlobFilters); }
		}

		IFilterPart[] IFilterPart.GetSimplifiedVersion(JoinCondition lastJoinCondition)
		{
			Simplify();
			return filterParts.ToArray();
		}

		IEnumerable<IFilterPart> IFilterPart.FilterParts => filterParts;

		public FilterStringBuilder ShallowClone()
		{
			return CloneCore(false);
		}

		public FilterStringBuilder DeepClone()
		{
			return CloneCore(true);
		}

		IFilterPart IFilterPart.DeepClone()
		{
			return DeepClone();
		}

		public FilterStringBuilder(int capacity = 0)
		{
			ModificationsEnabled = true;
			filterParts = new List<IFilterPart>(capacity);
		}

		public override bool HasParameters
		{
			get
			{
				foreach (IFilterPart filterPart in filterParts)
				{
					if (filterPart.HasParameters)
					{
						return true;
					}
				}
				return false;
			}
		}

		public override bool HasComparisonOperatorLike
		{
			get
			{
				return filterParts.Any(x => x.HasComparisonOperatorLike);
			}
		}

		/// <summary>
		/// The number of IFilterParts that are stored in the FilterStringBuilder instance
		/// </summary>
		public int Count
		{
			get { return filterParts.Count; }
		}

		public IFilterPart GetFilterPart(int index)
		{
			return filterParts[index];
		}

		// Tested through ZQuery
		public ZSqlParameter[] GetAllSingleEqualParameters()
		{
			List<ZSqlParameter> result = new List<ZSqlParameter>();
			foreach (ZQuery compositePart in GetCompositeParts())
			{
				int filterPartsWithParameters = 0;
				ZSqlParameter singleEqualParam = null;

				foreach (IFilterPart filterPart in compositePart.FilterParts.filterParts)
				{
					if (filterPart.HasParameters)
					{
						filterPartsWithParameters++;
						if (filterPartsWithParameters > 1)
						{
							singleEqualParam = null;
							break;
						}
					}
					ZSqlParameter potentialParam = filterPart as ZSqlParameter;
					if (potentialParam != null && potentialParam.ComparisonOperator == SQLComparisonOperator.Equal)
					{
						singleEqualParam = potentialParam;
					}
				}
				if (singleEqualParam != null)
				{
					result.Add(singleEqualParam);
				}
			}
			return result.ToArray();
		}

		// Tested through ZQuery
		public ZSqlParameter GetMostUniqueSingleEqualParameter()
		{
			ZSqlParameter result = null;
			foreach (var sqlParam in GetAllSingleEqualParameters())
			{
				if (result == null)
				{
					result = sqlParam;
				}
				else
				{
					if ((sqlParam.SchemaColumn.UniquenessRanking + AdjustmentForValue(sqlParam.Value))
						> (result.SchemaColumn.UniquenessRanking + AdjustmentForValue(result.Value)))
					{
						result = sqlParam;
					}
				}
			}
			return result;
		}

		internal int AdjustmentForValue(object value)
		{
			if (value == null
				|| value == DBNull.Value
				|| (value is IZType zType && zType.IsEmpty)
				|| (value is string str && string.IsNullOrEmpty(str))
				)
			{
				return -1000;
			}
			return 0;
		}

		public ZQuery[] GetOrParts() => GetPartsCore(JoinCondition.Or);

		public ZQuery[] GetAndParts() => GetPartsCore(JoinCondition.And);

		internal ZQuery[] GetPartsCore(JoinCondition joinCondition)
		{
			var result = new List<ZQuery>();

			foreach (var filterPart in FilterParts)
			{
				if (filterPart is JoinCondition join)
				{
					if (join != joinCondition && result.Count > 0)
					{
						result.Clear();
						break;
					}
				}
				else if (filterPart is ZSqlParameter sqlParameter)
				{
					result.Add(new ZQuery(sqlParameter));
				}
				else if (filterPart is FilterBracket filterBracket)
				{
					var bracketQuery = filterBracket.FilterParts.GetPartsCore(joinCondition);
					if (bracketQuery.Length > 0)
					{
						result.AddRange(bracketQuery);
					}
					else
					{
						result.Add(new ZQuery(filterBracket));
					}
				}
				else if (filterPart is ZQuery query)
				{
					if (!query.IsEmpty)
					{
						var queryPart = query.FilterParts.GetPartsCore(joinCondition);
						if (queryPart.Length > 0)
						{
							result.AddRange(queryPart);
						}
						else
						{
							result.Add(query);
						}
					}
				}
				else if (filterPart is ZSQLInFilter sqlInFilter)
				{
					if (joinCondition == JoinCondition.Or)
					{
						result.AddRange(sqlInFilter.GetOrParts());
					}
					else if (joinCondition == JoinCondition.And)
					{
						//In-filter should have no and part
						result.Add(new ZQuery(sqlInFilter));
					}
				}
				else
				{
					result.Clear();
					break;
				}
			}

			return result.ToArray();
		}

		internal IEnumerable<ZQuery> GetOutermostQueries()
		{
			var result = new List<ZQuery>();

			foreach (var filterPart in FilterParts)
			{
				if (filterPart is FilterBracket filterBracket)
				{
					result.AddRange(filterBracket.FilterParts.GetOutermostQueries());
				}
				else if (filterPart is ZQuery query)
				{
					result.Add(query);
				}
			}
			return result;
		}

		#region ToCSharpCode
#if DEBUG

		string OffsetToTimeSpanConstructor(TimeSpan offset)
		{
			string result = "new TimeSpan(" + offset.Ticks + ")";
			return result;
		}

		public string ToCSharpCode(string initialJoinCondition, NumberPublisher publisher, string queryVariable)
		{
			StringBuilder stringBuilder = new StringBuilder();
			string lastJoinCondition = initialJoinCondition != null && initialJoinCondition.Length > 0 ? initialJoinCondition : "JoinCondition.And";
			foreach (IFilterPart filterPart in filterParts)
			{
				if (filterPart is JoinCondition)
				{
					JoinCondition joinCondition = (JoinCondition)filterPart;
					lastJoinCondition = "JoinCondition." + ((ZString)(joinCondition.Text)).ToTitleCase();
				}
				else if (filterPart is ZDBOnlySubQuery)
				{
					ZDBOnlySubQuery query = (ZDBOnlySubQuery)filterPart;
					string innerQueryVariable = "";
					stringBuilder.Append(query.ToCSharpCode(lastJoinCondition, publisher, out innerQueryVariable));
					stringBuilder.AppendLine(innerQueryVariable + " = " + innerQueryVariable + ".ShallowCopy(\"" + query.FieldName + "\", \"" + query.SubQueryFieldName + "\");");
					stringBuilder.AppendLine(queryVariable + ".AddToFilter(" + lastJoinCondition + ", " + innerQueryVariable + ");");
					//query.FieldName + "\", \"" + query.SubQueryFieldName + "\", " + innerQueryVariable + ", " + lastJoinCondition + ");");
				}
				else if (filterPart is ZQuery)
				{
					ZQuery query = (ZQuery)filterPart;
					string innerQueryVariable = "";
					stringBuilder.Append(query.ToCSharpCode(lastJoinCondition, publisher, out innerQueryVariable));
					stringBuilder.AppendLine(queryVariable + ".AddToFilter(" + innerQueryVariable + ", " + lastJoinCondition + ");");
				}
				else if (filterPart is ZSqlParameter)
				{
					ZSqlParameter p = (ZSqlParameter)filterPart;
					stringBuilder.Append(queryVariable + ".AddToFilter(");
					if (lastJoinCondition != null)
					{
						stringBuilder.Append(lastJoinCondition + ", ");
					}
					stringBuilder.Append(p.SchemaColumn.TableName + "Schema." + p.SchemaColumn.ObjectName + ", ");
					stringBuilder.Append("SQLComparisonOperator." + p.ComparisonOperator.ToString() + ", ");

					string stringisedValue;
					if (p.Value is string)
					{
						stringisedValue = '"' + p.Value.ToString() + '"';
					}
					else if (p.Value is Guid)
					{
						stringisedValue = "new Guid(\"" + p.Value.ToString() + "\")";
					}
					else if (p.Value is DateTime)
					{
						DateTime pDateTime = (DateTime)p.Value;
						stringisedValue = "new ZDateTime(" + pDateTime.ToString("yyyy, M, d, H, m, s") + ")";
					}
					else if (p.Value is TimeSpan)
					{
						TimeSpan pTime = (TimeSpan)p.Value;
						stringisedValue = $"new ZTime({pTime.Hours}, {pTime.Minutes}, {pTime.Seconds})";
					}
					else if (p.Value is DateTimeOffset)
					{
						DateTimeOffset pDateTimeOffset = (DateTimeOffset)p.Value;
						stringisedValue = "new ZDateTimeOffset(" + pDateTimeOffset.Year.ToString() + ", " + pDateTimeOffset.Month.ToString() + ", " + pDateTimeOffset.Day.ToString()
							+ ", " + pDateTimeOffset.Hour.ToString() + ", " + pDateTimeOffset.Minute.ToString() + ", " + pDateTimeOffset.Second.ToString() + ", " + OffsetToTimeSpanConstructor(pDateTimeOffset.Offset) + ")"; //todo: offset helpers
					}
					else if (p.Value is SqlGeography pGeography)
					{
						stringisedValue = "new ZGeography(\"" + pGeography.AsTextZM().ToSqlString().ToString() + "\")";
					}
					else
					{
						stringisedValue = p.Value.ToString();
					}
					stringBuilder.AppendLine(stringisedValue + ");");
				}
				else if (filterPart is FilterBracket)
				{
					FilterBracket filterBracket = (FilterBracket)filterPart;
					string filterBracketObjectName;
					stringBuilder.Append(filterBracket.ToCSharpCode(lastJoinCondition, publisher, out filterBracketObjectName));
					stringBuilder.AppendLine(queryVariable + ".AddToFilter(" + lastJoinCondition + ", " + filterBracketObjectName + ");");
				}
				else if (filterPart is ZSQLInFilter)
				{
					ZSQLInFilter inFilter = (ZSQLInFilter)filterPart;
					stringBuilder.AppendLine(inFilter.ToCSharpCode(lastJoinCondition, publisher, queryVariable));
				}
				else if (filterPart is ZNonPersistentDataQuery)
				{
					ZNonPersistentDataQuery query = (ZNonPersistentDataQuery)filterPart;
					string queryObjectName;
					stringBuilder.AppendLine(query.ToCSharpCode(lastJoinCondition, publisher, out queryObjectName));
					stringBuilder.AppendLine(queryObjectName + ".AddToFilter(" + lastJoinCondition + ", " + queryObjectName + ");");
				}
				else
				{
					throw new NotSupportedException("Implement for " + filterPart.GetType().FullName);
				}
			}
			return stringBuilder.ToString();
		}

#endif
		#endregion

		public ZQuery[] GetCompositeParts()
		{
			var cachedResult = CompositePartsCache;
			if (cachedResult != null)
			{
				return cachedResult;
			}

			List<ZQuery> resultList = new List<ZQuery>();
			for (int i = 0; i < filterParts.Count; i++)
			{
				IFilterPart filterPart = filterParts[i];
				if (filterPart is JoinCondition)
				{
					if ((JoinCondition)filterPart == JoinCondition.Or)
					{
						resultList.Clear();
						break;
					}
				}
				else if (filterPart is FilterBracket)
				{
					resultList.Add(new ZQuery((FilterBracket)filterPart));
				}
				else if (filterPart is ZSqlParameter || filterPart is ZSQLInFilter || filterPart is ZDBOnlySubQuery)
				{
					resultList.Add(new ZQuery(filterPart));
				}
				else if (filterPart is ZQuery)
				{
					ZQuery innerFilterPart = (ZQuery)filterPart;
					if (!innerFilterPart.IsEmpty)
					{
						ZQuery[] compositeParts = innerFilterPart.GetCompositeParts();
						if (compositeParts.Length > 0)
						{
							resultList.AddRange(compositeParts);
						}
						else
						{
							resultList.Add(innerFilterPart);
						}
					}
				}
				else
				{
					resultList.Clear();
					break;
					// ErrorReporter.ReportOnce("UnknownFilterPartType", "Unknown filter part type : " + filterPart.GetType().FullName);
				}
			}

			var result = resultList.ToArray();
			CompositePartsCache = result;
			return result;
		}

		ZQuery[] CompositePartsCache
		{
			get
			{
				ZQuery[] result = null;
				if (compositePartsCache != null)
				{
					compositePartsCache.TryGetTarget(out result);
				}
				return result;
			}
			set
			{
				compositePartsCache = new WeakReference<ZQuery[]>(value);
			}
		}
		WeakReference<ZQuery[]> compositePartsCache;

		#region Contains Non-Bracketed Or

		protected bool containsNonBracketedOr;

		public bool ContainsNonBracketedOr
		{
			get { return containsNonBracketedOr; }
		}

		#endregion

		#region Equals / GetHashCode

		public override bool Equals(object obj)
		{
			FilterStringBuilder rhs = obj as FilterStringBuilder;
			return
				rhs != null &&
				ItemsEqual(filterParts, rhs.filterParts);
		}

		public override int GetHashCode()
		{
			return (filterParts.Count == 0 ? 0 : filterParts[0].GetHashCode()) ^ filterParts.Count;
		}

		static bool ItemsEqual(List<IFilterPart> lhs, List<IFilterPart> rhs)
		{
			bool result = lhs.Count == rhs.Count;

			if (result)
			{
				for (int i = 0; i < lhs.Count; i++)
				{
					ZSqlParameter lhsParam = lhs[i] as ZSqlParameter;
					ZSqlParameter rhsParam = rhs[i] as ZSqlParameter;

					if (lhsParam != null && rhsParam != null)
					{
						if (!lhsParam.EqualsIgnoringParameterName(rhsParam))
						{
							result = false;
							break;
						}
					}
					else if (!object.Equals(lhs[i], rhs[i]))
					{
						result = false;
						break;
					}
				}
			}

			return result;
		}

		#endregion

		public ZSqlParameter[] Params()
		{
			ZNonPersistentDataQuery query = new SqlFilterPartAppender(SqlBuilder.QueryType.Parameterised).GetParameterisedSql(this);
			return query.Parameters;
		}

		public bool NeedsBrackets
		{
			get { return false; }
		}

		public string LiteralTextADO
		{
			get { return new SqlFilterPartAppender(SqlBuilder.QueryType.LiteralADO).GetParameterisedSql(this).ParameterisedQueryText; }
		}

		public void AddLiteralTextADO(SqlBuilder sqlBuilder)
		{
			new SqlFilterPartAppender(SqlBuilder.QueryType.LiteralADO).AddParameterisedSql(sqlBuilder, this);
		}

		public bool FilterIsEmpty
		{
			get
			{
				bool result = true;
				foreach (IFilterPart filterPart in filterParts)
				{
					if (!filterPart.FilterIsEmpty)
					{
						result = false;
						break;
					}
				}

				return result;
			}
		}

		public bool ContainsOrOperator
		{
			get
			{
				bool result = false;
				for (int i = 0; i < filterParts.Count && !result; i++)
				{
					IFilterPart filterPart = filterParts[i];
					result = filterPart.ContainsOrOperator;
				}
				return result;
			}
		}

		public ZNonPersistentDataQuery ParameterisedSql(ParameterNameFactory factory)
		{
			return new SqlFilterPartAppender(SqlBuilder.QueryType.Parameterised).GetParameterisedSql(this);
		}

		public void ParameterisedSql(SqlBuilder sqlBuilder)
		{
			new SqlFilterPartAppender(SqlBuilder.QueryType.Parameterised).AddParameterisedSql(sqlBuilder, this);
		}

		public void Reset()
		{
			CheckModificationStatus();
			containsNonBracketedOr = false;
			ClearFilterParts();
		}

		public void Bracket()
		{
			CheckModificationStatus();
			FilterBracket bracket = new FilterBracket(filterParts);
			ClearFilterParts();
			AddFilterPart(bracket);
			containsNonBracketedOr = false;
		}

		public IFilterPart[] FilterParts
		{
			get { return filterParts.ToArray(); }
		}
		internal List<IFilterPart> filterParts;

		public void Append(IList<IFilterPart> filterParts)
		{
			foreach (IFilterPart filterPart in filterParts)
			{
				Append(filterPart);
			}
		}

		public void Append(IFilterPart filterPart)
		{
			if (filterPart == null)
			{
				throw new ArgumentNullException(nameof(filterPart));
			}
			if (filterPart == this)
			{
				throw new ApplicationException("Attempted to add this to yourself?");
			}

			CheckModificationStatus();
			if (AddFilterPart(filterPart))
			{
				CheckForOr(filterPart);
			}
		}

		bool ModificationsEnabled;

		void IFilterPart.DisableModifications()
		{
			ModificationsEnabled = false;
		}

		public void Append(JoinCondition joinOperator, IFilterPart param)
		{
			if (joinOperator == null)
			{
				throw new ArgumentNullException(nameof(joinOperator));
			}
			if (param == null)
			{
				throw new ArgumentNullException(nameof(param));
			}
			Append(joinOperator);
			Append(param);
		}

		public void CheckModificationStatus()
		{
			if (!ModificationsEnabled)
			{
				ErrorReporter.ReportOnce("ModificationsEnabled", "The current filter has been cloned - no further changes should be made");
			}
		}

		public override string ToString()
		{
			return LiteralTextADO;
		}

		protected FilterStringBuilder CloneCore(bool isDeepClone)
		{
			FilterStringBuilder result = new FilterStringBuilder(filterParts.Count);
			foreach (IFilterPart filterPart in filterParts)
			{
				if (isDeepClone)
				{
					result.AddFilterPart(filterPart.DeepClone());
				}
				else
				{
					result.AddFilterPart(filterPart);
				}
			}
			result.containsNonBracketedOr = containsNonBracketedOr;
			return result;
		}

		void ClearFilterParts()
		{
			filterParts.Clear();
			compositePartsCache = null;
		}

		bool AddFilterPart(IFilterPart filterPart)
		{
			if (filterPart is JoinCondition)
			{
				if (filterParts.Count == 0)
				{
					return false;
				}
				else if (filterParts[filterParts.Count - 1] is JoinCondition)
				{
					throw new ArgumentException("Cannot add two JoinCondition objects in sequence", nameof(filterPart));
				}
			}
			filterPart.DisableModifications();
			filterParts.Add(filterPart);
			compositePartsCache = null;
			return true;
		}

		void CheckForOr(IFilterPart filterPart)
		{
			if (!containsNonBracketedOr && filterPart.ContainsOrOperator)
			{
				containsNonBracketedOr = true;
			}
		}

		#region IEnumerable Members

		IEnumerator<IFilterPart> IEnumerable<IFilterPart>.GetEnumerator()
		{
			return filterParts.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return filterParts.GetEnumerator();
		}

		#endregion

	}
}
