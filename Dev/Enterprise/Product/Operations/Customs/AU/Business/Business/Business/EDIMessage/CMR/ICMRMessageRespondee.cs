using CargoWise.Types;
using Enterprise.Freight.Integration;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public interface ICMRMessageRespondee
	{
		ZString Details { get; }
		EDIMessageCollection Messages { get; }
		ZString ShortDescription { get; }
	}

	public interface ICMRMessageRespondeeReference
	{
		ZString GetHTMLFormatDetailsIfNeeded(ZString details, bool isForHtml);
	}

	public interface ICMRControlMessageRespondee
	{
		ZString UpdateStatusWhenControlMessageSyntaxError(EDIMessage incomingMessage, EDIMessage outgoingMessage);
		EDIMessageCollection Messages { get; }
	}

	public interface ICMRCargoReportEventsLogger
	{
		IParentForCargoReporter ParentForCargoReportingEvents { get; }
	}
}
