using System;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Customs.IT.MessageContracts.Declaration;
using CargoWise.Customs.IT.MessageContracts.Declaration.Import;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared.Testing;
using Enterprise.MasterFiles.Business;
using Moq;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Import.Testing;

sealed class I2HeaderWrapperTest : I2HeaderWrapperTestBase
{
	public override void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Expected exception when entryHeader is null", () => new I2HeaderWrapper(entryHeader: null, messageSendingWrapperFactory));
		AssertExceptionThrown<ArgumentNullException>("Expected exception when messageSendingWrapperFactory is null", () => new I2HeaderWrapper(EntryHeader, null));
	}

	public override void TestAuthorizations()
	{
		var wrapper = CreateWrapper();
		AssertNotNull(nameof(II2Header.Authorizations), wrapper.Authorizations);
		AssertEquals($"{nameof(II2Header.Authorizations)} count", 0, wrapper.Authorizations.Count);

		var authorizationMock = new Mock<IAuthorization>();
		entryInstructionWrapperMock.Setup(e => e.Authorizations).Returns(new[] { authorizationMock.Object });

		wrapper = CreateWrapper();
		var authorizations = wrapper.Authorizations;
		AssertEquals($"{nameof(II2Header.Authorizations)} count", 1, authorizations.Count);
	}

	public override void TestDeclarationCustomsOffice()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(II2Header.DeclarationCustomsOffice), wrapper.DeclarationCustomsOffice);

		declarationWrapperMock.Setup(d => d.DeclarationCustomsOffice).Returns("000000");
		wrapper = CreateWrapper();
		AssertEquals(nameof(II2Header.DeclarationCustomsOffice), "000000", wrapper.DeclarationCustomsOffice);
	}

	public override void TestGrossMass()
	{
		var wrapper = CreateWrapper();
		AssertEquals(nameof(II2Header.GrossMass), 0m, wrapper.GrossMass);

		entryHeaderWrapperMock.Setup(e => e.GrossMass).Returns(23.0563m);
		wrapper = CreateWrapper();
		AssertEquals(nameof(II2Header.GrossMass), 23.0563m, wrapper.GrossMass);
	}

	public override void TestLocationOfGoods()
	{
		var wrapper = CreateWrapper();
		AssertNull(nameof(II2Header.LocationOfGoods), wrapper.LocationOfGoods);

		var locationOfGoodsMock = new Mock<ILocationOfGoods>();
		declarationWrapperMock.Setup(d => d.ImportLocationOfGoods).Returns(locationOfGoodsMock.Object);
		wrapper = CreateWrapper();
		AssertNotNull(nameof(II2Header.LocationOfGoods), wrapper.LocationOfGoods);
	}

	public override void TestLrn()
	{
		var wrapper = CreateWrapper();
		AssertEquals(nameof(II2Header.Lrn), ITEDIMessage.ITMessageNumberPlaceholder, wrapper.Lrn);
	}

	public override void TestPresentationCustomsOffice()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(II2Header.PresentationCustomsOffice), wrapper.PresentationCustomsOffice);

		declarationWrapperMock.Setup(d => d.PresentationCustomsOffice).Returns("DE123123");
		wrapper = CreateWrapper();
		AssertEquals(nameof(II2Header.PresentationCustomsOffice), "DE123123", wrapper.PresentationCustomsOffice);
	}

	public override void TestPreviousDocuments()
	{
		var wrapper = CreateWrapper();
		AssertNotNull(nameof(II2Header.PreviousDocuments), wrapper.PreviousDocuments);
		AssertEquals($"{nameof(II2Header.PreviousDocuments)} count", 0, wrapper.PreviousDocuments.Count);

		var previousDocumentMock = new Mock<IPreviousDocument>();
		entryInstructionWrapperMock.Setup(d => d.PreviousDocuments).Returns(new[] { previousDocumentMock.Object });

		wrapper = CreateWrapper();
		var previousDocumentWrappers = wrapper.PreviousDocuments;
		AssertEquals($"{nameof(II2Header.PreviousDocuments)} count", 1, previousDocumentWrappers.Count);
	}

	public override void TestRepresentative()
	{
		var wrapper = CreateWrapper();
		AssertNull(nameof(II2Header.Representative), wrapper.Representative);

		var representativeMock = new Mock<IRepresentative>();
		declarationWrapperMock.Setup(d => d.Representative).Returns(representativeMock.Object);

		wrapper = CreateWrapper();
		AssertNotNull(nameof(II2Header.Representative), wrapper.Representative);
	}

	public override void TestSignerFiscalCode()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty($"When the current user does not have a COD certificate, {nameof(II2Header.SignerFiscalCode)}", wrapper.SignerFiscalCode);

		var currentUser = GlbStaff.CurrentUser;
		var codCertificate = currentUser.Certificates.AddNew();
		codCertificate.XZ_Type = ItalyOrgCusCodeInfo.OrgCusCodes.CodiceFiscale;
		codCertificate.XZ_RefNumber = "";
		AssertNullOrEmpty($"When the current user has a COD certificate with empty number, {nameof(II2Header.SignerFiscalCode)}", wrapper.SignerFiscalCode);

		codCertificate.XZ_RefNumber = "ITCOD";
		wrapper = CreateWrapper();
		AssertEquals($"When the current user has a COD certificate with filled number, {nameof(II2Header.SignerFiscalCode)}", "ITCOD", wrapper.SignerFiscalCode);

		var automaticSignature = GlbStaffWrapper.Get(currentUser).AutomaticSignaturePasswordCollection.AddNew();
		automaticSignature.GP_Name = "REMOTE_ITCOD";
		automaticSignature.IsConfigurationActive = true;

		wrapper = CreateWrapper();
		AssertEquals($"When the current user has a valid automatic signature certificate with filled number, {nameof(IH1Header.SignerFiscalCode)}", "REMOTE_ITCOD", wrapper.SignerFiscalCode);

		automaticSignature.IsConfigurationActive = false;
		wrapper = CreateWrapper();
		AssertEquals($"When the current user has an automatic signature certificate but it is not valid, {nameof(IH1Header.SignerFiscalCode)}", "ITCOD", wrapper.SignerFiscalCode);
	}

	protected override II2Header CreateWrapper() => new I2HeaderWrapper(EntryHeader, messageSendingWrapperFactory);

	protected override void SetUp()
	{
		base.SetUp();
		var entryInstruction = JobDeclaration.CustomsEntryInstructions.AddNew();
		EntryHeader.CH_CEI_Instruction = entryInstruction.PK;

		declarationWrapperMock = new Mock<IJobDeclarationCustomsMessageWrapper>();
		entryInstructionWrapperMock = new Mock<ICusEntryInstructionCustomsMessageWrapper>();
		entryHeaderWrapperMock = new Mock<ICusEntryHeaderCustomsMessageWrapper>();

		messageSendingWrapperFactory = new MessageSendingWrapperFactoryMockBuilder()
			.ConfigureGetNewJobDeclarationCustomsMessageWrapper(declarationWrapperMock.Object)
			.ConfigureGetNewCusEntryHeaderCustomsMessageWrapper(entryHeaderWrapperMock.Object)
			.ConfigureGetNewCusEntryInstructionCustomsMessageWrapper(entryInstructionWrapperMock.Object)
			.Build();
	}

	Mock<IJobDeclarationCustomsMessageWrapper> declarationWrapperMock;
	Mock<ICusEntryInstructionCustomsMessageWrapper> entryInstructionWrapperMock;
	Mock<ICusEntryHeaderCustomsMessageWrapper> entryHeaderWrapperMock;
	CustomsMessageSending.IT.IMessageSendingWrapperFactory messageSendingWrapperFactory;
}
