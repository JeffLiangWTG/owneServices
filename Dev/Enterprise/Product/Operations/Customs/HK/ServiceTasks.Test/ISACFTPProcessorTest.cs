using System;
using System.IO;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO.Testing;
using CargoWise.Types;
using Enterprise.Customs.HK.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Customs.HK.ServiceTasks.Testing
{
	class ISACFTPProcessorTest : TestCaseWithFactory
	{
		public void TestSendWithInvalidOutputPath()
		{
			using (eHubMessagingRegistry.Instance.SendHKISACEViaEHub.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (HKDataRegistry.Instance.HKTraxonOutputDirectory.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ""))
			using (HKDataRegistry.Instance.ISACFTPServerOutputAddress.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, " :\\//s"))
			{
				var interchange = Factory.New<EDIInterchange>();
				interchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.Traxon;
				interchange.EI_Status = EDIInterchange.Status.Queued;
				interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
				interchange.EI_HeaderText = "HEADERTEXT";
				interchange.EI_BodyText = "BODYTEXT";
				interchange.EI_FooterText = "FOOTERTEXT";
				interchange.EI_InterchangeNum = "12345";
				interchange.EI_GB = GlbBranch.CurrentBranch.PK;
				Factory.Save();

				sender.ExecuteBatch();
				ZString ftpSettingLocation = ((IRegistryItemInternals)HKDataRegistry.Instance.ISACFTPServerOutputAddress).Location;
				ftpSettingLocation = ftpSettingLocation.Left(ftpSettingLocation.LastIndexOf(" -> "));
				AssertContains(string.Format(ISACFTPProcessor.FTPOutputPathError, ((IRegistryItemInternals)HKDataRegistry.Instance.ISACFTPServerOutputAddress).Location, ftpSettingLocation), sender.Logger.UserLogStrings[0]);
			}
		}

		public void TestUploadViaFTPToFTPServerWithoutSub()
		{
			Directory.Delete(Path.Combine(ftpTestHelper.LocalDirectory, FtpFolderName));
			AssertUploadViaFTP(ftpTestHelper.LocalDirectory, "");
		}

		public void TestUploadViaFTPToFTPServerWithSub()
		{
			AssertUploadViaFTP(Path.Combine(ftpTestHelper.LocalDirectory, FtpFolderName), FtpFolderName);
		}

		public void TestUploadViaFTPToFTPServerWithSubInSub()
		{
			var ftpDirectory = Path.Combine(ftpTestHelper.LocalDirectory, FtpFolderName + "\\Sub");
			Directory.CreateDirectory(ftpDirectory);
			AssertUploadViaFTP(ftpDirectory, FtpFolderName + "/Sub");
		}

		protected override void SetUp()
		{
			base.SetUp();
			ftpTestHelper = new FtpTestHelper(UserName, Password);
			ftpTestHelper.Start();
			Directory.CreateDirectory(Path.Combine(ftpTestHelper.LocalDirectory, FtpFolderName));
			HKDataRegistry.Instance.HKTraxonSenderID.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "RHKAGT021332880/HKG81");
			HKDataRegistry.Instance.HKTraxonRecipientReferencePassword.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "PRDAGENT027");
			sender = new Sender();
		}

		protected override void TearDown()
		{
			ftpTestHelper.Dispose();
			base.TearDown();
		}

		void AssertUploadViaFTP(string remotePath, string ftpServerOutputAddressSuffix)
		{
			var transmitMessage = Factory.New<TraxonMessage>();
			transmitMessage.EM_MessageText = EDIMessage.MessageNumberPlaceHolder;
			Factory.Save();
			using (eHubMessagingRegistry.Instance.SendHKISACEViaEHub.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (HKDataRegistry.Instance.HKTraxonOutputDirectory.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ""))
			using (HKDataRegistry.Instance.ISACFTPServerOutputAddress.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ftpTestHelper.ServerAddress.AbsoluteUri + ftpServerOutputAddressSuffix))
			using (HKDataRegistry.Instance.ISACFTPPassword.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Password))
			using (HKDataRegistry.Instance.ISACFTPUserName.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, UserName))
			{
				sender.ExecuteBatch();
				AssertEquals(3, sender.Logger.UserLogStrings.Count);
				AssertContains("1 new interchange(s) sent.", sender.Logger.UserLogStrings[2]);
				transmitMessage.Reload();
				AssertEquals("Message Status", EDIMessage.Status.Sent, transmitMessage.EM_Status);
				var interchange = transmitMessage.Interchange;
				AssertEquals("Interchange Status", EDIInterchange.Status.Sent, interchange.EI_Status);
				var remoteFile = Path.Combine(remotePath, interchange.EI_InterchangeNum + ".edi");
				var filesFound = Directory.GetFiles(remotePath);
				AssertEquals("Should have one file created in the target folder", 1, filesFound.Length);
				AssertEquals(remoteFile, filesFound[0]);

				interchange.EI_Status = EDIInterchange.Status.Queued;
				Factory.Save();
				sender.ExecuteBatch();
				AssertEquals(2, sender.Logger.UserLogStrings.Count);
				AssertContains("1 new interchange(s) sent.", sender.Logger.UserLogStrings[1]);
				transmitMessage.Reload();
				AssertEquals("Message Status", EDIMessage.Status.Sent, transmitMessage.EM_Status);
				interchange = transmitMessage.Interchange;
				interchange.Reload();
				AssertEquals("Interchange Status", EDIInterchange.Status.Sent, interchange.EI_Status);
				filesFound = Directory.GetFiles(remotePath);
				AssertEquals("Should have one file created in the target folder", 1, filesFound.Length);
				AssertEquals(remoteFile, filesFound[0]);
				using (var sr = File.Open(remoteFile, FileMode.Append))
				{
					interchange.EI_Status = EDIInterchange.Status.Queued;
					Factory.Save();
					sender.ExecuteBatch();
					AssertEquals(2, sender.Logger.UserLogStrings.Count);
					string Normalize(string input) => Regex.Replace(input, @"[\s]+", " ").Trim();
					string normalizedActual = Normalize(sender.Logger.UserLogStrings[0]);
					string normalizedExpected = Normalize("1.edi]. Error [CargoWise.IO.FtpException: Could not upload file after 5 tries with a 2 second pause between each. Giving up. ---> CargoWise.IO.FtpException: Fail uploading. Method: STOR ---> System.Net.WebException: The remote server returned an error: (451) Local error in processing. at System.Net.FtpWebRequest.SyncRequestCallback(Object obj)");
					AssertContains(normalizedExpected, normalizedActual);
					transmitMessage.Reload();
					AssertEquals("Message Status", EDIMessage.Status.Sent, transmitMessage.EM_Status);
					interchange = transmitMessage.Interchange;
					interchange.Reload();
					AssertEquals("Interchange Status", EDIInterchange.Status.Failed, interchange.EI_Status);
					filesFound = Directory.GetFiles(remotePath);
					AssertEquals("Should have one file created in the target folder", 1, filesFound.Length);
					AssertEquals(remoteFile, filesFound[0]);
				}
			}
		}

		Sender sender;
		FtpTestHelper ftpTestHelper;
		const string FtpFolderName = "Test";
		const string UserName = "testuser";
		const string Password = "testpwd";
	}
}
