using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Integration.DocumentEngine;

namespace Enterprise.DocumentEngine.DataProviders.BOFunctionExtractors.Testing
{
	sealed class GetMetricTargetRangeFunctionExtractorTest : Core.Testing.BaseFunctionExtractorTest
	{
		public void TestConstructor()
		{
			var extractor = GetMetricTargetRangeFunctionExtractor.ParseAndExtract("GetMetricTargetRange(\"code\")");
			AssertEquals("code", extractor.MetricCode);
		}

		public override void TestGetMethodInfoChainLink()
		{
			var dummy = new DocDataMetricRankingCollectionForTest();
			var extractor = GetMetricTargetRangeFunctionExtractor.ParseAndExtract("GetMetricTargetRange(\"code\")");

			var chainLink = extractor.GetMethodInfoChainLink(typeof(IDocDataMetricRanking));

			AssertEquals("chainLink.ReflectOutObject(dummy, dummy)", "MetricTargetRange for code", chainLink.ReflectOutObject(dummy, dummy));
		}

		class MyWrapper : DocumentWrapper
		{
		}

		class DocDataMetricRankingCollectionForTest : DocumentWrapperCollection<MyWrapper>, IDocDataMetricRanking
		{
			public DocDataMetricRankingCollectionForTest()
				: base(null)
			{
			}

			public ZString GetMetricRanking(ZString metricCode, ZDecimal metricValue, bool isPercentage)
			{
				return "MetricRanking result";
			}

			public ZString GetMetricTargetRange(ZString metricCode)
			{
				return string.Format("MetricTargetRange for {0}", metricCode);
			}

			public ZString GetMetricTargetRanking(ZString metricCode)
			{
				return "MetricTargetRanking result";
			}

			public ZDecimal GetMetricTargetRangeMax(ZString metricCode)
			{
				return ZDecimal.Zero;
			}

			public ZDecimal GetMetricTargetRangeMin(ZString metricCode)
			{
				return new ZDecimal(69);
			}
		}
	}
}
