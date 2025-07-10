using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.Customs.IT.Registry;
using Enterprise.Customs.IT.Registry.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ServiceManager.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;

namespace Enterprise.Customs.IT.ServiceTasks.Testing;

[TestedType(typeof(MessageSenderServiceTask))]
sealed class MessageSenderServiceTaskTest : MessagingServiceTaskTest<MessageSenderServiceTask>
{
	protected override void AssertSpecificHostedServiceAttributeProperties(HostedServiceAttribute attribute)
	{
		AssertEquals("Code", ServiceTaskCodeList.Codes.MessageSender, attribute.Code);
		AssertEquals("Description", ServiceTaskCodeList.Descriptions.MessageSender, attribute.Description);
		AssertEquals("Type", typeof(MessageSenderServiceTask), attribute.Type);
		AssertEquals("MinimumPeriod", "1minute", attribute.MinimumPeriod);
		AssertEquals("DefaultScheduleRunEvery", "15minutes", attribute.DefaultScheduleRunEvery);
	}

	public void TestInitialiseSchedule()
	{
		var testTask = new MessageSenderServiceTask();
		InitialiseTaskSchedule(testTask, out StmServiceTask taskSchedule);

		CombineAssertions(() =>
		{
			AssertEquals("IsActive", ZBool.True, taskSchedule.SST_Active);
			Assert("TaskPeriod", taskSchedule.Recurrence.MinutesRange);
			AssertEquals("TaskPeriodCount", 15, taskSchedule.Recurrence.Period);
			AssertEquals("WeekDaysOnly", ZBool.False, taskSchedule.Recurrence.WeekDaysOnly);
			AssertEquals("Is DailyStartTime empty?", true, taskSchedule.Recurrence.CalcDailyStartTimeUtc.IsEmpty);
		});
	}

	public void TestRunTaskWithOneMatchingMessage()
	{
		var declarant = Factory.NewWithValidTestData<OrgHeader>();
		declarant.OH_Code = "DEC1";
		Factory.Save();
		var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
		ITCustomsNumberViewStmNumsWrapperTestHelper.SetupTestCustomsDeclarationsNumberRange(company, ZDate.Today.Year, "11111111111", 1);
		new AccountCollectionTestBuilder(company.PK)
			.AppendAccount("11111111111-001", "1234")
			.AppendAccountDetail("1234-DEC1", "DEC1")
			.Build();

		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_CustomsProfile = "1234-DEC1";
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();

		var message = CreateQueuedOutboundMessage(SADConstants.CustomsInterchangeType.IdocR, "01");
		message.EM_LinkedObject = entryHeader;

		var unmatchingMessage = Factory.New<ITEDIMessage>();
		unmatchingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		unmatchingMessage.EM_Status = EDIMessage.Status.Queued;

		Factory.Save();

		CombineAssertions("PRE-CONDITION", () =>
		{
			AssertNull("Matching Message Interchange", message.Interchange);
			AssertNull("Unmatching Message Interchange", unmatchingMessage.Interchange);
		});

		var logs = InitialiseAndRunTaskSchedule(new MessageSenderServiceTask());

		message.Reload();
		unmatchingMessage.Reload();

		CombineAssertions("Status", () =>
		{
			AssertEquals("Matching Message Status", EDIMessage.Status.Sent, message.EM_Status);
			AssertEquals("Unmatching Message Status", EDIMessage.Status.Queued, unmatchingMessage.EM_Status);

			AssertNotNull("Matching Message Interchange", message.Interchange);
			AssertNull("Unmatching Message Interchange", unmatchingMessage.Interchange);

			AssertEquals("Log Count", 1, logs.Count);
		});

		CombineAssertions("Created Interchange", () =>
		{
			ITInterchangeProviderTestHelper.AssertCreatedInterchange(message);
			AssertEquals($"1st Log [{logs[0]}] contains expected message?", true, logs[0].EndsWith("1 message(s) have been processed."));
		});
	}

