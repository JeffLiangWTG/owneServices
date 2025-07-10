namespace Enterprise.RemotePrinting.Client.Tests
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Reflection;
	using System.Text;
	using System.Xml;
	using CargoWise.eHub.Common.Extensions;
	using CargoWise.eHub.Common.Service.Reference;
	using Enterprise.RemotePrinting.Client.RemotePrintServer;
	using Moq;
	using NUnit.Framework;

	public static class CustomseHubTestHelper
	{
		public static void OnSettingDownloaded(ICustomseHubClientSetting setting)
		{
			var controllerMock = new Mock<ICustomsMessageController>();
			controllerMock.Setup(c => c.ConfigSetting).Returns(new WebClientConfiguration());
			controllerMock.Setup(c => c.ShowInformation(It.IsAny<string>())).Callback((string message) => { });
			controllerMock.Setup(c => c.ResetConfigSetting()).Callback(() => { });

			var decryptor = new CustomsClientDecryptor(controllerMock.Object, setting);
			decryptor.DecryptIfNeeded();
		}

		public static void CreateTestFolders(string testName, IEnumerable<string> folders)
		{
			foreach (var folder in folders)
			{
				Directory.CreateDirectory(Path.Combine(TestFolder, testName, folder));
			}
		}

		public static void DeleteTestFolders(string testName)
		{
			var path = Path.Combine(TestFolder, testName);
			if (Directory.Exists(path))
			{
				Directory.Delete(Path.Combine(TestFolder, testName), true);
			}
		}

		public static void AssertTextEqualsIgnoreXmlFormats(string message, string expected, string actual)
		{
			Assertion.AssertMultilineASCIIEquals(message, ClearXmlFormats(expected), ClearXmlFormats(actual));
		}

		static string ClearXmlFormats(string input)
		{
			var doc = new XmlDocument();
			doc.LoadXml(input);
			return doc.DocumentElement.OuterXml;
		}

		public static void AssertZipEquals(string message, string outputFilePath, string expectedRes)
		{
			using (var expectedStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(expectedRes))
			using (var expectedMemoryStream = new MemoryStream())
			using (var actualStream = File.OpenRead(outputFilePath))
			using (var actualMemoryStream = new MemoryStream())
			{
				expectedStream.WriteTo(expectedMemoryStream);
				actualStream.WriteTo(actualMemoryStream);
				Assertion.AssertEquals(message, expectedMemoryStream.ToArray(), actualMemoryStream.ToArray());
			}
		}

		public static void AssertText(string outputFullPath, string expectedOutputRes)
		{
			string actualText = string.Empty;
			string expectedText = string.Empty;

			using (var expectedContentStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(expectedOutputRes))
			{
				expectedText = expectedContentStream.ReadToEnd();
			}

			if (outputFullPath.EndsWith(".txt"))
			{
				actualText = File.ReadAllText(outputFullPath, Encoding.UTF8);
			}

			Assertion.AssertEquals(expectedText, actualText);
		}

		public static void AssertXmlText(string outputFullPath, string expectedOutputRes)
		{
			string actualText = string.Empty;
			string expectedText = string.Empty;

			using (var expectedContentStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(expectedOutputRes))
			{
				expectedText = expectedContentStream.ReadToEnd();
			}

			if (outputFullPath.EndsWith(".xml"))
			{
				actualText = File.ReadAllText(outputFullPath, Encoding.UTF8);
			}
			else if (outputFullPath.EndsWith(".zip"))
			{
				using (var actualStream = new MemoryStream(File.ReadAllBytes(outputFullPath)))
				using (var actualStreamDecompressed = actualStream.DecodeAndDecompress())
				{
					actualText = actualStreamDecompressed.ReadToEnd();
				}
			}

			AssertTextEqualsIgnoreXmlFormats(outputFullPath, expectedText, actualText);
		}

		public static void AssertFileEncoding(string filePath, Encoding expectedEncoding)
		{
			using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
			using (StreamReader sr = new StreamReader(filePath))
			{
				var numBytesToRead = (int)fs.Length;
				byte[] actualBytes = new byte[numBytesToRead];
				fs.Read(actualBytes, 0, numBytesToRead);

				var text = sr.ReadToEnd();
				var expectedBytes = expectedEncoding.GetBytes(text);
				Assertion.AssertEquals(expectedBytes, actualBytes);
			}
		}

		public static string TestFolder => Path.Combine(
			AssemblyDirectory,
			"CustomseHubTest"
		);

		[ThreadStatic]
		static string fAssemblyDirectory;

		static string AssemblyDirectory
		{
			get
			{
				if (string.IsNullOrWhiteSpace(fAssemblyDirectory))
				{
					var location = Assembly.GetExecutingAssembly().Location;
					var uri = new UriBuilder(location);
					var path = Uri.UnescapeDataString(uri.Path);
					fAssemblyDirectory = Path.GetDirectoryName(path);
				}
				return fAssemblyDirectory;
			}
		}

		static Stream GetTestFileStream(string expectedOutputRes)
		{
			return Assembly.GetExecutingAssembly().GetManifestResourceStream(expectedOutputRes);
		}

		public static void AssertText(string message, string outputFullPath, string expectedOutputRes)
		{
			string actualText = string.Empty;
			string expectedText = string.Empty;

			using (var expectedContentStream = GetTestFileStream(expectedOutputRes))
			{
				expectedText = expectedContentStream.ReadToEnd();
			}

			if (outputFullPath.EndsWith(".txt"))
			{
				actualText = File.ReadAllText(outputFullPath, Encoding.UTF8);
			}
			Assertion.AssertEquals(message, expectedText, actualText);
		}

		public static void AssertXmlFile(string outputFullPath, string expectedOutputRes)
		{
			string actualText = string.Empty;
			string expectedText = string.Empty;

			using (var expectedContentStream = GetTestFileStream(expectedOutputRes))
			{
				expectedText = expectedContentStream.ReadToEnd();
			}

			if (outputFullPath.EndsWith(".xml"))
			{
				actualText = File.ReadAllText(outputFullPath, Encoding.UTF8);
			}
			else if (outputFullPath.EndsWith(".zip"))
			{
				using (var actualStream = new MemoryStream(File.ReadAllBytes(outputFullPath)))
				using (var actualStreamDecompressed = actualStream.DecodeAndDecompress())
				{
					actualText = actualStreamDecompressed.ReadToEnd();
				}
			}

			AssertTextEqualsIgnoreXmlFormats(outputFullPath, expectedText, actualText);
		}
	}

	public static class CNSWTestHelper
	{
		public static void CreateTestFolders()
		{
			CustomseHubTestHelper.CreateTestFolders("CNSWTest", new[] { "Send", "Receive", "ErrorResponse", "Archive", "AcdaSend", "AcdaReceive", "AcdaErrorResponse", "AcdaArchive" });
		}

		public static void DeleteTestFolders()
		{
			CustomseHubTestHelper.DeleteTestFolders("CNSWTest");
		}

		public static WebClient CreateFakeWebClient()
		{
			var config = Configurator.GetProxyDefaultSystemSettings();
			var mock = new Mock<WebClient>(new RemotePrintingServiceAdaptor(config));
			mock.Setup(m => m.GetCNSWClientApplicationSetting(CNSWSettingManagerForTesting.EmptyMachine)).Returns(new CNSWClientApplicationSettingWrapper(new CNSWClientSetting()));
			mock.Setup(m => m.GetCNSWClientApplicationSetting(CNSWSettingManagerForTesting.WrongMachine)).Returns(new CNSWClientApplicationSettingWrapper(new CNSWClientSetting
			{
				MachineName = CNSWSettingManagerForTesting.WrongMachine,
				SendFolder = @"ZZZ:\Sender_Folder_That_Does_Not_Exist",
				ReceiveFolder = @"ZZZ:\Receive_Folder_That_Does_Not_Exist",
				ErrorResponseFolder = @"ZZZ:\Err_Folder_That_Does_Not_Exist",
				ArchiveFolder = @"ZZZ:\Archive_Folder_That_Does_Not_Exist",
				RunningIntervalInSeconds = -1,
				EHubClientID = "ENTCMPSVR",
				EHubClientPassword = Convert.ToBase64String(Encoding.UTF8.GetBytes("Password")),
				EHubClientStatus = "OK",
				EHubGatewayServerAddress = "ehub.exception.address",
			}));
			mock.Setup(m => m.GetCNSWClientApplicationSetting(CNSWSettingManagerForTesting.CorrectMachine)).Returns(new CNSWClientApplicationSettingWrapper(new CNSWClientSetting
			{
				MachineName = CNSWSettingManagerForTesting.CorrectMachine,

				SendFolder = TestSendFolder,
				ReceiveFolder = TestReceiveFolder,
				ErrorResponseFolder = TestErrorResponseFolder,
				ArchiveFolder = TestArchiveFolder,

				AcdaSendFolder = TestAcdaSendFolder,
				AcdaReceiveFolder = TestAcdaReceiveFolder,
				AcdaErrorResponseFolder = TestAcdaErrorResponseFolder,
				AcdaArchiveFolder = TestAcdaArchiveFolder,

				RunningIntervalInSeconds = 20,
				EHubClientID = "ENTCMPSVR_CSW",
				EHubClientPassword = Convert.ToBase64String(Encoding.UTF8.GetBytes("Password")),
				EHubClientStatus = "OK",
				EHubGatewayServerAddress = CustomseHubServiceClientProxyForTesting.Test_CorrectAddress
			}));
			mock.Setup(client => client.GetCNSWClientApplicationSetting(CNSWSettingManagerForTesting.AcdaFolderNotSetMachine)).Returns(new CNSWClientApplicationSettingWrapper(new CNSWClientSetting
			{
				MachineName = CNSWSettingManagerForTesting.AcdaFolderNotSetMachine,

				SendFolder = TestSendFolder,
				ReceiveFolder = TestReceiveFolder,
				ErrorResponseFolder = TestErrorResponseFolder,
				ArchiveFolder = TestArchiveFolder,

				AcdaSendFolder = null,
				AcdaReceiveFolder = null,
				AcdaErrorResponseFolder = null,
				AcdaArchiveFolder = null,

				RunningIntervalInSeconds = 20,
				EHubClientID = "ENTCMPSVR_CSW",
				EHubClientPassword = Convert.ToBase64String(Encoding.UTF8.GetBytes("Password")),
				EHubClientStatus = "OK",
				EHubGatewayServerAddress = CustomseHubServiceClientProxyForTesting.Test_CorrectAddress
			}));

			return mock.Object;
		}

		public static string TestSendFolder => Path.Combine(TestDirectory, "Send");

		public static string TestReceiveFolder => Path.Combine(TestDirectory, "Receive");

		public static string TestErrorResponseFolder => Path.Combine(TestDirectory, "ErrorResponse");

		public static string TestArchiveFolder => Path.Combine(TestDirectory, "Archive");

		public static string TestAcdaSendFolder => Path.Combine(TestDirectory, "AcdaSend");

		public static string TestAcdaReceiveFolder => Path.Combine(TestDirectory, "AcdaReceive");

		public static string TestAcdaErrorResponseFolder => Path.Combine(TestDirectory, "AcdaErrorResponse");

		public static string TestAcdaArchiveFolder => Path.Combine(TestDirectory, "AcdaArchive");

		public static string TestDirectory { get; } = Path.Combine(CustomseHubTestHelper.TestFolder, "CNSWTest");
	}

	public static class TWNCATKTestHelper
	{
		public static WebClient CreateFakeWebClient()
		{
			var config = Configurator.GetProxyDefaultSystemSettings();
			var mockedClient = new Mock<WebClient>(new RemotePrintingServiceAdaptor(config));

			mockedClient.Setup(m => m.GetTWNCATKClientApplicationSetting(TWNCATKSettingManagerForTesting.CorrectMachineName))
				.Returns(new TWNCATKClientApplicationSettingWrapper(new TWNCATKClientSetting()
				{
					MachineName = TWNCATKSettingManagerForTesting.CorrectMachineName,
					EHubClientID = "ID",
					EHubClientPassword = Convert.ToBase64String(Encoding.UTF8.GetBytes("PWD")),
					EHubClientStatus = "OK",
					EHubGatewayServerAddress = CustomseHubServiceClientProxyForTesting.Test_CorrectAddress,
					RunningIntervalInSeconds = 20,
					SendToFolder = Path.Combine(CustomseHubTestHelper.TestFolder, "TWNCATK", "SendToFolder")
				}));
			mockedClient.Setup(m => m.GetTWNCATKClientApplicationSetting(TWNCATKSettingManagerForTesting.EmptySettingMachine))
				.Returns(new TWNCATKClientApplicationSettingWrapper(new TWNCATKClientSetting()));
			return mockedClient.Object;
		}
	}

	public static class JPNACCSTestHelper
	{
		public static WebClient CreateFakeWebClient()
		{
			var config = Configurator.GetProxyDefaultSystemSettings();
			var mockedClient = new Mock<WebClient>(new RemotePrintingServiceAdaptor(config));

			mockedClient.Setup(m => m.GetJPNACCSClientApplicationSetting(JPNACCSSettingManagerForTesting.CorrectMachineName))
				.Returns(new JPNACCSClientApplicationSettingWrapper(new JPNACCSClientSetting
				{
					DomainName = "WebPrint.WisetechGlobal.com",
					NACCSMailbox = "NACCS@MAIL.TEST.NACCS6",
					MailBoxInfos = new[] { new MailBoxInfo { CompanyCode = "TST", MailBox = "xxx@MAIL.TEST.NACCS6", MailBoxPassword = Convert.ToBase64String(Encoding.UTF8.GetBytes("123")) } },
					ReceivingInterval = 3,
					SendingInterval = 15,
					InterchangeCountPerBatchOnReceivingValue = 100,
					XTIdleConnectionKeepAliveInSecondsValue = 60d,
					XTIdleConnectionRetryPauseInSecondsValue = 15d,
					Verbose = true,
					xTApplicationNode = "WTLCTU_JPC",
					xTServerAddress = "xttest-eservices.wisegrid.net:61002",
					xTServerCertificate = JPNACCSSettingManagerForTesting.Certificate,
					xTPassword = Convert.ToBase64String(Encoding.UTF8.GetBytes("TEST12345"))
				}, JPNACCSSettingManagerForTesting.CorrectMachineName));
			mockedClient.Setup(m => m.GetJPNACCSClientApplicationSetting(JPNACCSSettingManagerForTesting.EmptySettingMachine))
				.Returns(new JPNACCSClientApplicationSettingWrapper(new JPNACCSClientSetting(), string.Empty));
			return mockedClient.Object;
		}
	}

	public static class CLSMSTestHelper
	{
		public static void CreateTestFolders()
		{
			CustomseHubTestHelper.CreateTestFolders("CLSMSTest", new[] { "Send", "Receive", "Accepted", "Rejected", "Invalid", "Unknown" });
		}

		public static void DeleteTestFolders()
		{
			CustomseHubTestHelper.DeleteTestFolders("CLSMSTest");
		}

		public static WebClient CreateFakeWebClient(string machineType)
		{
			var config = Configurator.GetProxyDefaultSystemSettings();
			var mock = new Mock<WebClient>(new RemotePrintingServiceAdaptor(config));
			if (machineType == CLSMSSettingManagerForTesting.EmptyMachine)
			{
				mock.Setup(m => m.GetCLSMSClientApplicationSetting(CLSMSSettingManagerForTesting.EmptyMachine)).Returns(new CLSMSClientApplicationSettingWrapper(new CLSMSClientSetting()));
			}
			else if (machineType == CLSMSSettingManagerForTesting.WrongMachine)
			{
				mock.Setup(m => m.GetCLSMSClientApplicationSetting(CLSMSSettingManagerForTesting.WrongMachine)).Returns(new CLSMSClientApplicationSettingWrapper(new CLSMSClientSetting
				{
					ServerAddress = "ehub.exception.address",
					ServerCertificate = "Certificate",
					RunningIntervalInSeconds = -1,
					SendFolder = @"ZZZ:\Sender_Folder_That_Does_Not_Exist",
					ReceiveFolder = @"ZZZ:\Receive_Folder_That_Does_Not_Exist",
					AcceptedFolder = @"ZZZ:\Accepted_Folder_That_Does_Not_Exist",
					RejectedFolder = @"ZZZ:\Rejected_Folder_That_Does_Not_Exist",
					InvalidFolder = @"ZZZ:\Invalid_Folder_That_Does_Not_Exist",
					UnknownFolder = @"ZZZ:\Unknown_Folder_That_Does_Not_Exist",
					MachineName = CLSMSSettingManagerForTesting.WrongMachine
				}));
			}
			else if (machineType == CLSMSSettingManagerForTesting.CorrectMachine)
			{
				mock.Setup(m => m.GetCLSMSClientApplicationSetting(CLSMSSettingManagerForTesting.CorrectMachine)).Returns(new CLSMSClientApplicationSettingWrapper(new CLSMSClientSetting
				{
					ApplicationNodeName = "xt-application:{cfb01c8e-f063-4e3a-bc3e-7e274f84e66r}",
					ApplicationNodePassword = Convert.ToBase64String(Encoding.UTF8.GetBytes("xHGatewayPW")),
					ServerAddress = "127.0.0.1:61001",
					ServerCertificate = "-----BEGIN CERTIFICATE-----\nMIIETjCCAragAwIBAgIJANaqHXch0txmMA0GCSqGSIb3DQEBCwUAMA0xCzAJBgNV\nBAYTAkFVMCAXDTIxMDQyMzAwNTUyN1oYDzIwMjQwNDIzMDAwMDAwWjANMQswCQYD\nVQQGEwJBVTCCAaIwDQYJKoZIhvcNAQEBBQADggGPADCCAYoCggGBAJ0+AyGGGDfU\nbJmCQszgnfJC9MmRm6kRl970hmVgl1UAJ8Oa10T/s+JA/JAhyo48Rvl0Ukh9JD5Z\nC6VayqyP8I1SdREFUXBhjChC6xt+LiA6/ZFs1Z3j5eoWH6S/uS5IWeRp7LIoFrp0\nvMyW8RaCQdOLi1oM6cCGv5c8KQAPFrqcrKwxyBFIk1I8SoUvObpt0QWZPJLcTq37\nuuWxIx//4mOG0Sqgv5aSCbpvYNi2c8FEm+MHTKz6yOVXLoQpHFb4ZBpolI4IxY2E\ncBxiVWYTrLQczoUaMC+0PMUXspoZfE0BWaCd6u+Ddxn1sLoSYF7d1wILMpNogwx5\nl1zKlDCEswZ+5VY8WoEjyojl+r2GYZf7dG86o8Zenhumxr49PE01ZX1WCFadWbZ/\n713CvgJgYnCfumU9tjH0h9VkXMpuomImH1zWKXqFHbHd7pgyewFqMDYESiC++kc4\nGwPhaNrmE98g/TWwLU+kaVdu0DXBe2dROkjlclkLzzgFziWHFNgsYQIDAQABo4Gu\nMIGrMAwGA1UdEwEB/wQCMAAwDgYDVR0PAQH/BAQDAgKkMB0GA1UdJQQWMBQGCCsG\nAQUFBwMCBggrBgEFBQcDATAsBgNVHREEJTAjhwR/AAABhxAAAAAAAAAAAAAAAAAA\nAAABgglsb2NhbGhvc3QwHQYDVR0OBBYEFMq3gb9uyEcOarASQOweAsgbXdC3MB8G\nA1UdIwQYMBaAFMq3gb9uyEcOarASQOweAsgbXdC3MA0GCSqGSIb3DQEBCwUAA4IB\ngQAaLTdlIwhB7kD8bLR9aU3IogeAmUkojF9UeNd9wUpTWVBUE15LItnjbW55hLkJ\n+zgPjCUdG/hroVUDdwiK63c0MzvQ0LH7YDj93rvmfZdiLbQQs57dwsTiFjoqSxNb\nug3P9cqbBPThQ/gzjloIynvAgkrm32p7J5jDGRyx1hbKcXjFINPw+EsrtH8NjPYL\njVC5zckS0IQIfkwCPV90yK3f3gluF6a0zlhhkCxbg1IfXF5l/+4H3kzFMGZpsdU9\nAAwckL2rg3ixd92sIBIBxTIJxbH5BtYlnaLCEmEdNeucWJ33QEjnV/aZE2jxiLk2\ndEh7PhfSIoIQqawxHG8HqOiWgBe25xdGS+2zjZ3jRpy+SZ2lsfzflpg3W7HQUeTU\naDrGqtmiE2tueesZJYtW46TiUEBv0eqhyYOxhRIWSu3aH4VazAL7h09bjShKhvhj\ngp/yyoaVtfrG6lGX7Hw7wt7K48D4leEnH5NaVpw8EboVEwuDm15LIMAO25py09jd\noEs=\n-----END CERTIFICATE-----\n",
					RunningIntervalInSeconds = 20,
					SendFolder = TestSendFolder,
					ReceiveFolder = TestReceiveFolder,
					AcceptedFolder = TestAcceptedFolder,
					RejectedFolder = TestRejectedFolder,
					InvalidFolder = TestInvalidFolder,
					UnknownFolder = TestUnknownFolder,
					MachineName = CLSMSSettingManagerForTesting.CorrectMachine
				}));
			}

			return mock.Object;
		}

		public static string TestSendFolder => Path.Combine(TestDirectory, "Send");

		public static string TestReceiveFolder => Path.Combine(TestDirectory, "Receive");

		public static string TestPendingFolder => Path.Combine(TestDirectory, "Pending");

		public static string TestAcceptedFolder => Path.Combine(TestDirectory, "Accepted");

		public static string TestRejectedFolder => Path.Combine(TestDirectory, "Rejected");

		public static string TestInvalidFolder => Path.Combine(TestDirectory, "Invalid");

		public static string TestUnknownFolder => Path.Combine(TestDirectory, "Unknown");

		public static string TestDirectory { get; } = Path.Combine(CustomseHubTestHelper.TestFolder, "CLSMSTest");
	}

	interface IEHubClientSettingManagerForTesting
	{
		bool SendStreamCalledForTesting { get; }
		void Set_SendStreamCalled_ForTesting(bool called);

		string SentContentForTesting { get; }
		void Set_SentContent_ForTesting(string content);
	}

	class CustomseHubServiceClientProxyForTesting : CustomseHubServiceClientProxy
	{
		public const string Test_CorrectAddress = "ehub.correct.address";
		public const string Test_UnreachableAddress = "ehub.unreachable.address";
		public const string Test_RetrievedFile = "RetrievedFile.xml";

		readonly IEHubClientSettingManagerForTesting Manager;

		readonly string TextForRetrieveStream;

		public CustomseHubServiceClientProxyForTesting(
			IEHubClientSettingManagerForTesting manager,
			string serverAddress, string clientID, string password, string textForRetrieveStream = ""
		) : base(serverAddress, clientID, password)
		{
			Manager = manager;
			TextForRetrieveStream = textForRetrieveStream;
		}

		bool MockPing()
		{
			if (ServerAddress == Test_CorrectAddress)
			{
				return true;
			}
			else if (ServerAddress == Test_UnreachableAddress)
			{
				return false;
			}
			else
			{
				throw new Exception($"Exception thrown when trying to get response from: {ServerAddress}.");
			}
		}

		SendStreamResponse MockSendStream(SendStreamRequest request)
		{
			Manager.Set_SendStreamCalled_ForTesting(true);

			if (request != null && request.Messages.Length > 0)
			{
				var sentStream = request.Messages[0].MessageStream.DecodeAndDecompress();
				using (var reader = new StreamReader(sentStream, Encoding.UTF8))
				{
					sentStream.SeekBegin();
					Manager.Set_SentContent_ForTesting(reader.ReadToEnd());
				}
			}
			return new SendStreamResponse();
		}

		RetrieveStreamResponse MockRetrieveStream()
		{
			return new RetrieveStreamResponse(
						It.IsAny<string>(),
						new[]
						{
							new CargoWise.eHub.Common.eHubGatewayMessage
							{
								FileName = Test_RetrievedFile,
								MessageStream = new MemoryStream(Encoding.UTF8.GetBytes(TextForRetrieveStream)).CompressAndEncode()
							}
						});
		}

		protected override eHubStreamedService Client
		{
			get
			{
				var mock = new Mock<eHubStreamedService>();
				mock.Setup(m => m.Ping()).Returns(MockPing());
				mock.Setup(m => m.SendStream(It.IsAny<SendStreamRequest>())).Callback<SendStreamRequest>(
					streamRequest =>
					{
						MockSendStream(streamRequest);
					});
				mock.Setup(m => m.RetrieveStream(It.IsAny<RetrieveStreamRequest>())).Returns(MockRetrieveStream());

				return mock.Object;
			}
		}

		public new void Dispose()
		{
			base.Dispose();
		}

		public static WebClient CreateFakeWebClient()
		{
			var config = Configurator.GetProxyDefaultSystemSettings();
			var mock = new Mock<WebClient>(new RemotePrintingServiceAdaptor(config));
			mock.Setup(m => m.GetCNSWClientApplicationSetting(CNSWSettingManagerForTesting.EmptyMachine)).Returns(new CNSWClientApplicationSettingWrapper(new CNSWClientSetting()));
			mock.Setup(m => m.GetCNSWClientApplicationSetting(CNSWSettingManagerForTesting.WrongMachine)).Returns(new CNSWClientApplicationSettingWrapper(new CNSWClientSetting
			{
				MachineName = CNSWSettingManagerForTesting.WrongMachine,
				SendFolder = @"ZZZ:\Sender_Folder_That_Does_Not_Exist",
				ReceiveFolder = @"ZZZ:\Receive_Folder_That_Does_Not_Exist",
				ErrorResponseFolder = @"ZZZ:\Err_Folder_That_Does_Not_Exist",
				ArchiveFolder = @"ZZZ:\Archive_Folder_That_Does_Not_Exist",
				RunningIntervalInSeconds = -1,
				EHubClientID = "ENTCMPSVR",
				EHubClientPassword = Convert.ToBase64String(Encoding.UTF8.GetBytes("Password")),
				EHubClientStatus = "OK",
				EHubGatewayServerAddress = "ehub.exception.address",
			}));
			mock.Setup(m => m.GetCNSWClientApplicationSetting(CNSWSettingManagerForTesting.CorrectMachine)).Returns(new CNSWClientApplicationSettingWrapper(CorrectCNSWClientSetting));
			return mock.Object;
		}

		[ThreadStatic]
		static string fCNSWTestDirectory;
		static string CNSWTestDirectory => string.IsNullOrWhiteSpace(fCNSWTestDirectory) ? (fCNSWTestDirectory = Path.Combine(CustomseHubTestHelper.TestFolder, "CNSWTest")) : fCNSWTestDirectory;

		[ThreadStatic]
		static CNSWClientSetting fCorrectCNSWClientSetting;

		public static CNSWClientSetting CorrectCNSWClientSetting => fCorrectCNSWClientSetting ?? (fCorrectCNSWClientSetting = new CNSWClientSetting
		{
			MachineName = CNSWSettingManagerForTesting.CorrectMachine,
			SendFolder = Path.Combine(CNSWTestDirectory, "Send"),
			ReceiveFolder = Path.Combine(CNSWTestDirectory, "Receive"),
			ErrorResponseFolder = Path.Combine(CNSWTestDirectory, "Error"),
			ArchiveFolder = Path.Combine(CNSWTestDirectory, "Archive"),
			RunningIntervalInSeconds = 20,
			EHubClientID = "ENTCMPSVR_CSW",
			EHubClientPassword = Convert.ToBase64String(Encoding.UTF8.GetBytes("Password")),
			EHubClientStatus = "OK",
			EHubGatewayServerAddress = CustomseHubServiceClientProxyForTesting.Test_CorrectAddress
		});

		[ThreadStatic]
		static string fCLSMSTestDirectory;
		static string CLSMSTestDirectory => string.IsNullOrWhiteSpace(fCLSMSTestDirectory) ? (fCLSMSTestDirectory = Path.Combine(CustomseHubTestHelper.TestFolder, "CLSMSTest")) : fCLSMSTestDirectory;

		[ThreadStatic]
		static CLSMSClientSetting fCorrectCLSMSClientSetting;

		public static CLSMSClientSetting CorrectCLSMSClientSetting => fCorrectCLSMSClientSetting ?? (fCorrectCLSMSClientSetting = new CLSMSClientSetting
		{
			RegistrationKey = "Key",
			ApplicationNodeName = "Name",
			ApplicationNodePassword = Convert.ToBase64String(Encoding.UTF8.GetBytes("Password")),
			RunningIntervalInSeconds = 20,
			SendFolder = Path.Combine(CLSMSTestDirectory, "Send"),
			ReceiveFolder = Path.Combine(CLSMSTestDirectory, "Receive"),
			AcceptedFolder = Path.Combine(CLSMSTestDirectory, "Accepted"),
			RejectedFolder = Path.Combine(CLSMSTestDirectory, "Rejected"),
			InvalidFolder = Path.Combine(CLSMSTestDirectory, "Invalid"),
			UnknownFolder = Path.Combine(CLSMSTestDirectory, "Unknown"),
			MachineName = CLSMSSettingManagerForTesting.CorrectMachine
		});
	}
}
