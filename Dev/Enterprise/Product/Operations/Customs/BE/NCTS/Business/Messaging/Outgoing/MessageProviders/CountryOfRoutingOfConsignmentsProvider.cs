using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Types;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public class CountryOfRoutingOfConsignmentsProvider : ICountryOfRoutingOfConsignment
	{
		readonly ZString countryCode;

		public CountryOfRoutingOfConsignmentsProvider(ZString countryCode, int sequenceNumber)
		{
			this.countryCode = countryCode;
			SequenceNumber = sequenceNumber;
		}

		public int SequenceNumber { get; }

		public string Country => countryCode;
	}
}
