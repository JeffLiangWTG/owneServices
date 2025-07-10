using System;
using System.Collections.Generic;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;
using Enterprise.Customs.IT.Messaging.MessageStructure;
using Enterprise.Customs.IT.Registry.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;
using Moq;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class SadOutgoingCustomsMessageCreationStrategyBaseOnlyTest : TestCaseWithFactory
{
	public void TestGetStrategy()
	{
		var valuesProviderMock = new Mock<ISadOutgoingCustomsMessageGeneratorValuesProvider>() { CallBase = true };
		valuesProviderMock.Setup(m => m.GetApplicationReference()).Returns("XXYYZZ");
		valuesProviderMock.Setup(m => m.FountainProvider).Returns(It.IsAny<ICustomsMessageFountainProvider>());
		valuesProviderMock.Setup(m => m.GetSubType()).Returns("AA");
		valuesProviderMock.Setup(m => m.GetCustomsMessageObjects()).Returns(It.IsAny<IEnumerable<ISadCustomsMessage>>());
		valuesProviderMock.Setup(m => m.Parent).Returns(It.IsAny<BusinessObject>());

		AssertExceptionThrown<ArgumentNullException>("factory is required", () => SadOutgoingCustomsMessageCreationStrategy.GetStrategy(CustomsMessageSendingModeList.Codes.AutomaticProcedure, null, valuesProviderMock.Object));
		AssertExceptionThrown<ArgumentNullException>("valuesProvider is required", () => SadOutgoingCustomsMessageCreationStrategy.GetStrategy(CustomsMessageSendingModeList.Codes.AutomaticProcedure, Factory, null));
		AssertExceptionThrown<ArgumentNullException>("ValuesProvider.FountainProvider is required", () => SadOutgoingCustomsMessageCreationStrategy.GetStrategy(CustomsMessageSendingModeList.Codes.AutomaticProcedure, Factory, valuesProviderMock.Object));
		AssertExceptionThrown<ArgumentNullException>("ValuesProvider.GetCustomsMessageObjects() are required", () => SadOutgoingCustomsMessageCreationStrategy.GetStrategy(CustomsMessageSendingModeList.Codes.AutomaticProcedure, Factory, valuesProviderMock.Object));
		AssertExceptionThrown<ArgumentNullException>("ValuesProvider.Parent is required", () => SadOutgoingCustomsMessageCreationStrategy.GetStrategy(CustomsMessageSendingModeList.Codes.AutomaticProcedure, Factory, valuesProviderMock.Object));
		AssertExceptionThrown<InvalidOperationException>("When messageSendingMode is empty", () => SadOutgoingCustomsMessageCreationStrategy.GetStrategy("", Factory, valuesProviderMock.Object));
		AssertExceptionThrown<InvalidOperationException>("When messageSendingMode is not valid", () => SadOutgoingCustomsMessageCreationStrategy.GetStrategy("XXX", Factory, valuesProviderMock.Object));

		valuesProviderMock.Setup(m => m.FountainProvider).Returns(new Mock<ICustomsMessageFountainProvider>() { CallBase = true }.Object);
		valuesProviderMock.Setup(m => m.GetCustomsMessageObjects()).Returns(new SadCustomsMessage[] { new MockSadCustomsMessage() });
		valuesProviderMock.Setup(m => m.Parent).Returns(Factory.New<DummyBusinessObject>());
		AssertType<AutomaticProcedureSadOutgoingCustomsMessageCreationStrategy>("AutomaticProcedure", SadOutgoingCustomsMessageCreationStrategy.GetStrategy(CustomsMessageSendingModeList.Codes.AutomaticProcedure, Factory, valuesProviderMock.Object));
		AssertType<FallbackProcedureSadOutgoingCustomsMessageCreationStrategy>("FallbackProcedure", SadOutgoingCustomsMessageCreationStrategy.GetStrategy(CustomsMessageSendingModeList.Codes.FallbackProcedure, Factory, valuesProviderMock.Object));
		AssertType<ManualProcedureSadOutgoingCustomsMessageCreationStrategy>("ManualProcedure", SadOutgoingCustomsMessageCreationStrategy.GetStrategy(CustomsMessageSendingModeList.Codes.ManualProcedure, Factory, valuesProviderMock.Object));
	}
}

