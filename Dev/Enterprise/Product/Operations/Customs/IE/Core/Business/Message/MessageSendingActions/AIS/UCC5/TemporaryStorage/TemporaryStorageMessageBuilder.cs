using CargoWise.Customs.IE.MessageContracts.AIS.UCC5;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.AIS.UCC5.CusTempStorage;

namespace Enterprise.Customs.IE.Business.CusTempStorage.UCC5
{
	public sealed class TemporaryStorageMessageBuilder : CusTempStorage.TemporaryStorageMessageBuilder
	{
		public TemporaryStorageMessageBuilder(TemporaryStorageMessageSendingObject messageSendingObject, EU.Business.CusTempStorage.TemporaryStorageMessageFunction messageFunction)
			: base(messageSendingObject, messageFunction)
		{
		}

		protected override ZString GetApplicationCode() => EDIMessage.ApplicationCodes.IECustomsUCC5Import;

		protected override IXmlMessageBuilder GetMessageBuilder(EU.Business.ErrorCollector errorCollector)
		{
			IXmlMessageBuilder result = null;
			if (Header is TemporaryStorageHeader)
			{
				var messageType = MessageFunction.MessageType;
				switch (messageType)
				{
					/// **********************
					/// Uncomment when message builders and providers are complete for UCC5
					/// **********************
					case IETemporaryStorageMessageTypeList.Codes.Declaration:
						result = new TS315MessageBuilder(new TS315MessageProvider(MessageSendingObject));
						break;
					case IETemporaryStorageMessageTypeList.Codes.Invalidation:
						result = new TS314MessageBuilder(new TS314MessageProvider(MessageSendingObject));
						break;
					case IETemporaryStorageMessageTypeList.Codes.Amendment:
						result = new TS313MessageBuilder(new TS313MessageProvider(MessageSendingObject));
						break;
					case IETemporaryStorageMessageTypeList.Codes.PresentationNotification:
						result = new TS332MessageBuilder(new TS332MessageProvider(MessageSendingObject));
						break;
					//case IETemporaryStorageMessageTypeList.Codes.GoodsStatusReportDeclaration:
					default:
						errorCollector.AddError(Res.GetString("94E27F49-D85A-4F81-8956-22A1CDEA905D", "Message Type '{0}' is not currently supported for V1 (UCC5).", messageType), true);
						break;
				}
			}
			return result;
		}
	}
}
