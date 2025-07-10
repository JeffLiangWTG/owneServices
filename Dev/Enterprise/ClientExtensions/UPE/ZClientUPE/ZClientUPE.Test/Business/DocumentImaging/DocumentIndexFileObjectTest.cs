using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.UPE.Testing;
using Enterprise.Environment;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Client.UPE.DocumentImaging.Testing
{
	sealed class DocumentIndexFileObjectTest : TestCaseWithDummy
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImageFileNames()
		{
			AssertEquals("There should be 2 filenames", 2, IndexFileObject.ImageFiles.Count);
			AssertEquals("W41G0RYZ.001", IndexFileObject.ImageFiles[0].Name);
			AssertEquals("W41G0RYZ.002", IndexFileObject.ImageFiles[1].Name);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestHouseBill()
		{
			AssertEquals("HouseBill", "M1302370459", IndexFileObject.HouseBill);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDocumentType()
		{
			TestHelper.SetValidRegistryDocumentImageTypes();
			AssertEquals("Should load the correct DocumentType", IndexFileObject.UPSDocTypeCode, IndexFileObject.DocumentType.UPSCode);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDocumentType_IfNotFoundFallbackToCatchAll()
		{
			AssertEquals("Should return the catch-all DocumentType if the document's UPS code is not registered", "", IndexFileObject.DocumentType.UPSCode);
			AssertEquals("Should return the catch-all DocumentType if the document's UPS code is not registered", "MSC", IndexFileObject.DocumentType.DocTypeCode);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestOldIndexFile()
		{
			IndexFile.CreationTime = ZDateTime.Now.AddMonths(-2).ToDateTime();
			IndexFile.LastWriteTime = ZDateTime.Now.AddMonths(-2).ToDateTime();
			indexFileObject = null;
			Assert("Index file is old...", IndexFileObject.IsIndexFileOld);

			IndexFile.CreationTime = ZDateTime.Now.AddDays(-10).ToDateTime();
			IndexFile.LastWriteTime = ZDateTime.Now.AddDays(-10).ToDateTime();
			indexFileObject = null;
			Assert("Index file is not old...", !IndexFileObject.IsIndexFileOld);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestFirstImage()
		{
			AssertEquals("First image", Env.TempPath + "W41G0RYZ.tif", IndexFileObject.FirstTiffImageFilename);
			AssertNotNull("First image tiff", IndexFileObject.FirstImageBinary);
		}

		protected override void TearDown()
		{
			UPETestHelper.DeleteFiles(Env.TempPath);
			base.TearDown();
		}

		NotificationBuffer EmailedNotifications
		{
			get { return emailedNotifications ?? (emailedNotifications = new NotificationBuffer()); }
		}
		NotificationBuffer emailedNotifications;

		UPETestHelper TestHelper
		{
			get { return testHelper ?? (testHelper = new UPETestHelper()); }
		}
		UPETestHelper testHelper;

		FileInfo IndexFile
		{
			get
			{
				if (indexFile == null)
				{
					foreach (string testFileName in Directory.GetFiles(UPETestHelper.TestFiles.DocumentImaging.Folder))
					{
						FileInfo testFile = new FileInfo(testFileName);
						if (testFile.Exists)
						{
							FileInfo tempFile = new FileInfo(Path.Combine(Env.TempPath, testFile.Name));
							testFile.CopyTo(tempFile.FullName);
							File.SetAttributes(tempFile.FullName, FileAttributes.Normal);
						}
					}
					indexFile = new FileInfo(Path.Combine(EnvProxy.Instance.TempPath, "W41G0RYZ.000"));
				}
				return indexFile;
			}
		}
		FileInfo indexFile;

		DocumentIndexFileObject IndexFileObject
		{
			get { return indexFileObject ?? (indexFileObject = new DocumentIndexFileObject(IndexFile, TestHelper.Buffer, EmailedNotifications)); }
		}
		DocumentIndexFileObject indexFileObject;
	}
}
