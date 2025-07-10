using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRCommunityProtectionRiskCollection : BusinessObjectCollection<CMRCommunityProtectionRisk>
	{
		public CMRCommunityProtectionRiskCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
