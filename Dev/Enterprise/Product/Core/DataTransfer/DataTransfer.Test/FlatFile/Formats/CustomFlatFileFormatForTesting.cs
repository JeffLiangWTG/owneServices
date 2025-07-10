namespace Enterprise.DataTransfer.Business.Testing
{
	sealed class CustomFlatFileFormatForTesting : CsvFlatFileFormat
	{
		protected override string GetClientSpecificFileExtension()
		{
			return "XYZ";
		}

		protected override string GetClientSpecificFileExtensionDescription()
		{
			return "My Client's special file extension";
		}

		public override FileExtensionType FileExtensionForExport
		{
			get { return FileExtensionType.ClientSpecific; }
		}

		public override FileExtensionType FileExtensionForImport
		{
			get { return FileExtensionType.ClientSpecific; }
		}
	}
}
