using System.Text;
using NUnit.Framework;

namespace Enterprise.RemotePrinting.Client.Tests
{
	public class CLSMSSettingManagerForTesting : CLSMSClientApplicationSettingManager, IEHubClientSettingManagerForTesting
	{
		public const string EmptyMachine = "EmptyMachine";

		public const string WrongMachine = "WrongMachine";

		public const string CorrectMachine = "CorrectMachine";

		public bool SendStreamCalled { get; set; }

		public CLSMSSettingManagerForTesting(string localMachineName, WebClient webServiceClient) : base(localMachineName, webServiceClient)
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

	public class CLSMSClientApplicationSettingManagerTest : TestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			CLSMSTestHelper.CreateTestFolders();
		}

		protected override void TearDown()
		{
			CLSMSTestHelper.DeleteTestFolders();
			base.TearDown();
		}

		public void TestGetSetting_WrongMachine()
		{
			var logBuilder = new StringBuilder();
			var testItem = new CLSMSSettingManagerForTesting(CLSMSSettingManagerForTesting.WrongMachine, CLSMSTestHelper.CreateFakeWebClient(CLSMSSettingManagerForTesting.WrongMachine));
			testItem.LogInformation += (o, e) => logBuilder.AppendLine(e.Message);
			_ = testItem.CurrentSetting;
			AssertContains("Wrong Setting",
				@"Access to Send To Folder 'ZZZ:\Sender_Folder_That_Does_Not_Exist' failed, Invalid name. Parameter name: name
	Access to Receive Folder 'ZZZ:\Receive_Folder_That_Does_Not_Exist' failed, Invalid name. Parameter name: name
	Access to Invalid Folder 'ZZZ:\Invalid_Folder_That_Does_Not_Exist' failed, Invalid name. Parameter name: name
	Access to Rejected Folder 'ZZZ:\Rejected_Folder_That_Does_Not_Exist' failed, Invalid name. Parameter name: name
	Access to Unknown Folder 'ZZZ:\Unknown_Folder_That_Does_Not_Exist' failed, Invalid name. Parameter name: name
	Access to Accepted Folder 'ZZZ:\Accepted_Folder_That_Does_Not_Exist' failed, Invalid name. Parameter name: name",
				logBuilder.ToString());
		}

		public void TestGetSetting_CorrectConfigs()
		{
			var logBuilder = new StringBuilder();
			CLSMSTestHelper.CreateTestFolders();
			var testItem = new CLSMSSettingManagerForTesting(CLSMSSettingManagerForTesting.CorrectMachine, CLSMSTestHelper.CreateFakeWebClient(CLSMSSettingManagerForTesting.CorrectMachine));
			testItem.LogInformation += (o, e) => logBuilder.AppendLine(e.Message);

			var setting = testItem.CurrentSetting;
			AssertContains("Correct Setting", @"CLSMS client application setting is valid", logBuilder.ToString());
			Assert("Should have returned the new setting.", setting.IsValid && setting.MachineName == CustomseHubServiceClientProxyForTesting.CorrectCNSWClientSetting.MachineName);
			CLSMSTestHelper.DeleteTestFolders();

			setting = testItem.CurrentSetting;
			logBuilder.Clear();
			AssertSame(setting, testItem.CurrentSetting);
			AssertNotContains("Should have cached", "CLSMS client application setting downloaded", logBuilder.ToString());

			logBuilder.Clear();
			testItem.ClearCachedSetting();
			setting = testItem.CurrentSetting;
			AssertContains("Should have redownloaded", "CLSMS client application setting downloaded", logBuilder.ToString());
			AssertSame("Should have cached", setting, testItem.CurrentSetting);
		}
	}
}
