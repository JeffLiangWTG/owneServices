using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using CargoWise.eHub.Common.Extensions;
using NUnit.Framework;

namespace Enterprise.RemotePrinting.Client.Tests
{
	sealed class CNSWMessageSenderControllerTest : TestCase
	{
		public class CNSWMessageSenderControllerForTesting : CNSWMessageSenderController
		{
			public CNSWMessageSenderControllerForTesting(CancellationToken cancellationToken) : base(CNSWSettingManagerForTesting.CorrectMachine, cancellationToken) { }

			protected override CustomseHubClientSettingManager CreateNewSettingManager(string machineName)
			{
				return new CNSWSettingManagerForTesting(machineName, CNSWTestHelper.CreateFakeWebClient());
			}

			protected override WebClientConfiguration GetNewConfigSetting(string configName) => new ();

			protected override void Process()
			{
				base.Process();
				ShouldStop = true;
			}

			public void Test_ProcessResponseMessage(Stream responseStream, string trackingId, string fileName)
			{
				ProcessResponseMessage(responseStream, trackingId, fileName, (CNSWClientApplicationSettingWrapper)SettingManager.CurrentSetting);
			}
		}

		const string OutputFileName = "CNSWMessageSenderControllerTest.TestProcessResponseMessage.Output";

		const string ExpectedOutputPath = "Enterprise.RemotePrinting.Client.Tests.Customs.TestFiles.ExpectedOutput.";

		public void TestPlainTextZip()
		{
			var painTextFileName = Path.Combine(CNSWTestHelper.TestArchiveFolder, OutputFileName + ".txt");
			using (var txtStream = GetInputStream("PlainText.zip"))
			{
				TestItem.Test_ProcessResponseMessage(txtStream, "TEST_TrackingID", OutputFileName);
			}
			CustomseHubTestHelper.AssertText("", painTextFileName, ExpectedOutputPath + "PainText.txt");
		}

		public void TestDecMessageZip()
		{
			var decOutputFileName = Path.Combine(CNSWTestHelper.TestSendFolder, OutputFileName + ".xml");
			using (var decStream = GetInputStream("DecMessage.zip"))
			{
				TestItem.Test_ProcessResponseMessage(decStream, "TEST_TrackingID", OutputFileName);
			}
			CustomseHubTestHelper.AssertXmlText(decOutputFileName, ExpectedOutputPath + "DecMessage.xml");
		}

		public void TestGmiMessageWithDecZip()
		{
			var decOutputFileName = Path.Combine(CNSWTestHelper.TestSendFolder, OutputFileName + ".xml");
			using (var gmiStreamWithDec = GetInputStream("GmiMessageWithDec.zip"))
			{
				TestItem.Test_ProcessResponseMessage(gmiStreamWithDec, "TEST_TrackingID", OutputFileName);
			}
			CustomseHubTestHelper.AssertXmlText(decOutputFileName, ExpectedOutputPath + "DecMessage.xml");
			CustomseHubTestHelper.AssertFileEncoding(decOutputFileName, new UTF8Encoding(false));
		}

		public void TestGmiMessageWithZip()
		{
			var zipOutputFileName = Path.Combine(CNSWTestHelper.TestSendFolder, OutputFileName + ".zip");
			using (var gmiStreamWithZip = GetInputStream("GmiMessageWithZip.zip"))
			{
				TestItem.Test_ProcessResponseMessage(gmiStreamWithZip, "TEST_TrackingID", OutputFileName);
			}
			CustomseHubTestHelper.AssertZipEquals("Output zip file should have same bytes as expected.", zipOutputFileName, ExpectedOutputPath + "ZippedMessage.zip");
		}

		public void TestImportAgrRequest()
		{
			var importAgrRequestOutPutFielName = Path.Combine(CNSWTestHelper.TestAcdaSendFolder, "59851.xml");
			using (var importAgrRequestStream = GetInputStream("ImportAgrRequest.zip"))
			{
				TestItem.Test_ProcessResponseMessage(importAgrRequestStream, "TEST_TrackingID", OutputFileName);
			}
			CustomseHubTestHelper.AssertXmlText(importAgrRequestOutPutFielName, ExpectedOutputPath + "ImportAgrRequest.xml");
			CustomseHubTestHelper.AssertFileEncoding(importAgrRequestOutPutFielName, new UTF8Encoding(false));
		}

		Stream GetInputStream(string fileName)
		{
			return TestAssebmly.GetManifestResourceStream($@"Enterprise.RemotePrinting.Client.Tests.Customs.TestFiles.Input.{fileName}").EncodeStream();
		}

		CNSWMessageSenderControllerForTesting TestItem;
		StringBuilder Logger;
		Assembly TestAssebmly;
		CancellationTokenSource cts;

		protected override void SetUp()
		{
			base.SetUp();
			cts = new CancellationTokenSource();
			CNSWTestHelper.DeleteTestFolders();
			CNSWTestHelper.CreateTestFolders();
			Logger = new StringBuilder();
			TestItem = new CNSWMessageSenderControllerForTesting(cts.Token);
			TestItem.ShowInformation += (s, e) => Logger.AppendLine(e.Message);

			TestAssebmly = Assembly.GetExecutingAssembly();
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
			base.TearDown();
		}
	}
}
