using System;
using System.IO;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture;
using Moq;
using NUnit.Framework;

namespace Enterprise.Client.TNT.AirCargo.Testing
{
	[TestedType(typeof(ImportManagerTestClass))]
	public class ImportManagerTest : NonPersistentBusinessObjectTestCase
	{
		public void TestConstructor()
		{
			ImportManagerTestClass manager = new ImportManagerTestClass(Factory, ValidFilename);
			AssertNotNull("Manager should not be null", manager);
			AssertNotNull("Manager.FileFullName should not be null", manager.FileFullName);
			AssertEquals("Manager.FileFullName", ValidFilename, manager.FileFullName);
			AssertNotNull("Manager.RefUNLOCO_List should not be null", manager.RefUNLOCO_List);
		}

		[ExpectException(typeof(ArgumentException))]
		public void TestConstructor_EmptyFilename()
		{
			ImportManagerTestClass manager = new ImportManagerTestClass(Factory, "");
		}

#region TestLoadFromFile
		public void TestLoadFromFile_ValidFile()
		{
			ImportManagerTestClass manager = new ImportManagerTestClass(Factory, ValidFilename);
			NotificationBuffer buffer = new NotificationBuffer();
			Manager_LoadProgressCalled = false;
			try
			{
				manager.LoadProgress += new TNTProgressEventHandler(Manager_LoadProgress);
				manager.LoadFromFile(buffer);
				AssertEquals("Bufer has no errors: Buffer contains:" + System.Environment.NewLine + buffer.AsString, false, buffer.HasErrors);
				AssertEquals("PreLoadSetup was called", true, manager.PreLoadSetupCalled);
				AssertEquals("ProcessRecord was called", true, manager.ProcessRecordCalled);
				AssertEquals("Manger's Load Process was called", true, Manager_LoadProgressCalled);
			}
			finally
			{
				manager.LoadProgress -= new TNTProgressEventHandler(Manager_LoadProgress);
			}
		}

		public void TestLoadFromFile_EmptyFile()
		{
			ImportManagerTestClass manager = new ImportManagerTestClass(Factory, EmptyFilename);
			NotificationBuffer buffer = new NotificationBuffer();
			Manager_LoadProgressCalled = false;
			manager.LoadFromFile(buffer);
			AssertEquals("Bufer has errors: Buffer contains:" + System.Environment.NewLine + buffer.AsString, true, buffer.HasErrors);
			AssertEquals("Bufer has errors: Buffer contains:" + System.Environment.NewLine + buffer.AsString, true, buffer.ContainsNotificationType(TNTErrorType.Error));
			string errorMessage = string.Format("File '{0}' is empty", EmptyFilename);
			AssertEquals("Bufer should contain error message '" + errorMessage + "': Buffer contains:" + System.Environment.NewLine + buffer.AsString, true, buffer.AsString.IndexOf(errorMessage) >= 0);
			AssertEquals("PreLoadSetup was not called", false, manager.PreLoadSetupCalled);
			AssertEquals("ProcessRecord was not called", false, manager.ProcessRecordCalled);
		}

		public void TestLoadFromFile_MissingFile()
		{
			ZString filename = Path.Combine(Env.TempPath, "blah blah lahs.txt");
			ImportManagerTestClass manager = new ImportManagerTestClass(Factory, filename);
			NotificationBuffer buffer = new NotificationBuffer();
			Manager_LoadProgressCalled = false;
			manager.LoadFromFile(buffer);
			AssertEquals("Bufer has errors: Buffer contains:" + System.Environment.NewLine + buffer.AsString, true, buffer.HasErrors);
			AssertEquals("Bufer has errors: Buffer contains:" + System.Environment.NewLine + buffer.AsString, true, buffer.ContainsNotificationType(TNTErrorType.IOError));
			string errorMessage = string.Format("File '{0}' doesn't exists", filename);
			AssertEquals("Bufer should contain error message '" + errorMessage + "': Buffer contains:" + System.Environment.NewLine + buffer.AsString, true, buffer.AsString.IndexOf(errorMessage) >= 0);
			AssertEquals("PreLoadSetup was not called", false, manager.PreLoadSetupCalled);
			AssertEquals("ProcessRecord was not called", false, manager.ProcessRecordCalled);
		}

		public void TestLoadFromFile_LastRecordAlsoLoaded()
		{
			ImportManagerTestClass manager = new ImportManagerTestClass(Factory, SmallFilename);
			NotificationBuffer buffer = new NotificationBuffer();
			Manager_LoadProgressCalled = false;
			try
			{
				manager.LoadProgress += new TNTProgressEventHandler(Manager_LoadProgress);
				manager.LoadFromFile(buffer);
				AssertEquals("Bufer has no errors: Buffer contains:" + System.Environment.NewLine + buffer.AsString, false, buffer.HasErrors);
				AssertEquals("PreLoadSetup was called", true, manager.PreLoadSetupCalled);
				AssertEquals("ProcessRecord was called", true, manager.ProcessRecordCalled);
				AssertEquals("Number of time ProcessRecord was called", 8, manager.NoOfTimeProcessRecordWasCalled);
				AssertEquals("Manger's Load Process was called", true, Manager_LoadProgressCalled);
			}
			finally
			{
				manager.LoadProgress -= new TNTProgressEventHandler(Manager_LoadProgress);
			}
		}

