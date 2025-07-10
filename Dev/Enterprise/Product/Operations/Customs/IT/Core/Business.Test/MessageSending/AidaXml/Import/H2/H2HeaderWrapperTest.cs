using System;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Customs.IT.MessageContracts.Declaration;
using CargoWise.Customs.IT.MessageContracts.Declaration.Import;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared.Testing;
using Enterprise.MasterFiles.Business;
using Moq;
using MessageBuilder = CargoWise.Customs.IT.MessageContracts.Declaration;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Import.Testing;

sealed class H2HeaderWrapperTest : H2HeaderWrapperTestBase
{
	public override void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Expected exception when entryHeader is null", () => new H2HeaderWrapper(entryHeader: null, messageSendingWrapperFactory));

		var entryHeaderWithNoParentDeclaration = Factory.New<CusEntryHeader>();
		entryHeaderWithNoParentDeclaration.CH_CEI_Instruction = Factory.New<CusEntryInstruction>().PK;
		AssertExceptionThrown<ArgumentNullException>("Expected exception when parent declaration is null", () => new H2HeaderWrapper(entryHeaderWithNoParentDeclaration, messageSendingWrapperFactory));

		var entryHeaderWithNoEntryInstruction = Factory.New<JobDeclaration>().CustomsEntryHeaders.AddNew();
		AssertExceptionThrown<ArgumentNullException>("Expected exception when related Entry Instruction is null", () => new H2HeaderWrapper(entryHeaderWithNoEntryInstruction, messageSendingWrapperFactory));

