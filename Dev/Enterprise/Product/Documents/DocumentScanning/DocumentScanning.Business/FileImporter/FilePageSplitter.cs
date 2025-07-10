using System.Collections.Generic;
using System.Linq;
using Enterprise.DocumentEngine.PreviewableDocument;

namespace Enterprise.DocumentScanning.Business
{
#if DEBUG
	public
#endif
	class FilePageSplitter : IFileSplitter
	{
		readonly string refType, docType;
		readonly bool isUsingCoversheet;

		public FilePageSplitter(bool isUsingCoversheet, string refType, string docType)
		{
			this.isUsingCoversheet = isUsingCoversheet;
			this.refType = refType;
			this.docType = docType;
		}

		public IEnumerable<FileSplitParameters> SplitFile(IPreviewableDocument document)
		{
			for (var i = 0; i < document.NumberOfPages; i++)
			{
				yield return new FileSplitParameters(refType, string.Empty, docType, string.Empty, i, i + 1, null, Enumerable.Empty<string>(), !isUsingCoversheet);
			}
		}
	}
}
