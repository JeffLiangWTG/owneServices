using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.GeneralLedger.GLBudget.Testing
{
	public class AccGLBudgetValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckAU_AllocationType()
		{
			GLBudget testBudget = Factory.New<GLBudget>();
			testBudget.AU_AllocationType = "XXX";
			testBudget.Validation.ValidateAU_AllocationType();

			AssertHasError(testBudget.AU_AllocationTypeInfo, "Allocation type has the invalid value XXX");
		}

		public void TestCheckBudgetYear()
		{
			GLBudget testBudget = Factory.New<GLBudget>();
			testBudget.AU_Year = 1004;

			testBudget.Validation.ValidateAU_Year();

			AssertHasError(testBudget.AU_YearInfo, "There is no period setup for the year 1004");
		}

		public void TestCheckAU_AG()
		{
			AccGLHeader gLHeader = TestObjectCreator.CreateGLHeader();

			GLBudget newBudget = Factory.NewWithValidTestData<GLBudget>();
			newBudget.AU_AG = gLHeader.PK;
			newBudget.AU_Year = 2006;

			AssertNoError(newBudget.AU_AGInfo, "There is already another Budget defined for this Year, Account, Branch and Department.");

			Factory.Save();

			GLBudget newBudget2 = Factory.NewWithValidTestData<GLBudget>();
			newBudget2.AU_GB = newBudget.AU_GB;
			newBudget2.AU_GE = newBudget.AU_GE;
			newBudget2.AU_Year = 2006;
			newBudget2.AU_AG = gLHeader.PK;

			AssertHasError(newBudget2.AU_AGInfo, "There is already another Budget defined for this Year, Account, Branch and Department.");
		}

		public void TestCheckAU_AGBasedOnCompanyFilter()
		{
			var creator = new TestObjectCreator(Factory);
			var glHeader1 = creator.CreateGLHeader();
			glHeader1.AG_IsGlobal = true;
			var glHeader2 = creator.CreateGLHeader();
			glHeader2.AG_IsGlobal = false;
			var filterForGLHeader2 = glHeader2.CompanyFilters.AddNew();
			filterForGLHeader2.ACF_GC_Company = GlbCompany.CurrentCompany.PK;
			var glHeader3 = creator.CreateGLHeader();
			glHeader3.AG_IsGlobal = false;
			var filterForGLHeader3 = glHeader3.CompanyFilters.AddNew();
			filterForGLHeader3.ACF_GC_Company = creator.NonCurrentCompany.PK;

			GLBudget budget = Factory.NewWithValidTestData<GLBudget>();

			budget.AU_AG = glHeader1.PK;
			AssertNoErrors(budget.AU_AGInfo);

			budget.AU_AG = glHeader2.PK;
			AssertNoErrors(budget.AU_AGInfo);

			budget.AU_AG = glHeader3.PK;
			AssertHasError(budget.AU_AGInfo, "This GL Account cannot be used.");
		}

		public void TestCheckAU_GB()
		{
			AccGLHeader gLHeader = TestObjectCreator.CreateGLHeader();

			GLBudget newBudget = Factory.NewWithValidTestData<GLBudget>();
			newBudget.AU_AG = gLHeader.PK;
			newBudget.AU_Year = 2006;

			AssertNoError(newBudget.AU_GBInfo, "There is already another Budget defined for this Year, Account, Branch and Department.");

			Factory.Save();

			GLBudget newBudget2 = Factory.NewWithValidTestData<GLBudget>();
			newBudget2.AU_GB = newBudget.AU_GB;
			newBudget2.AU_GE = newBudget.AU_GE;
			newBudget2.AU_Year = 2006;
			newBudget2.AU_AG = gLHeader.PK;

			AssertHasError(newBudget2.AU_GBInfo, "There is already another Budget defined for this Year, Account, Branch and Department.");
		}

		public void TestCheckAU_GB_InvalidBranch()
		{
			GLBudget newBudget = Factory.NewWithValidTestData<GLBudget>();
			ZQuery branchQuery = new ZQuery(GlbBranchSchema.GB_GC, SQLComparisonOperator.Equal, GlbCompany.CurrentCompany.PK);
			branchQuery.AddToFilter(GlbBranchSchema.PK, SQLComparisonOperator.NotEqual, GlbBranch.CurrentBranch.PK);
			newBudget.AU_GB = Factory.LoadTop1<GlbBranch>(branchQuery).PK;
			newBudget.AU_AG = TestObjectCreator.CreateGLHeader().PK;
			newBudget.AU_Year = 2006;

			AssertNoError("Prerequisite: should be no error", newBudget.AU_GBInfo, "The selected branch is not valid. Please choose a new Branch from the list.");
			newBudget.AU_GB = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_GC, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK)).PK;
			AssertHasError("Prerequisite: should be no error", newBudget.AU_GBInfo, "The selected branch is not valid. Please choose a new Branch from the list.");
		}

		public void TestLastYear()
		{
			ZInt lastYear = ZDateTime.Now.Year - 1;

			ZDateTime now = ZDateTime.Now;
			ZDateTime oneDayBeforeNow = now.AddDays(-1);
			ZDateTime oneDayAfterNow = now.AddDays(1);

			ZDateTime lastYearStartDate = oneDayBeforeNow.AddYears(-1);
			ZDateTime lastYearEndDate = oneDayAfterNow.AddYears(-1);

			AccPeriodManagement lastYearPeriod = Factory.New<AccPeriodManagement>();
			lastYearPeriod.AM_GC_Company = GlbCompany.CurrentCompany.PK;
			lastYearPeriod.AM_StartDate = lastYearStartDate;
			lastYearPeriod.AM_EndDate = lastYearEndDate;
			lastYearPeriod.AM_Year = (ZShort)lastYear;
			lastYearPeriod.AM_Period = lastYear * 100 + 1;

			AccPeriodManagement thisYearPeriod = Factory.New<AccPeriodManagement>();
			thisYearPeriod.AM_GC_Company = GlbCompany.CurrentCompany.PK;
			thisYearPeriod.AM_StartDate = ZDateTime.Now.AddDays(-1);
			thisYearPeriod.AM_EndDate = ZDateTime.Now.AddDays(1);
			thisYearPeriod.AM_Year = (ZShort)ZDateTime.Now.Year;
			thisYearPeriod.AM_Period = (ZDateTime.Now.Year * 100) + 1;

			Factory.Save();

			GLBudget newBudget = Factory.NewWithValidTestData<GLBudget>();
			newBudget.AU_Year = lastYear;
			AssertEquals("No Errors Are Expected, You Should be able to enter a previous year's budget", false, newBudget.AU_YearInfo.HasErrors());

			newBudget.AU_Year = ZDateTime.Now.Year;
			AssertEquals("No Errors Are Expected, You should be able to enter the current year budget", false, newBudget.AU_YearInfo.HasErrors());
		}

		public void TestValidateTotalPercentage()
		{
			GLBudget newBudget = Factory.NewWithValidTestData<GLBudget>();

			newBudget.BudgetLines.AddNew();
			newBudget.BudgetLines.AddNew();
			newBudget.BudgetLines.AddNew();

			newBudget.AU_AllocationType = AllocationTypeList.Codes.percentage;

			newBudget.BudgetLines[0].AD_Percent = 30;
			newBudget.BudgetLines[1].AD_Percent = 30;
			newBudget.BudgetLines[2].AD_Percent = 30;

			AssertHasError(newBudget.TotalPercentageInfo, "Percentage total must be 100.");

			newBudget.BudgetLines[0].AD_Percent = 40;
			AssertNoError(newBudget.TotalPercentageInfo, "Percentage total must be 100.");

			newBudget.AU_AllocationType = AllocationTypeList.Codes.byPeriod;
			AssertNoError(newBudget.TotalPercentageInfo, "Percentage total must be 100.");
		}

		public void TestBranchDepartmentCombinationValidation_AccGLBudgetValidation()
		{
			var bizObj = Factory.NewWithValidTestData<GLBudget>();

			GlbBranchCombinationValidationTest.ValidateBranchDepartmentCombinationsForBizObj(Factory,
				(branch, department) => { bizObj.AU_GB = branch; bizObj.AU_GE = department; }, bizObj.AU_GEInfo);
		}

		TestObjectCreator fTestObjectCreator;
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
	}
}