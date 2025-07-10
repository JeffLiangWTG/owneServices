using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.Testing;
using GlowIndexQueryService.Business;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Module.Test
{
	[TestedType(typeof(IndexSearchTagWithJobOrWorkflowFilter))]
	public class IndexSearchTagWithJobOrWorkflowFilterTest : IndexSearchModuleFilterTestCase<IndexSearchTagWithJobOrWorkflowFilter>
	{
		protected override FilterCategory ExpectedDefaultCategory => expectedDefaultCategory;

		static readonly FilterCategory expectedDefaultCategory = new FilterCategory((NoResString)"Buffer Management");

		protected override ZString ExpectedDescription => "Tag";

		public override void TestGetGlowIndexQuery()
		{
			var field = SearchField.Create("Tag", "Tag");
			var filter = new IndexSearchTagWithJobOrWorkflowFilter(field, ExpectedDefaultCategory, DummyModuleIDs.Dummy, GetTagDefinitionCodeFilter, List);
			var guidString = "39a44233-ae33-46af-9005-2690617ef502";
			filter.Property = new ZGuid(guidString);

			filter.ComparisonOperator = ModuleGuidAppliedToSubCollectionFilter.IsAppliedComparisonOperator;
			AssertEquals($"(Tag eq {guidString})", filter.GetGlowIndexQuery().ToUrlComponent());

			filter.ComparisonOperator = ModuleGuidAppliedToSubCollectionFilter.NotAppliedComparisonOperator;
			AssertEquals($"(Tag ne {guidString})", filter.GetGlowIndexQuery().ToUrlComponent());
		}

		protected override ModuleFilter GetNewModuleFilter()
		{
			var field = SearchField.Create("Tag", "Tag");
			return new IndexSearchTagWithJobOrWorkflowFilter(field, ExpectedDefaultCategory, DummyModuleIDs.Dummy, GetTagDefinitionCodeFilter, List);
		}

		DummyBusinessObjectCollection List => new DummyBusinessObjectCollection(Factory);

		IGlowQuery GetTagDefinitionCodeFilter(string fieldName, ZGuid property, ZString dropDownTypeName, string comparisonOperator)
		{
			return IndexSearchTagWithJobOrWorkflowFilter.GetGlowIndexQueryCore(fieldName, property, comparisonOperator);
		}
	}
}
