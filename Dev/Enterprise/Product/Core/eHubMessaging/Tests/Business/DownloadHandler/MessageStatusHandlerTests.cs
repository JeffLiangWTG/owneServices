using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.eHub.Adapter;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.eHubMessaging.Tests;
using Enterprise.eHubMessaging.Tests.Business.DownloadHandler;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture;
using Moq;

namespace Enterprise.eHubMessaging.Business.DownloadHandler.Tests
{
	abstract class MessageStatusHandlerTests<THandler> : TestCaseWithFactory where THandler : MessageStatusHandler, new()
	{
		public void TestSaveMessage_Successful()
		{
			var mocks = new MockRepository(MockBehavior.Default);
			var testInterchange = CreateTestInterchange(EDIInterchange.Status.eHubPending, EDIInterchange.Direction.Transmit);
			var sessionGuid = testInterchange.PK.ToGuid();

			var testeHubMessage = CreateMessage(mocks, sessionGuid);

			Handler.SaveMessage(testeHubMessage, TestHelpers.ValidCompanyForTest(Handler.FactoryProvider.Current), new NotificationBuffer());

			Factory.ReloadAll<EDIInterchange>();

			CombineAssertions(() =>
			{
				AssertEquals(HandlerStatus, testInterchange.EI_Status);
				AssertEquals("Last Key Reported: ", string.Empty, ErrorReporter.LastKeyReported);
				AssertEquals("Last Message Reported: ", string.Empty, ErrorReporter.LastMessageReported);
			});
		}

