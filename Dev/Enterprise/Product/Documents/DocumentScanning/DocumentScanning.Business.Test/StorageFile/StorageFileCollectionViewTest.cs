using System;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DocumentScanning.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.Business.Test
{
	[TestedType(typeof(StorageFileCollectionView))]
	sealed class StorageFileCollectionViewTest : BusinessObjectCollectionViewTestCase<StorageFileCollectionView>
	{
		protected override BusinessObjectFactory NewFactory()
		{
			return new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
		}

		DocumentFactory MasterFactory
		{
			get { return (DocumentFactory)Factory; }
		}

		protected override StorageFileCollectionView GetCollectionToTest()
		{
			StorageMain main = MasterFactory.New<StorageMain>();
			SetupFiles();
			return main.Files;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			StorageFile file = StorageFile.New_DEBUG(MasterFactory);
			file.SC_DataType = "PDF";
			return file;
		}

		public void TestAddOrUpdateFromFilenames_AddingDelayedUntilFileConfigured()
		{
			var samplePDFFilePath = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentScanning.Business.Test.TestDocs.Sample.PDF");
			var storageFileCollection = Parent.Files;
			storageFileCollection.CountChanged += (sender, e) =>
			{
				StorageDocsTestHelper.AssertFileOrDocumentConfigured((BusinessObjectCollection)sender, Core.Constants.FileFormats.PDF);
			};
			storageFileCollection.AddOrUpdateFromFilenames(FileAction.CreateNew, new[] { samplePDFFilePath });
		}

		public void TestIsThisPartOfTheCollection()
		{
			SetupFiles();
			File1.SC_IsDeleted = true;
			File2.SC_IsDeleted = true;

			AssertEquals("CollectionView should have 3 elements when excluding deleted docs by default", 3, CollectionView.Count);

			CollectionView.IncludeDeletedDocuments = true;
			AssertEquals("Collection should have all elements when including deleted docs", 5, CollectionView.Count);

			CollectionView.IncludeDeletedDocuments = false;
			Assert("Deleted doc - not part of collection", !CollectionView.IsThisPartOfTheCollectionExposed(File1));
			Assert("Deleted doc - not part of collection", !CollectionView.IsThisPartOfTheCollectionExposed(File2));
			Assert("Not deleted - part of collection", CollectionView.IsThisPartOfTheCollectionExposed(File3));
			Assert("Not deleted - part of collection", CollectionView.IsThisPartOfTheCollectionExposed(File4));
			Assert("Not deleted - part of collection", CollectionView.IsThisPartOfTheCollectionExposed(File5));

			File2.SC_IsDeleted = false;
			File2.SC_DataType = "TIF";
			AssertEquals("CollectionView should have 4 elements - its still storagefile regardless of SC_DataType", 4, CollectionView.Count);

			Assert("Deleted doc - not part of collection", !CollectionView.IsThisPartOfTheCollectionExposed(File1));
			Assert("TIF storagefile - part of collection", CollectionView.IsThisPartOfTheCollectionExposed(File2));
			Assert("Not deleted - part of collection", CollectionView.IsThisPartOfTheCollectionExposed(File3));
			Assert("Not deleted - part of collection", CollectionView.IsThisPartOfTheCollectionExposed(File4));
			Assert("Not deleted - part of collection", CollectionView.IsThisPartOfTheCollectionExposed(File5));

			int count = Parent.Files.Count;
			StorageFile newFile = Parent.Files.AddNew();
			newFile.SC_DataType = "txt";
			newFile.SC_IsDeleted = false;
			newFile.SC_IsPublished = true;
			CollectionView.Rebuild();
			AssertEquals(count + 1, CollectionView.Count);

			CollectionView.SC_DataTypeFilter = "TXT";
			CollectionView.Rebuild();
			AssertEquals(1, CollectionView.Count);
		}

		public void TestIsThisPartOfTheCollectionForImage()
		{
			StorageDocs doc = Factory.New<StorageDocs>();
			StorageFile file = Factory.New<StorageFile>();
			SetupFiles();
			doc.SC_DataType = "XLS";
			file.SC_DataType = "QQQ";
			Assert("should return false for storagedoc with any datatype - its always image", !CollectionView.IsThisPartOfTheCollectionExposed(doc));
			Assert("should return true for storagefile with any datatype - its not image always", CollectionView.IsThisPartOfTheCollectionExposed(file));
		}

		public void TestIncludeDeletedDocuments()
		{
			SetupFiles();
			AssertEquals("Collection count should be all elements", 5, CollectionView.Count);
			File1.SC_IsDeleted = true;
			File2.SC_IsDeleted = true;

			AssertEquals("Collection view count is two less than collection count", 3, CollectionView.Count);

			CollectionView.IncludeDeletedDocuments = true;
			AssertEquals("Collection count should be all elements", 5, CollectionView.Count);
		}

		public void TestExcludeUnpublishedDocuments()
		{
			SetupFiles();
			AssertEquals("Collection count should be all elements", 5, CollectionView.Count);

			File1.SC_IsPublished = false;
			File2.SC_IsPublished = false;
			File3.SC_IsPublished = true;
			File4.SC_IsPublished = true;
			File5.SC_IsPublished = true;

			AssertEquals("Collection count includes all - exclude unpublished should be false by default", 5, CollectionView.Count);

			CollectionView.ExcludeUnpublishedDocuments = true;
			AssertEquals("Collection count is two less", 3, CollectionView.Count);

			CollectionView.ExcludeUnpublishedDocuments = false;
			AssertEquals("Collection count includes all", 5, CollectionView.Count);
		}

		public void TestExcludeUnpublishedAndDeletedDocuments()
		{
			SetupFiles();
			File1.SC_IsPublished = false;
			File1.SC_IsDeleted = true;
			File2.SC_IsPublished = false;
			File3.SC_IsPublished = true;
			File4.SC_IsPublished = true;
			File5.SC_IsPublished = true;
			File5.SC_IsDeleted = true;

			AssertEquals("Collection view count should be 3 - not including deleted docs by default", 3, CollectionView.Count);

			CollectionView.ExcludeUnpublishedDocuments = true;
			AssertEquals("Collection view count is 2 - not including deleted docs or unpublished docs", 2, CollectionView.Count);

			CollectionView.IncludeDeletedDocuments = true;
			AssertEquals("Collection view count is 3 - now including deleted docs", 3, CollectionView.Count);

			CollectionView.ExcludeUnpublishedDocuments = false;
			AssertEquals("Collection view count should equals collection count", 5, CollectionView.Count);
		}

		public void TestAddNewDefaultValues()
		{
			Parent.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			Parent.SM_ParentFK = Org.PK;
			AssertEquals("Files count 0", 0, Parent.Files.Count);

			StorageFile file = Parent.Files.AddNew();

			AssertEquals("Files count 1", 1, Parent.Files.Count);
			Assert("Files date is not empty", !Parent.Files[0].SC_Date.IsEmpty);
		}

		public void TestMasterFactory()
		{
			AssertEquals("Master factory object should be same as that passed in to parent", MasterFactory, Parent.Files.MasterFactory);
		}

		public void TestAdditionalFilter()
		{
			Parent.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			Parent.SM_ParentFK = Org.PK;

			var factoryOne = MasterFactory.GetFactory(1);
			var file = StorageFile.New_DEBUG(factoryOne);
			file.SC_SM = Parent.PK;

			AssertEquals("Files Count should be 0, the File added doesn't have a filename which is mandatory for a file", 0, Parent.Files.Count);

			file.SC_DataType = "WEBP";

			Parent.Files.CollectionToFilter.Load();
			AssertEquals("FileCount should be 1, the file has a datatype other than supported image formats now so it is included in the collection", 1, Parent.Files.Count);
		}

		public void TestAddOrUpdateFromFilenames()
		{
			var filenameToAdd1 = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentScanning.Business.Test.TestDocs.Sample.PDF", "Sample.pdf");

			AssertEquals("Precondition: no files in collection", 0, Parent.Files.Count);
			StorageFile[] filesAdded = Parent.Files.AddOrUpdateFromFilenames(FileAction.Overwrite, filenameToAdd1, SmallGifPath);

			AssertEquals("Should now be two files in collection", 2, Parent.Files.Count);
			AssertEquals("First file should have data type", "PDF", filesAdded[0].SC_DataType);
			AssertEquals("First file should have filename", "Sample", filesAdded[0].SC_FileName);
			Assert("First file should have image data set", filesAdded[0].SC_ImageData.Length > 0);

			AssertEquals("Second file should have data type", "GIF", filesAdded[1].SC_DataType);
			AssertEquals("Second file should have filename", "small", filesAdded[1].SC_FileName);
			Assert("Second file should have image data set", filesAdded[1].SC_ImageData.Length > 0);

			Parent.Files.AddOrUpdateFromFilenames(FileAction.Overwrite, filenameToAdd1);
			AssertEquals("Should still be two files in collection; having the same name will not add a new one, only update existing.", 2, Parent.Files.Count);

			StorageFile file = Parent.Files[0];
			file.SC_ImageData = new byte[] { 1, 2, 3 };
			int originalBlobLength = file.SC_ImageData.Length;
			Parent.Files.AskForOverwriteOrCreateNew += new OverwriteOrCreateNewEventHandler(Files_AskForOverwriteOrCreateNewReturnOverwrite);
			Parent.Files.AddOrUpdateFromFilenames(FileAction.AskForUserInput, filenameToAdd1);
			Parent.Files.AskForOverwriteOrCreateNew -= new OverwriteOrCreateNewEventHandler(Files_AskForOverwriteOrCreateNewReturnOverwrite);
			Assert("Blob lengths should be different because data should be different after overwrite", originalBlobLength != file.SC_ImageData.Length);

			Parent.Files.AskForOverwriteOrCreateNew += new OverwriteOrCreateNewEventHandler(Files_AskForOverwriteOrCreateNewReturnCreateNew);
			Parent.Files.AddOrUpdateFromFilenames(FileAction.AskForUserInput, filenameToAdd1);
			Parent.Files.AskForOverwriteOrCreateNew -= new OverwriteOrCreateNewEventHandler(Files_AskForOverwriteOrCreateNewReturnCreateNew);
			AssertEquals("Should now have three files in the files collection, because we selected to create a new file", 3, Parent.Files.Count);
			AssertEquals("The third file should have a name with [2] appended", "Sample[2].pdf", Parent.Files[2].SC_FileNameWithExtension);

			Parent.Files[0].SC_IsPublished = false;
			Parent.Files[1].SC_IsPublished = true;
			Parent.Files[2].SC_IsPublished = true;
			Parent.Files.ExcludeUnpublishedDocuments = true;
			Parent.Files.AddOrUpdateFromFilenames(FileAction.Overwrite, filenameToAdd1);
			Parent.Files.ExcludeUnpublishedDocuments = false;
			AssertEquals("Should now have four files in the files collection, because instead of overwrite file we cannot access we should add a new file", 4, Parent.Files.Count);
			AssertEquals("The fourth file should have a name with [3] appended", "Sample[3].pdf", Parent.Files[3].SC_FileNameWithExtension);
		}

		void Files_AskForOverwriteOrCreateNewReturnOverwrite(object sender, FilenameEventArgs args)
		{
			args.Action = FileAction.Overwrite;
		}

		void Files_AskForOverwriteOrCreateNewReturnCreateNew(object sender, FilenameEventArgs args)
		{
			args.Action = FileAction.CreateNew;
		}

		public void TestAddOrUpdateFromFilenamesWithOpenFiles()
		{
			var filename = SmallGifPath;
			using (var stream = File.OpenWrite(filename))
			{
				AssertEquals("Precondition: no files in collection", 0, Parent.Files.Count);

				try
				{
					Parent.Files.AddOrUpdateFromFilenames(FileAction.CreateNew, filename);
				}
				catch (FileAccessException)
				{
				}

				AssertEquals("File shouldn't have been added if the file was open - file count should still be 0. " +
					"Check that you are trying to read the blob contents *before* calling AddNew() on the collection.", 0, Parent.Files.Count);
			}
		}

		public void TestAddOrUpdateFromFilenamesWithLongNamesAndDuplicates()
		{
			string filename = Path.Combine(Temp.TempPath, new string('x', 259 - Temp.TempPath.Length));

			try
			{
				File.WriteAllText(filename, "");

				AssertEquals("Precondition: no files in collection", 0, Parent.Files.Count);
				Parent.Files.AddOrUpdateFromFilenames(FileAction.CreateNew, filename);
				AssertEquals("Should now have 1 file in collection", 1, Parent.Files.Count);

				for (int i = 2; i < 10; i++)
				{
					Parent.Files.AddOrUpdateFromFilenames(FileAction.CreateNew, filename);
					AssertEquals("Number of files that should be in collection", i, Parent.Files.Count);
					Assert("File's name should be truncated AND have a number appended", Parent.Files[i - 1].SC_FileName.EndsWith("[" + i + "]"));
				}

				Parent.Files.AddOrUpdateFromFilenames(FileAction.CreateNew, filename);
				AssertEquals("Should now have 10 files in collection", 10, Parent.Files.Count);
				Assert("Filename should be truncated and have a number appended", Parent.Files[9].SC_FileName.EndsWith("[10]"));
			}
			finally
			{
				if (File.Exists(filename))
				{
					File.Delete(filename);
				}
			}
		}

		public void TestAddOrUpdateFromFilenamesWithSpacesAtTheEnd()
		{
			string nameWithSpace = "A file with a space at the end ";
			string filename = Path.Combine(Temp.TempPath, nameWithSpace + ".txt");

			try
			{
				File.WriteAllText(filename, "");

				AssertEquals("Precondition: no files in collection", 0, Parent.Files.Count);
				Parent.Files.AddOrUpdateFromFilenames(FileAction.CreateNew, filename);
				AssertEquals("Should now have 1 file in collection", 1, Parent.Files.Count);
				AssertEquals("File's name should be trimmed", nameWithSpace.Trim(), Parent.Files[0].SC_FileName);

				Parent.Files.AddOrUpdateFromFilenames(FileAction.CreateNew, filename);
				AssertEquals("Should now have two files in collection", 2, Parent.Files.Count);
				AssertEquals("File's name should be trimmed with a number appended", nameWithSpace.Trim() + "[2]", Parent.Files[1].SC_FileName);
			}
			finally
			{
				if (File.Exists(filename))
				{
					File.Delete(filename);
				}
			}
		}

		public void TestAddOrUpdatefromFilenamesWithFileActionOverride()
		{
			var filename = SmallGifPath;
			Parent.Files.AddOrUpdateFromFilenames(FileAction.CreateNew, filename);
			AssertEquals("File count, should have created a file by default because it is a new file without a duplicate name existing", 1, Parent.Files.Count);

			Parent.Files.AddOrUpdateFromFilenames(FileAction.CreateNew, filename);
			AssertEquals("File count should be 2, new file should have been created", 2, Parent.Files.Count);

			Parent.Files.AddOrUpdateFromFilenames(FileAction.Overwrite, filename);
			AssertEquals("File count should still be 2, the original file should have been overwritten", 2, Parent.Files.Count);
		}

		public void TestAllowNew()
		{
			AssertEquals("Collection should not allow new", false, Parent.Files.AllowNew);
		}

		public void TestFilesOpen()
		{
			using (StorageFile file1 = Parent.Files.AddNew())
			{
				file1.SC_FileName = "small";
				file1.SC_DataType = "GIF";
				file1.SC_ImageData = resourceRetriever.Value.GetBytes("Enterprise.DocumentScanning.Business.Test.TestDocs.small.gif");
				file1.SaveToTempFile();

				using (StorageFile file2 = Parent.Files.AddNew())
				{
					file2.SC_FileName = "Sample";
					file2.SC_DataType = "PDF";
					file2.SC_ImageData = resourceRetriever.Value.GetBytes("Enterprise.DocumentScanning.Business.Test.TestDocs.Sample.PDF");
					file2.SaveToTempFile();

					AssertEquals("Should have no files open", 0, Parent.Files.FilesOpen.Length);

					using (FileStream stream1 = File.OpenRead(file1.TempFileName))
					{
						AssertEquals("Should have 1 file open", 1, Parent.Files.FilesOpen.Length);

						using (FileStream stream2 = File.OpenRead(file2.TempFileName))
						{
							AssertEquals("Should have 2 files open", 2, Parent.Files.FilesOpen.Length);
						}

						AssertEquals("should have 1 file open", 1, Parent.Files.FilesOpen.Length);
					}
					AssertEquals("Shoud have no files open", 0, Parent.Files.FilesOpen.Length);
				}
			}
		}

		public void TestFilenameExists()
		{
			StorageFile file1 = Parent.Files.AddNew();
			file1.SC_FileName = "hello";
			file1.SC_DataType = "DOC";

			AssertEquals("file already exists with hello.doc", true, Parent.Files.FilenameExists("hello.doc"));
			AssertEquals("file does not exist as goodbye.doc", false, Parent.Files.FilenameExists("goodbye.doc"));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[ExpectNoExceptions()]
		public void TestSC_FilenameTruncated()
		{
			ZString filenameOnly = "AReallyLongNameThatExtendsPastTheLimitOfSC_FilenameOnTheStorageDocsTableSoItMightCauseASilentExceptionAboutMaxlengthExceededIfYouTryToAssignSomethingToIt";
			ZString fileWithReallyLongName = Path.Combine(Temp.TempPath, filenameOnly + ".PDF");

			try
			{
				File.Copy(Path.Combine(BaseSourcePath, TestDocsHelper.TestDocsPath, @"Sample.PDF"), fileWithReallyLongName, true);
				StorageFile[] addedFiles = Parent.Files.AddOrUpdateFromFilenames(FileAction.CreateNew, fileWithReallyLongName);
				AssertEquals("One file should have been added", 1, addedFiles.Length);
				AssertEquals("File name should have been truncated at the maxlength", filenameOnly.SubstringSafe(0, StorageDocsSchema.SC_FileName.MaxLength), addedFiles[0].SC_FileName);
			}
			finally
			{
				if (File.Exists(fileWithReallyLongName))
				{
					File.SetAttributes(fileWithReallyLongName, FileAttributes.Normal);
					File.Delete(fileWithReallyLongName);
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[ExpectNoExceptions()]
		public void TestSC_DataTypeTruncated()
		{
			ZString longExtension = "WithAReallyLongExtensionThatGoesPastTheMaxLengthOfSC_DataType";
			ZString fileWithReallyLongName = Path.Combine(Temp.TempPath, "AnOrdinaryFilename." + longExtension);

			try
			{
				File.Copy(Path.Combine(BaseSourcePath, TestDocsHelper.TestDocsPath, @"Sample.PDF"), fileWithReallyLongName, true);
				StorageFile[] addedFiles = Parent.Files.AddOrUpdateFromFilenames(FileAction.CreateNew, fileWithReallyLongName);
				AssertEquals("One file should have been added", 1, addedFiles.Length);
				AssertEquals("File data type should have been truncated at the maxlength", longExtension.SubstringSafe(0, StorageDocsSchema.SC_DataType.MaxLength).ToUpper(), addedFiles[0].SC_DataType);
			}
			finally
			{
				if (File.Exists(fileWithReallyLongName))
				{
					File.SetAttributes(fileWithReallyLongName, FileAttributes.Normal);
					File.Delete(fileWithReallyLongName);
				}
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			(new DocManagerDBHelper()).LastWritableDatabaseWithFreeSpace(); //to ensure SD001 exists
			TestCaseHelper.ClearTable(StorageDocsSchema.Constants.TableName);
			TestCaseHelper.ClearTable(StorageMainSchema.Constants.TableName);
			TestCaseHelper.ClearTable((new DocManagerDBHelperTestClass()).GetDatabaseName(1) + ".dbo." + StorageDocsSchema.Constants.TableName);
			SetupParent();
			Org = MasterFactory.LoadTop1<OrgHeader>(new ZQuery());
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
					smallGifPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentScanning.Business.Test.TestDocs.small.gif", "small.gif");
				}
				return smallGifPath;
			}
		}
		string smallGifPath;

		void SetupParent()
		{
			Parent = MasterFactory.New<StorageMain>();
			Parent.SM_DB = 1;
		}

		void SetupFiles()
		{
			if (Parent == null)
			{
				SetupParent(); // needed for reflection test
			}

			CollectionView = Parent.Files;
			File1 = Parent.Files.AddNew();
			File1.SC_DataType = "PDF";
			File2 = Parent.Files.AddNew();
			File2.SC_DataType = "PDF";
			File3 = Parent.Files.AddNew();
			File3.SC_DataType = "PDF";
			File4 = Parent.Files.AddNew();
			File4.SC_DataType = "PDF";
			File5 = Parent.Files.AddNew();
			File5.SC_DataType = "PDF";
		}

		StorageFileCollectionView CollectionView;
		StorageFile File1;
		StorageFile File2;
		StorageFile File3;
		StorageFile File4;
		StorageFile File5;

		StorageMain Parent;
		OrgHeader Org;
	}
}
