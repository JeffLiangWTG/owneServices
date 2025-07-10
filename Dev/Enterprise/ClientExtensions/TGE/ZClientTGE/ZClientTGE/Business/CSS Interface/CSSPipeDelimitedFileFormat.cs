using Enterprise.DataTransfer.Business;

namespace Enterprise.Client.TGE.Business
{
	internal class CSSPipeDelimitedFileFormat : PipeDelimitedFlatFileFormat
	{
		public override FileExtensionType FileExtensionForExport
		{
			get { return FileExtensionType.ClientSpecific; }
		}

		protected override string GetClientSpecificFileExtension()
		{
			return CSSFileExtension;
		}
		internal const string CSSFileExtension = "txt";

		protected override bool IncludeQuotesWhenExporting
		{
			get { return false; }
		}
	}
}
