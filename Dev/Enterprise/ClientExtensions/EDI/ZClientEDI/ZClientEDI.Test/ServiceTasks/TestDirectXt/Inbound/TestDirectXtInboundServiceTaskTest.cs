using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.ServiceTasks.TestDirectXt;
using Enterprise.Messaging.Business;
using Enterprise.xTMessaging.Business;
using Enterprise.xTMessaging.ServiceTasks;
using Enterprise.xTMessaging.Shared;
using Enterprise.xTMessaging.Shared.Test;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using Xware.Xt.Grpc.Application;
using Xware.Xt.Grpc.Config;

namespace ZClientEDI.Test.ServiceTasks.TestDirectXt
{
	[TestedType(typeof(TestDirectXtInboundServiceTask))]
	class TestDirectXtInboundServiceTaskTest : ServiceTaskTestCase<TestDirectXtInboundServiceTask>
	{
		public void TestHostedServiceAttribute()
		{
			var attribute = typeof(TestDirectXtInboundServiceTask).Assembly.GetCustomAttributes(true).OfType<HostedServiceAttribute>().Single(x => x.Code == TestDirectXtInboundServiceTask.ServiceTaskCode);
			CombineAssertions(() =>
			{
				AssertEquals("Description", TestDirectXtInboundServiceTask.ServiceTaskDescription, attribute.Description);
				AssertEquals("Category", "ESV", attribute.Category);
				AssertEquals("ConfigControlType", typeof(TestDirectXtInboundServiceTask), attribute.Type);
				AssertEquals("AllowsMultipleInstances", false, attribute.AllowsMultipleInstances);
				AssertEquals("IsMandatory", true, attribute.IsMandatory);
				AssertEquals("CanRunInAnyBranch", true, attribute.CanRunInAnyBranch);
			});
		}

		public void TestRunTask()
		{
			var messageMetaDataForTest = new Dictionary<string, string>();
			messageMetaDataForTest.Add(Constants.CustomMsgAttributes.ApplicationCode, "TST");
			messageMetaDataForTest.Add(Constants.CustomMsgAttributes.SourceParty, "XH");
			messageMetaDataForTest.Add(Constants.CustomMsgAttributes.DestinationParty, "TSTTSTTST");
			messageMetaDataForTest.Add(Constants.CustomMsgAttributes.MessageTrackingID, "6B72FDCB-BDDC-47C6-B263-F1245A084AD9");
			var incomingMessageList = new Dictionary<ulong, TestIncomingMessagePreparation>();
			incomingMessageList[1u] = new TestIncomingMessagePreparation() { TestInstruction = TestInstructionValues.SUCCESS, MsgBody = "Body1", AckResponse = ErrorCode.ErrOk, MetaData = messageMetaDataForTest };
			incomingMessageList[2u] = new TestIncomingMessagePreparation() { TestInstruction = TestInstructionValues.SUCCESS, MsgBody = "Body2", AckResponse = ErrorCode.ErrOk, MetaData = messageMetaDataForTest };
			incomingMessageList[3u] = new TestIncomingMessagePreparation() { TestInstruction = TestInstructionValues.SUCCESS, MsgBody = "Body3", AckResponse = ErrorCode.ErrOk, MetaData = messageMetaDataForTest };

			(var mockedMsgClientProvider, _)  = TestDirectXtTestUtils.GetMockedMsgClientProviderInbound(incomingMessageList);

			var taskForTest = new TestDirectXtInboundServiceTaskTestWrapper();
			taskForTest.ClientProvider = mockedMsgClientProvider;
			taskForTest.AttributeModifier = null;
			taskForTest.IsTest = true;

			using (DirectxTMessagingRegistry.Instance.ConnectionToXTServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ConnectionToXTServerOptions.XtTest.Code))
			{
				taskForTest.RunTask();
			}

			var loadedMessages = Factory.Load<EDIInterchange>(new ZQuery(EDIInterchangeSchema.EI_From, "XH"));
			AssertEquals(3, loadedMessages.Length);
			AssertContainsExactElementsInAnyOrder(
				incomingMessageList.Select(inMsg => $"{EDIInterchange.Status.Queued} - {EDIInterchange.TransportType.tXT} - {inMsg.Value.MsgBody}"),
				loadedMessages.Select(em => $"{em.EI_Status} - {em.EI_TransportType} - {em.EI_BodyText}")
				);
		}

		public void TestTaskIsDisabledInTest()
		{
			var methodInfo = typeof(TestDirectXtInboundServiceTask).GetMethod(nameof(TestDirectXtInboundServiceTask.CheckIsClientProductionDatabase));
			Assert("[HostedServiceRequirement] is applied", Attribute.IsDefined(methodInfo, typeof(HostedServiceRequirementAttribute)));

			var taskCheck = TestDirectXtInboundServiceTask.CheckIsClientProductionDatabase();
			AssertEquals("This is not a production system.", taskCheck);
		}

		public void TestInitializationFailure()
		{
			var logger = new TestUtils.TestLogger();
			var processor = new TestDirectXtInboundProcessor(logger);
			var config = new Configuration
			{
				Connect = "localhost",
				CA = "C",
				Application = new Application { URI = "U", Password = "P" }
			};

			var exception = AssertExceptionThrown<MsgServerConnectionException>(() => ((IInterchangeProcessor)processor).Process(config, new CancellationToken()));

			AssertEquals("Exception Message", Enterprise.xTMessaging.Shared.Utils.RpcErrorReportKey, exception.Message);

			var loadedMessages = Factory.Load<EDIInterchange>(new ZQuery(EDIInterchangeSchema.EI_From, "XH"));
			AssertEquals(0, loadedMessages.Length);

			ErrorReporter.Clear();
		}

		public void TestMinimumPeriod()
		{
			AssertEquals("5minutes", GetHostedServiceAttributes().Single().MinimumPeriod);
		}

		public void TestIsProduction()
		{
			AssertEquals(false, new TestDirectXtInboundServiceTaskTestWrapper().IsProduction());
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();
	}
}
