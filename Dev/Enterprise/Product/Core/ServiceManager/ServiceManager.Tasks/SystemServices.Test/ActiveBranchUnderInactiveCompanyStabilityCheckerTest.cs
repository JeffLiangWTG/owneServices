using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.StabilityChecker;

namespace Enterprise.ServiceManager.Tasks.SystemServices.Testing
{
	internal class ActiveBranchUnderInactiveCompanyStabilityCheckerTest : TestCaseWithFactory
	{
		public void TestResults()
		{
			var checker = new ActiveBranchUnderInactiveCompanyStabilityChecker();
			var results = checker.Check();
			AssertEquals(0, results.Length);

			var company1 = Factory.New<GlbCompany>();
			company1.GC_RN_NKCountryCode = "AU";
			company1.GC_Code = "GC1";
			company1.GC_IsActive = false;

			var branch1 = Factory.New<GlbBranch>();
			branch1.GB_Code = "GB1";
			branch1.GB_GC = company1.PK;
			branch1.GB_IsActive = true;

			var branch2 = Factory.New<GlbBranch>();
			branch2.GB_Code = "GB2";
			branch2.GB_GC = company1.PK;
			branch2.GB_IsActive = false;

			var company2 = Factory.New<GlbCompany>();
			company2.GC_RN_NKCountryCode = "US";
			company2.GC_Code = "GC2";
			company2.GC_IsActive = true;

			var branch3 = Factory.New<GlbBranch>();
			branch3.GB_Code = "GB3";
			branch3.GB_GC = company2.PK;
			branch3.GB_IsActive = true;

			var branch4 = Factory.New<GlbBranch>();
			branch4.GB_Code = "GB4";
			branch4.GB_GC = company2.PK;
			branch4.GB_IsActive = false;

			Factory.Save();

			results = checker.Check();
			AssertEquals(1, results.Length);
			AssertEquals("StabilityLevel", StabilityResultLevel.Warning, results[0].StabilityLevel);
			AssertEquals("Description", "Branch (GB1) is attached to an inactive Company (GC1) record. Please activate Company (GC1) record or change to a valid branch code that is attached to an active company record.", results[0].Description);
		}
	}
}
