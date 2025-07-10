using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Messaging.Business.EDIMessage;

namespace Enterprise.Customs.CH.Business;

class CharteraOutputGetMessageRequestSender : BaseGetMessageRequestSender
{
	public CharteraOutputGetMessageRequestSender(LoggingInformation logger) : base(logger)
	{
	}

	protected override string FriendlyName => (NoResString)"Chartera Output Get Message Request";

	protected override ZString ApplicationCode => ApplicationCodes.CHCustomsCharteraOutput;
}
