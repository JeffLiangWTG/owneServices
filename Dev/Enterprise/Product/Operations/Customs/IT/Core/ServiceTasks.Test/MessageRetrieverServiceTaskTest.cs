using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Registry;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ServiceManager.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;

namespace Enterprise.Customs.IT.ServiceTasks.Testing;

[TestedType(typeof(MessageRetrieverServiceTask))]
sealed class MessageRetrieverServiceTaskTest : MessagingServiceTaskTest<MessageRetrieverServiceTask>
{
	protected override void AssertSpecificHostedServiceAttributeProperties(HostedServiceAttribute attribute)
	{
		AssertEquals("Code", ServiceTaskCodeList.Codes.MessageRetriever, attribute.Code);
		AssertEquals("Description", ServiceTaskCodeList.Descriptions.MessageRetriever, attribute.Description);
		AssertEquals("Type", typeof(MessageRetrieverServiceTask), attribute.Type);
		AssertEquals("MinimumPeriod", "30seconds", attribute.MinimumPeriod);
		AssertEquals("DefaultScheduleRunEvery", "15minutes", attribute.DefaultScheduleRunEvery);
	}

	public void TestHostedServiceBusinessObjectBindingAttributes()
	{
		var testTask = new MessageRetrieverServiceTask();
		var hostedServiceBusinessObjectBindingAttributes = testTask.GetType().Assembly
			.GetCustomAttributes(typeof(HostedServiceBusinessObjectBindingAttribute), inherit: false)
			.OfType<HostedServiceBusinessObjectBindingAttribute>()
			.Where(x => x.ServiceTaskCode == Enterprise.Customs.IT.ServiceTasks.ServiceTaskCodeList.Codes.MessageRetriever);

		AssertEquals("HostedServiceBusinessObjectBindingAttribute count", 2, hostedServiceBusinessObjectBindingAttributes.Count());

		var predicates = hostedServiceBusinessObjectBindingAttributes.SelectMany(x => x.Predicates).ToArray();
		CombineAssertions(() =>
		{
			AssertCollectionContains("EI_ApplicationCode = ITM", "EI_ApplicationCode=ITM", predicates);
			AssertCollectionContains("EI_ApplicationCode = ITH", "EI_ApplicationCode=ITH", predicates);
		});
	}

