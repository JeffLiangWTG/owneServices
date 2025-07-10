using System;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Business.NCTS;
using Enterprise.Customs.GB.CDS;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.GovernmentGateway.ServiceTasks.Ncts.Processors.Phase5
{
	public class NctsInboundInterchangeProcessorPhase5 : InboundInterchangeProcessor
	{
		public NctsInboundInterchangeProcessorPhase5(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override IInboundMessageCreator GetMessageCreator(EDIInterchange interchange) => new InboundMessageCreator(Logger);

		class InboundMessageCreator : IInboundMessageCreator
		{
			public InboundMessageCreator(LoggingInformation logger)
			{
				this.logger = logger;
			}

			void IInboundMessageCreator.CreateMessagesForInterchange(EDIInterchange interchange)
			{
				if (interchange is CDSInterchange cdsInterchange)
				{
					var customsBusinessResponse = cdsInterchange.GBCustomsBusinessResponse;
					Spawn<NCTSInboundEDIMessage>(interchange, customsBusinessResponse.ResponseBodyXml, customsBusinessResponse.ServiceReference, customsBusinessResponse.GetMessageSubType(logger));
				}
			}

			static T Spawn<T>(EDIInterchange interchange, ZString messageText, string serviceReference, ZString messageSubType) where T : GbEDIMessage
			{
				T message = null;
				if (!messageText.IsEmpty)
				{
					message = interchange.Factory.New<T>();
					message.EM_MessageText = messageText;
					message.EM_ApplicationReference = serviceReference;
					message.EM_MessageNum = serviceReference;
					message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
					message.EM_MessageSubType = messageSubType;

					interchange.ContainedMessages.Add(message);
				}
				return message;
			}

			readonly LoggingInformation logger;
		}

		protected override Type TypeOfInterchangeToCreate() => typeof(CDSInterchange);

		protected override string[] ApplicationCodes => new[] { EDIMessage.ApplicationCodes.GbCustomsNCTS };
	}
}
