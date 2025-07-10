using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.Customs.IT.Registry.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class AutomaticProcedureMessageSender_NctsHeaderTest : ITMessageSender_NctsHeaderTest
{
	protected override ZString ExpectedMessageStatus => EDIMessageStatusList.Codes.Queued;

	protected override ZString ExpectedMessageType => SADConstants.CustomsInterchangeType.IdocR;

	protected override IOutgoingCustomsMessageCreationStrategy GetMessageGenerator(ISadOutgoingCustomsMessageGeneratorValuesProvider sendingObject)
		=> new AutomaticProcedureSadOutgoingCustomsMessageCreationStrategy(Factory, sendingObject);
}

sealed class FallbackProcedureMessageSender_NctsHeaderTest : ITMessageSender_NctsHeaderTest
{
	protected override ZString ExpectedMessageStatus => EDIMessageStatusList.Codes.Manual;

	protected override ZString ExpectedMessageType => Fallback;

	protected override IOutgoingCustomsMessageCreationStrategy GetMessageGenerator(ISadOutgoingCustomsMessageGeneratorValuesProvider sendingObject)
		=> new FallbackProcedureSadOutgoingCustomsMessageCreationStrategy(Factory, sendingObject);

	const string Fallback = "FBK";
}

sealed class ManualProcedureMessageSender_NctsHeaderTest : ITMessageSender_NctsHeaderTest
{
	protected override ZString ExpectedMessageStatus => EDIMessageStatusList.Codes.Manual;

	protected override ZString ExpectedMessageType => SADConstants.CustomsInterchangeType.IdocR;

	protected override IOutgoingCustomsMessageCreationStrategy GetMessageGenerator(ISadOutgoingCustomsMessageGeneratorValuesProvider sendingObject)
		=> new ManualProcedureSadOutgoingCustomsMessageCreationStrategy(Factory, sendingObject);
}

abstract class ITMessageSender_NctsHeaderTest : TestCaseWithFactory
{
	[TestDate(2019, 11, 25)]
	public void TestSendMessageSuccessfully_WithoutM2Lines()
	{
		nctsHeader.BH_JobReference = "A0001";

		var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
		var declarantTaxNumber = "11111111111";
		ITCustomsNumberViewStmNumsWrapperTestHelper.SetupTestCustomsDeclarationsNumberRange(company, ZDate.Today.Year, declarantTaxNumber, currentValue: 1);
		new AccountCollectionTestBuilder(company.PK)
			.AppendAccount("11111111111-001", "1234")
			.AppendAccountDetail("1234-DEC1", "DEC1")
			.Build();

		Factory.Save();

		var messageResult = SendMessage(nctsHeader);
		Factory.Save();

		AssertNotNull("Message sent result", messageResult);
		AssertEquals("BH_MessageStatus", NctsMessageStatusList.Codes.DepartureDeclarationSent, nctsHeader.BH_MessageStatus);
		AssertEquals("BM_EntryDate", new ZDate(2019, 11, 25), nctsHeader.MovementHeader.BM_EntryDate);
		CheckNctsHeaderMessagesCollectionUsingStandardProcedure(nctsHeader);
	}

	[TestDate(2020, 01, 01)]
	public void TestSendMessageSuccessfullyCloningLastYearNumberRange()
	{
		var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
		new AccountCollectionTestBuilder(company.PK.ToGuid())
			.AppendAccount("11111111111-001", "1234")
			.AppendAccountDetail("1234-DEC1", "DEC1")
			.Build();

		var tempFactory = new BusinessObjectFactory();
		var tempCompany = tempFactory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
		ITCustomsNumberViewStmNumsWrapperTestHelper.SetupTestCustomsDeclarationsNumberRange(tempCompany, 2019, "11111111111", currentValue: 10000, minimumValue: 1, maximumValue: 10000);
		tempFactory.Save();
		company.Reload();

		var messageResult = SendMessage(nctsHeader);
		Factory.Save();

		AssertNotNull("Message sent result", messageResult);
		AssertEquals("BH_MessageStatus", NctsMessageStatusList.Codes.DepartureDeclarationSent, nctsHeader.BH_MessageStatus);
		AssertEquals("BM_EntryDate", new ZDate(2020, 01, 01), nctsHeader.MovementHeader.BM_EntryDate);
		CheckNctsHeaderMessagesCollectionUsingStandardProcedure(nctsHeader);
	}

	ITEDIMessage SendMessage(NctsHeader nctsHeader)
	{
		var sendingObject = new TIRMessageSendingObject(nctsHeader);
		var sendableCustomsEntry = new NctsHeaderSendableCustomsEntry(nctsHeader);
		var messageSender = new ITMessageSender(Factory, GetMessageGenerator(sendingObject), sendableCustomsEntry);
		return messageSender.Send();
	}

	protected abstract IOutgoingCustomsMessageCreationStrategy GetMessageGenerator(ISadOutgoingCustomsMessageGeneratorValuesProvider sendingObject);

	protected abstract ZString ExpectedMessageStatus { get; }

	protected abstract ZString ExpectedMessageType { get; }

	void CheckNctsHeaderMessagesCollectionUsingStandardProcedure(NctsHeader nctsHeader) => CheckNctsHeaderMessagesCollection(nctsHeader, ExpectedMessageStatus, ExpectedMessageType);

	void CheckNctsHeaderMessagesCollection(NctsHeader nctsHeader, ZString messageStatus, ZString messageType)
	{
		var messages = nctsHeader.Messages;
		AssertNotNull("Messages not null", messages);
		AssertEquals("One message expected", 1, messages.Count);

		CombineAssertions(() =>
		{
			var message = messages[0];
			AssertNotEquals("Message Text", "", message.EM_MessageText);
			AssertEquals("Application Code", "ITM", message.EM_ApplicationCode);
			AssertEquals("Status", messageStatus, message.EM_Status);
			AssertEquals("Receive", "TRX", message.EM_ReceiveTransmit);
			AssertEquals("Sub Type", "ET", message.EM_MessageSubType);
			AssertEquals("Type", messageType, message.EM_MessageType);
			AssertEquals("Application Reference", "1234:BBB:IT000000", message.EM_ApplicationReference);
			AssertEquals("Message Num", "000001", message.EM_MessageNum);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		Factory.New<OrgHeader>().OH_Code = "DEC1";
		Factory.Save();

		nctsHeader = Factory.NewDepartureNctsHeader();
		nctsHeader.BH_CustomsProfile = "1234-DEC1";
		nctsHeader.Subscriber = "BBB";
		nctsHeader.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture, "IT000000");
	}

	NctsHeader nctsHeader;
}
