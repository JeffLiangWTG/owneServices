using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class FilteredInvoicingLineBaseCollectionView : BusinessObjectCollectionView<InvoicingLineBase>, IHaveAbstractElementType
	{
		public FilteredInvoicingLineBaseCollectionView(InvoicingLineBaseCollection collectionToFilter)
			: base(collectionToFilter)
		{
		}

		#region IsThisPartOfTheCollection

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			var result = true;
			var invoiceLine = element as InvoicingLineBase;
			if (invoiceLine != null && (invoiceLine.IsInDatabase || (invoiceLine.TransactionHeader != null && invoiceLine.TransactionHeader.AH_Ledger == LedgerTypes.IncompleteTransactions))
				&& !IsAllowedtoViewTransactionOutsideLoginPermission() && invoiceLine.Branch != null && invoiceLine.Department != null)
			{
				if (invoiceLine.Branch != GlbBranch.CurrentBranch || invoiceLine.Department != GlbDepartment.CurrentDepartment)
				{
					result = AllowedToLogin(invoiceLine.Branch, invoiceLine.Department);
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

		protected bool IsAllowedtoViewTransactionOutsideLoginPermission()
		{
			var invoiceBaseLineCollection = collectionToFilter as InvoicingLineBaseCollection;
			var header = invoiceBaseLineCollection != null ? invoiceBaseLineCollection.InvoicingBase : null;
			var isAllowed = true;
			if (header != null)
			{
				if (header.AH_Ledger == LedgerTypes.AccountsReceivable)
				{
					isAllowed = Env.Security.ReceivablesViewingFinancialOutsideLoginPermission.IsAllowed;
				}
				else if (header.AH_Ledger == LedgerTypes.AccountsPayable || header.AH_Ledger == LedgerTypes.UnapprovedPayableTransactions || header.AH_Ledger == LedgerTypes.IncompleteTransactions)
				{
					isAllowed = Env.Security.PayablesViewingFinancialOutsideLoginPermission.IsAllowed;
				}
			}
			return isAllowed;
		}

		#endregion

		#region Implementation

		protected override bool AllowNewCore
		{
			get { return collectionToFilter.AllowNew; }
		}

		protected override bool AllowRemoveCore
		{
			get { return collectionToFilter.AllowRemove; }
		}

		Type IHaveAbstractElementType.NonAbstractTypeOfElements
		{
			get
			{
				return typeof(AccTransactionLines);
			}
		}

		protected override BusinessObject AddNewCore(Type bizOType)
		{
			throw new NotSupportedException("This collection contains abstract type entity.");
		}

		#endregion

		public override IDisposable SuspendAdditionallyForImport()
		{
			return new DisposableList(new[] { base.SuspendAdditionallyForImport(), (collectionToFilter as InvoicingLineBaseCollection)?.InvoicingBase?.GetSetFinalFlagWhenImportingFromSplitChargeAndLinesSuspender(true).GetSuspender() });
		}
	}
}
