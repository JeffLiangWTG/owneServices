using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.CRM.Common.Testing
{
	[TestedType(typeof(CrmOpportunityFormCustomisationSettingProvider))]
	sealed class CrmOpportunityFormCustomisationSettingProviderTest : FormCustomisationSettingsProviderTest<CrmOpportunityFormCustomisationSettingProvider>
	{
		public override void TestPropertiesThatAffectWorkflow()
		{
			var expected = new string[]
			{
				CrmOpportunitySchema.COP_SalesType.Name,
				CrmOpportunitySchema.COP_SourceType.Name,
				CrmOpportunitySchema.COP_ProductType.Name
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

		public override CrmOpportunityFormCustomisationSettingProvider GetNewProvider()
		{
			return new CrmOpportunityFormCustomisationSettingProvider();
		}
	}
}
