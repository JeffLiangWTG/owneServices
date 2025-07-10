using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DataTransfer.Xml;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Client.WFN.ServiceTasks.Testing
{
	[TestedType(typeof(WFNShipmentAndConsolImportServiceTask))]
	public class WFNShipmentAndConsolImportServiceTaskTest : ServiceTaskTestCase<WFNShipmentAndConsolImportServiceTask>
	{
		[TestDate(2001, 1, 2)]
		public void TestExecuteBatch()
		{
			int bizObjCount = Factory.GetDatabaseCount(typeof(ForwardingConsol));
			Env.OutgoingMailManager.EmailsCreated.Clear();
			string pathToFile = Path.Combine(importDirectoryPath, "test.xml");
			string pathToBackupFile = Path.Combine(backupDirectoryPath, "02-Jan-01_1200test.xml");
			using (var resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly))
			{
				var srcTestFilePath = resourceRetriever.SaveResourceToFile("Import.TestFiles.Test.xml");
				File.Copy(srcTestFilePath, pathToFile, true);
				File.SetAttributes(pathToFile, FileAttributes.Normal);
				new FileInfo(pathToFile).CreationTimeUtc = ZDateTime.UtcNow.AddMinutes(-3).ToDateTime();
				RunTaskSchedule(serviceTask);
				AssertContains("Starting Dimerco XML Import...", serviceTask.Buffer.AsString);
				AssertContains("XML Import Completed...", serviceTask.Buffer.AsString);
				Assert(!File.Exists(pathToFile));
				Assert(File.Exists(pathToBackupFile));
				AssertEquals("One email should have been created", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				EmailDef email = Env.OutgoingMailManager.EmailsCreated[0];
				AssertEquals("Dimerco XML Import - Success", email.Subject);
				Assert("Email body:", email.Body.Contains("Processed file test.xml"));
				AssertEquals("New consol hould have been created", bizObjCount + 1, Factory.GetDatabaseCount(typeof(ForwardingConsol)));
				ForwardingConsol[] consols = Factory.Load<ForwardingConsol>(new ZQuery(JobConsolSchema.JK_MasterBillNum, "02307174974"));
				AssertEquals("One consol should be loaded", 1, consols.Length);
			}
		}

		[TestDate(2011, 1, 2)]
		public void TestProcessAnInaccessibleFile()
		{
			var importDirInfo = new DirectoryInfo(importDirectoryPath);
			var filesList = importDirInfo.GetFiles("*.xml");
			Env.OutgoingMailManager.EmailsCreated.Clear();
			AssertEquals("Precondition: there is no file in the import directory", filesList.Length, 0);
			string pathToFile = Path.Combine(importDirectoryPath, "test.xml");

			using (var resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly))
			{
				var srcTestFilePath = resourceRetriever.SaveResourceToFile("Import.TestFiles.Test.xml");
				File.Copy(srcTestFilePath, pathToFile, true);
				File.SetAttributes(pathToFile, FileAttributes.Normal);
				new FileInfo(pathToFile).CreationTimeUtc = ZDateTime.UtcNow.AddHours(-1).ToDateTime();
				using (var file = new FileStream(pathToFile, FileMode.Open, FileAccess.Read, FileShare.None))
				{
					RunTaskSchedule(serviceTask);
				}

				AssertContains("Cannot process file test.xml as it is locked by another process. The file will be processed in the next run", serviceTask.Buffer.AsString);
				AssertEquals("No email should be generated", Env.OutgoingMailManager.EmailsCreated.Count, 0);
			}
		}

		[TestDate(2001, 1, 2)]
		public void TestExecuteBatchWhenNoFilesToImport()
		{
			Assert("Precondition: no files to import", Directory.GetFiles(WFNDataRegistry.Instance.DimercoXMLImportDirectory, "*.xml").Length == 0);
			Env.OutgoingMailManager.EmailsCreated.Clear();
			RunTaskSchedule(serviceTask);
			AssertContains("Import should have been run", "XML Import Completed...", serviceTask.Buffer.AsString);
			AssertEquals("No emails should have been created", 0, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		[TestDate(2012, 11, 2)]
		public void TestCheckForNewData()
		{
			var serviceTaskForTest = new WFNShipmentAndConsolImportServiceTaskrForTestWithImportingError();
			InitialiseTaskSchedule(serviceTaskForTest);
			var importDirInfo = new DirectoryInfo(importDirectoryPath);
			var filesList = importDirInfo.GetFiles("*.xml");
			AssertEquals("Precondition: there is no file in the import directory", filesList.Length, 0);
			string pathToFile = Path.Combine(importDirectoryPath, "test.xml");

			using (var resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly))
			{
				var srcTestFilePath = resourceRetriever.SaveResourceToFile("Import.TestFiles.Test.xml");
				File.Copy(srcTestFilePath, pathToFile, true);
				File.SetAttributes(pathToFile, FileAttributes.Normal);
				var importfile = new FileInfo(pathToFile);
				importfile.CreationTimeUtc = ZDateTime.UtcNow.AddHours(-1).ToDateTime();
				Assert("Precondition: File is created awhile ago (more than 2 mins ago)", (ZDateTime.UtcNow - importfile.CreationTimeUtc) > new TimeSpan(0, 2, 0));
				var newlyCreatedFile = Env.GetTempFileName(importDirectoryPath, "xml");
				Assert("Precondition: File is just created (within 2 mins from now)", (ZDateTime.UtcNow - new FileInfo(newlyCreatedFile).CreationTimeUtc) < new TimeSpan(0, 2, 0));
				filesList = importDirInfo.GetFiles("*.xml");
				AssertEquals("Precondition: there are two files in the import directory", filesList.Length, 2);
				var fileToBeProcessed = serviceTaskForTest.GetFileListForProcess();
				AssertEquals("There is only one file to be processed", 1, fileToBeProcessed.Length);
				AssertEquals(ZString.Format("The file to be processed should be {0}", "test.xml"), "test.xml", fileToBeProcessed[0].Name);
			}
		}

		[TestDate(2001, 1, 2)]
		public void TestExecuteWithDataImportingError()
		{
			TestDateAttribute.Date = ZDateTime.Now.AddDays(1).ToDateTime();
			string pathToFile = Path.Combine(importDirectoryPath, "test.xml");

			using (var resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly))
			{
				var srcTestFilePath = resourceRetriever.SaveResourceToFile("Import.TestFiles.Test.xml");
				File.Copy(srcTestFilePath, pathToFile, true);
				Env.OutgoingMailManager.EmailsCreated.Clear();
				File.SetAttributes(pathToFile, FileAttributes.Normal);
				new FileInfo(pathToFile).CreationTimeUtc = ZDateTime.UtcNow.AddHours(-1).ToDateTime();
				var serviceTaskForTest = new WFNShipmentAndConsolImportServiceTaskrForTestWithImportingError();
				InitialiseTaskSchedule(serviceTaskForTest);
				RunTaskSchedule(serviceTaskForTest);
				AssertEmailWithAttachmentCreated("test.xml");
			}
		}

		public void TestIsEnvironmentDataValid_Empty()
		{
			WFNDataRegistry.Instance.DimercoXMLImportDirectory = "";
			WFNDataRegistry.Instance.DimercoXMLImportNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty);
			RunTaskSchedule(serviceTask);
			AssertContains("The Import Directory is empty or invalid. Please check in System->Registry->WFN Client Extensions->Import->Dimerco", serviceTask.Buffer.AsString);
			AssertContains("The Notification Group is empty or invalid. Please check in System->Registry->WFN Client Extensions->Import->Dimerco", serviceTask.Buffer.AsString);
			serviceTask.Buffer.Clear();
			WFNDataRegistry.Instance.DimercoXMLImportDirectory = "DirectoryNotExists";
			WFNDataRegistry.Instance.DimercoXMLImportNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.NewGuid());
			RunTaskSchedule(serviceTask);
			AssertContains("The Import Directory is empty or invalid. Please check in System->Registry->WFN Client Extensions->Import->Dimerco", serviceTask.Buffer.AsString);
			AssertContains("The Notification Group is empty or invalid. Please check in System->Registry->WFN Client Extensions->Import->Dimerco", serviceTask.Buffer.AsString);
		}

		public void TestIsEnvironmentDataValid()
		{
			RunTaskSchedule(serviceTask);
			AssertNotContains("The Import Directory is empty or invalid. Please check in System->Registry->WFN Client Extensions->Import->Dimerco", serviceTask.Buffer.AsString);
			AssertNotContains("The Notification Group is empty or invalid. Please check in System->Registry->WFN Client Extensions->Import->Dimerco", serviceTask.Buffer.AsString);
		}

		[TestDate(2001, 1, 2)]
		public void TestImportFromXmlWithError()
		{
			TestDateAttribute.Date = ZDateTime.Now.AddDays(1).ToDateTime();
			string pathToFile = Path.Combine(importDirectoryPath, "WrongXmlFile.xml");

			using (var resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly))
			{
				var srcTestFilePath = resourceRetriever.SaveResourceToFile("Import.TestFiles.WrongXmlFile.xml");
				File.Copy(srcTestFilePath, pathToFile, true);
				File.SetAttributes(pathToFile, FileAttributes.Normal);
				new FileInfo(pathToFile).CreationTimeUtc = ZDateTime.UtcNow.AddHours(-1).ToDateTime();
				Env.OutgoingMailManager.EmailsCreated.Clear();
				RunTaskSchedule(serviceTask);
				AssertEmailWithAttachmentCreated("WrongXmlFile.xml");
			}
		}

		public void TestMinimumPeriod()
		{
			AssertEquals("1minute", GetHostedServiceAttributes().Single().MinimumPeriod);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();
		void AssertEmailWithAttachmentCreated(string displayName)
		{
			AssertEquals("One email should have been created", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			EmailDef email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("Emai subject", "Dimerco XML Import - Failure", email.Subject);
			AssertEquals("Should be an attachment", 1, email.Attachments.Count);
			AttachmentDef att = email.Attachments[0];
			AssertEquals("File attached", displayName, att.DisplayName);
		}

#region SetUp
		protected override void SetUpCore()
		{
			base.SetUpCore();
			serviceTask = new WFNShipmentAndConsolImportServiceTask();
			InitialiseTaskSchedule(serviceTask);
			SetUpRegistryItem();
		}

		protected override void TearDownCore()
		{
			base.TearDownCore();
			RemoveDirectories();
		}

		void SetUpRegistryItem()
		{
			importDirectoryPath = Path.Combine(Env.TempPath, "WFN");
			Directory.CreateDirectory(importDirectoryPath);
			WFNDataRegistry.Instance.DimercoXMLImportDirectory = importDirectoryPath;
			backupDirectoryPath = Path.Combine(importDirectoryPath, "Backup");
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "a@b.com";
			GlbGroup group = Factory.NewWithValidTestData<GlbGroup>();
			group.GG_Code = "NT";
			group.Staff.Add(staff);
			Factory.Save();
			WFNDataRegistry.Instance.DimercoXMLImportNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());
		}

		void RemoveDirectories()
		{
			if (!string.IsNullOrEmpty(importDirectoryPath))
			{
				TempDirectory.DeleteDirectory(importDirectoryPath);
			}

			if (!string.IsNullOrEmpty(backupDirectoryPath))
			{
				TempDirectory.DeleteDirectory(backupDirectoryPath);
			}
		}

		WFNShipmentAndConsolImportServiceTask serviceTask;
		string importDirectoryPath;
		string backupDirectoryPath;
		class WFNShipmentAndConsolImportServiceTaskrForTestWithImportingError : WFNShipmentAndConsolImportServiceTask
		{
			protected override void ConvertAndSaveToDatabase(IValueObject[] valueObjects)
			{
				Buffer.Notify(new ErrorNotification(ErrorType.ImportingDataError, "testError"));
				base.ConvertAndSaveToDatabase(valueObjects);
			}

			public FileInfo[] GetFileListForProcess()
			{
				return base.CheckForNewData();
			}
		}
#endregion
	}
}
