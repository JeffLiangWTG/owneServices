using System.IO;
using System.Text;
using System.Threading;
using NUnit.Framework;

namespace Enterprise.RemotePrinting.Client.Tests
{
	public class CNSWMessageReceiverControllerTest : TestCase
	{
		public class CNSWMessageReceiverControllerForTesting : CNSWMessageReceiverController
		{
			public CNSWMessageReceiverControllerForTesting(CancellationToken cancellationToken) : base(CNSWSettingManagerForTesting.CorrectMachine, cancellationToken) { }

			public CNSWMessageReceiverControllerForTesting(string machineName, CancellationToken cancellationToken) : base(machineName, cancellationToken) { }

			protected override CustomseHubClientSettingManager CreateNewSettingManager(string machineName)
			{
				return new CNSWSettingManagerForTesting(machineName, CNSWTestHelper.CreateFakeWebClient());
			}

			public new CNSWSettingManagerForTesting SettingManager => (CNSWSettingManagerForTesting)base.SettingManager;

			protected override WebClientConfiguration GetNewConfigSetting(string configName) => new ();

			public void Test_ProcessCore()
			{
				base.ProcessCore(SettingManager.CurrentSetting);
			}
		}

		public void TestProcess()
		{
			var testXml = @"<?xml version=""1.0"" encoding=""UTF-8"" standalone =""yes"" ?>
<DEC_RESULT>
  <CUS_CIQ_NO>I20180000147015879</CUS_CIQ_NO>
  <ENTRY_ID></ENTRY_ID>
  <NOTICE_DATE>2018-12-12T14:40:43</NOTICE_DATE>
  <CHANNEL>7</CHANNEL>
  <NOTE>I20180000147015879,I20180000147015879直接申报成功</NOTE>
  <CUSTOM_MASTER></CUSTOM_MASTER>
  <I_E_DATE></I_E_DATE>
  <D_DATE></D_DATE>
</DEC_RESULT>";

			using (var writer = new StreamWriter(Path.Combine(TestReceiverController.SettingManager.CurrentSetting.ReceiveFolder, "RetrievedFile.xml")))
			{
				writer.Write(testXml);
			}

			TestReceiverController.Test_ProcessCore();
			Assert(TestReceiverController.SettingManager.SendStreamCalledForTesting);
			AssertContains("sent to eHub, RecipientID: ENTCMPSVR. Archived", TestLogger.ToString());
			CustomseHubTestHelper.AssertTextEqualsIgnoreXmlFormats("", testXml, TestReceiverController.SettingManager.SentContentForTesting);
		}

		public void TestProcessDecFailedMessage()
		{
			var testXml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Root>
	<resultFlag>
		<Type>1</Type>
		<Value>11</Value>
	</resultFlag>
	<failCode version=""1"">C134</failCode>
	<failInfo><![CDATA[[
  ""[读卡器底层库]打开读卡器失败:错误码=50200"",
  ""Err:Custom50200""
]]]></failInfo>
	<retData></retData>
</Root>";
			using (var writer = new StreamWriter(Path.Combine(TestReceiverController.SettingManager.CurrentSetting.ReceiveFolder, "DecFailedMessage.xml")))
			{
				writer.Write(testXml);
			}

			TestReceiverController.Test_ProcessCore();
			Assert(TestReceiverController.SettingManager.SendStreamCalledForTesting);
			AssertContains("sent to eHub, RecipientID: ENTCMPSVR. Archived", TestLogger.ToString());

			var expectedSentContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Root xmlns=""http://www.chinaport.gov.cn/dec"">
	<resultFlag>
		<Type>1</Type>
		<Value>11</Value>
	</resultFlag>
	<failCode version=""1"">C134</failCode>
	<failInfo><![CDATA[[
  ""[读卡器底层库]打开读卡器失败:错误码=50200"",
  ""Err:Custom50200""
]]]></failInfo>
	<retData></retData>
</Root>";

			CustomseHubTestHelper.AssertTextEqualsIgnoreXmlFormats("", expectedSentContent, TestReceiverController.SettingManager.SentContentForTesting);
		}

