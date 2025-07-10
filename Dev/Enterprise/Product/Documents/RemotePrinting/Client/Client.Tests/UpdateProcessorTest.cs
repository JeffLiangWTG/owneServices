using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using CargoWise.IO;
using Moq;
using NUnit.Framework;

namespace Enterprise.RemotePrinting.Client.Tests
{
	public class UpdateProcessorTest : TestCase
	{
		public void TestDownloadWithWebServiceUrlScheme()
		{
			var filePath = Temp.GetTempFileName();
			var webConfig1 = new WebClientConfiguration("https://server.com", "user1", "pwd1", 1, "machine1",
				true, "proxy1", 43, "proxyuser1", "proxypwd1", false,
				false, 5000, false, 0, 0, 100, 123, false, false, 0, 0, 0, false,
				0, 0, false, 0,
				ConnectionRegistryManagerTest.GetWebClientUpdateConfigurationForTest(), false, 100);

			const string ConfigName = "configForTest1";
			var registryManager = new ConnectionRegistryManagerTest.ConnectionRegistryManagerForTest();

			registryManager.LoadFromRegistry(ConfigName);
			registryManager.SaveRemotePrintingRegistryValues(webConfig1);
			try
			{
				var logs = new StringBuilder();
				var configProvider = new UpdateConfigurationProvider(registryManager, ConfigName, "machine1", null);
				var updateProcessor = new UpdateProcessorForDownloadingTest(new ClientUpdateForTest() { Link = "http://server.com/testfile" }, configProvider, (message) => logs.AppendLine(message));

				var mockHttpWebResponse = new Mock<HttpWebResponse>();
				mockHttpWebResponse.Setup(o => o.StatusCode).Returns(HttpStatusCode.OK);
				mockHttpWebResponse.Setup(o => o.ToString()).Returns("mockHttpWebResponse");
				mockHttpWebResponse.Setup(o => o.GetResponseStream()).Returns(new MemoryStream(Encoding.UTF8.GetBytes("123")));

				var mockRequest = new Mock<WebRequest>();
				var callCount = 0;
				mockRequest.Setup(r => r.GetResponse())
					.Returns(mockHttpWebResponse.Object)
					.Callback(() =>
					{
						callCount++;
						if (callCount == 1)
						{
							throw new InvalidOperationException("Jerry Test Error Message");
						}
					});
				updateProcessor.RequestForTest = mockRequest.Object;
				updateProcessor.Download_Exposed(filePath);

				var expectedLogs = @"Download update file failed: Jerry Test Error Message
Retry download update file with Url: https://server.com:443/testfile
";
				AssertEquals(expectedLogs, logs.ToString());
			}
			finally
			{
				if (File.Exists(filePath))
				{
					File.Delete(filePath);
				}
				registryManager.DeleteFromRegistry(ConfigName);
			}
		}

		public void TestCredentialsWithProtectedPassword()
		{
			var protectedPwd = ProtectedDataHelper.Protect("pwd1");
			var webConfig1 = new WebClientConfiguration("url1", "user1", protectedPwd, 1, "machine1",
				true, "proxy1", 43, "proxyuser1", "proxypwd1", false,
				false, 5000, false, 0, 0, 100, 123, false, false, 0, 0, 0, false,
				0, 0, false, 0,
				ConnectionRegistryManagerTest.GetWebClientUpdateConfigurationForTest(), false, 100);

			const string ConfigName = "configForTest1";
			var registryManager = new ConnectionRegistryManagerTest.ConnectionRegistryManagerForTest();

			registryManager.LoadFromRegistry(ConfigName);
			registryManager.SaveRemotePrintingRegistryValues(webConfig1);
			try
			{
				var configProvider = new UpdateConfigurationProvider(registryManager, ConfigName, "machine1", null);
				var updateProcessor = new UpdateProcessorForTest(new ClientUpdateForTest(), configProvider);

				var webRequest = updateProcessor.NewWebRequest_Exposed("http://some.where/file.ext");

				var password = ((NetworkCredential)webRequest.Credentials).Password;

				CombineAssertions(() =>
				{
					AssertNotEquals("pwd1", protectedPwd);
					AssertEquals("pwd1", password);
				});
			}
			finally
			{
				registryManager.DeleteFromRegistry(ConfigName);
			}
		}

