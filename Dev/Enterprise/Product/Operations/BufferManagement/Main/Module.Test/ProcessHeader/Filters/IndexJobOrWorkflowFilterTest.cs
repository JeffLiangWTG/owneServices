using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using GlowIndexQueryService.Business;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Module.Test
{
	[TestedType(typeof(IndexJobOrWorkflowFilter))]
	class IndexJobOrWorkflowFilterTest : IndexSearchModuleFilterTestCase<IndexTemplateFilter>
	{
		protected override FilterCategory ExpectedDefaultCategory => FilterCategories.TextSearch;

		protected override ModuleFilter GetNewModuleFilter()
		{
			var field = SearchField.Create("moo");
			return new IndexJobOrWorkflowFilter(field, new IndexJobOrWorkflowFilterOptions());
		}

		public override void TestGetGlowIndexQuery()
		{
			var filter = new IndexJobOrWorkflowFilter(SearchField.Create("moo"), new IndexJobOrWorkflowFilterOptions());

			filter.Property = "ALL";
			AssertEquals("", filter.GetGlowIndexQuery().ToUrlComponent());

			filter.Property = "JOB";
			AssertEquals("(JobOrWorkflow eq null)", filter.GetGlowIndexQuery().ToUrlComponent());

			filter.Property = "WFL";
			AssertEquals("(JobOrWorkflow ne null)", filter.GetGlowIndexQuery().ToUrlComponent());
		}
	}
}
