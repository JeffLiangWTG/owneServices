using System;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Customs.IT.MessageContracts.Declaration;
using CargoWise.Customs.IT.MessageContracts.Declaration.Import;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared.Testing;
using Enterprise.MasterFiles.Business;
using Moq;
using MessageBuilder = CargoWise.Customs.IT.MessageContracts.Declaration;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Import.Testing;

sealed class H5HeaderWrapperTest : H5HeaderWrapperTestBase
{
	public override void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new H5HeaderWrapper(entryHeader: null, messageSendingWrapperFactory));

		var entryHeaderWithNoParentDeclaration = Factory.New<CusEntryHeader>();
		entryHeaderWithNoParentDeclaration.CH_CEI_Instruction = Factory.New<CusEntryInstruction>().PK;
		AssertExceptionThrown<ArgumentNullException>("Expected exception when parent declaration is null", () => new H5HeaderWrapper(entryHeaderWithNoParentDeclaration, messageSendingWrapperFactory));

		var entryHeaderWithNoEntryInstruction = Factory.New<JobDeclaration>().CustomsEntryHeaders.AddNew();
		AssertExceptionThrown<ArgumentNullException>("Expected exception when related Entry Instruction is null", () => new H5HeaderWrapper(entryHeaderWithNoEntryInstruction, messageSendingWrapperFactory));

		AssertExceptionThrown<ArgumentNullException>("Expected exception when messageSendingWrapperFactory is null", () => new H5HeaderWrapper(EntryHeader, null));
	}

	public override void TestAcceptanceDate()
	{
		var wrapper = CreateWrapper();
		AssertNull(nameof(IH5Header.AcceptanceDate), wrapper.AcceptanceDate);

		var testSimpleDecAcceptanceDate = new DateTime(2022, 01, 20);
		entryInstructionWrapperMock.Setup(e => e.AcceptanceDate).Returns(testSimpleDecAcceptanceDate);

		wrapper = CreateWrapper();
		AssertEquals(nameof(IH5Header.AcceptanceDate), testSimpleDecAcceptanceDate, wrapper.AcceptanceDate);
	}

	public override void TestAdditionalSupplyChainActors()
	{
		var wrapper = CreateWrapper();
		AssertNotNull(nameof(IH5Header.AdditionalSupplyChainActors), wrapper.AdditionalSupplyChainActors);
		AssertEquals(nameof(IH5Header.AdditionalSupplyChainActors), 0, wrapper.AdditionalSupplyChainActors.Count);

		var additionalSupplyChainActor = new Mock<IAdditionalSupplyChainActor>();
		entryInstructionWrapperMock.Setup(e => e.AdditionalSupplyChainActors).Returns(new[] { additionalSupplyChainActor.Object });
		wrapper = CreateWrapper();
		AssertEquals(nameof(IH5Header.AdditionalSupplyChainActors), 1, wrapper.AdditionalSupplyChainActors.Count);
	}

	public override void TestAdditionOrDeductions()
	{
		var wrapper = CreateWrapper();
		AssertNotNull(nameof(IH5Header.AdditionOrDeductions), wrapper.AdditionOrDeductions);
		AssertEquals($"{nameof(IH5Header.AdditionOrDeductions)} count", 0, wrapper.AdditionOrDeductions.Count);
	}

	public override void TestAmendment()
	{
		var wrapper = CreateWrapper();
		AssertNull(nameof(IH5Header.Amendment), wrapper.Amendment);

		var amendmentMock = new Mock<IDeclarationAmendment>();
		wrapper = new H5HeaderWrapper(EntryHeader, messageSendingWrapperFactory, amendmentMock.Object);
		AssertNotNull(nameof(IH5Header.Amendment), wrapper.Amendment);
	}

	public override void TestArrivalMeansOfTransport()
	{
		var wrapper = CreateWrapper();
		AssertNull(nameof(IH5Header.ArrivalMeansOfTransport), wrapper.ArrivalMeansOfTransport);

		declarationWrapperMock.Setup(x => x.ArrivalMeansOfTransport).Returns(new Mock<IArrivalMeansOfTransport>().Object);
		AssertNotNull(nameof(IH5Header.ArrivalMeansOfTransport), wrapper.ArrivalMeansOfTransport);
	}

	public override void TestAdditionalInformation()
	{
		var wrapper = CreateWrapper();
		AssertNotNull(nameof(IH5Header.AdditionalInformation), wrapper.AdditionalInformation);

		var additionalInfoMock = new Mock<IAdditionalInformation>();
		entryHeaderWrapperMock.Setup(e => e.AdditionalInformation).Returns(new[] { additionalInfoMock.Object });

		wrapper = CreateWrapper();
		AssertEquals($"{nameof(IH5Header.AdditionalInformation)} count", 1, wrapper.AdditionalInformation.Count);
	}

	public override void TestAuthorizations()
	{
		var wrapper = CreateWrapper();
		AssertNotNull(nameof(IH5Header.Authorizations), wrapper.Authorizations);
		AssertEquals($"{nameof(IH5Header.Authorizations)} count", 0, wrapper.Authorizations.Count);

		var authorizationMock = new Mock<IAuthorization>();
		entryInstructionWrapperMock.Setup(e => e.Authorizations).Returns(new[] { authorizationMock.Object });

		wrapper = CreateWrapper();
		var authorizations = wrapper.Authorizations;
		AssertEquals($"{nameof(IH5Header.Authorizations)} count", 1, authorizations.Count);
	}

	public override void TestBorderMeansOfTransportMode()
	{
		var wrapper = CreateWrapper();
		AssertEquals(nameof(IH5Header.BorderMeansOfTransportMode), 0, wrapper.BorderMeansOfTransportMode);

		declarationWrapperMock.Setup(d => d.BorderTransportMode).Returns(4);

		wrapper = CreateWrapper();
		AssertEquals(nameof(IH5Header.BorderMeansOfTransportMode), 4, wrapper.BorderMeansOfTransportMode);
	}

	public override void TestBorderMeansOfTransportNationality()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IH5Header.BorderMeansOfTransportNationality), wrapper.BorderMeansOfTransportNationality);

		declarationWrapperMock.Setup(d => d.BorderMeansOfTransportNationality).Returns("IT");

		wrapper = CreateWrapper();
		AssertEquals(nameof(IH5Header.BorderMeansOfTransportNationality), "IT", wrapper.BorderMeansOfTransportNationality);
	}

	public override void TestCountryOfDestination()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IH5Header.CountryOfDestination), wrapper.CountryOfDestination);

		declarationWrapperMock.Setup(d => d.GoodsCountryOfDestination).Returns("TR");

		wrapper = CreateWrapper();
		AssertEquals(nameof(IH5Header.CountryOfDestination), "TR", wrapper.CountryOfDestination);
	}

	public override void TestCountryOfDispatch()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IH5Header.CountryOfDispatch), wrapper.CountryOfDispatch);

		declarationWrapperMock.Setup(d => d.GoodsCountryOfOrigin).Returns("DE");

		wrapper = CreateWrapper();
		AssertEquals(nameof(IH5Header.CountryOfDispatch), "DE", wrapper.CountryOfDispatch);
	}

	public override void TestCustomsDutyPayerIdentificationNumber()
	{
		var wrapper = CreateWrapper();
		AssertEquals(nameof(IH5Header.CustomsDutyPayerIdentificationNumber), "", wrapper.CustomsDutyPayerIdentificationNumber);

		declarationWrapperMock.Setup(x => x.DutyPayerIdentificationNumber).Returns("DTYID");
		wrapper = CreateWrapper();
		AssertEquals(nameof(IH5Header.CustomsDutyPayerIdentificationNumber), "DTYID", wrapper.CustomsDutyPayerIdentificationNumber);
	}

	public override void TestDeclarant()
	{
		var wrapper = CreateWrapper();
		AssertNull(nameof(IH5Header.Declarant), wrapper.Declarant);

		var declarant = TraderWrapperTest.SetupTrader("DECLARANT");
		declarationWrapperMock.Setup(d => d.ImportDeclarant).Returns(declarant.Object);

		wrapper = CreateWrapper();
		AssertNotNull(nameof(IH5Header.Declarant), wrapper.Declarant);
	}

	public override void TestDeclarationCustomsOffice()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IH5Header.DeclarationCustomsOffice), wrapper.DeclarationCustomsOffice);

		declarationWrapperMock.Setup(d => d.DeclarationCustomsOffice).Returns("000000");
		wrapper = CreateWrapper();
		AssertEquals(nameof(IH5Header.DeclarationCustomsOffice), "000000", wrapper.DeclarationCustomsOffice);
	}

	public override void TestGrossMass()
	{
		var wrapper = CreateWrapper();
		AssertEquals(nameof(IH5Header.GrossMass), 0m, wrapper.GrossMass);

		entryHeaderWrapperMock.Setup(e => e.GrossMass).Returns(23.0563m);

		wrapper = CreateWrapper();
		AssertEquals(nameof(IH5Header.GrossMass), 23.0563m, wrapper.GrossMass);
	}

	public override void TestImporter()
	{
		var wrapper = CreateWrapper();
		AssertNull(nameof(IH5Header.Importer), wrapper.Importer);

		var importer = TraderWrapperTest.SetupTrader("IMPORTER");
		declarationWrapperMock.Setup(d => d.Importer).Returns(importer.Object);

		wrapper = CreateWrapper();
		AssertNotNull(nameof(IH5Header.Importer), wrapper.Importer);
	}

	public override void TestInlandTransportMode()
	{
		var wrapper = CreateWrapper();
		AssertEquals(nameof(IH5Header.InlandTransportMode), 0, wrapper.InlandTransportMode);

		declarationWrapperMock.Setup(d => d.InlandTransportMode).Returns(1);

		wrapper = CreateWrapper();
		AssertEquals(nameof(IH5Header.InlandTransportMode), 1, wrapper.InlandTransportMode);
	}

	public override void TestInvoiceCurrencyCode()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IH5Header.InvoiceCurrencyCode), wrapper.InvoiceCurrencyCode);

		entryHeaderWrapperMock.Setup(e => e.InvoiceCurrencyCode).Returns("AUD");

		wrapper = CreateWrapper();
		AssertEquals(nameof(IH5Header.InvoiceCurrencyCode), "AUD", wrapper.InvoiceCurrencyCode);
	}

	public override void TestInvoiceTotalAmount()
	{
		var wrapper = CreateWrapper();
		AssertNull(nameof(IH5Header.InvoiceTotalAmount), wrapper.InvoiceTotalAmount);

		entryHeaderWrapperMock.Setup(e => e.InvoiceTotalAmount).Returns(334m);

		wrapper = CreateWrapper();
		AssertEquals(nameof(IH5Header.InvoiceTotalAmount), 334m, wrapper.InvoiceTotalAmount);
	}

	public override void TestLocationOfGoods()
	{
		var wrapper = CreateWrapper();
		AssertNull(nameof(IH5Header.LocationOfGoods), wrapper.LocationOfGoods);

		var locationOfGoodsMock = new Mock<ILocationOfGoods>();
		declarationWrapperMock.Setup(d => d.ImportLocationOfGoods).Returns(locationOfGoodsMock.Object);

		wrapper = CreateWrapper();
		AssertNotNull(nameof(IH5Header.LocationOfGoods), wrapper.LocationOfGoods);
	}

	public override void TestLrn()
	{
		var wrapper = CreateWrapper();
		AssertEquals(nameof(IH5Header.Lrn), ITEDIMessage.ITMessageNumberPlaceholder, wrapper.Lrn);
	}

	public override void TestNatureOfTransaction()
	{
		var wrapper = CreateWrapper();
		AssertEquals(nameof(IH5Header.NatureOfTransaction), 0, wrapper.NatureOfTransaction);

		entryHeaderWrapperMock.Setup(e => e.NatureOfTransaction).Returns(11);

		wrapper = CreateWrapper();
		AssertEquals(nameof(IH5Header.NatureOfTransaction), 11, wrapper.NatureOfTransaction);
	}

	public override void TestNumberOfPackages()
	{
		var wrapper = CreateWrapper();
		AssertEquals(nameof(IH5Header.NumberOfPackages), 0, wrapper.NumberOfPackages);

		entryHeaderWrapperMock.Setup(e => e.NumberOfPackages).Returns(22);

		wrapper = CreateWrapper();
		AssertEquals(nameof(IH5Header.NumberOfPackages), 22, wrapper.NumberOfPackages);
	}

	public override void TestPresentationCustomsOffice()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IH5Header.PresentationCustomsOffice), wrapper.PresentationCustomsOffice);

		declarationWrapperMock.Setup(d => d.PresentationCustomsOffice).Returns("DE123123");

		wrapper = CreateWrapper();

		AssertEquals(nameof(IH5Header.PresentationCustomsOffice),
			"DE123123",
			wrapper.PresentationCustomsOffice);
	}

	public override void TestPreviousDocuments()
	{
		var wrapper = CreateWrapper();
		AssertNotNull(nameof(IH5Header.PreviousDocuments), wrapper.PreviousDocuments);
		AssertEquals($"{nameof(IH5Header.PreviousDocuments)} count", 0, wrapper.PreviousDocuments.Count);

		var previousDocumentMock = new Mock<IPreviousDocument>();
		entryInstructionWrapperMock.Setup(d => d.PreviousDocuments).Returns(new[] { previousDocumentMock.Object });

		wrapper = CreateWrapper();
		var previousDocumentWrappers = wrapper.PreviousDocuments;
		AssertEquals($"{nameof(IH5Header.PreviousDocuments)} count", 1, previousDocumentWrappers.Count);
	}

	public override void TestRegionOfDestination()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IH5Header.RegionOfDestination), wrapper.RegionOfDestination);

		declarationWrapperMock.Setup(d => d.RegionOfDestination).Returns("XX");

		wrapper = CreateWrapper();
		AssertEquals(nameof(IH5Header.RegionOfDestination), "XX", wrapper.RegionOfDestination);
	}

	public override void TestRepresentative()
	{
		var wrapper = CreateWrapper();
		AssertNull(nameof(IH5Header.Representative), wrapper.Representative);

		var representativeMock = new Mock<IRepresentative>();
		declarationWrapperMock.Setup(d => d.Representative).Returns(representativeMock.Object);

		wrapper = CreateWrapper();
		AssertNotNull(nameof(IH5Header.Representative), wrapper.Representative);
	}

	public override void TestSignerFiscalCode()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty($"When the current user does not have a COD certificate, {nameof(IH5Header.SignerFiscalCode)}", wrapper.SignerFiscalCode);

		var currentUser = GlbStaff.CurrentUser;
		var codCertificate = currentUser.Certificates.AddNew();
		codCertificate.XZ_Type = ItalyOrgCusCodeInfo.OrgCusCodes.CodiceFiscale;
		codCertificate.XZ_RefNumber = "";
		AssertNullOrEmpty($"When the current user has a COD certificate with empty number, {nameof(IH5Header.SignerFiscalCode)}", wrapper.SignerFiscalCode);

		codCertificate.XZ_RefNumber = "ITCOD";
		wrapper = CreateWrapper();
		AssertEquals($"When the current user has a COD certificate with filled number, {nameof(IH5Header.SignerFiscalCode)}", "ITCOD", wrapper.SignerFiscalCode);

		var automaticSignature = GlbStaffWrapper.Get(currentUser).AutomaticSignaturePasswordCollection.AddNew();
		automaticSignature.GP_Name = "REMOTE_ITCOD";
		automaticSignature.IsConfigurationActive = true;

		wrapper = CreateWrapper();
		AssertEquals($"When the current user has a valid automatic signature certificate with filled number, {nameof(IH1Header.SignerFiscalCode)}", "REMOTE_ITCOD", wrapper.SignerFiscalCode);

		automaticSignature.IsConfigurationActive = false;
		wrapper = CreateWrapper();
		AssertEquals($"When the current user has an automatic signature certificate but it is not valid, {nameof(IH1Header.SignerFiscalCode)}", "ITCOD", wrapper.SignerFiscalCode);
	}

	public override void TestSupplier()
	{
		var wrapper = CreateWrapper();
		AssertNull(nameof(IH5Header.Supplier), wrapper.Supplier);

		var supplier = TraderWrapperTest.SetupTrader("SUPPLIER");
		declarationWrapperMock.Setup(d => d.Supplier).Returns(supplier.Object);

		wrapper = CreateWrapper();
		AssertNotNull(nameof(IH5Header.Supplier), wrapper.Supplier);
	}

	public override void TestSupportingDocuments()
	{
		var wrapper = CreateWrapper();
		AssertNotNull(nameof(IH5Header.SupportingDocuments), wrapper.SupportingDocuments);
		AssertEquals($"{nameof(IH5Header.SupportingDocuments)} count", 0, wrapper.SupportingDocuments.Count);

		var supportingDocumentMock = new Mock<MessageBuilder.ISupportingDocument>();
		entryInstructionWrapperMock.Setup(d => d.SupportingDocuments).Returns(new[] { supportingDocumentMock.Object });

		wrapper = CreateWrapper();
		var supportingDocumentWrappers = wrapper.SupportingDocuments;
		AssertEquals($"{nameof(IH5Header.SupportingDocuments)} count", 1, supportingDocumentWrappers.Count);
	}

	public override void TestTermsOfDelivery()
	{
		var wrapper = CreateWrapper();
		AssertNull(nameof(IH5Header.TermsOfDelivery), wrapper.TermsOfDelivery);

		var termsOfDeliveryMock = new Mock<ITermsOfDelivery>();
		entryHeaderWrapperMock.Setup(e => e.TermOfDelivery).Returns(termsOfDeliveryMock.Object);

		wrapper = CreateWrapper();
		AssertNotNull(nameof(IH5Header.TermsOfDelivery), wrapper.TermsOfDelivery);
	}

	public override void TestUcr()
	{
		declarationWrapperMock.Setup(d => d.Ucr).Returns(ZString.Empty);
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IH5Header.Ucr), wrapper.Ucr);

		declarationWrapperMock.Setup(d => d.Ucr).Returns("UCR");
		AssertEquals(nameof(IH5Header.Ucr), "UCR", wrapper.Ucr);
	}

	public override void TestWarehouse()
	{
		var wrapper = CreateWrapper();
		AssertNull(nameof(IH5Header.Warehouse), wrapper.Warehouse);

		var warehouseMock = new Mock<IWarehouse>();
		entryInstructionWrapperMock.Setup(e => e.Warehouse).Returns(warehouseMock.Object);

		wrapper = CreateWrapper();
		AssertNotNull(nameof(IH5Header.Warehouse), wrapper.Warehouse);
	}

	protected override IH5Header CreateWrapper() => new H5HeaderWrapper(EntryHeader, messageSendingWrapperFactory);

	protected override void SetUp()
	{
		base.SetUp();
		entryInstruction = JobDeclaration.CustomsEntryInstructions.AddNew();
		EntryHeader.CH_CEI_Instruction = entryInstruction.PK;

		declarationWrapperMock = new Mock<IJobDeclarationCustomsMessageWrapper>();
		entryHeaderWrapperMock = new Mock<ICusEntryHeaderCustomsMessageWrapper>();
		entryInstructionWrapperMock = new Mock<ICusEntryInstructionCustomsMessageWrapper>();

		messageSendingWrapperFactory = new MessageSendingWrapperFactoryMockBuilder()
			.ConfigureGetNewJobDeclarationCustomsMessageWrapper(declarationWrapperMock.Object)
			.ConfigureGetNewCusEntryHeaderCustomsMessageWrapper(entryHeaderWrapperMock.Object)
			.ConfigureGetNewCusEntryInstructionCustomsMessageWrapper(entryInstructionWrapperMock.Object)
			.Build();
	}

	Mock<IJobDeclarationCustomsMessageWrapper> declarationWrapperMock;
	Mock<ICusEntryHeaderCustomsMessageWrapper> entryHeaderWrapperMock;
	Mock<ICusEntryInstructionCustomsMessageWrapper> entryInstructionWrapperMock;
	CusEntryInstruction entryInstruction;
	CustomsMessageSending.IT.IMessageSendingWrapperFactory messageSendingWrapperFactory;
}
