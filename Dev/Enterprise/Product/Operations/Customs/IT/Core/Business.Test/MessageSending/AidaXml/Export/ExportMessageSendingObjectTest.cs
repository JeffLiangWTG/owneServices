using System;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Customs.IT.MessageContracts.Declaration.Export;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Testing;
using Enterprise.Customs.Universal;
using Moq;
using NUnit.Framework;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Export.Testing;

[TestedType(typeof(ExportMessageSendingObject))]
sealed class ExportMessageSendingObjectTest : Ucc6JobDeclarationMessageSendingObjectTest
{
	protected override string ExpectedServiceTypeNamespace => "http://exportservice.domest.sogei.it";

	protected override string ExpectedServiceTypePrefix => "exp";

	protected override string ExpectedApplicationReference => "EXP";

	protected override ZString ExpectedMessageSubType => "B1";

	protected override void ConfigureDataForISadCustomsMessageGeneratorValuesProviderMembersTest()
	{
		base.ConfigureDataForISadCustomsMessageGeneratorValuesProviderMembersTest();

		EntryInstruction.CEI_Style = "B1";
	}

	public override void TestCustomsMessageText()
	{
		var sendingObject = GetNewSendingObject();

		EntryInstruction.CEI_Style = "B1";
		AssertContains("B1 Declaration", "<DeclarationB1>", sendingObject.CustomsMessageText);

		EntryInstruction.CEI_Style = "B2";
		sendingObject = GetNewSendingObject();
		AssertContains("B2 Declaration", "<DeclarationB2>", sendingObject.CustomsMessageText);

		EntryInstruction.CEI_Style = "B4";
		sendingObject = GetNewSendingObject();
		AssertContains("B4 Declaration", "<DeclarationB4>", sendingObject.CustomsMessageText);

		EntryInstruction.CEI_Style = "C1";
		sendingObject = GetNewSendingObject();
		AssertContains("C1 Declaration", "<DeclarationC1>", sendingObject.CustomsMessageText);

		EntryInstruction.CEI_Style = "C2";
		sendingObject = GetNewSendingObject();
		AssertContains("C2 Declaration", "<DeclarationC2>", sendingObject.CustomsMessageText);

		EntryInstruction.CEI_Style = "BX";
		sendingObject = GetNewSendingObject();
		AssertEquals("CEI_Style other than B1/B2/B4 generates an empty message", "", sendingObject.CustomsMessageText);
	}

	public override void TestCustomsMessageTextForCancellationMessages()
	{
		var sendingObject = (Ucc6JobDeclarationMessageSendingObject)GetNewBusinessObject();
		sendingObject.MessageType = "CAN";
		var valuesProvider = (IAidaXmlOutgoingCustomsMessageGeneratorValuesProvider)sendingObject;
		AssertContains("CustomsMessageText when MessageType is CAN", "<Cancellation />", valuesProvider.CustomsMessageText);
	}

	public override void TestLookups()
	{
		var sendingObject = (ExportMessageSendingObject)GetNewBusinessObject();

		AssertNotNull("Lookups", sendingObject.Lookups);
		AssertType<ExportMessageSendingObjectLookups>("Lookups type", sendingObject.Lookups);
	}

	public void TestCustomsMessageTextForAmendmentMessages()
	{
		var ceiStyles = new[] { "B1", "B2", "B4" };

		var sendingObject = (Ucc6JobDeclarationMessageSendingObject)GetNewBusinessObject();
		sendingObject.MessageType = "AMD";
		sendingObject.VOCReason = "A";

		foreach (var ceiStyle in ceiStyles)
		{
			EntryInstruction.CEI_Style = ceiStyle;
			CombineAssertions($"When CEI_Style={ceiStyle}", () =>
			{
				var valuesProvider = (IAidaXmlOutgoingCustomsMessageGeneratorValuesProvider)sendingObject;
				AssertContains("CustomsMessageText when MessageType is AMD", "<Causal>A</Causal>", valuesProvider.CustomsMessageText);
			});
		}
	}

	public void TestGetMessageBuilder_WhenTransitionPeriodIsOn()
	{
		using (TemporarySetTransitionPeriod(isActive: true))
		{
			CombineAssertions(() =>
			{
				SetEntryStyleAndAssertMessageBuilderType<B1TransitionPeriodMessageBuilder>("B1");
				SetEntryStyleAndAssertMessageBuilderType<B2TransitionPeriodMessageBuilder>("B2");
				SetEntryStyleAndAssertMessageBuilderType<B4TransitionPeriodMessageBuilder>("B4");
				SetEntryStyleAndAssertMessageBuilderType<C1TransitionPeriodMessageBuilder>("C1");
				SetEntryStyleAndAssertMessageBuilderType<C2TransitionPeriodMessageBuilder>("C2");
			});
		}
	}

