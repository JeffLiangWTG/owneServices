using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.CH;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(BordereauResponseMessageProcessor))]
sealed class BordereauResponseMessageProcessorTest : TestCaseWithFactory
{
	BordereauResponseMessageProcessor GetMessageProcessor(LoggingInformation logger) => new BordereauResponseMessageProcessor(Logger);

	LoggingInformationForTesting Logger => logger ??= new LoggingInformationForTesting();
	LoggingInformationForTesting logger;

	public void TestMessageFriendlyName()
	{
		var processor = GetMessageProcessor(Logger);
		AssertEquals("Customs Bordereau Message Response Processor", processor.MessageFriendlyName);
	}

	public void TestMessageFilter()
	{
		var ediMessage = Factory.New<EDIMessage>();
		var processor = GetMessageProcessor(Logger);

		AssertCanProcess(true, MessageTypeCodeList.Codes.BOR, MessageSubTypeCodeList.Codes.BordereauResponse);
		AssertCanProcess(false, MessageTypeCodeList.Codes.BOR, string.Empty);
		AssertCanProcess(false, string.Empty, MessageSubTypeCodeList.Codes.BordereauResponse);
		AssertCanProcess(false, MessageTypeCodeList.Codes.BOR, MessageSubTypeCodeList.Codes.BordereauResponse, applicationCode: string.Empty);

		void AssertCanProcess(bool expectedCanProcess, string messageType, string messageSubType, string applicationCode = ApplicationCodeList.Codes.CHCustomsEdec)
		{
			ediMessage.EM_ApplicationCode = applicationCode;
			ediMessage.EM_MessageType = messageType;
			ediMessage.EM_MessageSubType = messageSubType;
			AssertEquals($"ApplicationCode={applicationCode} MessageType={messageSubType} MessageSubType={messageSubType}", expectedCanProcess, processor.CanProcess(ediMessage));
		}
	}

