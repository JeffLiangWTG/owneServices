using System;
using System.Linq;
using CargoWise.Customs.IT.MessageContracts.Declaration.Import;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared.Testing;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Import.Testing;

sealed class I2MessageWrapperTest : I2MessageWrapperTestBase
{
	public override void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("When entryHeader is null", () => new I2MessageWrapper(null, messageSendingWrapperFactory));
		AssertExceptionThrown<ArgumentNullException>("When messageSendingWrapperFactory is null", () => new I2MessageWrapper(EntryHeader, null));
	}

	public override void TestHeader()
	{
		var wrapper = CreateWrapper();
		AssertType<I2HeaderWrapper>("Header Type", wrapper.Header);
	}

	public override void TestItems()
	{
		var wrapper = CreateWrapper();
		AssertEquals("Items Count", 1, wrapper.Items.Count);
		AssertType<I2ItemWrapper>("Item Type", wrapper.Items.Single());
	}

	protected override II2Message CreateWrapper() => new I2MessageWrapper(EntryHeader, messageSendingWrapperFactory);

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
