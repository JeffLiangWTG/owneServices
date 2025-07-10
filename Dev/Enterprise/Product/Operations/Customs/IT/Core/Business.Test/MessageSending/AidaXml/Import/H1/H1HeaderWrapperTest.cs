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

sealed class H1HeaderWrapperTest : H1HeaderWrapperTestBase
{
	public override void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Expected exception when entryHeader is null", () => new H1HeaderWrapper(entryHeader: null, messageSendingWrapperFactory));

		var entryHeaderWithNoParentDeclaration = Factory.New<CusEntryHeader>();
		entryHeaderWithNoParentDeclaration.CH_CEI_Instruction = Factory.New<CusEntryInstruction>().PK;
		AssertExceptionThrown<ArgumentNullException>("Expected exception when parent declaration is null", () => new H1HeaderWrapper(entryHeaderWithNoParentDeclaration, messageSendingWrapperFactory));

		var entryHeaderWithNoEntryInstruction = Factory.New<JobDeclaration>().CustomsEntryHeaders.AddNew();
		AssertExceptionThrown<ArgumentNullException>("Expected exception when related Entry Instruction is null", () => new H1HeaderWrapper(entryHeaderWithNoEntryInstruction, messageSendingWrapperFactory));

		AssertExceptionThrown<ArgumentNullException>("Expected exception when messageSendingWrapperFactory is null", () => new H1HeaderWrapper(EntryHeader, null));
	}

	public override void TestAmendment()
	{
		var wrapper = CreateWrapper();
		AssertNull(nameof(IH1Header.Amendment), wrapper.Amendment);

		var amendmentMock = new Mock<IDeclarationAmendment>();
		wrapper = new H1HeaderWrapper(EntryHeader, messageSendingWrapperFactory, amendmentMock.Object);
		AssertNotNull(nameof(IH1Header.Amendment), wrapper.Amendment);
	}

	public override void TestDeclarationCustomsOffice()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IH1Header.DeclarationCustomsOffice), wrapper.DeclarationCustomsOffice);

		declarationCustomsMessageWrapperMock.Setup(d => d.DeclarationCustomsOffice).Returns("000000");
		wrapper = CreateWrapper();
		AssertEquals(nameof(IH1Header.DeclarationCustomsOffice), "000000", wrapper.DeclarationCustomsOffice);
	}

	public override void TestAdditionalDeclarationType()
	{
		entryInstructionCustomsMessageWrapperMock.Setup(e => e.AdditionalDeclarationType).Returns(ZString.Empty);
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IH1Header.AdditionalDeclarationType), wrapper.AdditionalDeclarationType);

		entryInstructionCustomsMessageWrapperMock.Setup(e => e.AdditionalDeclarationType).Returns("D");
		wrapper = CreateWrapper();
		AssertEquals(nameof(IH1Header.AdditionalDeclarationType), "D", wrapper.AdditionalDeclarationType);
	}

	public override void TestSeller()
	{
		var wrapper = CreateWrapper();
		AssertNull(nameof(IH1Header.Seller), wrapper.Seller);

		var sellerMock = TraderWrapperTest.SetupTrader("SELLER");
		declarationCustomsMessageWrapperMock.Setup(d => d.Seller).Returns(sellerMock.Object);

		wrapper = CreateWrapper();
		AssertNotNull(nameof(IH1Header.Seller), wrapper.Seller);
	}

	public override void TestSignerFiscalCode()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty($"When the current user does not have a COD certificate, {nameof(IH1Header.SignerFiscalCode)}", wrapper.SignerFiscalCode);

		var currentUser = GlbStaff.CurrentUser;
		var codCertificate = currentUser.Certificates.AddNew();
		codCertificate.XZ_Type = ItalyOrgCusCodeInfo.OrgCusCodes.CodiceFiscale;
		codCertificate.XZ_RefNumber = "";
		AssertNullOrEmpty($"When the current user has a COD certificate with empty number, {nameof(IH1Header.SignerFiscalCode)}", wrapper.SignerFiscalCode);

		codCertificate.XZ_RefNumber = "ITCOD";
		wrapper = CreateWrapper();
		AssertEquals($"When the current user has a COD certificate with filled number, {nameof(IH1Header.SignerFiscalCode)}", "ITCOD", wrapper.SignerFiscalCode);

		var automaticSignature = GlbStaffWrapper.Get(currentUser).AutomaticSignaturePasswordCollection.AddNew();
		automaticSignature.GP_Name = "REMOTE_ITCOD";
		automaticSignature.IsConfigurationActive = true;

		wrapper = CreateWrapper();
		AssertEquals($"When the current user has a valid automatic signature certificate with filled number, {nameof(IH1Header.SignerFiscalCode)}", "REMOTE_ITCOD", wrapper.SignerFiscalCode);

		automaticSignature.IsConfigurationActive = false;
		wrapper = CreateWrapper();
		AssertEquals($"When the current user has an automatic signature certificate but it is not valid, {nameof(IH1Header.SignerFiscalCode)}", "ITCOD", wrapper.SignerFiscalCode);
	}

	public override void TestLrn()
	{
		var wrapper = CreateWrapper();
		AssertEquals(nameof(IH1Header.Lrn), ITEDIMessage.ITMessageNumberPlaceholder, wrapper.Lrn);
	}

	public override void TestTermsOfDelivery()
	{
		var wrapper = CreateWrapper();
		AssertNull(nameof(IH1Header.TermsOfDelivery), wrapper.TermsOfDelivery);

		var termsOfDeliveryMock = new Mock<ITermsOfDelivery>();
		entryHeaderCustomsMessageWrapperMock.Setup(d => d.TermOfDelivery).Returns(termsOfDeliveryMock.Object);

		AssertNotNull(nameof(IH1Header.TermsOfDelivery), wrapper.TermsOfDelivery);
	}

	public override void TestUcr()
	{
		declarationCustomsMessageWrapperMock.Setup(d => d.Ucr).Returns(ZString.Empty);
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IH1Header.Ucr), wrapper.Ucr);

		declarationCustomsMessageWrapperMock.Setup(d => d.Ucr).Returns("UCR");
		AssertEquals(nameof(IH1Header.Ucr), "UCR", wrapper.Ucr);
	}

	public override void TestDeferredPayment()
	{
		entryHeaderCustomsMessageWrapperMock.Setup(d => d.DeferredPayment).Returns(ZString.Empty);
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IH1Header.DeferredPayment), wrapper.DeferredPayment);

		entryHeaderCustomsMessageWrapperMock.Setup(d => d.DeferredPayment).Returns("123456A");
		wrapper = CreateWrapper();
		AssertEquals(nameof(IH1Header.DeferredPayment), "123456A", wrapper.DeferredPayment);
	}

	public override void TestWarehouse()
	{
		var wrapper = CreateWrapper();
		AssertNull(nameof(IH1Header.Warehouse), wrapper.Warehouse);

		var warehouseMock = new Mock<IWarehouse>();
		entryInstructionCustomsMessageWrapperMock.Setup(e => e.Warehouse).Returns(warehouseMock.Object);

		wrapper = CreateWrapper();
		AssertNotNull(nameof(IH1Header.Warehouse), wrapper.Warehouse);
	}

	public override void TestPreviousDocuments()
	{
		var wrapper = CreateWrapper();
		AssertNotNull(nameof(IH1Header.PreviousDocuments), wrapper.PreviousDocuments);
		AssertEquals($"{nameof(IH1Header.PreviousDocuments)} count", 0, wrapper.PreviousDocuments.Count);

		var previousDocumentMock = new Mock<IPreviousDocument>();
		entryInstructionCustomsMessageWrapperMock.Setup(d => d.PreviousDocuments).Returns(new[] { previousDocumentMock.Object });

		wrapper = CreateWrapper();
		var previousDocumentWrappers = wrapper.PreviousDocuments;
		AssertEquals($"{nameof(IH1Header.PreviousDocuments)} count", 1, previousDocumentWrappers.Count);
	}

	public override void TestSupportingDocuments()
	{
		var wrapper = CreateWrapper();
		AssertNotNull(nameof(IH1Header.SupportingDocuments), wrapper.SupportingDocuments);
		AssertEquals($"{nameof(IH1Header.SupportingDocuments)} count", 0, wrapper.SupportingDocuments.Count);

		var supportingDocumentMock = new Mock<MessageBuilder.ISupportingDocument>();
		entryInstructionCustomsMessageWrapperMock.Setup(d => d.SupportingDocuments).Returns(new[] { supportingDocumentMock.Object });

		wrapper = CreateWrapper();
		var supportingDocumentWrappers = wrapper.SupportingDocuments;
		AssertEquals($"{nameof(IH1Header.SupportingDocuments)} count", 1, supportingDocumentWrappers.Count);
	}

	public override void TestAdditionalInformation()
	{
		var wrapper = CreateWrapper();
		AssertNotNull(nameof(IH1Header.AdditionalInformation), wrapper.AdditionalInformation);

		var additionalInfoMock = new Mock<IAdditionalInformation>();
		entryHeaderCustomsMessageWrapperMock.Setup(e => e.AdditionalInformation).Returns(new[] { additionalInfoMock.Object });

		wrapper = CreateWrapper();
		AssertEquals($"{nameof(IH1Header.AdditionalInformation)} count", 1, wrapper.AdditionalInformation.Count);
	}

	public override void TestAdditionOrDeductions()
	{
		var wrapper = CreateWrapper();
		AssertNotNull(nameof(IH1Header.AdditionOrDeductions), wrapper.AdditionOrDeductions);
		AssertEquals($"{nameof(IH1Header.AdditionOrDeductions)} count", 0, wrapper.AdditionOrDeductions.Count);
	}

	public override void TestSupplier()
	{
		var wrapper = CreateWrapper();
		AssertNull(nameof(IH1Header.Supplier), wrapper.Supplier);

		var supplier = TraderWrapperTest.SetupTrader("SUPPLIER");
		declarationCustomsMessageWrapperMock.Setup(d => d.Supplier).Returns(supplier.Object);

		wrapper = CreateWrapper();
		AssertNotNull(nameof(IH1Header.Supplier), wrapper.Supplier);
	}

	public override void TestImporter()
	{
		var wrapper = CreateWrapper();
		AssertNull(nameof(IH1Header.Importer), wrapper.Importer);

		var importer = TraderWrapperTest.SetupTrader("IMPORTER");
		declarationCustomsMessageWrapperMock.Setup(d => d.Importer).Returns(importer.Object);

		wrapper = CreateWrapper();
		AssertNotNull(nameof(IH1Header.Importer), wrapper.Importer);
	}

	public override void TestCustomsDutyPayerIdentificationNumber()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IH1Header.CustomsDutyPayerIdentificationNumber), wrapper.CustomsDutyPayerIdentificationNumber);

		declarationCustomsMessageWrapperMock.Setup(d => d.DutyPayerIdentificationNumber).Returns("ID1234");
		wrapper = CreateWrapper();
		AssertEquals(nameof(IH1Header.CustomsDutyPayerIdentificationNumber), "ID1234", wrapper.CustomsDutyPayerIdentificationNumber);
	}

	public override void TestDeclarant()
	{
		var wrapper = CreateWrapper();
		AssertNull(nameof(IH1Header.Declarant), wrapper.Declarant);

		var declarant = TraderWrapperTest.SetupTrader("DECLARANT");
		declarationCustomsMessageWrapperMock.Setup(d => d.ImportDeclarant).Returns(declarant.Object);

		wrapper = CreateWrapper();
		AssertNotNull(nameof(IH1Header.Declarant), wrapper.Declarant);
	}

	public override void TestRepresentative()
	{
		var wrapper = CreateWrapper();
		AssertNull(nameof(IH1Header.Representative), wrapper.Representative);

		var representativeMock = new Mock<IRepresentative>();
		declarationCustomsMessageWrapperMock.Setup(d => d.Representative).Returns(representativeMock.Object);

		wrapper = CreateWrapper();
		AssertNotNull(nameof(IH1Header.Representative), wrapper.Representative);
	}

	public override void TestAdditionalSupplyChainActors()
	{
		var wrapper = CreateWrapper();
		AssertNotNull(nameof(IH1Header.AdditionalSupplyChainActors), wrapper.AdditionalSupplyChainActors);
		AssertEquals(nameof(IH1Header.AdditionalSupplyChainActors), 0, wrapper.AdditionalSupplyChainActors.Count);

		var additionalSupplyChainActor = new Mock<IAdditionalSupplyChainActor>();

		entryInstructionCustomsMessageWrapperMock.Setup(e => e.AdditionalSupplyChainActors).Returns(new[] { additionalSupplyChainActor.Object });
		wrapper = CreateWrapper();
		AssertEquals(nameof(IH1Header.AdditionalSupplyChainActors), 1, wrapper.AdditionalSupplyChainActors.Count);
	}

	public override void TestGuaranteeHolderIdentificationNumber()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IH1Header.GuaranteeHolderIdentificationNumber), wrapper.GuaranteeHolderIdentificationNumber);

		entryInstructionCustomsMessageWrapperMock.Setup(e => e.GetGuaranteeHolderIdentificationNumber(It.IsAny<IEoriTrader>())).Returns("IT1234766767");
		wrapper = CreateWrapper();
		AssertEquals(nameof(IH1Header.GuaranteeHolderIdentificationNumber), "IT1234766767", wrapper.GuaranteeHolderIdentificationNumber);
	}

	public override void TestAuthorizations()
	{
		var wrapper = CreateWrapper();
		AssertNotNull(nameof(IH1Header.Authorizations), wrapper.Authorizations);
		AssertEquals($"{nameof(IH1Header.Authorizations)} count", 0, wrapper.Authorizations.Count);

		var authorizationMock = new Mock<IAuthorization>();
		entryInstructionCustomsMessageWrapperMock.Setup(e => e.Authorizations).Returns(new[] { authorizationMock.Object });

		wrapper = CreateWrapper();
		var authorizations = wrapper.Authorizations;
		AssertEquals($"{nameof(IH1Header.Authorizations)} count", 1, authorizations.Count);
	}

	public override void TestBorderMeansOfTransportMode()
	{
		var wrapper = CreateWrapper();
		AssertEquals(nameof(IH1Header.BorderMeansOfTransportMode), 0, wrapper.BorderMeansOfTransportMode);

		declarationCustomsMessageWrapperMock.Setup(d => d.BorderTransportMode).Returns(22);
		AssertEquals(nameof(IH1Header.BorderMeansOfTransportMode), 22, wrapper.BorderMeansOfTransportMode);
	}

	public override void TestInvoiceCurrencyCode()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IH1Header.InvoiceCurrencyCode), wrapper.InvoiceCurrencyCode);

		entryHeaderCustomsMessageWrapperMock.Setup(e => e.InvoiceCurrencyCode).Returns("AUD");

		wrapper = CreateWrapper();
		AssertEquals(nameof(IH1Header.InvoiceCurrencyCode), "AUD", wrapper.InvoiceCurrencyCode);
	}

	public override void TestInvoiceTotalAmount()
	{
		var wrapper = CreateWrapper();
		AssertNull(nameof(IH1Header.InvoiceTotalAmount), wrapper.InvoiceTotalAmount);

		entryHeaderCustomsMessageWrapperMock.Setup(e => e.InvoiceTotalAmount).Returns(334m);

		wrapper = CreateWrapper();
		AssertEquals(nameof(IH1Header.InvoiceTotalAmount), 334m, wrapper.InvoiceTotalAmount);
	}

	public override void TestIsContainerizedTransport()
	{
		var wrapper = CreateWrapper();
		AssertEquals(nameof(IH1Header.IsContainerizedTransport), false, wrapper.IsContainerizedTransport);

		declarationCustomsMessageWrapperMock.Setup(d => d.ContainerModeForImportMessage).Returns(1);
		AssertEquals(nameof(IH1Header.IsContainerizedTransport), true, wrapper.IsContainerizedTransport);

		declarationCustomsMessageWrapperMock.Setup(d => d.ContainerModeForImportMessage).Returns(2);
		AssertEquals(nameof(IH1Header.IsContainerizedTransport), false, wrapper.IsContainerizedTransport);
	}

	public override void TestInternalCurrencyUnit()
	{
		GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency = "";
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IH1Header.InternalCurrencyUnit), wrapper.InternalCurrencyUnit);

		GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency = "CHF";
		wrapper = CreateWrapper();
		AssertEquals(nameof(IH1Header.InternalCurrencyUnit), "CHF", wrapper.InternalCurrencyUnit);
	}

	public override void TestExchangeRate()
	{
		var wrapper = CreateWrapper();
		AssertEquals(nameof(IH1Header.ExchangeRate), 0m, wrapper.ExchangeRate);

		entryHeaderCustomsMessageWrapperMock.Setup(e => e.ExchangeRate).Returns(1.1239m);

		wrapper = CreateWrapper();
		AssertEquals(nameof(IH1Header.ExchangeRate), 1.1239m, wrapper.ExchangeRate);
	}

	public override void TestFiscalReferences()
	{
		var wrapper = CreateWrapper();
		AssertNotNull(nameof(IH1Header.FiscalReferences), wrapper.FiscalReferences);
		AssertEquals($"{nameof(IH1Header.FiscalReferences)} count", 0, wrapper.FiscalReferences.Count);

		var fiscalReferenceMock = new Mock<IFiscalReference>();
		entryInstructionCustomsMessageWrapperMock.Setup(e => e.FiscalReferences)
			.Returns(new[] { fiscalReferenceMock.Object });

		wrapper = CreateWrapper();
		AssertNotNull(nameof(IH1Header.FiscalReferences), wrapper.FiscalReferences);
		AssertEquals($"{nameof(IH1Header.FiscalReferences)} count", 1, wrapper.FiscalReferences.Count);
	}

	public override void TestCountryOfDestination()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IH1Header.CountryOfDestination), wrapper.CountryOfDestination);

		declarationCustomsMessageWrapperMock.Setup(d => d.GoodsCountryOfDestination).Returns("TR");

		wrapper = CreateWrapper();
		AssertEquals(nameof(IH1Header.CountryOfDestination), "TR", wrapper.CountryOfDestination);
	}

	public override void TestRegionOfDestination()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IH1Header.RegionOfDestination), wrapper.RegionOfDestination);

		declarationCustomsMessageWrapperMock.Setup(d => d.RegionOfDestination).Returns("XX");

		wrapper = CreateWrapper();
		AssertEquals(nameof(IH1Header.RegionOfDestination), "XX", wrapper.RegionOfDestination);
	}

	public override void TestCountryOfDispatch()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IH1Header.CountryOfDispatch), wrapper.CountryOfDispatch);

		declarationCustomsMessageWrapperMock.Setup(d => d.GoodsCountryOfOrigin).Returns("DE");

		wrapper = CreateWrapper();
		AssertEquals(nameof(IH1Header.CountryOfDispatch), "DE", wrapper.CountryOfDispatch);
	}

	public override void TestLocationOfGoods()
	{
		var wrapper = CreateWrapper();
		AssertNull(nameof(IH1Header.LocationOfGoods), wrapper.LocationOfGoods);

		var locationOfGoodsMock = new Mock<ILocationOfGoods>();
		declarationCustomsMessageWrapperMock.Setup(d => d.ImportLocationOfGoods).Returns(locationOfGoodsMock.Object);

		wrapper = CreateWrapper();
		AssertNotNull(nameof(IH1Header.LocationOfGoods), wrapper.LocationOfGoods);
	}

	public override void TestPresentationCustomsOffice()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IH1Header.PresentationCustomsOffice), wrapper.PresentationCustomsOffice);

		declarationCustomsMessageWrapperMock.Setup(d => d.PresentationCustomsOffice).Returns("DE123123");

		wrapper = CreateWrapper();

		AssertEquals(nameof(IH1Header.PresentationCustomsOffice),
			"DE123123",
			wrapper.PresentationCustomsOffice);
	}

	public override void TestSupervisingCustomsOffice()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty("Empty when SCO is not defined", wrapper.SupervisingCustomsOffice);

		declarationCustomsMessageWrapperMock.Setup(d => d.SupervisingCustomsOffice).Returns("LV002000");

		wrapper = CreateWrapper();
		AssertEquals(nameof(IH1Header.SupervisingCustomsOffice), "LV002000", wrapper.SupervisingCustomsOffice);
	}

	public override void TestAcceptanceDate()
	{
		var wrapper = CreateWrapper();
		AssertNull(nameof(IH1Header.AcceptanceDate), wrapper.AcceptanceDate);

		var testSimpleDecAcceptanceDate = new DateTime(2022, 01, 20);
		entryInstructionCustomsMessageWrapperMock.Setup(e => e.AcceptanceDate).Returns(testSimpleDecAcceptanceDate);

		wrapper = CreateWrapper();
		AssertEquals(nameof(IH1Header.AcceptanceDate), testSimpleDecAcceptanceDate, wrapper.AcceptanceDate);
	}

	public override void TestGrossMass()
	{
		var wrapper = CreateWrapper();
		AssertEquals(nameof(IH1Header.GrossMass), 0m, wrapper.GrossMass);

		entryHeaderCustomsMessageWrapperMock.Setup(e => e.GrossMass).Returns(23.0563m);

		wrapper = CreateWrapper();
		AssertEquals(nameof(IH1Header.GrossMass), 23.0563m, wrapper.GrossMass);
	}

	public override void TestNumberOfPackages()
	{
		var wrapper = CreateWrapper();
		AssertEquals(nameof(IH1Header.NumberOfPackages), 0, wrapper.NumberOfPackages);

		entryHeaderCustomsMessageWrapperMock.Setup(e => e.NumberOfPackages).Returns(22);

		wrapper = CreateWrapper();
		AssertEquals(nameof(IH1Header.NumberOfPackages), 22, wrapper.NumberOfPackages);
	}

	public override void TestInlandTransportMode()
	{
		var wrapper = CreateWrapper();
		AssertNull(nameof(IH1Header.InlandTransportMode), wrapper.InlandTransportMode);

		declarationCustomsMessageWrapperMock.Setup(d => d.InlandTransportMode).Returns(1);

		wrapper = CreateWrapper();
		AssertEquals(nameof(IH1Header.InlandTransportMode), 1, wrapper.InlandTransportMode);
	}

	public override void TestBuyer()
	{
		var wrapper = CreateWrapper();
		AssertNull(nameof(IH1Header.Buyer), wrapper.Buyer);

		var buyer = TraderWrapperTest.SetupTrader("BUYER");
		declarationCustomsMessageWrapperMock.Setup(d => d.Buyer).Returns(buyer.Object);

		wrapper = CreateWrapper();
		AssertNotNull(nameof(IH1Header.Buyer), wrapper.Buyer);
	}

	public override void TestBorderMeansOfTransportNationality()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IH1Header.BorderMeansOfTransportNationality), wrapper.BorderMeansOfTransportNationality);

		declarationCustomsMessageWrapperMock.Setup(d => d.BorderMeansOfTransportNationality).Returns("IT");

		wrapper = CreateWrapper();
		AssertEquals(nameof(IH1Header.BorderMeansOfTransportNationality), "IT", wrapper.BorderMeansOfTransportNationality);
	}

	public override void TestArrivalMeansOfTransport()
	{
		var wrapper = CreateWrapper();
		AssertNull(nameof(IH1Header.ArrivalMeansOfTransport), wrapper.ArrivalMeansOfTransport);
	}

	public override void TestGuarantees()
	{
		var wrapper = CreateWrapper();
		AssertNotNull(nameof(IH1Header.Guarantees), wrapper.Guarantees);
		AssertEquals($"{nameof(IH1Header.Guarantees)} count", 0, wrapper.Guarantees.Count);

		var guaranteeMock = new Mock<IGuarantee>();
		entryInstructionCustomsMessageWrapperMock.Setup(e => e.Guarantees).Returns(new[] { guaranteeMock.Object });

		wrapper = CreateWrapper();
		var guaranteesWrappers = wrapper.Guarantees;
		AssertEquals($"{nameof(IH1Header.Guarantees)} count", 1, guaranteesWrappers.Count);
	}

	public override void TestGuaranteeTypes()
	{
		var wrapper = CreateWrapper();
		AssertNotNull(nameof(IH1Header.GuaranteeTypes), wrapper.GuaranteeTypes);
		AssertEquals($"{nameof(IH1Header.GuaranteeTypes)} count", 0, wrapper.GuaranteeTypes.Count);

		entryInstructionCustomsMessageWrapperMock.Setup(e => e.GuaranteeTypes).Returns(new[] { "A", "B", "C" });

		wrapper = CreateWrapper();
		var guaranteesTypeWrappers = wrapper.GuaranteeTypes;
		AssertEquals($"{nameof(IH1Header.GuaranteeTypes)} count", 3, guaranteesTypeWrappers.Count);
		AssertSame(nameof(IH1Header.GuaranteeTypes), guaranteesTypeWrappers, wrapper.GuaranteeTypes);
		AssertArrayEqualsByElements(nameof(IH1Header.GuaranteeTypes), new string[] { "A", "B", "C" }, guaranteesTypeWrappers.ToArray());
	}

	public override void TestNatureOfTransaction()
	{
		var wrapper = CreateWrapper();
		AssertEquals(nameof(IH1Header.NatureOfTransaction), 0, wrapper.NatureOfTransaction);

		entryHeaderCustomsMessageWrapperMock.Setup(e => e.NatureOfTransaction).Returns(11);

		wrapper = CreateWrapper();
		AssertEquals(nameof(IH1Header.NatureOfTransaction), 11, wrapper.NatureOfTransaction);
	}

	protected override IH1Header CreateWrapper() => new H1HeaderWrapper(EntryHeader, messageSendingWrapperFactory);

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
