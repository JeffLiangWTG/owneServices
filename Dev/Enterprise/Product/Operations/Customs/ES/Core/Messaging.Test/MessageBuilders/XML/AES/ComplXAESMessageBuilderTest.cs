using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.ES.MessageDefinitions.Version1.AES.ES_CC515X_v514.CC515XV1Ent;
using CargoWise.Types;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders.Testing;

[TestedType(typeof(ComplXAESMessageBuilder))]
public class ComplXAESMessageBuilderTest : AESCommonMessageBuilderTest<ComplXAESMessageBuilder, IComplXAESMessageDataProvider, Cc515Xv1Ent>
{
	#region Tests
	public override void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("When provider is null", () => CreateMessageBuilderWithNullProvider());
	}

	[TestDate(2020, 1, 9, 15, 13, 23, 456)]
	public override void TestCreateEDIMessage()
	{
		var messageBuilder = CreateMessageBuilder();

		CombineAssertions(() =>
		{
			AssertEquals("messageBuilder.MessageType", ExpectedMessageType, messageBuilder.MessageType);
			AssertEquals("messageBuilder.MessageSubType", ExpectedMessageSubType, messageBuilder.MessageSubType);
			AssertEquals("messageBuilder.Provider", mockProvider.Object, messageBuilder.Provider);
			AssertUnsignedMessageText(messageBuilder.UnsignedMessageText);
			AssertSignedMessageText(messageBuilder.GetSignedMessageText());
		});
	}

	public override void TestPopulatePhaseID()
	{
		mockProvider.Setup(m => m.PhaseIDSpecified).Returns(false);
		var messageText = CreateMessageBuilder().GetSignedMessageText();
		AssertNotContains("PhaseID", messageText);
	}

	public void TestPopulateExportOperation()
	{
		mockProvider.Setup(m => m.ExportOperation).Returns((IComplXAESExportOperation)null);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public void TestPopulateDeclarant()
	{
		mockProvider.Setup(m => m.Declarant).Returns((IPartyIdProvider)null);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public void TestPopulateRepresentative()
	{
		mockProvider.Setup(m => m.Representative).Returns((ICommonRepresentative)null);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public void TestPopulateGoodsShipment()
	{
		mockProvider.Setup(m => m.GoodsShipment).Returns((IComplXAESGoodsShipment)null);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public void TestPopulateDeliveryTerms()
	{
		mockProvider.Setup(m => m.GoodsShipment.DeliveryTerms).Returns((ICommonDeliveryTerms)null);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public void TestPopulateWarehouse()
	{
		mockProvider.Setup(m => m.GoodsShipment.Warehouse).Returns((IWarehouseCommon)null);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public void TestPopulateConsignment()
	{
		mockProvider.Setup(m => m.GoodsShipment.Consignment).Returns((IComplXAESConsignment)null);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public void TestPopulateActiveBorderTransportMeans()
	{
		mockProvider.Setup(m => m.GoodsShipment.Consignment.ActiveBorderTransportMeans).Returns((ITransportMediumInfoCommon)null);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public void TestPopulateTransportCharges()
	{
		mockConsignment.Setup(m => m.TransportChargesMoP).Returns(ZString.Empty);
		mockGoodsShipment.Setup(m => m.Consignment).Returns(mockConsignment.Object);
		mockProvider.Setup(m => m.GoodsShipment).Returns(mockGoodsShipment.Object);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public void TestPopulateLine()
	{
		mockGoodsShipment.Setup(m => m.Lines).Returns((IReadOnlyCollection<IComplXAESLine>)null);
		mockProvider.Setup(m => m.GoodsShipment).Returns(mockGoodsShipment.Object);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());

		var messageText = CreateMessageBuilder().GetSignedMessageText();
		var expectedResult = @$"</{XMLTestFileConstants.XmlElementNamespace}Consignment>
    </{XMLTestFileConstants.XmlElementNamespace}GoodsShipment>";

		CombineAssertions(() =>
		{
			AssertContains(expectedResult, messageText);
		});
	}

	public void TestPopulateOrigin()
	{
		mockLine1.Setup(m => m.Origin).Returns((IAESCommonOrigin)null);
		mockGoodsShipment.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });
		mockProvider.Setup(m => m.GoodsShipment).Returns(mockGoodsShipment.Object);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public void TestPopulateCommodity()
	{
		mockLine1.Setup(m => m.Commodity).Returns((IComplXAESCommodity)null);
		mockGoodsShipment.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });
		mockProvider.Setup(m => m.GoodsShipment).Returns(mockGoodsShipment.Object);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public void TestPopulateGoodsMeasure()
	{
		mockCommodity.Setup(m => m.GoodsMeasure).Returns((ICommonGoodsMeasureWithSpecified)null);
		mockLine1.Setup(m => m.Commodity).Returns(mockCommodity.Object);
		mockGoodsShipment.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });
		mockProvider.Setup(m => m.GoodsShipment).Returns(mockGoodsShipment.Object);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public void TestPopulatePreviousDocument()
	{
		mockLine1.Setup(m => m.PreviousDocuments).Returns((IReadOnlyCollection<IAESCommonDocument>)null);
		mockGoodsShipment.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });
		mockProvider.Setup(m => m.GoodsShipment).Returns(mockGoodsShipment.Object);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());

		var messageText = CreateMessageBuilder().GetSignedMessageText();
		var expectedResult = @$"</{XMLTestFileConstants.XmlElementNamespace}Commodity>
        <{XMLTestFileConstants.XmlElementNamespace}SupportingDocument>";

		CombineAssertions(() =>
		{
			AssertContains(expectedResult, messageText);
		});
	}

	public void TestPopulateSupportingDocument()
	{
		mockLine1.Setup(m => m.SupportingDocuments).Returns((IReadOnlyCollection<IComplXAESSupportingDocument>)null);
		mockGoodsShipment.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });
		mockProvider.Setup(m => m.GoodsShipment).Returns(mockGoodsShipment.Object);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());

		var messageText = CreateMessageBuilder().GetSignedMessageText();
		var expectedResult = @$"</{XMLTestFileConstants.XmlElementNamespace}PreviousDocument>
      </{XMLTestFileConstants.XmlElementNamespace}GoodsItem>";

		CombineAssertions(() =>
		{
			AssertContains(expectedResult, messageText);
		});
	}

	#endregion

	protected override ZString ExpectedMessageType => DeclarationMessageTypeList.Codes.TypeXExportUcc6;

	protected override ComplXAESMessageBuilder CreateMessageBuilder() => MockRandomGenerator(new ComplXAESMessageBuilder(mockProvider.Object, ExpectedMessageType, ExpectedMessageSubType));

	protected override ComplXAESMessageBuilder CreateMessageBuilderWithNullProvider() => new ComplXAESMessageBuilder(default, ExpectedMessageType, ExpectedMessageSubType);

	protected override ZString GetTestFile() => GetTestFileContents(XMLTestFileConstants.AESTestFilePath, "TestAESComplX.txt");

	#region Structures SetUp

	protected override void SetUp()
	{
		base.SetUp();

		mockProvider.Setup(m => m.Message).Returns(SetUpMessage().Object);
		mockProvider.Setup(m => m.IsFinalPeriod).Returns(true);
		mockProvider.Setup(m => m.PhaseIDSpecified).Returns(true);

		var mockExportOperation = SetUpExportOperation();
		mockProvider.Setup(m => m.ExportOperation).Returns(mockExportOperation.Object);

		var mockDeclarant = new Mock<IPartyIdProvider>();
		mockDeclarant.Setup(m => m.Id).Returns("ESA78587268");
		mockProvider.Setup(m => m.Declarant).Returns(mockDeclarant.Object);

		var mockRepresentative = new Mock<IPartyIdProvider>();
		mockRepresentative.Setup(m => m.Id).Returns("ESA78587268");
		mockProvider.Setup(m => m.Representative).Returns(mockRepresentative.Object);

		mockGoodsShipment = SetUpHeaderGoodsShipment();
		mockProvider.Setup(m => m.GoodsShipment).Returns(mockGoodsShipment.Object);
	}

	Mock<IComplXAESGoodsShipment> mockGoodsShipment;
	Mock<IComplXAESConsignment> mockConsignment;
	Mock<IComplXAESLine> mockLine1;

	protected Mock<IComplXAESExportOperation> SetUpExportOperation()
	{
		var mockExportOperation = new Mock<IComplXAESExportOperation>();
		mockExportOperation.Setup(m => m.MRN).Returns("21ES00999910000054");
		mockExportOperation.Setup(m => m.TotalAmount).Returns(11.879);
		mockExportOperation.Setup(m => m.Currency).Returns("JPY");

		return mockExportOperation;
	}

	Mock<IComplXAESGoodsShipment> SetUpHeaderGoodsShipment()
	{
		var mockHeader = new Mock<IComplXAESGoodsShipment>();
		mockHeader.Setup(m => m.NatureOfTransaction).Returns("91");

		var mockDeliveryTerms = new Mock<ICommonDeliveryTerms>();
		mockDeliveryTerms.Setup(m => m.Incoterm).Returns("XXX");
		mockDeliveryTerms.Setup(m => m.UNLCode).Returns("UNLCode");
		mockDeliveryTerms.Setup(m => m.IncotermLocation).Returns("ES009999000002");
		mockDeliveryTerms.Setup(m => m.DeliveryCountry).Returns("ES");
		mockDeliveryTerms.Setup(m => m.DeliveryText).Returns("hh");
		mockHeader.Setup(m => m.DeliveryTerms).Returns(mockDeliveryTerms.Object);

		var mockWarehouse = BuilderHelperTest.SetUpWarehouseCommon();
		mockHeader.Setup(m => m.Warehouse).Returns(mockWarehouse);

		mockConsignment = SetUpConsignment();
		mockHeader.Setup(m => m.Consignment).Returns(mockConsignment.Object);

		var mockPreviousDocument1 = SetUpAESCommonDocument("1", "NCLE", "12012021", "1", "NAR", 2.128, true);
		var mockPreviousDocument2 = SetUpAESCommonDocument("2", "NCLE", "12012021", "1", "NAR", 2, false);
		var mockPreviousDocuments = new IAESCommonDocument[] { mockPreviousDocument1.Object, mockPreviousDocument2.Object };

		var mockSupportingDocument1 = SetUpSupportingDocument("1", "N380", "Factura", "Custom", new DateTime(2022, 7, 21), "1");
		var mockSupportingDocument2 = SetUpSupportingDocument("2", "N325", "Proforma", "Custom", new DateTime(2022, 7, 21), "1");
		var mockSupportingDocuments = new IComplXAESSupportingDocument[] { mockSupportingDocument1.Object, mockSupportingDocument2.Object };

		mockLine1 = SetUpLine("1", mockPreviousDocuments, mockSupportingDocuments);

		var mockLine2 = SetUpLine("2", Enumerable.Empty<IAESCommonDocument>(), Enumerable.Empty<IComplXAESSupportingDocument>());

		var mockLines = new IComplXAESLine[] { mockLine1.Object, mockLine2.Object };

		mockHeader.Setup(m => m.Lines).Returns(mockLines);

		return mockHeader;
	}

	Mock<IComplXAESConsignment> SetUpConsignment()
	{
		var mockConsignment = new Mock<IComplXAESConsignment>();
		mockConsignment.Setup(m => m.InlandModeOfTransport).Returns("3");
		mockConsignment.Setup(m => m.ModeOfTransportAtBorder).Returns("3");

		var mockActiveBorderTransport = BuilderHelperTest.SetUpTransportMediumInfo("30", "DE", "CZ");
		mockConsignment.Setup(m => m.ActiveBorderTransportMeans).Returns(mockActiveBorderTransport.Object);

		mockConsignment.Setup(m => m.TransportChargesMoP).Returns("H");

		return mockConsignment;
	}

	Mock<IComplXAESLine> SetUpLine(ZString sequenceNumber, IEnumerable<IAESCommonDocument> previousDocuments, IEnumerable<IComplXAESSupportingDocument> documents)
	{
		var mockLine = new Mock<IComplXAESLine>();
		mockLine.Setup(m => m.SequenceNumber).Returns(sequenceNumber);
		mockLine.Setup(m => m.StatisticalValue).Returns(1234.5623m);

		var mockOrigin = new Mock<IAESCommonOrigin>();
		mockOrigin.Setup(m => m.CountryOfOrigin).Returns("ES");
		mockOrigin.Setup(m => m.StateOfOrigin).Returns("28");
		mockLine.Setup(m => m.Origin).Returns(mockOrigin.Object);

		mockCommodity = new Mock<IComplXAESCommodity>();

		var mockGoodsMeasure = new Mock<ICommonGoodsMeasureWithSpecified>();
		mockGoodsMeasure.Setup(m => m.GrossWeight).Returns(310);
		mockGoodsMeasure.Setup(m => m.GrossWeightSpecified).Returns(true);
		mockGoodsMeasure.Setup(m => m.NetWeight).Returns(300);
		mockGoodsMeasure.Setup(m => m.NetWeightSpecified).Returns(true);
		mockCommodity.Setup(m => m.GoodsMeasure).Returns(mockGoodsMeasure.Object);

		mockLine.Setup(m => m.Commodity).Returns(mockCommodity.Object);

		mockLine.Setup(m => m.PreviousDocuments).Returns((IReadOnlyCollection<IAESCommonDocument>)previousDocuments);
		mockLine.Setup(m => m.SupportingDocuments).Returns((IReadOnlyCollection<IComplXAESSupportingDocument>)documents);

		return mockLine;
	}

	Mock<IComplXAESCommodity> mockCommodity;

	Mock<IComplXAESSupportingDocument> SetUpSupportingDocument(ZString sequenceNumber, ZString name, ZString number, ZString issueAuthorityName, DateTime documentDate, ZString line)
	{
		var mockDocument = new Mock<IComplXAESSupportingDocument>();
		mockDocument.Setup(m => m.SequenceNumber).Returns(sequenceNumber);
		mockDocument.Setup(m => m.Name).Returns(name);
		mockDocument.Setup(m => m.Number).Returns(number);
		var mockCommonDocument = SetUpCommonSupportingDocumentExtraFields(issueAuthorityName, documentDate);
		mockDocument.Setup(m => m.CommonSupportingDocumentExtraFields).Returns(mockCommonDocument.Object);
		mockDocument.Setup(m => m.LineNumber).Returns(line);

		return mockDocument;
	}
	#endregion
}