		public void TestLoadFromFile_IsExtraCheckOKReturnFalse()
		{
			Mock<ImportManagerTestClass> mock = new Mock<ImportManagerTestClass>(new object[] { Factory, new ZString(ValidFilename) });
			mock.CallBase = true;
			mock.Setup(m => m.IsExtraCheckOK(It.IsAny<string[]>(), It.IsAny<NotificationBuffer>())).Returns(false);
			ImportManagerTestClass manager = mock.Object;
			NotificationBuffer buffer = new NotificationBuffer();
			manager.PreLoadSetupCalled = false;
			manager.ProcessRecordCalled = false;
			manager.LoadFromFile(buffer);
			AssertEquals("Bufer has no errors: Buffer contains:" + System.Environment.NewLine + buffer.AsString, false, buffer.HasErrors);
			AssertEquals("PreLoadSetup was not called", false, manager.PreLoadSetupCalled);
			AssertEquals("ProcessRecord was not called", false, manager.ProcessRecordCalled);
		}

		[TestDate(2005, 11, 22, 20, 40, 35)]
		public void TestMoveFileToProcessedDirectory()
		{
			string testFilename = Path.Combine(Env.TempPath, "TestFileToBeMoved.txt");
			File.Copy(SmallFilename, testFilename, true);
			Assert(string.Format("PreCondition: Testfile {0} should exists", testFilename), File.Exists(testFilename));
			ImportManagerTestClass manager = new ImportManagerTestClass(Factory, testFilename);
			manager.ProcessedDirectoryTest = ProcessedDirectory;
			ZString checkProcessedDirectory = ProcessedDirectory + @"\20051122\";
			if (Directory.Exists(checkProcessedDirectory))
			{
				TempDirectory.DeleteDirectory(checkProcessedDirectory);
			}

			AssertEquals("PreCondition: Processed directory '" + checkProcessedDirectory + "' should not exist", false, Directory.Exists(checkProcessedDirectory));
			manager.MoveFileToProcessedDirectory();
			AssertEquals("Processed Directory '" + checkProcessedDirectory + "' should exist", true, Directory.Exists(checkProcessedDirectory));
			ZString processedFile = Path.Combine(checkProcessedDirectory, "TestFileToBeMoved.txt.20051122.204035");
			AssertEquals("ProcessedFile '" + processedFile + "' should exist", true, File.Exists(processedFile));
		}

#endregion
#region Implementation
#region ProcessedDirectory
		ZString ProcessedDirectory
		{
			get
			{
				if (fProcessedDirectory.IsEmpty)
				{
					fProcessedDirectory = Path.Combine(Env.TempPath, "Processed");
				}

				if (!Directory.Exists(fProcessedDirectory))
				{
					Directory.CreateDirectory(fProcessedDirectory);
				}

				return fProcessedDirectory;
			}
		}

		ZString fProcessedDirectory;
#endregion
		void Manager_LoadProgress(object sender, TNTProgressEventArgs e)
		{
			Manager_LoadProgressCalled = true;
		}

		bool Manager_LoadProgressCalled;
		ZString ValidFilename
		{
			get
			{
				return fValidFilename;
			}
		}

		ZString EmptyFilename
		{
			get
			{
				return fEmptyFilename;
			}
		}

		ZString SmallFilename
		{
			get
			{
				return fSmallFilename;
			}
		}

		string fValidFilename;
		string fEmptyFilename;
		string fSmallFilename;
		EmbeddedResourceRetriever resourceRetriever;

		protected override void SetUp()
		{
			base.SetUp();
			resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly);
			fValidFilename = resourceRetriever.SaveResourceToFile("Enterprise.Client.TNT.Testing.DataManipulation.AirCargo.INDInterface.Testing.SYD.IND.20050809.141533.ok");
			fEmptyFilename = resourceRetriever.SaveResourceToFile("Enterprise.Client.TNT.Testing.DataManipulation.AirCargo.INDInterface.Testing.SYD.IND.EmptyFile.ok");
			fSmallFilename = resourceRetriever.SaveResourceToFile("Enterprise.Client.TNT.Testing.DataManipulation.AirCargo.INDInterface.Testing.SYD.IND.20050822.201010.ok");
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ImportManagerTestClass(Factory, "blah");
		}

		protected override void TearDown()
		{
			TempDirectory.DeleteDirectory(ProcessedDirectory);
			base.TearDown();
			resourceRetriever.Dispose();
		}

		public class ImportManagerTestClass : ImportManager
		{
			public ImportManagerTestClass(BusinessObjectFactory factory, ZString filename) : base(factory, filename)
			{
				fNoOfTimeProcessRecordWasCalled = 0;
				PreLoadSetupCalled = false;
				ProcessRecordCalled = false;
			}

			public string ProcessedDirectoryTest;
			protected override string ProcessedDirectory
			{
				get
				{
					return ProcessedDirectoryTest;
				}
			}

			public bool PreLoadSetupCalled;
			protected override void PreLoadSetup()
			{
				PreLoadSetupCalled = true;
			}

			public int NoOfTimeProcessRecordWasCalled
			{
				get
				{
					return fNoOfTimeProcessRecordWasCalled;
				}
			}

			int fNoOfTimeProcessRecordWasCalled;
			public bool ProcessRecordCalled;
			public INotification ProcessRecordNotifyEvent;
			protected override void ProcessRecord(IQDownBaseRecord record, INotifications notify)
			{
				fNoOfTimeProcessRecordWasCalled++;
				if (ProcessRecordNotifyEvent != null)
				{
					notify.Notify(ProcessRecordNotifyEvent);
				}

				ProcessRecordCalled = true;
			}
		}
#endregion
	}
}
