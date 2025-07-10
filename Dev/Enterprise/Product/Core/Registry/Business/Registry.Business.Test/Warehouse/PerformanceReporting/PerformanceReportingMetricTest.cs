using CargoWise.Types;
using Enterprise.Registry.Business.Warehouse;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(PerformanceReportingMetric))]
	sealed class PerformanceReportingMetricTest : RegistryBusinessObjectTemplateTestCase<PerformanceReportingMetric>
	{
		#region Properties

		#region TestMetricCode

		public void TestMetricCode()
		{
			var metric = new PerformanceReportingMetric();
			AssertEquals(ZString.Empty, metric.MetricCode);

			metric.MetricCode = "Code   ";
			AssertEquals("Code", metric.MetricCode);
		}

		#endregion

		#region TestMetricName

		public void TestMetricName()
		{
			var metric = new PerformanceReportingMetric();
			AssertEquals("Should be empty when there is no code set.", ZString.Empty, metric.MetricName);

			metric.MetricCode = PerformanceReportingMetricType.Codes.InboundDockToStock;
			AssertEquals("Should have the name corresponding to the code", "Inbound - Dock To Stock", metric.MetricName);
		}

		#endregion

		#region TestMetricCategories

		public void TestMetricCategories()
		{
			var metric = new PerformanceReportingMetric();
			AssertNotNull(metric.MetricCategories);
		}

		#endregion

		#endregion

		#region Validation

		#region TestValidateMetricCode

		public void TestValidateMetricCode()
		{
			var metricSettings = new PerformanceReportingMetricCollection();
			var metricSetting1 = metricSettings.AddNew(PerformanceReportingMetricType.Codes.InboundDamageFreeReceiptsJob);
			metricSetting1.MetricCode = "";
			metricSettings.RunPreSaveValidation();
			AssertHasErrorContaining(metricSetting1.MetricCodeInfo, "Please enter a Metric Code.");

			metricSetting1.MetricCode = PerformanceReportingMetricType.Codes.InboundDamageFreeReceiptsJob;
			AssertNoErrorContaining(metricSetting1.MetricCodeInfo, "Please enter a Metric Code.");
		}

		#endregion

		#endregion

		#region Implementation

		protected override PerformanceReportingMetric GetBusinessObjectToClone()
		{
			var result = new PerformanceReportingMetric();
			result.FillWithValidTestData();

			var category = result.MetricCategories.AddNew();
			category.FillWithValidTestData();

			return result;
		}

		protected override PerformanceReportingMetric GetBusinessObjectToSerialise()
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
