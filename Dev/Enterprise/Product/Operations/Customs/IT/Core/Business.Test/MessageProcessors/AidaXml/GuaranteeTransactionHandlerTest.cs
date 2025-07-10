using System;
using CargoWise.Customs.IT.MessageDefinitions.Declaration.Import.data;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class GuaranteeTransactionHandlerTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("When customsLinkedObjectAdapter is null", () => new GuaranteeTransactionHandler(customsLinkedObjectAdapter: null, originalSentMessage: Factory.New<EDIMessage>()));
		AssertExceptionThrown<ArgumentNullException>("When originalSentMessage is null", () => new GuaranteeTransactionHandler(customsLinkedObjectAdapter: adapterMock.Object, originalSentMessage: null));
	}

	[ExpectNoExceptions]
	public void TestHandle()
	{
		responseMessageHandler.Handle(new Data { });
		adapterMock.As<IGuaranteeTransactionSupporter>().Verify(x => x.ConfirmPendingTransactions(It.IsAny<ZString>()), Times.Once());
	}

	protected override void SetUp()
	{
		base.SetUp();
		adapterMock = new Mock<IXmlCustomsLinkedObjectAdapter>();
		adapterMock.As<IGuaranteeTransactionSupporter>().Setup(x => x.ConfirmPendingTransactions(It.IsAny<ZString>()));
		responseMessageHandler = new GuaranteeTransactionHandler(adapterMock.Object, Factory.New<EDIMessage>());
	}

	Mock<IXmlCustomsLinkedObjectAdapter> adapterMock;
	IResponseMessageHandler responseMessageHandler;
}
