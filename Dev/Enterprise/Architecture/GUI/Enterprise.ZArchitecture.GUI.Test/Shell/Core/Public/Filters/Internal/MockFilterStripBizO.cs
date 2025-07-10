using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	public class MockFilterStripBizO : FilterStripBusinessObject
	{
		DummyBusinessObjectCollection dummies;

		public MockFilterStripBizO()
		{
		}

		public DummyBusinessObjectCollection Dummies
		{
			get { return dummies ?? (dummies = new DummyBusinessObjectCollection(Factory)); }
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();
			var decimalRangeFilter = result.AddNumberRangeFilter("Number Range (Decimal)", DummyBizoSchema.Z0_Decimal);
			decimalRangeFilter.ShowUpAndDownArrows = true;
			result.AddNumberRangeFilter("Number Range (Short)", DummyBizoSchema.Z0_Short);
			result.AddTextRangeFilter("Text Range", DummyBizoSchema.Z0_Description);
			result.AddTextFilter("Text", DummyBizoSchema.Z0_Description);
			result.AddDateFilter("Date Range", DummyBizoSchema.Z0_Date);
			result.AddTimeFilter("Time Range", DummyBizoSchema.Z0_SmallDateTime);
			result.AddSingleDateFilter("Single Date", DummyBizoSchema.Z0_Date);

			GetFlagsQuery filter = delegate { return new ZQuery(); };
			result.AddFlagsFilter("Flags Filter",
				new string[] { "Flag1", "Flag2", "Flag3", "Flag4", "Flag5", "Flag6", "Flag7", "Flag8", "Flag9", "Flag10" },
				new GetFlagsQuery[] { filter, filter, filter, filter, filter, filter, filter, filter, filter, filter });

			return result;
		}
	}
}
