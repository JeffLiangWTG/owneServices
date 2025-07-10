using CargoWise.Data;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing
{
	public class AccountingPostingTest : TransactionedTestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper();
			testHelper.PostPeriodsForEntireYear(2004, GlbCompany.CurrentCompany.PK);
			testHelper.PostPeriodsForEntireYear(ZDateTime.Today.Year + 1, GlbCompany.CurrentCompany.PK);
			Db.Connection.BeginTransaction();
		}

		protected override void TearDown()
		{
			base.TearDown();
			Db.Connection.RollbackTransaction();
		}
	}
}
