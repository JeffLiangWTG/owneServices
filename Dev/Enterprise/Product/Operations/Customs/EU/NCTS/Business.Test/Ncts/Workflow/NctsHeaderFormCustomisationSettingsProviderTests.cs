using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(NctsHeaderFormCustomisationSettingsProvider))]
	sealed class NctsHeaderFormCustomisationSettingsProviderTests : FormCustomisationSettingsProviderTest<NctsHeaderFormCustomisationSettingsProvider>
	{
		public override void TestDisplayTabs()
		{
			var provider = GetNewProvider();
			AssertEquals(0, provider.DisplayTabs.Count);
		}

		public override void TestPropertiesThatAffectWorkflow()
		{
			var expected = new string[]
			{
				NctsHeader.Schema.Principal
			};
			var provider = GetNewProvider();
			AssertContainsExactElementsInAnyOrder(expected, provider.PropertiesThatAffectWorkflow);
		}

		public override void TestTabPlacementProhibitions()
		{
			var provider = GetNewProvider();
			AssertEquals(0, provider.TabPlacementProhibitions.Length);
		}
		public override NctsHeaderFormCustomisationSettingsProvider GetNewProvider() => new NctsHeaderFormCustomisationSettingsProvider();
	}
}
