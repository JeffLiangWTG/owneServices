using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Business.Testing;

[TestsSubclassesOf(typeof(BaseMessageProcessor<>))]
abstract class BaseMessageProcessorAbstractTest<TProcessor, TDataProvider> : TestCaseWithFactory
	where TProcessor : BaseMessageProcessor<TDataProvider>
	where TDataProvider : class, IInboundMessageDataProvider
{
	protected override void SetUp()
	{
		base.SetUp();
		inboundMessage = Factory.New<AEEDIMessage>();
		inboundMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		processor = (TProcessor)Activator.CreateInstance(typeof(TProcessor));
		mockDataProvider = new Mock<TDataProvider>();
		mockDataProviderFactory = new Mock<IMessageDataProviderFactory>();
		mockDataProviderFactory.Setup(x => x.GetMessageDataProvider(inboundMessage)).Returns(mockDataProvider.Object);
		MessageDataProviderFactory.Instance.Value = mockDataProviderFactory.Object;
	}

	protected override void TearDown()
	{
		base.TearDown();
		MessageProcessorFactory.Instance.ResetValue();
	}

	protected Mock<IMessageDataProviderFactory> mockDataProviderFactory;
	protected Mock<TDataProvider> mockDataProvider;
	protected AEEDIMessage inboundMessage;
	protected TProcessor processor;
}
