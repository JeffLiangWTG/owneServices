using System;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Customs.IT.MessageContracts.Declaration;
using CargoWise.Customs.IT.MessageContracts.Declaration.Export;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared.Testing;
using Moq;
using CustomsMessageBuilders = CargoWise.Customs.IT.MessageContracts.Declaration;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Export.Testing;

sealed class B4HeaderWrapperTest : B4HeaderWrapperTestBase
{
	public override void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Expected exception when entryHeader is null", () => new B4HeaderWrapper(entryHeader: null, exportMessageSendingWrapperFactory));

		var entryHeaderWithNoParentDeclaration = Factory.New<CusEntryHeader>();
		entryHeaderWithNoParentDeclaration.CH_CEI_Instruction = Factory.New<CusEntryInstruction>().PK;
		AssertExceptionThrown<ArgumentNullException>("Expected exception when parent declaration is null", () => new B4HeaderWrapper(entryHeaderWithNoParentDeclaration, exportMessageSendingWrapperFactory));

		var entryHeaderWithNoEntryInstruction = Factory.New<JobDeclaration>().CustomsEntryHeaders.AddNew();
		AssertExceptionThrown<ArgumentNullException>("Expected exception when related Entry Instruction is null", () => new B4HeaderWrapper(entryHeaderWithNoEntryInstruction, exportMessageSendingWrapperFactory));

