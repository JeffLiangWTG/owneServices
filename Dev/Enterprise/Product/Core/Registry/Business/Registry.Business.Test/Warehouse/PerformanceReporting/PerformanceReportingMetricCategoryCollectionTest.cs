using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business.Warehouse;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(PerformanceReportingMetricCategoryCollection))]
	sealed class PerformanceReportingMetricCategoryCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<PerformanceReportingMetricCategoryCollection>
	{
		#region TestIndexer_UsingName

		public void TestIndexer_UsingName()
		{
			var categories = new PerformanceReportingMetricCategoryCollection(null);
			var category1 = categories.AddNew();
			category1.EnglishCategoryName = "Cat 1";

			var category2 = categories.AddNew();
			category2.EnglishCategoryName = "Cat 2";

			AssertEquals(category1, categories["Cat 1"]);
			AssertEquals(category2, categories["Cat 2"]);
		}

		#endregion

		#region TestIsDuplicateSetting

		public void TestIsDuplicateSetting()
		{
			var categories = new PerformanceReportingMetricCategoryCollection(null);
			var category1 = categories.AddNew();
			category1.EnglishCategoryName = "Cat 1";
			AssertEquals("Should not be duplicate", false, categories.IsDuplicateSetting(category1));

			var category2 = categories.AddNew();
			category2.EnglishCategoryName = category1.CategoryName;
			AssertEquals("Should be duplicate", true, categories.IsDuplicateSetting(category1));

			category2.EnglishCategoryName = "Cat 2";
			AssertEquals("Should not be duplicate", false, categories.IsDuplicateSetting(category1));
		}

		#endregion

		#region TestInitializeDefault

		public void TestInitializeDefault()
		{
			var collection = new PerformanceReportingMetricCategoryCollection(null);
			collection.InitializeDefault(PerformanceReportingMetricType.Codes.OutboundOnTimeReadyToShip);
			AssertEquals(collection[PerformanceReportingMetricCategory.TypeBestInClass].Value, new ZDecimal(99.98));
			AssertEquals(collection[PerformanceReportingMetricCategory.TypeAdvantage].Value, new ZDecimal(99.2));
			AssertEquals(collection[PerformanceReportingMetricCategory.TypeTypical].Value, new ZDecimal(98));
			AssertEquals(collection[PerformanceReportingMetricCategory.TypeDisadvantage].Value, new ZDecimal(93));
			AssertEquals(collection[PerformanceReportingMetricCategory.TypeMajorOpportunity].Value, new ZDecimal(93));

			collection = new PerformanceReportingMetricCategoryCollection(null);
			collection.InitializeDefault(PerformanceReportingMetricType.Codes.OutboundTotalOrderCycleTimeEnteredToReleased);
			AssertEquals(collection[PerformanceReportingMetricCategory.TypeMajorOpportunity].Value, new ZDecimal(48));
			AssertEquals(collection[PerformanceReportingMetricCategory.TypeDisadvantage].Value, new ZDecimal(24));
			AssertEquals(collection[PerformanceReportingMetricCategory.TypeTypical].Value, new ZDecimal(13.2));
			AssertEquals(collection[PerformanceReportingMetricCategory.TypeAdvantage].Value, new ZDecimal(5.8));
			AssertEquals(collection[PerformanceReportingMetricCategory.TypeBestInClass].Value, new ZDecimal(5.8));

			collection = new PerformanceReportingMetricCategoryCollection(null);
			collection.InitializeDefault(PerformanceReportingMetricType.Codes.OutboundFillRateOrder);
			AssertEquals(collection[PerformanceReportingMetricCategory.TypeBestInClass].Value, new ZDecimal(99.7));
			AssertEquals(collection[PerformanceReportingMetricCategory.TypeAdvantage].Value, new ZDecimal(98.29));
			AssertEquals(collection[PerformanceReportingMetricCategory.TypeTypical].Value, new ZDecimal(96.97));
			AssertEquals(collection[PerformanceReportingMetricCategory.TypeDisadvantage].Value, new ZDecimal(92));
			AssertEquals(collection[PerformanceReportingMetricCategory.TypeMajorOpportunity].Value, new ZDecimal(92));

			collection = new PerformanceReportingMetricCategoryCollection(null);
			collection.InitializeDefault(PerformanceReportingMetricType.Codes.OutboundFillRateLine);
			AssertEquals(collection[PerformanceReportingMetricCategory.TypeBestInClass].Value, new ZDecimal(99.7));
			AssertEquals(collection[PerformanceReportingMetricCategory.TypeAdvantage].Value, new ZDecimal(98.6));
			AssertEquals(collection[PerformanceReportingMetricCategory.TypeTypical].Value, new ZDecimal(97.5));
			AssertEquals(collection[PerformanceReportingMetricCategory.TypeDisadvantage].Value, new ZDecimal(94.34));
			AssertEquals(collection[PerformanceReportingMetricCategory.TypeMajorOpportunity].Value, new ZDecimal(94.34));

			collection = new PerformanceReportingMetricCategoryCollection(null);
			collection.InitializeDefault(PerformanceReportingMetricType.Codes.OutboundBackOrdersAsAPercentOfTotalOrders);
			AssertEquals(collection[PerformanceReportingMetricCategory.TypeMajorOpportunity].Value, new ZDecimal(9.74));
			AssertEquals(collection[PerformanceReportingMetricCategory.TypeDisadvantage].Value, new ZDecimal(3));
			AssertEquals(collection[PerformanceReportingMetricCategory.TypeTypical].Value, new ZDecimal(1));
			AssertEquals(collection[PerformanceReportingMetricCategory.TypeAdvantage].Value, new ZDecimal(0.084));
			AssertEquals(collection[PerformanceReportingMetricCategory.TypeBestInClass].Value, new ZDecimal(0.084));

			collection = new PerformanceReportingMetricCategoryCollection(null);
			collection.InitializeDefault(PerformanceReportingMetricType.Codes.InboundDockToStock);
			AssertEquals(collection[PerformanceReportingMetricCategory.TypeMajorOpportunity].Value, new ZDecimal(34.4));
			AssertEquals(collection[PerformanceReportingMetricCategory.TypeDisadvantage].Value, new ZDecimal(16));
			AssertEquals(collection[PerformanceReportingMetricCategory.TypeTypical].Value, new ZDecimal(7));
			AssertEquals(collection[PerformanceReportingMetricCategory.TypeAdvantage].Value, new ZDecimal(4));
			AssertEquals(collection[PerformanceReportingMetricCategory.TypeBestInClass].Value, new ZDecimal(4));

			collection = new PerformanceReportingMetricCategoryCollection(null);
			collection.InitializeDefault(PerformanceReportingMetricType.Codes.InboundOnTimeReceipts);
			AssertEquals(collection[PerformanceReportingMetricCategory.TypeBestInClass].Value, new ZDecimal(97.3));
			AssertEquals(collection[PerformanceReportingMetricCategory.TypeAdvantage].Value, new ZDecimal(95));
			AssertEquals(collection[PerformanceReportingMetricCategory.TypeTypical].Value, new ZDecimal(90));
			AssertEquals(collection[PerformanceReportingMetricCategory.TypeDisadvantage].Value, new ZDecimal(84));
			AssertEquals(collection[PerformanceReportingMetricCategory.TypeMajorOpportunity].Value, new ZDecimal(84));
		}

		#endregion

		public void TestTargetCategory()
		{
			var collection = new PerformanceReportingMetricCategoryCollection(null);
			collection.InitializeDefault(PerformanceReportingMetricType.Codes.OutboundOnTimeReadyToShip);

			AssertEquals(PerformanceReportingMetricCategory.TypeTypical, collection.TargetCategory.CategoryName);

			collection[PerformanceReportingMetricCategory.TypeTypical].IsTarget = false;
			collection[PerformanceReportingMetricCategory.TypeBestInClass].IsTarget = true;
			AssertEquals(PerformanceReportingMetricCategory.TypeBestInClass, collection.TargetCategory.CategoryName);
		}

		#region Implementation

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new PerformanceReportingMetricCategory();
		}

		protected override PerformanceReportingMetricCategoryCollection GetCollectionToTest()
		{
			return new PerformanceReportingMetricCategoryCollection(null);
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
