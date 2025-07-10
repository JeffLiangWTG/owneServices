using CargoWise.Types;

namespace Enterprise.Integration
{
	public interface IEDICommunicationParty
	{
		ZGuid PK { get; }

		ZBool ECP_IsActive { get; }

		ZString ECP_Name { get; }

		ZString ECP_Summary { get; }

		ZString ECP_ApplicationCode { get; }

		ZGuid ECP_GS_SecurityProxy { get; }
	}
}
