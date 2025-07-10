using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Registry.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class ITInterchangeProvider_CusEntryHeaderTest : TestCaseWithFactory
{
	public void TestMessagesPopulateNewInterchange()
	{
		var declarant = Factory.NewWithValidTestData<OrgHeader>();
		declarant.OH_Code = "DEC1";

		Factory.Save();
		new AccountCollectionTestBuilder(GlbCompany.CurrentCompany.PK)
			.AppendAccount("11111111111-001", "1234")
			.AppendAccountDetail("1234-DEC1", "DEC1")
			.Build();

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_GS_NKCusAgent = "~U1";
		declaration.JE_CustomsProfile = "1234-DEC1";
		var message = declaration.CustomsEntryHeaders.AddNew().Messages.AddNew();
		message.EM_MessageType = SADConstants.CustomsInterchangeType.IdocR;
		message.MessageNumberStrategy = new FixedMessageNumberStrategy(ZString.Empty);

		var ediMessageCollection = new NonDependentEDIMessageCollection(Factory);
		ediMessageCollection.Add(message);

		var interchangeProvider = new SadInterchangeProvider(ediMessageCollection);
		interchangeProvider.PackCollatedMessagesIntoInterchanges();

		var interchanges = interchangeProvider.Interchanges;
		Factory.Save();
		message.Reload();

		AssertEquals("One interchange created", 1, interchanges.Length);
		AssertNotNull("Message is linked to the interchange", message.Interchange);
		AssertEquals("Message status", EDIMessage.Status.Sent, message.EM_Status);

		CombineAssertions("Created Interchange", () =>
		{
			ITInterchangeProviderTestHelper.AssertCreatedInterchange(message, EDIInterchange.Status.eHubQueued);
		});
	}
}
