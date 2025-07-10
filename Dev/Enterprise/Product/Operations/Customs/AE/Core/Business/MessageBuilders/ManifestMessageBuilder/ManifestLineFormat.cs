using Enterprise.DataTransfer.Business;

namespace Enterprise.Customs.AE.Business;

public class ManifestLineFormat : CsvFlatFileFormat
{
	public ManifestLineFormat() : base() { }

	protected override bool IncludeQuotesWhenExporting
	{
		get { return true; }
	}
}
