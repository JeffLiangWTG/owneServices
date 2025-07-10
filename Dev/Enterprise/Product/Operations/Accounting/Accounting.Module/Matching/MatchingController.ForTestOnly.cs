#if DEBUG

using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Module
{
	public partial class MatchingController
	{
		public ZArchitecture.GUI.IZForm GetForm_ForTestOnly(IBusiness businessEntity)
		{
			return GetForm(businessEntity);
		}

		public IBusiness GetLoadedBusinessEntityInLocalFactory_ForTestOnly(IBusiness sourceEntity)
		{
			return GetLoadedBusinessEntityInLocalFactory(sourceEntity);
		}
	}
}

#endif