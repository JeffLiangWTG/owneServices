using System;
using System.IO;
using System.Reflection;
using System.Xml.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using static NUnit.Framework.Assertion;
using static NUnit.Framework.AssertionWithHtml;

namespace Enterprise.Customs.CH.Business.Testing;

public static class TestingData
{
	internal static CHEDIMessage CreateOutboundMessage(this BusinessObjectFactory factory, ZString messageType, ZString messageText
		, string applicationCode = ApplicationCodeList.Codes.CHCustomsEdec
		, string direction = CHEDIMessage.Direction.Transmit
		, string status = CHEDIMessage.Status.Queued
		, ZDateTime? heldUntilDate = null)
	{
		var message = factory.New<CHEDIMessage>();
		message.EM_ApplicationCode = applicationCode;
		message.EM_MessageType = messageType;
		message.EM_ReceiveTransmit = direction;
		message.EM_Status = status;
		message.EM_MessageText = messageText;
		message.EM_IsTestMessage = true;
		message.EM_ApplicationReference = "certificateName";
		message.EM_HeldUntilDate = heldUntilDate ?? ZDateTime.Empty;
		return message;
	}

	internal static void AssertOutgoingInterchange(ZString declarationType, EDIInterchange interchange, string expectedInterchangeType, string expectedDestination, string expectedTransportType, string expectedStatus, string expectedBody = null)
	{
		AssertNotNull("interchange", interchange);

		CombineAssertions(() =>
		{
			AssertEquals($"{declarationType} interchange.EI_TransportType", expectedTransportType, interchange.EI_TransportType);
			AssertEquals($"{declarationType} interchange.EI_ApplicationCode", ApplicationCodeList.Codes.CHCustomsEdec, interchange.EI_ApplicationCode);
			AssertEquals($"{declarationType} interchange.EI_ReceiveTransmit", EDIInterchange.Direction.Transmit, interchange.EI_ReceiveTransmit);
			AssertEquals($"{declarationType} interchange.EI_InterchangeType", expectedInterchangeType, interchange.EI_InterchangeType);
			AssertEquals($"{declarationType} interchange.EI_From", GlbCompany.CurrentCompany.LicenceKeyIdentifier, interchange.EI_From);
			AssertEquals($"{declarationType} interchange.EI_To", expectedDestination, interchange.EI_To);
			AssertEquals($"{declarationType} interchange.EI_Status", expectedStatus, interchange.EI_Status);
			AssertEquals($"{declarationType} interchange.EI_Priority", "HGH", interchange.EI_Priority);
			AssertNotEquals($"{declarationType} interchange.EI_SessionGUID", ZGuid.Empty, interchange.EI_SessionGUID);
			AssertEquals($"{declarationType} interchange.EI_HeaderText", ZString.Empty, interchange.EI_HeaderText);
			AssertEquals($"{declarationType} interchange.EI_FooterText", ZString.Empty, interchange.EI_FooterText);
			if (expectedBody != null)
			{
				var actualDoc = XDocument.Load(new StringReader(interchange.EI_BodyText));
				var expectedDoc = XDocument.Load(new StringReader(expectedBody));
				Assert($"{declarationType} interchange.EI_BodyText", XNode.DeepEquals(expectedDoc, actualDoc));
			}
		});
	}

	internal static ZString MessageBodyText => @"<?xml version=""1.0"" encoding=""utf-8""?>
<goodsDeclarations xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" schemaVersion=""4.0"" xmlns=""http://www.e-dec.ch/xml/schema/edec/v4"">
  <goodsDeclaration>
  </goodsDeclaration>
</goodsDeclarations>";

	internal static ZString SoapMessageText => @"<?xml version=""1.0"" encoding=""utf-8""?>
<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"">
  <soapenv:Header />
  <soapenv:Body>
    <goodsDeclarations xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" schemaVersion=""4.0"" xmlns=""http://www.e-dec.ch/xml/schema/edec/v4"">
      <goodsDeclaration>
      </goodsDeclaration>
    </goodsDeclarations>
  </soapenv:Body>
</soapenv:Envelope>";

	public static string ReadManifestResourceContent(string resourceName, Assembly assembly = null)
	{
		assembly = assembly ?? Assembly.GetAssembly(typeof(TestingData));
		var result = string.Empty;
		using (var stream = assembly.GetManifestResourceStream(resourceName) ?? throw new Exception($@"Missing resource: ""{resourceName}"" in ""{assembly.FullName}"""))
		using (var reader = new StreamReader(stream))
		{
			result = reader.ReadToEnd();
		}
		return result;
	}

	public static string InputEdecResponseAcceptanceResponseSOAP => ReadManifestResourceContent(@"Enterprise.Customs.CH.Business.Test.Messaging.TestFiles.Input.EdecResponseAcceptanceSOAP.txt");

