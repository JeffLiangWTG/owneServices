using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.Data.Utils;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Billing.Integration;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation.Testing
{
	class XmlImportTaskTest : ImportTaskTest
	{
		#region TestEmailIsSentWhenImportSucceeded

		public void TestEmailIsSentWhenImportSucceeded()
		{
			string testDirectory = Env.TempPath;
			SystemDataRegistry.Instance.EmailNotificationForErrorsOnly.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			SystemDataRegistry.Instance.ConsolsDataImportDirectory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, testDirectory);
			NotificationDataRegistry.Instance.ConsolShipmentImportNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, PostMasterGroup.PK.ToGuid());

			AssertEquals("Precondition: Saved Emails", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			try
			{
				using (StreamWriter writer = new StreamWriter(Path.Combine(testDirectory, "TempFile.xml")))
				{
					writer.Write("Sample text");
				}
				XmlImportTask task = new XmlImportTaskForTesting();
				task.Run();
				AssertEquals("One new email should have been created", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				EmailDef item = Env.OutgoingMailManager.EmailsCreated[0];
				Assert("Subject should contain success notification", item.Subject.Contains("Test Xml Import Succeeded"));
				AssertEquals("No attachments expected", 0, item.Attachments.Count);
			}
			finally
			{
				DeleteIfExists(testDirectory + @"\TempFile.xml");
			}
		}

		#endregion

		#region TestEmailIsNotSentWhenDisabledAndImportSucceeded

		public void TestEmailIsNotSentWhenDisabledAndImportSucceeded()
		{
			string testDirectory = Env.TempPath;
			SystemDataRegistry.Instance.EmailNotificationForErrorsOnly.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			SystemDataRegistry.Instance.ConsolsDataImportDirectory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, testDirectory);
			NotificationDataRegistry.Instance.ConsolShipmentImportNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, PostMasterGroup.PK.ToGuid());

			AssertEquals("Precondition: Saved Emails", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			try
			{
				using (StreamWriter writer = new StreamWriter(Path.Combine(testDirectory, "TempFile.xml")))
				{
					writer.Write("Sample text");
				}
				XmlImportTask task = new XmlImportTaskForTesting();
				task.Run();

				AssertEquals("Expecting no new emails created", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			}
			finally
			{
				DeleteIfExists(testDirectory + @"\TempFile.xml");
			}
		}

		#endregion

		#region TestEmailIsSendWhenImportingDataError

		public void TestEmailIsSendWhenImportingDataError()
		{
			string testDirectory = Env.TempPath;
			SystemDataRegistry.Instance.ConsolsDataImportDirectory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, testDirectory);
			NotificationDataRegistry.Instance.ConsolShipmentImportNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, PostMasterGroup.PK.ToGuid());

			AssertEquals("Precondition: Saved Emails", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			try
			{
				using (StreamWriter writer = new StreamWriter(Path.Combine(testDirectory, "TempFile.xml")))
				{
					writer.Write("Sample text");
				}
				XmlImportTask task = new XmlImportTaskWithError();
				task.Run();
				AssertEquals("One new email should have been created", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				EmailDef item = Env.OutgoingMailManager.EmailsCreated[0];
				Assert("Subject should contain failure notification", item.Subject.Contains("Test Xml Import Failed"));
				AssertEquals("Should contain the notification in the body", true, item.Body.IndexOf(ErrorType.ImportingDataError.Message + " (Importing Error)") > -1);
				AssertEquals("One attachment expected", 1, item.Attachments.Count);
			}
			finally
			{
				DeleteIfExists(testDirectory + @"\TempFile.xml");
			}
		}

		#endregion

		#region DataErrorPreventSaveTest

		public void TestNoBusinessObjectLeftInCacheAfterErrorPreventSaveOccured()
		{
			Guid groupPk = Enterprise.Core.Constants.Groups.AllPK;
			GlbGroup group = Factory.Load<GlbGroup>(groupPk);
			var staff = group.Staff.AddNew();
			staff.GS_EmailAddress = "test@test.com";
			staff.GS_Code = "ZAC";

			NotificationDataRegistry.Instance.OrganisationImportNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, groupPk);

			string testDir = Env.TempPath;

			SystemDataRegistry.Instance.OrganisationDataImportDirectory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, testDir);

			Factory.Save();

			var testFile = Path.Combine(testDir, "TempFile.xml");

			var buffer = new NotificationBuffer();
			var task = new XmlImportTaskWithErrorPreventSave(buffer);
			var orgCount = Factory.GetDatabaseCount(typeof(OrgHeader));

			try
			{
				CreateTestFileFromEmbeddedResource("Enterprise.ServiceManager.Tasks.XMLAutomation.Test.Testing.OrganisationForErrorPreventSaveTest1.xml", testFile);
				Assert("No error notifications found", !buffer.HasErrors);

				task.Run();

				Assert("Error that prevents save found", buffer.ContainsNotificationType(ErrorType.DataErrorPreventSave));
				Assert("No organisations has been saved", orgCount == Factory.GetDatabaseCount(typeof(OrgHeader)));

				CreateTestFileFromEmbeddedResource("Enterprise.ServiceManager.Tasks.XMLAutomation.Test.Testing.OrganisationForErrorPreventSaveTest2.xml", testFile);

				task.Run();

				Assert("Expecting only one organisation to be saved", (orgCount + 1) == Factory.GetDatabaseCount(typeof(OrgHeader)));
			}
			finally
			{
				DeleteIfExists(testFile);
			}
		}

		class OrganisationDataAdapterWithErrorPreventSave : OrganisationValueObjectDataAdapter
		{
			protected override void ImportFromValueObjectCore(OrgHeader bizObj, DataTransfer.Xml.XsdVersion1.Organisation valueObj, IValueObjectImportContext context)
			{
				base.ImportFromValueObjectCore(bizObj, valueObj, context);

				if (valueObj.OrganisationDetails.Name == "ERROR PREVENT SAVE WILL BE GENERATED AFTERWARD")
				{
					context.Notify(new ErrorNotification(ErrorType.DataErrorPreventSave));
				}
			}
		}

		class XmlImportTaskWithErrorPreventSave : XmlImportTask
		{
			public XmlImportTaskWithErrorPreventSave(INotifications notifications)
				: base(SystemDataRegistry.Instance.OrganisationDataImportDirectory, notifications, NotificationDataRegistry.Instance.OrganisationImportNotificationGroup, BillingInterfaceName.Test)
			{
			}

			protected override DataImporter NewImporter()
			{
				return new XmlDataImporter(new OrganisationDataAdapterWithErrorPreventSave());
			}
		}

		#endregion

		#region Import Files With Invalid Xml Schema

		[NUnit.Framework.TestDate(2007, 10, 10, 10, 10, 10, 100)]
		public void TestInvalidXmlSchemaFile()
		{
			ZString testDirectory = Env.TempPath;
			ZString fileName = GetFileName();
			ZString newFilePath = Path.Combine(BadMessageFileDirectoryName, XmlMultiTypeImportTask.ConstructBadMessageFileNameForTest(fileName));
			ZString expectedFileMoveNotificationMessage = "Original file has been moved from " + Path.Combine(testDirectory, fileName) + " to " + newFilePath;

			CreateTestFileWithInvalidXmlSchema(testDirectory, fileName);
			SystemDataRegistry.Instance.ConsolsDataImportDirectory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, testDirectory);
			NotificationDataRegistry.Instance.ConsolShipmentImportNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, PostMasterGroup.PK.ToGuid());

			XmlImportTask task = new XmlImportTaskWithError(SystemDataRegistry.Instance.ConsolsDataImportDirectory, Notify, NotificationDataRegistry.Instance.ConsolShipmentImportNotificationGroup);
			task.Run();
			AssertEquals(true, Notify.HasErrors);
			AssertEquals("Should contain xml schema validation error", true, Notify.ContainsNotificationType(ErrorType.XmlSchemaValidation));
			AssertEquals("Import file should be deleted", false, File.Exists(Path.Combine(testDirectory, fileName)));
			AssertEquals("Import file should be renamed and moved to bad messages file directory", true, File.Exists(newFilePath));
			AssertEquals("Should contain info notification: " + expectedFileMoveNotificationMessage, true, Notify.AsString.IndexOf(expectedFileMoveNotificationMessage) > -1);
		}

		#endregion

		#region Import files with out of range smalldatetime

		[NUnit.Framework.TestDate(2007, 10, 10, 10, 10, 10, 100)]
		public void TestImportWithOutOfRangeSmallDateTime()
		{
			ZString testDirectory = Env.TempPath;
			ZString fileName = GetFileName();
			ZString newFilePath = Path.Combine(BadMessageFileDirectoryName, XmlMultiTypeImportTask.ConstructBadMessageFileNameForTest(fileName));
			ZString expectedFileMoveNotificationMessage = "Original file has been moved from " + Path.Combine(testDirectory, fileName) + " to " + newFilePath;

			CreateTestFileFromEmbeddedResource("Enterprise.ServiceManager.Tasks.XMLAutomation.Test.Testing.ShipmentWithOutOfRangeSmallDateTime.xml", Path.Combine(testDirectory, fileName));

			SystemDataRegistry.Instance.ConsolsDataImportDirectory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, testDirectory);
			NotificationDataRegistry.Instance.ConsolShipmentImportNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, PostMasterGroup.PK.ToGuid());

			XmlImportTask task = new ShipmentXmlImportTask(SystemDataRegistry.Instance.ConsolsDataImportDirectory, Notify, NotificationDataRegistry.Instance.ConsolShipmentImportNotificationGroup);
			task.Run();
			AssertEquals(true, Notify.HasErrors);
			AssertEquals("Should contain DataOutOfRangeError", true, Notify.ContainsNotificationType(ErrorType.DataOutOfRangeError));
			AssertEquals("Import file should be deleted", false, File.Exists(Path.Combine(testDirectory, fileName)));
			AssertEquals("Import file should be renamed and moved to bad messages file directory", true, File.Exists(newFilePath));
			AssertEquals("Should contain info notification: " + expectedFileMoveNotificationMessage, true, Notify.AsString.IndexOf(expectedFileMoveNotificationMessage) > -1);
		}

		#endregion

		#region Replace Word characters

		public void TestImportWithMSWordSmartQuotes_ShouldReplaceCharacters()
		{
			var testDirectory = Env.TempPath;
			SystemDataRegistry.Instance.ConsolsDataImportDirectory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, testDirectory);

			try
			{
				using (var writer = new StreamWriter(Path.Combine(testDirectory, "TempFile.xml")))
				{
					writer.Write("“‘Bow ties’ are cool”");
				}
				var task = new XmlImportTaskForTesting();
				var importer = new DataImporterForCharacterReplacementTesting();
				task.DataImporterOverride = importer;
				task.Run();

				AssertEquals("\"'Bow ties' are cool\"", importer.FileContents);
			}
			finally
			{
				DeleteIfExists(testDirectory + @"\TempFile.xml");
			}
		}

		#endregion

		#region Concurrent import of duplicate data in different files

		public void TestConcurrentImportOfDuplicateDataInDifferentFiles()
		{
			AssertConcurrentImportOfDuplicateDataInDifferentFiles(false);
		}

		public void TestConcurrentImportOfDuplicateDataInDifferentFiles_WhenFailsOnMultipleAttempts()
		{
			AssertConcurrentImportOfDuplicateDataInDifferentFiles(true);
		}

		public void AssertConcurrentImportOfDuplicateDataInDifferentFiles(bool forceFail)
		{
			ZString testDirectory = Env.TempPath;
			File.WriteAllText(Path.Combine(testDirectory, "file1.xml"), "aaa");
			File.WriteAllText(Path.Combine(testDirectory, "file2.xml"), "bbb");

			SystemDataRegistry.Instance.ConsolsDataImportDirectory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, testDirectory);

			var notyficationGroupRegistryItem = new GuidRegistryItem("Temp", (NoResString)"Temp", (NoResString)"Temp", (NoResString)"Temp", RegistryStorageFlags.System);
			notyficationGroupRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty);

			var dataStorage = new DataStorageForTest();

			// Preload common data in main thread
			var tempTask = new XmlImportTaskForConcurrentTesting(SystemDataRegistry.Instance.ConsolsDataImportDirectory, Notify, notyficationGroupRegistryItem, dataStorage, false);
			AssertNotNull(tempTask);
			AssertNotNull(GlbCompany.CurrentCompany.OrgProxy);

			ParameterizedThreadStart action = objectData =>
			{
				using (Db.DisposableActionForDbConnection())
				{
					// Connection on secondary thread is different and not in transaction
					Db.Connection.BeginTransaction();
					try
					{
						var importTask = new XmlImportTaskForConcurrentTesting(SystemDataRegistry.Instance.ConsolsDataImportDirectory, (INotifications)objectData, notyficationGroupRegistryItem, dataStorage, forceFail);
						importTask.Run();
					}
					finally
					{
						Db.Connection.RollbackTransaction();
					}
				}
			};
			var thread1 = new Thread(action);
			var thread2 = new Thread(action);

			var notifications1 = new NotificationBuffer();
			var notifications2 = new NotificationBuffer();

			thread1.Start(notifications1);
			thread2.Start(notifications2);

			thread1.Join();
			thread2.Join();

			var keys = dataStorage.GetAllKeys();
			if (forceFail)
			{
				AssertEquals("Only one file should be imported and saved.", 1, keys.Length);
				AssertEquals(1, keys[0]);
			}
			else
			{
				AssertEquals("Bothe files should be imported and saved.", 2, keys.Length);
				AssertEquals(1, keys[0]);
				AssertEquals(2, keys[1]);
			}

			var notifications = "First buffer:\r\n" + notifications1.AsString + "\r\nSecond buffer:\r\n" + notifications2.AsString;

			Assert(notifications, notifications.Contains("Attempting to import same file again."));
			AssertEquals(notifications, forceFail, notifications.Contains("Second import attempt failed."));
		}

		class XmlImportTaskForConcurrentTesting : XmlImportTask
		{
			public XmlImportTaskForConcurrentTesting(StringRegistryItem registryPath, INotifications notify, GuidRegistryItem notificationGroup, DataStorageForTest dataStorage, bool forceFail)
				: base(registryPath, notify, notificationGroup, BillingInterfaceName.Test)
			{
				this.dataStorage = dataStorage;
				this.forceFail = forceFail;
			}

			readonly DataStorageForTest dataStorage;
			readonly bool forceFail;

			protected override DataImporter NewImporter()
			{
				return new DataImporterForConcurrentTesting(dataStorage, forceFail);
			}

			class DataImporterForConcurrentTesting : DataImporter
			{
				public DataImporterForConcurrentTesting(DataStorageForTest dataStorage, bool forceFail) : base()
				{
					this.dataStorage = dataStorage;
					this.forceFail = forceFail;
				}

				readonly DataStorageForTest dataStorage;
				readonly bool forceFail;

				protected override bool ImportDataToFactoryCore(TextReader dataReader, string attachmentFileName, INotifications notifications, out ITransactionParticipant[] additionalTransactionActions)
				{
					var newKey = forceFail ? 1 : dataStorage.GetLastKey() + 1;

					additionalTransactionActions = new ITransactionParticipant[] { new TransactionParticipantForTest(() => { dataStorage.AddKey(newKey); return ChangedTableNames.Empty; }) };

					Thread.Sleep(100);

					return true;
				}
			}
		}

		class DataStorageForTest
		{
			public int GetLastKey()
			{
				lock (mutex)
				{
					return keys.Count > 0 ? keys.Keys.Max() : 0;
				}
			}

			public void AddKey(int key)
			{
				lock (mutex)
				{
					if (keys.ContainsKey(key))
					{
						throw new ArgumentException($"Key {key} is already added");
					}
					keys.Add(key, null);
				}
			}

			public int[] GetAllKeys()
			{
				return keys.Keys.OrderBy(key => key).ToArray();
			}

			readonly Dictionary<int, object> keys = new Dictionary<int, object>();
			readonly object mutex = new object();
		}

		class TransactionParticipantForTest : ITransactionParticipant
		{
			public TransactionParticipantForTest(Func<ChangedTableNames> action)
			{
				this.action = action;
			}

			readonly Func<ChangedTableNames> action;

			public IChangedTableNames SaveInTransaction()
			{
				return action?.Invoke();
			}

			public ITransactionManager BeginTransactionWithManager()
			{
				return new StubTransactionManager();
			}

			public void OnAllTransactionsBeginning() { }
			public void OnAllTransactionsCommitted(IChangedTableNames changedTableNames) { }
			public void OnAllTransactionsRolledBack() { }

			public bool IsInTransaction => false;
			public bool AllowTransactionWithOtherParticipant => false;

			public ITransactionParticipant[] ChildParticipants => Array.Empty<ITransactionParticipant>();

			public IEnumerable<ISqlApplicationLock> TransactionLocks => Enumerable.Empty<ISqlApplicationLock>();
		}

		#endregion

		#region Overrides

		protected override ImportTask GetImportTask()
		{
			return new XmlImportTaskForTesting(SystemDataRegistry.Instance.LocalCartageDataImportDirectory, Notify, NotificationDataRegistry.Instance.LocalCartageNotificationGroup);
		}

		protected override ZString GetFileName()
		{
			return "Sample.xml";
		}

		protected override void SetRegistryItem(ZString directoryName)
		{
			SystemDataRegistry.Instance.LocalCartageDataImportDirectory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, directoryName);
		}

		#endregion

		#region XmlImportTaskForTesting

		protected class XmlImportTaskForTesting : XmlImportTask
		{
			public XmlImportTaskForTesting()
				: base(SystemDataRegistry.Instance.ConsolsDataImportDirectory, new NotificationBuffer(new NotificationBuffer()), NotificationDataRegistry.Instance.ConsolShipmentImportNotificationGroup, BillingInterfaceName.Test)
			{
			}

			public XmlImportTaskForTesting(StringRegistryItem registryPath, INotifications notify, GuidRegistryItem notificationGroup)
				: base(registryPath, notify, notificationGroup, BillingInterfaceName.Test)
			{
			}

			internal DataImporter DataImporterOverride { get; set; }

			protected override DataImporter NewImporter()
			{
				return DataImporterOverride ?? new DataImporterForTesting();
			}

			public override ZString TaskDescription
			{
				get { return "Test Xml Import"; }
			}
		}

		protected class XmlImportTaskWithError : XmlImportTask
		{
			public XmlImportTaskWithError()
				: base(SystemDataRegistry.Instance.ConsolsDataImportDirectory, new NotificationBuffer(new NotificationBuffer()), NotificationDataRegistry.Instance.ConsolShipmentImportNotificationGroup, BillingInterfaceName.Test)
			{
			}

			public XmlImportTaskWithError(StringRegistryItem registryPath, INotifications notify, GuidRegistryItem notificationGroup)
				: base(registryPath, notify, notificationGroup, BillingInterfaceName.Test)
			{
			}

			protected override DataImporter NewImporter()
			{
				return new TestImporterWithError();
			}

			public override ZString TaskDescription
			{
				get { return "Test Xml Import"; }
			}
		}

		#region DataImporterForTesting

		protected class DataImporterForTesting : DataImporter
		{
			protected override bool ImportDataToFactoryCore(TextReader dataReader, string attachmentFileName, INotifications notifications, out ITransactionParticipant[] additionalTransactionActions)
			{
				additionalTransactionActions = Array.Empty<ITransactionParticipant>();
				return true;
			}
		}

		class TestImporterWithError : DataImporter
		{
			protected override bool ImportDataToFactoryCore(TextReader dataReader, string attachmentFileName, INotifications notifications, out ITransactionParticipant[] additionalTransactionActions)
			{
				additionalTransactionActions = Array.Empty<ITransactionParticipant>();
				notifications.Notify(new ErrorNotification(ErrorType.ImportingDataError, "Importing Error"));
				notifications.Notify(new ErrorNotification(ErrorType.XmlSchemaValidation, "Importing Error"));
				return false;
			}
		}

		class DataImporterForCharacterReplacementTesting : DataImporter
		{
			protected override bool ImportDataToFactoryCore(TextReader dataReader, string attachmentFileName, INotifications notifications, out ITransactionParticipant[] additionalTransactionActions)
			{
				FileContents = dataReader.ReadToEnd();
				additionalTransactionActions = Array.Empty<ITransactionParticipant>();
				return true;
			}

			internal string FileContents { get; private set; }
		}

		#endregion

		#endregion

		#region implementation
		protected override void SetUp()
		{
			base.SetUp();
			BadMessageFileDirectoryName = Path.Combine(Env.TempPath, Temp.GetNewTempSubdirectory());
			Assert("Bad Message Files Directory should exist", Directory.Exists(BadMessageFileDirectoryName));
			SetBadMessageFileDirectoryRegistryItem(BadMessageFileDirectoryName);

			PostMasterGroup = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			PostMasterGroup.Staff[0].GS_EmailAddress = "a@b.c";
			Factory.Save();
		}

		protected override void TearDown()
		{
			base.TearDown();
			DeleteDirectory(BadMessageFileDirectoryName);
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		protected void DeleteDirectory(string directoryName)
		{
			if (Directory.Exists(directoryName))
			{
				DirectoryInfo directoryPath = new DirectoryInfo(directoryName);
				List<FileInfo> files = new List<FileInfo>();
				files.AddRange(directoryPath.GetFiles());
				foreach (FileInfo file in files)
				{
					DeleteIfExists(file.FullName);
				}
				Directory.Delete(directoryName);
			}
		}

		protected void CreateTestFileWithInvalidXmlSchema(ZString testDirectory, ZString fileName)
		{
			using (StreamWriter writer = new StreamWriter(Path.Combine(testDirectory, fileName)))
			{
				writer.Write(GenerateInvalidXmlSchemaXml());
			}
		}

		protected ZString GenerateInvalidXmlSchemaXml()
		{
			return "<Interchange></Info>";
		}

		protected void AssertInvalidXmlSchemaFileContent(ZString filePath, ZString expectedData)
		{
			using (StreamReader reader = new StreamReader(filePath))
			{
				ZString fileData = reader.ReadToEnd();
				AssertEquals("Invalid data in file " + filePath, expectedData, fileData);
			}
		}

		protected void SetBadMessageFileDirectoryRegistryItem(ZString directoryName)
		{
			SystemDataRegistry.Instance.UnprocessedMessagesDataDirectory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, directoryName);
		}

		protected void CreateTestFileFromEmbeddedResource(string embeddedResourcePath, string fileToCreatePath)
		{
			using (var sourceStream = resourceRetriever.Value.GetStream(embeddedResourcePath))
			using (var file = File.Create(fileToCreatePath))
			{
				sourceStream.CopyTo(file);
			}
		}

		protected ZString BadMessageFileDirectoryName;

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());

		GlbGroup PostMasterGroup;
		#endregion
	}
}
