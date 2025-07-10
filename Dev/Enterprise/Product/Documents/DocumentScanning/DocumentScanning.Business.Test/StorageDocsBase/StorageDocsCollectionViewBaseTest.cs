using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentScanning.Business.Testing;
using Enterprise.Environment;
using Enterprise.Freight.LocalCartage.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.Business.Test
{
	[TestedType(typeof(StorageDocsCollectionViewBase))]
	public class StorageDocsCollectionViewBaseTest : BusinessObjectCollectionViewTestCase<StorageDocsCollectionViewBase>
	{
		[ExpectNoExceptions]
		public void TestCFSShipmentAddDocuments()
		{
			var cfsShipment = (Freight.Business.CommonShipment)Factory.New<Freight.Integration.CFS.ICFSShipment>();
			cfsShipment.FillWithValidTestData();
			Parent.SM_Type = Core.Constants.DocManagerCodes.CFSShipmentReceival;
			Parent.SM_ParentFK = cfsShipment.PK;
			Parent.SM_DB = 1;
			var size20KB = Encoding.Default.GetBytes(new string('A', 1024 * 20));
			CreateNewDocument(Parent, Core.Constants.RefDocTypes.CertificateOfReceipt, ZDateTime.Now, size20KB, false, false, true);
			CreateNewDocument(Parent, Core.Constants.RefDocTypes.MasterBill, ZDateTime.Now, size20KB, false, false, true);
			Factory.Save();

			var requiredDocuments = Factory.Load<JobRequiredDocument>(new ZQuery());
			AssertEquals(2, requiredDocuments.Length);
		}

		public void TestSortHumanReadableAttachmentSize()
		{
			var size20KB = Encoding.Default.GetBytes(new string('A', 1024 * 20));
			var size450KB = Encoding.Default.GetBytes(new string('A', 1024 * 450));
			var size1500KB = Encoding.Default.GetBytes(new string('A', 1024 * 1500));
			var size300KB = Encoding.Default.GetBytes(new string('A', 1024 * 300));

			CreateNewDocument(Parent, "ABC", ZDateTime.Now, size20KB, false, false, true);
			CreateNewDocument(Parent, "ABC", ZDateTime.Now, size450KB, false, false, true);
			CreateNewDocument(Parent, "ABC", ZDateTime.Now, size1500KB, false, false, true);
			CreateNewDocument(Parent, "ABC", ZDateTime.Now, size300KB, false, false, true);

			AssertEquals(4, Parent.eDocsView.Count);

			Parent.eDocsView.Sort("HumanReadableAttachmentSize", ListSortDirection.Ascending);
			AssertEquals(size20KB.Length, Parent.eDocsView[0].SC_ImageData.Length);
			AssertEquals(size300KB.Length, Parent.eDocsView[1].SC_ImageData.Length);
			AssertEquals(size450KB.Length, Parent.eDocsView[2].SC_ImageData.Length);
			AssertEquals(size1500KB.Length, Parent.eDocsView[3].SC_ImageData.Length);

			Parent.eDocsView.Sort("HumanReadableAttachmentSize", ListSortDirection.Descending);
			AssertEquals(size1500KB.Length, Parent.eDocsView[0].SC_ImageData.Length);
			AssertEquals(size450KB.Length, Parent.eDocsView[1].SC_ImageData.Length);
			AssertEquals(size300KB.Length, Parent.eDocsView[2].SC_ImageData.Length);
			AssertEquals(size20KB.Length, Parent.eDocsView[3].SC_ImageData.Length);
		}

		public void TestSqlApplicationLockForAddRelatedRequiredDocuments()
		{
			var shipment = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Forwarding.IForwardingShipment)));
			Factory.Save();

			Parent.SM_Type = Core.Constants.DocManagerCodes.Shipment;
			Parent.SM_ParentFK = shipment.PK;
			Parent.SM_DB = 1;
			Factory.Save();

			var storageDocsView = new StorageDocsCollectionViewBase(Parent, Parent.eDocs);
			var countBefore = shipment.Factory.SqlLockCount;
			storageDocsView.AddRelatedRequiredDocuments();
			var countAfter = shipment.Factory.SqlLockCount;
			AssertEquals("There should be no SqlApplicationLock acquired as no eDocs added", countBefore, countAfter);

			StorageDocs storageDocs1 = CreateStorageDocument<StorageDocs>(Core.Constants.RefDocTypes.MiscellaneousDocument, DateTime.Now, "_Test document.doc", "_Test description");
			Parent.eDocs.Add(storageDocs1);
			countBefore = shipment.Factory.SqlLockCount;
			storageDocsView.AddRelatedRequiredDocuments();
			countAfter = shipment.Factory.SqlLockCount;
			AssertEquals("There should be no SqlApplicationLock acquired as we should not check the duplication of eDoc type MSC", countBefore, countAfter);

			StorageDocs storageDocs2 = CreateStorageDocument<StorageDocs>(Core.Constants.RefDocTypes.MasterBill, DateTime.Now, "_Test document2.doc", "_Test description2.doc");
			Parent.eDocs.Add(storageDocs2);
			countBefore = shipment.Factory.SqlLockCount;
			storageDocsView.AddRelatedRequiredDocuments();
			ErrorReporter.Clear();
			countAfter = shipment.Factory.SqlLockCount;
			AssertEquals("There should be 1 SqlApplicationLock acquired for MBL document", 1, countAfter - countBefore);

			Factory.Save();
			AssertEquals("All locks should be released", 0, shipment.Factory.SqlLockCount);

			ErrorReporter.Clear();
		}

		protected override void TearDown()
		{
			Parent.ReleaseSqlLocks();
		}

		public void TestTypeOfElements()
		{
			var view = new StorageDocsCollectionViewBase(Parent.eDocs);
			AssertEquals("type of elements", typeof(StorageDocsBase), view.TypeOfElements);
		}

		public override void TestAddNew()
		{
			Assert("not required here", true);
		}

		public void TestExcludeUnpublishedDocuments()
		{
			var doc1 = (StorageDocs)Parent.eDocs.AddNew(typeof(StorageDocs));
			doc1.SC_IsPublished = false;
			var doc2 = (StorageDocs)Parent.eDocs.AddNew(typeof(StorageDocs));
			doc2.SC_IsPublished = true;
			var doc3 = (StorageDocs)Parent.eDocs.AddNew(typeof(StorageDocs));
			doc3.SC_IsPublished = false;
			var file1 = (StorageFile)Parent.eDocs.AddNew(typeof(StorageFile));
			file1.SC_IsPublished = true;

			var view = new StorageDocsCollectionViewBase(Parent.eDocs);
			AssertEquals("Collection count", 4, view.Count);

			view.ExcludeUnpublishedDocuments = true;
			AssertEquals("Collection count excluding unpublished docs", 2, view.Count);
		}

		public void TestIsCompanySpecific()
		{
			var doc1 = (StorageDocs)Parent.eDocs.AddNew(typeof(StorageDocs));
			doc1.SC_GC_Company = ZGuid.Empty;
			var doc2 = (StorageDocs)Parent.eDocs.AddNew(typeof(StorageDocs));
			doc2.SC_GC_Company = GlbCompany.CurrentCompany.PK;
			var doc3 = (StorageDocs)Parent.eDocs.AddNew(typeof(StorageDocs));
			doc3.SC_GC_Company = ZGuid.NewZGuid();

			var view = new StorageDocsCollectionViewBase(Parent.eDocs);
			view.ShowDocumentsForAllCompanies = false;
			AssertEquals("Shouldn't include doc3", 2, view.Count);
			AssertNull("Shouldn't include doc3", view.FindByPK(doc3.PK));

			view.ShowDocumentsForAllCompanies = true;
			AssertEquals("Should include doc3", 3, view.Count);
			AssertNotNull("Should include doc3", view.FindByPK(doc3.PK));

			view.ShowDocumentsForAllCompanies = false;
			doc3.SC_GC_Company = GlbCompany.CurrentCompany.PK;
			AssertEquals("Should include doc3", 3, view.Count);
			AssertNotNull("Should include doc3", view.FindByPK(doc3.PK));
		}

		public void TestIsBranchSpecific()
		{
			var doc1 = (StorageDocs)Parent.eDocs.AddNew(typeof(StorageDocs));
			doc1.SC_GB_Branch = ZGuid.Empty;
			var doc2 = (StorageDocs)Parent.eDocs.AddNew(typeof(StorageDocs));
			doc2.SC_GB_Branch = GlbBranch.CurrentBranch.PK;
			var doc3 = (StorageDocs)Parent.eDocs.AddNew(typeof(StorageDocs));
			doc3.SC_GB_Branch = ZGuid.NewZGuid();

			var view = new StorageDocsCollectionViewBase(Parent.eDocs);
			view.ShowDocumentsForAllBranches = false;
			AssertEquals("Shouldn't include doc3", 2, view.Count);
			AssertNull("Shouldn't include doc3", view.FindByPK(doc3.PK));

			view.ShowDocumentsForAllBranches = true;
			AssertEquals("Should include doc3", 3, view.Count);
			AssertNotNull("Should include doc3", view.FindByPK(doc3.PK));

			view.ShowDocumentsForAllBranches = false;
			doc3.SC_GB_Branch = GlbBranch.CurrentBranch.PK;
			AssertEquals("Should include doc3", 3, view.Count);
			AssertNotNull("Should include doc3", view.FindByPK(doc3.PK));
		}

		public void TestIsDepartmentSpecific()
		{
			var doc1 = (StorageDocs)Parent.eDocs.AddNew(typeof(StorageDocs));
			doc1.SC_GE_Department = ZGuid.Empty;
			var doc2 = (StorageDocs)Parent.eDocs.AddNew(typeof(StorageDocs));
			doc2.SC_GE_Department = GlbDepartment.CurrentDepartment.PK;
			var doc3 = (StorageDocs)Parent.eDocs.AddNew(typeof(StorageDocs));
			doc3.SC_GE_Department = ZGuid.NewZGuid();

			var view = new StorageDocsCollectionViewBase(Parent.eDocs);
			view.ShowDocumentsForAllDepartments = false;
			AssertEquals("Shouldn't include doc3", 2, view.Count);
			AssertNull("Shouldn't include doc3", view.FindByPK(doc3.PK));

			view.ShowDocumentsForAllDepartments = true;
			AssertEquals("Should include doc3", 3, view.Count);
			AssertNotNull("Should include doc3", view.FindByPK(doc3.PK));

			view.ShowDocumentsForAllDepartments = false;
			doc3.SC_GE_Department = GlbDepartment.CurrentDepartment.PK;
			AssertEquals("Should include doc3", 3, view.Count);
			AssertNotNull("Should include doc3", view.FindByPK(doc3.PK));
		}

		public void TestIncludeDeletedDocuments()
		{
			var doc1 = (StorageDocs)Parent.eDocs.AddNew(typeof(StorageDocs));
			doc1.SC_IsDeleted = false;
			var doc2 = (StorageDocs)Parent.eDocs.AddNew(typeof(StorageDocs));
			doc2.SC_IsDeleted = true;
			var doc3 = (StorageDocs)Parent.eDocs.AddNew(typeof(StorageDocs));
			doc3.SC_IsDeleted = false;
			var file1 = (StorageFile)Parent.eDocs.AddNew(typeof(StorageFile));
			file1.SC_IsDeleted = true;

			var view = new StorageDocsCollectionViewBase(Parent.eDocs);
			AssertEquals("Collection count", 2, view.Count);

			view.IncludeDeletedDocuments = true;
			AssertEquals("Collection count including deleted docs", 4, view.Count);

			view.ExcludeUnpublishedDocuments = true;
			doc1.SC_IsPublished = true;
			AssertEquals("Collection count excluding unpublished docs", 1, view.Count);
		}

		public void TestAllowNew()
		{
			var view = new StorageDocsCollectionViewBase(Parent.eDocs);
			Assert("Should not AllowNew on the collection", !view.AllowNew);
		}

		public void TestAddRelatedRequiredDocuments_EnsureActiveRefDocTypeIsLoaded()
		{
			var shipment = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Forwarding.IForwardingShipment)));
			Factory.Save();

			Parent.SM_Type = Core.Constants.DocManagerCodes.Shipment;
			Parent.SM_ParentFK = shipment.PK;
			Parent.SM_DB = 1;
			Factory.Save();

			var storageDocsView = new StorageDocsCollectionViewBase(Parent, Parent.eDocs);
			AssertEquals("There should be zero storage docs initially.", 0, storageDocsView.Count);

			var docType = Factory.NewWithValidTestData<RefDocType>();
			docType.RT_DocType = "AAA";
			docType.RT_IsActive = false;
			docType.RT_ReferenceType = Core.Constants.ReferenceTypes.BusinessEntityProcessWorkflow;

			var anotherFactory = new BusinessObjectFactory();
			var docTypeWithSameName = anotherFactory.NewWithValidTestData<RefDocType>();
			docTypeWithSameName.RT_DocType = "AAA";
			docTypeWithSameName.RT_IsActive = true;
			docTypeWithSameName.RT_ReferenceType = Core.Constants.ReferenceTypes.SupplyChainLogistics;

			anotherFactory.Save();

			var storageDoc = CreateStorageDocument<StorageDocs>(docType.RT_DocType, ZDateTime.Now, "_Test document.doc", "_Test description");
			Parent.eDocs.Add(storageDoc);
			AddAndLoadRequiredDocument(shipment, storageDocsView);

			AssertEquals("There should now be one storage doc.", 1, storageDocsView.Count);
			AssertEquals("There should now be 1 required document.", 1, ((IHaveRequiredDocuments)shipment["DocsAndCartage"]).RequiredDocuments.Count);

			shipment.Factory.ReleaseSqlLocks();
			ErrorReporter.Clear();
		}

		public void TestAddRelatedRequiredDocuments()
		{
			var shipment = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Forwarding.IForwardingShipment)));
			Factory.Save();

			Parent.SM_Type = Core.Constants.DocManagerCodes.Shipment;
			Parent.SM_ParentFK = shipment.PK;
			Parent.SM_DB = 1;
			Factory.Save();

			AssertNotNull("The StorageMain must have a valid parent.", Parent.DocumentOwner);
			AssertEquals("There should be zero required docs initially.", 0, ((IHaveRequiredDocuments)shipment["DocsAndCartage"]).RequiredDocuments.Count);

			var storageDocsView = new StorageDocsCollectionViewBase(Parent, Parent.eDocs);
			AssertEquals("There should be zero storage docs initially.", 0, storageDocsView.Count);

			storageDocsView.AddRelatedRequiredDocuments();
			AssertEquals("There should be zero documents on the view, because there are no storage docs.", 0, storageDocsView.Count);

			StorageDocs storageDocs1 = CreateStorageDocument<StorageDocs>(Core.Constants.RefDocTypes.MiscellaneousDocument,
				DateTime.Now, "_Test document.doc", "_Test description");

			Parent.eDocs.Add(storageDocs1);
			AssertEquals("There should now be one storage doc.", 1, storageDocsView.Count);

			AddAndLoadRequiredDocument(shipment, storageDocsView);

			AssertEquals("There should still be zero required documents, because the storage doc type is MSC.", 0, ((IHaveRequiredDocuments)shipment["DocsAndCartage"]).RequiredDocuments.Count);

			StorageDocs storageDocs2 = CreateStorageDocument<StorageDocs>(Core.Constants.RefDocTypes.MasterBill,
				DateTime.Now, "_Test document2.doc", "_Test description2.doc");

			Parent.eDocs.Add(storageDocs2);
			AssertEquals("There should now be 2 storage docs.", 2, storageDocsView.Count);

			AddAndLoadRequiredDocument(shipment, storageDocsView);

			AssertEquals("There should now be 1 required document.", 1, ((IHaveRequiredDocuments)shipment["DocsAndCartage"]).RequiredDocuments.Count);
			AssertEquals("The required document type should match the type of the eDoc.", storageDocs2.SC_DocType,
				((IHaveRequiredDocuments)shipment["DocsAndCartage"]).RequiredDocuments[0].EQ_DocType);
			AssertEquals("The required document received date should match the type of the eDoc.", storageDocs2.SC_DateLocalBranchTime,
				((IHaveRequiredDocuments)shipment["DocsAndCartage"]).RequiredDocuments[0].EQ_DateReceived.ToLocalZDateTime());
			AssertEquals("The Doc category should be SCL", "SCL",
				((IHaveRequiredDocuments)shipment["DocsAndCartage"]).RequiredDocuments[0].EQ_DocCategory);
			AssertEquals("Origin is AddedEDoc", JobRequiredDocument.JRDOrigin.AddedEDoc,
				((IHaveRequiredDocuments)shipment["DocsAndCartage"]).RequiredDocuments[0].Origin);

			AddAndLoadRequiredDocument(shipment, storageDocsView);

			AssertEquals("There should still be one required document because required documents should not be duplicated if they already exist.",
				1, ((IHaveRequiredDocuments)shipment["DocsAndCartage"]).RequiredDocuments.Count);

			StorageFile storageFile1 = CreateStorageDocument<StorageFile>(Core.Constants.RefDocTypes.CommercialInvoice,
				DateTime.Now, "_Test document 2.xls", "_Test description 2");

			Parent.eDocs.Add(storageFile1);
			AssertEquals("There should be 3 eDocs.", 3, Parent.eDocs.Count);

			AddAndLoadRequiredDocument(shipment, storageDocsView);

			AssertEquals("There should be two required documents on the shipment.", 2, ((IHaveRequiredDocuments)shipment["DocsAndCartage"]).RequiredDocuments.Count);

			((IHaveRequiredDocuments)shipment["DocsAndCartage"]).RequiredDocuments.RemoveAll();
			((IHaveRequiredDocuments)Parent.DocumentOwner["DocsAndCartage"]).RequiredDocuments.RemoveAll();
			storageDocs1.Delete();
			storageDocs2.Delete();
			storageFile1.Delete();
			Factory.Save();
			AssertEquals("Shipment should contain zero required documents.", 0, ((IHaveRequiredDocuments)shipment["DocsAndCartage"]).RequiredDocuments.Count);
			AssertEquals("Shipment should contain zero required documents.", 0,
				((IHaveRequiredDocuments)Parent.DocumentOwner["DocsAndCartage"]).RequiredDocuments.Count);

			JobRequiredDocument requiredDocument = ((IHaveRequiredDocuments)shipment["DocsAndCartage"]).RequiredDocuments.AddNew();
			requiredDocument.EQ_DocType = Core.Constants.RefDocTypes.CommercialInvoice;
			requiredDocument.EQ_DateReceived = ZDateTimeOffset.Empty;

			Factory.Save();
			((IHaveRequiredDocuments)shipment["DocsAndCartage"]).RequiredDocuments.Load();
			AssertEquals("Shipment should contain 1 required document.", 1, ((IHaveRequiredDocuments)shipment["DocsAndCartage"]).RequiredDocuments.Count);
			AssertEquals("Required document should have an empty received date.", ZDateTimeOffset.Empty,
				((IHaveRequiredDocuments)shipment["DocsAndCartage"]).RequiredDocuments[0].EQ_DateReceived);

			StorageFile storageFile2 = CreateStorageDocument<StorageFile>(Core.Constants.RefDocTypes.CommercialInvoice,
				DateTime.Now, "_Test document 3.pdf", "_Test description 3");
			Parent.eDocs.Add(storageFile2);

			AssertEquals("There should be 1 eDoc.", 1, Parent.eDocs.Count);

			((IHaveRequiredDocuments)Parent.DocumentOwner["DocsAndCartage"]).RequiredDocuments.Load();
			AddAndLoadRequiredDocument(shipment, storageDocsView);

			AssertEquals("The date of the existing required document should be updated because it was null.", storageFile2.SC_DateLocalBranchTime,
				((IHaveRequiredDocuments)shipment["DocsAndCartage"]).RequiredDocuments[0].EQ_DateReceived.ToLocalZDateTime());

			StorageFile storageFileObsolete = CreateStorageDocument<StorageFile>(Core.Constants.RefDocTypes.MasterBill,
				DateTime.Now, "_Test obsolete document.pdf", "");
			Factory.Save();
			Parent.eDocs.Add(storageFileObsolete);
			AddAndLoadRequiredDocument(shipment, storageDocsView);
			AssertEquals("Shipment should still contain 1 required document.", 1, ((IHaveRequiredDocuments)shipment["DocsAndCartage"]).RequiredDocuments.Count);

			ErrorReporter.Clear();
		}

		public void TestAddRelatedRequiredDocuments_SetTheRightCategory_HR()
		{
			var bizo = (IHaveRequiredDocuments)Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Recruiter.IHRJobApplicant)));

			var newDocType = Factory.New<RefDocType>();
			newDocType.RT_ReferenceType = "HRE";
			newDocType.RT_DocType = "CVS";
			newDocType.RT_Desc = "Get a job!";
			newDocType.RT_AllowMultiplePeriodicDocs = false;

			Parent.SM_Type = Core.Constants.DocManagerCodes.JobApplicant;
			Parent.SM_ParentFK = bizo.PK;
			Parent.SM_DB = 1;
			Factory.Save();

			var storageDocsView = new StorageDocsCollectionViewBase(Parent, Parent.eDocs);

			StorageDocs storageDocs = CreateStorageDocument<StorageDocs>(newDocType.RT_DocType, DateTime.Now, "_Test document2.doc", "_Test description2.doc");

			Parent.eDocs.Add(storageDocs);
			storageDocsView.AddRelatedRequiredDocuments();

			AssertEquals("There should now be 1 required document.", 1, bizo.RequiredDocuments.Count);
			var reqDoc = bizo.RequiredDocuments[0];
			AssertEquals("The required document type should match the type of the eDoc.", storageDocs.SC_DocType, reqDoc.EQ_DocType);
			AssertEquals("The required document received date should match the type of the eDoc.", storageDocs.SC_DateLocalBranchTime, reqDoc.EQ_DateReceived.ToLocalZDateTime());
			AssertEquals("The required document category should be HRE", Core.Constants.ReferenceTypes.HumanResourcesStaffEmployment, reqDoc.EQ_DocCategory);

			bizo.Factory.ReleaseSqlLocks();
			ErrorReporter.Clear();
		}

		public void TestAddRelatedRequiredDocuments_SetTheRightCategory_ORG_ACV()
		{
			var bizo = (IHaveRequiredDocuments)Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IOrgHeader)));

			Parent.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			Parent.SM_ParentFK = bizo.PK;
			Parent.SM_DB = 1;
			Factory.Save();

			var storageDocsView = new StorageDocsCollectionViewBase(Parent, Parent.eDocs);

			StorageDocs storageDocs = CreateStorageDocument<StorageDocs>("ACV", DateTime.Now, "_Test document2.doc", "_Test description2.doc");

			Parent.eDocs.Add(storageDocs);
			storageDocsView.AddRelatedRequiredDocuments();

			AssertEquals("There should now be 1 required document.", 1, bizo.RequiredDocuments.Count);
			var reqDoc = bizo.RequiredDocuments[0];
			AssertEquals("The required document type should match the type of the eDoc.", storageDocs.SC_DocType, reqDoc.EQ_DocType);
			AssertEquals("The required document received date should match the type of the eDoc.", storageDocs.SC_DateLocalBranchTime, reqDoc.EQ_DateReceived.ToLocalZDateTime());
			AssertEquals("The required document category should be CSR", Core.Constants.ReferenceTypes.ClientSupplierRelationship, reqDoc.EQ_DocCategory);

			AssertEquals("The required document usage should be defaulted.", JobRequiredDocument.DocUsage.Broker, reqDoc.EQ_DocUsage);
			AssertEquals("The required document period should be defaulted.", Core.Constants.JobRequiredDocuments.DocumentPeriods.OncePerShipment, reqDoc.EQ_DocPeriod);

			bizo.Factory.ReleaseSqlLocks();
			ErrorReporter.Clear();
		}

		public void TestAddRelatedRequiredDocuments_SetTheRightCategory_ORG_WTE()
		{
			var bizo = (IHaveRequiredDocuments)Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IOrgHeader)));

			Parent.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			Parent.SM_ParentFK = bizo.PK;
			Parent.SM_DB = 1;
			Factory.Save();

			var storageDocsView = new StorageDocsCollectionViewBase(Parent, Parent.eDocs);

			StorageDocs storageDocs = CreateStorageDocument<StorageDocs>("WTE", DateTime.Now, "_Test document2.doc", "_Test description2.doc");

			Parent.eDocs.Add(storageDocs);
			storageDocsView.AddRelatedRequiredDocuments();

			AssertEquals("There should now be 1 required document.", 1, bizo.RequiredDocuments.Count);
			var reqDoc = bizo.RequiredDocuments[0];
			AssertEquals("The required document type should match the type of the eDoc.", storageDocs.SC_DocType, reqDoc.EQ_DocType);
			AssertEquals("The required document received date should match the type of the eDoc.", storageDocs.SC_DateLocalBranchTime, reqDoc.EQ_DateReceived.ToLocalZDateTime());
			AssertEquals("The required document category should be CSR", Core.Constants.ReferenceTypes.ClientSupplierRelationship, reqDoc.EQ_DocCategory);

			AssertEquals("The required document usage should be defaulted.", JobRequiredDocument.DocUsage.Creditor, reqDoc.EQ_DocUsage);
			AssertEquals("The required document period should be defaulted.", Core.Constants.JobRequiredDocuments.DocumentPeriods.Periodic, reqDoc.EQ_DocPeriod);

			bizo.Factory.ReleaseSqlLocks();
			ErrorReporter.Clear();
		}

		public void TestAddRelatedRequiredDocumentsWithInactiveDocType()
		{
			var shipment = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Forwarding.IForwardingShipment)));
			Factory.Save();

			Parent.SM_Type = Core.Constants.DocManagerCodes.Shipment;
			Parent.SM_ParentFK = shipment.PK;
			Parent.SM_DB = 1;

			var docType = Factory.NewWithValidTestData<RefDocType>();
			docType.RT_DocType = "AAA";
			docType.RT_IsActive = false;

			Factory.Save();

			var storageDocsView = new StorageDocsCollectionViewBase(Parent, Parent.eDocs);
			var storageDocs1 = CreateStorageDocument<StorageDocs>("AAA", DateTime.Now, "_Test document.doc", "");
			Parent.eDocs.Add(storageDocs1);
			AssertEquals("There should now be one storage doc.", 1, storageDocsView.Count);

			AddAndLoadRequiredDocument(shipment, storageDocsView);

			AssertEquals("There should now be 0 required documents.", 0, ((IHaveRequiredDocuments)shipment["DocsAndCartage"]).RequiredDocuments.Count);

			ErrorReporter.Clear();
		}

		public void TestHideDeniedDocumentType()
		{
			var shipment = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Forwarding.IForwardingShipment)));
			Factory.Save();

			Parent.SM_Type = Core.Constants.DocManagerCodes.Shipment;
			Parent.SM_ParentFK = shipment.PK;
			Parent.SM_DB = 1;

			var docType = Factory.NewWithValidTestData<RefDocType>();
			docType.RT_DocType = "AAA";
			docType.RT_IsActive = true;
			var docType2 = Factory.NewWithValidTestData<RefDocType>();
			docType2.RT_DocType = "BBB";
			docType2.RT_IsActive = true;
			Factory.Save();

			Env.Security.GetDocumentTypeViewCheckPoint("AAA").IsAllowed = false;

			var storageDocsView = new StorageDocsCollectionViewBase(Parent, Parent.eDocs);
			var storageDocs1 = CreateStorageDocument<StorageDocs>("AAA", DateTime.Now, "_Test document.doc", "");
			var storageDocs2 = CreateStorageDocument<StorageDocs>("BBB", DateTime.Now, "_Test document2.doc", "");
			Parent.eDocs.Add(storageDocs1);
			Parent.eDocs.Add(storageDocs2);

			AssertEquals("There should now be one storage doc.", 1, storageDocsView.Count);
			AssertEquals("BBB", storageDocsView[0].SC_DocType);
		}

		public void TestAddRelatedRequiredDocumentsWithInexistentDocType()
		{
			var shipment = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Forwarding.IForwardingShipment)));
			Factory.Save();

			Parent.SM_Type = Core.Constants.DocManagerCodes.Shipment;
			Parent.SM_ParentFK = shipment.PK;
			Parent.SM_DB = 1;

			var storageDocsView = new StorageDocsCollectionViewBase(Parent, Parent.eDocs);
			StorageDocs storageDocs1 = CreateStorageDocument<StorageDocs>("AAA", DateTime.Now, "_Test document.doc", "");
			Parent.eDocs.Add(storageDocs1);
			AssertEquals("There should now be one storage doc.", 1, storageDocsView.Count);

			AddAndLoadRequiredDocument(shipment, storageDocsView);

			AssertEquals("There should now be 0 required documents.", 0, ((IHaveRequiredDocuments)shipment["DocsAndCartage"]).RequiredDocuments.Count);

			ErrorReporter.Clear();
		}

		public void TestAddRelatedRequiredDocumentsWithExistentDocType()
		{
			var shipment = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Forwarding.IForwardingShipment)));
			Factory.Save();

			Parent.SM_Type = Core.Constants.DocManagerCodes.Shipment;
			Parent.SM_ParentFK = shipment.PK;
			Parent.SM_DB = 1;

			var docType = Factory.NewWithValidTestData<RefDocType>();
			docType.RT_DocType = "AAA";
			docType.RT_IsActive = true;
			Factory.Save();

			var storageDocsView = new StorageDocsCollectionViewBase(Parent, Parent.eDocs);
			StorageDocs storageDocs1 = CreateStorageDocument<StorageDocs>("AAA", DateTime.Now, "_Test document.doc", "");
			Parent.eDocs.Add(storageDocs1);
			AssertEquals("There should now be one storage doc.", 1, storageDocsView.Count);

			AddAndLoadRequiredDocument(shipment, storageDocsView);

			AssertEquals("There should now be 1 required document.", 1, ((IHaveRequiredDocuments)shipment["DocsAndCartage"]).RequiredDocuments.Count);

			shipment.Factory.ReleaseSqlLocks();
			ErrorReporter.Clear();
		}

		public void TestAddRelatedRequiredDocumentsForLandedCosting()
		{
			var testDec = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			var invoice = (BusinessObject)Factory.New<Enterprise.Integration.Customs.Shared.IBaseJobComInvoiceHeader>();
			invoice[JobComInvoiceHeaderSchema.Constants.JZ_JE] = testDec.PK;
			invoice[JobComInvoiceHeaderSchema.Constants.JZ_InvoiceAmount] = 1000m;
			invoice[JobComInvoiceHeaderSchema.Constants.JZ_RX_NKInvoice_Currency] = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			invoice[JobComInvoiceHeaderSchema.Constants.JZ_IncoTerm] = "FOB";

			var invoiceLine = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobComInvoiceLine>();
			invoiceLine[JobComInvoiceLineSchema.Constants.JI_JZ] = invoice.PK;
			invoiceLine[JobComInvoiceLineSchema.Constants.JI_LinePrice] = 1000m;

			var charge = ((BusinessObjectCollection)invoiceLine["Charges"]).AddNew();
			charge[JobComInvHeaderChargeSchema.Constants.J7_ParentID] = invoiceLine.PK;
			charge[JobComInvHeaderChargeSchema.Constants.J7_ParentTableCode] = JobComInvoiceLineSchema.Constants.Prefix;
			charge[JobComInvHeaderChargeSchema.Constants.J7_ChargeType] = "OFT";
			charge[JobComInvHeaderChargeSchema.Constants.J7_Amount] = 100m;
			charge[JobComInvHeaderChargeSchema.Constants.J7_RX_NKCurrency] = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			var jobInvoicing = new JobHeader.Loader((IJobHeaderParent)Factory.New<ICommonCartage>()).TryCreate();
			jobInvoicing.FillWithValidTestData();
			jobInvoicing[JobHeaderSchema.JH_ParentID.Name] = testDec.PK;

			var charge1 = Factory.New<JobCharge>();
			charge1.FillWithValidTestData();
			charge1.JR_JH = jobInvoicing.PK;
			charge1.JR_AC = Enterprise.Environment.Env.Registry.FreightChargeCode;
			charge1.JR_LocalSellAmt = 150m;
			charge1.JR_OSSellAmt = 150m;
			charge1.JR_RX_NKSellCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			Factory.Save();

			var lCHeader = (BusinessObject)Factory.New<Enterprise.Integration.LandedCosting.ILandedCostHeader>();
			lCHeader[LandedCostHeaderSchema.LT_ParentID.Name] = testDec.PK;
			lCHeader[LandedCostHeaderSchema.LT_ParentTableCode.Name] = "JE";

			Parent.SM_ParentFK = lCHeader.PK;
			Parent.SM_Type = Core.Constants.DocManagerCodes.LandedCostHeader;
			Parent.SM_DB = 1;
			Factory.Save();

			var storageDocs1 = CreateStorageDocument<StorageDocs>(Core.Constants.RefDocTypes.LandedCosting, DateTime.Now, "_Test document.doc", "_Test description.doc");
			Parent.eDocs.Add(storageDocs1);

			var storageDocsView = new StorageDocsCollectionViewBase(Parent, Parent.eDocs);
			storageDocsView.AddRelatedRequiredDocuments();
			AssertEquals("There should now be one storage doc.", 1, storageDocsView.Count);
			AssertEquals(Core.Constants.RefDocTypes.LandedCosting, storageDocsView[0].SC_DocType);

			((IHaveRequiredDocuments)testDec["DocsAndCartage"]).RequiredDocuments.Load();
			AssertEquals("There should be one related document", 1, ((IHaveRequiredDocuments)testDec["DocsAndCartage"]).RequiredDocuments.Count);

			var reqDocument = ((IHaveRequiredDocuments)testDec["DocsAndCartage"]).RequiredDocuments[0];
			AssertEquals("The required document type should match the type of the eDoc.", storageDocs1.SC_DocType, reqDocument.EQ_DocType);
			AssertEquals("The required document received date should match the type of the eDoc.", storageDocs1.SC_DateLocalBranchTime, reqDocument.EQ_DateReceived.ToLocalZDateTime());
			AssertEquals(JobRequiredDocument.DocUsage.Import, reqDocument.EQ_DocUsage);

			testDec.Factory.ReleaseSqlLocks();
			ErrorReporter.Clear();
		}

		[TestDate(2012, 09, 20, 11, 54, 02)]
		public void TestReceivedDatesAreProperlyUpdatedWithMultipleDocs()
		{
			var shipment = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Forwarding.IForwardingShipment)));
			Parent.SM_Type = Core.Constants.DocManagerCodes.Shipment;
			Parent.SM_ParentFK = shipment.PK;
			Parent.SM_DB = 1;

			var requiredDocumentsParent = shipment["DocsAndCartage"] as IHaveRequiredDocuments;
			AddAndReturnNewRequiredDoc(requiredDocumentsParent, Core.Constants.RefDocTypes.CommercialInvoice);
			AddAndReturnNewRequiredDoc(requiredDocumentsParent, Core.Constants.RefDocTypes.HouseBill);
			AddAndReturnNewRequiredDoc(requiredDocumentsParent, Core.Constants.RefDocTypes.PackingList);
			AssertEquals("There should be 3 required documents", 3, requiredDocumentsParent.RequiredDocuments.Count);

			Factory.Save();

			foreach (JobRequiredDocument doc in requiredDocumentsParent.RequiredDocuments)
			{
				doc.EQ_DocNumber = doc.EQ_DocType + "001";
			}

			var storageDocsView = new StorageDocsCollectionViewBase(Parent, Parent.eDocs);
			var storageDocs1 = CreateStorageDocument<StorageDocs>(Core.Constants.RefDocTypes.CommercialInvoice,
				ZDateTime.Now, "_Test document.doc", "_Test description");
			var storageDocs2 = CreateStorageDocument<StorageDocs>(Core.Constants.RefDocTypes.HouseBill,
				ZDateTime.Now, "_Test document.doc", "_Test description");
			var storageDocs3 = CreateStorageDocument<StorageDocs>(Core.Constants.RefDocTypes.PackingList,
				ZDateTime.Now, "_Test document.doc", "_Test description");

			Parent.eDocs.Add(storageDocs1);
			Parent.eDocs.Add(storageDocs2);
			Parent.eDocs.Add(storageDocs3);
			AssertEquals("There should be three storage docs.", 3, storageDocsView.Count);

			AddAndLoadRequiredDocument(shipment, storageDocsView);

			foreach (JobRequiredDocument doc in requiredDocumentsParent.RequiredDocuments)
			{
				AssertEquals("Received date should be updated", ZDateTimeOffset.Now, doc.EQ_DateReceived);
				AssertEquals("EQ_DocNumber should be updated", doc.EQ_DocType + "001", doc.EQ_DocNumber);
			}
		}

		[UseSnapshotProtection(skipTransaction: true)]
		public void TestAddRequiredDocuments_Concurrency()
		{
			var shipment = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Forwarding.IForwardingShipment)));
			var storageMain = MasterFactory.New<StorageMain>();
			storageMain.SM_Type = Core.Constants.DocManagerCodes.Shipment;
			storageMain.SM_ParentFK = shipment.PK;
			storageMain.SM_DB = 1;

			var file = storageMain.eDocs.Factory.New<StorageDocs>();
			file.SC_SM = storageMain.PK;
			file.SC_DocType = Core.Constants.RefDocTypes.PackingList;
			file.SC_FileName = "test.txt";
			file.SC_Desc = "test desc";
			Factory.Save();

			var requiredDocumentsParent = shipment["DocsAndCartage"] as IHaveRequiredDocuments;
			AssertEquals(0, requiredDocumentsParent.RequiredDocuments.Count);

			var tasks = new List<Task>();
			var action = () =>
			{
				using (Db.DisposableActionForDbConnection())
				{
					using var dbConnection = Db.NewExtraConnectionToMainDb();
					dbConnection.BeginTransaction();
					var factory = new BusinessObjectFactory(dbConnection) { RefreshEnabled = true };
					var masterFactory = new DbBackendDocumentFactory(factory) { RefreshEnabled = true };
					var storageMain1 = masterFactory.Load<StorageMain>(storageMain.PK);

					var storageDocsView = new StorageDocsCollectionViewBase(storageMain1, storageMain1.eDocs);
					storageDocsView.AddRelatedRequiredDocuments();
					factory.Save();
					masterFactory.Save();
					dbConnection.CommitTransaction();
				}
			};

			// Start 50 threads to add required document with its own DB connection
			for (var i = 0; i < 50; i++)
			{
				tasks.Add(Task.Factory.StartNew(action));
			}

			Task.WaitAll(tasks.ToArray());

			requiredDocumentsParent.RequiredDocuments.Load();
			AssertEquals("There should be only 1 required document added", 1, requiredDocumentsParent.RequiredDocuments.Count);
		}

		public void TestNoDuplicateDocumentTrackingRecordsWhenCreatedInDifferentFactories()
		{
			var organisation = Factory.New<OrgHeader>();
			organisation.OH_Code = "org111";
			Factory.Save();
			organisation.RequiredDocuments.Load();

			var parentMain = MasterFactory.New<StorageMain>();
			parentMain.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			parentMain.SM_ParentFK = organisation.PK;
			parentMain.SM_DB = 1;

			StorageDocs doc = parentMain.Documents.AddNew();
			doc.SC_DocType = "AGI";
			MasterFactory.Save();
			AssertEquals(1, organisation.RequiredDocuments.Count);

			var factory1 = new BusinessObjectFactory();
			factory1.RefreshEnabled = false;
			var documentFactory1 = new DocumentFactoryProvider().GetFactory(factory1);
			var organisationInFactory1 = factory1.Load<OrgHeader>(organisation.PK);
			AssertNotNull(organisationInFactory1);
			organisationInFactory1.RequiredDocuments.Load();

			var parentMainInFactory1 = documentFactory1.Load<StorageMain>(parentMain.PK);
			StorageDocs doc1 = parentMainInFactory1.Documents.AddNew();
			doc1.SC_DocType = "ACV";

			var factory2 = new BusinessObjectFactory();
			var documentFactory2 = new DocumentFactoryProvider().GetFactory(factory2);
			var organisationInFactory2 = factory2.Load<OrgHeader>(organisation.PK);
			AssertNotNull(organisationInFactory2);
			organisationInFactory2.RequiredDocuments.Load();

			var parentMainInFactory2 = documentFactory2.Load<StorageMain>(parentMain.PK);
			StorageDocs doc2 = parentMainInFactory2.Documents.AddNew();
			doc2.SC_DocType = "ACV";

			//this defect happens because the record is created in another process (in service task). Here need to remove factory1 from PersistentFactoryCacheManager
			//so that the query cache will not be removed when calling Save() of documentFactory2 in order to replicate the defect.
			foreach (WeakReference reference in PersistentFactoryCacheManager.Instance.persistentFactoryWeakReferences)
			{
				if (reference.Target == organisationInFactory1.RequiredDocuments.Factory)
				{
					PersistentFactoryCacheManager.Instance.persistentFactoryWeakReferences.Remove(reference);
					break;
				}
			}

			documentFactory2.Save();
			documentFactory1.Save();

			var query = new ZQuery(JobRequiredDocumentSchema.EQ_DocType, "ACV") { ReLoadExistingRows = true };
			query.AddToFilter(JobRequiredDocumentSchema.EQ_ParentID, organisation.PK);
			var documents = new BusinessObjectFactory().Load<JobRequiredDocument>(query);
			AssertEquals("There should be only 1 document tracking record with type ACV", 1, documents.Length);

			AssertNotEquals("Doesn't try to save the duplicate record.", "Saving_Duplicate_Doc_Type", ErrorReporter.LastKeyReported);
		}

		[ExpectNoExceptions]
		public void TestDocumentTrackingRecordsDuplicationCheckIgoreCaseForDocType()
		{
			var organisation = Factory.New<OrgHeader>();
			organisation.OH_Code = "org111";
			Factory.Save();
			organisation.RequiredDocuments.Load();

			var parentMain = MasterFactory.New<StorageMain>();
			parentMain.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			parentMain.SM_ParentFK = organisation.PK;
			parentMain.SM_DB = 1;

			StorageDocs doc = parentMain.Documents.AddNew();
			doc.SC_DocType = "AGI";
			MasterFactory.Save();
			AssertEquals(1, organisation.RequiredDocuments.Count);

			var factory1 = new BusinessObjectFactory();
			factory1.RefreshEnabled = false;
			var documentFactory1 = new DocumentFactoryProvider().GetFactory(factory1);
			var organisationInFactory1 = factory1.Load<OrgHeader>(organisation.PK);
			AssertNotNull(organisationInFactory1);
			organisationInFactory1.RequiredDocuments.Load();

			var parentMainInFactory1 = documentFactory1.Load<StorageMain>(parentMain.PK);
			StorageDocs doc1 = parentMainInFactory1.Documents.AddNew();
			doc1.SC_DocType = "acv";

			var factory2 = new BusinessObjectFactory();
			var documentFactory2 = new DocumentFactoryProvider().GetFactory(factory2);
			var organisationInFactory2 = factory2.Load<OrgHeader>(organisation.PK);
			AssertNotNull(organisationInFactory2);
			organisationInFactory2.RequiredDocuments.Load();

			var parentMainInFactory2 = documentFactory2.Load<StorageMain>(parentMain.PK);
			StorageDocs doc2 = parentMainInFactory2.Documents.AddNew();
			doc2.SC_DocType = "ACV";

			documentFactory2.Save();
			documentFactory1.Save();

			var query = new ZQuery(JobRequiredDocumentSchema.EQ_DocType, "ACV") { ReLoadExistingRows = true };
			query.AddToFilter(JobRequiredDocumentSchema.EQ_ParentID, organisation.PK);
			var documents = new BusinessObjectFactory().Load<JobRequiredDocument>(query);
			AssertEquals("There should be only 1 document tracking record with type ACV", 1, documents.Length);
			AssertNotEquals("Doesn't try to save the duplicate record.", "Saving_Duplicate_Doc_Type", ErrorReporter.LastKeyReported);
		}

		T CreateStorageDocument<T>(ZString docType, ZDateTime date, string fileName, string description) where T : StorageDocsBase
		{
			T storageDocs = Factory.New<T>();
			storageDocs.SC_SM = Parent.PK;
			storageDocs.SC_DocType = docType;
			storageDocs.SC_Date = date;
			storageDocs.SC_FileName = fileName;
			storageDocs.SC_Desc = description;

			return storageDocs;
		}

		void AddAndLoadRequiredDocument(BusinessObject shipment, StorageDocsCollectionViewBase storageDocsView)
		{
			storageDocsView.AddRelatedRequiredDocuments();
			((IHaveRequiredDocuments)shipment["DocsAndCartage"]).RequiredDocuments.Load();
		}

		public void TestFindDocByName()
		{
			StorageFile file1 = Parent.Files.AddNew();
			file1.SC_FileName = "hello";
			file1.SC_DataType = "DOC";

			StorageFile file2 = Parent.Files.AddNew();
			file2.SC_FileName = "goodbye";
			file2.SC_DataType = "MSG";

			AssertEquals("should find correct file", file1, Parent.Files.FindDocByName("hello", ".doc"));
			AssertEquals("should find correct file", file2, Parent.Files.FindDocByName("goodbye", ".msg"));
			AssertNull("should return null", Parent.Files.FindDocByName("hello", ".msg"));

			string veryLongName = "AReallyLongNameThatExtendsPastTheLimitOfSC_FilenameOnTheStorageDocsTableSoItMightCauseASilentExceptionAboutMaxlengthExceededIfYouTryToAssignSomethingToItAReallyLongNameThatExtendsPastTheLimitOfSC_FilenameOnTheStorageDocsTableSoItMightCauseASilentExceptionAboutMaxlengthExceededIfYouTryToAssignSomethingToItAReallyLongNameThatExtendsPastTheLimitOfSC_FilenameOnTheStorageDocsTableSoItMightCauseASilentExceptionAboutMaxlengthExceededIfYouTryToAssignSomethingToIt";
			string veryLongDataType = "AReallyLongDataTypeThatExtendsPastTheLimitOfSC_DataType";
			StorageFile file3 = Parent.Files.AddNew();
			file3.SC_FileName = veryLongName.Substring(0, StorageDocsSchema.SC_FileName.MaxLength);
			file3.SC_DataType = veryLongDataType.Substring(0, StorageDocsSchema.SC_DataType.MaxLength);

			AssertEquals("Should find correct file even though the values are truncated", file3, Parent.Files.FindDocByName(veryLongName, veryLongDataType));

			Parent.Files.ExcludeUnpublishedDocuments = true;
			StorageFile file4 = (StorageFile)Parent.eDocs.AddNew(typeof(StorageFile));
			file4.SC_FileName = "byebye";
			file4.SC_DataType = "MSG";
			file4.SC_IsPublished = false;
			Assert("User shouldn't see this the file", !Parent.Files.Contains(file4));
			AssertEquals("Still file should be found", file4, Parent.Files.FindDocByName("byebye", ".msg"));
			AssertEquals("File should not be found", null, Parent.Files.FindDocByName("byebye", ".msg", false));
		}

		#region IStorageDocsBaseCollection Members

		public void TestGetMostRecentEDoc()
		{
			StorageDocs systemGeneratedEDoc = CreateNewDocument(Parent, "ABC", new ZDateTime(2004, 11, 1), new ZBlob(), true, false, false);
			StorageDocs nonSystemGeneratedEDoc = CreateNewDocument(Parent, "ABC", new ZDateTime(2004, 10, 1), new ZBlob(), false, false, false);
			StorageDocs earlierEDoc = CreateNewDocument(Parent, "ABC", new ZDateTime(2004, 9, 1), new ZBlob(), false, false, false);
			AssertEquals("GetMostRecentEDoc()", systemGeneratedEDoc, ((IStorageDocsBaseCollection)Parent.eDocs).GetMostRecentEDoc("ABC"));
		}

		public void TestContainsDocType()
		{
			var eDoc = Parent.Files.AddNew();
			eDoc.SC_DocType = "ABC";
			AssertEquals(true, ((IStorageDocsBaseCollection)Parent.Files).ContainsDocType("ABC"));

			eDoc.SC_IsDeleted = true;
			AssertEquals(false, ((IStorageDocsBaseCollection)Parent.Files).ContainsDocType("ABC"));
		}

		public void TestGetMostRecentEDoc_NotDeleted()
		{
			StorageDocs deletedFirst = CreateNewDocument(Parent, "ABC", new ZDateTime(2004, 11, 1), new ZBlob(), true, true, false);
			StorageDocs deletedSecond = CreateNewDocument(Parent, "ABC", new ZDateTime(2004, 10, 1), new ZBlob(), false, true, false);
			StorageDocs deletedThird = CreateNewDocument(Parent, "ABC", new ZDateTime(2004, 9, 1), new ZBlob(), false, true, false);

			AssertEquals("GetMostRecentEDoc will not return any documents that are deleted.", null, ((IStorageDocsBaseCollection)Parent.eDocs).GetMostRecentEDoc("ABC"));

			StorageDocs notDeletedSecond = CreateNewDocument(Parent, "ABC", new ZDateTime(2004, 10, 1), new ZBlob(), false, false, false);
			StorageDocs notDeletedThird = CreateNewDocument(Parent, "ABC", new ZDateTime(2004, 9, 1), new ZBlob(), false, false, false);

			AssertEquals("GetMostRecentEDoc will return the latest date of all docs not deleted", notDeletedSecond, ((IStorageDocsBaseCollection)Parent.eDocs).GetMostRecentEDoc("ABC"));
		}

		public void TestGetFromUniqueKey()
		{
			StorageDocs eDoc1 = CreateNewDocument(Parent, "ABC", new ZDateTime(2004, 11, 1), new ZBlob(), true, false, false);
			StorageDocs eDoc2 = CreateNewDocument(Parent, "ABC", new ZDateTime(2004, 10, 1), new ZBlob(), false, false, false);
			AssertEquals(eDoc2.PK, ((IStorageDocsBaseCollection)Parent.eDocs).GetFromUniqueKey(eDoc2.PK.ToGuid()).UniqueKey);
			AssertEquals(eDoc1.PK, ((IStorageDocsBaseCollection)Parent.eDocs).GetFromUniqueKey(eDoc1.PK.ToGuid()).UniqueKey);
		}

		public void TestAdd_IStorageDocsBaseCollection()
		{
			StorageDocs document = CreateNewDocument(Parent, "ABC", new ZDateTime(2004, 11, 1), new ZBlob(), true, true, false);
			IStorageDocsBaseCollection collection = Parent.eDocs;

			collection.Add(document);
			AssertEquals("There should be 1 document added to the collection", 1, collection.Count);
		}

		public void TestRemove_IStorageDocsBaseCollection()
		{
			StorageDocs document = CreateNewDocument(Parent, "ABC", new ZDateTime(2004, 11, 1), new ZBlob(), true, true, false);
			IStorageDocsBaseCollection collection = Parent.eDocs;

			collection.Add(document);
			AssertEquals("There should be 1 document added to the collection", 1, collection.Count);

			collection.Remove(document);
			AssertEquals("The document should be removed", 0, collection.Count);
		}

		public void TestContains()
		{
			StorageDocs document = CreateNewDocument(Parent, "ABC", new ZDateTime(2004, 11, 1), new ZBlob(), true, true, false);
			IStorageDocsBaseCollection collection = Parent.eDocs;
			collection.Add(document);
			AssertEquals("Collection now contains the document", true, collection.Contains(document));
		}

		public void TestIndexer()
		{
			StorageDocs document = CreateNewDocument(Parent, "ABC", new ZDateTime(2004, 11, 1), new ZBlob(), true, true, false);
			IStorageDocsBaseCollection collection = Parent.eDocs;

			collection.Add(document);
			AssertNotNull(collection[0]);
		}

		#endregion

		#region Implementation

		StorageMain Parent
		{
			get
			{
				if (fParent == null)
				{
					fParent = MasterFactory.New<StorageMain>();
				}
				return fParent;
			}
		}
		StorageMain fParent;

		StorageDocs CreateNewDocument(StorageMain parent, string docType, ZDateTime lastEdited, ZBlob imageData, bool isSystemGenerated, bool isDeleted, bool isPublished)
		{
			StorageDocs document = parent.Documents.AddNew();
			document.SC_DocType = docType;
			document.SC_Date = lastEdited;
			document.SC_ImageData = imageData;
			document.SC_IsSystemGenerated = isSystemGenerated;
			document.SC_IsDeleted = isDeleted;
			document.SC_IsPublished = isPublished;
			return document;
		}

		JobRequiredDocument AddAndReturnNewRequiredDoc(IHaveRequiredDocuments requiredDocumentsParent, ZString documentType)
		{
			var doc = requiredDocumentsParent.RequiredDocuments.AddNew();
			doc.EQ_DocCategory = Core.Constants.ReferenceTypes.SupplyChainLogistics;
			doc.EQ_DocType = documentType;
			doc.EQ_DocUsage = JobRequiredDocument.DocUsage.Both;
			doc.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.OncePerShipment;
			return doc;
		}

		DocumentFactory MasterFactory
		{
			get { return (DocumentFactory)Factory; }
		}

		protected override BusinessObjectFactory NewFactory()
		{
			return new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
		}

		protected override StorageDocsCollectionViewBase GetCollectionToTest()
		{
			return Parent.eDocsView;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return StorageDocs.New_DEBUG(MasterFactory);
		}

		#endregion

		#region Setup

		protected override void SetUp()
		{
			base.SetUp();
			using var auxConnection = Db.NewAdminConnection();
			AdoTestUtils.CreateDbIfNotExists(auxConnection, storageDocsDataBaseName);
		}

		protected override void OnAfterBaseTestCaseRunBare()
		{
			using (var adminConnection = Db.NewAdminConnection())
			{
				AdoTestUtils.DropDbIfExists(adminConnection, storageDocsDataBaseName);
			}
			base.OnAfterBaseTestCaseRunBare();
		}

		readonly string storageDocsDataBaseName = new DocManagerDBHelperTestClass().GetDatabaseName(1);

		#endregion
	}
}
