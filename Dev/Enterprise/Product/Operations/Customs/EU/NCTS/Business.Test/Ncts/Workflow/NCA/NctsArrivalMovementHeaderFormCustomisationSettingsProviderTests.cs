using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(NctsArrivalMovementHeaderFormCustomisationSettingsProvider))]
	sealed class NctsArrivalMovementHeaderFormCustomisationSettingsProviderTests : FormCustomisationSettingsProviderTest<NctsArrivalMovementHeaderFormCustomisationSettingsProvider>
	{
		public override void TestDisplayTabs()
		{
			var provider = GetNewProvider();
			AssertEquals(10, provider.DisplayTabs.Count);
			AssertNotNull(((FormCustomisableElement)provider.DisplayTabs.ToArray()[0]).ElementName);
		}

		public override void TestPropertiesThatAffectWorkflow()
		{
			var expected = new string[]
			{
				NctsArrivalMovementHeader.Schema.ExportFlag,
				NctsArrivalMovementHeader.Schema.AuthorizationCode,
				NctsArrivalMovementHeader.Schema.DestinationCustomsOfficeCodeForArrival,
				NctsArrivalMovementHeader.Schema.BM_NoChangesToReport,
				NctsArrivalMovementHeader.Schema.BM_StateOfSealsBoolean
			};
			var provider = GetNewProvider();
			AssertContainsExactElementsInAnyOrder(expected, provider.PropertiesThatAffectWorkflow);
		}

		public override void TestTabPlacementProhibitions()
		{
			var provider = GetNewProvider();
			AssertEquals(0, provider.TabPlacementProhibitions.Length);
		}

		public override NctsArrivalMovementHeaderFormCustomisationSettingsProvider GetNewProvider() => new NctsArrivalMovementHeaderFormCustomisationSettingsProvider();
	}
}
