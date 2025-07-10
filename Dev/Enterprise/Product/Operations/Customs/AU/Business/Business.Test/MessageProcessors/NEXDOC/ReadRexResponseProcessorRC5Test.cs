using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.AU;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(ReadRexResponseProcessorRC5))]
	sealed class ReadRexResponseProcessorRC5Test : NEXDOCMessageProcessorAbstractTest
	{
		public void TestProcessReadRexResponse()
		{
			SetupEmailGroups();
			CreateSupplier("TSUP");
			CreateImporter("TIMP", "INTERNAT INST OF TROPIC AGRIC");
			CreateManufacturer("TMFR", "FITZROY KEBABS");
			CreateAQISEstablishment("TAQS", "AQIS SYDNEY");

			var message = CreateNEXDOCInterchangeAndMessageFromUniversalXML(embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("ReadREX_response.xml")));
			Factory.Save();

			var messageProcessor = new ReadRexResponseProcessorRC5(new LoggingInformation());
			messageProcessor.ProcessMessage(message);
			Factory.Save();

			message.Reload();
			AssertEquals("message processed (EM_Status)", EDIMessage.Status.Received, message.EM_Status);

			var otherFactory = new BusinessObjectFactory();
			var invoices = otherFactory.Load<JobComInvoiceHeader>(new ZQuery(JobComInvoiceHeaderSchema.JZ_InvoiceNumber, "INV000123"));
			AssertEquals("commercial invoice created", 1, invoices.Length);
			var invoice = invoices[0];
			AssertEquals(GlbBranch.CurrentBranch.PK, invoice.JZ_GB);

			new Customs.Business.FakeDeclarationCreatorForInvoice(invoice);

			// has expected values
			AssertEquals(AUJobMessageTypeList.Codes.Quarantine, invoice.JZ_MessageType);
			AssertEquals("TSUP", invoice.Supplier.OH_Code);
			AssertEquals("TIMP", invoice.Buyer.OH_Code);
			AssertEquals("AUD", invoice.JZ_RX_NKInvoice_Currency);
			AssertEquals("BKG190912REF4", invoice.JZ_ExporterReference);
			AssertEquals("JZ_InvoiceAmount", 5800.0m, invoice.JZ_InvoiceAmount);
			AssertEquals("JZ_IncoTerm", Core.Constants.IncoTerms.FreeOnBoard, invoice.JZ_IncoTerm);

			var containerRefs = invoice.InvoiceHeaderRefs.Find(r => r.J2_ReferenceType == Customs.Business.InvoiceHeaderRefsTypeList.Codes.CN).Select(i => i.J2_ReferenceNumber).ToArray();
			AssertContainsExactElementsInAnyOrder("containerRefs", new string[] { "MAEU9304911", "MAEU9302930" }, containerRefs);

			var quarantineHeader = invoice.QuarantineExDocHeader;
			AssertEquals("AU", quarantineHeader.QH_RN_NKOriginCountry);
			AssertEquals("", quarantineHeader.QH_RL_NKBorderInspectionPort);
			AssertEquals(true, quarantineHeader.QH_ObtainExportCustomsPermit);
			AssertEquals(false, quarantineHeader.QH_ShipsStores);
			AssertEquals("77", quarantineHeader.QH_AuthorisationEstablishment);
			AssertEquals(new ZDate(2019, 06, 26), quarantineHeader.QH_AuthorisationDate);
			AssertEquals("QH_AuthorisationComments", "NICE WORK", quarantineHeader.QH_AuthorisationComments);
			AssertEquals("QH_ExportPermitNumber", "12345678901234567890123456789012345", quarantineHeader.QH_ExportPermitNumber);
			AssertEquals("ZA_EDN", "12345678901234567890123456789012345", invoice.AddInfo.ZA_EDN_Hidden);
			AssertEquals("QH_LastAmendDateTime", new ZDateTimeOffset(2019, 9, 12, 16, 17, 45, 1, System.TimeSpan.FromHours(10)), quarantineHeader.QH_LastAmendDateTime);
			AssertEquals("QH_ExporterDeclaration", "MEAT IS SOURCED FROM HGP FREE CATTLE", quarantineHeader.QH_ExporterDeclaration);

			var exporterDeclarationCodes = quarantineHeader.SupportingInfos.Cast<QuarantineSupportingInfo>().Select(i => i.CSI_Description).ToArray();
			AssertContainsExactElementsInAnyOrder("exporterDeclarationCodes", new string[] { "TAC", "DYCOMP" }, exporterDeclarationCodes);

			AssertEquals("NO", quarantineHeader.QH_ImportedProductFlag);
			AssertEquals("NO", quarantineHeader.QH_LegallyImportedFlag);
			AssertEquals("NO", quarantineHeader.QH_ManufacturedTreatedPackagedLabelledInAustralia);

			AssertEquals(1.00M, quarantineHeader.QH_AbsoluteTemperature);
			AssertEquals("CEL", quarantineHeader.QH_TemperatureUM);
			AssertEquals("124DF", quarantineHeader.QH_StartHoldSeal);
			AssertEquals("KD3", quarantineHeader.QH_EndHoldSeal);
			AssertEquals("DAI", quarantineHeader.QH_ProduceType);
			AssertEquals("H", quarantineHeader.QH_ProductUseIndicator);

			AssertEquals("M", quarantineHeader.QH_CertificatePrintIndicator);
			AssertEquals("CGAA02417", quarantineHeader.QH_CertificateRequiredLocation);
			AssertEquals(true, quarantineHeader.QH_SplitHealthCertByContainer);
			AssertEquals(false, quarantineHeader.QH_SplitHealthCertByMarks);
			AssertEquals(false, quarantineHeader.QH_SplitHealthCertByPacker);

			AssertEquals("REX0000044172", quarantineHeader.QH_RequestForPermitNumber);
			AssertEquals(CusEntryNumber.EntryType.RequestForPermitStatus, quarantineHeader.RequestForPermitEntryType);
			AssertEquals(EXDOCComplianceStatusCodesForCusEntryNumber.Codes.CtrdCertificateReady, quarantineHeader.RequestForPermitStatus);

			var lines = invoice.JobComInvoiceLines.Cast<JobComInvoiceLine>();
			AssertLine1(lines.First(x => x.JI_LineNo == 1));
			AssertLine2(lines.First(x => x.JI_LineNo == 2));
			AssertLine3(lines.First(x => x.JI_LineNo == 3));
			AssertEquals(3, invoice.JobComInvoiceLines.Count);

			AssertEquals("Message Linked to quarantine header", quarantineHeader.PK, message.EM_LinkedObject.PK);

			// check for email sent.
			AssertEquals("An Email should be generated", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			var responseEmail = Env.OutgoingMailManager.EmailsCreated.Last();
			AssertEquals("Response Email Subject", "NEXDOC Notification Advice", responseEmail.Subject);
			AssertContains("Response Email Message", "A Transferred REX REX0000044172 Has Been Received", responseEmail.Body);
			AssertContains("Response Email Details", "REX Number: REX0000044172", responseEmail.Body);
		}

		public void TestProcessReadRexResponse_EnumTagsExcluded()
		{
			SetupEmailGroups();
			CreateSupplier("TSUP");
			CreateImporter("TIMP", "INTERNAT INST OF TROPIC AGRIC");
			CreateManufacturer("TMFR", "FITZROY KEBABS");
			CreateAQISEstablishment("TAQS", "AQIS SYDNEY");

			var message = CreateNEXDOCInterchangeAndMessageFromUniversalXML(embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("ReadREX_response_enums_excluded_RC4.xml")));
			Factory.Save();

			var messageProcessor = new ReadRexResponseProcessorRC5(new LoggingInformation());
			messageProcessor.ProcessMessage(message);
			Factory.Save();

			message.Reload();
			AssertEquals("message processed (EM_Status)", EDIMessage.Status.Received, message.EM_Status);

			var otherFactory = new BusinessObjectFactory();
			var invoices = otherFactory.Load<JobComInvoiceHeader>(new ZQuery(JobComInvoiceHeaderSchema.JZ_InvoiceNumber, "INV000123"));
			var invoice = invoices[0];
			new Customs.Business.FakeDeclarationCreatorForInvoice(invoice);

			// enum based properties have default values
			var quarantineHeader = invoice.QuarantineExDocHeader;
			AssertEquals("QH_LastAmendDateTime", ZDateTimeOffset.Empty, quarantineHeader.QH_LastAmendDateTime);
			AssertEquals("QH_ProductUseIndicator", ZString.Empty, quarantineHeader.QH_ProductUseIndicator);
			AssertEquals("QH_AuthorisationDate", ZDateTime.Empty, quarantineHeader.QH_AuthorisationDate);
			AssertEquals("QH_CertificatePrintIndicator", ZString.Empty, quarantineHeader.QH_CertificatePrintIndicator);
			AssertEquals("QH_SplitHealthCertByContainer", false, quarantineHeader.QH_SplitHealthCertByContainer);
			AssertEquals("QH_SplitHealthCertByMarks", false, quarantineHeader.QH_SplitHealthCertByMarks);
			AssertEquals("QH_SplitHealthCertByPacker", false, quarantineHeader.QH_SplitHealthCertByPacker);

			AssertEquals("CE_EntryStatus", ZString.Empty, quarantineHeader.RequestForPermitStatus);

			var invoiceLine1 = invoice.JobComInvoiceLines.Cast<JobComInvoiceLine>().First(x => x.JI_LineNo == 1);
			AssertEquals("JI_TempImportDate", ZDateTime.Empty, invoiceLine1.JI_TempImportDate);
			AssertEquals("JI_LinePrice", ZDecimal.Zero, invoiceLine1.JI_LinePrice);

			var quarantineLine1 = invoiceLine1.QuarantineExDocLine;
			var treatmentProcess = quarantineLine1.Processes.Cast<QuarantineExDocEstablishmentAndTime>().First(p => p.EE_TreatmentCode == "HEAT");
			AssertEquals("treatmentProcess.EE_TreatmentInfo", "COOKED IN ITS OWN JUICES", treatmentProcess.EE_TreatmentInfo);
			AssertEquals("treatmentProcess.EE_StartDate", new ZDateTime(2019, 07, 30), treatmentProcess.EE_StartDate);
			AssertEquals("treatmentProcess.EE_EndDate", ZDateTime.Empty, treatmentProcess.EE_EndDate);
			AssertEquals("treatmentProcess.EE_EstablishmentPostedStatus", NEXDOCEstablishmentPostedStatus.Codes.Lodged, treatmentProcess.EE_EstablishmentPostedStatus);

			var productionProcess = quarantineLine1.Processes.Cast<QuarantineExDocEstablishmentAndTime>().First(p => p.EE_ProcessingType == "PC");
			AssertEquals("productionProcess.EE_AuthorisationEstablishmentID", "77", productionProcess.EE_AuthorisationEstablishmentID);
			AssertEquals("productionProcess.EE_E2_Address", aqisEstablishment.PK, productionProcess.Address.OrganisationPK);
			AssertEquals("productionProcess.EE_StartDate", ZDateTime.Empty, productionProcess.EE_StartDate);
			AssertEquals("productionProcess.EE_EndDate", ZDateTime.Empty, productionProcess.EE_EndDate);
			AssertEquals("productionProcess.EE_EstablishmentIndicator", "PC", productionProcess.EE_EstablishmentIndicator);
			AssertEquals("productionProcess.EE_EstablishmentPostedStatus", NEXDOCEstablishmentPostedStatus.Codes.Lodged, productionProcess.EE_EstablishmentPostedStatus);
		}

		public void TestProcessReadRexResponse_ProductLinesWithoutPermits()
		{
			var rexNumber = "REX001234";
			var message = CreateNEXDOCInterchangeAndMessageFromUniversalXML(embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("ReadREX_response_products.xml")));
			Factory.Save();

			var messageProcessor = new ReadRexResponseProcessorRC5(new LoggingInformation());
			messageProcessor.ProcessMessage(message);
			Factory.Save();

			message.Reload();
			AssertEquals("message processed (EM_Status)", EDIMessage.Status.Received, message.EM_Status);

			var otherFactory = new BusinessObjectFactory();
			var invoices = otherFactory.Load<JobComInvoiceHeader>(new ZQuery(JobComInvoiceHeaderSchema.JZ_InvoiceNumber, rexNumber));
			AssertEquals("commercial invoice created", 1, invoices.Length);

			var line = invoices[0].JobComInvoiceLines[0];
			AssertEquals("JI_LineNo", (ZShort)1, line.JI_LineNo);
			AssertEquals("line.AddInfo.ZA_ORG", "AU", line.AddInfo.ZA_ORG);
			AssertEquals("line.AddInfo.ZA_AQISTempContainerNumber_Hidden", "MAEU9304911", line.AddInfo.ZA_AQISTempContainerNumber_Hidden);

			var quarantineLine = line.QuarantineExDocLine;
			AssertEquals("QL_ProductType", "AMF", quarantineLine.QL_ProductType);
			AssertEquals("QL_Category", "DC0152", quarantineLine.QL_Category);
			AssertEquals("QL_PackType", "BG", quarantineLine.QL_PackType);
			AssertEquals("QL_PreservationType", "C", quarantineLine.QL_PreservationType);
			AssertEquals("QL_NetQuantity", 10.0m, quarantineLine.QL_NetQuantity);
			AssertEquals("QL_NetQuantityUnit", "KGM", quarantineLine.QL_NetQuantityUnit);
			AssertEquals("QL_OuterPackCount", 2, quarantineLine.QL_OuterPackCount);
			AssertEquals("QL_OuterPackType", "BG", quarantineLine.QL_OuterPackType);
			AssertEquals("QL_OuterPackWeight", 5.0m, quarantineLine.QL_OuterPackWeight);
			AssertEquals("QL_OuterPackWeightUnit", "KGM", quarantineLine.QL_OuterPackWeightUnit);
			AssertEquals("QL_OuterPackAccuracy", EXDOCPackAccuracyCodes.Codes.EqualTo, quarantineLine.QL_OuterPackAccuracy);
		}

		public void TestProcessReadRexResponse_RexNumberAsInvoiceNumber()
		{
			var rexNumber = "REX001234";
			var message = CreateNEXDOCInterchangeAndMessageFromUniversalXML(
$@"<?xml version=""1.0"" encoding=""UTF-8""?>
<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<Header xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
		<SenderID>NEXDOCSTest</SenderID>
		<RecipientID>HYEDAUCM2</RecipientID>
		<DeliveryMetadata>
			<ValueCollection>
				<Value>
					<Name>RexNumber</Name>
					<Type>String</Type>
					<Data>{rexNumber}</Data>
				</Value>
			</ValueCollection>
		</DeliveryMetadata>
	</Header>
	<Body xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
		<ns1:ReadRexResponse xmlns:ns1=""http://agriculture.gov.au/nexdoc/ReadRexSoap_1.0"" xmlns:ns2=""http://agriculture.gov.au/nexdoc/common/rex/CommonTypes_1.0"">
			 <ns1:rexResponseDetails />
		</ns1:ReadRexResponse>
	</Body>
</UniversalInterchange>");

			var messageProcessor = new ReadRexResponseProcessorRC5(new LoggingInformation());
			messageProcessor.ProcessMessage(message);
			Factory.Save();

			message.Reload();
			AssertEquals("message processed (EM_Status)", EDIMessage.Status.Received, message.EM_Status);

			var invoices = Factory.Load<JobComInvoiceHeader>(new ZQuery(JobComInvoiceHeaderSchema.JZ_InvoiceNumber, rexNumber));
			AssertEquals("commercial invoice created", 1, invoices.Length);
		}

		public void TestProcessReadRexResponse_Fish()
		{
			SetupEmailGroups();
			CreateSupplier("TSUP");
			CreateImporter("TIMP", "INTERNAT INST OF TROPIC AGRIC");
			CreateManufacturer("TMFR", "FITZROY KEBABS");
			CreateAQISEstablishment("TAQS", "AQIS SYDNEY");

			var message = CreateNEXDOCInterchangeAndMessageFromUniversalXML(embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("ReadREX_response_Fish.xml")));
			Factory.Save();

			var messageProcessor = new ReadRexResponseProcessorRC5(new LoggingInformation());
			messageProcessor.ProcessMessage(message);
			Factory.Save();

			message.Reload();
			AssertEquals("message processed (EM_Status)", EDIMessage.Status.Received, message.EM_Status);

			var otherFactory = new BusinessObjectFactory();
			var invoices = otherFactory.Load<JobComInvoiceHeader>(new ZQuery(JobComInvoiceHeaderSchema.JZ_InvoiceNumber, "INV000123"));
			AssertEquals("commercial invoice created", 1, invoices.Length);
			var invoice = invoices[0];
			AssertEquals(GlbBranch.CurrentBranch.PK, invoice.JZ_GB);

			new Customs.Business.FakeDeclarationCreatorForInvoice(invoice);
			var quarantineHeader = invoice.QuarantineExDocHeader;

			AssertEquals(1.00M, quarantineHeader.QH_MaximumTemperature);
			AssertEquals(-9.00M, quarantineHeader.QH_MinimumTemperature);
			AssertEquals("CEL", quarantineHeader.QH_TemperatureUM);

			var expectedCatchZones = new string[] { "Zone 1", "Zone 2", "Zone 3", "Zone 4" };
			AssertContainsExactElementsInAnyOrder("NexDocCatchZones", expectedCatchZones, quarantineHeader.NexDocCatchZones.Select(catchZone => catchZone.CY_Data));

			var invoiceLine1 = (JobComInvoiceLine)invoice.JobComInvoiceLines.First();
			var quarantineLine1 = invoiceLine1.QuarantineExDocLine;
			AssertEquals("QL_CatchStartDate", new ZDateTime(2019, 8, 1), quarantineLine1.QL_CatchStartDate);
			AssertEquals("QL_CatchEndDate", new ZDateTime(2019, 8, 5), quarantineLine1.QL_CatchEndDate);
			AssertEquals("QL_FishWaterIndicator", "S", quarantineLine1.QL_FishWaterIndicator);

			var harvestAreas = quarantineLine1.Processes.Where(p => p.EE_ProcessingType == EXDOCProcessTypeCodes.Codes.Harvest).ToArray();
			AssertEquals("HarvestAreas.Count", 2, harvestAreas.Length);
			var harvestArea1 = harvestAreas[0];
			AssertEquals("harvestArea1.EE_StartDate", new ZDateTime(2019, 8, 8), harvestArea1.EE_StartDate);
			AssertEquals("harvestArea1.EE_EndDate", new ZDateTime(2019, 8, 15), harvestArea1.EE_EndDate);
			AssertEquals("harvestArea1.EE_Depuration", new ZDateTime(2019, 8, 7), harvestArea1.EE_Depuration);
			AssertEquals("harvestArea1.EE_AuthorisationEstablishmentID", "80", harvestArea1.EE_AuthorisationEstablishmentID);
			AssertEquals("harvestArea1.EE_EstablishmentPostedStatus", NEXDOCEstablishmentPostedStatus.Codes.Lodged, harvestArea1.EE_EstablishmentPostedStatus);
			var harvestArea2 = harvestAreas[1];
			AssertEquals("harvestArea2.EE_StartDate", new ZDateTime(2019, 8, 8), harvestArea2.EE_StartDate);
			AssertEquals("harvestArea2.EE_HarvestArea", "BOOMER BAY", harvestArea2.EE_HarvestArea);
			AssertEquals("harvestArea2.EE_LeaseNumber", "10", harvestArea2.EE_LeaseNumber);
			AssertEquals("harvestArea2.EE_Depuration", new ZDateTime(2019, 8, 7), harvestArea2.EE_Depuration);
			AssertEquals("harvestArea2.EE_AuthorisationEstablishmentID", "80", harvestArea2.EE_AuthorisationEstablishmentID);
			AssertEquals("harvestArea2.EE_EstablishmentPostedStatus", NEXDOCEstablishmentPostedStatus.Codes.Lodged, harvestArea2.EE_EstablishmentPostedStatus);
		}

		public void TestProcessReadRexResponse_Wool()
		{
			SetupEmailGroups();
			CreateSupplier("TSUP");
			CreateImporter("TIMP", "INTERNAT INST OF TROPIC AGRIC");
			CreateManufacturer("TMFR", "FITZROY KEBABS");
			CreateAQISEstablishment("TAQS", "AQIS SYDNEY");

			var message = CreateNEXDOCInterchangeAndMessageFromUniversalXML(embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("ReadREX_response_Wool.xml")));
			Factory.Save();

			var messageProcessor = new ReadRexResponseProcessorRC5(new LoggingInformation());
			messageProcessor.ProcessMessage(message);
			Factory.Save();

			message.Reload();
			AssertEquals("message processed (EM_Status)", EDIMessage.Status.Received, message.EM_Status);

			var otherFactory = new BusinessObjectFactory();
			var invoices = otherFactory.Load<JobComInvoiceHeader>(new ZQuery(JobComInvoiceHeaderSchema.JZ_InvoiceNumber, "INV000123"));
			AssertEquals("commercial invoice created", 1, invoices.Length);
			var invoice = invoices[0];
			AssertEquals(GlbBranch.CurrentBranch.PK, invoice.JZ_GB);

			new Customs.Business.FakeDeclarationCreatorForInvoice(invoice);
			var quarantineHeader = invoice.QuarantineExDocHeader;

			AssertEquals(new ZDate(2024, 1, 2), quarantineHeader.QH_PackDate);
		}

		public void TestProcessReadRexResponse_SkinsAndHides()
		{
			SetupEmailGroups();
			CreateSupplier("TSUP");
			CreateImporter("TIMP", "INTERNAT INST OF TROPIC AGRIC");
			CreateManufacturer("TMFR", "FITZROY KEBABS");
			CreateAQISEstablishment("TAQS", "AQIS SYDNEY");

			var message = CreateNEXDOCInterchangeAndMessageFromUniversalXML(embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("ReadREX_response_SkinsAndHides.xml")));
			Factory.Save();

			var messageProcessor = new ReadRexResponseProcessorRC5(new LoggingInformation());
			messageProcessor.ProcessMessage(message);
			Factory.Save();

			message.Reload();
			AssertEquals("message processed (EM_Status)", EDIMessage.Status.Received, message.EM_Status);

			var otherFactory = new BusinessObjectFactory();
			var invoices = otherFactory.Load<JobComInvoiceHeader>(new ZQuery(JobComInvoiceHeaderSchema.JZ_InvoiceNumber, "INV000123"));
			AssertEquals("commercial invoice created", 1, invoices.Length);
			var invoice = invoices[0];
			AssertEquals(GlbBranch.CurrentBranch.PK, invoice.JZ_GB);

			new Customs.Business.FakeDeclarationCreatorForInvoice(invoice);
			var quarantineHeader = invoice.QuarantineExDocHeader;

			AssertEquals("QH_PackDate", new ZDate(2024, 1, 2), quarantineHeader.QH_PackDate);
			AssertEquals("LoadingEstablishmentLocation", aqisEstablishment.PK, invoice.AQISLoadingEstablishmentLocation.OrganisationPK);
			AssertEquals("QH_LoadingDate", new ZDate(2024, 3, 4), quarantineHeader.QH_LoadingDate);

			var invoiceLine1 = (JobComInvoiceLine)invoice.JobComInvoiceLines.First();
			var quarantineLine1 = invoiceLine1.QuarantineExDocLine;
			AssertEquals("QL_SaltingDate", new ZDate(2024, 5, 6), quarantineLine1.QL_SaltingDate);
		}

		[ExpectNoExceptions]
		public void TestProcessReadRexResponse_Fish_LackNodes()
		{
			SetupEmailGroups();
			CreateSupplier("TSUP");
			CreateImporter("TIMP", "INTERNAT INST OF TROPIC AGRIC");
			CreateManufacturer("TMFR", "FITZROY KEBABS");
			CreateAQISEstablishment("TAQS", "AQIS SYDNEY");

			var message = CreateNEXDOCInterchangeAndMessageFromUniversalXML(embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("ReadREX_response_Fish_LackNodes.xml")));
			Factory.Save();

			var messageProcessor = new ReadRexResponseProcessorRC5(new LoggingInformation());
			messageProcessor.ProcessMessage(message);
			Factory.Save();

			var invoices = Factory.Load<JobComInvoiceHeader>(new ZQuery(JobComInvoiceHeaderSchema.JZ_InvoiceNumber, "INV000123"));
			AssertEquals("commercial invoice created", 1, invoices.Length);
			var invoice = invoices[0];
			var invoiceLine1 = (JobComInvoiceLine)invoice.JobComInvoiceLines.First();
			var quarantineLine1 = invoiceLine1.QuarantineExDocLine;
			Assert("QL_CatchStartDate", quarantineLine1.QL_CatchStartDate.IsEmpty);
			Assert("QL_CatchEndDate", quarantineLine1.QL_CatchEndDate.IsEmpty);
			Assert("QL_FishWaterIndicator", quarantineLine1.QL_FishWaterIndicator.IsEmpty);
		}

		public void TestUpdateValuesOnExistingInvoice()
		{
			SetupEmailGroups();

			var rexNumber = "REX001234";
			var message = CreateNEXDOCInterchangeAndMessageFromUniversalXML(
$@"<?xml version=""1.0"" encoding=""UTF-8""?>
<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<Header xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
		<SenderID>NEXDOCSTest</SenderID> 
		<RecipientID>HYEDAUCM2</RecipientID>
		<DeliveryMetadata>
			<ValueCollection>
				<Value>
					<Name>RexNumber</Name>
					<Type>String</Type>
					<Data>{rexNumber}</Data>
				</Value>
			</ValueCollection>
		</DeliveryMetadata>
	</Header>
	<Body xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
		<ns1:ReadRexResponse xmlns:ns1=""http://agriculture.gov.au/nexdoc/ReadRexSoap_1.0"" xmlns:ns2=""http://agriculture.gov.au/nexdoc/common/rex/CommonTypes_1.0"">
			 <ns1:rexResponseDetails>
				<ns2:permitNumber>1234567890123456789012345678901234567890</ns2:permitNumber>
				<ns2:complianceStatus>CTRD</ns2:complianceStatus>
				<ns2:lastAmendDateTime>2019-09-12T09:17:45.001+10:00</ns2:lastAmendDateTime>
			 </ns1:rexResponseDetails>
			 <ns1:exportDetails>
				 <ns2:sew>
					 <ns2:edn>ABCDEFGHIJKLMNOPQRSTUVWXYZABCDEFGHIJKLMNOPQRSTUVWXYZ</ns2:edn>
				 </ns2:sew>
			 </ns1:exportDetails>
		</ns1:ReadRexResponse>
	</Body>
</UniversalInterchange>");

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Quarantine;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = rexNumber;

			var exdocHeader = invoice.QuarantineExDocHeader;
			exdocHeader.QH_RequestForPermitNumber = rexNumber;
			exdocHeader.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.OrdrOrder;

			Factory.Save();

			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(Core.Constants.CountryCodes.NewZealand)) // GMT +12
			{
				var messageProcessor = new ReadRexResponseProcessorRC5(new LoggingInformation());
				messageProcessor.ProcessMessage(message);
				Factory.Save();
			}

			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(Core.Constants.CountryCodes.Australia)) // GMT +10
			{
				CombineAssertions(() =>
				{
					var docHeader = invoice.QuarantineExDocHeader;

					AssertContainsExactElementsInAnyOrder("Messages", new[] { message.PK }, docHeader.Messages.GetPKs());

					AssertEquals("ExportPermitNumber", "12345678901234567890123456789012345", docHeader.QH_ExportPermitNumber);
					AssertEquals("LastAmendDateTime", "2019-09-11T23:17:45.0010000+00:00", docHeader.QH_LastAmendDateTime.ToISO8601String());
					AssertEquals("EntryStatus", "CTR", docHeader.RequestForPermitStatus);

					AssertEquals("EDN", "ABCDEFGHIJKLMNOPQRSTUVWXYZABCDEFGHI", invoice.AddInfo.ZA_EDN_Hidden);
					AssertEquals("DeclarationNumber", "ABCDEFGHIJKLMNOPQRSTUVWXYZABCDEFGHI", declaration.DeclarationNumber);
				});
			}
		}

		public void TestProcessingError_UnrecognisedRexNumber()
		{
			SetupEmailGroups();

			var message = CreateNEXDOCInterchangeAndMessageFromUniversalXML(
@"<?xml version=""1.0"" encoding=""UTF-8""?>
<n1:UniversalInterchange xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""
xmlns:n1=""http://www.cargowise.com/Schemas/Universal/2011/11"" 
xsi:schemaLocation=""http://www.cargowise.com/Schemas/Universal/2011/11 UniversalInterchange.xsd"">
	<n1:Header>
		<n1:SenderID>NEXDOCSTest</n1:SenderID>
		<n1:RecipientID>HYEDAUCM2</n1:RecipientID>
		<n1:DeliveryMetadata>
			<n1:ValueCollection>
				<n1:Value>
					<n1:Name>NotARexNumber</n1:Name>
					<n1:Type>String</n1:Type>
					<n1:Data>1234</n1:Data>
				</n1:Value>
			</n1:ValueCollection>
		</n1:DeliveryMetadata>
	</n1:Header>
	<n1:Body>
		<ns1:ReadRexResponse xmlns:ns1=""http://agriculture.gov.au/nexdoc/ReadRexSoap_1.0"" xmlns:ns2=""http://agriculture.gov.au/nexdoc/common/rex/CommonTypes_1.0"">
			 <ns1:rexResponseDetails>
				<ns2:complianceStatus>CTRD</ns2:complianceStatus>
				<ns2:lastAmendDateTime>2019-09-12T16:17:45.001+10:00</ns2:lastAmendDateTime>
			 </ns1:rexResponseDetails>
		</ns1:ReadRexResponse>
	</n1:Body>
</n1:UniversalInterchange>");

			Factory.Save();

			var messageProcessor = new ReadRexResponseProcessorRC5(new LoggingInformation());
			messageProcessor.ProcessMessage(message);
			Factory.Save();

			message.Reload();
			AssertEquals("message.EM_Status", EDIMessage.Status.Error, message.EM_Status);

			var responseEmail = Env.OutgoingMailManager.EmailsCreated.Last();
			AssertEquals("Response Email Subject", "ERROR PROCESSING", responseEmail.Subject);
			AssertContains("Response Email Body contains Status", "FATAL PROCESSING ERROR: Invalid or Missing Rex Number. Cannot Process.", responseEmail.Body);
		}

		public void TestProcessingError_UnexpectedResponseBody()
		{
			SetupEmailGroups();

			var message = CreateNEXDOCInterchangeAndMessageFromUniversalXML(
@"<?xml version=""1.0"" encoding=""UTF-8""?>
<n1:UniversalInterchange xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""
xmlns:n1=""http://www.cargowise.com/Schemas/Universal/2011/11"" 
xsi:schemaLocation=""http://www.cargowise.com/Schemas/Universal/2011/11 UniversalInterchange.xsd"">
	<n1:Header>
		<n1:SenderID>NEXDOCSTest</n1:SenderID>
		<n1:RecipientID>HYEDAUCM2</n1:RecipientID>
		<n1:DeliveryMetadata>
			<n1:ValueCollection>
				<n1:Value>
					<n1:Name>RexNumber</n1:Name>
					<n1:Type>String</n1:Type>
					<n1:Data>REX0000028829</n1:Data>
				</n1:Value>
			</n1:ValueCollection>
		</n1:DeliveryMetadata>
	</n1:Header>
	<n1:Body>
		<ns1:SomeOtherResponse xmlns:ns1=""http://agriculture.gov.au/nexdoc/RexOwnershipSoap_1.0"" xmlns:ns0=""http://agriculture.gov.au/nexdoc/common/CommonTypes_1.0"">
			<ns1:outcome>CLOSED_ACCEPTED</ns1:outcome>
		</ns1:SomeOtherResponse>
	</n1:Body>
</n1:UniversalInterchange>");

			Factory.Save();

			var messageProcessor = new ReadRexResponseProcessorRC5(new LoggingInformation());
			messageProcessor.ProcessMessage(message);
			Factory.Save();

			message.Reload();
			AssertEquals("message.EM_Status", EDIMessage.Status.Error, message.EM_Status);

			var responseEmail = Env.OutgoingMailManager.EmailsCreated.Last();
			AssertEquals("Response Email Subject", "ERROR PROCESSING", responseEmail.Subject);
			AssertContains("Response Email Body contains Status", "FATAL PROCESSING ERROR: Invalid ReadRexResponse. Cannot Process.", responseEmail.Body);
		}

		void AssertLine1(JobComInvoiceLine invoiceLine1)
		{
			AssertEquals((ZShort)1, invoiceLine1.JI_LineNo);
			AssertEquals("VIC", invoiceLine1.JI_AUState);
			AssertEquals(4000m, invoiceLine1.JI_LinePrice);
			AssertEquals("1", invoiceLine1.JI_TempImportNum);
			AssertEquals(new ZDateTime(2019, 07, 08), invoiceLine1.JI_TempImportDate);
			AssertEquals("0406.10.00", invoiceLine1.JI_Tariff);

			var quarantineLine1 = invoiceLine1.QuarantineExDocLine;
			AssertEquals("AMF", quarantineLine1.QL_ProductType);
			AssertEquals("BG", quarantineLine1.QL_PackType);
			AssertEquals("C", quarantineLine1.QL_PreservationType);
			AssertEquals("DC0152", quarantineLine1.QL_Category);
			AssertEquals("", quarantineLine1.QL_CutCode);
			AssertEquals("", quarantineLine1.QL_SupplimentaryCode);

			AssertEquals("ZD035", quarantineLine1.QL_HCFormatAllocated);
			AssertEquals("356", quarantineLine1.QL_HCFormatRequested);

			AssertEquals(ZDateTime.Empty, quarantineLine1.QL_UseByStart);
			AssertEquals(ZDateTime.Empty, quarantineLine1.QL_UseByEnd);
			AssertEquals("", quarantineLine1.QL_BatchCode);

			AssertEquals(10.0M, quarantineLine1.QL_NetQuantity);
			AssertEquals("KGM", quarantineLine1.QL_NetQuantityUnit);
			AssertEquals(0M, quarantineLine1.QL_ImperialNetWeight);
			AssertEquals("", quarantineLine1.QL_ImperialNetWeightUnit);
			AssertEquals(0M, quarantineLine1.QL_AqisCustomsWeight);
			AssertEquals("", quarantineLine1.QL_AqisCustomsWeightUQ);
			AssertEquals(100.0M, quarantineLine1.QL_GrossMetricWeight);
			AssertEquals("KGM", quarantineLine1.QL_GrossMetricWeightUnit);
			AssertEquals(100.0M, invoiceLine1.JI_Weight);
			AssertEquals("KG", invoiceLine1.JI_WeightUQ);

			// outerProductPackaging
			AssertEquals(2, quarantineLine1.QL_OuterPackCount);
			AssertEquals("BG", quarantineLine1.QL_OuterPackType);
			AssertEquals(5.0M, quarantineLine1.QL_OuterPackWeight);
			AssertEquals("KGM", quarantineLine1.QL_OuterPackWeightUnit);
			AssertEquals("4", quarantineLine1.QL_OuterPackAccuracy);
			AssertEquals("1-10", quarantineLine1.QL_ShippingMarks);
			// intermediateProductPackaging
			AssertEquals(0, quarantineLine1.QL_IntermediatePackCount);
			AssertEquals("", quarantineLine1.QL_IntermediatePackType);
			AssertEquals(0M, quarantineLine1.QL_IntermediatePackWeight);
			AssertEquals("", quarantineLine1.QL_IntermediatePackWeightUnit);
			AssertEquals("", quarantineLine1.QL_IntermediatePackAccuracy);
			// innerProductPackaging
			AssertEquals(0, quarantineLine1.QL_InnerPackCount);
			AssertEquals("", quarantineLine1.QL_InnerPackType);
			AssertEquals(0M, quarantineLine1.QL_InnerPackWeight);
			AssertEquals("", quarantineLine1.QL_InnerPackWeightUnit);
			AssertEquals("", quarantineLine1.QL_InnerPackAccuracy);

			AssertEquals(2, quarantineLine1.Processes.Count);
			var treatmentProcess = quarantineLine1.Processes.Cast<QuarantineExDocEstablishmentAndTime>().First(p => p.EE_TreatmentCode == "HEAT");
			AssertEquals("COOKED IN ITS OWN JUICES", treatmentProcess.EE_TreatmentInfo);
			AssertEquals(new ZDateTime(2019, 07, 30), treatmentProcess.EE_StartDate);
			AssertEquals(new ZDateTime(2019, 07, 31), treatmentProcess.EE_EndDate);
			AssertEquals("EE_EstablishmentPostedStatus", NEXDOCEstablishmentPostedStatus.Codes.Lodged, treatmentProcess.EE_EstablishmentPostedStatus);

			var productionProcess = quarantineLine1.Processes.Cast<QuarantineExDocEstablishmentAndTime>().First(p => p.EE_ProcessingType == "PC");
			AssertEquals("EE_AuthorisationEstablishmentID", "77", productionProcess.EE_AuthorisationEstablishmentID);
			AssertEquals("EE_E2_Address", aqisEstablishment.PK, productionProcess.Address.OrganisationPK);
			AssertEquals("EE_EstablishmentIndicator", "PC", productionProcess.EE_EstablishmentIndicator);
			AssertEquals("EE_StartDate", new ZDateTime(2019, 08, 30), productionProcess.EE_StartDate);
			AssertEquals("EE_EndDate", new ZDateTime(2019, 08, 31), productionProcess.EE_EndDate);
			AssertEquals("EE_EstablishmentPostedStatus", NEXDOCEstablishmentPostedStatus.Codes.Lodged, productionProcess.EE_EstablishmentPostedStatus);

			AssertEquals("3065", invoiceLine1.ManufacturerAddress.OA_PostCode);
			AssertEquals("TMFR", invoiceLine1.ManufacturerAddress.Header.OH_Code);
			AssertEquals("AU", invoiceLine1.JI_CountryOfOrigin);

			AssertEquals(1, invoiceLine1.ContainersPivot.Count);
			AssertEquals("MAEU9304911", invoiceLine1.ContainersPivot[0].ContainerNumber);
			AssertEquals("SL12345", invoiceLine1.ContainersPivot[0].Container.SealNumberForBinding);
		}

		void AssertLine2(JobComInvoiceLine invoiceLine2)
		{
			AssertEquals((ZShort)2, invoiceLine2.JI_LineNo);
			AssertEquals("VIC", invoiceLine2.JI_AUState);
			AssertEquals(1000m, invoiceLine2.JI_LinePrice);
			AssertEquals("2", invoiceLine2.JI_TempImportNum);
			AssertEquals(new ZDateTime(2019, 07, 07), invoiceLine2.JI_TempImportDate);
			AssertEquals("0406.10.00", invoiceLine2.JI_Tariff);

			var quarantineLine2 = invoiceLine2.QuarantineExDocLine;
			AssertEquals("AMF", quarantineLine2.QL_ProductType);
			AssertEquals("BG", quarantineLine2.QL_PackType);
			AssertEquals("C", quarantineLine2.QL_PreservationType);
			AssertEquals("DC0152", quarantineLine2.QL_Category);
			AssertEquals("", quarantineLine2.QL_CutCode);
			AssertEquals("", quarantineLine2.QL_SupplimentaryCode);

			AssertEquals(ZDateTime.Empty, quarantineLine2.QL_UseByStart);
			AssertEquals(ZDateTime.Empty, quarantineLine2.QL_UseByEnd);
			AssertEquals("", quarantineLine2.QL_BatchCode);

			AssertEquals(100.0M, quarantineLine2.QL_NetQuantity);
			AssertEquals("KGM", quarantineLine2.QL_NetQuantityUnit);
			AssertEquals(0M, quarantineLine2.QL_ImperialNetWeight);
			AssertEquals("", quarantineLine2.QL_ImperialNetWeightUnit);
			AssertEquals(0M, quarantineLine2.QL_AqisCustomsWeight);
			AssertEquals("", quarantineLine2.QL_AqisCustomsWeightUQ);
			AssertEquals(100.0M, quarantineLine2.QL_GrossMetricWeight);
			AssertEquals("KGM", quarantineLine2.QL_GrossMetricWeightUnit);
			AssertEquals(100.0M, invoiceLine2.JI_Weight);
			AssertEquals("KG", invoiceLine2.JI_WeightUQ);

			// outerProductPackaging
			AssertEquals(5, quarantineLine2.QL_OuterPackCount);
			AssertEquals("BG", quarantineLine2.QL_OuterPackType);
			AssertEquals(20.0M, quarantineLine2.QL_OuterPackWeight);
			AssertEquals("KGM", quarantineLine2.QL_OuterPackWeightUnit);
			AssertEquals("4", quarantineLine2.QL_OuterPackAccuracy);
			AssertEquals("", quarantineLine2.QL_ShippingMarks);
			// intermediateProductPackaging
			AssertEquals(0, quarantineLine2.QL_IntermediatePackCount);
			AssertEquals("", quarantineLine2.QL_IntermediatePackType);
			AssertEquals(0M, quarantineLine2.QL_IntermediatePackWeight);
			AssertEquals("", quarantineLine2.QL_IntermediatePackWeightUnit);
			AssertEquals("", quarantineLine2.QL_IntermediatePackAccuracy);
			// innerProductPackaging
			AssertEquals(0, quarantineLine2.QL_InnerPackCount);
			AssertEquals("", quarantineLine2.QL_InnerPackType);
			AssertEquals(0M, quarantineLine2.QL_InnerPackWeight);
			AssertEquals("", quarantineLine2.QL_InnerPackWeightUnit);
			AssertEquals("", quarantineLine2.QL_InnerPackAccuracy);

			AssertEquals(1, quarantineLine2.Processes.Count);
			var productionProcess = quarantineLine2.Processes.Cast<QuarantineExDocEstablishmentAndTime>().First(p => p.EE_ProcessingType == "PC");
			AssertEquals("EE_AuthorisationEstablishmentID", "77", productionProcess.EE_AuthorisationEstablishmentID);
			AssertEquals("EE_E2_Address", aqisEstablishment.PK, productionProcess.Address.OrganisationPK);
			AssertEquals("EE_EstablishmentIndicator", "PC", productionProcess.EE_EstablishmentIndicator);
			AssertEquals("EE_StartDate", new ZDateTime(2019, 06, 26), productionProcess.EE_StartDate);
			AssertEquals("EE_EndDate", new ZDateTime(2019, 06, 27), productionProcess.EE_EndDate);
			AssertEquals("EE_EstablishmentPostedStatus", NEXDOCEstablishmentPostedStatus.Codes.Lodged, productionProcess.EE_EstablishmentPostedStatus);

			AssertEquals(ZGuid.Empty, invoiceLine2.JI_OA_ManufacturerAddress);
			AssertEquals("AU", invoiceLine2.JI_CountryOfOrigin);

			AssertEquals(1, invoiceLine2.ContainersPivot.Count);
			AssertEquals("MAEU9304911", invoiceLine2.ContainersPivot[0].ContainerNumber);
		}

		void AssertLine3(JobComInvoiceLine invoiceLine3)
		{
			AssertEquals((ZShort)3, invoiceLine3.JI_LineNo);
			AssertEquals("VIC", invoiceLine3.JI_AUState);
			AssertEquals(800m, invoiceLine3.JI_LinePrice);
			AssertEquals("", invoiceLine3.JI_TempImportNum);
			AssertEquals(ZDateTime.Empty, invoiceLine3.JI_TempImportDate);
			AssertEquals("0406.10.00", invoiceLine3.JI_Tariff);

			var quarantineLine3 = invoiceLine3.QuarantineExDocLine;
			AssertEquals("AMF", quarantineLine3.QL_ProductType);
			AssertEquals("DC0152", quarantineLine3.QL_Category);
			AssertEquals("BG", quarantineLine3.QL_PackType);
			AssertEquals("C", quarantineLine3.QL_PreservationType);
			AssertEquals("IN4032", quarantineLine3.QL_CutCode);
			AssertEquals("A1", quarantineLine3.QL_SupplimentaryCode);

			AssertEquals("ZD035", quarantineLine3.QL_HCFormatAllocated);
			AssertEquals("356", quarantineLine3.QL_HCFormatRequested);

			AssertEquals(new ZDateTime(2019, 06, 27), quarantineLine3.QL_UseByStart);
			AssertEquals(new ZDateTime(2019, 06, 28), quarantineLine3.QL_UseByEnd);
			AssertEquals("23423", quarantineLine3.QL_BatchCode);

			AssertEquals(100.0M, quarantineLine3.QL_NetQuantity);
			AssertEquals("KGM", quarantineLine3.QL_NetQuantityUnit);
			AssertEquals(80M, quarantineLine3.QL_ImperialNetWeight);
			AssertEquals("KGM", quarantineLine3.QL_ImperialNetWeightUnit);
			AssertEquals(100M, quarantineLine3.QL_AqisCustomsWeight);
			AssertEquals("KGM", quarantineLine3.QL_AqisCustomsWeightUQ);
			AssertEquals(200.0M, quarantineLine3.QL_GrossMetricWeight);
			AssertEquals("KGM", quarantineLine3.QL_GrossMetricWeightUnit);
			AssertEquals(200.0M, invoiceLine3.JI_Weight);
			AssertEquals("KG", invoiceLine3.JI_WeightUQ);

			// outerProductPackaging
			AssertEquals(4, quarantineLine3.QL_OuterPackCount);
			AssertEquals("BG", quarantineLine3.QL_OuterPackType);
			AssertEquals(20.0M, quarantineLine3.QL_OuterPackWeight);
			AssertEquals("KGM", quarantineLine3.QL_OuterPackWeightUnit);
			AssertEquals("4", quarantineLine3.QL_OuterPackAccuracy);
			AssertEquals("MARKS", quarantineLine3.QL_ShippingMarks);
			// intermediateProductPackaging
			AssertEquals(3, quarantineLine3.QL_IntermediatePackCount);
			AssertEquals("BG", quarantineLine3.QL_IntermediatePackType);
			AssertEquals(30M, quarantineLine3.QL_IntermediatePackWeight);
			AssertEquals("KGM", quarantineLine3.QL_IntermediatePackWeightUnit);
			AssertEquals("4", quarantineLine3.QL_IntermediatePackAccuracy);
			// innerProductPackaging
			AssertEquals(3, quarantineLine3.QL_InnerPackCount);
			AssertEquals("BG", quarantineLine3.QL_InnerPackType);
			AssertEquals(30M, quarantineLine3.QL_InnerPackWeight);
			AssertEquals("KGM", quarantineLine3.QL_InnerPackWeightUnit);
			AssertEquals("3", quarantineLine3.QL_InnerPackAccuracy);

			AssertEquals(1, quarantineLine3.Processes.Count);
			var productionProcess = quarantineLine3.Processes.Cast<QuarantineExDocEstablishmentAndTime>().First(p => p.EE_ProcessingType == "PC");
			AssertEquals("EE_AuthorisationEstablishmentID", "", productionProcess.EE_AuthorisationEstablishmentID);
			AssertEquals("EE_E2_Address", ZGuid.Empty, productionProcess.EE_E2_Address);
			AssertEquals("EE_EstablishmentIndicator", "PC", productionProcess.EE_EstablishmentIndicator);
			AssertEquals("EE_StartDate", new ZDateTime(2019, 06, 26), productionProcess.EE_StartDate);
			AssertEquals("EE_EndDate", new ZDateTime(2019, 06, 27), productionProcess.EE_EndDate);
			AssertEquals("EE_EstablishmentPostedStatus", NEXDOCEstablishmentPostedStatus.Codes.Lodged, productionProcess.EE_EstablishmentPostedStatus);

			AssertEquals("3065", invoiceLine3.ManufacturerAddress.OA_PostCode);
			AssertEquals("TMFR", invoiceLine3.ManufacturerAddress.Header.OH_Code);
			AssertEquals("NZ", invoiceLine3.JI_CountryOfOrigin);

			AssertEquals(1, invoiceLine3.ContainersPivot.Count);
			AssertEquals("MAEU9302930", invoiceLine3.ContainersPivot[0].ContainerNumber);
		}

		void CreateSupplier(ZString code)
		{
			var refCountryCodeAU = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.Australia);

			var supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = code;
			supplier.SetCustomsCode(OrgCusCode.AUQuarantineCodeTypes.NEXDOCSExportNumber, refCountryCodeAU, "AA0220");
		}

		void CreateImporter(ZString code, ZString fullName)
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = code;
			importer.OH_FullName = fullName;
			var mainAddress = importer.MainAddress;
			mainAddress.Address1 = "IKEJA VIA APAP QUAYS, LAGOS";
			mainAddress.Address2 = "NIGERIA";
			mainAddress.City = "LAGOS";
			mainAddress.State = "AN";
			mainAddress.OA_RN_NKCountryCode = "NG";
			mainAddress.Postcode = "100214";
		}

		void CreateManufacturer(ZString code, ZString fullName)
		{
			var manufacturer = Factory.New<OrgHeader>();
			manufacturer.OH_Code = code;
			manufacturer.OH_FullName = fullName;
			var mainAddress = manufacturer.MainAddress;
			mainAddress.Address1 = "89 SMITH STREET";
			mainAddress.City = "FITZROY";
			mainAddress.State = "VIC";
			mainAddress.OA_RN_NKCountryCode = "AU";
			mainAddress.Postcode = "3065";
		}

		void CreateAQISEstablishment(ZString code, ZString fullName)
		{
			aqisEstablishment = Factory.NewWithValidTestData<OrgHeader>();
			aqisEstablishment.OH_Code = code;
			aqisEstablishment.OH_FullName = fullName;
			var mainAddress = aqisEstablishment.MainAddress;
			mainAddress.Address1 = "185 O'RIORDAN ST";
			mainAddress.City = "MASCOT";
			mainAddress.State = "NSW";
			mainAddress.OA_RN_NKCountryCode = "AU";
			mainAddress.Postcode = "2020";
			var esnCusCode = mainAddress.CustomsCodes.AddNew();
			esnCusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Australia;
			esnCusCode.OK_CodeType = OrgCusCode.AUQuarantineCodeTypes.EXDOCEstablishmentNumber;
			esnCusCode.OK_CustomsRegNo = "77";
		}

		OrgHeader aqisEstablishment;
	}
}
