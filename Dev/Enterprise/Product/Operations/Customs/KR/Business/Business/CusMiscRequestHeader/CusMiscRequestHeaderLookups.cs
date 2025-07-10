using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;
using Constants = Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	public class CusMiscRequestHeaderLookups : Customs.Business.CusMiscRequestHeaderLookups
	{
		public CusMiscRequestHeaderLookups(AutoCusMiscRequestHeader parent)
			: base(parent)
		{
		}

		public CodeDescriptionPairList MessageTypeList => Factory.GetCachedValue<ElectronicDocumentTypeList>();
		public CodeDescriptionPairList CustomsMessageStatusTypeList => Factory.GetCachedValue<CustomsMessageStatusTypeList>();
		public CodeDescriptionPairList CustomsEntryStatusTypeList => Factory.GetCachedValue<CustomsEntryStatusTypeList>();
		public ZZRefCusCodeListCombinedCollection CustomsOfficeList => ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.KoreaSouth, Constants.ZZ.NKCodeType.CustomsOffice, ZDateTime.Today);
		public ZZRefCusCodeListCombinedCollection CustomsDivisionList => ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.KoreaSouth, Constants.ZZ.NKCodeType.CustomsDepartment, ZDateTime.Today);
	}
}
