using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Customs.CH.Business.MessagingConstants;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(BordereauListResponseMessageProcessor))]
sealed class BordereauListResponseMessageProcessorTest : TestCaseWithFactory
{
	BordereauListResponseMessageProcessor GetMessageProcessor(LoggingInformation logger) => new BordereauListResponseMessageProcessor(Logger);

	LoggingInformationForTesting Logger => logger ?? (logger = new LoggingInformationForTesting());
	LoggingInformationForTesting logger;

	public void TestMessageFriendlyName()
	{
		var processor = GetMessageProcessor(Logger);
		AssertEquals("Customs Bordereau List Message Response Processor", processor.MessageFriendlyName);
	}

	public void TestMessageFilter()
	{
		var ediMessage = Factory.New<EDIMessage>();
		var processor = GetMessageProcessor(Logger);

		AssertCanProcess(true, MessageTypeCodeList.Codes.BOR, MessageSubTypeCodeList.Codes.BordereauList);
		AssertCanProcess(false, MessageTypeCodeList.Codes.BOR, string.Empty);
		AssertCanProcess(false, string.Empty, MessageSubTypeCodeList.Codes.BordereauList);
		AssertCanProcess(false, MessageTypeCodeList.Codes.BOR, MessageSubTypeCodeList.Codes.BordereauList, applicationCode: string.Empty);

		void AssertCanProcess(bool expectedCanProcess, string messageType, string messageSubType, string applicationCode = ApplicationCodeList.Codes.CHCustomsEdec)
		{
			ediMessage.EM_ApplicationCode = applicationCode;
			ediMessage.EM_MessageType = messageType;
			ediMessage.EM_MessageSubType = messageSubType;
			AssertEquals("", expectedCanProcess, processor.CanProcess(ediMessage));
		}
	}

	[TestDate]
	public void TestBordereauRequests() => CombineAssertions(() =>
	{
		const string customsRegNo = "CUSREG01";

		var messageText = TestingData.EdecBordereauTypeBordereauListXml(
			bordereauNumber1: "1", processingCenterNumber1: "71", creationDate1: new ZDate(2024, 1, 1),
			bordereauNumber2: "2", processingCenterNumber2: "72", creationDate2: new ZDate(2024, 1, 2));
		var (company, ediMessage) = MessageProcessorTestHelper.CreateCompanyMessagesAndInterchanges(Factory, ApplicationCodeList.Codes.CHCustomsEdec, MessageTypeCodeList.Codes.BOR, MessageSubTypeCodeList.Codes.BordereauList, messageText, certificateCredential: true, customsRegNo: customsRegNo);
		var credentials = GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(company).GlbExternalPassword;

		TestDateAttribute.AddMinutes(1);
		var start = ZDateTime.UtcNow;
		MessageProcessorTestHelper.ProcessMessage(GetMessageProcessor(Logger), ediMessage);
		Factory.Save();

		var ediMessages = LoadEDIMessages(start);
		AssertEquals("Count", 2, ediMessages.Length);
		AssertMessage(ediMessages[0], "1", "71", new ZDate(2024, 1, 1));
		AssertMessage(ediMessages[1], "2", "72", new ZDate(2024, 1, 2));
		AssertNotEquals("Unique EM_MessageNum", ediMessages[0].EM_MessageNum, ediMessages[1].EM_MessageNum);
		AssertNotEquals("Unique EI_InterchangeNum", ediMessages[0].Interchange.EI_InterchangeNum, ediMessages[1].Interchange.EI_InterchangeNum);

		void AssertMessage(EDIMessage ediMessage, string expectedBordereauNumber, string expectedProcessingCenter, ZDate expectedCreationDate)
		{
			AssertEquals("EM_ApplicationCode", ApplicationCodeList.Codes.CHCustomsEdec, ediMessage.EM_ApplicationCode);
			AssertEquals("EM_MessageType", MessageTypeCodeList.Codes.BOR, ediMessage.EM_MessageType);
			AssertEquals("EM_MessageSubType", MessageSubTypeCodeList.Codes.BordereauResponse, ediMessage.EM_MessageSubType);
			AssertEquals("EM_ReceiveTransmit", ReceiveTransmitList.Codes.Transmit, ediMessage.EM_ReceiveTransmit);
			AssertEquals("EM_Status", EDIMessage.Status.Sent, ediMessage.EM_Status);
			AssertEquals("EM_LinkTable", GlbCompanySchema.Constants.TableName, ediMessage.EM_LinkTable);
			AssertEquals("EM_LinkUniqueID", company.PK, ediMessage.EM_LinkUniqueID);
			AssertEquals("EM_GP", credentials.PK, ediMessage.EM_GP);

			var ediInterchange = ediMessage.Interchange;
			AssertEquals("EI_ApplicationCode", ApplicationCodeList.Codes.CHCustomsEdec, ediInterchange.EI_ApplicationCode);
			AssertEquals("EI_InterchangeType", MessageTypeCodeList.Codes.BOR, ediInterchange.EI_InterchangeType);
			AssertEquals("EI_ReceiveTransmit", ReceiveTransmitList.Codes.Transmit, ediInterchange.EI_ReceiveTransmit);
			AssertEquals("EI_From", company.LicenceKeyIdentifier, ediInterchange.EI_From);
			AssertEquals("EI_To", CustomsDestinationCodes.CustomsBordereauSoap, ediInterchange.EI_To);
			AssertEquals("EI_TransportType", EDIInterchange.TransportType.xT, ediInterchange.EI_TransportType);
			AssertEquals("EI_Status", EDIInterchange.Status.Queued, ediInterchange.EI_Status);
			AssertEquals("EI_Priority", EDIInterchangePriorityList.Codes.High, ediInterchange.EI_Priority);
			AssertNotEquals("EI_SessionGUID", ZString.Empty, ediInterchange.EI_SessionGUID);
			AssertEquals("EI_IsActive", ZBool.True, ediInterchange.EI_IsActive);
			AssertEquals("EI_GP", credentials.PK, ediInterchange.EI_GP);
			AssertEquals("EI_HeaderText", ZString.Empty, ediInterchange.EI_HeaderText);

			var emXml = XDocument.Parse(ediMessage.EM_MessageText);
			var nsm = new XmlNamespaceManager(new NameTable());
			nsm.AddNamespace("b", emXml.Root.Name.NamespaceName);
			AssertEquals("<requestorTraderIdentificationNumber>", customsRegNo, emXml.XPathSelectElement("/b:bordereauRequest/b:requestorTraderIdentificationNumber", nsm)?.Value);
			var bordereau = emXml.XPathSelectElement("/b:bordereauRequest/b:bordereau", nsm);
			AssertEquals("<bordereauNumber>", expectedBordereauNumber, bordereau.XPathSelectElement("b:bordereauNumber", nsm)?.Value);
			AssertEquals("<processingCenterNumber>", expectedProcessingCenter, bordereau.XPathSelectElement("b:processingCenterNumber", nsm)?.Value);
			AssertEquals("<creationDate>", expectedCreationDate.ToString("yyyy-MM-dd"), bordereau.XPathSelectElement("b:creationDate", nsm)?.Value);

			var eiXml = XDocument.Parse(ediInterchange.EI_BodyText);
			nsm.AddNamespace("s", eiXml.Root.Name.NamespaceName);
			AssertEquals("EI_BodyText has SOAP envelope", "Envelope", eiXml.Root.Name.LocalName);
			AssertNotNull("EI_BodyText contains request", eiXml.XPathSelectElement("/s:Envelope/s:Body/b:bordereauRequest", nsm));
		}
	});

