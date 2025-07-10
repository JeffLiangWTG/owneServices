using System.Reflection;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Moq;
using Moq.Protected;
using CusEntryHeader = Enterprise.Customs.BE.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.BE.Business.Testing;

public abstract class AESMessageSenderTest<TMessageSender, TProvider> : MessageSenderTest<TMessageSender, TProvider>
	where TMessageSender : MessageSender
	where TProvider : class, IMessageHeader
{
	protected override string MessageDirectory =>
#if NETFRAMEWORK
		@"Enterprise.Customs.BE.Business.Testing.Messaging.Outgoing.TestFiles.";
#else
		@"Enterprise.Customs.BE.Business.Testing.Messaging.Outgoing.TestFiles.net8.";
#endif

	protected override Assembly XmlContentAssembly => Assembly.GetExecutingAssembly();

	protected override ZString MessageType => EDIInterchange.ApplicationCodes.BECustomsAesSystem;

	protected override ZString ParentTableName => CusEntryHeader.Schema.TableName;

	protected abstract ZString EntryStatus { get; }

	protected override void AssertMessage(BEMessage message)
	{
		base.AssertMessage(message);
		AssertSendCore();
	}

	void AssertSendCore()
	{
		CombineAssertions(() =>
		{
			AssertEquals("MessageStatus", Common.Shared.MessageStatusList.Codes.Sent, header.MessageStatus);
			AssertEquals("EntryStatus", EntryStatus, header.CH_EntryStatus);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();

		header = Factory.NewWithValidTestData<CusEntryHeader>();
		header.CH_EntryStatus = "ZZZ";
		action = new ExportEntryMessageSendingAction(header) { TypeOfEntry = EntryType };

		mockProvider = new Mock<TProvider> { CallBase = true };
		SetUpMockProviderData(mockProvider);
		var mockMessageSender = new Mock<TMessageSender>(action) { CallBase = true };
		mockMessageSender.Protected().Setup<TProvider>("GetDataProvider", header)
			.Returns(mockProvider.Object);
		messageSender = mockMessageSender.Object;
	}

	protected override void SetUp_TestDeclaration()
	{
		header = Factory.NewWithValidTestData<CusEntryHeader>();
		header.CH_EntryStatus = "ZZZ";
		header.Declaration.ZG_IsTrainingDeclaration = true;
		action = new ExportEntryMessageSendingAction(header) { TypeOfEntry = EntryType, IsTestDeclaration = true };

		mockProvider = new Mock<TProvider> { CallBase = true };
		SetUpMockProviderData(mockProvider);
		var mockMessageSender = new Mock<TMessageSender>(action) { CallBase = true };
		mockMessageSender.Protected().Setup<TProvider>("GetDataProvider", header)
			.Returns(mockProvider.Object);
		messageSender = mockMessageSender.Object;
	}

	protected ExportEntryMessageSendingAction action;
	protected CusEntryHeader header;
}
