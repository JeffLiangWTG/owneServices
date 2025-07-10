using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.GenericCharge;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business
{
	/// <summary>
	/// Commonly used collections for findbox BindToLists and lookups.
	/// </summary>
	public static class FindboxLookupCollections
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Internal key")]
		public static string CachingKey
		{
			get { return "Accounting" + GlbCompany.CurrentCompany.PK.ToStringKey(); }
		}

		public static GenericChargeCollection GetGenericChargeCollection(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue(GetPrefixKey(), () => new GenericChargeCollection(factory));
		}

		public static DebtorCollection GetDebtorCollection(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue(GetPrefixKey(), () => new DebtorCollection(factory));
		}

		public static CreditorCollection GetCreditorCollection(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue(GetPrefixKey(), () => new CreditorCollection(factory));
		}

		public static ForwarderCollection GetForwarderCollection(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue(GetPrefixKey(), () => new ForwarderCollection(factory));
		}

		public static OrgHeaderCollection GetOrgHeaderCollection(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue(GetPrefixKey(), () => new OrgHeaderCollection(factory));
		}

		public static OrganisationsFindBoxCollection GetOrganisationsFindBoxCollection(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue(GetPrefixKey(), () => new OrganisationsFindBoxCollection(factory));
		}

		public static ARInvoiceTermsList GetARInvoiceTermsList(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue(GetPrefixKey(), () => new ARInvoiceTermsList());
		}

		public static APInvoiceTermsList GetAPInvoiceTermsList(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue(GetPrefixKey(), () => new APInvoiceTermsList());
		}

		public static BusinessObjectCollection GetViewQuotedBookingCollection(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue(GetPrefixKey(), () => new Freight.QuotedBookings.Business.ViewOneOffQuoteCollection(factory));
		}

		#region Acc Ref Tables

		public static AccChargeCodeCollection GetChargeCodeCollection(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue(GetPrefixKey(), () => new AccChargeCodeCollection(factory));
		}

		public static AccChargeCodeCollection GetChargeCodeCollection(BusinessObjectFactory factory, GlbDepartment department)
		{
			return factory.GetCachedValue(GetPrefixKey() + department.GE_Code, () =>
			{
				var query = new ZQuery(AccChargeCodeSchema.AC_DepartmentFilterList, SQLComparisonOperator.Contains, department.GE_Code);
				query.AddToFilter(JoinCondition.Or, AccChargeCodeSchema.AC_DepartmentFilterList, SQLComparisonOperator.Contains, "ALL");
				return new AccChargeCodeCollection(factory, query);
			});
		}

		public static AccTaxRateCollection GetTaxRateCollection(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue(GetPrefixKey(), () => new AccTaxRateCollection(factory));
		}

		public static AccWithholdingCollection GetWithholdingCollection(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue(GetPrefixKey(), () => new AccWithholdingCollection(factory));
		}

		public static AccChequeBookCollection GetChequeBookCollection(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue(GetPrefixKey(), () => new AccChequeBookCollection(factory));
		}

		public static AccBankAccountCollection GetBankAccounts(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue(GetPrefixKey(), () => new AccBankAccountCollection(factory));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Internal key")]
		public static AccBankAccountCollection GetBankAccounts(BusinessObjectFactory factory, GlbBranch branch)
		{
			var branchCode = (branch != null ? (string)branch.GB_Code : "Branch");
			return factory.GetCachedValue(GetPrefixKey() + branchCode, () => new AccBankAccountCollection(factory, branch));
		}

		public static AccBankAccountCollection GetBankAccounts(BusinessObjectFactory factory, GlbCompany company)
		{
			return factory.GetCachedValue(GetPrefixKey(), () => new AccBankAccountCollection(factory, company));
		}

		public static AccGLHeaderCollection GetGLHeaders(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue(GetPrefixKey(), () => new AccGLHeaderCollection(factory));
		}

		public static JobCollection GetJobCollection_CurrentCompanyOnly(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue(GetPrefixKey() + "CurrentCompany", () => new JobCollection(factory, new ZQuery(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK)));
		}

		public static TransactionHeaderCollection GetTransactionHeaderCollection(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue(GetPrefixKey(), () => new TransactionHeaderCollection(factory));
		}

		#endregion

		#region Ref Table Collections

		public static RefCurrencyCollection GetCurrencyCollection(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue(GetPrefixKey(), () => new RefCurrencyCollection(factory));
		}

		public static RefUNLOCOCollection GetUNLOCOCollection(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue(GetPrefixKey(), () => new RefUNLOCOCollection(factory));
		}

		#endregion

		#region Glb Table Collections

		public static GlbBranchCollection GetAllBranchesCollection(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue(GetPrefixKey(), () => new GlbBranchCollection(factory));
		}

		public static GlbBranchDependentCollection GetCompanyBranchesCollection(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue(GetPrefixKey(), () => new GlbBranchDependentCollection(factory));
		}

		public static GlbDepartmentCollection GetDepartmentCollection(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue(GetPrefixKey(), () => new GlbDepartmentCollection(factory));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Internal key")]
		public static GlbDepartmentCollection GetDepartmentCollection_ActiveOnly(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue(GetPrefixKey() + "Active", () => new GlbDepartmentCollection(factory, new ZQuery(GlbDepartmentSchema.GE_IsActive, true)));
		}

		public static GlbCompanyCollection GetCompanyCollection(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue(GetPrefixKey(), () => new GlbCompanyCollection(factory));
		}

		public static GlbStaffCollection GetStaffCollection(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue(GetPrefixKey(), () => new GlbStaffCollection(factory));
		}

		#endregion

		public static AccGLHeaderCollection GetBSH_PnL_Active_DirectPosting_GLHeaderCollection(BusinessObjectFactory factory, Action<AccGLHeaderCollection, List<AccGLHeader>> showGLAccountsForImportAction)
		{
			var filter = new ZQuery(AccGLHeaderSchema.AG_DisallowDirectPosting, ZBool.False);
			filter.AddToFilter(AccGLHeaderSchema.AG_AccountType, SQLComparisonOperator.Equal, new ZString[] { AccountTypeComboBoxConstants.BalanceSheetAccount, AccountTypeComboBoxConstants.ProfitAndLossAccount });
			filter.AddToFilter(AccGLHeaderSchema.AG_IsActive, ZBool.True);

			var result = new AccGLHeaderCollection(factory, filter, showGLAccountsForImportAction);

			result.SetOverrideNotificationWhenAdditionalFilterNotMet(Res.GetString("6cfab28f-9716-4e50-b2bf-44605b3a2d52", "A valid GL account must be either Profit and Loss or Balance Sheet type account, must be an active account and allowed for direct posting. Please choose another GL account."));
			return result;
		}

		public static GlbBranchCollection GetActiveBranchForCompanyCollection(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue(GetPrefixKey() + "ActiveBranchForCompany", () =>
			{
				var filter = new ZQuery(GlbBranchSchema.GB_GC, GlbCompany.CurrentCompany.PK);
				filter.AddToFilter(GlbBranchSchema.GB_IsActive, ZBool.True);

				var result = new GlbBranchCollection(factory, filter);

				result.SetOverrideNotificationWhenAdditionalFilterNotMet(Res.GetString("e6da8adf-375d-45cc-972c-34c702aabd89", "This branch cannot be chosen because it belongs to another company or is an inactive branch. Please choose another branch."));
				return result;
			});
		}

		public static OrgDebtorGroupCollection GetOrgDebtorGroupCollection(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue(GetPrefixKey(), () => new OrgDebtorGroupCollection(factory));
		}

		static string GetPrefixKey()
		{
			return CachingKey;
		}
	}
}
