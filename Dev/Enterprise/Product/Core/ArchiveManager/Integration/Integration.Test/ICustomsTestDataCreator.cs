using CargoWise.Types;

namespace Enterprise.ArchiveManager.Integration.Test
{
	public interface ICustomsTestDataCreator : ICustomsAttachedDeclarationDataCreator
	{
		ZGuid CreateStandAloneDeclarationData();
		void CreateAttachedSeaCargo(ZGuid shipmentPK);
		void CreateAttachedAirCargo(ZGuid shipmentPK);
		void CreateOceanBillForConsol(ZGuid consolPK);
	}
}
