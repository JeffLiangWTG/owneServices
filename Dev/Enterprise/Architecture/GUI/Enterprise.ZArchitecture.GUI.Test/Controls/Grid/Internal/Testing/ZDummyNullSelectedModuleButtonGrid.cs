using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZDummyNullSelectedModuleButtonGrid : ZDummyModuleButtonGrid
	{
		protected override BusinessObject ConvertSelectedObjectToEditableObjectForModule(BusinessObject originalBizO)
		{
			return null;
		}

		protected override BusinessObject GetObjectToEdit(BusinessObject selected)
		{
			return null;
		}
	}
}
