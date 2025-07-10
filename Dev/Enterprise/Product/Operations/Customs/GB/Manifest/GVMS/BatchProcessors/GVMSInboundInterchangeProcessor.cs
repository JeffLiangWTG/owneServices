using System;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.GB.CDS;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.GVMS
{
	public class GVMSInboundInterchangeProcessor : InboundInterchangeProcessor
	{
		public GVMSInboundInterchangeProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override IInboundMessageCreator GetMessageCreator(EDIInterchange interchange) => new InboundMessageCreator();

		class InboundMessageCreator : IInboundMessageCreator
		{
			void IInboundMessageCreator.CreateMessagesForInterchange(EDIInterchange interchange)
			{
				if (interchange is CDSInterchange cdsInterchange)
				{
					var customsBusinessResponse = cdsInterchange.GBCustomsBusinessResponse;
					Spawn<GVMSEDIMessage>(interchange, customsBusinessResponse.ResponseBodyJson, customsBusinessResponse.MessageId);
				}
			}

			static T Spawn<T>(EDIInterchange interchange, ZString messageText, ZString messageID) where T : GVMSEDIMessage
			{
				var message = interchange.Factory.New<T>();
				message.EM_MessageText = messageText;
				message.EM_ApplicationReference = messageID.KeepAlphanumericCharacters();
				message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
				message.EM_MessageInterpretation = new GVMSEDIMessagePrettier(message).MakeHumanReadable();
				interchange.ContainedMessages.Add(message);
				return message;
			}
		}

		protected override Type TypeOfInterchangeToCreate() => typeof(CDSInterchange);

		protected override string[] ApplicationCodes => new[] { EDIMessage.ApplicationCodes.GbCustomsGVMSManifest };
	}
}
