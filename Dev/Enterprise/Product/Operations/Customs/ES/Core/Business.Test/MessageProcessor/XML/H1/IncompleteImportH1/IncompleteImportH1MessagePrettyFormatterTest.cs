using System;
using System.Collections.ObjectModel;
using CargoWise.Customs.ES.MessageDefinitions.Version1.H1.ES_CC415R;
using CargoWise.Customs.ES.MessageDefinitions.Version1.H1.PDI400V1Sal;
using CargoWise.Customs.ES.MessageDefinitions.Version1.H1.TD11;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;

namespace Enterprise.Customs.ES.Business.Testing;

sealed class IncompleteImportH1MessagePrettyFormatterTest : ImportH1CommonMessagePrettyFormatterTest<IncompleteImportH1MessagePrettyFormatter, Pdi400V1Sal>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Null entry header", () => new IncompleteImportH1MessagePrettyFormatter(new Pdi400V1Sal(), null));
	}

	public void TestDescription()
	{
		var expectedDescription10 = "<br><table border=\"0\"><tr><td>Description:</td><td>&nbsp;&nbsp;</td><td>10 - Accepted PDI</td></tr></table>";
		var expectedDescription11 = "<br><table border=\"0\"><tr><td>Description:</td><td>&nbsp;&nbsp;</td><td>11 - Amended PDI</td></tr></table>";

		CombineAssertions(() =>
		{
			var declarationResponse = SetResponseData("CSV", new Collection<GoodsItemTypeD01>(), new Collection<NotificationTypeD>(), operation: "10");
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
			AssertContains("Test Description with value 10", expectedDescription10, messageInterpretationText);

			declarationResponse = SetResponseData("CSV", new Collection<GoodsItemTypeD01>(), new Collection<NotificationTypeD>(), operation: "11");
			messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
			AssertContains("Test Description with value 11", expectedDescription11, messageInterpretationText);
		});
	}

	public void TestDomain()
	{
		var expectedDomain10 = "<table border=\"0\"><tr><td>Domain:</td><td>&nbsp;&nbsp;</td><td>10 - No CCI, no national centralized</td></tr></table>";
		var expectedDomain11 = "<table border=\"0\"><tr><td>Domain:</td><td>&nbsp;&nbsp;</td><td>11 - No CCI, national centralized</td></tr></table>";
		var expectedDomain20 = "<table border=\"0\"><tr><td>Domain:</td><td>&nbsp;&nbsp;</td><td>20 - CCI</td></tr></table>";

		CombineAssertions(() =>
		{
			var declarationResponse = SetResponseData("CSV", new Collection<GoodsItemTypeD01>(), new Collection<NotificationTypeD>(), domain: "10");
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
			AssertContains("Test Domain with value 10", expectedDomain10, messageInterpretationText);

			declarationResponse = SetResponseData("CSV", new Collection<GoodsItemTypeD01>(), new Collection<NotificationTypeD>(), domain: "11");
			messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
			AssertContains("Test Domain with value 11", expectedDomain11, messageInterpretationText);

			declarationResponse = SetResponseData("CSV", new Collection<GoodsItemTypeD01>(), new Collection<NotificationTypeD>(), domain: "20");
			messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
			AssertContains("Test Domain with value 20", expectedDomain20, messageInterpretationText);
		});
	}

	public void TestAcceptance()
	{
		var expectedAcceptance = "<br><table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>13-08-2024, 17:41:57</td></tr></table>";

		CombineAssertions(() =>
		{
			var declarationResponse = SetResponseData("CSV", new Collection<GoodsItemTypeD01>(), new Collection<NotificationTypeD>(), registrationDate: ZString.Empty);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
			AssertNotContains("Test Acceptance not included if it's not in the response", expectedAcceptance, messageInterpretationText);

			declarationResponse = SetResponseData("CSV", new Collection<GoodsItemTypeD01>(), new Collection<NotificationTypeD>(), registrationDate: "2024-08-13T17:41:57");
			messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
			AssertContains("Test Acceptance included if it's in the response", expectedAcceptance, messageInterpretationText);
		});
	}

	public void TestCustomsRegistrationNumber()
	{
		var expectedCustomsRegistrationNumber = "<br><table border=\"0\"><tr><td>Customs Registration Number:</td><td>&nbsp;&nbsp;</td><td>25ES00999912345678</td></tr></table>";

		CombineAssertions(() =>
		{
			var declarationResponse = SetResponseData("CSV", new Collection<GoodsItemTypeD01>(), new Collection<NotificationTypeD>(), mrn: ZString.Empty, registrationNumber: ZString.Empty);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
			AssertNotContains("Test Customs Registration Number not included if it's not in the response", expectedCustomsRegistrationNumber, messageInterpretationText);

			declarationResponse = SetResponseData("CSV", new Collection<GoodsItemTypeD01>(), new Collection<NotificationTypeD>(), mrn: "25ES00999912345678");
			messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
			AssertContains("Test Customs Registration Number included if it's in the response", expectedCustomsRegistrationNumber, messageInterpretationText);

			declarationResponse = SetResponseData("CSV", new Collection<GoodsItemTypeD01>(), new Collection<NotificationTypeD>(), registrationNumber: "25ES00999912345678");
			messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
			AssertContains("Test Customs Registration Number included if it's in the response", expectedCustomsRegistrationNumber, messageInterpretationText);
		});
	}

	public void TestRequiredDocuments()
	{
		Factory.AddDocumentsToRefDataForTest();

		var expectedRequiredDocuments = "<br><H3>Required Certificates</H3>" +
			"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\"><tr><td><strong>Item</strong></td><td><strong>Measure</strong></td><td><strong>Agency</strong></td><td><strong>Documents</strong></td></tr>" +
			"<tr><td>1</td><td>SNM</td><td>SIF05 Sanidad Exterior - Mº Sanidad</td>" +
				"<td><table width=\"100%\">" +
					"<tr><td>N851 - CERTIFICADO FITOSANITARIO</td></tr>" +
					"<tr><td>C085 - DOCUMENTO SANITARIO COMUN PARA VEGETALES[A]</td></tr>" +
				"</table></td></tr>" +
			"</table>";

		var measure = SetMeasure("SNM", new Collection<DocumentRequiredTypeD>() { SetDocument("N851"), SetDocument("C085") });
		var pcaCertificate = SetCertificate("SIF05", "Sanidad Exterior - Mº Sanidad", new Collection<TaricMeasureTypeD>() { measure });

		var goodItem = new GoodsItemTypeD01()
		{
			DeclarationGoodsItemNumber = "1",
			Pca = new Collection<PcaTypeD>() { pcaCertificate }
		};

		CombineAssertions(() =>
		{
			var declarationResponse = SetResponseData("CSV", new Collection<GoodsItemTypeD01>(), new Collection<NotificationTypeD>());
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
			AssertNotContains("Test Required Documents not included if it's not in the response", expectedRequiredDocuments, messageInterpretationText);

			declarationResponse = SetResponseData("CSV", new Collection<GoodsItemTypeD01>() { goodItem }, new Collection<NotificationTypeD>());
			messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
			AssertContains("Test Required Documents included if it's in the response", expectedRequiredDocuments, messageInterpretationText);
		});
	}

	public void TestNotifications()
	{
		var expectedNotifications = "<br><H3>Notifications</H3>" +
			"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\"><tr><td><strong>Code</strong></td><td><strong>Text</strong></td></tr>" +
			"<tr><td>1</td><td>Text1</td></tr>" +
			"<tr><td>2</td><td>Text2</td></tr>" +
			"</table>";

		CombineAssertions(() =>
		{
			var declarationResponse = SetResponseData("CSV", new Collection<GoodsItemTypeD01>(), new Collection<NotificationTypeD>());
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
			AssertNotContains("Test Notifications not included if it's not in the response", expectedNotifications, messageInterpretationText);

			declarationResponse = SetResponseData("CSV", new Collection<GoodsItemTypeD01>(), new Collection<NotificationTypeD>() { SetNotification("1", "Text1"), SetNotification("2", "Text2") });
			messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
			AssertContains("Test Notifications included if it's in the response", expectedNotifications, messageInterpretationText);
		});
	}

	Pdi400V1Sal SetResponseData(ZString declarationCSV, Collection<GoodsItemTypeD01> goodsItems, Collection<NotificationTypeD> notifications, string operation = "", string domain = "", string registrationDate = "", string mrn = "", string registrationNumber = "")
	{
		var response = new Pdi400V1Sal();

		var importOperation = new MCciOperationTypeD01()
		{
			Domain = domain,
		};

		if (mrn != ZString.Empty)
		{
			importOperation.Mrn = mrn;
		}

		if (registrationNumber != ZString.Empty)
		{
			importOperation.CustomsRegistrationNumber = registrationNumber;
		}

		response.Cc415R = new Cc415RTypeD()
		{
			Ack = new Ack415TypeD()
			{
				EdeclarationCsvId = declarationCSV,
				OperationRegistered = operation,
				GoodsItem = goodsItems,
				Notification = notifications,
				ImportOperation = importOperation
			},
			Cc426R = new Cc426RTypeD()
			{
				DeclarationRegistrationDateAndTime = registrationDate
			},
		};

		return response;
	}

	PcaTypeD SetCertificate(ZString pcaCode, ZString pcaName, Collection<TaricMeasureTypeD> measures)
	{
		var certificate = new PcaTypeD();
		certificate.Code = pcaCode;
		certificate.Name = pcaName;
		certificate.TaricMeasure = measures;
		return certificate;
	}

	TaricMeasureTypeD SetMeasure(ZString measure, Collection<DocumentRequiredTypeD> documents)
	{
		return new TaricMeasureTypeD()
		{
			TaricMeasureCode = measure,
			DocumentRequired = documents
		};
	}

	DocumentRequiredTypeD SetDocument(ZString document)
	{
		return new DocumentRequiredTypeD()
		{
			Type = document,
		};
	}

	NotificationTypeD SetNotification(ZString code, ZString text)
	{
		return new NotificationTypeD()
		{
			Code = code,
			Text = text,
		};
	}

	protected override void SetUp()
	{
		base.SetUp();

		entryHeader = Factory.NewWithValidTestData<CusEntryHeader>();
	}
	CusEntryHeader entryHeader;

	protected override IncompleteImportH1MessagePrettyFormatter GetNewPrettyFormatter(Pdi400V1Sal response) => new IncompleteImportH1MessagePrettyFormatter(response, entryHeader);

	protected override Pdi400V1Sal ResponseData()
	{
		Factory.AddDocumentsToRefDataForTest();

		var measure1 = SetMeasure("SNM", new Collection<DocumentRequiredTypeD>() { SetDocument("N851"), SetDocument("C085") });
		var measure2 = SetMeasure("VIM", new Collection<DocumentRequiredTypeD>() { SetDocument("N853"), SetDocument("C657") });
		var pcaCertificate1 = SetCertificate("SIF05", "Sanidad Exterior - Mº Sanidad", new Collection<TaricMeasureTypeD>() { measure1, measure2 });

		var measure3 = SetMeasure("SVI", new Collection<DocumentRequiredTypeD>() { SetDocument("C678"), SetDocument("1405") });
		var pcaCertificate2 = SetCertificate("SIF02", "Sanidad Animal, M Agricultura", new Collection<TaricMeasureTypeD>() { measure3 });

		var measure4 = SetMeasure("MIV", new Collection<DocumentRequiredTypeD>() { SetDocument("1413") });
		var pcaCertificate3 = SetCertificate("SIF06", "Sanidad Interior - Mº Sanidad", new Collection<TaricMeasureTypeD>() { measure4 });

		var goodItem1 = new GoodsItemTypeD01()
		{
			DeclarationGoodsItemNumber = "1",
			Pca = new Collection<PcaTypeD>() { pcaCertificate1, pcaCertificate2 }
		};

		var goodItem2 = new GoodsItemTypeD01()
		{
			DeclarationGoodsItemNumber = "2",
			Pca = new Collection<PcaTypeD>() { pcaCertificate3 }
		};

		var goodItems = new Collection<GoodsItemTypeD01>() { goodItem1, goodItem2 };
		var notifications = new Collection<NotificationTypeD>() { SetNotification("1", "Text1"), SetNotification("2", "Text2") };

		return SetResponseData("ABCDEFGHIJKLMNOP", goodItems, notifications, "10", "10", "2024-08-13T17:41:57", "25ES00999912345678");
	}

	protected override Pdi400V1Sal ErrorResponseData(bool isFunctionalError)
	{
		var declarationResponse = ResponseData();
		if (isFunctionalError)
		{
			declarationResponse.Cc456A = SetErrorResponseData();
		}
		else
		{
			declarationResponse.Cd917A = SetXMLErrorResponseData();
		}

		return declarationResponse;
	}

	protected override ZString ExpectedMessageDetailsAccepted => "<H3>Accepted Declaration</H3>" +
		"<br><table border=\"0\"><tr><td>Description:</td><td>&nbsp;&nbsp;</td><td>10 - Accepted PDI</td></tr></table>" +
		"<table border=\"0\"><tr><td>Domain:</td><td>&nbsp;&nbsp;</td><td>10 - No CCI, no national centralized</td></tr></table>" +
		"<br><table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>13-08-2024, 17:41:57</td></tr></table>" +
		"<br><table border=\"0\"><tr><td>Customs Registration Number:</td><td>&nbsp;&nbsp;</td><td>25ES00999912345678</td></tr></table>" +
		"<br><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>ABCDEFGHIJKLMNOP</td></tr></table>" +
		"<br><H3>Required Certificates</H3>" +
			"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\"><tr><td><strong>Item</strong></td><td><strong>Measure</strong></td><td><strong>Agency</strong></td><td><strong>Documents</strong></td></tr>" +
			"<tr><td>1</td><td>SNM</td><td>SIF05 Sanidad Exterior - Mº Sanidad</td>" +
				"<td><table width=\"100%\">" +
					"<tr><td>N851 - CERTIFICADO FITOSANITARIO</td></tr>" +
					"<tr><td>C085 - DOCUMENTO SANITARIO COMUN PARA VEGETALES[A]</td></tr>" +
				"</table></td></tr>" +
			"<tr><td>1</td><td>VIM</td><td>SIF05 Sanidad Exterior - Mº Sanidad</td>" +
				"<td><table width=\"100%\">" +
					"<tr><td>N853 - DOC.SANIT.COMUN ENTRADA PRODUCTOS(B)</td></tr>" +
					"<tr><td>C657 - CERTIFICADO DE SANIDAD</td></tr>" +
				"</table></td></tr>" +
			"<tr><td>1</td><td>SVI</td><td>SIF02 Sanidad Animal, M Agricultura</td>" +
				"<td><table width=\"100%\">" +
					"<tr><td>C678 - DOC.SANIT.COMUN PIENSOS+ALIMENTOS</td></tr>" +
					"<tr><td>1405 - INSPECCION SANIDAD EXTERIOR. NO PROCEDE</td></tr>" +
				"</table></td></tr>" +
			"<tr><td>2</td><td>MIV</td><td>SIF06 Sanidad Interior - Mº Sanidad</td>" +
				"<td><table width=\"100%\">" +
					"<tr><td>1413 - INSPECCION SANIDAD EXTERIOR-NO AFECTADOS</td></tr>" +
				"</table></td></tr>" +
			"</table>" +
			"<br><H3>Notifications</H3>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\"><tr><td><strong>Code</strong></td><td><strong>Text</strong></td></tr>" +
				"<tr><td>1</td><td>Text1</td></tr>" +
				"<tr><td>2</td><td>Text2</td></tr>" +
				"</table>";
}
