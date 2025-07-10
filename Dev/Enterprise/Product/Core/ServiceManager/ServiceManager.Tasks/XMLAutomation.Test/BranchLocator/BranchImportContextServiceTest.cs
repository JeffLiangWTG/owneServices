using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation.Testing
{
	sealed class BranchImportContextServiceTest : TestCaseWithFactory
	{
		public void TestSet()
		{
			AssertEquals("prerequisite", "SYD", GlbBranch.CurrentBranch.GB_Code);

			BranchImportContextService.GetInstance(Factory).Set(branchGDN);
			AssertEquals("GDN", GlbBranch.CurrentBranch.GB_Code);

			Factory.Save();
			AssertEquals("SYD", GlbBranch.CurrentBranch.GB_Code);

			BranchImportContextService.GetInstance(Factory).Set(branchHGK);
			AssertEquals("HGK", GlbBranch.CurrentBranch.GB_Code);

			Factory.Save();
			AssertEquals("SYD", GlbBranch.CurrentBranch.GB_Code);
		}

		public void TestIsInBranchContext()
		{
			AssertEquals("prerequisite", "SYD", GlbBranch.CurrentBranch.GB_Code);
			AssertEquals(false, BranchImportContextService.GetInstance(Factory).IsInBranchContext);

			BranchImportContextService.GetInstance(Factory).Set(branchSYD);
			AssertEquals(true, BranchImportContextService.GetInstance(Factory).IsInBranchContext);

			BranchImportContextService.GetInstance(Factory).Set(branchGDN);
			AssertEquals(true, BranchImportContextService.GetInstance(Factory).IsInBranchContext);

			Factory.Save();
			AssertEquals(false, BranchImportContextService.GetInstance(Factory).IsInBranchContext);
		}

		public void TestAllowSetBranchFirstTimeOnly()
		{
			AssertEquals("prerequisite", "SYD", GlbBranch.CurrentBranch.GB_Code);

			BranchImportContextService.GetInstance(Factory).Set(branchGDN);
			AssertEquals("GDN", GlbBranch.CurrentBranch.GB_Code);

			BranchImportContextService.GetInstance(Factory).Set(branchHGK);
			AssertEquals("GDN", GlbBranch.CurrentBranch.GB_Code);

			Factory.Save();
			AssertEquals("SYD", GlbBranch.CurrentBranch.GB_Code);
		}

		GlbBranch branchSYD;
		GlbBranch branchGDN;
		GlbBranch branchHGK;

		protected override void SetUp()
		{
			base.SetUp();

			GlbCompany company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);

			XMLAutomationTestHelper.DeleteAllBranchesExceptCurrentBranch(company);

			branchSYD = company.Branches[0];
			branchSYD.GB_RL_NKHomePort = "AUSYD";
			branchSYD.GB_Code = "SYD";

			branchGDN = company.Branches.AddNew();
			branchGDN.GB_RL_NKHomePort = "PLGDN";
			branchGDN.GB_Code = "GDN";

			branchHGK = company.Branches.AddNew();
			branchHGK.GB_RL_NKHomePort = "HKHGK";
			branchHGK.GB_Code = "HGK";

			Factory.Save();
		}
	}
}
