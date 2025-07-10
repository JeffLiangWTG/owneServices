using System.Text;
using NUnit.Framework;

namespace Enterprise.RemotePrinting.Client.Tests
{
	public class PrintHubConnectionTest : TestCase
	{
		public void TestCookieContainerIsInitialized()
		{
			var connection = new PrintHubConnection("http://localhost");
			AssertNotNull("CookieContainer should be ready for sticky session cookies", connection.CookieContainer);
		}

		public void TestPrintHubConnection_CloseFailed()
		{
			var connection = new PrintHubConnectionForTest("TestUrl");
			connection.ThrowExceptionForTest = true;
			var errorLog = new StringBuilder();
			connection.Logged += (message) => errorLog.Append(message);
			connection.Close_Exposed();

			AssertEquals("Error in SignalR when Hub Connection was closed: 'Connection was disconnected before invocation result was received.'",
				errorLog.ToString());
		}
	}
	class PrintHubConnectionForTest : PrintHubConnection
	{
		public PrintHubConnectionForTest(string url) : base(url)
		{
		}

		public void Close_Exposed() => OnClosed();
	}
}
