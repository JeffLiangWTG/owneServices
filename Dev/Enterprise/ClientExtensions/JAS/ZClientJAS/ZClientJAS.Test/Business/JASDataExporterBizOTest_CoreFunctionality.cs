using System;
using System.IO;
using System.Text;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Moq;
using Moq.Protected;

namespace Enterprise.Client.JAS.Business.Testing
{
	sealed class JASDataExporterBizOTest_CoreFunctionality : TestCaseWithFactory
	{
		public void TestValidation()
		{
			AssertEquals("Default validation type", typeof(JASDataExporterBizOValidation), DataExporterBizO.Validation.GetType());
		}

		public void TestRunPreSaveValidation()
		{
			var dataExporterBizOMock = new Mock<JASDataExporterBizO>();
			dataExporterBizOMock.CallBase = true;
			var dataExporterBizOValidationMock = new Mock<JASDataExporterBizOValidation>(new object[] { dataExporterBizOMock.Object });
			dataExporterBizOValidationMock.CallBase = true;
			dataExporterBizOMock.Protected()
				.Setup<JASDataExporterBizOValidation>("GetNewJASDataExporterBizOValidation")
				.Returns(dataExporterBizOValidationMock.Object);
			dataExporterBizOValidationMock.Setup(m => m.ValidateAll());
			AssertNoExceptionThrown(() => dataExporterBizOMock.Object.RunPreSaveValidation());
			dataExporterBizOValidationMock.VerifyAll();
		}

		public void TestDefaultValues()
		{
			AssertEquals("Default should be email", JASDataExporterBizO.EmailDeliveryMethodCode, DataExporterBizO.DeliveryMethod);
			Assert("Default should be individual email recipient type", DataExporterBizO.IsIndividualEmailRecipient);
		}

		public void TestExportDirectoryInfo()
		{
			AssertEquals(300, DataExporterBizO.ExportDirectoryInfo.MaxLength);
			AssertEquals("ExportDirectory", DataExporterBizO.ExportDirectoryInfo.Name);
		}

		public void TestExportDirectory()
		{
			DataExporterBizO.DeliveryMethod = JASDataExporterBizO.DirectoryDeliveryMethodCode;
			var tempPath = Env.TempPath;
			AssertEquals(true, Directory.Exists(tempPath));
			DataExporterBizO.ExportDirectory = tempPath;
			AssertEquals(false, DataExporterBizO.ExportDirectoryInfo.HasErrors());
			var dir = @"\ThisIsA\Clients\JAS\TestDirectoryThatDoesNotExist_";
			DataExporterBizO.ExportDirectory = dir;
			Assert(DataExporterBizO.ExportDirectoryInfo.HasErrors());
			AssertEquals(1, DataExporterBizO.ExportDirectoryInfo.GetErrors().Count());
			var expectedText = string.Format("Directory \"{0}\" does not exist. Please select a different directory.", dir);
			AssertEquals(expectedText, DataExporterBizO.ExportDirectoryInfo.GetErrors().GetFirstMessage());
		}

		public void TestDeliveryMethod()
		{
			DataExporterBizO.DeliveryMethod = "ASD";
			AssertEquals("ASD", DataExporterBizO.DeliveryMethod);
			BusinessObjectValidationTestCase.AssertListValidationInvalidCodeError(DataExporterBizO.DeliveryMethodInfo, true);
			DataExporterBizO.DeliveryMethod = JASDataExporterBizO.EmailDeliveryMethodCode;
			AssertNoErrors("Valid delivery method", DataExporterBizO.DeliveryMethodInfo);
			AssertEquals(3, DataExporterBizO.DeliveryMethodInfo.MaxLength);
		}

		public void TestIsEmailDeliveryMethod()
		{
			DataExporterBizO.DeliveryMethod = JASDataExporterBizO.DirectoryDeliveryMethodCode;
			Assert("Not email delivery method", !DataExporterBizO.IsEmailDeliveryMethod);
			DataExporterBizO.DeliveryMethod = JASDataExporterBizO.EmailDeliveryMethodCode;
			Assert("Should be true", DataExporterBizO.IsEmailDeliveryMethod);
		}

