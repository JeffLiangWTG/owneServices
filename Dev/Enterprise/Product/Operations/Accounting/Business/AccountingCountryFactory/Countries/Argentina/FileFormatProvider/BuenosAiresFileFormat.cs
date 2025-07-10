using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	sealed class BuenosAiresFileFormat : IOrgTaxRateImportFileFormat
	{
		public short StartDate => 2;
		public short EndDate => 3;
		public short RegistrationCode => 4;
		public short PerceptionRate => 8;
		public char Delimiter => ';';
		public short NumberOfColumnsInFormat => 11;
	}
}
