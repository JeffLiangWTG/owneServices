using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.Customs.IT.NCTS.Business.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml.Testing;

sealed class IrildesRequestMessageCreationStrategyTest : TestCaseWithFactory
{
	[TestDate(2022, 10, 01)]
	public void TestGenerateMessage()
	{
		var expectedMessage = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.NCTS.Business.Testing.NCTS.MessageSending.AidaXml.TestFiles.TestIrildesRequestMessage.xml");

		var nctsHeader = Factory.NewDepartureNctsHeader();
		nctsHeader.BH_ApplicationCode = "NC5";
		nctsHeader.MovementReferenceEntryNumber.CE_EntryNum = "24ITQ0B8TEK17951J4";

		var mauCertificatePK = Guid.NewGuid();
		var mauCertificateMock = new Mock<IGlbMauExternalPassword>();
		mauCertificateMock
			.Setup(m => m.PK)
			.Returns(mauCertificatePK);
		mauCertificateMock
			.Setup(m => m.DeclarantTaxNumber)
			.Returns("13149600150");

		var messageGenerationContextMock = new Mock<IIrildesRequestContext>();
		messageGenerationContextMock
			.Setup(ctx => ctx.NctsHeader)
			.Returns(nctsHeader);
		messageGenerationContextMock
			.Setup(ctx => ctx.MauCertificate)
			.Returns(mauCertificateMock.Object);

		IOutgoingCustomsMessageCreationStrategy messageGenerator = new IrildesRequestMessageCreationStrategy(messageGenerationContextMock.Object);
		var generatedMessage = messageGenerator.GenerateMessage();

		AssertNotNull("Generated Message", generatedMessage);
		CombineAssertions(() =>
		{
			AssertEquals(nameof(ITEDIMessage.EM_ApplicationCode), "ITH", generatedMessage.EM_ApplicationCode);
			AssertEquals(nameof(ITEDIMessage.EM_MessageType), "IRI", generatedMessage.EM_MessageType);
			AssertEquals(nameof(ITEDIMessage.EM_MessageSubType), "XXX", generatedMessage.EM_MessageSubType);
			AssertEquals(nameof(ITEDIMessage.EM_ReceiveTransmit), "TRX", generatedMessage.EM_ReceiveTransmit);
			AssertEquals(nameof(ITEDIMessage.EM_Status), "QUE", generatedMessage.EM_Status);
			AssertEquals(nameof(ITEDIMessage.EM_LinkedObject), nctsHeader.MovementHeader, generatedMessage.EM_LinkedObject);
			AssertEquals(nameof(ITEDIMessage.EM_GP), mauCertificatePK, generatedMessage.EM_GP);
			AssertEquals(nameof(ITEDIMessage.EM_ApplicationReference), "", generatedMessage.EM_ApplicationReference);
			AssertMultilineASCIIEquals(nameof(ITEDIMessage.EM_MessageText), expectedMessage, generatedMessage.EM_MessageText);
		});
	}

	public void TestGenerateMessage_InvalidContext()
	{
		var messageGenerationContextMock = new Mock<IIrildesRequestContext>();
		IOutgoingCustomsMessageCreationStrategy messageGenerator = new IrildesRequestMessageCreationStrategy(messageGenerationContextMock.Object);

		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>("When NctsHeader is null", () => messageGenerator.GenerateMessage());

			messageGenerationContextMock
				.Setup(x => x.NctsHeader)
				.Returns(Factory.New<NctsHeader>());
			AssertExceptionThrown<ArgumentNullException>("When NctsHeader.MovementHeader is null", () => messageGenerator.GenerateMessage());
		});
	}
}
