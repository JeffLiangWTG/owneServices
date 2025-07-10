using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	sealed class CapitalFederalFileFormat : IOrgTaxRateImportFileFormat
	{
		public short StartDate => 1;
		public short EndDate => 2;
		public short RegistrationCode => 3;
		public short PerceptionRate => 7;
		public char Delimiter => ';';
		public short NumberOfColumnsInFormat => 12;
	}
}
