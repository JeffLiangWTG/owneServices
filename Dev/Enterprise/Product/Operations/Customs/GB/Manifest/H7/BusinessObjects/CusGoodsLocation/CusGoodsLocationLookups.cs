using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.GB.H7.Business
{
	public class CusGoodsLocationLookups : EU.Business.CusGoodsLocationLookups
	{
		public CusGoodsLocationLookups(EU.Business.CusGoodsLocation parent)
			: base(parent)
		{
		}

		protected new CusGoodsLocation Parent => (CusGoodsLocation)base.Parent;

		public override CodeDescriptionPairList QualifierList => Factory.GetCachedValue("GBH7.CusGoodsLocationLookups.QualifierList", () =>
			new CodeDescriptionPairList
			{
				new CodeDescriptionPair(CusGoodsLocationQualifierList.Codes.UnLocode, CusGoodsLocationQualifierList.Descriptions.UnLocode)
			});

		public override ICollection UnlocodeList
		{
			get
			{
				var result = ZZRefCusCodeListCombinedCollection.GetCachedCollection(
					Factory,
					Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services,
					Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port,
					ZDateTime.Today);

				var attributeValue = new ZString(Parent.CGL_Type + "U");
				result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Constants.ZZRefCusCodeListFilters.AttributeValue, "Property", attributeValue));

				return result;
			}
		}
	}
}
