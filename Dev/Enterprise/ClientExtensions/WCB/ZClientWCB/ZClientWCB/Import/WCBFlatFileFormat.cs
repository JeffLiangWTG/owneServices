using Enterprise.DataTransfer.Business;

namespace Enterprise.Client.WCB
{
	public class WCBFlatFileFormat : CsvFlatFileFormat
	{
		public override FileExtensionType FileExtensionForImport
		{
			get { return FileExtensionType.Txt; }
		}
	}
}
