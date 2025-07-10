using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using static Enterprise.Customs.EU.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.ES.Manifest.Business
{
	public class SupportingDocumentLookups : CusSupportingInfoLookups
	{
		public SupportingDocumentLookups(AutoCusSupportingInfo parent) : base(parent)
		{
		}

		public ZZRefCusCodeListCombinedCollection TypeCodeList => ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.Spain, RefCusCodeListType.Code.SupportingDocumentOfNCTS, ZDateTime.Today);
	}
}
