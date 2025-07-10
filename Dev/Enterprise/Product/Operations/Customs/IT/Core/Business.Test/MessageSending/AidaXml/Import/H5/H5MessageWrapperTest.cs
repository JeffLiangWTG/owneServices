using System;
using System.Linq;
using CargoWise.Customs.IT.MessageContracts.Declaration.Import;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared.Testing;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Import.Testing;

sealed class H5MessageWrapperTest : H5MessageWrapperTestBase
{
	public override void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new H5MessageWrapper(null, messageSendingWrapperFactory));
	}

	public override void TestHeader()
	{
		var wrapper = CreateWrapper();
		AssertType<H5HeaderWrapper>("Header Type", wrapper.Header);
	}

	public override void TestItems()
	{
		var wrapper = CreateWrapper();
		AssertEquals("Items Count", 1, wrapper.Items.Count);
		AssertType<H5ItemWrapper>("Item Type", wrapper.Items.Single());
	}

	protected override IH5Message CreateWrapper() => new H5MessageWrapper(EntryHeader, messageSendingWrapperFactory);

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
