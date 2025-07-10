using Enterprise.Customs.KR.Messaging;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.KR.Business
{
	public class ParcelLookups : Customs.Business.CusSupportingInfoLookups
	{
		public ParcelLookups(Parcel parent)
			: base(parent)
		{
		}

		public CodeDescriptionPairList DeliveryTypeCodeList => Factory.GetCachedValue<DeliveryTypeCodeList>();
	}
}
