using System;
using System.IO;

namespace Enterprise.DocumentEngine.Service
{
	public interface IDocumentPreviewService
	{
		DocumentCommand GetDocumentCommand(Guid documentCommandPk, string tablePrefix, Guid businessObjectPk);
		void WriteDocumentPreview(DocumentCommand documentCommand, Stream outputStream);
	}
}
