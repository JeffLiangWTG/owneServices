using CargoWise.Customs.FR.MessageContracts;
using CargoWise.Customs.FR.MessageDefinitions;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE;

namespace Enterprise.Customs.FR.Business.MessageSending
{
	public class DeltaIEMessageBuilderManager : MessageBuilderManager<DeltaIEJobDeclarationMessageSendingObject>
	{
		public DeltaIEMessageBuilderManager()
		{
		}

		public override ZString BuilderType => DeltaIEMessageTypes.Codes.DEC;

		public override IMessageBuilderBase NewMessageBuilder(DeltaIEJobDeclarationMessageSendingObject objectToSend)
		{
			switch (objectToSend.MessageType + objectToSend.WrapperModifier)
			{
				case DeltaIESendMessageSubTypeList.Codes.ImportDeclaration:
					return new CC415MessageBuilder(CC415BWrapper.New(objectToSend.Header));
				case DeltaIESendMessageSubTypeList.Codes.Invalidation:
					return new CC414MessageBuilder(CC414BWrapper.New(objectToSend));
				case DeltaIESendMessageSubTypeList.Codes.Invalidation + DeltaIEJobDeclarationMessageSendingObject.Schema.ForOperationalAction:
					return new CC414MessageBuilder(CC414BWrapper.New(objectToSend, true));
				case DeltaIESendMessageSubTypeList.Codes.PresentationNotification:
					return new CC432MessageBuilder(CC432BWrapper.New(objectToSend.Header));
				case DeltaIESendMessageSubTypeList.Codes.AmendmentRequest:
					return new CC413MessageBuilder(CC413BWrapper.New(objectToSend));
				case DeltaIESendMessageSubTypeList.Codes.AmendmentRequest + DeltaIEJobDeclarationMessageSendingObject.Schema.ForOperationalAction:
					return new CC413MessageBuilder(CC413BWrapper.New(objectToSend, true));
				default:
					return null;
			}
		}
	}
}
