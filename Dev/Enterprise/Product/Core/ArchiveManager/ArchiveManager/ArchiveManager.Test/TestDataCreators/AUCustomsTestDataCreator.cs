using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ArchiveManager.Integration.Test;
using Enterprise.Customs.AU.Declaration.Business;

namespace Enterprise.ArchiveManager.Test.TestDataCreators
{
	public sealed class AUCustomsTestDataCreator : IAUCustomsTestDataCreator
	{
		public void CreateAttachedCusSCADepotHouse(ZGuid shipmentPK)
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			CusSCADepotHouse house = factory.New<CusSCADepotHouse>();
			house.CX_JS = shipmentPK;

			factory.Save();
		}
	}
}
