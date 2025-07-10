using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Integration.DocumentEngine;

namespace Enterprise.DocumentEngine.DataProviders.BOFunctionExtractors.Testing
{
	sealed class GetMetricRankingFunctionExtractorTest : Core.Testing.BaseFunctionExtractorTest
	{
		public void TestConstructor()
		{
			var extractor = GetMetricRankingFunctionExtractor.ParseAndExtract("<GetMetricRanking(\"code\", 69)>");
			AssertEquals("code", extractor.MetricCode);
			AssertEquals(new ZDecimal(69), extractor.Value);
			AssertEquals(false, extractor.IsPercentage);

			var extractorWithOptionalParam = GetMetricRankingFunctionExtractor.ParseAndExtract("<GetMetricRanking(\"code\", 69, true)>");
			AssertEquals("code", extractorWithOptionalParam.MetricCode);
			AssertEquals(new ZDecimal(69), extractorWithOptionalParam.Value);
			AssertEquals(true, extractorWithOptionalParam.IsPercentage);
		}

		public override void TestGetMethodInfoChainLink()
		{
			var dummy = new DocDataMetricRankingCollectionForTest();
			var extractor = GetMetricRankingFunctionExtractor.ParseAndExtract("GetMetricRanking(\"code\", 69)");
			var chainLink = extractor.GetMethodInfoChainLink(typeof(IDocDataMetricRanking));
			AssertEquals("chainLink.ReflectOutObject(dummy, dummy)", "MetricRanking for code with 69 and isPercentage = False.", chainLink.ReflectOutObject(dummy, dummy));

			var extractorWithOptionalParam = GetMetricRankingFunctionExtractor.ParseAndExtract("GetMetricRanking(\"code\", 69, true)");
			var chainLinkWithOptionalParam = extractorWithOptionalParam.GetMethodInfoChainLink(typeof(IDocDataMetricRanking));
			AssertEquals("chainLink.ReflectOutObject(dummy, dummy)", "MetricRanking for code with 69 and isPercentage = True.", chainLinkWithOptionalParam.ReflectOutObject(dummy, dummy));
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
				return string.Format("MetricRanking for {0} with {1} and isPercentage = {2}.", metricCode, metricValue, isPercentage);
			}

			public ZString GetMetricTargetRange(ZString metricCode)
			{
				return "MetricTargetRange result";
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
				return ZDecimal.Zero;
			}
		}
	}
}
