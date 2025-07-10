using Enterprise.Customs.IT.Business.Declaration;
using Moq;
using static Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared.CustomsMessageSending.IT;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared.Testing;

sealed class MessageSendingWrapperFactoryMockBuilder
{
	public MessageSendingWrapperFactoryMockBuilder()
	{
		messageSendingWrapperFactoryMock = new Mock<IMessageSendingWrapperFactory>();
	}

	readonly Mock<IMessageSendingWrapperFactory> messageSendingWrapperFactoryMock;

	public MessageSendingWrapperFactoryMockBuilder ConfigureAllMembers()
	{
		ConfigureGetNewJobDeclarationCustomsMessageWrapper();
		ConfigureGetNewCusEntryHeaderCustomsMessageWrapper();
		ConfigureGetNewCusEntryInstructionCustomsMessageWrapper();
		ConfigureGetNewCusEntryLineCustomsMessageWrapper();

		return this;
	}

	public MessageSendingWrapperFactoryMockBuilder ConfigureGetNewJobDeclarationCustomsMessageWrapper(IJobDeclarationCustomsMessageWrapper declarationCustomsMessageWrapper = null)
	{
		if (declarationCustomsMessageWrapper is null)
		{
			declarationCustomsMessageWrapper = new Mock<IJobDeclarationCustomsMessageWrapper>().Object;
		}

		messageSendingWrapperFactoryMock
			.Setup(x => x.GetNewJobDeclarationCustomsMessageWrapper(It.IsAny<JobDeclaration>()))
			.Returns(declarationCustomsMessageWrapper);

		return this;
	}

	public MessageSendingWrapperFactoryMockBuilder ConfigureGetNewCusEntryHeaderCustomsMessageWrapper(ICusEntryHeaderCustomsMessageWrapper entryHeaderCustomsMessageWrapper = null)
	{
		if (entryHeaderCustomsMessageWrapper is null)
		{
			entryHeaderCustomsMessageWrapper = new Mock<ICusEntryHeaderCustomsMessageWrapper>().Object;
		}

		messageSendingWrapperFactoryMock
			.Setup(x => x.GetNewCusEntryHeaderCustomsMessageWrapper(It.IsAny<CusEntryHeader>()))
			.Returns(entryHeaderCustomsMessageWrapper);

		return this;
	}

	public MessageSendingWrapperFactoryMockBuilder ConfigureGetNewCusEntryInstructionCustomsMessageWrapper(ICusEntryInstructionCustomsMessageWrapper entryInstructionCustomsMessageWrapper = null)
	{
		if (entryInstructionCustomsMessageWrapper is null)
		{
			entryInstructionCustomsMessageWrapper = new Mock<ICusEntryInstructionCustomsMessageWrapper>().Object;
		}

		messageSendingWrapperFactoryMock
			.Setup(x => x.GetNewCusEntryInstructionCustomsMessageWrapper(It.IsAny<CusEntryInstruction>()))
			.Returns(entryInstructionCustomsMessageWrapper);

		return this;
	}

	public MessageSendingWrapperFactoryMockBuilder ConfigureGetNewCusEntryLineCustomsMessageWrapper(ICusEntryLineCustomsMessageWrapper entryLineWrapperMock = null)
	{
		if (entryLineWrapperMock is null)
		{
			entryLineWrapperMock = new Mock<ICusEntryLineCustomsMessageWrapper>().Object;
		}

		messageSendingWrapperFactoryMock
			.Setup(x => x.GetNewCusEntryLineCustomsMessageWrapper(It.IsAny<CusEntryLine>()))
			.Returns(entryLineWrapperMock);

		return this;
	}

	public IMessageSendingWrapperFactory Build()
	{
		return messageSendingWrapperFactoryMock.Object;
	}
}
