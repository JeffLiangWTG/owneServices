using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.AccountingVoucherPrint
{
	public static class LedgerDescription
	{
		public static string AccountsReceivable => Res.GetString("588608b8-39c6-4cca-99f0-9b24a8119fd5", "Account Receivable");
		public static string AccountsPayable => Res.GetString("f9fc961f-9b49-4882-9387-40880cc98cd6", "Account Payable");
		public static string Cashbook => Res.GetString("580130b3-906f-4fea-a890-5e0550abd5b9", "Cashbook");
		public static string JobCosting => Res.GetString("429727fe-a6b4-492d-a8b6-1646f1362d09", "Job Costing");
		public static string GeneralLedger => Res.GetString("bf98603b-f260-49aa-a410-562c96bd8f55", "General Ledger");
	}

	public static class TransactionDescription
	{
		public static string Invoice => Res.GetString("72ef53cd-a2c6-4e58-96e0-0d9a6a595c4f", "Invoice");
		public static string CreditNote => Res.GetString("d79b322e-73b8-4f98-af82-b955885204da", "Credit Note");
		public static string Adjustment => Res.GetString("4fb65556-bcca-4b89-91be-44a08bb68ffd", "Adjustment");
		public static string Receipt => Res.GetString("4b16d41c-f003-495d-bd2a-3d94764c1e50", "Receipt");
		public static string Payment => Res.GetString("a03ba794-545c-4803-9ba8-949db256c31d", "Payment");
		public static string Discount => Res.GetString("66a44fb7-44f8-45a0-9fae-d330ab10f25b", "Discount");
		public static string ExchangeDiff => Res.GetString("e3c78def-e9a2-441e-b4c3-956f602cbb66", "Exchange Difference");
		public static string Overpayment => Res.GetString("d0b84024-86a4-432a-b4f3-f09766913c21", "Overpayment");
		public static string ARAPJournal => Res.GetString("8d13be1b-a4c1-4f17-98d4-3d7c74b10052", "AR/AP Journal");
		public static string Transfer => Res.GetString("8c77c490-0911-4a7b-a3d9-a583dac1aad4", "Transfer");
		public static string Contra => Res.GetString("f95825dd-8591-4160-8d15-8c6af7f18322", "Contra");
		public static string GeneralJournal => Res.GetString("44d5c247-472a-47c4-a258-fc3bb96bff08", "General Journal");
		public static string ReversingJournal => Res.GetString("4ff14272-ebe5-432e-8af8-a514fd4a174c", "Reversing Journal");
		public static string AutoJournal => Res.GetString("f0af1c76-05d9-445a-bb17-929c0e3d9f23", "Auto Journal");
		public static string DirectReceipt => Res.GetString("107a82a4-55b1-4816-97c8-2029533dde0b", "Direct Receipt");
		public static string DirectPayment => Res.GetString("1c1781ec-a7c9-41df-8cfb-ebdc24f196bb", "Direct Payment");
		public static string Accrual => Res.GetString("df8cad59-7443-4339-a742-abb0e4185a5a", "Accrual");
		public static string WIP => Res.GetString("d3818bb1-2646-4f74-afee-b8b0f9cbb825", "WIP");
		public static string JobRevenueJournal => Res.GetString("590811bd-f4a0-40d5-ae8e-248b4ab3b457", "Job Revenue Journal");
		public static string CFXJournal => Res.GetString("ddfed276-f0d0-4d55-95e9-4a65fac3ffc2", "CFX Journal");
	}

	public class LedgerTransactionAssociator
	{
		public OptionalFilterCriteriaList GetLedgerList()
		{
			OptionalFilterCriteriaList result = new OptionalFilterCriteriaList();

			result.Add(new LedgerFilterCriteria(LedgerDescription.AccountsReceivable, LedgerTypes.AccountsReceivable));
			result.Add(new LedgerFilterCriteria(LedgerDescription.AccountsPayable, LedgerTypes.AccountsPayable));
			result.Add(new LedgerFilterCriteria(LedgerDescription.Cashbook, LedgerTypes.CashBook));
			result.Add(new LedgerFilterCriteria(LedgerDescription.JobCosting, LedgerTypes.JobCosting));
			result.Add(new LedgerFilterCriteria(LedgerDescription.GeneralLedger, LedgerTypes.General));

			return result;
		}

		public OptionalFilterCriteriaList GetTransactionListByLedger(OptionalFilterCriteriaList ledgerCollection, bool defaultValue)
		{
			OptionalFilterCriteriaList result = new OptionalFilterCriteriaList();

			if (ledgerCollection.FindSelectedValueByDescription(LedgerDescription.AccountsPayable)
			|| (ledgerCollection.FindSelectedValueByDescription(LedgerDescription.AccountsReceivable)))
			{
				SetARAPTypes(result, defaultValue);
			}

			if (ledgerCollection.FindSelectedValueByDescription(LedgerDescription.Cashbook))
			{
				SetCashbookTypes(result, defaultValue);
			}

			if (ledgerCollection.FindSelectedValueByDescription(LedgerDescription.JobCosting))
			{
				SetJobCostingTypes(result, defaultValue);
			}

			if (ledgerCollection.FindSelectedValueByDescription(LedgerDescription.GeneralLedger))
			{
				SetGeneralLedgerTypes(result, defaultValue);
			}

			return result;
		}

		void SetGeneralLedgerTypes(OptionalFilterCriteriaList result, bool defaultValue)
		{
			result.Add(new TransactionFilterCriteria(TransactionDescription.GeneralJournal, TransactionTypes.GLStandardJournal), defaultValue);
		}

		void SetJobCostingTypes(OptionalFilterCriteriaList result, bool defaultValue)
		{
			result.Add(new OptionalFilterCriteria(TransactionDescription.Accrual, new ZQuery()), defaultValue);
			result.Add(new OptionalFilterCriteria(TransactionDescription.WIP, new ZQuery()), defaultValue);
			result.Add(new TransactionFilterCriteria(TransactionDescription.JobRevenueJournal, TransactionTypes.JobRevenueJournal), defaultValue);
			result.Add(new TransactionFilterCriteria(TransactionDescription.CFXJournal, TransactionTypes.Journal), defaultValue);
		}

		void SetCashbookTypes(OptionalFilterCriteriaList result, bool defaultValue)
		{
			result.Add(new TransactionFilterCriteria(TransactionDescription.DirectReceipt, TransactionTypes.DirectReceipt), defaultValue);
			result.Add(new TransactionFilterCriteria(TransactionDescription.DirectPayment, TransactionTypes.DirectPayment), defaultValue);
			result.Add(new TransactionFilterCriteria(TransactionDescription.ExchangeDiff, TransactionTypes.ExchangeDifference), defaultValue);
			result.Add(new TransactionFilterCriteria(TransactionDescription.Transfer, TransactionTypes.Transfer), defaultValue);
		}

		void SetARAPTypes(OptionalFilterCriteriaList result, bool defaultValue)
		{
			result.Add(new TransactionFilterCriteria(TransactionDescription.Invoice, TransactionTypes.Invoice), defaultValue);
			result.Add(new TransactionFilterCriteria(TransactionDescription.Adjustment, TransactionTypes.AdjustmentNote), defaultValue);
			result.Add(new TransactionFilterCriteria(TransactionDescription.CreditNote, TransactionTypes.CreditNote), defaultValue);
			result.Add(new TransactionFilterCriteria(TransactionDescription.Payment, TransactionTypes.Payment), defaultValue);
			result.Add(new TransactionFilterCriteria(TransactionDescription.Receipt, TransactionTypes.Receipt), defaultValue);
			result.Add(new TransactionFilterCriteria(TransactionDescription.Discount, TransactionTypes.Discount), defaultValue);
			result.Add(new TransactionFilterCriteria(TransactionDescription.ExchangeDiff, TransactionTypes.ExchangeDifference), defaultValue);
			result.Add(new TransactionFilterCriteria(TransactionDescription.Overpayment, TransactionTypes.Overpayment), defaultValue);
			result.Add(new TransactionFilterCriteria(TransactionDescription.ARAPJournal, TransactionTypes.Journal), defaultValue);
			result.Add(new TransactionFilterCriteria(TransactionDescription.Transfer, TransactionTypes.Transfer), defaultValue);

			result.Add(new OptionalFilterCriteria(TransactionDescription.Contra, ContraFilter), defaultValue);
		}

		ZQuery fContraFilter;
		ZQuery ContraFilter
		{
			get
			{
				if (fContraFilter == null)
				{
					fContraFilter = new ZQuery();
					fContraFilter.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable);
					fContraFilter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Contra);
				}
				return fContraFilter;
			}
		}

#if DEBUG
		public ZQuery ContraFilter_ForTestOnly
		{
			get { return ContraFilter; }
		}
#endif
	}
}