abstract class SadOutgoingCustomsMessageCreationStrategyTest : TestCaseWithFactory
{
	public void TestGenerateMessage()
	{
		var parentBizObj = GetParentBizObj();

		var fountainProxyMock = new Mock<INumberFountainProxy>();
		fountainProxyMock.Setup(m => m.GetNext(It.IsAny<IDbConnected>())).Returns(1);

		var fountainProviderMock = new Mock<ICustomsMessageFountainProvider>();
		fountainProviderMock.Setup(m => m.DeclarantTaxNumber).Returns("DECTAXNUM");
		fountainProviderMock.Setup(m => m.TryGetNumberFountain()).Returns(fountainProxyMock.Object);
		fountainProviderMock.Setup(m => m.HasClonableNumberRanges).Returns(false);

		var valuesProviderMock = new Mock<ISadOutgoingCustomsMessageGeneratorValuesProvider>();
		valuesProviderMock.Setup(m => m.GetApplicationReference()).Returns("XXYYZZ");
		valuesProviderMock.Setup(m => m.FountainProvider).Returns(fountainProviderMock.Object);
		valuesProviderMock.Setup(m => m.GetSubType()).Returns("AA");
		valuesProviderMock.Setup(m => m.GetCustomsMessageObjects()).Returns(new SadCustomsMessage[] { new MockSadCustomsMessage() });
		valuesProviderMock.Setup(m => m.Parent).Returns(parentBizObj);

		var generator = GetStrategy(Factory, valuesProviderMock.Object);
		var generatedMessage = generator.GenerateMessage();
		CombineAssertions(() =>
		{
			AssertEquals("EM_MessageText", "TEXT", generatedMessage.EM_MessageText);
			AssertEquals("EM_ReceiveTransmit", EDIMessage.Direction.Transmit, generatedMessage.EM_ReceiveTransmit);
			AssertEquals("EM_MessageSubType", "AA", generatedMessage.EM_MessageSubType);
			AssertEquals("EM_ApplicationReference", "XXYYZZ", generatedMessage.EM_ApplicationReference);
			AssertEquals("MessageReferenceNumber", "000001", generatedMessage.MessageNumberStrategy.GetMessageReferenceNumber());
			AssertSame("EM_LinkedObject", parentBizObj, generatedMessage.EM_LinkedObject);
		});
		AssertGeneratorSpecificValues(generatedMessage);
	}

	protected virtual void AssertGeneratorSpecificValues(ITEDIMessage generatedMessage)
	{
	}

	protected virtual BusinessObject GetParentBizObj() => Factory.New<DummyBusinessObject>();

	protected abstract IOutgoingCustomsMessageCreationStrategy GetStrategy(BusinessObjectFactory factory, ISadOutgoingCustomsMessageGeneratorValuesProvider valuesProvider);
}

#region MockCustomsMessage

class MockSadCustomsMessage : SadCustomsMessage
{
	[MessageLayout(Order = 0)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 4, false)]
	public ZString MockField => "TEXT";
}

#endregion

sealed class AutomaticProcedureSadOutgoingCustomsMessageCreationStrategyTest : SadOutgoingCustomsMessageCreationStrategyTest
{
	protected override void AssertGeneratorSpecificValues(ITEDIMessage generatedMessage)
	{
		AssertEquals("EM_Status", EDIMessageStatusList.Codes.Queued, generatedMessage.EM_Status);
		AssertEquals("EM_MessageType", SADConstants.CustomsInterchangeType.IdocR, generatedMessage.EM_MessageType);
	}

	protected override IOutgoingCustomsMessageCreationStrategy GetStrategy(BusinessObjectFactory factory, ISadOutgoingCustomsMessageGeneratorValuesProvider valuesProvider) => new AutomaticProcedureSadOutgoingCustomsMessageCreationStrategy(factory, valuesProvider);
}

