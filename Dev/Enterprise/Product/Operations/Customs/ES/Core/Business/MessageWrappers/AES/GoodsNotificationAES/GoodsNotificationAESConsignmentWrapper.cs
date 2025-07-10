using System.Collections.Generic;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class GoodsNotificationAESConsignmentWrapper : AESCommonConsignmentWrapper, IGoodsNotificationAESConsignment
	{
		public GoodsNotificationAESConsignmentWrapper(CusEntryHeader entryHeader) : base(entryHeader, false)
		{
		}

		public IReadOnlyCollection<IAESCommonTransportEquipment> TransportEquipment => transportEquipment ?? (transportEquipment = AESCommonTransportEquipmentWrapper.GetTransportEquipmentList(entryHeader));
		IReadOnlyCollection<AESCommonTransportEquipmentWrapper> transportEquipment;

		public IAESCommonLocationOfGoods LocationOfGoods => locationOfGoods ?? (locationOfGoods = new AESCommonLocationOfGoodsWrapper(entryInstruction));
		AESCommonLocationOfGoodsWrapper locationOfGoods;

		public IReadOnlyCollection<ICommonDepartureTransportMeans> DepartureTransportMeans => departureTransportMeans ?? (departureTransportMeans = GetDepartureTransportMeans());
		IReadOnlyCollection<CommonDepartureTransportMeansWrapper> departureTransportMeans;
	}
}