		AssertExceptionThrown<ArgumentNullException>("Expected exception when exportMessageSendingWrapperFactory is null", () => new B4HeaderWrapper(EntryHeader, null));
	}

	public override void TestDeclarationCustomsOffice()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IB4Header.DeclarationCustomsOffice), wrapper.DeclarationCustomsOffice);

		declarationCustomsMessageWrapperMock.Setup(d => d.DeclarationCustomsOffice).Returns("000000");
		wrapper = CreateWrapper();
		AssertEquals(nameof(IB4Header.DeclarationCustomsOffice), "000000", wrapper.DeclarationCustomsOffice);
	}

	public override void TestAmendment()
	{
		var wrapper = CreateWrapper();
		AssertNull(nameof(IB4Header.Amendment), wrapper.Amendment);

		var amendmentMock = new Mock<IDeclarationAmendment>();
		wrapper = new B4HeaderWrapper(EntryHeader, exportMessageSendingWrapperFactory, amendmentMock.Object);
		AssertNotNull(nameof(IB4Header.Amendment), wrapper.Amendment);
	}

	public override void TestDeclarationType()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IB4Header.DeclarationType), wrapper.DeclarationType);

		declarationCustomsMessageWrapperMock.Setup(d => d.EntryStyle).Returns("EX");
		wrapper = CreateWrapper();
		AssertEquals(nameof(IB4Header.DeclarationType), "EX", wrapper.DeclarationType);
	}

	public override void TestAdditionalDeclarationType()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IB4Header.AdditionalDeclarationType), wrapper.AdditionalDeclarationType);

		entryInstructionCustomsMessageWrapperMock.Setup(d => d.AdditionalDeclarationType).Returns("A");
		wrapper = CreateWrapper();
		AssertEquals(nameof(IB4Header.AdditionalDeclarationType), "A", wrapper.AdditionalDeclarationType);
	}

	public override void TestLrn()
	{
		var wrapper = CreateWrapper();
		AssertEquals(nameof(IB4Header.Lrn), ITEDIMessage.ITMessageNumberPlaceholder, wrapper.Lrn);
	}

	public override void TestAcceptanceDate()
	{
		var wrapper = CreateWrapper();
		AssertNull(nameof(IB4Header.AcceptanceDate), wrapper.AcceptanceDate);

		var testSimpleDecAcceptanceDate = new DateTime(2023, 05, 16);
		entryInstructionCustomsMessageWrapperMock.Setup(e => e.AcceptanceDate).Returns(testSimpleDecAcceptanceDate);

		wrapper = CreateWrapper();
		AssertEquals(nameof(IB4Header.AcceptanceDate), testSimpleDecAcceptanceDate, wrapper.AcceptanceDate);
	}

	public override void TestAuthorizations()
	{
		var wrapper = CreateWrapper();
		AssertNotNull(nameof(IB4Header.Authorizations), wrapper.Authorizations);
		AssertEquals($"{nameof(IB4Header.Authorizations)} count", 0, wrapper.Authorizations.Count);

		var authorizationMock = new Mock<IAuthorization>();
		entryInstructionCustomsMessageWrapperMock.Setup(e => e.Authorizations).Returns(new[] { authorizationMock.Object });

		wrapper = CreateWrapper();
		var authorizations = wrapper.Authorizations;
		AssertEquals($"{nameof(IB4Header.Authorizations)} count", 1, authorizations.Count);
	}

	public override void TestExporter()
	{
		var wrapper = CreateWrapper();
		AssertNull(nameof(IB4Header.Exporter), wrapper.Exporter);

		var exporter = TraderWrapperTest.SetupTrader("EXPORTER");
		declarationCustomsMessageWrapperMock.Setup(d => d.Exporter).Returns(exporter.Object);

		wrapper = CreateWrapper();
		AssertNotNull(nameof(IB4Header.Exporter), wrapper.Exporter);
	}

	public override void TestDeclarant()
	{
		var wrapper = CreateWrapper();
		AssertNull(nameof(IB4Header.Declarant), wrapper.Declarant);

		var declarant = TraderWrapperTest.SetupTrader("DECLARANT");
		declarationCustomsMessageWrapperMock.Setup(d => d.ExportDeclarant).Returns(declarant.Object);

		wrapper = CreateWrapper();
		AssertNotNull(nameof(IB4Header.Declarant), wrapper.Declarant);
	}

	public override void TestRepresentative()
	{
		var wrapper = CreateWrapper();
		AssertNull(nameof(IB4Header.Representative), wrapper.Representative);

		var representativeMock = new Mock<IRepresentative>();
		declarationCustomsMessageWrapperMock.Setup(d => d.Representative).Returns(representativeMock.Object);

		wrapper = CreateWrapper();
		AssertNotNull(nameof(IB4Header.Representative), wrapper.Representative);
	}

	public override void TestInvoiceCurrencyCode()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IB4Header.InvoiceCurrencyCode), wrapper.InvoiceCurrencyCode);

		entryHeaderCustomsMessageWrapperMock.Setup(e => e.InvoiceCurrencyCode).Returns("AUD");

		wrapper = CreateWrapper();
		AssertEquals(nameof(IB4Header.InvoiceCurrencyCode), "AUD", wrapper.InvoiceCurrencyCode);
	}

	public override void TestInvoiceTotalAmount()
	{
		var wrapper = CreateWrapper();
		AssertNull(nameof(IB4Header.InvoiceTotalAmount), wrapper.InvoiceTotalAmount);

		entryHeaderCustomsMessageWrapperMock.Setup(e => e.InvoiceTotalAmount).Returns(334m);

		wrapper = CreateWrapper();
		AssertEquals(nameof(IB4Header.InvoiceTotalAmount), 334m, wrapper.InvoiceTotalAmount);
	}

	public override void TestGoodsPresentationDateTime()
	{
		var wrapper = CreateWrapper();
		AssertNull(nameof(IB4Header.GoodsPresentationDateTime), wrapper.GoodsPresentationDateTime);

		var testGoodsPresentationDateTime = new DateTime(2023, 03, 03);
		entryInstructionCustomsMessageWrapperMock.Setup(e => e.GoodsPresentationDateTime).Returns(testGoodsPresentationDateTime);

		wrapper = CreateWrapper();
		AssertEquals(nameof(IB4Header.GoodsPresentationDateTime), testGoodsPresentationDateTime, wrapper.GoodsPresentationDateTime);
	}

	public override void TestExitCustomsOffice()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IB4Header.ExitCustomsOffice), wrapper.ExitCustomsOffice);

		declarationCustomsMessageWrapperMock.Setup(d => d.ExitCustomsOffice).Returns("111111");
		wrapper = CreateWrapper();
		AssertEquals(nameof(IB4Header.ExitCustomsOffice), "111111", wrapper.ExitCustomsOffice);
	}

	public override void TestExportCustomsOffice()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IB4Header.ExportCustomsOffice), wrapper.ExportCustomsOffice);

		declarationCustomsMessageWrapperMock.Setup(d => d.ExportCustomsOffice).Returns("222222");
		wrapper = CreateWrapper();
		AssertEquals(nameof(IB4Header.ExportCustomsOffice), "222222", wrapper.ExportCustomsOffice);
	}

	public override void TestPresentationCustomsOffice()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IB4Header.PresentationCustomsOffice), wrapper.PresentationCustomsOffice);

		declarationCustomsMessageWrapperMock.Setup(d => d.PresentationCustomsOffice).Returns("DE123123");

		wrapper = CreateWrapper();
		AssertEquals(nameof(IB4Header.PresentationCustomsOffice), "DE123123", wrapper.PresentationCustomsOffice);
	}

	public override void TestPreviousDocuments()
	{
		var wrapper = CreateWrapper();
		var previousDocuments = wrapper.PreviousDocuments;
		AssertNotNull(nameof(IB4Header.PreviousDocuments), previousDocuments);
		AssertEquals($"{nameof(IB4Header.PreviousDocuments)} Count", 0, previousDocuments.Count);

		var mockPreviousDocument = new Mock<IPreviousDocument>();
		entryInstructionCustomsMessageWrapperMock.Setup(e => e.PreviousDocuments)
			.Returns(new[] { mockPreviousDocument.Object });

		wrapper = CreateWrapper();
		previousDocuments = wrapper.PreviousDocuments;
		AssertEquals($"{nameof(IB4Header.PreviousDocuments)} Count", 1, previousDocuments.Count);
	}

	public override void TestAdditionalInformation()
	{
		var wrapper = CreateWrapper();
		AssertNotNull(nameof(IB4Header.AdditionalInformation), wrapper.AdditionalInformation);
		AssertEquals(nameof(IB4Header.AdditionalInformation), 0, wrapper.AdditionalInformation.Count);

		var additionalInfoMock = new Mock<IAdditionalInformation>();
		entryInstructionCustomsMessageWrapperMock.Setup(e => e.AdditionalInformation)
			.Returns(new[] { additionalInfoMock.Object });

		wrapper = CreateWrapper();
		AssertEquals(nameof(IB4Header.AdditionalInformation), 1, wrapper.AdditionalInformation.Count);
	}

	public override void TestSupportingDocuments()
	{
		var wrapper = CreateWrapper();
		var supportingDocuments = wrapper.SupportingDocuments;
		AssertNotNull(nameof(IB4Header.SupportingDocuments), supportingDocuments);
		AssertEquals($"{nameof(IB4Header.SupportingDocuments)} Count", 0, supportingDocuments.Count);

		var mockSupportingDocument = new Mock<CustomsMessageBuilders.ISupportingDocument>();
		entryInstructionCustomsMessageWrapperMock.Setup(e => e.SupportingDocuments)
			.Returns(new[] { mockSupportingDocument.Object });

		wrapper = CreateWrapper();
		supportingDocuments = wrapper.SupportingDocuments;
		AssertNotNull(nameof(IB4Header.SupportingDocuments), supportingDocuments);
		AssertEquals($"{nameof(IB4Header.SupportingDocuments)} Count", 1, supportingDocuments.Count);
	}

	public override void TestTransportDocuments()
	{
		var wrapper = CreateWrapper();
		AssertNotNull(nameof(IB4Header.TransportDocuments), wrapper.TransportDocuments);
		AssertEquals(nameof(IB4Header.TransportDocuments), 0, wrapper.TransportDocuments.Count);

		var transportDocumentMock = new Mock<ITransportDocument>();
		entryInstructionCustomsMessageWrapperMock.Setup(e => e.TransportDocuments)
			.Returns(new[] { transportDocumentMock.Object });

		wrapper = CreateWrapper();
		AssertEquals(nameof(IB4Header.TransportDocuments), 1, wrapper.TransportDocuments.Count);
	}

	public override void TestUcr()
	{
		declarationCustomsMessageWrapperMock.Setup(d => d.Ucr).Returns(ZString.Empty);
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IB4Header.Ucr), wrapper.Ucr);

		declarationCustomsMessageWrapperMock.Setup(d => d.Ucr).Returns("UCR");
		AssertEquals(nameof(IB4Header.Ucr), "UCR", wrapper.Ucr);
	}

	public override void TestAdditionalSupplyChainActors()
	{
		var wrapper = CreateWrapper();
		AssertNotNull(nameof(IB4Header.AdditionalSupplyChainActors), wrapper.AdditionalSupplyChainActors);
		AssertEquals(nameof(IB4Header.AdditionalSupplyChainActors), 0, wrapper.AdditionalSupplyChainActors.Count);

		var additionalSupplyChainActor = new Mock<IAdditionalSupplyChainActor>();

		entryInstructionCustomsMessageWrapperMock.Setup(e => e.AdditionalSupplyChainActors).Returns(new[] { additionalSupplyChainActor.Object });
		wrapper = CreateWrapper();
		AssertEquals(nameof(IB4Header.AdditionalSupplyChainActors), 1, wrapper.AdditionalSupplyChainActors.Count);
	}

	public override void TestBorderMeansOfTransportMode()
	{
		var wrapper = CreateWrapper();
		AssertEquals(nameof(IB4Header.BorderMeansOfTransportMode), 0, wrapper.BorderMeansOfTransportMode);

		declarationCustomsMessageWrapperMock.Setup(d => d.BorderTransportMode).Returns(22);
		AssertEquals(nameof(IB4Header.BorderMeansOfTransportMode), 22, wrapper.BorderMeansOfTransportMode);
	}

	public override void TestCarrierIdentificationNumber()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IB4Header.CarrierIdentificationNumber), wrapper.CarrierIdentificationNumber);

		declarationCustomsMessageWrapperMock.Setup(d => d.CarrierIdentificationNumber).Returns("IT02028530281");
		wrapper = CreateWrapper();
		AssertEquals(nameof(IB4Header.CarrierIdentificationNumber), "IT02028530281", wrapper.CarrierIdentificationNumber);
	}

	public override void TestConsignee()
	{
		var wrapper = CreateWrapper();
		AssertNull(nameof(IB4Header.Consignee), wrapper.Consignee);

		var consigneeMock = TraderWrapperTest.SetupTrader("CONSIGNEE");
		entryHeaderCustomsMessageWrapperMock.Setup(x => x.Consignee).Returns(consigneeMock.Object);

		wrapper = CreateWrapper();
		AssertNotNull(nameof(IB4Header.Consignee), wrapper.Consignee);
	}

	public override void TestConsignmentRoutings()
	{
		var wrapper = CreateWrapper();
		AssertNotNull(nameof(IB4Header.ConsignmentRoutings), wrapper.ConsignmentRoutings);
		AssertEquals($"{nameof(IB4Header.ConsignmentRoutings)} count", 0, wrapper.ConsignmentRoutings.Count);

		var consignmentRoutingMock = new Mock<IConsignmentCountryRouting>();
		declarationCustomsMessageWrapperMock.Setup(x => x.ConsignmentRoutings).Returns(new[] { consignmentRoutingMock.Object });

		wrapper = CreateWrapper();
		AssertEquals(nameof(IB4Header.ConsignmentRoutings), 1, wrapper.ConsignmentRoutings.Count);
	}

	public override void TestCountryOfDestination()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IB4Header.CountryOfDestination), wrapper.CountryOfDestination);

		declarationCustomsMessageWrapperMock.Setup(d => d.GoodsCountryOfDestination).Returns("TR");

		wrapper = CreateWrapper();
		AssertEquals(nameof(IB4Header.CountryOfDestination), "TR", wrapper.CountryOfDestination);
	}

	public override void TestCountryOfExport()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IB4Header.CountryOfExport), wrapper.CountryOfExport);

		declarationCustomsMessageWrapperMock.Setup(d => d.CountryOfExport).Returns("TR");

		wrapper = CreateWrapper();
		AssertEquals(nameof(IB4Header.CountryOfExport), "TR", wrapper.CountryOfExport);
	}

	public override void TestGrossMass()
	{
		var wrapper = CreateWrapper();
		AssertEquals(nameof(IB4Header.GrossMass), 0m, wrapper.GrossMass);

		entryHeaderCustomsMessageWrapperMock.Setup(e => e.GrossMass).Returns(23.0563m);

		wrapper = CreateWrapper();
		AssertEquals(nameof(IB4Header.GrossMass), 23.0563m, wrapper.GrossMass);
	}

	public override void TestIsContainerizedTransport()
	{
		var wrapper = CreateWrapper();
		AssertEquals(nameof(IB4Header.IsContainerizedTransport), false, wrapper.IsContainerizedTransport);

		declarationCustomsMessageWrapperMock.Setup(d => d.IsContainerizedTransport).Returns(true);
		AssertEquals(nameof(IB4Header.IsContainerizedTransport), true, wrapper.IsContainerizedTransport);

		declarationCustomsMessageWrapperMock.Setup(d => d.IsContainerizedTransport).Returns(false);
		AssertEquals(nameof(IB4Header.IsContainerizedTransport), false, wrapper.IsContainerizedTransport);
	}

	public override void TestLocationOfGoods()
	{
		var wrapper = CreateWrapper();
		AssertNull(nameof(IB4Header.LocationOfGoods), wrapper.LocationOfGoods);

		var locationOfGoodsMock = new Mock<ILocationOfGoods>();
		declarationCustomsMessageWrapperMock.Setup(d => d.ExportLocationOfGoods).Returns(locationOfGoodsMock.Object);

		wrapper = CreateWrapper();
		AssertNotNull(nameof(IB4Header.LocationOfGoods), wrapper.LocationOfGoods);
	}

	public override void TestNatureOfTransaction()
	{
		var wrapper = CreateWrapper();
		AssertNull(nameof(IB4Header.NatureOfTransaction), wrapper.NatureOfTransaction);

		entryHeaderCustomsMessageWrapperMock.Setup(e => e.ExportNatureOfTransaction).Returns(11);

		wrapper = CreateWrapper();
		AssertEquals(nameof(IB4Header.NatureOfTransaction), 11, wrapper.NatureOfTransaction);
	}

	public override void TestTermsOfDelivery()
	{
		var wrapper = CreateWrapper();
		AssertNull(nameof(IB4Header.TermsOfDelivery), wrapper.TermsOfDelivery);

		var termsOfDeliveryMock = new Mock<ITermsOfDelivery>();
		entryHeaderCustomsMessageWrapperMock.Setup(d => d.TermOfDelivery).Returns(termsOfDeliveryMock.Object);

		AssertNotNull(nameof(IB4Header.TermsOfDelivery), wrapper.TermsOfDelivery);
	}

	public override void TestTransportChargesMethodOfPayment()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IB4Header.TransportChargesMethodOfPayment), wrapper.TransportChargesMethodOfPayment);

		entryHeaderCustomsMessageWrapperMock.Setup(d => d.TransportChargesMethodOfPayment).Returns("A");

		wrapper = CreateWrapper();
		AssertEquals(nameof(IB4Header.TransportChargesMethodOfPayment), "A", wrapper.TransportChargesMethodOfPayment);
	}

	public override void TestTransportEquipment()
	{
		var wrapper = CreateWrapper();
		AssertNotNull(nameof(IB4Header.TransportEquipment), wrapper.TransportEquipment);
		AssertEquals($"{nameof(IB4Header.TransportEquipment)} count", 0, wrapper.TransportEquipment.Count);

		var transportEquipmentMock = new Mock<ITransportEquipment>();
		entryHeaderCustomsMessageWrapperMock.Setup(d => d.TransportEquipment).Returns(new[] { transportEquipmentMock.Object });

		wrapper = CreateWrapper();
		var transportEquipmentWrappers = wrapper.TransportEquipment;
		AssertEquals($"{nameof(IB4Header.TransportEquipment)} count", 1, transportEquipmentWrappers.Count);
	}

	public override void TestWarehouse()
	{
		var wrapper = CreateWrapper();
		AssertNull(nameof(IB4Header.Warehouse), wrapper.Warehouse);

		var warehouseMock = new Mock<IWarehouse>();
		entryInstructionCustomsMessageWrapperMock.Setup(e => e.Warehouse).Returns(warehouseMock.Object);

		wrapper = CreateWrapper();
		AssertNotNull(nameof(IB4Header.Warehouse), wrapper.Warehouse);
	}

	protected override IB4Header CreateWrapper() => new B4HeaderWrapper(EntryHeader, exportMessageSendingWrapperFactory);

	protected override void SetUp()
	{
		base.SetUp();

		entryInstruction = JobDeclaration.CustomsEntryInstructions.AddNew();
		EntryHeader.CH_CEI_Instruction = entryInstruction.PK;

		declarationCustomsMessageWrapperMock = new Mock<IJobDeclarationCustomsMessageWrapper>();
		entryHeaderCustomsMessageWrapperMock = new Mock<ICusEntryHeaderCustomsMessageWrapper>();
		entryInstructionCustomsMessageWrapperMock = new Mock<IExportCusEntryInstructionCustomsMessageWrapper>();

		exportMessageSendingWrapperFactory = new ExportMessageSendingWrapperFactoryMockBuilder()
			.ConfigureGetNewJobDeclarationCustomsMessageWrapper(declarationCustomsMessageWrapperMock.Object)
			.ConfigureGetNewCusEntryHeaderCustomsMessageWrapper(entryHeaderCustomsMessageWrapperMock.Object)
			.ConfigureGetNewCusEntryInstructionCustomsMessageWrapper(entryInstructionCustomsMessageWrapperMock.Object)
			.Build();
	}

	CusEntryInstruction entryInstruction;
	CustomsMessageSending.IT.IExportMessageSendingWrapperFactory exportMessageSendingWrapperFactory;
	Mock<IJobDeclarationCustomsMessageWrapper> declarationCustomsMessageWrapperMock;
	Mock<ICusEntryHeaderCustomsMessageWrapper> entryHeaderCustomsMessageWrapperMock;
	Mock<IExportCusEntryInstructionCustomsMessageWrapper> entryInstructionCustomsMessageWrapperMock;
}
