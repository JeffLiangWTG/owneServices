using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public abstract class CreditNote : InvoicingBase
	{
		public CreditNote(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public void CopyValuesFrom(InvoicingBase invoiceOrCreditNote)
		{
			base.CopyPersistentValuesFrom(invoiceOrCreditNote);

			using (SetExchangeRateSuspender.GetSuspender())
			{
				AH_RX_NKTransactionCurrency = invoiceOrCreditNote.AH_RX_NKTransactionCurrency;
				UseJobExchangeRate = invoiceOrCreditNote.UseJobExchangeRate;
			}
			AH_ExchangeRate = invoiceOrCreditNote.AH_ExchangeRate;
			AH_GB_TaxBranch = invoiceOrCreditNote.AH_GB_TaxBranch;
		}

		#region Overrides

		public override ZGuid OriginalTransactionReference
		{
			get
			{
				return AH_TransactionBelongsToGroup;
			}
			set
			{
				if (OriginalTransactionReference != value)
				{
					var oldValue = base.OriginalTransactionReference;
					base.OriginalTransactionReference = value;
					AH_TransactionBelongsToGroup = value;
					if (OriginalTransactionIsSet)
					{
						AH_OriginalTransactionNum = ZString.Empty;
						AH_OriginalInvoiceDate = ZDate.Empty;
					}
					if (oldValue != value)
					{
						ReasonCode = string.Empty;
						ReasonDescription = string.Empty;
						ReasonCodeInfo.RefreshBinding();
						ReasonDescriptionInfo.RefreshBinding();
					}

					OriginalTransactionReferenceInfo.RefreshBinding();

					if (Validation is CreditNoteValidation creditNoteValidation)
					{
						creditNoteValidation.ValidateOriginalTransactionReference();
					}
				}

				PopulateFromOriginalTransaction(OriginalReferenceTransaction);
			}
		}

		protected override ZString TransactionType
		{
			get
			{
				return Enterprise.ZArchitecture.Core.TransactionTypes.CreditNote;
			}
		}

		public override bool IsCommissionable
		{
			get { return AH_Ledger == LedgerTypes.AccountsPayable || AH_Ledger == LedgerTypes.AccountsReceivable; }
		}

		protected override TransactionHeaderValidation GetNewValidationCore()
		{
			return new CreditNoteValidation(this);
		}

		public override bool HasApprovalRequest
		{
			get { return TransactionRelatedApprovalRequest != null; }
		}

		protected bool AH_OSTaxAmount_ReadOnly
		{
			get { return true; }
		}

		protected override bool AH_OSExTaxAmount_ReadOnly
		{
			get { return true; }
		}

		protected bool AH_OSWHTAmount_ReadOnly
		{
			get { return true; }
		}

		protected override bool AH_OSExtraTaxAmount_ReadOnly
		{
			get { return true; }
		}

		protected bool AH_LocalTaxAmount_ReadOnly
		{
			get { return true; }
		}

		protected override bool AH_LocalExTaxAmount_ReadOnly
		{
			get { return true; }
		}

		protected bool AH_LocalWHTAmount_ReadOnly
		{
			get { return true; }
		}

		protected override bool AH_LocalExtraTaxAmount_ReadOnly
		{
			get { return true; }
		}

		protected override void OnSavingBeforeBase()
		{
			UpdateReversedCAIAndEXXPostDate();
			base.OnSavingBeforeBase();
		}

		#endregion

		#region Cash Advance related logic

		internal IEnumerable<Journal.Journal> ReversedCashAdvanceCAIJournals { get; set; }
		internal IEnumerable<ExchangeDifference> ReversedCashAdvanceEXXs { get; set; }

		void UpdateReversedCAIAndEXXPostDate()
		{
			if (!IsInDatabase && IsReversalTransaction)
			{
				var originalInvoice = OriginalTransaction as Invoice;
				if (originalInvoice != null)
				{
					if (originalInvoice.IsCashAdvanceFunctionalityEnabled &&
						!originalInvoice.IsManualSettingOfCashAdvanceRequestStatusToPaidAllowed)
					{
						if (ReversedCashAdvanceCAIJournals.Any())
						{
							ReversedCashAdvanceCAIJournals.ForEach(x => x.AH_PostDate = AH_PostDate);
						}
						if (ReversedCashAdvanceEXXs.Any())
						{
							ReversedCashAdvanceEXXs.ForEach(x => x.AH_PostDate = AH_PostDate);
						}
					}
				}
			}
		}

		#endregion
	}
}
