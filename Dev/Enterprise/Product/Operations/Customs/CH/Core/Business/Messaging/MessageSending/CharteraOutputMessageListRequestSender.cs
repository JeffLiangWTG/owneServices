using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Messaging.Business.EDIMessage;

namespace Enterprise.Customs.CH.Business;

class CharteraOutputMessageListRequestSender : BaseMessageListRequestSender
{
	public CharteraOutputMessageListRequestSender(LoggingInformation logger) : base(logger)
	{
	}

	protected override string FriendlyName => (NoResString)"Chartera Output Message List Request";

	protected override ZString ApplicationCode => ApplicationCodes.CHCustomsCharteraOutput;
}
