using CargoWise.Types;
using Enterprise.Integration.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;

namespace Enterprise.Customs.IL.Business
{
	public interface IFeedbackMessageUnpacker
	{
		IUniversalCustomsInterchangeUnpackerResult Unpack(EDIInterchange interchange, ZString bodyText, ZString responseHeaderText, ZString correlationId, EDIInterchange outgoingInterchange, EDIMessage outgoingMessage, ILoggingInformation logger);
	}
}
