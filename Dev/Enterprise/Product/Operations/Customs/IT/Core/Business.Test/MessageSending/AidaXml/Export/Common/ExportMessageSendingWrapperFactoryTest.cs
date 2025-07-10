using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared;
using static Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared.CustomsMessageSending.IT;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Export.Testing;

sealed class ExportMessageSendingWrapperFactoryTest : TestCaseWithFactory
{
	public void TestGetNewJobDeclarationCustomsMessageWrapper()
	{
		var declaration = Factory.New<JobDeclaration>();
		AssertType<JobDeclarationCustomsMessageWrapper>(nameof(IExportMessageSendingWrapperFactory.GetNewJobDeclarationCustomsMessageWrapper)
			, exportMessageSendingWrapperFactory.GetNewJobDeclarationCustomsMessageWrapper(declaration));
	}

	public void TestGetNewCusEntryHeaderCustomsMessageWrapper()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		AssertType<ExportCusEntryHeaderCustomsMessageWrapper>(nameof(IExportMessageSendingWrapperFactory.GetNewCusEntryHeaderCustomsMessageWrapper)
		, exportMessageSendingWrapperFactory.GetNewCusEntryHeaderCustomsMessageWrapper(entryHeader));
	}

	public void TestGetNewCusEntryInstructionCustomsMessageWrapper()
	{
		var entryInstruction = Factory.New<JobDeclaration>()
			.CustomsEntryInstructions
			.AddNew();

		AssertType<ExportCusEntryInstructionCustomsMessageWrapper>(nameof(IExportMessageSendingWrapperFactory.GetNewCusEntryInstructionCustomsMessageWrapper)
			, exportMessageSendingWrapperFactory.GetNewCusEntryInstructionCustomsMessageWrapper(entryInstruction));
	}

	public void TestGetNewCusEntryLineCustomsMessageWrapper()
	{
		var entryLine = Factory.New<JobDeclaration>()
			.CustomsEntryHeaders
			.AddNew()
			.MergedLines
			.AddNew();

		AssertType<CusEntryLineCustomsMessageWrapper>(nameof(IExportMessageSendingWrapperFactory.GetNewCusEntryInstructionCustomsMessageWrapper)
			, exportMessageSendingWrapperFactory.GetNewCusEntryLineCustomsMessageWrapper(entryLine));
	}

	public void TestExportMessageSendingWrapperFactoryIsConfigured()
	{
		IExportMessageSendingWrapperFactory messageSendingWrapperFactory = null;
		AssertNoExceptionThrown("Get IMessageSendingWrapperFactory", () => messageSendingWrapperFactory = ObjectFactory.Get<IExportMessageSendingWrapperFactory>());
		AssertType<ExportMessageSendingWrapperFactory>(messageSendingWrapperFactory);
	}

	protected override void SetUp()
	{
		base.SetUp();
		exportMessageSendingWrapperFactory = new ExportMessageSendingWrapperFactory();
	}

	IExportMessageSendingWrapperFactory exportMessageSendingWrapperFactory;
}
