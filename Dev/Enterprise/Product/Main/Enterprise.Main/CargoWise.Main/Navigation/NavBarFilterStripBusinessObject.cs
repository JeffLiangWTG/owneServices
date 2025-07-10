using Enterprise.ZArchitecture.Business;

namespace CargoWise.Main.Navigation
{
	class NavBarFilterStripBusinessObject : FilterStripBusinessObject
	{
		public void SetModuleId(Enterprise.ZArchitecture.Modules.ModuleIdentifier moduleID)
		{
			Internals.ClearLayoutsCache();
			Internals.LayoutContext = moduleID.Name;
		}

		IFilterStripBusinessObjectInternals Internals
		{
			get { return this; }
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			return new ModuleFilterCollection();
		}
	}
}
