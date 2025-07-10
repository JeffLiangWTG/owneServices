using System.Threading;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Registry.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IT.Business.Testing;

abstract class ITOutgoingMessageProcessorBaseTest<TITOutgoingMessageProcessor> : TestCaseWithFactory
	where TITOutgoingMessageProcessor : ITOutgoingMessageProcessor
{
	public void TestProcessMessage()
	{
		Factory.NewWithValidTestData<OrgHeader>().OH_Code = "DEC1";
		Factory.Save();

		var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
		ITCustomsNumberViewStmNumsWrapperTestHelper.SetupTestCustomsDeclarationsNumberRange(company, ZDate.Today.Year, "11111111111", 1);
		var registryAccount = new AccountCollectionTestBuilder(company.PK)
			.AppendAccount("11111111111-001", "1234")
			.AppendAccountDetail("1234-DEC1", "DEC1")
			.Build()[0];

		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_CustomsProfile = "1234-DEC1";
		var message = declaration.CustomsEntryHeaders.AddNew().Messages.AddNew();
		message.EM_MessageType = OutgoingMessageTypeToTest;
		message.MessageNumberStrategy = new FixedMessageNumberStrategy(ZString.Empty);
		Factory.Save();

		AssertNull("[PRE-CONDITION] Message Interchange", message.Interchange);

		var logger = new LoggingInformation();
		var testProcessor = GetOutgoingMessageProcessor(logger);
		testProcessor.ProcessMessage(CancellationToken.None);

		message.Reload();

		CombineAssertions("Status", () =>
		{
			AssertEquals("Message Status", EDIMessage.Status.Sent, message.EM_Status);
			AssertNotNull("Message Interchange", message.Interchange);
			AssertEquals("Log Count", 1, logger.UserLogStrings.Count);
		});

		CombineAssertions("Created Interchange", () =>
		{
			ITInterchangeProviderTestHelper.AssertCreatedInterchange(message);
			AssertEquals($"1st Log [{logger.UserLogStrings[0]}] contains expected message?", true, logger.UserLogStrings[0].EndsWith("1 message(s) have been processed."));
		});
	}

	protected abstract TITOutgoingMessageProcessor GetOutgoingMessageProcessor(LoggingInformation logger);

	protected abstract string OutgoingMessageTypeToTest { get; }
}
