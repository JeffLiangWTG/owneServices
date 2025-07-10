using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.GeneralLedger.GLBudget
{
	public class GLBudgetCollection : BusinessObjectCollection<GLBudget>
	{
		public GLBudgetCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public GLBudgetCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			ZQuery result = base.CreateAdditionalFilter();

			ZDBOnlyQuery budgetFilter = new ZDBOnlyQuery(typeof(GLBudget));
			ZDBOnlySubQuery branchFilter = new ZDBOnlySubQuery(typeof(GlbBranch), AccGLBudgetSchema.AU_GB);
			branchFilter.AddToFilter(GlbBranchSchema.GB_GC, GlbCompany.CurrentCompany.PK);
			budgetFilter.AddSubQuery(branchFilter, JoinCondition.And);
			result.AddToFilter(budgetFilter);

			return result;
		}

		#region Property Overrides

		#endregion

		protected override bool AllowNewCore
		{
			get { return false; }
		}
	}
}

