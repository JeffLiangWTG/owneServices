using System;
using System.IO;
using System.Text;
using System.Xml;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.SystemMerge.Business;
using Enterprise.DataTransfer.SystemMerge.Xml.Testing;
using Enterprise.DataTransfer.Xml;
using Enterprise.DocumentScanning.Business;
using Enterprise.DocumentScanning.Business.Test;
using Enterprise.ZArchitecture;
using Xsd = Enterprise.DataTransfer.SystemMerge.XmlDefinition;

namespace Enterprise.DataTransfer.SystemMerge.DataAdapters.Testing
{
	internal class SysMergeEdocsValueObjectDataAdapterTest : TestCaseWithDocumentFactory
	{
		public void TestExport()
		{
			StorageDocsForDataTransfer doc = GetTestStorageDocs();
			Xsd.SystemMergeStorageDoc xsdDoc = adapter.ExportToValueObject(doc, new ValueObjectExportContext(new NotificationBuffer()));
			AssertXsdAndBizObjMatch(xsdDoc, doc);
		}

		public void TestImport()
		{
			StorageDocsForDataTransfer doc = GetTestStorageDocs();
			Xsd.SystemMergeStorageDoc xsdDoc = adapter.ExportToValueObject(doc, new ValueObjectExportContext(new NotificationBuffer()));
			BusinessObjectFactory importingFactory = NewFactory();
			IValueObjectImportContext importingContext = new ValueObjectImportContext(importingFactory, new NotificationBuffer());
			StorageDocsForDataTransfer importedDoc = importingFactory.NewWithPrimaryKey<StorageDocsForDataTransfer>(new Guid(xsdDoc.PK));
			adapter.ImportFromValueObject(importedDoc, xsdDoc, importingContext);
			AssertXsdAndBizObjMatch(xsdDoc, doc);
		}

		public void TestExportAndImport()
		{
			// Create new StorageDocs
			StorageDocsForDataTransfer doc = GetTestStorageDocs();
			Xsd.SystemMergeStorageDoc xsdDoc1 = adapter.ExportToValueObject(doc, new ValueObjectExportContext(new NotificationBuffer()));

			// Write ValueObject to XML for Compare
			string exportedDocXml1 = WriteValueObjectToXml(xsdDoc1);
			AssertNonEmptyElementTagsSpecified(exportedDocXml1);

			// Import from XMLValueObject to BusinessObject
			DocumentFactory importingFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			IValueObjectImportContext importingContext = new ValueObjectImportContext(importingFactory, new NotificationBuffer());
			StorageDocsForDataTransfer importedDoc = importingFactory.NewWithPrimaryKey<StorageDocsForDataTransfer>(new Guid(xsdDoc1.PK));
			adapter.ImportFromValueObject(importedDoc, xsdDoc1, importingContext);
			importedDoc.ParentOrgPk = new ZGuid(xsdDoc1.OrgHeaderPk);
			AssertXsdAndBizObjMatch(xsdDoc1, importedDoc);

			// Export BusinessObject To ValueObject
			Xsd.SystemMergeStorageDoc xsdDoc2 = adapter.ExportToValueObject(importedDoc, new ValueObjectExportContext(new NotificationBuffer()));
			AssertXsdAndBizObjMatch(xsdDoc2, importedDoc);

			// Write ValueObject to XML for Compare
			string exportedDocXml2 = WriteValueObjectToXml(xsdDoc2);

			AssertMultilineASCIIEquals("Comparing 2 ValueObject in XML format", exportedDocXml1, exportedDocXml2);
		}

		public void TestDoNotExportIfMarkAsDeleted()
		{
			StorageDocsForDataTransfer doc = GetTestStorageDocs();
			doc.SC_IsDeleted = true;
			Xsd.SystemMergeStorageDoc xsdDoc = adapter.ExportToValueObject(doc, new ValueObjectExportContext(new NotificationBuffer()));

			AssertEquals("XSD is specified?", false, xsdDoc.IsSpecified);
		}

		public void TestDoNotExportIfImageDataIsEmpty()
		{
			StorageDocsForDataTransfer doc = GetTestStorageDocs();
			doc.SC_ImageData = new ZBlob();
			Xsd.SystemMergeStorageDoc xsdDoc = adapter.ExportToValueObject(doc, new ValueObjectExportContext(new NotificationBuffer()));

			AssertEquals("XSD is specified?", false, xsdDoc.IsSpecified);
		}

