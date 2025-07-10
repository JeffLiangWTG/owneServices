
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRCommunityProtectionProfileCollection : BusinessObjectCollection<CMRCommunityProtectionProfile>
	{
		public CMRCommunityProtectionProfileCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public bool Contains(ZInt riskID)
		{
			foreach (CMRCommunityProtectionProfile profile in this)
			{
				if (profile.CP_CommunityProtectionRiskIdentifier == riskID)
				{
					return true;
				}
			}
			return false;
		}
	}
}
