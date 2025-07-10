using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public static class VesselHelper
	{
		public static bool IsExist(BusinessObjectFactory factory, ZString vesselName, ZString lloydsIMO)
		{
			return factory.ExistsInDatabase(RefVesselSchema.Constants.TableName, ExistFilter(vesselName, lloydsIMO));
		}

		static ZQuery ExistFilter(ZString vesselName, ZString lloydsIMO)
		{
			var existFilter = new ZQuery(RefVesselSchema.RV_Code, vesselName);
			existFilter.AddToFilter(RefVesselSchema.RV_LloydsNumber, lloydsIMO);
			return existFilter;
		}
	}
}
