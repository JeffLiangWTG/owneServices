using System;
using CargoWise.Customs.IT.MessageDefinitions.Declaration.Import.data;
using CargoWise.EntityFramework.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class DepositedDeclarationMessageHandlerTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("When adapter is null", () => new DepositedDeclarationMessageHandler(adapter: null));
	}

	[ExpectNoExceptions]
	public void TestHandleWhenResponseStatusIsDeposited()
	{
		adapterMock.Setup(x => x.SetStatusAsDeposited());
		adapterMock.Setup(x => x.SetLocalReferenceNumber(It.IsAny<string>()));

		var innerData = new Data() { Stato = 2, Lrn = "2024EDIDAT000000000001" };
		responseMessageHandler.Handle(innerData);
		adapterMock.Verify(x => x.SetStatusAsDeposited(), Times.Once());
		adapterMock.Verify(x => x.SetLocalReferenceNumber(innerData.Lrn), Times.Once());
	}

	[ExpectNoExceptions]
	public void TestHandleWhenResponseStatusIsNotDeposited()
	{
		adapterMock.Setup(x => x.SetStatusAsDeposited());
		adapterMock.Setup(x => x.SetLocalReferenceNumber(It.IsAny<string>()));

		var innerData = new Data() { Stato = 99, Lrn = "2024EDIDAT000000000001" };
		responseMessageHandler.Handle(innerData);
		adapterMock.Verify(x => x.SetStatusAsDeposited(), Times.Never());
		adapterMock.Verify(x => x.SetLocalReferenceNumber(It.IsAny<string>()), Times.Never());
	}

	protected override void SetUp()
	{
		base.SetUp();
		adapterMock = new Mock<IXmlCustomsLinkedObjectAdapter>();
		responseMessageHandler = new DepositedDeclarationMessageHandler(adapterMock.Object);
	}

	Mock<IXmlCustomsLinkedObjectAdapter> adapterMock;
	IResponseMessageHandler responseMessageHandler;
}
