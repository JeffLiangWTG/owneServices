using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.CO.Manifest.Business
{
	public class AsycudaBillSynchroniser : ASYCUDA.Business.AsycudaBillSynchroniser
	{
		public AsycudaBillSynchroniser(AsycudaBill destination, ForwardingShipment shipmentSource)
			: base(destination, shipmentSource)
		{
		}

		protected override void HookSynchronisers()
		{
			base.HookSynchronisers();
			Synchronisers.Add(new FieldSynchroniser(Destination.ABL_OA_GoodsLocationInfo, Source.JS_OA_ImportReleaseDepotInfo));
		}

		protected override void HookFreightValueSynchronisers(List<ISynchroniser> synchronisers)
		{
			Synchronisers.Add(new FieldSynchroniser(Destination.ABL_GoodsValueInfo, Source.JS_GoodsValueInfo));
			Synchronisers.Add(new FieldSynchroniser(Destination.ABL_RX_NKGoodsValueCurrencyInfo, () => GetGoodsValueCurrency(), () => GetGoodsValueCurrencyInfo()));
		}

		IZType GetGoodsValueCurrency()
		{
			return !Source.JS_RX_NKGoodsValueCurr.IsEmpty ? Source.JS_RX_NKGoodsValueCurr : (ZString)CurrencyCodes.UnitedStates;
		}

		IEnumerable<ZPropertyInfo> GetGoodsValueCurrencyInfo()
		{
			return new ZPropertyInfo[] { Source.JS_RX_NKGoodsValueCurrInfo };
		}
	}
}