		public void TestIsDirectoryDeliveryMethod()
		{
			DataExporterBizO.DeliveryMethod = JASDataExporterBizO.EmailDeliveryMethodCode;
			Assert("Not Directory delivery method", !DataExporterBizO.IsDirectoryDeliveryMethod);
			DataExporterBizO.DeliveryMethod = JASDataExporterBizO.DirectoryDeliveryMethodCode;
			Assert("Should be true", DataExporterBizO.IsDirectoryDeliveryMethod);
		}

		public void TestEmailAddress()
		{
			DataExporterBizO.DeliveryMethod = JASDataExporterBizO.EmailDeliveryMethodCode;
			DataExporterBizO.IsIndividualEmailRecipient = true;
			DataExporterBizO.EmailAddress = "asdfsdaf";
			AssertHasErrorContaining(DataExporterBizO.EmailAddressInfo, "Email Address is not valid");
			DataExporterBizO.EmailAddress = "test@edi.com.au";
			AssertNoErrors("Valid email address", DataExporterBizO.EmailAddressInfo);
		}

		public void TestEmailGroupPK()
		{
			DataExporterBizO.DeliveryMethod = JASDataExporterBizO.EmailDeliveryMethodCode;
			DataExporterBizO.IsGroupEmailRecipient = true;
			GlbGroup group = DataExporterBizO.EmailGroups.AddNew();
			DataExporterBizO.EmailGroupPK = ZGuid.NewZGuid();
			BusinessObjectValidationTestCase.AssertListValidationInvalidCodeError(DataExporterBizO.EmailGroupPKInfo, true);
			DataExporterBizO.EmailGroupPK = group.PK;
			AssertNoErrors("Valid email group", DataExporterBizO.EmailGroupPKInfo);
		}

