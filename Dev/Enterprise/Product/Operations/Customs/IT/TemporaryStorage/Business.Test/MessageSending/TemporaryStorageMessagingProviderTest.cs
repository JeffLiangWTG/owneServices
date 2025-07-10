using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.TemporaryStorage.Business.Testing;

[TestedType(typeof(TemporaryStorageMessagingProvider))]
sealed class TemporaryStorageMessagingProviderTest : TestCaseWithFactory
{
	public void TestMessagingProviderType()
	{
		AssertType<TemporaryStorageMessagingProvider>("MessagingProvider Type", header.MessagingProvider);
	}

	public void TestDefaultMessageType_WhenCustomsStatusIsNotAMG()
	{
		var sendingObjectParent = new TemporaryStorageMessageSendingObjectParent(header);
		AssertEquals("Default MessageType", "NEW", header.MessagingProvider.GetDefaultMessageType(sendingObjectParent.SendingObjectsCollection[0]));
	}

	public void TestDefaultMessageType_WhenCustomsStatusIsAMG()
	{
		header.CustomsStatus = "AMG";
		var sendingObjectParent = new TemporaryStorageMessageSendingObjectParent(header);
		AssertEquals("Default MessageType with Customs Status AMG", "AMD", header.MessagingProvider.GetDefaultMessageType(sendingObjectParent.SendingObjectsCollection[0]));
	}

	public void TestMessageTypes_WhenCustomsStatusIsNotAMG()
	{
		AssertEquals("Available MessageTypes", "NEW - New Declaration", header.MessagingProvider.GetMessageTypes(header).ElementsAsString);
	}

	public void TestMessageTypes_WhenCustomsStatusIsAMG()
	{
		header.CustomsStatus = "AMG";
		AssertEquals("Available MessageTypes", "AMD - Amendment", header.MessagingProvider.GetMessageTypes(header).ElementsAsString);
	}

	protected override void SetUp()
	{
		base.SetUp();
		header = Factory.New<TemporaryStorageHeader>();
	}

	TemporaryStorageHeader header;
}