	[TestDate]
	public void TestBordereauRequestAlreadyProcessed() => CombineAssertions(() =>
	{
		var messageText = TestingData.EdecBordereauTypeBordereauListXml(
			bordereauNumber1: "1", processingCenterNumber1: "71", creationDate1: new ZDate(2024, 1, 1),
			bordereauNumber2: "2", processingCenterNumber2: "72", creationDate2: new ZDate(2024, 1, 2));
		var (company, ediMessage) = MessageProcessorTestHelper.CreateCompanyMessagesAndInterchanges(Factory, ApplicationCodeList.Codes.CHCustomsEdec, MessageTypeCodeList.Codes.BOR, MessageSubTypeCodeList.Codes.BordereauList, messageText, certificateCredential: true);
		var credentials = GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(company).GlbExternalPassword;

		var otherCompany = MessageProcessorTestHelper.CreateCompany(Factory, companyCode: "C99", branchCode: "B99");

		CreateCustomsSummaryHeader(company, "1", new ZDate(2024, 1, 1));
		CreateCustomsSummaryHeader(company, "9", new ZDate(2024, 1, 2));
		CreateCustomsSummaryHeader(company, "2", new ZDate(2024, 1, 9));
		CreateCustomsSummaryHeader(otherCompany, "2", new ZDate(2024, 1, 2));
		Factory.Save();

		TestDateAttribute.AddMinutes(1);
		var start = ZDateTime.UtcNow;
		MessageProcessorTestHelper.ProcessMessage(GetMessageProcessor(Logger), ediMessage);
		Factory.Save();

		var ediMessages = LoadEDIMessages(start);
		AssertEquals("Message count", 1, ediMessages.Length);
		var xml = XDocument.Parse(ediMessages[0].EM_MessageText);
		var nsm = new XmlNamespaceManager(new NameTable());
		nsm.AddNamespace("b", xml.Root.Name.NamespaceName);
		AssertEquals("<bordereauNumber>", "2", xml.XPathSelectElement("/b:bordereauRequest/b:bordereau/b:bordereauNumber", nsm)?.Value);
	});

	void CreateCustomsSummaryHeader(GlbCompany company, string boderoNumber, ZDate creationDate)
	{
		var summaryHeader = Factory.New<CustomsSummaryHeader>();
		summaryHeader.B2_StatementNumber = boderoNumber;
		summaryHeader.B2_ProcessDate = creationDate;
		summaryHeader.B2_GC = company.PK;
	}

	EDIMessage[] LoadEDIMessages(ZDateTime since)
	{
		return Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, since));
	}
}
