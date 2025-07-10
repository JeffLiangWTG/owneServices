using System.Collections.Generic;
using CargoWise.Customs.EU.MessageContracts.ICS2;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class PartyIdProvider : IPartyId
	{
		public PartyIdProvider(string identificationNumber, IReadOnlyCollection<IIdentifierTypePair> communications)
		{
			IdentificationNumber = identificationNumber;
			Communications = communications;
		}

		public static PartyIdProvider New(string identificationNumber, IReadOnlyCollection<IIdentifierTypePair> communications) => new PartyIdProvider(identificationNumber, communications);

		public string IdentificationNumber { get; }

		public IReadOnlyCollection<IIdentifierTypePair> Communications { get; }
	}
}
