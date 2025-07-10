using System;
using System.IO;
using System.Text;
using System.Xml;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Billing.Integration;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Xml;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation.Testing
{
	class XmlMultiTypeImportTaskTest : XmlImportTaskTest
	{
		#region Test Cases

		#region Multiple Type Xml Import Files

		public void TestUnsupportedXMLImportViaServiceTask()
		{
			ZString testDirectory = Env.TempPath;
			ZString fileName = GetFileName();
			ZString unsupportedRootCollectionName = "Unsupported";
			string expectedNotificationMessage = "This XML file cannot be imported via Service Task. Please import this file via the User Interface.";

			CreateTestMultiTypeFile(testDirectory, fileName, new[] { unsupportedRootCollectionName }, true);
			SetRegistryItem(testDirectory);

			ImportDirector.ImportTasks.Clear();
			ImportDirector.ImportTasks.Add(new XmlMultiTypeImportTask(ImportDirector, Notify));

			Task.Run();

			AssertEquals(expectedNotificationMessage, ErrorReporter.LastExceptionReported.Message);
			Assert("Should suggest importing via GUI", Notify.AsString.Contains(expectedNotificationMessage));

			ErrorReporter.Clear();
		}

		[NUnit.Framework.TestDate(2007, 10, 10, 10, 10, 10, 100)]
		public void TestMultiCollectionFileWithInterchangeInfo()
		{
			ZString testDirectory = Env.TempPath;
			ZString fileName = GetFileName();
			ZString unknownRootCollectionName = "Unknown";
			ZString newFileName1 = XmlMultiTypeImportTask.ConstructNewMessageFileNameForTest(fileName, Importer1.RootCollectionElementName);
			ZString newFileName2 = XmlMultiTypeImportTask.ConstructNewMessageFileNameForTest(fileName, Importer2.RootCollectionElementName);
			ZString newFileName3 = XmlMultiTypeImportTask.ConstructBadMessageFileNameForTest(fileName, Importer3.RootCollectionElementName);
			ZString newFileName4 = XmlMultiTypeImportTask.ConstructBadMessageFileNameForTest(fileName, Importer4.RootCollectionElementName);
			ZString newFileName5 = XmlMultiTypeImportTask.ConstructBadMessageFileNameForTest(fileName, unknownRootCollectionName);
			ZString newFilePath1 = Path.Combine(TempTaskDirectoryName1, newFileName1);
			ZString newFilePath2 = Path.Combine(TempTaskDirectoryName2, newFileName2);
			ZString newFilePath3 = Path.Combine(BadMessageFileDirectoryName, newFileName3);
			ZString newFilePath4 = Path.Combine(BadMessageFileDirectoryName, newFileName4);
			ZString newFilePath5 = Path.Combine(BadMessageFileDirectoryName, newFileName5);
			ZString expectedNewFileNotificationMessage1 = "New file has been created: " + newFilePath1;
			ZString expectedNewFileNotificationMessage2 = "New file has been created: " + newFilePath2;
			ZString expectedNewFileNotificationMessage3 = "New file has been created: " + newFilePath3;
			ZString expectedNewFileNotificationMessage4 = "New file has been created: " + newFilePath4;
			ZString expectedNewFileNotificationMessage5 = "New file has been created: " + newFilePath5;
			ZString expectedMissingDataDirectoryNotificationMessage1 = ImportTaskWithoutDir1.SearchingDirectoryCaption;
			ZString expectedMissingDataDirectoryNotificationMessage2 = ImportTaskWithoutDir2.SearchingDirectoryCaption;
			ZString expectedUnkownRecordTypeNotificationMessage = unknownRootCollectionName + " record type is unknown.";

			CreateTestMultiTypeFile(testDirectory, fileName, new ZString[] {
					Importer1.RootCollectionElementName,
					Importer2.RootCollectionElementName,
					Importer3.RootCollectionElementName,
					Importer4.RootCollectionElementName,
					unknownRootCollectionName
					}, true);
			SetRegistryItem(testDirectory);
			Task.Run();
			AssertEquals(true, Notify.HasErrors);
			AssertEquals("Should contain Missing Data Directory error", true, Notify.ContainsNotificationType(ErrorType.MissingDataDirectory));
			AssertEquals("Should contain Missing Data Directory error notification: " + expectedMissingDataDirectoryNotificationMessage1, true, Notify.AsString.IndexOf(expectedMissingDataDirectoryNotificationMessage1) > -1);
			AssertEquals("Should contain Missing Data Directory error notification: " + expectedMissingDataDirectoryNotificationMessage2, true, Notify.AsString.IndexOf(expectedMissingDataDirectoryNotificationMessage2) > -1);
			AssertEquals("Should contain Unkown Record Type error", true, Notify.ContainsNotificationType(ErrorType.UnknownRecordType));
			AssertEquals("Should contain Unkown Record Type error notification: " + expectedUnkownRecordTypeNotificationMessage, true, Notify.AsString.IndexOf(expectedUnkownRecordTypeNotificationMessage) > -1);
			AssertEquals("Import file should be deleted", false, File.Exists(Path.Combine(testDirectory, fileName)));
			AssertEquals("New Import file (" + newFilePath1 + ") should be created under ImportTaskWithDir1 file directory", true, File.Exists(newFilePath1));
			AssertEquals("New Import file (" + newFilePath2 + ") should be created under ImportTaskWithDir2 file directory", true, File.Exists(newFilePath2));
			AssertEquals("New Import file (" + newFilePath3 + ") should be created under bad messages file directory", true, File.Exists(newFilePath3));
			AssertEquals("New Import file (" + newFilePath4 + ") should be created under bad messages file directory", true, File.Exists(newFilePath4));
			AssertEquals("New Import file (" + newFilePath5 + ") should be created under bad messages file directory", true, File.Exists(newFilePath5));
			AssertEquals("Should contain info notification: " + expectedNewFileNotificationMessage1, true, Notify.AsString.IndexOf(expectedNewFileNotificationMessage1) > -1);
			AssertEquals("Should contain info notification: " + expectedNewFileNotificationMessage2, true, Notify.AsString.IndexOf(expectedNewFileNotificationMessage2) > -1);
			AssertEquals("Should contain info notification: " + expectedNewFileNotificationMessage3, true, Notify.AsString.IndexOf(expectedNewFileNotificationMessage3) > -1);
			AssertEquals("Should contain info notification: " + expectedNewFileNotificationMessage4, true, Notify.AsString.IndexOf(expectedNewFileNotificationMessage4) > -1);
			AssertEquals("Should contain info notification: " + expectedNewFileNotificationMessage5, true, Notify.AsString.IndexOf(expectedNewFileNotificationMessage5) > -1);
			AssertXmlFileContent(newFilePath1, GenerateCollectionXml(Importer1.RootCollectionElementName, true, 1));
			AssertXmlFileContent(newFilePath2, GenerateCollectionXml(Importer2.RootCollectionElementName, true, 2));
			AssertXmlFileContent(newFilePath3, GenerateCollectionXml(Importer3.RootCollectionElementName, true, 3));
			AssertXmlFileContent(newFilePath4, GenerateCollectionXml(Importer4.RootCollectionElementName, true, 4));
			AssertXmlFileContent(newFilePath5, GenerateCollectionXml(unknownRootCollectionName, true, 5));
		}

		[NUnit.Framework.TestDate(2007, 10, 10, 10, 10, 10, 100)]
		public void TestMultiCollectionFileWithNoInterchangeInfo()
		{
			ZString testDirectory = Env.TempPath;
			ZString fileName = GetFileName();
			ZString unknownRootCollectionName = "Unknown";
			ZString newFilePath = Path.Combine(BadMessageFileDirectoryName, XmlMultiTypeImportTask.ConstructBadMessageFileNameForTest(fileName));
			ZString expectedFileMoveNotificationMessage = "Original file has been moved from " + Path.Combine(testDirectory, fileName) + " to " + newFilePath;
			ZString originalFileData;

			CreateTestMultiTypeFile(testDirectory, fileName, new ZString[] {
					Importer1.RootCollectionElementName,
					Importer2.RootCollectionElementName,
					Importer3.RootCollectionElementName,
					Importer4.RootCollectionElementName,
					unknownRootCollectionName
					}, false);
			SetRegistryItem(testDirectory);
			originalFileData = ReadFileContent(Path.Combine(testDirectory, fileName));
			Task.Run();
			AssertEquals(true, Notify.HasErrors);
			AssertEquals("Should contain xml schema validation error", true, Notify.ContainsNotificationType(ErrorType.XmlSchemaValidation));
			AssertEquals("Import file should be deleted", false, File.Exists(Path.Combine(testDirectory, fileName)));
			AssertEquals("Import file should be renamed and moved to bad messages file directory", true, File.Exists(newFilePath));
			AssertEquals("Should contain info notification: " + expectedFileMoveNotificationMessage, true, Notify.AsString.IndexOf(expectedFileMoveNotificationMessage) > -1);
			AssertInvalidXmlSchemaFileContent(newFilePath, originalFileData);
		}

		#endregion

		#region Single Type Xml Import Files

		[NUnit.Framework.TestDate(2007, 10, 10, 10, 10, 10, 100)]
		public void TestCollectionWithInterchangeInfoWithImporterDirectorySet()
		{
			ZString testDirectory = Env.TempPath;
			ZString rootCollectionElementName = Importer1.RootCollectionElementName;
			ZString fileName = GetFileName();
			ZString newFileName = XmlMultiTypeImportTask.ConstructNewMessageFileNameForTest(fileName, rootCollectionElementName);
			ZString newFilePath = Path.Combine(TempTaskDirectoryName1, newFileName);
			ZString expectedNewFileNotificationMessage = "New file has been created: " + newFilePath;

			CreateTestSingleTypeFile(testDirectory, fileName, rootCollectionElementName, true);
			SetRegistryItem(testDirectory);
			Task.Run();
			AssertEquals(false, Notify.HasErrors);
			AssertEquals("Import file should be deleted", false, File.Exists(Path.Combine(testDirectory, fileName)));
			AssertEquals("New Import file (" + newFilePath + ") should be created under ImportTaskWithDir1 file directory", true, File.Exists(newFilePath));
			AssertEquals("Should contain info notification: " + expectedNewFileNotificationMessage, true, Notify.AsString.IndexOf(expectedNewFileNotificationMessage) > -1);
			AssertXmlFileContent(newFilePath, GenerateCollectionXml(rootCollectionElementName, true));
		}

		[NUnit.Framework.TestDate(2007, 10, 10, 10, 10, 10, 100)]
		public void TestCollectionWithNoInterchangeInfoWithImporterDirectorySet()
		{
			ZString testDirectory = Env.TempPath;
			ZString rootCollectionElementName = Importer1.RootCollectionElementName;
			ZString fileName = GetFileName();
			ZString newFileName = XmlMultiTypeImportTask.ConstructNewMessageFileNameForTest(fileName, rootCollectionElementName);
			ZString newFilePath = Path.Combine(TempTaskDirectoryName1, newFileName);
			ZString expectedNewFileNotificationMessage = "New file has been created: " + newFilePath;

			CreateTestSingleTypeFile(testDirectory, fileName, rootCollectionElementName, false);
			SetRegistryItem(testDirectory);
			Task.Run();
			AssertEquals(false, Notify.HasErrors);
			AssertEquals("Import file should be deleted", false, File.Exists(Path.Combine(testDirectory, fileName)));
			AssertEquals("New Import file (" + newFilePath + ") should be created under ImportTaskWithDir1 file directory", true, File.Exists(newFilePath));
			AssertEquals("Should contain info notification: " + expectedNewFileNotificationMessage, true, Notify.AsString.IndexOf(expectedNewFileNotificationMessage) > -1);
			AssertXmlFileContent(newFilePath, GenerateCollectionXml(rootCollectionElementName, false));
		}

		[NUnit.Framework.TestDate(2007, 10, 10, 10, 10, 10, 100)]
		public void TestCollectionWithInterchangeInfoWithImporterDirectoryNotSet()
		{
			ZString testDirectory = Env.TempPath;
			ZString rootCollectionElementName = Importer3.RootCollectionElementName;
			ZString fileName = GetFileName();
			ZString newFileName = XmlMultiTypeImportTask.ConstructBadMessageFileNameForTest(fileName, rootCollectionElementName);
			ZString newFilePath = Path.Combine(BadMessageFileDirectoryName, newFileName);
			ZString expectedNewFileNotificationMessage = "New file has been created: " + newFilePath;
			ZString expectedMissingDataDirectoryNotificationMessage = ImportTaskWithoutDir1.SearchingDirectoryCaption;

			CreateTestSingleTypeFile(testDirectory, fileName, rootCollectionElementName, true);
			SetRegistryItem(testDirectory);
			Task.Run();
			AssertEquals(true, Notify.HasErrors);
			AssertEquals("Should contain Missing Data Directory error", true, Notify.ContainsNotificationType(ErrorType.MissingDataDirectory));
			AssertEquals("Should contain Missing Data Directory error notification: " + expectedMissingDataDirectoryNotificationMessage, true, Notify.AsString.IndexOf(expectedMissingDataDirectoryNotificationMessage) > -1);
			AssertEquals("Import file should be deleted", false, File.Exists(Path.Combine(testDirectory, fileName)));
			AssertEquals("New Import file (" + newFilePath + ") should be created under bad messages file directory", true, File.Exists(newFilePath));
			AssertEquals("Should contain info notification: " + expectedNewFileNotificationMessage, true, Notify.AsString.IndexOf(expectedNewFileNotificationMessage) > -1);
			AssertXmlFileContent(newFilePath, GenerateCollectionXml(rootCollectionElementName, true));
		}

		[NUnit.Framework.TestDate(2007, 10, 10, 10, 10, 10, 100)]
		public void TestCollectionWithNoInterchangeInfoWithImporterDirectoryNotSet()
		{
			ZString testDirectory = Env.TempPath;
			ZString rootCollectionElementName = Importer3.RootCollectionElementName;
			ZString fileName = GetFileName();
			ZString newFileName = XmlMultiTypeImportTask.ConstructBadMessageFileNameForTest(fileName, rootCollectionElementName);
			ZString newFilePath = Path.Combine(BadMessageFileDirectoryName, newFileName);
			ZString expectedNewFileNotificationMessage = "New file has been created: " + newFilePath;
			ZString expectedMissingDataDirectoryNotificationMessage = ImportTaskWithoutDir1.SearchingDirectoryCaption;

			CreateTestSingleTypeFile(testDirectory, fileName, rootCollectionElementName, false);
			SetRegistryItem(testDirectory);
			Task.Run();
			AssertEquals(true, Notify.HasErrors);
			AssertEquals("Should contain Missing Data Directory error", true, Notify.ContainsNotificationType(ErrorType.MissingDataDirectory));
			AssertEquals("Should contain Missing Data Directory error notification: " + expectedMissingDataDirectoryNotificationMessage, true, Notify.AsString.IndexOf(expectedMissingDataDirectoryNotificationMessage) > -1);
			AssertEquals("Import file should be deleted", false, File.Exists(Path.Combine(testDirectory, fileName)));
			AssertEquals("New Import file (" + newFilePath + ") should be created under bad messages file directory", true, File.Exists(newFilePath));
			AssertEquals("Should contain info notification: " + expectedNewFileNotificationMessage, true, Notify.AsString.IndexOf(expectedNewFileNotificationMessage) > -1);
			AssertXmlFileContent(newFilePath, GenerateCollectionXml(rootCollectionElementName, false));
		}

		[NUnit.Framework.TestDate(2007, 10, 10, 10, 10, 10, 100)]
		public void TestSingleTypeFileWithInterchangeInfoNoImporter()
		{
			ZString testDirectory = Env.TempPath;
			ZString rootCollectionElementName = "TestCollection";
			ZString fileName = GetFileName();
			ZString newFileName = XmlMultiTypeImportTask.ConstructBadMessageFileNameForTest(fileName, rootCollectionElementName);
			ZString newFilePath = Path.Combine(BadMessageFileDirectoryName, newFileName);
			ZString expectedNewFileNotificationMessage = "New file has been created: " + newFilePath;
			ZString expectedUnkownRecordTypeNotificationMessage = rootCollectionElementName + " record type is unknown.";

			CreateTestSingleTypeFile(testDirectory, fileName, rootCollectionElementName, true);
			SetRegistryItem(testDirectory);
			Task.Run();
			AssertEquals(true, Notify.HasErrors);
			AssertEquals("Should contain Unkown Record Type error", true, Notify.ContainsNotificationType(ErrorType.UnknownRecordType));
			AssertEquals("Should contain Unkown Record Type error notification: " + expectedUnkownRecordTypeNotificationMessage, true, Notify.AsString.IndexOf(expectedUnkownRecordTypeNotificationMessage) > -1);
			AssertEquals("Import file should be deleted", false, File.Exists(Path.Combine(testDirectory, fileName)));
			AssertEquals("New Import file (" + newFilePath + ") should be created under bad messages file directory", true, File.Exists(newFilePath));
			AssertEquals("Should contain info notification: " + expectedNewFileNotificationMessage, true, Notify.AsString.IndexOf(expectedNewFileNotificationMessage) > -1);
			AssertXmlFileContent(newFilePath, GenerateCollectionXml(rootCollectionElementName, true));
		}

		[NUnit.Framework.TestDate(2007, 10, 10, 10, 10, 10, 100)]
		public void TestSingleTypeFileNoInterchangeInfoNoImporter()
		{
			ZString testDirectory = Env.TempPath;
			ZString rootCollectionElementName = "TestCollection";
			ZString fileName = GetFileName();
			ZString newFileName = XmlMultiTypeImportTask.ConstructBadMessageFileNameForTest(fileName, rootCollectionElementName);
			ZString newFilePath = Path.Combine(BadMessageFileDirectoryName, newFileName);
			ZString expectedNewFileNotificationMessage = "New file has been created: " + newFilePath;
			ZString expectedUnkownRecordTypeNotificationMessage = rootCollectionElementName + " record type is unknown.";

			CreateTestSingleTypeFile(testDirectory, fileName, rootCollectionElementName, false);
			SetRegistryItem(testDirectory);
			Task.Run();
			AssertEquals(true, Notify.HasErrors);
			AssertEquals("Should contain Unkown Record Type error", true, Notify.ContainsNotificationType(ErrorType.UnknownRecordType));
			AssertEquals("Should contain Unkown Record Type error notification: " + expectedUnkownRecordTypeNotificationMessage, true, Notify.AsString.IndexOf(expectedUnkownRecordTypeNotificationMessage) > -1);
			AssertEquals("Import file should be deleted", false, File.Exists(Path.Combine(testDirectory, fileName)));
			AssertEquals("New Import file (" + newFilePath + ") should be created under bad messages file directory", true, File.Exists(newFilePath));
			AssertEquals("Should contain info notification: " + expectedNewFileNotificationMessage, true, Notify.AsString.IndexOf(expectedNewFileNotificationMessage) > -1);
			AssertXmlFileContent(newFilePath, GenerateCollectionXml(rootCollectionElementName, false));
		}

		#endregion

		#region Verify XmlMultiTypeImportTask Prerequisites

		public void TestBadMessageFileDirectoryRequirement()
		{
			ZString expectedWarningMessage = "No " + (SystemDataRegistry.Instance.UnprocessedMessagesDataDirectory as IRegistryItemInternals).Location + " registry item set. Associated import not running.";
			ZString badMessageFilePath = ZString.Empty;
			Notify.Clear();
			SystemDataRegistry.Instance.UnprocessedMessagesDataDirectory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, badMessageFilePath);
			XmlMultiTypeImportTask task = new XmlMultiTypeImportTask(ImportDirector, Notify);
			task.Run();
			AssertEquals("Should have warnings", true, Notify.HasWarnings);
			AssertEquals("Warning should contain: '" + expectedWarningMessage + "'", true, Notify.AsString.IndexOf(expectedWarningMessage) > -1);

			badMessageFilePath = Path.Combine(Env.TempPath, "BogusDir");
			expectedWarningMessage = "The directory " + badMessageFilePath + " does not exist. Associated import not running";
			AssertEquals("Precondition: Directory should not exist", false, Directory.Exists(badMessageFilePath));
			Notify.Clear();
			SystemDataRegistry.Instance.UnprocessedMessagesDataDirectory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, badMessageFilePath);
			task = new XmlMultiTypeImportTask(ImportDirector, Notify);
			task.Run();
			AssertEquals("Should have warnings", true, Notify.HasWarnings);
			AssertEquals("Warning should contain: '" + expectedWarningMessage + "'", true, Notify.AsString.IndexOf(expectedWarningMessage) > -1);
		}

		#endregion

		#endregion

		#region Implementation

		protected ZString ReadFileContent(ZString filePath)
		{
			ZString fileData = ZString.Empty;
			using (StreamReader reader = new StreamReader(filePath))
			{
				fileData = reader.ReadToEnd();
			}
			return fileData;
		}

		protected void AssertXmlFileContent(ZString filePath, ZString expectedXml)
		{
			using (XmlTextReader reader = new XmlTextReader(filePath))
			{
				reader.MoveToContent();
				ZString fileXml = reader.ReadOuterXml();
				AssertEquals("Invalid xml in file " + filePath, expectedXml, fileXml);
			}
		}

		protected override void CreateTestFile(string testDirectory, string fileName)
		{
			using (XmlTextWriter writer = new XmlTextWriter(Path.Combine(testDirectory, fileName), Encoding.ASCII))
			{
				writer.Formatting = Formatting.Indented;
				Xsd.XmlInterchange interchange = Xsd.XmlInterchange.NewPopulatedInterchange(Factory);
				XmlValueObjectSerializer serialiser = new XmlValueObjectSerializer(typeof(Xsd.XmlInterchange));
				serialiser.Serialize(writer, interchange);
				writer.Close();
			}
		}

		protected void CreateTestMultiTypeFile(ZString testDirectory, ZString fileName, ZString[] collectionNames, bool addInterchangeInfo)
		{
			using (XmlTextWriter writer = new XmlTextWriter(Path.Combine(testDirectory, fileName), Encoding.ASCII))
			{
				writer.WriteStartDocument();
				writer.WriteRaw(GenerateCollectionsXml(collectionNames, addInterchangeInfo));
			}
		}

		protected void CreateTestSingleTypeFile(ZString testDirectory, ZString fileName, ZString collectionName, bool addInterchangeInfo)
		{
			using (XmlTextWriter writer = new XmlTextWriter(Path.Combine(testDirectory, fileName), Encoding.ASCII))
			{
				writer.WriteStartDocument();
				writer.WriteRaw(GenerateCollectionXml(collectionName, addInterchangeInfo));
			}
		}

		protected ZString GenerateInterchangeInfo(ZString payload)
		{
			return "<XmlInterchange><InterchangeInfo>Blah</InterchangeInfo><Payload>" + payload + "</Payload></XmlInterchange>";
		}

		protected ZString GenerateCollectionsXml(ZString[] collectionNames, bool addInterchangeInfo)
		{
			ZString result = ZString.Empty;
			for (int f = 0; f < collectionNames.Length; f++)
			{
				result += GenerateCollectionXml(collectionNames[f], false, f + 1);
			}
			if (addInterchangeInfo)
			{
				result = GenerateInterchangeInfo(result);
			}
			return result;
		}

		protected ZString GenerateCollectionXml(ZString collectionName, bool addInterchangeInfo)
		{
			return GenerateCollectionXml(collectionName, addInterchangeInfo, 1);
		}

		protected ZString GenerateCollectionXml(ZString collectionName, bool addInterchangeInfo, int index)
		{
			return GenerateCollectionXml(collectionName, addInterchangeInfo, "<Blah" + index + "> Test" + index + "</Blah" + index + ">");
		}

		protected ZString GenerateCollectionXml(ZString collectionName, bool addInterchangeInfo, ZString innerXml)
		{
			ZString result = "<" + collectionName + ">" + innerXml + "</" + collectionName + ">";
			if (addInterchangeInfo)
			{
				result = GenerateInterchangeInfo(result);
			}
			return result;
		}

		protected override ImportTask GetImportTask()
		{
			return new XmlMultiTypeImportTask(ImportDirector, Notify);
		}

		protected override ZString GetFileName()
		{
			return "MultiTypeSample.xml";
		}

		protected override void SetRegistryItem(ZString directoryName)
		{
			SystemDataRegistry.Instance.DefaultMessagesDataImportDirectory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, directoryName);
		}

		#region Test Help Clases

		class TestBatchImportDirector : BatchImportDirector
		{
			public TestBatchImportDirector(ImportTask task1, ImportTask task2, ImportTask task3, ImportTask task4)
				: base(null)
			{
				ImportTasks.RemoveRange(0, ImportTasks.Count);
				ImportTasks.Add(task1);
				ImportTasks.Add(task2);
				ImportTasks.Add(task3);
				ImportTasks.Add(task4);
			}
		}

		class TestDataImporter : XmlDataImporter
		{
			public TestDataImporter(ZString rootCollectionElementName)
				: base(null)
			{
				this.RootCollectionElementName = rootCollectionElementName;
			}

			public override bool CanImportXmlCollection(string rootCollectionElementName, string collectionXml)
			{
				return rootCollectionElementName == this.RootCollectionElementName;
			}

			public ZString RootCollectionElementName;
		}

		class TestXmlImportTask : XmlImportTask
		{
			public TestXmlImportTask(ZString rootCollectionElementName, StringRegistryItem registryPath, INotifications notify, GuidRegistryItem notificationGroup)
				: base(registryPath, notify, notificationGroup, BillingInterfaceName.Test)
			{
				this.RootCollectionElementName = rootCollectionElementName;
			}

			protected override DataImporter NewImporter()
			{
				return new TestDataImporter(RootCollectionElementName);
			}

			public ZString RootCollectionElementName;
		}

		#endregion

		StringRegistryItem GetTempRegistryDirectory(ZString name, ZString caption, ZString value)
		{
			StringRegistryItem result = new StringRegistryItem(
				name,
				(NoResString)"TEST",
				(NoResString)caption,
				(NoResString)"TEST",
				RegistryStorageFlags.System | RegistryStorageFlags.Company);

			result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.DirectoryBrowser);
			result.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
			return result;
		}

		GuidRegistryItem GetTempNotificationEmailGroup(ZString name, ZString caption)
		{
			GuidRegistryItem result = new GuidRegistryItem(
				name,
				(NoResString)"TEST",
				(NoResString)caption,
				(NoResString)"TEST",
				RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				RegistryOptions.Default,
				RegistryFactory.Instance.GetGroupPK("ALL"));

			result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
			GlbGroup postMasterGroup = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			postMasterGroup.Staff[0].GS_EmailAddress = "a@b.c";
			result.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, postMasterGroup.PK.ToGuid());
			return result;
		}

		protected override void SetUp()
		{
			base.SetUp();

			TempTaskDirectoryName1 = Temp.GetNewTempSubdirectory();
			TempTaskDirectoryName2 = Temp.GetNewTempSubdirectory();
			Importer1 = new TestDataImporter("Object1");
			Importer2 = new TestDataImporter("Object2");
			Importer3 = new TestDataImporter("Object3");
			Importer4 = new TestDataImporter("Object4");
			ImportTaskWithDir1 = new TestXmlImportTask("Object1", GetTempRegistryDirectory("TestDirectory1", "TaskWithDir1 Directory Name", TempTaskDirectoryName1), null, GetTempNotificationEmailGroup("TestGroup1", "TaskWithDir1 Notification Group"));
			ImportTaskWithDir2 = new TestXmlImportTask("Object2", GetTempRegistryDirectory("TestDirectory2", "TaskWithDir2 Directory Name", TempTaskDirectoryName2), null, GetTempNotificationEmailGroup("TestGroup2", "TaskWithDir2 Notification Group"));
			ImportTaskWithoutDir1 = new TestXmlImportTask("Object3", GetTempRegistryDirectory("TestDirectory3", "TaskWithoutDir1 Directory Name", ZString.Empty), null, GetTempNotificationEmailGroup("TestGroup3", "TaskWithoutDir1 Notification Group"));
			ImportTaskWithoutDir2 = new TestXmlImportTask("Object4", GetTempRegistryDirectory("TestDirectory4", "TaskWithoutDir2 Directory Name", ZString.Empty), null, GetTempNotificationEmailGroup("TestGroup4", "TaskWithoutDir2 Notification Group"));
			ImportDirector = new TestBatchImportDirector(ImportTaskWithDir1, ImportTaskWithDir2, ImportTaskWithoutDir1, ImportTaskWithoutDir2);
		}

		protected override void TearDown()
		{
			base.TearDown();
			DeleteDirectory(TempTaskDirectoryName1);
			DeleteDirectory(TempTaskDirectoryName2);
		}

		BatchImportDirector ImportDirector;

		XmlImportTask ImportTaskWithDir1;
		XmlImportTask ImportTaskWithDir2;
		XmlImportTask ImportTaskWithoutDir1;
		XmlImportTask ImportTaskWithoutDir2;
		TestDataImporter Importer1;
		TestDataImporter Importer2;
		TestDataImporter Importer3;
		TestDataImporter Importer4;
		ZString TempTaskDirectoryName1;
		ZString TempTaskDirectoryName2;

		#endregion
	}
}
