using System;
using System.Linq;
using CargoWise.Customs.IT.MessageContracts.Declaration.Import;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared.Testing;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Import.Testing;

sealed class H3MessageWrapperTest : H3MessageWrapperTestBase
{
	public override void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("When entryHeader is null", () => new H3MessageWrapper(null, messageSendingWrapperFactory));
		AssertExceptionThrown<ArgumentNullException>("When messageSendingWrapperFactory is null", () => new H3MessageWrapper(EntryHeader, null));
	}

	public override void TestHeader()
	{
		var wrapper = CreateWrapper();
		AssertType<H3HeaderWrapper>("Header Type", wrapper.Header);
	}

	public override void TestItems()
	{
		var wrapper = CreateWrapper();
		AssertEquals("Items Count", 1, wrapper.Items.Count);
		AssertType<H3ItemWrapper>("Item Type", wrapper.Items.Single());
	}

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

	protected override IH3Message CreateWrapper() => new H3MessageWrapper(EntryHeader, messageSendingWrapperFactory);
}
