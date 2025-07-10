using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;

namespace Enterprise.Accounting.Business.ARAP.CashAdvance
{
	public interface ICashAdvanceRequirement
	{
		BaseCharge JobCharge { get; }

		ZGuid CashAdvanceRequestLinePK { get; }

		bool IsCashAdvanceRequired { get; }

		bool IsManualSettingOfCashAdvanceRequestStatusToPaidAllowed { get; }

		bool IsFunctionalityEnabled { get; }

		CashAdvanceRequestHeader CashAdvanceRequest { get; }

		ZDecimal OSAmount { get; }

		ZDecimal LocalAmount { get; }

		ZDecimal OSPaidAmount { get; }

		ZDecimal LocalPaidAmount { get; }

		ZDecimal OSOutstandingAmount { get; }

		ZDecimal LocalOutstandingAmount { get; }

		ZString OSCurrency { get; }

		bool IsPending { get; }

		bool IsRequested { get; }

		bool IsPaid { get; }

		bool IsInvoiced { get; }

		bool IsCancelled { get; }

		bool HasActiveCashAdvanceRequestLine { get; }

		void Cancel();

		void MarkAsInvoiced(Invoice relatedInvoice);

		void UndoInvoicedStatus(Invoice relatedInvoice);

		void RemoveCashAdvanceRequirement();
	}
}
