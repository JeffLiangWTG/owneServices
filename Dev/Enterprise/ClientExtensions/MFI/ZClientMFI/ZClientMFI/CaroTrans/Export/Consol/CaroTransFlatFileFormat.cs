using Enterprise.DataTransfer.Business;

namespace Enterprise.Client.MFI.CaroTrans
{
	public class CaroTransFlatFileFormat : CsvFlatFileFormat
	{
		public override FileExtensionType FileExtensionForExport
		{
			get { return FileExtensionType.ClientSpecific; }
		}

		protected override string GetClientSpecificFileExtension()
		{
			return Constants.CargoTransDefaultFileExtension;
		}
	}
}