	public void TestInitialiseSchedule()
	{
		var testTask = new MessageRetrieverServiceTask();
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

	public void TestRunTaskWithSomeMatchingInterchanges()
	{
		// Prepare test interchanges
		var interchange1 = Factory.New<EDIInterchange>();
		interchange1.EI_BodyText = "01N6            01N60927.U05190000208370304100    005916R         001 00003    INVIO IN AMBIENTE DI PROVA    \r\n0     000. 0  0 0\r\nFri Sep 27 21:52:13 2019\r\n";
		interchange1.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
		interchange1.EI_Status = EDIInterchange.Status.Queued;
		interchange1.EI_ApplicationCode = EDIInterchange.ApplicationCodes.ITCustoms;
		interchange1.EI_InterchangeType = EDIInterchange.ApplicationCodes.ITCustoms;
		interchange1.EI_InterchangeNum = "0001";
		interchange1.EI_From = "TEST";
		interchange1.EI_To = "TEST";

		var interchange2 = Factory.New<EDIInterchange>();
		interchange2.EI_BodyText = "01N6            01N60928.U05190000208370304100    005916R         001 00003    INVIO IN AMBIENTE DI PROVA    \r\n0     000. 0  0 0\r\nSat Sep 28 18:43:00 2019\r\n";
		interchange2.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
		interchange2.EI_Status = EDIInterchange.Status.Queued;
		interchange2.EI_ApplicationCode = EDIInterchange.ApplicationCodes.ITCustoms;
		interchange2.EI_InterchangeType = SADConstants.CustomsInterchangeType.IrispX;
		interchange2.EI_InterchangeNum = "0002";
		interchange2.EI_From = "TEST";
		interchange2.EI_To = "TEST";

		var unmatchingInterchange = Factory.New<EDIInterchange>();
		unmatchingInterchange.EI_Status = EDIInterchange.Status.Cancelled;
		unmatchingInterchange.EI_From = "TEST";
		unmatchingInterchange.EI_To = "TEST";

		Factory.Save();

		// Run service task
		var logs = InitialiseAndRunTaskSchedule(new MessageRetrieverServiceTask());

		interchange1.Reload();
		interchange2.Reload();
		unmatchingInterchange.Reload();

		CombineAssertions("Status and Message/Log Count", () =>
		{
			AssertEquals("Interchange1 Status", EDIInterchange.Status.Received, interchange1.EI_Status);
			AssertEquals("Interchange2 Status", EDIInterchange.Status.Received, interchange2.EI_Status);
			AssertEquals("Unmatched Interchange Status", EDIInterchange.Status.Cancelled, unmatchingInterchange.EI_Status);

			AssertEquals("Interchange1 Message Count", 1, interchange1.ContainedMessages.Count);
			AssertEquals("Interchange2 Message Count", 1, interchange2.ContainedMessages.Count);
			AssertEquals("Unmatching Interchange Message Count", 0, unmatchingInterchange.ContainedMessages.Count);

			AssertEquals("Log Count", 2, logs.Count);
		});

		CombineAssertions("Logs", () =>
		{
			AssertEquals($"1st Log [{logs[0]}] contains expected message?", true, logs[0].EndsWith("Interchange '0001' has been processed successfully."));
			AssertEquals($"2nd Log [{logs[1]}] contains expected message?", true, logs[1].EndsWith("Interchange '0002' has been processed successfully."));
		});

		CombineAssertions("Interchange1 Message", () =>
		{
			var createdMessage11 = interchange1.ContainedMessages[0];
			AssertEquals("EM_EI", interchange1.PK, createdMessage11.EM_EI);
			AssertEquals("EM_ApplicationCode", ApplicationCodeList.Codes.ITCustoms, createdMessage11.EM_ApplicationCode);
			AssertEquals("EM_MessageType", interchange1.EI_InterchangeType, createdMessage11.EM_MessageType);
			AssertEquals("EM_MessageSubType", "XXX", createdMessage11.EM_MessageSubType);
			AssertEquals("EM_MessageNum", "", createdMessage11.EM_MessageNum);
			AssertEquals("EM_IsTestMessage", true, createdMessage11.EM_IsTestMessage);
			AssertEquals("EM_MessageText", interchange1.EI_BodyText, createdMessage11.EM_MessageText);
			AssertEquals("EM_ReceiveTransmit", EDIMessage.Direction.Receive, createdMessage11.EM_ReceiveTransmit);
			AssertEquals("EM_Status", EDIMessage.Status.Queued, createdMessage11.EM_Status);
		});

		CombineAssertions("Interchange2 Message", () =>
		{
			var createdMessage21 = interchange2.ContainedMessages[0];
			AssertEquals("EM_EI", interchange2.PK, createdMessage21.EM_EI);
			AssertEquals("EM_ApplicationCode", ApplicationCodeList.Codes.ITCustoms, createdMessage21.EM_ApplicationCode);
			AssertEquals("EM_MessageType", interchange2.EI_InterchangeType, createdMessage21.EM_MessageType);
			AssertEquals("EM_MessageSubType", "XXX", createdMessage21.EM_MessageSubType);
			AssertEquals("EM_MessageNum", "", createdMessage21.EM_MessageNum);
			AssertEquals("EM_IsTestMessage", true, createdMessage21.EM_IsTestMessage);
			AssertEquals("EM_MessageText", interchange2.EI_BodyText, createdMessage21.EM_MessageText);
			AssertEquals("EM_ReceiveTransmit", EDIMessage.Direction.Receive, createdMessage21.EM_ReceiveTransmit);
			AssertEquals("EM_Status", EDIMessage.Status.Queued, createdMessage21.EM_Status);
		});
	}

	public void TestTaskRunWhenUseUCMPForITCEnabled()
	{
		var interchange = Factory.New<EDIInterchange>();
		interchange.EI_BodyText = "01N6            01N60927.U05190000208370304100    005916R         001 00003    INVIO IN AMBIENTE DI PROVA    \r\n0     000. 0  0 0\r\nFri Sep 27 21:52:13 2019\r\n";
		interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
		interchange.EI_Status = EDIInterchange.Status.Queued;
		interchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.ITCustoms;
		interchange.EI_InterchangeType = EDIInterchange.ApplicationCodes.ITCustoms;
		interchange.EI_InterchangeNum = "0001";
		interchange.EI_From = "TEST";
		interchange.EI_To = "TEST";

		Factory.Save();

		using (ITCustomsDataRegistry.Instance.UseUCMPForCategoryITC.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
		{
			var logs = InitialiseAndRunTaskSchedule(new MessageRetrieverServiceTask());
			interchange.Reload();
			AssertEquals("Interchange Message Count", 0, interchange.ContainedMessages.Count);
		}
	}

	public void TestRunTaskWithManualUploadedInterchange()
	{
		const string validUnsignedIcntrl =
		"004R            004R1004.U04190015588917275100    01790970139     001 00005    INVIO IN AMBIENTE REALE       \r\n" +
		"0     000. 0  0 0\r\n" +
		"Wed Jan 02 04:42:59 2019";

		var sentInterchangeWithIdoc = Factory.New<EDIInterchange>();
		var sentIdoc = Factory.New<ITEDIMessage>();
		sentInterchangeWithIdoc.ContainedMessages.Add(sentIdoc);
		sentInterchangeWithIdoc.EI_ApplicationCode = EDIMessage.ApplicationCodes.ITCustoms;
		sentInterchangeWithIdoc.EI_From = "FROM";
		sentInterchangeWithIdoc.EI_To = "TO";
		sentInterchangeWithIdoc.EI_SessionGUID = new ZGuid("8C2900CD-E6F1-482E-9D4A-0D4C60BA1292");
		sentInterchangeWithIdoc.EI_HeaderText =
					"<ITMessage>" +
						"<Header>004R            004R1004.R04190015588917275100    01790970139     001 00005</Header>" +
					"</ITMessage>";
		sentIdoc.EM_MessageType = SADConstants.CustomsInterchangeType.IdocR;
		sentIdoc.MessageNumberStrategy = new FixedMessageNumberStrategy("000001");
		Factory.Save();

		var manualUploader = new ResponseFileManualUploader(sentIdoc, validUnsignedIcntrl);
		manualUploader.ProcessExternalResponseMessage();

		var query = new ZQuery();
		query.AddToFilter(EDIInterchangeSchema.EI_ReceiveTransmit, EDIInterchange.Direction.Receive);
		query.OrderBy = EDIInterchange.Schema.EI_SystemCreateTimeUtc + " DESC";
		var queuedInterchange = Factory.LoadTop1<EDIInterchange>(query);
		AssertNotNull("EDIInterchange from manual upload has been queued correctly", queuedInterchange);

		var logs = InitialiseAndRunTaskSchedule(new MessageRetrieverServiceTask());
		sentInterchangeWithIdoc.Reload();
		queuedInterchange.Reload();

		CombineAssertions("Status and Message", () =>
		{
			AssertEquals(EDIInterchange.Status.Received, queuedInterchange.EI_Status);
			AssertEquals(1, queuedInterchange.ContainedMessages.Count);
		});

		CombineAssertions("Logs", () =>
		{
			AssertEquals(1, logs.Count);
			AssertEquals($"1st Log [{logs[0]}] contains expected message?", true, logs[0].EndsWith("Interchange '1' has been processed successfully."));
		});
	}

	public void TestRunTaskWithXTradeInterchange()
	{
		var interchange1 = Factory.New<EDIInterchange>();
		interchange1.EI_BodyText = "<SOAP BODY TEXT>";
		interchange1.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
		interchange1.EI_Status = EDIInterchange.Status.Queued;
		interchange1.EI_ApplicationCode = ApplicationCodeList.Codes.ITCustomsXTrade;
		interchange1.EI_InterchangeType = EDIInterchange.ApplicationCodes.ITCustoms;
		interchange1.EI_InterchangeNum = "0001";
		interchange1.EI_From = "TEST";
		interchange1.EI_To = "TEST";

		Factory.Save();

		var logs = InitialiseAndRunTaskSchedule(new MessageRetrieverServiceTask());
		interchange1.Reload();

		CombineAssertions("Status and Message/Log Count", () =>
		{
			AssertEquals("Interchange1 Status", EDIInterchange.Status.Received, interchange1.EI_Status);
			AssertEquals("Interchange1 Message Count", 1, interchange1.ContainedMessages.Count);
			AssertEquals("Log Count", 1, logs.Count);
		});

		AssertEquals($"Log [{logs[0]}] contains expected message?", true, logs[0].EndsWith("Interchange '0001' has been processed successfully."));

		CombineAssertions("Interchange1 Message", () =>
		{
			var createdMessage11 = interchange1.ContainedMessages[0];
			AssertEquals("EM_EI", interchange1.PK, createdMessage11.EM_EI);
			AssertEquals("EM_ApplicationCode", ApplicationCodeList.Codes.ITCustomsXTrade, createdMessage11.EM_ApplicationCode);
			AssertEquals("EM_MessageType", interchange1.EI_InterchangeType, createdMessage11.EM_MessageType);
			AssertEquals("EM_MessageSubType", "XXX", createdMessage11.EM_MessageSubType);
			AssertEquals("EM_MessageNum", "", createdMessage11.EM_MessageNum);
			AssertEquals("EM_IsTestMessage", true, createdMessage11.EM_IsTestMessage);
			AssertEquals("EM_MessageText", interchange1.EI_BodyText, createdMessage11.EM_MessageText);
			AssertEquals("EM_ReceiveTransmit", EDIMessage.Direction.Receive, createdMessage11.EM_ReceiveTransmit);
			AssertEquals("EM_Status", EDIMessage.Status.Queued, createdMessage11.EM_Status);
		});
	}

	protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
	{
		get
		{
			return new TaskNudgeInformationForTest[]
			{
				new TaskNudgeInformationForTest(
					EDIInterchangeSchema.Constants.TableName,
					"IT Customs Message Retriever for eHub",
					EDIInterchangeSchema.Constants.EI_Status + "=" + EDIInterchange.Status.Queued,
					EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + EDIInterchange.Direction.Receive,
					EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
					EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + EDIInterchange.ApplicationCodes.ITCustoms),

				new TaskNudgeInformationForTest(
					EDIInterchangeSchema.Constants.TableName,
					"IT Customs Message Retriever for xT",
					EDIInterchangeSchema.Constants.EI_Status + "=" + EDIInterchange.Status.Queued,
					EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + EDIInterchange.Direction.Receive,
					EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
					EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + ApplicationCodeList.Codes.ITCustomsXTrade),
			};
		}
	}
}
