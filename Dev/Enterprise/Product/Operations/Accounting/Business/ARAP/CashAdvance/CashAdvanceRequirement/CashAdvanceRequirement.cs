using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.ARAP.CashAdvance
{
	public abstract class CashAdvanceRequirement : ICashAdvanceRequirement
	{
		public CashAdvanceRequirement(BaseCharge jobCharge)
		{
			Argument.NotNull(jobCharge, nameof(jobCharge));
			Charge = jobCharge;
		}

		protected BaseCharge Charge { get; }

		protected BusinessObjectFactory Factory => Charge.Factory;

		protected abstract ZGuid LinePK { get; }

		protected abstract ZBool CashAdvanceRequired { get; }

		protected abstract ZDecimal JobChargeOSAmount { get; }

		protected abstract ZDecimal JobChargeLocalAmount { get; }

		protected abstract ZString JobChargeOSCurrency { get; }

		protected AccCashAdvanceRequestLine Line => Factory.Load<AccCashAdvanceRequestLine>(LinePK);

		void RemoveCashAdvanceRequirement()
		{
			if (((ICashAdvanceRequirement)this).IsPending)
			{
				RemoveCashAdvanceRequirementCore();
			}
			else
			{
				throw new InvalidOperationException(FormattableString.Invariant($"Advance Payment request for charge: {Charge.ChargeCode.AC_Code} is not in pending state."));
			}
		}

		protected abstract void RemoveCashAdvanceRequirementCore();

		protected abstract bool IsManualSettingOfCashAdvanceRequestStatusToPaidAllowed { get; }

		protected abstract bool IsFunctionalityEnabled { get; }

		protected abstract int Multiplier { get; }

		void UpdateInvoiceOutstandingAmount(Invoice relatedInvoice)
		{
			if (LocalPaidAmount > 0 && !relatedInvoice.AH_FullyPaidDate.IsValid)
			{
				var amountPaidInCompanyCurrency = LocalPaidAmount * Multiplier;

				if (Math.Abs(relatedInvoice.AH_OutstandingAmount) < Math.Abs(amountPaidInCompanyCurrency))
				{
					relatedInvoice.AH_OutstandingAmount = 0;
				}
				else
				{
					relatedInvoice.AH_OutstandingAmount -= amountPaidInCompanyCurrency;
				}
			}
		}

		void ReverseUpdatedInvoiceOutstandingAmount(Invoice relatedInvoice)
		{
			if (IsManualSettingOfCashAdvanceRequestStatusToPaidAllowed) // We should only set AH_OutstandingAmount here for manual payment, because for real payment it will be later set in InvoicingBaseReversing's UnmatchRelatedMatchingSessions() during reversing, set it twice will result invalid outstanding amount error
			{
				var invoiceTotal = Math.Abs(relatedInvoice.AH_LocalTotal);
				var overpayJournals = relatedInvoice.LoadOverpaymentCAIJournals();

				if (overpayJournals.Any())
				{
					var invoiceLine = Line.RequestHeader.CAH_Ledger == LedgerTypes.AccountsReceivable ? Line.RelatedJobCharge.ARLine : Line.RelatedJobCharge.APLine;
					var postedInvoiceLineAmount = invoiceLine.AL_LineAmount + invoiceLine.AL_GSTVAT;
					if (Math.Abs(relatedInvoice.AH_OutstandingAmount) + Math.Abs(postedInvoiceLineAmount) <= invoiceTotal)
					{
						relatedInvoice.AH_OutstandingAmount += Math.Abs(postedInvoiceLineAmount) * Multiplier;
						if (relatedInvoice.AH_FullyPaidDate.IsValid)
						{
							relatedInvoice.AH_FullyPaidDate = ZDateTime.Empty;
						}
					}
				}
				else if (Math.Abs(relatedInvoice.AH_OutstandingAmount) + LocalPaidAmount <= invoiceTotal)
				{
					relatedInvoice.AH_OutstandingAmount += LocalPaidAmount * Multiplier;
					if (relatedInvoice.AH_FullyPaidDate.IsValid)
					{
						relatedInvoice.AH_FullyPaidDate = ZDateTime.Empty;
					}
				}
			}
		}

		#region ICashAdvanceRequirement

		ZGuid ICashAdvanceRequirement.CashAdvanceRequestLinePK => Line?.PK ?? ZGuid.Empty;

		CashAdvanceRequestHeader ICashAdvanceRequirement.CashAdvanceRequest => CashAdvanceRequest;
		CashAdvanceRequestHeader CashAdvanceRequest => Line?.Factory.Load<CashAdvanceRequestHeader>(Line.CAL_CAH_RequestHeader);

		ZDecimal ICashAdvanceRequirement.OSAmount => CashAdvanceRequired ? (Line?.CAL_OSAmount ?? JobChargeOSAmount) : ZDecimal.Zero;

		ZDecimal ICashAdvanceRequirement.LocalAmount => CashAdvanceRequired ? (Line?.CAL_LocalAmount ?? JobChargeLocalAmount) : ZDecimal.Zero;

		ZDecimal ICashAdvanceRequirement.OSPaidAmount => CashAdvanceRequired ? (Line?.CAL_OSPaidAmount ?? ZDecimal.Zero) : ZDecimal.Zero;

		ZDecimal ICashAdvanceRequirement.LocalPaidAmount => LocalPaidAmount;
		protected ZDecimal LocalPaidAmount => CashAdvanceRequired ? (Line?.CAL_LocalPaidAmount ?? ZDecimal.Zero) : ZDecimal.Zero;

		ZDecimal ICashAdvanceRequirement.OSOutstandingAmount => CashAdvanceRequired ? (Line?.CAL_OSOutstandingAmount ?? ZDecimal.Zero) : ZDecimal.Zero;

		ZDecimal ICashAdvanceRequirement.LocalOutstandingAmount => CashAdvanceRequired ? (Line?.CAL_LocalOutstandingAmount ?? ZDecimal.Zero) : ZDecimal.Zero;

		ZString ICashAdvanceRequirement.OSCurrency => CashAdvanceRequired ? (Line?.RequestHeader.CAH_RX_NKTransactionCurrency ?? JobChargeOSCurrency) : ZString.Empty;

		bool ICashAdvanceRequirement.IsPending => CashAdvanceRequired && Line == null;

		bool ICashAdvanceRequirement.IsRequested => Line?.IsRequested ?? false;

		bool ICashAdvanceRequirement.IsPaid => Line?.IsPaid ?? false;

		bool ICashAdvanceRequirement.IsInvoiced => Line?.IsInvoiced ?? false;

		bool ICashAdvanceRequirement.IsCancelled => Line?.IsCancelled ?? false;

		bool ICashAdvanceRequirement.HasActiveCashAdvanceRequestLine => Line != null && !Line.IsCancelled;

		BaseCharge ICashAdvanceRequirement.JobCharge => Charge;

		bool ICashAdvanceRequirement.IsCashAdvanceRequired => CashAdvanceRequired;

		bool ICashAdvanceRequirement.IsManualSettingOfCashAdvanceRequestStatusToPaidAllowed => IsManualSettingOfCashAdvanceRequestStatusToPaidAllowed;

		bool ICashAdvanceRequirement.IsFunctionalityEnabled => IsFunctionalityEnabled;

		void ICashAdvanceRequirement.Cancel() => Line?.Cancel();

		void ICashAdvanceRequirement.MarkAsInvoiced(Invoice relatedInvoice)
		{
			Line?.MarkAsInvoiced();
			if (Line != null && Line.IsInvoiced && relatedInvoice != null)
			{
				if (IsFunctionalityEnabled)
				{
					if (IsManualSettingOfCashAdvanceRequestStatusToPaidAllowed)
					{
						UpdateInvoiceOutstandingAmount(relatedInvoice);
					}
					else
					{
						CashAdvanceRequest.UpdateStatusFromLine();
					}
				}
			}
		}

		void ICashAdvanceRequirement.UndoInvoicedStatus(Invoice relatedInvoice)
		{
			Line?.UndoInvoicedStatus();
			if (Line != null && Line.IsPaid && relatedInvoice != null)
			{
				if (IsFunctionalityEnabled)
				{
					ReverseUpdatedInvoiceOutstandingAmount(relatedInvoice);
				}
			}
		}

		void ICashAdvanceRequirement.RemoveCashAdvanceRequirement() => RemoveCashAdvanceRequirement();

		#endregion
	}
}
