
using CargoWise.Types;

namespace Enterprise.Client.JAS.Business.Cognos
{
	public class CognosLine : ICognosLine
	{
		public ZString AccountName
		{
			get { return fAccountName; }
			set { fAccountName = value; }
		}

		public ZString AccountCode
		{
			get { return fAccountCode; }
			set { fAccountCode = value; }
		}

		public ZString CounterCompany
		{
			get { return fCounterCompany; }
			set { fCounterCompany = value; }
		}

		public ZString Mode
		{
			get { return fMode; }
			set { fMode = value; }
		}

		public ZString Branch
		{
			get { return fBranch; }
			set { fBranch = value; }
		}

		public ZString Business
		{
			get { return fBusiness; }
			set { fBusiness = value; }
		}

		public ZDecimal Amount
		{
			get { return fAmount; }
			set { fAmount = value; }
		}

		public ZString TransactionCurrency
		{
			get { return fTransactionCurrency; }
			set { fTransactionCurrency = value; }
		}

		public ZDecimal TransactionAmount
		{
			get { return fTransactionAmount; }
			set { fTransactionAmount = value; }
		}

		public ZString Geographical
		{
			get { return fGeographical; }
			set { fGeographical = value; }
		}

		ZString fAccountName;
		ZString fAccountCode;
		ZString fCounterCompany;
		ZString fMode;
		ZString fBranch;
		ZString fBusiness;
		ZDecimal fAmount;
		ZString fTransactionCurrency;
		ZDecimal fTransactionAmount;
		ZString fGeographical;
	}
}
