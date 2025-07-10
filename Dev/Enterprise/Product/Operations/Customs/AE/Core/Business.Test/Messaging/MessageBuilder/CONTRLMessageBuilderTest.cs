using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Edifact.D23A.Elements;
using Enterprise.Messaging.Business;
using Moq;

namespace Enterprise.Customs.AE.Business.Testing;

sealed class CONTRLMessageBuilderTest : TestCaseWithFactory
{
	public void TestBuildMessage_ThisLevelAndAllLowerLevelsRejected() => CombineAssertions(() =>
	{
		var provider = CreateDataProviderForTest(ActionCodedList.ThisLevelAndAllLowerLevelsRejected);
		var messageBuilder = new CONTRLMessageBuilder(provider);
		var message = messageBuilder.PopulateMessages().GetBuilderResults().Single().Message;
		var expectedText = @"UNH+<<AE-MSG-NUM>>+CONTRL:4:2:UN'
UCI+123456+++4'
UNT+3+<<AE-MSG-NUM>>'";
		AssertEquals(AEConstants.Messaging.MessageTypes.CONTRL, message.EM_MessageType);
		AssertEquals(expectedText, message.EM_MessageText);
		AssertEquals(linkedObjectForTest, message.EM_LinkedObject);
		AssertEquals(requestMessage.PK, message.EM_EM_RequestMessage);
	});

	public void TestBuildMessage_ThisLevelAcknowledgedAndAllLowerLevelsAcknowledgedIfNotExplicitlyRejected() => CombineAssertions(() =>
	{
		var provider = CreateDataProviderForTest(ActionCodedList.ThisLevelAcknowledgedAndAllLowerLevelsAcknowledgedIfNotExplicitlyRejected);
		var messageBuilder = new CONTRLMessageBuilder(provider);
		var message = messageBuilder.PopulateMessages().GetBuilderResults().Single().Message;
		var expectedText = @"UNH+<<AE-MSG-NUM>>+CONTRL:4:2:UN'
UCI+123456+++7'
UNT+3+<<AE-MSG-NUM>>'";
		AssertEquals(AEConstants.Messaging.MessageTypes.CONTRL, message.EM_MessageType);
		AssertEquals(expectedText, message.EM_MessageText);
		AssertEquals(linkedObjectForTest, message.EM_LinkedObject);
		AssertEquals(requestMessage.PK, message.EM_EM_RequestMessage);
	});

	public void TestBuildMessage_InterchangeReceived() => CombineAssertions(() =>
	{
		var provider = CreateDataProviderForTest(ActionCodedList.InterchangeReceived);
		var messageBuilder = new CONTRLMessageBuilder(provider);
		var message = messageBuilder.PopulateMessages().GetBuilderResults().Single().Message;
		var expectedText = @"UNH+<<AE-MSG-NUM>>+CONTRL:4:2:UN'
UCI+123456+++8'
UNT+3+<<AE-MSG-NUM>>'";
		AssertEquals(AEConstants.Messaging.MessageTypes.CONTRL, message.EM_MessageType);
		AssertEquals(expectedText, message.EM_MessageText);
		AssertEquals(linkedObjectForTest, message.EM_LinkedObject);
		AssertEquals(requestMessage.PK, message.EM_EM_RequestMessage);
	});

	ICONTRLMessageProvider CreateDataProviderForTest(ActionCodedList actionCoded)
	{
		var mockMessageProvider = new Mock<ICONTRLMessageProvider>();
		mockMessageProvider.Setup(x => x.MessageHeader).Returns(new CONTRLMessageHeaderProvider());
		mockMessageProvider.Setup(x => x.ActionCoded).Returns(actionCoded);
		mockMessageProvider.Setup(x => x.InterchangeControlReference).Returns("123456");
		mockMessageProvider.Setup(x => x.RequestMessage).Returns(requestMessage);
		mockMessageProvider.Setup(x => x.Messages).Returns(messages);
		mockMessageProvider.Setup(x => x.Factory).Returns(Factory);
		mockMessageProvider.Setup(x => x.AddNewEDIMessage()).Returns(messageToSend);
		return mockMessageProvider.Object;
	}

	protected override void SetUp()
	{
		base.SetUp();
		linkedObjectForTest = Factory.New<LinkedObjectForTest>();
		messages = new EDIMessageCollection(linkedObjectForTest);
		requestMessage = Factory.New<EDIMessage>();
		messageToSend = (AEEDIMessage)messages.AddNew(typeof(AEEDIMessage));
	}

	LinkedObjectForTest linkedObjectForTest;

	EDIMessageCollection messages;

	EDIMessage requestMessage;

	AEEDIMessage messageToSend;
}
