using System;
using CargoWise.Customs.IT.MessageDefinitions.Declaration.Import.data;
using CargoWise.EntityFramework.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class UnderControlDeclarationMessageHandlerTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("When adapter is null", () => new UnderControlDeclarationMessageHandler(adapter: null));
	}

	[ExpectNoExceptions]
	public void TestHandleWhenResponseStatusIsUnderControl()
	{
		adapterMock.Setup(x => x.SetStatusAsUnderControl());

		var innerData = new Data() { Stato = 5 };
		responseMessageHandler.Handle(innerData);
		adapterMock.Verify(x => x.SetStatusAsUnderControl(), Times.Once());
	}

	[ExpectNoExceptions]
	public void TestHandleWhenResponseStatusIsNotUnderControl()
	{
		adapterMock.Setup(x => x.SetStatusAsUnderControl());

		var innerData = new Data() { Stato = 99 };
		responseMessageHandler.Handle(innerData);
		adapterMock.Verify(x => x.SetStatusAsUnderControl(), Times.Never());
	}

	protected override void SetUp()
	{
		base.SetUp();
		adapterMock = new Mock<IXmlCustomsLinkedObjectAdapter>();
		responseMessageHandler = new UnderControlDeclarationMessageHandler(adapterMock.Object);
	}

	Mock<IXmlCustomsLinkedObjectAdapter> adapterMock;
	IResponseMessageHandler responseMessageHandler;
}
