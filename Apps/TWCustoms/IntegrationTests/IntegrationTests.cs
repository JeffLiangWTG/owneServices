using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Xml.Linq;
using CargoWise.eHub.Products.TWCustoms.IntegrationTests.Helpers;
using CargoWise.eHub.Products.TWCustoms.TWCustomsGateway;
using CargoWise.eHub.Products.TWCustoms.TWCustomsGatewayAdapter;
using CargoWise.eHub.Products.TWCustoms.TWCustomsGatewayAdapter.Data;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Moq;
using NUnit.Framework;
using Serilog;

namespace CargoWise.eHub.Products.TWCustoms.IntegrationTests
{
	[TestFixture]
	public class IntegrationTests : TestBase
	{
		private Mock<PluginManager> pluginManagerMock;
		private Mock<IFileManager> fileManagerMock;
		private HttpClient twCustomsGatewayTestClient;
		private ILogger logger;
		private Mock<IHttpClient> httpClientMock;
		private const string pluginFolder = "K:\\Plugin";

		private readonly static string certificateBase64 = GetResourceAsString("TestFiles.CertificateBase64.txt");

		[SetUp]
		public void Initialize()
		{
			var binariesPath = Path.GetDirectoryName(GetType().Assembly.Location);
			httpClientMock = new Mock<IHttpClient>();
			logger = new LoggerConfiguration().MinimumLevel.Debug().WriteTo.Console().CreateLogger();
			fileManagerMock = new Mock<IFileManager>();
			pluginManagerMock = new Mock<PluginManager>(fileManagerMock.Object, logger);
			fileManagerMock.Setup(x => x.DirectoryExists(pluginFolder)).Returns(true);
			fileManagerMock.Setup(x => x.FileExists("K:\\Plugin\\PluginHandler.bat")).Returns(true);
			var builder = WebHost.CreateDefaultBuilder()
				.UseStartup<Startup>()
				.UseContentRoot(binariesPath)
				.UseEnvironment("UnitTest")
				.ConfigureServices(services =>
				{
					services.TryAddSingleton(fileManagerMock.Object);
					services.TryAddSingleton(httpClientMock.Object);
					services.TryAddSingleton<IPluginManager>(pluginManagerMock.Object);
				});
			var server = new TestServer(builder);
			twCustomsGatewayTestClient = server.CreateClient();
		}

		[Test]
		public void TestGatewayHealthCheck()
		{
			using var sw = new StringWriter();
			Console.SetOut(sw);
			var response = twCustomsGatewayTestClient.GetAsync("/wtg/status").Result;
			Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
			Assert.AreEqual("text/plain", response.Content.Headers.ContentType.MediaType);
			Assert.AreEqual("utf-8", response.Content.Headers.ContentType.CharSet);
			Assert.AreEqual("INFO(TWCustomsGateway): Service is alive.", response.Content.ReadAsStringAsync().Result);
		}

