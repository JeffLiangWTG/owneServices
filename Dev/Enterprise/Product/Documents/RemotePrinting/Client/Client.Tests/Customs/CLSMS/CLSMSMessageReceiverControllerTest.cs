using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Enterprise.xTMessaging.Shared;
using NUnit.Framework;

namespace Enterprise.RemotePrinting.Client.Tests
{
	public class CLSMSMessageReceiverControllerTest : TestCase
	{
		public class CLSMSMessageReceiverControllerForTesting : CLSMSMessageReceiverController
		{
			public CLSMSMessageReceiverControllerForTesting(CancellationToken cancellationToken) : base(CLSMSSettingManagerForTesting.CorrectMachine, cancellationToken) { }

			protected override CustomseHubClientSettingManager CreateNewSettingManager(string machineName)
			{
				return new CLSMSSettingManagerForTesting(machineName, CLSMSTestHelper.CreateFakeWebClient(CLSMSSettingManagerForTesting.CorrectMachine));
			}

			public new CLSMSSettingManagerForTesting SettingManager => (CLSMSSettingManagerForTesting)base.SettingManager;

			protected override WebClientConfiguration GetNewConfigSetting(string configName) => new ();

			public void Test_ProcessCore()
			{
				base.ProcessCore(SettingManager.CurrentSetting);
			}

			protected override void InitializeDXTConnector(DirectxTConnector connector)
			{
			}

			protected override (bool, long, string) SendMessage(DirectxTConnector connector, BasicXtMessageInfo xtMessageInfo)
			{
				return (true, 1, string.Empty);
			}

			protected override string GetDestinationParty(ICLSMSClientApplicationSetting clsmsSetting)
			{
				return "DATTest";
			}
		}

		public void TestProcess()
		{
			var testXml = @"<Documento>Test Message</Documento>";

			var acceptedFolder = TestReceiverController.SettingManager.CurrentSetting.AcceptedFolder;
			using (var writer = new StreamWriter(Path.Combine(acceptedFolder, "AcceptedFile.xml")))
			{
				writer.Write(testXml);
			}

			Assert(File.Exists(Path.Combine(acceptedFolder, "AcceptedFile.xml")));
			TestReceiverController.Test_ProcessCore();
			Assert("After message sent, the file renamed.", !File.Exists(Path.Combine(acceptedFolder, "AcceptedFile.xml")));

			var hasBackUpFile = Directory.EnumerateFiles(TestReceiverController.SettingManager.CurrentSetting.UnknownFolder).Any(x => Path.GetFileName(x).Contains("AcceptedFile"));
			Assert("After message sent, the file move UnknownFolder as backup and renamed", hasBackUpFile);
		}

		CLSMSMessageReceiverControllerForTesting TestReceiverController;
		StringBuilder TestLogger;
		CancellationTokenSource cts;

		protected override void SetUp()
		{
			base.SetUp();
			cts = new CancellationTokenSource();
			TestLogger = new StringBuilder();
			TestReceiverController = new CLSMSMessageReceiverControllerForTesting(cts.Token);
			TestReceiverController.ShowInformation += (o, e) => TestLogger.AppendLine(e.Message);

			CLSMSTestHelper.DeleteTestFolders();
			CLSMSTestHelper.CreateTestFolders();
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

			CLSMSTestHelper.DeleteTestFolders();
			TestLogger.Clear();
			base.TearDown();
		}
	}
}
