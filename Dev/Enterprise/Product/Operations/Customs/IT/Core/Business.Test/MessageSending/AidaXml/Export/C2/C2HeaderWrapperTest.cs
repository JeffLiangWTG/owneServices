using System;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Customs.IT.MessageContracts.Declaration;
using CargoWise.Customs.IT.MessageContracts.Declaration.Export;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared.Testing;
using Enterprise.Customs.IT.Business.Testing;
using Moq;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Export.Testing;

sealed class C2HeaderWrapperTest : C2HeaderWrapperTestBase
{
	public override void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Expected exception when entryHeader is null", () => new C2HeaderWrapper(entryHeader: null, exportMessageSendingWrapperFactory));

		var entryHeaderWithNoParentDeclaration = Factory.New<CusEntryHeader>();
		entryHeaderWithNoParentDeclaration.CH_CEI_Instruction = Factory.New<CusEntryInstruction>().PK;
		AssertExceptionThrown<ArgumentNullException>("Expected exception when parent declaration is null", () => new C2HeaderWrapper(entryHeaderWithNoParentDeclaration, exportMessageSendingWrapperFactory));

		var entryHeaderWithNoEntryInstruction = Factory.New<JobDeclaration>().CustomsEntryHeaders.AddNew();
		AssertExceptionThrown<ArgumentNullException>("Expected exception when related Entry Instruction is null", () => new C2HeaderWrapper(entryHeaderWithNoEntryInstruction, exportMessageSendingWrapperFactory));

