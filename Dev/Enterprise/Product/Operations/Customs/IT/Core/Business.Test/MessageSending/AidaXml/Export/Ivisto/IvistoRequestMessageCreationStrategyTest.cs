using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Business.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Export.Testing;

sealed class IvistoRequestMessageCreationStrategyTest : TestCaseWithFactory
{
	[TestDate(2022, 10, 01)]
	public void TestGenerateMessage()
	{
		var mauCertificatePK = Guid.NewGuid();
		var messageGenerationContext = CreateMessageGenerationContext(mauCertificatePK);
		var messageGenerator = (IOutgoingCustomsMessageCreationStrategy)new IvistoRequestMessageCreationStrategy(Factory, messageGenerationContext);
		var generatedMessage = messageGenerator.GenerateMessage();

		var expectedMessage = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageSending.AidaXml.TestFiles.TestIvistoRequestMessage.xml");

		AssertNotNull("Generated Message", generatedMessage);
		CombineAssertions(() =>
		{
			AssertEquals(nameof(ITEDIMessage.EM_ApplicationCode), "ITH", generatedMessage.EM_ApplicationCode);
			AssertEquals(nameof(ITEDIMessage.EM_MessageType), "IVI", generatedMessage.EM_MessageType);
			AssertEquals(nameof(ITEDIMessage.EM_MessageSubType), "XXX", generatedMessage.EM_MessageSubType);
			AssertEquals(nameof(ITEDIMessage.EM_ReceiveTransmit), "TRX", generatedMessage.EM_ReceiveTransmit);
			AssertEquals(nameof(ITEDIMessage.EM_Status), "QUE", generatedMessage.EM_Status);
			AssertEquals(nameof(ITEDIMessage.EM_LinkedObject), entryHeader, generatedMessage.EM_LinkedObject);
			AssertEquals(nameof(ITEDIMessage.EM_GP), mauCertificatePK, generatedMessage.EM_GP);
			AssertEquals(nameof(ITEDIMessage.EM_ApplicationReference), "", generatedMessage.EM_ApplicationReference);
			AssertMultilineASCIIEquals(nameof(ITEDIMessage.EM_MessageText), expectedMessage, generatedMessage.EM_MessageText);
		});
	}

	public void TestGenerateMessage_InvalidContext()
	{
		var messageGenerationContextMock = new Mock<IIvistoRequestContext>();
		var messageGenerator = (IOutgoingCustomsMessageCreationStrategy)new IvistoRequestMessageCreationStrategy(Factory, messageGenerationContextMock.Object);

		AssertExceptionThrown<ArgumentNullException>("Entry Header", () => messageGenerator.GenerateMessage());
	}

	protected override void SetUp()
	{
		base.SetUp();
		entryHeader = Factory.New<JobDeclaration>().CustomsEntryHeaders.AddNew();
		entryHeader.MovementReferenceNumberSetter("23ITQ011T0000063E6");
	}

	CusEntryHeader entryHeader;

	IIvistoRequestContext CreateMessageGenerationContext(Guid mauCertificatePk)
	{
		var mauCertificateMock = new Mock<IGlbMauExternalPassword>();
		mauCertificateMock.Setup(m => m.PK).Returns(mauCertificatePk);
		mauCertificateMock.Setup(m => m.DeclarantTaxNumber).Returns("123454555");

		var contextMock = new Mock<IIvistoRequestContext>();
		contextMock.Setup(ctx => ctx.EntryHeader).Returns(entryHeader);
		contextMock.Setup(ctx => ctx.MauCertificate).Returns(mauCertificateMock.Object);

		return contextMock.Object;
	}
}