		public void TestDeliverFiles_ToEmail_IndividualRecipient()
		{
			DataExporterBizO.EmailAddress = "mehmeh@laughingisgood.com";
			DataExporterBizO.DeliveryMethod = JASDataExporterBizO.EmailDeliveryMethodCode;
			DataExporterBizO.IsIndividualEmailRecipient = true;
			DataExporterBizO.ExportDirectory = TargetDir;
			string file1 = CreateEmptyTestFile("Attachment1");
			string file2 = CreateEmptyTestFile("Attachment2");
			string fileName1 = Path.GetFileName(file1);
			string fileName2 = Path.GetFileName(file2);
			DataExporterBizO.DeliverFiles(file1, file2);
			Assert("Source file should be removed once delivered", !File.Exists(file1));
			Assert("Source file should be removed once delivered", !File.Exists(file2));
			Assert("Should not be delivered to the export directory", !File.Exists(Path.Combine(TargetDir, fileName1)));
			Assert("Should not be delivered to the export directory", !File.Exists(Path.Combine(TargetDir, fileName2)));
			AssertEquals("There should be an email sent", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			EmailDef emailSent = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("mehmeh@laughingisgood.com", emailSent.Recipients[0]);
			AssertEquals("EmailSubject", emailSent.Subject);
			AssertEquals("export file should be sent as an attachment", fileName1, emailSent.Attachments[0].DisplayName);
			AssertEquals("export file should be sent as an attachment", fileName2, emailSent.Attachments[1].DisplayName);
			AssertEquals("Attachment1", GetStringFromByte(emailSent.Attachments[0].Data));
			AssertEquals("Attachment2", GetStringFromByte(emailSent.Attachments[1].Data));
		}

		public void TestDeliverFiles_ToEmail_GroupRecipient()
		{
			GlbGroup group = Factory.New<GlbGroup>();
			group.GG_Code = "ADM";
			GlbStaff newStaff = Factory.New<GlbStaff>();
			newStaff.GS_EmailAddress = "mehmeh@laughingisgood.com";
			newStaff.Groups.Add(group);
			newStaff.GS_Code = "ZAC";
			Factory.Save();
			DataExporterBizO.EmailGroups.Add(group);
			DataExporterBizO.DeliveryMethod = JASDataExporterBizO.EmailDeliveryMethodCode;
			DataExporterBizO.IsGroupEmailRecipient = true;
			DataExporterBizO.EmailGroupPK = group.PK;
			DataExporterBizO.ExportDirectory = TargetDir;
			string file1 = CreateEmptyTestFile("Attachment1");
			string file2 = CreateEmptyTestFile("Attachment2");
			string fileName1 = Path.GetFileName(file1);
			string fileName2 = Path.GetFileName(file2);
			DataExporterBizO.DeliverFiles(file1, file2);
			Assert("Source file should be removed once delivered", !File.Exists(file1));
			Assert("Source file should be removed once delivered", !File.Exists(file2));
			Assert("Should not be delivered to the export directory", !File.Exists(Path.Combine(TargetDir, fileName1)));
			Assert("Should not be delivered to the export directory", !File.Exists(Path.Combine(TargetDir, fileName2)));
			AssertEquals("There should be an email sent", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			EmailDef emailSent = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("mehmeh@laughingisgood.com", emailSent.Recipients[0]);
			AssertEquals("EmailSubject", emailSent.Subject);
			AssertEquals("export file should be sent as an attachment", fileName1, emailSent.Attachments[0].DisplayName);
			AssertEquals("export file should be sent as an attachment", fileName2, emailSent.Attachments[1].DisplayName);
			AssertEquals("Attachment1", GetStringFromByte(emailSent.Attachments[0].Data));
			AssertEquals("Attachment2", GetStringFromByte(emailSent.Attachments[1].Data));
		}

		public void TestDeliverFiles_ToExportDirectory()
		{
			DataExporterBizO.DeliveryMethod = JASDataExporterBizO.DirectoryDeliveryMethodCode;
			DataExporterBizO.ExportDirectory = TargetDir;
			string file1 = CreateEmptyTestFile("Attachment1");
			string file2 = CreateEmptyTestFile("Attachment2");
			string fileName1 = Path.GetFileName(file1);
			string fileName2 = Path.GetFileName(file2);
			DataExporterBizO.DeliverFiles(file1, file2);
			Assert("Source file should be removed once delivered", !File.Exists(file1));
			Assert("Source file should be removed once delivered", !File.Exists(file2));
			Assert("Should be delivered to the export directory", File.Exists(Path.Combine(TargetDir, fileName1)));
			Assert("Should be delivered to the export directory", File.Exists(Path.Combine(TargetDir, fileName2)));
		}

		#region Implementation
		JASDataExporterBizOForTest DataExporterBizO
		{
			get
			{
				if (fDataExporterBizO == null)
				{
					fDataExporterBizO = new JASDataExporterBizOForTest();
				}

				return fDataExporterBizO;
			}
		}

		string GetStringFromByte(byte[] data)
		{
			string dataAsString = Encoding.ASCII.GetString(data);
			string[] lines = dataAsString.Split('\n');
			Array.Sort(lines);
			return string.Join("\n", lines).Trim();
		}

		string CreateEmptyTestFile(string content)
		{
			string fileName = ZGuid.NewZGuid().ToString();
			string fullPath = Path.Combine(TestDir, fileName);
			File.WriteAllText(fullPath, content);
			return fullPath;
		}

		protected override void SetUp()
		{
			base.SetUp();
			Directory.CreateDirectory(TestDir);
			Directory.CreateDirectory(TargetDir);
		}

		protected override void TearDown()
		{
			base.TearDown();
			TempDirectory.DeleteDirectory(TestDir);
			TempDirectory.DeleteDirectory(TargetDir);
		}

		readonly string TestDir = Path.Combine(Env.TempPath, "_JASDataExporterBizOTest_");
		readonly string TargetDir = Path.Combine(Env.TempPath, "_JASDataExporterBizOTarget_");
		JASDataExporterBizOForTest fDataExporterBizO;
		#endregion
	}
}
