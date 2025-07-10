using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public class ArrivalRouteTransshipmentEventWrapper : IArrivalRouteEvent
	{
		public ArrivalRouteTransshipmentEventWrapper(EnRouteTransshipment transshipment)
		{
			this.transshipment = Argument.NotNull(transshipment, nameof(transshipment));
		}
		readonly EnRouteTransshipment transshipment;

		public ZString EventPlace => transshipment.BN_EventPlace;

		public ZString EventPlaceLanguage => ZString.Empty;

		public ZString EventCountry => transshipment.BN_EventCountryCode;

		public ZBool IncidentInEvent => false;

		public IArrivalFormData IncidentFormData => null;

		public ZString NewSealsInEventNum => ZString.Empty;

		public IReadOnlyCollection<IArrivalNewSealsInformation> NewSealsInformation => newSealsInformation ?? (newSealsInformation = Array.Empty<IArrivalNewSealsInformation>());
		IReadOnlyCollection<IArrivalNewSealsInformation> newSealsInformation;

		public ZString NewTransportNationality => transshipment.BN_TransportCountryCode;

		public IArrivalFormData TransferFormData => transferFormData ?? (transferFormData = new ArrivalFormDataTransshipmentWrapper(transshipment));
		ArrivalFormDataTransshipmentWrapper transferFormData;

		public IReadOnlyCollection<ZString> NewContainerIDs
		{
			get
			{
				if (newContainerIDs == null)
				{
					newContainerIDs = transshipment.Containers
						.Select(container => container.BC_ContainerNum)
						.ToList().AsReadOnly();
				}
				return newContainerIDs;
			}
		}
		IReadOnlyCollection<ZString> newContainerIDs;
	}
}
