using System.Collections;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.Universal.RefCusCodeListTypes;
using UniversalReferenceConstants = Enterprise.Customs.IE.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.IE.H7.Business
{
	public class CusGoodsLocationLookups : EU.Business.CusGoodsLocationLookups
	{
		public CusGoodsLocationLookups(EU.Business.CusGoodsLocation parent)
			: base(parent)
		{
		}

		protected new CusGoodsLocation Parent => (CusGoodsLocation)base.Parent;

		public override CodeDescriptionPairList QualifierList => Factory.GetCachedValue("IEH7.CusGoodsLocationLookups.QualifierList", () =>
		{
			var qualifierList = new CusGoodsLocationQualifierList();
			qualifierList.RemoveCode(CusGoodsLocationQualifierList.Codes.PostcodeAddress);
			qualifierList.RemoveCode(CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier);
			qualifierList.RemoveCode(CusGoodsLocationQualifierList.Codes.GnssCoordinates);
			qualifierList.RemoveCode(CusGoodsLocationQualifierList.Codes.EoriNumber);
			qualifierList.RemoveCode(CusGoodsLocationQualifierList.Codes.AuthorizationNumber);
			qualifierList.RemoveCode(CusGoodsLocationQualifierList.Codes.Address);
			return qualifierList;
		});

		public override CodeDescriptionPairList TypeList => GetCachedList(Parent.Factory, Core.Constants.CountryCodes.Ireland,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.GoodsOfLocationType, ZDateTime.Today, includeParentDataGrouping: false);

		public override ICollection UnlocodeList => ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.Ireland, UniversalReferenceConstants.RefCusCodeListTypes.Codes.GoodsLocation, ZDateTime.Today);
	}
}
