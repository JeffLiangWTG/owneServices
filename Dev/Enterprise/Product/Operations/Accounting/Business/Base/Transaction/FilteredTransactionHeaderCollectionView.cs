using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Business.Base.Transaction
{
	[ModuleID(ModuleId.ARTransaction)]
	public class FilteredTransactionHeaderCollectionView : BusinessObjectCollectionView<TransactionHeader>
	{
		public FilteredTransactionHeaderCollectionView(TransactionHeaderCollection collectionToFilter)
			: base(collectionToFilter)
		{
		}

		#region IsThisPartOfTheCollection

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			var result = true;
			var transactionHeader = element as TransactionHeader;
			if (transactionHeader != null && !IsAllowedtoViewTransactionOutsideLoginPermission(transactionHeader) && transactionHeader.Branch != null && transactionHeader.Department != null)
			{
				if (transactionHeader.Branch != GlbBranch.CurrentBranch || transactionHeader.Department != GlbDepartment.CurrentDepartment)
				{
					result = AllowedToLogin(transactionHeader.Branch, transactionHeader.Department);
				}
			}
			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Combining Cache key, not related to GUI")]
		protected bool AllowedToLogin(GlbBranch branch, GlbDepartment department)
		{
			return Factory.GetCachedValue("Login BRN:" + branch.GB_Code + " DEP:" + department.GE_Code, delegate
			{
				var security = new UserLoginController().GetSecurityForUser(GlbStaff.CurrentUser.GS_LoginName, branch.PK.ToGuid(), department.PK.ToGuid());
				return security.Login.IsAllowed;
			});
		}

		bool IsAllowedtoViewTransactionOutsideLoginPermission(TransactionHeader header)
		{
			if (header.AH_Ledger == LedgerTypes.AccountsReceivable)
			{
				return Env.Security.ReceivablesViewingFinancialOutsideLoginPermission.IsAllowed;
			}
			else if (header.AH_Ledger == LedgerTypes.AccountsPayable || header.AH_Ledger == LedgerTypes.UnapprovedPayableTransactions || header.AH_Ledger == LedgerTypes.IncompleteTransactions)
			{
				return Env.Security.PayablesViewingFinancialOutsideLoginPermission.IsAllowed;
			}
			else
			{
				return true;
			}
		}

		#endregion

		#region Implementation

		protected override BusinessObject AddNewCore()
		{
			throw new NotSupportedException("This collection contains abstract type entity.");
		}

		protected override BusinessObject AddNewCore(Type bizOType)
		{
			throw new NotSupportedException("This collection contains abstract type entity.");
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		public override void Load()
		{
			collectionToFilter.Load();
		}

		public override Type GetTypeOfElementsFromPK(ZGuid pK)
		{
			return pK.IsValid ? FindByPK(pK).GetType() : typeof(TransactionHeader);
		}

		#endregion
	}
}
