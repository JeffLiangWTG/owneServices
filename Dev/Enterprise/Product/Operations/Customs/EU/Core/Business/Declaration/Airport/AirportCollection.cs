using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Business.Declaration
{
	[ModuleID(ZArchitecture.Modules.ModuleId.RefUNLOCO)]
	public class AirportCollection : BusinessObjectCollection<Airport>
	{
		public AirportCollection(BusinessObjectFactory factory, string foreignUnloco, bool isNonEuAirport)
			: base(factory)
		{
			this.foreignUnloco = foreignUnloco;
			this.isNonEuAirport = isNonEuAirport;
		}

		public AirportCollection(BusinessObjectFactory factory, bool isNonEuAirport) : base(factory)
		{
			this.isNonEuAirport = isNonEuAirport;
		}

		readonly bool isNonEuAirport;
		readonly ZString foreignUnloco;

		protected override ZQuery CreateRelationshipFilter()
		{
			var unlocoQuery = new ZDBOnlyQuery(typeof(RefUNLOCO));

			var refCusTradeGroupCountryQuery = new ZDBOnlySubQuery(typeof(CusRefTradeGroupCountryView), CusRefTradeGroupCountryViewSchema.ZZB_RN_NKTradeGroupCountryCode, notIn: isNonEuAirport);
			refCusTradeGroupCountryQuery.AddToFilter(CusRefTradeGroupCountryViewSchema.ZZB_StartDate, SQLComparisonOperator.LessThanOrEqualTo, ZDateTime.Now);
			refCusTradeGroupCountryQuery.AddToFilter(CusRefTradeGroupCountryViewSchema.ZZB_EndDate, SQLComparisonOperator.GreaterThanOrEqualTo, ZDateTime.Now);
			var refCusTradeGroupQuery = new ZDBOnlySubQuery(typeof(CusRefTradeGroupView), CusRefTradeGroupViewSchema.PK);

			refCusTradeGroupQuery.AddToFilter(CusRefTradeGroupViewSchema.ZZA_TradeGroup, new[] { Core.Constants.Customs.Universal.RefCusTradeGroup.Codes.EuropeanUnionForCustoms, Core.Constants.Customs.Universal.RefCusTradeGroup.Codes.CustomsUnionAdditionalMembers });
			refCusTradeGroupCountryQuery.AddSubQuery(CusRefTradeGroupCountryViewSchema.ZZB_ZZA_TradeGroup, refCusTradeGroupQuery, JoinCondition.And);

			unlocoQuery.AddToFilter(RefUNLOCOSchema.RL_IATA, SQLComparisonOperator.NotEqual, string.Empty);
			unlocoQuery.AddToFilter(RefUNLOCOSchema.RL_HasAirport, true);
			unlocoQuery.AddSubQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, refCusTradeGroupCountryQuery, JoinCondition.And);
			if (!foreignUnloco.IsEmpty)
			{
				string filterCriteriaHmmHowStrictShouldWeBe = foreignUnloco.Left(2);  // To change our strictness, change the right-hand side of this assignment to foreignUnloco or "" or foreignUnloco.Left(2)
				unlocoQuery.AddToFilter(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.StartsWith, filterCriteriaHmmHowStrictShouldWeBe);
			}
			return unlocoQuery;
		}
	}
}
