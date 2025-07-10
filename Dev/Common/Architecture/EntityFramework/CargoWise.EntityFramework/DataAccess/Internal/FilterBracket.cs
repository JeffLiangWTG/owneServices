using System.Collections.Generic;
using System.Text;
using CargoWise.Schema;

namespace CargoWise.EntityFramework
{
	class FilterBracket : AbstractFilterPart, IFilterPart, IFilterPartsProvider
	{
		public FilterBracket(IList<IFilterPart> filterParts)
			: this(new FilterStringBuilder(filterParts.Count))
		{
			FilterParts.Append(filterParts);
		}

#if DEBUG
		#region Support for ToCSharpCode method
		internal FilterBracket(ZQuery query)
			: this(new FilterStringBuilder(query.FilterParts.Count))
		{
			FilterParts.Append(query.FilterParts.filterParts);
		}

		#endregion
#endif

		FilterBracket(FilterStringBuilder builder)
		{
			FilterParts = builder;
		}

		#region Equals / GetHashCode

		public override bool Equals(object obj)
		{
			FilterBracket rhs = obj as FilterBracket;
			return rhs != null && FilterParts.Equals(rhs.FilterParts);
		}

		public override int GetHashCode()
		{
			return 0;
		}

		#endregion

		#region IFilterPart Members

		public ZNonPersistentDataQuery ParameterisedSql(ParameterNameFactory factory)
		{
			return FilterParts.ParameterisedSql(factory);
		}

		public void ParameterisedSql(SqlBuilder sqlBuilder)
		{
			FilterParts.ParameterisedSql(sqlBuilder);
		}

		public FilterStringBuilder FilterParts { get; }

		public string LiteralTextADO
		{
			get { return FilterParts.LiteralTextADO; }
		}

		public void AddLiteralTextADO(SqlBuilder sqlBuilder)
		{
			FilterParts.AddLiteralTextADO(sqlBuilder);
		}

		public bool NeedsBrackets
		{
			get { return true; }
		}

		IFilterPart[] IFilterPart.GetSimplifiedVersion(JoinCondition lastJoinCondition)
		{
			FilterParts.Simplify();
			if (lastJoinCondition != null && FilterParts.ContainsOnly(lastJoinCondition))
			{
				return FilterParts.FilterParts;
			}
			else
			{
				return new IFilterPart[] { this };
			}
		}

		IEnumerable<IFilterPart> IFilterPart.FilterParts => FilterParts;

		void IFilterPart.DisableModifications()
		{
			// Object is immutable so no work to do
		}

		IEnumerable<SchemaColumn> IFilterPart.BlobFilters
		{
			get { return FilterParts.BlobFilters; }
		}

		bool IFilterPart.ContainsOrOperator
		{
			get { return FilterParts.ContainsOrOperator; }
		}

		public bool FilterIsEmpty
		{
			get { return FilterParts.FilterIsEmpty; }
		}

		public FilterBracket DeepClone()
		{
			return new FilterBracket(FilterParts.DeepClone());
		}

		IFilterPart IFilterPart.DeepClone()
		{
			return DeepClone();
		}

		public override bool HasParameters
		{
			get { return FilterParts.HasParameters; }
		}

		public override bool HasComparisonOperatorLike
		{
			get { return FilterParts.HasComparisonOperatorLike; }
		}

#if DEBUG
		internal string ToCSharpCode(string initialJoinCondition, NumberPublisher publisher, out string filterBracketVariableName)
		{
			string queryVariable = "filterQuery" + publisher.GetNextVariableNumberSuffix().ToString();
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("ZQuery " + queryVariable + " = new ZQuery();");
			stringBuilder.AppendLine(FilterParts.ToCSharpCode(initialJoinCondition, publisher, queryVariable));
			filterBracketVariableName = "filterBracket" + publisher.GetNextVariableNumberSuffix().ToString();
			stringBuilder.AppendLine("FilterBracket " + filterBracketVariableName + " = new FilterBracket(" + queryVariable + ");");
			return stringBuilder.ToString();
		}
#endif

		#endregion

		#region IFilterPartsProvider Members

		IFilterPart[] IFilterPartsProvider.FilterParts
		{
			get { return FilterParts.FilterParts; }
		}

		ZQuery[] IFilterPartsProvider.GetCompositeParts()
		{
			return FilterParts.GetCompositeParts();
		}

		#endregion
	}
}
