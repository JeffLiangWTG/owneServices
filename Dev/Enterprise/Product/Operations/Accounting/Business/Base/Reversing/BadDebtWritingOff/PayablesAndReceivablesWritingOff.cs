using System;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Security;

namespace Enterprise.Accounting.Business.Base.Reversing.BadDebtWritingOff
{
	public partial class PayablesAndReceivablesWritingOff : PayablesAndReceivablesReversing
	{
		public PayablesAndReceivablesWritingOff(IBadDebtWritingOff badDebtTransaction)
			: base(badDebtTransaction as IPayablesAndReceivables)
		{
		}

		protected override ZString GenerateCantReverseErrorMessage()
		{
			ZString result = ZString.Empty;
			if (OriginalTransaction.IsReversed)
			{
				result = AlreadyReversedErrorMessage;
			}
			if (result == ZString.Empty && IsTransactionForInactiveOrganisation)
			{
				result = TransactionForInactiveOrganisationErrorMessage;
			}
			if (result == ZString.Empty && !CanWriteOffJournal)
			{
				result = HaventSecuryRightsErrorMessage;
			}
			if (result == ZString.Empty && !IsBadDebtAccountSetInRegistry)
			{
				result = HaventBadDebtAccountInRegistryErrorMessage;
			}
			if (result == ZString.Empty && !IsTransactionNotPartiallyPaid)
			{
				result = CantWriteOffPartiallyPaidTransactionErrorMessage;
			}
			return result;
		}

		protected bool CanWriteOffJournal
		{
			get
			{
				return !(OriginalTransaction is ARJournal) ||
					(OriginalTransaction is ARJournal && Env.Security.BadDebtWriteOffReceivablesJournal.IsAllowed);
			}
		}

		protected bool IsBadDebtAccountSetInRegistry
		{
			get
			{
				return (Guid)AccountingConfigurationRegistry.Instance.BadDebtWriteOffAccount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty) != Guid.Empty;
			}
		}

		protected string HaventSecuryRightsErrorMessage
		{
			get { return Res.GetString("0bdc27aa-145d-41db-b2b8-d3f4d430feab", "{0} Bad Debt Writing Off.", SecurityCore.SecurityErrorMessage); }
		}

		protected string HaventBadDebtAccountInRegistryErrorMessage
		{
			get
			{
				return Res.GetString("89beba6c-2d1a-4ecb-86c6-f8fbdac57655", "Please set the default Bad Debt Write-Off Account in the Registry:\r\n\r\nAccounting > General Ledger Defaults > Link Account > Bad Debt Write-Off Account");
			}
		}

		protected bool IsTransactionNotPartiallyPaid
		{
			get
			{
				TransactionHeader originalTransactionHeader = OriginalTransaction as TransactionHeader;
				return originalTransactionHeader == null ||
					Math.Abs(originalTransactionHeader.AH_OutstandingAmount) ==
				Math.Abs(originalTransactionHeader.AH_LocalTotal);
			}
		}

		protected string CantWriteOffPartiallyPaidTransactionErrorMessage
		{
			get
			{
				return Res.GetString("2e9e9db0-4a70-4c70-9b20-e366c9ce437e", "This transaction cannot be {0} because it has been partially paid.",
					GetOperationNameInPastParticiple);
			}
		}

		protected override ZString GetOperationNameInPastParticiple
		{
			get { return Res.GetString("64C2316C-373A-4392-A74D-7D4D9BD8829E", "written off"); }
		}

		protected override bool CanTransactionBeReversed()
		{
			bool result = !OriginalTransaction.IsReversed;
			if (result)
			{
				result &= !IsTransactionForInactiveOrganisation;
			}
			if (result)
			{
				result &= CanWriteOffJournal;
			}
			if (result)
			{
				result &= IsBadDebtAccountSetInRegistry;
			}
			if (result)
			{
				result &= IsTransactionNotPartiallyPaid;
			}
			return result;
		}

		protected override void SetReversingDescriptionOnTransactions()
		{
			ZString reverseTransactionDesc = String.Format(AccountingConfigurationRegistry.Instance.GetTransactionDescriptionFromCode(AccountingConstants.VoucherItemRegistryCode.BadDebtWriteOff,
										Res.GetString("804a0f4f-dc8b-4c56-bafa-a2ca237f65e9", "Bad Debt Write-Off")) + " {0}", OriginalTransaction.TransactionNumber);

			ReverseTransaction.SetDescription(reverseTransactionDesc);
			ReverseTransaction.SetNumberOfSupportingDocuments(AccountingConfigurationRegistry.Instance.GetVoucherNoOfAttchmentsFromCode(AccountingConstants.VoucherItemRegistryCode.BadDebtWriteOff, 0));
		}

		protected override void SetCancellationFlagOnTransactionsToReverse()
		{
		}

		protected override void GenerateReverseTransactions()
		{
			OriginalTransaction.GenerateReverseTransaction(false);
			fReverseTransaction = OriginalTransaction.ReverseTransaction;
		}
	}
}