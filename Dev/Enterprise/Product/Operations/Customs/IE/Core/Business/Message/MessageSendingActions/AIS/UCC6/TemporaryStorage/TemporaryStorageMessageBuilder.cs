using CargoWise.Customs.IE.MessageContracts.AIS;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.AIS.CusTempStorage;

namespace Enterprise.Customs.IE.Business.CusTempStorage.UCC6
{
	public sealed class TemporaryStorageMessageBuilder : CusTempStorage.TemporaryStorageMessageBuilder
	{
		public TemporaryStorageMessageBuilder(TemporaryStorageMessageSendingObject messageSendingObject, EU.Business.CusTempStorage.TemporaryStorageMessageFunction messageFunction) : base(messageSendingObject, messageFunction)
		{
		}

		protected override ZString GetApplicationCode() => EDIMessage.ApplicationCodes.IECustomsImport;

		protected override IXmlMessageBuilder GetMessageBuilder(EU.Business.ErrorCollector errorCollector)
		{
			IXmlMessageBuilder result = null;
			if (Header is TemporaryStorageHeader)
			{
				var messageType = MessageFunction.MessageType;
				switch (messageType)
				{
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
						errorCollector.AddError(Res.GetString("E229AC38-FACB-43BD-B908-4553C384E34D", "Message Type '{0}' is not currently supported.", messageType), true);
						break;
				}
			}
			return result;
		}
	}
}
