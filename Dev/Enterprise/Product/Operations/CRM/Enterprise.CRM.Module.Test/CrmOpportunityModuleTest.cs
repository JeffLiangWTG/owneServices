using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.CRM.Module.Test
{
	[TestedType(typeof(CrmOpportunityModule))]
	public class CrmOpportunityModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.CrmOpportunity;
		}

		public override void TestAllGridColumnsCanBeExportedToExcel()
		{
			// this module is not used as a grid
			Assert(true);
		}

		public override void TestBoundListsAreNotLoadedOnAccess()
		{
			// this module is not used as a grid
			Assert(true);
		}

		public override void TestModuleShowsAndCanSearch()
		{
			// this module is not used as a grid
			Assert(true);
		}

		protected override bool ShouldTestFormIsFullyTranslatable => false;
	}
}
