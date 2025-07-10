using CargoWise.EntityFramework;
using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.ExitControl.Module.Testing
{
	[TestedType(typeof(ExitControlModule))]
	sealed class ExitControlModuleTest : ZModuleBasherTest
	{
		public void TestGetNewBusinessObjectCollectionMatchesFilter()
		{
			var company1 = Factory.New<GlbCompany>();
			company1.GC_Code = "TS1";
			company1.GC_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			company1.GC_Name = "Test Company 1";

			var branch1 = company1.Branches.AddNew();
			branch1.GB_Code = "BR1";
			branch1.GB_RL_NKHomePort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			branch1.GB_BranchName = "Test Branch 1";

			var header1 = Factory.New<CusExitHeader>();
			header1.CXH_JobReference = header1.PK.ToString().Substring(0, 35);
			header1.CXH_GC_Company = company1.PK;
			header1.CXH_GB_Branch = branch1.PK;

			var header2 = Factory.New<CusExitHeader>();
			header2.CXH_JobReference = header2.PK.ToString().Substring(0, 35);

			var header3 = Factory.New<CusExitHeader>();
			header3.CXH_ApplicationCode = "DAC";

			ZQuery filter;
			using (var module = GetModule())
			{
				filter = module.GetNewBusinessObjectCollection().CompleteFilter;
			}

			CombineAssertions(() =>
			{
				AssertEquals("header1", expected: false, header1.MatchesFilter(filter));
				AssertEquals("header2", expected: true, header2.MatchesFilter(filter));
				AssertEquals("header3", expected: false, header3.MatchesFilter(filter));
			});
		}

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.EU.ExitControl;
	}
}
