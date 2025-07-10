using CargoWise.Customs.FR.MessageContracts.MessageBuilders;
using CargoWise.Customs.FR.MessageContracts.MessageBuilders.PNTS;
using CargoWise.Customs.FR.MessageDefinitions;
using CargoWise.Types;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.Customs.FR.Business.EdiMessages;
using Enterprise.Customs.FR.Business.MessageProcessors;
using Enterprise.Customs.FR.Business.MessagesWrappers.PNTS;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.FR.Business.MessageSending
{
	public class TemporaryStorageMessageBuilder : EU.Business.CusTempStorage.TemporaryStorageMessageBuilder
	{
		public TemporaryStorageMessageBuilder(TemporaryStorageMessageSendingObject messageSendingObject, TemporaryStorageMessageFunction messageFunction)
			: base(messageSendingObject, messageFunction)
		{
		}

		protected override ZString GetApplicationCode() => EDIMessage.ApplicationCodes.FRCustomsMessage;

		protected override IMessageNumberStrategy GetMessageNumberStrategy(BuilderResult builderResult) => new FRMessageNumberStrategy(builderResult.Message.Factory, FREDIMessage.ApplicationCodes.FRCustomsMessage);

		protected override ZString GetMessageOwner(TemporaryStorageHeader header) => header.Branch.Company.GC_CustomsRegistrationNo.Left(EDIMessage.Schema.EM_MessageOwnerMaxLength);

		protected override ZString GetMessageText(ErrorCollector errorCollector)
		{
			var messageBuilder = GetMessageBuilder(Header, MessageFunction.MessageType);
			if (messageBuilder != null)
			{
				return messageBuilder.GetMessage();
			}
			else
			{
				return ZString.Empty;
			}
		}

		IMessageBuilderBase GetMessageBuilder(TemporaryStorageHeader header, ZString messageType)
		{
			switch (messageType)
			{
				case TemporaryStorageMessageTypeList.Codes.PresentationNotification:
					var iets007Wrapper = IETS007Wrapper.New(header);
					return new IETS007MessageBuilder(iets007Wrapper);
				case TemporaryStorageMessageTypeList.Codes.PreLodgedTSD:
					var iets015Wrapper = IETS015Wrapper.New(header);
					return new IETS015MessageBuilder(iets015Wrapper);
				case TemporaryStorageMessageTypeList.Codes.CombinedTSD:
					var iets115Wrapper = IETS115Wrapper.New(header);
					return new IETS115MessageBuilder(iets115Wrapper);
				case TemporaryStorageMessageTypeList.Codes.TransferNotificationTSD:
					return null;
				case TemporaryStorageMessageTypeList.Codes.DeconsolidationNotificationTSD:
					return null;
				case TemporaryStorageMessageTypeList.Codes.AmendmentRequestTSD:
					var iets413Wrapper = IETS413Wrapper.New(header);
					return new IETS413MessageBuilder(iets413Wrapper);
				case TemporaryStorageMessageTypeList.Codes.InvalidationRequestTSD:
					var iets414Wrapper = IETS414Wrapper.New(header, (InvalidationRequestTSDMessageFunction)MessageFunction);
					return new IETS414MessageBuilder(iets414Wrapper);
				default:
					return null;
			}
		}
	}
}
