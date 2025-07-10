using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Integration.DocumentEngine;

namespace Enterprise.DocumentEngine.DataProviders.BOFunctionExtractors.Testing
{
	sealed class GetMetricTargetRangeMaxFunctionExtractorTest : Core.Testing.BaseFunctionExtractorTest
	{
		public void TestConstructor()
		{
			var extractor = GetMetricTargetRangeMaxFunctionExtractor.ParseAndExtract("GetMetricTargetRangeMax(\"code\")");
			AssertEquals("code", extractor.MetricCode);
		}

		public override void TestGetMethodInfoChainLink()
		{
			var dummy = new DocDataMetricRankingCollectionForTest();
			var extractor = GetMetricTargetRangeMaxFunctionExtractor.ParseAndExtract("GetMetricTargetRangeMax(\"code\")");

			var chainLink = extractor.GetMethodInfoChainLink(typeof(IDocDataMetricRanking));

			AssertEquals("chainLink.ReflectOutObject(dummy, dummy)", new ZDecimal(69), chainLink.ReflectOutObject(dummy, dummy));
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
				return new ZDecimal(69);
			}

			public ZDecimal GetMetricTargetRangeMin(ZString metricCode)
			{
				return ZDecimal.Zero;
			}
		}
	}
}
