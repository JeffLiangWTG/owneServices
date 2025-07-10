using System;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DocumentScanning.Business;
using Enterprise.DocumentScanning.DataTransfer.Universal;
using Enterprise.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Moq;
using NUnit.Framework;
using SimpleLogger = Enterprise.UniversalDataBuss.Management.SimpleLogger;

namespace Enterprise.DocumentScanning.DataTransfer.Test.Universal.DocumentRequest
{
	public class UniversalDocumentRequestHandlerTest : TestCaseWithFactory
	{
		IHttpXmlProcessingResult SendDocumentRequest(IXmlSessionTracker sessionTracker, string requestXml)
		{
			var handler = new UniversalDocumentRequestHandler(sessionTracker);
			var request = handler.CreateRequestMessage();
			var response = handler.CreateResponseMessage();
			var responseMessageSaver = new UniversalResponseSaver(request, response, sessionTracker);
			using (var stream = (SubStreamableStream)new MemoryStream())
			{
				new StreamWriter(stream) { AutoFlush = true }.Write(requestXml);
				request.SetMessageTextSource(stream);
				request.Save();
			}
			return handler.Process(request, responseMessageSaver);
		}

		void AssertResponseMessageLinkedToParentByDexEvent(BusinessObject parent)
		{
			var logParent = new BusinessObjectFactory().Load(parent.GetType(), parent.PK) as IStmALogParent;
			var exportLog = logParent.Logs.MostRecentLogByEventTime(AutoEvents.DataExport);
			AssertNotNull("Expecting a DEX event", exportLog);
			AssertNotNull("DEX event should be linked to response message", exportLog?.RelatedEDIMessage?.Message);
		}

		public void TestDocManagerRequestReturnAccountingInvoice()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			var company = branch.Company;
			var invoice = Factory.NewWithValidTestData<AccTransactionHeader>();
			invoice.AH_TransactionType = TransactionTypes.Invoice;
			invoice.AH_GC = company.PK;
			var docInfo = invoice.DocManagerInfo();

			const string fileName = "Invoice.msc";
			CreateEDocForTest(docInfo, fileName, Core.Constants.RefDocTypes.MiscellaneousDocument, true, new ZDateTime(2016, 7, 1, 12, 15, 00));

			docInfo.MasterFactory.Save();
			Factory.Save();

			var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;

			var requestXML = $@"<UniversalDocumentRequest xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
    <DocumentRequest>
        <DataContext>
            <DataTargetCollection>
                <DataTarget>
                    <Type>DocManager</Type>
                    <Key>{Core.Constants.DocManagerCodes.ReceivableInvoice} {invoice.AH_TransactionNum}</Key>
                </DataTarget>
            </DataTargetCollection>
			<Company>
			<Code>{company.GC_Code}</Code>
			</Company>
			<EnterpriseID>{registrationKey.EnterpriseCode}</EnterpriseID>
			<ServerID>{registrationKey.ServerCode}</ServerID>
		</DataContext>
		<FilterCollection></FilterCollection>
    </DocumentRequest>
</UniversalDocumentRequest>";

			var xmlSessionTracker = ObjectFactory.Get<IXmlSessionTracker>("IXmlSessionTracker", new SimpleLogger());
			using (var processingResult = SendDocumentRequest(xmlSessionTracker, requestXML))
			{
				AssertNotNull(processingResult);
				AssertEquals("PRS", processingResult.Status);

				processingResult.ResponseMessageText.Position = 0;
				var responseMessageText = new StreamReader(processingResult.ResponseMessageText).ReadToEnd();

				AssertContains($@"<DataSourceCollection>
        <DataSource>
          <Type>AccountingInvoice</Type>
          <Key>AR {TransactionTypes.Invoice} {invoice.AH_TransactionNum}</Key>
        </DataSource>
      </DataSourceCollection>", responseMessageText);

				AssertContains($"<FileName>{fileName}</FileName>", responseMessageText);
			}
		}

