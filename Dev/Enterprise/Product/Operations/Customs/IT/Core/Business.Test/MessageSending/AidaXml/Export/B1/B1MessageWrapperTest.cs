using System;
using System.Linq;
using CargoWise.Customs.IT.MessageContracts.Declaration.Export;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Export.Testing;

sealed class B1MessageWrapperTest : B1MessageWrapperTestBase
{
	public override void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("When entryHeader is null", () => new B1MessageWrapper(entryHeader: null, exportMessageSendingWrapperFactory));
		AssertExceptionThrown<ArgumentNullException>("When messageSendingWrapperFactory is null", () => new B1MessageWrapper(EntryHeader, null));
	}

	public override void TestMessageHeader()
	{
		var messageWrapper = CreateWrapper();
		AssertType<B1HeaderWrapper>("MessageHeader Type", messageWrapper.MessageHeader);
	}

	public override void TestItems()
	{
		var messageWrapper = CreateWrapper();
		AssertEquals("Items Count", 1, messageWrapper.Items.Count);
		AssertType<B1ItemWrapper>("Item Type", messageWrapper.Items.Single());
	}

	protected override IB1Message CreateWrapper() => new B1MessageWrapper(EntryHeader, exportMessageSendingWrapperFactory);

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
