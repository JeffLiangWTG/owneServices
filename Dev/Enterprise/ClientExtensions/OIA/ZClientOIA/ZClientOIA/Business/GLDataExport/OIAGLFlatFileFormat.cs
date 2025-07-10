using Enterprise.DataTransfer.Business;

namespace Enterprise.Client.OIA.Business
{
	internal class OIAGLFlatFileFormat : CsvFlatFileFormat
	{
		public override FileExtensionType FileExtensionForExport
		{
			get { return FileExtensionType.Csv; }
		}

		protected override bool IncludeQuotesWhenExporting
		{
			get { return true; }
		}
	}
}
