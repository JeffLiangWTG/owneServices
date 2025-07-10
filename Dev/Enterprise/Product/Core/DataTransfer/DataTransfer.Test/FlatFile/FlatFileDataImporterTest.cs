using System;
using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.Data.Utils;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Billing.Integration;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.Environment;
using Enterprise.MailManager;
using Enterprise.MailManager.MailFilters;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Business.Testing
{
	sealed class FlatFileDataImporterTest : TestCaseWithFactory
	{
		public void TestImportDataToFactory()
		{
			var bESTCOFilter = new ZQuery(OrgHeaderSchema.OH_Code, "BESTCO");
			var iCEINCFilter = new ZQuery(OrgHeaderSchema.OH_Code, "ICEINC");

			AssertNull("precondition: BESTCO org not in the db", Factory.LoadTop1(typeof(OrgHeader), bESTCOFilter));
			AssertNull("precondition: ICEINC org not in the db", Factory.LoadTop1(typeof(OrgHeader), iCEINCFilter));

			var factoryProvider = new SingleBusinessObjectFactoryProvider(Factory);
			using (var tempFile = TempFile.New())
			using (var reader = new StreamReader(OrgTestFileStream))
			{
				var importer = new FlatFileDataImporterForTesting(factoryProvider);
				var returnVal = importer.ImportDataToFactory(reader, tempFile.Filename, new NotificationBuffer(), SourceInfo.EmptySourceInfo, out var notUsed);
				AssertEquals("Return Value should be True", true, returnVal);
				AssertEquals("Importer.IsValidDataBeforeImportHasBeenHit", true, importer.IsValidDataBeforeImportHasBeenHit);
				factoryProvider.Current.Save();
			}

			AssertNotNull("ICEINC org in the db after import", Factory.LoadTop1(typeof(OrgHeader), iCEINCFilter));
			AssertNotNull("BESTCO org in the db after import", Factory.LoadTop1(typeof(OrgHeader), bESTCOFilter));
		}

		public void TestImportDataToFactory_WithInvalidFile()
		{
			var bESTCOFilter = new ZQuery(OrgHeaderSchema.OH_Code, "BESTCO");
			var iCEINCFilter = new ZQuery(OrgHeaderSchema.OH_Code, "ICEINC");

			AssertNull("precondition: BESTCO org not in the db", Factory.LoadTop1(typeof(OrgHeader), bESTCOFilter));
			AssertNull("precondition: ICEINC org not in the db", Factory.LoadTop1(typeof(OrgHeader), iCEINCFilter));

			var resourceStream = resourceRetriever.Value.GetStream("Enterprise.DataTransfer.Test.FlatFile.TestFiles.InvalidOrgTestFile.csv");

			using (var tempFile = TempFile.New())
			using (var reader = new StreamReader(resourceStream))
			{
				var importer = new FlatFileDataImporterForTesting_ReturnFalseOnImport();
				ITransactionParticipant[] notUsed;
				var returnVal = importer.ImportDataToFactory(reader, tempFile.Filename, new NotificationBuffer(), SourceInfo.EmptySourceInfo, out notUsed);
				AssertEquals("Return Value should be False", false, returnVal);
			}

			AssertNull("ICEINC org not in the db after import", Factory.LoadTop1(typeof(OrgHeader), iCEINCFilter));
			AssertNull("BESTCO org not in the db after import", Factory.LoadTop1(typeof(OrgHeader), bESTCOFilter));
		}

		public void TestFactory()
		{
			var testSimple = new FlatFileDataImporterForTesting();
			var factoryProperty = typeof(FlatFileDataImporter).GetProperty("Factory", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			AssertNull("DONT PASS A FACTORY INTO THE CLASS, Use the FactoryProvider on ImportDataToFactoryCore", factoryProperty);
			var factoryField = typeof(FlatFileDataImporter).GetField("Factory", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			AssertNull("DONT PASS A FACTORY INTO THE CLASS, Use the FactoryProvider on ImportDataToFactoryCore", factoryField);
		}

		public void TestFileExtension()
		{
			var importer = new FlatFileDataImporterForTesting();
			AssertEquals("File extension should return whatever the file format says:", importer.FlatFileFormatForTest.FileExtensionForImport, importer.FileExtensionType);
		}

		public void TestSupportsHistoryForDuplicatesPrevention()
		{
			var importer = new FlatFileDataImporterForTesting_DuplicatesPrevention();
			Assert("SupportsHistoryForDuplicatesPrevention", importer.SupportsHistoryForDuplicatesPrevention_ForTestOnly);
		}

		[TestDate(2020, 2, 3, 15, 30, 11)]
		public void TestStmDataImportHistoryRecordIsCreated()
		{
			var factoryProvider = new SingleBusinessObjectFactoryProvider(Factory);
			AssertNull("Pre-condition: No StmDataImportHistory records in Db", Factory.LoadTop1<StmDataImportHistory>(new ZQuery()));

			using (var tempFile = TempFile.New())
			using (var reader = new StreamReader(OrgTestFileStream))
			{
				var importer = new FlatFileDataImporterForTesting_DuplicatesPrevention(factoryProvider);
				try
				{
					var returnVal = importer.ImportDataToFactory(reader, tempFile.Filename, new NotificationBuffer(), SourceInfo.EmptySourceInfo, out var extraFactories);
					AssertEquals("Return Value should be True", true, returnVal);
					AssertNotNull(extraFactories);
					AssertEquals("There is one extra Factory", 1, extraFactories.Length);
					var historyFactory = extraFactories[0] as BusinessObjectFactory;
					AssertNotNull("History Factory", historyFactory);
					AssertNotNull("A new StmDataImportHistory record in extra Factory", historyFactory.LoadTop1<StmDataImportHistory>(new ZQuery() { FetchOnlyFromLocalCache = true }));

					factoryProvider.Current.ChildFactories.Add(historyFactory); // Simulate what ImportData does
					factoryProvider.Current.Save();
					var historyRecordsInDb = Factory.Load<StmDataImportHistory>(new ZQuery());
					var historyRecord = historyRecordsInDb.FirstOrDefault();
					AssertHistoryRecord(historyRecord, "TEST", ZDateTimeOffset.Now, tempFile.Filename, expectedHash);

					AssertNotNull("ImportLockWithHash", importer.ImportLockWithHash_ForTestOnly);
					var expectedKey = GlbCompany.CurrentCompany.GC_Code + "TEST" + Convert.ToBase64String(expectedHash);
					AssertEquals("ImportLockWithHash.Key", expectedKey, importer.ImportLockWithHash_ForTestOnly.Key);
					Assert("ImportLockWithHash.IsHoldingLock", importer.ImportLockWithHash_ForTestOnly.IsHoldingLock());
				}
				finally
				{
					if (importer.ImportLockWithHash_ForTestOnly != null)
					{
						importer.ImportLockWithHash_ForTestOnly.Dispose();
					}
				}
			}
		}

		public void TestSqlApplicationLockPreventsSimultaneousSameFileImport()
		{
			var factoryProvider = new SingleBusinessObjectFactoryProvider(Factory);

			AssertNull("Pre-condition: No StmDataImportHistory records in Db", Factory.LoadTop1<StmDataImportHistory>(new ZQuery()));

			using (var tempFile = TempFile.New())
			using (TextReader reader = new StreamReader(OrgTestFileStream))
			{
				var importer = new FlatFileDataImporterForTesting_DuplicatesPrevention(factoryProvider);
				try
				{
					var returnVal = importer.ImportDataToFactory(reader, tempFile.Filename, new NotificationBuffer(), SourceInfo.EmptySourceInfo, out var extraFactories);
					AssertEquals("Return Value should be True", true, returnVal);
					AssertNotNull(extraFactories);
					AssertEquals("There is one extra Factory", 1, extraFactories.Length);
					var historyFactory = extraFactories[0] as BusinessObjectFactory;
					AssertNotNull("History Factory", historyFactory);
					AssertNotNull("A new StmDataImportHistory record in extra Factory", historyFactory.LoadTop1<StmDataImportHistory>(new ZQuery() { FetchOnlyFromLocalCache = true }));

					AssertNotNull("AppLock is placed", importer.ImportLockWithHash_ForTestOnly);
					Assert(importer.ImportLockWithHash_ForTestOnly.IsHoldingLock());

					using (var tempFile2 = TempFile.New())
					using (var reader2 = new StreamReader(OrgTestFileStream))
					using (var newConnection = Db.NewExtraConnectionToMainDb())
					{
						var factoryProvider2 = new BusinessObjectFactoryProvider(newConnection);
						var buffer2 = new NotificationBuffer();

						var importer2 = new FlatFileDataImporterForTesting_DuplicatesPrevention(factoryProvider2);
						var returnVal2 = importer2.ImportDataToFactory(reader2, tempFile.Filename, buffer2, SourceInfo.EmptySourceInfo, out var extraFactories2);
						AssertEquals("Return Value should be False", false, returnVal2);
						AssertNotNull(extraFactories2);
						AssertEquals("No extra Transaction Participants", 0, extraFactories2.Length);

						AssertNull(importer2.ImportLockWithHash_ForTestOnly);
						Assert(buffer2.HasErrors);
						AssertStartsWith("Expected Error", "An import of the same file is in progress.", buffer2.AsString);
					}
				}
				finally
				{
					if (importer.ImportLockWithHash_ForTestOnly != null)
					{
						importer.ImportLockWithHash_ForTestOnly.Dispose();
					}
				}
			}
		}

		[TestDate(2020, 2, 3, 20, 46, 19)]
		public void TestStmDataImportHistoryPreventsSameFileImport()
		{
			var factoryProvider = new SingleBusinessObjectFactoryProvider(Factory);

			AssertNull("Pre-condition: No StmDataImportHistory records in Db", Factory.LoadTop1<StmDataImportHistory>(new ZQuery()));

			using (var tempFile = TempFile.New())
			using (var reader = new StreamReader(OrgTestFileStream))
			{
				var importer = new FlatFileDataImporterForTesting_DuplicatesPrevention(factoryProvider);
				var returnVal = importer.ImportData(reader, tempFile.Filename, new NotificationBuffer(), SourceInfo.EmptySourceInfo);
				AssertEquals("Return Value should be True", true, returnVal);

				AssertNull("AppLock is released", importer.ImportLockWithHash_ForTestOnly);

				using (var tempFile2 = TempFile.New())
				{
					using (var reader2 = new StreamReader(OrgTestFileStream))
					using (var newConnection = Db.NewExtraConnectionToMainDb())
					{
						var factoryProvider2 = new BusinessObjectFactoryProvider(newConnection);
						ITransactionParticipant[] extraFactories2;
						var buffer2 = new NotificationBuffer();

						var importer2 = new FlatFileDataImporterForTesting_DuplicatesPrevention(factoryProvider2);
						var returnVal2 = importer2.ImportDataToFactory(reader2, tempFile.Filename, buffer2, SourceInfo.EmptySourceInfo, out extraFactories2);
						AssertEquals("Return Value should be False", false, returnVal2);
						AssertNotNull(extraFactories2);
						AssertEquals("No extra Transaction Participants", 0, extraFactories2.Length);

						AssertNull(importer2.ImportLockWithHash_ForTestOnly);
						Assert(buffer2.HasErrors);
						AssertStartsWith("Expected Error", "A flat file with the same Hash was imported by E at 03-Feb-20 20:46:19 +00:00 with file name " + tempFile.Filename, buffer2.AsString);
					}
				}
			}
		}

		[TestDate(2020, 2, 4, 11, 37, 18)]
		public void TestStmDataImportHistoryCleanUpOnSuccess()
		{
			AssertStmDataImportHistoryCleanUp(true);
		}

		[TestDate(2020, 2, 4, 12, 10, 35)]
		public void TestStmDataImportHistoryCleanUpOnFailure()
		{
			AssertStmDataImportHistoryCleanUp(false);
		}

		[TestDate(2020, 2, 3, 20, 46, 19)]
		public void TestStmDataImportHistoryPreventsSameFileImportForDifferent()
		{
			var company1 = Factory.New<GlbCompany>();
			company1.GC_Code = "TST";
			company1.GC_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			company1.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;

			var newBranch = company1.Branches.AddNew();
			newBranch.GB_Code = "TST";
			newBranch.GB_RL_NKHomePort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.StartsWith, company1.GC_RN_NKCountryCode)).Code;
			newBranch.GB_BranchName = "Test Branch";

			Factory.Save();

			var factoryProvider = new SingleBusinessObjectFactoryProvider(Factory);

			AssertNull("Pre-condition: No StmDataImportHistory records in Db", Factory.LoadTop1<StmDataImportHistory>(new ZQuery()));

			using (var tempFile = TempFile.New())
			using (var reader = new StreamReader(OrgTestFileStream))
			{
				var importer = new FlatFileDataImporterForTesting_DuplicatesPrevention_DifferentCompany(factoryProvider);
				var returnVal = importer.ImportData(reader, tempFile.Filename, new NotificationBuffer(), SourceInfo.EmptySourceInfo);
				AssertEquals("Return Value should be True", true, returnVal);

				AssertNull("AppLock is released", importer.ImportLockWithHash_ForTestOnly);

				using (Env.SetTemporaryUserContext(Env.CurrentUserPK, newBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
				using (var tempFile2 = TempFile.New())
				{
					using (var reader2 = new StreamReader(OrgTestFileStream))
					using (var newConnection = Db.NewExtraConnectionToMainDb())
					{
						var factoryProvider2 = new BusinessObjectFactoryProvider(newConnection);
						ITransactionParticipant[] extraFactories2;
						var buffer2 = new NotificationBuffer();

						var importer2 = new FlatFileDataImporterForTesting_DuplicatesPrevention_DifferentCompany(factoryProvider2);
						var returnVal2 = importer2.ImportDataToFactory(reader2, tempFile.Filename, buffer2, SourceInfo.EmptySourceInfo, out extraFactories2);
						AssertEquals("Return Value should be False", false, returnVal2);
						AssertNotNull(extraFactories2);
						AssertEquals("No extra Transaction Participants", 0, extraFactories2.Length);

						AssertNull(importer2.ImportLockWithHash_ForTestOnly);
						Assert(buffer2.HasErrors);
						AssertStartsWith("Expected Error", "A flat file with the same Hash was imported by E in company EDI at 03-Feb-20 20:46:19 +00:00 with file name " + tempFile.Filename, buffer2.AsString);
					}
				}
			}
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());

		Stream OrgTestFileStream => resourceRetriever.Value.GetStream("Enterprise.DataTransfer.Test.FlatFile.TestFiles.OrgTestFile.csv");

		void AssertStmDataImportHistoryCleanUp(bool onSuccess)
		{
			var recordToPurge = Factory.New<StmDataImportHistory>();
			recordToPurge.DIH_GC = Environment.Env.CurrentCompanyPK;
			recordToPurge.DIH_ImportType = "TEST";
			recordToPurge.DIH_DataHash = new ZBlob(onSuccess ? new byte[] { 1, 2, 3 } : expectedHash);
			recordToPurge.DIH_GS_NKImportStaff = "E";
			recordToPurge.DIH_ImportDateTime = ZDateTimeOffset.Today.AddDays(-1).AddSeconds(-1);

			var recordToKeep = Factory.New<StmDataImportHistory>();
			recordToKeep.DIH_GC = Environment.Env.CurrentCompanyPK;
			recordToKeep.DIH_ImportType = "TEST";
			recordToKeep.DIH_DataHash = new ZBlob(new byte[] { 4, 5, 6 });
			recordToKeep.DIH_GS_NKImportStaff = "E";
			recordToKeep.DIH_ImportDateTime = ZDateTimeOffset.Today.AddDays(-1);

			var unrelatedRecord = Factory.New<StmDataImportHistory>();
			unrelatedRecord.DIH_GC = Environment.Env.CurrentCompanyPK;
			unrelatedRecord.DIH_ImportType = "Other";
			unrelatedRecord.DIH_DataHash = new ZBlob(new byte[] { 7, 8, 9 });
			unrelatedRecord.DIH_GS_NKImportStaff = "E";
			unrelatedRecord.DIH_ImportDateTime = ZDateTimeOffset.Today.AddDays(-2);

			Factory.Save();
			var recordToPurgePK = recordToPurge.PK;
			var recordToKeepPK = recordToKeep.PK;
			var unrelatedRecordPK = unrelatedRecord.PK;

			var factoryProvider = new SingleBusinessObjectFactoryProvider(Factory);

			using (var tempFile = TempFile.New())
			{
				using (TextReader reader = new StreamReader(OrgTestFileStream))
				{
					var importer = new FlatFileDataImporterForTesting_DuplicatesPrevention(factoryProvider);
					var returnVal = importer.ImportData(reader, tempFile.Filename, new NotificationBuffer(), SourceInfo.EmptySourceInfo);
					AssertEquals("Return Value should be as expected", onSuccess, returnVal);
				}

				var newRecords = Factory.Load<StmDataImportHistory>(new ZQuery(StmDataImportHistorySchema.PK, SQLComparisonOperator.NotEqual, new ZGuid[] { recordToPurgePK, recordToKeepPK, unrelatedRecordPK }));
				var newRecord = newRecords.FirstOrDefault();
				if (onSuccess)
				{
					AssertHistoryRecord(newRecord, "TEST", ZDateTimeOffset.Now, tempFile.Filename, expectedHash);
				}
				else
				{
					AssertNull("No new history record as import should fail due to existing history record with samer hash", newRecord);
				}
			}

			var newFactory = new BusinessObjectFactory();
			AssertNull("Old history record should be deleted", newFactory.Load<StmDataImportHistory>(recordToPurgePK));
			AssertNotNull("Not so old history record should be kept", newFactory.Load<StmDataImportHistory>(recordToKeepPK));
			AssertNotNull("Unrelated history record should be kept", newFactory.Load<StmDataImportHistory>(unrelatedRecordPK));
		}

		readonly byte[] expectedHash = new byte[] { 62, 60, 62, 70, 6, 51, 23, 32, 182, 67, 192, 234, 178, 241, 12, 187, 142, 118, 249, 23, 136, 58, 76, 116, 65, 164, 112, 18, 171, 222, 12, 63 };

		void AssertHistoryRecord(StmDataImportHistory historyRecord, ZString importType, ZDateTimeOffset importTime, ZString sourceDescription, byte[] hash)
		{
			AssertNotNull("History Record", historyRecord);
			AssertEquals("DIH_GC", Environment.Env.CurrentCompanyPK, historyRecord.DIH_GC);
			AssertEquals("DIH_ImportType", importType, historyRecord.DIH_ImportType);
			AssertEquals("DIH_GS_NKImportStaff", "E", historyRecord.DIH_GS_NKImportStaff);
			AssertEquals("DIH_ImportDateTime", importTime, historyRecord.DIH_ImportDateTime);
			AssertEquals("DIH_SourceDescription", sourceDescription, historyRecord.DIH_SourceDescription);
			AssertEquals("DIH_DataHash", new ZBlob(hash), historyRecord.DIH_DataHash);
		}

		class FlatFileDataImporterForTesting : FlatFileDataImporter
		{
			public FlatFileDataImporterForTesting()
			{
			}

			public FlatFileDataImporterForTesting(BusinessObjectFactoryProvider factoryProvider)
				: base(factoryProvider)
			{
			}

			public FlatFileDataImporterForTesting(BusinessObject bizO)
				: base(bizO)
			{
			}

			public new bool ImportDataToFactory(
				TextReader dataReader, string attachmentFileName,
				INotifications notifications, ISourceInfo info, out ITransactionParticipant[] additionalTransactionActions)
			{
				return base.ImportDataToFactory(dataReader, attachmentFileName, notifications, info, out additionalTransactionActions);
			}

			protected override IFlatFileFormat FlatFileFormat
			{
				get { return new CsvFlatFileFormat(); }
			}

			protected override bool ExtractToDataAdapter(IValueObject xSD, INotifications notifications)
			{
				var orgs = (Xsd.Organisations)xSD;

				if (orgs.Organisation != null)
				{
					foreach (Xsd.Organisation org in orgs.Organisation)
					{
						var dataAdapter = new OrganisationValueObjectDataAdapter();
						var orgToImport = FactoryProvider.Current.New<OrgHeader>();

						var importContext = new ValueObjectImportContext(FactoryProvider, notifications);
						((IValueObjectDataAdapter)dataAdapter).ImportFromValueObject(orgToImport, org, importContext);
						orgToImport.OH_Code = org.EDICode;
					}
				}

				return true;
			}

			protected override IFlatFileConverter CreateConverter(INotifications notifications)
			{
				return new OrgFlatFileConverterForTesting(notifications, FactoryProvider.Current);
			}

			protected override IValueObject CreateXsd()
			{
				return new Xsd.Organisations();
			}

			public bool IsValidDataBeforeImportHasBeenHit;
			protected override (bool Result, TextReader ReaderOut, byte[] Hash) IsValidDataBeforeImport(TextReader reader, INotifications notifications)
			{
				IsValidDataBeforeImportHasBeenHit = true;
				return base.IsValidDataBeforeImport(reader, notifications);
			}

			protected override IMailFilter GetMailItemFilter() => QueryMailFilter.AllQueuedItems_ForTesting;
		}

		class FlatFileDataImporterForTesting_ReturnFalseOnImport : FlatFileDataImporterForTesting
		{
			protected override bool ImportDataToFactoryCore(TextReader dataReader, string attachmentFileName, INotifications notifications, out ITransactionParticipant[] additionalTransactionActions)
			{
				additionalTransactionActions = null;
				return false;
			}
		}

		class FlatFileDataImporterForTesting_DuplicatesPrevention : FlatFileDataImporterForTesting
		{
			public FlatFileDataImporterForTesting_DuplicatesPrevention()
				: base()
			{
			}

			public FlatFileDataImporterForTesting_DuplicatesPrevention(BusinessObjectFactoryProvider factoryProvider)
				: base(factoryProvider)
			{
			}

			public SqlApplicationLock ImportLockWithHash_ForTestOnly => ImportLockWithHash;

			public bool SupportsHistoryForDuplicatesPrevention_ForTestOnly => SupportsHistoryForDuplicatesPrevention;

			protected override string ImportTypeForDuplicatesPrevention => "TEST";
			protected override int DaysToKeepHistoryFor => 1;
		}

		class FlatFileDataImporterForTesting_DuplicatesPrevention_DifferentCompany : FlatFileDataImporterForTesting
		{
			public FlatFileDataImporterForTesting_DuplicatesPrevention_DifferentCompany()
				: base()
			{
			}

			public FlatFileDataImporterForTesting_DuplicatesPrevention_DifferentCompany(BusinessObjectFactoryProvider factoryProvider)
				: base(factoryProvider)
			{
			}

			public SqlApplicationLock ImportLockWithHash_ForTestOnly => ImportLockWithHash;

			public bool SupportsHistoryForDuplicatesPrevention_ForTestOnly => SupportsHistoryForDuplicatesPrevention;

			protected override string ImportTypeForDuplicatesPrevention => "TEST";
			protected override int DaysToKeepHistoryFor => 1;

			protected override bool ShouldCheckImportHistoryInCurrentCompany => false;
		}
	}
}
