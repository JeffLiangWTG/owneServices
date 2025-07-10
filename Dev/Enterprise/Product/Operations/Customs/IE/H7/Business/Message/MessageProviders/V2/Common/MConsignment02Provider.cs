using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.H7.Business
{
	public class MConsignment02Provider : IMConsignment02
	{
		readonly AsycudaBill bill;

		public MConsignment02Provider(AsycudaBill bill)
		{
			this.bill = Argument.NotNull(bill, nameof(bill));
		}

		public IReadOnlyCollection<IMTransportEquipment> TransportEquipments => Array.Empty<IMTransportEquipment>();

		public IGoodsLocation LocationOfGoods => CachedValueHelper.GetValue(ref locationOfGoodsCached, () => new GoodsLocationProvider(bill.CusGoodsLocation));
		CachedValue<IGoodsLocation> locationOfGoodsCached;
	}
}
