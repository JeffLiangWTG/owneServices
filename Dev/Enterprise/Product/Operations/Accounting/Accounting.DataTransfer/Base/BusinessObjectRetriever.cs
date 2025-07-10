using CargoWise.Common.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.DataTransfer
{
	[SuppressStaticMethodsAreLocatedOnCorrectClassMessage]
	public static class BusinessObjectRetriever
	{
		#region Branch

		public static ZQuery BranchFilter(string branchCode)
		{
			ZQuery filter = new ZQuery(GlbBranchSchema.GB_Code, branchCode);
			filter.AddToFilter(JoinCondition.And, GlbBranchSchema.GB_GC, SQLComparisonOperator.Equal, GlbCompany.CurrentCompany.PK);
			return filter;
		}

		public static GlbBranch GetBranchFromBranchCode(BusinessObjectFactory factory, string branchCode)
		{
			return factory.LoadTop1<GlbBranch>(BranchFilter(branchCode));
		}

		#endregion

		#region Department

		public static ZQuery DepartmentFilter(string departmentCode)
		{
			return new ZQuery(GlbDepartmentSchema.GE_Code, departmentCode);
		}

		public static GlbDepartment GetDepartmentFromDepartmentCode(BusinessObjectFactory factory, string departmentCode)
		{
			return factory.LoadTop1<GlbDepartment>(DepartmentFilter(departmentCode));
		}

		#endregion

		#region Bank

		public static ZQuery BankFilter(string bankCode)
		{
			ZQuery filter = new ZQuery(AccBankAccountSchema.AB_Code, bankCode);
			filter.AddToFilter(JoinCondition.And, AccBankAccountSchema.AB_GC, SQLComparisonOperator.Equal, GlbCompany.CurrentCompany.PK);
			return filter;
		}

		public static AccBankAccount GetBankFromBankCode(BusinessObjectFactory factory, string bankCode)
		{
			return factory.LoadTop1<AccBankAccount>(BankFilter(bankCode));
		}

		#endregion

		#region Cheque Book

		public static ZQuery ChequeBookFilter(string chequeBookCode)
		{
			ZQuery filter = new ZQuery(AccChequeBookSchema.AK_Code, chequeBookCode);
			filter.AddToFilter(AccChequeBookSchema.AK_GB, GlbCompany.CurrentCompany.Branches.GetPKs());
			return filter;
		}

		public static AccChequeBook GetChequeBookFromChequeBookCode(BusinessObjectFactory factory, string chequeBookCode)
		{
			return factory.LoadTop1<AccChequeBook>(ChequeBookFilter(chequeBookCode));
		}

		#endregion

		#region GL Account

		public static ZQuery GLHeaderFilter(string gLAccountNumber, bool restrictToPostableAccountsOnly)
		{
			ZQuery result = new ZQuery(AccGLHeaderSchema.AG_AccountNum, gLAccountNumber);

			if (restrictToPostableAccountsOnly)
			{
				ZQuery accountTypeFilter = new ZQuery(AccGLHeaderSchema.AG_AccountType, SQLComparisonOperator.Equal, Core.Constants.AccountType.ProfitAndLossAccount);
				accountTypeFilter.AddToFilter(JoinCondition.Or, AccGLHeaderSchema.AG_AccountType, SQLComparisonOperator.Equal, Core.Constants.AccountType.BalanceSheetAccount);
				result.AddToFilter(accountTypeFilter, JoinCondition.And);
				result.AddToFilter(JoinCondition.And, AccGLHeaderSchema.AG_DisallowDirectPosting, SQLComparisonOperator.Equal, ZBool.False);
			}

			return result;
		}

		public static AccGLHeader GetGLHeaderFromGLAccountNumber(BusinessObjectFactory factory, string gLAccountNumber, bool restrictToPostableAccountsOnly)
		{
			return factory.LoadTop1<AccGLHeader>(GLHeaderFilter(gLAccountNumber, restrictToPostableAccountsOnly));
		}

		#endregion

		#region GL Period

		public static int GetGLPeriodFromDate(BusinessObjectFactory factory, ZDateTime date)
		{
			return new AccountingPeriodCalculator(factory).GetPeriodFromDate(date, GlbCompany.CurrentCompany.PK);
		}

		#endregion

		#region OrgHeader

		public static OrgHeader GetOrgHeaderByCode(BusinessObjectFactory factory, string code)
		{
			ZQuery filter = new ZQuery(OrgHeaderSchema.OH_Code, code);
			filter.AddToFilter(OrgHeaderSchema.OH_IsActive, "Y");

			return factory.LoadTop1<OrgHeader>(filter);
		}

		#endregion

		#region AccGroups

		public static AccGroups GetAccGroupsByCode(BusinessObjectFactory factory, string code)
		{
			ZQuery filter = new ZQuery(AccGroupsSchema.AR_Code, code);
			return factory.LoadTop1<AccGroups>(filter);
		}

		#endregion

		#region GlbStaff

		public static GlbStaff GetGlbStaffByCode(BusinessObjectFactory factory, string code)
		{
			ZQuery filter = new ZQuery(GlbStaffSchema.GS_Code, code);
			filter.AddToFilter(GlbStaffSchema.GS_IsActive, "Y");

			return factory.LoadTop1<GlbStaff>(filter);
		}

		#endregion

		#region GlbGroup

		public static GlbGroup GetGlbGroupByCode(BusinessObjectFactory factory, string code)
		{
			ZQuery filter = new ZQuery(GlbGroupSchema.GG_Code, code);
			filter.AddToFilter(GlbGroupSchema.GG_IsActive, "Y");

			return factory.LoadTop1<GlbGroup>(filter);
		}

		#endregion
	}
}
