using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing;

[TestedType(typeof(ShippingBillMessageSender))]
sealed class ShippingBillMessageSenderTest : BaseMessageSenderAbstractTest<ShippingBillMessageSender, DeclarationMessageSendingObject>
{
	protected override string ExpectedMessageType => EDIMessageTypeList.Codes.ShippingBill;

	protected override string GetExpectedMessageSubType(string messageType) => EDIMessageTypeList.Codes.ShippingBill + messageType;

	protected override IReadOnlyList<string> ExpectedMessageTags => Array.Empty<string>();

	protected override string ExpectedMessageOwner => "INNSA1";

	protected override IReadOnlyList<string> MessageTyeList => new DeclarationMessageTypeList().GetAllCodes();

	protected override string GetExpectedCustomsStatus(string messageType) => string.Empty;

	protected override (DeclarationMessageSendingObject sendingObject, ShippingBillMessageSender messageSender) CreateMessageSender(string messageType)
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_CustomsOffice = "INNSA1";
		var header = Factory.NewWithValidTestData<CusEntryHeader>();
		declaration.ActiveEntryHeaders.Add(header);
		var messageSendingObject = new DeclarationMessageSendingObject(header);
		messageSendingObject.MessageType = messageType;
		var messageSender = new ShippingBillMessageSender(messageSendingObject);
		return (messageSendingObject, messageSender);
	}
}
