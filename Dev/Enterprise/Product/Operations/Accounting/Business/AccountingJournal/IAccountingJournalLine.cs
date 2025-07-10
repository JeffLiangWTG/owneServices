using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business
{
	public interface IAccountingJournalLine : IDebitCreditAmounts
	{
		public ZGuid AL_AC { get; }

		public AccChargeCode ChargeCode { get; }

		public ZGuid AL_AG { get; }

		public AccGLHeader GLHeader { get; }

		public ZString AL_Desc { get; }

		public ZString AL_RevRecognitionType { get; }

		public ZDecimal AL_ExchangeRate { get; }

		public ZString AL_RX_NKTransactionCurrency { get; }

		public RefCurrency Currency { get; }

		public RefCurrency LocalCurrency { get; }

		public ZString TransactionHeaderCurrency { get; }

		public ZGuid AL_GC { get; }

		public GlbCompany Company { get; }

		public ZGuid AL_GB { get; }

		public GlbBranch Branch { get; }

		public ZGuid AL_GE { get; }

		public GlbDepartment Department { get; }

		public string TaxBasis { get; }

		public string GLAccountDescription { get; }

		public ZString MultiSubAccountTypeCode { get; }

		public ZInt AL_PostPeriod { get; }

		public ZDateTime AL_PostDate { get; }

		public ZString JournalEntriesNumber { get; }

		public ZBool IsGenerateAndStoreJournalEntriesForPostedAccountingTransactionsOn { get; }
	}
}
