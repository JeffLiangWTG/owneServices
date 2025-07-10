using System.Collections.Generic;

namespace Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders
{
	public interface INotifGoodsNCTSMessageDataProvider : IDepartureAndNotificationNCTSCommonMessageDataProvider
	{
		INCTSCommonTransitOperationLRN TransitOperation { get; }
		INCTSCommonHolderOfTheTransitProcedure HolderOfTheTransitProcedure { get; }
		INotifGoodsNCTSConsignment Consignment { get; }
	}

	public interface INotifGoodsNCTSConsignment : INCTSCommonDepartureAndNotifConsignment
	{
		IReadOnlyCollection<INCTSCommonHouseConsignmentSeqNum> HouseConsignment { get; }
	}
}
