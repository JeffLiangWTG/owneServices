using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(CustomsMessageHelper))]
sealed class CustomsMessageHelperTest : TestCaseWithFactory
{
	public void TestCreateHeaderAttributes() => CombineAssertions(() =>
	{
		var headerAttributes = CustomsMessageHelper.CreateHeaderAttributes();
		AssertType<Dictionary<string, string>>("Type", headerAttributes);
		AssertEquals("Empty", 0, headerAttributes.Count);
	});

	public void TestAddBpId()
	{
		const string BpId = "234567";
		var company = MessageProcessorTestHelper.CreateCompany(Factory, bpid: BpId);

		using (DisposableEnvironment.ForCompany(company.GC_Code))
		{
			var headerAttributes = CustomsMessageHelper.CreateHeaderAttributes().AddBpId();
			AssertHeaderAttributes(headerAttributes, MessagingConstants.CustomMsgAttributes.BpId, BpId);
		}
	}

	public void TestAddMessageId() => AssertHeaderAttributes(CustomsMessageHelper.CreateHeaderAttributes().AddMessageId("1234"), MessagingConstants.CustomMsgAttributes.MessageID, "1234");

	public void TestAddLastMessageId() => AssertHeaderAttributes(CustomsMessageHelper.CreateHeaderAttributes().AddLastMessageId("4321"), MessagingConstants.CustomMsgAttributes.LastMessageID, "4321");

	public void TestAddMessageType() => AssertHeaderAttributes(CustomsMessageHelper.CreateHeaderAttributes().AddMessageType("type"), MessagingConstants.CustomMsgAttributes.MessageType, "type");

	public void TestAddPartnerTopic() => CombineAssertions(() =>
	{
		AssertEquals("No Attributes Added", 0, CustomsMessageHelper.CreateHeaderAttributes().AddPartnerTopicIfEnabled().Count);

		using (CHCustomsDataRegistry.Instance.EnablePartnerTopic.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
		{
			AssertHeaderAttributes(CustomsMessageHelper.CreateHeaderAttributes().AddPartnerTopicIfEnabled(), MessagingConstants.CustomMsgAttributes.PartnerTopic, CustomsMessageHelper.PartnerTopic);
		}
	});

	public void TestMultipleAdd()
	{
		var expectedAttributes = new Dictionary<string, string>()
			{
				{ MessagingConstants.CustomMsgAttributes.MessageID, "1234" },
				{ MessagingConstants.CustomMsgAttributes.LastMessageID, "4321" },
				{ MessagingConstants.CustomMsgAttributes.MessageType, "type" },
			};

		var headerAttributes = CustomsMessageHelper.CreateHeaderAttributes()
			.AddMessageId("1234")
			.AddLastMessageId("4321")
			.AddMessageType("type");

		JsonTestHelper.AssertEquals("Attributes", expectedAttributes, headerAttributes);
	}

	public void TestPartnerTopic() => AssertEquals(CustomsMessageHelper.PartnerTopic, $"CW-{GlbCompany.CurrentCompany.LicenceKeyIdentifier}");

	public void TestCreateEDIInterchanges() => CombineAssertions(() =>
	{
		var ediMessage1 = CreateEDIMessage();
		var ediMessage2 = CreateEDIMessage();
		var ediMessage3 = CreateEDIMessage(applicationCode: "XXX");
		var ediMessage4 = CreateEDIMessage(messageText: string.Empty);

		var ediInterchanges = CustomsMessageHelper.CreateEDIInterchanges(ediMessage1, ediMessage2);

		AssertEquals("# of interchanges", 2, ediInterchanges.Length);
		AssertNull("No message provided", CustomsMessageHelper.CreateEDIInterchanges());
		AssertNull("No provider", CustomsMessageHelper.CreateEDIInterchanges(ediMessage3));
		AssertEquals("Incomplete message", 0, CustomsMessageHelper.CreateEDIInterchanges(ediMessage4)?.Length);
	});

	public void TestCreateEDIInterchange() => CombineAssertions(() =>
	{
		var ediMessage1 = CreateEDIMessage();
		var ediMessage2 = CreateEDIMessage(applicationCode: "XXX");
		var ediMessage3 = CreateEDIMessage(messageText: string.Empty);
		ediMessage1.CreateEDIInterchange();
		ediMessage2.CreateEDIInterchange();
		AssertNotNull("Interchange created", ediMessage1.Interchange);
		AssertNull("No provider - no interchange created", ediMessage2.Interchange);
		AssertNull("Incomplete message - no interchange created", ediMessage2.Interchange);
	});

	EDIMessage CreateEDIMessage(string applicationCode = EDIMessage.ApplicationCodes.CHCustomsPassar, string messageText = "TEST")
	{
		var ediMessage = Factory.New<EDIMessage>();
		ediMessage.EM_ApplicationCode = applicationCode;
		ediMessage.EM_LinkedObject = GlbCompany.CurrentCompany;
		ediMessage.EM_MessageText = messageText;
		return ediMessage;
	}

	void AssertHeaderAttributes(Dictionary<string, string> headerAttributes, string expectedKey, string expectedValue) => JsonTestHelper.AssertEquals("Attributes", new Dictionary<string, string>() { { expectedKey, expectedValue } }, headerAttributes);
}
