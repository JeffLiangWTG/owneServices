using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class ImportAndUpdateDataReferenceFileDataTest : TransactionedTestCase
	{
		[UseSnapshotProtection]
		public void TestRefundReason()
		{
			using (var otherConnection = Db.NewExtraConnectionToMainDb())
			{
				otherConnection.ExecuteScalar("Delete from " + CMRRefundReasonSchema.Constants.TableName);

				var sql = $@"Insert into {CMRRefundReasonSchema.Constants.TableName} (CR_PK, CR_RefundReasonType, CR_RefundReasonStartDate, CR_RefundReasonName, CR_RefundReasonTimeLimitDayCount, CR_RefundReasonDescription)
				  Values (newid(), 'A', '20000101', 'REFUND REASON CODE A', 1460, 'DETERIORATED DAMAGED LOST OR DESTROYED BEFORE CUSTOMS CONTROL')";
				otherConnection.ExecuteNonQuery(sql);

				AssertEquals("One row in db", 1, (int)otherConnection.ExecuteScalar("Select count(*) from " + CMRRefundReasonSchema.Constants.TableName));
			}

			var testFile = embeddedResourceRetriever.SaveResourceToFile("RFNRSNTP-P1-EDMAIN-2111050157.txt");
			var zipFilePath = Path.Combine(Env.TempPath, "RefundReason.tar.gz");

			try
			{
				CreateTarGZFromFile(zipFilePath, testFile);
				var zipFileBytes = File.ReadAllBytes(zipFilePath);
				var expectedLogs = $@"Processing 1 Unzipped Files in {importAndUpdateTestClass.FileSupporter.UnzipFilePath}.";

				importAndUpdateTestClass.ImportData_Exposed(zipFileBytes, TestConnection);
				AssertEquals(expectedLogs, GetLogEntriesAsText());

				AssertEquals("Data in db", 48, (int)TestConnection.ExecuteScalar("Select count(*) from " + CMRRefundReasonSchema.Constants.TableName));
			}
			finally
			{
				if (File.Exists(zipFilePath))
				{
					File.Delete(zipFilePath);
				}
			}
		}

		public void TestRefundReasonReportsBadData()
		{
			var testFile = embeddedResourceRetriever.SaveResourceToFile("RFNRSNTP-P1-EDMAIN-BAD.txt");
			var zipFilePath = Path.Combine(Env.TempPath, "RefundReason.tar.gz");

			try
			{
				CreateTarGZFromFile(zipFilePath, testFile);
				var zipFileBytes = File.ReadAllBytes(zipFilePath);
				var expectedLogs = $@"Processing 1 Unzipped Files in {importAndUpdateTestClass.FileSupporter.UnzipFilePath}.
RefundReasonType B has an entry with an invalid Start Date.
RefundReasonType ZA has an overlapping entry. Start Date of 2017-05-11 is invalid.
RefundReasonType ZA has an overlapping entry. End Date of  is invalid.
RefundReasonType ZB has an overlapping entry. Start Date of 2017-04-18 is invalid.
RefundReasonType ZC has an overlapping entry. End Date of 2017-04-18 is invalid.";

				importAndUpdateTestClass.ImportData_Exposed(zipFileBytes, TestConnection);
				AssertEquals(expectedLogs, GetLogEntriesAsText());

				AssertEquals("Data in db", 8, (int)TestConnection.ExecuteScalar("Select count(*) from " + CMRRefundReasonSchema.Constants.TableName));
			}
			finally
			{
				if (File.Exists(zipFilePath))
				{
					File.Delete(zipFilePath);
				}
			}
		}

		void CreateTarGZFromFile(string tgzFilename, string sourceFile)
		{
			using (var outStream = File.Create(tgzFilename))
			using (var gzoStream = new GZipOutputStream(outStream))
			using (var tarArchive = TarArchive.CreateOutputTarArchive(gzoStream))
			{
				var tarEntry = TarEntry.CreateEntryFromFile(sourceFile);
				tarEntry.Name = Path.GetFileName(sourceFile);
				tarArchive.WriteEntry(tarEntry, false);
				tarArchive.Close();
			}
		}

		[UseSnapshotProtection]
		public void TestUpgraderWithLogger()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				connection.ExecuteScalar("Delete from " + CMRCodeListsSchema.Constants.TableName);
				AssertEquals("No rows in db", 0, (int)connection.ExecuteScalar("Select count(*) from " + CMRCodeListsSchema.Constants.TableName));
			}

			var zipFile = embeddedResourceRetriever.GetBytes(GetEmbeddedResourcePath("MainFiles.tar.gz"));
			var testLogger = new DetailedLoggerForTest();
			var upgrader = new ImportReferenceFileData(testLogger);
			upgrader.ImportData(zipFile);

			var countCommand = "Select count(*) from " + CMRCodeListsSchema.Constants.TableName;
			var result = (int)TestConnection.ExecuteScalar(countCommand);
			AssertEquals("Data in db", 6, result);
			AssertContains("Processing 5 Unzipped Files", string.Join("\r\n", testLogger.Logs.Select(l => l.Item2)));
		}

		[UseSnapshotProtection]
		public void TestDynamicLoadUpgraderWithoutLogger()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				connection.ExecuteScalar("Delete from " + CMRCodeListsSchema.Constants.TableName);
				AssertEquals("No rows in db", 0, (int)connection.ExecuteScalar("Select count(*) from " + CMRCodeListsSchema.Constants.TableName));
			}

			var zipFile = embeddedResourceRetriever.GetBytes(GetEmbeddedResourcePath("MainFiles.tar.gz"));
			var upgrader = (ICMRReferenceFileUpgrader)CargoWise.Application.ObjectFactory.Get<Integration.Customs.AU.IImportAndUpdateDataReferenceFileData>();
			upgrader.ImportData(zipFile);

			var countCommand = "Select count(*) from " + CMRCodeListsSchema.Constants.TableName;
			var result = (int)TestConnection.ExecuteScalar(countCommand);
			AssertEquals("Data in db", 6, result);
		}

		public void TestRetryOnReadError()
		{
			ZString sqlCommand = "Delete from " + CMRCodeListsSchema.Constants.TableName;
			TestConnection.ExecuteScalar(sqlCommand);

			sqlCommand = "Select count(*) from " + CMRCodeListsSchema.Constants.TableName;
			int result = (int)TestConnection.ExecuteScalar(sqlCommand);
			AssertEquals("No rows in db", 0, result);

			bool shouldFail = true;
			var tempPaths = new List<ZString>();
			void DeleteFileFunc(IEnumerable<string> files)
			{
				if (shouldFail)
				{
					var fileToDelete = files.First(f => f.Contains("LGMNTQST"));
					File.Delete(fileToDelete);
					shouldFail = false;
				}
				tempPaths.Add(importAndUpdateTestClass.FileSupporter.UnzipFilePath);
			}
			importAndUpdateTestClass.OnImportDataFromFiles += DeleteFileFunc;

			file = embeddedResourceRetriever.GetBytes(GetEmbeddedResourcePath("MainFiles.tar.gz"));
			importAndUpdateTestClass.ImportData_Exposed(file, TestConnection);

			ZString firstTempPath = tempPaths[0];
			ZString lastTempPath = tempPaths[1];

			var expectedLogs = $@"Processing 5 Unzipped Files in {firstTempPath}.
Error while importing the CMR Reference File LGMNTQST.Could not find file '{firstTempPath}\LGMNTQST-Q1-EDMAIN-1202070020.txt'.
Processing 1 Unzipped Files in {lastTempPath}.";
			AssertEquals(expectedLogs, GetLogEntriesAsText());

			result = (int)TestConnection.ExecuteScalar(sqlCommand);
			AssertEquals("Data in db", 6, result);
		}

		public void TestRetryOnReadError_ReadFailure()
		{
			ZString sqlCommand = "Delete from " + CMRCodeListsSchema.Constants.TableName;
			TestConnection.ExecuteScalar(sqlCommand);

			sqlCommand = "Select count(*) from " + CMRCodeListsSchema.Constants.TableName;
			int result = (int)TestConnection.ExecuteScalar(sqlCommand);
			AssertEquals("No rows in db", 0, result);

			var tempPaths = new List<ZString>();
			void DeleteFileFunc(IEnumerable<string> files)
			{
				var fileToDelete = files.First(f => f.Contains("LGMNTQST"));
				File.Delete(fileToDelete);
				tempPaths.Add(importAndUpdateTestClass.FileSupporter.UnzipFilePath);
			}
			importAndUpdateTestClass.OnImportDataFromFiles += DeleteFileFunc;

			file = embeddedResourceRetriever.GetBytes(GetEmbeddedResourcePath("MainFiles.tar.gz"));
			var updateException = AssertExceptionThrown<UpdateReferenceFilesException>(() => importAndUpdateTestClass.ImportData_Exposed(file, TestConnection));
			AssertStartsWith("", @"Processing failed with errors. Error while importing the CMR Reference File LGMNTQST. Could not find file ", updateException.Message);

			ZString firstTempPath = tempPaths[0];
			ZString secondTempPath = tempPaths[1];
			ZString thirdTempPath = tempPaths[2];

			var expectedLogs = $@"Processing 5 Unzipped Files in {firstTempPath}.
Error while importing the CMR Reference File LGMNTQST.Could not find file '{firstTempPath}\LGMNTQST-Q1-EDMAIN-1202070020.txt'.
Processing 1 Unzipped Files in {secondTempPath}.
Error while importing the CMR Reference File LGMNTQST.Could not find file '{secondTempPath}\LGMNTQST-Q1-EDMAIN-1202070020.txt'.
Processing 1 Unzipped Files in {thirdTempPath}.
Error while importing the CMR Reference File LGMNTQST.Could not find file '{thirdTempPath}\LGMNTQST-Q1-EDMAIN-1202070020.txt'.
Some files could not be processed.";
			AssertEquals(expectedLogs, GetLogEntriesAsText());

			result = (int)TestConnection.ExecuteScalar(sqlCommand);
			AssertEquals("Data in db", 6, result);
		}

		public void TestInsertTestData()
		{
			InsertData();

			ZString sqlCommand = "Select count(*) from " + CMRPreferenceRulePeriodSnapshotSchema.Constants.TableName;
			AssertEquals("Data in CMRPreferenceRulePeriodSnapshot Table", 17, (int)TestConnection.ExecuteScalar(sqlCommand));

			sqlCommand = "Select count(*) from " + CMRSeaImpendingArrivalsSchema.Constants.TableName;
			AssertEquals("Data in CMRSeaImpendingArrivals Table", 622, (int)TestConnection.ExecuteScalar(sqlCommand));

			sqlCommand = "Select count(*) from " + CMRMessageAdviceSchema.Constants.TableName;
			AssertEquals("Data in CMRMessageAdviceSchema Table", 477, (int)TestConnection.ExecuteScalar(sqlCommand));

			sqlCommand = @"select XM_MessageAdviceText from " + CMRMessageAdviceSchema.Constants.TableName + @"
where XM_MessageAdviceIdentifier = '315' and XM_MessageAdviceStartDate = '2008-07-03'";
			string expectedText = "FROM 1 JULY 2008 THE IMPORTATION OF LASER POINTERS INTO AUSTRALIA WILL BE CONTROLLED UNDER SCHEDULE 2 OF THE CUSTOMS (PROHIBITED IMPORTS) REGULATIONS 1956. THE CONTROL APPLIES TO ALL HAND-HELD LASER POINTERS, DESIGNED OR ADAPTED TO EMIT A LASER BEAM WITH AN ACCESSIBLE EMISSION LEVEL OF GREATER THAN 1 MILLIWATT (MW).  WRITTEN PERMISSION WILL BE REQUIRED TO IMPORT THESE GOODS INTO AUSTRALIA.  SEE WWW.CUSTOMS.GOV.AU FOR FURTHER INFORMATION.";
			AssertEquals("Full text including second line", expectedText, (string)TestConnection.ExecuteScalar(sqlCommand));

			sqlCommand = "Select count(*) from " + CMRLodgementQuestionSchema.Constants.TableName;
			AssertEquals("Data in CMRLodgementQuestionSchema Table", 456, (int)TestConnection.ExecuteScalar(sqlCommand));

			sqlCommand = @"select CQ_LodgementQuestionText from " + CMRLodgementQuestionSchema.Constants.TableName + @"
where CQ_LodgementQuestionIdentifier = '375' and CQ_LodgementQuestionStartDate = '2007-06-30'";
			expectedText = "I DECLARE THAT THESE GOODS ARE PART OF A BULK ORDER OR REQUIRE PAYMENT OF ALL DUTY AND TAXES";
			AssertEquals("Full text including second line", expectedText, (string)TestConnection.ExecuteScalar(sqlCommand));

			var query = new ZQuery(CMRSeaImpendingArrivalsSchema.SI_OriginalMessageTimestamp, "20070518163817631142");
			query.IsNoLock = true;

			using (IDataReader reader = TestConnection.Command("select top 1 * from " + CMRSeaImpendingArrivalsSchema.Constants.TableName + " where " + query.LiteralTextADO).ExecuteReader())
			{
				if (reader.Read())
				{
					AssertEquals("Original ETA", new ZDateTime(2007, 05, 26, 14, 0, 0), reader[CMRSeaImpendingArrivalsSchema.Constants.SI_OriginalETA]);
					AssertEquals("Original Port", "AUDAM", reader[CMRSeaImpendingArrivalsSchema.Constants.SI_OriginalFirstPortCode]);
					Assert("Current ETA", new ZDateTime(reader[CMRSeaImpendingArrivalsSchema.Constants.SI_CurrentETA]).IsEmpty);
					Assert("ATA", new ZDateTime(reader[CMRSeaImpendingArrivalsSchema.Constants.SI_ActualTimeOfArrival]).IsEmpty);
				}
				else
				{
					Fail();
				}
			}
		}

		public void TestInsertTestDataForCodeList()
		{
			ZString sqlCommand = "Delete from " + CMRCodeListsSchema.Constants.TableName;
			TestConnection.ExecuteScalar(sqlCommand);

			sqlCommand = "Select count(*) from " + CMRCodeListsSchema.Constants.TableName;
			int result = (int)TestConnection.ExecuteScalar(sqlCommand);
			AssertEquals("No rows in db", 0, result);

			TestConnection.ExecuteNonQuery(string.Format("insert into {0} (CI_PK, CI_Code)VALUES(newid(), 'FOO')", CMRCodeListsSchema.Constants.TableName));

			file = embeddedResourceRetriever.GetBytes(GetEmbeddedResourcePath("MainFiles.tar.gz"));
			importAndUpdateTestClass.ImportData_Exposed(file, TestConnection);

			result = (int)TestConnection.ExecuteScalar(sqlCommand);
			AssertEquals("Data in db", 6, result);
		}

		protected override void SetUp()
		{
			base.SetUp();
			testLogger = new DetailedLoggerForTest();
			importAndUpdateTestClass = new ImportReferenceFileDataForTesting(testLogger);
			embeddedResourceRetriever = new EmbeddedResourceRetriever();
		}

		protected override void TearDown()
		{
			base.TearDown();
			embeddedResourceRetriever.Dispose();
		}
		EmbeddedResourceRetriever embeddedResourceRetriever;
		ImportReferenceFileDataForTesting importAndUpdateTestClass;
		DetailedLoggerForTest testLogger;
		byte[] file;

		void InsertData()
		{
			ZString sqlCommand = "Delete from " + CMRPreferenceRulePeriodSnapshotSchema.Constants.TableName;
			TestConnection.ExecuteScalar(sqlCommand);

			sqlCommand = "Select count(*) from " + CMRPreferenceRulePeriodSnapshotSchema.Constants.TableName;
			int result = (int)TestConnection.ExecuteScalar(sqlCommand);
			AssertEquals("No rows in db", 0, result);

			file = embeddedResourceRetriever.GetBytes(GetEmbeddedResourcePath("MainFiles.tar.gz"));
			importAndUpdateTestClass.ImportData_Exposed(file, TestConnection);
		}

		string GetLogEntriesAsText()
		{
			var logEntries = testLogger.Logs.Select(l => ZString.Format("{0}{1}", l.Item2, l.Item3?.Message ?? string.Empty));
			return string.Join("\r\n", logEntries);
		}

		string GetEmbeddedResourcePath(string fileName) => "Enterprise.Customs.AU.Declaration.Business.Testing.Data.Import.CMRReferenceFileDataImporter.TestFiles." + fileName;

		sealed class ImportReferenceFileDataForTesting : ImportReferenceFileData
		{
			public ImportReferenceFileDataForTesting(ILogger logger) : base(logger)
			{
			}

			public void ImportData_Exposed(ZBlob fileToUnzip, DbConnection connection)
			{
				ImportData(fileToUnzip, connection);
			}

			public Action<IEnumerable<string>> OnImportDataFromFiles;

			protected override IEnumerable<string> ImportDataFromFiles(IEnumerable<string> unzippedFiles, DbConnection connection)
			{
				OnImportDataFromFiles?.Invoke(unzippedFiles);
				return base.ImportDataFromFiles(unzippedFiles, connection);
			}

			protected override int RetryWaitDelayMilliseconds => 1000;
		}
	}
}
