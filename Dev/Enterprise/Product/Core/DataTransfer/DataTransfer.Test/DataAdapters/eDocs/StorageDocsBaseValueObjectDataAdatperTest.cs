using System;
using System.IO;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DataTransfer.Xml.Testing;
using Enterprise.DataTransfer.Xml.XsdVersion1;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.DataAdapters.Testing
{
	[TestedType(typeof(StorageDocsBaseValueObjectDataAdatper))]
	sealed class StorageDocsBaseValueObjectDataAdatperTest : ValueObjectDataAdapterTest<BusinessObject, Xsd.DocumentMessage>
	{
		public void TestOverridenProperties()
		{
			AssertEquals("RootCollectionElementName", "DocumentMessages", Adapter.RootCollectionElementName);
			AssertEquals("RootElementName", "DocumentMessage", Adapter.RootElementName);
		}

		StorageDocsBaseValueObjectDataAdatper Adapter
		{
			get
			{
				if (adapter == null)
				{
					adapter = new StorageDocsBaseValueObjectDataAdatper();
				}
				return adapter;
			}
		}
		StorageDocsBaseValueObjectDataAdatper adapter;

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		protected override bool IsExportToValueObjectSupported
		{
			get { return true; }
		}

		protected override bool IsExportToCollectionSupported
		{
			get { return false; }
		}

		protected override bool IsImportFromValueObjectSupported
		{
			get { return false; }
		}

		protected override bool IsCreateOrUpdateFromValueObjectSupported
		{
			get
			{
				return false;
			}
		}

		public new void TestExportToValueObjectNotSupportedException()
		{
			Assert(true);
		}

		public new void TestImportFromValueObjectNotSupportedException()
		{
			Assert(true);
		}

		protected override string ExpectedRootCollectionElementName
		{
			get { return "DocumentMessages"; }
		}

		protected override string ExpectedRootElementName
		{
			get { return "DocumentMessage"; }
		}

		protected override BusinessObjectAndExpectedOutputFileName GetEmptyBizObjSample()
		{
			var documentFactoryProvider = ObjectFactory.Get<IDocumentFactoryProvider>();
			var documentFactory = (BusinessObjectFactory)documentFactoryProvider.GetFactory(Factory);
			var storageMain = documentFactory.New<IStorageMain>();

			var eDoc = storageMain.AddFileOrDocument(new byte[] { 0x1, 0x2 }, "DoesNotMatter", "MSC", true);
			eDoc.SetValuesForTest(new ZDateTime(2009, 09, 08, 08, 56, 0), "JPG");

			var expectedOutputFilename = resourceRetriever.Value.SaveResourceToFile("Enterprise.DataTransfer.Test.DataAdapters.eDocs.Testing.EmptyEDoc.xml");
			return new BusinessObjectAndExpectedOutputFileName((BusinessObject)eDoc, expectedOutputFilename, ValidationKind.None, "Empty eDoc");
		}

		protected override BusinessObjectAndExpectedOutputFileName GetPopulatedBizObjWithEmptyFieldsSample()
		{
			return GetEmptyBizObjSample();
		}

		protected override BusinessObjectAndExpectedOutputFileName GetFullyPopulatedBizObjSample()
		{
			var documentFactoryProvider = ObjectFactory.Get<IDocumentFactoryProvider>();
			var documentFactory = (BusinessObjectFactory)documentFactoryProvider.GetFactory(Factory);

			var shipment = (BusinessObject)Factory.LoadTop1<Enterprise.Integration.Forwarding.IForwardingShipment>(new ZQuery(JobShipmentSchema.JS_UniqueConsignRef, "S019071977")) ?? Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Forwarding.IForwardingShipment)));
			shipment[JobShipmentSchema.JS_UniqueConsignRef] = "S019071977";

			var storageMain = (BusinessObject)documentFactory.LoadTop1<IStorageMain>(new ZQuery(StorageMainSchema.SM_ParentFK, shipment.PK)) ?? documentFactory.NewWithValidTestData(ObjectFactory.GetType(typeof(IStorageMain)));

			storageMain[StorageMainSchema.Constants.SM_Type] = "SHP";
			storageMain[StorageMainSchema.Constants.SM_ParentFK] = shipment.PK;

			var eDoc = (storageMain as IStorageMain).AddFileOrDocument((SubStreamableStream)new MemoryStream(new byte[] { 0x1, 0x2 }), "PopulatedVersion", "MSC", true);
			eDoc.SetValuesForTest(new ZDateTime(2009, 09, 08, 08, 56, 0), "JPG");
			eDoc.IsPublished = true;
			eDoc.Description = "Adults only";

			Factory.Save();
			documentFactory.Save();

			var expectedOutputFilename = resourceRetriever.Value.SaveResourceToFile("Enterprise.DataTransfer.Test.DataAdapters.eDocs.Testing.PopulatedEDoc.xml");
			return new BusinessObjectAndExpectedOutputFileName((BusinessObject)eDoc, expectedOutputFilename, ValidationKind.None, "Populated eDoc");
		}

		protected override BusinessObjectAndExpectedOutputFileName[] GetMiscSampleBusinessObjects()
		{
			return Array.Empty<BusinessObjectAndExpectedOutputFileName>();
		}

		protected override ValueObjectDataAdapter<BusinessObject, DocumentMessage> GetNewBizObjXmlDataAdapter()
		{
			return new StorageDocsBaseValueObjectDataAdatper();
		}

		protected override string[] XmlNodesToExcludeFromCoverageTest
		{
			get
			{
				return new string[]
				{
					// these nodes are not used in this adapter
					"Document/SaveVersions",
					"DocumentLink/Date",
					"DocumentLink/Description",
					"ReferenceKeys/ReferenceKeyType",
					"ReferenceKeys/Value",
					"Events/Event/CodeDescription",
					"Events/Event/DateTime",
					"Events/Event/PostedDateTime",
					"Events/Event/Information",
					"Events/Event/User",
					"Events/Event/UserName",
					"Events/Event/UserEmailAddress",
					"Events/Event/TriggeredBy",
					"Events/Event/ReferenceKeys/ReferenceKeyDateTime",
					"Events/Event/ReferenceKeys/ReferenceKeyCountry",
					"Events/Event/ReferenceKeys/ReferenceKeyType",
					"Events/Event/ReferenceKeys/Value",
					"Events/Event/IsEstimatedDate",
					"Events/Version",
					"Events/InitialDataExported"
				};
			}
		}

		public void TestExportEventIsBeingGeneratedOnParentBizObj()
		{
			IDocumentFactoryProvider documentFactoryProvider = ObjectFactory.Get<IDocumentFactoryProvider>();
			BusinessObjectFactory documentFactory = (BusinessObjectFactory)documentFactoryProvider.GetFactory(Factory);

			var shipment = (BusinessObject)Factory.LoadTop1<Enterprise.Integration.Forwarding.IForwardingShipment>(new ZQuery(JobShipmentSchema.JS_UniqueConsignRef, "S019071977")) ?? Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Forwarding.IForwardingShipment)));
			shipment[JobShipmentSchema.JS_UniqueConsignRef] = "S019071977";

			var storageMain = (BusinessObject)documentFactory.LoadTop1<IStorageMain>(new ZQuery(StorageMainSchema.SM_ParentFK, shipment.PK)) ?? documentFactory.NewWithValidTestData(ObjectFactory.GetType(typeof(IStorageMain)));

			storageMain[StorageMainSchema.Constants.SM_Type] = "SHP";
			storageMain[StorageMainSchema.Constants.SM_ParentFK] = shipment.PK;

			IeDoc eDoc = (storageMain as IStorageMain).AddFileOrDocument((SubStreamableStream)new MemoryStream(new byte[] { 0x1, 0x2 }), "PopulatedVersion", "MSC", true);
			eDoc.SetValuesForTest(new ZDateTime(2009, 09, 08, 08, 56, 0), "JPG");
			eDoc.IsPublished = true;
			eDoc.Description = "Adults only";

			Factory.Save();
			documentFactory.Save();

			Assert("Precondition: DEX event is not present", !Array.Exists<StmALog>((shipment as EnterpriseBusinessObject).Logs.GetAllLogs().ToArray<StmALog>(), x => (x.SL_SE_NKEvent == Enterprise.ZArchitecture.Business.Events.DataExport.Code && x.SL_Reference.EqualsIgnoringCase("eDoc: PopulatedVersion.jpg"))));

			Adapter.ExportToValueObject(eDoc as BusinessObject, new ValueObjectExportContext(Notify));

			Assert("DEX event should have been generated during export process", Array.Exists<StmALog>((shipment as EnterpriseBusinessObject).Logs.GetAllLogs().ToArray<StmALog>(), x => (x.SL_SE_NKEvent == Enterprise.ZArchitecture.Business.Events.DataExport.Code && x.SL_Reference.EqualsIgnoringCase("eDoc: PopulatedVersion.jpg"))));
		}

		public void TestExportReferenceKeysWhenDocOwnerIsCusEntryHeader()
		{
			IDocumentFactoryProvider documentFactoryProvider = ObjectFactory.Get<IDocumentFactoryProvider>();
			var documentFactory = (BusinessObjectFactory)documentFactoryProvider.GetFactory(Factory);

			var declaration = (BusinessObject)Factory.LoadTop1<Enterprise.Integration.Customs.IBaseJobDeclaration>(new ZQuery(JobDeclarationSchema.JE_DeclarationReference, "B019071977")) ?? Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Customs.IBaseJobDeclaration)));
			declaration[JobDeclarationSchema.JE_DeclarationReference] = "B019071977";
			declaration[JobDeclarationSchema.JE_HouseBill] = "H019071977";
			declaration[JobDeclarationSchema.JE_MasterBill] = "M019071977";

			var cusEntryHeader = (BusinessObject)Factory.LoadTop1<Enterprise.Integration.Customs.ICusEntryHeader>(new ZQuery(CusEntryHeaderSchema.CH_JE, declaration.PK));
			if (cusEntryHeader == null)
			{
				cusEntryHeader = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Customs.ICusEntryHeader)));
				cusEntryHeader[CusEntryHeaderSchema.CH_JE] = declaration.PK;
			}

			var storageMain = (BusinessObject)documentFactory.LoadTop1<IStorageMain>(new ZQuery(StorageMainSchema.SM_ParentFK, declaration.PK)) ?? documentFactory.NewWithValidTestData(ObjectFactory.GetType(typeof(IStorageMain)));

			storageMain[StorageMainSchema.Constants.SM_Type] = "CEH";
			storageMain[StorageMainSchema.Constants.SM_ParentFK] = cusEntryHeader.PK;

			IeDoc eDoc = (storageMain as IStorageMain).AddFileOrDocument((SubStreamableStream)new MemoryStream(new byte[] { 0x1, 0x2 }), "PopulatedVersion", "MSC", true);
			eDoc.SetValuesForTest(new ZDateTime(2009, 09, 08, 08, 56, 0), "JPG");
			eDoc.IsPublished = true;
			eDoc.Description = "Adults only";

			Factory.Save();
			documentFactory.Save();

			var output = Adapter.ExportToValueObject(eDoc as BusinessObject, new ValueObjectExportContext(Notify));

			AssertEquals(3, output.ReferenceKeys.Count);
		}

		NotificationBuffer Notify
		{
			get
			{
				if (notify == null)
				{
					notify = new NotificationBuffer();
				}
				return notify;
			}
		}
		NotificationBuffer notify;

		protected override void MasterSetUp()
		{
			base.MasterSetUp();

			using var auxConnection = Db.NewAdminConnection();
			AdoTestUtils.CreateDbIfNotExists(auxConnection, Db.DatabaseName + "_SD001");
		}
	}
}
