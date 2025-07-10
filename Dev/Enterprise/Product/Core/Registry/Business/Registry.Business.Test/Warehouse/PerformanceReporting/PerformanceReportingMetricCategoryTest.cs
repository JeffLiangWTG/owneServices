using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Registry.Business.Warehouse;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(PerformanceReportingMetricCategory))]
	sealed class PerformanceReportingMetricCategoryTest : RegistryBusinessObjectTemplateTestCase
	{
		#region TestParent

		public void TestParent()
		{
			var metric = new PerformanceReportingMetric();
			var category = new PerformanceReportingMetricCategory();
			metric.MetricCategories.Add(category);

			AssertEquals(metric, category.Parent);
		}

		#endregion

		#region Properties

		#region TestCategoryName

		public void TestCategoryName()
		{
			var category = new PerformanceReportingMetricCategory();
			category.EnglishCategoryName = "Cat 1";
			AssertEquals("Cat 1", category.EnglishCategoryName);
		}

		#endregion

		#region TestOperator

		public void TestOperator()
		{
			var category = new PerformanceReportingMetricCategory();
			category.Operator = "<";
			AssertEquals("<", category.Operator);
		}

		#endregion

		#region TestValue

		public void TestValue()
		{
			var category = new PerformanceReportingMetricCategory();
			category.Value = 69;
			AssertEquals(new ZDecimal(69), category.Value);
		}

		#endregion

		#region TestIsTarget

		public void TestIsTarget()
		{
			var category = new PerformanceReportingMetricCategory();
			category.IsTarget = true;
			AssertEquals(true, category.IsTarget);
			category.IsTarget = false;
			AssertEquals(false, category.IsTarget);
		}

		#endregion

		#endregion

		#region Validation

		#region TestValidateCategoryName

		public void TestValidateCategoryName()
		{
			var metric = new PerformanceReportingMetric();
			var category1 = metric.MetricCategories.AddNew();
			metric.MetricCategories.RunPreSaveValidation();
			AssertHasErrorContaining(category1.CategoryNameInfo, "Please enter a value.");

			category1.EnglishCategoryName = "Cat 1";
			AssertNoErrorContaining(category1.CategoryNameInfo, "Please enter a value.");

			var category2 = metric.MetricCategories.AddNew();
			category2.EnglishCategoryName = category1.EnglishCategoryName;
			metric.MetricCategories.RunPreSaveValidation();
			AssertHasErrorContaining(category2.CategoryNameInfo, "Category Name must be unique.");

			category2.EnglishCategoryName = "Cat 2";
			metric.MetricCategories.RunPreSaveValidation();
			AssertEquals("HasErrors", false, category2.CategoryNameInfo.HasErrors());
		}

		#endregion

		#region TestValidateValue

		public void TestValidateValue()
		{
			var category = new PerformanceReportingMetricCategory();
			AssertNoErrors("Precondition", category.ValueInfo);

			category.Value = -1;
			AssertHasError(category.ValueInfo, "value cannot be negative.");

			category.Value = 1;
			AssertNoErrors(category.ValueInfo);

			var metric = new PerformanceReportingMetric();
			var category1 = metric.MetricCategories.AddNew();
			category1.Value = 10;
			AssertNoErrors(category1.ValueInfo);

			var category2 = metric.MetricCategories.AddNew();
			category2.Value = 0;
			AssertNoErrors(category1.ValueInfo);
			AssertHasError(category2.ValueInfo, "The lowest category must have its value equal to the value immediately above it.");

			category2.Value = 10;
			AssertNoErrors(category2.ValueInfo);

			var category3 = metric.MetricCategories.AddNew();
			category1.Value = 80;
			category2.Value = 40;
			category3.Value = 40;
			AssertNoErrors(category1.ValueInfo);
			AssertNoErrors(category2.ValueInfo);
			AssertNoErrors(category3.ValueInfo);

			category1.Value = 20;
			AssertHasError(category2.ValueInfo, "The value must be in descending order.");

			category1.Value = 50;
			AssertNoErrors(category2.ValueInfo);

			category1.Value = 40;
			AssertHasError(category2.ValueInfo, "The value must be in descending order.");

			category2.Value = 30;
			AssertNoErrors(category2.ValueInfo);
			AssertHasError(category3.ValueInfo, "The lowest category must have its value equal to the value immediately above it.");

			category3.Value = 30;
			AssertNoErrors(category3.ValueInfo);
		}

		#endregion

		#region TestValidateIsTarget

		public void TestValidateIsTarget()
		{
			var metric = new PerformanceReportingMetric();
			var category1 = metric.MetricCategories.AddNew();
			category1.IsTarget = false;
			AssertNoErrors(category1.IsTargetInfo);

			var category2 = metric.MetricCategories.AddNew();
			category2.IsTarget = false;
			AssertNoErrors(category1.IsTargetInfo);
			AssertNoErrors(category2.IsTargetInfo);

			category1.IsTarget = true;
			AssertNoErrors(category1.IsTargetInfo);
			AssertNoErrors(category2.IsTargetInfo);

			category2.IsTarget = true;
			AssertHasError(category1.IsTargetInfo, "Only one category can be selected as target.");
			AssertHasError(category2.IsTargetInfo, "Only one category can be selected as target.");

			category1.IsTarget = false;
			AssertNoErrors(category1.IsTargetInfo);
			AssertNoErrors(category2.IsTargetInfo);
		}

		#endregion

		#endregion

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			var result = new PerformanceReportingMetricCategory();
			result.EnglishCategoryName = "Cat 1";
			result.Value = 69;
			return result;
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		#endregion
	}
}
