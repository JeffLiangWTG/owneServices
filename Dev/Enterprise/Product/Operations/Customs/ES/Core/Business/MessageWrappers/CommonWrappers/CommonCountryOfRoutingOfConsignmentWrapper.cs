using CargoWise.Types;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class CommonCountryOfRoutingOfConsignmentWrapper : ICommonCountryOfRoutingOfConsignment
	{
		public CommonCountryOfRoutingOfConsignmentWrapper(ZShort seqNum, ZString country)
		{
			SequenceNumber = seqNum.ToString();
			CountryOfRouting = country;
		}

		public ZString SequenceNumber { get; }

		public ZString CountryOfRouting { get; }
	}
}
