using System;
using Enterprise.AlwaysOn.Setup;
using NUnit.Framework;

namespace Enterprise.AlwaysOn.Testing
{
	public class ListenerTest : TestCase
	{
		public void TestListenerConstruction_WithoutId()
		{
			const string testDnsName = "test.dns.com";
			const string testIp = "192.168.0.1";
			const string testNetMask = "255.255.255.0";
			const int testPort = 22;

			var testSqlServer = new SqlServerInfo("My test\\SQL Server");
			var testDatabase = AlwaysOnDatabaseFactory.New("TestDB", "TheGroup", Guid.Empty);
			var testServer = new PrimaryServerInstance(testSqlServer);
			var testGroup = new AvailabilityGroup(testSqlServer, testDatabase);

			var listenerIp = new ListenerIp(testIp, testNetMask);
			var testListener = new Listener(testDnsName, testPort, testServer, testGroup, listenerIp);

			AssertNotNull(testListener);
			AssertNull(testListener.Id);
			AssertEquals(testDnsName, testListener.DnsName);
			AssertEquals(testIp, testListener.IpAddresses[0].IpAddress);
			AssertEquals(testNetMask, testListener.IpAddresses[0].SubnetMask);
			AssertEquals(testPort, testListener.Port);
			AssertEquals(testServer, testListener.Server);
			AssertEquals(testGroup, testListener.AvailabilityGroup);
		}

		public void TestListenerConstruction_WithId()
		{
			const string testDnsName = "test.dns.com";
			const string testIp = "192.168.0.1";
			const string testNetMask = "255.255.255.0";
			const int testPort = 22;
			const string testId = "anidfordb";

			var testSqlServer = new SqlServerInfo("My test\\SQL Server");
			var testDatabase = AlwaysOnDatabaseFactory.New("TestDB", "TheGroup", Guid.Empty);
			var testServer = new PrimaryServerInstance(testSqlServer);
			var testGroup = new AvailabilityGroup(testSqlServer, testDatabase);

			var listenerIp = new ListenerIp(testIp, testNetMask);
			var testListener = new Listener(testDnsName, testPort, testServer, testGroup, listenerIp, testId);

			AssertNotNull(testListener);
			AssertEquals(testId, testListener.Id);
			AssertEquals(testDnsName, testListener.DnsName);
			AssertEquals(testIp, testListener.IpAddresses[0].IpAddress);
			AssertEquals(testNetMask, testListener.IpAddresses[0].SubnetMask);
			AssertEquals(testPort, testListener.Port);
			AssertEquals(testServer, testListener.Server);
			AssertEquals(testGroup, testListener.AvailabilityGroup);
		}
	}
}