	public void TestRunWithMultipleMatchingMessages()
	{
		var declarant = Factory.NewWithValidTestData<OrgHeader>();
		declarant.OH_Code = "DEC1";
		Factory.Save();
		var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
		ITCustomsNumberViewStmNumsWrapperTestHelper.SetupTestCustomsDeclarationsNumberRange(company, ZDate.Today.Year, "11111111111", 1);
		new AccountCollectionTestBuilder(company.PK)
			.AppendAccount("11111111111-001", "1234")
			.AppendAccountDetail("1234-DEC1", "DEC1")
			.Build();

		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_CustomsProfile = "1234-DEC1";
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();

		var message1 = CreateQueuedOutboundMessage(SADConstants.CustomsInterchangeType.IdocR, "01", "HERE THERE IS THE INTERCHANGE HEADER\r\nTIM           12345600012345	A	0123456	01	02072019	HERE SOME OTHER DATA");
		message1.EM_LinkedObject = entryHeader;
		var message2 = CreateQueuedOutboundMessage(SADConstants.CustomsInterchangeType.IdocR, "02");
		message2.EM_LinkedObject = entryHeader;
		var message3 = CreateQueuedOutboundMessage(MessageProcessorConstants.InterchangeTypes.SingleWindowRequest, "03");
		message3.EM_LinkedObject = entryHeader;
		var message4 = CreateQueuedOutboundMessage("NEW", "04", applicationCode: EDIInterchange.ApplicationCodes.ITCustomsXTrade);
		message4.EM_LinkedObject = entryHeader;

		Factory.Save();

		CombineAssertions("PRE-CONDITION", () =>
		{
			AssertNull("Message1 Message Interchange", message1.Interchange);
			AssertNull("Message2 Message Interchange", message2.Interchange);
			AssertNull("Message3 Message Interchange", message3.Interchange);
			AssertNull("Message4 Message Interchange", message4.Interchange);
		});

		var logs = InitialiseAndRunTaskSchedule(new MessageSenderServiceTask());

		message1.Reload();
		message2.Reload();
		message3.Reload();
		message4.Reload();

		CombineAssertions("Status", () =>
		{
			AssertEquals("Message1 Status", EDIMessage.Status.Sent, message1.EM_Status);
			AssertEquals("Message2 Status", EDIMessage.Status.Sent, message2.EM_Status);
			AssertEquals("Message3 Status", EDIMessage.Status.Sent, message3.EM_Status);
			AssertEquals("Message4 Status", EDIMessage.Status.Sent, message4.EM_Status);

			AssertNotNull("Message1 Interchange", message1.Interchange);
			AssertNotNull("Message2 Interchange", message2.Interchange);
			AssertNotNull("Message3 Interchange", message3.Interchange);
			AssertNotNull("Message4 Interchange", message4.Interchange);
			AssertNotEquals("message1.Interchange and message2.Interchange must be different (no collation)", message1.EM_EI, message2.EM_EI);
			AssertNotEquals("message2.Interchange and message3.Interchange must be different (no collation)", message2.EM_EI, message3.EM_EI);
			AssertNotEquals("message3.Interchange and message4.Interchange must be different (no collation)", message3.EM_EI, message4.EM_EI);

			AssertEquals("Log Count", 3, logs.Count);
		});

		CombineAssertions("Created Interchanges", () =>
		{
			ITInterchangeProviderTestHelper.AssertCreatedInterchange(message1);
			ITInterchangeProviderTestHelper.AssertCreatedInterchange(message2);
			ITInterchangeProviderTestHelper.AssertCreatedInterchange(message3);
			AssertEquals($"1st Log [{logs[0]}] contains expected message?", true, logs[0].EndsWith("2 message(s) have been processed."));
			AssertEquals($"2nd Log [{logs[1]}] contains expected message?", true, logs[1].EndsWith("1 message(s) have been processed."));
			AssertEquals($"3rd Log [{logs[2]}] contains expected message?", true, logs[1].EndsWith("1 message(s) have been processed."));
		});
	}

	public void TestTaskRunWhenUseUCMPForITCEnabled()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var message = CreateQueuedOutboundMessage(SADConstants.CustomsInterchangeType.IdocR, "01");
		message.EM_LinkedObject = entryHeader;
		Factory.Save();
		using (ITCustomsDataRegistry.Instance.UseUCMPForCategoryITC.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
		{
			var logs = InitialiseAndRunTaskSchedule(new MessageSenderServiceTask());
			AssertNull("Don't run service tasks", message.Interchange);
		}
	}

	protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
	{
		get
		{
			return new[]
			{
				GetNudgeInformationForTest(queueName: "IT Customs Message Sender for eHub", EDIMessage.ApplicationCodes.ITCustoms),
				GetNudgeInformationForTest(queueName: "IT Customs Message Sender for XTrade", EDIMessage.ApplicationCodes.ITCustomsXTrade),
			};
		}
	}

	static TaskNudgeInformationForTest GetNudgeInformationForTest(string queueName, string applicationCode)
	{
		return new TaskNudgeInformationForTest(EDIMessageSchema.Constants.TableName
			, queueName
			, EDIMessageSchema.Constants.EM_IsActive + "=Y"
			, EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Transmit
			, EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued
			, EDIMessageSchema.Constants.EM_ApplicationCode + "=" + applicationCode
			, EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL");
	}

	ITEDIMessage CreateQueuedOutboundMessage(string messageType, string messageNumber, string messageText = null, string applicationCode = EDIMessage.ApplicationCodes.ITCustoms)
	{
		var message = Factory.New<ITEDIMessage>();
		message.EM_ApplicationCode = applicationCode;
		message.EM_MessageType = messageType;
		message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
		message.EM_Status = EDIMessage.Status.Queued;
		message.EM_MessageNum = messageNumber;
		message.MessageNumberStrategy = new FixedMessageNumberStrategy(messageNumber);
		message.EM_MessageText = messageText ?? "TEST MESSAGE TEXT " + messageNumber;
		return message;
	}
}