	public static string InputEdecResponseAcceptanceResponseMail => ReadManifestResourceContent(@"Enterprise.Customs.CH.Business.Test.Messaging.TestFiles.Input.EdecResponseAcceptanceMail.txt");

	public static string InputEdecResponseAcceptanceXml => ReadManifestResourceContent(@"Enterprise.Customs.CH.Business.Test.Messaging.TestFiles.Input.EdecResponseAcceptance.xml");

	public static string InputEdecResponseAcceptanceDocOnlyResponse => ReadManifestResourceContent(@"Enterprise.Customs.CH.Business.Test.Messaging.TestFiles.Input.EdecResponseAcceptanceDocOnly.txt");

	public static string InputEdecEdecResponseRejectionResponseSOAP => ReadManifestResourceContent(@"Enterprise.Customs.CH.Business.Test.Messaging.TestFiles.Input.EdecResponseRejectionSOAP.txt");

	public static string InputEdecEdecResponseRejectionResponseMail => ReadManifestResourceContent(@"Enterprise.Customs.CH.Business.Test.Messaging.TestFiles.Input.EdecResponseRejectionMail.txt");

	public static string InputEdecResponseRejectionXml => ReadManifestResourceContent(@"Enterprise.Customs.CH.Business.Test.Messaging.TestFiles.Input.EdecResponseRejection.xml");

	public static string InputEdecEdecResponseStatusResponseMail => ReadManifestResourceContent(@"Enterprise.Customs.CH.Business.Test.Messaging.TestFiles.Input.EdecResponseStatusMail.txt");

	public static string InputEComResponseAcceptanceSOAP => ReadManifestResourceContent(@"Enterprise.Customs.CH.Business.Test.Messaging.TestFiles.Input.EComResponseAcceptanceSOAP.txt");

	public static string InputEComResponseRejectionSOAP => ReadManifestResourceContent(@"Enterprise.Customs.CH.Business.Test.Messaging.TestFiles.Input.EComResponseRejectionSOAP.txt");

	public static string InputEComResponseXMLSchemaErrorsSOAP => ReadManifestResourceContent(@"Enterprise.Customs.CH.Business.Test.Messaging.TestFiles.Input.EComResponseXMLSchemaErrorsSOAP.txt");

	public static string InputEComCustomsCompliantRequestMail => ReadManifestResourceContent(@"Enterprise.Customs.CH.Business.Test.Messaging.TestFiles.Input.EComCustomsCompliantRequestMail.txt");

	public static string InputEdbResponseAcceptance => ReadManifestResourceContent(@"Enterprise.Customs.CH.Business.Test.Messaging.TestFiles.Input.EbdAcceptance.txt");

	public static string InputEdbResponseRejection => ReadManifestResourceContent(@"Enterprise.Customs.CH.Business.Test.Messaging.TestFiles.Input.EbdRejection.txt");

	public static string InputEdbUnsupportedSchema => ReadManifestResourceContent(@"Enterprise.Customs.CH.Business.Test.Messaging.TestFiles.Input.EbdUnsupportedSchema.txt");

	public static string InputTokenRefreshResponse => ReadManifestResourceContent(@"Enterprise.Customs.CH.Business.Test.Messaging.TestFiles.Input.InputTokenRefreshResponse.txt");

	public static string InputTokenRefreshResponseError => ReadManifestResourceContent(@"Enterprise.Customs.CH.Business.Test.Messaging.TestFiles.Input.InputTokenRefreshResponseError.txt");

	public static string InputEvvResponse => ReadManifestResourceContent(@"Enterprise.Customs.CH.Business.Test.Messaging.TestFiles.Input.eVVResponseDuties.txt");

	public static string ExpectedEdecResponseAcceptanceDocOnlySOAP => ReadManifestResourceContent(@"Enterprise.Customs.CH.Business.Test.Messaging.TestFiles.Output.EdecResponseAcceptanceDocOnlySOAP.txt");

	public static string ExpectedEdecResponseAcceptanceXmlOnlySOAP => ReadManifestResourceContent(@"Enterprise.Customs.CH.Business.Test.Messaging.TestFiles.Output.EdecResponseAcceptanceXmlOnlySOAP.txt");

	public static string ExpectedEdecResponseRejectionXmlOnlySOAP => ReadManifestResourceContent(@"Enterprise.Customs.CH.Business.Test.Messaging.TestFiles.Output.EdecResponseRejectionXmlOnlySOAP.txt");

	public static string ExpectedEdecResponseAcceptanceXmlOnlyMail => ReadManifestResourceContent(@"Enterprise.Customs.CH.Business.Test.Messaging.TestFiles.Output.EdecResponseAcceptanceXmlOnlyMail.txt");

	public static string ExpectedEdecResponseRejectionXmlOnlyMail => ReadManifestResourceContent(@"Enterprise.Customs.CH.Business.Test.Messaging.TestFiles.Output.EdecResponseRejectionXmlOnlyMail.txt");

