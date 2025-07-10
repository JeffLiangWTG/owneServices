using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Messaging.Module
{
	public class EDIInterchangeTextFilter : ModuleTextFilter
	{
		internal EDIInterchangeTextFilter(ZString description, FilterStripBusinessObject filterBusinessObject)
			: base(description, (comparisonOperator, value) => GetQuery(comparisonOperator, value))
		{
			this.filterBusinessObject = filterBusinessObject;
		}

		readonly FilterStripBusinessObject filterBusinessObject;

		static ZQuery GetQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var resultQuery = new ZQuery();
			var joinCondition = (comparisonOperator.IsNegativeSQLOperator() || comparisonOperator == SpecialComparisonOperator.IsBlank && value.IsEmpty) ? JoinCondition.And : JoinCondition.Or;
			resultQuery.AddToFilter(joinCondition, EDIInterchangeSchema.EI_BodyNText, comparisonOperator, value);
			resultQuery.AddToFilter(joinCondition, EDIInterchangeSchema.EI_BodyText, comparisonOperator, value);

			var sql = "IsNull(dbo.CLRUncompressAsString(" + EDIInterchangeSchema.EI_BodyData.Name + "), '') " + comparisonOperator.ComparisonText(value) + " @Value";
			resultQuery.AddFilterAndZSQLParameterCollection(sql, new ZSqlParameterCollection(
							ZSqlParameter.New("@Value", GetSqlValue(comparisonOperator, value), EDIInterchangeSchema.EI_BodyData)), joinCondition);
			return resultQuery;
		}

		static string GetSqlValue(SQLComparisonOperator comparisonOperator, string value)
		{
			if (comparisonOperator == SQLComparisonOperator.StartsWith || comparisonOperator == SQLComparisonOperator.DoesNotStartWith)
			{
				value = value + "%";
			}
			else if (comparisonOperator == SQLComparisonOperator.Contains || comparisonOperator == SQLComparisonOperator.NotContains)
			{
				value = "%" + value + "%";
			}
			return value;
		}

		protected override ModuleFilterValidation GetNewValidation()
		{
			return new EDIInterchangeTextFilterValidation(this, filterBusinessObject);
		}
	}
}