		AssertExceptionThrown<ArgumentNullException>("Expected exception when exportMessageSendingWrapperFactory is null", () => new C2HeaderWrapper(EntryHeader, null));
	}

	public override void TestAmendment()
	{
		var wrapper = CreateWrapper();
		AssertNull(nameof(IC2Header.Amendment), wrapper.Amendment);

		var amendmentMock = new Mock<IDeclarationAmendment>();
		wrapper = new C2HeaderWrapper(EntryHeader, exportMessageSendingWrapperFactory, amendmentMock.Object);
		AssertNotNull(nameof(IC2Header.Amendment), wrapper.Amendment);
	}

	public override void TestAuthorizations()
	{
		var wrapper = CreateWrapper();
		AssertNotNull(nameof(IC2Header.Authorizations), wrapper.Authorizations);
		AssertEquals($"{nameof(IC2Header.Authorizations)} count", 0, wrapper.Authorizations.Count);

		var authorizationMock = new Mock<IAuthorization>();
		entryInstructionCustomsMessageWrapperMock.Setup(e => e.Authorizations).Returns(new[] { authorizationMock.Object });

		wrapper = CreateWrapper();
		var authorizations = wrapper.Authorizations;
		AssertEquals($"{nameof(IC2Header.Authorizations)} count", 1, authorizations.Count);
	}

	public override void TestDeclarant()
	{
		var wrapper = CreateWrapper();
		AssertNull(nameof(IC2Header.Declarant), wrapper.Declarant);

		var declarant = TraderWrapperTest.SetupTrader("DECLARANT");
		declarationCustomsMessageWrapperMock.Setup(d => d.ExportDeclarant).Returns(declarant.Object);

		wrapper = CreateWrapper();
		AssertNotNull(nameof(IC2Header.Declarant), wrapper.Declarant);
	}

	public override void TestConsignmentRoutings()
	{
		var wrapper = CreateWrapper();
		AssertNotNull(nameof(IC2Header.ConsignmentRoutings), wrapper.ConsignmentRoutings);
		AssertEquals($"{nameof(IC2Header.ConsignmentRoutings)} count", 0, wrapper.ConsignmentRoutings.Count);

		var consignmentRoutingMock = new Mock<IConsignmentCountryRouting>();
		declarationCustomsMessageWrapperMock.Setup(x => x.ConsignmentRoutings).Returns(new[] { consignmentRoutingMock.Object });

		wrapper = CreateWrapper();
		AssertEquals(nameof(IC2Header.ConsignmentRoutings), 1, wrapper.ConsignmentRoutings.Count);
	}

	public override void TestDeclarationCustomsOffice()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IC2Header.DeclarationCustomsOffice), wrapper.DeclarationCustomsOffice);

		declarationCustomsMessageWrapperMock.Setup(d => d.DeclarationCustomsOffice).Returns("000000");
		wrapper = CreateWrapper();
		AssertEquals(nameof(IC2Header.DeclarationCustomsOffice), "000000", wrapper.DeclarationCustomsOffice);
	}

	public override void TestLocationOfGoods()
	{
		var wrapper = CreateWrapper();
		AssertNull(nameof(IC2Header.LocationOfGoods), wrapper.LocationOfGoods);

		var locationOfGoodsMock = new Mock<ILocationOfGoods>();
		declarationCustomsMessageWrapperMock.Setup(d => d.ExportLocationOfGoods).Returns(locationOfGoodsMock.Object);

		wrapper = CreateWrapper();
		AssertNotNull(nameof(IC2Header.LocationOfGoods), wrapper.LocationOfGoods);
	}

	public override void TestLrn()
	{
		var wrapper = CreateWrapper();
		AssertEquals(nameof(IC2Header.Lrn), ITEDIMessage.ITMessageNumberPlaceholder, wrapper.Lrn);
	}

	public override void TestPresentationCustomsOffice()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IC2Header.PresentationCustomsOffice), wrapper.PresentationCustomsOffice);

		declarationCustomsMessageWrapperMock.Setup(d => d.PresentationCustomsOffice).Returns("DE123123");

		wrapper = CreateWrapper();
		AssertEquals(nameof(IC2Header.PresentationCustomsOffice), "DE123123", wrapper.PresentationCustomsOffice);
	}

	public override void TestPreviousLrn()
	{
		const string previousLrn = "2024WTLDPL000000000291";
		var depositedDeclarationPositiveResponse = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.Export_DepositedDeclarationPositiveResponse.xml");
		var cancellationRejectedResponse = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.Export_CancellationRejectedResponse.xml");

		var wrapper = CreateWrapper();
		AssertEquals("Missing response message", string.Empty, wrapper.PreviousLrn);

		var depositedMessage = EntryHeader.Messages.AddNew();
		depositedMessage.EM_MessageType = "RES";
		depositedMessage.EM_SystemCreateTimeUtc = DateTime.UtcNow.AddDays(-1);
		depositedMessage.EM_MessageText = depositedDeclarationPositiveResponse;

		wrapper = CreateWrapper();
		AssertEquals("Deposited LRN response message", previousLrn, wrapper.PreviousLrn);

		var cancellationMessage = EntryHeader.Messages.AddNew();
		cancellationMessage.EM_MessageType = "RES";
		cancellationMessage.EM_SystemCreateTimeUtc = DateTime.UtcNow;
		cancellationMessage.EM_MessageText = cancellationRejectedResponse;

		wrapper = CreateWrapper();
		AssertEquals("Multiple response messages", previousLrn, wrapper.PreviousLrn);
	}

	public override void TestPreviousDocuments()
	{
		var wrapper = CreateWrapper();
		var previousDocuments = wrapper.PreviousDocuments;
		AssertNotNull(nameof(IC2Header.PreviousDocuments), previousDocuments);
		AssertEquals($"{nameof(IC2Header.PreviousDocuments)} Count", 0, previousDocuments.Count);

		var mockPreviousDocument = new Mock<IPreviousDocument>();
		entryInstructionCustomsMessageWrapperMock.Setup(e => e.PreviousDocuments).Returns(new[] { mockPreviousDocument.Object });

		wrapper = CreateWrapper();
		previousDocuments = wrapper.PreviousDocuments;
		AssertEquals($"{nameof(IC2Header.PreviousDocuments)} Count", 1, previousDocuments.Count);
	}

	public override void TestRepresentative()
	{
		var wrapper = CreateWrapper();
		AssertNull(nameof(IC2Header.Representative), wrapper.Representative);

		var representativeMock = new Mock<IRepresentative>();
		declarationCustomsMessageWrapperMock.Setup(d => d.Representative).Returns(representativeMock.Object);

		wrapper = CreateWrapper();
		AssertNotNull(nameof(IC2Header.Representative), wrapper.Representative);
	}

	public override void TestSpecificCircumstanceIndicator()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IC2Header.SpecificCircumstanceIndicator), wrapper.SpecificCircumstanceIndicator);

		declarationCustomsMessageWrapperMock.Setup(x => x.SpecificCircumstanceIndicator).Returns("A20");

		wrapper = CreateWrapper();
		AssertEquals(nameof(IC2Header.SpecificCircumstanceIndicator), "A20", wrapper.SpecificCircumstanceIndicator);
	}

	public override void TestTermsOfDelivery()
	{
		var wrapper = CreateWrapper();
		AssertNull(nameof(IC2Header.TermsOfDelivery), wrapper.TermsOfDelivery);

		var termsOfDeliveryMock = new Mock<ITermsOfDelivery>();
		entryHeaderCustomsMessageWrapperMock.Setup(d => d.TermOfDelivery).Returns(termsOfDeliveryMock.Object);

		AssertNotNull(nameof(IC2Header.TermsOfDelivery), wrapper.TermsOfDelivery);
	}

	public override void TestTransportChargesMethodOfPayment()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(IC2Header.TransportChargesMethodOfPayment), wrapper.TransportChargesMethodOfPayment);

		entryHeaderCustomsMessageWrapperMock.Setup(d => d.TransportChargesMethodOfPayment).Returns("A");

		wrapper = CreateWrapper();
		AssertEquals(nameof(IC2Header.TransportChargesMethodOfPayment), "A", wrapper.TransportChargesMethodOfPayment);
	}

	public override void TestAcceptanceDate()
	{
		var wrapper = CreateWrapper();
		AssertNull(nameof(IC2Header.AcceptanceDate), wrapper.AcceptanceDate);

		var testAcceptanceDate = new DateTime(2023, 03, 03);
		entryInstructionCustomsMessageWrapperMock.Setup(e => e.AcceptanceDate).Returns(testAcceptanceDate);

		wrapper = CreateWrapper();
		AssertEquals(nameof(IC2Header.AcceptanceDate), testAcceptanceDate, wrapper.AcceptanceDate);
	}

	protected override IC2Header CreateWrapper() => new C2HeaderWrapper(EntryHeader, exportMessageSendingWrapperFactory);

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
