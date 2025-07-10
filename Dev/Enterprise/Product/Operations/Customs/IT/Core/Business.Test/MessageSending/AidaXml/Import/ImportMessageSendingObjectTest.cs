using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Customs.IT.MessageContracts.Declaration.Import;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Import.Testing;

[TestedType(typeof(ImportMessageSendingObject))]
sealed class ImportMessageSendingObjectTest : Ucc6JobDeclarationMessageSendingObjectTest
{
	protected override string ExpectedServiceTypeNamespace => "http://importservice.domest.sogei.it";

	protected override string ExpectedServiceTypePrefix => "imp";

	protected override string ExpectedApplicationReference => "IMP";

	protected override ZString ExpectedMessageSubType => "H1";

	protected override void ConfigureDataForISadCustomsMessageGeneratorValuesProviderMembersTest()
	{
		base.ConfigureDataForISadCustomsMessageGeneratorValuesProviderMembersTest();

		EntryInstruction.CEI_Style = "H1";
	}

	public override void TestLookups()
	{
		var sendingObject = (ImportMessageSendingObject)GetNewBusinessObject();

		AssertNotNull("Lookups", sendingObject.Lookups);
		AssertType<ImportMessageSendingObjectLookups>("Lookups type", sendingObject.Lookups);
	}

	public override void TestCustomsMessageText()
	{
		var sendingObject = GetNewSendingObject();

		EntryInstruction.CEI_Style = "H1";
		AssertContains("H1 Declaration", "<DichiarazioneH1>", sendingObject.CustomsMessageText);

		EntryInstruction.CEI_Style = "H2";
		sendingObject = GetNewSendingObject();
		AssertContains("H2 Declaration", "<DichiarazioneH2>", sendingObject.CustomsMessageText);

		EntryInstruction.CEI_Style = "H3";
		sendingObject = GetNewSendingObject();
		AssertContains("H3 Declaration", "<DichiarazioneH3>", sendingObject.CustomsMessageText);

		EntryInstruction.CEI_Style = "H4";
		sendingObject = GetNewSendingObject();
		AssertContains("H4 Declaration", "<DichiarazioneH4>", sendingObject.CustomsMessageText);

		EntryInstruction.CEI_Style = "H5";
		sendingObject = GetNewSendingObject();
		AssertContains("H5 Declaration", "<DichiarazioneH5>", sendingObject.CustomsMessageText);

		EntryInstruction.CEI_Style = "I1";
		sendingObject = GetNewSendingObject();
		AssertContains("I1 Declaration", "<DichiarazioneI1>", sendingObject.CustomsMessageText);

		EntryInstruction.CEI_Style = "I2";
		sendingObject = GetNewSendingObject();
		AssertContains("I2 Declaration", "<DichiarazioneI2>", sendingObject.CustomsMessageText);

		EntryInstruction.CEI_Style = "XX";
		sendingObject = GetNewSendingObject();
		AssertEquals("Invalid Declaration Type: XX", "", sendingObject.CustomsMessageText);

		IAidaXmlOutgoingCustomsMessageGeneratorValuesProvider GetNewSendingObject()
			=> new ImportMessageSendingObject(EntryHeader, JobDeclarationMessageSendingObjectParent);
	}

	public override void TestCustomsMessageTextForCancellationMessages()
	{
		var importMessageSendingObject = new ImportMessageSendingObject(EntryHeader, JobDeclarationMessageSendingObjectParent);
		importMessageSendingObject.MessageType = "CAN";
		var valuesProvider = (IAidaXmlOutgoingCustomsMessageGeneratorValuesProvider)importMessageSendingObject;
		AssertContains("CustomsMessageText when MessageType is CAN", "<Annullamento />", valuesProvider.CustomsMessageText);
	}

	public void TestNewDeclarationMessagesDoesNotHaveAmendmentInfo()
	{
		var importMessageSendingObject = new ImportMessageSendingObject(EntryHeader, JobDeclarationMessageSendingObjectParent);
		var valuesProvider = (IAidaXmlOutgoingCustomsMessageGeneratorValuesProvider)importMessageSendingObject;

		EntryInstruction.CEI_Style = "H1";
		importMessageSendingObject.MessageType = "NEW";
		importMessageSendingObject.VOCReason = "A";
		AssertNotContains("CustomsMessageText", "<Causale>", valuesProvider.CustomsMessageText);
	}

