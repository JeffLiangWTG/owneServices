using System.Collections.Generic;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class MConsignment03Provider : IMConsignment03
	{
		readonly EntryHeaderWrapper entryHeaderWrapper;

		public MConsignment03Provider(EntryHeaderWrapper entryHeaderWrapper)
		{
			this.entryHeaderWrapper = entryHeaderWrapper;
		}

		public decimal GrossMass => entryHeaderWrapper.EntryHeader.TotalGrossWeightInKG;

		public IReadOnlyCollection<IMTransportEquipment> TransportEquipments => transportEquipments ?? (transportEquipments = AISMessageProviderHelper.GetIM415TransportEquipments(entryHeaderWrapper.EntryHeader));
		public IReadOnlyCollection<IMTransportEquipment> transportEquipments;

		public IGoodsLocation LocationOfGoods => CachedValueHelper.GetValue(ref locationOfGoodsCached, () => new GoodsLocationProvider(entryHeaderWrapper.Instruction.GoodsLocation));
		CachedValue<IGoodsLocation> locationOfGoodsCached;
	}
}
