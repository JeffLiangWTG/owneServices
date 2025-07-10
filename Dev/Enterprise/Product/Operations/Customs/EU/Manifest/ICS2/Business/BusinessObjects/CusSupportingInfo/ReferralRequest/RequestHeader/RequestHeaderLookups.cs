using CargoWise.EntityFramework;
using Enterprise.Customs.ManifestBase;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class RequestHeaderLookups : EUMemberStateCommunicationLookups
	{
		public RequestHeaderLookups(AutoEUMemberStateCommunication parent) : base(parent)
		{
		}

		public CodeDescriptionPairList RequestTypeList => CachedValueHelper.GetValue(ref requestTypeList, () => Factory.GetCachedValue<EUICS2ReferralRequestTypeList>());
		CachedValue<CodeDescriptionPairList> requestTypeList;

		public CodeDescriptionPairList TransportDocumentTypeList => CachedValueHelper.GetValue(ref transportDocumentTypeList, () => Factory.GetCachedValue<ASYCUDA.Business.TransportDocumentTypes>());
		CachedValue<CodeDescriptionPairList> transportDocumentTypeList;

		public IBusinessObjectCollection HouseBillList => ((AsycudaManifestHeader)Parent.Parent).Bills;

		public new RequestHeader Parent => (RequestHeader)base.Parent;
	}
}
