using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.CashBook.DirectPayment;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.CashBook.Testing
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
	class DirectTransactionLineBaseLookupsTest : TestCaseWithFactory
	{
		public void TestGLHeaders()
		{
			var glHeaders = TestBizO.GLHeaders;
			glHeaders.Load();
			var countBeforeAddNew = glHeaders.Count;

			Env.SetUserContext(new UserContext(Env.CurrentUser.LoginName, TestObjectCreator.NonCurrentCompanyBranch.PK.ToGuid(), TestObjectCreator.NonCurrentDepartment.PK.ToGuid()));

			var invalidGLHeader1 = Factory.NewWithValidTestData<AccGLHeader>();
			invalidGLHeader1.AG_AccountNum = "1234567890";
			invalidGLHeader1.AG_AccountType = AccountTypeComboBoxConstants.ProfitAndLossAccount;
			invalidGLHeader1.AG_DisallowDirectPosting = true;

			var invalidGLHeader2 = Factory.NewWithValidTestData<AccGLHeader>();
			invalidGLHeader2.AG_AccountNum = "1234567891";
			invalidGLHeader2.AG_AccountType = AccountTypeComboBoxConstants.Header;

			var validGLHeader = Factory.NewWithValidTestData<AccGLHeader>();
			validGLHeader.AG_AccountNum = "1234567892";
			validGLHeader.AG_AccountType = AccountTypeComboBoxConstants.ProfitAndLossAccount;

			Factory.Save();

			glHeaders.Load();
			AssertEquals(countBeforeAddNew + 1, glHeaders.Count);
		}

		public void TestShowGLAccountsForImportAction()
		{
			AssertNull(TestBizO.GLHeaders.ShowGLAccountsForImportAction);
			TestLine.ShowGLAccountsForImportAction = (glHeaderCollection, glHeaderList) => { glHeaderList.Add(Factory.New<AccGLHeader>()); };
			AssertNotNull(TestBizO.GLHeaders.ShowGLAccountsForImportAction);
			AssertEquals(TestLine.ShowGLAccountsForImportAction, TestBizO.GLHeaders.ShowGLAccountsForImportAction);
		}

		public void TestBranches()
		{
			TestBizO.Branches.Load();
			int countBeforeAddNew = TestBizO.Branches.Count;
			var currentBranchPk = GlbBranch.CurrentBranch.PK;

			Env.SetUserContext(new UserContext(Env.CurrentUser.LoginName, TestObjectCreator.NonCurrentCompanyBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()));
			Env.SetUserContext(new UserContext(Env.CurrentUser.LoginName, currentBranchPk.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()));

			GlbBranch branch1 = Factory.NewWithValidTestData<GlbBranch>();
			branch1.GB_IsActive = false;
			branch1.GB_GC = GlbCompany.CurrentCompany.PK;

			GlbBranch branch2 = Factory.NewWithValidTestData<GlbBranch>();
			branch2.GB_GC = ZGuid.NewZGuid();
			branch2.GB_GC = GlbCompany.CurrentCompany.PK;

			Factory.Save();

			TestBizO.Branches.Load();
			AssertEquals(countBeforeAddNew + 1, TestBizO.Branches.Count);
		}

		public void TestDepartments()
		{
			int countBeforeAddNew = TestBizO.Departments.Count;

			Env.SetUserContext(new UserContext(Env.CurrentUser.LoginName, TestObjectCreator.NonCurrentCompanyBranch.PK.ToGuid(), TestObjectCreator.NonCurrentDepartment.PK.ToGuid()));

			GlbDepartment department1 = Factory.NewWithValidTestData<GlbDepartment>();
			department1.GE_IsActive = false;

			GlbDepartment department2 = Factory.NewWithValidTestData<GlbDepartment>();

			Factory.Save();

			AssertEquals(countBeforeAddNew + 1, TestBizO.Departments.Count);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			TestLine = Factory.NewWithValidTestData<DirectPaymentLine>();
			TestBizO = new DirectTransactionLineBaseLookups(TestLine);
		}

		TestObjectCreator TestObjectCreator
		{
			get
			{
				if (fTestObjectCreator == null)
				{
					fTestObjectCreator = new TestObjectCreator(Factory);
				}
				return fTestObjectCreator;
			}
		}

		TestObjectCreator fTestObjectCreator;
		DirectTransactionLineBaseLookups TestBizO;
		DirectPaymentLine TestLine;

		#endregion
	}
}
