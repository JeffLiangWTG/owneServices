using System.Collections.Generic;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.MX.Manifest.Business
{
	public class AsycudaBillSynchroniser : ASYCUDA.Business.AsycudaBillSynchroniser
	{
		public AsycudaBillSynchroniser(AsycudaBill destination, ForwardingShipment shipmentSource)
			: base(destination, shipmentSource)
		{
		}

		protected override void HookFreightValueSynchronisers(List<ISynchroniser> synchronisers)
		{
			Synchronisers.Add(new FieldSynchroniser(Destination.ABL_TransportValueInfo, Source.JS_GoodsValueInfo));
			Synchronisers.Add(new FieldSynchroniser(Destination.ABL_RX_NKTransportValueCurrencyInfo, Source.JS_RX_NKGoodsValueCurrInfo));
		}
	}
}
