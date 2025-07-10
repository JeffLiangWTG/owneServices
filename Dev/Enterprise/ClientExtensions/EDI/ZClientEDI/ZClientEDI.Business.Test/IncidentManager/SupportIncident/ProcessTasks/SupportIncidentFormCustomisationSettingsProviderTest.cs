using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	[TestedType(typeof(SupportIncidentFormCustomisationSettingsProvider))]
	class SupportIncidentFormCustomisationSettingsProviderTest : FormCustomisationSettingsProviderTest<SupportIncidentFormCustomisationSettingsProvider>
	{
		public override void TestPropertiesThatAffectWorkflow()
		{
			var expected = new string[]
			{
				IncidentMainSchema.IM_OH_Client.Name,
				IncidentMainSchema.IM_Product.Name,
				IncidentMainSchema.IM_Category.Name,
				IncidentMainSchema.IM_Source.Name,
				IncidentMainSchema.IM_ProgramArea.Name,
				IncidentMainSchema.IM_Language.Name
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
		public override SupportIncidentFormCustomisationSettingsProvider GetNewProvider()
		{
			return new SupportIncidentFormCustomisationSettingsProvider();
		}
	}
}