	public static string ExpectedEdecResponseStatusXmlOnlyMail => ReadManifestResourceContent(@"Enterprise.Customs.CH.Business.Test.Messaging.TestFiles.Output.EdecResponseStatusXmlOnlyMail.txt");

	public static string GetXTConfigurationRequestMessage(string companyCode = "SAS", string status = "VAL", string customsID = "87654321") => string.Format(ReadManifestResourceContent(@"Enterprise.Customs.CH.Business.Test.Messaging.TestFiles.Input.XTConfigurationRequestMessage.txt"), companyCode, status, customsID);

	public static string XTConfigurationResponseMessageFailure => ReadManifestResourceContent(@"Enterprise.Customs.CH.Business.Test.Messaging.TestFiles.Input.XTConfigurationResponseMessageFailure.txt");

	public static string XTConfigurationResponseMessageSuccess => ReadManifestResourceContent(@"Enterprise.Customs.CH.Business.Test.Messaging.TestFiles.Input.XTConfigurationResponseMessageSuccess.txt");

	public static string InputEvvResponseDTY(string documentNumber = "22CHEI000043143375", string documentVersion = "1") => string.Format(ReadManifestResourceContent(@"Enterprise.Customs.CH.Business.Test.Business.MessageProcessors.Evv.TestFiles.eVV_Response-OK-DTY.xml"), documentNumber, documentVersion);
	public static string InputEvvResponseAllDTY(string documentLanguage = "de", string transportationType = "2", string declarationType = "1", string documentNumber = "22CHEI000043143375", string documentVersion = "1", string requestorTraderIdentificationNumber = "CHE326684996") => string.Format(ReadManifestResourceContent(@"Enterprise.Customs.CH.Business.Test.Business.MessageProcessors.Evv.TestFiles.eVV_Response-All-DTY.xml"), documentLanguage, transportationType, declarationType, documentNumber, documentVersion, requestorTraderIdentificationNumber);
	public static string InputEvvResponseVAT(string invoiceCurrencyType = "1", string vatSuffix = "1", string documentLanguage = "de", string documentNumber = "22CHEI000043143375", string documentVersion = "1", string requestorTraderIdentificationNumber = "CHE326684996") => string.Format(ReadManifestResourceContent(@"Enterprise.Customs.CH.Business.Test.Business.MessageProcessors.Evv.TestFiles.eVV_Response-OK-VAT.xml"), invoiceCurrencyType, vatSuffix, documentLanguage, documentNumber, documentVersion, requestorTraderIdentificationNumber);
	public static string InputEvvResponseVATValidSignature => ReadManifestResourceContent(@"Enterprise.Customs.CH.Business.Test.Business.MessageProcessors.Evv.TestFiles.eVV_Response-OK-VAT-ValidSignature.xml");
	public static string InputEvvResponseVATInvalidSignature => ReadManifestResourceContent(@"Enterprise.Customs.CH.Business.Test.Business.MessageProcessors.Evv.TestFiles.eVV_Response-OK-VAT-InvalidSignature.xml");
	public static string InputEvvResponseVATExpiredCertificate => ReadManifestResourceContent(@"Enterprise.Customs.CH.Business.Test.Business.MessageProcessors.Evv.TestFiles.eVV_Response-OK-VAT-ExpiredCertificate.xml");
	public static string InputEvvResponseVATInvalidSwissCustomsCertificate => ReadManifestResourceContent(@"Enterprise.Customs.CH.Business.Test.Business.MessageProcessors.Evv.TestFiles.eVV_Response-OK-VAT-InvalidSwissCustomsCertificate.xml");
	public static string InputEvvResponseEXP => ReadManifestResourceContent(@"Enterprise.Customs.CH.Business.Test.Business.MessageProcessors.Evv.TestFiles.eVV_Response-OK-EXP.xml");
	public static string InputEvvResponseRuleErrors => ReadManifestResourceContent(@"Enterprise.Customs.CH.Business.Test.Business.MessageProcessors.Evv.TestFiles.eVV_Response-REJ-ruleErrors.xml");
	public static string InputEvvResponseXMLSchemaErrors => ReadManifestResourceContent(@"Enterprise.Customs.CH.Business.Test.Business.MessageProcessors.Evv.TestFiles.eVV_Response-REJ-XMLSchemaErrors.xml");
	public static string InputEvvResponseRefundVAT(string documentNumber = "22CHEI000043143375", string documentVersion = "1") => string.Format(ReadManifestResourceContent("Enterprise.Customs.CH.Business.Test.Business.MessageProcessors.Evv.TestFiles.eVV_Response-OK-RBV.xml"), documentNumber, documentVersion);
	public static string InputEvvResponseRefundCustomsDuties(string documentNumber = "22CHEI000043143375", string documentVersion = "1") => string.Format(ReadManifestResourceContent("Enterprise.Customs.CH.Business.Test.Business.MessageProcessors.Evv.TestFiles.eVV_Response-OK-RBD.xml"), documentNumber, documentVersion);

