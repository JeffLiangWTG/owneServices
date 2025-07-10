using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ES.Business
{
	public static class CusGuaranteeHeaderHelper
	{
		public static CusGuaranteeHeader LoadCusGuaranteeHeaderFromReference(BusinessObjectFactory factory, ZString guaranteeReference, ZString countryCode, ZString type)
		{
			var query = new ZQuery(CusPermitHeaderSchema.CPH_ApplicationCode, CusPermitHeaderApplicationCodeList.Codes.Guarantee);
			query.AddToFilter(CusPermitHeaderSchema.CPH_Number, guaranteeReference);
			query.AddToFilter(CusPermitHeaderSchema.CPH_RN_NKCountryCode, countryCode);
			query.AddToFilter(CusPermitHeaderSchema.CPH_Type, type);
			return factory.LoadTop1<CusGuaranteeHeader>(query);
		}

		public static CusGuaranteeHeader[] LoadCusGuaranteeHeadersFromReferenceWithOBLTransaction(BusinessObjectFactory factory, ZString guaranteeReference, ZString countryCode, ZString type)
		{
			var query = new ZQuery(CusPermitHeaderSchema.CPH_ApplicationCode, CusPermitHeaderApplicationCodeList.Codes.Guarantee);
			query.AddToFilter(CusPermitHeaderSchema.CPH_Number, guaranteeReference);
			query.AddToFilter(CusPermitHeaderSchema.CPH_RN_NKCountryCode, countryCode);
			query.AddToFilter(CusPermitHeaderSchema.CPH_Type, type);
			query.AddToFilter(CusPermitHeaderSchema.CPH_StartDate, SQLComparisonOperator.LessThanOrEqualTo, ZDate.Today);

			var endDateQuery = new ZQuery(CusPermitHeaderSchema.CPH_EndDate, SQLComparisonOperator.GreaterThanOrEqualTo, ZDate.Today);
			endDateQuery.AddToFilter(JoinCondition.Or, CusPermitHeaderSchema.CPH_EndDate, null);
			query.AddToFilter(endDateQuery);

			var guarantees = factory.Load<CusGuaranteeHeader>(query);
			return guarantees.Where(x => x.HasOpeningBalanceTransaction).ToArray();
		}

		public static ZString PopulateGuaranteeFromLocation(BusinessObjectFactory factory, ZString goodsLocation)
		{
			var permitHeaderQuery = new ZDBOnlyQuery(typeof(CusPermitHeader));
			permitHeaderQuery.AddToFilter(CusPermitHeaderSchema.CPH_ApplicationCode, CusPermitHeaderApplicationCodeList.Codes.Guarantee);
			permitHeaderQuery.AddToFilter(CusPermitHeaderSchema.CPH_Type, EUGuaranteeTypeList.Codes.TST);

			var permitRuleQuery = new ZDBOnlySubQuery(typeof(Customs.Business.CusGuaranteeRule), CusPermitRuleSchema.CPR_CPH_PermitHeader);
			permitRuleQuery.AddToFilter(CusPermitRuleSchema.CPR_RuleCode, EU.Business.PermitRuleCodeList.Codes.TSP);
			permitRuleQuery.AddToFilter(CusPermitRuleSchema.CPR_ValueFrom, goodsLocation);

			permitHeaderQuery.AddSubQuery(permitRuleQuery, JoinCondition.And);

			var source = factory.Load<CusGuaranteeHeader>(permitHeaderQuery);

			return source.Length == 1 ? source[0].CPH_Number : ZString.Empty;
		}
	}
}
