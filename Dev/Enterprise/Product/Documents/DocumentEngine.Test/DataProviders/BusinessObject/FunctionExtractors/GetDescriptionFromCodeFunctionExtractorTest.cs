using Enterprise.DocumentEngine.DataProviders.BOFunctionExtractors.Core.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.DataProviders.BOFunctionExtractors.Testing
{
	sealed class GetDescriptionFromCodeFunctionExtractorTest : BaseFunctionExtractorTest
	{
		public override void TestGetMethodInfoChainLink()
		{
			var opportunities = new OpportunityStatusCollection()
			{
				{ "CRT", (NoResString)"Current", false, false, true, "" },
				{ "ARB", (NoResString)"Arbitrary", false, false, false, "" },
				{ "TST", (NoResString)"Test", false, false, true, "" }
			};

			var extractor = new GetDescriptionFromCodeFunctionExtractor("CRT");
			var chainLink = extractor.GetMethodInfoChainLink(opportunities.GetType());

			AssertEquals("Current", chainLink.ReflectOutObject(opportunities, opportunities));
		}
	}
}
