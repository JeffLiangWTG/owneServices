using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public class NotificationUnloadingNCTS5SendMessageWrapper : NCTS5CommonSendMessageWrapper, INotifUnloadingNCTSMessageDataProvider
	{
		public NotificationUnloadingNCTS5SendMessageWrapper(NctsHeader header, ICertificateProvider certificateData) : base(header, certificateData)
		{
			arrivalMovement = Argument.NotNull(nctsHeader.ArrivalMovementHeader, nameof(nctsHeader.ArrivalMovementHeader));
		}
		protected readonly NctsArrivalMovementHeader arrivalMovement;

		public INotifUnloadingNCTSTransitOperation TransitOperation => transitOperation ?? (transitOperation = new NotificationUnloadingNCTS5TransitOperationWrapper(nctsHeader));
		NotificationUnloadingNCTS5TransitOperationWrapper transitOperation;

		public ZString CustomsOfficeOfDestinationActual => arrivalMovement.DestinationCustomsOfficeCodeForArrival;

		public IPartyIdProvider TraderAtDestination => traderAtDestination ?? (traderAtDestination = NCTS5CommonTraderAtDestinationWrapper.New(nctsHeader.DestinationTrader));
		NCTS5CommonTraderAtDestinationWrapper traderAtDestination;

		public IPartyIdProvider RepresentativeAtDestination => representativeAtDestination ?? (representativeAtDestination = NCTS5CommonTraderAtDestinationWrapper.New(arrivalMovement.Representative));
		NCTS5CommonTraderAtDestinationWrapper representativeAtDestination;

		public IUnloadingRemarksNCTS UnloadingRemarks => unloadingRemarks ?? (unloadingRemarks = new NotificationUnloadingNCTS5UnloadingRemarksWrapper(arrivalMovement));
		NotificationUnloadingNCTS5UnloadingRemarksWrapper unloadingRemarks;

		public INotifUnloadingConsignment Consignment => consignment ?? (consignment = arrivalMovement.BM_NoChangesToReport ? null : new NotificationUnloadingNCTS5ConsignmentWrapper(nctsHeader));
		NotificationUnloadingNCTS5ConsignmentWrapper consignment;
	}
}
