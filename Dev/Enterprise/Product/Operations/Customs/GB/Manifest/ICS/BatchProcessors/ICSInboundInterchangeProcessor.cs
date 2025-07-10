using System;
using CargoWise.Customs.GB.MessageDefinitions;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.ICS
{
	public class ICSInboundInterchangeProcessor : InboundInterchangeProcessor
	{
		public ICSInboundInterchangeProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override string[] ApplicationCodes => new[] { EDIMessage.ApplicationCodes.GbMessageICSGreatBritain, EDIMessage.ApplicationCodes.GbMessageICSNorthernIreland };

		protected override Type TypeOfInterchangeToCreate() => typeof(ICSInterchange);

		protected override IInboundMessageCreator GetMessageCreator(EDIInterchange interchange) => new InboundMessageCreator(Logger);

		class InboundMessageCreator : IInboundMessageCreator
		{
			public InboundMessageCreator(LoggingInformation logger) => this.logger = logger;
			readonly LoggingInformation logger;

			void IInboundMessageCreator.CreateMessagesForInterchange(EDIInterchange interchange)
			{
				if (interchange is ICSInterchange icsInterchange)
				{
					var customsBusinessResponse = icsInterchange.GBCustomsBusinessResponse;
					var provider = customsBusinessResponse.Provider;
					var body = customsBusinessResponse.ResponseBodyXml;
					var correlation = customsBusinessResponse.CorrelationId;
					var subType = customsBusinessResponse.GetMessageSubType(logger);
					if (provider == nameof(ProviderType.ICSGB)) // I think Grace moved this to the Customs repo recently
					{
						Spawn<IcsSsGreatBritainEDIMessage>(interchange, body, correlation, subType);
					}
					else if (provider == nameof(ProviderType.ICSNI)) // I think Grace moved this to the Customs repo recently
					{
						Spawn<IcsNorthernIrelandEDIMessage>(interchange, body, correlation, subType);
					}
				}
			}

			static T Spawn<T>(EDIInterchange interchange, ZString messageText, string correlationId, ZString messageSubType) where T : GbEDIMessage
			{
				T message = null;
				if (!messageText.IsEmpty)
				{
					message = interchange.Factory.New<T>();
					message.EM_MessageText = messageText;
					message.EM_ApplicationReference = correlationId;
					message.EM_MessageNum = interchange.EI_InterchangeNum + "/" + correlationId;
					message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
					message.EM_MessageType = messageSubType;
					message.EM_MessageSubType = messageSubType;
					interchange.ContainedMessages.Add(message);
				}
				return message;
			}
		}
	}
}
