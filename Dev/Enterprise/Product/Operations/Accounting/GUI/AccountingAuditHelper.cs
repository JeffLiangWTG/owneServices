using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.GUI
{
	public static class AccountingAuditHelper
	{
		#region Audit Transaction & Undo Audit Transaction

		public static void HandleAuditTransaction(object sender, AuditAndCashEventArgs e)
		{
			var length = e?.SelectedTransactions?.Length ?? 0;

			if (length == 0)
			{
				Globals.Message.Show(Res.GetString("38ff597a-1090-11e8-b642-0ed5f89f718b", "Please select a transaction to audit."));
				return;
			}

			if (!e.SecurityCheckpoint.IsAllowed)
			{
				e.SecurityCheckpoint.ShowError();
				return;
			}

			if (e.SelectedTransactions.Any(x => !string.IsNullOrEmpty(x.AH_GS_NKAuditedBy)))
			{
				if (length == 1)
				{
					Globals.Message.Show(Res.GetString("51aa2b0a-a240-4821-a167-d55c18768ab7", "Selected transaction has been already audited."));
					return;
				}
				else
				{
					Globals.Message.Show(Res.GetString("4b19a7dc-1090-11e8-b642-0ed5f89f718b", "Some of the selected transactions have been already audited."));
					return;
				}
			}

			if (e.SelectedTransactions.Any(x => x.AH_SystemCreateUser == GlbStaff.CurrentUser.GS_Code))
			{
				if (length == 1)
				{
					Globals.Message.Show(Res.GetString("4ffbfa5c-1090-11e8-b642-0ed5f89f718b", "You cannot audit transaction created by yourself."));
					return;
				}
				else
				{
					Globals.Message.Show(Res.GetString("A555E764-E368-44E9-B422-B8C1429615A3", "You cannot audit transactions created by yourself."));
					return;
				}
			}

			BulkHandle(e.SelectedTransactions, (NoResString)"Transaction Audited", true, false); // Event description
		}

		public static void HandleUndoAuditTransaction(object sender, AuditAndCashEventArgs e)
		{
			var length = e?.SelectedTransactions?.Length ?? 0;

			if (length == 0)
			{
				Globals.Message.Show(Res.GetString("54539e5c-1090-11e8-b642-0ed5f89f718b", "Please select a transaction to undo audit."));
				return;
			}

			if (!e.SecurityCheckpoint.IsAllowed)
			{
				e.SecurityCheckpoint.ShowError();
				return;
			}

			if (e.SelectedTransactions.Any(x => string.IsNullOrEmpty(x.AH_GS_NKAuditedBy)))
			{
				if (length == 1)
				{
					Globals.Message.Show(Res.GetString("58fb6d4a-1090-11e8-b642-0ed5f89f718b", "Selected transaction has not been audited yet."));
					return;
				}
				else
				{
					Globals.Message.Show(Res.GetString("5c545c22-1090-11e8-b642-0ed5f89f718b", "Some of the selected transactions have not been audited yet."));
					return;
				}
			}

			if (e.SelectedTransactions.Any(x => x.AH_GS_NKAuditedBy != GlbStaff.CurrentUser.GS_Code))
			{
				if (length == 1)
				{
					Globals.Message.Show(Res.GetString("602a9e42-1090-11e8-b642-0ed5f89f718b", "You cannot undo audit transaction not audited by yourself."));
					return;
				}
				else
				{
					Globals.Message.Show(Res.GetString("45426162-2E44-4A20-9A68-D9DC99463E47", "Some of the selected transactions were not audited by yourself."));
					return;
				}
			}

			BulkHandle(e.SelectedTransactions, (NoResString)"Transaction Undo Audited", true, true); // Event description
		}
		#endregion

		#region Record Cashier & Clear Cashier

		public static void HandleRecordCashier(object sender, AuditAndCashEventArgs e)
		{
			var length = e?.SelectedTransactions?.Length ?? 0;

			if (length == 0)
			{
				Globals.Message.Show(Res.GetString("d3f21683-b1af-4a60-9c90-2c30d4eb6af7", "Please select a transaction to record cashier."));
				return;
			}

			if (!e.SecurityCheckpoint.IsAllowed)
			{
				e.SecurityCheckpoint.ShowError();
				return;
			}

			if (e.SelectedTransactions.Any(x => !string.IsNullOrEmpty(x.AH_GS_NKCashier)))
			{
				if (length == 1)
				{
					Globals.Message.Show(Res.GetString("b9757713-29a8-4886-b4bb-eb5267e11abd", "Cashier detail has been recorded against selected transaction."));
					return;
				}
				else
				{
					Globals.Message.Show(Res.GetString("aae346ff-5c0e-454a-b844-e9d76cc01471", "Cashier details have been recorded against some of the selected transactions."));
					return;
				}
			}

			if (e.SelectedTransactions.Any(x => x.AH_Ledger != LedgerTypes.CashBook && x.AH_TransactionType != TransactionTypes.Receipt && x.AH_TransactionType != TransactionTypes.Payment && x.AH_TransactionType != TransactionTypes.DirectReceipt && x.AH_TransactionType != TransactionTypes.DirectPayment && x.AH_TransactionType != TransactionTypes.Transfer)
				|| (e.SelectedTransactions.Any(x => (x.AH_Ledger == LedgerTypes.AccountsReceivable || x.AH_Ledger == LedgerTypes.AccountsPayable) && x.AH_TransactionType == TransactionTypes.Transfer)))
			{
				if (length == 1)
				{
					Globals.Message.Show(Res.GetString("8F33FE78-1F95-4A5E-87E8-88737F8F8B5B", "Selected transaction is non-cash transaction."));
					return;
				}
				else
				{
					Globals.Message.Show(Res.GetString("6662BAB7-8360-48F4-881A-3B739640272E", "Selected transactions include non-cash transactions."));
					return;
				}
			}

			BulkHandle(e.SelectedTransactions, (NoResString)"Cashier Recorded", false, false); // Event description
		}

		public static void HandleClearCashier(object sender, AuditAndCashEventArgs e)
		{
			var length = e?.SelectedTransactions?.Length ?? 0;

			if (length == 0)
			{
				Globals.Message.Show(Res.GetString("a2f64180-c2hj-1a78-7c91-1c34d5eb0gh1", "Please select a transaction to clear cashier."));
				return;
			}

			if (!e.SecurityCheckpoint.IsAllowed)
			{
				e.SecurityCheckpoint.ShowError();
				return;
			}

			if (e.SelectedTransactions.Any(x => string.IsNullOrEmpty(x.AH_GS_NKCashier)))
			{
				if (length == 1)
				{
					Globals.Message.Show(Res.GetString("ab16efe2-fd30-4234-b32d-969c5579d7b9", "No cashier detail has been recorded against selected transaction."));
					return;
				}
				else
				{
					Globals.Message.Show(Res.GetString("4e9bf249-3b08-4c5a-9858-1412836acaf9", "No cashier details have been recorded against some of the selected transactions."));
					return;
				}
			}

			if (e.SelectedTransactions.Any(x => x.AH_GS_NKCashier != GlbStaff.CurrentUser.GS_Code))
			{
				if (length == 1)
				{
					Globals.Message.Show(Res.GetString("a204f182-1090-11e8-b642-0ed5f89f718b", "You cannot clear cashier details of transaction of which you are not the cashier."));
					return;
				}
				else
				{
					Globals.Message.Show(Res.GetString("AF0249FC-BEB1-471D-9640-9F58223AA86B", "You cannot clear cashier details of transactions of which you are not the cashier."));
					return;
				}
			}

			BulkHandle(e.SelectedTransactions, (NoResString)"Cashier Cleared", false, true); // Event description
		}

		static void BulkHandle(AccTransactionHeader[] transactions, string eventDesc, bool isAuditAction, bool isUndoAction)
		{
			var newFactory = new BusinessObjectFactory();
			var transactionList = transactions.ToList();
			var loadedTransactions = new List<AccTransactionHeader>();

			var pairTranactions = transactionList.Where(x => x.AH_TransactionType == TransactionTypes.Transfer || x.AH_TransactionType == TransactionTypes.Contra);
			pairTranactions.ForEach(x =>
			{
				((TransactionHeader)x).RelatedTransactions.ForEach(y =>
				{
					if (!transactionList.Contains(y))
					{
						loadedTransactions.Add(y as AccTransactionHeader);
					}
				});
			});

			transactionList.AddRange(loadedTransactions);
			var selectedTransactions = newFactory.Load<AccTransactionHeader>(new ZQuery(AccTransactionHeaderSchema.PK, transactionList.Select(x => x.PK))).ToList();

			if (isAuditAction)
			{
				selectedTransactions.ForEach(x =>
				{
					x.AH_GS_NKAuditedBy = isUndoAction ? ZString.Empty : GlbStaff.CurrentUser.GS_Code;
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					x.Logs.AddNew(Events.EditedARecord, eventDesc);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				});
			}
			else
			{
				selectedTransactions.ForEach(x =>
				{
					x.AH_GS_NKCashier = isUndoAction ? ZString.Empty : GlbStaff.CurrentUser.GS_Code;
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					x.Logs.AddNew(Events.EditedARecord, eventDesc);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				});
			}

			newFactory.Save();
		}
		#endregion
	}
}
