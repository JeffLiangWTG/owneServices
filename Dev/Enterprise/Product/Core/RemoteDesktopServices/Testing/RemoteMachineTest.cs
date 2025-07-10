using System.Net;
using Enterprise.RemoteDesktopServices.Server;
using NUnit.Framework;

namespace Enterprise.RemoteDesktopServices.Testing
{
	class RemoteMachineTest : RemoteDesktopServicesTest
	{
		public void TestGetFullyQualifiedMachineName()
		{
			AssertEquals(Dns.GetHostEntry("LocalHost").HostName.ToLowerInvariant(), new RemoteClient(Server.EnterpriseChannel.Instance).FullyQualifiedMachineName);
		}
	}

	class RemoteClientTest : TestCase
	{
		[DeveloperOnlyTest]
		//This test is for debug build running as remote application and expected to fail
		public void TestGetFullyQualifiedMachineName()
		{
			AssertEquals(Dns.GetHostEntry("LocalHost").HostName.ToLowerInvariant(), new RemoteClient(Server.EnterpriseChannel.Instance).FullyQualifiedMachineName);
		}
	}
}
