using CargoWise.Types;
using Enterprise.Customs.Business.AccumulativeAmendment;

namespace Enterprise.Customs.KR.Messaging
{
	public interface IExportVehicleNo
	{
		[ID()]
		ZString SequenceNo { get; }
		[DataItemID("E102")]
		ZString VIN { get; }
	}
}
