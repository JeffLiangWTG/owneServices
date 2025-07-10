using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.ZArchitecture.Testing
{
	sealed class FilterStripBusinessObjectForTest : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();
			var fiter1 = new ModuleTextFilter("desc", delegate
			{ return new ZQuery(); });
			filters.AddFilter(fiter1);
			return filters;
		}

		protected override List<IFilterStripsHelper> GetCustomFilterStripsHelpersCore()
		{
			return helpersToAddForTest;
		}

		public void AddHelperForTest(IFilterStripsHelper helper)
		{
			helpersToAddForTest.Add(helper);
		}

		readonly List<IFilterStripsHelper> helpersToAddForTest = new List<IFilterStripsHelper>();
	}
}
