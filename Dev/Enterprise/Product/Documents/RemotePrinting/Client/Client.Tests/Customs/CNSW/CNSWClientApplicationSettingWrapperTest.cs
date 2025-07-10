using Enterprise.RemotePrinting.Client.RemotePrintServer;
using NUnit.Framework;

namespace Enterprise.RemotePrinting.Client.Tests
{
	public class CNSWClientApplicationSettingWrapperTest : TestCase
	{
		public void TestProperties()
		{
			var testItem = new CNSWClientApplicationSettingWrapper(new CNSWClientSetting()
			{
				MachineName = "Machine Name",
				SendFolder = "Send Folder",
				ReceiveFolder = "Receive Folder",
				ErrorResponseFolder = "Error Response Folder",
				ArchiveFolder = "Archive Folder",
				RunningIntervalInSeconds = 99,
				EHubClientID = "eHub client ID",
				EHubClientStatus = "OK",
				DecryptedEHubClientPassword = "A very strong password",
				EHubGatewayServerAddress = "eHub Gateway Server Address"
			});

			CombineAssertions(() =>
			{
				AssertEquals("Machine Name", testItem.MachineName);
				AssertEquals("Send Folder", testItem.SendFolder);
				AssertEquals("Receive Folder", testItem.ReceiveFolder);
				AssertEquals("Error Response Folder", testItem.ErrorResponseFolder);
				AssertEquals("Archive Folder", testItem.ArchiveFolder);
				AssertEquals(99, testItem.RunningIntervalInSeconds);
				AssertEquals("eHub client ID", testItem.EHubClientID);
				AssertEquals("OK", testItem.EHubClientStatus);
				AssertEquals("A very strong password", testItem.EHubClientPassword);
				AssertEquals("eHub Gateway Server Address", testItem.EHubGatewayServerAddress);
			});
		}
	}
}
