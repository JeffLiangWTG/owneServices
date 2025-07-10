using System;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Business.EmailNotification
{
	[Serializable]
	public class InvalidReverseTransactionException : ArgumentException
	{
		public InvalidReverseTransactionException(ZString errorMessage)
			: base(errorMessage)
		{
		}

#if NETFRAMEWORK
		protected InvalidReverseTransactionException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{ }
#endif
	}

	public class ZTransactionReversedEmail : AccountingEmailDef
	{
		public ZTransactionReversedEmail(IReversing originalTransaction)
		{
			if (originalTransaction == null)
			{
				ZString exceptionMessage = (NoResString)"Transaction being passed cannot be null";
				throw new InvalidReverseTransactionException(exceptionMessage);
			}
			else if (originalTransaction != null && originalTransaction.ReverseTransaction == null)
			{
				ZString exceptionMessage = (NoResString)"This transaction has not been reversed correctly. It does not have its reversing transaction generated yet";
				throw new InvalidReverseTransactionException(exceptionMessage);
			}
			else
			{
				this.subject = GetSubjectCore(originalTransaction, originalTransaction.ReverseTransaction);
				this.body = GetBodyCore(originalTransaction, originalTransaction.ReverseTransaction);
			}
		}

		readonly string subject;
		readonly string body;

		ZDateTime GetTransactionDate(IReversing transaction)
		{
			return transaction.TransactionDate.IsValid ?
						transaction.TransactionDate :
						ZDateTime.Now;
		}

		ZDateTime GetPostDate(IReversing transaction)
		{
			return transaction.PostDate.IsValid ?
					transaction.PostDate :
					ZDateTime.Now;
		}

		int GetTransactionAmountDecimalPlaces(IReversing transaction)
		{
			RefCurrency currency = RefCurrency.LoadFromCurrencyCode(transaction.Factory, transaction.CurrencyCode);
			return currency != null ? currency.Decimals : 2;
		}

		ZString StringForUnknownUser
		{
			get
			{
				return Res.GetString("a10bbb9f-17ce-4075-99bb-420a1357abbd", "Unknown");
			}
		}

		string GetBodyCore(IReversing originalTransaction, IReversing reverseTransaction)
		{
			GlbStaff originalTransactionCreator = originalTransaction is TransactionHeader ? ((TransactionHeader)originalTransaction).Creator : GlbStaff.CurrentUser;

			return Res.GetString("e9d9dffe-8139-4660-a036-35a0081afb19", "{0} {1} {2} dated {3} for {4} {5} posted to {6} by {7} ({8}) has been reversed by {9} {10} {11} dated {12} for {13} {14} posted to {15} by {16} ({17})\r\n\r\nReason: {18}",
				originalTransaction.Ledger,
				originalTransaction.TransactionType,
				originalTransaction.TransactionNumber,
				GetTransactionDate(originalTransaction).ToShortDateString(),
				originalTransaction.CurrencyCode,
				originalTransaction.OverseasTotalAmount.ToString(string.Format("N{0}", GetTransactionAmountDecimalPlaces(originalTransaction))),
				GetPostDate(originalTransaction).ToShortDateString(),
				(originalTransactionCreator != null) ? originalTransactionCreator.GS_LoginName : StringForUnknownUser,
				(originalTransactionCreator != null) ? originalTransactionCreator.GS_FullName : StringForUnknownUser,
				reverseTransaction.Ledger,
				reverseTransaction.TransactionType,
				reverseTransaction.TransactionNumber,
				GetTransactionDate(reverseTransaction).ToShortDateString(),
				reverseTransaction.CurrencyCode,
				reverseTransaction.OverseasTotalAmount.ToString(string.Format("N{0}", GetTransactionAmountDecimalPlaces(reverseTransaction))),
				GetPostDate(reverseTransaction).ToShortDateString(),
				(GlbStaff.CurrentUser != null) ? GlbStaff.CurrentUser.GS_LoginName : StringForUnknownUser,
				(GlbStaff.CurrentUser != null) ? GlbStaff.CurrentUser.GS_FullName : StringForUnknownUser,
				reverseTransaction.ReversingReason);
		}

		protected override string GetBody()
		{
			return body;
		}

		string GetSubjectCore(IReversing originalTransaction, IReversing reverseTransaction)
		{
			return Res.GetString("3c8fd8e1-69d0-44ea-a856-89de7e0ca88f", "{0} {1} {2} ({3}  {4} {5}) has been reversed by {6} {7} {8} ({9} {10} {11})",
				originalTransaction.Ledger,
				originalTransaction.TransactionType,
				originalTransaction.TransactionNumber,
				GetTransactionDate(originalTransaction).ToShortDateString(),
				originalTransaction.CurrencyCode,
				originalTransaction.OverseasTotalAmount.ToString(string.Format("N{0}", GetTransactionAmountDecimalPlaces(originalTransaction))),
				reverseTransaction.Ledger,
				reverseTransaction.TransactionType,
				reverseTransaction.TransactionNumber,
				GetTransactionDate(reverseTransaction).ToShortDateString(),
				reverseTransaction.CurrencyCode,
				reverseTransaction.OverseasTotalAmount.ToString(string.Format("N{0}", GetTransactionAmountDecimalPlaces(reverseTransaction))));
		}

		protected override string GetSubject()
		{
			return subject;
		}

		protected override GuidRegistryItem Recipient
		{
			get { return AccountingConfigurationRegistry.Instance.TransactionReverseNotifyGroup; }
		}
	}
}
