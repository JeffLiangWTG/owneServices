using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.EMCS.Business
{
	public class EMCSAddInfoJobDeclarationLookups : EU.EMCS.Business.EMCSAddInfoJobDeclarationLookups
	{
		public EMCSAddInfoJobDeclarationLookups(EU.EMCS.Business.EMCSAddInfoJobDeclaration parent) : base(parent)
		{
		}

		public override CodeDescriptionPairList DeferredSubmissionList => Factory.GetCachedValue<EmcsDeferredSubmissionList>();

		public override CodeDescriptionPairList GuarantorTypeList => Declaration.IsConsignor ? RefCusCodeListTypes.GetCachedListMatchSingleAttributeValues(Factory
			, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN
			, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EMCSGuarantorTypes
			, ZDateTime.Today
			, false
			, RefCusCodeListAttributeTypes.Codes.IsJointGuarantor, new ZString[] { YesNoList.Codes.No })
		: base.GuarantorTypeList;

		public override CodeDescriptionPairList OriginTypeList
		{
			get
			{
				CodeDescriptionPairList result;
				if (Declaration.IsConsolidatedDocument())
				{
					result = Factory.GetCachedValue("DE|EMCSOriginTypeList|DeclarationIsConsolidatedDocument", () =>
					{
						var list = new CodeDescriptionPairList();
						list.AddPair(EMCSOriginTypeList.Codes.TaxWarehouse, EMCSOriginTypeList.Descriptions.TaxWarehouse);
						return list;
					});
				}
				else
				{
					result = base.OriginTypeList;
				}
				return result;
			}
		}
		protected new EMCSJobDeclaration Declaration => (EMCSJobDeclaration)base.Declaration;
	}
}