		public void TestShouldLogExternalStorageExceptionInsteadOfReportingToUs()
		{
			using (SystemDataRegistry.Instance.EDocsStorageProvider.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "S3"))
			using (SystemDataRegistry.Instance.EDocsStorageServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://S3.wtg"))
			using (SystemDataRegistry.Instance.EDocsStorageAccess.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ""))
			{
				var shipment = Factory.New<Forwarding.IForwardingShipment>();
				var shipmentNumber = shipment.JS_UniqueConsignRef = "S00009001";

				var docManagerInfo = ((IDocManagerSupport)shipment).DocManagerInfo;
				var eDoc1 = CreateEDocForTest(docManagerInfo, "File1", "INV", true, new ZDateTime(2016, 7, 1, 12, 15, 00));
				var storageDocs = docManagerInfo.AllEDocs[0] as StorageDocsBase;
				storageDocs.SC_ImageData = ZBlob.Empty;
				docManagerInfo.MasterFactory.Save();
				Factory.Save();

				var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;

				var requestXml =
	$@"<UniversalDocumentRequest xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <DocumentRequest>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
          <Key>{shipmentNumber}</Key>
        </DataTarget>
      </DataTargetCollection>

      <Company>
        <Code>{GlbCompany.CurrentCompany.GC_Code}</Code>
      </Company>
      <EnterpriseID>{registrationKey.EnterpriseCode}</EnterpriseID>
      <ServerID>{registrationKey.ServerCode}</ServerID>
    </DataContext>

    <FilterCollection></FilterCollection>
  </DocumentRequest>
</UniversalDocumentRequest>";

				var xmlSessionTracker = new XmlSessionTracker(new SimpleLogger());

				using (var processingResult = SendDocumentRequest(xmlSessionTracker, requestXml))
				{
					AssertNotNull(processingResult);
					AssertEquals("ERR", processingResult.Status);
					Assert(xmlSessionTracker.HasErrors);
					AssertEquals(@"Error - Unable to access S3 storage, please contact your system administrator to check the configuration of the eDocs storage. Error message: Access credentials were not provided.", xmlSessionTracker.ToString());
				}
			}
		}

		public void TestDocManagerRequest_FindEdocInMultipleJobDeclarations()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00001000";

			var company1 = Factory.New<GlbCompany>();
			company1.GC_Code = "DUS";
			var glbBranch1 = Factory.New<GlbBranch>();
			glbBranch1.GB_Code = "AAA";
			glbBranch1.GB_GC = company1.PK;
			var jobDeclaration1 = Factory.New<Customs.US.IJobDeclaration>();
			jobDeclaration1.JE_GC = company1.PK;
			jobDeclaration1.JE_GB = glbBranch1.PK;
			jobDeclaration1.JE_JS = shipment.PK;

			var company2 = Factory.New<GlbCompany>();
			company2.GC_Code = "DAN";
			var glbBranch2 = Factory.New<GlbBranch>();
			glbBranch2.GB_Code = "BBB";
			glbBranch2.GB_GC = company2.PK;
			var jobDeclaration2 = Factory.New<Customs.CA.IJobDeclaration>();
			jobDeclaration2.JE_GC = company2.PK;
			jobDeclaration2.JE_GB = glbBranch2.PK;
			jobDeclaration2.JE_JS = shipment.PK;

			Factory.Save();

