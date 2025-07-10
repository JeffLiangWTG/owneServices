//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccGLBudgetValidation
//
//    This class should be used for overriding validation in AutoAccGLBudgetValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.GeneralLedger.GLBudget
{
	public class AccGLBudgetValidation : AutoAccGLBudgetValidation
	{
		public AccGLBudgetValidation(AutoAccGLBudget parent)
			: base(parent)
		{
		}
	}

	public class GLBudgetValidation : AccGLBudgetValidation
	{
		public GLBudgetValidation(GLBudget parent)
			: base(parent)
		{
		}

		#region Property Overrides

		public void ValidateTotalPercentage()
		{
			ValidateCalculatedProperty(((GLBudget)Parent).TotalPercentageInfo);
		}

		protected virtual void CheckTotalPercentage()
		{
			ZDecimal percentage = ((GLBudget)Parent).TotalPercentage;
			if (percentage != 0m && percentage != 100m)
			{
				((GLBudget)Parent).TotalPercentageInfo.AddError(Res.GetString("3a5d0f51-85e7-4a26-b5ce-427d7c74c326", "Percentage total must be 100."));
			}
		}

		protected override void CheckAU_AllocationType()
		{
			base.CheckAU_AllocationType();

			if (!((GLBudget)Parent).AllocationTypes.ContainsCode(Parent.AU_AllocationType))
			{
				Parent.AU_AllocationTypeInfo.AddError(Res.GetString("42381691-d653-4237-91e1-3bd995fe5370", "Allocation type has the invalid value {0}", Parent.AU_AllocationType));
			}
		}

		protected override void CheckAU_Year()
		{
			base.CheckAU_Year();
			PeriodManager.FinancialYear = (ZShort)Parent.AU_Year;

			if (PeriodManager.Periods.Count <= 0)
			{
				Parent.AU_YearInfo.AddError(Res.GetString("3cc39c32-c0e5-441e-b490-db4589b7dbc3", "There is no period setup for the year {0}", Parent.AU_Year));
			}

			if (DoesBudgetExist())
			{
				Parent.AU_YearInfo.AddError(Res.GetString("61366184-5095-4f97-b4f7-d5f409b66b8a", "There is already another Budget defined for this Year, Account, Branch and Department."));
			}

			ValidateAU_GB();
			ValidateAU_GE();
			ValidateAU_AG();
		}

		protected override void CheckAU_AG()
		{
			base.CheckAU_AG();

			if (DoesBudgetExist())
			{
				Parent.AU_AGInfo.AddError(Res.GetString("61366184-5095-4f97-b4f7-d5f409b66b8a", "There is already another Budget defined for this Year, Account, Branch and Department."));
			}

			if (Parent.GLHeader != null && !Parent.GLHeader.AG_IsGlobal && !Parent.GLHeader.CompanyFilters.Cast<AccGLHeaderCompanyFilter>().Any(x => x.ACF_GC_Company == GlbCompany.CurrentCompany.PK))
			{
				Parent.AU_AGInfo.AddError(Res.GetString("a29b72fa-7e2a-4440-b797-c13ca917ff44", "This GL Account cannot be used."));
			}

			ValidateAU_GB();
			ValidateAU_GE();
			ValidateAU_Year();
		}

		protected override void CheckAU_GB()
		{
			base.CheckAU_AG();

			if (DoesBudgetExist())
			{
				Parent.AU_GBInfo.AddError(Res.GetString("61366184-5095-4f97-b4f7-d5f409b66b8a", "There is already another Budget defined for this Year, Account, Branch and Department."));
			}
			if (Parent.Branch != null && GlbCompany.CurrentCompany.PK != Parent.Branch.GB_GC)
			{
				Parent.AU_GBInfo.AddError(Res.GetString("0ae42783-05d9-4b63-be9b-e6a7b3b72184", "The selected branch is not valid. Please choose a new Branch from the list."));
			}
			ValidateAU_AG();
			ValidateAU_GE();
			ValidateAU_Year();
		}

		protected override void CheckAU_GE()
		{
			base.CheckAU_AG();

			if (DoesBudgetExist())
			{
				Parent.AU_GEInfo.AddError(Res.GetString("61366184-5095-4f97-b4f7-d5f409b66b8a", "There is already another Budget defined for this Year, Account, Branch and Department."));
			}
			ValidateAU_GB();
			ValidateAU_AG();
			ValidateAU_Year();

			GlbBranchCombinationValidation.CheckBranchDepartmentCombination(Parent.AU_GEInfo, Parent.Branch, Parent.Department);
		}

		#region Implementation

		bool DoesBudgetExist()
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(GLBudget));
			query.AddToFilter(AccGLBudgetSchema.AU_Year, Parent.AU_Year);
			query.AddToFilter(AccGLBudgetSchema.AU_AG, Parent.AU_AG);
			query.AddToFilter(AccGLBudgetSchema.AU_GB, Parent.AU_GB);
			query.AddToFilter(AccGLBudgetSchema.AU_GE, Parent.AU_GE);
			query.AddToFilter(AccGLBudgetSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);

			GLBudget result = Parent.Factory.LoadTop1<GLBudget>(query);

			return result != null;
		}

		PeriodManagement.PeriodManager fPeriodManager;
		PeriodManagement.PeriodManager PeriodManager
		{
			get
			{
				if (fPeriodManager == null)
				{
					fPeriodManager = new PeriodManagement.PeriodManager(Parent.Factory);
				}
				return fPeriodManager;
			}
		}

		AccountingPeriodCalculator fPeriodCalculator;
		protected AccountingPeriodCalculator PeriodCalculator
		{
			get
			{
				if (fPeriodCalculator == null)
				{
					fPeriodCalculator = new AccountingPeriodCalculator(Parent.Factory);
				}

				return fPeriodCalculator;
			}
		}

		#endregion
		#endregion
	}
}
