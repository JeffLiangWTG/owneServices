using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.xTMessaging.Business;
using Enterprise.xTMessaging.Shared;
using Enterprise.xTMessaging.Shared.Test;
using Xware.Xt.Grpc.Application;
using Xware.Xt.Grpc.Config;
using static Enterprise.xTMessaging.Shared.Test.TestUtils;

namespace Enterprise.xTMessaging.ServiceTasks.Test
{
	class InboundInterchangeProcessorTest : TestCaseWithFactory
	{
		public void TestInitializationFailure()
		{
			using (DirectxTMessagingRegistry.Instance.EnableXTIServiceTask.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var logger = new TestLogger();
				var processor = new InboundInterchangeProcessor(logger);

				var config = new Configuration
				{
					Connect = "localhost",
					CA = "C",
					Application = new Application { URI = "U", Password = "P" }
				};

				var exception = AssertExceptionThrown<MsgServerConnectionException>(() => ((IInterchangeProcessor)processor).Process(config, new CancellationToken()));
				AssertEquals("Exception Message", Shared.Utils.RpcErrorReportKey, exception.Message);
			}
		}

		public void TestProcess()
		{
			using (DirectxTMessagingRegistry.Instance.EnableXTIServiceTask.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var messageMetaDataForTest = new Dictionary<string, string>();
				messageMetaDataForTest.Add(Constants.CustomMsgAttributes.ApplicationCode, "TST");
				messageMetaDataForTest.Add(Constants.CustomMsgAttributes.SourceParty, "TST");
				messageMetaDataForTest.Add(Constants.CustomMsgAttributes.DestinationParty, "TST");
				messageMetaDataForTest.Add(Constants.CustomMsgAttributes.MessageTrackingID, "6B72FDCB-BDDC-47C6-B263-F1245A084AD9");
				var incomingMessageList = new Dictionary<ulong, ITestIncomingMessage>();
				incomingMessageList[1u] = new TestIncomingMessagePreparation() { TestInstruction = TestInstructionValues.SUCCESS, MsgBody = "Body1", AckResponse = ErrorCode.ErrOk, MetaData = messageMetaDataForTest };
				incomingMessageList[2u] = new TestIncomingMessagePreparation() { TestInstruction = TestInstructionValues.SUCCESS, MsgBody = "Body2", AckResponse = ErrorCode.ErrOk, MetaData = messageMetaDataForTest };
				incomingMessageList[3u] = new TestIncomingMessagePreparation() { TestInstruction = TestInstructionValues.SUCCESS, MsgBody = "Body3", AckResponse = ErrorCode.ErrOk, MetaData = messageMetaDataForTest };

				var testLogger = new TestLogger();

				var processor = new InboundInterchangeProcessorTestWrapper(testLogger);
				(var msgClientProvider, _) = TestUtils.GetMockedMsgClientProviderWithIncomingMessages(incomingMessageList);
				processor.ClientProvider = msgClientProvider;
				using (DirectxTMessagingRegistry.Instance.XTIdleConnectionKeepAliveInSeconds.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
				{
					((IInterchangeProcessor)processor).Process(new Configuration(), new CancellationToken());
				}

				var logs = string.Join("|", testLogger.AllLogs.Select(l => l.Message).ToList());
				var infoLogs = testLogger.InfoLogs.ToList();
				CombineAssertions(() =>
				{
					AssertContains("Start Initialize DirectxT Connector.", logs);
					AssertContains("Finish Initialize DirectxT Connector. Total Time: ", logs);
					AssertContains("Start Receive Message(s).", logs);
					AssertContains("3 message(s) is ready to be received", logs);
					AssertContains("Msg:1 has been acknowledged with status StatusOk/0", logs);
					AssertContains("Msg:2 has been acknowledged with status StatusOk/0", logs);
					AssertContains("Msg:3 has been acknowledged with status StatusOk/0", logs);
					AssertContains("Finish Receive Message(s). Total Time: ", logs);
					AssertCollectionContains(@"Initialization completed - connect to GRPC client", infoLogs);
					AssertCollectionContains("3 message(s) is ready to be received", infoLogs);
					AssertCollectionContains("Msg:1 has been acknowledged with status StatusOk/0", infoLogs);
					AssertCollectionContains("Msg:2 has been acknowledged with status StatusOk/0", infoLogs);
					AssertCollectionContains("Msg:3 has been acknowledged with status StatusOk/0", infoLogs);
					AssertEquals(true, infoLogs.Any(l => l.StartsWith("Finish Receive 3 Interchange(s)")));
					AssertCollectionContains("0 message(s) is ready to be received", infoLogs);
					AssertCollectionContains("Logged Off the GRPC client", infoLogs);
				});

				var loadedMessages = Factory.Load<EDIInterchange>(new ZQuery());
				AssertEquals(3, loadedMessages.Length);
				AssertContainsExactElementsInAnyOrder(
					incomingMessageList.Select(inMsg => $"{EDIInterchangeStatusList.Codes.Queued} - {EDIInterchangeTransportTypeList.Codes.xT} - {inMsg.Value.MsgBody}"),
					loadedMessages.Select(em => $"{em.EI_Status} - {em.EI_TransportType} - {em.EI_BodyText}")
					);
			}
		}

		public void TestProcessWhenDisabled()
		{
			using (DirectxTMessagingRegistry.Instance.EnableXTIServiceTask.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var testLogger = new TestLogger();
				var processor = new InboundInterchangeProcessorTestWrapper(testLogger);
				processor.ClientProvider = TestUtils.GetMoqMsgClientProvider();
				((IInterchangeProcessor)processor).Process(new Configuration(), new CancellationToken());

				var logs = string.Join("|", testLogger.AllLogs.Select(l => l.Message));
				CombineAssertions(() =>
				{
					AssertContains("Inbound Processing is currently disabled as this system has not sent any requests to xT in the past month.", logs);
				});
			}
		}

		public void TestProcessIncomingMessageIsBinaryReadableAsEIBodyText()
		{
			using (DirectxTMessagingRegistry.Instance.EnableXTIServiceTask.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var messageMetaDataForTest = new Dictionary<string, string>();
				messageMetaDataForTest.Add(Constants.CustomMsgAttributes.ApplicationCode, "TST");
				messageMetaDataForTest.Add(Constants.CustomMsgAttributes.SourceParty, "TST");
				messageMetaDataForTest.Add(Constants.CustomMsgAttributes.DestinationParty, "TST");
				messageMetaDataForTest.Add(Constants.CustomMsgAttributes.MessageTrackingID, "6B72FDCB-BDDC-47C6-B263-F1245A084AD9");
				var incomingMessageList = new Dictionary<ulong, ITestIncomingMessage>();
				incomingMessageList[1u] = new TestIncomingBinaryMessagePreparation() { TestInstruction = TestInstructionValues.SUCCESS, MsgBody = Encoding.UTF8.GetBytes("Body1"), AckResponse = ErrorCode.ErrOk, MetaData = messageMetaDataForTest };
				incomingMessageList[2u] = new TestIncomingBinaryMessagePreparation() { TestInstruction = TestInstructionValues.SUCCESS, MsgBody = Encoding.ASCII.GetBytes("Body2"), AckResponse = ErrorCode.ErrOk, MetaData = messageMetaDataForTest };
				incomingMessageList[3u] = new TestIncomingBinaryMessagePreparation() { TestInstruction = TestInstructionValues.SUCCESS, MsgBody = Encoding.Unicode.GetBytes("Body3"), AckResponse = ErrorCode.ErrOk, MetaData = messageMetaDataForTest };
				incomingMessageList[4u] = new TestIncomingBinaryMessagePreparation() { TestInstruction = TestInstructionValues.SUCCESS, MsgBody = Encoding.UTF32.GetBytes("Body4"), AckResponse = ErrorCode.ErrOk, MetaData = messageMetaDataForTest };
#if NETFRAMEWORK
				incomingMessageList[5u] = new TestIncomingBinaryMessagePreparation() { TestInstruction = TestInstructionValues.SUCCESS, MsgBody = Encoding.UTF8.GetBytes("Body5"), AckResponse = ErrorCode.ErrOk, MetaData = messageMetaDataForTest };
#endif
				incomingMessageList[6u] = new TestIncomingBinaryMessagePreparation() { TestInstruction = TestInstructionValues.SUCCESS, MsgBody = Encoding.GetEncoding(1252).GetBytes("Body6"), AckResponse = ErrorCode.ErrOk, MetaData = messageMetaDataForTest };
				incomingMessageList[7u] = new TestIncomingBinaryMessagePreparation() { TestInstruction = TestInstructionValues.SUCCESS, MsgBody = Encoding.GetEncoding("iso-8859-1").GetBytes("Body7"), AckResponse = ErrorCode.ErrOk, MetaData = messageMetaDataForTest };

				var testLogger = new TestLogger();

				var processor = new InboundInterchangeProcessorTestWrapper(testLogger);
				(var msgClientProvider, _) = TestUtils.GetMockedMsgClientProviderWithIncomingMessages(incomingMessageList);
				processor.ClientProvider = msgClientProvider;
				((IInterchangeProcessor)processor).Process(new Configuration(), new CancellationToken());

				var loadedMessages = Factory.Load<EDIInterchange>(new ZQuery());

#if NETFRAMEWORK
				AssertEquals(7, loadedMessages.Length);
#elif NET
				AssertEquals(6, loadedMessages.Length);
#endif
				AssertContainsExactElementsInAnyOrder(
					incomingMessageList.Select(inMsg => $"{EDIInterchangeStatusList.Codes.Queued} - {EDIInterchangeTransportTypeList.Codes.xT} - {Encoding.UTF8.GetString(inMsg.Value.MsgBody as byte[])}"),
					loadedMessages.Select(em => $"{em.EI_Status} - {em.EI_TransportType} - {em.EI_BodyText}")
					);
			}
		}

		public void TestProcessIncomingMessageIsBinaryNotReadableAsEIBodyText()
		{
			using (MemoryStream outputStream = new MemoryStream())
			using (DirectxTMessagingRegistry.Instance.EnableXTIServiceTask.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var messageMetaDataForTest = new Dictionary<string, string>();
				messageMetaDataForTest.Add(Constants.CustomMsgAttributes.ApplicationCode, "TST");
				messageMetaDataForTest.Add(Constants.CustomMsgAttributes.SourceParty, "TST");
				messageMetaDataForTest.Add(Constants.CustomMsgAttributes.DestinationParty, "TST");
				messageMetaDataForTest.Add(Constants.CustomMsgAttributes.MessageTrackingID, "6B72FDCB-BDDC-47C6-B263-F1245A084AD9");
				var incomingMessageList = new Dictionary<ulong, ITestIncomingMessage>();

				var inputBytes = Encoding.UTF8.GetBytes("�😊�😊�😊�😊");

				using (GZipStream gzipStream = new GZipStream(outputStream, CompressionMode.Compress))
				{
					gzipStream.Write(inputBytes, 0, inputBytes.Length);
				}

				var compressedBytes = outputStream.ToArray();

				incomingMessageList[8u] = new TestIncomingBinaryMessagePreparation() { TestInstruction = TestInstructionValues.SUCCESS, MsgBody = compressedBytes, AckResponse = ErrorCode.ErrOk, MetaData = messageMetaDataForTest };

				var testLogger = new TestLogger();

				var processor = new InboundInterchangeProcessorTestWrapper(testLogger);
				(var msgClientProvider, _) = TestUtils.GetMockedMsgClientProviderWithIncomingMessages(incomingMessageList);
				processor.ClientProvider = msgClientProvider;
				((IInterchangeProcessor)processor).Process(new Configuration(), new CancellationToken());

				var loadedMessages = Factory.Load<EDIInterchange>(new ZQuery());
				AssertEquals(1, loadedMessages.Length);
				AssertContainsExactElementsInAnyOrder(
					incomingMessageList.Select(inMsg => $"{EDIInterchangeStatusList.Codes.Queued} - {EDIInterchangeTransportTypeList.Codes.xT} - {inMsg.Value.MsgBody as byte[]}"),
					loadedMessages.Select(em => $"{em.EI_Status} - {em.EI_TransportType} - {em.GetEI_BodyDataReader().ReadFully()}")
					);
			}
		}
	}
}
