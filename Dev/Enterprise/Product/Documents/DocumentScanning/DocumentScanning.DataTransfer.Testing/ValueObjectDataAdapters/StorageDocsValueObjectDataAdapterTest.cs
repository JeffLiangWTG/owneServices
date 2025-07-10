using System;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.DataTransfer.Xml.Testing;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.FileFormatUtilities;
using Enterprise.DocumentScanning.Business;
using Enterprise.DocumentScanning.Business.Test;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DocumentScanning.DataTransfer.Test.ValueObjectDataAdapters
{
	[TestedType(typeof(StorageDocsValueObjectDataAdapter))]
	sealed class StorageDocsValueObjectDataAdapterTest : ValueObjectDataAdapterTest<StorageDocs, Xsd.Document>
	{
		public void TestBusinessObjectType()
		{
			var dataAdapter = new StorageDocsValueObjectDataAdapter();
			AssertEquals("Business Object Type should be StorageDocs", typeof(StorageDocs), dataAdapter.BusinessObjectType);
		}

		public void TestValueObjectType()
		{
			var dataAdapter = new StorageDocsValueObjectDataAdapter();
			AssertEquals("Value Object Type should be Xsd.Document", typeof(Xsd.Document), dataAdapter.ValueObjectType);
		}

		public void TestSchema()
		{
			var dataAdapter = new StorageDocsValueObjectDataAdapter();
			AssertEquals("Schema should be SingleDocumentSchema", XmlSchemaDefinitions.Instance.SingleDocumentSchema, dataAdapter.Schema);
		}

		public void TestCollectionSchema()
		{
			var dataAdapter = new StorageDocsValueObjectDataAdapter();
			AssertEquals("CollectionSchema should be DoucmentsSchema", XmlSchemaDefinitions.Instance.DocumentsSchema, dataAdapter.CollectionSchema);
		}

		#region Test Import / Create / Update

		[ExpectException(typeof(NotSupportedException))]
		public void TestThrowsNotSupportedExceptionIfParetPKIsNotValid()
		{
			IValueObjectDataAdapter adapter = new StorageDocsValueObjectDataAdapter();
			adapter.CreateOrUpdateFromValueObject(new Xsd.Document(), ImportContext);
		}

		[ExpectException(typeof(NotSupportedException))]
		public void TestParentObjectDoesNotImplementIDocManagerSupport()
		{
			IValueObjectDataAdapter adapter = new StorageDocsValueObjectDataAdapter(new BusinessObjectThatDoesNotImplementIDocManagerSupport());
			adapter.CreateOrUpdateFromValueObject(new Xsd.Document(), ImportContext);
		}

		[TestDate(2005, 12, 6, 15, 39, 53)]
		public void TestCreateImageWithEmptyDate()
		{
			var org = DocumentFactory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			IValueObjectDataAdapter adapter = new StorageDocsValueObjectDataAdapter(org);
			var document = CreateXSDDocument();
			document.Date = ZDateTime.Empty;

			DocumentFactory.Save();

			var storageDoc = (StorageDocs)adapter.CreateOrUpdateFromValueObject(document, ImportContext);
			var storageMain = DocumentFactory.Load<StorageMain>(storageDoc.SC_SM);
			AssertNotNull("A  new storage main was not created", storageMain);
			AssertEquals("StorageMain was not attached to the right BusinessObject", org.PK, storageMain.SM_ParentFK);
			AssertEquals("Should have 1 StorageDocs", 1, storageMain.Documents.Count);
			AssertEquals("Should be Current Date Time", ZDateTime.UtcNow, storageDoc.SC_Date);
		}

		public void TestCreateImageAndStorageMain()
		{
			var org = DocumentFactory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			IValueObjectDataAdapter adapter = new StorageDocsValueObjectDataAdapter(org);
			var document = CreateXSDDocument();

			DocumentFactory.Save();
			var storageDoc = (StorageDocs)adapter.CreateOrUpdateFromValueObject(document, ImportContext);
			var storageMain = DocumentFactory.Load<StorageMain>(storageDoc.SC_SM);
			AssertNotNull("A new storage main was not created", storageMain);
			AssertEquals("StorageMain was not attached to the right BusinessObject", org.PK, storageMain.SM_ParentFK);
			Assert("StorageMain should be allocated to a database", storageMain.SM_DB != 0);
			var sM_Type = ((IDocManagerSupport)org).DocManagerInfo.DocManagerCode;
			AssertEquals("SM_Type was not Correct", sM_Type, storageMain.SM_Type);
			AssertEquals("Should have 1 StorageDocs", 1, storageMain.Documents.Count);
			AssertStorageDoc(storageDoc);
			AssertNotificationMessage(Notify, true, eDocUpdatedMessage);
		}

		[ExpectNoExceptions]
		public void TestEmulateRealLifeImportWithOpenTransaction()
		{
			var org = DocumentFactory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			IValueObjectDataAdapter adapter = new StorageDocsValueObjectDataAdapter(org);
			var document = CreateXSDDocument();

			// emulating XmlDataTransferDirector.DoImport opening transaction
			CargoWise.Data.Db.Connection.BeginTransaction();
			try
			{
				adapter.CreateOrUpdateFromValueObject(document, ImportContext);
			}
			finally
			{
				CargoWise.Data.Db.Connection.RollbackTransaction();
			}
		}

		public void TestCreateImageUpdateStorageMain()
		{
			var org = DocumentFactory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			var storageMain = DocumentFactory.NewWithValidTestData<StorageMain>();
			storageMain.SM_ParentFK = org.PK;
			IValueObjectDataAdapter adapter = new StorageDocsValueObjectDataAdapter(org);
			var document = CreateXSDDocument();

			DocumentFactory.Save();

			var storageDoc = (StorageDocs)adapter.CreateOrUpdateFromValueObject(document, ImportContext);
			AssertEquals("Storage Main should have 1 StorageDocs", 1, storageMain.Documents.Count);
			AssertEquals("StorageDocs did not attach to the right StorageMain", storageMain.PK, storageDoc.SC_SM);
			AssertStorageDoc(storageDoc);

			var newStorageDoc = (StorageDocs)adapter.CreateOrUpdateFromValueObject(document, ImportContext);
			AssertEquals("Storage Main should have 2 StorageDocs", 2, storageMain.Documents.Count);
			AssertEquals("NewStorageDoc did not attach to the right StorageMain", storageMain.PK, newStorageDoc.SC_SM);
			Assert("A new storage docs was not created, it should not update the current one", newStorageDoc.PK != storageDoc.PK);
			AssertNotificationMessage(Notify, true, eDocUpdatedMessage);
		}

		public void TestUpdateImage()
		{
			var org = DocumentFactory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			var storageMain = DocumentFactory.New<StorageMain>();
			storageMain.SM_ParentFK = org.PK;
			storageMain.SM_DB = 1;

			var newDocType = DocumentFactory.New<RefDocType>();
			newDocType.RT_DocType = "MBL";
			newDocType.RT_ReferenceType = Core.Constants.DocManagerCodes.Organisation;
			newDocType.RT_Desc = "Masterbill";

			var storageDoc = storageMain.Documents.AddNew();
			storageDoc.SC_ImageData = new byte[] { 9, 7, 6, 2 };
			storageDoc.SC_Date = new ZDateTime(2005, 12, 6, 10, 28, 23);
			storageDoc.SC_DocType = "MBL";
			storageDoc.SC_Desc = "Masterbill";

			IValueObjectDataAdapter adapter = new StorageDocsValueObjectDataAdapter(org);

			DocumentFactory.Save();

			var updatedStorageDoc = (StorageDocs)adapter.CreateOrUpdateFromValueObject(CreateXSDDocument(), ImportContext);
			AssertEquals("StorageDoc was not updated correctly, because UpdatedStorageDoc and StorageDoc are not the same", storageDoc.PK, updatedStorageDoc.PK);
			AssertStorageDoc(updatedStorageDoc);
			AssertNotificationMessage(Notify, true, eDocUpdatedMessage);
		}

		public void TestImportEmptyDocument()
		{
			var org = DocumentFactory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			IValueObjectDataAdapter adapter = new StorageDocsValueObjectDataAdapter(org);
			var storageDoc = (StorageDocs)adapter.CreateOrUpdateFromValueObject(new Xsd.Document(), ImportContext);

			DocumentFactory.Save();

			var storageMain = DocumentFactory.Load<StorageMain>(storageDoc.SC_SM);
			AssertNull("Storage Main should be null, because it should not have been saved", storageMain);
			Assert("Notification should have warnings", Notify.HasWarnings);
			AssertNotificationMessage(Notify, true, EmptyDataWarning);
		}

		public void TestImportStorageDocWithNoImageData()
		{
			var org = DocumentFactory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			var storageMain = DocumentFactory.NewWithValidTestData<StorageMain>();
			storageMain.SM_ParentFK = org.PK;
			IValueObjectDataAdapter adapter = new StorageDocsValueObjectDataAdapter(org);

			DocumentFactory.Save();

			var storageDoc = (StorageDocs)adapter.CreateOrUpdateFromValueObject(new Xsd.Document(), ImportContext);
			AssertEquals("Storage Main should have NO StorageDocs", 0, storageMain.Documents.Count);
			Assert("Notification should have warnings", Notify.HasWarnings);
			AssertNotificationMessage(Notify, true, EmptyDataWarning);
		}

		public void TestEmptyDocumentTypeIsDefaultedToMSC()
		{
			var document = CreateXSDDocument();
			document.DocumentType = ZString.Empty;
			var org = DocumentFactory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			IValueObjectDataAdapter adapter = new StorageDocsValueObjectDataAdapter(org);

			var storageDoc = (StorageDocs)adapter.CreateOrUpdateFromValueObject(document, ImportContext);
			AssertEquals("Document Type should be MSC", Core.Constants.RefDocTypes.MiscellaneousDocument, storageDoc.SC_DocType);
			AssertEquals("Description", "Masterbill", storageDoc.SC_Desc);
			AssertNotificationMessage(Notify, false, eDocCreatedMessage);
		}

		public void TestCreateNonTiffFormat()
		{
			var document = CreateXSDDocument();
			document.Data = SamplePdfBytes;
			var org = DocumentFactory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			IValueObjectDataAdapter adapter = new StorageDocsValueObjectDataAdapter(org);

			var storageDoc = (StorageDocs)adapter.CreateOrUpdateFromValueObject(document, ImportContext);
			Assert("Notification should have warning", Notify.HasWarnings);
			AssertNotificationMessage(Notify, true, WrongFormatWarning);
			AssertNotificationMessage(Notify, false, eDocUpdatedMessage);
			AssertNotificationMessage(Notify, false, eDocCreatedMessage);
		}

		public void TestUpdateNonTiffImageFormat()
		{
			var document = CreateXSDDocument();
			document.Data = SamplePdfBytes;
			var org = DocumentFactory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			var storageMain = DocumentFactory.NewWithValidTestData<StorageMain>(TestBusinessObjectKind.MinimumRequiredToSave);
			var storageDoc = storageMain.Documents.AddNew();
			storageDoc.SC_ImageData = new byte[] { 9, 7, 6, 2 };
			storageDoc.SC_Date = new ZDateTime(2005, 12, 6, 10, 28, 23);
			storageDoc.SC_DocType = "MBL";
			storageDoc.SC_Desc = "Masterbill";
			IValueObjectDataAdapter adapter = new StorageDocsValueObjectDataAdapter(org);

			adapter.CreateOrUpdateFromValueObject(document, ImportContext);
			Assert("Notification should have warning", Notify.HasWarnings);
			AssertNotificationMessage(Notify, true, WrongFormatWarning);
			AssertNotificationMessage(Notify, false, eDocUpdatedMessage);
			AssertNotificationMessage(Notify, false, eDocCreatedMessage);
		}

		void AssertNotificationMessage(NotificationBuffer notify, bool expectedResult, ZString message)
		{
			var result = false;
			foreach (var @event in notify.Events)
			{
				if (@event.Message == message)
				{
					result = true;
					break;
				}
			}
			AssertEquals("Notification message was incorrect", expectedResult, result);
		}

		void AssertStorageDoc(StorageDocs storageDoc)
		{
			AssertEquals("Image Data", SmallTifBytes, storageDoc.SC_ImageData);
			AssertEquals("DataType", "TIF", storageDoc.SC_DataType);
			AssertEquals("Date", new ZDateTime(2005, 12, 6, 10, 28, 0), storageDoc.SC_Date.ToSmallDateTimeFloor());
			AssertEquals("Description", "Masterbill", storageDoc.SC_Desc);
		}

		#endregion Test Import / Create / Update

		#region Test Export

		public void TestExportStorageDocs()
		{
			var dataAdapter = new StorageDocsValueObjectDataAdapter();
			var storageMain = DocumentFactory.NewWithValidTestData<StorageMain>();

			var storageDoc = PopulatedBusinessObject();
			var storageDocDate = new ZDateTime(2005, 10, 25, 16, 1, 21);
			storageDoc.SC_Date = storageDocDate;
			storageDoc.SC_SM = storageMain.PK;
			DocumentFactory.Save();

			var document = dataAdapter.ExportToValueObject(storageDoc, new ValueObjectExportContext(new NotificationBuffer()));
			AssertEquals("Data Type", "TIF", document.DataType);
			AssertEquals("Document Type", "MBL", document.DocumentType);
			AssertEquals("Description", "Masterbill", document.Description);
			AssertEquals("FileName", "Small", document.FileName);
			AssertEquals("Date", storageDocDate, document.Date);
			AssertDataCanBeReadCorrectly(document, dataAdapter);
		}

		public void TestConvertDataToPDFFormat()
		{
			var dataAdapter = new StorageDocsValueObjectDataAdapterPDF(null);
			var storageDocs = PopulatedBusinessObject();

			var document = dataAdapter.ExportToValueObject(storageDocs, new ValueObjectExportContext(new NotificationBuffer()));
			var expectedPDFValue = DocumentConverter.ConvertTIFToPDF(SmallTifBytes);
			AssertEquals("Lenght of Bytes should be equal", expectedPDFValue.Length, document.Data.Length);
			//			AssertEquals("Image Data was not in PDF Format", ExpectedPDFValue, Document.Data); Cannot test contents because PDF conversion is not consistent
			AssertDataCanBeReadCorrectly(document, dataAdapter);
		}

		class StorageDocsValueObjectDataAdapterPDF : StorageDocsValueObjectDataAdapter
		{
			public StorageDocsValueObjectDataAdapterPDF(BusinessObject bizO)
				: base(bizO)
			{
			}

			protected override OutputFormatType ExportFormatType
			{
				get { return OutputFormatType.PDF; }
			}
		}

		void AssertDataCanBeReadCorrectly(Xsd.Document document, StorageDocsValueObjectDataAdapter dataAdapter)
		{
			var serializer = new XmlValueObjectSerializer(dataAdapter.ValueObjectType);
			var xmlElement = serializer.SerialiseToXmlElement(document);
			var list = xmlElement.GetElementsByTagName("Data");
			var node = list.Item(0);
			var pDFinBytes = Convert.FromBase64String(node.InnerText);
			AssertEquals("File was NOT Read Correctly", document.Data, pDFinBytes);
		}

		public void TestExportedTIFDataIsCompressed()
		{
			var dataAdapter = new StorageDocsValueObjectDataAdapter();
			var storageDocs = PopulatedBusinessObject();

			using (var resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly))
			{
				var tempDirectoryPath = resourceRetriever.SaveAllResourcesToFiles();
				storageDocs.SC_ImageData = File.ReadAllBytes(Path.Combine(tempDirectoryPath, "Enterprise.DocumentScanning.DataTransfer.Test.truecolour.tif"));
				var document = dataAdapter.ExportToValueObject(storageDocs, new ValueObjectExportContext(new NotificationBuffer()));
				Assert("Exported Document should be compressed, less bytes that original", document.Data.Length < storageDocs.SC_ImageData.Length);
			}
		}

		#endregion Test Export

		#region Overrides For Base

		protected override string ExpectedRootCollectionElementName
		{
			get { return "Documents"; }
		}

		protected override string ExpectedRootElementName
		{
			get { return "Document"; }
		}

		protected override ValueObjectDataAdapter<StorageDocs, Xsd.Document> GetNewBizObjXmlDataAdapter()
		{
			var org = DocumentFactory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = new ZString(Guid.NewGuid().ToString().Replace("-", "")).Left(OrgHeaderSchema.OH_Code.MaxLength);
			org.MainAddress.OA_Code = new ZString(Guid.NewGuid().ToString().Replace("-", "")).Left(OrgAddressSchema.OA_Code.MaxLength);
			return new StorageDocsValueObjectDataAdapter(org);
		}

		protected override StorageDocs NewBusinessObject()
		{
			return StorageDocs.New_DEBUG(DocumentFactory);
		}

		protected override BusinessObjectAndExpectedOutputFileName GetEmptyBizObjSample()
		{
			return new BusinessObjectAndExpectedOutputFileName(
				EmptyBusinessObject(), EmptyStorageDocsPath, ValidationKind.None, "Empty StorageDocs");
		}

		protected override BusinessObjectAndExpectedOutputFileName GetFullyPopulatedBizObjSample()
		{
			var fullyPopulatedStorageDocsPath = resourceRetrieverDataTransfer.Value.SaveResourceToFile("Enterprise.DocumentScanning.DataTransfer.Test.ValueObjectDataAdapters.Testing.FullyPopulatedStorageDocs.xml");

			return new BusinessObjectAndExpectedOutputFileName(PopulatedBusinessObject(), fullyPopulatedStorageDocsPath, ValidationKind.Xsd | ValidationKind.FactorySave, "Fully Populated StorageDocs");
		}

		protected override BusinessObjectAndExpectedOutputFileName GetPopulatedBizObjWithEmptyFieldsSample()
		{
			return new BusinessObjectAndExpectedOutputFileName(
				PopulatedBusinessObjectWithEmptyFields(), EmptyStorageDocsPath, ValidationKind.None, "Populated StorageDocs With Empty Fields");
		}

		protected override BusinessObjectAndExpectedOutputFileName[] GetMiscSampleBusinessObjects()
		{
			return Array.Empty<BusinessObjectAndExpectedOutputFileName>();
		}

		protected override void TestExportToAndImportFromAndExportToValueObject(BusinessObjectAndExpectedOutputFileName sample)
		{
			Assert("This test is not required because import from value is not supported, implement ImportFromValueObjectCore first", true);
		}

		#endregion Overrides For Base

		#region Implementation

		StorageDocs EmptyBusinessObject()
		{
			var storageDoc = StorageDocs.New_DEBUG(DocumentFactory);

			storageDoc.SC_DataType = "";
			storageDoc.SC_Date = new ZDateTime(2005, 10, 21);

			return storageDoc;
		}

		StorageDocs PopulatedBusinessObject()
		{
			var storageDoc = StorageDocs.New_DEBUG(DocumentFactory);
			storageDoc.SC_DataType = "TIF";
			storageDoc.SC_DocType = "MBL";
			storageDoc.SC_Desc = "Masterbill";
			storageDoc.SC_FileName = "Small";
			storageDoc.SC_Date = new ZDateTime(2005, 10, 21);
			storageDoc.SC_ImageData = SmallTifBytes;
			storageDoc.SC_IsSystemGenerated = true;
			storageDoc.SC_SaveVersions = true;
			storageDoc.SC_IsPublished = false;

			return storageDoc;
		}

		StorageDocs PopulatedBusinessObjectWithEmptyFields()
		{
			var storageDoc = StorageDocs.New_DEBUG(DocumentFactory);
			storageDoc.SC_DataType = "";
			storageDoc.SC_DocType = "";
			storageDoc.SC_Desc = "";
			storageDoc.SC_FileName = "";
			storageDoc.SC_Date = new ZDateTime(2005, 10, 21);
			return storageDoc;
		}

		Xsd.Document CreateXSDDocument()
		{
			var document = new Xsd.Document();
			document.Data = SmallTifBytes;
			document.DataType = "TIF";
			document.Date = new ZDateTime(2005, 12, 6, 10, 28, 23);
			document.Description = "Masterbill";
			document.DocumentType = "MBL";
			return document;
		}

		class BusinessObjectThatDoesNotImplementIDocManagerSupport : NonPersistentBusinessObject
		{
		}

		const string EmptyDataWarning = "Warning: No Document Data To Import";
		const string eDocCreatedMessage = "eDoc created";
		const string eDocUpdatedMessage = "eDoc updated";
		const string WrongFormatWarning = "Warning: Could not import document, File is not an image";

		#endregion Implementation

		#region SetUp

		NotificationBuffer Notify;

		protected override void SetUp()
		{
			base.SetUp();
			Notify = new NotificationBuffer();
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetrieverBusiness.IsValueCreated)
			{
				resourceRetrieverBusiness.Value.Dispose();
			}
			if (resourceRetrieverDataTransfer.IsValueCreated)
			{
				resourceRetrieverDataTransfer.Value.Dispose();
			}
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetrieverBusiness = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever(typeof(StorageDocsBaseTest).Assembly));

		readonly Lazy<EmbeddedResourceRetriever> resourceRetrieverDataTransfer = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());

		byte[] SmallTifBytes => resourceRetrieverBusiness.Value.GetBytes("Enterprise.DocumentScanning.Business.Test.TestDocs.small.tif");

		byte[] SamplePdfBytes => resourceRetrieverBusiness.Value.GetBytes("Enterprise.DocumentScanning.Business.Test.TestDocs.Sample.PDF");

		string EmptyStorageDocsPath
		{
			get
			{
				if (string.IsNullOrEmpty(emptyStorageDocsPath))
				{
					emptyStorageDocsPath = resourceRetrieverDataTransfer.Value.SaveResourceToFile("Enterprise.DocumentScanning.DataTransfer.Test.ValueObjectDataAdapters.Testing.EmptyStorageDocs.xml");
				}
				return emptyStorageDocsPath;
			}
		}
		string emptyStorageDocsPath;

		DocumentFactory DocumentFactory
		{
			get
			{
				if (fDocumentFactory == null)
				{
					fDocumentFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
				}
				return fDocumentFactory;
			}
		}

		DocumentFactory fDocumentFactory;

		ValueObjectImportContext ImportContext
		{
			get
			{
				if (fImportContext == null)
				{
					fImportContext = new ValueObjectImportContext(DocumentFactory, Notify);
				}
				return fImportContext;
			}
		}

		ValueObjectImportContext fImportContext;

		#endregion SetUp
	}
}
