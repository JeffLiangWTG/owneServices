using System;
using System.Linq;
using CargoWise.Customs.IT.MessageContracts.Declaration.Import;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared.Testing;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Import.Testing;

sealed class H4MessageWrapperTest : H4MessageWrapperTestBase
{
	public override void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new H4MessageWrapper(null, messageSendingWrapperFactory));
	}

	public override void TestHeader()
	{
		var wrapper = CreateWrapper();
		AssertType<H4HeaderWrapper>("Header Type", wrapper.Header);
	}

	public override void TestItems()
	{
		var wrapper = CreateWrapper();
		AssertEquals("Items Count", 1, wrapper.Items.Count);
		AssertType<H4ItemWrapper>("Item Type", wrapper.Items.Single());
	}
	protected override IH4Message CreateWrapper() => new H4MessageWrapper(EntryHeader, messageSendingWrapperFactory);

	protected override void SetUp()
	{
		base.SetUp();

		var entryInstruction = JobDeclaration.CustomsEntryInstructions.AddNew();
		EntryHeader.CH_CEI_Instruction = entryInstruction.PK;

		messageSendingWrapperFactory = new MessageSendingWrapperFactoryMockBuilder()
			.ConfigureAllMembers()
			.Build();
	}

	CustomsMessageSending.IT.IMessageSendingWrapperFactory messageSendingWrapperFactory;
}
