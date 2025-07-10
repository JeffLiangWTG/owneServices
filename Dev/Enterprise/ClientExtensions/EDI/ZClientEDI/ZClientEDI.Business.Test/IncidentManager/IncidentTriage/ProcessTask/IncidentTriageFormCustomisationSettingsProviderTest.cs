using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	[TestedType(typeof(IncidentTriageFormCustomisationSettingsProvider))]
	public class IncidentTriageFormCustomisationSettingsProviderTest : FormCustomisationSettingsProviderTest<IncidentTriageFormCustomisationSettingsProvider>
	{
		public override IncidentTriageFormCustomisationSettingsProvider GetNewProvider()
		{
			return new IncidentTriageFormCustomisationSettingsProvider();
		}

		public override void TestDisplayTabs()
		{
			var provider = GetNewProvider();
			AssertEquals(0, provider.DisplayTabs.Count);
			AssertEquals(null, provider.DisplayTabs.Settings);
		}

		public override void TestPropertiesThatAffectWorkflow()
		{
			var expected = new string[]
			{
				IncidentTriageSchema.IMT_Product.Name,
				IncidentTriageSchema.IMT_ProductArea.Name,
				IncidentTriageSchema.IMT_Module.Name,
			};
			var provider = GetNewProvider();
			AssertContainsExactElementsInAnyOrder(expected, provider.PropertiesThatAffectWorkflow);
		}

		public override void TestTabPlacementProhibitions()
		{
			var provider = GetNewProvider();
			AssertEquals(0, provider.TabPlacementProhibitions.Length);
		}
	}
}
