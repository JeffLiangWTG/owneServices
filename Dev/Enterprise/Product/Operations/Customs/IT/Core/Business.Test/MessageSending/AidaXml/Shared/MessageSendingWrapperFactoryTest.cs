using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IT.Business.Declaration;
using static Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared.CustomsMessageSending.IT;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared.Testing;

sealed class MessageSendingWrapperFactoryTest : TestCaseWithFactory
{
	public void TestGetNewJobDeclarationCustomsMessageWrapper()
	{
		var declaration = Factory.New<JobDeclaration>();
		AssertType<JobDeclarationCustomsMessageWrapper>(nameof(IMessageSendingWrapperFactory.GetNewJobDeclarationCustomsMessageWrapper)
			, messageSendingWrapperFactory.GetNewJobDeclarationCustomsMessageWrapper(declaration));
	}

	public void TestGetNewCusEntryHeaderCustomsMessageWrapper()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		AssertType<CusEntryHeaderCustomsMessageWrapper>(nameof(IMessageSendingWrapperFactory.GetNewCusEntryHeaderCustomsMessageWrapper)
			, messageSendingWrapperFactory.GetNewCusEntryHeaderCustomsMessageWrapper(entryHeader));
	}

	public void TestGetNewCusEntryInstructionCustomsMessageWrapper()
	{
		var entryInstruction = Factory.New<JobDeclaration>()
			.CustomsEntryInstructions
			.AddNew();

		AssertType<CusEntryInstructionCustomsMessageWrapper>(nameof(IMessageSendingWrapperFactory.GetNewCusEntryInstructionCustomsMessageWrapper)
			, messageSendingWrapperFactory.GetNewCusEntryInstructionCustomsMessageWrapper(entryInstruction));
	}

	public void TestGetNewCusEntryLineCustomsMessageWrapper()
	{
		var entryLine = Factory.New<JobDeclaration>()
			.CustomsEntryHeaders
			.AddNew()
			.MergedLines
			.AddNew();

		AssertType<CusEntryLineCustomsMessageWrapper>(nameof(IMessageSendingWrapperFactory.GetNewCusEntryInstructionCustomsMessageWrapper)
			, messageSendingWrapperFactory.GetNewCusEntryLineCustomsMessageWrapper(entryLine));
	}

	public void TestMessageSendingWrapperFactoryIsConfigured()
	{
		IMessageSendingWrapperFactory messageSendingWrapperFactory = null;
		AssertNoExceptionThrown("Get IMessageSendingWrapperFactory", () => messageSendingWrapperFactory = ObjectFactory.Get<IMessageSendingWrapperFactory>());
		AssertType<MessageSendingWrapperFactory>(messageSendingWrapperFactory);
	}

	protected override void SetUp()
	{
		base.SetUp();
		messageSendingWrapperFactory = new MessageSendingWrapperFactory();
	}

	IMessageSendingWrapperFactory messageSendingWrapperFactory;
}
