using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.PayableOrder.Testing
{
	using CargoWise.EntityFramework.Testing;

	internal class AccPayableOrderLineLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestBranches()
		{
			GlbBranch currentCompanybranch = Factory.NewWithValidTestData<GlbBranch>();
			currentCompanybranch.GB_GC = GlbCompany.CurrentCompany.PK;
			GlbBranch currentCompanyInActivebranch = Factory.NewWithValidTestData<GlbBranch>();
			currentCompanyInActivebranch.GB_GC = GlbCompany.CurrentCompany.PK;
			currentCompanyInActivebranch.GB_IsActive = false;
			GlbCompany nonCurrentCompany = Factory.NewWithValidTestData<GlbCompany>();
			GlbBranch nonCurrentCompanyBranch = nonCurrentCompany.Branches.AddNew();
			Factory.Save();
			AccTransactionLines transactionLines = Factory.New<AccTransactionLines>();
			transactionLines.Lookups.Branches.Load();
			Assert("Should contain current company branches", transactionLines.Lookups.Branches.Contains(currentCompanybranch));
			Assert("Should not contain inactive branches", !transactionLines.Lookups.Branches.Contains(currentCompanyInActivebranch));
			Assert("Should not contain current company branches", !transactionLines.Lookups.Branches.Contains(nonCurrentCompanyBranch));
		}
	}
}