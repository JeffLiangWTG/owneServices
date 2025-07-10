using System;
using CargoWise.eHub.Adapter;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Moq;

namespace Enterprise.eHubMessaging.Business.DownloadHandler.Tests
{
	class MessageStatusSuccessHandlerTests : MessageStatusHandlerTests<MessageStatusSuccessHandler>
	{
		protected override IeHubMessage CreateMessage(MockRepository mocks, Guid sessionGuid)
		{
			var testeHubMessage = mocks.Create<IeHubMessage>(MockBehavior.Strict);
			testeHubMessage.Setup(m => m.TrackingID).Returns(sessionGuid);
			testeHubMessage.Setup(m => m.ApplicationCode).Returns(ApplicationCodeList.Codes.XMS);

			return testeHubMessage.Object;
		}

		protected override string[] HandledStatus { get { return new string[] { EDIInterchange.Status.Failed, EDIInterchange.Status.eHubPending, EDIInterchange.Status.eHubQueued }; } }

		protected override MessageStatusHandler TestHandler { get { return new MessageStatusSuccessHandler(); } }
	}
}
