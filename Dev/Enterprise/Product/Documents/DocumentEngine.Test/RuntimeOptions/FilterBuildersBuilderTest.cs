using System.Linq;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	sealed class FilterBuildersBuilderTest : TestCase
	{
		public void TestFilterBuilderDocumenters()
		{
			var builder = new FilterBuildersBuilder();
			var documenters = builder.FilterBuilderDocumenters;
			Assert("Builder should have documenters defined.", documenters.Count > 0);
			foreach (var documenter in documenters)
			{
				Assert("Each documenter should have documentation.", documenters != null);
			}

			var lookupDocumenter = documenters.FirstOrDefault(d => d.Useage == "{lookup type} Lookup");
			AssertCollectionContains("LookupBuilder should contains LinkToScheduledReportRecipientForOrganisation for Report Reference Guide", "LinkToScheduledReportRecipientForOrganisation", lookupDocumenter.SupportedProperties);
		}
	}
}
