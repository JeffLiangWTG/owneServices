using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageProcessors
{
	public interface IExportResponseMessageProvider : ICUSRESV921ESMessageProvider
	{
		ZString UniqueReferenceNumber { get; }
		ZString MessageFunctionCAN { get; }
		ZString CustomsClearanceStatus { get; }
		ZString CSVT2LFCode { get; }
	}
}
