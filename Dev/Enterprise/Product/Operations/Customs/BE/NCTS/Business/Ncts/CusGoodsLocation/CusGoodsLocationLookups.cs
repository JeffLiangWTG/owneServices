using System.Collections;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.BE.NCTS.Business;

public class CusGoodsLocationLookups : EU.NCTS.Business.CusGoodsLocationLookups
{
	public CusGoodsLocationLookups(CusGoodsLocation parent) : base(parent)
	{
	}

	public override CodeDescriptionPairList QualifierList => Factory.GetCachedValue("BE.CusGoodsLocationLookups.QualifierList_" + ((CusGoodsLocation)Parent).IsParentIncidentPhase5Arrival, () =>
	{
		var qualifierList = new CodeDescriptionPairList();
		qualifierList.AddPair(CusGoodsLocationQualifierList.Codes.UnLocode, CusGoodsLocationQualifierList.Descriptions.UnLocode);

		if (((CusGoodsLocation)Parent).IsParentIncidentPhase5Arrival)
		{
			qualifierList.AddPair(CusGoodsLocationQualifierList.Codes.GnssCoordinates, CusGoodsLocationQualifierList.Descriptions.GnssCoordinates);
			qualifierList.AddPair(CusGoodsLocationQualifierList.Codes.Address, CusGoodsLocationQualifierList.Descriptions.Address);
		}
		else
		{
			qualifierList.AddPair(CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier, CusGoodsLocationQualifierList.Descriptions.CustomsOfficeIdentifier);
		}

		return qualifierList;
	});

	public override CodeDescriptionPairList TypeList => Factory.GetCachedValue($"BE.CusGoodsLocationLookups.TypeList.{Parent.CGL_Qualifier}", () =>
	{
		var typeList = new CodeDescriptionPairList();

		if (Parent.CGL_Qualifier == CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier)
		{
			typeList.AddPair(CusGoodsLocationTypeList.Codes.DesignatedLocation, CusGoodsLocationTypeList.Descriptions.DesignatedLocation);
		}
		else if (Parent.CGL_Qualifier == CusGoodsLocationQualifierList.Codes.UnLocode)
		{
			typeList.AddPair(CusGoodsLocationTypeList.Codes.ApprovedPlace, CusGoodsLocationTypeList.Descriptions.ApprovedPlace);
		}

		return typeList;
	});

	public override ICollection UnlocodeList => ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.Belgium, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, CargoWise.Types.ZDateTime.Today);
}
