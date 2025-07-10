using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Business.Declaration.Testing
{
	class BadgeCodeGetterTest : TestCaseWithFactory
	{
		[ExpectNoExceptions()]
		public void TestInstanceCachedForWithNullBranch()
		{
			var comp = Factory.NewWithValidTestData<GlbCompany>();
			comp.GC_RN_NKCountryCode = "GB";
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_BranchName = "Another Branch";
			comp.Branches.Add(branch);
			Factory.Save();

			var curBranchPK = GlbBranch.CurrentBranch.PK;

			AssertNotEquals(branch.PK, curBranchPK);

			JobDeclaration declaration;
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry("GB"))
			{
				declaration = Factory.New<JobDeclaration>();

				AssertNotEquals(declaration.JE_GB, curBranchPK);

				declaration.JE_GB = ZGuid.Empty;
			}

			var badgeCodeGetter = BadgeCodeGetter.InstanceCachedFor(declaration);
			var result = badgeCodeGetter.GetBadgeList("IMP", "GBLON");

			AssertNotNull(result);
		}
	}
}
