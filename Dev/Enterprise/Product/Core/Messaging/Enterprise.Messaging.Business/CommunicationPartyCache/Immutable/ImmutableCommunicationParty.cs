using CargoWise.Types;
using Enterprise.Integration;

namespace Enterprise.Messaging.Business
{
	public class ImmutableCommunicationParty : IEDICommunicationParty
	{
		public ImmutableCommunicationParty(ZGuid pk, ZBool ecpIsActive, ZString ecpName, ZString ecpSummary, ZString ecpApplicationCode, ZGuid ecpSecurityProxy)
		{
			PK = pk;
			ECP_IsActive = ecpIsActive;
			ECP_Name = ecpName;
			ECP_Summary = ecpSummary;
			ECP_ApplicationCode = ecpApplicationCode;
			ECP_GS_SecurityProxy = ecpSecurityProxy;
		}

		public static ImmutableCommunicationParty FromFactoryObject(IEDICommunicationParty communicationParty)
		{
			if (communicationParty == null)
			{
				return null;
			}
			return new ImmutableCommunicationParty(
				communicationParty.PK,
				communicationParty.ECP_IsActive,
				communicationParty.ECP_Name,
				communicationParty.ECP_Summary,
				communicationParty.ECP_ApplicationCode,
				communicationParty.ECP_GS_SecurityProxy
			);
		}

		public ZGuid PK { get; }

		public ZBool ECP_IsActive { get; }

		public ZString ECP_Name { get; }

		public ZString ECP_Summary { get; }

		public ZString ECP_ApplicationCode { get; }

		public ZGuid ECP_GS_SecurityProxy { get; }
	}
}
