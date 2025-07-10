using System;
using System.IO;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Client.TNT.AirCargo.Testing
{
	public class AirCargoFileImporterTest : AirCargoFileImporterTestCase
	{
		[GuiTest]
		public void TestImport_CheckEnvironmentValidReturnFalse()
		{
			using (ZForm form = new ZForm())
			{
				string expectedErrorMessage = "Error in Test Check Environment Valid";
				AirCargoFileImporterTestClass importer = new AirCargoFileImporterTestClass(form);
				importer.CheckEnvironmentValidWasCalled = false;
				importer.CheckEnvironmentValid_ErrorMessage = expectedErrorMessage;
				importer.CheckEnvironmentValid_Result = false;
				AssertEquals("PreCondition: CheckEnvironmentValidWasCalled", false, importer.CheckEnvironmentValidWasCalled);
				AssertImport(importer, expectedErrorMessage);
				AssertEquals("CheckEnvironmentValidWasCalled", true, importer.CheckEnvironmentValidWasCalled);
			}
		}

		[GuiTest]
		public void TestImport_FileNotExist()
		{
			using (ZForm form = new ZForm())
			{
				string expectedErrorMessage = @"File 'Blah \ blah \ blah'";
				AirCargoFileImporterTestClass importer = new AirCargoFileImporterTestClass(form);
				importer.TestFilePath = @"Blah \ blah \ blah";
				AssertImport(importer, expectedErrorMessage);
			}
		}

		[GuiTest]
		public void TestImport_InvalidFile()
		{
			using (ZForm form = new ZForm())
			{
				ImportManagerTest.ImportManagerTestClass manager = new ImportManagerTest.ImportManagerTestClass(Factory, NotValidFileTest);
				manager.ProcessedDirectoryTest = ProcessedDirectory;
				AirCargoFileImporterTestClass importer = new AirCargoFileImporterTestClass(form);
				importer.NewImportManagerTest = manager;
				importer.TestFilePath = NotValidFileTest;
				string expectedErrorMessage = string.Format("File '{0}' is not an Import Type file", NotValidFileTest);
				using (ZForm newForm = new ZForm())
				{
					importer.NewFormTest = newForm;
					AssertImport(importer, expectedErrorMessage);
				}
			}
		}

		[TestDate(2005, 11, 22, 20, 40, 35)]
		[GuiTest]
		public void TestImport_DataLoadWithError()
		{
			using (ZForm form = new ZForm())
			{
				ImportManagerTest.ImportManagerTestClass manager = new ImportManagerTest.ImportManagerTestClass(Factory, TestFile);
				manager.ProcessedDirectoryTest = ProcessedDirectory;
				AirCargoFileImporterTestClass importer = new AirCargoFileImporterTestClass(form);
				importer.NewImportManagerTest = manager;
				importer.TestFilePath = TestFile;
				importer.GetNewImportManagerWasCalled = false;
				manager.ProcessRecordCalled = false;
				manager.ProcessRecordNotifyEvent = new ErrorNotification(ErrorType.Error, "Test Error In Load");
				AssertEquals("PreCondition: File '" + importer.TestFilePath + "' should exist", true, File.Exists(importer.TestFilePath));
				NotificationBuffer buffer = new NotificationBuffer();
				buffer.Notify(manager.ProcessRecordNotifyEvent);
				string expectedErrorMessage = buffer.AsString;
				AssertEquals("PreCondition: Importer.GetNewImportManagerWasCalled", false, importer.GetNewImportManagerWasCalled);
				AssertEquals("PreCondition: Manager.ProcessRecordCalled", false, manager.ProcessRecordCalled);
				AssertImport(importer, expectedErrorMessage);
				AssertEquals("File '" + importer.TestFilePath + "' should not exist", false, File.Exists(importer.TestFilePath));
				AssertEquals("Importer.GetNewImportManagerWasCalled should have been called", true, importer.GetNewImportManagerWasCalled);
				AssertEquals("Manager.ProcessRecordCalled", true, manager.ProcessRecordCalled);
			}
		}

		[TestDate(2005, 11, 22, 20, 40, 35)]
		[GuiTest]
		public override void TestImport()
		{
			using (ZForm form = new ZForm())
			{
				ImportManagerTest.ImportManagerTestClass manager = new ImportManagerTest.ImportManagerTestClass(Factory, TestFile);
				manager.ProcessedDirectoryTest = ProcessedDirectory;
				AirCargoFileImporterTestClass importer = new AirCargoFileImporterTestClass(form);
				importer.NewImportManagerTest = manager;
				importer.TestFilePath = TestFile;
				importer.GetNewFormWasCalled = false;
				AssertEquals("PreCondition: File '" + importer.TestFilePath + "' should exist", true, File.Exists(importer.TestFilePath));
				AssertEquals("PreCondition: Importer.GetNewFormWasCalled", false, importer.GetNewFormWasCalled);
				using (ZForm newForm = new ZForm())
				{
					importer.NewFormTest = newForm;
					importer.Import();
				}

				AssertEquals("File '" + importer.TestFilePath + "' should not exist", true, File.Exists(importer.TestFilePath));
				AssertEquals("Importer.GetNewFormWasCalled should have been called", true, importer.GetNewFormWasCalled);
			}
		}

		[GuiTest]
		public void TestFileExtension()
		{
			using (ZForm form = new ZForm())
			{
				AirCargoFileImporterTestClass importer = new AirCargoFileImporterTestClass(form);
				importer.FileExtensionCoreTest = "abc,def";
				AssertEquals("FileExtension", "abc,def", importer.FileExtensionTest);
				importer.FileExtensionCoreTest = "abc.def";
				AssertEquals("'.' should be remove from the FileExtension", "abcdef", importer.FileExtensionTest);
			}
		}

#region Implementation
		void AssertImport(AirCargoFileImporterTestClass importer, ZString expectedErrorMessage)
		{
			AssertNotNull("Importer should exist", importer);
			UnitTestUserNotification userNotification = Globals.Message as UnitTestUserNotification;
			AssertNotNull("UserNotification should not be null", userNotification);
			userNotification.ClearMessagesAndAnswers();
			AssertEquals("Initial Length (UserNotification.None)", 1, userNotification.PreviousMessages.Length);
			importer.Import();
			var lastMessage = userNotification.LastMessage.Text ?? string.Empty;
			AssertEquals("Expected 1 new error message", 2, userNotification.PreviousMessages.Length);
			AssertEquals("Last Error Message:" + System.Environment.NewLine + lastMessage, expectedErrorMessage, lastMessage);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var filePath = resourceRetriever.SaveResourceToFile("Enterprise.Client.TNT.Testing.DataManipulation.AirCargo.INDInterface.Testing.SYD.IND.20050809.141533.ok");
			SoureFile = new FileInfo(filePath);
			notValidFilePath = resourceRetriever.SaveResourceToFile("Enterprise.Client.TNT.Testing.DataManipulation.AirCargo.INDInterface.Testing.SYD.EX1.NotIQDownFile.TXT");
		}

		string notValidFilePath;
		FileInfo SoureFile;
#region TestFile
		ZString TestFile
		{
			get
			{
				if (fTestFile.IsEmpty)
				{
					fTestFile = Path.Combine(SourceDirectory, "SYD.IND.20050811.091011" + QuantumFile.Extension);
				}

				if (!File.Exists(fTestFile))
				{
					File.Copy(SoureFile.FullName, fTestFile);
				}

				return fTestFile;
			}
		}

		ZString fTestFile;
#endregion
#region NotValidFile
		protected override FileInfo NotValidFile
		{
			get
			{
				if (fNotValidFile == null)
				{
					fNotValidFile = new FileInfo(notValidFilePath);
				}

				return fNotValidFile;
			}
		}

		FileInfo fNotValidFile;
#endregion
		protected override FileInfo InvalidFileFormat
		{
			get
			{
				throw new NotSupportedException("InvalidFileFormat");
			}
		}

#region AirCargoFileImporterTestClass
		class AirCargoFileImporterTestClass : AirCargoFileImporter
		{
			public AirCargoFileImporterTestClass(Form modalForm) : base(modalForm)
			{
				CheckEnvironmentValid_Result = true;
				CheckEnvironmentValid_ErrorMessage = "";
				GetNewFormWasCalled = false;
				CheckEnvironmentValidWasCalled = false;
				GetNewImportManagerWasCalled = false;
				FileExtensionCoreTest = "OK";
			}

			public string FileExtensionTest
			{
				get
				{
					return base.FileExtension;
				}
			}

#region Implementation
#region CheckEnvironmentValid
			public bool CheckEnvironmentValidWasCalled;
			public bool CheckEnvironmentValid_Result;
			public string CheckEnvironmentValid_ErrorMessage;
			protected override bool CheckEnvironmentValid(out string errorMessage)
			{
				errorMessage = CheckEnvironmentValid_ErrorMessage;
				CheckEnvironmentValidWasCalled = true;
				return CheckEnvironmentValid_Result;
			}

#endregion
#region SourceDirectory
			protected override string SourceDirectory
			{
				get
				{
					return "";
				}
			}

#endregion
			protected override string ValidFilePattern
			{
				get
				{
					return @".OK$";
				}
			}

#region FileExtensionCore
			public string FileExtensionCoreTest;
			protected override string FileExtensionCore
			{
				get
				{
					return FileExtensionCoreTest;
				}
			}

#endregion
			protected override string FileType
			{
				get
				{
					return "Import Type";
				}
			}

#region GetNewForm
			public ZForm NewFormTest;
			public bool GetNewFormWasCalled;
			protected override ZForm GetNewForm(ImportManager manager)
			{
				GetNewFormWasCalled = true;
				return NewFormTest;
			}

#endregion
#region GetNewImportManager
			public ImportManager NewImportManagerTest;
			public bool GetNewImportManagerWasCalled;
			protected override ImportManager GetNewImportManager(string fileFullPath)
			{
				GetNewImportManagerWasCalled = true;
				return NewImportManagerTest;
			}
#endregion
#endregion
		}
#endregion
#endregion
	}
}
