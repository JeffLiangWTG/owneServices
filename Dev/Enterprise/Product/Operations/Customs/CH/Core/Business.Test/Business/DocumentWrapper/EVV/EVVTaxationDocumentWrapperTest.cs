using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(EVVTaxationDocumentWrapper))]
public class EVVTaxationDocumentWrapperTest : NonPersistentBusinessObjectTestCase
{
	public void TestNew_ShouldReturnInstance_WhenMessageIsValid()
	{
		var entryHeader = Factory.New<CusEntryHeader>();
		var message = (CHEDIMessage)entryHeader.Messages.AddNew();
		message.EM_MessageText = TestingData.InputEvvResponseVATValidSignature;

		var documentWrapper = EVVTaxationDocumentWrapper.New(message, Factory);

		AssertNotNull(documentWrapper);
	}

	public void TestProperties_DTY()
	{
		var document = CreateEVVDocumentWrapper(TestingData.InputEvvResponseAllDTY());

		CombineAssertions(() =>
		{
			AssertEquals("Schema Version", "3.0", document.SchemaVersion);
			AssertEquals("Requestor Trader Identification Number", "CHE326684996", document.RequestorTraderIdentificationNumber);
			AssertEquals("Document Type", "taxationDecisionCustomsDuties", document.DocumentType);
			AssertEquals("Document Title", "VERANLAGUNGSVERFÜGUNG ZOLL", document.DocumentTitle);
			AssertEquals("Document Number", "22CHEI000043143375", document.DocumentNumber);
			AssertEquals("Document Version", "1", document.DocumentVersion);
			AssertEquals("Document Date", new ZDateTime(2022, 6, 21, 23, 55, 0), document.DocumentDateTime);
			AssertEquals("DeclarationType", "1", document.DeclarationType);
			AssertEquals("Acceptance Date", new ZDateTime(2022, 6, 20, 15, 55, 55), document.AcceptanceDateTime);
			AssertEquals("Customs Office Number", "CH001251", document.CustomsOfficeNumber);
			AssertEquals("Customs Office Name", "Zoll Nord - Basel Mitte EVO", document.CustomsOfficeName);
			AssertEquals("Customs Office Street", "Wiesendamm 4, Postfach 133", document.CustomsOfficeStreet);
			AssertEquals("Customs Office Country", "CH", document.CustomsOfficeCountry);
			AssertEquals("Customs Office City", "BASEL", document.CustomsOfficeCity);
			AssertEquals("Customs Office Phone Number", ZString.Empty, document.CustomsOfficePhoneNumber);
			AssertEquals("Customs Office Address Supplement 1", ZString.Empty, document.CustomsOfficeAddressSupplement1);
			AssertEquals("Customs Office Address Supplement 2", ZString.Empty, document.CustomsOfficeAddressSupplement2);

			AssertEquals("TraderDeclarationNumber", "HYESASCM2000000005", document.TraderDeclarationNumber);
			AssertEquals("TraderReference", "TEST", document.TraderReference);
			AssertEquals("AccessCode", "cfQ04Nt36dDhHcD6", document.AccessCode);

			AssertEquals("Consignor address", "RHEIN-NADEL MASCHINENADEL", document.ConsignorAddress.Name);
			AssertEquals("Consignee address", "SBB", document.ConsigneeAddress.Name);
			AssertEquals("Consignee TIN/UID", "CHE105908410", document.ConsigneeTIN);
			AssertEquals("Importer address", "SISA IMPORT EXPORT", document.ImporterAddress.Name);
			AssertEquals("Importer TIN/UID", "CHE105908410", document.ImporterTIN);
			AssertEquals("Declarant address", "SISA_RZ1-AS", document.DeclarantAddress.Name);
			AssertEquals("Declarant Number", "72", document.DeclarantNumber);
			AssertEquals("Declarant TIN/UID", "CHE326684996", document.DeclarantTIN);

			AssertEquals("BodereauNumber", "1734", document.BodereauNumber);
			AssertEquals("DispatchCountry", "DE", document.DispatchCountry);
			AssertEquals("NumberOfGoods", 1, document.NumberOfGoods);
			AssertEquals("AccountNumber", "31500", document.AccountNumber);
			AssertEquals("AccountName", "Sieber TransPo Berneck", document.AccountName);
			AssertEquals("Incoterms", "FOB", document.Incoterms);
			AssertEquals("TransportationCountry", "DE", document.TransportationCountry);
			AssertEquals("TransportationNumber", "123", document.TransportationNumber);
			AssertEquals("Containers", "CTR1, CTR2, CTR3", document.Containers);
			AssertEquals("SpecialMentions", "special mentions", document.SpecialMentions);
			AssertEquals("TotalAmount", 30.60m, document.TotalAmount);
		});
	}

