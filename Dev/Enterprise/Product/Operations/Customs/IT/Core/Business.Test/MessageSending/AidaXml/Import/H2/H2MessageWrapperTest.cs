using System;
using System.Linq;
using CargoWise.Customs.IT.MessageContracts.Declaration.Import;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared.Testing;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Import.Testing;

sealed class H2MessageWrapperTest : H2MessageWrapperTestBase
{
	public override void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("When entryHeader is null", () => new H2MessageWrapper(entryHeader: null, messageSendingWrapperFactory));
		AssertExceptionThrown<ArgumentNullException>("When messageSendingWrapperFactory is null", () => new H2MessageWrapper(EntryHeader, null));
	}

	public override void TestHeader()
	{
		var messageWrapper = CreateWrapper();
		AssertType<H2HeaderWrapper>("MessageHeader Type", messageWrapper.Header);
	}

	public override void TestItems()
	{
		var messageWrapper = CreateWrapper();
		AssertEquals("Items Count", 1, messageWrapper.Items.Count);
		AssertType<H2ItemWrapper>("Item Type", messageWrapper.Items.Single());
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

	protected override IH2Message CreateWrapper() => new H2MessageWrapper(EntryHeader, messageSendingWrapperFactory);
}
