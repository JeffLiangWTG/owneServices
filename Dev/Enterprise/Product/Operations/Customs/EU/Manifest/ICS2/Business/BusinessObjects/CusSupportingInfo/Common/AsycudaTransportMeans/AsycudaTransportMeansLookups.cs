using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using UniversalReferenceConstants = Enterprise.Customs.EU.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class AsycudaTransportMeansLookups : CusTransportMeansLookups
	{
		public AsycudaTransportMeansLookups(CusTransportMeans parent) : base(parent)
		{
		}

		public CodeDescriptionPairList TypesTransportOfIdentification => Factory.GetCachedValue<EUICS2ModeOfTransportIdentifierTypeList>();

		public CodeDescriptionPairList MeansOfTransportTypeList => RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, UniversalReferenceConstants.RefCusCodeListType.Code.Code_IC2MT, ZDateTime.Today);

		public ZZRefCusCodeListCombinedCollection CountryList => GetEunNC008Collection();

		ZZRefCusCodeListCombinedCollection GetEunNC008Collection()
		{
			var list = ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, UniversalReferenceConstants.RefCusCodeListType.Code.Code_NC008, ZDateTime.Today);
			list.Load();
			list.Sort(RefCusCodeListSchema.Constants.ZZD_Code);
			return list;
		}
	}
}
