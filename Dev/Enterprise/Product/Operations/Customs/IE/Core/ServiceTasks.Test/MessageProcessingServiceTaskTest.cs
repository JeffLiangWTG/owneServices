using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Messaging.Testing;
using Enterprise.Customs.ServiceTasks.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using EDIMessage = Enterprise.Messaging.Business.EDIMessage;

namespace Enterprise.Customs.IE.ServiceTasks.Testing
{
	[TestedType(typeof(MessageProcessingServiceTask))]
	public class MessageProcessingServiceTaskTest : BranchMessageProcessorServiceTest<MessageProcessingServiceTask>
	{
		public void TestServiceAttribute()
		{
			AssertSingleHostedServiceAttribute("IEM", "IE Customs Message Processor", "IEC");
		}

		public void TestMinimumPeriod()
		{
			var hostedServiceAttribute = GetHostedServiceAttributes().Single();
			AssertEquals("MinimumPeriod", "60Seconds", hostedServiceAttribute.MinimumPeriod);
		}

		public void TestProcessingNCTSMessage()
		{
			var declaration = Factory.New<JobDeclaration>();
			var outgoingMessage = Factory.New<EDIMessage>();
			outgoingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.IECustomsNCTS;
			outgoingMessage.EM_ApplicationReference = "6debb28a-c9b0-44fb-9e85-0737b42aef48";
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			outgoingMessage.EM_LinkedObject = entryHeader;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive; // to allow saving of message number
			outgoingMessage.EM_MessageNum = "TEST1234567890";
			var incomingMessage = Factory.New<EDIMessage>();
			incomingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.IECustomsNCTS;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_ApplicationReference = "6debb28a-c9b0-44fb-9e85-0737b42aef48";
			incomingMessage.EM_MessageType = "!@#";
			incomingMessage.EM_LinkedObject = entryHeader;
			incomingMessage.EM_Status = EDIMessage.Status.PreProcessedOK;
			Factory.Save();
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			Factory.Save();
			ErrorReporter.Clear();
			var serviceTask = new MessageProcessingServiceTask();
			var logger = new TestServiceLogger();
			serviceTask.ServiceLogger = logger;
			serviceTask.RunTask();
			var message = NewFactory().Load<EDIMessage>(incomingMessage.PK);
			AssertEquals("EM_Status should be failed as '!@#' is an invalid message type", EDIMessage.Status.Failed, message.EM_Status);
		}

		public void TestHostedServiceRequirement()
		{
			var methodInfo = typeof(MessageProcessingServiceTask).GetMethod(nameof(MessageProcessingServiceTask.IsRequired));
			Assert("HostedServiceRequirement is applied", Attribute.IsDefined(methodInfo, typeof(HostedServiceRequirementAttribute)));
		}

		protected override MessageProcessingServiceTask CreateServiceTask() => new MessageProcessingServiceTask();

