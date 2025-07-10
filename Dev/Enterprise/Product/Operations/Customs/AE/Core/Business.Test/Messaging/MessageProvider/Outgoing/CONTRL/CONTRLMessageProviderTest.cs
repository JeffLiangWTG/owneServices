using Enterprise.Edifact.D23A.Elements;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AE.Business.Testing;

sealed class CONTRLMessageProviderTest : Customs.Business.Testing.DataProviderTestCase<CONTRLMessageProvider>
{
	public void TestMessageHeader() => CombineAssertions(() =>
	{
		var provider = GetProvider();
		var messageHeaderProvider = provider.MessageHeader;
		AssertType<CONTRLMessageHeaderProvider>(messageHeaderProvider);
		AssertEquals(MessageTypeList.SyntaxAndServiceReportMessage.ToString(), messageHeaderProvider.MessageType);
	});

	public void TestActionCoded() => CombineAssertions(() =>
	{
		var provider = GetProvider();
		AssertEquals(ActionCodedList.ThisLevelAndAllLowerLevelsRejected, provider.ActionCoded);

		isCUSRESMessageParsed = true;
		provider = GetProvider();
		AssertEquals(ActionCodedList.ThisLevelAcknowledgedAndAllLowerLevelsAcknowledgedIfNotExplicitlyRejected, provider.ActionCoded);

		cUSRESMessage.EM_LinkedObject = linkedObjectForTest;
		provider = GetProvider();
		AssertEquals(ActionCodedList.InterchangeReceived, provider.ActionCoded);
	});

	public void TestInterchangeControlReference()
	{
		var provider = GetProvider();
		AssertEquals("084059B3333333", provider.InterchangeControlReference);
	}

	public void TestRequestMessage()
	{
		var provider = GetProvider();
		AssertEquals(cUSRESMessage, provider.RequestMessage);
	}

	public void TestAddNewEDIMessage()
	{
		var provider = GetProvider();
		var message = provider.AddNewEDIMessage();
		AssertNull(message.EM_LinkedObject);

		cUSRESMessage.EM_LinkedObject = linkedObjectForTest;
		provider = GetProvider();
		message = provider.AddNewEDIMessage();
		AssertNotNull(provider.Messages);
		AssertEquals(1, provider.Messages.Count);
		AssertEquals(linkedObjectForTest, message.EM_LinkedObject);
	}

	public void TestBaseConstruction() => CombineAssertions(() =>
	{
		var provider = GetProvider();
		AssertNull(provider.Messages);
		AssertSame(cUSRESMessage.Factory, provider.Factory);

		cUSRESMessage.EM_LinkedObject = linkedObjectForTest;
		provider = GetProvider();
		AssertNotNull(provider.Messages);
		AssertSame(linkedObjectForTest, provider.Messages.Master);
		AssertSame(linkedObjectForTest.Factory, provider.Factory);
	});

	protected override CONTRLMessageProvider GetProvider()
	{
		return new CONTRLMessageProvider(cUSRESMessage, isCUSRESMessageParsed);
	}

	protected override void SetUp()
	{
		base.SetUp();
		cUSRESInterchange = Factory.New<EDIInterchange>();
		cUSRESInterchange.EI_BodyText = @"UNB+UNOB:4::2:02+UAENAIC+SERPRID::FORFFID:LOCFFID+20240516:0840+084059B3333333'
UNH+123H456+CUSRES'
UNZ+1+084059B3333333'
";

		cUSRESMessage = Factory.New<EDIMessage>();
		cUSRESMessage.EM_EI = cUSRESInterchange.PK;
		cUSRESMessage.EM_LinkedObject = null;

		isCUSRESMessageParsed = false;
		linkedObjectForTest = Factory.New<LinkedObjectForTest>();
	}

	EDIInterchange cUSRESInterchange;
	EDIMessage cUSRESMessage;
	bool isCUSRESMessageParsed;
	LinkedObjectForTest linkedObjectForTest;
}