			var requestXML = "<UniversalDocumentRequest xmlns=\"http://www.cargowise.com/Schemas/Universal/2011/11\">\r\n    <DocumentRequest>\r\n        <DataContext>\r\n            <DataTargetCollection>\r\n                <DataTarget>\r\n                    <Type>DocManager</Type>\r\n                    <Key>DEC S00001000</Key>\r\n                </DataTarget>\r\n            </DataTargetCollection>\r\n            <Company>\r\n                <Code>DUS</Code>\r\n            </Company>\r\n            <EnterpriseID>EDI</EnterpriseID>\r\n            <ServerID>DAT</ServerID>\r\n        </DataContext>\r\n        <ReturnDocumentDescriptionsOnly>false</ReturnDocumentDescriptionsOnly>\r\n    </DocumentRequest>\r\n</UniversalDocumentRequest>";
			var xmlSessionTracker = ObjectFactory.Get<IXmlSessionTracker>("IXmlSessionTracker", new SimpleLogger());
			using (SendDocumentRequest(xmlSessionTracker, requestXML))
			{
				foreach (var log in xmlSessionTracker.Logs)
				{
					AssertNotEquals("Could not find valid constructor for multiple declarations", "The unique ID S00001000 you have supplied for Reference Type DEC exists in multiple records.", log.Message);
				}
			}
		}

		public void TestDocumentRequest_WithEmptyEdoc()
		{
			using (SystemDataRegistry.Instance.EDocsStorageProvider.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "S3"))
			{
				var persisterMock = new Mock<IExternalPersister>();
				var persisterProviderMock = new Mock<IExternalPersisterProvider>();
				persisterProviderMock.Setup(x => x.GetExternalPersister(Core.Constants.EDocsStorageProviders.Code.S3)).Returns(persisterMock.Object);
				ObjectFactory.Substitute(persisterProviderMock.Object);

				var shipment = Factory.New<Forwarding.IForwardingShipment>();
				var shipmentNumber = shipment.JS_UniqueConsignRef = "S00009001";

				var docManagerInfo = ((IDocManagerSupport)shipment).DocManagerInfo;

				var eDoc1 = CreateEDocForTest(docManagerInfo, "File1", "INV", true, new ZDateTime(2016, 7, 1, 12, 15, 00)) as StorageDocsBase;
				var eDoc2 = CreateEDocForTest(docManagerInfo, "File2", "INV", true, new ZDateTime(2016, 7, 1, 12, 15, 00)) as StorageDocsBase;
				eDoc1.SC_ImageData = ZBlob.Empty;
				eDoc1.SC_UncompressedSize = 0;
				docManagerInfo.MasterFactory.Save();
				Factory.Save();

				persisterMock.Setup(x => x.RetrieveStream(It.IsAny<ZGuid>())).Throws(new ExternalStorageObjectNotFoundException(eDoc1.PK.ToString(), "Original document is empty", "S3", null));

				var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;

				var requestXml =
	$@"<UniversalDocumentRequest xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <DocumentRequest>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
          <Key>{shipmentNumber}</Key>
        </DataTarget>
      </DataTargetCollection>

      <Company>
        <Code>{GlbCompany.CurrentCompany.GC_Code}</Code>
      </Company>
      <EnterpriseID>{registrationKey.EnterpriseCode}</EnterpriseID>
      <ServerID>{registrationKey.ServerCode}</ServerID>
    </DataContext>

    <FilterCollection></FilterCollection>
  </DocumentRequest>
</UniversalDocumentRequest>";

				var xmlSessionTracker = new XmlSessionTracker(new SimpleLogger());
				using (var processingResult = SendDocumentRequest(xmlSessionTracker, requestXml))
				{
					AssertNotNull(processingResult);
					AssertEquals("PRS", processingResult.Status);

					processingResult.ResponseMessageText.Position = 0;
					var responseMessageText = new StreamReader(processingResult.ResponseMessageText).ReadToEnd();

					// Contains both files, and no error
					AssertContains($"<FileName>{eDoc1.GetFileNameOnlyWithExtension()}</FileName>", responseMessageText);
					AssertContains($"<FileName>{eDoc2.GetFileNameOnlyWithExtension()}</FileName>", responseMessageText);
					Assert(!xmlSessionTracker.HasErrors);
				}
			}
		}

		public void TestDocumentRequest_WithGiantEdoc()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			var shipmentNumber = shipment.JS_UniqueConsignRef = "S00009001";

			var docManagerInfo = ((IDocManagerSupport)shipment).DocManagerInfo;

			ZBlob MakeABigEdoc()
			{
				var size = 100000000;
				var bytes = new byte[size];
				var random = new Random(1337_807);
				random.NextBytes(bytes);
				return new ZBlob(bytes);
			}

			var eDoc1 = CreateEDocForTest(docManagerInfo, "File1", "INV", true, new ZDateTime(2016, 7, 1, 12, 15, 00), MakeABigEdoc);
			docManagerInfo.MasterFactory.Save();
			Factory.Save();

			var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;

			var requestXml =
$@"<UniversalDocumentRequest xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <DocumentRequest>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
          <Key>{shipmentNumber}</Key>
        </DataTarget>
      </DataTargetCollection>

      <Company>
        <Code>{GlbCompany.CurrentCompany.GC_Code}</Code>
      </Company>
      <EnterpriseID>{registrationKey.EnterpriseCode}</EnterpriseID>
      <ServerID>{registrationKey.ServerCode}</ServerID>
    </DataContext>

    <FilterCollection></FilterCollection>
  </DocumentRequest>
</UniversalDocumentRequest>";

			var xmlSessionTracker = new XmlSessionTracker(new SimpleLogger());
			using (var processingResult = SendDocumentRequest(xmlSessionTracker, requestXml))
			{
				AssertNotNull(processingResult);
				AssertEquals("PRS", processingResult.Status);
				AssertResponseMessageLinkedToParentByDexEvent(shipment as BusinessObject);

				processingResult.ResponseMessageText.Position = 0;
				var responseMessageText = new StreamReader(processingResult.ResponseMessageText).ReadToEnd();

				AssertContains(
	$@"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>ForwardingShipment</Type>
          <Key>{shipmentNumber}</Key>
        </DataSource>
      </DataSourceCollection>", responseMessageText);

				AssertContains($"<FileName>{eDoc1.FileName}</FileName>", responseMessageText);
			}
		}

		public void TestReturnDocumentDescriptionsOnly()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			var shipmentNumber = shipment.JS_UniqueConsignRef = "S00009001";

			var docManagerInfo = ((IDocManagerSupport)shipment).DocManagerInfo;
			var eDoc1 = CreateEDocForTest(docManagerInfo, "File1", "INV", true, new ZDateTime(2016, 7, 1, 12, 15, 00));
			docManagerInfo.MasterFactory.Save();
			Factory.Save();

			var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;

			var requestXml =
