using System.Collections;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IE.Business.CusTempStorage
{
	public class TemporaryStorageAdditionalInfoLookups : EU.Business.CusTempStorage.TemporaryStorageAdditionalInfoLookups
	{
		public TemporaryStorageAdditionalInfoLookups(EU.Business.CusTempStorage.TemporaryStorageAdditionalInfo parent) : base(parent)
		{
		}

		public override CodeDescriptionPairList SubTypeList => Factory.GetCachedValue<AdditionalInfoSubTypeList>();

		public override ICollection CodeList
		{
			get
			{
				switch (Parent.CSI_SubType)
				{
					case AdditionalInfoSubTypeList.Codes.AdditionalInformation:
						return ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.Ireland, EU.NCTS.Business.UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_AI44N, ZDateTime.Today);
					case AdditionalInfoSubTypeList.Codes.AdditionalReference:
						return ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.Ireland, EU.NCTS.Business.UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_AR44N, ZDateTime.Today);
					case AdditionalInfoSubTypeList.Codes.TransportDocument:
						return ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TransportDocumentTemporaryStorage, ZDateTime.Today);
					default:
						return base.CodeList;
				}
			}
		}
	}
}
