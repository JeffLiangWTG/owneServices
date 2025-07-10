using System;
using System.Data;
using System.IO;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.DeliveryMethods;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.Business.Test
{
	[TestedType(typeof(StorageDocs))]
	public class StorageDocsTest : StorageDocsBaseTest
	{
		#region TestCaseOverrides

		protected override BusinessObject GetNewBusinessObject()
		{
			return StorageDocs.NewWithParentWithoutFK_DEBUG(MasterFactory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return StorageDocs.NewWithParent_DEBUG(MasterFactory);
		}

		protected override BusinessObjectFactory NewFactory()
		{
			return MasterFactory;
		}

		#endregion

		#region Public Methods

		public void TestSC_ImageDataWithNullValue()
		{
			StorageDocs document = MasterFactory.NewWithParent(typeof(StorageDocs)) as StorageDocs;
			document.SC_ImageData = null;
			AssertEquals("SC_ImageDataWithNullValue", true, document.SC_ImageData.IsEmpty);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSC_ImageData()
		{
			string onePageImageFile_1 = Path.Combine(BaseSourcePath, TestDocsHelper.TestDocsPath, @"compressed.jpg");
			string onePageImageFile_2 = Path.Combine(BaseSourcePath, TestDocsHelper.TestDocsPath, @"NotCompressed.tif");
			StorageDocs document1 = MasterFactory.NewWithParent(typeof(StorageDocs)) as StorageDocs;
			StorageDocs document2 = MasterFactory.NewWithParent(typeof(StorageDocs)) as StorageDocs;
			document1.SC_ImageData = DocumentUtilities.GetFileAsBytes(onePageImageFile_1);
			document2.SC_ImageData = DocumentUtilities.GetFileAsBytes(onePageImageFile_2);

			AssertEquals("Size should equal", document1.SC_ImageData, DocumentUtilities.GetFileAsBytes(onePageImageFile_1));
			AssertEquals("Size should not equal", document2.SC_ImageData, DocumentUtilities.GetFileAsBytes(onePageImageFile_2));
		}

		public void TestGetModuleID()
		{
			AssertEquals(ModuleIDs.Organisation, Document.ModuleID);

			StorageMain parentMain = MasterFactory.New<StorageMain>();
			parentMain.SM_Type = Core.Constants.DocManagerCodes.Shipment;

			Document.SC_SM = parentMain.PK;

			AssertEquals(ModuleIDs.JobShipment, Document.ModuleID);
		}

		#endregion

		#region Overrides

		public void TestCopyPersistentValuesFromCreatesNoErrors()
		{
			StorageMain parentMain = MasterFactory.New<StorageMain>();
			parentMain.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			parentMain.SM_ParentFK = Org.PK;

			StorageDocs newDocument = StorageDocs.NewWithParent_DEBUG(MasterFactory);
			Document.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
			Document.SC_Desc = "abcdefg";
			Document.SC_SM = parentMain.PK;
			Document.SC_Date = ZDateTime.Now;

			newDocument.CopyPersistentValuesFrom(Document);

			Assert("New doc shouldn't have any errors", !newDocument.HasErrors);
		}

		public void TestCopyPersistentValuesFrom()
		{
			StorageMain parentMain = MasterFactory.New<StorageMain>();
			parentMain.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			parentMain.SM_ParentFK = Org.PK;

			Document.SC_DocType = Core.Constants.RefDocTypes.AirFreightManifest;
			Document.SC_Desc = "ABCDE";
			Document.SC_SM = parentMain.PK;

			StorageDocs newDocument = StorageDocs.NewWithParent_DEBUG(MasterFactory);
			newDocument.CopyPersistentValuesFrom(Document);

			AssertEquals("DocType should have copied", Document.SC_DocType, newDocument.SC_DocType);
			AssertEquals("Desc should have copied, it should not get reset from the doc type (because the ", Document.SC_Desc, newDocument.SC_Desc);
		}

		public void TestCopyPersistentValuesFrom_DoesNotGenerateErrorsForValidDocType()
		{
			BusinessObject shipment = (BusinessObject)Factory.New<Enterprise.Integration.Freight.ICommonShipment>();

			StorageMain parentMain = MasterFactory.New<StorageMain>();
			parentMain.SM_Type = Core.Constants.DocManagerCodes.Shipment;
			parentMain.SM_ParentFK = shipment.PK;

			Document.SC_SM = parentMain.PK;
			Document.SC_DocType = Core.Constants.RefDocTypes.AgentsInstruction;
			Document.SC_Desc = "ABCDE";

			AssertEquals("Should not be any errors for the doc type", false, Document.SC_DocTypeInfo.HasErrors());

			Document.SC_SM = ZGuid.Empty;
			StorageDocs newDocument = MasterFactory.New<StorageDocs>();
			newDocument.SC_SM = parentMain.PK;

			newDocument.CopyPersistentValuesFrom(Document);

			AssertEquals("New document should not have any errors for the doc type", false, newDocument.SC_DocTypeInfo.HasErrors());
		}

		public void TestCopyPersistentValuesFrom_DoesNotCopyIfSystemGenerated()
		{
			RefDocType newDocType = MasterFactory.New<RefDocType>();
			newDocType.RT_DocType = "ZZZ";
			newDocType.RT_ReferenceType = Core.Constants.ReferenceTypes.ClientSupplierRelationship;
			newDocType.RT_Desc = "Test Description";
			newDocType.RT_IsActive = true;
			newDocType.RT_IsPublished = true;
			newDocType.RT_SaveVersions = true;

			Factory.Save();

			StorageDocs testDocument = (StorageDocs)GetNewTestBizO(MasterFactory, Core.Constants.DocManagerCodes.Organisation);
			testDocument.SC_DocType = "ZZZ";
			AssertEquals("Valid doctype, description should update properly", testDocument.SC_Desc, newDocType.RT_Desc);
			Assert("Valid doctype, IsPublished should update properly", testDocument.SC_IsPublished);

			testDocument.SC_Desc = "blah";
			StorageDocs docToCopyTo = MasterFactory.New<StorageDocs>();
			docToCopyTo.CopyPersistentValuesFrom(testDocument);
			AssertEquals("Copy should not set the Description back to the default for the DocType", "blah", docToCopyTo.SC_Desc);

			docToCopyTo = MasterFactory.New<StorageDocs>();
			testDocument.SC_IsSystemGenerated = true;
			docToCopyTo.CopyPersistentValuesFrom(testDocument);
			AssertEquals("Copy should keep the same description from the TestDocument, because it was system generated", testDocument.SC_Desc, docToCopyTo.SC_Desc);
		}

		public void TestCopyPersistentValuesFrom_CopiesCompanyBranchDepartmentCorrectly()
		{
			var company1 = Factory.New<GlbCompany>();
			company1.GC_Code = "C1";
			company1.GC_Name = "C1";

			var branch1 = Factory.New<GlbBranch>();
			branch1.GB_Code = "B1";
			branch1.GB_BranchName = "Branch 1";
			branch1.GB_GC = company1.PK;

			var department1 = Factory.New<GlbDepartment>();
			department1.GE_Code = "D1";
			department1.GE_Desc = "Department 1";

			Factory.Save();

			StorageMain parentMain = MasterFactory.New<StorageMain>();
			parentMain.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			parentMain.SM_ParentFK = Org.PK;

			Document.SC_DocType = Core.Constants.RefDocTypes.AirFreightManifest;
			Document.SC_SM = parentMain.PK;

			Document.SC_GC_Company = company1.PK;
			Document.SC_GB_Branch = branch1.PK;
			Document.SC_GE_Department = department1.PK;

			StorageDocs newDocument = StorageDocs.NewWithParent_DEBUG(MasterFactory);
			newDocument.CopyPersistentValuesFrom(Document);

			AssertEquals("Doesn't copy Department information correctly", department1.GE_Code, newDocument.DepartmentCode);
			AssertEquals("Doesn't copy Company information correctly", company1.GC_Code, newDocument.CompanyCode);
			AssertEquals("Doesn't copy Branch information correctly", branch1.GB_Code, newDocument.BranchCode);
		}

		public void TestReadonlyOnFirstAccessWithValidSM_Type()
		{
			StorageDocs newDoc = StorageDocs.NewWithParent_DEBUG(MasterFactory);
			BusinessObject documentParent = MasterFactory.NewWithValidTestData(ObjectFactory.GetType(typeof(Forwarding.IForwardingShipment)));
			newDoc.ParentMain.SM_ParentFK = documentParent.PK;
			newDoc.ParentMain.SM_Type = Core.Constants.DocManagerCodes.Shipment;

			AssertEquals("Before anything is done, DocType should not be readonly", false, newDoc.SC_DocTypeInfo.ReadOnly);

			Assert("After setting reftype, correct RefTypeData loaded", AssemblyDataLookup.GetAssemblyDataFromDocManagerCode(Core.Constants.DocManagerCodes.Shipment).Equals(newDoc.OwnerAssemblyData));
			AssertEquals("After setting reftype, DocType should not be readonly", false, newDoc.SC_DocTypeInfo.ReadOnly);
		}

		public void TestReadonlyOnFirstAccessWithInvalidSM_Type()
		{
			StorageDocs newDoc = StorageDocs.NewWithParent_DEBUG(MasterFactory);
			BusinessObject documentParent = MasterFactory.NewWithValidTestData(ObjectFactory.GetType(typeof(Forwarding.IForwardingShipment)));
			newDoc.ParentMain.SM_ParentFK = documentParent.PK;
			newDoc.ParentMain.SM_Type = Core.Constants.DocManagerCodes.Shipment;

			AssertEquals("Doctype should not be readonly", false, newDoc.SC_DocTypeInfo.ReadOnly);

			newDoc.ParentMain.SM_Type = "AB";
			AssertEquals("After setting with invalid RefType, Doctype should be readonly", true, newDoc.SC_DocTypeInfo.ReadOnly);
		}

		public void TestReadonlyOnFirstAccessWithEmptySM_Type()
		{
			StorageDocs newDoc = StorageDocs.NewWithParent_DEBUG(MasterFactory);
			BusinessObject documentParent = MasterFactory.NewWithValidTestData(ObjectFactory.GetType(typeof(Forwarding.IForwardingShipment)));
			newDoc.ParentMain.SM_ParentFK = documentParent.PK;
			newDoc.ParentMain.SM_Type = Core.Constants.DocManagerCodes.Shipment;

			AssertEquals("Doctype should not be readonly", false, newDoc.SC_DocTypeInfo.ReadOnly);

			newDoc.ParentMain.SM_Type = "";
			AssertEquals("After setting an empty RefType, Doctype should be readonly", true, newDoc.SC_DocTypeInfo.ReadOnly);
		}

		public void TestOnLoadedOfAPrivateDocumentTypeDoesntDeleteTheType()
		{
			OrgHeader org = MasterFactory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "d"));
			StorageMain parentMain = MasterFactory.New<StorageMain>();
			parentMain.SM_Type = Core.Constants.DocManagerCodes.Shipment;
			parentMain.SM_ParentFK = org.PK;
			parentMain.SM_DB = 1;

			StorageDocs savedDoc = StorageDocs.New_DEBUG(MasterFactory);
			savedDoc.SC_DocType = Core.Constants.RefDocTypes.InternallyCreatedPrivateDocument;
			savedDoc.SC_SM = parentMain.PK;

			MasterFactory.Save();

			AssertEquals("Saved doc type is PRV", Core.Constants.RefDocTypes.InternallyCreatedPrivateDocument, savedDoc.SC_DocType);
			Assert("Saved doc has no errors", !savedDoc.SC_DocTypeInfo.HasErrors());
			Assert("Saved doc description is not blank", !savedDoc.SC_Desc.IsEmpty);

			StorageDocs retrievedDoc = MasterFactory.Load<StorageDocs>(savedDoc.PK);
			AssertEquals("Retrieved Doc DocType is PRV", Core.Constants.RefDocTypes.InternallyCreatedPrivateDocument, retrievedDoc.SC_DocType);
			Assert("Retrieved Doc has no errors", !retrievedDoc.SC_DocTypeInfo.HasErrors());
			Assert("Retrieved Doc description is not blank", !retrievedDoc.SC_Desc.IsEmpty);
		}

		public void TestOnFactorySavingIfDocumentsAreAllocated()
		{
			StorageMain allocatedMain = MasterFactory.New<StorageMain>();
			allocatedMain.SM_ParentFK = Org.PK;
			allocatedMain.SM_Type = Core.Constants.DocManagerCodes.Shipment;
			allocatedMain.SM_DB = 1;

			StorageDocs doc = allocatedMain.Documents.AddNew();
			doc.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
			doc.SC_Desc = "QQQQQQQQQQQQQQQQQQQQ";

			MasterFactory.Save();

			Assert("doc now saved in db", doc.IsInDatabase);
			AssertEquals("Doc's SC_SM fk is still valid", doc.SC_SM, allocatedMain.PK);

			var anotherFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory()).GetFactory(1);
			StorageDocs retrievedDoc = anotherFactory.Load<StorageDocs>(doc.PK);
			AssertEquals("Retrieved doc has the parent's type from OnLoaded", Core.Constants.DocManagerCodes.Shipment, retrievedDoc.ParentMain.SM_Type);
		}

		public void TestOnSaving_WhenDocTypeHasChanges_ShouldUpdateDDIEvent()
		{
			var parent = MasterFactory.NewWithValidTestData<StorageMain>();
			var owner = MasterFactory.NewWithValidTestData<OrgHeader>();
			parent.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			parent.SM_ParentFK = owner.PK;
			var file = parent.Files.AddNew();
			file.SC_DocType = "CIV";
			file.EnableAddEventLogsForNewDocument();
			MasterFactory.Save();
			var logs = owner.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, StorageDocsBase.CreateReference(file.PK, "CIV", "")));
			AssertEquals("A DDI event for CIV DocType should be logged.", 1, logs.Length);
			Assert("DDI Event should not be cancelled.", !logs[0].IsCancelled);
			file.SC_DocType = "ACV";

			MasterFactory.Save();
			var oldLogs = owner.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, StorageDocsBase.CreateReference(file.PK, "CIV", "")));
			var newLogs = owner.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, StorageDocsBase.CreateReference(file.PK, "ACV", "")));
			AssertEquals(1, oldLogs.Length);
			Assert("Old log should be cancelled.", oldLogs[0].IsCancelled);
			AssertEquals(1, newLogs.Length);
			Assert("New log should not be cancelled.", !newLogs[0].IsCancelled);
		}

		public void TestOnSaving_WhenDocSourceHasChanges_ShouldUpdateDDIEvent()
		{
			using (SystemDataRegistry.Instance.DDIDocumentSource.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var source1 = Factory.NewWithValidTestData<RefDocSource>();
				source1.RDS_Code = "ABZ";
				var source2 = Factory.NewWithValidTestData<RefDocSource>();
				source2.RDS_Code = "DEX";
				Factory.Save();

				var parent = MasterFactory.NewWithValidTestData<StorageMain>();
				var owner = MasterFactory.NewWithValidTestData<OrgHeader>();
				parent.SM_Type = Core.Constants.DocManagerCodes.Organisation;
				parent.SM_ParentFK = owner.PK;
				var file = parent.Files.AddNew();
				file.SC_DocType = "ACV";
				file.SC_RDS_NKDocSource = "ABZ";
				file.DocType.RT_SE_NKDocumentReceivedEvent = "AID";
				file.EnableAddEventLogsForNewDocument();
				MasterFactory.Save();
				var dDILogs = owner.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, StorageDocsBase.CreateReference(file.PK, "ACV", "ABZ")));
				var aIDLogs = owner.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, file.PK.ToString()).AddToFilter(StmALogSchema.SL_SE_NKEvent, "AID"));
				AssertEquals("A DDI event for ABZ DocSource should be logged.", 1, dDILogs.Length);
				AssertEquals("An AID event should be logged.", 1, aIDLogs.Length);
				Assert("DDI Event should not be cancelled.", !dDILogs[0].IsCancelled);
				file.SC_RDS_NKDocSource = "DEX";

				MasterFactory.Save();
				var oldLogs = owner.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, StorageDocsBase.CreateReference(file.PK, "ACV", "ABZ")));
				var newLogs = owner.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, StorageDocsBase.CreateReference(file.PK, "ACV", "DEX")));
				AssertEquals(1, oldLogs.Length);
				Assert("Old log should be cancelled.", oldLogs[0].IsCancelled);
				AssertEquals(1, newLogs.Length);
				Assert("New log should not be cancelled.", !newLogs[0].IsCancelled);
			}
		}

		public void TestIsTifFile()
		{
			StorageDocs doc = StorageDocs.NewWithParent_DEBUG(MasterFactory);
			AssertEquals("StorageDocs should always return true for IsTifFile", true, doc.IsImageFile);
		}

		#endregion

		#region List properties

		public void TestSC_FormCategory_ListWithAllocatedDocument()
		{
			StorageDocs allocatedDocument = StorageDocs.NewWithParentWithoutFK_DEBUG(FactoryOne);
			StorageMain allocatedParent = allocatedDocument.ParentMain;
			allocatedParent.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			allocatedParent.SM_ParentFK = Org.PK;

			MasterFactory.Save();

			AssertEquals("Precondition: Document is allocated", true, allocatedDocument.IsAllocated);

			AssertEquals("Document's SC_FormCategory list will be the same number of elements in the assembly data lookup",
				AssemblyDataLookup.DocManagerCodes.Count, allocatedDocument.SC_FormCategory_List.Count);

			AssertEquals("Document's SC_FormCategory list is the same as the assembly data lookup list",
				AssemblyDataLookup.DocManagerCodes, allocatedDocument.SC_FormCategory_List);
		}

		#endregion

		#region Properties

		public void TestWasOpenInExternalEditorDefault()
		{
			StorageDocs doc = MasterFactory.New<StorageDocs>();
			Assert(!doc.WasOpenInExternalEditor);
		}

		public void TestSC_IsDeleted()
		{
			StorageMain main = MasterFactory.New<StorageMain>();
			main.SM_DB = 1;
			main.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			main.SM_ParentFK = MasterFactory.LoadTop1<OrgHeader>(new ZQuery()).PK;

			StorageDocs newDoc = main.Documents.AddNew();
			newDoc.Validation.ValidateSC_DocType();

			Assert("New document added to a parent main collection has errors", newDoc.HasErrors);

			newDoc.SC_IsDeleted = true;
			Assert("New document once deleted no longer has errors, validation is run over again", !newDoc.HasErrors);
		}

		public void TestAbleToSaveAtVariousStagesOfDataInput()
		{
			StorageDocs newDocument = StorageDocs.NewWithParent_DEBUG(MasterFactory);
			Assert("No errors on brand new document, can still save", !newDocument.HasErrors);

			newDocument.ParentMain.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			Assert("Still no errors, can still save", !newDocument.HasErrors);

			newDocument.ParentMain.SM_Type = "";
			Assert("Should revert to UNA, but can still save", !newDocument.HasErrors);

			newDocument.ParentMain.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			newDocument.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
			Assert("Ref and Doc types filled in, no ParentFK yet, but still valid", !newDocument.HasErrors);

			newDocument.ParentMain.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			newDocument.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
			newDocument.SC_Desc = "Misc doc";
			Assert("Ref and Doc types filled in, desc filled in, no ParentFK yet, but still valid", !newDocument.HasErrors);

			newDocument.ParentMain.SM_Type = "";
			Assert("RefType reverts to UNA, no error", !newDocument.HasErrors);

			newDocument.ParentMain.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			newDocument.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
			newDocument.SC_Desc = "Misc Doc";
			newDocument.ParentMain.SM_ParentFK = Org.PK;
			Assert("Everything filled in, no error", !newDocument.HasErrors);
		}

		public void TestSC_SaveVersionsReadonlyStatus()
		{
			RefDocType newDocType = MasterFactory.New<RefDocType>();
			newDocType.RT_ReferenceType = Core.Constants.ReferenceTypes.ClientSupplierRelationship;
			newDocType.RT_DocType = "AAA";
			newDocType.RT_SaveVersions = true;

			Document.SC_DocType = "AAA";
			Assert("Document save versions flag is true becase of doctype save versions flag", Document.SC_SaveVersions);
			Assert("Document save versions flag is readonly if true", Document.SC_SaveVersionsInfo.ReadOnly);

			newDocType.RT_SaveVersions = false;
			Document.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument; // reset
			Document.SC_DocType = "AAA";
			Assert("Document save versions flag is false becase of doctype save versions flag", !Document.SC_SaveVersions);
			Assert("Document save versions flag is not readonly", !Document.SC_SaveVersionsInfo.ReadOnly);
		}

		public void TestSC_IsPublishedReadonlyStatus()
		{
			RefDocType newDocType = MasterFactory.New<RefDocType>();
			newDocType.RT_ReferenceType = Core.Constants.ReferenceTypes.ClientSupplierRelationship;
			newDocType.RT_DocType = "AAA";
			newDocType.RT_IsPublished = true;
			newDocType.RT_IsPublishUpdatable = true;

			Document.SC_DocType = "AAA";
			Assert("Document published flag is true becase of doctype published flag", Document.SC_IsPublished);
			Assert("Document published flag is not readonly", !Document.SC_IsPublishedInfo.ReadOnly);

			newDocType.RT_IsPublishUpdatable = false;
			Document.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument; // reset
			Document.SC_DocType = "AAA";
			Assert("Document published flag is true becase of doctype published flag", Document.SC_IsPublished);
			Assert("Document published flag is readonly because published is not updatable", Document.SC_IsPublishedInfo.ReadOnly);

			newDocType.RT_IsPublished = false;
			Document.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument; // reset
			Document.SC_DocType = "AAA";
			Assert("Document published flag is false becase of doctype published flag", !Document.SC_IsPublished);
			Assert("Document published flag is readonly because published is not updatable", Document.SC_IsPublishedInfo.ReadOnly);

			newDocType.RT_IsPublishUpdatable = true;
			Document.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument; // reset
			Document.SC_DocType = "AAA";
			Assert("Document published flag is false becase of doctype published flag", !Document.SC_IsPublished);
			Assert("Document published flag is not readonly because published is updatable", !Document.SC_IsPublishedInfo.ReadOnly);
		}

		#endregion

		public void TestSaveThisVersionOnDocType()
		{
			RefDocType docType = MasterFactory.New<RefDocType>();
			docType.RT_SaveVersions = true;
			docType.RT_DocType = "XYZ";
			docType.RT_ReferenceType = Core.Constants.ReferenceTypes.ClientSupplierRelationship;

			Document.SC_SaveVersions = false;
			Document.SC_DocType = "XYZ";
			Assert("Document's SaveThisVersion property should be true, because it's true on the DocType", Document.SaveThisVersion);

			Document.SC_SaveVersions = false;
			docType.RT_SaveVersions = false;
			Assert("Document's SaveThisVersion property should be false, because it's false on the DocType", !Document.SaveThisVersion);
		}

		public void TestSaveThisVersionOnStorageDocs()
		{
			RefDocType docType = MasterFactory.New<RefDocType>();
			docType.RT_SaveVersions = false;
			docType.RT_DocType = "XYZ";
			docType.RT_ReferenceType = Core.Constants.ReferenceTypes.ClientSupplierRelationship;

			Document.SC_DocType = "XYZ";
			Assert("Document's SaveThisVersion property should be false, because it's false on the DocType and on the document", !Document.SaveThisVersion);

			Document.SC_SaveVersions = true;
			Assert("Document's SaveThisVersion property should be true, because it's true on the DocType", Document.SaveThisVersion);
		}

		#region IDeliverable

		protected override void AssertDeliveryInfo(StorageDocsBase doc, DeliveryInfo info)
		{
			AssertEquals("AttachedFilename", doc.SC_FileName, info.AttachedFilename);
			AssertEquals("DeliveryFormat", DeliveryInfo.DeliveryFormats.TIFF, info.DeliveryFormat);
		}

		public override void TestDeliveryMode()
		{
			StorageDocs document = StorageDocs.NewWithParent_DEBUG(MasterFactory);
			AssertEquals("DeliveryMode defaulted to ALL", "EML,ELC,EPR,FAX,PRN,FTP,DND,EDC", document.DeliveryMode);
		}

		public override void TestGetSupportedDeliveryMethodsCore()
		{
			var doc = StorageDocs.NewWithParent_DEBUG(MasterFactory);
			AssertContainsExactElementsInAnyOrder(Core.Constants.ContactNotifyModes.All, doc.GetSupportedDeliveryMethods());

			var docType = Factory.NewWithValidTestData<RefDocType>();
			docType.RT_DocType = "ZZZ";
			docType.RT_Desc = "Test for Non HP Raw";
			docType.RT_IsActive = true;
			docType.RT_IsSystem = true;
			var menu = Factory.NewWithValidTestData<StmMenuItemBase>();
			var eDoc = menu.EDocsView.AddNew();
			eDoc.SX_RT_DocType = docType.PK;
			eDoc.SX_PrintCopyType = nameof(PrintCopyType.EML);
			((IDeliverable)doc).MenuItem = menu;
			doc.SC_DocType = "ZZZ";
			Factory.Save();
			AssertContainsExactElementsInAnyOrder(new string[] { Core.Constants.ContactNotifyModes.Email }, doc.GetSupportedDeliveryMethods());

			eDoc.SX_PrintCopyType = nameof(PrintCopyType.ALL);
			AssertContainsExactElementsInAnyOrder(Core.Constants.ContactNotifyModes.All, doc.GetSupportedDeliveryMethods());
		}

		public override void TestGetSupportedDeliveryMethodDispiteOfPrintCopyType()
		{
			var doc = StorageDocs.NewWithParent_DEBUG(MasterFactory);
			AssertContainsExactElementsInAnyOrder(Core.Constants.ContactNotifyModes.All, doc.GetSupportedDeliveryMethodDespiteOfPrintCopyType());

			var docType = Factory.NewWithValidTestData<RefDocType>();
			docType.RT_DocType = "ZZZ";
			docType.RT_Desc = "Test for Non HP Raw";
			docType.RT_IsActive = true;
			docType.RT_IsSystem = true;
			var menu = Factory.NewWithValidTestData<StmMenuItemBase>();
			var eDoc = menu.EDocsView.AddNew();
			eDoc.SX_RT_DocType = docType.PK;
			eDoc.SX_PrintCopyType = nameof(PrintCopyType.EML);
			((IDeliverable)doc).MenuItem = menu;
			doc.SC_DocType = "ZZZ";
			Factory.Save();
			AssertContainsExactElementsInAnyOrder(Core.Constants.ContactNotifyModes.All, doc.GetSupportedDeliveryMethodDespiteOfPrintCopyType());
		}

		public override void TestSupportsDeliveryMethod()
		{
			var doc = StorageDocs.NewWithParent_DEBUG(MasterFactory);
			AssertNull(((IDeliverable)doc).MenuItem);
			Assert(doc.SupportsDeliveryMethod(Core.Constants.ContactNotifyModes.Email));
			Assert(doc.SupportsDeliveryMethod(Core.Constants.ContactNotifyModes.Electronic));
			Assert(doc.SupportsDeliveryMethod(Core.Constants.ContactNotifyModes.EPrint));
			Assert(doc.SupportsDeliveryMethod(Core.Constants.ContactNotifyModes.Fax));
			Assert(doc.SupportsDeliveryMethod(Core.Constants.ContactNotifyModes.Print));
			Assert(doc.SupportsDeliveryMethod(Core.Constants.ContactNotifyModes.Ftp));
			Assert(doc.SupportsDeliveryMethod(Core.Constants.ContactNotifyModes.DoNotDeliver));

			var docType = Factory.NewWithValidTestData<RefDocType>();
			docType.RT_DocType = "ZZZ";
			docType.RT_Desc = "Test for Non HP Raw";
			docType.RT_IsActive = true;
			docType.RT_IsSystem = true;
			var menu = Factory.NewWithValidTestData<StmMenuItemBase>();
			var eDoc = menu.EDocsView.AddNew();
			eDoc.SX_RT_DocType = docType.PK;
			eDoc.SX_PrintCopyType = nameof(PrintCopyType.EML);
			((IDeliverable)doc).MenuItem = menu;
			doc.SC_DocType = "ZZZ";
			Factory.Save();

			AssertNotNull(((IDeliverable)doc).MenuItem);
			Assert(doc.SupportsDeliveryMethod(Core.Constants.ContactNotifyModes.Email));
			Assert(!doc.SupportsDeliveryMethod(Core.Constants.ContactNotifyModes.Electronic));
			Assert(!doc.SupportsDeliveryMethod(Core.Constants.ContactNotifyModes.EPrint));
			Assert(!doc.SupportsDeliveryMethod(Core.Constants.ContactNotifyModes.Fax));
			Assert(!doc.SupportsDeliveryMethod(Core.Constants.ContactNotifyModes.Print));
			Assert(!doc.SupportsDeliveryMethod(Core.Constants.ContactNotifyModes.Ftp));
			Assert(!doc.SupportsDeliveryMethod(Core.Constants.ContactNotifyModes.DoNotDeliver));

			eDoc.SX_PrintCopyType = nameof(PrintCopyType.ALL);
			Assert(doc.SupportsDeliveryMethod(Core.Constants.ContactNotifyModes.Email));
			Assert(doc.SupportsDeliveryMethod(Core.Constants.ContactNotifyModes.Electronic));
			Assert(doc.SupportsDeliveryMethod(Core.Constants.ContactNotifyModes.EPrint));
			Assert(doc.SupportsDeliveryMethod(Core.Constants.ContactNotifyModes.Fax));
			Assert(doc.SupportsDeliveryMethod(Core.Constants.ContactNotifyModes.Print));
			Assert(doc.SupportsDeliveryMethod(Core.Constants.ContactNotifyModes.Ftp));
			Assert(doc.SupportsDeliveryMethod(Core.Constants.ContactNotifyModes.DoNotDeliver));
		}

		public override void TestFileExtension()
		{
			var document = StorageDocs.NewWithParent_DEBUG(MasterFactory);
			AssertEquals("FileExtension fixed to TIF", OrgConstants.AttachmentType.TIF, ((IDeliverable)document).FileExtension);
		}

		public override void TestAllAvailableDeliveryModes()
		{
			StorageDocs document = StorageDocs.NewWithParent_DEBUG(MasterFactory);
			AssertEquals("AllAvailableDeliveryModes defaulted to ALL", "EML,ELC,EPR,FAX,PRN,FTP,DND,EDC", document.AllAvailableDeliveryModes);
		}

		public override void TestName()
		{
			AssertEquals("Name should be the document description", Document.SC_Desc, ((IDeliverable)Document).Name);
		}

		public override void TestBindingName()
		{
			AssertEquals("BindingName should be the document FileNameWithExtension", Document.SC_FileNameWithExtension, ((IDeliverable)Document).NameForBinding);
		}

		public void TestGetDeliveryInfo_JpgFile()
		{
			var document = StorageDocs.NewWithParent_DEBUG(MasterFactory);
			document.SC_ImageData = SmallJpgBytes;
			document.SC_DataType = Core.Constants.FileFormats.JPG;
			var deliverable = (IDeliverable)document;
			var info = deliverable.GetDeliveryInfo(false);
			AssertEquals("FileFormat should be TIF because of HackedFileExtensionOnlyForIDeliverable", Core.Constants.FileFormats.TIF, info.FileFormat);
			AssertEquals("DeliveryFormat", DeliveryInfo.DeliveryFormats.TIFF, info.DeliveryFormat);
		}

		#endregion

		public virtual void TestDeletingDocumentIfNotSavedAlsoDeletesItsParent()
		{
			StorageDocs newDocument = StorageDocs.NewWithParentWithoutFK_DEBUG(MasterFactory);
			StorageMain newDocumentParent = newDocument.ParentMain;

			Assert("NewDocument shouldn't be in database", !newDocument.IsInDatabase);
			Assert("NewDocumentParent shouldn't be in database", !newDocumentParent.IsInDatabase);

			newDocument.DeleteQuietly();

			Assert("Deleted flag set, but document shouldn't be deleted", !newDocument.IsDeleted);
			Assert("Parent shouldn't be deleted", !newDocumentParent.IsDeleted);
			MasterFactory.Save();

			Assert("Now document should be deleted", newDocument.IsDeleted);
			Assert("Now parent should be deleted", newDocumentParent.IsDeleted);
		}

		public void TestStorageDocsImplementsIDeliverableAndIeDoc()
		{
			AssertNotNull("Storagedocs implements IDeliverable - if not then some aspects of DocumentEngine.DocumentPack delivery will not work.", Document);
			AssertNotNull("StorageDocs implements IeDoc - if not then some aspects of DocumentEngine.DocumentPack merging and delivery will not work.", Document);
		}

		public void TestAddDocType()
		{
			RefDocType newDocType = Factory.New<RefDocType>();
			newDocType.RT_DocType = "000";
			newDocType.RT_ReferenceType = Core.Constants.DocManagerCodes.Organisation;
			newDocType.RT_Desc = "000 Doc Type";

			int initialMasterCount = Document.SC_DocType_List.Count;
			int initialGUICount = Document.SC_DocType_List.Count;

			Document.AddDocType(newDocType);

			AssertEquals("Should be one more document type in the list", initialMasterCount + 1, Document.SC_DocType_List.Count);
			AssertEquals("Should be one more document type in the list", initialGUICount + 1, Document.SC_DocType_List.Count);

			AssertEquals("The new doc type should be first in the list alphabetically", newDocType.RT_DocType, Document.SC_DocType_List[0].Code);
			AssertEquals("The new doc type should be first in the list alphabetically", newDocType.RT_DocType, Document.SC_DocType_List[0].Code);
		}

		public void TestSC_DescriptionForWeb()
		{
			Document.SC_FileName = "TestFileName1";
			AssertEquals("Description for web should be FileName", Document.SC_FileNameWithExtension, Document.SC_DescriptionForWeb);

			Document.SC_FileName = "TestFileName2";
			AssertEquals("Description for web should be FileName", Document.SC_FileNameWithExtension, Document.SC_DescriptionForWeb);
		}

		public override void TestEDocsFormat()
		{
			var document = MasterFactory.NewWithParent(typeof(StorageDocs)) as StorageDocs;
			document.SC_ImageData = SmallTifBytes;

			AssertEquals("TIF", document.EDocFormat);

			document.SC_ImageData = SmallJpgBytes;

			AssertEquals("JPG", document.EDocFormat);

			document.SC_ImageData = SmallGifBytes;

			AssertEquals("GIF", document.EDocFormat);

			document.SC_ImageData = SamplePdfBytes;

			AssertEquals("If SC_Image is not set to image content, EDocFormat is default to TIF", "TIF", document.EDocFormat);
		}

		OrgHeader CreateRandomOrg(BusinessObjectFactory factory)
		{
			OrgHeader org = factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = new ZString(Guid.NewGuid().ToString().Replace("-", "")).Left(OrgHeaderSchema.OH_Code.MaxLength);
			org.MainAddress.OA_Code = new ZString(Guid.NewGuid().ToString().Replace("-", "")).Left(OrgAddressSchema.OA_Code.MaxLength);
			return org;
		}

		public void TestOnFactorySavingConversionToJpegAndGif()
		{
			var factory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			var org1 = CreateRandomOrg(factory);
			var org2 = CreateRandomOrg(factory);
			var org3 = CreateRandomOrg(factory);
			var org4 = CreateRandomOrg(factory);
			var org5 = CreateRandomOrg(factory);

			var main1 = factory.NewWithValidTestData<StorageMain>();
			var main2 = factory.NewWithValidTestData<StorageMain>();
			var main3 = factory.NewWithValidTestData<StorageMain>();
			var main4 = factory.NewWithValidTestData<StorageMain>();
			var main5 = factory.NewWithValidTestData<StorageMain>();
			main1.SM_ParentFK = org1.PK;
			main2.SM_ParentFK = org2.PK;
			main3.SM_ParentFK = org3.PK;
			main4.SM_ParentFK = org4.PK;
			main5.SM_ParentFK = org5.PK;

			MasterFactory.Save();

			var doc1 = factory.New<StorageDocs>();
			doc1.SC_SM = main1.PK;
			doc1.SC_ImageData = SmallTifBytes;

			var doc2 = factory.New<StorageDocs>();
			doc2.SC_SM = main2.PK;
			doc2.SC_ImageData = SmallJpgBytes;

			var doc3 = factory.New<StorageDocs>();
			doc3.SC_SM = main3.PK;
			doc3.SC_ImageData = SmallGifBytes;

			var coloredTifBytes = resourceRetriever.Value.GetBytes("Enterprise.DocumentScanning.Business.Test.TestDocs.colored.tif");
			var doc4 = factory.New<StorageDocs>();
			doc4.SC_SM = main4.PK;
			doc4.SC_ImageData = coloredTifBytes;

			factory.Save();

			var factory1 = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			var assert1 = factory1.Load<StorageDocs>(doc1.PK);
			var assert2 = factory1.Load<StorageDocs>(doc2.PK);
			var assert3 = factory1.Load<StorageDocs>(doc3.PK);
			var assert4 = factory1.Load<StorageDocs>(doc4.PK);

			AssertEquals("not converted to jpeg as it is bw tiff", "TIF", assert1.EDocFormat);
			AssertEquals("stays jpeg", "JPG", assert2.EDocFormat);
			AssertEquals("not converted to jpeg from GIF", "GIF", assert3.EDocFormat);
			AssertEquals("not converted to jpeg from tif as it is small size", "TIF", assert4.EDocFormat);

			var pk = Guid.NewGuid();
			var sql = "INSERT INTO dbo.StorageDocs (SC_PK, SC_SM, SC_DocType, SC_Desc, SC_IsSystemGenerated, SC_ImageData, SC_Date, SC_SystemCreateTimeUtc, SC_SystemLastEditTimeUtc) VALUES (@PK, @FK, 'MSC', 'EDI (Sydney) - Commercial Invoice - S00001002', 'N', @Image, '2000-01-01', '2000-01-01', '2000-01-01')";
			DbCommand command;

			command = Db.Connection.Command(sql);// this is a test!
			command.AddParameter("@PK", SqlDbType.UniqueIdentifier, pk);
			command.AddParameter("@FK", SqlDbType.UniqueIdentifier, main5.PK.ToGuid());
			command.AddParameter("@Image", SqlDbType.VarBinary, coloredTifBytes);
			command.ExecuteNonQuery();

			var doc = factory1.Load<StorageDocs>(pk);
			AssertNotNull(doc);

			factory1.Save();

			var factory2 = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			var assert5 = factory2.Load<StorageDocs>(pk);
			AssertEquals("not converted to jpeg as it is bw tiff and is already in db", "TIF", assert5.EDocFormat);
		}

		public void TestConvertingToJPEGInSC_ImageData()
		{
			// This bmp resource is actually in PNG format.
			var pngBytes = resourceRetriever.Value.GetBytes("Enterprise.DocumentScanning.Business.Test.TestDocs.ABCDEF012345.bmp");
			var factory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			var doc1 = factory.New<StorageDocs>();
			doc1.SC_FileName = "ABCDEF012345";
			doc1.SC_DataType = "PNG";
			doc1.SC_ImageData = pngBytes;

			AssertEquals("Image should be converted to JPEG format", "JPG", doc1.EDocFormat);
			AssertEquals("Extension should change to JPG", "JPG", doc1.SC_DataType);

			using (DocManagerRegistry.Instance.ConvertImagesToJpeg.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				doc1.SC_DataType = "PNG";
				doc1.SC_ImageData = pngBytes;

				AssertEquals("Image should not be converted to JPEG format as registry setting is disabled", "PNG", doc1.EDocFormat);
				AssertEquals("Extension should stay as PNG", "PNG", doc1.SC_DataType);
			}
		}

		public void TestCopyPersistentValues_WhenImageDataIsSameAsLazyLoadingPlaceholderAndNotInDatabase_ShouldNotLoadFromDatabase()
		{
			var storageDocs1 = Factory.New<StorageDocs>();
			storageDocs1.SC_ImageData = new byte[] { 1, 2, 3 };

			var storageDocs2 = Factory.New<StorageDocs>();
			storageDocs2.SC_ImageData = new byte[] { 0, 0 };

			AssertEquals(DataRowState.Added, ((INeedRow)storageDocs2).Row.RowState);

			AssertNoExceptionThrown(() => storageDocs1.CopyPersistentValuesFrom(storageDocs2));
		}

		[ExpectNoExceptions]
		public void TestOnFactorySaving_DoesNotThrowInvalidObjectNameException()
		{
			var docType = MasterFactory.NewWithValidTestData<RefDocType>();
			docType.RT_ReferenceType = "ALL";
			Document.SC_DocType = docType.RT_DocType;
			MasterFactory.Save();
			docType.RT_SE_NKDocumentReceivedEvent = "ADD";
			Document.SC_DocType = Core.Constants.RefDocTypes.EntryPrint;
			MasterFactory.Save();
		}

		public void TestCustomEventOnDocTypeCanTriggerMilestone()
		{
			const string arbitraryEventCode = AutoEvents.MiscellaneousEventCode;

			var docType = Factory.NewWithValidTestData<RefDocType>();
			docType.RT_SE_NKDocumentReceivedEvent = arbitraryEventCode;
			docType.RT_LogMacro = "|TYP=MANUAL";
			docType.RT_ReferenceType = "ALL";

			var parent = Factory.NewWithValidTestData<OrgHeader>();
			var milestone = parent.WorkflowItems.Milestones.AddNew();
			((IBaseTrigger)milestone).TriggerEventCode = arbitraryEventCode;
			((IBaseTrigger)milestone).TriggerCondition = "RFP";
			((IBaseTrigger)milestone).TriggerConditionValue = "TYP=MANUAL";
			Factory.Save();

			var document = GetNewTestBizO(MasterFactory, Core.Constants.DocManagerCodes.Organisation);
			document.ParentMain.SM_ParentFK = parent.PK;
			document.SC_DocType = docType.RT_DocType;

			MasterFactory.Save();

			AssertNotEquals(ZDateTime.Empty, milestone.P9_ActualDate);
		}

		public void TestHackedFileExtensionOnlyForIDeliverable()
		{
			var storageDocs = Factory.New<StorageDocs>();
			storageDocs.SC_ImageData = SmallJpgBytes;
			storageDocs.SC_DataType = Core.Constants.FileFormats.JPG;
			storageDocs.SC_DocType = "MSC";
			AssertEquals("StorageDocs should return TIF for JPG images because of HackedFileExtensionOnlyForIDeliverable", Core.Constants.FileFormats.TIF, ((IDeliverable)storageDocs).FileExtension);
		}

		#region Implementation

		protected override StorageDocsBase GetNewTestBizO(NumberedBusinessObjectFactory factory)
		{
			return StorageDocs.NewWithParentWithoutFK_DEBUG(factory);
		}

		protected override StorageDocsBase GetNewTestBizOForSave(NumberedBusinessObjectFactory factory)
		{
			return StorageDocs.NewWithParent_DEBUG(factory);
		}

		protected override void SetupForFileOpen()
		{
			base.SetupForFileOpen();
			SystemDataRegistry.Instance.UseDefaultWindowsImageViewer.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
		}

		protected override StorageDocsBase GetObjectForTestingOpeningFile()
		{
			StorageDocsBase result = MasterFactory.NewWithParent(typeof(StorageDocsForTesting));
			result.ParentMain.SM_ParentFK = Factory.NewWithValidTestData<OrgHeader>().PK;
			result.ParentMain.SM_DB = 1;
			return result;
		}

		class StorageDocsForTesting : StorageDocs, IIsLaunched
		{
			public StorageDocsForTesting(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public bool IsLaunched { get; private set; }

			protected override void LaunchProcess()
			{
				IsLaunched = true;
			}
		}

		OrgHeader org1;
		OrgHeader org2;

		protected override void SetUp()
		{
			base.SetUp();

			ZQuery orgQuery = new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "W");
			org1 = MasterFactory.LoadTop1(typeof(OrgHeader), orgQuery) as OrgHeader;

			orgQuery = new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "U");
			org2 = MasterFactory.LoadTop1(typeof(OrgHeader), orgQuery) as OrgHeader;

			FactoryOne = MasterFactory.GetFactory(1);
			Parent = MasterFactory.New(typeof(StorageMain)) as StorageMain;
			Parent.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			Parent.SM_ParentFK = org1.PK;
			Parent.SM_DB = 1;
			Document = Parent.Documents.AddNew();
			Document.SC_Date = ZDateTime.Now;

			ZQuery filter = new ZQuery(OrgHeaderSchema.OH_FullName, SQLComparisonOperator.StartsWith, "R");
			Org = MasterFactory.LoadTop1(typeof(OrgHeader), filter) as OrgHeader;

			Document2 = StorageDocs.NewWithParent_DEBUG(MasterFactory);
			Parent2 = Document2.ParentMain;
			Parent2.SM_ParentFK = org2.PK;
			Parent2.SM_DB = 1;

			AllDocTypeFilter = new ZQuery(RefDocTypeSchema.RT_ReferenceType, Core.Constants.ReferenceTypes.All);
			AllDocTypeFilter.AddToFilter(JoinCondition.And, RefDocTypeSchema.RT_DocType, SQLComparisonOperator.NotEqual,
				Core.Constants.RefDocTypes.InternallyCreatedPrivateDocument);
			AllDocTypeFilter.AddToFilter(JoinCondition.And, RefDocTypeSchema.RT_DocType, SQLComparisonOperator.NotEqual,
				Core.Constants.RefDocTypes.InternallyCreatedPublicDocument);
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>(); //storage main requires parent.

			var doc = (StorageDocs)Factory.NewWithValidTestData(GetExpectedBusinessObjectType(), TestBusinessObjectKind.MinimumRequiredToSave | TestBusinessObjectKind.PopulateDependentCollections);
			doc.ParentMain.SM_ParentFK = org.PK;
			doc.ParentMain.SM_DB = 1;
			return doc;
		}

		NumberedBusinessObjectFactory FactoryOne;

		protected StorageDocs Document { get; set; }

		StorageDocs Document2 { get; set; }

		protected StorageMain Parent;
		StorageMain Parent2;
		protected OrgHeader Org;
		ZQuery AllDocTypeFilter;

		public override bool ExpectedTIF
		{
			get { return true; }
		}

		public override bool ExpectedJPG
		{
			get { return true; }
		}

		public override bool ExpectedTXT
		{
			get { return false; }
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());

		byte[] SmallTifBytes => resourceRetriever.Value.GetBytes("Enterprise.DocumentScanning.Business.Test.TestDocs.small.tif");

		byte[] SmallJpgBytes => resourceRetriever.Value.GetBytes("Enterprise.DocumentScanning.Business.Test.TestDocs.small.JPG");

		byte[] SmallGifBytes => resourceRetriever.Value.GetBytes("Enterprise.DocumentScanning.Business.Test.TestDocs.small.gif");

		byte[] SamplePdfBytes => resourceRetriever.Value.GetBytes("Enterprise.DocumentScanning.Business.Test.TestDocs.Sample.PDF");

		#endregion
	}
}