		AssertExceptionThrown<ArgumentNullException>("Expected exception when messageSendingWrapperFactory is null", () => new H2HeaderWrapper(EntryHeader, null));
	}

	public override void TestDeclarationCustomsOffice()
	{
		var h2HeaderWrapper = CreateWrapper();
		AssertEquals(nameof(IH2Header.DeclarationCustomsOffice), "", h2HeaderWrapper.DeclarationCustomsOffice);

		declarationWrapperMock.Setup(d => d.DeclarationCustomsOffice).Returns("000000");

		h2HeaderWrapper = CreateWrapper();
		AssertEquals(nameof(IH2Header.DeclarationCustomsOffice), "000000", h2HeaderWrapper.DeclarationCustomsOffice);
	}

	public override void TestAdditionalDeclarationType()
	{
		var h2HeaderWrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IH2Header.AdditionalDeclarationType), h2HeaderWrapper.AdditionalDeclarationType);

		entryInstructionWrapperMock.Setup(e => e.AdditionalDeclarationType).Returns("D");

		h2HeaderWrapper = CreateWrapper();
		AssertEquals(nameof(IH2Header.AdditionalDeclarationType), "D", h2HeaderWrapper.AdditionalDeclarationType);
	}

	public override void TestAmendment()
	{
		var h2HeaderWrapper = CreateWrapper();
		AssertNull(nameof(IH2Header.Amendment), h2HeaderWrapper.Amendment);

		h2HeaderWrapper = new H2HeaderWrapper(EntryHeader, messageSendingWrapperFactory, new Mock<IDeclarationAmendment>().Object);
		AssertNotNull(nameof(IH2Header.Amendment), h2HeaderWrapper.Amendment);
	}

	public override void TestSignerFiscalCode()
	{
		var h2HeaderWrapper = CreateWrapper();
		AssertEquals($"When the current user does not have a COD certificate, {nameof(IH2Header.SignerFiscalCode)}", "", h2HeaderWrapper.SignerFiscalCode);

		var currentUser = GlbStaff.CurrentUser;
		var codCertificate = currentUser.Certificates.AddNew();
		codCertificate.XZ_Type = ItalyOrgCusCodeInfo.OrgCusCodes.CodiceFiscale;
		codCertificate.XZ_RefNumber = "";
		AssertEquals($"When the current user has a COD certificate with empty number, {nameof(IH2Header.SignerFiscalCode)}", "", h2HeaderWrapper.SignerFiscalCode);

		codCertificate.XZ_RefNumber = "ITCOD";
		h2HeaderWrapper = CreateWrapper();
		AssertEquals($"When the current user has a COD certificate with filled number, {nameof(IH2Header.SignerFiscalCode)}", "ITCOD", h2HeaderWrapper.SignerFiscalCode);

		var automaticSignature = GlbStaffWrapper.Get(currentUser).AutomaticSignaturePasswordCollection.AddNew();
		automaticSignature.GP_Name = "REMOTE_ITCOD";
		automaticSignature.IsConfigurationActive = true;

		h2HeaderWrapper = CreateWrapper();
		AssertEquals($"When the current user has a valid automatic signature certificate with filled number, {nameof(IH1Header.SignerFiscalCode)}", "REMOTE_ITCOD", h2HeaderWrapper.SignerFiscalCode);

		automaticSignature.IsConfigurationActive = false;
		h2HeaderWrapper = CreateWrapper();
		AssertEquals($"When the current user has an automatic signature certificate but it is not valid, {nameof(IH1Header.SignerFiscalCode)}", "ITCOD", h2HeaderWrapper.SignerFiscalCode);
	}

	public override void TestPreviousDocuments()
	{
		var h2HeaderWrapper = CreateWrapper();
		AssertNotNull(nameof(IH2Header.PreviousDocuments), h2HeaderWrapper.PreviousDocuments);
		AssertEquals($"{nameof(IH2Header.PreviousDocuments)} count", 0, h2HeaderWrapper.PreviousDocuments.Count);

		var previousDocumentMock = new Mock<IPreviousDocument>();
		entryInstructionWrapperMock.Setup(d => d.PreviousDocuments).Returns(new[] { previousDocumentMock.Object });

		h2HeaderWrapper = CreateWrapper();
		var previousDocumentWrappers = h2HeaderWrapper.PreviousDocuments;
		AssertEquals($"{nameof(IH2Header.PreviousDocuments)} count", 1, previousDocumentWrappers.Count);
	}

	public override void TestAdditionalInformation()
	{
		var wrapper = CreateWrapper();
		AssertNotNull(nameof(IH2Header.AdditionalInformation), wrapper.AdditionalInformation);

		var additionalInfoMock = new Mock<IAdditionalInformation>();
		entryHeaderWrapperMock.Setup(e => e.AdditionalInformation).Returns(new[] { additionalInfoMock.Object });

		wrapper = CreateWrapper();
		AssertEquals($"{nameof(IH2Header.AdditionalInformation)} count", 1, wrapper.AdditionalInformation.Count);
	}

	public override void TestSupportingDocuments()
	{
		var h2HeaderWrapper = CreateWrapper();
		AssertNotNull(nameof(IH2Header.SupportingDocuments), h2HeaderWrapper.SupportingDocuments);
		AssertEquals($"{nameof(IH2Header.SupportingDocuments)} count", 0, h2HeaderWrapper.SupportingDocuments.Count);

		var supportingDocumentMock = new Mock<MessageBuilder.ISupportingDocument>();
		entryInstructionWrapperMock.Setup(d => d.SupportingDocuments).Returns(new[] { supportingDocumentMock.Object });

		h2HeaderWrapper = CreateWrapper();
		var supportingDocumentWrappers = h2HeaderWrapper.SupportingDocuments;
		AssertEquals($"{nameof(IH2Header.SupportingDocuments)} count", 1, supportingDocumentWrappers.Count);
	}

	public override void TestUcr()
	{
		var h2HeaderWrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IH2Header.Ucr), h2HeaderWrapper.Ucr);

		declarationWrapperMock.Setup(d => d.Ucr).Returns("UCR");
		h2HeaderWrapper = CreateWrapper();

		AssertEquals(nameof(IH2Header.Ucr), "UCR", h2HeaderWrapper.Ucr);
	}

	public override void TestLrn()
	{
		var h2HeaderWrapper = CreateWrapper();
		AssertEquals(nameof(IH2Header.Lrn), ITEDIMessage.ITMessageNumberPlaceholder, h2HeaderWrapper.Lrn);
	}

	public override void TestWarehouse()
	{
		var h2HeaderWrapper = CreateWrapper();
		AssertNull(nameof(IH2Header.Warehouse), h2HeaderWrapper.Warehouse);

		var warehouseMock = new Mock<IWarehouse>();
		entryInstructionWrapperMock.Setup(e => e.Warehouse).Returns(warehouseMock.Object);

		h2HeaderWrapper = CreateWrapper();
		AssertNotNull(nameof(IH2Header.Warehouse), h2HeaderWrapper.Warehouse);
	}

	public override void TestImporter()
	{
		var h2HeaderWrapper = CreateWrapper();
		AssertNull(nameof(IH2Header.Importer), h2HeaderWrapper.Importer);

		var importer = TraderWrapperTest.SetupTrader("IMPORTER");
		declarationWrapperMock.Setup(d => d.Importer).Returns(importer.Object);

		h2HeaderWrapper = CreateWrapper();
		var importerWrapper = h2HeaderWrapper.Importer;
		AssertNotNull(nameof(IH2Header.Importer), importerWrapper);
	}

	public override void TestDeclarant()
	{
		var h2HeaderWrapper = CreateWrapper();
		AssertNull(nameof(IH2Header.Declarant), h2HeaderWrapper.Declarant);

		var declarant = TraderWrapperTest.SetupTrader("DECLARANT");
		declarationWrapperMock.Setup(d => d.ImportDeclarant).Returns(declarant.Object);

		h2HeaderWrapper = CreateWrapper();
		AssertNotNull(nameof(IH2Header.Declarant), h2HeaderWrapper.Declarant);
	}

	public override void TestRepresentative()
	{
		var h2HeaderWrapper = CreateWrapper();
		AssertNull(nameof(IH2Header.Representative), h2HeaderWrapper.Representative);

		var representativeMock = new Mock<IRepresentative>();
		declarationWrapperMock.Setup(d => d.Representative).Returns(representativeMock.Object);

		h2HeaderWrapper = CreateWrapper();
		AssertNotNull(nameof(IH2Header.Representative), h2HeaderWrapper.Representative);
	}

	public override void TestAdditionalSupplyChainActors()
	{
		var h2HeaderWrapper = CreateWrapper();
		AssertNotNull(nameof(IH2Header.AdditionalSupplyChainActors), h2HeaderWrapper.AdditionalSupplyChainActors);
		AssertEquals(nameof(IH2Header.AdditionalSupplyChainActors), 0, h2HeaderWrapper.AdditionalSupplyChainActors.Count);

		var additionalSupplyChainActor = new Mock<IAdditionalSupplyChainActor>();

		entryInstructionWrapperMock.Setup(e => e.AdditionalSupplyChainActors).Returns(new[] { additionalSupplyChainActor.Object });
		h2HeaderWrapper = CreateWrapper();
		AssertEquals(nameof(IH2Header.AdditionalSupplyChainActors), 1, h2HeaderWrapper.AdditionalSupplyChainActors.Count);
	}

	public override void TestAuthorizations()
	{
		var h2HeaderWrapper = CreateWrapper();
		AssertNotNull(nameof(IH2Header.Authorizations), h2HeaderWrapper.Authorizations);
		AssertEquals($"{nameof(IH2Header.Authorizations)} count", 0, h2HeaderWrapper.Authorizations.Count);

		var authorizationMock = new Mock<IAuthorization>();
		entryInstructionWrapperMock.Setup(e => e.Authorizations).Returns(new[] { authorizationMock.Object });

		h2HeaderWrapper = CreateWrapper();
		var authorizations = h2HeaderWrapper.Authorizations;
		AssertEquals($"{nameof(IH2Header.Authorizations)} count", 1, authorizations.Count);
	}

	public override void TestCountryOfDestination()
	{
		var h2HeaderWrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IH2Header.CountryOfDestination), h2HeaderWrapper.CountryOfDestination);

		declarationWrapperMock.Setup(d => d.GoodsCountryOfDestination).Returns("TR");

		h2HeaderWrapper = CreateWrapper();
		AssertEquals(nameof(IH2Header.CountryOfDestination), "TR", h2HeaderWrapper.CountryOfDestination);
	}

	public override void TestRegionOfDestination()
	{
		var h2HeaderWrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IH2Header.RegionOfDestination), h2HeaderWrapper.RegionOfDestination);

		declarationWrapperMock.Setup(d => d.RegionOfDestination).Returns("XX");

		h2HeaderWrapper = CreateWrapper();
		AssertEquals(nameof(IH2Header.RegionOfDestination), "XX", h2HeaderWrapper.RegionOfDestination);
	}

	public override void TestCountryOfDispatch()
	{
		var h2HeaderWrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IH2Header.CountryOfDispatch), h2HeaderWrapper.CountryOfDispatch);

		declarationWrapperMock.Setup(d => d.GoodsCountryOfOrigin).Returns("DE");

		h2HeaderWrapper = CreateWrapper();
		AssertEquals(nameof(IH2Header.CountryOfDispatch), "DE", h2HeaderWrapper.CountryOfDispatch);
	}

	public override void TestLocationOfGoods()
	{
		var h2HeaderWrapper = CreateWrapper();
		AssertNull(nameof(IH2Header.LocationOfGoods), h2HeaderWrapper.LocationOfGoods);

		var locationOfGoodsMock = new Mock<ILocationOfGoods>();
		declarationWrapperMock.Setup(d => d.ImportLocationOfGoods).Returns(locationOfGoodsMock.Object);

		h2HeaderWrapper = CreateWrapper();
		AssertNotNull(nameof(IH2Header.LocationOfGoods), h2HeaderWrapper.LocationOfGoods);
	}

	public override void TestPresentationCustomsOffice()
	{
		var h2HeaderWrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IH2Header.PresentationCustomsOffice), h2HeaderWrapper.PresentationCustomsOffice);

		declarationWrapperMock.Setup(d => d.PresentationCustomsOffice).Returns("DE123123");

		h2HeaderWrapper = CreateWrapper();

		AssertEquals(nameof(IH2Header.PresentationCustomsOffice),
			"DE123123",
			h2HeaderWrapper.PresentationCustomsOffice);
	}

	public override void TestSupervisingCustomsOffice()
	{
		var h2HeaderWrapper = CreateWrapper();
		AssertNullOrEmpty("Empty when SCO is not defined", h2HeaderWrapper.SupervisingCustomsOffice);

		declarationWrapperMock.Setup(d => d.SupervisingCustomsOffice).Returns("LV002000");

		h2HeaderWrapper = CreateWrapper();
		AssertEquals(nameof(IH2Header.SupervisingCustomsOffice), "LV002000", h2HeaderWrapper.SupervisingCustomsOffice);
	}

	public override void TestGrossMass()
	{
		var h2HeaderWrapper = CreateWrapper();
		AssertEquals(nameof(IH2Header.GrossMass), 0m, h2HeaderWrapper.GrossMass);

		entryHeaderWrapperMock.Setup(e => e.GrossMass).Returns(23.0563m);

		h2HeaderWrapper = CreateWrapper();
		AssertEquals(nameof(IH2Header.GrossMass), 23.0563m, h2HeaderWrapper.GrossMass);
	}

	public override void TestIsContainerizedTransport()
	{
		var h2HeaderWrapper = CreateWrapper();
		AssertEquals(nameof(IH2Header.IsContainerizedTransport), false, h2HeaderWrapper.IsContainerizedTransport);

		declarationWrapperMock.Setup(d => d.IsContainerizedTransport).Returns(true);
		h2HeaderWrapper = CreateWrapper();
		AssertEquals(nameof(IH2Header.IsContainerizedTransport), true, h2HeaderWrapper.IsContainerizedTransport);
	}

	public override void TestBorderMeansOfTransportMode()
	{
		var h2HeaderWrapper = CreateWrapper();
		AssertEquals(nameof(IH2Header.BorderMeansOfTransportMode), 0, h2HeaderWrapper.BorderMeansOfTransportMode);

		declarationWrapperMock.Setup(d => d.BorderTransportMode).Returns(4);

		h2HeaderWrapper = CreateWrapper();
		AssertEquals(nameof(IH2Header.BorderMeansOfTransportMode), 4, h2HeaderWrapper.BorderMeansOfTransportMode);
	}

	public override void TestInlandTransportMode()
	{
		var h2HeaderWrapper = CreateWrapper();
		AssertNull(nameof(IH2Header.InlandTransportMode), h2HeaderWrapper.InlandTransportMode);

		declarationWrapperMock.Setup(d => d.InlandTransportMode).Returns(1);

		h2HeaderWrapper = CreateWrapper();
		AssertEquals(nameof(IH2Header.InlandTransportMode), 1, h2HeaderWrapper.InlandTransportMode);
	}

	public override void TestNatureOfTransaction()
	{
		var h2HeaderWrapper = CreateWrapper();
		AssertEquals(nameof(IH2Header.NatureOfTransaction), 0, h2HeaderWrapper.NatureOfTransaction);

		entryHeaderWrapperMock.Setup(e => e.NatureOfTransaction).Returns(11);

		h2HeaderWrapper = CreateWrapper();
		AssertEquals(nameof(IH2Header.NatureOfTransaction), 11, h2HeaderWrapper.NatureOfTransaction);
	}

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
			.ConfigureGetNewCusEntryInstructionCustomsMessageWrapper(entryInstructionWrapperMock.Object)
			.ConfigureGetNewCusEntryHeaderCustomsMessageWrapper(entryHeaderWrapperMock.Object)
			.Build();
	}

	protected override IH2Header CreateWrapper() => new H2HeaderWrapper(EntryHeader, messageSendingWrapperFactory);

	Mock<IJobDeclarationCustomsMessageWrapper> declarationWrapperMock;
	Mock<ICusEntryHeaderCustomsMessageWrapper> entryHeaderWrapperMock;
	Mock<ICusEntryInstructionCustomsMessageWrapper> entryInstructionWrapperMock;
	CusEntryInstruction entryInstruction;
	CustomsMessageSending.IT.IMessageSendingWrapperFactory messageSendingWrapperFactory;
}
