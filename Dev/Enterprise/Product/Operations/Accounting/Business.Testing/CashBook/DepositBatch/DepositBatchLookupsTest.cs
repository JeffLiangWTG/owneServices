using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.CashBook.DepositBatch.Testing
{
	class DepositBatchLookupsTest : TestCaseWithFactory
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
		public void TestBranches()
		{
			var testObjectCreator = new TestObjectCreator(Factory);

			DepositBatch testDepositBatch = Factory.New<DepositBatch>();
			DepositBatchLookups testLookup = new DepositBatchLookups(testDepositBatch);
			GlbBranchCollection branches = testLookup.Branches;
			branches.Load();
			int countBeforeAddNew = branches.Count;
			var currentBranchPk = GlbBranch.CurrentBranch.PK;

			Env.SetUserContext(new UserContext(Env.CurrentUser.LoginName, testObjectCreator.NonCurrentCompanyBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()));
			Env.SetUserContext(new UserContext(Env.CurrentUser.LoginName, currentBranchPk.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()));

			GlbBranch branch1 = Factory.NewWithValidTestData<GlbBranch>();
			branch1.GB_IsActive = false;
			branch1.GB_GC = GlbCompany.CurrentCompany.PK;

			GlbBranch branch2 = Factory.NewWithValidTestData<GlbBranch>();
			branch2.GB_GC = ZGuid.NewZGuid();
			branch2.GB_GC = GlbCompany.CurrentCompany.PK;

			Factory.Save();
			branches = testLookup.Branches;
			branches.Load();
			AssertEquals(countBeforeAddNew + 1, branches.Count);
		}
	}
}
