using System;
using System.Linq;
using CargoWise.Customs.IT.MessageContracts.Declaration.Import;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared.Testing;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Import.Testing;

sealed class I1MessageWrapperTest : I1MessageWrapperTestBase
{
	public override void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("When entryHeader is null", () => new I1MessageWrapper(entryHeader: null, messageSendingWrapperFactory));
		AssertExceptionThrown<ArgumentNullException>("When messageSendingWrapperFactory is null", () => new I1MessageWrapper(EntryHeader, null));
	}

	public override void TestHeader()
	{
		var messageWrapper = CreateWrapper();
		AssertType<I1HeaderWrapper>("MessageHeader Type", messageWrapper.Header);
	}

	public override void TestItems()
	{
		var messageWrapper = CreateWrapper();
		AssertEquals("Items Count", 1, messageWrapper.Items.Count);
		AssertType<I1ItemWrapper>("Item Type", messageWrapper.Items.Single());
	}

	protected override II1Message CreateWrapper() => new I1MessageWrapper(EntryHeader, messageSendingWrapperFactory);

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
