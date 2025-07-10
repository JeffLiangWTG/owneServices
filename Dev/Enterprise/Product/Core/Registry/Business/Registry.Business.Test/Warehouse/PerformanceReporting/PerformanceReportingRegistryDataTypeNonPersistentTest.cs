using Enterprise.Registry.Business.Warehouse;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(PerformanceReportingRegistryDataType))]
	sealed class PerformanceReportingRegistryDataTypeNonPersistentTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<PerformanceReportingRegistryDataType>
	{
		protected override string ExpectedEditorName => "PerformanceReportingRegistryItemEditor";

		protected override PerformanceReportingRegistryDataType GetNewDataType()
		{
			return new PerformanceReportingRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var collection = new PerformanceReportingMetricCollection();
			var metricSetting = collection.AddNew(PerformanceReportingMetricType.Codes.InboundDamageFreeReceiptsJob);
			metricSetting.MetricCode = PerformanceReportingMetricType.Codes.InboundDamageFreeReceiptsJob;

			var collection2 = new PerformanceReportingMetricCollection();
			var metricSetting2 = collection2.AddNew(PerformanceReportingMetricType.Codes.InboundEarlyReceipts);
			metricSetting2.MetricCode = PerformanceReportingMetricType.Codes.InboundEarlyReceipts;

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection, new PerformanceReportingRegistryDataType().Serialise(collection)),
				new ValidSampleAndBinaryValueInDB(collection2, new PerformanceReportingRegistryDataType().Serialise(collection2))
			};
		}
	}
}
