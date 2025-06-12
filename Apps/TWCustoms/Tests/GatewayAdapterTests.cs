using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using CargoWise.eHub.Products.TWCustoms.TWCustomsGatewayAdapter;
using CargoWise.eHub.Products.TWCustoms.TWCustomsGatewayAdapter.Data;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;
using Serilog;

namespace CargoWise.eHub.Products.TWCustoms.Tests
{
	[TestFixture]
	public class GatewayAdapterTests : TestBase
	{
		private Mock<PluginManager> pluginManageMock;
		private Mock<IFileManager> fileManagerMock;
		private IConfiguration configuration;
		private ILogger logger;
		private Mock<IHttpClient> httpClientMock;
		private GatewayAdapter adapter;

		private readonly static string certificateBase64 = GetResourceAsString("TestFiles.CertificateBase64.txt");
		
		[SetUp]
		public void Setup()
		{
			fileManagerMock = new Mock<IFileManager>(MockBehavior.Strict);
			 configuration = new ConfigurationBuilder()
				.AddJsonFile("appsettings.json")
				.AddJsonFile("appsettings.UnitTest.json")
				.Build();
			logger = new LoggerConfiguration().MinimumLevel.Debug().WriteTo.Console().CreateLogger();
			httpClientMock = new Mock<IHttpClient>();
			pluginManageMock = new Mock<PluginManager>(fileManagerMock.Object, logger);
			adapter = new GatewayAdapter(configuration, pluginManageMock.Object, fileManagerMock.Object, httpClientMock.Object, logger);
			fileManagerMock.Setup(x => x.DirectoryExists("K:\\Plugin")).Returns(true);
			fileManagerMock.Setup(x => x.FileExists("K:\\Plugin\\PluginHandler.bat")).Returns(true);
		}

