using CargoWise.Types;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MarketingManager.GUI.Testing
{
	[TestedType(typeof(OrganisationModuleFilter))]
	class OrganisationModuleFilterFilterTest : ModuleFilterTestCase<OrganisationModuleFilter>
	{
		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, Filter.IsExpensiveQuery);
		}

		protected override OrganisationModuleFilter GetNewModuleFilter()
		{
			return new OrganisationModuleFilter(ModuleIDs.Organisation, ViewCampaignContactSchema.PK, ViewCampaignContactSchema.VCC_OH, Factory, typeof(CampaignContact));
		}

		protected override FilterCategory ExpectedDefaultCategory => FilterCategories.Organisations;
		protected override ZString ExpectedDescription
		{
			get
			{
				return "Organization (Multiple)";
			}
		}

		protected override FilterCategory InitialTestCatergory
		{
			get
			{
				return FilterCategories.GetOrCreateFilterCategory((NoResString)"License");
			}
		}
	}
}
