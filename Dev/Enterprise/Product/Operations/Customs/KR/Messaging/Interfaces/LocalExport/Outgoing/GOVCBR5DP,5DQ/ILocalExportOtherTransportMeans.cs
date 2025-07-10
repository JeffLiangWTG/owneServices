using CargoWise.Types;
using Enterprise.Customs.Business.AccumulativeAmendment;

namespace Enterprise.Customs.KR.Messaging
{
	public interface ILocalExportOtherTransportMeans
	{
		[ID()]
		ZInt SequenceNo { get; }
		[DataItemID("11B")]
		ZString WorkingVesselName { get; }
		[DataItemID("11C")]
		ZString WorkingVesselLloydsNumber { get; }
		[DataItemID("11D")]
		ZString TransportVehicleRegNo { get; }
	}
}
