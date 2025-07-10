using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business.Warehouse;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(PerformanceReportingMetricCollection))]
	sealed class PerformanceReportingMetricCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<PerformanceReportingMetricCollection>
	{
		#region TestIndexer_UsingName

		public void TestIndexer_UsingName()
		{
			var settings = new PerformanceReportingMetricCollection();
			var setting1 = settings.AddNew(PerformanceReportingMetricType.Codes.InboundDamageFreeReceiptsJob);
			var setting2 = settings.AddNew(PerformanceReportingMetricType.Codes.OutboundBackOrdersAsAPercentOfTotalOrders);

			AssertEquals(setting1, settings[PerformanceReportingMetricType.Codes.InboundDamageFreeReceiptsJob]);
			AssertEquals(setting2, settings[PerformanceReportingMetricType.Codes.OutboundBackOrdersAsAPercentOfTotalOrders]);
		}

		#endregion

		#region 

		#region TestGetMetricRanking

		public void TestGetMetricRanking()
		{
			var settings = PerformanceReportingMetricCollection.GetDefault();

			AssertEquals(PerformanceReportingMetricCategory.TypeBestInClass, settings.GetMetricRanking(PerformanceReportingMetricType.Codes.OutboundOnTimeReadyToShip, new ZDecimal(100)));
			AssertEquals(PerformanceReportingMetricCategory.TypeBestInClass, settings.GetMetricRanking(PerformanceReportingMetricType.Codes.OutboundOnTimeReadyToShip, new ZDecimal(99.98)));
			AssertEquals(PerformanceReportingMetricCategory.TypeAdvantage, settings.GetMetricRanking(PerformanceReportingMetricType.Codes.OutboundOnTimeReadyToShip, new ZDecimal(99.5)));
			AssertEquals(PerformanceReportingMetricCategory.TypeAdvantage, settings.GetMetricRanking(PerformanceReportingMetricType.Codes.OutboundOnTimeReadyToShip, new ZDecimal(99.2)));
			AssertEquals(PerformanceReportingMetricCategory.TypeTypical, settings.GetMetricRanking(PerformanceReportingMetricType.Codes.OutboundOnTimeReadyToShip, new ZDecimal(98.5)));
			AssertEquals(PerformanceReportingMetricCategory.TypeTypical, settings.GetMetricRanking(PerformanceReportingMetricType.Codes.OutboundOnTimeReadyToShip, new ZDecimal(98)));
			AssertEquals(PerformanceReportingMetricCategory.TypeDisadvantage, settings.GetMetricRanking(PerformanceReportingMetricType.Codes.OutboundOnTimeReadyToShip, new ZDecimal(95)));
			AssertEquals(PerformanceReportingMetricCategory.TypeDisadvantage, settings.GetMetricRanking(PerformanceReportingMetricType.Codes.OutboundOnTimeReadyToShip, new ZDecimal(93)));
			AssertEquals(PerformanceReportingMetricCategory.TypeMajorOpportunity, settings.GetMetricRanking(PerformanceReportingMetricType.Codes.OutboundOnTimeReadyToShip, new ZDecimal(69)));

			// same as the above but values are divided by 100 as per the actual doc.
			AssertEquals(PerformanceReportingMetricCategory.TypeBestInClass, settings.GetMetricRanking(PerformanceReportingMetricType.Codes.OutboundOnTimeReadyToShip, new ZDecimal(1), isPercentage: true));
			AssertEquals(PerformanceReportingMetricCategory.TypeBestInClass, settings.GetMetricRanking(PerformanceReportingMetricType.Codes.OutboundOnTimeReadyToShip, new ZDecimal(0.9998), isPercentage: true));
			AssertEquals(PerformanceReportingMetricCategory.TypeAdvantage, settings.GetMetricRanking(PerformanceReportingMetricType.Codes.OutboundOnTimeReadyToShip, new ZDecimal(0.995), isPercentage: true));
			AssertEquals(PerformanceReportingMetricCategory.TypeAdvantage, settings.GetMetricRanking(PerformanceReportingMetricType.Codes.OutboundOnTimeReadyToShip, new ZDecimal(0.992), isPercentage: true));
			AssertEquals(PerformanceReportingMetricCategory.TypeTypical, settings.GetMetricRanking(PerformanceReportingMetricType.Codes.OutboundOnTimeReadyToShip, new ZDecimal(0.985), isPercentage: true));
			AssertEquals(PerformanceReportingMetricCategory.TypeTypical, settings.GetMetricRanking(PerformanceReportingMetricType.Codes.OutboundOnTimeReadyToShip, new ZDecimal(0.98), isPercentage: true));
			AssertEquals(PerformanceReportingMetricCategory.TypeDisadvantage, settings.GetMetricRanking(PerformanceReportingMetricType.Codes.OutboundOnTimeReadyToShip, new ZDecimal(0.95), isPercentage: true));
			AssertEquals(PerformanceReportingMetricCategory.TypeDisadvantage, settings.GetMetricRanking(PerformanceReportingMetricType.Codes.OutboundOnTimeReadyToShip, new ZDecimal(0.93), isPercentage: true));
			AssertEquals(PerformanceReportingMetricCategory.TypeMajorOpportunity, settings.GetMetricRanking(PerformanceReportingMetricType.Codes.OutboundOnTimeReadyToShip, new ZDecimal(0.69), isPercentage: true));
		}

		#endregion

		#region TestGetMetricTargetRanking

		public void TestGetMetricTargetRanking()
		{
			var settings = PerformanceReportingMetricCollection.GetDefault();

			// default target is 'typical'
			AssertEquals(PerformanceReportingMetricCategory.TypeTypical, settings.GetMetricTargetRanking(PerformanceReportingMetricType.Codes.OutboundOnTimeReadyToShip));

			settings[PerformanceReportingMetricType.Codes.OutboundOnTimeReadyToShip].MetricCategories[PerformanceReportingMetricCategory.TypeTypical].IsTarget = false;
			settings[PerformanceReportingMetricType.Codes.OutboundOnTimeReadyToShip].MetricCategories[PerformanceReportingMetricCategory.TypeBestInClass].IsTarget = true;
			AssertEquals(PerformanceReportingMetricCategory.TypeBestInClass, settings.GetMetricTargetRanking(PerformanceReportingMetricType.Codes.OutboundOnTimeReadyToShip));
		}

		#endregion

		#region TestGetMetricTargetRangeMin

		public void TestGetMetricTargetRangeMin()
		{
			var settings = PerformanceReportingMetricCollection.GetDefault();

			var metricCategories = settings[PerformanceReportingMetricType.Codes.OutboundOnTimeReadyToShip].MetricCategories;

			metricCategories[PerformanceReportingMetricCategory.TypeTypical].IsTarget = false;
			metricCategories[PerformanceReportingMetricCategory.TypeBestInClass].IsTarget = true;
			AssertEquals(new ZDecimal(99.98), settings.GetMetricTargetRangeMin(PerformanceReportingMetricType.Codes.OutboundOnTimeReadyToShip));

			metricCategories[PerformanceReportingMetricCategory.TypeBestInClass].IsTarget = false;
			metricCategories[PerformanceReportingMetricCategory.TypeAdvantage].IsTarget = true;
			AssertEquals(new ZDecimal(99.2), settings.GetMetricTargetRangeMin(PerformanceReportingMetricType.Codes.OutboundOnTimeReadyToShip));

			metricCategories[PerformanceReportingMetricCategory.TypeAdvantage].IsTarget = false;
			metricCategories[PerformanceReportingMetricCategory.TypeTypical].IsTarget = true;
			AssertEquals(new ZDecimal(98), settings.GetMetricTargetRangeMin(PerformanceReportingMetricType.Codes.OutboundOnTimeReadyToShip));

			metricCategories[PerformanceReportingMetricCategory.TypeTypical].IsTarget = false;
			metricCategories[PerformanceReportingMetricCategory.TypeDisadvantage].IsTarget = true;
			AssertEquals(new ZDecimal(93), settings.GetMetricTargetRangeMin(PerformanceReportingMetricType.Codes.OutboundOnTimeReadyToShip));

			metricCategories[PerformanceReportingMetricCategory.TypeDisadvantage].IsTarget = false;
			metricCategories[PerformanceReportingMetricCategory.TypeMajorOpportunity].IsTarget = true;
			AssertEquals(new ZDecimal(0), settings.GetMetricTargetRangeMin(PerformanceReportingMetricType.Codes.OutboundOnTimeReadyToShip));
		}

		#endregion

		#region TestGetMetricTargetRangeMax

		public void TestGetMetricTargetRangeMax()
		{
			var settings = PerformanceReportingMetricCollection.GetDefault();

			var metricCategories = settings[PerformanceReportingMetricType.Codes.OutboundOnTimeReadyToShip].MetricCategories;

			metricCategories[PerformanceReportingMetricCategory.TypeTypical].IsTarget = false;
			metricCategories[PerformanceReportingMetricCategory.TypeBestInClass].IsTarget = true;
			AssertEquals(new ZDecimal(0), settings.GetMetricTargetRangeMax(PerformanceReportingMetricType.Codes.OutboundOnTimeReadyToShip));

			metricCategories[PerformanceReportingMetricCategory.TypeBestInClass].IsTarget = false;
			metricCategories[PerformanceReportingMetricCategory.TypeAdvantage].IsTarget = true;
			AssertEquals(new ZDecimal(99.98), settings.GetMetricTargetRangeMax(PerformanceReportingMetricType.Codes.OutboundOnTimeReadyToShip));

			metricCategories[PerformanceReportingMetricCategory.TypeAdvantage].IsTarget = false;
			metricCategories[PerformanceReportingMetricCategory.TypeTypical].IsTarget = true;
			AssertEquals(new ZDecimal(99.2), settings.GetMetricTargetRangeMax(PerformanceReportingMetricType.Codes.OutboundOnTimeReadyToShip));

			metricCategories[PerformanceReportingMetricCategory.TypeTypical].IsTarget = false;
			metricCategories[PerformanceReportingMetricCategory.TypeDisadvantage].IsTarget = true;
			AssertEquals(new ZDecimal(98), settings.GetMetricTargetRangeMax(PerformanceReportingMetricType.Codes.OutboundOnTimeReadyToShip));

			metricCategories[PerformanceReportingMetricCategory.TypeDisadvantage].IsTarget = false;
			metricCategories[PerformanceReportingMetricCategory.TypeMajorOpportunity].IsTarget = true;
			AssertEquals(new ZDecimal(93), settings.GetMetricTargetRangeMax(PerformanceReportingMetricType.Codes.OutboundOnTimeReadyToShip));
		}

		#endregion

		#region TestGetMetricTargetRange

		public void TestGetMetricTargetRange()
		{
			var settings = PerformanceReportingMetricCollection.GetDefault();

			var metricCategories = settings[PerformanceReportingMetricType.Codes.OutboundOnTimeReadyToShip].MetricCategories;

			metricCategories[PerformanceReportingMetricCategory.TypeTypical].IsTarget = false;
			metricCategories[PerformanceReportingMetricCategory.TypeBestInClass].IsTarget = true;
			AssertEquals(">= 99.98", settings.GetMetricTargetRange(PerformanceReportingMetricType.Codes.OutboundOnTimeReadyToShip));

			metricCategories[PerformanceReportingMetricCategory.TypeBestInClass].IsTarget = false;
			metricCategories[PerformanceReportingMetricCategory.TypeAdvantage].IsTarget = true;
			AssertEquals(">= 99.2 & < 99.98", settings.GetMetricTargetRange(PerformanceReportingMetricType.Codes.OutboundOnTimeReadyToShip));

			metricCategories[PerformanceReportingMetricCategory.TypeAdvantage].IsTarget = false;
			metricCategories[PerformanceReportingMetricCategory.TypeTypical].IsTarget = true;
			AssertEquals(">= 98 & < 99.2", settings.GetMetricTargetRange(PerformanceReportingMetricType.Codes.OutboundOnTimeReadyToShip));

			metricCategories[PerformanceReportingMetricCategory.TypeTypical].IsTarget = false;
			metricCategories[PerformanceReportingMetricCategory.TypeDisadvantage].IsTarget = true;
			AssertEquals(">= 93 & < 98", settings.GetMetricTargetRange(PerformanceReportingMetricType.Codes.OutboundOnTimeReadyToShip));

			metricCategories[PerformanceReportingMetricCategory.TypeDisadvantage].IsTarget = false;
			metricCategories[PerformanceReportingMetricCategory.TypeMajorOpportunity].IsTarget = true;
			AssertEquals("< 93", settings.GetMetricTargetRange(PerformanceReportingMetricType.Codes.OutboundOnTimeReadyToShip));
		}

		#endregion

		#endregion

		#region Implementation

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new PerformanceReportingMetric();
		}

		protected override PerformanceReportingMetricCollection GetCollectionToTest()
		{
			return new PerformanceReportingMetricCollection();
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
