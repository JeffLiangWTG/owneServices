
using CargoWise.Types;

using Enterprise.Freight.Business;

namespace Enterprise.DocumentWrappers
{
	public interface ITransportDetails
	{
		ZString ParentDescription { get; }
		ZString TransportMode { get; }
		ZString TransportType { get; }
		ZString TransportTypeDescription { get; }
		ZString Vessel { get; }
		ZString VoyageFlight { get; }
		ZString Load { get; }
		ZString Discharge { get; }

		ZByte LegOrder { get; }

		ZDateTime ETD { get; }
		ZDateTime ETA { get; }
		ZDateTime ATD { get; }
		ZDateTime ATA { get; }

		ZDateTime LCLReceivalCommences { get; }
		ZDateTime LCLCutOff { get; }
		ZDateTime LCLAvailabilityDate { get; }
		ZDateTime LCLStorageDate { get; }

		ZDateTime FCLReceivalCommences { get; }
		ZDateTime FCLCutOff { get; }
		ZDateTime FCLAvailabilityDate { get; }
		ZDateTime FCLStorageDate { get; }

		ZGuid Carrier { get; }

		IFlightDetailsSuppression SuppressingBizO { get; }
	}
}
