using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AsycudaCustoms.Business
{
	public class CommonCusBondDetailLookups : CusBondDetailLookups
	{
		public CommonCusBondDetailLookups(CommonCusBondDetail parent) : base(parent)
		{
		}

		protected new CommonCusBondDetail Parent => (CommonCusBondDetail)base.Parent;

		public CodeDescriptionPairList BondTypeList => Factory.GetCachedValue<GuaranteeBondTypeList>();

		public CusGuaranteeHeaderCollection GuaranteeCollection
		{
			get
			{
				var subQuery = new ZQuery(CusPermitHeaderSchema.CPH_EndDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, ZDate.Today);
				subQuery.AddToFilter(JoinCondition.Or, CusPermitHeaderSchema.CPH_EndDate, SQLComparisonOperator.Equal, null);
				var query = new ZQuery(CusPermitHeaderSchema.CPH_StartDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, ZDate.Today);
				query.AddToFilter(subQuery);

				return new CusGuaranteeHeaderCollection(Factory, new ZString[] { GlbCompany.CurrentCompany.GC_RN_NKCountryCode }, Array.Empty<ZString>())
				{
					AdditionalFilter = query
				};
			}
		}

		public CodeDescriptionPairList ActivityCodeList => Factory.GetCachedValue("196f2cec-cdee-4036-a6eb-9aa2d8739506|ActivityCodeList", () =>
		{
			var result = new GuaranteeActivityCodeList();
			result.RemoveCode(GuaranteeActivityCodeList.Codes.ConsumeAndRelease);
			return result;
		});

		public CodeDescriptionPairList StatusList => Factory.GetCachedValue<GuaranteeStatusList>();
	}
}
