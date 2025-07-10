using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

abstract class BaseNctsMessageDataProviderTest<TDataProvider, TMessageSendingObject> : BaseTransitDataProviderTest<TDataProvider, TMessageSendingObject>
	where TDataProvider : class, IPassarMessage
	where TMessageSendingObject : class, INctsMessageSendingObject
{
	public void TestMessageIdentification()
	{
		CombineAssertions(() =>
		{
			AssertEquals("MessageIdentification source", MessageSendingObject.MessageIdentification, DataProvider.MessageIdentification);
			AssertEquals("Each invocation should return the same value", DataProvider.MessageIdentification, DataProvider.MessageIdentification);
		});
	}

	public void TestOppositeInformation()
	{
		CombineAssertions(() =>
		{
			AssertNotNull(DataProvider.OppositeInformation);
			AssertSame("cached", DataProvider.OppositeInformation, DataProvider.OppositeInformation);
		});
	}
}
