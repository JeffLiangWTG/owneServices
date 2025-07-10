using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.Imaging;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.Business.Test
{
	public abstract class TestUtils : TestCase
	{
		public static readonly string TestRepositoryPath = Path.Combine(TestCase.BaseSourcePath, TestDocsHelper.TestDocsPath, @"");

		public static void AssertCorrectNumberOfPages(StorageDocs document, int expectedPageCount)
		{
			AssertCorrectNumberOfPages(ZString.Empty, document, expectedPageCount);
		}

		public static void AssertCorrectNumberOfPages(string message, StorageDocs document, int expectedPageCount)
		{
			string tempFile = ZString.Empty;

			try
			{
				tempFile = document.SaveToTempFile();
				AssertCorrectNumberOfPages(message, tempFile, expectedPageCount);
			}
			finally
			{
				if (File.Exists(tempFile))
				{
					File.Delete(tempFile);
				}
			}
		}

		public static void AssertCorrectNumberOfPages(string filename, int expectedPageCount)
		{
			AssertCorrectNumberOfPages(ZString.Empty, filename, expectedPageCount);
		}

		public static void AssertCorrectNumberOfPages(string message, string filename, int expectedPageCount)
		{
			using (var fileReader = new ImageFileReaderWithLock(filename))
			{
				AssertEquals(message + " Page count expected " + expectedPageCount, expectedPageCount, fileReader.PageSelector.TotalPages);
			}
		}

		/// <summary>
		/// Create some rows simulating real-world usage for documents. 
		/// </summary>
		public static void CreateLotsOfDocumentRecords(bool markSomeAsDeleted)
		{
			var dbHelper = new DocManagerDBHelper();

			var insertDECRowsQuery =
				"INSERT INTO " + StorageMainSchema.Constants.SqlSchemaName + "." + StorageMainSchema.Constants.TableName + " (" +
				StorageMainSchema.PK.Name + ", " +
				StorageMainSchema.SM_Type.Name + ", " +
				StorageMainSchema.SM_DB.Name + ", " +
				StorageMainSchema.SM_ParentFK.Name + ")" +
				"SELECT TOP 100 newid(), 'DEC', '1', newid() " +
				"FROM sys.objects s1, sys.objects s2";

			Db.Connection.ExecuteNonQuery(insertDECRowsQuery); // Inserting large numbers of rows is much faster going directly to DB than doing it through ZArchitecture

			var insertSHPRowsQuery =
				"INSERT INTO " + StorageMainSchema.Constants.SqlSchemaName + "." + StorageMainSchema.Constants.TableName + " (" +
				StorageMainSchema.PK.Name + ", " +
				StorageMainSchema.SM_Type.Name + ", " +
				StorageMainSchema.SM_DB.Name + ", " +
				StorageMainSchema.SM_ParentFK.Name + ")" +
				"SELECT TOP 5000 newid(), 'SHP', '1', newid() " +
				"FROM sys.objects s1, sys.objects s2";

			Db.Connection.ExecuteNonQuery(insertSHPRowsQuery); // Inserting large numbers of rows is much faster going directly to DB than doing it through ZArchitecture

			var insertORGRowsQuery =
				"INSERT INTO " + StorageMainSchema.Constants.SqlSchemaName + "." + StorageMainSchema.Constants.TableName + " (" +
				StorageMainSchema.PK.Name + ", " +
				StorageMainSchema.SM_Type.Name + ", " +
				StorageMainSchema.SM_DB.Name + ", " +
				StorageMainSchema.SM_ParentFK.Name + ")" +
				"SELECT TOP 5000 newid(), 'ORG', '1', newid() " +
				"FROM sys.objects s1, sys.objects s2";

			Db.Connection.ExecuteNonQuery(insertORGRowsQuery); // Inserting large numbers of rows is much faster going directly to DB than doing it through ZArchitecture

			var insertCONRowsQuery =
				"INSERT INTO " + StorageMainSchema.Constants.SqlSchemaName + "." + StorageMainSchema.Constants.TableName + " (" +
				StorageMainSchema.PK.Name + ", " +
				StorageMainSchema.SM_Type.Name + ", " +
				StorageMainSchema.SM_DB.Name + ", " +
				StorageMainSchema.SM_ParentFK.Name + ")" +
				"SELECT TOP 5000 newid(), 'CON', '1', newid() " +
				"FROM sys.objects s1, sys.objects s2";

			Db.Connection.ExecuteNonQuery(insertCONRowsQuery); // Inserting large numbers of rows is much faster going directly to DB than doing it through ZArchitecture

			var insert10000DocumentsQuery =
				@"DECLARE @SqlText nvarchar(4000) 
				SET @SqlText = '' 
				DECLARE @Next_SMPK uniqueidentifier 

				DECLARE DbCursor CURSOR FAST_FORWARD READ_ONLY FOR
				SELECT top 10000 SM_PK FROM dbo.StorageMain

				OPEN DbCursor
				FETCH NEXT FROM DbCursor INTO @Next_SMPK
				WHILE (@@FETCH_STATUS = 0)
				BEGIN
				SET @SqlText = 'INSERT INTO " + dbHelper.GetDatabaseName(1) + @"..StorageDocs (SC_PK, SC_SM, SC_DocType, SC_Desc, SC_Date, SC_ImageData, SC_SystemCreateTimeUtc, SC_SystemLastEditTimeUtc) VALUES (newid(), @ParamNext_SMPK, ''MSC'', ''hello there'', ''2000-01-01'', cast(''12345678901234567890'' as varbinary(max)), ''2000-01-01'', ''2000-01-01'')'
				EXEC sp_executesql @SqlText, N'@ParamNext_SMPK uniqueidentifier', @ParamNext_SMPK = @Next_SMPK

				FETCH NEXT FROM DbCursor INTO @Next_SMPK
				END
				CLOSE DbCursor
				DEALLOCATE DbCursor";

			Db.Connection.ExecuteNonQuery(insert10000DocumentsQuery); // Inserting large numbers of rows is much faster going directly to DB than through ZArchitecture

			// create some unallocated documents 
			var testFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			var mains = new StorageMainCollection(testFactory);
			using (var resourceRetriever = new EmbeddedResourceRetriever())
			{
				var testTifBytes = resourceRetriever.GetBytes("Enterprise.DocumentScanning.Business.Test.TestDocs.Test.tif");

				for (var i = 0; i <= 50; ++i)
				{
					var newMain = mains.AddNew();
					var document = newMain.Documents.AddNew();
					document.SC_ImageData = testTifBytes;
					var org = testFactory.NewWithValidTestData<OrgHeader>();
					newMain.SM_ParentFK = org.PK;
					newMain.SM_DB = 1;
				}
			}

			testFactory.Save();

			if (markSomeAsDeleted)
			{
				var deleteSomeDocumentsQuery =
					@"DECLARE @SqlText nvarchar(4000) 
					SET @SqlText = '' 
					DECLARE @Next_SCPK uniqueidentifier 
					DECLARE DbCursor CURSOR FAST_FORWARD READ_ONLY FOR
					SELECT top 500 SC_PK FROM " + dbHelper.GetDatabaseName(1) + @"..StorageDocs

					OPEN DbCursor
					FETCH NEXT FROM DbCursor INTO @Next_SCPK
					WHILE (@@FETCH_STATUS = 0)
					BEGIN
					SET @SqlText = 'UPDATE " + dbHelper.GetDatabaseName(1) + @"..StorageDocs SET SC_IsDeleted = ''Y'' WHERE SC_PK = @ParamPK'
					EXEC sp_executesql @SqlText, N'@ParamPK uniqueidentifier', @ParamPK = @Next_SCPK

					FETCH NEXT FROM DbCursor INTO @Next_SCPK
					END
					CLOSE DbCursor
					DEALLOCATE DbCursor";
				Db.Connection.ExecuteNonQuery(deleteSomeDocumentsQuery); // Inserting large numbers of rows is much faster going directly to DB than through ZArchitecture
			}
		}
	}

	public class DocumentUtilitiesTest : TestCaseWithDocumentFactory
	{
		public void TestGetInverseList()
		{
			var aList = new[] { 1, 4, 5 };
			var result = DocumentUtilities.GetInverseList(aList, 7);

			// expecting result : 0, 2, 3, 6.
			AssertEquals("Count", 4, result.Length);
			AssertEquals("Found 0", 0, result[0]);
			AssertEquals("Found 1", 2, result[1]);
			AssertEquals("Found 2", 3, result[2]);
			AssertEquals("Found 3", 6, result[3]);
		}

		public void TestGetReorderPagesCommentMulti()
		{
			var comment = DocumentUtilities.GetReorderPagesComment(new[] { 2, 3 }, 0);

			AssertEquals("Comment (multi)", "Pages 3,4 moved after page 1", comment);
		}

		public void TestGetReorderPagesCommentSingle()
		{
			var comment = DocumentUtilities.GetReorderPagesComment(new[] { 2 }, 1);

			AssertEquals("Comment (single)", "Page 3 moved after page 2", comment);
		}

		public void TestGetReorderPagesCommentEmpty()
		{
			var comment = DocumentUtilities.GetReorderPagesComment(Array.Empty<int>(), 1);

			AssertEquals("Comment (empty)", "", comment);
		}

		public void TestGetReorderPagesCommentMultiFront()
		{
			var comment = DocumentUtilities.GetReorderPagesComment(new[] { 2, 3 }, -1);

			AssertEquals("Comment (multi)", "Pages 3,4 moved before page 1", comment);
		}

		public void TestGetReorderPagesCommentSingleFront()
		{
			var comment = DocumentUtilities.GetReorderPagesComment(new[] { 2 }, -1);

			AssertEquals("Comment (single)", "Page 3 moved before page 1", comment);
		}

		public void TestGetReorderPagesCommentEmptyFront()
		{
			var comment = DocumentUtilities.GetReorderPagesComment(Array.Empty<int>(), -1);

			AssertEquals("Comment (empty)", "", comment);
		}

		public void TestSaveToTempFile()
		{
			var doc1 = StorageDocs.NewWithParent_DEBUG(MasterFactory);
			doc1.SC_ImageData = SmallTifBytes;
			var tempFile = doc1.SaveToTempFile();

			Assert("File created and exists", File.Exists(tempFile));

			try
			{
				var testImage = Image.FromFile(tempFile);
				testImage.Dispose();
			}
			catch (Exception e)
			{
				Fail("Image.FromFile() failed ... it shouldn't have. Exception message: " + e.Message);
			}

			File.Delete(tempFile);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetDocumentImageFromFile()
		{
			var imagePath = TestUtils.TestRepositoryPath + "small_2pages.tif";
			var documentImage = DocumentUtilities.GetDocumentImageFromFile(imagePath);

			AssertNotNull("Document image exists", documentImage);
			AssertEquals("Got all pages", 2, documentImage.GetFrameCount(FrameDimension.Page));

			var stream = File.OpenRead(imagePath);

			AssertNotNull("Can still read stream of the file after retrieving image", stream);
			stream.Close();
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetFileAsBytes()
		{
			// Size 0 tests that reading the file with File.ReadAllBytes(...) works correctly
			DocManagerRegistry.Instance.FileUploadStreamBufferSize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);

			var imagePath = TestUtils.TestRepositoryPath + "small_2pages.tif";
			var documentBlob = DocumentUtilities.GetFileAsBytes(imagePath);
			var info = new FileInfo(imagePath);
			var expected = info.Length;

			AssertEquals("Size", expected, documentBlob.LongLength);

			var stream = File.OpenRead(imagePath);

			AssertNotNull("Can still read stream of the file after retrieving image", stream);
			stream.Close();

			// Size > 0 tests that reading the file with FileStream works correctly
			DocManagerRegistry.Instance.FileUploadStreamBufferSize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 8192);

			documentBlob = DocumentUtilities.GetFileAsBytes(imagePath);
			info = new FileInfo(imagePath);
			expected = info.Length;

			AssertEquals("Size", expected, documentBlob.LongLength);

			stream = File.OpenRead(imagePath);

			AssertNotNull("Can still read stream of the file after retrieving image", stream);
			stream.Close();
		}

		public void TestIsTiffFile()
		{
			AssertEquals("tif", true, DocumentUtilities.IsTiffFile("xxx.tif"));
			AssertEquals("tiff", true, DocumentUtilities.IsTiffFile("xxx.tiff"));
			AssertEquals("jpg", false, DocumentUtilities.IsTiffFile("xxx.jpg"));
			AssertEquals("csv", false, DocumentUtilities.IsTiffFile("xxx.csv"));
		}

		public void TestGetNewTempTiffFilename()
		{
			var newFilename = DocumentUtilities.GetTempFilename();

			AssertNotNullOrEmpty("New filename generated", newFilename);

			var info = new FileInfo(newFilename);

			Assert("File does not exist yet", !info.Exists);
		}

		public void TestGetImageFromFile()
		{
			var small3PagesTestFile = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentScanning.Business.Test.TestDocs.small_3pages.tif");
			using (var curImage = DocumentUtilities.GetImageFromFile(small3PagesTestFile))
			using (var selector = new StandardImagePageSelector(curImage))
			{
				Assert("Image", selector.CurrentImage != null);
				AssertEquals("Pages", 3, selector.TotalPages);
			}
		}

		public void TestConvertFileToTiffWithImageFiles()
		{
			var existingFile = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentScanning.Business.Test.TestDocs.small.tif");
			var resultingFile = DocumentUtilities.ConvertFileToTiff(existingFile);

			using (var convertedFile = Image.FromFile(resultingFile))
			{
				AssertEquals(System.Drawing.Imaging.ImageFormat.Tiff, convertedFile.RawFormat);
			}

			System.IO.File.Delete(resultingFile);

			resultingFile = DocumentUtilities.ConvertFileToTiff(SmallGifPath);

			using (var convertedFile = Image.FromFile(resultingFile))
			{
				AssertEquals(System.Drawing.Imaging.ImageFormat.Tiff, convertedFile.RawFormat);
			}

			System.IO.File.Delete(resultingFile);
		}

		public void TestConvertFileToTiffWithPDFFile()
		{
			ImportWithInvalidFile(resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentScanning.Business.Test.TestDocs.Sample.PDF"));
		}

		public void TestConvertFileToTiffWithXLSFile()
		{
			ImportWithInvalidFile(resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentScanning.Business.Test.TestDocs.Test.xls"));
		}

		void ImportWithInvalidFile(string existingFile)
		{
			var resultingFile = string.Empty;
			var exceptionThrown = false;

			try
			{
				resultingFile = DocumentUtilities.ConvertFileToTiff(existingFile);
			}
			catch (Exception)
			{
				exceptionThrown = true;
			}

			Assert("exception thrown when trying to convert non image file to an Image", exceptionThrown);
			Assert("Resulting file returned does not exist when the image conversion was not successful", !File.Exists(resultingFile));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestConvertFileToTiffForMultipageFiles()
		{
			var existingFile = Path.Combine(BaseSourcePath, TestDocsHelper.TestDocsPath, @"lowres_5pages.tif");
			var resultingFile = DocumentUtilities.ConvertFileToTiff(existingFile);

			using (var convertedImage = Image.FromFile(resultingFile))
			{
				var pageCount = convertedImage.GetFrameCount(new FrameDimension(convertedImage.FrameDimensionsList[0]));

				AssertEquals("converted image should have all 5 pages", 5, pageCount);
			}

			System.IO.File.Delete(resultingFile);

			resultingFile = DocumentUtilities.ConvertFileToTiff(SmallGifPath);

			using (var convertedImage = Image.FromFile(resultingFile))
			{
				var pageCount = convertedImage.GetFrameCount(new FrameDimension(convertedImage.FrameDimensionsList[0]));

				AssertEquals("Converted image has one page", 1, pageCount);
			}

			System.IO.File.Delete(resultingFile);
		}

		public void TestConvertFileToTiffWithByteArray()
		{
			// TIF file
			using (var file = SaveBytesToFile(DocumentUtilities.ConvertFileToTiff(SmallTifBytes, "small.tif")))
			using (var convertedFile = Image.FromFile(file.Filename))
			{
				AssertEquals(System.Drawing.Imaging.ImageFormat.Tiff, convertedFile.RawFormat);
			}

			// GIF file
			using (var file = SaveBytesToFile(DocumentUtilities.ConvertFileToTiff(SmallGifBytes, "small.gif")))
			using (var convertedFile = Image.FromFile(file.Filename))
			{
				AssertEquals(System.Drawing.Imaging.ImageFormat.Tiff, convertedFile.RawFormat);
			}
		}

		public void TestConvertFileToTiffWithPDFFileWithByteArray()
		{
			var existingFile = resourceRetriever.Value.GetBytes("Enterprise.DocumentScanning.Business.Test.TestDocs.Sample.PDF");
			ConvertFileToTiffWithInvalidImageFile(existingFile, "Sample.pdf");
		}

		public void TestConvertFileToTiffWithXLSFileWithByteArray()
		{
			var existingFile = resourceRetriever.Value.GetBytes("Enterprise.DocumentScanning.Business.Test.TestDocs.Test.xls");
			ConvertFileToTiffWithInvalidImageFile(existingFile, "Test.xls");
		}

		void ConvertFileToTiffWithInvalidImageFile(byte[] fileContents, string fileName)
		{
			var exceptionThrown = false;

			try
			{
				DocumentUtilities.ConvertFileToTiff(fileContents, fileName);
			}
			catch (Exception)
			{
				exceptionThrown = true;
			}

			Assert("exception thrown when trying to convert non image file to an Image", exceptionThrown);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestConvertFileToTiffForMultipageFilesWithByteArray()
		{
			var existingFile = Path.Combine(BaseSourcePath, TestDocsHelper.TestDocsPath, @"lowres_5pages.tif");
			var contents = DocumentUtilities.GetFileAsBytes(existingFile);

			using (var file = SaveBytesToFile(DocumentUtilities.ConvertFileToTiff(contents, Path.GetFileName(existingFile))))
			using (var convertedImage = Image.FromFile(file.Filename))
			{
				var pageCount = convertedImage.GetFrameCount(new FrameDimension(convertedImage.FrameDimensionsList[0]));

				AssertEquals("converted image should have all 5 pages", 5, pageCount);
				AssertEquals("Image should have TIF encoding", ImageFormat.Tiff, convertedImage.RawFormat);
			}

			using (var file = SaveBytesToFile(DocumentUtilities.ConvertFileToTiff(SmallGifBytes, "small.gif")))
			using (var convertedImage = Image.FromFile(file.Filename))
			{
				var pageCount = convertedImage.GetFrameCount(new FrameDimension(convertedImage.FrameDimensionsList[0]));

				AssertEquals("Converted image has one page", 1, pageCount);
				AssertEquals("Image should have TIF encoding", ImageFormat.Tiff, convertedImage.RawFormat);
			}
		}

		TempFile SaveBytesToFile(byte[] contents)
		{
			var file = TempFile.New();

			using (var stream = new FileStream(file.Filename, FileMode.OpenOrCreate))
			{
				stream.Write(contents, 0, contents.Length);
			}

			return file;
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAppendMultiPageImageToFile()
		{
			var outputFile = DocumentUtilities.GetTempFilename();
			File.Copy(Path.Combine(BaseSourcePath, TestDocsHelper.TestDocsPath, @"small_3pages.tif"), outputFile, true);
			File.SetAttributes(outputFile, System.IO.FileAttributes.Normal);

			try
			{
				var imageFile = Path.Combine(BaseSourcePath, TestDocsHelper.TestDocsPath, @"small_2pages.tif");
				DocumentUtilities.AppendMultiPageImageToFile(imageFile, outputFile);

				using (var reader2 = new ImageFileReaderWithLock(outputFile))
				{
					AssertEquals("Num pages", 5, reader2.PageSelector.TotalPages);
				}
			}
			finally
			{
				File.Delete(outputFile);
			}
		}

		public void TestRemoveIllegalCharacters()
		{
			var pathWithIllegalCharacters = @"he*ll\o?@""there";

			AssertEquals("Illegal characters should be removed", "hello@there", DocumentUtilities.RemoveIllegalCharacters(pathWithIllegalCharacters));

			var pathWithoutIllegalCharacters = "hello there";

			AssertEquals("Path should be untouched", "hello there", DocumentUtilities.RemoveIllegalCharacters(pathWithoutIllegalCharacters));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCopySelectedPages()
		{
			var outputFile = DocumentUtilities.GetTempFilename();
			var sourceFile = Path.Combine(BaseSourcePath, TestDocsHelper.TestDocsPath, @"ABCDEF012345.pdf");

			AssertExceptionThrown<CorruptedDocumentException>(() => DocumentUtilities.CopySelectedPages(sourceFile, outputFile, new[] { 1 }));
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

		string SmallGifPath
		{
			get
			{
				if (string.IsNullOrEmpty(smallGifPath))
				{
					smallGifPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentScanning.Business.Test.TestDocs.small.gif");
				}
				return smallGifPath;
			}
		}
		string smallGifPath;

		byte[] SmallTifBytes => resourceRetriever.Value.GetBytes("Enterprise.DocumentScanning.Business.Test.TestDocs.small.tif");

		byte[] SmallGifBytes => resourceRetriever.Value.GetBytes("Enterprise.DocumentScanning.Business.Test.TestDocs.small.gif");
	}
}
