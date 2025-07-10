using Enterprise.DataTransfer.Business;

namespace Enterprise.Client.ELG
{
	public class SagAccountsFlatFileFormat : CsvFlatFileFormat
	{
		public override FileExtensionType FileExtensionForExport
		{
			get { return FileExtensionType.Csv;	}
		}

		protected override bool IncludeQuotesWhenExporting
		{
			get { return false; }
		}
	}
}
