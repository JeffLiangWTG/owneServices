using CargoWise.EntityFramework.Testing;

namespace Enterprise.Accounting.DataTransfer.GLJournals.Testing
{
	public abstract class GLJournalEnvironmentForADAWTest : NonPersistentBusinessObjectTestCase
	{
		public void TestEnvironmentValues()
		{
			var environment = GetNewBusinessObject() as GLJournalEnvironmentForADAW;

			environment.CompanyCode = "XXX";
			environment.BranchCode = "ABC";
			AssertNull(environment.Company);
			AssertNull(environment.Branch);

			environment.CompanyCode = "DEM";
			environment.BranchCode = "SYD";
			environment.Sequence = 1;
			AssertNotNull(environment.Company);
			AssertNotNull(environment.Branch);
			AssertEquals(1, environment.Sequence);
		}

		public virtual void TestProperties()
		{
			var environment = GetNewBusinessObject() as GLJournalEnvironmentForADAW;

			CombineAssertions(() =>
			{
				AssertNotNull("CompanyCode Property", environment.FindPropertyInfo(nameof(environment.CompanyCode)));
				AssertNotNull("BranchCode Property", environment.FindPropertyInfo(nameof(environment.BranchCode)));
			});
		}

		public void TestValidation()
		{
			var environment = GetNewBusinessObject() as GLJournalEnvironmentForADAW;

			environment.CompanyCode = "XXX";
			AssertEquals("The company code XXX is invalid or inactive.", environment.ValidateCompany());

			environment.CompanyCode = "DEM";
			AssertEquals("", environment.ValidateCompany());

			environment.BranchCode = "ABC";
			AssertEquals("The branch code ABC is invalid or inactive.", environment.ValidateBranch());

			environment.BranchCode = "SYD";
			AssertEquals("", environment.ValidateBranch());

			AssertEquals("The branch code SYD is not valid in company DEM.", environment.ValidateCompanyMatchBranch());
		}
	}
}
