using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.AccQueryClaims;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class UACreditNote : APCreditNote
	{
		public UACreditNote(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

		#region Overrides

		protected override void OnSavingCore()
		{
			SetEmptySubTypeWhenOrderingByPostDateEnabled();
			base.OnSavingCore();
		}

		protected override ZString Ledger
		{
			get { return LedgerTypes.UnapprovedPayableTransactions; }
		}

		protected override ZString TransactionType
		{
			get { return TransactionTypes.UACreditNote; }
		}

		protected override void GenerateReverseTransactionCore(bool mustTransform)
		{
			fIsReversing = true;
			IsReverseTransaction = true;
			SetCancellationFlag(true);
		}

		protected override bool IsTransactionInDatabaseReadOnlyCore
		{
			get { return base.IsTransactionInDatabaseReadOnlyCore && !IsReverseTransaction; }
		}

		public override Type DependentTransactionLineType
		{
			get { return typeof(UACreditNoteLine); }
		}

		protected override bool AllowDelete
		{
			get { return true; }
		}

		protected override void DeleteCore()
		{
			if (IsInDatabase)
			{
				new UnapprovedTransactionRejecter().Reject(this);
			}
			else
			{
				base.DeleteCore();
			}
		}

		protected override bool AH_TransactionNum_ReadOnly
		{
			get { return base.AH_TransactionNum_ReadOnly || IsRelatedToClaim; }
		}

		protected override TransactionHeaderValidation GetNewValidationCore()
		{
			return new UACreditNoteValidation(this);
		}

		public override bool AH_RX_NKTransactionCurrency_ReadOnly
		{
			get { return base.AH_RX_NKTransactionCurrency_ReadOnly || IsRelatedToClaim; }
			set { base.AH_RX_NKTransactionCurrency_ReadOnly = value; }
		}

		protected override bool AH_OH_ReadOnly
		{
			get { return base.AH_OH_ReadOnly || IsRelatedToClaim; }
		}

		protected override AccountingNumberFountainWrapper NumberFountainForTransactionNumber
		{
			get { return null; }
		}

		public override bool HasApprovalRequest
		{
			get { return false; }
		}

		internal protected override BranchLevelPostingConfigurationRegistryItem EnforceBranchLevelPostingRegistryItem
		{
			get { return null; }
		}

		#endregion

		AccQueryClaimBase fRelatedClaim;
		public AccQueryClaimBase RelatedClaim
		{
			get
			{
				if (IsInDatabase)
				{
					if (!AH_TransactionBelongsToGroup.IsValid)
					{
						return null;
					}

					ZDBOnlySubQuery apInvoiceQuery = new ZDBOnlySubQuery(typeof(APInvoice), AccTransactionHeaderSchema.PK);
					apInvoiceQuery.AddToFilter(AccTransactionHeaderSchema.AH_TransactionBelongsToGroup, AH_TransactionBelongsToGroup);
					apInvoiceQuery.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable);
					apInvoiceQuery.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Invoice);
					apInvoiceQuery.AddToFilter(AccTransactionHeaderSchema.AH_IsCancelled, false);
					ZDBOnlyQuery claimQuery = new ZDBOnlyQuery(typeof(APAccQueryClaim));
					claimQuery.AddSubQuery(AccQueryClaimSchema.AY_AH, apInvoiceQuery, JoinCondition.And);
					return Factory.LoadTop1<APAccQueryClaim>(claimQuery);
				}
				return fRelatedClaim;
			}
			set { fRelatedClaim = value; }
		}

		public bool IsRelatedToClaim
		{
			get { return RelatedClaim != null; }
		}

		protected override bool IsEnforcePostingAtFixedPlaceOfSupplyLevelRegistryEnabled => AccountingConfigurationRegistry.Instance.EnforcePostingAtFixedPlaceOfSupplyLevelForPayableTransactions.GetValueWithoutFallback(Company.PK.ToGuid(), Guid.Empty, Guid.Empty);

		void SetEmptySubTypeWhenOrderingByPostDateEnabled()
		{
			if (IsComplianceNumberAllocationMandatory && !AH_ComplianceSubType.IsEmpty)
			{
				AH_ComplianceSubType = ZString.Empty;
				LastWarningMessage = EmptySubTypeWhenOrderingByPostDateEnabledMessage;
			}
		}

		public ZString LastWarningMessage;
		string EmptySubTypeWhenOrderingByPostDateEnabledMessage => Res.GetString("999B7050-1D29-43EE-BF23-2161DE9E2804",
			$"Note: Unapproved AP Credit Note is saved without Compliance Sub Type, because it must be set only when Compliance Number is allocated");

		public override bool ShouldSetExchangeRateWhenSetInvoiceDate => true;
	}
}
