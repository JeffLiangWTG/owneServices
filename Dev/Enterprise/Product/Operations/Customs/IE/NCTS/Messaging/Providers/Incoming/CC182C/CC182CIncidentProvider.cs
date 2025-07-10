using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IE.NCTS.Messaging
{
	public class CC182CIncidentProvider
	{
		public CC182CIncidentProvider(IncidentType03 incidentType)
		{
			this.incidentType = Argument.NotNull(incidentType, nameof(incidentType));
		}
		readonly IncidentType03 incidentType;

		public ZString IncidentCode => incidentType.Code;

		public ZString IncidentText => incidentType.Text;

		public ZDate EndorsementDate => (incidentType.Endorsement?.Date).ConvertToZDate();

		public ZString EndorsementAuthority => incidentType.Endorsement?.Authority ?? ZString.Empty;

		public ZString EndorsementPlace => incidentType.Endorsement?.Place ?? ZString.Empty;

		public ZString EndorsementCountry => incidentType.Endorsement?.Country ?? ZString.Empty;

		public IReadOnlyCollection<CC182CTransportEquipmentProvider> TransportEquipment => transportEquipment ?? (transportEquipment = incidentType.TransportEquipment?.Select(x => new CC182CTransportEquipmentProvider(x)).ToArray() ?? Array.Empty<CC182CTransportEquipmentProvider>());
		IReadOnlyCollection<CC182CTransportEquipmentProvider> transportEquipment;
	}
}
