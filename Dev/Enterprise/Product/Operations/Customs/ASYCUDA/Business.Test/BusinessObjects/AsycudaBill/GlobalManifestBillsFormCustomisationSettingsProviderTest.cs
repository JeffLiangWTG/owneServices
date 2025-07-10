using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	[TestedType(typeof(GlobalManifestBillsFormCustomisationSettingsProvider))]
	sealed class GlobalManifestBillsFormCustomisationSettingsProviderTest : FormCustomisationSettingsProviderTest<GlobalManifestBillsFormCustomisationSettingsProvider>
	{
		public override void TestPropertiesThatAffectWorkflow()
		{
			var expected = new string[]
			{
				AsycudaBill.Schema.ABL_ShipmentType
			};
			var provider = GetNewProvider();
			AssertContainsExactElementsInAnyOrder(expected, provider.PropertiesThatAffectWorkflow);
		}

		public override void TestDisplayTabs()
		{
			var provider = GetNewProvider();
			AssertEquals(0, provider.DisplayTabs.Count);
			AssertEquals(null, provider.DisplayTabs.Settings);
		}

		public override void TestTabPlacementProhibitions()
		{
			var provider = GetNewProvider();
			AssertEquals(0, provider.TabPlacementProhibitions.Length);
		}

		public override GlobalManifestBillsFormCustomisationSettingsProvider GetNewProvider() => new GlobalManifestBillsFormCustomisationSettingsProvider();
	}
}