	public void TestMessageProcessing() => CombineAssertions(() =>
	{
		var entryHeader1 = CreateEntryHeader("MRN001");
		var entryHeader2 = CreateEntryHeader("MRN002.1");
		var entryHeader3 = CreateEntryHeader("MRN003.2");
		var entryHeader4 = CreateEntryHeader("MRN004");
		var shipment = CreateShipment("MRN005");

		var documentNumber = "11";
		var documentDate = new ZDate(2010, 6, 16);
		var customsOfficeNumber = "CH001251";

		var messageText = TestingData.EdecBordereauTypeBordereauXml(documentNumber, documentDate, customsOfficeNumber);

		(string documentType, string mrn, string amount, string traderReference)[] customsOfficeDetails =
		[
			("VVZ", "MRN001", "-8.30", "TR12345"),
			("VVM", "MRN002", "8.30", "TR12346"),
			("RBZ", "MRN003", "-2.70", "TR12347"),
			("RBM", "MRN004", "-5.00", string.Empty),
			("VVZ", "MRN005", "4.22", "TR6789"),
			("VVZ", "MRN106", "-3.30", "TR67890"),
			("VVZ", "MRN107", "1.15", string.Empty),
			("VVZ", "MRN108", "4.64", "TR67891")
		];

		RemoveDetailsFromMessage(ref messageText);
		foreach (var detail in customsOfficeDetails)
		{
			AddDetailToMessage(ref messageText, detail.documentType, detail.mrn, detail.amount, detail.traderReference);
		}

		var (company, ediMessage) = MessageProcessorTestHelper.CreateCompanyMessagesAndInterchanges(Factory, ApplicationCodeList.Codes.CHCustomsEdec, MessageTypeCodeList.Codes.BOR, MessageSubTypeCodeList.Codes.BordereauResponse, messageText, companyCode: GlbCompany.CurrentCompany.GC_Code, certificateCredential: true, customsRegNo: CustomsRegNo);
		var credentials = GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(company).GlbExternalPassword;
		Factory.Save();

		TestDateAttribute.AddMinutes(1);
		var start = ZDateTime.UtcNow;
		MessageProcessorTestHelper.ProcessMessage(GetMessageProcessor(Logger), ediMessage);
		Factory.Save();

		var ediMessages = LoadEDIMessages(start);
		var ediMessagesCount = ediMessages.Length;
		AssertRequest(MessageSubTypeCodeList.Codes.TaxationDecisionCustomsDuties, entryHeader1, "MRN001", EvvDocumentType.Codes.TaxationDecisionCustomsDuties);
		AssertRequest(MessageSubTypeCodeList.Codes.TaxationDecisionVat, entryHeader2, "MRN002", EvvDocumentType.Codes.TaxationDecisionVAT);
		AssertRequest(MessageSubTypeCodeList.Codes.TaxationDecisionReimbursementCustomsDuties, entryHeader3, "MRN003", EvvDocumentType.Codes.RefundCustomsDuties);
		AssertRequest(MessageSubTypeCodeList.Codes.TaxationDecisionReimbursementVat, entryHeader4, "MRN004", EvvDocumentType.Codes.RefundVAT);
		AssertRequest(MessageSubTypeCodeList.Codes.TaxationDecisionCustomsDuties, shipment, "MRN005", EvvDocumentType.Codes.TaxationDecisionCustomsDuties);
		AssertEquals("Unexpected EDIMessages", 0, ediMessagesCount);

		var query = new ZDBOnlyQuery(typeof(CustomsSummaryHeader));
		query.AddToFilter(CusStatementHeaderSchema.B2_StatementNumber, documentNumber);
		query.AddToFilter(CusStatementHeaderSchema.B2_ProcessDate, documentDate);
		var summaryHeader = Factory.LoadTop1<CustomsSummaryHeader>(query);
		AssertNotNull("Summary header", summaryHeader);
		AssertEquals("Summary header's account number", "12345", summaryHeader.B2_AccountNo);
		AssertEquals("Summary header's number of summary lines", 8, summaryHeader.SummaryLines.Count);

		string[] existingEntryNumbers = ["MRN001", "MRN002", "MRN003", "MRN004", "MRN005"];

		foreach (var detail in customsOfficeDetails)
		{
			var summaryLine = summaryHeader.SummaryLines.FirstOrDefault(y => y.B3_EntryNum == $"{detail.mrn}.{MRNVersion}");
			AssertNotNull($"Summary line for Entry number {detail.mrn}.{MRNVersion}", summaryLine);
			AssertEquals($"Summary line Entry number {detail.mrn}.{MRNVersion} - B3_BrokerReference", detail.traderReference, summaryLine.B3_BrokerReference);
			AssertEquals($"Summary line Entry number {detail.mrn}.{MRNVersion} - B3_Status", existingEntryNumbers.Contains(detail.mrn) ? "SNT" : "SKP", summaryLine.B3_Status);
			AssertNotNull($"Summary line Entry number {detail.mrn}.{MRNVersion} - LineCharge", summaryLine.LineCharge);
			AssertEquals($"Summary line Entry number {detail.mrn}.{MRNVersion} - ChargeType", detail.documentType, summaryLine.ChargeType);
			AssertEquals($"Summary line Entry number {detail.mrn}.{MRNVersion} - ReferenceNumber", customsOfficeNumber, summaryLine.ReferenceNumber);
			AssertEquals($"Summary line Entry number {detail.mrn}.{MRNVersion} - ChargeAmount", ZDecimal.Parse(detail.amount), summaryLine.ChargeAmount);
		}

		void AssertRequest(string messageSubType, BusinessObject linkedObject, string expectedMRN, string expectedDocumentType, [CallerLineNumber] int lineNunber = 0)
		{
			var ediMessage = ediMessages.Where(x => x.EM_MessageSubType == messageSubType && x.EM_LinkedObject == linkedObject).SingleOrDefault();
			AssertNotNull($"[{lineNunber}] Missing EDIMessage: EM_MessageSubType={messageSubType} EM_LinkedObject={linkedObject.TableName}:{linkedObject.PK}", ediMessage);
			if (ediMessage == null)
			{
				return;
			}
			ediMessagesCount--;

			AssertEquals($"[{lineNunber}] EM_ApplicationReference", $"{expectedMRN}.{MRNVersion}", ediMessage.EM_ApplicationReference);

			var xml = XDocument.Parse(ediMessage.EM_MessageText);
			var nsm = new XmlNamespaceManager(new NameTable());
			nsm.AddNamespace("r", xml.Root.Name.NamespaceName);
			AssertEquals($"[{lineNunber}] requestorTraderIdentificationNumber", CustomsRegNo, xml.XPathSelectElement("/r:receiptRequest/r:requestorTraderIdentificationNumber", nsm)?.Value);
			AssertEquals($"[{lineNunber}] customsDeclarationNumber", expectedMRN, xml.XPathSelectElement("/r:receiptRequest/r:receipt/r:customsDeclarationNumber", nsm)?.Value);
			AssertEquals($"[{lineNunber}] customsDeclarationVersion", MRNVersion, xml.XPathSelectElement("/r:receiptRequest/r:receipt/r:customsDeclarationVersion", nsm)?.Value);
			AssertEquals($"[{lineNunber}] documentType", expectedDocumentType, xml.XPathSelectElement("/r:receiptRequest/r:receipt/r:documentType", nsm)?.Value);

			EventsTestHelper.AssertEventAdded(linkedObject, Events.ElectronicAssessmentDecisionStatus, expectedReference: $"|STU=Requested|TYP={expectedDocumentType}", sinceUtc: start);
		}
	});

