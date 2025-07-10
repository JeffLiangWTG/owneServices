using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class IM432HeaderProvider : IM413_414_415_432_433HeaderProvider, IIM432Header, IIM432Operation
	{
		public IM432HeaderProvider(AISMessageSendingAction sendingAction) : base(sendingAction)
		{
		}

		public IIM432Operation ImportOperation => this;

		public string LRN => entryHeader.CH_BGMReference;

		public string MRN => entryHeader.MovementReferenceNumber;

		IIM432GoodsShipment IIM432Header.GoodsShipment => CachedValueHelper.GetValue(ref goodsShipmentCached, () => new IM432GoodsShipmentProvider(entryHeaderWrapper));
		CachedValue<IIM432GoodsShipment> goodsShipmentCached;
	}
}
