using Enterprise.BatchProcessor;
using Enterprise.Customs.Business.MessageProcessors.UCMP;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;

[assembly: UniversalCustomsInterchangeUnpacker(EDIInterchange.ApplicationCodes.UnitedArabEmirates, typeof(Enterprise.Customs.AE.Business.AEInterchangeUnpacker))]

namespace Enterprise.Customs.AE.Business;

public sealed class AEInterchangeUnpacker : IUniversalCustomsInterchangeUnpacker
{
	public IUniversalCustomsInterchangeUnpackerResult Unpack(EDIInterchange interchange, EDIInterchange outgoingInterchange, EDIMessage outgoingMessage, LoggingInformation logger)
	{
		var interchangeType = interchange.EI_InterchangeType;
		return (string)interchangeType switch
		{
			AEConstants.Messaging.MessageTypes.CONTRL => new NAICInterchangeUnpacker().Unpack(interchange),
			AEConstants.Messaging.MessageTypes.CUSRES => new NAICInterchangeUnpacker().Unpack(interchange),
			AEConstants.Messaging.MessageTypes.DOCSUC => new DocInterchangeUnpacker().Unpack(interchange, outgoingMessage),
			AEConstants.Messaging.MessageTypes.DOCERR => new DocInterchangeUnpacker().Unpack(interchange, outgoingMessage),
			AEConstants.Messaging.MessageTypes.XTTERR => XTFailureInterchangeHandler.Unpack(interchange, outgoingMessage),
			_ => new EDIInterchangeUnpackerResult($"Unknown interchange type {interchangeType}."),
		};
	}
}
