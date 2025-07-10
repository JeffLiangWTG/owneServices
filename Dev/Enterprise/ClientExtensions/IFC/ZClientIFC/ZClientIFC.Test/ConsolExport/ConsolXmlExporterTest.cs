using System;
using System.IO;
using System.Linq;
using System.Xml;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Xml;
using Enterprise.DocumentEngine.FileFormatUtilities;
using Enterprise.DocumentScanning.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.DataTransfer;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.IFC.ConsolExport.Testing
{
	public class ConsolXmlExporterTest : TestCaseWithFactory
	{
		[TestTimeZone]
		[TestDate(2005, 1, 1, 0, 0, 0)]
		public void TestConsolBatchProcess()
		{
			SystemDataRegistry.Instance.SimpleXMLExportFormat.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			ZString fileName = "";
			TestFile = CreateTestFile(Env.TempPath, "ExpectedOutput.xml");
			try
			{
				Exporter.Export(Consol, null);
				fileName = Exporter.ExportedFileNameForTesting;
				AssertEquals("FileName should be set", Path.Combine(Env.TempPath, "C99999999010120051200.xml"), fileName);
				Assert("File should have been created", File.Exists(fileName));
				AssertFilesEqual("Should be same as test file", TestFile, fileName);
			}
			finally
			{
				DeleteIfExists(TestFile);
				DeleteIfExists(fileName);
			}
		}

		[TestTimeZone]
		[TestDate(2005, 1, 1, 0, 0, 0)]
		public void TestConsolExportWithExistingFile()
		{
			SystemDataRegistry.Instance.SimpleXMLExportFormat.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			ZString newFile = Path.Combine(Env.TempPath, "C99999999010120051200.xml");
			TestFile = CreateTestFile(Env.TempPath, "ExpectedOutput.xml");
			using (FileStream stream = File.Create(newFile))
			{
			}

			AssertASCIIFileSameAsString(newFile, "");
			try
			{
				Exporter.Export(Consol, null);
				AssertEquals("File Names should be the same", newFile, Exporter.ExportedFileNameForTesting);
				AssertFilesEqual("Should have overwritten new file", TestFile, newFile);
			}
			finally
			{
				DeleteIfExists(TestFile);
				DeleteIfExists(newFile);
			}
		}

		[TestDate(2005, 1, 1, 0, 0, 0)]
		public void TestConsolExportWithDocuments()
		{
			ZString fileName = "";
			bool previousConsolValue = SystemDataRegistry.Instance.IncludeConsoleDocs.Value;
			bool previousShipmentValue = SystemDataRegistry.Instance.IncludeShipmenteDocs.Value;
			try
			{
				var testImageFile = resourceRetriever.SaveResourceToFile("ConsolExport.Testing.TestImage.tif");
				DocumentFactory masterFactory = new DocumentFactoryProvider().GetFactory(Factory);
				RefDocType docType = masterFactory.New<RefDocType>();
				docType.RT_ReferenceType = Consol.DocManagerInfo.DocManagerCode;
				docType.RT_DocType = "ABC";
				docType.RT_Desc = "A Test Document";
				StorageMain parent = masterFactory.RetrieveExistingOrCreateStorageMainForPK(Consol.PK, Consol.DocManagerInfo.DocManagerCode);
				StorageDocs publishedDocument = parent.Documents.AddNew();
				publishedDocument.SC_DocType = "ABC";
				publishedDocument.SC_IsPublished = true;
				publishedDocument.SC_ImageData = File.ReadAllBytes(testImageFile);
				// this doc shouldn't be exported
				StorageDocs unpublishedDocument = parent.Documents.AddNew();
				unpublishedDocument.SC_DocType = "ABC";
				unpublishedDocument.SC_IsPublished = false;
				unpublishedDocument.SC_ImageData = File.ReadAllBytes(testImageFile);
				// this doc shouldn't be exported
				StorageDocs deletedDocument = parent.Documents.AddNew();
				deletedDocument.SC_DocType = "ABC";
				deletedDocument.SC_IsDeleted = true;
				deletedDocument.SC_ImageData = File.ReadAllBytes(testImageFile);
				masterFactory.Save();
				SystemDataRegistry.Instance.IncludeConsoleDocs.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				SystemDataRegistry.Instance.IncludeShipmenteDocs.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				Exporter.Export(Consol, null);
				fileName = Exporter.ExportedFileNameForTesting;
				AssertEquals("FileName should be set", Path.Combine(Env.TempPath, "C99999999010120051200.xml"), fileName);
				Assert("File should have been created", File.Exists(fileName));
				byte[] expectedPDFBytes = DocumentConverter.ConvertTIFToPDF(File.ReadAllBytes(testImageFile));
				using (StreamReader reader = new StreamReader(fileName))
				{
					XmlValueObjectSerializer consolsSerializer = new XmlValueObjectSerializer(typeof(Xsd.Consols));
					XmlTextReader payloadReader = new XmlTextReader(reader);
					Xsd.XmlInterchange interchange;
					Xsd.Consols consolsXsd = (Xsd.Consols)Xsd.XmlInterchange.DeserializeInterchangeAndPayload(payloadReader, consolsSerializer, out interchange);
					byte[] pDFData = consolsXsd.Consol[0].Documents[0].Data;
					AssertEquals("should be the same length", expectedPDFBytes.Length, pDFData.Length);
					// can't test the actual contents, because the PDF generated always has a different timestamp encoded.
					AssertEquals("Data Type should be exported as PDF format", "PDF", consolsXsd.Consol[0].Documents[0].DataType);
				}
			}
			finally
			{
				SystemDataRegistry.Instance.IncludeConsoleDocs.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, previousConsolValue);
				SystemDataRegistry.Instance.IncludeShipmenteDocs.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, previousShipmentValue);
				DeleteIfExists(fileName);
			}
		}

		void AssertFilesEqual(ZString message, ZString expectedFile, ZString actualFile)
		{
			// Child order of events in the exported xml are non-deterministic, so we cannot directly compare the xml.
			// To get around this we will assert the start and end of the xml match.
			ZString expectedFileAsString = "";
			ZString actualFileAsString = "";
			using (StreamReader reader = new StreamReader(expectedFile))
			{
				expectedFileAsString = reader.ReadToEnd();
			}

			expectedFileAsString = expectedFileAsString.Replace(@"<AgentReference>A99999999</AgentReference>", @"<AgentReference>C99999999</AgentReference>");
			using (StreamReader reader = new StreamReader(actualFile))
			{
				actualFileAsString = reader.ReadToEnd();
			}

			var indexOfFirstEventTag = expectedFileAsString.Split("<Event>")[0].Length;
			var indexOfLastEventTag = expectedFileAsString.Length - expectedFileAsString.Split("</Event>").Last().Length;
			AssertEquals("Xml lengths should match", expectedFileAsString.Length, actualFileAsString.Length);
			AssertEquals("Start of XML should be equal", expectedFileAsString.Substring(0, indexOfFirstEventTag), actualFileAsString.Substring(0, indexOfFirstEventTag));
			AssertEquals("End of XML should be equal", expectedFileAsString.Substring(indexOfLastEventTag), actualFileAsString.Substring(indexOfLastEventTag));
		}

		#region Implementation
		ConsolXmlExporter Exporter;
		ForwardingConsol Consol;
		ZString TestFile;
		EmbeddedResourceRetriever resourceRetriever;
		protected override void SetUp()
		{
			base.SetUp();
			Exporter = new ConsolXmlExporter();
			Consol = Factory.New<ForwardingConsol>();
			Consol.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			Consol.JK_UniqueConsignRef = "C99999999";
			Consol.JK_MasterBillNum = "001";
			IFCDataRegistry.Instance.FSCExportDirectory = Env.TempPath;
			Consol.Factory.Save();
			resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly);
		}
		protected override void TearDown()
		{
			base.TearDown();
			resourceRetriever.Dispose();
		}

		protected ZString CreateTestFile(string testDirectory, string fileName)
		{
			var path = Path.Combine(testDirectory, fileName);
			using (StreamWriter writer = new StreamWriter(path))
			{
				var interchange = Xsd.XmlInterchange.NewPopulatedInterchange(Factory);
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
				consol.JK_UniqueConsignRef = "A99999999";
				consol.JK_MasterBillNum = "001";
				var dummyCollection = new DummyBusinessObjectCollection(Factory)
				{ consol };
				interchange.Payload.Data = dummyCollection;
				interchange.Payload.DataAdapter = new ForwardingConsolValueObjectDataAdapter();
				interchange.Payload.Context = new ValueObjectExportContext(new NotificationBuffer());
				consol.Factory.Save();
				var serialiser = new XmlValueObjectSerializer(typeof(Xsd.XmlInterchange));
				serialiser.Serialize(writer, interchange);
				consol.Factory.CleanUp();
				writer.Close();
			}

			return Path.Combine(testDirectory, fileName);
		}
		#endregion
	}
}
