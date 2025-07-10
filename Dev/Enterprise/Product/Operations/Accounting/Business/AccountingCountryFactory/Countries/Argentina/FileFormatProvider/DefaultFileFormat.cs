using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	sealed class DefaultFileFormat : IOrgTaxRateImportFileFormat
	{
		public short StartDate => 1;
		public short EndDate => 2;
		public short RegistrationCode => 0;
		public short PerceptionRate => 3;
		public char Delimiter => ';';
		public short NumberOfColumnsInFormat => 4;
	}
}
