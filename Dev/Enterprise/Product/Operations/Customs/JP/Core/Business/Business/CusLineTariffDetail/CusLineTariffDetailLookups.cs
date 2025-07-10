using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.JP.Business
{
	public class CusLineTariffDetailLookups : Customs.Business.CusLineTariffDetailLookups
	{
		public CusLineTariffDetailLookups(CusLineTariffDetail parent) : base(parent)
		{
		}

		public new CusLineTariffDetail Parent => (CusLineTariffDetail)base.Parent;

		public override ICodeDescriptionPairList TariffTypeList
		{
			get
			{
				return Factory.GetCachedValue("JP.CusLineTariffDetailLookups.TariffTypeList", () =>
				{
					var result = new CodeDescriptionPairList();
					var tariffTypeQuery = new ZQuery(RefCusTariffTypeSchema.ZZI_ZZZ_NKDataGrouping, Core.Constants.CountryCodes.Japan);
					tariffTypeQuery.AddToFilter(RefCusTariffTypeSchema.ZZI_TariffType, SQLComparisonOperator.NotEqual, new [] { "HSN", "EXP", "IMP" });
					var tariffTypes = Factory.Load<RefCusTariffType>(tariffTypeQuery);
					foreach (var tariffType in tariffTypes)
					{
						result.AddPairIfNotExist(tariffType.ZZI_TariffType, tariffType.ZZI_Description);
					}
					return result;
				});
			}
		}

		public TariffViewCollection TariffCollection => TariffViewCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.Japan, Parent.BZ_Type, ZDateTime.Today);

		public CodeDescriptionPairList ExemptionReductionCodeList
		{
			get
			{
				var today = ZDateTime.Today;
				return Factory.GetCachedValue($"JP.CusLineTariffDetailLookups.ExemptionReductionCodeList-{today.ToShortDateString()}", () =>
				{
					var result = new CodeDescriptionPairList();
					var codeQuery = new ZQuery(RefCusCodeListSchema.ZZD_ZZZ_NKDataGrouping, Core.Constants.CountryCodes.Japan);
					codeQuery.AddToFilter(RefCusCodeListSchema.ZZD_ZZK_NKCodeType, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanImportConsumptionTaxExemptionCode);
					codeQuery.AddToFilter(RefCusCodeListSchema.ZZD_StartDate, SQLComparisonOperator.LessThanOrEqualTo, today);
					codeQuery.AddToFilter(RefCusCodeListSchema.ZZD_EndDate, SQLComparisonOperator.GreaterThanOrEqualTo, today);
					codeQuery.OrderBy = RefCusCodeListSchema.Constants.ZZD_Code + OrderByClause.Ascending;
					Factory.Load<RefCusCodeList>(codeQuery).ForEach(c => result.AddPairIfNotExist(c.ZZD_Code, c.ZZD_Description));
					return result;
				});
			}
		}
	}
}
