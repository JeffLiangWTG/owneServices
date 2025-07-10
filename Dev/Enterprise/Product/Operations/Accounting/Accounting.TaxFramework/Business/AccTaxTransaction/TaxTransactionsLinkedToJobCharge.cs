using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Accounting.TaxFramework.Business
{
	public class TaxTransactionsLinkedToJobCharge : NonPersistentBusinessObject
	{
		public TaxTransactionsLinkedToJobCharge()
		{
		}
		public TaxTransactionsLinkedToJobCharge(AccTaxTransaction accTaxTransaction, AccTaxRecordTransactionLinePivot accTaxRecordTransactionLinePivot)
		{
			this.AccTaxTransaction = accTaxTransaction;
			this.AccTaxRecordTransactionLinePivot = accTaxRecordTransactionLinePivot;
		}

		AccTaxTransaction AccTaxTransaction { get; }
		AccTaxRecordTransactionLinePivot AccTaxRecordTransactionLinePivot { get; }

		public ZString ATT_AT_TaxID
		{
			get
			{
				return AccTaxTransaction.TaxID != null ? AccTaxTransaction.TaxID.AT_Code : ZString.Empty;
			}
		}
		public ZString ATT_A9_TaxMessage
		{
			get
			{
				return AccTaxTransaction.TaxMessage != null ? AccTaxTransaction.TaxMessage.A9_Code : ZString.Empty;
			}
		}
		public ZDecimal ATT_Rate
		{
			get
			{
				return AccTaxTransaction.ATT_Rate;
			}
		}
		public ZString ATT_RX_NKOSTaxCurrency
		{
			get
			{
				return AccTaxTransaction.ATT_RX_NKOSTaxCurrency;
			}
		}
		public ZDecimal ATT_OSTaxBaseAmount
		{
			get
			{
				return AccTaxTransaction.ATT_OSTaxBaseAmount;
			}
		}
		public ZDecimal ATT_OSTaxAmount
		{
			get
			{
				return AccTaxTransaction.ATT_OSTaxAmount;
			}
		}
		public ZString ATT_TaxAuthorityServiceCode
		{
			get
			{
				return AccTaxTransaction.ATT_TaxAuthorityServiceCode;
			}
		}
		public ZString ATT_TaxSystemCode
		{
			get
			{
				return AccTaxTransaction.ATT_TaxSystemCode;
			}
		}
		public ZDecimal ATT_LocalTaxBaseAmount
		{
			get
			{
				return AccTaxTransaction.ATT_LocalTaxBaseAmount;
			}
		}
		public ZDecimal ATT_LocalTaxAmount
		{
			get
			{
				return AccTaxTransaction.ATT_LocalTaxAmount;
			}
		}
		public ZString ATT_Basis
		{
			get
			{
				return AccTaxTransaction.ATT_Basis;
			}
		}
		public ZBool ATT_AffectsSourceTransactionTotal
		{
			get
			{
				return AccTaxTransaction.ATT_AffectsSourceTransactionTotal;
			}
		}
		public ZDate ATT_TaxDate
		{
			get
			{
				return AccTaxTransaction.ATT_TaxDate;
			}
		}
		public ZDate ATT_PostDate
		{
			get
			{
				return AccTaxTransaction.ATT_PostDate;
			}
		}
		public ZString ATT_Ledger
		{
			get
			{
				return AccTaxTransaction.ATT_Ledger;
			}
		}
		public ZString ATT_TaxSuperType
		{
			get
			{
				return AccTaxTransaction.ATT_TaxSuperType;
			}
		}
		public ZDate ATT_RealisationDate
		{
			get
			{
				return AccTaxTransaction.ATT_RealisationDate;
			}
		}
		public ZString ATT_AH_MatchTransaction_ForBinding
		{
			get
			{
				return AccTaxTransaction.ATT_AH_MatchTransaction_ForBinding;
			}
		}
		public ZString TaxAuthorityCode
		{
			get
			{
				return AccTaxTransaction.TaxAuthorityCode;
			}
		}
		public ZString ATT_TaxAuthorityServiceCodeDescription
		{
			get
			{
				return AccTaxTransaction.ATT_TaxAuthorityServiceCodeDescription;
			}
		}
		public ZDecimal ATP_LocalTaxAmount
		{
			get
			{
				return AccTaxRecordTransactionLinePivot.ATP_LocalTaxAmount;
			}
		}
	}
}