	public static string InputEComResponseAcceptance => ReadManifestResourceContent(@"Enterprise.Customs.CH.Business.Test.Business.MessageProcessors.ECom.TestFiles.EComResponseAcceptance.xml");
	public static string InputEComResponseRejection => ReadManifestResourceContent(@"Enterprise.Customs.CH.Business.Test.Business.MessageProcessors.ECom.TestFiles.EComResponseRejection.xml");
	public static string InputEComResponseXMLSchemaErrors => ReadManifestResourceContent(@"Enterprise.Customs.CH.Business.Test.Business.MessageProcessors.ECom.TestFiles.EComResponseXMLSchemaErrors.xml");
	public static string GetEComRequest(string customsDeclarationNumber = "14CHEI000000358095") => String.Format(ReadManifestResourceContent(@"Enterprise.Customs.CH.Business.Test.Business.MessageProcessors.ECom.TestFiles.EComRequest.xml"), customsDeclarationNumber);

	public static string InputUniversalEvent => ReadManifestResourceContent(@"Enterprise.Customs.CH.Business.Test.Messaging.TestFiles.Input.UniversalEvent.xml");

	public static string InputUniversalEventReq => ReadManifestResourceContent(@"Enterprise.Customs.CH.Business.Test.Messaging.TestFiles.Input.UniversalEventREQ.xml");

