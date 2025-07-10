using System;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DocumentScanning.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.Business.Test
{
	sealed class SerializableEDocTest : TestCaseWithDocumentFactory
	{
		public void TestToBusinessObjectIfEmpty()
		{
			var docToCopy = StorageDocs.NewWithParent_DEBUG(MasterFactory);
			var dataObject = new SerializableEDoc(docToCopy);
			AssertExceptionThrown(
				typeof(EmptyContentEDocsException),
				"The eDoc content cannot be empty.",
				() => dataObject.ToBusinessObject(MasterFactory.New<StorageMain>()));
		}

		public void TestToBusinessObjectDocument()
		{
			TestCaseHelper.ClearTable(StorageDocsSchema.Constants.TableName);
			TestCaseHelper.ClearTable(StorageMainSchema.Constants.TableName);
			MasterFactory.Save();
			AssertEquals("Precondition: Should be no StorageDocs objects", 0, MasterFactory.GetDatabaseCount(typeof(StorageDocs)));
			AssertEquals("Precondition: Should be no StorageMain objects", 0, MasterFactory.GetDatabaseCount(typeof(StorageMain)));

			var org = MasterFactory.LoadTop1<OrgHeader>(new ZQuery());

			var parent = MasterFactory.New<StorageMain>();
			parent.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			parent.SM_ParentFK = org.PK;
			parent.SM_DB = 1;

			StorageDocs docToCopy = parent.Documents.AddNew();
			docToCopy.SC_DocType = "ABC";
			docToCopy.SC_Desc = "Description";
			docToCopy.SC_Date = new ZDateTime(2005, 01, 01);
			docToCopy.SC_IsPublished = true;
			docToCopy.SC_IsDeleted = false;
			docToCopy.SC_IsSystemGenerated = true;
			docToCopy.SC_SaveVersions = true;
			docToCopy.SC_ImageData = SmallTif;

			var parentToRestoreTo = MasterFactory.New<StorageMain>();
			var dataObject = new SerializableEDoc(docToCopy);
			StorageDocsBase restoredObject = dataObject.ToBusinessObject(parentToRestoreTo);
			Assert("Should return a StorageDocs object if no filename", restoredObject is StorageDocs);

			StorageDocs doc = (StorageDocs)restoredObject;

			Assert("Restored doc is not the same as the original", docToCopy.PK != doc.PK);
			AssertEquals("doc should have same properties as original - doctype", "ABC", doc.SC_DocType);
			AssertEquals("doc should have same properties as original - desc", "Description", doc.SC_Desc);
			Assert("doc should have NEW property date", doc.SC_Date != docToCopy.SC_Date);
			AssertEquals("doc should have same properties as original - filename", ZString.Empty, doc.SC_FileName);
			AssertEquals("doc should have same properties as original - published", true, doc.SC_IsPublished);
			AssertEquals("doc should have same properties as original - deleted", false, doc.SC_IsDeleted);
			AssertEquals("doc should never be system generated - a doc originating from copy/paste or drag/drop is not original", false, doc.SC_IsSystemGenerated);
			AssertEquals("doc should have same properties as original - save versions", true, doc.SC_SaveVersions);
			AssertEquals("doc should be assigned to the NEW parent", parentToRestoreTo.PK, doc.SC_SM);
			AssertEquals("doc should have same properties as original - Image", docToCopy.SC_ImageData, doc.SC_ImageData);
		}

		public void TestToBusinessObjectFile()
		{
			TestCaseHelper.ClearTable(StorageDocsSchema.Constants.TableName);
			TestCaseHelper.ClearTable(StorageMainSchema.Constants.TableName);
			MasterFactory.Save();
			AssertEquals("Precondition: Should be no StorageDocs objects", 0, MasterFactory.GetDatabaseCount(typeof(StorageDocs)));
			AssertEquals("Precondition: Should be no StorageMain objects", 0, MasterFactory.GetDatabaseCount(typeof(StorageMain)));

			var parent = MasterFactory.New<StorageMain>();
			parent.SM_Type = Core.Constants.DocManagerCodes.Shipment;

			StorageFile fileToCopy = StorageFile.New_DEBUG(MasterFactory);
			fileToCopy.SC_Date = new ZDateTime(2005, 01, 01);
			fileToCopy.SC_FileName = "This is a Filename";
			fileToCopy.SC_DataType = "PDF"; // this is the deterministic property
			fileToCopy.SC_DocType = "ABC";
			fileToCopy.SC_Desc = "description";
			fileToCopy.SC_IsPublished = true;
			fileToCopy.SC_IsDeleted = false;
			fileToCopy.SC_IsSystemGenerated = true;
			fileToCopy.SC_SaveVersions = true;
			fileToCopy.SC_ImageData = SamplePdf;
			fileToCopy.SC_SM = parent.PK;

			var parentToRestoreTo = MasterFactory.New<StorageMain>();
			var dataObject = new SerializableEDoc(fileToCopy);
			StorageDocsBase restoredObject = dataObject.ToBusinessObject(parentToRestoreTo);
			Assert("Should return a storagefile object if there is a filename", restoredObject is StorageFile);

			StorageFile file = (StorageFile)restoredObject;

			Assert("Restored file is not the same as the original", fileToCopy.PK != file.PK);
			AssertEquals("file should have same properties as original - doctype", "ABC", file.SC_DocType);
			AssertEquals("file should have same properties as original - desc", "description", file.SC_Desc);
			Assert("file should have NEW property date", file.SC_Date != fileToCopy.SC_Date);
			AssertEquals("file should have same properties as original - filename", "This is a Filename", file.SC_FileName);
			AssertEquals("file should have same properties as original - published", true, file.SC_IsPublished);
			AssertEquals("file should have same properties as original - deleted", false, file.SC_IsDeleted);
			AssertEquals("file should never be system generated - a doc originating from copy/paste or drag/drop is not original", false, file.SC_IsSystemGenerated);
			AssertEquals("file should have same properties as original - save versions", true, file.SC_SaveVersions);
			AssertEquals("file should be assigned to the NEW parent", parentToRestoreTo.PK, file.SC_SM);
			AssertEquals("file should have same properties as original - DataType", fileToCopy.SC_DataType, file.SC_DataType);
			AssertEquals("file should have same properties as original - Image", fileToCopy.SC_ImageData, file.SC_ImageData);
		}

		public void TestToBusinessObjectDocumentWithParent()
		{
			TestCaseHelper.ClearTable(StorageDocsSchema.Constants.TableName);
			TestCaseHelper.ClearTable(StorageMainSchema.Constants.TableName);
			MasterFactory.Save();
			AssertEquals("Precondition: Should be no StorageDocs objects", 0, MasterFactory.GetDatabaseCount(typeof(StorageDocs)));
			AssertEquals("Precondition: Should be no StorageMain objects", 0, MasterFactory.GetDatabaseCount(typeof(StorageMain)));

			var org = MasterFactory.LoadTop1<OrgHeader>(new ZQuery());

			var parent = MasterFactory.New<StorageMain>();
			parent.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			parent.SM_ParentFK = org.PK;
			parent.SM_DB = 1;

			StorageDocs docToCopy = parent.Documents.AddNew();
			docToCopy.SC_DocType = "ABC";
			docToCopy.SC_Desc = "Description";
			docToCopy.SC_Date = new ZDateTime(2005, 01, 01);
			docToCopy.SC_IsPublished = true;
			docToCopy.SC_IsDeleted = false;
			docToCopy.SC_IsSystemGenerated = true;
			docToCopy.SC_SaveVersions = true;
			docToCopy.SC_ImageData = SmallTif;

			var dataObject = new SerializableEDoc(docToCopy);

			var shipmentPK = ZGuid.NewZGuid();
			var anotherParent = MasterFactory.New<StorageMain>();
			anotherParent.SM_Type = Core.Constants.DocManagerCodes.Shipment;
			anotherParent.SM_ParentFK = shipmentPK;
			anotherParent.SM_DB = 1;

			StorageDocsBase restoredObject = dataObject.ToBusinessObject(anotherParent);
			Assert("Should return a StorageDocs object if data type is default", restoredObject is StorageDocs);

			StorageDocs doc = (StorageDocs)restoredObject;

			Assert("Restored doc is not the same as the original", docToCopy.PK != doc.PK);
			AssertEquals("doc should have same properties as original - doctype", "ABC", doc.SC_DocType);
			AssertEquals("doc should have same properties as original - desc", "Description", doc.SC_Desc);
			Assert("doc should have NEW property date", doc.SC_Date != docToCopy.SC_Date);
			AssertEquals("doc should have same properties as original - filename", ZString.Empty, doc.SC_FileName);
			AssertEquals("doc should have same properties as original - published", true, doc.SC_IsPublished);
			AssertEquals("doc should have same properties as original - deleted", false, doc.SC_IsDeleted);
			AssertEquals("doc should never be system generated - a doc originating from copy/paste or drag/drop is not original", false, doc.SC_IsSystemGenerated);
			AssertEquals("doc should have same properties as original - save versions", true, doc.SC_SaveVersions);
			AssertEquals("doc should have same properties as original - Image", docToCopy.SC_ImageData, doc.SC_ImageData);
			AssertEquals("Doc should have the alternative parent passed in to the ToBusinessObject Method", anotherParent, doc.ParentMain);
		}

		public void TestToBusinessObjectFileWithParent()
		{
			TestCaseHelper.ClearTable(StorageDocsSchema.Constants.TableName);
			TestCaseHelper.ClearTable(StorageMainSchema.Constants.TableName);
			MasterFactory.Save();
			AssertEquals("Precondition: Should be no StorageDocs objects", 0, MasterFactory.GetDatabaseCount(typeof(StorageDocs)));
			AssertEquals("Precondition: Should be no StorageMain objects", 0, MasterFactory.GetDatabaseCount(typeof(StorageMain)));

			var parent = MasterFactory.New<StorageMain>();
			parent.SM_Type = Core.Constants.DocManagerCodes.Shipment;

			StorageFile fileToCopy = StorageFile.NewWithParent_DEBUG(MasterFactory);
			fileToCopy.SC_Date = new ZDateTime(2005, 01, 01);
			fileToCopy.SC_FileName = "This is a Filename";
			fileToCopy.SC_DataType = "XLS"; // this is the deterministic property
			fileToCopy.SC_DocType = "ABC";
			fileToCopy.SC_Desc = "description";
			fileToCopy.SC_IsPublished = true;
			fileToCopy.SC_IsDeleted = false;
			fileToCopy.SC_IsSystemGenerated = true;
			fileToCopy.SC_SaveVersions = true;
			fileToCopy.SC_ImageData = resourceRetriever.Value.GetBytes("Enterprise.DocumentScanning.Business.Test.TestDocs.Test.xls");
			fileToCopy.SC_SM = parent.PK;

			var dataObject = new SerializableEDoc(fileToCopy);

			StorageMain anotherParent = MasterFactory.New<StorageMain>();
			StorageDocsBase restoredObject = dataObject.ToBusinessObject(anotherParent);
			Assert("Should return a StorageFile object if there is a filename", restoredObject is StorageFile);

			StorageFile file = (StorageFile)restoredObject;

			Assert("Restored file is not the same as the original", fileToCopy.PK != file.PK);
			AssertEquals("file should have same properties as original - doctype", "ABC", file.SC_DocType);
			AssertEquals("file should have same properties as original - desc", "description", file.SC_Desc);
			Assert("file should have NEW property date", file.SC_Date != fileToCopy.SC_Date);
			AssertEquals("file should have same properties as original - filename", "This is a Filename", file.SC_FileName);
			AssertEquals("file should have same properties as original - published", true, file.SC_IsPublished);
			AssertEquals("file should have same properties as original - deleted", false, file.SC_IsDeleted);
			AssertEquals("file should never be system generated - a doc originating from copy/paste or drag/drop is not original", false, file.SC_IsSystemGenerated);
			AssertEquals("file should have same properties as original - save versions", true, file.SC_SaveVersions);
			AssertEquals("file should have same properties as original - Image", fileToCopy.SC_ImageData, file.SC_ImageData);
			AssertEquals("file should have same properties as original - DataType", fileToCopy.SC_DataType, file.SC_DataType);
			AssertEquals("file should have the alternative parent passed in to the ToBusinessObject Method", anotherParent, file.ParentMain);
		}

		public void TestToBusinessObjectFileWithParentAndDescription()
		{
			TestCaseHelper.ClearTable(StorageDocsSchema.Constants.TableName);
			TestCaseHelper.ClearTable(StorageMainSchema.Constants.TableName);
			MasterFactory.Save();
			AssertEquals("Precondition: Should be no StorageDocs objects", 0, MasterFactory.GetDatabaseCount(typeof(StorageDocs)));
			AssertEquals("Precondition: Should be no StorageMain objects", 0, MasterFactory.GetDatabaseCount(typeof(StorageMain)));

			var newDocType = MasterFactory.New<RefDocType>();
			newDocType.RT_DocType = "ABC";
			newDocType.RT_ReferenceType = Core.Constants.ReferenceTypes.SupplyChainLogistics;
			newDocType.RT_Desc = "ABC Description";

			var parent = MasterFactory.New<StorageMain>();
			parent.SM_Type = Core.Constants.DocManagerCodes.Shipment;
			StorageFile fileToCopy = parent.Files.AddNew();
			fileToCopy.SC_Date = new ZDateTime(2005, 01, 01);
			fileToCopy.SC_FileName = "This is a Filename"; // this is part of the deterministic properties
			fileToCopy.SC_DataType = "PDF"; // this is part of the deterministic properties
			fileToCopy.SC_DocType = "ABC";
			fileToCopy.SC_Desc = "description";
			fileToCopy.SC_IsPublished = true;
			fileToCopy.SC_IsDeleted = false;
			fileToCopy.SC_IsSystemGenerated = true;
			fileToCopy.SC_SaveVersions = true;
			fileToCopy.SC_ImageData = SamplePdf;
			fileToCopy.SC_SM = parent.PK;

			var dataObject = new SerializableEDoc(fileToCopy);

			var anotherParent = MasterFactory.New<StorageMain>();
			anotherParent.SM_Type = Core.Constants.DocManagerCodes.Shipment;
			StorageDocsBase restoredObject = dataObject.ToBusinessObject(anotherParent, true, false);
			Assert("Should return a StorageFile object if there is a filename and this is not an image", restoredObject is StorageFile);

			StorageFile file = (StorageFile)restoredObject;

			Assert("Restored file is not the same as the original", fileToCopy.PK != file.PK);
			AssertEquals("file should have same properties as original - doctype", "ABC", file.SC_DocType);
			AssertEquals("file should have the description inherited from the DocType, because the original is system generated", "ABC Description", file.SC_Desc);
			Assert("file should have NEW property date", file.SC_Date != fileToCopy.SC_Date);
			AssertEquals("file should have same properties as original - filename", "This is a Filename", file.SC_FileName);
			AssertEquals("file should have same properties as original - published", true, file.SC_IsPublished);
			AssertEquals("file should have same properties as original - deleted", false, file.SC_IsDeleted);
			AssertEquals("file should never be system generated - a doc originating from copy/paste or drag/drop is not original", false, file.SC_IsSystemGenerated);
			AssertEquals("file should have same properties as original - save versions", true, file.SC_SaveVersions);
			AssertEquals("file should have same properties as original - Image", fileToCopy.SC_ImageData, file.SC_ImageData);
			AssertEquals("file should have the alternative parent passed in to the ToBusinessObject Method", anotherParent, file.ParentMain);
		}

		public void TestToBusinessObjectDocumentWithParentAndDescription()
		{
			TestCaseHelper.ClearTable(StorageDocsSchema.Constants.TableName);
			TestCaseHelper.ClearTable(StorageMainSchema.Constants.TableName);
			MasterFactory.Save();
			AssertEquals("Precondition: Should be no StorageDocs objects", 0, MasterFactory.GetDatabaseCount(typeof(StorageDocs)));
			AssertEquals("Precondition: Should be no StorageMain objects", 0, MasterFactory.GetDatabaseCount(typeof(StorageMain)));

			var newDocType = MasterFactory.New<RefDocType>();
			newDocType.RT_DocType = "ABC";
			newDocType.RT_ReferenceType = Core.Constants.ReferenceTypes.SupplyChainLogistics;
			newDocType.RT_Desc = "ABC Description";

			var parent = MasterFactory.New<StorageMain>();
			parent.SM_Type = Core.Constants.DocManagerCodes.Shipment;
			StorageDocs imageToCopy = parent.Documents.AddNew();
			imageToCopy.SC_Date = new ZDateTime(2005, 01, 01);
			imageToCopy.SC_FileName = "This is a Filename"; // this is part of the deterministic properties
			imageToCopy.SC_DataType = "TIF"; // this is part of the deterministic properties
			imageToCopy.SC_DocType = "ABC";
			imageToCopy.SC_Desc = "description";
			imageToCopy.SC_IsPublished = true;
			imageToCopy.SC_IsDeleted = false;
			imageToCopy.SC_IsSystemGenerated = true;
			imageToCopy.SC_SaveVersions = true;
			imageToCopy.SC_ImageData = SmallTif;
			imageToCopy.SC_SM = parent.PK;

			var dataObject = new SerializableEDoc(imageToCopy);

			var anotherParent = MasterFactory.New<StorageMain>();
			anotherParent.SM_Type = Core.Constants.DocManagerCodes.Shipment;
			StorageDocsBase restoredObject = dataObject.ToBusinessObject(anotherParent, true, false);
			Assert("Should return a StorageDocs object if there is a filename and this is an image", restoredObject is StorageDocs);

			StorageDocs image = (StorageDocs)restoredObject;

			Assert("Restored doc is not the same as the original", imageToCopy.PK != image.PK);
			AssertEquals("doc should have same properties as original - doctype", "ABC", image.SC_DocType);
			AssertEquals("doc should have the description inherited from the DocType, because the original is system generated", "ABC Description", image.SC_Desc);
			Assert("doc should have NEW property date", image.SC_Date != imageToCopy.SC_Date);
			AssertEquals("doc should have same properties as original - filename", "This is a Filename", image.SC_FileName);
			AssertEquals("doc should have same properties as original - published", true, image.SC_IsPublished);
			AssertEquals("doc should have same properties as original - deleted", false, image.SC_IsDeleted);
			AssertEquals("doc should never be system generated - a doc originating from copy/paste or drag/drop is not original", false, image.SC_IsSystemGenerated);
			AssertEquals("doc should have same properties as original - save versions", true, image.SC_SaveVersions);
			AssertEquals("doc should have same properties as original - Image", imageToCopy.SC_ImageData, image.SC_ImageData);
			AssertEquals("doc should have the alternative parent passed in to the ToBusinessObject Method", anotherParent, image.ParentMain);
		}

		public void TestSaveToTempFile()
		{
			var parent = MasterFactory.New<StorageMain>();
			parent.SM_Type = Core.Constants.DocManagerCodes.Shipment;

			byte[] expectedBytes = SmallTif;
			StorageFile fileToCopy = StorageFile.New_DEBUG(MasterFactory);
			fileToCopy.SC_Date = new ZDateTime(2005, 01, 01);
			fileToCopy.SC_FileName = "This is a Filename"; // this is the deterministic property
			fileToCopy.SC_DocType = "ABC";
			fileToCopy.SC_Desc = "description";
			fileToCopy.SC_IsPublished = true;
			fileToCopy.SC_IsDeleted = false;
			fileToCopy.SC_IsSystemGenerated = true;
			fileToCopy.SC_SaveVersions = true;
			fileToCopy.SC_ImageData = expectedBytes;
			fileToCopy.SC_SM = parent.PK;

			var doc = new SerializableEDoc(fileToCopy);

			string tempFile = string.Empty;
			try
			{
				tempFile = doc.FileNameWithExtension;
				AssertEquals(expectedBytes, DocumentUtilities.GetFileAsBytes(tempFile));
				AssertEquals("The file is still in disk", true, File.Exists(tempFile));
			}
			finally
			{
				if (File.Exists(tempFile))
				{
					File.Delete(tempFile);
				}
			}
		}

		public void TestIsTifFile()
		{
			StorageDocs docToCopy = StorageDocs.NewWithParent_DEBUG(MasterFactory);

			var doc = new SerializableEDoc(docToCopy);
			AssertEquals("By default docs are tif files (based on SC_DataType)", true, doc.IsImageFile);

			StorageFile file = StorageFile.NewWithParent_DEBUG(MasterFactory);
			file.SC_DataType = "PDF";
			doc = new SerializableEDoc(file);
			AssertEquals("If data type is changed to non-TIF value, the serialised doc is not a tif file", false, doc.IsImageFile);
		}

		protected override void TearDown()
		{
			base.TearDown();
			DeleteTempFiles();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());

		byte[] SmallTif => resourceRetriever.Value.GetBytes("Enterprise.DocumentScanning.Business.Test.TestDocs.small.tif");

		byte[] SamplePdf => resourceRetriever.Value.GetBytes("Enterprise.DocumentScanning.Business.Test.TestDocs.Sample.PDF");
	}
}
