using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZDummyNonPersistentModuleButtonGrid : ZDummyModuleButtonGrid
	{
		protected override BusinessObject ConvertSelectedObjectToEditableObjectForModule(BusinessObject originalBizO)
		{
			var factory = new BusinessObjectFactory();
			var bizo = factory.NewWithValidTestData<DummyChildBusinessObject>();
			factory.Save();
			return bizo;
		}
	}
}
