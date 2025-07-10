using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.BE.Business;

public sealed class PNTSCusGoodsLocationLookups : EU.Business.CusGoodsLocationLookups
{
	public PNTSCusGoodsLocationLookups(EU.Business.CusGoodsLocation parent) : base(parent)
	{
	}

	new CusGoodsLocation Parent => (CusGoodsLocation)base.Parent;

	public override CodeDescriptionPairList QualifierList
	{
		get
		{
			return Factory.GetCachedValue("TemporaryStorageGoodsLocationQualifierList", () =>
			{
				var result = new CusGoodsLocationQualifierList();
				result.RemoveCode(CusGoodsLocationQualifierList.Codes.PostcodeAddress);
				result.RemoveCode(CusGoodsLocationQualifierList.Codes.Address);
				return result;
			});
		}
	}

	public override CodeDescriptionPairList TypeList
	{
		get
		{
			var qualifier = Parent.CGL_Qualifier.ToUpper();
			if (!qualifier.Equals(CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier))
			{
				return Factory.GetCachedValue($"TemporaryStorageGoodsLocationTypeList_{qualifier}", () =>
				{
					var result = new CusGoodsLocationTypeList();
					result.RemoveCode(CusGoodsLocationTypeList.Codes.AuthorizedPlace);
					result.RemoveCode(CusGoodsLocationTypeList.Codes.ApprovedPlace);
					if (!qualifier.Equals(CusGoodsLocationQualifierList.Codes.UnLocode))
					{
						result.RemoveCode(CusGoodsLocationTypeList.Codes.DesignatedLocation);
					}
					return result;
				});
			}
			else
			{
				return base.TypeList;
			}
		}
	}
}
