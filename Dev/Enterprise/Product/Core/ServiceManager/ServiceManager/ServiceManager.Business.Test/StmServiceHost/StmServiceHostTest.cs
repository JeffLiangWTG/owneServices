using System;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.ServiceManager.Business.Testing
{
	[TestedType(typeof(StmServiceHost))]
	class StmServiceHostTest : Enterprise.ZArchitecture.Business.Testing.EnterpriseBusinessObjectTestCase
	{
		public void TestPreventDelete()
		{
			AssertEquals("PrefventDelete", false, PreventDeleteAttribute.IsTrue(typeof(StmServiceHost)));
		}

		public void TestProxyReadOnlyProperties()
		{
			var host = Factory.New<StmServiceHost>();
			host.SH_ProxyAutoDetect = true;
			Assert(host.SH_ProxyHostInfo.ReadOnly);
			Assert(host.SH_ProxyPortInfo.ReadOnly);
			Assert(host.SH_ProxyAuthenticationInfo.ReadOnly);
			Assert(host.SH_ProxyUserNameInfo.ReadOnly);
			Assert(host.SH_ProxyPasswordInfo.ReadOnly);

			host.SH_ProxyAutoDetect = false;
			Assert(!host.SH_ProxyHostInfo.ReadOnly);
			Assert(!host.SH_ProxyPortInfo.ReadOnly);
			Assert(!host.SH_ProxyAuthenticationInfo.ReadOnly);
			Assert(host.SH_ProxyUserNameInfo.ReadOnly);
			Assert(host.SH_ProxyPasswordInfo.ReadOnly);

			host.SH_ProxyAuthentication = true;
			Assert(!host.SH_ProxyUserNameInfo.ReadOnly);
			Assert(!host.SH_ProxyPasswordInfo.ReadOnly);
		}

		public void TestSH_Status()
		{
			var testHost = Factory.New<StmServiceHost>();
			var command = Db.Connection.Command($"SELECT SH_Status FROM dbo.StmServiceHost WHERE SH_PK = '{testHost.PK}'");
			Factory.Save();
			CombineAssertions(() =>
			{
				Test(testHost, null, command, "BizO and DB values of SH_Status should both be INS when SH_DeleteTimeStampUtc has not been set");
				Test(testHost, DateTime.UtcNow, command, "BizO and DB values of SH_Status should both be DEL at set Utc Time");
				Test(testHost, DateTime.UtcNow.AddMinutes(-1), command, "BizO and DB values of SH_Status should both be DEL after 1 minute");
				Test(testHost, DateTime.UtcNow.AddMinutes(-59), command, "BizO and DB values of SH_Status should both be DEL after 59 minutes");
				Test(testHost, DateTime.UtcNow.AddHours(-1), command, "BizO and DB values of SH_Status should both be OBS after 1 hour");
				Test(testHost, DateTime.UtcNow.AddHours(-3), command, "BizO and DB values of SH_Status should both be OBS after 3 hours");
				Test(testHost, DateTime.UtcNow.AddMinutes(1), command, "BizO and DB values of SH_Status should both be INS 1 Minute in the future");
				Test(testHost, DateTime.UtcNow.AddHours(1), command, "BizO and DB values of SH_Status should both be INS if timestamp 1 Hour in future");
				Test(testHost, DateTime.UtcNow.AddHours(3), command, "BizO and DB values of SH_Status should both be INS if timestamp 3 Hour in future");
			});

			void Test(StmServiceHost host, DateTime? setTime, DbCommand cmd, string testDescription)
			{
				if (setTime.HasValue)
				{
					testHost.SH_DeleteTimeStampUtc = (DateTime)setTime;
					Factory.Save();
				}
				AssertEquals(testDescription, testHost.SH_Status, (string)cmd.ExecuteScalar());
			}
		}

		public void TestStatusDescription()
		{
			var testHost = Factory.New<StmServiceHost>();
			CombineAssertions(() =>
			{
				Test(testHost, null, StmServiceHostStatusTypes.Descriptions.Installed, "INS -> Installed");
				Test(testHost, DateTime.UtcNow.AddMinutes(-1), StmServiceHostStatusTypes.Descriptions.Deleting, "DEL -> Deleting");
				Test(testHost, DateTime.UtcNow.AddHours(-2), StmServiceHostStatusTypes.Descriptions.Obsolete, "OBS -> Obsolete");
			});

			void Test(StmServiceHost host, DateTime? setTime, string expectedValue, string testDescription)
			{
				if (setTime.HasValue)
				{
					testHost.SH_DeleteTimeStampUtc = (DateTime)setTime;
					Factory.Save();
				}
				AssertEquals(testDescription, testHost.StatusDescription, expectedValue);
			}
		}

		public void TestProxySettings()
		{
			var host = Factory.New<StmServiceHost>();
			AssertEquals("default AutoDetect", true, host.SH_ProxyAutoDetect);

			AssertEquals(ZString.Empty, host.SH_ProxyHost);
			AssertEquals(0, host.SH_ProxyPort);
			AssertEquals(false, host.SH_ProxyAuthentication);
			AssertEquals(ZString.Empty, host.SH_ProxyUserName);
			AssertEquals(ZString.Empty, host.SH_ProxyPassword);

			host.SH_ProxyAutoDetect = false;
			host.SH_ProxyHost = "test.com";
			host.SH_ProxyPort = 1080;
			AssertEquals(false, host.SH_ProxyAutoDetect);
			AssertEquals("test.com", host.SH_ProxyHost);
			AssertEquals(1080, host.SH_ProxyPort);
			AssertEquals(false, host.SH_ProxyAuthentication);
			AssertEquals(ZString.Empty, host.SH_ProxyUserName);
			AssertEquals(ZString.Empty, host.SH_ProxyPassword);

			host.SH_ProxyAutoDetect = true;
			AssertEquals(true, host.SH_ProxyAutoDetect);
			AssertEquals(ZString.Empty, host.SH_ProxyHost);
			AssertEquals(0, host.SH_ProxyPort);
			AssertEquals(false, host.SH_ProxyAuthentication);
			AssertEquals(ZString.Empty, host.SH_ProxyUserName);
			AssertEquals(ZString.Empty, host.SH_ProxyPassword);

			host.SH_ProxyAutoDetect = false;
			host.SH_ProxyHost = "test.com";
			host.SH_ProxyPort = 1080;
			host.SH_ProxyAuthentication = true;
			host.SH_ProxyUserName = "user";
			host.SH_ProxyPassword = "password";
			AssertEquals(false, host.SH_ProxyAutoDetect);
			AssertEquals("test.com", host.SH_ProxyHost);
			AssertEquals(1080, host.SH_ProxyPort);
			AssertEquals(true, host.SH_ProxyAuthentication);
			AssertEquals("user", host.SH_ProxyUserName);
			AssertEquals("password", host.SH_ProxyPassword);

			host.SH_ProxyAuthentication = false;
			AssertEquals(false, host.SH_ProxyAutoDetect);
			AssertEquals("test.com", host.SH_ProxyHost);
			AssertEquals(1080, host.SH_ProxyPort);
			AssertEquals(false, host.SH_ProxyAuthentication);
			AssertEquals(ZString.Empty, host.SH_ProxyUserName);
			AssertEquals(ZString.Empty, host.SH_ProxyPassword);

			host.SH_ProxyAuthentication = true;
			host.SH_ProxyUserName = "user";
			host.SH_ProxyPassword = "password";
			host.SH_ProxyAutoDetect = true;
			AssertEquals(true, host.SH_ProxyAutoDetect);
			AssertEquals(ZString.Empty, host.SH_ProxyHost);
			AssertEquals(0, host.SH_ProxyPort);
			AssertEquals(false, host.SH_ProxyAuthentication);
			AssertEquals(ZString.Empty, host.SH_ProxyUserName);
			AssertEquals(ZString.Empty, host.SH_ProxyPassword);

			host.ProxyBypassOnLocal = true;
			AssertEquals(true, host.ProxyBypassOnLocal);

			host.ProxyBypassOnLocal = false;
			AssertEquals(false, host.ProxyBypassOnLocal);
		}

		public void TestConvertBypassAddressToRegex()
		{
			AssertEquals(@"^(?:.*://)?192(?::[0-9]{1,5})?$", StmServiceHost.ConvertBypassAddressToRegex("192"));
			AssertEquals(@"^(?:.*://)?10\.55(?::[0-9]{1,5})?$", StmServiceHost.ConvertBypassAddressToRegex("10.55"));
			AssertEquals(@"^(?:.*://)?10\.1\..*(?::[0-9]{1,5})?$", StmServiceHost.ConvertBypassAddressToRegex("10.1.*"));
			AssertEquals(@"^(?:.*://)?www\.example\..*(?::[0-9]{1,5})?$", StmServiceHost.ConvertBypassAddressToRegex("www.example.*"));
			AssertEquals(@"^(?:.*://)?.*:8080$", StmServiceHost.ConvertBypassAddressToRegex("*:8080"));
			AssertEquals(@"^(?:.*://)?1\.2\.3\.4:77$", StmServiceHost.ConvertBypassAddressToRegex("1.2.3.4:77"));
			AssertEquals(@"^http://4\.5\.6\.7:88$", StmServiceHost.ConvertBypassAddressToRegex("http://4.5.6.7:88"));
			AssertEquals(@"^htt.*://1\.2\.3\.4:80$", StmServiceHost.ConvertBypassAddressToRegex("htt*://1.2.3.4:80"));
			AssertEquals(@"^(?:.*://)?htt.*1\.2\.3\.4:80$", StmServiceHost.ConvertBypassAddressToRegex("htt*1.2.3.4:80"));
			AssertEquals(@"^http://.*(?::[0-9]{1,5})?$", StmServiceHost.ConvertBypassAddressToRegex("http://*"));
			AssertEquals(@"^(?:.*://)?123\.1.*\.66\..*(?::[0-9]{1,5})?$", StmServiceHost.ConvertBypassAddressToRegex("123.1*.66.*"));
		}

		public void TestGetWebProxy()
		{
			var host = Factory.New<StmServiceHost>();
			host.SH_ProxyAutoDetect = false;
			host.SH_ProxyHost = "127.0.0.1";
			host.SH_ProxyPort = 80;

			var proxy = host.GetWebProxy();
			var proxyUri = proxy.GetProxy(new Uri("http://www.cargowise.com"));
			AssertEquals("proxy address", "127.0.0.1", proxyUri.Host);
			AssertEquals("proxy port", 80, proxyUri.Port);

			host.ProxyBypassList = "198.1.*; 198.2.*.2:8080 ;;;";
			proxy = host.GetWebProxy();
			AssertEquals("bypassed", true, proxy.IsBypassed(new Uri("http://198.1.1.1")));
			AssertEquals("bypassed", false, proxy.IsBypassed(new Uri("http://198.2.2.2")));
			AssertEquals("bypassed", false, proxy.IsBypassed(new Uri("http://198.2.2.2:80")));
			AssertEquals("bypassed", true, proxy.IsBypassed(new Uri("http://198.2.2.2:8080")));
			AssertEquals("bypassed", false, proxy.IsBypassed(new Uri("http://198.2.2.3:8080")));

			host.ProxyBypassList = "";
			host.ProxyBypassOnLocal = true;
			proxy = host.GetWebProxy();
			AssertEquals("bypassed", true, proxy.IsBypassed(new Uri("http://webserver/")));

			host.ProxyBypassOnLocal = false;
			proxy = host.GetWebProxy();
			AssertEquals("bypassed", false, proxy.IsBypassed(new Uri("http://webserver/")));
		}

		public void TestProxyBypassOnLocal()
		{
			var host = Factory.New<StmServiceHost>();
			host.SH_ProxyAutoDetect = false;
			host.SH_ProxyHost = "my.host";
			host.SH_ProxyPort = 1234;
			AssertEquals("default ProxyBypassOnLocal", true, host.ProxyBypassOnLocal);

			host.ProxyBypassOnLocal = false;
			AssertEquals("ProxyBypassOnLocal", false, host.ProxyBypassOnLocal);

			host.ProxyBypassOnLocal = true;
			AssertEquals("ProxyBypassOnLocal", true, host.ProxyBypassOnLocal);

			host.ProxyBypassOnLocal = false;
			AssertEquals("ProxyBypassOnLocal", false, host.ProxyBypassOnLocal);

			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var hostReloaded = factory2.Load<StmServiceHost>(host.PK);
			AssertEquals("ProxyBypassOnLocal", false, hostReloaded.ProxyBypassOnLocal);
			hostReloaded.ProxyBypassOnLocal = true;
			factory2.Save();

			host.Reload();
			AssertEquals("ProxyBypassOnLocal", true, host.ProxyBypassOnLocal);
		}

		public void TestProxyBypassList()
		{
			var query = new ZQuery();
			var initialStmDataCount = Factory.GetDatabaseCount(typeof(StmData), query);

			var host = Factory.New<StmServiceHost>();
			host.SH_ProxyAutoDetect = false;
			host.SH_ProxyHost = "my.host";
			host.SH_ProxyPort = 1234;
			AssertEquals("default ProxyBypassList", "", host.ProxyBypassList);

			host.ProxyBypassList = "192.1.*;198.*:8080";
			AssertEquals("ProxyBypassList", "192.1.*;198.*:8080", host.ProxyBypassList);

			Factory.Save();
			AssertEquals("One StmData", initialStmDataCount + 1, Factory.GetDatabaseCount(typeof(StmData), query));

			var factory2 = new BusinessObjectFactory();
			var hostReloaded = factory2.Load<StmServiceHost>(host.PK);
			AssertEquals("ProxyBypassList", "192.1.*;198.*:8080", hostReloaded.ProxyBypassList);
			hostReloaded.ProxyBypassList = "www.cargowise.com";
			factory2.Save();
			AssertEquals("One StmData", initialStmDataCount + 1, factory2.GetDatabaseCount(typeof(StmData), query));

			host.Reload();
			AssertEquals("ProxyBypassList", "www.cargowise.com", host.ProxyBypassList);

			host.Delete();
			Factory.Save();
			AssertEquals("StmData deleted", initialStmDataCount, Factory.GetDatabaseCount(typeof(StmData), query));
		}
	}
}
