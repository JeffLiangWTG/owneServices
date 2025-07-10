using System;
using CargoWise.Types;
using GlowIndexQueryService.Business;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(IndexSearchModuleDateTimeOffsetFilter))]
	public sealed class IndexSearchModuleDateTimeOffsetFilterTest : IndexSearchModuleFilterTestCase<IndexSearchModuleDateTimeOffsetFilter>
	{
		protected override ModuleFilter GetNewModuleFilter()
		{
			var field = new SearchField("moo", null, typeof(ZDateTime));
			return new IndexSearchModuleDateTimeOffsetFilter(field);
		}
		protected override FilterCategory ExpectedDefaultCategory => FilterCategories.Dates;

		public override void TestGetGlowIndexQuery()
		{
			var filter = new IndexSearchModuleDateFilter(new SearchField("ETD", null, typeof(ZDateTime)));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;

			AssertEquals(false, filter.ConvertFromLocalToUTC);

			AssertEquals(string.Empty, filter.GetGlowIndexQuery().ToUrlComponent());

			filter.Property1 = new ZDateTime(2023, 1, 1);
			AssertEquals("(ETD ge 2023-01-01T00:00:00Z)", filter.GetGlowIndexQuery().ToUrlComponent());

			filter.Property1 = new ZDateTime(2023, 1, 1);
			filter.Property2 = new ZDateTime(2023, 12, 31);
			AssertEquals("((ETD ge 2023-01-01T00:00:00Z) and (ETD lt 2023-12-31T00:00:00Z))", filter.GetGlowIndexQuery().ToUrlComponent());

			filter.Property1 = ZDateTime.Empty;
			filter.Property2 = new ZDateTime(2023, 12, 31);
			AssertEquals("(ETD lt 2023-12-31T00:00:00Z)", filter.GetGlowIndexQuery().ToUrlComponent());
		}

		[TestUtcOffset(+8, 0, 0)]
		public void TestUtcOffset()
		{
			var field = new SearchField("CREATEDATE", string.Empty, typeof(DateTime), uiHidden: false, isUtcTime: true);

			var filter = new IndexSearchModuleDateFilter(field);
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;

			AssertEquals(true, filter.ConvertFromLocalToUTC);

			filter.Property1 = new ZDateTime(2023, 1, 1, 0, 0, 0);
			AssertEquals("(CREATEDATE ge 2022-12-31T16:00:00Z)", filter.GetGlowIndexQuery().ToUrlComponent());
		}
	}
}