		[TestCase(@"\HYETST\TST\BRK", "SendMessage.xml", "PluginSendConfiguration.txt", "HYETST_TST_BRK_TBK0461-A_Certificate.pfx", "HYETST_TST_TBK0461_BRK_Send_TVA.picfg")]
		[TestCase(@"\HYETST\TST", "SendMessage_ForwarderManifest.xml", "PluginSendConfiguration_ForwarderManifest.txt", "HYETST_TST_TBK0461-A_Certificate.pfx", "HYETST_TST_TBK0461_Send_TVF.picfg")]
		[TestCase(@"\WTLCTU\DTW", "SendMessage_NXM.xml", "PluginSendConfiguration_NXM.txt", "WTLCTU_DTW_TBK0461-A_Certificate.pfx", @"WTLCTU_DTW_TBK0461_Send_NXM.picfg")]
		public void TestSampleSendMessage(string combinedPath, string messageXmlFileName, string cfgContentFileName, string certFileName, string cfgFileName)
		{
			var message = GetResourceAsString($"TestFiles.{messageXmlFileName}");
			var messageContent = new StringContent(message, Encoding.UTF8, "application/xml");
			var configurationContent = GetResourceAsString($"TestFiles.{cfgContentFileName}");
			var messageBodyXml = GetEmbeddedResource("TestFiles.SendMessage_Body_XML.xml");
			fileManagerMock.Setup(x => x.FileExists(It.IsAny<string>())).Returns(false);
			fileManagerMock.Setup(x => x.CreateFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Callback<string, string, bool>(
				(filePath, content, isBase64String) =>
				{
					if (filePath.EndsWith(".xml"))
					{
						var actual = XDocument.Load(new MemoryStream(Convert.FromBase64String(content)));
						var expected = XDocument.Load(messageBodyXml);
						Assert.AreEqual(actual.ToString(), expected.ToString());
					}
				});
			pluginManagerMock.Setup(x => x.StartProcess(It.IsAny<PluginInfo>())).Returns(string.Empty);

			using (var sw = new StringWriter())
			{
				Console.SetOut(sw);

				var response = twCustomsGatewayTestClient.PostAsync("/SendMessage", messageContent).Result;
				var responseContent = response.Content.ReadAsStringAsync().Result;
				var pluginResponse = Serialize.FromXML<TWCustomsGatewayResponse>(responseContent);

				Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
				Assert.AreEqual(TWCustomsGatewayResponse.OK.ResponseMessage, pluginResponse.ResponseMessage);
				Assert.AreEqual(TWCustomsGatewayResponse.OK.ResponseCode, pluginResponse.ResponseCode);
				StringAssert.Contains($@"Create certificate file: K:\Plugin\cert{combinedPath}\{certFileName}", sw.ToString());
				StringAssert.Contains($@"Create config file: K:\Plugin\conf{combinedPath}\{cfgFileName}", sw.ToString());
			}
			fileManagerMock.Verify(x => x.CreateFile(Path.Combine(pluginFolder, $@"conf{combinedPath}", cfgFileName), configurationContent, false), Times.Once);
			fileManagerMock.Verify(x => x.CreateFile(Path.Combine(pluginFolder, $@"cert{combinedPath}", certFileName), certificateBase64, true), Times.Once);
			fileManagerMock.Verify(x => x.CreateFile(Path.Combine(pluginFolder, $@"data\SendSrc{combinedPath}\TBK0461\Attach", "N5203.97162640001206150001.1.jpg"), It.IsAny<string>(), true), Times.Once);
			fileManagerMock.Verify(x => x.CreateFile(Path.Combine(pluginFolder, $@"data\SendSrc{combinedPath}\TBK0461\Attach", "N5203.97162640001206150001.2.jpg"), It.IsAny<string>(), true), Times.Once);
		}

		[TestCase(@"\HYETST\TST\BRK", "SendMessage_CAA.xml", "PluginSendConfiguration_CAA.txt", "HYETST_TST_BRK_TBK0461-A_Certificate.pfx", "HYETST_TST_TBK0461_BRK_Send_TVA_CAA.picfg")]
		public void TestSampleSendMessage_CAA(string combinedPath, string messageXmlFileName, string cfgContentFileName, string certFileName, string cfgFileName)
		{
			var message = GetResourceAsString($"TestFiles.{messageXmlFileName}");
			var messageContent = new StringContent(message, Encoding.UTF8, "application/xml");
			var configurationContent = GetResourceAsString($"TestFiles.{cfgContentFileName}");
			var messageBodyXml = GetEmbeddedResource("TestFiles.SendMessage_Body_XML.xml");
			fileManagerMock.Setup(x => x.FileExists(It.IsAny<string>())).Returns(false);
			fileManagerMock.Setup(x => x.CreateFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Callback<string, string, bool>(
				(filePath, content, isBase64String) =>
				{
					if (filePath.EndsWith(".xml"))
					{
						var actual = XDocument.Load(new MemoryStream(Convert.FromBase64String(content)));
						var expected = XDocument.Load(messageBodyXml);
						Assert.AreEqual(actual.ToString(), expected.ToString());
					}
				});
			pluginManagerMock.Setup(x => x.StartProcess(It.IsAny<PluginInfo>())).Returns(string.Empty);

			using (var sw = new StringWriter())
			{
				Console.SetOut(sw);

				var response = twCustomsGatewayTestClient.PostAsync("/SendMessage", messageContent).Result;
				var responseContent = response.Content.ReadAsStringAsync().Result;
				var pluginResponse = Serialize.FromXML<TWCustomsGatewayResponse>(responseContent);

				Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
				Assert.AreEqual(TWCustomsGatewayResponse.OK.ResponseMessage, pluginResponse.ResponseMessage);
				Assert.AreEqual(TWCustomsGatewayResponse.OK.ResponseCode, pluginResponse.ResponseCode);
				StringAssert.Contains($@"Create certificate file: K:\Plugin\cert{combinedPath}\{certFileName}", sw.ToString());
				StringAssert.Contains($@"Create config file: K:\Plugin\conf{combinedPath}\{cfgFileName}", sw.ToString());
			}
			fileManagerMock.Verify(x => x.CreateFile(Path.Combine(pluginFolder, $@"conf{combinedPath}", cfgFileName), configurationContent, false), Times.Once);
			fileManagerMock.Verify(x => x.CreateFile(Path.Combine(pluginFolder, $@"cert{combinedPath}", certFileName), certificateBase64, true), Times.Once);
			fileManagerMock.Verify(x => x.CreateFile(Path.Combine(pluginFolder, $@"data\SendSrc{combinedPath}\TBK0461_CAA\Attach", "N5203.97162640001206150001.1.jpg"), It.IsAny<string>(), true), Times.Once);
			fileManagerMock.Verify(x => x.CreateFile(Path.Combine(pluginFolder, $@"data\SendSrc{combinedPath}\TBK0461_CAA\Attach", "N5203.97162640001206150001.2.jpg"), It.IsAny<string>(), true), Times.Once);
		}

		[Test]
		public void TestSampleSendMessageError()
		{
			fileManagerMock.Setup(x => x.FileExists(It.IsAny<string>())).Returns(false);
			fileManagerMock.Setup(x => x.CreateFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()));
			pluginManagerMock.Setup(x => x.StartProcess(It.IsAny<PluginInfo>())).Returns(
				@"20200203 13:06:51^PL^I0000^Begin PlugInHandler, version: 01.18.01^
20200203 13:06:51^PL^I0001^Read Config File: conf\TBK0461_S_TVA.picfg Sucess^
20200203 13:06:51^PL^I0009^Begin process: TBK0461_S_TVA.picfg, version: 01.18.01^
20200203 13:06:52^PL^I0011^Send, Send finish: .\data\SendSrc\TBK0461 no files^
20200203 13:06:53^PL^E0022^Send, Send file by tymcomm+J fail! return code :12, please refer to tymcomm's log^
20200203 13:06:53^PL^I0010^End process: TBK0461_S_TVA.picfg^");

			var message = GetResourceAsString("TestFiles.SendMessage.xml");
			var content = new StringContent(message, Encoding.UTF8, "application/xml");

			using (var sw = new StringWriter())
			{
				Console.SetOut(sw);

				var response = twCustomsGatewayTestClient.PostAsync("/SendMessage", content).Result;
				var responseContent = response.Content.ReadAsStringAsync().Result;
				var pluginResponse = Serialize.FromXML<TWCustomsGatewayResponse>(responseContent);

				Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
				Assert.AreEqual("Send file by tymcomm+J fail! return code :12, please refer to tymcomm's log", pluginResponse.ResponseMessage);
				Assert.AreEqual("E0022", pluginResponse.ResponseCode);
			}
		}

		[Test]
		public void TestSampleSendMessageException()
		{
			fileManagerMock.Setup(x => x.FileExists(It.IsAny<string>())).Returns(false);
			fileManagerMock.Setup(x => x.CreateFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()));
			pluginManagerMock.Setup(x => x.StartProcess(It.IsAny<PluginInfo>())).Throws(new Exception("Process throws an exception."));

			var message = GetResourceAsString("TestFiles.SendMessage.xml");
			var content = new StringContent(message, Encoding.UTF8, "application/xml");

			using (var sw = new StringWriter())
			{
				Console.SetOut(sw);
				var response = twCustomsGatewayTestClient.PostAsync("/SendMessage", content).Result;
				var responseContent = response.Content.ReadAsStringAsync().Result;
				var pluginResponse = Serialize.FromXML<TWCustomsGatewayResponse>(responseContent);

				Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
				Assert.AreEqual("-1", pluginResponse.ResponseCode);
				Assert.IsTrue(pluginResponse.ResponseMessage.Contains("Process throws an exception."));

			}
		}

		[TestCase(@"\HYETST\TST\BRK", "ReceiveMessage.xml", "PluginReceiveConfiguration.txt", "HYETST_TST_BRK_TBK0461-A_Certificate.pfx", "HYETST_TST_TBK0461_BRK_Receive_UVC.picfg")]
		public void TestSampleReceiveMessage(string combinedPath, string messageXmlFileName, string cfgContentFileName, string certFileName, string cfgFileName)
		{
			var message = GetResourceAsString($"TestFiles.{messageXmlFileName}");
			var configurationContent = GetResourceAsString($"TestFiles.{cfgContentFileName}");
			var content = new StringContent(message, Encoding.UTF8, "application/xml");
			fileManagerMock.Setup(x => x.FileExists(It.IsAny<string>())).Returns(false);
			fileManagerMock.Setup(x => x.CreateFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()));
			fileManagerMock.Setup(x => x.GetTWCustomsResponseFiles(It.IsAny<string>())).Returns(new[]
			{
				new FileInfo("File1"),
				new FileInfo("File2"),
				new FileInfo("File3")
			});
			fileManagerMock.Setup(x => x.GetText(It.IsAny<string>())).Returns("<Response />");
			fileManagerMock.Setup(x => x.Delete(It.IsAny<string>()));
			pluginManagerMock.Setup(x => x.StartProcess(It.IsAny<PluginInfo>())).Returns(string.Empty);
			httpClientMock.Setup(x => x.PostAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(new HttpResponseMessage(HttpStatusCode.OK));

			using (var sw = new StringWriter())
			{
				Console.SetOut(sw);

				var response = twCustomsGatewayTestClient.PostAsync("/ReceiveMessage", content).Result;
				var responseContent = response.Content.ReadAsStringAsync().Result;
				var pluginResponse = Serialize.FromXML<TWCustomsGatewayResponse>(responseContent);

				Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
				Assert.AreEqual(TWCustomsGatewayResponse.OK.ResponseMessage, pluginResponse.ResponseMessage);
				Assert.AreEqual(TWCustomsGatewayResponse.OK.ResponseCode, pluginResponse.ResponseCode);
				StringAssert.Contains($@"Create certificate file: K:\Plugin\cert{combinedPath}\{certFileName}", sw.ToString());
				StringAssert.Contains($@"Create config file: K:\Plugin\conf{combinedPath}\{cfgFileName}", sw.ToString());
			}
			fileManagerMock.Verify(x => x.CreateFile(Path.Combine(pluginFolder, $@"conf{combinedPath}", cfgFileName), configurationContent, false), Times.Once);
			fileManagerMock.Verify(x => x.CreateFile(Path.Combine(pluginFolder, $@"cert{combinedPath}", certFileName), certificateBase64, true), Times.Once);
			fileManagerMock.Verify(x => x.Delete(It.IsAny<string>()), Times.Exactly(3));
			fileManagerMock.Verify(x => x.GetText(It.IsAny<string>()), Times.Exactly(3));
			httpClientMock.Verify(x => x.PostAsync("http://Test-Server/TestService.svc", It.IsAny<string>()), Times.Exactly(3));
		}

		[Test]
		public void TestSampleReceiveMessageError()
		{
			fileManagerMock.Setup(x => x.FileExists(It.IsAny<string>())).Returns(false);
			fileManagerMock.Setup(x => x.CreateFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()));
			pluginManagerMock.Setup(x => x.StartProcess(It.IsAny<PluginInfo>())).Returns(@"20200206 11:12:02^PL^I0000^Begin PlugInHandler, version: 01.18.01^
20200206 11:12:02^PL^I0001^Read Config File: conf\TBK0461_R_TVA.picfg Sucess^
20200206 11:12:02^PL^I0009^Begin process: TBK0461_R_TVA.picfg, version: 01.18.01^
20200206 11:12:03^PL^E0023^Receive, recv file by tymcomm+J fail! return code :12, please refer to tymcomm's log^
20200206 11:12:03^PL^I0012^Receive, Receive finish: TBK0461_R_TVA.picfg, success: -1 records^
20200206 11:12:04^PL^I0010^End process: TBK0461_R_TVA.picfg^");

			var message = GetResourceAsString("TestFiles.ReceiveMessage.xml");
			var content = new StringContent(message, Encoding.UTF8, "application/xml");

			using (var sw = new StringWriter())
			{
				Console.SetOut(sw);

				var response = twCustomsGatewayTestClient.PostAsync("/ReceiveMessage", content).Result;
				var responseContent = response.Content.ReadAsStringAsync().Result;
				var pluginResponse = Serialize.FromXML<TWCustomsGatewayResponse>(responseContent);

				Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
				Assert.AreEqual("recv file by tymcomm+J fail! return code :12, please refer to tymcomm's log", pluginResponse.ResponseMessage);
				Assert.AreEqual("E0023", pluginResponse.ResponseCode);
			}
		}

		[Test]
		public void TestSampleReceiveMessageException()
		{
			fileManagerMock.Setup(x => x.FileExists(It.IsAny<string>())).Returns(false);
			fileManagerMock.Setup(x => x.CreateFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()));
			pluginManagerMock.Setup(x => x.StartProcess(It.IsAny<PluginInfo>())).Throws(new Exception("Process throws an exception."));

			var message = GetResourceAsString("TestFiles.ReceiveMessage.xml");
			var content = new StringContent(message, Encoding.UTF8, "application/xml");

			using (var sw = new StringWriter())
			{
				Console.SetOut(sw);
				var response = twCustomsGatewayTestClient.PostAsync("/ReceiveMessage", content).Result;
				var responseContent = response.Content.ReadAsStringAsync().Result;
				var pluginResponse = Serialize.FromXML<TWCustomsGatewayResponse>(responseContent);

				Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
				Assert.AreEqual("-1", pluginResponse.ResponseCode);
				Assert.IsTrue(pluginResponse.ResponseMessage.Contains("Process throws an exception."));
			}
		}
	}
}