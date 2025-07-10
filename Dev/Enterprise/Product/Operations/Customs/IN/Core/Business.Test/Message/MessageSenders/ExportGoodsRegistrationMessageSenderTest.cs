using System.Collections.Generic;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing;

[TestedType(typeof(ExportGoodsRegistrationMessageSender))]
sealed class ExportGoodsRegistrationMessageSenderTest : BaseMessageSenderAbstractTest<ExportGoodsRegistrationMessageSender, DeclarationMessageSendingObject>
{
	protected override string ExpectedMessageType => EDIMessageTypeList.Codes.ShippingBill;

	protected override string GetExpectedMessageSubType(string messageType) => EDIMessageSubTypeList.Codes.GoodsRegistration;

	protected override IReadOnlyList<string> ExpectedMessageTags => new[]
	{
		"HREC",
		"TREC",
		"<SB_GOODS_REG>",
		"<END-SB_GOODS_REG>"
	};

	protected override string ExpectedMessageOwner => "INNSA1";

	protected override IReadOnlyList<string> MessageTyeList => new DeclarationMessageTypeList().GetAllCodes();

	protected override string GetExpectedCustomsStatus(string messageType) => string.Empty;

	protected override (DeclarationMessageSendingObject sendingObject, ExportGoodsRegistrationMessageSender messageSender) CreateMessageSender(string messageType)
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_CustomsOffice = "INNSA1";
		var header = Factory.NewWithValidTestData<CusEntryHeader>();
		declaration.ActiveEntryHeaders.Add(header);
		var messageSendingObject = new DeclarationMessageSendingObject(header);
		messageSendingObject.MessageType = DeclarationMessageTypeList.Codes.GoodsRegistration;
		var messageSender = new ExportGoodsRegistrationMessageSender(messageSendingObject);
		return (messageSendingObject, messageSender);
	}
}