		protected override BranchMessageProcessorServiceTestHelperData SetupDataForTesting()
		{
			var declaration = Factory.New<JobDeclaration>();
			var outgoingMessage = Factory.New<AISOutboundEDIMessage>();
			outgoingMessage.EM_ApplicationReference = "6debb28a-c9b0-44fb-9e85-0737b42aef48";
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			outgoingMessage.EM_LinkedObject = entryHeader;
			Factory.Save();

			var interchange = AISInterchangeProcessorTestHelper.CreateAISVersion2_0IM415VInterchange(Factory, "6debb28a-c9b0-44fb-9e85-0737b42aef48", "A", "ACPTESTIM0990446123456", "21IEDUB11A782454R2", new ZDateTime(2023, 08, 11));
			Factory.Save();
			var serviceTask = new MessageRetrievingServiceTask();
			var logger = new TestServiceLogger();
			serviceTask.ServiceLogger = logger;
			serviceTask.RunTask();

			var message = interchange.ContainedMessages[0];
			return new BranchMessageProcessorServiceTestHelperData { MessagePK = message.PK };
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => new[]
		{
			new TaskNudgeInformationForTest(
				table: EDIMessageSchema.Constants.TableName,
				queueName: "IE Export Message Processing",
				EDIMessageSchema.Constants.EM_Status + "=QUE",
				EDIMessageSchema.Constants.EM_ApplicationCode + "=IEE",
				EDIMessageSchema.Constants.EM_IsActive + "=Y",
				EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL",
				EDIMessageSchema.Constants.EM_ReceiveTransmit + "=RCV"
			),
			new TaskNudgeInformationForTest(
				table: EDIMessageSchema.Constants.TableName,
				queueName: "IE Export Message Pre-Processing",
				EDIMessageSchema.Constants.EM_Status + "=PPS",
				EDIMessageSchema.Constants.EM_ApplicationCode + "=IEE",
				EDIMessageSchema.Constants.EM_IsActive + "=Y",
				EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL",
				EDIMessageSchema.Constants.EM_ReceiveTransmit + "=RCV"
			),
			new TaskNudgeInformationForTest(
				table: EDIMessageSchema.Constants.TableName,
				queueName: "IE Import Message Processing",
				EDIMessageSchema.Constants.EM_Status + "=QUE",
				EDIMessageSchema.Constants.EM_ApplicationCode + "=IEI",
				EDIMessageSchema.Constants.EM_IsActive + "=Y",
				EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL",
				EDIMessageSchema.Constants.EM_ReceiveTransmit + "=RCV"
			),
			new TaskNudgeInformationForTest(
				table: EDIMessageSchema.Constants.TableName,
				queueName: "IE Import Message Pre-Processing",
				EDIMessageSchema.Constants.EM_Status + "=PPS",
				EDIMessageSchema.Constants.EM_ApplicationCode + "=IEI",
				EDIMessageSchema.Constants.EM_IsActive + "=Y",
				EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL",
				EDIMessageSchema.Constants.EM_ReceiveTransmit + "=RCV"
			),
			new TaskNudgeInformationForTest(
				table: EDIMessageSchema.Constants.TableName,
				queueName: "IE Import UCC5 Message Processing",
				EDIMessageSchema.Constants.EM_Status + "=QUE",
				EDIMessageSchema.Constants.EM_ApplicationCode + "=IE5",
				EDIMessageSchema.Constants.EM_IsActive + "=Y",
				EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL",
				EDIMessageSchema.Constants.EM_ReceiveTransmit + "=RCV"
			),
			new TaskNudgeInformationForTest(
				table: EDIMessageSchema.Constants.TableName,
				queueName: "IE Import UCC5 Message Pre-Processing",
				EDIMessageSchema.Constants.EM_Status + "=PPS",
				EDIMessageSchema.Constants.EM_ApplicationCode + "=IE5",
				EDIMessageSchema.Constants.EM_IsActive + "=Y",
				EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL",
				EDIMessageSchema.Constants.EM_ReceiveTransmit + "=RCV"
			),
			new TaskNudgeInformationForTest(
				table: EDIMessageSchema.Constants.TableName,
				queueName: "IE EMCS Message Processing",
				EDIMessageSchema.Constants.EM_Status + "=QUE",
				EDIMessageSchema.Constants.EM_ApplicationCode + "=IEM",
				EDIMessageSchema.Constants.EM_IsActive + "=Y",
				EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL",
				EDIMessageSchema.Constants.EM_ReceiveTransmit + "=RCV"
			),
			new TaskNudgeInformationForTest(
				table: EDIMessageSchema.Constants.TableName,
				queueName: "IE EMCS Message Pre-Processing",
				EDIMessageSchema.Constants.EM_Status + "=PPS",
				EDIMessageSchema.Constants.EM_ApplicationCode + "=IEM",
				EDIMessageSchema.Constants.EM_IsActive + "=Y",
				EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL",
				EDIMessageSchema.Constants.EM_ReceiveTransmit + "=RCV"
			),
			new TaskNudgeInformationForTest(
				table: EDIMessageSchema.Constants.TableName,
				queueName: "IE NCTS Message Processing",
				EDIMessageSchema.Constants.EM_Status + "=QUE",
				EDIMessageSchema.Constants.EM_ApplicationCode + "=IEN",
				EDIMessageSchema.Constants.EM_IsActive + "=Y",
				EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL",
				EDIMessageSchema.Constants.EM_ReceiveTransmit + "=RCV"
			),
			new TaskNudgeInformationForTest(
				table: EDIMessageSchema.Constants.TableName,
				queueName: "IE NCTS Message Pre-Processing",
				EDIMessageSchema.Constants.EM_Status + "=PPS",
				EDIMessageSchema.Constants.EM_ApplicationCode + "=IEN",
				EDIMessageSchema.Constants.EM_IsActive + "=Y",
				EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL",
				EDIMessageSchema.Constants.EM_ReceiveTransmit + "=RCV"
			),
			new TaskNudgeInformationForTest(
				table: EDIMessageSchema.Constants.TableName,
				queueName: "IE Customs And Excise Report Message Processing",
				EDIMessageSchema.Constants.EM_Status + "=QUE",
				EDIMessageSchema.Constants.EM_ApplicationCode + "=IER",
				EDIMessageSchema.Constants.EM_IsActive + "=Y",
				EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL",
				EDIMessageSchema.Constants.EM_ReceiveTransmit + "=RCV"
			),
			new TaskNudgeInformationForTest(
				table: EDIMessageSchema.Constants.TableName,
				queueName: "IE Customs And Excise Report Message Pre-Processing",
				EDIMessageSchema.Constants.EM_Status + "=PPS",
				EDIMessageSchema.Constants.EM_ApplicationCode + "=IER",
				EDIMessageSchema.Constants.EM_IsActive + "=Y",
				EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL",
				EDIMessageSchema.Constants.EM_ReceiveTransmit + "=RCV"
			),
			new TaskNudgeInformationForTest(
				table: EDIMessageSchema.Constants.TableName,
				queueName: "IE PBN Message Processing",
				EDIMessageSchema.Constants.EM_Status + "=QUE",
				EDIMessageSchema.Constants.EM_ApplicationCode + "=IEP",
				EDIMessageSchema.Constants.EM_IsActive + "=Y",
				EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL",
				EDIMessageSchema.Constants.EM_ReceiveTransmit + "=RCV"
			),
			new TaskNudgeInformationForTest(
				table: EDIMessageSchema.Constants.TableName,
				queueName: "IE PBN Message Pre-Processing",
				EDIMessageSchema.Constants.EM_Status + "=PPS",
				EDIMessageSchema.Constants.EM_ApplicationCode + "=IEP",
				EDIMessageSchema.Constants.EM_IsActive + "=Y",
				EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL",
				EDIMessageSchema.Constants.EM_ReceiveTransmit + "=RCV"
			)
		};

		protected override void AssertResult(BusinessObjectFactory factory, BranchMessageProcessorServiceTestHelperData testData, MessageProcessingServiceTask serviceTask)
		{
			var message = factory.Load<EDIMessage>(testData.MessagePK);
			AssertEquals("EM_Status", EDIMessage.Status.ProcessedOK, message.EM_Status);
		}
	}
}
