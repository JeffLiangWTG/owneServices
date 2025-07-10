using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	public class JCSUserContextProviderTest : TestCaseWithFactory
	{
		public void TestSetContext()
		{
			var companyPK = Env.CurrentCompanyPK;
			var branchPK = Env.CurrentBranchPK;

			var objectCreator = new TestObjectCreator(Factory);

			var glbCompany1 = objectCreator.CreateCompanyAndBranch("USLAX");
			glbCompany1.Factory.Save();

			var glbCompany2 = objectCreator.CreateCompanyAndBranch("AUSYD");
			glbCompany2.Factory.Save();

			using (var contextProvider = new JCSUserContextProvider())
			{
				contextProvider.SetUserContext(glbCompany1.PK, glbCompany1.Branches[0].PK);
				AssertCurrentCompanyAndBranch(contextProvider, glbCompany1.PK, glbCompany1.Branches[0].PK);

				contextProvider.SetUserContext(glbCompany2.PK, glbCompany2.Branches[0].PK);
				AssertCurrentCompanyAndBranch(contextProvider, glbCompany2.PK, glbCompany2.Branches[0].PK);

				contextProvider.SetUserContext(glbCompany1.PK, glbCompany1.Branches[0].PK);
				AssertCurrentCompanyAndBranch(contextProvider, glbCompany1.PK, glbCompany1.Branches[0].PK);

				contextProvider.SetUserContext(glbCompany1.PK, glbCompany1.Branches[0].PK);
				AssertCurrentCompanyAndBranch(contextProvider, glbCompany1.PK, glbCompany1.Branches[0].PK);
			}

			AssertEquals("Original Company PK", companyPK, Env.CurrentCompanyPK);
			AssertEquals("Original Branch PK", branchPK, Env.CurrentBranchPK);
		}

		void AssertCurrentCompanyAndBranch(JCSUserContextProvider contextProvider, ZGuid expectedCompany, ZGuid expectedBranch)
		{
			AssertEquals("Company PK 1", expectedCompany, contextProvider.CompanyPK);
			AssertEquals("Branch PK", expectedBranch, contextProvider.BranchPK);
			AssertEquals("Original Company PK", expectedCompany, Env.CurrentCompanyPK);
			AssertEquals("Original Branch PK", expectedBranch, Env.CurrentBranchPK);
		}
	}
}