sealed class FallbackProcedureSadOutgoingCustomsMessageCreationStrategyTest : SadOutgoingCustomsMessageCreationStrategyTest
{
	protected override void AssertGeneratorSpecificValues(ITEDIMessage generatedMessage)
	{
		AssertEquals("EM_Status", EDIMessageStatusList.Codes.Manual, generatedMessage.EM_Status);
		AssertEquals("EM_MessageType", "FBK", generatedMessage.EM_MessageType);
	}

	protected override IOutgoingCustomsMessageCreationStrategy GetStrategy(BusinessObjectFactory factory, ISadOutgoingCustomsMessageGeneratorValuesProvider valuesProvider) => new FallbackProcedureSadOutgoingCustomsMessageCreationStrategy(factory, valuesProvider);
}

sealed class ManualProcedureSadOutgoingCustomsMessageCreationStrategyTest : SadOutgoingCustomsMessageCreationStrategyTest
{
	protected override void AssertGeneratorSpecificValues(ITEDIMessage generatedMessage)
	{
		AssertEquals("EM_Status", EDIMessageStatusList.Codes.Manual, generatedMessage.EM_Status);
		AssertEquals("EM_MessageType", SADConstants.CustomsInterchangeType.IdocR, generatedMessage.EM_MessageType);
		var packedInterchange = generatedMessage.Interchange;
		AssertNotNull("Message.Interchange", packedInterchange);

		CombineAssertions(() =>
		{
			AssertEquals("Interchange PK", generatedMessage.EM_EI, packedInterchange.PK);
			AssertEquals("Interchange EI_ApplicationCode", EDIInterchange.ApplicationCodes.ITCustoms, packedInterchange.EI_ApplicationCode);
			AssertEquals("Interchange EI_InterchangeType", generatedMessage.EM_MessageType, packedInterchange.EI_InterchangeType);
			AssertEquals("Interchange EI_ReceiveTransmit", EDIInterchange.Direction.Transmit, packedInterchange.EI_ReceiveTransmit);
			AssertEquals("Interchange EI_Status", EDIInterchangeStatusList.Codes.Manual, packedInterchange.EI_Status);
			AssertEquals("Interchange EI_HeaderText",
				"<ITMessage>" +
					"<Staff>BBB</Staff>" +
					"<Node>1234</Node>" +
					"<MessageType>R</MessageType>" +
					"<AccountNumber>11111111111-001</AccountNumber>" +
					"<Header>1234            PLACEHOLDER            000000    11111111111     001 00002</Header>" +
				"</ITMessage>",
				packedInterchange.EI_HeaderText);
		});
	}

	protected override BusinessObject GetParentBizObj()
	{
		var parentBizObj = Factory.New<ApplicationReferenceDummyBusinessObject>();
		parentBizObj.CustomsOffice = "000000";
		parentBizObj.Node = "1234";
		parentBizObj.Subscriber = "BBB";
		parentBizObj.CustomsProfile = "1234-DEC1";
		return parentBizObj;
	}

	protected override IOutgoingCustomsMessageCreationStrategy GetStrategy(BusinessObjectFactory factory, ISadOutgoingCustomsMessageGeneratorValuesProvider valuesProvider) => new ManualProcedureSadOutgoingCustomsMessageCreationStrategy(factory, valuesProvider);

	protected override void SetUp()
	{
		base.SetUp();
		Factory.New<OrgHeader>().OH_Code = "DEC1";
		Factory.Save();
		var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
		ITCustomsNumberViewStmNumsWrapperTestHelper.SetupTestCustomsDeclarationsNumberRange(company, ZDate.Today.Year, "11111111111", currentValue: 1);
		new AccountCollectionTestBuilder(company.PK)
			.AppendAccount("11111111111-001", "1234")
			.AppendAccountDetail("1234-DEC1", "DEC1")
			.Build();
		Factory.Save();
	}
}
