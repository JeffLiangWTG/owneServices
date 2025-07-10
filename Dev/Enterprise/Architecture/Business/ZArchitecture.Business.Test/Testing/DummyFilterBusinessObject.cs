using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Business.Testing
{
	public class DummyFilterBusinessObject : FilterStripBusinessObject, IRelatedModuleFilterBusinessObject
	{
		readonly bool withCustomSqlFilter;

		public DummyFilterBusinessObject()
			: this(withCustomSqlFilter: true)
		{
		}

		public DummyFilterBusinessObject(bool withCustomSqlFilter)
		{
			this.withCustomSqlFilter = withCustomSqlFilter;
			this.QueryObjectType = typeof(DummyBusinessObject);
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new DummyModuleFilterCollection();

			result.AddTextFilter("Z0_Description", DummyBizoSchema.Z0_Description);
			result.AddTextFilter("Z0_Code", DummyBizoSchema.Z0_Code);
			result.AddNumberRangeFilter("Z0_Number", DummyBizoSchema.Z0_Number);
			result.AddDateFilter("Z0_Date", DummyBizoSchema.Z0_Date);
			result.AddTimeFilter("Z0_SmallDateTime", DummyBizoSchema.Z0_SmallDateTime);
			result.AddGuidFilter("Z0_Guid", DummyModuleIDs.Dummy, DummyBizoSchema.Z0_Guid, new DummyBusinessObjectCollection(Factory));

			return result;
		}

		public override void OnLoaded()
		{
			onLoadedCalledCount++;
			base.OnLoaded();
		}

		public override ZQuery Filter
		{
			get
			{
				var result = base.Filter;
				if (FilterMaximumRows.HasValue)
				{
					result.MaximumRows = FilterMaximumRows.Value;
				}
				return result;
			}
		}

		protected override bool ShouldAddCustomSqlFilter => withCustomSqlFilter;

		public int? FilterMaximumRows
		{
			get { return filterMaximumRows; }
			set { filterMaximumRows = value; }
		}

		public int OnLoadedCalledCount
		{
			get { return onLoadedCalledCount; }
		}

		int? filterMaximumRows;
		int onLoadedCalledCount;
	}
}
