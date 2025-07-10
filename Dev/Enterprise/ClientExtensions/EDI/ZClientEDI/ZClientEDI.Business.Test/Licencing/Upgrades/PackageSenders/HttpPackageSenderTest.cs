using System;
using System.IO;
using System.Text;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Client.EDI.AutoDeploy.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.ReleaseBuilds.Business;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.MailManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.VersionInfo;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using WTG.DevTools.Definitions;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business.Test
{
	class HttpPackageSenderTest : TestCaseWithFactory
	{
		public void TestGetVersionInfo()
		{
			string fileName = Path.GetFileName(TestFilePath);
			HttpPackageSender genericSender = GetNewPackageSender("", "", CreateUpgrade(ReleaseRings.Codes.GPR));
			HttpPackageSender clientSpecificSender = GetNewPackageSender(TestEnterpriseCode, "", CreateUpgrade(ReleaseRings.Codes.GPR));
			AssertEquals("genericSender.GetVersionInfo().PackageURL", PackagePathGenerator.GenerateHttpPath("", fileName), genericSender.GetVersionInfo(fileName).PackageURL);
			AssertEquals("clientSpecificSender.GetVersionInfo().PackageURL", PackagePathGenerator.GenerateHttpPath(TestEnterpriseCode, fileName), clientSpecificSender.GetVersionInfo(fileName).PackageURL);
		}

		protected void TestRunCore()
		{
			MailItemCollection mailItems = new StandardMailItemCollection(Factory);
			mailItems.Load();

			AssertEquals("One email should be sent.", 1, mailItems.Count);
			AssertEquals("Email.MI_Status", MailManager.MailStatus.Queued, mailItems[0].MI_Status);
			AssertEquals("Email.MI_Subject", "ediEnterprise Version Info 20041230_1356", mailItems[0].MI_Subject);

			AssertEquals("Email should have 2 recipients.", 2, mailItems[0].MailRecipients.Count);
			AssertEquals("First Recipient Address", "testemailHTP@edi.com.au", mailItems[0].MailRecipients[0].EmailAddress);
			AssertEquals("Second Recipient Address", "testemail2@edi.com.au", mailItems[0].MailRecipients[1].EmailAddress);

			AssertEquals("Email should have one attachment.", 1, mailItems[0].MailAttachments.Count);
			AssertEquals("Email attachment name", "VersionInfo_20041230_1356.xml", mailItems[0].MailAttachments[0].MA_FileName);

			string xml = new UTF8Encoding().GetString(mailItems[0].MailAttachments[0].MA_Data);

			string expectedValue =
				"<?xml version=\"1.0\" encoding=\"utf-8\"?>" + System.Environment.NewLine +
				"<VersionInfo>" + System.Environment.NewLine +
				"  <ReleaseBuildPK>00000000-0000-0000-0000-000000000000</ReleaseBuildPK>" + System.Environment.NewLine +
				"  <ExeVersionDate>30-DEC-04 13:56</ExeVersionDate>" + System.Environment.NewLine +
				"  <MajorVersion>1</MajorVersion>" + System.Environment.NewLine +
				"  <MinorVersion>2</MinorVersion>" + System.Environment.NewLine +
				"  <Release>3</Release>" + System.Environment.NewLine +
				"  <Patch>4</Patch>" + System.Environment.NewLine +
				"  <Comment>Deployed via Web</Comment>" + System.Environment.NewLine +
				"  <ForceDownload>Y</ForceDownload>" + System.Environment.NewLine +
				"  <PackageURL>" + PackagePathGenerator.GenerateHttpPath(TestEnterpriseCode, "Package20041230_135600_1_2_3_4.edp") + "</PackageURL>" + System.Environment.NewLine +
				"</VersionInfo>";

			AssertEquals("Attachment XML", expectedValue, xml);
		}

		public void TestRun()
		{
			TestCaseHelper.ClearTable(MailDBItemsSchema.Constants.TableName);

			UpgradesToClient upgrade1 = GetTestUpgradesToClient(UpgradeMethods.Codes.Http);
			UpgradesToClient upgrade2 = GetTestUpgradesToClient(UpgradeMethods.Codes.Http);
			upgrade2.LicDatabase.LD_PublicEmailAddressForUpdate = "testemail2@edi.com.au";

			Factory.Save();

			DirectoryInfo baseDirectory = new DirectoryInfo(BasePath);
			string olderFile1 = Path.Combine(TestDirectory, "Package20041228_135600_1_2_3_4.edp");
			string olderFile2 = Path.Combine(TestDirectory, "Package20041229_135600_1_2_3_4.edp");
			string newerFile = Path.Combine(TestDirectory, "Package20041231_135600_1_2_3_4.edp");
			string otherFile = Path.Combine(TestDirectory, "Package20041229_135600_1_2_3_4.txt");
			string otherFile1 = Path.Combine(TestDirectory, "Package20041229_135600_1_2_3_4.edpbak");
			string targetFileName = Path.Combine(TestDirectory, "Package20041230_135600_1_2_3_4.edp");

			try
			{
				TryToGetControl();
				try
				{
					baseDirectory.Create();
					File.WriteAllBytes(TestFilePath, TestData);
					Directory.CreateDirectory(TestDirectory);
					CreateFile(olderFile1);
					CreateFile(olderFile2);
					File.SetCreationTime(olderFile1, ZDateTime.Now.AddDays(-8).ToDateTime());
					File.SetCreationTime(olderFile2, ZDateTime.Now.AddDays(-8).ToDateTime());
					CreateFile(newerFile);
					CreateFile(otherFile);
					CreateFile(otherFile1);

					File.SetCreationTime(olderFile1, ZDateTime.Now.AddDays(-15).ToDateTime());
					File.SetCreationTime(olderFile2, ZDateTime.Now.AddDays(-15).ToDateTime());
				}
				finally
				{
					TryToReleaseControl();
				}

				var sender = GetNewPackageSenderWithOverriddenTargetDirectory(TestDirectory, "!@#", TestFilePath, upgrade1, upgrade2);
				AssertEquals(true, sender.Run());
				AssertEquals(targetFileName + " should exist.", true, File.Exists(targetFileName));

				using (FileStream stream = File.OpenRead(targetFileName))
				{
					byte[] data = new byte[stream.Length];
					stream.Read(data, 0, (int)stream.Length);
					AssertEquals("Data", TestData, data);
				}

				AssertEquals("Older files should be deleted.", false, File.Exists(olderFile1));
				AssertEquals("Older files should be deleted.", false, File.Exists(olderFile2));
				AssertEquals("Newer files should not be deleted.", true, File.Exists(newerFile));
				AssertEquals("Files that do not have an .edp extension should not be deleted.", true, File.Exists(otherFile));
				AssertEquals("Files that have an .edp* extension should not be deleted because they do not match RegEx.", true, File.Exists(otherFile1));

				TestRunCore();
			}
			finally
			{
				TryToGetControl();
				try
				{
					DeleteDirectory(baseDirectory);
				}
				finally
				{
					TryToReleaseControl();
				}
			}
		}

		public void TestRun_SendViaSystemMessage()
		{
			var systemMessageCreator = new DebugOnlyOutgoingSystemMessage();
			ObjectFactory.Substitute<IOutgoingSystemMessage>(systemMessageCreator);

			EDIDataRegistry.Instance.AllSystemMessagesViaEhubReleaseBuilds.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "16.10.20.0");
			UpgradesToClient upgradeViaEmail = GetTestUpgradesToClient(UpgradeMethods.Codes.Http);
			UpgradesToClient upgradeViaEHub = GetTestUpgradesToClient(UpgradeMethods.Codes.Http);
			upgradeViaEmail.LicDatabase.LD_PublicEmailAddressForUpdate = "testemail1@edi.com.au";
			upgradeViaEHub.LicDatabase.LD_PublicEmailAddressForUpdate = "testemail2@edi.com.au";
			upgradeViaEHub.LicDatabase.CurrentVersion.VersionNumber = new VersionNumber(16, 10, 20, 0);

			Factory.Save();

			var targetDir = PackagePathGenerator.GenerateWebServerRootPath("ENT");
			var mockPublisher = new Mock<IPackagePublishService>();
			mockPublisher.Setup(m => m.PublishPackage(TestFilePath, targetDir)).Returns(true);
			mockPublisher.Setup(m => m.IsPackageAlreadyPublished(Path.GetFileName(TestFilePath), "ENT")).Returns(TriState.False);

			var sender = new HttpPackageSender("ENT", TestFilePath, upgradeViaEmail, upgradeViaEHub);
			sender.SetPackagePublishForTest(mockPublisher.Object);
			sender.Run();

			mockPublisher.VerifyAll();

			MailItemCollection mailItems = new StandardMailItemCollection(Factory);
			mailItems.Load();

			AssertEquals("Emails sent", 1, mailItems.Count);
			AssertEquals("Email.MI_Status", MailManager.MailStatus.Queued, mailItems[0].MI_Status);
			AssertEquals("Email.MI_Subject", "ediEnterprise Version Info 20041230_1356", mailItems[0].MI_Subject);

			AssertEquals("Email recipient #", 1, mailItems[0].MailRecipients.Count);
			AssertEquals("Recipient Address", "testemail1@edi.com.au", mailItems[0].MailRecipients[0].EmailAddress);

			AssertEquals("Email should have one attachment.", 1, mailItems[0].MailAttachments.Count);
			AssertEquals("Email attachment name", "VersionInfo_20041230_1356.xml", mailItems[0].MailAttachments[0].MA_FileName);

			string xml = new UTF8Encoding().GetString(mailItems[0].MailAttachments[0].MA_Data);
			string expectedUrl = PackagePathGenerator.GenerateHttpPath("ENT", "Package20041230_135600_1_2_3_4.edp");

			string expectedValue =
				"<?xml version=\"1.0\" encoding=\"utf-8\"?>" + System.Environment.NewLine +
				"<VersionInfo>" + System.Environment.NewLine +
				"  <ReleaseBuildPK>00000000-0000-0000-0000-000000000000</ReleaseBuildPK>" + System.Environment.NewLine +
				"  <ExeVersionDate>30-DEC-04 13:56</ExeVersionDate>" + System.Environment.NewLine +
				"  <MajorVersion>1</MajorVersion>" + System.Environment.NewLine +
				"  <MinorVersion>2</MinorVersion>" + System.Environment.NewLine +
				"  <Release>3</Release>" + System.Environment.NewLine +
				"  <Patch>4</Patch>" + System.Environment.NewLine +
				"  <Comment>Deployed via Web</Comment>" + System.Environment.NewLine +
				"  <ForceDownload>Y</ForceDownload>" + System.Environment.NewLine +
				"  <PackageURL>" + expectedUrl + "</PackageURL>" + System.Environment.NewLine +
				"</VersionInfo>";

			AssertEquals("Attachment XML", expectedValue, xml);

			var systemXml = DebugOnlyOutgoingSystemMessage.XmlMessageBody;

			string expectedSystemXml =
				"<UpgradeDownload>" + System.Environment.NewLine +
				"  <ExeVersionDate>30-DEC-04 13:56</ExeVersionDate>" + System.Environment.NewLine +
				"  <MajorVersion>1</MajorVersion>" + System.Environment.NewLine +
				"  <MinorVersion>2</MinorVersion>" + System.Environment.NewLine +
				"  <Release>3</Release>" + System.Environment.NewLine +
				"  <Patch>4</Patch>" + System.Environment.NewLine +
				"  <Comment>Deployed via Web</Comment>" + System.Environment.NewLine +
				"  <ForceDownload>Y</ForceDownload>" + System.Environment.NewLine +
				"  <PackageURL>" + expectedUrl + "</PackageURL>" + System.Environment.NewLine +
				"</UpgradeDownload>";

			AssertEquals("System XML", expectedSystemXml, systemXml);

			var downloadInfo = new PackageDownloadInfo(systemXml);
			AssertEquals(expectedUrl, downloadInfo.PackageURL);
			AssertEquals(new ZDateTime(2004, 12, 30, 13, 56, 0), downloadInfo.ExeVersionDate);
			AssertEquals(1, downloadInfo.MajorVersion);
			AssertEquals(2, downloadInfo.MinorVersion);
			AssertEquals(3, downloadInfo.Release);
			AssertEquals(4, downloadInfo.Patch);
			AssertEquals(true, downloadInfo.ForceDownload);
			AssertEquals("Deployed via Web", downloadInfo.Comment);
		}

		public void TestRun_IsPackageAlreadyPublished()
		{
			var systemMessageCreator = new DebugOnlyOutgoingSystemMessage();
			ObjectFactory.Substitute<IOutgoingSystemMessage>(systemMessageCreator);

			EDIDataRegistry.Instance.AllSystemMessagesViaEhubReleaseBuilds.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "16.10.20.0");
			UpgradesToClient upgradeViaEmail = GetTestUpgradesToClient(UpgradeMethods.Codes.Http);
			UpgradesToClient upgradeViaEHub = GetTestUpgradesToClient(UpgradeMethods.Codes.Http);
			upgradeViaEmail.LicDatabase.LD_PublicEmailAddressForUpdate = "testemail1@edi.com.au";
			upgradeViaEHub.LicDatabase.LD_PublicEmailAddressForUpdate = "testemail2@edi.com.au";
			upgradeViaEHub.LicDatabase.CurrentVersion.VersionNumber = new VersionNumber(16, 10, 20, 0);

			Factory.Save();

			PackagePathGenerator.GenerateWebServerRootPath("ENT");
			var mockPublisher = new Mock<IPackagePublishService>();
			mockPublisher.Setup(m => m.IsPackageAlreadyPublished(Path.GetFileName(TestFilePath), "ENT")).Returns(TriState.True);

			var sender = new HttpPackageSender("ENT", TestFilePath, upgradeViaEmail, upgradeViaEHub);
			sender.SetPackagePublishForTest(mockPublisher.Object);
			AssertEquals(true, sender.Run());

			mockPublisher.VerifyAll();
		}

		public void TestRun_ShouldSendEmptyUrlWhenDownloadSkipOptimizationApplied()
		{
			// Arrange
			var systemMessageCreator = new DebugOnlyOutgoingSystemMessage();
			ObjectFactory.Substitute<IOutgoingSystemMessage>(systemMessageCreator);

			EDIDataRegistry.Instance.AllSystemMessagesViaEhubReleaseBuilds.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "16.10.20.0");
			UpgradesToClient upgradeViaEmail = GetTestUpgradesToClient(UpgradeMethods.Codes.Http);
			upgradeViaEmail.LicDatabase.LD_PublicEmailAddressForUpdate = "testemail1@edi.com.au";
			upgradeViaEmail.LicDatabase.LD_EnablePackageDownloadOptimization = true;
			upgradeViaEmail.Build.HL_IsRolledOut = true;

			UpgradesToClient upgradeViaEHub = GetTestUpgradesToClient(UpgradeMethods.Codes.Http);
			upgradeViaEHub.LicDatabase.LD_PublicEmailAddressForUpdate = "testemail2@edi.com.au";
			upgradeViaEHub.LicDatabase.CurrentVersion.VersionNumber = new VersionNumber(16, 10, 20, 0);
			upgradeViaEHub.LicDatabase.LD_EnablePackageDownloadOptimization = true;
			upgradeViaEHub.Build.HL_IsRolledOut = true;

			Factory.Save();

			var targetDir = PackagePathGenerator.GenerateWebServerRootPath("ENT");
			var mockPublisher = new Mock<IPackagePublishService>();
			mockPublisher.Setup(m => m.PublishPackage(TestFilePath, targetDir)).Returns(true);
			mockPublisher.Setup(m => m.IsPackageAlreadyPublished(Path.GetFileName(TestFilePath), "ENT")).Returns(TriState.False);

			var sender = new HttpPackageSender("ENT", TestFilePath, upgradeViaEmail, upgradeViaEHub);
			sender.SetPackagePublishForTest(mockPublisher.Object);

			// Act
			sender.Run();

			// Assert
			MailItemCollection mailItems = new StandardMailItemCollection(Factory);
			mailItems.Load();

			string xml = new UTF8Encoding().GetString(mailItems[0].MailAttachments[0].MA_Data);
			string expectedValue =
				"<?xml version=\"1.0\" encoding=\"utf-8\"?>" + System.Environment.NewLine +
				"<VersionInfo>" + System.Environment.NewLine +
				"  <ReleaseBuildPK>00000000-0000-0000-0000-000000000000</ReleaseBuildPK>" + System.Environment.NewLine +
				"  <ExeVersionDate>30-DEC-04 13:56</ExeVersionDate>" + System.Environment.NewLine +
				"  <MajorVersion>1</MajorVersion>" + System.Environment.NewLine +
				"  <MinorVersion>2</MinorVersion>" + System.Environment.NewLine +
				"  <Release>3</Release>" + System.Environment.NewLine +
				"  <Patch>4</Patch>" + System.Environment.NewLine +
				"  <Comment>The package has been rolled out to CargoWise Cloud</Comment>" + System.Environment.NewLine +
				"  <ForceDownload>N</ForceDownload>" + System.Environment.NewLine +
				"  <PackageURL />" + System.Environment.NewLine +
				"</VersionInfo>";

			AssertEquals("Attachment XML", expectedValue, xml);

			var systemXml = DebugOnlyOutgoingSystemMessage.XmlMessageBody;
			var downloadInfo = new PackageDownloadInfo(systemXml);
			AssertEquals("The package has been rolled out to CargoWise Cloud", downloadInfo.Comment);
			AssertEquals(string.Empty, downloadInfo.PackageURL);
		}

		public void TestRun_ShouldSendRealUrlWhenDownloadSkipOptimizationDisabled()
		{
			// Arrange
			var systemMessageCreator = new DebugOnlyOutgoingSystemMessage();
			ObjectFactory.Substitute<IOutgoingSystemMessage>(systemMessageCreator);

			EDIDataRegistry.Instance.AllSystemMessagesViaEhubReleaseBuilds.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "16.10.20.0");
			UpgradesToClient upgradeViaEmail = GetTestUpgradesToClient(UpgradeMethods.Codes.Http);
			upgradeViaEmail.LicDatabase.LD_PublicEmailAddressForUpdate = "testemail1@edi.com.au";
			upgradeViaEmail.LicDatabase.LD_EnablePackageDownloadOptimization = false;
			upgradeViaEmail.Build.HL_IsRolledOut = true;

			UpgradesToClient upgradeViaEHub = GetTestUpgradesToClient(UpgradeMethods.Codes.Http);
			upgradeViaEHub.LicDatabase.LD_PublicEmailAddressForUpdate = "testemail2@edi.com.au";
			upgradeViaEHub.LicDatabase.CurrentVersion.VersionNumber = new VersionNumber(16, 10, 20, 0);
			upgradeViaEHub.LicDatabase.LD_EnablePackageDownloadOptimization = false;
			upgradeViaEHub.Build.HL_IsRolledOut = true;

			Factory.Save();

			var targetDir = PackagePathGenerator.GenerateWebServerRootPath("ENT");
			var mockPublisher = new Mock<IPackagePublishService>();
			mockPublisher.Setup(m => m.PublishPackage(TestFilePath, targetDir)).Returns(true);
			mockPublisher.Setup(m => m.IsPackageAlreadyPublished(Path.GetFileName(TestFilePath), "ENT")).Returns(TriState.False);

			var sender = new HttpPackageSender("ENT", TestFilePath, upgradeViaEmail, upgradeViaEHub);
			sender.SetPackagePublishForTest(mockPublisher.Object);

			// Act
			sender.Run();

			// Assert
			MailItemCollection mailItems = new StandardMailItemCollection(Factory);
			mailItems.Load();

			string expectedUrl = PackagePathGenerator.GenerateHttpPath("ENT", "Package20041230_135600_1_2_3_4.edp");
			string xml = new UTF8Encoding().GetString(mailItems[0].MailAttachments[0].MA_Data);

			string expectedValue =
				"<?xml version=\"1.0\" encoding=\"utf-8\"?>" + System.Environment.NewLine +
				"<VersionInfo>" + System.Environment.NewLine +
				"  <ReleaseBuildPK>00000000-0000-0000-0000-000000000000</ReleaseBuildPK>" + System.Environment.NewLine +
				"  <ExeVersionDate>30-DEC-04 13:56</ExeVersionDate>" + System.Environment.NewLine +
				"  <MajorVersion>1</MajorVersion>" + System.Environment.NewLine +
				"  <MinorVersion>2</MinorVersion>" + System.Environment.NewLine +
				"  <Release>3</Release>" + System.Environment.NewLine +
				"  <Patch>4</Patch>" + System.Environment.NewLine +
				"  <Comment>Deployed via Web</Comment>" + System.Environment.NewLine +
				"  <ForceDownload>Y</ForceDownload>" + System.Environment.NewLine +
				"  <PackageURL>" + expectedUrl + "</PackageURL>" + System.Environment.NewLine +
				"</VersionInfo>";

			AssertEquals("Attachment XML", expectedValue, xml);

			var systemXml = DebugOnlyOutgoingSystemMessage.XmlMessageBody;
			var downloadInfo = new PackageDownloadInfo(systemXml);

			AssertEquals(expectedUrl, downloadInfo.PackageURL);
			AssertEquals("Deployed via Web", downloadInfo.Comment);
		}

		public void TestRun_ShouldSendRealUrlWhenReleaseBuildNotRolledOut()
		{
			// Arrange
			var systemMessageCreator = new DebugOnlyOutgoingSystemMessage();
			ObjectFactory.Substitute<IOutgoingSystemMessage>(systemMessageCreator);

			EDIDataRegistry.Instance.AllSystemMessagesViaEhubReleaseBuilds.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "16.10.20.0");
			UpgradesToClient upgradeViaEmail = GetTestUpgradesToClient(UpgradeMethods.Codes.Http);
			upgradeViaEmail.LicDatabase.LD_PublicEmailAddressForUpdate = "testemail1@edi.com.au";
			upgradeViaEmail.LicDatabase.LD_EnablePackageDownloadOptimization = true;
			upgradeViaEmail.Build.HL_IsRolledOut = false;

			UpgradesToClient upgradeViaEHub = GetTestUpgradesToClient(UpgradeMethods.Codes.Http);
			upgradeViaEHub.LicDatabase.LD_PublicEmailAddressForUpdate = "testemail2@edi.com.au";
			upgradeViaEHub.LicDatabase.CurrentVersion.VersionNumber = new VersionNumber(16, 10, 20, 0);
			upgradeViaEHub.LicDatabase.LD_EnablePackageDownloadOptimization = true;
			upgradeViaEHub.Build.HL_IsRolledOut = false;

			Factory.Save();

			var targetDir = PackagePathGenerator.GenerateWebServerRootPath("ENT");
			var mockPublisher = new Mock<IPackagePublishService>();
			mockPublisher.Setup(m => m.PublishPackage(TestFilePath, targetDir)).Returns(true);
			mockPublisher.Setup(m => m.IsPackageAlreadyPublished(Path.GetFileName(TestFilePath), "ENT")).Returns(TriState.False);

			var sender = new HttpPackageSender("ENT", TestFilePath, upgradeViaEmail, upgradeViaEHub);
			sender.SetPackagePublishForTest(mockPublisher.Object);

			// Act
			sender.Run();

			// Assert
			MailItemCollection mailItems = new StandardMailItemCollection(Factory);
			mailItems.Load();

			string expectedUrl = PackagePathGenerator.GenerateHttpPath("ENT", "Package20041230_135600_1_2_3_4.edp");

			string xml = new UTF8Encoding().GetString(mailItems[0].MailAttachments[0].MA_Data);

			string expectedValue =
				"<?xml version=\"1.0\" encoding=\"utf-8\"?>" + System.Environment.NewLine +
				"<VersionInfo>" + System.Environment.NewLine +
				"  <ReleaseBuildPK>00000000-0000-0000-0000-000000000000</ReleaseBuildPK>" + System.Environment.NewLine +
				"  <ExeVersionDate>30-DEC-04 13:56</ExeVersionDate>" + System.Environment.NewLine +
				"  <MajorVersion>1</MajorVersion>" + System.Environment.NewLine +
				"  <MinorVersion>2</MinorVersion>" + System.Environment.NewLine +
				"  <Release>3</Release>" + System.Environment.NewLine +
				"  <Patch>4</Patch>" + System.Environment.NewLine +
				"  <Comment>Deployed via Web</Comment>" + System.Environment.NewLine +
				"  <ForceDownload>Y</ForceDownload>" + System.Environment.NewLine +
				"  <PackageURL>" + expectedUrl + "</PackageURL>" + System.Environment.NewLine +
				"</VersionInfo>";

			AssertEquals("Attachment XML", expectedValue, xml);

			var systemXml = DebugOnlyOutgoingSystemMessage.XmlMessageBody;
			var downloadInfo = new PackageDownloadInfo(systemXml);

			AssertEquals(expectedUrl, downloadInfo.PackageURL);
			AssertEquals("Deployed via Web", downloadInfo.Comment);
		}

		public void TestGetTargetDirectory()
		{
			HttpPackageSender genericSender = GetNewPackageSender("", "", CreateUpgrade(ReleaseRings.Codes.GPR));
			HttpPackageSender clientSpecificSender = GetNewPackageSender(TestEnterpriseCode, "", CreateUpgrade(ReleaseRings.Codes.GPR));
			AssertEquals("genericSender.GetTargetDirectory()", PackagePathGenerator.GenerateWebServerRootPath(""), genericSender.GetTargetDirectory());
			AssertEquals("clientSpecificSender.GetTargetDirectory()", PackagePathGenerator.GenerateWebServerRootPath(TestEnterpriseCode), clientSpecificSender.GetTargetDirectory());
		}

		protected virtual HttpPackageSender GetNewPackageSenderWithOverriddenTargetDirectory(string targetDirectory, string licenceEnterpriseCode, string packagePath, params UpgradesToClient[] upgradeRequests)
		{
			var mockSender = new Mock<HttpPackageSender>(new object[] { licenceEnterpriseCode, packagePath, upgradeRequests });
			mockSender.CallBase = true;
			mockSender.Setup(m => m.GetTargetDirectory()).Returns(targetDirectory);
			return mockSender.Object;
		}

		public void TestRemoveEmailSent()
		{
			AssertNotEquals(Guid.Empty, Env.Registry.PostMasterGroup);

			GlbStaff staff1 = Factory.New<GlbStaff>();
			staff1.GS_LoginName = "testPostMaster";
			staff1.GS_Code = "tpm";
			staff1.GS_EmailAddress = "testPostMaster@hotmail.com";

			GlbGroup postmastersGroup = Factory.Load<GlbGroup>(Env.Registry.PostMasterGroup);
			postmastersGroup.Staff.Add(staff1);

			Factory.Save();

			UpgradesToClient upgrade1 = GetTestUpgradesToClient(UpgradeMethods.Codes.Http);
			AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);

			var sender = GetNewPackageSenderWithOverriddenTargetDirectory(TestDirectory, "!@#", TestFilePath, upgrade1);
			var accessHelper = new PackagePublishServiceForTest();
			accessHelper.LastErrorMessage = "some error";
			sender.SetPackagePublishForTest(accessHelper);
			sender.Run();

			AssertEquals("There Should don't have any email created", 0, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		#region Implementation

		void CreateFile(string fileName)
		{
			if (!File.Exists(fileName))
			{
				using (FileStream stream = File.Create(fileName))
				{
				}
			}
		}

		protected UpgradesToClient CreateUpgrade(string releaseRing)
		{
			ReleaseBuild build = Factory.New<ReleaseBuild>();
			build.HL_ReleaseStatus = releaseRing;

			UpgradesToClient result = Factory.New<UpgradesToClient>();
			result.L1_HL = build.PK;
			var database = TestClient.LicCompany.LicDatabases.AddNew();
			result.L1_LD = database.PK;

			return result;
		}

		protected void DeleteDirectory(DirectoryInfo directory)
		{
			try
			{
				directory.Delete(true);
			}
			catch
			{
				TempDirectory.DeleteDirectory(directory);
			}
		}

		protected virtual void TryToGetControl()
		{
		}

		protected virtual void TryToReleaseControl()
		{
		}

		string basePath;
		protected string BasePath
		{
			get { return basePath ?? (basePath = Path.Combine(BasePathCore, Guid.NewGuid().ToString())); }
		}

		protected virtual string BasePathCore
		{
			get { return Env.TempPath; }
		}

		byte[] TestData
		{
			get { return new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }; }
		}

		protected string TestDirectory
		{
			get { return Path.Combine(BasePath, "!@#"); }
		}

		protected string TestFilePath
		{
			get { return Path.Combine(BasePath, "Package20041230_135600_1_2_3_4.edp"); }
		}

		protected LicenceDatabase GetTestDatabase(string upgradeMethod)
		{
			LicenceDatabase database = TestClient.LicCompany.LicEnterprise.Databases.AddNew();
			TestClient.LicCompany.LicDatabases.Add(database);
			database.LD_PublicEmailAddressForUpdate = "testemail" + upgradeMethod + "@edi.com.au";
			FirstSupportingVersion version;

			switch (upgradeMethod)
			{
				case UpgradeMethods.Codes.Http:
					version = new HttpDownload();
					break;
				default:
					version = null;
					break;
			}

			if (version != null)
			{
				ReleaseBuild build = Factory.New<ReleaseBuild>();
				build.HL_MajorVersion = version.VersionMajorNumber;
				build.HL_MinorVersion = version.VersionMinorNumber;
				build.HL_Release = version.VersionReleaseNumber;
				database.LD_HL_CurrentRunningVersion = build.PK;
			}

			database.LD_ServerCode = TestClient.LicCompany.LicEnterprise.Databases.Count.ToString().PadLeft(3);
			return database;
		}

		protected UpgradesToClient GetTestUpgradesToClient(string upgradeMethod)
		{
			UpgradesToClient upgrade = Factory.NewWithValidTestData<UpgradesToClient>();
			LicenceDatabase database = GetTestDatabase(upgradeMethod);
			upgrade.L1_LD = database.PK;
			upgrade.L1_RequestedUpgradeMethod = upgradeMethod;
			upgrade.L1_ActualUpgradeMethod = upgradeMethod;
			upgrade.L1_HL = Factory.New<ReleaseBuild>().PK;
			return upgrade;
		}

		protected HttpPackageSender GetNewPackageSender(string licenceEnterpriseCode, string packagePath, params UpgradesToClient[] upgradeRequests)
		{
			return new HttpPackageSender(licenceEnterpriseCode, packagePath, upgradeRequests);
		}

		protected EDIOrgHeader TestClient
		{
			get
			{
				if (testClient == null)
				{
					testClient = Factory.New<EDIOrgHeader>();
					testClient.OH_Code = "BOOGASYD";
					testClient.CreateAndLoadLicenceForOrg();
					testClient.LicenceEnterpriseCode = TestEnterpriseCode;
				}
				return testClient;
			}
		}

		EDIOrgHeader testClient;
		protected const string TestEnterpriseCode = "!@#";

		protected override void TearDown()
		{
			DeleteIfExists(TestLockedFilePath);
			base.TearDown();
		}

		string TestLockedFilePath
		{
			get { return Path.Combine(Env.TempPath, "SomeLockedFile"); }
		}

		public class PackagePublishServiceForTest : IPackagePublishService
		{
			public string TryToGetControlPath;

			public string TryToReleaseControlPath;

			public bool TryToGetControl(string path)
			{
				TryToGetControlPath = path;
				return false;
			}

			public bool TryToReleaseControl(string path)
			{
				TryToReleaseControlPath = path;
				return false;
			}

			public TriState IsPackageAlreadyPublished(string packageName, string clientSpecificCode)
			{
				return TriState.NotDetermined;
			}

			public bool PublishPackage(string packagePath, string targetDirectory)
			{
				return false;
			}

			public string LastErrorMessage { get; set; }
		}

		#endregion
	}
}