$@"<UniversalDocumentRequest xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
<DocumentRequest>
<DataContext>
    <DataTargetCollection>
    <DataTarget>
        <Type>ForwardingShipment</Type>
        <Key>{shipmentNumber}</Key>
    </DataTarget>
    </DataTargetCollection>

    <Company>
    <Code>{GlbCompany.CurrentCompany.GC_Code}</Code>
    </Company>
    <EnterpriseID>{registrationKey.EnterpriseCode}</EnterpriseID>
    <ServerID>{registrationKey.ServerCode}</ServerID>
</DataContext>
<ReturnDocumentDescriptionsOnly>PLACE_HOLDER</ReturnDocumentDescriptionsOnly>
<FilterCollection></FilterCollection>
</DocumentRequest>
</UniversalDocumentRequest>";

			var xmlSessionTracker = new XmlSessionTracker(new SimpleLogger());
			using (var trueResult = SendDocumentRequest(xmlSessionTracker, requestXml.Replace("PLACE_HOLDER", "true")))
			using (var falseResult = SendDocumentRequest(xmlSessionTracker, requestXml.Replace("PLACE_HOLDER", "false")))
			using (var emptyResult = SendDocumentRequest(xmlSessionTracker, requestXml.Replace("PLACE_HOLDER", "")))
			{
				AssertNotNull(trueResult);
				AssertEquals("PRS", trueResult.Status);
				var trueMessageText = new StreamReader(trueResult.ResponseMessageText).ReadToEnd();
				AssertContains($"<FileName>{eDoc1.FileName}</FileName>", trueMessageText);
				var falseMessageText = new StreamReader(falseResult.ResponseMessageText).ReadToEnd();
				var emptyMessageText = new StreamReader(emptyResult.ResponseMessageText).ReadToEnd();

				CombineAssertions("ImageData should be excluded when ReturnDocumentDescriptionsOnly is true", () =>
				{
					AssertNotContains("<ImageData>", trueMessageText);
					AssertContains("<ImageData>", falseMessageText);
					AssertContains("<ImageData>", emptyMessageText);
				});
			}
		}

		public void TestWithRelatedObjectsCastIeDoc()
		{
			var declaration = Factory.New<Customs.IBaseJobDeclaration>();
			declaration.JE_DeclarationReference = "B00009002";
			var custEntryHeader = Factory.New<Customs.ICusEntryHeader>();
			custEntryHeader["CH_JE"] = declaration.PK;
			custEntryHeader["CH_BGMReference"] = "B00009003";
			var bizo = declaration as BusinessObject;

			var parentDocManagerInfo = ((IDocManagerSupport)declaration).DocManagerInfo;
			var childDocMangaerInfo = ((IDocManagerSupport)custEntryHeader).DocManagerInfo;

			var eDoc1 = CreateEDocForTest(parentDocManagerInfo, "File1", "EPR", true, new ZDateTime(2016, 7, 1, 12, 15, 00));
			var eDoc2 = CreateEDocForTest(childDocMangaerInfo, "File2", "EPR", true, new ZDateTime(2017, 7, 1, 12, 15, 00));

			parentDocManagerInfo.MasterFactory.Save();
			childDocMangaerInfo.MasterFactory.Save();
			Factory.Save();

			var documentFactory = new DocumentFactoryProvider().GetFactory(Factory);
			var parentMain = documentFactory.New<StorageMain>();
			parentMain.SM_Type = Core.Constants.DocManagerCodes.CustomsEntry;
			parentMain.SM_ParentFK = custEntryHeader.PK;

			AssertEquals(2, parentDocManagerInfo.GetRelatedEDocs().ToList().Count);
			AssertExceptionThrown<InvalidCastException>("Unable to cast object of type 'Enterprise.Customs.AU.Declaration.Business.CusEntryHeader' to type 'Enterprise.MasterFiles.Integration.IeDoc'", () => parentDocManagerInfo.RelatedObjects.Cast<IeDoc>().ToList());
		}

		public void TestProcessWithRelatedEDocFilter()
		{
			var declaration = Factory.New<Customs.IBaseJobDeclaration>();
			declaration.JE_DeclarationReference = "B00009001";
			var custEntryHeader = Factory.New<Customs.ICusEntryHeader>();
			custEntryHeader["CH_JE"] = declaration.PK;
			custEntryHeader["CH_BGMReference"] = "B00009002";
			var custEntryHeader2 = Factory.New<Customs.ICusEntryHeader>();
			custEntryHeader2["CH_JE"] = declaration.PK;
			custEntryHeader2["CH_BGMReference"] = "B00009003";

			var bizo = declaration as BusinessObject;

			var parentDocManagerInfo = ((IDocManagerSupport)declaration).DocManagerInfo;
			var childDocMangaerInfo = ((IDocManagerSupport)custEntryHeader).DocManagerInfo;

			var eDoc1 = CreateEDocForTest(parentDocManagerInfo, "File1", "EPR", true, new ZDateTime(2016, 7, 1, 12, 15, 00));
			var eDoc4 = CreateEDocForTest(parentDocManagerInfo, "File4", "FRF", true, new ZDateTime(2020, 7, 1, 12, 15, 00));
			var eDoc2 = CreateEDocForTest(childDocMangaerInfo, "File2", "EPR", true, new ZDateTime(2017, 7, 1, 12, 15, 00));
			var eDoc3 = CreateEDocForTest(childDocMangaerInfo, "File3", "FRF", true, new ZDateTime(2018, 7, 1, 12, 15, 00));

			parentDocManagerInfo.MasterFactory.Save();
			childDocMangaerInfo.MasterFactory.Save();
			Factory.Save();

			AssertEquals(4, parentDocManagerInfo.GetRelatedEDocs().ToList().Count);

			AssertProcessRequestWithCusFilter(bizo, declaration.JE_DeclarationReference,
			null,
			new[] { eDoc1, eDoc4 });

			AssertProcessRequestWithCusFilter(bizo, declaration.JE_DeclarationReference,
			new[]
			{
			new Tuple<string, string>("RelatedEDoc","Customs Entry B00009002"),
			new Tuple<string, string>("DocumentType","FRF"),
			},
			new[] { eDoc3, eDoc4 });

			AssertProcessRequestWithCusFilter(bizo, declaration.JE_DeclarationReference,
			new[]
			{
			new Tuple<string, string>("RelatedEDoc","ASDASDAS"),
			},
			new[] { eDoc1, eDoc4 });

			AssertProcessRequestWithCusFilter(bizo, declaration.JE_DeclarationReference,
			new[]
			{
			new Tuple<string, string>("RelatedEDoc","Customs Entry B00009002"),
			new Tuple<string, string>("DocumentType","EPR"),
			},
			new[] { eDoc1, eDoc2 });

			AssertProcessRequestWithCusFilter(bizo, declaration.JE_DeclarationReference,
			new[]
			{
			new Tuple<string, string>("RelatedEDoc","Customs Entry B00009002"),
			new Tuple<string, string>("RelatedEDoc","Customs Entry B33333333")
			},
			new[] { eDoc1, eDoc2, eDoc3, eDoc4 });

			AssertProcessRequestWithCusFilter(bizo, declaration.JE_DeclarationReference,
			new[]
			{
			new Tuple<string, string>("RelatedEDoc",""),
			new Tuple<string, string>("RelatedEDoc","Customs Entry B00009002")
			},
			new[] { eDoc1, eDoc2, eDoc3, eDoc4 });

			AssertProcessRequestWithCusFilter(bizo, declaration.JE_DeclarationReference,
			new[]
			{
			new Tuple<string, string>("RelatedEDoc","Customs Entry B00009002")
			},
			new[] { eDoc1, eDoc2, eDoc3, eDoc4 });

			AssertProcessRequestWithCusFilter(bizo, declaration.JE_DeclarationReference,
			new[]
			{
			new Tuple<string, string>("RelatedEDoc","")
			},
			new[] { eDoc1, eDoc2, eDoc3, eDoc4 });

			AssertProcessRequestWithCusFilter(bizo, declaration.JE_DeclarationReference,
			new[]
			{
			new Tuple<string, string>("RelatedEDoc",""),
			new Tuple<string, string>("RelatedEDoc","Customs Entry B00009002"),
			new Tuple<string, string>("DocumentType","EPR")
			},
			new[] { eDoc1, eDoc2 });

			AssertProcessRequestWithCusFilter(bizo, declaration.JE_DeclarationReference,
			new[]
			{
			new Tuple<string, string>("RelatedEDoc",""),
			new Tuple<string, string>("RelatedEDoc","Customs Entry B00009002"),
			new Tuple<string, string>("DocumentType","FRF")
			},
			new[] { eDoc3, eDoc4 });

			AssertProcessRequestWithCusFilter(bizo, declaration.JE_DeclarationReference,
			new[]
			{
			new Tuple<string, string>("RelatedEDoc","AAA"),
			new Tuple<string, string>("DocumentType","FRF")
			},
			new[] { eDoc4 });
		}

		[TestDate(2016, 7, 1, 12, 15, 00)]
		public void TestProcessWithFilter()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00009001";
			var bizo = shipment as BusinessObject;

			var docManagerInfo = ((IDocManagerSupport)shipment).DocManagerInfo;
			var eDoc1 = CreateEDocForTest(docManagerInfo, "File1", "INV", true, new ZDateTime(2016, 7, 1, 12, 15, 00));
			docManagerInfo.MasterFactory.Save();

			TestDateAttribute.Date = new DateTime(2016, 7, 1, 15, 30, 00);
			var eDoc2 = CreateEDocForTest(docManagerInfo, "File2", "VET", false, new ZDateTime(2016, 7, 1, 15, 30, 00), GlbCompany.CurrentCompany.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid());
			docManagerInfo.MasterFactory.Save();

			TestDateAttribute.Date = new DateTime(2016, 7, 2, 12, 15, 00);
			var eDoc3 = CreateEDocForTest(docManagerInfo, "File3", "MSC", true, new ZDateTime(2016, 7, 2, 12, 15, 00));
			docManagerInfo.MasterFactory.Save();

			TestDateAttribute.Date = new DateTime(2016, 7, 3, 12, 15, 00);
			var eDoc4 = CreateEDocForTest(docManagerInfo, "File4", "ARN", false, new ZDateTime(2016, 7, 3, 12, 15, 00), GlbCompany.CurrentCompany.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid());
			docManagerInfo.MasterFactory.Save();

			TestDateAttribute.Date = new DateTime(2016, 7, 4, 12, 15, 00);
			var eDoc5 = CreateEDocForTest(docManagerInfo, "File5", "DAL", true, new ZDateTime(2016, 7, 4, 12, 15, 00));
			docManagerInfo.MasterFactory.Save();

			TestDateAttribute.Date = new DateTime(2016, 7, 5, 12, 15, 00);
			var eDoc6 = CreateEDocForTest(docManagerInfo, "File6", "DOR", false, new ZDateTime(2016, 7, 5, 12, 15, 00));
			docManagerInfo.MasterFactory.Save();

			Factory.Save();

			AssertProcessRequestWithFilter(bizo, "S00009999", null, null, expectError: true, expectedErrorText: "There is no business object matching the criteria.");

			AssertProcessRequestWithFilter(bizo, shipment.JS_UniqueConsignRef,
				null,
				new[] { eDoc1, eDoc2, eDoc3, eDoc4, eDoc5, eDoc6 });

			AssertProcessRequestWithFilter(bizo, shipment.JS_UniqueConsignRef,
				new[] { new Tuple<string, string>("IsPublished", "True") },
				new[] { eDoc1, eDoc3, eDoc5 },
				new[] { eDoc2, eDoc4, eDoc6 });

			AssertProcessRequestWithFilter(bizo, shipment.JS_UniqueConsignRef,
				new[]
				{
					new Tuple<string, string>("DocumentType", "DOR"),
					new Tuple<string, string>("DocumentType", "VET")
				},
				new[] { eDoc2, eDoc6 },
				new[] { eDoc1, eDoc3, eDoc4, eDoc5 });

			AssertProcessRequestWithFilter(bizo, shipment.JS_UniqueConsignRef,
				new[]
				{
					new Tuple<string, string>("SaveDateUTCFrom", "2016-07-01T15:00"),
					new Tuple<string, string>("SaveDateUTCTo", "2016-07-03T00:00")
				},
				new[] { eDoc2, eDoc3 },
				new[] { eDoc1, eDoc4, eDoc5, eDoc6 });

			AssertProcessRequestWithFilter(bizo, shipment.JS_UniqueConsignRef,
				new[]
				{
					new Tuple<string, string>("DocumentType", "VET"),
					new Tuple<string, string>("SaveDateUTCFrom", "2016-07-01T15:00"),
					new Tuple<string, string>("SaveDateUTCTo", "2016-07-03T00:00")
				},
				new[] { eDoc2 },
				new[] { eDoc1, eDoc3, eDoc4, eDoc5, eDoc6 });

			AssertProcessRequestWithFilter(bizo, shipment.JS_UniqueConsignRef,
				new[]
				{
					new Tuple<string, string>("CompanyCode", GlbCompany.CurrentCompany.GC_Code)
				},
				new[] { eDoc2, eDoc4 },
				new[] { eDoc1, eDoc3, eDoc5, eDoc6 });

			AssertProcessRequestWithFilter(bizo, shipment.JS_UniqueConsignRef,
				new[]
				{
					new Tuple<string, string>("BranchCode", GlbBranch.CurrentBranch.GB_Code)
				},
				new[] { eDoc2, eDoc4 },
				new[] { eDoc1, eDoc3, eDoc5, eDoc6 });

			AssertProcessRequestWithFilter(bizo, shipment.JS_UniqueConsignRef,
				new[]
				{
					new Tuple<string, string>("DepartmentCode", GlbDepartment.CurrentDepartment.GE_Code)
				},
				new[] { eDoc2, eDoc4 },
				new[] { eDoc1, eDoc3, eDoc5, eDoc6 });

			AssertProcessRequestWithFilter(bizo, shipment.JS_UniqueConsignRef,
				new[]
				{
					new Tuple<string, string>(nameof(DocumentFilterType.FileName), eDoc1.FileName)
				},
				new[] { eDoc1 });

			AssertProcessRequestWithFilter(bizo, shipment.JS_UniqueConsignRef,
				new[]
				{
					new Tuple<string, string>(nameof(DocumentFilterType.DocumentID), eDoc1.UniqueKey.ToString())
				},
				new[] { eDoc1 });

			AssertProcessRequestWithFilter(bizo, shipment.JS_UniqueConsignRef,
				new[]
				{
					new Tuple<string, string>(nameof(DocumentFilterType.DocumentID), eDoc1.UniqueKey.ToString()),
					new Tuple<string, string>(nameof(DocumentFilterType.FileName), eDoc1.FileName),
					new Tuple<string, string>(nameof(DocumentFilterType.FileName), eDoc2.FileName)
				},
				new[] { eDoc1 });

			AssertProcessRequestWithFilter(bizo, shipment.JS_UniqueConsignRef,
				new[]
				{
					new Tuple<string, string>("IsPublished", "True"),
					new Tuple<string, string>("IsPublished", "False")
				},
				null,
				expectError: true, expectedErrorText: "Both True and False are specified for filter IsPublished.");

			AssertProcessRequestWithFilter(bizo, shipment.JS_UniqueConsignRef,
				new[]
				{
					new Tuple<string, string>("SaveDateUTCFrom", "2016-07-07a"),
				},
				null,
				expectError: true, expectedErrorText: "Cannot parse date and time '2016-07-07a'.");

			AssertProcessRequestWithFilter(bizo, shipment.JS_UniqueConsignRef,
				new[]
				{
					new Tuple<string, string>(nameof(DocumentFilterType.DocumentID), "2016-07-07a"),
				},
				null,
				expectError: true, expectedErrorText: "Cannot parse guid filter DocumentID value from text '2016-07-07a'.");
		}

		void AssertProcessRequestWithFilter(BusinessObject bizo, string shipmentNumber, Tuple<string, string>[] filters, IeDoc[] expectedEDocs, IeDoc[] notExpectedEDocs = null, bool expectError = false, string expectedErrorText = null)
		{
			var filtersText = new StringBuilder();
			if (filters != null)
			{
				foreach (var filter in filters)
				{
					filtersText.Append($@"
      <Filter>
        <Type>{filter.Item1}</Type>
        <Value>{filter.Item2}</Value>
      </Filter>");
				}
			}

			var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;

			var requestXml =
$@"<UniversalDocumentRequest xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <DocumentRequest>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
          <Key>{shipmentNumber}</Key>
        </DataTarget>
      </DataTargetCollection>

      <Company>
        <Code>{GlbCompany.CurrentCompany.GC_Code}</Code>
      </Company>
      <EnterpriseID>{registrationKey.EnterpriseCode}</EnterpriseID>
      <ServerID>{registrationKey.ServerCode}</ServerID>
    </DataContext>

    <FilterCollection>{filtersText}
    </FilterCollection>
  </DocumentRequest>
</UniversalDocumentRequest>";

			var xmlSessionTracker = new XmlSessionTracker(new SimpleLogger());
			var handler = new UniversalDocumentRequestHandler(xmlSessionTracker);
			var request = handler.CreateRequestMessage();
			var response = handler.CreateResponseMessage();
			using (var stream = (SubStreamableStream)new MemoryStream())
			{
				new StreamWriter(stream) { AutoFlush = true }.Write(requestXml);
				request.SetMessageTextSource(stream);
				request.Save();
			}

			var responseMessageSaver = new UniversalResponseSaver(request, response, xmlSessionTracker);
			using (var processingResult = handler.Process(request, responseMessageSaver))
			{
				AssertNotNull(processingResult);

				if (expectError)
				{
					AssertEquals("ERR", processingResult.Status);
					AssertContains(expectedErrorText, xmlSessionTracker.ToString());
					return;
				}

				AssertEquals("PRS", processingResult.Status);
				AssertResponseMessageLinkedToParentByDexEvent(bizo);

				processingResult.ResponseMessageText.Position = 0;
				var responseMessageText = new StreamReader(processingResult.ResponseMessageText).ReadToEnd();

				AssertContains(
	$@"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>ForwardingShipment</Type>
          <Key>{shipmentNumber}</Key>
        </DataSource>
      </DataSourceCollection>", responseMessageText);

				if (expectedEDocs != null)
				{
					foreach (var eDoc in expectedEDocs)
					{
						AssertContains($"<FileName>{eDoc.FileName}</FileName>", responseMessageText);
					}
				}

				if (notExpectedEDocs != null)
				{
					foreach (var eDoc in notExpectedEDocs)
					{
						AssertNotContains($"<FileName>{eDoc.FileName}</FileName>", responseMessageText);
					}
				}

				AssertNotContains("Should not contain trigger information", "<EventBranch>", responseMessageText);
			}
		}

		IeDoc CreateEDocForTest(DocManagerInfo docManagerInfo, string fileName, string docType, bool isPublished, ZDateTime docDateTime, Func<ZBlob> blobFactory = null)
		{
			return CreateEDocForTest(docManagerInfo, fileName, docType, isPublished, docDateTime, Guid.Empty, Guid.Empty, Guid.Empty, blobFactory);
		}

		IeDoc CreateEDocForTest(DocManagerInfo docManagerInfo, string fileName, string docType, bool isPublished, ZDateTime docDateTime, Guid company, Guid branch, Guid department, Func<ZBlob> blobFactory = null)
		{
			var eDoc = docManagerInfo.AddFileOrDocument(
				blobFactory?.Invoke() ?? new ZBlob(new byte[] { 1, 2, 3 }),
				fileName,
				docType,
				visibleCompanyPK: company,
				visibleBranchPK: branch,
				visibleDepartmentPK: department);
			eDoc.IsPublished = isPublished;
			eDoc.SetValuesForTest(docDateTime, docType);

			return eDoc;
		}

		void AssertProcessRequestWithCusFilter(BusinessObject bizo, string declartionNumber, Tuple<string, string>[] filters, IeDoc[] expectedEDocs, IeDoc[] notExpectedEDocs = null, bool expectError = false, string expectedErrorText = null)
		{
			var filtersText = new StringBuilder();
			if (filters != null)
			{
				foreach (var filter in filters)
				{
					filtersText.Append($@"
      <Filter>
        <Type>{filter.Item1}</Type>
        <Value>{filter.Item2}</Value>
      </Filter>");
				}
			}

			var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;

			var requestXml =
$@"<UniversalDocumentRequest xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <DocumentRequest>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>CustomsDeclaration</Type>
          <Key>{declartionNumber}</Key>
        </DataTarget>
      </DataTargetCollection>

      <Company>
        <Code>{GlbCompany.CurrentCompany.GC_Code}</Code>
      </Company>
      <EnterpriseID>{registrationKey.EnterpriseCode}</EnterpriseID>
      <ServerID>{registrationKey.ServerCode}</ServerID>
    </DataContext>

    <FilterCollection>{filtersText}
    </FilterCollection>
  </DocumentRequest>
</UniversalDocumentRequest>";

			var xmlSessionTracker = new XmlSessionTracker(new SimpleLogger());
			var handler = new UniversalDocumentRequestHandler(xmlSessionTracker);
			var request = handler.CreateRequestMessage();
			var response = handler.CreateResponseMessage();
			using (var stream = (SubStreamableStream)new MemoryStream())
			{
				new StreamWriter(stream) { AutoFlush = true }.Write(requestXml);
				request.SetMessageTextSource(stream);
				request.Save();
			}

			var responseMessageSaver = new UniversalResponseSaver(request, response, xmlSessionTracker);
			using (var processingResult = handler.Process(request, responseMessageSaver))
			{
				AssertNotNull(processingResult);

				if (expectError)
				{
					AssertEquals("ERR", processingResult.Status);
					AssertContains(expectedErrorText, xmlSessionTracker.ToString());
					return;
				}

				AssertEquals("PRS", processingResult.Status);
				AssertResponseMessageLinkedToParentByDexEvent(bizo);

				processingResult.ResponseMessageText.Position = 0;
				var responseMessageText = new StreamReader(processingResult.ResponseMessageText).ReadToEnd();

				AssertContains(
	$@"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>CustomsDeclaration</Type>
          <Key>{declartionNumber}</Key>
        </DataSource>
      </DataSourceCollection>", responseMessageText);

				if (expectedEDocs != null)
				{
					foreach (var eDoc in expectedEDocs)
					{
						AssertContains($"<FileName>{eDoc.FileName}</FileName>", responseMessageText);
					}
				}

				if (expectedEDocs != null)
				{
					foreach (var eDoc in expectedEDocs)
					{
						var currentFileLength = $"<FileName>{eDoc.FileName}</FileName>".Length;
						var newMessage = responseMessageText.Replace($"<FileName>{eDoc.FileName}</FileName>", "");
						AssertEquals($"There are duplicated response", newMessage.Length + currentFileLength, responseMessageText.Length);
					}
				}

				if (notExpectedEDocs != null)
				{
					foreach (var eDoc in notExpectedEDocs)
					{
						AssertNotContains($"<FileName>{eDoc.FileName}</FileName>", responseMessageText);
					}
				}

				AssertNotContains("Should not contain trigger information", "<EventBranch>", responseMessageText);
			}
		}
	}
}
