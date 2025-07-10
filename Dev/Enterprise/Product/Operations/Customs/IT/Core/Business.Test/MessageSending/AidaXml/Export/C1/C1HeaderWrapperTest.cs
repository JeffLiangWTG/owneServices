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

sealed class C1HeaderWrapperTest : C1HeaderWrapperTestBase
{
	public override void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Expected exception when entryHeader is null", () => new C1HeaderWrapper(entryHeader: null, exportMessageSendingWrapperFactory));

		var entryHeaderWithNoParentDeclaration = Factory.New<CusEntryHeader>();
		entryHeaderWithNoParentDeclaration.CH_CEI_Instruction = Factory.New<CusEntryInstruction>().PK;
		AssertExceptionThrown<ArgumentNullException>("Expected exception when parent declaration is null", () => new C1HeaderWrapper(entryHeaderWithNoParentDeclaration, exportMessageSendingWrapperFactory));

		var entryHeaderWithNoEntryInstruction = Factory.New<JobDeclaration>().CustomsEntryHeaders.AddNew();
		AssertExceptionThrown<ArgumentNullException>("Expected exception when related Entry Instruction is null", () => new C1HeaderWrapper(entryHeaderWithNoEntryInstruction, exportMessageSendingWrapperFactory));

		AssertExceptionThrown<ArgumentNullException>("Expected exception when exportMessageSendingWrapperFactory is null", () => new C1HeaderWrapper(EntryHeader, null));
	}

	public override void TestAdditionalDeclarationType()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IC1Header.AdditionalDeclarationType), wrapper.AdditionalDeclarationType);

		entryInstructionCustomsMessageWrapperMock.Setup(d => d.AdditionalDeclarationType).Returns("A");
		wrapper = CreateWrapper();
		AssertEquals(nameof(IC1Header.AdditionalDeclarationType), "A", wrapper.AdditionalDeclarationType);
	}

	public override void TestAdditionalInformation()
	{
		var wrapper = CreateWrapper();
		AssertNotNull(nameof(IC1Header.AdditionalInformation), wrapper.AdditionalInformation);
		AssertEquals(nameof(IC1Header.AdditionalInformation), 0, wrapper.AdditionalInformation.Count);

		var additionalInfoMock = new Mock<IAdditionalInformation>();
		entryInstructionCustomsMessageWrapperMock.Setup(e => e.AdditionalInformation)
			.Returns(new[] { additionalInfoMock.Object });

		wrapper = CreateWrapper();
		AssertEquals(nameof(IC1Header.AdditionalInformation), 1, wrapper.AdditionalInformation.Count);
	}

	public override void TestAdditionalReferences()
	{
		var wrapper = CreateWrapper();
		AssertNotNull(nameof(IC1Header.AdditionalReferences), wrapper.AdditionalReferences);
		AssertEquals(nameof(IC1Header.AdditionalReferences), 0, wrapper.AdditionalReferences.Count);

		var additionalReferenceMock = new Mock<IAdditionalReference>();
		entryInstructionCustomsMessageWrapperMock.Setup(e => e.AdditionalReferences)
			.Returns(new[] { additionalReferenceMock.Object });

		wrapper = CreateWrapper();
		AssertEquals(nameof(IC1Header.AdditionalReferences), 1, wrapper.AdditionalReferences.Count);
	}

	public override void TestAdditionalSupplyChainActors()
	{
		var wrapper = CreateWrapper();
		AssertNotNull(nameof(IC1Header.AdditionalSupplyChainActors), wrapper.AdditionalSupplyChainActors);
		AssertEquals(nameof(IC1Header.AdditionalSupplyChainActors), 0, wrapper.AdditionalSupplyChainActors.Count);

		var additionalSupplyChainActor = new Mock<IAdditionalSupplyChainActor>();

		entryInstructionCustomsMessageWrapperMock.Setup(e => e.AdditionalSupplyChainActors).Returns(new[] { additionalSupplyChainActor.Object });
		wrapper = CreateWrapper();
		AssertEquals(nameof(IC1Header.AdditionalSupplyChainActors), 1, wrapper.AdditionalSupplyChainActors.Count);
	}

	public override void TestAmendment()
	{
		var wrapper = CreateWrapper();
		AssertNull(nameof(IC1Header.Amendment), wrapper.Amendment);

		var amendmentMock = new Mock<IDeclarationAmendment>();
		wrapper = new C1HeaderWrapper(EntryHeader, exportMessageSendingWrapperFactory, amendmentMock.Object);
		AssertNotNull(nameof(IC1Header.Amendment), wrapper.Amendment);
	}

	public override void TestAuthorizations()
	{
		var wrapper = CreateWrapper();
		AssertNotNull(nameof(IC1Header.Authorizations), wrapper.Authorizations);
		AssertEquals($"{nameof(IC1Header.Authorizations)} count", 0, wrapper.Authorizations.Count);

		var authorizationMock = new Mock<IAuthorization>();
		entryInstructionCustomsMessageWrapperMock.Setup(e => e.Authorizations).Returns(new[] { authorizationMock.Object });

		wrapper = CreateWrapper();
		var authorizations = wrapper.Authorizations;
		AssertEquals($"{nameof(IC1Header.Authorizations)} count", 1, authorizations.Count);
	}

	public override void TestCarrierIdentificationNumber()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IC1Header.CarrierIdentificationNumber), wrapper.CarrierIdentificationNumber);

		declarationCustomsMessageWrapperMock.Setup(d => d.CarrierIdentificationNumber).Returns("IT02028530281");
		wrapper = CreateWrapper();
		AssertEquals(nameof(IC1Header.CarrierIdentificationNumber), "IT02028530281", wrapper.CarrierIdentificationNumber);
	}

	public override void TestConsignee()
	{
		var wrapper = CreateWrapper();
		AssertNull(nameof(IC1Header.Consignee), wrapper.Consignee);

		var consigneeMock = TraderWrapperTest.SetupTrader("CONSIGNEE");
		entryHeaderCustomsMessageWrapperMock.Setup(x => x.Consignee).Returns(consigneeMock.Object);

		wrapper = CreateWrapper();
		AssertNotNull(nameof(IC1Header.Consignee), wrapper.Consignee);
	}

	public override void TestConsignmentRoutings()
	{
		var wrapper = CreateWrapper();
		AssertNotNull(nameof(IC1Header.ConsignmentRoutings), wrapper.ConsignmentRoutings);
		AssertEquals($"{nameof(IC1Header.ConsignmentRoutings)} count", 0, wrapper.ConsignmentRoutings.Count);

		var consignmentRoutingMock = new Mock<IConsignmentCountryRouting>();
		declarationCustomsMessageWrapperMock.Setup(x => x.ConsignmentRoutings).Returns(new[] { consignmentRoutingMock.Object });

		wrapper = CreateWrapper();
		AssertEquals(nameof(IC1Header.ConsignmentRoutings), 1, wrapper.ConsignmentRoutings.Count);
	}

	public override void TestConsignor()
	{
		var wrapper = CreateWrapper();
		AssertNull(nameof(IC1Header.Consignor), wrapper.Consignor);

		var consignorMock = TraderWrapperTest.SetupTrader("CONSIGNOR");
		entryHeaderCustomsMessageWrapperMock.Setup(x => x.Consignor).Returns(consignorMock.Object);

		wrapper = CreateWrapper();
		AssertNotNull(nameof(IC1Header.Consignor), wrapper.Consignor);
	}

	public override void TestCountryOfDestination()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IC1Header.CountryOfDestination), wrapper.CountryOfDestination);

		entryHeaderCustomsMessageWrapperMock.Setup(d => d.CountryOfDestination).Returns("TR");

		wrapper = CreateWrapper();
		AssertEquals(nameof(IC1Header.CountryOfDestination), "TR", wrapper.CountryOfDestination);
	}

	public override void TestCountryOfExport()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IC1Header.CountryOfExport), wrapper.CountryOfExport);

		entryHeaderCustomsMessageWrapperMock.Setup(d => d.CountryOfExport).Returns("TR");

		wrapper = CreateWrapper();
		AssertEquals(nameof(IC1Header.CountryOfExport), "TR", wrapper.CountryOfExport);
	}

	public override void TestDeclarant()
	{
		var wrapper = CreateWrapper();
		AssertNull(nameof(IC1Header.Declarant), wrapper.Declarant);

		var declarant = TraderWrapperTest.SetupTrader("DECLARANT");
		declarationCustomsMessageWrapperMock.Setup(d => d.ExportDeclarant).Returns(declarant.Object);

		wrapper = CreateWrapper();
		AssertNotNull(nameof(IC1Header.Declarant), wrapper.Declarant);
	}

	public override void TestDeclarationCustomsOffice()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IC1Header.DeclarationCustomsOffice), wrapper.DeclarationCustomsOffice);

		declarationCustomsMessageWrapperMock.Setup(d => d.DeclarationCustomsOffice).Returns("000000");
		wrapper = CreateWrapper();
		AssertEquals(nameof(IC1Header.DeclarationCustomsOffice), "000000", wrapper.DeclarationCustomsOffice);
	}

	public override void TestDeclarationType()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IC1Header.DeclarationType), wrapper.DeclarationType);

		declarationCustomsMessageWrapperMock.Setup(d => d.EntryStyle).Returns("EX");
		wrapper = CreateWrapper();
		AssertEquals(nameof(IC1Header.DeclarationType), "EX", wrapper.DeclarationType);
	}

	public override void TestDeferredPayment()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IC1Header.DeferredPayment), wrapper.DeferredPayment);

		entryHeaderCustomsMessageWrapperMock.Setup(d => d.DeferredPayment).Returns("123456A");
		wrapper = CreateWrapper();
		AssertEquals(nameof(IC1Header.DeferredPayment), "123456A", wrapper.DeferredPayment);
	}

	public override void TestExitCustomsOffice()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IC1Header.ExitCustomsOffice), wrapper.ExitCustomsOffice);

		declarationCustomsMessageWrapperMock.Setup(d => d.ExitCustomsOffice).Returns("111111");
		wrapper = CreateWrapper();
		AssertEquals(nameof(IC1Header.ExitCustomsOffice), "111111", wrapper.ExitCustomsOffice);
	}

	public override void TestExportCustomsOffice()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IC1Header.ExportCustomsOffice), wrapper.ExportCustomsOffice);

		declarationCustomsMessageWrapperMock.Setup(d => d.ExportCustomsOffice).Returns("222222");
		wrapper = CreateWrapper();
		AssertEquals(nameof(IC1Header.ExportCustomsOffice), "222222", wrapper.ExportCustomsOffice);
	}

	public override void TestExporter()
	{
		var wrapper = CreateWrapper();
		AssertNull(nameof(IC1Header.Exporter), wrapper.Exporter);

		var exporter = TraderWrapperTest.SetupTrader("EXPORTER");
		declarationCustomsMessageWrapperMock.Setup(d => d.Exporter).Returns(exporter.Object);

		wrapper = CreateWrapper();
		AssertNotNull(nameof(IC1Header.Exporter), wrapper.Exporter);
	}

	public override void TestGoodsPresentationDateTime()
	{
		var wrapper = CreateWrapper();
		AssertNull(nameof(IC1Header.GoodsPresentationDateTime), wrapper.GoodsPresentationDateTime);

		var testGoodsPresentationDateTime = new DateTime(2023, 03, 03);
		entryInstructionCustomsMessageWrapperMock.Setup(e => e.GoodsPresentationDateTime).Returns(testGoodsPresentationDateTime);

		wrapper = CreateWrapper();
		AssertEquals(nameof(IC1Header.GoodsPresentationDateTime), testGoodsPresentationDateTime, wrapper.GoodsPresentationDateTime);
	}

	public override void TestGrossMass()
	{
		var wrapper = CreateWrapper();
		AssertEquals(nameof(IC1Header.GrossMass), 0m, wrapper.GrossMass);

		entryHeaderCustomsMessageWrapperMock.Setup(e => e.GrossMass).Returns(23.0563m);

		wrapper = CreateWrapper();
		AssertEquals(nameof(IC1Header.GrossMass), 23.0563m, wrapper.GrossMass);
	}

	public override void TestIsSecurityDeclaration()
	{
		var wrapper = CreateWrapper();
		AssertEquals(nameof(IC1Header.IsSecurityDeclaration), false, wrapper.IsSecurityDeclaration);

		declarationCustomsMessageWrapperMock.Setup(d => d.IsSecurityDeclaration).Returns(true);
		wrapper = CreateWrapper();
		AssertEquals(nameof(IC1Header.IsSecurityDeclaration), true, wrapper.IsSecurityDeclaration);
	}

	public override void TestLocationOfGoods()
	{
		var wrapper = CreateWrapper();
		AssertNull(nameof(IC1Header.LocationOfGoods), wrapper.LocationOfGoods);

		var locationOfGoodsMock = new Mock<ILocationOfGoods>();
		declarationCustomsMessageWrapperMock.Setup(d => d.ExportLocationOfGoods).Returns(locationOfGoodsMock.Object);

		wrapper = CreateWrapper();
		AssertNotNull(nameof(IC1Header.LocationOfGoods), wrapper.LocationOfGoods);
	}

	public override void TestLrn()
	{
		var wrapper = CreateWrapper();
		AssertEquals(nameof(IC1Header.Lrn), ITEDIMessage.ITMessageNumberPlaceholder, wrapper.Lrn);
	}

	public override void TestPresentationCustomsOffice()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IC1Header.PresentationCustomsOffice), wrapper.PresentationCustomsOffice);

		declarationCustomsMessageWrapperMock.Setup(d => d.PresentationCustomsOffice).Returns("DE123123");

		wrapper = CreateWrapper();
		AssertEquals(nameof(IC1Header.PresentationCustomsOffice), "DE123123", wrapper.PresentationCustomsOffice);
	}

	public override void TestPreviousDocuments()
	{
		var wrapper = CreateWrapper();
		var previousDocuments = wrapper.PreviousDocuments;
		AssertNotNull(nameof(IC1Header.PreviousDocuments), previousDocuments);
		AssertEquals($"{nameof(IC1Header.PreviousDocuments)} Count", 0, previousDocuments.Count);

		var mockPreviousDocument = new Mock<IPreviousDocument>();
		entryInstructionCustomsMessageWrapperMock.Setup(e => e.PreviousDocuments)
			.Returns(new[] { mockPreviousDocument.Object });

		wrapper = CreateWrapper();
		previousDocuments = wrapper.PreviousDocuments;
		AssertEquals($"{nameof(IC1Header.PreviousDocuments)} Count", 1, previousDocuments.Count);
	}

	public override void TestRepresentative()
	{
		var wrapper = CreateWrapper();
		AssertNull(nameof(IC1Header.Representative), wrapper.Representative);

		var representativeMock = new Mock<IRepresentative>();
		declarationCustomsMessageWrapperMock.Setup(d => d.Representative).Returns(representativeMock.Object);

		wrapper = CreateWrapper();
		AssertNotNull(nameof(IC1Header.Representative), wrapper.Representative);
	}

	public override void TestSpecificCircumstanceIndicator()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IC1Header.SpecificCircumstanceIndicator), wrapper.SpecificCircumstanceIndicator);

		declarationCustomsMessageWrapperMock.Setup(x => x.SpecificCircumstanceIndicator).Returns("A20");

		wrapper = CreateWrapper();
		AssertEquals(nameof(IC1Header.SpecificCircumstanceIndicator), "A20", wrapper.SpecificCircumstanceIndicator);
	}

	public override void TestSupervisingCustomsOffice()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty("Empty when SVO is not defined", wrapper.SupervisingCustomsOffice);

		declarationCustomsMessageWrapperMock.Setup(d => d.SupervisingCustomsOffice).Returns("LV002000");

		wrapper = CreateWrapper();
		AssertEquals(nameof(IC1Header.SupervisingCustomsOffice), "LV002000", wrapper.SupervisingCustomsOffice);
	}

	public override void TestSupportingDocuments()
	{
		var wrapper = CreateWrapper();
		var supportingDocuments = wrapper.SupportingDocuments;
		AssertNotNull(nameof(IC1Header.SupportingDocuments), supportingDocuments);
		AssertEquals($"{nameof(IC1Header.SupportingDocuments)} Count", 0, supportingDocuments.Count);

		var mockSupportingDocument = new Mock<CustomsMessageBuilders.ISupportingDocument>();
		entryInstructionCustomsMessageWrapperMock.Setup(e => e.SupportingDocuments)
			.Returns(new[] { mockSupportingDocument.Object });

		wrapper = CreateWrapper();
		supportingDocuments = wrapper.SupportingDocuments;
		AssertNotNull(nameof(IC1Header.SupportingDocuments), supportingDocuments);
		AssertEquals($"{nameof(IC1Header.SupportingDocuments)} Count", 1, supportingDocuments.Count);
	}

	public override void TestTermsOfDelivery()
	{
		var wrapper = CreateWrapper();
		AssertNull(nameof(IC1Header.TermsOfDelivery), wrapper.TermsOfDelivery);

		var termsOfDeliveryMock = new Mock<ITermsOfDelivery>();
		entryHeaderCustomsMessageWrapperMock.Setup(d => d.TermOfDelivery).Returns(termsOfDeliveryMock.Object);

		AssertNotNull(nameof(IC1Header.TermsOfDelivery), wrapper.TermsOfDelivery);
	}

	public override void TestTransportChargesMethodOfPayment()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IC1Header.TransportChargesMethodOfPayment), wrapper.TransportChargesMethodOfPayment);

		entryHeaderCustomsMessageWrapperMock.Setup(d => d.TransportChargesMethodOfPayment).Returns("A");

		wrapper = CreateWrapper();
		AssertEquals(nameof(IC1Header.TransportChargesMethodOfPayment), "A", wrapper.TransportChargesMethodOfPayment);
	}

	public override void TestTransportDocuments()
	{
		var wrapper = CreateWrapper();
		AssertNotNull(nameof(IC1Header.TransportDocuments), wrapper.TransportDocuments);
		AssertEquals(nameof(IC1Header.TransportDocuments), 0, wrapper.TransportDocuments.Count);

		var transportDocumentMock = new Mock<ITransportDocument>();
		entryInstructionCustomsMessageWrapperMock.Setup(e => e.TransportDocuments)
			.Returns(new[] { transportDocumentMock.Object });

		wrapper = CreateWrapper();
		AssertEquals(nameof(IC1Header.TransportDocuments), 1, wrapper.TransportDocuments.Count);
	}

	public override void TestUcr()
	{
		declarationCustomsMessageWrapperMock.Setup(d => d.Ucr).Returns(ZString.Empty);
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IC1Header.Ucr), wrapper.Ucr);

		declarationCustomsMessageWrapperMock.Setup(d => d.Ucr).Returns("UCR");
		AssertEquals(nameof(IC1Header.Ucr), "UCR", wrapper.Ucr);
	}

	protected override IC1Header CreateWrapper() => new C1HeaderWrapper(EntryHeader, exportMessageSendingWrapperFactory);

	protected override void SetUp()
	{
		base.SetUp();

		entryInstruction = JobDeclaration.CustomsEntryInstructions.AddNew();
		EntryHeader.CH_CEI_Instruction = entryInstruction.PK;

		entryInstructionCustomsMessageWrapperMock = new Mock<IExportCusEntryInstructionCustomsMessageWrapper>();
		declarationCustomsMessageWrapperMock = new Mock<IJobDeclarationCustomsMessageWrapper>();
		entryHeaderCustomsMessageWrapperMock = new Mock<ICusEntryHeaderCustomsMessageWrapper>();

		exportMessageSendingWrapperFactory = new ExportMessageSendingWrapperFactoryMockBuilder()
			.ConfigureGetNewCusEntryInstructionCustomsMessageWrapper(entryInstructionCustomsMessageWrapperMock.Object)
			.ConfigureGetNewJobDeclarationCustomsMessageWrapper(declarationCustomsMessageWrapperMock.Object)
			.ConfigureGetNewCusEntryHeaderCustomsMessageWrapper(entryHeaderCustomsMessageWrapperMock.Object)
			.Build();
	}

	CusEntryInstruction entryInstruction;
	CustomsMessageSending.IT.IExportMessageSendingWrapperFactory exportMessageSendingWrapperFactory;
	Mock<IExportCusEntryInstructionCustomsMessageWrapper> entryInstructionCustomsMessageWrapperMock;
	Mock<IJobDeclarationCustomsMessageWrapper> declarationCustomsMessageWrapperMock;
	Mock<ICusEntryHeaderCustomsMessageWrapper> entryHeaderCustomsMessageWrapperMock;
}
