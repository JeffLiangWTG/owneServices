//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccGLBudgetLookups
//
//    This class should be used for overriding collections in AutoAccGLBudgetLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.GeneralLedger.GLBudget
{
	public partial class AccGLBudgetLookups : AutoAccGLBudgetLookups
	{
		public AccGLBudgetLookups(AutoAccGLBudget parent)
			: base(parent)
		{
		}

		public override AccGLHeaderCollection GLHeaders
		{
			get
			{
				var query = BalanceSheetAndPLFilter;
				query.AddToFilter(GlobalGLAccountFilter);

				return new AccGLHeaderCollection(Factory, query);
			}
		}

		ZQuery BalanceSheetAndPLFilter
		{
			get
			{
				ZQuery query = new ZQuery(AccGLHeaderSchema.AG_AccountType, new ZString(AccountTypesList.Codes.BalanceSheet));
				query.AddToFilter(JoinCondition.Or, AccGLHeaderSchema.AG_AccountType, SQLComparisonOperator.Equal, new ZString(AccountTypesList.Codes.ProfitLoss));
				return query;
			}
		}

		ZQuery GlobalGLAccountFilter
		{
			get
			{
				ZDBOnlyQuery companyFilterQuery = new ZDBOnlyQuery(typeof(AccGLHeader));
				ZDBOnlySubQuery companyFilterSubQuery = new ZDBOnlySubQuery(typeof(AccGLHeaderCompanyFilter), AccGLHeaderCompanyFilterSchema.ACF_AG_Header);
				companyFilterSubQuery.AddToFilter(AccGLHeaderCompanyFilterSchema.ACF_GC_Company, GlbCompany.CurrentCompany.PK);
				companyFilterQuery.AddSubQuery(companyFilterSubQuery, JoinCondition.And);
				companyFilterQuery.AddToFilter(JoinCondition.Or, AccGLHeaderSchema.AG_IsGlobal, true);

				return companyFilterQuery;
			}
		}
	}
}


