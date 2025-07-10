using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Enterprise.AlwaysOn.Setup;
using Moq;

namespace Enterprise.AlwaysOn.Testing
{
	public class ListenerFactoryTest : AlwaysOnTestFixture
	{
		public void TestNew()
		{
			var testIp1 = "192.168.0.1";
			var testNetMask1 = "255.255.255.0";

			var testListenerIp = new List<ListenerIp>();
			testListenerIp.Add(new ListenerIp(testIp1, testNetMask1));

			var testListener = ListenerFactory.New(testDnsName, testPort, testServer, testGroup, testListenerIp);

			AssertNotNull(testListener);
			AssertEquals(1, testListener.IpAddresses.Count);
			AssertNull(testListener.Id);
			AssertEquals(testDnsName, testListener.DnsName);
			AssertEquals(testIp1, testListener.IpAddresses[0].IpAddress);
			AssertEquals(testNetMask1, testListener.IpAddresses[0].SubnetMask);
			AssertEquals(testPort, testListener.Port);
			AssertEquals(testServer, testListener.Server);
			AssertEquals(testGroup, testListener.AvailabilityGroup);

			var testIp2 = "10.11.12.13";
			var testNetMask2 = "255.255.0.0";
			testListener.IpAddresses.Add(new ListenerIp(testIp2, testNetMask2));

			AssertEquals(2, testListener.IpAddresses.Count);
			AssertEquals(testDnsName, testListener.DnsName);
			AssertEquals(testIp1, testListener.IpAddresses[0].IpAddress);
			AssertEquals(testIp2, testListener.IpAddresses[1].IpAddress);
			AssertEquals(testNetMask1, testListener.IpAddresses[0].SubnetMask);
			AssertEquals(testNetMask2, testListener.IpAddresses[1].SubnetMask);
			AssertEquals(testPort, testListener.Port);
			AssertEquals(testServer, testListener.Server);
			AssertEquals(testGroup, testListener.AvailabilityGroup);

			AssertEquals("ALTER AVAILABILITY GROUP [TheGroup] ADD LISTENER N'test.dns.com' (WITH IP ((N'192.168.0.1', N'255.255.255.0'),(N'10.11.12.13', N'255.255.0.0')), PORT=22);", ListenerFactory.GetSaveListenerSqlStatement(testListener));
		}

		public void TestSaveListener_DoesNotRemoveExistingListeners()
		{
			// Arrange
			const string listener1testIp = "192.168.0.1";
			const string testNetMask = "255.255.255.0";
			var testListener1Ip = new List<ListenerIp> { new ListenerIp(listener1testIp, testNetMask) };
			var testListener1 = ListenerFactory.New(testDnsName, testPort, testServer, testGroup, testListener1Ip);

			var scriptList = new List<string>();
			var sqlContext = new SqlExecutionContextForTest(scriptList, null);

			ListenerFactory.Save(testListener1, sqlContext);
			AssertEquals(1, scriptList.Count);
			AssertEquals("First listener was successfully saved.", "ALTER AVAILABILITY GROUP [TheGroup] ADD LISTENER N'test.dns.com' (WITH IP ((N'192.168.0.1', N'255.255.255.0')), PORT=22);", scriptList[0]);

			const string testDnsName2 = "second.listener.com";
			const string listener2testIp = "0.0.0.2";
			var testListener2Ip = new List<ListenerIp> { new ListenerIp(listener2testIp, testNetMask) };
			var testListener2 = ListenerFactory.New(testDnsName2, testPort, testServer, testGroup, testListener2Ip);

			// Act
			ListenerFactory.Save(testListener2, sqlContext);

			// Assert
			AssertEquals(2, scriptList.Count);
			Assert("Existing listener was not removed.", !scriptList.Exists(x => x.Contains("REMOVE")));
			AssertEquals("Second listener was successfully saved.", "ALTER AVAILABILITY GROUP [TheGroup] ADD LISTENER N'second.listener.com' (WITH IP ((N'0.0.0.2', N'255.255.255.0')), PORT=22);", scriptList[1]);
		}

		public void TestUpdateExistingListener_DoesNotRemoveExistingListeners()
		{
			// Arrange
			const string testIp1 = "192.168.0.1";
			const string testIp2 = "0.0.0.2";
			const string testNetMask = "255.255.255.0";
			const string testId = "testIDforDB";
			var testListenerIp = new List<ListenerIp> { new ListenerIp(testIp1, testNetMask) };
			var testListener = ListenerFactory.New(testDnsName, testPort, testServer, testGroup, testListenerIp, testId);

			var scriptList = new List<string>();
			var sqlContext = new SqlExecutionContextForTest(scriptList, null);

			var readerMock = new Mock<IDataReader>();
			readerMock.SetupSequence(m => m.Read()).Returns(false).Returns(true).Returns(false);
			readerMock.Setup(m => m.GetString(0)).Returns(testIp1);
			readerMock.Setup(m => m.GetString(1)).Returns(testNetMask);

			sqlContext.dataReaderOverride = readerMock.Object;

			ListenerFactory.Save(testListener, sqlContext);
			var alterQueries = scriptList.FindAll(sql => sql.Contains("ALTER"));
			AssertEquals(1, alterQueries.Count);
			AssertEquals("Initial listener was successfully saved.", "ALTER AVAILABILITY GROUP [TheGroup] ADD LISTENER N'test.dns.com' (WITH IP ((N'192.168.0.1', N'255.255.255.0')), PORT=22);", alterQueries.First());

			testListener.IpAddresses.Add(new ListenerIp(testIp2, testNetMask));

			// Act
			ListenerFactory.Save(testListener, sqlContext);

			// Assert
			alterQueries = scriptList.FindAll(sql => sql.Contains("ALTER"));
			AssertEquals(2, alterQueries.Count);
			Assert("Existing listener was not removed.", !scriptList.Exists(x => x.Contains("REMOVE")));
			AssertEquals("Existing listener was modified with additional IP", "ALTER AVAILABILITY GROUP [TheGroup] MODIFY LISTENER N'test.dns.com' (ADD IP (N'0.0.0.2', N'255.255.255.0'));", alterQueries.Last());
		}

		protected override void SetUp()
		{
			testSqlServer = new SqlServerInfo("My test\\SQL Server");
			testDatabase = AlwaysOnDatabaseFactory.New("TestDB", "TheGroup", Guid.Empty);
			testServer = new PrimaryServerInstance(testSqlServer);
			testGroup = new AvailabilityGroup(testSqlServer, testDatabase);
			testDnsName = "test.dns.com";
			testPort = 22;

			base.SetUp();
		}

		SqlServerInfo testSqlServer;
		IAlwaysOnDatabase testDatabase;
		PrimaryServerInstance testServer;
		AvailabilityGroup testGroup;
		string testDnsName;
		int testPort;
	}
}
