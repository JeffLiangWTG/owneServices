using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.ElectronicMessaging.Common
{
	public static class EInvoicingControllerIdDecider
	{
		public static ControllerID GetControllerIDRelatedToTransaction(ZString ledger, ZString transactionType)
		{
			ControllerID controllerID = null;
			if (ledger == LedgerTypes.AccountsReceivable)
			{
				if (transactionType == TransactionTypes.Invoice)
				{
					controllerID = ControllerIDs.ARInvoice;
				}
				else if (transactionType == TransactionTypes.CreditNote)
				{
					controllerID = ControllerIDs.ARCreditNote;
				}
				else if (transactionType == TransactionTypes.AdjustmentNote)
				{
					controllerID = ControllerIDs.ARAdjustmentNote;
				}
			}
			else if (ledger == LedgerTypes.AccountsPayable)
			{
				if (transactionType == TransactionTypes.Invoice)
				{
					controllerID = ControllerIDs.APInvoice;
				}
				else if (transactionType == TransactionTypes.CreditNote)
				{
					controllerID = ControllerIDs.APCreditNote;
				}
				else if (transactionType == TransactionTypes.AdjustmentNote)
				{
					controllerID = ControllerIDs.APAdjustmentNote;
				}
			}
			return controllerID;
		}

		public static ControllerID GetControllerIDRelatedToComplianceDocument(ZString ledger)
		{
			ControllerID controllerID = null;
			if (ledger == LedgerTypes.AccountsReceivable)
			{
				controllerID = ControllerIDs.ARComplianceDocument;
			}
			else if (ledger == LedgerTypes.AccountsPayable)
			{
				controllerID = ControllerIDs.APComplianceDocument;
			}
			return controllerID;
		}
	}
}
