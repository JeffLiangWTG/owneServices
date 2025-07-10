using CargoWise.Types;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	public class PortugalReportSAFTWriterProvider : IReportSAFTWriter
	{
		SAFTVersion IReportSAFTWriter.GetSAFTVersion => SAFTVersion.SAFT1_04;

		ZString IReportSAFTWriter.GetLocalLanguage => ZString.Empty;
	}
}