	public static string GetNC084(string documentId = "241112112808153097", string mrn = "24CH11EXXZYM40H3K1", string mrnVersion = "1", string oppositeInformationText = null) => string.Format(ReadManifestResourceContent(@"Enterprise.Customs.CH.Business.Test.Messaging.TestFiles.Input.NC084.xml"), documentId, mrn, mrnVersion, oppositeInformationText ?? CustomsMessageHelper.PartnerTopic);
	public static string GetNC124(string correlationId = "0091444b-8599-4333-b1f6-9a3b105775d9", string mrn = "21CHODUG284YTOACN8", string mrnVersion = "1", string decision = "ACCEPTED", string oppositeInformationText = null) => string.Format(ReadManifestResourceContent(@"Enterprise.Customs.CH.Business.Test.Messaging.TestFiles.Input.NC124.xml"), correlationId, mrn, mrnVersion, decision, oppositeInformationText ?? CustomsMessageHelper.PartnerTopic);
	public static string GetNC124V2_Import(string correlationId = "0091444b-8599-4333-b1f6-9a3b105775d9", string mrn = "21CHODUG284YTOACN8", string mrnVersion = "1", string decision = "ACCEPTED", string oppositeInformationText = null) => string.Format(ReadManifestResourceContent(@"Enterprise.Customs.CH.Business.Test.Messaging.TestFiles.Input.NC124V2_Import.xml"), correlationId, mrn, mrnVersion, decision, oppositeInformationText ?? CustomsMessageHelper.PartnerTopic);
	public static string GetNC124V2_Export(string correlationId = "0091444b-8599-4333-b1f6-9a3b105775d9", string mrn = "21CHODUG284YTOACN8", string mrnVersion = "1", string decision = "ACCEPTED", string oppositeInformationText = null) => string.Format(ReadManifestResourceContent(@"Enterprise.Customs.CH.Business.Test.Messaging.TestFiles.Input.NC124V2_Export.xml"), correlationId, mrn, mrnVersion, decision, oppositeInformationText ?? CustomsMessageHelper.PartnerTopic);
	public static string GetNC909(string correlationId = "0091444b-8599-4333-b1f6-9a3b105775d9") => string.Format(ReadManifestResourceContent(@"Enterprise.Customs.CH.Business.Test.Messaging.TestFiles.Input.NC909.xml"), correlationId);
	public static string GetNE004(string correlationId = "hiRJuniqueFjJ013gsvDeEFdXttcdDCg001", ZDate? activationDeadline = null, ZDateTime? decisionDateAndTime = null, string decision = "ACCEPTED", bool initiatedByCustoms = false, string gdrn = "21CHODUG284YTOACN8", string gdrnVersion = "1") => string.Format(ReadManifestResourceContent(@"Enterprise.Customs.CH.Business.Test.Messaging.TestFiles.Input.NE004.xml"), correlationId, ToXsDate(activationDeadline ?? new ZDate(2023, 07, 01)), ToXsDateTimeNoOffset(decisionDateAndTime ?? new ZDateTime(2023, 06, 01, 12, 42, 00)), decision, ToXsBoolean(initiatedByCustoms), gdrn, gdrnVersion);
	public static string GetNE009(string correlationId = "4d88c2b7-dfba-42c7-bd69-f3a8eb622bdf", string gdrnNumber = "23CH12EXTZMNGLFXN3", string gdrnVersion = "1", ZDateTime? issueDateTimeUTC = null, string decision = "ACCEPTED", string initiatedByCustoms = "false") => string.Format(ReadManifestResourceContent(@"Enterprise.Customs.CH.Business.Test.Messaging.TestFiles.Input.NE009.xml"), correlationId, gdrnNumber, gdrnVersion, ToXsDateTimeNoOffset(issueDateTimeUTC ?? new ZDateTime(2023, 06, 01, 12, 42, 00)), decision, initiatedByCustoms);
	public static string GetNE028(string correlationId = "hiRJuniqueFjJxkZgsvDeEFdXttcdDCg012", ZDate? expiryDate = null, ZDateTime? issueDateTimeUTC = null, string decision = "ACCEPTED") => string.Format(ReadManifestResourceContent(@"Enterprise.Customs.CH.Business.Test.Messaging.TestFiles.Input.NE028.xml"), correlationId, ToXsDate(expiryDate ?? new ZDate(2023, 07, 01)), ToXsDateTimeNoOffset(issueDateTimeUTC ?? new ZDateTime(2023, 06, 01, 12, 42, 00)), decision);
	public static string GetNE029(string gdrn = "21CHODUG284YTOACN8", string gdrnVersion = "1", ZDateTime? preparationDateAndTime = null, string oppositeInformationText = null) => string.Format(ReadManifestResourceContent(@"Enterprise.Customs.CH.Business.Test.Messaging.TestFiles.Input.NE029.xml"), gdrn, gdrnVersion, ToXsDateTimeNoOffset(preparationDateAndTime ?? new ZDateTime(2008, 9, 29, 3, 49, 45)), oppositeInformationText ?? CustomsMessageHelper.PartnerTopic);
	public static string GetNE060(string gdrnNumber = "21CHA6F0XKQGN8AMN0", string gdrnVersion = "2", string selectionStatus = "PRELIMINARY", string inspectionDecision = "CLEAR", string oppositeInformationText = null) => string.Format(ReadManifestResourceContent(@"Enterprise.Customs.CH.Business.Test.Messaging.TestFiles.Input.NE060.xml"), gdrnNumber, gdrnVersion, selectionStatus, inspectionDecision, oppositeInformationText ?? CustomsMessageHelper.PartnerTopic);
	public static string GetNE083(string gdrnNumber = "21CHA6F0XKQGN8AMN0", string gdrnVersion = "2", string oppositeInformationText = null) => string.Format(ReadManifestResourceContent(@"Enterprise.Customs.CH.Business.Test.Messaging.TestFiles.Input.NE083.xml"), gdrnNumber, gdrnVersion, oppositeInformationText ?? CustomsMessageHelper.PartnerTopic);
	public static string GetNE096(string correlationId = "hiRJuniqueFjJ013gsvDeEFdXttcdDCg001", string gdrnNumber = "21CHODUG284YTOACN8", string gdrnVersion = "1", ZDateTime? issueDateTimeUTC = null, string decision = "ACCEPTED", string initiatedByCustoms = "false", string oppositeInformationText = null) => string.Format(ReadManifestResourceContent(@"Enterprise.Customs.CH.Business.Test.Messaging.TestFiles.Input.NE096.xml"), correlationId, gdrnNumber, gdrnVersion, ToXsDateTimeNoOffset(issueDateTimeUTC ?? new ZDateTime(2023, 06, 01, 12, 42, 00)), decision, initiatedByCustoms, oppositeInformationText ?? CustomsMessageHelper.PartnerTopic);
	public static string GetNE131(string gdrnNumber = "21CHA6F0XKQGN8AMN0", string gdrnVersion = "1", string gdrnState = "201", string decision = "ACCEPTED", string oppositeInformationText = null) => string.Format(ReadManifestResourceContent(@"Enterprise.Customs.CH.Business.Test.Messaging.TestFiles.Input.NE131.xml"), gdrnNumber, gdrnVersion, gdrnState, decision, oppositeInformationText ?? CustomsMessageHelper.PartnerTopic);
	public static string GetNE021(string correlationId = "2087840A-90ED-4954-AC3A-AF7E1CB9935F") => string.Format(ReadManifestResourceContent(@"Enterprise.Customs.CH.Business.Test.Messaging.TestFiles.Input.NE021.xml"), correlationId);
	public static string GetNT004(string correlationId = null, string decision = null, string mrn = "20CH123456789012P2", string mrnVersion = "1", ZDate? activationDeadline = null, bool initiatedByCustoms = false, ZDateTime? decisionDateAndTime = null) => string.Format(ReadManifestResourceContent(@"Enterprise.Customs.CH.Business.Test.Messaging.TestFiles.Input.NT004.xml"), correlationId ?? "rdGacBQcFWFCwW", decision ?? "REJECTED", mrn, mrnVersion, ToXsDate(activationDeadline ?? new ZDate(2021, 11, 6)), ToXsBoolean(initiatedByCustoms), ToXsDateTimeNoOffset(decisionDateAndTime ?? new ZDateTime(2021, 10, 6, 17, 19, 57)));
	public static string GetNT008(string correlationId = "0091444b-8599-4333-b1f6-9a3b105775d9", DateTime? decisionDateAndTime = null, string decision = "ACCEPTED", string arrivalReferenceNumber = "220430-GTAN-1D5F2", string oppositeInformationText = null) => string.Format(ReadManifestResourceContent(@"Enterprise.Customs.CH.Business.Test.Messaging.TestFiles.Input.NT008.xml"), correlationId, ToXsDateTimeNoOffset(decisionDateAndTime ?? ZDateTime.Now), decision, arrivalReferenceNumber, oppositeInformationText ?? CustomsMessageHelper.PartnerTopic);
	public static string GetNT009(string correlationId = "0091444b-8599-4333-b1f6-9a3b105775d9", string mrn = "21CHODUG284YTOACN8", string mrnVersion = "1", string decision = "REJECTED", string initiatedByCustoms = "false", string oppositeInformationText = null) => string.Format(ReadManifestResourceContent(@"Enterprise.Customs.CH.Business.Test.Messaging.TestFiles.Input.NT009.xml"), correlationId, mrn, mrnVersion, decision, initiatedByCustoms, oppositeInformationText ?? CustomsMessageHelper.PartnerTopic);
	public static string GetNT019(string mrn = "00AA00000000000000", string mrnVersion = "1", string oppositeInformationText = null) => string.Format(ReadManifestResourceContent(@"Enterprise.Customs.CH.Business.Test.Messaging.TestFiles.Input.NT019.xml"), mrn, mrnVersion, oppositeInformationText ?? CustomsMessageHelper.PartnerTopic);
	public static string GetNT021(string correlationId = "A70BFA8B-A43F-4FCC-9E75-9596C0D81F1F", string oppositeInformationText = null) => string.Format(ReadManifestResourceContent(@"Enterprise.Customs.CH.Business.Test.Messaging.TestFiles.Input.NT021.xml"), correlationId, oppositeInformationText ?? CustomsMessageHelper.PartnerTopic);
	public static string GetNT025(string mrn = "22CHVL2525YYN7IZJ7", string mrnVersion = "1", string releaseIndicator = "FULL_RELEASE", ZDate? releaseDate = null, string oppositeInformationText = null) => string.Format(ReadManifestResourceContent(@"Enterprise.Customs.CH.Business.Test.Messaging.TestFiles.Input.NT025.xml"), mrn, mrnVersion, releaseIndicator, ToXsDate(releaseDate ?? new ZDate(2022, 2, 14)), oppositeInformationText ?? CustomsMessageHelper.PartnerTopic);
	public static string GetNT028(string correlationId = "0091444b-8599-4333-b1f6-9a3b105775d9", string mrnNumber = "22CHVL2525YYN7IZJ7", string mrnVersion = "1", string decision = "ACCEPTED", string oppositeInformationText = null) => string.Format(ReadManifestResourceContent(@"Enterprise.Customs.CH.Business.Test.Messaging.TestFiles.Input.NT028.xml"), correlationId, mrnNumber, mrnVersion, decision, oppositeInformationText ?? CustomsMessageHelper.PartnerTopic);
	public static string GetNT029(string correlationId = "0091444b-8599-4333-b1f6-9a3b105775d9", string mrn = "00AA00000000000000", string mrnVersion = "1", ZDateTime? preparationDateAndTime = null, string oppositeInformationText = null) => string.Format(ReadManifestResourceContent(@"Enterprise.Customs.CH.Business.Test.Messaging.TestFiles.Input.NT029.xml"), correlationId, mrn, mrnVersion, ToXsDateTimeNoOffset(preparationDateAndTime ?? ZDateTime.Now), oppositeInformationText ?? CustomsMessageHelper.PartnerTopic);
	public static string GetNT035(string correlationId = "0091444b-8599-4333-b1f6-9a3b105775d9", string mrn = "21CH16360164625756", string mrnVersion = "1", string oppositeInformationText = null) => string.Format(ReadManifestResourceContent(@"Enterprise.Customs.CH.Business.Test.Messaging.TestFiles.Input.NT035.xml"), correlationId, mrn, mrnVersion, oppositeInformationText ?? CustomsMessageHelper.PartnerTopic);
	public static string GetNT043V4(string mrn = "22DE16137320157570", string messageIdentification = "dNGMtGospbQtRtCxZyRsHWIBJafPOIXDniN", string oppositeInformationText = null) => string.Format(ReadManifestResourceContent(@"Enterprise.Customs.CH.Business.Test.Messaging.TestFiles.Input.NT043V4.xml"), messageIdentification, mrn, oppositeInformationText ?? CustomsMessageHelper.PartnerTopic);
	public static string GetNT043V5(string mrn = "22DE16137320157570", string messageIdentification = "dNGMtGospbQtRtCxZyRsHWIBJafPOIXDniN", string oppositeInformationText = null) => string.Format(ReadManifestResourceContent(@"Enterprise.Customs.CH.Business.Test.Messaging.TestFiles.Input.NT043V5.xml"), messageIdentification, mrn, oppositeInformationText ?? CustomsMessageHelper.PartnerTopic);
	public static string GetNT045(string correlationId = "0091444b-8599-4333-b1f6-9a3b105775d9", string mrn = "20DE16137020157570", string mrnVersion = "1") => string.Format(ReadManifestResourceContent(@"Enterprise.Customs.CH.Business.Test.Messaging.TestFiles.Input.NT045.xml"), correlationId, mrn, mrnVersion);
	public static string GetNT055(string correlationId = "00441D8F-FDE7-4BBA-B064-A9159C3EE7E1", string mrn = "00AA00000000000000", string mrnVersion = "1", string oppositeInformationText = null) => string.Format(ReadManifestResourceContent(@"Enterprise.Customs.CH.Business.Test.Messaging.TestFiles.Input.NT055.xml"), correlationId, mrn, mrnVersion, oppositeInformationText ?? CustomsMessageHelper.PartnerTopic);
	public static string GetNT057(string correlationId = "E90CDD5F-382A-4F59-8C02-CE239D890456", string decision = "REJECTED", string oppositeInformationText = null) => string.Format(ReadManifestResourceContent(@"Enterprise.Customs.CH.Business.Test.Messaging.TestFiles.Input.NT057.xml"), correlationId, decision, oppositeInformationText ?? CustomsMessageHelper.PartnerTopic);
	public static string GetNT060(string correlationId = "00441D8F-FDE7-4BBA-B064-A9159C3EE7E1", string mrn = "00AA00000000000000", string mrnVersion = "1", string oppositeInformationText = null) => string.Format(ReadManifestResourceContent(@"Enterprise.Customs.CH.Business.Test.Messaging.TestFiles.Input.NT060.xml"), correlationId, mrn, mrnVersion, oppositeInformationText ?? CustomsMessageHelper.PartnerTopic);
	public static string GetNT061(string correlationId = "0091444b-8599-4333-b1f6-9a3b105775d9", string referenceNumber = "dossier01", string arrivalReferenceNumber = "ARN123", string inspectionDecision = "CLEAR", string selectionStatus = "FINAL", string oppositeInformationText = null) => string.Format(ReadManifestResourceContent(@"Enterprise.Customs.CH.Business.Test.Messaging.TestFiles.Input.NT061.xml"), correlationId, referenceNumber, arrivalReferenceNumber, inspectionDecision, selectionStatus, oppositeInformationText ?? CustomsMessageHelper.PartnerTopic);
	public static string GetNT140(string correlationId = "26224602-6FB4-45DB-9E7A-D0048B7AD05E", string mrn = "00AA00000000000000", string mrnVersion = "1", string messageIdentification = "token", string oppositeInformationText = null) => string.Format(ReadManifestResourceContent(@"Enterprise.Customs.CH.Business.Test.Messaging.TestFiles.Input.NT140.xml"), correlationId, mrn, mrnVersion, messageIdentification, oppositeInformationText ?? CustomsMessageHelper.PartnerTopic);
	public static string GetNT146(string correlationId = "507710A8-9F8C-4FB6-8251-77C6A74BBFCA", string decision = "REJECTED", string oppositeInformationText = null) => string.Format(ReadManifestResourceContent(@"Enterprise.Customs.CH.Business.Test.Messaging.TestFiles.Input.NT146.xml"), correlationId, decision, oppositeInformationText ?? CustomsMessageHelper.PartnerTopic);
	public static string GetNT182(string correlationId = "0091444b-8599-4333-b1f6-9a3b105775d9", string mrn = "20DE16137020157570", string mrnVersion = "1") => string.Format(ReadManifestResourceContent(@"Enterprise.Customs.CH.Business.Test.Messaging.TestFiles.Input.NT182.xml"), correlationId, mrn, mrnVersion);
	public static string GetNT504(string correlationId = null, string mrn = "00AA00000000000000", string mrnVersion = "1", string decision = null, ZDateTime? decisionDateAndTime = null, ZDate? activationDeadline = null, bool initiatedByCustoms = false, string oppositeInformationText = null) => string.Format(ReadManifestResourceContent(@"Enterprise.Customs.CH.Business.Test.Messaging.TestFiles.Input.NT504.xml"), correlationId ?? "A4C6DDBC-FF84-4C3F-B46C-56BCDFD13395", mrn, mrnVersion, decision ?? "REJECTED", ToXsDateTimeNoOffset(decisionDateAndTime ?? new ZDateTime(2021, 11, 29, 3, 49, 45)), ToXsDate(activationDeadline ?? new ZDate(2021, 11, 19)), ToXsBoolean(initiatedByCustoms), oppositeInformationText ?? CustomsMessageHelper.PartnerTopic);
	public static string GetNT528(string correlationId = "0091444b-8599-4333-b1f6-9a3b105775d9", string mrnNumber = "22CHVL2525YYN7IZJ7", string mrnVersion = "1", string decision = "ACCEPTED", string oppositeInformationText = null) => string.Format(ReadManifestResourceContent(@"Enterprise.Customs.CH.Business.Test.Messaging.TestFiles.Input.NT528.xml"), correlationId, mrnNumber, mrnVersion, decision, oppositeInformationText ?? CustomsMessageHelper.PartnerTopic);
	public static string NT004 => ReadManifestResourceContent(@"Enterprise.Customs.CH.Business.Test.Messaging.TestFiles.Input.NT004.xml");
	public static string NT019 => ReadManifestResourceContent(@"Enterprise.Customs.CH.Business.Test.Messaging.TestFiles.Input.NT019.xml");
	public static string NT045 => ReadManifestResourceContent(@"Enterprise.Customs.CH.Business.Test.Messaging.TestFiles.Input.NT045.xml");
	public static string NT057 => ReadManifestResourceContent(@"Enterprise.Customs.CH.Business.Test.Messaging.TestFiles.Input.NT057.xml");
	public static string CharteraDocument => ReadManifestResourceContent(@"Enterprise.Customs.CH.Business.Test.Messaging.TestFiles.Input.CharteraDocument.xml");
	public static string CharteraSearchResult => ReadManifestResourceContent(@"Enterprise.Customs.CH.Business.Test.Messaging.TestFiles.Input.CharteraSearchResult.xml");
	public static string CharteraError => ReadManifestResourceContent(@"Enterprise.Customs.CH.Business.Test.Messaging.TestFiles.Input.CharteraError.xml");
	public static string EdecBordereauTypeBordereau => ReadManifestResourceContent(@"Enterprise.Customs.CH.Business.Test.Messaging.TestFiles.Input.EdecBordereauTypeBordereau.txt");
	public static string EdecBordereauTypeBordereauList => ReadManifestResourceContent(@"Enterprise.Customs.CH.Business.Test.Messaging.TestFiles.Input.EdecBordereauTypeBordereauList.txt");
	public static string EdecBordereauTypeBordereauListXml(string bordereauNumber1 = "11", string bordereauNumber2 = "12", string processingCenterNumber1 = "72", string processingCenterNumber2 = "72", ZDate? creationDate1 = null, ZDate? creationDate2 = null) => string.Format(ReadManifestResourceContent(@"Enterprise.Customs.CH.Business.Test.Messaging.TestFiles.Input.EdecBordereauTypeBordereauList.xml"), bordereauNumber1, bordereauNumber2, processingCenterNumber1, processingCenterNumber2, ToXsDate(creationDate1 ?? new ZDate(2010, 6, 16)), ToXsDate(creationDate2 ?? new ZDate(2010, 6, 17)));
	public static string EdecBordereauTypeBordereauXml(string documentNumber = "11", ZDate? documentDate = null, string customsOfficeNumber = "CH001251") => string.Format(ReadManifestResourceContent("Enterprise.Customs.CH.Business.Test.Messaging.TestFiles.Input.EdecBordereauTypeBordereau.xml"), documentNumber, ToXsDate(documentDate ?? new ZDate(2010, 6, 16)), customsOfficeNumber);
	public static string EdecBordereauTypeBordereauRequestRejectionRuleError => ReadManifestResourceContent(@"Enterprise.Customs.CH.Business.Test.Messaging.TestFiles.Input.EdecBordereauTypeBordereauRequestRejectionRuleError.txt");
	public static string EdecBordereauTypeBordereauRequestRejectionXMLSchemaError => ReadManifestResourceContent(@"Enterprise.Customs.CH.Business.Test.Messaging.TestFiles.Input.EdecBordereauTypeBordereauRequestRejectionXMLSchemaError.txt");
	public static string EdecBordereauUnsupportedSchema => ReadManifestResourceContent(@"Enterprise.Customs.CH.Business.Test.Messaging.TestFiles.Input.EdecBordereauUnsupportedSchema.txt");

	static string ToXsDateTimeNoOffset(ZDateTime dateTime) => dateTime.ToString("yyyy-MM-dd'T'HH:mm:ss");
	static string ToXsDate(ZDate dateTime) => dateTime.ToString("yyyy-MM-dd");
	static string ToXsBoolean(bool value) => value ? "true" : "false";
}
