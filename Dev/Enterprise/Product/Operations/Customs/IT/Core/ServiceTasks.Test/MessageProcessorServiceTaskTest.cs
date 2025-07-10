using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.Customs.IT.Registry;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ServiceManager.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;

namespace Enterprise.Customs.IT.ServiceTasks.Testing;

[TestedType(typeof(MessageProcessorServiceTask))]
sealed class MessageProcessorServiceTaskTest : MessagingServiceTaskTest<MessageProcessorServiceTask>
{
	protected override void AssertSpecificHostedServiceAttributeProperties(HostedServiceAttribute attribute)
	{
		AssertEquals("Code", ServiceTaskCodeList.Codes.MessageProcessor, attribute.Code);
		AssertEquals("Description", ServiceTaskCodeList.Descriptions.MessageProcessor, attribute.Description);
		AssertEquals("Type", typeof(MessageProcessorServiceTask), attribute.Type);
		AssertEquals("MinimumPeriod", "30seconds", attribute.MinimumPeriod);
		AssertEquals("DefaultScheduleRunEvery", "15minutes", attribute.DefaultScheduleRunEvery);
	}

	public void TestInitialiseSchedule()
	{
		var testTask = new MessageProcessorServiceTask();
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

	public void TestRunTaskWithCustomsDeclarationSingleWindowPositiveAckResponseMessage()
	{
		var jobDeclaration = Factory.New<JobDeclaration>();
		var entryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_Status = "AWO";
		var sentIdocEdiMessage = entryHeader.Messages.AddNew();
		sentIdocEdiMessage.EM_MessageText = @"HERE THERE IS THE INTERCHANGE HEADER
TIM           12345600012345	A	0123456	01	02072019	HERE SOME OTHER DATA";
		sentIdocEdiMessage.EM_Status = EDIMessage.Status.Sent;
		sentIdocEdiMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
		sentIdocEdiMessage.EM_MessageSubType = SADConstants.MessageSubTypes.IM;
		sentIdocEdiMessage.EM_MessageType = SADConstants.CustomsInterchangeType.IdocR;
		sentIdocEdiMessage.EM_MessageNum = "123456";
		sentIdocEdiMessage.MessageNumberStrategy = new FixedMessageNumberStrategy(sentIdocEdiMessage.EM_MessageNum);

		var sentEdiInterchange = Factory.New<ITEDIInterchange>();
		sentEdiInterchange.EI_InterchangeNum = "1";
		sentEdiInterchange.EI_SessionGUID = new ZGuid("77ABFE3E-177A-4872-9815-4A6C69AD0400");
		sentEdiInterchange.ContainedMessages.Add(sentIdocEdiMessage);
		sentEdiInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
		sentEdiInterchange.EI_HeaderText = SadIncomingCustomsMessageProcessorTestHelper.GetSentHeaderText(idocFileName: "4JEZ0102.RB9");

		var headerMessageText = ZString.Empty;
		var bodyMessageText = ZString.Empty;

		var receivedIcntrlEdiMessage = Factory.New<ITEDIMessage>();
		receivedIcntrlEdiMessage.EM_ApplicationCode = ApplicationCodeList.Codes.ITCustoms;
		receivedIcntrlEdiMessage.EM_Status = EDIMessage.Status.Queued;
		receivedIcntrlEdiMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		receivedIcntrlEdiMessage.EM_MessageType = "WSA";
		receivedIcntrlEdiMessage.EM_MessageNum = "123456";
		receivedIcntrlEdiMessage.MessageNumberStrategy = new FixedMessageNumberStrategy(receivedIcntrlEdiMessage.EM_MessageNum);
		receivedIcntrlEdiMessage.EM_MessageText = $"{headerMessageText}\r\n{bodyMessageText}".TrimStart();

		var receivedEdiInterchange = Factory.New<ITEDIInterchange>();
		receivedEdiInterchange.EI_InterchangeNum = "2";
		receivedEdiInterchange.EI_SessionGUID = new ZGuid("761B61D9-BCFC-4C48-98D1-110D22D9477E");
		receivedEdiInterchange.ContainedMessages.Add(receivedIcntrlEdiMessage);
		receivedEdiInterchange.EI_HeaderText = $@"<ITMessage>	
	<MessageType>WSA</MessageType>
	<FileName>4JEZ0102.WSAB9</FileName>
	<eHubTrackingIDFromSentInterchange>77ABFE3E-177A-4872-9815-4A6C69AD0400</eHubTrackingIDFromSentInterchange>
	<Header>{headerMessageText}</Header>
</ITMessage>";

		receivedEdiInterchange.EI_BodyText = bodyMessageText;

		receivedEdiInterchange.EI_InterchangeType = "WSA";

		Factory.Save();

		// Run service task
		var logs = InitialiseAndRunTaskSchedule(new MessageProcessorServiceTask());

		receivedIcntrlEdiMessage.Reload();
		receivedEdiInterchange.Reload();
		entryHeader.Reload();
		entryHeader.Messages.Reload(true);

		AssertEquals("Message status", "RCV", receivedIcntrlEdiMessage.EM_Status);
		AssertEquals("Interchange status", "RCV", receivedEdiInterchange.EI_Status);
		AssertEquals("Entry header status", "AWO", entryHeader.CH_Status);
		AssertEquals("Messages count for header", 2, entryHeader.Messages.Count);

		var icntrlMessage = entryHeader.Messages.Find(x => x.EM_Status == "RCV").SingleOrDefault();
		AssertNotNull("ICNTRL message linked to entry header", icntrlMessage);
		AssertEquals("ITM", icntrlMessage.EM_ApplicationCode);
		AssertEquals("RCV", icntrlMessage.EM_Status);
		AssertEquals("WSA", icntrlMessage.EM_MessageType);
		AssertEquals("123456", icntrlMessage.EM_MessageNum);
		AssertEquals(bodyMessageText, icntrlMessage.EM_MessageText);
	}

	public void TestTaskRunWhenUseUCMPForITCEnabled()
	{
		var jobDeclaration = Factory.New<JobDeclaration>();
		var entryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_Status = "AWO";
		var sentIdocEdiMessage = entryHeader.Messages.AddNew();
		sentIdocEdiMessage.EM_MessageText = @"HERE THERE IS THE INTERCHANGE HEADER
TIM           12345600012345	A	0123456	01	02072019	HERE SOME OTHER DATA";
		sentIdocEdiMessage.EM_Status = EDIMessage.Status.Sent;
		sentIdocEdiMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
		sentIdocEdiMessage.EM_MessageSubType = SADConstants.MessageSubTypes.IM;
		sentIdocEdiMessage.EM_MessageType = SADConstants.CustomsInterchangeType.IdocR;
		sentIdocEdiMessage.EM_MessageNum = "123456";
		sentIdocEdiMessage.MessageNumberStrategy = new FixedMessageNumberStrategy(sentIdocEdiMessage.EM_MessageNum);

		var sentEdiInterchange = Factory.New<ITEDIInterchange>();
		sentEdiInterchange.EI_InterchangeNum = "1";
		sentEdiInterchange.EI_SessionGUID = new ZGuid("77ABFE3E-177A-4872-9815-4A6C69AD0400");
		sentEdiInterchange.ContainedMessages.Add(sentIdocEdiMessage);
		sentEdiInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
		sentEdiInterchange.EI_HeaderText = SadIncomingCustomsMessageProcessorTestHelper.GetSentHeaderText(idocFileName: "4JEZ0102.RB9");

		var headerMessageText = ZString.Empty;
		var bodyMessageText = ZString.Empty;
		var receivedIcntrlEdiMessage = Factory.New<ITEDIMessage>();
		receivedIcntrlEdiMessage.EM_ApplicationCode = ApplicationCodeList.Codes.ITCustoms;
		receivedIcntrlEdiMessage.EM_Status = EDIMessage.Status.Queued;
		receivedIcntrlEdiMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		receivedIcntrlEdiMessage.EM_MessageType = "WSA";
		receivedIcntrlEdiMessage.EM_MessageNum = "123456";
		receivedIcntrlEdiMessage.MessageNumberStrategy = new FixedMessageNumberStrategy(receivedIcntrlEdiMessage.EM_MessageNum);
		receivedIcntrlEdiMessage.EM_MessageText = $"{headerMessageText}\r\n{bodyMessageText}".TrimStart();
		var receivedEdiInterchange = Factory.New<ITEDIInterchange>();
		receivedEdiInterchange.EI_InterchangeNum = "2";
		receivedEdiInterchange.EI_SessionGUID = new ZGuid("761B61D9-BCFC-4C48-98D1-110D22D9477E");
		receivedEdiInterchange.ContainedMessages.Add(receivedIcntrlEdiMessage);
		receivedEdiInterchange.EI_HeaderText = $@"<ITMessage>	
	<MessageType>WSA</MessageType>
	<FileName>4JEZ0102.WSAB9</FileName>
	<eHubTrackingIDFromSentInterchange>77ABFE3E-177A-4872-9815-4A6C69AD0400</eHubTrackingIDFromSentInterchange>
	<Header>{headerMessageText}</Header>
</ITMessage>";
		receivedEdiInterchange.EI_BodyText = bodyMessageText;
		receivedEdiInterchange.EI_InterchangeType = "WSA";

		Factory.Save();

		using (ITCustomsDataRegistry.Instance.UseUCMPForCategoryITC.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
		{
			var logs = InitialiseAndRunTaskSchedule(new MessageProcessorServiceTask());

			receivedIcntrlEdiMessage.Reload();
			receivedEdiInterchange.Reload();
			entryHeader.Reload();
			entryHeader.Messages.Reload(true);

			AssertEquals("Message status has no changes", "QUE", receivedIcntrlEdiMessage.EM_Status);
			AssertEquals("Entry header status has no changes", "AWO", entryHeader.CH_Status);
			AssertEquals("Messages count for header has no changes", 1, entryHeader.Messages.Count);
		}
	}

	protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
	{
		get
		{
			return new TaskNudgeInformationForTest[]
			{
				new TaskNudgeInformationForTest(
					EDIMessageSchema.Constants.TableName,
					"IT Customs Message Processor for eHub",
					EDIMessageSchema.Constants.EM_IsActive + "=Y",
					EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
					EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
					EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.ITCustoms,
					EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"),

				new TaskNudgeInformationForTest(
					EDIMessageSchema.Constants.TableName,
					"IT Customs Message Processor for xT",
					EDIMessageSchema.Constants.EM_IsActive + "=Y",
					EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
					EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
					EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.ITCustomsXTrade,
					EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"),
			};
		}
	}
}
