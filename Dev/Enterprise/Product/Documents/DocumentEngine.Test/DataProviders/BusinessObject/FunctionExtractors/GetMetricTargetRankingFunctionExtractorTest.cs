using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Integration.DocumentEngine;

namespace Enterprise.DocumentEngine.DataProviders.BOFunctionExtractors.Testing
{
	sealed class GetMetricTargetRankingFunctionExtractorTest : Core.Testing.BaseFunctionExtractorTest
	{
		public void TestConstructor()
		{
			var extractor = GetMetricTargetRankingFunctionExtractor.ParseAndExtract("GetMetricTargetRanking(\"code\")");
			AssertEquals("code", extractor.MetricCode);
		}

		public override void TestGetMethodInfoChainLink()
		{
			var dummy = new DocDataMetricRankingCollectionForTest();
			var extractor = GetMetricTargetRankingFunctionExtractor.ParseAndExtract("GetMetricTargetRanking(\"code\")");

			var chainLink = extractor.GetMethodInfoChainLink(typeof(IDocDataMetricRanking));

			AssertEquals("chainLink.ReflectOutObject(dummy, dummy)", "MetricTargetRanking for code", chainLink.ReflectOutObject(dummy, dummy));
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
				return "MetricTargetRange result";
			}

			public ZString GetMetricTargetRanking(ZString metricCode)
			{
				return string.Format("MetricTargetRanking for {0}", metricCode);
			}

			public ZDecimal GetMetricTargetRangeMax(ZString metricCode)
			{
				return ZDecimal.Zero;
			}

			public ZDecimal GetMetricTargetRangeMin(ZString metricCode)
			{
				return ZDecimal.Zero;
			}
		}
	}
}
