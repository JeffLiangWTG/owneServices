using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Manifest.ICS2.Business;
using Enterprise.Customs.ServiceTasks.Testing;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.EU.Manifest.ICS2.ServiceTasks.Test
{
	[TestedType(typeof(MessageProcessorServiceTask))]
	sealed class MessageProcessorServiceTaskTest : BranchMessageProcessorServiceTest<MessageProcessorServiceTask>
	{
		public void TestServiceAttribute()
		{
			AssertSingleHostedServiceAttribute("EUP", "EU ICS2 Message Processor", "ICS");
			AssertEquals(true, GetHostedServiceAttributes().Single().CanRunInAnyBranch);
		}

		public void TestMinimumPeriod()
		{
			AssertEquals("60Seconds", GetHostedServiceAttributes().Single().MinimumPeriod);
		}

		public void TestProcessXERMessage()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_MessageStatus = MessageStatusList.Codes.AwaitingResponse;

			var outgoingInterchange = Factory.NewWithValidTestData<EDIInterchange>();
			outgoingInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			outgoingInterchange.EI_ApplicationCode = ApplicationCodeList.Codes.EUICS2;
			outgoingInterchange.EI_SessionGUID = ZGuid.BrettsGuid;
			outgoingInterchange.EI_TransportType = EDIInterchangeTransportTypeList.Codes.xT;

			var outgoingMessage = Factory.NewWithValidTestData<ICS2OutboundEDIMessage>();
			outgoingMessage.EM_EI = outgoingInterchange.PK;
			outgoingMessage.EM_ApplicationCode = EDIInterchange.ApplicationCodes.IC2;
			outgoingMessage.EM_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			outgoingMessage.EM_MessageType = MessageTypes.Codes.F14;
			outgoingMessage.EM_LinkedObject = header;
			outgoingMessage.EM_Status = EDIMessage.Status.ProcessedOK;

			var incomingInterchange = Factory.NewWithValidTestData<EDIInterchange>();
			incomingInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			incomingInterchange.EI_ApplicationCode = ApplicationCodeList.Codes.EUICS2;
			incomingInterchange.EI_SessionGUID = ZGuid.BrettsGuid;
			incomingInterchange.EI_TransportType = EDIInterchangeTransportTypeList.Codes.xT;
			incomingInterchange.EI_InterchangeType = Customs.Business.MessageProcessors.UCMP.Constant.MessageTypes.XER;

			var incomingMessage = Factory.NewWithValidTestData<ICS2InboundEDIMessage>();
			incomingMessage.EM_EI = incomingInterchange.PK;
			incomingMessage.EM_ApplicationCode = EDIInterchange.ApplicationCodes.IC2;
			incomingMessage.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
			incomingMessage.EM_MessageType = Customs.Business.MessageProcessors.UCMP.Constant.MessageTypes.XER;
			incomingMessage.EM_MessageSubType = Customs.Business.MessageProcessors.UCMP.Constant.MessageTypes.XER;
			incomingMessage.EM_Status = EDIMessageStatusList.Codes.Queued;
			incomingMessage.EM_MessageText = @"
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
	<Event>
		<EventTime>2025-04-08 19:21:42.974</EventTime>
		<EventType>IRJ</EventType>
		<EventParameters>
			<MessageType>XER</MessageType>
			<Type>BusinessError</Type>
			<Reason>[3] POST TaskCancelledException: The Http Request task was cancelled due to timeout 100000s</Reason>
		</EventParameters>
	</Event>
</UniversalEvent>";

			Factory.Save();
			var serviceTask = CreateServiceTask();
			var logger = new TestServiceLogger();
			serviceTask.ServiceLogger = logger;
			serviceTask.RunTask();
			var message = NewFactory().Load<EDIMessage>(incomingMessage.PK);
			AssertEquals("EM_Status", EDIMessage.Status.ProcessedOK, message.EM_Status);
		}

		protected override MessageProcessorServiceTask CreateServiceTask() => new MessageProcessorServiceTask();

		protected override BranchMessageProcessorServiceTestHelperData SetupDataForTesting()
		{
			var manifestHeader = Factory.New<AsycudaManifestHeader>();

			var manifestHeaderLRNEntryNum = CusEntryNumber.New(manifestHeader, CusEntryNumberTypes.Standard.LocalReferenceNumber, manifestHeader.AMA_RN_NKCountry);
			manifestHeaderLRNEntryNum.CE_EntryNum = "localReferenceNumber";

			var msg = Factory.New<TestEdiMessage>();
			msg.EM_Status = "QUE";
			msg.EM_ReceiveTransmit = "RCV";
			msg.EM_MessageType = "N99";
			msg.EM_ApplicationCode = "IC2";
			msg.EM_MessageText = @"<?xml version=""1.0"" encoding=""utf-8""?>
												<IE3N99 xmlns=""urn:wco:datamodel:eu:ics2:2"">
												  <functionalReference>localReferenceNumber</functionalReference>
												</IE3N99>";
			return new BranchMessageProcessorServiceTestHelperData { MessagePK = msg.PK };
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => new[]
		{
			new TaskNudgeInformationForTest(
				table: EDIMessageSchema.Constants.TableName,
				queueName: "EU ICS2 Message Processor",
				EDIMessageSchema.Constants.EM_Status + "=" + "QUE",
				EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + "RCV",
				EDIMessageSchema.Constants.EM_IsActive + "=Y",
				EDIMessageSchema.Constants.EM_ApplicationCode + "=" + "IC2"
				),
			new TaskNudgeInformationForTest(
				table: EDIMessageSchema.Constants.TableName,
				queueName: "EU ICS2 Message Pre-Processing",
				EDIMessageSchema.Constants.EM_Status + "=" + "PPS",
				EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + "RCV",
				EDIMessageSchema.Constants.EM_IsActive + "=Y",
				EDIMessageSchema.Constants.EM_ApplicationCode + "=" + "IC2"
				)
		};

		protected override void AssertResult(BusinessObjectFactory factory, BranchMessageProcessorServiceTestHelperData testData, MessageProcessorServiceTask serviceTask)
		{
			var message = factory.Load<EDIMessage>(testData.MessagePK);
			AssertEquals("Message should have been processed.", EDIMessage.Status.ProcessedOK, message.EM_Status);
		}
	}
}