	public void TestProperties_VAT() => CombineAssertions(() =>
	{
		var document = CreateEVVDocumentWrapper(TestingData.InputEvvResponseVAT());

		AssertEquals("Schema Version", "3.0", document.SchemaVersion);
		AssertEquals("Requestor Trader Identification Number", "CHE326684996", document.RequestorTraderIdentificationNumber);
		AssertEquals("Document Type", "taxationDecisionVAT", document.DocumentType);
		AssertEquals("Document Number", "22CHEI000043143375", document.DocumentNumber);
		AssertEquals("Document Version", "1", document.DocumentVersion);
		AssertEquals("Document Date", new ZDateTime(2022, 6, 21, 23, 55, 0), document.DocumentDateTime);

		AssertEquals("VATNumber", "CHE105908410", document.VATNumber);
	});

	public void TestVATSuffix() => CombineAssertions(() =>
	{
		AssertVATSuffix("1", "VAT");
		AssertVATSuffix("0", ZString.Empty);

		void AssertVATSuffix(string messageVATSuffix, string expectedVATSuffix)
		{
			var document = CreateEVVDocumentWrapper(TestingData.InputEvvResponseVAT(vatSuffix: messageVATSuffix));
			AssertEquals($"Message: VATSuffix={messageVATSuffix}", expectedVATSuffix, document.VATSuffix);
		}
	});

	public void TestInvoiceCurrencyType() => CombineAssertions(() =>
	{
		using var resCacheDE = Res.GetLanguageInstance("DE-DE").UseMockData();
		using var resCacheFR = Res.GetLanguageInstance("FR-FR").UseMockData();
		using var resCacheIT = Res.GetLanguageInstance("IT-IT").UseMockData();
		PutResString("EVVInvoiceCurrencyType|OtherCurrenciesOfTheEu", "Andere Währungen der EU", "Autres monnaies UE", "Altre monete UE");
		PutResString("EVVInvoiceCurrencyType|OtherCurrenciesEGGbp", "Andere Währungen (z.B. GBP)", "Autres monnaies (p.ex. GBP)", "Altre monete (ad es. GBP)");

		AssertInvoiceCurrencyType("1", "CHF", "CHF", "CHF", "CHF");
		AssertInvoiceCurrencyType("2", "EUR", "EUR", "EUR", "EUR");
		AssertInvoiceCurrencyType("3", "Other currencies of the EU", "Andere Währungen der EU", "Autres monnaies UE", "Altre monete UE");
		AssertInvoiceCurrencyType("4", "USD", "USD", "USD", "USD");
		AssertInvoiceCurrencyType("5", "Other currencies (e.g. GBP)", "Andere Währungen (z.B. GBP)", "Autres monnaies (p.ex. GBP)", "Altre monete (ad es. GBP)");

		void AssertInvoiceCurrencyType(string messageInvoiceCurrencyType, string expectedInvoiceCurrencyTypeEN, string expectedInvoiceCurrencyTypeDE, string expectedInvoiceCurrencyTypeFR, string expectedInvoiceCurrencyTypeIT)
		{
			var document = CreateEVVDocumentWrapper(TestingData.InputEvvResponseVAT(invoiceCurrencyType: messageInvoiceCurrencyType, documentLanguage: "en"));
			AssertEquals($"Message: InvoiceCurrenyType={messageInvoiceCurrencyType} DocumentLanguage=en", expectedInvoiceCurrencyTypeEN, document.InvoiceCurrencyType);
			document = CreateEVVDocumentWrapper(TestingData.InputEvvResponseVAT(invoiceCurrencyType: messageInvoiceCurrencyType, documentLanguage: "de"));
			AssertEquals($"Message: InvoiceCurrenyType={messageInvoiceCurrencyType} DocumentLanguage=de", expectedInvoiceCurrencyTypeDE, document.InvoiceCurrencyType);
			document = CreateEVVDocumentWrapper(TestingData.InputEvvResponseVAT(invoiceCurrencyType: messageInvoiceCurrencyType, documentLanguage: "fr"));
			AssertEquals($"Message: InvoiceCurrenyType={messageInvoiceCurrencyType} DocumentLanguage=fr ", expectedInvoiceCurrencyTypeFR, document.InvoiceCurrencyType);
			document = CreateEVVDocumentWrapper(TestingData.InputEvvResponseVAT(invoiceCurrencyType: messageInvoiceCurrencyType, documentLanguage: "it"));
			AssertEquals($"Message: InvoiceCurrenyType={messageInvoiceCurrencyType} DocumentLanguage=it", expectedInvoiceCurrencyTypeIT, document.InvoiceCurrencyType);
		}

		void PutResString(string key, string stringDE, string stringFR, string stringIT)
		{
			resCacheDE.PutString(key, stringDE);
			resCacheFR.PutString(key, stringFR);
			resCacheIT.PutString(key, stringIT);
		}
	});

