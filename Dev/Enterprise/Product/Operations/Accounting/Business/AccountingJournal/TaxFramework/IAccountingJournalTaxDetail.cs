using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business
{
	public interface IAccountingJournalTaxDetail
	{
		ZString TaxConfiguration { get; }
		ZString GLAccount { get; }
		ZString GLAccountDesc { get; }
		ZString BranchCode { get; }
		ZString DepartmentCode { get; }
		ZDecimal Amount { get; }
		ZDate PostDate { get; }
		ZString PostPeriod { get; }
		ZString Basis { get; }
		ZDecimal OSAmount { get; }
		RefCurrency Currency { get; }
		RefCurrency LocalCurrency { get; }
		ZString AlternateAccountNum { get; }
		ZString AlternateAccountDesc { get; }
		ZBool IsMissingPeriodForReportingBook { get; }
	}
}
