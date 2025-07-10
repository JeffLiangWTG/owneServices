using System;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.DocumentScanning.Business;
using Enterprise.DocumentScanning.DataTransfer.Universal;
using Enterprise.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.DataTransfer.Test
{
	[TestedType(typeof(DocManagerWrapper))]
	sealed class DocManagerWrapperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDeliverEDocWithAllocationDetailsInFileName()
		{
			var shipment1 = Factory.New<Forwarding.IForwardingShipment>();
			shipment1.JS_UniqueConsignRef = "S00009001";
			var shipment2 = Factory.New<Forwarding.IForwardingShipment>();
			shipment2.JS_UniqueConsignRef = "S00009002";
			Factory.Save();

			ImportDocMessage("[SHP TSR S00009002] hello.txt", "SGVsbG8sIFdvcmxkIQ==", false);

			var reloadedShipment1 = new BusinessObjectFactory().Load<Forwarding.IForwardingShipment>(shipment1.PK);
			var reloadedDocManagerInfo1 = ((IDocManagerSupport)reloadedShipment1).DocManagerInfo;
			AssertEquals(0, reloadedDocManagerInfo1.AllEDocs.Count);

			var reloadedShipment2 = new BusinessObjectFactory().Load<Forwarding.IForwardingShipment>(shipment2.PK);
			var reloadedDocManagerInfo2 = ((IDocManagerSupport)reloadedShipment2).DocManagerInfo;
			AssertEquals(1, reloadedDocManagerInfo2.AllEDocs.Count);

			var newDoc = reloadedDocManagerInfo2.AllEDocs.Cast<IeDoc>().FirstOrDefault();
			AssertNotNull(newDoc);
			AssertEquals("hello.txt", newDoc.FileName);
			AssertEquals("TSR", newDoc.DocType);

			using (var stream = newDoc.GetImageDataReader())
			using (var streamReader = new StreamReader(stream, Encoding.ASCII))
			{
				AssertEquals("Hello, World!", streamReader.ReadToEnd()); // <ImageData>SGVsbG8sIFdvcmxkIQ==</ImageData>
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDeliverEDocWithSingleDocTiffAndBarcode()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00001000";
			Factory.Save();

			const string fileName = "MultipageWithShipmentBarcodes.tif";
			var testFile = GetFullTestFileName(fileName);
			var data = Convert.ToBase64String(File.ReadAllBytes(testFile));

			ImportDocMessage("MultipageWithShipmentBarcodes.tif", data, false);

			var reloadedShipment = new BusinessObjectFactory().Load<Forwarding.IForwardingShipment>(shipment.PK);
			var reloadedDocManagerInfo = ((IDocManagerSupport)reloadedShipment).DocManagerInfo;
			AssertEquals(1, reloadedDocManagerInfo.AllEDocs.Count);

			var newDoc = reloadedDocManagerInfo.AllEDocs.Cast<IeDoc>().FirstOrDefault();
			AssertNotNull(newDoc);
			AssertEquals(fileName, newDoc.FileName);
			AssertEquals("CIV", newDoc.DocType);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDeliverEDocWithMultiDocPdfAndBarcode()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00001001";
			Factory.Save();

			const string fileName = "ShipmentScannedDocs.pdf";
			var testFile = GetFullTestFileName(fileName);
			var data = Convert.ToBase64String(File.ReadAllBytes(testFile));

			ImportDocMessage(fileName, data, false);

			var reloadedShipment = new BusinessObjectFactory().Load<Forwarding.IForwardingShipment>(shipment.PK);
			var reloadedDocManagerInfo = ((IDocManagerSupport)reloadedShipment).DocManagerInfo;
			AssertEquals(2, reloadedDocManagerInfo.AllEDocs.Count);

			Assert(reloadedDocManagerInfo.AllEDocs.Cast<IeDoc>().Any(eDoc => eDoc.DocType == "CIV"));
			Assert(reloadedDocManagerInfo.AllEDocs.Cast<IeDoc>().Any(eDoc => eDoc.DocType == "DGF"));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDeliverEDocWithMultiDocPdfAndBarcodeUnallocated()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00001234";
			Factory.Save();

			const string fileName = "ShipmentScannedDocs.pdf";
			var testFile = GetFullTestFileName(fileName);
			var data = Convert.ToBase64String(File.ReadAllBytes(testFile));

			ImportDocMessage(fileName, data, true);

			var reloadedShipment = new BusinessObjectFactory().Load<Forwarding.IForwardingShipment>(shipment.PK);
			var reloadedDocManagerInfo = ((IDocManagerSupport)reloadedShipment).DocManagerInfo;
			AssertEquals(0, reloadedDocManagerInfo.AllEDocs.Count);

			var docFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			var anallocatedEDocs = docFactory.Load<StorageDocsUnallocated>(new ZQuery { OrderBy = StorageDocsSchema.Constants.SC_DocType });

			AssertEquals(2, anallocatedEDocs.Length);

			AssertEquals(anallocatedEDocs[0].SC_DocType, "CIV");
			AssertEquals(anallocatedEDocs[1].SC_DocType, "DGF");
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDeliverUnallocatedEDoc()
		{
			const string fileName = "Test.tif";
			var testFile = GetFullTestFileName(fileName);
			var data = Convert.ToBase64String(File.ReadAllBytes(testFile));

			var docFilter = new ZQuery(StorageDocsSchema.SC_FileName, Path.GetFileNameWithoutExtension(fileName));
			AssertEquals(0, Factory.GetDatabaseCount(typeof(StorageDocsUnallocated), docFilter));

			ImportDocMessage(fileName, data, true);

			AssertEquals("eDoc should be imported into main db as unallocated", 1, Factory.GetDatabaseCount(typeof(StorageDocsUnallocated), docFilter));
		}

		public void TestUnsupportedFileTypeReturnErrorLogs()
		{
			ImportDocMessage("hello.txt", "SGVsbG8sIFdvcmxkIQ==", false, @"Error - Couldn't import file 'hello.txt'. Please make sure the allocation information is added to the beginning of the file name or the subject of the email (e.g. '[DocManager SHP CIV S00001000]')
Error - Document hello.txt was not imported.");
		}

		#region Implementation

		const string TestFilesFolder = @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\";

		string GetFullTestFileName(string fileName)
		{
			return Path.Combine(BaseSourcePath, TestFilesFolder, fileName);
		}

		void ImportDocMessage(string fileName, string imageData, bool expectedUnallocated, string expectedError = null)
		{
			var initialUnallocatedDocsCount = Factory.GetDatabaseCount(typeof(StorageDocsUnallocated));
			var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;

			var requestXml =
				$@"<UniversalEvent version=""1.1"" xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
  <Event>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>DocManager</Type>
        </DataTarget>
      </DataTargetCollection>

      <Company>
        <Code>EDI</Code>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Name>Eagle Datamation International</Name>
      </Company>
      <DataProvider>{registrationKey.EnterpriseCode}{registrationKey.ServerCode}EDI</DataProvider>
      <EnterpriseID>{registrationKey.EnterpriseCode}</EnterpriseID>
      <ServerID>{registrationKey.ServerCode}</ServerID>
    </DataContext>

    <EventTime>2018-06-07T11:06:10.707</EventTime>
    <EventType>DDI</EventType>
    <IsEstimate>false</IsEstimate>

    <AttachedDocumentCollection>
      <AttachedDocument>
        <FileName>{fileName}</FileName>
        <ImageData>{imageData}</ImageData>
        <Type>
          <Code></Code>
        </Type>
        <IsPublished>true</IsPublished>
        <VisibleCompanyCode></VisibleCompanyCode>
        <VisibleBranchCode></VisibleBranchCode>
        <VisibleDepartmentCode></VisibleDepartmentCode>
      </AttachedDocument>
    </AttachedDocumentCollection>

  </Event>
</UniversalEvent>";

			var xmlSessionTracker = ObjectFactory.Get<IXmlSessionTracker>("IXmlSessionTracker", new SimpleLogger());
			var handler = new UniversalEventImportHandler(xmlSessionTracker);
			var request = handler.CreateRequestMessage();
			using (var stream = (SubStreamableStream)new MemoryStream())
			using (var streamWriter = new StreamWriter(stream))
			{
				streamWriter.AutoFlush = true;
				streamWriter.Write(requestXml);
				request.SetMessageTextSource(stream);
				request.Save();
			}

			string responseMessageText;
			using ((request as BusinessObject).Factory.AddDisposableService())
			using (var processingResult = handler.Process(request))
			using (var stream = processingResult.ResponseMessageText)
			using (var streamReader = new StreamReader(stream))
			{
				AssertNotNull(processingResult);
				AssertEquals("PRS", processingResult.Status);

				stream.Position = 0;
				responseMessageText = streamReader.ReadToEnd();
			}

			AssertContains(
				@"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>DocManager</Type>
          <Key></Key>
        </DataSource>
      </DataSourceCollection>",
				responseMessageText);

			AssertNotContains("<AttachedDocumentCollection>", responseMessageText);

			if (string.IsNullOrEmpty(expectedError))
			{
				AssertContains("<EventType>DIM</EventType>", responseMessageText);
				AssertNotContains("Should not have errors", "<Type>FailureReason</Type>", responseMessageText);
			}
			else
			{
				AssertContains("Should have error",
					$@"<Context>
        <Type>FailureReason</Type>
        <Value>{expectedError}</Value>
      </Context>",
					responseMessageText);
			}

			if (expectedUnallocated)
			{
				Assert("Should have new unallocated eDocs", Factory.GetDatabaseCount(typeof(StorageDocsUnallocated)) > initialUnallocatedDocsCount);
			}
			else
			{
				AssertEquals("Should not have new unallocated eDocs", initialUnallocatedDocsCount, Factory.GetDatabaseCount(typeof(StorageDocsUnallocated)));
			}
		}

		#endregion Implementation
	}
}
