using CargoWise.Customs.IL.MessageDefinitions.OutgoingMessageRequest;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class OutgoingMessageRequestWrapperTest : DataProviderTestCase<IOutgoingMessageRequest>
	{
		public void TestRequestContentHeader()
		{
			AssertNotNull(Provider.RequestContentHeader);
			AssertType<RequestContentHeaderWrapper>(Provider.RequestContentHeader);
		}

		public void TestPeekWay()
		{
			AssertNotNull(Provider.PeekWay);
			AssertType<OutgoingMessageRequestPeekWayWrapper>(Provider.PeekWay);
		}

		public void TestGetOptions()
		{
			AssertNull(Provider.GetOptions);
			var wrapper = OutgoingMessageRequestWrapper.New(int.Parse(PeekWayList.Codes._3), "Service1", 200, ZDateTime.BrettsBirthday, ZDateTime.BrettsBirthday);
			AssertNotNull(wrapper.GetOptions);
			AssertType<OutgoingMessageRequestGetOptionsWrapper>(wrapper.GetOptions);
		}

		protected override IOutgoingMessageRequest GetProvider() => OutgoingMessageRequestWrapper.New(int.Parse(PeekWayList.Codes._2), null, 200, ZDateTime.BrettsBirthday, ZDateTime.BrettsBirthday);
	}
}
