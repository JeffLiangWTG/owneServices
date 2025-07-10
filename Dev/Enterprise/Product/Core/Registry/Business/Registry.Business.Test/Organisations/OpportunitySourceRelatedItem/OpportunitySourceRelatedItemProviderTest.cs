using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	sealed class OpportunitySourceRelatedItemProviderTest : TestCase
	{
		public void TestSecondaryListMatchesMapping()
		{
			OpportunitySourceRelatedItemProvider provider = new OpportunitySourceRelatedItemProvider();
			foreach (CodeDescriptionPair pair in OpportunitySourceRelatedItemProvider.SecondaryList)
			{
				Assert("each item in list should be in mapping", provider.Mapping.ContainsKey(pair.Description));
			}
		}
	}
}
