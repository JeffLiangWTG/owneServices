using System;
using System.IO;
using System.Linq;
using System.ServiceModel;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.eHub.Adapter;
using Enterprise.eHubMessaging.Tests;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture;
using Moq;

namespace Enterprise.eHubMessaging.Business.DownloadHandler.Tests
{
	class MessageStatusFailedHandlerTests : MessageStatusHandlerTests<MessageStatusFailedHandler>
	{
		protected override IeHubMessage CreateMessage(MockRepository mocks, Guid sessionGuid)
		{
			var testMessageStream = new MemoryStream();
			var testFailureMessage = System.Text.Encoding.Default.GetBytes("test failure");
			testMessageStream.Write(testFailureMessage, 0, testFailureMessage.Length);
			testMessageStream.Seek(0, SeekOrigin.Begin);
			var testeHubMessage = mocks.Create<IeHubMessage>(MockBehavior.Strict);
			testeHubMessage.Setup(m => m.TrackingID).Returns(sessionGuid);
			testeHubMessage.Setup(m => m.ApplicationCode).Returns(ApplicationCodeList.Codes.XMS);
			testeHubMessage.Setup(m => m.MessageStream).Returns(testMessageStream);

			return testeHubMessage.Object;
		}

		protected override string[] HandledStatus { get { return new string[] { EDIInterchange.Status.eHubPending, EDIInterchange.Status.eHubQueued }; } }

		protected override MessageStatusHandler TestHandler { get { return new MessageStatusFailedHandler(); } }

		[UseSnapshotProtection]
		public void TestAllExceptionsReportedWhenSaveFactoryThrows()
		{
			using (var extraConnection = Db.NewExtraConnectionToMainDb())
			{
				var mocks = new MockRepository(MockBehavior.Default);
				var testInterchange = CreateTestInterchange(EDIInterchange.Status.eHubPending, EDIInterchange.Direction.Transmit);
				var sessionGuid = testInterchange.PK.ToGuid();

				var testeHubMessage = CreateMessage(mocks, sessionGuid);

				var handler = new Mock<MessageStatusFailedHandler>() { CallBase = true };
				handler.Setup(m => m.DbConnection).Returns(extraConnection);
				handler.Setup(x => x.SaveFactory())
								  .Callback(() =>
								  {
									  var cmd = extraConnection.Command("SELECT 1");
									  var reader = cmd.ExecuteReader();
									  throw new InvalidOperationException("SaveFactory");
								  });
				var exceptionThrown = AssertExceptionThrown<AggregateException>(() => handler.Object.SaveMessage(testeHubMessage, TestHelpers.ValidCompanyForTest(Handler.FactoryProvider.Current), new NotificationBuffer()));
				AssertEquals("The aggregate exception should contain two exceptions", 2, exceptionThrown.InnerExceptions.Count);
				Assert("Contains save exception", exceptionThrown.InnerExceptions.FirstOrDefault(e => e is InvalidOperationException && e.Message == "SaveFactory") != null);
				Assert("Contains dispose exception", exceptionThrown.InnerExceptions.FirstOrDefault(e => e is CommunicationException && e.Message == "Unable to execute command on the database") != null);
				AssertEquals(string.Empty, ErrorReporter.LastMessageReported);
			}
		}
	}
}