	public void TestTransportMode() => CombineAssertions(() =>
	{
		using var resCache = Res.GetLanguageInstance("FR-FR").UseMockData();
		resCache.PutString("EVVTransportModeList|AirTraffic", "Air traffic France");

		AssertTransportMode("2", "Rail traffic");
		AssertTransportMode("3", "Road traffic");
		AssertTransportMode("4", "Air traffic");
		AssertTransportMode("5", "Postal traffic");
		AssertTransportMode("7", "Pipeline, etc.");
		AssertTransportMode("8", "Inland waterways");
		AssertTransportMode("9", "Self-propulsion");
		AssertTransportMode("1", ZString.Empty);

		AssertTransportMode("4", "Air traffic France", documentLanguage: SwissCustomsLanguageList.Codes.French);

		void AssertTransportMode(string transportModeCode, ZString expectedTransportMode, string documentLanguage = "en")
		{
			var messageText = Regex.Replace(TestingData.InputEvvResponseAllDTY(documentLanguage), @"<transportMode>\d+</transportMode>", $@"<transportMode>{transportModeCode}</transportMode>");
			var document = CreateEVVDocumentWrapper(messageText);
			AssertEquals($"transportMode={transportModeCode}", expectedTransportMode, document.TransportMode);
		}
	});

	public void TestTransportationType() => CombineAssertions(() =>
	{
		RefCusCodeTestHelper.CreateTransportationTypeList(Factory, language: Factory.GetDocumentLanguage(SwissCustomsLanguageList.Codes.French));

		var wrapper = CreateEVVDocumentWrapper(TestingData.InputEvvResponseAllDTY(documentLanguage: SwissCustomsLanguageList.Codes.French));
		AssertEquals("TransportationType", "Truck [FR]", wrapper.TransportationType);

		wrapper = CreateEVVDocumentWrapper(TestingData.InputEvvResponseAllDTY(transportationType: "88"));
		AssertEquals("Fallback for unknown code", "--", wrapper.TransportationType);

		var messageText = Regex.Replace(TestingData.InputEvvResponseAllDTY(), @"<transportationType>.*</transportationType>", string.Empty);
		wrapper = CreateEVVDocumentWrapper(messageText);
		AssertEquals("Fallback when element missing", "--", wrapper.TransportationType);
	});

	public void TestTransportationCountry()
	{
		var messageText = Regex.Replace(TestingData.InputEvvResponseAllDTY(), @"<transportationCountry>\w+</transportationCountry>", string.Empty);
		var document = CreateEVVDocumentWrapper(messageText);
		AssertEquals("No country provided", "--", document.TransportationCountry);
	}

	public void TestContainers() => CombineAssertions(() =>
	{
		var messageText = Regex.Replace(TestingData.InputEvvResponseAllDTY(), @"(?s)<container>.*</container>", @"<container><containerNumber>111</containerNumber></container>");
		var document = CreateEVVDocumentWrapper(messageText);
		AssertEquals("Single container", "111", document.Containers);

		messageText = Regex.Replace(TestingData.InputEvvResponseAllDTY(), @"(?s)<container>.*</container>", @"<container><containerNumber>111</containerNumber></container><container><containerNumber>222</containerNumber></container>");
		document = CreateEVVDocumentWrapper(messageText);
		AssertEquals("Multiple containers", "111, 222", document.Containers);

		messageText = Regex.Replace(TestingData.InputEvvResponseAllDTY(), @"(?s)<container>.*</container>", string.Empty);
		document = CreateEVVDocumentWrapper(messageText);
		AssertEquals("No containers", string.Empty, document.Containers);
	});

