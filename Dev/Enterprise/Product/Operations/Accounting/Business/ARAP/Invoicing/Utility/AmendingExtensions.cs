using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public static class AmendingExtensions
	{
		public static InvoicingBase GenerateAmendingTransaction(this IAmending amending, Type typeOfAmendment)
		{
			if (typeof(ARCreditNote).IsAssignableFrom(typeOfAmendment))
			{
				return (InvoicingBase)amending.GenerateAmendingTransaction(TransactionTypes.CreditNote);
			}
			else if (typeof(ARInvoice).IsAssignableFrom(typeOfAmendment))
			{
				return (InvoicingBase)amending.GenerateAmendingTransaction(TransactionTypes.Invoice);
			}
			else if (typeof(APInvoice).IsAssignableFrom(typeOfAmendment))
			{
				return (InvoicingBase)amending.GenerateAmendingTransaction(TransactionTypes.CreditNote);
			}

			return null;
		}

		public static TAmendment GenerateAmendingTransaction<TAmendment>(this IAmending amending) where TAmendment : class, IAmending
		{
			return GenerateAmendingTransaction(amending, typeof(TAmendment)) as TAmendment;
		}

		public static bool CheckIsAmendingTransaction(this IAmending amending, bool hasBeenCreatedAsAmending)
		{
			var result = false;
			var original = amending.OriginalTransaction;
			if (original == null)
			{
				result = amending is CreditNote && amending.IsAmendingTransaction_SoftReference;
			}
			else
			{
				var reversing = original as IReversing;
				result = hasBeenCreatedAsAmending || amending.IsInDatabase || amending.IsAmendingTransaction_SoftReference;
				result = result && original != null && (original.Ledger == LedgerTypes.AccountsReceivable || original.Ledger == LedgerTypes.AccountsPayable);
				result = result && (original.TransactionType == TransactionTypes.CreditNote || original.TransactionType == TransactionTypes.Invoice);
				result = result && reversing != null && !reversing.IsReversed;
			}

			return result;
		}

		public static ZGuid[] GetOriginalTransactionJobPKs(this IAmending amending)
		{
				List<ZGuid> result = new List<ZGuid>();

				if (amending.IsAmendingTransaction)
				{
					InvoicingBase original = amending.OriginalTransaction as InvoicingBase;
					if (original != null)
					{
						result.AddRange((from MasterFiles.Business.JobHeader job in original.InvoiceDependentJobs.Select(a => a.Job) select job.PK).Distinct());
					}
				}

				return result.ToArray();
		}

		public static ZGuid GetOriginalTransactionAccountPK(this IAmending amending)
		{
				ZGuid result = ZGuid.Empty;

				if (amending.IsAmendingTransaction)
				{
					InvoicingBase original = amending.OriginalTransaction as InvoicingBase;
					if (original != null)
					{
						result = original.AH_OH;
					}
				}

				return result;
		}

		public static void PopulateFromOriginalTransaction(this IAmending amending)
		{
			if (amending.IsAmendingTransaction)
			{
				InvoicingBase source = amending.OriginalTransaction as InvoicingBase;
				InvoicingBase target = amending as InvoicingBase;

				if (source != null && target != null && source.AH_Ledger == target.AH_Ledger)
				{
					target.AH_Desc = string.Format(Res.GetString("{207FC364-FF8B-4A1A-BF03-54568982526E}", "Amendment related to") + " {0} {1}", source.AH_TransactionType, source.AH_TransactionNum).ToUpper();
					if (source.AH_Ledger == LedgerTypes.AccountsReceivable)
					{
						target.SetConsolidatedInvoiceRefForAmendedTransactionStrategy = new SetConsolidatedInvoiceRefForAmendedTransactionStrategy(source);
					}
					target.PopulateFromOriginalTransaction(source, source.AH_TransactionType != target.AH_TransactionType);
				}
			}
		}
	}
}
