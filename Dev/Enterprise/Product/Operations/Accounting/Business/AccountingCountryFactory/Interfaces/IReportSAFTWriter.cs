using CargoWise.Types;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	public interface IReportSAFTWriter
{
		SAFTVersion GetSAFTVersion { get; }

		ZString GetLocalLanguage { get; }
	}
}
