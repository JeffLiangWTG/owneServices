using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.ZArchitecture.Testing
{
	sealed class GridColourStripBizoForCustomSqlTest : GridColourStripBusinessObject
	{
		public GridColourStripBizoForCustomSqlTest()
			: base(new FilterStripBusinessObjectForTest(), null, null, false)
		{ }

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();
			var fiter = new ModuleSQLFilter(CustomSqlFilterDescription, typeof(DummyBusinessObject));
			fiter.Property1 = "Z0_Guid = (SELECT Z0_Guid FROM dbo.DummyBizo WITH (NOLOCK) WHERE Z0_Code LIKE 'HFG%')";
			fiter.IsActive = true;
			filters.AddFilter(fiter);
			return filters;
		}
	}
}
