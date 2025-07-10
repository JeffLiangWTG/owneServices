using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Registry.Testing
{
	[TestedType(typeof(RegistryPimaAndBadgeHelper))]
	class RegistryPimaAndBadgeHelperTests : TestCaseWithFactory
	{
		public void TestGetPrimaryBranchPkFromRegistryBasedOnPima()
		{
			var company1 = Factory.New<GlbCompany>();
			company1.GC_RN_NKCountryCode = "GB";
			var company2 = Factory.New<GlbCompany>();
			company2.GC_RN_NKCountryCode = "AU";
			var branch1 = Factory.New<GlbBranch>();
			branch1.GB_RN_NKCountryCode = "GB";
			branch1.GB_Code = "L01";
			branch1.GB_GC = company1.PK;
			branch1.GB_RL_NKHomePort = "GBLO1";
			branch1.GB_IsActive = true;
			company1.Branches.Add(branch1);
			var branch2 = Factory.New<GlbBranch>();
			branch2.GB_RN_NKCountryCode = "GB";
			branch2.GB_Code = "L02";
			branch2.GB_GC = company1.PK;
			branch2.GB_RL_NKHomePort = "GBLO2";
			branch2.GB_IsActive = true;
			company1.Branches.Add(branch2);
			var branch3 = Factory.New<GlbBranch>();
			branch3.GB_RN_NKCountryCode = "AU";
			branch3.GB_Code = "L03";
			branch3.GB_GC = company2.PK;
			branch3.GB_RL_NKHomePort = "AULO3";
			branch3.GB_IsActive = true;
			Factory.Save();

			using (DisposableEnvironment.ForBranch(branch3.PK.ToGuid()))
			{
				ZString recipientPimaFromInterchange = new ZString("unregisteredPima");
				var result = RegistryPimaAndBadgeHelper.GetPrimaryBranchPkFromRegistryBasedOnPima(recipientPimaFromInterchange, Factory);
				AssertNotEquals(branch3.PK, result);
				Assert(result == branch1.PK || result == branch2.PK);
			}
		}
	}
}
