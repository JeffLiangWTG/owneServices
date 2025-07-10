using System.Text;
using NUnit.Framework;

namespace Enterprise.RemotePrinting.Client.Tests
{
	public class CNSWSettingManagerForTesting : CNSWClientApplicationSettingManager, IEHubClientSettingManagerForTesting
	{
		public const string EmptyMachine = "EmptyMachine";

		public const string WrongMachine = "WrongMachine";

		public const string CorrectMachine = "CorrectMachine";

		public const string AcdaFolderNotSetMachine = "AcdaFolderNotSetMachine";

		public bool SendStreamCalled { get; set; }

		public CNSWSettingManagerForTesting(string localMachineName, WebClient webServiceClient) : base(localMachineName, webServiceClient)
		{
		}

		public bool SendStreamCalledForTesting => fSendStreamCalled;
		bool fSendStreamCalled;
		public void Set_SendStreamCalled_ForTesting(bool called)
		{
			fSendStreamCalled = called;
		}

		public string SentContentForTesting => fSentContent;
		string fSentContent;
		public void Set_SentContent_ForTesting(string content)
		{
			fSentContent = content;
		}

		protected override CustomseHubServiceClientProxy CreateEHubServiceClient(string serverAddress, string clientId, string password)
		{
			return new CustomseHubServiceClientProxyForTesting(this, serverAddress, clientId, password);
		}
	}

	public class CNSWClientApplicationSettingManagerTest : TestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			CNSWTestHelper.CreateTestFolders();
		}

		protected override void TearDown()
		{
			CNSWTestHelper.DeleteTestFolders();
			base.TearDown();
		}

		public void TestGetSetting_EmptyMachine()
		{
			var logBuilder = new StringBuilder();

			var manager = new CNSWSettingManagerForTesting(CNSWSettingManagerForTesting.EmptyMachine, CNSWTestHelper.CreateFakeWebClient());
			manager.LogInformation += (o, e) => logBuilder.AppendLine(e.Message);
			manager.OnSettingDownloaded += CustomseHubTestHelper.OnSettingDownloaded;

			_ = manager.CurrentSetting;
			AssertMultilineASCIIEquals("Empty Setting", @"CNSW client application setting downloaded:
	MachineName:
	RunningIntervalInSeconds:0
	EHubGatewayServerAddress:
	EHubClientID:
	EHubClientStatus:
	EHubClientPassword:
	ArchiveFolder:
	ReceiveFolder:
	SendFolder:
	ErrorResponseFolder:
Validating CNSW client application setting:
	CNSW Setting is empty. If you need the feature of sending message to China Customs Single Window, please open Cargo Wise One, go to Registry > Customs > China > Single Window Client Application Settings and set it.",
				logBuilder.Replace("**********", "").ToString());
		}

		public void TestGetSetting_WrongMachine()
		{
			var logBuilder = new StringBuilder();

			var manager = new CNSWSettingManagerForTesting(CNSWSettingManagerForTesting.WrongMachine, CNSWTestHelper.CreateFakeWebClient());
			manager.LogInformation += (o, e) => logBuilder.AppendLine(e.Message);
			manager.OnSettingDownloaded += CustomseHubTestHelper.OnSettingDownloaded;

			_ = manager.CurrentSetting;
			AssertMultilineASCIIEquals("Wrong Setting", @"CNSW client application setting downloaded:
	MachineName:WrongMachine
	RunningIntervalInSeconds:-1
	EHubGatewayServerAddress:ehub.exception.address
	EHubClientID:ENTCMPSVR
	EHubClientStatus:OK
	EHubClientPassword:**********
	ArchiveFolder:ZZZ:\Archive_Folder_That_Does_Not_Exist
	ReceiveFolder:ZZZ:\Receive_Folder_That_Does_Not_Exist
	SendFolder:ZZZ:\Sender_Folder_That_Does_Not_Exist
	ErrorResponseFolder:ZZZ:\Err_Folder_That_Does_Not_Exist
Validating CNSW client application setting:
	Connecting to server: ehub.exception.address failed Exception thrown when trying to get response from: ehub.exception.address.
	Access to Send Folder 'ZZZ:\Sender_Folder_That_Does_Not_Exist' failed, Invalid name. Parameter name: name
	Access to Receive Folder 'ZZZ:\Receive_Folder_That_Does_Not_Exist' failed, Invalid name. Parameter name: name
	Access to Error Response Folder 'ZZZ:\Err_Folder_That_Does_Not_Exist' failed, Invalid name. Parameter name: name
	Access to Archive Folder 'ZZZ:\Archive_Folder_That_Does_Not_Exist' failed, Invalid name. Parameter name: name",
				logBuilder.ToString());
		}

		public void TestGetSetting_CorrectConfigs()
		{
			var logBuilder = new StringBuilder();
			CNSWTestHelper.CreateTestFolders();

			var manager = new CNSWSettingManagerForTesting(CNSWSettingManagerForTesting.CorrectMachine, CNSWTestHelper.CreateFakeWebClient());
			manager.LogInformation += (o, e) => logBuilder.AppendLine(e.Message);
			manager.OnSettingDownloaded += CustomseHubTestHelper.OnSettingDownloaded;

			var setting = manager.CurrentSetting;
			AssertContains("Correct Setting", @"CNSW client application setting is valid", logBuilder.ToString());
			Assert("Should have returned the new setting.", setting.IsValid && setting.MachineName == CustomseHubServiceClientProxyForTesting.CorrectCNSWClientSetting.MachineName && setting.EHubClientID == CustomseHubServiceClientProxyForTesting.CorrectCNSWClientSetting.EHubClientID);
			CNSWTestHelper.DeleteTestFolders();

			setting = manager.CurrentSetting;
			logBuilder.Clear();
			AssertSame(setting, manager.CurrentSetting);
			AssertNotContains("Should have cached", "CNSW client application setting downloaded", logBuilder.ToString());

			logBuilder.Clear();
			manager.ClearCachedSetting();
			setting = manager.CurrentSetting;
			AssertContains("Should have redownloaded", "CNSW client application setting downloaded", logBuilder.ToString());
			AssertSame("Should have cached", setting, manager.CurrentSetting);
		}

		public void TestGetSetting_AcdaFoldersNotSet()
		{
			var logBuilder = new StringBuilder();

			var manager = new CNSWSettingManagerForTesting(CNSWSettingManagerForTesting.AcdaFolderNotSetMachine, CNSWTestHelper.CreateFakeWebClient());
			manager.LogInformation += (o, e) => logBuilder.AppendLine(e.Message);
			manager.OnSettingDownloaded += CustomseHubTestHelper.OnSettingDownloaded;

			_ = manager.CurrentSetting;
			AssertMultilineASCIIEquals(
				"Almost correct configs only ACDA folders not set yet.",
				$@"CNSW client application setting downloaded:
	MachineName:AcdaFolderNotSetMachine
	RunningIntervalInSeconds:20
	EHubGatewayServerAddress:ehub.correct.address
	EHubClientID:ENTCMPSVR_CSW
	EHubClientStatus:OK
	EHubClientPassword:**********
	ArchiveFolder:{CNSWTestHelper.TestArchiveFolder}
	ReceiveFolder:{CNSWTestHelper.TestReceiveFolder}
	SendFolder:{CNSWTestHelper.TestSendFolder}
	ErrorResponseFolder:{CNSWTestHelper.TestErrorResponseFolder}
Validating CNSW client application setting:
	CNSW client application setting is valid",
				logBuilder.ToString()
			);
		}
	}
}
