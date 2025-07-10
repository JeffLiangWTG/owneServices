using CargoWise.Types;
using Enterprise.Integration;

namespace Enterprise.Messaging.Business
{
	public class ImmutableCommunicationPartyConfig : IEDICommunicationPartyConfig
	{
		public ImmutableCommunicationPartyConfig(ZGuid pk, ZString eccDirection, ZGuid eccEcaAuth, ZGuid eccEcpParty, ZString eccEndpoint, ZGuid eccGbBranch, ZGuid eccGeDepartment, ZBool eccIsActive, IEDICommunicationParty party, IEDIClientAuth auth)
		{
			PK = pk;
			ECC_Direction = eccDirection;
			ECC_ECA_Auth = eccEcaAuth;
			ECC_ECP_Party = eccEcpParty;
			ECC_Endpoint = eccEndpoint;
			ECC_GB_Branch = eccGbBranch;
			ECC_GE_Department = eccGeDepartment;
			ECC_IsActive = eccIsActive;
			Party = party;
			Auth = auth;
		}

		public static ImmutableCommunicationPartyConfig FromFactoryObject(IEDICommunicationPartyConfig communicationPartyConfig)
		{
			if (communicationPartyConfig == null)
			{
				return null;
			}
			return new ImmutableCommunicationPartyConfig(
				communicationPartyConfig.PK,
				communicationPartyConfig.ECC_Direction,
				communicationPartyConfig.ECC_ECA_Auth,
				communicationPartyConfig.ECC_ECP_Party,
				communicationPartyConfig.ECC_Endpoint,
				communicationPartyConfig.ECC_GB_Branch,
				communicationPartyConfig.ECC_GE_Department,
				communicationPartyConfig.ECC_IsActive,
				ImmutableCommunicationParty.FromFactoryObject(communicationPartyConfig.Party),
				ImmutableCommunicationAuth.FromFactoryObject(communicationPartyConfig.Auth)
			);
		}

		public ZGuid PK { get; }

		public ZString ECC_Direction { get; }
		public ZGuid ECC_ECA_Auth { get; }
		public ZGuid ECC_ECP_Party { get; }
		public ZString ECC_Endpoint { get; }
		public ZGuid ECC_GB_Branch { get; }
		public ZGuid ECC_GE_Department { get; }
		public ZBool ECC_IsActive { get; }

		public IEDICommunicationParty Party { get; }
		public IEDIClientAuth Auth { get; }
	}
}
