using Enterprise.BatchProcessor;
using Enterprise.Customs.Business.MessageProcessors.UCMP;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;

[assembly: UniversalCustomsInterchangeUnpacker(EDIInterchange.ApplicationCodes.IC2, typeof(Enterprise.Customs.EU.Manifest.ICS2.Business.ICS2InterchangeUnpacker))]

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public sealed class ICS2InterchangeUnpacker : IUniversalCustomsInterchangeUnpacker
	{
		public IUniversalCustomsInterchangeUnpackerResult Unpack(EDIInterchange interchange, EDIInterchange outgoingInterchange, EDIMessage outgoingMessage, LoggingInformation logger)
		{
			return (string)interchange.EI_InterchangeType switch
			{
				EUICS2InterchangeTypeList.Codes.MailboxRequest => new MailboxRequestUnpacker(logger).Unpack(interchange),
				Constant.MessageTypes.XER => XTFailureInterchangeHandler.Unpack(interchange, outgoingMessage),
				_ => new NormalAcknowledgementUnpacker().Unpack(interchange),
			};
		}
	}
}
