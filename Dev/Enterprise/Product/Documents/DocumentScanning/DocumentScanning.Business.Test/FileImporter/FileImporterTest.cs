using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DocumentEngine.Imaging;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.RemoteDesktopServices;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.Business.Test
{
	public class FileImporterExposed : FileImporter
	{
		public FileImporterExposed(DocumentFactory docFactory, bool isForImport)
			: base(docFactory, isForImport)
		{
		}
	}

	public static class GCHelper
	{
		const int GarbageCollectorWaitMillis = 3 * 1000;
		public static bool RunGarbageCollector()
		{
			Thread t = new Thread(() =>
			{
				GC.Collect();
				GC.WaitForPendingFinalizers();
			});

			t.Start();
			return t.Join(GarbageCollectorWaitMillis);
		}
	}

	[TestedType(typeof(FileImporter))]
	sealed class FileImporterTest : DocumentImporterTestCase
	{
		readonly string TestRepositoryPath = TestUtils.TestRepositoryPath;

		public void TestSinglePageImportUsesDocumentResultWhenCoverSheetIsTrue()
		{
			var importer = new FileImporter(MasterFactory, true)
			{
				OutputOption = Constants.NewFileSingle,
				IsUsingCoverSheet = true,
				JobType = "ORG",
				DocType = "SYS"
			};

			var result = importer.ExecuteSort(MultipageTestDocumentTifPath);
			try
			{
				CombineAssertions(() =>
				{
					var i = 0;
					foreach (var document in result)
					{
						AssertEquals($"Because we have a cover sheet all should be of the type DocumentResult. Page {++i}.", typeof(DocumentResult), document.GetType());
					}
				});
			}
			finally
			{
				CleanupFile(result);
			}
		}

		#region Testing scanning options - set 1

		public void TestReadTwoBarcodesOnSameDocument()
		{
			Importer.OutputOption = Constants.Automatic;
			// import the files
			var twoBarcodesOnSamePageTifPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentScanning.Business.Test.TestDocs.TwoBarcodesOnSamePage.TIF");
			ScanningFinishedEventArgs result = Importer.ExecuteSort(twoBarcodesOnSamePageTifPath);
			try
			{
				AssertEquals("Number of documents: 1", 1, result.FileDetailsCount);

				DocumentResult singleResult = result.GetFileDetail(0);
				Assert("File should exist", File.Exists(singleResult.FilePath));

				using (ImageFileReaderWithLock testPageReader = new ImageFileReaderWithLock(singleResult.FilePath))
				{
					AssertEquals("Number of pages ", 1, testPageReader.PageSelector.TotalPages);
					AssertEquals("ScannedBarcodeValue", "^DMC=CN00000023;MFD;| , ^DMC=CN00000004;MFD;|", singleResult.ScannedBarcodeValue);
				}
			}
			finally
			{
				CleanupFile(result);
			}
		}

		public void TestExecuteSortMultipageFileWithAutomatic()
		{
			SetupShipmentObjects();
			Importer.OutputOption = Constants.Automatic;
			// import the files
			ScanningFinishedEventArgs result = Importer.ExecuteSort(MultipageTestDocumentTifPath);
			try
			{
				AssertEquals("Number of documents: 4", 4, result.FileDetailsCount);

				for (int i = 0; i < result.FileDetailsCount; i++)
				{
					DocumentResult singleResult = result.GetFileDetail(i);
					Assert("File should exist", File.Exists(singleResult.FilePath));

					using (ImageFileReaderWithLock testPageReader = new ImageFileReaderWithLock(singleResult.FilePath))
					{
						switch (i)
						{
							case 0: // first document in lot
								AssertEquals("Number of pages ", 1, testPageReader.PageSelector.TotalPages);
								Assert("SingleResult should be a basebarcode type", singleResult is BaseBarcode);
								AssertEquals("it should be a shipment reftype", Core.Constants.DocManagerCodes.Shipment, ((BaseBarcode)singleResult).DocManagerCode);
								AssertEquals("ref code", "S00001000", ((BaseBarcode)singleResult).RefCode);
								AssertEquals("ScannedBarcodeValue", "^SHP=S00001000| , ^DOC=CIV|", ((BaseBarcode)singleResult).ScannedBarcodeValue);
								AssertEquals("doctype", "CIV", ((BaseBarcode)singleResult).DocType);
								AssertEquals("RefPK should match", ShipmentS00001000.PK, ((BaseBarcode)singleResult).RefPK);
								break;

							case 1: // second document in lot
								AssertEquals("Number of pages - should keep the document with the barcode because it's actually relevant", 2, testPageReader.PageSelector.TotalPages);
								Assert("SingleResult should be a basebarcode type", singleResult is BaseBarcode);
								AssertEquals("it should be a shipment reftype", Core.Constants.DocManagerCodes.Shipment, ((BaseBarcode)singleResult).DocManagerCode);
								AssertEquals("refcode", ShipmentWithHouseBill[JobShipmentSchema.JS_UniqueConsignRef].ToString(), ((BaseBarcode)singleResult).RefCode);
								AssertEquals("ScannedBarcodeValue", "[ROHHBLSYDLAX12345678901234567890]", ((BaseBarcode)singleResult).ScannedBarcodeValue);
								AssertEquals("doctype", "HBL", ((BaseBarcode)singleResult).DocType);
								AssertEquals("RefPK should match", ShipmentWithHouseBill.PK, ((BaseBarcode)singleResult).RefPK);
								break;

							case 2: // third document in lot
								AssertEquals("Number of pages ", 2, testPageReader.PageSelector.TotalPages);
								Assert("SingleResult should be a basebarcode type", singleResult is BaseBarcode);
								AssertEquals("it has no reftype", ZString.Empty, ((BaseBarcode)singleResult).DocManagerCode);
								AssertEquals("it has no ref code", ZString.Empty, ((BaseBarcode)singleResult).RefCode);
								AssertEquals("ScannedBarcodeValue", "^DOC=PKL|", ((BaseBarcode)singleResult).ScannedBarcodeValue);
								AssertEquals("doc type found", "PKL", ((BaseBarcode)singleResult).DocType);
								AssertEquals("doc type found", ZGuid.Empty, ((BaseBarcode)singleResult).RefPK);
								break;

							case 3: // fourth document in lot
								AssertEquals("Number of pages ", 1, testPageReader.PageSelector.TotalPages);
								Assert("SingleResult should be a basebarcode type", singleResult is BaseBarcode);
								AssertEquals("it has no reftype", ZString.Empty, ((BaseBarcode)singleResult).DocManagerCode);
								AssertEquals("it has no ref code", ZString.Empty, ((BaseBarcode)singleResult).RefCode);
								AssertEquals("ScannedBarcodeValue", "^DOC=MAN|", ((BaseBarcode)singleResult).ScannedBarcodeValue);
								AssertEquals("doc type found", "MAN", ((BaseBarcode)singleResult).DocType);
								AssertEquals("doc type found", ZGuid.Empty, ((BaseBarcode)singleResult).RefPK);
								break;
						}
					}
				}
			}
			finally
			{
				CleanupFile(result);
			}
		}

		public void TestExecuteSortMultipageFileWithAutomaticSingle()
		{
			SetupShipmentObjects();
			Importer.OutputOption = Constants.AutomaticSingle;
			// import the files
			ScanningFinishedEventArgs result = Importer.ExecuteSort(MultipageTestDocumentTifPath);
			try
			{
				AssertEquals("Number of documents: 6", 6, result.FileDetailsCount);

				for (int i = 0; i < result.FileDetailsCount; i++)
				{
					DocumentResult singleResult = result.GetFileDetail(i);
					Assert("File should exist", File.Exists(singleResult.FilePath));

					using (ImageFileReaderWithLock testPageReader = new ImageFileReaderWithLock(singleResult.FilePath))
					{
						switch (i)
						{
							case 0: // first document in lot
								AssertEquals("Number of pages ", 1, testPageReader.PageSelector.TotalPages);
								Assert("SingleResult should be a basebarcode type", singleResult is BaseBarcode);
								AssertEquals("it should be a shipment reftype", Core.Constants.DocManagerCodes.Shipment, ((BaseBarcode)singleResult).DocManagerCode);
								AssertEquals("ref code", "S00001000", ((BaseBarcode)singleResult).RefCode);
								AssertEquals("ScannedBarcodeValue", "^SHP=S00001000| , ^DOC=CIV|", ((BaseBarcode)singleResult).ScannedBarcodeValue);
								AssertEquals("doctype", "CIV", ((BaseBarcode)singleResult).DocType);
								AssertEquals("RefPK should match", ShipmentS00001000.PK, ((BaseBarcode)singleResult).RefPK);
								break;

							case 1: // second document in lot
								AssertEquals("Number of pages (the barcoded page is kept because it's a special barcode)", 1, testPageReader.PageSelector.TotalPages);
								Assert("SingleResult should be a basebarcode type", singleResult is BaseBarcode);
								AssertEquals("it should be a shipment reftype", Core.Constants.DocManagerCodes.Shipment, ((BaseBarcode)singleResult).DocManagerCode);
								AssertEquals("refcode", ShipmentWithHouseBill[JobShipmentSchema.JS_UniqueConsignRef], ((BaseBarcode)singleResult).RefCode);
								AssertEquals("ScannedBarcodeValue", "[ROHHBLSYDLAX12345678901234567890]", ((BaseBarcode)singleResult).ScannedBarcodeValue);
								AssertEquals("doctype", "HBL", ((BaseBarcode)singleResult).DocType);
								AssertEquals("RefPK should match", ShipmentWithHouseBill.PK, ((BaseBarcode)singleResult).RefPK);
								break;

							case 2: // third document in lot
								AssertEquals("Number of pages (the barcoded page is kept because it's a special barcode)", 1, testPageReader.PageSelector.TotalPages);
								Assert("SingleResult should be a basebarcode type", singleResult is BaseBarcode);
								AssertEquals("it should be a shipment reftype", Core.Constants.DocManagerCodes.Shipment, ((BaseBarcode)singleResult).DocManagerCode);
								AssertEquals("refcode", ShipmentWithHouseBill[JobShipmentSchema.JS_UniqueConsignRef], ((BaseBarcode)singleResult).RefCode);
								AssertEquals("ScannedBarcodeValue", string.Empty, ((BaseBarcode)singleResult).ScannedBarcodeValue);
								AssertEquals("doctype", "HBL", ((BaseBarcode)singleResult).DocType);
								AssertEquals("RefPK should match", ShipmentWithHouseBill.PK, ((BaseBarcode)singleResult).RefPK);
								break;

							case 3: // fourth document in lot
								AssertEquals("Number of pages ", 1, testPageReader.PageSelector.TotalPages);
								Assert("SingleResult should be a basebarcode type", singleResult is BaseBarcode);
								AssertEquals("it has no reftype", ZString.Empty, ((BaseBarcode)singleResult).DocManagerCode);
								AssertEquals("it has no ref code", ZString.Empty, ((BaseBarcode)singleResult).RefCode);
								AssertEquals("ScannedBarcodeValue", "^DOC=PKL|", ((BaseBarcode)singleResult).ScannedBarcodeValue);
								AssertEquals("doc type found", "PKL", ((BaseBarcode)singleResult).DocType);
								AssertEquals("doc type found", ZGuid.Empty, ((BaseBarcode)singleResult).RefPK);
								break;

							case 4: // fifth document in lot
								AssertEquals("Number of pages ", 1, testPageReader.PageSelector.TotalPages);
								Assert("SingleResult should be a basebarcode type", singleResult is BaseBarcode);
								AssertEquals("it has no reftype", ZString.Empty, ((BaseBarcode)singleResult).DocManagerCode);
								AssertEquals("it has no ref code", ZString.Empty, ((BaseBarcode)singleResult).RefCode);
								AssertEquals("ScannedBarcodeValue", ZString.Empty, ((BaseBarcode)singleResult).ScannedBarcodeValue);
								AssertEquals("doc type found", "PKL", ((BaseBarcode)singleResult).DocType);
								AssertEquals("doc type found", ZGuid.Empty, ((BaseBarcode)singleResult).RefPK);
								break;

							case 5: // sixth document in lot
								AssertEquals("Number of pages ", 1, testPageReader.PageSelector.TotalPages);
								Assert("SingleResult should be a basebarcode type", singleResult is BaseBarcode);
								AssertEquals("it has no reftype", ZString.Empty, ((BaseBarcode)singleResult).DocManagerCode);
								AssertEquals("it has no ref code", ZString.Empty, ((BaseBarcode)singleResult).RefCode);
								AssertEquals("ScannedBarcodeValue", "^DOC=MAN|", ((BaseBarcode)singleResult).ScannedBarcodeValue);
								AssertEquals("doc type found", "MAN", ((BaseBarcode)singleResult).DocType);
								AssertEquals("doc type found", ZGuid.Empty, ((BaseBarcode)singleResult).RefPK);
								break;
						}
					}
				}
			}
			finally
			{
				CleanupFile(result);
			}
		}

		public void TestExecuteSortMultipageFileWithNewFileBatch()
		{
			Importer.OutputOption = Constants.NewFileBatch;
			// import the files
			ScanningFinishedEventArgs result = Importer.ExecuteSort(MultipageTestDocumentTifPath);
			try
			{
				AssertEquals("Number of documents: 1", 1, result.FileDetailsCount);

				DocumentResult singleResult = result.GetFileDetail(0);
				Assert("File should exist", File.Exists(singleResult.FilePath));

				using (ImageFileReaderWithLock testPageReader = new ImageFileReaderWithLock(singleResult.FilePath))
				{
					AssertEquals("Number of pages ", 10, testPageReader.PageSelector.TotalPages);
					Assert("result is not barcoded", !(singleResult is BaseBarcode));
				}
			}
			finally
			{
				// this function is supposed to create a new file for each document result,
				// but in the test we want to clean them up. 
				TryDeleteFile(result.GetFileDetail(0).FilePath);
			}
		}

		public void TestExecuteSortMultipageFileWithNewFileSingle()
		{
			Importer.OutputOption = Constants.NewFileSingle;

			// import the files
			ScanningFinishedEventArgs result = Importer.ExecuteSort(MultipageTestDocumentTifPath);
			try
			{
				AssertEquals("Number of documents: 10", 10, result.FileDetailsCount);

				for (int i = 0; i < result.FileDetailsCount; i++)
				{
					DocumentResult singleResult = result.GetFileDetail(i);
					Assert("File should exist", File.Exists(singleResult.FilePath));

					using (ImageFileReaderWithLock testPageReader = new ImageFileReaderWithLock(singleResult.FilePath))
					{
						AssertEquals("Number of pages - 1 page each document", 1, testPageReader.PageSelector.TotalPages);
						Assert("result is not barcoded", !(singleResult is BaseBarcode));
					}
				}
			}
			finally
			{
				CleanupFile(result);
			}
		}

		#endregion

		#region Testing scanning options - set 2

		public void TestExecuteSortMultipageFileWithAutomaticDifferentFile()
		{
			Importer.OutputOption = Constants.Automatic;

			// MultipageTifFile3 has two plain pages, a barcoded page, another two plain pages
			ScanningFinishedEventArgs result = Importer.ExecuteSort(MultipageTifFile3Path);
			try
			{
				AssertEquals("Number of documents: 3", 3, result.FileDetailsCount);

				for (int i = 0; i < result.FileDetailsCount; i++)
				{
					DocumentResult singleResult = result.GetFileDetail(i);
					Assert("File should exist", File.Exists(singleResult.FilePath));

					using (ImageFileReaderWithLock testPageReader = new ImageFileReaderWithLock(singleResult.FilePath))
					{
						switch (i)
						{
							case 0: // first document in lot
								AssertEquals("Number of pages ", 1, testPageReader.PageSelector.TotalPages);
								Assert("SingleResult is a plain page, not barcoded", !(singleResult is BaseBarcode));
								break;

							case 1: // second document in lot
								AssertEquals("Number of pages ", 1, testPageReader.PageSelector.TotalPages);
								Assert("SingleResult is a plain page, not barcoded", !(singleResult is BaseBarcode));
								break;

							case 2: // third document in lot
								AssertEquals("Number of pages ", 2, testPageReader.PageSelector.TotalPages);
								Assert("SingleResult should be a basebarcode type", singleResult is BaseBarcode);
								AssertEquals("it should have no Ref type", ZString.Empty, ((BaseBarcode)singleResult).DocManagerCode);
								AssertEquals("it should have no Ref code", ZString.Empty, ((BaseBarcode)singleResult).RefCode);
								AssertEquals("it has no job number so no value picked up", ZString.Empty, ((BaseBarcode)singleResult).RefCode);
								AssertEquals("doc type should be found", "CIV", ((BaseBarcode)singleResult).DocType);
								AssertEquals("it should have no RefPK", ZGuid.Empty, ((BaseBarcode)singleResult).RefPK);
								break;
						}
					}
				}
			}
			finally
			{
				CleanupFile(result);
			}
		}

		public void TestImportFileWithoutUsingCoverSheetMultiPagesTif()
		{
			Importer.IsUsingCoverSheet = false;
			Importer.JobType = Core.Constants.DocManagerCodes.Shipment;
			Importer.DocType = Core.Constants.RefDocTypes.ArrivalNotice;

			SetupShipmentObjects();
			Importer.OutputOption = Constants.Automatic;
			ScanningFinishedEventArgs result = Importer.ExecuteSort(MultipageTestDocumentTifPath);
			try
			{
				AssertEquals("Number of documents: 5", 5, result.FileDetailsCount);

				for (int i = 0; i < result.FileDetailsCount; i++)
				{
					DocumentResult singleResult = result.GetFileDetail(i);
					Assert("File should exist", File.Exists(singleResult.FilePath));

					using (ImageFileReaderWithLock testPageReader = new ImageFileReaderWithLock(singleResult.FilePath))
					{
						AssertEquals("Job type should come from importer", Importer.JobType, ((BaseBarcode)singleResult).DocManagerCode);
						AssertEquals("Doc type should come from importer", Importer.DocType, ((BaseBarcode)singleResult).DocType);

						switch (i)
						{
							case 0: // first document in lot
								AssertEquals("Number of pages ", 1, testPageReader.PageSelector.TotalPages);
								AssertEquals("ref code", "S00001000", ((BaseBarcode)singleResult).RefCode);
								AssertEquals("ScannedBarcodeValue", "^SHP=S00001000|", ((BaseBarcode)singleResult).ScannedBarcodeValue);
								AssertEquals("RefPK should match", ShipmentS00001000.PK, ((BaseBarcode)singleResult).RefPK);
								break;

							case 1: // second document in lot
								AssertEquals("Number of pages ", 2, testPageReader.PageSelector.TotalPages);
								AssertEquals("ref code", "S00001000", ((BaseBarcode)singleResult).RefCode);
								AssertEquals("ScannedBarcodeValue", "^DOC=CIV|", ((BaseBarcode)singleResult).ScannedBarcodeValue);
								AssertEquals("RefPK should match", ShipmentS00001000.PK, ((BaseBarcode)singleResult).RefPK);
								break;

							case 2: // third document in lot
								AssertEquals("Number of pages", 2, testPageReader.PageSelector.TotalPages);
								AssertEquals("refcode", ShipmentWithHouseBill[JobShipmentSchema.JS_UniqueConsignRef].ToString(), ((BaseBarcode)singleResult).RefCode);
								AssertEquals("ScannedBarcodeValue", "[ROHHBLSYDLAX12345678901234567890]", ((BaseBarcode)singleResult).ScannedBarcodeValue);
								AssertEquals("RefPK should match", ShipmentWithHouseBill.PK, ((BaseBarcode)singleResult).RefPK);
								break;

							case 3: // fourth document in lot
								AssertEquals("Number of pages ", 3, testPageReader.PageSelector.TotalPages);
								AssertEquals("it has no ref code", ZString.Empty, ((BaseBarcode)singleResult).RefCode);
								AssertEquals("it has no job number so no value picked up", "^DOC=PKL|", ((BaseBarcode)singleResult).ScannedBarcodeValue);
								AssertEquals("RefPK is empty", ZGuid.Empty, ((BaseBarcode)singleResult).RefPK);
								break;

							case 4: // fifth document in lot
								AssertEquals("Number of pages ", 2, testPageReader.PageSelector.TotalPages);
								AssertEquals("it has no ref code", ZString.Empty, ((BaseBarcode)singleResult).RefCode);
								AssertEquals("ScannedBarcodeValue", "^DOC=MAN|", ((BaseBarcode)singleResult).ScannedBarcodeValue);
								AssertEquals("RefPK is empty", ZGuid.Empty, ((BaseBarcode)singleResult).RefPK);
								break;
						}
					}
				}
			}
			finally
			{
				CleanupFile(result);
			}

			Importer.OutputOption = Constants.NewFileSingle;
			result = Importer.ExecuteSort(MultipageTestDocumentTifPath);
			try
			{
				AssertEquals("Number of documents: 10", 10, result.FileDetailsCount);

				for (int i = 0; i < result.FileDetailsCount; i++)
				{
					DocumentResult singleResult = result.GetFileDetail(i);
					Assert("File should exist", File.Exists(singleResult.FilePath));

					using (ImageFileReaderWithLock testPageReader = new ImageFileReaderWithLock(singleResult.FilePath))
					{
						AssertEquals("Job type should come from importer", Importer.JobType, ((BaseBarcode)singleResult).DocManagerCode);
						AssertEquals("Doc type should come from importer", Importer.DocType, ((BaseBarcode)singleResult).DocType);
					}
				}
			}
			finally
			{
				CleanupFile(result);
			}

			Importer.OutputOption = Constants.NewFileBatch;
			result = Importer.ExecuteSort(MultipageTestDocumentTifPath);
			try
			{
				AssertEquals("Number of documents: 1", 1, result.FileDetailsCount);

				DocumentResult singleResult = result.GetFileDetail(0);
				Assert("File should exist", File.Exists(singleResult.FilePath));

				using (ImageFileReaderWithLock testPageReader = new ImageFileReaderWithLock(singleResult.FilePath))
				{
					AssertEquals("Job type should come from importer", Importer.JobType, ((BaseBarcode)singleResult).DocManagerCode);
					AssertEquals("Doc type should come from importer", Importer.DocType, ((BaseBarcode)singleResult).DocType);
				}
			}
			finally
			{
				CleanupFile(result);
			}
		}

		/// <summary>
		/// this function is supposed to create a new file for each document result,
		// /but in the test we want to clean them up. 
		/// </summary>
		void CleanupFile(ScanningFinishedEventArgs result)
		{
			for (int i = 0; i < result.FileDetailsCount; i++)
			{
				DocumentResult singleResult = result.GetFileDetail(i);
				TryDeleteFile(singleResult.FilePath);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportFileWithoutUsingCoverSheetMultipleFiles_DocTypeSpecified()
		{
			Assert("Was unable to run the GC in an appropriate time for this memory heavy test. Maybe the finalizer queue is blocked?", GCHelper.RunGarbageCollector());

			Importer = new FileImporterExposed(MasterFactory, true);
			Importer.ImportSuccessful += new ScanningFinishedEventHandler(Importer_ImportSuccessful_NoCoverSheet_DocTypeSpecified);
			Importer.OutputOption = Constants.Automatic;
			Importer.IsUsingCoverSheet = false;
			Importer.JobType = Core.Constants.DocManagerCodes.Shipment;
			Importer.DocType = Core.Constants.RefDocTypes.ArrivalNotice;

			Importer.DefaultImportDirectory = Path.Combine(TestRepositoryPath, "NoCoverSheet");
			var failedImports = new List<string>();
			int importedFileCount = Importer.ImportFromDirectory(Importer.DefaultImportDirectory, failedImports);
			AssertEquals(5, importedFileCount + failedImports.Count);
		}

		void Importer_ImportSuccessful_NoCoverSheet_DocTypeSpecified(object sender, ScanningFinishedEventArgs ea)
		{
			foreach (DocumentResult result in ea)
			{
				try
				{
					BaseBarcode barcode = result as BaseBarcode;
					AssertNotNull("Result should be barcode.", barcode);
					AssertEquals("Job type should come from importer", Core.Constants.DocManagerCodes.Shipment, barcode.DocManagerCode);
					AssertEquals("Doc type should come from importer", Core.Constants.RefDocTypes.ArrivalNotice, barcode.DocType);
					if (barcode.FullBarcodeText == "^SHP=S00038411|")
					{
						AssertEquals("Should pick up the job id from barcode", "S00038411", barcode.RefCode);
						AssertEquals("Should pick up the job id from barcode", "^SHP=S00038411|", barcode.ScannedBarcodeValue);
					}
				}
				finally
				{
					File.Delete(result.FilePath);
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportFileWithoutUsingCoverSheetMultipleFiles_DocTypeNotSpecified()
		{
			Assert("Was unable to run the GC in an appropriate time for this memory heavy test. Maybe the finalizer queue is blocked?", GCHelper.RunGarbageCollector());

			Importer = new FileImporterExposed(MasterFactory, true);
			Importer.ImportSuccessful += new ScanningFinishedEventHandler(Importer_ImportSuccessful_NoCoverSheet_DocTypeNotSpecified);
			Importer.OutputOption = Constants.Automatic;
			Importer.IsUsingCoverSheet = false;
			Importer.JobType = Core.Constants.DocManagerCodes.Organisation;
			Importer.DocType = string.Empty;

			Importer.DefaultImportDirectory = Path.Combine(TestRepositoryPath, "NoCoverSheet");

			var failedImport = new List<string>();
			int importedFileCount = Importer.ImportFromDirectory(Importer.DefaultImportDirectory, failedImport);
			AssertEquals(5, importedFileCount + failedImport.Count);
		}

		void Importer_ImportSuccessful_NoCoverSheet_DocTypeNotSpecified(object sender, ScanningFinishedEventArgs ea)
		{
			foreach (DocumentResult result in ea)
			{
				try
				{
					BaseBarcode barcode = result as BaseBarcode;
					AssertNotNull("Result should be barcode.", barcode);
					AssertEquals("Job type should come from importer", Core.Constants.DocManagerCodes.Organisation, barcode.DocManagerCode);

					switch (barcode.FullBarcodeText)
					{
						case "^DOC=CIV|":
							AssertEquals("Doc type should come from barcode", "CIV", barcode.DocType);
							break;
						case "^SHP=S00038411|":
							AssertEquals("Doc type should come from barcode", "", barcode.DocType);
							AssertEquals("Should pick up the job id from barcode", "^SHP=S00038411|", barcode.ScannedBarcodeValue);
							AssertEquals("Should pick up the job id from barcode", "S00038411", barcode.RefCode);
							break;
						default:
							break;
					}
				}
				finally
				{
					File.Delete(result.FilePath);
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportFileWithoutUsingCoverSheetMultipleFiles_DocTypeAndJobTypeNotSpecified()
		{
			Assert("Was unable to run the GC in an appropriate time for this memory heavy test. Maybe the finalizer queue is blocked?", GCHelper.RunGarbageCollector());

			Importer = new FileImporterExposed(MasterFactory, true);
			Importer.ImportSuccessful += new ScanningFinishedEventHandler(Importer_ImportSuccessful_NoCoverSheet_DocTypeAndJobTypeNotSpecified);
			Importer.OutputOption = Constants.Automatic;
			Importer.IsUsingCoverSheet = false;
			Importer.JobType = string.Empty;
			Importer.DocType = string.Empty;

			Importer.DefaultImportDirectory = Path.Combine(TestRepositoryPath, "NoCoverSheet");
			int importedFileCount = Importer.ImportFromDirectory(Importer.DefaultImportDirectory, new List<string>());
			AssertEquals(5, importedFileCount);
		}

		public void TestImportFileCatchesUnreadableDocumentException()
		{
			var failedImport = new List<string>();
			var filesToCheck = new List<string>();
			var testDir = Directory.GetCurrentDirectory() + "\\UnreadableExceptionTest";
			var testFile = testDir + "\\Boom.png";
			Directory.CreateDirectory(testDir);
			filesToCheck.Add(testFile);

			AssertEquals("Wrong amount of files to check", 1, filesToCheck.Count);

			int importedFileCount = Importer.ImportFiles(filesToCheck, failedImport);

			AssertEquals("Wrong amount of files failed to import", 2, failedImport.Count); // 2 because of "The following files could not be imported:"
			AssertEquals("Wrong amount of files imported", 0, importedFileCount);

			AssertArrayEqualsByElements(new[]
			{
@"The following files could not be imported:",
$"- {Path.GetFileName(testFile)}: The document could not be read"
			}, failedImport.ToArray());

			Directory.Delete(testDir, true);
		}

		void Importer_ImportSuccessful_NoCoverSheet_DocTypeAndJobTypeNotSpecified(object sender, ScanningFinishedEventArgs ea)
		{
			foreach (DocumentResult result in ea)
			{
				try
				{
					BaseBarcode barcode = result as BaseBarcode;
					if (barcode != null)
					{
						switch (barcode.FullBarcodeText)
						{
							case "^DOC=CIV|":
								AssertEquals("Doc type should come from barcode", string.Empty, barcode.DocManagerCode);
								AssertEquals("Doc type should come from barcode", "CIV", barcode.DocType);
								break;
							case "^SHP=S00038411|":
								AssertEquals("Doc type should come from barcode", "SHP", barcode.DocManagerCode);
								AssertEquals("Doc type should come from barcode", "", barcode.DocType);
								AssertEquals("Should pick up the job id from barcode", "^SHP=S00038411|", barcode.ScannedBarcodeValue);
								AssertEquals("Should pick up the job id from barcode", "S00038411", barcode.RefCode);
								break;
							case "^DMC=CN00000004;MFD;|":
								AssertEquals("Doc type should come from barcode", "DMC", barcode.DocManagerCode);
								AssertEquals("Doc type should come from barcode", "MFD", barcode.DocType);
								AssertEquals("Should pick up the original barcode number.", "^DMC=CN00000004;MFD;|", barcode.ScannedBarcodeValue);
								// Don't test RefCode, as booking consigments are being deprecated
								break;
							default:
								AssertEquals("Doc type should come from barcode", string.Empty, barcode.DocManagerCode);
								AssertEquals("Doc type should come from barcode", string.Empty, barcode.DocType);
								break;
						}
					}
				}
				finally
				{
					File.Delete(result.FilePath);
				}
			}
		}

		public void TestExecuteSortMultipageFileWithAutomaticSingleDifferentFile()
		{
			Importer.OutputOption = Constants.AutomaticSingle;

			// MultipageTifFile3 has two plain pages, a barcoded page, another two plain pages
			ScanningFinishedEventArgs result = Importer.ExecuteSort(MultipageTifFile3Path);
			try
			{
				AssertEquals("Number of documents: 4", 4, result.FileDetailsCount);

				for (int i = 0; i < result.FileDetailsCount; i++)
				{
					DocumentResult singleResult = result.GetFileDetail(i);
					Assert("File should exist", File.Exists(singleResult.FilePath));

					using (ImageFileReaderWithLock testPageReader = new ImageFileReaderWithLock(singleResult.FilePath))
					{
						switch (i)
						{
							case 0: // first document in lot
								AssertEquals("Number of pages ", 1, testPageReader.PageSelector.TotalPages);
								Assert("SingleResult is a plain page, not barcoded", !(singleResult is BaseBarcode));
								break;

							case 1: // second document in lot
								AssertEquals("Number of pages ", 1, testPageReader.PageSelector.TotalPages);
								Assert("SingleResult is a plain page, not barcoded", !(singleResult is BaseBarcode));
								break;

							case 2: // third document in lot
								AssertEquals("Number of pages should be 1 - split into 2 docs", 1, testPageReader.PageSelector.TotalPages);
								Assert("SingleResult should be a BaseBarcode type", singleResult is BaseBarcode);
								AssertEquals("it should have no Ref type", ZString.Empty, ((BaseBarcode)singleResult).DocManagerCode);
								AssertEquals("it should have no Ref code", ZString.Empty, ((BaseBarcode)singleResult).RefCode);
								AssertEquals("ScannedBarcodeValue", "^DOC=CIV|", ((BaseBarcode)singleResult).ScannedBarcodeValue);
								AssertEquals("doc type should be found", "CIV", ((BaseBarcode)singleResult).DocType);
								AssertEquals("it should have no RefPK", ZGuid.Empty, ((BaseBarcode)singleResult).RefPK);
								break;

							case 3: // third document in lot
								AssertEquals("Number of pages should be 1 - split into 2 docs", 1, testPageReader.PageSelector.TotalPages);
								Assert("SingleResult should be a BaseBarcode type", singleResult is BaseBarcode);
								AssertEquals("it should have no Ref type", ZString.Empty, ((BaseBarcode)singleResult).DocManagerCode);
								AssertEquals("it should have no Ref code", ZString.Empty, ((BaseBarcode)singleResult).RefCode);
								AssertEquals("ScannedBarcodeValue", ZString.Empty, ((BaseBarcode)singleResult).ScannedBarcodeValue);
								AssertEquals("doc type should be found", "CIV", ((BaseBarcode)singleResult).DocType);
								AssertEquals("it should have no RefPK", ZGuid.Empty, ((BaseBarcode)singleResult).RefPK);
								break;
						}
					}
				}
			}
			finally
			{
				CleanupFile(result);
			}
		}

		public void TestExecuteSortMultipageFileWithNewFileBatchDifferentFile()
		{
			Importer.OutputOption = Constants.NewFileBatch;

			// MultipageTifFile3 has two plain pages, a barcoded page, another two plain pages
			ScanningFinishedEventArgs result = Importer.ExecuteSort(MultipageTifFile3Path);
			try
			{
				AssertEquals("Number of documents: 1", 1, result.FileDetailsCount);
				DocumentResult singleResult = result.GetFileDetail(0);
				Assert("File should exist", File.Exists(singleResult.FilePath));

				using (ImageFileReaderWithLock testPageReader = new ImageFileReaderWithLock(singleResult.FilePath))
				{
					AssertEquals("Number of pages should be 5, barcodes ignored", 5, testPageReader.PageSelector.TotalPages);
					Assert("SingleResult is a plain page, not barcoded", !(singleResult is BaseBarcode));
				}
			}
			finally
			{
				// this function is supposed to create a new file for each document result,
				// but in the test we want to clean them up. 
				TryDeleteFile(result.GetFileDetail(0).FilePath);
			}
		}

		public void TestExecuteSortMultipageFileWithNewFileSingleDifferentFile()
		{
			Importer.OutputOption = Constants.NewFileSingle;

			// MultipageTifFile3 has two plain pages, a barcoded page, another two plain pages
			ScanningFinishedEventArgs result = Importer.ExecuteSort(MultipageTifFile3Path);
			try
			{
				AssertEquals("Number of documents: 5", 5, result.FileDetailsCount);

				for (int i = 0; i < result.FileDetailsCount; i++)
				{
					DocumentResult singleResult = result.GetFileDetail(i);
					Assert("File should exist", File.Exists(singleResult.FilePath));

					using (ImageFileReaderWithLock testPageReader = new ImageFileReaderWithLock(singleResult.FilePath))
					{
						AssertEquals("Number of pages should be 1, barcodes ignored", 1, testPageReader.PageSelector.TotalPages);
						Assert("SingleResult is a plain page, not barcoded", !(singleResult is BaseBarcode));
					}
				}
			}
			finally
			{
				CleanupFile(result);
			}
		}

		#endregion

		#region Properties

		#region DefaultImportDirectory

		public void TestDefaultImportDirectory()
		{
			AssertEquals("Should have same as registry by default", Env.Registry.DMImportConfigurationSettings.DefaultDirectory, Importer.DefaultImportDirectory);
			Importer.DefaultImportDirectory = "C:\abc";  // Testing a path for default import directory
			AssertEquals("C:\abc", Importer.DefaultImportDirectory);
			Assert(Importer.HasChanges);

			Importer.DefaultImportDirectory = "C:\abcd";
			AssertEquals("C:\abcd", Importer.DefaultImportDirectory);
			Assert(Importer.HasChanges);
		}

		#endregion

		#region IsAutoAllocate

		public void TestIsAutoAllocate()
		{
			AssertEquals("Auto allocate should be same as registry by default", Env.Registry.DMImportConfigurationSettings.AutoAllocate, Importer.IsAutoAllocate);

			Importer.IsAutoAllocate = false;
			Assert("Should be able to freely assign IsAutoAllocate", !Importer.IsAutoAllocate);

			Importer.IsAutoAllocate = true;
			Assert("Should be able to freely assign IsAutoAllocate", Importer.IsAutoAllocate);
		}

		#endregion

		#region IsUsingCoverSheet

		public void TestIsUsingCoverSheet()
		{
			AssertEquals("IsUsingCoverSheet should be same as registry", Env.Registry.DMImportConfigurationSettings.AutoAllocate, Importer.IsUsingCoverSheet);
			AssertEquals("IsUsingCoverSheet should be true by default", true, Importer.IsUsingCoverSheet);

			Importer.IsUsingCoverSheet = false;
			Assert("Should be able to freely assign IsUsingCoverSheet", !Importer.IsUsingCoverSheet);

			Importer.IsUsingCoverSheet = true;
			Assert("Should be able to freely assign IsUsingCoverSheet", Importer.IsUsingCoverSheet);
		}

		#endregion

		#region IsOutputAutomatic

		public void TestIsOutputAutomatic()
		{
			if (CurrentRegistrySettings.OutputOption == Constants.Automatic)
			{
				AssertEquals("Scan Manager OutputOption should be Automatic", Constants.Automatic, Importer.OutputOption);
				Assert(Importer.IsOutputAutomatic);
			}
			else
			{
				Assert("Scan Manager OutputOption should not be Automatic", Constants.Automatic != Importer.OutputOption);
				Assert(!Importer.IsOutputAutomatic);
			}
		}

		#endregion

		#region IsOutputAutomaticSingle

		public void TestIsOutputAutomaticSingle()
		{
			if (CurrentRegistrySettings.OutputOption == Constants.AutomaticSingle)
			{
				AssertEquals("Scan Manager OutputOption should be AutomaticSingle", Constants.AutomaticSingle, Importer.OutputOption);
				Assert(Importer.IsOutputAutomaticSingle);
			}
			else
			{
				Assert("Scan Manager OutputOption should not be AutomaticSingle", Constants.AutomaticSingle != Importer.OutputOption);
				Assert(!Importer.IsOutputAutomaticSingle);
			}
		}

		#endregion

		#region IsOutputNewFileBatch

		public void TestIsOutputNewFileBatch()
		{
			if (CurrentRegistrySettings.OutputOption == Constants.NewFileBatch)
			{
				AssertEquals("Scan Manager OutputOption should be NewFileBatch", Constants.NewFileBatch, Importer.OutputOption);
				Assert(Importer.IsOutputNewFileBatch);
			}
			else
			{
				Assert("Scan Manager OutputOption should not be NewFileBatch", Constants.NewFileBatch != Importer.OutputOption);
				Assert(!Importer.IsOutputNewFileBatch);
			}
		}

		#endregion

		#region IsOutputNewFileSingle

		public void TestIsOutputNewFileSingle()
		{
			if (CurrentRegistrySettings.OutputOption == Constants.NewFileSingle)
			{
				AssertEquals("Scan Manager OutputOption should be NewFileSingle", Constants.NewFileSingle, Importer.OutputOption);
				Assert(Importer.IsOutputNewFileSingle);
			}
			else
			{
				Assert("Scan Manager OutputOption should not be NewFileSingle", Constants.NewFileSingle != Importer.OutputOption);
				Assert(!Importer.IsOutputNewFileSingle);
			}
		}

		#endregion

		#region OutputOption
		public void TestOutputOption()
		{
			AssertEquals("initially should equal registry setting", Env.Registry.DMImportConfigurationSettings.OutputOption, Importer.OutputOption);

			Importer.OutputOption = Constants.AutomaticSingle;
			AssertEquals("Correct string should be returned", Constants.AutomaticSingle, Importer.OutputOption);
			Assert("Boolean BindTo should be set for radio buttons", Importer.IsOutputAutomaticSingle);
			Assert("Options not set should be false", !Importer.IsOutputAutomatic);
			Assert("Options not set should be false", !Importer.IsOutputNewFileBatch);
			Assert("Options not set should be false", !Importer.IsOutputNewFileSingle);

			Importer.OutputOption = Constants.NewFileBatch;
			AssertEquals("Correct string should be returned", Constants.NewFileBatch, Importer.OutputOption);
			Assert("Options not set should be false", !Importer.IsOutputAutomatic);
			Assert("Options not set should be false", !Importer.IsOutputNewFileSingle);
			Assert("Options not set should be false", !Importer.IsOutputAutomaticSingle);
			Assert("Options not set should be true", Importer.IsOutputNewFileBatch);
		}
		#endregion

		#region DeleteSourceFilesAfterImport

		public void TestDeleteSourceFilesAfterImport()
		{
			AssertEquals("has same as registry setting on load", Env.Registry.DMImportConfigurationSettings.DeleteSourceFilesAfterImport, Importer.DeleteSourceFilesAfterImport);

			Importer.DeleteSourceFilesAfterImport = !Importer.DeleteSourceFilesAfterImport;
			AssertEquals("Should be able to change the value of DeleteSourceFilesAfterImport as normal - reversed value now expected",
				!Env.Registry.DMImportConfigurationSettings.DeleteSourceFilesAfterImport, Importer.DeleteSourceFilesAfterImport);
			Assert(Importer.HasChanges);
		}

		#endregion

		#region IncludeSubdirectories

		public void TestIncludeSubdirectories()
		{
			AssertEquals("has same as registry setting on load", Env.Registry.DMImportConfigurationSettings.IncludeSubdirectories, Importer.IncludeSubdirectories);

			Importer.IncludeSubdirectories = !Importer.IncludeSubdirectories;
			AssertEquals("Should be able to change the value of IncludeSubdirectories as normal - reversed value now expected",
				!Env.Registry.DMImportConfigurationSettings.IncludeSubdirectories, Importer.IncludeSubdirectories);
			Assert(Importer.HasChanges);
		}

		#endregion

		#region TestFileCount

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestFileCount()
		{
			CombineAssertions(() =>
			{
				Importer.DefaultImportDirectory = BaseSourcePath + @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs";

				Importer.IncludeSubdirectories = false;
				AssertEquals("number of files that should be retrieved from the TestDocs folder - non supported extensions should be ignored", 60, Importer.FileCount);

				Importer.IncludeSubdirectories = true;
				AssertEquals("number of files that should be retrieved including subfolders - non supported extensions should be ignored", 66, Importer.FileCount);
			});
		}

		#endregion

		#region TestJobTypeList

		public void TestJobTypeList()
		{
			AssertContainsExactElementsInAnyOrder(AssemblyDataLookup.DocManagerCodesForAllocation, Importer.JobTypeList);
		}

		#endregion

		#region TestDocTypeList

		public void TestDocTypeList()
		{
			Importer.JobType = Core.Constants.DocManagerCodes.Shipment;
			CodeDescriptionPairList shipmentDocTypes = new CodeDescriptionPairList();
			shipmentDocTypes.AddPair(string.Empty, "Use doc type from barcode.");
			shipmentDocTypes.AddRange(DocScanningHelper.GetCategoryDocTypesFromJobType(Importer.JobType, MasterFactory, false));
			AssertContainsExactElementsInAnyOrder(shipmentDocTypes, Importer.DocTypeList);

			Importer.JobType = Core.Constants.DocManagerCodes.Organisation;
			CodeDescriptionPairList organizationDocTypes = new CodeDescriptionPairList();
			organizationDocTypes.AddPair(string.Empty, "Use doc type from barcode.");
			organizationDocTypes.AddRange(DocScanningHelper.GetCategoryDocTypesFromJobType(Importer.JobType, MasterFactory, false));
			Env.Security.GetDocumentTypeUploadCheckPoint(organizationDocTypes[1].Code).IsAllowed = false;
			AssertContainsExactElementsInAnyOrder(organizationDocTypes, Importer.DocTypeList);
		}

		#endregion

		#endregion

		#region Validation

		#region ValidateDefaultImportDirectory

		public void TestValidateDefaultImportDirectory()
		{
			Assert("On load shouldn't have any errors", !Importer.DefaultImportDirectoryInfo.HasErrors());

			Importer.DefaultImportDirectory = @"C:\abcdefg";
			Assert("Set to a nonexistent directory - should error", Importer.DefaultImportDirectoryInfo.HasErrors());

			Importer.DefaultImportDirectory = @"C:\";
			Assert("Back to a valid directory - no error", !Importer.DefaultImportDirectoryInfo.HasErrors());
		}

		public void TestValidateDefaultImportDirectory_SelectPathClientMachine()
		{
			var terminalServiceMock = new Mock<TerminalService>();
			terminalServiceMock.Setup(x => x.IsRemoteAppSession).Returns(true);
			ObjectFactory.Substitute<TerminalService>(terminalServiceMock.Object);

			Importer.DefaultImportDirectory = @"\\127.0.0.1\C$";
			AssertEquals(1, Importer.DefaultImportDirectoryInfo.GetErrors().Count());
			AssertEquals("Please select a local directory to import", Importer.DefaultImportDirectoryInfo.GetErrors().First().Message);

			Importer.DefaultImportDirectory = $@"\\{System.Environment.MachineName}\C$";
			AssertEquals(1, Importer.DefaultImportDirectoryInfo.GetErrors().Count());
			AssertEquals("Please select a local directory to import", Importer.DefaultImportDirectoryInfo.GetErrors().First().Message);
		}

		public void TestValidateDefaultImportDirectory_SelectLocalPath()
		{
			var terminalServiceMock = new Mock<TerminalService>();
			terminalServiceMock.Setup(x => x.IsRemoteAppSession).Returns(false);
			ObjectFactory.Substitute<TerminalService>(terminalServiceMock.Object);

			Importer.DefaultImportDirectory = @"C:\"; // Testing a path for default import directory
			AssertEquals(false, Importer.DefaultImportDirectoryInfo.HasErrors());

			Importer.DefaultImportDirectory = @"C:\0A45C8DD96BB4E88BF1B60F4CF2BA91C"; // A non-exist local path
			AssertEquals(1, Importer.DefaultImportDirectoryInfo.GetErrors().Count());
			AssertEquals("Please select a valid directory to import", Importer.DefaultImportDirectoryInfo.GetErrors().First().Message);

			Importer.DefaultImportDirectory = @"\\127.0.0.1\C$";
			AssertEquals(1, Importer.DefaultImportDirectoryInfo.GetErrors().Count());
			AssertEquals("Please select a local directory to import", Importer.DefaultImportDirectoryInfo.GetErrors().First().Message);

			Importer.DefaultImportDirectory = $@"\\{System.Environment.MachineName}\C$";
			AssertEquals(1, Importer.DefaultImportDirectoryInfo.GetErrors().Count());
			AssertEquals("Please select a local directory to import", Importer.DefaultImportDirectoryInfo.GetErrors().First().Message);
		}

		public void TestValidateDefaultImportDirectory_SelectUNCPath()
		{
			var terminalServiceMock = new Mock<TerminalService>();
			terminalServiceMock.Setup(x => x.IsRemoteAppSession).Returns(false);
			ObjectFactory.Substitute<TerminalService>(terminalServiceMock.Object);

			// A valid UNC path with a non-exist folder, it should not check for directory existence
			Importer.DefaultImportDirectory = $@"\\{System.Environment.MachineName}\0A45C8DD96BB4E88BF1B60F4CF2BA91C";
			AssertEquals(1, Importer.DefaultImportDirectoryInfo.GetErrors().Count());
			AssertEquals("Please select a local directory to import", Importer.DefaultImportDirectoryInfo.GetErrors().First().Message);

			// A non-exist UNC path, it should not check for directory existence
			Importer.DefaultImportDirectory = @"\\B4E2D896-962A-4569-9011-E95A4A67320F\testunc";
			AssertEquals(1, Importer.DefaultImportDirectoryInfo.GetErrors().Count());
			AssertEquals("Please select a local directory to import", Importer.DefaultImportDirectoryInfo.GetErrors().First().Message);
		}

		#endregion

		#endregion

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportFromDirectory()
		{
			Importer = new FileImporterExposed(MasterFactory, true);
			Importer.ImportSuccessful += new ScanningFinishedEventHandler(Importer_ImportSuccessful);
			Importer.DefaultImportDirectory = Path.Combine(BaseSourcePath, TestDocsHelper.TestDocsPath, @"Subfolder");
			Importer.IncludeSubdirectories = false;
			int importedFileCount = Importer.ImportFromDirectory(Importer.DefaultImportDirectory, new List<string>());
			AssertEquals("imported file count (no subdirectories included)", 0, importedFileCount);

			Importer.IncludeSubdirectories = true;
			importedFileCount = Importer.ImportFromDirectory(Importer.DefaultImportDirectory, new List<string>());
			AssertEquals("imported file count with subdirectories included", 1, importedFileCount);
		}

		public void TestImportFromDirectoryWithCompanyInBarcode()
		{
			string directoryName = Path.Combine(Env.TempPath, "ImportTest");
			var filename = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentScanning.Business.Test.TestDocs.DDR3036.TIF");
			try
			{
				GlbCompany companyMEL = MasterFactory.New<GlbCompany>();
				companyMEL.GC_Code = "MEL";
				GlbBranch branchMel = MasterFactory.New<GlbBranch>();
				branchMel.GB_GC = companyMEL.PK;
				MasterFactory.Save();
				DocumentFactory factory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());

				ZGuid pkCurrentCompany = ZGuid.NewZGuid();
				Db.Connection.ExecuteNonQuery($"INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_GC, AH_GB, AH_GE, AH_Ledger, AH_TransactionNum, AH_InvoiceDate, AH_TransactionType) VALUES ('{pkCurrentCompany}', '{GlbCompany.CurrentCompany.PK}', '{GlbBranch.CurrentBranch.PK}', '{GlbDepartment.CurrentDepartment.PK}', 'CB', '00003036', GETDATE(), 'DDB')");

				Directory.CreateDirectory(directoryName);
				AssertImportAndResults(directoryName, filename, factory, ZGuid.Empty, "No RefPK should be matched as the transaction is not for MEL company.");

				ZGuid pkMelCompany = ZGuid.NewZGuid();
				Db.Connection.ExecuteNonQuery($"INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_GC, AH_GB, AH_GE, AH_Ledger, AH_TransactionNum, AH_InvoiceDate, AH_TransactionType) VALUES ('{pkMelCompany}', '{companyMEL.PK}', '{branchMel.PK}', '{GlbDepartment.CurrentDepartment.PK}', 'CB', '00003036', GETDATE(), 'DDB')");

				factory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
				using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branchMel.PK.ToGuid(), Env.CurrentDepartment.PK))
				{
					AssertImportAndResults(directoryName, filename, factory, pkMelCompany, "Mel Company transaction should be matched as the transaction is for MEL company.");
				}
			}
			finally
			{
				Directory.Delete(directoryName, true);
			}
		}

		void AssertImportAndResults(string directoryName, string filename, DocumentFactory factory, ZGuid pk, string refPkMessage)
		{
			using (TempFile tempFile = TempFile.New(directoryName, "tif"))
			{
				CopyFileAndSetupImporter(factory, filename, tempFile);

				AssertEquals(1, Importer.ImportFromDirectory(Importer.DefaultImportDirectory, new List<string>()));

				AssertEquals(0, Directory.GetFiles(directoryName).Length);
				AssertEquals(1, TestImportFromDirectoryWithCompanyInBarcodeLastResult.FileDetailsCount);
				foreach (DocumentResult result in TestImportFromDirectoryWithCompanyInBarcodeLastResult)
				{
					if (result is BaseBarcode)
					{
						AssertEquals(refPkMessage, pk, ((BaseBarcode)result).RefPK);
					}
					else
					{
						Fail("should be barcode!");
					}
					TestUtils.AssertCorrectNumberOfPages("converted TIF files should have just one page", result.FilePath, 2);
					File.Delete(result.FilePath);
				}
			}
		}

		void CopyFileAndSetupImporter(DocumentFactory factory, string filename, TempFile tempFile)
		{
			File.Copy(filename, tempFile.Filename, true);
			File.SetAttributes(tempFile.Filename, FileAttributes.Normal);

			Importer = new FileImporterExposed(factory, true);
			Importer.DefaultImportDirectory = Path.GetDirectoryName(tempFile.Filename);
			Importer.DeleteSourceFilesAfterImport = true;
			Importer.ImportSuccessful += new ScanningFinishedEventHandler(Importer_ImportSuccessful2);
		}

		ScanningFinishedEventArgs TestImportFromDirectoryWithCompanyInBarcodeLastResult;
		/// This goes with the above test
		void Importer_ImportSuccessful2(object sender, ScanningFinishedEventArgs ea)
		{
			TestImportFromDirectoryWithCompanyInBarcodeLastResult = ea;
		}

		[ExpectNoExceptions()]
		public void TestImportFromDirectoryWithFileOpen()
		{
			var smallGifPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentScanning.Business.Test.TestDocs.small.tif");
			var directoryName = Path.GetDirectoryName(smallGifPath);
			File.SetAttributes(smallGifPath, FileAttributes.Normal);

			Importer = new FileImporterExposed(MasterFactory, true);
			Importer.DefaultImportDirectory = directoryName;
			Importer.DeleteSourceFilesAfterImport = true;

			using (File.OpenWrite(smallGifPath))
			{
				try
				{
					Importer.ImportFromDirectory(Importer.DefaultImportDirectory, new List<string>());
				}
				catch (IOException) { }
			}

			AssertEquals("there should be no filename in the list of files not deleted", 0, Importer.FilesNotDeletedOnImport.Count);
		}

		/// This goes with the above test
		void Importer_ImportSuccessful(object sender, ScanningFinishedEventArgs ea)
		{
			foreach (DocumentResult result in ea)
			{
				TestUtils.AssertCorrectNumberOfPages("converted TIF files should have just one page", result.FilePath, 1);
				File.Delete(result.FilePath);
			}
		}

		public void TestReloadRegistrySettings()
		{
			DMImportConfigurationSettingsStruct registrySettings = Env.Registry.DMImportConfigurationSettings;
			FileImporter fileImporter = new FileImporter(MasterFactory, true);
			fileImporter.ReloadRegistrySettings();

			AssertEquals("FileImporter should have same settings as the Registry", registrySettings.AutoAllocate, fileImporter.IsAutoAllocate);
			AssertEquals(registrySettings.DefaultDirectory, fileImporter.DefaultImportDirectory);
			AssertEquals(registrySettings.DeleteSourceFilesAfterImport, fileImporter.DeleteSourceFilesAfterImport);
			AssertEquals(registrySettings.IncludeSubdirectories, fileImporter.IncludeSubdirectories);
			AssertEquals(registrySettings.OutputOption, fileImporter.OutputOption);
		}

		public void TestSaveRegistrySettings()
		{
			DMImportConfigurationSettingsStruct registrySettingsBefore = Env.Registry.DMImportConfigurationSettings;

			try
			{
				FileImporter fileImporter = new FileImporter(MasterFactory, true);
				AssertEquals("FileImporter should have same settings as the Registry", registrySettingsBefore.AutoAllocate, fileImporter.IsAutoAllocate);
				AssertEquals(registrySettingsBefore.DefaultDirectory, fileImporter.DefaultImportDirectory);
				AssertEquals(registrySettingsBefore.DeleteSourceFilesAfterImport, fileImporter.DeleteSourceFilesAfterImport);
				AssertEquals(registrySettingsBefore.IncludeSubdirectories, fileImporter.IncludeSubdirectories);
				AssertEquals(registrySettingsBefore.OutputOption, fileImporter.OutputOption);

				fileImporter.IsAutoAllocate = false;
				fileImporter.OutputOption = Constants.NewFileBatch;
				fileImporter.DefaultImportDirectory = @"C:\"; // Testing a path for default import directory
				fileImporter.DeleteSourceFilesAfterImport = false;
				fileImporter.IncludeSubdirectories = true;
				fileImporter.SaveRegistrySettings();

				DMImportConfigurationSettingsStruct registrySettingsAfter = Env.Registry.DMImportConfigurationSettings;
				AssertEquals("registry settings should be saved from FileImproter correctly", registrySettingsAfter.AutoAllocate, fileImporter.IsAutoAllocate);
				AssertEquals(registrySettingsAfter.DefaultDirectory, fileImporter.DefaultImportDirectory);
				AssertEquals(registrySettingsAfter.DeleteSourceFilesAfterImport, fileImporter.DeleteSourceFilesAfterImport);
				AssertEquals(registrySettingsAfter.IncludeSubdirectories, fileImporter.IncludeSubdirectories);
				AssertEquals(registrySettingsAfter.OutputOption, fileImporter.OutputOption);
			}
			catch
			{
				Env.Registry.DMImportConfigurationSettings = registrySettingsBefore;
			}
		}

		public void TestIsSupportedImageFile()
		{
			AssertEquals("should be supported file extension", true, FileImporter.IsSupportedImageFile(".BMP"));
			AssertEquals("should be supported file extension -lowercase", true, FileImporter.IsSupportedImageFile(".bmp"));

			AssertEquals("should be supported file extension", true, FileImporter.IsSupportedImageFile(".GIF"));
			AssertEquals("should be supported file extension -lowercase", true, FileImporter.IsSupportedImageFile(".gif"));

			AssertEquals("should be supported file extension", true, FileImporter.IsSupportedImageFile(".JPG"));
			AssertEquals("should be supported file extension -lowercase", true, FileImporter.IsSupportedImageFile(".jpg"));

			AssertEquals("should be supported file extension", true, FileImporter.IsSupportedImageFile(".PNG"));
			AssertEquals("should be supported file extension -lowercase", true, FileImporter.IsSupportedImageFile(".png"));

			AssertEquals("should be supported file extension", true, FileImporter.IsSupportedImageFile(".TIF"));
			AssertEquals("should be supported file extension -lowercase", true, FileImporter.IsSupportedImageFile(".tif"));

			AssertEquals("should be unsupported file extension", false, FileImporter.IsSupportedImageFile(".XLS"));
			AssertEquals("should be unsupported file extension -lowercase", false, FileImporter.IsSupportedImageFile(".xls"));

			AssertEquals("should be unsupported file extension", false, FileImporter.IsSupportedImageFile(".PDF"));
			AssertEquals("should be unsupported file extension -lowercase", false, FileImporter.IsSupportedImageFile(".pdf"));

			AssertEquals("should be unsupported file extension", false, FileImporter.IsSupportedImageFile(".EMF"));
		}

		public void TestIsSupported()
		{
			var supported = new[] { "BMP", "GIF", "PNG", "JPG", "JPEG", "TIFF", "TIF", "PDF" };
			var unsupported = new[] { "XLS", "CS", "WPF" };

			CombineAssertions(() =>
			{
				foreach (var s in supported)
				{
					Assert(s + " should be supported", FileImporter.IsSupported(s));
				}

				foreach (var s in unsupported)
				{
					Assert(s + " isn't be supported (yet)", !FileImporter.IsSupported(s));
				}
			});
		}

		public void TestMasterFactory()
		{
			AssertEquals("MasterFactory is same as instance passed in", MasterFactory, Importer.MasterFactory);
		}

		public void TestDefaultImportDirectoryLength()
		{
			Assert("DefaultImportDirectory should allow at least 1000 chars for those really really really long directory names", Importer.DefaultImportDirectoryInfo.MaxLength >= 1000);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new FileImporter(new DocumentFactoryProvider().GetFactory(Factory), false);
		}

		protected override void SetUp()
		{
			base.SetUp();
			Importer = new FileImporterExposed(MasterFactory, true);
			CurrentRegistrySettings = Env.Registry.DMImportConfigurationSettings;
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		protected override ScanningFinishedEventArgs GetImportResult(string outputOption, string sourceFileName)
		{
			Importer.OutputOption = outputOption;
			return Importer.ExecuteSort(sourceFileName);
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());

		string MultipageTestDocumentTifPath
		{
			get
			{
				if (string.IsNullOrEmpty(multipageTestDocumentTifPath))
				{
					multipageTestDocumentTifPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentScanning.Business.Test.TestDocs.MultipageTestDocument.tif");
				}
				return multipageTestDocumentTifPath;
			}
		}
		string multipageTestDocumentTifPath;

		string MultipageTifFile3Path
		{
			get
			{
				if (string.IsNullOrEmpty(multipageTifFile3Path))
				{
					multipageTifFile3Path = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentScanning.Business.Test.TestDocs.MultipageTifFile3.tif");
				}
				return multipageTifFile3Path;
			}
		}
		string multipageTifFile3Path;

		void TryDeleteFile(string path)
		{
			if (File.Exists(path))
			{
				File.Delete(path);
			}
		}

		FileImporterExposed Importer;
		DMImportConfigurationSettingsStruct CurrentRegistrySettings;
	}
}
