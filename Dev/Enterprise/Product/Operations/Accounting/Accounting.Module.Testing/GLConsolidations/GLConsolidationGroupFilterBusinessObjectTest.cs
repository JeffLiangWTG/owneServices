using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.GeneralLedger.GLConsolidations;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(GLConsolidationGroupFilterBusinessObject))]
	public class GLConsolidationGroupFilterBusinessObjectTest : AccountingFilterStripBusinessObjectTestCase
	{
		public void TestCodeFilter()
		{
			Group1.YR_Code = "ABC";
			Group2.YR_Code = "XYZ";
			Factory.Save();

			AssertContainsExactElementsInAnyOrder(new[] { Group1, Group2 }, FilterCollection.Find(new ZQuery()));

			var filter = (ModuleTextFilter)FilterBO["Code"];
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "ABC";
			filter.IsActive = true;

			AssertContainsExactElementsInAnyOrder(new[] { Group1 }, FilterCollection.Find(FilterBO.Filter));

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "111";
			filter.IsActive = true;

			AssertContainsExactElementsInAnyOrder(System.Array.Empty<AccConsolidationGroup>(), FilterCollection.Find(FilterBO.Filter));

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "XYZ";
			filter.IsActive = true;

			AssertContainsExactElementsInAnyOrder(new[] { Group2 }, FilterCollection.Find(FilterBO.Filter));
		}

		public void TestDescriptionFilter()
		{
			Group1.YR_Description = "ABC Description";
			Group2.YR_Description = "XYZ Description";
			Factory.Save();

			AssertContainsExactElementsInAnyOrder(new[] { Group1, Group2 }, FilterCollection.Find(new ZQuery()));

			var filter = (ModuleTextFilter)FilterBO["Description"];
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "Desc";
			filter.IsActive = true;

			AssertContainsExactElementsInAnyOrder(new[] { Group1, Group2 }, FilterCollection.Find(FilterBO.Filter));

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "ABC Description";
			filter.IsActive = true;

			AssertContainsExactElementsInAnyOrder(new[] { Group1 }, FilterCollection.Find(FilterBO.Filter));

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "XYZ";
			filter.IsActive = true;

			AssertContainsExactElementsInAnyOrder(new[] { Group2 }, FilterCollection.Find(FilterBO.Filter));
		}

		public void TestParentGroupFilter()
		{
			Group1.YR_YR_ConsolidationGroup = Group2.PK;
			Factory.Save();

			AssertContainsExactElementsInAnyOrder(new[] { Group1, Group2 }, FilterCollection.Find(new ZQuery()));

			var filter = (ModuleGuidFilter)FilterBO["Parent Group"];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = Group1.PK;
			filter.IsActive = true;

			AssertContainsExactElementsInAnyOrder(System.Array.Empty<AccConsolidationGroup>(), FilterCollection.Find(FilterBO.Filter));

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = Group2.PK;
			filter.IsActive = true;

			AssertContainsExactElementsInAnyOrder(new[] { Group1 }, FilterCollection.Find(FilterBO.Filter));
		}

		public void TestLastProcessedDateFilter()
		{
			Group1.YR_HighWatermark = new ZDateTime(2012, 01, 01);
			Group2.YR_HighWatermark = new ZDateTime(2013, 01, 01);
			Factory.Save();

			AssertContainsExactElementsInAnyOrder(new[] { Group1, Group2 }, FilterCollection.Find(new ZQuery()));

			var filter = (ModuleDateFilter)FilterBO["Last Processed Date"];

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.IsActive = true;
			filter.Property1 = new ZDateTime(2012, 01, 01);
			filter.Property2 = new ZDateTime(2012, 01, 02);

			AssertContainsExactElementsInAnyOrder(new[] { Group1 }, FilterCollection.Find(FilterBO.Filter));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.IsActive = true;
			filter.Property1 = new ZDateTime(2013, 01, 01);
			filter.Property2 = new ZDateTime(2013, 01, 02);

			AssertContainsExactElementsInAnyOrder(new[] { Group2 }, FilterCollection.Find(FilterBO.Filter));
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new GLConsolidationGroupFilterBusinessObject();
		}

		AccConsolidationGroup Group1;
		AccConsolidationGroup Group2;

		AccConsolidationGroupCollection FilterCollection;
		GLConsolidationGroupFilterBusinessObject FilterBO;

		protected override void SetUp()
		{
			base.SetUp();

			Group1 = Factory.New<AccConsolidationGroup>();
			Group1.YR_Code = "ABC";

			Group2 = Factory.New<AccConsolidationGroup>();
			Group2.YR_Code = "XYZ";

			Factory.Save();

			FilterCollection = new AccConsolidationGroupCollection(Factory);
			FilterBO = (GLConsolidationGroupFilterBusinessObject)GetNewFilterStripBusinessObject();
		}
	}
}
