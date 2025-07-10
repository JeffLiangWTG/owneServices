using System;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using Enterprise.Security;
using Moq;
using NUnit.Framework;

namespace Enterprise.Startup.Tools.Testing
{
	sealed class NetworkMonitorTest : TestCase
	{
		public void TestNetworkMonitorStopWillNotAddingResponseToList()
		{
			var monitor = new NetworkMonitorForTest();
			monitor.Start();
			Thread.Sleep(50);
			monitor.Stop();
			Thread.Sleep(100);
			AssertEquals(0, monitor.TcpPingResponsesList.Count);
		}

		public void TestGetIpAddressThrowException()
		{
			// Arrange
			var mockClient = new Mock<IWiseCloudSecurityClient>();
			mockClient.Setup(client => client.GetClientIPAddress(It.IsAny<string>(), It.IsAny<string>())).Throws(() => new Exception("Any exception"));
			using (ObjectFactory.Substitute(mockClient.Object))
			{
				// Act && Assert
				AssertNoExceptionThrown(() => _ = new NetworkMonitor().IPAddress);
				AssertEquals(NetworkMonitor.RetrieveIPErrorMessage, ErrorReporter.LastKeyReported);
			}
			ErrorReporter.Clear();
		}

		class NetworkMonitorForTest : NetworkMonitor
		{
			protected override TCPPingResponse GetResponse()
			{
				Thread.Sleep(100);
				return new TCPPingResponse(StartTime);
			}

			protected override double RemoteNetworkMonitorPingIntervalInMilliseconds => 30;
		}
	}
}
