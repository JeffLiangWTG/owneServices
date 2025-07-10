using System.Collections.Generic;
using Enterprise.Customs.ES.NCTS.Business;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;
using Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class NotificationGoodsNCTS5ConsignmentWrapper : NCTS5CommonDepartureAndNotifConsignmentWrapper, INotifGoodsNCTSConsignment
	{
		public NotificationGoodsNCTS5ConsignmentWrapper(NctsHeader header) : base(header)
		{
		}

		public IReadOnlyCollection<INCTSCommonHouseConsignmentSeqNum> HouseConsignment => houseConsignment ?? (houseConsignment = new List<NCTS5CommonHouseConsignmentSeqNumWrapper>());
		IReadOnlyCollection<NCTS5CommonHouseConsignmentSeqNumWrapper> houseConsignment;
	}
}
