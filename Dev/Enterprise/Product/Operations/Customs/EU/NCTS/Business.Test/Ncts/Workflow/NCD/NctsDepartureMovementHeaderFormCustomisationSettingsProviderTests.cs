using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(NctsDepartureMovementHeaderFormCustomisationSettingsProvider))]
	sealed class NctsDepartureMovementHeaderFormCustomisationSettingsProviderTests : FormCustomisationSettingsProviderTest<NctsDepartureMovementHeaderFormCustomisationSettingsProvider>
	{
		public override void TestDisplayTabs()
		{
			var provider = GetNewProvider();
			AssertEquals(14, provider.DisplayTabs.Count);
			AssertNotNull(((FormCustomisableElement)provider.DisplayTabs.ToArray()[0]).ElementName);
		}

		public override void TestPropertiesThatAffectWorkflow()
		{
			var expected = new string[]
			{
				NctsDepartureMovementHeader.Schema.BM_InBondEntryType,
				NctsDepartureMovementHeader.Schema.BM_AdditionalDeclarationType,
				NctsDepartureMovementHeader.Schema.BM_InlandTransportMode,
				NctsDepartureMovementHeader.Schema.IsSimplifiedNctsProcedure,
				NctsDepartureMovementHeader.Schema.BM_RL_NKDestinationPort
			};
			var provider = GetNewProvider();
			AssertContainsExactElementsInAnyOrder(expected, provider.PropertiesThatAffectWorkflow);
		}

		public override void TestTabPlacementProhibitions()
		{
			var provider = GetNewProvider();
			AssertEquals(0, provider.TabPlacementProhibitions.Length);
		}

		public override NctsDepartureMovementHeaderFormCustomisationSettingsProvider GetNewProvider() => new NctsDepartureMovementHeaderFormCustomisationSettingsProvider();
	}
}
