using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;

namespace Enterprise.RemotePrinting.Client.Tests
{
	public class TWNCATKSettingManagerForTesting : TWNCATKClientApplicationSettingManager, IEHubClientSettingManagerForTesting
	{
		public const string CorrectMachineName = "CorrectMachineName";

		public const string EmptySettingMachine = "EmptySettingMachine";

		public TWNCATKSettingManagerForTesting(string localMachineName, WebClient webServiceClient) : base(localMachineName, webServiceClient) { }

		public static string TextForRetrieveStream { get => fTextForRetrieveStream ?? (fTextForRetrieveStream = ""); set => fTextForRetrieveStream = value; }
		[ThreadStatic]
		static string fTextForRetrieveStream;

		bool IEHubClientSettingManagerForTesting.SendStreamCalledForTesting => true;
		void IEHubClientSettingManagerForTesting.Set_SendStreamCalled_ForTesting(bool called) { }
		string IEHubClientSettingManagerForTesting.SentContentForTesting => "";
		void IEHubClientSettingManagerForTesting.Set_SentContent_ForTesting(string content) { }

		protected override CustomseHubServiceClientProxy CreateEHubServiceClient(string serverAddress, string clientId, string password)
		{
			return new CustomseHubServiceClientProxyForTesting(this, serverAddress, clientId, password, TextForRetrieveStream);
		}
	}

	public class TWNCATKClientApplicationSettingManagerTest : TestCase
	{
		public void TestTWNCATKSetting()
		{
			var mockedClient = TWNCATKTestHelper.CreateFakeWebClient();
			var logger = new StringBuilder();

			var manager = new TWNCATKSettingManagerForTesting("CorrectMachineName", mockedClient);
			manager.LogInformation += (s, e) => logger.AppendLine(e.Message);
			manager.OnSettingDownloaded += CustomseHubTestHelper.OnSettingDownloaded;

			var setting = manager.CurrentSetting as ITWNCATKClientApplicationSetting;
			Assert("Should returned the correct setting.",
				setting.IsValid
				&& setting.MachineName == TWNCATKSettingManagerForTesting.CorrectMachineName
				&& setting.EHubClientID == "ID"
				&& setting.EHubClientPassword == "PWD"
				&& setting.EHubClientStatus == "OK"
				&& setting.EHubGatewayServerAddress == CustomseHubServiceClientProxyForTesting.Test_CorrectAddress
				&& setting.RunningIntervalInSeconds == 20
				&& setting.SendToFolder == Path.Combine(CustomseHubTestHelper.TestFolder, "TWNCATK", "SendToFolder")
			);
			AssertContains("Log for correct setting", "NCATK client application setting is valid", logger.ToString());

			logger.Clear();
			var emptySettingManager = new TWNCATKSettingManagerForTesting(TWNCATKSettingManagerForTesting.EmptySettingMachine, mockedClient);
			emptySettingManager.LogInformation += (s, e) => logger.AppendLine(e.Message);
			var emptySetting = emptySettingManager.CurrentSetting;
			AssertContains("Log for empty setting",
				"NCATK Setting is empty. If you need the feature of sending message to Taiwan Customs, please open Cargo Wise One, go to Registry > Customs > Taiwan > NCATK Message Sending Configuration and set it.",
				logger.ToString()
			);
			Assert("Setting should not be valid", !emptySetting.IsValid);
		}

		protected override void SetUp()
		{
			base.SetUp();
			CustomseHubTestHelper.CreateTestFolders("TWNCATK", new List<string> { "SendToFolder" });
		}

		protected override void TearDown()
		{
			CustomseHubTestHelper.DeleteTestFolders("TWNCATK");
			base.TearDown();
		}
	}
}
