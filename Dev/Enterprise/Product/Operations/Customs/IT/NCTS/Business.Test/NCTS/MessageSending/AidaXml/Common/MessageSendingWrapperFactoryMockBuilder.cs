using Moq;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml.Testing;

sealed class MessageSendingWrapperFactoryMockBuilder
{
	public MessageSendingWrapperFactoryMockBuilder()
	{
		messageSendingWrapperFactory = new Mock<IMessageSendingWrapperFactory>();
	}

	readonly Mock<IMessageSendingWrapperFactory> messageSendingWrapperFactory;

	public MessageSendingWrapperFactoryMockBuilder ConfigureGetNewHouseConsignmentCustomsMessageWrapper(IHouseConsignmentCustomsMessageWrapper houseConsignmentCustomsMessageWrapper = null)
	{
		var wrapper = houseConsignmentCustomsMessageWrapper ?? new Mock<IHouseConsignmentCustomsMessageWrapper>().Object;
		messageSendingWrapperFactory.Setup(f => f.GetNewHouseConsignmentCustomsMessageWrapper(It.IsAny<NctsBill>()))
			.Returns(wrapper);
		return this;
	}

	public MessageSendingWrapperFactoryMockBuilder ConfigureGetNewConsignmentItemCustomsMessageWrapper(IConsignmentItemCustomsMessageWrapper consignmentItemCustomsMessageWrapper = null)
	{
		var wrapper = consignmentItemCustomsMessageWrapper ?? new Mock<IConsignmentItemCustomsMessageWrapper>().Object;
		messageSendingWrapperFactory.Setup(f => f.GetNewConsignmentItemCustomsMessageWrapper(It.IsAny<NctsDepartureCargoDesc>()))
			.Returns(wrapper);
		return this;
	}

	public MessageSendingWrapperFactoryMockBuilder ConfigureAllMembers()
	{
		ConfigureGetNewHouseConsignmentCustomsMessageWrapper();
		ConfigureGetNewConsignmentItemCustomsMessageWrapper();
		return this;
	}

	public IMessageSendingWrapperFactory Build() => messageSendingWrapperFactory.Object;
}