		StorageDocsForDataTransfer GetTestStorageDocs()
		{
			OrgHeaderForDataTransfer org = Factory.LoadTop1<OrgHeaderForDataTransfer>(new ZQuery());

			StorageDocsForDataTransfer doc = MasterFactory.New<StorageDocsForDataTransfer>();
			doc.ParentOrgPk = org.PK;
			doc.SC_FileName = "File1";
			doc.SC_DataType = "MSG";
			doc.SC_Date = new ZDateTime(2009, 1, 11, 7, 30, 0);
			doc.SC_Desc = "Desc 1";
			doc.SC_DocType = "MSC";
			doc.SC_IsDeleted = false;
			doc.SC_IsPublished = true;
			doc.SC_IsSystemGenerated = false;
			doc.SC_ImageData = new ZBlob(new byte[] { 1, 10, 11, 100, 110, 111 });

			return doc;
		}

		#region Assert Methods

		#region Assert XSD vs BizObj

		void AssertXsdAndBizObjMatch(Xsd.SystemMergeStorageDoc xsdDoc, StorageDocsForDataTransfer doc)
		{
			AssertEquals("Parent Org PK", doc.ParentOrgPk.ToString(), xsdDoc.OrgHeaderPk);
			AssertEquals("StorageDocs PK", doc.PK.ToString(), xsdDoc.PK);
			AssertEquals("ImageData", doc.SC_ImageData, xsdDoc.ImageData);

			AssertEquals("DataType", doc.SC_DataType, xsdDoc.DataType);
			AssertEquals("Date", doc.SC_Date, xsdDoc.Date);
			AssertEquals("Desc", doc.SC_Desc, xsdDoc.Desc);
			AssertEquals("DocType", doc.SC_DocType, xsdDoc.DocType);
			AssertEquals("FileName", doc.SC_FileName, xsdDoc.FileName);
			AssertEquals("IsPublished", doc.SC_IsPublished, xsdDoc.IsPublished);
			AssertEquals("IsSystemGenerated", doc.SC_IsSystemGenerated, xsdDoc.IsSystemGenerated);
			AssertEquals("SaveVersions", doc.SC_SaveVersions, xsdDoc.SaveVersions);
		}

		#endregion

		void AssertNonEmptyElementTagsSpecified(string exportedXml)
		{
			int maxIndex = exportedXml.IndexOf("</SystemMergeStorageDoc>") - 1;

			AssertTagSpecified(exportedXml, "PK", maxIndex);
			AssertTagSpecified(exportedXml, "OrgHeaderPk", maxIndex);
			AssertTagSpecified(exportedXml, "DataType", maxIndex);
			AssertTagSpecified(exportedXml, "DocType", maxIndex);
			AssertTagSpecified(exportedXml, "Desc", maxIndex);
			AssertTagSpecified(exportedXml, "FileName", maxIndex);
			AssertTagSpecified(exportedXml, "Date", maxIndex);
			AssertTagSpecified(exportedXml, "IsPublished", maxIndex);
			AssertTagSpecified(exportedXml, "IsSystemGenerated", maxIndex);
			AssertTagSpecified(exportedXml, "SaveVersions", maxIndex);
			AssertTagSpecified(exportedXml, "ImageData", maxIndex);
		}

		void AssertTagSpecified(string exportedXml, string tagName, int maxIndex)
		{
			int tagIndex = exportedXml.IndexOf("<" + tagName + ">");
			AssertEquals(tagName + " tag specified?", true, tagIndex >= 0 && tagIndex <= maxIndex);
		}

		#endregion

		#region Write XML

		string WriteValueObjectToXml(IValueObject valueObj)
		{
			StringWriter writer = new UTF8StringWriter();
			XmlTextWriter xmlWriter = new XmlTextWriter(writer);
			xmlWriter.Formatting = Formatting.Indented;
			SysMergeEdocsXmlValueObjectSerializerForTesting serializer = new SysMergeEdocsXmlValueObjectSerializerForTesting(adapter);

			serializer.WriteToXml(xmlWriter, adapter, valueObj, new NotificationBuffer());
			xmlWriter.Flush();
			writer.Flush();
			string result = writer.GetStringBuilder().ToString();

			return result;
		}

		public class UTF8StringWriter : StringWriter
		{
			public override Encoding Encoding
			{
				get { return Encoding.UTF8; }
			}
		}

		#endregion

		readonly SysMergeEdocsValueObjectDataAdapter adapter = new SysMergeEdocsValueObjectDataAdapter();
	}
}
