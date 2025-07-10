using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class UAInvoice : APInvoice
	{
		public UAInvoice(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

		#region Overrides

		protected override ZString Ledger
		{
			get { return LedgerTypes.UnapprovedPayableTransactions; }
		}

		protected override ZString TransactionType
		{
			get { return TransactionTypes.UAInvoice; }
		}

		protected override ZBool DefaultIsCashInvoiceCore
		{
			get { return false; }
		}

		protected override void UpdateRelevantCashAdvanceRequestsCore()
		{
			//Cash advance logic is not applicable to UA Invoice
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
			get { return typeof(UAInvoiceLine); }
		}

#if DEBUG
		public override bool HasImplementedGenerateAmendingTransaction => false;
#endif

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

		protected override bool IsEnforcePostingAtFixedPlaceOfSupplyLevelRegistryEnabled => AccountingConfigurationRegistry.Instance.EnforcePostingAtFixedPlaceOfSupplyLevelForPayableTransactions.GetValueWithoutFallback(Company.PK.ToGuid(), Guid.Empty, Guid.Empty);

		public override bool ShouldSetExchangeRateWhenSetInvoiceDate => true;

		#endregion
	}
}
