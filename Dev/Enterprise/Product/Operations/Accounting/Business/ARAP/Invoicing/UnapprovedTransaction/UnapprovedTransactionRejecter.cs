using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class UnapprovedTransactionRejecter
	{
		public void Reject(InvoicingBase transaction)
		{
			if (transaction.AH_Ledger != LedgerTypes.UnapprovedPayableTransactions)
			{
				throw new ArgumentException("This helper class can only be used with Unapproved transactions.");
			}

			transaction.AH_IsCancelled = true;

			var transformer = new ReverseInvoicingTransformer(transaction.Factory);
			transformer.Transform(transaction);

			transaction.Logs.AddNew(Events.Cancelled);
			AppendRejectedToTransactionNum(transaction);
			transaction.Lines.RemoveAndDeleteAll();
		}

		void AppendRejectedToTransactionNum(InvoicingBase transaction)
		{
			string transactionNum = transaction.AH_TransactionNum;

			if (transactionNum.Length > AccTransactionHeaderSchema.AH_TransactionNum.MaxLength - 2)
			{
				transactionNum = transactionNum.Substring(0, AccTransactionHeaderSchema.AH_TransactionNum.MaxLength - 2);
			}
			transactionNum += "RJ";
			transaction.AH_TransactionNum = transactionNum;

			string sql = @"SELECT
								max(AH_TransactionCount) as MaxCount 
							FROM
								dbo.AccTransactionHeader 
							WHERE
								AH_GC = @CompanyPK and
								AH_Ledger = @LedgerType and
								AH_TransactionType = @TransactionType and
								AH_TransactionNum = @TransactionNum";

			ZSqlParameterCollection parameters = new ZSqlParameterCollection();
			parameters.Add("@TransactionNum", transactionNum, AccTransactionHeaderSchema.AH_TransactionNum);
			parameters.Add("@CompanyPK", transaction.AH_GC, AccTransactionHeaderSchema.AH_GC);
			parameters.Add("@LedgerType", transaction.AH_Ledger, AccTransactionHeaderSchema.AH_Ledger);
			parameters.Add("@TransactionType", transaction.AH_TransactionType, AccTransactionHeaderSchema.AH_TransactionCategory);

			DynamicBusinessObjectCollection dynamicCollection = new DynamicBusinessObjectCollection(transaction.Factory);
			dynamicCollection.Load(sql, parameters);
			ZByte maxCount = dynamicCollection[0]["MaxCount"] == DBNull.Value ? (ZByte)0 : (ZByte)dynamicCollection[0]["MaxCount"];
			transaction.AH_TransactionCount = (ZByte)(maxCount + 1);
		}
	}
}