	public void TestSpecialMention()
	{
		var messageText = Regex.Replace(TestingData.InputEvvResponseAllDTY(), @"(?s)<specialMention>.*</specialMention>", @"<specialMention><text>Line1</text></specialMention><specialMention><text>Line2</text></specialMention>");
		var document = CreateEVVDocumentWrapper(messageText);
		AssertEquals("Multiple special mentions", new ZString("Line1\r\nLine2"), document.SpecialMentions);
	}

	public void TestPreviousDocuments() => CombineAssertions(() =>
	{
		var messageText = Regex.Replace(TestingData.InputEvvResponseAllDTY(), @"(?s)<previousDocument>.*</previousDocument>", @"
				<previousDocument>
					<previousDocumentType>doc1</previousDocumentType>
				</previousDocument>
				<previousDocument>
					<previousDocumentType>doc2</previousDocumentType>
				</previousDocument>");
		var document = CreateEVVDocumentWrapper(messageText);
		AssertEquals("Count", 2, document.PreviousDocuments.Count);
		AssertEquals("1st document", "doc1", ((EVVPreviousDocumentWrapper)document.PreviousDocuments[0]).Type);
		AssertEquals("2nd document", "doc2", ((EVVPreviousDocumentWrapper)document.PreviousDocuments[1]).Type);

		AssertSame("cached", document.PreviousDocuments, document.PreviousDocuments);

		messageText = Regex.Replace(TestingData.InputEvvResponseAllDTY(), @"(?s)<previousDocument>.*</previousDocument>", string.Empty);
		document = CreateEVVDocumentWrapper(messageText);
		AssertEquals("Count when empty", 0, document.PreviousDocuments.Count);
	});

	public void TestDuties() => CombineAssertions(() =>
	{
		var messageText = Regex.Replace(TestingData.InputEvvResponseAllDTY(), @"(?s)<detailAmount>.*</detailAmount>", @"
				<detailAmount>
					<type>100</type>
					<amount>10</amount>
					<roundedAmount>0</roundedAmount>
				</detailAmount>
				<detailAmount>
					<type>200</type>
					<amount>20</amount>
					<roundedAmount>0</roundedAmount>
				</detailAmount>");
		var document = CreateEVVDocumentWrapper(messageText);
		AssertEquals("Count", 2, document.Duties.Count);
		AssertEquals("1st duty", 10m, document.Duties[0].Amount);
		AssertEquals("2nd duty", 20m, document.Duties[1].Amount);

		AssertSame("cached", document.Duties, document.Duties);
	});

	public void TestGoodsItems() => CombineAssertions(() =>
	{
		const string goodsItemDetails = @"
					<description>X</description>
					<commodityCode>1234.5678</commodityCode>
					<grossMass>1</grossMass>
					<permitObligation>0</permitObligation>
					<nonCustomsLawObligation>0</nonCustomsLawObligation>
					<statistic>
						<customsClearanceType>1</customsClearanceType>
						<commercialGood>1</commercialGood>
						<statisticalValue>1</statisticalValue>
						<repair>0</repair>
					</statistic>
					<packaging>
						<packagingType>PK</packagingType>
					</packaging>";
		var messageText = Regex.Replace(TestingData.InputEvvResponseAllDTY(), @"(?s)<goodsItem>.*</goodsItem>", $@"
				<goodsItem>
					<customsItemNumber>11</customsItemNumber>
					{goodsItemDetails}
				</goodsItem>
				<goodsItem>
					<customsItemNumber>12</customsItemNumber>
					{goodsItemDetails}
				</goodsItem>");
		var document = CreateEVVDocumentWrapper(messageText);
		AssertEquals("Count", 2, document.GoodsItems.Count);
		AssertEquals("1st goods item", "11", document.GoodsItems[0].CustomsItemNumber);
		AssertEquals("2nd goods item", "12", document.GoodsItems[1].CustomsItemNumber);

		AssertSame("cached", document.GoodsItems, document.GoodsItems);
	});

	public void TestLegalAdvisories() => CombineAssertions(() =>
	{
		var document = CreateEVVDocumentWrapper(TestingData.InputEvvResponseAllDTY());
		var legalAdvisories = document.LegalAdvisories;

		AssertEquals("Legal Advisories Count", 3, legalAdvisories.Count);
		AssertEquals("Legal Advisories 1st", 1, legalAdvisories[0].SequenceNumber);
		AssertEquals("Legal Advisories 2nd", 2, legalAdvisories[1].SequenceNumber);
		AssertEquals("Legal Advisories 3rd", 3, legalAdvisories[2].SequenceNumber);
		AssertSame("Legal Advisories same", document.LegalAdvisories, document.LegalAdvisories);
	});

	public void TestWrappersCached() => CombineAssertions(() =>
	{
		var document = CreateEVVDocumentWrapper(TestingData.InputEvvResponseAllDTY());
		AssertSame("ConsigneeAddress", document.ConsigneeAddress, document.ConsigneeAddress);
		AssertSame("ConsignorAddress", document.ConsignorAddress, document.ConsignorAddress);
		AssertSame("DeclarantAddress", document.DeclarantAddress, document.DeclarantAddress);
		AssertSame("ImporterAddress", document.ImporterAddress, document.ImporterAddress);
	});

	public void TestDocumentLanguage() => CombineAssertions(() =>
	{
		AssertEquals("DE-DE", CreateEVVDocumentWrapper(TestingData.InputEvvResponseAllDTY(documentLanguage: SwissCustomsLanguageList.Codes.German)).DocumentLanguage);
		AssertEquals("FR-FR", CreateEVVDocumentWrapper(TestingData.InputEvvResponseAllDTY(documentLanguage: SwissCustomsLanguageList.Codes.French)).DocumentLanguage);
		AssertEquals("IT-IT", CreateEVVDocumentWrapper(TestingData.InputEvvResponseAllDTY(documentLanguage: SwissCustomsLanguageList.Codes.Italian)).DocumentLanguage);
	});

	public void TestEnsty() => CombineAssertions(() =>
	{
		RefCusCodeTestHelper.CreateEnstyCodeList(Factory);

		AssertEnsty("2", "Provisorisch");
		AssertEnsty("02", "Provisorisch");
		AssertEnsty("77", ZString.Empty);

		void AssertEnsty(string declarationType, ZString expectedEnsty)
		{
			var wrapper = CreateEVVDocumentWrapper(TestingData.InputEvvResponseAllDTY(declarationType: declarationType));
			AssertEquals($"declarationType={declarationType}", expectedEnsty, wrapper.Ensty);
		}
	});

	public void TestDocumentFilename() => CombineAssertions(() =>
	{
		var wrapper = CreateEVVDocumentWrapper(TestingData.InputEvvResponseAllDTY(documentNumber: "docNumber", documentVersion: "5", requestorTraderIdentificationNumber: "requestorTIN"));
		AssertEquals("e-dec_receiptResponse_receipt_taxationDecisionCustomsDuties_docNumber_5_requestorTIN", wrapper.DocumentFilename);

		wrapper = CreateEVVDocumentWrapper(TestingData.InputEvvResponseVAT(documentNumber: "docNumber", documentVersion: "5", requestorTraderIdentificationNumber: "requestorTIN"));
		AssertEquals("e-dec_receiptResponse_receipt_taxationDecisionVAT_docNumber_5_requestorTIN", wrapper.DocumentFilename);
	});

	protected override BusinessObject GetNewBusinessObject()
	{
		return CreateEVVDocumentWrapper(TestingData.InputEvvResponseVATValidSignature);
	}

	EVVTaxationDocumentWrapper CreateEVVDocumentWrapper(ZString messageText)
	{
		return EVVTaxationDocumentWrapper.New(CreateEDIMessage(messageText), Factory);
	}

	CHEDIMessage CreateEDIMessage(ZString messageText)
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var message = (CHEDIMessage)entryHeader.Messages.AddNew();
		message.EM_MessageText = messageText;
		message.EM_LinkedObject = entryHeader;
		message.EM_MessageType = MessageTypeCodeList.Codes.EVV;
		message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		return message;
	}
}

