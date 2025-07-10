using CargoWise.Customs.IL.MessageDefinitions.OutgoingMessageRequest;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class OutgoingMessageRequestPeekWayWrapperTest : DataProviderTestCase<IOutgoingMessageRequestPeekWay>
	{
		public void TestPeekWay()
		{
			AssertEquals(2, Provider.PeekWay);
		}

		public void TestTake()
		{
			AssertEquals(400, Provider.Take);
		}

		protected override IOutgoingMessageRequestPeekWay GetProvider() => OutgoingMessageRequestPeekWayWrapper.New(400, int.Parse(PeekWayList.Codes._2));
	}
}
