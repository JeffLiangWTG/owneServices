using CargoWise.Customs.IL.MessageDefinitions.Common;
using CargoWise.Customs.IL.MessageDefinitions.OutgoingMessageRequest;
using CargoWise.Types;

namespace Enterprise.Customs.IL.Business
{
	public class OutgoingMessageRequestWrapper : IOutgoingMessageRequest
	{
		OutgoingMessageRequestWrapper(int peekWay, string serviceName, int take, ZDateTime fromDate, ZDateTime toDate)
		{
			this.peekWay = peekWay;
			this.serviceName = serviceName;
			this.take = take;
			this.fromDate = fromDate;
			this.toDate = toDate;
		}

		internal static OutgoingMessageRequestWrapper New(int peekWay, string serviceName, int take, ZDateTime fromDate, ZDateTime toDate)
			=> new OutgoingMessageRequestWrapper(peekWay, serviceName, take, fromDate, toDate);

		public IRequestContentHeader RequestContentHeader => RequestContentHeaderWrapper.New();

		public IOutgoingMessageRequestPeekWay PeekWay => OutgoingMessageRequestPeekWayWrapper.New(take, peekWay);

		public IOutgoingMessageRequestGetOptions GetOptions => OutgoingMessageRequestGetOptionsWrapper.NewOrNull(serviceName, fromDate, toDate);

		readonly int peekWay;
		readonly string serviceName;
		readonly int take;
		readonly ZDateTime fromDate;
		readonly ZDateTime toDate;
	}
}
