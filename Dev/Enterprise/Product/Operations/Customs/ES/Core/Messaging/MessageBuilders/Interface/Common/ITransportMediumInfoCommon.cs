using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders;

public interface ITransportMediumInfoCommon
{
	ZString TransportMode { get; }

	#region Fields For TPL

	ZString TransportId { get; }
	ZString TransportNationality { get; }

	#endregion
}

public interface ICommonDepartureTransportMeans : ITransportMediumInfoCommon
{
	ZString SequenceNumber { get; }
}

public interface ICommonArrivalTransportMeans
{
	ZString Type { get; }
	ZString Id { get; }
}