	public void TestEDIMessageForDetailAlreadyExists()
	{
		var entryHeader1 = CreateEntryHeader("MRN001");
		var entryHeader2 = CreateEntryHeader("MRN002.1");
		var entryHeader3 = CreateEntryHeader("MRN003.2");
		var entryHeader4 = CreateEntryHeader("MRN004");
		var shipment = CreateShipment("MRN005");

		var documentNumber = "11";
		var documentDate = new ZDate(2010, 6, 16);
		var customsOfficeNumber = "CH001251";

		var messageText = TestingData.EdecBordereauTypeBordereauXml(documentNumber, documentDate, customsOfficeNumber);

		(string documentType, string mrn, string amount, string traderReference)[] customsOfficeDetails =
		[
			("VVZ", "MRN001", "-8.30", "TR12345"),
			("VVM", "MRN002", "8.30", "TR12346"),
			("RBZ", "MRN003", "-2.70", "TR12347"),
			("RBM", "MRN004", "-5.00", string.Empty),
			("VVZ", "MRN005", "4.22", "TR6789"),
			("VVZ", "MRN106", "-3.30", "TR67890"),
			("VVZ", "MRN107", "1.15", string.Empty),
			("VVZ", "MRN108", "4.64", "TR67891")
		];

		RemoveDetailsFromMessage(ref messageText);
		foreach (var detail in customsOfficeDetails)
		{
			AddDetailToMessage(ref messageText, detail.documentType, detail.mrn, detail.amount, detail.traderReference);
		}

		var (company, ediMessage) = MessageProcessorTestHelper.CreateCompanyMessagesAndInterchanges(Factory, ApplicationCodeList.Codes.CHCustomsEdec, MessageTypeCodeList.Codes.BOR, MessageSubTypeCodeList.Codes.BordereauResponse, messageText, companyCode: GlbCompany.CurrentCompany.GC_Code, certificateCredential: true, customsRegNo: CustomsRegNo);
		var credentials = GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(company).GlbExternalPassword;
		Factory.Save();

		foreach (var detail in customsOfficeDetails.Take(5))
		{
			var ediMessageEvvForDetail = MessageProcessorTestHelper.CreateEDIMessage(Factory, MessageTypeCodeList.Codes.EVV, GetChargeTypeListFromDocumentType(detail.documentType), direction: ReceiveTransmitList.Codes.Receive, applicationReference: detail.mrn + "." + MRNVersion);
			GetMessageProcessor(Logger).ProcessMessage(ediMessageEvvForDetail);
		}
		Factory.Save();

		MessageProcessorTestHelper.ProcessMessage(GetMessageProcessor(Logger), ediMessage);
		Factory.Save();

		TestDateAttribute.AddMinutes(1);
		var start = ZDateTime.UtcNow;
		var ediMessages = LoadEDIMessages(start);
		var ediMessagesCount = ediMessages.Length;

		AssertRequestForDetailDoesNotExist(MessageSubTypeCodeList.Codes.TaxationDecisionCustomsDuties, entryHeader1);
		AssertRequestForDetailDoesNotExist(MessageSubTypeCodeList.Codes.TaxationDecisionVat, entryHeader2);
		AssertRequestForDetailDoesNotExist(MessageSubTypeCodeList.Codes.TaxationDecisionReimbursementCustomsDuties, entryHeader3);
		AssertRequestForDetailDoesNotExist(MessageSubTypeCodeList.Codes.TaxationDecisionReimbursementVat, entryHeader4);
		AssertRequestForDetailDoesNotExist(MessageSubTypeCodeList.Codes.TaxationDecisionCustomsDuties, shipment);

		var query = new ZDBOnlyQuery(typeof(CustomsSummaryHeader));
		query.AddToFilter(CusStatementHeaderSchema.B2_StatementNumber, documentNumber);
		query.AddToFilter(CusStatementHeaderSchema.B2_ProcessDate, documentDate);
		var summaryHeader = Factory.LoadTop1<CustomsSummaryHeader>(query);

		foreach (var detail in customsOfficeDetails.Take(5))
		{
			var summaryLine = summaryHeader.SummaryLines.FirstOrDefault(y => y.B3_EntryNum == $"{detail.mrn}.{MRNVersion}");
			AssertNotNull($"Summary line for Entry number {detail.mrn}.{MRNVersion}", summaryLine);
			AssertEquals($"Summary line Entry number {detail.mrn}.{MRNVersion} - B3_Status", "RCV", summaryLine.B3_Status);
		}

		void AssertRequestForDetailDoesNotExist(string messageSubType, BusinessObject linkedObject, [CallerLineNumber] int lineNunber = 0)
		{
			var ediMessage = ediMessages.Where(x => x.EM_MessageSubType == messageSubType && x.EM_LinkedObject == linkedObject).SingleOrDefault();
			AssertNull($"[{lineNunber}] Missing EDIMessage: EM_MessageSubType={messageSubType} EM_LinkedObject={linkedObject.TableName}:{linkedObject.PK}", ediMessage);
		}

		string GetChargeTypeListFromDocumentType(string documentType)
		{
			return documentType switch
			{
				BordereauChargeTypeList.Codes.Duties => MessageSubTypeCodeList.Codes.TaxationDecisionCustomsDuties,
				BordereauChargeTypeList.Codes.Vat => MessageSubTypeCodeList.Codes.TaxationDecisionVat,
				BordereauChargeTypeList.Codes.DutiesRefund => MessageSubTypeCodeList.Codes.TaxationDecisionReimbursementCustomsDuties,
				BordereauChargeTypeList.Codes.VatRefund => MessageSubTypeCodeList.Codes.TaxationDecisionReimbursementVat,
				_ => string.Empty
			};
		}
	}

