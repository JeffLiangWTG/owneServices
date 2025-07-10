using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.xTMessaging.Business;
using Enterprise.xTMessaging.Shared;
using Enterprise.xTMessaging.Shared.Test;
using Enterprise.ZArchitecture.Schema;
using Grpc.Core;
using NUnit.Framework;
using Xware.Xt.Grpc.Application;
using Xware.Xt.Grpc.Config;

namespace Enterprise.xTMessaging.ServiceTasks.Test
{
	class OutboundInterchangeProcessorTest : TestCaseWithFactory
	{
		public void TestSending_ValidateInterchanges()
		{
			using (DirectxTMessagingRegistry.Instance.XTIdleConnectionKeepAliveInSeconds.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
			using (DirectxTMessagingRegistry.Instance.XTIdleConnectionRetryPauseInSeconds.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
			{
				var interchangeWithEmptyApplicationCode = TestUtils.CreateInterchangeForXT(Factory);
				interchangeWithEmptyApplicationCode.EI_ApplicationCode = "";
				interchangeWithEmptyApplicationCode.EI_InterchangeNum = "100001";
				var interchangeWithEmptyFrom = TestUtils.CreateInterchangeForXT(Factory);
				interchangeWithEmptyFrom.EI_InterchangeNum = "100002";
				interchangeWithEmptyFrom.EI_From = "";
				var interchangeValid = TestUtils.CreateInterchangeForXT(Factory);
				interchangeValid.EI_InterchangeNum = "100003";
				Factory.Save();
				var testLogger = new TestUtils.TestLogger();

				var (msgClientProvider, _, _) = TestUtils.GetMockedMsgClientProviderWithMessageInspection();
				var processor = new OutboundInterchangeProcessorTestWrapper(testLogger)
				{
					ClientProvider = msgClientProvider
				};
				((IInterchangeProcessor)processor).Process(new Configuration(), new CancellationToken());
				var interchangeWithEmptyApplicationCodeReloaded = new BusinessObjectFactory().Load<EDIInterchange>(interchangeWithEmptyApplicationCode.PK);
				AssertEquals(interchangeWithEmptyApplicationCodeReloaded.EI_Status, EDIInterchange.Status.Error);
				var interchangeWithEmptyFromReloaded = new BusinessObjectFactory().Load<EDIInterchange>(interchangeWithEmptyFrom.PK);
				AssertEquals(interchangeWithEmptyFromReloaded.EI_Status, EDIInterchange.Status.Error);
				var interchangeValidReloaded = new BusinessObjectFactory().Load<EDIInterchange>(interchangeValid.PK);
				AssertEquals(interchangeValidReloaded.EI_Status, EDIInterchange.Status.Sent);
				var logToBeAssertion = testLogger.ErrorLogs;
				AssertCollectionContains("Interchange: 100001 was set to ERR:EI_ApplicationCode should not be null.", logToBeAssertion);
				AssertCollectionContains("Interchange: 100002 was set to ERR:EI_From should not be null.", logToBeAssertion);
				AssertCollectionContains("Interchange: 100003 was set to SNT.", testLogger.InfoLogs);
			}
		}

		public void TestSending_ValidateInterchanges_SavingFail()
		{
			using (DirectxTMessagingRegistry.Instance.XTIdleConnectionKeepAliveInSeconds.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
			using (DirectxTMessagingRegistry.Instance.XTIdleConnectionRetryPauseInSeconds.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
			{
				var interchangeWithEmptyApplicationCode = TestUtils.CreateInterchangeForXT(Factory);
				interchangeWithEmptyApplicationCode.EI_ApplicationCode = "";
				interchangeWithEmptyApplicationCode.EI_InterchangeNum = "100001";
				var interchangeWithEmptyFrom = TestUtils.CreateInterchangeForXT(Factory);
				interchangeWithEmptyFrom.EI_InterchangeNum = "100002";
				interchangeWithEmptyFrom.EI_From = "";
				var interchangeValid = TestUtils.CreateInterchangeForXT(Factory);
				interchangeValid.EI_InterchangeNum = "100003";
				Factory.Save();
				var testLogger = new TestUtils.TestLogger();

				var (msgClientProvider, _, _) = TestUtils.GetMockedMsgClientProviderWithMessageInspection();
				var processor = new OutboundInterchangeProcessorTestWrapper(testLogger, new List<bool> { true })
				{
					ClientProvider = msgClientProvider
				};
				((IInterchangeProcessor)processor).Process(new Configuration(), new CancellationToken());
				var interchangeWithEmptyApplicationCodeReloaded = new BusinessObjectFactory().Load<EDIInterchange>(interchangeWithEmptyApplicationCode.PK);
				AssertEquals(interchangeWithEmptyApplicationCodeReloaded.EI_Status, EDIInterchange.Status.Error);
				var interchangeWithEmptyFromReloaded = new BusinessObjectFactory().Load<EDIInterchange>(interchangeWithEmptyFrom.PK);
				AssertEquals(interchangeWithEmptyFromReloaded.EI_Status, EDIInterchange.Status.Error);
				var interchangeValidReloaded = new BusinessObjectFactory().Load<EDIInterchange>(interchangeValid.PK);
				AssertEquals(interchangeValidReloaded.EI_Status, EDIInterchange.Status.Sent);
				var logToBeAssertion = testLogger.AllLogs.Select(l => l.Message).ToArray();
				AssertCollectionContains("Interchange: 100001 was set to ERR:EI_ApplicationCode should not be null.", logToBeAssertion);
				AssertCollectionContains("Interchange: 100002 was set to ERR:EI_From should not be null.", logToBeAssertion);
				AssertCollectionContains("Failed to save 2 interchange(s)(100001/100002).", logToBeAssertion);
				AssertCollectionContains("2 invalid interchange(s) would be skipped in this round and handled in next round.", logToBeAssertion);
				AssertCollectionContains("Interchange: 100003 was set to SNT.", logToBeAssertion);
				AssertCollectionContains("Send 1 interchange(s)(100003) successfully.", logToBeAssertion);

				interchangeWithEmptyApplicationCode.Reload();
				interchangeWithEmptyFrom.Reload();

				AssertEquals(EDIInterchangeStatusList.Codes.Error, interchangeWithEmptyApplicationCode.EI_Status);
				AssertEquals(EDIInterchangeStatusList.Codes.Error, interchangeWithEmptyFrom.EI_Status);
			}
		}

		public void TestSending_TransactionCommit()
		{
			using (DirectxTMessagingRegistry.Instance.XTIdleConnectionKeepAliveInSeconds.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
			using (DirectxTMessagingRegistry.Instance.XTIdleConnectionRetryPauseInSeconds.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
			{
				var interchange = TestUtils.CreateInterchangeForXT(Factory);
				interchange.EI_HeaderText = "RANDOM STRING";
				interchange.EI_SessionGUID = ZGuid.NewZGuid();
				Factory.Save();

				var actionOnStartTransactionAsyncWasCalled = false;
				var actionOnEndTransactionAsyncWasCalled = false;

				var (msgClientProvider, _, _) = TestUtils.GetMockedMsgClientProviderWithMessageInspection(
					ErrorCode.ErrOk,
					() => { actionOnStartTransactionAsyncWasCalled = true; },
					_ => { actionOnEndTransactionAsyncWasCalled = true; });
				var processor = new OutboundInterchangeProcessorTestWrapper(new TestUtils.TestLogger())
				{
					ClientProvider = msgClientProvider
				};
				((IInterchangeProcessor)processor).Process(new Configuration(), new CancellationToken());
				AssertEquals("actionOnStartTransactionAsyncWasCalled should be true", true, actionOnStartTransactionAsyncWasCalled);
				AssertEquals("actionOnEndTransactionAsyncWasCalled should be true", true, actionOnEndTransactionAsyncWasCalled);
			}
		}

		public void TestSending_BatchSuccess()
		{
			using (DirectxTMessagingRegistry.Instance.XTIdleConnectionKeepAliveInSeconds.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
			using (DirectxTMessagingRegistry.Instance.XTIdleConnectionRetryPauseInSeconds.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
			{
				var interchange1 = TestUtils.CreateInterchangeForXT(Factory, interchangeNum: "100001");
				interchange1.EI_SystemCreateTimeUtc = DateTime.UtcNow.AddSeconds(-5);
				TestUtils.CreateInterchangeForXT(Factory, interchangeNum: "100002");
				Factory.Save();

				var actionOnStartTransactionAsyncWasCalled = false;
				var actionOnEndTransactionAsyncWasCalled = false;

				var (msgClientProvider, _, _) = TestUtils.GetMockedMsgClientProviderWithMessageInspection(
					ErrorCode.ErrOk,
					() => { actionOnStartTransactionAsyncWasCalled = true; },
					_ => { actionOnEndTransactionAsyncWasCalled = true; });
				var testLogger = new TestUtils.TestLogger();
				var processor = new OutboundInterchangeProcessorTestWrapper(testLogger)
				{
					ClientProvider = msgClientProvider
				};
				((IInterchangeProcessor)processor).Process(new Configuration(), new CancellationToken());
				AssertEquals("actionOnStartTransactionAsyncWasCalled should be true", true, actionOnStartTransactionAsyncWasCalled);
				AssertEquals("actionOnEndTransactionAsyncWasCalled should be true", true, actionOnEndTransactionAsyncWasCalled);
				AssertCollectionContains("Send 2 interchange(s)(100001/100002) successfully.", testLogger.InfoLogs);
			}
		}

		public void TestSending_BatchSuccess_DifferentBranch()
		{
			using (DirectxTMessagingRegistry.Instance.XTIdleConnectionKeepAliveInSeconds.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
			using (DirectxTMessagingRegistry.Instance.XTIdleConnectionRetryPauseInSeconds.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
			{
				var branch1 = Factory.New<GlbBranch>();
				branch1.GB_Code = "TT1";
				branch1.GB_GC = GlbCompany.CurrentCompany.PK;

				var branch2 = Factory.New<GlbBranch>();
				branch2.GB_Code = "TT2";
				branch2.GB_GC = GlbCompany.CurrentCompany.PK;

				var interchange1 = TestUtils.CreateInterchangeForXT(Factory, interchangeNum: "100001");
				interchange1.EI_SystemCreateTimeUtc = DateTime.UtcNow.AddSeconds(-5);
				interchange1.EI_GB = branch1.PK;
				var interchange2 = TestUtils.CreateInterchangeForXT(Factory, interchangeNum: "100002");
				interchange2.EI_GB = branch2.PK;
				Factory.Save();

				var actionOnStartTransactionAsyncWasCalled = false;
				var actionOnEndTransactionAsyncWasCalled = false;

				var (msgClientProvider, _, _) = TestUtils.GetMockedMsgClientProviderWithMessageInspection(
					ErrorCode.ErrOk,
					() => { actionOnStartTransactionAsyncWasCalled = true; },
					_ => { actionOnEndTransactionAsyncWasCalled = true; });
				var testLogger = new TestUtils.TestLogger();
				var processor = new OutboundInterchangeProcessorTestWrapper(testLogger)
				{
					ClientProvider = msgClientProvider
				};
				((IInterchangeProcessor)processor).Process(new Configuration(), new CancellationToken());
				AssertEquals("actionOnStartTransactionAsyncWasCalled should be true", true, actionOnStartTransactionAsyncWasCalled);
				AssertEquals("actionOnEndTransactionAsyncWasCalled should be true", true, actionOnEndTransactionAsyncWasCalled);

				AssertCollectionContains("Send 1 interchange(s)(100001) successfully.", testLogger.InfoLogs);
				AssertCollectionContains("Send 1 interchange(s)(100002) successfully.", testLogger.InfoLogs);

				interchange1.Reload();
				interchange2.Reload();

				AssertEquals(interchange1.Logs.MostRecentLog.SL_GB_NKBranch, branch1.GB_Code);
				AssertEquals(interchange2.Logs.MostRecentLog.SL_GB_NKBranch, branch2.GB_Code);
			}
		}

		public void TestSending_BatchAndFirstIndividualFail()
		{
			using (DirectxTMessagingRegistry.Instance.XTIdleConnectionKeepAliveInSeconds.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
			using (DirectxTMessagingRegistry.Instance.XTIdleConnectionRetryPauseInSeconds.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
			{
				var interchange1 = TestUtils.CreateInterchangeForXT(Factory, interchangeNum: "100001");
				interchange1.EI_SystemCreateTimeUtc = DateTime.UtcNow.AddSeconds(-5);
				TestUtils.CreateInterchangeForXT(Factory, interchangeNum: "100002");
				Factory.Save();

				var actionOnStartTransactionAsyncWasCalled = false;
				var actionOnEndTransactionAsyncWasCalled = false;

				var (msgClientProvider, _, _) = TestUtils.GetMockedMsgClientProviderWithMessageInspection(
					ErrorCode.ErrOk,
					() => { actionOnStartTransactionAsyncWasCalled = true; },
					_ => { actionOnEndTransactionAsyncWasCalled = true; });
				var testLogger = new TestUtils.TestLogger();
				var processor = new OutboundInterchangeProcessorTestWrapper(testLogger, new List<bool> { true, true })
				{
					ClientProvider = msgClientProvider
				};
				((IInterchangeProcessor)processor).Process(new Configuration(), new CancellationToken());
				AssertEquals("actionOnStartTransactionAsyncWasCalled should be true", true, actionOnStartTransactionAsyncWasCalled);
				AssertEquals("actionOnEndTransactionAsyncWasCalled should be true", true, actionOnEndTransactionAsyncWasCalled);

				AssertCollectionContains("Failed to send 2 interchange(s): Failed to save 2 interchange(s)(100001/100002).", testLogger.WarningLogs);
				AssertCollectionContains("Failed to send 1 interchange(s): Failed to save 1 interchange(s)(100001).", testLogger.ErrorLogs);
				AssertCollectionContains("Individually processing done, 1 interchange(s) processed and 1 failed.", testLogger.InfoLogs);
				AssertCollectionContains("Failed interchange(s):100001, trying to increasing their EI_RetryCount(and EI_Status if necessary)...", testLogger.InfoLogs);
				AssertCollectionContains("EI_RetryCount of interchange 100001 increased.", testLogger.InfoLogs);

				AssertEquals("Failed to send 1 interchange(s): Failed to save 1 interchange(s)(100001).", ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
			}
		}

		public void TestSending_BatchAndSecondIndividualFail()
		{
			using (DirectxTMessagingRegistry.Instance.XTIdleConnectionKeepAliveInSeconds.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
			using (DirectxTMessagingRegistry.Instance.XTIdleConnectionRetryPauseInSeconds.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
			{
				var interchange1 = TestUtils.CreateInterchangeForXT(Factory, interchangeNum: "100001");
				interchange1.EI_SystemCreateTimeUtc = DateTime.UtcNow.AddSeconds(-5);
				TestUtils.CreateInterchangeForXT(Factory, interchangeNum: "100002");
				Factory.Save();

				var actionOnStartTransactionAsyncWasCalled = false;
				var actionOnEndTransactionAsyncWasCalled = false;

				var (msgClientProvider, _, _) = TestUtils.GetMockedMsgClientProviderWithMessageInspection(
					ErrorCode.ErrOk,
					() => { actionOnStartTransactionAsyncWasCalled = true; },
					_ => { actionOnEndTransactionAsyncWasCalled = true; });
				var testLogger = new TestUtils.TestLogger();
				var processor = new OutboundInterchangeProcessorTestWrapper(testLogger, new List<bool> { true, false, true })
				{
					ClientProvider = msgClientProvider
				};
				((IInterchangeProcessor)processor).Process(new Configuration(), new CancellationToken());
				AssertEquals("actionOnStartTransactionAsyncWasCalled should be true", true, actionOnStartTransactionAsyncWasCalled);
				AssertEquals("actionOnEndTransactionAsyncWasCalled should be true", true, actionOnEndTransactionAsyncWasCalled);

				AssertCollectionContains("Failed to send 2 interchange(s): Failed to save 2 interchange(s)(100001/100002).", testLogger.WarningLogs);
				AssertCollectionContains("Failed to send 1 interchange(s): Failed to save 1 interchange(s)(100002).", testLogger.ErrorLogs);
				AssertCollectionContains("Individually processing done, 1 interchange(s) processed and 1 failed.", testLogger.InfoLogs);
				AssertCollectionContains("Failed interchange(s):100002, trying to increasing their EI_RetryCount(and EI_Status if necessary)...", testLogger.InfoLogs);
				AssertCollectionContains("EI_RetryCount of interchange 100002 increased.", testLogger.InfoLogs);

				AssertEquals("Failed to send 1 interchange(s): Failed to save 1 interchange(s)(100002).", ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
			}
		}

		public void TestSending_BatchAndSecondIndividualFail_MeetMaxRetryCount()
		{
			using (DirectxTMessagingRegistry.Instance.XTIdleConnectionKeepAliveInSeconds.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
			using (DirectxTMessagingRegistry.Instance.XTIdleConnectionRetryPauseInSeconds.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
			using (DirectxTMessagingRegistry.Instance.XTSendingRetryLimit.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 2))
			{
				var interchange = TestUtils.CreateInterchangeForXT(Factory, interchangeNum: "100001");
				var ediMessage = Factory.NewWithValidTestData<EDIMessage>();
				ediMessage.EM_EI = interchange.PK;
				ediMessage.EM_MessageNum = "123";
				interchange.ContainedMessages.Add(ediMessage);
				Factory.Save();

				var (msgClientProvider, _, _) = TestUtils.GetMockedMsgClientProviderWithMessageInspection(ErrorCode.ErrOk, () => { }, _ => { }, throwExceptionOnStartTransaction: true);
				var testLogger = new TestUtils.TestLogger();
				var processor = new OutboundInterchangeProcessorTestWrapper(testLogger)
				{
					ClientProvider = msgClientProvider
				};

				((IInterchangeProcessor)processor).Process(new Configuration(), new CancellationToken());
				ediMessage.Reload();
				AssertEquals(EDIMessageStatusList.Codes.Queued, ediMessage.EM_Status);
				AssertEquals(0, ediMessage.Notes.VisibleNotes.Count);
				((IInterchangeProcessor)processor).Process(new Configuration(), new CancellationToken());
				ediMessage.Reload();
				AssertEquals(EDIMessageStatusList.Codes.Queued, ediMessage.EM_Status);
				AssertEquals(0, ediMessage.Notes.VisibleNotes.Count);
				((IInterchangeProcessor)processor).Process(new Configuration(), new CancellationToken());
				ediMessage.Reload();
				AssertEquals(EDIMessageStatusList.Codes.Error, ediMessage.EM_Status);
				AssertEquals(1, ediMessage.Notes.VisibleNotes.Count);
				AssertEquals("Message Delivery Failure", ediMessage.Notes.VisibleNotes[0].ST_Description);
				AssertEquals("Message: 123 was set to ERR as per: Retry count of interchange:100001 meet the maximum value.", ediMessage.Notes.VisibleNotes[0].ST_NoteText);

				ErrorReporter.Clear();
			}
		}

		public void TestSending_BatchAndSecondIndividualFailThenIncreasingRetryCountFail()
		{
			using (DirectxTMessagingRegistry.Instance.XTIdleConnectionKeepAliveInSeconds.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
			using (DirectxTMessagingRegistry.Instance.XTIdleConnectionRetryPauseInSeconds.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
			{
				var interchange1 = TestUtils.CreateInterchangeForXT(Factory, interchangeNum: "100001");
				interchange1.EI_SystemCreateTimeUtc = DateTime.UtcNow.AddSeconds(-5);
				TestUtils.CreateInterchangeForXT(Factory, interchangeNum: "100002");
				Factory.Save();

				var actionOnStartTransactionAsyncWasCalled = false;
				var actionOnEndTransactionAsyncWasCalled = false;

				var (msgClientProvider, _, _) = TestUtils.GetMockedMsgClientProviderWithMessageInspection(
					ErrorCode.ErrOk,
					() => { actionOnStartTransactionAsyncWasCalled = true; },
					_ => { actionOnEndTransactionAsyncWasCalled = true; });
				var testLogger = new TestUtils.TestLogger();
				var processor = new OutboundInterchangeProcessorTestWrapper(testLogger, new List<bool> { true, false, true, true })
				{
					ClientProvider = msgClientProvider
				};
				((IInterchangeProcessor)processor).Process(new Configuration(), new CancellationToken());
				AssertEquals("actionOnStartTransactionAsyncWasCalled should be true", true, actionOnStartTransactionAsyncWasCalled);
				AssertEquals("actionOnEndTransactionAsyncWasCalled should be true", true, actionOnEndTransactionAsyncWasCalled);

				AssertCollectionContains("Failed to send 2 interchange(s): Failed to save 2 interchange(s)(100001/100002).", testLogger.WarningLogs);
				AssertCollectionContains("Failed to send 1 interchange(s): Failed to save 1 interchange(s)(100002).", testLogger.ErrorLogs);
				AssertCollectionContains("Individually processing done, 1 interchange(s) processed and 1 failed.", testLogger.InfoLogs);
				AssertCollectionContains("Failed interchange(s):100002, trying to increasing their EI_RetryCount(and EI_Status if necessary)...", testLogger.InfoLogs);
				AssertCollectionContains("Failed to increase EI_RetryCount of interchange 100002, please pay attention on it.", testLogger.ErrorLogs);

				AssertEquals("Failed to send 1 interchange(s): Failed to save 1 interchange(s)(100002).", ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
			}
		}

		public void TestInitializationFailure()
		{
			var testInterchange = TestUtils.CreateInterchangeForXT(Factory);
			Factory.Save();

			var logger = new TestUtils.TestLogger();
			var processor = new OutboundInterchangeProcessor(logger);
			var config = new Configuration
			{
				Connect = "localhost",
				CA = "C",
				Application = new Application { URI = "U", Password = "P" }
			};

			var exception = AssertExceptionThrown<MsgServerConnectionException>(() => ((IInterchangeProcessor)processor).Process(config, new CancellationToken()));

			AssertEquals("Exception Message", Shared.Utils.RpcErrorReportKey, exception.Message);

			testInterchange.Reload();
			AssertEquals(EDIInterchange.Status.Queued, testInterchange.EI_Status);
		}

		public void TestProcess_EmptyProperties()
		{
			using (DirectxTMessagingRegistry.Instance.XTIdleConnectionKeepAliveInSeconds.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
			using (DirectxTMessagingRegistry.Instance.XTIdleConnectionRetryPauseInSeconds.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
			using (DirectxTMessagingRegistry.Instance.EnableXTIServiceTask.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var (interchangesShouldBeSent, interchangesShouldNotBeSent) = SetupIncompleteInterchangeForTesting(Factory);
				Factory.Save();
				var testLogger = new TestUtils.TestLogger();
				var processor = new OutboundInterchangeProcessorTestWrapper(testLogger);
				processor.ClientProvider = TestUtils.GetMoqMsgClientProvider();
				((IInterchangeProcessor)processor).Process(new Configuration(), new CancellationToken());
				AssertEndToEndResult(interchangesShouldBeSent, interchangesShouldNotBeSent);
				Assert("only 1 interchange should be send", testLogger.AllLogs.Select(l => l.Message).Contains("Validating interchange(s) done, 1 interchange(s) will be sent."));
				Assert("interchange 2 should fail log", testLogger.AllLogs.Select(l => l.Message).Contains("Interchange: 2 was set to ERR:EI_ApplicationCode should not be null."));
				Assert("interchange 3 should fail log", testLogger.AllLogs.Select(l => l.Message).Contains("Interchange: 3 was set to ERR:EI_From should not be null."));
			}
		}

		public void TestProcess()
		{
			using (DirectxTMessagingRegistry.Instance.XTIdleConnectionKeepAliveInSeconds.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
			using (DirectxTMessagingRegistry.Instance.XTIdleConnectionRetryPauseInSeconds.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
			using (DirectxTMessagingRegistry.Instance.EnableXTIServiceTask.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				Assert("XTI Service task should be disabled", !DirectxTMessagingRegistry.Instance.EnableXTIServiceTask.Value);

				var (interchangesShouldBeUpdated, interchangesShouldNotBeUpdated) = SetupDataForTesting(Factory);
				Factory.Save();
				var processor = new OutboundInterchangeProcessorTestWrapper(new TestUtils.TestLogger());
				processor.ClientProvider = TestUtils.GetMoqMsgClientProvider();
				using (DirectxTMessagingRegistry.Instance.XTIdleConnectionKeepAliveInSeconds.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
				{
					((IInterchangeProcessor)processor).Process(new Configuration(), new CancellationToken());
				}
				AssertEndToEndResult(interchangesShouldBeUpdated, interchangesShouldNotBeUpdated);
				Assert("XTI Service task should be enabled after interchanges have been sent", DirectxTMessagingRegistry.Instance.EnableXTIServiceTask.Value);
			}
		}

		public void TestProcess_MessageStatus()
		{
			using (DirectxTMessagingRegistry.Instance.XTIdleConnectionKeepAliveInSeconds.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
			using (DirectxTMessagingRegistry.Instance.XTIdleConnectionRetryPauseInSeconds.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
			using (DirectxTMessagingRegistry.Instance.EnableXTIServiceTask.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var interchange = TestUtils.CreateInterchangeForXT(Factory, applicationCode: "");
				var ediMessage = Factory.NewWithValidTestData<EDIMessage>();
				ediMessage.EM_EI = interchange.PK;
				ediMessage.EM_MessageNum = "123";
				interchange.ContainedMessages.Add(ediMessage);
				Factory.Save();
				var processor = new OutboundInterchangeProcessorTestWrapper(new TestUtils.TestLogger());
				processor.ClientProvider = TestUtils.GetMoqMsgClientProvider();
				using (DirectxTMessagingRegistry.Instance.XTIdleConnectionKeepAliveInSeconds.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
				{
					((IInterchangeProcessor)processor).Process(new Configuration(), new CancellationToken());
				}
				AssertEquals(EDIMessageStatusList.Codes.Error, ediMessage.EM_Status);
				AssertEquals(1, ediMessage.Notes.VisibleNotes.Count);
				AssertEquals("Message Delivery Failure", ediMessage.Notes.VisibleNotes[0].ST_Description);
				AssertEquals("Message: 123 was set to ERR as per: Failed to send interchange: 1.", ediMessage.Notes.VisibleNotes[0].ST_NoteText);
			}
		}

		public void TestNoExceptionsWhenProcessingLargeMessagesAsynchronously()
		{
			var (interchangesShouldBeUpdated, interchangesShouldNotBeUpdated) = SetupDataForTesting(Factory, useBigBodies: true);
			Factory.Save();
			var processor = new OutboundInterchangeProcessorTestWrapper(new TestUtils.TestLogger());
			processor.ClientProvider = TestUtils.GetMoqMsgClientProvider();
			using (DirectxTMessagingRegistry.Instance.XTIdleConnectionKeepAliveInSeconds.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
			{
				AssertNoExceptionThrown(() => ((IInterchangeProcessor)processor).Process(new Configuration(), new CancellationToken()));
			}
			AssertEndToEndResult(interchangesShouldBeUpdated, interchangesShouldNotBeUpdated);
		}

		public void TestProcessorThrowsSessionTimeoutExceptionWhenEndTransactionThrowUnauthenticatedRpcException()
		{
			using (DirectxTMessagingRegistry.Instance.XTIdleConnectionKeepAliveInSeconds.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
			using (DirectxTMessagingRegistry.Instance.XTIdleConnectionRetryPauseInSeconds.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
			{
				var interchange = TestUtils.CreateInterchangeForXT(Factory);
				interchange.EI_HeaderText = "RANDOM STRING";
				interchange.EI_SessionGUID = ZGuid.NewZGuid();
				Factory.Save();

				var status = new Status(Grpc.Core.StatusCode.Unauthenticated, "Invalid Token");
				var rpcException = new RpcException(status);
				var msgClientProvider = TestUtils.GetMoqMsgClientProviderWithRunActionWrappersForExceptionHandlingTesting(rpcException, TestUtils.MsgClientMethod.EndTransaction);
				var testLogger = new TestUtils.TestLogger();
				var processor = new OutboundInterchangeProcessorTestWrapper(testLogger)
				{
					ClientProvider = msgClientProvider,
				};

				var exception = AssertExceptionThrown<MsgSessionTimeoutException>(() => ((IInterchangeProcessor)processor).Process(new Configuration(), new CancellationToken()));

				AssertEquals("Exception Message", Shared.Utils.SessionTimeoutErrorReportKey, exception.Message);

				interchange.Reload();
				AssertEquals(EDIInterchange.Status.Sent, interchange.EI_Status);
			}
		}

		public void TestProcessorThrowsSessionTimeoutExceptionWhenSubmitMsgThrowsUnauthenticatedRpcException()
		{
			using (DirectxTMessagingRegistry.Instance.XTIdleConnectionKeepAliveInSeconds.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
			using (DirectxTMessagingRegistry.Instance.XTIdleConnectionRetryPauseInSeconds.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
			{
				var interchange = TestUtils.CreateInterchangeForXT(Factory);
				interchange.EI_HeaderText = "RANDOM STRING";
				interchange.EI_SessionGUID = ZGuid.NewZGuid();
				Factory.Save();

				var status = new Status(Grpc.Core.StatusCode.Unauthenticated, "Invalid Token");
				var rpcException = new RpcException(status);
				var msgClientProvider = TestUtils.GetMoqMsgClientProviderWithRunActionWrappersForExceptionHandlingTesting(rpcException, TestUtils.MsgClientMethod.SubmitMsg);
				var testLogger = new TestUtils.TestLogger();
				var processor = new OutboundInterchangeProcessorTestWrapper(testLogger)
				{
					ClientProvider = msgClientProvider,
				};

				var exception = AssertExceptionThrown<MsgServerConnectionException>(() => ((IInterchangeProcessor)processor).Process(new Configuration(), new CancellationToken()));

				AssertEquals("Exception Message", Shared.Utils.SessionTimeoutErrorReportKey, exception.Message);

				interchange.Reload();
				AssertEquals(EDIInterchange.Status.Queued, interchange.EI_Status);
			}
		}

		public void TestProcessorHandlesExceptionWhenStartTransactionThrowUnauthenticatedRpcException()
		{
			using (DirectxTMessagingRegistry.Instance.XTIdleConnectionKeepAliveInSeconds.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
			using (DirectxTMessagingRegistry.Instance.XTIdleConnectionRetryPauseInSeconds.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
			{
				var interchange = TestUtils.CreateInterchangeForXT(Factory);
				interchange.EI_HeaderText = "RANDOM STRING";
				interchange.EI_SessionGUID = ZGuid.NewZGuid();
				Factory.Save();

				var status = new Status(Grpc.Core.StatusCode.Unauthenticated, "Invalid Token");
				var rpcException = new RpcException(status);
				var msgClientProvider = TestUtils.GetMoqMsgClientProviderWithRunActionWrappersForExceptionHandlingTesting(rpcException, TestUtils.MsgClientMethod.StartTransaction);
				var testLogger = new TestUtils.TestLogger();
				var processor = new OutboundInterchangeProcessorTestWrapper(testLogger)
				{
					ClientProvider = msgClientProvider,
				};

				var exception = AssertExceptionThrown<MsgSessionTimeoutException>(() => ((IInterchangeProcessor)processor).Process(new Configuration(), new CancellationToken()));

				AssertEquals("Exception Message", Shared.Utils.SessionTimeoutErrorReportKey, exception.Message);

				interchange.Reload();
				AssertEquals(EDIInterchange.Status.Queued, interchange.EI_Status);
			}
		}

		public void TestProcessorHandlesExceptionWhenEndTransactionThrowXtTransactionException()
		{
			using (DirectxTMessagingRegistry.Instance.XTIdleConnectionKeepAliveInSeconds.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
			using (DirectxTMessagingRegistry.Instance.XTIdleConnectionRetryPauseInSeconds.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
			{
				var interchange = TestUtils.CreateInterchangeForXT(Factory);
				interchange.EI_HeaderText = "RANDOM STRING";
				interchange.EI_SessionGUID = ZGuid.NewZGuid();
				Factory.Save();

				var status = new Status(Grpc.Core.StatusCode.Cancelled, "Cancelled");
				var rpcException = new RpcException(status);
				var msgClientProvider = TestUtils.GetMoqMsgClientProviderWithRunActionWrappersForExceptionHandlingTesting(rpcException, TestUtils.MsgClientMethod.EndTransaction);
				var testLogger = new TestUtils.TestLogger();
				var processor = new OutboundInterchangeProcessorTestWrapper(testLogger)
				{
					ClientProvider = msgClientProvider,
				};

				((IInterchangeProcessor)processor).Process(new Configuration(), new CancellationToken());

				AssertEquals(processor.BatchCount, 2);
				var error = "Failed to send 1 interchange(s): EndTransaction Failed(IsCancellationRequested=False) - Direct xT Client - Rpc Error - Status(StatusCode=\"Cancelled\", Detail=\"Cancelled\"). EndTransaction Failed(IsCancellationRequested=False) - Direct xT Client - Rpc Error - Status(StatusCode=\"Cancelled\", Detail=\"Cancelled\")";
				AssertContainsExactElementsInExactOrder(new[] { error }, testLogger.ErrorLogs);
				AssertEquals(error, ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
			}
		}

		public void TestInterchangeCountToSendPerBatch()
		{
			using (DirectxTMessagingRegistry.Instance.InterchangeCountPerBatchOnSending.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 2))
			{
				var (interchangesShouldBeUpdated, interchangesShouldNotBeUpdated) = SetupDataForTesting(Factory);
				Factory.Save();
				var processor = new OutboundInterchangeProcessorTestWrapper(new TestUtils.TestLogger());
				processor.ClientProvider = TestUtils.GetMoqMsgClientProvider();

				using (DirectxTMessagingRegistry.Instance.XTIdleConnectionKeepAliveInSeconds.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
				{
					((IInterchangeProcessor)processor).Process(new Configuration(), new CancellationToken());
				}

				AssertEquals(processor.BatchCount, 2);
			}

			using (DirectxTMessagingRegistry.Instance.InterchangeCountPerBatchOnSending.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 1))
			{
				var (interchangesShouldBeUpdated, interchangesShouldNotBeUpdated) = SetupDataForTesting(Factory);
				Factory.Save();
				var processor = new OutboundInterchangeProcessorTestWrapper(new TestUtils.TestLogger());
				processor.ClientProvider = TestUtils.GetMoqMsgClientProvider();

				using (DirectxTMessagingRegistry.Instance.XTIdleConnectionKeepAliveInSeconds.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
				{
					((IInterchangeProcessor)processor).Process(new Configuration(), new CancellationToken());
				}

				AssertEquals(processor.BatchCount, 4);
			}
		}

		[TestDate(2023, 10, 16, 17, 6, 0)]
		public void TestInterchangeProcessOrder()
		{
			var newBranch1 = Factory.New<GlbBranch>();
			newBranch1.GB_Code = "AVB";
			newBranch1.GB_GC = GlbBranch.CurrentBranch.GB_GC;
			var newBranch2 = Factory.New<GlbBranch>();
			newBranch2.GB_Code = "AVC";
			newBranch2.GB_GC = GlbBranch.CurrentBranch.GB_GC;
			var now = TestDateAttribute.Date;

			var interchange1 = TestUtils.CreateInterchangeForXT(Factory, interchangeNum: "100001");
			interchange1.EI_GB = newBranch1.PK;
			TestDateAttribute.Date = now.AddDays(4);
			Factory.Save();
			var interchange2 = TestUtils.CreateInterchangeForXT(Factory, interchangeNum: "100002");
			interchange2.EI_GB = newBranch2.PK;
			TestDateAttribute.Date = now.AddDays(1);
			Factory.Save();
			var interchange3 = TestUtils.CreateInterchangeForXT(Factory, interchangeNum: "100003");
			interchange3.EI_GB = newBranch2.PK;
			TestDateAttribute.Date = now.AddDays(-10);
			Factory.Save();
			var interchange4 = TestUtils.CreateInterchangeForXT(Factory, interchangeNum: "100014");
			interchange4.EI_GB = newBranch1.PK;
			TestDateAttribute.Date = now.AddDays(-1);
			Factory.Save();
			var interchange5 = TestUtils.CreateInterchangeForXT(Factory, interchangeNum: "100005");
			interchange5.EI_GB = newBranch1.PK;
			var interchange6 = TestUtils.CreateInterchangeForXT(Factory, interchangeNum: "100000");
			interchange6.EI_GB = newBranch1.PK;
			TestDateAttribute.Date = now;
			Factory.Save();

			Factory.Save();

			var logger = new TestUtils.TestLogger();
			var processor = new OutboundInterchangeProcessorTestWrapper(logger) { ClientProvider = TestUtils.GetMoqMsgClientProvider() };

			using (DirectxTMessagingRegistry.Instance.XTIdleConnectionKeepAliveInSeconds.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
			{
				((IInterchangeProcessor)processor).Process(new Configuration(), new CancellationToken());
			}

			var logMessages = logger.AllLogs.Select(l => l.Message).ToList();
			var m1ProcessIndex = logMessages.IndexOf(l => l == "Interchange: 100001 was set to SNT.");
			var m2ProcessIndex = logMessages.IndexOf(l => l == "Interchange: 100002 was set to SNT.");
			var m3ProcessIndex = logMessages.IndexOf(l => l == "Interchange: 100003 was set to SNT.");
			var m4ProcessIndex = logMessages.IndexOf(l => l == "Interchange: 100014 was set to SNT.");
			var m5ProcessIndex = logMessages.IndexOf(l => l == "Interchange: 100005 was set to SNT.");
			var m6ProcessIndex = logMessages.IndexOf(l => l == "Interchange: 100000 was set to SNT.");

			AssertLessThan("should process in correct order EI_SystemCreateTimeUtc", m3ProcessIndex, m2ProcessIndex);
			AssertLessThan("should process in correct order EI_SystemCreateTimeUtc", m2ProcessIndex, m4ProcessIndex);
			AssertLessThan("should process in correct order EI_SystemCreateTimeUtc", m4ProcessIndex, m6ProcessIndex);
			AssertLessThan("should process in correct order EI_SystemCreateTimeUtc", m6ProcessIndex, m5ProcessIndex);
			AssertLessThan("should process in correct order EI_SystemCreateTimeUtc", m5ProcessIndex, m1ProcessIndex);
		}

		[ExpectNoExceptions]
		public void TestProcess_NotProcessAndNoExceptionWhenNoEDIInterchange()
		{
			using (DirectxTMessagingRegistry.Instance.EnableXTIServiceTask.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var query = new ZQuery(EDIInterchangeSchema.EI_TransportType, EDIInterchange.TransportType.xT);
				var interchange = Factory.LoadTop1<EDIInterchange>(query);
				AssertNull("Precondition: No XT interchange", interchange);

				var logger = new TestUtils.TestLogger();
				var processor = new OutboundInterchangeProcessorTestWrapper(logger);
				processor.ClientProvider = TestUtils.GetMoqMsgClientProvider();
				((IInterchangeProcessor)processor).Process(new Configuration(), new CancellationToken());

				AssertArrayEqualsByElements("No error log: Not process, or there should be connector Initialize error due to null msgClientProvider", new[] { "Not process due to no interchange to be sent." }, logger.AllLogs.Select(x => x.Message).ToArray());
				Assert("XTI Service task should still be disabled as there are no interchanges to send", !DirectxTMessagingRegistry.Instance.EnableXTIServiceTask.Value);
			}
		}

		public void TestStatusUpdate()
		{
			using (DirectxTMessagingRegistry.Instance.XTIdleConnectionKeepAliveInSeconds.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
			using (DirectxTMessagingRegistry.Instance.XTIdleConnectionRetryPauseInSeconds.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
			{
				var interchanges = new[] {
					TestUtils.CreateInterchangeForXT(Factory, applicationCode: "TTT"),
					TestUtils.CreateInterchangeForXT(Factory),
					TestUtils.CreateInterchangeForXT(Factory),
					};
				Factory.Save();

				var logger = new TestUtils.TestLogger();
				var processor = new OutboundInterchangeProcessorTestWrapper(logger);

				processor.ClientProvider = TestUtils.GetMoqMsgClientProvider(new[] {
				new SubmitMsgReply() { Errorcode = (int)ErrorCode.ErrOk },
				new SubmitMsgReply() { Errorcode = (int)ErrorCode.ErrConfig },
				new SubmitMsgReply() { Errorcode = (int)ErrorCode.ErrPlock },
			});

				using (DirectxTMessagingRegistry.Instance.XTIdleConnectionKeepAliveInSeconds.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 5))
				using (DirectxTMessagingRegistry.Instance.XTIdleConnectionRetryPauseInSeconds.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 3))
				{
					((IInterchangeProcessor)processor).Process(new Configuration(), new CancellationToken());
				}

				Factory.ReloadAll<EDIInterchange>();
				AssertArrayEqualsByElements("EDIInterchange Status", new ZString[] {
					EDIMessage.Status.Sent,
					EDIMessage.Status.Error,
					EDIMessage.Status.Error
				}, interchanges.Select(ei => ei.EI_Status).ToArray());

				AssertArrayEqualsByElements("EDIInterchange Notes", new ZString[] {
					"Interchange: 1 was set to SNT.",
					"Interchange: 2 was set to ERR:Rejected by xT Server. Error 'ErrConfig - Configuration Error (28)' returned.",
					"Interchange: 3 was set to ERR:Rejected by xT Server. Error 'ErrPlock - The processing lock is set (14)' returned."
				}, interchanges.Select(ei => ei.Notes.GetAllNotesVisibleToCurrentCompany().FirstOrDefault()?.ST_NoteText ?? ZString.Empty).ToArray());

				var debugLogs = string.Join("|", logger.AllLogs.Select(l => l.Message));
				var debugLogsAsArray = logger.AllLogs.Select(l => l.Message.Trim()).ToArray();
				var infoLogs = string.Join("|", logger.InfoLogs);
				CombineAssertions(() =>
				{
					AssertContains("Start Initialize DirectxT Connector.", debugLogs);
					AssertContains("Finish Initialize DirectxT Connector. Total Time: ", debugLogs);
					AssertContains("Start validating 3 interchange(s).", debugLogs);
					AssertContains("Asynchronously sending 1 interchange(s) grouped by Application Code: TTT", debugLogs);
					AssertContains("Asynchronously sending 2 interchange(s) grouped by Application Code: TST", debugLogs);
					AssertContains("Interchange: 1 was set to SNT", debugLogs);
					AssertContains("Interchange: 2 was set to ERR:Rejected by xT Server. Error 'ErrConfig - Configuration Error (28)' returned.", debugLogs);
					AssertContains("Interchange: 3 was set to ERR:Rejected by xT Server. Error 'ErrPlock - The processing lock is set (14)' returned.", debugLogs);
					AssertContains("Initialization completed - connect to GRPC client", infoLogs);
					AssertContains("Finish handling 3 interchange(s). Total Time:", infoLogs);
					AssertContains("The connection is idle but the keep alive has not yet expired. Pausing for 3 second(s) before we retry.", debugLogs);
					AssertContains("Logged Off the GRPC client", infoLogs);
				});

				AssertEquals("The connection did not pause for retry the right amount of times", debugLogsAsArray.Count(a => a.Contains("The connection is idle but the keep alive has not yet expired. Pausing for 3 second(s) before we retry.")), 2);
			}
		}

		public static (List<EDIInterchange> interchangesShouldBeUpdated, List<EDIInterchange> interchangesShouldNotBeUpdated) SetupIncompleteInterchangeForTesting(BusinessObjectFactory factory)
		{
			var interchangesShouldBeSent = new List<EDIInterchange>
			{
				TestUtils.CreateInterchangeForXT(factory, interchangeNum: "1")
			};
			var interchangesShouldNotBeSent = new List<EDIInterchange>
			{
				TestUtils.CreateInterchangeForXT(factory, applicationCode: string.Empty, interchangeNum: "2"),
				TestUtils.CreateInterchangeForXT(factory, from: string.Empty, interchangeNum: "3"),
			};
			return (interchangesShouldBeSent, interchangesShouldNotBeSent);
		}

		public void TestInterchangexTInternalMsgIDShouldOnlyBeUpdatedWhenSuccessfullySent()
		{
			var interchanges = new[] {
				TestUtils.CreateInterchangeForXT(Factory, EDIMessage.Status.Queued, true, EDIMessage.Direction.Transmit, EDIInterchange.TransportType.xT),
				TestUtils.CreateInterchangeForXT(Factory, EDIMessage.Status.Queued, true, EDIMessage.Direction.Transmit, EDIInterchange.TransportType.xT),
				TestUtils.CreateInterchangeForXT(Factory, EDIMessage.Status.Queued, true, EDIMessage.Direction.Transmit, EDIInterchange.TransportType.xT)
				};
			Factory.Save();

			var logger = new TestUtils.TestLogger();
			var processor = new OutboundInterchangeProcessorTestWrapper(logger);

			processor.ClientProvider = TestUtils.GetMoqMsgClientProvider(new[] {
				new SubmitMsgReply() { Errorcode = (int)ErrorCode.ErrOk },
				new SubmitMsgReply() { Errorcode = (int)ErrorCode.ErrConfig },
				new SubmitMsgReply() { Errorcode = (int)ErrorCode.ErrPlock },
			});

			using (DirectxTMessagingRegistry.Instance.XTIdleConnectionKeepAliveInSeconds.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 5))
			using (DirectxTMessagingRegistry.Instance.XTIdleConnectionRetryPauseInSeconds.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 3))
			{
				((IInterchangeProcessor)processor).Process(new Configuration(), new CancellationToken());
			}

			AssertArrayEqualsByElements("EDIInterchange xT Msg Id status should only be set for sent interchanges:", new ZLong[]
			{
				123L,
				0L,
				0L,
			}, interchanges.Select(ei => ei.EI_XTInternalMsgID).ToArray());
		}

		public void TestOutgoingInterchangeQueryFilterIncludesTableFetchHint()
		{
			var logger = new TestUtils.TestLogger();
			var processor = new OutboundInterchangeProcessorExposed(logger);

			var query = processor.GetFilterQueryExposed(15);
			AssertEquals("ZQuery for outgoing interchanges should force use this index",
						 EDIInterchangeSchema.Constants.Indexes.NR_RX__EI_ReceiveTransmit_EI_Status_EI_TransportType_EI_ApplicationCode_EI_From_EI_SystemCreateTimeUtc_XTTRCVTRX,
						 query.TableIndexHints.FirstOrDefault().IndexName);
		}

		public static (List<EDIInterchange> interchangesShouldBeUpdated, List<EDIInterchange> interchangesShouldNotBeUpdated) SetupDataForTesting(BusinessObjectFactory factory, bool useBigBodies = false)
		{
			List<EDIInterchange> interchangesShouldBeUpdated;

			if (useBigBodies)
			{
				interchangesShouldBeUpdated = new List<EDIInterchange>
				{
					TestUtils.CreateInterchangeWithLargeBody(factory),
					TestUtils.CreateInterchangeWithLargeBody(factory),
					TestUtils.CreateInterchangeWithLargeBody(factory),
					TestUtils.CreateInterchangeWithLargeBody(factory)
				};
			}
			else
			{
				interchangesShouldBeUpdated = new List<EDIInterchange>
				{
					TestUtils.CreateInterchangeForXT(factory),
					TestUtils.CreateInterchangeForXT(factory),
					TestUtils.CreateInterchangeForXT(factory),
					TestUtils.CreateInterchangeForXT(factory)
				};
			}

			var interchangesShouldNotBeUpdated = new List<EDIInterchange>
			{
				TestUtils.CreateInterchangeForXT(factory, status: EDIMessage.Status.Rejected),
				TestUtils.CreateInterchangeForXT(factory, isActive: false),
				TestUtils.CreateInterchangeForXT(factory, receiveTransmit: EDIMessage.Direction.Receive),
				TestUtils.CreateInterchangeForXT(factory, transportType: EDIInterchange.TransportType.eHub)
			};
			return (interchangesShouldBeUpdated, interchangesShouldNotBeUpdated);
		}

		internal static void AssertEndToEndResult(List<EDIInterchange> interchangesShouldBeUpdated, List<EDIInterchange> interchangesShouldNotBeUpdated)
		{
			CombineAssertions(() =>
			{
				foreach (var interchange in interchangesShouldBeUpdated)
				{
					interchange.Reload();
					AssertEquals($"{interchange.EI_IsActive}, {interchange.EI_ReceiveTransmit}, {interchange.EI_TransportType}", EDIMessage.Status.Sent, interchange.EI_Status);
					AssertEquals($"Interchange xT Msg Id should be set:", 12345L, interchange.EI_XTInternalMsgID);
				}
				foreach (var interchange in interchangesShouldNotBeUpdated)
				{
					interchange.Reload();
					AssertNotEquals($"{interchange.EI_IsActive}, {interchange.EI_ReceiveTransmit}, {interchange.EI_TransportType}", EDIMessage.Status.Sent, interchange.EI_Status);
					AssertEquals($"Interchange xT Msg Id should not be set:", 0L, interchange.EI_XTInternalMsgID);
				}
			});
		}

		class OutboundInterchangeProcessorExposed : OutboundInterchangeProcessor
		{
			public OutboundInterchangeProcessorExposed(ILogger logger) : base(logger) { }
			public ZQuery GetFilterQueryExposed(int maxRows) => GetFilterQuery(maxRows);
		}
	}
}
