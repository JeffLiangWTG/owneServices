using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using CargoWiseOne.WebInfrastructure.TestFramework;
using Moq;
using NUnit.Framework;

namespace Enterprise.RemotePrinting.Server.Testing
{
	[TestRequiresAdministrativePrivileges("Admin privilege is required to access web site.")]
	class SiteConfigRetrieverTest : TestCase
	{
		public void TestGetSiteConfigWithForceToUseHttps()
		{
			var site = testManager.AddWebSite((string name) => Directory.CreateDirectory(Path.Combine(rootDirectory, name)).FullName);

			const string appName = "/SomeApplicationForTest";
			var path = Directory.CreateDirectory(Path.Combine(WebSiteTestManager.GetPhysicalPath(site), appName.Substring(1))).FullName;
			var app = site.Applications.Add(appName, path);

			site.Bindings.Clear();
			site.Bindings.Add("10.20.30.50:80:test.com", "http");
			site.Bindings.Add("10.20.30.50:443:test.com", "https");

			testManager.ServerManager.CommitChanges();

			var siteConfig = new SiteConfigRetriever(site.Id, appName, IPAddress.Parse("10.20.30.50"), new Uri("http://test.com"), false);
			var (protocol, port, hostName) = siteConfig.GetSiteConfig();

			AssertEquals("http", protocol);
			AssertEquals(80, port);
			AssertEquals("test.com", hostName);

			siteConfig = new SiteConfigRetriever(site.Id, appName, IPAddress.Parse("10.20.30.50"), new Uri("http://test.com"), true);
			(protocol, port, hostName) = siteConfig.GetSiteConfig();

			AssertEquals("https", protocol);
			AssertEquals(443, port);
			AssertEquals("test.com", hostName);
		}

		public void TestGetSiteConfigShouldHandleCOMException()
		{
			var mockRetriever = new Mock<SiteConfigRetriever>(1234L, "/SomeApplicationForTest", IPAddress.Parse("0.0.0.0"), new Uri("http://test.com"), false);
			mockRetriever.Setup(r => r.Execute()).Throws(new COMException("Creating an instance of the COM component with CLSID {2B72133B-3F5B-4602-8952-803546CE3344} from the IClassFactory failed due to the following error: 80070008 Not enough storage is available to process this command. (Exception from HRESULT: 0x80070008)."));
			var retriever = mockRetriever.Object;

			AssertNoExceptionThrown(() => retriever.GetSiteConfig());
			mockRetriever.Verify(r => r.Execute(), Times.Exactly(4));
		}

		public void TestGetSiteConfigWithEndPointIsNull()
		{
			var site = testManager.AddWebSite((string name) => Directory.CreateDirectory(Path.Combine(rootDirectory, name)).FullName);
			var appName = "/SomeApplicationForTest";
			var path = Directory.CreateDirectory(Path.Combine(WebSiteTestManager.GetPhysicalPath(site), appName.Substring(1))).FullName;
			var app = site.Applications.Add(appName, path);
			site.Bindings.Clear();
			site.Bindings.Add(string.Format(CultureInfo.InvariantCulture, "*:443:{0}", site), "test");
			testManager.ServerManager.CommitChanges();

			var siteConfig = new SiteConfigRetriever(site.Id, appName, IPAddress.Parse("0.0.0.0"), new Uri("http://test.com"), false);
			AssertNoExceptionThrown(() => siteConfig.GetSiteConfig());
		}

		public void TestGetSiteConfigCorrectIpAndScheme()
		{
			var site = testManager.AddWebSite((string name) => Directory.CreateDirectory(Path.Combine(rootDirectory, name)).FullName);

			const string appName = "/SomeApplicationForTest";
			var path = Directory.CreateDirectory(Path.Combine(WebSiteTestManager.GetPhysicalPath(site), appName.Substring(1))).FullName;
			var app = site.Applications.Add(appName, path);

			site.Bindings.Clear();
			site.Bindings.Add("10.20.30.40:441:test1.com", "https");
			site.Bindings.Add("10.20.30.50:442:test2.com", "http");
			site.Bindings.Add("*:443:test3.com", "https");
			site.Bindings.Add("10.20.30.50:444:test4.com", "https");

			testManager.ServerManager.CommitChanges();

			var siteConfig = new SiteConfigRetriever(site.Id, appName, IPAddress.Parse("10.20.30.50"), new Uri("https://test.com"), false);
			var (protocol, port, hostName) = siteConfig.GetSiteConfig();

			AssertEquals("https", protocol);
			AssertEquals(444, port);
			AssertEquals("test4.com", hostName);

			siteConfig = new SiteConfigRetriever(site.Id, appName, IPAddress.Parse("10.20.30.50"), new Uri("http://test.com"), false);
			(protocol, port, hostName) = siteConfig.GetSiteConfig();

			AssertEquals("http", protocol);
			AssertEquals(442, port);
			AssertEquals("test2.com", hostName);

			siteConfig = new SiteConfigRetriever(site.Id, appName, IPAddress.Parse("10.20.30.60"), new Uri("https://test.com"), false);
			(protocol, port, hostName) = siteConfig.GetSiteConfig();

			AssertEquals("https", protocol);
			AssertEquals(443, port);
			AssertEquals("test3.com", hostName);
		}

		protected override void SetUp()
		{
			base.SetUp();
			testManager = new WebSiteTestManager(HtmlFail);
			rootDirectory = Path.Combine(TempForTest.TempPath, Guid.NewGuid().ToString());
			Directory.CreateDirectory(rootDirectory);
		}

		protected override void TearDown()
		{
			base.TearDown();
			testManager.Dispose();
			WebSiteTestManager.DeleteApplicationRootDirectory(rootDirectory);
		}

		string rootDirectory;
		WebSiteTestManager testManager;
	}
}
