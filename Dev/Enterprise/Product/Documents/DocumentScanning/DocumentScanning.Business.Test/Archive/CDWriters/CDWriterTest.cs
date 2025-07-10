using System;
using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.Business.Test
{
	public abstract class CDWriterTest : NonPersistentBusinessObjectTestCase
	{
		public abstract CDWriter Writer { get; }

		public void TestArchiveManager()
		{
			AssertEquals("ArchiveManager should be the same instance", ArchiveManager, Writer.ArchiveManager);
		}

		[SnailTest()]
		public abstract void TestCurrentDrive();

		[SnailTest()]
		public abstract void TestDriveList();

		[SnailTest()]
		public abstract void TestWriteSpeed();

		[SnailTest()]
		public abstract void TestWriteSpeedList();

		public void TestPercentage()
		{
			AssertEquals("Percent 0 by default", 0, Writer.Percent);
			Writer.Percent = 50;
			AssertEquals("Percent 50 after set", 50, Writer.Percent);
		}

		public void TestPhase()
		{
			Assert("CurrentPhase blank by default", Writer.CurrentPhase.IsEmpty);
			Writer.CurrentPhase = "Initialisation";
			AssertEquals("CurrentPhase updated", "Initialisation", Writer.CurrentPhase);
		}

		public void TestSessionLog()
		{
			Assert("SessionLog blank by default", Writer.SessionLog.IsEmpty);
			Writer.SessionLog = "Some log lines ";
			AssertEquals("SessionLog updated", "Some log lines ", Writer.SessionLog);
		}

		public void TestSessionLogInfo()
		{
			Assert("Session log always readonly to user", Writer.SessionLogInfo.ReadOnly);
		}

		public void TestCDVolumeLabel()
		{
			Assert("CDVolume Label blank by default", Writer.CDVolumeLabel.IsEmpty);
			Writer.CDVolumeLabel = "Test";
			AssertEquals("CDVolumeLabel updated", "Test", Writer.CDVolumeLabel);
		}

		public void TestCreateFileNameTruncates()
		{
			OrgHeader org = MasterFactory.LoadTop1<OrgHeader>(new ZQuery());
			StorageMain main = MasterFactory.New<StorageMain>();
			main.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			main.SM_ParentFK = org.PK;

			String char100 = new String('a', 100);
			StorageDocsBase document = main.Documents.AddNew();
			document.SC_FileName = char100 + char100;
			AssertEquals("A_[] aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa.TIF", Writer.CreateFileName(65, ZString.Empty, char100 + char100, document));
			Assert(Writer.CreateFileName(65, ZString.Empty, char100 + char100, document).Length + 200 < 256);
			AssertEquals("A_[] aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa.TIF", Writer.CreateFileName(65, ZString.Empty, char100, document));
			Assert(Writer.CreateFileName(65, ZString.Empty, char100, document).Length + 100 < 256);
			document.SC_FileName = char100;
			AssertEquals("A_[] aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa.TIF", Writer.CreateFileName(65, ZString.Empty, char100 + char100, document));
			Assert(Writer.CreateFileName(65, ZString.Empty, char100 + char100, document).Length + 200 < 256);
		}

		public abstract void TestBurn();

		public void TestSaveFilesToFilesystemAndCreateIndex()
		{
			OrgHeader org = MasterFactory.LoadTop1<OrgHeader>(new ZQuery());
			StorageMain main = MasterFactory.New<StorageMain>();
			main.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			main.SM_ParentFK = org.PK;

			for (int i = 0; i < 40; i++) // 40 files - tests the alphabetic prefixes don't double up, and add some unpublished ones
			{
				StorageDocsBase document;
				if (i % 2 == 0)
				{
					document = main.Documents.AddNew();
				}
				else
				{
					document = main.Files.AddNew();
				}

				document.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
				document.SC_Desc = "ThisIsAllTheSameDescription";
				document.SC_IsPublished = (i < 30);

				if (i % 4 == 0)
				{
					document.SC_DocType = "*99";
					document.SC_Desc = "Invalid DocType";
				}
			}

			ArchiveManager.ListToArchive.Add(main);
			Writer.SaveFilesToFilesystemAndCreateIndex();

			string newDirectoryName = Path.Combine(Writer.CDArchiveTempDirectory, "Organization " + org.OH_Code);
			Assert("New directory should exist in the CD Writing directory", Directory.Exists(newDirectoryName));
			AssertEquals("Directory should contain 30 files", 30, Directory.GetFiles(newDirectoryName).Length);
			AssertEquals(8, Directory.GetFiles(newDirectoryName).Count(x => x.Contains("_[_99] Invalid DocType")));

			Directory.Delete(Writer.CDArchiveTempDirectory, true);
		}

		public void TestSaveFilesToFilesystemAndCreateIndexWithPDFAndTIF()
		{
			OrgHeader org = MasterFactory.LoadTop1<OrgHeader>(new ZQuery());
			StorageMain main = MasterFactory.New<StorageMain>();
			main.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			main.SM_ParentFK = org.PK;

			StorageDocsBase doc = main.Documents.AddNew();
			doc.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
			doc.SC_Desc = "Hello";
			doc.SC_IsPublished = true;

			StorageDocsBase file = main.Files.AddNew();
			file.SC_FileName = "Sample";
			file.SC_DataType = "PDF";
			file.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
			file.SC_Desc = "A misc document";
			file.SC_IsPublished = true;

			ArchiveManager.ListToArchive.Add(main);
			Writer.SaveFilesToFilesystemAndCreateIndex();

			string newDirectoryName = Path.Combine(Writer.CDArchiveTempDirectory, "Organization " + org.OH_Code);
			Assert("New directory should exist in the CD Writing directory", Directory.Exists(newDirectoryName));

			string[] filesInDirectory = Directory.GetFiles(newDirectoryName);
			AssertEquals("Directory should contain 2 files (one tif, one pdf)", 2, filesInDirectory.Length);

			AssertEquals("File should have the correct extension", ".PDF", Path.GetExtension(filesInDirectory[0]).ToUpper());
			AssertEquals("File should contain the filename", true, Path.GetFileNameWithoutExtension(filesInDirectory[0]).IndexOf("Sample") > -1);

			AssertEquals("File should have the correct extension", ".TIF", Path.GetExtension(filesInDirectory[1]).ToUpper());
			AssertEquals("File should contain the description", true, Path.GetFileNameWithoutExtension(filesInDirectory[1]).IndexOf("Hello") > -1);

			Directory.Delete(Writer.CDArchiveTempDirectory, true);
		}

		public void TestCDArchiveTempDirectory()
		{
			Assert("CD Archive Temp Directory should ALWAYS be in the enterprise temp directory", Writer.CDArchiveTempDirectory.Contains(Temp.TempPath));
		}

		public void TestSaveFilesToFilesystemAndCreateIndex_WithIllegalCharacter_Issue00904430()
		{
			OrgHeader org = MasterFactory.LoadTop1<OrgHeader>(new ZQuery());
			StorageMain storageMain = MasterFactory.New<StorageMain>();
			storageMain.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			storageMain.SM_ParentFK = org.PK;
			storageMain.DocumentOwner[CodePropertyAttribute.CodePropertyNameFromType(storageMain.DocumentOwner.GetType())] = @"` ~ ^ | < >";
			AssertEquals("Precodition:", @"Organization ` ~ ^ | < >", storageMain.DocumentOwnerDescription);

			StorageDocsBase document = storageMain.Files.AddNew();
			document.SC_Desc = "RandomDocument";
			document.SC_IsPublished = true;
			ArchiveManager.ListToArchive.Add(storageMain);

			AssertNoExceptionThrown(() => Writer.SaveFilesToFilesystemAndCreateIndex());
			Directory.Delete(Writer.CDArchiveTempDirectory, true);
		}

		protected ArchiveEDocsManager ArchiveManager
		{
			get { return fArchiveManager; }
		}

		protected DocumentFactory MasterFactory
		{
			get { return fMasterFactory; }
		}

		protected override void SetUp()
		{
			base.SetUp();
			fMasterFactory = new DocumentFactoryProvider().GetFactory(Factory);
			fArchiveManager = new ArchiveEDocsManager(MasterFactory);
		}

		DocumentFactory fMasterFactory;
		ArchiveEDocsManager fArchiveManager;
	}
}