		#region TestMethods
		[TestCaseSource(typeof(Source), nameof(Source.SourceOfSend))]
		public void GatewaySendMessageTest_TV_Test(TWCustomsGatewaySendRequest sendRequest, Source.ExpectedData expectedData)
		{
			fileManagerMock.Setup(x => x.FileExists(It.IsAny<string>())).Returns(false);
			fileManagerMock.Setup(x => x.CreateFile(expectedData.CertFileName, It.IsAny<string>(), true)).Callback<string, string, bool>(
				(filePath, content, isBase64) =>
				{
					Assert.AreEqual(certificateBase64, content);
				});
			fileManagerMock.Setup(x => x.CreateFile(expectedData.CfgFileName, It.IsAny<string>(), false)).Callback<string, string, bool>(
				(filePath, content, isBase64) =>
				{
					Assert.AreEqual(expectedData.ConfigurationContent, content);
				});
			fileManagerMock.Setup(x => x.CreateFile(expectedData.XmlFileName, It.IsAny<string>(), true)).Callback<string, string, bool>(
				(filePath, content, isBase64) =>
				{
					Assert.AreEqual(@"<SampleMessage/>", Encoding.UTF8.GetString(Convert.FromBase64String(content)));
				});
			pluginManageMock.Setup(x => x.StartProcess(It.IsAny<PluginInfo>())).Returns(@"20200206 21:46:13^PL^I0000^Begin PlugInHandler, version: 01.18.01^
20200206 21:46:13^PL^I0001^Read Config File: C:\Plugin\conf\HYETST\TST\BRK\HYETST_TST_TBK0461_BRK_Send_TVA.picfg Sucess ^
20200206 21:46:13^PL^I0009^Begin process: HYETST_TST_TBK0461_BRK_Send_TVA.picfg, version: 01.18.01^
20200206 21:46:16^PL^I0005^Send, .\data\SendSrc\HYETST\TST\BRK\TBK0461\N5203.97162640001206150001.xml, trans charset success^
20200206 21:46:17^PL^I0008^Send, .\data\SendSrc\HYETST\TST\BRK\TBK0461\N5203.97162640001206150001.xml validate success^
20200206 21:46:17^PL^I0006^Send, N5203.97162640001206150001.xml is packed to N5203.20200206214617236.packed^
20200206 21:46:17^PL^I0014^Send, put N5203.20200206214617236.packed to .\data\SendTarget\HYETST\TST\BRK\TBK0461\N5203.20200206214617236.packed^
20200206 21:46:17^PL^I0020^Send, backup N5203.97162640001206150001.xml to C:\Plugin\.\data\SendBak\HYETST\TST\BRK\TBK0461\20200206\21\N5203.97162640001206150001.xml^
20200206 21:46:18^PL^I0011^Send, Send file by tymcomm+J finish! return code :0^
20200206 21:46:18^PL^I0011^Send, Send finish: TBK0461_S_TVA.picfg^
20200206 21:46:19^PL^I0010^End process: HYETST_TST_TBK0461_BRK_Send_TVA.picfg^");

			var result = adapter.SendMessage(sendRequest);

			Assert.AreEqual(null, result.ErrorCode);
			Assert.AreEqual(null, result.ErrorMessge);
			fileManagerMock.Verify(x => x.CreateFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Exactly(3));
		}

		[TestCaseSource(typeof(Source), nameof(Source.SourceOfSend))]
		public void GatewaySendMessageTest_TV_DuplicateFile_Test(TWCustomsGatewaySendRequest sendRequest, Source.ExpectedData expectedData)
		{
			fileManagerMock.Setup(x => x.FileExists(It.IsAny<string>())).Returns(false);
			fileManagerMock.Setup(x => x.CreateFile(expectedData.CertFileName, It.IsAny<string>(), true)).Callback<string, string, bool>(
				(filePath, content, isBase64) =>
				{
					Assert.AreEqual(certificateBase64, content);
				});
			fileManagerMock.Setup(x => x.CreateFile(expectedData.CfgFileName, It.IsAny<string>(), false)).Callback<string, string, bool>(
				(filePath, content, isBase64) =>
				{
					Assert.AreEqual(expectedData.ConfigurationContent, content);
				});
			fileManagerMock.Setup(x => x.FileExists(expectedData.XmlFileName)).Returns(true);
			fileManagerMock.Setup(x => x.CreateFile(expectedData.XmlFileName, It.IsAny<string>(), true)).Callback<string, string, bool>(
				(filePath, content, isBase64) =>
				{
					Assert.AreEqual(@"<SampleMessage/>", Encoding.UTF8.GetString(Convert.FromBase64String(content)));
				});
			pluginManageMock.Setup(x => x.StartProcess(It.IsAny<PluginInfo>())).Returns(@"20200206 21:46:13^PL^I0000^Begin PlugInHandler, version: 01.18.01^
20200206 21:46:13^PL^I0001^Read Config File: C:\Plugin\conf\HYETST\TST\BRK\HYETST_TST_TBK0461_BRK_Send_TVA.picfg Sucess ^
20200206 21:46:13^PL^I0009^Begin process: HYETST_TST_TBK0461_BRK_Send_TVA.picfg, version: 01.18.01^
20200206 21:46:16^PL^I0005^Send, .\data\SendSrc\HYETST\TST\BRK\TBK0461\N5203.97162640001206150001.xml, trans charset success^
20200206 21:46:17^PL^I0008^Send, .\data\SendSrc\HYETST\TST\BRK\TBK0461\N5203.97162640001206150001.xml validate success^
20200206 21:46:17^PL^I0006^Send, N5203.97162640001206150001.xml is packed to N5203.20200206214617236.packed^
20200206 21:46:17^PL^I0014^Send, put N5203.20200206214617236.packed to .\data\SendTarget\HYETST\TST\BRK\TBK0461\N5203.20200206214617236.packed^
20200206 21:46:17^PL^I0020^Send, backup N5203.97162640001206150001.xml to C:\Plugin\.\data\SendBak\HYETST\TST\BRK\TBK0461\20200206\21\N5203.97162640001206150001.xml^
20200206 21:46:18^PL^I0011^Send, Send file by tymcomm+J finish! return code :0^
20200206 21:46:18^PL^I0011^Send, Send finish: TBK0461_S_TVA.picfg^
20200206 21:46:19^PL^I0010^End process: HYETST_TST_TBK0461_BRK_Send_TVA.picfg^");

			var result = adapter.SendMessage(sendRequest);

			Assert.AreEqual(null, result.ErrorCode);
			Assert.AreEqual(null, result.ErrorMessge);
			fileManagerMock.Verify(x => x.CreateFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Exactly(2));
			fileManagerMock.Verify(x => x.FileExists(It.IsAny<string>()), Times.Exactly(3));
		}

		[TestCaseSource(typeof(Source), nameof(Source.SourceOfSend))]
		public void GatewaySendMessageTest_Error(TWCustomsGatewaySendRequest sendRequest, Source.ExpectedData expectedData)
		{
			fileManagerMock.Setup(x => x.FileExists(It.IsAny<string>())).Returns(false);
			fileManagerMock.Setup(x => x.CreateFile(expectedData.CertFileName, It.IsAny<string>(), true)).Callback<string, string, bool>(
				(filePath, content, isBase64) =>
				{
					Assert.AreEqual(certificateBase64, content);
				});
			fileManagerMock.Setup(x => x.CreateFile(expectedData.CfgFileName, It.IsAny<string>(), false)).Callback<string, string, bool>(
				(filePath, content, isBase64) =>
				{
					Assert.AreEqual(expectedData.ConfigurationContent, content);
				});
			fileManagerMock.Setup(x => x.CreateFile(expectedData.XmlFileName, It.IsAny<string>(), true)).Callback<string, string, bool>(
				(filePath, content, isBase64) =>
				{
					Assert.AreEqual(@"<SampleMessage/>", Encoding.UTF8.GetString(Convert.FromBase64String(content)));
				});
			pluginManageMock.Setup(x => x.StartProcess(It.IsAny<PluginInfo>())).Returns(@"20200203 13:06:51^PL^I0000^Begin PlugInHandler, version: 01.18.01^
20200203 13:06:51^PL^I0001^Read Config File: conf\HYETST\TST\BRK\HYETST_TST_TBK0461_BRK_Send_TVA.picfg Sucess^
20200203 13:06:51^PL^I0009^Begin process: TBK0461_S_TVA.picfg, version: 01.18.01^
20200203 13:06:52^PL^I0011^Send, Send finish: .\data\SendSrc\TBK0461 no files^
20200203 13:06:53^PL^E0022^Send, Send file by tymcomm+J fail! return code :12, please refer to tymcomm's log^
20200203 13:06:53^PL^I0010^End process: HYETST_TST_TBK0461_BRK_Send_TVA.picfg^");

			var result = adapter.SendMessage(sendRequest);

			Assert.AreEqual("E0022", result.ErrorCode);
			Assert.AreEqual("Send file by tymcomm+J fail! return code :12, please refer to tymcomm's log", result.ErrorMessge);
			fileManagerMock.Verify(x => x.CreateFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Exactly(3));
		}

		[TestCaseSource(typeof(Source), nameof(Source.SourceOfSend))]
		public void GatewaySendMessageTest_UpdateCertificate(TWCustomsGatewaySendRequest sendRequest, Source.ExpectedData expectedData)
		{
			fileManagerMock.Setup(x => x.FileExists(expectedData.CertFileName)).Returns(true);
			fileManagerMock.Setup(x => x.GetBytes(expectedData.CertFileName)).Returns(new byte[] { 0, 1 });
			fileManagerMock.Setup(x => x.FileExists(expectedData.CfgFileName)).Returns(true);
			fileManagerMock.Setup(x => x.GetText(expectedData.CfgFileName)).Returns(expectedData.ConfigurationContent);
			fileManagerMock.Setup(x => x.CreateFile(expectedData.CertFileName, It.IsAny<string>(), true)).Callback<string, string, bool>(
				(filePath, content, isBase64) =>
				{
					Assert.AreEqual(certificateBase64, content);
				});
			fileManagerMock.Setup(x => x.FileExists(expectedData.XmlFileName)).Returns(false);
			fileManagerMock.Setup(x => x.CreateFile(expectedData.XmlFileName, It.IsAny<string>(), true));
			pluginManageMock.Setup(x => x.StartProcess(It.IsAny<PluginInfo>())).Returns(string.Empty);

			var result = adapter.SendMessage(sendRequest);

			Assert.AreEqual(null, result.ErrorCode);
			Assert.AreEqual(null, result.ErrorMessge);
			fileManagerMock.Verify(x => x.CreateFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Exactly(2));
		}

		[TestCaseSource(typeof(Source), nameof(Source.SourceOfSend))]
		public void GatewaySendMessageTest_UpdateConfig(TWCustomsGatewaySendRequest sendRequest, Source.ExpectedData expectedData)
		{
			fileManagerMock.Setup(x => x.FileExists(expectedData.CertFileName)).Returns(true);
			fileManagerMock.Setup(x => x.GetBytes(expectedData.CertFileName)).Returns(Convert.FromBase64String(certificateBase64));
			fileManagerMock.Setup(x => x.FileExists(expectedData.CfgFileName)).Returns(true);
			fileManagerMock.Setup(x => x.GetText(expectedData.CfgFileName)).Returns(GetResourceAsString("TestFiles.ExistingPluginSendConfiguration_TVA.txt"));
			fileManagerMock.Setup(x => x.CreateFile(expectedData.CfgFileName, It.IsAny<string>(), false));
			fileManagerMock.Setup(x => x.FileExists(expectedData.XmlFileName)).Returns(false);
			fileManagerMock.Setup(x => x.CreateFile(expectedData.XmlFileName, It.IsAny<string>(), true));
			pluginManageMock.Setup(x => x.StartProcess(It.IsAny<PluginInfo>())).Returns(string.Empty);

			var result = adapter.SendMessage(sendRequest);

			Assert.AreEqual(null, result.ErrorCode);
			Assert.AreEqual(null, result.ErrorMessge);
			fileManagerMock.Verify(x => x.CreateFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Exactly(2));
		}

		[TestCaseSource(typeof(Source), nameof(Source.SourceOfReceive))]
		public void GatewayReceiveMessageTest_UVC_Test(TWCustomsGatewayReceiveRequest receiveRequest, Source.ExpectedData expectedData)
		{
			var twCustomsResponse = @"<TWCustomsResponse xmlns=""http://cargowise.com/ehub/products/TWCustoms"">
	<Header>
		<Sender>TestClientID</Sender>
		<Recipient>HYETSTTST</Recipient>
	</Header>
	<Body>
		<InterchangeNum>{1}</InterchangeNum>
		{0}
	</Body>
</TWCustomsResponse>";
			fileManagerMock.Setup(x => x.FileExists(It.IsAny<string>())).Returns(false);
			fileManagerMock.Setup(x => x.CreateFile(expectedData.CertFileName, It.IsAny<string>(), true)).Callback<string, string, bool>(
				(filePath, content, isBase64) =>
				{
					Assert.AreEqual(certificateBase64, content);
				});
			fileManagerMock.Setup(x => x.CreateFile(expectedData.CfgFileName, It.IsAny<string>(), false)).Callback<string, string, bool>(
				(filePath, content, isBase64) =>
				{
					Assert.AreEqual(expectedData.ConfigurationContent, content);
				});
			fileManagerMock.Setup(x => x.GetTWCustomsResponseFiles($@"K:\Plugin\data\RecvTarget{expectedData.CombinedPath}\TBK0461")).Returns(new[]
			{
				new FileInfo("File1"),
				new FileInfo("File2"),
				new FileInfo("File3")
			});
			fileManagerMock.Setup(x => x.GetText(It.IsAny<string>())).Returns<string>((filePath) =>
			{
				if (filePath.Contains("File1"))
					return "<File1Content />";
				if (filePath.Contains("File2"))
					return "<File2Content/>";
				return "<File3Content />";
			});
			fileManagerMock.Setup(x => x.Delete(It.IsAny<string>()));
			httpClientMock.Setup(x => x.PostAsync("http://Test-Server/TestService.svc", It.IsAny<string>())).Returns(new HttpResponseMessage(HttpStatusCode.OK));
			pluginManageMock.Setup(x => x.StartProcess(It.IsAny<PluginInfo>())).Returns(@"20200207 09:11:03^PL^I0000^Begin PlugInHandler, version: 01.18.01^
20200207 09:11:03^PL^I0001^Read Config File: C:\Plugin\conf\TBK0461_R_UVC.picfg Sucess^
20200207 09:11:03^PL^I0009^Begin process: TBK0461_R_UVC.picfg, version: 01.18.01^
20200207 09:11:04^PL^I0012^Receive, Receive finish: TBK0461_R_UVC.picfg, success: 0 records^
20200207 09:11:05^PL^I0010^End process: TBK0461_R_UVC.picfg^");

			var result = adapter.ReceiveMessage(receiveRequest);

			Assert.AreEqual(null, result.ErrorCode);
			Assert.AreEqual(null, result.ErrorMessge);
			fileManagerMock.Verify(x => x.CreateFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Exactly(2));
			httpClientMock.Verify(x => x.PostAsync("http://Test-Server/TestService.svc", string.Format(twCustomsResponse, "<File1Content />", "File1")), Times.Once);
			httpClientMock.Verify(x => x.PostAsync("http://Test-Server/TestService.svc", string.Format(twCustomsResponse, "<File2Content />", "File2")), Times.Once);
			httpClientMock.Verify(x => x.PostAsync("http://Test-Server/TestService.svc", string.Format(twCustomsResponse, "<File3Content />", "File3")), Times.Once);
		}
		#endregion

		public static T Deserialize<T>(string input) where T : ITWCustomsRequest
		{
			System.Xml.Serialization.XmlSerializer ser = new System.Xml.Serialization.XmlSerializer(typeof(T));

			using (StringReader sr = new StringReader(input))
			{
				return (T)ser.Deserialize(sr);
			}
		}

		#region TestDataSource
		public static class Source
		{
			public static IEnumerable<TestCaseData> SourceOfSend()
			{
				yield return new TestCaseData(
					Deserialize<TWCustomsGatewaySendRequest>(GetResourceAsString("TestFiles.SendMessage.xml")),
					new ExpectedData
					{
						CertFileName = certFileName_TWCustomsAccount,
						CfgFileName = cfgFileName_TWCustomsAccount_Send_TVA,
						XmlFileName = xmlFileName_TWCustomsAccount,
						ConfigurationContent = configurationContent_TWCustomsAccount_Send_TVA,
					})
					.SetName("TWCustomsAccount TVA {M}");

				yield return new TestCaseData(
					Deserialize<TWCustomsGatewaySendRequest>(GetResourceAsString("TestFiles.SendMessage_CAA.xml")),
					new ExpectedData
					{
						CertFileName = certFileName_TWCustomsAccount,
						CfgFileName = cfgFileName_TWCustomsAccount_Send_TVA_CAA,
						XmlFileName = xmlFileName_TWCustomsAccount_CAA,
						ConfigurationContent = configurationContent_TWCustomsAccount_Send_TVA_CAA,
					})
					.SetName("TWCustomsAccount TVA_CAA {M}");

				yield return new TestCaseData(
					Deserialize<TWCustomsGatewaySendRequest>(GetResourceAsString("TestFiles.SendMessage_ForwarderManifest.xml")),
					new ExpectedData
					{
						CertFileName = certFileName_ForwarderManifest,
						CfgFileName = cfgFileName_ForwarderManifest_Send_TVF,
						XmlFileName = xmlFileName_ForwarderManifest,
						ConfigurationContent = configurationContent_ForwarderManifest_Send_TVF,
					})
					.SetName("TWCustomsForwarderManifest TVA {M}");
			}

			public static IEnumerable<TestCaseData> SourceOfReceive()
			{
				yield return new TestCaseData(
					Deserialize<TWCustomsGatewayReceiveRequest>(GetResourceAsString("TestFiles.ReceiveMessage.xml")),
					new ExpectedData
					{
						CertFileName = certFileName_TWCustomsAccount,
						CfgFileName = cfgFileName_TWCustomsAccount_Receive_UVC,
						ConfigurationContent = configurationContent_TWCustomsAccount_Receive_UVC,
						CombinedPath = combinedPath_TWCustomsAccount,
					})
					.SetName("TWCustomsAccount UVC {M}");
			}

			static string RemoveStaffCode(string input, string staffCode)
			{
				return input.Replace($@"\{staffCode}", string.Empty)
							.Replace($"/{staffCode}", string.Empty)
							.Replace($"_{staffCode}", string.Empty);
			}

			const string combinedPath_TWCustomsAccount = @"\HYETST\TST\BRK";
			readonly static string combinedPath_ForwarderManifest = RemoveStaffCode(combinedPath_TWCustomsAccount, "BRK");

			const string certFileName_TWCustomsAccount = @"K:\Plugin\cert\HYETST\TST\BRK\HYETST_TST_BRK_TBK0461-A_Certificate.pfx";
			readonly static string certFileName_ForwarderManifest = RemoveStaffCode(certFileName_TWCustomsAccount, "BRK");

			const string cfgFileName_TWCustomsAccount_Send_TVA = @"K:\Plugin\conf\HYETST\TST\BRK\HYETST_TST_TBK0461_BRK_Send_TVA.picfg";
			const string cfgFileName_TWCustomsAccount_Send_TVA_CAA = @"K:\Plugin\conf\HYETST\TST\BRK\HYETST_TST_TBK0461_BRK_Send_TVA_CAA.picfg";
			readonly static string cfgFileName_TWCustomsAccount_Receive_UVC = cfgFileName_TWCustomsAccount_Send_TVA.Replace("Send_TVA", "Receive_UVC");
			readonly static string cfgFileName_ForwarderManifest_Send_TVF = @"K:\Plugin\conf\HYETST\TST\HYETST_TST_TBK0461_Send_TVF.picfg";

			const string xmlFileName_TWCustomsAccount = @"K:\Plugin\data\SendSrc\HYETST\TST\BRK\TBK0461\N5203.97162640001206150001.xml";
			const string xmlFileName_TWCustomsAccount_CAA = @"K:\Plugin\data\SendSrc\HYETST\TST\BRK\TBK0461_CAA\N5203.97162640001206150001.xml";
			readonly static string xmlFileName_ForwarderManifest = RemoveStaffCode(xmlFileName_TWCustomsAccount, "BRK");

			readonly static string configurationContent_TWCustomsAccount_Send_TVA = GetResourceAsString("TestFiles.PluginSendConfiguration_TVA.txt");
			readonly static string configurationContent_TWCustomsAccount_Send_TVA_CAA = GetResourceAsString("TestFiles.PluginSendConfiguration_TVA_CAA.txt");
			readonly static string configurationContent_ForwarderManifest_Send_TVF = GetResourceAsString("TestFiles.PluginSendConfiguration_TVF_ForwarderManifest.txt");

			readonly static string configurationContent_TWCustomsAccount_Receive_UVC = GetResourceAsString("TestFiles.PluginReceiveConfiguration_UVC.txt");

			public class ExpectedData
			{
				public string CertFileName;
				public string CfgFileName;
				public string XmlFileName;
				public string ConfigurationContent;
				public string CombinedPath;
			}
		}
		#endregion
	}
}