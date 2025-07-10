using System;
using System.Globalization;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.CashBook
{
	/// <summary>
	/// Immutable and structurally comparable snapshot of fields in bank reconciliation line items (IBankReconMergedTransaction).
	/// </summary>
	public class BankReconTransactionSnapshot : IEquatable<BankReconTransactionSnapshot>
	{
		public ZGuid PK { get; }
		public ZDateTime TransactionDate { get; }
		public ZDateTime InvoiceDate { get; }
		public ZString Type { get; }
		public ZString Method { get; }
		public ZString ChequeRef { get; }
		public ZString BatchNo { get; }
		public ZString Payee { get; }
		public ZDecimal Debit { get; }
		public ZDecimal Credit { get; }
		public ZBool IsCleared { get; }
		public ZString LineType { get; }
		public ZDateTime ClearedDate { get; }

		public BankReconTransactionSnapshot(
			ZGuid pk,
			ZDateTime transactionDate,
			ZDateTime invoiceDate,
			ZString type,
			ZString method,
			ZString chequeRef,
			ZString batchNo,
			ZString payee,
			ZDecimal debit,
			ZDecimal credit,
			ZBool isCleared,
			ZString lineType,
			ZDateTime clearedDate)
		{
			PK = pk;
			TransactionDate = transactionDate;
			InvoiceDate = invoiceDate;
			Type = type;
			Method = method;
			ChequeRef = chequeRef;
			BatchNo = batchNo;
			Payee = payee;
			Debit = debit;
			Credit = credit;
			IsCleared = isCleared;
			LineType = lineType;
			ClearedDate = clearedDate;
		}

		public BankReconTransactionSnapshot(IBankReconMergedTransaction transaction)
			: this(
				  transaction.Identifier,
				  transaction.TransactionDate,
				  transaction.InvoiceDate,
				  transaction.Type,
				  transaction.Method,
				  transaction.ChequeRef,
				  transaction.BatchNo,
				  transaction.Payee,
				  transaction.Debit,
				  transaction.Credit,
				  transaction.IsCleared,
				  transaction.LineType,
				  transaction.ClearedDate)
		{
		}

		public override bool Equals(object obj)
			=> obj is BankReconTransactionSnapshot ts
			&& this.Equals(ts);

		public bool Equals(BankReconTransactionSnapshot other)
			=> this.PK == other.PK;

		public bool EqualsWithAmount(BankReconTransactionSnapshot other)
			=> this.Equals(other)
			&& this.Credit == other.Credit
			&& this.Debit == other.Debit;

		public override string ToString()
			=> PK.ToString()
			+ " " + (Debit != 0m ? Debit : Credit).ToString(null, CultureInfo.InvariantCulture);

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = 1454080889;
				hashCode = hashCode * -1521134295 + PK.GetHashCode();
				return hashCode;
			}
		}
	}
}
