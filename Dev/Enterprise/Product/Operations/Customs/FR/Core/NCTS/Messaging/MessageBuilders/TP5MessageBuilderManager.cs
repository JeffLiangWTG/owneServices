using CargoWise.Customs.FR.MessageContracts;
using CargoWise.Customs.FR.MessageDefinitions;
using CargoWise.Types;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.MessageSending;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.Customs.FR.NCTS.Messaging.TP5;

namespace Enterprise.Customs.FR.NCTS.Messaging
{
	public class TP5MessageBuilderManager : MessageBuilderManager<TP5MessageSendingObject>
	{
		public TP5MessageBuilderManager()
		{
		}

		public override ZString BuilderType => Business.MessageTypeList.Codes.TP5;

		public override IMessageBuilderBase NewMessageBuilder(TP5MessageSendingObject objectToSend)
		{
			switch (objectToSend.MessageType)
			{
				case TP5MessageTypeList.Codes.CC007C:
					return new TP5007MessageBuilder(CC007CWrapper.New((NctsHeader)objectToSend.NctsHeader));
				case TP5MessageTypeList.Codes.CC013C:
					return new TP5013MessageBuilder(CC013CWrapper.New(objectToSend));
				case TP5MessageTypeList.Codes.CC014C:
					return new TP5014MessageBuilder(CC014CWrapper.New(objectToSend));
				case TP5MessageTypeList.Codes.CC015C:
					return new TP5015MessageBuilder(CC015CWrapper.New(objectToSend));
				case TP5MessageTypeList.Codes.CC034C:
					return new TP5034MessageBuilder(CC034CWrapper.New(objectToSend));
				case TP5MessageTypeList.Codes.CC044C:
					return new TP5044MessageBuilder(CC044CWrapper.New((NctsHeader)objectToSend.NctsHeader));
				case TP5MessageTypeList.Codes.CC141C:
					return new TP5141MessageBuilder(CC141CWrapper.New(objectToSend));
				case TP5MessageTypeList.Codes.CC170C:
					return new TP5170MessageBuilder(CC170CWrapper.New(objectToSend));
				default:
					return null;
			}
		}
	}
}
