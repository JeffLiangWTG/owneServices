using System;
using System.Linq;
using CargoWise.Customs.IT.MessageContracts.Declaration;
using CargoWise.Customs.IT.MessageContracts.Declaration.Export;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared;
using Moq;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Export.Testing;

sealed class B4MessageWrapperTest : B4MessageWrapperTestBase
{
	public override void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("When EntryHeader is null", () => new B4MessageWrapper(entryHeader: null, exportMessageSendingWrapperFactory));
		AssertExceptionThrown<ArgumentNullException>("When ExportMessagingSendingWrapperFactory is null", () => new B4MessageWrapper(EntryHeader, exportMessageSendingWrapperFactory: null));
	}

	public override void TestMessageHeader()
	{
		var messageWrapper = CreateWrapper();
		AssertType<B4HeaderWrapper>("MessageHeader Type", messageWrapper.MessageHeader);
	}

	public override void TestItems()
	{
		var messageWrapper = CreateWrapper();
		var items = messageWrapper.Items;

		AssertNotNull("Items", items);
		AssertEquals("Items Count", 1, items.Count);
		AssertType<B4ItemWrapper>("Item Type", items.Single());
	}

	public void TestAmendment()
	{
		var messageWrapper = CreateWrapper();
		AssertNull("Amendment", messageWrapper.MessageHeader?.Amendment);

		var amendmentMock = new Mock<IDeclarationAmendment>();
		messageWrapper = new B4MessageWrapper(EntryHeader, exportMessageSendingWrapperFactory, amendmentMock.Object);
		AssertNotNull("Amendment", messageWrapper.MessageHeader?.Amendment);
	}

	protected override IB4Message CreateWrapper() => new B4MessageWrapper(EntryHeader, exportMessageSendingWrapperFactory);

	protected override void SetUp()
	{
		base.SetUp();

		var entryInstruction = JobDeclaration.CustomsEntryInstructions.AddNew();
		EntryHeader.CH_CEI_Instruction = entryInstruction.PK;

		exportMessageSendingWrapperFactory = new ExportMessageSendingWrapperFactoryMockBuilder()
			.ConfigureAllMembers()
			.Build();
	}

	CustomsMessageSending.IT.IExportMessageSendingWrapperFactory exportMessageSendingWrapperFactory;
}
