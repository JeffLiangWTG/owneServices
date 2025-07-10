namespace Enterprise.DataTransfer.Business
{
	public class CsvFlatFileFormat : DelimitedFlatFileFormat
	{
		public CsvFlatFileFormat()
		{
			IncludeQuotes = true;
		}

		public CsvFlatFileFormat(bool includeQuotes)
		{
			this.IncludeQuotes = includeQuotes;
		}

		public override FileExtensionType FileExtensionForExport
		{
			get { return FileExtensionType.Csv; }
		}

		public override FileExtensionType FileExtensionForImport
		{
			get { return FileExtensionType.Csv; }
		}

		protected override char Delimiter
		{
			get { return ','; }
		}

		protected override bool IncludeQuotesWhenExporting
		{
			get { return IncludeQuotes; }
		}

		readonly bool IncludeQuotes;
	}
}
