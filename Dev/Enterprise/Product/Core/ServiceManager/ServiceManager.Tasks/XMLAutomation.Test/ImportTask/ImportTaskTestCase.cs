using System;
using System.IO;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation
{
	sealed class ImportTaskTestCase : TestCaseWithFactory
	{
		[ExpectException(typeof(ArgumentNullException))]
		public void TestConstructorWithNullArgument()
		{
			ImportTaskForTestWithNullArgument importer = new ImportTaskForTestWithNullArgument(SystemDataRegistry.Instance.LocalCartageDataImportDirectory, new NotificationBuffer(), null);
		}

		public void TestProcessWithException()
		{
			NotificationDataRegistry.Instance.LocalCartageNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, PmgGroup.PK.ToGuid());

			var notify = new NotificationBuffer(new NotificationBuffer());
			var mockTask = new Mock<ImportTask>(new object[] { SystemDataRegistry.Instance.LocalCartageDataImportDirectory, notify, NotificationDataRegistry.Instance.LocalCartageNotificationGroup });
			mockTask.CallBase = true;
			mockTask.Protected().Setup<ZString>("FileExtension")
						 .Returns(new ZString("*.xml"));
			mockTask.Protected().Setup("ProcessFile", ItExpr.IsAny<FileInfo>()).Throws(new Exception("Testing Exception"));
			var task = mockTask.Object;
			AssertNotNull("Should not be null", task);
			task.Run();
			mockTask.VerifyAll();
			AssertEquals("Testing Exception", ErrorReporter.LastExceptionReported.Message);
			ErrorReporter.Clear();
		}

		public void TestExceptionMessage()
		{
			NotificationDataRegistry.Instance.LocalCartageNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, PmgGroup.PK.ToGuid());
			Env.OutgoingMailManager.EmailsCreated.Clear();

			AssertEquals("Precondition", true, File.Exists(FileName));
			ImportTaskThatThrowsException task = new ImportTaskThatThrowsException(SystemDataRegistry.Instance.LocalCartageDataImportDirectory);
			task.Run();
			string expectedMessage = String.Format("Error: The file {0} could not be imported as an error has occurred: Exception thrown\r\n\r\nException details follow:", FileName);
			string errorMessage = task.NotificationBuffer.Events[task.NotificationBuffer.Events.Length - 1].Message;
			Assert(errorMessage.StartsWith(expectedMessage));
			ErrorReporter.Clear();
		}

		public void TestFileGetsDeletedAfterProcessing()
		{
			AssertEquals("Precondition", true, File.Exists(FileName));
			ImportTaskForTest task = new ImportTaskForTest(SystemDataRegistry.Instance.LocalCartageDataImportDirectory);

			GlbGroup postmasters = Factory.Load<GlbGroup>(task.NotificationGroup.Value);
			GlbStaff postmaster = postmasters.Staff.AddNew();
			postmaster.GS_EmailAddress = "aa@bb.cc";
			postmaster.GS_Code = "ZAC";
			Factory.Save();

			using (FileStream fs = File.Open(FileName, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
			{
				task.Run();
				AssertEquals("File shouldn't be still processed as its locked and no ", 0, Env.OutgoingMailManager.EmailsCreated.Count);
				fs.Close();
				task.Run();
				AssertEquals("File is processed and deleted", 0, Env.OutgoingMailManager.EmailsCreated.Count);
				Assert("File should have been deleted", !File.Exists(FileName));
			}
			ErrorReporter.Clear();
		}

		public void TestReadonlyFileIsDeleted()
		{
			Assert("Precondition: File exists", File.Exists(FileName));
			ImportTaskForTest task = new ImportTaskForTest(SystemDataRegistry.Instance.LocalCartageDataImportDirectory);

			File.SetAttributes(FileName, FileAttributes.ReadOnly);
			task.Run();

			Assert("File should have been deleted", !File.Exists(FileName));
		}

		public void TestFileGetsDeletedIfException()
		{
			NotificationDataRegistry.Instance.LocalCartageNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, PmgGroup.PK.ToGuid());
			Env.OutgoingMailManager.EmailsCreated.Clear();

			AssertEquals("Precondition", true, File.Exists(FileName));
			ImportTaskThatThrowsException task = new ImportTaskThatThrowsException(SystemDataRegistry.Instance.LocalCartageDataImportDirectory);
			task.Run();
			AssertEquals("File should have been deleted", false, File.Exists(FileName));
			AssertEquals("One email should have been created", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			EmailDef email = Env.OutgoingMailManager.EmailsCreated[0];
			Assert("There are no attachments", email.Attachments.Count > 0);
			AttachmentDef attachment = email.Attachments[0];
			AssertEquals("Wrong attachment", Path.GetFileName(FileName), attachment.DisplayName);

			Assert("Email Body:", email.Body.Contains("The file " + FileName + " could not be imported as an error has occurred"));
			ErrorReporter.Clear();
		}

		public void TestProcess_ShouldNotReportXmlExceptions()
		{
			var exceptions = new Exception[] { new System.Xml.XmlException(), new InvalidOperationException { Source = "System.Xml" } };
			foreach (var ex in exceptions)
			{
				ProcessWithExpectedException_AssertDoesNotReport(ex);
			}
		}

		void ProcessWithExpectedException_AssertDoesNotReport(Exception ex)
		{
			ErrorReporter.Clear();

			NotificationDataRegistry.Instance.LocalCartageNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, PmgGroup.PK.ToGuid());
			Env.OutgoingMailManager.EmailsCreated.Clear();

			CreateTestFile();
			AssertEquals("Precondition: file should exist", true, File.Exists(FileName));
			var task = new ImportTaskThatThrowsException(SystemDataRegistry.Instance.LocalCartageDataImportDirectory)
			{
				ExceptionToThrow = ex
			};
			task.Run();

			AssertEquals("File should have been deleted", false, File.Exists(FileName));
			AssertEquals("One email should have been created", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			var email = Env.OutgoingMailManager.EmailsCreated[0];
			Assert("There should have been one attachment", email.Attachments.Count > 0);
			var attachment = email.Attachments[0];
			AssertEquals("Wrong attachment", Path.GetFileName(FileName), attachment.DisplayName);

			Assert("Email Body:", email.Body.Contains("The file " + FileName + " could not be imported as an error has occurred"));
			Assert(email.Body.Contains(ex.GetType().Name));

			AssertNull(ErrorReporter.LastExceptionReported);
		}

		public void TestWithMultipleCompanies()
		{
			GlbCompany newCompany1 = GlbCompany.CurrentCompany;
			GlbCompany newCompany2 = Factory.New<GlbCompany>();
			newCompany2.GC_Code = "~TT";
			newCompany2.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedKingdom;
			GlbBranch newBranch2a = newCompany2.Branches.AddNew();
			newBranch2a.GB_Code = "~TB";
			newBranch2a.GB_RL_NKHomePort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, newCompany2.GC_RN_NKCountryCode)).RL_Code;
			Factory.Save();

			NotificationDataRegistry.Instance.LocalCartageNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, PmgGroup.PK.ToGuid());
			string tempTaskDirectory1 = Temp.GetNewTempSubdirectory();
			string tempTaskDirectory2 = Temp.GetNewTempSubdirectory();
			string tempTaskDirectory3 = Temp.GetNewTempSubdirectory();
			SystemDataRegistry.Instance.LocalCartageDataImportDirectory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, tempTaskDirectory1);
			SystemDataRegistry.Instance.LocalCartageDataImportDirectory.SetValue(newCompany1.PK.ToGuid(), Guid.Empty, Guid.Empty, tempTaskDirectory2);
			SystemDataRegistry.Instance.LocalCartageDataImportDirectory.SetValue(newCompany2.PK.ToGuid(), Guid.Empty, Guid.Empty, tempTaskDirectory3);

			try
			{
				CreateTestFile(tempTaskDirectory1);
				CreateTestFile(tempTaskDirectory2);
				CreateTestFile(tempTaskDirectory3);
				int numberOfProcessedFiles = 0;
				ImportTaskWithCallback task = new ImportTaskWithCallback(SystemDataRegistry.Instance.LocalCartageDataImportDirectory, fileInfo =>
				{
					numberOfProcessedFiles++;
					if (fileInfo.DirectoryName.Equals(tempTaskDirectory3, StringComparison.OrdinalIgnoreCase))
					{
						AssertEquals(newCompany2.PK, GlbCompany.CurrentCompany.PK);
					}
					else
					{
						AssertEquals(newCompany1.PK, GlbCompany.CurrentCompany.PK);
					}
				});
				task.Run();
				AssertEquals(3, numberOfProcessedFiles);
			}
			finally
			{
				Directory.Delete(tempTaskDirectory1);
				Directory.Delete(tempTaskDirectory2);
				Directory.Delete(tempTaskDirectory3);
			}
		}

		public void TestWithMultipleBranchesOfOneCompany()
		{
			GlbCompany newCompany1 = GlbCompany.CurrentCompany;
			Factory.Save();

			NotificationDataRegistry.Instance.LocalCartageNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, PmgGroup.PK.ToGuid());
			string tempTaskDirectory1 = Temp.GetNewTempSubdirectory();
			string tempTaskDirectory2 = Temp.GetNewTempSubdirectory();
			SystemDataRegistry.Instance.LocalCartageDataImportDirectory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, tempTaskDirectory1);
			SystemDataRegistry.Instance.LocalCartageDataImportDirectory.SetValue(newCompany1.PK.ToGuid(), Guid.Empty, Guid.Empty, tempTaskDirectory2);

			var context = new TemporaryUserContext();
			context.BranchPK = Factory.Load<GlbBranch>(new ZQuery(GlbBranchSchema.GB_Code, "TES"))[0].PK.ToGuid(); //BNE is the default branch for this company, need any other branch
			context.DepartmentPK = Factory.Load<GlbDepartment>(new ZQuery())[0].PK.ToGuid();

			using (context.Set())
			{
				try
				{
					CreateTestFile(tempTaskDirectory1);
					CreateTestFile(tempTaskDirectory2);
					int numberOfProcessedFiles = 0;
					ImportTaskWithCallback task = new ImportTaskWithCallback(SystemDataRegistry.Instance.LocalCartageDataImportDirectory, fileInfo =>
					{
						numberOfProcessedFiles++;
						AssertEquals(newCompany1.PK, GlbCompany.CurrentCompany.PK);
						AssertEquals(context.BranchPK, GlbBranch.CurrentBranch.PK);
					});
					task.Run();
					AssertEquals(2, numberOfProcessedFiles);
				}
				finally
				{
					Directory.Delete(tempTaskDirectory1);
					Directory.Delete(tempTaskDirectory2);
				}
			}
		}

		GlbGroup PmgGroup
		{
			get
			{
				if (pmgGroup == null)
				{
					pmgGroup = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
					pmgGroup.Staff[0].GS_EmailAddress = "test@cargowise.com";
					Factory.Save();
				}
				return pmgGroup;
			}
		}
		GlbGroup pmgGroup;

		protected override void SetUp()
		{
			base.SetUp();
			CreateTestFile();
			SystemDataRegistry.Instance.LocalCartageDataImportDirectory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Env.TempPath);
		}

		void CreateTestFile()
		{
			using (StreamWriter writer = new StreamWriter(FileName))
			{
				writer.Write("Sample text");
			}
		}

		void CreateTestFile(string directory)
		{
			using (StreamWriter writer = new StreamWriter(Path.Combine(directory, "TempFile.xml")))
			{
				writer.Write("Sample text");
			}
		}

		protected override void TearDown()
		{
			base.TearDown();
			DeleteIfExists(FileName);
		}

		string FileName
		{
			get { return Path.Combine(Env.TempPath, "TempFile.xml"); }
		}

		#region Test classes

		class ImportTaskThatThrowsException : ImportTaskForTest
		{
			public ImportTaskThatThrowsException(StringRegistryItem registryPath)
				: base(registryPath)
			{
			}

			internal Exception ExceptionToThrow
			{
				get { return exceptionToThrow ?? new Exception("Exception thrown"); }
				set { exceptionToThrow = value; }
			}
			Exception exceptionToThrow;

			protected override void ProcessFile(FileInfo dataFile)
			{
				throw ExceptionToThrow;
			}
		}

		class ImportTaskForTestWithNullArgument : ImportTask
		{
			public ImportTaskForTestWithNullArgument(StringRegistryItem registryPath, INotifications notify, GuidRegistryItem notificationGroup)
				: base(registryPath, notify, notificationGroup)
			{
			}

			protected override void ProcessFile(FileInfo dataFile)
			{
			}

			protected override ZString FileExtension
			{
				get { return ""; }
			}
		}

		#endregion
	}
}
