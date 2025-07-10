using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;
using Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.NCTS.Business
{
	public class ESNctsMessageBuilderManager
	{
		public ESNctsMessageBuilderManager(NctsHeaderMessageSendingObject objectToSend, ICertificateProvider certificate)
		{
			sendingObject = Argument.NotNull(objectToSend, nameof(objectToSend));
			nctsHeader = Argument.NotNull(sendingObject.NctsHeader, nameof(sendingObject.NctsHeader));
			certificateData = Argument.NotNull(certificate, nameof(certificate));
			messageType = sendingObject.MessageType;
			messageSubType = sendingObject.MessageSubType;
		}

		public ESNctsMessageBuilderManager(NctsHeader header, ICertificateProvider certificate, ZString messageType)
		{
			nctsHeader = Argument.NotNull(header, nameof(header));
			certificateData = Argument.NotNull(certificate, nameof(certificate));
			this.messageType = messageType;
			messageSubType = DeclarationMessageSubTypeList.Codes.OriginalDeclaration;
		}

		readonly NctsHeaderMessageSendingObject sendingObject;
		readonly NctsHeader nctsHeader;
		readonly ICertificateProvider certificateData;
		readonly ZString messageType;
		readonly ZString messageSubType;

		public IMessageBuilderBase NewMessageBuilder()
		{
			switch (messageType)
			{
				case DeclarationMessageTypeList.Codes.NctsDeparture:
					return new DepartureMessageBuilder(new DepartureSendMessageWrapper(nctsHeader, certificateData), messageType, messageSubType);
				case DeclarationMessageTypeList.Codes.NctsTir:
					return new TIRMessageBuilder(new TIRSendMessageWrapper(nctsHeader, certificateData), messageType, messageSubType);
				case DeclarationMessageTypeList.Codes.NctsArrivalNotification:
				case DeclarationMessageTypeList.Codes.NctsUnloadingRemarks:
				case DeclarationMessageTypeList.Codes.NctsArrivalNotificationWithDepartureTnnPlusAvi:
				case DeclarationMessageTypeList.Codes.NctsArrivalNotificationWithUnloadingRemarksAviPlusObs:
				case DeclarationMessageTypeList.Codes.NctsArrivalNotificationWithDepartureAndUnloadingRemarksTnnPlusAviPlusOb:
					return new ArrivalMessageBuilder(new ArrivalSendMessageWrapper(nctsHeader, certificateData, messageType), messageType, messageSubType);

				case DeclarationMessageTypeList.Codes.Ncts5Departure:
				case DeclarationMessageTypeList.Codes.Ncts5DeparturePreDeclaration:
					return new DepartureNCTSMessageBuilder(new DepartureNCTS5SendMessageWrapper(nctsHeader, certificateData, messageType), messageType, messageSubType);

				case DeclarationMessageTypeList.Codes.Ncts5ArrivalNotification:
					return new ArrivalNCTSMessageBuilder(new ArrivalNCTS5SendMessageWrapper(nctsHeader, certificateData), messageType, messageSubType);
				case DeclarationMessageTypeList.Codes.Ncts5IndirectDepartureRegistration:
					return new TNNNCTSMessageBuilder(new TNNNCTS5SendMessageWrapper(nctsHeader, certificateData), messageType, messageSubType);
				case DeclarationMessageTypeList.Codes.Ncts5ArrivalDownloadGoods:
					return new NotifUnloadingNCTSMessageBuilder(new NotificationUnloadingNCTS5SendMessageWrapper(nctsHeader, certificateData), messageType, messageSubType);

				case DeclarationMessageTypeList.Codes.Ncts5DepartureNotification:
					return new NotifGoodsNCTSMessageBuilder(new NotificationGoodsNCTS5SendMessageWrapper(nctsHeader, certificateData), messageType, messageSubType);
				case DeclarationMessageTypeList.Codes.Ncts5DepartureAmendment:
					return new AmendmentNCTSMessageBuilder(new AmendmentNCTS5SendMessageWrapper(nctsHeader, certificateData, messageType), messageType, messageSubType);
				case DeclarationMessageTypeList.Codes.Ncts5DepartureCancellation:
					return new CancelNCTSMessageBuilder(new CancelNCTS5SendMessageWrapper(nctsHeader, certificateData, sendingObject.ReasonForCancellation), messageType, messageSubType);
				case DeclarationMessageTypeList.Codes.TransitNcts5Query:
					return new QueryNCTSMessageBuilder(new QueryNCTS5SendMessageWrapper(nctsHeader, certificateData), messageType, messageSubType);
				default:
					throw new NotImplementedException("CW1 doesn't yet support building message type " + messageType);
			}
		}

		public IMessageBuilderBase NewNCTSAnnexMessageBuilder(IEnumerable<NctsCusStorageDocPivot> docPivotList, ZString requestDispatch)
		{
			return new AnnexNCTSMessageBuilder(new AnnexNCTS5SendMessageWrapper(nctsHeader, certificateData, docPivotList, requestDispatch), messageType, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);
		}
	}
}
