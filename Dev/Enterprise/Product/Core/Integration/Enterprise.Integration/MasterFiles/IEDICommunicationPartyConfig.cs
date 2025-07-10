using CargoWise.Types;

namespace Enterprise.Integration
{
	public interface IEDICommunicationPartyConfig
	{
		ZGuid PK { get; }
		ZString ECC_Direction { get; }
		ZGuid ECC_ECA_Auth { get; }
		ZGuid ECC_ECP_Party { get; }
		ZString ECC_Endpoint { get; }
		ZGuid ECC_GB_Branch { get; }
		ZGuid ECC_GE_Department { get; }
		ZBool ECC_IsActive { get; }

		IEDICommunicationParty Party { get; }
		IEDIClientAuth Auth { get; }
	}
}
