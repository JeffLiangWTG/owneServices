using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders
{
	public interface IArrivalRouteEvent
	{
		ZString EventPlace { get; }
		ZString EventPlaceLanguage { get; }

		#region Fields For PAC

		ZString EventCountry { get; }
		ZBool IncidentInEvent { get; }

		#region Fields For PCI
		IArrivalFormData IncidentFormData { get; }
		#endregion

		ZString NewSealsInEventNum { get; }

		#region Fields For PCI
		IReadOnlyCollection<IArrivalNewSealsInformation> NewSealsInformation { get; }
		#endregion

		ZString NewTransportNationality { get; }

		#region Fields For PCI
		IArrivalFormData TransferFormData { get; }
		IReadOnlyCollection<ZString> NewContainerIDs { get; }
		#endregion

		#endregion
	}
}
