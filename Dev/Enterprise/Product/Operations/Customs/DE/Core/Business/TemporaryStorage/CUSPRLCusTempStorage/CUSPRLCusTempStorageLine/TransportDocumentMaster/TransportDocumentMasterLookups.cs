using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public class TransportDocumentMasterLookups : CusSupportingInfoLookups
	{
		public TransportDocumentMasterLookups(TransportDocumentMaster parent) : base(parent)
		{
		}

		public ZZRefCusCodeListCombinedCollection TransportNumberTypeList => ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory,
			Core.Constants.CountryCodes.Germany, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_C0754, ZDateTime.Now);
	}
}
