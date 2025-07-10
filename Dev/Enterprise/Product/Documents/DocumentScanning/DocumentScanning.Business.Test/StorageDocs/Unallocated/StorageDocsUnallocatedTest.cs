using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.DeliveryMethods;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.Business.Test
{
	[TestedType(typeof(StorageDocsUnallocated))]
	sealed class StorageDocsUnallocatedTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<StorageDocsUnallocated>();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			DocumentFactory factoryForCreate = factory as DocumentFactory ?? Factory as DocumentFactory;

			return base.GetNewBusinessObjectForDeleteTest(factoryForCreate);
		}

		protected override BusinessObjectFactory NewFactory()
		{
			return new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public override void TestBizObjectFields()
		{
			base.TestBizObjectFields();
		}

		public void TestSettingTypeRecalculatesDescription()
		{
			Document.SM_Type = Core.Constants.DocManagerCodes.Unallocated;
			Document.SC_DocType = "MCD";
			Document.SC_Desc = "";
			Document.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			AssertEquals("Miscellaneous Customs Document", Document.SC_Desc);
		}

		public void TestSettingTypeClearsUniqueID()
		{
			var documentParent = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Warehouse.Integration.IWhsReceive)));
			Document.SM_Type = Core.Constants.DocManagerCodes.WarehouseReceiveDocket;
			Document.SC_ParentID = documentParent.PK;

			Document.SM_Type = Core.Constants.DocManagerCodes.WarehouseReceiveDocket;
			AssertEquals("Should *not* have cleared SC_ParentID.", documentParent.PK, Document.SC_ParentID);

			Document.SM_Type = Core.Constants.DocManagerCodes.WarehouseOrderDocket;
			AssertEquals("Should have cleared SC_ParentID.", ZGuid.Empty, Document.SC_ParentID);
		}

		public void TestIsAutoAllocatable()
		{
			Assert("Empty fields, doument is not allocatable", !Document.IsAutoAllocatable);

			Document.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			Document.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
			Document.SC_Desc = Core.Constants.RefDocTypeDescriptions.MiscellaneousDocument;
			Document.SC_ParentID = Org.PK;

			Assert("Valid FK to allocate to, document should be allocatable", Document.IsAutoAllocatable);

			Document.SC_ParentID = ZGuid.NewZGuid();
			Assert("Not a valid FK for item to allocate to, document should not be allocatable", !Document.IsAutoAllocatable);

			Document.SC_ParentID = Org.PK;
			Assert(Document.IsAutoAllocatable);
		}

		public void TestIsAutoAllocatableWithEmptyDescription()
		{
			using (Document.GetValidationSuspender())
			{
				Document.SC_DocType = "MSC";
				Document.SC_DataType = Core.Constants.ReferenceTypes.All;
				Document.SC_Desc = string.Empty;

				Assert("Document should be allocatable without description otherwise DMI service task cannot allocate MSC document types",
					Document.IsAutoAllocatable);
			}
		}

		public void TestSC_ParentIDInfo()
		{
			AssertEquals("Precondition", Document.SC_ParentIDInfo.ReadOnly, true);
			Document.SC_DataType = "SHP";
			AssertEquals(Document.SC_ParentIDInfo.ReadOnly, false);
			Document.SC_DataType = "";
			AssertEquals(Document.SC_ParentIDInfo.ReadOnly, true);
		}

		public void TestReadonlyOnFirstAccessWithValidSM_Type()
		{
			BusinessObject documentParent = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Forwarding.IForwardingShipment)));
			Document.SC_ParentID = documentParent.PK;
			Document.SM_Type = Core.Constants.DocManagerCodes.Shipment;

			AssertEquals("Before anything is done, DocType should not be readonly", false, Document.SC_DocTypeInfo.ReadOnly);

			Assert("After setting reftype, correct RefTypeData loaded", AssemblyDataLookup.GetAssemblyDataFromDocManagerCode(Core.Constants.DocManagerCodes.Shipment).Equals(Document.OwnerAssemblyData));
			AssertEquals("After setting reftype, DocType should not be readonly", false, Document.SC_DocTypeInfo.ReadOnly);
		}

		public void TestReadonlyOnFirstAccessWithInvalidSM_Type()
		{
			BusinessObject documentParent = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Forwarding.IForwardingShipment)));
			Document.SC_ParentID = documentParent.PK;
			Document.SM_Type = Core.Constants.DocManagerCodes.Shipment;

			AssertEquals("Doctype should not be readonly", false, Document.SC_DocTypeInfo.ReadOnly);

			Document.SM_Type = "AB";
			AssertEquals("After setting with invalid RefType, Doctype should be readonly", true, Document.SC_DocTypeInfo.ReadOnly);
		}

		public void TestSM_TypeViaSC_DataType()
		{
			AssertEquals("Precondition", Document.SC_DataType, "UNA");
			AssertEquals(Document.SM_Type, "UNA");

			Document.SC_DataType = "1";
			AssertEquals(Document.SC_DataType, Document.SM_Type);

			Document.SM_Type = "2";
			AssertEquals(Document.SM_Type, Document.SC_DataType);
		}

		public void TestSettingDataTypeToUNA()
		{
			AssertEquals("Precondition", Document.SC_DataType, "UNA");
			AssertEquals(Document.SM_Type, "UNA");

			Document.SM_Type = "ORG";
			Document.SC_DocType = "QUO";
			AssertEquals("ORG", Document.SM_Type);
			AssertEquals("QUO", Document.SC_DocType);
			AssertNotEquals(ZString.Empty, Document.SC_Desc);

			Document.SM_Type = "UNA";
			AssertEquals("UNA", Document.SM_Type);
			AssertEquals(ZString.Empty, Document.SC_DocType);
			AssertEquals(ZString.Empty, Document.SC_Desc);
		}

		public void TestIsAllocated()
		{
			Assert("Precondition", !Document.IsAllocated);
			Document.SC_ParentID = ZGuid.NewZGuid();
			Assert(Document.IsAllocated);
		}

		public void TestSC_SM()
		{
			Assert("Precondition", Document.SC_SM.IsEmpty);
			Document.SC_SM = ZGuid.NewZGuid();
			Assert("You can't assign it!", Document.SC_SM.IsEmpty);
		}

		public void TestSC_ParentID_List()
		{
			Document.SM_Type = Core.Constants.DocManagerCodes.Shipment;
			Assert("Collection should not be of NonPersistent type", !(Document.SC_ParentID_List is INonPersistentBusinessObjectCollection));
			Document.SM_Type = "ABC";
			Assert("Collection should be of NonPersistent type", Document.SC_ParentID_List is NonPersistentUnallocatedObjectCollection);
			Document.SM_Type = Core.Constants.DocManagerCodes.Product;
			Assert("Collection should be somethig else", !(Document.SC_ParentID_List is NonPersistentUnallocatedObjectCollection));
		}

		public void TestSC_ParentID_List_Consol()
		{
			Document.SM_Type = Core.Constants.DocManagerCodes.Consol;
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Forwarding.IForwardingConsol>(), Document.SC_ParentID_List.TypeOfElements);
		}

		public void TestSC_IsDeleted()
		{
			Document.SC_DataType = "bad";
			Document.Validation.ValidateSC_DataType();
			Assert("Should have errors", Document.HasErrors);

			Document.SC_IsDeleted = true;
			Assert("New document once deleted no longer has errors, validation is run over again", !Document.HasErrors);

			Document.SC_IsDeleted = false;
			Assert("Validation should re-run after setting to note deleted", Document.HasErrors);

			Document.SC_IsDeleted = true;
			Assert("New document once deleted no longer has errors, validation is run over again", !Document.HasErrors);
		}

		public void TestGetDeliveryInfo_PDFFile()
		{
			var document = Factory.New<StorageDocsUnallocated>();
			document.SC_ImageData = SamplePdfBytes;
			var deliverable = (IDeliverable)document;
			var info = deliverable.GetDeliveryInfo(false);
			AssertEquals("FileFormat", Core.Constants.FileFormats.PDF, info.FileFormat);
			AssertEquals("DeliveryFormat", DeliveryInfo.DeliveryFormats.File, info.DeliveryFormat);
		}

		public void TestSetAllocateStatus()
		{
			Assert("Empty fields, document is not allocatable", !Document.IsAutoAllocatable);

			BaseBarcode documentResult = null;
			AssertEquals("No suitable barcode detected", Document.SetAllocateStatus(documentResult));

			documentResult = new DocTypeBarcode(MasterFactory, "");
			AssertEquals("Barcode text missing/not found", Document.SetAllocateStatus(documentResult));

			documentResult = new DocTypeBarcode(MasterFactory, "a");
			AssertEquals("Barcode type unrecognized (not Shipment or Document Type)", Document.SetAllocateStatus(documentResult));

			documentResult = new DocTypeBarcode(MasterFactory, "^SHP=S00123456|");
			AssertEquals("Barcode lacks a Document Type", Document.SetAllocateStatus(documentResult));

			documentResult = new DocTypeBarcode(MasterFactory, "^DOC=CIV|");
			Document.SC_DocType = documentResult.DocType;
			AssertEquals("Barcode lacks a Document Manager Code", Document.SetAllocateStatus(documentResult));

			documentResult = new DocTypeBarcode(MasterFactory, "^SHP=S00123456;TST|");
			AssertEquals("Could not find related Job for Barcode", Document.SetAllocateStatus(documentResult));

			BusinessObject shipment = (BusinessObject)Factory.LoadTop1<Enterprise.Integration.Freight.ICommonShipment>(new ZQuery(JobShipmentSchema.JS_UniqueConsignRef, "S00123456"));

			if (shipment == null)
			{
				shipment = Factory.NewWithValidTestData(ObjectFactory.GetType<Enterprise.Integration.Freight.ICommonShipment>());
				shipment[JobShipmentSchema.JS_UniqueConsignRef] = "S00123456";
			}

			shipment[JobShipmentSchema.JS_TransportMode] = Core.Constants.TransportModes.Sea;
			shipment[JobShipmentSchema.JS_PackingMode] = "FCL";
			Factory.Save();

			documentResult = new DocTypeBarcode(MasterFactory, "^SHP=S00123456;TST|");

			Document.SM_Type = "";
			AssertEquals("eDocs Storage lacks a type", Document.SetAllocateStatus(documentResult));

			Document.SM_Type = Core.Constants.ReferenceTypes.Unallocated;
			AssertEquals("eDocs Storage's type is Unallocated", Document.SetAllocateStatus(documentResult));

			Document.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			AssertEquals("Description is empty", Document.SetAllocateStatus(documentResult));

			Document.SC_Desc = Core.Constants.RefDocTypeDescriptions.MiscellaneousDocument;
			Document.SC_DescMultilingual = Core.Constants.RefDocTypeDescriptions.MiscellaneousDocument;
			Document.SC_DataType = "invalid";
			AssertEquals("Validation errors prevent allocation", Document.SetAllocateStatus(documentResult));

			Document.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
			Document.SC_Desc = Core.Constants.RefDocTypeDescriptions.MiscellaneousDocument;
			Document.SC_DescMultilingual = Core.Constants.RefDocTypeDescriptions.MiscellaneousDocument;
			Document.SC_DataType = "SHP";
			AssertEquals("Allocatable", Document.SetAllocateStatus(documentResult));

			var allocationStatusNote = Document.AllocationStatusNote;
			AssertEquals(true, allocationStatusNote.HasLoadedNote);
			Document.Delete();
			AssertEquals(false, allocationStatusNote.HasLoadedNote);
		}

		public void TestNotesAreDeletedOnDelete()
		{
			var allocationStatusNote = Document.AllocationStatusNote;
			var allocationNotesNote = Document.AllocationNotesNote;
			AssertEquals(false, allocationStatusNote.HasLoadedNote);
			AssertEquals(false, allocationNotesNote.HasLoadedNote);

			Document.AllocationStatus = "a";
			Document.AllocationNotes = "b";
			AssertEquals(true, allocationStatusNote.HasLoadedNote);
			AssertEquals(true, allocationNotesNote.HasLoadedNote);
			AssertEquals("a", Document.AllocationStatus);
			AssertEquals("b", Document.AllocationNotes);
			AssertEquals("a", Document.AllocationStatusNote.Text);
			AssertEquals("b", Document.AllocationNotesNote.Text);

			Document.Delete();
			AssertEquals(false, allocationStatusNote.HasLoadedNote);
			AssertEquals(false, allocationNotesNote.HasLoadedNote);
			AssertEquals(true, string.IsNullOrEmpty(Document.AllocationStatus));
			AssertEquals(true, string.IsNullOrEmpty(Document.AllocationNotes));

			Document.AllocationStatus = "a";
			Document.AllocationNotes = "b";
			AssertEquals(true, allocationStatusNote.HasLoadedNote);
			AssertEquals(true, allocationNotesNote.HasLoadedNote);
			AssertEquals("a", Document.AllocationStatus);
			AssertEquals("b", Document.AllocationNotes);
			AssertEquals("a", Document.AllocationStatusNote.Text);
			AssertEquals("b", Document.AllocationNotesNote.Text);

			Document.AllocationStatus = null;
			Document.AllocationNotes = "";
			Factory.Save();
			AssertEquals(true, allocationStatusNote.HasLoadedNote);
			AssertEquals(true, allocationNotesNote.HasLoadedNote);

			Document.Delete();
			AssertEquals(false, allocationStatusNote.HasLoadedNote);
			AssertEquals(false, allocationNotesNote.HasLoadedNote);
			AssertEquals(true, string.IsNullOrEmpty(Document.AllocationStatus));
			AssertEquals(true, string.IsNullOrEmpty(Document.AllocationNotes));
		}

		public void TestSC_FormCategory_ListWithUnallocatedDocument()
		{
			AssertEquals("Precondition: Document is unallocated", true, Document.IsUnallocated);

			AssertEquals("Document's SC_FormCategory list will have the same number of elements in the assembly lookup",
					AssemblyDataLookup.DocManagerCodesForAllocation.Count, Document.SC_FormCategory_List.Count);

			AssertEquals("Document's SC_FormCategory list will be the same as the assembly lookup list for allocation",
				AssemblyDataLookup.DocManagerCodesForAllocation, Document.SC_FormCategory_List);
		}

		public void TestScannedBarcodeValueMaxLength()
		{
			//Arrange
			var barcodes = new List<string>() { "^SHP=S15SMEX0001886|", "^DOC=POD|" };
			var longBarcodes = new List<string>()
			{ "ATB150029681020157650",
				"IAH00000579",
				"ATB150029681020157650",
				"IAH00000576",
				"ATB150029681020157650",
				"IAH00000561",
				"^CON=C15AIAH00000308;MAN;|"
			};

			//Act
			Document.ScannedBarcodeValue = BarcodeHelper.JoinValidBarcodes(barcodes, StorageDocsBarcode.Schema.SCB_BarcodeMaxLength);

			Document.ScannedBarcodeValue = BarcodeHelper.JoinValidBarcodes(longBarcodes, StorageDocsBarcode.Schema.SCB_BarcodeMaxLength);

			//Assert
			Assert("Value in the field should be set to less than maxlength", Document.ScannedBarcodeValue.Length <= StorageDocsBarcode.Schema.SCB_BarcodeMaxLength);
		}

		public void TestScannedBarcodeValueReadonly()
		{
			Assert(Document.ScannedBarcodeValueInfo.ReadOnly);
		}

		public void TestCompanyCode_ReadOnly()
		{
			AssertEquals("Company code is not editable, meaning that the Allocate eDocs menu can't change the company", false, Document.CompanyCodeInfo.ReadOnly);
		}

		public void TestBranchCode_ReadOnly()
		{
			AssertEquals("Branch code is not editable, meaning that the Allocate eDocs menu can't change the branch", false, Document.BranchCodeInfo.ReadOnly);
		}

		public void TestDepartmentCode_ReadOnly()
		{
			AssertEquals("Department code is not editable, meaning that the Allocate eDocs menu can't change the department", false, Document.DepartmentCodeInfo.ReadOnly);
		}

		StorageDocsUnallocated Document;
		OrgHeader Org;

		protected override void SetUp()
		{
			base.SetUp();
			Document = Factory.New<StorageDocsUnallocated>();
			ZQuery filter = new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "A");
			Org = Factory.LoadTop1(typeof(OrgHeader), filter) as OrgHeader;
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());

		byte[] SamplePdfBytes => resourceRetriever.Value.GetBytes("Enterprise.DocumentScanning.Business.Test.TestDocs.Sample.PDF");

		DocumentFactory MasterFactory => masterFactory ?? (masterFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory()));

		DocumentFactory masterFactory;
	}
}