		public void TestProcessImportAgrRequest()
		{
			var testXml = @"
<ImportAgrResponse>
	<ResponseInfo>
		<ResponseCode>1</ResponseCode>
		<ResponseMessage>导入成功</ResponseMessage>
		<CopCusCode>3117980008</CopCusCode>
	</ResponseInfo>
	<ConsignNo>20212243495988986</ConsignNo>
</ImportAgrResponse>";
			var filePath = Path.Combine(TestReceiverController.SettingManager.CurrentSetting.AcdaReceiveFolder, "success_1234_567890.xml");
			using (var writer = new StreamWriter(filePath))
			{
				writer.Write(testXml);
			}

			TestReceiverController.Test_ProcessCore();
			AssertEquals("Should have sent stream to eHub.", true, TestReceiverController.SettingManager.SendStreamCalledForTesting);
			CustomseHubTestHelper.AssertTextEqualsIgnoreXmlFormats("Sent content", @"
<ns0:GenericMessageInterchange xmlns:ns0=""http://cargowise.com/ehub/core/genericmessagedelivery"">
	<Header>
		<SenderID>ENTCMPSVR_CSW</SenderID>
		<RecipientID>ENTCMPSVR</RecipientID>
		<InterchangeType>CSW</InterchangeType>
		<InterchangeNumber>1234</InterchangeNumber>
	</Header>
	<Body>
		<ImportAgrResponse>
			<ResponseInfo>
				<ResponseCode>1</ResponseCode>
				<ResponseMessage>导入成功</ResponseMessage>
				<CopCusCode>3117980008</CopCusCode>
			</ResponseInfo>
			<ConsignNo>20212243495988986</ConsignNo>
		</ImportAgrResponse>
	</Body>
</ns0:GenericMessageInterchange>",

			TestReceiverController.SettingManager.SentContentForTesting);
			var achivedFileText = File.ReadAllText(Path.Combine(TestReceiverController.SettingManager.CurrentSetting.AcdaArchiveFolder, "success_1234_567890.xml"));
			AssertMultilineASCIIEquals("Should have archived.", testXml, achivedFileText);
		}

		public void TestProcessImportAgrRequest_UnexpectedXml()
		{
			var testXml = @"
<ImportAgrResponseX>
	<ResponseInfo>
		<ResponseCode>1</ResponseCode>
		<ResponseMessage>导入成功</ResponseMessage>
		<CopCusCode>3117980008</CopCusCode>
	</ResponseInfo>
	<ConsignNo>20212243495988986</ConsignNo>
</ImportAgrResponseX>";
			var filePath = Path.Combine(TestReceiverController.SettingManager.CurrentSetting.AcdaReceiveFolder, "success_1234_567890.xml");
			using (var writer = new StreamWriter(filePath))
			{
				writer.Write(testXml);
			}

			TestReceiverController.Test_ProcessCore();
			var achivedFileText = File.ReadAllText(Path.Combine(TestReceiverController.SettingManager.CurrentSetting.AcdaArchiveFolder, "success_1234_567890.xml"));
			AssertMultilineASCIIEquals("Should have archived.", testXml, achivedFileText);
		}

