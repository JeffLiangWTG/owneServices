using System.Collections;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Business.CusTempStorage
{
	public class TemporaryStorageAdditionalInfoLookups : AdditionalInfoLookups
	{
		public TemporaryStorageAdditionalInfoLookups(TemporaryStorageAdditionalInfo parent) : base(parent)
		{
		}

		public new TemporaryStorageAdditionalInfo Parent => (TemporaryStorageAdditionalInfo)base.Parent;

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
						return ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, dataGrouping, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AI44T, ZDateTime.Today);
					case AdditionalInfoSubTypeList.Codes.AdditionalReference:
						return ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, dataGrouping, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AR44T, ZDateTime.Today);
					case AdditionalInfoSubTypeList.Codes.TransportDocument:
						return ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, dataGrouping, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TransportDocumentTemporaryStorage, ZDateTime.Today);
					default:
						return new CodeDescriptionPairList();
				}
			}
		}

		public override CodeDescriptionPairList SubTypeList => Factory.GetCachedValue("EU.TemporaryStorageAdditionalInfoLookups.SubTypeList", () => new AdditionalInfoSubTypeList());
	}
}
