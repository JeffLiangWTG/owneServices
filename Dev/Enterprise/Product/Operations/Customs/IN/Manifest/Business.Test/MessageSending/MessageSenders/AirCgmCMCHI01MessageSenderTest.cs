using System.Collections.Generic;
using Enterprise.Customs.IN.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Manifest.Business.Testing;

[TestedType(typeof(AirCgmCMCHI01MessageSender))]
sealed class AirCgmCMCHI01MessageSenderTest : IN.Business.Testing.BaseMessageSenderAbstractTest<AirCgmCMCHI01MessageSender, ManifestMessageSendingObject>
{
	protected override string ExpectedMessageType => EDIMessageTypeList.Codes.ConsolGeneralManifest;

	protected override string GetExpectedMessageSubType(string messageType) => EDIMessageSubTypeList.Codes.AirCgm;

	protected override IReadOnlyList<string> ExpectedMessageTags => new[] { "HREC", "TREC", "<consoligm>", "<consmaster>", "<conshouse>" };

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

	protected override (ManifestMessageSendingObject sendingObject, AirCgmCMCHI01MessageSender messageSender) CreateMessageSender(string messageType)
	{
		var manifestHeader = Factory.New<CGMAsycudaManifestHeader>();
		manifestHeader.AMA_CustomsOffice = ExpectedMessageOwner;
		var bill = manifestHeader.Bills.AddNew();
		var messageSendingObject = new ManifestMessageSendingObject(manifestHeader);
		messageSendingObject.MessageType = messageType;
		manifestHeader.MasterBill.ABL_BillStatus = messageType;
		bill.ABL_BillStatus = messageType;
		var messageSender = new AirCgmCMCHI01MessageSender(messageSendingObject);
		return (messageSendingObject, messageSender);
	}
}
