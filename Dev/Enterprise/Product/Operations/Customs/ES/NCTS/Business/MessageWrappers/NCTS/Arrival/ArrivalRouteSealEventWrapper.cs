using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public class ArrivalRouteSealEventWrapper : IArrivalRouteEvent
	{
		public ArrivalRouteSealEventWrapper(EnRouteSeal seal)
		{
			this.seal = Argument.NotNull(seal, nameof(seal));
		}
		readonly EnRouteSeal seal;

		public ZString EventPlace => seal.BN_EventPlace;

		public ZString EventPlaceLanguage => ZString.Empty;

		public ZString EventCountry => seal.BN_EventCountryCode;

		public ZBool IncidentInEvent => false;

		public IArrivalFormData IncidentFormData => null;

		public ZString NewSealsInEventNum => seal.BN_NoOfSeals.ToString();

		public IReadOnlyCollection<IArrivalNewSealsInformation> NewSealsInformation
		{
			get
			{
				if (newSealsInformation == null)
				{
					newSealsInformation = seal.SealContainers
						.Cast<SealContainer>()
						.Select(container => new ArrivalNewSealsInformationWrapper(container))
						.ToList().AsReadOnly();
				}
				return newSealsInformation;
			}
		}
		IReadOnlyCollection<ArrivalNewSealsInformationWrapper> newSealsInformation;

		public ZString NewTransportNationality => ZString.Empty;

		public IArrivalFormData TransferFormData => null;

		public IReadOnlyCollection<ZString> NewContainerIDs => newContainerIDs ?? (newContainerIDs = new List<ZString>().AsReadOnly());
		IReadOnlyCollection<ZString> newContainerIDs;
	}
}
