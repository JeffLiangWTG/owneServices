using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MailManager;
using Enterprise.MailManager.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class DiagnosticCycleControllerTest : TestCaseWithFactory
	{
		public void TestLoadReplyEdiInterchangeByIndex()
		{
			using (TestConnection.TrackExecutedCommands())
			{
				var consolKey = ZGuid.NewZGuid().ToString();

				var message = Factory.New<CMRSTREQRMessage>();
				Factory.Save();

				testController = new DiagnosticCycleControllerForTest(message.PK, consolKey, Factory);
				testController.CheckNextDiagnosticCycleStep(DiagnosticStatusList.Codes.WaitingForTestMessageResponse);

				var executedCommand = TestConnection.ExecutedCommands.First(c => c.Contains("SELECT  TOP 1") && c.Contains("FROM dbo.EDIInterchange"));

				AssertContains(@"EDIInterchange often has a large data count, we should make sure the query should be supported by appropriate indexes. This filter will fetch the index - 'NR_RX__EI_SystemCreateTimeUtc' with a large data count. See more details in CS00879576 And WI00346569.",
					@"and EI_SystemCreateTimeUtc >",
					executedCommand,
					true);
			}
		}

		public void TestCheckNextDiagnosticCycleStep()
		{
			var testMessage = Factory.New<CMRSTREQRMessage>();
			var consolKey = ZGuid.NewZGuid().ToString();
			testController = new DiagnosticCycleControllerForTest(testMessage.PK, consolKey, Factory);

			AssertEquals("Nonexistant step ignored", "!Ex", testController.CheckNextDiagnosticCycleStep("!Ex"));

			//
			// TestMessageAtQUEStatus => LookingForOutgoingItem
			AssertStepTransition(
				DiagnosticStatusList.Codes.TestMessageAtQUEStatus,
				DiagnosticStatusList.Codes.TestMessageStatusInvalid,
				() => testMessage.EM_Status = "XXX",
				DiagnosticStatusList.Codes.LookingForOutgoingItem,
				() =>
				{
					testMessage.EM_Status = EDIMessage.Status.Sent;
					var intx = Factory.New<EDIInterchange>();
					intx.EI_Status = EDIInterchange.Status.Queued;
					testMessage.EM_EI = intx.PK;
				}
			);

			//
			// TestLookingForOutgoingItem => OutgoingItemAtQUEStatus
			var outEmail = Factory.New<MailItem>();
			AssertStepTransition(
				DiagnosticStatusList.Codes.LookingForOutgoingItem,
				DiagnosticStatusList.Codes.OutInterchangeStatusInvalid,
				() => testMessage.Interchange.EI_Status = "XXX",
				DiagnosticStatusList.Codes.OutgoingItemAtQUEStatus,
				() =>
				{
					testMessage.Interchange.EI_Status = EDIInterchange.Status.Sent;
					outEmail.MI_Direction = MailDirection.Transmit;
				}
			);

			//
			// OutgoingItemAtQUEStatus => WaitingForTestMessageResponse
			AssertEquals("OEQ -> WTR has no conditions, should pass straight on", DiagnosticStatusList.Codes.WaitingForTestMessageResponse, testController.CheckNextDiagnosticCycleStep(DiagnosticStatusList.Codes.OutgoingItemAtQUEStatus));

			//
			// WaitingForTestMessageResponse = > InResponseMailReceived
			var inIntx = Factory.New<EDIInterchange>();

			AssertStepTransition(
				DiagnosticStatusList.Codes.WaitingForTestMessageResponse,
				null, null,
				DiagnosticStatusList.Codes.InComingResponseReceived,
				() =>
				{
					testMessage.Interchange.EI_Status = EDIInterchange.Status.Sent;
					inIntx.EI_ApplicationCode = EDIInterchange.ApplicationCodes.CMR;
					inIntx.EI_InterchangeType = "AUC";
					inIntx.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
					inIntx.EI_InterchangeNum = testMessage.Interchange.EI_InterchangeNum;
					inIntx.EI_SystemCreateTimeUtc = DateTime.Now;
					inIntx.EI_SystemLastEditTimeUtc = DateTime.Now;
					inIntx.EI_BodyText = "RFF+ABO:" + consolKey.Replace("-", "");
				}
			);

			//
			// InResponseMailReceived => InInterchangeAtQUEStatus
			var inEdiMsg = Factory.New<EDIMessage>();
			AssertStepTransition(
				DiagnosticStatusList.Codes.InComingResponseReceived,
				null, null,
				DiagnosticStatusList.Codes.InInterchangeProcessed,
				() =>
				{
					inEdiMsg.EM_EI = inIntx.PK;
					inIntx.ContainedMessages.Add(inEdiMsg);
				}
			);

			//
			// InInterchangeAtQUEStatus => OKSuccess
			AssertStepTransition(
				DiagnosticStatusList.Codes.InInterchangeProcessed,
				null, null,
				DiagnosticStatusList.Codes.OKSuccess,
				() => inEdiMsg.EM_Status = EDIMessage.Status.Error
			);
		}

		void AssertStepTransition(string currentStatus, string expectedFailStatus, Action setFailScenario, string expectedNextStatus, Action nextStepAction)
		{
			AssertEquals("Before performing step", currentStatus, testController.CheckNextDiagnosticCycleStep(currentStatus));

			if (expectedFailStatus != null)
			{
				AssertWhenDiagnosticProcessFails(currentStatus, expectedFailStatus, setFailScenario);
			}

			nextStepAction();
			AssertEquals("After performing step", expectedNextStatus, testController.CheckNextDiagnosticCycleStep(currentStatus));
		}

		void AssertWhenDiagnosticProcessFails(string currentStatus, string expectedFailStatus, Action setFailScenario)
		{
			try
			{
				setFailScenario();
				testController.CheckNextDiagnosticCycleStep(currentStatus);
				Fail("Should throw exception");
			}
			catch (DiagnosticMessageException ex)
			{
				AssertEquals("Failed Status", expectedFailStatus, ex.FailStatus);
			}
		}

		DiagnosticCycleController testController;

		class DiagnosticCycleControllerForTest : DiagnosticCycleController
		{
			public DiagnosticCycleControllerForTest(ZGuid testEdiMessagePk, ZString consolKey, BusinessObjectFactory testFactory)
				: base(testEdiMessagePk, DateTime.Now.AddDays(-3), consolKey)
			{
				this.testFactory = testFactory;
			}

			protected override BusinessObjectFactory GetNewBusinessObjectFactory() => testFactory;

			readonly BusinessObjectFactory testFactory;
		}
	}
}
