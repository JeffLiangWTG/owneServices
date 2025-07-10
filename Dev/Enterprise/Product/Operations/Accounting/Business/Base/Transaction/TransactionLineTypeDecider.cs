using System;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.WIPAccrual;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.Base.Transaction
{
	public class TransactionLineTypeDecider : TypeDecider
	{
		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			string transactionLineType = row[TransactionLine.Schema.AL_LineType].ToString();
			ZGuid transactionHeaderPK = new ZGuid(row[TransactionLine.Schema.AL_AH].ToString());

			try
			{
				switch (transactionLineType)
				{
					case TransactionLineTypes.Cost:
						{
							TransactionHeader transactionHeader = factory.Load<TransactionHeader>(transactionHeaderPK);
							if (transactionHeader != null)
							{
								if (transactionHeader.AH_Ledger == LedgerTypes.AccountsPayable ||
									transactionHeader.AH_Ledger == LedgerTypes.UnapprovedPayableTransactions ||
									transactionHeader.AH_Ledger == LedgerTypes.IncompleteTransactions ||
									transactionHeader.AH_Ledger == LedgerTypes.JobCosting)
								{
									switch (transactionHeader.AH_TransactionType)
									{
										case TransactionTypes.IncompleteInvoice:
										case TransactionTypes.Invoice:
											return typeof(APInvoiceLine);
										case TransactionTypes.IncompleteCreditNote:
										case TransactionTypes.CreditNote:
											return typeof(APCreditNoteLine);
										case TransactionTypes.IncompleteAdjustmentNote:
										case TransactionTypes.AdjustmentNote:
											return typeof(APAdjustmentNoteLine);
										case TransactionTypes.UAInvoice:
											return typeof(UAInvoiceLine);
										case TransactionTypes.UACreditNote:
											return typeof(UACreditNoteLine);
										case TransactionTypes.JobRevenueJournal:
											return typeof(JobRevenueJournalLine);
										default:
											throw new ArgumentException(GetTransactionTypeIsNotValidError(transactionHeader.AH_TransactionType, transactionHeader.AH_Ledger, transactionLineType));
									}
								}
								else
								{
									throw new ArgumentException(GetTransactionLedgerIsNotValidError(transactionHeader.AH_Ledger, transactionLineType));
								}
							}
							else
							{
								throw new ArgumentException(GetTransactionNotLoadedError(transactionHeaderPK, transactionLineType, factory));
							}
						}
					case TransactionLineTypes.Revenue:
						{
							TransactionHeader transactionHeader = factory.Load<TransactionHeader>(transactionHeaderPK);
							if (transactionHeader != null)
							{
								if (transactionHeader.AH_Ledger == LedgerTypes.AccountsReceivable)
								{
									switch (transactionHeader.AH_TransactionType)
									{
										case TransactionTypes.Invoice:
											return typeof(ARInvoiceLine);
										case TransactionTypes.CreditNote:
											return typeof(ARCreditNoteLine);
										case TransactionTypes.AdjustmentNote:
											return typeof(ARAdjustmentNoteLine);
										default:
											throw new ArgumentException(GetTransactionTypeIsNotValidError(transactionHeader.AH_TransactionType, transactionHeader.AH_Ledger, transactionLineType));
									}
								}
								else if (transactionHeader.AH_Ledger == LedgerTypes.JobCosting)
								{
									switch (transactionHeader.AH_TransactionType)
									{
										case TransactionTypes.Journal:
											return typeof(JCJournalLine);
										case TransactionTypes.JobRevenueJournal:
											return typeof(JobRevenueJournalLine);
										default:
											throw new ArgumentException(GetTransactionTypeIsNotValidError(transactionHeader.AH_TransactionType, transactionHeader.AH_Ledger, transactionLineType));
									}
								}
								else
								{
									throw new ArgumentException(GetTransactionLedgerIsNotValidError(transactionHeader.AH_Ledger, transactionLineType));
								}
							}
							else
							{
								throw new ArgumentException(GetTransactionNotLoadedError(transactionHeaderPK, transactionLineType, factory));
							}
						}
					case TransactionLineTypes.UnapprovedCost:
						{
							TransactionHeader transactionHeader = factory.Load<TransactionHeader>(transactionHeaderPK);
							if (transactionHeader != null)
							{
								if (transactionHeader.AH_Ledger == LedgerTypes.UnapprovedPayableTransactions)
								{
									switch (transactionHeader.AH_TransactionType)
									{
										case TransactionTypes.UAInvoice:
											return typeof(UAInvoiceLine);
										case TransactionTypes.UACreditNote:
											return typeof(UACreditNoteLine);
										default:
											throw new ArgumentException(GetTransactionTypeIsNotValidError(transactionHeader.AH_TransactionType, transactionHeader.AH_Ledger, transactionLineType));
									}
								}
								else
								{
									throw new ArgumentException(GetTransactionLedgerIsNotValidError(transactionHeader.AH_Ledger, transactionLineType));
								}
							}
							else
							{
								throw new ArgumentException(GetTransactionNotLoadedError(transactionHeaderPK, transactionLineType, factory));
							}
						}
					default:
						{
							if (WIPAccrualTypeDecider.IsApplicableTo(row))
							{
								return new WIPAccrualTypeDecider().GetTypeForLoad(row, factory);
							}
							throw new ArgumentException(string.Format("Transaction Line Type '{0}' is not a valid.", transactionLineType));
						}
				}
			}
			catch (ArgumentException ex)
			{
				ErrorReporter.ReportOnce("TransactionLineTypeDecider", string.Format("Problem Line PK '{0}'.", row[TransactionLine.Schema.PK]), ex);
				throw;
			}
		}

		public override Type GetTypeForNew()
		{
			throw new NoConcreteTypeException("New Transaction Line type cannot be determined");
		}

		public override Type GetTypeForBinding()
		{
			return null;
		}

		string GetTransactionNotLoadedError(ZGuid transactionPK, string lineType, BusinessObjectFactory factory)
		{
			var reason = string.Empty;
			if (transactionPK.IsValid)
			{
				var bizOs = factory.GetBizOsForPK(transactionPK.ToGuid());
				if (bizOs.Any(x => x.IsDeleted))
				{
					reason = (NoResString)" because it was deleted";
				}
			}

			return string.Format((NoResString)"Transaction Header with PK '{0}' can't be loaded{2}, Line Type '{1}'.", transactionPK, lineType, reason);
		}

		string GetTransactionLedgerIsNotValidError(string transactionLedger, string lineType)
		{
			return string.Format((NoResString)"Transaction Header Ledger '{0}' is not valid for Line Type '{1}'.", transactionLedger, lineType);
		}

		string GetTransactionTypeIsNotValidError(string transactionType, string transactionLedger, string lineType)
		{
			return string.Format((NoResString)"Transaction Type '{0}' is not valid for Transaction Header Ledger '{1}' and Line Type '{2}'.", transactionType, transactionLedger, lineType);
		}
	}
}
