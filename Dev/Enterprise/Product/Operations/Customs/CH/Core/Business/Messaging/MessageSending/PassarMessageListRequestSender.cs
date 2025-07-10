using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Messaging.Business.EDIMessage;

namespace Enterprise.Customs.CH.Business;

class PassarMessageListRequestSender : BaseMessageListRequestSender
{
	public PassarMessageListRequestSender(LoggingInformation logger) : base(logger)
	{
	}

	protected override string FriendlyName => (NoResString)"Passar Message List Request";

	protected override ZString ApplicationCode => ApplicationCodes.CHCustomsPassar;

	protected override Dictionary<string, string> GetHeaderTextWithAttribute(ZString lastMessageId)
	{
		return base.GetHeaderTextWithAttribute(lastMessageId)
			.AddPartnerTopicIfEnabled();
	}
}
