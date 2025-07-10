using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DataConverters.Accounting.Cyber2
{
	public abstract class LedgerDataImporter : InterbaseImporter
	{
		public LedgerDataImporter(ProgressLogger logger, ZString dataSourcePath, ZBool importToCSVFile, ZString accountType) : base(logger, dataSourcePath, true, importToCSVFile)
		{
			this.AccountType = accountType;
		}
		protected readonly ZString AccountType;

		protected internal ZString GetEnterpriseLedgerType(ZString sourceLedgerType)
		{
			return (sourceLedgerType == "C") ? LedgerTypes.AccountsPayable : LedgerTypes.AccountsReceivable;
		}

		protected internal ZString GetLocalCurrencyCodeIfCurrencyIsEmpty(ZString currency)
		{
			return currency.IsEmpty ? GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency : currency;
		}
	}
}
