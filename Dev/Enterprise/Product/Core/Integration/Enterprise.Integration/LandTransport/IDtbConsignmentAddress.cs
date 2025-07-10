using CargoWise.Types;

namespace Enterprise.Integration.LandTransport
{
	public interface IDtbConsignmentAddress
	{
		ZGuid PK { get; }

		ZGuid LTS_LTC_Consignment { get; }
	}
}
