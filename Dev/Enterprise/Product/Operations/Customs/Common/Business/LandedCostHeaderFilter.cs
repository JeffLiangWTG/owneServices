using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Common
{
	public class LandedCostHeaderFilter : ZQuery
	{
		public LandedCostHeaderFilter(ILandedCostHeader lCHeaderHost)
		{
			//Mycheckouts is a lying pos
			AddToFilter(LandedCostHeaderSchema.LT_ParentID, lCHeaderHost.PK);
			FetchOnlyFromLocalCache = !lCHeaderHost.IsInDatabase;
		}
	}
}
