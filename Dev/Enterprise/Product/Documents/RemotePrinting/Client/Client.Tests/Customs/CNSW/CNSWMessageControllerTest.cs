using System.Globalization;
using System.Text;
using System.Threading;
using NUnit.Framework;

namespace Enterprise.RemotePrinting.Client.Tests
{
	public class CNSWMessageControllerTest : TestCase
	{
		public void TestProcess_CorrectSetting()
		{
			var logger = new StringBuilder();

			var controller = new CNSWMessageControllerForTesting(CNSWSettingManagerForTesting.CorrectMachine, cts.Token);
			controller.ShowInformation += (o, e) => logger.AppendLine(e.Message);
			controller.Run();

			AssertContains("CNSW Message Sender Process Started.", logger.ToString());
			AssertContains("CNSW client application setting downloaded:", logger.ToString());
			AssertContains("CNSW client application setting is valid", logger.ToString());
			AssertContains("Pause 20 seconds", logger.ToString());
			AssertContains("CNSW Message Sender Process Stoped.", logger.ToString());
		}

		public void TestProcess_EmptySetting()
		{
			var logger = new StringBuilder();
			var controller = new CNSWMessageControllerForTesting(CNSWSettingManagerForTesting.EmptyMachine, cts.Token);

			controller.ShowInformation += (o, e) => logger.AppendLine(e.Message);
			controller.Run();

			AssertContains("CNSW Message Sender Process Started.", logger.ToString());
			AssertContains("CNSW client application setting downloaded:", logger.ToString());
			AssertContains("Validating CNSW client application setting:", logger.ToString());
			AssertContains("CNSW Setting is empty. If you need the feature of sending message to China Customs Single Window, please open Cargo Wise One, go to Registry > Customs > China > Single Window Client Application Settings and set it.", logger.ToString());
			AssertContains("Pause 1200 seconds.", logger.ToString());
			AssertContains("CNSW Message Sender Process Stoped.", logger.ToString());
		}

		public void TestProcess_WrongSetting()
		{
			var logger = new StringBuilder();
			var controller = new CNSWMessageControllerForTesting(CNSWSettingManagerForTesting.WrongMachine, cts.Token);
			controller.ShowInformation += (o, e) => logger.AppendLine(e.Message);
			controller.Run();

			AssertContains("CNSW Message Sender Process Started.", logger.ToString());
			AssertContains("CNSW client application setting downloaded:", logger.ToString());
			AssertContains("Validating CNSW client application setting:", logger.ToString());
			AssertContains("Exception thrown when trying to get response from: ehub.exception.address", logger.ToString());
			AssertContains("Pause 300 seconds.", logger.ToString());
			AssertContains("CNSW Message Sender Process Stoped.", logger.ToString());
		}

		public void TestLoggingEnablements()
		{
			var controller = new CNSWMessageControllerForTesting(CNSWSettingManagerForTesting.EmptyMachine, cts.Token);
			controller.Run();
			AssertEquals("Logging should NOT be enabled if there is no valid setting.", false, controller.LoggingEnable);

			controller = new CNSWMessageControllerForTesting(CNSWSettingManagerForTesting.CorrectMachine, cts.Token);
			controller.Run();
			AssertEquals("Logging should BE enabled for a valid setting.", true, controller.LoggingEnable);
		}

		CancellationTokenSource cts;

		protected override void SetUp()
		{
			base.SetUp();
			cts = new CancellationTokenSource();
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
			base.TearDown();
		}

		public class CNSWMessageControllerForTesting : CNSWMessageSenderController
		{
			public CNSWMessageControllerForTesting(string localMachineName, CancellationToken cancellationToken) : base(localMachineName, cancellationToken)
			{
			}

			protected override CustomseHubClientSettingManager CreateNewSettingManager(string machineName)
			{
				return new CNSWSettingManagerForTesting(machineName, CNSWTestHelper.CreateFakeWebClient());
			}

			protected override void InitialiseWebServiceClient()
			{
				OnShowInformation("Process Started");
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
			protected override void PauseWhenRequested(int requestPauseInSeconds)
			{
				OnShowInformation(string.Format(CultureInfo.InvariantCulture, "Pause {0} seconds.", requestPauseInSeconds));
				ShouldStop = true;
			}

			protected override int ProcessCore(ICustomseHubClientSetting setting)
			{
				return 0;
			}
		}
	}
}
