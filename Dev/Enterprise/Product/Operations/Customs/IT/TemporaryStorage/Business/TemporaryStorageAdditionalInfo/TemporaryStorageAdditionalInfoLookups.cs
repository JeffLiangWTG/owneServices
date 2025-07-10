using System.Collections;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.TemporaryStorage.Business;

sealed class TemporaryStorageAdditionalInfoLookups : EU.Business.CusTempStorage.TemporaryStorageAdditionalInfoLookups
{
	public TemporaryStorageAdditionalInfoLookups(EU.Business.CusTempStorage.TemporaryStorageAdditionalInfo parent) : base(parent)
	{
	}

	public override CodeDescriptionPairList SubTypeList
	{
		get
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(AdditionalInfoSubTypeList.Codes.AdditionalInformation, AdditionalInfoSubTypeList.Descriptions.AdditionalInformation);
			return result;
		}
	}

	public override ICollection CodeList
	{
		get
		{
			var temporaryStorageHeader = Parent.TemporaryStorageHeader;
			var dataGrouping = temporaryStorageHeader?.AMA_RN_NKCountry ?? Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN;
			var subType = Parent.CSI_SubType;
			switch (subType)
			{
				case AdditionalInfoSubTypeList.Codes.AdditionalInformation:
					return ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, dataGrouping, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, ZDateTime.Today);

				default:
					return new CodeDescriptionPairList();
			}
		}
	}
}
