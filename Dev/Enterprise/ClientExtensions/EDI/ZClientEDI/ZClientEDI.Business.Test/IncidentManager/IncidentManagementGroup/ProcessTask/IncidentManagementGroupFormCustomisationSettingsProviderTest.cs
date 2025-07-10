using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	[TestedType(typeof(IncidentManagementGroupFormCustomisationSettingsProvider))]
	public class IncidentManagementGroupFormCustomisationSettingsProviderTest : FormCustomisationSettingsProviderTest<IncidentManagementGroupFormCustomisationSettingsProvider>
	{
		public override void TestPropertiesThatAffectWorkflow()
		{
			var expected = new string[]
			{
				IncidentManagementGroupSchema.ING_Type.Name,
				IncidentManagementGroupSchema.ING_Product.Name,
				IncidentManagementGroupSchema.ING_ProductArea.Name,
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

		public override IncidentManagementGroupFormCustomisationSettingsProvider GetNewProvider()
		{
			return new IncidentManagementGroupFormCustomisationSettingsProvider();
		}
	}
}
