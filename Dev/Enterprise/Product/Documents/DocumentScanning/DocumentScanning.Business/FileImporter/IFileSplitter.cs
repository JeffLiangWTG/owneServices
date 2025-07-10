using System.Collections.Generic;
using Enterprise.DocumentEngine.PreviewableDocument;

namespace Enterprise.DocumentScanning.Business
{
	public interface IFileSplitter
	{
		IEnumerable<FileSplitParameters> SplitFile(IPreviewableDocument document);
	}
}