		public void TestSaveMessage_DeletedInterchange()
		{
			try
			{
				var mocks = new MockRepository(MockBehavior.Default);
				var testInterchange = CreateTestInterchange(EDIInterchange.Status.eHubPending, EDIInterchange.Direction.Transmit);
				var sessionGuid = testInterchange.EI_SessionGUID.ToGuid();
				testInterchange.EI_SessionGUID = sessionGuid;

				testInterchange.Delete();
				Factory.Save();

				var testeHubMessage = CreateMessage(mocks, sessionGuid);

				var notification = new NotificationBuffer();
				Handler.SaveMessage(testeHubMessage, TestHelpers.ValidCompanyForTest(Handler.FactoryProvider.Current), notification);

				AssertEquals(String.Empty, ErrorReporter.LastMessageReported);
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}

		public void TestSaveMessage_StatusNotPending()
		{
			foreach (var status in HandledStatus)
			{
				HandleMessageWithNonPendingStatus(status);
			}

			HandleMessageWithNonPendingStatus(EDIInterchange.Status.Received);
		}

		public void TestSaveMessage_SameSenderAndRecipient()
		{
			try
			{
				var mocks = new MockRepository(MockBehavior.Default);
				var inComingInterchange = CreateTestInterchange(EDIInterchange.Status.Received, EDIInterchange.Direction.Receive);
				var outGoingInterchange = CreateTestInterchange(EDIInterchange.Status.eHubPending, EDIInterchange.Direction.Transmit, inComingInterchange.EI_SessionGUID.ToGuid());
				var sessionGuid = outGoingInterchange.EI_SessionGUID.ToGuid();
				AssertEquals(outGoingInterchange.EI_SessionGUID, inComingInterchange.EI_SessionGUID);

				var testMessage = CreateMessage(mocks, sessionGuid);

				Handler.SaveMessage(testMessage, TestHelpers.ValidCompanyForTest(Handler.FactoryProvider.Current), new NotificationBuffer());

				CombineAssertions(() =>
				{
					AssertEquals("Last Key Reported: ", string.Empty, ErrorReporter.LastKeyReported);
					AssertEquals("Last Message Reported: ", string.Empty, ErrorReporter.LastMessageReported);
				});
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}

		public void TestSaveMessage_DuplicateInterchange()
		{
			try
			{
				var mocks = new MockRepository(MockBehavior.Default);
				var inComingInterchange = CreateTestInterchange(EDIInterchange.Status.Received, EDIInterchange.Direction.Receive);
				var outGoingInterchange1 = CreateTestInterchange(EDIInterchange.Status.eHubPending, EDIInterchange.Direction.Transmit, inComingInterchange.EI_SessionGUID.ToGuid());
				var outGoingInterchange2 = CreateTestInterchange(EDIInterchange.Status.eHubPending, EDIInterchange.Direction.Transmit, inComingInterchange.EI_SessionGUID.ToGuid());
				var sessionGuid = inComingInterchange.EI_SessionGUID.ToGuid();
				AssertEquals(outGoingInterchange1.EI_SessionGUID, inComingInterchange.EI_SessionGUID);
				AssertEquals(outGoingInterchange2.EI_SessionGUID, inComingInterchange.EI_SessionGUID);

				var testMessage = CreateMessage(mocks, sessionGuid);

				Handler.SaveMessage(testMessage, TestHelpers.ValidCompanyForTest(Handler.FactoryProvider.Current), new NotificationBuffer());

				Factory.ReloadAll<EDIInterchange>();
				CombineAssertions(() =>
				{
					Assert("One of the duplicate interchanges should have been acknowledged", outGoingInterchange1.EI_Status == HandlerStatus || outGoingInterchange2.EI_Status == HandlerStatus);
					AssertEquals("Missing ErrorReport", MessageStatusHandler.DuplicateInterchangeErrorKey, ErrorReporter.LastKeyReported);
					AssertEquals("Last Message Reported", string.Format("Duplicate Interchanges found - Interchange with {0} '{1}' was found {2} time(s)", outGoingInterchange1.EI_SessionGUIDInfo.HumanReadableName, sessionGuid, 2), ErrorReporter.LastMessageReported);
				});
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}

		#region Abstract Members

		protected abstract MessageStatusHandler TestHandler { get; }

		protected abstract IeHubMessage CreateMessage(MockRepository mocks, Guid sessionGuid);

		protected abstract string[] HandledStatus { get; }

		#endregion

		#region Scenarios

		void HandleMessageWithNonPendingStatus(string status)
		{
			try
			{
				var mocks = new MockRepository(MockBehavior.Default);
				var testInterchange = CreateTestInterchange(status, EDIInterchange.Direction.Transmit);
				var sessionGuid = testInterchange.EI_SessionGUID.ToGuid();
				var testMessage = CreateMessage(mocks, sessionGuid);

				Handler.SaveMessage(testMessage, TestHelpers.ValidCompanyForTest(Handler.FactoryProvider.Current), new NotificationBuffer());

				if (HandledStatus.Contains(status))
				{
					AssertEquals("Last Key Reported", string.Empty, ErrorReporter.LastKeyReported);
				}
				else
				{
					AssertEquals("Last Key Reported", MessageStatusHandler.InterchangeNotFoundErrorKey, ErrorReporter.LastKeyReported);
				}
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}

		#endregion

		#region Test Helper Methods

		protected MessageStatusHandler Handler { get { return new THandler(); } }

		protected string HandlerStatus { get { return (Handler is MessageStatusSuccessHandler) ? EDIInterchange.Status.Sent : EDIInterchange.Status.Failed; } }

		protected EDIInterchange CreateTestInterchangeMessage(string from, string to, string receiveTransmit, BusinessObjectFactory factory, bool onlyCreateInterchange)
		{
			return TestInterchangeMessage.GetNew(TestHelpers.ValidCompanyForTest(factory).FirstActiveBranch.PK, from, to, receiveTransmit, factory, onlyCreateInterchange);
		}

		protected EDIInterchange CreateTestInterchange(string status, string receiveTransmit)
		{
			var testInterchange = CreateTestInterchangeMessage("BLAH", "BLAH", receiveTransmit, Factory, true);
			testInterchange.EditAs(status, receiveTransmit, Factory);
			return testInterchange;
		}

		protected EDIInterchange CreateTestInterchange(string status, string receiveTransmit, Guid sessionGuid)
		{
			var testInterchange = CreateTestInterchangeMessage("BLAH", "BLAH", receiveTransmit, Factory, true);
			testInterchange.EditAs(status, receiveTransmit, sessionGuid, Factory);
			return testInterchange;
		}

		#endregion
	}
}
