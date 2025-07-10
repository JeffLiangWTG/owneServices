using System;
using System.Linq;
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

sealed class H4HeaderWrapperTest : H4HeaderWrapperTestBase
{
	public override void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Expected exception when argument is null", () => new H4HeaderWrapper(entryHeader: null, messageSendingWrapperFactory));

		var entryHeaderWithNoParentDeclaration = Factory.New<CusEntryHeader>();
		entryHeaderWithNoParentDeclaration.CH_CEI_Instruction = Factory.New<CusEntryInstruction>().PK;
		AssertExceptionThrown<ArgumentNullException>("Expected exception when parent declaration is null", () => new H4HeaderWrapper(entryHeaderWithNoParentDeclaration, messageSendingWrapperFactory));

		var entryHeaderWithNoEntryInstruction = Factory.New<JobDeclaration>().CustomsEntryHeaders.AddNew();
		AssertExceptionThrown<ArgumentNullException>("Expected exception when related Entry Instruction is null", () => new H4HeaderWrapper(entryHeaderWithNoEntryInstruction, messageSendingWrapperFactory));

		AssertExceptionThrown<ArgumentNullException>("Expected exception when messageSendingWrapperFactory is null", () => new H1HeaderWrapper(EntryHeader, null));
	}

	public override void TestDeclarationCustomsOffice()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IH4Header.DeclarationCustomsOffice), wrapper.DeclarationCustomsOffice);

		declarationWrapperMock.Setup(d => d.DeclarationCustomsOffice).Returns("000000");
		wrapper = CreateWrapper();
		AssertEquals(nameof(IH4Header.DeclarationCustomsOffice), "000000", wrapper.DeclarationCustomsOffice);
	}

	public override void TestAdditionalDeclarationType()
	{
		entryInstructionWrapperMock.Setup(e => e.AdditionalDeclarationType).Returns(ZString.Empty);
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IH4Header.AdditionalDeclarationType), wrapper.AdditionalDeclarationType);

		entryInstructionWrapperMock.Setup(e => e.AdditionalDeclarationType).Returns("D");
		wrapper = CreateWrapper();
		AssertEquals(nameof(IH4Header.AdditionalDeclarationType), "D", wrapper.AdditionalDeclarationType);
	}

	public override void TestSignerFiscalCode()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty($"When the current user does not have a COD certificate, {nameof(IH4Header.SignerFiscalCode)}", wrapper.SignerFiscalCode);

		var currentUser = GlbStaff.CurrentUser;
		var codCertificate = currentUser.Certificates.AddNew();
		codCertificate.XZ_Type = ItalyOrgCusCodeInfo.OrgCusCodes.CodiceFiscale;
		codCertificate.XZ_RefNumber = "";
		AssertNullOrEmpty($"When the current user has a COD certificate with empty number, {nameof(IH4Header.SignerFiscalCode)}", wrapper.SignerFiscalCode);

		codCertificate.XZ_RefNumber = "ITCOD";
		wrapper = CreateWrapper();
		AssertEquals($"When the current user has a COD certificate with filled number, {nameof(IH4Header.SignerFiscalCode)}", "ITCOD", wrapper.SignerFiscalCode);

		var automaticSignature = GlbStaffWrapper.Get(currentUser).AutomaticSignaturePasswordCollection.AddNew();
		automaticSignature.GP_Name = "REMOTE_ITCOD";
		automaticSignature.IsConfigurationActive = true;

		wrapper = CreateWrapper();
		AssertEquals($"When the current user has a valid automatic signature certificate with filled number, {nameof(IH1Header.SignerFiscalCode)}", "REMOTE_ITCOD", wrapper.SignerFiscalCode);

		automaticSignature.IsConfigurationActive = false;
		wrapper = CreateWrapper();
		AssertEquals($"When the current user has an automatic signature certificate but it is not valid, {nameof(IH1Header.SignerFiscalCode)}", "ITCOD", wrapper.SignerFiscalCode);
	}

	public override void TestAmendment()
	{
		var wrapper = CreateWrapper();
		AssertNull(nameof(IH4Header.Amendment), wrapper.Amendment);

		wrapper = new H4HeaderWrapper(EntryHeader, messageSendingWrapperFactory, new Mock<IDeclarationAmendment>().Object);
		AssertNotNull(nameof(IH4Header.Amendment), wrapper.Amendment);
	}

	public override void TestPreviousDocuments()
	{
		var wrapper = CreateWrapper();
		AssertNotNull(nameof(IH4Header.PreviousDocuments), wrapper.PreviousDocuments);
		AssertEquals($"{nameof(IH4Header.PreviousDocuments)} count", 0, wrapper.PreviousDocuments.Count);

		var previousDocumentMock = new Mock<IPreviousDocument>();
		entryInstructionWrapperMock.Setup(d => d.PreviousDocuments).Returns(new[] { previousDocumentMock.Object });

		wrapper = CreateWrapper();
		var previousDocumentWrappers = wrapper.PreviousDocuments;
		AssertEquals($"{nameof(IH4Header.PreviousDocuments)} count", 1, previousDocumentWrappers.Count);
	}

	public override void TestAdditionalInformation()
	{
		var wrapper = CreateWrapper();
		AssertNotNull(nameof(IH4Header.AdditionalInformation), wrapper.AdditionalInformation);

		var additionalInfoMock = new Mock<IAdditionalInformation>();
		entryHeaderWrapperMock.Setup(e => e.AdditionalInformation).Returns(new[] { additionalInfoMock.Object });

		wrapper = CreateWrapper();
		AssertEquals($"{nameof(IH4Header.AdditionalInformation)} count", 1, wrapper.AdditionalInformation.Count);
	}

	public override void TestSupportingDocuments()
	{
		var wrapper = CreateWrapper();
		AssertNotNull(nameof(IH4Header.SupportingDocuments), wrapper.SupportingDocuments);
		AssertEquals($"{nameof(IH4Header.SupportingDocuments)} count", 0, wrapper.SupportingDocuments.Count);

		var supportingDocumentMock = new Mock<MessageBuilder.ISupportingDocument>();
		entryInstructionWrapperMock.Setup(d => d.SupportingDocuments).Returns(new[] { supportingDocumentMock.Object });

		wrapper = CreateWrapper();
		var supportingDocumentWrappers = wrapper.SupportingDocuments;
		AssertEquals($"{nameof(IH3Header.SupportingDocuments)} count", 1, supportingDocumentWrappers.Count);
	}

	public override void TestUcr()
	{
		declarationWrapperMock.Setup(d => d.Ucr).Returns(ZString.Empty);
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IH4Header.Ucr), wrapper.Ucr);

		declarationWrapperMock.Setup(d => d.Ucr).Returns("UCR");
		AssertEquals(nameof(IH4Header.Ucr), "UCR", wrapper.Ucr);
	}

	public override void TestLrn()
	{
		var wrapper = CreateWrapper();
		AssertEquals(nameof(IH4Header.Lrn), ITEDIMessage.ITMessageNumberPlaceholder, wrapper.Lrn);
	}

	public override void TestDeferredPayment()
	{
		entryHeaderWrapperMock.Setup(d => d.DeferredPayment).Returns(ZString.Empty);
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IH4Header.DeferredPayment), wrapper.DeferredPayment);

		entryHeaderWrapperMock.Setup(d => d.DeferredPayment).Returns("123456A");
		wrapper = CreateWrapper();
		AssertEquals(nameof(IH4Header.DeferredPayment), "123456A", wrapper.DeferredPayment);
	}

	public override void TestWarehouse()
	{
		var wrapper = CreateWrapper();
		AssertNull(nameof(IH4Header.Warehouse), wrapper.Warehouse);

		var warehouseMock = new Mock<IWarehouse>();
		entryInstructionWrapperMock.Setup(e => e.Warehouse).Returns(warehouseMock.Object);

		wrapper = CreateWrapper();
		AssertNotNull(nameof(IH4Header.Warehouse), wrapper.Warehouse);
	}

	public override void TestSupplier()
	{
		var wrapper = CreateWrapper();
		AssertNull(nameof(IH4Header.Supplier), wrapper.Supplier);

		var supplier = TraderWrapperTest.SetupTrader("SUPPLIER");
		declarationWrapperMock.Setup(d => d.Supplier).Returns(supplier.Object);

		wrapper = CreateWrapper();
		AssertNotNull(nameof(IH4Header.Supplier), wrapper.Supplier);
	}

	public override void TestImporter()
	{
		var wrapper = CreateWrapper();
		AssertNull(nameof(IH4Header.Importer), wrapper.Importer);

		var importer = TraderWrapperTest.SetupTrader("IMPORTER");
		declarationWrapperMock.Setup(d => d.Importer).Returns(importer.Object);

		wrapper = CreateWrapper();
		AssertNotNull(nameof(IH4Header.Importer), wrapper.Importer);
	}

	public override void TestDeclarant()
	{
		var wrapper = CreateWrapper();
		AssertNull(nameof(IH4Header.Declarant), wrapper.Declarant);

		var declarant = TraderWrapperTest.SetupTrader("DECLARANT");
		declarationWrapperMock.Setup(d => d.ImportDeclarant).Returns(declarant.Object);

		wrapper = CreateWrapper();
		AssertNotNull(nameof(IH4Header.Declarant), wrapper.Declarant);
	}

	public override void TestRepresentative()
	{
		var wrapper = CreateWrapper();
		AssertNull(nameof(IH4Header.Representative), wrapper.Representative);

		var representativeMock = new Mock<IRepresentative>();
		declarationWrapperMock.Setup(d => d.Representative).Returns(representativeMock.Object);

		wrapper = CreateWrapper();
		AssertNotNull(nameof(IH4Header.Representative), wrapper.Representative);
	}

	public override void TestAdditionalSupplyChainActors()
	{
		var wrapper = CreateWrapper();
		AssertNotNull(nameof(IH4Header.AdditionalSupplyChainActors), wrapper.AdditionalSupplyChainActors);
		AssertEquals(nameof(IH4Header.AdditionalSupplyChainActors), 0, wrapper.AdditionalSupplyChainActors.Count);

		var additionalSupplyChainActor = new Mock<IAdditionalSupplyChainActor>();

		entryInstructionWrapperMock.Setup(e => e.AdditionalSupplyChainActors).Returns(new[] { additionalSupplyChainActor.Object });
		wrapper = CreateWrapper();
		AssertEquals(nameof(IH4Header.AdditionalSupplyChainActors), 1, wrapper.AdditionalSupplyChainActors.Count);
	}

	public override void TestAuthorizations()
	{
		var wrapper = CreateWrapper();
		AssertNotNull(nameof(IH4Header.Authorizations), wrapper.Authorizations);
		AssertEquals($"{nameof(IH4Header.Authorizations)} count", 0, wrapper.Authorizations.Count);

		var authorizationMock = new Mock<IAuthorization>();
		entryInstructionWrapperMock.Setup(e => e.Authorizations).Returns(new[] { authorizationMock.Object });

		wrapper = CreateWrapper();
		var authorizations = wrapper.Authorizations;
		AssertEquals($"{nameof(IH4Header.Authorizations)} count", 1, authorizations.Count);
	}

	public override void TestGuaranteeHolderIdentificationNumber()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IH4Header.GuaranteeHolderIdentificationNumber), wrapper.GuaranteeHolderIdentificationNumber);

		entryInstructionWrapperMock.Setup(e => e.GetGuaranteeHolderIdentificationNumber(It.IsAny<IEoriTrader>())).Returns("IT1234766767");
		wrapper = CreateWrapper();
		AssertEquals(nameof(IH4Header.GuaranteeHolderIdentificationNumber), "IT1234766767", wrapper.GuaranteeHolderIdentificationNumber);
	}

	public override void TestTermsOfDelivery()
	{
		var wrapper = CreateWrapper();
		AssertNull(nameof(IH4Header.TermsOfDelivery), wrapper.TermsOfDelivery);

		var termsOfDeliveryMock = new Mock<ITermsOfDelivery>();
		entryHeaderWrapperMock.Setup(e => e.TermOfDelivery).Returns(termsOfDeliveryMock.Object);

		wrapper = CreateWrapper();
		AssertNotNull(nameof(IH4Header.TermsOfDelivery), wrapper.TermsOfDelivery);
	}

	public override void TestInvoiceCurrencyCode()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IH4Header.InvoiceCurrencyCode), wrapper.InvoiceCurrencyCode);

		entryHeaderWrapperMock.Setup(e => e.InvoiceCurrencyCode).Returns("AUD");
		wrapper = CreateWrapper();
		AssertEquals(nameof(IH4Header.InvoiceCurrencyCode), "AUD", wrapper.InvoiceCurrencyCode);
	}

	public override void TestInvoiceTotalAmount()
	{
		var wrapper = CreateWrapper();
		AssertEquals(nameof(IH4Header.InvoiceTotalAmount), 0m, wrapper.InvoiceTotalAmount);

		entryHeaderWrapperMock.Setup(e => e.InvoiceTotalAmount).Returns(334m);
		wrapper = CreateWrapper();
		AssertEquals(nameof(IH4Header.InvoiceTotalAmount), 334m, wrapper.InvoiceTotalAmount);
	}

	public override void TestInternalCurrencyUnit()
	{
		GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency = "";
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IH4Header.InternalCurrencyUnit), wrapper.InternalCurrencyUnit);

		GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency = "CHF";
		wrapper = CreateWrapper();
		AssertEquals(nameof(IH4Header.InternalCurrencyUnit), "CHF", wrapper.InternalCurrencyUnit);
	}

	public override void TestExchangeRate()
	{
		var wrapper = CreateWrapper();
		AssertEquals(nameof(IH4Header.ExchangeRate), 0m, wrapper.ExchangeRate);

		entryHeaderWrapperMock.Setup(e => e.ExchangeRate).Returns(1.1239m);
		wrapper = CreateWrapper();
		AssertEquals(nameof(IH4Header.ExchangeRate), 1.1239m, wrapper.ExchangeRate);
	}

	public override void TestCountryOfDestination()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IH4Header.CountryOfDestination), wrapper.CountryOfDestination);

		declarationWrapperMock.Setup(d => d.GoodsCountryOfDestination).Returns("TR");
		wrapper = CreateWrapper();
		AssertEquals(nameof(IH4Header.CountryOfDestination), "TR", wrapper.CountryOfDestination);
	}

	public override void TestRegionOfDestination()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IH4Header.RegionOfDestination), wrapper.RegionOfDestination);

		declarationWrapperMock.Setup(d => d.RegionOfDestination).Returns("XX");
		wrapper = CreateWrapper();
		AssertEquals(nameof(IH4Header.RegionOfDestination), "XX", wrapper.RegionOfDestination);
	}

	public override void TestCountryOfDispatch()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IH4Header.CountryOfDispatch), wrapper.CountryOfDispatch);

		declarationWrapperMock.Setup(d => d.GoodsCountryOfOrigin).Returns("DE");
		wrapper = CreateWrapper();
		AssertEquals(nameof(IH4Header.CountryOfDispatch), "DE", wrapper.CountryOfDispatch);
	}

	public override void TestLocationOfGoods()
	{
		var wrapper = CreateWrapper();
		AssertNull(nameof(IH4Header.LocationOfGoods), wrapper.LocationOfGoods);

		var locationOfGoodsMock = new Mock<ILocationOfGoods>();
		declarationWrapperMock.Setup(d => d.ImportLocationOfGoods).Returns(locationOfGoodsMock.Object);

		wrapper = CreateWrapper();
		AssertNotNull(nameof(IH4Header.LocationOfGoods), wrapper.LocationOfGoods);
	}

	public override void TestPresentationCustomsOffice()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IH4Header.PresentationCustomsOffice), wrapper.PresentationCustomsOffice);

		declarationWrapperMock.Setup(d => d.PresentationCustomsOffice).Returns("DE123123");
		wrapper = CreateWrapper();
		AssertEquals(nameof(IH4Header.PresentationCustomsOffice), "DE123123", wrapper.PresentationCustomsOffice);
	}

	public override void TestSupervisingCustomsOffice()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty("Empty when SCO is not defined", wrapper.SupervisingCustomsOffice);

		declarationWrapperMock.Setup(d => d.SupervisingCustomsOffice).Returns("LV002000");
		wrapper = CreateWrapper();
		AssertEquals(nameof(IH4Header.SupervisingCustomsOffice), "LV002000", wrapper.SupervisingCustomsOffice);
	}

	public override void TestAcceptanceDate()
	{
		var wrapper = CreateWrapper();
		AssertNull(nameof(IH4Header.AcceptanceDate), wrapper.AcceptanceDate);

		var testSimpleDecAcceptanceDate = new DateTime(2022, 01, 20);
		entryInstructionWrapperMock.Setup(e => e.AcceptanceDate).Returns(testSimpleDecAcceptanceDate);
		wrapper = CreateWrapper();
		AssertEquals(nameof(IH4Header.AcceptanceDate), testSimpleDecAcceptanceDate, wrapper.AcceptanceDate);
	}

	public override void TestGrossMass()
	{
		var wrapper = CreateWrapper();
		AssertEquals(nameof(IH4Header.GrossMass), 0m, wrapper.GrossMass);

		entryHeaderWrapperMock.Setup(e => e.GrossMass).Returns(23.0563m);
		wrapper = CreateWrapper();
		AssertEquals(nameof(IH4Header.GrossMass), 23.0563m, wrapper.GrossMass);
	}

	public override void TestNumberOfPackages()
	{
		var wrapper = CreateWrapper();
		AssertEquals(nameof(IH4Header.NumberOfPackages), 0, wrapper.NumberOfPackages);

		entryHeaderWrapperMock.Setup(e => e.NumberOfPackages).Returns(22);
		wrapper = CreateWrapper();
		AssertEquals(nameof(IH4Header.NumberOfPackages), 22, wrapper.NumberOfPackages);
	}

	public override void TestIsContainerizedTransport()
	{
		var wrapper = CreateWrapper();
		AssertEquals(nameof(IH4Header.IsContainerizedTransport), false, wrapper.IsContainerizedTransport);

		declarationWrapperMock.Setup(d => d.IsContainerizedTransport).Returns(true);
		wrapper = CreateWrapper();
		AssertEquals(nameof(IH4Header.IsContainerizedTransport), true, wrapper.IsContainerizedTransport);
	}

	public override void TestBorderMeansOfTransportMode()
	{
		var wrapper = CreateWrapper();
		AssertEquals(nameof(IH4Header.BorderMeansOfTransportMode), 0, wrapper.BorderMeansOfTransportMode);

		declarationWrapperMock.Setup(d => d.BorderTransportMode).Returns(4);
		wrapper = CreateWrapper();
		AssertEquals(nameof(IH4Header.BorderMeansOfTransportMode), 4, wrapper.BorderMeansOfTransportMode);
	}

	public override void TestInlandTransportMode()
	{
		var wrapper = CreateWrapper();
		AssertNull(nameof(IH4Header.InlandTransportMode), wrapper.InlandTransportMode);

		declarationWrapperMock.Setup(d => d.InlandTransportMode).Returns(1);
		wrapper = CreateWrapper();
		AssertEquals(nameof(IH4Header.InlandTransportMode), 1, wrapper.InlandTransportMode);
	}

	public override void TestArrivalMeansOfTransport()
	{
		var wrapper = CreateWrapper();
		AssertNull(nameof(IH4Header.ArrivalMeansOfTransport), wrapper.ArrivalMeansOfTransport);
	}

	public override void TestBorderMeansOfTransportNationality()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IH4Header.BorderMeansOfTransportNationality), wrapper.BorderMeansOfTransportNationality);

		declarationWrapperMock.Setup(d => d.BorderMeansOfTransportNationality).Returns("IT");
		wrapper = CreateWrapper();
		AssertEquals(nameof(IH4Header.BorderMeansOfTransportNationality), "IT", wrapper.BorderMeansOfTransportNationality);
	}

	public override void TestGuaranteeTypes()
	{
		var wrapper = CreateWrapper();
		AssertNotNull(nameof(IH4Header.GuaranteeTypes), wrapper.GuaranteeTypes);
		AssertEquals($"{nameof(IH4Header.GuaranteeTypes)} count", 0, wrapper.GuaranteeTypes.Count);

		entryInstructionWrapperMock.Setup(e => e.GuaranteeTypes).Returns(new[] { "A", "B", "C" });
		wrapper = CreateWrapper();
		var guaranteesTypeWrappers = wrapper.GuaranteeTypes;
		AssertEquals($"{nameof(IH4Header.GuaranteeTypes)} count", 3, guaranteesTypeWrappers.Count);
		AssertSame(nameof(IH4Header.GuaranteeTypes), guaranteesTypeWrappers, wrapper.GuaranteeTypes);
		AssertArrayEqualsByElements(nameof(IH4Header.GuaranteeTypes), new string[] { "A", "B", "C" }, guaranteesTypeWrappers.ToArray());
	}

	public override void TestGuarantees()
	{
		var wrapper = CreateWrapper();
		AssertNotNull(nameof(IH4Header.Guarantees), wrapper.Guarantees);
		AssertEquals($"{nameof(IH4Header.Guarantees)} count", 0, wrapper.Guarantees.Count);

		var guaranteeMock = new Mock<IGuarantee>();
		entryInstructionWrapperMock.Setup(e => e.Guarantees).Returns(new[] { guaranteeMock.Object });
		wrapper = CreateWrapper();
		var guaranteesWrappers = wrapper.Guarantees;
		AssertEquals($"{nameof(IH4Header.Guarantees)} count", 1, guaranteesWrappers.Count);
	}

	public override void TestNatureOfTransaction()
	{
		var wrapper = CreateWrapper();
		AssertEquals(nameof(IH4Header.NatureOfTransaction), 0, wrapper.NatureOfTransaction);

		entryHeaderWrapperMock.Setup(e => e.NatureOfTransaction).Returns(11);

		wrapper = CreateWrapper();
		AssertEquals(nameof(IH4Header.NatureOfTransaction), 11, wrapper.NatureOfTransaction);
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
			.ConfigureGetNewCusEntryHeaderCustomsMessageWrapper(entryHeaderWrapperMock.Object)
			.ConfigureGetNewCusEntryInstructionCustomsMessageWrapper(entryInstructionWrapperMock.Object)
			.Build();
	}

	Mock<IJobDeclarationCustomsMessageWrapper> declarationWrapperMock;
	Mock<ICusEntryHeaderCustomsMessageWrapper> entryHeaderWrapperMock;
	Mock<ICusEntryInstructionCustomsMessageWrapper> entryInstructionWrapperMock;
	CusEntryInstruction entryInstruction;
	CustomsMessageSending.IT.IMessageSendingWrapperFactory messageSendingWrapperFactory;

	protected override IH4Header CreateWrapper() => new H4HeaderWrapper(EntryHeader, messageSendingWrapperFactory);
}
