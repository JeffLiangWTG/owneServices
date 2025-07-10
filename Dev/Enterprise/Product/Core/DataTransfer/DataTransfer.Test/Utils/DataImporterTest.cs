using System;
using System.IO;
using System.Text;
using System.Threading;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Billing.Integration;
using Enterprise.MailManager;
using Enterprise.MailManager.Business;
using Enterprise.MailManager.MailFilters;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Business.Testing
{
	sealed class DataImporterTest : TestCaseWithFactory
	{
		public void TestFactoryConstructor()
		{
			DummyDataImporter importer = new DummyDataImporter(Factory);
			AssertEquals(
				"When passing a BusinessObjectFactory into the constructor you should have a factory provider that always returns that factory",
				typeof(SingleBusinessObjectFactoryProvider), importer.FactoryProvider.GetType());
		}

		public void TestImportData_UnhandledExceptionIsSilent()
		{
			using (Stream blankStream = new MemoryStream())
			{
				ErrorReporter.Clear();
				ExceptionThrowingBusinessObjectFactory.ExceptionCount = 0;
				using (TextReader reader = new StreamReader(blankStream))
				{
					var importer = new ExceptionThrowingDataImporter();
					importer.ThrowExceptionInImportData = false;
					Assert("OnImportDataEndWasCalled", !importer.OnImportDataEndWasCalled);
					importer.ImportData(reader, "", new NotificationBuffer(), SourceInfo.EmptySourceInfo);
					Assert("OnImportDataEndWasCalled should be set", importer.OnImportDataEndWasCalled);
				}
				AssertEquals("Should have raised the correct silent exception", TestUnhandledExceptionMessage, ErrorReporter.LastExceptionReported.Message);
				ErrorReporter.Clear();
			}
		}

		public void TestImportData_UnhandledCriticalValidationException()
		{
			NotificationBuffer buffer = new NotificationBuffer();

			using (Stream blankStream = new MemoryStream())
			{
				ErrorReporter.Clear();
				using (TextReader reader = new StreamReader(blankStream))
				{
					DummyDataImporter importer = new DummyDataImporter(new CriticalValidationExceptionThrowingBusinessObjectFactory());
					importer.ImportData(reader, "", buffer, SourceInfo.EmptySourceInfo);
				}
			}
			AssertContains("Error: Critical Error during saving", buffer.AsString);
		}

		public void TestImportData_NoIssueReportedWhenCannotSave()
		{
			NotificationBuffer buffer = new NotificationBuffer();

			using (Stream blankStream = new MemoryStream())
			{
				ErrorReporter.Clear();
				using (TextReader reader = new StreamReader(blankStream))
				{
					DummyDataImporter importer = new DummyDataImporter(new CannotSaveExceptionThrowingBusinessObjectFactory());
					importer.ImportData(reader, "", buffer, SourceInfo.EmptySourceInfo);
				}
			}
			AssertEquals("No issue should be reported", 0, ExceptionReporterTestListener.Instance.Count);
			AssertContains("Error: Cannot Save: It is not possible to save the data", buffer.AsString);
		}

		public void TestImportDataWithOnlySaveDataWhenNoRecordsHaveErrorsFlag()
		{
			NotificationBuffer notify = new NotificationBuffer();
			TestDataImporterForNotificationTesting importer = new TestDataImporterForNotificationTesting();
			importer.ImportData(new StreamReader(new MemoryStream()), "", notify, SourceInfo.EmptySourceInfo);
			Assert("Factory should be saved because OnlySaveDataWhenNoRecordsHaveErrors is false and we don't care about errors", notify.AsString.Contains("Saving the data to the database..."));

			notify = new NotificationBuffer();
			importer.OnlySaveDataWhenNoRecordsHaveErrors = true;
			importer.ImportData(new StreamReader(new MemoryStream()), "", notify, SourceInfo.EmptySourceInfo);
			Assert("Factory shouldn't be saved because OnlySaveDataWhenNoRecordsHaveErrors is true and there are some errors", !notify.AsString.Contains("Saving the data to the database..."));

			notify = new NotificationBuffer();
			DummyDataImporter importerWithoutErrors = new DummyDataImporter();
			importerWithoutErrors.OnlySaveDataWhenNoRecordsHaveErrors = true;
			importerWithoutErrors.ImportData(new StreamReader(new MemoryStream()), "", notify, SourceInfo.EmptySourceInfo);
			Assert("Factory should be saved because OnlySaveDataWhenNoRecordsHaveErrors is true but there are no errors", notify.AsString.Contains("Saving the data to the database..."));
		}

		public void TestImportDataShouldUseEachTimeNewFactory()
		{
			NotificationBuffer notify = new NotificationBuffer();
			TestDataImporterForNotificationTesting importer = new TestDataImporterForNotificationTesting();
			importer.OnlySaveDataWhenNoRecordsHaveErrors = true;
			importer.ImportData(new StreamReader(new MemoryStream()), "", notify, SourceInfo.EmptySourceInfo);
			BusinessObjectFactory firstFactory = importer.FactoryProvider.Current;

			importer.ImportData(new StreamReader(new MemoryStream()), "", notify, SourceInfo.EmptySourceInfo);
			AssertNotEquals("After ImportData the Factory should be new", firstFactory, importer.FactoryProvider.Current);
		}

		public void TestImportFromEmails()
		{
			DummyDataImporter importer = new DummyDataImporter();
			NotificationBuffer notify = new NotificationBuffer();

			MailItem testItem = QueueNewReceivedMailItemWithAttachment("Attachment1 Data");
			Factory.Save();

			importer.ImportFromEmails(ZDateTime.Empty, notify, SourceInfo.EmptySourceInfo);
			testItem.Reload();
			AssertEquals("1 attachment should be imported", 1, importer.LastDataItemsImported.Count);
			AssertEquals("The correct attachment should be imported", "Attachment1 Data", importer.LastDataItemsImported[0]);
			AssertEquals("The MailItem should be 'processed' after it has been imported", MailStatus.Processed, testItem.MI_Status);

			MailItem testItem2 = QueueNewReceivedMailItemWithAttachment("Attachment2 Data");
			Factory.Save();

			importer.LastDataItemsImported.Clear();
			importer.ImportFromEmails(ZDateTime.Empty, notify, SourceInfo.EmptySourceInfo);
			testItem2.Reload();
			AssertEquals("The second attachment should be imported", 1, importer.LastDataItemsImported.Count);
			AssertEquals("The correct attachment should be imported", "Attachment2 Data", importer.LastDataItemsImported[0]);
			AssertEquals("The MailItem should be 'processed' after it has been imported", MailStatus.Processed, testItem2.MI_Status);
		}

		public void TestImportFromEmails_UnhandledExceptionIsSilent()
		{
			MailItem testItem = QueueNewReceivedMailItemWithAttachment("Attachment1 Data");
			Factory.Save();

			NotificationBuffer notify = new NotificationBuffer();
			ErrorReporter.Clear();
			var importer = new ExceptionThrowingDataImporter();
			Assert("OnImportDataEndWasCalled", !importer.OnImportDataEndWasCalled);
			importer.ImportFromEmails(ZDateTime.Invalid, notify, SourceInfo.EmptySourceInfo);
			Assert("OnImportDataEndWasCalled should be set", importer.OnImportDataEndWasCalled);
			AssertEquals("Should have raised the correct silent exception", TestUnhandledExceptionMessage, ErrorReporter.LastExceptionReported.Message);
			ErrorReporter.Clear();
		}

		public void TestImportFromEmails_SortsByReceivedTime()
		{
			DummyDataImporter importer = new DummyDataImporter();

			MailItem testItem2 = QueueNewReceivedMailItemWithAttachment("Attachment2 Data");
			testItem2.MI_ReceivedDateTime = new ZDateTime(2005, 2, 2);
			Factory.Save();
			Thread.Sleep(500);
			MailItem testItem1 = QueueNewReceivedMailItemWithAttachment("Attachment1 Data");
			testItem1.MI_ReceivedDateTime = new ZDateTime(2005, 1, 1);
			Factory.Save();
			Thread.Sleep(500);
			MailItem testItem3 = QueueNewReceivedMailItemWithAttachment("Attachment3 Data");
			testItem3.MI_ReceivedDateTime = new ZDateTime(2005, 3, 3);
			Factory.Save();

			NotificationBuffer notify = new NotificationBuffer();
			importer.ImportFromEmails(new ZDateTime(2005, 2, 1), notify, SourceInfo.EmptySourceInfo);
			AssertEquals("3 attachment should be imported", 2, importer.LastDataItemsImported.Count);
			AssertEquals("The correct attachment in received time order should be imported", "Attachment2 Data", importer.LastDataItemsImported[0]);
			AssertEquals("The correct attachment in received time order should be imported", "Attachment3 Data", importer.LastDataItemsImported[1]);
		}

		public void TestShouldProcessMailItemCore()
		{
			DummyDataImporter importer = new DummyDataImporter();
			NotificationBuffer notify = new NotificationBuffer();

			MailItem testItem = QueueNewReceivedMailItemWithAttachment("Attachment1 Data");
			Factory.Save();

			importer.ShouldProcessNextMailItem = false;
			importer.ImportFromEmails(ZDateTime.Empty, notify, SourceInfo.EmptySourceInfo);
			testItem.Reload();
			AssertEquals("No attachments should be imported because ShouldProcessMailItemCore returned false", 0, importer.LastDataItemsImported.Count);
		}

		public void TestMarkMailItemStatusAsFailedIfAttachmentNotSuitable()
		{
			DummyDataImporter importer = new DummyDataImporter();
			NotificationBuffer notify = new NotificationBuffer();

			MailItem testItem = QueueNewReceivedMailItemWithAttachment("Attachment Data");
			Factory.Save();

			importer.ImportDataWillBeSuccessful = false;
			importer.ImportFromEmails(ZDateTime.Empty, notify, SourceInfo.EmptySourceInfo);
			testItem.Reload();
			AssertEquals("The correct attachment should be attempted to be imported", "Attachment Data", importer.LastDataItemsImported[0]);
			AssertEquals("A MailItem with an invalid attachment should still be marked as 'Failed'", MailStatus.Failed, testItem.MI_Status);
		}

		public void TestImportDataProducesNotificationsWithCorrectAppearance()
		{
			NotificationBuffer notify = new NotificationBuffer();
			new TestDataImporterForNotificationTesting().ImportData(new StreamReader(new MemoryStream()), "", notify, SourceInfo.EmptySourceInfo);

			bool allowNumericCharactersInCodeGeneration = Registry.Business.OrganisationsDataRegistry.Instance.AllowNumericCharactersInCodeGeneration.Value;
			bool useUnmatchedOrganisationForMatching = Registry.Business.OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.Value.IsEnabled;
			string threshold = Registry.Business.OrganisationsDataRegistry.Instance.OrgMatchThreshold.Value;
			string orgProxy = GlbCompany.CurrentCompany.OrgProxy.OH_Code;
			string companyCode = GlbCompany.CurrentCompany.GC_Code;
			string branchCode = GlbBranch.CurrentBranch.GB_Code;

			string expectedNotificationsString =
					"Registry value for Organizations -> Allow Numeric Characters In Code Generation is currently " + (allowNumericCharactersInCodeGeneration ? "Enabled" : "Disabled") + ".\r\n" +
					"Registry value for Organizations -> Use Default Organization for Matching is currently " + (useUnmatchedOrganisationForMatching ? "Enabled" : "Disabled") + ".\r\n" +
					"Current Organization Match Threshold - " + threshold + ".\r\n" +
					"Current Organization Proxy - " + orgProxy + ".\r\n" +
					"Current Company - " + companyCode + ".\r\n" +
					"Current Branch - " + branchCode + ".\r\n" +
					"\r\n" +
					"Error: Required field empty (Some Field)\r\n" +
					"\r\n" +
					"Saving the data to the database...\r\n";

			AssertMultilineASCIIEquals("Notifications should be of correct appearance", expectedNotificationsString, notify.AsString);
		}

		public void TestImportDataProducesNotificationsWithCorrectAppearance_NoOrgProxy()
		{
			GlbCompany.CurrentCompany.GC_OH_OrgProxy = ZGuid.Empty;
			ImportDataAndAssert();

			using (EnvProxy.Instance.SetTemporaryUserContext(null))
			{
				ImportDataAndAssert();
			}

			void ImportDataAndAssert()
			{
				var notificationBuffer = new NotificationBuffer();
				new TestDataImporterForNotificationTesting().ImportData(new StreamReader(new MemoryStream()), string.Empty, notificationBuffer, SourceInfo.EmptySourceInfo);
				AssertContains("Notifications should be of correct appearance", "Current Organization Proxy - N/A", notificationBuffer.AsString);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestEncoding()
		{
			string inputFileAsString;

			using (TextReader reader = new StreamReader(UnusualCharactersTestFile, Encoding.GetEncoding("windows-1252")))
			{
				inputFileAsString = reader.ReadToEnd();
			}

			string inputFileAsDefaultEncoding;

			using (TextReader reader = new StreamReader(UnusualCharactersTestFile))
			{
				inputFileAsDefaultEncoding = reader.ReadToEnd();
			}

			Assert("The input file should be different if encoded using the default encoding type", inputFileAsString != inputFileAsDefaultEncoding);

			DummyImporterWithDifferentEncoding dummyImporter = new DummyImporterWithDifferentEncoding();
			dummyImporter.ImportData(UnusualCharactersTestFile, new NotificationBuffer(), SourceInfo.EmptySourceInfo);

			AssertEquals("Output file should be the match the input file", inputFileAsString, dummyImporter.OutputFileAsString);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestNullEncoding()
		{
			DummyImporterWithNullEncoding dummyImporter = new DummyImporterWithNullEncoding();

			string inputFileAsUTF8;

			using (TextReader reader = new StreamReader(UnusualCharactersTestFile, Encoding.UTF8))
			{
				inputFileAsUTF8 = reader.ReadToEnd();
			}

			using (TextReader reader = new StreamReader(UnusualCharactersTestFile))
			{
				dummyImporter.ImportData(reader, UnusualCharactersTestFile, new NotificationBuffer(), SourceInfo.EmptySourceInfo);
			}

			AssertEquals("Output file should be in UTF8 as default", inputFileAsUTF8, dummyImporter.OutputFileAsString);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAdditionalTransactionActions_InTransaction()
		{
			DummyImporterWithAdditionalTransactionActions dummyImporter = new DummyImporterWithAdditionalTransactionActions();

			string inputFileAsUTF8;
			NotificationBuffer notifications = new NotificationBuffer();

			using (TextReader reader = new StreamReader(UnusualCharactersTestFile, Encoding.UTF8))
			{
				inputFileAsUTF8 = reader.ReadToEnd();
			}

			using (TextReader reader = new StreamReader(UnusualCharactersTestFile))
			{
				dummyImporter.ImportData(reader, UnusualCharactersTestFile, notifications, SourceInfo.EmptySourceInfo);
			}

			dummyImporter.EndTransaction();
			Assert("No errors occured", !notifications.HasErrors);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportData_WithWrongEncoding()
		{
			using (StreamReader reader = new StreamReader(UnusualCharactersTestFile, Encoding.BigEndianUnicode))
			{
				DummyImporterWithDifferentEncoding importer = new DummyImporterWithDifferentEncoding();
				importer.ImportData(reader, "", null, SourceInfo.EmptySourceInfo);

				AssertEquals("Developer error should have been reported", "Encoding on Stream different to Encoding override.", ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportDataToFactory_WithWrongEncoding()
		{
			using (StreamReader reader = new StreamReader(UnusualCharactersTestFile, Encoding.BigEndianUnicode))
			{
				DummyImporterWithDifferentEncoding importer = new DummyImporterWithDifferentEncoding();
				ITransactionParticipant[] transactionActions;
				importer.ImportDataToFactory(reader, "", null, SourceInfo.EmptySourceInfo, out transactionActions);

				AssertEquals("Developer error should have been reported", "Encoding on Stream different to Encoding override.", ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
			}
		}

		public void TestProcessWithSaveExceptionHandling()
		{
			var importer = new DummyDataImporter();

			const string expectedMessage = "Test exception message";

			var notifications = new NotificationBuffer();
			var unhandledException = new NullReferenceException(expectedMessage);
			importer.ProcessWithSaveExceptionHandling(() => throw unhandledException, notifications);
			AssertNotContains("Exception is handled but message is not added to notifications.", expectedMessage, notifications.AsString);
			AssertEquals("Exception count", 1, ExceptionReporterTestListener.Instance.Count);
			AssertEquals("Reported InnerException", unhandledException, ExceptionReporterTestListener.Instance[0].InnerException);
			ErrorReporter.Clear();

			var dummy = Factory.New<DummyBusinessObject>();
			var handledExceptions = new Exception[]
			{
				new IOException(expectedMessage),
				new ZSaveConcurrencyException(new ZDataConcurrencyException(new Exception(expectedMessage), ((INeedRow)dummy).Row, ((IDbConnected)Factory).Connection), Factory),
				new ZSaveException(new ZDataException(new Exception(expectedMessage), ((INeedRow)dummy).Row, ((IDbConnected)Factory).Connection), Factory),
				new EmailSendFailedException(expectedMessage),
				new ArgumentOutOfRangeException(expectedMessage),
				new ArgumentException(expectedMessage),
				new TestUserVisibleException(expectedMessage),
				new TestCriticalCheckException(null, CriticalValidationErrorType.DummyErrorKeyForTest, expectedMessage),
				new ZCannotSaveException(expectedMessage, ""),
				new InvalidOperationException(expectedMessage)
			};

			foreach (var exception in handledExceptions)
			{
				notifications = new NotificationBuffer();
				importer.ProcessWithSaveExceptionHandling(() => throw exception, notifications);
				AssertContains("Exception is handled and message is added to notifications.", expectedMessage, notifications.AsString);
			}
		}

		MailItem QueueNewReceivedMailItemWithAttachment(string attachmentData)
		{
			MailItem mailItem = Factory.New<MailItem>();
			mailItem.MI_SendDateTime = ZDateTime.Now;
			mailItem.MI_ReceivedDateTime = ZDateTime.Now;
			mailItem.MI_Status = MailStatus.Queued;
			mailItem.MI_Direction = MailDirection.Receive;
			mailItem.MI_Application = "ALL"; //a debug only filter so no logic to test
			MailAttachment attachment = mailItem.MailAttachments.AddNew();
			attachment.MA_Data = ZBlob.FromUTF8(attachmentData);
			return mailItem;
		}

		readonly string UnusualCharactersTestFile = BaseSourcePath + @"Enterprise\Product\Core\DataTransfer\DataTransfer.Test\FlatFile\TestFiles\UnusualCharactersTestFile.txt";

		const string TestUnhandledExceptionMessage = "TestUnhandledExceptionMessage";

		sealed class TestDataImporterForNotificationTesting : DataImporter
		{
			internal new BusinessObjectFactoryProvider FactoryProvider => base.FactoryProvider;

			protected override bool ImportDataToFactoryCore(TextReader dataReader, string attachmentFileName, INotifications notifications, out ITransactionParticipant[] additionalTransactionActions)
			{
				notifications.Notify(new ErrorNotification(ErrorType.RequiredFieldEmpty, "Some Field"));

				additionalTransactionActions = null;
				return true;
			}

			protected override IMailFilter GetMailItemFilter() => QueryMailFilter.AllQueuedItems_ForTesting;
		}

		sealed class DummyImporterWithDifferentEncoding : DataImporter
		{
			public string OutputFileAsString;

			protected override Encoding Encoding
			{
				get { return Encoding.GetEncoding("windows-1252"); }
			}

			protected override bool ImportDataToFactoryCore(TextReader dataReader, string attachmentFileName, INotifications notifications, out ITransactionParticipant[] additionalTransactionActions)
			{
				OutputFileAsString = dataReader.ReadToEnd();
				additionalTransactionActions = null;
				return true;
			}

			protected override IMailFilter GetMailItemFilter() => QueryMailFilter.AllQueuedItems_ForTesting;
		}

		sealed class DummyImporterWithAdditionalTransactionActions : DataImporter
		{
			protected override bool ImportDataToFactoryCore(TextReader dataReader, string attachmentFileName, INotifications notifications, out ITransactionParticipant[] additionalTransactionActions)
			{
				BusinessObjectFactory newParticipant = new BusinessObjectFactory(Db.Connection);
				Db.Connection.BeginTransaction();
				additionalTransactionActions = new ITransactionParticipant[] { newParticipant };
				return true;
			}

			public void EndTransaction()
			{
				Db.Connection.RollbackTransaction();
			}

			protected override IMailFilter GetMailItemFilter() => QueryMailFilter.AllQueuedItems_ForTesting;
		}

		sealed class DummyImporterWithNullEncoding : DataImporter
		{
			public string OutputFileAsString;

			protected override bool ImportDataToFactoryCore(TextReader dataReader, string attachmentFileName, INotifications notifications, out ITransactionParticipant[] additionalTransactionActions)
			{
				OutputFileAsString = dataReader.ReadToEnd();
				additionalTransactionActions = null;
				return true;
			}

			protected override IMailFilter GetMailItemFilter() => QueryMailFilter.AllQueuedItems_ForTesting;
		}

		sealed class ExceptionThrowingBusinessObjectFactory : BusinessObjectFactory, ITransactionParticipant
		{
			public static int ExceptionCount;

			protected override IChangedTableNames SaveInTransactionCore()
			{
				ExceptionCount++;
				throw new Exception(TestUnhandledExceptionMessage);
			}
		}

		sealed class CriticalValidationExceptionThrowingBusinessObjectFactory : BusinessObjectFactory, ITransactionParticipant
		{
			protected override IChangedTableNames SaveInTransactionCore()
			{
				throw new TestCriticalCheckException(null, CriticalValidationErrorType.DummyErrorKeyForTest, "CRITICAL EXCEPTION THROWN!!!!!!");
			}
		}

		[Serializable]
		class TestCriticalCheckException : OnSavingCriticalCheckException
		{
			public TestCriticalCheckException(ISupportCriticalValidation businessEntity, CriticalValidationErrorType errorType, string message)
				: base(businessEntity, errorType, message, string.Empty)
			{
			}

#if NETFRAMEWORK
			protected TestCriticalCheckException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
				: base(info, context)
			{
			}
#endif
		}

		sealed class CannotSaveExceptionThrowingBusinessObjectFactory : BusinessObjectFactory, ITransactionParticipant
		{
			protected override IChangedTableNames SaveInTransactionCore()
			{
				throw new TestCannotSaveException("Error: Cannot Save: It is not possible to save the data", "ValidationOnLocalChargeCodeException");
			}
		}

		[Serializable]
		class TestCannotSaveException : ZCannotSaveException
		{
			public TestCannotSaveException(string message, string heading)
				: base(message, heading)
			{
			}

#if NETFRAMEWORK
			protected TestCannotSaveException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
				: base(info, context)
			{
			}
#endif
		}

		[Serializable]
		[ExceptionVisibility(ExceptionVisibility.User)]
		class TestUserVisibleException : Exception
		{
			public TestUserVisibleException(string message) : base(message)
			{
			}

#if NETFRAMEWORK
			protected TestUserVisibleException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
				: base(info, context)
			{
			}
#endif
		}

		class DummyDataImporter : DataImporter
		{
			public DummyDataImporter()
			{
			}

			public DummyDataImporter(BusinessObjectFactoryProvider factoryProvider)
				: base(factoryProvider)
			{
			}

			public DummyDataImporter(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public readonly StringCollectionX LastDataItemsImported = new StringCollectionX();

			public new BusinessObjectFactoryProvider FactoryProvider
			{
				get { return base.FactoryProvider; }
			}

			public bool ImportDataWillBeSuccessful = true;
			protected override bool ImportDataToFactoryCore(
				TextReader dataReader, string attachmentFileName,
				INotifications notifications, out ITransactionParticipant[] additionalTransactionActions)
			{
				string data = dataReader.ReadToEnd();
				LastDataItemsImported.Add(data);

				additionalTransactionActions = null;
				return ImportDataWillBeSuccessful;
			}

			public bool ShouldProcessNextMailItem = true;
			protected override bool ShouldProcessMailItemCore(MailItem item)
			{
				return ShouldProcessNextMailItem;
			}

			protected override IMailFilter GetMailItemFilter() => QueryMailFilter.AllQueuedItems_ForTesting;
		}

		sealed class ExceptionThrowingDataImporter : DummyDataImporter
		{
			public bool ThrowExceptionInImportData = true;
			public bool OnImportDataEndWasCalled;

			protected override bool ImportDataToFactoryCore(
				TextReader dataReader, string attachmentFileName,
				INotifications notifications, out ITransactionParticipant[] additionalTransactionActions)
			{
				bool result = base.ImportDataToFactoryCore(dataReader, attachmentFileName, notifications, out additionalTransactionActions);
				additionalTransactionActions = new ITransactionParticipant[] { new ExceptionThrowingBusinessObjectFactory() };
				return result;
			}

			protected override void OnImportDataEnd(NotificationBuffer buffer, bool sucessfullyImported)
			{
				base.OnImportDataEnd(buffer, sucessfullyImported);
				OnImportDataEndWasCalled = true;
			}
		}
	}
}
