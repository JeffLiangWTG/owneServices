using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.ServiceModel;
using System.Threading;
using CargoWise.eHub.Adapter;
using CargoWise.EntityFramework;
using Enterprise.eHubMessaging.ServiceTasks;
using Enterprise.eHubMessaging.Tests.EndToEndMessageProcessing.Mocks;
using Moq;
using NUnit.Framework;

namespace Enterprise.eHubMessaging.Tests.ServiceTasks.Outbound
{
	[TestsSubclassesOf(typeof(OutboundServiceTaskJob<,>))]
	abstract class OutboundServiceTaskJobTest<TOutboundServiceTaskJob, TOutboundItem, TLightweightOutboundItem> : ServiceTaskJobWithAdapterTests<TOutboundServiceTaskJob>
		where TOutboundServiceTaskJob : OutboundServiceTaskJob<TOutboundItem, TLightweightOutboundItem>
		where TOutboundItem : BusinessObject
		where TLightweightOutboundItem : LightweightOutboundItem<TOutboundItem>
	{
		public void TestAdapterSendMessagesExceptionsNotHandled()
		{
			TestAdapterSendMessagesException(new ArgumentException("The provided URI scheme 'http' is invalid; expected 'https'.\r\nParameter name: via"));
			TestAdapterSendMessagesException(new CommunicationException("timeout", new WebException("The underlying connection was closed")));
			TestAdapterSendMessagesException(new CommunicationException("The socket connection was aborted", new IOException("Unable to write data to the transport connection", new SocketException(10054))));
			TestAdapterSendMessagesException(new ServiceActivationException("The requested service, 'https://hub.5logistics.com/outbound/service.svc' could not be activated. See the server's diagnostic trace logs for more information."));
			TestAdapterSendMessagesException(new ProtocolException("The content type text/html of the response message does not match the content type of the binding (text/xml; charset=utf-8)", new WebException("(413) Request Entity Too Large")));
			TestAdapterSendMessagesException(new ServerTooBusyException("The HTTP service located at https://ehub-ausyd.cargowise.net/eHubGateway/eHubStreamedService.svc is unavailable. This could be because the service is too busy or because no endpoint was found listening at the specified address. Please ensure that the address is correct and try accessing the service again later.", new WebException()));
			TestAdapterSendMessagesException(new EndpointNotFoundException("There was no endpoint listening at https://ehub-ausyd.cargowise.net/eHubGateway/eHubStreamedService.svc that could accept the message. This is often caused by an incorrect address or SOAP action. See InnerException, if present, for more details.", new WebException()));
			TestAdapterSendMessagesException(new TimeoutException());
			TestAdapterSendMessagesException(new ProtocolViolationException());
		}

		void TestAdapterSendMessagesException<TException>(TException exception)
			where TException : Exception
		{
			var mockOutbox = new Mock<IMessageOutbox>(MockBehavior.Strict);
			mockOutbox.Setup(x => x.AddMessage(It.IsAny<IeHubMessage>()));
			mockOutbox.Setup(x => x.Count).Returns(1);
			mockOutbox.Setup(x => x.SizeInKiloBytes).Returns(100);
			mockOutbox.Setup(x => x.Clear());

			var mockAdapter = new Mock<IeHubAdapter>(MockBehavior.Strict);
			mockAdapter.Setup(x => x.Outbox).Returns(mockOutbox.Object);
			mockAdapter.Setup(x => x.SendMessages()).Throws(exception);
			mockAdapter.Setup(x => x.Dispose());

			var mockServiceTaskJob = CreateMockJob_Moq(adaptorFactory: new AdaptorFactoryMockWithOneAdaptor(mockAdapter.Object));
			var exceptionThrown = AssertExceptionThrown<TException>(() => mockServiceTaskJob.Object.Execute(CancellationToken.None));
			AssertEquals(exception, exceptionThrown);
		}
	}
}
