using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public class NotificationUnloadingNCTS5TransitOperationWrapper : NCTS5CommonTransitOperationMRNWrapper, INotifUnloadingNCTSTransitOperation
	{
		public NotificationUnloadingNCTS5TransitOperationWrapper(NctsHeader header) : base(header)
		{
			arrivalMovement = Argument.NotNull(nctsHeader.ArrivalMovementHeader, nameof(nctsHeader.ArrivalMovementHeader));
		}
		protected readonly NctsArrivalMovementHeader arrivalMovement;

		public ZString OtherThingsToReport => arrivalMovement.OtherThingsToReport;
	}
}
