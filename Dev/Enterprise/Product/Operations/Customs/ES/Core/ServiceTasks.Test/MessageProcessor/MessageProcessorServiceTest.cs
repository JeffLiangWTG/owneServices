using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.ServiceTasks.Testing;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.Testing;
using Enterprise.ServiceManager.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.ES.ServiceTasks.Testing
{
	[TestedType(typeof(MessageProcessorService))]
	class MessageProcessorServiceTest : BranchMessageProcessorServiceTest<MessageProcessorService>
	{
		public void TestHostedServiceAttribute()
		{
			var hostedServiceAttributes = GetHostedServiceAttributes();
			AssertEquals("Expected single attribute", 1, hostedServiceAttributes.Length);
			var hostedServiceAttribute = hostedServiceAttributes.Single();

			CombineAssertions(() =>
			{
				AssertEquals("Code", "PES", hostedServiceAttribute.Code);
				AssertEquals("Description", "ES Customs Message Processing", hostedServiceAttribute.Description);
				AssertEquals("Category", "ESC", hostedServiceAttribute.Category);
				AssertEquals("RequiresCompanyInCountry", Enterprise.Core.Constants.CountryCodes.Spain, hostedServiceAttribute.RequiresCompanyInCountry);
				AssertEquals("CanRunInAnyBranch", true, hostedServiceAttribute.CanRunInAnyBranch);
				AssertEquals("AllowsMultipleInstances", false, hostedServiceAttribute.AllowsMultipleInstances);
				AssertEquals("MinimumPeriod", "1minute", hostedServiceAttribute.MinimumPeriod);
				AssertEquals("DefaultScheduleRunEvery", "15minutes", hostedServiceAttribute.DefaultScheduleRunEvery);
			});
		}

		public void TestInitialiseSchedule()
		{
			var serviceTask = new MessageProcessorService();
			InitialiseTaskSchedule(serviceTask, out StmServiceTask taskSchedule);

			CombineAssertions(() =>
			{
				AssertEquals("IsActive", ZBool.True, taskSchedule.SST_Active);
				Assert("TaskPeriod", taskSchedule.Recurrence.MinutesRange);
				AssertEquals("TaskPeriodCount", 15, taskSchedule.Recurrence.Period);
				AssertEquals("WeekDaysOnly", ZBool.False, taskSchedule.Recurrence.WeekDaysOnly);
				AssertEquals("Is DailyStartTime empty?", true, taskSchedule.Recurrence.CalcDailyStartTimeUtc.IsEmpty);
			});
		}

		public void TestRunTask()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();

			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			Factory.Save();

			var message1_EHub = CreateDataForPreProcessing(entryHeader, DeclarationMessageTypeList.Codes.Export, entryHeader.CH_BGMReference, "UNH+02110053624233+CUSRES:1:921:UN:ECS003'BGM+962+S900052890/1+11'NAD+EX+A78587268:167:148'NAD+1+ESA78587268:167:148'DTM+148:2001021100:201'DTM+268:20200401:102'GIS+4:117:148'GIS+24:119:A2:1'RFF+ABT:20ES00999910000035'AUT+LT6B5QJ98HC6DHNY+LEVA'DTM+204:2001021100:201'AUT+3J9MCS7TAM3KLZQC+T2LF'DTM+204:2001021100:201'UNT+14+02110053624233'UNZ+1+02110053624233'", EDIInterchange.TransportType.eHub);

			var message2 = CreateDataForPreProcessing(entryHeader, "TS1", entryHeader.CH_BGMReference, "UNH+02110053624233+CUSRES:1:921:UN:ECS003'BGM+962+S900052890/1+11'NAD+EX+A78587268:167:148'NAD+1+ESA78587268:167:148'DTM+148:2001021100:201'DTM+268:20200401:102'GIS+4:117:148'GIS+24:119:A2:1'RFF+ABT:20ES00999910000035'AUT+LT6B5QJ98HC6DHNY+LEVA'DTM+204:2001021100:201'AUT+3J9MCS7TAM3KLZQC+T2LF'DTM+204:2001021100:201'UNT+14+02110053624233'UNZ+1+02110053624233'", EDIInterchange.TransportType.eHub);

			var message3_DirectxT = CreateDataForPreProcessing(entryHeader, DeclarationMessageTypeList.Codes.Export, ZString.Empty, "UNH+02110053624233+CUSRES:1:921:UN:ECS003'BGM+962+S900052890/1+11'NAD+EX+A78587268:167:148'NAD+1+ESA78587268:167:148'DTM+148:2001021100:201'DTM+268:20200401:102'GIS+4:117:148'GIS+24:119:A2:1'RFF+ABT:20ES00999910000035'AUT+LT6B5QJ98HC6DHNY+LEVA'DTM+204:2001021100:201'AUT+3J9MCS7TAM3KLZQC+T2LF'DTM+204:2001021100:201'UNT+14+02110053624233'UNZ+1+02110053624233'", EDIInterchange.TransportType.xT);

			var message4 = CreateDataForPreProcessing(entryHeader, "TS2", entryHeader.CH_BGMReference, "UNH+02110053624233+CUSRES:1:921:UN:ECS003'BGM+962+S900052890/1+11'NAD+EX+A78587268:167:148'NAD+1+ESA78587268:167:148'DTM+148:2001021100:201'DTM+268:20200401:102'GIS+4:117:148'GIS+24:119:A2:1'RFF+ABT:20ES00999910000035'AUT+LT6B5QJ98HC6DHNY+LEVA'DTM+204:2001021100:201'AUT+3J9MCS7TAM3KLZQC+T2LF'DTM+204:2001021100:201'UNT+14+02110053624233'UNZ+1+02110053624233'", EDIInterchange.TransportType.eHub);

			var message5_EHub_PPS = SetMessageAsPreProcessed(entryHeader, CreateDataForPreProcessing(entryHeader, DeclarationMessageTypeList.Codes.Export, entryHeader.CH_BGMReference, "UNH+02110053624233+CUSRES:1:921:UN:ECS003'BGM+962+S900052890/1+11'NAD+EX+A78587268:167:148'NAD+1+ESA78587268:167:148'DTM+148:2001021100:201'DTM+268:20200401:102'GIS+4:117:148'GIS+24:119:A2:1'RFF+ABT:20ES00999910000035'AUT+LT6B5QJ98HC6DHNY+LEVA'DTM+204:2001021100:201'AUT+3J9MCS7TAM3KLZQC+T2LF'DTM+204:2001021100:201'UNT+14+02110053624233'UNZ+1+02110053624233'", EDIInterchange.TransportType.eHub));

			var message6_PPS = SetMessageAsPreProcessed(entryHeader, CreateDataForPreProcessing(entryHeader, "TS1", entryHeader.CH_BGMReference, "UNH+02110053624233+CUSRES:1:921:UN:ECS003'BGM+962+S900052890/1+11'NAD+EX+A78587268:167:148'NAD+1+ESA78587268:167:148'DTM+148:2001021100:201'DTM+268:20200401:102'GIS+4:117:148'GIS+24:119:A2:1'RFF+ABT:20ES00999910000035'AUT+LT6B5QJ98HC6DHNY+LEVA'DTM+204:2001021100:201'AUT+3J9MCS7TAM3KLZQC+T2LF'DTM+204:2001021100:201'UNT+14+02110053624233'UNZ+1+02110053624233'", EDIInterchange.TransportType.eHub));

			var message7_DirectxT_PPS = SetMessageAsPreProcessed(entryHeader, CreateDataForPreProcessing(entryHeader, DeclarationMessageTypeList.Codes.Export, ZString.Empty, "UNH+02110053624233+CUSRES:1:921:UN:ECS003'BGM+962+S900052890/1+11'NAD+EX+A78587268:167:148'NAD+1+ESA78587268:167:148'DTM+148:2001021100:201'DTM+268:20200401:102'GIS+4:117:148'GIS+24:119:A2:1'RFF+ABT:20ES00999910000035'AUT+LT6B5QJ98HC6DHNY+LEVA'DTM+204:2001021100:201'AUT+3J9MCS7TAM3KLZQC+T2LF'DTM+204:2001021100:201'UNT+14+02110053624233'UNZ+1+02110053624233'", EDIInterchange.TransportType.xT));

			var message8_PPS = SetMessageAsPreProcessed(entryHeader, CreateDataForPreProcessing(entryHeader, "TS2", entryHeader.CH_BGMReference, "UNH+02110053624233+CUSRES:1:921:UN:ECS003'BGM+962+S900052890/1+11'NAD+EX+A78587268:167:148'NAD+1+ESA78587268:167:148'DTM+148:2001021100:201'DTM+268:20200401:102'GIS+4:117:148'GIS+24:119:A2:1'RFF+ABT:20ES00999910000035'AUT+LT6B5QJ98HC6DHNY+LEVA'DTM+204:2001021100:201'AUT+3J9MCS7TAM3KLZQC+T2LF'DTM+204:2001021100:201'UNT+14+02110053624233'UNZ+1+02110053624233'", EDIInterchange.TransportType.eHub));

			Factory.Save();

			ErrorReporter.Clear();
			var task = new MessageProcessorService();
			InitialiseTaskSchedule(task);
			RunTaskSchedule(task);

			CombineAssertions(() =>
			{
				message1_EHub.Reload();
				AssertEquals("Ehub EXP message with status QUE should be processed correctly", EDIMessage.Status.Received, message1_EHub.EM_Status);

				message2.Reload();
				AssertEquals("Ehub TS1 message with status QUE should not be processed", EDIMessage.Status.Queued, message2.EM_Status);

				message3_DirectxT.Reload();
				AssertEquals("Direct xT EXP message with status QUE should be processed correctly", EDIMessage.Status.Received, message3_DirectxT.EM_Status);

				message4.Reload();
				AssertEquals("Ehub TS2 message with status QUE should not be processed", EDIMessage.Status.Queued, message4.EM_Status);

				message5_EHub_PPS.Reload();
				AssertEquals("Ehub EXP message with status PPS should be processed correctly", EDIMessage.Status.Received, message5_EHub_PPS.EM_Status);

				message6_PPS.Reload();
				AssertEquals("Ehub TS1 message with status PPS should not be processed", EDIMessage.Status.PreProcessedOK, message6_PPS.EM_Status);

				message7_DirectxT_PPS.Reload();
				AssertEquals("Direct xT EXP message with status PPS should be processed correctly", EDIMessage.Status.Received, message7_DirectxT_PPS.EM_Status);

				message8_PPS.Reload();
				AssertEquals("Ehub TS2 message with status PPS should not be processed", EDIMessage.Status.PreProcessedOK, message8_PPS.EM_Status);

				AssertEquals("ServiceTaskThatRequiresSetUserContext is not reported", ZString.Empty, ErrorReporter.LastMessageReported);
				AssertNotContains("EnableImplicitUserContextAccessReporting message", "accesses environment current branch without setting the environment first", ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
			});
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						MessageProcessorService.FriendlyName + " QUE",
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.ESCustomsMessage,
						EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"),

					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						MessageProcessorService.FriendlyName + " PPS",
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.PreProcessedOK,
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.ESCustomsMessage,
						EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"),
				};
			}
		}

		protected override MessageProcessorService CreateServiceTask() => new MessageProcessorService();

		protected override BranchMessageProcessorServiceTestHelperData SetupDataForTesting()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			Factory.Save();

			entryHeader.CH_BGMReference = entryHeader.CH_BGMReference;

			var message = CreateDataForPreProcessing(entryHeader, DeclarationMessageTypeList.Codes.Export, entryHeader.CH_BGMReference, "UNH+02110053624233+CUSRES:1:921:UN:ECS003'BGM+962+S900052890/1+11'NAD+EX+A78587268:167:148'NAD+1+ESA78587268:167:148'DTM+148:2001021100:201'DTM+268:20200401:102'GIS+4:117:148'GIS+24:119:A2:1'RFF+ABT:20ES00999910000035'AUT+LT6B5QJ98HC6DHNY+LEVA'DTM+204:2001021100:201'AUT+3J9MCS7TAM3KLZQC+T2LF'DTM+204:2001021100:201'UNT+14+02110053624233'UNZ+1+02110053624233'", EDIInterchange.TransportType.eHub);
			return new BranchMessageProcessorServiceTestHelperData()
			{
				MessagePK = message.PK
			};
		}

		TestEdiMessage CreateDataForPreProcessing(CusEntryHeader entryHeader, ZString messageType, ZString applicationReference, ZString messageText, ZString interchangeTransportType)
		{
			var interchangeID = ZGuid.NewZGuid();

			var sentInterchange = Factory.New<EDIInterchange>();
			sentInterchange.EI_SessionGUID = interchangeID;
			sentInterchange.EI_ApplicationCode = ApplicationCodeList.Codes.ESCustomsMessage;
			sentInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			sentInterchange.EI_From = "TEST";
			sentInterchange.EI_To = SpanishCustomsTypeCodeList.Codes.EdifactSpanishCustoms;
			var sentMessageEntry = Factory.New<TestEdiMessage>();
			sentMessageEntry.EM_LinkedObject = entryHeader;
			sentMessageEntry.EM_ApplicationReference = "CertName";
			sentInterchange.ContainedMessages.Add(sentMessageEntry);

			var responseInterchange = Factory.New<EDIInterchange>();
			responseInterchange.EI_ApplicationCode = ApplicationCodeList.Codes.ESCustomsMessage;
			responseInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			responseInterchange.EI_SessionGUID = interchangeID;
			responseInterchange.EI_From = SpanishCustomsTypeCodeList.Codes.EdifactSpanishCustoms;
			responseInterchange.EI_To = "TEST";
			responseInterchange.EI_TransportType = interchangeTransportType;
			if (interchangeTransportType != EDIInterchange.TransportType.xT)
			{
				responseInterchange.EI_HeaderText = string.Format(@"<?xml version=""1.0"" encoding=""utf-8""?>
<Headers>
  <BrokerCode>AZ</BrokerCode>
  <CertificateName>CertName</CertificateName>
  <CertificateThumbPrint>CertThumbPrint</CertificateThumbPrint>
  <EntryReferenceNumber>1234123444</EntryReferenceNumber>
  <TestMessage>N</TestMessage>
  <SentEDIMessageNumber>1</SentEDIMessageNumber>
</Headers>");
			}

			var message = Factory.New<TestEdiMessage>();
			message.EM_ApplicationCode = ApplicationCodeList.Codes.ESCustomsMessage;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_IsActive = true;
			message.EM_MessageType = messageType;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_ApplicationReference = applicationReference;
			message.EM_MessageText = messageText;
			responseInterchange.ContainedMessages.Add(message);

			return message;
		}

		TestEdiMessage SetMessageAsPreProcessed(CusEntryHeader entryHeader, TestEdiMessage message)
		{
			message.EM_LinkedObject = entryHeader;
			message.EM_Status = EDIMessage.Status.PreProcessedOK;
			return message;
		}
	}
}
