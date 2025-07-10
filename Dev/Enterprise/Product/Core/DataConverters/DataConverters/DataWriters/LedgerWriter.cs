using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

#if DEBUG
#endif

using Enterprise.ZArchitecture.Core;

namespace Enterprise.DataConverters
{
	public class LedgerWriter : DataWriter
	{
		public LedgerWriter(BusinessObjectFactory factory) : base(factory)
		{
		}

		#region Public fields to be filled in when importing

		public ZString Account;
		public ZString Reference;
		public ZString Currency;
		public ZString Branch;
		public ZString Department;
		public ZString Ledger;
		public ZDateTime DueDate;
		public ZDateTime Date;
		public ZDecimal LocalBalance;
		public ZDecimal ForeignBalance;

		#endregion

		protected override internal ZString GetAnyReasonRecordShouldBeExcluded()
		{
			return (LocalBalance == ZDecimal.Zero) ? new ZString("Zero amounts") : ZString.Empty;
		}

		protected override BusinessObject GetExistingBusinessObject()
		{
			ZQuery filter = new ZQuery(AccTransactionHeaderSchema.AH_Ledger, Ledger);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Journal);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_RX_NKTransactionCurrency, CurrencyMatcher.MatchedCode);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_Desc, Reference);
			AccTransactionHeader journal = Factory.LoadTop1<AccTransactionHeader>(filter);
			return journal;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New(JournalType);
		}

		Type JournalType
		{
			get { return (Ledger == LedgerTypes.AccountsReceivable) ? typeof(ARJournal) : typeof(APJournal); }
		}

		public override ZString RecordDescription
		{
			get {	return new ZString("Transaction: " + Account + "/" + Reference); }
		}

		protected override internal ZString CSVOutputLine
		{
			get
			{
				return new ZString(Account + ","
						+ Reference + ","
						+ Date.ToString("yyyyMMdd") + ","
						+ DueDate.ToString("yyyyMMdd") + ","
						+ Currency + ","
						+ ForeignBalance + ","
						+ LocalBalance + ","
						+ Branch + ","
						+ Department);
			}
		}

		protected override void UpdateEnterpriseValues(BusinessObject businessObjectToUpdate)
		{
			ErrorReporter.ReportOnce("UpdateValuesInLedgerDataWriter", "UpdateValues in LedgerDataWriter Should never be called. Ledger Items should never be updated.");
		}

		protected CurrencyMatcher CurrencyMatcher
		{
			get
			{
				if (fCurrencyMatcher == null)
				{
					fCurrencyMatcher = new CurrencyMatcher(Currency, Factory);
				}
				return fCurrencyMatcher;
			}
		}
		CurrencyMatcher fCurrencyMatcher;
	}
}
