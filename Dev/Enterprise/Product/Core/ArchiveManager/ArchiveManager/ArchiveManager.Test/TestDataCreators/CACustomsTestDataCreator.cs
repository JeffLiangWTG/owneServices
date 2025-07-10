using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ArchiveManager.Integration.Test;

namespace Enterprise.ArchiveManager.Test.TestDataCreators
{
	public class CACustomsTestDataCreator : ICACustomsTestDataCreator
	{
		public void CreateAttachedCusCAeMHMaster(ZGuid consolPK)
		{
			var factory = new BusinessObjectFactory();
			var eMHMaster = factory.New<Enterprise.Integration.Customs.CA.ICusCAeMHMaster>();
			eMHMaster.BP_ParentID = consolPK;
			eMHMaster.BP_ParentTableCode = "JK";
			eMHMaster.BP_IsActive = false;

			factory.Save();
		}
	}
}
