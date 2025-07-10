using System.Collections.Generic;
using Enterprise.Customs.IN.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Manifest.Business.Testing;

[TestedType(typeof(SeaCgmCMCHI21MessageSender))]
sealed class SeaCgmCMCHI21MessageSenderTest : IN.Business.Testing.BaseMessageSenderAbstractTest<SeaCgmCMCHI21MessageSender, ManifestMessageSendingObject>
{
	protected override string ExpectedMessageType => EDIMessageTypeList.Codes.ConsolGeneralManifest;

	protected override string GetExpectedMessageSubType(string messageType) => EDIMessageSubTypeList.Codes.SeaCgm;

	protected override IReadOnlyList<string> ExpectedMessageTags => new[] { "HREC", "TREC", "<consoligm>", "<conscargo>", "<conscont>" };

	protected override string ExpectedMessageOwner => "TestOffice";

	protected override IReadOnlyList<string> MessageTyeList => new ManifestMessageTypeList().GetAllCodes();

	protected override string GetExpectedCustomsStatus(string messageType)
	{
		return messageType switch
		{
			ManifestMessageTypeList.Codes.Amendment => RegistrationStatusList.Codes.ManifestIsUnderAmendment,
			ManifestMessageTypeList.Codes.Delete => RegistrationStatusList.Codes.ManifestDeleteRequisition,
			_ => string.Empty
		};
	}

	protected override (ManifestMessageSendingObject sendingObject, SeaCgmCMCHI21MessageSender messageSender) CreateMessageSender(string messageType)
	{
		var manifestHeader = Factory.New<CGMAsycudaManifestHeader>();
		manifestHeader.AMA_CustomsOffice = ExpectedMessageOwner;
		var messageSendingObject = new ManifestMessageSendingObject(manifestHeader);
		messageSendingObject.MessageType = messageType;
		var messageSender = new SeaCgmCMCHI21MessageSender(messageSendingObject);
		return (messageSendingObject, messageSender);
	}
}
