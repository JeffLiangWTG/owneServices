using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Netting;

namespace Enterprise.DocumentWrappers
{
	public class DocNettingTransaction : DocBaseWrapper
	{
		protected DocNettingTransaction(NettingTransaction nettingTransaction, BusinessObjectFactory factoryToWrap)
			: base(nettingTransaction, factoryToWrap)
		{
			Argument.NotNull(nettingTransaction, "NettingTransaction");
		}

		public static DocNettingTransaction New(NettingTransaction nettingCalculation, BusinessObjectFactory factoryToWrap)
		{
			return new DocNettingTransaction(nettingCalculation, factoryToWrap);
		}

		public NettingTransaction Transaction
		{
			get { return (NettingTransaction)WrappedObject; }
		}

		public ZString Currency
		{
			get { return Transaction.Currency; }
		}

		public ZString NettingCurrency
		{
			get { return Transaction.NettingCurrency; }
		}

		public ZString OrgCode
		{
			get { return Transaction.OrgCode; }
		}

		public ZDecimal Amount
		{
			get { return Transaction.Amount; }
		}

		public ZDecimal NettingSystemAmount
		{
			get { return Transaction.NettingSystemAmount; }
		}

		public ZString ParticipatingOrgCode
		{
			get { return Transaction.ParticipatingOrgCode; }
		}

		public ZString ParticipatingCompanyCode
		{
			get { return Transaction.ParticipantCompanyCode; }
		}

		public ZString ParticipatingCompanyname
		{
			get { return Transaction.ParticipantCompanyName; }
		}
		public ZString TransactionReference
		{
			get { return Transaction.MatchingTransactionReference; }
		}

		public ZString ParticipantCurrency
		{
			get { return Transaction.ParticipantCurrency; }
		}

		public ZDecimal ParticipantAmount
		{
			get { return Transaction.ParticipantAmount; }
		}

		public ZInt One
		{
			get { return 1; } //This property is only used to show the number of transactions grouped
		}

		public DocNettingTransactionReference Reference
		{
			get { return DocNettingTransactionReference.New(((ISupportTransactionReference)Transaction).TransactionRef, Factory); }
		}

		public DocNettingTransactionLineReference LineReference
		{
			get
			{
				return DocNettingTransactionLineReference.New(((ISupportTransactionReference)Transaction).TransactionLineRef, Factory);
			}
		}
	}
}
