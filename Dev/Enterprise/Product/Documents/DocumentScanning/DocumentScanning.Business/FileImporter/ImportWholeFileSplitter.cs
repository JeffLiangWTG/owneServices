using System.Collections.Generic;
using System.Linq;
using Enterprise.DocumentEngine.PreviewableDocument;

namespace Enterprise.DocumentScanning.Business
{
	class ImportWholeFileSplitter : IFileSplitter
	{
		readonly string docType, jobType;
		readonly bool isUsingCoversheet;

		public ImportWholeFileSplitter(bool isUsingCoversheet, string jobType, string docType)
		{
			this.docType = docType;
			this.jobType = jobType;
			this.isUsingCoversheet = isUsingCoversheet;
		}

		public IEnumerable<FileSplitParameters> SplitFile(IPreviewableDocument document)
		{
			yield return new FileSplitParameters(
				jobType,
				string.Empty,
				docType,
				string.Empty,
				0,
				document.NumberOfPages,
				null,
				Enumerable.Empty<string>(),
				!isUsingCoversheet
			);
		}
	}
}
