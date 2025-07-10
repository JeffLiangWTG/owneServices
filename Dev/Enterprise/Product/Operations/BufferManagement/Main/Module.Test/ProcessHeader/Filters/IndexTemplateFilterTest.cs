using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using GlowIndexQueryService.Business;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Module.Test
{
	[TestedType(typeof(IndexTemplateFilter))]
	class IndexTemplateFilterTest : IndexSearchModuleFilterTestCase<IndexTemplateFilter>
	{
		protected override FilterCategory ExpectedDefaultCategory => FilterCategories.TextSearch;

		protected override ModuleFilter GetNewModuleFilter()
		{
			var field = SearchField.Create("moo");
			return new IndexTemplateFilter(field, new TemplateFilterOptions());
		}

		public override void TestGetGlowIndexQuery()
		{
			var filter = new IndexTemplateFilter(SearchField.Create("moo"), new ProcessTaskFilterBusinessObject().Statuses);

			filter.Property = "ALL";
			AssertEquals("", filter.GetGlowIndexQuery().ToUrlComponent());

			filter.Property = "TMP";
			AssertEquals("(WorkflowType eq 'P0')", filter.GetGlowIndexQuery().ToUrlComponent());

			filter.Property = "NON";
			AssertEquals("(WorkflowType ne 'P0')", filter.GetGlowIndexQuery().ToUrlComponent());
		}
	}
}
