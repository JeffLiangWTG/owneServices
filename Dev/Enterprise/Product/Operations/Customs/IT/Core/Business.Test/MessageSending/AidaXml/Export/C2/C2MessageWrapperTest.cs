using System;
using System.Linq;
using CargoWise.Customs.IT.MessageContracts.Declaration.Export;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Export.Testing;

sealed class C2MessageWrapperTest : C2MessageWrapperTestBase
{
	public override void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("When EntryHeader is null", () => new C2MessageWrapper(entryHeader: null, exportMessageSendingWrapperFactory));
		AssertExceptionThrown<ArgumentNullException>("When ExportMessagingSendingWrapperFactory is null", () => new C2MessageWrapper(EntryHeader, exportMessageSendingWrapperFactory: null));
	}

	public override void TestMessageHeader()
	{
		var messageWrapper = CreateWrapper();
		AssertType<C2HeaderWrapper>("MessageHeader Type", messageWrapper.MessageHeader);
	}

	public override void TestItems()
	{
		var messageWrapper = CreateWrapper();
		var items = messageWrapper.Items;

		AssertNotNull("Items", items);
		AssertEquals("Items Count", 1, items.Count);
		AssertType<C2ItemWrapper>("Item Type", items.Single());
	}

	protected override IC2Message CreateWrapper() => new C2MessageWrapper(EntryHeader, exportMessageSendingWrapperFactory);

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