		public void TestProcess_AcdaFolderNotSet()
		{
			var testItem_AcdaFolderNotSet = new CNSWMessageReceiverControllerForTesting(CNSWSettingManagerForTesting.AcdaFolderNotSetMachine, cts.Token);
			var testLoggerAcdaFolderNotSet = new StringBuilder();
			testItem_AcdaFolderNotSet.ShowInformation += (o, e) => testLoggerAcdaFolderNotSet.AppendLine(e.Message);

			var testXml = @"<?xml version=""1.0"" encoding=""UTF-8"" standalone =""yes"" ?>
<DEC_RESULT>
  <CUS_CIQ_NO>I20180000147015879</CUS_CIQ_NO>
  <ENTRY_ID></ENTRY_ID>
  <NOTICE_DATE>2018-12-12T14:40:43</NOTICE_DATE>
  <CHANNEL>7</CHANNEL>
  <NOTE>I20180000147015879,I20180000147015879直接申报成功</NOTE>
  <CUSTOM_MASTER></CUSTOM_MASTER>
  <I_E_DATE></I_E_DATE>
  <D_DATE></D_DATE>
</DEC_RESULT>";

			using (var writer = new StreamWriter(Path.Combine(testItem_AcdaFolderNotSet.SettingManager.CurrentSetting.ReceiveFolder, "RetrievedFile.xml")))
			{
				writer.Write(testXml);
			}

			testItem_AcdaFolderNotSet.Test_ProcessCore();
			Assert(testItem_AcdaFolderNotSet.SettingManager.SendStreamCalledForTesting);
			AssertContains("Normal messages should be processed without ACDA settings.", "sent to eHub, RecipientID: ENTCMPSVR. Archived", testLoggerAcdaFolderNotSet.ToString());
			CustomseHubTestHelper.AssertTextEqualsIgnoreXmlFormats("", testXml, testItem_AcdaFolderNotSet.SettingManager.SentContentForTesting);
		}

		public void TestProcess_BackupWhenArchivingFailes()
		{
			var testXml = @"
<ImportAgrResponse>
	<ResponseInfo>
		<ResponseCode>1</ResponseCode>
		<ResponseMessage>导入成功</ResponseMessage>
		<CopCusCode>3117980008</CopCusCode>
	</ResponseInfo>
	<ConsignNo>20212243495988986</ConsignNo>
</ImportAgrResponse>";
			var filePath = Path.Combine(TestReceiverController.SettingManager.CurrentSetting.AcdaReceiveFolder, "success_1234_567890.xml");
			using (var writer = new StreamWriter(filePath))
			{
				writer.Write(testXml);
			}

			var acdaArchiveFolder = TestReceiverController.SettingManager.CurrentSetting.AcdaArchiveFolder;
			Directory.Move(acdaArchiveFolder, acdaArchiveFolder + "_1");
			TestReceiverController.Test_ProcessCore();

			AssertEquals("To make sure archiving failed.", false, File.Exists(Path.Combine(TestReceiverController.SettingManager.CurrentSetting.AcdaArchiveFolder, "success_1234_567890.xml")));
			AssertMultilineASCIIEquals("Should be backed up correctly.", testXml, File.ReadAllText(Path.Combine(TestReceiverController.SettingManager.CurrentSetting.AcdaReceiveFolder, "success_1234_567890.xml.bak")));

			Directory.Move(acdaArchiveFolder + "_1", acdaArchiveFolder);
			AssertEquals("To make sure acdaArchiveFolder_1 moved back.", false, Directory.Exists(acdaArchiveFolder + "_1"));
			AssertEquals("To make sure acdaArchiveFolder_1 moved back.", true, Directory.Exists(acdaArchiveFolder));
		}

		CNSWMessageReceiverControllerForTesting TestReceiverController;
		StringBuilder TestLogger;
		CancellationTokenSource cts;

		protected override void SetUp()
		{
			base.SetUp();
			cts = new CancellationTokenSource();
			TestLogger = new StringBuilder();
			TestReceiverController = new CNSWMessageReceiverControllerForTesting(cts.Token);
			TestReceiverController.ShowInformation += (o, e) => TestLogger.AppendLine(e.Message);

			CNSWTestHelper.DeleteTestFolders();
			CNSWTestHelper.CreateTestFolders();
		}

		protected override void TearDown()
		{
			try
			{
				cts.Cancel();
			}
			finally
			{
				cts.Dispose();
			}
			CNSWTestHelper.DeleteTestFolders();
			TestLogger.Clear();
			base.TearDown();
		}
	}
}
