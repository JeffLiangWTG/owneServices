using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.ServiceTasks.TestDirectXt;
using Enterprise.Messaging.Business;
using Enterprise.xTMessaging.Business;
using Enterprise.xTMessaging.ServiceTasks;
using Enterprise.xTMessaging.Shared;
using Enterprise.xTMessaging.Shared.Test;
using Xware.Xt.Grpc.Application;
using Xware.Xt.Grpc.Config;

namespace ZClientEDI.Test.ServiceTasks.TestDirectXt
{
	class TestDirectXtOutboundProcessorTests : TestCaseWithFactory
	{
		public void TestInitializationFailure()
		{
			using (DirectxTMessagingRegistry.Instance.XTIdleConnectionKeepAliveInSeconds.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
			using (DirectxTMessagingRegistry.Instance.XTIdleConnectionRetryPauseInSeconds.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
			{
				var testinterchange = TestDirectXtTestUtils.CreateInterchange(Factory, EDIMessage.Status.Queued, true, EDIMessage.Direction.Transmit, EDIInterchange.TransportType.tXT);
				Factory.Save();

				var logger = new TestUtils.TestLogger();
				var processor = new TestDirectXtOutboundProcessor(logger);
				var config = new Configuration
				{
					Connect = "localhost",
					CA = "C",
					Application = new Application { URI = "U", Password = "P" }
				};

				var exception = AssertExceptionThrown<MsgServerConnectionException>(() => ((IInterchangeProcessor)processor).Process(config, new CancellationToken()));

				AssertEquals("Exception Message", Enterprise.xTMessaging.Shared.Utils.RpcErrorReportKey, exception.Message);

				testinterchange.Reload();
				AssertEquals(EDIInterchange.Status.Queued, testinterchange.EI_Status);
				ErrorReporter.Clear();
			}
		}

		public void TestProcess_SendMessageToTest()
		{
			using (DirectxTMessagingRegistry.Instance.XTIdleConnectionKeepAliveInSeconds.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
			using (DirectxTMessagingRegistry.Instance.XTIdleConnectionRetryPauseInSeconds.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
			{
				var (interchangesShouldBeUpdated, interchangesShouldNotBeUpdated) = SetupDataForTesting(Factory);
				Factory.Save();
				var processor = new TestDirectXtOutboundProcessorTestWrapper(new TestUtils.TestLogger());
				processor.ClientProvider = TestDirectXtTestUtils.GetMockedMsgClientProviderOutbound();
				((IInterchangeProcessor)processor).Process(new Configuration(), new CancellationToken());

				AssertEndToEndResult(interchangesShouldBeUpdated, interchangesShouldNotBeUpdated);
			}
		}

		public void TestInterchangeStatusUpdate()
		{
			using (DirectxTMessagingRegistry.Instance.XTIdleConnectionKeepAliveInSeconds.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
			using (DirectxTMessagingRegistry.Instance.XTIdleConnectionRetryPauseInSeconds.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
			{
				var interchanges = new[]
				{
					TestDirectXtTestUtils.CreateInterchange(Factory, EDIMessage.Status.Queued, true, EDIMessage.Direction.Transmit, EDIInterchange.TransportType.tXT),
					TestDirectXtTestUtils.CreateInterchange(Factory, EDIMessage.Status.Queued, true, EDIMessage.Direction.Transmit, EDIInterchange.TransportType.tXT),
					TestDirectXtTestUtils.CreateInterchange(Factory, EDIMessage.Status.Queued, true, EDIMessage.Direction.Transmit, EDIInterchange.TransportType.tXT)
				};
				Factory.Save();

				var logger = new TestUtils.TestLogger();
				var processor = new TestDirectXtOutboundProcessorTestWrapper(logger);
				processor.ClientProvider = TestDirectXtTestUtils.GetMockedMsgClientProviderOutbound(new[]
				{
					new SubmitMsgReply() { Errorcode = (int)ErrorCode.ErrOk },
					new SubmitMsgReply() { Errorcode = (int)ErrorCode.ErrConfig },
					new SubmitMsgReply() { Errorcode = (int)ErrorCode.ErrPlock },
				});
				((IInterchangeProcessor)processor).Process(new Configuration(), new CancellationToken());

				Factory.ReloadAll<EDIInterchange>();
				AssertArrayEqualsByElements("EDIInterchange Status", new ZString[]
				{
					EDIMessage.Status.Sent,
					EDIMessage.Status.Error,
					EDIMessage.Status.Error
				}, interchanges.Select(ei => ei.EI_Status).ToArray());

				AssertArrayEqualsByElements("EDIInterchange xT Msg Id status should only be set for sent interchanges:", new ZLong[]
	{
				123L,
				0L,
				0L,
				}, interchanges.Select(ei => ei.EI_XTInternalMsgID).ToArray());

				AssertArrayEqualsByElements("EDIInterchange Notes", new ZString[] {
				"Interchange: 1 was set to SNT.",
				"Interchange: 2 was set to ERR:Rejected by xT Server. Error 'ErrConfig - Configuration Error (28)' returned.",
				"Interchange: 3 was set to ERR:Rejected by xT Server. Error 'ErrPlock - The processing lock is set (14)' returned."
			}, interchanges.Select(ei => ei.Notes.GetAllNotesVisibleToCurrentCompany().FirstOrDefault()?.ST_NoteText ?? ZString.Empty).ToArray());

				var logs = string.Join("|", logger.AllLogs.Select(x => x.Message.Trim()));
				CombineAssertions(() =>
				{
					AssertContains("Start Initialize DirectxT Connector.", logs);
					AssertContains("Finish Initialize DirectxT Connector. Total Time: ", logs);
					AssertContains("Start validating 3 interchange(s).", logs);
					AssertContains("Interchange: 1 was set to SNT.", logs);
					AssertContains("Interchange: 2 was set to ERR:Rejected by xT Server. Error 'ErrConfig - Configuration Error (28)' returned.", logs);
					AssertContains("Interchange: 3 was set to ERR:Rejected by xT Server. Error 'ErrPlock - The processing lock is set (14)' returned.", logs);
					AssertContains("Finish handling 3 interchange(s). Total Time: ", logs);
				});
			}
		}

		public static (List<EDIInterchange> interchangesShouldBeUpdated, List<EDIInterchange> interchangesShouldNotBeUpdated) SetupDataForTesting(BusinessObjectFactory factory)
		{
			using (DirectxTMessagingRegistry.Instance.XTIdleConnectionKeepAliveInSeconds.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
			using (DirectxTMessagingRegistry.Instance.XTIdleConnectionRetryPauseInSeconds.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
			{
				var interchangesShouldBeUpdated = new List<EDIInterchange>
				{
					TestDirectXtTestUtils.CreateInterchange(factory, EDIMessage.Status.Queued, true, EDIMessage.Direction.Transmit, EDIInterchange.TransportType.tXT),
					TestDirectXtTestUtils.CreateInterchange(factory, EDIMessage.Status.Queued, true, EDIMessage.Direction.Transmit, EDIInterchange.TransportType.tXT)
				};
				var interchangesShouldNotBeUpdated = new List<EDIInterchange>
				{
					TestDirectXtTestUtils.CreateInterchange(factory, EDIMessage.Status.Rejected, true, EDIMessage.Direction.Transmit, EDIInterchange.TransportType.tXT),
					TestDirectXtTestUtils.CreateInterchange(factory, EDIMessage.Status.Queued, false, EDIMessage.Direction.Transmit, EDIInterchange.TransportType.tXT),
					TestDirectXtTestUtils.CreateInterchange(factory, EDIMessage.Status.Queued, true, EDIMessage.Direction.Receive, EDIInterchange.TransportType.tXT),
					TestDirectXtTestUtils.CreateInterchange(factory, EDIMessage.Status.Queued, true, EDIMessage.Direction.Receive, EDIInterchange.TransportType.xT),
					TestDirectXtTestUtils.CreateInterchange(factory, EDIMessage.Status.Queued, true, EDIMessage.Direction.Transmit, EDIInterchange.TransportType.eHub),
					TestDirectXtTestUtils.CreateInterchange(factory, EDIMessage.Status.Queued, true, EDIMessage.Direction.Transmit, EDIInterchange.TransportType.eHub)
				};
				return (interchangesShouldBeUpdated, interchangesShouldNotBeUpdated);
			}
		}

		internal static void AssertEndToEndResult(List<EDIInterchange> interchangesShouldBeUpdated, List<EDIInterchange> interchangesShouldNotBeUpdated)
		{
			using (DirectxTMessagingRegistry.Instance.XTIdleConnectionKeepAliveInSeconds.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
			using (DirectxTMessagingRegistry.Instance.XTIdleConnectionRetryPauseInSeconds.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
			{
				CombineAssertions(() =>
				{
					foreach (var interchange in interchangesShouldBeUpdated)
					{
						interchange.Reload();
						AssertEquals($"{interchange.EI_IsActive}, {interchange.EI_ReceiveTransmit}, {interchange.EI_TransportType}", EDIMessage.Status.Sent, interchange.EI_Status);
						AssertEquals("Interchange xT Internal Msg ID should be updated: ", interchange.EI_XTInternalMsgID, 12345L);
					}

					foreach (var interchange in interchangesShouldNotBeUpdated)
					{
						interchange.Reload();
						AssertNotEquals($"{interchange.EI_IsActive}, {interchange.EI_ReceiveTransmit}, {interchange.EI_TransportType}", EDIMessage.Status.Sent, interchange.EI_Status);
						AssertEquals("Interchange xT Internal Msg ID should not be updated: ", interchange.EI_XTInternalMsgID, 0L);
					}
				});
			}
		}
	}
}
