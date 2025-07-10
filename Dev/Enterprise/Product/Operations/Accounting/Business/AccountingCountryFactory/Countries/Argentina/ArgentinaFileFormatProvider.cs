using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	public class ArgentinaFileFormatProvider : IOrgTaxRateImportFileFormatProvider
	{
		public IOrgTaxRateImportFileFormat GetFileFormat(ZString taxAuthorityCode)
		{
			switch (taxAuthorityCode)
			{
				case "S01AR":
					return new CapitalFederalFileFormat();
				case "S02AR":
					return new BuenosAiresFileFormat();
				default:
					return new DefaultFileFormat();
			}
		}
	}
}
