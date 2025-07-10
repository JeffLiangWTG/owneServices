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

sealed class I1HeaderWrapperTest : I1HeaderWrapperTestBase
{
	public override void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Expected exception when entryHeader is null", () => new I1HeaderWrapper(entryHeader: null, messageSendingWrapperFactory));

		var entryHeaderWithNoParentDeclaration = Factory.New<CusEntryHeader>();
		entryHeaderWithNoParentDeclaration.CH_CEI_Instruction = Factory.New<CusEntryInstruction>().PK;
		AssertExceptionThrown<ArgumentNullException>("Expected exception when parent declaration is null", () => new I1HeaderWrapper(entryHeaderWithNoParentDeclaration, messageSendingWrapperFactory));

		var entryHeaderWithNoEntryInstruction = Factory.New<JobDeclaration>().CustomsEntryHeaders.AddNew();
		AssertExceptionThrown<ArgumentNullException>("Expected exception when related Entry Instruction is null", () => new I1HeaderWrapper(entryHeaderWithNoEntryInstruction, messageSendingWrapperFactory));

		AssertExceptionThrown<ArgumentNullException>("Expected exception when messageSendingWrapperFactory is null", () => new I1HeaderWrapper(EntryHeader, null));
	}

	public override void TestAdditionalDeclarationType()
	{
		entryInstructionCustomsMessageWrapperMock.Setup(e => e.AdditionalDeclarationType).Returns(ZString.Empty);
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(II1Header.AdditionalDeclarationType), wrapper.AdditionalDeclarationType);

		entryInstructionCustomsMessageWrapperMock.Setup(e => e.AdditionalDeclarationType).Returns("D");
		wrapper = CreateWrapper();
		AssertEquals(nameof(II1Header.AdditionalDeclarationType), "D", wrapper.AdditionalDeclarationType);
	}

	public override void TestAdditionalInformation()
	{
		var wrapper = CreateWrapper();
		AssertNotNull(nameof(II1Header.AdditionalInformation), wrapper.AdditionalInformation);

		var additionalInfoMock = new Mock<IAdditionalInformation>();
		entryHeaderCustomsMessageWrapperMock.Setup(e => e.AdditionalInformation).Returns(new[] { additionalInfoMock.Object });

		wrapper = CreateWrapper();
		AssertEquals($"{nameof(II1Header.AdditionalInformation)} count", 1, wrapper.AdditionalInformation.Count);
	}

	public override void TestAdditionalSupplyChainActors()
	{
		var wrapper = CreateWrapper();
		AssertNotNull(nameof(II1Header.AdditionalSupplyChainActors), wrapper.AdditionalSupplyChainActors);
		AssertEquals(nameof(II1Header.AdditionalSupplyChainActors), 0, wrapper.AdditionalSupplyChainActors.Count);

		var additionalSupplyChainActor = new Mock<IAdditionalSupplyChainActor>();

		entryInstructionCustomsMessageWrapperMock.Setup(e => e.AdditionalSupplyChainActors).Returns(new[] { additionalSupplyChainActor.Object });
		wrapper = CreateWrapper();
		AssertEquals(nameof(II1Header.AdditionalSupplyChainActors), 1, wrapper.AdditionalSupplyChainActors.Count);
	}

	public override void TestAmendment()
	{
		var wrapper = CreateWrapper();
		AssertNull(nameof(II1Header.Amendment), wrapper.Amendment);

		var amendmentMock = new Mock<IDeclarationAmendment>();
		wrapper = new I1HeaderWrapper(EntryHeader, messageSendingWrapperFactory, amendmentMock.Object);
		AssertNotNull(nameof(II1Header.Amendment), wrapper.Amendment);
	}

	public override void TestAuthorizations()
	{
		var wrapper = CreateWrapper();
		AssertNotNull(nameof(II1Header.Authorizations), wrapper.Authorizations);
		AssertEquals($"{nameof(II1Header.Authorizations)} count", 0, wrapper.Authorizations.Count);

		var authorizationMock = new Mock<IAuthorization>();
		entryInstructionCustomsMessageWrapperMock.Setup(e => e.Authorizations).Returns(new[] { authorizationMock.Object });

		wrapper = CreateWrapper();
		var authorizations = wrapper.Authorizations;
		AssertEquals($"{nameof(II1Header.Authorizations)} count", 1, authorizations.Count);
	}

	public override void TestCountryOfDispatch()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(II1Header.CountryOfDispatch), wrapper.CountryOfDispatch);

		declarationCustomsMessageWrapperMock.Setup(d => d.GoodsCountryOfOrigin).Returns("DE");

		wrapper = CreateWrapper();
		AssertEquals(nameof(II1Header.CountryOfDispatch), "DE", wrapper.CountryOfDispatch);
	}

	public override void TestCustomsDutyPayerIdentificationNumber()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(II1Header.CustomsDutyPayerIdentificationNumber), wrapper.CustomsDutyPayerIdentificationNumber);

		declarationCustomsMessageWrapperMock.Setup(d => d.DutyPayerIdentificationNumber).Returns("ID1234");
		wrapper = CreateWrapper();
		AssertEquals(nameof(II1Header.CustomsDutyPayerIdentificationNumber), "ID1234", wrapper.CustomsDutyPayerIdentificationNumber);
	}

	public override void TestDeclarant()
	{
		var wrapper = CreateWrapper();
		AssertNull(nameof(II1Header.Declarant), wrapper.Declarant);

		var declarant = TraderWrapperTest.SetupTrader("DECLARANT");
		declarationCustomsMessageWrapperMock.Setup(d => d.ImportDeclarant).Returns(declarant.Object);

		wrapper = CreateWrapper();
		AssertNotNull(nameof(II1Header.Declarant), wrapper.Declarant);
	}

	public override void TestDeclarationCustomsOffice()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IH1Header.DeclarationCustomsOffice), wrapper.DeclarationCustomsOffice);

		declarationCustomsMessageWrapperMock.Setup(d => d.DeclarationCustomsOffice).Returns("000000");
		wrapper = CreateWrapper();
		AssertEquals(nameof(IH1Header.DeclarationCustomsOffice), "000000", wrapper.DeclarationCustomsOffice);
	}

	public override void TestDeclarationEntryStyle()
	{
		var i1HeaderWrapper = CreateWrapper();
		AssertNull(nameof(II1Header.DeclarationEntryStyle), i1HeaderWrapper.DeclarationEntryStyle);

		declarationCustomsMessageWrapperMock.Setup(d => d.EntryStyle).Returns("CO");
		i1HeaderWrapper = CreateWrapper();
		AssertEquals(nameof(II1Header.DeclarationEntryStyle), "CO", i1HeaderWrapper.DeclarationEntryStyle);
	}

	public override void TestGrossMass()
	{
		var wrapper = CreateWrapper();
		AssertEquals(nameof(II1Header.GrossMass), 0m, wrapper.GrossMass);

		entryHeaderCustomsMessageWrapperMock.Setup(e => e.GrossMass).Returns(23.0563m);

		wrapper = CreateWrapper();
		AssertEquals(nameof(II1Header.GrossMass), 23.0563m, wrapper.GrossMass);
	}

	public override void TestImporter()
	{
		var wrapper = CreateWrapper();
		AssertNull(nameof(II1Header.Importer), wrapper.Importer);

		var importer = TraderWrapperTest.SetupTrader("IMPORTER");
		declarationCustomsMessageWrapperMock.Setup(d => d.Importer).Returns(importer.Object);

		wrapper = CreateWrapper();
		AssertNotNull(nameof(II1Header.Importer), wrapper.Importer);
	}

	public override void TestInvoiceCurrencyCode()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(II1Header.InvoiceCurrencyCode), wrapper.InvoiceCurrencyCode);

		entryHeaderCustomsMessageWrapperMock.Setup(e => e.InvoiceCurrencyCode).Returns("AUD");

		wrapper = CreateWrapper();
		AssertEquals(nameof(II1Header.InvoiceCurrencyCode), "AUD", wrapper.InvoiceCurrencyCode);
	}

	public override void TestInvoiceTotalAmount()
	{
		var wrapper = CreateWrapper();
		AssertNull(nameof(II1Header.InvoiceTotalAmount), wrapper.InvoiceTotalAmount);

		entryHeaderCustomsMessageWrapperMock.Setup(e => e.InvoiceTotalAmount).Returns(334m);
		wrapper = CreateWrapper();
		AssertNull($"When Currency empty, {nameof(II1Header.InvoiceTotalAmount)}", wrapper.InvoiceTotalAmount);

		entryHeaderCustomsMessageWrapperMock.Setup(e => e.InvoiceCurrencyCode).Returns("AUD");
		wrapper = CreateWrapper();
		AssertEquals($"When Currency added, {nameof(II1Header.InvoiceTotalAmount)}", 334m, wrapper.InvoiceTotalAmount);
	}

	public override void TestLocationOfGoods()
	{
		var wrapper = CreateWrapper();
		AssertNull(nameof(II1Header.LocationOfGoods), wrapper.LocationOfGoods);

		var locationOfGoodsMock = new Mock<ILocationOfGoods>();
		declarationCustomsMessageWrapperMock.Setup(d => d.ImportLocationOfGoods).Returns(locationOfGoodsMock.Object);

		wrapper = CreateWrapper();
		AssertNotNull(nameof(II1Header.LocationOfGoods), wrapper.LocationOfGoods);
	}

	public override void TestLrn()
	{
		var i1HeaderWrapper = CreateWrapper();
		AssertEquals(nameof(II1Header.Lrn), ITEDIMessage.ITMessageNumberPlaceholder, i1HeaderWrapper.Lrn);
	}

	public override void TestPresentationCustomsOffice()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(II1Header.PresentationCustomsOffice), wrapper.PresentationCustomsOffice);

		declarationCustomsMessageWrapperMock.Setup(d => d.PresentationCustomsOffice).Returns("DE123123");

		wrapper = CreateWrapper();

		AssertEquals(nameof(II1Header.PresentationCustomsOffice),
			"DE123123",
			wrapper.PresentationCustomsOffice);
	}

	public override void TestPreviousDocuments()
	{
		var wrapper = CreateWrapper();
		AssertNotNull(nameof(II1Header.PreviousDocuments), wrapper.PreviousDocuments);
		AssertEquals($"{nameof(II1Header.PreviousDocuments)} count", 0, wrapper.PreviousDocuments.Count);

		var previousDocumentMock = new Mock<IPreviousDocument>();
		entryInstructionCustomsMessageWrapperMock.Setup(d => d.PreviousDocuments).Returns(new[] { previousDocumentMock.Object });

		wrapper = CreateWrapper();
		var previousDocumentWrappers = wrapper.PreviousDocuments;
		AssertEquals($"{nameof(II1Header.PreviousDocuments)} count", 1, previousDocumentWrappers.Count);
	}

	public override void TestRepresentative()
	{
		var wrapper = CreateWrapper();
		AssertNull(nameof(II1Header.Representative), wrapper.Representative);

		var representativeMock = new Mock<IRepresentative>();
		declarationCustomsMessageWrapperMock.Setup(d => d.Representative).Returns(representativeMock.Object);

		wrapper = CreateWrapper();
		AssertNotNull(nameof(II1Header.Representative), wrapper.Representative);
	}

	public override void TestSignerFiscalCode()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty($"When the current user does not have a COD certificate, {nameof(II1Header.SignerFiscalCode)}", wrapper.SignerFiscalCode);

		var currentUser = GlbStaff.CurrentUser;
		var codCertificate = GlbStaff.CurrentUser.Certificates.AddNew();
		codCertificate.XZ_Type = ItalyOrgCusCodeInfo.OrgCusCodes.CodiceFiscale;
		codCertificate.XZ_RefNumber = "";
		AssertNullOrEmpty($"When the current user has a COD certificate with empty number, {nameof(II1Header.SignerFiscalCode)}", wrapper.SignerFiscalCode);

		codCertificate.XZ_RefNumber = "ITCOD";
		wrapper = CreateWrapper();
		AssertEquals($"When the current user has a COD certificate with filled number, {nameof(II1Header.SignerFiscalCode)}", "ITCOD", wrapper.SignerFiscalCode);

		var automaticSignature = GlbStaffWrapper.Get(currentUser).AutomaticSignaturePasswordCollection.AddNew();
		automaticSignature.GP_Name = "REMOTE_ITCOD";
		automaticSignature.IsConfigurationActive = true;

		wrapper = CreateWrapper();
		AssertEquals($"When the current user has a valid automatic signature certificate with filled number, {nameof(IH1Header.SignerFiscalCode)}", "REMOTE_ITCOD", wrapper.SignerFiscalCode);

		automaticSignature.IsConfigurationActive = false;
		wrapper = CreateWrapper();
		AssertEquals($"When the current user has an automatic signature certificate but it is not valid, {nameof(IH1Header.SignerFiscalCode)}", "ITCOD", wrapper.SignerFiscalCode);
	}

	public override void TestSupervisingCustomsOffice()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty("Empty when SCO is not defined", wrapper.SupervisingCustomsOffice);

		declarationCustomsMessageWrapperMock.Setup(d => d.SupervisingCustomsOffice).Returns("LV002000");

		wrapper = CreateWrapper();
		AssertEquals(nameof(II1Header.SupervisingCustomsOffice), "LV002000", wrapper.SupervisingCustomsOffice);
	}

	public override void TestSupplier()
	{
		var wrapper = CreateWrapper();
		AssertNull(nameof(II1Header.Supplier), wrapper.Supplier);

		var supplier = TraderWrapperTest.SetupTrader("SUPPLIER");
		declarationCustomsMessageWrapperMock.Setup(d => d.Supplier).Returns(supplier.Object);

		wrapper = CreateWrapper();
		AssertNotNull(nameof(II1Header.Supplier), wrapper.Supplier);
	}

	public void TestSupplier_WhenExporterAddressIsNotSet()
	{
		var i1HeaderWrapper = CreateWrapper();
		AssertNull(nameof(II1Header.Supplier), i1HeaderWrapper.Supplier);
	}

	public override void TestSupportingDocuments()
	{
		var wrapper = CreateWrapper();
		AssertNotNull(nameof(II1Header.SupportingDocuments), wrapper.SupportingDocuments);
		AssertEquals($"{nameof(II1Header.SupportingDocuments)} count", 0, wrapper.SupportingDocuments.Count);

		var supportingDocumentMock = new Mock<MessageBuilder.ISupportingDocument>();
		entryInstructionCustomsMessageWrapperMock.Setup(d => d.SupportingDocuments).Returns(new[] { supportingDocumentMock.Object });

		wrapper = CreateWrapper();
		var supportingDocumentWrappers = wrapper.SupportingDocuments;
		AssertEquals($"{nameof(II1Header.SupportingDocuments)} count", 1, supportingDocumentWrappers.Count);
	}

	public override void TestUcr()
	{
		declarationCustomsMessageWrapperMock.Setup(d => d.Ucr).Returns(ZString.Empty);
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(II1Header.Ucr), wrapper.Ucr);

		declarationCustomsMessageWrapperMock.Setup(d => d.Ucr).Returns("UCR");
		AssertEquals(nameof(II1Header.Ucr), "UCR", wrapper.Ucr);
	}

	protected override II1Header CreateWrapper() => new I1HeaderWrapper(EntryHeader, messageSendingWrapperFactory);

	protected override void SetUp()
	{
		base.SetUp();

		entryInstruction = JobDeclaration.CustomsEntryInstructions.AddNew();
		EntryHeader.CH_CEI_Instruction = entryInstruction.PK;

		declarationCustomsMessageWrapperMock = new Mock<IJobDeclarationCustomsMessageWrapper>();
		entryHeaderCustomsMessageWrapperMock = new Mock<ICusEntryHeaderCustomsMessageWrapper>();
		entryInstructionCustomsMessageWrapperMock = new Mock<ICusEntryInstructionCustomsMessageWrapper>();

		messageSendingWrapperFactory = new MessageSendingWrapperFactoryMockBuilder()
			.ConfigureGetNewJobDeclarationCustomsMessageWrapper(declarationCustomsMessageWrapperMock.Object)
			.ConfigureGetNewCusEntryHeaderCustomsMessageWrapper(entryHeaderCustomsMessageWrapperMock.Object)
			.ConfigureGetNewCusEntryInstructionCustomsMessageWrapper(entryInstructionCustomsMessageWrapperMock.Object)
			.Build();
	}

	CusEntryInstruction entryInstruction;
	Mock<IJobDeclarationCustomsMessageWrapper> declarationCustomsMessageWrapperMock;
	Mock<ICusEntryHeaderCustomsMessageWrapper> entryHeaderCustomsMessageWrapperMock;
	Mock<ICusEntryInstructionCustomsMessageWrapper> entryInstructionCustomsMessageWrapperMock;
	CustomsMessageSending.IT.IMessageSendingWrapperFactory messageSendingWrapperFactory;
}
