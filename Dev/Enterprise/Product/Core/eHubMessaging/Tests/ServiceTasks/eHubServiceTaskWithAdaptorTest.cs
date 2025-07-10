using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Common;
using CargoWise.eHub.Adapter;
using CargoWise.EntityFramework;
using Enterprise.eHubMessaging.Business;
using Enterprise.eHubMessaging.ServiceTasks;
using Enterprise.ServiceManager.Business;
using Enterprise.ServiceManager.Shared;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.eHubMessaging.Tests.ServiceTasks
{
	[TestsSubclassesOf(typeof(eHubServiceTaskWithAdaptor))]
	abstract class eHubServiceTaskWithAdaptorTest<TServiceTaskWithAdaptor, TServiceTaskJobWithAdapter> : eHubServiceTaskTest<TServiceTaskWithAdaptor, TServiceTaskJobWithAdapter>
		where TServiceTaskWithAdaptor : eHubServiceTaskWithAdaptor
		where TServiceTaskJobWithAdapter : ServiceTaskJobWithAdapter
	{
		[TestDate(2016, 06, 12)]
		public void TestCommunicationExceptionHandling()
		{
			TestExceptionHandling(
				new CommunicationException("timeout", new WebException("The underlying connection was closed")),
				LogMessages.TimeoutMessage, shouldContainDiagnostics: true);
			TestExceptionHandling(
				new CommunicationException("The socket connection was aborted", new IOException("Unable to write data to the transport connection", new SocketException(10054))),
				LogMessages.TimeoutMessage, shouldContainDiagnostics: true);
			TestExceptionHandling(
				new CommunicationException("The socket connection was aborted", new IOException("Unable to read data from the transport connection: The connection was closed.")),
				LogMessages.TimeoutMessage, shouldContainDiagnostics: true);
			TestExceptionHandling(
				new CommunicationException("The socket connection was aborted", new SocketException(10054)),
				LogMessages.TimeoutMessage, shouldContainDiagnostics: true);
		}

		[TestDate(2016, 06, 12)]
		public void TestEndpointNotFoundExceptionHandling()
		{
			TestExceptionHandling(
				new EndpointNotFoundException("There was no endpoint listening at https://ehub-ausyd.cargowise.net/eHubGateway/eHubStreamedService.svc that could accept the message. This is often caused by an incorrect address or SOAP action. See InnerException, if present, for more details.", new WebException()),
				EndpointNotFoundMessage, shouldContainDiagnostics: true);
		}

		[TestDate(2016, 06, 12)]
		public void TestTimeoutExceptionHandling()
		{
			TestExceptionHandling(new TimeoutException(), LogMessages.TimeoutMessage, shouldContainDiagnostics: true);
		}

		[TestDate(2016, 06, 12)]
		public void TestProtocolExceptionWithInnerWebExceptionHandling()
		{
			TestExceptionHandling(
				new ProtocolException("The content type text/html of the response message does not match the content type of the binding (text/xml; charset=utf-8)", new WebException("(413) Request Entity Too Large")),
				"(413) Request Entity Too Large");
		}

		[TestDate(2016, 06, 12)]
		public void TestProtocolExceptionHandling()
		{
			TestExceptionHandling(new ProtocolException("The content type text/html of the response message does not match the content type of the binding (text/xml; charset=utf-8)"), "Protocal Error Occurred");
		}

		[TestDate(2016, 06, 12)]
		public void TestProtocolViolationExceptionHandling()
		{
			TestExceptionHandling(
				new ProtocolViolationException("Chunked encoding upload is not supported on the HTTP/1.0 protocol."),
				LogMessages.ProtocolViolationMessage);
		}

		[TestDate(2016, 06, 12)]
		public void TestServerTooBusyExceptionHandling()
		{
			TestExceptionHandling(
				new ServerTooBusyException("The HTTP service located at https://ehub-ausyd.cargowise.net/eHubGateway/eHubStreamedService.svc is unavailable. This could be because the service is too busy or because no endpoint was found listening at the specified address. Please ensure that the address is correct and try accessing the service again later.", new WebException()),
				LogMessages.ServerTooBusyMessage);
		}

		[TestDate(2016, 06, 12)]
		public void TestCreateAdapterExceptionHandling()
		{
			TestExceptionHandling(new CreateAdapterException(), "Adapter was null");
		}

		[TestDate(2016, 06, 12)]
		public void TestUriFormatExceptionHandling()
		{
			TestExceptionHandling(new UriFormatException(), LogMessages.BadUriMessage, true);
		}

		[TestDate(2016, 06, 12)]
		public void TesteHubAdapterExceptionHandling()
		{
			TestExceptionHandling(new eHubAdapterException("some error on eHub"), "some error on eHub");
		}

		[TestDate(2016, 06, 12)]
		public void TestArgumentExceptionHandling()
		{
			TestExceptionHandling(
				new ArgumentException(@"The provided URI scheme 'http' is invalid; expected 'https'.
Parameter name: via"),
				LogMessages.InvalidScheme("https", "http"),
				true);
		}

		[TestDate(2016, 06, 12)]
		public virtual void TestArgumentExceptionWithoutSchemeMessageHandling()
		{
			TestExceptionNotHandled(new ArgumentException("Some other exception"));
		}

		[TestDate(2016, 06, 12)]
		public void TestServiceActivationExceptionHandling()
		{
			TestExceptionHandling(
				new ServiceActivationException("The requested service, 'https://hub.5logistics.com/outbound/service.svc' could not be activated. See the server's diagnostic trace logs for more information."),
				"The requested service, 'https://hub.5logistics.com/outbound/service.svc' could not be activated. See the server's diagnostic trace logs for more information.");
		}

		protected override Mock<TServiceTaskWithAdaptor> CreateMock(ICompanySettingsManager companySettingsManager = null, bool runContinuously = false, bool isProduction = false)
		{
			var eHubCommunicationDiagnoster = new Mock<IEHubCommunicationDiagnoster>();
			eHubCommunicationDiagnoster.Setup(m => m.Run()).Returns("**Diagnostics**"); // skip running Diagnoster in this test
			var diagnosterFactory = new Mock<IEHubCommunicationDiagnosterFactory>();
			diagnosterFactory.Setup(m => m.Create(It.IsAny<string>())).Returns(eHubCommunicationDiagnoster.Object);

			var mockServiceTask = new Mock<TServiceTaskWithAdaptor>(CreateAdaptorFactory(), diagnosterFactory.Object) { CallBase = true };
			SetupMock(mockServiceTask, companySettingsManager, runContinuously, isProduction);
			return mockServiceTask;
		}

		protected override Mock<TServiceTaskJobWithAdapter> StubMockJob(Mock<TServiceTaskWithAdaptor> serviceTask)
		{
			var job = new Mock<TServiceTaskJobWithAdapter>(serviceTask.Object, serviceTask.Object.Notifier, CreateAdaptorFactory()) { CallBase = true };
			job.Setup(x => x.CanExecute).Returns(true);
			serviceTask.Setup(m => m.GetJobs()).Returns(new[] { job.Object });
			return job;
		}

		protected virtual IAdaptorFactory CreateAdaptorFactory() => new GatewayAdaptorFactory();

		#region Test via proxy functions

		protected Dictionary<string, int> GetHostAddressesFromProxy(string code)
		{
			int port = 9000;
			string proxyHost = "127.0.0.1";
			host.SH_ProxyAutoDetect = false;
			host.SH_ProxyHost = proxyHost;
			host.SH_ProxyPort = port;
			host.ProxyBypassOnLocal = false;
			DataRegistry.Instance.EHubTesting = true;
			Factory.Save();

			var listener = new ServerListener(proxyHost, port);
			listener.StartServer();

			var acceptConnectionTask = Task.Run(() =>
			{
				while (listener.AcceptConnection() != null)
				{ }
			});

			Thread.Sleep(1000);
			RunServiceTaskForProxy();
			Thread.Sleep(1000);

			listener.StopServer();
			acceptConnectionTask.Wait();

			return listener.GetConnectionURLs();
		}

		protected readonly string diagnosticTestURL = "www.wisetechglobal.com";

		protected void RunServiceTaskForProxy()
		{
			var serviceTask = new Mock<TServiceTaskWithAdaptor>() { CallBase = true };
			serviceTask.Object.ServiceLogger = new TestServiceLogger();

			WebRequest.DefaultWebProxy = null; // Set to not using proxy
			InitialiseAndRunTaskSchedule(serviceTask.Object);
		}

		protected void SetUpForProxyTest()
		{
			var hostName = ServiceManagerHelper.GetHostName();
			var loadedHost = Factory.LoadTop1<StmServiceHost>(new ZQuery(StmServiceHostSchema.SH_HostName, hostName));
			if (loadedHost != null)
			{
				host = loadedHost;
			}
			else
			{
				host = Factory.New<StmServiceHost>();
				host.SH_HostName = hostName;
				Factory.Save();
			}
		}

		StmServiceHost host;

		class ClientConnection
		{
			readonly Socket clientSocket;

			public ClientConnection(Socket client)
			{
				clientSocket = client;
			}

			public string GetHostAddress()
			{
				string eOL = "\r\n";
				string requestLine = "";
				byte[] requestBuffer = new byte[1];

				// Handle Request from Client
				while (!requestLine.EndsWith(eOL))
				{
					clientSocket.Receive(requestBuffer);
					string fromByte = Encoding.ASCII.GetString(requestBuffer);
					requestLine += fromByte;
				}

				// Send response to Client
				clientSocket.Send(Encoding.ASCII.GetBytes("RandomResponse"));

				clientSocket.Disconnect(false);
				clientSocket.Dispose();

				return requestLine.Trim().Split(' ')[1].Replace("http://", "").Split('/')[0].Split(':')[0];
			}
		}

		class ServerListener
		{
			readonly TcpListener listener;
			readonly Dictionary<string, int> addressTime;

			public ServerListener(string address, int port)
			{
				listener = new TcpListener(IPAddress.Parse(address), port);
				addressTime = new Dictionary<string, int>();
			}

			public void StartServer()
			{
				listener.Start();
			}

			public void StopServer()
			{
				listener.Stop();
			}

			public string AcceptConnection()
			{
				try
				{
					Socket newClient = listener.AcceptSocket();
					var client = new ClientConnection(newClient);
					var hostAddress = client.GetHostAddress();
					CountHostAddress(hostAddress);
					return hostAddress;
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					return null;
				}
			}

			void CountHostAddress(string hostAddress)
			{
				if (addressTime.ContainsKey(hostAddress))
				{
					addressTime[hostAddress] = addressTime[hostAddress] + 1;
				}
				else
				{
					addressTime[hostAddress] = 1;
				}
			}

			internal Dictionary<string, int> GetConnectionURLs()
			{
				return addressTime;
			}
		}

		#endregion
	}
}
