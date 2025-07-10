using CargoWise.Types;

namespace Enterprise.ArchiveManager.Integration.Test
{
	public interface IAUCustomsTestDataCreator
	{
		void CreateAttachedCusSCADepotHouse(ZGuid shipmentPK);
	}
}
