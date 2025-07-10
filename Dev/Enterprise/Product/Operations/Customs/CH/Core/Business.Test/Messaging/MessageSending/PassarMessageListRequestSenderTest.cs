using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Customs.CH.Business.MessagingConstants;
using static Enterprise.Messaging.Business.EDIMessage;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(PassarMessageListRequestSender))]
sealed class PassarMessageListRequestSenderTest : BaseMessageListRequestSenderTest
{
	protected override string ApplicationCode => ApplicationCodes.CHCustomsPassar;

	protected override string FriendlyName => "Passar Message List Request";

	protected override string CustomsDestinationCode => MessagingConstants.CustomsDestinationCodes.CustomsPassar;

	protected override BaseMessageListRequestSender CreateMessageListRequestSender(LoggingInformation logger) => new PassarMessageListRequestSender(logger);

	protected override string GetExpectedHeaderText(GlbCompany company, CusPollingTransaction transaction = null)
	{
		var bpid = company.OrgProxy.CustomsCodes.GetCustomsRegNo(OrgCusCode.SwissCodeTypes.BID);
		var lastMessageId = transaction?.CPT_TransactionID;
		return $@"{{""{CustomMsgAttributes.BpId}"":""{bpid}"",""{CustomMsgAttributes.LastMessageID}"":""{lastMessageId}""}}";
	}
}

