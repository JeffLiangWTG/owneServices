using System.Collections;

namespace Enterprise.DataTransfer.Business.Testing
{
	sealed class FileExtensionFilterBuilderForTesting : FileExtensionFilterBuilder
	{
		public FileExtensionFilterBuilderForTesting() : base()
		{
		}

		public FileExtensionFilterBuilderForTesting(bool useUpperCaseFileExtension) : base(useUpperCaseFileExtension)
		{
		}

		public ArrayList PublicFileExtensionsList
		{
			get { return FileExtensions; }
		}
	}
}
