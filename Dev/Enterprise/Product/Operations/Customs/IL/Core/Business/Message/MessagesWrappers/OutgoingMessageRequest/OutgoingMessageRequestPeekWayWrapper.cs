using CargoWise.Customs.IL.MessageDefinitions.OutgoingMessageRequest;

namespace Enterprise.Customs.IL.Business
{
	public class OutgoingMessageRequestPeekWayWrapper : IOutgoingMessageRequestPeekWay
	{
		OutgoingMessageRequestPeekWayWrapper(int take, int peekWay)
		{
			Take = take;
			PeekWay = peekWay;
		}

		public static OutgoingMessageRequestPeekWayWrapper New(int take, int peekWay) => new OutgoingMessageRequestPeekWayWrapper(take, peekWay);

		public int PeekWay { get; }

		public int Take { get; }
	}
}