	public void TestCustomsSummaryAlreadyExists() => CombineAssertions(() =>
	{
		var entryHeader = CreateEntryHeader("MRN001");

		AssertBordereauProcessed("B001");
		AssertBordereauProcessed("B002", sameCompany: false);
		AssertBordereauProcessed("B003", sameNumber: false);
		AssertBordereauProcessed("B004", sameDate: false);

		void AssertBordereauProcessed(string documentNumber, bool sameCompany = true, bool sameNumber = true, bool sameDate = true, [CallerLineNumber] int lineNunber = 0)
		{
			var processingExpected = !sameCompany || !sameNumber || !sameDate;

			var messageText = TestingData.EdecBordereauTypeBordereauXml(documentNumber: documentNumber, documentDate: new ZDate(2024, 1, 1));
			RemoveDetailsFromMessage(ref messageText);
			AddDetailToMessage(ref messageText, "VVZ", "MRN001");

			var (company, ediMessage) = MessageProcessorTestHelper.CreateCompanyMessagesAndInterchanges(Factory, ApplicationCodeList.Codes.CHCustomsEdec, MessageTypeCodeList.Codes.BOR, MessageSubTypeCodeList.Codes.BordereauResponse, messageText, certificateCredential: true, customsRegNo: CustomsRegNo);
			var credentials = GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(company).GlbExternalPassword;
			CreateCustomsSummaryHeader(sameCompany ? company : GlbCompany.CurrentCompany, sameNumber ? documentNumber : "B999", new ZDate(2024, 1, sameDate ? 1 : 9));
			Factory.Save();

			TestDateAttribute.AddMinutes(1);
			var start = ZDateTime.UtcNow;
			MessageProcessorTestHelper.ProcessMessage(GetMessageProcessor(Logger), ediMessage);
			Factory.Save();
			AssertEquals($"[{lineNunber}] No. of created messsages", processingExpected ? 1 : 0, LoadEDIMessages(start).Length);
		}
	});