		public void TestIsUpdateRequired_NoInstalledVersion()
		{
			var clientUpdate = new ClientUpdateForTest
			{
				Version = "0.0.0",
				Link = "http://server/file"
			};

			using (UpdateProcessor.OverrideInstalledVersionForTest(""))
			{
				var updateProcessor = new UpdateProcessor(clientUpdate);

				Assert("Should not require update", !updateProcessor.IsUpdateRequired());
				AssertEquals(1, DirectoryPathForTest.GetFiles().Length);

				var fileName = DirectoryPathForTest.GetFiles("LogFileForTest.txt").First().FullName;
				using (var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
				using (var stream = new StreamReader(fileStream))
				{
					var log = stream.ReadToEnd();
					AssertContains("Cannot get client installed version", log);
				}
			}
		}

		public void TestIsUpdateRequired_Yes()
		{
			var clientUpdate = new ClientUpdateForTest
			{
				Version = "99.99.99",
				Link = "http://server/file"
			};

			using (UpdateProcessor.OverrideInstalledVersionForTest("1.0.0"))
			{
				var updateProcessor = new UpdateProcessor(clientUpdate);
				Assert("Should require update to high", updateProcessor.IsUpdateRequired());
			}
		}

		public void TestIsUpdateRequired_No()
		{
			var clientUpdate = new ClientUpdateForTest
			{
				Version = "1.1.1",
				Link = "http://server/file"
			};

			var emailLog = new StringBuilder();

			var mock = new Mock<IUpdateConfigurationProvider>();
			mock.Setup(i => i.MachineName).Returns("MachineName-Jerry");
			mock.Setup(i => i.SendNotification(It.IsAny<string>()))
				.Callback(new Action<string>((message) =>
				{
					emailLog.Append(message);
				}));

			var registryManager = new ConnectionRegistryManagerTest.ConnectionRegistryManagerForTest();
			var configName = "configForTest3";

			registryManager.LoadFromRegistry(configName);

			try
			{
				using (UpdateProcessor.OverrideInstalledVersionForTest("1.1.2"))
				using (UpdateProcessor.OverrideConnectionRegistryManagerForTest(registryManager))
				{
					var updateProcessor = new UpdateProcessor(clientUpdate, mock.Object);
					var expectedEmailMessage = @"There is a newer version of the WebPrint Client that is installed on the MachineName-Jerry.
The version of the WebPrint Client that is installed is 1.1.2
The version of the WebPrint Client that is available on the Remote server is 1.1.1

Please follow these steps to update the WebPrint Client to the version on the Remote server:
1. Uninstall the current version of the WebPrint Client on the MachineName-Jerry.
2. Download the latest WebPrint Client from http://server/file to the CargoWiseOneWebPrintClientIntSetup.msi.
3. Install the WebPrint Client on the MachineName-Jerry.
4. Start the WebPrint Client.
";

					Assert("Should not require update to low version", !updateProcessor.IsUpdateRequired());
					AssertEquals(1, DirectoryPathForTest.EnumerateFiles().Count());

				var fileName = DirectoryPathForTest.GetFiles("LogFileForTest.txt").First().FullName;
				using (var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
				using (var stream = new StreamReader(fileStream))
				{
					var log = stream.ReadToEnd();
					AssertContains("WebPrint Client appears to have newer version than WebPrint Server.", log);
				}

					AssertEquals(expectedEmailMessage, emailLog.ToString());
				}
			}
			finally
			{
				registryManager.DeleteFromRegistry(configName);
			}
		}

		public void TestIsUpdateRequired_ErrorMessage()
		{
			var clientUpdate = new ClientUpdateForTest
			{
				Version = "0.0.0",
				Link = "Some message"
			};

			using (UpdateProcessor.OverrideInstalledVersionForTest("1.0.0"))
			{
				var updateProcessor = new UpdateProcessor(clientUpdate);

				Assert("Should not require update to version 0.0", !updateProcessor.IsUpdateRequired());

				var fileName = DirectoryPathForTest.GetFiles("LogFileForTest.txt").First().FullName;
				using (var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
				using (var stream = new StreamReader(fileStream))
				{
					var log = stream.ReadToEnd();
					AssertContains("Remote Printing Server did not provide Client update package with error message:", log);
					AssertContains("Some message", log);
				}
			}
		}

		public void TestVersionResourceIsEmbedded()
		{
			var version = UpdateProcessorForTest.ReadResource_Exposed();
			AssertNotNullOrEmpty("Embedded Version Resource", version);
		}

		public void TestGetInstalledVersion()
		{
			var embeddedVersion = UpdateProcessorForTest.ReadResource_Exposed();
			AssertEquals(embeddedVersion, UpdateProcessor.GetInstalledVersion());
		}

		public void TestNewWebRequest_Authentication()
		{
			var webConfig1 = new WebClientConfiguration("url1", "user1", "pwd1", 1, "machine1",
				true, "proxy1", 43, "proxyuser1", "proxypwd1", false,
				false, 5000, false, 0, 0, 100, 123, false, false, 0, 0, 0, false,
				0, 0, false, 0,
				ConnectionRegistryManagerTest.GetWebClientUpdateConfigurationForTest(), false, 100);

			const string ConfigName = "configForTest1";
			var registryManager = new ConnectionRegistryManagerTest.ConnectionRegistryManagerForTest();

			registryManager.LoadFromRegistry(ConfigName);
			registryManager.SaveRemotePrintingRegistryValues(webConfig1);
			try
			{
				var configProvider = new UpdateConfigurationProvider(registryManager, ConfigName, "machine1", null);
				var updateProcessor = new UpdateProcessorForTest(new ClientUpdateForTest(), configProvider);

				var webRequest = updateProcessor.NewWebRequest_Exposed("http://some.where/file.ext");
				AssertEquals(false, ((HttpWebRequest)webRequest).AllowAutoRedirect);

				AssertNotNull(webRequest.Credentials);

				var networkCredentials = webRequest.Credentials as NetworkCredential;
				AssertNotNull(networkCredentials);
				AssertEquals("user1", networkCredentials.UserName);
				AssertEquals("pwd1", networkCredentials.Password);
			}
			finally
			{
				registryManager.DeleteFromRegistry(ConfigName);
			}
		}

		public void TestNewWebRequest_Proxy()
		{
			var webConfig1 = new WebClientConfiguration("url1", "user1", "pwd1", 1, "machine1",
				true, "proxy1", 43, "proxyuser1", "proxypwd1", false,
				false, 5000, false, 0, 0, 100, 123, false, false, 0, 0, 0, false,
				0, 0, false, 0,
				ConnectionRegistryManagerTest.GetWebClientUpdateConfigurationForTest(), false, 100);

			const string ConfigName = "configForTest1";
			var registryManager = new ConnectionRegistryManagerTest.ConnectionRegistryManagerForTest();

			registryManager.LoadFromRegistry(ConfigName);
			registryManager.SaveRemotePrintingRegistryValues(webConfig1);
			try
			{
				var configProvider = new UpdateConfigurationProvider(registryManager, ConfigName, "machine1", null);
				var updateProcessor = new UpdateProcessorForTest(new ClientUpdateForTest(), configProvider);

				var webRequest = updateProcessor.NewWebRequest_Exposed("http://some.where/file.ext");
				AssertEquals(false, ((HttpWebRequest)webRequest).AllowAutoRedirect);

				AssertNotNull(webRequest.Proxy);
				var webProxy = webRequest.Proxy as WebProxy;
				AssertNotNull(webProxy);

				AssertEquals("proxy1", webProxy.Address.Host);
				AssertEquals(43, webProxy.Address.Port);

				var networkCredentials = webProxy.Credentials as NetworkCredential;
				AssertNotNull(networkCredentials);
				AssertEquals("proxyuser1", networkCredentials.UserName);
				AssertEquals("proxypwd1", networkCredentials.Password);
			}
			finally
			{
				registryManager.DeleteFromRegistry(ConfigName);
			}
		}

		#region Set Up

		protected override void SetUp()
		{
			base.SetUp();

			var fileTarget = LogWriter.RegisterFileTarget(DirectoryPathForTest.ToString(), "LogForTest", WebPrintEventLogEntryType.Information, WebPrintEventLogEntryType.Error, "*", 500);
			fileTarget.FileName = new FileInfo(Path.Combine(DirectoryPathForTest.ToString(), "LogFileForTest.txt")).FullName;
			fileTarget.MaxArchiveFiles = 2;
		}

		readonly DirectoryInfo DirectoryPathForTest = new DirectoryInfo(Path.Combine(Constants.WebPrintClientDataPath, "LogsForTest"));

		protected override void TearDown()
		{
			LogWriter.UnregisterAllLogTargets();
			if (DirectoryPathForTest.Exists)
			{
				foreach (var tempfile in DirectoryPathForTest.EnumerateFiles())
				{
					File.Delete(tempfile.FullName);
				}
				DirectoryPathForTest.Delete();
			}
			base.TearDown();
		}

		#endregion
	}

	class UpdateProcessorForTest : UpdateProcessor
	{
		public UpdateProcessorForTest(IClientUpdate update, IUpdateConfigurationProvider updateConfigurationProvider = null, IErrorResponseWebRequestProcessor responseProcessor = null)
			: base(update, updateConfigurationProvider, responseProcessor: responseProcessor)
		{
		}

		public WebRequest NewWebRequest_Exposed(string requestUriString) => NewWebRequest(requestUriString);

		public static string ReadResource_Exposed() => ReadResource(VersionResourceName);
	}

	class UpdateProcessorForDownloadingTest : UpdateProcessor
	{
		public UpdateProcessorForDownloadingTest(IClientUpdate update, IUpdateConfigurationProvider updateConfigurationProvider, Action<string> onShowInformation)
			: base(update, updateConfigurationProvider, responseProcessor: null, onShowInformation: onShowInformation)
		{
		}

		public WebRequest RequestForTest { get; set; }

		public void Download_Exposed(string localFile) => Download(localFile);

		protected override WebRequest NewWebRequest(string requestUriString) => RequestForTest;
	}

	class ClientUpdateForTest : IClientUpdate
	{
		public string Version { get; set; }

		public string Link { get; set; }
	}
}
