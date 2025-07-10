using CargoWise.Types;
using Enterprise.BatchProcessor;

namespace Enterprise.Messaging.Business.MessageProcessor
{
	public interface IUniversalCustomsEDIMessagePacker
	{
		bool AllowEmptyMessageBody { get; }
		ZString Pack(EDIMessage message, EDIInterchange interchange, LoggingInformation logger);
	}
}
