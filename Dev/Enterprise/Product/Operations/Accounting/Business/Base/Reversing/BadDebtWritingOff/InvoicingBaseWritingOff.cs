using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Base.Reversing.BadDebtWritingOff
{
	public class InvoicingBaseWritingOff : PayablesAndReceivablesWritingOff
	{
		public InvoicingBaseWritingOff(IBadDebtWritingOff badDebtTransaction)
			: base(badDebtTransaction)
		{
		}

		protected override ZString GenerateCantReverseErrorMessage()
		{
			string result = base.GenerateCantReverseErrorMessage();
			if (result == ZString.Empty && !CanWriteOffInvoice.result)
			{
				result = CanWriteOffInvoice.errorMessage;
			}
			if (result == ZString.Empty && !CanWriteOffCreditNote)
			{
				result = HaventSecuryRightsErrorMessage;
			}
			return result;
		}

		protected (bool result, string errorMessage) CanWriteOffInvoice
		{
			get
			{
				if (OriginalTransaction is ARInvoice arInvoice)
				{
					if (!Env.Security.BadDebtWriteOffReceivablesInvoice.IsAllowed)
					{
						return (false, HaventSecuryRightsErrorMessage);
					}
					else if (AccountingMasterFilesUtils.ShouldPreventCreateReversalTransactions(arInvoice.AH_Ledger, arInvoice.AH_GC))
					{
						return (false, AccountingMasterFilesUtils.ARInvoiceReversalDisallowedMessage.Replace("Posting", AccountingConstants.WriteOffAsBadDebtText));
					}
				}

				return (true, string.Empty);
			}
		}

		protected bool CanWriteOffCreditNote
		{
			get
			{
				return !(OriginalTransaction is ARCreditNote) ||
					(OriginalTransaction is ARCreditNote && Env.Security.BadDebtWriteOffReceivablesCreditNote.IsAllowed);
			}
		}

		protected override bool CanTransactionBeReversed()
		{
			bool result = base.CanTransactionBeReversed();
			if (result)
			{
				result &= CanWriteOffInvoice.result;
			}
			if (result)
			{
				result &= CanWriteOffCreditNote;
			}
			return result;
		}
	}
}
