using Enterprise.RemotePrinting.Client.RemotePrintServer;
using NUnit.Framework;

namespace Enterprise.RemotePrinting.Client.Tests
{
	sealed class CLSMSClientApplicationSettingWrapperTest : TestCase
	{
		public void TestProperties()
		{
			var testItem = new CLSMSClientApplicationSettingWrapper(new CLSMSClientSetting()
			{
				ServerAddress = "localhost",
				ServerCertificate =  "Certificate",
				RegistrationKey = "Key",
				ApplicationNodeName = "Name",
				DecryptedApplicationNodePassword = "Password",
				RunningIntervalInSeconds = 99,
				SendFolder = "Send Folder",
				ReceiveFolder = "Receive Folder",
				AcceptedFolder = "Accepted Folder",
				RejectedFolder = "Rejected Folder",
				InvalidFolder = "Invalid Folder",
				UnknownFolder = "Unknown Folder",
				MachineName = "Machine Name"
			});

			CombineAssertions(() =>
			{
				AssertEquals("localhost", testItem.ServerAddress);
				AssertEquals("Certificate", testItem.CertificateForTheServer);
				AssertEquals("Machine Name", testItem.MachineName);
				AssertEquals("Key", testItem.CW1LicenseKey);
				AssertEquals("Password", testItem.ApplicationNodePassword);
				AssertEquals("Name", testItem.ApplicationNodeName);
				AssertEquals(99, testItem.RunningIntervalInSeconds);
				AssertEquals("Send Folder", testItem.SendFolder);
				AssertEquals("Receive Folder", testItem.ReceiveFolder);
				AssertEquals("Accepted Folder", testItem.AcceptedFolder);
				AssertEquals("Rejected Folder", testItem.RejectedFolder);
				AssertEquals("Invalid Folder", testItem.InvalidFolder);
				AssertEquals("Unknown Folder", testItem.UnknownFolder);
			});
		}
	}
}
