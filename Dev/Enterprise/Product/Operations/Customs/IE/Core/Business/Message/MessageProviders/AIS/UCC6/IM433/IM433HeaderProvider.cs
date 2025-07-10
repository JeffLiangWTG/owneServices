using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class IM433HeaderProvider : IM413_414_415_432_433HeaderProvider, IIM433Header, IIM433ImportOperation
	{
		public IM433HeaderProvider(AISMessageSendingAction sendingAction) : base(sendingAction)
		{
		}

		public IIM433ImportOperation ImportOperation => this;

		public string LRN => entryHeader.CH_BGMReference;

		public string MRN => entryHeader.MovementReferenceNumber;

		IM433GoodsShipment IIM433Header.GoodsShipment => CachedValueHelper.GetValue(ref goodsShipmentCached, () => new IM433GoodsShipmentProvider(entryHeaderWrapper));
		CachedValue<IM433GoodsShipment> goodsShipmentCached;
	}
}
