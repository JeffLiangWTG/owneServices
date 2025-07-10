using CargoWise.Common;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.IE.NCTS.Messaging
{
	public class CC043CDepartureTransportMeansProvider
	{
		public CC043CDepartureTransportMeansProvider(DepartureTransportMeansType02 departureTransportMeans)
		{
			departureTransportMeansType = Argument.NotNull(departureTransportMeans, nameof(departureTransportMeans));
		}
		readonly DepartureTransportMeansType02 departureTransportMeansType;

		public ZShort SequenceNumber => CachedValueHelper.GetValue(ref sequenceNumberCached, () => ZShort.ParseSafe(departureTransportMeansType.SequenceNumber, ZShort.Zero));
		CachedValue<ZShort> sequenceNumberCached;

		public ZString TypeOfIdentification => departureTransportMeansType.TypeOfIdentification ?? ZString.Empty;

		public ZString IdentificationNumber => departureTransportMeansType.IdentificationNumber ?? ZString.Empty;

		public ZString Nationality => departureTransportMeansType.Nationality ?? ZString.Empty;
	}
}