	public void TestCustomsMessageTextForAmendmentMessages()
	{
		var sendingObject = new ImportMessageSendingObject(EntryHeader, JobDeclarationMessageSendingObjectParent);
		EntryInstruction.CEI_Style = "H1";
		sendingObject.MessageType = "AMD";
		sendingObject.VOCReason = "A";
		var valuesProvider = (IAidaXmlOutgoingCustomsMessageGeneratorValuesProvider)sendingObject;
		AssertContains("CustomsMessageText when MessageType is AMD", "<Causale>A</Causale>", valuesProvider.CustomsMessageText);
	}

	public void TestDeclarationType()
	{
		EntryInstruction.CEI_Style = "H1";

		var importMessageSendingObject = new ImportMessageSendingObject(EntryHeader, JobDeclarationMessageSendingObjectParent);
		AssertEquals("When EntryHeader-> EntryStatus is not DEP, DeclarationType", "H1", importMessageSendingObject.DeclarationType);

		EntryHeader.CH_Status = "ACO";
		EntryHeader.CH_EntryStatus = "DEP";
		importMessageSendingObject = new ImportMessageSendingObject(EntryHeader, JobDeclarationMessageSendingObjectParent);
		AssertEquals("When EntryHeader-> EntryStatus is DEP, DeclarationType", "I2", importMessageSendingObject.DeclarationType);
	}

	public void TestGetMessageBuilder()
	{
		CombineAssertions(() =>
		{
			SetEntryStyleAndAssertMessageBuilderType<H1MessageBuilder>("H1");
			SetEntryStyleAndAssertMessageBuilderType<H2MessageBuilder>("H2");
			SetEntryStyleAndAssertMessageBuilderType<H3MessageBuilder>("H3");
			SetEntryStyleAndAssertMessageBuilderType<H4MessageBuilder>("H4");
			SetEntryStyleAndAssertMessageBuilderType<H5MessageBuilder>("H5");
			SetEntryStyleAndAssertMessageBuilderType<I1MessageBuilder>("I1");
			SetEntryStyleAndAssertMessageBuilderType<I2MessageBuilder>("I2");
		});

		void SetEntryStyleAndAssertMessageBuilderType<TExpectedType>(string entryStyle)
		{
			EntryInstruction.CEI_Style = entryStyle;
			var messageSendingObject = new ImportMessageSendingObjectForTest(EntryHeader, JobDeclarationMessageSendingObjectParent);
			var messageBuilder = messageSendingObject.GetMessageBuilder_Exposed();
			AssertNotNull($"MessageBuilder for Declaration Type {entryStyle}", messageBuilder);
			AssertType<TExpectedType>($"Message Builder Type for Declaration  {entryStyle} ", messageBuilder);
		}
	}

	public void TestGetCancellationXmlMessageBuilderType()
	{
		var messageSendingObject = new ImportMessageSendingObjectForTest(EntryHeader, JobDeclarationMessageSendingObjectParent);
		AssertType<CancellationMessageBuilder>(messageSendingObject.GetCancellationXmlMessageBuilder_Exposed());
	}

	protected override void SetUp()
	{
		base.SetUp();
		Declaration.JE_MessageType = "IMP";
	}

	sealed class ImportMessageSendingObjectForTest : ImportMessageSendingObject
	{
		public ImportMessageSendingObjectForTest(CusEntryHeader header, JobDeclarationMessageSendingObjectParent jobDeclarationMessageSendingObjectParent) : base(header, jobDeclarationMessageSendingObjectParent)
		{
		}

		internal IXmlMessageBuilder GetMessageBuilder_Exposed() => GetMessageBuilder();

		internal IXmlMessageBuilder GetCancellationXmlMessageBuilder_Exposed() => GetCancellationXmlMessageBuilder(new Mock<ICancellation>().Object);
	}
}
