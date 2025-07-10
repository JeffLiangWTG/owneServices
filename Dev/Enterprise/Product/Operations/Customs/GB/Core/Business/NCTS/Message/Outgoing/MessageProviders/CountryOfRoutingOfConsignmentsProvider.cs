using CargoWise.Customs.GB.MessageContracts.NCTS.Phase5;
using CargoWise.Types;

namespace Enterprise.Customs.GB.Business.NCTS
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
