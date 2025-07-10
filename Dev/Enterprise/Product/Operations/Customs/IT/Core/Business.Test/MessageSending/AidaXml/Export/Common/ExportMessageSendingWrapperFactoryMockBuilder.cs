using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared;
using Moq;
using static Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared.CustomsMessageSending.IT;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Export.Testing;

sealed class ExportMessageSendingWrapperFactoryMockBuilder
{
	public ExportMessageSendingWrapperFactoryMockBuilder()
	{
		exportMessageSendingWrapperFactory = new Mock<IExportMessageSendingWrapperFactory>();
	}

	readonly Mock<IExportMessageSendingWrapperFactory> exportMessageSendingWrapperFactory;

	public ExportMessageSendingWrapperFactoryMockBuilder ConfigureAllMembers()
	{
		ConfigureGetNewJobDeclarationCustomsMessageWrapper();
		ConfigureGetNewCusEntryHeaderCustomsMessageWrapper();
		ConfigureGetNewCusEntryInstructionCustomsMessageWrapper();
		ConfigureGetNewCusEntryLineCustomsMessageWrapper();
		return this;
	}

	public ExportMessageSendingWrapperFactoryMockBuilder ConfigureGetNewJobDeclarationCustomsMessageWrapper(IJobDeclarationCustomsMessageWrapper jobDeclarationCustomsMessageWrapper = null)
	{
		var customsMessageWrapper = jobDeclarationCustomsMessageWrapper ?? new Mock<IJobDeclarationCustomsMessageWrapper>().Object;
		exportMessageSendingWrapperFactory.Setup(e => e.GetNewJobDeclarationCustomsMessageWrapper(It.IsAny<JobDeclaration>()))
			.Returns(customsMessageWrapper);
		return this;
	}

	public ExportMessageSendingWrapperFactoryMockBuilder ConfigureGetNewCusEntryHeaderCustomsMessageWrapper(ICusEntryHeaderCustomsMessageWrapper entryHeaderCustomsMessageWrapper = null)
	{
		var customsMessageWrapper = entryHeaderCustomsMessageWrapper ?? new Mock<ICusEntryHeaderCustomsMessageWrapper>().Object;
		exportMessageSendingWrapperFactory.Setup(e => e.GetNewCusEntryHeaderCustomsMessageWrapper(It.IsAny<CusEntryHeader>()))
			.Returns(customsMessageWrapper);
		return this;
	}

	public ExportMessageSendingWrapperFactoryMockBuilder ConfigureGetNewCusEntryInstructionCustomsMessageWrapper(IExportCusEntryInstructionCustomsMessageWrapper cusEntryInstructionCustomsMessageWrapper = null)
	{
		var customsMessageWrapper = cusEntryInstructionCustomsMessageWrapper ?? new Mock<IExportCusEntryInstructionCustomsMessageWrapper>().Object;
		exportMessageSendingWrapperFactory.Setup(e => e.GetNewCusEntryInstructionCustomsMessageWrapper(It.IsAny<CusEntryInstruction>()))
			.Returns(customsMessageWrapper);
		return this;
	}

	public ExportMessageSendingWrapperFactoryMockBuilder ConfigureGetNewCusEntryLineCustomsMessageWrapper(ICusEntryLineCustomsMessageWrapper entryLineCustomsMessageWrapper = null)
	{
		var customsMessageWrapper = entryLineCustomsMessageWrapper ?? new Mock<ICusEntryLineCustomsMessageWrapper>().Object;
		exportMessageSendingWrapperFactory.Setup(e => e.GetNewCusEntryLineCustomsMessageWrapper(It.IsAny<CusEntryLine>()))
			.Returns(customsMessageWrapper);
		return this;
	}

	public IExportMessageSendingWrapperFactory Build() => exportMessageSendingWrapperFactory.Object;
}
