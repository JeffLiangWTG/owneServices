using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.UPE.Business;
using Enterprise.Client.UPE.Business.CommercialInvoice;
using Enterprise.Client.UPE.Testing;
using Enterprise.DocumentScanning.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.UPE.DocumentImaging.Testing
{
	sealed class DocumentImagePurgerTest : DocumentImageImportingTestCase
	{
		[TestDate(2013, 01, 12, 9, 18, 00)]
		public void TestPurgeOldDocumentsFromCusHAWBs()
		{
			TestPurgeOldDocuments(CusHAWBStorageDoc);
		}

		[TestDate(2013, 01, 12, 9, 18, 00)]
		public void TestPurgeOldDocumentsFromJobDecs()
		{
			TestPurgeOldDocuments(JobDeclarationStorageDoc);
		}

		[TestDate(2013, 01, 12, 9, 18, 00)]
		public void TestPurgeOldDocumentsFromCusHAWBs_NonUPEBranches()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch = company.Branches.AddNew();
			branch.GB_Code = "ABC";
			Factory.Save();
			CusHAWB.MAWB.CM_GB = branch.PK;
			Factory.Save();
			TestPurgeOldDocuments_NonUPEBranches(CusHAWBStorageDoc);
		}

		[TestDate(2013, 01, 12, 9, 18, 00)]
		public void TestPurgeOldDocumentsFromJobDecs_NonUPEBranches()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch = company.Branches.AddNew();
			branch.GB_Code = "ABC";
			Factory.Save();
			JobDeclaration.JE_GB = branch.PK;
			Factory.Save();
			TestPurgeOldDocuments_NonUPEBranches(JobDeclarationStorageDoc);
		}

		[TestDate]
		public void TestPurgeOver100Documents()
		{
			var documents = Create110StorageDocsToBePurged();
			AssertEquals("Purging 110 documents at once", 110, Purger.PurgeDocumentsForTest());
			var newFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			for (var i = 0; i < documents.Length; i++)
			{
				var deletedDocument = newFactory.Load<StorageDocs>(documents[i].PK);
				AssertNull("All purged documents should be deleted", deletedDocument);
			}
		}

		public void TestDontAttemptPurgeOfDocumentsInNonExistantDocumentDB()
		{
			const int NonExistantDBNumber = 100;
			CusHAWBStorageDoc.SC_Date = ZDateTime.Now.AddMonths(-7);
			CusHAWBStorageMain.SM_DB = NonExistantDBNumber;
			Factory.Save();
			AssertEquals("No documents should be purged because the DB doesnt exist", 0, Purger.PurgeDocumentsForTest());
		}

		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
		}

		readonly DocumentImagePurger Purger = new DocumentImagePurger();

		StorageDocs[] Create110StorageDocsToBePurged()
		{
			TestDateAttribute.Date = DateTime.Now.AddMonths(-10);
			var documents = new StorageDocs[110];
			for (var i = 0; i < documents.Length; i++)
			{
				documents[i] = CreateCusHAWBAndStorageMainAndStorageDoc();
			}

			Factory.Save();
			TestDateAttribute.Date = DateTime.Now;
			return documents;
		}

		StorageDocs CreateCusHAWBAndStorageMainAndStorageDoc()
		{
			var cusHAWB = Factory.NewWithValidTestData<UPECusHAWB>();
			cusHAWB.CS_HAWB = "M1302370459";
			var storageMain = Factory.New<StorageMain>();
			storageMain.SM_ParentFK = cusHAWB.PK;
			storageMain.SM_DB = 1;
			var storageDoc = CusHAWBStorageMain.Documents.AddNew();
			storageDoc.SC_ImageData = UPETestHelper.TestFiles.CommercialInvoiceBmpBytes;
			storageDoc.SC_DocType = CommercialInvoiceDocManager.commercialInvoiceDocType;
			return storageDoc;
		}

		void TestPurgeOldDocuments(StorageDocs document)
		{
			Factory.Save();
			TestDateAttribute.Date = new DateTime(2013, 04, 17, 9, 19, 00);
			document.SC_Date = ZDateTime.Now;
			Factory.Save();
			TestDateAttribute.Date = new DateTime(2013, 04, 17, 9, 18, 00);
			Purger.PurgeDocumentsForTest();
			AssertEquals("Documents should not be purged if it is less than 6 months old", false, document.IsDeleted);
			TestDateAttribute.Date = new DateTime(2013, 01, 12, 9, 18, 00);
			document.SC_Date = ZDateTime.Now;
			Factory.Save();
			TestDateAttribute.Date = new DateTime(2013, 07, 22, 9, 18, 00);
			Purger.PurgeDocumentsForTest();
			AssertEquals("Documents should be purged if it is more than 6 months old", true, document.IsDeleted);
			AssertEquals("Should not purge an Organisation StorageDoc", false, OrgStorageDoc.IsDeleted);
		}

		void TestPurgeOldDocuments_NonUPEBranches(StorageDocs document)
		{
			Factory.Save();
			TestDateAttribute.Date = new DateTime(2013, 04, 17, 9, 19, 00);
			document.SC_Date = ZDateTime.Now;
			Factory.Save();
			TestDateAttribute.Date = new DateTime(2013, 04, 17, 9, 18, 00);
			Purger.PurgeDocumentsForTest();
			AssertEquals("Documents should not be purged if it is less than 6 months old", false, document.IsDeleted);
			TestDateAttribute.Date = new DateTime(2013, 01, 12, 9, 18, 00);
			document.SC_Date = ZDateTime.Now;
			Factory.Save();
			TestDateAttribute.Date = new DateTime(2013, 07, 22, 9, 18, 00);
			Purger.PurgeDocumentsForTest();
			AssertEquals("Documents should not be purged if it is more than 6 months old", false, document.IsDeleted);
			AssertEquals("Should not purge an Organisation StorageDoc", false, OrgStorageDoc.IsDeleted);
		}
	}
}
