using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Messaging.Module
{
	public class EDIMessageTextFilter : ModuleTextFilter
	{
		internal EDIMessageTextFilter(ZString description, FilterStripBusinessObject filterBusinessObject)
			: base(description, (comparisonOperator, value) => GetQuery(comparisonOperator, value))
		{
			this.filterBusinessObject = filterBusinessObject;
		}

		readonly FilterStripBusinessObject filterBusinessObject;

		static ZQuery GetQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZQuery();
			var joinCondition = (comparisonOperator.IsNegativeSQLOperator() || comparisonOperator == SpecialComparisonOperator.IsBlank && value.IsEmpty) ? JoinCondition.And : JoinCondition.Or;
			query.AddToFilter(EDIMessageSchema.EM_MessageText, comparisonOperator, value);
			query.AddToFilter(joinCondition, EDIMessageSchema.EM_MessageNText, comparisonOperator, value);

			var sql = "IsNull(dbo.CLRUncompressAsString(" + EDIMessageSchema.EM_MessageData.Name + "), '') " + comparisonOperator.ComparisonText(value) + " @Value";
			query.AddFilterAndZSQLParameterCollection(sql, new ZSqlParameterCollection(
							ZSqlParameter.New("@Value", GetSqlValue(comparisonOperator, value), EDIMessageSchema.EM_MessageData)), joinCondition);

			return query;
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
			return new EDIMessageTextFilterValidation(this, filterBusinessObject);
		}
	}
}
