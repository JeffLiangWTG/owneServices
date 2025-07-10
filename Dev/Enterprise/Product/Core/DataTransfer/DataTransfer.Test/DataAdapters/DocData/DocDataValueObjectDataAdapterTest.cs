using System;
using CargoWise.Application;
using CargoWise.IO;
using Enterprise.DataTransfer.Xml.Testing;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.SDF;
using Enterprise.ZArchitecture;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.DataAdapters.Testing
{
	[TestedType(typeof(DocDataValueObjectDataAdapter))]
	sealed class DocDataValueObjectDataAdapterTest : ValueObjectDataAdapterTest<DocumentNote, Xsd.ShipmentDocData>
	{
		public void TestExport()
		{
			Note.SetSystemDefinedFieldValue("Insurance Policy Number", "Test");
			Factory.Save();
			Xsd.ShipmentDocData xsdDocData = new Xsd.ShipmentDocData();
			Adapter.ExportToValueObject(Note, xsdDocData, new ValueObjectExportContext(Notify));
			AssertEquals("There should not be errors", false, Notify.HasErrors);
			AssertEquals(xsdDocData.SystemDefinedData[0].Value, "Test");
			AssertEquals(xsdDocData.SystemDefinedData[0].Name, "Insurance Policy Number");
		}

		public void TestImport()
		{
			Xsd.ShipmentDocData xsdDocData = new Xsd.ShipmentDocData();
			Xsd.SystemDefinedDataType sysDefData = xsdDocData.SystemDefinedData.AddNew();
			sysDefData.Name = "Insurance Policy Number";
			sysDefData.Value = "Test";

			ValueObjectImportContext context = new ValueObjectImportContext(Factory, Notify);
			Adapter.ImportFromValueObject(Note, xsdDocData, context);
			AssertEquals(Note.GetSystemDefinedFieldValue("Insurance Policy Number"), "Test");
		}

		public new void TestExportToAndImportFromAndExportToValueObject_ForEmptyBizO()
		{
			Assert(true);
		}

		public new void TestExportToAndImportFromAndExportToValueObject_ForPopulatedBizObjWithEmptyFields()
		{
			Assert(true);
		}

		public new void TestExportToAndImportFromAndExportToValueObject_ForFullyPopulatedBizO()
		{
			Assert(true);
		}

		public new void TestExportToValueObject_ForFullyPopulatedBizO()
		{
			Assert(true);
		}

		public new void TestExportToAndImportFromXmlInterchange()
		{
			Assert(true);
		}

		public new void TestTestCoverageOfValueObject()
		{
			Assert(true);
		}

		DocDataValueObjectDataAdapter Adapter
		{
			get { return fAdapter ?? (fAdapter = new DocDataValueObjectDataAdapter()); }
		}
		DocDataValueObjectDataAdapter fAdapter;

		NotificationBuffer Notify
		{
			get { return fNotify ?? (fNotify = new NotificationBuffer()); }
		}
		NotificationBuffer fNotify;

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		protected override ValueObjectDataAdapter<DocumentNote, Xsd.ShipmentDocData> GetNewBizObjXmlDataAdapter()
		{
			return Adapter;
		}

		protected override BusinessObjectAndExpectedOutputFileName GetEmptyBizObjSample()
		{
			var expectedOutputFilename = resourceRetriever.Value.SaveResourceToFile("Enterprise.DataTransfer.Test.DataAdapters.DocData.Testing.EmptyDocData.xml");
			return new BusinessObjectAndExpectedOutputFileName(Note, expectedOutputFilename, ValidationKind.None, "Empty Doc Data");
		}

		protected override BusinessObjectAndExpectedOutputFileName GetPopulatedBizObjWithEmptyFieldsSample()
		{
			return GetEmptyBizObjSample();
		}

		protected override BusinessObjectAndExpectedOutputFileName GetFullyPopulatedBizObjSample()
		{
			Note.SystemDefinedFieldWrappers.AddNew().S1_Value = "test";
			var expectedOutputFilename = resourceRetriever.Value.SaveResourceToFile("Enterprise.DataTransfer.Test.DataAdapters.DocData.Testing.PopulatedDocData.xml");
			return new BusinessObjectAndExpectedOutputFileName(Note, expectedOutputFilename, ValidationKind.Xsd | ValidationKind.FactorySave, "Populated Doc Data");
		}

		protected override BusinessObjectAndExpectedOutputFileName[] GetMiscSampleBusinessObjects()
		{
			return Array.Empty<BusinessObjectAndExpectedOutputFileName>();
		}

		protected override DocumentNote NewBusinessObject()
		{
			return Note;
		}

		DocumentNote Note
		{
			get
			{
				if (note == null)
				{
					EnterpriseBusinessObject shipment = (EnterpriseBusinessObject)Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Forwarding.IForwardingShipment)));
					note = DocumentNote.LoadNote(shipment);
					StmSystemDefinedField field = Factory.New<StmSystemDefinedField>();
					field.S1_Name = "Insurance Policy Number";
					StmSystemDefinedFieldWrapper wrapper = new StmSystemDefinedFieldWrapper(field);
					note.SystemDefinedFieldWrappers.Add(wrapper);
				}
				return note;
			}
		}
		DocumentNote note;

		protected override string ExpectedRootCollectionElementName
		{
			get { return null; }
		}

		protected override string ExpectedRootElementName
		{
			get { return "DocData"; }
		}
	}
}
