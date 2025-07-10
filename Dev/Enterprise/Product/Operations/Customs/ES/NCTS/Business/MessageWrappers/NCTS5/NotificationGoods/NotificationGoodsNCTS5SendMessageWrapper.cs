using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public class NotificationGoodsNCTS5SendMessageWrapper : NCTS5DepartureAndNotificationCommonSendMessageWrapper, INotifGoodsNCTSMessageDataProvider
	{
		public NotificationGoodsNCTS5SendMessageWrapper(NctsHeader header, ICertificateProvider certificateData) : base(header, certificateData)
		{
		}

		public INCTSCommonTransitOperationLRN TransitOperation => transitOperation ?? (transitOperation = new NCTS5CommonTransitOperationLRNWrapper(nctsHeader));
		NCTS5CommonTransitOperationLRNWrapper transitOperation;

		public INCTSCommonHolderOfTheTransitProcedure HolderOfTheTransitProcedure => holderOfTheTransitProcedure ?? (holderOfTheTransitProcedure = NCTS5CommonHolderOfTheTransitProcedureWrapper.New(nctsHeader));
		NCTS5CommonHolderOfTheTransitProcedureWrapper holderOfTheTransitProcedure;

		public INotifGoodsNCTSConsignment Consignment => consignment ?? (consignment = new NotificationGoodsNCTS5ConsignmentWrapper(nctsHeader));
		NotificationGoodsNCTS5ConsignmentWrapper consignment;
	}
}
