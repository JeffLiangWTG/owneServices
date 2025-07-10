using CargoWise.Common;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.IE.NCTS.Messaging
{
	public class CC043CIncidentProvider
	{
		public CC043CIncidentProvider(IncidentType04 incident)
		{
			incidentType = Argument.NotNull(incident, nameof(incident));
		}
		readonly IncidentType04 incidentType;

		public ZShort SequenceNumber => CachedValueHelper.GetValue(ref sequenceNumberCached, () => ZShort.ParseSafe(incidentType.SequenceNumber, ZShort.Zero));
		CachedValue<ZShort> sequenceNumberCached;

		public ZString Code => incidentType.Code;

		public ZString Text => incidentType.Text;

		public CC043CEndorsementProvider Endorsement => endorsementCached ?? (endorsementCached =  incidentType.Endorsement == null ? null : new CC043CEndorsementProvider(incidentType.Endorsement));
		CC043CEndorsementProvider endorsementCached;

		public CC043CLocationProvider Location => locationCached ?? (locationCached =  incidentType.Location == null ? null : new CC043CLocationProvider(incidentType.Location));
		CC043CLocationProvider locationCached;
	}
}
