using System.IO;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Billing.Integration;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.SystemMerge.Business;
using Enterprise.DataTransfer.SystemMerge.DataAdapters;
using Enterprise.DocumentScanning.Business;
using Enterprise.DocumentScanning.Business.Test;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DataTransfer.SystemMerge.GUI.Testing
{
	sealed class SysMergeEdocsXmlDataTransferDirectorTest : TestCaseWithDocumentFactory
	{
		public void TestAdapter()
		{
			var director = new SysMergeEdocsXmlDataTransferDirectorForTest();
			AssertEquals("Adapter", typeof(SysMergeEdocsValueObjectDataAdapter), director.Adapter_Exposed.GetType());
		}

		/// <summary>
		/// TestEdocs.xml has 5 documents (DocPk - OrgPk - FileName):
		///  - (869b0a9a-9ae2-4f93-8760-ac8c8c3b3afb) - (3088d562-2ec6-4854-89c2-c2e1415d52df) - TextFile
		///  - (7d927cc2-3aa2-47d1-82cb-172a18b9593c) - (f497aa24-8387-4eb2-888a-694f94b25f76) - ImageFile
		///  - (75f0d899-da32-4a71-bfca-557f0f508951) - (3088d562-2ec6-4854-89c2-c2e1415d52df) - DupPkFile
		///  - (41f79b6b-d72c-4b20-88db-65e33e754240) - (3088d562-2ec6-4854-89c2-c2e1415d52df) - BlankFile
		///  - (c5bdee21-3705-4e9c-b514-61c1b3bcf5fc) - (bbb1e9a7-fe19-41fe-bc74-39f0eb2bc13c) - InvalidOrgFile
		/// </summary>
		[NUnit.Framework.DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportMultipleDocumentsSavesSuccessfulOnesEvenWhenOthersFail()
		{
			// --------------------
			// - CREATE TEST DATA -
			// --------------------
			var org1Pk = new ZGuid("3088d562-2ec6-4854-89c2-c2e1415d52df");
			var org2Pk = new ZGuid("f497aa24-8387-4eb2-888a-694f94b25f76");
			var org3Pk = new ZGuid("bbb1e9a7-fe19-41fe-bc74-39f0eb2bc13c");
			var pk_ExistingDoc = new ZGuid("75f0d899-da32-4a71-bfca-557f0f508951");

			var org1 = MasterFactory.NewWithPrimaryKey<OrgHeaderForDataTransfer>(org1Pk.ToGuid());
			org1.OH_Code = "3088d562-2ec";
			var storageMain1 = MasterFactory.RetrieveExistingOrCreateStorageMainForPK(org1.PK, "ORG");
			var doc_Existing = MasterFactory.GetFactory(storageMain1.SM_DB).NewWithPrimaryKey<StorageDocsForDataTransfer>(pk_ExistingDoc.ToGuid());
			doc_Existing.SC_SM = storageMain1.PK;
			doc_Existing.SC_Date = new ZDateTime(2009, 1, 20);
			doc_Existing.SC_ImageData = new ZBlob(new byte[] { 1, 12, 123 });
			doc_Existing.SC_FileName = "ExistingPkFile";

			var org2 = MasterFactory.NewWithPrimaryKey<OrgHeaderForDataTransfer>(org2Pk.ToGuid());
			org2.OH_Code = "f497aa24-838";

			MasterFactory.Save();

			// ------------------
			// - PRE-CONDITIONS -
			// ------------------

			// Documents which do not exist before import
			var pk_Text = new ZGuid("869b0a9a-9ae2-4f93-8760-ac8c8c3b3afb");
			var pk_Image = new ZGuid("7d927cc2-3aa2-47d1-82cb-172a18b9593c");
			var pk_Blank = new ZGuid("41f79b6b-d72c-4b20-88db-65e33e754240");
			var pk_InvalidOrg = new ZGuid("c5bdee21-3705-4e9c-b514-61c1b3bcf5fc");

			var doc_Text = MasterFactory.GetFactory(storageMain1.SM_DB).Load<StorageDocsForDataTransfer>(pk_Text);
			var doc_Blank = MasterFactory.GetFactory(storageMain1.SM_DB).Load<StorageDocsForDataTransfer>(pk_Blank);
			var storageMain2 = MasterFactory.GetStorageMainForPK(org2Pk);
			var storageMain3 = MasterFactory.GetStorageMainForPK(org3Pk);

			AssertNull("[PRE-CONDITION] Document Text should not exist", doc_Text);
			AssertNull("[PRE-CONDITION] Document Blank should not exist", doc_Blank);
			AssertNull("[PRE-CONDITION] StorageMain for Org2 should not exist", storageMain2);
			AssertNull("[PRE-CONDITION] Org3 does not exist => StorageMain for Org3 should not exist", storageMain3);

			// Document which (PK) exists before import
			int doc_DupPk_Count = MasterFactory.GetFactory(storageMain1.SM_DB).Load<StorageDocsForDataTransfer>(new ZQuery(StorageDocsSchema.SC_FileName, "DupPkFile")).Length;
			AssertEquals("[PRE-CONDITION] Document DupPkFile count", 0, doc_DupPk_Count);

			// --------------
			// - RUN IMPORT -
			// --------------

			// Import XML File To Database
			var director = new SysMergeEdocsXmlDataTransferDirectorForTest();
			var fileName = Path.Combine(BaseSourcePath, "Enterprise", "Product", "Core", "DataTransfer", "DataTransfer.SystemMerge", "GUI", "Testing", "TestEdocs.xml");
			director.Import(fileName, new NotificationBuffer(), SourceInfo.EmptySourceInfo);

			// -------------------
			// - LOAD AND ASSERT -
			// -------------------

			// Load products from Database using a new factory
			var loadFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			storageMain1 = loadFactory.GetStorageMainForPK(org1Pk);
			storageMain2 = loadFactory.GetStorageMainForPK(org2Pk);
			storageMain3 = loadFactory.GetStorageMainForPK(org3Pk);

			AssertNotNull("StorageMain for Org1 should exist", storageMain1);
			AssertEquals("StorageMain for Org1 - DB Number", 1, storageMain1.SM_DB);
			AssertNotNull("StorageMain for Org2 should be created", storageMain2);
			AssertEquals("StorageMain for Org2 - DB Number", 1, storageMain2.SM_DB);
			AssertNull("Org3 does not exist => StorageMain for Org3 should not be created", storageMain3);

			// Document - TextFile
			doc_Text = loadFactory.GetFactory(storageMain1.SM_DB).Load<StorageDocsForDataTransfer>(pk_Text);
			AssertNotNull("Document TextFile should be imported", doc_Text);
			AssertEquals("Document TextFile - StorageMain PK", storageMain1.PK, doc_Text.SC_SM);
			AssertEquals("Document TextFile - FileName", "TextFile", doc_Text.SC_FileName);
			AssertEquals("Document TextFile - DataType", "TXT", doc_Text.SC_DataType);
			AssertEquals("Document TextFile - DocType", "EXM", doc_Text.SC_DocType);
			AssertEquals("Document TextFile - Desc", "Examination Result", doc_Text.SC_Desc);
			AssertEquals("Document TextFile - Date", new ZDateTime(2009, 1, 16, 9, 48, 0), doc_Text.SC_Date);
			AssertEquals("Document TextFile - IsPublished", false, doc_Text.SC_IsPublished);
			AssertEquals("Document TextFile - ImageData length", 18, doc_Text.SC_ImageData.Length);

			// Document - ImageFile
			var doc_Image = loadFactory.GetFactory(storageMain2.SM_DB).Load<StorageDocsForDataTransfer>(pk_Image);
			AssertNotNull("Document ImageFile should be imported", doc_Image);
			AssertEquals("Document ImageFile - StorageMain PK", storageMain2.PK, doc_Image.SC_SM);
			AssertEquals("Document ImageFile - FileName", "ImageFile", doc_Image.SC_FileName);
			AssertEquals("Document ImageFile - DataType", "TIF", doc_Image.SC_DataType);
			AssertEquals("Document ImageFile - DocType", "EXD", doc_Image.SC_DocType);
			AssertEquals("Document ImageFile - Desc", "Exporter Documents", doc_Image.SC_Desc);
			AssertEquals("Document ImageFile - Date", new ZDateTime(2009, 1, 16, 9, 53, 0), doc_Image.SC_Date);
			AssertEquals("Document ImageFile - IsPublished", true, doc_Image.SC_IsPublished);
			AssertEquals("Document ImageFile - ImageData length", 202, doc_Image.SC_ImageData.Length);

			// Document - DupPkFile
			doc_DupPk_Count = loadFactory.GetFactory(storageMain1.SM_DB).Load<StorageDocsForDataTransfer>(new ZQuery(StorageDocsSchema.SC_FileName, "DupPkFile")).Length;
			AssertEquals("Document DupPkFile skiped - same PK exists", 0, doc_DupPk_Count);

			// Document - BlankFile
			doc_Blank = loadFactory.GetFactory(storageMain1.SM_DB).Load<StorageDocsForDataTransfer>(pk_Blank);
			AssertNotNull("Document BlankFile should be imported", doc_Blank);
			AssertEquals("Document BlankFile - StorageMain PK", storageMain1.PK, doc_Blank.SC_SM);
			AssertEquals("Document BlankFile - FileName", "BlankFile", doc_Blank.SC_FileName);
			AssertEquals("Document BlankFile - DataType", "XML", doc_Blank.SC_DataType);
			AssertEquals("Document BlankFile - DocType", "MSC", doc_Blank.SC_DocType);
			AssertEquals("Document BlankFile - Desc", "Blank Document", doc_Blank.SC_Desc);
			AssertEquals("Document BlankFile - Date", new ZDateTime(2009, 1, 20, 10, 27, 0), doc_Blank.SC_Date);
			AssertEquals("Document BlankFile - IsPublished", false, doc_Blank.SC_IsPublished);
			AssertEquals("Document BlankFile - ImageData length", 0, doc_Blank.SC_ImageData.Length);

			// Document - InvalidOrgFile
			var doc_InvalidOrg = loadFactory.GetFactory(1).Load<StorageDocsForDataTransfer>(pk_InvalidOrg);
			AssertNull("Document InvalidOrgFile should not be imported (Inexistent OrgHeader PK)", doc_InvalidOrg);
		}

		sealed class SysMergeEdocsXmlDataTransferDirectorForTest : SysMergeEdocsXmlDataTransferDirector
		{
			internal IValueObjectDataAdapter Adapter_Exposed => Adapter;
		}
	}
}
