using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class FixedMessageNumberStrategyTest : TestCase
{
	public void TestGetMessageReferenceNumber()
	{
		var strategy = new FixedMessageNumberStrategy("");
		AssertEquals("Message Number", "", strategy.GetMessageReferenceNumber());

		strategy = new FixedMessageNumberStrategy("123456");
		AssertEquals("Message Number", "123456", strategy.GetMessageReferenceNumber());
	}
}