	CusEntryHeader CreateEntryHeader(string mrn, GlbCompany company = null)
	{
		company ??= GlbCompany.CurrentCompany;
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_GC = company.PK;
		declaration.JE_GB = company.FirstActiveBranch.PK;
		declaration.JE_MessageType = CHJobMessageTypeList.Codes.Import;
		var entryHeader = declaration.ActiveEntryHeaders.AddNew();
		entryHeader.MovementReferenceNumberSetter(mrn);
		return entryHeader;
	}

	ForwardingShipment CreateShipment(string mrn, string entryNumCountry = Core.Constants.CountryCodes.Switzerland, GlbCompany company = null)
	{
		company ??= GlbCompany.CurrentCompany;
		var shipment = Factory.New<ForwardingShipment>();
		shipment.CreateJobHeaderWithMutex();
		shipment.Job.JH_GC = company.PK;
		shipment.Job.JH_GB = company.FirstActiveBranch.PK;
		var entryNum = Factory.New<CusEntryNumber>();
		entryNum.CE_ParentTable = shipment.TableName;
		entryNum.CE_ParentID = shipment.PK;
		entryNum.CE_EntryNum = mrn;
		entryNum.CE_RN_NKCountryCode = entryNumCountry;
		return shipment;
	}

	void CreateCustomsSummaryHeader(GlbCompany company, string bodereauNumber, ZDate creationDate)
	{
		var summaryHeader = Factory.New<CustomsSummaryHeader>();
		summaryHeader.B2_StatementNumber = bodereauNumber;
		summaryHeader.B2_ProcessDate = creationDate;
		summaryHeader.B2_GC = company.PK;
	}

	static void RemoveDetailsFromMessage(ref string messageText)
	{
		messageText = Regex.Replace(messageText, @"(?s)(<detail>.*</detail>)", string.Empty);
	}

	static void AddDetailToMessage(ref string messageText, string documentType, string customsReference, string amount = "-8.30", string traderReference = null)
	{
		var detail = $"""
					<detail>
						<documentType>
							<documentTypeAbbreviation>{documentType}</documentTypeAbbreviation>
							<documentTypeName>...</documentTypeName>
						</documentType>
						{(string.IsNullOrEmpty(traderReference) ? string.Empty : $"<traderReference>{traderReference}</traderReference>")}
						<customsReference>{customsReference}</customsReference>
						<customsDeclarationVersion>{MRNVersion}</customsDeclarationVersion>
						<amount>{amount}</amount>
					</detail>
					""";
		messageText = Regex.Replace(messageText, @"(?s)(?=</customsOffice>)", detail);
	}
	EDIMessage[] LoadEDIMessages(ZDateTime since)
	{
		return Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, since));
	}

	const string CustomsRegNo = "CREG001";
	const string MRNVersion = "8";
}
