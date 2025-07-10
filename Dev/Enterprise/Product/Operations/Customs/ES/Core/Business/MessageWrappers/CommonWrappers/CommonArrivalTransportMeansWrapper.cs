using CargoWise.Types;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers;

public class CommonArrivalTransportMeansWrapper : ICommonArrivalTransportMeans
{
	public CommonArrivalTransportMeansWrapper(ZString type, ZString id)
	{
		Type = type;
		Id = id;
	}

	public ZString Type { get; }

	public ZString Id { get; }
}