	public void TestGetMessageBuilder_WhenTransitionPeriodIsOff()
	{
		using (TemporarySetTransitionPeriod(isActive: false))
		{
			CombineAssertions(() =>
			{
				SetEntryStyleAndAssertMessageBuilderType<B1MessageBuilder>("B1");
				SetEntryStyleAndAssertMessageBuilderType<B2MessageBuilder>("B2");
				SetEntryStyleAndAssertMessageBuilderType<B4MessageBuilder>("B4");
				SetEntryStyleAndAssertMessageBuilderType<C1MessageBuilder>("C1");
				SetEntryStyleAndAssertMessageBuilderType<C2MessageBuilder>("C2");
			});
		}
	}

	public void TestGetCancellationXmlMessageBuilderType()
	{
		var messageSendingObject = new ExportMessageSendingObjectForTest(EntryHeader, JobDeclarationMessageSendingObjectParent);
		AssertType<CancellationMessageBuilder>(messageSendingObject.GetCancellationMessageBuilder_Exposed());
	}

	public void TestMessageType_CancellationAndAmendmentLegislativeReference_Default()
	{
		var sendingObject = (Ucc6JobDeclarationMessageSendingObject)GetNewBusinessObject();
		AssertEquals("Reference must be empty when messageType is not AMD", string.Empty, sendingObject.CancellationAndAmendmentLegislativeReference);

		sendingObject.MessageType = EDIMessageTypeList.Codes.Amendment;
		AssertEquals("Reference must be Default 1, When messageType is set to AMD", "1", sendingObject.CancellationAndAmendmentLegislativeReference);

		sendingObject.MessageType = EDIMessageTypeList.Codes.Cancellation;
		AssertEquals("Reference must be Reset ,When messageType change from AMD to CAN", string.Empty, sendingObject.CancellationAndAmendmentLegislativeReference);
	}

	public void TestDeclarationType()
	{
		EntryInstruction.CEI_Style = "B1";

		var exportMessageSendingObject = new ExportMessageSendingObject(EntryHeader, JobDeclarationMessageSendingObjectParent);
		AssertEquals("DeclarationType when CH_EntryStatus is not DEP", "B1", exportMessageSendingObject.DeclarationType);

		EntryHeader.CH_EntryStatus = "DEP";
		exportMessageSendingObject = new ExportMessageSendingObject(EntryHeader, JobDeclarationMessageSendingObjectParent);
		AssertEquals("DeclarationType when CH_EntryStatus is DEP", "C2", exportMessageSendingObject.DeclarationType);
	}

	protected override void SetUp()
	{
		base.SetUp();
		Declaration.JE_MessageType = "EXP";
	}

	void SetEntryStyleAndAssertMessageBuilderType<TExpectedType>(ZString entryStyle)
	{
		Declaration.MessageVersion = MessageVersionList.Codes.XML;
		EntryInstruction.CEI_Style = entryStyle;
		var messageSendingObject = new ExportMessageSendingObjectForTest(EntryHeader, JobDeclarationMessageSendingObjectParent);
		var messageBuilder = messageSendingObject.GetMessageBuilder_Exposed();
		AssertNotNull($"MessageBuilder for Declaration Type {entryStyle}", messageBuilder);
		AssertType<TExpectedType>($"Message Builder Type for Declaration  {entryStyle} ", messageBuilder);
	}

	IAidaXmlOutgoingCustomsMessageGeneratorValuesProvider GetNewSendingObject()
		=> new ExportMessageSendingObject(EntryHeader, JobDeclarationMessageSendingObjectParent);

	IDisposable TemporarySetTransitionPeriod(bool isActive)
		=> ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.AESTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, value: isActive);

	sealed class ExportMessageSendingObjectForTest : ExportMessageSendingObject
	{
		public ExportMessageSendingObjectForTest(CusEntryHeader header, JobDeclarationMessageSendingObjectParent jobDeclarationMessageSendingObjectParent) : base(header, jobDeclarationMessageSendingObjectParent)
		{
		}

		internal IXmlMessageBuilder GetMessageBuilder_Exposed() => GetMessageBuilder();

		internal IXmlMessageBuilder GetCancellationMessageBuilder_Exposed() => GetCancellationXmlMessageBuilder(new Mock<ICancellation>().Object);
	}
}
